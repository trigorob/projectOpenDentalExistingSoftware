using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using OpenDentBusiness;
using CodeBase;
using System.Collections.Generic;
using System.Linq;

namespace OpenDental{
///<summary></summary>
	public class FormPath : ODForm {
		private OpenDental.UI.Button butOK;
		private OpenDental.UI.Button butCancel;
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.TextBox textExportPath;
		private System.Windows.Forms.TextBox textDocPath;
		private OpenDental.UI.Button butBrowseExport;
		private OpenDental.UI.Button butBrowseDoc;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label labelPathSameForAll;
		private System.Windows.Forms.Label label3;
		private OpenDental.UI.Button butBrowseLetter;
		private System.Windows.Forms.TextBox textLetterMergePath;
		private FolderBrowserDialog fb;
		private CheckBox checkMultiplePaths;
		private RadioButton radioAtoZfolderNotRequired;
		private RadioButton radioUseFolder;
		private Label labelLocalPath;
		private TextBox textLocalPath;
		private OpenDental.UI.Button butBrowseLocal;
		private OpenDental.UI.Button butBrowseServer;
		private Label labelServerPath;
		private TextBox textServerPath;
		private GroupBox groupbox1;
		private TabControl tabControlDataStorageType;
		private TabPage tabAtoZ;
		private TabPage tabInDatabase;
		private TabPage tabDropbox;
		private Label label2;
		private Label label4;
		private Label label5;
		private TextBox textAtoZPath;
		private UI.Button butAuthorize;
		private Label labelWebhostURL;
		private TextBox textAccessToken;
		private RadioButton radioDropboxStorage;

		///<summary>If this is set to true before opening this form, then the program cannot find the AtoZ path and needs user input.</summary>
		public bool IsStartingUp;
		
		#region Dropbox Private Variables
		private Program _progCur;
		private ProgramProperty _programPropPathAtoZ;
		private ProgramProperty _dropboxAccessToken;
		///<summary>Set to true if the Dropbox API has been loaded already.</summary>
		private bool _hasDropboxLoaded;
		private Label label13;
		#endregion

		///<summary>This is the database storage type that the user has chosen (or was pulled from the database.
		///DO NOT change the value of this variable outside of SetRadioButtonChecked() or there is a chance for a stack overflow exception</summary>
		private DataStorageType _storageType=DataStorageType.LocalAtoZ;

		///<summary></summary>
		public FormPath(){
			InitializeComponent();
			Lan.F(this);
			//We only show the tabs in the designer for development purposes.  We want to hide them for our users.
			//Because the tab control is in "flat buttons" appearance and "fixed size" style the tabs will not show even if they are one pixel tall.
			//0,0 does not work because some size is required.
			tabControlDataStorageType.ItemSize=new Size(0,1);
		}

