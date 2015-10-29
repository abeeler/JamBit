using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamBit
{
    class Playlist
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
        public void SaveToDatabase(SQLiteConnection db)
        {
            List<int> songsSaved = new List<int>();
            try
            {
                songsSaved = db.Table<PlaylistItem>().Where(pi => pi.PlaylistID == ID).ToList<PlaylistItem>().Select<PlaylistItem, int>(pi => pi.SongID) as List<int>;
            } catch (Exception) { }

            if (songsSaved == null) songsSaved = new List<int>();

            foreach (int id in _songs.Except(songsSaved))
                db.Insert(new PlaylistItem(ID, id));

            foreach (int id in songsSaved.Except(_songs))
                try { db.Delete(db.Get<PlaylistItem>(pi => pi.PlaylistID == ID && pi.SongID == id)); }
                catch (Exception) { }
        }
    }
}
