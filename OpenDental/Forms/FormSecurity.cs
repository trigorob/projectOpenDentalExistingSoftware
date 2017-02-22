using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.UI;
using System.Linq;

namespace OpenDental{
	/// <summary>
	/// Summary description for FormBasicTemplate.
	/// </summary>
	public class FormSecurity:ODForm {
		private OpenDental.UI.Button butClose;
		private OpenDental.UI.Button butAddGroup;
		private OpenDental.UI.Button butAddUser;
		private System.Windows.Forms.TreeView treePermissions;
		private System.Windows.Forms.ImageList imageListPerm;
		private System.Windows.Forms.Label labelPerm;
		private System.ComponentModel.IContainer components;
		private long SelectedGroupNum;
		private TreeNode clickedPermNode;
		private System.Windows.Forms.CheckBox checkTimecardSecurityEnabled;
		private OpenDental.UI.Button butSetAll;
		private OpenDental.UI.ODGrid gridMain;
		private bool changed;
		private ComboBox comboUsers;
		private ComboBox comboSchoolClass;
		private Label labelSchoolClass;
		private CheckBox checkCannotEditOwn;
		private Label label1;
		private TextBox textDateLock;
		private OpenDental.UI.Button butChange;
		private CheckBox checkPasswordsMustBeStrong;
		private TextBox textDaysLock;
		private Label label2;
		private CheckBox checkLogOffWindows;
		private TextBox textLogOffAfterMinutes;
		private Label label3;
		private Label label4;
		private CheckBox checkUserNameManualEntry;
		private Label labelGlobalDateLockDisabled;
		private CheckBox checkDisableBackupReminder;
		private TextBox textPowerSearch;
		private ComboBox comboGroups;
		private Label labelPermission;
		private Label labelClinic;
		private ComboBox comboClinic;
		private List<Clinic> _listClinics;
		private List<UserGroup> _listUserGroups;
		///<summary>Contains all users to alloiw for speady refilling and filtering of users into the displayable list of users _listUserodCur.</summary>
		private List<Userod> _listUsersAll;
		///<summary>Continuously refilled based on search/filter criteria from _listUserodAll.</summary>
		private List<Userod> _listUserodCur;
		private GroupBox groupBox2;
		private Label labelFilterType;
		private Label label5;
		///<summary>Used for filtering within the form, saves many calls to DB.</summary>
		private Dictionary<long,Provider> _dictProvNumProvs;
		///<summary>When false, FillUsers will refresh cache from db.</summary>
		private bool _isCacheValid;
		private List<string> _listShowOnlyTypes;