		///<summary></summary>
		protected override void Dispose( bool disposing ){
			if( disposing ){
				if(components != null){
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		private void InitializeComponent(){
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPath));
			this.butOK = new OpenDental.UI.Button();
			this.butCancel = new OpenDental.UI.Button();
			this.textDocPath = new System.Windows.Forms.TextBox();
			this.textExportPath = new System.Windows.Forms.TextBox();
			this.butBrowseExport = new OpenDental.UI.Button();
			this.butBrowseDoc = new OpenDental.UI.Button();
			this.fb = new System.Windows.Forms.FolderBrowserDialog();
			this.label1 = new System.Windows.Forms.Label();
			this.labelPathSameForAll = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.butBrowseLetter = new OpenDental.UI.Button();
			this.textLetterMergePath = new System.Windows.Forms.TextBox();
			this.checkMultiplePaths = new System.Windows.Forms.CheckBox();
			this.groupbox1 = new System.Windows.Forms.GroupBox();
			this.radioDropboxStorage = new System.Windows.Forms.RadioButton();
			this.tabControlDataStorageType = new System.Windows.Forms.TabControl();
			this.tabAtoZ = new System.Windows.Forms.TabPage();
			this.butBrowseLocal = new OpenDental.UI.Button();
			this.butBrowseServer = new OpenDental.UI.Button();
			this.labelServerPath = new System.Windows.Forms.Label();
			this.textServerPath = new System.Windows.Forms.TextBox();
			this.labelLocalPath = new System.Windows.Forms.Label();
			this.textLocalPath = new System.Windows.Forms.TextBox();
			this.tabInDatabase = new System.Windows.Forms.TabPage();
			this.label2 = new System.Windows.Forms.Label();
			this.tabDropbox = new System.Windows.Forms.TabPage();
			this.label5 = new System.Windows.Forms.Label();
			this.textAtoZPath = new System.Windows.Forms.TextBox();
			this.butAuthorize = new OpenDental.UI.Button();
			this.labelWebhostURL = new System.Windows.Forms.Label();
			this.textAccessToken = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.radioAtoZfolderNotRequired = new System.Windows.Forms.RadioButton();
			this.radioUseFolder = new System.Windows.Forms.RadioButton();
			this.label13 = new System.Windows.Forms.Label();
			this.groupbox1.SuspendLayout();
			this.tabControlDataStorageType.SuspendLayout();
			this.tabAtoZ.SuspendLayout();
			this.tabInDatabase.SuspendLayout();
			this.tabDropbox.SuspendLayout();
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
			this.butOK.Location = new System.Drawing.Point(513, 520);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 26);
			this.butOK.TabIndex = 2;
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
			this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.butCancel.Location = new System.Drawing.Point(594, 520);
			this.butCancel.Name = "butCancel";
			this.butCancel.Size = new System.Drawing.Size(75, 26);
			this.butCancel.TabIndex = 3;
			this.butCancel.Text = "&Cancel";
			this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
			// 
			// textDocPath
			// 
			this.textDocPath.Location = new System.Drawing.Point(7, 51);
			this.textDocPath.Name = "textDocPath";
			this.textDocPath.Size = new System.Drawing.Size(497, 20);
			this.textDocPath.TabIndex = 1;
			// 
			// textExportPath
			// 
			this.textExportPath.Location = new System.Drawing.Point(19, 385);
			this.textExportPath.Name = "textExportPath";
			this.textExportPath.Size = new System.Drawing.Size(515, 20);
			this.textExportPath.TabIndex = 1;
			// 
			// butBrowseExport
			// 
			this.butBrowseExport.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butBrowseExport.Autosize = true;
			this.butBrowseExport.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butBrowseExport.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butBrowseExport.CornerRadius = 4F;
			this.butBrowseExport.Location = new System.Drawing.Point(538, 382);
			this.butBrowseExport.Name = "butBrowseExport";
			this.butBrowseExport.Size = new System.Drawing.Size(76, 25);
			this.butBrowseExport.TabIndex = 91;
			this.butBrowseExport.Text = "Browse";
			this.butBrowseExport.Click += new System.EventHandler(this.butBrowseExport_Click);
			// 
			// butBrowseDoc
			// 
			this.butBrowseDoc.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butBrowseDoc.Autosize = true;
			this.butBrowseDoc.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butBrowseDoc.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butBrowseDoc.CornerRadius = 4F;
			this.butBrowseDoc.Location = new System.Drawing.Point(510, 47);
			this.butBrowseDoc.Name = "butBrowseDoc";
			this.butBrowseDoc.Size = new System.Drawing.Size(76, 25);
			this.butBrowseDoc.TabIndex = 2;
			this.butBrowseDoc.Text = "&Browse";
			this.butBrowseDoc.Click += new System.EventHandler(this.butBrowseDoc_Click);
			// 
			// fb
			// 
			this.fb.SelectedPath = "C:\\";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(20, 321);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(596, 59);
			this.label1.TabIndex = 92;
			this.label1.Text = resources.GetString("label1.Text");
			this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// labelPathSameForAll
			// 
			this.labelPathSameForAll.Location = new System.Drawing.Point(7, 6);
			this.labelPathSameForAll.Name = "labelPathSameForAll";
			this.labelPathSameForAll.Size = new System.Drawing.Size(579, 41);
			this.labelPathSameForAll.TabIndex = 93;
			this.labelPathSameForAll.Text = resources.GetString("labelPathSameForAll.Text");
			this.labelPathSameForAll.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(20, 411);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(596, 57);
			this.label3.TabIndex = 96;
			this.label3.Text = resources.GetString("label3.Text");
			this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// butBrowseLetter
			// 
			this.butBrowseLetter.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butBrowseLetter.Autosize = true;
			this.butBrowseLetter.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butBrowseLetter.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butBrowseLetter.CornerRadius = 4F;
			this.butBrowseLetter.Location = new System.Drawing.Point(538, 471);
			this.butBrowseLetter.Name = "butBrowseLetter";
			this.butBrowseLetter.Size = new System.Drawing.Size(76, 25);
			this.butBrowseLetter.TabIndex = 95;
			this.butBrowseLetter.Text = "Browse";
			this.butBrowseLetter.Click += new System.EventHandler(this.butBrowseLetter_Click);
			// 
			// textLetterMergePath
			// 
			this.textLetterMergePath.Location = new System.Drawing.Point(19, 474);
			this.textLetterMergePath.Name = "textLetterMergePath";
			this.textLetterMergePath.Size = new System.Drawing.Size(515, 20);
			this.textLetterMergePath.TabIndex = 94;
			// 
			// checkMultiplePaths
			// 
			this.checkMultiplePaths.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.checkMultiplePaths.Location = new System.Drawing.Point(7, 77);
			this.checkMultiplePaths.Name = "checkMultiplePaths";
			this.checkMultiplePaths.Size = new System.Drawing.Size(580, 44);
			this.checkMultiplePaths.TabIndex = 98;
			this.checkMultiplePaths.Text = resources.GetString("checkMultiplePaths.Text");
			this.checkMultiplePaths.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.checkMultiplePaths.UseVisualStyleBackColor = true;
			// 
			// groupbox1
			// 
			this.groupbox1.Controls.Add(this.radioDropboxStorage);
			this.groupbox1.Controls.Add(this.tabControlDataStorageType);
			this.groupbox1.Controls.Add(this.radioAtoZfolderNotRequired);
			this.groupbox1.Controls.Add(this.radioUseFolder);
			this.groupbox1.Location = new System.Drawing.Point(10, 12);
			this.groupbox1.Name = "groupbox1";
			this.groupbox1.Size = new System.Drawing.Size(624, 316);
			this.groupbox1.TabIndex = 0;
			this.groupbox1.TabStop = false;
			this.groupbox1.Text = "A to Z Images Folder for storing images and documents";
			// 
			// radioDropboxStorage
			// 
			this.radioDropboxStorage.Checked = true;
			this.radioDropboxStorage.Location = new System.Drawing.Point(9, 57);
			this.radioDropboxStorage.Name = "radioDropboxStorage";
			this.radioDropboxStorage.Size = new System.Drawing.Size(435, 17);
			this.radioDropboxStorage.TabIndex = 102;
			this.radioDropboxStorage.TabStop = true;
			this.radioDropboxStorage.Text = "Store images in Dropbox (an internet connection will be required)";
			this.radioDropboxStorage.UseVisualStyleBackColor = true;
			this.radioDropboxStorage.Click += new System.EventHandler(this.radioDropboxStorage_Click);
			// 
			// tabControlDataStorageType
			// 
			this.tabControlDataStorageType.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
			this.tabControlDataStorageType.Controls.Add(this.tabAtoZ);
			this.tabControlDataStorageType.Controls.Add(this.tabInDatabase);
			this.tabControlDataStorageType.Controls.Add(this.tabDropbox);
			this.tabControlDataStorageType.ItemSize = new System.Drawing.Size(100, 10);
			this.tabControlDataStorageType.Location = new System.Drawing.Point(11, 76);
			this.tabControlDataStorageType.Name = "tabControlDataStorageType";
			this.tabControlDataStorageType.SelectedIndex = 0;
			this.tabControlDataStorageType.Size = new System.Drawing.Size(606, 234);
			this.tabControlDataStorageType.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
			this.tabControlDataStorageType.TabIndex = 97;
			this.tabControlDataStorageType.TabStop = false;
			// 
			// tabAtoZ
			// 
			this.tabAtoZ.Controls.Add(this.butBrowseLocal);
			this.tabAtoZ.Controls.Add(this.labelPathSameForAll);
			this.tabAtoZ.Controls.Add(this.butBrowseServer);
			this.tabAtoZ.Controls.Add(this.butBrowseDoc);
			this.tabAtoZ.Controls.Add(this.labelServerPath);
			this.tabAtoZ.Controls.Add(this.textDocPath);
			this.tabAtoZ.Controls.Add(this.textServerPath);
			this.tabAtoZ.Controls.Add(this.checkMultiplePaths);
			this.tabAtoZ.Controls.Add(this.labelLocalPath);
			this.tabAtoZ.Controls.Add(this.textLocalPath);
			this.tabAtoZ.Location = new System.Drawing.Point(4, 14);
			this.tabAtoZ.Name = "tabAtoZ";
			this.tabAtoZ.Padding = new System.Windows.Forms.Padding(3);
			this.tabAtoZ.Size = new System.Drawing.Size(598, 216);
			this.tabAtoZ.TabIndex = 0;
			this.tabAtoZ.Text = "AtoZ";
			this.tabAtoZ.UseVisualStyleBackColor = true;
			// 
			// butBrowseLocal
			// 
			this.butBrowseLocal.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butBrowseLocal.Autosize = true;
			this.butBrowseLocal.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butBrowseLocal.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butBrowseLocal.CornerRadius = 4F;
			this.butBrowseLocal.Location = new System.Drawing.Point(510, 193);
			this.butBrowseLocal.Name = "butBrowseLocal";
			this.butBrowseLocal.Size = new System.Drawing.Size(76, 25);
			this.butBrowseLocal.TabIndex = 103;
			this.butBrowseLocal.Text = "Browse";
			this.butBrowseLocal.Click += new System.EventHandler(this.butBrowseLocal_Click);
			// 
			// butBrowseServer
			// 
			this.butBrowseServer.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butBrowseServer.Autosize = true;
			this.butBrowseServer.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butBrowseServer.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butBrowseServer.CornerRadius = 4F;
			this.butBrowseServer.Location = new System.Drawing.Point(510, 147);
			this.butBrowseServer.Name = "butBrowseServer";
			this.butBrowseServer.Size = new System.Drawing.Size(76, 25);
			this.butBrowseServer.TabIndex = 106;
			this.butBrowseServer.Text = "Browse";
			this.butBrowseServer.Click += new System.EventHandler(this.butBrowseServer_Click);
			// 
			// labelServerPath
			// 
			this.labelServerPath.Location = new System.Drawing.Point(7, 131);
			this.labelServerPath.Name = "labelServerPath";
			this.labelServerPath.Size = new System.Drawing.Size(488, 17);
			this.labelServerPath.TabIndex = 107;
			this.labelServerPath.Text = "Path override for this server.  Usually leave blank.";
			this.labelServerPath.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// textServerPath
			// 
			this.textServerPath.Location = new System.Drawing.Point(7, 151);
			this.textServerPath.Name = "textServerPath";
			this.textServerPath.Size = new System.Drawing.Size(497, 20);
			this.textServerPath.TabIndex = 105;
			// 
			// labelLocalPath
			// 
			this.labelLocalPath.Location = new System.Drawing.Point(7, 177);
			this.labelLocalPath.Name = "labelLocalPath";
			this.labelLocalPath.Size = new System.Drawing.Size(498, 17);
			this.labelLocalPath.TabIndex = 104;
			this.labelLocalPath.Text = "Path override for this computer.  Usually leave blank.";
			this.labelLocalPath.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// textLocalPath
			// 
			this.textLocalPath.Location = new System.Drawing.Point(7, 197);
			this.textLocalPath.Name = "textLocalPath";
			this.textLocalPath.Size = new System.Drawing.Size(497, 20);
			this.textLocalPath.TabIndex = 102;
			// 
			// tabInDatabase
			// 
			this.tabInDatabase.Controls.Add(this.label2);
			this.tabInDatabase.Location = new System.Drawing.Point(4, 14);
			this.tabInDatabase.Name = "tabInDatabase";
			this.tabInDatabase.Padding = new System.Windows.Forms.Padding(3);
			this.tabInDatabase.Size = new System.Drawing.Size(598, 216);
			this.tabInDatabase.TabIndex = 1;
			this.tabInDatabase.Text = "Database";
			this.tabInDatabase.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(6, 7);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(583, 124);
			this.label2.TabIndex = 97;
			this.label2.Text = resources.GetString("label2.Text");
			// 
			// tabDropbox
			// 
			this.tabDropbox.Controls.Add(this.label5);
			this.tabDropbox.Controls.Add(this.textAtoZPath);
			this.tabDropbox.Controls.Add(this.butAuthorize);
			this.tabDropbox.Controls.Add(this.labelWebhostURL);
			this.tabDropbox.Controls.Add(this.textAccessToken);
			this.tabDropbox.Controls.Add(this.label4);
			this.tabDropbox.Location = new System.Drawing.Point(4, 14);
			this.tabDropbox.Name = "tabDropbox";
			this.tabDropbox.Padding = new System.Windows.Forms.Padding(3);
			this.tabDropbox.Size = new System.Drawing.Size(598, 216);
			this.tabDropbox.TabIndex = 2;
			this.tabDropbox.Text = "Dropbox";
			this.tabDropbox.UseVisualStyleBackColor = true;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(99, 77);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(129, 19);
			this.label5.TabIndex = 127;
			this.label5.Text = "Dropbox AtoZ Path";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textAtoZPath
			// 
			this.textAtoZPath.Location = new System.Drawing.Point(230, 76);
			this.textAtoZPath.Name = "textAtoZPath";
			this.textAtoZPath.Size = new System.Drawing.Size(199, 20);
			this.textAtoZPath.TabIndex = 126;
			// 
			// butAuthorize
			// 
			this.butAuthorize.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butAuthorize.Autosize = true;
			this.butAuthorize.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butAuthorize.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butAuthorize.CornerRadius = 4F;
			this.butAuthorize.Location = new System.Drawing.Point(230, 128);
			this.butAuthorize.Name = "butAuthorize";
			this.butAuthorize.Size = new System.Drawing.Size(199, 24);
			this.butAuthorize.TabIndex = 125;
			this.butAuthorize.Text = "Authorize Dropbox";
			this.butAuthorize.Click += new System.EventHandler(this.butAuthorize_Click);
			// 
			// labelWebhostURL
			// 
			this.labelWebhostURL.Location = new System.Drawing.Point(99, 103);
			this.labelWebhostURL.Name = "labelWebhostURL";
			this.labelWebhostURL.Size = new System.Drawing.Size(129, 19);
			this.labelWebhostURL.TabIndex = 124;
			this.labelWebhostURL.Text = "Access Token";
			this.labelWebhostURL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textAccessToken
			// 
			this.textAccessToken.Location = new System.Drawing.Point(230, 102);
			this.textAccessToken.Name = "textAccessToken";
			this.textAccessToken.ReadOnly = true;
			this.textAccessToken.Size = new System.Drawing.Size(199, 20);
			this.textAccessToken.TabIndex = 123;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(6, 3);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(436, 15);
			this.label4.TabIndex = 93;
			this.label4.Text = "Store images on your Dropbox account.";
			this.label4.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// radioAtoZfolderNotRequired
			// 
			this.radioAtoZfolderNotRequired.Location = new System.Drawing.Point(9, 38);
			this.radioAtoZfolderNotRequired.Name = "radioAtoZfolderNotRequired";
			this.radioAtoZfolderNotRequired.Size = new System.Drawing.Size(537, 17);
			this.radioAtoZfolderNotRequired.TabIndex = 101;
			this.radioAtoZfolderNotRequired.TabStop = true;
			this.radioAtoZfolderNotRequired.Text = "Store images directly in database.  No AtoZ folder. (Some features will be unavai" +
    "lable)";
			this.radioAtoZfolderNotRequired.UseVisualStyleBackColor = true;
			this.radioAtoZfolderNotRequired.Click += new System.EventHandler(this.radioAtoZfolderNotRequired_Click);
			// 
			// radioUseFolder
			// 
			this.radioUseFolder.Location = new System.Drawing.Point(9, 19);
			this.radioUseFolder.Name = "radioUseFolder";
			this.radioUseFolder.Size = new System.Drawing.Size(333, 17);
			this.radioUseFolder.TabIndex = 0;
			this.radioUseFolder.TabStop = true;
			this.radioUseFolder.Text = "Store images and documents on a local or network folder.";
			this.radioUseFolder.UseVisualStyleBackColor = true;
			this.radioUseFolder.Click += new System.EventHandler(this.radioUseFolder_Click);
			// 
			// label13
			// 
			this.label13.ForeColor = System.Drawing.Color.Firebrick;
			this.label13.Location = new System.Drawing.Point(22, 514);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(488, 32);
			this.label13.TabIndex = 109;
			this.label13.Text = "Drobbox has been disabled until 17.1 due to missing functionality";
			this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// FormPath
			// 
			this.AcceptButton = this.butOK;
			this.CancelButton = this.butCancel;
			this.ClientSize = new System.Drawing.Size(681, 558);
			this.Controls.Add(this.label13);
			this.Controls.Add(this.groupbox1);
			this.Controls.Add(this.butBrowseLetter);
			this.Controls.Add(this.butBrowseExport);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.textLetterMergePath);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textExportPath);
			this.Controls.Add(this.butCancel);
			this.Controls.Add(this.butOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormPath";
			this.Text = "Edit Paths";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.FormPath_Closing);
			this.Load += new System.EventHandler(this.FormPath_Load);
			this.groupbox1.ResumeLayout(false);
			this.tabControlDataStorageType.ResumeLayout(false);
			this.tabAtoZ.ResumeLayout(false);
			this.tabAtoZ.PerformLayout();
			this.tabInDatabase.ResumeLayout(false);
			this.tabDropbox.ResumeLayout(false);
			this.tabDropbox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private void FormPath_Load(object sender, System.EventArgs e){
			if(!IsStartingUp && !Security.IsAuthorized(Permissions.Setup)) {//Verify user has Setup permission to change paths, after user has logged in.
				butOK.Enabled=false;
			}
			textDocPath.Text=PrefC.GetString(PrefName.DocPath);
			//ComputerPref compPref=ComputerPrefs.GetForLocalComputer();
			if(ReplicationServers.Server_id==0) {
				labelServerPath.Visible=false;
				textServerPath.Visible=false;
				butBrowseServer.Visible=false;
			}
			else {
				labelServerPath.Text="Path override for this server.  Server id = "+ReplicationServers.Server_id.ToString();
				textServerPath.Text=ReplicationServers.GetAtoZpath();
			}
			textLocalPath.Text=ImageStore.LocalAtoZpath;//This was set on startup.  //compPref.AtoZpath;
			textExportPath.Text=PrefC.GetString(PrefName.ExportPath);
			textLetterMergePath.Text=PrefC.GetString(PrefName.LetterMergePath);
			SetRadioButtonChecked(PrefC.AtoZfolderUsed);
			// The opt***_checked event will enable/disable the appropriate UI elements.
			checkMultiplePaths.Checked=(textDocPath.Text.LastIndexOf(';')!=-1);	
			//Also set the "multiple paths" checkbox at startup based on the current image folder list format. No need to store this info in the db.
			if(IsStartingUp) {//and failed to find path
				MsgBox.Show(this,"Could not find the path for the AtoZ folder.");
				if(Security.CurUser==null || !Security.IsAuthorized(Permissions.Setup)) {
					//The user is still allowed to set the "Path override for this computer", thus the user has a way to temporariliy get into OD in worst case.
					//For example, if the primary folder path is wrong or has changed, the user can set the path override for this computer to get into OD, then
					//can to to Setup | Data Paths to fix the primary path.
					radioUseFolder.Enabled=false;
					textDocPath.ReadOnly=true;
					butBrowseDoc.Enabled=false;
					checkMultiplePaths.Enabled=false;
					textServerPath.ReadOnly=true;
					butBrowseServer.Enabled=false;
					radioAtoZfolderNotRequired.Enabled=false;
					textExportPath.ReadOnly=true;
					butBrowseExport.Enabled=false;
					textLetterMergePath.ReadOnly=true;
					butBrowseLetter.Enabled=false;
					ActiveControl=textLocalPath;//Focus on textLocalPath, since this is the only textbox the user can edit in this case.
				}
			}
#if !DEBUG
			if(_storageType != DataStorageType.DropboxAtoZ) {
				radioDropboxStorage.Enabled=false;
			}
#endif
		}

