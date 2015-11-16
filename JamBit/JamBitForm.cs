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
            if (Properties.Settings.Default.LastPlaylistIndex > 0)
            {
                currentPlaylist = db.Get<Playlist>(Properties.Settings.Default.LastPlaylistIndex);
                lblPlaylistName.Text = currentPlaylist.Name;
                foreach (PlaylistItem pi in db.Table<PlaylistItem>().Where<PlaylistItem>(pi => pi.PlaylistID == currentPlaylist.ID))
                    AddSongToPlaylist(pi.SongID);
            }

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

        #region Library Tree View Methods

        /// <summary>
        /// Reset the tree view and populate it with the songs in the library, the recently played songs, and the user-created playlists
        /// </summary>
        private void PopulateLibraryTree()
        {
            // Clear the tree view initially
            treeLibrary.Nodes.Clear();

            //
            // Load the Library node, holding every song in the library
            //
            LibraryNode libraryNode = new LibraryNode(LibraryNode.LibraryNodeType.Playable, "Library");
            treeLibrary.Nodes.Add(libraryNode);

            // Initialize the nodes to be used 
            LibraryNode artistNode = null, albumNode = null;

            foreach (Song s in db.Table<Song>())
            {
                // If this is the first song or the last song doesn't have the same artist, find the artist node in the tree or make one
                if (artistNode == null || artistNode.Text != s.Artist)
                    artistNode = GetOrMakeNode(libraryNode, s.Artist, tn => tn.Text.ToLower() == s.Artist.ToLower());

                // If this is the first song or the last song doesn't have the same album, find the artist node in the tree or make one
                if (albumNode == null || albumNode.Text != s.Album)
                    albumNode = GetOrMakeNode(artistNode, s.Album, tn => tn.Text.ToLower() == s.Album.ToLower());

                // If the filename of the song exists, insert the song based on track number
                if (File.Exists(s.FileName))
                    albumNode.Nodes.Insert((int)s.Data.Tag.Track, new LibraryNode(s));
                else
                    albumNode.Nodes.Add(new LibraryNode(s));
            }
            
            //
            // Load the Recently Played node, holding a list of the last songs played
            //
            LibraryNode recentNode = new LibraryNode(LibraryNode.LibraryNodeType.Playable, "Recently Played");
            treeLibrary.Nodes.Add(recentNode);

            // Add a node to the tree for every song in the recent song list
            try
            {
                foreach (RecentSong rs in db.Table<RecentSong>())
                {
                    Song song = db.Get<Song>(rs.SongID);
                    recentNode.Nodes.Add(new LibraryNode(song));
                }
            }
            catch (SQLiteException) { }

            //
            // Load the Plyalists node, holding a node for each playlist the user has made
            //
            LibraryNode playlistsNode = new LibraryNode(LibraryNode.LibraryNodeType.Playlists, "Playlists");
            treeLibrary.Nodes.Add(playlistsNode);

            // Add a node to the tree for each playlist
            try
            {
                foreach (Playlist p in db.Table<Playlist>())
                    playlistsNode.Nodes.Add(new LibraryNode(LibraryNode.LibraryNodeType.Playlist, p.Name, p.ID));
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Searches the parent node for the first node that matches the given predicate. 
        /// If no matches are found, creates a new node with the provided text
        /// </summary>
        /// <param name="parentNode">The node to search</param>
        /// <param name="textToUse">The text to use if a new node needs to made</param>
        /// <param name="predicate">The predicate to search with</param>
        /// <returns>The found or created node in the tree</returns>
        public LibraryNode GetOrMakeNode(TreeNode parentNode, string textToUse, Func<TreeNode, bool> predicate)
        {
            LibraryNode node = parentNode.Nodes.Cast<TreeNode>().Where(predicate).FirstOrDefault() as LibraryNode;
            if (node != null)
                return node;

            node = new LibraryNode(LibraryNode.LibraryNodeType.Playable, textToUse);
            parentNode.Nodes.Add(node);
            return node;
        }

        /// <summary>
        /// Attempts to find the node in the tree view that matches the given song
        /// </summary>
        /// <param name="s">The song to search for</param>
        /// <returns>The matching node or null</returns>
        public LibraryNode GetSongNode(Song s)
        {
            return treeLibrary.Nodes[0].Nodes.Cast<LibraryNode>().FirstOrDefault(artist => artist.Text == s.Artist).
                Nodes.Cast<LibraryNode>().FirstOrDefault(album => album.Text == s.Album).
                Nodes.Cast<LibraryNode>().FirstOrDefault(song => song.Text == s.Title);
        }

        #endregion

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
        /// Adds a song to the playlist by ID
        /// </summary>
        /// <param name="id"></param>
        public void AddSongToPlaylist(int id)
        {
            AddSongToPlaylist(db.Get<Song>(id));
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
            new OptionsForm(this).Show();
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
            currentPlaylist.SaveToDatabase(db);
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
            currentPlaylist.SaveToDatabase(db);
            lblPlaylistName.Text = currentPlaylist.Name;
            Properties.Settings.Default.LastPlaylistIndex = currentPlaylist.ID;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Clear the current playlist
        /// </summary>
        public void ClearPlaylist()
        {
            MusicPlayer.CloseSong();
            currentPlaylist = new Playlist();
            shuffledSongs.Clear();
            lblPlaylistName.Text = "";
            lstPlaylist.Items.Clear();
            playlistIndex = -1;
            Properties.Settings.Default.LastPlaylistIndex = 0;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Recursively add all songs in a playable node to the current playlist
        /// </summary>
        /// <param name="playable">The node to add</param>
        public void AddPlayableNodeSongs(LibraryNode playable)
        {
            if (playable.Nodes.Count == 0)
                AddSongToPlaylist((int)playable.DatabaseKey);
            else
                foreach (LibraryNode child in playable.Nodes.Cast<LibraryNode>())
                    AddPlayableNodeSongs(child);
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
                foreach (PlaylistItem pi in db.Table<PlaylistItem>().Where<PlaylistItem>(pi => pi.PlaylistID == currentPlaylist.ID))
                    AddSongToPlaylist(pi.SongID);
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
                ClearPlaylist();

            // Remove the playlist node from the tree
            treeLibrary.Nodes[2].Nodes.Cast<LibraryNode>().First<LibraryNode>(ln => (int)ln.DatabaseKey == id).Remove();
        }
        
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

        public void RemoveSongFromPlaylistByID(int id)
        {
            int index = currentPlaylist.Songs.IndexOf(id);
            if (index > -1)
                RemoveSongFromPlaylist(index);
        }

        public void RemoveSongFromPlaylist(int index)
        {
            if (playlistIndex == index)
                playlistIndex--;
            shuffledSongs.Remove(currentPlaylist.Songs[index]);
            currentPlaylist.Songs.RemoveAt(index);
            lstPlaylist.Items.RemoveAt(index);
        }

        #endregion

        #region Control Event Methods

        #region Playback Control Event Methods

        private void btnPlay_Click(object sender, EventArgs e)
        {
            // Play the current song
            PlaySong();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            // Pause the current song
            PauseSong();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            ShowOpenFileDialog();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            // Prematurely end the song
            ChangeSong();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            // Prematurely end the song with the flag for previous
            ChangeSong(true);
        }

        private void btnPlayMode_Click(object sender, EventArgs e)
        {
            // If at the last playmode, restart at the beginning
            if (playMode == RepeatMode.None)
                playMode = RepeatMode.Loop;

            // Otherwise just go to the next playmode
            else
                playMode++;

            SetPlayMode(playMode);
        }

        private void prgSongTime_SelectedValue(object sender, EventArgs e)
        {
            // Update the current song to the new position chosen by the user
            MusicPlayer.SeekTo((((double)prgSongTime.Value / 1000 * MusicPlayer.curSong.Length)));

            // Update the current time label to the new position
            int seconds = (int)MusicPlayer.CurrentTime();
            lblCurrentTime.Text = String.Format("{0}:{1:D2}", seconds / 60, seconds % 60);
        }

        private void pgrVolume_ValueSlidTo(object sender, EventArgs e)
        {
            // Change the volume on the music player to the new volume
            MusicPlayer.SetVolume(prgVolume.Value);

            // Update the user setting with the new volume
            Properties.Settings.Default.PreferredVolume = (byte)prgVolume.Value;
            Properties.Settings.Default.Save();
        }

        #endregion

        #region Programmed Control Event Methods

        private void checkTime_Tick(object sender, EventArgs e)
        {
            // Get the current position of the song in seconds
            int seconds = (int)MusicPlayer.CurrentTime();

            // Update the label using an appropriate format
            lblCurrentTime.Text = String.Format("{0}:{1:D2}", seconds / 60, seconds % 60);

            // Update the progress bar to the current position
            prgSongTime.SetValue((int)((double)seconds / MusicPlayer.curSong.Length * prgSongTime.Maximum));
        }

        private void playlistScrollTimer_Tick(object sender, EventArgs e)
        {
            if (playlistScrollDirection == 1 && lstPlaylist.TopItem.Index + playlistItemsViewable >= lstPlaylist.Items.Count)
                return;
            if (playlistScrollDirection == -1 && lstPlaylist.TopItem.Index == 0)
                return;

            lstPlaylist.TopItem = lstPlaylist.Items[lstPlaylist.TopItem.Index + playlistScrollDirection];
        }

        private void openFileDialog_OnFileOk(object sender, CancelEventArgs e)
        {
            // For each song opened, attempt to add it to the library
            foreach (string fileName in openFileDialog.FileNames)
                AddSongToLibrary(new Song(fileName));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Safely remove database connection from memory
            db.Dispose();
        }

        private void libraryScanner_DoWork(object sender, DoWorkEventArgs e)
        {
            // The folder currently being scanned
            string folder;

            // Repeat until there are no folders left to scan
            while (foldersToScan.Count > 0)
            {
                Song s = new Song();
                if (foldersToScan.TryDequeue(out folder))
                    // For each file in the current folder
                    foreach (string file in Directory.GetFiles(folder, "*.mp3", SearchOption.AllDirectories))
                    {
                        try
                        {
                            s = new Song(file);
                            libraryScanner.ReportProgress(0, s);
                        }

                        // If the filename is too long
                        catch (System.IO.PathTooLongException) { }
                        if (s.Data != null)
                            s.Data.Dispose();
                    }
            }
        }

        private void libraryScanner_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            AddSongToLibrary(e.UserState as Song);
        }

        private void libraryScanner_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private void playlistDeleter_DoWork(object sender, DoWorkEventArgs e)
        {
            int id;
            while (playlistsToDelete.Count > 0)
            {
                if (!playlistsToDelete.TryDequeue(out id)) continue;
                db.BeginTransaction();
                Playlist toDelete = db.Get<Playlist>(id);

                foreach (PlaylistItem pi in db.Table<PlaylistItem>().Where(obj => obj.PlaylistID == id).ToList())
                    db.Delete(pi);
                db.Delete(db.Get<Playlist>(id));
                db.Commit();
            }
        }

        #endregion

        #region Playlist ListView

        private void lstPlaylist_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                currentPlaylistOptions.Show(sender as Control, e.Location);
        }

        private void lstPlaylist_DoubleClick(object sender, EventArgs e)
        {
            // If there is a selection in the list of songs in the playlist
            if (lstPlaylist.SelectedIndices.Count == 1 && playlistIndex != lstPlaylist.SelectedIndices[0])
            {
                // Open the selected song
                playlistIndex = lstPlaylist.SelectedIndices[0];
                OpenSong();
            }
        }

        private void lstPlaylist_DragDrop(object sender, DragEventArgs e)
        {
            playlistScrollTimer.Stop();
            if (lstPlaylist.SelectedItems.Count == 0) return;

            //Obtain the item that is located at the specified location of the mouse pointer.
            Point listPoint = lstPlaylist.PointToClient(new Point(e.X, e.Y));
            ListViewItem dragToItem = lstPlaylist.GetItemAt(listPoint.X, listPoint.Y);
            if (dragToItem == null) return;

            List<int> newIndexes = new List<int>();
            for (int i = 0; i < currentPlaylist.Songs.Count; i++)
                newIndexes.Add(i);

            bool before = dragToItem.Index < lstPlaylist.SelectedItems[0].Index;

            int placementIndex = dragToItem.Index;
            foreach (ListViewItem item in lstPlaylist.SelectedItems)
            {
                if (item.Index == dragToItem.Index) continue;
                newIndexes.Remove(item.Index);
                newIndexes.Insert(placementIndex, item.Index);
                ListViewItem toInsert = item.Clone() as ListViewItem;
                toInsert.Selected = true;
                lstPlaylist.Items.Insert(placementIndex, toInsert);
                lstPlaylist.Items.Remove(item);

                if (before) placementIndex++;
            }

            List<int> newIDs = new List<int>();
            List<PlaylistItem> toUpdate = new List<PlaylistItem>();
            for (int i = 0; i < newIndexes.Count; i++)
            {
                newIDs.Add(db.Table<PlaylistItem>().First<PlaylistItem>(pi => pi.PlaylistID == currentPlaylist.ID && pi.SongID == currentPlaylist.Songs[i]).ID);
                toUpdate.Add(db.Table<PlaylistItem>().First<PlaylistItem>(pi => pi.PlaylistID == currentPlaylist.ID && pi.SongID == currentPlaylist.Songs[newIndexes[i]]));
            }

            db.BeginTransaction();

            for (int i = 0; i < newIndexes.Count; i++)
            {
                if (newIndexes[i] == i) continue;
                toUpdate[i].ID = newIDs[i];
                db.Update(toUpdate[i]);
            }

            db.Commit();

            currentPlaylist = db.Get<Playlist>(currentPlaylist.ID);
            foreach (PlaylistItem pi in db.Table<PlaylistItem>().Where<PlaylistItem>(pi => pi.PlaylistID == currentPlaylist.ID))
                currentPlaylist.Songs.Add(pi.SongID);

            playlistIndex = newIndexes.IndexOf(playlistIndex);
        }

        private void lstPlaylist_ItemDrag(object sender, ItemDragEventArgs e)
        {
            lstPlaylist.DoDragDrop(lstPlaylist.SelectedItems, DragDropEffects.Move);
        }

        private void lstPlaylist_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void lstPlaylist_DragOver(object sender, DragEventArgs e)
        {
            Point position = lstPlaylist.PointToClient(new Point(e.X, e.Y));

            if (position.Y <= (lstPlaylist.Font.Height * 2))
            {
                // getting close to top, ensure previous item is visible
                playlistScrollDirection = -1;
                playlistScrollTimer.Start();
            }
            else if (position.Y >= lstPlaylist.ClientSize.Height - lstPlaylist.Font.Height / 2)
            {
                // getting close to bottom, ensure next item is visible
                playlistScrollDirection = 1;
                playlistScrollTimer.Start();
            }
            else
                playlistScrollTimer.Stop();
        }

        #endregion

        #region Menu Event Methods

        private void mnuFileOpen_Click(object sender, EventArgs e) { ShowOpenFileDialog(); }

        private void mnuFilePreferences_Click(object sender, EventArgs e) { OpenPreferencesForm(); }        

        private void mnuFileClose_Click(object sender, EventArgs e) { CloseForm(); }

        private void mnuPlaybackPlay_Click(object sender, EventArgs e) { PlaySong(); }

        private void mnuPlaybackPause_Click(object sender, EventArgs e) { PauseSong(); }

        private void mnuPlayModeLoop_Click(object sender, EventArgs e) { SetPlayMode(RepeatMode.Loop); }

        private void mnuPlayModeRepeat_Click(object sender, EventArgs e) { SetPlayMode(RepeatMode.Repeat); }

        private void mnuPlayModeShuffle_Click(object sender, EventArgs e) { SetPlayMode(RepeatMode.Shuffle); }

        private void mnuPlayModeNone_Click(object sender, EventArgs e) { SetPlayMode(RepeatMode.None); }

        private void mnuPlaylistNext_Click(object sender, EventArgs e) { ChangeSong(); }

        private void mnuPlaylistPrevious_Click(object sender, EventArgs e) { ChangeSong(true); }

        private void mnuPlaylistCreate_Click(object sender, EventArgs e) { CreatePlaylist(); }

        private void mnuPlaylistSave_Click(object sender, EventArgs e) { SavePlaylist(); }

        private void mnuPlaylistSaveAs_Click(object sender, EventArgs e) { SaveAsNewPlaylist(); }

        private void mnuPlaylistClear_Click(object sender, EventArgs e) { ClearPlaylist(); }

        #endregion

        #region Context Menu Event Methods

        private void libraryOptions_ItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == playableOptions.Items[0])
                foreach (TreeNode node in treeLibrary.SelectedNodes)
                    AddPlayableNodeSongs(node as LibraryNode);
        }

        private void currentPlaylistOptions_ItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            // Iterate from the last selection to the first to prevent changes in indices during removal
            if (e.ClickedItem == currentPlaylistOptions.Items[0])
                for (int i = lstPlaylist.SelectedIndices.Count - 1; i >= 0; i--)
                {
                    RemoveSongFromPlaylist(lstPlaylist.SelectedIndices[i]);
                }                    
        }

        private void playlistOptions_ItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == playlistOptions.Items[0])
                LoadPlaylist((int)playlistRightClicked.DatabaseKey);
            else
                DeletePlaylist((int)playlistRightClicked.DatabaseKey);
        }

        private void playlistsOptions_ItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == playlistsOptions.Items[0])
                CreatePlaylist();
        }

        #endregion

        #region Library TreeView

        private void treeLibrary_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                LibraryNode clicked = treeLibrary.GetNodeAt(e.Location) as LibraryNode;
                switch (clicked.LibraryType)
                {
                    case LibraryNode.LibraryNodeType.Playlists:
                        playlistsOptions.Show(sender as Control, e.Location);
                        break;
                    case LibraryNode.LibraryNodeType.Playlist:
                        playlistRightClicked = clicked;
                        playlistOptions.Show(sender as Control, e.Location);
                        break;
                    default:
                        playableOptions.Show(sender as Control, e.Location);
                        break;
                }
            }                
        }

        private void treeLibrary_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (treeLibrary.HitTest(e.Location).Location == TreeViewHitTestLocations.PlusMinus)
                return;

            LibraryNode node = e.Node as LibraryNode;
            switch (node.LibraryType)
            {
                case LibraryNode.LibraryNodeType.Playable:
                    AddPlayableNodeSongs(node);
                    break;
                case LibraryNode.LibraryNodeType.Playlist:
                    LoadPlaylist((int)node.DatabaseKey);
                    break;
            }
        }

        private void treeLibrary_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = preventExpand;
            preventExpand = false;
        }

        private void treeLibrary_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = preventExpand;
            preventExpand = false;
        }

        private void treeLibrary_MouseDown(object sender, MouseEventArgs e)
        {
            if (treeLibrary.HitTest(e.Location).Location != TreeViewHitTestLocations.PlusMinus)
            {
                preventExpand = (int)DateTime.Now.Subtract(lastMouseDown).TotalMilliseconds < SystemInformation.DoubleClickTime;
                lastMouseDown = DateTime.Now;
            }
        }

        #endregion

        #endregion
    }
}
