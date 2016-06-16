using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JamBit
{
    public partial class JamBitForm
    {
        #region Playback Control Event Methods

        private void btnPlay_Click(object sender, EventArgs e)
        {
            // Play the current song
            PlaySong();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            // Pause the current song
            PauseSong();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            ShowOpenFileDialog();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            // Prematurely end the song
            ChangeSong();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            // Prematurely end the song with the flag for previous
            ChangeSong(true);
        }

        private void btnPlayMode_Click(object sender, EventArgs e)
        {
            // If at the last playmode, restart at the beginning
            if (playMode == RepeatMode.None)
                playMode = RepeatMode.Loop;

            // Otherwise just go to the next playmode
            else
                playMode++;

            SetPlayMode(playMode);
        }

        private void prgSongTime_SelectedValue(object sender, EventArgs e)
        {
            // Update the current song to the new position chosen by the user
            MusicPlayer.SeekTo((((double)prgSongTime.Value / 1000 * MusicPlayer.curSong.Length)));

            // Update the current time label to the new position
            int seconds = (int)MusicPlayer.CurrentTime();
            lblCurrentTime.Text = String.Format("{0}:{1:D2}", seconds / 60, seconds % 60);
        }

        private void pgrVolume_ValueSlidTo(object sender, EventArgs e)
        {
            // Change the volume on the music player to the new volume
            MusicPlayer.SetVolume(prgVolume.Value);

            // Update the user setting with the new volume
            Properties.Settings.Default.PreferredVolume = (byte)prgVolume.Value;
            Properties.Settings.Default.Save();
        }

        #endregion

        #region Programmed Control Event Methods

        private void checkTime_Tick(object sender, EventArgs e)
        {
            // Get the current position of the song in seconds
            int seconds = (int)MusicPlayer.CurrentTime();

            // Update the label using an appropriate format
            lblCurrentTime.Text = String.Format("{0}:{1:D2}", seconds / 60, seconds % 60);

            // Update the progress bar to the current position
            prgSongTime.SetValue((int)((double)seconds / MusicPlayer.curSong.Length * prgSongTime.Maximum));
        }

        private void playlistScrollTimer_Tick(object sender, EventArgs e)
        {
            if (playlistScrollDirection == 1 && lstPlaylist.TopItem.Index + playlistItemsViewable >= lstPlaylist.Items.Count)
                return;
            if (playlistScrollDirection == -1 && lstPlaylist.TopItem.Index == 0)
                return;

            lstPlaylist.TopItem = lstPlaylist.Items[lstPlaylist.TopItem.Index + playlistScrollDirection];
        }

        private void openFileDialog_OnFileOk(object sender, CancelEventArgs e)
        {
            // For each song opened, attempt to add it to the library
            foreach (string fileName in openFileDialog.FileNames)
                AddSongToLibrary(new Song(fileName));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Safely remove database connection from memory
            if (dbOperationsActive > 0)
                e.Cancel = true;
            else
                db.Dispose();
        }

        private void libraryScanner_DoWork(object sender, DoWorkEventArgs e)
        {
            // The folder currently being scanned
            string folder;

            // Repeat until there are no folders left to scan
            while (foldersToScan.Count > 0)
            {
                Song s = new Song();
                if (foldersToScan.TryDequeue(out folder))
                    // For each file in the current folder
                    foreach (string file in Directory.GetFiles(folder, "*.mp3", SearchOption.AllDirectories))
                    {
                        try
                        {
                            s = new Song(file);
                            Song inLibrary = db.Find<Song>(dbSong => dbSong.Title == s.Title && dbSong.Artist == s.Artist);

                            if (inLibrary == null)
                            {
                                db.Insert(s);

                                List<TreeNode[]> nodesToAdd = new List<TreeNode[]>();

                                LibraryNode artistNode, albumNode;
                                if (!GetNode(treeLibrary.Nodes[0], tn => tn.Text.ToLower() == s.Artist.ToLower(), out artistNode))
                                {
                                    artistNode = new LibraryNode(LibraryNode.LibraryNodeType.Playable, s.Artist);
                                    nodesToAdd.Add(new TreeNode[] { treeLibrary.Nodes[0], artistNode });
                                }

                                if (!GetNode(artistNode, tn => tn.Text.ToLower() == s.Album.ToLower(), out albumNode))
                                {
                                    albumNode = new LibraryNode(LibraryNode.LibraryNodeType.Playable, s.Album);
                                    nodesToAdd.Add(new TreeNode[] { artistNode, albumNode });
                                }

                                nodesToAdd.Add(new TreeNode[] { albumNode, new LibraryNode(s) });

                                libraryScanner.ReportProgress(0, nodesToAdd.ToArray());
                            }
                            else
                            {
                                if (inLibrary.FileName != s.FileName)
                                {
                                    inLibrary.FileName = s.FileName;
                                    db.Update(inLibrary);
                                }
                            }
                        }

                        // If the filename is too long
                        catch (System.IO.PathTooLongException) { }
                        if (s.Data != null)
                            s.Data.Dispose();
                    }
            }
        }

        private void libraryScanner_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            TreeNode[][] nodesToAdd = e.UserState as TreeNode[][];
            foreach (TreeNode[] nodes in nodesToAdd)
            {
                nodes[0].Nodes.Add(nodes[1]);
            }
        }

        private void libraryScanner_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private void playlistDeleter_DoWork(object sender, DoWorkEventArgs e)
        {
            int id;
            while (playlistsToDelete.Count > 0)
            {
                if (!playlistsToDelete.TryDequeue(out id)) continue;
                db.BeginTransaction();
                Playlist toDelete = db.Get<Playlist>(id);

                foreach (PlaylistItem pi in db.Table<PlaylistItem>().Where(obj => obj.PlaylistID == id).ToList())
                    db.Delete(pi);
                db.Delete(db.Get<Playlist>(id));
                db.Commit();
            }
        }

        #endregion

        #region Playlist ListView

        private void lstPlaylist_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                currentPlaylistOptions.Show(sender as Control, e.Location);
        }

        private void lstPlaylist_DoubleClick(object sender, EventArgs e)
        {
            // If there is a selection in the list of songs in the playlist
            if (lstPlaylist.SelectedIndices.Count == 1 && playlistIndex != lstPlaylist.SelectedIndices[0])
            {
                // Open the selected song
                playlistIndex = lstPlaylist.SelectedIndices[0];
                OpenSong();
            }
        }

        private void lstPlaylist_DragDrop(object sender, DragEventArgs e)
        {
            playlistScrollTimer.Stop();
            if (lstPlaylist.SelectedItems.Count == 0) return;

            //Obtain the item that is located at the specified location of the mouse pointer.
            Point listPoint = lstPlaylist.PointToClient(new Point(e.X, e.Y));
            ListViewItem dragToItem = lstPlaylist.GetItemAt(listPoint.X, listPoint.Y);
            if (dragToItem == null) return;

            List<int> newIndexes = new List<int>();
            for (int i = 0; i < currentPlaylist.Songs.Count; i++)
                newIndexes.Add(i);

            bool before = dragToItem.Index < lstPlaylist.SelectedItems[0].Index;

            int placementIndex = dragToItem.Index;
            foreach (ListViewItem item in lstPlaylist.SelectedItems)
            {
                if (item.Index == dragToItem.Index) continue;
                newIndexes.Remove(item.Index);
                newIndexes.Insert(placementIndex, item.Index);
                ListViewItem toInsert = item.Clone() as ListViewItem;
                toInsert.Selected = true;
                lstPlaylist.Items.Insert(placementIndex, toInsert);
                lstPlaylist.Items.Remove(item);

                if (before) placementIndex++;
            }

            List<int> newIDs = new List<int>();
            List<PlaylistItem> toUpdate = new List<PlaylistItem>();
            for (int i = 0; i < newIndexes.Count; i++)
            {
                newIDs.Add(db.Table<PlaylistItem>().First<PlaylistItem>(pi => pi.PlaylistID == currentPlaylist.ID && pi.SongID == currentPlaylist.Songs[i]).ID);
                toUpdate.Add(db.Table<PlaylistItem>().First<PlaylistItem>(pi => pi.PlaylistID == currentPlaylist.ID && pi.SongID == currentPlaylist.Songs[newIndexes[i]]));
            }

            db.BeginTransaction();

            for (int i = 0; i < newIndexes.Count; i++)
            {
                if (newIndexes[i] == i) continue;
                toUpdate[i].ID = newIDs[i];
                db.Update(toUpdate[i]);
            }

            db.Commit();

            currentPlaylist = db.Get<Playlist>(currentPlaylist.ID);
            foreach (PlaylistItem pi in db.Table<PlaylistItem>().Where<PlaylistItem>(pi => pi.PlaylistID == currentPlaylist.ID))
                currentPlaylist.Songs.Add(pi.SongID);

            playlistIndex = newIndexes.IndexOf(playlistIndex);
        }

        private void lstPlaylist_ItemDrag(object sender, ItemDragEventArgs e)
        {
            lstPlaylist.DoDragDrop(lstPlaylist.SelectedItems, DragDropEffects.Move);
        }

        private void lstPlaylist_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void lstPlaylist_DragOver(object sender, DragEventArgs e)
        {
            Point position = lstPlaylist.PointToClient(new Point(e.X, e.Y));

            if (position.Y <= (lstPlaylist.Font.Height * 2))
            {
                // getting close to top, ensure previous item is visible
                playlistScrollDirection = -1;
                playlistScrollTimer.Start();
            }
            else if (position.Y >= lstPlaylist.ClientSize.Height - lstPlaylist.Font.Height / 2)
            {
                // getting close to bottom, ensure next item is visible
                playlistScrollDirection = 1;
                playlistScrollTimer.Start();
            }
            else
                playlistScrollTimer.Stop();
        }

        #endregion

        #region Menu Event Methods

        private void mnuFileOpen_Click(object sender, EventArgs e) { ShowOpenFileDialog(); }

        private void mnuFilePreferences_Click(object sender, EventArgs e) { OpenPreferencesForm(); }

        private void mnuFileClose_Click(object sender, EventArgs e) { CloseForm(); }

        private void mnuPlaybackPlay_Click(object sender, EventArgs e) { PlaySong(); }

        private void mnuPlaybackPause_Click(object sender, EventArgs e) { PauseSong(); }

        private void mnuPlayModeLoop_Click(object sender, EventArgs e) { SetPlayMode(RepeatMode.Loop); }

        private void mnuPlayModeRepeat_Click(object sender, EventArgs e) { SetPlayMode(RepeatMode.Repeat); }

        private void mnuPlayModeShuffle_Click(object sender, EventArgs e) { SetPlayMode(RepeatMode.Shuffle); }

        private void mnuPlayModeNone_Click(object sender, EventArgs e) { SetPlayMode(RepeatMode.None); }

        private void mnuPlaylistNext_Click(object sender, EventArgs e) { ChangeSong(); }

        private void mnuPlaylistPrevious_Click(object sender, EventArgs e) { ChangeSong(true); }

        private void mnuPlaylistCreate_Click(object sender, EventArgs e) { CreatePlaylist(); }

        private void mnuPlaylistSave_Click(object sender, EventArgs e) { if (currentPlaylist.ID > 1) SavePlaylist(); else SaveAsNewPlaylist(); }

        private void mnuPlaylistSaveAs_Click(object sender, EventArgs e) { SaveAsNewPlaylist(); }

        private void mnuPlaylistClear_Click(object sender, EventArgs e) { LoadDefaultPlaylist(); }

        private void mnuPlaylistDelete_Click(object sender, EventArgs e) { DeletePlaylist(); }

        private void mnuPlaylistRename_Click(object sender, EventArgs e) { RenamePlaylist(); }

        #endregion

        #region Context Menu Event Methods

        private void libraryOptions_ItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == playableOptions.Items[0])
            {
                foreach (TreeNode node in treeLibrary.SelectedNodes)
                    AddPlayableNodeSongs(node as LibraryNode);
                SavePlaylist();
            }
        }

        private void currentPlaylistOptions_ItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            // Iterate from the last selection to the first to prevent changes in indices during removal
            if (e.ClickedItem == currentPlaylistOptions.Items[0])
            {
                lstPlaylist.BeginUpdate();

                for (int i = lstPlaylist.SelectedIndices.Count - 1; i >= 0; i--)
                    RemoveSongFromPlaylist(lstPlaylist.SelectedIndices[i], false);
                lstPlaylist.EndUpdate();

                SavePlaylist();
            }
        }

        private void playlistOptions_ItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == playlistOptions.Items[0])
                LoadPlaylist((int)playlistRightClicked.DatabaseKey);
            else if (e.ClickedItem == playlistOptions.Items[1])
                RenamePlaylist((int)playlistRightClicked.DatabaseKey);
            else
                DeletePlaylist((int)playlistRightClicked.DatabaseKey);
        }

        private void playlistsOptions_ItemClick(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == playlistsOptions.Items[0])
                CreatePlaylist();
        }

        #endregion

        #region Library TreeView

        private void treeLibrary_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                LibraryNode clicked = treeLibrary.GetNodeAt(e.Location) as LibraryNode;
                switch (clicked.LibraryType)
                {
                    case LibraryNode.LibraryNodeType.Playlists:
                        playlistsOptions.Show(sender as Control, e.Location);
                        break;
                    case LibraryNode.LibraryNodeType.Playlist:
                        playlistRightClicked = clicked;
                        playlistOptions.Show(sender as Control, e.Location);
                        break;
                    default:
                        playableOptions.Show(sender as Control, e.Location);
                        break;
                }
            }
        }

        private void treeLibrary_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (treeLibrary.HitTest(e.Location).Location == TreeViewHitTestLocations.PlusMinus)
                return;

            LibraryNode node = e.Node as LibraryNode;
            switch (node.LibraryType)
            {
                case LibraryNode.LibraryNodeType.Playable:
                    AddPlayableNodeSongs(node);
                    SavePlaylist();
                    break;
                case LibraryNode.LibraryNodeType.Playlist:
                    LoadPlaylist((int)node.DatabaseKey);
                    break;
            }
        }

        private void treeLibrary_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = preventExpand;
            preventExpand = false;
        }

        private void treeLibrary_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = preventExpand;
            preventExpand = false;
        }

        private void treeLibrary_MouseDown(object sender, MouseEventArgs e)
        {
            if (treeLibrary.HitTest(e.Location).Location != TreeViewHitTestLocations.PlusMinus)
            {
                preventExpand = (int)DateTime.Now.Subtract(lastMouseDown).TotalMilliseconds < SystemInformation.DoubleClickTime;
                lastMouseDown = DateTime.Now;
            }
        }

        #endregion
    }
}