		private void SetRadioButtonChecked(DataStorageType storageType) {
			switch(storageType) {
				case DataStorageType.LocalAtoZ:
					if(_storageType==DataStorageType.DropboxAtoZ
						&& !MsgBox.Show(this,MsgBoxButtons.OKCancel,"If you switch to storing images in a local AtoZ path, you will not be able to switch back "
						+"to Dropbox or SFTP. Are you sure you want to switch?")) 
					{
						SetRadioButtonChecked(_storageType);//This can cause a stack overflow exception if someone sets _storageType from outside of this method.
						return;
					}
					_storageType=DataStorageType.LocalAtoZ;
					radioUseFolder.Checked=true;//Will only do something when SetRadioButtonChecked is called on Load
					tabControlDataStorageType.SelectedTab=tabAtoZ;
					break;
				case DataStorageType.InDatabase:
					if(_storageType==DataStorageType.DropboxAtoZ
						&& !MsgBox.Show(this,MsgBoxButtons.OKCancel,"If you switch to storing images in the database, you will not be able to switch back "
						+"to Dropbox or SFTP. Are you sure you want to switch?")) 
					{
						SetRadioButtonChecked(_storageType);//This can cause a stack overflow exception if someone sets _storageType from outside of this method.
						return;
					}
					_storageType=DataStorageType.InDatabase;
					radioAtoZfolderNotRequired.Checked=true;//Will only do something when SetRadioButtonChecked is called on Load
					tabControlDataStorageType.SelectedTab=tabInDatabase;
					break;
				case DataStorageType.DropboxAtoZ:
					string error="";
					if(!LoadDropboxSetup(out error)) {
						SetRadioButtonChecked(_storageType);//This can cause a stack overflow exception if someone sets _storageType from outside of this method.
						MsgBox.Show(this,error);
						return;
					}
					radioDropboxStorage.Checked=true;//Will only do something when SetRadioButtonChecked is called on Load
					tabControlDataStorageType.SelectedTab=tabDropbox;
					_storageType=DataStorageType.DropboxAtoZ;
					break;
				default:
					MsgBox.Show(this,"There was an error retrieving your preferred data storage method.  Please call support to solve this issue.");
					break;
			}
		}

