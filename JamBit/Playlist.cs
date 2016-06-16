using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JamBit
{
    public class Playlist
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [MaxLength(64)]
        public string Name { get; set; }

        private List<int> _songs = new List<int>();
        [Ignore]
        public List<int> Songs { get { return _songs; } }

        [Ignore]
        public int Count { get { return _songs.Count; } }

        public Playlist() { ID = 0; }

        /// <summary>
        /// Save the current state of the playlist back to the database
        /// </summary>
        /// <param name="db">The database to save to</param>
        public void SaveToDatabase(JamBitForm form)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += SaveToPlaylistBackgroundWork;
            bw.RunWorkerAsync(form);
        }

        private void SaveToPlaylistBackgroundWork(object sender, DoWorkEventArgs e)
        {
            JamBitForm form = e.Argument as JamBitForm;
            SQLiteConnection db = form.db;
            Interlocked.Increment(ref form.dbOperationsActive);

            List<int> songsSaved = new List<int>();

            foreach (PlaylistItem pi in db.Table<PlaylistItem>().Where(pi => pi.PlaylistID == ID))
                songsSaved.Add(pi.SongID);

            try {
                foreach (int id in songsSaved.Count > 0 ? _songs.Except(songsSaved) : _songs)
                    db.Insert(new PlaylistItem(ID, id));
            } catch (InvalidOperationException exc) { Console.WriteLine(exc.StackTrace); }

            try {
                foreach (int id in songsSaved.Except(_songs))
                {
                    PlaylistItem toDelete = db.Get<PlaylistItem>(pi => pi.PlaylistID == ID && pi.SongID == id);
                    db.Delete(toDelete);
                }
            } catch (InvalidOperationException) { }

            Interlocked.Decrement(ref form.dbOperationsActive);
        }
    }
}
