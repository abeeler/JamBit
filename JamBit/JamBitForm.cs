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
            
            //db.DropTable<Song>();

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

            // Initialize the dialog for openning new files
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

            // Start playlist
            playlistPopulating.RunWorkerAsync();

            checkTime = new Timer();
            checkTime.Interval = 1000;
            checkTime.Tick += new System.EventHandler(checkTime_Tick);            
        }

        #region Form Methods

        private void RefreshPlayer()
        {
            prgSongTime.SetValue(0);
            lblSongInformation.CycleText = new string[]{
                "Title: " + MusicPlayer.curSong.Data.Tag.Title,
                "Artist: " + MusicPlayer.curSong.Data.Tag.FirstPerformer,
                "Album: " + MusicPlayer.curSong.Data.Tag.Album 
            };
            lblCurrentTime.Text = "0:00";
            String format = MusicPlayer.curSong.Data.Properties.Duration.Hours > 0 ? @"h':'mm':'ss" : @"mm':'ss";
            lblSongLength.Text = (MusicPlayer.curSong.Data.Properties.Duration - new TimeSpan(0, 0, 1)).ToString(format);
        }

        public void PauseTimeCheck() { checkTime.Stop(); }

        public void StartTimeCheck() { checkTime.Start(); }

        public void SongEnded()
        {
            switch (playMode)
            {
                case RepeatMode.Loop:
                    btnNext_Click(this, new EventArgs());
                    break;
            }
            RefreshPlayer();
        }

        public void AddSong(string fileName)
        {
            Song s = new Song(fileName);
            try { db.Get<Song>(s.Checksum); }
            catch (System.InvalidOperationException)
            {
                currentPlaylist.Songs.Add(s);
                lstPlaylist.Items.Add(new ListViewItem(new string[] {
                    s.Data.Tag.Title, s.Data.Tag.FirstPerformer, s.Data.Tag.Album
                }));
                db.Insert(s);
            }
            s.Data.Dispose();
        }

        public void LibraryScan(params string[] folderPaths)
        {
            foreach (string folder in folderPaths)
                foldersToScan.Enqueue(folder);
            if (!libraryScanner.IsBusy)
                libraryScanner.RunWorkerAsync();
        }

        #endregion

        #region Control Event Methods

        private void checkTime_Tick(object sender, EventArgs e)
        {
            int seconds = (int)MusicPlayer.CurrentTime();
            lblCurrentTime.Text = String.Format("{0}:{1:D2}", seconds / 60, seconds % 60);

            prgSongTime.SetValue((int)((double)seconds / MusicPlayer.curSong.Length * prgSongTime.Maximum));
        }

        private void prgSongTime_SelectedValue(object sender, EventArgs e)
        {
            MusicPlayer.SeekTo((((double)prgSongTime.Value / 1000 * MusicPlayer.curSong.Length)));
            int seconds = (int)MusicPlayer.CurrentTime();
            lblCurrentTime.Text = String.Format("{0}:{1:D2}", seconds / 60, seconds % 60);
        }

        private void pgrVolume_ValueSlidTo(object sender, EventArgs e)
        {
            MusicPlayer.SetVolume(prgVolume.Value);
            Properties.Settings.Default.PreferredVolume = (byte)prgVolume.Value;
            Properties.Settings.Default.Save();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            MusicPlayer.PlaySong();      
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            MusicPlayer.PauseSong();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            openFileDialog.ShowDialog();
        }

        private void openFileDialog_OnFileOk(object sender, CancelEventArgs e)
        {
            lblSongInformation.CycleText = new string[] { openFileDialog.FileName };
            MusicPlayer.OpenSong(openFileDialog.FileName);
            RefreshPlayer();

            db.BeginTransaction();
            foreach (string fileName in openFileDialog.FileNames)
                AddSong(fileName);
            db.Commit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            db.Dispose();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (currentPlaylist.Count > 0) {
                if (playlistIndex == currentPlaylist.Count) playlistIndex = 0;
                MusicPlayer.OpenSong(currentPlaylist.Songs[playlistIndex++]);
                RefreshPlayer();
            }
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (currentPlaylist.Count > 0)
            {
                playlistIndex = playlistIndex == 1 ? currentPlaylist.Count : playlistIndex - 1;
                MusicPlayer.OpenSong(currentPlaylist.Songs[playlistIndex - 1]);
                RefreshPlayer();
            }
        }

        private void lstPlaylist_DoubleClick(object sender, EventArgs e)
        {
            if (lstPlaylist.SelectedIndices.Count == 1)
            {
                playlistIndex = lstPlaylist.SelectedIndices[0];
                MusicPlayer.OpenSong(currentPlaylist.Songs[playlistIndex++]);
                RefreshPlayer();
            }
        }

        private void lstPlaylist_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstPlaylist.SelectedIndices.Count == 1)
            {
                playlistIndex = lstPlaylist.SelectedIndices[0];
                MusicPlayer.OpenSong(currentPlaylist.Songs[playlistIndex++]);
                RefreshPlayer();
            }
        }

        private void mnuFileOpen_Click(object sender, EventArgs e)
        {
            btnOpen_Click(sender, e);
        }

        private void mnuPrefLibFolders_Click(object sender, EventArgs e)
        {
            new OptionsForm(this).Show();
        }

        private void libraryScanner_DoWork(object sender, DoWorkEventArgs e)
        {
            string folder;

            while (foldersToScan.Count > 0)
            {
                Song s = new Song();
                if (foldersToScan.TryDequeue(out folder))
                    foreach (string file in Directory.GetFiles(folder, "*.mp3", SearchOption.AllDirectories))
                    {
                        try
                        {
                            s = new Song(file);
                            db.Get<Song>(s.Checksum);
                        }
                        catch (System.InvalidOperationException)
                        {
                            libraryScanner.ReportProgress(0, new ListViewItem(new string[] {
                                s.Data.Tag.Title, s.Data.Tag.FirstPerformer, s.Data.Tag.Album
                            }));
                            currentPlaylist.Songs.Add(s);
                            db.Insert(s);
                        }
                        catch (System.IO.PathTooLongException) { }
                        if (s.Data != null)
                            s.Data.Dispose();
                    }
            }                
        }

        private void playlistPopulating_DoWork(object sender, DoWorkEventArgs e)
        {
            playlistPopulating.ReportProgress(1);
            currentPlaylist = new Playlist();
            for (int i = 0; i < db.Table<Song>().Count(); i += 10)
                foreach (Song s in db.Table<Song>().Skip(i).Take(10))
                {
                    currentPlaylist.Songs.Add(s);
                    playlistPopulating.ReportProgress(0, new ListViewItem(new string[] {
                        s.Data.Tag.Title, s.Data.Tag.FirstPerformer, s.Data.Tag.Album
                    }));
                }
            playlistPopulating.ReportProgress(2);
        }

        private void playlistBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 1)
            {
                this.Cursor = Cursors.WaitCursor;
                Application.DoEvents();
            }
            else if (e.ProgressPercentage == 2)
                this.Cursor = Cursors.Default;
            else
                lstPlaylist.Items.Add((ListViewItem)e.UserState);
        }

        #endregion
    }
}
