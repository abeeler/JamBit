using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JamBit
{
    public class LibraryNode : TreeNode
    {
        public enum LibraryNodeType { None, Playable, Playlist, Playlists }

        public LibraryNodeType LibraryType { get; private set; }
        public object DatabaseKey { get; set; }

        public LibraryNode(LibraryNodeType type, string text, object key = null)
        {
            LibraryType = type;
            this.Text = text;
            DatabaseKey = key;
        }
    }
}
