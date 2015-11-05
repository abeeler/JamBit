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

        private Timer checkTime;
        private OpenFileDialog openFileDialog;
        private BackgroundWorker libraryScanner;
        private BackgroundWorker playlistPopulating;
        private ContextMenuStrip libraryOptions;
        private ContextMenuStrip playlistOptions;
        private ToolStripMenuItem selectedPlayMode;

        private ConcurrentQueue<string> foldersToScan;

        private RepeatMode playMode;
        private Random shuffleRandom = new Random();
        private Playlist currentPlaylist = new Playlist();
        private int playlistIndex = -1;
        private bool preventExpand = false;
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

            //ResetApplication();

            // Create tables if they do not already exist
            db.CreateTable<Song>();
            db.CreateTable<Playlist>();
            db.CreateTable<PlaylistItem>();
            db.CreateTable<RecentSong>();

            // Create concurrent queue for folder scanning
            foldersToScan = new ConcurrentQueue<string>();

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
            libraryOptions = new ContextMenuStrip();
            libraryOptions.Items.Add("Add selection to current playlist");
            libraryOptions.ItemClicked += libraryOptions_ItemClick;

            // Initialize the playlist options context menu
            playlistOptions = new ContextMenuStrip();
            playlistOptions.Items.Add("Remove selection from current playlist");
            playlistOptions.ItemClicked += playlistOptions_ItemClick;

            // Initialize the background worker for scanning in files from library folders
            libraryScanner = new BackgroundWorker();
            libraryScanner.WorkerReportsProgress = true;
            libraryScanner.WorkerSupportsCancellation = true;
            libraryScanner.DoWork += libraryScanner_DoWork;
            libraryScanner.ProgressChanged += libraryScanner_ProgressChanged;
            libraryScanner.RunWorkerCompleted += libraryScanner_RunWorkerCompleted;

            // Initialzize tree view
            LibraryNode libraryNode = new LibraryNode(LibraryNode.LibraryNodeType.Library);
            libraryNode.Text = "Library";
            treeLibrary.Nodes.Add(libraryNode);

            foreach (Song artist in db.Table<Song>().Distinct<Song>(new Song.ArtistComparator()))
            {
                LibraryNode artistNode = new LibraryNode(LibraryNode.LibraryNodeType.Artist);
                artistNode.Text = artist.Artist;
                foreach (Song album in db.Table<Song>().Where(s => s.Artist == artist.Artist).Distinct<Song>(new Song.AlbumComparator()))
                {
                    LibraryNode albumNode = new LibraryNode(LibraryNode.LibraryNodeType.Album);
                    albumNode.DatabaseKey = artistNode.Text;
                    albumNode.Text = album.Album;
                    foreach (Song song in db.Table<Song>().Where(s => s.Artist == artist.Artist && s.Album == album.Album))
                    {
                        LibraryNode songNode = new LibraryNode(LibraryNode.LibraryNodeType.Song);
                        songNode.DatabaseKey = song.ID;
                        songNode.Text = song.Title;
                        albumNode.Nodes.Add(songNode);
                    }
                    artistNode.Nodes.Add(albumNode);
                }
                libraryNode.Nodes.Add(artistNode);
            }

            LibraryNode recentNode = new LibraryNode(LibraryNode.LibraryNodeType.RecentlyPlayed);
            recentNode.Text = "Recently Played";
            treeLibrary.Nodes.Add(recentNode);

            try
            {
                foreach (RecentSong rs in db.Table<RecentSong>())
                {
                    Song song = db.Get<Song>(rs.SongID);
                    LibraryNode songNode = new LibraryNode(LibraryNode.LibraryNodeType.Song);
                    songNode.DatabaseKey = song.ID;
                    songNode.Text = song.Title;
                    recentNode.Nodes.Add(songNode);
                }
            }
            catch (SQLiteException) { }

            LibraryNode playlistsNode = new LibraryNode(LibraryNode.LibraryNodeType.Playlists);
            playlistsNode.Text = "Playlists";
            treeLibrary.Nodes.Add(playlistsNode);

            try
            {
                foreach (Playlist p in db.Table<Playlist>())
                {
                    LibraryNode playlistNode = new LibraryNode(LibraryNode.LibraryNodeType.Playlist);
                    playlistNode.Text = p.Name;
                    playlistNode.DatabaseKey = p.ID;
                    playlistsNode.Nodes.Add(playlistNode);
                }
            } catch (Exception) { }

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
            checkTime = new Timer();
            checkTime.Interval = 1000;
            checkTime.Tick += new System.EventHandler(checkTime_Tick);            
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
        public void PauseTimeCheck() { checkTime.Stop(); }

        /// <summary>
        /// Start the timer that updates the current song position
        /// </summary>
        public void StartTimeCheck() { checkTime.Start(); }

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
        /// Attempt to add a new song to the library
        /// </summary>
        /// <param name="fileName">The filename of the song to add</param>
        public void AddSong(string fileName)
        {
            // Create a new song object using the given file
            Song s = new Song(fileName);

            // Attempt to find the song in the database
            if (db.Find<Song>(dbSong => dbSong.Title == s.Title && dbSong.Artist == s.Artist) == null)
            {
                // If it is not found, add the song to the library and the current playlist
                currentPlaylist.Songs.Add(s.ID);
                lstPlaylist.Items.Add(new ListViewItem(new string[] {
                    s.Data.Tag.Title, s.Data.Tag.FirstPerformer, s.Data.Tag.Album, s.PlayCount.ToString()
                }));
                db.Insert(s);
            }

            // Ensure the data from the song is not leaked
            s.Data.Dispose();
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

        public void AddSongToPlaylist(int id)
        {
            AddSongToPlaylist(db.Get<Song>(id));
            try {  }
            catch (System.InvalidOperationException) { }
        }

        public void AddSongToPlaylist(Song s)
        {
            if (!currentPlaylist.Songs.Contains(s.ID))
            {
                currentPlaylist.Songs.Add(s.ID);
                lstPlaylist.Items.Add(new ListViewItem(new string[] {
                    s.Title, s.Artist, s.Album, s.PlayCount.ToString()
                }));                
            }
        }

        private void OpenSong() { OpenSong(db.Get<Song>(currentPlaylist.Songs[playlistIndex])); }

        private void OpenSong(Song s)
        {
            MusicPlayer.OpenSong(s);
            RefreshPlayer();
            s.PlayCount++;
            db.Update(s);

            if (!shuffledSongs.Contains(s.ID))
                shuffledSongs.Add(s.ID);

            lstPlaylist.Items[playlistIndex].SubItems[3] = 
                new ListViewItem.ListViewSubItem(lstPlaylist.Items[playlistIndex], s.PlayCount.ToString());

            bool foundMatch = false;
            try {
                db.Table<RecentSong>().Where(recentSong => recentSong.SongID == s.ID).First();
            } catch (Exception) { foundMatch = true; }

            if (foundMatch)
            {
                try {
                    foundMatch = db.Table<RecentSong>().Count() < Properties.Settings.Default.RecentlyPlayedMax;
                } catch (SQLiteException) { foundMatch = true; }

                if (foundMatch)
                    db.Insert(new RecentSong(s.ID));
                else
                {
                    if (Properties.Settings.Default.RecentlyPlayedIndex == Properties.Settings.Default.RecentlyPlayedMax + 1)
                    {
                        Properties.Settings.Default.RecentlyPlayedIndex = 1;
                        Properties.Settings.Default.Save();
                    }
                        
                    RecentSong toUpdate = db.Get<RecentSong>(Properties.Settings.Default.RecentlyPlayedIndex++);
                    toUpdate.SongID = s.ID;
                    db.Update(toUpdate);
                }

            }
        }

        public void ResetApplication()
        {
            db.DropTable<Song>();
            db.DropTable<Playlist>();
            db.DropTable<PlaylistItem>();
            db.DropTable<RecentSong>();

            Properties.Settings.Default.LastPlaylistIndex = 0;
            Properties.Settings.Default.LastPlayMode = 0;
            Properties.Settings.Default.RecentlyPlayedIndex = 1;
            Properties.Settings.Default.Save();
        }

        public void OpenFiles()
        {
            openFileDialog.ShowDialog();
        }

        public void OpenPreferencesForm()
        {
            new OptionsForm(this).Show();
        }

        public void CloseForm()
        {
            Close();
        }

        public void PlaySong()
        {
            if (playlistIndex == -1 && currentPlaylist.Count > 0)
                ChangeSong();
            MusicPlayer.PlaySong();
            KeepSystemAwake();
        }

        public void PauseSong()
        {
            MusicPlayer.PauseSong();
            RestoreExecutionState();
        }

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

        public void CreatePlaylist()
        {
            Playlist newPlaylist = new Playlist();
            newPlaylist.Name = MusicPlayerControlsLibrary.Prompt.ShowDialog("Enter new playlist name: ", "Create a Playlist");
            CreatePlaylist(newPlaylist);
        }

        public void CreatePlaylist(Playlist newPlaylist)
        {
            db.Insert(newPlaylist);
            LibraryNode playlistNode = new LibraryNode(LibraryNode.LibraryNodeType.Playlist);
            playlistNode.Text = newPlaylist.Name;
            playlistNode.DatabaseKey = newPlaylist.ID;
            treeLibrary.Nodes[2].Nodes.Add(playlistNode);
        }

        public void SavePlaylist()
        {
            db.Update(currentPlaylist);
            currentPlaylist.SaveToDatabase(db);
        }

        public void SaveNewPlaylist()
        {
            currentPlaylist.ID = 0;
            currentPlaylist.Name = MusicPlayerControlsLibrary.Prompt.ShowDialog("Enter new playlist name: ", "Create a Playlist");
            CreatePlaylist(currentPlaylist);
            currentPlaylist.SaveToDatabase(db);
            lblPlaylistName.Text = currentPlaylist.Name;
            Properties.Settings.Default.LastPlaylistIndex = currentPlaylist.ID;
            Properties.Settings.Default.Save();
        }

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
            OpenFiles();
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

        private void openFileDialog_OnFileOk(object sender, CancelEventArgs e)
        {
            // For each song opened, attempt to add it to the library
            foreach (string fileName in openFileDialog.FileNames)
                AddSong(fileName);
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
                            // Attempt to open the current file
                            s = new Song(file);

                            // Check for that song in the database
                            if (db.Find<Song>(dbSong => dbSong.Title == s.Title && dbSong.Artist == s.Artist) == null)
                            {
                                db.Insert(s);

                                LibraryNode artistNode = treeLibrary.Nodes[0].Nodes.Cast<TreeNode>().Where(tn => tn.Text.ToLower() == s.Artist.ToLower()).ToList().FirstOrDefault() as LibraryNode;
                                if (artistNode == null)
                                {
                                    artistNode = new LibraryNode(LibraryNode.LibraryNodeType.Artist);
                                    artistNode.Text = s.Artist;
                                    artistNode.DatabaseKey = s.Artist;
                                    libraryScanner.ReportProgress(0, new TreeNode[] { treeLibrary.Nodes[0], artistNode });
                                }

                                LibraryNode albumNode = artistNode.Nodes.Cast<TreeNode>().Where(tn => tn.Text.ToLower() == s.Album.ToLower()).ToList().FirstOrDefault() as LibraryNode;
                                if (albumNode == null)
                                {
                                    albumNode = new LibraryNode(LibraryNode.LibraryNodeType.Album);
                                    albumNode.Text = s.Album;
                                    albumNode.DatabaseKey = s.Artist;
                                    libraryScanner.ReportProgress(0, new TreeNode[] { artistNode, albumNode });
                                }

                                LibraryNode titleNode = new LibraryNode(LibraryNode.LibraryNodeType.Song);
                                titleNode.Text = s.Title;
                                titleNode.DatabaseKey = s.ID;
                                libraryScanner.ReportProgress(0, new TreeNode[] { albumNode, titleNode });
                            }
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
            TreeNode[] nodes = e.UserState as TreeNode[];
            nodes[0].Nodes.Add(nodes[1]);
        }

        private void libraryScanner_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        #endregion

        #region Playlist ListView

        private void lstPlaylist_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                playlistOptions.Show(sender as Control, e.Location);
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

        #endregion

        #region Menu Event Methods

        private void mnuFileOpen_Click(object sender, EventArgs e) { OpenFiles(); }

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

        private void mnuPlaylistSaveAs_Click(object sender, EventArgs e) { SaveNewPlaylist(); }

        private void mnuPlaylistClear_Click(object sender, EventArgs e) { ClearPlaylist(); }

        #endregion

        #region Context Menu Event Methods

        private void libraryOptions_ItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == libraryOptions.Items[0])
                foreach (TreeNode node in treeLibrary.SelectedNodes)
                    treeLibrary_NodeMouseDoubleClick(this, new TreeNodeMouseClickEventArgs(node, MouseButtons.Left, 1, 0, 0));
        }

        private void playlistOptions_ItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == playlistOptions.Items[0])
                for (int i = lstPlaylist.SelectedIndices.Count - 1; i >= 0; i--)
                {
                    if (playlistIndex == lstPlaylist.SelectedIndices[i])
                        playlistIndex--;
                    shuffledSongs.Remove(currentPlaylist.Songs[lstPlaylist.SelectedIndices[i]]);
                    currentPlaylist.Songs.RemoveAt(lstPlaylist.SelectedIndices[i]);
                    lstPlaylist.Items.RemoveAt(lstPlaylist.SelectedIndices[i]);
                }                    
        }

        #endregion

        #region Library TreeView

        private void treeLibrary_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                switch ((treeLibrary.GetNodeAt(e.Location) as LibraryNode).LibraryType)
                {
                    case LibraryNode.LibraryNodeType.Playlists:
                        break;
                    default:
                        libraryOptions.Show(sender as Control, e.Location);
                        break;
                }
                
        }

        private void treeLibrary_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (treeLibrary.HitTest(e.Location).Location == TreeViewHitTestLocations.PlusMinus)
                return;

            LibraryNode node = e.Node as LibraryNode;
            switch (node.LibraryType)
            {
                case LibraryNode.LibraryNodeType.Song:
                    AddSongToPlaylist((int)node.DatabaseKey);
                    break;
                case LibraryNode.LibraryNodeType.Album:
                    foreach (Song s in db.Table<Song>().Where(s => s.Album == node.Text && s.Artist == (string)node.DatabaseKey))
                        AddSongToPlaylist(s);
                    break;
                case LibraryNode.LibraryNodeType.Artist:
                    foreach (Song s in db.Table<Song>().Where(s => s.Artist == node.Text))
                        AddSongToPlaylist(s);
                    break;
                case LibraryNode.LibraryNodeType.Playlist:
                    ClearPlaylist();
                    try
                    {
                        currentPlaylist = db.Get<Playlist>(node.DatabaseKey);
                        shuffledSongs.Clear();
                        lblPlaylistName.Text = currentPlaylist.Name;
                        Properties.Settings.Default.LastPlaylistIndex = currentPlaylist.ID;
                        Properties.Settings.Default.Save();
                        foreach (PlaylistItem pi in db.Table<PlaylistItem>().Where<PlaylistItem>(pi => pi.PlaylistID == currentPlaylist.ID))
                            AddSongToPlaylist(pi.SongID);
                    }
                    catch (Exception) { }
                    break;
                case LibraryNode.LibraryNodeType.RecentlyPlayed:
                    try
                    {
                        foreach (RecentSong rs in db.Table<RecentSong>())
                            AddSongToPlaylist(db.Get<Song>(rs.SongID));
                    }
                    catch (SQLiteException) { }
                    break;
                case LibraryNode.LibraryNodeType.Library:
                    try
                    {
                        foreach (Song s in db.Table<Song>())
                            AddSongToPlaylist(s);
                    }
                    catch (SQLiteException) { }
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