		///<summary></summary>
		public FormSecurity()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			Plugins.HookAddCode(this,"FormSecurity.InitializeComponent_end");
			Lan.F(this);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSecurity));
			this.imageListPerm = new System.Windows.Forms.ImageList(this.components);
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.labelFilterType = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.comboUsers = new System.Windows.Forms.ComboBox();
			this.comboClinic = new System.Windows.Forms.ComboBox();
			this.textPowerSearch = new System.Windows.Forms.TextBox();
			this.labelClinic = new System.Windows.Forms.Label();
			this.comboGroups = new System.Windows.Forms.ComboBox();
			this.labelPermission = new System.Windows.Forms.Label();
			this.comboSchoolClass = new System.Windows.Forms.ComboBox();
			this.labelSchoolClass = new System.Windows.Forms.Label();
			this.checkDisableBackupReminder = new System.Windows.Forms.CheckBox();
			this.labelGlobalDateLockDisabled = new System.Windows.Forms.Label();
			this.checkUserNameManualEntry = new System.Windows.Forms.CheckBox();
			this.textLogOffAfterMinutes = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.checkLogOffWindows = new System.Windows.Forms.CheckBox();
			this.textDaysLock = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.checkPasswordsMustBeStrong = new System.Windows.Forms.CheckBox();
			this.treePermissions = new System.Windows.Forms.TreeView();
			this.butChange = new OpenDental.UI.Button();
			this.textDateLock = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.checkCannotEditOwn = new System.Windows.Forms.CheckBox();
			this.gridMain = new OpenDental.UI.ODGrid();
			this.butSetAll = new OpenDental.UI.Button();
			this.checkTimecardSecurityEnabled = new System.Windows.Forms.CheckBox();
			this.butAddUser = new OpenDental.UI.Button();
			this.butAddGroup = new OpenDental.UI.Button();
			this.butClose = new OpenDental.UI.Button();
			this.labelPerm = new System.Windows.Forms.Label();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// imageListPerm
			// 
			this.imageListPerm.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListPerm.ImageStream")));
			this.imageListPerm.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListPerm.Images.SetKeyName(0, "grayBox.gif");
			this.imageListPerm.Images.SetKeyName(1, "checkBoxUnchecked.gif");
			this.imageListPerm.Images.SetKeyName(2, "checkBoxChecked.gif");
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.labelFilterType);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.comboUsers);
			this.groupBox2.Controls.Add(this.comboClinic);
			this.groupBox2.Controls.Add(this.textPowerSearch);
			this.groupBox2.Controls.Add(this.labelClinic);
			this.groupBox2.Controls.Add(this.comboGroups);
			this.groupBox2.Controls.Add(this.labelPermission);
			this.groupBox2.Controls.Add(this.comboSchoolClass);
			this.groupBox2.Controls.Add(this.labelSchoolClass);
			this.groupBox2.Location = new System.Drawing.Point(8, 3);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(511, 83);
			this.groupBox2.TabIndex = 247;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Filters";
			// 
			// labelFilterType
			// 
			this.labelFilterType.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labelFilterType.Location = new System.Drawing.Point(6, 56);
			this.labelFilterType.Name = "labelFilterType";
			this.labelFilterType.Size = new System.Drawing.Size(88, 21);
			this.labelFilterType.TabIndex = 248;
			this.labelFilterType.Text = "Username";
			this.labelFilterType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(3, 14);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(62, 16);
			this.label5.TabIndex = 247;
			this.label5.Text = "Show Only";
			this.label5.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// comboUsers
			// 
			this.comboUsers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboUsers.FormattingEnabled = true;
			this.comboUsers.Location = new System.Drawing.Point(6, 32);
			this.comboUsers.Name = "comboUsers";
			this.comboUsers.Size = new System.Drawing.Size(118, 21);
			this.comboUsers.TabIndex = 1;
			this.comboUsers.SelectionChangeCommitted += new System.EventHandler(this.comboUsers_SelectionChangeCommitted);
			// 
			// comboClinic
			// 
			this.comboClinic.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboClinic.Location = new System.Drawing.Point(260, 32);
			this.comboClinic.MaxDropDownItems = 30;
			this.comboClinic.Name = "comboClinic";
			this.comboClinic.Size = new System.Drawing.Size(118, 21);
			this.comboClinic.TabIndex = 245;
			this.comboClinic.Visible = false;
			this.comboClinic.SelectionChangeCommitted += new System.EventHandler(this.comboClinic_SelectionChangeCommitted);
			// 
			// textPowerSearch
			// 
			this.textPowerSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textPowerSearch.Location = new System.Drawing.Point(97, 56);
			this.textPowerSearch.Name = "textPowerSearch";
			this.textPowerSearch.Size = new System.Drawing.Size(118, 21);
			this.textPowerSearch.TabIndex = 243;
			this.textPowerSearch.TextChanged += new System.EventHandler(this.textPowerSearch_TextChanged);
			// 
			// labelClinic
			// 
			this.labelClinic.Location = new System.Drawing.Point(257, 14);
			this.labelClinic.Name = "labelClinic";
			this.labelClinic.Size = new System.Drawing.Size(40, 16);
			this.labelClinic.TabIndex = 246;
			this.labelClinic.Text = "Clinic";
			this.labelClinic.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.labelClinic.Visible = false;
			// 
			// comboGroups
			// 
			this.comboGroups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboGroups.Location = new System.Drawing.Point(133, 32);
			this.comboGroups.MaxDropDownItems = 30;
			this.comboGroups.Name = "comboGroups";
			this.comboGroups.Size = new System.Drawing.Size(118, 21);
			this.comboGroups.TabIndex = 245;
			this.comboGroups.SelectionChangeCommitted += new System.EventHandler(this.comboGroups_SelectionChangeCommitted);
			// 
			// labelPermission
			// 
			this.labelPermission.Location = new System.Drawing.Point(130, 14);
			this.labelPermission.Name = "labelPermission";
			this.labelPermission.Size = new System.Drawing.Size(62, 16);
			this.labelPermission.TabIndex = 246;
			this.labelPermission.Text = "Group";
			this.labelPermission.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// comboSchoolClass
			// 
			this.comboSchoolClass.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboSchoolClass.Location = new System.Drawing.Point(387, 32);
			this.comboSchoolClass.MaxDropDownItems = 30;
			this.comboSchoolClass.Name = "comboSchoolClass";
			this.comboSchoolClass.Size = new System.Drawing.Size(118, 21);
			this.comboSchoolClass.TabIndex = 2;
			this.comboSchoolClass.Visible = false;
			this.comboSchoolClass.SelectionChangeCommitted += new System.EventHandler(this.comboSchoolClass_SelectionChangeCommitted);
			// 
			// labelSchoolClass
			// 
			this.labelSchoolClass.Location = new System.Drawing.Point(384, 14);
			this.labelSchoolClass.Name = "labelSchoolClass";
			this.labelSchoolClass.Size = new System.Drawing.Size(40, 16);
			this.labelSchoolClass.TabIndex = 91;
			this.labelSchoolClass.Text = "Class";
			this.labelSchoolClass.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			this.labelSchoolClass.Visible = false;
			// 
			// checkDisableBackupReminder
			// 
			this.checkDisableBackupReminder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkDisableBackupReminder.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkDisableBackupReminder.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkDisableBackupReminder.Location = new System.Drawing.Point(8, 624);
			this.checkDisableBackupReminder.Name = "checkDisableBackupReminder";
			this.checkDisableBackupReminder.Size = new System.Drawing.Size(224, 16);
			this.checkDisableBackupReminder.TabIndex = 105;
			this.checkDisableBackupReminder.Text = "Disable Monthly Backup Reminder";
			this.checkDisableBackupReminder.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkDisableBackupReminder.Click += new System.EventHandler(this.checkDisableBackupReminder_Click);
			// 
			// labelGlobalDateLockDisabled
			// 
			this.labelGlobalDateLockDisabled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.labelGlobalDateLockDisabled.Location = new System.Drawing.Point(375, 652);
			this.labelGlobalDateLockDisabled.Name = "labelGlobalDateLockDisabled";
			this.labelGlobalDateLockDisabled.Size = new System.Drawing.Size(113, 42);
			this.labelGlobalDateLockDisabled.TabIndex = 104;
			this.labelGlobalDateLockDisabled.Text = "(Disabled from Central \r\nManagement Tool) ";
			this.labelGlobalDateLockDisabled.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.labelGlobalDateLockDisabled.Visible = false;
			// 
			// checkUserNameManualEntry
			// 
			this.checkUserNameManualEntry.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkUserNameManualEntry.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkUserNameManualEntry.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkUserNameManualEntry.Location = new System.Drawing.Point(238, 624);
			this.checkUserNameManualEntry.Name = "checkUserNameManualEntry";
			this.checkUserNameManualEntry.Size = new System.Drawing.Size(224, 16);
			this.checkUserNameManualEntry.TabIndex = 103;
			this.checkUserNameManualEntry.Text = "Manually enter log on credentials";
			this.checkUserNameManualEntry.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textLogOffAfterMinutes
			// 
			this.textLogOffAfterMinutes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.textLogOffAfterMinutes.Location = new System.Drawing.Point(325, 563);
			this.textLogOffAfterMinutes.Name = "textLogOffAfterMinutes";
			this.textLogOffAfterMinutes.Size = new System.Drawing.Size(29, 20);
			this.textLogOffAfterMinutes.TabIndex = 5;
			this.textLogOffAfterMinutes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label4.Location = new System.Drawing.Point(358, 563);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(130, 18);
			this.label4.TabIndex = 102;
			this.label4.Text = "minutes.  0 to disable.";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label3.Location = new System.Drawing.Point(235, 563);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(90, 18);
			this.label3.TabIndex = 101;
			this.label3.Text = "Log off after ";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkLogOffWindows
			// 
			this.checkLogOffWindows.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkLogOffWindows.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkLogOffWindows.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkLogOffWindows.Location = new System.Drawing.Point(238, 588);
			this.checkLogOffWindows.Name = "checkLogOffWindows";
			this.checkLogOffWindows.Size = new System.Drawing.Size(224, 16);
			this.checkLogOffWindows.TabIndex = 7;
			this.checkLogOffWindows.Text = "Log off when Windows logs off";
			this.checkLogOffWindows.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textDaysLock
			// 
			this.textDaysLock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.textDaysLock.Location = new System.Drawing.Point(219, 677);
			this.textDaysLock.Name = "textDaysLock";
			this.textDaysLock.ReadOnly = true;
			this.textDaysLock.Size = new System.Drawing.Size(82, 20);
			this.textDaysLock.TabIndex = 98;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.Location = new System.Drawing.Point(95, 677);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(123, 18);
			this.label2.TabIndex = 97;
			this.label2.Text = "Lock Days";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkPasswordsMustBeStrong
			// 
			this.checkPasswordsMustBeStrong.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkPasswordsMustBeStrong.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkPasswordsMustBeStrong.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkPasswordsMustBeStrong.Location = new System.Drawing.Point(238, 606);
			this.checkPasswordsMustBeStrong.Name = "checkPasswordsMustBeStrong";
			this.checkPasswordsMustBeStrong.Size = new System.Drawing.Size(224, 16);
			this.checkPasswordsMustBeStrong.TabIndex = 9;
			this.checkPasswordsMustBeStrong.Text = "Passwords must be strong";
			this.checkPasswordsMustBeStrong.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkPasswordsMustBeStrong.Click += new System.EventHandler(this.checkPasswordsMustBeStrong_Click);
			// 
			// treePermissions
			// 
			this.treePermissions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treePermissions.HideSelection = false;
			this.treePermissions.ImageIndex = 0;
			this.treePermissions.ImageList = this.imageListPerm;
			this.treePermissions.ItemHeight = 15;
			this.treePermissions.Location = new System.Drawing.Point(525, 26);
			this.treePermissions.Name = "treePermissions";
			this.treePermissions.SelectedImageIndex = 0;
			this.treePermissions.ShowPlusMinus = false;
			this.treePermissions.ShowRootLines = false;
			this.treePermissions.Size = new System.Drawing.Size(402, 640);
			this.treePermissions.TabIndex = 6;
			this.treePermissions.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treePermissions_AfterSelect);
			this.treePermissions.DoubleClick += new System.EventHandler(this.treePermissions_DoubleClick);
			this.treePermissions.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treePermissions_MouseDown);
			// 
			// butChange
			// 
			this.butChange.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butChange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butChange.Autosize = true;
			this.butChange.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butChange.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butChange.CornerRadius = 4F;
			this.butChange.Location = new System.Drawing.Point(304, 664);
			this.butChange.Name = "butChange";
			this.butChange.Size = new System.Drawing.Size(70, 24);
			this.butChange.TabIndex = 10;
			this.butChange.Text = "Change";
			this.butChange.Click += new System.EventHandler(this.butChange_Click);
			// 
			// textDateLock
			// 
			this.textDateLock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.textDateLock.Location = new System.Drawing.Point(219, 656);
			this.textDateLock.Name = "textDateLock";
			this.textDateLock.ReadOnly = true;
			this.textDateLock.Size = new System.Drawing.Size(82, 20);
			this.textDateLock.TabIndex = 94;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.Location = new System.Drawing.Point(95, 656);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(123, 18);
			this.label1.TabIndex = 93;
			this.label1.Text = "Lock Date";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// checkCannotEditOwn
			// 
			this.checkCannotEditOwn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkCannotEditOwn.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkCannotEditOwn.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkCannotEditOwn.Location = new System.Drawing.Point(8, 606);
			this.checkCannotEditOwn.Name = "checkCannotEditOwn";
			this.checkCannotEditOwn.Size = new System.Drawing.Size(224, 16);
			this.checkCannotEditOwn.TabIndex = 8;
			this.checkCannotEditOwn.Text = "Users cannot edit their own timecard";
			this.checkCannotEditOwn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// gridMain
			// 
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.HasAddButton = false;
			this.gridMain.HasMultilineHeaders = false;
			this.gridMain.HeaderHeight = 15;
			this.gridMain.HScrollVisible = true;
			this.gridMain.Location = new System.Drawing.Point(8, 89);
			this.gridMain.Name = "gridMain";
			this.gridMain.ScrollValue = 0;
			this.gridMain.Size = new System.Drawing.Size(511, 453);
			this.gridMain.TabIndex = 59;
			this.gridMain.Title = "Users";
			this.gridMain.TitleHeight = 18;
			this.gridMain.TranslationName = "TableSecurity";
			this.gridMain.CellDoubleClick += new OpenDental.UI.ODGridClickEventHandler(this.gridMain_CellDoubleClick);
			this.gridMain.CellClick += new OpenDental.UI.ODGridClickEventHandler(this.gridMain_CellClick);
			// 
			// butSetAll
			// 
			this.butSetAll.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butSetAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSetAll.Autosize = true;
			this.butSetAll.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butSetAll.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butSetAll.CornerRadius = 4F;
			this.butSetAll.Location = new System.Drawing.Point(525, 672);
			this.butSetAll.Name = "butSetAll";
			this.butSetAll.Size = new System.Drawing.Size(79, 24);
			this.butSetAll.TabIndex = 11;
			this.butSetAll.Text = "Set All";
			this.butSetAll.Click += new System.EventHandler(this.butSetAll_Click);
			// 
			// checkTimecardSecurityEnabled
			// 
			this.checkTimecardSecurityEnabled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkTimecardSecurityEnabled.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkTimecardSecurityEnabled.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkTimecardSecurityEnabled.Location = new System.Drawing.Point(8, 588);
			this.checkTimecardSecurityEnabled.Name = "checkTimecardSecurityEnabled";
			this.checkTimecardSecurityEnabled.Size = new System.Drawing.Size(224, 16);
			this.checkTimecardSecurityEnabled.TabIndex = 6;
			this.checkTimecardSecurityEnabled.Text = "TimecardSecurityEnabled";
			this.checkTimecardSecurityEnabled.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkTimecardSecurityEnabled.Click += new System.EventHandler(this.checkTimecardSecurityEnabled_Click);
			// 
			// butAddUser
			// 
			this.butAddUser.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butAddUser.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butAddUser.Autosize = true;
			this.butAddUser.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butAddUser.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butAddUser.CornerRadius = 4F;
			this.butAddUser.Location = new System.Drawing.Point(118, 547);
			this.butAddUser.Name = "butAddUser";
			this.butAddUser.Size = new System.Drawing.Size(75, 24);
			this.butAddUser.TabIndex = 4;
			this.butAddUser.Text = "Add User";
			this.butAddUser.Click += new System.EventHandler(this.butAddUser_Click);
			// 
			// butAddGroup
			// 
			this.butAddGroup.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butAddGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butAddGroup.Autosize = true;
			this.butAddGroup.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butAddGroup.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butAddGroup.CornerRadius = 4F;
			this.butAddGroup.Location = new System.Drawing.Point(8, 547);
			this.butAddGroup.Name = "butAddGroup";
			this.butAddGroup.Size = new System.Drawing.Size(75, 24);
			this.butAddGroup.TabIndex = 3;
			this.butAddGroup.Text = "Edit Groups";
			this.butAddGroup.Click += new System.EventHandler(this.butEditGroups_Click);
			// 
			// butClose
			// 
			this.butClose.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butClose.Autosize = true;
			this.butClose.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butClose.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butClose.CornerRadius = 4F;
			this.butClose.Location = new System.Drawing.Point(852, 672);
			this.butClose.Name = "butClose";
			this.butClose.Size = new System.Drawing.Size(75, 24);
			this.butClose.TabIndex = 12;
			this.butClose.Text = "Close";
			this.butClose.Click += new System.EventHandler(this.butClose_Click);
			// 
			// labelPerm
			// 
			this.labelPerm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.labelPerm.Location = new System.Drawing.Point(522, 9);
			this.labelPerm.Name = "labelPerm";
			this.labelPerm.Size = new System.Drawing.Size(285, 15);
			this.labelPerm.TabIndex = 5;
			this.labelPerm.Text = "Permissions for group:";
			this.labelPerm.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// FormSecurity
			// 
			this.ClientSize = new System.Drawing.Size(934, 700);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.checkDisableBackupReminder);
			this.Controls.Add(this.labelGlobalDateLockDisabled);
			this.Controls.Add(this.checkUserNameManualEntry);
			this.Controls.Add(this.textLogOffAfterMinutes);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.checkLogOffWindows);
			this.Controls.Add(this.textDaysLock);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.checkPasswordsMustBeStrong);
			this.Controls.Add(this.treePermissions);
			this.Controls.Add(this.butChange);
			this.Controls.Add(this.textDateLock);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.checkCannotEditOwn);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.butSetAll);
			this.Controls.Add(this.checkTimecardSecurityEnabled);
			this.Controls.Add(this.butAddUser);
			this.Controls.Add(this.butAddGroup);
			this.Controls.Add(this.butClose);
			this.Controls.Add(this.labelPerm);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(920, 337);
			this.Name = "FormSecurity";
			this.Text = "Security";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormSecurity_FormClosing);
			this.Load += new System.EventHandler(this.FormSecurity_Load);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private void FormSecurity_Load(object sender, System.EventArgs e) {
			Signalods.Subscribe(this);
			_listUsersAll=UserodC.GetListt();//ordered by username
			_dictProvNumProvs=Providers.GetMultProviders(_listUsersAll.Select(x => x.ProvNum).ToList()).ToDictionary(x=>x.ProvNum,x=>x);
			_listShowOnlyTypes=new List<string>();
			_listShowOnlyTypes.Add("All Users");
			_listShowOnlyTypes.Add("Providers");
			_listShowOnlyTypes.Add("Employees");
			if(!PrefC.GetBool(PrefName.EasyHideDentalSchools)) {
				_listShowOnlyTypes.Add("Students");
				_listShowOnlyTypes.Add("Instructors");
			}
			_listShowOnlyTypes.Add("Other");
			comboUsers.Items.AddRange(_listShowOnlyTypes.Select(x => Lan.g(this,x)).ToArray());
			comboUsers.SelectedIndex=0;
			comboSchoolClass.Items.Add(Lan.g(this,"All"));
			comboSchoolClass.SelectedIndex=0;
			for(int i=0;i<SchoolClasses.List.Length;i++) {
				comboSchoolClass.Items.Add(SchoolClasses.GetDescript(SchoolClasses.List[i]));
			}
			FillTreePermissionsInitial();
			FillGrids();
			textLogOffAfterMinutes.Text=PrefC.GetInt(PrefName.SecurityLogOffAfterMinutes).ToString();
			checkPasswordsMustBeStrong.Checked=PrefC.GetBool(PrefName.PasswordsMustBeStrong);
			checkTimecardSecurityEnabled.Checked=PrefC.GetBool(PrefName.TimecardSecurityEnabled);
			checkCannotEditOwn.Checked=PrefC.GetBool(PrefName.TimecardUsersDontEditOwnCard);
			checkCannotEditOwn.Enabled=checkTimecardSecurityEnabled.Checked;
			checkLogOffWindows.Checked=PrefC.GetBool(PrefName.SecurityLogOffWithWindows);
			checkUserNameManualEntry.Checked=PrefC.GetBool(PrefName.UserNameManualEntry);
			if(PrefC.GetDate(PrefName.BackupReminderLastDateRun).ToShortDateString()==DateTime.MaxValue.AddMonths(-1).ToShortDateString()) {
				checkDisableBackupReminder.Checked=true;
			}
			if(PrefC.GetInt(PrefName.SecurityLockDays)>0) {
				textDaysLock.Text=PrefC.GetInt(PrefName.SecurityLockDays).ToString();
			}
			if(PrefC.GetDate(PrefName.SecurityLockDate).Year>1880){
				textDateLock.Text=PrefC.GetDate(PrefName.SecurityLockDate).ToShortDateString();
			}
			if(PrefC.GetBool(PrefName.CentralManagerSecurityLock)) {
				butChange.Enabled=false;
				labelGlobalDateLockDisabled.Visible=true;
			}
			if(PrefC.HasClinicsEnabled) {
				comboClinic.Visible=true;
				labelClinic.Visible=true;
				_listClinics=Clinics.GetClinics();
				comboClinic.Items.Clear();
				comboClinic.Items.Add(Lan.g(this,"All Clinics"));
				comboClinic.SelectedIndex=0;
				for(int i = 0;i<_listClinics.Count;i++) {
					comboClinic.Items.Add(_listClinics[i].Abbr);
				}
			}
			_listUserGroups=UserGroups.GetList();
			comboGroups.Items.Clear();
			comboGroups.Items.Add(Lan.g(this,"All Groups"));
			comboGroups.SelectedIndex=0;
			for(int i = 0;i<_listUserGroups.Count;i++) {
				comboGroups.Items.Add(_listUserGroups[i].Description);
			}
		}

		public void ProcessSignals(List<Signalod> listSigs) {
			if(listSigs.Any(x => x.IType==InvalidType.Security)) {
				//Working in this form causes these signals to be generated. So there is a good chance we are processing our own signals.
				_listUsersAll=UserodC.GetListt();//already refreshed cache from the signal before we got here.
			}
		}

		private void FillTreePermissionsInitial(){
			TreeNode node;
			TreeNode node2;//second level
			TreeNode node3;
			node=SetNode("Main Menu");
				node2=SetNode(Permissions.Setup);
					node3=SetNode(Permissions.ProblemEdit);
						node2.Nodes.Add(node3);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.ChooseDatabase);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.SecurityAdmin);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.AuditTrail);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.Schedules);
					node.Nodes.Add(node2);
				node2=SetNode("Merge Tools");
					node3=SetNode(Permissions.MedicationMerge);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.PatientMerge);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.ProviderMerge);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.ReferralMerge);
						node2.Nodes.Add(node3);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.Providers);
					node3=SetNode(Permissions.ProviderAlphabetize);
						node2.Nodes.Add(node3);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.ProviderFeeEdit);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.Blockouts);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.UserQuery);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.UserQueryAdmin);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.Reports);
					node3=SetNode(Permissions.GraphicalReportSetup);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.GraphicalReports);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.ReportProdInc);
						node2.Nodes.Add(node3);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.RefAttachAdd);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.RefAttachDelete);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.ReferralAdd);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.AutoNoteQuickNoteEdit);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.WikiListSetup);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.EServicesSetup);
					node.Nodes.Add(node2);
				treePermissions.Nodes.Add(node);
			node=SetNode("Main Toolbar");
				node2=SetNode(Permissions.CommlogEdit);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.EmailSend);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.WebMailSend);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.SheetEdit);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.TaskEdit);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.TaskNoteEdit);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.TaskListCreate);
					node.Nodes.Add(node2);
			treePermissions.Nodes.Add(node);
			node=SetNode(Permissions.AppointmentsModule);
				node2=SetNode(Permissions.AppointmentCreate);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.AppointmentMove);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.AppointmentEdit);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.AppointmentCompleteEdit);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.EcwAppointmentRevise);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.InsPlanVerifyList);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.ApptConfirmStatusEdit);
					node.Nodes.Add(node2);
				treePermissions.Nodes.Add(node);
			node=SetNode(Permissions.FamilyModule);
				node2=SetNode(Permissions.InsPlanEdit);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.InsPlanChangeAssign);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.InsPlanChangeSubsc);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.CarrierCreate);
					node.Nodes.Add(node2);
				treePermissions.Nodes.Add(node);
			node=SetNode(Permissions.AccountModule);
				node2=SetNode("Claim");
					node3=SetNode(Permissions.ClaimSend);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.ClaimSentEdit);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.ClaimDelete);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.ClaimProcReceivedEdit);
						node2.Nodes.Add(node3);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.AccountProcsQuickAdd);
					node.Nodes.Add(node2);
				node2=SetNode("Insurance Payment");
					node3=SetNode(Permissions.InsPayCreate);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.InsPayEdit);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.InsWriteOffEdit);
						node2.Nodes.Add(node3);
					node.Nodes.Add(node2);
				node2=SetNode("Payment");
					node3=SetNode(Permissions.PaymentCreate);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.PaymentEdit);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.SplitCreatePastLockDate);
						node2.Nodes.Add(node3);
					node.Nodes.Add(node2);
				node2=SetNode("Adjustment");
					node3=SetNode(Permissions.AdjustmentCreate);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.AdjustmentEdit);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.AdjustmentEditZero);
						node2.Nodes.Add(node3);
					node.Nodes.Add(node2);
				treePermissions.Nodes.Add(node);
			node=SetNode(Permissions.TPModule);
				node2=SetNode(Permissions.TreatPlanEdit);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.TreatPlanPresenterEdit);
					node.Nodes.Add(node2);
				treePermissions.Nodes.Add(node);
			node=SetNode(Permissions.ChartModule);
				node2=SetNode("Procedure");
					node3=SetNode(Permissions.ProcComplCreate);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.ProcComplEdit);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.ProcComplEditLimited);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.ProcEditShowFee);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.ProcDelete);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.ProcedureNote);
						node2.Nodes.Add(node3);
						node.Nodes.Add(node2);
				node2=SetNode("Rx");
					node3=SetNode(Permissions.RxCreate);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.RxEdit);
						node2.Nodes.Add(node3);
						node.Nodes.Add(node2);
			if(!Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)) {
				node2=SetNode(Permissions.OrthoChartEdit);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.PerioEdit);
					node.Nodes.Add(node2);
			}
				node2 = SetNode("Anesthesia");
					node3 = SetNode(Permissions.AnesthesiaIntakeMeds);
						node2.Nodes.Add(node3);
					node3 = SetNode(Permissions.AnesthesiaControlMeds);
						node2.Nodes.Add(node3);
						node.Nodes.Add(node2);
				treePermissions.Nodes.Add(node);
			node=SetNode(Permissions.ImagesModule);
				node2=SetNode(Permissions.ImageDelete);
					node.Nodes.Add(node2);
				treePermissions.Nodes.Add(node);
			node=SetNode(Permissions.ManageModule);
				node2=SetNode(Permissions.Accounting);
					node3=SetNode(Permissions.AccountingCreate);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.AccountingEdit);
						node2.Nodes.Add(node3);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.Billing);
					node.Nodes.Add(node2);	
				node2=SetNode(Permissions.DepositSlips);
					node.Nodes.Add(node2);
				node2=SetNode(Permissions.Backup);
					node.Nodes.Add(node2);
				node2=SetNode("Timecard");
					node3=SetNode(Permissions.TimecardsEditAll);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.TimecardDeleteEntry);
						node2.Nodes.Add(node3);
					node.Nodes.Add(node2);
				node2=SetNode("Equipment");
					node3=SetNode(Permissions.EquipmentSetup);
						node2.Nodes.Add(node3);
					node3=SetNode(Permissions.EquipmentDelete);
						node2.Nodes.Add(node3);
					node.Nodes.Add(node2);
				treePermissions.Nodes.Add(node);
			node=SetNode("EHR");
				node2=SetNode(Permissions.EhrEmergencyAccess);
				node.Nodes.Add(node2);
				node2=SetNode(Permissions.EhrMeasureEventEdit);
				node.Nodes.Add(node2);
				treePermissions.Nodes.Add(node);
			node=SetNode("Dental School");
				node2=SetNode(Permissions.AdminDentalInstructors);
				node.Nodes.Add(node2);
				node2=SetNode(Permissions.AdminDentalStudents);
				node.Nodes.Add(node2);
				node2=SetNode(Permissions.AdminDentalEvaluations);
				node.Nodes.Add(node2);
				//node2=SetNode(Permissions.EhrInfoButton);
				//	node.Nodes.Add(node2);
				//node2=SetNode(Permissions.EhrShowCDS);
				//	node.Nodes.Add(node2);
				treePermissions.Nodes.Add(node);
			treePermissions.ExpandAll();
		}

		///<summary>This just keeps FillTreePermissionsInitial looking cleaner.</summary>
		private TreeNode SetNode(Permissions perm){
			TreeNode retVal=new TreeNode();
			retVal.Text=GroupPermissions.GetDesc(perm);
			retVal.Tag=perm;
			retVal.ImageIndex=1;
			retVal.SelectedImageIndex=1;
			return retVal;
		}

		///<summary>Only called from FillTreePermissionsInitial</summary>
		private TreeNode SetNode(string text){
			TreeNode retVal=new TreeNode();
			retVal.Text=Lan.g(this,text);
			retVal.Tag=Permissions.None;
			retVal.ImageIndex=0;
			retVal.SelectedImageIndex=0;
			return retVal;
		}

		private void FillGrids() {
			FillGridUsers();
			FillTreePerm();
		}

		private void FillGridUsers(){
			if(!_isCacheValid) {
				Cache.Refresh(InvalidType.Security);//includes Userods, UserGroups, and GroupPermissions
				_listUsersAll=UserodC.GetListt();//ordered by username
				_isCacheValid=true;
			}
			SelectedGroupNum=0;
			gridMain.BeginUpdate();
			gridMain.Columns.Clear();
			ODGridColumn col=new ODGridColumn(Lan.g("TableSecurity","Username"),90);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lan.g("TableSecurity","Group"),90);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lan.g("TableSecurity","Employee"),90);
			gridMain.Columns.Add(col);
			if(PrefC.IsODHQ) {
				col=new ODGridColumn(Lan.g("TableSecurity","Job Roles"),60,HorizontalAlignment.Center);
				gridMain.Columns.Add(col);
			}
			col=new ODGridColumn(Lan.g("TableSecurity","Provider"),90);
			gridMain.Columns.Add(col);
			if(!PrefC.GetBool(PrefName.EasyNoClinics)) {
				col=new ODGridColumn(Lan.g("TableSecurity","Clinic"),80);
				gridMain.Columns.Add(col);
				col=new ODGridColumn(Lan.g("TableSecurity","ClinicRestricted"),100,HorizontalAlignment.Center);
				gridMain.Columns.Add(col);
			}
			col=new ODGridColumn(Lan.g("TableSecurity","Strong"),80,HorizontalAlignment.Center);
			gridMain.Columns.Add(col);
			if(!PrefC.GetBool(PrefName.EasyHideDentalSchools)) {
				col=new ODGridColumn(Lan.g("TableSecurity","Class"),100);
				gridMain.Columns.Add(col);
			}
			gridMain.Rows.Clear();
			ODGridRow row;
			_listUserodCur=getFilteredUsersHelper();
			string userdesc;
			for(int i=0;i<_listUserodCur.Count;i++) {
				row=new ODGridRow();
				userdesc=_listUserodCur[i].UserName;
				if(_listUserodCur[i].IsHidden){
					userdesc+=Lan.g(this,"(hidden)");
				}
				row.Cells.Add(userdesc);
				row.Cells.Add(UserGroups.GetGroup(_listUserodCur[i].UserGroupNum).Description);
				row.Cells.Add(Employees.GetNameFL(_listUserodCur[i].EmployeeNum));
				if(PrefC.IsODHQ) {
					if(JobPermissions.GetForUser(_listUserodCur[i].UserNum).Count>0) {
						row.Cells.Add("X");
					}
					else {
						row.Cells.Add("");
					}
				}
				row.Cells.Add(Providers.GetLongDesc(_listUserodCur[i].ProvNum));
				if(!PrefC.GetBool(PrefName.EasyNoClinics)) {
					row.Cells.Add(Clinics.GetAbbr(_listUserodCur[i].ClinicNum));
					row.Cells.Add(_listUserodCur[i].ClinicIsRestricted?"X":"");
				}
				row.Cells.Add(_listUserodCur[i].PasswordIsStrong?"X":"");
				if(!PrefC.GetBool(PrefName.EasyHideDentalSchools)) {
					Provider prov=Providers.GetProv(_listUserodCur[i].ProvNum);
					if(prov==null) {
						row.Cells.Add("");
					}
					else {
						row.Cells.Add(SchoolClasses.GetDescript(prov.SchoolClassNum));
					}
				}
				row.Tag=_listUserodCur[i];
				gridMain.Rows.Add(row);
			}
			gridMain.EndUpdate();	
		}

		private List<Userod> getFilteredUsersHelper() {
			List<Userod> retVal=new List<Userod>(_listUsersAll);
			retVal.RemoveAll(x => x.UserNumCEMT>0);//NEVER show CEMT users in this form. They MUST be edited from the CEMT tool.
			long classNum=0;
			if(comboSchoolClass.Visible && comboSchoolClass.SelectedIndex>0){
				classNum=SchoolClasses.List[comboSchoolClass.SelectedIndex-1].SchoolClassNum;
			}
			switch(_listShowOnlyTypes[comboUsers.SelectedIndex]) {
				case "Employees":
					retVal.RemoveAll(x => x.EmployeeNum==0);
					break;
				case "Providers":
					retVal.RemoveAll(x => x.ProvNum==0);
					break;
				case "Students":
					//might not count user as student if attached to invalid providers.
					retVal.RemoveAll(x => !_dictProvNumProvs.ContainsKey(x.ProvNum) || _dictProvNumProvs[x.ProvNum].IsInstructor);
					if(classNum>0) {
						retVal.RemoveAll(x => _dictProvNumProvs[x.ProvNum].SchoolClassNum!=classNum);
					}
					break;
				case "Instructors":
					retVal.RemoveAll(x => !_dictProvNumProvs.ContainsKey(x.ProvNum) || !_dictProvNumProvs[x.ProvNum].IsInstructor);
					if(classNum>0) {
						retVal.RemoveAll(x => _dictProvNumProvs[x.ProvNum].SchoolClassNum!=classNum);
					}
					break;
				case "Other":
					retVal.RemoveAll(x => x.EmployeeNum!=0 || x.ProvNum!=0);
					break;
				case "All Users":
				default:
					break;
			}			
			if(comboClinic.SelectedIndex>0) {
				long clinicNum=_listClinics[comboClinic.SelectedIndex-1].ClinicNum;
				retVal.RemoveAll(x=>x.ClinicNum!=clinicNum);
			}
			if(comboGroups.SelectedIndex>0) {
				long groupNum=_listUserGroups[comboGroups.SelectedIndex-1].UserGroupNum;
				retVal.RemoveAll(x=>x.UserGroupNum!=groupNum);
			}
			if(!string.IsNullOrWhiteSpace(textPowerSearch.Text)) {
				//If complaints of slowness arise, consider caching a local list of Employees (see _dictProvNumProvs) for use here and in filling the grid.
				switch(_listShowOnlyTypes[comboUsers.SelectedIndex]) {
					case "Employees":
						retVal.RemoveAll(x => !Employees.GetNameFL(x.EmployeeNum).ToLower().Contains(textPowerSearch.Text.ToLower()));
						break;
					case "Providers":
					case "Students":
					case "Instructors":
						retVal.RemoveAll(x => !_dictProvNumProvs[x.ProvNum].GetLongDesc().ToLower().Contains(textPowerSearch.Text.ToLower()));
						break;
					case "All Users":
					case "Other":
					default:
						retVal.RemoveAll(x => !x.UserName.ToLower().Contains(textPowerSearch.Text.ToLower()));
						break;
				}
			}
			return retVal;
		}

		private void gridMain_CellClick(object sender,ODGridClickEventArgs e) {
			List<Userod>listTempUsers=gridMain.Rows.OfType<ODGridRow>().ToList().Select(x => (Userod)x.Tag).ToList();
			SelectedGroupNum=listTempUsers[e.Row].UserGroupNum;
			for(int i=0;i<listTempUsers.Count;i++) {
				if(listTempUsers[i].UserGroupNum==SelectedGroupNum) {
					gridMain.Rows[i].ColorText=Color.Red;
				}
				else{
					gridMain.Rows[i].ColorText=Color.Black;
				}
			}
			gridMain.Invalidate();
			FillTreePerm();
		}

		private void comboUsers_SelectionChangeCommitted(object sender,EventArgs e) {
			string FilterType;
			switch(_listShowOnlyTypes[comboUsers.SelectedIndex]) {
				case "Employees":
					FilterType="Employee Name";
					break;
				case "Providers":
				case "Students":
				case "Instructors":
					FilterType="Provider Name";
					break;
				case "All Users":
				case "Other":
				default:
					FilterType="Username";
					break;
			}
			if(_listShowOnlyTypes[comboUsers.SelectedIndex]=="Students") {
				labelSchoolClass.Visible=true;
				comboSchoolClass.Visible=true;
			}
			else {
				labelSchoolClass.Visible=false;
				comboSchoolClass.Visible=false;
			}
			labelFilterType.Text=Lan.g(this,FilterType);
			FillGrids();
		}

		private void comboSchoolClass_SelectionChangeCommitted(object sender,EventArgs e) {
			FillGrids();
		}

		private void comboClinic_SelectionChangeCommitted(object sender,EventArgs e) {
			FillGrids();
		}

		private void comboGroups_SelectionChangeCommitted(object sender,EventArgs e) {
			FillGrids();
		}

		private void butEditGroups_Click(object sender, System.EventArgs e) {
			FormUserGroups FormU=new FormUserGroups();
			FormU.ShowDialog();
			_isCacheValid=false;
			FillGrids();
			changed=true;
		}

		private void butAddUser_Click(object sender, System.EventArgs e) {
			Userod user=new Userod();
			user.UserGroupNum=SelectedGroupNum;
			FormUserEdit FormU=new FormUserEdit(user);
			FormU.IsNew=true;
			FormU.ShowDialog();
			if(FormU.DialogResult==DialogResult.Cancel){
				return;
			}
			_isCacheValid=false;
			FillGrids();
			changed=true;
		}

		private void gridMain_CellDoubleClick(object sender,ODGridClickEventArgs e) {
			Userod user=Userods.GetUser(_listUserodCur[e.Row].UserNum);
			if(gridMain.Columns[e.Col].Heading=="Job Roles") {
				FormJobPermissions FormJR=new FormJobPermissions(user.UserNum);
				FormJR.ShowDialog();
				return;
			}
			FormUserEdit FormU=new FormUserEdit(user);
			FormU.ShowDialog();
			if(FormU.DialogResult==DialogResult.Cancel){
				return;
			}
			if(Security.CurUser.UserNum==user.UserNum) {
				Security.CurUser=FormU.UserCur;//if user changed their own password, this keeps the CurUser synched.  Needed for eCW bridge.
			}
			_isCacheValid=false;
			FillGridUsers();
			for(int i=0;i<_listUserodCur.Count;i++) {
				if(_listUserodCur[i].UserNum==FormU.UserCur.UserNum) {
					gridMain.SetSelected(i,true);
					SelectedGroupNum=FormU.UserCur.UserGroupNum;
				}
			}
			FillTreePerm();
			changed=true;
		}

		private void FillTreePerm(){
			GroupPermissions.RefreshCache();
			if(SelectedGroupNum==0){
				labelPerm.Text="";
				treePermissions.Enabled=false;
			}
			else{
				int gCount=_listUsersAll.Count(x=>x.UserGroupNum==SelectedGroupNum);
				string users=gCount==1?Lan.g(this,"user"):Lan.g(this,"users");
				labelPerm.Text=Lan.g(this,"Permissions for group:")+"  "+UserGroups.GetGroup(SelectedGroupNum).Description+string.Format(" ({0} {1})",gCount,users);
				treePermissions.Enabled=true;
			}
			for(int i=0;i<treePermissions.Nodes.Count;i++){
				FillNodes(treePermissions.Nodes[i],SelectedGroupNum);
			}
		}

		///<summary>A recursive function that sets the checkbox for a node.  Also sets the text for the node.</summary>
		private void FillNodes(TreeNode node,long userGroupNum) {
			//first, any child nodes
			for(int i=0;i<node.Nodes.Count;i++){
				FillNodes(node.Nodes[i],userGroupNum);
			}
			//then this node
			if(node.ImageIndex==0){
				return;
			}
			node.ImageIndex=1;
			node.Text=GroupPermissions.GetDesc((Permissions)node.Tag);
			for(int i=0;i<GroupPermissionC.List.Length;i++){
				if(GroupPermissionC.List[i].UserGroupNum==userGroupNum
					&& GroupPermissionC.List[i].PermType==(Permissions)node.Tag)
				{
					node.ImageIndex=2;
					if(GroupPermissionC.List[i].NewerDate.Year>1880){
						node.Text+=" ("+Lan.g(this,"if date newer than")+" "+GroupPermissionC.List[i].NewerDate.ToShortDateString()+")";
					}
					else if(GroupPermissionC.List[i].NewerDays>0){
						node.Text+=" ("+Lan.g(this,"if days newer than")+" "+GroupPermissionC.List[i].NewerDays.ToString()+")";
					}
				}
			}
		}

		private void treePermissions_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
			clickedPermNode=treePermissions.GetNodeAt(e.X,e.Y);
			if(clickedPermNode==null){
				return;
			}
			//treePermissions.BeginUpdate();
			if(clickedPermNode.Parent==null){//level 1
				if(e.X<5 || e.X>17){
					return;
				}
			}
			else if(clickedPermNode.Parent.Parent==null){//level 2
				if(e.X<24 || e.X>36){
					return;
				}
			}
			else if(clickedPermNode.Parent.Parent.Parent==null){//level 3
				if(e.X<43 || e.X>55){
					return;
				}
			}
			if(clickedPermNode.ImageIndex==1){//unchecked, so need to add a permission
				GroupPermission perm=new GroupPermission();
				perm.PermType=(Permissions)clickedPermNode.Tag;
				perm.UserGroupNum=SelectedGroupNum;
				if(GroupPermissions.PermTakesDates(perm.PermType)){
					FormGroupPermEdit FormG=new FormGroupPermEdit(perm);
					FormG.IsNew=true;
					FormG.ShowDialog();
					if(FormG.DialogResult==DialogResult.Cancel){
						treePermissions.EndUpdate();
						return;
					}
				}
				else{
					try{
						GroupPermissions.Insert(perm);
					}
					catch(Exception ex){
						MessageBox.Show(ex.Message);
						return;
					}
				}
				if(perm.PermType==Permissions.ProcComplEdit) {
					GroupPermission permLimited=GroupPermissions.GetPerm(SelectedGroupNum,Permissions.ProcComplEditLimited);
					if(permLimited==null) {
						GroupPermissions.RefreshCache();//refresh NewerDays/Date to add the same for ProcComplEditLimited
						perm=GroupPermissions.GetPerm(SelectedGroupNum,Permissions.ProcComplEdit);
						permLimited=new GroupPermission();
						permLimited.NewerDate=perm.NewerDate;
						permLimited.NewerDays=perm.NewerDays;
						permLimited.UserGroupNum=perm.UserGroupNum;
						permLimited.PermType=Permissions.ProcComplEditLimited;
						try {
							GroupPermissions.Insert(permLimited);
						}
						catch(Exception ex) {
							MessageBox.Show(ex.Message);
							return;
						}
					}
				}
			}
			else if(clickedPermNode.ImageIndex==2){//checked, so need to delete the perm
				try{
					GroupPermissions.RemovePermission(SelectedGroupNum,(Permissions)clickedPermNode.Tag);
				}
				catch(Exception ex){
					MessageBox.Show(ex.Message);
					return;
				}
				if((Permissions)clickedPermNode.Tag==Permissions.ProcComplEditLimited
					&& GroupPermissions.HasPermission(SelectedGroupNum,Permissions.ProcComplEdit))
				{
					try {
						GroupPermissions.RemovePermission(SelectedGroupNum,Permissions.ProcComplEdit);
					}
					catch(Exception ex) {
						MessageBox.Show(ex.Message);
						return;
					}
				}
			}
			FillTreePerm();
			changed=true;		
		}

		private void treePermissions_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e) {
			treePermissions.SelectedNode=null;
			treePermissions.EndUpdate();
		}

		private void treePermissions_DoubleClick(object sender, System.EventArgs e) {
			if(clickedPermNode==null){
				return;
			}
			Permissions permType=(Permissions)clickedPermNode.Tag;
			if(!GroupPermissions.PermTakesDates(permType)){
				return;
			}
			GroupPermission perm=GroupPermissions.GetPerm(SelectedGroupNum,(Permissions)clickedPermNode.Tag);
			if(perm==null){
				return;
			}
			FormGroupPermEdit FormG=new FormGroupPermEdit(perm);
			FormG.ShowDialog();
			if(FormG.DialogResult==DialogResult.Cancel){
				return;
			}
			FillTreePerm();
			changed=true;
		}

		private void butSetAll_Click(object sender,EventArgs e) {
			if(gridMain.SelectedIndices.Length==0){
				MsgBox.Show(this,"Please select user first.");
				return;
			}
			GroupPermission perm;
			for(int i=0;i<Enum.GetNames(typeof(Permissions)).Length;i++){
				if(i==(int)Permissions.SecurityAdmin
					|| i==(int)Permissions.StartupMultiUserOld
					|| i==(int)Permissions.StartupSingleUserOld
					|| i==(int)Permissions.EhrKeyAdd)
				{
					continue;
				}
				perm=GroupPermissions.GetPerm(SelectedGroupNum,(Permissions)i);
				if(perm==null){
					perm=new GroupPermission();
					perm.PermType=(Permissions)i;
					perm.UserGroupNum=SelectedGroupNum;
					try{
						GroupPermissions.Insert(perm);
					}
					catch(Exception ex){
						MessageBox.Show(ex.Message);
					}
					changed=true;
				}
			}
			FillTreePerm();
		}

		private void checkTimecardSecurityEnabled_Click(object sender,EventArgs e) {
			checkCannotEditOwn.Enabled=checkTimecardSecurityEnabled.Checked;
		}

		private void checkPasswordsMustBeStrong_Click(object sender,EventArgs e) {
			if(checkPasswordsMustBeStrong.Checked) {
				Prefs.UpdateBool(PrefName.PasswordsMustBeStrong,true);
				DataValid.SetInvalid(InvalidType.Prefs);
			}
			else{//unchecking the box
				if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Warning.  If this box is unchecked, the strong password flag on all users will be reset.  If strong passwords are again turned on later, then each users will have to edit their password in order to cause the strong password flag to be set again.")) {
					checkPasswordsMustBeStrong.Checked=true;//recheck it.
					return;
				}
				Userods.ResetStrongPasswordFlags();
				Prefs.UpdateBool(PrefName.PasswordsMustBeStrong,false);
				DataValid.SetInvalid(InvalidType.Security,InvalidType.Prefs);
			}
			_isCacheValid=false;
			FillGrids();
		}

		private void checkDisableBackupReminder_Click(object sender,EventArgs e) {
			InputBox inputbox=new InputBox("Please enter password");
			inputbox.setTitle("Change Backup Reminder Settings");
			inputbox.ShowDialog();
			if(inputbox.DialogResult!=DialogResult.OK) {
				checkDisableBackupReminder.Checked=!checkDisableBackupReminder.Checked;
				return;
			}
			if(inputbox.textResult.Text!="abracadabra") {
				checkDisableBackupReminder.Checked=!checkDisableBackupReminder.Checked;
				MsgBox.Show(this,"Wrong password");
				return;
			}
		}

		private void textPowerSearch_TextChanged(object sender,EventArgs e) {
			if(gridMain.SelectedIndices.Length>0) {
				FillGrids();//fill both if we are deselcting anything.
			}
			else {
				FillGridUsers();//only refill user grid if we haven't deselected anything
			}
		}

		private void butChange_Click(object sender,EventArgs e) {
			FormSecurityLock FormS=new FormSecurityLock();
			FormS.ShowDialog();//prefs are set invalid within that form if needed.
			if(PrefC.GetInt(PrefName.SecurityLockDays)>0){
				textDaysLock.Text=PrefC.GetInt(PrefName.SecurityLockDays).ToString();
			}
			else{
				textDaysLock.Text="";
			}
			if(PrefC.GetDate(PrefName.SecurityLockDate).Year>1880) {
				textDateLock.Text=PrefC.GetDate(PrefName.SecurityLockDate).ToShortDateString();
			}
			else {
				textDateLock.Text="";
			}
		}

		private void butClose_Click(object sender, System.EventArgs e) {
			Close();
		}

		private void FormSecurity_FormClosing(object sender,FormClosingEventArgs e) {
			if(textLogOffAfterMinutes.Text!="") {
				try {
					int logOffMinutes=Int32.Parse(textLogOffAfterMinutes.Text);
					if(logOffMinutes<0) {//Automatic log off must be a positive numerical value.
						throw new Exception();
					}
				}
				catch {
					MsgBox.Show(this,"Log off after minutes is invalid.");
					e.Cancel=true;
					return;
				}
			}
			if(changed){
				DataValid.SetInvalid(InvalidType.Security);
			}
			if(	//Prefs.UpdateBool(PrefName.PasswordsMustBeStrong,checkPasswordsMustBeStrong.Checked) //handled when box clicked.
				Prefs.UpdateBool(PrefName.TimecardSecurityEnabled,checkTimecardSecurityEnabled.Checked) 
				| Prefs.UpdateBool(PrefName.TimecardUsersDontEditOwnCard,checkCannotEditOwn.Checked) 
				| Prefs.UpdateBool(PrefName.SecurityLogOffWithWindows,checkLogOffWindows.Checked)
				| Prefs.UpdateBool(PrefName.UserNameManualEntry,checkUserNameManualEntry.Checked)
				| Prefs.UpdateInt(PrefName.SecurityLogOffAfterMinutes,PIn.Int(textLogOffAfterMinutes.Text))
				)
			{
				DataValid.SetInvalid(InvalidType.Prefs);
			}
			if(checkDisableBackupReminder.Checked) {
				Prefs.UpdateDateT(PrefName.BackupReminderLastDateRun,DateTime.MaxValue.AddMonths(-1)); //if MaxValue, gives error on startup.
			}
			else {
				Prefs.UpdateDateT(PrefName.BackupReminderLastDateRun,DateTimeOD.Today);
			}
		}

	}
}





















