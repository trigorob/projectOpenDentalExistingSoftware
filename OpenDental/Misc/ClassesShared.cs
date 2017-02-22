using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text; 
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;
using OpenDentBusiness;
using CodeBase;

namespace OpenDental{

	///<summary></summary>
	public class Shared{

		///<summary></summary>
		public Shared(){
			
		}

		///<summary>Converts numbers to ordinals.  For example, 120 to 120th, 73 to 73rd.  Probably doesn't work too well with foreign language translations.  Used in the Birthday postcards.</summary>
		public static string NumberToOrdinal(int number){
			if(number==11) {
				return "11th";
			}
			if(number==12) {
				return "12th";
			}
			if(number==13) {
				return "13th";
			}
			string str=number.ToString();
			string last=str.Substring(str.Length-1);
			switch(last){
				case "0":
				case "4":
				case "5":
				case "6":
				case "7":
				case "8":
				case "9":
					return str+"th";
				case "1":
					return str+"st";
				case "2":
					return str+"nd";
				case "3":
					return str+"rd";
			}
			return "";//will never happen
		}

		///<summary>Returns false if the backup, repair, or the optimze failed.
		///Set isSilent to true to suppress the failure message boxes.  However, progress windows will always be shown.</summary>
		public static bool BackupRepairAndOptimize(bool isSilent,BackupLocation backupLocation,bool isSecurityLogged=true) {
			if(!MakeABackup(isSilent,backupLocation,isSecurityLogged)) {
				return false;
			}
			//Create a thread that will show a window and then stay open until the closing phrase is thrown from this form.
			Action actionCloseRepairAndOptimizeProgress=ODProgress.ShowProgressStatus("RepairAndOptimizeProgress");
			try {
				DatabaseMaintenance.RepairAndOptimize();
				actionCloseRepairAndOptimizeProgress();
			}
			catch(Exception ex) {//MiscData.MakeABackup() could have thrown an exception.
				actionCloseRepairAndOptimizeProgress();
				//Show the user that something what went wrong when not in silent mode.
				if(!isSilent) {
					if(ex.Message!="") {
						MessageBox.Show(ex.Message);
					}
					MsgBox.Show("FormDatabaseMaintenance","Optimize and Repair failed.");
				}
				return false;
			}
			return true;
		}

		///<summary>This is a wrapper method for MiscData.MakeABackup() that will show a progress window so that the user can see progress.
		///Returns false if making a backup failed.</summary>
		public static bool MakeABackup(BackupLocation backupLocation) {
			return MakeABackup(false,backupLocation);
		}

