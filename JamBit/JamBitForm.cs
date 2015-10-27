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

        private Timer checkTime;
        private OpenFileDialog openFileDialog;
        private SQLite.SQLiteConnection db;
        private RepeatMode playMode = RepeatMode.Loop;
        private Random shuffleRandom = new Random();
        private Playlist currentPlaylist;
        private int playlistIndex = 1;
        private BackgroundWorker libraryScanner;
        private BackgroundWorker playlistPopulating;
        private ConcurrentQueue<string> foldersToScan;

        #endregion

        enum RepeatMode { None, Loop, Repeat, Shuffle }

        public JamBitForm()
        {
            InitializeComponent();

            // Establish DB connection
            db = new SQLiteConnection(Path.Combine(Application.UserAppDataPath, "jambit.db"));
            
            db.DropTable<Song>();

            // Create tables if they do not already exist
            db.CreateTable<Song>();

            // Create concurrent queue for folder scanning
            foldersToScan = new ConcurrentQueue<string>();

            // Initialize music player
            MusicPlayer.parentForm = this;
            MusicPlayer.SetVolume(Properties.Settings.Default.PreferredVolume);
            prgVolume.Value = Properties.Settings.Default.PreferredVolume;

            // If there are songs in the library, load the first
            if (db.Table<Song>().Count() > 0)
            {
                MusicPlayer.OpenSong(db.Table<Song>().First());
                RefreshPlayer();
            }

            // Initialize the dialog for opening new files
            openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "MP3|*.mp3|" +
                "Music Files|*.mp3";
            openFileDialog.FileOk += openFileDialog_OnFileOk;

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
            treeLibrary.Nodes.Add("Library");
            foreach (Song artist in db.Table<Song>().Distinct<Song>(new Song.ArtistComparator()))
            {
                TreeNode artistNode = new TreeNode();
                artistNode.Text = artist.Artist;
                foreach (Song album in db.Table<Song>().Where(s => s.Artist == artist.Artist).Distinct<Song>(new Song.AlbumComparator()))
                {
                    TreeNode albumNode = new TreeNode();
                    albumNode.Text = album.Album;
                    foreach (Song song in db.Table<Song>().Where(s => s.Artist == artist.Artist && s.Album == album.Album))
                        albumNode.Nodes.Add(song.Title);
                    artistNode.Nodes.Add(albumNode);
                }
                treeLibrary.Nodes[0].Nodes.Add(artistNode);
            }

            // Start playlist
            playlistPopulating.RunWorkerAsync();

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
            lblSongInformation.CycleText = new string[]{
                "Title: " + MusicPlayer.curSong.Data.Tag.Title,
                "Artist: " + MusicPlayer.curSong.Data.Tag.FirstPerformer,
                "Album: " + MusicPlayer.curSong.Data.Tag.Album 
            };

            // Update total song length label, hiding hours if the song is shorter than that
            String format = MusicPlayer.curSong.Data.Properties.Duration.Hours > 0 ? @"h':'mm':'ss" : @"mm':'ss";
            lblSongLength.Text = (MusicPlayer.curSong.Data.Properties.Duration - new TimeSpan(0, 0, 1)).ToString(format);
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
                    MusicPlayer.OpenSong(MusicPlayer.curSong);
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
                    if (playlistIndex == currentPlaylist.Count)
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
                currentPlaylist.Songs.Add(s);
                lstPlaylist.Items.Add(new ListViewItem(new string[] {
                    s.Data.Tag.Title, s.Data.Tag.FirstPerformer, s.Data.Tag.Album
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
            // Load the first song openned in the music player
            MusicPlayer.OpenSong(openFileDialog.FileName);
            RefreshPlayer();

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
                if (playlistIndex == currentPlaylist.Count) playlistIndex = 0;
                MusicPlayer.OpenSong(currentPlaylist.Songs[playlistIndex++]);
                RefreshPlayer();
            }
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            // If there is more than one song in the playlist
            if (currentPlaylist.Count > 1)
            {
                // Open the previous song in the playlist
                playlistIndex = playlistIndex == 1 ? currentPlaylist.Count : playlistIndex - 1;
                MusicPlayer.OpenSong(currentPlaylist.Songs[playlistIndex - 1]);
                RefreshPlayer();
            }
        }

        private void lstPlaylist_DoubleClick(object sender, EventArgs e)
        {
            // If there is a selection in the list of songs in the playlist
            if (lstPlaylist.SelectedIndices.Count == 1)
            {
                // Open the selected song
                playlistIndex = lstPlaylist.SelectedIndices[0];
                MusicPlayer.OpenSong(currentPlaylist.Songs[playlistIndex++]);
                RefreshPlayer();
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
                                currentPlaylist.Songs.Add(s);

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

            // Take ten songs from the database and add them to the playlist
            for (int i = 0; i < db.Table<Song>().Count(); i += 10)
                foreach (Song s in db.Table<Song>().Skip(i).Take(10))
                {
                    currentPlaylist.Songs.Add(s);

                    // Send the song information back to the main thread
                    playlistPopulating.ReportProgress(0, new ListViewItem(new string[] {
                        s.Data.Tag.Title, s.Data.Tag.FirstPerformer, s.Data.Tag.Album
                    }));
                }

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

        #endregion
    }
}
