using SQLite;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
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

        private ConcurrentQueue<string> foldersToScan;

        private RepeatMode playMode = RepeatMode.Loop;
        private Random shuffleRandom = new Random();
        private Playlist currentPlaylist = new Playlist();
        private int playlistIndex = -1;
        private bool preventExpand = false;
        private DateTime lastMouseDown;

        #endregion

        enum RepeatMode { None, Loop, Repeat, Shuffle }

        public JamBitForm()
        {
            InitializeComponent();

            // Establish DB connection
            db = new SQLiteConnection(Path.Combine(Application.UserAppDataPath, "jambit.db"));
            
            //db.DropTable<Song>();
            //db.DropTable<RecentSong>();

            // Create tables if they do not already exist
            db.CreateTable<Song>();
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

            // Initialize the background worker for scanning in files from library folders
            libraryScanner = new BackgroundWorker();
            libraryScanner.WorkerReportsProgress = true;
            libraryScanner.WorkerSupportsCancellation = true;
            libraryScanner.DoWork += libraryScanner_DoWork;
            libraryScanner.ProgressChanged += playlistBackgroundWorker_ProgressChanged;

            // Initialize the background worker for populating the playlist from the database
            playlistPopulating = new BackgroundWorker();
            playlistPopulating.WorkerReportsProgress = true;
            playlistPopulating.WorkerSupportsCancellation = true;
            playlistPopulating.DoWork += playlistPopulating_DoWork;
            playlistPopulating.ProgressChanged += playlistBackgroundWorker_ProgressChanged;

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

            treeLibrary.NodeMouseDoubleClick += treeLibrary_NodeMouseDoubleClick;
            treeLibrary.MouseClick += treeLibrary_MouseClick;
            treeLibrary.MouseDown += treeLibrary_MouseDown;
            treeLibrary.BeforeExpand += treeLibrary_BeforeExpand;

            // Start playlist

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
        public void SongEnded()
        {
            switch (playMode)
            {
                // Loop the current playlist
                // Will play the next song or the first in the playlist if at the end
                case RepeatMode.Loop:
                    btnNext_Click(this, new EventArgs());
                    break;

                // Repeat the current song
                case RepeatMode.Repeat:
                    OpenSong(MusicPlayer.curSong);
                    break;

                // Randomly select a song from the current playlist
                case RepeatMode.Shuffle:
                    // TODO: Improve shuffle algorithm
                    playlistIndex = 1 + shuffleRandom.Next(currentPlaylist.Count);
                    btnNext_Click(this, new EventArgs());
                    break;

                // Play through to the end of the playlist and stop
                // After the last song, will load the first and pause the player
                case RepeatMode.None:
                    if (playlistIndex >= currentPlaylist.Count - 1)
                        MusicPlayer.PauseSong();
                    btnNext_Click(this, new EventArgs());
                    break;
            }
            RefreshPlayer();
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
                libraryScanner.RunWorkerAsync();
        }

        public void AddSongToPlaylist(int id)
        {
            try { AddSongToPlaylist(db.Get<Song>(id)); }
            catch (System.InvalidOperationException) { }
        }

        public void AddSongToPlaylist(Song s)
        {
            if (!currentPlaylist.Songs.Contains(s.ID))
            {
                currentPlaylist.Songs.Add(s.ID);
                lstPlaylist.Items.Add(new ListViewItem(new string[] {
                    s.Data.Tag.Title, s.Data.Tag.FirstPerformer, s.Data.Tag.Album, s.PlayCount.ToString()
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
                        Properties.Settings.Default.RecentlyPlayedIndex = 1;
                    RecentSong toUpdate = db.Get<RecentSong>(Properties.Settings.Default.RecentlyPlayedIndex++);
                    toUpdate.SongID = s.ID;
                    db.Update(toUpdate);
                }

            }
            // TODO: Update playcount in playlist display
        }

        #endregion

        #region Control Event Methods

        private void checkTime_Tick(object sender, EventArgs e)
        {
            // Get the current position of the song in seconds
            int seconds = (int)MusicPlayer.CurrentTime();

            // Update the label using an appropriate format
            lblCurrentTime.Text = String.Format("{0}:{1:D2}", seconds / 60, seconds % 60);

            // Update the progress bar to the current position
            prgSongTime.SetValue((int)((double)seconds / MusicPlayer.curSong.Length * prgSongTime.Maximum));
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

        private void btnPlay_Click(object sender, EventArgs e)
        {
            // Play the current song
            if (playlistIndex == -1 && currentPlaylist.Count > 0)
            {
                playlistIndex = 0;
                OpenSong();
            }
            MusicPlayer.PlaySong();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            // Pause the current song
            MusicPlayer.PauseSong();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            // Show the dialog for opening new files
            openFileDialog.ShowDialog();
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

        private void btnNext_Click(object sender, EventArgs e)
        {
            // If there is more than one song in the playlist
            if (currentPlaylist.Count > 1) {
                // Open the next song in the playlist
                if (++playlistIndex == currentPlaylist.Count) playlistIndex = 0;
                OpenSong();
            }
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            // If there is more than one song in the playlist
            if (currentPlaylist.Count > 1)
            {
                // Open the previous song in the playlist
                playlistIndex = playlistIndex <= 0 ? currentPlaylist.Count - 1 : playlistIndex - 1;
                OpenSong();
            }
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

        private void mnuFileOpen_Click(object sender, EventArgs e)
        {
            // Opens the dialog for opening new song files
            btnOpen_Click(sender, e);
        }

        private void mnuPrefLibFolders_Click(object sender, EventArgs e)
        {
            // Show the options form
            new OptionsForm(this).Show();
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
                                // If the song is not in the database
                                // Update the playlist
                                libraryScanner.ReportProgress(0, new ListViewItem(new string[] {
                                    s.Data.Tag.Title, s.Data.Tag.FirstPerformer, s.Data.Tag.Album
                                }));

                                // Add the song to the database
                                db.Insert(s);
                            }
                        }                        
                        
                        // If the filename is too long
                        catch (System.IO.PathTooLongException) { }
                        if (s.Data != null)
                            s.Data.Dispose();
                    }
            }                
        }

        private void playlistPopulating_DoWork(object sender, DoWorkEventArgs e)
        {
            // Update the form with a loading cursor
            playlistPopulating.ReportProgress(1);

            // Reset the current playlist
            currentPlaylist = new Playlist();



            // Return the cursor to normal
            playlistPopulating.ReportProgress(2);
        }

        private void playlistBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // If the state is 1
            if (e.ProgressPercentage == 1)
                // Change the cursor to a loading symbol
                this.Cursor = Cursors.WaitCursor;
            // If the state is 2
            else if (e.ProgressPercentage == 2)
                // Return the cursor to normal
                this.Cursor = Cursors.Default;
            // Otherwise
            else
                // Add the passed argument to the playlist
                lstPlaylist.Items.Add((ListViewItem)e.UserState);
        }

        private void btnPlayMode_Click(object sender, EventArgs e)
        {
            // If at the last playmode, restart at the beginning
            if (playMode == RepeatMode.Shuffle)
                playMode = RepeatMode.None;

            // Otherwise just go to the next playmode
            else
                playMode++;

            // Update button text
            btnPlayMode.Text = Enum.GetName(playMode.GetType(), playMode);
        }

        private void treeLibrary_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
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
                    break;
                case LibraryNode.LibraryNodeType.RecentlyPlayed:
                    try { 
                        foreach (RecentSong rs in db.Table<RecentSong>())
                            AddSongToPlaylist(db.Get<Song>(rs.SongID));
                    }
                    catch (SQLiteException) { }
                    break;
                case LibraryNode.LibraryNodeType.Library:
                    try {
                        foreach (Song s in db.Table<Song>())
                            AddSongToPlaylist(s);
                    } catch (SQLiteException) { }
                    break;
            }
        }

        private void libraryOptions_ItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Add selection to current playlist")
                foreach (TreeNode node in treeLibrary.SelectedNodes)
                    treeLibrary_NodeMouseDoubleClick(this, new TreeNodeMouseClickEventArgs(node, MouseButtons.Left, 1, 0, 0));
        }

        private void treeLibrary_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                libraryOptions.Show(sender as Control, e.Location);
        }

        private void treeLibrary_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = preventExpand;
            preventExpand = false;
        }

        private void treeLibrary_MouseDown(object sender, MouseEventArgs e)
        {
            preventExpand = (int)DateTime.Now.Subtract(lastMouseDown).TotalMilliseconds < SystemInformation.DoubleClickTime;
            lastMouseDown = DateTime.Now;
        }

        private void clearCurrentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MusicPlayer.CloseSong();
            currentPlaylist = new Playlist();
            lstPlaylist.Items.Clear();
            playlistIndex = -1;
        }

        #endregion
    }
}