		///<summary>This is a wrapper method for MiscData.MakeABackup() that will show a progress window so that the user can see progress.
		///Set isSilent to true to suppress the failure message boxes.  However, the progress window will always be shown.
		///Returns false if making a backup failed.</summary>
		public static bool MakeABackup(bool isSilent,BackupLocation backupLocation,bool isSecurityLogged=true) {
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				return false;//Because MiscData.MakeABackup() is not yet Oracle compatible.
			}
			//Create a thread that will show a window and then stay open until the closing action is called.
			Action actionCloseBackupProgress=ODProgress.ShowProgressStatus("BackupProgress");
			try {
				MiscData.MakeABackup();
				actionCloseBackupProgress();//Close the progress window.
			}
			catch(Exception ex) {//MiscData.MakeABackup() could have thrown an exception.
				actionCloseBackupProgress();//Close the progress window.
				//Show the user that something what went wrong when not in silent mode.
				if(!isSilent) {
					if(ex.Message!="") {
						MessageBox.Show(ex.Message);
					}
					//Reusing translation in ClassConvertDatabase, since it is most likely the only place a translation would have been performed previously.
					MsgBox.Show("ClassConvertDatabase","Backup failed. Your database has not been altered.");
				}
				return false;
			}
			if(isSecurityLogged && PrefC.GetStringNoCache(PrefName.UpdateStreamLinePassword)!="abracadabra") {
				SecurityLogs.MakeLogEntryNoCache(Permissions.Backup,0,Lan.g("Backups","A backup was created when running the")+" "+backupLocation.ToString());
			}
			return true;
		}


	}

	///<summary>A wrapper class around FormProgressStatus so that other engineers are not responsible for managing their own progress statuses.</summary>
	public class ODProgress {

		///<summary>Launches a progress window that will listen specifically for ODEvents with the passed in name.
		///Returns an action that should be invoked whenever the long computations have finished which will close the progress window.
		///eventName should be set to the name of the ODEvents that this specific progress window should be processing.
		///Optionally set the startingMessage to whatever you desire.  If not set, defaults to "Please Wait..."</summary>
		public static Action ShowProgressStatus(string eventName,string startingMessage="") {
			return ShowProgressStatus(eventName,typeof(ODEvent),false,startingMessage);
		}

		///<summary>Launches a progress window that will listen specifically for ODEvents with the passed in name.
		///Returns an action that should be invoked whenever the long computations have finished which will close the progress window.
		///eventName should be set to the name of the ODEvents that this specific progress window should be processing.
		///eventType must be a Type that contains an event called "Fired" and a method called "Fire".
		///Optionally set tag to an object that should be sent as the first "event arg" to the new progress window.
		///	This is typically a string that should be the very first message that shows to the user.
		///	If not set, the default message that will display will be "Please Wait..."</summary>
		public static Action ShowProgressStatus(string eventName,Type eventType,bool hasHistory=false,object tag=null) {
			ODThread odThread=new ODThread(new ODThread.WorkerDelegate((ODThread o) => {
				FormProgressStatus FormPS=new FormProgressStatus(eventName,eventType,hasHistory);
				FormPS.TopMost=true;//Make this window show on top of ALL other windows.
				//Set the starting progress message if one was passed in.
				if(tag!=null) {
					FormPS.ODEvent_Fired(new ODEventArgs(eventName,tag));
				}
				//Check to make sure that the calling method hasn't already finished its long computations.
				//odThread's tag will be set to "true" if all computations have already finished and thus we do not need to show any progress window.
				if(o.Tag!=null && o.Tag.GetType()==typeof(bool) && (bool)o.Tag) {
					try {
						FormPS.Close();//Causes FormProgressStatus_FormClosing to get called which deregisters the ODEvent handler it currently has.
					}
					catch(Exception ex) {
						ex.DoNothing();
					}
					return;
				}
				//From this point forward, the only way to kill FormProgressStatus is with a DEFCON 1 message via an ODEvent with the corresponding eventName.
				FormPS.ShowDialog();
			}));
			odThread.SetApartmentState(ApartmentState.STA);//This is required for ReportComplex due to the history UI elements.
			odThread.AddExceptionHandler(new ODThread.ExceptionDelegate((Exception e) => { }));//Do nothing.
			odThread.Name="ProgressStatusThread_"+eventName;
			odThread.Start(true);
			return new Action(() => {
				//For progress threads, there is a race condition where the DEFCON 1 event will not get processed.
				//This is due to the fact that it took the thread longer to instantiate FormProgressStatus than it took the calling method to invoke this action.
				//Since we don't have access to FormProgressStatus within the thread from here, we simply flag odThread's Tag so that it knows to die.
				if(odThread!=null) {
					odThread.Tag=true;
				}
				//Send the phrase that closes the window in case it is processing events.
				eventType.GetMethod("Fire").Invoke(eventType,new object[] { new ODEventArgs(eventName,"DEFCON 1") });
			});
		}

		///<summary>FormProgressExtended is extremely tailored to monitoring the progress of FormBilling.
		///Much will need to change about this to make it generic.
		///Launches a progress window that will listen specifically for ODEvents with the passed in name.
		///Returns an action that should be invoked whenever the long computations have finished which will close the progress window.
		///eventName should be set to the name of the ODEvents that this specific progress window should be processing.
		///eventType must be a Type that contains an event called "Fired" and a method called "Fire".
		///Optionally set tag to an object that should be sent as the first "event arg" to the new progress window.
		///	This will typically be a ProgressBarHelper.</summary>
		public static Action ShowProgressExtended(string eventName,Type eventType,object tag=null) {
			ODThread odThread=new ODThread(new ODThread.WorkerDelegate((ODThread o) => {
				FormProgressExtended FormPE=new FormProgressExtended(eventName,eventType);
				//FormProgressExtended should NOT be the top most form.  Other windows might be popping up requiring attention from the user.
				//FormPE.TopMost=true;//Make this window show on top of ALL other windows.
				if(tag!=null) {
					FormPE.ODEvent_Fired(new ODEventArgs(eventName,tag));
				}
				//Check to make sure that the calling method hasn't already finished its long computations.
				//odThread's tag will be set to "true" if all computations have already finished and thus we do not need to show any progress window.
				if(o.Tag!=null && o.Tag.GetType()==typeof(bool) && (bool)o.Tag) {
					try {
						FormPE.Close();//Causes FormProgressStatus_FormClosing to get called which deregisters the ODEvent handler it currently has.
					}
					catch(Exception ex) {
						ex.DoNothing();
					}
					return;
				}
				FormPE.BringToFront();
				//From this point forward, the only way to kill FormProgressStatus is with a DEFCON 1 message via an ODEvent with the corresponding eventName.
				FormPE.ShowDialog();
			}));
			odThread.SetApartmentState(ApartmentState.STA);//This is required for ReportComplex due to the history UI elements.
			odThread.AddExceptionHandler(new ODThread.ExceptionDelegate((Exception e) => { }));//Do nothing.
			odThread.Name="ProgressStatusThread_"+eventName;
			odThread.Start(true);
			return new Action(() => {
				//For progress threads, there is a race condition where the DEFCON 1 event will not get processed.
				//This is due to the fact that it took the thread longer to instantiate FormProgressStatus than it took the calling method to invoke this action.
				//Since we don't have access to FormProgressStatus within the thread from here, we simply flag odThread's Tag so that it knows to die.
				if(odThread!=null) {
					odThread.Tag=true;
				}
				//Send the phrase that closes the window in case it is processing events.
				eventType.GetMethod("Fire").Invoke(eventType,new object[] { new ODEventArgs(eventName,"DEFCON 1") });
			});
		}

	}

