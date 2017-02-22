namespace OpenDental{
	partial class FormProgressStatus {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProgressStatus));
			this.labelMsg = new System.Windows.Forms.Label();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.textHistoryMsg = new System.Windows.Forms.TextBox();
			this.butCopyToClipboard = new OpenDental.UI.Button();
			this.butClose = new OpenDental.UI.Button();
			this.SuspendLayout();
			// 
			// labelMsg
			// 
			this.labelMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelMsg.Location = new System.Drawing.Point(52, 11);
			this.labelMsg.Name = "labelMsg";
			this.labelMsg.Size = new System.Drawing.Size(366, 36);
			this.labelMsg.TabIndex = 1;
			this.labelMsg.Text = "Please Wait...";
			this.labelMsg.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// progressBar
			// 
			this.progressBar.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.progressBar.Location = new System.Drawing.Point(55, 50);
			this.progressBar.MarqueeAnimationSpeed = 50;
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(363, 23);
			this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.progressBar.TabIndex = 2;
			// 
			// textHistoryMsg
			// 
			this.textHistoryMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textHistoryMsg.Location = new System.Drawing.Point(12, 11);
			this.textHistoryMsg.Multiline = true;
			this.textHistoryMsg.Name = "textHistoryMsg";
			this.textHistoryMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textHistoryMsg.Size = new System.Drawing.Size(445, 36);
			this.textHistoryMsg.TabIndex = 3;
			this.textHistoryMsg.Visible = false;
			// 
			// butCopyToClipboard
			// 
			this.butCopyToClipboard.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butCopyToClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butCopyToClipboard.Autosize = true;
			this.butCopyToClipboard.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butCopyToClipboard.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butCopyToClipboard.CornerRadius = 4F;
			this.butCopyToClipboard.Location = new System.Drawing.Point(12, 50);
			this.butCopyToClipboard.Name = "butCopyToClipboard";
			this.butCopyToClipboard.Size = new System.Drawing.Size(100, 23);
			this.butCopyToClipboard.TabIndex = 5;
			this.butCopyToClipboard.Text = "Copy to Clipboard";
			this.butCopyToClipboard.UseVisualStyleBackColor = true;
			this.butCopyToClipboard.Visible = false;
			this.butCopyToClipboard.Click += new System.EventHandler(this.butCopyToClipboard_Click);
			// 
			// butClose
			// 
			this.butClose.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butClose.Autosize = true;
			this.butClose.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butClose.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butClose.CornerRadius = 4F;
			this.butClose.Location = new System.Drawing.Point(382, 50);
			this.butClose.Name = "butClose";
			this.butClose.Size = new System.Drawing.Size(75, 23);
			this.butClose.TabIndex = 4;
			this.butClose.Text = "Close";
			this.butClose.UseVisualStyleBackColor = true;
			this.butClose.Visible = false;
			this.butClose.Click += new System.EventHandler(this.butClose_Click);
			// 
			// FormProgressStatus
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.ClientSize = new System.Drawing.Size(469, 104);
			this.Controls.Add(this.butCopyToClipboard);
			this.Controls.Add(this.butClose);
			this.Controls.Add(this.progressBar);
			this.Controls.Add(this.labelMsg);
			this.Controls.Add(this.textHistoryMsg);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(100, 100);
			this.Name = "FormProgressStatus";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormProgressStatus_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label labelMsg;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.TextBox textHistoryMsg;
		private UI.Button butClose;
		private UI.Button butCopyToClipboard;
	}
}