		///<summary>Tries to show the file browser dialog to the user.  Returns true if the user actually selected a path from the dialog.
		///Returns false if the user cancels out.  Also, shows a warning message and returns false if an exception occurred.</summary>
		private bool ShowFileBrowserDialog() {
			//A customer is having a "Unable to retrieve root folder" unhandled exception occur when trying to show the file browser dialog.
			//Therefore, try to show the dialog and if any exception occurs simply show a message box giving some suggestions to the user.
			try {
				return (fb.ShowDialog()==DialogResult.OK);
			}
			catch(Exception) {
				MsgBox.Show(this,"There was an error showing the Browse window.\r\nTry running as an Administrator or manually typing in a path.");
				return false;
			}
		}

		///<summary>Returns the given path with the local OS path separators as necessary.</summary>
		public static string FixDirSeparators(string path){
			if(Environment.OSVersion.Platform==PlatformID.Unix){
				path.Replace('\\',Path.DirectorySeparatorChar);
			}
			else{//Windows
				path.Replace('/',Path.DirectorySeparatorChar);
			}
			return path;
		}

		private void butBrowseDoc_Click(object sender,EventArgs e) {
			if(!ShowFileBrowserDialog()) {
				return;
			}
			//Ensure that the path entered has slashes matching the current OS (in case entered manually).
			string path=FixDirSeparators(fb.SelectedPath);
			if(checkMultiplePaths.Checked && textDocPath.Text.Length>0) {
				string messageText=Lan.g(this,"Replace existing document paths? Click No to add path to existing document paths.");
				switch(MessageBox.Show(messageText,"",MessageBoxButtons.YesNoCancel)) {
					case DialogResult.Yes:
						textDocPath.Text=path;//Replace existing paths with new path.
						break;
					case DialogResult.No://Append to existing paths?
						//Do not append a path which is already present in the list.
						if(!IsPathInList(path,textDocPath.Text)) {
							textDocPath.Text=textDocPath.Text+";"+path;
						}
						break;
					default://Cancel button.
						break;
				}
			}
			else{
				textDocPath.Text=path;//Just replace existing paths with new path.
			}
		}

