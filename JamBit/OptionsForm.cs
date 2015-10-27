using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JamBit
{
    public partial class OptionsForm : Form
    {
        private FolderBrowserDialog libraryFolderDialog;
        private JamBitForm parent;

        public OptionsForm(JamBitForm parent)
        {
            InitializeComponent();

            this.parent = parent;

            // Initailze folder browser dialog to add new folders as library folders
            libraryFolderDialog = new FolderBrowserDialog();
            libraryFolderDialog.Description = "Select a folder to scan for music files to add to your library.";
            libraryFolderDialog.RootFolder = Environment.SpecialFolder.MyComputer;
            libraryFolderDialog.ShowNewFolderButton = true;

            // If there are saved library folders, populate the list with them
            if (Properties.Settings.Default.LibraryFolders != null)
                foreach (string path in Properties.Settings.Default.LibraryFolders.Cast<string>())
                    lstLibraryFolders.Items.Add(path);
            else
                Properties.Settings.Default.LibraryFolders = new System.Collections.Specialized.StringCollection();
        }

        #region Control Event Methods

        private void btnAddFolder_Click(object sender, EventArgs e)
        {
            // If the user has selected a folder not already added
            if (libraryFolderDialog.ShowDialog() == DialogResult.OK && !Properties.Settings.Default.LibraryFolders.Contains(libraryFolderDialog.SelectedPath))
            {
                // Add it to the saved folders and scan it immediately
                lstLibraryFolders.Items.Add(libraryFolderDialog.SelectedPath);
                parent.LibraryScan(libraryFolderDialog.SelectedPath);
                Properties.Settings.Default.LibraryFolders.Add(libraryFolderDialog.SelectedPath);
                Properties.Settings.Default.Save();
            }
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            // Begin scanning library folders if at least one is saved
            if (Properties.Settings.Default.LibraryFolders != null)
                foreach (string folder in Properties.Settings.Default.LibraryFolders.Cast<string>())
                    parent.LibraryScan(folder);
        }

        #endregion
    }
}
