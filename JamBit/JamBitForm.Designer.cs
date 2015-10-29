namespace JamBit
{
    partial class JamBitForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblCurrentTime = new System.Windows.Forms.Label();
            this.lblSongLength = new System.Windows.Forms.Label();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.lstPlaylist = new System.Windows.Forms.ListView();
            this.clmTitle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmArtist = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmAlbum = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.clmPlayCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnPrevious = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.preferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPrefLibFolders = new System.Windows.Forms.ToolStripMenuItem();
            this.playlistsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearCurrentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnPlayMode = new System.Windows.Forms.Button();
            this.treeLibrary = new MusicPlayerControlsLibrary.MultipleSelectTreeView();
            this.lblSongInformation = new MusicPlayerControlsLibrary.MarqueeLabel();
            this.prgVolume = new MusicPlayerControlsLibrary.SlidableProgressBar();
            this.prgSongTime = new MusicPlayerControlsLibrary.SlidableProgressBar();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblCurrentTime
            // 
            this.lblCurrentTime.AutoSize = true;
            this.lblCurrentTime.Location = new System.Drawing.Point(12, 44);
            this.lblCurrentTime.Name = "lblCurrentTime";
            this.lblCurrentTime.Size = new System.Drawing.Size(28, 13);
            this.lblCurrentTime.TabIndex = 5;
            this.lblCurrentTime.Text = "0:00";
            this.lblCurrentTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSongLength
            // 
            this.lblSongLength.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSongLength.Location = new System.Drawing.Point(379, 44);
            this.lblSongLength.Name = "lblSongLength";
            this.lblSongLength.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblSongLength.Size = new System.Drawing.Size(34, 13);
            this.lblSongLength.TabIndex = 6;
            this.lblSongLength.Text = "0:00";
            this.lblSongLength.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(12, 68);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(75, 23);
            this.btnPlay.TabIndex = 7;
            this.btnPlay.Text = "Play";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(104, 68);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(75, 23);
            this.btnPause.TabIndex = 8;
            this.btnPause.Text = "Pause";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(197, 68);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 10;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // lstPlaylist
            // 
            this.lstPlaylist.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.lstPlaylist.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmTitle,
            this.clmArtist,
            this.clmAlbum,
            this.clmPlayCount});
            this.lstPlaylist.FullRowSelect = true;
            this.lstPlaylist.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstPlaylist.HideSelection = false;
            this.lstPlaylist.Location = new System.Drawing.Point(12, 126);
            this.lstPlaylist.MultiSelect = false;
            this.lstPlaylist.Name = "lstPlaylist";
            this.lstPlaylist.Size = new System.Drawing.Size(401, 254);
            this.lstPlaylist.TabIndex = 11;
            this.lstPlaylist.UseCompatibleStateImageBehavior = false;
            this.lstPlaylist.View = System.Windows.Forms.View.Details;
            this.lstPlaylist.DoubleClick += new System.EventHandler(this.lstPlaylist_DoubleClick);
            // 
            // clmTitle
            // 
            this.clmTitle.Text = "Title";
            this.clmTitle.Width = 122;
            // 
            // clmArtist
            // 
            this.clmArtist.Text = "Artist";
            this.clmArtist.Width = 73;
            // 
            // clmAlbum
            // 
            this.clmAlbum.Text = "Album";
            this.clmAlbum.Width = 93;
            // 
            // clmPlayCount
            // 
            this.clmPlayCount.Text = "Play Count";
            this.clmPlayCount.Width = 201;
            // 
            // btnPrevious
            // 
            this.btnPrevious.Location = new System.Drawing.Point(15, 97);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(75, 23);
            this.btnPrevious.TabIndex = 12;
            this.btnPrevious.Text = "Previous";
            this.btnPrevious.UseVisualStyleBackColor = true;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(104, 97);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 23);
            this.btnNext.TabIndex = 13;
            this.btnNext.Text = "Next";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.preferencesToolStripMenuItem,
            this.playlistsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(773, 24);
            this.menuStrip1.TabIndex = 14;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFileOpen});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // mnuFileOpen
            // 
            this.mnuFileOpen.Name = "mnuFileOpen";
            this.mnuFileOpen.Size = new System.Drawing.Size(103, 22);
            this.mnuFileOpen.Text = "Open";
            this.mnuFileOpen.Click += new System.EventHandler(this.mnuFileOpen_Click);
            // 
            // preferencesToolStripMenuItem
            // 
            this.preferencesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuPrefLibFolders});
            this.preferencesToolStripMenuItem.Name = "preferencesToolStripMenuItem";
            this.preferencesToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.preferencesToolStripMenuItem.Text = "Preferences";
            // 
            // mnuPrefLibFolders
            // 
            this.mnuPrefLibFolders.Name = "mnuPrefLibFolders";
            this.mnuPrefLibFolders.Size = new System.Drawing.Size(151, 22);
            this.mnuPrefLibFolders.Text = "Library Folders";
            this.mnuPrefLibFolders.Click += new System.EventHandler(this.mnuPrefLibFolders_Click);
            // 
            // playlistsToolStripMenuItem
            // 
            this.playlistsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearCurrentToolStripMenuItem,
            this.addNewToolStripMenuItem});
            this.playlistsToolStripMenuItem.Name = "playlistsToolStripMenuItem";
            this.playlistsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.playlistsToolStripMenuItem.Text = "Playlists";
            // 
            // clearCurrentToolStripMenuItem
            // 
            this.clearCurrentToolStripMenuItem.Name = "clearCurrentToolStripMenuItem";
            this.clearCurrentToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.clearCurrentToolStripMenuItem.Text = "Clear Current";
            this.clearCurrentToolStripMenuItem.Click += new System.EventHandler(this.clearCurrentToolStripMenuItem_Click);
            // 
            // addNewToolStripMenuItem
            // 
            this.addNewToolStripMenuItem.Name = "addNewToolStripMenuItem";
            this.addNewToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.addNewToolStripMenuItem.Text = "Add New";
            // 
            // btnPlayMode
            // 
            this.btnPlayMode.Location = new System.Drawing.Point(197, 97);
            this.btnPlayMode.Name = "btnPlayMode";
            this.btnPlayMode.Size = new System.Drawing.Size(75, 23);
            this.btnPlayMode.TabIndex = 16;
            this.btnPlayMode.Text = "Loop";
            this.btnPlayMode.UseVisualStyleBackColor = true;
            this.btnPlayMode.Click += new System.EventHandler(this.btnPlayMode_Click);
            // 
            // treeLibrary
            // 
            this.treeLibrary.HideSelection = false;
            this.treeLibrary.LastSelectedNode = null;
            this.treeLibrary.Location = new System.Drawing.Point(419, 39);
            this.treeLibrary.Name = "treeLibrary";
            this.treeLibrary.Size = new System.Drawing.Size(342, 361);
            this.treeLibrary.TabIndex = 17;
            // 
            // lblSongInformation
            // 
            this.lblSongInformation.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblSongInformation.BackColor = System.Drawing.Color.Transparent;
            this.lblSongInformation.CycleText = new string[] {
        " "};
            this.lblSongInformation.LabelSpeed = 30;
            this.lblSongInformation.Location = new System.Drawing.Point(12, 24);
            this.lblSongInformation.Margin = new System.Windows.Forms.Padding(0);
            this.lblSongInformation.Name = "lblSongInformation";
            this.lblSongInformation.PauseLength = 2500;
            this.lblSongInformation.Size = new System.Drawing.Size(749, 12);
            this.lblSongInformation.TabIndex = 9;
            // 
            // prgVolume
            // 
            this.prgVolume.Location = new System.Drawing.Point(12, 386);
            this.prgVolume.Name = "prgVolume";
            this.prgVolume.Size = new System.Drawing.Size(401, 14);
            this.prgVolume.Step = 0;
            this.prgVolume.TabIndex = 4;
            this.prgVolume.Value = 50;
            this.prgVolume.ValueSlidTo += new System.EventHandler(this.pgrVolume_ValueSlidTo);
            // 
            // prgSongTime
            // 
            this.prgSongTime.Location = new System.Drawing.Point(52, 39);
            this.prgSongTime.Maximum = 1000;
            this.prgSongTime.Name = "prgSongTime";
            this.prgSongTime.Size = new System.Drawing.Size(321, 23);
            this.prgSongTime.Step = 0;
            this.prgSongTime.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.prgSongTime.TabIndex = 3;
            this.prgSongTime.ValueSelected += new System.EventHandler(this.prgSongTime_SelectedValue);
            // 
            // JamBitForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(773, 412);
            this.Controls.Add(this.treeLibrary);
            this.Controls.Add(this.btnPlayMode);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnPrevious);
            this.Controls.Add(this.lstPlaylist);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.lblSongInformation);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.lblSongLength);
            this.Controls.Add(this.lblCurrentTime);
            this.Controls.Add(this.prgVolume);
            this.Controls.Add(this.prgSongTime);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "JamBitForm";
            this.Text = "JamBit";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MusicPlayerControlsLibrary.SlidableProgressBar prgSongTime;
        private MusicPlayerControlsLibrary.SlidableProgressBar prgVolume;
        private System.Windows.Forms.Label lblCurrentTime;
        private System.Windows.Forms.Label lblSongLength;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnPause;
        private MusicPlayerControlsLibrary.MarqueeLabel lblSongInformation;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.ListView lstPlaylist;
        private System.Windows.Forms.ColumnHeader clmTitle;
        private System.Windows.Forms.ColumnHeader clmArtist;
        private System.Windows.Forms.ColumnHeader clmAlbum;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuFileOpen;
        private System.Windows.Forms.ToolStripMenuItem preferencesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuPrefLibFolders;
        private System.Windows.Forms.Button btnPlayMode;
        private MusicPlayerControlsLibrary.MultipleSelectTreeView treeLibrary;
        private System.Windows.Forms.ColumnHeader clmPlayCount;
        private System.Windows.Forms.ToolStripMenuItem playlistsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearCurrentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNewToolStripMenuItem;
    }
}

