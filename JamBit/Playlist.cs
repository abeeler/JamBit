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

        public void Initialize(SQLiteConnection db)
        {
            _songs.Clear();
            foreach (PlaylistItem pi in db.Table<PlaylistItem>().Where<PlaylistItem>(pi => pi.Playlist == ID))
                _songs.Add(db.Get<Song>(pi.Song));           
        }

        public void SaveToDatabase(SQLiteConnection db)
        {
            
        }
    }
}
