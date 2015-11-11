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
            this.mnuFilePreferences = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileClose = new System.Windows.Forms.ToolStripMenuItem();
            this.playbackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPlaybackPlay = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPlaybackPause = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPlaybackPlayMode = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPlayModeLoop = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPlayModeRepeat = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPlayModeShuffle = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPlayModeNone = new System.Windows.Forms.ToolStripMenuItem();
            this.playlistToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPlaylistNext = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPlaylistPrevious = new System.Windows.Forms.ToolStripMenuItem();
            this.gToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
            this.mnuPlaylistCreate = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPlaylistSave = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPlaylistSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPlaylistClear = new System.Windows.Forms.ToolStripMenuItem();
            this.btnPlayMode = new System.Windows.Forms.Button();
            this.treeLibrary = new MusicPlayerControlsLibrary.MultipleSelectTreeView();
            this.lblSongInformation = new MusicPlayerControlsLibrary.MarqueeLabel();
            this.prgVolume = new MusicPlayerControlsLibrary.SlidableProgressBar();
            this.prgSongTime = new MusicPlayerControlsLibrary.SlidableProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.lblPlaylistName = new System.Windows.Forms.Label();
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
            this.lstPlaylist.AllowDrop = true;
            this.lstPlaylist.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.clmTitle,
            this.clmArtist,
            this.clmAlbum,
            this.clmPlayCount});
            this.lstPlaylist.FullRowSelect = true;
            this.lstPlaylist.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstPlaylist.HideSelection = false;
            this.lstPlaylist.Location = new System.Drawing.Point(12, 126);
            this.lstPlaylist.Name = "lstPlaylist";
            this.lstPlaylist.Size = new System.Drawing.Size(401, 254);
            this.lstPlaylist.TabIndex = 11;
            this.lstPlaylist.UseCompatibleStateImageBehavior = false;
            this.lstPlaylist.View = System.Windows.Forms.View.Details;
            this.lstPlaylist.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.lstPlaylist_ItemDrag);
            this.lstPlaylist.DragDrop += new System.Windows.Forms.DragEventHandler(this.lstPlaylist_DragDrop);
            this.lstPlaylist.DragEnter += new System.Windows.Forms.DragEventHandler(this.lstPlaylist_DragEnter);
            this.lstPlaylist.DragOver += new System.Windows.Forms.DragEventHandler(this.lstPlaylist_DragOver);
            this.lstPlaylist.DoubleClick += new System.EventHandler(this.lstPlaylist_DoubleClick);
            this.lstPlaylist.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lstPlaylist_MouseClick);
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
            this.playbackToolStripMenuItem,
            this.playlistToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(773, 24);
            this.menuStrip1.TabIndex = 14;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFileOpen,
            this.mnuFilePreferences,
            this.mnuFileClose});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // mnuFileOpen
            // 
            this.mnuFileOpen.Name = "mnuFileOpen";
            this.mnuFileOpen.Size = new System.Drawing.Size(137, 22);
            this.mnuFileOpen.Text = "Open File(s)";
            this.mnuFileOpen.Click += new System.EventHandler(this.mnuFileOpen_Click);
            // 
            // mnuFilePreferences
            // 
            this.mnuFilePreferences.Name = "mnuFilePreferences";
            this.mnuFilePreferences.Size = new System.Drawing.Size(137, 22);
            this.mnuFilePreferences.Text = "Preferences";
            this.mnuFilePreferences.Click += new System.EventHandler(this.mnuFilePreferences_Click);
            // 
            // mnuFileClose
            // 
            this.mnuFileClose.Name = "mnuFileClose";
            this.mnuFileClose.Size = new System.Drawing.Size(137, 22);
            this.mnuFileClose.Text = "Close";
            this.mnuFileClose.Click += new System.EventHandler(this.mnuFileClose_Click);
            // 
            // playbackToolStripMenuItem
            // 
            this.playbackToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuPlaybackPlay,
            this.mnuPlaybackPause,
            this.mnuPlaybackPlayMode});
            this.playbackToolStripMenuItem.Name = "playbackToolStripMenuItem";
            this.playbackToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.playbackToolStripMenuItem.Text = "Playback";
            // 
            // mnuPlaybackPlay
            // 
            this.mnuPlaybackPlay.Name = "mnuPlaybackPlay";
            this.mnuPlaybackPlay.Size = new System.Drawing.Size(130, 22);
            this.mnuPlaybackPlay.Text = "Play";
            this.mnuPlaybackPlay.Click += new System.EventHandler(this.mnuPlaybackPlay_Click);
            // 
            // mnuPlaybackPause
            // 
            this.mnuPlaybackPause.Name = "mnuPlaybackPause";
            this.mnuPlaybackPause.Size = new System.Drawing.Size(130, 22);
            this.mnuPlaybackPause.Text = "Pause";
            this.mnuPlaybackPause.Click += new System.EventHandler(this.mnuPlaybackPause_Click);
            // 
            // mnuPlaybackPlayMode
            // 
            this.mnuPlaybackPlayMode.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuPlayModeLoop,
            this.mnuPlayModeRepeat,
            this.mnuPlayModeShuffle,
            this.mnuPlayModeNone});
            this.mnuPlaybackPlayMode.Name = "mnuPlaybackPlayMode";
            this.mnuPlaybackPlayMode.Size = new System.Drawing.Size(130, 22);
            this.mnuPlaybackPlayMode.Text = "Play Mode";
            // 
            // mnuPlayModeLoop
            // 
            this.mnuPlayModeLoop.Name = "mnuPlayModeLoop";
            this.mnuPlayModeLoop.Size = new System.Drawing.Size(111, 22);
            this.mnuPlayModeLoop.Text = "Loop";
            this.mnuPlayModeLoop.Click += new System.EventHandler(this.mnuPlayModeLoop_Click);
            // 
            // mnuPlayModeRepeat
            // 
            this.mnuPlayModeRepeat.Name = "mnuPlayModeRepeat";
            this.mnuPlayModeRepeat.Size = new System.Drawing.Size(111, 22);
            this.mnuPlayModeRepeat.Text = "Repeat";
            this.mnuPlayModeRepeat.Click += new System.EventHandler(this.mnuPlayModeRepeat_Click);
            // 
            // mnuPlayModeShuffle
            // 
            this.mnuPlayModeShuffle.Name = "mnuPlayModeShuffle";
            this.mnuPlayModeShuffle.Size = new System.Drawing.Size(111, 22);
            this.mnuPlayModeShuffle.Text = "Shuffle";
            this.mnuPlayModeShuffle.Click += new System.EventHandler(this.mnuPlayModeShuffle_Click);
            // 
            // mnuPlayModeNone
            // 
            this.mnuPlayModeNone.Name = "mnuPlayModeNone";
            this.mnuPlayModeNone.Size = new System.Drawing.Size(111, 22);
            this.mnuPlayModeNone.Text = "None";
            this.mnuPlayModeNone.Click += new System.EventHandler(this.mnuPlayModeNone_Click);
            // 
            // playlistToolStripMenuItem
            // 
            this.playlistToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuPlaylistNext,
            this.mnuPlaylistPrevious,
            this.gToolStripMenuItem,
            this.mnuPlaylistCreate,
            this.mnuPlaylistSave,
            this.mnuPlaylistSaveAs,
            this.mnuPlaylistClear});
            this.playlistToolStripMenuItem.Name = "playlistToolStripMenuItem";
            this.playlistToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.playlistToolStripMenuItem.Text = "Playlist";
            // 
            // mnuPlaylistNext
            // 
            this.mnuPlaylistNext.Name = "mnuPlaylistNext";
            this.mnuPlaylistNext.Size = new System.Drawing.Size(197, 22);
            this.mnuPlaylistNext.Text = "Next Song";
            this.mnuPlaylistNext.Click += new System.EventHandler(this.mnuPlaylistNext_Click);
            // 
            // mnuPlaylistPrevious
            // 
            this.mnuPlaylistPrevious.Name = "mnuPlaylistPrevious";
            this.mnuPlaylistPrevious.Size = new System.Drawing.Size(197, 22);
            this.mnuPlaylistPrevious.Text = "Previous Song";
            this.mnuPlaylistPrevious.Click += new System.EventHandler(this.mnuPlaylistPrevious_Click);
            // 
            // gToolStripMenuItem
            // 
            this.gToolStripMenuItem.Name = "gToolStripMenuItem";
            this.gToolStripMenuItem.Size = new System.Drawing.Size(194, 6);
            // 
            // mnuPlaylistCreate
            // 
            this.mnuPlaylistCreate.Name = "mnuPlaylistCreate";
            this.mnuPlaylistCreate.Size = new System.Drawing.Size(197, 22);
            this.mnuPlaylistCreate.Text = "Create New Playlist";
            this.mnuPlaylistCreate.Click += new System.EventHandler(this.mnuPlaylistCreate_Click);
            // 
            // mnuPlaylistSave
            // 
            this.mnuPlaylistSave.Name = "mnuPlaylistSave";
            this.mnuPlaylistSave.Size = new System.Drawing.Size(197, 22);
            this.mnuPlaylistSave.Text = "Save Current Playlist";
            this.mnuPlaylistSave.Click += new System.EventHandler(this.mnuPlaylistSave_Click);
            // 
            // mnuPlaylistSaveAs
            // 
            this.mnuPlaylistSaveAs.Name = "mnuPlaylistSaveAs";
            this.mnuPlaylistSaveAs.Size = new System.Drawing.Size(197, 22);
            this.mnuPlaylistSaveAs.Text = "Save Current Playlist As";
            this.mnuPlaylistSaveAs.Click += new System.EventHandler(this.mnuPlaylistSaveAs_Click);
            // 
            // mnuPlaylistClear
            // 
            this.mnuPlaylistClear.Name = "mnuPlaylistClear";
            this.mnuPlaylistClear.Size = new System.Drawing.Size(197, 22);
            this.mnuPlaylistClear.Text = "Clear Current Playlist";
            this.mnuPlaylistClear.Click += new System.EventHandler(this.mnuPlaylistClear_Click);
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(278, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 17);
            this.label1.TabIndex = 18;
            this.label1.Text = "Playlist:";
            // 
            // lblPlaylistName
            // 
            this.lblPlaylistName.AutoSize = true;
            this.lblPlaylistName.Location = new System.Drawing.Point(290, 102);
            this.lblPlaylistName.Name = "lblPlaylistName";
            this.lblPlaylistName.Size = new System.Drawing.Size(0, 13);
            this.lblPlaylistName.TabIndex = 19;
            // 
            // JamBitForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(773, 412);
            this.Controls.Add(this.lblPlaylistName);
            this.Controls.Add(this.label1);
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
        private System.Windows.Forms.Button btnPlayMode;
        private MusicPlayerControlsLibrary.MultipleSelectTreeView treeLibrary;
        private System.Windows.Forms.ColumnHeader clmPlayCount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblPlaylistName;
        private System.Windows.Forms.ToolStripMenuItem mnuFilePreferences;
        private System.Windows.Forms.ToolStripMenuItem mnuFileClose;
        private System.Windows.Forms.ToolStripMenuItem playbackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuPlaybackPlay;
        private System.Windows.Forms.ToolStripMenuItem mnuPlaybackPause;
        private System.Windows.Forms.ToolStripMenuItem playlistToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuPlaylistNext;
        private System.Windows.Forms.ToolStripMenuItem mnuPlaylistPrevious;
        private System.Windows.Forms.ToolStripMenuItem mnuPlaylistCreate;
        private System.Windows.Forms.ToolStripMenuItem mnuPlaylistSave;
        private System.Windows.Forms.ToolStripMenuItem mnuPlaylistSaveAs;
        private System.Windows.Forms.ToolStripMenuItem mnuPlaylistClear;
        private System.Windows.Forms.ToolStripSeparator gToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuPlaybackPlayMode;
        private System.Windows.Forms.ToolStripMenuItem mnuPlayModeLoop;
        private System.Windows.Forms.ToolStripMenuItem mnuPlayModeRepeat;
        private System.Windows.Forms.ToolStripMenuItem mnuPlayModeShuffle;
        private System.Windows.Forms.ToolStripMenuItem mnuPlayModeNone;
    }
}

