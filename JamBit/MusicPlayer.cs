using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JamBit
{
    class MusicPlayer
    {
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string lpstrCommand, StringBuilder lpstrReturnString, int uReturnLength, int hwndCallback);
        private static int returnVal;
        private static StringBuilder returnData = new StringBuilder();

        private static int currentVolume = 500;
        public static Song curSong;

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
            mciSendString("close curSong", null, 0, 0);
            mciSendString("open \"" + song.FileName + "\" type MPEGVideo alias curSong", null, 0, 0);
            mciSendString("setaudio curSong volume to " + currentVolume, null, 0, 0);

            if (willPlay) PlaySong();
        }

        public static void PlaySong()
        {
            mciSendString("play curSong", null, 0, 0);
        }

        public static void PauseSong()
        {
            mciSendString("stop curSong", null, 0, 0);
        }

        public static bool CurrentlyPlaying()
        {
            mciSendString("status curSong mode", returnData, returnData.Capacity, 0);
            return returnData.ToString().Substring(0, 7) == "playing";
        }

        public static void SeekTo(int seconds)
        {
            if (CurrentlyPlaying())
                mciSendString("play curSong from " + (seconds * 1000), null, 0, 0);
            else
                mciSendString("seek curSong to " + (seconds * 1000), null, 0, 0);
        }

        public static long SongLength()
        {
            if (curSong == null)
                return 0;

            mciSendString("status curSong length", returnData, returnData.Capacity, 0);
            return long.Parse(returnData.ToString());
        }

        public static String CurrentTime()
        {
            if (curSong == null)
                return "0:00";

            mciSendString("status curSong position", returnData, returnData.Capacity, 0);
            return returnData.ToString();
        }

        public static void SetVolume(int volume)
        {
            if (volume < 0)
                volume = 0;
            if (volume > 1000)
                volume = 1000;
            currentVolume = volume;
            mciSendString("setaudio curSong volume to " + volume, null, 0, 0);
        }
    }
}
