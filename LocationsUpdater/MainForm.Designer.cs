namespace LocationsUpdater {
	partial class MainForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.downloadProgress = new System.Windows.Forms.ProgressBar();
			this.locationsFileUrl = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.downloadType = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.statusLabelA = new System.Windows.Forms.ToolStripStatusLabel();
			this.statusLabelB = new System.Windows.Forms.ToolStripStatusLabel();
			this.defaultButton = new System.Windows.Forms.Button();
			this.abortButton = new System.Windows.Forms.Button();
			this.downloadButton = new System.Windows.Forms.Button();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
			this.toolStripContainer1.ContentPanel.SuspendLayout();
			this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer1.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// downloadProgress
			// 
			this.downloadProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.downloadProgress.Location = new System.Drawing.Point(15, 56);
			this.downloadProgress.MarqueeAnimationSpeed = 25;
			this.downloadProgress.Name = "downloadProgress";
			this.downloadProgress.Size = new System.Drawing.Size(390, 16);
			this.downloadProgress.TabIndex = 0;
			// 
			// locationsFileUrl
			// 
			this.locationsFileUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.locationsFileUrl.Location = new System.Drawing.Point(15, 30);
			this.locationsFileUrl.Name = "locationsFileUrl";
			this.locationsFileUrl.Size = new System.Drawing.Size(190, 20);
			this.locationsFileUrl.TabIndex = 1;
			this.locationsFileUrl.Text = "http://ac.warcry.com/atlas/atlas_xml.php";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(94, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Locations file URL";
			// 
			// downloadType
			// 
			this.downloadType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.downloadType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.downloadType.FormattingEnabled = true;
			this.downloadType.Items.AddRange(new object[] {
            "Crossroads of Dereth",
            "ACSpedia"});
			this.downloadType.Location = new System.Drawing.Point(211, 29);
			this.downloadType.Name = "downloadType";
			this.downloadType.Size = new System.Drawing.Size(133, 21);
			this.downloadType.TabIndex = 4;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(208, 13);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(88, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Database source";
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.BottomToolStripPanel
			// 
			this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip1);
			// 
			// toolStripContainer1.ContentPanel
			// 
			this.toolStripContainer1.ContentPanel.Controls.Add(this.defaultButton);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.abortButton);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.downloadButton);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.downloadProgress);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.label1);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.label2);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.locationsFileUrl);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.downloadType);
			this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(417, 108);
			this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainer1.LeftToolStripPanelVisible = false;
			this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
			this.toolStripContainer1.Name = "toolStripContainer1";
			this.toolStripContainer1.RightToolStripPanelVisible = false;
			this.toolStripContainer1.Size = new System.Drawing.Size(417, 154);
			this.toolStripContainer1.TabIndex = 6;
			this.toolStripContainer1.Text = "toolStripContainer1";
			// 
			// toolStripContainer1.TopToolStripPanel
			// 
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menuStrip1);
			// 
			// statusStrip1
			// 
			this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabelA,
            this.statusLabelB});
			this.statusStrip1.Location = new System.Drawing.Point(0, 0);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(417, 22);
			this.statusStrip1.TabIndex = 0;
			// 
			// statusLabelA
			// 
			this.statusLabelA.Name = "statusLabelA";
			this.statusLabelA.Size = new System.Drawing.Size(25, 17);
			this.statusLabelA.Text = "Idle";
			// 
			// statusLabelB
			// 
			this.statusLabelB.Name = "statusLabelB";
			this.statusLabelB.Size = new System.Drawing.Size(402, 17);
			this.statusLabelB.Spring = true;
			this.statusLabelB.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// defaultButton
			// 
			this.defaultButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.defaultButton.Location = new System.Drawing.Point(350, 29);
			this.defaultButton.Name = "defaultButton";
			this.defaultButton.Size = new System.Drawing.Size(55, 21);
			this.defaultButton.TabIndex = 8;
			this.defaultButton.Text = "Default";
			this.defaultButton.UseVisualStyleBackColor = true;
			this.defaultButton.Click += new System.EventHandler(this.defaultButton_Click);
			// 
			// abortButton
			// 
			this.abortButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.abortButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.abortButton.Enabled = false;
			this.abortButton.Location = new System.Drawing.Point(326, 78);
			this.abortButton.Name = "abortButton";
			this.abortButton.Size = new System.Drawing.Size(79, 24);
			this.abortButton.TabIndex = 7;
			this.abortButton.Text = "&Abort";
			this.abortButton.UseVisualStyleBackColor = true;
			this.abortButton.Click += new System.EventHandler(this.abortButton_Click);
			// 
			// downloadButton
			// 
			this.downloadButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.downloadButton.Location = new System.Drawing.Point(202, 78);
			this.downloadButton.Name = "downloadButton";
			this.downloadButton.Size = new System.Drawing.Size(118, 24);
			this.downloadButton.TabIndex = 6;
			this.downloadButton.Text = "&Download";
			this.downloadButton.UseVisualStyleBackColor = true;
			this.downloadButton.Click += new System.EventHandler(this.downloadButton_Click);
			// 
			// menuStrip1
			// 
			this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(417, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.ShortcutKeyDisplayString = "Alt+F4";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// MainForm
			// 
			this.AcceptButton = this.downloadButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.abortButton;
			this.ClientSize = new System.Drawing.Size(417, 154);
			this.Controls.Add(this.toolStripContainer1);
			this.MainMenuStrip = this.menuStrip1;
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(1024, 188);
			this.MinimumSize = new System.Drawing.Size(325, 188);
			this.Name = "MainForm";
			this.Text = "GoArrow Locations Updater";
			this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
			this.toolStripContainer1.ContentPanel.ResumeLayout(false);
			this.toolStripContainer1.ContentPanel.PerformLayout();
			this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.PerformLayout();
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ProgressBar downloadProgress;
		private System.Windows.Forms.TextBox locationsFileUrl;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox downloadType;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ToolStripContainer toolStripContainer1;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripStatusLabel statusLabelA;
		private System.Windows.Forms.Button abortButton;
		private System.Windows.Forms.Button downloadButton;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripStatusLabel statusLabelB;
		private System.Windows.Forms.Button defaultButton;
	}
}

