namespace JamBit
{
    partial class Form1
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
            this.lblSongInformation = new MusicPlayerControlsLibrary.MarqueeLabel();
            this.prgVolume = new MusicPlayerControlsLibrary.SlidableProgressBar();
            this.prgSongTime = new MusicPlayerControlsLibrary.SlidableProgressBar();
            this.btnOpen = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblCurrentTime
            // 
            this.lblCurrentTime.AutoSize = true;
            this.lblCurrentTime.Location = new System.Drawing.Point(12, 39);
            this.lblCurrentTime.Name = "lblCurrentTime";
            this.lblCurrentTime.Size = new System.Drawing.Size(28, 13);
            this.lblCurrentTime.TabIndex = 5;
            this.lblCurrentTime.Text = "0:00";
            this.lblCurrentTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSongLength
            // 
            this.lblSongLength.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.lblSongLength.AutoSize = true;
            this.lblSongLength.Location = new System.Drawing.Point(244, 39);
            this.lblSongLength.Name = "lblSongLength";
            this.lblSongLength.Size = new System.Drawing.Size(28, 13);
            this.lblSongLength.TabIndex = 6;
            this.lblSongLength.Text = "0:00";
            this.lblSongLength.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(15, 83);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(75, 23);
            this.btnPlay.TabIndex = 7;
            this.btnPlay.Text = "Play";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(106, 83);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(75, 23);
            this.btnPause.TabIndex = 8;
            this.btnPause.Text = "Pause";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // lblSongInformation
            // 
            this.lblSongInformation.BackColor = System.Drawing.Color.Transparent;
            this.lblSongInformation.CycleText = new string[] {
        "marqueeLabel1"};
            this.lblSongInformation.LabelSpeed = 30;
            this.lblSongInformation.Location = new System.Drawing.Point(15, 9);
            this.lblSongInformation.Margin = new System.Windows.Forms.Padding(0);
            this.lblSongInformation.Name = "lblSongInformation";
            this.lblSongInformation.PauseLength = 2500;
            this.lblSongInformation.Size = new System.Drawing.Size(254, 12);
            this.lblSongInformation.TabIndex = 9;
            // 
            // prgVolume
            // 
            this.prgVolume.Location = new System.Drawing.Point(15, 235);
            this.prgVolume.Maximum = 1000;
            this.prgVolume.Name = "prgVolume";
            this.prgVolume.Size = new System.Drawing.Size(254, 14);
            this.prgVolume.Step = 0;
            this.prgVolume.TabIndex = 4;
            this.prgVolume.Value = 500;
            this.prgVolume.ValueSlidTo += new System.EventHandler(this.pgrVolume_ValueSlidTo);
            // 
            // prgSongTime
            // 
            this.prgSongTime.Location = new System.Drawing.Point(52, 34);
            this.prgSongTime.Maximum = 1000;
            this.prgSongTime.Name = "prgSongTime";
            this.prgSongTime.Size = new System.Drawing.Size(180, 23);
            this.prgSongTime.Step = 0;
            this.prgSongTime.TabIndex = 3;
            this.prgSongTime.ValueSelected += new System.EventHandler(this.prgSongTime_SelecedValue);
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(197, 83);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(75, 23);
            this.btnOpen.TabIndex = 10;
            this.btnOpen.Text = "Open";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.lblSongInformation);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.btnPlay);
            this.Controls.Add(this.lblSongLength);
            this.Controls.Add(this.lblCurrentTime);
            this.Controls.Add(this.prgVolume);
            this.Controls.Add(this.prgSongTime);
            this.Name = "Form1";
            this.Text = "Form1";
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
    }
}

