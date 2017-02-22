using CodeBase;
using Microsoft.Win32;
using OpenDental.UI;
using OpenDentBusiness;
using OpenDentBusiness.Mobile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Globalization;
using System.Data;
using System.Linq;
using System.IO;
using WebServiceSerializer;
using OpenDentBusiness.WebServiceMainHQ;
using OpenDentBusiness.WebTypes.WebSched.TimeSlot;

namespace OpenDental {
	///<summary>Form manages all eServices setup.  Also includes monitoring for the Listener Service that is required for HQ hosted eServices.</summary>
	public partial class FormEServicesSetup:ODForm {
		private const string WEB_SCHED_NEW_PAT_APPT_SIGN_UP_URL="http://www.patientviewer.com/WebSchedNewPatSignUp.html";
		private static MobileWeb.Mobile mb=new MobileWeb.Mobile();
		private static int _batchSize=100;
		///<summary>All statements of a patient are not uploaded. The limit is defined by the recent [statementLimitPerPatient] records</summary>
		private static int _statementLimitPerPatient=5;
		///<summary>This variable prevents the synching methods from being called when a previous synch is in progress.</summary>
		private static bool _isSynching;
		///<summary>This variable prevents multiple error message boxes from popping up if mobile synch server is not available.</summary>
		private static bool _isServerAvail=true;
		///<summary>True if a pref was saved and the other workstations need to have their cache refreshed when this form closes.</summary>
		private bool _hasPrefsChanged;
		///<summary>True if a clinic has changed and the other workstations need to have their cache refreshed when this form closes.</summary>
		private bool _hasClinicsChanged;
		///<summary>If this variable is true then records are uploaded one at a time so that an error in uploading can be traced down to a single record</summary>
		private static bool _isTroubleshootMode=false;
		private static FormProgress FormP;
		///<summary>The background color used when the OpenDentalCustListener service is down.  Using Red was deemed too harsh.
		///This variable should be treated as a constant which is why it is in all caps.  The type 'System.Drawing.Color' cannot be declared const.</summary>
		private Color COLOR_ESERVICE_CRITICAL_BACKGROUND=Color.OrangeRed;
		///<summary>The text color used when the OpenDentalCustListener service is down.
		///This variable should be treated as a constant which is why it is in all caps.  The type 'System.Drawing.Color' cannot be declared const.</summary>
		private Color COLOR_ESERVICE_CRITICAL_TEXT=Color.Yellow;
		///<summary>The background color used when the OpenDentalCustListener service has an error that has not be processed.
		///This variable should be treated as a constant which is why it is in all caps.  The type 'System.Drawing.Color' cannot be declared const.</summary>
		private Color COLOR_ESERVICE_ERROR_BACKGROUND=Color.LightGoldenrodYellow;
		///<summary>The text color used when the OpenDentalCustListener service has an error that has not be processed.
		///This variable should be treated as a constant which is why it is in all caps.  The type 'System.Drawing.Color' cannot be declared const.</summary>
		private Color COLOR_ESERVICE_ERROR_TEXT=Color.OrangeRed;
		///<summary>A list of all clinics.  This list could include clinics that the user should not have access to so be careful using it. For the sake of modular code, there are seperate lists for the Integrated Texting (sms) and Automated eConfirmation (eC) tabs.</summary>
		private List<Clinic> _smsListAllClinics;
		///<summary>A list of clinics that the user currently logged into has access to. For the sake of modular code, there are seperate lists for the Integrated Texting (sms) and Automated eConfirmation (eC) tabs.</summary>
		private List<Clinic> _smsListClinics;
		///<summary>The currently selected clinic for use in the SMS tab.</summary>
		private Clinic _smsClinicCur;
		private List<SmsPhone> _listPhones;
		private List<RecallType> _listRecallTypes;
		///<summary>A list of all operatories that have IsWebSched set to true.</summary>
		private List<Operatory> _listWebSchedRecallOps;
		///<summary>A list of all operatories that have IsWebSched set to true.</summary>
		private List<Operatory> _listWebSchedNewPatApptOps;
		///<summary>A deep copy of ProviderC.GetListShort().  Use the cache instead of this list if you need an up to date list of providers.</summary>
		private List<Provider> _listProviders;
		///<summary>Provider number used to filter the Time Slots grid.  0 is treated as 'All'</summary>
		private long _webSchedProvNum=0;
		///<summary>Clinic number used to filter the Time Slots grid.  0 is treated as 'Unassigned'</summary>
		private long _webSchedClinicNum=0;
		private ListenerServiceType _listenerType=ListenerServiceType.NoListener;
		///<summary>Set to true if there was a problem retrieving the URLs for New Patient Appointments.</summary>
		private bool _hasWebSchedNewPatApptURLError=false;
		private ODThread _threadGetWebSchedNewPatApptHostedURLs=null;
		private List<NewPatApptURL> _listWebSchedNewPatApptHostedURLs=new List<NewPatApptURL>();
		///<summary>Keeps track of the last selected index for the Web Sched New Pat Appt URL grid.</summary>
		private int _indexLastNewPatURL=-1;
		private TimeSpan _automationStart;
		private TimeSpan _automationEnd;
		//==================== eConfirm & eRemind Variables ====================
		private List<Def> _listDefsApptStatus;
		private List<Clinic> _ecListClinics;
		private Clinic _ecClinicCur;
		///<summary>When using clinics, this is the index of the clinic rules to use.</summary>
		private long _clinicRuleIdx;//not acutal idx, actually just ClinicNum
		///<summary>Key = ClinicNum, 0=Practice/Defaults. Value = Rules defined for that clinic. If a clinic uses defaults, its respective list of rules will be empty.</summary>
		private Dictionary<long,List<ApptReminderRule>> _dictClinicRules;

		private WebServiceMainHQ _webServiceMain {
			get {
				return WebServiceMainHQProxy.GetWebServiceMainHQInstance();
			}
		}
		
		///<summary>Launches the eServices Setup window defaulted to the tab of the eService passed in.</summary>
		public FormEServicesSetup(EService setTab=EService.PatientPortal) {
			InitializeComponent();
			Lan.F(this);
			Lan.C(this,menuWebSchedNewPatApptHostedURLsRightClick);
			switch(setTab) {
				case EService.ListenerService:
					tabControl.SelectTab(tabListenerService);
					break;
				case EService.MobileOld:
					tabControl.SelectTab(tabMobileOld);
					break;
				case EService.MobileNew:
					tabControl.SelectTab(tabMobileNew);
					break;
				case EService.WebSched:
					tabControl.SelectTab(tabWebSched);
					break;
				case EService.SmsService:
					tabControl.SelectTab(tabSmsServices);
					break;
				case EService.eConfirmRemind:
					tabControl.SelectTab(tabRemindConfirmSetup);
					break;
				case EService.eMisc:
					tabControl.SelectTab(tabMisc);
					break;
				case EService.PatientPortal:
				default:
					tabControl.SelectTab(tabPatientPortal);
					break;
			}
		}

		private void FormEServicesSetup_Load(object sender,EventArgs e) {
			textRedirectUrlPatientPortal.Text=PrefC.GetString(PrefName.PatientPortalURL);
			textBoxNotificationSubject.Text=PrefC.GetString(PrefName.PatientPortalNotifySubject);
			textBoxNotificationBody.Text=PrefC.GetString(PrefName.PatientPortalNotifyBody);
			#region mobile synch
			textMobileSyncServerURL.Text=PrefC.GetString(PrefName.MobileSyncServerURL);
			textSynchMinutes.Text=PrefC.GetInt(PrefName.MobileSyncIntervalMinutes).ToString();
			textDateBefore.Text=PrefC.GetDate(PrefName.MobileExcludeApptsBeforeDate).ToShortDateString();
			textMobileSynchWorkStation.Text=PrefC.GetString(PrefName.MobileSyncWorkstationName);
			textMobileUserName.Text=PrefC.GetString(PrefName.MobileUserName);
			textMobilePassword.Text="";//not stored locally, and not pulled from web server
			DateTime lastRun=PrefC.GetDateT(PrefName.MobileSyncDateTimeLastRun);
			if(lastRun.Year>1880) {
				textDateTimeLastRun.Text=lastRun.ToShortDateString()+" "+lastRun.ToShortTimeString();
			}
			//Hide the Old-style Mobile Synch tab and make visible the button that unhides the tab, 
			//only if mobile synch has not been used for at least a month. If used again, the clock will reset
			//and the 
			if(MiscData.GetNowDateTime().Subtract(lastRun.Date).TotalDays>30) {
				groupNotUsed.Visible=true;
				butShowOldMobileSych.Visible=true;
				tabControl.TabPages.Remove(tabMobileOld);
			}
			//Web server is not contacted when loading this form.  That would be too slow.
			//CreateAppointments(5);
			#endregion
			#region Web Sched
			#region WebSched Automation Settings
			//.NET has a bug in the DateTimePicker control where the text will not get updated and will instead default to showing DateTime.Now.
			//In order to get the control into a mode where it will display the correct value that we set, we need to set the property Checked to true.
			//Today's date will show even when the property is defaulted to true (via the designer), so we need to do it programmatically right here.
			//E.g. set your computer region to Assamese (India) and the DateTimePickers on the Automation Setting tab will both be set to todays date
			// if the tab is NOT set to be the first tab to display (don't ask me why it works then).
			//This is bad for our customers because setting both of the date pickers to the same date and time will cause automation to stop.
			dateRunStart.Checked=true;
			dateRunEnd.Checked=true;
			//Now that the DateTimePicker controls are ready to display the DateTime we set, go ahead and set them.
			try {
				dateRunStart.Value=PrefC.GetDateT(PrefName.AutomaticCommunicationTimeStart);
				dateRunEnd.Value=PrefC.GetDateT(PrefName.AutomaticCommunicationTimeEnd);
			}
			catch(Exception) {
				//Loading the picker controls with the DateTime fields from the database failed.  The date picker controls default to 7 AM and 10 PM.
			}
			_automationStart=dateRunStart.Value.TimeOfDay;
			_automationEnd=dateRunEnd.Value.TimeOfDay;
			switch(PrefC.GetInt(PrefName.WebSchedAutomaticSendSetting)) {
				case (int)WebSchedAutomaticSend.DoNotSend:
					radioDoNotSend.Checked=true;
					break;
				case (int)WebSchedAutomaticSend.SendToEmail:
					radioSendToEmail.Checked=true;
					break;
				case (int)WebSchedAutomaticSend.SendToEmailNoPreferred:
					radioSendToEmailNoPreferred.Checked=true;
					break;
				case (int)WebSchedAutomaticSend.SendToEmailOnlyPreferred:
					radioSendToEmailOnlyPreferred.Checked=true;
					break;
			}
			#endregion WebSched Automation Settings
			#region Web Sched - Recalls
			labelWebSchedEnable.Text="";
			labelWebSchedNewPatApptEnable.Text="";
			if(PrefC.GetBool(PrefName.WebSchedService)) {
				butWebSchedEnable.Visible=false;
				butSignUp.Visible=false;
				labelWebSchedEnable.Text=Lan.g(this,"Recall Web Sched service is enabled.  Call to disable service.");
			}
			if(PrefC.GetBool(PrefName.WebSchedNewPatApptEnabled)) {
				butWebSchedNewPatApptEnable.Visible=false;
				butWebSchedNewPatApptSignUp.Visible=false;
				labelWebSchedNewPatApptEnable.Text=Lan.g(this,"New Patient Appts Web Sched service is enabled.  Call to disable service.");
			}
			textWebSchedDateStart.Text=DateTime.Today.ToShortDateString();
			comboWebSchedClinic.Items.Clear();
			comboWebSchedClinic.Items.Add(Lan.g(this,"Unassigned"));
			_smsListAllClinics=Clinics.GetList().ToList();
			for(int i=0;i<_smsListAllClinics.Count;i++) {
				comboWebSchedClinic.Items.Add(_smsListAllClinics[i].Abbr);
			}
			comboWebSchedClinic.SelectedIndex=0;
			_listProviders=ProviderC.GetListShort();
			comboWebSchedProviders.Items.Clear();
			comboWebSchedProviders.Items.Add(Lan.g(this,"All"));
			for(int i=0;i<_listProviders.Count;i++) {
				comboWebSchedProviders.Items.Add(_listProviders[i].GetLongDesc());
			}
			comboWebSchedProviders.SelectedIndex=0;
			if(PrefC.GetBool(PrefName.EasyNoClinics)) {
				labelWebSchedClinic.Visible=false;
				comboWebSchedClinic.Visible=false;
				butWebSchedPickClinic.Visible=false;
			}
			FillGridWebSchedRecallTypes();
			FillGridWebSchedOperatories();
			FillGridWebSchedTimeSlots();
			listBoxWebSchedProviderPref.SelectedIndex=PrefC.GetInt(PrefName.WebSchedProviderRule);
			#endregion Web Sched - Recalls
			#region Web Sched - New Patient Appointments
			int newPatApptDays=PrefC.GetInt(PrefName.WebSchedNewPatApptSearchAfterDays);
			textWebSchedNewPatApptSearchDays.Text=newPatApptDays > 0 ? newPatApptDays.ToString() : "";
			textWebSchedNewPatApptLength.Text=PrefC.GetString(PrefName.WebSchedNewPatApptTimePattern);
			DateTime dateWebSchedNewPatSearch=DateTime.Now;
			dateWebSchedNewPatSearch=dateWebSchedNewPatSearch.AddDays(newPatApptDays);
			textWebSchedNewPatApptsDateStart.Text=dateWebSchedNewPatSearch.ToShortDateString();
			gridWebSchedNewPatApptURLs.ContextMenu=menuWebSchedNewPatApptHostedURLsRightClick;
			GetWebSchedNewPatApptHostedURLs();
			FillGridWebSchedNewPatApptProcs();
			FillGridWebSchedNewPatApptOps();
			#endregion Web Sched - New Patient Appointments
			#endregion
			#region Listener Service
			textListenerPort.Text=PrefC.GetString(PrefName.CustListenerPort);
#if !DEBUG
			try {
				_listenerType=WebSerializer.DeserializePrimitiveOrThrow<ListenerServiceType>(
					_webServiceMain.GetEConnectorType(WebSerializer.SerializePrimitive<string>(PrefC.GetString(PrefName.RegistrationKey)))
				);
			}
			catch(Exception ex) {
				checkAllowEConnectorComm.Enabled=false;
			} 
#endif
			SetEConnectorCommunicationStatus();
			//Check to see if the eConnector service is already installed.  If it is, disable the install button.
			//Users who want to install multiple on one computer can use the Service Manager instead.
			try {
				if(ServicesHelper.GetServicesByExe("OpenDentalEConnector.exe").Count > 0) {
					butInstallEConnector.Enabled=false;
				}
			}
			catch(Exception) {
				//Do nothing.  The Install button will simply be visible.
			}
			FillTextListenerServiceStatus();
			FillGridListenerService();
			#endregion
			#region Sms Service
			_smsListClinics=Clinics.GetForUserod(Security.CurUser);
			if(_smsClinicCur==null && _smsListClinics.Count>0) {
				_smsClinicCur=_smsListClinics[0];//default to first clinic in list, if no clinics were passed into this form using the constructor.
			}
			if(!PrefC.HasClinicsEnabled) {
				gridClinics.Height+=29;
				butDefaultClinic.Visible=false;
				butDefaultClinicClear.Visible=false;
			}
			FillComboClinicSms();
			textCountryCode.Text=CultureInfo.CurrentCulture.Name.Substring(CultureInfo.CurrentCulture.Name.Length-2);
			FillGridClinics();
			FillGridSmsUsage();
			SetSmsServiceAgreement();
			#endregion
			#region eConfirm & eRemind
			FillTabRemindConfirm();
			#endregion eConfirm & eRemind
			SetControlEnabledState();
		}

		private void butShowOldMobileSych_Click(object sender,EventArgs e) {
			butShowOldMobileSych.Enabled=false;
			tabControl.TabPages.Add(tabMobileOld);
			tabControl.SelectedTab=tabMobileOld;
		}

		private void linkLabelAboutWebSched_Click(object sender,EventArgs e) {
			MessageBox.Show("Use the Web Sched button in the Recall List window to send "
				+"emails with a link that will allow patients to quickly schedule their recall appointments. "
				+"You may also publish a link to the New Patient Appt URL on your web site to allow new patients to schedule "
				+"their first appointment.  All appointments scheduled using Web Sched will instantly show "
				+"up on the schedule.",Lan.g(this,"Web Sched Information"),MessageBoxButtons.OK);
			//We do not want to hardcode links to pages in the manual.
			//try {
			//	Process.Start("http://opendental.com/manual/websched.html");
			//}
			//catch(Exception ex) {
			//	ex.DoNothing();
			//	MsgBox.Show(this,"Unable to open online manual.");
			//}
		}

		private void FillComboClinicSms() {
			if(PrefC.GetBool(PrefName.EasyNoClinics)) {
				comboClinicSms.Items.Add(PrefC.GetString(PrefName.PracticeTitle));
				comboClinicSms.SelectedIndex=0;
				comboClinicSms.Enabled=false;
			}
			else {
				for(int i=0;i<_smsListClinics.Count;i++) {
					comboClinicSms.Items.Add(_smsListClinics[i].Abbr);
				}
				if(comboClinicSms.Items.Count>0) {
					comboClinicSms.SelectedIndex=0;//select first clinic in list
				}
			}
		}

		private void SetControlEnabledState() {
			if(!Security.IsAuthorized(Permissions.EServicesSetup)) {
				//Disable certain buttons but let them continue to view
				butSavePatientPortal.Enabled=false;
				butGetUrlPatientPortal.Enabled=false;
				groupBoxNotification.Enabled=false;
				textListenerPort.Enabled=false;
				butListenerServiceAck.Enabled=false;
				butSaveListenerPort.Enabled=false;
				butWebSchedEnable.Enabled=false;
				listBoxWebSchedProviderPref.Enabled=false;
				butRecallSchedSetup.Enabled=false;
				butSetFeaturesPatientPortal.Enabled=false;
				((Control)tabMobileOld).Enabled=false;
			}
		}

