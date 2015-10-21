using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JamBit
{
    public partial class Form1 : Form
    {
        private Timer checkTime;

        public Form1()
        {
            InitializeComponent();

            checkTime = new Timer();
            checkTime.Interval = 1000;
            checkTime.Tick += new System.EventHandler(checkTime_Tick);
            checkTime.Start();
            MusicPlayer.OpenSong("C:/song.mp3");
            MusicPlayer.SetVolume(prgVolume.Value);
            RefreshPlayer();
        }

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

        private void checkTime_Tick(object sender, EventArgs e)
        {
            int seconds = (int)(long.Parse(MusicPlayer.CurrentTime()) / 1000);
            lblCurrentTime.Text = String.Format("{0}:{1:D2}", seconds / 60, seconds % 60);

            prgSongTime.SetValue((int)((double)seconds / MusicPlayer.curSong.Length * prgSongTime.Maximum));
            
            if (seconds >= MusicPlayer.curSong.Length)
            {
                MusicPlayer.SeekTo(0);
                MusicPlayer.PlaySong();
                RefreshPlayer();
            }
        }

        private void prgSongTime_SelecedValue(object sender, EventArgs e)
        {
            MusicPlayer.SeekTo(((int)((double)prgSongTime.Value / 1000 * MusicPlayer.curSong.Length)));
            int seconds = (int)(long.Parse(MusicPlayer.CurrentTime()) / 1000);
            lblCurrentTime.Text = String.Format("{0}:{1:D2}", seconds / 60, seconds % 60);
        }

        private void pgrVolume_ValueSlidTo(object sender, EventArgs e)
        {
            MusicPlayer.SetVolume(prgVolume.Value);
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            checkTime.Start();
            MusicPlayer.PlaySong();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            checkTime.Stop();
            MusicPlayer.PauseSong();
        }
    }
}
