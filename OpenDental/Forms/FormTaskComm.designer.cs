namespace OpenDental{
	partial class FormTaskComm {
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTaskComm));
			this.butCommlog = new OpenDental.UI.Button();
			this.butBoth = new OpenDental.UI.Button();
			this.butTask = new OpenDental.UI.Button();
			this.butClear = new OpenDental.UI.Button();
			this.textNote = new OpenDental.ODtextBox();
			this.labelDescription = new System.Windows.Forms.Label();
			this.labelSavedTaskComm = new System.Windows.Forms.Label();
			this.groupBoth = new System.Windows.Forms.GroupBox();
			this.butNowEnd = new OpenDental.UI.Button();
			this.butNow = new OpenDental.UI.Button();
			this.label16 = new System.Windows.Forms.Label();
			this.textDateTime = new System.Windows.Forms.TextBox();
			this.labelDateTimeEnd = new System.Windows.Forms.Label();
			this.textUser = new System.Windows.Forms.TextBox();
			this.textDateTimeEnd = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.groupTask = new System.Windows.Forms.GroupBox();
			this.butCurrentPatTask = new OpenDental.UI.Button();
			this.butTaskListOne = new OpenDental.UI.Button();
			this.butChangeTaskPat = new OpenDental.UI.Button();
			this.butTaskListAdd = new OpenDental.UI.Button();
			this.butTaskListClear = new OpenDental.UI.Button();
			this.butColor = new System.Windows.Forms.Button();
			this.label8 = new System.Windows.Forms.Label();
			this.comboTaskPriorities = new System.Windows.Forms.ComboBox();
			this.labelPatDiffTask = new System.Windows.Forms.Label();
			this.listObjectType = new System.Windows.Forms.ListBox();
			this.label7 = new System.Windows.Forms.Label();
			this.textObjectDesc = new System.Windows.Forms.TextBox();
			this.textTaskList = new System.Windows.Forms.TextBox();
			this.labelObjectDesc = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.groupCommlog = new System.Windows.Forms.GroupBox();
			this.butCurrentPatCommlog = new OpenDental.UI.Button();
			this.butChangeCommPat = new OpenDental.UI.Button();
			this.labelPatDiffCommlog = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.listType = new System.Windows.Forms.ListBox();
			this.listSentOrReceived = new System.Windows.Forms.ListBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.listMode = new System.Windows.Forms.ListBox();
			this.textPatientName = new System.Windows.Forms.TextBox();
			this.timerSaved = new System.Windows.Forms.Timer(this.components);
			this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
			this.menuItemTools = new System.Windows.Forms.MenuItem();
			this.menuItemOnTop = new System.Windows.Forms.MenuItem();
			this.menuItemSettings = new System.Windows.Forms.MenuItem();
			this.menuItemAddWindow = new System.Windows.Forms.MenuItem();
			this.menuItemProgNotes = new System.Windows.Forms.MenuItem();
			this.menuItemTaskList = new System.Windows.Forms.MenuItem();
			this.groupBoth.SuspendLayout();
			this.groupTask.SuspendLayout();
			this.groupCommlog.SuspendLayout();
			this.SuspendLayout();
			// 
			// butCommlog
			// 
			this.butCommlog.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butCommlog.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.butCommlog.Autosize = true;
			this.butCommlog.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butCommlog.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butCommlog.CornerRadius = 4F;
			this.butCommlog.Location = new System.Drawing.Point(148, 580);
			this.butCommlog.Name = "butCommlog";
			this.butCommlog.Size = new System.Drawing.Size(75, 24);
			this.butCommlog.TabIndex = 4;
			this.butCommlog.Text = "Commlog";
			// 
			// butBoth
			// 
			this.butBoth.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butBoth.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.butBoth.Autosize = true;
			this.butBoth.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butBoth.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butBoth.CornerRadius = 4F;
			this.butBoth.Location = new System.Drawing.Point(312, 580);
			this.butBoth.Name = "butBoth";
			this.butBoth.Size = new System.Drawing.Size(75, 24);
			this.butBoth.TabIndex = 5;
			this.butBoth.Text = "Both";
			// 
			// butTask
			// 
			this.butTask.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butTask.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.butTask.Autosize = true;
			this.butTask.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butTask.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butTask.CornerRadius = 4F;
			this.butTask.Location = new System.Drawing.Point(476, 580);
			this.butTask.Name = "butTask";
			this.butTask.Size = new System.Drawing.Size(75, 24);
			this.butTask.TabIndex = 6;
			this.butTask.Text = "Task";
			// 
			// butClear
			// 
			this.butClear.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butClear.Autosize = true;
			this.butClear.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butClear.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butClear.CornerRadius = 4F;
			this.butClear.Location = new System.Drawing.Point(31, 553);
			this.butClear.Name = "butClear";
			this.butClear.Size = new System.Drawing.Size(75, 24);
			this.butClear.TabIndex = 7;
			this.butClear.Text = "Clear";
			// 
			// textNote
			// 
			this.textNote.AcceptsTab = true;
			this.textNote.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textNote.BackColor = System.Drawing.SystemColors.Window;
			this.textNote.DetectLinksEnabled = false;
			this.textNote.DetectUrls = false;
			this.textNote.Location = new System.Drawing.Point(32, 409);
			this.textNote.Name = "textNote";
			this.textNote.QuickPasteType = OpenDentBusiness.QuickPasteType.CommLog;
			this.textNote.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.textNote.Size = new System.Drawing.Size(635, 138);
			this.textNote.TabIndex = 171;
			this.textNote.Text = "";
			// 
			// labelDescription
			// 
			this.labelDescription.Location = new System.Drawing.Point(28, 390);
			this.labelDescription.Name = "labelDescription";
			this.labelDescription.Size = new System.Drawing.Size(270, 16);
			this.labelDescription.TabIndex = 170;
			this.labelDescription.Text = "Commlog Note / Task Description";
			this.labelDescription.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// labelSavedTaskComm
			// 
			this.labelSavedTaskComm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.labelSavedTaskComm.ForeColor = System.Drawing.Color.Red;
			this.labelSavedTaskComm.Location = new System.Drawing.Point(428, 553);
			this.labelSavedTaskComm.Name = "labelSavedTaskComm";
			this.labelSavedTaskComm.Size = new System.Drawing.Size(239, 22);
			this.labelSavedTaskComm.TabIndex = 178;
			this.labelSavedTaskComm.Text = "Saved.";
			this.labelSavedTaskComm.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.labelSavedTaskComm.Visible = false;
			// 
			// groupBoth
			// 
			this.groupBoth.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.groupBoth.Controls.Add(this.butNowEnd);
			this.groupBoth.Controls.Add(this.butNow);
			this.groupBoth.Controls.Add(this.label16);
			this.groupBoth.Controls.Add(this.textDateTime);
			this.groupBoth.Controls.Add(this.labelDateTimeEnd);
			this.groupBoth.Controls.Add(this.textUser);
			this.groupBoth.Controls.Add(this.textDateTimeEnd);
			this.groupBoth.Controls.Add(this.label1);
			this.groupBoth.Location = new System.Drawing.Point(148, 7);
			this.groupBoth.Name = "groupBoth";
			this.groupBoth.Size = new System.Drawing.Size(394, 106);
			this.groupBoth.TabIndex = 179;
			this.groupBoth.TabStop = false;
			this.groupBoth.Text = "Both";
			// 
			// butNowEnd
			// 
			this.butNowEnd.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butNowEnd.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.butNowEnd.Autosize = true;
			this.butNowEnd.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butNowEnd.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butNowEnd.CornerRadius = 4F;
			this.butNowEnd.Location = new System.Drawing.Point(315, 70);
			this.butNowEnd.Name = "butNowEnd";
			this.butNowEnd.Size = new System.Drawing.Size(39, 20);
			this.butNowEnd.TabIndex = 184;
			this.butNowEnd.Text = "Now";
			// 
			// butNow
			// 
			this.butNow.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butNow.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.butNow.Autosize = true;
			this.butNow.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butNow.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butNow.CornerRadius = 4F;
			this.butNow.Location = new System.Drawing.Point(315, 45);
			this.butNow.Name = "butNow";
			this.butNow.Size = new System.Drawing.Size(39, 20);
			this.butNow.TabIndex = 183;
			this.butNow.Text = "Now";
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(29, 20);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(73, 16);
			this.label16.TabIndex = 122;
			this.label16.Text = "User";
			this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDateTime
			// 
			this.textDateTime.Location = new System.Drawing.Point(104, 45);
			this.textDateTime.Name = "textDateTime";
			this.textDateTime.Size = new System.Drawing.Size(205, 20);
			this.textDateTime.TabIndex = 113;
			// 
			// labelDateTimeEnd
			// 
			this.labelDateTimeEnd.Location = new System.Drawing.Point(23, 72);
			this.labelDateTimeEnd.Name = "labelDateTimeEnd";
			this.labelDateTimeEnd.Size = new System.Drawing.Size(81, 18);
			this.labelDateTimeEnd.TabIndex = 118;
			this.labelDateTimeEnd.Text = "End";
			this.labelDateTimeEnd.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// textUser
			// 
			this.textUser.Location = new System.Drawing.Point(104, 19);
			this.textUser.Name = "textUser";
			this.textUser.ReadOnly = true;
			this.textUser.Size = new System.Drawing.Size(205, 20);
			this.textUser.TabIndex = 123;
			// 
			// textDateTimeEnd
			// 
			this.textDateTimeEnd.Location = new System.Drawing.Point(104, 70);
			this.textDateTimeEnd.Name = "textDateTimeEnd";
			this.textDateTimeEnd.Size = new System.Drawing.Size(205, 20);
			this.textDateTimeEnd.TabIndex = 119;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(23, 47);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(81, 18);
			this.label1.TabIndex = 110;
			this.label1.Text = "Date / Time";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// groupTask
			// 
			this.groupTask.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.groupTask.Controls.Add(this.butCurrentPatTask);
			this.groupTask.Controls.Add(this.butTaskListOne);
			this.groupTask.Controls.Add(this.butChangeTaskPat);
			this.groupTask.Controls.Add(this.butTaskListAdd);
			this.groupTask.Controls.Add(this.butTaskListClear);
			this.groupTask.Controls.Add(this.butColor);
			this.groupTask.Controls.Add(this.label8);
			this.groupTask.Controls.Add(this.comboTaskPriorities);
			this.groupTask.Controls.Add(this.labelPatDiffTask);
			this.groupTask.Controls.Add(this.listObjectType);
			this.groupTask.Controls.Add(this.label7);
			this.groupTask.Controls.Add(this.textObjectDesc);
			this.groupTask.Controls.Add(this.textTaskList);
			this.groupTask.Controls.Add(this.labelObjectDesc);
			this.groupTask.Controls.Add(this.label10);
			this.groupTask.Location = new System.Drawing.Point(367, 119);
			this.groupTask.Name = "groupTask";
			this.groupTask.Size = new System.Drawing.Size(300, 268);
			this.groupTask.TabIndex = 181;
			this.groupTask.TabStop = false;
			this.groupTask.Text = "Task";
			// 
			// butCurrentPatTask
			// 
			this.butCurrentPatTask.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butCurrentPatTask.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.butCurrentPatTask.Autosize = true;
			this.butCurrentPatTask.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butCurrentPatTask.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butCurrentPatTask.CornerRadius = 4F;
			this.butCurrentPatTask.Location = new System.Drawing.Point(200, 240);
			this.butCurrentPatTask.Name = "butCurrentPatTask";
			this.butCurrentPatTask.Size = new System.Drawing.Size(75, 22);
			this.butCurrentPatTask.TabIndex = 183;
			this.butCurrentPatTask.Text = "Current";
			// 
			// butTaskListOne
			// 
			this.butTaskListOne.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butTaskListOne.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.butTaskListOne.Autosize = true;
			this.butTaskListOne.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butTaskListOne.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butTaskListOne.CornerRadius = 4F;
			this.butTaskListOne.Location = new System.Drawing.Point(24, 95);
			this.butTaskListOne.Name = "butTaskListOne";
			this.butTaskListOne.Size = new System.Drawing.Size(39, 20);
			this.butTaskListOne.TabIndex = 184;
			this.butTaskListOne.Text = "One";
			// 
			// butChangeTaskPat
			// 
			this.butChangeTaskPat.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butChangeTaskPat.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.butChangeTaskPat.Autosize = true;
			this.butChangeTaskPat.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butChangeTaskPat.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butChangeTaskPat.CornerRadius = 4F;
			this.butChangeTaskPat.Location = new System.Drawing.Point(24, 240);
			this.butChangeTaskPat.Name = "butChangeTaskPat";
			this.butChangeTaskPat.Size = new System.Drawing.Size(75, 22);
			this.butChangeTaskPat.TabIndex = 184;
			this.butChangeTaskPat.Text = "Change";
			// 
			// butTaskListAdd
			// 
			this.butTaskListAdd.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butTaskListAdd.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.butTaskListAdd.Autosize = true;
			this.butTaskListAdd.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butTaskListAdd.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butTaskListAdd.CornerRadius = 4F;
			this.butTaskListAdd.Location = new System.Drawing.Point(130, 95);
			this.butTaskListAdd.Name = "butTaskListAdd";
			this.butTaskListAdd.Size = new System.Drawing.Size(39, 20);
			this.butTaskListAdd.TabIndex = 185;
			this.butTaskListAdd.Text = "Add";
			// 
			// butTaskListClear
			// 
			this.butTaskListClear.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butTaskListClear.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.butTaskListClear.Autosize = true;
			this.butTaskListClear.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butTaskListClear.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butTaskListClear.CornerRadius = 4F;
			this.butTaskListClear.Location = new System.Drawing.Point(236, 95);
			this.butTaskListClear.Name = "butTaskListClear";
			this.butTaskListClear.Size = new System.Drawing.Size(39, 20);
			this.butTaskListClear.TabIndex = 186;
			this.butTaskListClear.Text = "Clear";
			// 
			// butColor
			// 
			this.butColor.Enabled = false;
			this.butColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.butColor.Location = new System.Drawing.Point(24, 31);
			this.butColor.Name = "butColor";
			this.butColor.Size = new System.Drawing.Size(24, 21);
			this.butColor.TabIndex = 162;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(182, 12);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(94, 16);
			this.label8.TabIndex = 161;
			this.label8.Text = "Task Priority";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboTaskPriorities
			// 
			this.comboTaskPriorities.FormattingEnabled = true;
			this.comboTaskPriorities.Location = new System.Drawing.Point(54, 31);
			this.comboTaskPriorities.Name = "comboTaskPriorities";
			this.comboTaskPriorities.Size = new System.Drawing.Size(222, 21);
			this.comboTaskPriorities.TabIndex = 160;
			// 
			// labelPatDiffTask
			// 
			this.labelPatDiffTask.ForeColor = System.Drawing.Color.Red;
			this.labelPatDiffTask.Location = new System.Drawing.Point(21, 149);
			this.labelPatDiffTask.Name = "labelPatDiffTask";
			this.labelPatDiffTask.Size = new System.Drawing.Size(90, 48);
			this.labelPatDiffTask.TabIndex = 157;
			this.labelPatDiffTask.Text = "Different patient than the one selected in OD";
			this.labelPatDiffTask.Visible = false;
			// 
			// listObjectType
			// 
			this.listObjectType.Location = new System.Drawing.Point(156, 139);
			this.listObjectType.Name = "listObjectType";
			this.listObjectType.Size = new System.Drawing.Size(120, 43);
			this.listObjectType.TabIndex = 150;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(173, 122);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(103, 14);
			this.label7.TabIndex = 151;
			this.label7.Text = "Object Type";
			this.label7.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			// 
			// textObjectDesc
			// 
			this.textObjectDesc.Location = new System.Drawing.Point(24, 203);
			this.textObjectDesc.Multiline = true;
			this.textObjectDesc.Name = "textObjectDesc";
			this.textObjectDesc.Size = new System.Drawing.Size(252, 34);
			this.textObjectDesc.TabIndex = 0;
			this.textObjectDesc.Text = "line 1\r\nline 2";
			// 
			// textTaskList
			// 
			this.textTaskList.Location = new System.Drawing.Point(24, 73);
			this.textTaskList.Name = "textTaskList";
			this.textTaskList.ReadOnly = true;
			this.textTaskList.Size = new System.Drawing.Size(252, 20);
			this.textTaskList.TabIndex = 148;
			// 
			// labelObjectDesc
			// 
			this.labelObjectDesc.Location = new System.Drawing.Point(160, 184);
			this.labelObjectDesc.Name = "labelObjectDesc";
			this.labelObjectDesc.Size = new System.Drawing.Size(116, 16);
			this.labelObjectDesc.TabIndex = 8;
			this.labelObjectDesc.Text = "ObjectDesc";
			this.labelObjectDesc.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(182, 55);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(94, 16);
			this.label10.TabIndex = 149;
			this.label10.Text = "Task List";
			this.label10.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			// 
			// groupCommlog
			// 
			this.groupCommlog.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.groupCommlog.Controls.Add(this.butCurrentPatCommlog);
			this.groupCommlog.Controls.Add(this.butChangeCommPat);
			this.groupCommlog.Controls.Add(this.labelPatDiffCommlog);
			this.groupCommlog.Controls.Add(this.label6);
			this.groupCommlog.Controls.Add(this.listType);
			this.groupCommlog.Controls.Add(this.listSentOrReceived);
			this.groupCommlog.Controls.Add(this.label4);
			this.groupCommlog.Controls.Add(this.label3);
			this.groupCommlog.Controls.Add(this.label5);
			this.groupCommlog.Controls.Add(this.listMode);
			this.groupCommlog.Controls.Add(this.textPatientName);
			this.groupCommlog.Location = new System.Drawing.Point(32, 119);
			this.groupCommlog.Name = "groupCommlog";
			this.groupCommlog.Size = new System.Drawing.Size(329, 268);
			this.groupCommlog.TabIndex = 180;
			this.groupCommlog.TabStop = false;
			this.groupCommlog.Text = "Commlog";
			// 
			// butCurrentPatCommlog
			// 
			this.butCurrentPatCommlog.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butCurrentPatCommlog.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.butCurrentPatCommlog.Autosize = true;
			this.butCurrentPatCommlog.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butCurrentPatCommlog.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butCurrentPatCommlog.CornerRadius = 4F;
			this.butCurrentPatCommlog.Location = new System.Drawing.Point(139, 195);
			this.butCurrentPatCommlog.Name = "butCurrentPatCommlog";
			this.butCurrentPatCommlog.Size = new System.Drawing.Size(75, 22);
			this.butCurrentPatCommlog.TabIndex = 182;
			this.butCurrentPatCommlog.Text = "Current";
			// 
			// butChangeCommPat
			// 
			this.butChangeCommPat.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butChangeCommPat.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.butChangeCommPat.Autosize = true;
			this.butChangeCommPat.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butChangeCommPat.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butChangeCommPat.CornerRadius = 4F;
			this.butChangeCommPat.Location = new System.Drawing.Point(9, 195);
			this.butChangeCommPat.Name = "butChangeCommPat";
			this.butChangeCommPat.Size = new System.Drawing.Size(75, 22);
			this.butChangeCommPat.TabIndex = 182;
			this.butChangeCommPat.Text = "Change";
			// 
			// labelPatDiffCommlog
			// 
			this.labelPatDiffCommlog.ForeColor = System.Drawing.Color.Red;
			this.labelPatDiffCommlog.Location = new System.Drawing.Point(229, 169);
			this.labelPatDiffCommlog.Name = "labelPatDiffCommlog";
			this.labelPatDiffCommlog.Size = new System.Drawing.Size(90, 48);
			this.labelPatDiffCommlog.TabIndex = 156;
			this.labelPatDiffCommlog.Text = "Different patient than the one selected in OD";
			this.labelPatDiffCommlog.Visible = false;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(6, 16);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(82, 16);
			this.label6.TabIndex = 111;
			this.label6.Text = "Type";
			this.label6.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// listType
			// 
			this.listType.Location = new System.Drawing.Point(8, 34);
			this.listType.Name = "listType";
			this.listType.Size = new System.Drawing.Size(120, 95);
			this.listType.TabIndex = 112;
			// 
			// listSentOrReceived
			// 
			this.listSentOrReceived.Location = new System.Drawing.Point(229, 34);
			this.listSentOrReceived.Name = "listSentOrReceived";
			this.listSentOrReceived.Size = new System.Drawing.Size(87, 43);
			this.listSentOrReceived.TabIndex = 117;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(228, 16);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(95, 16);
			this.label4.TabIndex = 116;
			this.label4.Text = "Sent or Received";
			this.label4.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(140, 17);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(82, 16);
			this.label3.TabIndex = 114;
			this.label3.Text = "Mode";
			this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(6, 146);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(78, 20);
			this.label5.TabIndex = 31;
			this.label5.Text = "Patient";
			this.label5.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// listMode
			// 
			this.listMode.Location = new System.Drawing.Point(141, 34);
			this.listMode.Name = "listMode";
			this.listMode.Size = new System.Drawing.Size(73, 95);
			this.listMode.TabIndex = 115;
			// 
			// textPatientName
			// 
			this.textPatientName.Location = new System.Drawing.Point(9, 169);
			this.textPatientName.Name = "textPatientName";
			this.textPatientName.ReadOnly = true;
			this.textPatientName.Size = new System.Drawing.Size(205, 20);
			this.textPatientName.TabIndex = 32;
			// 
			// timerSaved
			// 
			this.timerSaved.Interval = 1500;
			// 
			// mainMenu1
			// 
			this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemTools,
            this.menuItemAddWindow});
			// 
			// menuItemTools
			// 
			this.menuItemTools.Index = 0;
			this.menuItemTools.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemOnTop,
            this.menuItemSettings});
			this.menuItemTools.Text = "Tools";
			// 
			// menuItemOnTop
			// 
			this.menuItemOnTop.Enabled = false;
			this.menuItemOnTop.Index = 0;
			this.menuItemOnTop.Text = "Always On Top";
			// 
			// menuItemSettings
			// 
			this.menuItemSettings.Index = 1;
			this.menuItemSettings.Text = "Settings";
			// 
			// menuItemAddWindow
			// 
			this.menuItemAddWindow.Index = 1;
			this.menuItemAddWindow.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemProgNotes,
            this.menuItemTaskList});
			this.menuItemAddWindow.Text = "Add Window";
			// 
			// menuItemProgNotes
			// 
			this.menuItemProgNotes.Index = 0;
			this.menuItemProgNotes.Text = "Progress Notes";
			// 
			// menuItemTaskList
			// 
			this.menuItemTaskList.Index = 1;
			this.menuItemTaskList.Text = "Task List";
			// 
			// FormTaskComm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(698, 614);
			this.Controls.Add(this.groupBoth);
			this.Controls.Add(this.groupTask);
			this.Controls.Add(this.groupCommlog);
			this.Controls.Add(this.labelSavedTaskComm);
			this.Controls.Add(this.textNote);
			this.Controls.Add(this.labelDescription);
			this.Controls.Add(this.butClear);
			this.Controls.Add(this.butTask);
			this.Controls.Add(this.butBoth);
			this.Controls.Add(this.butCommlog);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.mainMenu1;
			this.MinimumSize = new System.Drawing.Size(694, 638);
			this.Name = "FormTaskComm";
			this.Text = "TaskComm";
			this.groupBoth.ResumeLayout(false);
			this.groupBoth.PerformLayout();
			this.groupTask.ResumeLayout(false);
			this.groupTask.PerformLayout();
			this.groupCommlog.ResumeLayout(false);
			this.groupCommlog.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private UI.Button butCommlog;
		private UI.Button butBoth;
		private UI.Button butTask;
		private UI.Button butClear;
		private ODtextBox textNote;
		private System.Windows.Forms.Label labelDescription;
		private System.Windows.Forms.Label labelSavedTaskComm;
		private System.Windows.Forms.GroupBox groupBoth;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.TextBox textDateTime;
		private System.Windows.Forms.Label labelDateTimeEnd;
		private System.Windows.Forms.TextBox textUser;
		private System.Windows.Forms.TextBox textDateTimeEnd;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupTask;
		private System.Windows.Forms.Button butColor;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.ComboBox comboTaskPriorities;
		private System.Windows.Forms.Label labelPatDiffTask;
		private System.Windows.Forms.ListBox listObjectType;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox textObjectDesc;
		private System.Windows.Forms.TextBox textTaskList;
		private System.Windows.Forms.Label labelObjectDesc;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.GroupBox groupCommlog;
		private System.Windows.Forms.Label labelPatDiffCommlog;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ListBox listType;
		private System.Windows.Forms.ListBox listSentOrReceived;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ListBox listMode;
		private System.Windows.Forms.TextBox textPatientName;
		private System.Windows.Forms.Timer timerSaved;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItemTools;
		private System.Windows.Forms.MenuItem menuItemOnTop;
		private System.Windows.Forms.MenuItem menuItemSettings;
		private System.Windows.Forms.MenuItem menuItemAddWindow;
		private System.Windows.Forms.MenuItem menuItemProgNotes;
		private System.Windows.Forms.MenuItem menuItemTaskList;
		private UI.Button butCurrentPatCommlog;
		private UI.Button butChangeCommPat;
		private UI.Button butNowEnd;
		private UI.Button butNow;
		private UI.Button butTaskListOne;
		private UI.Button butTaskListAdd;
		private UI.Button butTaskListClear;
		private UI.Button butCurrentPatTask;
		private UI.Button butChangeTaskPat;
	}
}