		#region patient portal
		private void butGetUrlPatientPortal_Click(object sender,EventArgs e) {
			try {
				string url=CustomerUpdatesProxy.GetHostedURL(eServiceCode.PatientPortal);
				textOpenDentalUrlPatientPortal.Text=url;
				if(textRedirectUrlPatientPortal.Text=="") {
					textRedirectUrlPatientPortal.Text=url;
				}
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		
		private void butSetFeaturesPatientPortal_Click(object sender,EventArgs e) {
			try {
				string url=WebSerializer.DeserializePrimitiveOrThrow<string>(
					_webServiceMain.BuildFeaturePortalUrl(PrefC.GetString(PrefName.RegistrationKey),eServiceCode.PatientPortal.ToString()));
				WebBrowser webBrowser=new WebBrowser();
				webBrowser.Navigate(new Uri(url));
				webBrowser.Dock=DockStyle.Fill;
				SecurityLogs.MakeLogEntry(Permissions.EServicesSetup,0,Lan.g(this,"Patient Portal features were accessed."));
				ODForm form=new ODForm();				
				form.Size=new Size(500,500);
				form.Controls.Add(webBrowser);
				form.Text="Choose Features";
				form.Show();				
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}

		private void butSavePatientPortal_Click(object sender,EventArgs e) {
#if !DEBUG
			if(!textRedirectUrlPatientPortal.Text.ToUpper().StartsWith("HTTPS")) {
				MsgBox.Show(this,"Patient Facing URL must start with HTTPS.");
				return;
			}
#endif
			if(textBoxNotificationSubject.Text=="") {
				MsgBox.Show(this,"Notification Subject is empty");
				textBoxNotificationSubject.Focus();
				return;
			}
			if(textBoxNotificationBody.Text=="") {
				MsgBox.Show(this,"Notification Body is empty");
				textBoxNotificationBody.Focus();
				return;
			}
			if(!textBoxNotificationBody.Text.Contains("[URL]")) { //prompt user that they omitted the URL field but don't prevent them from continuing
				if(!MsgBox.Show(this,MsgBoxButtons.YesNo,"[URL] not included in notification body. Continue without setting the [URL] field?")) {
					textBoxNotificationBody.Focus();
					return;
				}
			}
			if(Prefs.UpdateString(PrefName.PatientPortalURL,textRedirectUrlPatientPortal.Text)
				| Prefs.UpdateString(PrefName.PatientPortalNotifySubject,textBoxNotificationSubject.Text)
				| Prefs.UpdateString(PrefName.PatientPortalNotifyBody,textBoxNotificationBody.Text)) 
			{
				_hasPrefsChanged=true;//Sends invalid signal upon closing the form.
			}
			MsgBox.Show(this,"Patient Portal Info Saved");
		}
		#endregion

		#region mobile web (new-style)
		private void butGetUrlMobileWeb_Click(object sender,EventArgs e) {
			try {
				string url=CustomerUpdatesProxy.GetHostedURL(eServiceCode.MobileWeb);
				textOpenDentalUrlMobileWeb.Text=url;				
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}
		#endregion

		#region mobile synch (old-style)
		private void butCurrentWorkstation_Click(object sender,EventArgs e) {
			textMobileSynchWorkStation.Text=System.Environment.MachineName.ToUpper();
		}

		private void butSaveMobileSynch_Click(object sender,EventArgs e) {
			Cursor=Cursors.WaitCursor;
			if(!SavePrefs()) {
				Cursor=Cursors.Default;
				return;
			}
			Cursor=Cursors.Default;
			MsgBox.Show(this,"Done");
		}

		///<summary>Returns false if validation failed.  This also makes sure the web service exists, the customer is paid, and the registration key is correct.</summary>
		private bool SavePrefs() {
			//validation
			if(textSynchMinutes.errorProvider1.GetError(textSynchMinutes)!=""
				|| textDateBefore.errorProvider1.GetError(textDateBefore)!="") {
				MsgBox.Show(this,"Please fix data entry errors first.");
				return false;
			}
			//yes, workstation is allowed to be blank.  That's one way for user to turn off auto synch.
			//if(textMobileSynchWorkStation.Text=="") {
			//	MsgBox.Show(this,"WorkStation cannot be empty");
			//	return false;
			//}
			// the text field is read because the keyed in values have not been saved yet
			//if(textMobileSyncServerURL.Text.Contains("192.168.0.196") || textMobileSyncServerURL.Text.Contains("localhost")) {
			if(textMobileSyncServerURL.Text.Contains("10.10.1.196") || textMobileSyncServerURL.Text.Contains("localhost")) {
				IgnoreCertificateErrors();// done so that TestWebServiceExists() does not thow an error.
			}
			// if this is not done then an old non-functional url prevents any new url from being saved.
			Prefs.UpdateString(PrefName.MobileSyncServerURL,textMobileSyncServerURL.Text);
			if(!TestWebServiceExists()) {
				MsgBox.Show(this,"Web service not found.");
				return false;
			}
			if(mb.GetCustomerNum(PrefC.GetString(PrefName.RegistrationKey))==0) {
				MsgBox.Show(this,"Registration key is incorrect.");
				return false;
			}
			if(!VerifyPaidCustomer()) {
				return false;
			}
			//Minimum 10 char.  Must contain uppercase, lowercase, numbers, and symbols. Valid symbols are: !@#$%^&+= 
			//The set of symbols checked was far too small, not even including periods, commas, and parentheses.
			//So I rewrote it all.  New error messages say exactly what's wrong with it.
			if(textMobileUserName.Text!="") {//allowed to be blank
				if(textMobileUserName.Text.Length<10) {
					MsgBox.Show(this,"User Name must be at least 10 characters long.");
					return false;
				}
				if(!Regex.IsMatch(textMobileUserName.Text,"[A-Z]+")) {
					MsgBox.Show(this,"User Name must contain an uppercase letter.");
					return false;
				}
				if(!Regex.IsMatch(textMobileUserName.Text,"[a-z]+")) {
					MsgBox.Show(this,"User Name must contain an lowercase letter.");
					return false;
				}
				if(!Regex.IsMatch(textMobileUserName.Text,"[0-9]+")) {
					MsgBox.Show(this,"User Name must contain a number.");
					return false;
				}
				if(!Regex.IsMatch(textMobileUserName.Text,"[^0-9a-zA-Z]+")) {//absolutely anything except number, lower or upper.
					MsgBox.Show(this,"User Name must contain punctuation or symbols.");
					return false;
				}
			}
			if(textDateBefore.Text=="") {//default to one year if empty
				textDateBefore.Text=DateTime.Today.AddYears(-1).ToShortDateString();
				//not going to bother informing user.  They can see it.
			}
			//save to db------------------------------------------------------------------------------------
			if(Prefs.UpdateString(PrefName.MobileSyncServerURL,textMobileSyncServerURL.Text)
				| Prefs.UpdateInt(PrefName.MobileSyncIntervalMinutes,PIn.Int(textSynchMinutes.Text))//blank entry allowed
				| Prefs.UpdateString(PrefName.MobileExcludeApptsBeforeDate,POut.Date(PIn.Date(textDateBefore.Text),false))//blank 
				| Prefs.UpdateString(PrefName.MobileSyncWorkstationName,textMobileSynchWorkStation.Text)
				| Prefs.UpdateString(PrefName.MobileUserName,textMobileUserName.Text)) 
			{
				_hasPrefsChanged=true;
			}
			//Username and password-----------------------------------------------------------------------------
			mb.SetMobileWebUserPassword(PrefC.GetString(PrefName.RegistrationKey),textMobileUserName.Text.Trim(),textMobilePassword.Text.Trim());
			return true;
		}

		///<summary>Uploads Preferences to the Patient Portal /Mobile Web.</summary>
		public static void UploadPreference(PrefName prefname) {
			if(PrefC.GetString(PrefName.RegistrationKey)=="") {
				return;//Prevents a bug when using the trial version with no registration key.  Practice edit, OK, was giving error.
			}
			try {
				if(TestWebServiceExists()) {
					Prefm prefm = Prefms.GetPrefm(prefname.ToString());
					mb.SetPreference(PrefC.GetString(PrefName.RegistrationKey),prefm);
				}
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message);//may not show if called from a thread but that does not matter - the failing of this method should not stop the  the code from proceeding.
			}
		}

		private void butDelete_Click(object sender,EventArgs e) {
			if(!SavePrefs()) {
				return;
			}
			if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Delete all your data from our server?  This happens automatically before a full synch.")) {
				return;
			}
			mb.DeleteAllRecords(PrefC.GetString(PrefName.RegistrationKey));
			MsgBox.Show(this,"Done");
		}

		private void butFullSync_Click(object sender,EventArgs e) {
			if(!SavePrefs()) {
				return;
			}
			if(_isSynching) {
				MsgBox.Show(this,"A Synch is in progress at the moment. Please try again later.");
				return;
			}
			if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"This will be time consuming. Continue anyway?")) {
				return;
			}
			//for full synch, delete all records then repopulate.
			mb.DeleteAllRecords(PrefC.GetString(PrefName.RegistrationKey));
			ShowProgressForm(DateTime.MinValue);
		}

		private void butSync_Click(object sender,EventArgs e) {
			if(!SavePrefs()) {
				return;
			}
			if(_isSynching) {
				MsgBox.Show(this,"A Synch is in progress at the moment. Please try again later.");
				return;
			}
			if(PrefC.GetDate(PrefName.MobileExcludeApptsBeforeDate).Year<1880) {
				MsgBox.Show(this,"Full synch has never been run before.");
				return;
			}
			DateTime changedSince=PrefC.GetDateT(PrefName.MobileSyncDateTimeLastRun);
			ShowProgressForm(changedSince);
		}

		private void ShowProgressForm(DateTime changedSince) {
			if(checkTroubleshooting.Checked) {
				_isTroubleshootMode=true;
			}
			else {
				_isTroubleshootMode=false;
			}
			DateTime timeSynchStarted=MiscData.GetNowDateTime();
			FormP=new FormProgress();
			FormP.MaxVal=100;//to keep the form from closing until the real MaxVal is set.
			FormP.NumberMultiplication=1;
			FormP.DisplayText="Preparing records for upload.";
			FormP.NumberFormat="F0";
			//start the thread that will perform the upload
			ThreadStart uploadDelegate= delegate { UploadWorker(changedSince,timeSynchStarted); };
			Thread workerThread=new Thread(uploadDelegate);
			workerThread.Start();
			//display the progress dialog to the user:
			FormP.ShowDialog();
			if(FormP.DialogResult==DialogResult.Cancel) {
				workerThread.Abort();
			}
			_hasPrefsChanged=true;
			textDateTimeLastRun.Text=PrefC.GetDateT(PrefName.MobileSyncDateTimeLastRun).ToShortDateString()+" "+PrefC.GetDateT(PrefName.MobileSyncDateTimeLastRun).ToShortTimeString();
		}


		///<summary>This is the function that the worker thread uses to actually perform the upload.  Can also call this method in the ordinary way if the data to be transferred is small.  The timeSynchStarted must be passed in to ensure that no records are skipped due to small time differences.</summary>
		private static void UploadWorker(DateTime changedSince,DateTime timeSynchStarted) {
			int totalCount=100;
			try {//Dennis: try catch may not work: Does not work in threads, not sure about this. Note that all methods inside this try catch block are without exception handling. This is done on purpose so that when an exception does occur it does not update PrefName.MobileSyncDateTimeLastRun
				//The handling of PrefName.MobileSynchNewTables79 should never be removed in future versions
				DateTime changedProv=changedSince;
				DateTime changedDeleted=changedSince;
				DateTime changedPat=changedSince;
				DateTime changedStatement=changedSince;
				DateTime changedDocument=changedSince;
				DateTime changedRecall=changedSince;
				if(!PrefC.GetBoolSilent(PrefName.MobileSynchNewTables79Done,false)) {
					changedProv=DateTime.MinValue;
					changedDeleted=DateTime.MinValue;
				}
				if(!PrefC.GetBoolSilent(PrefName.MobileSynchNewTables112Done,false)) {
					changedPat=DateTime.MinValue;
					changedStatement=DateTime.MinValue;
					changedDocument=DateTime.MinValue;
				}
				if(!PrefC.GetBoolSilent(PrefName.MobileSynchNewTables121Done,false)) {
					changedRecall=DateTime.MinValue;
					UploadPreference(PrefName.PracticeTitle); //done again because the previous upload did not include the prefnum
				}
				bool synchDelPat=true;
				if(PrefC.GetDateT(PrefName.MobileSyncDateTimeLastRun).Hour==timeSynchStarted.Hour) {
					synchDelPat=false;// synching delPatNumList is timeconsuming (15 seconds) for a dental office with around 5000 patients and it's mostly the same records that have to be deleted every time a synch happens. So it's done only once hourly.
				}
				//MobileWeb
				List<long> patNumList=Patientms.GetChangedSincePatNums(changedPat);
				List<long> aptNumList=Appointmentms.GetChangedSinceAptNums(changedSince,PrefC.GetDate(PrefName.MobileExcludeApptsBeforeDate));
				List<long> rxNumList=RxPatms.GetChangedSinceRxNums(changedSince);
				List<long> provNumList=Providerms.GetChangedSinceProvNums(changedProv);
				List<long> pharNumList=Pharmacyms.GetChangedSincePharmacyNums(changedSince);
				List<long> allergyDefNumList=AllergyDefms.GetChangedSinceAllergyDefNums(changedSince);
				List<long> allergyNumList=Allergyms.GetChangedSinceAllergyNums(changedSince);
				//exclusively Patient Portal
				/*
				List<long> eligibleForUploadPatNumList=Patientms.GetPatNumsEligibleForSynch();
				List<long> labPanelNumList=LabPanelms.GetChangedSinceLabPanelNums(changedSince,eligibleForUploadPatNumList);
				List<long> labResultNumList=LabResultms.GetChangedSinceLabResultNums(changedSince);
				List<long> medicationNumList=Medicationms.GetChangedSinceMedicationNums(changedSince);
				List<long> medicationPatNumList=MedicationPatms.GetChangedSinceMedicationPatNums(changedSince,eligibleForUploadPatNumList);
				List<long> diseaseDefNumList=DiseaseDefms.GetChangedSinceDiseaseDefNums(changedSince);
				List<long> diseaseNumList=Diseasems.GetChangedSinceDiseaseNums(changedSince,eligibleForUploadPatNumList);
				List<long> icd9NumList=ICD9ms.GetChangedSinceICD9Nums(changedSince);
				List<long> statementNumList=Statementms.GetChangedSinceStatementNums(changedStatement,eligibleForUploadPatNumList,statementLimitPerPatient);
				List<long> documentNumList=Documentms.GetChangedSinceDocumentNums(changedDocument,statementNumList);
				List<long> recallNumList=Recallms.GetChangedSinceRecallNums(changedRecall);*/
				List<long> delPatNumList=Patientms.GetPatNumsForDeletion();
				//List<DeletedObject> dO=DeletedObjects.GetDeletedSince(changedDeleted);dennis: delete this line later
				List<long> deletedObjectNumList=DeletedObjects.GetChangedSinceDeletedObjectNums(changedDeleted);//to delete appointments from mobile
				totalCount= patNumList.Count+aptNumList.Count+rxNumList.Count+provNumList.Count+pharNumList.Count
					//+labPanelNumList.Count+labResultNumList.Count+medicationNumList.Count+medicationPatNumList.Count
					//+allergyDefNumList.Count//+allergyNumList.Count+diseaseDefNumList.Count+diseaseNumList.Count+icd9NumList.Count
					//+statementNumList.Count+documentNumList.Count+recallNumList.Count
					+deletedObjectNumList.Count;
				if(synchDelPat) {
					totalCount+=delPatNumList.Count;
				}
				double currentVal=0;
				if(Application.OpenForms["FormProgress"]!=null) {// without this line the following error is thrown: "Invoke or BeginInvoke cannot be called on a control until the window handle has been created." or a null pointer exception is thrown when an automatic synch is done by the system.
					FormP.Invoke(new PassProgressDelegate(PassProgressToDialog),
						new object[] { currentVal,"?currentVal of ?maxVal records uploaded",totalCount,"" });
				}
				_isSynching=true;
				SynchGeneric(patNumList,SynchEntity.patient,totalCount,ref currentVal);
				SynchGeneric(aptNumList,SynchEntity.appointment,totalCount,ref currentVal);
				SynchGeneric(rxNumList,SynchEntity.prescription,totalCount,ref currentVal);
				SynchGeneric(provNumList,SynchEntity.provider,totalCount,ref currentVal);
				SynchGeneric(pharNumList,SynchEntity.pharmacy,totalCount,ref currentVal);
				//pat portal
				/*
				SynchGeneric(labPanelNumList,SynchEntity.labpanel,totalCount,ref currentVal);
				SynchGeneric(labResultNumList,SynchEntity.labresult,totalCount,ref currentVal);
				SynchGeneric(medicationNumList,SynchEntity.medication,totalCount,ref currentVal);
				SynchGeneric(medicationPatNumList,SynchEntity.medicationpat,totalCount,ref currentVal);
				SynchGeneric(allergyDefNumList,SynchEntity.allergydef,totalCount,ref currentVal);
				SynchGeneric(allergyNumList,SynchEntity.allergy,totalCount,ref currentVal);
				SynchGeneric(diseaseDefNumList,SynchEntity.diseasedef,totalCount,ref currentVal);
				SynchGeneric(diseaseNumList,SynchEntity.disease,totalCount,ref currentVal);
				SynchGeneric(icd9NumList,SynchEntity.icd9,totalCount,ref currentVal);
				SynchGeneric(statementNumList,SynchEntity.statement,totalCount,ref currentVal);
				SynchGeneric(documentNumList,SynchEntity.document,totalCount,ref currentVal);
				SynchGeneric(recallNumList,SynchEntity.recall,totalCount,ref currentVal);*/
				if(synchDelPat) {
					SynchGeneric(delPatNumList,SynchEntity.patientdel,totalCount,ref currentVal);
				}
				//DeleteObjects(dO,totalCount,ref currentVal);// this has to be done at this end because objects may have been created and deleted between synchs. If this function is place above then the such a deleted object will not be deleted from the server.
				SynchGeneric(deletedObjectNumList,SynchEntity.deletedobject,totalCount,ref currentVal);// this has to be done at this end because objects may have been created and deleted between synchs. If this function is place above then the such a deleted object will not be deleted from the server.
				if(!PrefC.GetBoolSilent(PrefName.MobileSynchNewTables79Done,true)) {
					Prefs.UpdateBool(PrefName.MobileSynchNewTables79Done,true);
				}
				if(!PrefC.GetBoolSilent(PrefName.MobileSynchNewTables112Done,true)) {
					Prefs.UpdateBool(PrefName.MobileSynchNewTables112Done,true);
				}
				if(!PrefC.GetBoolSilent(PrefName.MobileSynchNewTables121Done,true)) {
					Prefs.UpdateBool(PrefName.MobileSynchNewTables121Done,true);
				}
				Prefs.UpdateDateT(PrefName.MobileSyncDateTimeLastRun,timeSynchStarted);
				_isSynching=false;
			}
			catch(Exception e) {
				_isSynching=false;// this will ensure that the synch can start again. If this variable remains true due to an exception then a synch will never take place automatically.
				if(Application.OpenForms["FormProgress"]!=null) {// without this line the following error is thrown: "Invoke or BeginInvoke cannot be called on a control until the window handle has been created." or a null pointer exception is thrown when an automatic synch is done by the system.
					FormP.Invoke(new PassProgressDelegate(PassProgressToDialog),
						new object[] { 0,"?currentVal of ?maxVal records uploaded",totalCount,e.Message });
				}
			}
		}

		///<summary>a general function to reduce the amount of code for uploading</summary>
		private static void SynchGeneric(List<long> PKNumList,SynchEntity entity,double totalCount,ref double currentVal) {
			//Dennis: a try catch block here has been avoid on purpose.
			List<long> BlockPKNumList=null;
			int localBatchSize=_batchSize;
			if(_isTroubleshootMode) {
				localBatchSize=1;
			}
			string AtoZpath=ImageStore.GetPreferredAtoZpath();
			for(int start=0;start<PKNumList.Count;start+=localBatchSize) {
				if((start+localBatchSize)>PKNumList.Count) {
					localBatchSize=PKNumList.Count-start;
				}
				try {
					BlockPKNumList=PKNumList.GetRange(start,localBatchSize);
					switch(entity) {
						case SynchEntity.patient:
							List<Patientm> changedPatientmList=Patientms.GetMultPats(BlockPKNumList);
							mb.SynchPatients(PrefC.GetString(PrefName.RegistrationKey),changedPatientmList.ToArray());
							break;
						case SynchEntity.appointment:
							List<Appointmentm> changedAppointmentmList=Appointmentms.GetMultApts(BlockPKNumList);
							mb.SynchAppointments(PrefC.GetString(PrefName.RegistrationKey),changedAppointmentmList.ToArray());
							break;
						case SynchEntity.prescription:
							List<RxPatm> changedRxList=RxPatms.GetMultRxPats(BlockPKNumList);
							mb.SynchPrescriptions(PrefC.GetString(PrefName.RegistrationKey),changedRxList.ToArray());
							break;
						case SynchEntity.provider:
							List<Providerm> changedProvList=Providerms.GetMultProviderms(BlockPKNumList);
							mb.SynchProviders(PrefC.GetString(PrefName.RegistrationKey),changedProvList.ToArray());
							break;
						case SynchEntity.pharmacy:
							List<Pharmacym> changedPharmacyList=Pharmacyms.GetMultPharmacyms(BlockPKNumList);
							mb.SynchPharmacies(PrefC.GetString(PrefName.RegistrationKey),changedPharmacyList.ToArray());
							break;
						case SynchEntity.labpanel:
							List<LabPanelm> ChangedLabPanelList=LabPanelms.GetMultLabPanelms(BlockPKNumList);
							mb.SynchLabPanels(PrefC.GetString(PrefName.RegistrationKey),ChangedLabPanelList.ToArray());
							break;
						case SynchEntity.labresult:
							List<LabResultm> ChangedLabResultList=LabResultms.GetMultLabResultms(BlockPKNumList);
							mb.SynchLabResults(PrefC.GetString(PrefName.RegistrationKey),ChangedLabResultList.ToArray());
							break;
						case SynchEntity.medication:
							List<Medicationm> ChangedMedicationList=Medicationms.GetMultMedicationms(BlockPKNumList);
							mb.SynchMedications(PrefC.GetString(PrefName.RegistrationKey),ChangedMedicationList.ToArray());
							break;
						case SynchEntity.medicationpat:
							List<MedicationPatm> ChangedMedicationPatList=MedicationPatms.GetMultMedicationPatms(BlockPKNumList);
							mb.SynchMedicationPats(PrefC.GetString(PrefName.RegistrationKey),ChangedMedicationPatList.ToArray());
							break;
						case SynchEntity.allergy:
							List<Allergym> ChangedAllergyList=Allergyms.GetMultAllergyms(BlockPKNumList);
							mb.SynchAllergies(PrefC.GetString(PrefName.RegistrationKey),ChangedAllergyList.ToArray());
							break;
						case SynchEntity.allergydef:
							List<AllergyDefm> ChangedAllergyDefList=AllergyDefms.GetMultAllergyDefms(BlockPKNumList);
							mb.SynchAllergyDefs(PrefC.GetString(PrefName.RegistrationKey),ChangedAllergyDefList.ToArray());
							break;
						case SynchEntity.disease:
							List<Diseasem> ChangedDiseaseList=Diseasems.GetMultDiseasems(BlockPKNumList);
							mb.SynchDiseases(PrefC.GetString(PrefName.RegistrationKey),ChangedDiseaseList.ToArray());
							break;
						case SynchEntity.diseasedef:
							List<DiseaseDefm> ChangedDiseaseDefList=DiseaseDefms.GetMultDiseaseDefms(BlockPKNumList);
							mb.SynchDiseaseDefs(PrefC.GetString(PrefName.RegistrationKey),ChangedDiseaseDefList.ToArray());
							break;
						case SynchEntity.icd9:
							List<ICD9m> ChangedICD9List=ICD9ms.GetMultICD9ms(BlockPKNumList);
							mb.SynchICD9s(PrefC.GetString(PrefName.RegistrationKey),ChangedICD9List.ToArray());
							break;
						case SynchEntity.statement:
							List<Statementm> ChangedStatementList=Statementms.GetMultStatementms(BlockPKNumList);
							mb.SynchStatements(PrefC.GetString(PrefName.RegistrationKey),ChangedStatementList.ToArray());
							break;
						case SynchEntity.document:
							List<Documentm> ChangedDocumentList=Documentms.GetMultDocumentms(BlockPKNumList,AtoZpath);
							mb.SynchDocuments(PrefC.GetString(PrefName.RegistrationKey),ChangedDocumentList.ToArray());
							break;
						case SynchEntity.recall:
							List<Recallm> ChangedRecallList=Recallms.GetMultRecallms(BlockPKNumList);
							mb.SynchRecalls(PrefC.GetString(PrefName.RegistrationKey),ChangedRecallList.ToArray());
							break;
						case SynchEntity.deletedobject:
							List<DeletedObject> ChangedDeleteObjectList=DeletedObjects.GetMultDeletedObjects(BlockPKNumList);
							mb.DeleteObjects(PrefC.GetString(PrefName.RegistrationKey),ChangedDeleteObjectList.ToArray());
							break;
						case SynchEntity.patientdel:
							mb.DeletePatientsRecords(PrefC.GetString(PrefName.RegistrationKey),BlockPKNumList.ToArray());
							break;
					}
					//progressIndicator.CurrentVal+=LocalBatchSize;//not allowed
					currentVal+=localBatchSize;
					if(Application.OpenForms["FormProgress"]!=null) {// without this line the following error is thrown: "Invoke or BeginInvoke cannot be called on a control until the window handle has been created." or a null pointer exception is thrown when an automatic synch is done by the system.
						FormP.Invoke(new PassProgressDelegate(PassProgressToDialog),
							new object[] { currentVal,"?currentVal of ?maxVal records uploaded",totalCount,"" });
					}
				}
				catch(Exception e) {
					if(_isTroubleshootMode) {
						string errorMessage=entity+ " with Primary Key = "+BlockPKNumList[0].ToString()+" failed to synch. "+"\n"+e.Message;
						throw new Exception(errorMessage);
					}
					else {
						throw e;
					}
				}
			}//for loop ends here
		}

		///<summary>This method gets invoked from the worker thread.</summary>
		private static void PassProgressToDialog(double currentVal,string displayText,double maxVal,string errorMessage) {
			FormP.CurrentVal=currentVal;
			FormP.DisplayText=displayText;
			FormP.MaxVal=maxVal;
			FormP.ErrorMessage=errorMessage;
		}

