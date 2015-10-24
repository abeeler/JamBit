﻿using System;
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

            if (Properties.Settings.Default.LIbraryFolders.Length > 0)
                foreach (string path in Properties.Settings.Default.LIbraryFolders.Split(';'))
                    lstLibraryFolders.Items.Add(path);
        }

        private void btnAddFolder_Click(object sender, EventArgs e)
        {
            if (libraryFolderDialog.ShowDialog() == DialogResult.OK)
            {
                lstLibraryFolders.Items.Add(libraryFolderDialog.SelectedPath);
                parent.StartScan(libraryFolderDialog.SelectedPath);
                if (Properties.Settings.Default.LIbraryFolders.Length > 0)
                    Properties.Settings.Default.LIbraryFolders += ";";
                Properties.Settings.Default.LIbraryFolders += libraryFolderDialog.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }
    }
}
