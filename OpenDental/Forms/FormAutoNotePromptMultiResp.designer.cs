namespace OpenDental{
	partial class FormAutoNotePromptMultiResp {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAutoNotePromptMultiResp));
			this.labelPrompt = new System.Windows.Forms.Label();
			this.listMain = new System.Windows.Forms.CheckedListBox();
			this.butPreview = new OpenDental.UI.Button();
			this.butSkip = new OpenDental.UI.Button();
			this.butOK = new OpenDental.UI.Button();
			this.butCancel = new OpenDental.UI.Button();
			this.butSelectAll = new OpenDental.UI.Button();
			this.butSelectNone = new OpenDental.UI.Button();
			this.SuspendLayout();
			// 
			// labelPrompt
			// 
			this.labelPrompt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labelPrompt.Location = new System.Drawing.Point(12, 3);
			this.labelPrompt.Name = "labelPrompt";
			this.labelPrompt.Size = new System.Drawing.Size(387, 56);
			this.labelPrompt.TabIndex = 114;
			this.labelPrompt.Text = "Prompt";
			this.labelPrompt.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// listMain
			// 
			this.listMain.CheckOnClick = true;
			this.listMain.FormattingEnabled = true;
			this.listMain.HorizontalScrollbar = true;
			this.listMain.Location = new System.Drawing.Point(15, 95);
			this.listMain.Name = "listMain";
			this.listMain.Size = new System.Drawing.Size(382, 214);
			this.listMain.TabIndex = 1;
			// 
			// butPreview
			// 
			this.butPreview.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butPreview.Autosize = true;
			this.butPreview.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butPreview.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butPreview.CornerRadius = 4F;
			this.butPreview.Location = new System.Drawing.Point(241, 325);
			this.butPreview.Name = "butPreview";
			this.butPreview.Size = new System.Drawing.Size(75, 24);
			this.butPreview.TabIndex = 15;
			this.butPreview.Text = "&Preview";
			this.butPreview.Click += new System.EventHandler(this.butPreview_Click);
			// 
			// butSkip
			// 
			this.butSkip.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butSkip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSkip.Autosize = true;
			this.butSkip.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butSkip.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butSkip.CornerRadius = 4F;
			this.butSkip.Location = new System.Drawing.Point(160, 325);
			this.butSkip.Name = "butSkip";
			this.butSkip.Size = new System.Drawing.Size(75, 24);
			this.butSkip.TabIndex = 10;
			this.butSkip.Text = "&Skip";
			this.butSkip.Click += new System.EventHandler(this.butSkip_Click);
			// 
			// butOK
			// 
			this.butOK.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Autosize = true;
			this.butOK.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butOK.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butOK.CornerRadius = 4F;
			this.butOK.Location = new System.Drawing.Point(79, 325);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 24);
			this.butOK.TabIndex = 5;
			this.butOK.Text = "&OK";
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// butCancel
			// 
			this.butCancel.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butCancel.Autosize = true;
			this.butCancel.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butCancel.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butCancel.CornerRadius = 4F;
			this.butCancel.Location = new System.Drawing.Point(322, 325);
			this.butCancel.Name = "butCancel";
			this.butCancel.Size = new System.Drawing.Size(75, 24);
			this.butCancel.TabIndex = 20;
			this.butCancel.Text = "&Cancel";
			this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
			// 
			// butSelectAll
			// 
			this.butSelectAll.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSelectAll.Autosize = true;
			this.butSelectAll.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butSelectAll.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butSelectAll.CornerRadius = 4F;
			this.butSelectAll.Location = new System.Drawing.Point(241, 65);
			this.butSelectAll.Name = "butSelectAll";
			this.butSelectAll.Size = new System.Drawing.Size(75, 24);
			this.butSelectAll.TabIndex = 25;
			this.butSelectAll.Text = "Select &All";
			this.butSelectAll.Click += new System.EventHandler(this.butSelectAll_Click);
			// 
			// butSelectNone
			// 
			this.butSelectNone.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butSelectNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSelectNone.Autosize = true;
			this.butSelectNone.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butSelectNone.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butSelectNone.CornerRadius = 4F;
			this.butSelectNone.Location = new System.Drawing.Point(319, 65);
			this.butSelectNone.Name = "butSelectNone";
			this.butSelectNone.Size = new System.Drawing.Size(78, 24);
			this.butSelectNone.TabIndex = 30;
			this.butSelectNone.Text = "Select &None";
			this.butSelectNone.Click += new System.EventHandler(this.butSelectNone_Click);
			// 
			// FormAutoNotePromptMultiResp
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(409, 361);
			this.Controls.Add(this.butSelectNone);
			this.Controls.Add(this.butSelectAll);
			this.Controls.Add(this.listMain);
			this.Controls.Add(this.butPreview);
			this.Controls.Add(this.butSkip);
			this.Controls.Add(this.labelPrompt);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.butCancel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormAutoNotePromptMultiResp";
			this.Text = "Prompt Multi Response";
			this.Load += new System.EventHandler(this.FormAutoNotePromptMultiResp_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormAutoNotePromptMultiResp_KeyDown);
			this.ResumeLayout(false);

		}

		#endregion

		private OpenDental.UI.Button butOK;
		private OpenDental.UI.Button butCancel;
		private System.Windows.Forms.Label labelPrompt;
		private OpenDental.UI.Button butSkip;
		private OpenDental.UI.Button butPreview;
		private System.Windows.Forms.CheckedListBox listMain;
		private UI.Button butSelectAll;
		private UI.Button butSelectNone;
	}
}