		/*
		private static void DeleteObjects(List<DeletedObject> dO,double totalCount,ref double currentVal) {
			int LocalBatchSize=BatchSize;
			if(IsTroubleshootMode) {
				LocalBatchSize=1;
			}
			for(int start=0;start<dO.Count;start+=LocalBatchSize) {
				try {
				if((start+LocalBatchSize)>dO.Count) {
					mb.DeleteObjects(PrefC.GetString(PrefName.RegistrationKey),dO.ToArray()); //dennis check this - why is it not done in batches.
					LocalBatchSize=dO.Count-start;
				}
				currentVal+=BatchSize;
				if(Application.OpenForms["FormProgress"]!=null) {// without this line the following error is thrown: "Invoke or BeginInvoke cannot be called on a control until the window handle has been created." or a null pointer exception is thrown when an automatic synch is done by the system.
					FormP.Invoke(new PassProgressDelegate(PassProgressToDialog),
						new object[] {currentVal,"?currentVal of ?maxVal records uploaded",totalCount,"" });
				}
								}
				catch(Exception e) {
					if(IsTroubleshootMode) {
						//string errorMessage="DeleteObjects with Primary Key = "+BlockPKNumList.First() + " failed to synch. " +  "\n" + e.Message;
						//throw new Exception(errorMessage);
					}
					else {
						throw e;
					}
				}
			}//for loop ends here
			
		}
		*/
		/// <summary>An empty method to test if the webservice is up and running. This was made with the intention of testing the correctness of the webservice URL. If an incorrect webservice URL is used in a background thread the exception cannot be handled easily to a point where even a correct URL cannot be keyed in by the user. Because an exception in a background thread closes the Form which spawned it.</summary>
		private static bool TestWebServiceExists() {
			try {
				mb.Url=PrefC.GetString(PrefName.MobileSyncServerURL);
				if(mb.ServiceExists()) {
					return true;
				}
			}
			catch {
				return false;
			}
			return false;
		}

		private bool VerifyPaidCustomer() {
			//if(textMobileSyncServerURL.Text.Contains("192.168.0.196") || textMobileSyncServerURL.Text.Contains("localhost")) {
			if(textMobileSyncServerURL.Text.Contains("10.10.1.196") || textMobileSyncServerURL.Text.Contains("localhost")) {
				IgnoreCertificateErrors();
			}
			bool isPaidCustomer=mb.IsPaidCustomer(PrefC.GetString(PrefName.RegistrationKey));
			if(!isPaidCustomer) {
				textSynchMinutes.Text="0";
				Prefs.UpdateInt(PrefName.MobileSyncIntervalMinutes,0);
				_hasPrefsChanged=true;
				MsgBox.Show(this,"This feature requires a separate monthly payment.  Please call customer support.");
				return false;
			}
			return true;
		}

		///<summary>Called from FormOpenDental and from FormEhrOnlineAccess.  doForce is set to false to follow regular synching interval.</summary>
		public static void SynchFromMain(bool doForce) {
			if(Application.OpenForms["FormPatientPortalSetup"]!=null) {//tested.  This prevents main synch whenever this form is open.
				return;
			}
			if(_isSynching) {
				return;
			}
			DateTime timeSynchStarted=MiscData.GetNowDateTime();
			if(!doForce) {//if doForce, we skip checking the interval
				if(timeSynchStarted < PrefC.GetDateT(PrefName.MobileSyncDateTimeLastRun).AddMinutes(PrefC.GetInt(PrefName.MobileSyncIntervalMinutes))) {
					return;
				}
			}
			//if(PrefC.GetString(PrefName.MobileSyncServerURL).Contains("192.168.0.196") || PrefC.GetString(PrefName.MobileSyncServerURL).Contains("localhost")) {
			if(PrefC.GetString(PrefName.MobileSyncServerURL).Contains("10.10.1.196") || PrefC.GetString(PrefName.MobileSyncServerURL).Contains("localhost")) {
				IgnoreCertificateErrors();
			}
			if(!TestWebServiceExists()) {
				if(!doForce) {//if being used from FormOpenDental as part of timer
					if(_isServerAvail) {//this will only happen the first time to prevent multiple windows.
						_isServerAvail=false;
						DialogResult res=MessageBox.Show("Mobile synch server not available.  Synch failed.  Turn off synch?","",MessageBoxButtons.YesNo);
						if(res==DialogResult.Yes) {
							Prefs.UpdateInt(PrefName.MobileSyncIntervalMinutes,0);
						}
					}
				}
				return;
			}
			else {
				_isServerAvail=true;
			}
			DateTime changedSince=PrefC.GetDateT(PrefName.MobileSyncDateTimeLastRun);
			//FormProgress FormP=new FormProgress();//but we won't display it.
			//FormP.NumberFormat="";
			//FormP.DisplayText="";
			//start the thread that will perform the upload
			ThreadStart uploadDelegate= delegate { UploadWorker(changedSince,timeSynchStarted); };
			Thread workerThread=new Thread(uploadDelegate);
			workerThread.Start();
		}

		#region Testing
		///<summary>This allows the code to continue by not throwing an exception even if there is a problem with the security certificate.</summary>
		private static void IgnoreCertificateErrors() {
			System.Net.ServicePointManager.ServerCertificateValidationCallback+=
			delegate(object sender,System.Security.Cryptography.X509Certificates.X509Certificate certificate,
									System.Security.Cryptography.X509Certificates.X509Chain chain,
									System.Net.Security.SslPolicyErrors sslPolicyErrors) {
				return true;
			};
		}

		/// <summary>For testing only</summary>
		private static void CreatePatients(int PatientCount) {
			for(int i=0;i<PatientCount;i++) {
				Patient newPat=new Patient();
				newPat.LName="Mathew"+i;
				newPat.FName="Dennis"+i;
				newPat.Address="Address Line 1.Address Line 1___"+i;
				newPat.Address2="Address Line 2. Address Line 2__"+i;
				newPat.AddrNote="Lives off in far off Siberia Lives off in far off Siberia"+i;
				newPat.AdmitDate=new DateTime(1985,3,3).AddDays(i);
				newPat.ApptModNote="Flies from Siberia on specially chartered flight piloted by goblins:)"+i;
				newPat.AskToArriveEarly=1555;
				newPat.BillingType=3;
				newPat.ChartNumber="111111"+i;
				newPat.City="NL";
				newPat.ClinicNum=i;
				newPat.CreditType="A";
				newPat.DateFirstVisit=new DateTime(1985,3,3).AddDays(i);
				newPat.Email="dennis.mathew________________seb@siberiacrawlmail.com";
				newPat.HmPhone="416-222-5678";
				newPat.WkPhone="416-222-5678";
				newPat.Zip="M3L 2L9";
				newPat.WirelessPhone="416-222-5678";
				newPat.Birthdate=new DateTime(1970,3,3).AddDays(i);
				Patients.Insert(newPat,false);
				//set Guarantor field the same as PatNum
				Patient patOld=newPat.Copy();
				newPat.Guarantor=newPat.PatNum;
				Patients.Update(newPat,patOld);
			}
		}

		/// <summary>For testing only</summary>
		private static void CreateAppointments(int AppointmentCount) {
			long[] patNumArray=Patients.GetAllPatNums(true);
			DateTime appdate= DateTime.Now;
			for(int i=0;i<patNumArray.Length;i++) {
				appdate=appdate.AddMinutes(20);
				for(int j=0;j<AppointmentCount;j++) {
					Appointment apt=new Appointment();
					appdate=appdate.AddMinutes(20);
					apt.PatNum=patNumArray[i];
					apt.DateTimeArrived=appdate;
					apt.DateTimeAskedToArrive=appdate;
					apt.DateTimeDismissed=appdate;
					apt.DateTimeSeated=appdate;
					apt.AptDateTime=appdate;
					apt.Note="some notenote noten otenotenot enotenot enote"+j;
					apt.IsNewPatient=true;
					apt.ProvNum=3;
					apt.AptStatus=ApptStatus.Scheduled;
					apt.AptDateTime=appdate;
					apt.Op=2;
					apt.Pattern="//XX//////";
					apt.ProcDescript="4-BWX";
					apt.ProcsColored="<span color=\"-16777216\">4-BWX</span>";
					Appointments.Insert(apt);
				}
			}
		}

		/// <summary>For testing only</summary>
		private static void CreatePrescriptions(int PrescriptionCount) {
			long[] patNumArray=Patients.GetAllPatNums(true);
			for(int i=0;i<patNumArray.Length;i++) {
				for(int j=0;j<PrescriptionCount;j++) {
					RxPat rxpat= new RxPat();
					rxpat.Drug="VicodinA VicodinB VicodinC"+j;
					rxpat.Disp="50.50";
					rxpat.IsControlled=true;
					rxpat.PatNum=patNumArray[i];
					rxpat.RxDate=new DateTime(2010,12,1,11,0,0);
					RxPats.Insert(rxpat);
				}
			}
		}

		private static void CreateStatements(int StatementCount) {
			long[] patNumArray=Patients.GetAllPatNums(true);
			for(int i=0;i<patNumArray.Length;i++) {
				for(int j=0;j<StatementCount;j++) {
					Statement st= new Statement();
					st.DateSent=new DateTime(2010,12,1,11,0,0).AddDays(1+j);
					st.DocNum=i+j;
					st.PatNum=patNumArray[i];
					Statements.Insert(st);
				}
			}
		}

		#endregion Testing

		#endregion

		#region Web Sched

		private void butWebSchedEnable_Click(object sender,EventArgs e) {
			labelWebSchedEnable.Text="";
			Application.DoEvents();
			//The enable button is not enabled for offices that already have the service enabled.  Therefore go straight to making the web call to our service.
			Cursor.Current=Cursors.WaitCursor;
			string error="";
			try {
				Recalls.ValidateWebSched();
			}
			catch(Exception ex) {
				//Prep a generic error response just in case something unexpected went wrong.
				error=Lan.g(this,"There was a problem enabling the Recall Web Sched service.  Please give us a call or try again.");
				if(ex.GetType()==typeof(ODException)) {
					error=ex.Message;//Show special errors for ODExceptions that were already translated.
					//At this point we know something went wrong.  So we need to give the user a hint as to why they can't enable
					if(((ODException)ex).ErrorCode==110) {//Customer not registered for Web Sched monthly service
						//We want to launch our Web Sched page if the user is not signed up:
						try {
							Process.Start(Recalls.GetWebSchedPromoURL());
						}
						catch(Exception) {
							//The promotional web site can't be shown, most likely due to the computer not having a default browser.  Simply do nothing.
						}
					}
				}
			}
			Cursor.Current=Cursors.Default;
			if(error!="") {
				//Just in case no browser was opened for them, make the message next to the button say something now so that they can visually see that something should have happened.
				labelWebSchedEnable.Text=error;
				MessageBox.Show(error);
				return;
			}
			//Everything went good, the office is actively on support and has an active WebSched repeating charge.
			butWebSchedEnable.Enabled=false;
			labelWebSchedEnable.Text=Lan.g(this,"The Recall Web Sched service has been enabled.");
			//This if statement will only save database calls in the off chance that this window was originally loaded with the pref turned off and got turned on by another computer while open.
			if(Prefs.UpdateBool(PrefName.WebSchedService,true)) {
				_hasPrefsChanged=true;
				SecurityLogs.MakeLogEntry(Permissions.EServicesSetup,0,"The Recall Web Sched service was enabled.");
			}
		}

		private void butSignUp_Click(object sender,EventArgs e) {
			try {
				Process.Start(Recalls.GetWebSchedPromoURL());
			}
			catch(Exception) {
				//The promotional web site can't be shown, most likely due to the computer not having a default browser.
				MessageBox.Show(Lan.g(this,"Sign up page could not load.  Please visit the following web site")+":\r\n"+Recalls.GetWebSchedPromoURL());
			}
		}

		///<summary>Shows the Operatories window and allows the user to edit them.  Does not show the window if user does not have Setup permission.
		///Refreshes all corresponding grids within the Web Sched tab that display Operatory information.  Feel free to add to this method.</summary>
		private void ShowOperatoryEditAndRefreshGrids() {
			if(!Security.IsAuthorized(Permissions.Setup)) {
				return;
			}
			FormOperatories FormO=new FormOperatories();
			FormO.ShowDialog();
			FillGridWebSchedOperatories();
			FillGridWebSchedTimeSlots();
			FillGridWebSchedNewPatApptOps();
			FillGridWebSchedNewPatApptTimeSlots();
			SecurityLogs.MakeLogEntry(Permissions.Setup,0,"Operatories accessed via EServices Setup window.");
		}

		#region Web Sched - Recalls
		///<summary>Also refreshed the combo box of available recall types.</summary>
		private void FillGridWebSchedRecallTypes() {
			//Keep track of the previously selected recall type.
			long selectedRecallTypeNum=0;
			if(comboWebSchedRecallTypes.SelectedIndex!=-1) {
				selectedRecallTypeNum=_listRecallTypes[comboWebSchedRecallTypes.SelectedIndex].RecallTypeNum;
			}
			//Fill the combo boxes for the time slots preview.
			comboWebSchedRecallTypes.Items.Clear();
			_listRecallTypes=RecallTypeC.GetListt();
			for(int i=0;i<_listRecallTypes.Count;i++) {
				comboWebSchedRecallTypes.Items.Add(_listRecallTypes[i].Description);
				if(_listRecallTypes[i].RecallTypeNum==selectedRecallTypeNum) {
					comboWebSchedRecallTypes.SelectedIndex=i;
				}
			}
			if(selectedRecallTypeNum==0 && comboWebSchedRecallTypes.Items.Count > 0) {
				comboWebSchedRecallTypes.SelectedIndex=0;//Arbitrarily select the first recall type.
			}
			gridWebSchedRecallTypes.BeginUpdate();
			gridWebSchedRecallTypes.Columns.Clear();
			ODGridColumn col=new ODGridColumn(Lan.g("TableRecallTypes","Description"),130);
			gridWebSchedRecallTypes.Columns.Add(col);
			col=new ODGridColumn(Lan.g("TableRecallTypes","Time Pattern"),100);
			gridWebSchedRecallTypes.Columns.Add(col);
			col=new ODGridColumn(Lan.g("TableRecallTypes","Time Length"),0);
			col.TextAlign=HorizontalAlignment.Center;
			gridWebSchedRecallTypes.Columns.Add(col);
			gridWebSchedRecallTypes.Rows.Clear();
			ODGridRow row;
			for(int i=0;i<_listRecallTypes.Count;i++) {
				row=new ODGridRow();
				row.Cells.Add(_listRecallTypes[i].Description);
				row.Cells.Add(_listRecallTypes[i].TimePattern);
				int timeLength=RecallTypes.ConvertTimePattern(_listRecallTypes[i].TimePattern).Length * 5;
				if(timeLength==0) {
					row.Cells.Add("");
				}
				else {
					row.Cells.Add(timeLength.ToString()+" "+Lan.g("TableRecallTypes","mins"));
				}
				gridWebSchedRecallTypes.Rows.Add(row);
			}
			gridWebSchedRecallTypes.EndUpdate();
		}

		private void FillGridWebSchedOperatories() {
			_listWebSchedRecallOps=Operatories.GetOpsForWebSched();
			int opNameWidth=170;
			int clinicWidth=80;
			if(!PrefC.HasClinicsEnabled) {
				opNameWidth+=clinicWidth;
			}
			gridWebSchedOperatories.BeginUpdate();
			gridWebSchedOperatories.Columns.Clear();
			gridWebSchedOperatories.Columns.Add(new ODGridColumn(Lan.g("TableOperatories","Op Name"),opNameWidth));
			gridWebSchedOperatories.Columns.Add(new ODGridColumn(Lan.g("TableOperatories","Abbrev"),70));
			if(PrefC.HasClinicsEnabled) {
				gridWebSchedOperatories.Columns.Add(new ODGridColumn(Lan.g("TableOperatories","Clinic"),clinicWidth));
			}
			gridWebSchedOperatories.Columns.Add(new ODGridColumn(Lan.g("TableOperatories","Provider"),90));
			gridWebSchedOperatories.Columns.Add(new ODGridColumn(Lan.g("TableOperatories","Hygienist"),90));
			gridWebSchedOperatories.Rows.Clear();
			ODGridRow row;
			for(int i=0;i<_listWebSchedRecallOps.Count;i++) {
				row=new ODGridRow();
				row.Cells.Add(_listWebSchedRecallOps[i].OpName);
				row.Cells.Add(_listWebSchedRecallOps[i].Abbrev);
				if(PrefC.HasClinicsEnabled) {
					row.Cells.Add(Clinics.GetAbbr(_listWebSchedRecallOps[i].ClinicNum));
				}
				row.Cells.Add(Providers.GetAbbr(_listWebSchedRecallOps[i].ProvDentist));
				row.Cells.Add(Providers.GetAbbr(_listWebSchedRecallOps[i].ProvHygienist));
				gridWebSchedOperatories.Rows.Add(row);
			}
			gridWebSchedOperatories.EndUpdate();
		}

		private void FillGridWebSchedTimeSlots() {
			//Validate time slot settings.
			if(textWebSchedDateStart.errorProvider1.GetError(textWebSchedDateStart)!="") {
				//Don't bother warning the user.  It will just be annoying.  The red indicator should be sufficient.
				return;
			}
			if(comboWebSchedRecallTypes.SelectedIndex < 0
				|| comboWebSchedClinic.SelectedIndex < 0
				|| comboWebSchedProviders.SelectedIndex < 0) 
			{
				return;
			}
			DateTime dateStart=PIn.Date(textWebSchedDateStart.Text);
			RecallType recallType=_listRecallTypes[comboWebSchedRecallTypes.SelectedIndex];
			Clinic clinic=_smsListAllClinics.Find(x => x.ClinicNum==_webSchedClinicNum);//null clinic is treated as unassigned.
			List<Provider> listProviders=new List<Provider>(_listProviders);//Use all providers by default.
			Provider provider=_listProviders.Find(x => x.ProvNum==_webSchedProvNum);
			if(provider!=null) {
				//Only use the provider that the user picked from the provider picker.
				listProviders=new List<Provider>() { provider };
			}
			Cursor=Cursors.WaitCursor;
			List<TimeSlot> listTimeSlots=new List<TimeSlot>();
			try {
				//Get the next 30 days of open time schedules with the current settings
				listTimeSlots=TimeSlots.GetAvailableWebSchedTimeSlots(recallType,listProviders,clinic,dateStart,dateStart.AddDays(30));
			}
			catch(Exception) {
				//The user might not have Web Sched ops set up correctly.  Don't warn them here because it is just annoying.  They'll figure it out.
			}
			Cursor=Cursors.Default;
			gridWebSchedTimeSlots.BeginUpdate();
			gridWebSchedTimeSlots.Columns.Clear();
			ODGridColumn col=new ODGridColumn("",0);
			col.TextAlign=HorizontalAlignment.Center;
			gridWebSchedTimeSlots.Columns.Add(col);
			gridWebSchedTimeSlots.Rows.Clear();
			ODGridRow row;
			DateTime dateTimeSlotLast=DateTime.MinValue;
			foreach(TimeSlot timeSlot in listTimeSlots) {
				//Make a new row for every unique day.
				if(dateTimeSlotLast.Date!=timeSlot.DateTimeStart.Date) {
					dateTimeSlotLast=timeSlot.DateTimeStart;
					row=new ODGridRow();
					row.ColorBackG=Color.LightBlue;
					row.Cells.Add(timeSlot.DateTimeStart.ToShortDateString());
					gridWebSchedTimeSlots.Rows.Add(row);
				}
				row=new ODGridRow();
				row.Cells.Add(timeSlot.DateTimeStart.ToShortTimeString()+" - "+timeSlot.DateTimeStop.ToShortTimeString());
				gridWebSchedTimeSlots.Rows.Add(row);
			}
			gridWebSchedTimeSlots.EndUpdate();
		}

		private void gridWebSchedRecallTypes_DoubleClick(object sender,EventArgs e) {
			if(!Security.IsAuthorized(Permissions.Setup)) {
				return;
			}
			FormRecallTypes FormRT=new FormRecallTypes();
			FormRT.ShowDialog();
			FillGridWebSchedRecallTypes();
			FillGridWebSchedTimeSlots();
			SecurityLogs.MakeLogEntry(Permissions.Setup,0,"Recall Types accessed via EServices Setup window.");
		}

		private void gridWebSchedOperatories_DoubleClick(object sender,EventArgs e) {
			ShowOperatoryEditAndRefreshGrids();
		}

		private void listBoxWebSchedProviderPref_SelectedIndexChanged(object sender,EventArgs e) {
			if(Prefs.UpdateInt(PrefName.WebSchedProviderRule,listBoxWebSchedProviderPref.SelectedIndex)) {
				_hasPrefsChanged=true;
			}
		}

		private void comboWebSchedRecallTypes_SelectionChangeCommitted(object sender,EventArgs e) {
			FillGridWebSchedTimeSlots();
		}

		private void comboWebSchedProviders_SelectionChangeCommitted(object sender,EventArgs e) {
			_webSchedProvNum=0;
			if(comboWebSchedProviders.SelectedIndex > 0) {//Greater than 0 due to "All"
				_webSchedProvNum=_listProviders[comboWebSchedProviders.SelectedIndex-1].ProvNum;//-1 for 'All'
			}
			FillGridWebSchedTimeSlots();
		}

		private void comboWebSchedClinic_SelectionChangeCommitted(object sender,EventArgs e) {
			_webSchedClinicNum=0;
			if(comboWebSchedClinic.SelectedIndex > 0) {//Greater than 0 due to "Unassigned"
				_webSchedClinicNum=_smsListAllClinics[comboWebSchedClinic.SelectedIndex-1].ClinicNum;//-1 for 'Unassigned'
			}
			FillGridWebSchedTimeSlots();
		}

		private void textWebSchedDateStart_TextChanged(object sender,EventArgs e) {
			//Only refresh the grid if the user has typed in a valid date.
			if(textWebSchedDateStart.errorProvider1.GetError(textWebSchedDateStart)=="") {
				FillGridWebSchedTimeSlots();
			}
		}

