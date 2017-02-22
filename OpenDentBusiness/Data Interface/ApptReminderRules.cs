using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Globalization;
using CodeBase;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace OpenDentBusiness{

	///<summary></summary>
	public class ApptReminderRules{

		///<summary>If true, then </summary>
		public static bool IsReminders {
			get {
				return PrefC.GetBool(PrefName.ApptRemindAutoEnabled) && Db.GetLong("SELECT count(*) FROM ApptReminderRule WHERE TSPrior>0")>0;
			}
		}

		///<summary>Gets all, sorts by TSPrior Desc, Should never be more than 3 (per clinic if this is implemented for clinics.)</summary>
		public static List<ApptReminderRule> GetAll(){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<ApptReminderRule>>(MethodBase.GetCurrentMethod());
			}
			string command="SELECT * FROM apptreminderrule ";
			return Crud.ApptReminderRuleCrud.SelectMany(command).OrderByDescending(x => new[] { 1,2,0 }.ToList().IndexOf((int)x.TypeCur)).ToList();
		}

		///<summary>16.3.29 is more strict about reminder rule setup. Prompt the user and allow them to exit the update if desired. Get all currently enabled reminder rules.
		///Returns 2 element list of bool. 
		///[0] indicates if any single clinic/practice has more than 1 same day reminder. 
		///[1] indicates if any single clinic/practice has more than 1 future day reminder.</summary>
		public static List<bool> Get_16_3_29_ConversionFlags() {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<bool>>(System.Reflection.MethodBase.GetCurrentMethod());
			}
			//We can't use CRUD here as we may be in between versions so use DataTable directly.
			string command="SELECT ApptReminderRuleNum, TypeCur, TSPrior, ClinicNum FROM apptreminderrule WHERE TypeCur=0";
			var groups=Db.GetTable(command).Select().Select(x => new {
				ApptReminderRuleNum = PIn.Long(x[0].ToString()),
				TypeCur = PIn.Int(x[1].ToString()),
				TSPrior = TimeSpan.FromTicks(PIn.Long(x[2].ToString())),
				ClinicNum = PIn.Long(x[3].ToString())
			})
			//All rules grouped by clinic and whether they are same day or future day.
			.GroupBy(x => new { ClincNum=x.ClinicNum, IsSameDay=x.TSPrior.TotalDays<1 });
			return new List<bool>() {
				//Any 1 single clinic has more than 1 same day reminder.
				groups.Any(x => x.Key.IsSameDay && x.Count()>1),
				//Any 1 single clinic has more than 1 future day reminder.
				groups.Any(x => !x.Key.IsSameDay && x.Count()>1)
			};
		}

		public static List<ApptReminderRule> GetDefaults() {
			return GetForClinic(0);
		}

		///<summary>Gets all from the DB, sorts by TSPrior Desc, Should never be more than 3 (per clinic if this is implemented for clinics.)</summary>
		public static List<ApptReminderRule> GetForClinic(long clinicNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<ApptReminderRule>>(MethodBase.GetCurrentMethod(),clinicNum);
			}
			string command = "SELECT * FROM apptreminderrule WHERE ClinicNum="+POut.Long(clinicNum);
			return Crud.ApptReminderRuleCrud.SelectMany(command).OrderByDescending(x => x.TSPrior.TotalMinutes).ToList();
		}
		
		public static void SyncByClinic(List<ApptReminderRule> listNew, long ClinicNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),listNew,ClinicNum);
				return;
			}
			List<ApptReminderRule> listOld = ApptReminderRules.GetForClinic(ClinicNum);//ClinicNum can be 0
			Crud.ApptReminderRuleCrud.Sync(listNew,listOld);
		}

		///<summary>Gets one ApptReminderRule from the db.</summary>
		public static ApptReminderRule GetOne(long apptReminderRuleNum){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				return Meth.GetObject<ApptReminderRule>(MethodBase.GetCurrentMethod(),apptReminderRuleNum);
			}
			return Crud.ApptReminderRuleCrud.SelectOne(apptReminderRuleNum);
		}

		///<summary></summary>
		public static long Insert(ApptReminderRule apptReminderRule){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				apptReminderRule.ApptReminderRuleNum=Meth.GetLong(MethodBase.GetCurrentMethod(),apptReminderRule);
				return apptReminderRule.ApptReminderRuleNum;
			}
			return Crud.ApptReminderRuleCrud.Insert(apptReminderRule);
		}

		///<summary></summary>
		public static void Update(ApptReminderRule apptReminderRule){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				Meth.GetVoid(MethodBase.GetCurrentMethod(),apptReminderRule);
				return;
			}
			Crud.ApptReminderRuleCrud.Update(apptReminderRule);
		}

		///<summary></summary>
		public static void Delete(long apptReminderRuleNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),apptReminderRuleNum);
				return;
			}
			Crud.ApptReminderRuleCrud.Delete(apptReminderRuleNum);
		}

		///<summary>Update Appointment.Confirmed. Returns true if update was allowed. Returns false if it was prevented.</summary>
		public static bool UpdateAppointmentConfirmationStatus(long aptNum,long confirmDefNum,string commaListOfExcludedDefNums) {
			Appointment aptCur=Appointments.GetOneApt(aptNum);
			if(aptCur==null) {
				return false;
			}
			List<long> preventChangeFrom=commaListOfExcludedDefNums.Split(new char[] { ',' },StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList();
			if(preventChangeFrom.Contains(aptCur.Confirmed)) { //This appointment is in a confirmation state that can no longer be updated.
				return false;
			}
			//Keep the update small.
			Appointment aptOld=aptCur.Copy();
			aptCur.Confirmed=confirmDefNum;
			Appointments.Update(aptCur,aptOld);
			//No need for aptOld in this signal.
			Signalods.SetInvalidAppt(aptCur);
			SecurityLogs.MakeLogEntry(Permissions.ApptConfirmStatusEdit,aptCur.PatNum,"Appointment confirmation status changed from "
				+DefC.GetName(DefCat.ApptConfirmed,aptOld.Confirmed)+" to "+DefC.GetName(DefCat.ApptConfirmed,aptCur.Confirmed)
				+" due to an eConfirmation.",aptCur.AptNum,LogSources.AutoConfirmations);
			return true;			
		}

		public static ApptReminderRule CreateDefaultReminderRule(ApptReminderType ruleType,long clinicNum = 0) {
			switch(ruleType) {
				case ApptReminderType.ReminderSameDay:
					return new ApptReminderRule() {
						ClinicNum=clinicNum,//works with practice too because _listClinics[0] is a spoofed "Practice/Defaults" clinic with ClinicNum=0
						TypeCur=ApptReminderType.ReminderSameDay,
						TSPrior=TimeSpan.FromHours(3),
						TemplateSMS="Appointment Reminder: [NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]. If you have questions call [ClinicPhone].",//default message
						TemplateEmail="Appointment Reminder: [NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]. If you have questions call [ClinicPhone].",//default message
						TemplateEmailSubject="Appointment Reminder",//default subject
						TemplateSMSAggShared="Appointment Reminder:\n[Appts]\nIf you have questions call [ClinicPhone].",
						TemplateSMSAggPerAppt="[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName].",
						TemplateEmailSubjAggShared="Appointment Reminder",
						TemplateEmailAggShared="Appointment Reminder:\r\n[Appts]\r\nIf you have questions call [ClinicPhone].",
						TemplateEmailAggPerAppt="[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName].",
						//SendOrder="0,1,2" //part of ctor
					};
				case ApptReminderType.ReminderFutureDay:
					return new ApptReminderRule() {
						ClinicNum=clinicNum,//works with practice too because _listClinics[0] is a spoofed "Practice/Defaults" clinic with ClinicNum=0
						TypeCur=ApptReminderType.ReminderFutureDay,
						TSPrior=TimeSpan.FromDays(3),
						TemplateSMS="Appointment Reminder: [NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]. If you have questions call [ClinicPhone].",//default message
						TemplateEmail="Appointment Reminder: [NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]. If you have questions call [ClinicPhone].",//default message
						TemplateEmailSubject="Appointment Reminder",//default subject
						TemplateSMSAggShared="Appointment Reminder:\n[Appts]\nIf you have questions call [ClinicPhone].",
						TemplateSMSAggPerAppt="[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName].",
						TemplateEmailSubjAggShared="Appointment Reminder",
						TemplateEmailAggShared="Appointment Reminder:\r\n[Appts]\r\nIf you have questions call [ClinicPhone].",
						TemplateEmailAggPerAppt="[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName].",
						//SendOrder="0,1,2" //part of ctor
					};
				case ApptReminderType.ConfirmationFutureDay:
					return new ApptReminderRule() {
						ClinicNum=clinicNum,//works with practice too because _listClinics[0] is a spoofed "Practice/Defaults" clinic with ClinicNum=0
						TypeCur=ApptReminderType.ConfirmationFutureDay,
						TSPrior=TimeSpan.FromDays(7),
						TemplateSMS="Appointment Confirmation: [NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]. Reply [ConfirmCode] to confirm or go to [ConfirmURL] for confirmation options, or call [ClinicPhone].",//default message
						TemplateEmail="Appointment Confirmation: [NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName]. Goto [ConfirmURL] for confirmation options, or call [ClinicPhone].",//default message
						TemplateEmailSubject="Appointment Confirmation",//default subject
						TemplateSMSAggShared="Appointment Confirmation:\n[Appts]\nGoto [ConfirmURL] for confirmation options, or call [ClinicPhone].",
						TemplateSMSAggPerAppt="[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName].",
						TemplateEmailSubjAggShared="Appointment Confirmation",
						TemplateEmailAggShared="Appointment Confirmation:\r\n[Appts]\r\nGoto [ConfirmURL] for confirmation options, or call [ClinicPhone].",
						TemplateEmailAggPerAppt="[NameF] is scheduled for [ApptTime] on [ApptDate] at [ClinicName].",
						//SendOrder="0,1,2" //part of ctor
					};
			}
			return null;
		}

		public static List<string> GetAvailableTags(ApptReminderType type) {
			List<string> retVal = new List<string>() {
				"[NameF]",
				"[ApptTime]",
				"[ApptTimeAskedArrive]",
				"[ApptDate]",
				"[ClinicName]",
				"[ClinicPhone]",
				"[ProvName]",
				"[ProvAbbr]",
				"[PracticeName]",
				"[PracticePhone]"
			};
			if(type==ApptReminderType.ConfirmationFutureDay) {
				retVal.AddRange(new[] {
					"[ConfirmCode]",
					"[ConfirmURL]"
				});
			}
			retVal.Sort();//alphabetical
			return retVal;
		}	
	}
}