		private void butBrowseServer_Click(object sender,EventArgs e) {
			if(ShowFileBrowserDialog()) {
				textServerPath.Text=fb.SelectedPath;
			}
		}

		private void butBrowseLocal_Click(object sender,EventArgs e) {
			if(ShowFileBrowserDialog()) {
				textLocalPath.Text=fb.SelectedPath;
			}
		}

		private void butBrowseExport_Click(object sender, System.EventArgs e) {
			if(ShowFileBrowserDialog()) {
				textExportPath.Text=fb.SelectedPath;
			}
		}

		private void butBrowseLetter_Click(object sender, System.EventArgs e) {
			if(ShowFileBrowserDialog()) {
				textLetterMergePath.Text=fb.SelectedPath;
			}
		}

		///<summary>Returns true if the given path is part of the imagePaths list, false otherwise.</summary>
		private static bool IsPathInList(string path,string imagePaths){
			string[] pathArray=imagePaths.Split(new char[] { ';' });
			for(int i=0;i<pathArray.Length;i++){
				if(pathArray[i]==path){//Case sensitive (since these could be unix paths).
					return true;
				}
			}
			return false;
		}

		private void radioUseFolder_Click(object sender,EventArgs e) {
			labelPathSameForAll.Enabled = radioUseFolder.Checked;
			textDocPath.Enabled = radioUseFolder.Checked;
			butBrowseDoc.Enabled = radioUseFolder.Checked;
			checkMultiplePaths.Enabled = radioUseFolder.Checked;
			//even though server path might not be visible:
			labelServerPath.Enabled=radioUseFolder.Checked;
			textServerPath.Enabled=radioUseFolder.Checked;
			butBrowseServer.Enabled=radioUseFolder.Checked;
			//
			labelLocalPath.Enabled=radioUseFolder.Checked;
			textLocalPath.Enabled=radioUseFolder.Checked;
			butBrowseLocal.Enabled=radioUseFolder.Checked;
			SetRadioButtonChecked(DataStorageType.LocalAtoZ);
		}