		private void WebSchedRecallAutoSendRadioButtons_CheckedChanged(object sender,EventArgs e) {
			//Only do validation when the Web Sched tab is selected and Do Not Send is NOT checked.
			if(tabControl.SelectedTab!=tabWebSched || radioDoNotSend.Checked) {
				return;
			}
			//Validate the following recall setup preferences.  See task #880961 or #879613 for more details.
			//1. The Days Past field is not blank
			//2. The Initial Reminder field is greater than 0
			//3. The Second(or more) Reminder field is greater than 0
			List<string> listSetupErrors=new List<string>();
			Dictionary<string,Pref> dictPrefs=PrefC.GetDict();
			if(PrefC.GetLong(PrefName.RecallDaysPast,dictPrefs)==-1) {//Days Past field
				listSetupErrors.Add("- "+Lan.g(this,"Days Past (e.g. 1095, blank, etc) field cannot be blank."));
			}
			if(PrefC.GetLong(PrefName.RecallShowIfDaysFirstReminder,dictPrefs) < 1) {//Initial Reminder field
				listSetupErrors.Add("- "+Lan.g(this,"Initial Reminder field has to be greater than 0."));
			}
			if(PrefC.GetLong(PrefName.RecallShowIfDaysSecondReminder,dictPrefs) < 1) {//Second(or more) Reminder field
				listSetupErrors.Add("- "+Lan.g(this,"Second (or more) Reminder field has to be greater than 0."));
			}
			if(listSetupErrors.Count > 0) {
				//Check the "Do Not Send" radio button which will automatically uncheck all the other radio buttons in the group box.
				radioDoNotSend.Checked=true;
				MessageBox.Show(Lan.g(this,"Recall Setup settings are not correctly set in order to Send Messages Automatically to patients:")
						+"\r\n"+string.Join("\r\n",listSetupErrors)
					,Lan.g(this,"Web Sched - Recall Setup Error"));
			}
		}

		private void butWebSchedSetup_Click(object sender,EventArgs e) {
			if(!Security.IsAuthorized(Permissions.Setup)) {
				return;
			}
			FormRecallSetup FormRS=new FormRecallSetup();
			FormRS.ShowDialog();
			SecurityLogs.MakeLogEntry(Permissions.Setup,0,"Recall Setup accessed via EServices Setup window.");
		}

		private void butWebSchedToday_Click(object sender,EventArgs e) {
			textWebSchedDateStart.Text=DateTime.Today.ToShortDateString();
			//Don't need to call FillTimeSlots because textChanged event already calls it.
		}

		private void butWebSchedPickProv_Click(object sender,EventArgs e) {
			FormProviderPick FormPP=new FormProviderPick();
			if(comboWebSchedProviders.SelectedIndex>0) {
				FormPP.SelectedProvNum=_webSchedProvNum;
			}
			FormPP.ShowDialog();
			if(FormPP.DialogResult!=DialogResult.OK) {
				return;
			}
			comboWebSchedProviders.SelectedIndex=_listProviders.FindIndex(x => x.ProvNum==FormPP.SelectedProvNum)+1;//+1 for 'All'
			_webSchedProvNum=FormPP.SelectedProvNum;
			FillGridWebSchedTimeSlots();
		}

		private void butWebSchedPickClinic_Click(object sender,EventArgs e) {
			FormClinics FormC=new FormClinics();
			FormC.IsSelectionMode=true;
			FormC.ShowDialog();
			if(FormC.DialogResult!=DialogResult.OK) {
				return;
			}
			comboWebSchedClinic.SelectedIndex=_smsListAllClinics.FindIndex(x => x.ClinicNum==FormC.SelectedClinicNum)+1;//+1 for 'Unassigned'
			_webSchedClinicNum=FormC.SelectedClinicNum;
			FillGridWebSchedTimeSlots();
		}
		#endregion

		#region Web Sched - New Pat Appts
		private void textWebSchedNewPatApptLength_Leave(object sender,EventArgs e) {
			//Only refresh if the value of this preference changed.  _indexLastNewPatURL will be set to -1 if a refresh is needed.
			if(_indexLastNewPatURL==-1) {
				FillGridWebSchedNewPatApptTimeSlots();
			}
		}

		private void textWebSchedNewPatApptSearchDays_Leave(object sender,EventArgs e) {
			//Only refresh if the value of this preference changed.  _indexLastNewPatURL will be set to -1 if a refresh is needed.
			if(_indexLastNewPatURL==-1) {
				FillGridWebSchedNewPatApptTimeSlots();
			}
		}

		private void FillGridWebSchedNewPatApptProcs() {
			List<string> listProcCodes=PrefC.GetString(PrefName.WebSchedNewPatApptProcs)
				.Split(new char[] { ',' },StringSplitOptions.RemoveEmptyEntries)
				.ToList();
			gridWebSchedNewPatApptProcs.BeginUpdate();
			gridWebSchedNewPatApptProcs.Columns.Clear();
			gridWebSchedNewPatApptProcs.Columns.Add(new ODGridColumn(Lan.g(this,"Proc Codes"),0,HorizontalAlignment.Center));
			gridWebSchedNewPatApptProcs.Rows.Clear();
			ODGridRow row;
			foreach(string procCode in listProcCodes) {
				row=new ODGridRow();
				row.Cells.Add(procCode);
				row.Tag=procCode;
				gridWebSchedNewPatApptProcs.Rows.Add(row);
			}
			gridWebSchedNewPatApptProcs.EndUpdate();
		}

		private void gridWebSchedNewPatApptURLs_CellClick(object sender,ODGridClickEventArgs e) {
			if(e.Row==_indexLastNewPatURL) {
				return;
			}
			FillGridWebSchedNewPatApptTimeSlots();
		}

		private void gridWebSchedNewPatApptURLs_DoubleClick(object sender,EventArgs e) {
			//Only go get the URLs again on double click if there was an error.
			if(_hasWebSchedNewPatApptURLError) {
				GetWebSchedNewPatApptHostedURLs();
			}
		}

		private void gridWebSchedNewPatApptURLs_CellDoubleClick(object sender,ODGridClickEventArgs e) {
			//Open the URL that the user double clicked on in case they are curious to see how the Web Sched app works.
			NavigateToURL(((NewPatApptURL)gridWebSchedNewPatApptURLs.Rows[e.Row].Tag).URL);
		}

		///<summary>Spawns a thread that makes a call to one of our web methods in order to get URL information for each clinic.
		///Once a response has been recieved, the grid that displays the URLs will be updated.</summary>
		private void GetWebSchedNewPatApptHostedURLs() {
			_threadGetWebSchedNewPatApptHostedURLs=new ODThread(GetWebSchedNewPatApptHostedURLsWorker);
			_threadGetWebSchedNewPatApptHostedURLs.AddThreadExitHandler(GetWebSchedNewPatApptHostedURLsExited);
			_threadGetWebSchedNewPatApptHostedURLs.AddExceptionHandler(GetWebSchedNewPatApptHostedURLsException);
			_threadGetWebSchedNewPatApptHostedURLs.Start();
		}

		///<summary>Makes a web call to get all of the URLs from HQ.  Throws exceptions.
		///If no exception is thrown then the web call was successful and that _listWebSchedNewPatApptHostedURLs has been filled with URLs.</summary>
		private void GetWebSchedNewPatApptHostedURLsWorker(ODThread thread) {
			_indexLastNewPatURL=-1;//Will eventually force the time slots grid to refresh.
			_hasWebSchedNewPatApptURLError=false;
			List<long> listClinicNums=new List<long>() { 0 };//Always start with 0 which represents headquarters (mainly for non-clinic users).
			List<Clinic> listClinics=new List<Clinic>();
			if(PrefC.HasClinicsEnabled) {
				listClinics=Clinics.GetList().ToList();
				listClinicNums.AddRange(listClinics.Select(x => x.ClinicNum));
			}
			//Make a web call to HQ to get the URLs for all the clinics.
			string response=WebServiceMainHQProxy.GetWebServiceMainHQInstance()
					.BuildWebSchedNewPatApptURLs(PrefC.GetString(PrefName.RegistrationKey),String.Join("|",listClinicNums));
			//Parse Response
			XmlDocument doc=new XmlDocument();
			XmlNode nodeError=null;
			XmlNode nodeResponse=null;
			XmlNodeList nodeURLs=null;
			try {
				doc.LoadXml(response);
				nodeError=doc.SelectSingleNode("//Error");
				nodeResponse=doc.SelectSingleNode("//GetWebSchedURLsResponse");
			}
			catch {
				//Invalid web service response passed in.  Node will be null and will throw correctly.
			}
			#region Error Handling
			if(nodeError!=null || nodeResponse==null) {
				string error=Lans.g("WebSched","There was an error with the web request.  Please try again or give us a call.");
				//Either something went wrong or someone tried to get cute and use our Web Sched service when they weren't supposed to.
				if(nodeError!=null) {
					error+="\r\n"+Lans.g("WebSched","Error Details")+":\r\n" +nodeError.InnerText;
				}
				throw new Exception(error);
			}
			nodeURLs=doc.GetElementsByTagName("URL");
			if(nodeURLs==null) {
				throw new Exception("Invalid response from server recieved.");
			}
			#endregion
			_listWebSchedNewPatApptHostedURLs=new List<NewPatApptURL>();
			//At this point we know we got a valid response from our web service.  Fill _listWebSchedNewPatApptHostedURLs with the payload results.
			//Loop through all the URL nodes that were returned.
			//Each URL node will contain a CN attribute which will be the corresponding ClinicNum.
			foreach(XmlNode node in nodeURLs) {
				long clinicNumIn=0;
				XmlAttribute attributeClinicNum=node.Attributes["CN"];
				if(attributeClinicNum!=null) {
					clinicNumIn=PIn.Long(attributeClinicNum.Value);
				}
				_listWebSchedNewPatApptHostedURLs.Add(new NewPatApptURL() {
					ClinicNum=clinicNumIn,
					Clinic=listClinics.FirstOrDefault(x => x.ClinicNum==clinicNumIn),
					URL=node.InnerText
				});
			}
		}

		private void GetWebSchedNewPatApptHostedURLsExited(ODThread thread) {
			if(this.InvokeRequired) {
				this.BeginInvoke((Action)delegate () { GetWebSchedNewPatApptHostedURLsExited(thread); });
				return;
			}
			if(!_hasWebSchedNewPatApptURLError) {
				FillGridWebSchedNewPatApptHostedURLs();
				FillGridWebSchedNewPatApptTimeSlots();
			}
			thread.QuitAsync();
		}

		private void GetWebSchedNewPatApptHostedURLsException(Exception e) {
			if(this.InvokeRequired) {
				this.BeginInvoke((Action)delegate () { GetWebSchedNewPatApptHostedURLsException(e); });
				return;
			}
			//Set the error boolean to true so that users can double click on the grid to try and download the URLs again.
			_hasWebSchedNewPatApptURLError=true;
			_threadGetWebSchedNewPatApptHostedURLs.QuitAsync();
			_threadGetWebSchedNewPatApptHostedURLs=null;//Might not be necessary
			//Let the user know to double click in order to try downloading the URLs again.
			gridWebSchedNewPatApptURLs.BeginUpdate();
			gridWebSchedNewPatApptURLs.Title=Lan.g(this,"Error Retrieving URLs - Double Click to Retry");
			gridWebSchedNewPatApptURLs.EndUpdate();
		}

		private void FillGridWebSchedNewPatApptHostedURLs() {
			int selectedIndex=gridWebSchedNewPatApptURLs.GetSelectedIndex();
			long selectedClinicNum=-1;
			if(selectedIndex > -1) {
				selectedClinicNum=((NewPatApptURL)gridWebSchedNewPatApptURLs.Rows[selectedIndex].Tag).ClinicNum;
			}
			gridWebSchedNewPatApptURLs.BeginUpdate();
			//Always update the grids title because the user could have had an error on load and double clicked to retry.
			gridWebSchedNewPatApptURLs.Title=Lan.g(this,"Hosted URLs");
			gridWebSchedNewPatApptURLs.Columns.Clear();
			gridWebSchedNewPatApptURLs.Columns.Add(new ODGridColumn(Lan.g(this,"Location"),100));
			gridWebSchedNewPatApptURLs.Columns.Add(new ODGridColumn(Lan.g(this,"Excluded"),55,HorizontalAlignment.Center));
			gridWebSchedNewPatApptURLs.Columns.Add(new ODGridColumn(Lan.g(this,"URL"),0));
			gridWebSchedNewPatApptURLs.Rows.Clear();
			ODGridRow row;
			foreach(NewPatApptURL newPatApptURL in _listWebSchedNewPatApptHostedURLs) {
				//Always add a row that represents headquarters as the very first row.  Every office will ALWAYS have a "headquarters" row available.
				row=new ODGridRow();
				if(newPatApptURL.ClinicNum==0) {
					row.Cells.Add(Lan.g(this,"Headquarters"));
					row.Cells.Add(PrefC.GetBool(PrefName.WebSchedNewPatApptEnabled) ? "" : "X");
					row.Cells.Add(newPatApptURL.URL);
				}
				else {
					if(newPatApptURL.Clinic==null) {
						continue;//This shouldn't happen.
					}
					row.Cells.Add(newPatApptURL.Clinic.Abbr);
					row.Cells.Add(newPatApptURL.Clinic.IsNewPatApptExcluded ? "X" : "" );
					row.Cells.Add(newPatApptURL.URL);
				}
				row.Tag=newPatApptURL;
				gridWebSchedNewPatApptURLs.Rows.Add(row);
			}
			gridWebSchedNewPatApptURLs.EndUpdate();
			if(_listWebSchedNewPatApptHostedURLs.Count < 1) {//This should never happen.
				return;//No row to select / preserve so just return and do nothing.
			}
			//Now to select headquarters OR keep whatever clinic they had selected when this fill was called.
			gridWebSchedNewPatApptURLs.SetSelected(false);
			int indexDesired=0;
			if(selectedClinicNum > -1) {
				indexDesired=_listWebSchedNewPatApptHostedURLs.FindIndex(x => x.ClinicNum==selectedClinicNum);
				if(indexDesired==-1) {
					indexDesired=0;//There should always be at least one row for HQ.
				}
			}
			gridWebSchedNewPatApptURLs.SetSelected(indexDesired,true);
		}

		private void menuWebSchedNewPatApptHostedURLsRightClick_Popup(object sender,EventArgs e) {
			//Always default the menu item to exclude
			menuItemExcludeLocation.Text=Lan.g(this,"Exclude Location");
			if(gridWebSchedNewPatApptURLs.GetSelectedIndex() < 0) {
				return;
			}
			//Try and determine what the text for the "Exclude / Include" menu item should be.
			Clinic clinic=((NewPatApptURL)gridWebSchedNewPatApptURLs.Rows[gridWebSchedNewPatApptURLs.GetSelectedIndex()].Tag).Clinic;
			if(clinic!=null) {
				menuItemExcludeLocation.Text=clinic.IsNewPatApptExcluded ? Lan.g(this,"Include Location") : Lan.g(this,"Exclude Location");
			}
		}

		private void menuItemExcludeLocation_Click(object sender,EventArgs e) {
			//Always default the menu item to exclude
			if(gridWebSchedNewPatApptURLs.GetSelectedIndex() < 0) {
				MsgBox.Show(this,"Select a location first.");
				return;
			}
			if(!Security.IsAuthorized(Permissions.Setup)) {
				return;
			}
			if(gridWebSchedNewPatApptURLs.GetSelectedIndex()==0) {
				MsgBox.Show(this,"Headquarters can be excluded by disabling the New Patient Appts feature.");
				return;
			}
			//Flip the bit on the clinic.
			Clinic clinic=((NewPatApptURL)gridWebSchedNewPatApptURLs.Rows[gridWebSchedNewPatApptURLs.GetSelectedIndex()].Tag).Clinic;
			clinic.IsNewPatApptExcluded=!clinic.IsNewPatApptExcluded;
			Clinics.Update(clinic);
			foreach(NewPatApptURL newPatApptURL in _listWebSchedNewPatApptHostedURLs) {
				if(newPatApptURL.ClinicNum!=clinic.ClinicNum) {
					continue;
				}
				newPatApptURL.Clinic.IsNewPatApptExcluded=clinic.IsNewPatApptExcluded;
			}
			_hasClinicsChanged=true;
			SecurityLogs.MakeLogEntry(Permissions.Setup,0,Lan.g(this,"Clinics - via eServices Setup window"));
			FillGridWebSchedNewPatApptHostedURLs();
			FillGridWebSchedNewPatApptTimeSlots();
		}

		private void menuItemCopyURL_Click(object sender,EventArgs e) {
			if(gridWebSchedNewPatApptURLs.GetSelectedIndex() < 0) {
				MsgBox.Show(this,"Select a URL to copy first.");
				return;
			}
			try {
				Clipboard.SetText(((NewPatApptURL)gridWebSchedNewPatApptURLs.Rows[gridWebSchedNewPatApptURLs.GetSelectedIndex()].Tag).URL);
			}
			catch(Exception) {
				MsgBox.Show(this,"Could not copy the URL to the clipboard.");
			}
		}

		private void menuItemNavigateToURL_Click(object sender,EventArgs e) {
			if(gridWebSchedNewPatApptURLs.GetSelectedIndex() < 0) {
				MsgBox.Show(this,"Select a URL to navigate to first.");
				return;
			}
			NavigateToURL(((NewPatApptURL)gridWebSchedNewPatApptURLs.Rows[gridWebSchedNewPatApptURLs.GetSelectedIndex()].Tag).URL);
		}

		private void NavigateToURL(string URL) {
			try {
				Process.Start(URL);
			}
			catch(Exception) {
				MsgBox.Show(this,"There was a problem launching the URL with a web browser.  Make sure a default browser has been set.");
			}
		}

		private void FillGridWebSchedNewPatApptOps() {
			_listWebSchedNewPatApptOps=Operatories.GetOpsForWebSchedNewPatAppts();
			int opNameWidth=200;
			int clinicWidth=80;
			if(!PrefC.HasClinicsEnabled) {
				opNameWidth+=clinicWidth;
			}
			gridWebSchedNewPatApptOps.BeginUpdate();
			gridWebSchedNewPatApptOps.Columns.Clear();
			gridWebSchedNewPatApptOps.Columns.Add(new ODGridColumn(Lan.g("FormEServicesSetup","Op Name"),opNameWidth));
			gridWebSchedNewPatApptOps.Columns.Add(new ODGridColumn(Lan.g("FormEServicesSetup","Abbrev"),90));
			if(PrefC.HasClinicsEnabled) {
				gridWebSchedNewPatApptOps.Columns.Add(new ODGridColumn(Lan.g("FormEServicesSetup","Clinic"),clinicWidth));
			}
			gridWebSchedNewPatApptOps.Columns.Add(new ODGridColumn(Lan.g("FormEServicesSetup","Provider"),90));
			gridWebSchedNewPatApptOps.Columns.Add(new ODGridColumn(Lan.g("FormEServicesSetup","Hygienist"),90));
			gridWebSchedNewPatApptOps.Rows.Clear();
			ODGridRow row;
			foreach(Operatory op in _listWebSchedNewPatApptOps) {
				row=new ODGridRow();
				row.Cells.Add(op.OpName);
				row.Cells.Add(op.Abbrev);
				if(PrefC.HasClinicsEnabled) {
					row.Cells.Add(Clinics.GetAbbr(op.ClinicNum));
				}
				row.Cells.Add(Providers.GetAbbr(op.ProvDentist));
				row.Cells.Add(Providers.GetAbbr(op.ProvHygienist));
				row.Tag=op;
				gridWebSchedNewPatApptOps.Rows.Add(row);
			}
			gridWebSchedNewPatApptOps.EndUpdate();
		}

		private void FillGridWebSchedNewPatApptTimeSlots() {
			//Validate time slot settings.
			if(textWebSchedNewPatApptsDateStart.errorProvider1.GetError(textWebSchedNewPatApptsDateStart)!="") {
				//Don't bother warning the user.  It will just be annoying.  The red indicator should be sufficient.
				return;
			}
			if(gridWebSchedNewPatApptURLs.GetSelectedIndex() < 0) {
				return;//Nothing to do.
			}
			_indexLastNewPatURL=gridWebSchedNewPatApptURLs.GetSelectedIndex();
			Cursor=Cursors.WaitCursor;
			DateTime dateStart=PIn.DateT(textWebSchedNewPatApptsDateStart.Text);
			DateTime dateEnd=dateStart.AddDays(30);
			List<TimeSlot> listTimeSlots=new List<TimeSlot>();
			NewPatApptURL newPatApptURL=(NewPatApptURL)gridWebSchedNewPatApptURLs.Rows[gridWebSchedNewPatApptURLs.GetSelectedIndex()].Tag;
			//Only get time slots for headquarters or clinics that are NOT excluded (aka included).
			if(newPatApptURL.Clinic==null || (newPatApptURL.Clinic!=null && !newPatApptURL.Clinic.IsNewPatApptExcluded)) {
				long clinicNum=((NewPatApptURL)gridWebSchedNewPatApptURLs.Rows[gridWebSchedNewPatApptURLs.GetSelectedIndex()].Tag).ClinicNum;
				try {
					listTimeSlots=TimeSlots.GetAvailableNewPatApptTimeSlots(dateStart,dateEnd,clinicNum);
				}
				catch(Exception) {
					//The user might not have Web Sched ops set up correctly.  Don't warn them here because it is just annoying.  They'll figure it out.
				}
			}
			Cursor=Cursors.Default;
			gridWebSchedNewPatApptTimeSlots.BeginUpdate();
			gridWebSchedNewPatApptTimeSlots.Columns.Clear();
			ODGridColumn col=new ODGridColumn("",0);
			col.TextAlign=HorizontalAlignment.Center;
			gridWebSchedNewPatApptTimeSlots.Columns.Add(col);
			gridWebSchedNewPatApptTimeSlots.Rows.Clear();
			ODGridRow row;
			DateTime dateTimeSlotLast=DateTime.MinValue;
			foreach(TimeSlot timeSlot in listTimeSlots) {
				//Make a new row for every unique day.
				if(dateTimeSlotLast.Date!=timeSlot.DateTimeStart.Date) {
					dateTimeSlotLast=timeSlot.DateTimeStart;
					row=new ODGridRow();
					row.ColorBackG=Color.LightBlue;
					row.Cells.Add(timeSlot.DateTimeStart.ToShortDateString());
					gridWebSchedNewPatApptTimeSlots.Rows.Add(row);
				}
				row=new ODGridRow();
				row.Cells.Add(timeSlot.DateTimeStart.ToShortTimeString()+" - "+timeSlot.DateTimeStop.ToShortTimeString());
				gridWebSchedNewPatApptTimeSlots.Rows.Add(row);
			}
			gridWebSchedNewPatApptTimeSlots.EndUpdate();
		}

		private void gridWebSchedNewPatApptOps_DoubleClick(object sender,EventArgs e) {
			ShowOperatoryEditAndRefreshGrids();
		}

