namespace OpenDental {
	partial class FormProgressExtended {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProgressExtended));
			this.label1 = new System.Windows.Forms.Label();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.label2 = new System.Windows.Forms.Label();
			this.progressBar2 = new System.Windows.Forms.ProgressBar();
			this.label3 = new System.Windows.Forms.Label();
			this.progressBar3 = new System.Windows.Forms.ProgressBar();
			this.labelWarning = new System.Windows.Forms.Label();
			this.textMsg = new System.Windows.Forms.TextBox();
			this.butPause = new OpenDental.UI.Button();
			this.butClose = new OpenDental.UI.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.label3Percent = new System.Windows.Forms.Label();
			this.label2Percent = new System.Windows.Forms.Label();
			this.label1Percent = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(75, 19);
			this.label1.TabIndex = 1;
			this.label1.Text = "1 Progress";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(94, 11);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(282, 23);
			this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.progressBar1.TabIndex = 0;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(13, 45);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(75, 38);
			this.label2.TabIndex = 1;
			this.label2.Text = "2 Progress";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// progressBar2
			// 
			this.progressBar2.Location = new System.Drawing.Point(94, 53);
			this.progressBar2.Name = "progressBar2";
			this.progressBar2.Size = new System.Drawing.Size(282, 23);
			this.progressBar2.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.progressBar2.TabIndex = 0;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(13, 87);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(75, 38);
			this.label3.TabIndex = 1;
			this.label3.Text = "3 Progress";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// progressBar3
			// 
			this.progressBar3.Location = new System.Drawing.Point(94, 95);
			this.progressBar3.Name = "progressBar3";
			this.progressBar3.Size = new System.Drawing.Size(282, 23);
			this.progressBar3.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.progressBar3.TabIndex = 0;
			// 
			// labelWarning
			// 
			this.labelWarning.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.labelWarning.Location = new System.Drawing.Point(94, 227);
			this.labelWarning.Name = "labelWarning";
			this.labelWarning.Size = new System.Drawing.Size(216, 23);
			this.labelWarning.TabIndex = 8;
			this.labelWarning.Text = "Finishing actions. Please wait...";
			this.labelWarning.Visible = false;
			// 
			// textMsg
			// 
			this.textMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textMsg.Location = new System.Drawing.Point(13, 154);
			this.textMsg.Multiline = true;
			this.textMsg.Name = "textMsg";
			this.textMsg.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textMsg.Size = new System.Drawing.Size(404, 67);
			this.textMsg.TabIndex = 10;
			// 
			// butPause
			// 
			this.butPause.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butPause.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butPause.Autosize = true;
			this.butPause.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butPause.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butPause.CornerRadius = 4F;
			this.butPause.Location = new System.Drawing.Point(13, 227);
			this.butPause.Name = "butPause";
			this.butPause.Size = new System.Drawing.Size(75, 23);
			this.butPause.TabIndex = 5;
			this.butPause.Text = "Pause";
			this.butPause.UseVisualStyleBackColor = true;
			this.butPause.Click += new System.EventHandler(this.butPause_Click);
			// 
			// butClose
			// 
			this.butClose.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butClose.Autosize = true;
			this.butClose.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butClose.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butClose.CornerRadius = 4F;
			this.butClose.Location = new System.Drawing.Point(342, 227);
			this.butClose.Name = "butClose";
			this.butClose.Size = new System.Drawing.Size(75, 23);
			this.butClose.TabIndex = 4;
			this.butClose.Text = "Cancel";
			this.butClose.UseVisualStyleBackColor = true;
			this.butClose.Click += new System.EventHandler(this.butClose_Click);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(13, 132);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(93, 19);
			this.label4.TabIndex = 11;
			this.label4.Text = "Progress Log";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label3Percent
			// 
			this.label3Percent.Location = new System.Drawing.Point(376, 87);
			this.label3Percent.Name = "label3Percent";
			this.label3Percent.Size = new System.Drawing.Size(41, 38);
			this.label3Percent.TabIndex = 12;
			this.label3Percent.Text = "3%";
			this.label3Percent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label2Percent
			// 
			this.label2Percent.Location = new System.Drawing.Point(376, 45);
			this.label2Percent.Name = "label2Percent";
			this.label2Percent.Size = new System.Drawing.Size(41, 38);
			this.label2Percent.TabIndex = 13;
			this.label2Percent.Text = "2%";
			this.label2Percent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label1Percent
			// 
			this.label1Percent.Location = new System.Drawing.Point(376, 13);
			this.label1Percent.Name = "label1Percent";
			this.label1Percent.Size = new System.Drawing.Size(41, 19);
			this.label1Percent.TabIndex = 14;
			this.label1Percent.Text = "1%";
			this.label1Percent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// FormProgressExtended
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.ControlLight;
			this.ClientSize = new System.Drawing.Size(428, 262);
			this.Controls.Add(this.labelWarning);
			this.Controls.Add(this.butPause);
			this.Controls.Add(this.butClose);
			this.Controls.Add(this.label3Percent);
			this.Controls.Add(this.label2Percent);
			this.Controls.Add(this.label1Percent);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.textMsg);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.progressBar3);
			this.Controls.Add(this.progressBar2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.progressBar1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(100, 100);
			this.Name = "FormProgressExtended";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormProgressExtended_FormClosing);
			this.Shown += new System.EventHandler(this.FormProgressExtended_Shown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private UI.Button butClose;
		private UI.Button butPause;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ProgressBar progressBar2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ProgressBar progressBar3;
		private System.Windows.Forms.Label labelWarning;
		private System.Windows.Forms.TextBox textMsg;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3Percent;
		private System.Windows.Forms.Label label2Percent;
		private System.Windows.Forms.Label label1Percent;
	}
}