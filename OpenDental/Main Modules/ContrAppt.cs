/*=============================================================================================================
Open Dental GPL license Copyright (C) 2003  Jordan Sparks, DMD.  http://www.open-dent.com,  www.docsparks.com
See header in FormOpenDental.cs for complete text.  Redistributions must retain this text.
===============================================================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using OpenDental.Bridges;
using OpenDental.UI;
using OpenDentBusiness;
using OpenDentBusiness.HL7;
using OpenDentBusiness.UI;

namespace OpenDental {

	///<summary></summary>
	public class ContrAppt:System.Windows.Forms.UserControl {
		private OpenDental.ContrApptSheet ContrApptSheet2;
		private ContrApptSingle[] ContrApptSingle3;//the '3' has no significance
		private System.Windows.Forms.MonthCalendar Calendar2;
		private System.Windows.Forms.Label labelDate;
		private System.Windows.Forms.Label labelDate2;
		private System.ComponentModel.IContainer components;// Required designer variable.
		private bool mouseIsDown=false;
		///<summary>The point where the mouse was originally down.  In Appt Sheet coordinates</summary>
		private Point mouseOrigin = new Point();
		///<summary>Control origin.  If moving an appointment, this is the location where the appointment was at the beginning of the drag.</summary>
		private Point contOrigin = new Point();
		private ContrApptSingle TempApptSingle;
		private System.Windows.Forms.ImageList imageListMain;
		private bool boolAptMoved=false;
		private OpenDental.UI.Button butToday;
		private System.Windows.Forms.Panel panelSheet;
		private System.Windows.Forms.Panel panelCalendar;
		private System.Windows.Forms.Panel panelArrows;
		private System.Windows.Forms.VScrollBar vScrollBar1;
		private System.Windows.Forms.Panel panelOps;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ListBox listConfirmed;
		private System.Windows.Forms.Button butComplete;
		private System.Windows.Forms.Button butUnsched;
		private System.Windows.Forms.Button butDelete;
		private System.Windows.Forms.Button butBreak;
		///<summary>The actual operatoryNum of the clicked op.</summary>
		public static long SheetClickedonOp;
		///<summary></summary>
		public static int SheetClickedonHour;
		///<summary>The exact minute the user clicked on within the hour.  E.g. 58</summary>
		public static int SheetClickedonMin;
		private System.Drawing.Printing.PrintDocument pd2;
		private OpenDental.UI.Button butBack;
		private OpenDental.UI.Button butClearPin;
		private OpenDental.UI.Button butFwd;
		private System.Windows.Forms.Panel panelAptInfo;
		private System.Windows.Forms.Label label2;
		private OpenDental.UI.ODToolBar ToolBarMain;
		private System.Windows.Forms.TextBox textLab;
		private System.Windows.Forms.TextBox textProduction;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.ComboBox comboView;
		private System.Windows.Forms.ContextMenu menuPatient;
		///<summary></summary>
		public FormRpPrintPreview pView;
		private OpenDental.UI.Button butMakeAppt;
		private bool cardPrintFamily;
		private System.Windows.Forms.ContextMenu menuApt;
		private System.Windows.Forms.ContextMenu menuBlockout;
		private System.Windows.Forms.ContextMenu menuWeeklyApt;
		private List<Schedule> SchedListPeriod;
		private OpenDental.UI.Button butSearch;
		private System.Windows.Forms.GroupBox groupSearch;
		private OpenDental.UI.Button butSearchNext;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TextBox textBefore;
		private System.Windows.Forms.RadioButton radioBeforeAM;
		private System.Windows.Forms.RadioButton radioBeforePM;
		private System.Windows.Forms.RadioButton radioAfterPM;
		private System.Windows.Forms.RadioButton radioAfterAM;
		private System.Windows.Forms.TextBox textAfter;
		private System.Windows.Forms.Label label11;
		private OpenDental.UI.Button butSearchClose;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button butSearchCloseX;
		private System.Windows.Forms.ListBox listProviders;
		private System.Windows.Forms.ListBox listSearchResults;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.DateTimePicker dateSearch;
		private List<DateTime> SearchResults;
		private OpenDental.UI.Button butRefresh;
		private bool ResizingAppt;
		private int ResizingOrigH;
		//private bool isWeeklyView;
		public static DateTime WeekStartDate;
		public static DateTime WeekEndDate;
		private OpenDental.UI.Button butLab;
		///<summary>The index of the day as shown on the screen.  Only used in weekly view.</summary>
		public static int SheetClickedonDay;
		///<summary></summary>
		private Panel infoBubble;
		///<summary>The dataset that holds all the data (well, not quite all of it yet)</summary>
		private DataSet DS;
		///<summary>If the user has done a blockout/copy, then this will contain the blockout that is on the "clipboard".</summary>
		private Schedule BlockoutClipboard;
		///<summary>This has to be tracked globally because mouse might move directly from one appt to another without any break.  This is the only way to know if we are still over the same appt.</summary>
		private long bubbleAptNum;
		private DateTime bubbleTime;
		private Point bubbleLocation;
		private ODGrid gridEmpSched;
		private OpenDental.UI.ODPictureBox PicturePat;
		//private string PatCurName;
		//private int PatCurNum;
		private Timer timerInfoBubble;
		//private string PatCurChartNumber;
		private TabControl tabControl;
		private TabPage tabWaiting;
		private TabPage tabSched;
		private ODGrid gridWaiting;
		private OpenDental.UI.Button butMonth;
		//<summary></summary>
		//public static Size PinboardSize=new Size(106,92);
		private PinBoard pinBoard;
		//private ContrApptSingle PinApptSingle;
		///<summary>Local computer time.  Used by waiting room feature as delta time for display refresh.</summary>
		private DateTime LastTimeDataRetrieved;
		private Timer timerWaitingRoom;
		private OpenDental.UI.Button butBackMonth;
		private OpenDental.UI.Button butFwdMonth;
		private OpenDental.UI.Button butBackWeek;
		private OpenDental.UI.Button butFwdWeek;
		private RadioButton radioDay;
		private RadioButton radioWeek;
		private bool InitializedOnStartup;
		public Patient PatCur;
		private FormRecallList FormRecallL;
		private FormASAP FormASAP;
		private FormConfirmList FormConfirmL;
		private OpenDental.UI.Button butGraph;
		private Timer timerTests;
		//private int stressCounter;
		///<summary>When a popup happens durring attempted drag off pinboard, this helps cancel the drag.</summary>
		private bool CancelPinMouseDown;
		private DateTime apptPrintStartTime;
		private DateTime apptPrintStopTime;
		private int apptPrintFontSize;
		private UI.Button butProvPick;
		private int apptPrintColsPerPage;
		private GroupBox groupBox1;
		private UI.Button butProvHygenist;
		private UI.Button butProvDentist;
		public List<Provider> ProviderList;
		private int pagesPrinted;
		private int pageRow;
		private int pageColumn;
		private UI.Button butFamRecall;
		private UI.Button butViewAppts;
		private UI.Button butMakeRecall;
		private Panel panelMakeButtons;
		private List<DisplayField> _aptBubbleDefs;
		///<summary>This is a list of ApptViews that are available in comboView, which will be filtered for the currently selected clinic if clincs are
		///enabled.  This list will contain the same number of items as comboView minus 1 for 'none' and is filled at the same time as comboView.
		///Use this list when accessing the view by comboView.SelectedIndex.</summary>
		private List<ApptView> _listApptViews;
		private FormTrackNext FormTN;
		private TabPage tabProv;
		private ODGrid gridProv;
		private FormUnsched FormUnsched2;
		///<summary></summary>
		[Category("Data"),Description("Occurs when a user has taken action on an item needing an action taken.")]
		public event ActionNeededEventHandler ActionTaken=null;
		private bool _isPrintPreview;
		private Label labelNoneView;
		private TabPage tabReminders;
		private ODGrid gridReminders;
		private ImageList imageListTasks;
		private ContextMenu menuReminderEdit;
		private MenuItem menuItemReminderDone;
		private MenuItem menuItemReminderGoto;

		///<summary>Used to determine whether the scrollbar position needs to be set.</summary>
		private bool _hasLayedOutScrollBar=false;

		///<summary></summary>
		public ContrAppt() {
			Logger.openlog.Log("Initializing appointment module...",Logger.Severity.INFO);
			InitializeComponent();// This call is required by the Windows.Forms Form Designer.
			menuWeeklyApt=new System.Windows.Forms.ContextMenu();
			infoBubble=new Panel();
			infoBubble.Visible=false;
			infoBubble.Size=new Size(200,300);
			infoBubble.MouseMove+=new MouseEventHandler(InfoBubble_MouseMove);
			infoBubble.BackColor=Color.FromArgb(255,250,190);
			PicturePat=new OpenDental.UI.ODPictureBox();
			PicturePat.Location=new Point(6,17);
			PicturePat.Size=new Size(100,100);
			PicturePat.BackColor=Color.FromArgb(232,220,190);
			PicturePat.TextNullImage=Lan.g(this,"Patient Picture Unavailable");
			PicturePat.MouseMove+=new MouseEventHandler(PicturePat_MouseMove);
			infoBubble.Controls.Clear();
			infoBubble.Controls.Add(PicturePat);
			this.Controls.Add(infoBubble);
			ContrApptSheet2.MouseWheel+=new MouseEventHandler(ContrApptSheet2_MouseWheel);
			_listApptViews=new List<ApptView>();
			Lan.C(this,menuReminderEdit);
			gridReminders.ContextMenu=menuReminderEdit;
		}

		///<summary></summary>
		protected override void Dispose(bool disposing) {
			if(disposing) {
				if(components != null) {
					components.Dispose();
				}
				for(int i=0;i<pinBoard.ApptList.Count;i++) {
					DataRow row=pinBoard.ApptList[i].DataRoww;
					if(row["AptStatus"].ToString()==((int)ApptStatus.UnschedList).ToString() && PIn.DateT(row["AptDateTime"].ToString()).Year<1880) {
						Appointment aptCur=Appointments.GetOneApt(PIn.Long(row["AptNum"].ToString()));
						if(aptCur.AptDateTime.Year<1880) {//if the date was not updated since put on the pinboard
							Appointments.Delete(aptCur.AptNum);
							string logText=Lan.g(this,"Deleted from pinboard while closing Open Dental")+": ";
							if(aptCur.AptDateTime.Year>1880) {
								logText+=aptCur.AptDateTime.ToString()+", ";
							}
							logText+=aptCur.ProcDescript;
							SecurityLogs.MakeLogEntry(Permissions.AppointmentEdit,aptCur.PatNum,logText);
						}
					}
				}
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ContrAppt));
			this.imageListMain = new System.Windows.Forms.ImageList(this.components);
			this.Calendar2 = new System.Windows.Forms.MonthCalendar();
			this.labelDate = new System.Windows.Forms.Label();
			this.labelDate2 = new System.Windows.Forms.Label();
			this.panelArrows = new System.Windows.Forms.Panel();
			this.butBackMonth = new OpenDental.UI.Button();
			this.butFwdMonth = new OpenDental.UI.Button();
			this.butBackWeek = new OpenDental.UI.Button();
			this.butFwdWeek = new OpenDental.UI.Button();
			this.butToday = new OpenDental.UI.Button();
			this.butBack = new OpenDental.UI.Button();
			this.butFwd = new OpenDental.UI.Button();
			this.panelSheet = new System.Windows.Forms.Panel();
			this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
			this.ContrApptSheet2 = new OpenDental.ContrApptSheet();
			this.labelNoneView = new System.Windows.Forms.Label();
			this.panelAptInfo = new System.Windows.Forms.Panel();
			this.listConfirmed = new System.Windows.Forms.ListBox();
			this.butComplete = new System.Windows.Forms.Button();
			this.butUnsched = new System.Windows.Forms.Button();
			this.butDelete = new System.Windows.Forms.Button();
			this.butBreak = new System.Windows.Forms.Button();
			this.panelCalendar = new System.Windows.Forms.Panel();
			this.radioWeek = new System.Windows.Forms.RadioButton();
			this.radioDay = new System.Windows.Forms.RadioButton();
			this.butGraph = new OpenDental.UI.Button();
			this.butMonth = new OpenDental.UI.Button();
			this.pinBoard = new OpenDental.UI.PinBoard();
			this.butLab = new OpenDental.UI.Button();
			this.butSearch = new OpenDental.UI.Button();
			this.textProduction = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.textLab = new System.Windows.Forms.TextBox();
			this.comboView = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.butClearPin = new OpenDental.UI.Button();
			this.panelOps = new System.Windows.Forms.Panel();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.pd2 = new System.Drawing.Printing.PrintDocument();
			this.menuApt = new System.Windows.Forms.ContextMenu();
			this.menuPatient = new System.Windows.Forms.ContextMenu();
			this.menuBlockout = new System.Windows.Forms.ContextMenu();
			this.groupSearch = new System.Windows.Forms.GroupBox();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.butProvHygenist = new OpenDental.UI.Button();
			this.butProvDentist = new OpenDental.UI.Button();
			this.butProvPick = new OpenDental.UI.Button();
			this.butRefresh = new OpenDental.UI.Button();
			this.listSearchResults = new System.Windows.Forms.ListBox();
			this.listProviders = new System.Windows.Forms.ListBox();
			this.butSearchClose = new OpenDental.UI.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.textAfter = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.radioBeforePM = new System.Windows.Forms.RadioButton();
			this.radioBeforeAM = new System.Windows.Forms.RadioButton();
			this.textBefore = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.radioAfterAM = new System.Windows.Forms.RadioButton();
			this.radioAfterPM = new System.Windows.Forms.RadioButton();
			this.dateSearch = new System.Windows.Forms.DateTimePicker();
			this.label9 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.butSearchCloseX = new System.Windows.Forms.Button();
			this.butSearchNext = new OpenDental.UI.Button();
			this.timerInfoBubble = new System.Windows.Forms.Timer(this.components);
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabWaiting = new System.Windows.Forms.TabPage();
			this.gridWaiting = new OpenDental.UI.ODGrid();
			this.tabSched = new System.Windows.Forms.TabPage();
			this.gridEmpSched = new OpenDental.UI.ODGrid();
			this.tabProv = new System.Windows.Forms.TabPage();
			this.gridProv = new OpenDental.UI.ODGrid();
			this.tabReminders = new System.Windows.Forms.TabPage();
			this.gridReminders = new OpenDental.UI.ODGrid();
			this.timerWaitingRoom = new System.Windows.Forms.Timer(this.components);
			this.timerTests = new System.Windows.Forms.Timer(this.components);
			this.panelMakeButtons = new System.Windows.Forms.Panel();
			this.butMakeAppt = new OpenDental.UI.Button();
			this.butFamRecall = new OpenDental.UI.Button();
			this.butMakeRecall = new OpenDental.UI.Button();
			this.butViewAppts = new OpenDental.UI.Button();
			this.imageListTasks = new System.Windows.Forms.ImageList(this.components);
			this.ToolBarMain = new OpenDental.UI.ODToolBar();
			this.menuReminderEdit = new System.Windows.Forms.ContextMenu();
			this.menuItemReminderDone = new System.Windows.Forms.MenuItem();
			this.menuItemReminderGoto = new System.Windows.Forms.MenuItem();
			this.panelArrows.SuspendLayout();
			this.panelSheet.SuspendLayout();
			this.panelAptInfo.SuspendLayout();
			this.panelCalendar.SuspendLayout();
			this.groupSearch.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tabWaiting.SuspendLayout();
			this.tabSched.SuspendLayout();
			this.tabProv.SuspendLayout();
			this.tabReminders.SuspendLayout();
			this.panelMakeButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// imageListMain
			// 
			this.imageListMain.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListMain.ImageStream")));
			this.imageListMain.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListMain.Images.SetKeyName(0, "Pat.gif");
			this.imageListMain.Images.SetKeyName(1, "print.gif");
			this.imageListMain.Images.SetKeyName(2, "apptLists.gif");
			this.imageListMain.Images.SetKeyName(3, "DT Rapid Call.png");
			// 
			// Calendar2
			// 
			this.Calendar2.Location = new System.Drawing.Point(0, 24);
			this.Calendar2.Name = "Calendar2";
			this.Calendar2.ScrollChange = 1;
			this.Calendar2.TabIndex = 23;
			this.Calendar2.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.Calendar2_DateSelected);
			// 
			// labelDate
			// 
			this.labelDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelDate.Location = new System.Drawing.Point(2, 4);
			this.labelDate.Name = "labelDate";
			this.labelDate.Size = new System.Drawing.Size(56, 16);
			this.labelDate.TabIndex = 24;
			this.labelDate.Text = "Wed";
			// 
			// labelDate2
			// 
			this.labelDate2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelDate2.Location = new System.Drawing.Point(46, 4);
			this.labelDate2.Name = "labelDate2";
			this.labelDate2.Size = new System.Drawing.Size(100, 20);
			this.labelDate2.TabIndex = 25;
			this.labelDate2.Text = "-  Oct 20";
			// 
			// panelArrows
			// 
			this.panelArrows.Controls.Add(this.butBackMonth);
			this.panelArrows.Controls.Add(this.butFwdMonth);
			this.panelArrows.Controls.Add(this.butBackWeek);
			this.panelArrows.Controls.Add(this.butFwdWeek);
			this.panelArrows.Controls.Add(this.butToday);
			this.panelArrows.Controls.Add(this.butBack);
			this.panelArrows.Controls.Add(this.butFwd);
			this.panelArrows.Location = new System.Drawing.Point(1, 189);
			this.panelArrows.Name = "panelArrows";
			this.panelArrows.Size = new System.Drawing.Size(217, 24);
			this.panelArrows.TabIndex = 32;
			// 
			// butBackMonth
			// 
			this.butBackMonth.AdjustImageLocation = new System.Drawing.Point(-3, -1);
			this.butBackMonth.Autosize = true;
			this.butBackMonth.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butBackMonth.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butBackMonth.CornerRadius = 4F;
			this.butBackMonth.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.butBackMonth.Image = ((System.Drawing.Image)(resources.GetObject("butBackMonth.Image")));
			this.butBackMonth.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butBackMonth.Location = new System.Drawing.Point(1, 0);
			this.butBackMonth.Name = "butBackMonth";
			this.butBackMonth.Size = new System.Drawing.Size(32, 22);
			this.butBackMonth.TabIndex = 57;
			this.butBackMonth.Text = "M";
			this.butBackMonth.Click += new System.EventHandler(this.butBackMonth_Click);
			// 
			// butFwdMonth
			// 
			this.butFwdMonth.AdjustImageLocation = new System.Drawing.Point(5, -1);
			this.butFwdMonth.Autosize = false;
			this.butFwdMonth.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butFwdMonth.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butFwdMonth.CornerRadius = 4F;
			this.butFwdMonth.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.butFwdMonth.Image = ((System.Drawing.Image)(resources.GetObject("butFwdMonth.Image")));
			this.butFwdMonth.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.butFwdMonth.Location = new System.Drawing.Point(188, 0);
			this.butFwdMonth.Name = "butFwdMonth";
			this.butFwdMonth.Size = new System.Drawing.Size(29, 22);
			this.butFwdMonth.TabIndex = 56;
			this.butFwdMonth.Text = "M";
			this.butFwdMonth.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butFwdMonth.Click += new System.EventHandler(this.butFwdMonth_Click);
			// 
			// butBackWeek
			// 
			this.butBackWeek.AdjustImageLocation = new System.Drawing.Point(-3, -1);
			this.butBackWeek.Autosize = true;
			this.butBackWeek.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butBackWeek.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butBackWeek.CornerRadius = 4F;
			this.butBackWeek.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.butBackWeek.Image = ((System.Drawing.Image)(resources.GetObject("butBackWeek.Image")));
			this.butBackWeek.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butBackWeek.Location = new System.Drawing.Point(33, 0);
			this.butBackWeek.Name = "butBackWeek";
			this.butBackWeek.Size = new System.Drawing.Size(33, 22);
			this.butBackWeek.TabIndex = 55;
			this.butBackWeek.Text = "W";
			this.butBackWeek.Click += new System.EventHandler(this.butBackWeek_Click);
			// 
			// butFwdWeek
			// 
			this.butFwdWeek.AdjustImageLocation = new System.Drawing.Point(5, -1);
			this.butFwdWeek.Autosize = false;
			this.butFwdWeek.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butFwdWeek.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butFwdWeek.CornerRadius = 4F;
			this.butFwdWeek.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.butFwdWeek.Image = ((System.Drawing.Image)(resources.GetObject("butFwdWeek.Image")));
			this.butFwdWeek.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.butFwdWeek.Location = new System.Drawing.Point(158, 0);
			this.butFwdWeek.Name = "butFwdWeek";
			this.butFwdWeek.Size = new System.Drawing.Size(30, 22);
			this.butFwdWeek.TabIndex = 54;
			this.butFwdWeek.Text = "W";
			this.butFwdWeek.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butFwdWeek.Click += new System.EventHandler(this.butFwdWeek_Click);
			// 
			// butToday
			// 
			this.butToday.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butToday.Autosize = false;
			this.butToday.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butToday.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butToday.CornerRadius = 4F;
			this.butToday.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.butToday.Location = new System.Drawing.Point(85, 0);
			this.butToday.Name = "butToday";
			this.butToday.Size = new System.Drawing.Size(54, 22);
			this.butToday.TabIndex = 29;
			this.butToday.Text = "Today";
			this.butToday.Click += new System.EventHandler(this.butToday_Click);
			// 
			// butBack
			// 
			this.butBack.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butBack.Autosize = true;
			this.butBack.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butBack.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butBack.CornerRadius = 4F;
			this.butBack.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.butBack.Image = ((System.Drawing.Image)(resources.GetObject("butBack.Image")));
			this.butBack.Location = new System.Drawing.Point(66, 0);
			this.butBack.Name = "butBack";
			this.butBack.Size = new System.Drawing.Size(19, 22);
			this.butBack.TabIndex = 51;
			this.butBack.Click += new System.EventHandler(this.butBack_Click);
			// 
			// butFwd
			// 
			this.butFwd.AdjustImageLocation = new System.Drawing.Point(5, -1);
			this.butFwd.Autosize = false;
			this.butFwd.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butFwd.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butFwd.CornerRadius = 4F;
			this.butFwd.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.butFwd.Image = ((System.Drawing.Image)(resources.GetObject("butFwd.Image")));
			this.butFwd.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.butFwd.Location = new System.Drawing.Point(139, 0);
			this.butFwd.Name = "butFwd";
			this.butFwd.Size = new System.Drawing.Size(19, 22);
			this.butFwd.TabIndex = 53;
			this.butFwd.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butFwd.Click += new System.EventHandler(this.butFwd_Click);
			// 
			// panelSheet
			// 
			this.panelSheet.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelSheet.Controls.Add(this.vScrollBar1);
			this.panelSheet.Controls.Add(this.ContrApptSheet2);
			this.panelSheet.Controls.Add(this.labelNoneView);
			this.panelSheet.Location = new System.Drawing.Point(0, 17);
			this.panelSheet.Name = "panelSheet";
			this.panelSheet.Size = new System.Drawing.Size(235, 726);
			this.panelSheet.TabIndex = 44;
			this.panelSheet.Resize += new System.EventHandler(this.panelSheet_Resize);
			// 
			// vScrollBar1
			// 
			this.vScrollBar1.Dock = System.Windows.Forms.DockStyle.Right;
			this.vScrollBar1.Location = new System.Drawing.Point(216, 0);
			this.vScrollBar1.Name = "vScrollBar1";
			this.vScrollBar1.Size = new System.Drawing.Size(17, 724);
			this.vScrollBar1.TabIndex = 23;
			this.vScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.vScrollBar1_Scroll);
			// 
			// ContrApptSheet2
			// 
			this.ContrApptSheet2.Location = new System.Drawing.Point(2, -550);
			this.ContrApptSheet2.Name = "ContrApptSheet2";
			this.ContrApptSheet2.Size = new System.Drawing.Size(60, 1728);
			this.ContrApptSheet2.TabIndex = 22;
			this.ContrApptSheet2.DoubleClick += new System.EventHandler(this.ContrApptSheet2_DoubleClick);
			this.ContrApptSheet2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ContrApptSheet2_MouseDown);
			this.ContrApptSheet2.MouseLeave += new System.EventHandler(this.ContrApptSheet2_MouseLeave);
			this.ContrApptSheet2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ContrApptSheet2_MouseMove);
			this.ContrApptSheet2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ContrApptSheet2_MouseUp);
			// 
			// labelNoneView
			// 
			this.labelNoneView.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.labelNoneView.AutoSize = true;
			this.labelNoneView.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labelNoneView.Location = new System.Drawing.Point(-57, 248);
			this.labelNoneView.Name = "labelNoneView";
			this.labelNoneView.Size = new System.Drawing.Size(324, 66);
			this.labelNoneView.TabIndex = 83;
			this.labelNoneView.Text = "Please select a clinic \r\nor an appointment view.";
			this.labelNoneView.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// panelAptInfo
			// 
			this.panelAptInfo.Controls.Add(this.listConfirmed);
			this.panelAptInfo.Controls.Add(this.butComplete);
			this.panelAptInfo.Controls.Add(this.butUnsched);
			this.panelAptInfo.Controls.Add(this.butDelete);
			this.panelAptInfo.Controls.Add(this.butBreak);
			this.panelAptInfo.Location = new System.Drawing.Point(665, 404);
			this.panelAptInfo.Name = "panelAptInfo";
			this.panelAptInfo.Size = new System.Drawing.Size(107, 116);
			this.panelAptInfo.TabIndex = 45;
			// 
			// listConfirmed
			// 
			this.listConfirmed.IntegralHeight = false;
			this.listConfirmed.Location = new System.Drawing.Point(31, 2);
			this.listConfirmed.Name = "listConfirmed";
			this.listConfirmed.Size = new System.Drawing.Size(73, 111);
			this.listConfirmed.TabIndex = 75;
			this.listConfirmed.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listConfirmed_MouseDown);
			// 
			// butComplete
			// 
			this.butComplete.BackColor = System.Drawing.SystemColors.Control;
			this.butComplete.Image = ((System.Drawing.Image)(resources.GetObject("butComplete.Image")));
			this.butComplete.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
			this.butComplete.Location = new System.Drawing.Point(2, 57);
			this.butComplete.Name = "butComplete";
			this.butComplete.Size = new System.Drawing.Size(28, 28);
			this.butComplete.TabIndex = 69;
			this.butComplete.TabStop = false;
			this.butComplete.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butComplete.UseVisualStyleBackColor = false;
			this.butComplete.Click += new System.EventHandler(this.butComplete_Click);
			// 
			// butUnsched
			// 
			this.butUnsched.BackColor = System.Drawing.SystemColors.Control;
			this.butUnsched.Image = ((System.Drawing.Image)(resources.GetObject("butUnsched.Image")));
			this.butUnsched.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
			this.butUnsched.Location = new System.Drawing.Point(2, 1);
			this.butUnsched.Name = "butUnsched";
			this.butUnsched.Size = new System.Drawing.Size(28, 28);
			this.butUnsched.TabIndex = 68;
			this.butUnsched.TabStop = false;
			this.butUnsched.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butUnsched.UseVisualStyleBackColor = false;
			this.butUnsched.Click += new System.EventHandler(this.butUnsched_Click);
			// 
			// butDelete
			// 
			this.butDelete.BackColor = System.Drawing.SystemColors.Control;
			this.butDelete.Image = ((System.Drawing.Image)(resources.GetObject("butDelete.Image")));
			this.butDelete.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
			this.butDelete.Location = new System.Drawing.Point(2, 85);
			this.butDelete.Name = "butDelete";
			this.butDelete.Size = new System.Drawing.Size(28, 28);
			this.butDelete.TabIndex = 66;
			this.butDelete.TabStop = false;
			this.butDelete.UseVisualStyleBackColor = false;
			this.butDelete.Click += new System.EventHandler(this.butDelete_Click);
			// 
			// butBreak
			// 
			this.butBreak.BackColor = System.Drawing.SystemColors.Control;
			this.butBreak.Image = ((System.Drawing.Image)(resources.GetObject("butBreak.Image")));
			this.butBreak.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
			this.butBreak.Location = new System.Drawing.Point(2, 29);
			this.butBreak.Name = "butBreak";
			this.butBreak.Size = new System.Drawing.Size(28, 28);
			this.butBreak.TabIndex = 65;
			this.butBreak.TabStop = false;
			this.butBreak.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butBreak.UseVisualStyleBackColor = false;
			this.butBreak.Click += new System.EventHandler(this.butBreak_Click);
			// 
			// panelCalendar
			// 
			this.panelCalendar.Controls.Add(this.radioWeek);
			this.panelCalendar.Controls.Add(this.panelArrows);
			this.panelCalendar.Controls.Add(this.radioDay);
			this.panelCalendar.Controls.Add(this.butGraph);
			this.panelCalendar.Controls.Add(this.butMonth);
			this.panelCalendar.Controls.Add(this.pinBoard);
			this.panelCalendar.Controls.Add(this.butLab);
			this.panelCalendar.Controls.Add(this.butSearch);
			this.panelCalendar.Controls.Add(this.textProduction);
			this.panelCalendar.Controls.Add(this.label7);
			this.panelCalendar.Controls.Add(this.textLab);
			this.panelCalendar.Controls.Add(this.comboView);
			this.panelCalendar.Controls.Add(this.label2);
			this.panelCalendar.Controls.Add(this.butClearPin);
			this.panelCalendar.Controls.Add(this.Calendar2);
			this.panelCalendar.Controls.Add(this.labelDate);
			this.panelCalendar.Controls.Add(this.labelDate2);
			this.panelCalendar.Location = new System.Drawing.Point(665, 28);
			this.panelCalendar.Name = "panelCalendar";
			this.panelCalendar.Size = new System.Drawing.Size(219, 375);
			this.panelCalendar.TabIndex = 46;
			// 
			// radioWeek
			// 
			this.radioWeek.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioWeek.Location = new System.Drawing.Point(43, 238);
			this.radioWeek.Name = "radioWeek";
			this.radioWeek.Size = new System.Drawing.Size(68, 16);
			this.radioWeek.TabIndex = 92;
			this.radioWeek.Text = "Week";
			this.radioWeek.Click += new System.EventHandler(this.radioWeek_Click);
			// 
			// radioDay
			// 
			this.radioDay.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioDay.Location = new System.Drawing.Point(43, 218);
			this.radioDay.Name = "radioDay";
			this.radioDay.Size = new System.Drawing.Size(68, 16);
			this.radioDay.TabIndex = 91;
			this.radioDay.Text = "Day";
			this.radioDay.Click += new System.EventHandler(this.radioDay_Click);
			// 
			// butGraph
			// 
			this.butGraph.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butGraph.Autosize = true;
			this.butGraph.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butGraph.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butGraph.CornerRadius = 4F;
			this.butGraph.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butGraph.Location = new System.Drawing.Point(3, 309);
			this.butGraph.Name = "butGraph";
			this.butGraph.Size = new System.Drawing.Size(42, 24);
			this.butGraph.TabIndex = 78;
			this.butGraph.TabStop = false;
			this.butGraph.Text = "Emp";
			this.butGraph.Visible = false;
			this.butGraph.Click += new System.EventHandler(this.butGraph_Click);
			// 
			// butMonth
			// 
			this.butMonth.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butMonth.Autosize = false;
			this.butMonth.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butMonth.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butMonth.CornerRadius = 4F;
			this.butMonth.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.butMonth.Location = new System.Drawing.Point(152, 1);
			this.butMonth.Name = "butMonth";
			this.butMonth.Size = new System.Drawing.Size(65, 22);
			this.butMonth.TabIndex = 79;
			this.butMonth.Text = "Month";
			this.butMonth.Visible = false;
			this.butMonth.Click += new System.EventHandler(this.butMonth_Click);
			// 
			// pinBoard
			// 
			this.pinBoard.Location = new System.Drawing.Point(119, 213);
			this.pinBoard.Name = "pinBoard";
			this.pinBoard.SelectedIndex = -1;
			this.pinBoard.Size = new System.Drawing.Size(99, 96);
			this.pinBoard.TabIndex = 78;
			this.pinBoard.Text = "pinBoard";
			this.pinBoard.SelectedIndexChanged += new System.EventHandler(this.pinBoard_SelectedIndexChanged);
			this.pinBoard.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pinBoard_MouseDown);
			this.pinBoard.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pinBoard_MouseMove);
			this.pinBoard.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pinBoard_MouseUp);
			// 
			// butLab
			// 
			this.butLab.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butLab.Autosize = true;
			this.butLab.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butLab.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butLab.CornerRadius = 4F;
			this.butLab.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butLab.Location = new System.Drawing.Point(3, 333);
			this.butLab.Name = "butLab";
			this.butLab.Size = new System.Drawing.Size(79, 21);
			this.butLab.TabIndex = 77;
			this.butLab.Text = "Lab Cases";
			this.butLab.Click += new System.EventHandler(this.butLab_Click);
			// 
			// butSearch
			// 
			this.butSearch.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butSearch.Autosize = true;
			this.butSearch.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butSearch.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butSearch.CornerRadius = 4F;
			this.butSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butSearch.Location = new System.Drawing.Point(43, 285);
			this.butSearch.Name = "butSearch";
			this.butSearch.Size = new System.Drawing.Size(75, 24);
			this.butSearch.TabIndex = 40;
			this.butSearch.Text = "Search";
			this.butSearch.Click += new System.EventHandler(this.butSearch_Click);
			// 
			// textProduction
			// 
			this.textProduction.BackColor = System.Drawing.Color.White;
			this.textProduction.Location = new System.Drawing.Point(85, 353);
			this.textProduction.Name = "textProduction";
			this.textProduction.ReadOnly = true;
			this.textProduction.Size = new System.Drawing.Size(133, 20);
			this.textProduction.TabIndex = 38;
			this.textProduction.Text = "$100";
			this.textProduction.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(16, 357);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(68, 15);
			this.label7.TabIndex = 39;
			this.label7.Text = "Daily Prod";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textLab
			// 
			this.textLab.BackColor = System.Drawing.Color.White;
			this.textLab.Location = new System.Drawing.Point(85, 333);
			this.textLab.Name = "textLab";
			this.textLab.ReadOnly = true;
			this.textLab.Size = new System.Drawing.Size(133, 20);
			this.textLab.TabIndex = 36;
			this.textLab.Text = "All Received";
			this.textLab.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// comboView
			// 
			this.comboView.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboView.Location = new System.Drawing.Point(85, 312);
			this.comboView.MaxDropDownItems = 30;
			this.comboView.Name = "comboView";
			this.comboView.Size = new System.Drawing.Size(133, 21);
			this.comboView.TabIndex = 35;
			this.comboView.SelectionChangeCommitted += new System.EventHandler(this.comboView_SelectionChangeCommitted);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(17, 314);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(66, 16);
			this.label2.TabIndex = 34;
			this.label2.Text = "View";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// butClearPin
			// 
			this.butClearPin.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butClearPin.Autosize = true;
			this.butClearPin.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butClearPin.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butClearPin.CornerRadius = 4F;
			this.butClearPin.Image = ((System.Drawing.Image)(resources.GetObject("butClearPin.Image")));
			this.butClearPin.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butClearPin.Location = new System.Drawing.Point(43, 260);
			this.butClearPin.Name = "butClearPin";
			this.butClearPin.Size = new System.Drawing.Size(75, 24);
			this.butClearPin.TabIndex = 33;
			this.butClearPin.Text = "Clear";
			this.butClearPin.Click += new System.EventHandler(this.butClearPin_Click);
			// 
			// panelOps
			// 
			this.panelOps.Location = new System.Drawing.Point(0, 0);
			this.panelOps.Name = "panelOps";
			this.panelOps.Size = new System.Drawing.Size(676, 17);
			this.panelOps.TabIndex = 48;
			// 
			// toolTip1
			// 
			this.toolTip1.AutoPopDelay = 5000;
			this.toolTip1.InitialDelay = 100;
			this.toolTip1.ReshowDelay = 100;
			// 
			// pd2
			// 
			this.pd2.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.pd2_PrintPage);
			// 
			// groupSearch
			// 
			this.groupSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.groupSearch.Controls.Add(this.groupBox1);
			this.groupSearch.Controls.Add(this.butProvPick);
			this.groupSearch.Controls.Add(this.butRefresh);
			this.groupSearch.Controls.Add(this.listSearchResults);
			this.groupSearch.Controls.Add(this.listProviders);
			this.groupSearch.Controls.Add(this.butSearchClose);
			this.groupSearch.Controls.Add(this.groupBox2);
			this.groupSearch.Controls.Add(this.label8);
			this.groupSearch.Controls.Add(this.butSearchCloseX);
			this.groupSearch.Controls.Add(this.butSearchNext);
			this.groupSearch.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupSearch.Location = new System.Drawing.Point(380, 340);
			this.groupSearch.Name = "groupSearch";
			this.groupSearch.Size = new System.Drawing.Size(219, 366);
			this.groupSearch.TabIndex = 74;
			this.groupSearch.TabStop = false;
			this.groupSearch.Text = "Search For Opening";
			this.groupSearch.Visible = false;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.butProvHygenist);
			this.groupBox1.Controls.Add(this.butProvDentist);
			this.groupBox1.Location = new System.Drawing.Point(130, 253);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(85, 63);
			this.groupBox1.TabIndex = 89;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Search by";
			// 
			// butProvHygenist
			// 
			this.butProvHygenist.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butProvHygenist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butProvHygenist.Autosize = true;
			this.butProvHygenist.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butProvHygenist.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butProvHygenist.CornerRadius = 4F;
			this.butProvHygenist.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butProvHygenist.Location = new System.Drawing.Point(6, 37);
			this.butProvHygenist.Name = "butProvHygenist";
			this.butProvHygenist.Size = new System.Drawing.Size(73, 22);
			this.butProvHygenist.TabIndex = 92;
			this.butProvHygenist.Text = "Hygienists";
			this.butProvHygenist.Click += new System.EventHandler(this.butProvHygenist_Click);
			// 
			// butProvDentist
			// 
			this.butProvDentist.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butProvDentist.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butProvDentist.Autosize = true;
			this.butProvDentist.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butProvDentist.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butProvDentist.CornerRadius = 4F;
			this.butProvDentist.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butProvDentist.Location = new System.Drawing.Point(6, 14);
			this.butProvDentist.Name = "butProvDentist";
			this.butProvDentist.Size = new System.Drawing.Size(73, 22);
			this.butProvDentist.TabIndex = 91;
			this.butProvDentist.Text = "Providers";
			this.butProvDentist.Click += new System.EventHandler(this.butProvDentist_Click);
			// 
			// butProvPick
			// 
			this.butProvPick.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butProvPick.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butProvPick.Autosize = true;
			this.butProvPick.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butProvPick.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butProvPick.CornerRadius = 4F;
			this.butProvPick.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butProvPick.Location = new System.Drawing.Point(6, 340);
			this.butProvPick.Name = "butProvPick";
			this.butProvPick.Size = new System.Drawing.Size(82, 22);
			this.butProvPick.TabIndex = 88;
			this.butProvPick.Text = "Providers...";
			this.butProvPick.Click += new System.EventHandler(this.butProvPick_Click);
			// 
			// butRefresh
			// 
			this.butRefresh.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.butRefresh.Autosize = true;
			this.butRefresh.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butRefresh.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butRefresh.CornerRadius = 4F;
			this.butRefresh.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butRefresh.Location = new System.Drawing.Point(153, 318);
			this.butRefresh.Name = "butRefresh";
			this.butRefresh.Size = new System.Drawing.Size(62, 22);
			this.butRefresh.TabIndex = 88;
			this.butRefresh.Text = "Search";
			this.butRefresh.Click += new System.EventHandler(this.butRefresh_Click);
			// 
			// listSearchResults
			// 
			this.listSearchResults.IntegralHeight = false;
			this.listSearchResults.Location = new System.Drawing.Point(6, 32);
			this.listSearchResults.Name = "listSearchResults";
			this.listSearchResults.Size = new System.Drawing.Size(193, 134);
			this.listSearchResults.TabIndex = 87;
			this.listSearchResults.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listSearchResults_MouseDown);
			// 
			// listProviders
			// 
			this.listProviders.Location = new System.Drawing.Point(6, 269);
			this.listProviders.Name = "listProviders";
			this.listProviders.SelectionMode = System.Windows.Forms.SelectionMode.None;
			this.listProviders.Size = new System.Drawing.Size(118, 69);
			this.listProviders.TabIndex = 86;
			// 
			// butSearchClose
			// 
			this.butSearchClose.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butSearchClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butSearchClose.Autosize = true;
			this.butSearchClose.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butSearchClose.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butSearchClose.CornerRadius = 4F;
			this.butSearchClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butSearchClose.Location = new System.Drawing.Point(153, 342);
			this.butSearchClose.Name = "butSearchClose";
			this.butSearchClose.Size = new System.Drawing.Size(62, 22);
			this.butSearchClose.TabIndex = 85;
			this.butSearchClose.Text = "Close";
			this.butSearchClose.Click += new System.EventHandler(this.butSearchClose_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.textAfter);
			this.groupBox2.Controls.Add(this.label11);
			this.groupBox2.Controls.Add(this.radioBeforePM);
			this.groupBox2.Controls.Add(this.radioBeforeAM);
			this.groupBox2.Controls.Add(this.textBefore);
			this.groupBox2.Controls.Add(this.label10);
			this.groupBox2.Controls.Add(this.panel1);
			this.groupBox2.Controls.Add(this.dateSearch);
			this.groupBox2.Controls.Add(this.label9);
			this.groupBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupBox2.Location = new System.Drawing.Point(6, 168);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(193, 84);
			this.groupBox2.TabIndex = 84;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Date/Time Restrictions";
			// 
			// textAfter
			// 
			this.textAfter.Location = new System.Drawing.Point(57, 60);
			this.textAfter.Name = "textAfter";
			this.textAfter.Size = new System.Drawing.Size(44, 20);
			this.textAfter.TabIndex = 88;
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(1, 62);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(53, 16);
			this.label11.TabIndex = 87;
			this.label11.Text = "After";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// radioBeforePM
			// 
			this.radioBeforePM.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioBeforePM.Location = new System.Drawing.Point(151, 41);
			this.radioBeforePM.Name = "radioBeforePM";
			this.radioBeforePM.Size = new System.Drawing.Size(37, 15);
			this.radioBeforePM.TabIndex = 86;
			this.radioBeforePM.Text = "pm";
			// 
			// radioBeforeAM
			// 
			this.radioBeforeAM.Checked = true;
			this.radioBeforeAM.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioBeforeAM.Location = new System.Drawing.Point(108, 41);
			this.radioBeforeAM.Name = "radioBeforeAM";
			this.radioBeforeAM.Size = new System.Drawing.Size(37, 15);
			this.radioBeforeAM.TabIndex = 85;
			this.radioBeforeAM.TabStop = true;
			this.radioBeforeAM.Text = "am";
			// 
			// textBefore
			// 
			this.textBefore.Location = new System.Drawing.Point(57, 38);
			this.textBefore.Name = "textBefore";
			this.textBefore.Size = new System.Drawing.Size(44, 20);
			this.textBefore.TabIndex = 84;
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(1, 40);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(53, 16);
			this.label10.TabIndex = 83;
			this.label10.Text = "Before";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.radioAfterAM);
			this.panel1.Controls.Add(this.radioAfterPM);
			this.panel1.Location = new System.Drawing.Point(105, 60);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(84, 20);
			this.panel1.TabIndex = 86;
			// 
			// radioAfterAM
			// 
			this.radioAfterAM.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioAfterAM.Location = new System.Drawing.Point(3, 2);
			this.radioAfterAM.Name = "radioAfterAM";
			this.radioAfterAM.Size = new System.Drawing.Size(37, 15);
			this.radioAfterAM.TabIndex = 89;
			this.radioAfterAM.Text = "am";
			// 
			// radioAfterPM
			// 
			this.radioAfterPM.Checked = true;
			this.radioAfterPM.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioAfterPM.Location = new System.Drawing.Point(46, 2);
			this.radioAfterPM.Name = "radioAfterPM";
			this.radioAfterPM.Size = new System.Drawing.Size(36, 15);
			this.radioAfterPM.TabIndex = 90;
			this.radioAfterPM.TabStop = true;
			this.radioAfterPM.Text = "pm";
			// 
			// dateSearch
			// 
			this.dateSearch.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateSearch.Location = new System.Drawing.Point(57, 16);
			this.dateSearch.Name = "dateSearch";
			this.dateSearch.Size = new System.Drawing.Size(130, 20);
			this.dateSearch.TabIndex = 90;
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(1, 19);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(54, 16);
			this.label9.TabIndex = 89;
			this.label9.Text = "After";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(6, 251);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(92, 16);
			this.label8.TabIndex = 80;
			this.label8.Text = "Providers";
			this.label8.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// butSearchCloseX
			// 
			this.butSearchCloseX.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.butSearchCloseX.ForeColor = System.Drawing.SystemColors.Control;
			this.butSearchCloseX.Image = ((System.Drawing.Image)(resources.GetObject("butSearchCloseX.Image")));
			this.butSearchCloseX.Location = new System.Drawing.Point(185, 7);
			this.butSearchCloseX.Name = "butSearchCloseX";
			this.butSearchCloseX.Size = new System.Drawing.Size(16, 16);
			this.butSearchCloseX.TabIndex = 0;
			this.butSearchCloseX.Click += new System.EventHandler(this.butSearchCloseX_Click);
			// 
			// butSearchNext
			// 
			this.butSearchNext.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butSearchNext.Autosize = true;
			this.butSearchNext.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butSearchNext.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butSearchNext.CornerRadius = 4F;
			this.butSearchNext.Image = ((System.Drawing.Image)(resources.GetObject("butSearchNext.Image")));
			this.butSearchNext.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.butSearchNext.Location = new System.Drawing.Point(111, 9);
			this.butSearchNext.Name = "butSearchNext";
			this.butSearchNext.Size = new System.Drawing.Size(71, 22);
			this.butSearchNext.TabIndex = 77;
			this.butSearchNext.Text = "More";
			this.butSearchNext.Click += new System.EventHandler(this.butSearchMore_Click);
			// 
			// timerInfoBubble
			// 
			this.timerInfoBubble.Interval = 300;
			this.timerInfoBubble.Tick += new System.EventHandler(this.timerInfoBubble_Tick);
			// 
			// tabControl
			// 
			this.tabControl.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.tabControl.Controls.Add(this.tabWaiting);
			this.tabControl.Controls.Add(this.tabSched);
			this.tabControl.Controls.Add(this.tabProv);
			this.tabControl.Controls.Add(this.tabReminders);
			this.tabControl.Location = new System.Drawing.Point(665, 521);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(219, 187);
			this.tabControl.TabIndex = 78;
			// 
			// tabWaiting
			// 
			this.tabWaiting.Controls.Add(this.gridWaiting);
			this.tabWaiting.Location = new System.Drawing.Point(4, 22);
			this.tabWaiting.Name = "tabWaiting";
			this.tabWaiting.Padding = new System.Windows.Forms.Padding(3);
			this.tabWaiting.Size = new System.Drawing.Size(211, 161);
			this.tabWaiting.TabIndex = 0;
			this.tabWaiting.Text = "Waiting";
			this.tabWaiting.UseVisualStyleBackColor = true;
			// 
			// gridWaiting
			// 
			this.gridWaiting.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridWaiting.HasAddButton = false;
			this.gridWaiting.HasMultilineHeaders = false;
			this.gridWaiting.HeaderHeight = 15;
			this.gridWaiting.HScrollVisible = false;
			this.gridWaiting.Location = new System.Drawing.Point(0, 0);
			this.gridWaiting.Margin = new System.Windows.Forms.Padding(0);
			this.gridWaiting.Name = "gridWaiting";
			this.gridWaiting.ScrollValue = 0;
			this.gridWaiting.Size = new System.Drawing.Size(211, 161);
			this.gridWaiting.TabIndex = 78;
			this.gridWaiting.Title = "Waiting Room";
			this.gridWaiting.TitleHeight = 18;
			this.gridWaiting.TranslationName = "TableApptWaiting";
			// 
			// tabSched
			// 
			this.tabSched.Controls.Add(this.gridEmpSched);
			this.tabSched.Location = new System.Drawing.Point(4, 22);
			this.tabSched.Name = "tabSched";
			this.tabSched.Padding = new System.Windows.Forms.Padding(3);
			this.tabSched.Size = new System.Drawing.Size(211, 161);
			this.tabSched.TabIndex = 1;
			this.tabSched.Text = "Emp";
			this.tabSched.UseVisualStyleBackColor = true;
			// 
			// gridEmpSched
			// 
			this.gridEmpSched.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridEmpSched.HasAddButton = false;
			this.gridEmpSched.HasMultilineHeaders = false;
			this.gridEmpSched.HeaderHeight = 15;
			this.gridEmpSched.HScrollVisible = true;
			this.gridEmpSched.Location = new System.Drawing.Point(0, 0);
			this.gridEmpSched.Margin = new System.Windows.Forms.Padding(0);
			this.gridEmpSched.Name = "gridEmpSched";
			this.gridEmpSched.ScrollValue = 0;
			this.gridEmpSched.Size = new System.Drawing.Size(211, 161);
			this.gridEmpSched.TabIndex = 77;
			this.gridEmpSched.Title = "Employee Schedules";
			this.gridEmpSched.TitleHeight = 18;
			this.gridEmpSched.TranslationName = "TableApptEmpSched";
			this.gridEmpSched.DoubleClick += new System.EventHandler(this.gridEmpSched_DoubleClick);
			// 
			// tabProv
			// 
			this.tabProv.Controls.Add(this.gridProv);
			this.tabProv.Location = new System.Drawing.Point(4, 22);
			this.tabProv.Name = "tabProv";
			this.tabProv.Size = new System.Drawing.Size(211, 161);
			this.tabProv.TabIndex = 2;
			this.tabProv.Text = "Prov";
			this.tabProv.UseVisualStyleBackColor = true;
			// 
			// gridProv
			// 
			this.gridProv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridProv.HasAddButton = false;
			this.gridProv.HasMultilineHeaders = false;
			this.gridProv.HeaderHeight = 15;
			this.gridProv.HScrollVisible = true;
			this.gridProv.Location = new System.Drawing.Point(0, 0);
			this.gridProv.Margin = new System.Windows.Forms.Padding(0);
			this.gridProv.Name = "gridProv";
			this.gridProv.ScrollValue = 0;
			this.gridProv.Size = new System.Drawing.Size(211, 161);
			this.gridProv.TabIndex = 79;
			this.gridProv.Title = "Provider Schedules";
			this.gridProv.TitleHeight = 18;
			this.gridProv.TranslationName = "TableAppProv";
			this.gridProv.CellDoubleClick += new OpenDental.UI.ODGridClickEventHandler(this.gridProv_CellDoubleClick);
			// 
			// tabReminders
			// 
			this.tabReminders.Controls.Add(this.gridReminders);
			this.tabReminders.Location = new System.Drawing.Point(4, 22);
			this.tabReminders.Name = "tabReminders";
			this.tabReminders.Size = new System.Drawing.Size(211, 161);
			this.tabReminders.TabIndex = 3;
			this.tabReminders.Text = "Reminders";
			this.tabReminders.UseVisualStyleBackColor = true;
			// 
			// gridReminders
			// 
			this.gridReminders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridReminders.HasAddButton = false;
			this.gridReminders.HasMultilineHeaders = false;
			this.gridReminders.HeaderHeight = 15;
			this.gridReminders.HScrollVisible = false;
			this.gridReminders.Location = new System.Drawing.Point(0, 0);
			this.gridReminders.Name = "gridReminders";
			this.gridReminders.ScrollValue = 0;
			this.gridReminders.Size = new System.Drawing.Size(211, 161);
			this.gridReminders.TabIndex = 0;
			this.gridReminders.Title = "Reminders";
			this.gridReminders.TitleHeight = 18;
			this.gridReminders.TranslationName = null;
			this.gridReminders.CellDoubleClick += new OpenDental.UI.ODGridClickEventHandler(this.gridReminders_CellDoubleClick);
			this.gridReminders.MouseDown += new System.Windows.Forms.MouseEventHandler(this.gridReminders_MouseDown);
			// 
			// timerWaitingRoom
			// 
			this.timerWaitingRoom.Enabled = true;
			this.timerWaitingRoom.Interval = 1000;
			this.timerWaitingRoom.Tick += new System.EventHandler(this.timerWaitingRoom_Tick);
			// 
			// timerTests
			// 
			this.timerTests.Tick += new System.EventHandler(this.timerTests_Tick);
			// 
			// panelMakeButtons
			// 
			this.panelMakeButtons.Controls.Add(this.butMakeAppt);
			this.panelMakeButtons.Controls.Add(this.butFamRecall);
			this.panelMakeButtons.Controls.Add(this.butMakeRecall);
			this.panelMakeButtons.Controls.Add(this.butViewAppts);
			this.panelMakeButtons.Location = new System.Drawing.Point(772, 404);
			this.panelMakeButtons.Name = "panelMakeButtons";
			this.panelMakeButtons.Size = new System.Drawing.Size(112, 116);
			this.panelMakeButtons.TabIndex = 82;
			// 
			// butMakeAppt
			// 
			this.butMakeAppt.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butMakeAppt.Autosize = true;
			this.butMakeAppt.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butMakeAppt.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butMakeAppt.CornerRadius = 4F;
			this.butMakeAppt.Image = ((System.Drawing.Image)(resources.GetObject("butMakeAppt.Image")));
			this.butMakeAppt.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butMakeAppt.Location = new System.Drawing.Point(5, 5);
			this.butMakeAppt.Name = "butMakeAppt";
			this.butMakeAppt.Size = new System.Drawing.Size(103, 24);
			this.butMakeAppt.TabIndex = 76;
			this.butMakeAppt.TabStop = false;
			this.butMakeAppt.Text = "Make Appt";
			this.butMakeAppt.Click += new System.EventHandler(this.butMakeAppt_Click);
			// 
			// butFamRecall
			// 
			this.butFamRecall.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butFamRecall.Autosize = true;
			this.butFamRecall.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butFamRecall.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butFamRecall.CornerRadius = 4F;
			this.butFamRecall.Image = global::OpenDental.Properties.Resources.butRecall;
			this.butFamRecall.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butFamRecall.Location = new System.Drawing.Point(5, 57);
			this.butFamRecall.Name = "butFamRecall";
			this.butFamRecall.Size = new System.Drawing.Size(103, 24);
			this.butFamRecall.TabIndex = 81;
			this.butFamRecall.TabStop = false;
			this.butFamRecall.Text = "Fam Recall";
			this.butFamRecall.Click += new System.EventHandler(this.butFamRecall_Click);
			// 
			// butMakeRecall
			// 
			this.butMakeRecall.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butMakeRecall.Autosize = true;
			this.butMakeRecall.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butMakeRecall.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butMakeRecall.CornerRadius = 4F;
			this.butMakeRecall.Image = global::OpenDental.Properties.Resources.butRecall;
			this.butMakeRecall.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butMakeRecall.Location = new System.Drawing.Point(5, 31);
			this.butMakeRecall.Name = "butMakeRecall";
			this.butMakeRecall.Size = new System.Drawing.Size(103, 24);
			this.butMakeRecall.TabIndex = 79;
			this.butMakeRecall.TabStop = false;
			this.butMakeRecall.Text = "Make Recall";
			this.butMakeRecall.Click += new System.EventHandler(this.butMakeRecall_Click);
			// 
			// butViewAppts
			// 
			this.butViewAppts.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butViewAppts.Autosize = true;
			this.butViewAppts.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butViewAppts.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butViewAppts.CornerRadius = 4F;
			this.butViewAppts.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butViewAppts.Location = new System.Drawing.Point(5, 83);
			this.butViewAppts.Name = "butViewAppts";
			this.butViewAppts.Size = new System.Drawing.Size(103, 24);
			this.butViewAppts.TabIndex = 80;
			this.butViewAppts.TabStop = false;
			this.butViewAppts.Text = "View Pat Appts";
			this.butViewAppts.Click += new System.EventHandler(this.butViewAppts_Click);
			// 
			// imageListTasks
			// 
			this.imageListTasks.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListTasks.ImageStream")));
			this.imageListTasks.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListTasks.Images.SetKeyName(0, "TaskList.gif");
			this.imageListTasks.Images.SetKeyName(1, "checkBoxChecked.gif");
			this.imageListTasks.Images.SetKeyName(2, "checkBoxUnchecked.gif");
			this.imageListTasks.Images.SetKeyName(3, "TaskListHighlight.gif");
			this.imageListTasks.Images.SetKeyName(4, "checkBoxNew.gif");
			// 
			// ToolBarMain
			// 
			this.ToolBarMain.ImageList = this.imageListMain;
			this.ToolBarMain.Location = new System.Drawing.Point(680, 2);
			this.ToolBarMain.Name = "ToolBarMain";
			this.ToolBarMain.Size = new System.Drawing.Size(203, 25);
			this.ToolBarMain.TabIndex = 73;
			this.ToolBarMain.ButtonClick += new OpenDental.UI.ODToolBarButtonClickEventHandler(this.ToolBarMain_ButtonClick);
			// 
			// menuReminderEdit
			// 
			this.menuReminderEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemReminderDone,
            this.menuItemReminderGoto});
			this.menuReminderEdit.Popup += new System.EventHandler(this.menuReminderEdit_Popup);
			// 
			// menuItemReminderDone
			// 
			this.menuItemReminderDone.Index = 0;
			this.menuItemReminderDone.Text = "Done (affects all users)";
			this.menuItemReminderDone.Click += new System.EventHandler(this.menuItemReminderDone_Click);
			// 
			// menuItemReminderGoto
			// 
			this.menuItemReminderGoto.Index = 1;
			this.menuItemReminderGoto.Text = "Go To";
			this.menuItemReminderGoto.Click += new System.EventHandler(this.menuItemReminderGoto_Click);
			// 
			// ContrAppt
			// 
			this.Controls.Add(this.groupSearch);
			this.Controls.Add(this.panelMakeButtons);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.ToolBarMain);
			this.Controls.Add(this.panelOps);
			this.Controls.Add(this.panelCalendar);
			this.Controls.Add(this.panelAptInfo);
			this.Controls.Add(this.panelSheet);
			this.Name = "ContrAppt";
			this.Size = new System.Drawing.Size(939, 708);
			this.Load += new System.EventHandler(this.ContrAppt_Load);
			this.Layout += new System.Windows.Forms.LayoutEventHandler(this.ContrAppt_Layout);
			this.Resize += new System.EventHandler(this.ContrAppt_Resize);
			this.panelArrows.ResumeLayout(false);
			this.panelSheet.ResumeLayout(false);
			this.panelSheet.PerformLayout();
			this.panelAptInfo.ResumeLayout(false);
			this.panelCalendar.ResumeLayout(false);
			this.panelCalendar.PerformLayout();
			this.groupSearch.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.tabControl.ResumeLayout(false);
			this.tabWaiting.ResumeLayout(false);
			this.tabSched.ResumeLayout(false);
			this.tabProv.ResumeLayout(false);
			this.tabReminders.ResumeLayout(false);
			this.panelMakeButtons.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void ContrApptSheet2_MouseWheel(object sender,System.Windows.Forms.MouseEventArgs e) {
			int max=vScrollBar1.Maximum-vScrollBar1.LargeChange;//panelTable.Height-panelScroll.Height+3;
			int newScrollVal=vScrollBar1.Value-(int)(e.Delta/4);
			if(newScrollVal > max) {
				vScrollBar1.Value=max;
			}
			else if(newScrollVal < vScrollBar1.Minimum) {
				vScrollBar1.Value=vScrollBar1.Minimum;
			}
			else {
				vScrollBar1.Value=newScrollVal;
			}
			ContrApptSheet2.Location=new Point(0,-vScrollBar1.Value);
		}

		///<summary>Overload used when jumping here from another module, and you want to place appointments on the pinboard.</summary>
		public void ModuleSelectedWithPinboard(long patNum,List<long> listPinApptNums,List<long> listOpNums=null,List<long> listProvNums=null) {
			ModuleSelected(patNum,listPinApptNums,listOpNums,listProvNums);
			SendToPinboard(listPinApptNums);
		}

		///<summary>Refreshes the module for the passed in patient.  A patNum of 0 is acceptable.
		///Any ApptNums within listPinApptNums will get forcefully added to the main DataSet for the appointment module.</summary>
		public void ModuleSelected(long patNum,List<long> listPinApptNums=null,List<long> listOpNums=null,List<long> listProvNums=null) {
			if(IsHqNoneView()) {
				return;
			}
			RefreshModuleDataPatient(patNum);
			RefreshModuleDataPeriod(listPinApptNums,listOpNums,listProvNums);
			LayoutScrollOpProv();
			RefreshModuleScreenPatient();
			RefreshModuleScreenPeriod();
			Plugins.HookAddCode(this,"ContrAppt.ModuleSelected_end",patNum);
		}

		///<summary>Refreshes everything except the patient info.  If false, will not refresh the appointment bubble.</summary>
		public void RefreshPeriod(bool isRefreshBubble=true,List<long> listOpNums=null,List<long> listProvNums=null) {	
			if(IsHqNoneView()) {
				return;
			}
			long oldBubbleNum=bubbleAptNum;
			RefreshModuleDataPeriod(listOpNums:listOpNums,listProvNums:listProvNums);
			LayoutScrollOpProv();
			RefreshModuleScreenPeriod();
			if(!isRefreshBubble) {
				bubbleAptNum=oldBubbleNum;
			}
		}

		
		///<summary>Returns true if the none appointment view is selected, clinics is turned on, and the Headquarters clinic is selected.
		///Also disables pretty much every control available in the appointment module if it is going to return true, otherwise re-enables them.</summary>
		private bool IsHqNoneView() {
			if(PrefC.HasClinicsEnabled && Clinics.ClinicNum==0 && comboView.SelectedIndex==0) {
				ContrApptSheet2.Visible=false;
				panelOps.Visible=false;
				vScrollBar1.Visible=false;
				labelNoneView.Visible=true;
				butBack.Enabled=false;
				butBackMonth.Enabled=false;
				butBackWeek.Enabled=false;
				butToday.Enabled=false;
				butFwd.Enabled=false;
				butFwdMonth.Enabled=false;
				butFwdWeek.Enabled=false;
				Calendar2.Enabled=false;
				panelMakeButtons.Enabled=false;
				pinBoard.ClearSelected();
				textLab.Text="";
				textProduction.Text="";
				//TODO Change this to only stop printing and lists
				ToolBarMain.Visible=false;
				return true;
			}
			else {
				ContrApptSheet2.Visible=true;
				panelOps.Visible=true;
				vScrollBar1.Visible=true;
				labelNoneView.Visible=false;
				butBack.Enabled=true;
				butBackMonth.Enabled=true;
				butBackWeek.Enabled=true;
				butToday.Enabled=true;
				butFwd.Enabled=true;
				butFwdMonth.Enabled=true;
				butFwdWeek.Enabled=true;
				Calendar2.Enabled=true;
				ToolBarMain.Visible=true;
				return false;
			}
		}

		///<summary>Fills PatCur from the database unless the patnum has not changed.</summary>
		public void RefreshModuleDataPatient(long patNum) {
			if(patNum==0) {
				PatCur=null;
				return;
			}
			//if(PatCur !=null && PatCur.PatNum==patNum) {//if patient has not changed
			//  return;//don't do anything
			//}
			//We have to go to the db because we need to get the most recent patient info. Mainly used for the AskedToArriveEarly time.
			PatCur=Patients.GetPat(patNum);
			Plugins.HookAddCode(this, "ContrAppt.RefreshModuleDataPatient_end");
		}

		///<summary>Gets the entire DS for appointments and schedules.  Gets op and prov indices for current view.</summary>
		private void RefreshModuleDataPeriod(List<long> listPinApptNums = null,List<long> listOpNums = null,List<long> listProvNums = null) {
			_aptBubbleDefs=DisplayFields.GetForCategory(DisplayFieldCategory.AppointmentBubble);
			bubbleAptNum=0;
			DateTime startDate;
			DateTime endDate;
			if(ApptDrawing.IsWeeklyView) {
				startDate=WeekStartDate;
				endDate=WeekEndDate;
			}
			else {
				startDate=AppointmentL.DateSelected;
				endDate=AppointmentL.DateSelected;
			}
			if(startDate.Year<1880 || endDate.Year<1880) {
				return;
			}
			//Calendar2.SetSelectionRange(startDate,endDate);
			if(PatCur==null) {
				//there cannot be a selected appointment if no patient is loaded.
				ContrApptSingle.SelectedAptNum=-1;//fixes a minor bug.
			}
			//Regardless of clinics, none view gets all providers and operatories (or close to) and we should not filter based on ops and provs.
			if(comboView.SelectedIndex<1) {
				listOpNums=null;
				listProvNums=null;
			}
			else {
				long apptViewNum = 0;
				//First time loading the Appt Module for non-clinic users
				if(ApptViewItemL.ApptViewCur==null) {
					//GetApptViewForUser needs a CurUser to the specific appointment views for the clinic/user combination
					if(Security.CurUser!=null) {
						//Can return null here, which will cause apptViewNum to remain 0.
						ApptView apptViewCur = GetApptViewForUser();
						if(apptViewCur!=null) {
							apptViewNum=apptViewCur.ApptViewNum;
						}
					}
					else {
						//No valid user so the appointment view will be set to whatever the computerpref table has for the current computer
						apptViewNum=ComputerPrefs.LocalComputer.ApptViewNum;
					}
				}
				else {
					//Has been filled before, so it is safe to call
					apptViewNum=ApptViewItemL.ApptViewCur.ApptViewNum;
				}
				if(listOpNums==null) {
					listOpNums=ApptViewItems.GetOpsForView(apptViewNum);
				}
				if(listProvNums==null) {
					listProvNums=ApptViewItems.GetProvsForView(apptViewNum);
				}
			}
			DS=Appointments.RefreshPeriod(startDate,endDate,Clinics.ClinicNum,listPinApptNums,listOpNums,listProvNums);
			LastTimeDataRetrieved=DateTime.Now;
			SchedListPeriod=Schedules.ConvertTableToList(DS.Tables["Schedule"]);
			ApptView viewCur=null;
			if(comboView.SelectedIndex>0) {
				viewCur=_listApptViews[comboView.SelectedIndex-1];
			}
			ApptViewItemL.GetForCurView(viewCur,ApptDrawing.IsWeeklyView,SchedListPeriod);
		}

		/// <summary>Called from both ModuleSelected and from RefreshPeriod.  Do not call it from any event like Layout.  This also clears listConfirmed.</summary>
		public void LayoutScrollOpProv() {//ModuleSelectedOld(int patNum){
			//the scrollbar logic cannot be moved to someplace where it will be activated while working in apptbook
			//RefreshVisops();//forces reset after changing databases
			int oldHeight=ContrApptSheet2.Height;
			int oldVScrollVal=vScrollBar1.Value;
			if(DefC.Short!=null) {
				ApptView viewCur=null;
				if(comboView.SelectedIndex>0) {
					viewCur=_listApptViews[comboView.SelectedIndex-1];
				}
				ApptViewItemL.GetForCurView(viewCur,ApptDrawing.IsWeeklyView,SchedListPeriod);//refreshes visops,etc
				ApptDrawing.ApptSheetWidth=panelSheet.Width-vScrollBar1.Width;
				ApptDrawing.ComputeColWidth(0);
				ContrApptSheet2.Height=ApptDrawing.LineH*24*ApptDrawing.RowsPerHr;
			}
			this.SuspendLayout();
			vScrollBar1.Enabled=true;
			vScrollBar1.Minimum=0;
			vScrollBar1.LargeChange=12*ApptDrawing.LineH;//12 rows
			vScrollBar1.Maximum=ContrApptSheet2.Height-panelSheet.Height+vScrollBar1.LargeChange;
			if(vScrollBar1.Maximum<0) {
				vScrollBar1.Maximum=0;
			}
			//Max is set again in Resize event
			vScrollBar1.SmallChange=6*ApptDrawing.LineH;//6 rows
			if(!_hasLayedOutScrollBar && vScrollBar1.Value==0) {//Should only run on startup
				int rowsPerHr=60/ApptDrawing.MinPerIncr*ApptDrawing.RowsPerIncr;
				//use the row setting from the selected view.
				if(_listApptViews.Count>0 && comboView.SelectedIndex>0) {					
					TimeSpan apptTimeScrollStart=_listApptViews[comboView.SelectedIndex-1].ApptTimeScrollStart;
					if(_listApptViews[comboView.SelectedIndex-1].IsScrollStartDynamic) {//Scroll start time at the earliest scheduled operatory or appointment
						//Get the schedules that have any operatory visible
						List<Schedule> listVisScheds=new List<Schedule>();
						foreach(Schedule sched in SchedListPeriod) {
							if(sched.Ops.Any(x => ApptDrawing.VisOps.Exists(y => x==y.OperatoryNum))//The schedule is linked to a visible operatory
								|| ApptDrawing.VisOps.Exists(x => x.ProvDentist==sched.ProvNum && !x.IsHygiene)//The dentist is in a visible operatory
								|| ApptDrawing.VisOps.Exists(x => x.ProvHygienist==sched.ProvNum && x.IsHygiene))//The hygienist is in a visible operatory
							{
								listVisScheds.Add(sched);
							}
						}
						long schedProvUnassinged=PrefC.GetLong(PrefName.ScheduleProvUnassigned);
						bool opShowsDefaultProv=false;
						foreach(Operatory op in ApptDrawing.VisOps) {
							if((op.ProvDentist!=0 && !op.IsHygiene)
								||(op.ProvHygienist!=0 && op.IsHygiene))
							{
								continue;//The operatory has a provider assigned to it
							}
							if(SchedListPeriod.Any(x => x.Ops.Contains(op.OperatoryNum))) {
								continue;//The operatory has a scheduled assigned to it
							}
							opShowsDefaultProv=true;//The operatory will have the provider for unassigned operatories
							break;
						}
						if(opShowsDefaultProv && SchedListPeriod.Exists(x => x.ProvNum==schedProvUnassinged)) {//The provider for unassigned ops has a schedule
							//Add that provider's earliest schedule
							listVisScheds.Add(SchedListPeriod.FindAll(x => x.ProvNum==schedProvUnassinged).OrderBy(x => x.StartTime).FirstOrDefault());
						}
						//Get the appointment times that are in a visible operatory
						List<TimeSpan> listVisAptTimes=new List<TimeSpan>();
						foreach(DataRow row in DS.Tables["Appointments"].Rows) {
							long opNum=PIn.Long(row["Op"].ToString());
							if(!ApptDrawing.VisOps.Exists(x => x.OperatoryNum==opNum) //The appointment is in a visible operatory
								|| !new[] { "1","2","4","5","7","8" }.Contains(row["AptStatus"].ToString())) //Scheduled,Complete,ASAP,Broken,PtNote,PtNoteComp
							{
								continue;
							}
							listVisAptTimes.Add(PIn.Date(row["AptDateTime"].ToString()).TimeOfDay);
						}
						TimeSpan earliestApt=new TimeSpan();
						TimeSpan earliestOp=new TimeSpan();
						if(listVisAptTimes.Count>0 && listVisScheds.Count>0) {//There is at least one schedule and at least one appointment visible
							earliestApt=listVisScheds.Min(x => x.StartTime);
							earliestOp=listVisAptTimes.Min();
							if(TimeSpan.Compare(earliestOp,earliestApt)==1) {//earliestOp is later than earliestApt
								apptTimeScrollStart=earliestApt;
							}
							else {//earliestApt is later than earliestOp or they are both equal
								apptTimeScrollStart=earliestOp;
							}
						}
						else if(listVisScheds.Count>0) {//There is at least one visible schedule and no visible appointments
							apptTimeScrollStart=listVisScheds.Min(x => x.StartTime);
						}
						else if(listVisAptTimes.Count>0) {//There is at least one visible appointment and no visible schedules
							apptTimeScrollStart=listVisAptTimes.Min();
						}
						//else apptTimeScrollStart will remain as the start time listed in the appt view		
					}
					rowsPerHr=60/ApptDrawing.MinPerIncr*_listApptViews[comboView.SelectedIndex-1].RowsPerIncr;//comboView.SelectedIndex-1 because combo box contains none but list does not.
					double apptTimeHrs=((apptTimeScrollStart.Hours*60)+apptTimeScrollStart.Minutes)/60.0;
					if(apptTimeHrs*rowsPerHr*ApptDrawing.LineH<vScrollBar1.Maximum-vScrollBar1.LargeChange) {
						vScrollBar1.Value=(int)(apptTimeHrs*rowsPerHr*ApptDrawing.LineH);
					}
					else {
						vScrollBar1.Value=vScrollBar1.Maximum;
					}
				}
				else if(8*rowsPerHr*ApptDrawing.LineH<vScrollBar1.Maximum-vScrollBar1.LargeChange) {
					vScrollBar1.Value=8*rowsPerHr*ApptDrawing.LineH;//8am
				}
			}
			else if(ContrApptSheet2.Height!=oldHeight && oldHeight > 0) {//Try not to move the scroll bar around when changing between views that have different row increment values.
				//the max prevents setting scroll value to a negative, the min prevents setting scroll value beyond maximum scroll allowed
				vScrollBar1.Value=Math.Min(oldVScrollVal*ContrApptSheet2.Height/oldHeight,Math.Max(vScrollBar1.Maximum-vScrollBar1.LargeChange,0));
			}
			_hasLayedOutScrollBar=true;
			if(vScrollBar1.Value>vScrollBar1.Maximum-vScrollBar1.LargeChange) {
				if(vScrollBar1.Maximum-vScrollBar1.LargeChange>=0) {//but don't allow setting negative number
					vScrollBar1.Value=vScrollBar1.Maximum-vScrollBar1.LargeChange;
				}
			}
			ContrApptSheet2.Location=new Point(0,-vScrollBar1.Value);
			toolTip1.RemoveAll();//without this line, the program becomes sluggish.
			for(int i=panelOps.Controls.Count-1;i>=0;i--) {
				if(panelOps.Controls[i]!=null) {
					panelOps.Controls[i].Dispose();
				}
			}
			panelOps.Controls.Clear();
			Operatory curOp;
			//If the the Operatory name does not fit on one line, expand panelOps so that two lines will fit
			panelOps.Height=17;//Enough to fit one line of text	
			if(!ApptDrawing.IsWeeklyView) {			
				for(int i=0;i<ApptDrawing.ColCount;i++) {
					curOp=ApptDrawing.VisOps[i];
					Size textSize=TextRenderer.MeasureText(curOp.OpName,new Font("Microsoft Sans Serif",8.25f,FontStyle.Regular));
					if(textSize.Width>ApptDrawing.ColWidth) {
						panelOps.Height=30;//Enough to fit two lines of text
						break;
					}
				}
			}
			panelSheet.Location=new Point(panelSheet.Location.X,panelOps.Height);
			vScrollBar1.Location=new Point(vScrollBar1.Location.X,panelOps.Height);
			for(int i=0;i<ApptDrawing.ProvCount;i++) {
				Panel panProv=new Panel();
				panProv.BackColor=ApptDrawing.VisProvs[i].ProvColor;
				panProv.Location=new Point(2+(int)ApptDrawing.TimeWidth+(int)ApptDrawing.ProvWidth*i,0);
				panProv.Width=(int)ApptDrawing.ProvWidth;
				if(i==0) {//just looks a little nicer:
					panProv.Location=new Point(panProv.Location.X-1,panProv.Location.Y);
					panProv.Width=panProv.Width+1;
				}
				panProv.Height=panelOps.Height;
				panProv.BorderStyle=BorderStyle.Fixed3D;
				panProv.ForeColor=Color.DarkGray;
				panelOps.Controls.Add(panProv);
				toolTip1.SetToolTip(panProv,ApptDrawing.VisProvs[i].Abbr);
			}
			if(ApptDrawing.IsWeeklyView) {
				for(int i=0;i<ApptDrawing.NumOfWeekDaysToDisplay;i++) {
					Panel panOpName=new Panel();
					Label labOpName=new Label();
					labOpName.Text=WeekStartDate.AddDays(i).ToString("dddd-d");
					panOpName.Location=new Point
						(2+(int)ApptDrawing.TimeWidth+i*(int)ApptDrawing.ColDayWidth,0);
					panOpName.Width=(int)ApptDrawing.ColDayWidth;
					panOpName.Height=18;
					panOpName.BorderStyle=BorderStyle.Fixed3D;
					panOpName.ForeColor=Color.DarkGray;
					panOpName.MouseDown += new System.Windows.Forms.MouseEventHandler(panOpName_MouseDown);
					panOpName.Tag=i;//stores the day index
					//add label within panOpName
					labOpName.Location=new Point(0,-2);
					labOpName.Width=panOpName.Width;
					labOpName.Height=18;
					labOpName.TextAlign=ContentAlignment.MiddleCenter;
					labOpName.ForeColor=Color.Black;
					labOpName.MouseDown += new System.Windows.Forms.MouseEventHandler(panOpName_MouseDown);
					labOpName.Tag=i;//stores the day index
					panOpName.Controls.Add(labOpName);
					panelOps.Controls.Add(panOpName);
				}
			}
			else {
				for(int i=0;i<ApptDrawing.ColCount;i++) {
					Panel panOpName=new Panel();
					Label labOpName=new Label();
					curOp=ApptDrawing.VisOps[i];
					labOpName.Text=curOp.OpName;
					if(curOp.ProvDentist!=0 && !curOp.IsHygiene) {
						panOpName.BackColor=Providers.GetColor(curOp.ProvDentist);
					}
					else if(curOp.ProvHygienist!=0 && curOp.IsHygiene) {
						panOpName.BackColor=Providers.GetColor(curOp.ProvHygienist);
					}
					float xPos=ApptDrawing.TimeWidth+ApptDrawing.ProvWidth*ApptDrawing.ProvCount+i*ApptDrawing.ColWidth;
					panOpName.Location=new Point(2+(int)xPos,0);
					panOpName.Width=(int)ApptDrawing.ColWidth;
					panOpName.Height=panelOps.Height;
					panOpName.BorderStyle=BorderStyle.Fixed3D;
					panOpName.ForeColor=Color.DarkGray;
					//add label within panOpName
					labOpName.Location=new Point(0,-2);
					labOpName.Width=panOpName.Width;
					labOpName.Height=panelOps.Height;
					labOpName.TextAlign=ContentAlignment.MiddleCenter;
					labOpName.ForeColor=Color.Black;
					panOpName.Controls.Add(labOpName);
					panelOps.Controls.Add(panOpName);
				}
			}
			this.ResumeLayout();
			listConfirmed.Items.Clear();
			for(int i=0;i<DefC.Short[(int)DefCat.ApptConfirmed].Length;i++) {
				this.listConfirmed.Items.Add(DefC.Short[(int)DefCat.ApptConfirmed][i].ItemValue);
			}
		}

		///<summary>Refreshes the appointment info panel to the right if an appointment is currently selected.</summary>
		public void RefreshModuleScreenPatient() {
			if(PatCur==null) {
				panelMakeButtons.Enabled=false;
				/*butMakeAppt.Enabled=false;
				butMakeRecall.Enabled=false;
				butFamRecall.Enabled=false;
				butViewAppts.Enabled=false;*/
			}
			else {
				panelMakeButtons.Enabled=true;
			}
			if(ContrApptSingle.SelectedAptNum>0) {
				butUnsched.Enabled=true;
				butBreak.Enabled=true;
				butComplete.Enabled=true;
				butDelete.Enabled=true;
				if(!Security.IsAuthorized(Permissions.ApptConfirmStatusEdit,true)) {//Suppress message because it would be very annoying to users.
					listConfirmed.Enabled=false;
				}
				else {
					listConfirmed.Enabled=true;
				}
			}
			else {
				butUnsched.Enabled=false;
				butBreak.Enabled=false;
				butComplete.Enabled=false;
				butDelete.Enabled=false;
				listConfirmed.Enabled=false;
			}
			if(panelAptInfo.Enabled && DS!=null) {
				long aptconfirmed=0;
				string tableAptNum;
				if(pinBoard.SelectedIndex==-1) {//no pinboard appt selected
					for(int i=0;i<DS.Tables["Appointments"].Rows.Count;i++) {
						tableAptNum=DS.Tables["Appointments"].Rows[i]["AptNum"].ToString();
						if(tableAptNum==ContrApptSingle.SelectedAptNum.ToString()) {
							aptconfirmed=PIn.Long(DS.Tables["Appointments"].Rows[i]["Confirmed"].ToString());
							break;
						}
					}
				}
				else {//pinboard appt selected
					aptconfirmed=PIn.Long(pinBoard.SelectedAppt.DataRoww["Confirmed"].ToString());
				}
				listConfirmed.SelectedIndex=DefC.GetOrder(DefCat.ApptConfirmed,aptconfirmed);//could be -1
			}
			else {
				listConfirmed.SelectedIndex=-1;
			}
		}

		///<summary>Redraws screen based on data already gathered.  RefreshModuleDataPeriod will have already retrieved the data from the db.</summary>
		public void RefreshModuleScreenPeriod() {
			DateTime startDate;
			DateTime endDate;
			if(ApptDrawing.IsWeeklyView) {
				startDate=WeekStartDate;
				endDate=WeekEndDate;
			}
			else {
				startDate=AppointmentL.DateSelected;
				endDate=AppointmentL.DateSelected;
			}
			if(startDate.Year<1880 || endDate.Year<1880) {
				return;
			}
			Calendar2.SetSelectionRange(startDate,endDate);
			ApptDrawing.ProvBar=new int[ApptDrawing.VisProvs.Count][];
			for(int i=0;i<ApptDrawing.VisProvs.Count;i++) {
				ApptDrawing.ProvBar[i]=new int[24*ApptDrawing.RowsPerHr]; //[144]; or 24*6
			}
			if(ContrApptSingle3!=null) {//I think this is not needed.
				for(int i=0;i<ContrApptSingle3.Length;i++) {
					if(ContrApptSingle3[i]!=null) {
						ContrApptSingle3[i].Dispose();
					}
					ContrApptSingle3[i]=null;
				}
				ContrApptSingle3=null;
			}
			labelDate.Text=startDate.ToString("ddd");
			labelDate2.Text=startDate.ToString("-  MMM d");
			ContrApptSheet2.Controls.Clear();
			ContrApptSingle3=new ContrApptSingle[DS.Tables["Appointments"].Rows.Count];
			List<long> listAptNums=new List<long> ();
			DataRow row;
			for(int i=0;i<DS.Tables["Appointments"].Rows.Count;i++) {
				row=DS.Tables["Appointments"].Rows[i];
				if(PIn.Date(row["AptDateTime"].ToString()).Date<startDate.Date || PIn.Date(row["AptDateTime"].ToString()).Date>endDate.Date){
					continue;//Appointment is outside of our date range.
				}
				listAptNums.Add(PIn.Long(row["AptNum"].ToString()));
				ContrApptSingle3[i]=new ContrApptSingle();
				ContrApptSingle3[i].Visible=false;
				if(ContrApptSingle.SelectedAptNum.ToString()==row["AptNum"].ToString()) {//if this is the selected apt
					//if the selected patient was changed from another module, then deselect the apt.
					if(PatCur==null || PatCur.PatNum.ToString()!=row["PatNum"].ToString()) {
						ContrApptSingle.SelectedAptNum=-1;
					}
				}
				ContrApptSingle3[i].DataRoww=row;
				ContrApptSingle3[i].TableApptFields=DS.Tables["ApptFields"];
				ContrApptSingle3[i].TablePatFields=DS.Tables["PatFields"];
				ContrApptSingle3[i].PatternShowing=ApptSingleDrawing.GetPatternShowing(row["Pattern"].ToString());
				if(!ApptDrawing.IsWeeklyView) {
					ApptDrawing.ProvBarShading(row);
				}
				ContrApptSingle3[i].Location=ApptSingleDrawing.SetLocation(row,0,ApptDrawing.VisOps.Count,0);
				ContrApptSingle3[i].Size=ApptSingleDrawing.SetSize(row);
				ContrApptSheet2.Controls.Add(ContrApptSingle3[i]);
			}//end for
			//PinApptSingle.Refresh();
			pinBoard.Invalidate();
			ApptDrawing.SchedListPeriod=SchedListPeriod;
			ContrApptSheet2.CreateShadow();
			CreateAptShadowsOnMain(listAptNums);
			ContrApptSheet2.DrawShadow();
			List<long> opNums = null;
			if(PrefC.HasClinicsEnabled && Clinics.ClinicNum>0) {
				opNums = Operatories.GetOpsForClinic(Clinics.ClinicNum).Select(x => x.OperatoryNum).ToList();
			}
			List<LabCase> labCaseList=LabCases.GetForPeriod(startDate,endDate,opNums);
			FillLab(labCaseList);
			FillProduction();
			FillProvSched();
			FillEmpSched();
			FillWaitingRoom();
			LayoutPanels();
		}

		///<summary>This is public so that FormOpenDental can pass refreshed tasks here in order to avoid an extra query.</summary>
		public void RefreshReminders(List <Task> listReminderTasks) {
			List<Task> listSortedReminderTasks=listReminderTasks
				.Where(x => x.DateTimeEntry.Date <= DateTimeOD.Today)
				.OrderBy(x => x.DateTimeEntry)
				.ToList();
			tabReminders.Text=Lan.g(this,"Reminders");
			if(listSortedReminderTasks.Count > 0) {
				tabReminders.Text+="*";
			}
			gridReminders.BeginUpdate();
			if(gridReminders.Columns.Count==0) {
				gridReminders.Columns.Clear();
				ODGridColumn col=new ODGridColumn("",17);//The status column showing new/viewed in a checkbox.
				col.ImageList=imageListTasks;
				gridReminders.Columns.Add(col);
				col=new ODGridColumn(Lan.g("TableTasks","Description"),200);//any width
				gridReminders.Columns.Add(col);
			}
			gridReminders.Rows.Clear();
			for(int i=0;i<listSortedReminderTasks.Count;i++) {
				ODGridRow row=new ODGridRow();
				SetReminderGridRow(row,listSortedReminderTasks[i]);
				gridReminders.Rows.Add(row);
			}
			gridReminders.EndUpdate();
		}

		///<summary>This logic mimics filling a row within UserControlTasks.FillGrid().
		///However, the logic is simpler here because we are only dealing with reminders.</summary>
		private void SetReminderGridRow(ODGridRow row,Task reminderTask) {
			row.Tag=reminderTask;
			row.Cells.Clear();
			string dateStr="";
			if(reminderTask.DateTask.Year>1880) {
				if(reminderTask.DateType==TaskDateType.Day) {
					dateStr+=reminderTask.DateTask.ToShortDateString()+" - ";
				}
				else if(reminderTask.DateType==TaskDateType.Week) {
					dateStr+=Lan.g(this,"Week of")+" "+reminderTask.DateTask.ToShortDateString()+" - ";
				}
				else if(reminderTask.DateType==TaskDateType.Month) {
					dateStr+=reminderTask.DateTask.ToString("MMMM")+" - ";
				}
			}
			else if(reminderTask.DateTimeEntry.Year>1880) {
				dateStr+=reminderTask.DateTimeEntry.ToShortDateString()+" "+reminderTask.DateTimeEntry.ToShortTimeString()+" - ";
			}
			string objDesc="";
			if(reminderTask.TaskStatus==TaskStatusEnum.Done){
				objDesc=Lan.g(this,"Done:")+reminderTask.DateTimeFinished.ToShortDateString()+" - ";
			}
			if(reminderTask.ObjectType==TaskObjectType.Patient) {
				if(reminderTask.KeyNum!=0) {
					objDesc+=Patients.GetPat(reminderTask.KeyNum).GetNameLF()+" - ";
				}
			}
			else if(reminderTask.ObjectType==TaskObjectType.Appointment) {
				if(reminderTask.KeyNum!=0) {
					Appointment AptCur=Appointments.GetOneApt(reminderTask.KeyNum);
					if(AptCur!=null) {
						objDesc=Patients.GetPat(AptCur.PatNum).GetNameLF()//this is going to stay. Still not optimized, but here at HQ, we don't use it.
							+"  "+AptCur.AptDateTime.ToString()
							+"  "+AptCur.ProcDescript
							+"  "+AptCur.Note
							+" - ";
					}
				}
			}
			if(!reminderTask.Descript.StartsWith("==") && reminderTask.UserNum!=0) {
				objDesc+=Userods.GetName(reminderTask.UserNum)+" - ";
			}
			if(PrefC.GetBool(PrefName.TasksNewTrackedByUser)) {//The new way
				if(reminderTask.TaskStatus==TaskStatusEnum.Done) {
					row.Cells.Add("1");
				}
				else {
					if(reminderTask.IsUnread) {
						row.Cells.Add("4");
					}
					else{
						row.Cells.Add("2");
					}
				}
			}
			else {
				switch(reminderTask.TaskStatus) {
					case TaskStatusEnum.New:
						row.Cells.Add("4");
						break;
					case TaskStatusEnum.Viewed:
						row.Cells.Add("2");
						break;
					case TaskStatusEnum.Done:
						row.Cells.Add("1");
						break;
				}
			}
			row.Cells.Add(dateStr+objDesc+reminderTask.Descript);
			//No need to do any text detection for triage priorities, we'll just use the task priority colors.
			row.ColorBackG=DefC.GetColor(DefCat.TaskPriorities,reminderTask.PriorityDefNum);
		}

		///<summary>The logic for this function was copied from UserControlTasks.gridMain_MouseDown() and modified slightly for this scenaro.</summary>
		private void gridReminders_MouseDown(object sender,MouseEventArgs e) {
			int clickedI=gridReminders.PointToRow(e.Y);
			int clickedCol=gridReminders.PointToCol(e.X);
			if(clickedI==-1){
				return;
			}
			gridReminders.SetSelected(clickedI,true);//if right click.
			if(e.Button!=MouseButtons.Left) {
				return;
			}
			ODGridRow row=gridReminders.Rows[clickedI];
			Task reminderTask=((Task)row.Tag).Copy();
			if(clickedCol==0){//check tasks off
				if(PrefC.GetBool(PrefName.TasksNewTrackedByUser)) {
					long userNumInbox=TaskLists.GetMailboxUserNum(reminderTask.TaskListNum);
					if(userNumInbox != 0 && userNumInbox != Security.CurUser.UserNum) {
						MsgBox.Show(this,"Not allowed to mark off tasks in someone else's inbox.");
						return;
					}
					//might not need to go to db to get this info 
					//might be able to check this:
					//if(task.IsUnread) {
					//But seems safer to go to db.
					if(TaskUnreads.IsUnread(Security.CurUser.UserNum,reminderTask.TaskNum)) {
						TaskUnreads.SetRead(Security.CurUser.UserNum,reminderTask.TaskNum);
						reminderTask.TaskStatus=TaskStatusEnum.Viewed;
						gridReminders.BeginUpdate();
						SetReminderGridRow(row,reminderTask);//To get the status to immediately show up in the reminders grid.
						gridReminders.EndUpdate();
						DataValid.SetInvalidTask(reminderTask.TaskNum,false);
					}
					//if already read, nothing else to do.  If done, nothing to do
				}
				else {
					if(reminderTask.TaskStatus==TaskStatusEnum.New) {
						Task taskOld=reminderTask.Copy();
						reminderTask.TaskStatus=TaskStatusEnum.Viewed;
						try {
							Tasks.Update(reminderTask,taskOld);
							gridReminders.BeginUpdate();
							SetReminderGridRow(row,reminderTask);//To get the status to immediately show up in the reminders grid.
							gridReminders.EndUpdate();
							DataValid.SetInvalidTask(reminderTask.TaskNum,false);
						}
						catch(Exception ex) {
							MessageBox.Show(ex.Message);
							return;
						}
					}
					//no longer allowed to mark done from here
				}
			}
		}

		///<summary>Logic mimics UserControlTasks.gridMain_CellDoubleClick()</summary>
		private void gridReminders_CellDoubleClick(object sender,ODGridClickEventArgs e) {
			if(e.Col==0){
				//no longer allow double click on checkbox, because it's annoying.
				return;
			}
			ODGridRow row=gridReminders.Rows[e.Row];
			Task reminderTask=((Task)row.Tag);
			//It's important to grab the task directly from the db because the status in this list is fake, being the "unread" status instead.
			Task task=Tasks.GetOne(reminderTask.TaskNum);
			FormTaskEdit FormT=new FormTaskEdit(task,task.Copy());
			FormT.Show();//non-modal
		}
		
		///<summary>Logic mimics UserControlTasks.SetMenusEnabled()</summary>
		private void menuReminderEdit_Popup(object sender,EventArgs e) {
			if(gridReminders.GetSelectedIndex()==-1) {
				return;
			}
			Task task=(Task)gridReminders.Rows[gridReminders.GetSelectedIndex()].Tag;
			menuItemReminderGoto.Enabled=true;
			if(task.ObjectType==TaskObjectType.None) {
				menuItemReminderGoto.Enabled=false;
			}
		}

		///<summary>Logic mimics UserControlTasks.DoneClicked()</summary>
		private void menuItemReminderDone_Click(object sender,EventArgs e) {
			if(gridReminders.GetSelectedIndex()==-1) {
				return;
			}
			Task task=(Task)gridReminders.Rows[gridReminders.GetSelectedIndex()].Tag;
			Task oldTask=task.Copy();
			task.TaskStatus=TaskStatusEnum.Done;
			if(task.DateTimeFinished.Year<1880) {
				task.DateTimeFinished=DateTime.Now;
			}
			try {
				Tasks.Update(task,oldTask);
			}
			catch(Exception ex) {
				//Revert the changes to the task because something went wrong.
				task.TaskStatus=oldTask.TaskStatus;
				task.DateTimeFinished=oldTask.DateTimeFinished;
				MessageBox.Show(ex.Message);
				return;
			}
			TaskUnreads.DeleteForTask(task.TaskNum);
			TaskHist taskHist=new TaskHist(oldTask);
			taskHist.UserNumHist=Security.CurUser.UserNum;
			TaskHists.Insert(taskHist);
			DataValid.SetInvalidTask(task.TaskNum,false);
			gridReminders.BeginUpdate();
			gridReminders.Rows.RemoveAt(gridReminders.GetSelectedIndex());
			gridReminders.EndUpdate();
		}

		///<summary>Logic mimics UserControlTasks.GoTo_Clicked()</summary>
		private void menuItemReminderGoto_Click(object sender,EventArgs e) {
			if(gridReminders.GetSelectedIndex()==-1) {
				return;
			}
			Task task=(Task)gridReminders.Rows[gridReminders.GetSelectedIndex()].Tag;
			FormOpenDental.S_TaskGoTo(task.ObjectType,task.KeyNum);
		}

		///<summary>Only used when viewing weekly.</summary>
		private void panOpName_MouseDown(object sender,System.Windows.Forms.MouseEventArgs e) {
			if(e.Button!=MouseButtons.Left) {
				return;
			}
			int dayI=0;
			if(sender.GetType()==typeof(Panel)) {
				dayI=(int)((Panel)sender).Tag;
			}
			else {
				dayI=(int)((Label)sender).Tag;
			}
			AppointmentL.DateSelected=WeekStartDate.AddDays(dayI);
			SetWeeklyView(false);
		}

		///<summary>Sets the ContrApptSingle array to null.</summary>
		public void ModuleUnselected() {
			ContrApptSheet2.Shadow=null;
			if(ContrApptSingle3!=null) {//too complex?
				for(int i=0;i<ContrApptSingle3.Length;i++) {
					if(ContrApptSingle3[i]!=null) {
						ContrApptSingle3[i].Dispose();
						ContrApptSingle3[i]=null;
					}
				}
				ContrApptSingle3=null;
			}
			Plugins.HookAddCode(this,"ContrAppt.ModuleUnselected_end");
		}

		/*
		///<summary>Was RefreshModuleData and FillPatientButton.  Gets the data for the specified patient. Does not refresh any appointment data.  This function should always be called when the patient changes since that's all this function is responsible for.</summary>
		private void RefreshModulePatient(int patNum){//
			if(PatCur.PatNum==patNum) {//if patient has not changed
				return;//don't do anything
			}
			PatCurNum=patNum;//might be zero
			bool hasEmail;
			string chartNumber;
			if(PatCurNum==0){
				PatCurName="";
				PatCurChartNumber="";
				butOther.Enabled=false;
				hasEmail=false;
				chartNumber="";
			}
			else{
				Patient pat=Patients.GetPat(PatCurNum);
				PatCurName=pat.GetNameLF();
				PatCurChartNumber=pat.ChartNumber;
				hasEmail=pat.Email!="";
				chartNumber=pat.ChartNumber;
				//family can wait until user clicks on downarrow.
				PatientL.AddPatsToMenu(menuPatient,new EventHandler(menuPatient_Click),PatCurName,PatCurNum);
				butOther.Enabled=true;
			}
			butUnsched.Enabled=butOther.Enabled;
			butBreak.Enabled=butOther.Enabled;
			butComplete.Enabled=butOther.Enabled;
			butDelete.Enabled=butOther.Enabled;
			//ParentForm.Text=Patients.GetMainTitle(PatCurName,PatCurNum,PatCurChartNumber);
			if(panelAptInfo.Enabled && DS!=null) {
				int aptconfirmed=0;
				for(int i=0;i<DS.Tables["Appointments"].Rows.Count;i++) {
					if(DS.Tables["Appointments"].Rows[i]["AptNum"].ToString()==ContrApptSingle.ClickedAptNum.ToString()) {
						aptconfirmed=PIn.PInt(DS.Tables["Appointments"].Rows[i]["Confirmed"].ToString());
						break;
					}
				}
				listConfirmed.SelectedIndex=DefC.GetOrder(DefCat.ApptConfirmed,aptconfirmed);//could be -1
			}
			else {
				listConfirmed.SelectedIndex=-1;
			}
//JSPARKS - THIS SHOULD BE MOVED TO BE CALLED EXPLICITLY FROM WITHIN VARIOUS PLACES IN THIS MODULE
//JUST LIKE IN THE OTHER MODULES.  WHEN IT'S HERE, IT IS ALSO GETTING CALLED EVERY TIME MODULESELECTED
//GETS TRIGGERED FROM PARENT FORM.
			OnPatientSelected(PatCurNum,PatCurName,hasEmail,chartNumber);
		}*/

		/*This was resulting in too many firings of ModuleSelected
		///<summary>Currently only used when comboView really does change.  Otherwise, just call ModuleSelected.  Triggered in FunctionKeyPress, SetView, and  FillViews</summary>
		private void comboView_SelectedIndexChanged(object sender,System.EventArgs e) {
			if(PatCur==null) {
				ModuleSelected(0);
			}
			else {
				ModuleSelected(PatCur.PatNum);
			}
		}*/

		///<summary></summary>
		private void comboView_SelectionChangeCommitted(object sender,EventArgs e) {
			if(comboView.SelectedIndex==0) {
				SetView(0,true);
			}
			else {
				SetView(_listApptViews[comboView.SelectedIndex-1].ApptViewNum,true);
			}
		}

		///<summary>Not used</summary>
		private void ContrAppt_Layout(object sender,System.Windows.Forms.LayoutEventArgs e) {
			//This event actually happens quite frequently: once for every appointment placed on the screen.
			//Moved it all to Resize event.
		}

		///<summary>Might not be getting called enough.</summary>
		private void ContrAppt_Resize(object sender,System.EventArgs e) {
			//This didn't work so well.  Very slow and caused program to not be able to unminimize
			try {//so it doesn't crash if not connected to DB
				//ModuleSelected();
			}
			catch { }
			//even though the part above didn't work, we're going to try adding the stuff below.  It was important to get it out
			//of the Layout event, which fires very frequently.
			LayoutPanels();
		}

		///<summary>This might not be getting called frequently enough.  Done on resize and when refreshing the period.  But it used to be done very very frequently.</summary>
		private void LayoutPanels() {
			if(Calendar2.Width>panelCalendar.Width) {//for example, Chinese calendar
				panelCalendar.Width=Calendar2.Width+1;
				//panelAptInfo.Width=Calendar2.Width+1;
			}
			//Assumes widths of the first 2 panels were set the same in the designer,
			ToolBarMain.Location=new Point(ClientSize.Width-panelCalendar.Width-2,0);
			panelCalendar.Location=new Point(ClientSize.Width-panelCalendar.Width-2,ToolBarMain.Height);
			panelAptInfo.Location=new Point(ClientSize.Width-panelCalendar.Width-2,ToolBarMain.Height+panelCalendar.Height);
			//butOther.Location=new Point(panelAptInfo.Location.X+32,panelAptInfo.Location.Y+84);
			panelMakeButtons.Location=new Point(panelAptInfo.Right+2,panelAptInfo.Top);
			panelSheet.Width=ClientSize.Width-panelCalendar.Width-2;
			panelSheet.Height=ClientSize.Height-panelSheet.Location.Y;
			tabControl.Location=new Point(panelAptInfo.Left,panelAptInfo.Bottom+1);
			if(tabControl.Top>panelSheet.Bottom) {
				tabControl.Height=0;
			}
			else {
				tabControl.Height=panelSheet.Height-tabControl.Top+21;
			}
			if(!DefC.DefShortIsNull) {
				ApptView viewCur=null;
				if(comboView.SelectedIndex>0) {
					viewCur=_listApptViews[comboView.SelectedIndex-1];
				}
				ApptViewItemL.GetForCurView(viewCur,ApptDrawing.IsWeeklyView,SchedListPeriod);//refreshes visops,etc
				ApptDrawing.ApptSheetWidth=panelSheet.Width-vScrollBar1.Width;
				ApptDrawing.ComputeColWidth(0);
			}
			panelOps.Width=panelSheet.Width;
		}

		///<summary>Called from FormOpenDental upon startup.</summary>
		public void InitializeOnStartup() {
			//jsparks-
			//This method was inefficient and was causing 4 refreshes: RefreshPeriod, FillViews->comboView_SelectedIndexChanged, SetView?, SetWeeklyView.
			//This was especially inefficient, because after calling this method, FormOD was refreshing this module anyway.  So about 5 refreshes on startup.
			//Now, InitializedOnStartup remains false until the end of this method, preventing all refreshes when inside this method.
			//Verified that it only does one RefreshPeriod call to the db.
			if(InitializedOnStartup) {
				return;
			}
			LayoutPanels();
			ApptDrawing.RowsPerIncr=1;
			AppointmentL.DateSelected=DateTime.Now;
			ContrApptSingle.SelectedAptNum=-1;
			//RefreshPeriod();//Don't think this is needed.
			FillViews();//this will load the recently used ApptView and set comboView.SelectedIndex and calls ModuleSelected
			menuWeeklyApt.MenuItems.Clear();
			menuWeeklyApt.MenuItems.Add(Lan.g(this,"Copy to Pinboard"),new EventHandler(menuWeekly_Click));
			menuApt.MenuItems.Clear();
			menuApt.MenuItems.Add(Lan.g(this,"Copy to Pinboard"),new EventHandler(menuApt_Click));
			menuApt.MenuItems.Add("-");
			menuApt.MenuItems.Add(Lan.g(this,"Send to Unscheduled List"),new EventHandler(menuApt_Click));
			menuApt.MenuItems.Add(Lan.g(this,"Break Appointment"),new EventHandler(menuApt_Click));
			menuApt.MenuItems.Add(Lan.g(this,"Set Complete"),new EventHandler(menuApt_Click));
			menuApt.MenuItems.Add(Lan.g(this,"Delete"),new EventHandler(menuApt_Click));
			menuApt.MenuItems.Add(Lan.g(this,"Other Appointments"),new EventHandler(menuApt_Click));
			menuApt.MenuItems.Add("-");
			menuApt.MenuItems.Add(Lan.g(this,"Print Label"),new EventHandler(menuApt_Click));
			menuApt.MenuItems.Add(Lan.g(this,"Print Card"),new EventHandler(menuApt_Click));
			menuApt.MenuItems.Add(Lan.g(this,"Print Card for Entire Family"),new EventHandler(menuApt_Click));
			menuApt.MenuItems.Add(Lan.g(this,"Routing Slip"),new EventHandler(menuApt_Click));
			menuBlockout.MenuItems.Clear();
			menuBlockout.MenuItems.Add(Lan.g(this,"Edit Blockout"),OnBlockEdit_Click);
			menuBlockout.MenuItems.Add(Lan.g(this,"Cut Blockout"),OnBlockCut_Click);
			menuBlockout.MenuItems.Add(Lan.g(this,"Copy Blockout"),OnBlockCopy_Click);
			menuBlockout.MenuItems.Add(Lan.g(this,"Paste Blockout"),OnBlockPaste_Click);
			menuBlockout.MenuItems.Add(Lan.g(this,"Delete Blockout"),OnBlockDelete_Click);
			menuBlockout.MenuItems.Add(Lan.g(this,"Add Blockout"),OnBlockAdd_Click);
			menuBlockout.MenuItems.Add(Lan.g(this,"Blockout Cut-Copy-Paste"),OnBlockCutCopyPaste_Click);
			menuBlockout.MenuItems.Add(Lan.g(this,"Clear All Blockouts for Day"),OnClearBlockouts_Click);
			menuBlockout.MenuItems.Add(Lan.g(this,"Clear All Blockouts for Day, Op only"),OnClearBlockoutsOp_Click);
			if(PrefC.HasClinicsEnabled) {
				menuBlockout.MenuItems.Add(Lan.g(this,"Clear All Blockouts for Day, Clinic only"),OnClearBlockoutsClinic_Click);
			}
			menuBlockout.MenuItems.Add(Lan.g(this,"Edit Blockout Types"),OnBlockTypes_Click);
			Lan.C(this,new Control[]
				{
				butToday,
				//butTodayWk,
				butSearch,
				butClearPin,
				label2,
				label7,
				butMakeAppt,
				butMakeRecall,
				butFamRecall,
				butViewAppts,
				radioDay,
				radioWeek,
				tabWaiting,
				tabSched,
				tabProv,
				butLab,
				butBackWeek,
				butBackMonth,
				butFwdWeek,
				butFwdMonth,
				gridEmpSched,
				gridWaiting,
				gridProv
				});
			LayoutToolBar();
			//Appointment action buttons
			toolTip1.SetToolTip(butUnsched,Lan.g(this,"Send to Unscheduled List"));
			toolTip1.SetToolTip(butBreak,Lan.g(this,"Break"));
			toolTip1.SetToolTip(butComplete,Lan.g(this,"Set Complete"));
			toolTip1.SetToolTip(butDelete,Lan.g(this,"Delete"));
			//toolTip1.SetToolTip(butOther,Lan.g(this,"Other Appointments"));
			if(PrefC.GetString(PrefName.RegistrationKey).StartsWith("UPR6J92T29")) {
				butGraph.Visible=true;
			}
			SetWeeklyView(PrefC.GetBool(PrefName.ApptModuleDefaultToWeek));
			InitializedOnStartup=true;//moved this down to prevent view setting from triggering ModuleSelected().
		}

		///<summary></summary>
		public void LayoutToolBar() {
			ToolBarMain.Buttons.Clear();
			//ODToolBarButton button;
			//button=new ODToolBarButton("",0,Lan.g(this,"Select Patient"),"Patient");
			//button.Style=ODToolBarButtonStyle.DropDownButton;
			//button.DropDownMenu=menuPatient;
			//ToolBarMain.Buttons.Add(button);
			ToolBarMain.Buttons.Add(new ODToolBarButton("",2,Lan.g(this,"Appointment Lists"),"Lists"));
			ToolBarMain.Buttons.Add(new ODToolBarButton("",1,Lan.g(this,"Print Schedule"),"Print"));
			if(!ProgramProperties.IsAdvertisingDisabled(ProgramName.RapidCall)) {
				ToolBarMain.Buttons.Add(new ODToolBarButton("",3,Lan.g(this,"Rapid Call"),"RapidCall"));
			}
			ProgramL.LoadToolbar(ToolBarMain,ToolBarsAvail.ApptModule);
			ToolBarMain.Invalidate();
			Plugins.HookAddCode(this,"ContrAppt.LayoutToolBar_end",PatCur);
		}

		///<summary>Not in use.  See InstantClasses instead.</summary>
		private void ContrAppt_Load(object sender,System.EventArgs e) {

		}

		///<summary>The key press from the main form is passed down to this module.  This is guaranteed to be between the keys of F1 and F12.</summary>
		public void FunctionKeyPress(Keys keys) {
			string keyName=Enum.GetName(typeof(Keys),keys);//keyName will be F1, F2, ... F12
			int fKeyVal=int.Parse(keyName.TrimStart('F'));//strip off the F and convert to an int
			if(_listApptViews.Count<fKeyVal) {
				return;
			}
			SetView(_listApptViews[fKeyVal-1].ApptViewNum,true);
		}

		/// <summary>Sets the index of comboView for the specified ApptViewNum.  Then, does a ModuleSelected().  If saveToDb, then it will remember the ApptViewNum and currently selected ClinicNum for this workstation.</summary>
		private void SetView(long apptViewNum,bool saveToDb) {
			comboView.SelectedIndex=0;
			for(int i=0;i<_listApptViews.Count;i++) {
				if(apptViewNum==_listApptViews[i].ApptViewNum) {
					comboView.SelectedIndex=i+1;//+1 for 'none'
					break;
				}
			}
			if(!InitializedOnStartup) {
				return;//prevent ModuleSelected().
			}
			if(InitializedOnStartup && !Visible) {
				return;
			}
			if(saveToDb) {
				ComputerPrefs.LocalComputer.ApptViewNum=apptViewNum;
				ComputerPrefs.LocalComputer.ClinicNum=Clinics.ClinicNum;
				ComputerPrefs.Update(ComputerPrefs.LocalComputer);
				UserodApptViews.InsertOrUpdate(Security.CurUser.UserNum,Clinics.ClinicNum,apptViewNum);
			}
			if(PatCur==null) {
				ModuleSelected(0,listOpNums:ApptViewItems.GetOpsForView(apptViewNum),listProvNums:ApptViewItems.GetProvsForView(apptViewNum));
			}
			else {
				ModuleSelected(PatCur.PatNum,listOpNums:ApptViewItems.GetOpsForView(apptViewNum),listProvNums:ApptViewItems.GetProvsForView(apptViewNum));
			}
		}
		
		///<summary>Fills comboView and _listApptViews with the current list of views.  Also called from FormOpenDental.RefreshLocalData().</summary>
		public void FillViews() {
			comboView.Items.Clear();
			_listApptViews.Clear();
			comboView.Items.Add(Lan.g(this,"none"));
			string f="";
			for(int i=0;i<ApptViewC.List.Length;i++) {
				if(!PrefC.GetBool(PrefName.EasyNoClinics) && Clinics.ClinicNum!=ApptViewC.List[i].ClinicNum) {
					//This is intentional, we do NOT want 'Headquarters' to have access to clinic specific apptviews.  
					//Likewise, we do not want clinic specific views to be accessible from specific clinic filters.
					continue;
				}
				_listApptViews.Add(ApptViewC.List[i].Copy());
				if(_listApptViews.Count<=12)
					f="F"+_listApptViews.Count.ToString()+"-";
				else
					f="";
				comboView.Items.Add(f+ApptViewC.List[i].Description);
			}
			ApptView apptViewCur=GetApptViewForUser();
			if(apptViewCur!=null) {
				SetView(apptViewCur.ApptViewNum,false);//this also triggers ModuleSelected()
			}
			else {
				SetView(0,false);//this also triggers ModuleSelected()
			}
		}

		///<summary>Returns an ApptView for the currently logged in user and clinic combination. Can return null.
		///Will return the first available appointment view if this is the first time that this computer has connected to this database.</summary>
		private ApptView GetApptViewForUser() {
			//load the recently used apptview from the db, either the userodapptview table if an entry exists or the computerpref table if an entry for this computer exists
			ApptView apptViewCur=null;
			UserodApptView userodApptViewCur=UserodApptViews.GetOneForUserAndClinic(Security.CurUser.UserNum,Clinics.ClinicNum);
			if(userodApptViewCur!=null) { //if there is an entry in the userodapptview table for this user
				if(InitializedOnStartup //if either ContrAppt has already been initialized
					|| (Security.CurUser.ClinicIsRestricted //or the current user is restricted
					&& Clinics.ClinicNum!=ComputerPrefs.LocalComputer.ClinicNum)) //and FormOpenDental.ClinicNum (set to the current user's clinic) is not the computerpref clinic
				{
					apptViewCur=ApptViews.GetApptView(userodApptViewCur.ApptViewNum); //then load the view for the user in the userodapptview table
				}
			}
			if(apptViewCur==null //if no entry in the userodapptview table
				&& Clinics.ClinicNum==ComputerPrefs.LocalComputer.ClinicNum) //and if the program level ClinicNum is the stored recent ClinicNum for this computer 
			{
				apptViewCur=ApptViews.GetApptView(ComputerPrefs.LocalComputer.ApptViewNum);//use the computerpref for this computer and user
			}
			//Larger offices do not want to take the time to load all the data required to display the "none" view.
			//Therefore, for a NEW computer that is connecting to the database for the first time, load up the first available view that is not the none view.
			if(apptViewCur==null //if no entry in the ComputerPref table
				&& _listApptViews.Count>0) //There is an appointment view other than "none" to select
			{ 
				apptViewCur=_listApptViews[0];
			}
			return apptViewCur;
		}

		///<summary>Clicked today.</summary>
		private void butToday_Click(object sender,System.EventArgs e) {
			AppointmentL.DateSelected=DateTimeOD.Today;
			SetWeeklyView(radioWeek.Checked);
		}

		///<summary>Clicked back one day.</summary>
		private void butBack_Click(object sender,System.EventArgs e) {
			AppointmentL.DateSelected=AppointmentL.DateSelected.AddDays(-1);
			SetWeeklyView(radioWeek.Checked);
		}

		///<summary>Clicked forward one day.</summary>
		private void butFwd_Click(object sender,System.EventArgs e) {
			AppointmentL.DateSelected=AppointmentL.DateSelected.AddDays(1);
			SetWeeklyView(radioWeek.Checked);
		}

		private void butBackMonth_Click(object sender,EventArgs e) {
			AppointmentL.DateSelected=AppointmentL.DateSelected.AddMonths(-1);
			SetWeeklyView(radioWeek.Checked);
		}

		private void butBackWeek_Click(object sender,EventArgs e) {
			AppointmentL.DateSelected=AppointmentL.DateSelected.AddDays(-7);
			SetWeeklyView(radioWeek.Checked);
		}

		private void butFwdWeek_Click(object sender,EventArgs e) {
			AppointmentL.DateSelected=AppointmentL.DateSelected.AddDays(7);
			SetWeeklyView(radioWeek.Checked);
		}

		private void butFwdMonth_Click(object sender,EventArgs e) {
			AppointmentL.DateSelected=AppointmentL.DateSelected.AddMonths(1);
			SetWeeklyView(radioWeek.Checked);
		}

		private void radioDay_Click(object sender,EventArgs e) {
			SetWeeklyView(false);
		}

		private void radioWeek_Click(object sender,EventArgs e) {
			SetWeeklyView(true);
		}

		///<summary>Clicked a date on the calendar.</summary>
		private void Calendar2_DateSelected(object sender,System.Windows.Forms.DateRangeEventArgs e) {
			AppointmentL.DateSelected=Calendar2.SelectionStart;
			SetWeeklyView(radioWeek.Checked);
		}

		///<summary>Switches between weekly view and daily view.  AppointmentL.DateSelected needs to be set first.  Calculates WeekStartDate and WeekEndDate based on AppointmentL.DateSelected.  Then calls either RefreshPeriod or ModuleSelected.</summary>
		private void SetWeeklyView(bool isWeeklyView) {
			//if the weekly view doesn't change, then just RefreshPeriod
			bool weeklyViewChanged=false;
			if(isWeeklyView!=ApptDrawing.IsWeeklyView) {
				weeklyViewChanged=true;
			}
			//for those few times when the radiobuttons aren't quite right:
			if(isWeeklyView) {
				radioWeek.Checked=true;
				butFwd.Enabled=false;
				butBack.Enabled=false;
			}
			else {
				radioDay.Checked=true;
				butFwd.Enabled=true;
				butBack.Enabled=true;
			}
			if((int)AppointmentL.DateSelected.DayOfWeek==0) {//if sunday
				WeekStartDate=AppointmentL.DateSelected.AddDays(-6).Date;//go back to previous monday
			}
			else {
				WeekStartDate=AppointmentL.DateSelected.AddDays(1-(int)AppointmentL.DateSelected.DayOfWeek).Date;//go back to current monday
			}
			WeekEndDate=WeekStartDate.AddDays(ApptDrawing.NumOfWeekDaysToDisplay-1).Date;
			ApptDrawing.IsWeeklyView=isWeeklyView;
			if(!InitializedOnStartup) {
				return;//prevent refreshing repeatedly on startup
			}
			long apptViewNum=0;
			if(ApptViewItemL.ApptViewCur!=null) {
				apptViewNum=ApptViewItemL.ApptViewCur.ApptViewNum;
			}
			if(weeklyViewChanged || isWeeklyView) {
				if(PatCur==null) {
					ModuleSelected(0,listOpNums:ApptViewItems.GetOpsForView(apptViewNum),listProvNums:ApptViewItems.GetProvsForView(apptViewNum));
				}
				else {
					ModuleSelected(PatCur.PatNum,listOpNums:ApptViewItems.GetOpsForView(apptViewNum),listProvNums:ApptViewItems.GetProvsForView(apptViewNum));
				}
			}
			else {
				RefreshPeriod(listOpNums:ApptViewItems.GetOpsForView(apptViewNum),listProvNums:ApptViewItems.GetProvsForView(apptViewNum));
			}
		}

		///<summary>Fills the lab summary for the day.</summary>
		private void FillLab(List<LabCase> labCaseList) {
			int notRec=0;
			for(int i=0;i<labCaseList.Count;i++) {
				if(labCaseList[i].DateTimeChecked.Year>1880) {
					continue;
				}
				if(labCaseList[i].DateTimeRecd.Year>1880) {
					continue;
				}
				notRec++;
			}
			if(notRec==0) {
				textLab.Font=new Font(FontFamily.GenericSansSerif,8,FontStyle.Regular);
				textLab.ForeColor=Color.Black;
				textLab.Text=Lan.g(this,"All Received");
			}
			else {
				textLab.Font=new Font(FontFamily.GenericSansSerif,8,FontStyle.Bold);
				textLab.ForeColor=Color.DarkRed;
				textLab.Text=notRec.ToString()+Lan.g(this," NOT RECEIVED");
			}
		}

		///<summary>Fills the production summary for the day.</summary>
		private void FillProduction() {
			bool showProduction=false;
			for(int i=0;i<ApptViewItemL.ApptRows.Count;i++) {
				if(ApptViewItemL.ApptRows[i].ElementDesc=="Production") {
					showProduction=true;
				}
			}
			if(!showProduction) {
				textProduction.Text="";
				return;
			}
			decimal grossproduction=0;
			decimal netproduction=0;
			int indexProv;
			for(int i=0;i<DS.Tables["Appointments"].Rows.Count;i++) {
				indexProv=-1;
				if(DS.Tables["Appointments"].Rows[i]["IsHygiene"].ToString()=="1") {//if isHygiene
					if(DS.Tables["Appointments"].Rows[i]["ProvHyg"].ToString()=="0") {//if no hyg prov set.
						indexProv=ApptDrawing.GetIndexProv(PIn.Long(DS.Tables["Appointments"].Rows[i]["ProvNum"].ToString()));
					}
					else {
						indexProv=ApptDrawing.GetIndexProv(PIn.Long(DS.Tables["Appointments"].Rows[i]["ProvHyg"].ToString()));
					}
				}
				else {//not hyg
					indexProv=ApptDrawing.GetIndexProv(PIn.Long(DS.Tables["Appointments"].Rows[i]["ProvNum"].ToString()));
				}
				if(indexProv==-1) {
					continue;
				}
				if(DS.Tables["Appointments"].Rows[i]["AptStatus"].ToString()!=((int)ApptStatus.Broken).ToString()
					&& DS.Tables["Appointments"].Rows[i]["AptStatus"].ToString()!=((int)ApptStatus.UnschedList).ToString()
					&& DS.Tables["Appointments"].Rows[i]["AptStatus"].ToString()!=((int)ApptStatus.PtNote).ToString()
					&& DS.Tables["Appointments"].Rows[i]["AptStatus"].ToString()!=((int)ApptStatus.PtNoteCompleted).ToString()) 
				{
					long clinicNum=PIn.Long(DS.Tables["Appointments"].Rows[i]["ClinicNum"].ToString());
					//When the program is restricted to a specific clinic, only count up production for the corresponding clinic.
					if(!PrefC.GetBool(PrefName.EasyNoClinics) 
						&& Clinics.ClinicNum!=0
						&& Clinics.ClinicNum!=clinicNum) {
						continue;//This appointment is for a different clinic.  Do not include this production in the daily prod.
					}
					//In order to get production numbers split by provider, it would require generating total production numbers
					//in another table from the business layer.  But that will only work if hyg procedures are appropriately assigned
					//when setting appointments.
					grossproduction+=PIn.Decimal(DS.Tables["Appointments"].Rows[i]["productionVal"].ToString());
					netproduction+=PIn.Decimal(DS.Tables["Appointments"].Rows[i]["productionVal"].ToString())-PIn.Decimal(DS.Tables["Appointments"].Rows[i]["writeoffPPO"].ToString());
				}
			}
			if(PrefC.GetBool(PrefName.ApptModuleAdjustmentsInProd) && DS.Tables["Appointments"].Rows.Count>0) {
				netproduction+=PIn.Decimal(DS.Tables["Appointments"].Rows[0]["adjustmentTotal"].ToString());
			}
			textProduction.Text=grossproduction.ToString("c0");
			if(grossproduction!=netproduction) {
				textProduction.Text+=Lan.g(this,", net:")+netproduction.ToString("c0");
			}
		}

		///<summary></summary>
		private void FillProvSched() {
			DataTable table=DS.Tables["ProvSched"];
			gridProv.BeginUpdate();
			gridProv.Columns.Clear();
			ODGridColumn col=new ODGridColumn(Lan.g("TableApptProvSched","Provider"),80);
			gridProv.Columns.Add(col);
			col=new ODGridColumn(Lan.g("TableApptProvSched","Schedule"),70);
			gridProv.Columns.Add(col);
			col=new ODGridColumn(Lan.g("TableApptProvSched","Notes"),100);
			gridProv.Columns.Add(col);
			gridProv.Rows.Clear();
			ODGridRow row;
			foreach(DataRow dRow in table.Rows) { 
				row=new ODGridRow();
				row.Cells.Add(dRow["ProvAbbr"].ToString());
				row.Cells.Add(dRow["schedule"].ToString());
				row.Cells.Add(dRow["Note"].ToString());
				gridProv.Rows.Add(row);
			}
			gridProv.EndUpdate();
		}

		///<summary></summary>
		private void FillEmpSched() {
			DataTable table=DS.Tables["EmpSched"];
			gridEmpSched.BeginUpdate();
			gridEmpSched.Columns.Clear();
			ODGridColumn col=new ODGridColumn(Lan.g("TableApptEmpSched","Employee"),80);
			gridEmpSched.Columns.Add(col);
			col=new ODGridColumn(Lan.g("TableApptEmpSched","Schedule"),70);
			gridEmpSched.Columns.Add(col);
			col=new ODGridColumn(Lan.g("TableApptEmpSched","Notes"),100);
			gridEmpSched.Columns.Add(col);
			gridEmpSched.Rows.Clear();
			ODGridRow row;
			for(int i=0;i<table.Rows.Count;i++) {
				row=new ODGridRow();
				row.Cells.Add(table.Rows[i]["empName"].ToString());
				row.Cells.Add(table.Rows[i]["schedule"].ToString());
				row.Cells.Add(table.Rows[i]["Note"].ToString());
				gridEmpSched.Rows.Add(row);
			}
			gridEmpSched.EndUpdate();
		}

		///<summary></summary>
		private void FillWaitingRoom() {
			if(DS==null) {
				return;
			}
			TimeSpan delta=DateTime.Now-LastTimeDataRetrieved;
			DataTable table=DS.Tables["WaitingRoom"];
			List<Operatory> listOpsForClinic=new List<Operatory>();
			List<Operatory> listOpsForApptView=new List<Operatory>();
			if(PrefC.GetBool(PrefName.WaitingRoomFilterByView)) {
				//In order to filter the waiting room by appointment view, we need to always grab the operatories visible for TODAY.
				//This way, regardless of what day the customer is looking at, the waiting room will only change when they change appointment views.
				List<Schedule> listSchedulesForToday=Schedules.ConvertTableToList(Schedules.GetPeriodSchedule(DateTime.Now,DateTime.Now));
				ApptView viewCur=null;
				if(comboView.SelectedIndex>0) {
					viewCur=_listApptViews[comboView.SelectedIndex-1];
				}
				listOpsForApptView=ApptViewItemL.GetOpsForApptView(viewCur,ApptDrawing.IsWeeklyView,listSchedulesForToday);
			}
			if(!PrefC.GetBool(PrefName.EasyNoClinics)) {//Using clinics
				listOpsForClinic=Operatories.GetOpsForClinic(Clinics.ClinicNum);
			}
			gridWaiting.BeginUpdate();
			gridWaiting.Columns.Clear();
			ODGridColumn col=new ODGridColumn(Lan.g("TableApptWaiting","Patient"),130);
			gridWaiting.Columns.Add(col);
			col=new ODGridColumn(Lan.g("TableApptWaiting","Waited"),100,HorizontalAlignment.Center);
			gridWaiting.Columns.Add(col);
			gridWaiting.Rows.Clear();
			DateTime waitTime;
			ODGridRow row;
			int waitingRoomAlertTime=PrefC.GetInt(PrefName.WaitingRoomAlertTime);
			Color waitingRoomAlertColor=PrefC.GetColor(PrefName.WaitingRoomAlertColor);
			for(int i=0;i<table.Rows.Count;i++) {
				//Always filter the waiting room by appointment view first, regardless of using clinics or not.
				if(PrefC.GetBool(PrefName.WaitingRoomFilterByView)) {
					bool isInView=false;
					for(int j=0;j<listOpsForApptView.Count;j++) {
						if(listOpsForApptView[j].OperatoryNum==PIn.Long(table.Rows[i]["OpNum"].ToString())) {
							isInView=true;
							break;
						}
					}
					if(!isInView) {
						continue;
					}
				}
				//We only want to filter the waiting room by the clinic's operatories when clinics are enabled and they are not using 'Headquarters' mode.
				if(!PrefC.GetBool(PrefName.EasyNoClinics) && Clinics.ClinicNum!=0) {
					bool isInView=false;
					for(int j=0;j<listOpsForClinic.Count;j++) {
						if(listOpsForClinic[j].OperatoryNum==PIn.Long(table.Rows[i]["OpNum"].ToString())) {
							isInView=true;
							break;
						}
					}
					if(!isInView) {
						continue;
					}
				}
				row=new ODGridRow();
				row.Cells.Add(table.Rows[i]["patName"].ToString());
				waitTime=DateTime.Parse(table.Rows[i]["waitTime"].ToString());//we ignore date
				waitTime+=delta;
				row.Cells.Add(waitTime.ToString("H:mm:ss"));
				row.Bold=false;
				if(waitingRoomAlertTime>0 && waitingRoomAlertTime<=waitTime.Minute+(waitTime.Hour*60)) {
					row.ColorText=waitingRoomAlertColor;
					row.Bold=true;
				}
				gridWaiting.Rows.Add(row);
			}
			gridWaiting.EndUpdate();
		}

		private void gridEmpSched_DoubleClick(object sender,EventArgs e) {
			gridSchedDoubleClickHelper();
		}

		private void gridProv_CellDoubleClick(object sender,ODGridClickEventArgs e) {
			gridSchedDoubleClickHelper();
		}

		private void gridSchedDoubleClickHelper() {
			if(ApptDrawing.IsWeeklyView) {
				MsgBox.Show(this,"Not available in weekly view");
				return;
			}
			if(!Security.IsAuthorized(Permissions.Schedules)) {
				return;
			}
			FormScheduleDayEdit FormSDE = new FormScheduleDayEdit(AppointmentL.DateSelected,Clinics.ClinicNum);
			FormSDE.ShowOkSchedule=true;
			FormSDE.ShowDialog();
			SecurityLogs.MakeLogEntry(Permissions.Schedules,0,"");
			SetWeeklyView(false);//to refresh
			if(FormSDE.GotoScheduleOnClose) {
				FormSchedule FormS = new FormSchedule();
				FormS.ShowDialog();
			}
		}

		///<summary>Creates one bitmap image for each appointment if visible, and draws those bitmaps onto the main appt background image.
		///To redraw specific appts, pass the apptNums in as a list.  Pass no parameters to redraw all appts.</summary>
		private void CreateAptShadowsOnMain(List<long> listAptNumsOnlyRedraw=null) {
			if(ContrApptSheet2.Shadow==null) {//if user resizes window to be very narrow
				return;
			}
			DateTime startDate=(ApptDrawing.IsWeeklyView?WeekStartDate:AppointmentL.DateSelected);
			DateTime endDate=(ApptDrawing.IsWeeklyView?WeekEndDate:AppointmentL.DateSelected);
			using(Graphics grfx=Graphics.FromImage(ContrApptSheet2.Shadow)) {
				for(int i=0;i<DS.Tables["Appointments"].Rows.Count;i++) {
					if(listAptNumsOnlyRedraw!=null && !listAptNumsOnlyRedraw.Contains(PIn.Long(DS.Tables["Appointments"].Rows[i]["AptNum"].ToString()))) {
						continue;//Don't redraw this appointment
					}
					if(PIn.Date(DS.Tables["Appointments"].Rows[i]["AptDateTime"].ToString()).Date<startDate.Date || PIn.Date(DS.Tables["Appointments"].Rows[i]["AptDateTime"].ToString()).Date>endDate.Date){
						continue;//Appointment is outside of our date range.
					}
					ContrApptSingle3[i].CreateShadow();
					if(ContrApptSingle3[i].Location.X>=ApptDrawing.TimeWidth+ApptDrawing.ProvWidth*ApptDrawing.ProvCount
			    && ContrApptSingle3[i].Width>3
			    && ContrApptSingle3[i].Shadow!=null) {
						grfx.DrawImage(ContrApptSingle3[i].Shadow
							,ContrApptSingle3[i].Location.X,ContrApptSingle3[i].Location.Y);
					}
					if(ContrApptSingle3[i].Shadow!=null) {
						ContrApptSingle3[i].Shadow.Dispose();
						ContrApptSingle3[i].Shadow=null;
					}
				}
			}
		}

		///<summary>Gets the index within the array of appointment controls, based on the supplied primary key.</summary>
		private int GetIndex(long myAptNum) {
			int retVal=-1;
			for(int i=0;i<ContrApptSingle3.Length;i++) {
				if(ContrApptSingle3[i].DataRoww["AptNum"].ToString()==myAptNum.ToString()) {
					retVal=i;
				}
			}
			return retVal;
		}

		private void SendToPinBoard(long aptNum) {
			List<long> list=new List<long>();
			list.Add(aptNum);
			SendToPinboard(list);
		}

		///<summary>Loads all info for for specified appointment into the control that displays the pinboard appointment. Runs RefreshModulePatient.  
		///Sets pinboard appointment as selected.</summary>
		private void SendToPinboard(List<long> aptNums) {
			if(IsHqNoneView()) {
				MsgBox.Show(this,"Appointments can't be sent to the pinboard when an appointment view or clinic hasn't been selected.");
				return;
			}
			if(aptNums.Count==0) {
				return;
			}
			DataRow row=null;
			for(int a=0;a<aptNums.Count;a++) {
				row=null;
				for(int i=0;i<DS.Tables["Appointments"].Rows.Count;i++) {
					if(DS.Tables["Appointments"].Rows[i]["AptNum"].ToString()==aptNums[a].ToString()) {
						row=DS.Tables["Appointments"].Rows[i];
						break;
					}
				}
				DataTable tableApptFields=DS.Tables["ApptFields"];
				DataTable tablePatFields=DS.Tables["PatFields"];
				//The appointment wasn't found in the main DataSet so we need to go to the database and get that appointment's information.
				//==Andrew 06/10/2016 - This should never get hit anymore but we left it here as a failsafe since we're backporting.
				if(row==null) {
					List<long> listOpNums=null;
					List<long> listProvNums=null;
					if(Clinics.ClinicNum!=0 || comboView.SelectedIndex!=0) {
						listOpNums=ApptDrawing.VisOps.Select(x => x.OperatoryNum).ToList();
						listProvNums=ApptDrawing.VisProvs.Select(x => x.ProvNum).ToList();
					}
					DataTable tableAppts=Appointments.RefreshOneApt(aptNums[a],false,listOpNums:listOpNums,listProvNums:listProvNums).Tables["Appointments"];
					row=tableAppts.Rows[0];
					if(row["AptStatus"].ToString()=="6") {//planned
						//then do it again the right way
						tableAppts=Appointments.RefreshOneApt(aptNums[a],true,listOpNums:listOpNums,listProvNums:listProvNums).Tables["Appointments"];
						row=tableAppts.Rows[0];
					}
					//The appt fields are not in DS.Tables["ApptFields"] since the appt is not visible on the schedule.
					tableApptFields=Appointments.GetApptFields(tableAppts);
				}
				//Send this planned appointment's information to the pinboard.
				pinBoard.AddAppointment(row,tableApptFields,tablePatFields);
			}
			RefreshModuleDataPatient(PIn.Long(row["PatNum"].ToString()));//set the pt to the last appt on the pinboard.
			FormOpenDental.S_Contr_PatientSelected(PatCur,true,false);
			mouseIsDown=false;
			boolAptMoved=false;
			ContrApptSingle.SelectedAptNum=-1;
		}

		private void pinBoard_SelectedIndexChanged(object sender,EventArgs e) {
			RefreshModuleDataPatient(PIn.Long(pinBoard.ApptList[pinBoard.SelectedIndex].DataRoww["PatNum"].ToString()));
			RefreshModuleScreenPatient();
			CancelPinMouseDown=false;
			FormOpenDental.S_Contr_PatientSelected(PatCur,false,false);
			//The line above can trigger a popup dialog which can cause the tempAppt to get stuck to the mouse
			//RefreshModulePatient(PIn.PInt(pinBoard.ApptList[pinBoard.SelectedIndex].DataRoww["PatNum"].ToString()));
			//Since this is usually caused by user mouse, then it goes right into pinBoard_MouseDown().
		}

		///<Summary>Sets selected and prepares for drag.</Summary>
		private void pinBoard_MouseDown(object sender,MouseEventArgs e) {
			if(pinBoard.SelectedIndex==-1) {
				return;
			}
			if(mouseIsDown) {//User right clicked while draging appt around.
				return;
			}
			if(e.Button==MouseButtons.Right) {
				ContextMenu cmen=new ContextMenu();
				MenuItem menuItemProv=new MenuItem(Lan.g(this,"Change Provider"));
				menuItemProv.Click+=new EventHandler(menuItemProv_Click);
				cmen.MenuItems.Add(menuItemProv);
				cmen.Show(pinBoard,e.Location);
				return;
			}
			if(CancelPinMouseDown) {//I'm worried that setting this to false in pinBoard_SelectedIndexChanged is not frequent enough,
				//because a mouse down could happen without the selected index changing.
				//But in that case, a popup would already have happened.
				//Worst case scenario is that user would have to try again.
				CancelPinMouseDown=false;
				return;
			}
			mouseIsDown = true;
			//ContrApptSingle.PinBoardIsSelected=true;
			TempApptSingle=new ContrApptSingle();
			TempApptSingle.DataRoww=pinBoard.SelectedAppt.DataRoww;
			TempApptSingle.TableApptFields=pinBoard.SelectedAppt.TableApptFields;
			TempApptSingle.TablePatFields=pinBoard.SelectedAppt.TablePatFields;
			TempApptSingle.Visible=false;
			Controls.Add(TempApptSingle);
			TempApptSingle.Location=ApptSingleDrawing.SetLocation(TempApptSingle.DataRoww,0,ApptDrawing.VisOps.Count,0);
			TempApptSingle.Size=ApptSingleDrawing.SetSize(TempApptSingle.DataRoww);
			TempApptSingle.PatternShowing=ApptSingleDrawing.GetPatternShowing(TempApptSingle.DataRoww["Pattern"].ToString());
			TempApptSingle.CreateShadow();
			TempApptSingle.BringToFront();
			ContrApptSingle.SelectedAptNum=-1;
			//RefreshModulePatient(PIn.PInt(PinApptSingle.DataRoww["PatNum"].ToString()));//already done
			//PinApptSingle.CreateShadow();
			//PinApptSingle.Refresh();
			CreateAptShadowsOnMain();//to clear previous selection
			ContrApptSheet2.DrawShadow();
			//mouseOrigin is in ContrAppt coordinates (essentially, the entire window)
			mouseOrigin.X=e.X+pinBoard.Location.X+panelCalendar.Location.X;
			//e.X+PinApptSingle.Location.X;
			mouseOrigin.Y=e.Y+pinBoard.SelectedAppt.Location.Y+pinBoard.Location.Y+panelCalendar.Location.Y;
			//e.Y+PinApptSingle.Location.Y;
			contOrigin=new Point(pinBoard.Location.X+panelCalendar.Location.X,
				pinBoard.SelectedAppt.Location.Y+pinBoard.Location.Y+panelCalendar.Location.Y);
			//PinApptSingle.Location;
		}

		void menuItemProv_Click(object sender,EventArgs e) {
			//throw new NotImplementedException();
			Appointment apt=Appointments.GetOneApt(PIn.Long(pinBoard.SelectedAppt.DataRoww["AptNum"].ToString()));
			Appointment oldApt=apt.Copy();
			if(apt==null) {
				MessageBox.Show("Appointment not found.");
				return;
			}
			long provNum=apt.ProvNum;
			if(apt.IsHygiene) {
				provNum=apt.ProvHyg;
			}
			FormProviderPick formPick=new FormProviderPick();
			formPick.SelectedProvNum=provNum;
			formPick.ShowDialog();
			if(formPick.DialogResult!=DialogResult.OK) {
				return;
			}
			if(formPick.SelectedProvNum==provNum) {
				return;//provider not changed.
			}
			if(apt.IsHygiene) {
				apt.ProvHyg=formPick.SelectedProvNum;
			}
			else {
				apt.ProvNum=formPick.SelectedProvNum;
			}
			List<Procedure> procsForSingleApt=Procedures.GetProcsForSingle(apt.AptNum,false);
			List<long> codeNums=new List<long>();
			for(int p=0;p<procsForSingleApt.Count;p++) {
				codeNums.Add(procsForSingleApt[p].CodeNum);
			}
			string calcPattern=Appointments.CalculatePattern(apt.ProvNum,apt.ProvHyg,codeNums,true);
			if(apt.Pattern != calcPattern) {
				if(!apt.TimeLocked || MsgBox.Show(this,MsgBoxButtons.YesNo,"Appointment length is locked.  Change length for new provider anyway?")) {
					apt.Pattern=calcPattern;
				}
			}
			Appointments.Update(apt,oldApt);
			Procedures.SetProvidersInAppointment(apt,procsForSingleApt);
			List<long> listOpNums=null;
			List<long> listProvNums=null;
			if(Clinics.ClinicNum!=0 || comboView.SelectedIndex!=0) {
				listOpNums=ApptDrawing.VisOps.Select(x => x.OperatoryNum).ToList();
				listProvNums=ApptDrawing.VisProvs.Select(x => x.ProvNum).ToList();
			}
			ModuleSelected(PatCur.PatNum,listOpNums:listOpNums,listProvNums:listProvNums);
			DataRow row=null;
			if(apt.AptStatus==ApptStatus.Planned) {
				row=Appointments.RefreshOneApt(apt.AptNum,true,listOpNums:listOpNums,listProvNums:listProvNums).Tables["Appointments"].Rows[0];
			}
			else {
				row=Appointments.RefreshOneApt(apt.AptNum,false,listOpNums:listOpNums,listProvNums:listProvNums).Tables["Appointments"].Rows[0];
			}
			if(row==null) {
				return;
			}
			pinBoard.ResetData(row);
			//MessageBox.Show(Providers.GetAbbr(apt.ProvNum));
		}

		///<Summary>Moves pinboard appt if mouse is down.</Summary>
		private void pinBoard_MouseMove(object sender,MouseEventArgs e) {
			if(!mouseIsDown) {
				return;
			}
			//not sure what this is:
			//if((Math.Abs(e.X+PinApptSingle.Location.X-mouseOrigin.X)<1)
			//	&&(Math.Abs(e.Y+PinApptSingle.Location.Y-mouseOrigin.Y)<1)){
			//	return;
			//}
			if(TempApptSingle.Location==new Point(0,0)) {
				TempApptSingle.Height=1;//to prevent flicker in UL corner
			}
			TempApptSingle.Visible=true;
			boolAptMoved=true;
			TempApptSingle.Location=new Point(
				contOrigin.X+(e.X+pinBoard.Location.X+panelCalendar.Location.X)-mouseOrigin.X,
				contOrigin.Y+(e.Y+pinBoard.SelectedAppt.Location.Y+pinBoard.Location.Y+panelCalendar.Location.Y)-mouseOrigin.Y);
			if(TempApptSingle.Height==1) {
				TempApptSingle.Size=ApptSingleDrawing.SetSize(TempApptSingle.DataRoww);
			}
		}

		///<Summary>Usually happens after a pinboard appt has been dragged onto main appt sheet.</Summary>
		private void pinBoard_MouseUp(object sender,MouseEventArgs e) {
			if(!boolAptMoved) {
				mouseIsDown=false;
				if(TempApptSingle!=null) {
					TempApptSingle.Dispose();
				}
				return;
			}
			//Make sure there are operatories for the appointment to be scheduled and make sure the user dragged the appointment to a valid location.
			if(ApptDrawing.VisOps.Count==0 || TempApptSingle.Location.X>ContrApptSheet2.Width) {
				mouseIsDown=false;
				boolAptMoved=false;
				TempApptSingle.Dispose();
				return;
			}
			if(pinBoard.SelectedAppt.DataRoww["AptStatus"].ToString()==((int)ApptStatus.Planned).ToString()//if Planned appt is on pinboard
				&& (!Security.IsAuthorized(Permissions.AppointmentCreate)//and no permission to create a new appt
					|| PatRestrictions.IsRestricted(PIn.Long(pinBoard.SelectedAppt.DataRoww["PatNum"].ToString()),PatRestrict.ApptSchedule)))//or pat restricted
			{
				mouseIsDown = false;
				boolAptMoved=false;
				TempApptSingle.Dispose();
				return;
			}
			//security prevents moving an appointment by preventing placing it on the pinboard, not here
			//We no longer ask user this question.  It just slows things down: "Move Appointment?"
			//convert loc to new time
			Appointment aptCur=Appointments.GetOneApt(PIn.Long(pinBoard.SelectedAppt.DataRoww["AptNum"].ToString()));
			if(aptCur==null) {
				MsgBox.Show(this,"This appointment has been deleted since it was moved to the pinboard. It will now be cleared from the pinboard.");
				mouseIsDown = false;
				boolAptMoved=false;
				TempApptSingle.Dispose();
				pinBoard.ClearSelected();
				return;
			}
			Appointment aptOld=aptCur.Copy();
			RefreshModuleDataPatient(PIn.Long(pinBoard.SelectedAppt.DataRoww["PatNum"].ToString()));//redundant?
			//Patient pat=Patients.GetPat(PIn.PInt(pinBoard.SelectedAppt.DataRoww["PatNum"].ToString()));
			if(aptCur.IsNewPatient && AppointmentL.DateSelected!=aptCur.AptDateTime) {
				Procedures.SetDateFirstVisit(AppointmentL.DateSelected,4,PatCur);
			}
			int tHr=ApptDrawing.ConvertToHour(TempApptSingle.Location.Y-ContrApptSheet2.Location.Y-panelSheet.Location.Y);
			int tMin=ApptDrawing.ConvertToMin(TempApptSingle.Location.Y-ContrApptSheet2.Location.Y-panelSheet.Location.Y);
			DateTime tDate=AppointmentL.DateSelected;
			if(ApptDrawing.IsWeeklyView) {
				tDate=WeekStartDate.AddDays(ApptDrawing.ConvertToDay(TempApptSingle.Location.X-ContrApptSheet2.Location.X));
			}
			DateTime fromDate=aptCur.AptDateTime.Date;
			aptCur.AptDateTime=new DateTime(tDate.Year,tDate.Month,tDate.Day,tHr,tMin,0);
			if(AppointmentRuleC.List.Length>0) {
				//this is crude and temporary:
				List<long> aptNums=new List<long>();
				for(int i=0;i<DS.Tables["Appointments"].Rows.Count;i++) {
					aptNums.Add(PIn.Long(DS.Tables["Appointments"].Rows[i]["AptNum"].ToString()));//ListDay[i].AptNum;
				}
				List<Procedure> procsMultApts=Procedures.GetProcsMultApts(aptNums);
				Procedure[] procsForOne=Procedures.GetProcsOneApt(aptCur.AptNum,procsMultApts);
				ArrayList doubleBookedCodes=
					AppointmentL.GetDoubleBookedCodes(aptCur,DS.Tables["Appointments"].Copy(),procsMultApts,procsForOne);
				if(doubleBookedCodes.Count>0) {//if some codes would be double booked
					if(AppointmentRules.IsBlocked(doubleBookedCodes)) {
						MessageBox.Show(Lan.g(this,"Not allowed to double book:")+" "
							+AppointmentRules.GetBlockedDescription(doubleBookedCodes));
						mouseIsDown = false;
						boolAptMoved=false;
						TempApptSingle.Dispose();
						return;
					}
				}
			}
			Operatory curOp=ApptDrawing.VisOps
				[ApptDrawing.ConvertToOp(TempApptSingle.Location.X-ContrApptSheet2.Location.X)];
			aptCur.Op=curOp.OperatoryNum;
			long assignedDent=Schedules.GetAssignedProvNumForSpot(SchedListPeriod,curOp,false,aptCur.AptDateTime);
			long assignedHyg=Schedules.GetAssignedProvNumForSpot(SchedListPeriod,curOp,true,aptCur.AptDateTime);
			List<Procedure> procsForSingleApt=null;
			if(aptCur.AptStatus!=ApptStatus.PtNote && aptCur.AptStatus!=ApptStatus.PtNoteCompleted) {
				if(PatCur.AskToArriveEarly>0) {
					aptCur.DateTimeAskedToArrive=aptCur.AptDateTime.AddMinutes(-PatCur.AskToArriveEarly);
					MessageBox.Show(Lan.g(this,"Ask patient to arrive")+" "+PatCur.AskToArriveEarly
						+" "+Lan.g(this,"minutes early at")+" "+aptCur.DateTimeAskedToArrive.ToShortTimeString()+".");
				}
				else {
					aptCur.DateTimeAskedToArrive=DateTime.MinValue;
				}
				//if no dentist/hygienist is assigned to spot, then keep the original dentist/hygienist without prompt.  All appts must have prov.
				if((assignedDent!=0 && assignedDent!=aptCur.ProvNum) || (assignedHyg!=0 && assignedHyg!=aptCur.ProvHyg)) {
					if(MessageBox.Show(Lan.g(this,"Change provider?"),"",MessageBoxButtons.YesNo)==DialogResult.Yes) {
						if(assignedDent!=0) {
							aptCur.ProvNum=assignedDent;
						}
						if(assignedHyg!=0 || PrefC.GetBool(PrefName.ApptSecondaryProviderConsiderOpOnly)) {//the hygienist will only be changed if the spot has a hygienist.
							aptCur.ProvHyg=assignedHyg;
						}
						if(curOp.IsHygiene) {
							aptCur.IsHygiene=true;
						}
						else {//op not marked as hygiene op
							if(assignedDent==0) {//no dentist assigned
								if(assignedHyg!=0) {//hyg is assigned (we don't really have to test for this)
									aptCur.IsHygiene=true;
								}
							}
							else {//dentist is assigned
								if(assignedHyg==0) {//hyg is not assigned
									aptCur.IsHygiene=false;
								}
								//if both dentist and hyg are assigned, it's tricky
								//only explicitly set it if user has a dentist assigned to the op
								if(curOp.ProvDentist!=0) {
									aptCur.IsHygiene=false;
								}
							}
						}
						bool isplanned=aptCur.AptStatus==ApptStatus.Planned;
						procsForSingleApt=Procedures.GetProcsForSingle(aptCur.AptNum,isplanned);
						List<long> codeNums=new List<long>();
						for(int p=0;p<procsForSingleApt.Count;p++) {
							codeNums.Add(procsForSingleApt[p].CodeNum);
						}
						string calcPattern=Appointments.CalculatePattern(aptCur.ProvNum,aptCur.ProvHyg,codeNums,true);
						if(aptCur.Pattern != calcPattern && !PrefC.GetBool(PrefName.AppointmentTimeIsLocked)) {
							if(aptCur.TimeLocked){
								if(MsgBox.Show(this,MsgBoxButtons.YesNo,"Appointment length is locked.  Change length for new provider anyway?")) {
									aptCur.Pattern=calcPattern;
								}
							}
							else {//appt time not locked
								if(MsgBox.Show(this,MsgBoxButtons.YesNo,"Change length for new provider?")) {
									aptCur.Pattern=calcPattern;
								}
							}
						}
					}
				}
			}
			//Appointment is ready to be placed on the schedule.  Refresh all appointments to avoid overlapping.
			RefreshPeriod();
			if(DoesOverlap(aptCur)) {
				int startingOp=ApptDrawing.GetIndexOp(aptCur.Op);
				bool stillOverlaps=true;
				for(int i=startingOp;i<ApptDrawing.VisOps.Count;i++) {
					aptCur.Op=ApptDrawing.VisOps[i].OperatoryNum;
					if(!DoesOverlap(aptCur)) {
						stillOverlaps=false;
						break;
					}
				}
				if(stillOverlaps) {
					for(int i=startingOp;i>=0;i--) {
						aptCur.Op=ApptDrawing.VisOps[i].OperatoryNum;
						if(!DoesOverlap(aptCur)) {
							stillOverlaps=false;
							break;
						}
					}
				}
				if(stillOverlaps) {
					MessageBox.Show(Lan.g(this,"Appointment overlaps existing appointment."));
					mouseIsDown=false;
					boolAptMoved=false;
					TempApptSingle.Dispose();
					return;
				}
			}
			Operatory opCur=Operatories.GetOperatory(aptCur.Op);
			Operatory opOld=Operatories.GetOperatory(aptOld.Op);
			if(opOld==null || opCur.SetProspective!=opOld.SetProspective) {
				if(opCur.SetProspective && PatCur.PatStatus!=PatientStatus.Prospective) { //Don't need to prompt if patient is already prospective.
					if(MsgBox.Show(this,MsgBoxButtons.OKCancel,"Patient's status will be set to Prospective.")) {
						Patient patOld=PatCur.Copy();
						PatCur.PatStatus=PatientStatus.Prospective;
						Patients.Update(PatCur,patOld);
					}
				}
				else if(!opCur.SetProspective && PatCur.PatStatus==PatientStatus.Prospective) {
					//Do we need to warn about changing FROM prospective? Assume so for now.
					if(MsgBox.Show(this,MsgBoxButtons.OKCancel,"Patient's status will change from Prospective to Patient.")) {
						Patient patOld=PatCur.Copy();
						PatCur.PatStatus=PatientStatus.Patient;
						Patients.Update(PatCur,patOld);
					}
				}
			}
			if(aptCur.AptStatus==ApptStatus.Broken) {
				aptCur.AptStatus=ApptStatus.Scheduled;
			}
			if(aptCur.AptStatus==ApptStatus.UnschedList) {
				aptCur.AptStatus=ApptStatus.Scheduled;
			}
			//original position of provider settings
			if(curOp.ClinicNum==0){
				aptCur.ClinicNum=PatCur.ClinicNum;
			}
			else{
				aptCur.ClinicNum=curOp.ClinicNum;
			}
			bool isCreate=false;
			if(aptCur.AptStatus==ApptStatus.Planned) {//if Planned appt is on pinboard
				long plannedAptNum=aptCur.AptNum;
				LabCase lab=LabCases.GetForPlanned(aptCur.AptNum);
				aptCur.NextAptNum=aptCur.AptNum;
				aptCur.AptStatus=ApptStatus.Scheduled;
				aptCur.TimeLocked=PrefC.GetBool(PrefName.AppointmentTimeIsLocked);
				try {
					Appointments.Insert(aptCur);//now, aptnum is different.
					for(int i=0;i<pinBoard.SelectedAppt.TableApptFields.Rows.Count;i++) {//Duplicate the appointment fields.
						if(aptOld.AptNum==PIn.Long(pinBoard.SelectedAppt.TableApptFields.Rows[i]["AptNum"].ToString())) {
							ApptField apptField = new ApptField();
							apptField.AptNum=aptCur.AptNum;
							apptField.FieldName=PIn.String(pinBoard.SelectedAppt.TableApptFields.Rows[i]["FieldName"].ToString());
							apptField.FieldValue=PIn.String(pinBoard.SelectedAppt.TableApptFields.Rows[i]["FieldValue"].ToString());
							ApptFields.Insert(apptField);
						}
					}
					//for(int i=0;i<pinBoard.SelectedAppt.TablePatFields.Rows.Count;i++) {//Duplicate the patient fields.
					//	PatField patField=new PatField();
					//	patField.PatNum=aptCur.PatNum;
					//	patField.FieldName=PIn.String(pinBoard.SelectedAppt.TablePatFields.Rows[i]["FieldName"].ToString());
					//	patField.FieldValue=PIn.String(pinBoard.SelectedAppt.TablePatFields.Rows[i]["FieldValue"].ToString());
					//	PatFields.Insert(patField);
					//}
				}
				catch(ApplicationException ex) {
					MessageBox.Show(ex.Message);
					return;
				}
				//If there is an existing HL7 def enabled, send a SIU message if there is an outbound SIU message defined
				if(HL7Defs.IsExistingHL7Enabled()) {
					//S12 - New Appt Booking event
					MessageHL7 messageHL7=MessageConstructor.GenerateSIU(PatCur,Patients.GetPat(PatCur.Guarantor),EventTypeHL7.S12,aptCur);
					//Will be null if there is no outbound SIU message defined, so do nothing
					if(messageHL7!=null) {
						HL7Msg hl7Msg=new HL7Msg();
						hl7Msg.AptNum=aptCur.AptNum;
						hl7Msg.HL7Status=HL7MessageStatus.OutPending;//it will be marked outSent by the HL7 service.
						hl7Msg.MsgText=messageHL7.ToString();
						hl7Msg.PatNum=PatCur.PatNum;
						HL7Msgs.Insert(hl7Msg);
#if DEBUG
						MessageBox.Show(this,messageHL7.ToString());
#endif
					}
				}
				List<Procedure> ProcList=Procedures.Refresh(aptCur.PatNum);
				bool procAlreadyAttached=false;
				for(int i=0;i<ProcList.Count;i++) {
					if(ProcList[i].PlannedAptNum==plannedAptNum) {//if on the planned apt
						if(ProcList[i].AptNum>0) {//already attached to another appt
							procAlreadyAttached=true;
						}
						else {//only update procedures not already attached to another apt
							Procedures.UpdateAptNum(ProcList[i].ProcNum,aptCur.AptNum);
							//.Update(ProcCur,ProcOld);//recall synch not required.
						}
					}
				}
				if(procAlreadyAttached) {
					MessageBox.Show(Lan.g(this,"One or more procedures could not be scheduled because they were already attached to another appointment. Someone probably forgot to update the Next appointment in the Chart module."));
					FormApptEdit formAE=new FormApptEdit(aptCur.AptNum);
					CheckStatus();
					formAE.IsNew=true;
					formAE.ShowDialog();//to force refresh of aptDescript
					if(formAE.DialogResult!=DialogResult.OK) {//apt gets deleted from within aptEdit window.
						TempApptSingle.Dispose();
						RefreshModuleScreenPatient();
						RefreshPeriod();
						mouseIsDown = false;
						boolAptMoved=false;
						return;
					}
				}
				else {
					SecurityLogs.MakeLogEntry(Permissions.AppointmentCreate,aptCur.PatNum,
						aptCur.AptDateTime.ToString()+", "+aptCur.ProcDescript,
						aptCur.AptNum);
				}
				if(lab!=null) {
					LabCases.AttachToAppt(lab.LabCaseNum,aptCur.AptNum);
				}
				isCreate=true;
			}//if planned appointment is on pinboard
			else {//simple drag off pinboard to a new date/time
				aptCur.Confirmed=DefC.Short[(int)DefCat.ApptConfirmed][0].DefNum;//Causes the confirmation status to be reset.
				try {
					Appointments.Update(aptCur,aptOld);
					if(aptOld.AptStatus==ApptStatus.UnschedList && aptOld.AptDateTime==DateTime.MinValue) { //If new appt is being added to schedule from pinboard
						SecurityLogs.MakeLogEntry(Permissions.AppointmentCreate,aptCur.PatNum,
							aptCur.AptDateTime.ToString()+", "+aptCur.ProcDescript,
							aptCur.AptNum);
						isCreate=true;
					}
					else { //If existing appt is being moved
						SecurityLogs.MakeLogEntry(Permissions.AppointmentMove,aptCur.PatNum,
							aptCur.ProcDescript+", from "+aptOld.AptDateTime.ToString()+", to "+aptCur.AptDateTime.ToString(),
							aptCur.AptNum);
					}
					if(aptCur.Confirmed!=aptOld.Confirmed) {
						//Log confirmation status changes.
						SecurityLogs.MakeLogEntry(Permissions.ApptConfirmStatusEdit,aptCur.PatNum,
							Lans.g(this,"Appointment confirmation status automatically changed from")+" "
							+DefC.GetName(DefCat.ApptConfirmed,aptOld.Confirmed)+" "+Lans.g(this,"to")+" "+DefC.GetName(DefCat.ApptConfirmed,aptCur.Confirmed)
							+Lans.g(this,"from the appointment module")+".",aptCur.AptNum);
					}
					//If there is an existing HL7 def enabled, send a SIU message if there is an outbound SIU message defined
					if(HL7Defs.IsExistingHL7Enabled()) {
						//S12 - New Appt Booking event, S13 - Appt Rescheduling
						MessageHL7 messageHL7=null;
						if(isCreate) {
							messageHL7=MessageConstructor.GenerateSIU(PatCur,Patients.GetPat(PatCur.Guarantor),EventTypeHL7.S12,aptCur);
						}
						else {
							messageHL7=MessageConstructor.GenerateSIU(PatCur,Patients.GetPat(PatCur.Guarantor),EventTypeHL7.S13,aptCur);
						}
						//Will be null if there is no outbound SIU message defined, so do nothing
						if(messageHL7!=null) {
							HL7Msg hl7Msg=new HL7Msg();
							hl7Msg.AptNum=aptCur.AptNum;
							hl7Msg.HL7Status=HL7MessageStatus.OutPending;//it will be marked outSent by the HL7 service.
							hl7Msg.MsgText=messageHL7.ToString();
							hl7Msg.PatNum=PatCur.PatNum;
							HL7Msgs.Insert(hl7Msg);
#if DEBUG
							MessageBox.Show(this,messageHL7.ToString());
#endif
						}
					}
				}
				catch(ApplicationException ex) {
					MessageBox.Show(ex.Message);
					return;
				}
			}
			if(procsForSingleApt==null) {
				procsForSingleApt=Procedures.GetProcsForSingle(aptCur.AptNum,false);
			}
			Procedures.SetProvidersInAppointment(aptCur,procsForSingleApt);
			TempApptSingle.Dispose();
			pinBoard.ClearSelected();
			//PinApptSingle.Visible=false;
			//ContrApptSingle.PinBoardIsSelected=false;
			ContrApptSingle.SelectedAptNum=aptCur.AptNum;
			RefreshModuleScreenPatient();
			//RefreshModulePatient(PatCurNum);
			RefreshPeriod();//date moving to for this computer; This line may not be needed
			//SetInvalid();//date moving to for other computers
			//AppointmentL.DateSelected=fromDate;
			//SetInvalid();//for date moved from for other computers.
			AppointmentL.DateSelected=aptCur.AptDateTime;
			Signalods.SetInvalidAppt(aptCur,aptOld);
			mouseIsDown = false;
			boolAptMoved=false;
			List<string> procCodes=new List<string>();
			for(int i=0;i<procsForSingleApt.Count;i++) {
				procCodes.Add(ProcedureCodes.GetProcCode((long)procsForSingleApt[i].CodeNum).ProcCode);
			}
			if(isCreate) {//new appointment (not planned) is being added to the schedule from the pinboard, trigger ScheduleProcedure automation
				AutomationL.Trigger(AutomationTrigger.ScheduleProcedure,procCodes,aptCur.PatNum);
			}
			//Recalls.SynchScheduledApptLazy(aptCur.PatNum, aptCur.AptDateTime, procCodes);
			Recalls.SynchScheduledApptFull(aptCur.PatNum);
		}

		///<summary>Copied from FormApptsOther. Does not limit appointment creation, only warns user. This check should be run before creating a new appointment. </summary>
		private void CheckStatus() {
			if(PatCur.PatStatus == PatientStatus.Inactive
				|| PatCur.PatStatus == PatientStatus.Archived
				|| PatCur.PatStatus == PatientStatus.Prospective) {
				MsgBox.Show(this,"Warning. Patient is not active.");
			}
			if(PatCur.PatStatus == PatientStatus.Deceased) {
				MsgBox.Show(this,"Warning. Patient is deceased.");
			}
		}

		///<summary>Checks if the appointment's start time overlaps another appt.  Tests all appts for the day, even if not visible.  Call RefreshPeriod before calling this.</summary>
		private bool HasValidStartTime(Appointment aptCur) {
			DateTime aptDateTime;
			for(int i=0;i<DS.Tables["Appointments"].Rows.Count;i++) {
				if(DS.Tables["Appointments"].Rows[i]["AptNum"].ToString()==aptCur.AptNum.ToString()) {
					continue;
				}
				if(DS.Tables["Appointments"].Rows[i]["Op"].ToString()!=aptCur.Op.ToString()) {
					continue;
				}
				aptDateTime=PIn.DateT(DS.Tables["Appointments"].Rows[i]["AptDateTime"].ToString());
				if(aptDateTime.Date!=aptCur.AptDateTime.Date) {
					continue;
				}
				//tests start time
				if(aptCur.AptDateTime.TimeOfDay >= aptDateTime.TimeOfDay
					&& aptCur.AptDateTime.TimeOfDay < aptDateTime.TimeOfDay.Add(TimeSpan.FromMinutes(
					DS.Tables["Appointments"].Rows[i]["Pattern"].ToString().Length*5))) 
				{
					return false;
				}
			}
			return true;
		}

		///<summary>Called when releasing an appointment to make sure it does not overlap any other appointment.  Tests all appts for the day, even if not visible.  Recommended to RefreshPeriod before calling this.</summary>
		private bool DoesOverlap(Appointment aptCur) {
			DateTime aptDateTime;
			for(int i=0;i<DS.Tables["Appointments"].Rows.Count;i++) {
				if(DS.Tables["Appointments"].Rows[i]["AptNum"].ToString()==aptCur.AptNum.ToString()) {
					continue;
				}
				if(DS.Tables["Appointments"].Rows[i]["Op"].ToString()!=aptCur.Op.ToString()) {
					continue;
				}
				aptDateTime=PIn.DateT(DS.Tables["Appointments"].Rows[i]["AptDateTime"].ToString());
				if(aptDateTime.Date!=aptCur.AptDateTime.Date) {
					continue;
				}
				//tests start time
				if(aptCur.AptDateTime.TimeOfDay >= aptDateTime.TimeOfDay
					&& aptCur.AptDateTime.TimeOfDay < aptDateTime.TimeOfDay.Add(TimeSpan.FromMinutes(
					DS.Tables["Appointments"].Rows[i]["Pattern"].ToString().Length*5))) {
					//Debug.WriteLine(TimeSpan.FromMinutes(ListDay[i].Pattern.Length*5).ToString());
					return true;
				}
				//tests stop time
				if(aptCur.AptDateTime.TimeOfDay.Add(TimeSpan.FromMinutes(aptCur.Pattern.Length*5)) > aptDateTime.TimeOfDay
					&& aptCur.AptDateTime.TimeOfDay.Add(TimeSpan.FromMinutes(aptCur.Pattern.Length*5))
					<= aptDateTime.TimeOfDay.Add(TimeSpan.FromMinutes(DS.Tables["Appointments"].Rows[i]["Pattern"].ToString().Length*5))) {
					return true;
				}
				//tests engulf
				if(aptCur.AptDateTime.TimeOfDay <= aptDateTime.TimeOfDay
					&& aptCur.AptDateTime.TimeOfDay.Add(TimeSpan.FromMinutes(aptCur.Pattern.Length*5))
					>= aptDateTime.TimeOfDay.Add(TimeSpan.FromMinutes(DS.Tables["Appointments"].Rows[i]["Pattern"].ToString().Length*5))) {
					return true;
				}
			}
			return false;
		}

		///<summary>Returns the apptNum of the appointment at these coordinates, or 0 if none.  This is new code which is going to replace some of the outdated code on this page.</summary>
		private long HitTestAppt(Point point) {
			if(ApptDrawing.VisOps.Count==0) {//no ops visible.
				return 0;
			}
			int day=ApptDrawing.XPosToDay(point.X);
			if(ApptDrawing.IsWeeklyView) {
				//this is a workaround because we start on Monday:
				if(day==6) {
					day=0;
				}
				else {
					day=day+1;
				}
			}
			DateTime startDate=(ApptDrawing.IsWeeklyView?WeekStartDate:AppointmentL.DateSelected);
			DateTime endDate=(ApptDrawing.IsWeeklyView?WeekEndDate:AppointmentL.DateSelected);
			//if operatories were just hidden and VisOps is mismatched with ListShort
			int xOp=ApptDrawing.XPosToOpIdx(point.X);
			if(xOp>ApptDrawing.VisOps.Count-1) {
				return 0;
			}
			long op=ApptDrawing.VisOps[xOp].OperatoryNum;
			int hour=ApptDrawing.YPosToHour(point.Y);
			int minute=ApptDrawing.YPosToMin(point.Y);
			TimeSpan time=new TimeSpan(hour,minute,0);
			TimeSpan aptTime;
			int aptDayOfWeek;
			for(int i=0;i<DS.Tables["Appointments"].Rows.Count;i++) {
				DataRow row = DS.Tables["Appointments"].Rows[i];
				if(op.ToString()!=row["Op"].ToString()) {
					continue;
				}
				aptDayOfWeek=(int)PIn.DateT(row["AptDateTime"].ToString()).DayOfWeek;
				if(ApptDrawing.IsWeeklyView && aptDayOfWeek!=day) {
					continue;
				}
				if(PIn.Date(row["AptDateTime"].ToString()).Date<startDate.Date || PIn.Date(row["AptDateTime"].ToString()).Date>endDate.Date){
					continue;//Appointment is outside of our date range.
				}
				aptTime=PIn.DateT(row["AptDateTime"].ToString()).TimeOfDay;
				if(aptTime <= time
					&& time < aptTime+TimeSpan.FromMinutes(row["Pattern"].ToString().Length*5)) {
					return PIn.Long(row["AptNum"].ToString());
				}
			}
			return 0;
		}

		///<summary>If the given point is in the bottom few pixels of an appointment, then this returns true.  Use HitTestAppt to figure out which appointment.</summary>
		private bool HitTestApptBottom(Point point) {
			long aptnum=HitTestAppt(point);
			if(aptnum==0) {
				return false;
			}
			//get the appointment control in order to measure
			ContrApptSingle apptSing=null;
			for(int i=0;i<DS.Tables["Appointments"].Rows.Count;i++) {
				//ListDay.Length;i++) {
				if(aptnum.ToString()==DS.Tables["Appointments"].Rows[i]["AptNum"].ToString()) {
					apptSing=ContrApptSingle3[i];
				}
			}
			//find the bottom border of the appointment
			int bottom=apptSing.Bottom;
			if(point.Y>bottom-8) {
				return true;
			}
			return false;
		}

		///<summary>Mouse down event anywhere on the sheet.  Could be a blank space or on an actual appointment.</summary>
		private void ContrApptSheet2_MouseDown(object sender,System.Windows.Forms.MouseEventArgs e) {
			if(Plugins.HookMethod(this,"ContrApptSheet2_MouseDown_start",ContrApptSingle.ClickedAptNum,e)) {
				return;
			}
			if(infoBubble.Visible) {
				infoBubble.Visible=false;
			}
			if(ApptDrawing.VisOps.Count==0) {//no ops visible.
				return;
			}
			if(mouseIsDown) {//if user clicks right mouse button while dragging
				return;
			}
			//some of this is a little redundant, but still necessary for now.
			SheetClickedonHour=ApptDrawing.YPosToHour(e.Y);
			SheetClickedonMin=ApptDrawing.YPosToMin(e.Y);
			TimeSpan sheetClickedOnTime=new TimeSpan(SheetClickedonHour,SheetClickedonMin,0);
			ContrApptSingle.ClickedAptNum=HitTestAppt(e.Location);
			SheetClickedonOp=ApptDrawing.VisOps[ApptDrawing.XPosToOpIdx(e.X)].OperatoryNum;
			SheetClickedonDay=ApptDrawing.XPosToDay(e.X);
			if(!ApptDrawing.IsWeeklyView) {
				//if Sunday is selected, we want to go to forward to the Sunday after the first day of the week (always Monday) not the Sunday before
				//used to determine if a blockout from a list of blockouts is on the day selected
				//this value will be added to the first day of the week, always Monday, to get the day selected
				//Example: if user clicks on Wednesday, (int)Wednesday=3, (int)Monday=1, SheetClickedonDay=3-1=2, Monday.AddDays(SheetClickedonDay)=Wednesday
				//Example: if user clicks on Sunday, (int)Sunday=0, SheetClickedonDay=6, Monday.AddDays(SheetClickedonDay)=the Sunday following the start of the week Monday
				if(AppointmentL.DateSelected.DayOfWeek==DayOfWeek.Sunday) {
					SheetClickedonDay=6;
				}
				else {
					SheetClickedonDay=((int)AppointmentL.DateSelected.DayOfWeek)-1;
				}
			}
			long prevSelectedAptNum=ContrApptSingle.SelectedAptNum;
			Graphics grfx=ContrApptSheet2.CreateGraphics();
			//if clicked on an appt-----------------------------------------------------------------------------------------------
			if(ContrApptSingle.ClickedAptNum!=0) {
				if(e.Button==MouseButtons.Left) {
					mouseIsDown = true;
				}
				int thisIndex=GetIndex(ContrApptSingle.ClickedAptNum);
				pinBoard.SelectedIndex=-1;
				//ContrApptSingle.PinBoardIsSelected=false;
				if(ContrApptSingle.SelectedAptNum!=-1//unselects previously selected unless it's the same appt
					&& ContrApptSingle.SelectedAptNum!=ContrApptSingle.ClickedAptNum) {
					int prevSel=GetIndex(ContrApptSingle.SelectedAptNum);
					//has to be done before refresh prev:
					ContrApptSingle.SelectedAptNum=ContrApptSingle.ClickedAptNum;
					if(prevSel!=-1) {
						ContrApptSingle3[prevSel].CreateShadow();
						if(ContrApptSingle3[prevSel].Shadow!=null) {
							grfx.DrawImage(ContrApptSingle3[prevSel].Shadow,ContrApptSingle3[prevSel].Location.X
								,ContrApptSingle3[prevSel].Location.Y);
						}
					}
				}
				//again, in case missed in loop above:
				ContrApptSingle.SelectedAptNum=ContrApptSingle.ClickedAptNum;
				ContrApptSingle3[thisIndex].CreateShadow();
				if(ContrApptSingle3[thisIndex].Shadow!=null) {
					grfx.DrawImage(ContrApptSingle3[thisIndex].Shadow,ContrApptSingle3[thisIndex].Location.X
								,ContrApptSingle3[thisIndex].Location.Y);
				}
				long oldPatNum=(PatCur==null ? 0 : PatCur.PatNum);
				RefreshModuleDataPatient(PIn.Long(ContrApptSingle3[thisIndex].DataRoww["PatNum"].ToString()));
				if(PatCur.PatNum!=oldPatNum) {
					FormOpenDental.S_Contr_PatientSelected(PatCur,true,false);
				}
				else {
					RefreshModuleScreenPatient();
				}
				if(e.Button==MouseButtons.Right) {
					menuApt.MenuItems.RemoveByKey("Phone Div");
					menuApt.MenuItems.RemoveByKey("Home Phone");
					menuApt.MenuItems.RemoveByKey("Work Phone");
					menuApt.MenuItems.RemoveByKey("Wireless Phone");
					menuApt.MenuItems.RemoveByKey("Text Div");
					menuApt.MenuItems.RemoveByKey("Send Text");
					menuApt.MenuItems.RemoveByKey("Send Confirmation Text");
					menuApt.MenuItems.RemoveByKey("Ortho Chart");
					if(PrefC.GetBool(PrefName.ApptModuleShowOrthoChartItem)) {
						menuApt.MenuItems.Add(Lan.g(this,"Go To")+" "+OrthoChartTabs.Listt[0].TabName,new EventHandler(menuApt_Click));
						menuApt.MenuItems[menuApt.MenuItems.Count-1].Name="Ortho Chart";
					}
					//Phone numbers
					if(!String.IsNullOrEmpty(PatCur.HmPhone)||!String.IsNullOrEmpty(PatCur.WkPhone)||!String.IsNullOrEmpty(PatCur.WirelessPhone)) {
						menuApt.MenuItems.Add("-");
						menuApt.MenuItems[menuApt.MenuItems.Count-1].Name="Phone Div";
					}
					if(!String.IsNullOrEmpty(PatCur.HmPhone)) {
						menuApt.MenuItems.Add(Lan.g(this,"Call Home Phone")+" "+PatCur.HmPhone,new EventHandler(menuApt_Click));
						menuApt.MenuItems[menuApt.MenuItems.Count-1].Name="Home Phone";
					}
					if(!String.IsNullOrEmpty(PatCur.WkPhone)) {
						menuApt.MenuItems.Add(Lan.g(this,"Call Work Phone")+" "+PatCur.WkPhone,new EventHandler(menuApt_Click));
						menuApt.MenuItems[menuApt.MenuItems.Count-1].Name="Work Phone";
					}
					if(!String.IsNullOrEmpty(PatCur.WirelessPhone)) {
						menuApt.MenuItems.Add(Lan.g(this,"Call Wireless Phone")+" "+PatCur.WirelessPhone,new EventHandler(menuApt_Click));
						menuApt.MenuItems[menuApt.MenuItems.Count-1].Name="Wireless Phone";
					}
					//Texting
					menuApt.MenuItems.Add("-");
					menuApt.MenuItems[menuApt.MenuItems.Count-1].Name="Text Div";
					menuApt.MenuItems.Add(Lan.g(this,"Send Text"),menuApt_Click);
					menuApt.MenuItems[menuApt.MenuItems.Count-1].Name="Send Text";
					if(!SmsPhones.IsIntegratedTextingEnabled() && !Programs.IsEnabled(ProgramName.CallFire)) {
						menuApt.MenuItems[menuApt.MenuItems.Count-1].Enabled=false;
					}
					menuApt.MenuItems.Add(Lan.g(this,"Send Confirmation Text"),menuApt_Click);
					menuApt.MenuItems[menuApt.MenuItems.Count-1].Name="Send Confirmation Text";
					if(!SmsPhones.IsIntegratedTextingEnabled() && !Programs.IsEnabled(ProgramName.CallFire)) {
						menuApt.MenuItems[menuApt.MenuItems.Count-1].Enabled=false;
					}
					//menuApt.MenuItems.Add(Lan.g(this,"Send Reminder Text"),menuApt_Click);
					//if(!SmsPhones.IsIntegratedTextingEnabled() && !Programs.IsEnabled(ProgramName.CallFire)) {
					//	menuApt.MenuItems[menuApt.MenuItems.Count-1].Enabled=false;
					//}

					menuApt.Show(ContrApptSheet2,new Point(e.X,e.Y));
				}
				else {
					TempApptSingle=new ContrApptSingle();
					TempApptSingle.Visible=false;//otherwise I get a phantom appt while holding mouse down
					TempApptSingle.DataRoww=ContrApptSingle3[thisIndex].DataRoww;
					TempApptSingle.TableApptFields=ContrApptSingle3[thisIndex].TableApptFields;
					TempApptSingle.TablePatFields=ContrApptSingle3[thisIndex].TablePatFields;
					Controls.Add(TempApptSingle);
					TempApptSingle.Location=ApptSingleDrawing.SetLocation(TempApptSingle.DataRoww,0,ApptDrawing.VisOps.Count,0);
					TempApptSingle.Size=ApptSingleDrawing.SetSize(TempApptSingle.DataRoww);
					TempApptSingle.PatternShowing=ApptSingleDrawing.GetPatternShowing(TempApptSingle.DataRoww["Pattern"].ToString());
					TempApptSingle.BringToFront();
					//mouseOrigin is in ApptSheet coordinates
					mouseOrigin=e.Location;
					contOrigin=ContrApptSingle3[thisIndex].Location;
					TempApptSingle.CreateShadow();
					if(HitTestApptBottom(e.Location)) {
						ResizingAppt=true;
						ResizingOrigH=TempApptSingle.Height;
					}
					else {
						ResizingAppt=false;
					}
				}
			}
			//not clicked on appt---------------------------------------------------------------------------------------------------
			else {
				if(e.Button==MouseButtons.Right) {
					int clickedOnBlockCount=0;
					Schedule[] ListForType=Schedules.GetForType(SchedListPeriod,ScheduleType.Blockout,0);
					//List<ScheduleOp> listForSched;
					for(int i=0;i<ListForType.Length;i++) {
						if(ListForType[i].SchedDate.Date!=WeekStartDate.AddDays(SheetClickedonDay).Date) {
							continue;
						}
						if(ListForType[i].StartTime > sheetClickedOnTime
							|| ListForType[i].StopTime <= sheetClickedOnTime) {
							continue;
						}
						//listForSched=ScheduleOps.GetForSched(ListForType[i].ScheduleNum);
						for(int p=0;p<ListForType[i].Ops.Count;p++) {
							if(ListForType[i].Ops[p]==SheetClickedonOp) {
								clickedOnBlockCount++;
								break;//out of ops loop
							}
						}
					}
					if(clickedOnBlockCount>0) {
						menuBlockout.MenuItems[0].Enabled=true;//Edit
						menuBlockout.MenuItems[1].Enabled=true;//Cut
						menuBlockout.MenuItems[2].Enabled=true;//Clone
						menuBlockout.MenuItems[3].Enabled=false;//paste. Can't paste on top of an existing blockout
						menuBlockout.MenuItems[4].Enabled=true;//Delete
						if(clickedOnBlockCount>1) {
						  MsgBox.Show(this,"There are multiple blockouts in this slot.  You should try to delete or move one of them.");
						}
					}
					else {
						menuBlockout.MenuItems[0].Enabled=false;//edit
						menuBlockout.MenuItems[1].Enabled=false;//edit
						menuBlockout.MenuItems[2].Enabled=false;//copy
						if(BlockoutClipboard==null) {
							menuBlockout.MenuItems[3].Enabled=false;//paste
						}
						else {
							menuBlockout.MenuItems[3].Enabled=true;
						}
						menuBlockout.MenuItems[4].Enabled=false;//delete
					}
					menuBlockout.Show(ContrApptSheet2,new Point(e.X,e.Y));
				}
			}
			grfx.Dispose();
			pinBoard.Invalidate();
			//if(PinApptSingle.Visible){
			//	PinApptSingle.CreateShadow();
			//	PinApptSingle.Refresh();
			//}
			CreateAptShadowsOnMain(new List<long> { ContrApptSingle.SelectedAptNum,prevSelectedAptNum });
		}

		///<summary></summary>
		private void ContrApptSheet2_MouseMove(object sender,System.Windows.Forms.MouseEventArgs e) {
			if(!mouseIsDown) {
				InfoBubbleDraw(e.Location);
				//decide what the pointer should look like.
				if(HitTestApptBottom(e.Location)) {
					Cursor=Cursors.SizeNS;
				}
				else {
					Cursor=Cursors.Default;
				}
				return;
			}
			//if resizing an appointment
			if(ResizingAppt) {
				TempApptSingle.Location=new Point(
					contOrigin.X+ContrApptSheet2.Location.X+panelSheet.Location.X+1,//the 1 is an unknown factor
					contOrigin.Y+ContrApptSheet2.Location.Y+panelSheet.Location.Y+1);//ditto
				TempApptSingle.Height=ResizingOrigH+e.Y-mouseOrigin.Y;
				if(TempApptSingle.Height<4) {//unhandled exception if smaller.
					TempApptSingle.Height=4;
				}
				TempApptSingle.CreateShadow();
				TempApptSingle.Visible=true;
				return;
			}
			//dragging an appointment
			//if(ApptDrawing.IsWeeklyView){
			//	boolAptMoved=false;
			//	return;
			//}
			int thisIndex=GetIndex(ContrApptSingle.SelectedAptNum);
			if((Math.Abs(e.X-mouseOrigin.X)<3)//enhances double clicking
				&(Math.Abs(e.Y-mouseOrigin.Y)<3)) {
				boolAptMoved=false;
				return;
			}
			boolAptMoved=true;
			TempApptSingle.Location=new Point(
				contOrigin.X+e.X-mouseOrigin.X+ContrApptSheet2.Location.X+panelSheet.Location.X,
				contOrigin.Y+e.Y-mouseOrigin.Y+ContrApptSheet2.Location.Y+panelSheet.Location.Y);
			TempApptSingle.Visible=true;
		}

		///<summary>Used by parent form when a dialog needs to be displayed on the mouse down.</summary>
		public void MouseUpForced() {
			if(pinBoard.SelectedIndex!=-1) {
				CancelPinMouseDown=true;
			}
			if(!mouseIsDown) {
				return;
			}
			mouseIsDown=false;
			if(TempApptSingle!=null) {
				TempApptSingle.Dispose();
				TempApptSingle=null;
			}
		}

		///<summary>Usually dropping an appointment to a new location.</summary>
		private void ContrApptSheet2_MouseUp(object sender,System.Windows.Forms.MouseEventArgs e) {
			if(!mouseIsDown) {
				return;
			}
			/*if(ApptDrawing.IsWeeklyView) {
				ResizingAppt=false;
				mouseIsDown=false;
				TempApptSingle.Dispose();
				return;
			}*/
			int thisIndex=GetIndex(ContrApptSingle.SelectedAptNum);
			Appointment aptOld;
			//Resizing-------------------------------------------------------------------------------------------------------------
			if(ResizingAppt) {
				if(!TempApptSingle.Visible) {//click with no drag
					ResizingAppt=false;
					mouseIsDown=false;
					TempApptSingle.Dispose();
					return;
				}
				//convert Bottom to a time
				int hr=ApptDrawing.ConvertToHour
					(TempApptSingle.Bottom-ContrApptSheet2.Location.Y-panelSheet.Location.Y);
				int minute=ApptDrawing.ConvertToMin
					(TempApptSingle.Bottom-ContrApptSheet2.Location.Y-panelSheet.Location.Y);
				TimeSpan bottomSpan=new TimeSpan(hr,minute,0);
				//subtract to get the new length of appt
				TimeSpan newspan=bottomSpan-PIn.DateT(TempApptSingle.DataRoww["AptDateTime"].ToString()).TimeOfDay;
				int newpatternL=(int)newspan.TotalMinutes/5;
				if(newpatternL < ApptDrawing.MinPerIncr/5) {//eg. if 1 < 10/5, would make appt too short. 
					newpatternL=ApptDrawing.MinPerIncr/5;//sets new pattern length at one increment, typically 2 or 3 5min blocks
				}
				else if(newpatternL>78) {//max length of 390 minutes
					newpatternL=78;
				}
				string pattern=TempApptSingle.DataRoww["Pattern"].ToString();
				if(newpatternL<pattern.Length) {//shorten to match new pattern length
					pattern=pattern.Substring(0,newpatternL);
				}
				else if(newpatternL>pattern.Length) {//make pattern longer.
					pattern=pattern.PadRight(newpatternL,'/');
				}
				//Now, check for overlap with other appts.
				DateTime aptDateTime;
				DateTime aptDateTimeCur=PIn.DateT(TempApptSingle.DataRoww["AptDateTime"].ToString());
				for(int i=0;i<DS.Tables["Appointments"].Rows.Count;i++) {
					if(DS.Tables["Appointments"].Rows[i]["AptNum"].ToString()==TempApptSingle.DataRoww["AptNum"].ToString()) {
						continue;
					}
					if(DS.Tables["Appointments"].Rows[i]["Op"].ToString()!=TempApptSingle.DataRoww["Op"].ToString()) {
						continue;
					}
					aptDateTime=PIn.DateT(DS.Tables["Appointments"].Rows[i]["AptDateTime"].ToString());
					if(ApptDrawing.IsWeeklyView && aptDateTime.Date!=aptDateTimeCur.Date) {
						continue;
					}
					if(aptDateTime<aptDateTimeCur) {
						continue;//we don't care about appointments that are earlier than this one
					}
					if(aptDateTime.TimeOfDay < aptDateTimeCur.TimeOfDay+TimeSpan.FromMinutes(5*pattern.Length)) {
						newspan=aptDateTime.TimeOfDay-aptDateTimeCur.TimeOfDay;
						newpatternL=(int)newspan.TotalMinutes/5;
						pattern=pattern.Substring(0,newpatternL);
					}
				}
				if(pattern=="") {
					pattern="///";
				}
				Appointments.SetPattern(PIn.Long(TempApptSingle.DataRoww["AptNum"].ToString()),pattern);
				SecurityLogs.MakeLogEntry(Permissions.AppointmentEdit,PatCur.PatNum,Lan.g(this,"Appointment resized from the appointment module."),
					PIn.Long(TempApptSingle.DataRoww["AptNum"].ToString()));//Generate FKey to the appointment to show the audit entry in the ApptEdit window.
				ResizingAppt=false;
				mouseIsDown=false;
				RefreshModuleDataPatient(PatCur.PatNum);
				FormOpenDental.S_Contr_PatientSelected(PatCur,true,false);
				//RefreshModulePatient(PatCurNum);
				RefreshPeriod(true);
				TempApptSingle.Dispose();
				Signalods.SetInvalidAppt(Appointments.GetOneApt(PIn.Long(TempApptSingle.DataRoww["AptNum"].ToString())));
				return;
			}//end if(ResizingAppt)
			if((Math.Abs(e.X-mouseOrigin.X)<7)
				&&(Math.Abs(e.Y-mouseOrigin.Y)<7)) {
				boolAptMoved=false;
			}
			//it was a click with no drag-----------------------------------------------------------------------------------------
			if(!boolAptMoved) {
				mouseIsDown=false;
				TempApptSingle.Dispose();
				return;
			}
			//dragging to pinboard, so place a copy there---------------------------------------------------------------------------
			if(TempApptSingle.Location.X>ContrApptSheet2.Width) {
				if(!Security.IsAuthorized(Permissions.AppointmentMove)
					|| PatRestrictions.IsRestricted(PatCur.PatNum,PatRestrict.ApptSchedule))//PatCur is updated in ContrApptSheet2_MouseDown 
				{
					mouseIsDown=false;
					TempApptSingle.Dispose();
					return;
				}
				//cannot allow moving completed procedure because it could cause completed procs to change date.  Security must block this.
				if(TempApptSingle.DataRoww["AptStatus"].ToString()=="2") {//complete
					mouseIsDown=false;
					TempApptSingle.Dispose();
					MsgBox.Show(this,"Not allowed to move completed appointments.");
					return;
				}
				int prevSel=GetIndex(ContrApptSingle.SelectedAptNum);
				List<long> list=new List<long>();
				list.Add(ContrApptSingle.SelectedAptNum);
				SendToPinboard(list);//sets selectedAptNum=-1. do before refresh prev
				if(prevSel!=-1) {
					CreateAptShadowsOnMain();
					ContrApptSheet2.DrawShadow();
				}
				RefreshModuleDataPatient(PatCur.PatNum);
				FormOpenDental.S_Contr_PatientSelected(PatCur,true,false);
				//RefreshModulePatient(PatCurNum);
				TempApptSingle.Dispose();
				return;
			}
			//moving to a new location-----------------------------------------------------------------------------------------------
			Appointment apt=Appointments.GetOneApt(ContrApptSingle.SelectedAptNum);
			aptOld=apt.Copy();
			int tHr=ApptDrawing.ConvertToHour
				(TempApptSingle.Location.Y-ContrApptSheet2.Location.Y-panelSheet.Location.Y);
			int tMin=ApptDrawing.ConvertToMin
				(TempApptSingle.Location.Y-ContrApptSheet2.Location.Y-panelSheet.Location.Y);
			long opNum=ApptDrawing.VisOps[ApptDrawing.ConvertToOp(TempApptSingle.Location.X-ContrApptSheet2.Location.X)].OperatoryNum;
			bool timeWasMoved=tHr!=apt.AptDateTime.Hour
				|| tMin!=apt.AptDateTime.Minute;
			bool isOpChanged=true;
			if(opNum==apt.Op) {
				isOpChanged=false;
			}
			if(timeWasMoved || isOpChanged) {//no question for notes
				if(apt.AptStatus == ApptStatus.PtNote | apt.AptStatus == ApptStatus.PtNoteCompleted) {
					if(!Security.IsAuthorized(Permissions.AppointmentMove) || PatRestrictions.IsRestricted(apt.PatNum,PatRestrict.ApptSchedule)) {
						mouseIsDown = false;
						boolAptMoved = false;
						TempApptSingle.Dispose();
						return;
					}
				}
				else {
					if(!Security.IsAuthorized(Permissions.AppointmentMove)
						|| (apt.AptStatus==ApptStatus.Complete && (!Security.IsAuthorized(Permissions.AppointmentCompleteEdit)))
						|| PatRestrictions.IsRestricted(apt.PatNum,PatRestrict.ApptSchedule)
						|| !MsgBox.Show(this,true,"Move Appointment?"))
					{
						mouseIsDown = false;
						boolAptMoved = false;
						TempApptSingle.Dispose();
						return;
					}
				}
			}
			//convert loc to new date/time
			DateTime tDate=apt.AptDateTime.Date;
			if(ApptDrawing.IsWeeklyView) {
				tDate=WeekStartDate.AddDays(ApptDrawing.ConvertToDay(TempApptSingle.Location.X-ContrApptSheet2.Location.X));
			}
			apt.AptDateTime=new DateTime(tDate.Year,tDate.Month,tDate.Day,tHr,tMin,0);
			List<Procedure> procsMultApts=null;
			Procedure[] procsForOne=null;
			if(AppointmentRuleC.List.Length>0) {
				List<long> aptNums=new List<long>();
				for(int i=0;i<DS.Tables["Appointments"].Rows.Count;i++) {
					aptNums.Add(PIn.Long(DS.Tables["Appointments"].Rows[i]["AptNum"].ToString()));//ListDay[i].AptNum;
				}
				procsMultApts=Procedures.GetProcsMultApts(aptNums);
			}
			if(AppointmentRuleC.List.Length>0) {
				long[] aptNums=new long[DS.Tables["Appointments"].Rows.Count];
				for(int i=0;i<DS.Tables["Appointments"].Rows.Count;i++) {
					aptNums[i]=PIn.Long(DS.Tables["Appointments"].Rows[i]["AptNum"].ToString());//ListDay[i].AptNum;
				}
				procsForOne=Procedures.GetProcsOneApt(apt.AptNum,procsMultApts);
				ArrayList doubleBookedCodes=
					AppointmentL.GetDoubleBookedCodes(apt,DS.Tables["Appointments"].Copy(),procsMultApts,procsForOne);
				if(doubleBookedCodes.Count>0) {//if some codes would be double booked
					if(AppointmentRules.IsBlocked(doubleBookedCodes)) {
						MessageBox.Show(Lan.g(this,"Not allowed to double book:")+" "
							+AppointmentRules.GetBlockedDescription(doubleBookedCodes));
						mouseIsDown = false;
						boolAptMoved=false;
						TempApptSingle.Dispose();
						return;
					}
				}
			}
			Operatory curOp=ApptDrawing.VisOps[ApptDrawing.ConvertToOp(TempApptSingle.Location.X-ContrApptSheet2.Location.X)];
			apt.Op=curOp.OperatoryNum;
			//Set providers----------------------
			long assignedDent=Schedules.GetAssignedProvNumForSpot(SchedListPeriod,curOp,false,apt.AptDateTime);
			long assignedHyg=Schedules.GetAssignedProvNumForSpot(SchedListPeriod,curOp,true,apt.AptDateTime);
			List<Procedure> procsForSingleApt=null;
			if(apt.AptStatus!=ApptStatus.PtNote && apt.AptStatus!=ApptStatus.PtNoteCompleted) {
				if(timeWasMoved) {
					if(PatCur.AskToArriveEarly>0) {
						apt.DateTimeAskedToArrive=apt.AptDateTime.AddMinutes(-PatCur.AskToArriveEarly);
						MessageBox.Show(Lan.g(this,"Ask patient to arrive")+" "+PatCur.AskToArriveEarly
							+" "+Lan.g(this,"minutes early at")+" "+apt.DateTimeAskedToArrive.ToShortTimeString()+".");
					}
					else {
						if(apt.DateTimeAskedToArrive.Year>1880
							&& (aptOld.AptDateTime-aptOld.DateTimeAskedToArrive).TotalMinutes>0) 
						{
							apt.DateTimeAskedToArrive=apt.AptDateTime-(aptOld.AptDateTime-aptOld.DateTimeAskedToArrive);
							if(MessageBox.Show(Lan.g(this,"Ask patient to arrive")+" "+(aptOld.AptDateTime-aptOld.DateTimeAskedToArrive).TotalMinutes
								+" "+Lan.g(this,"minutes early at")+" "+apt.DateTimeAskedToArrive.ToShortTimeString()+"?","",MessageBoxButtons.YesNo)==DialogResult.No) {
									apt.DateTimeAskedToArrive=aptOld.DateTimeAskedToArrive;
							}
						}
						else {
							apt.DateTimeAskedToArrive=DateTime.MinValue;
						}
					}
				}
				//if no dentist/hygenist is assigned to spot, then keep the original dentist/hygenist without prompt.  All appts must have prov.
				if((assignedDent!=0 && assignedDent!=apt.ProvNum) || (assignedHyg!=0 && assignedHyg!=apt.ProvHyg)) {
					object[] parameters3={ apt,assignedDent,assignedHyg,procsForSingleApt,this };//Only used in following plugin hook.
					if((Plugins.HookMethod(this,"ContrAppt.ContrApptSheet2_MouseUp_apptProvChangeQuestion",parameters3))) {
						apt=(Appointment)parameters3[0];
						assignedDent=(long)parameters3[1];
						assignedDent=(long)parameters3[2];
						goto PluginApptProvChangeQuestionEnd;
					}
					if(MsgBox.Show(this,MsgBoxButtons.YesNo,"Change provider?")) {
						if(assignedDent!=0) {//the dentist will only be changed if the spot has a dentist.
							apt.ProvNum=assignedDent;
						}
						if(assignedHyg!=0 || PrefC.GetBool(PrefName.ApptSecondaryProviderConsiderOpOnly)) {
							apt.ProvHyg=assignedHyg;
						}
						if(curOp.IsHygiene) {
							apt.IsHygiene=true;
						}
						else {//op not marked as hygiene op
							if(assignedDent==0) {//no dentist assigned
								if(assignedHyg!=0) {//hyg is assigned (we don't really have to test for this)
									apt.IsHygiene=true;
								}
							}
							else {//dentist is assigned
								if(assignedHyg==0) {//hyg is not assigned
									apt.IsHygiene=false;
								}
								//if both dentist and hyg are assigned, it's tricky
								//only explicitly set it if user has a dentist assigned to the op
								if(curOp.ProvDentist!=0) {
									apt.IsHygiene=false;
								}
							}
						}
						procsForSingleApt=Procedures.GetProcsForSingle(apt.AptNum,false);
						List<long> codeNums=new List<long>();
						for(int p=0;p<procsForSingleApt.Count;p++) {
							codeNums.Add(procsForSingleApt[p].CodeNum);
						}
						string calcPattern=Appointments.CalculatePattern(apt.ProvNum,apt.ProvHyg,codeNums,true);
						if(apt.Pattern != calcPattern && !PrefC.GetBool(PrefName.AppointmentTimeIsLocked)) {
							if(apt.TimeLocked) {
								if(MsgBox.Show(this,MsgBoxButtons.YesNo,"Appointment length is locked.  Change length for new provider anyway?")) {
									apt.Pattern=calcPattern;
								}
							}
							else {//appt time not locked
								if(MsgBox.Show(this,MsgBoxButtons.YesNo,"Change length for new provider?")) {
									apt.Pattern=calcPattern;
								}
							}
						}
					}
				PluginApptProvChangeQuestionEnd:{}
				}
			}
			//Check in-memory dataset to look at overlap.
			if(DoesOverlap(apt)) {
				int startingOp=ApptDrawing.GetIndexOp(apt.Op);
				bool stillOverlaps=true;
				for(int i=startingOp;i<ApptDrawing.VisOps.Count;i++) {
					apt.Op=ApptDrawing.VisOps[i].OperatoryNum;
					if(!DoesOverlap(apt)) {
						stillOverlaps=false;
						break;
					}
				}
				if(stillOverlaps) {
					for(int i=startingOp;i>=0;i--) {
						apt.Op=ApptDrawing.VisOps[i].OperatoryNum;
						if(!DoesOverlap(apt)) {
							stillOverlaps=false;
							break;
						}
					}
				}
				if(stillOverlaps) {
					MessageBox.Show(Lan.g(this,"Appointment overlaps existing appointment."));
					mouseIsDown=false;
					boolAptMoved=false;
					TempApptSingle.Dispose();
					return;
				}
			}
			else {//Doesn't overlap, let's do a custom query to look at the operatory specifically
				List<Appointment> listAppts=Appointments.GetAppointmentsForOpsByPeriod(new List<long>() { apt.Op },apt.AptDateTime,apt.AptDateTime);
				foreach(Appointment aptInOp in listAppts) {
					if(aptInOp.AptNum==apt.AptNum) {
						continue;
					}
					DateTime apptScheduledEndTime=aptInOp.AptDateTime.Add(TimeSpan.FromMinutes(aptInOp.Pattern.Length*5));
					DateTime aptMovedEndTime=apt.AptDateTime.Add(TimeSpan.FromMinutes(apt.Pattern.Length*5));
					if((apt.AptDateTime >= aptInOp.AptDateTime && apt.AptDateTime < apptScheduledEndTime)//Apt we're moving's begin time is between appt's start and end times.
						|| (aptMovedEndTime > aptInOp.AptDateTime && aptMovedEndTime <=  apptScheduledEndTime))//Apt we're moving's end time is between appt's start and end times.
					{
						MsgBox.Show(this,"Appointment overlaps existing appointment.");
						mouseIsDown=false;
						boolAptMoved=false;
						TempApptSingle.Dispose();
						ModuleSelected(PatCur.PatNum);
						return;
					}
				}
			}
			Operatory opCur=Operatories.GetOperatory(apt.Op);
			Operatory opOld=Operatories.GetOperatory(aptOld.Op);
			if(opOld==null || opCur.SetProspective!=opOld.SetProspective) {
				if(opCur.SetProspective && PatCur.PatStatus!=PatientStatus.Prospective) { //Don't need to prompt if patient is already prospective.
					if(MsgBox.Show(this,MsgBoxButtons.OKCancel,"Patient's status will be set to Prospective.")) {
						Patient patOld=PatCur.Copy();
						PatCur.PatStatus=PatientStatus.Prospective;
						Patients.Update(PatCur,patOld);
					}
				}
				else if(!opCur.SetProspective && PatCur.PatStatus==PatientStatus.Prospective) {
					//Do we need to warn about changing FROM prospective? Assume so for now.
					if(MsgBox.Show(this,MsgBoxButtons.OKCancel,"Patient's status will change from Prospective to Patient.")) {
						Patient patOld=PatCur.Copy();
						PatCur.PatStatus=PatientStatus.Patient;
						Patients.Update(PatCur,patOld);
					}
				}
			}
			object[] parameters2 = { apt.AptDateTime,aptOld.AptDateTime,apt.AptStatus };
			if((Plugins.HookMethod(this,"ContrAppt.ContrApptSheet2_MouseUp_apptDoNotUnbreakApptSameDay",parameters2))) {
				apt.AptStatus = (ApptStatus)parameters2[2];
				goto PluginApptDoNotUnbreakApptSameDay;
			}
			if(apt.AptStatus==ApptStatus.Broken && (timeWasMoved || isOpChanged)) {
				apt.AptStatus=ApptStatus.Scheduled;
			}
			PluginApptDoNotUnbreakApptSameDay: { }
			//original location of provider code
			if(curOp.ClinicNum==0){
				apt.ClinicNum=PatCur.ClinicNum;
			}
			else{
				apt.ClinicNum=curOp.ClinicNum;
			}
			if(apt.AptDateTime!=aptOld.AptDateTime 
				&& apt.Confirmed!=DefC.Short[(int)DefCat.ApptConfirmed][0].DefNum 
				&& apt.AptDateTime.Date!=DateTime.Today) 
			{
				if(MsgBox.Show(this,MsgBoxButtons.YesNo,"Reset Confirmation Status?")) {
					apt.Confirmed=DefC.Short[(int)DefCat.ApptConfirmed][0].DefNum;//Causes the confirmation status to be reset.
				}
			}
			try {
				Appointments.Update(apt,aptOld);
			}
			catch(ApplicationException ex) {
				MessageBox.Show(ex.Message);
			}
			if(apt.Confirmed!=aptOld.Confirmed) {
				//Log confirmation status changes.
				SecurityLogs.MakeLogEntry(Permissions.ApptConfirmStatusEdit,apt.PatNum,
					Lans.g(this,"Appointment confirmation status changed from")+" "
					+DefC.GetName(DefCat.ApptConfirmed,aptOld.Confirmed)+" "+Lans.g(this,"to")+" "+DefC.GetName(DefCat.ApptConfirmed,apt.Confirmed)
					+Lans.g(this,"from the appointment module")+".",apt.AptNum);
			}
			if(apt.AptStatus!=ApptStatus.Complete) {
				if(procsForSingleApt==null) {
					procsForSingleApt=Procedures.GetProcsForSingle(apt.AptNum,false);
				}
				Procedures.SetProvidersInAppointment(apt,procsForSingleApt);
			}
			if(apt.AptStatus!=ApptStatus.Complete) { //seperate log entry for editing completed appointments
				SecurityLogs.MakeLogEntry(Permissions.AppointmentMove,apt.PatNum,
					apt.ProcDescript+" from "+aptOld.AptDateTime.ToString()+", to "+apt.AptDateTime.ToString(),
					apt.AptNum);
			}
			else {
				SecurityLogs.MakeLogEntry(Permissions.AppointmentCompleteEdit,apt.PatNum,
						"moved "+apt.ProcDescript+" from "+aptOld.AptDateTime.ToString()+", to "+apt.AptDateTime.ToString(),
						apt.AptNum);
			}
			//If there is an existing HL7 def enabled, send a SIU message if there is an outbound SIU message defined
			if(HL7Defs.IsExistingHL7Enabled()) {
				//S13 - Appt Rescheduling
				MessageHL7 messageHL7=MessageConstructor.GenerateSIU(PatCur,Patients.GetPat(PatCur.Guarantor),EventTypeHL7.S13,apt);
				//Will be null if there is no outbound SIU message defined, so do nothing
				if(messageHL7!=null) {
					HL7Msg hl7Msg=new HL7Msg();
					hl7Msg.AptNum=apt.AptNum;
					hl7Msg.HL7Status=HL7MessageStatus.OutPending;//it will be marked outSent by the HL7 service.
					hl7Msg.MsgText=messageHL7.ToString();
					hl7Msg.PatNum=PatCur.PatNum;
					HL7Msgs.Insert(hl7Msg);
#if DEBUG
					MessageBox.Show(this,messageHL7.ToString());
#endif
				}
			}
			RefreshModuleDataPatient(PatCur.PatNum);
			FormOpenDental.S_Contr_PatientSelected(PatCur,true,false);
			//RefreshModulePatient(PatCurNum);
			RefreshPeriod();
			mouseIsDown = false;
			boolAptMoved=false;
			List<string> procCodes=new List<string>();
			if(procsForSingleApt!=null) {
				for(int i=0;i<procsForSingleApt.Count;i++) {
					procCodes.Add(ProcedureCodes.GetProcCode((long)procsForSingleApt[i].CodeNum).ProcCode);
				}
			}
			//Recalls.SynchScheduledApptLazy(apt.PatNum,apt.AptDateTime,procCodes);
			Recalls.SynchScheduledApptFull(apt.PatNum);
			TempApptSingle.Dispose();
			Signalods.SetInvalidAppt(apt,aptOld);
		}

		private void ContrApptSheet2_MouseLeave(object sender,EventArgs e) {
			InfoBubbleDraw(new Point(-1,-1));
			timerInfoBubble.Enabled=false;//redundant?
			Cursor=Cursors.Default;
			Plugins.HookAddCode(this,"ContrAppt.ContrApptSheet2_MouseLeave_end");
		}

		///<summary></summary>
		private void InfoBubble_MouseMove(object sender,System.Windows.Forms.MouseEventArgs e) {
			//Calculate the real point in sheet coordinates
			Point p=new Point(e.X+infoBubble.Left-ContrApptSheet2.Left-panelSheet.Left,
				e.Y+infoBubble.Top-ContrApptSheet2.Top-panelSheet.Top);
			InfoBubbleDraw(p);
		}

		///<summary></summary>
		private void PicturePat_MouseMove(object sender,System.Windows.Forms.MouseEventArgs e) {
			//Calculate the real point in sheet coordinates
			Point p=new Point(e.X+infoBubble.Left+PicturePat.Left-ContrApptSheet2.Left-panelSheet.Left,
				e.Y+infoBubble.Top+PicturePat.Top-ContrApptSheet2.Top-panelSheet.Top);
			InfoBubbleDraw(p);
		}

		///<summary>Does a hit test to determine if over an appointment.  Fills the bubble with data and then positions it.</summary>
		private void InfoBubbleDraw(Point p) {
			//remember where to draw for hover effect
			if((comboView.SelectedIndex==0 && PrefC.GetBool(PrefName.AppointmentBubblesDisabled))
					|| (comboView.SelectedIndex>0 && _listApptViews[comboView.SelectedIndex-1].IsApptBubblesDisabled))
			{
				infoBubble.Visible=false;
				timerInfoBubble.Enabled=false;
				return;
			}
			bubbleLocation=p;
			long aptNum=HitTestAppt(p);
			if(aptNum==0 || HitTestApptBottom(p)) {
				if(infoBubble.Visible) {
					infoBubble.Visible=false;
					timerInfoBubble.Enabled=false;
				}
				return;
			}
			int yval=p.Y+ContrApptSheet2.Top+panelSheet.Top+10;//TODO Figure out the Prov bar height
			int xval=p.X+ContrApptSheet2.Left+panelSheet.Left+10;
			if(aptNum==bubbleAptNum) {
				if(DateTime.Now.AddMilliseconds(-280) > bubbleTime | !PrefC.GetBool(PrefName.ApptBubbleDelay)) {
					infoBubble.Visible=true;
					if(yval > panelSheet.Bottom-infoBubble.Height) {
						yval=panelSheet.Bottom-infoBubble.Height;
					}
					infoBubble.Location=new Point(xval,yval);
				}
				return;
			}
			if(aptNum!=bubbleAptNum) {
				//reset timer for popup delay
				timerInfoBubble.Enabled=false;
				timerInfoBubble.Enabled=true;
				//delay for hover effect 0.28 sec
				bubbleTime=DateTime.Now;
				bubbleAptNum=aptNum;
				//most data is already present in DS.Appointment, but we do need to get the patient picture
				bool hasPatientPicture=false;
				for(int i=0;i<_aptBubbleDefs.Count;i++) {
					if(_aptBubbleDefs[i].InternalName=="Patient Picture") {
						hasPatientPicture=true;
					}
				}
				infoBubble.BackgroundImage=new Bitmap(infoBubble.Width,800);
				Image img=infoBubble.BackgroundImage;//alias
				Graphics g=Graphics.FromImage(img);//infoBubble.BackgroundImage);
				g.TextRenderingHint=TextRenderingHint.ClearTypeGridFit;
				g.SmoothingMode=SmoothingMode.HighQuality;
				g.FillRectangle(new SolidBrush(infoBubble.BackColor),0,0,img.Width,img.Height);
				DataRow row=null;
				for(int i=0;i<DS.Tables["Appointments"].Rows.Count;i++) {
					if(DS.Tables["Appointments"].Rows[i]["AptNum"].ToString()==aptNum.ToString()) {
						row=DS.Tables["Appointments"].Rows[i];
					}
				}
				if(!hasPatientPicture) {
					infoBubble.Controls.Remove(PicturePat);
				}
				else {
					PicturePat.Location=new Point(6,17);
					if(!infoBubble.Controls.Contains(PicturePat)) {
						infoBubble.Controls.Add(PicturePat);
					}
					PicturePat.Image=null;
					if(row["ImageFolder"].ToString()!=""
						&& (PrefC.AtoZfolderUsed==DataStorageType.LocalAtoZ || PrefC.AtoZfolderUsed==DataStorageType.DropboxAtoZ))//Do not use patient image when A to Z folders are disabled.
					{
						try {
							Bitmap patPict;
							Documents.GetPatPict(PIn.Long(row["PatNum"].ToString()),
								ODFileUtils.CombinePaths(ImageStore.GetPreferredAtoZpath(),
									row["ImageFolder"].ToString().Substring(0,1).ToUpper(),
									row["ImageFolder"].ToString(),""),
									out patPict);
							PicturePat.Image=patPict;
						}
						catch(ApplicationException) { }  //A customer called in and an exception got through.  Added exception parameter as attempted fix.
					}
				}
				Font font=new Font(FontFamily.GenericSansSerif,9f);
				Brush brush=Brushes.Black;
				float x=0;
				float y=0;
				float h=0;
				float rowH=g.MeasureString("X",font).Height;
				for(int i=0;i<_aptBubbleDefs.Count;i++) {
					if(i==0) {
						font=new Font(FontFamily.GenericSansSerif,10f,FontStyle.Bold);
						x=8;
						y=0;
					}
					if(i==1) {
						font=new Font(FontFamily.GenericSansSerif,9f);
						y-=3;
						if(hasPatientPicture) {
							x=110;
							PicturePat.Location=new Point(PicturePat.Location.X,(int)y+5);
						}
						else {
							x=2;
						}
					}
					if(hasPatientPicture && y>=(PicturePat.Height+PicturePat.Location.Y)) {
						x=2;
					}
					switch(_aptBubbleDefs[i].InternalName) {
						case "Patient Name":
							h=g.MeasureString(row["patientName"].ToString(),font,infoBubble.Width-(int)x).Height;
							g.DrawString(row["patientName"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
							y+=h;
							continue;
						case "Patient Picture":
							//We have already dealt with this above.
							continue;
						case "Appt Day":
							h=g.MeasureString(row["aptDay"].ToString(),font,infoBubble.Width-(int)x).Height;
							g.DrawString(row["aptDay"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
							y+=h;
							continue;
						case "Appt Date":
							h=g.MeasureString(row["aptDate"].ToString(),font,infoBubble.Width-(int)x).Height;
							g.DrawString(row["aptDate"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
							y+=h;
							continue;
						case "Appt Time":
							h=g.MeasureString(row["aptTime"].ToString(),font,infoBubble.Width-(int)x).Height;
							g.DrawString(row["aptTime"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
							y+=h;
							continue;
						case "Appt Length":
							h=g.MeasureString(row["aptLength"].ToString(),font,infoBubble.Width-(int)x).Height;
							g.DrawString(row["aptLength"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
							y+=h;
							continue;
						case "Provider":
							h=g.MeasureString(row["provider"].ToString(),font,infoBubble.Width-(int)x).Height;
							g.DrawString(row["provider"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
							y+=h;
							continue;
						case "Production":
							h=g.MeasureString(row["production"].ToString(),font,infoBubble.Width-(int)x).Height;
							g.DrawString(row["production"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
							y+=h;
							continue;
						case "Confirmed":
							h=g.MeasureString(row["confirmed"].ToString(),font,infoBubble.Width-(int)x).Height;
							g.DrawString(row["confirmed"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
							y+=h;
							continue;
						case "Appt Status":
							if(row["AptStatus"].ToString()==((int)ApptStatus.ASAP).ToString()) {
								h=g.MeasureString(Lan.g("enumApptStatus","ASAP"),font,infoBubble.Width-(int)x).Height;
								g.DrawString(Lan.g("enumApptStatus","ASAP"),font,Brushes.Red,new RectangleF(x,y,infoBubble.Width-(int)x,h));
								y+=h;
							}
							continue;
						case "Med Flag":
							if(row["preMedFlag"].ToString()!="") {
								h=g.MeasureString(row["preMedFlag"].ToString(),font,infoBubble.Width-(int)x).Height;
								g.DrawString(row["preMedFlag"].ToString(),font,Brushes.Red,new RectangleF(x,y,infoBubble.Width-(int)x,h));
								y+=h;
							}
							continue;
						case "Med Note":
							if(row["MedUrgNote"].ToString()!="") {
								h=g.MeasureString(row["MedUrgNote"].ToString(),font,infoBubble.Width-(int)x).Height;
								g.DrawString(row["MedUrgNote"].ToString(),font,Brushes.Red,new RectangleF(x,y,infoBubble.Width-(int)x,h));
								y+=h;
							}
							continue;
						case "Lab":
							if(row["lab"].ToString()!="") {
								h=g.MeasureString(row["lab"].ToString(),font,infoBubble.Width-(int)x).Height;
								g.DrawString(row["lab"].ToString(),font,Brushes.Red,new RectangleF(x,y,infoBubble.Width-(int)x,h));
								y+=h;
							}
							continue;
						case "Procedures":
							if(row["procs"].ToString()!="") {
								h=g.MeasureString(row["procs"].ToString(),font,infoBubble.Width-(int)x).Height;
								g.DrawString(row["procs"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
								y+=h;
							}
							continue;
						case "Note":
							string noteStr=row["Note"].ToString();
							int maxNoteLength=PrefC.GetInt(PrefName.AppointmentBubblesNoteLength);
							if(noteStr.Trim()!="" && maxNoteLength>0 && noteStr.Length>maxNoteLength) {//Trim text
								noteStr=noteStr.Substring(0,maxNoteLength)+"...";
							}
							if(noteStr.Trim()!="") { //draw text
								h=g.MeasureString(noteStr,font,infoBubble.Width-(int)x).Height;
								g.DrawString(noteStr,font,Brushes.Blue,new RectangleF(x,y,infoBubble.Width-(int)x,h));
								y+=h;
							}
							continue;
						case "PatNum":
							h=g.MeasureString(row["patNum"].ToString(),font,infoBubble.Width-(int)x).Height;
							g.DrawString(row["patNum"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
							y+=h;
							continue;
						case "ChartNum":
							if(row["chartNumber"].ToString()!="") {
								h=g.MeasureString(row["chartNumber"].ToString(),font,infoBubble.Width-(int)x).Height;
								g.DrawString(row["chartNumber"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
								y+=h;
							}
							continue;
						case "Billing Type":
							h=g.MeasureString(row["billingType"].ToString(),font,infoBubble.Width-(int)x).Height;
							g.DrawString(row["billingType"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
							y+=h;
							continue;
						case "Horizontal Line":
							y+=3;
							g.DrawLine(new Pen(Brushes.Gray,1.5f),3,y,infoBubble.Width-3,y);
							y+=2;
							continue;
						case "Age":
							h=g.MeasureString(row["age"].ToString(),font,infoBubble.Width-(int)x).Height;
							g.DrawString(row["age"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
							y+=h;
							continue;
						case "Home Phone":
							h=g.MeasureString(row["hmPhone"].ToString(),font,infoBubble.Width-(int)x).Height;
							g.DrawString(row["hmPhone"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
							y+=h;
							continue;
						case "Work Phone":
							h=g.MeasureString(row["wkPhone"].ToString(),font,infoBubble.Width-(int)x).Height;
							g.DrawString(row["wkPhone"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
							y+=h;
							continue;
						case "Wireless Phone":
							h=g.MeasureString(row["wirelessPhone"].ToString(),font,infoBubble.Width-(int)x).Height;
							g.DrawString(row["wirelessPhone"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
							y+=h;
							continue;
						case "Contact Methods":
							if(row["contactMethods"].ToString()!="") {
								h=g.MeasureString(row["contactMethods"].ToString(),font,infoBubble.Width).Height;
								g.DrawString(row["contactMethods"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width,h));
								y+=h;
							}
							continue;
						case "Insurance":
							if(row["insurance"].ToString()!="") {//overkill since it's only one line
								h=g.MeasureString(row["insurance"].ToString(),font,infoBubble.Width-(int)x).Height;
								g.DrawString(row["insurance"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
								y+=h;
							}
							continue;
						case "Address Note":
							if(row["addrNote"].ToString()!="") {
								h=g.MeasureString(row["addrNote"].ToString(),font,infoBubble.Width-(int)x).Height;
								g.DrawString(row["addrNote"].ToString(),font,Brushes.Red,new RectangleF(x,y,infoBubble.Width-(int)x,h));
								y+=h;
							}
							continue;
						case "Fam Note":
							if(row["famFinUrgNote"].ToString()!="") {
								h=g.MeasureString(row["famFinUrgNote"].ToString(),font,infoBubble.Width-(int)x).Height;
								g.DrawString(row["famFinUrgNote"].ToString(),font,Brushes.Red,new RectangleF(x,y,infoBubble.Width-(int)x,h));
								y+=h;
							}
							continue;
						case "Appt Mod Note":
							if(row["apptModNote"].ToString()!="") {
								h=g.MeasureString(row["apptModNote"].ToString(),font,infoBubble.Width-(int)x).Height;
								g.DrawString(row["apptModNote"].ToString(),font,Brushes.Red,new RectangleF(x,y,infoBubble.Width-(int)x,h));
								y+=h;
							}
							continue;
						case "ReferralFrom":
							if(row["referralFrom"].ToString()!="") {
								h=g.MeasureString(row["referralFrom"].ToString(),font,infoBubble.Width-(int)x).Height;
								g.DrawString(row["referralFrom"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
								y+=h;
							}
							continue;
						case "ReferralTo":
							if(row["referralTo"].ToString()!="") {
								h=g.MeasureString(row["referralTo"].ToString(),font,infoBubble.Width-(int)x).Height;
								g.DrawString(row["referralTo"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
								y+=h;
							}
							continue;
						case "Language":
							if(row["Language"].ToString()!="") {
								h=g.MeasureString(row["Language"].ToString(),font,infoBubble.Width-(int)x).Height;
								g.DrawString(row["Language"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
								y+=h;
							}
							continue;
						case "Email":
							if(row["Email"].ToString()!="") {
								h=g.MeasureString(row["Email"].ToString(),font,infoBubble.Width-(int)x).Height;
								g.DrawString(row["Email"].ToString(),font,brush,new RectangleF(x,y,infoBubble.Width-(int)x,h));
								y+=h;
							}
							continue;
					}
				}
				//other family members?
				if(hasPatientPicture && y<PicturePat.Height+PicturePat.Location.Y) {
					y=PicturePat.Height+PicturePat.Location.Y;
				}
				g.DrawRectangle(Pens.Gray,0,0,infoBubble.Width-1,(int)y+4);
				g.Dispose();
				infoBubble.Size=new Size(infoBubble.Width,(int)y+5);
				infoBubble.BringToFront();
			}
			if(yval > panelSheet.Bottom-infoBubble.Height) {
				yval=panelSheet.Bottom-infoBubble.Height;
			}
			infoBubble.Location=new Point(xval,yval);
			/*only show right away if option set for no delay, otherwise, it will not show
			until mouse had hovered for at least 0.28 seconds(arbitrary #)
			the timer fires at 0.30 seconds, so the difference was introduced because
			of what seemed to be inconsistencies in the timer function */
			if(!PrefC.GetBool(PrefName.ApptBubbleDelay)) {
				infoBubble.Visible=true;
			}
			else {
				infoBubble.Visible=false;
			}
		}

		///<summary>Double click on appt sheet or on a single appointment.</summary>
		private void ContrApptSheet2_DoubleClick(object sender,System.EventArgs e) {
			if(Plugins.HookMethod(this,"ContrApptSheet2_DoubleClick_start",ContrApptSingle.ClickedAptNum,e)) {
				return;
			}
			mouseIsDown=false;
			//this logic is a little different than mouse down for now because on the first click of a 
			//double click, an appointment control is created under the mouse.
			if(ContrApptSingle.ClickedAptNum!=0) {//on appt
				long patnum=PIn.Long(TempApptSingle.DataRoww["PatNum"].ToString());
				TempApptSingle.Dispose();
				if(Appointments.GetOneApt(ContrApptSingle.ClickedAptNum)==null) {
					MsgBox.Show(this,"Selected appointment no longer exists.");
					RefreshModuleDataPeriod();
					RefreshModuleScreenPeriod();
					return;
				}
				//security handled inside the form
				FormApptEdit FormAE=new FormApptEdit(ContrApptSingle.ClickedAptNum);
				FormAE.ShowDialog();
				if(FormAE.DialogResult==DialogResult.OK) {
					Appointment apt=Appointments.GetOneApt(ContrApptSingle.ClickedAptNum);
					if(apt!=null && DoesOverlap(apt)) {
						Appointment aptOld=apt.Copy();
						MsgBox.Show(this,"Appointment is too long and would overlap another appointment.  Automatically shortened to fit.");
						while(DoesOverlap(apt)) {
							apt.Pattern=apt.Pattern.Substring(0,apt.Pattern.Length-1);
							if(apt.Pattern.Length==1) {
								break;
							}
						}
						try {
							Appointments.Update(apt,aptOld);
						}
						catch(ApplicationException ex) {
							MessageBox.Show(ex.Message);
						}
					}
					ModuleSelected(patnum);//apt.PatNum);//apt might be null if user deleted appt.
					Signalods.SetInvalidAppt(apt,FormAE.GetAppointmentOld());//use old from form because aptOld is a clone of apt from DB.
				}
				else if(FormAE.DialogResult==DialogResult.Cancel && FormAE.HasProcsChangedAndCancel) { //If user canceled but changed the procs on appt first
					//Refresh the grid, don't need to check length because it didn't change.  Plus user might not want to change length.
					ModuleSelected(patnum);
					Signalods.SetInvalidAppt(FormAE.GetAppointmentOld());//use old here because they cancelled.
				}
			}
			//not on apt, so trying to schedule an appointment---------------------------------------------------------------------
			else {
				if(!Security.IsAuthorized(Permissions.AppointmentCreate)) {
					return;
				}
				if(ApptDrawing.VisOps.Count==0) {//no ops visible.
					return;
				}
				FormPatientSelect FormPS=new FormPatientSelect();
				if(PatCur!=null) {
					FormPS.InitialPatNum=PatCur.PatNum;
				}
				FormPS.ShowDialog();
				if(FormPS.DialogResult!=DialogResult.OK) {
					return;
				}
				if(PatCur==null || FormPS.SelectedPatNum!=PatCur.PatNum) {//if the patient was changed
					RefreshModuleDataPatient(FormPS.SelectedPatNum);
					FormOpenDental.S_Contr_PatientSelected(PatCur,true,false);
					//RefreshModulePatient(FormPS.SelectedPatNum);
				}
				if(PatCur!=null && PatRestrictions.IsRestricted(PatCur.PatNum,PatRestrict.ApptSchedule)) {
					return;
				}
				Appointment apt;
				if(FormPS.NewPatientAdded) {
					Operatory curOp=Operatories.GetOperatory(SheetClickedonOp);
					DateTime d=AppointmentL.DateSelected;
					if(ApptDrawing.IsWeeklyView) {
						d=WeekStartDate.AddDays(SheetClickedonDay);
					}
					//minutes always rounded down.
					int minutes=(int)(ContrAppt.SheetClickedonMin/ApptDrawing.MinPerIncr)*ApptDrawing.MinPerIncr;
					DateTime dateTimeStart=new DateTime(d.Year,d.Month,d.Day,ContrAppt.SheetClickedonHour,minutes,0);
					DateTime dateTimeAskedToArrive=DateTime.MinValue;
					if(PatCur.AskToArriveEarly > 0) {
						dateTimeAskedToArrive=dateTimeStart.AddMinutes(-PatCur.AskToArriveEarly);
						MessageBox.Show(Lan.g(this,"Ask patient to arrive")+" "+PatCur.AskToArriveEarly
							+" "+Lan.g(this,"minutes early at")+" "+dateTimeAskedToArrive.ToShortTimeString()+".");
					}
					apt=Appointments.CreateApptForNewPatient(PatCur,curOp,dateTimeStart,dateTimeAskedToArrive,"/X/",SchedListPeriod);
					//New patient. Set to prospective if operatory is set to set prospective.
					if(curOp.SetProspective) {
						if(MsgBox.Show(this,MsgBoxButtons.OKCancel,"Patient's status will be set to Prospective.")) {
							Patient patOld=PatCur.Copy();
							PatCur.PatStatus=PatientStatus.Prospective;
							Patients.Update(PatCur,patOld);
						}
					}
					FormApptEdit FormAE=new FormApptEdit(apt.AptNum);//this is where security log entry is made
					FormAE.IsNew=true;
					FormAE.ShowDialog();
					if(FormAE.DialogResult==DialogResult.OK) {
						if(apt.IsNewPatient) {
							AutomationL.Trigger(AutomationTrigger.CreateApptNewPat,null,apt.PatNum,apt.AptNum);
						}
						AutomationL.Trigger(AutomationTrigger.CreateAppt,null,apt.PatNum,apt.AptNum);
						RefreshModuleDataPatient(PatCur.PatNum);
						FormOpenDental.S_Contr_PatientSelected(PatCur,true,false);
						//RefreshModulePatient(PatCurNum);
						RefreshPeriod();
						if(apt!=null && !HasValidStartTime(apt)) {
							Appointment aptOld=apt.Copy();
							MsgBox.Show(this,"Appointment start time would overlap another appointment.  Moving appointment to pinboard.");
							SendToPinBoard(apt.AptNum);
							apt.AptStatus=ApptStatus.UnschedList;
							try {
								Appointments.Update(apt,aptOld);
							}
							catch(ApplicationException ex) {
								MessageBox.Show(ex.Message);
							}
							RefreshPeriod();
							Signalods.SetInvalidAppt(apt);//No need for old appt here. It is a new appt anyway.
							return;//It's ok to skip the rest of the method here. The appointment is now on the pinboard and must be rescheduled
						}
						apt=Appointments.GetOneApt(apt.AptNum);  //Need to get appt from DB so we have the time pattern
						if(apt!=null && DoesOverlap(apt)) {
							Appointment aptOld=apt.Copy();
							MsgBox.Show(this,"Appointment is too long and would overlap another appointment.  Automatically shortened to fit.");
							while(DoesOverlap(apt)) {
								apt.Pattern=apt.Pattern.Substring(0,apt.Pattern.Length-1);
								if(apt.Pattern.Length==1) {
									break;
								}
							}
							try {
								Appointments.Update(apt,aptOld);
							}
							catch(ApplicationException ex) {
								MessageBox.Show(ex.Message);
							}
						}
						RefreshPeriod();
						Signalods.SetInvalidAppt(apt);
					}
				}
				else {//new patient not added
					if(Appointments.HasOutsandingAppts(PatCur.PatNum) | (Plugins.HookMethod(this,"ContrAppt.ContrApptSheet2_DoubleClick_apptOtherShow"))) {
						DisplayOtherDlg(true);
					}
					else {
						FormApptsOther FormAO=new FormApptsOther(PatCur.PatNum);//doesn't actually get shown
						CheckStatus();
						FormAO.InitialClick=true;
						FormAO.MakeAppointment();
						//if(FormAO.OResult==OtherResult.Cancel) {//this wasn't catching user hitting cancel from within appt edit window
						//	return;
						//}
						if(FormAO.AptNumsSelected.Count>0) {
							ContrApptSingle.SelectedAptNum=FormAO.AptNumsSelected[0];
						}
						//RefreshModuleDataPatient(FormAO.SelectedPatNum);//patient won't have changed
						//OnPatientSelected(PatCur.PatNum,PatCur.GetNameLF(),PatCur.Email!="",PatCur.ChartNumber);
						RefreshPeriod();
						apt=Appointments.GetOneApt(ContrApptSingle.SelectedAptNum);
						if(apt!=null && DoesOverlap(apt)) {
							Appointment aptOld=apt.Copy();
							MsgBox.Show(this,"Appointment is too long and would overlap another appointment.  Automatically shortened to fit.");
							while(DoesOverlap(apt)) {
								apt.Pattern=apt.Pattern.Substring(0,apt.Pattern.Length-1);
								if(apt.Pattern.Length==1) {
									break;
								}
							}
							try {
								Appointments.Update(apt,aptOld);
							}
							catch(ApplicationException ex) {
								MessageBox.Show(ex.Message);
							}
						}
						RefreshPeriod();
						Signalods.SetInvalidAppt(apt);//no need for old appt here.
					}
				}
			}
		}

		///<summary>Displays the Other Appointments for the current patient, then refreshes screen as needed.  initialClick specifies whether the user doubleclicked on a blank time to get to this dialog.</summary>
		public void DisplayOtherDlg(bool initialClick) {
			if(PatCur==null) {
				return;
			}
			FormApptsOther FormAO=new FormApptsOther(PatCur.PatNum);
			FormAO.InitialClick=initialClick;
			FormAO.ShowDialog();
			if(FormAO.OResult==OtherResult.Cancel) {
				return;
			}
			switch(FormAO.OResult) {
				case OtherResult.CopyToPinBoard:
					SendToPinboard(FormAO.AptNumsSelected);
					RefreshModuleDataPatient(FormAO.SelectedPatNum);
					FormOpenDental.S_Contr_PatientSelected(PatCur,true,false);
					//RefreshModulePatient(FormAO.SelectedPatNum);
					RefreshPeriod();
					break;
				case OtherResult.NewToPinBoard:
					SendToPinboard(FormAO.AptNumsSelected);
					RefreshModuleDataPatient(FormAO.SelectedPatNum);
					FormOpenDental.S_Contr_PatientSelected(PatCur,true,false);
					//RefreshModulePatient(FormAO.SelectedPatNum);
					RefreshPeriod();
					break;
				case OtherResult.PinboardAndSearch:
					SendToPinboard(FormAO.AptNumsSelected);
					if(ApptDrawing.IsWeeklyView) {
						break;
					}
					dateSearch.Text=FormAO.DateJumpToString;
					if(!groupSearch.Visible) {//if search not already visible
						ShowSearch();
					}
					DoSearch();
					break;
				case OtherResult.CreateNew:
					ContrApptSingle.SelectedAptNum=FormAO.AptNumsSelected[0];
					RefreshModuleDataPatient(FormAO.SelectedPatNum);
					FormOpenDental.S_Contr_PatientSelected(PatCur,true,false);
					//RefreshModulePatient(FormAO.SelectedPatNum);
					RefreshPeriod();
					Appointment apt=Appointments.GetOneApt(ContrApptSingle.SelectedAptNum);
					if(apt!=null && !HasValidStartTime(apt)) {
						Appointment aptOld=apt.Copy();
						MsgBox.Show(this,"Appointment start time would overlap another appointment.  Moving appointment to pinboard.");
						SendToPinBoard(apt.AptNum);
						apt.AptStatus=ApptStatus.UnschedList;
						try {
							Appointments.Update(apt,aptOld);
						}
						catch(ApplicationException ex) {
							MessageBox.Show(ex.Message);
						}
						RefreshPeriod();
						break;
					}
					if(apt!=null && DoesOverlap(apt)) {
						Appointment aptOld=apt.Copy();
						MsgBox.Show(this,"Appointment is too long and would overlap another appointment.  Automatically shortened to fit.");
						while(DoesOverlap(apt)) {
							apt.Pattern=apt.Pattern.Substring(0,apt.Pattern.Length-1);
							if(apt.Pattern.Length==1) {
								break;
							}
						}
						try {
							Appointments.Update(apt,aptOld);
						}
						catch(ApplicationException ex) {
							MessageBox.Show(ex.Message);
						}
					}
					RefreshPeriod();
					Signalods.SetInvalidAppt(apt);
					break;
				case OtherResult.GoTo:
					ContrApptSingle.SelectedAptNum=FormAO.AptNumsSelected[0];
					AppointmentL.DateSelected=PIn.Date(FormAO.DateJumpToString);
					RefreshModuleDataPatient(FormAO.SelectedPatNum);
					FormOpenDental.S_Contr_PatientSelected(PatCur,true,false);
					if(ApptDrawing.IsWeeklyView) {
						if((int)AppointmentL.DateSelected.DayOfWeek==0) {//if sunday
							WeekStartDate=AppointmentL.DateSelected.AddDays(-6).Date;//go back to the previous monday
						}
						else {
							WeekStartDate=AppointmentL.DateSelected.AddDays(1-(int)AppointmentL.DateSelected.DayOfWeek).Date;//go back to current monday
						}
						WeekEndDate=WeekStartDate.AddDays(ApptDrawing.NumOfWeekDaysToDisplay-1).Date;
					}
					//RefreshModulePatient(FormAO.SelectedPatNum);
					RefreshPeriod();
					break;
			}
		}

		private void ToolBarMain_ButtonClick(object sender,OpenDental.UI.ODToolBarButtonClickEventArgs e) {
			if(e.Button.Tag.GetType()==typeof(string)) {
				//standard predefined button
				switch(e.Button.Tag.ToString()) {
					case "Lists":
						OnLists_Click();
						break;
					case "Print":
						OnPrint_Click();
						break;
					case "RapidCall":
						try {
							RapidCall.ShowPage();
						}
						catch(Exception ex) {
							MessageBox.Show(ex.Message);
						}
						break;
				}
			}
			else if(e.Button.Tag.GetType()==typeof(Program)) {
				if(PatCur!=null) {
					Patient pat=Patients.GetPat(PatCur.PatNum);
					ProgramL.Execute(((Program)e.Button.Tag).ProgramNum,pat);
				}
			}
		}

		/*private void OnPat_Click() {
			FormPatientSelect formPS = new FormPatientSelect();
			formPS.ShowDialog();
			if(formPS.DialogResult!=DialogResult.OK){
				return;
			}
			RefreshModulePatient(formPS.SelectedPatNum);
			DisplayOtherDlg(false);
		}*/

		private void OnUnschedList_Click() {
			//Reselect existing window if available, if not create a new instance
			if(FormUnsched2==null || FormUnsched2.IsDisposed) {
				FormUnsched2=new FormUnsched();
			}
			FormUnsched2.Show();
			if(FormUnsched2.WindowState==FormWindowState.Minimized) {//only applicable if re-using an existing instance
				FormUnsched2.WindowState=FormWindowState.Normal;
			}
			FormUnsched2.BringToFront();
		}

		private void OnASAPList_Click() {
			if(FormASAP==null || FormASAP.IsDisposed) {
				FormASAP=new FormASAP();
			}
			FormASAP.Show();
			if(FormASAP.WindowState==FormWindowState.Minimized) {
				FormASAP.WindowState=FormWindowState.Normal;
			}
			FormASAP.BringToFront();
		}

		private void OnRadiology_Click() {
			List<FormRadOrderList> listFormROLs=Application.OpenForms.OfType<FormRadOrderList>().ToList();
			if(listFormROLs.Count > 0) {
				listFormROLs[0].RefreshRadOrdersForUser(Security.CurUser);
				listFormROLs[0].BringToFront();
			}
			else {
				FormRadOrderList FormPRL=new FormRadOrderList(Security.CurUser);
				FormPRL.FormClosing+=FormPRL_FormClosing;
				FormPRL.Show();
			}
		}

		private void OnInsVerify_Click() {
            List<FormInsVerificationList> listFormROLs=Application.OpenForms.OfType<FormInsVerificationList>().ToList();
			if(listFormROLs.Count>0) {
				listFormROLs[0].FillGrids();
				listFormROLs[0].BringToFront();
			}
			else {
				FormInsVerificationList FormIVL=new FormInsVerificationList();
				FormIVL.FormClosing+=FormIVL_FormClosing;
				FormIVL.Show();
			}
        }

		private void FormPRL_FormClosing(object sender,FormClosingEventArgs e) {
			ActionTaken.Invoke(sender,new ActionNeededEventArgs(ActionNeededTypes.RadiologyProcedures));
		}

		private void FormIVL_FormClosing(object sender,FormClosingEventArgs e) {
            //Action does not currently need to be taken when leaving the insurance verification list window.
		}

		private void OnRecall_Click() {
			if(FormRecallL==null || FormRecallL.IsDisposed) {
				FormRecallL=new FormRecallList();
			}
			FormRecallL.Show();
			if(FormRecallL.WindowState==FormWindowState.Minimized) {
				FormRecallL.WindowState=FormWindowState.Normal;
			}
			FormRecallL.BringToFront();
		}

		private void OnConfirm_Click() {
			if(FormConfirmL==null || FormConfirmL.IsDisposed) {
				FormConfirmL=new FormConfirmList();
			}
			FormConfirmL.Show();
			if(FormConfirmL.WindowState==FormWindowState.Minimized) {
				FormConfirmL.WindowState=FormWindowState.Normal;
			}
			FormConfirmL.BringToFront();
		}

		private void OnTrack_Click() {
			if(FormTN==null || FormTN.IsDisposed) {
				FormTN=new FormTrackNext();
			}
			FormTN.Show();
			if(FormTN.WindowState==FormWindowState.Minimized) {
				FormTN.WindowState=FormWindowState.Normal;
			}
			FormTN.BringToFront();
		}

		private void OnLists_Click() {
			FormApptLists FormA=new FormApptLists();
			FormA.ShowDialog();
			if(FormA.DialogResult==DialogResult.Cancel) {
				return;
			}
			switch(FormA.SelectionResult) {
				case ApptListSelection.Recall:
					OnRecall_Click();
					break;
				case ApptListSelection.Confirm:
					OnConfirm_Click();
					break;
				case ApptListSelection.Planned:
					OnTrack_Click();
					break;
				case ApptListSelection.Unsched:
					OnUnschedList_Click();
					break;
				case ApptListSelection.ASAP:
					OnASAPList_Click();
					break;
				case ApptListSelection.Radiology:
					OnRadiology_Click();
					break;
				case ApptListSelection.InsVerify:
					OnInsVerify_Click();
					break;
			}
		}

		private void OnPrint_Click() {
			if(ApptDrawing.VisOps.Count==0) {//no ops visible.
				MsgBox.Show(this,"There must be at least one operatory showing in order to Print Schedule.");
				return;
			}
			if(PrinterSettings.InstalledPrinters.Count==0) {
				MessageBox.Show(Lan.g(this,"Printer not installed"));
				return;
			}
			FormApptPrintSetup FormAPS=new FormApptPrintSetup();
			FormAPS.ShowDialog();
			if(FormAPS.DialogResult!=DialogResult.OK) {
				return;
			}
			apptPrintStartTime=FormAPS.ApptPrintStartTime;
			apptPrintStopTime=FormAPS.ApptPrintStopTime;
			apptPrintFontSize=FormAPS.ApptPrintFontSize;
			apptPrintColsPerPage=FormAPS.ApptPrintColsPerPage;
			_isPrintPreview=FormAPS.IsPrintPreview;
			pagesPrinted=0;
			pageRow=0;
			pageColumn=0;
			PrintReport();
			ApptDrawing.LineH=12;//Reset the LineH to default.
			CopyScheduleToClipboard();
			if(PatCur==null) {
				ModuleSelected(0);//Refresh the public variables in ApptDrawing.cs
			}
			else {
				ModuleSelected(PatCur.PatNum);//Refresh the public variables in ApptDrawing.cs
			}
		}

		///<summary></summary>
		public void PrintReport() {
			pd2=new PrintDocument();
			pd2.PrintPage += new PrintPageEventHandler(this.pd2_PrintPage);
			//pd2.DefaultPageSettings.Margins= new Margins(10,40,40,60);
			if(_isPrintPreview) {
				FormPrintPreview pView=new FormPrintPreview(PrintSituation.Appointments,pd2,0,"Daily appointment view for "+apptPrintStartTime.ToShortDateString()+" printed");
				pView.ShowDialog();
			}
			else {
#if DEBUG
				FormPrintPreview pView=new FormPrintPreview(PrintSituation.Appointments,pd2,0,"Daily appointment view for "+apptPrintStartTime.ToShortDateString()+" printed");
				pView.ShowDialog();
#else
				if(!PrinterL.SetPrinter(pd2,PrintSituation.Appointments,0,"Daily appointment view for "+apptPrintStartTime.ToShortDateString()+" printed")){
					return;
				}
				try{
					pd2.Print();
				}
				catch{
					MessageBox.Show(Lan.g(this,"Printer not available"));
				}
#endif
 			}
		}

		private void pd2_PrintPage(object sender,System.Drawing.Printing.PrintPageEventArgs e) {
			PrintApptSchedule(sender,e);
		}

		private void PrintApptSchedule(object sender,System.Drawing.Printing.PrintPageEventArgs e) {
			//Logic needs to be added here for calculating if printing will fit on the page. Then call drawing in a loop for number of required pages. 
			Rectangle pageBounds=e.PageBounds;
			int headerOffset=75;
			int footerOffset=40;
			int marginOffset=30;
			//The extra 15 is for the right side of that page.  For some reason the right margin needs a little extra room.
			ApptDrawing.ApptSheetWidth=pageBounds.Width-((marginOffset*2)+15);
			ApptDrawing.ComputeColWidth(apptPrintColsPerPage);
			ApptDrawing.SetLineHeight(apptPrintFontSize);//Measure size the user set to determine the line height for printout.
			int startHour=apptPrintStartTime.Hour;
			int stopHour=apptPrintStopTime.Hour;
			if(stopHour==0) {
				stopHour=24;
			}
			float totalHeight=ApptDrawing.LineH*ApptDrawing.RowsPerHr*(stopHour-startHour);
			//Figure out how many pages are needed to print.
			int pagesAcross=(int)Math.Ceiling((decimal)ApptDrawing.VisOps.Count/(decimal)apptPrintColsPerPage);
			int pagesTall=(int)Math.Ceiling((decimal)totalHeight/(decimal)(pageBounds.Height-(headerOffset+footerOffset)));
			int totalPages=pagesAcross*pagesTall;
			if(ApptDrawing.IsWeeklyView) {
				pagesAcross=1;
				totalPages=1*pagesTall;
			}
			//Decide what page currently on thus knowing what hours to print.
			#region HoursOnPage
			int hoursPerPage=(int)Math.Floor((decimal)(pageBounds.Height-(headerOffset+footerOffset))/(decimal)(ApptDrawing.LineH*ApptDrawing.RowsPerHr));
			int hourBegin=startHour;
			int hourEnd=hourBegin+hoursPerPage;
			if(pageRow>0) {
				hourBegin=startHour+(hoursPerPage*pageRow);
				hourEnd=hourBegin+hoursPerPage;
			}
			if(hourEnd>stopHour) {//Don't show too many hours.
				hourEnd=stopHour;
			}
			ApptDrawing.ApptSheetHeight=ApptDrawing.LineH*ApptDrawing.RowsPerHr*(hourEnd-hourBegin);
			if(hourEnd>23) {//Midnight must be 0.
				hourEnd=0;
			}
			DateTime beginTime=new DateTime(1,1,1,hourBegin,0,0);
			DateTime endTime=new DateTime(1,1,1,hourEnd,0,0);
			#endregion
			e.Graphics.TranslateTransform(marginOffset,headerOffset);//Compensate for header and margin.
			ApptDrawing.DrawAllButAppts(e.Graphics,false,beginTime,endTime,apptPrintColsPerPage,pageColumn,apptPrintFontSize,true);
			//Draw the appointments.
			#region ApptSingleDrawing
			//Clear out the ProvBar from previous page.
			ApptDrawing.ProvBar=new int[ApptDrawing.VisProvs.Count][];
			for(int i=0;i<ApptDrawing.VisProvs.Count;i++) {
				ApptDrawing.ProvBar[i]=new int[24*ApptDrawing.RowsPerHr]; //[144]; or 24*6
			}
			if(ContrApptSingle3!=null) {//I think this is not needed.
				for(int i=0;i<ContrApptSingle3.Length;i++) {
					if(ContrApptSingle3[i]!=null) {
						ContrApptSingle3[i].Dispose();
					}
					ContrApptSingle3[i]=null;
				}
				ContrApptSingle3=null;
			}
			DateTime startDate=(ApptDrawing.IsWeeklyView?WeekStartDate:AppointmentL.DateSelected);
			DateTime endDate=(ApptDrawing.IsWeeklyView?WeekEndDate:AppointmentL.DateSelected);
			ContrApptSingle3=new ContrApptSingle[DS.Tables["Appointments"].Rows.Count];
			for(int i=0;i<DS.Tables["Appointments"].Rows.Count;i++) {
				DataRow dataRoww=DS.Tables["Appointments"].Rows[i];
				if(PIn.Date(dataRoww["AptDateTime"].ToString()).Date<startDate.Date || PIn.Date(dataRoww["AptDateTime"].ToString()).Date>endDate.Date){
					continue;//Appointment is outside of our date range.
				}
				if(!ApptDrawing.IsWeeklyView) {
					ApptDrawing.ProvBarShading(dataRoww);//Always fill prov bars.
				}
				//Filter the list of appointments here for those those within the time frame.
				if(!ApptSingleDrawing.ApptWithinTimeFrame(dataRoww,beginTime,endTime,apptPrintColsPerPage,pageColumn)) {
					continue;
				}
				ContrApptSingle3[i]=new ContrApptSingle();
				ContrApptSingle3[i].Visible=false;
				ContrApptSingle3[i].DataRoww=dataRoww;
				ContrApptSingle3[i].TableApptFields=DS.Tables["ApptFields"];
				ContrApptSingle3[i].TablePatFields=DS.Tables["PatFields"];
				ContrApptSingle3[i].PatternShowing=ApptSingleDrawing.GetPatternShowing(dataRoww["Pattern"].ToString());
				ContrApptSingle3[i].Size=ApptSingleDrawing.SetSize(dataRoww);
				ContrApptSingle3[i].Location=ApptSingleDrawing.SetLocation(dataRoww,hourBegin,apptPrintColsPerPage,pageColumn);
				e.Graphics.ResetTransform();
				e.Graphics.TranslateTransform(ContrApptSingle3[i].Location.X+marginOffset,ContrApptSingle3[i].Location.Y+headerOffset);
				ApptSingleDrawing.DrawEntireAppt(e.Graphics,dataRoww,ContrApptSingle3[i].PatternShowing,ContrApptSingle3[i].Size.Width,ContrApptSingle3[i].Size.Height,
					false,false,-1,ApptViewItemL.ApptRows,ApptViewItemL.ApptViewCur,DS.Tables["ApptFields"],DS.Tables["PatFields"],apptPrintFontSize,true);
			}
			#endregion
			e.Graphics.ResetTransform();
			//Cover the portions of the appointments that don't belong on the page.
			e.Graphics.FillRectangle(new SolidBrush(Color.White),0,0,pageBounds.Width,headerOffset-1);
			e.Graphics.FillRectangle(new SolidBrush(Color.White),0,ApptDrawing.ApptSheetHeight+headerOffset,pageBounds.Width,totalHeight);
			//Draw the header
			DrawPrintingHeader(e.Graphics,totalPages,pageBounds.Width,pageBounds.Height);
			pagesPrinted++;
			pageColumn++;
			if(totalPages==pagesPrinted) {
				pagesPrinted=0;
				pageRow=0;
				pageColumn=0;
				e.HasMorePages=false;
			}
			else {
				e.HasMorePages=true;
				if(pagesPrinted==pagesAcross*(pageRow+1)) {
					pageRow++;
					pageColumn=0;
				}
			}
		}

		///<summary>Header and footer for printing.</summary>
		private void DrawPrintingHeader(Graphics g,int totalPages,float pageWidth,float pageHeight) {
			float xPos=0;//starting pos
			float yPos=25f;//starting pos
			//Print Title------------------------------------------------------------------------------
			string title;
			string date;
			if(ApptDrawing.IsWeeklyView) {
				title=Lan.g(this,"Weekly Appointments");
				date=WeekStartDate.DayOfWeek.ToString()+" "+WeekStartDate.ToShortDateString()
					+" - "+WeekEndDate.DayOfWeek.ToString()+" "+WeekEndDate.ToShortDateString();
			}
			else {
				title=Lan.g(this,"Daily Appointments");
				date=AppointmentL.DateSelected.DayOfWeek.ToString()+"   "+AppointmentL.DateSelected.ToShortDateString();
			}
			Font titleFont=new Font("Arial",12,FontStyle.Bold);
			float xTitle = (float)((pageWidth/2)-((g.MeasureString(title,titleFont).Width/2)));
			g.DrawString(title,titleFont,Brushes.Black,xTitle,yPos);//centered
			//Print Date--------------------------------------------------------------------------------
			Font dateFont=new Font("Arial",8,FontStyle.Regular);
			float xDate = (float)((pageWidth/2)-((g.MeasureString(date,dateFont).Width/2)));
			yPos+=20;
			g.DrawString(date,dateFont,Brushes.Black,xDate,yPos);//centered
			//Col titles-----------------------------------------------------------------------------
			if(!ApptDrawing.IsWeeklyView) {
				string[] headers = new string[apptPrintColsPerPage];
				Font headerFont=new Font("Arial",8);
				yPos+=15;
				xPos+=(int)(ApptDrawing.TimeWidth+(ApptDrawing.ProvWidth*ApptDrawing.ProvCount)+30);//30 for margins.
				int xCenter=0;
				for(int i=0;i<apptPrintColsPerPage;i++) {
					if(i==ApptDrawing.VisOps.Count) {
						break;
					}
					int k=apptPrintColsPerPage*pageColumn+i;
					if(k>=ApptDrawing.VisOps.Count) {
						break;
					}
					headers[i]=ApptDrawing.VisOps[k].OpName;
					if(g.MeasureString(headers[i],headerFont).Width>ApptDrawing.ColWidth) {
						RectangleF rf=new RectangleF(xPos,yPos,ApptDrawing.ColWidth,g.MeasureString(headers[i],headerFont).Height);
						g.DrawString(headers[i],headerFont,Brushes.Black,rf);
					}
					else {
						xCenter=(int)((ApptDrawing.ColWidth/2)-(g.MeasureString(headers[i],headerFont).Width/2));
						g.DrawString(headers[i],headerFont,Brushes.Black,(int)(xPos+xCenter),yPos);
					}
					xPos+=ApptDrawing.ColWidth;
				}
			}
			else {
				string columnDate="";
				Font headerFont=new Font("Arial",8);
				yPos+=15;
				xPos+=(int)(ApptDrawing.TimeWidth)+30;//30 for margins.
				int xCenter=0;
				int day=WeekStartDate.Day;
				int daysInMonth=DateTime.DaysInMonth(WeekStartDate.Year,WeekStartDate.Month);
				for(int i=0;i<7;i++) {
					switch(i) {
						case 0:
							columnDate="Monday-"+day;
							break;
						case 1:
							columnDate="Tuesday-"+day;
							break;
						case 2:
							columnDate="Wednesday-"+day;
							break;
						case 3:
							columnDate="Thursday-"+day;
							break;
						case 4:
							columnDate="Friday-"+day;
							break;
						case 5:
							columnDate="Saturday-"+day;
							break;
						case 6:
							columnDate="Sunday-"+day;
							break;
					}
					day++;
					if(day>daysInMonth) {
						day=1;//Week contains days in the next month.
					}
					xCenter=(int)((ApptDrawing.ColDayWidth/2)-(g.MeasureString(columnDate,headerFont).Width/2));
					g.DrawString(columnDate,headerFont,Brushes.Black,(int)(xPos+xCenter),yPos);
					xPos+=ApptDrawing.ColDayWidth;
				}
			}
			//Print Footer-----------------------------------------------------------------------------
			string page=(pagesPrinted+1)+" / "+totalPages;
			float xPage = (float)(400-((g.MeasureString(page,dateFont).Width/2)));
			yPos=pageHeight-40;
			g.DrawString(page,dateFont,Brushes.Black,xPage,yPos);
		}

		///<summary>Sends an image of the current appointment schedule to the clipboard.  Some users 'paste' to their own editor for more control.</summary>
		private void CopyScheduleToClipboard() {
			ArrayList aListStart=new ArrayList();
			ArrayList aListStop=new ArrayList();
			DateTime startTime;
			DateTime stopTime;
			for(int i=0;i<SchedListPeriod.Count;i++) {
				if(SchedListPeriod[i].SchedType!=ScheduleType.Provider) {
					continue;
				}
				if(SchedListPeriod[i].StartTime==TimeSpan.FromHours(0)) {//ignore notes at midnight
					continue;
				}
				aListStart.Add(SchedListPeriod[i].SchedDate+SchedListPeriod[i].StartTime);
				aListStop.Add(SchedListPeriod[i].SchedDate+SchedListPeriod[i].StopTime);
			}
			if(aListStart.Count > 0) {//makes sure there is at least one timeblock
				startTime=(DateTime)aListStart[0];
				for(int i=0;i<aListStart.Count;i++) {
					//if (A) OR (B AND C)
					if((((DateTime)(aListStart[i])).Hour < startTime.Hour) 
						|| (((DateTime)(aListStart[i])).Hour==startTime.Hour 
						&& ((DateTime)(aListStart[i])).Minute < startTime.Minute)) {
						startTime=(DateTime)aListStart[i];
					}
				}
				stopTime=(DateTime)aListStop[0];
				for(int i=0;i<aListStop.Count;i++) {
					//if (A) OR (B AND C)
					if((((DateTime)(aListStop[i])).Hour > stopTime.Hour) 
						|| (((DateTime)(aListStop[i])).Hour==stopTime.Hour 
						&& ((DateTime)(aListStop[i])).Minute > stopTime.Minute)) {
						stopTime=(DateTime)aListStop[i];
					}
				}
			}
			else {//office is closed
				startTime=new DateTime(AppointmentL.DateSelected.Year,AppointmentL.DateSelected.Month
					,AppointmentL.DateSelected.Day
					,ApptDrawing.ConvertToHour(-ContrApptSheet2.Location.Y)
					,ApptDrawing.ConvertToMin(-ContrApptSheet2.Location.Y)
					,0);
				if(ApptDrawing.ConvertToHour(-ContrApptSheet2.Location.Y)+12<23) {
					//we will be adding an extra hour later
					stopTime=new DateTime(AppointmentL.DateSelected.Year,AppointmentL.DateSelected.Month
						,AppointmentL.DateSelected.Day
						,ApptDrawing.ConvertToHour(-ContrApptSheet2.Location.Y)+12//add 12 hours
						,ApptDrawing.ConvertToMin(-ContrApptSheet2.Location.Y)
						,0);
				}
				else {
					stopTime=new DateTime(AppointmentL.DateSelected.Year,AppointmentL.DateSelected.Month
						,AppointmentL.DateSelected.Day
						,22
						,ApptDrawing.ConvertToMin(-ContrApptSheet2.Location.Y)
						,0);
				}
			}
			int recX=0;
			int recY=(int)(ApptDrawing.LineH*ApptDrawing.RowsPerHr*startTime.Hour);
			int recWidth=(int)ContrApptSheet2.Shadow.Width;
			int recHeight=(int)((ApptDrawing.LineH*ApptDrawing.RowsPerHr
				*(stopTime.Hour-startTime.Hour+1)));
			Rectangle imageRect = new Rectangle(recX,recY,recWidth,recHeight); //Holds new dimensions for temp image
			Bitmap imageTemp=ContrApptSheet2.Shadow.Clone(imageRect,PixelFormat.DontCare);  //Clones image and sets size to only show the time open for that day. (Needs to be rewritten)
			Clipboard.SetDataObject(imageTemp);
		}

		///<summary>Clears the pinboard.</summary>
		private void butClearPin_Click(object sender,System.EventArgs e) {
			if(pinBoard.ApptList.Count==0) {
				MsgBox.Show(this,"There are no appointments on the pinboard to clear.");
				return;
			}
			if(pinBoard.SelectedIndex==-1) {
				MsgBox.Show(this,"Please select an appointment first.");
				return;
			}
			DataRow row=pinBoard.ApptList[pinBoard.SelectedIndex].DataRoww;
			pinBoard.ClearSelected();
			ContrApptSingle.SelectedAptNum=-1;
			if(row["AptStatus"].ToString()==((int)ApptStatus.UnschedList).ToString()) {//unscheduled status
				if(PIn.DateT(row["AptDateTime"].ToString()).Year<1880) {//Indicates that this was a brand new appt
					Appointment aptCur=Appointments.GetOneApt(PIn.Long(row["AptNum"].ToString()));
					if(aptCur.AptDateTime.Year>1880){//if date is now present
						//don't do anything to db.  Appt removed from pinboard above, and Refresh will happen below.
					}
					else{
						Appointments.Delete(PIn.Long(row["AptNum"].ToString()));
					}
				}
				else {//was actually on the unscheduled list
					//do nothing to database
				}
			}
			else if(PIn.DateT(row["AptDateTime"].ToString()).Year>1880) {//already scheduled
				//do nothing to database
			}
			else if(row["AptStatus"].ToString()==((int)ApptStatus.Planned).ToString()) {
				//do nothing except remove it from pinboard
			}
			else {//Not sure when this would apply, since new appts start out as unsched.  Maybe patient notes?  Leave it just in case.
				//this gets rid of new appointments that never made it off the pinboard
				Appointments.Delete(PIn.Long(row["AptNum"].ToString()));
			}
			if(pinBoard.SelectedIndex==-1) {
				if(PatCur==null) {
					//Do nothing
				}
				else {
					ModuleSelected(PatCur.PatNum);
				}
			}
			else {
				RefreshModuleDataPatient(PIn.Long(pinBoard.ApptList[pinBoard.SelectedIndex].DataRoww["PatNum"].ToString()));
				FormOpenDental.S_Contr_PatientSelected(PatCur,true,false);
				//RefreshModulePatient(PIn.PInt(pinBoard.ApptList[pinBoard.SelectedIndex].DataRoww["PatNum"].ToString()));
			}
		}

		///<summary>The scrollbar has been moved by the user.</summary>
		private void vScrollBar1_Scroll(object sender,System.Windows.Forms.ScrollEventArgs e) {
			if(e.Type==ScrollEventType.ThumbTrack) {//moving
				ContrApptSheet2.IsScrolling=true;
				ContrApptSheet2.Location=new Point(0,-e.NewValue);
			}
			if(e.Type==ScrollEventType.EndScroll) {//done moving
				ContrApptSheet2.IsScrolling=true;
				ContrApptSheet2.Location=new Point(0,-e.NewValue);
				ContrApptSheet2.IsScrolling=false;
				ContrApptSheet2.Select();
			}
		}

		///<summary>Occurs whenever the panel holding the appt sheet is resized.</summary>
		private void panelSheet_Resize(object sender,System.EventArgs e) {
			vScrollBar1.Maximum=ContrApptSheet2.Height-panelSheet.Height+vScrollBar1.LargeChange;
		}

		private void menuWeekly_Click(object sender,System.EventArgs e) {
			switch(((MenuItem)sender).Index) {
				case 0:
					OnCopyToPin_Click();
					break;
			}
		}

		private void menuApt_Click(object sender,System.EventArgs e) {
			switch(((MenuItem)sender).Index) {
				case 0:
					OnCopyToPin_Click();
					break;
				//1: divider
				case 2:
					OnUnsched_Click();
					break;
				case 3:
					OnBreak_Click();
					break;
				case 4:
					OnComplete_Click();
					break;
				case 5:
					OnDelete_Click();
					break;
				case 6:
					DisplayOtherDlg(false);
					break;
				//7: divider
				case 8:
					PrintApptLabel();
					break;
				case 9:
					cardPrintFamily=false;
					PrintApptCard();
					break;
				case 10:
					cardPrintFamily=true;
					PrintApptCard();
					break;
				case 11:
					//for now, this only allows one type of routing slip.  But it could be easily changed.
					Appointment apt=Appointments.GetOneApt(ContrApptSingle.ClickedAptNum);
					if(apt==null) {
						MsgBox.Show(this,"Selected appointment no longer exists.");
						RefreshModuleDataPeriod();
						RefreshModuleScreenPeriod();
						return;
					}
					FormRpRouting FormR=new FormRpRouting();
					FormR.AptNum=ContrApptSingle.ClickedAptNum;
					List<SheetDef> customSheetDefs=SheetDefs.GetCustomForType(SheetTypeEnum.RoutingSlip);
					if(customSheetDefs.Count==0) {
						FormR.SheetDefNum=0;
					}
					else {
						FormR.SheetDefNum=customSheetDefs[0].SheetDefNum;
					}
					FormR.ShowDialog();
					break;
			}
			//cannot use menu item index because some menu items may not exist
			switch(((MenuItem)sender).Name) {
				case "Ortho Chart": //Open Patient Ortho Chart
					FormOrthoChart FormOC=new FormOrthoChart(PatCur);
					FormOC.ShowDialog();
					break;
				case "Home Phone": //Call Home Phone
					if(Programs.GetCur(ProgramName.DentalTekSmartOfficePhone).Enabled) {
						DentalTek.PlaceCall(PatCur.HmPhone);
					}
					else {
						AutomaticCallDialingDisabledMessage();
					}
					break;
				case "Work Phone": //Call Work Phone
					if(Programs.GetCur(ProgramName.DentalTekSmartOfficePhone).Enabled) {
						DentalTek.PlaceCall(PatCur.WkPhone);
					}
					else {
						AutomaticCallDialingDisabledMessage();
					}
					break;
				case "Wireless Phone": //Call Wireless Phone
					if(Programs.GetCur(ProgramName.DentalTekSmartOfficePhone).Enabled) {
						DentalTek.PlaceCall(PatCur.WirelessPhone);
					}
					else {
						AutomaticCallDialingDisabledMessage();
					}
					break;
				case "Send Text":
					Appointment appt=Appointments.GetOneApt(ContrApptSingle.ClickedAptNum);
					if(appt==null) {
						MsgBox.Show(this,"Selected appointment no longer exists.");
						RefreshModuleDataPeriod();
						RefreshModuleScreenPeriod();
						return;
					}
					FormOpenDental.S_OnTxtMsg_Click(appt.PatNum,"");
					break;
				case "Send Confirmation Text":
					appt=Appointments.GetOneApt(ContrApptSingle.ClickedAptNum);
					if(appt==null) {
						MsgBox.Show(this,"Selected appointment no longer exists.");
						RefreshModuleDataPeriod();
						RefreshModuleScreenPeriod();
						return;
					}
					Patient pat=Patients.GetPat(appt.PatNum);
					string message=PrefC.GetString(PrefName.ConfirmTextMessage);
					message=message.Replace("[NameF]",pat.GetNameFirst());
					message=message.Replace("[NameFL]",pat.GetNameFL());
					message=message.Replace("[date]",appt.AptDateTime.ToShortDateString());
					message=message.Replace("[time]",appt.AptDateTime.ToShortTimeString());
					FormOpenDental.S_OnTxtMsg_Click(pat.PatNum,message);
					break;
			}
		}

		private void AutomaticCallDialingDisabledMessage() {
			if(ProgramProperties.IsAdvertisingDisabled(ProgramName.DentalTekSmartOfficePhone)) {
				return;
			}
			MessageBox.Show(Lan.g(this,"Automatic dialing of patient phone numbers requires an additional service")+".\r\n"
							+Lan.g(this,"Contact Open Dental for more information")+".");
			try {
				Process.Start("http://www.opendental.com/manual/dentaltekinfo.html");
			}
			catch(Exception) {
				MessageBox.Show(Lan.g(this,"Could not find")+" http://www.opendental.com/contact.html" + "\r\n"
							+Lan.g(this,"Please set up a default web browser."));
			}
		}

		///<summary>Sends current appointment to unscheduled list.</summary>
		private void butUnsched_Click(object sender,System.EventArgs e) {
			OnUnsched_Click();
		}

		private void butBreak_Click(object sender,System.EventArgs e) {
			OnBreak_Click();
		}

		private void butComplete_Click(object sender,System.EventArgs e) {
			OnComplete_Click();
		}

		private void butDelete_Click(object sender,System.EventArgs e) {
			OnDelete_Click();
		}

		private void butMakeAppt_Click(object sender,System.EventArgs e) {
			if(PatCur==null) {
				return;
			}
			if(!Security.IsAuthorized(Permissions.AppointmentCreate)) {
				return;
			}
			if(PatRestrictions.IsRestricted(PatCur.PatNum,PatRestrict.ApptSchedule)) {
				return;
			}
			if(Appointments.HasOutsandingAppts(PatCur.PatNum)) {
				DisplayOtherDlg(false);
				return;
			}
			FormApptsOther FormAO=new FormApptsOther(PatCur.PatNum);//doesn't actually get shown
			CheckStatus();
			FormAO.InitialClick=false;
			FormAO.MakeAppointment();
			SendToPinboard(FormAO.AptNumsSelected);
		}

		private void butMakeRecall_Click(object sender,EventArgs e) {
			if(PatCur==null) {
				return;
			}
			if(!Security.IsAuthorized(Permissions.AppointmentCreate)) {
				return;
			}
			if(PatRestrictions.IsRestricted(PatCur.PatNum,PatRestrict.ApptSchedule)) {
				return;
			}
			if(Appointments.HasOutsandingAppts(PatCur.PatNum,true)) {
				DisplayOtherDlg(false);
				return;
			}
			FormApptsOther FormAO=new FormApptsOther(PatCur.PatNum);//doesn't actually get shown
			FormAO.InitialClick=false;
			FormAO.MakeRecallAppointment();
			if(FormAO.DialogResult!=DialogResult.OK) {
				return;
			}
			SendToPinboard(FormAO.AptNumsSelected);
			if(ApptDrawing.IsWeeklyView) {
				return;
			}
			dateSearch.Text=FormAO.DateJumpToString;
			if(!groupSearch.Visible) {//if search not already visible
				ShowSearch();
			}
			DoSearch();
		}

		private void butFamRecall_Click(object sender,EventArgs e) {
			if(PatCur==null) {
				return;
			}
			if(!Security.IsAuthorized(Permissions.AppointmentCreate)) {
				return;
			}
			if(Appointments.HasOutsandingAppts(PatCur.PatNum)) {
				DisplayOtherDlg(false);
				return;
			}
			FormApptsOther FormAO=new FormApptsOther(PatCur.PatNum);//doesn't actually get shown
			FormAO.InitialClick=false;
			FormAO.MakeRecallFamily();
			if(FormAO.DialogResult!=DialogResult.OK) {
				return;
			}
			SendToPinboard(FormAO.AptNumsSelected);
			if(ApptDrawing.IsWeeklyView) {
				return;
			}
			dateSearch.Text=FormAO.DateJumpToString;
			if(!groupSearch.Visible) {//if search not already visible
				ShowSearch();
			}
			DoSearch();
		}

		private void butViewAppts_Click(object sender,EventArgs e) {
			DisplayOtherDlg(false);
		}

		/*private void MakeAppointment(bool toPinboard) {
			if(toPinboard) {
				SendToPinBoard(FormAO.AptNumsSelected);
				if(ApptDrawing.IsWeeklyView) {
					break;
				}
				dateSearch.Text=FormAO.DateJumpToString;
				if(!groupSearch.Visible) {//if search not already visible
					ShowSearch();
				}
				DoSearch();
			}
			else {
				RefreshModuleDataPatient(PatCur.PatNum);
				OnPatientSelected(PatCur.PatNum,PatCur.GetNameLF(),PatCur.Email!="",PatCur.ChartNumber);
				//RefreshModulePatient(FormAO.SelectedPatNum);
				Appointment apt=Appointments.GetOneApt(ContrApptSingle.SelectedAptNum);
				if(apt!=null && DoesOverlap(apt)) {
					Appointment aptOld=apt.Clone();
					MsgBox.Show(this,"Appointment is too long and would overlap another appointment.  Automatically shortened to fit.");
					while(DoesOverlap(apt)) {
						apt.Pattern=apt.Pattern.Substring(0,apt.Pattern.Length-1);
						if(apt.Pattern.Length==1) {
							break;
						}
					}
					try {
						Appointments.Update(apt,aptOld);
					}
					catch(ApplicationException ex) {
						MessageBox.Show(ex.Message);
					}
				}
				RefreshPeriod();
				SetInvalid();
			}
		}*/

		private void OnUnsched_Click() {
			if(ContrApptSingle.SelectedAptNum==-1) {
				MsgBox.Show(this,"Please select an appointment first.");
				return;
			}
			Appointment apt = Appointments.GetOneApt(ContrApptSingle.SelectedAptNum);
			if(apt==null) {
				MsgBox.Show(this,"Selected appointment no longer exists.");
				RefreshModuleDataPeriod();
				RefreshModuleScreenPeriod();
				return;
			}
			if((apt.AptStatus!=ApptStatus.Complete && !Security.IsAuthorized(Permissions.AppointmentMove)) //seperate permissions for complete appts.
				|| (apt.AptStatus==ApptStatus.Complete && !Security.IsAuthorized(Permissions.AppointmentCompleteEdit))) {
				return;
			}
			if(PatRestrictions.IsRestricted(apt.PatNum,PatRestrict.ApptSchedule)) {
				return;
			}
			if(apt.AptStatus == ApptStatus.PtNote | apt.AptStatus == ApptStatus.PtNoteCompleted) {
				return;
			}
			if(MessageBox.Show(Lan.g(this,"Send Appointment to Unscheduled List?")
				,"",MessageBoxButtons.OKCancel)!=DialogResult.OK) {
				return;
			}
			int thisI=GetIndex(ContrApptSingle.SelectedAptNum);
			if(thisI==-1) {//selected appt is on a different day
				MsgBox.Show(this,"Please select an appointment first.");
				return;
			}
			Appointments.SetAptStatus(ContrApptSingle.SelectedAptNum,ApptStatus.UnschedList);
			Patient pat=Patients.GetPat(PIn.Long(ContrApptSingle3[thisI].DataRoww["PatNum"].ToString()));
			if(apt.AptStatus!=ApptStatus.Complete) { //seperate log entry for editing completed appts.
				SecurityLogs.MakeLogEntry(Permissions.AppointmentMove,pat.PatNum, 
					ContrApptSingle3[thisI].DataRoww["procs"].ToString()+", "+ContrApptSingle3[thisI].DataRoww["AptDateTime"].ToString()+", Sent to Unscheduled List",
					PIn.Long(ContrApptSingle3[thisI].DataRoww["AptNum"].ToString()));
			}
			else {
				SecurityLogs.MakeLogEntry(Permissions.AppointmentCompleteEdit,pat.PatNum,
					ContrApptSingle3[thisI].DataRoww["procs"].ToString()+", "+ContrApptSingle3[thisI].DataRoww["AptDateTime"].ToString()+", Sent to Unscheduled List",
					PIn.Long(ContrApptSingle3[thisI].DataRoww["AptNum"].ToString()));
			}
			//If there is an existing HL7 def enabled, send a SIU message if there is an outbound SIU message defined
			if(HL7Defs.IsExistingHL7Enabled()) {
				//S15 - Appt Cancellation event
				MessageHL7 messageHL7=MessageConstructor.GenerateSIU(pat,Patients.GetPat(pat.Guarantor),EventTypeHL7.S15,apt);
				//Will be null if there is no outbound SIU message defined, so do nothing
				if(messageHL7!=null) {
					HL7Msg hl7Msg=new HL7Msg();
					hl7Msg.AptNum=apt.AptNum;
					hl7Msg.HL7Status=HL7MessageStatus.OutPending;//it will be marked outSent by the HL7 service.
					hl7Msg.MsgText=messageHL7.ToString();
					hl7Msg.PatNum=pat.PatNum;
					HL7Msgs.Insert(hl7Msg);
#if DEBUG
					MessageBox.Show(this,messageHL7.ToString());
#endif
				}
			}
			ModuleSelected(pat.PatNum);
			Signalods.SetInvalidAppt(apt);
			Recalls.SynchScheduledApptFull(apt.PatNum);
		}

		private void OnBreak_Click() {
			if(PrefC.GetBool(PrefName.BrokenApptAdjustment) 
				&& PrefC.GetLong(PrefName.BrokenAppointmentAdjustmentType)==0) 
			{
				//They want broken appointment adjustments but don't have it set up.
				MsgBox.Show(this,"Broken appointment adjustment type is not setup yet.  Please go to Setup | Appointment | Appts Preferences to fix this.");
				return;
			}
			int thisI=GetIndex(ContrApptSingle.SelectedAptNum);
			if(thisI==-1) {//selected appt is on a different day
				MsgBox.Show(this,"Please select an appointment first.");
				return;
			}
			ContrApptSingle apptSingle=ContrApptSingle3[thisI];
			Appointment apt=Appointments.GetOneApt(ContrApptSingle.SelectedAptNum);
			if(apt==null) {
				MsgBox.Show(this,"Selected appointment no longer exists.");
				RefreshModuleDataPeriod();
				RefreshModuleScreenPeriod();
				return;
			}
			Patient pat=Patients.GetPat(PIn.Long(apptSingle.DataRoww["PatNum"].ToString()));
			if((apt.AptStatus!=ApptStatus.Complete && !Security.IsAuthorized(Permissions.AppointmentEdit)) //seperate permissions for completed appts.
				|| (apt.AptStatus==ApptStatus.Complete && !Security.IsAuthorized(Permissions.AppointmentCompleteEdit))) 
			{
				return;
			}
			if(apt.AptStatus == ApptStatus.PtNote || apt.AptStatus == ApptStatus.PtNoteCompleted) {
				MsgBox.Show(this,"Only appointments may be broken, not notes.");
				return;
			}
			if(!MsgBox.Show(this,true,"Break appointment?")) {
				return;
			}
			Appointments.SetAptStatus(ContrApptSingle.SelectedAptNum,ApptStatus.Broken);
			if(apt.AptStatus!=ApptStatus.Complete) { //seperate log entry for completed appointments.
				SecurityLogs.MakeLogEntry(Permissions.AppointmentEdit,pat.PatNum,
					apptSingle.DataRoww["procs"].ToString()+", "+apptSingle.DataRoww["AptDateTime"].ToString()
					+", Broken from the Appts module.",PIn.Long(apptSingle.DataRoww["AptNum"].ToString()));
			}
			else {
				SecurityLogs.MakeLogEntry(Permissions.AppointmentCompleteEdit,pat.PatNum,
					apptSingle.DataRoww["procs"].ToString()+", "+apptSingle.DataRoww["AptDateTime"].ToString()
					+", Broken from the Appts module.",PIn.Long(apptSingle.DataRoww["AptNum"].ToString()));
			}
			//If there is an existing HL7 def enabled, send a SIU message if there is an outbound SIU message defined
			if(HL7Defs.IsExistingHL7Enabled()) {
				//S15 - Appt Cancellation event
				MessageHL7 messageHL7=MessageConstructor.GenerateSIU(pat,Patients.GetPat(pat.Guarantor),EventTypeHL7.S15,apt);
				//Will be null if there is no outbound SIU message defined, so do nothing
				if(messageHL7!=null) {
					HL7Msg hl7Msg=new HL7Msg();
					hl7Msg.AptNum=apt.AptNum;
					hl7Msg.HL7Status=HL7MessageStatus.OutPending;//it will be marked outSent by the HL7 service.
					hl7Msg.MsgText=messageHL7.ToString();
					hl7Msg.PatNum=pat.PatNum;
					HL7Msgs.Insert(hl7Msg);
#if DEBUG
					MessageBox.Show(this,messageHL7.ToString());
#endif
				}
			}
			long provNum=PIn.Long(apptSingle.DataRoww["ProvNum"].ToString());//remember before ModuleSelected
			//If broken appointment procedure automation is enabled, create a D9986 procedure. 
			//The D9986 procedurecode should always exist if BrokenApptProcedure preference is true.											
			if(PrefC.GetBool(PrefName.BrokenApptProcedure)) {
				ProcedureCode procCodeBrokenApt=ProcedureCodes.GetProcCode("D9986");
				Procedure procedureCur=new Procedure();
				procedureCur.PatNum=pat.PatNum;
				procedureCur.ProvNum=provNum;
				procedureCur.CodeNum=procCodeBrokenApt.CodeNum;
				procedureCur.ProcDate=DateTime.Today;
				procedureCur.DateEntryC=DateTime.Now;
				procedureCur.ProcStatus=ProcStat.C;
				procedureCur.ClinicNum=apt.ClinicNum;
				procedureCur.UserNum=Security.CurUser.UserNum;
				procedureCur.Note=Lan.g(this,"Appt BROKEN for")+" "+apt.ProcDescript+"  "+apt.AptDateTime.ToString();
				procedureCur.PlaceService=(PlaceOfService)PrefC.GetLong(PrefName.DefaultProcedurePlaceService);//Default Proc Place of Service for the Practice is used. 
				List<InsSub> listInsSubs=InsSubs.RefreshForFam(Patients.GetFamily(pat.PatNum));
				List<InsPlan> listInsPlans=InsPlans.RefreshForSubList(listInsSubs);
				List<PatPlan> listPatPlans=PatPlans.Refresh(pat.PatNum);
				InsPlan insPlanPrimary=null;
				InsSub insSubPrimary=null;
				if(listPatPlans.Count>0) {
					insSubPrimary=InsSubs.GetSub(listPatPlans[0].InsSubNum,listInsSubs);
					insPlanPrimary=InsPlans.GetPlan(insSubPrimary.PlanNum,listInsPlans);
				}
				double procFee;
				long feeSch;
				if(insPlanPrimary==null || procCodeBrokenApt.NoBillIns) {
					feeSch=Fees.GetFeeSched(0,pat.FeeSched,procedureCur.ProvNum);
				}
				else {//Only take into account the patient's insurance fee schedule if the D9986 procedure is not marked as NoBillIns
					feeSch=Fees.GetFeeSched(insPlanPrimary.FeeSched,pat.FeeSched,procedureCur.ProvNum);
				}
				procFee=Fees.GetAmount0(procedureCur.CodeNum,feeSch,procedureCur.ClinicNum,procedureCur.ProvNum);//Will be 0 if no fee schedule was found.
				if(insPlanPrimary!=null && insPlanPrimary.PlanType=="p" && !insPlanPrimary.IsMedical) {//PPO
					double provFee=Fees.GetAmount0(procedureCur.CodeNum,Providers.GetProv(procedureCur.ProvNum).FeeSched,procedureCur.ClinicNum,
						procedureCur.ProvNum);
					procedureCur.ProcFee=Math.Max(provFee,procFee);
				}
				else {
					procedureCur.ProcFee=procFee;
				}
				if(!PrefC.GetBool(PrefName.EasyHidePublicHealth)) {
					procedureCur.SiteNum=pat.SiteNum;
				}
				Procedures.Insert(procedureCur);
				//Now make a claimproc if the patient has insurance.  We do this now for consistency because a claimproc could get created in the future.
				List<Benefit> listBenefits=Benefits.Refresh(listPatPlans,listInsSubs);
				List<ClaimProc> listClaimProcsForProc=ClaimProcs.RefreshForProc(procedureCur.ProcNum);
				Procedures.ComputeEstimates(procedureCur,pat.PatNum,listClaimProcsForProc,false,listInsPlans,listPatPlans,listBenefits,pat.Age,listInsSubs);
				FormProcBroken FormPB=new FormProcBroken(procedureCur);
				FormPB.IsNew=true;
				FormPB.ShowDialog();
			}
			if(PrefC.GetBool(PrefName.BrokenApptAdjustment)) {
				Adjustment AdjustmentCur=new Adjustment();
				AdjustmentCur.DateEntry=DateTime.Today;
				AdjustmentCur.AdjDate=DateTime.Today;
				AdjustmentCur.ProcDate=DateTime.Today;
				AdjustmentCur.ProvNum=provNum;
				AdjustmentCur.PatNum=pat.PatNum;
				AdjustmentCur.AdjType=PrefC.GetLong(PrefName.BrokenAppointmentAdjustmentType);
				AdjustmentCur.ClinicNum=apt.ClinicNum;
				FormAdjust FormA=new FormAdjust(pat,AdjustmentCur);
				FormA.IsNew=true;
				FormA.ShowDialog();
			}
			if(PrefC.GetBool(PrefName.BrokenApptCommLog)) {
				Commlog CommlogCur=new Commlog();
				CommlogCur.PatNum=pat.PatNum;
				CommlogCur.CommDateTime=DateTime.Now;
				CommlogCur.CommType=Commlogs.GetTypeAuto(CommItemTypeAuto.APPT);
				CommlogCur.Note=Lan.g(this,"Appt BROKEN for")+" "+apt.ProcDescript+"  "+apt.AptDateTime.ToString();
				CommlogCur.Mode_=CommItemMode.None;
				CommlogCur.UserNum=Security.CurUser.UserNum;
				FormCommItem FormCI=new FormCommItem(CommlogCur);
				FormCI.IsNew=true;
				FormCI.ShowDialog();
			}
			ModuleSelected(pat.PatNum);//Must be ran after the "D9986" break logic due to the addition of a completed procedure.
			Signalods.SetInvalidAppt(apt);
			AutomationL.Trigger(AutomationTrigger.BreakAppointment,null,pat.PatNum);
			Recalls.SynchScheduledApptFull(apt.PatNum);
		}

		private void OnComplete_Click() {
			if(!Security.IsAuthorized(Permissions.AppointmentEdit)) {
				return;
			}
			int thisI=GetIndex(ContrApptSingle.SelectedAptNum);
			if(thisI==-1) {//selected appt is on a different day
				MsgBox.Show(this,"Please select an appointment first.");
				return;
			}
			Appointment apt = Appointments.GetOneApt(ContrApptSingle.SelectedAptNum);
			if(apt==null) {
				MsgBox.Show(this,"Selected appointment no longer exists.");
				RefreshModuleDataPeriod();
				RefreshModuleScreenPeriod();
				return;
			}
			if(apt.AptDateTime.Date>DateTime.Today) {
				if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Appointment is in the future.  Set complete anyway?")) {
					return;
				}
			}
			if(apt.AptStatus == ApptStatus.PtNoteCompleted) {
				return;
			}
			if(!Security.IsAuthorized(Permissions.ProcComplCreate,apt.AptDateTime)) {
				return;
			}
			//Procedures.SetDateFirstVisit(Appointments.Cur.AptDateTime.Date);//done when making appt instead
			Family fam = Patients.GetFamily(apt.PatNum);
			Patient pat = fam.GetPatient(apt.PatNum);
			List<InsSub> SubList=InsSubs.RefreshForFam(fam);
			List<InsPlan> PlanList=InsPlans.RefreshForSubList(SubList);
			List<PatPlan> PatPlanList = PatPlans.Refresh(apt.PatNum);
			if(apt.AptStatus == ApptStatus.PtNote) {
				Appointments.SetAptStatus(apt.AptNum,ApptStatus.PtNoteCompleted);
				SecurityLogs.MakeLogEntry(Permissions.AppointmentEdit,apt.PatNum,
					apt.AptDateTime.ToString()+", Patient NOTE Set Complete",
					apt.AptNum);//shouldn't ever happen, but don't allow procedures to be completed from notes
			}
			else {
				InsSub sub1=InsSubs.GetSub(PatPlans.GetInsSubNum(PatPlanList,PatPlans.GetOrdinal(PriSecMed.Primary,PatPlanList,PlanList,SubList)),SubList);
				InsSub sub2=InsSubs.GetSub(PatPlans.GetInsSubNum(PatPlanList,PatPlans.GetOrdinal(PriSecMed.Secondary,PatPlanList,PlanList,SubList)),SubList);
				Appointments.SetAptStatusComplete(apt.AptNum,sub1.PlanNum,sub2.PlanNum);
				ProcedureL.SetCompleteInAppt(apt,PlanList,PatPlanList,pat.SiteNum,pat.Age,SubList);//loops through each proc
				if(apt.AptStatus!=ApptStatus.Complete) { // seperate log entry for editing completed appointments.
					SecurityLogs.MakeLogEntry(Permissions.AppointmentEdit,apt.PatNum,
						ContrApptSingle3[GetIndex(apt.AptNum)].DataRoww["procs"].ToString()+", "+ apt.AptDateTime.ToString()+", Set Complete",
						apt.AptNum);//Log showing the appt. is set complete
				}
				else {
					SecurityLogs.MakeLogEntry(Permissions.AppointmentCompleteEdit,apt.PatNum,
						ContrApptSingle3[GetIndex(apt.AptNum)].DataRoww["procs"].ToString()+", "+ apt.AptDateTime.ToString()+", Set Complete",
						apt.AptNum);//Log showing the appt. is set complete
				}
				//If there is an existing HL7 def enabled, send a SIU message if there is an outbound SIU message defined
				if(HL7Defs.IsExistingHL7Enabled()) {
					//S14 - Appt Modification event
					MessageHL7 messageHL7=MessageConstructor.GenerateSIU(pat,fam.GetPatient(pat.Guarantor),EventTypeHL7.S14,apt);
					//Will be null if there is no outbound SIU message defined, so do nothing
					if(messageHL7!=null) {
						HL7Msg hl7Msg=new HL7Msg();
						hl7Msg.AptNum=apt.AptNum;
						hl7Msg.HL7Status=HL7MessageStatus.OutPending;//it will be marked outSent by the HL7 service.
						hl7Msg.MsgText=messageHL7.ToString();
						hl7Msg.PatNum=pat.PatNum;
						HL7Msgs.Insert(hl7Msg);
#if DEBUG
						MessageBox.Show(this,messageHL7.ToString());
#endif
					}
				}
			}
			Recalls.SynchScheduledApptFull(apt.PatNum);
			ModuleSelected(pat.PatNum);
			Signalods.SetInvalidAppt(apt);
		}

		private void OnDelete_Click() {
			long selectedAptNum=ContrApptSingle.SelectedAptNum;
			Appointment apt = Appointments.GetOneApt(selectedAptNum);
			if(apt==null) {
				MsgBox.Show(this,"Selected appointment no longer exists.");
				RefreshModuleDataPeriod();
				RefreshModuleScreenPeriod();
				return;
			}
			if((apt.AptStatus!=ApptStatus.Complete && !Security.IsAuthorized(Permissions.AppointmentEdit)) //seperate permission for completed appts.
				|| (apt.AptStatus==ApptStatus.Complete && !Security.IsAuthorized(Permissions.AppointmentCompleteEdit))) 
			{
				return;
			}
			int thisI=GetIndex(selectedAptNum);
			if(thisI==-1) {//selected appt is on a different day
				MsgBox.Show(this,"Please select an appointment first.");
				return;
			}
			if(apt.AptStatus == ApptStatus.PtNote | apt.AptStatus == ApptStatus.PtNoteCompleted) {
				if(!MsgBox.Show(this,true,"Delete Patient Note?")) {
					return;
				}
				if(apt.Note != "") {
					if(MessageBox.Show(Commlogs.GetDeleteApptCommlogMessage(apt.Note,apt.AptStatus),"Question...",MessageBoxButtons.YesNo) == DialogResult.Yes) {
						Commlog CommlogCur = new Commlog();
						CommlogCur.PatNum = apt.PatNum;
						CommlogCur.CommDateTime = DateTime.Now;
						CommlogCur.CommType =Commlogs.GetTypeAuto(CommItemTypeAuto.APPT);
						CommlogCur.Note = "Deleted Patient NOTE from schedule, saved copy: ";
						CommlogCur.Note += apt.Note;
						CommlogCur.UserNum=Security.CurUser.UserNum;
						//there is no dialog here because it is just a simple entry
						Commlogs.Insert(CommlogCur);
					}
				}
				SecurityLogs.MakeLogEntry(Permissions.AppointmentEdit,PatCur.PatNum,
					ContrApptSingle3[thisI].DataRoww["procs"].ToString()+", "+ContrApptSingle3[thisI].DataRoww["AptDateTime"].ToString()+", "+"NOTE Deleted",
					PIn.Long(ContrApptSingle3[thisI].DataRoww["AptNum"].ToString()));
			}
			else {
				if(!MsgBox.Show(this,true,"Delete Appointment?")) {
					return;
				}
				if(apt.Note != "") {
					if(MessageBox.Show(Commlogs.GetDeleteApptCommlogMessage(apt.Note,apt.AptStatus),"Question...",MessageBoxButtons.YesNo) == DialogResult.Yes) {
						Commlog CommlogCur = new Commlog();
						CommlogCur.PatNum = apt.PatNum;
						CommlogCur.CommDateTime = DateTime.Now;
						CommlogCur.CommType =Commlogs.GetTypeAuto(CommItemTypeAuto.APPT);
						CommlogCur.Note = "Deleted Appointment & saved note: ";
						if(apt.ProcDescript != "") {
							CommlogCur.Note += apt.ProcDescript + ": ";
						}
						CommlogCur.Note += apt.Note;
						CommlogCur.UserNum=Security.CurUser.UserNum;
						//there is no dialog here because it is just a simple entry
						Commlogs.Insert(CommlogCur);
					}
				}
				if(apt.AptStatus!=ApptStatus.Complete) {// seperate log entry for editing completed appointments.
					SecurityLogs.MakeLogEntry(Permissions.AppointmentEdit,PatCur.PatNum,
						ContrApptSingle3[thisI].DataRoww["procs"].ToString()+", "+ContrApptSingle3[thisI].DataRoww["AptDateTime"].ToString()+", "+"Deleted",
						PIn.Long(ContrApptSingle3[thisI].DataRoww["AptNum"].ToString()));
				}
				else {
					SecurityLogs.MakeLogEntry(Permissions.AppointmentCompleteEdit,PatCur.PatNum,
						ContrApptSingle3[thisI].DataRoww["procs"].ToString()+", "+ContrApptSingle3[thisI].DataRoww["AptDateTime"].ToString()+", "+"Deleted",
						PIn.Long(ContrApptSingle3[thisI].DataRoww["AptNum"].ToString()));
				}
				//If there is an existing HL7 def enabled, send a SIU message if there is an outbound SIU message defined
				if(HL7Defs.IsExistingHL7Enabled()) {
					//S17 - Appt Deletion event
					MessageHL7 messageHL7=MessageConstructor.GenerateSIU(PatCur,Patients.GetPat(PatCur.Guarantor),EventTypeHL7.S17,apt);
					//Will be null if there is no outbound SIU message defined, so do nothing
					if(messageHL7!=null) {
						HL7Msg hl7Msg=new HL7Msg();
						hl7Msg.AptNum=apt.AptNum;
						hl7Msg.HL7Status=HL7MessageStatus.OutPending;//it will be marked outSent by the HL7 service.
						hl7Msg.MsgText=messageHL7.ToString();
						hl7Msg.PatNum=PatCur.PatNum;
						HL7Msgs.Insert(hl7Msg);
#if DEBUG
						MessageBox.Show(this,messageHL7.ToString());
#endif
					}
				}
			}
			Appointments.Delete(selectedAptNum);
			ContrApptSingle.SelectedAptNum=-1;
			pinBoard.SelectedIndex=-1;
			DataRow row;
			for(int i=0;i<pinBoard.ApptList.Count;i++) {
				row=pinBoard.ApptList[i].DataRoww;
				if(selectedAptNum.ToString()==row["AptNum"].ToString()) {
					pinBoard.SelectedIndex=i;
					pinBoard.ClearSelected();
					pinBoard.SelectedIndex=-1;
				}
			}
			//ContrApptSingle.PinBoardIsSelected=false;
			//PatCurNum=0;
			if(PatCur==null) {
				ModuleSelected(0);
			}
			else {
				ModuleSelected(PatCur.PatNum);
			}
			Signalods.SetInvalidAppt(apt);
			Recalls.SynchScheduledApptFull(apt.PatNum);
		}

		private void PrintApptLabel() {
			Appointment apt=Appointments.GetOneApt(ContrApptSingle.SelectedAptNum);
			if(apt==null) {
				MsgBox.Show(this,"Selected appointment no longer exists.");
				RefreshModuleDataPeriod();
				RefreshModuleScreenPeriod();
				return;
			}
			LabelSingle.PrintAppointment(ContrApptSingle.SelectedAptNum);
		}

		private void OnBlockCopy_Click(object sender,EventArgs e) {
			if(!Security.IsAuthorized(Permissions.Blockouts)) {
				return;
			}
			//not even enabled if not right click on a blockout
			Schedule SchedCur=GetClickedBlockout();
			if(SchedCur==null) {
				MessageBox.Show("Blockout not found.");
				return;//should never happen
			}
			BlockoutClipboard=SchedCur.Copy();
		}

		private void OnBlockCut_Click(object sender,EventArgs e) {
			if(!Security.IsAuthorized(Permissions.Blockouts)) {
				return;
			}
			//not even enabled if not right click on a blockout
			Schedule SchedCur=GetClickedBlockout();
			if(SchedCur==null) {
				MessageBox.Show("Blockout not found.");
				return;//should never happen
			}
			BlockoutClipboard=SchedCur.Copy();
			Schedules.Delete(SchedCur);
			SecurityLogs.MakeLogEntry(Permissions.Blockouts,0,"Blockout cut.");
			RefreshPeriod();
		}

		private void OnBlockPaste_Click(object sender,EventArgs e) {
			if(!Security.IsAuthorized(Permissions.Blockouts)) {
				return;
			}
			Schedule sched=BlockoutClipboard.Copy();
			sched.Ops=new List<long>();
			sched.Ops.Add(SheetClickedonOp);
			sched.SchedDate=AppointmentL.DateSelected;
			if(ApptDrawing.IsWeeklyView) {
				sched.SchedDate=WeekStartDate.AddDays(SheetClickedonDay);
			}
			TimeSpan span=sched.StopTime-sched.StartTime;
			TimeSpan timeOfDay=new TimeSpan(SheetClickedonHour,SheetClickedonMin,0);
			timeOfDay=TimeSpan.FromMinutes(
				((int)Math.Round((decimal)timeOfDay.TotalMinutes/(decimal)ApptDrawing.MinPerIncr))*ApptDrawing.MinPerIncr);
			sched.StartTime=timeOfDay;
			sched.StopTime=sched.StartTime.Add(span);
			if(sched.StopTime >= TimeSpan.FromDays(1)) {//long span that spills over to next day
				MsgBox.Show(this,"This Blockout would go past midnight.");
				return;
			}
			sched.ScheduleNum=0;//Because Schedules.Overlaps() ignores matching ScheduleNums and we used the Copy() function above. Also, we insert below, so a new key will be created anyway.
			if(Schedules.Overlaps(sched)) {
				MsgBox.Show(this,"Blockouts not allowed to overlap.");
				return;
			}
			Schedules.Insert(sched,true);
			SecurityLogs.MakeLogEntry(Permissions.Blockouts,0,"Blockout paste.");
			RefreshPeriod();
		}

		private void OnBlockEdit_Click(object sender,EventArgs e) {
			if(!Security.IsAuthorized(Permissions.Blockouts)) {
				return;
			}
			//not even enabled if not right click on a blockout
			Schedule SchedCur=GetClickedBlockout();
			if(SchedCur==null) {
				MessageBox.Show("Blockout not found.");
				return;//should never happen
			}
			FormScheduleBlockEdit FormSB=new FormScheduleBlockEdit(SchedCur,Clinics.ClinicNum);
			FormSB.ShowDialog();
			RefreshPeriod();
		}

		private Schedule GetClickedBlockout() {
			DateTime startDate;
			DateTime endDate;
			if(ApptDrawing.IsWeeklyView) {
				startDate=WeekStartDate;
				endDate=WeekEndDate;
			}
			else {
				startDate=AppointmentL.DateSelected;
				endDate=AppointmentL.DateSelected;
			}
			//no need to do this since schedule is refreshed in RefreshPeriod().
			//SchedListPeriod=Schedules.RefreshPeriod(startDate,endDate);
			Schedule[] ListForType=Schedules.GetForType(SchedListPeriod,ScheduleType.Blockout,0);
			//now find which blockout
			Schedule SchedCur=null;
			//date is irrelevant. This is just for the time:
			DateTime SheetClickedonTime=new DateTime(2000,1,1,SheetClickedonHour,SheetClickedonMin,0);
			for(int i=0;i<ListForType.Length;i++) {
				//skip if op doesn't match
				if(!ListForType[i].Ops.Contains(SheetClickedonOp)) {
					continue;
				}
				if(ListForType[i].SchedDate.Date!=WeekStartDate.AddDays(SheetClickedonDay)) {
					continue;
				}
				if(ListForType[i].StartTime <= SheetClickedonTime.TimeOfDay
					&& SheetClickedonTime.TimeOfDay < ListForType[i].StopTime) {
					SchedCur=ListForType[i];
					break;
				}
			}
			return SchedCur;//might be null;
		}

		private void OnBlockDelete_Click(object sender,EventArgs e) {
			if(!Security.IsAuthorized(Permissions.Blockouts)) {
				return;
			}
			Schedule SchedCur=GetClickedBlockout();
			if(SchedCur==null) {
				MessageBox.Show("Blockout not found.");
				return;//should never happen
			}
			Schedules.Delete(SchedCur);
			SecurityLogs.MakeLogEntry(Permissions.Blockouts,0,"Blockout delete.");
			RefreshPeriod();
		}

		private void OnBlockAdd_Click(object sender,EventArgs e) {
			if(!Security.IsAuthorized(Permissions.Blockouts)) {
				return;
			}
			Schedule SchedCur=new Schedule();
			SchedCur.SchedDate=AppointmentL.DateSelected;
			//Get the closest time in regards to the Appt View Time Increment preference.  Round the time down.
			int minutes=(int)((ContrAppt.SheetClickedonMin/ApptDrawing.MinPerIncr)*ApptDrawing.MinPerIncr);
			SchedCur.StartTime=new TimeSpan(ContrAppt.SheetClickedonHour,minutes,0);
			SchedCur.StopTime=new TimeSpan(ContrAppt.SheetClickedonHour+1,minutes,0);
			if(SchedCur.StartTime>TimeSpan.FromHours(23)) {//if user clicked anywhere during the last hour of the day, set blockout to the last hour of the day.
				SchedCur.StartTime=new TimeSpan(23,00,00);
				SchedCur.StopTime=new TimeSpan(23,59,00);
			}
			if(ApptDrawing.IsWeeklyView) {
				SchedCur.SchedDate=WeekStartDate.AddDays(SheetClickedonDay);
			}
			SchedCur.SchedType=ScheduleType.Blockout;
			FormScheduleBlockEdit FormSB=new FormScheduleBlockEdit(SchedCur,Clinics.ClinicNum);
			FormSB.IsNew=true;
			FormSB.ShowDialog();
			RefreshPeriod();
		}

		private void OnBlockCutCopyPaste_Click(object sender,EventArgs e) {
			if(!Security.IsAuthorized(Permissions.Blockouts)) {
				return;
			}
			FormBlockoutCutCopyPaste FormB=new FormBlockoutCutCopyPaste();
			FormB.DateSelected=AppointmentL.DateSelected;
			if(ApptDrawing.IsWeeklyView) {
				FormB.DateSelected=WeekStartDate.AddDays(SheetClickedonDay);
			}
			if(comboView.SelectedIndex==0) {
				FormB.ApptViewNumCur=0;
			}
			else {
				FormB.ApptViewNumCur=_listApptViews[comboView.SelectedIndex-1].ApptViewNum;
			}
			FormB.ShowDialog();
			SecurityLogs.MakeLogEntry(Permissions.Blockouts,0,"Blockout cut copy paste.");
			RefreshPeriod();
		}

		private void OnClearBlockouts_Click(object sender,EventArgs e) {
			if(!Security.IsAuthorized(Permissions.Blockouts)) {
				return;
			}
			if(!MsgBox.Show(this,true,"Clear all blockouts for day?")) {
				return;
			}
			if(ApptDrawing.IsWeeklyView) {
				Schedules.ClearBlockoutsForDay(WeekStartDate.AddDays(SheetClickedonDay));
			}
			else {
				Schedules.ClearBlockoutsForDay(AppointmentL.DateSelected);
			}
			SecurityLogs.MakeLogEntry(Permissions.Blockouts,0,"Blockout clear.");
			RefreshPeriod();
		}

		private void OnClearBlockoutsOp_Click(object sender,EventArgs e) {
			if(!Security.IsAuthorized(Permissions.Blockouts)) {
				return;
			}
			if(!MsgBox.Show(this,true,"Clear all blockouts for day and operatory?")) {
				return;
			}
			DateTime dateClear=AppointmentL.DateSelected;
			if(ApptDrawing.IsWeeklyView) {
				dateClear=WeekStartDate.AddDays(SheetClickedonDay);
			}
			Schedules.ClearBlockoutsForOp(SheetClickedonOp,dateClear);
			SecurityLogs.MakeLogEntry(Permissions.Blockouts,0,"Blockout clear operatory: "+Operatories.GetAbbrev(SheetClickedonOp));
			RefreshPeriod();
		}

		private void OnClearBlockoutsClinic_Click(object sender,EventArgs e) {
			if(!Security.IsAuthorized(Permissions.Blockouts)) {
				return;
			}
			if(!MsgBox.Show(this,true,"Clear all blockouts for day and clinic?")) {
				return;
			}
			DateTime dateClear = AppointmentL.DateSelected;
			if(ApptDrawing.IsWeeklyView) {
				dateClear=WeekStartDate.AddDays(SheetClickedonDay);
			}
			Operatory operatory=Operatories.GetOperatory(SheetClickedonOp);
			Schedules.ClearBlockoutsForClinic(operatory.ClinicNum,dateClear);
			SecurityLogs.MakeLogEntry(Permissions.Blockouts,0,"Blockout clear clinic: "+Operatories.GetAbbrev(SheetClickedonOp));
			RefreshPeriod();
		}

		private void OnBlockTypes_Click(object sender,EventArgs e) {
			if(!Security.IsAuthorized(Permissions.Setup)) {
				return;
			}
			FormDefinitions FormD=new FormDefinitions(DefCat.BlockoutTypes);
			FormD.ShowDialog();
			SecurityLogs.MakeLogEntry(Permissions.Setup,0,"Definitions.");
			RefreshPeriod();
		}

		private void OnCopyToPin_Click() {
			if(!Security.IsAuthorized(Permissions.AppointmentMove)) {
				return;
			}
			//cannot allow moving completed procedure because it could cause completed procs to change date.  Security must block this.
			//ContrApptSingle3[thisIndex].DataRoww;
			Appointment appt=Appointments.GetOneApt(ContrApptSingle.SelectedAptNum);
			if(appt==null) {
				MsgBox.Show(this,"Appointment not found.");
				return;
			}
			if(appt.AptStatus==ApptStatus.Complete) {
				MsgBox.Show(this,"Not allowed to move completed appointments.");
				return;
			}
			if(PatRestrictions.IsRestricted(appt.PatNum,PatRestrict.ApptSchedule)) {
				return;
			}
			int prevSel=GetIndex(ContrApptSingle.SelectedAptNum);
			SendToPinBoard(ContrApptSingle.SelectedAptNum);//sets selectedAptNum=-1. do before refresh prev
			if(prevSel!=-1) {
				CreateAptShadowsOnMain();
				ContrApptSheet2.DrawShadow();
			}
			//RefreshModulePatient(PatCurNum);
			//RefreshPeriod();
		}

		private void listConfirmed_MouseDown(object sender,System.Windows.Forms.MouseEventArgs e) {
			if(listConfirmed.IndexFromPoint(e.X,e.Y)==-1) {
				return;
			}
			if(ContrApptSingle.SelectedAptNum==-1) {
				return;
			}
			long newStatus=DefC.Short[(int)DefCat.ApptConfirmed][listConfirmed.IndexFromPoint(e.X,e.Y)].DefNum;
			long oldStatus=Appointments.GetApptConfirmationStatus(ContrApptSingle.SelectedAptNum);
			Appointments.SetConfirmed(ContrApptSingle.SelectedAptNum,newStatus);
			if(newStatus!=oldStatus) {
				//Log confirmation status changes.
				SecurityLogs.MakeLogEntry(Permissions.ApptConfirmStatusEdit,PatCur.PatNum,Lans.g(this,"Appointment confirmation status changed from")+" "
					+DefC.GetName(DefCat.ApptConfirmed,oldStatus)+" "+Lans.g(this,"to")+" "+DefC.GetName(DefCat.ApptConfirmed,newStatus)
					+" "+Lans.g(this,"from the appointment module")+".",ContrApptSingle.SelectedAptNum);
			}
			RefreshPeriod();
			Signalods.SetInvalidAppt(Appointments.GetOneApt(ContrApptSingle.SelectedAptNum));
		}

		private void butSearch_Click(object sender,System.EventArgs e) {
			if(pinBoard.ApptList.Count==0) {
				MsgBox.Show(this,"An appointment must be placed on the pinboard before a search can be done.");
				return;
			}
			if(pinBoard.SelectedIndex==-1) {
				if(pinBoard.ApptList.Count==1) {
					pinBoard.SelectedIndex=0;
				}
				else {
					MsgBox.Show(this,"An appointment on the pinboard must be selected before a search can be done.");
					return;
				}
			}
			if(!groupSearch.Visible) {//if search not already visible
				dateSearch.Text=DateTime.Today.ToShortDateString();
				ShowSearch();
			}
			DoSearch();
		}

		///<summary>Positions the search box, fills it with initial data except date, and makes it visible.</summary>
		private void ShowSearch() {
			ProviderList=new List<Provider>();
			groupSearch.Location=new Point(panelCalendar.Location.X,panelCalendar.Location.Y+pinBoard.Bottom+2);
			textBefore.Text="";
			textAfter.Text="";
			listProviders.Items.Clear();
			for(int i=0;i<ProviderC.ListShort.Count;i++) {
				if(pinBoard.SelectedAppt.DataRoww["IsHygiene"].ToString()=="1"
					&& ProviderC.ListShort[i].ProvNum.ToString()==pinBoard.SelectedAppt.DataRoww["ProvHyg"].ToString()) {
					listProviders.Items.Add(ProviderC.ListShort[i].Abbr);//If their appiontment is hygine, the list will start with just their hygine provider
					ProviderList.Add(ProviderC.ListShort[i]);
				}
				else if(pinBoard.SelectedAppt.DataRoww["IsHygiene"].ToString()=="0"
					&& ProviderC.ListShort[i].ProvNum.ToString()==pinBoard.SelectedAppt.DataRoww["ProvNum"].ToString()) {
					listProviders.Items.Add(ProviderC.ListShort[i].Abbr);//If their appointment is not hygine, they will start with just their primary provider
					ProviderList.Add(ProviderC.ListShort[i]);
				}
			}
			Plugins.HookAddCode(this,"ContrAppt.ShowSearch_end",listProviders,PIn.Long(pinBoard.SelectedAppt.DataRoww["AptNum"].ToString()));
			groupSearch.Visible=true;
		}

		private void DoSearch() {
			Cursor=Cursors.WaitCursor;
			DateTime afterDate;
			try {
				afterDate=PIn.Date(dateSearch.Text);
				if(afterDate.Year<1880) {
					throw new Exception();
				}
			}
			catch {
				Cursor=Cursors.Default;
				MsgBox.Show(this,"Invalid date.");
				return;
			}
			TimeSpan beforeTime=new TimeSpan(0);
			if(textBefore.Text!="") {
				try {
					string[] hrmin=textBefore.Text.Split(new char[] { ':' },StringSplitOptions.RemoveEmptyEntries);//doesn't work with foreign times.
					string hr="0";
					if(hrmin.Length>0) {
						hr=hrmin[0];
					}
					string min="0";
					if(hrmin.Length>1) {
						min=hrmin[1];
					}
					beforeTime=TimeSpan.FromHours(PIn.Double(hr))
						+TimeSpan.FromMinutes(PIn.Double(min));
					if(radioBeforePM.Checked && beforeTime.Hours<12) {
						beforeTime=beforeTime+TimeSpan.FromHours(12);
					}
				}
				catch {
					Cursor=Cursors.Default;
					MsgBox.Show(this,"Invalid time.");
					return;
				}
			}
			TimeSpan afterTime=new TimeSpan(0);
			if(textAfter.Text!="") {
				try {
					string[] hrmin=textAfter.Text.Split(new char[] { ':' },StringSplitOptions.RemoveEmptyEntries);//doesn't work with foreign times.
					string hr="0";
					if(hrmin.Length>0) {
						hr=hrmin[0];
					}
					string min="0";
					if(hrmin.Length>1) {
						min=hrmin[1];
					}
					afterTime=TimeSpan.FromHours(PIn.Double(hr))
						+TimeSpan.FromMinutes(PIn.Double(min));
					if(radioAfterPM.Checked && afterTime.Hours<12) {
						afterTime=afterTime+TimeSpan.FromHours(12);
					}
				}
				catch {
					Cursor=Cursors.Default;
					MsgBox.Show(this,"Invalid time.");
					return;
				}
			}
			if(listProviders.Items.Count==0) {
				Cursor=Cursors.Default;
				MsgBox.Show(this,"Please pick a provider.");
				return;
			}
			long[] providers=new long[listProviders.Items.Count];
			List<long> providerNums = new List<long>();
			for(int i=0;i<providers.Length;i++) {
				providers[i]=ProviderList[i].ProvNum;
				providerNums.Add(ProviderList[i].ProvNum);
				//providersList.Add(providers[i]);
			}
			//the result might be empty
			SearchResults=AppointmentL.GetSearchResults(PIn.Long(pinBoard.SelectedAppt.DataRoww["AptNum"].ToString()),
				afterDate,providerNums,10,beforeTime,afterTime);
			listSearchResults.Items.Clear();
			for(int i=0;i<SearchResults.Count;i++) {
				listSearchResults.Items.Add(
					SearchResults[i].ToString("ddd")+"\t"+SearchResults[i].ToShortDateString()+"     "+SearchResults[i].ToShortTimeString());
			}
			if(listSearchResults.Items.Count>0) {
				listSearchResults.SetSelected(0,true);
				AppointmentL.DateSelected=SearchResults[0];
			}
			SetWeeklyView(false);//jump to that day.
			Cursor=Cursors.Default;
			//scroll to make visible?
			//highlight schedule?*/
		}

		private void butSearchMore_Click(object sender,System.EventArgs e) {
			if(pinBoard.SelectedAppt==null) {
				MsgBox.Show(this,"There is no appointment on the pinboard.");
				return;
			}
			if(SearchResults.Count<1) {
				return;
			}
			dateSearch.Text=SearchResults[SearchResults.Count-1].ToShortDateString();
			DoSearch();
		}

		private void listSearchResults_MouseDown(object sender,System.Windows.Forms.MouseEventArgs e) {
			int clickedI=listSearchResults.IndexFromPoint(e.X,e.Y);
			if(clickedI==-1) {
				return;
			}
			AppointmentL.DateSelected=SearchResults[clickedI];
			SetWeeklyView(false);
		}

		private void butRefresh_Click(object sender,EventArgs e) {
			if(pinBoard.SelectedAppt==null ) {
				if(pinBoard.ApptList.Count>0) {//if there are any appointments in the pinboard.
					pinBoard.SelectedIndex=pinBoard.ApptList.Count-1;//select last appt
				}
				else {
					MsgBox.Show(this,"There are no appointments on the pinboard.");
					return;
				}
			}
			DoSearch();
		}

		private void butSearchCloseX_Click(object sender,System.EventArgs e) {
			groupSearch.Visible=false;
		}

		private void butSearchClose_Click(object sender,System.EventArgs e) {
			groupSearch.Visible=false;
		}

		private void button1_Click_1(object sender,System.EventArgs e) {
			MessageBox.Show(Lan.g(this,this.GetType().Name));
		}

		private void butLab_Click(object sender,EventArgs e) {
			FormLabCases FormL=new FormLabCases();
			FormL.ShowDialog();
			if(FormL.GoToAptNum!=0) {
				Appointment apt=Appointments.GetOneApt(FormL.GoToAptNum);
				Patient pat=Patients.GetPat(apt.PatNum);
				//PatientSelectedEventArgs eArgs=new OpenDental.PatientSelectedEventArgs(pat.PatNum,pat.GetNameLF(),pat.Email!="",pat.ChartNumber);
				//if(PatientSelected!=null){
				//	PatientSelected(this,eArgs);
				//}
				//Contr_PatientSelected(this,eArgs);
				FormOpenDental.S_Contr_PatientSelected(pat,false,false);
				GotoModule.GotoAppointment(apt.AptDateTime,apt.AptNum);
			}
		}

		///<summary>Happens once per minute.  It used to just move the red timebar down without querying the database.  But now it queries the database so that the waiting room list shows accurately.</summary>
		public void TickRefresh() {
			try {
				DateTime startDate;
				DateTime endDate;
				if(ApptDrawing.IsWeeklyView) {
					startDate=WeekStartDate;
					endDate=WeekEndDate;
				}
				else {
					startDate=AppointmentL.DateSelected;
					endDate=AppointmentL.DateSelected;
				}
				if(PrefC.GetBool(PrefName.ApptModuleRefreshesEveryMinute)) {
					RefreshPeriod(false);
				}
				else {
					ContrApptSheet2.CreateShadow();
					CreateAptShadowsOnMain();
					ContrApptSheet2.DrawShadow();
				}
			}
			catch {
				//prevents rare malfunctions. For instance, during editing of views, if tickrefresh happens.
			}
			//GC.Collect();	
		}

		///<summary>"Ganga's Code: Printing the Appointment Card - 9/9/2004"</summary>
		private void PrintApptCard() {
			pd2=new PrintDocument();
			pd2.PrintPage+=new PrintPageEventHandler(this.pd2_PrintApptCard);
			pd2.DefaultPageSettings.Margins=new Margins(0,0,0,0);
			pd2.OriginAtMargins=true;//forces origin to upper left of actual page
#if DEBUG
			FormRpPrintPreview pView=new FormRpPrintPreview();
			pView.printPreviewControl2.Document=pd2;
			pView.ShowDialog();
#else
			if(PrinterL.SetPrinter(pd2,PrintSituation.Postcard,PatCur.PatNum,"Appointment reminder postcard printed")) {
				pd2.Print();
			}
#endif
		}

		private void pd2_PrintApptCard(object sender,PrintPageEventArgs ev) {
			Graphics g=ev.Graphics;
			long apptClinicNum=0;
			for(int i=0;i<DS.Tables["Appointments"].Rows.Count;i++) {
				if(PIn.Long(DS.Tables["Appointments"].Rows[i]["AptNum"].ToString())==ContrApptSingle.SelectedAptNum) {
					apptClinicNum=PIn.Long(DS.Tables["Appointments"].Rows[i]["ClinicNum"].ToString());
					break;
				}
			}
			Clinic clinic=Clinics.GetClinic(apptClinicNum);
			//Return Address--------------------------------------------------------------------------
			string str="";
			string phone="";
			if(PrefC.HasClinicsEnabled && clinic!=null) {//Use clinic on appointment if clinic exists and has clinics enabled
				str=clinic.Description+"\r\n";
				g.DrawString(str,new Font(FontFamily.GenericSansSerif,9,FontStyle.Bold),Brushes.Black,60,60);
				str=clinic.Address+"\r\n";
				if(clinic.Address2!="") {
					str+=clinic.Address2+"\r\n";
				}
				str+=clinic.City+"  "+clinic.State+"  "+clinic.Zip+"\r\n";
				phone=clinic.Phone;
			}
			else {//Otherwise use practice information
				str=PrefC.GetString(PrefName.PracticeTitle)+"\r\n";
				g.DrawString(str,new Font(FontFamily.GenericSansSerif,9,FontStyle.Bold),Brushes.Black,60,60);
				str=PrefC.GetString(PrefName.PracticeAddress)+"\r\n";
				if(PrefC.GetString(PrefName.PracticeAddress2)!="") {
					str+=PrefC.GetString(PrefName.PracticeAddress2)+"\r\n";
				}
				str+=PrefC.GetString(PrefName.PracticeCity)+"  "
					+PrefC.GetString(PrefName.PracticeST)+"  "
					+PrefC.GetString(PrefName.PracticeZip)+"\r\n";
				phone=PrefC.GetString(PrefName.PracticePhone);
			}
			if(CultureInfo.CurrentCulture.Name=="en-US" && phone.Length==10) {
				str+="("+phone.Substring(0,3)+")"+phone.Substring(3,3)+"-"+phone.Substring(6);
			}
			else {//any other phone format
				str+=phone;
			}
			g.DrawString(str,new Font(FontFamily.GenericSansSerif,8),Brushes.Black,60,75);
			//Body text-------------------------------------------------------------------------------
			string name;
			str="Appointment Reminders:"+"\r\n\r\n";
			Appointment[] aptsOnePat;
			Family fam=Patients.GetFamily(PatCur.PatNum);
			Patient pat=fam.GetPatient(PatCur.PatNum);
			for(int i=0;i<fam.ListPats.Length;i++) {
				if(!cardPrintFamily && fam.ListPats[i].PatNum!=pat.PatNum) {
					continue;
				}
				name=fam.ListPats[i].FName;
				if(name.Length>15) {//trim name so it won't be too long
					name=name.Substring(0,15);
				}
				aptsOnePat=Appointments.GetForPat(fam.ListPats[i].PatNum);
				for(int a=0;a<aptsOnePat.Length;a++) {
					if(aptsOnePat[a].AptDateTime.Date<=DateTime.Today) {
						continue;//ignore old appts
					}
					if(aptsOnePat[a].AptStatus!=ApptStatus.Scheduled && aptsOnePat[a].AptStatus!=ApptStatus.ASAP){
						continue;
					}
					str+=name+": "+aptsOnePat[a].AptDateTime.ToShortDateString()+" "+aptsOnePat[a].AptDateTime.ToShortTimeString()+"\r\n";
				}
			}
			g.DrawString(str,new Font(FontFamily.GenericSansSerif,9),Brushes.Black,40,180);
			//Patient's Address-----------------------------------------------------------------------
			Patient guar;
			if(cardPrintFamily) {
				guar=fam.ListPats[0].Copy();
			}
			else {
				guar=pat.Copy();
			}
			str=guar.FName+" "+guar.LName+"\r\n"
				+guar.Address+"\r\n";
			if(guar.Address2!="") {
				str+=guar.Address2+"\r\n";
			}
			str+=guar.City+"  "+guar.State+"  "+guar.Zip;
			g.DrawString(str,new Font(FontFamily.GenericSansSerif,11),Brushes.Black,300,240);
			//CommLog entry---------------------------------------------------------------------------
			Commlog CommlogCur=new Commlog();
			CommlogCur.CommDateTime=DateTime.Now;
			CommlogCur.CommType=Commlogs.GetTypeAuto(CommItemTypeAuto.MISC);
			CommlogCur.Note="Appointment card sent";
			CommlogCur.PatNum=pat.PatNum;
			CommlogCur.UserNum=Security.CurUser.UserNum;
			//there is no dialog here because it is just a simple entry
			Commlogs.Insert(CommlogCur);
			ev.HasMorePages = false;
		}

		private void timerInfoBubble_Tick(object sender,EventArgs e) {
			InfoBubbleDraw(bubbleLocation);
			timerInfoBubble.Enabled =false;
		}

		private void butMonth_Click(object sender,EventArgs e) {
			FormMonthView FormM=new FormMonthView();
			FormM.ShowDialog();
		}

		private void timerWaitingRoom_Tick(object sender,EventArgs e) {
			FillWaitingRoom();
		}

		private void timerTests_Tick(object sender,EventArgs e) {
			//stress test #2:
			//ContrApptSheet2.CreateShadow();
			//CreateAptShadows();
			//ContrApptSheet2.DrawShadow();
			//stress test #3:
			//stressCounter++;
			//if(Math.IEEERemainder((double)stressCounter,500d)==0) {
			//	Debug.WriteLine("stress counter: "+stressCounter.ToString());
			//}
			//FillWaitingRoom();
			//stress test #4:
			//LayoutScrollOpProv();
			//if(PatCur!=null) {
			//	this.ModuleSelected(PatCur.PatNum);
			//}
		}

		private void butGraph_Click(object sender,EventArgs e) {
			//only visible on computers at OD corporate.
			FormGraphEmployeeTime form=new FormGraphEmployeeTime();
			form.ShowDialog();
		}

		private void butProvPick_Click(object sender,EventArgs e) {
			FormProvidersMultiPick FormPMP=new FormProvidersMultiPick();
			FormPMP.SelectedProviders=ProviderList;
			FormPMP.ShowDialog();
			if(FormPMP.DialogResult!=DialogResult.OK) {
				return;
			}
			listProviders.Items.Clear();
			for(int i=0;i<FormPMP.SelectedProviders.Count;i++) {
				listProviders.Items.Add(FormPMP.SelectedProviders[i].Abbr);
			}
			ProviderList=FormPMP.SelectedProviders;
			if(pinBoard.SelectedAppt==null) {
				MsgBox.Show(this,"There is no appointment on the pinboard.");
				return;
			}
			DoSearch();
		}

		private void butProvHygenist_Click(object sender,EventArgs e) {
			ProviderList=new List<Provider>();
			listProviders.Items.Clear();
			for(int i=0;i<ProviderC.ListShort.Count;i++) {
				if(ApptViewItemL.ProvIsInView(ProviderC.ListShort[i].ProvNum)) {
					if(ProviderC.ListShort[i].IsSecondary) {
						ProviderList.Add(ProviderC.ListShort[i]);
						listProviders.Items.Add(ProviderC.ListShort[i].Abbr);
					}
				}
			}
			if(pinBoard.SelectedAppt==null) {
				MsgBox.Show(this,"There is no appointment on the pinboard.");
				return;
			}
			DoSearch();
		}

		private void butProvDentist_Click(object sender,EventArgs e) {
			ProviderList=new List<Provider>();
			listProviders.Items.Clear();
			for(int i=0;i<ProviderC.ListShort.Count;i++){
				if(ApptViewItemL.ProvIsInView(ProviderC.ListShort[i].ProvNum)){
					if(!ProviderC.ListShort[i].IsSecondary) {
						ProviderList.Add(ProviderC.ListShort[i]);
						listProviders.Items.Add(ProviderC.ListShort[i].Abbr);
					}
				}
			}
			if(pinBoard.SelectedAppt==null) {
				MsgBox.Show(this,"There is no appointment on the pinboard.");
				return;
			}
			DoSearch();
		}






		//private void butTest_Click(object sender,EventArgs e) {
		//	timerTests.Enabled=!timerTests.Enabled;
		//}

		/*
		private void butStress_Click(object sender,EventArgs e) {
			timerStress.Enabled=true;
		}

		private void timerStress_Tick(object sender,EventArgs e) {
			
		}*/





























	}




}