		private void butWebSchedNewPatApptEnable_Click(object sender,EventArgs e) {
			labelWebSchedNewPatApptEnable.Text="";
			Application.DoEvents();
			//The enable button is not enabled for offices that already have the service enabled.  Therefore go straight to making the web call to our service.
			if(!ValidateWebSchedNewPatAppts()) {
				return;//Error message would have already shown.
			}
			//Everything went good, the office is actively on support and has an active WebSchNew repeating charge.
			butWebSchedNewPatApptEnable.Visible=false;
			butWebSchedNewPatApptSignUp.Visible=false;
			labelWebSchedEnable.Text=Lan.g(this,"The New Patient Appts Web Sched service has been enabled.");
			//This if statement will only save database calls in the off chance that this window was originally loaded with the pref turned off and got turned on by another computer while open.
			if(Prefs.UpdateBool(PrefName.WebSchedNewPatApptEnabled,true)) {
				_hasPrefsChanged=true;
				SecurityLogs.MakeLogEntry(Permissions.EServicesSetup,0,"The New Patient Appts Web Sched service was enabled.");
			}
		}

		///<summary>Returns true is this registration key is on support for New Pat Appts, false if they are not.
		///Makes a web call to servicehq in order to validate the registration key.
		///If user is not on support or there is an error, this method shows error messages and launches our promo site accordingly.</summary>
		private bool ValidateWebSchedNewPatAppts() {
			Cursor.Current=Cursors.WaitCursor;
			string error="";
			try {
				WebSerializer.DeserializePrimitiveListOrThrow<long>(WebServiceMainHQProxy.GetWebServiceMainHQInstance()
					.ValidateEService(PrefC.GetString(PrefName.RegistrationKey),eServiceCode.WebSchedNewPatAppt.ToString()));
			}
			catch(Exception) {
				//Prep a generic error response just in case something unexpected went wrong.
				error=Lan.g(this,"There was a problem enabling the New Patient Appts Web Sched service.  Please give us a call or try again.");
				//We want to launch our Web Sched promo page in case the user is not signed up:
				try {
					Process.Start(WEB_SCHED_NEW_PAT_APPT_SIGN_UP_URL);
				}
				catch(Exception) {
					//The promotional web site can't be shown, most likely due to the computer not having a default browser.  Simply do nothing.
				}
			}
			Cursor.Current=Cursors.Default;
			if(error!="") {
				//In case no browser was opened, set the message next to the button so that they can visually see that something happened.
				labelWebSchedNewPatApptEnable.Text=error;
				MessageBox.Show(error);
				return false;
			}
			return true;
		}

		private void butWebSchedNewPatApptSignUp_Click(object sender,EventArgs e) {
			try {
				Process.Start(WEB_SCHED_NEW_PAT_APPT_SIGN_UP_URL);
			}
			catch(Exception) {
				//The promotional web site can't be shown, most likely due to the computer not having a default browser.
				MessageBox.Show(Lan.g(this,"Sign up page could not load.  Please visit the following web site")+":\r\n"+WEB_SCHED_NEW_PAT_APPT_SIGN_UP_URL);
			}
		}

		private void butWebSchedNewPatApptsAdd_Click(object sender,EventArgs e) {
			FormProcCodes FormPC=new FormProcCodes();
			FormPC.IsSelectionMode=true;
			FormPC.ShowDialog();
			if(FormPC.DialogResult!=DialogResult.OK) {
				return;
			}
			string procCode=ProcedureCodes.GetStringProcCode(FormPC.SelectedCodeNum);
			string prefProcs=PrefC.GetString(PrefName.WebSchedNewPatApptProcs);
			if(!string.IsNullOrEmpty(prefProcs)) {
				prefProcs+=",";
			}
			prefProcs+=procCode;
			Prefs.UpdateString(PrefName.WebSchedNewPatApptProcs,prefProcs);
			FillGridWebSchedNewPatApptProcs();
			_hasPrefsChanged=true;//Causes preference cache to be invalidated when this window is closed.
		}

		private void butWebSchedNewPatApptsRemove_Click(object sender,EventArgs e) {
			int selectedIndex=gridWebSchedNewPatApptProcs.GetSelectedIndex();
			if(selectedIndex==-1) {
				MsgBox.Show(this,"Select a procedure to remove.");
				return;
			}
			string procCode=(string)gridWebSchedNewPatApptProcs.Rows[selectedIndex].Tag;
			List<string> listProcCodes=PrefC.GetString(PrefName.WebSchedNewPatApptProcs).Split(',').ToList();
			listProcCodes.Remove(procCode);
			Prefs.UpdateString(PrefName.WebSchedNewPatApptProcs,string.Join(",",listProcCodes));
			FillGridWebSchedNewPatApptProcs();
			_hasPrefsChanged=true;//Causes preference cache to be invalidated when this window is closed.
		}

		private void butWebSchedNewPatApptsToday_Click(object sender,EventArgs e) {
			textWebSchedNewPatApptsDateStart.Text=DateTime.Today.ToShortDateString();
		}

		private void textWebSchedNewPatApptSearchDays_Validated(object sender,EventArgs e) {
			if(textWebSchedNewPatApptSearchDays.errorProvider1.GetError(textWebSchedNewPatApptSearchDays)!="") {
				return;
			}
			int newPatApptDays=PIn.Int(textWebSchedNewPatApptSearchDays.Text);
			if(Prefs.UpdateInt(PrefName.WebSchedNewPatApptSearchAfterDays,newPatApptDays > 0 ? newPatApptDays : 0)) {
				_hasPrefsChanged=true;
				_indexLastNewPatURL=-1;//Force refresh of the grid in because this setting changed.
			}
		}

		private void textWebSchedNewPatApptLength_TextChanged(object sender,EventArgs e) {
			int selectionStart=textWebSchedNewPatApptLength.SelectionStart;
			char[] arrayChars=textWebSchedNewPatApptLength.Text.ToCharArray();
			string newPatApptLength=new string(Array.FindAll<char>(arrayChars,(x => (x=='x' || x=='X' || x=='/')))).ToUpper();
			textWebSchedNewPatApptLength.Text=newPatApptLength;
			//If no text was removed, put the cursor back to where it was
			if(arrayChars.Length==newPatApptLength.Length) {
				textWebSchedNewPatApptLength.Select(selectionStart,0);
			}
			else if(selectionStart > 0) {//The character typed in was removed and there is still text in the box.
				textWebSchedNewPatApptLength.Select(selectionStart-1,0);
			}
			if(Prefs.UpdateString(PrefName.WebSchedNewPatApptTimePattern,newPatApptLength)) {
				_hasPrefsChanged=true;
				_indexLastNewPatURL=-1;//Force refresh of the grid in because this setting changed.
			}
		}

		private void textWebSchedNewPatApptsDateStart_TextChanged(object sender,EventArgs e) {
			//Only refresh the grid if the user has typed in a valid date.
			if(textWebSchedNewPatApptsDateStart.errorProvider1.GetError(textWebSchedNewPatApptsDateStart)=="") {
				FillGridWebSchedNewPatApptTimeSlots();
			}
		}
		#endregion

		#endregion Web Sched

		#region Listener Service

		///<summary>Updates the text box that is displaying the current status of the Listener Service.  Returns the status just in case other logic is needed outside of updating the status box.</summary>
		private eServiceSignalSeverity FillTextListenerServiceStatus() {
			eServiceSignalSeverity eServiceStatus=EServiceSignals.GetListenerServiceStatus();
			if(eServiceStatus==eServiceSignalSeverity.Critical) {
				textListenerServiceStatus.BackColor=COLOR_ESERVICE_CRITICAL_BACKGROUND;
				textListenerServiceStatus.ForeColor=COLOR_ESERVICE_CRITICAL_TEXT;
				butStartListenerService.Enabled=true;
			}
			else if(eServiceStatus==eServiceSignalSeverity.Error) {
				textListenerServiceStatus.BackColor=COLOR_ESERVICE_ERROR_BACKGROUND;
				textListenerServiceStatus.ForeColor=COLOR_ESERVICE_ERROR_TEXT;
				butStartListenerService.Enabled=true;
			}
			else {
				textListenerServiceStatus.BackColor=SystemColors.Control;
				textListenerServiceStatus.ForeColor=SystemColors.WindowText;
				butStartListenerService.Enabled=false;
			}
			textListenerServiceStatus.Text=eServiceStatus.ToString();
			return eServiceStatus;
		}

		private void SetEConnectorCommunicationStatus() {
			textEConnectorListeningType.Text=_listenerType.ToString();
			labelListenerPort.Visible=false;//Invisible unless ListenerServiceType is ListenerService
			textListenerPort.Visible=false;
			switch(_listenerType) {
				case ListenerServiceType.ListenerService:
					checkAllowEConnectorComm.Checked=true;
					labelListenerPort.Visible=true;
					textListenerPort.Visible=true;
					break;
				case ListenerServiceType.ListenerServiceProxy:
					checkAllowEConnectorComm.Checked=true;
					break;
				case ListenerServiceType.NoListener:
					checkAllowEConnectorComm.Checked=false;
					break;
				case ListenerServiceType.DisabledByHQ:
				default:
					checkAllowEConnectorComm.Enabled=false;
					break;
			}
		}

		private void butInstallEConnector_Click(object sender,EventArgs e) {
			DialogResult result;
			//Check to see if the update server preference is set.
			//If set, make sure that this is set to the computer currently logged on.
			string updateServerName=PrefC.GetString(PrefName.WebServiceServerName);
			if(!string.IsNullOrEmpty(updateServerName) && !ODEnvironment.IdIsThisComputer(updateServerName.ToLower())) {
				result=MessageBox.Show(Lan.g(this,"The eConnector service should be installed on the Update Server")+": "+updateServerName+"\r\n"
					+Lan.g(this,"Are you trying to install the eConnector on a different computer by accident?"),"",MessageBoxButtons.YesNoCancel);
				//Only saying No to this message box pop up will allow the user to continue (meaning they fully understand what they are getting into).
				if(result!=DialogResult.No) {
					return;
				}
			}
			//Only ask the user if they want to set the Update Server Name preference if it is not already set.
			if(string.IsNullOrEmpty(updateServerName)) {
				result=MessageBox.Show(Lan.g(this,"The computer that has the eConnector service installed should be set as the Update Server.")+"\r\n"
					+Lan.g(this,"Would you like to make this computer the Update Server?"),"",MessageBoxButtons.YesNoCancel);
				if(result==DialogResult.Cancel) {
					return;
				}
				else if(result==DialogResult.Yes) {
					Prefs.UpdateString(PrefName.WebServiceServerName,Dns.GetHostName());
					_hasPrefsChanged=true;
				}
			}
			//If this is the first time installing the eConnector, ask them if they are willing to accept inbound comms from Open Dental.
			if(!PrefC.GetBool(PrefName.EConnectorEnabled)) {
				bool startListenerCommunications=false;
				string messageStartListener=Lan.g(this,"eServices will not work as expected untill inbound communication is allowed.")+"\r\n"
					+Lan.g(this,"Do you want to accept inbound communication?");
				if(_listenerType==ListenerServiceType.NoListener && MessageBox.Show(messageStartListener,"",MessageBoxButtons.YesNo)==DialogResult.Yes) {
					startListenerCommunications=true;
				}
				try {
					_listenerType=WebSerializer.DeserializePrimitiveOrThrow<ListenerServiceType>(
						OpenDentBusiness.WebServiceMainHQProxy.GetWebServiceMainHQInstance().SetEConnectorType(
							WebSerializer.SerializePrimitive<string>(OpenDentBusiness.PrefC.GetString(OpenDentBusiness.PrefName.RegistrationKey)),
							startListenerCommunications
						)
					);
					string logText=Lan.g(this,"eConnector status set to")+" "+_listenerType.ToString()+" "
						+Lan.g("PrefL","by manually installing the eConnector service.");
					SecurityLogs.MakeLogEntry(Permissions.EServicesSetup,0,logText);
				}
				catch(Exception) {
					MsgBox.Show(this,"Failure sending the eConnector communication status.  Please contact us to enable eServices.");
					//Do NOT install the eConnector until setting the eConnector type is successful.  
					//It will not work properly until this setting has be instatiated for the first time.
					return;
				}
			}
			//At this point the user wants to install the eConnector service (or upgrade the old cust listener to the eConnector).
			bool isListening;
			if(!ServicesHelper.UpgradeOrInstallEConnector(false,out isListening)) {
				//Warning messages would have already been shown to the user, simply return.
				return;
			}
			//The eConnector service was successfully installed and is running, set the EConnectorEnabled flag true if false.
			if(Prefs.UpdateBool(PrefName.EConnectorEnabled,true)) {
				_hasPrefsChanged=true;
			}
			SetEConnectorCommunicationStatus();
			MsgBox.Show(this,"eConnector successfully installed");
			butInstallEConnector.Enabled=false;
			FillTextListenerServiceStatus();
			FillGridListenerService();
		}

		private void FillGridListenerService() {
			//Display some historical information for the last 30 days in this grid about the lifespan of the listener heartbeats.
			List<EServiceSignal> listESignals=EServiceSignals.GetServiceHistory(eServiceCode.ListenerService,DateTime.Today.AddDays(-30),DateTime.Today);
			gridListenerServiceStatusHistory.BeginUpdate();
			gridListenerServiceStatusHistory.Columns.Clear();
			ODGridColumn col=new ODGridColumn(Lan.g(this,"DateTime"),120);
			gridListenerServiceStatusHistory.Columns.Add(col);
			col=new ODGridColumn(Lan.g(this,"Status"),90);
			gridListenerServiceStatusHistory.Columns.Add(col);
			col=new ODGridColumn(Lan.g(this,"Details"),0);
			gridListenerServiceStatusHistory.Columns.Add(col);
			gridListenerServiceStatusHistory.Rows.Clear();
			ODGridRow row;
			for(int i=0;i<listESignals.Count;i++) {
				row=new ODGridRow();
				row.Cells.Add(listESignals[i].SigDateTime.ToString());
				row.Cells.Add(listESignals[i].Severity.ToString());
				row.Cells.Add(listESignals[i].Description.ToString());
				//Color the row if it is an error that has not been processed.
				if(listESignals[i].Severity==eServiceSignalSeverity.Error && !listESignals[i].IsProcessed) {
					row.ColorBackG=COLOR_ESERVICE_ERROR_BACKGROUND;
				}
				gridListenerServiceStatusHistory.Rows.Add(row);
			}
			gridListenerServiceStatusHistory.EndUpdate();
		}

		private void butSaveListenerPort_Click(object sender,EventArgs e) {
			if(textListenerPort.errorProvider1.GetError(textListenerPort)!="") {
				MessageBox.Show(Lan.g(this,"Listener Port must be a number between 0-65535."));
				return;
			}
			if(Prefs.UpdateString(PrefName.CustListenerPort,textListenerPort.Text)) {
				_hasPrefsChanged=true;//Sends invalid signal upon closing the form.
			}
			ListenerServiceType listenerTypeOld=_listenerType;
			try {
				_listenerType=WebSerializer.DeserializePrimitiveOrThrow<ListenerServiceType>(
					_webServiceMain.SetEConnectorType(WebSerializer.SerializePrimitive<string>(PrefC.GetString(PrefName.RegistrationKey))
						,checkAllowEConnectorComm.Checked)
				);
				if(_listenerType!=listenerTypeOld) {
					string logText=Lan.g(this,"eConnector status manually changed from")+" "+listenerTypeOld.ToString()+" "
						+Lan.g("PrefL","to")+" "+_listenerType.ToString();
					SecurityLogs.MakeLogEntry(Permissions.EServicesSetup,0,logText);
				}
				SetEConnectorCommunicationStatus();
			}
			catch(Exception) {
				MsgBox.Show(this,"Could not update the eConnector communication status.  Please contact us to enable eServices.");
				return;
			}
			MsgBox.Show(this,"eConnector settings saved.");
		}

