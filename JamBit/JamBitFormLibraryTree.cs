using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JamBit
{
    public partial class JamBitForm
    {
        #region Library Tree View Methods

        /// <summary>
        /// Reset the tree view and populate it with the songs in the library, the recently played songs, and the user-created playlists
        /// </summary>
        private void PopulateLibraryTree()
        {
            // Clear the tree view initially
            treeLibrary.Nodes.Clear();

            treeLibrary.BeginUpdate();

            //
            // Load the Library node, holding every song in the library
            //
            LibraryNode libraryNode = new LibraryNode(LibraryNode.LibraryNodeType.Playable, "Library");
            treeLibrary.Nodes.Add(libraryNode);

            // Initialize the nodes to be used 
            LibraryNode artistNode = null, albumNode = null;

            foreach (Song s in db.Table<Song>())
            {
                // If this is the first song or the last song doesn't have the same artist, find the artist node in the tree or make one
                if (artistNode == null || artistNode.Text != s.Artist)
                    artistNode = GetOrMakeNode(libraryNode, s.Artist, tn => tn.Text.ToLower() == s.Artist.ToLower());

                // If this is the first song or the last song doesn't have the same album, find the artist node in the tree or make one
                if (albumNode == null || albumNode.Text != s.Album)
                    albumNode = GetOrMakeNode(artistNode, s.Album, tn => tn.Text.ToLower() == s.Album.ToLower());

                // If the filename of the song exists, insert the song based on track number
                if (File.Exists(s.FileName))
                    albumNode.Nodes.Insert((int)s.Data.Tag.Track, new LibraryNode(s));
                else
                    albumNode.Nodes.Add(new LibraryNode(s));
            }

            //
            // Load the Recently Played node, holding a list of the last songs played
            //
            LibraryNode recentNode = new LibraryNode(LibraryNode.LibraryNodeType.Playable, "Recently Played");
            treeLibrary.Nodes.Add(recentNode);

            // Add a node to the tree for every song in the recent song list
            try
            {
                foreach (RecentSong rs in db.Table<RecentSong>())
                {
                    Song song = db.Get<Song>(rs.SongID);
                    recentNode.Nodes.Add(new LibraryNode(song));
                }
            }
            catch (SQLiteException) { }

            //
            // Load the Plyalists node, holding a node for each playlist the user has made
            //
            LibraryNode playlistsNode = new LibraryNode(LibraryNode.LibraryNodeType.Playlists, "Playlists");
            treeLibrary.Nodes.Add(playlistsNode);

            // Add a node to the tree for each playlist
            try
            {
                foreach (Playlist p in db.Table<Playlist>().Where(playlist => playlist.ID > 1))
                    playlistsNode.Nodes.Add(new LibraryNode(LibraryNode.LibraryNodeType.Playlist, p.Name, p.ID));
            }
            catch (Exception) { }

            treeLibrary.EndUpdate();
        }

        /// <summary>
        /// Searches the parent node for the first node that matches the given predicate
        /// </summary>
        /// <param name="parentNode">The node to search</param>
        /// <param name="predicate">The predicate to search with</param>
        /// <param name="node">The LibraryNode that will be sent back</param>
        /// <returns>Whether a node was found or not</returns>
        public bool GetNode(TreeNode parentNode, Func<TreeNode, bool> predicate, out LibraryNode node)
        {
            node = parentNode.Nodes.Cast<TreeNode>().Where(predicate).FirstOrDefault() as LibraryNode;
            return node != null;
        }

        /// <summary>
        /// Searches the parent node for the first node that matches the given predicate. 
        /// If no matches are found, creates a new node with the provided text
        /// </summary>
        /// <param name="parentNode">The node to search</param>
        /// <param name="textToUse">The text to use if a new node needs to made</param>
        /// <param name="predicate">The predicate to search with</param>
        /// <returns>The found or created node in the tree</returns>
        public LibraryNode GetOrMakeNode(TreeNode parentNode, string textToUse, Func<TreeNode, bool> predicate)
        {
            LibraryNode node;
            if (GetNode(parentNode, predicate, out node))
                return node;

            node = new LibraryNode(LibraryNode.LibraryNodeType.Playable, textToUse);
            parentNode.Nodes.Add(node);
            return node;
        }

        /// <summary>
        /// Attempts to find the node in the tree view that matches the given song
        /// </summary>
        /// <param name="s">The song to search for</param>
        /// <returns>The matching node or null</returns>
        public LibraryNode GetSongNode(Song s)
        {
            return treeLibrary.Nodes[0].Nodes.Cast<LibraryNode>().FirstOrDefault(artist => artist.Text == s.Artist).
                Nodes.Cast<LibraryNode>().FirstOrDefault(album => album.Text == s.Album).
                Nodes.Cast<LibraryNode>().FirstOrDefault(song => song.Text == s.Title);
        }

        #endregion
    }
}
