namespace OpenDental{
	partial class FormInsVerificationSetup {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormInsVerificationSetup));
			this.butOK = new OpenDental.UI.Button();
			this.butCancel = new OpenDental.UI.Button();
			this.label23 = new System.Windows.Forms.Label();
			this.textInsBenefitEligibilityDays = new OpenDental.ValidNumber();
			this.textPatientEnrollmentDays = new OpenDental.ValidNumber();
			this.label29 = new System.Windows.Forms.Label();
			this.textScheduledAppointmentDays = new OpenDental.ValidNumber();
			this.checkInsVerifyUseCurrentUser = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label4 = new System.Windows.Forms.Label();
			this.checkInsVerifyExcludePatVerify = new System.Windows.Forms.CheckBox();
			this.checkExcludePatientClones = new System.Windows.Forms.CheckBox();
			this.groupBox1.SuspendLayout();
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
			this.butOK.Location = new System.Drawing.Point(204, 203);
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
			this.butCancel.Location = new System.Drawing.Point(285, 203);
			this.butCancel.Name = "butCancel";
			this.butCancel.Size = new System.Drawing.Size(75, 24);
			this.butCancel.TabIndex = 2;
			this.butCancel.Text = "&Cancel";
			this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
			// 
			// label23
			// 
			this.label23.Location = new System.Drawing.Point(6, 42);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(250, 20);
			this.label23.TabIndex = 75;
			this.label23.Text = "Plan benefits haven\'t been verified in";
			this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textInsBenefitEligibilityDays
			// 
			this.textInsBenefitEligibilityDays.Location = new System.Drawing.Point(262, 42);
			this.textInsBenefitEligibilityDays.MaxVal = 99999;
			this.textInsBenefitEligibilityDays.MinVal = 0;
			this.textInsBenefitEligibilityDays.Name = "textInsBenefitEligibilityDays";
			this.textInsBenefitEligibilityDays.Size = new System.Drawing.Size(32, 20);
			this.textInsBenefitEligibilityDays.TabIndex = 76;
			this.textInsBenefitEligibilityDays.Text = "0";
			// 
			// textPatientEnrollmentDays
			// 
			this.textPatientEnrollmentDays.Location = new System.Drawing.Point(262, 68);
			this.textPatientEnrollmentDays.MaxVal = 99999;
			this.textPatientEnrollmentDays.MinVal = 0;
			this.textPatientEnrollmentDays.Name = "textPatientEnrollmentDays";
			this.textPatientEnrollmentDays.Size = new System.Drawing.Size(32, 20);
			this.textPatientEnrollmentDays.TabIndex = 84;
			this.textPatientEnrollmentDays.Text = "0";
			// 
			// label29
			// 
			this.label29.Location = new System.Drawing.Point(6, 68);
			this.label29.Name = "label29";
			this.label29.Size = new System.Drawing.Size(250, 20);
			this.label29.TabIndex = 83;
			this.label29.Text = "Patient eligibility hasn\'t been verified in";
			this.label29.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textScheduledAppointmentDays
			// 
			this.textScheduledAppointmentDays.Location = new System.Drawing.Point(262, 16);
			this.textScheduledAppointmentDays.MaxVal = 99999;
			this.textScheduledAppointmentDays.MinVal = 0;
			this.textScheduledAppointmentDays.Name = "textScheduledAppointmentDays";
			this.textScheduledAppointmentDays.Size = new System.Drawing.Size(32, 20);
			this.textScheduledAppointmentDays.TabIndex = 86;
			this.textScheduledAppointmentDays.Text = "0";
			// 
			// checkInsVerifyUseCurrentUser
			// 
			this.checkInsVerifyUseCurrentUser.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkInsVerifyUseCurrentUser.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkInsVerifyUseCurrentUser.Location = new System.Drawing.Point(12, 124);
			this.checkInsVerifyUseCurrentUser.Name = "checkInsVerifyUseCurrentUser";
			this.checkInsVerifyUseCurrentUser.Size = new System.Drawing.Size(346, 17);
			this.checkInsVerifyUseCurrentUser.TabIndex = 226;
			this.checkInsVerifyUseCurrentUser.Text = "Insurance Verification List defaults to the current user";
			this.checkInsVerifyUseCurrentUser.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(303, 42);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(36, 20);
			this.label2.TabIndex = 228;
			this.label2.Text = "days";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(303, 68);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(36, 20);
			this.label3.TabIndex = 229;
			this.label3.Text = "days";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(9, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(247, 20);
			this.label1.TabIndex = 230;
			this.label1.Text = "Scheduled appointment in";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.textPatientEnrollmentDays);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.textScheduledAppointmentDays);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label23);
			this.groupBox1.Controls.Add(this.label29);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.textInsBenefitEligibilityDays);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(346, 106);
			this.groupBox1.TabIndex = 231;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Show In List When";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(303, 16);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(36, 20);
			this.label4.TabIndex = 231;
			this.label4.Text = "days";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// checkInsVerifyExcludePatVerify
			// 
			this.checkInsVerifyExcludePatVerify.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkInsVerifyExcludePatVerify.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkInsVerifyExcludePatVerify.Location = new System.Drawing.Point(12, 142);
			this.checkInsVerifyExcludePatVerify.Name = "checkInsVerifyExcludePatVerify";
			this.checkInsVerifyExcludePatVerify.Size = new System.Drawing.Size(346, 17);
			this.checkInsVerifyExcludePatVerify.TabIndex = 232;
			this.checkInsVerifyExcludePatVerify.Text = "Exclude patients with insurance plans marked as Do Not Verify";
			this.checkInsVerifyExcludePatVerify.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkExcludePatientClones
			// 
			this.checkExcludePatientClones.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkExcludePatientClones.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkExcludePatientClones.Location = new System.Drawing.Point(12, 160);
			this.checkExcludePatientClones.Name = "checkExcludePatientClones";
			this.checkExcludePatientClones.Size = new System.Drawing.Size(346, 17);
			this.checkExcludePatientClones.TabIndex = 233;
			this.checkExcludePatientClones.Text = "Exclude Patient Clones";
			this.checkExcludePatientClones.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// FormInsVerificationSetup
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(372, 239);
			this.Controls.Add(this.checkExcludePatientClones);
			this.Controls.Add(this.checkInsVerifyExcludePatVerify);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.checkInsVerifyUseCurrentUser);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.butCancel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "FormInsVerificationSetup";
			this.Text = "Insurance Verification Setup";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormInsVerificationSetup_FormClosing);
			this.Load += new System.EventHandler(this.FormInsVerificationSetup_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private OpenDental.UI.Button butOK;
		private OpenDental.UI.Button butCancel;
        private System.Windows.Forms.Label label23;
        private ValidNumber textInsBenefitEligibilityDays;
        private System.Windows.Forms.Label label29;
        private ValidNumber textScheduledAppointmentDays;
        private ValidNumber textPatientEnrollmentDays;
		private System.Windows.Forms.CheckBox checkInsVerifyUseCurrentUser;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckBox checkInsVerifyExcludePatVerify;
		private System.Windows.Forms.CheckBox checkExcludePatientClones;
	}
}