		private void butStartListenerService_Click(object sender,EventArgs e) {
			//No setup permission check here so that anyone can hopefully get the service back up and running.
			//Check to see if the service started up on its own while we were in this window.
			if(FillTextListenerServiceStatus()==eServiceSignalSeverity.Working) {
				//Use a slightly different message than below so that we can easily tell which part of this method customers reached.
				MsgBox.Show(this,"Listener Service already started.  Please call us for support if eServices are still not working.");
				return;
			}
			//Check to see if the listener service is installed on this computer.
			List<ServiceController> listOdServices=ODEnvironment.GetAllOpenDentServices();
			List<ServiceController> listListenerServices=new List<ServiceController>();
			//Look for the service that uses "OpenDentalCustListener.exe"
			for(int i=0;i<listOdServices.Count;i++) {
				RegistryKey hklm=Registry.LocalMachine;
				hklm=hklm.OpenSubKey(@"System\CurrentControlSet\Services\"+listOdServices[i].ServiceName);
				string test=hklm.GetValue("ImagePath").ToString();
				string test1=test.Replace("\"","");
				string[] arrayExePath=hklm.GetValue("ImagePath").ToString().Replace("\"","").Split('\\');
				//This will not work if in the future we allow command line args for the listener service that include paths.
				if(arrayExePath[arrayExePath.Length-1].StartsWith("OpenDentalEConnector.exe")) {
					listListenerServices.Add(listOdServices[i]);
				}
			}
			if(listListenerServices.Count==0) {
				MsgBox.Show(this,"Listener Services were not found on this computer.  The service can only be started from the computer that is hosting Listener Services.");
				return;
			}
			Cursor=Cursors.WaitCursor;
			List<ServiceController> listListenerServicesErrors=new List<ServiceController>();
			for(int i=0;i<listListenerServices.Count;i++) {
				//The listener service is installed on this computer.  Try to start it if it is in a stopped or stop pending status.
				//If we do not do this, an InvalidOperationException will throw that says "An instance of the service is already running"
				if(listListenerServices[i].Status==ServiceControllerStatus.Stopped || listListenerServices[i].Status==ServiceControllerStatus.StopPending) {
					try {
						listListenerServices[i].Start();
						listListenerServices[i].WaitForStatus(ServiceControllerStatus.Running,new TimeSpan(0,0,7));
					}
					catch {
						//An InvalidOperationException can get thrown if the service could not be started.  E.g. current user is not running Open Dental as an administrator.
						listListenerServicesErrors.Add(listListenerServices[i]);
					}
				}
			}
			Cursor=Cursors.Default;
			if(listListenerServicesErrors.Count!=0) {
				string error=Lan.g(this,"There was a problem starting Listener Services.  Please go manually start the following Listener Services")+":";
				for(int i=0;i<listListenerServicesErrors.Count;i++) {
					error+="\r\n"+listListenerServicesErrors[i].DisplayName;
				}
				MessageBox.Show(error);
			}
			else {
				MsgBox.Show(this,"Listener Services Started.");
			}
			FillTextListenerServiceStatus();
			FillGridListenerService();
		}

		private void butListenerServiceHistoryRefresh_Click(object sender,EventArgs e) {
			FillTextListenerServiceStatus();
			FillGridListenerService();
		}

		private void butListenerServiceAck_Click(object sender,EventArgs e) {
			EServiceSignals.ProcessSignalsForSeverity(eServiceSignalSeverity.Error);
			FillTextListenerServiceStatus();
			FillGridListenerService();
			MsgBox.Show(this,"Errors successfully acknowledged.");
		}

		private void butListenerAlertsOff_Click(object sender,EventArgs e) {
			if(!Security.IsAuthorized(Permissions.SecurityAdmin)) {
				return;
			}
			//Insert a row into the eservicesignal table to indicate to all computers to stop monitoring.
			EServiceSignal signalDisable=new EServiceSignal();
			signalDisable.Description="Stop Monitoring clicked from setup window.";
			signalDisable.IsProcessed=true;
			signalDisable.ReasonCategory=0;
			signalDisable.ReasonCode=0;
			signalDisable.ServiceCode=(int)eServiceCode.ListenerService;
			signalDisable.Severity=eServiceSignalSeverity.NotEnabled;
			signalDisable.Tag="";
			signalDisable.SigDateTime=DateTime.Now;
			EServiceSignals.Insert(signalDisable);
			SecurityLogs.MakeLogEntry(Permissions.SecurityAdmin,0,"Listener Service monitoring manually stopped via eServices Setup window.");
			MsgBox.Show(this,"Monitoring shutdown signal sent.  This will take up to one minute.");
			FillGridListenerService();
			FillTextListenerServiceStatus();
		}

		#endregion

		#region Sms Services

		///<summary>Called on form load and when typing into Monthly limit box.</summary>
		private void SetSmsServiceAgreement(bool isTyping=false) {
			double smsMonthlyLimit;
			if(PrefC.GetBool(PrefName.EasyNoClinics)) {
				labelClinic.Text=Lans.g(this,"Practice Title");
				smsMonthlyLimit=PrefC.GetDouble(PrefName.SmsMonthlyLimit);
			}
			else if(_smsListClinics.Count==0) {//clinics enabled, but no clinics are present;
				labelClinic.Text=Lans.g(this,"Clinic");
				textSmsLimit.Enabled=false;
				checkSmsAgree.Enabled=false;
				butSmsSubmit.Enabled=false;
				butSmsCancel.Enabled=false;
				butSmsUnsubscribe.Enabled=false;
				comboClinicSms.Enabled=false;
				smsMonthlyLimit=0;
			}
			else {
				labelClinic.Text=Lans.g(this,"Clinic");
				smsMonthlyLimit=_smsListClinics[comboClinicSms.SelectedIndex].SmsMonthlyLimit;
			}
			//fill text
			if(String.IsNullOrEmpty(textSmsLimit.Text) && !isTyping) {//blank text box, fill with stored value
				textSmsLimit.Text=smsMonthlyLimit.ToString("c",new CultureInfo("en-US"));
			}
			//parse text, which will usually be displayed in USD, unless the user is typing
			double smsMonthlyLimitText=PIn.Double(textSmsLimit.Text.Trim('$'));
			//If they have a non-zero contract amount they should always be able to click unsubscribe.
			if(smsMonthlyLimit>0) {
				butSmsUnsubscribe.Enabled=true;
				butSmsSubmit.Text=Lans.g(this,"Update");
			}
			else {
				butSmsUnsubscribe.Enabled=false;
				butSmsSubmit.Text=Lans.g(this,"Submit");
			}
			//If they have entered something that does not match what is stored in DB enable cancel button.
			if(smsMonthlyLimitText==smsMonthlyLimit) {
				butSmsCancel.Enabled=false;
			}
			else {
				butSmsCancel.Enabled=true;
			}
			//They have typed in blank or 0
			if(smsMonthlyLimitText==0) {
				checkSmsAgree.Enabled=false;
				checkSmsAgree.Checked=false;
			}
			//Either they typed in the same amount or nothing has been edited
			else if(smsMonthlyLimitText==smsMonthlyLimit) {
				checkSmsAgree.Enabled=false;
				checkSmsAgree.Checked=true;
			}
			//They have typed something in that is not zero and does not match their currently set limit.
			else {
				checkSmsAgree.Enabled=true;
				checkSmsAgree.Checked=false;
			}
			butSmsSubmit.Enabled=checkSmsAgree.Checked&checkSmsAgree.Enabled;
		}

		private void checkSmsAgree_CheckedChanged(object sender,EventArgs e) {
			double monthlyLimit=0;
			if(PrefC.GetBool(PrefName.EasyNoClinics)) {
				monthlyLimit=PrefC.GetDouble(PrefName.SmsMonthlyLimit);
			}
			else {
				monthlyLimit=_smsClinicCur.SmsMonthlyLimit;
			}
			if(checkSmsAgree.Checked //they have checked agree
				&& PIn.Double(textSmsLimit.Text.Trim('$'))>0 //there is a valid dollar amount
				&& PIn.Double(textSmsLimit.Text.Trim('$'))!=monthlyLimit)//and the dollar amount isn't the already signed contract amount. 
			{
				butSmsSubmit.Enabled=true;
			}
			else {
				butSmsSubmit.Enabled=false;
			}
		}

		private void comboClinicSms_SelectedIndexChanged(object sender,EventArgs e) {
			if(_smsListClinics==null || _smsListClinics.Count==0) {
				_smsClinicCur=null;
				return;
			}
			_smsClinicCur=_smsListClinics[comboClinicSms.SelectedIndex];
			textSmsLimit.Text="";
			SetSmsServiceAgreement();
		}

		private void FillGridClinics() {
			Clinics.RefreshCache();
			_smsListClinics=Clinics.GetForUserod(Security.CurUser);//refresh potentially changed data.
			gridClinics.BeginUpdate();
			gridClinics.Columns.Clear();
			if(PrefC.HasClinicsEnabled) {
				gridClinics.Columns.Add(new ODGridColumn(Lan.g(this,"Def"),30) { TextAlign=HorizontalAlignment.Center });
			}
			gridClinics.Columns.Add(new ODGridColumn(Lan.g(this,"Location"),150));
			gridClinics.Columns.Add(new ODGridColumn(Lan.g(this,"Subscribed"),80));
			gridClinics.Columns.Add(new ODGridColumn(Lan.g(this,"Limit"),80));
			gridClinics.Rows.Clear();
			ODGridRow row;
			if(!PrefC.HasClinicsEnabled) {
				row=new ODGridRow();
				row.Cells.Add(PrefC.GetString(PrefName.PracticeTitle));
				row.Cells.Add(PrefC.GetDate(PrefName.SmsContractDate).Year>1800?Lan.g(this,"Yes"):Lan.g(this,"No"));
				row.Cells.Add((PrefC.GetDouble(PrefName.SmsMonthlyLimit)).ToString("c",new CultureInfo("en-US")));//Charge this month (Must always be in USD)
				gridClinics.Rows.Add(row);
			}
			else {
				long defClinic = PrefC.GetLong(PrefName.TextingDefaultClinicNum);
				for(int i=0;i<_smsListClinics.Count;i++) {
					row=new ODGridRow();
					row.Cells.Add(_smsListClinics[i].ClinicNum==defClinic ? "X" : "");
					row.Cells.Add(_smsListClinics[i].Abbr);
					row.Cells.Add(_smsListClinics[i].SmsContractDate.Year>1800?Lan.g(this,"Yes"):Lan.g(this,"No"));
					row.Cells.Add(_smsListClinics[i].SmsMonthlyLimit.ToString("c",new CultureInfo("en-US")));//Charge this month (Must always be in USD)
					gridClinics.Rows.Add(row);
				}
			}
			gridClinics.EndUpdate();
		}

		private void butDefaultClinic_Click(object sender,EventArgs e) {
			if(gridClinics.GetSelectedIndex()<0) {
				MsgBox.Show(this,"Select clinic to make default.");
				return;
			}
			//TODO: permissions check?
			Prefs.UpdateLong(PrefName.TextingDefaultClinicNum,_smsListClinics[gridClinics.GetSelectedIndex()].ClinicNum);
			Signalods.SetInvalid(InvalidType.Prefs);
			FillGridClinics();
		}

		private void butDefaultClinicClear_Click(object sender,EventArgs e) {
			//TODO: permissions check?
			Prefs.UpdateLong(PrefName.TextingDefaultClinicNum,0);
			Signalods.SetInvalid(InvalidType.Prefs);
			FillGridClinics();
		}

		private void FillGridSmsUsage() {
			if(PrefC.GetBool(PrefName.EasyNoClinics)) {
				_listPhones=SmsPhones.GetForPractice();
			}
			else {
				_listPhones=SmsPhones.GetForClinics(_smsListClinics.Select(x=>x.ClinicNum).ToList()); //new List<Clinic> { _clinicCur });
			}
			gridSmsSummary.BeginUpdate();
			gridSmsSummary.Columns.Clear();
			ODGridColumn col=new ODGridColumn(Lan.g(this,"Location"),120,HorizontalAlignment.Right);
			gridSmsSummary.Columns.Add(col);
			col=new ODGridColumn(Lan.g(this,"Primary\r\nPhone Number"),105,HorizontalAlignment.Right);
			gridSmsSummary.Columns.Add(col);
			col=new ODGridColumn(Lan.g(this,"Country\r\nCode"),60,HorizontalAlignment.Right);
			gridSmsSummary.Columns.Add(col);
			//col=new ODGridColumn(Lan.g(this,"Sent\r\nAll Time"),70,HorizontalAlignment.Right);
			//gridSmsSummary.Columns.Add(col);
			col=new ODGridColumn(Lan.g(this,"Sent\r\nFor Month"),70,HorizontalAlignment.Right);
			gridSmsSummary.Columns.Add(col);
			col=new ODGridColumn(Lan.g(this,"Sent\r\nCharges"),70,HorizontalAlignment.Right);
			gridSmsSummary.Columns.Add(col);
			//col=new ODGridColumn(Lan.g(this,"Received\r\nAll Time"),70,HorizontalAlignment.Right);
			//gridSmsSummary.Columns.Add(col);
			col=new ODGridColumn(Lan.g(this,"Received\r\nFor Month"),70,HorizontalAlignment.Right);
			gridSmsSummary.Columns.Add(col);
			col=new ODGridColumn(Lan.g(this,"Received\r\nCharges"),70,HorizontalAlignment.Right);
			gridSmsSummary.Columns.Add(col);
			gridSmsSummary.Rows.Clear();
			DataTable usage=SmsPhones.GetSmsUsageLocal(PrefC.GetBool(PrefName.EasyNoClinics)?new List<long>{0}:_smsListClinics.Select(x => x.ClinicNum).ToList(),dateTimePickerSms.Value);
			if(usage==null || usage.Rows.Count==0) {
				gridSmsSummary.EndUpdate();
				return;
			}
			//Only add 1 row if not using clinics. Otherwise add 1 row per clinic.
			for(int i=0;(!PrefC.GetBool(PrefName.EasyNoClinics) && i<_smsListClinics.Count) || (PrefC.GetBool(PrefName.EasyNoClinics) && i==0);i++) {// Or i==0 allows us to use the same code for practice and clinics
				ODGridRow row=new ODGridRow();
				if(PrefC.GetBool(PrefName.EasyNoClinics)) {					
					row.Cells.Add(PrefC.GetString(PrefName.PracticeTitle));
				}
				else {
					row.Cells.Add(_smsListClinics[i].Description);
				}
				bool hasRow=false;
				foreach(DataRow dataRow in usage.Rows) {
					if(PrefC.GetBool(PrefName.EasyNoClinics)) {
						//do nothing, we want to run through this code for practice level usage
					}
					else if(PIn.Long(dataRow["ClinicNum"].ToString())!=_smsListClinics[i].ClinicNum) {
						continue;
					}
					hasRow=true;
					row.Cells.Add(dataRow["PhoneNumber"].ToString());
					row.Cells.Add(dataRow["CountryCode"].ToString());
					//row.Cells.Add(dataRow["SentAllTime"].ToString());
					row.Cells.Add(dataRow["SentMonth"].ToString());
					row.Cells.Add(PIn.Double(dataRow["SentCharge"].ToString()).ToString("c",new CultureInfo("en-US")));
					//row.Cells.Add(dataRow["ReceivedAllTime"].ToString());
					row.Cells.Add(dataRow["ReceivedMonth"].ToString());
					row.Cells.Add(PIn.Double(dataRow["ReceivedCharge"].ToString()).ToString("c",new CultureInfo("en-US")));
				}
				if(!hasRow) {
					row.Cells.Add("");//phone number
					row.Cells.Add("");//country code
					//row.Cells.Add("0");//Sent All Time
					row.Cells.Add("0");//Sent Month
					row.Cells.Add((0f).ToString("c",new CultureInfo("en-US")));//Sent Charge
					//row.Cells.Add("0");//Rcvd All Time
					row.Cells.Add("0");//Rcvd Month
					row.Cells.Add((0f).ToString("c",new CultureInfo("en-US")));//Rcvd Charge
				}
				gridSmsSummary.Rows.Add(row);
			}
			if(!PrefC.GetBool(PrefName.EasyNoClinics) && _smsListClinics.Count>1) {//Total row if there is more than one clinic (Will not display for practice because practice will have no clinics.
				ODGridRow row=new ODGridRow();
				//long totalSent=0;
				long totalSentMonth=0;
				double totalSentCharge=0f;
				//long totalReceived=0;
				long totalReceivedMonth=0;
				double totalReceivedCharge=0f;
				foreach(DataRow dataRow in usage.Rows) {
					//totalSent+=PIn.Long(dataRow["SentAllTime"].ToString());
					totalSentMonth+=PIn.Long(dataRow["SentMonth"].ToString());
					totalSentCharge+=PIn.Double(dataRow["SentCharge"].ToString());
					//totalReceived+=PIn.Long(dataRow["ReceivedAllTime"].ToString());
					totalReceivedMonth+=PIn.Long(dataRow["ReceivedMonth"].ToString());
					totalReceivedCharge+=PIn.Double(dataRow["ReceivedCharge"].ToString());
				}
				row.Cells.Add("");
				row.Cells.Add("");
				row.Cells.Add(Lans.g(this,"Total"));
				//row.Cells.Add(totalSent.ToString());
				row.Cells.Add(totalSentMonth.ToString());
				row.Cells.Add(totalSentCharge.ToString("c",new CultureInfo("en-US")));
				//row.Cells.Add(totalReceived.ToString());
				row.Cells.Add(totalReceivedMonth.ToString());
				row.Cells.Add(totalReceivedCharge.ToString("c",new CultureInfo("en-US")));
				row.ColorBackG=Color.LightYellow;
				gridSmsSummary.Rows.Add(row);
			}
			gridSmsSummary.EndUpdate();
		}

		/////<summary>Not used. Delete after porting required code form it.</summary>
		//private void FillGridSmsUsageTwo() {
		//	if(PrefC.GetBool(PrefName.EasyNoClinics)) {
		//		_listPhones=SmsPhones.GetForPractice();
		//	}
		//	else {
		//		_listPhones=SmsPhones.GetForClinics(_listClinics);
		//	}
		//	gridSmsSummary.BeginUpdate();
		//	gridSmsSummary.Columns.Clear();
		//	ODGridColumn col=new ODGridColumn(Lan.g(this,"Location"),130);
		//	gridSmsSummary.Columns.Add(col);
		//	col=new ODGridColumn(Lan.g(this,"Subscribed"),50);
		//	gridSmsSummary.Columns.Add(col);
		//	col=new ODGridColumn(Lan.g(this,"Limit"),75,HorizontalAlignment.Right);
		//	gridSmsSummary.Columns.Add(col);
		//	col=new ODGridColumn(Lan.g(this,"Virtual Phone #"),130,HorizontalAlignment.Right);
		//	gridSmsSummary.Columns.Add(col);
		//	col=new ODGridColumn(Lan.g(this,"Sent All Time"),60,HorizontalAlignment.Right);
		//	gridSmsSummary.Columns.Add(col);
		//	col=new ODGridColumn(Lan.g(this,"Sent Last Mo"),60,HorizontalAlignment.Right);
		//	gridSmsSummary.Columns.Add(col);
		//	col=new ODGridColumn(Lan.g(this,"Sent This Mo"),60,HorizontalAlignment.Right);
		//	gridSmsSummary.Columns.Add(col);
		//	col=new ODGridColumn(Lan.g(this,"Charge This Mo"),60,HorizontalAlignment.Right);
		//	gridSmsSummary.Columns.Add(col);
		//	col=new ODGridColumn(Lan.g(this,"Rcvd All Time"),60,HorizontalAlignment.Right);
		//	gridSmsSummary.Columns.Add(col);
		//	col=new ODGridColumn(Lan.g(this,"Rcvd Last Mo"),60,HorizontalAlignment.Right);
		//	gridSmsSummary.Columns.Add(col);
		//	col=new ODGridColumn(Lan.g(this,"Rcvd This Mo"),60,HorizontalAlignment.Right);
		//	gridSmsSummary.Columns.Add(col);
		//	col=new ODGridColumn(Lan.g(this,"Charge This Mo"),60,HorizontalAlignment.Right);
		//	gridSmsSummary.Columns.Add(col);
		//	//ODGridColumn col=new ODGridColumn(Lan.g(this,"Virtual Phone #"),130,HorizontalAlignment.Right);
		//	//gridSmsSummary.Columns.Add(col);
		//	//col=new ODGridColumn(Lan.g(this,"All Time"),60,HorizontalAlignment.Right);
		//	//gridSmsSummary.Columns.Add(col);
		//	//col=new ODGridColumn(Lan.g(this,"Sent"),60,HorizontalAlignment.Right);
		//	//gridSmsSummary.Columns.Add(col);
		//	//col=new ODGridColumn(Lan.g(this,"This Month"),60,HorizontalAlignment.Right);
		//	//gridSmsSummary.Columns.Add(col);
		//	//col=new ODGridColumn(Lan.g(this,"This Month"),60,HorizontalAlignment.Right);
		//	//gridSmsSummary.Columns.Add(col);
		//	//col=new ODGridColumn(Lan.g(this,"Received"),60,HorizontalAlignment.Right);
		//	//gridSmsSummary.Columns.Add(col);
		//	//col=new ODGridColumn(Lan.g(this,"Last Month"),60,HorizontalAlignment.Right);
		//	//gridSmsSummary.Columns.Add(col);
		//	//col=new ODGridColumn(Lan.g(this,"Received This Month"),60,HorizontalAlignment.Right);
		//	//gridSmsSummary.Columns.Add(col);
		//	//col=new ODGridColumn(Lan.g(this,"Charge This Month"),60,HorizontalAlignment.Right);
		//	//gridSmsSummary.Columns.Add(col);
		//	gridSmsSummary.Rows.Clear();
		//	ODGridRow row;
		//	Dictionary<string,Dictionary<string,double>> usage=SmsPhones.GetSmsUsageLocal(_listPhones,dateTimePickerSms.Value);
		//	if(PrefC.GetBool(PrefName.EasyNoClinics)) {//Practice level information
		//		row=new ODGridRow();
		//		row.Cells.Add(PrefC.GetString(PrefName.PracticeTitle));
		//		row.Cells.Add(PrefC.GetDate(PrefName.SmsContractDate).Year>1800?"Yes":"No");
		//		row.Cells.Add((PrefC.GetDouble(PrefName.SmsMonthlyLimit)).ToString("c",new CultureInfo("en-US")));//Charge this month (Must always be in USD)
		//		bool firstRow=true;
		//		if(_listPhones.Count==0) {
		//			_listPhones.Add(new SmsPhone() { PhoneNumber="" });//dummy row
		//		}
		//		foreach(SmsPhone phone in _listPhones) {
		//			if(phone.ClinicNum!=0) {
		//				continue;
		//			}
		//			if(!firstRow) {
		//				row=new ODGridRow();
		//				row.Cells.Add("");
		//				row.Cells.Add("");
		//				row.Cells.Add("");
		//			}
		//			row.Cells.Add(phone.PhoneNumber);
		//			if(usage.ContainsKey(phone.PhoneNumber)) {
		//				row.Cells.Add(usage[phone.PhoneNumber]["SentAllTime"].ToString());//Sent All Time
		//				row.Cells.Add(usage[phone.PhoneNumber]["SentLastMonth"].ToString());//Sent Last Month
		//				row.Cells.Add(usage[phone.PhoneNumber]["SentThisMonth"].ToString());//Sent This Month
		//				row.Cells.Add(usage[phone.PhoneNumber]["SentThisMonthCost"].ToString("c",new CultureInfo("en-US")));//Charge this month
		//				row.Cells.Add(usage[phone.PhoneNumber]["InboundAllTime"].ToString());//Rcvd All Time
		//				row.Cells.Add(usage[phone.PhoneNumber]["InboundLastMonth"].ToString());//Rcvd Last Month
		//				row.Cells.Add(usage[phone.PhoneNumber]["InboundThisMonth"].ToString());//Rcvd This Month
		//				row.Cells.Add(usage[phone.PhoneNumber]["InboundThisMonthCost"].ToString("c",new CultureInfo("en-US")));//Charge This Month
		//			}
		//			else {
		//				row.Cells.Add("0");//Sent All Time
		//				row.Cells.Add("0");//Sent Last Month
		//				row.Cells.Add("0");//Sent This Month
		//				row.Cells.Add((0f).ToString("c",new CultureInfo("en-US")));//Charge this month
		//				row.Cells.Add("0");//Rcvd All Time
		//				row.Cells.Add("0");//Rcvd Last Month
		//				row.Cells.Add("0");//Rcvd This Month
		//				row.Cells.Add((0f).ToString("c",new CultureInfo("en-US")));//Charge this month
		//			}
		//			firstRow=false;
		//			gridSmsSummary.Rows.Add(row);
		//		}
		//		row=new ODGridRow();
		//		row.Cells.Add("");
		//		row.Cells.Add("");
		//		row.Cells.Add("");
		//		row.Cells.Add("TOTALS");
		//		row.Cells.Add("0");//Sent All Time
		//		row.Cells.Add("0");//Sent Last Month
		//		row.Cells.Add("0");//Sent This Month
		//		row.Cells.Add((0f).ToString("c",new CultureInfo("en-US")));//Charge this month
		//		row.Cells.Add("0");//Rcvd All Time
		//		row.Cells.Add("0");//Rcvd Last Month
		//		row.Cells.Add("0");//Rcvd This Month
		//		row.Cells.Add((0f).ToString("c",new CultureInfo("en-US")));//Charge this month
		//		gridSmsSummary.Rows.Add(row);
		//	}
		//	else {//Using Clinics
		//		for(int i=0;i<_listClinics.Count;i++) {
		//			row=new ODGridRow();
		//			row.Cells.Add(_listClinics[i].Description);
		//			row.Cells.Add(_listClinics[i].SmsContractDate.Year>1800?"Yes":"No");
		//			row.Cells.Add(_listClinics[i].SmsMonthlyLimit.ToString("c",new CultureInfo("en-US")));//Charge this month (Must always be in USD)
		//			bool firstRow=true;
		//			if(_listPhones.Count==0 || !_listPhones.Exists(p => p.ClinicNum==_listClinics[i].ClinicNum)) {
		//				_listPhones.Add(new SmsPhone() { PhoneNumber="",ClinicNum=_listClinics[i].ClinicNum });//dummy row
		//			}
		//			foreach(SmsPhone phone in _listPhones) {
		//				if(phone.ClinicNum!=_listClinics[i].ClinicNum) {
		//					continue;
		//				}
		//				if(!firstRow) {
		//					row=new ODGridRow();
		//					row.Cells.Add("");
		//					row.Cells.Add("");
		//					row.Cells.Add("");
		//				}
		//				row.Cells.Add(phone.PhoneNumber);
		//				if(usage.ContainsKey(phone.PhoneNumber)) {
		//					row.Cells.Add(usage[phone.PhoneNumber]["SentAllTime"].ToString());//Sent All Time
		//					row.Cells.Add(usage[phone.PhoneNumber]["SentLastMonth"].ToString());//Sent Last Month
		//					row.Cells.Add(usage[phone.PhoneNumber]["SentThisMonth"].ToString());//Sent This Month
		//					row.Cells.Add(usage[phone.PhoneNumber]["SentThisMonthCost"].ToString("c",new CultureInfo("en-US")));//Charge this month
		//					row.Cells.Add(usage[phone.PhoneNumber]["InboundAllTime"].ToString());//Rcvd All Time
		//					row.Cells.Add(usage[phone.PhoneNumber]["InboundLastMonth"].ToString());//Rcvd Last Month
		//					row.Cells.Add(usage[phone.PhoneNumber]["InboundThisMonth"].ToString());//Rcvd This Month
		//					row.Cells.Add(usage[phone.PhoneNumber]["InboundThisMonthCost"].ToString("c",new CultureInfo("en-US")));//Charge This Month
		//				}
		//				else {
		//					row.Cells.Add("0");//Sent All Time
		//					row.Cells.Add("0");//Sent Last Month
		//					row.Cells.Add("0");//Sent This Month
		//					row.Cells.Add((0f).ToString("c",new CultureInfo("en-US")));//Charge this month
		//					row.Cells.Add("0");//Rcvd All Time
		//					row.Cells.Add("0");//Rcvd Last Month
		//					row.Cells.Add("0");//Rcvd This Month
		//					row.Cells.Add((0f).ToString("c",new CultureInfo("en-US")));//Charge this month
		//				}
		//				firstRow=false;
		//				gridSmsSummary.Rows.Add(row);
		//			}
		//			row=new ODGridRow();
		//			row.ColorBackG=Color.LightGray;
		//			row.Cells.Add("");
		//			row.Cells.Add("");
		//			row.Cells.Add("");
		//			row.Cells.Add("Totals Per Clinic");
		//			row.Cells.Add("0");//Sent All Time
		//			row.Cells.Add("0");//Sent Last Month
		//			row.Cells.Add("0");//Sent This Month
		//			row.Cells.Add((0f).ToString("c",new CultureInfo("en-US")));//Charge this month
		//			row.Cells.Add("0");//Rcvd All Time
		//			row.Cells.Add("0");//Rcvd Last Month
		//			row.Cells.Add("0");//Rcvd This Month
		//			row.Cells.Add((0f).ToString("c",new CultureInfo("en-US")));//Charge this month
		//			gridSmsSummary.Rows.Add(row);
		//		}
		//		row=new ODGridRow();
		//		row.ColorBackG=Color.Gray;
		//		row.Cells.Add("");
		//		row.Cells.Add("");
		//		row.Cells.Add("");
		//		row.Cells.Add("Totals For All");
		//		row.Cells.Add("0");//Sent All Time
		//		row.Cells.Add("0");//Sent Last Month
		//		row.Cells.Add("0");//Sent This Month
		//		row.Cells.Add((0f).ToString("c",new CultureInfo("en-US")));//Charge this month
		//		row.Cells.Add("0");//Rcvd All Time
		//		row.Cells.Add("0");//Rcvd Last Month
		//		row.Cells.Add("0");//Rcvd This Month
		//		row.Cells.Add((0f).ToString("c",new CultureInfo("en-US")));//Charge this month
		//		gridSmsSummary.Rows.Add(row);
		//	}
		//	gridSmsSummary.EndUpdate();
		//}


		private void textSmsLimit_TextChanged(object sender,EventArgs e) {
			SetSmsServiceAgreement(true);
		}

		private void textSmsLimit_Leave(object sender,EventArgs e) {
			//Attempt to clean up the input.
			textSmsLimit.Text=PIn.Double(textSmsLimit.Text.Trim('$')).ToString("c",new CultureInfo("en-US"));
			SetSmsServiceAgreement();
		}

		private void butSmsSubmit_Click(object sender,EventArgs e) {
			if(!checkSmsAgree.Checked) {
				MsgBox.Show(this,"You must agree to the service agreement.");
			}
			double amount=PIn.Double(textSmsLimit.Text.Trim('$'));
			if(amount<=0) {
				MsgBox.Show(this,"Please enter valid amount.");
				return;
			}
			if(Programs.IsEnabled(ProgramName.CallFire)) {
				if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Call Fire is currently enabled for texting.\r\nPress OK to switch to Integrated Texting, press Cancel to retain Call Fire")) {
					return;
				}
				Program callfire=Programs.GetCur(ProgramName.CallFire);
				if(callfire!=null) {
					callfire.Enabled=false;
					Programs.Update(callfire);
				}
			}
			List<SmsPhone> listClinicPhones=new List<SmsPhone>();
			try {
				if(PrefC.GetBool(PrefName.EasyNoClinics)) {
					listClinicPhones=SmsPhones.SignContract(0,amount);
				}
				else {
					listClinicPhones=SmsPhones.SignContract(_smsClinicCur.ClinicNum,amount);
				}
			}
			catch (Exception ex){
				MessageBox.Show(ex.Message);
				return;
			}
			if(listClinicPhones==null || listClinicPhones.Count==0) {
				MsgBox.Show(this,"Unable to initialize account.");
				return;
			}
			if(PrefC.GetBool(PrefName.EasyNoClinics)) {
				Prefs.UpdateDateT(PrefName.SmsContractDate,DateTime.Now);
				Prefs.UpdateDouble(PrefName.SmsMonthlyLimit,amount);
				DataValid.SetInvalid(InvalidType.Prefs);
			}
			else {
				_smsClinicCur.SmsMonthlyLimit=amount;
				_smsClinicCur.SmsContractDate=DateTime.Now;
				Clinics.Update(_smsClinicCur);
				DataValid.SetInvalid(InvalidType.Providers);//includes clinics.
			}
			FillGridClinics();
			FillGridSmsUsage();
			textSmsLimit.Text="";//set blank so that when we call SetSmsServiceAgreement it will populate with new value.
			SetSmsServiceAgreement();
			DataValid.SetInvalid(InvalidType.SmsTextMsgReceivedUnreadCount);
		}

