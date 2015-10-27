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

        private List<Song> _songs = new List<Song>();
        [Ignore]
        public List<Song> Songs { get { return _songs; } }

        [Ignore]
        public int Count { get { return _songs.Count; } }

        /// <summary>
        /// Initialize this playlist using the given database connection. 
        /// Attempts to fill the list of songs using information in the database
        /// </summary>
        /// <param name="db">The database to use for intialization</param>
        public void Initialize(SQLiteConnection db)
        {
            _songs.Clear();
            try
            {
                foreach (PlaylistItem pi in db.Table<PlaylistItem>().Where<PlaylistItem>(pi => pi.PlaylistID == ID))
                    _songs.Add(db.Get<Song>(pi.SongID));
            }
            catch (InvalidOperationException) { }         
        }

        /// <summary>
        /// Save the current state of the playlist back to the database
        /// </summary>
        /// <param name="db">The database to save to</param>
        public void SaveToDatabase(SQLiteConnection db)
        {
            
        }
    }
}
