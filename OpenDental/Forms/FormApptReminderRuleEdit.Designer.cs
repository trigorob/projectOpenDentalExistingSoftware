namespace OpenDental {
	partial class FormApptReminderRuleEdit {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormApptReminderRuleEdit));
			this.butCancel = new System.Windows.Forms.Button();
			this.textTemplateEmail = new System.Windows.Forms.RichTextBox();
			this.textTemplateSms = new System.Windows.Forms.RichTextBox();
			this.labelLeadTime = new System.Windows.Forms.Label();
			this.butOk = new System.Windows.Forms.Button();
			this.gridPriorities = new OpenDental.UI.ODGrid();
			this.butUp = new OpenDental.UI.Button();
			this.butDown = new OpenDental.UI.Button();
			this.checkSendAll = new System.Windows.Forms.CheckBox();
			this.labelTags = new System.Windows.Forms.Label();
			this.butDelete = new OpenDental.UI.Button();
			this.textTemplateSubject = new System.Windows.Forms.RichTextBox();
			this.groupBox12 = new System.Windows.Forms.GroupBox();
			this.groupSendOrder = new System.Windows.Forms.GroupBox();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.labelRuleType = new System.Windows.Forms.Label();
			this.textTime = new OpenDental.ValidNum();
			this.groupBox12.SuspendLayout();
			this.groupSendOrder.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.SuspendLayout();
			// 
			// butCancel
			// 
			this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butCancel.Location = new System.Drawing.Point(422, 651);
			this.butCancel.Name = "butCancel";
			this.butCancel.Size = new System.Drawing.Size(75, 24);
			this.butCancel.TabIndex = 0;
			this.butCancel.Text = "Cancel";
			this.butCancel.UseVisualStyleBackColor = true;
			this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
			// 
			// textTemplateEmail
			// 
			this.textTemplateEmail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textTemplateEmail.Location = new System.Drawing.Point(6, 57);
			this.textTemplateEmail.Name = "textTemplateEmail";
			this.textTemplateEmail.Size = new System.Drawing.Size(398, 141);
			this.textTemplateEmail.TabIndex = 95;
			this.textTemplateEmail.Text = "";
			// 
			// textTemplateSms
			// 
			this.textTemplateSms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textTemplateSms.Location = new System.Drawing.Point(6, 17);
			this.textTemplateSms.Name = "textTemplateSms";
			this.textTemplateSms.Size = new System.Drawing.Size(398, 66);
			this.textTemplateSms.TabIndex = 69;
			this.textTemplateSms.Text = "";
			// 
			// labelLeadTime
			// 
			this.labelLeadTime.Location = new System.Drawing.Point(230, 17);
			this.labelLeadTime.Name = "labelLeadTime";
			this.labelLeadTime.Size = new System.Drawing.Size(51, 21);
			this.labelLeadTime.TabIndex = 15;
			this.labelLeadTime.Text = "Hours";
			this.labelLeadTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// butOk
			// 
			this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOk.Location = new System.Drawing.Point(341, 651);
			this.butOk.Name = "butOk";
			this.butOk.Size = new System.Drawing.Size(75, 24);
			this.butOk.TabIndex = 102;
			this.butOk.Text = "OK";
			this.butOk.UseVisualStyleBackColor = true;
			this.butOk.Click += new System.EventHandler(this.butOk_Click);
			// 
			// gridPriorities
			// 
			this.gridPriorities.HasAddButton = false;
			this.gridPriorities.HasMultilineHeaders = false;
			this.gridPriorities.HeaderHeight = 15;
			this.gridPriorities.HScrollVisible = false;
			this.gridPriorities.Location = new System.Drawing.Point(42, 19);
			this.gridPriorities.Name = "gridPriorities";
			this.gridPriorities.ScrollValue = 0;
			this.gridPriorities.Size = new System.Drawing.Size(359, 96);
			this.gridPriorities.TabIndex = 106;
			this.gridPriorities.Title = "Contact Methods";
			this.gridPriorities.TitleHeight = 18;
			this.gridPriorities.TranslationName = null;
			// 
			// butUp
			// 
			this.butUp.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butUp.Autosize = false;
			this.butUp.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butUp.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butUp.CornerRadius = 4F;
			this.butUp.Image = global::OpenDental.Properties.Resources.up;
			this.butUp.Location = new System.Drawing.Point(6, 19);
			this.butUp.Name = "butUp";
			this.butUp.Size = new System.Drawing.Size(30, 30);
			this.butUp.TabIndex = 103;
			this.butUp.UseVisualStyleBackColor = true;
			this.butUp.Click += new System.EventHandler(this.butUp_Click);
			// 
			// butDown
			// 
			this.butDown.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butDown.Autosize = false;
			this.butDown.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butDown.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butDown.CornerRadius = 4F;
			this.butDown.Image = global::OpenDental.Properties.Resources.down;
			this.butDown.Location = new System.Drawing.Point(6, 55);
			this.butDown.Name = "butDown";
			this.butDown.Size = new System.Drawing.Size(30, 30);
			this.butDown.TabIndex = 104;
			this.butDown.UseVisualStyleBackColor = true;
			this.butDown.Click += new System.EventHandler(this.butDown_Click);
			// 
			// checkSendAll
			// 
			this.checkSendAll.Location = new System.Drawing.Point(42, 121);
			this.checkSendAll.Name = "checkSendAll";
			this.checkSendAll.Size = new System.Drawing.Size(359, 18);
			this.checkSendAll.TabIndex = 105;
			this.checkSendAll.Text = "Send All - If available, send text AND email.";
			this.checkSendAll.UseVisualStyleBackColor = true;
			this.checkSendAll.CheckedChanged += new System.EventHandler(this.checkSendAll_CheckedChanged);
			// 
			// labelTags
			// 
			this.labelTags.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labelTags.Location = new System.Drawing.Point(3, 16);
			this.labelTags.Name = "labelTags";
			this.labelTags.Size = new System.Drawing.Size(404, 50);
			this.labelTags.TabIndex = 110;
			this.labelTags.Text = "Use template tags to create dynamic messages.";
			// 
			// butDelete
			// 
			this.butDelete.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butDelete.Autosize = true;
			this.butDelete.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butDelete.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butDelete.CornerRadius = 4F;
			this.butDelete.Image = global::OpenDental.Properties.Resources.deleteX;
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDelete.Location = new System.Drawing.Point(18, 649);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(84, 26);
			this.butDelete.TabIndex = 111;
			this.butDelete.Text = "Delete";
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// textTemplateSubject
			// 
			this.textTemplateSubject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textTemplateSubject.Location = new System.Drawing.Point(6, 17);
			this.textTemplateSubject.Name = "textTemplateSubject";
			this.textTemplateSubject.Size = new System.Drawing.Size(398, 36);
			this.textTemplateSubject.TabIndex = 113;
			this.textTemplateSubject.Text = "";
			// 
			// groupBox12
			// 
			this.groupBox12.Controls.Add(this.textTime);
			this.groupBox12.Controls.Add(this.labelLeadTime);
			this.groupBox12.Location = new System.Drawing.Point(54, 61);
			this.groupBox12.Name = "groupBox12";
			this.groupBox12.Size = new System.Drawing.Size(410, 51);
			this.groupBox12.TabIndex = 101;
			this.groupBox12.TabStop = false;
			this.groupBox12.Text = "Lead Time - (0 to disable)";
			// 
			// groupSendOrder
			// 
			this.groupSendOrder.Controls.Add(this.gridPriorities);
			this.groupSendOrder.Controls.Add(this.checkSendAll);
			this.groupSendOrder.Controls.Add(this.butDown);
			this.groupSendOrder.Controls.Add(this.butUp);
			this.groupSendOrder.Location = new System.Drawing.Point(54, 116);
			this.groupSendOrder.Name = "groupSendOrder";
			this.groupSendOrder.Size = new System.Drawing.Size(410, 148);
			this.groupSendOrder.TabIndex = 114;
			this.groupSendOrder.TabStop = false;
			this.groupSendOrder.Text = "Send Order";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.labelTags);
			this.groupBox2.Location = new System.Drawing.Point(54, 569);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(410, 69);
			this.groupBox2.TabIndex = 115;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Template Replacement Tags";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.textTemplateSms);
			this.groupBox3.Location = new System.Drawing.Point(54, 268);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(410, 89);
			this.groupBox3.TabIndex = 119;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Text Message";
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.textTemplateSubject);
			this.groupBox4.Controls.Add(this.textTemplateEmail);
			this.groupBox4.Location = new System.Drawing.Point(54, 361);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(410, 204);
			this.groupBox4.TabIndex = 120;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Email Subject and Body";
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.labelRuleType);
			this.groupBox5.Location = new System.Drawing.Point(54, 12);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(410, 45);
			this.groupBox5.TabIndex = 121;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Reminder Rule Type";
			// 
			// labelRuleType
			// 
			this.labelRuleType.Location = new System.Drawing.Point(7, 15);
			this.labelRuleType.Name = "labelRuleType";
			this.labelRuleType.Size = new System.Drawing.Size(397, 21);
			this.labelRuleType.TabIndex = 16;
			this.labelRuleType.Text = "labelRuleType";
			this.labelRuleType.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// textTime
			// 
			this.textTime.Location = new System.Drawing.Point(177, 18);
			this.textTime.MaxVal = 366;
			this.textTime.MinVal = 0;
			this.textTime.Name = "textTime";
			this.textTime.Size = new System.Drawing.Size(51, 20);
			this.textTime.TabIndex = 16;
			// 
			// FormApptReminderRuleEdit
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(509, 686);
			this.Controls.Add(this.groupBox5);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupSendOrder);
			this.Controls.Add(this.butDelete);
			this.Controls.Add(this.butOk);
			this.Controls.Add(this.groupBox12);
			this.Controls.Add(this.butCancel);
			this.Controls.Add(this.groupBox4);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormApptReminderRuleEdit";
			this.Text = "Appointment Reminder Rule";
			this.Load += new System.EventHandler(this.FormApptReminderRuleEdit_Load);
			this.groupBox12.ResumeLayout(false);
			this.groupBox12.PerformLayout();
			this.groupSendOrder.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button butCancel;
		private System.Windows.Forms.RichTextBox textTemplateEmail;
		private System.Windows.Forms.RichTextBox textTemplateSms;
		private System.Windows.Forms.Label labelLeadTime;
		private System.Windows.Forms.Button butOk;
		private UI.ODGrid gridPriorities;
		private UI.Button butUp;
		private UI.Button butDown;
		private System.Windows.Forms.CheckBox checkSendAll;
		private System.Windows.Forms.Label labelTags;
		private UI.Button butDelete;
		private System.Windows.Forms.RichTextBox textTemplateSubject;
		private System.Windows.Forms.GroupBox groupBox12;
		private System.Windows.Forms.GroupBox groupSendOrder;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.Label labelRuleType;
		private ValidNum textTime;
	}
}