		private void butSmsCancel_Click(object sender,EventArgs e) {
			textSmsLimit.Text="";//clear user input
			SetSmsServiceAgreement();//sets to previous value if applicable.
		}

		private void gridClinics_CellClick(object sender,ODGridClickEventArgs e) {
			if(e.Row==-1) {
				return;
			}
			comboClinicSms.SelectedIndex=e.Row;
			FillGridSmsUsage();
			SetSmsServiceAgreement();
		}

		private void butSmsUnsubscribe_Click(object sender,EventArgs e) {
			try {
				long ClinicNum=0;
				if(!PrefC.GetBool(PrefName.EasyNoClinics)) {
					ClinicNum=_smsClinicCur.ClinicNum;
				}
				if(!SmsPhones.UnSignContract(ClinicNum)) {
					MsgBox.Show(this,"Unable to unsign contract.");
					return;
				}
				if(PrefC.GetBool(PrefName.EasyNoClinics)) {
					Prefs.UpdateDateT(PrefName.SmsContractDate,DateTime.MinValue);
					textSmsLimit.Text="";
					Prefs.UpdateDouble(PrefName.SmsMonthlyLimit,0);
					DataValid.SetInvalid(InvalidType.Prefs);
					Prefs.RefreshCache();
				}
				else {
					_smsListClinics[comboClinicSms.SelectedIndex].SmsMonthlyLimit=0;
					textSmsLimit.Text="";
					_smsListClinics[comboClinicSms.SelectedIndex].SmsContractDate=DateTime.MinValue;
					Clinics.Update(_smsListClinics[comboClinicSms.SelectedIndex]);
					DataValid.SetInvalid(InvalidType.Providers);
					Clinics.RefreshCache();
				}
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
				return;
			}
			FillGridClinics();
			FillGridSmsUsage();
			SetSmsServiceAgreement();
			DataValid.SetInvalid(InvalidType.SmsTextMsgReceivedUnreadCount);
		}

		private void butBackMonth_Click(object sender,EventArgs e) {
			dateTimePickerSms.Value=dateTimePickerSms.Value.AddMonths(-1);
		}

		private void butFwdMonth_Click(object sender,EventArgs e) {
			dateTimePickerSms.Value=dateTimePickerSms.Value.AddMonths(1);//triggers refresh
		}

		private void butThisMonth_Click(object sender,EventArgs e) {
			dateTimePickerSms.Value=DateTime.Now.Date;//triggers refresh
		}

		private void dateTimePickerSms_ValueChanged(object sender,EventArgs e) {
			FillGridSmsUsage();
		}


		#endregion

		#region eConfirmations & eReminders

		private void FillTabRemindConfirm() {
			checkEnableNoClinic.Checked=PrefC.GetBool(PrefName.ApptConfirmEnableForClinicZero);
			if(PrefC.HasClinicsEnabled) {//CLINICS
				labelClinic.Visible=true;
				checkUseDefaultsEC.Visible=true;
				checkUseDefaultsEC.Enabled=false;//when loading form we will be viewing defaults.
				checkIsConfirmEnabled.Visible=true;
				groupAutomationStatuses.Text=Lan.g(this,"eConfirmation Settings")+" - "+Lan.g(this,"Affects all Clinics");
			}
			else {//NO CLINICS
				labelClinic.Visible=false;
				checkUseDefaultsEC.Visible=false;
				checkUseDefaultsEC.Enabled=false;
				checkUseDefaultsEC.Checked=false;
				checkIsConfirmEnabled.Visible=false;
				checkEnableNoClinic.Visible=false;
				groupAutomationStatuses.Text=Lan.g(this,"eConfirmation Settings");
			}
			setListClinicsAndDictRulesHelper();
			comboClinicEConfirm.SelectedIndex=0;
			_listDefsApptStatus=DefC.Short[(int)DefCat.ApptConfirmed].ToList();
			comboStatusESent.Items.Clear();
			comboStatusEAccepted.Items.Clear();
			comboStatusEDeclined.Items.Clear();
			comboStatusEFailed.Items.Clear();
			_listDefsApptStatus.ForEach(x => comboStatusESent.Items.Add(x.ItemName));
			_listDefsApptStatus.ForEach(x => comboStatusEAccepted.Items.Add(x.ItemName));
			_listDefsApptStatus.ForEach(x => comboStatusEDeclined.Items.Add(x.ItemName));
			_listDefsApptStatus.ForEach(x => comboStatusEFailed.Items.Add(x.ItemName));
			//SENT
			if(PrefC.GetLong(PrefName.ApptEConfirmStatusSent)>0) {
				comboStatusESent.SelectedIndex=_listDefsApptStatus.FindIndex(x => x.DefNum==PrefC.GetLong(PrefName.ApptEConfirmStatusSent));
			}
			else {
				comboStatusESent.SelectedIndex=0;
			}
			//CONFIRMED
			if(PrefC.GetLong(PrefName.ApptEConfirmStatusAccepted)>0) {
				comboStatusEAccepted.SelectedIndex=_listDefsApptStatus.FindIndex(x => x.DefNum==PrefC.GetLong(PrefName.ApptEConfirmStatusAccepted));
			}
			else {
				comboStatusEAccepted.SelectedIndex=0;
			}
			//NOT CONFIRMED
			if(PrefC.GetLong(PrefName.ApptEConfirmStatusDeclined)>0) {
				comboStatusEDeclined.SelectedIndex=_listDefsApptStatus.FindIndex(x => x.DefNum==PrefC.GetLong(PrefName.ApptEConfirmStatusDeclined));
			}
			else {
				comboStatusEDeclined.SelectedIndex=0;
			}
			//Failed
			if(PrefC.GetLong(PrefName.ApptEConfirmStatusSendFailed)>0) {
				comboStatusEFailed.SelectedIndex=_listDefsApptStatus.FindIndex(x => x.DefNum==PrefC.GetLong(PrefName.ApptEConfirmStatusSendFailed));
			}
			else {
				comboStatusEFailed.SelectedIndex=0;
			}
			FillRemindConfirmData();
		}

		///<summary>Fills in memory Rules dictionary and clinics list based. This is very different from AppointmentReminderRules.GetRuleAndClinics.</summary>
		private void setListClinicsAndDictRulesHelper() {
			if(PrefC.HasClinicsEnabled) {//CLINICS
				_ecListClinics=new List<Clinic>() { new Clinic() { Description="Defaults",Abbr="Defaults" } };
				_ecListClinics.AddRange(Clinics.GetForUserod(Security.CurUser));
			}
			else {//NO CLINICS
				_ecListClinics=new List<Clinic>() { new Clinic() { Description="Practice",Abbr="Practice" } };
			}
			List<ApptReminderRule> listRulesTemp = ApptReminderRules.GetAll();
			_dictClinicRules=_ecListClinics.Select(x => x.ClinicNum).ToDictionary(x => x,x => listRulesTemp.FindAll(y => y.ClinicNum==x));
			int idx = comboClinicEConfirm.SelectedIndex>0 ? comboClinicEConfirm.SelectedIndex : 0;
			comboClinicEConfirm.BeginUpdate();
			comboClinicEConfirm.Items.Clear();
			_ecListClinics.ForEach(x => comboClinicEConfirm.Items.Add(x.Abbr));//combo clinics may not be visible.
			if(idx>-1 && idx<comboClinicEConfirm.Items.Count) {
				comboClinicEConfirm.SelectedIndex=idx;
			}
			comboClinicEConfirm.EndUpdate();
		}

		private void FillRemindConfirmData() {
			fillGridRemindersMain();
			setAddButtons();
			//Reminder Activation Status
			if(PrefC.GetBool(PrefName.ApptRemindAutoEnabled)) {
				textStatusReminders.Text=Lan.g(this,"eReminders")+" : "+Lan.g(this,"Active");
				textStatusReminders.BackColor=Color.FromArgb(236,255,236);//light green
				textStatusReminders.ForeColor=Color.Black;//instead of disabled grey
				butActivateReminder.Text=Lan.g(this,"Deactivate eReminders");
			}
			else {
				textStatusReminders.Text=Lan.g(this,"eReminders")+" : "+Lan.g(this,"Inactive");
				textStatusReminders.BackColor=Color.FromArgb(254,235,233);//light red;
				textStatusReminders.ForeColor=Color.Black;//instead of disabled grey
				butActivateReminder.Text=Lan.g(this,"Activate eReminders");
			}
			//Confirmation Activation Status
			if(PrefC.GetBool(PrefName.ApptConfirmAutoEnabled)) {
				textStatusConfirmations.Text=Lan.g(this,"eConfirmations")+" : "+Lan.g(this,"Active");
				textStatusConfirmations.BackColor=Color.FromArgb(236,255,236);//light green
				textStatusConfirmations.ForeColor=Color.Black;//instead of disabled grey
				butActivateConfirm.Text=Lan.g(this,"Deactivate eConfirmations");
			}
			else {
				textStatusConfirmations.Text=Lan.g(this,"eConfirmations")+" : "+Lan.g(this,"Inactive");
				textStatusConfirmations.BackColor=Color.FromArgb(254,235,233);//light red;
				textStatusConfirmations.ForeColor=Color.Black;//instead of disabled grey
				butActivateConfirm.Text=Lan.g(this,"Activate eConfirmations");
			}
		}

		private void fillGridRemindersMain() {
			gridRemindersMain.BeginUpdate();
			gridRemindersMain.Columns.Clear();
			gridRemindersMain.Columns.Add(new ODGridColumn("Type",150) { TextAlign=HorizontalAlignment.Center });
			gridRemindersMain.Columns.Add(new ODGridColumn("Lead Time",250));
			//gridRemindersMain.Columns.Add(new ODGridColumn("Send\r\nAll",50) { TextAlign=HorizontalAlignment.Center });
			gridRemindersMain.Columns.Add(new ODGridColumn("Send Order",100));
			gridRemindersMain.NoteSpanStart=1;
			gridRemindersMain.NoteSpanStop=2;
			gridRemindersMain.Rows.Clear();
			ODGridRow row;
			List<ApptReminderRule> listTemp = new List<ApptReminderRule>();
			if(_ecClinicCur==null || _ecClinicCur.IsConfirmDefault) {//Use defaults
				_clinicRuleIdx=0;
			}
			else {
				_clinicRuleIdx=_ecClinicCur.ClinicNum;
			}
			listTemp=_dictClinicRules[_clinicRuleIdx];
			foreach(ApptReminderRule apptRule in listTemp) {
				string sendOrderText = string.Join(", ",apptRule.SendOrder.Split(',').Select(x => Enum.Parse(typeof(CommType),x).ToString()));
				row=new ODGridRow();
				row.Cells.Add(Lan.g(this,apptRule.TypeCur.GetDescription())
					+(_ecClinicCur.IsConfirmDefault ? "\r\n("+Lan.g(this,"Defaults")+")" : ""));
				if(apptRule.TSPrior<=TimeSpan.Zero) {
					row.Cells.Add(Lan.g(this,"Disabled"));
				}
				else {
					row.Cells.Add(apptRule.TSPrior.ToStringDH());
				}
				row.Cells.Add(apptRule.IsSendAll ? Lan.g(this,"All") : sendOrderText );
				row.Note=Lan.g(this,"SMS Template")+":\r\n"+apptRule.TemplateSMS+"\r\n\r\n"+Lan.g(this,"Email Subject Template")+":\r\n"+apptRule.TemplateEmailSubject+"\r\n"+Lan.g(this,"Email Template")+":\r\n"+apptRule.TemplateEmail;
				row.Tag=apptRule;
				if(gridRemindersMain.Rows.Count%2==1) {
					row.ColorBackG=Color.FromArgb(240,240,240);//light gray every other row.
				}
				gridRemindersMain.Rows.Add(row);
			}
			gridRemindersMain.EndUpdate();
		}

		private void gridRemindersMain_CellDoubleClick(object sender,ODGridClickEventArgs e) {
			if(e.Row<0 || !(gridRemindersMain.Rows[e.Row].Tag is ApptReminderRule)) {
				return;//we did not click on a valid row.
			}
			if(_ecClinicCur!=null && _ecClinicCur.ClinicNum>0 && _ecClinicCur.IsConfirmDefault && !switchFromDefaults()) {
				return;
			}
			ApptReminderRule arr = (ApptReminderRule)gridRemindersMain.Rows[e.Row].Tag;
			FormApptReminderRuleEdit FormARRE = new FormApptReminderRuleEdit(arr);
			FormARRE.ShowDialog();
			if(FormARRE.DialogResult!=DialogResult.OK) {
				return;
			}
			if(FormARRE.ApptReminderRuleCur==null) {//Delete
				_dictClinicRules[_clinicRuleIdx].RemoveAt(e.Row);
			}
			else if(FormARRE.ApptReminderRuleCur.IsNew) {//Update
				_dictClinicRules[_clinicRuleIdx].Add(FormARRE.ApptReminderRuleCur);//should never happen from the double click event
			}
			else {//Insert
				_dictClinicRules[_clinicRuleIdx][e.Row]=FormARRE.ApptReminderRuleCur;
			}
			FillRemindConfirmData();
		}

		private void setAddButtons() {
			if(comboClinicEConfirm.SelectedIndex>0) {//REAL CLINIC
				checkUseDefaultsEC.Visible=true;
				checkUseDefaultsEC.Enabled=true;
				checkIsConfirmEnabled.Enabled=true;//because we either cannot see it, or we are editing defaults.
				checkIsConfirmEnabled.Visible=true;
			}
			else {//CLINIC DEFAULTS/PRACTICE
				checkUseDefaultsEC.Visible=false;
				checkUseDefaultsEC.Enabled=false;
				checkIsConfirmEnabled.Enabled=false;//because we either cannot see it, or we are editing defaults.
				checkIsConfirmEnabled.Visible=false;
			}
			checkUseDefaultsEC.Checked=(_ecClinicCur!=null && _ecClinicCur.ClinicNum>0 && _ecClinicCur.IsConfirmDefault);
			if(_dictClinicRules[_clinicRuleIdx].Count(x => x.TypeCur==ApptReminderType.ReminderSameDay)==0) {
				butAddReminderSameDay.Enabled=true;
			}
			else {
				butAddReminderSameDay.Enabled=false;
			}
			if(_dictClinicRules[_clinicRuleIdx].Count(x => x.TypeCur==ApptReminderType.ReminderFutureDay)==0) {
				butAddReminderFutureDay.Enabled=true;
			}
			else {
				butAddReminderFutureDay.Enabled=false;
			}
			if(_dictClinicRules[_clinicRuleIdx].Count(x => x.TypeCur==ApptReminderType.ConfirmationFutureDay)==0) {
				butAddConfirmation.Enabled=true;
			}
			else {
				butAddConfirmation.Enabled=false;
			}
		}

		private void butAddReminderSameDay_Click(object sender,EventArgs e) {
			if(_ecClinicCur!=null && _ecClinicCur.ClinicNum>0 && !_ecClinicCur.IsConfirmDefault) {
				if(!switchFromDefaults()) {
					return;
				}
				if(_dictClinicRules[_ecClinicCur.ClinicNum].Count(x => x.TypeCur==ApptReminderType.ReminderSameDay && x.TSPrior.TotalDays<1)>0) {
					return;//Switched to defaults but reminder already exist.
				}
			}
			ApptReminderRule arr = ApptReminderRules.CreateDefaultReminderRule(ApptReminderType.ReminderSameDay,_ecClinicCur.ClinicNum);
			FormApptReminderRuleEdit FormARRE = new FormApptReminderRuleEdit(arr);
			FormARRE.ShowDialog();
			if(FormARRE.DialogResult!=DialogResult.OK) {
				return;
			}
			if(FormARRE.ApptReminderRuleCur==null || FormARRE.ApptReminderRuleCur.IsNew) {//Delete or Update
																																										//Nothing to update, this was a new rule.
			}
			else {//Insert
				_dictClinicRules[_clinicRuleIdx].Add(FormARRE.ApptReminderRuleCur);
			}
			FillRemindConfirmData();
		}

		private void butAddReminderFutureDay_Click(object sender,EventArgs e) {
			if(_ecClinicCur!=null && _ecClinicCur.ClinicNum>0 && !_ecClinicCur.IsConfirmDefault) {
				if(!switchFromDefaults()) {
					return;
				}
				if(_dictClinicRules[_ecClinicCur.ClinicNum].Count(x => x.TypeCur==ApptReminderType.ReminderFutureDay && x.TSPrior.TotalDays>=1)>0) {
					return;//Switched to defaults but reminder already exist.
				}
			}
			ApptReminderRule arr = ApptReminderRules.CreateDefaultReminderRule(ApptReminderType.ReminderFutureDay,_ecClinicCur.ClinicNum);
			FormApptReminderRuleEdit FormARRE = new FormApptReminderRuleEdit(arr);
			FormARRE.ShowDialog();
			if(FormARRE.DialogResult!=DialogResult.OK) {
				return;
			}
			if(FormARRE.ApptReminderRuleCur==null || FormARRE.ApptReminderRuleCur.IsNew) {//Delete or Update
				//Nothing to update, this was a new rule.
			}
			else {//Insert
				_dictClinicRules[_clinicRuleIdx].Add(FormARRE.ApptReminderRuleCur);
			}
			FillRemindConfirmData();
		}

		private void butAddConfirmation_Click(object sender,EventArgs e) {
			if(_ecClinicCur!=null && _ecClinicCur.ClinicNum>0 && _ecClinicCur.IsConfirmDefault) {
				if(!switchFromDefaults()) {
					return;
				}
				if(_dictClinicRules[_ecClinicCur.ClinicNum].Count(x => x.TypeCur==ApptReminderType.ConfirmationFutureDay)>0) {
					return;//Switched to defaults but confirmation already existed.
				}
			}
			ApptReminderRule arr = ApptReminderRules.CreateDefaultReminderRule(ApptReminderType.ConfirmationFutureDay,_ecClinicCur.ClinicNum);
			FormApptReminderRuleEdit FormARRE = new FormApptReminderRuleEdit(arr);
			FormARRE.ShowDialog();
			if(FormARRE.DialogResult!=DialogResult.OK) {
				return;
			}
			if(FormARRE.ApptReminderRuleCur==null || FormARRE.ApptReminderRuleCur.IsNew) {
				//Delete or Update
				//Nothing to delete or update, this was a new rule.
			}
			else {//Insert
				_dictClinicRules[_clinicRuleIdx].Add(FormARRE.ApptReminderRuleCur);
			}
			FillRemindConfirmData();
		}

