namespace OpenDental{
	partial class FormInsVerificationList {
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormInsVerificationList));
			this.label23 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.butRefresh = new OpenDental.UI.Button();
			this.comboVerifyUser = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.comboVerifyClinics = new System.Windows.Forms.ComboBox();
			this.labelClinic = new System.Windows.Forms.Label();
			this.comboFilterVerifyStatus = new System.Windows.Forms.ComboBox();
			this.label15 = new System.Windows.Forms.Label();
			this.textVerifyCarrier = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.comboVerifyRegions = new System.Windows.Forms.ComboBox();
			this.labelRegion = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.butVerifyUserPick = new OpenDental.UI.Button();
			this.textAppointmentScheduledDays = new OpenDental.ValidNumber();
			this.textPatientEnrollmentDays = new OpenDental.ValidNumber();
			this.textInsBenefitEligibilityDays = new OpenDental.ValidNumber();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.textSubscriberID = new System.Windows.Forms.TextBox();
			this.textSubscriberSSN = new System.Windows.Forms.TextBox();
			this.textSubscriberBirthdate = new System.Windows.Forms.TextBox();
			this.textSubscriberName = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.textInsPlanNote = new System.Windows.Forms.TextBox();
			this.textCarrierPhoneNumber = new System.Windows.Forms.TextBox();
			this.textInsPlanGroupNumber = new System.Windows.Forms.TextBox();
			this.textInsPlanGroupName = new System.Windows.Forms.TextBox();
			this.textInsPlanEmployer = new System.Windows.Forms.TextBox();
			this.textCarrierName = new System.Windows.Forms.TextBox();
			this.label17 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.textAssignUser = new System.Windows.Forms.TextBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.textInsVerifyNote = new System.Windows.Forms.TextBox();
			this.label19 = new System.Windows.Forms.Label();
			this.butAssignUserPick = new OpenDental.UI.Button();
			this.timerRefresh = new System.Windows.Forms.Timer(this.components);
			this.gridMain = new OpenDental.UI.ODGrid();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabVerify = new System.Windows.Forms.TabPage();
			this.butVerifyPat = new OpenDental.UI.Button();
			this.textPatBirthdate = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.comboSetVerifyStatus = new System.Windows.Forms.ComboBox();
			this.butVerifyPlan = new OpenDental.UI.Button();
			this.label16 = new System.Windows.Forms.Label();
			this.textInsVerifyReadOnlyNote = new System.Windows.Forms.TextBox();
			this.label20 = new System.Windows.Forms.Label();
			this.tabAssign = new System.Windows.Forms.TabPage();
			this.gridAssign = new OpenDental.UI.ODGrid();
			this.butAssignUser = new OpenDental.UI.Button();
			this._menuRightClick = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.butClose = new OpenDental.UI.Button();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabVerify.SuspendLayout();
			this.tabAssign.SuspendLayout();
			this.SuspendLayout();
			// 
			// label23
			// 
			this.label23.Location = new System.Drawing.Point(9, 11);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(261, 20);
			this.label23.TabIndex = 77;
			this.label23.Text = "Days until scheduled appointment:";
			this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(9, 55);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(261, 20);
			this.label4.TabIndex = 88;
			this.label4.Text = "Days since patient eligibility:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.butRefresh);
			this.groupBox3.Controls.Add(this.comboVerifyUser);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Controls.Add(this.comboVerifyClinics);
			this.groupBox3.Controls.Add(this.labelClinic);
			this.groupBox3.Controls.Add(this.comboFilterVerifyStatus);
			this.groupBox3.Controls.Add(this.label15);
			this.groupBox3.Controls.Add(this.textVerifyCarrier);
			this.groupBox3.Controls.Add(this.label6);
			this.groupBox3.Controls.Add(this.comboVerifyRegions);
			this.groupBox3.Controls.Add(this.labelRegion);
			this.groupBox3.Controls.Add(this.label2);
			this.groupBox3.Controls.Add(this.butVerifyUserPick);
			this.groupBox3.Controls.Add(this.label23);
			this.groupBox3.Controls.Add(this.label4);
			this.groupBox3.Controls.Add(this.textAppointmentScheduledDays);
			this.groupBox3.Controls.Add(this.textPatientEnrollmentDays);
			this.groupBox3.Controls.Add(this.textInsBenefitEligibilityDays);
			this.groupBox3.Location = new System.Drawing.Point(3, 4);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(969, 82);
			this.groupBox3.TabIndex = 90;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Verification Filters";
			// 
			// butRefresh
			// 
			this.butRefresh.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butRefresh.Autosize = true;
			this.butRefresh.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butRefresh.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butRefresh.CornerRadius = 4F;
			this.butRefresh.Location = new System.Drawing.Point(810, 55);
			this.butRefresh.Name = "butRefresh";
			this.butRefresh.Size = new System.Drawing.Size(75, 24);
			this.butRefresh.TabIndex = 232;
			this.butRefresh.Text = "Refresh";
			this.butRefresh.UseVisualStyleBackColor = true;
			this.butRefresh.Click += new System.EventHandler(this.butRefresh_Click);
			// 
			// comboVerifyUser
			// 
			this.comboVerifyUser.FormattingEnabled = true;
			this.comboVerifyUser.Location = new System.Drawing.Point(733, 10);
			this.comboVerifyUser.Name = "comboVerifyUser";
			this.comboVerifyUser.Size = new System.Drawing.Size(124, 21);
			this.comboVerifyUser.TabIndex = 231;
			this.comboVerifyUser.SelectionChangeCommitted += new System.EventHandler(this.comboVerifyUser_SelectionChangeCommitted);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(19, 32);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(250, 20);
			this.label3.TabIndex = 230;
			this.label3.Text = "Plan benefits haven\'t been verified in";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboVerifyClinics
			// 
			this.comboVerifyClinics.FormattingEnabled = true;
			this.comboVerifyClinics.Location = new System.Drawing.Point(451, 56);
			this.comboVerifyClinics.Name = "comboVerifyClinics";
			this.comboVerifyClinics.Size = new System.Drawing.Size(150, 21);
			this.comboVerifyClinics.TabIndex = 229;
			this.comboVerifyClinics.SelectionChangeCommitted += new System.EventHandler(this.comboVerifyClinic_SelectionChangeCommitted);
			// 
			// labelClinic
			// 
			this.labelClinic.Location = new System.Drawing.Point(373, 56);
			this.labelClinic.Name = "labelClinic";
			this.labelClinic.Size = new System.Drawing.Size(75, 20);
			this.labelClinic.TabIndex = 228;
			this.labelClinic.Text = "Clinic:";
			this.labelClinic.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboFilterVerifyStatus
			// 
			this.comboFilterVerifyStatus.FormattingEnabled = true;
			this.comboFilterVerifyStatus.Location = new System.Drawing.Point(733, 33);
			this.comboFilterVerifyStatus.Name = "comboFilterVerifyStatus";
			this.comboFilterVerifyStatus.Size = new System.Drawing.Size(152, 21);
			this.comboFilterVerifyStatus.TabIndex = 227;
			this.comboFilterVerifyStatus.SelectionChangeCommitted += new System.EventHandler(this.comboFilterVerifyStatus_SelectionChangeCommitted);
			// 
			// label15
			// 
			this.label15.Location = new System.Drawing.Point(607, 34);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(120, 20);
			this.label15.TabIndex = 226;
			this.label15.Text = "Verify Status:";
			this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textVerifyCarrier
			// 
			this.textVerifyCarrier.Location = new System.Drawing.Point(451, 11);
			this.textVerifyCarrier.Name = "textVerifyCarrier";
			this.textVerifyCarrier.Size = new System.Drawing.Size(150, 20);
			this.textVerifyCarrier.TabIndex = 225;
			this.textVerifyCarrier.TextChanged += new System.EventHandler(this.textVerifyCarrier_TextChanged);
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(373, 11);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(75, 20);
			this.label6.TabIndex = 223;
			this.label6.Text = "Carrier:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboVerifyRegions
			// 
			this.comboVerifyRegions.FormattingEnabled = true;
			this.comboVerifyRegions.Location = new System.Drawing.Point(451, 33);
			this.comboVerifyRegions.Name = "comboVerifyRegions";
			this.comboVerifyRegions.Size = new System.Drawing.Size(150, 21);
			this.comboVerifyRegions.TabIndex = 222;
			this.comboVerifyRegions.SelectionChangeCommitted += new System.EventHandler(this.comboVerifyRegions_SelectionChangeCommitted);
			// 
			// labelRegion
			// 
			this.labelRegion.Location = new System.Drawing.Point(373, 33);
			this.labelRegion.Name = "labelRegion";
			this.labelRegion.Size = new System.Drawing.Size(75, 20);
			this.labelRegion.TabIndex = 221;
			this.labelRegion.Text = "Region:";
			this.labelRegion.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(607, 11);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(120, 20);
			this.label2.TabIndex = 220;
			this.label2.Text = "For User:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butVerifyUserPick
			// 
			this.butVerifyUserPick.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butVerifyUserPick.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.butVerifyUserPick.Autosize = true;
			this.butVerifyUserPick.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butVerifyUserPick.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butVerifyUserPick.CornerRadius = 4F;
			this.butVerifyUserPick.Location = new System.Drawing.Point(858, 10);
			this.butVerifyUserPick.Name = "butVerifyUserPick";
			this.butVerifyUserPick.Size = new System.Drawing.Size(27, 20);
			this.butVerifyUserPick.TabIndex = 219;
			this.butVerifyUserPick.Text = "...";
			this.butVerifyUserPick.Click += new System.EventHandler(this.butVerifyUserPick_Click);
			// 
			// textAppointmentScheduledDays
			// 
			this.textAppointmentScheduledDays.Location = new System.Drawing.Point(272, 11);
			this.textAppointmentScheduledDays.MaxVal = 99999;
			this.textAppointmentScheduledDays.MinVal = 0;
			this.textAppointmentScheduledDays.Name = "textAppointmentScheduledDays";
			this.textAppointmentScheduledDays.Size = new System.Drawing.Size(51, 20);
			this.textAppointmentScheduledDays.TabIndex = 78;
			this.textAppointmentScheduledDays.Text = "0";
			this.textAppointmentScheduledDays.TextChanged += new System.EventHandler(this.textAppointmentScheduledDays_TextChanged);
			// 
			// textPatientEnrollmentDays
			// 
			this.textPatientEnrollmentDays.Location = new System.Drawing.Point(272, 55);
			this.textPatientEnrollmentDays.MaxVal = 99999;
			this.textPatientEnrollmentDays.MinVal = 0;
			this.textPatientEnrollmentDays.Name = "textPatientEnrollmentDays";
			this.textPatientEnrollmentDays.Size = new System.Drawing.Size(51, 20);
			this.textPatientEnrollmentDays.TabIndex = 89;
			this.textPatientEnrollmentDays.Text = "0";
			this.textPatientEnrollmentDays.TextChanged += new System.EventHandler(this.textPatientEnrollmentDays_TextChanged);
			// 
			// textInsBenefitEligibilityDays
			// 
			this.textInsBenefitEligibilityDays.Location = new System.Drawing.Point(272, 33);
			this.textInsBenefitEligibilityDays.MaxVal = 99999;
			this.textInsBenefitEligibilityDays.MinVal = 0;
			this.textInsBenefitEligibilityDays.Name = "textInsBenefitEligibilityDays";
			this.textInsBenefitEligibilityDays.Size = new System.Drawing.Size(51, 20);
			this.textInsBenefitEligibilityDays.TabIndex = 87;
			this.textInsBenefitEligibilityDays.Text = "0";
			this.textInsBenefitEligibilityDays.TextChanged += new System.EventHandler(this.textInsBenefitEligibilityDays_TextChanged);
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.textSubscriberID);
			this.groupBox4.Controls.Add(this.textSubscriberSSN);
			this.groupBox4.Controls.Add(this.textSubscriberBirthdate);
			this.groupBox4.Controls.Add(this.textSubscriberName);
			this.groupBox4.Controls.Add(this.label10);
			this.groupBox4.Controls.Add(this.label9);
			this.groupBox4.Controls.Add(this.label8);
			this.groupBox4.Controls.Add(this.label7);
			this.groupBox4.Location = new System.Drawing.Point(483, 349);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(232, 107);
			this.groupBox4.TabIndex = 91;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Subscriber";
			// 
			// textSubscriberID
			// 
			this.textSubscriberID.Location = new System.Drawing.Point(111, 79);
			this.textSubscriberID.Name = "textSubscriberID";
			this.textSubscriberID.ReadOnly = true;
			this.textSubscriberID.Size = new System.Drawing.Size(116, 20);
			this.textSubscriberID.TabIndex = 219;
			// 
			// textSubscriberSSN
			// 
			this.textSubscriberSSN.Location = new System.Drawing.Point(111, 57);
			this.textSubscriberSSN.Name = "textSubscriberSSN";
			this.textSubscriberSSN.ReadOnly = true;
			this.textSubscriberSSN.Size = new System.Drawing.Size(116, 20);
			this.textSubscriberSSN.TabIndex = 218;
			// 
			// textSubscriberBirthdate
			// 
			this.textSubscriberBirthdate.Location = new System.Drawing.Point(111, 35);
			this.textSubscriberBirthdate.Name = "textSubscriberBirthdate";
			this.textSubscriberBirthdate.ReadOnly = true;
			this.textSubscriberBirthdate.Size = new System.Drawing.Size(116, 20);
			this.textSubscriberBirthdate.TabIndex = 217;
			// 
			// textSubscriberName
			// 
			this.textSubscriberName.Location = new System.Drawing.Point(111, 13);
			this.textSubscriberName.Name = "textSubscriberName";
			this.textSubscriberName.ReadOnly = true;
			this.textSubscriberName.Size = new System.Drawing.Size(116, 20);
			this.textSubscriberName.TabIndex = 216;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(6, 79);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(99, 20);
			this.label10.TabIndex = 98;
			this.label10.Text = "SubscriberID";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(6, 57);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(99, 20);
			this.label9.TabIndex = 97;
			this.label9.Text = "SSN";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(6, 35);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(99, 20);
			this.label8.TabIndex = 96;
			this.label8.Text = "Birthdate";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(6, 13);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(99, 20);
			this.label7.TabIndex = 87;
			this.label7.Text = "Name";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.textInsPlanNote);
			this.groupBox5.Controls.Add(this.textCarrierPhoneNumber);
			this.groupBox5.Controls.Add(this.textInsPlanGroupNumber);
			this.groupBox5.Controls.Add(this.textInsPlanGroupName);
			this.groupBox5.Controls.Add(this.textInsPlanEmployer);
			this.groupBox5.Controls.Add(this.textCarrierName);
			this.groupBox5.Controls.Add(this.label17);
			this.groupBox5.Controls.Add(this.label18);
			this.groupBox5.Controls.Add(this.label11);
			this.groupBox5.Controls.Add(this.label12);
			this.groupBox5.Controls.Add(this.label13);
			this.groupBox5.Controls.Add(this.label14);
			this.groupBox5.Location = new System.Drawing.Point(3, 349);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(480, 131);
			this.groupBox5.TabIndex = 99;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Insurance Plan";
			// 
			// textInsPlanNote
			// 
			this.textInsPlanNote.Location = new System.Drawing.Point(249, 33);
			this.textInsPlanNote.Multiline = true;
			this.textInsPlanNote.Name = "textInsPlanNote";
			this.textInsPlanNote.ReadOnly = true;
			this.textInsPlanNote.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textInsPlanNote.Size = new System.Drawing.Size(225, 86);
			this.textInsPlanNote.TabIndex = 221;
			// 
			// textCarrierPhoneNumber
			// 
			this.textCarrierPhoneNumber.Location = new System.Drawing.Point(127, 33);
			this.textCarrierPhoneNumber.Name = "textCarrierPhoneNumber";
			this.textCarrierPhoneNumber.ReadOnly = true;
			this.textCarrierPhoneNumber.Size = new System.Drawing.Size(116, 20);
			this.textCarrierPhoneNumber.TabIndex = 220;
			// 
			// textInsPlanGroupNumber
			// 
			this.textInsPlanGroupNumber.Location = new System.Drawing.Point(127, 99);
			this.textInsPlanGroupNumber.Name = "textInsPlanGroupNumber";
			this.textInsPlanGroupNumber.ReadOnly = true;
			this.textInsPlanGroupNumber.Size = new System.Drawing.Size(116, 20);
			this.textInsPlanGroupNumber.TabIndex = 219;
			// 
			// textInsPlanGroupName
			// 
			this.textInsPlanGroupName.Location = new System.Drawing.Point(127, 77);
			this.textInsPlanGroupName.Name = "textInsPlanGroupName";
			this.textInsPlanGroupName.ReadOnly = true;
			this.textInsPlanGroupName.Size = new System.Drawing.Size(116, 20);
			this.textInsPlanGroupName.TabIndex = 218;
			// 
			// textInsPlanEmployer
			// 
			this.textInsPlanEmployer.Location = new System.Drawing.Point(127, 55);
			this.textInsPlanEmployer.Name = "textInsPlanEmployer";
			this.textInsPlanEmployer.ReadOnly = true;
			this.textInsPlanEmployer.Size = new System.Drawing.Size(116, 20);
			this.textInsPlanEmployer.TabIndex = 217;
			// 
			// textCarrierName
			// 
			this.textCarrierName.Location = new System.Drawing.Point(127, 11);
			this.textCarrierName.Name = "textCarrierName";
			this.textCarrierName.ReadOnly = true;
			this.textCarrierName.Size = new System.Drawing.Size(116, 20);
			this.textCarrierName.TabIndex = 216;
			// 
			// label17
			// 
			this.label17.Location = new System.Drawing.Point(249, 11);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(225, 20);
			this.label17.TabIndex = 104;
			this.label17.Text = "Plan Note";
			this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label18
			// 
			this.label18.Location = new System.Drawing.Point(8, 33);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(113, 20);
			this.label18.TabIndex = 99;
			this.label18.Text = "Carrier Phone";
			this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(8, 99);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(113, 20);
			this.label11.TabIndex = 98;
			this.label11.Text = "Group Number";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(8, 77);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(113, 20);
			this.label12.TabIndex = 97;
			this.label12.Text = "Group Name";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(8, 55);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(113, 20);
			this.label13.TabIndex = 96;
			this.label13.Text = "Employer Name";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(8, 11);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(113, 20);
			this.label14.TabIndex = 87;
			this.label14.Text = "Carrier Name";
			this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(6, 18);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(97, 20);
			this.label1.TabIndex = 86;
			this.label1.Text = "To User:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textAssignUser
			// 
			this.textAssignUser.Location = new System.Drawing.Point(109, 18);
			this.textAssignUser.Name = "textAssignUser";
			this.textAssignUser.ReadOnly = true;
			this.textAssignUser.Size = new System.Drawing.Size(165, 20);
			this.textAssignUser.TabIndex = 217;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.textInsVerifyNote);
			this.groupBox1.Controls.Add(this.label19);
			this.groupBox1.Controls.Add(this.textAssignUser);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.butAssignUserPick);
			this.groupBox1.Location = new System.Drawing.Point(3, 349);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(950, 134);
			this.groupBox1.TabIndex = 82;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Assign Verification";
			// 
			// textInsVerifyNote
			// 
			this.textInsVerifyNote.Location = new System.Drawing.Point(410, 17);
			this.textInsVerifyNote.Multiline = true;
			this.textInsVerifyNote.Name = "textInsVerifyNote";
			this.textInsVerifyNote.Size = new System.Drawing.Size(193, 107);
			this.textInsVerifyNote.TabIndex = 223;
			// 
			// label19
			// 
			this.label19.Location = new System.Drawing.Point(310, 16);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(94, 20);
			this.label19.TabIndex = 222;
			this.label19.Text = "Note:";
			this.label19.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butAssignUserPick
			// 
			this.butAssignUserPick.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butAssignUserPick.Autosize = true;
			this.butAssignUserPick.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butAssignUserPick.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butAssignUserPick.CornerRadius = 4F;
			this.butAssignUserPick.Location = new System.Drawing.Point(275, 18);
			this.butAssignUserPick.Name = "butAssignUserPick";
			this.butAssignUserPick.Size = new System.Drawing.Size(27, 20);
			this.butAssignUserPick.TabIndex = 85;
			this.butAssignUserPick.Text = "...";
			this.butAssignUserPick.Click += new System.EventHandler(this.butAssignUserPick_Click);
			// 
			// timerRefresh
			// 
			this.timerRefresh.Interval = 1000;
			this.timerRefresh.Tick += new System.EventHandler(this.timerRefresh_Tick);
			// 
			// gridMain
			// 
			this.gridMain.AllowSortingByColumn = true;
			this.gridMain.HasAddButton = false;
			this.gridMain.HasMultilineHeaders = false;
			this.gridMain.HeaderHeight = 15;
			this.gridMain.HScrollVisible = false;
			this.gridMain.Location = new System.Drawing.Point(3, 3);
			this.gridMain.Name = "gridMain";
			this.gridMain.ScrollValue = 0;
			this.gridMain.Size = new System.Drawing.Size(950, 343);
			this.gridMain.TabIndex = 4;
			this.gridMain.Title = "Insurance Verify List";
			this.gridMain.TitleHeight = 18;
			this.gridMain.TranslationName = "TableInsVerify";
			this.gridMain.CellDoubleClick += new OpenDental.UI.ODGridClickEventHandler(this.gridMain_CellDoubleClick);
			this.gridMain.CellClick += new OpenDental.UI.ODGridClickEventHandler(this.gridMain_CellClick);
			this.gridMain.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gridMain_MouseUp);
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabVerify);
			this.tabControl1.Controls.Add(this.tabAssign);
			this.tabControl1.Location = new System.Drawing.Point(3, 87);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(971, 541);
			this.tabControl1.TabIndex = 101;
			this.tabControl1.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl1_Selected);
			// 
			// tabVerify
			// 
			this.tabVerify.BackColor = System.Drawing.Color.Transparent;
			this.tabVerify.Controls.Add(this.butVerifyPat);
			this.tabVerify.Controls.Add(this.textPatBirthdate);
			this.tabVerify.Controls.Add(this.label5);
			this.tabVerify.Controls.Add(this.comboSetVerifyStatus);
			this.tabVerify.Controls.Add(this.butVerifyPlan);
			this.tabVerify.Controls.Add(this.label16);
			this.tabVerify.Controls.Add(this.textInsVerifyReadOnlyNote);
			this.tabVerify.Controls.Add(this.label20);
			this.tabVerify.Controls.Add(this.gridMain);
			this.tabVerify.Controls.Add(this.groupBox4);
			this.tabVerify.Controls.Add(this.groupBox5);
			this.tabVerify.Location = new System.Drawing.Point(4, 22);
			this.tabVerify.Name = "tabVerify";
			this.tabVerify.Padding = new System.Windows.Forms.Padding(3);
			this.tabVerify.Size = new System.Drawing.Size(963, 515);
			this.tabVerify.TabIndex = 0;
			this.tabVerify.Text = "Verification List";
			// 
			// butVerifyPat
			// 
			this.butVerifyPat.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butVerifyPat.Autosize = true;
			this.butVerifyPat.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butVerifyPat.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butVerifyPat.CornerRadius = 4F;
			this.butVerifyPat.Location = new System.Drawing.Point(294, 485);
			this.butVerifyPat.Name = "butVerifyPat";
			this.butVerifyPat.Size = new System.Drawing.Size(174, 24);
			this.butVerifyPat.TabIndex = 230;
			this.butVerifyPat.Text = "Mark Patient Eligibility Verified";
			this.butVerifyPat.UseVisualStyleBackColor = true;
			this.butVerifyPat.Click += new System.EventHandler(this.butVerifyPat_Click);
			// 
			// textPatBirthdate
			// 
			this.textPatBirthdate.Location = new System.Drawing.Point(594, 458);
			this.textPatBirthdate.Name = "textPatBirthdate";
			this.textPatBirthdate.ReadOnly = true;
			this.textPatBirthdate.Size = new System.Drawing.Size(116, 20);
			this.textPatBirthdate.TabIndex = 229;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(489, 458);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(99, 20);
			this.label5.TabIndex = 228;
			this.label5.Text = "Pat Birthdate";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboSetVerifyStatus
			// 
			this.comboSetVerifyStatus.FormattingEnabled = true;
			this.comboSetVerifyStatus.Location = new System.Drawing.Point(801, 359);
			this.comboSetVerifyStatus.Name = "comboSetVerifyStatus";
			this.comboSetVerifyStatus.Size = new System.Drawing.Size(152, 21);
			this.comboSetVerifyStatus.TabIndex = 227;
			this.comboSetVerifyStatus.SelectionChangeCommitted += new System.EventHandler(this.comboSetVerifyStatus_SelectionChangeCommitted);
			// 
			// butVerifyPlan
			// 
			this.butVerifyPlan.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butVerifyPlan.Autosize = true;
			this.butVerifyPlan.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butVerifyPlan.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butVerifyPlan.CornerRadius = 4F;
			this.butVerifyPlan.Location = new System.Drawing.Point(515, 485);
			this.butVerifyPlan.Name = "butVerifyPlan";
			this.butVerifyPlan.Size = new System.Drawing.Size(153, 24);
			this.butVerifyPlan.TabIndex = 100;
			this.butVerifyPlan.Text = "Mark Ins Benefits Verified";
			this.butVerifyPlan.UseVisualStyleBackColor = true;
			this.butVerifyPlan.Click += new System.EventHandler(this.butVerifyPlan_Click);
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(717, 360);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(83, 20);
			this.label16.TabIndex = 226;
			this.label16.Text = "Verify Status";
			this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textInsVerifyReadOnlyNote
			// 
			this.textInsVerifyReadOnlyNote.Location = new System.Drawing.Point(801, 382);
			this.textInsVerifyReadOnlyNote.Multiline = true;
			this.textInsVerifyReadOnlyNote.Name = "textInsVerifyReadOnlyNote";
			this.textInsVerifyReadOnlyNote.ReadOnly = true;
			this.textInsVerifyReadOnlyNote.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textInsVerifyReadOnlyNote.Size = new System.Drawing.Size(152, 84);
			this.textInsVerifyReadOnlyNote.TabIndex = 225;
			// 
			// label20
			// 
			this.label20.Location = new System.Drawing.Point(716, 384);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(83, 20);
			this.label20.TabIndex = 224;
			this.label20.Text = "Status Note";
			this.label20.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tabAssign
			// 
			this.tabAssign.BackColor = System.Drawing.Color.Transparent;
			this.tabAssign.Controls.Add(this.gridAssign);
			this.tabAssign.Controls.Add(this.groupBox1);
			this.tabAssign.Controls.Add(this.butAssignUser);
			this.tabAssign.Location = new System.Drawing.Point(4, 22);
			this.tabAssign.Name = "tabAssign";
			this.tabAssign.Padding = new System.Windows.Forms.Padding(3);
			this.tabAssign.Size = new System.Drawing.Size(963, 515);
			this.tabAssign.TabIndex = 1;
			this.tabAssign.Text = "Assign Verification";
			// 
			// gridAssign
			// 
			this.gridAssign.AllowSortingByColumn = true;
			this.gridAssign.HasAddButton = false;
			this.gridAssign.HasMultilineHeaders = false;
			this.gridAssign.HeaderHeight = 15;
			this.gridAssign.HScrollVisible = false;
			this.gridAssign.Location = new System.Drawing.Point(3, 3);
			this.gridAssign.Name = "gridAssign";
			this.gridAssign.ScrollValue = 0;
			this.gridAssign.SelectionMode = OpenDental.UI.GridSelectionMode.MultiExtended;
			this.gridAssign.Size = new System.Drawing.Size(950, 343);
			this.gridAssign.TabIndex = 5;
			this.gridAssign.Title = "Insurance Verification Assignment List";
			this.gridAssign.TitleHeight = 18;
			this.gridAssign.TranslationName = "TableInsVerifyAssign";
			this.gridAssign.MouseUp += new System.Windows.Forms.MouseEventHandler(this.gridAssign_MouseUp);
			// 
			// butAssignUser
			// 
			this.butAssignUser.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butAssignUser.Autosize = true;
			this.butAssignUser.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butAssignUser.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butAssignUser.CornerRadius = 4F;
			this.butAssignUser.Location = new System.Drawing.Point(441, 486);
			this.butAssignUser.Name = "butAssignUser";
			this.butAssignUser.Size = new System.Drawing.Size(75, 24);
			this.butAssignUser.TabIndex = 81;
			this.butAssignUser.Text = "Assign";
			this.butAssignUser.UseVisualStyleBackColor = true;
			this.butAssignUser.Click += new System.EventHandler(this.butAssignUser_Click);
			// 
			// _menuRightClick
			// 
			this._menuRightClick.Name = "_menuRightClick";
			this._menuRightClick.Size = new System.Drawing.Size(61, 4);
			// 
			// butClose
			// 
			this.butClose.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butClose.Autosize = true;
			this.butClose.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butClose.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butClose.CornerRadius = 4F;
			this.butClose.Location = new System.Drawing.Point(887, 631);
			this.butClose.Name = "butClose";
			this.butClose.Size = new System.Drawing.Size(75, 24);
			this.butClose.TabIndex = 2;
			this.butClose.Text = "&Close";
			this.butClose.Click += new System.EventHandler(this.butClose_Click);
			// 
			// FormInsVerificationList
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(974, 658);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.butClose);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(990, 697);
			this.Name = "FormInsVerificationList";
			this.Text = "Insurance Verification List";
			this.Load += new System.EventHandler(this.FormInsVerificationList_Load);
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.groupBox5.ResumeLayout(false);
			this.groupBox5.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.tabControl1.ResumeLayout(false);
			this.tabVerify.ResumeLayout(false);
			this.tabVerify.PerformLayout();
			this.tabAssign.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion
		private OpenDental.UI.Button butClose;
        private UI.ODGrid gridMain;
        private System.Windows.Forms.Label label23;
        private ValidNumber textAppointmentScheduledDays;
        private ValidNumber textInsBenefitEligibilityDays;
        private System.Windows.Forms.Label label4;
        private ValidNumber textPatientEnrollmentDays;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox textSubscriberID;
        private System.Windows.Forms.TextBox textSubscriberSSN;
        private System.Windows.Forms.TextBox textSubscriberBirthdate;
        private System.Windows.Forms.TextBox textSubscriberName;
        private System.Windows.Forms.TextBox textInsPlanNote;
        private System.Windows.Forms.TextBox textCarrierPhoneNumber;
        private System.Windows.Forms.TextBox textInsPlanGroupNumber;
        private System.Windows.Forms.TextBox textInsPlanGroupName;
        private System.Windows.Forms.TextBox textInsPlanEmployer;
        private System.Windows.Forms.TextBox textCarrierName;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textAssignUser;
        private System.Windows.Forms.Label label1;
        private UI.Button butAssignUserPick;
        private UI.Button butAssignUser;
        private UI.Button butVerifyUserPick;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelRegion;
		private System.Windows.Forms.ComboBox comboVerifyRegions;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textVerifyCarrier;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ComboBox comboFilterVerifyStatus;
		private UI.Button butVerifyPlan;
		private System.Windows.Forms.Timer timerRefresh;
		private System.Windows.Forms.TextBox textInsVerifyNote;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabVerify;
		private System.Windows.Forms.TabPage tabAssign;
		private UI.ODGrid gridAssign;
		private System.Windows.Forms.TextBox textInsVerifyReadOnlyNote;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.ContextMenuStrip _menuRightClick;
		private System.Windows.Forms.ComboBox comboVerifyClinics;
		private System.Windows.Forms.Label labelClinic;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox comboVerifyUser;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.ComboBox comboSetVerifyStatus;
		private UI.Button butRefresh;
		private System.Windows.Forms.TextBox textPatBirthdate;
		private System.Windows.Forms.Label label5;
		private UI.Button butVerifyPat;
	}
}