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

        public int Playlist { get; set; }

        public int Song { get; set; }
    }
}
