namespace JamBit
{
    partial class OptionsForm
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
            this.lstLibraryFolders = new System.Windows.Forms.ListView();
            this.label1 = new System.Windows.Forms.Label();
            this.btnAddFolder = new System.Windows.Forms.Button();
            this.btnRemoveFolder = new System.Windows.Forms.Button();
            this.btnScan = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lstLibraryFolders
            // 
            this.lstLibraryFolders.Location = new System.Drawing.Point(12, 35);
            this.lstLibraryFolders.Name = "lstLibraryFolders";
            this.lstLibraryFolders.Size = new System.Drawing.Size(260, 185);
            this.lstLibraryFolders.TabIndex = 0;
            this.lstLibraryFolders.UseCompatibleStateImageBehavior = false;
            this.lstLibraryFolders.View = System.Windows.Forms.View.List;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(103, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Library Folders";
            // 
            // btnAddFolder
            // 
            this.btnAddFolder.Location = new System.Drawing.Point(121, 6);
            this.btnAddFolder.Name = "btnAddFolder";
            this.btnAddFolder.Size = new System.Drawing.Size(70, 23);
            this.btnAddFolder.TabIndex = 2;
            this.btnAddFolder.Text = "Add";
            this.btnAddFolder.UseVisualStyleBackColor = true;
            this.btnAddFolder.Click += new System.EventHandler(this.btnAddFolder_Click);
            // 
            // btnRemoveFolder
            // 
            this.btnRemoveFolder.Location = new System.Drawing.Point(197, 6);
            this.btnRemoveFolder.Name = "btnRemoveFolder";
            this.btnRemoveFolder.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveFolder.TabIndex = 3;
            this.btnRemoveFolder.Text = "Remove";
            this.btnRemoveFolder.UseVisualStyleBackColor = true;
            // 
            // btnScan
            // 
            this.btnScan.Location = new System.Drawing.Point(12, 226);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(260, 23);
            this.btnScan.TabIndex = 4;
            this.btnScan.Text = "Scan Now";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // OptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.btnScan);
            this.Controls.Add(this.btnRemoveFolder);
            this.Controls.Add(this.btnAddFolder);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lstLibraryFolders);
            this.Name = "OptionsForm";
            this.Text = "OptionsForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lstLibraryFolders;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAddFolder;
        private System.Windows.Forms.Button btnRemoveFolder;
        private System.Windows.Forms.Button btnScan;
    }
}