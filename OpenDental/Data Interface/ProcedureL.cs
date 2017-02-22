using System.Collections.Generic;
using System.Linq;
using OpenDentBusiness;

namespace OpenDental {
	public class ProcedureL {
		///<summary>Sets all procedures for apt complete.  Flags procedures as CPOE as needed (when prov logged in).  Makes a log
		///entry for each completed proc.  Then fires the CompleteProcedure automation trigger.</summary>
		public static List<Procedure> SetCompleteInAppt(Appointment apt,List<InsPlan> PlanList,List<PatPlan> patPlans,long siteNum,
			int patientAge,List<InsSub> subList) 
		{
			//Get all procs attached to the appointment and go through the set complete logic.
			//==Andrew 06/09/2016 - We must go through all procedures even if they are already complete so that they get their fields updated.
			//E.g. if the appointment's provnum was changed and then re-completed, the procedures' ProvNums need to get updated in the db here.
			List<Procedure> listProcsInAppt=Procedures.GetProcsForSingle(apt.AptNum,false);
			//Remove already completed procedures if the user does not have permission to edit completed procedures.
			listProcsInAppt.RemoveAll(x => x.ProcStatus==ProcStat.C && !Security.IsAuthorized(Permissions.ProcComplEdit,x.ProcDate,true));
			if(listProcsInAppt.Count==0) {
				return listProcsInAppt;//Nothing to do.
			}
			listProcsInAppt=Procedures.SetCompleteInAppt(apt,PlanList,patPlans,siteNum,patientAge,listProcsInAppt,subList,Security.CurUser);
			listProcsInAppt.ForEach(x => LogProcComplCreate(apt.PatNum,x,x.ToothNum));
			if(Programs.UsingOrion) {
				OrionProcs.SetCompleteInAppt(listProcsInAppt);
			}
			//automation
			AutomationL.Trigger(AutomationTrigger.CompleteProcedure,listProcsInAppt.Select(x => ProcedureCodes.GetStringProcCode(x.CodeNum)).ToList(),apt.PatNum);
			return listProcsInAppt;
		}

		///<summary>Returns empty string if no duplicates, otherwise returns duplicate procedure information.  In all places where this is called, we are guaranteed to have the eCW bridge turned on.  So this is an eCW peculiarity rather than an HL7 restriction.  Other HL7 interfaces will not be checking for duplicate procedures unless we intentionally add that as a feature later.</summary>
		public static string ProcsContainDuplicates(List<Procedure> procs) {
			bool hasLongDCodes=false;
			HL7Def defCur=HL7Defs.GetOneDeepEnabled();
			if(defCur!=null) {
				hasLongDCodes=defCur.HasLongDCodes;
			}
			string info="";
			List<Procedure> procsChecked=new List<Procedure>();
			for(int i=0;i<procs.Count;i++) {
				Procedure proc=procs[i];
				ProcedureCode procCode=ProcedureCodes.GetProcCode(procs[i].CodeNum);
				string procCodeStr=procCode.ProcCode;
				if(procCodeStr.Length>5
					&& procCodeStr.StartsWith("D")
					&& !hasLongDCodes)
				{
					procCodeStr=procCodeStr.Substring(0,5);
				}
				for(int j=0;j<procsChecked.Count;j++) {
					Procedure procDup=procsChecked[j];
					ProcedureCode procCodeDup=ProcedureCodes.GetProcCode(procsChecked[j].CodeNum);
					string procCodeDupStr=procCodeDup.ProcCode;
					if(procCodeDupStr.Length>5
						&& procCodeDupStr.StartsWith("D")
						&& !hasLongDCodes)
					{
						procCodeDupStr=procCodeDupStr.Substring(0,5);
					}
					if(procCodeDupStr!=procCodeStr) {
						continue;
					}
					if(procDup.ToothNum!=proc.ToothNum) {
						continue;
					}
					if(procDup.ToothRange!=proc.ToothRange) {
						continue;
					}
					if(procDup.ProcFee!=proc.ProcFee) {
						continue;
					}
					if(procDup.Surf!=proc.Surf) {
						continue;
					}
					if(info!="") {
						info+=", ";
					}
					info+=procCodeDupStr;
				}
				procsChecked.Add(proc);
			}
			if(info!="") {
				info=Lan.g("ProcedureL","Duplicate procedures")+": "+info;
			}
			return info;
		}

		///<summary>Creates securitylog entry for a completed procedure.  Set toothNum to empty string and it will be omitted from the log entry. toothNums can be null or empty.</summary>
		public static void LogProcComplCreate(long patNum,Procedure procCur,string toothNums) {
			//No need to check RemotingRole; no call to db.
			if(procCur==null) {
				return;//Nothing to do.  Should never happen.
			}
			ProcedureCode procCode=ProcedureCodes.GetProcCode(procCur.CodeNum);
			string logText=procCode.ProcCode+", ";
			if(toothNums!=null && toothNums.Trim()!="") {
				logText+=Lans.g("Procedures","Teeth")+": "+toothNums+", ";
			}
			logText+=Lans.g("Procedures","Fee")+": "+procCur.ProcFee.ToString("F")+", "+procCode.Descript;
			SecurityLogs.MakeLogEntry(Permissions.ProcComplCreate,patNum,logText);
		}

	}
}
