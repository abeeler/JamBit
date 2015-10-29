using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JamBit
{
    class LibraryNode : TreeNode
    {
        public enum LibraryNodeType { None, Library, RecentlyPlayed, Playlist, Artist, Album, Song }

        public LibraryNodeType LibraryType { get; private set; }
        public object DatabaseKey { get; set; }

        public LibraryNode(LibraryNodeType type)
        {
            LibraryType = type;
        }
    }
}
