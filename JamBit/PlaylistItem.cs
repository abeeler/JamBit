using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JamBit
{
    class PlaylistItem
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public int PlaylistID { get; set; }

        public int SongID { get; set; }

        public PlaylistItem() { }

        public PlaylistItem(int playlistID, int songID)
        {
            this.PlaylistID = playlistID;
            this.SongID = songID;
        }
    }
}
