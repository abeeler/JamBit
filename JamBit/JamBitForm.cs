using SQLite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace JamBit
{
    public partial class JamBitForm : Form
    {
        #region Fields

        public SQLiteConnection db;
        public int dbOperationsActive = 0;

        private Timer timeCheckTimer;
        private Timer playlistScrollTimer;
        private OpenFileDialog openFileDialog;
        private BackgroundWorker libraryScanner;
        private BackgroundWorker playlistDeleter;
        private ContextMenuStrip playableOptions;
        private ContextMenuStrip currentPlaylistOptions;
        private ContextMenuStrip playlistOptions;
        private ContextMenuStrip playlistsOptions;
        private ToolStripMenuItem selectedPlayMode;
        private LibraryNode playlistRightClicked = null;

        private ConcurrentQueue<int> playlistsToDelete;
        private ConcurrentQueue<string> foldersToScan;

        private RepeatMode playMode;
        private Random shuffleRandom = new Random();
        private Playlist currentPlaylist = new Playlist();
        private int playlistIndex = -1;
        private bool preventExpand = false;
        private int playlistScrollDirection = 1;
        private int playlistItemsViewable;
        private DateTime lastMouseDown;
        private List<int> shuffledSongs = new List<int>();
        private OptionsForm optionsForm = null;

        #endregion

        #region Execution State Fields and Methods

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }

        void RestoreExecutionState()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }

        void KeepSystemAwake()
        {
            SetThreadExecutionState(
                EXECUTION_STATE.ES_SYSTEM_REQUIRED | 
                EXECUTION_STATE.ES_DISPLAY_REQUIRED |
                EXECUTION_STATE.ES_AWAYMODE_REQUIRED |
                EXECUTION_STATE.ES_CONTINUOUS);
        }

        #endregion

        public enum RepeatMode { Loop, Repeat, Shuffle, None }

        public JamBitForm()
        {
            InitializeComponent();

            // Establish DB connection
            db = new SQLiteConnection(Path.Combine(Application.UserAppDataPath, "jambit.db"));

            // Resets the application settings before anything occurs
            //ResetApplication();

            // Create tables if they do not already exist
            db.CreateTable<Song>();
            db.CreateTable<Playlist>();
            db.CreateTable<PlaylistItem>();
            db.CreateTable<RecentSong>();

            if (db.Find<Playlist>(1) == null)
                Console.WriteLine(db.Insert(new Playlist()));

            // Create concurrent queue for folder scanning
            foldersToScan = new ConcurrentQueue<string>();

            // Create concurrent queue for playlist deletion
            playlistsToDelete = new ConcurrentQueue<int>();

            // Initialize music player
            MusicPlayer.parentForm = this;
            MusicPlayer.SetVolume(Properties.Settings.Default.PreferredVolume);
            prgVolume.Value = Properties.Settings.Default.PreferredVolume;

            // Initialize the dialog for opening new files
            openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "MP3|*.mp3|" +
                "Music Files|*.mp3";
            openFileDialog.FileOk += openFileDialog_OnFileOk;

            // Initialize the library options context menu
            playableOptions = new ContextMenuStrip();
            playableOptions.Items.Add("Add selection to current playlist");
            playableOptions.ItemClicked += libraryOptions_ItemClick;

            // Initialize the current playlist options context menu
            currentPlaylistOptions = new ContextMenuStrip();
            currentPlaylistOptions.Items.Add("Remove selection from current playlist");
            currentPlaylistOptions.ItemClicked += currentPlaylistOptions_ItemClick;

            // Initialize the playlist options context menu
            playlistOptions = new ContextMenuStrip();
            playlistOptions.Items.Add("Load playlist");
            playlistOptions.Items.Add("Rename playlist");
            playlistOptions.Items.Add("Delete playlist");
            playlistOptions.ItemClicked += playlistOptions_ItemClick;

            // Initialize the playlist parent node options context menu
            playlistsOptions = new ContextMenuStrip();
            playlistsOptions.Items.Add("Create playlist");
            playlistsOptions.ItemClicked += playlistsOptions_ItemClick;

            // Initialize the background worker for scanning in files from library folders
            libraryScanner = new BackgroundWorker();
            libraryScanner.WorkerReportsProgress = true;
            libraryScanner.WorkerSupportsCancellation = true;
            libraryScanner.DoWork += libraryScanner_DoWork;
            libraryScanner.ProgressChanged += libraryScanner_ProgressChanged;
            libraryScanner.RunWorkerCompleted += libraryScanner_RunWorkerCompleted;

            // Initialize the background worker for deleting playlists from the database
            playlistDeleter = new BackgroundWorker();
            playlistDeleter.DoWork += playlistDeleter_DoWork;

            // Load library into tree view
            PopulateLibraryTree();

            // Register library tree view events
            treeLibrary.NodeMouseDoubleClick += treeLibrary_NodeMouseDoubleClick;
            treeLibrary.MouseClick += treeLibrary_MouseClick;
            treeLibrary.MouseDown += treeLibrary_MouseDown;
            treeLibrary.BeforeExpand += treeLibrary_BeforeExpand;
            treeLibrary.BeforeCollapse += treeLibrary_BeforeCollapse;

            // Load last playlist opened
            LoadPlaylist(Properties.Settings.Default.LastPlaylistIndex);            

            // Load last play mode used
            SetPlayMode((RepeatMode)Properties.Settings.Default.LastPlayMode);

            // Initialize timer to update visual representations of song position
            timeCheckTimer = new Timer();
            timeCheckTimer.Interval = 1000;
            timeCheckTimer.Tick += new System.EventHandler(checkTime_Tick);

            // Initialize timer to scroll the playlist while dragging
            playlistScrollTimer = new Timer();
            playlistScrollTimer.Interval = 150;
            playlistScrollTimer.Tick += playlistScrollTimer_Tick;

            // Initialize value for number of items visible in the ListView
            CountPlaylistItemsViewable();

        }

        #region Form Methods

        /// <summary>
        /// Refresh the player's labels and song progression bar for a newly loaded song
        /// </summary>
        private void RefreshPlayer()
        {
            // Reset progress bar value to 0
            prgSongTime.SetValue(0);

            // Reset current time label to 0
            lblCurrentTime.Text = "0:00";

            // Update marquee label information
            if (MusicPlayer.curSong != null)
            {
                lblSongInformation.CycleText = new string[] {
                    "Title: " + MusicPlayer.curSong.Data.Tag.Title,
                    "Artist: " + MusicPlayer.curSong.Data.Tag.FirstPerformer,
                    "Album: " + MusicPlayer.curSong.Data.Tag.Album
                };

                // Update total song length label, hiding hours if the song is shorter than that
                String format = MusicPlayer.curSong.Data.Properties.Duration.Hours > 0 ? @"h':'mm':'ss" : @"mm':'ss";
                lblSongLength.Text = (MusicPlayer.curSong.Data.Properties.Duration - new TimeSpan(0, 0, 1)).ToString(format);
            }
            else
            {
                lblSongInformation.CycleText = null;
                lblSongLength.Text = "0:00";
            }
        }

        /// <summary>
        /// Pause the timer that updates the current song position
        /// </summary>
        public void PauseTimeCheck() { timeCheckTimer.Stop(); }

        /// <summary>
        /// Start the timer that updates the current song position
        /// </summary>
        public void StartTimeCheck() { timeCheckTimer.Start(); }

        /// <summary>
        /// Called when a song playing the music player reaches the end. 
        /// Determines which song plays next based on the current RepeatMode
        /// </summary>
        public void ChangeSong(bool previous = false)
        {
            if (shuffledSongs.Count >= currentPlaylist.Count)
                shuffledSongs.Clear();

            switch (playMode)
            {
                // Loop the current playlist
                // Will play the next song or the first in the playlist if at the end
                case RepeatMode.Loop:
                    if (previous && --playlistIndex == -1) playlistIndex = currentPlaylist.Count - 1;
                    else if (++playlistIndex == currentPlaylist.Count) playlistIndex = 0;
                    break;

                // Randomly select a song from the current playlist
                case RepeatMode.Shuffle:
                    if (previous && shuffledSongs.Count > 1)
                    {
                        playlistIndex = currentPlaylist.Songs.IndexOf(shuffledSongs[shuffledSongs.Count - 2]);
                        shuffledSongs.RemoveRange(shuffledSongs.Count - 2, 2);
                    }
                    else
                    {
                        List<int> songsToPlay = currentPlaylist.Songs.Where(id => !shuffledSongs.Contains(id)).ToList();
                        playlistIndex = currentPlaylist.Songs.IndexOf(songsToPlay[shuffleRandom.Next(songsToPlay.Count)]);
                    }
                    break;

                // Play through to the end of the playlist and stop
                // After the last song, will load the first and pause the player
                case RepeatMode.None:
                    if (playlistIndex >= currentPlaylist.Count - 1)
                        MusicPlayer.PauseSong();
                    playlistIndex = 0;
                    break;
            }
            OpenSong();
        }

        /// <summary>
        /// Add a folder(s) to scan for files to add to the library. 
        /// If the library scanner is not active, starts it
        /// </summary>
        /// <param name="folderPaths"></param>
        public void LibraryScan(params string[] folderPaths)
        {
            // Enqueue each folder passed a parameter
            foreach (string folder in folderPaths)
                foldersToScan.Enqueue(folder);

            // Start the scanner if it isn't already running
            if (!libraryScanner.IsBusy)
            {
                this.Cursor = Cursors.WaitCursor;
                libraryScanner.RunWorkerAsync();
            }                
        }

        /// <summary>
        /// Adds multiple songs to the playlist and saves it
        /// </summary>
        /// <param name="ids"></param>
        public void AddSongsToPlaylist(IEnumerable<int> ids, bool save = true)
        {
            lstPlaylist.BeginUpdate();
            foreach (int id in ids)
                AddSongToPlaylist(db.Get<Song>(id));
            if (save)
                SavePlaylist();
            lstPlaylist.EndUpdate();
        }

        /// <summary>
        /// Adds a song to the playlist and saves it
        /// </summary>
        /// <param name="id"></param>
        public void AddSongToPlaylist(int id, bool save = true)
        {
            AddSongToPlaylist(db.Get<Song>(id));
            if (save)
                SavePlaylist();
        }

        /// <summary>
        /// Adds a song to the current playlist
        /// </summary>
        /// <param name="s"></param>
        public void AddSongToPlaylist(Song s)
        {
            if (currentPlaylist.Songs.Contains(s.ID)) return;

            currentPlaylist.Songs.Add(s.ID);
            lstPlaylist.Items.Add(new ListViewItem(new string[] {
                    s.Title, s.Artist, s.Album, s.PlayCount.ToString()
                }));
        }

        /// <summary>
        /// Open the current song in the playlist in the music player
        /// </summary>
        private void OpenSong() { OpenSong(db.Get<Song>(currentPlaylist.Songs[playlistIndex])); }

        /// <summary>
        /// Open the given song in the music player
        /// </summary>
        /// <param name="s">The song to open</param>
        private void OpenSong(Song s)
        {
            // If the song has no attached filename, change the song
            if (s.FileName.Length == 0)
            {
                RemoveSongFromPlaylist(playlistIndex);
                ChangeSong();
                return;
            }

            // If the song's file does not exist, remove that filename from the database and change the song
            if (!File.Exists(s.FileName))
            {
                s.FileName = "";
                db.Update(s);
                RemoveSongFromPlaylist(playlistIndex);
                ChangeSong();
                return;
            }

            // Open the song and update the display
            MusicPlayer.OpenSong(s);
            RefreshPlayer();
            s.PlayCount++;
            db.Update(s);

            // Updates the play count for the song
            lstPlaylist.Items[playlistIndex].SubItems[3] =
                new ListViewItem.ListViewSubItem(lstPlaylist.Items[playlistIndex], s.PlayCount.ToString());

            // If the song hasn't been played yet, add it to the shuffled songs
            if (!shuffledSongs.Contains(s.ID))
                shuffledSongs.Add(s.ID);

            //
            // Update the recently played song list
            //

            // If song isn't in list already
            if (db.Find<RecentSong>(rs => rs.SongID == s.ID) == null)
            {
                // If more songs can be added to the list, just add them
                if (db.Table<RecentSong>().Count() < Properties.Settings.Default.RecentlyPlayedMax)
                {
                    db.Insert(new RecentSong(s.ID));
                    treeLibrary.Nodes[1].Nodes.Add(new LibraryNode(s));
                }

                // Otherwise, replace the oldest song in the list with the new one
                else
                {
                    treeLibrary.Nodes[1].Nodes.RemoveAt(Properties.Settings.Default.RecentlyPlayedIndex);
                    treeLibrary.Nodes[1].Nodes.Insert(Properties.Settings.Default.RecentlyPlayedIndex, new LibraryNode(s));

                    RecentSong toUpdate = db.Get<RecentSong>(Properties.Settings.Default.RecentlyPlayedIndex++);
                    toUpdate.SongID = s.ID;
                    db.Update(toUpdate);

                    if (Properties.Settings.Default.RecentlyPlayedIndex >= Properties.Settings.Default.RecentlyPlayedMax)
                        Properties.Settings.Default.RecentlyPlayedIndex = 1;
                    Properties.Settings.Default.Save();
                }
            }
        }

        /// <summary>
        /// Resets all saved settings and clears all songs from the library. 
        /// Should only be called before any database is opened or before closing
        /// </summary>
        public void ResetApplication()
        {
            db.DropTable<Song>();
            db.DropTable<Playlist>();
            db.DropTable<PlaylistItem>();
            db.DropTable<RecentSong>();

            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Shows the dialog for opening music files
        /// </summary>
        public void ShowOpenFileDialog()
        {
            openFileDialog.ShowDialog();
        }

        /// <summary>
        /// Shows the options form
        /// </summary>
        public void OpenPreferencesForm()
        {
            if (optionsForm == null)
            {
                optionsForm = new OptionsForm(this);
                optionsForm.Show();
            } else {
                optionsForm.WindowState = FormWindowState.Minimized;
                optionsForm.Show();
                optionsForm.WindowState = FormWindowState.Normal;
            }
        }

        public void PreferencesFormClosed()
        {
            optionsForm = null;
        }

        /// <summary>
        /// Performs necessary operations and then closes the form
        /// </summary>
        public void CloseForm()
        {
            Close();
        }

        /// <summary>
        /// Play the current song. If no song was playing before, load the first
        /// </summary>
        public void PlaySong()
        {
            if (playlistIndex == -1 && currentPlaylist.Count > 0)
                ChangeSong();
            MusicPlayer.PlaySong();
            KeepSystemAwake();
        }

        /// <summary>
        /// Pause the current song
        /// </summary>
        public void PauseSong()
        {
            MusicPlayer.PauseSong();
            RestoreExecutionState();
        }

        /// <summary>
        /// Set the current play mode and update the settings
        /// </summary>
        /// <param name="setTo">The play mode to set the player to</param>
        public void SetPlayMode(RepeatMode setTo)
        {
            playMode = setTo;

            Properties.Settings.Default.LastPlayMode = (int)playMode;
            Properties.Settings.Default.Save();

            btnPlayMode.Text = Enum.GetName(playMode.GetType(), playMode);

            if (selectedPlayMode != null)
                selectedPlayMode.Checked = false;

            switch (setTo)
            {
                case RepeatMode.Loop:
                    selectedPlayMode = mnuPlayModeLoop;
                    break;
                case RepeatMode.Repeat:
                    selectedPlayMode = mnuPlayModeRepeat;
                    break;
                case RepeatMode.Shuffle:
                    selectedPlayMode = mnuPlayModeShuffle;
                    break;
                case RepeatMode.None:
                    selectedPlayMode = mnuPlayModeNone;
                    break;
            }
            selectedPlayMode.Checked = true;            
        }

        /// <summary>
        /// Create a new playlist, prompting the user for a name
        /// </summary>
        public void CreatePlaylist()
        {
            Playlist newPlaylist = new Playlist();
            string name = null;
            if (!MusicPlayerControlsLibrary.Prompt.ShowDialog("Enter new playlist name: ", "Create a Playlist", out name))
                return;
            newPlaylist.Name = name;
            CreatePlaylist(newPlaylist);

        }

        /// <summary>
        /// Save the provided playlist to the database
        /// </summary>
        /// <param name="newPlaylist">The playlist to save</param>
        public void CreatePlaylist(Playlist newPlaylist)
        {
            db.Insert(newPlaylist);
            treeLibrary.Nodes[2].Nodes.Add(new LibraryNode(LibraryNode.LibraryNodeType.Playlist, newPlaylist.Name, newPlaylist.ID));
        }

        /// <summary>
        /// Save the current playlist and its tracks to the database
        /// </summary>
        public void SavePlaylist()
        {
            db.Update(currentPlaylist);
            currentPlaylist.SaveToDatabase(this);
        }

        /// <summary>
        /// Save the current playlist as a new playlist with a new name
        /// </summary>
        public void SaveAsNewPlaylist()
        {
            string name = null;
            if (!MusicPlayerControlsLibrary.Prompt.ShowDialog("Enter new playlist name: ", "Create a Playlist", out name))
                return;
            currentPlaylist.ID = 0;
            currentPlaylist.Name = name;
            CreatePlaylist(currentPlaylist);
            currentPlaylist.SaveToDatabase(this);
            lblPlaylistName.Text = currentPlaylist.Name;
            Properties.Settings.Default.LastPlaylistIndex = currentPlaylist.ID;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Rename a playlist
        /// </summary>
        /// <param name="playlistID"></param>
        public void RenamePlaylist(int playlistID)
        {
            string newName = null;
            if (!MusicPlayerControlsLibrary.Prompt.ShowDialog("Enter new name for playlist", "Rename Playlist", out newName))
                return;

            Playlist toRename = playlistID == currentPlaylist.ID ? currentPlaylist : db.Get<Playlist>(playlistID);
            toRename.Name = newName;
            db.Update(toRename);
            treeLibrary.Nodes[2].Nodes.Cast<LibraryNode>()
                .First(ln => (int)ln.DatabaseKey == playlistID).Text = newName;

            if (playlistID == currentPlaylist.ID)
                lblPlaylistName.Text = newName;
        }
        public void RenamePlaylist() { RenamePlaylist(currentPlaylist.ID); }

        /// <summary>
        /// Clear the current playlist
        /// </summary>
        public void ClearPlaylist()
        {
            MusicPlayer.CloseSong();
            shuffledSongs.Clear();
            lblPlaylistName.Text = "";
            lstPlaylist.Items.Clear();
        }

        /// <summary>
        /// Recursively add all songs in a playable node to the current playlist
        /// </summary>
        /// <param name="playable">The node to add</param>
        public void AddPlayableNodeSongs(LibraryNode playable)
        {
            if (playable.Nodes.Count == 0)
                AddSongToPlaylist((int)playable.DatabaseKey, false);
            else
                foreach (LibraryNode child in playable.Nodes.Cast<LibraryNode>())
                    AddPlayableNodeSongs(child);
        }

        public void LoadDefaultPlaylist()
        {
            ClearPlaylist();

            currentPlaylist = db.Get<Playlist>(1);
            currentPlaylist.Songs.Clear();
            SavePlaylist();

            playlistIndex = -1;
            Properties.Settings.Default.LastPlaylistIndex = 1;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Load a playlist using the given ID
        /// </summary>
        /// <param name="id">The ID of the playlist to load</param>
        public void LoadPlaylist(int id)
        {
            ClearPlaylist();
            try
            {
                currentPlaylist = db.Get<Playlist>(id);
                shuffledSongs.Clear();
                lblPlaylistName.Text = currentPlaylist.Name;
                Properties.Settings.Default.LastPlaylistIndex = currentPlaylist.ID;
                Properties.Settings.Default.Save();
                
                AddSongsToPlaylist(db.Table<PlaylistItem>()
                    .Where<PlaylistItem>(pi => pi.PlaylistID == currentPlaylist.ID)
                    .Select(pi => pi.SongID),
                    false
                );
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Delete a playlist from the library by ID
        /// </summary>
        /// <param name="id">The ID of the playlist to delete</param>
        public void DeletePlaylist(int id)
        {
            // Add the ID to the list of playlists to delete
            playlistsToDelete.Enqueue(id);

            // If the background worker is free, start it up
            if (!playlistDeleter.IsBusy)
                playlistDeleter.RunWorkerAsync();

            // If the deleted playlist is the current one, delete it
            if (currentPlaylist.ID == id)
                LoadDefaultPlaylist();

            // Remove the playlist node from the tree
            treeLibrary.Nodes[2].Nodes.Cast<LibraryNode>().First<LibraryNode>(ln => (int)ln.DatabaseKey == id).Remove();
        }
        public void DeletePlaylist() { DeletePlaylist(currentPlaylist.ID); }
        
        /// <summary>
        /// Determine how many items in the playlist are viewable
        /// </summary>
        public void CountPlaylistItemsViewable()
        {
            if (lstPlaylist.Items.Count > 0) {
                playlistItemsViewable = lstPlaylist.ClientRectangle.Height / lstPlaylist.GetItemRect(0).Height;
                return;
            }

            lstPlaylist.Items.Add("Test");
            playlistItemsViewable = lstPlaylist.ClientRectangle.Height / lstPlaylist.GetItemRect(0).Height;
            lstPlaylist.Items.Clear();
        }

        /// <summary>
        /// Add a song to the library
        /// </summary>
        /// <param name="s">The song to add</param>
        public void AddSongToLibrary(Song s)
        {
            Song inLibrary = db.Find<Song>(dbSong => dbSong.Title == s.Title && dbSong.Artist == s.Artist);
            if (inLibrary == null)
            {
                db.Insert(s);

                LibraryNode artistNode = GetOrMakeNode(treeLibrary.Nodes[0], s.Artist, tn => tn.Text.ToLower() == s.Artist.ToLower());
                LibraryNode albumNode = GetOrMakeNode(artistNode, s.Album, tn => tn.Text.ToLower() == s.Album.ToLower());
                albumNode.Nodes.Insert((int)s.Data.Tag.Track, new LibraryNode(s));
            }
            else
            {
                if (inLibrary.FileName != s.FileName)
                {
                    inLibrary.FileName = s.FileName;
                    db.Update(inLibrary);
                }
            }
        }

        public void RemoveSongFromPlaylistByID(int id, bool save = true)
        {
            int index = currentPlaylist.Songs.IndexOf(id);
            if (index > -1)
                RemoveSongFromPlaylist(index, save);
        }

        public void RemoveSongFromPlaylist(int index, bool save = true)
        {
            if (playlistIndex == index)
                playlistIndex--;
            shuffledSongs.Remove(currentPlaylist.Songs[index]);
            currentPlaylist.Songs.RemoveAt(index);
            lstPlaylist.Items.RemoveAt(index);

            if (save)
                SavePlaylist();
        }

        #endregion
    }
}
