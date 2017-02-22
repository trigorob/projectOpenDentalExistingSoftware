namespace OpenDental{
	partial class FormTaskCommSettings {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTaskCommSettings));
			this.butOK = new OpenDental.UI.Button();
			this.butCancel = new OpenDental.UI.Button();
			this.checkCommlogOverride = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.checkManualLaunch = new System.Windows.Forms.CheckBox();
			this.checkKeepCommPatSynched = new System.Windows.Forms.CheckBox();
			this.checkKeepTaskPatSynched = new System.Windows.Forms.CheckBox();
			this.checkClearTaskList = new System.Windows.Forms.CheckBox();
			this.checkAutoClearNotes = new System.Windows.Forms.CheckBox();
			this.checkUpdateDateTime = new System.Windows.Forms.CheckBox();
			this.checkClearEndTime = new System.Windows.Forms.CheckBox();
			this.checkClearDateTimeEndSave = new System.Windows.Forms.CheckBox();
			this.checkRefreshModuleSave = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// butOK
			// 
			this.butOK.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Autosize = true;
			this.butOK.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butOK.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butOK.CornerRadius = 4F;
			this.butOK.Location = new System.Drawing.Point(381, 364);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 24);
			this.butOK.TabIndex = 3;
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
			this.butCancel.Location = new System.Drawing.Point(462, 364);
			this.butCancel.Name = "butCancel";
			this.butCancel.Size = new System.Drawing.Size(75, 24);
			this.butCancel.TabIndex = 2;
			this.butCancel.Text = "&Cancel";
			this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
			// 
			// checkCommlogOverride
			// 
			this.checkCommlogOverride.Location = new System.Drawing.Point(12, 55);
			this.checkCommlogOverride.Name = "checkCommlogOverride";
			this.checkCommlogOverride.Size = new System.Drawing.Size(525, 23);
			this.checkCommlogOverride.TabIndex = 4;
			this.checkCommlogOverride.Text = "Use TaskComm instead of typical commlog window.";
			this.checkCommlogOverride.UseVisualStyleBackColor = true;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(520, 31);
			this.label1.TabIndex = 12;
			this.label1.Text = "All settings in this window are linked to the user currently logged in.\r\nThere ar" +
    "e no global settings for TaskComm.";
			// 
			// checkManualLaunch
			// 
			this.checkManualLaunch.Location = new System.Drawing.Point(12, 84);
			this.checkManualLaunch.Name = "checkManualLaunch";
			this.checkManualLaunch.Size = new System.Drawing.Size(525, 23);
			this.checkManualLaunch.TabIndex = 13;
			this.checkManualLaunch.Text = "Only launch when program link button clicked.";
			this.checkManualLaunch.UseVisualStyleBackColor = true;
			// 
			// checkKeepCommPatSynched
			// 
			this.checkKeepCommPatSynched.Location = new System.Drawing.Point(12, 113);
			this.checkKeepCommPatSynched.Name = "checkKeepCommPatSynched";
			this.checkKeepCommPatSynched.Size = new System.Drawing.Size(525, 23);
			this.checkKeepCommPatSynched.TabIndex = 14;
			this.checkKeepCommPatSynched.Text = "Keep Commlog Patient Synched";
			this.checkKeepCommPatSynched.UseVisualStyleBackColor = true;
			// 
			// checkKeepTaskPatSynched
			// 
			this.checkKeepTaskPatSynched.Location = new System.Drawing.Point(12, 142);
			this.checkKeepTaskPatSynched.Name = "checkKeepTaskPatSynched";
			this.checkKeepTaskPatSynched.Size = new System.Drawing.Size(525, 23);
			this.checkKeepTaskPatSynched.TabIndex = 15;
			this.checkKeepTaskPatSynched.Text = "Keep Task Patient Synched";
			this.checkKeepTaskPatSynched.UseVisualStyleBackColor = true;
			// 
			// checkClearTaskList
			// 
			this.checkClearTaskList.Location = new System.Drawing.Point(12, 229);
			this.checkClearTaskList.Name = "checkClearTaskList";
			this.checkClearTaskList.Size = new System.Drawing.Size(525, 23);
			this.checkClearTaskList.TabIndex = 16;
			this.checkClearTaskList.Text = "Clicking \"Clear\" will clear the task list as well.";
			this.checkClearTaskList.UseVisualStyleBackColor = true;
			// 
			// checkAutoClearNotes
			// 
			this.checkAutoClearNotes.Location = new System.Drawing.Point(12, 200);
			this.checkAutoClearNotes.Name = "checkAutoClearNotes";
			this.checkAutoClearNotes.Size = new System.Drawing.Size(525, 23);
			this.checkAutoClearNotes.TabIndex = 17;
			this.checkAutoClearNotes.Text = "Automatically clear the Note / Description text box after creating a task or comm" +
    "log.";
			this.checkAutoClearNotes.UseVisualStyleBackColor = true;
			// 
			// checkUpdateDateTime
			// 
			this.checkUpdateDateTime.Location = new System.Drawing.Point(12, 171);
			this.checkUpdateDateTime.Name = "checkUpdateDateTime";
			this.checkUpdateDateTime.Size = new System.Drawing.Size(525, 23);
			this.checkUpdateDateTime.TabIndex = 18;
			this.checkUpdateDateTime.Text = "Update Date / Time With New Patient";
			this.checkUpdateDateTime.UseVisualStyleBackColor = true;
			// 
			// checkClearEndTime
			// 
			this.checkClearEndTime.Location = new System.Drawing.Point(12, 258);
			this.checkClearEndTime.Name = "checkClearEndTime";
			this.checkClearEndTime.Size = new System.Drawing.Size(525, 23);
			this.checkClearEndTime.TabIndex = 19;
			this.checkClearEndTime.Text = "Clicking \"Clear\" will clear the end date and time as well.";
			this.checkClearEndTime.UseVisualStyleBackColor = true;
			// 
			// checkClearDateTimeEndSave
			// 
			this.checkClearDateTimeEndSave.Location = new System.Drawing.Point(12, 287);
			this.checkClearDateTimeEndSave.Name = "checkClearDateTimeEndSave";
			this.checkClearDateTimeEndSave.Size = new System.Drawing.Size(525, 23);
			this.checkClearDateTimeEndSave.TabIndex = 20;
			this.checkClearDateTimeEndSave.Text = "Automatically clear the end date and time field when a commlog or task is saved.";
			this.checkClearDateTimeEndSave.UseVisualStyleBackColor = true;
			// 
			// checkRefreshModuleSave
			// 
			this.checkRefreshModuleSave.Location = new System.Drawing.Point(12, 316);
			this.checkRefreshModuleSave.Name = "checkRefreshModuleSave";
			this.checkRefreshModuleSave.Size = new System.Drawing.Size(525, 23);
			this.checkRefreshModuleSave.TabIndex = 21;
			this.checkRefreshModuleSave.Text = "Automatically refresh the currently selected module on save.";
			this.checkRefreshModuleSave.UseVisualStyleBackColor = true;
			// 
			// FormTaskCommSettings
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(549, 400);
			this.Controls.Add(this.checkRefreshModuleSave);
			this.Controls.Add(this.checkClearDateTimeEndSave);
			this.Controls.Add(this.checkClearEndTime);
			this.Controls.Add(this.checkUpdateDateTime);
			this.Controls.Add(this.checkAutoClearNotes);
			this.Controls.Add(this.checkClearTaskList);
			this.Controls.Add(this.checkKeepTaskPatSynched);
			this.Controls.Add(this.checkKeepCommPatSynched);
			this.Controls.Add(this.checkManualLaunch);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.checkCommlogOverride);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.butCancel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(565, 385);
			this.Name = "FormTaskCommSettings";
			this.Text = "TaskComm Settings";
			this.ResumeLayout(false);

		}

		#endregion

		private OpenDental.UI.Button butOK;
		private OpenDental.UI.Button butCancel;
		private System.Windows.Forms.CheckBox checkCommlogOverride;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox checkManualLaunch;
		private System.Windows.Forms.CheckBox checkKeepCommPatSynched;
		private System.Windows.Forms.CheckBox checkKeepTaskPatSynched;
		private System.Windows.Forms.CheckBox checkClearTaskList;
		private System.Windows.Forms.CheckBox checkAutoClearNotes;
		private System.Windows.Forms.CheckBox checkUpdateDateTime;
		private System.Windows.Forms.CheckBox checkClearEndTime;
		private System.Windows.Forms.CheckBox checkClearDateTimeEndSave;
		private System.Windows.Forms.CheckBox checkRefreshModuleSave;
	}
}