		private void comboClinicEConfirm_SelectedIndexChanged(object sender,EventArgs e) {
			if(_ecListClinics.Count==0 || _dictClinicRules.Count==0) {
				return;//form load;
			}
			if(_ecClinicCur!=null && _ecClinicCur.ClinicNum>0) {//do not update this clinic-pref if we are editing defaults.
				_ecClinicCur.IsConfirmEnabled=checkIsConfirmEnabled.Checked;
				Clinics.Update(_ecClinicCur);
				Signalods.SetInvalid(InvalidType.Providers);
				//no need to save changes here because all Appointment reminder rules are saved to the DB from the edit window.
			}
			if(_ecClinicCur!=null) {
				ApptReminderRules.SyncByClinic(_dictClinicRules[_ecClinicCur.ClinicNum],_ecClinicCur.ClinicNum);
			}
			if(comboClinicEConfirm.SelectedIndex>-1 && comboClinicEConfirm.SelectedIndex<_ecListClinics.Count) {
				_ecClinicCur=_ecListClinics[comboClinicEConfirm.SelectedIndex];
			}
			checkUseDefaultsEC.Checked=_ecClinicCur!=null && _ecClinicCur.IsConfirmDefault;
			checkIsConfirmEnabled.Checked=_ecClinicCur!=null && _ecClinicCur.IsConfirmEnabled;
			FillRemindConfirmData();
		}

		///<summary>Switches the currently selected clinic over to using defaults. Also prompts user before continuing. 
		///Returns false if user cancelled or if there is no need to have switched to defaults.</summary>
		private bool switchFromDefaults() {
			if(_ecClinicCur==null || _ecClinicCur.ClinicNum==0) {
				return false;//somehow editing default clinic anyways, no need to switch.
			}
			//if(!MsgBox.Show(this,true,"Would you like to make a copy of the defaults for this clinic and continue editing the copy?")) {
			//	return false;
			//}
			_dictClinicRules[_ecClinicCur.ClinicNum]=_dictClinicRules[0].Select(x => x.Copy()).ToList();
			_dictClinicRules[_ecClinicCur.ClinicNum].ForEach(x => x.ClinicNum=_ecClinicCur.ClinicNum);
			_ecClinicCur.IsConfirmDefault=false;
			_ecListClinics[_ecListClinics.FindIndex(x => x.ClinicNum==_ecClinicCur.ClinicNum)].IsConfirmDefault=false;
			//Clinics.Update(_clinicCur);
			//Signalods.SetInvalid(InvalidType.Providers);//for clinics
			FillRemindConfirmData();
			return true;
		}

		///<summary>Switches the currently selected clinic over to using defaults. Also prompts user before continuing. 
		///Returns false if user cancelled or if there is no need to have switched to defaults.</summary>
		private bool switchToDefaults() {
			if(_ecClinicCur==null || _ecClinicCur.ClinicNum==0) {
				return false;//somehow editing default clinic anyways, no need to switch.
			}
			if(_dictClinicRules[_ecClinicCur.ClinicNum].Count>0 && !MsgBox.Show(this,true,"Delete custom rules for this clinic and switch to using defaults? This cannot be undone.")) {
				checkUseDefaultsEC.Checked=false;//undo checking of box.
				return false;
			}
			_ecClinicCur.IsConfirmDefault=true;
			_dictClinicRules[_ecClinicCur.ClinicNum]=new List<ApptReminderRule>();
			FillRemindConfirmData();
			return true;
		}

		private void checkIsConfirmEnabled_CheckedChanged(object sender,EventArgs e) {
			FillRemindConfirmData();
		}

		private void checkUseDefaultsEC_CheckedChanged(object sender,EventArgs e) {
			//TURNING DEFAULTS OFF
			if(!checkUseDefaultsEC.Checked && _ecClinicCur.IsConfirmDefault && _ecClinicCur.ClinicNum>0) {//Default switched off
				_ecClinicCur.IsConfirmDefault=false;
				_ecListClinics[comboClinicEConfirm.SelectedIndex].IsConfirmDefault=false;
				FillRemindConfirmData();
				return;
			}
			//TURNING DEFAULTS ON
			else if(checkUseDefaultsEC.Checked && !_ecClinicCur.IsConfirmDefault && _ecClinicCur.ClinicNum>0) {//Default switched on
				switchToDefaults();
				return;
			}
			//Silently do nothing because we just "changed" the checkbox to the state of the current clinic. 
			//I.e. When switching from clinic 1 to clinic 2, if 1 uses defaults and 2 does not, then this allows the new clinic to be loaded without updating the DB.
		}

		private void butActivateConfirm_Click(object sender,EventArgs e) {
			if(!Security.IsAuthorized(Permissions.SecurityAdmin)
				|| !MsgBox.Show(this,MsgBoxButtons.OKCancel,"eConfirmations is a paid service. Access and usage charges apply, see website for details.")) {
				return;
			}
			var myServ = WebServiceMainHQProxy.GetWebServiceMainHQInstance();
			bool isApptConfirmAutoEnabled = PrefC.GetBool(PrefName.ApptConfirmAutoEnabled);
			try {
				if(!isApptConfirmAutoEnabled) {//Sign-up for ConfirmationRequest.
					string result = OpenDentBusiness.WebServiceMainHQProxy.GetWebServiceMainHQInstance().ConfirmationRequestSignAgreement(
						OpenDentBusiness.WebServiceMainHQProxy.CreateWebServiceHQPayload("",OpenDentBusiness.eServiceCode.ConfirmationRequest));
					XmlDocument doc = new XmlDocument();
					doc.LoadXml(result);
					XmlNode node = doc.SelectSingleNode("//Error");
					if(node!=null) {
						MsgBox.Show(this,node.InnerText);
						return;
					}
					node=doc.SelectSingleNode("//Success");
					if(node==null) { //Should never happen, we didn't get an explicit fail or success
						MsgBox.Show(this,"Unknown error has occured.");
						return;
					}
				}
				else {//Unsubscribe ConfirmationRequest.
					string result = OpenDentBusiness.WebServiceMainHQProxy.GetWebServiceMainHQInstance().ConfirmationRequestCancelService(
						OpenDentBusiness.WebServiceMainHQProxy.CreateWebServiceHQPayload("",OpenDentBusiness.eServiceCode.ConfirmationRequest));
					XmlDocument doc = new XmlDocument();
					doc.LoadXml(result);
					XmlNode node = doc.SelectSingleNode("//Error");
					if(node!=null) {
						MsgBox.Show(this,node.InnerText);
						return;
					}
					node=doc.SelectSingleNode("//Success");
					if(node==null) { //Should never happen, we didn't get an explicit fail or success
						MsgBox.Show(this,"Unknown error has occured.");
						return;
					}
				}
			}
			catch(Exception ex) {
				ex.DoNothing();
				MessageBox.Show(this,"Operation failed. Unable to change automated eConfirmation activation status.");
				return;
			}
			isApptConfirmAutoEnabled=!isApptConfirmAutoEnabled;
			Prefs.UpdateBool(PrefName.ApptConfirmAutoEnabled,isApptConfirmAutoEnabled);
			SecurityLogs.MakeLogEntry(Permissions.Setup,0,"Automated appointment eConfirmations "+(isApptConfirmAutoEnabled ? "activated" : "deactivated")+".");
			Prefs.RefreshCache();
			Signalods.SetInvalid(InvalidType.Prefs);
			FillRemindConfirmData();//updates all UI fields
		}

		private void butActivateReminder_Click(object sender,EventArgs e) {
			bool isApptRemindAutoEnabled = PrefC.GetBool(PrefName.ApptRemindAutoEnabled);
			isApptRemindAutoEnabled=!isApptRemindAutoEnabled;
			Prefs.UpdateBool(PrefName.ApptRemindAutoEnabled,isApptRemindAutoEnabled);
			SecurityLogs.MakeLogEntry(Permissions.Setup,0,"Automated appointment eReminders "+(isApptRemindAutoEnabled ? "activated" : "deactivated")+".");
			Prefs.RefreshCache();
			Signalods.SetInvalid(InvalidType.Prefs);
			FillRemindConfirmData();//updates all UI fields
		}

		private void butWizardConfirm_Click(object sender,EventArgs e) {
			//======================SAVE POTENTIAL CHANGES BEFORE STARTING======================
			Prefs.UpdateBool(PrefName.ApptConfirmEnableForClinicZero,checkEnableNoClinic.Checked);
			ApptReminderRules.SyncByClinic(_dictClinicRules[_ecClinicCur.ClinicNum],_ecClinicCur.ClinicNum);
			if(_ecClinicCur!=null && _ecClinicCur.ClinicNum!=0) {
				_ecClinicCur.IsConfirmEnabled=checkIsConfirmEnabled.Checked;
				Clinics.Update(_ecClinicCur);
				Signalods.SetInvalid(InvalidType.Providers);//Includes Clinics.
			}
			//======================GATHER WIZARD DATA======================
			List<ApptReminderRule> listRulesAllTemp = ApptReminderRules.GetAll();
			List<Clinic> listClinicsAllTemp = Clinics.GetList().ToList();
			ApptReminderRule ruleRemindDefault = listRulesAllTemp.FirstOrDefault(x => x.ClinicNum==0 && x.TypeCur==ApptReminderType.ReminderSameDay);
			ApptReminderRule ruleConfirmDefault = listRulesAllTemp.FirstOrDefault(x => x.ClinicNum==0 && x.TypeCur==ApptReminderType.ConfirmationFutureDay);
			//======================HELPER FUNCTIONS======================
			Func<string,string,bool> promptUserB = (heading,text) => {
				bool b = false;
				switch(MessageBox.Show(Lan.g(this,text),Lan.g(this,heading),MessageBoxButtons.YesNoCancel)) {
					case DialogResult.Yes:
						b=true;
						break;
					case DialogResult.Cancel:
						b=false;
						throw new ApplicationException(Lan.g(this,"Setup wizard cancelled. Run again at anytime to continue."));
					case DialogResult.No:
					default:
						b=false;
						break;
				}
				return b;
			};
			Action refreshLocalData = () => {
				Prefs.RefreshCache();
				Clinics.RefreshCache();
				listClinicsAllTemp = Clinics.GetList().ToList();
				listRulesAllTemp = ApptReminderRules.GetAll();
				ruleRemindDefault = listRulesAllTemp.FirstOrDefault(x => x.ClinicNum==0 && x.TypeCur==ApptReminderType.ReminderSameDay);
				ruleConfirmDefault = listRulesAllTemp.FirstOrDefault(x => x.ClinicNum==0 && x.TypeCur==ApptReminderType.ConfirmationFutureDay);
			};
			Action RefreshPublicData = () => {
				refreshLocalData();
				Signalods.SetInvalid(InvalidType.Prefs,InvalidType.Providers);
				FillTabRemindConfirm();
			};
			bool isReminderStarted = false;
			bool isConfirmationStarted = false;
			try {
				//======================INTEGRATED TEXTING======================
				if(!SmsPhones.IsIntegratedTextingEnabled()
					&& promptUserB("Enable Integrated Texting first?","Before we get started activating your automated messaging, we noticed that you have not enabled texting, which has fees associated with it.  Automated messaging is more effective if you include texting, but you can skip it if you prefer.\r\nWould you like to setup and enable Integrated Texting now?")) {
					tabControl.SelectedTab=tabSmsServices;
					MsgBox.Show(this,"Setup wizard suspended. Return to the Automated eConfirmation and eReminder tab and click Setup Wizard again to resume.");
					return;
					//FormEServicesSetup FormESS = new FormEServicesSetup(FormEServicesSetup.EService.SmsService);
					//FormESS.ShowDialog();
				}
				//======================REMINDERS======================
				bool isReminders = PrefC.GetBool(PrefName.ApptRemindAutoEnabled);
				if(!isReminders) {
					isReminders=promptUserB("eReminders Activation","Would you like to activate eReminders?");
					isReminderStarted|=isReminders;//true only if user answered yes
				}
				if(isReminders) {
					if(!PrefC.GetBool(PrefName.ApptRemindAutoEnabled)) {
						butActivateReminder_Click("Wizard",new EventArgs());
					}
					if(ruleRemindDefault==null) {
						if(!promptUserB("eReminders Activation","Would you like to use the default eReminder rule? (Can be edited later)")) {
							isReminderStarted=true;
							FormApptReminderRuleEdit FormARRE = new FormApptReminderRuleEdit(ApptReminderRules.CreateDefaultReminderRule(ApptReminderType.ReminderSameDay));
							FormARRE.ShowDialog();
							if(FormARRE.DialogResult==DialogResult.OK || FormARRE.ApptReminderRuleCur!=null) {
								ruleRemindDefault=FormARRE.ApptReminderRuleCur;
							}
						}
						//If user cancelled or deleted the reminder rule above, then use defaults.
						if(ruleRemindDefault==null) {
							ruleRemindDefault=ApptReminderRules.CreateDefaultReminderRule(ApptReminderType.ReminderSameDay);
						}
					}
					if(ruleRemindDefault.ApptReminderRuleNum==0) {//only if created above
						ApptReminderRules.Insert(ruleRemindDefault);
					}
					if(PrefC.HasClinicsEnabled) {
						if(!PrefC.GetBool(PrefName.ApptConfirmEnableForClinicZero) && promptUserB("eReminders Activation","Automated eReminder and eConfirmation messages are sent based on which clinic a given appointment is assigned to. Do you want to allow automated messages to be sent for an appointment that is not associated with a clinic?")) {
							isReminderStarted=true;
							Prefs.UpdateBool(PrefName.ApptConfirmEnableForClinicZero,true);
						}
						if(listClinicsAllTemp.All(x => x.IsConfirmEnabled)) {
							//Good
						}
						else if(listClinicsAllTemp.All(x => !x.IsConfirmEnabled) || promptUserB("eReminders Activation","eReminders and eConfirmations are enabled for some Clinics but not others. Would you like to enable all remaining clinics?")) {
							listClinicsAllTemp.ForEach(x => x.IsConfirmEnabled=true);
						}
						//Some clinics found with no rules defined AND not using defaults. Set to use defaults.
						listClinicsAllTemp.FindAll(x => !x.IsConfirmDefault && listRulesAllTemp.Count(y => y.ClinicNum==x.ClinicNum)==0).ForEach(x => x.IsConfirmDefault=true);
						listClinicsAllTemp.ForEach(x => Clinics.Update(x));
					}
					if(isReminderStarted) {
						MsgBox.Show(this,"Congratulations, eReminder activation complete!");
					}
					refreshLocalData();
				}//End IsReminders
				 //======================CONFIRMATIONS======================
				bool IsConfirmations = PrefC.GetBool(PrefName.ApptConfirmAutoEnabled);
				if(!IsConfirmations) {
					IsConfirmations=promptUserB("eConfirmations Activation","Would you like to activate eConfirmations?");
					isConfirmationStarted|=IsConfirmations;
				}
				if(IsConfirmations) {
					if(!PrefC.GetBool(PrefName.ApptConfirmAutoEnabled)) {
						butActivateConfirm_Click("Wizard",new EventArgs());
						if(!PrefC.GetBool(PrefName.ApptConfirmAutoEnabled) && !promptUserB("eConfirmations Activation","Unable to activate eConfirmations, would you like to continue with the rest of the setup process?")) {
							RefreshPublicData();
							return;
						}
						isConfirmationStarted=true;
					}
					if(ruleConfirmDefault==null) {
						if(!promptUserB("eConfirmations Activation","Would you like to use the default eConfirmation rule? (Can be edited later)")) {
							FormApptReminderRuleEdit FormARRE = new FormApptReminderRuleEdit(ApptReminderRules.CreateDefaultReminderRule(ApptReminderType.ConfirmationFutureDay));
							FormARRE.ShowDialog();
							if(FormARRE.DialogResult==DialogResult.OK || FormARRE.ApptReminderRuleCur!=null) {
								ruleConfirmDefault=FormARRE.ApptReminderRuleCur;
								isConfirmationStarted=true;
							}
						}
						//If user cancelled or deleted the reminder rule above, then use defaults.
						if(ruleConfirmDefault==null) {
							ruleConfirmDefault=ApptReminderRules.CreateDefaultReminderRule(ApptReminderType.ConfirmationFutureDay);
							isConfirmationStarted=true;
						}
					}
					if(ruleConfirmDefault.ApptReminderRuleNum==0) {//only if created above
						ApptReminderRules.Insert(ruleConfirmDefault);
						isConfirmationStarted=true;
					}
					if(PrefC.HasClinicsEnabled) {
						if(!PrefC.GetBool(PrefName.ApptConfirmEnableForClinicZero) && promptUserB("eConfirmations Activation","Automated eReminder and eConfirmation messages are sent based on which clinic a given appointment is assigned to. Do you want to allow automated messages to be sent for an appointment that is not associated with a clinic?")) {
							Prefs.UpdateBool(PrefName.ApptConfirmEnableForClinicZero,true);
							isConfirmationStarted=true;
						}
						if(listClinicsAllTemp.All(x => x.IsConfirmEnabled)) {
							//Good
						}
						else if(listClinicsAllTemp.All(x => !x.IsConfirmEnabled) || promptUserB("eConfirmations Activation","eReminders and eConfirmations are enabled for some Clinics but not others. Would you like to enable all remaining clinics?")) {
							listClinicsAllTemp.ForEach(x => x.IsConfirmEnabled=true);
						}
						//Some clinics found with no rules defined AND not using defaults. Set to use defaults.
						listClinicsAllTemp.FindAll(x => !x.IsConfirmDefault && listRulesAllTemp.Count(y => y.ClinicNum==x.ClinicNum)==0).ForEach(x => x.IsConfirmDefault=true);
						listClinicsAllTemp.ForEach(x => Clinics.Update(x));
					}
					if(isConfirmationStarted) {
						MsgBox.Show(this,"Congratulations, eConfirmation setup complete!");
					}
				}//End IsConfirmations
			}
			catch(ApplicationException ae) {
				MessageBox.Show(ae.Message);
			}
			catch(Exception ex) {
				MessageBox.Show(Lan.g(this,"An unexpected error has occured")+":\r\n\r\n"+ex.Message);
			}
			finally {
				RefreshPublicData();
			}
		}
		#endregion eConfirmations & eReminders

		///<summary>Validate form inputs and display any required messages to user. Returns true if all info valid.</summary>
		private bool validateForm() {
			if(comboStatusESent.SelectedIndex<0 || comboStatusEAccepted.SelectedIndex<0 || comboStatusEDeclined.SelectedIndex<0 || comboStatusEFailed.SelectedIndex<0) {
				MsgBox.Show(this,"Must select an appointment status for each eConfirmation status.");
				return false;
			}
			if(new[] { comboStatusEAccepted.SelectedIndex,comboStatusESent.SelectedIndex,comboStatusEDeclined.SelectedIndex,comboStatusEFailed.SelectedIndex }.GroupBy(x => x).Any(x => x.Count()>1)) {
				if(!MsgBox.Show(this,MsgBoxButtons.YesNo,"All eConfirmation appointment statuses should be different. Continue anyway?")) {
					return false;
				}
			}
			return true;
		}

		private void tabControl_SelectedIndexChanged(object sender,EventArgs e) {
			//jsalmon - The following method call was causing the "not authorized for ..." message to continuously pop up and was very annoying.
			//SetControlEnabledState();
		}

		///<summary>The sole purpose of this function is to remove compiler warnings. Should never be called. Does nothing.</summary>
		private void DoNothing() {
			var nothing = new object[] { _statementLimitPerPatient };
		}

		private void butClose_Click(object sender,EventArgs e) {
			Close();
		}

		private void FormEServicesSetup_FormClosing(object sender,FormClosingEventArgs e) {
			if(!validateForm()) {
				e.Cancel=true;
				return;
			}
			WebSchedAutomaticSend sendType = WebSchedAutomaticSend.SendToEmailOnlyPreferred;
			if(radioDoNotSend.Checked) {
				sendType=WebSchedAutomaticSend.DoNotSend;
			}
			else if(radioSendToEmail.Checked) {
				sendType=WebSchedAutomaticSend.SendToEmail;
			}
			else if(radioSendToEmailNoPreferred.Checked) {
				sendType=WebSchedAutomaticSend.SendToEmailNoPreferred;
			}
			_hasPrefsChanged|=
				Prefs.UpdateInt(PrefName.WebSchedAutomaticSendSetting,(int)sendType)
				| Prefs.UpdateDateT(PrefName.AutomaticCommunicationTimeStart,dateRunStart.Value)
				| Prefs.UpdateDateT(PrefName.AutomaticCommunicationTimeEnd,dateRunEnd.Value)
				| Prefs.UpdateLong(PrefName.ApptEConfirmStatusSent,_listDefsApptStatus[comboStatusESent.SelectedIndex].DefNum)
				| Prefs.UpdateLong(PrefName.ApptEConfirmStatusAccepted,_listDefsApptStatus[comboStatusEAccepted.SelectedIndex].DefNum)
				| Prefs.UpdateLong(PrefName.ApptEConfirmStatusDeclined,_listDefsApptStatus[comboStatusEDeclined.SelectedIndex].DefNum)
				| Prefs.UpdateLong(PrefName.ApptEConfirmStatusSendFailed,_listDefsApptStatus[comboStatusEFailed.SelectedIndex].DefNum)
				| Prefs.UpdateBool(PrefName.ApptConfirmEnableForClinicZero,checkEnableNoClinic.Checked);
			ApptReminderRules.SyncByClinic(_dictClinicRules[_ecClinicCur.ClinicNum],_ecClinicCur.ClinicNum);
			if(_ecClinicCur!=null && _ecClinicCur.ClinicNum!=0) {
				_ecClinicCur.IsConfirmEnabled=checkIsConfirmEnabled.Checked;
				Clinics.Update(_ecClinicCur);
				_hasClinicsChanged=true;
			}
		}

		private void FormPatientPortalSetup_FormClosed(object sender,FormClosedEventArgs e) {
			if(_hasPrefsChanged) {
				DataValid.SetInvalid(InvalidType.Prefs);
			}
			if(_hasClinicsChanged) {
				DataValid.SetInvalid(InvalidType.Providers);//Providers includes clinics.
			}
		}

		///<summary>This is a helper struct which is set to the Tag of the each row in gridWebSchedNewPatApptURLs</summary>
		private struct NewPatApptURL {
			///<summary>Will be set to 0 for headquarters.</summary>
			public long ClinicNum;
			///<summary>The web service will return a valid URL for this specific clinic.</summary>
			public string URL;
			///<summary>Will be null for headquarters.</summary>
			public Clinic Clinic;
		}

		private enum SynchEntity {
			patient,
			appointment,
			prescription,
			provider,
			pharmacy,
			labpanel,
			labresult,
			medication,
			medicationpat,
			allergy,
			allergydef,
			disease,
			diseasedef,
			icd9,
			statement,
			document,
			recall,
			deletedobject,
			patientdel
		}

		///<summary>Typically used in ctor determine which tab should be activated be default.</summary>
		public enum EService {
			PatientPortal,
			MobileOld,
			MobileNew,
			WebSched,
			ListenerService,
			SmsService,
			eConfirmRemind,
			eMisc
		}
		
	}
}