		private void radioAtoZfolderNotRequired_Click(object sender,EventArgs e) {
			if(radioAtoZfolderNotRequired.Checked){//user attempting to use db to store images
				InputBox inputbox=new InputBox("Please enter password");
				inputbox.ShowDialog();
				if(inputbox.DialogResult!=DialogResult.OK){
					SetRadioButtonChecked(_storageType);
					return;
				}
				if(inputbox.textResult.Text!="abracadabra"){//to keep ignorant people from clicking this box.
					SetRadioButtonChecked(_storageType);
					MsgBox.Show(this,"Wrong password");
					return;
				}
			}
			SetRadioButtonChecked(DataStorageType.InDatabase);
		}

		#region Dropbox Methods and Events

		///<summary>Returns true if loading Dropbox settings was successful.  False is something went wrong.
		///errorMsg will contain translated details about what went wrong in the case of a failure.</summary>
		private bool LoadDropboxSetup(out string errorMsg) {
			errorMsg="";
			if(_hasDropboxLoaded) {
				return true;
			}
			_progCur=Programs.GetCur(ProgramName.Dropbox);
			if(_progCur==null) {//Should never happen.
				errorMsg=Lan.g(this,"The Dropbox bridge is missing from the database.");
				return false;
			}
			try {
				List<ProgramProperty> listProperties=ProgramProperties.GetForProgram(_progCur.ProgramNum);
				_programPropPathAtoZ=listProperties.FirstOrDefault(x => x.PropertyDesc==DropboxApi.PropertyDescs.AtoZPath);
				_dropboxAccessToken=listProperties.FirstOrDefault(x => x.PropertyDesc==DropboxApi.PropertyDescs.AccessToken);
				textAtoZPath.Text=_programPropPathAtoZ.PropertyValue;
				textAccessToken.Text=_dropboxAccessToken.PropertyValue;
			}
			catch(Exception e) {
				errorMsg=Lan.g(this,"You are missing a program property for Dropbox.  Please contact support to resolve this issue.")+"\r\n"+e.StackTrace;
				return false;
			}
			_hasDropboxLoaded=true;
			return true;
		}

