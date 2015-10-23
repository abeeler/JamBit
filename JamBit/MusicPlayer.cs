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
        private static int currentVolume = 500;
        public static Song curSong;

        static MusicPlayer()
        {
            player = new WindowsMediaPlayer();

        }

        public static void OpenSong(string fileName) {
            // Attempt to find filename in library and play that

            // Otherwise create new song and add it to library

            // For now just open the song
            OpenSong(new Song(fileName));
        }
        public static void OpenSong(Song song)
        {
            bool willPlay = curSong != null && CurrentlyPlaying();
            curSong = song;
            player.URL = song.FileName;
            if (willPlay) player.controls.play();
        }

        public static void PlaySong()
        {
            player.controls.play();
        }

        public static void PauseSong()
        {
            player.controls.pause();
        }

        public static bool CurrentlyPlaying()
        {
            return player.playState == WMPPlayState.wmppsPlaying;
        }

        public static void SeekTo(int seconds)
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
    }
}
