using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;

namespace JamBit
{
    class MusicPlayer
    {
        private static WindowsMediaPlayer player;
        private static Timer playCheck;
        private static bool playing = false;
        public static Song curSong;
        public static JamBitForm parentForm;
        

        static MusicPlayer()
        {
            player = new WindowsMediaPlayer();
            player.PlayStateChange += PlayStateChanged;

            playCheck = new Timer();
            playCheck.Interval = 150;
            playCheck.Tick += playCheck_Tick;
        }


        public static void OpenSong(string fileName) {
            // Attempt to find filename in library and play that

            // Otherwise create new song and add it to library

            // For now just open the song
            OpenSong(new Song(fileName));
        }
        public static void OpenSong(Song song)
        {
            curSong = song;
            player.URL = song.FileName;

            playCheck.Start();
        }

        public static void CloseSong()
        {
            PauseSong();
            curSong = null;
            player.URL = null;
        }

        public static void PlaySong()
        {
            if (curSong != null)
            {
                player.controls.play();
                playing = true;
            }
        }

        public static void PauseSong()
        {
            player.controls.pause();
            playing = false;
        }

        public static bool CurrentlyPlaying()
        {
            return player.playState == WMPPlayState.wmppsPlaying;
        }

        public static void SeekTo(double seconds)
        {
            player.controls.currentPosition = seconds;
        }

        public static double CurrentTime()
        {
            return player.controls.currentPosition;
        }

        public static void SetVolume(int volume)
        {
            player.settings.volume = volume;
        }

        private static void PlayStateChanged(int newState)
        {
            switch((WMPPlayState)newState)
            {
                case WMPPlayState.wmppsStopped:
                case WMPPlayState.wmppsPaused:
                    parentForm.PauseTimeCheck();
                    break;
                case WMPPlayState.wmppsPlaying:
                    parentForm.StartTimeCheck();
                    break;
                case WMPPlayState.wmppsMediaEnded:
                    parentForm.SongEnded();
                    break;
            }
        }

        private static void playCheck_Tick(object sender, EventArgs e)
        {
            playCheck.Stop();
            if (playing)
                player.controls.play();
            else
                player.controls.pause();
        }
    }
}
