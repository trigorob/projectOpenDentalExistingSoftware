using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDentBusiness {
	public class DisplayFields {

		public static DataTable RefreshCache() {
			//No need to check RemotingRole; Calls GetTableRemotelyIfNeeded().
			string command = "SELECT * FROM displayfield ORDER BY ItemOrder";
			DataTable table=Cache.GetTableRemotelyIfNeeded(MethodBase.GetCurrentMethod(),command);
			table.TableName="DisplayField";
			FillCache(table);
			return table;
		}

		public static void FillCache(DataTable table){
			//No need to check RemotingRole; no call to db.
			DisplayFieldC.Listt=Crud.DisplayFieldCrud.TableToList(table);
		}

		///<summary></summary>
		public static long Insert(DisplayField field) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetLong(MethodBase.GetCurrentMethod(),field);
			}
			return Crud.DisplayFieldCrud.Insert(field);
		}

		///<summary></summary>
		public static void Update(DisplayField field) {			
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),field);
				return;
			}
			Crud.DisplayFieldCrud.Update(field);
		}

		///<summary></summary>
		public static void Delete(long displayFieldNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),displayFieldNum);
				return;
			}
			string command="DELETE FROM displayfield WHERE DisplayFieldNum = "+POut.Long(displayFieldNum);
			Db.NonQ(command);
		}

		///<summary></summary>
		public static void DeleteForChartView(long chartViewNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),chartViewNum);
				return;
			}
			string command="DELETE FROM displayfield WHERE ChartViewNum = "+POut.Long(chartViewNum);
			Db.NonQ(command);
		}

		///<Summary>Returns an ordered list for just one category.  Do not use with None, or it will malfunction.  These are display fields that the user has entered, which are stored in the db, and then are pulled into the cache.  Categories with no display fields will return the default list.</Summary>
		public static List<DisplayField> GetForCategory(DisplayFieldCategory category){
			//No need to check RemotingRole; no call to db.
			List<DisplayField> retVal=new List<DisplayField>();
			for(int i=0;i<DisplayFieldC.Listt.Count;i++){
				if(DisplayFieldC.Listt[i].Category==category){
					retVal.Add(DisplayFieldC.Listt[i].Copy());
				}
			}
			if(retVal.Count==0) {//default
				return DisplayFields.GetDefaultList(category);
			}
			return retVal;
		}

		///<Summary>Returns an ordered list for just one chart view</Summary>
		public static List<DisplayField> GetForChartView(long ChartViewNum) {
			//No need to check RemotingRole; no call to db.
			List<DisplayField> retVal=new List<DisplayField>();
			for(int i=0;i<DisplayFieldC.Listt.Count;i++) {
				if(DisplayFieldC.Listt[i].ChartViewNum==ChartViewNum && DisplayFieldC.Listt[i].Category==DisplayFieldCategory.None) {
					retVal.Add(DisplayFieldC.Listt[i].Copy());
				}
			}
			if(retVal.Count==0) {//default
				return DisplayFields.GetDefaultList(DisplayFieldCategory.None);
			}
			return retVal;
		}

		public static List<DisplayField> GetDefaultList(DisplayFieldCategory category){
			//No need to check RemotingRole; no call to db.
			List<DisplayField> list=new List<DisplayField>();
			switch (category) {
				#region 'None'
				case DisplayFieldCategory.None:
					list.Add(new DisplayField("Date",67,category));
					//list.Add(new DisplayField("Time",40));
					list.Add(new DisplayField("Th",27,category));
					list.Add(new DisplayField("Surf",40,category));
					list.Add(new DisplayField("Dx",28,category));
					list.Add(new DisplayField("Description",218,category));
					list.Add(new DisplayField("Stat",25,category));
					list.Add(new DisplayField("Prov",42,category));
					list.Add(new DisplayField("Amount",48,category));
					list.Add(new DisplayField("ADA Code",62,category));
					list.Add(new DisplayField("User",62,category));
					list.Add(new DisplayField("Signed",55,category));
					//list.Add(new DisplayField("Priority",65,category));
					//list.Add(new DisplayField("Date TP",67,category));
					//list.Add(new DisplayField("Date Entry",67,category));
					//list.Add(new DisplayField("Prognosis",60,category));
					//list.Add(new DisplayField("Length",40,category));
					//list.Add(new DisplayField("Abbr",50,category));
					//list.Add(new DisplayField("Locked",50,category));
					//list.Add(new DisplayField("HL7 Sent",60,category));
					//if(Programs.UsingOrion){
						//list.Add(new DisplayField("DPC",33,category));
						//list.Add(new DisplayField("Schedule By",72,category));
						//list.Add(new DisplayField("Stop Clock",67,category));
						//list.Add(new DisplayField("Stat 2",36,category));
						//list.Add(new DisplayField("On Call",45,category));
						//list.Add(new DisplayField("Effective Comm",90,category));
						//list.Add(new DisplayField("End Time",56,category));
						//list.Add(new DisplayField("Quadrant",55,category));
						//list.Add(new DisplayField("DPCpost",52,category));
					//}
					break;
				#endregion 'None'
				#region PatientSelect
				case DisplayFieldCategory.PatientSelect:
					list.Add(new DisplayField("LastName",75,category));
					list.Add(new DisplayField("First Name",75,category));
					//list.Add(new DisplayField("MI",25,category));
					list.Add(new DisplayField("Pref Name",60,category));
					list.Add(new DisplayField("Age",30,category));
					list.Add(new DisplayField("SSN",65,category));
					list.Add(new DisplayField("Hm Phone",90,category));
					list.Add(new DisplayField("Wk Phone",90,category));
					if(PrefC.GetBool(PrefName.DistributorKey)) {//if for OD HQ
						//list.Add(new DisplayField("OtherPhone",90,category));
						//list.Add(new DisplayField("Country",90,category));
						//list.Add(new DisplayField("RegKey",150,category));
					}
					list.Add(new DisplayField("PatNum",80,category));
					//list.Add(new DisplayField("ChartNum",60,category));
					list.Add(new DisplayField("Address",100,category));
					list.Add(new DisplayField("Status",65,category));
					//list.Add(new DisplayField("Bill Type",90,category));
					//list.Add(new DisplayField("City",80,category));
					//list.Add(new DisplayField("State",55,category));
					//list.Add(new DisplayField("Pri Prov",85,category));
					//list.Add(new DisplayField("Birthdate",70,category));
					//list.Add(new DisplayField("Site",90,category));
					//list.Add(new DisplayField("Email",90,category));
					//list.Add(new DisplayField("Clinic",90,category));
					//list.Add(new DisplayField("Wireless Ph",90,category));
					//list.Add(new DisplayField("Sec Prov",85,category));
					//list.Add(new DisplayField("LastVisit",70,category));
					//list.Add(new DisplayField("NextVisit",70,category));
					break;
				#endregion PatientSelect
				#region PatientInformation
				case DisplayFieldCategory.PatientInformation:
					list.Add(new DisplayField("Last",0,category));
					list.Add(new DisplayField("First",0,category));
					list.Add(new DisplayField("Middle",0,category));
					list.Add(new DisplayField("Preferred",0,category));
					list.Add(new DisplayField("Title",0,category));
					list.Add(new DisplayField("Salutation",0,category));
					list.Add(new DisplayField("Status",0,category));
					list.Add(new DisplayField("Gender",0,category));
					list.Add(new DisplayField("Position",0,category));
					list.Add(new DisplayField("Birthdate",0,category));
					list.Add(new DisplayField("Age",0,category));
					list.Add(new DisplayField("SS#",0,category));
					list.Add(new DisplayField("Address",0,category));
					list.Add(new DisplayField("Address2",0,category));
					list.Add(new DisplayField("City",0,category));
					list.Add(new DisplayField("State",0,category));
					if(PrefC.GetBool(PrefName.DockPhonePanelShow)) {
						list.Add(new DisplayField("Country",0,category));
					}
					list.Add(new DisplayField("Zip",0,category));
					list.Add(new DisplayField("Hm Phone",0,category));
					list.Add(new DisplayField("Wk Phone",0,category));
					list.Add(new DisplayField("Wireless Ph",0,category));
					list.Add(new DisplayField("E-mail",0,category));
					list.Add(new DisplayField("Contact Method",0,category));
					list.Add(new DisplayField("ABC0",0,category));
					//list.Add(new DisplayField("Chart Num",0,category));
					list.Add(new DisplayField("Billing Type",0,category));
					//list.Add(new DisplayField("Ward",0,category));
					//list.Add(new DisplayField("AdmitDate",0,category));
					list.Add(new DisplayField("Primary Provider",0,category));
					list.Add(new DisplayField("Sec. Provider",0,category));
					list.Add(new DisplayField("Payor Types",0,category));
					list.Add(new DisplayField("Language",0,category));
					//list.Add(new DisplayField("Clinic",0,category));
					//list.Add(new DisplayField("ResponsParty",0,category));
					list.Add(new DisplayField("Referrals",0,category));
					list.Add(new DisplayField("Addr/Ph Note",0,category));
					list.Add(new DisplayField("PatFields",0,category));
					//list.Add(new DisplayField("Guardians",0,category));
					//list.Add(new DisplayField("Arrive Early",0,category));
					//list.Add(new DisplayField("Super Head",0,category));
					if(PrefC.GetBool(PrefName.DistributorKey)) {
						list.Add(new DisplayField("References",0,category));
					}
					list.Add(new DisplayField("Pat Restrictions",0,category));
					list.Add(new DisplayField("ICE Name",0,category));
					list.Add(new DisplayField("ICE Phone",0,category));
					break;
				#endregion PatientInformation
				#region AccountModule
				case DisplayFieldCategory.AccountModule:
					list.Add(new DisplayField("Date",65,category));
					list.Add(new DisplayField("Patient",100,category));
					list.Add(new DisplayField("Prov",40,category));
					//list.Add(new DisplayField("Clinic",50,category));
					list.Add(new DisplayField("Code",46,category));
					list.Add(new DisplayField("Tth",26,category));
					list.Add(new DisplayField("Description",270,category));
					list.Add(new DisplayField("Charges",60,category));
					list.Add(new DisplayField("Credits",60,category));
					list.Add(new DisplayField("Balance",60,category));
					//list.Add(new DisplayField("Signed",60,category));
					//list.Add(new DisplayField("Abbr",110,category));
					break;
				#endregion AccountModule
				#region RecallList
				case DisplayFieldCategory.RecallList:
					list.Add(new DisplayField("Due Date",75,category));
					list.Add(new DisplayField("Patient",120,category));
					list.Add(new DisplayField("Age",30,category));
					list.Add(new DisplayField("Type",60,category));
					list.Add(new DisplayField("Interval",50,category));
					list.Add(new DisplayField("#Remind",55,category));
					list.Add(new DisplayField("LastRemind",75,category));
					list.Add(new DisplayField("Contact",120,category));
					list.Add(new DisplayField("Status",130,category));
					list.Add(new DisplayField("Note",215,category));
					//list.Add(new DisplayField("BillingType",100,category));
					break;
				#endregion RecallList
				#region ChartPatientInformation
				case DisplayFieldCategory.ChartPatientInformation:
					list.Add(new DisplayField("Age",0,category));
					list.Add(new DisplayField("ABC0",0,category));
					list.Add(new DisplayField("Billing Type",0,category));
					list.Add(new DisplayField("Referred From",0,category));
					list.Add(new DisplayField("Date First Visit",0,category));
					list.Add(new DisplayField("Prov. (Pri, Sec)",0,category));
					list.Add(new DisplayField("Pri Ins",0,category));
					list.Add(new DisplayField("Sec Ins",0,category));
					list.Add(new DisplayField("Payor Types",0,category));
					if(PrefC.GetBool(PrefName.DistributorKey)) {
						list.Add(new DisplayField("Registration Keys",0,category));
						list.Add(new DisplayField("Ehr Provider Keys",0,category));
						list.Add(new DisplayField("References",0,category));
					}
					if(!Programs.UsingEcwTightOrFullMode()) {//different default list for eCW:
						list.Add(new DisplayField("Premedicate",0,category));
						list.Add(new DisplayField("Problems",0,category));
						list.Add(new DisplayField("Med Urgent",0,category));
						list.Add(new DisplayField("Medical Summary",0,category));
						list.Add(new DisplayField("Service Notes",0,category));
						list.Add(new DisplayField("Medications",0,category));
						list.Add(new DisplayField("Allergies",0,category));
						list.Add(new DisplayField("Pat Restrictions",0,category));
					}
					//list.Add(new DisplayField("PatFields",0,category));
					//list.Add(new DisplayField("Birthdate",0,category));
					//list.Add(new DisplayField("City",0,category));
					//list.Add(new DisplayField("AskToArriveEarly",0,category));
					//list.Add(new DisplayField("Super Head",0,category));
					//list.Add(new DisplayField("Patient Portal",0,category));
					//list.Add(new DisplayField("Broken Appts",0,category));
					//if(PrefC.GetBool(PrefName.ShowFeatureEhr)) {
					//	list.Add(new DisplayField("Tobacco Use",0,category));
					//}
					break;
				#endregion ChartPatientInformation
				#region ProcedureGroupNote
				case DisplayFieldCategory.ProcedureGroupNote:
					list.Add(new DisplayField("Date",67,category));
					list.Add(new DisplayField("Th",27,category));
					list.Add(new DisplayField("Surf",40,category));
					list.Add(new DisplayField("Description",203,category));
					list.Add(new DisplayField("Stat",25,category));
					list.Add(new DisplayField("Prov",42,category));
					list.Add(new DisplayField("Amount",48,category));
					list.Add(new DisplayField("ADA Code",62,category));
					//if(Programs.UsingOrion){
					//  list.Add(new DisplayField("Stat 2",36,category));
					//  list.Add(new DisplayField("On Call",45,category));
					//  list.Add(new DisplayField("Effective Comm",90,category));
					//  list.Add(new DisplayField("Repair",45,category));
					//	list.Add(new DisplayField("DPCpost",52,category));
					//}
					break;
				#endregion ProcedureGroupNote
				#region TreatmentPlanModule
				case DisplayFieldCategory.TreatmentPlanModule:
					list.Add(new DisplayField("Done",50,category) {Description="Done"});
					list.Add(new DisplayField("Priority",50,category) {Description="Priority"});
					list.Add(new DisplayField("Tth",40,category) {Description="Tth"});
					list.Add(new DisplayField("Surf",45,category) {Description="Surf"});
					list.Add(new DisplayField("Code",50,category) {Description="Code"});
					list.Add(new DisplayField("Sub",28,category) {Description="Sub"});
					list.Add(new DisplayField("Description",202,category) {Description="Description"});
					list.Add(new DisplayField("Fee",50,category) {Description="Fee"});
					list.Add(new DisplayField("Pri Ins",50,category) {Description="Pri Ins"});
					list.Add(new DisplayField("Sec Ins",50,category) {Description="Sec Ins"});
					list.Add(new DisplayField("Discount",55,category) {Description="Discount"});
					list.Add(new DisplayField("Pat",50,category) {Description="Pat"});
					//list.Add(new DisplayField("Prognosis",60,category){Description="Prognosis"});
					//list.Add(new DisplayField("Dx",28,category){Description="Dx"});
					//list.Add(new DisplayField("Abbr",110,category){Description="Abbr"});
					break;
				#endregion TreatmentPlanModule
				#region OrthoChart
				case DisplayFieldCategory.OrthoChart:
					//Ortho chart has no default columns. User must explicitly set up columns.
					break;
				#endregion OrthoChart
				#region AppointmentBubble
				case DisplayFieldCategory.AppointmentBubble:
					list.Add(new DisplayField("Patient Name",0,category));
					list.Add(new DisplayField("Patient Picture",0,category));
					list.Add(new DisplayField("Appt Day",0,category));
					list.Add(new DisplayField("Appt Date",0,category));
					list.Add(new DisplayField("Appt Time",0,category));
					list.Add(new DisplayField("Appt Length",0,category));
					list.Add(new DisplayField("Provider",0,category));
					list.Add(new DisplayField("Production",0,category));
					list.Add(new DisplayField("Confirmed",0,category));
					list.Add(new DisplayField("Appt Status",0,category));
					list.Add(new DisplayField("Med Flag",0,category));
					list.Add(new DisplayField("Med Note",0,category));
					list.Add(new DisplayField("Lab",0,category));
					list.Add(new DisplayField("Procedures",0,category));
					list.Add(new DisplayField("Note",0,category));
					list.Add(new DisplayField("Horizontal Line",0,category));
					list.Add(new DisplayField("PatNum",0,category));
					list.Add(new DisplayField("ChartNum",0,category));
					list.Add(new DisplayField("Billing Type",0,category));
					list.Add(new DisplayField("Age",0,category));
					list.Add(new DisplayField("Home Phone",0,category));
					list.Add(new DisplayField("Work Phone",0,category));
					list.Add(new DisplayField("Wireless Phone",0,category));
					list.Add(new DisplayField("Contact Methods",0,category));
					list.Add(new DisplayField("Insurance",0,category));
					list.Add(new DisplayField("Address Note",0,category));
					list.Add(new DisplayField("Fam Note",0,category));
					list.Add(new DisplayField("Appt Mod Note",0,category));
					//list.Add(new DisplayField("ReferralFrom",0,category));
					//list.Add(new DisplayField("ReferralTo",0,category));
					//list.Add(new DisplayField("Language",0,category));
					//list.Add(new DisplayField("Email",0,category));		
					break;
				#endregion AppointmentBubble
				#region AccountPatientInformation
				case DisplayFieldCategory.AccountPatientInformation:
					//AccountPatientInformation has no default columns.  User must explicitly set up columns.
					//list.Add(new DisplayField("Billing Type",0,category));
					//list.Add(new DisplayField("PatFields",0,category));
					break;
				#endregion AccountPatientInformation
				#region StatementMainGrid
				case DisplayFieldCategory.StatementMainGrid:
					int i=0;
					list.Add(new DisplayField {Category=category,InternalName="date",Description="Date",ColumnWidth=75,ItemOrder=++i});
					list.Add(new DisplayField {Category=category,InternalName="patient",Description="Patient",ColumnWidth=100,ItemOrder=++i});
					list.Add(new DisplayField {Category=category,InternalName="ProcCode",Description="Code",ColumnWidth=45,ItemOrder=++i});
					list.Add(new DisplayField {Category=category,InternalName="tth",Description="Tooth",ColumnWidth=45,ItemOrder=++i});
					list.Add(new DisplayField {Category=category,InternalName="description",Description="Description",ColumnWidth=275,ItemOrder=++i});
					list.Add(new DisplayField {Category=category,InternalName="charges",Description="Charges",ColumnWidth=60,ItemOrder=++i});
					list.Add(new DisplayField {Category=category,InternalName="credits",Description="Credits",ColumnWidth=60,ItemOrder=++i});
					list.Add(new DisplayField {Category=category,InternalName="balance",Description="Balance",ColumnWidth=60,ItemOrder=++i});
					break;
				#endregion StatementMainGrid
				#region FamilyRecallGrid
				case DisplayFieldCategory.FamilyRecallGrid:
					list.Add(new DisplayField("Type",90,category));
					list.Add(new DisplayField("Due Date",80,category));
					list.Add(new DisplayField("Sched Date",80,category));
					list.Add(new DisplayField("Notes",255,category));
					//list.Add(new DisplayField("Previous Date",90,category));
					//list.Add(new DisplayField("Interval",80,category));
					break;
				#endregion FamilyRecallGrid
				#region AppointmentEdit
				case DisplayFieldCategory.AppointmentEdit:
					list.Add(new DisplayField("Stat",35,category));
					list.Add(new DisplayField("Priority",45,category));
					if(Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)) {
						list.Add(new DisplayField("Code",125,category));
					}
					else {
						list.Add(new DisplayField("Tth",25,category));
						list.Add(new DisplayField("Surf",50,category));
						list.Add(new DisplayField("Code",50,category));
					}
					list.Add(new DisplayField("Description",275,category));
					list.Add(new DisplayField("Fee",60,category));
					break;
				#endregion AppointmentEdit
				#region PlannedAppointmentEdit
				case DisplayFieldCategory.PlannedAppointmentEdit:
					list.Add(new DisplayField("Stat",35,category));
					list.Add(new DisplayField("Priority",45,category));
					if(Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)) {
						list.Add(new DisplayField("Code",125,category));
					}
					else {
						list.Add(new DisplayField("Tth",25,category));
						list.Add(new DisplayField("Surf",50,category));
						list.Add(new DisplayField("Code",50,category));
					}
					list.Add(new DisplayField("Description",275,category));
					list.Add(new DisplayField("Fee",60,category));
					break;
				#endregion PlannedAppointmentEdit
				default:
					break;
			}
			return list;
		}

		public static List<DisplayField> GetAllAvailableList(DisplayFieldCategory category){
			//No need to check RemotingRole; no call to db. 
			List<DisplayField> list=new List<DisplayField>();
			switch (category) {
				#region 'None'
				case DisplayFieldCategory.None://Currently only used for ChartViews
					list.Add(new DisplayField("Date",67,category));
					list.Add(new DisplayField("Time",40,category));
					list.Add(new DisplayField("Th",27,category));
					list.Add(new DisplayField("Surf",40,category));
					list.Add(new DisplayField("Dx",28,category));
					list.Add(new DisplayField("Description",218,category));
					list.Add(new DisplayField("Stat",25,category));
					list.Add(new DisplayField("Prov",42,category));
					list.Add(new DisplayField("Amount",48,category));
					list.Add(new DisplayField("ADA Code",62,category));
					list.Add(new DisplayField("User",62,category));
					list.Add(new DisplayField("Signed",55,category));
					list.Add(new DisplayField("Priority",44,category));
					list.Add(new DisplayField("Date TP",67,category));
					list.Add(new DisplayField("Date Entry",67,category));
					list.Add(new DisplayField("Prognosis",60,category));
					list.Add(new DisplayField("Length",40,category));
					list.Add(new DisplayField("Abbr",50,category));
					list.Add(new DisplayField("Locked",50,category));
					list.Add(new DisplayField("HL7 Sent",60,category));
					if(Programs.UsingOrion){
						list.Add(new DisplayField("DPC",33,category));
						list.Add(new DisplayField("Schedule By",72,category));
						list.Add(new DisplayField("Stop Clock",67,category));
						list.Add(new DisplayField("Stat 2",36,category));
						list.Add(new DisplayField("On Call",45,category));
						list.Add(new DisplayField("Effective Comm",90,category));
						list.Add(new DisplayField("End Time",56,category));//not visible unless orion
						list.Add(new DisplayField("Quadrant",55,category));//behavior is specific to orion
						list.Add(new DisplayField("DPCpost",52,category));
					}
					break;
				#endregion 'None'
				#region PatientSelect
				case DisplayFieldCategory.PatientSelect:
					list.Add(new DisplayField("LastName",75,category));
					list.Add(new DisplayField("First Name",75,category));
					list.Add(new DisplayField("MI",25,category));
					list.Add(new DisplayField("Pref Name",60,category));
					list.Add(new DisplayField("Age",30,category));
					list.Add(new DisplayField("SSN",65,category));
					list.Add(new DisplayField("Hm Phone",90,category));
					list.Add(new DisplayField("Wk Phone",90,category));
					if(PrefC.GetBool(PrefName.DistributorKey)) {//if for OD HQ
						list.Add(new DisplayField("OtherPhone",90,category));
						list.Add(new DisplayField("Country",90,category));
						list.Add(new DisplayField("RegKey",150,category));
					}
					list.Add(new DisplayField("PatNum",80,category));
					list.Add(new DisplayField("ChartNum",60,category));
					list.Add(new DisplayField("Address",100,category));
					list.Add(new DisplayField("Status",65,category));
					list.Add(new DisplayField("Bill Type",90,category));
					list.Add(new DisplayField("City",80,category));
					list.Add(new DisplayField("State",55,category));
					list.Add(new DisplayField("Pri Prov",85,category));
					list.Add(new DisplayField("Birthdate",70,category));
					list.Add(new DisplayField("Site",90,category));
					list.Add(new DisplayField("Email",90,category));
					list.Add(new DisplayField("Clinic",90,category));
					list.Add(new DisplayField("Wireless Ph",90,category));
					list.Add(new DisplayField("Sec Prov",85,category));
					list.Add(new DisplayField("LastVisit",70,category));
					list.Add(new DisplayField("NextVisit",70,category));
					break;
				#endregion PatientSelect
				#region PatientInformation
				case DisplayFieldCategory.PatientInformation:
					list.Add(new DisplayField("Last",0,category));
					list.Add(new DisplayField("First",0,category));
					list.Add(new DisplayField("Middle",0,category));
					list.Add(new DisplayField("Preferred",0,category));
					list.Add(new DisplayField("Title",0,category));
					list.Add(new DisplayField("Salutation",0,category));
					list.Add(new DisplayField("Status",0,category));
					list.Add(new DisplayField("Gender",0,category));
					list.Add(new DisplayField("Position",0,category));
					list.Add(new DisplayField("Birthdate",0,category));
					list.Add(new DisplayField("Age",0,category));
					list.Add(new DisplayField("SS#",0,category));
					list.Add(new DisplayField("Address",0,category));
					list.Add(new DisplayField("Address2",0,category));
					list.Add(new DisplayField("City",0,category));
					list.Add(new DisplayField("State",0,category));
					if(PrefC.GetBool(PrefName.DockPhonePanelShow)) {
						list.Add(new DisplayField("Country",0,category));
					}
					list.Add(new DisplayField("Zip",0,category));
					list.Add(new DisplayField("Hm Phone",0,category));
					list.Add(new DisplayField("Wk Phone",0,category));
					list.Add(new DisplayField("Wireless Ph",0,category));
					list.Add(new DisplayField("E-mail",0,category));
					list.Add(new DisplayField("Contact Method",0,category));
					list.Add(new DisplayField("ABC0",0,category));
					list.Add(new DisplayField("Chart Num",0,category));
					list.Add(new DisplayField("Billing Type",0,category));
					list.Add(new DisplayField("Ward",0,category));
					list.Add(new DisplayField("AdmitDate",0,category));
					list.Add(new DisplayField("Primary Provider",0,category));
					list.Add(new DisplayField("Sec. Provider",0,category));
					list.Add(new DisplayField("Payor Types",0,category));
					list.Add(new DisplayField("Language",0,category));
					list.Add(new DisplayField("Clinic",0,category));
					list.Add(new DisplayField("ResponsParty",0,category));
					list.Add(new DisplayField("Referrals",0,category));
					list.Add(new DisplayField("Addr/Ph Note",0,category));
					list.Add(new DisplayField("PatFields",0,category));
					list.Add(new DisplayField("Guardians",0,category));
					list.Add(new DisplayField("Arrive Early",0,category));
					list.Add(new DisplayField("Super Head",0,category));
					if(PrefC.GetBool(PrefName.DistributorKey)) {
						list.Add(new DisplayField("References",0,category));
					}
					list.Add(new DisplayField("Pat Restrictions",0,category));
					list.Add(new DisplayField("ICE Name",0,category));
					list.Add(new DisplayField("ICE Phone",0,category));
					break;
				#endregion PatientInformation
				#region AccountModule
				case DisplayFieldCategory.AccountModule:
					list.Add(new DisplayField("Date",65,category));
					list.Add(new DisplayField("Patient",100,category));
					list.Add(new DisplayField("Prov",40,category));
					list.Add(new DisplayField("Clinic",50,category));
					list.Add(new DisplayField("Code",46,category));
					list.Add(new DisplayField("Tth",26,category));
					list.Add(new DisplayField("Description",270,category));
					list.Add(new DisplayField("Charges",60,category));
					list.Add(new DisplayField("Credits",60,category));
					list.Add(new DisplayField("Balance",60,category));
					list.Add(new DisplayField("Signed",60,category));
					list.Add(new DisplayField("Abbr",110,category));
					break;
				#endregion AccountModule
				#region RecallList
				case DisplayFieldCategory.RecallList:
					list.Add(new DisplayField("Due Date",75,category));
					list.Add(new DisplayField("Patient",120,category));
					list.Add(new DisplayField("Age",30,category));
					list.Add(new DisplayField("Type",60,category));
					list.Add(new DisplayField("Interval",50,category));
					list.Add(new DisplayField("#Remind",55,category));
					list.Add(new DisplayField("LastRemind",75,category));
					list.Add(new DisplayField("Contact",120,category));
					list.Add(new DisplayField("Status",130,category));
					list.Add(new DisplayField("Note",215,category));
					list.Add(new DisplayField("BillingType",100,category));
					break;
				#endregion RecallList
				#region ChartPatientInformation
				case DisplayFieldCategory.ChartPatientInformation:
					list.Add(new DisplayField("Age",0,category));
					list.Add(new DisplayField("ABC0",0,category));
					list.Add(new DisplayField("Billing Type",0,category));
					list.Add(new DisplayField("Referred From",0,category));
					list.Add(new DisplayField("Date First Visit",0,category));
					list.Add(new DisplayField("Prov. (Pri, Sec)",0,category));
					list.Add(new DisplayField("Pri Ins",0,category));
					list.Add(new DisplayField("Sec Ins",0,category));
					list.Add(new DisplayField("Payor Types",0,category));
					if(PrefC.GetBool(PrefName.DistributorKey)) {
						list.Add(new DisplayField("Registration Keys",0,category));
						list.Add(new DisplayField("Ehr Provider Keys",0,category));
						list.Add(new DisplayField("References",0,category));
					}
					list.Add(new DisplayField("Premedicate",0,category));
					list.Add(new DisplayField("Problems",0,category));
					list.Add(new DisplayField("Med Urgent",0,category));
					list.Add(new DisplayField("Medical Summary",0,category));
					list.Add(new DisplayField("Service Notes",0,category));
					list.Add(new DisplayField("Medications",0,category));
					list.Add(new DisplayField("Allergies",0,category));
					list.Add(new DisplayField("PatFields",0,category));
					list.Add(new DisplayField("Birthdate",0,category));
					list.Add(new DisplayField("City",0,category));
					list.Add(new DisplayField("AskToArriveEarly",0,category));
					list.Add(new DisplayField("Super Head",0,category));
					list.Add(new DisplayField("Patient Portal",0,category));
					list.Add(new DisplayField("Broken Appts",0,category));
					if(PrefC.GetBool(PrefName.ShowFeatureEhr)) {
						list.Add(new DisplayField("Tobacco Use",0,category));
					}
					list.Add(new DisplayField("Pat Restrictions",0,category));
					break;
				#endregion ChartPatientInformation
				#region ProcedureGroupNote
				case DisplayFieldCategory.ProcedureGroupNote:
					list.Add(new DisplayField("Date",67,category));
					list.Add(new DisplayField("Th",27,category));
					list.Add(new DisplayField("Surf",40,category));
					list.Add(new DisplayField("Description",218,category));
					list.Add(new DisplayField("Stat",25,category));
					list.Add(new DisplayField("Prov",42,category));
					list.Add(new DisplayField("Amount",48,category));
					list.Add(new DisplayField("ADA Code",62,category));
					if(Programs.UsingOrion){
						list.Add(new DisplayField("Stat 2",36,category));
						list.Add(new DisplayField("On Call",45,category));
						list.Add(new DisplayField("Effective Comm",90,category));
						list.Add(new DisplayField("Repair",45,category));
						list.Add(new DisplayField("DPCpost",52,category));
					}
					break;
				#endregion ProcedureGroupNote
				#region TreatmentPlanModule
				case DisplayFieldCategory.TreatmentPlanModule:
					list.Add(new DisplayField("Done",50,category) {Description="Done"});
					list.Add(new DisplayField("Priority",50,category) {Description="Priority"});
					list.Add(new DisplayField("Tth",40,category) {Description="Tth"});
					list.Add(new DisplayField("Surf",45,category) {Description="Surf"});
					list.Add(new DisplayField("Code",50,category) {Description="Code"});
					list.Add(new DisplayField("Sub",28,category) {Description="Sub"});
					list.Add(new DisplayField("Description",202,category) {Description="Description"});
					list.Add(new DisplayField("Fee",50,category) {Description="Fee"});
					list.Add(new DisplayField("Pri Ins",50,category) {Description="Pri Ins"});
					list.Add(new DisplayField("Sec Ins",50,category) {Description="Sec Ins"});
					list.Add(new DisplayField("Discount",55,category) {Description="Discount"});
					list.Add(new DisplayField("Pat",50,category) {Description="Pat"});
					list.Add(new DisplayField("Prognosis",60,category) {Description="Prognosis"});
					list.Add(new DisplayField("Dx",28,category) {Description="Dx"});
					list.Add(new DisplayField("Abbr",110,category) {Description="Abbr"});
					break;
				#endregion TreatmentPlanModule
				#region OrthoChart
				case DisplayFieldCategory.OrthoChart:
					list=GetForCategory(DisplayFieldCategory.OrthoChart); //The display fields that the user has already saved
					List<string> listDistinctFieldNames=OrthoCharts.GetDistinctFieldNames();
					foreach(string fieldName in listDistinctFieldNames) {
						//If there aren't any display fields with the current field name, create one and add it to the list of available fields.
						if(!list.Any(x => x.Description==fieldName)) {
							DisplayField displayField=new DisplayField("",20,DisplayFieldCategory.OrthoChart);
							displayField.Description=fieldName;
							list.Add(displayField);
						}
					}
					break;
				#endregion OrthoChart
				#region AppointmentBubble
				case DisplayFieldCategory.AppointmentBubble:
					list.Add(new DisplayField("Patient Name",0,category));
					list.Add(new DisplayField("Patient Picture",0,category));
					list.Add(new DisplayField("Appt Day",0,category));
					list.Add(new DisplayField("Appt Date",0,category));
					list.Add(new DisplayField("Appt Time",0,category));
					list.Add(new DisplayField("Appt Length",0,category));
					list.Add(new DisplayField("Provider",0,category));
					list.Add(new DisplayField("Production",0,category));
					list.Add(new DisplayField("Confirmed",0,category));
					list.Add(new DisplayField("Appt Status",0,category));
					list.Add(new DisplayField("Med Flag",0,category));
					list.Add(new DisplayField("Med Note",0,category));
					list.Add(new DisplayField("Lab",0,category));
					list.Add(new DisplayField("Procedures",0,category));
					list.Add(new DisplayField("Note",0,category));
					list.Add(new DisplayField("Horizontal Line",0,category));
					list.Add(new DisplayField("PatNum",0,category));
					list.Add(new DisplayField("ChartNum",0,category));
					list.Add(new DisplayField("Billing Type",0,category));
					list.Add(new DisplayField("Age",0,category));
					list.Add(new DisplayField("Home Phone",0,category));
					list.Add(new DisplayField("Work Phone",0,category));
					list.Add(new DisplayField("Wireless Phone",0,category));
					list.Add(new DisplayField("Contact Methods",0,category));
					list.Add(new DisplayField("Insurance",0,category));
					list.Add(new DisplayField("Address Note",0,category));
					list.Add(new DisplayField("Fam Note",0,category));
					list.Add(new DisplayField("Appt Mod Note",0,category));
					list.Add(new DisplayField("ReferralFrom",0,category));
					list.Add(new DisplayField("ReferralTo",0,category));
					list.Add(new DisplayField("Language",0,category));
					list.Add(new DisplayField("Email",0,category));
					break;
				#endregion AppointmentBubble
				#region AccountPatientInformation
				case DisplayFieldCategory.AccountPatientInformation:
					list.Add(new DisplayField("Billing Type",0,category));
					list.Add(new DisplayField("PatFields",0,category));
					break;
				#endregion AccountPatientInformation
				#region StatementMainGrid
				case DisplayFieldCategory.StatementMainGrid:
					int i=0;
					list.Add(new DisplayField {Category=category,InternalName="date",Description="Date",ColumnWidth=75,ItemOrder=++i});
					list.Add(new DisplayField {Category=category,InternalName="patient",Description="Patient",ColumnWidth=100,ItemOrder=++i});
					list.Add(new DisplayField {Category=category,InternalName="ProcCode",Description="Code",ColumnWidth=45,ItemOrder=++i});
					list.Add(new DisplayField {Category=category,InternalName="tth",Description="Tooth",ColumnWidth=45,ItemOrder=++i});
					list.Add(new DisplayField {Category=category,InternalName="description",Description="Description",ColumnWidth=275,ItemOrder=++i});
					list.Add(new DisplayField {Category=category,InternalName="charges",Description="Charges",ColumnWidth=60,ItemOrder=++i});
					list.Add(new DisplayField {Category=category,InternalName="credits",Description="Credits",ColumnWidth=60,ItemOrder=++i});
					list.Add(new DisplayField {Category=category,InternalName="balance",Description="Balance",ColumnWidth=60,ItemOrder=++i});
					break;
				#endregion StatementMainGrid
				#region FamilyRecallGrid
				case DisplayFieldCategory.FamilyRecallGrid:
					list.Add(new DisplayField("Type",90,category));
					list.Add(new DisplayField("Due Date",80,category));
					list.Add(new DisplayField("Sched Date",80,category));
					list.Add(new DisplayField("Notes",255,category));
					list.Add(new DisplayField("Previous Date",90,category));
					list.Add(new DisplayField("Interval",80,category));
					break;
				#endregion FamilyRecallGrid
				#region AppointmentEdit
				case DisplayFieldCategory.AppointmentEdit:
					list.Add(new DisplayField("Stat",35,category));
					list.Add(new DisplayField("Priority",45,category));
					if(Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)) {
						list.Add(new DisplayField("Code",125,category));
					}
					else {
						list.Add(new DisplayField("Tth",25,category));
						list.Add(new DisplayField("Surf",50,category));
						list.Add(new DisplayField("Code",50,category));
					}
					list.Add(new DisplayField("Description",275,category));
					list.Add(new DisplayField("Fee",60,category));
					break;
				#endregion AppointmentEdit
				#region PlannedAppointmentEdit
				case DisplayFieldCategory.PlannedAppointmentEdit:
					list.Add(new DisplayField("Stat",35,category));
					list.Add(new DisplayField("Priority",45,category));
					if(Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)) {
						list.Add(new DisplayField("Code",125,category));
					}
					else {
						list.Add(new DisplayField("Tth",25,category));
						list.Add(new DisplayField("Surf",50,category));
						list.Add(new DisplayField("Code",50,category));
					}
					list.Add(new DisplayField("Description",275,category));
					list.Add(new DisplayField("Fee",60,category));
					break;
				#endregion PlannedAppointmentEdit
				default:
					break;
			}
			return list;
		}

		public static void SaveListForCategory(List<DisplayField> ListShowing,DisplayFieldCategory category){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),ListShowing,category);
				return;
			}
			bool isDefault=true;
			List<DisplayField> defaultList=GetDefaultList(category);
			if(ListShowing.Count!=defaultList.Count){
				isDefault=false;
			}
			else{
				for(int i=0;i<ListShowing.Count;i++){
					if(ListShowing[i].Description!=""){
						isDefault=false;
						break;
					}
					if(ListShowing[i].InternalName!=defaultList[i].InternalName){
						isDefault=false;
						break;
					}
					if(ListShowing[i].ColumnWidth!=defaultList[i].ColumnWidth) {
						isDefault=false;
						break;
					}
				}
			}
			string command="DELETE FROM displayfield WHERE Category="+POut.Long((int)category);
			Db.NonQ(command);
			if(isDefault){
				return;
			}
			for(int i=0;i<ListShowing.Count;i++){
				ListShowing[i].ItemOrder=i;
				Insert(ListShowing[i]);
			}
		}

		public static void SaveListForChartView(List<DisplayField> ListShowing,long ChartViewNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),ListShowing,ChartViewNum);
				return;
			}
			//Odds are, that if they are creating a custom view, that the fields are not default. If they are default, this code still works.
			string command="DELETE FROM displayfield WHERE ChartViewNum="+POut.Long((long)ChartViewNum);
			Db.NonQ(command);
			for(int i=0;i<ListShowing.Count;i++) {
				ListShowing[i].ItemOrder=i;
				ListShowing[i].ChartViewNum=ChartViewNum;
				Insert(ListShowing[i]);
			}
		}
		

	}
}