		private void butAuthorize_Click(object sender,EventArgs e) {
			FormDropboxAuthorize FormDA=new FormDropboxAuthorize();
			FormDA.ProgramPropertyAccessToken=_dropboxAccessToken;
			FormDA.ShowDialog();
			if(FormDA.DialogResult==DialogResult.OK) {
				_dropboxAccessToken=FormDA.ProgramPropertyAccessToken.Copy();
				textAccessToken.Text=FormDA.ProgramPropertyAccessToken.PropertyValue;
			}
		}

		private void radioDropboxStorage_Click(object sender,EventArgs e) {
			if(_storageType==DataStorageType.DropboxAtoZ) {
				return;
			}
			SetRadioButtonChecked(DataStorageType.DropboxAtoZ);
		}
		#endregion

		private void butOK_Click(object sender, System.EventArgs e){
			//remember that user might be using a website or a linux box to store images, therefore must allow forward slashes.
			if(radioUseFolder.Checked){
				if(textLocalPath.Text!=""){
					if(ImageStore.GetValidPathFromString(textLocalPath.Text)==null){
						MsgBox.Show(this,"The path override for this computer is invalid.  The folder must exist and must contain all 26 A through Z folders.");
						return;
					}
				}
				else if(textServerPath.Text!=""){
					if(ImageStore.GetValidPathFromString(textServerPath.Text)==null){
						MsgBox.Show(this,"The path override for this server is invalid.  The folder must exist and must contain all 26 A through Z folders.");
						return;
					}
				}
				else{
					if(ImageStore.GetValidPathFromString(textDocPath.Text)==null){
						MsgBox.Show(this,"The path is invalid.  The folder must exist and must contain all 26 A through Z folders.");
						return;
					}
				}				
    	}
			if(radioDropboxStorage.Checked && ProgramProperties.UpdateProgramPropertyWithValue(_programPropPathAtoZ,textAtoZPath.Text)) {
				MsgBox.Show(this,"Warning: Updating workstations older than 16.3 while using Dropbox may cause issues.\r\nIf experienced, use Setup.exe located in AtoZ folder on DropBox to reinstall.");
				DataValid.SetInvalid(InvalidType.Programs);
			}
			if(	Prefs.UpdateInt(PrefName.AtoZfolderUsed,(int)_storageType)
				| Prefs.UpdateString(PrefName.DocPath,textDocPath.Text)
				| Prefs.UpdateString(PrefName.ExportPath,textExportPath.Text)
				| Prefs.UpdateString(PrefName.LetterMergePath,textLetterMergePath.Text))
			{
				DataValid.SetInvalid(InvalidType.Prefs);
			}
			if(ImageStore.LocalAtoZpath!=textLocalPath.Text) {//if local path changed
				ImageStore.LocalAtoZpath=textLocalPath.Text;
				//ComputerPref compPref=ComputerPrefs.GetForLocalComputer();
				ComputerPrefs.LocalComputer.AtoZpath=ImageStore.LocalAtoZpath;
				ComputerPrefs.Update(ComputerPrefs.LocalComputer);
			}
			if(ReplicationServers.GetAtoZpath()!=textServerPath.Text) {
				ReplicationServer server=ReplicationServers.GetForLocalComputer();
				server.AtoZpath=textServerPath.Text;
				ReplicationServers.Update(server);
				DataValid.SetInvalid(InvalidType.ReplicationServers);
			}
			DialogResult=DialogResult.OK;
		}

		private void butCancel_Click(object sender,System.EventArgs e) {
			DialogResult=DialogResult.Cancel;
		}

		private void FormPath_Closing(object sender,System.ComponentModel.CancelEventArgs e) {
			/*
			if(DialogResult==DialogResult.OK) {
				return;
			}
			if(!IsStartingUp) {
				return;
			}
			//no need to check paths here.  If user hits cancel when starting up, it should always notify and exit.
			if(radioUseFolder.Checked 
				&& ImageStore.GetValidPathFromString(textDocPath.Text)==null 
				&& ImageStore.GetValidPathFromString(textLocalPath.Text)==null) 
			{
				MsgBox.Show(this,"Invalid A to Z path.  Closing program.");
				Application.Exit();
			}*/
		}
	}
}
