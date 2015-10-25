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

            libraryFolderDialog = new FolderBrowserDialog();
            libraryFolderDialog.Description = "Select a folder to scan for music files to add to your library.";
            libraryFolderDialog.RootFolder = Environment.SpecialFolder.MyComputer;
            libraryFolderDialog.ShowNewFolderButton = true;

            if (Properties.Settings.Default.LibraryFolders != null)
                foreach (string path in Properties.Settings.Default.LibraryFolders.Cast<string>())
                    lstLibraryFolders.Items.Add(path);
            else
                Properties.Settings.Default.LibraryFolders = new System.Collections.Specialized.StringCollection();
        }

        private void btnAddFolder_Click(object sender, EventArgs e)
        {
            if (libraryFolderDialog.ShowDialog() == DialogResult.OK)
            {
                lstLibraryFolders.Items.Add(libraryFolderDialog.SelectedPath);
                parent.LibraryScan(libraryFolderDialog.SelectedPath);
                Properties.Settings.Default.LibraryFolders.Add(libraryFolderDialog.SelectedPath);
                Properties.Settings.Default.Save();
            }
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            foreach (string folder in Properties.Settings.Default.LibraryFolders.Cast<string>())
                parent.LibraryScan(folder);
        }
    }
}