/*=================================Class DataValid=========================================
===========================================================================================*/

	///<summary>Handles a global event to keep local data synchronized.</summary>
	public class DataValid{

		/*
		///<summary>Triggers an event that causes a signal to be sent to all other computers telling them what kind of locally stored data needs to be updated.  Either supply a set of flags for the types, or supply a date if the appointment screen needs to be refreshed.  Yes, this does immediately refresh the local data, too.  The AllLocal override does all types except appointment date for the local computer only, such as when starting up.</summary>
		public static void SetInvalid(List<int> itypes){
			OnBecameInvalid(new OpenDental.ValidEventArgs(DateTime.MinValue,itypes,false,0));
		}*/

		///<summary>Triggers an event that causes a signal to be sent to all other computers telling them what kind of locally stored data needs to be updated.  Either supply a set of flags for the types, or supply a date if the appointment screen needs to be refreshed.  Yes, this does immediately refresh the local data, too.  The AllLocal override does all types except appointment date for the local computer only, such as when starting up.</summary>
		public static void SetInvalid(params InvalidType[] arrayITypes) {
			FormOpenDental.S_DataValid_BecomeInvalid(new ValidEventArgs(DateTime.MinValue,arrayITypes,false,0));
		}

		///<summary>Triggers an event that causes a signal to be sent to all other computers telling them what kind of locally stored data needs to be updated.  Either supply a set of flags for the types, or supply a date if the appointment screen needs to be refreshed.  Yes, this does immediately refresh the local data, too.  The AllLocal override does all types except appointment date for the local computer only, such as when starting up.</summary>
		public static void SetInvalid(bool onlyLocal) {
			FormOpenDental.S_DataValid_BecomeInvalid(new ValidEventArgs(DateTime.MinValue,new[] { InvalidType.AllLocal },true,0));
		}

		public static void SetInvalidTask(long taskNum,bool isPopup) {
			List<InvalidType> listITypes=new List<InvalidType>();
			if(isPopup){
				listITypes.Add(InvalidType.TaskPopup);
				//we also need to tell the database about all the users with unread tasks
				TaskUnreads.AddUnreads(taskNum,Security.CurUser.UserNum);
			}
			else{
				listITypes.Add(InvalidType.Task);
			}
			FormOpenDental.S_DataValid_BecomeInvalid(new ValidEventArgs(DateTime.MinValue,listITypes.ToArray(),false,taskNum));
		}

	}

	///<summary></summary>
	public delegate void ValidEventHandler(ValidEventArgs e);

	///<summary></summary>
	public class ValidEventArgs : System.EventArgs{
		private DateTime dateViewing;
		private InvalidType[] itypes;
		private bool onlyLocal;
		private long taskNum;
		
		///<summary></summary>
		public ValidEventArgs(DateTime dateViewing,InvalidType[] itypes,bool onlyLocal,long taskNum)
			: base() {
			this.dateViewing=dateViewing;
			this.itypes=itypes;
			this.onlyLocal=onlyLocal;
			this.taskNum=taskNum;
		}

		///<summary></summary>
		public DateTime DateViewing{
			get{return dateViewing;}
		}

		///<summary></summary>
		public InvalidType[] ITypes {
			get{return itypes;}
		}

		///<summary></summary>
		public bool OnlyLocal{
			get{return onlyLocal;}
		}

		///<summary></summary>
		public long TaskNum {
			get{return taskNum;}
		}

	}

	/*=================================Class GotoModule==================================================
	===========================================================================================*/

	///<summary>Used to trigger a global event to jump between modules and perform actions in other modules.  PatNum is optional.  If 0, then no effect.</summary>
	public class GotoModule{

		/*
		///<summary>This triggers a global event which the main form responds to by going directly to a module.</summary>
		public static void GoNow(DateTime dateSelected,Appointment pinAppt,int selectedAptNum,int iModule,int claimNum) {
			OnModuleSelected(new ModuleEventArgs(dateSelected,pinAppt,selectedAptNum,iModule,claimNum));
		}*/

		///<summary>Goes directly to an existing appointment.</summary>
		public static void GotoAppointment(DateTime dateSelected,long selectedAptNum) {
			OnModuleSelected(new ModuleEventArgs(dateSelected,new List<long>(),selectedAptNum,0,0,0,0));
		}

		///<summary>Goes directly to a claim in someone's Account.</summary>
		public static void GotoClaim(long claimNum) {
			OnModuleSelected(new ModuleEventArgs(DateTime.MinValue,new List<long>(),0,2,claimNum,0,0));
		}

		///<summary>Goes directly to an Account.  Sometimes, patient is selected some other way instead of being passed in here, so OK to pass in a patNum of zero.</summary>
		public static void GotoAccount(long patNum) {
			OnModuleSelected(new ModuleEventArgs(DateTime.MinValue,new List<long>(),0,2,0,patNum,0));
		}
		
		///<summary>Goes directly to Family module.  Sometimes, patient is selected some other way instead of being passed in here, so OK to pass in a patNum of zero.</summary>
		public static void GotoFamily(long patNum) {
			OnModuleSelected(new ModuleEventArgs(DateTime.MinValue,new List<long>(),0,1,0,patNum,0));
		}

		///<summary>Goes directly to TP module.  Sometimes, patient is selected some other way instead of being passed in here, so OK to pass in a patNum of zero.</summary>
		public static void GotoTreatmentPlan(long patNum) {
			OnModuleSelected(new ModuleEventArgs(DateTime.MinValue,new List<long>(),0,3,0,patNum,0));
		}

		public static void GotoChart(long patNum){
			OnModuleSelected(new ModuleEventArgs(DateTime.MinValue, new List<long>(), 0, 4, 0, patNum, 0));
		}

		public static void GotoManage(long patNum){
			OnModuleSelected(new ModuleEventArgs(DateTime.MinValue, new List<long>(), 0, 6, 0, patNum, 0));
		}

		///<summary>Puts appointment on pinboard, then jumps to Appointments module.
		///Sometimes, patient is selected some other way instead of being passed in here, so OK to pass in a patNum of zero.</summary>
		public static void PinToAppt(List<long> pinAptNums,long patNum) {
			OnModuleSelected(new ModuleEventArgs(DateTime.Today,pinAptNums,0,0,0,patNum,0));
		}

		///<summary>Jumps to Images module and pulls up the specified image.</summary>
		public static void GotoImage(long patNum,long docNum) {
			OnModuleSelected(new ModuleEventArgs(DateTime.MinValue,new List<long>(),0,5,0,patNum,docNum));
		}

		///<summary></summary>
		protected static void OnModuleSelected(ModuleEventArgs e){
			FormOpenDental.S_GotoModule_ModuleSelected(e);
		}
	}

	///<summary></summary>
	public class ModuleEventArgs : System.EventArgs{
		private DateTime dateSelected;
		private List<long> listPinApptNums;
		private long selectedAptNum;
		private int iModule;
		private long claimNum;
		private long patNum;
		private long docNum;//image

		///<summary></summary>
		public ModuleEventArgs(DateTime dateSelected,List<long> listPinApptNums,long selectedAptNum,int iModule,
			long claimNum,long patNum,long docNum)
			: base()
		{
			this.dateSelected=dateSelected;
			this.listPinApptNums=listPinApptNums;
			this.selectedAptNum=selectedAptNum;
			this.iModule=iModule;
			this.claimNum=claimNum;
			this.patNum=patNum;
			this.docNum=docNum;
		}

		///<summary>If going to the ApptModule, this lets you pick a date.</summary>
		public DateTime DateSelected{
			get{return dateSelected;}
		}

		///<summary>The aptNums of the appointments that we want to put on the pinboard of the Apt Module.</summary>
		public List<long> ListPinApptNums {
			get{return listPinApptNums;}
		}

		///<summary></summary>
		public long SelectedAptNum {
			get{return selectedAptNum;}
		}

		///<summary></summary>
		public int IModule{
			get{return iModule;}
		}

		///<summary>If going to Account module, this lets you pick a claim.</summary>
		public long ClaimNum {
			get{return claimNum;}
		}

		///<summary></summary>
		public long PatNum {
			get { return patNum; }
		}

		///<summary>If going to Images module, this lets you pick which image.</summary>
		public long DocNum {
			get { return docNum; }
		}
	}

	///<summary>Used to log where a backup was initiated from.
	///These enum values are named in a way so that they sound good at the end of this sentence:
	///"A backup was created when running the [enumValHere]"
	///</summary>
	public enum BackupLocation {
		ConvertScript,
		DatabaseMaintenanceTool,
		OptimizeTool,
		InnoDbTool
	}


	

}