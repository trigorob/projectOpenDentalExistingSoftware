﻿using CodeBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenDentBusiness {
	public class Procedures {
		///<summary>Gets all procedures for a single patient, without notes.  Does not include deleted procedures.</summary>
		public static List<Procedure> Refresh(long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),patNum);
			}
			string command="SELECT * FROM procedurelog WHERE PatNum="+POut.Long(patNum)
				+" AND ProcStatus !="+POut.Int((int)ProcStat.D)//don't include deleted
				+" ORDER BY ProcDate";
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets all procedures with a code num in listProcCodeNums for a single patient, without notes.  Does not include deleted procedures.</summary>
		public static List<Procedure> RefreshForProcCodeNums(long patNum,List<long> listProcCodeNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),patNum,listProcCodeNums);
			}
			if(listProcCodeNums==null || listProcCodeNums.Count==0) {
				return new List<Procedure>();
			}
			string command="SELECT * FROM procedurelog WHERE PatNum="+POut.Long(patNum)+" "+
				"AND CodeNum IN ("+String.Join(",",listProcCodeNums)+") "+
				"AND ProcStatus !="+POut.Int((int)ProcStat.D)+" "+//don't include deleted
				"ORDER BY ProcDate";
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets all completed procedures without notes for a list of patients.  Used when making auto splits.
		///Also returns any procedures attached to payplans that the current patient is responsible for.</summary>
		public static List<Procedure> GetCompleteForPats(List<long> listPatNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),listPatNums);
			}
			if(listPatNums==null || listPatNums.Count < 1) {
				return new List<Procedure>();
			}
			string command="SELECT * FROM ( "
				+" SELECT procedurelog.* "
				+" FROM patient "
				+" LEFT JOIN procedurelog ON procedurelog.PatNum = patient.Patnum "
				+" WHERE patient.PatNum IN ("+String.Join(",",listPatNums)+") "
				+" AND ProcStatus = "+POut.Int((int)ProcStat.C)+" "
				+" AND procedurelog.ProcNum IS NOT NULL "
				+" UNION "
				+" SELECT procedurelog.* "
				+" FROM patient "
				+" LEFT JOIN payplan ON payplan.Guarantor = patient.PatNum "
				+" LEFT JOIN payplancharge ON payplancharge.PayPlanNum = payplan.PayPlanNum "
				+" LEFT JOIN procedurelog ON procedurelog.ProcNum = payplancharge.ProcNum "
				+" WHERE patient.PatNum IN ("+String.Join(",",listPatNums)+") "
				+" AND ProcStatus = "+POut.Int((int)ProcStat.C)+" "
				+" AND procedurelog.ProcNum IS NOT NULL "
			+" )A "
			+" ORDER BY A.ProcDate ";
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets all completed and TP procedures for a family.</summary>
		public static List<Procedure> GetCompAndTpForPats(List<long> listPatNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),listPatNums);
			}
			string command="SELECT * from procedurelog WHERE PatNum IN("+String.Join(", ",listPatNums)+") "
				+"AND ProcStatus IN("+(int)ProcStat.C+","+(int)ProcStat.TP+") "
				+"ORDER BY ProcDate";
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary></summary>
		public static long Insert(Procedure procedure) {
			if(RemotingClient.RemotingRole!=RemotingRole.ServerWeb) {
				procedure.SecUserNumEntry=Security.CurUser.UserNum;//must be before normal remoting role check to get user at workstation
			}
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				procedure.ProcNum=Meth.GetLong(MethodBase.GetCurrentMethod(),procedure);
				return procedure.ProcNum;
			}
			if(procedure.ProcStatus==ProcStat.C) {
				procedure.DateComplete=DateTime.Today;
			}
			else {//In case someone tried to programmatically set the DateComplete when they shouldn't have.
				procedure.DateComplete=DateTime.MinValue;
			}
			Crud.ProcedureCrud.Insert(procedure);
			if(procedure.Note!="") {
				ProcNote note=new ProcNote();
				note.PatNum=procedure.PatNum;
				note.ProcNum=procedure.ProcNum;
				note.UserNum=procedure.UserNum;
				note.Note=procedure.Note;
				ProcNotes.Insert(note);
			}
			return procedure.ProcNum;
		}

		///<summary>Updates only the changed columns.</summary>
		public static bool Update(Procedure procedure,Procedure oldProcedure) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod(),procedure,oldProcedure);
			}
			//Setting a discount procedure to complete.
			if(oldProcedure.ProcStatus!=ProcStat.C && procedure.ProcStatus==ProcStat.C && !procedure.Discount.IsZero()) {
				Adjustments.CreateAdjustmentForDiscount(procedure);
			}
			//Setting the procedure to complete.
			if(oldProcedure.ProcStatus!=ProcStat.C && procedure.ProcStatus==ProcStat.C) {
				PayPlanCharges.UpdateAttachedPayPlanCharges(procedure);//does nothing if there are none.
			}
			//Setting a completed procedure to TP.
			if(oldProcedure.ProcStatus==ProcStat.C && procedure.ProcStatus!=ProcStat.C) {
				Adjustments.DeleteForProcedure(procedure.ProcNum);
			}
			if(procedure.ProcStatus==ProcStat.C && procedure.DateComplete.Year<1880) {
				procedure.DateComplete=DateTime.Today;
			}
			else if(procedure.ProcStatus!=ProcStat.C && procedure.DateComplete.Date==DateTime.Today.Date) {
				procedure.DateComplete=DateTime.MinValue;//db only field used by one customer and this is how they requested it.  PatNum #19191
			}
			bool result=Crud.ProcedureCrud.Update(procedure,oldProcedure);
			if(procedure.Note!=oldProcedure.Note
				|| procedure.UserNum!=oldProcedure.UserNum
				|| procedure.SigIsTopaz!=oldProcedure.SigIsTopaz
				|| procedure.Signature!=oldProcedure.Signature) 
			{
				ProcNote note=new ProcNote();
				note.PatNum=procedure.PatNum;
				note.ProcNum=procedure.ProcNum;
				note.UserNum=procedure.UserNum;
				note.Note=procedure.Note;
				note.SigIsTopaz=procedure.SigIsTopaz;
				note.Signature=procedure.Signature;
				ProcNotes.Insert(note);
			}
			return result;
		}

		///<summary>Throws an exception if the given procedure cannot be deleted safely.</summary>
		public static void ValidateDelete(long procNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),procNum);
				return;
			}
			//Test to see if the procedure is attached to a claim
			string command="SELECT COUNT(*) FROM claimproc WHERE ProcNum="+POut.Long(procNum)
				+" AND ClaimNum > 0";
			if(Db.GetCount(command)!="0") {
				throw new Exception(Lans.g("Procedures","Not allowed to delete a procedure that is attached to a claim or a pre-auth."));
			}
			//Test to see if any payment at all has been received for this proc
			command="SELECT COUNT(*) FROM claimproc WHERE ProcNum="+POut.Long(procNum)
				+" AND InsPayAmt > 0 AND Status != "+POut.Long((int)ClaimProcStatus.Preauth);
			if(Db.GetCount(command)!="0") {
				throw new Exception(Lans.g("Procedures","Not allowed to delete a procedure that is attached to an insurance payment."));
			}
			//Test to see if any referrals exist for this proc
			command="SELECT COUNT(*) FROM refattach WHERE ProcNum="+POut.Long(procNum);
			if(Db.GetCount(command)!="0") {
				throw new Exception(Lans.g("Procedures","Not allowed to delete a procedure with referrals attached."));
			}
			//Test to see if any paysplits are attached to this proc
			command="SELECT COUNT(*) FROM paysplit WHERE ProcNum="+POut.Long(procNum);
			if(Db.GetCount(command)!="0") {
				throw new Exception(Lans.g("Procedures","Not allowed to delete a procedure that is attached to a patient payment."));
			}
		}

		///<summary>If not allowed to delete, then it throws an exception, so surround it with a try catch. 
		///Also deletes any claimProcs, adjustments, and payplancharge credits.  
		///This does not actually delete the procedure, but just changes the status to deleted.</summary>
		///<param name="forceDelete">If true, forcefully deletes all objects attached to the procedure.</param>
		public static void Delete(long procNum,bool forceDelete=false) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),procNum,forceDelete);
				return;
			}
			if(CultureInfo.CurrentCulture.Name.EndsWith("CA")) {
				DeleteCanadianLabFeesForProcCode(procNum);//Deletes lab fees attached to current procedures.
			}
			string command;
			if(forceDelete) {
				//Delete referral attaches
				command="DELETE FROM refattach WHERE ProcNum="+POut.Long(procNum);
				Db.NonQ(command);
				//Remove the procedure from the pay split
				command="UPDATE paysplit SET ProcNum=0 WHERE ProcNum="+POut.Long(procNum);
				Db.NonQ(command);
				//Claimprocs deleted below
			}
			else {
				ValidateDelete(procNum);
			}
			//delete adjustments, audit logs added from Adjustments.DeleteForProcedure()
			Adjustments.DeleteForProcedure(procNum);
			//delete claimprocs
			command="DELETE from claimproc WHERE ProcNum = '"+POut.Long(procNum)+"'";
			Db.NonQ(command);
			PayPlanCharges.DeleteForProc(procNum);
			//resynch appointment description-------------------------------------------------------------------------------------
			command="SELECT AptNum,PlannedAptNum,DateComplete FROM procedurelog WHERE ProcNum = "+POut.Long(procNum);
			DataTable table=Db.GetTable(command);
			string aptnum=table.Rows[0][0].ToString();
			string plannedaptnum=table.Rows[0][1].ToString();
			DateTime dateComplete=PIn.Date(table.Rows[0]["DateComplete"].ToString());//set DateComplete=DateTime.MinValue if DateComplete==DateTime.Today
			string procdescript;
			if(aptnum!="0") {
				command=@"SELECT AbbrDesc FROM procedurecode,procedurelog
					WHERE procedurecode.CodeNum=procedurelog.CodeNum
					AND ProcNum != "+POut.Long(procNum)
					+" AND procedurelog.AptNum="+aptnum;
				table=Db.GetTable(command);
				procdescript="";
				for(int i=0;i<table.Rows.Count;i++) {
					if(i>0) procdescript+=", ";
					procdescript+=table.Rows[i]["AbbrDesc"].ToString();
				}
				command="UPDATE appointment SET ProcDescript='"+POut.String(procdescript)+"' "
					+"WHERE AptNum="+aptnum;
				Db.NonQ(command);
			}
			if(plannedaptnum!="0") {
				command=@"SELECT AbbrDesc FROM procedurecode,procedurelog
					WHERE procedurecode.CodeNum=procedurelog.CodeNum
					AND ProcNum != "+POut.Long(procNum)
					+" AND procedurelog.PlannedAptNum="+plannedaptnum;
				table=Db.GetTable(command);
				procdescript="";
				for(int i=0;i<table.Rows.Count;i++) {
					if(i>0) procdescript+=", ";
					procdescript+=table.Rows[i]["AbbrDesc"].ToString();
				}
				command="UPDATE appointment SET ProcDescript='"+POut.String(procdescript)+"' "
					+"WHERE NextAptNum="+plannedaptnum;
				Db.NonQ(command);
			}
			//set the procedure deleted-----------------------------------------------------------------------------------------
			command="UPDATE procedurelog SET ProcStatus = "+POut.Int((int)ProcStat.D)+", "
				+"AptNum=0, "
				+"PlannedAptNum=0";
			if(dateComplete.Date==DateTime.Today.Date) {
				command+=", DateComplete="+POut.Date(DateTime.MinValue);
			}
			command+=" WHERE ProcNum="+POut.Long(procNum);
			Db.NonQ(command);
		}


		///<summary>Creates a new procedure with the patient, surface, toothnum, and status for the specified procedure code.
		///Make sure to make a security log after calling this method.  This method requires that Security.CurUser be set prior to invoking.
		///Returns null procedure if one was not created for the patient.</summary>
		public static Procedure CreateProcForPat(long patNum,long codeNum,string surf,string toothNum,ProcStat procStatus,long provNum) {
			//No need to check RemotingRole; no call to db.
			Patient pat=Patients.GetPat(patNum);
			return CreateProcForPat(pat,codeNum,surf,toothNum,procStatus,provNum);
		}

		///<summary>Creates a new procedure with the patient, surface, toothnum, and status for the specified procedure code.
		///Make sure to make a security log after calling this method.  This method requires that Security.CurUser be set prior to invoking.
		///Returns null procedure if one was not created for the patient.</summary>
		public static Procedure CreateProcForPat(Patient pat,long codeNum,string surf,string toothNum,ProcStat procStatus,long provNum,long aptNum=0
			,List<InsSub> subList=null,List<InsPlan> insPlanList=null,List<PatPlan> patPlanList=null,List<Benefit> benefitList=null) 
		{
			//No need to check RemotingRole; no call to db.
			if(codeNum < 1) {
				return null;
			}
			if(provNum==0) {
				provNum=Patients.GetProvNum(pat);
			}
			Procedure proc=new Procedure();
			proc.PatNum=pat.PatNum;
			proc.ClinicNum=Clinics.ClinicNum;
			proc.ProcStatus=procStatus;
			proc.ProvNum=provNum;
			proc.AptNum=aptNum;
			if(surf=="") {
				proc.Surf="";
			}
			else { 
				proc.Surf=surf; //Note: Sealant code D1351 is not a surface code by default, but can be manually set.  For screens they will be surface specific.
			}
			if(toothNum=="") {
				proc.ToothNum="";
			}
			else {
				proc.ToothNum=toothNum;
			}
			proc.UserNum=Security.CurUser.UserNum;
			proc.CodeNum=codeNum;
			proc.ProcDate=DateTime.Today;
			proc.DateTP=DateTime.Today;
			//The below logic is a trimmed down version of the code existing in ContrChart.AddQuick()
			InsPlan insPlanPrimary=null;
			InsSub insSubPrimary=null;
			if(subList==null) {
				subList=InsSubs.RefreshForFam(Patients.GetFamily(pat.PatNum));
			}
			if(insPlanList==null) {
				insPlanList=InsPlans.RefreshForSubList(subList);
			}
			if(patPlanList==null) {
				patPlanList=PatPlans.Refresh(pat.PatNum);
			}
			if(benefitList==null) {
				benefitList=Benefits.Refresh(patPlanList,subList);
			}
			if(patPlanList.Count>0) {
				insSubPrimary=InsSubs.GetSub(patPlanList[0].InsSubNum,subList);
				insPlanPrimary=InsPlans.GetPlan(insSubPrimary.PlanNum,insPlanList);
			}
			//Get fee schedule and fee amount for dental or medical.
			long feeSch=Fees.GetFeeSched(pat,insPlanList,patPlanList,subList,provNum);
			proc.ProcFee=Fees.GetAmount0(proc.CodeNum,feeSch,proc.ClinicNum,provNum);
			if(insPlanPrimary!=null && insPlanPrimary.PlanType=="p") {//PPO
				double provFee=Fees.GetAmount0(proc.CodeNum,Providers.GetProv(provNum).FeeSched,proc.ClinicNum,provNum);
				proc.ProcFee=Math.Max(proc.ProcFee,provFee);//use greater of standard fee or ins fee
			}
			ProcedureCode procCodeCur=ProcedureCodes.GetProcCode(proc.CodeNum);
			proc.BaseUnits=procCodeCur.BaseUnits;
			proc.SiteNum=pat.SiteNum;
			proc.RevCode=procCodeCur.RevenueCodeDefault;
			proc.DateEntryC=DateTime.Now;
			proc.PlaceService=(PlaceOfService)PrefC.GetInt(PrefName.DefaultProcedurePlaceService);//Default Proc Place of Service for the Practice is used.
			proc.ProcNum=Procedures.Insert(proc);
			Procedures.ComputeEstimates(proc,pat.PatNum,new List<ClaimProc>(),true,insPlanList,patPlanList,benefitList,pat.Age,subList);
			return proc;
		}

		///<summary>Creates a new procedure for every proc code passed in.  Make sure to make a security log after calling this method.
		///This method requires that Security.CurUser be set prior to invoking.  Returns an empty list if none were created for the patient.</summary>
		public static List<Procedure> CreateProcsForPat(long patNum,List<long> listProcCodeNums,string surf,string toothNum,ProcStat procStatus
			,long provNum,long aptNum) 
		{
			//No need to check RemotingRole; no call to db.
			List<Procedure> listProcedures=new List<Procedure>();
			Patient patient=Patients.GetPat(patNum);
			List<InsSub> subList=InsSubs.RefreshForFam(Patients.GetFamily(patNum));
			List<InsPlan> insPlanList=InsPlans.RefreshForSubList(subList);
			List<PatPlan> patPlanList=PatPlans.Refresh(patNum);
			List<Benefit> benefitList=Benefits.Refresh(patPlanList,subList);
			foreach(long codeNum in listProcCodeNums) {
				Procedure proc=CreateProcForPat(patient,codeNum,surf,toothNum,procStatus,provNum,aptNum,subList,insPlanList,patPlanList,benefitList);
				if(proc!=null) {
					listProcedures.Add(proc);
				}
			}
			return listProcedures;
		}

		public static void UpdateAptNum(long procNum,long newAptNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),procNum,newAptNum);
				return;
			}
			string command="UPDATE procedurelog SET AptNum = "+POut.Long(newAptNum)
				+" WHERE ProcNum = "+POut.Long(procNum);
			Db.NonQ(command);
		}

		//public static void UpdatePlannedAptNum(long procNum,long newPlannedAptNum) {
		//	if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
		//		Meth.GetVoid(MethodBase.GetCurrentMethod(),procNum,newPlannedAptNum);
		//		return;
		//	}
		//	string command="UPDATE procedurelog SET PlannedAptNum = "+POut.Long(newPlannedAptNum)
		//		+" WHERE ProcNum = "+POut.Long(procNum);
		//	Db.NonQ(command);
		//}

		public static void UpdatePriority(long procNum,long newPriority) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),procNum,newPriority);
				return;
			}
			string command="UPDATE procedurelog SET Priority = "+POut.Long(newPriority)
				+" WHERE ProcNum = "+POut.Long(procNum);
			Db.NonQ(command);
		}

		public static void UpdateFee(long procNum,double newFee) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),procNum,newFee);
				return;
			}
			string command="UPDATE procedurelog SET ProcFee = "+POut.Double(newFee)
				+" WHERE ProcNum = "+POut.Long(procNum);
			Db.NonQ(command);
		}

		///<summary>Updates IsCpoe column in the procedurelog table with the passed in value for the corresponding procedure.
		///This method explicitly used instead of the generic Update method because this (and only this) field can get updated when a user cancels out
		///of the Procedure Edit window and no other changes should accidentally make their way to the database.</summary>
		public static void UpdateCpoeForProc(long procNum,bool isCpoe) {
			//No need to check RemotingRole; no call to db.
			UpdateCpoeForProcs(new List<long>() { procNum },isCpoe);
		}

		///<summary>Updates IsCpoe column in the procedurelog table with the passed in value for the corresponding procedures.
		///This method explicitly used instead of the generic Update method because this (and only this) field can get updated when a user cancels out
		///of the Procedure Edit window and no other changes should accidentally make their way to the database.</summary>
		public static void UpdateCpoeForProcs(List<long> listProcNums,bool isCpoe) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),listProcNums,isCpoe);
				return;
			}
			if(listProcNums==null || listProcNums.Count < 1) {
				return;
			}
			string command="UPDATE procedurelog SET IsCpoe = "+POut.Bool(isCpoe)
				+" WHERE ProcNum IN ("+string.Join(",",listProcNums)+")";
			Db.NonQ(command);
		}

		///<summary>Gets one procedure directly from the db.  Always includes the note regardless of includeNote value (since 2010 at least).
		///If the procNum is 0 or if the procNum does not exist in the database, this will return a new Procedure object with uninitialized fields.  
		///If, for example, a new Procedure object is sent through the middle tier with an uninitialized ProcStatus=0, 
		///  this will fail validation since the ProcStatus enum starts with 1.  
		///Make sure to handle a new Procedure object with uninitialized fields.</summary>
		public static Procedure GetOneProc(long procNum,bool includeNote) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<Procedure>(MethodBase.GetCurrentMethod(),procNum,includeNote);
			}
			Procedure proc=Crud.ProcedureCrud.SelectOne(procNum);
			if(proc==null){
				return new Procedure();
			}
			if(!includeNote) {
				return proc;
			}
			string command="SELECT * FROM procnote WHERE ProcNum="+POut.Long(procNum)+" ORDER BY EntryDateTime DESC";
			DbHelper.LimitOrderBy(command,1);
			DataTable table=Db.GetTable(command);
			if(table.Rows.Count==0) {
				return proc;
			}
			proc.UserNum   =PIn.Long(table.Rows[0]["UserNum"].ToString());
			proc.Note      =PIn.String(table.Rows[0]["Note"].ToString());
			proc.SigIsTopaz=PIn.Bool(table.Rows[0]["SigIsTopaz"].ToString());
			proc.Signature =PIn.String(table.Rows[0]["Signature"].ToString());
			return proc;
		}

		///<summary>Gets one procedure directly from the db.  Option to include the note.  If the procNum is 0 or if the procNum does not exist in the database, this will return a new Procedure object with uninitialized fields.  If, for example, a new Procedure object is sent through the middle tier with an uninitialized ProcStatus=0, this will fail validation since the ProcStatus enum starts with 1.  Make sure to handle a new Procedure object with uninitialized fields.</summary>
		public static List<Procedure> GetManyProc(List<long> listProcNums,bool includeNote) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),listProcNums,includeNote);
			}
			if(listProcNums==null || listProcNums.Count==0) {
				return new List<Procedure>();
			}
			string command="SELECT * FROM procedurelog WHERE ProcNum IN ("+string.Join(",",listProcNums)+")";
			List<Procedure> listProcs=Crud.ProcedureCrud.SelectMany(command);
			if(!includeNote) {
				return listProcs;
			}
			foreach(Procedure proc in listProcs) {
				command="SELECT * FROM procnote WHERE ProcNum="+POut.Long(proc.ProcNum)+" ORDER BY EntryDateTime DESC";
				DbHelper.LimitOrderBy(command,1);
				DataTable table=Db.GetTable(command);
				if(table.Rows.Count==0) {
					continue;
				}
				proc.UserNum=PIn.Long(table.Rows[0]["UserNum"].ToString());
				proc.Note=PIn.String(table.Rows[0]["Note"].ToString());
				proc.SigIsTopaz=PIn.Bool(table.Rows[0]["SigIsTopaz"].ToString());
				proc.Signature=PIn.String(table.Rows[0]["Signature"].ToString());
			}
			return listProcs;
		}

		///<summary>Gets Procedures for a single appointment directly from the database</summary>
		public static List<Procedure> GetProcsForSingle(long aptNum,bool isPlanned) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),aptNum,isPlanned);
			}
			string command;
			if(isPlanned) {
				command = "SELECT * from procedurelog WHERE PlannedAptNum = '"+POut.Long(aptNum)+"'";
			}
			else {
				command = "SELECT * from procedurelog WHERE AptNum = '"+POut.Long(aptNum)+"'";
			}
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets all Procedures that need to be displayed in FormApptEdit.</summary>
		public static List<Procedure> GetProcsForApptEdit(Appointment appt) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),appt);
			}
			string command="SELECT procedurelog.* FROM procedurelog "
				+"LEFT JOIN procedurecode ON procedurelog.CodeNum=procedurecode.CodeNum "
				+"WHERE procedurelog.PatNum="+POut.Long(appt.PatNum)+" "
				+"AND (procedurelog.ProcStatus="+POut.Long((int)ProcStat.TP)+" OR ";
			if(appt.AptStatus==ApptStatus.Planned) {
				command+="procedurelog.PlannedAptNum="+POut.Long(appt.AptNum)+" ";
			}
			else {//Scheduled
				command+="procedurelog.AptNum="+POut.Long(appt.AptNum)+" ";
			}
			if(appt.AptStatus==ApptStatus.Scheduled || appt.AptStatus==ApptStatus.Complete 
				|| appt.AptStatus==ApptStatus.ASAP || appt.AptStatus==ApptStatus.Broken)
			{
					command+="OR (procedurelog.AptNum=0 AND procedurelog.ProcStatus="+POut.Long((int)ProcStat.C)+" AND "
						+DbHelper.DtimeToDate("procedurelog.ProcDate")+"="+POut.Date(appt.AptDateTime)+") ";
			}
			command+=") AND procedurelog.ProcStatus != "+POut.Long((int)ProcStat.D)+" AND procedurecode.IsCanadianLab=0";
			List<Procedure> result=Crud.ProcedureCrud.SelectMany(command);
			for(int i=0;i<result.Count;i++){
				command="SELECT * FROM procnote WHERE ProcNum="+POut.Long(result[i].ProcNum)+" ORDER BY EntryDateTime DESC";
				command=DbHelper.LimitOrderBy(command,1);
				DataTable table=Db.GetTable(command);
				if(table.Rows.Count==0) {
					continue;
				}
				result[i].UserNum   =PIn.Long(table.Rows[0]["UserNum"].ToString());
				result[i].Note      =PIn.String(table.Rows[0]["Note"].ToString());
				result[i].SigIsTopaz=PIn.Bool(table.Rows[0]["SigIsTopaz"].ToString());
				result[i].Signature =PIn.String(table.Rows[0]["Signature"].ToString());
			}
			result.Sort(ProcedureLogic.CompareProcedures);
			return result;
		}

		///<summary>Gets all Procedures for a single date for the specified patient directly from the database.  Excludes deleted procs.</summary>
		public static List<Procedure> GetProcsForPatByDate(long patNum,DateTime date) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),patNum,date);
			}
			string command="SELECT * FROM procedurelog "
				+"WHERE PatNum="+POut.Long(patNum)+" "
				+"AND (ProcDate="+POut.Date(date)+" OR DateEntryC="+POut.Date(date)+") "
				+"AND ProcStatus!="+POut.Int((int)ProcStat.D);//exclude deleted procs
			List<Procedure> result=Crud.ProcedureCrud.SelectMany(command);
			for(int i=0;i<result.Count;i++){
				command="SELECT * FROM procnote WHERE ProcNum="+POut.Long(result[i].ProcNum)+" ORDER BY EntryDateTime DESC";
				command=DbHelper.LimitOrderBy(command,1);
				DataTable table=Db.GetTable(command);
				if(table.Rows.Count==0) {
					continue;
				}
				result[i].UserNum   =PIn.Long(table.Rows[0]["UserNum"].ToString());
				result[i].Note      =PIn.String(table.Rows[0]["Note"].ToString());
				result[i].SigIsTopaz=PIn.Bool(table.Rows[0]["SigIsTopaz"].ToString());
				result[i].Signature =PIn.String(table.Rows[0]["Signature"].ToString());
			}
			return result;
		}

		///<summary>Gets all procedures associated with corresponding claimprocs. Returns empty procedure list if an empty list was passed in.</summary>
		public static List<Procedure> GetProcsFromClaimProcs(List<ClaimProc> listClaimProc) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),listClaimProc);
			}
			if(listClaimProc.Count==0) {
				return new List<Procedure>();
			}
			string command="SELECT * FROM procedurelog WHERE ProcNum IN (";
			for(int i=0;i<listClaimProc.Count;i++) {
				if(i>0) {
					command+=",";
				}
				command+=listClaimProc[i].ProcNum;
			}
			command+=")";
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets a list of TP procedures that are attached to scheduled appointments that are not flagged as CPOE.</summary>
		public static List<Procedure> GetProcsNonCpoeAttachedToApptsForProv(long provNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),provNum);
			}
			string command="SELECT procedurelog.* "
				+"FROM procedurelog "
				+"INNER JOIN appointment ON procedurelog.AptNum=appointment.AptNum "
				+"INNER JOIN procedurecode ON procedurelog.CodeNum=procedurecode.CodeNum "
				+"WHERE procedurecode.IsRadiology=1 "
				+"AND appointment.AptStatus="+POut.Int((int)ApptStatus.Scheduled)+" "
				+"AND procedurelog.ProcStatus="+POut.Int((int)ProcStat.TP)+" "
				+"AND procedurelog.IsCpoe=0 "
				+"AND procedurelog.ProvNum="+POut.Long(provNum)+" "
				+"AND "+DbHelper.DtimeToDate("appointment.AptDateTime")+" >= "+DbHelper.Curdate()+" "
				+"ORDER BY appointment.AptDateTime";
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets a list of TP or C procedures starting a year into the past that are flagged as IsRadiology and IsCpoe for the specified patient.
		///Primarily used for showing patient specific MU data in the EHR dashboard.</summary>
		public static List<Procedure> GetProcsRadiologyCpoeForPat(long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),patNum);
			}
			//Since this is used for the dashboard and not directly used in any reporting calculations, we do not need to worry about the date that the
			// office updated past v15.4.1.
			DateTime dateStart=new DateTime(DateTime.Now.Year,1,1);//January first of this year.
			DateTime dateEnd=dateStart.AddYears(1).AddDays(-1);//Last day in December of this year.
			string command="SELECT procedurelog.* "
				+"FROM procedurelog "
				+"INNER JOIN procedurecode ON procedurelog.CodeNum=procedurecode.CodeNum AND procedurecode.IsRadiology=1 "
				+"WHERE procedurelog.ProcStatus IN ("+POut.Int((int)ProcStat.C)+","+POut.Int((int)ProcStat.TP)+") "
				+"AND procedurelog.PatNum="+POut.Long(patNum)+" "
				+"AND procedurelog.IsCpoe=1 "
				+"AND procedurelog.DateEntryC BETWEEN "+POut.Date(dateStart)+" AND "+POut.Date(dateEnd);
			return Crud.ProcedureCrud.SelectMany(command);
		}

		public static List<Procedure> GetProcsByStatusForPat(long patNum,ProcStat[] procStatuses) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),patNum,procStatuses);
			}
			if(procStatuses==null || procStatuses.Length==0) {
				return new List<Procedure>();
			}
			string command="SELECT * FROM procedurelog WHERE PatNum="+POut.Long(patNum)+" AND ProcStatus IN ("+string.Join(",",procStatuses.Select(x => (int)x))+")";
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets a string in M/yy format for the most recent completed procedure in the specified code range.  Gets directly from the database.</summary>
		public static string GetRecentProcDateString(long patNum,DateTime aptDate,string procCodeRange) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetString(MethodBase.GetCurrentMethod(),patNum,aptDate,procCodeRange);
			}
			if(aptDate.Year<1880) {
				aptDate=DateTime.Today;
			}
			string code1;
			string code2;
			if(procCodeRange.Contains("-")) {
				string[] codeSplit=procCodeRange.Split('-');
				code1=codeSplit[0].Trim();
				code2=codeSplit[1].Trim();
			}
			else {
				code1=procCodeRange.Trim();
				code2=procCodeRange.Trim();
			}
			string command="SELECT ProcDate FROM procedurelog "
				+"LEFT JOIN procedurecode ON procedurecode.CodeNum=procedurelog.CodeNum "
				+"WHERE PatNum="+POut.Long(patNum)+" "
				//+"AND CodeNum="+POut.Long(codeNum)+" "
				+"AND ProcDate < "+POut.Date(aptDate)+" "
				+"AND (ProcStatus ="+POut.Int((int)ProcStat.C)+" "
				+"OR ProcStatus ="+POut.Int((int)ProcStat.EC)+" "
				+"OR ProcStatus ="+POut.Int((int)ProcStat.EO)+") "
				+"AND procedurecode.ProcCode >= '"+POut.String(code1)+"' "
				+"AND procedurecode.ProcCode <= '"+POut.String(code2)+"' "
				+"ORDER BY ProcDate DESC";
			command=DbHelper.LimitOrderBy(command,1);
			DateTime date=PIn.Date(Db.GetScalar(command));
			if(date.Year<1880) {
				return "";
			}
			return date.ToString("M/yy");
		}

		///<summary>Gets the first completed procedure within the family.  Used to determine the earliest date the family became a customer.</summary>
		public static Procedure GetFirstCompletedProcForFamily(long guarantor) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<Procedure>(MethodBase.GetCurrentMethod(),guarantor);
			}
			string command="SELECT procedurelog.* FROM procedurelog "
				+"LEFT JOIN patient ON procedurelog.PatNum=patient.PatNum AND patient.Guarantor="+POut.Long(guarantor)+" "
				+"WHERE "+DbHelper.Year("procedurelog.ProcDate")+">1 "
				+"AND procedurelog.ProcStatus="+POut.Int((int)ProcStat.C)+" "
				+"ORDER BY procedurelog.ProcDate";
			command=DbHelper.LimitOrderBy(command,1);
			return Crud.ProcedureCrud.SelectOne(command);
		}

		///<summary>Gets a list (procsMultApts is a struct of type ProcDesc(aptNum, string[], and production) of all the procedures attached to the specified appointments.  Then, use GetProcsOneApt to pull procedures for one appointment from this list or GetProductionOneApt.  This process requires only one call to the database.  "myAptNums" is the list of appointments to get procedures for.  isForNext gets procedures for a list of next appointments rather than regular appointments.</summary>
		public static List<Procedure> GetProcsMultApts(List<long> myAptNums,bool isForPlanned=false) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),myAptNums,isForPlanned);
			}
			if(myAptNums.Count==0) {
				return new List<Procedure>();
			}
			string strAptNums="";
			for(int i=0;i<myAptNums.Count;i++) {
				if(i>0) {
					strAptNums+=" OR";
				}
				if(isForPlanned) {
					strAptNums+=" PlannedAptNum='"+POut.Long(myAptNums[i])+"'";
				}
				else {
					strAptNums+=" AptNum='"+POut.Long(myAptNums[i])+"'";
				}
			}
			string command = "SELECT * FROM procedurelog WHERE"+strAptNums;
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets procedures for one appointment by looping through the procsMultApts which was filled previously from GetProcsMultApts.</summary>
		public static Procedure[] GetProcsOneApt(long myAptNum,List<Procedure> procsMultApts) {
			//No need to check RemotingRole; no call to db.
			ArrayList al=new ArrayList();
			for(int i=0;i<procsMultApts.Count;i++) {
				if(procsMultApts[i].AptNum==myAptNum) {
					al.Add(procsMultApts[i].Copy());
				}
			}
			Procedure[] retVal=new Procedure[al.Count];
			al.CopyTo(retVal);
			return retVal;
		}

		/////<summary>Gets the production for one appointment by looping through the procsMultApts which was filled previously from GetProcsMultApts.</summary>
		//public static double GetProductionOneApt(long myAptNum,Procedure[] procsMultApts,bool isPlanned) {
		//	//No need to check RemotingRole; no call to db.
		//	double retVal=0;
		//	for(int i=0;i<procsMultApts.Length;i++) {
		//		if(isPlanned && procsMultApts[i].PlannedAptNum==myAptNum) {
		//			retVal+=procsMultApts[i].ProcFee*(procsMultApts[i].BaseUnits+procsMultApts[i].UnitQty);
		//		}
		//		if(!isPlanned && procsMultApts[i].AptNum==myAptNum) {
		//			retVal+=procsMultApts[i].ProcFee*(procsMultApts[i].BaseUnits+procsMultApts[i].UnitQty);
		//		}
		//	}
		//	return retVal;
		//}

		///<summary>Used in FormClaimEdit,FormClaimPrint,FormClaimPayTotal,ContrAccount etc to get description of procedure. Procedure list needs to include the procedure we are looking for.  If procNum could be 0 (e.g. total payment claimprocs) or if the list does not contain the procNum, this will return a new Procedure with uninitialized fields.  If, for example, a new Procedure object is sent through the middle tier with an uninitialized ProcStatus=0, this will fail validation since the ProcStatus enum starts with 1.  Make sure to handle a new Procedure object with uninitialized fields.</summary>
		public static Procedure GetProcFromList(List<Procedure> list,long procNum) {
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<list.Count;i++) {
				if(procNum==list[i].ProcNum) {
					return list[i];
				}
			}
			//MessageBox.Show("Error. Procedure not found");
			return new Procedure();
		}

		///<summary>Sets the patient.DateFirstVisit if necessary. A visitDate is required to be passed in because it may not be today's date. This is triggered by:
		///1. When any procedure is inserted regardless of status. From Chart or appointment. If no C procs and date blank, changes date.
		///2. When updating a procedure to status C. If no C procs, update visit date. Ask user first?
		///  #2 was recently changed to only happen if date is blank or less than 7 days old.
		///3. When an appointment is deleted. If no C procs, clear visit date.
		///  #3 was recently changed to not occur at all unless appt is of type IsNewPatient
		///4. Changing an appt date of type IsNewPatient. If no C procs, change visit date.
		///Old: when setting a procedure complete in the Chart module or the ProcEdit window.  Also when saving an appointment that is marked IsNewPat.</summary>
		public static void SetDateFirstVisit(DateTime visitDate,int situation,Patient pat) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),visitDate,situation,pat);
				return;
			}
			if(situation==1) {
				if(pat.DateFirstVisit.Year>1880) {
					return;//a date has already been set.
				}
			}
			if(situation==2) {
				if(pat.DateFirstVisit.Year>1880 && pat.DateFirstVisit<DateTime.Now.AddDays(-7)) {
					return;//a date has already been set.
				}
			}
			string command="SELECT COUNT(*) from procedurelog "
				+"INNER JOIN procedurecode on procedurecode.CodeNum = procedurelog.CodeNum "
					+"AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
				+"WHERE PatNum = '"+POut.Long(pat.PatNum)+"' "
				+"AND ProcStatus = '2'";
			DataTable table=Db.GetTable(command);
			if(PIn.Long(table.Rows[0][0].ToString())>0) {
				return;//there are already completed procs (for all situations)
			}
			if(situation==2) {
				//ask user first?
			}
			if(situation==3) {
				command="UPDATE patient SET DateFirstVisit ="+POut.Date(new DateTime(0001,01,01))
					+" WHERE PatNum ='"
					+POut.Long(pat.PatNum)+"'";
			}
			else {
				command="UPDATE patient SET DateFirstVisit ="
					+POut.Date(visitDate)+" WHERE PatNum ='"
					+POut.Long(pat.PatNum)+"'";
			}
			//MessageBox.Show(cmd.CommandText);
			//dcon.NonQ(command);
			Db.NonQ(command);
		}

		///<summary>Gets all completed procedures within a date range with optional ProcCodeNum and PatientNum filters. Date range is inclusive.</summary>
		public static List<Procedure> GetCompletedForDateRange(DateTime dateStart,DateTime dateStop,List<long> listProcCodeNums=null,List<long> listPatNums=null) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),dateStart,dateStop,listProcCodeNums,listPatNums);
			}
			string command="SELECT * FROM procedurelog WHERE ProcStatus=2 AND ProcDate>="+POut.Date(dateStart)+" AND ProcDate<="+POut.Date(dateStop);
			if (listProcCodeNums!=null && listProcCodeNums.Count > 0) {
				command+=" AND CodeNum IN ("+string.Join(",", listProcCodeNums)+")";
			}
			if(listPatNums!=null && listPatNums.Count > 0) {
				command+=" AND PatNum IN ("+string.Join(",",listPatNums)+")";
			}
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Gets all completed procedures having procedurelog.DateComplete within the date range. Date range is inclusive.</summary>
		public static List<Procedure> GetCompletedByDateCompleteForDateRange(DateTime dateStart,DateTime dateStop) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),dateStart,dateStop);
			}
			string command="SELECT * FROM procedurelog WHERE ProcStatus="+POut.Int((int)ProcStat.C)
				+" AND DateComplete>="+POut.Date(dateStart)
				+" AND DateComplete<="+POut.Date(dateStop);
			return Crud.ProcedureCrud.SelectMany(command);
		}

		///<summary>Called from FormApptsOther when creating a new appointment.  Returns true if there are any procedures marked complete for this patient.  The result is that the NewPt box on the appointment won't be checked.</summary>
		public static bool AreAnyComplete(long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod(),patNum);
			}
			string command="SELECT COUNT(*) FROM procedurelog "
				+"INNER JOIN procedurecode on procedurecode.CodeNum = procedurelog.CodeNum "
					+"AND procedurecode.ProcCode NOT IN ('D9986','D9987') "
				+"WHERE PatNum="+patNum.ToString()
				+" AND ProcStatus=2";
			DataTable table=Db.GetTable(command);
			if(table.Rows[0][0].ToString()=="0") {
				return false;
			}
			else return true;
		}

		///<summary>Called from AutoCodeItems.  Makes a call to the database to determine whether the specified tooth has been extracted or will be extracted. This could then trigger a pontic code.</summary>
		public static bool WillBeMissing(string toothNum,long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod(),toothNum,patNum);
			}
			//first, check for missing teeth
			string command="SELECT COUNT(*) FROM toothinitial "
				+"WHERE ToothNum='"+toothNum+"' "
				+"AND PatNum="+POut.Long(patNum)
				+" AND InitialType=0";//missing
			DataTable table=Db.GetTable(command);
			if(table.Rows[0][0].ToString()!="0") {
				return true;
			}
			//then, check for a planned extraction
			command="SELECT COUNT(*) FROM procedurelog,procedurecode "
				+"WHERE procedurelog.CodeNum=procedurecode.CodeNum "
				+"AND procedurelog.ToothNum='"+toothNum+"' "
				+"AND procedurelog.PatNum="+patNum.ToString()+" "
				+"AND procedurelog.ProcStatus <> "+POut.Int((int)ProcStat.D)+" "//Not deleted procedures
				+"AND procedurecode.PaintType=1";//extraction
			table=Db.GetTable(command);
			if(table.Rows[0][0].ToString()!="0") {
				return true;
			}
			return false;
		}

		public static void AttachToApt(long procNum,long aptNum,bool isPlanned) {
			//No need to check RemotingRole; no call to db.
			List<long> procNums=new List<long>();
			procNums.Add(procNum);
			AttachToApt(procNums,aptNum,isPlanned);
		}

		public static void AttachToApt(List<long> procNums,long aptNum,bool isPlanned) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),procNums,aptNum,isPlanned);
				return;
			}
			if(procNums.Count==0) {
				return;
			}
			string command="UPDATE procedurelog SET ";
			if(isPlanned) {
				command+="PlannedAptNum";
			}
			else {
				command+="AptNum";
			}
			command+="="+POut.Long(aptNum)+" WHERE ";
			for(int i=0;i<procNums.Count;i++) {
				if(i>0) {
					command+=" OR ";
				}
				command+="ProcNum="+POut.Long(procNums[i]);
			}
			Db.NonQ(command);
		}

		public static void DetachFromApt(List<long> procNums,bool isPlanned) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),procNums,isPlanned);
				return;
			}
			if(procNums.Count==0) {
				return;
			}
			string command="UPDATE procedurelog SET ";
			if(isPlanned) {
				command+="PlannedAptNum";
			}
			else {
				command+="AptNum";
			}
			command+="=0 WHERE ";
			for(int i=0;i<procNums.Count;i++) {
				if(i>0) {
					command+=" OR ";
				}
				command+="ProcNum="+POut.Long(procNums[i]);
			}
			Db.NonQ(command);
		}

		public static void DetachFromInvoice(long statementNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),statementNum);
				return;
			}
			string command="UPDATE procedurelog SET StatementNum=0 WHERE StatementNum="+POut.Long(statementNum);
			Db.NonQ(command);
		}

		public static void DetachAllFromInvoices(List<long> listStatementNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),listStatementNums);
				return;
			}
			if(listStatementNums==null || listStatementNums.Count==0) {
				return;
			}
			string command="UPDATE procedurelog SET StatementNum=0 WHERE StatementNum IN ("+string.Join(",",listStatementNums.Select(x => POut.Long(x)))+")";
			Db.NonQ(command);
		}

		public static Procedure GetProcForPatByToothSurfStat(long patNum,int toothNum,string surf,ProcStat procStat) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<Procedure>(MethodBase.GetCurrentMethod(),patNum,toothNum,surf,procStat);
			}
			string command="SELECT * FROM procedurelog "
				+"WHERE PatNum="+POut.Long(patNum)+" "
				+"AND Surf='"+POut.String(surf)+"' "
				+"AND ToothNum='"+POut.Int(toothNum)+"' "
				+"AND ProcStatus="+POut.Int((int)procStat);
			return Crud.ProcedureCrud.SelectOne(command);
		}



		//--------------------Taken from Procedure class--------------------------------------------------


		/*
		///<summary>Gets allowedOverride for this procedure based on supplied claimprocs. Includes all claimproc types.  Only used in main TP module when calculating PPOs. The claimProc array typically includes all claimProcs for the patient, but must at least include all claimprocs for this proc.</summary>
		public static double GetAllowedOverride(Procedure proc,ClaimProc[] claimProcs,int priPlanNum) {
			//double retVal=0;
			for(int i=0;i<claimProcs.Length;i++) {
				if(claimProcs[i].ProcNum==proc.ProcNum && claimProcs[i].PlanNum==priPlanNum) {
					return claimProcs[i].AllowedOverride;
					//retVal+=claimProcs[i].WriteOff;
				}
			}
			return 0;//retVal;
		}*/

		/*
		///<summary>Gets total writeoff for this procedure based on supplied claimprocs. Includes all claimproc types.  Only used in main TP module. The claimProc array typically includes all claimProcs for the patient, but must at least include all claimprocs for this proc.</summary>
		public static double GetWriteOff(Procedure proc,List<ClaimProc> claimProcs) {
			//No need to check RemotingRole; no call to db.
			double retVal=0;
			for(int i=0;i<claimProcs.Count;i++) {
				if(claimProcs[i].ProcNum==proc.ProcNum) {
					retVal+=claimProcs[i].WriteOff;
				}
			}
			return retVal;
		}*/

		///<summary>Used in ContrAccount.CreateClaim when validating selected procedures. Returns true if there is any claimproc for this procedure and plan which is marked NoBillIns.  The claimProcList can be all claimProcs for the patient or only those attached to this proc. Will be true if any claimProcs attached to this procedure are set NoBillIns.</summary>
		public static bool NoBillIns(Procedure proc,List<ClaimProc> claimProcList,long planNum) {
			//No need to check RemotingRole; no call to db.
			if(proc==null) {
				return false;
			}
			for(int i=0;i<claimProcList.Count;i++) {
				if(claimProcList[i].ProcNum==proc.ProcNum
					&& claimProcList[i].PlanNum==planNum
					&& claimProcList[i].NoBillIns) {
					return true;
				}
			}
			return false;
		}

		///<summary>Called from FormProcEdit to signal when to disable much of the editing in that form.  If the procedure is 'AttachedToClaim' then user
		///should not change it very much.  Also prevents user from Invalidating a locked procedure if attached to a claim.  The claimProcList can be all
		///claimProcs for the patient or only those attached to this proc.  Ignore preauth claims by setting isPreauthIncluded to false.</summary>
		public static bool IsAttachedToClaim(Procedure proc,List<ClaimProc> claimProcList,bool isPreauthIncluded=true) {
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<claimProcList.Count;i++) {
				if(claimProcList[i].ProcNum==proc.ProcNum
					&& claimProcList[i].ClaimNum>0
					&& (claimProcList[i].Status==ClaimProcStatus.CapClaim
					|| claimProcList[i].Status==ClaimProcStatus.NotReceived
					|| (claimProcList[i].Status==ClaimProcStatus.Preauth && isPreauthIncluded)
					|| claimProcList[i].Status==ClaimProcStatus.Received
					|| claimProcList[i].Status==ClaimProcStatus.Supplemental
					)) {
					return true;
				}
			}
			return false;
		}

		///<summary>Only called from FormProcEdit.  When attached  to a claim and user clicks Edit Anyway, we need to know the oldest claim date for security reasons.  The claimProcsForProc should only be claimprocs for this procedure.</summary>
		public static DateTime GetOldestClaimDate(List<ClaimProc> claimProcsForProc) {
			//No need to check RemotingRole; no call to db.
			Claim claim;
			DateTime retVal=DateTime.Today;
			for(int i=0;i<claimProcsForProc.Count;i++) {
				if(claimProcsForProc[i].ClaimNum==0){
					continue;
				}
				if(claimProcsForProc[i].Status==ClaimProcStatus.CapClaim
					|| claimProcsForProc[i].Status==ClaimProcStatus.NotReceived
					|| claimProcsForProc[i].Status==ClaimProcStatus.Preauth
					|| claimProcsForProc[i].Status==ClaimProcStatus.Received
					|| claimProcsForProc[i].Status==ClaimProcStatus.Supplemental
					) 
				{
					claim=Claims.GetClaim(claimProcsForProc[i].ClaimNum);
					if(claim.DateSent<retVal){
						retVal=claim.DateSent;
					}
				}
			}
			return retVal;
		}

		///<summary>Only called from FormProcEditAll to signal when to disable much of the editing in that form. If the procedure is 'AttachedToClaim' then user should not change it very much.  The claimProcList can be all claimProcs for the patient or only those attached to this proc.</summary>
		public static bool IsAttachedToClaim(List<Procedure> procList,List<ClaimProc> claimprocList) {
			//No need to check RemotingRole; no call to db.
			for(int j=0;j<procList.Count;j++) {
				if(IsAttachedToClaim(procList[j],claimprocList)) {
					return true;
				}
			}
			return false;
		}

		///<summary>Queries the database to determine if this procedure is attached to a claim already.</summary>
		public static bool IsAttachedToClaim(long procNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod(),procNum);
			}
			string command="SELECT COUNT(*) FROM claimproc "
				+"WHERE ProcNum="+POut.Long(procNum)+" "
				+"AND ClaimNum>0";
			DataTable table=Db.GetTable(command);
			if(table.Rows[0][0].ToString()=="0") {
				return false;
			}
			return true;
		}

		///<summary>Used in ContrAccount.CreateClaim to validate that procedure is not already attached to a claim for this specific insPlan.  The claimProcList can be all claimProcs for the patient or only those attached to this proc.</summary>
		public static bool IsAlreadyAttachedToClaim(Procedure proc,List<ClaimProc> claimProcList,long insSubNum) {
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<claimProcList.Count;i++) {
				if(claimProcList[i].ProcNum==proc.ProcNum
					&& claimProcList[i].InsSubNum==insSubNum
					&& claimProcList[i].ClaimNum>0
					&& claimProcList[i].Status!=ClaimProcStatus.Preauth) {
					return true;
				}
			}
			return false;
		}

		public static bool IsReferralAttached(long referralNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod(),referralNum);
			}
			string command="SELECT COUNT(*) FROM procedurelog WHERE OrderingReferralNum="+POut.Long(referralNum);
 			if(Db.GetCount(command)=="0") {
				return false;
			}
			return true;
		}

		///<summary>Only used in ContrAccount.OnInsClick to automate selection of procedures.  Returns true if this procedure should be selected.  This happens if there is at least one claimproc attached for this inssub that is an estimate, and it is not set to NoBillIns.  The list can be all ClaimProcs for patient, or just those for this procedure. The plan is the primary plan.</summary>
		public static bool NeedsSent(long procNum,long insSubNum,List<ClaimProc> claimProcList) {
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<claimProcList.Count;i++) {
				if(claimProcList[i].ProcNum==procNum
					&& !claimProcList[i].NoBillIns
					&& claimProcList[i].InsSubNum==insSubNum
					&& claimProcList[i].Status==ClaimProcStatus.Estimate) 
				{
					return true;
				}
			}
			return false;
		}

		///<summary>Only used in ContrAccount.CreateClaim and FormRepeatChargeUpdate.CreateClaim to decide whether a given procedure has an estimate that can be used to attach to a claim for the specified plan.  Returns a valid claimProc if this procedure has an estimate attached that is not set to NoBillIns.  The list can be all ClaimProcs for patient, or just those for this procedure. Returns null if there are no claimprocs that would work.</summary>
		public static ClaimProc GetClaimProcEstimate(long procNum,List<ClaimProc> claimProcList,InsPlan plan,long insSubNum) {
			//No need to check RemotingRole; no call to db.
			//bool matchOfWrongType=false;
			for(int i=0;i<claimProcList.Count;i++) {
				if(claimProcList[i].ProcNum==procNum
					&& !claimProcList[i].NoBillIns
					&& claimProcList[i].PlanNum==plan.PlanNum
					&& claimProcList[i].InsSubNum==insSubNum) 
				{
					if(plan.PlanType=="c") {
						if(claimProcList[i].Status==ClaimProcStatus.CapComplete) {
							return claimProcList[i];
						}
					}
					else {//any type except capitation
						if(claimProcList[i].Status==ClaimProcStatus.Estimate) {
							return claimProcList[i];
						}
					}
				}
			}
			return null;
		}

		/// <summary>Used by GetProcsForSingle and GetProcsMultApts to generate a short string description of a procedure.</summary>
		public static string ConvertProcToString(long codeNum,string surf,string toothNum,bool forAccount) {
			//No need to check RemotingRole; no call to db.
			string strLine="";
			ProcedureCode code=ProcedureCodes.GetProcCode(codeNum);
			switch(code.TreatArea) {
				case TreatmentArea.Surf:
					if(!forAccount) {
						strLine+="#"+Tooth.ToInternat(toothNum)+"-";//"#12-"
					}
					strLine+=Tooth.SurfTidyFromDbToDisplay(surf,toothNum);//"MOD-"
				break;
				case TreatmentArea.Tooth:
					if(!forAccount) {
						strLine+="#"+Tooth.ToInternat(toothNum)+"-";//"#12-"
					}
					break;
				default://area 3 or 0 (mouth)
					break;
				case TreatmentArea.Quad:
					strLine+=surf+"-";//"UL-"
					break;
				case TreatmentArea.Sextant:
					strLine+="S"+surf+"-";//"S2-"
					break;
				case TreatmentArea.Arch:
					strLine+=surf+"-";//"U-"
					break;
				case TreatmentArea.ToothRange:
					//strLine+=table.Rows[j][13].ToString()+" ";//don't show range
					break;
			}//end switch
			if(!forAccount) {
				strLine+=" "+code.AbbrDesc;
			}
			else if(code.LaymanTerm!=""){
				strLine+=" "+code.LaymanTerm;
			}
			else{
				strLine+=" "+code.Descript;
			}
			return strLine;
		}

		///<summary>Used to display procedure descriptions on appointments. The returned string also includes surf and toothNum.</summary>
		public static string GetDescription(Procedure proc) {
			//No need to check RemotingRole; no call to db.
			return ConvertProcToString(proc.CodeNum,proc.Surf,proc.ToothNum,false);
		}

		///<Summary>Supply the list of procedures attached to the appointment.  It will loop through each and assign the correct provider.
		///Also sets clinic.  Also sets procDate for TP procs.  js 7/24/12 This is not supposed to be called if the appointment is complete.</Summary>
		public static void SetProvidersInAppointment(Appointment apt,List<Procedure> procList) {
			//No need to check RemotingRole; no call to db.
			Procedure changedProc;
			for(int i=0;i<procList.Count;i++) {
				changedProc=procList[i].Copy();
				if(changedProc.ProcStatus==ProcStat.C && !Security.IsAuthorized(Permissions.ProcComplEdit,changedProc.ProcDate,true)) {
					continue;
				}
				changedProc=UpdateProcInAppointment(apt,changedProc);
				Update(changedProc,procList[i]);//won't go to db unless a field has changed.
			}
		}

		///<summary>Sets the provider and clinic for a proc based on the appt to which it is attached.  Also sets ProcDate for TP procs.  Changes are
		///reflected in proc returned, but not saved to the db (for synch later).</summary>
		public static Procedure UpdateProcInAppointment(Appointment apt,Procedure proc) {
			//No need to check RemotingRole; no call to db.
			if(proc.ProcStatus==ProcStat.C && !Security.IsAuthorized(Permissions.ProcComplEdit,proc.ProcDate,true)) {
				//This check is redundant but helps future calls to not miss the security check.
				return proc;//Don't make any changes to procedure.
			}
			ProcedureCode procCode=ProcedureCodes.GetProcCode(proc.CodeNum);
			if(procCode.ProvNumDefault!=0) {
				proc.ProvNum=procCode.ProvNumDefault;//Override ProvNum if there is a default provider for procCode
			}
			else if(apt.ProvHyg==0 || !procCode.IsHygiene) {//if either appt doesn't have a higiene prov or the proc is not a hygiene proc
				proc.ProvNum=apt.ProvNum;
			}
			else {//if the appointment has a hygiene provider and the proc IsHygiene
				proc.ProvNum=apt.ProvHyg;
			}
			proc.ClinicNum=apt.ClinicNum;
			if(proc.ProcStatus==ProcStat.TP) {
				proc.ProcDate=apt.AptDateTime;
			}
			return proc;
		}

		///<summary>Gets a list of procedures representing extracted teeth.  Status of C,EC,orEO. Includes procs with toothNum "1"-"32".  Will not include procs with procdate before 1880.  Used for Canadian e-claims instead of the usual ToothInitials.GetMissingOrHiddenTeeth, because Canada requires dates on the extracted teeth.  Supply all procedures for the patient.</summary>
		public static List<Procedure> GetCanadianExtractedTeeth(List<Procedure> procList) {
			//No need to check RemotingRole; no call to db.
			List<Procedure> extracted=new List<Procedure>();
			ProcedureCode procCode;
			for(int i=0;i<procList.Count;i++) {
				if(procList[i].ProcStatus!=ProcStat.C && procList[i].ProcStatus!=ProcStat.EC && procList[i].ProcStatus!=ProcStat.EO) {
					continue;
				}
				if(!Tooth.IsValidDB(procList[i].ToothNum)) {
					continue;
				}
				if(Tooth.IsSuperNum(procList[i].ToothNum)) {
					continue;
				}
				if(Tooth.IsPrimary(procList[i].ToothNum)) {
					continue;
				}
				if(procList[i].ProcDate.Year<1880) {
					continue;
				}
				procCode=ProcedureCodes.GetProcCode(procList[i].CodeNum);
				if(procCode.TreatArea!=TreatmentArea.Tooth) {
					continue;
				}
				if(procCode.PaintType!=ToothPaintingType.Extraction) {
					continue;
				}
#if DEBUG //Needed for certification so that we can manually change the order that extrated teeth are sent, even throuh this won't matter in production.
				int j=0;
				while(j<extracted.Count) {
					if(extracted[j].DateTStamp>=procList[i].DateTStamp) {
						break;
					}
					j++;
				}
				extracted.Insert(j,procList[i].Copy());
#endif
			}
			return extracted;
		}

		///<summary>Takes the list of all procedures for the patient, and finds any that are attached as lab procs to that proc.</summary>
		public static List<Procedure> GetCanadianLabFees(long procNumLab,List<Procedure> procList){
			//No need to check RemotingRole; no call to db.
			List<Procedure> retVal=new List<Procedure>();
			if(procNumLab==0) {//Ignore regular procedures.
				return retVal;
			}
			for(int i=0;i<procList.Count;i++) {
				if(procList[i].ProcNumLab==procNumLab) {
					retVal.Add(procList[i]);
				}
			}
			return retVal;
		}

		///<summary>Pulls the lab fees for the given procnums directly from the database.</summary>
		public static List<Procedure> GetCanadianLabFees(List<long> listProcNums){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),listProcNums);
			}
			if(listProcNums.Count==0) {
				return new List<Procedure>();
			}
			return Crud.ProcedureCrud.SelectMany("SELECT * FROM procedurelog WHERE ProcStatus<>"+POut.Int((int)ProcStat.D)+" AND ProcNumLab IN ("+string.Join(",",listProcNums)+")");
		}

		///<summary>Pulls the lab fees for the given procnum directly from the database.</summary>
		public static List<Procedure> GetCanadianLabFees(long procNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),procNum);
			}
			string command="SELECT * FROM procedurelog WHERE ProcStatus<>"+POut.Int((int)ProcStat.D)+" AND ProcNumLab="+POut.Long(procNum);
			return Crud.ProcedureCrud.SelectMany(command);
		}

		/*
		///<summary>InsEstTotal or override is retrieved from supplied claimprocs. Includes annual max and deductible.  The claimProc array typically includes all claimProcs for the patient, but must at least include the claimprocs for this proc that we need.  Will always return a meaningful value rather than -1.</summary>
		public static double GetEst(Procedure proc,List<ClaimProc> claimProcs,int planNum) {
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<claimProcs.Count;i++) {
				//adjustments automatically ignored since no ProcNum
				if(claimProcs[i].Status==ClaimProcStatus.CapClaim
					||claimProcs[i].Status==ClaimProcStatus.Preauth
					||claimProcs[i].Status==ClaimProcStatus.Supplemental) {
					continue;
				}
				if(claimProcs[i].ProcNum!=proc.ProcNum) {
					continue;
				}
				if(claimProcs[i].PlanNum!=planNum) {
					continue;
				}
				if(claimProcs[i].InsEstTotalOverride != -1){
					return claimProcs[i].InsEstTotalOverride;
				}
				return claimProcs[i].InsEstTotal;
			}
			return 0;
		}*/

		///<summary>Only fees, not estimates.  Returns number of fees changed.</summary>
		public static long GlobalUpdateFees(List<Fee> listFees) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetLong(MethodBase.GetCurrentMethod(),listFees);
			}
			//if we ran the DBM ProcedurelogWithInvalidProvNum, we wouldn't have to worry about this method using the practice default prov
			ODEvent.Fire(new ODEventArgs("FeeSchedUpdate",Lans.g("FeeSchedTools","Getting table of fees to update")+"..."));
			FeeCache feeCache = new FeeCache(listFees);
			//The query below must join 5 tables; procedurelog, patient, patplan, inssub, insplan
			string command="SELECT procedurelog.ProcNum,procedurelog.CodeNum,procedurelog.ProvNum,procedurelog.ClinicNum,procedurelog.ProcFee,"
				+"patient.FeeSched PatFeeSched,insplan.PlanType,insplan.FeeSched PlanFeeSched "
				+"FROM procedurelog "
				+"INNER JOIN patient ON patient.PatNum=procedurelog.PatNum "
				+"LEFT JOIN patplan ON patplan.PatNum=procedurelog.PatNum AND patplan.Ordinal=1 "
				+"LEFT JOIN inssub ON inssub.InsSubNum=patplan.InsSubNum "
				+"LEFT JOIN insplan ON insplan.PlanNum=inssub.PlanNum "
				+"WHERE procedurelog.ProcStatus="+POut.Int((int)ProcStat.TP)+" ";
			if(DataConnection.DBtype==DatabaseType.MySql) {
				//because sometimes a pat can have more than one primary patplan and DBM only has "manual fix needed" for this problem
				command+="GROUP BY procedurelog.ProcNum";
			}
			else {//oracle
				//because sometimes a pat can have more than one primary patplan and DBM only has "manual fix needed" for this problem
				command+="GROUP BY procedurelog.ProcNum,procedurelog.CodeNum,procedurelog.ProvNum,procedurelog.ClinicNum,procedurelog.ProcFee,"
					+"patient.FeeSched PatFeeSched,insplan.PlanType,insplan.FeeSched PlanFeeSched";
			}
			DataTable table=Db.GetTable(command);
			long procProvNum;
			long feeSchedNum;//just for convenience and to make code more legible
			double insfee;
			double standardfee;
			double newFee;
			long clinicNum;
			long codeNum;
			long procProvFeeSchedNum;
			long rowsChanged=0;
			Provider practDefaultProv=Providers.GetProv(PrefC.GetLong(PrefName.PracticeDefaultProv));
			Dictionary<long,long> dictProv=ProviderC.GetListLong().ToDictionary(x => x.ProvNum,x => x.FeeSched);
			StringBuilder commandCur=new StringBuilder();
			int rowNum=0;
			foreach(DataRow rowCur in table.Rows) {
				procProvNum=PIn.Long(rowCur["ProvNum"].ToString());
				clinicNum=PIn.Long(rowCur["ClinicNum"].ToString());
				codeNum=PIn.Long(rowCur["CodeNum"].ToString());
				standardfee=0;
				procProvFeeSchedNum=0;
				dictProv.TryGetValue(procProvNum,out procProvFeeSchedNum);
				if(rowCur["PlanType"].ToString()=="p") {//PPO
					if(procProvFeeSchedNum>0) {//because there are sometimes procedures with invalid ProvNum or ProvNum==0, use practice default prov fee
						standardfee=feeCache.GetAmount0(codeNum,procProvFeeSchedNum,clinicNum,procProvNum);
					}
					else {//this is what running the DBM ProcedurelogWithInvalidProvNum would result in, setting proc.ProvNum to practice default prov.ProvNum
						standardfee=feeCache.GetAmount0(codeNum,practDefaultProv.FeeSched,clinicNum,practDefaultProv.ProvNum);
					}
				}
				feeSchedNum=PIn.Long(rowCur["PlanFeeSched"].ToString());
				if(feeSchedNum==0) {
					feeSchedNum=PIn.Long(rowCur["PatFeeSched"].ToString());
					if(feeSchedNum==0) {
						feeSchedNum=procProvFeeSchedNum;
					}
				}
				insfee=feeCache.GetAmount0(codeNum,feeSchedNum,clinicNum,procProvNum);
				newFee=Math.Max(standardfee,insfee);//if planType is not PPO, standardfee will be 0 and insfee will always be selected
				if(newFee.IsEqual(PIn.Double(rowCur["ProcFee"].ToString()))) {
					continue;
				}
				commandCur.Append("UPDATE procedurelog SET ProcFee='"+POut.Double(newFee)+"' WHERE ProcNum="+POut.Long(PIn.Long(rowCur["ProcNum"].ToString()))+";");
				rowsChanged++;
				if(rowsChanged%1000==999) {//about 400,000 characters in length. makes approx 1/1000th of the trips to the db.
					Db.NonQ(commandCur.ToString());
					commandCur.Clear();
				}
				ODEvent.Fire(new ODEventArgs("FeeSchedUpdate",new ProgressBarHelper(Lans.g("FeeSchedTools","Updating fees, please wait")+"..."," ",rowNum,table.Rows.Count,ProgBarStyle.Continuous)));
				rowNum++;
			}
			if(!string.IsNullOrEmpty(commandCur.ToString())) {
				Db.NonQ(commandCur.ToString());
			}
			return rowsChanged;
		}

		///<summary>Used from TP to get a list of all TP procs, ordered by their treatment plan's priority, (conditionally) toothnum.</summary>
		public static Procedure[] GetListTPandTPi(List<Procedure> procList,List<TreatPlanAttach> listTreatPlanAttaches=null) {
			//No need to check RemotingRole; no call to db.
			return SortListByTreatPlanPriority(procList.FindAll(x => x.ProcStatus==ProcStat.TP || x.ProcStatus==ProcStat.TPi),listTreatPlanAttaches);
		}
		
		///<summary>Sorts the given list based on the procedure's priority, tooth, date, and procnum.
		///SortListByTreatPlanPriority() should be the only method of sorting procedures that need to emulate the treatment plan module.
		///This is to prevent recurring bugs due to different sort methodology.</summary>
		public static Procedure[] SortListByTreatPlanPriority(List<Procedure> listProcs,List<TreatPlanAttach> listTreatPlanAttaches=null) {
			//No need to check RemotingRole; no call to db.
			Dictionary<long,int> dictPriorities=new Dictionary<long, int>();
			Dictionary<long,int> dictProcNumPriority=new Dictionary<long, int>();
			bool hasTreatPlanAttaches=false;
			//Check to see if a list of TreatPlanAttaches was passed in.  If so, use the treatplanattach priorities instead of the priority of the procedure that's in the database.
			//This is used for multiple active treatment plans where we need to display a sorted priority even though that procedure might not have the same priority in another
			//saved treatment plan and thus would have a different priority in the database.
			if(listTreatPlanAttaches!=null) {
				hasTreatPlanAttaches=true;
				dictPriorities=DefC.GetListLong(DefCat.TxPriorities).ToDictionary(x=>x.DefNum,x=>x.ItemOrder);
				listTreatPlanAttaches.ForEach(x => dictProcNumPriority[x.ProcNum]=(x.Priority==0 ? -1 : dictPriorities[x.Priority])); 
			}
			//Procedure code is purposefully not included in the sorting of this list.  It will ruin PrefName.TreatPlanSortByTooth sorting.
			return listProcs
			.OrderBy(x => (hasTreatPlanAttaches ? dictProcNumPriority[x.ProcNum] : x.PriorityOrder)<0)
			.ThenBy(x => (hasTreatPlanAttaches ? dictProcNumPriority[x.ProcNum] : x.PriorityOrder))
			//.ThenBy(x => PrefC.IsTreatPlanSortByTooth ? x.ToothRange : 0)
			.ThenBy(x => PrefC.IsTreatPlanSortByTooth ? Tooth.ToInt(x.ToothNum) : 0)//Sorting by a constant causes nothing to happen.
			.ThenBy(x => x.ProcDate)
			.ThenBy(x => x.ProcNum)//This is necessary in case isTreatPlanSortByTooth is false, and to break any ties between the other sorting methods.
			.ToArray();
		}

		public static void ComputeEstimates(Procedure proc,long patNum,List<ClaimProc> claimProcs,bool isInitialEntry,List<InsPlan> planList,List<PatPlan> patPlans,List<Benefit> benefitList,int patientAge,List<InsSub> subList) {
			//This is a stub that needs revision.
			ComputeEstimates(proc,patNum,ref claimProcs,isInitialEntry,planList,patPlans,benefitList,null,null,true,patientAge,subList);
		}

		///<summary>Used whenever a procedure changes or a plan changes.  All estimates for a given procedure must be updated. This frequently includes adding claimprocs, but can also just edit the appropriate existing claimprocs. Skips status=Adjustment,CapClaim,Preauth,Supplemental.  Also fixes date,status,and provnum if appropriate.  The claimProc list only needs to include claimprocs for this proc, although it can include more.  Only set isInitialEntry true from Chart module; it is for cap procs.  loopList only contains information about procedures that come before this one in a list such as TP or claim.</summary>
		public static void ComputeEstimates(Procedure proc,long patNum,ref List<ClaimProc> claimProcs,bool isInitialEntry,List<InsPlan> planList,List<PatPlan> patPlans,List<Benefit> benefitList,List<ClaimProcHist> histList,List<ClaimProcHist> loopList,bool saveToDb,int patientAge,List<InsSub> subList) {
			//No need to check RemotingRole; no call to db.
			bool isHistorical=false;
			if(proc.ProcDate<DateTime.Today && proc.ProcStatus==ProcStat.C) {
				isHistorical=true;//Don't automatically create an estimate for completed procedures, especially if they are older than today.  Very important after a conversion from another software.
				//Special logic in place only for capitation plans:
				if(planList.Any(x => x.PlanType=="c") //11/19/2012 js We had a specific complaint where changing plan type to capitation automatically added WOs to historical procs.
				   && !claimProcs.Any(x => x.ProcNum==proc.ProcNum && new[] {ClaimProcStatus.CapClaim,ClaimProcStatus.CapComplete,ClaimProcStatus.CapEstimate}.Contains(x.Status))) 
				{
					//If there are any capitation plans but no capitation claimproc.statuses then return.
					//04/02/2013 Jason- To relax this filter for offices that enter treatment a few days after it's done, we will see if any capitation statuses exist.
					return;//There are no capitation claimprocs for this procedure, therefore we don't want to touch/damage this proc.
				}
			}
			//first test to see if each estimate matches an existing patPlan (current coverage),
			//delete any other estimates
			for(int i=0;i<claimProcs.Count;i++) {
				if(claimProcs[i].ProcNum!=proc.ProcNum) {
					continue;
				}
				if(claimProcs[i].PlanNum==0) {
					continue;
				}
				if(claimProcs[i].Status==ClaimProcStatus.CapClaim
					||claimProcs[i].Status==ClaimProcStatus.Preauth
					||claimProcs[i].Status==ClaimProcStatus.Supplemental) {
					continue;
					//ignored: adjustment
					//included: capComplete,CapEstimate,Estimate,NotReceived,Received
				}
				if(claimProcs[i].Status!=ClaimProcStatus.Estimate && claimProcs[i].Status!=ClaimProcStatus.CapEstimate) {
					continue;
				}
				bool planIsCurrent=false;
				for(int p=0;p<patPlans.Count;p++) {
					if(patPlans[p].InsSubNum==claimProcs[i].InsSubNum
						&& InsSubs.GetSub(patPlans[p].InsSubNum,subList).PlanNum==claimProcs[i].PlanNum) 
					{
						planIsCurrent=true;
						break;
					}
				}
				//If claimProc estimate is for a plan that is not current, delete it
				if(!planIsCurrent) {
					if(saveToDb) {
						ClaimProcs.Delete(claimProcs[i]);
					}
					else {
						claimProcs[i].DoDelete=true;
					}
				}
			}
			InsPlan planCur;
			InsSub subCur;
			//bool estExists;
			bool cpAdded=false;
			//loop through all patPlans (current coverage), and add any missing estimates
			for(int p=0;p<patPlans.Count;p++) {//typically, loop will only have length of 1 or 2
				if(isHistorical) {
					break;
				}
				PatPlan patPlanCur=patPlans[p];
				//test to see if estimate exists
				if(claimProcs.Any(x => x.ProcNum==proc.ProcNum && x.PlanNum!=0 && x.InsSubNum==patPlanCur.InsSubNum
					&& !new[] {ClaimProcStatus.CapClaim,ClaimProcStatus.Preauth,ClaimProcStatus.Supplemental }.Contains(x.Status)))
				{
					continue;//estimate exists
				}
				//estimate is missing, so add it.
				subCur=InsSubs.GetSub(patPlanCur.InsSubNum,subList);
				planCur=InsPlans.GetPlan(subCur.PlanNum,planList);
				if(planCur==null){//subCur can never be null) {//??
					continue;//??
				}
				ClaimProc cp=new ClaimProc();
				cp.ProcNum=proc.ProcNum;
				cp.PatNum=patNum;
				cp.ProvNum=proc.ProvNum;
				if(planCur.PlanType=="c") {
					if(proc.ProcStatus==ProcStat.C) {
						cp.Status=ClaimProcStatus.CapComplete;
					}
					else {
						cp.Status=ClaimProcStatus.CapEstimate;//this may be changed below
					}
				}
				else {
					cp.Status=ClaimProcStatus.Estimate;
				}
				cp.PlanNum=planCur.PlanNum;
				cp.InsSubNum=subCur.InsSubNum;
				cp.DateCP=proc.ProcDate;
				cp.AllowedOverride=-1;
				cp.PercentOverride=-1;
				cp.NoBillIns=ProcedureCodes.GetProcCode(proc.CodeNum).NoBillIns;
				cp.PaidOtherIns=-1;
				cp.CopayOverride=-1;
				cp.ProcDate=proc.ProcDate;
				cp.BaseEst=0;
				cp.InsEstTotal=0;
				cp.InsEstTotalOverride=-1;
				cp.DedEst=-1;
				cp.DedEstOverride=-1;
				cp.PaidOtherInsOverride=-1;
				cp.WriteOffEst=-1;
				cp.WriteOffEstOverride=-1;
				//ComputeBaseEst will fill AllowedOverride,Percentage,CopayAmt,BaseEst
				if(saveToDb) {
					ClaimProcs.Insert(cp);
				}
				else {
					claimProcs.Add(cp);//this newly added cp has no ClaimProcNum and is not yet in the db.
				}
				cpAdded=true;
			}
			//if any were added, refresh the list
			if(cpAdded && saveToDb) {//no need to refresh the list if !saveToDb, because list already made current.
				claimProcs=ClaimProcs.Refresh(patNum);
			}
			double paidOtherInsEstTotal=0;
			double paidOtherInsBaseEst=0;
			double writeOffEstOtherIns=0;
			//because secondary claimproc might come before primary claimproc in the list, we cannot simply loop through the claimprocs
			ComputeForOrdinal(1,claimProcs,proc,planList,isInitialEntry,ref paidOtherInsEstTotal,ref paidOtherInsBaseEst,ref writeOffEstOtherIns,
				patPlans,benefitList,histList,loopList,saveToDb,patientAge);
			ComputeForOrdinal(2,claimProcs,proc,planList,isInitialEntry,ref paidOtherInsEstTotal,ref paidOtherInsBaseEst,ref writeOffEstOtherIns,
				patPlans,benefitList,histList,loopList,saveToDb,patientAge);
			ComputeForOrdinal(3,claimProcs,proc,planList,isInitialEntry,ref paidOtherInsEstTotal,ref paidOtherInsBaseEst,ref writeOffEstOtherIns,
				patPlans,benefitList,histList,loopList,saveToDb,patientAge);
			ComputeForOrdinal(4,claimProcs,proc,planList,isInitialEntry,ref paidOtherInsEstTotal,ref paidOtherInsBaseEst,ref writeOffEstOtherIns,
				patPlans,benefitList,histList,loopList,saveToDb,patientAge);
			//At this point, for a PPO with secondary, the sum of all estimates plus primary writeoff might be greater than fee.
			if(patPlans.Count>1){
				subCur=InsSubs.GetSub(patPlans[0].InsSubNum,subList);
				planCur=InsPlans.GetPlan(subCur.PlanNum,planList);
				if(planCur.PlanType=="p") {
					//claimProcs=ClaimProcs.Refresh(patNum);
					//ClaimProc priClaimProc=null;
					int priClaimProcIdx=-1;
					double sumPay=0;//Either actual or estimate
					for(int i=0;i<claimProcs.Count;i++){
						if(claimProcs[i].ProcNum!=proc.ProcNum){
							continue;
						}
						if(claimProcs[i].Status==ClaimProcStatus.Adjustment
							|| claimProcs[i].Status==ClaimProcStatus.CapClaim
							|| claimProcs[i].Status==ClaimProcStatus.CapComplete
							|| claimProcs[i].Status==ClaimProcStatus.CapEstimate
							|| claimProcs[i].Status==ClaimProcStatus.Preauth)
						{
							continue;
						}
						if(claimProcs[i].PlanNum==planCur.PlanNum && claimProcs[i].WriteOffEst>0){
							priClaimProcIdx=i;
						}
						if(claimProcs[i].Status==ClaimProcStatus.Received
							|| claimProcs[i].Status==ClaimProcStatus.Supplemental ){
							sumPay+=claimProcs[i].InsPayAmt;
						}
						if(claimProcs[i].Status==ClaimProcStatus.Estimate){
							if(!claimProcs[i].InsEstTotalOverride.IsEqual(-1)){
								sumPay+=claimProcs[i].InsEstTotalOverride;
							}
							else{
								sumPay+=claimProcs[i].InsEstTotal;
							}
						}
						if(claimProcs[i].Status==ClaimProcStatus.NotReceived){
							sumPay+=claimProcs[i].InsPayEst;
						}
					}
					//Alter primary WO if needed.
					if(priClaimProcIdx!=-1){
						double procFee=proc.ProcFee*Math.Max(1,proc.BaseUnits+proc.UnitQty);
						if(sumPay+claimProcs[priClaimProcIdx].WriteOffEst > procFee) {
							double writeOffEst=procFee-sumPay;
							if(writeOffEst<0) {
								writeOffEst=0;
							}
							claimProcs[priClaimProcIdx].WriteOffEst=writeOffEst;
							if(saveToDb){
								ClaimProcs.Update(claimProcs[priClaimProcIdx]);
							}
						}
					}
				}
			}
		}

		///<summary>Passing in 4 will compute for 4 as well as any other situation such as dropped plan.</summary>
		private static void ComputeForOrdinal(int ordinal,List<ClaimProc> claimProcs,Procedure proc,List<InsPlan> planList,bool isInitialEntry,
			ref double paidOtherInsEstTotal,ref double paidOtherInsBaseEst,ref double writeOffEstOtherIns,
			List<PatPlan> patPlans,List<Benefit> benefitList,List<ClaimProcHist> histList,List<ClaimProcHist> loopList,bool saveToDb,int patientAge) {
			//No need to check RemotingRole; no call to db.
			InsPlan PlanCur;
			PatPlan patplan;
			for(int i=0;i<claimProcs.Count;i++) {
				if(claimProcs[i].ProcNum!=proc.ProcNum) {
					continue;
				}
				PlanCur=InsPlans.GetPlan(claimProcs[i].PlanNum,planList);
				if(PlanCur==null) {
					continue;//in older versions it still did a couple of small things even if plan was null, but don't know why
					//example:cap estimate changed to cap complete, and if estimate, then provnum set
					//but I don't see how PlanCur could ever be null
				}
				patplan=PatPlans.GetFromList(patPlans,claimProcs[i].InsSubNum);
				//capitation estimates are always forced to follow the status of the procedure
				if(PlanCur.PlanType=="c"
					&& (claimProcs[i].Status==ClaimProcStatus.CapComplete	|| claimProcs[i].Status==ClaimProcStatus.CapEstimate)) 
				{
					if(isInitialEntry) {
						//this will be switched to CapComplete further down if applicable.
						//This makes ComputeBaseEst work properly on new cap procs w status Complete
						claimProcs[i].Status=ClaimProcStatus.CapEstimate;
					}
					else if(proc.ProcStatus==ProcStat.C) {
						claimProcs[i].Status=ClaimProcStatus.CapComplete;
					}
					else {
						claimProcs[i].Status=ClaimProcStatus.CapEstimate;
					}
				}
				//ignored: adjustment
				//ComputeBaseEst automatically skips: capComplete,Preauth,capClaim,Supplemental
				//does recalc est on: CapEstimate,Estimate,NotReceived,Received
				//the cp is altered within ComputeBaseEst, but not saved.
				if(patplan==null) {//the plan for this claimproc was dropped 
					if(ordinal!=4) {//only process on the fourth round
						continue;
					}
					ClaimProcs.ComputeBaseEst(claimProcs[i],proc,PlanCur,0,
						benefitList,histList,loopList,patPlans,0,0,patientAge,0);
				}
				else if(patplan.Ordinal==1){
					if(ordinal!=1) {
						continue;
					}
					ClaimProcs.ComputeBaseEst(claimProcs[i],proc,PlanCur,patplan.PatPlanNum,
						benefitList,histList,loopList,patPlans,paidOtherInsEstTotal,paidOtherInsBaseEst,patientAge,writeOffEstOtherIns);
					if(claimProcs[i].Status==ClaimProcStatus.Received || claimProcs[i].Status==ClaimProcStatus.Supplemental) {
						paidOtherInsEstTotal+=claimProcs[i].InsPayAmt;
						paidOtherInsBaseEst+=claimProcs[i].InsPayAmt;
						writeOffEstOtherIns+=claimProcs[i].WriteOff;
					}
					else {
						paidOtherInsEstTotal+=claimProcs[i].InsEstTotal;
						paidOtherInsBaseEst+=claimProcs[i].BaseEst;
						writeOffEstOtherIns+=ClaimProcs.GetWriteOffEstimate(claimProcs[i]);
					}
				}
				else if(patplan.Ordinal==2){
					if(ordinal!=2) {
						continue;
					}
					ClaimProcs.ComputeBaseEst(claimProcs[i],proc,PlanCur,patplan.PatPlanNum,
						benefitList,histList,loopList,patPlans,paidOtherInsEstTotal,paidOtherInsBaseEst,patientAge,writeOffEstOtherIns);
					if(claimProcs[i].Status==ClaimProcStatus.Received || claimProcs[i].Status==ClaimProcStatus.Supplemental) {
						paidOtherInsEstTotal+=claimProcs[i].InsPayAmt;
						paidOtherInsBaseEst+=claimProcs[i].InsPayAmt;
						writeOffEstOtherIns+=claimProcs[i].WriteOff;
					}
					else {
						paidOtherInsEstTotal+=claimProcs[i].InsEstTotal;
						paidOtherInsBaseEst+=claimProcs[i].BaseEst;
						writeOffEstOtherIns+=ClaimProcs.GetWriteOffEstimate(claimProcs[i]);
					}
				}
				else if(patplan.Ordinal==3) {
					if(ordinal!=3) {
						continue;
					}
					ClaimProcs.ComputeBaseEst(claimProcs[i],proc,PlanCur,patplan.PatPlanNum,
						benefitList,histList,loopList,patPlans,paidOtherInsEstTotal,paidOtherInsBaseEst,patientAge,writeOffEstOtherIns);
					if(claimProcs[i].Status==ClaimProcStatus.Received || claimProcs[i].Status==ClaimProcStatus.Supplemental) {
						paidOtherInsEstTotal+=claimProcs[i].InsPayAmt;
						paidOtherInsBaseEst+=claimProcs[i].InsPayAmt;
						writeOffEstOtherIns+=claimProcs[i].WriteOff;
					}
					else {
						paidOtherInsEstTotal+=claimProcs[i].InsEstTotal;
						paidOtherInsBaseEst+=claimProcs[i].BaseEst;
						writeOffEstOtherIns+=ClaimProcs.GetWriteOffEstimate(claimProcs[i]);
					}
				}
				else{//patplan.Ordinal is 4 or greater.  Estimate won't be accurate if more than 4 insurances.
					if(ordinal!=4) {
						continue;
					}
					ClaimProcs.ComputeBaseEst(claimProcs[i],proc,PlanCur,patplan.PatPlanNum,
						benefitList,histList,loopList,patPlans,paidOtherInsEstTotal,paidOtherInsBaseEst,patientAge,writeOffEstOtherIns);
				}
				//This was a longstanding problem. I hope there are not other consequences for commenting it out.
				//claimProcs[i].DateCP=proc.ProcDate;
				claimProcs[i].ProcDate=proc.ProcDate;
				claimProcs[i].ClinicNum=proc.ClinicNum;
				//Wish we could do this, but it might change history.  It's needed when changing a completed proc to a different provider.
				//Can't do it here, though, because some people intentionally set provider different on claimprocs.
				//claimProcs[i].ProvNum=proc.ProvNum;
				if(isInitialEntry
					&&claimProcs[i].Status==ClaimProcStatus.CapEstimate
					&&proc.ProcStatus==ProcStat.C) 
				{
					claimProcs[i].Status=ClaimProcStatus.CapComplete;
				}
				//prov only updated if still an estimate
				if(claimProcs[i].Status==ClaimProcStatus.Estimate
					||claimProcs[i].Status==ClaimProcStatus.CapEstimate) {
					claimProcs[i].ProvNum=proc.ProvNum;
				}
				if(saveToDb) {
					ClaimProcs.Update(claimProcs[i]);
				}
			}
		}

		///<summary>After changing important coverage plan info, this is called to recompute estimates for all procedures for this patient.</summary>
		public static void ComputeEstimatesForAll(long patNum,List<ClaimProc> claimProcs,List<Procedure> procs,List<InsPlan> planList,List<PatPlan> patPlans,List<Benefit> benefitList,int patientAge,List<InsSub> subList) {
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<procs.Count;i++) {
				ComputeEstimates(procs[i],patNum,claimProcs,false,planList,patPlans,benefitList,patientAge,subList);
			}
		}

		///<summary>Loops through each proc in the dictionary.  Dictionary key is the index in the list of procs for appointment edit, only used if called
		///from FormApptEdit.  Does not add notes to a procedure that already has notes. Only called from ProcedureL.SetCompleteInAppt, security checked
		///before calling this.  Also sets provider for each proc and claimproc.  Returns dictIndexInListForProc with changes made to the procs.</summary>
		public static List<Procedure> SetCompleteInAppt(Appointment apt,List<InsPlan> planList,List<PatPlan> patPlans,long siteNum,int patientAge,
			List<Procedure> listProcsForAppt,List<InsSub> subList,Userod curUser)
		{
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Procedure>>(MethodBase.GetCurrentMethod(),apt,planList,patPlans,siteNum,patientAge,listProcsForAppt,subList,curUser);
			}
			if(listProcsForAppt.Count==0) {
				return listProcsForAppt;//Nothing to do.
			}
			List<ClaimProc> claimProcList=ClaimProcs.Refresh(apt.PatNum);
			List<Benefit> benefitList=Benefits.Refresh(patPlans,subList);
			//most recent note will be first in list.
			string command="SELECT * FROM procnote "
				+"WHERE ProcNum IN("+string.Join(",",listProcsForAppt.Select(x => x.ProcNum))+") "
				+"ORDER BY EntryDateTime DESC";
			DataTable rawNotes=Db.GetTable(command);
			ProcedureCode procCode;
			Procedure procOld;
			List<long> encounterProvNums=new List<long>();//for auto-inserting default encounters
			foreach(Procedure procCur in listProcsForAppt) {
				//Should only be procs for this appointment
				//attach the note, if it exists.
				foreach(DataRow row in rawNotes.Rows) {
					if(procCur.ProcNum.ToString()!=row["ProcNum"].ToString()) {
						continue;
					}
					procCur.UserNum=PIn.Long(row["UserNum"].ToString());
					procCur.Note=PIn.String(row["Note"].ToString());
					procCur.SigIsTopaz=PIn.Bool(row["SigIsTopaz"].ToString());
					procCur.Signature=PIn.String(row["Signature"].ToString());
					break;//out of note loop.
				}
				procOld=procCur.Copy();
				procCode=ProcedureCodes.GetProcCode(procCur.CodeNum);
				if(procCode.PaintType==ToothPaintingType.Extraction) {//if an extraction, then mark previous procs hidden
					//SetHideGraphical(procCur);//might not matter anymore
					ToothInitials.SetValue(apt.PatNum,procCur.ToothNum,ToothInitialType.Missing);
				}
				procCur.ProcStatus=ProcStat.C;
				if(procOld.ProcStatus!=ProcStat.C) {
					procCur.ProcDate=apt.AptDateTime.Date;//only change date to match appt if not already complete.
					if(procCur.ProcDate.Year<1880) {
						procCur.ProcDate=MiscData.GetNowDateTime().Date;//Change procdate to today if the appointment date was invalid
					}
					procCur.DateEntryC=DateTime.Now;//this triggers it to set to server time NOW().
					if(procCur.DiagnosticCode=="") {
						procCur.DiagnosticCode=PrefC.GetString(PrefName.ICD9DefaultForNewProcs);
					}
				}
				procCur.PlaceService=(PlaceOfService)PrefC.GetLong(PrefName.DefaultProcedurePlaceService);
				procCur.ClinicNum=apt.ClinicNum;
				procCur.SiteNum=siteNum;
				procCur.PlaceService=Clinics.GetPlaceService(apt.ClinicNum);
				if(procCode.ProvNumDefault!=0) {//Override provider for procedures with a default provider
					procCur.ProvNum=procCode.ProvNumDefault;
				}
				else if(apt.ProvHyg==0 || !procCode.IsHygiene) {//either no hygiene prov on the appt or the proc is not a hygiene proc
					procCur.ProvNum=apt.ProvNum;
				}
				else {//appointment has a hygiene prov and the proc IsHygiene
					procCur.ProvNum=apt.ProvHyg;
				}
				//if procedure was already complete, then don't add more notes.
				if(procOld.ProcStatus!=ProcStat.C) {
					procCur.Note+=ProcCodeNotes.GetNote(procCur.ProvNum,procCur.CodeNum);
				}
				if(Userods.IsUserCpoe(curUser)) {
					//Only change the status of IsCpoe to true.  Never set it back to false for any reason.  Once true, always true.
					procCur.IsCpoe=true;
				}
				if(CultureInfo.CurrentCulture.Name.EndsWith("CA")) {//Canada
					SetCanadianLabFeesCompleteForProc(procCur);
				}
				Plugins.HookAddCode(null,"Procedures.SetCompleteInAppt_procLoop",procCur,procOld);
				Update(procCur,procOld);//Updates payplan charges for the procedure if it went from any status to complete.
				ComputeEstimates(procCur,apt.PatNum,claimProcList,false,planList,patPlans,benefitList,patientAge,subList);
				ClaimProcs.SetProvForProc(procCur,claimProcList);
				//Add provnum to list to create an encounter later. Done to limit calls to DB from Encounters.InsertDefaultEncounter().
				if(procOld.ProcStatus!=ProcStat.C) {//check for distinct later.
					encounterProvNums.Add(procCur.ProvNum);
				}
			}
			//Auto-insert default encounters for the providers that did work on this appointment
			encounterProvNums.Distinct().ToList().ForEach(x => Encounters.InsertDefaultEncounter(apt.PatNum,x,apt.AptDateTime));
			Recalls.Synch(apt.PatNum);
			return listProcsForAppt;
			//Patient pt=Patients.GetPat(apt.PatNum);
			//jsparks-See notes within this method:
			//Reporting.Allocators.AllocatorCollection.CallAll_Allocators(pt.Guarantor);
		}

		///<summary>Returns all the unique diagnostic codes in the list.  If there is less than 12 unique codes then it will pad the list with empty
		///entries if isPadded is true.  Will always place the principal diagnosis as the first item in the list.</summary>
		public static List<string> GetUniqueDiagnosticCodes(List<Procedure> listProcs,bool isPadded) {
			return GetUniqueDiagnosticCodes(listProcs,isPadded,new List<byte>());
		}

		///<summary>Returns all the unique diagnostic codes in the list.  If there is less than 12 unique codes then it will pad the list with empty
		///entries if isPadded is true.  Will always place the principal diagnosis as the first item in the list.  The returned list and
		///listDiagnosticVersions will be the same length upon return.  When returning, listDiagnosticVersions will contain the diagnostic code versions
		///of each code in the returned list, used for allowing the user to mix diagnostic code versions on a single claim.  The listDiagnosticVersions
		///must be a valid list (not null).</summary>
		public static List<string> GetUniqueDiagnosticCodes(List<Procedure> listProcs,bool isPadded,List<byte> listDiagnosticVersions) {
			//No need to check RemotingRole; no call to db.
			List<string> listDiagnosticCodes=new List<string>();
			listDiagnosticVersions.Clear();
			for(int i=0;i<listProcs.Count;i++) {//Ensure that the principal diagnosis is first in the list.
				Procedure proc=listProcs[i];
				if(proc.IsPrincDiag && proc.DiagnosticCode!="") {
					listDiagnosticCodes.Add(proc.DiagnosticCode);
					listDiagnosticVersions.Add(proc.IcdVersion);
					break;
				}
			}
			for(int i=0;i<listProcs.Count;i++) {
				Procedure proc=listProcs[i];
				if(proc.DiagnosticCode!="" && !ExistsDiagnosticCode(listDiagnosticCodes,listDiagnosticVersions,proc.DiagnosticCode,proc.IcdVersion)) {
					listDiagnosticCodes.Add(proc.DiagnosticCode);
					listDiagnosticVersions.Add(proc.IcdVersion);
				}
				if(proc.DiagnosticCode2!="" && !ExistsDiagnosticCode(listDiagnosticCodes,listDiagnosticVersions,proc.DiagnosticCode2,proc.IcdVersion)) {
					listDiagnosticCodes.Add(proc.DiagnosticCode2);
					listDiagnosticVersions.Add(proc.IcdVersion);
				}
				if(proc.DiagnosticCode3!="" && !ExistsDiagnosticCode(listDiagnosticCodes,listDiagnosticVersions,proc.DiagnosticCode3,proc.IcdVersion)) {
					listDiagnosticCodes.Add(proc.DiagnosticCode3);
					listDiagnosticVersions.Add(proc.IcdVersion);
				}
				if(proc.DiagnosticCode4!="" && !ExistsDiagnosticCode(listDiagnosticCodes,listDiagnosticVersions,proc.DiagnosticCode4,proc.IcdVersion)) {
					listDiagnosticCodes.Add(proc.DiagnosticCode4);
					listDiagnosticVersions.Add(proc.IcdVersion);
				}
			}
			while(isPadded && listDiagnosticCodes.Count<12) {//Pad to at least 12 items.  Simplifies claim printing logic.
				listDiagnosticCodes.Add("");
				listDiagnosticVersions.Add(0);
			}
			return listDiagnosticCodes;
		}

		///<summary>Both listDiagCodes and listDiagVersions must be the same length and not null.</summary>
		private static bool ExistsDiagnosticCode(List<string> listDiagCodes,List<byte> listDiagVersions,string diagnosticCode,byte diagnosticVersion)	{
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<listDiagCodes.Count;i++) {
				if(listDiagCodes[i]==diagnosticCode && listDiagVersions[i]==diagnosticVersion) {
					return true;
				}
			}
			return false;
		}

		///<summary></summary>
		public static long GetClinicNum(long procNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetLong(MethodBase.GetCurrentMethod(),procNum);
			}
			string command="SELECT ClinicNum FROM procedurelog WHERE ProcNum="+POut.Long(procNum);
			return PIn.Long(Db.GetScalar(command));
		}

		//public static bool IsUsingCode(long codeNum) {
		//	if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
		//		return Meth.GetBool(MethodBase.GetCurrentMethod(),codeNum);
		//	}
		//	string command="SELECT COUNT(*) FROM procedurelog WHERE CodeNum="+POut.Long(codeNum);
		//	if(Db.GetCount(command)=="0") {
		//		return false;
		//	}
		//	return true;
		//}

		public static void SetCanadianLabFeesCompleteForProc(Procedure proc) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),proc);
				return;
			}
			//If this gets run on a lab fee itself, nothing will happen because result will be zero procs.
			string command="SELECT * FROM procedurelog WHERE ProcNumLab="+proc.ProcNum+" AND ProcStatus!="+POut.Int((int)ProcStat.D);
			List<Procedure> labFeesForProc=Crud.ProcedureCrud.SelectMany(command);
			if(proc.ProcNumLab==0) {//Regular procedure, not a lab.
				for(int i=0;i<labFeesForProc.Count;i++) {
					Procedure labFeeNew=labFeesForProc[i];
					Procedure labFeeOld=labFeeNew.Copy();
					labFeeNew.AptNum=proc.AptNum;
					labFeeNew.CanadianTypeCodes=proc.CanadianTypeCodes;
					labFeeNew.ClinicNum=proc.ClinicNum;
					labFeeNew.DateEntryC=proc.DateEntryC;
					labFeeNew.PlaceService=proc.PlaceService;
					labFeeNew.ProcDate=proc.ProcDate;
					labFeeNew.ProcStatus=ProcStat.C;
					labFeeNew.ProvNum=proc.ProvNum;
					labFeeNew.SiteNum=proc.SiteNum;
					labFeeNew.UserNum=proc.UserNum;
					Update(labFeeNew,labFeeOld);
				}
			}
			else {//Lab fee.  Set complete, set the parent procedure as well as any other lab fees complete.
				command="SELECT * FROM procedurelog WHERE ProcNum="+proc.ProcNumLab+" AND ProcStatus!="+POut.Int((int)ProcStat.D);
				Procedure procParent=Crud.ProcedureCrud.SelectOne(command);
				SetCanadianLabFeesCompleteForProc(procParent);
				Procedure parentProcNew=procParent;
				Procedure parentProcOld=procParent.Copy();
				parentProcNew.ProcStatus=ProcStat.C;
				Update(parentProcNew,parentProcOld);
			}
		}

		public static void SetCanadianLabFeesStatusForProc(Procedure proc) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),proc);
				return;
			}
			//If this gets run on a lab fee itself, nothing will happen because result will be zero procs.
			string command="SELECT * FROM procedurelog WHERE ProcNumLab="+proc.ProcNum+" AND ProcStatus!="+POut.Int((int)ProcStat.D);
			List<Procedure> labFeesForProc=Crud.ProcedureCrud.SelectMany(command);
			if(proc.ProcNumLab==0) {//Regular procedure, not a lab.
				for(int i=0;i<labFeesForProc.Count;i++) {
					Procedure labFeeNew=labFeesForProc[i];
					Procedure labFeeOld=labFeeNew.Copy();
					labFeeNew.ProcStatus=proc.ProcStatus;
					Update(labFeeNew,labFeeOld);
				}
			}
			else {//Lab fee.  If lab is set back to any status other than complete, set the parent procedure as well as any other lab fees back to that status.
				command="SELECT * FROM procedurelog WHERE ProcNum="+proc.ProcNumLab+" AND ProcStatus!="+POut.Int((int)ProcStat.D);
				Procedure procParent=Crud.ProcedureCrud.SelectOne(command);
				Procedure parentProcNew=procParent;
				Procedure parentProcOld=procParent.Copy();
				parentProcNew.ProcStatus=proc.ProcStatus;
				SetCanadianLabFeesStatusForProc(parentProcNew);
				Update(parentProcNew,parentProcOld);
			}
		}

		public static void DeleteCanadianLabFeesForProcCode(long procNum) {
			string command="SELECT * FROM procedurelog WHERE ProcNumLab="+procNum+" AND ProcStatus!="+POut.Int((int)ProcStat.D);
			List<Procedure> labFeeProcs=Crud.ProcedureCrud.SelectMany(command);
			for(int i=0;i<labFeeProcs.Count;i++) {
				Delete(labFeeProcs[i].ProcNum);
			}
		}

		/////<summary>Gets the number of procedures attached to a claim.</summary>
		//public static int GetCountForClaim(long claimNum) {
		//	if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
		//		return Meth.GetInt(MethodBase.GetCurrentMethod(),claimNum);
		//	}
		//	string command=
		//		"SELECT COUNT(*) FROM procedurelog "
		//		+"WHERE ProcNum IN "
		//		+"(SELECT claimproc.ProcNum FROM claimproc "
		//		+" WHERE ClaimNum="+claimNum+")";
		//	return PIn.Int(Db.GetCount(command));
		//}

		///<summary>Gets a list of procedures for </summary>
		public static DataTable GetReferred(DateTime dateFrom, DateTime dateTo, bool complete) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),dateFrom,dateTo,complete);
			}
			string command=
				"SELECT procedurelog.CodeNum,procedurelog.PatNum,LName,FName,MName,RefDate,DateProcComplete,refattach.Note,RefToStatus "
				+"FROM procedurelog "
				+"JOIN refattach ON procedurelog.ProcNum=refattach.ProcNum "
				+"JOIN referral ON refattach.ReferralNum=referral.ReferralNum "
				+"WHERE RefDate>="+POut.Date(dateFrom)+" "
				+"AND RefDate<="+POut.Date(dateTo)+" ";
			if(!complete) {
				command+="AND DateProcComplete="+POut.Date(DateTime.MinValue)+" ";
			}
			command+="ORDER BY RefDate";
			return Db.GetTable(command);
		}

		///<summary></summary>
		public static void Lock(DateTime date1, DateTime date2) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),date1,date2);
				return;
			}
			string command="UPDATE procedurelog SET IsLocked=1 "
				+"WHERE (ProcStatus="+POut.Int((int)ProcStat.C)+" "//completed
				+"OR CodeNum="+POut.Long(ProcedureCodes.GetCodeNum(ProcedureCodes.GroupProcCode))+") "//or group note
				+"AND ProcDate >= "+POut.Date(date1)+" "
				+"AND ProcDate <= "+POut.Date(date2);
			Db.NonQ(command);
		}

		///<summary>Inserts, updates, or deletes database rows to match supplied list.  Must always pass in two lists.</summary>
		public static void Sync(List<Procedure> listNew,List<Procedure> listOld,long userNum=0) {
			if(RemotingClient.RemotingRole!=RemotingRole.ServerWeb) {
				userNum=Security.CurUser.UserNum;
			}
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),listNew,listOld,userNum);
				return;
			}
			Crud.ProcedureCrud.Sync(listNew,listOld,userNum);
		}

		public static void SetTPActive(long patNum,List<long> listProcNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),patNum,listProcNums); //Never pass DB list through the web service (Note: Why?  Our proc list is special, it doesn't contain all procs so we shouldn't code this method to always use our limited list of procs........)
				return;
			}
			string command="UPDATE procedurelog SET ProcStatus="+POut.Int((int)ProcStat.TPi)+" WHERE PatNum="+POut.Long(patNum)+" "+
			  "AND ProcStatus="+POut.Int((int)ProcStat.TP)+" ";
			if(listProcNums.Count==0) {
				Db.NonQ(command);
				return; //no procedures left on active plan
			}
			command+="AND ProcNum NOT IN ("+string.Join(",",listProcNums)+") ";
			Db.NonQ(command);
			command="UPDATE procedurelog SET ProcStatus="+POut.Int((int)ProcStat.TP)+" WHERE PatNum="+POut.Long(patNum)+" "+
			  "AND ProcStatus="+POut.Int((int)ProcStat.TPi)+" AND ProcNum IN ("+string.Join(",",listProcNums)+") ";
			Db.NonQ(command);
		}
	}

	/*================================================================================================================
	=========================================== class ProcedureComparer =============================================*/

	///<summary>This sorts procedures based on priority, then tooth number, then code (but if Canadian lab code, uses proc code here instead of lab code).  Finally, if comparing a proc and its Canadian lab code, it puts the lab code after the proc.  It does not care about dates or status.  Currently used in TP module only.  The Chart module, Account module, and appointments use Procedurelog.CompareProcedures().</summary>
	public class ProcedureComparer:IComparer {
		///<summary>This sorts procedures based on priority, then tooth number.  It does not care about dates or status.  Currently used in TP module and Chart module sorting.</summary>
		int IComparer.Compare(Object objx,Object objy) {
			Procedure x=(Procedure)objx;
			Procedure y=(Procedure)objy;
			//first, by priority
			if(x.Priority!=y.Priority) {//if priorities are different
				if(x.Priority==0) {
					return 1;//x is greater than y. Priorities always come first.
				}
				if(y.Priority==0) {
					return -1;//x is less than y. Priorities always come first.
				}
				return DefC.GetOrder(DefCat.TxPriorities,x.Priority).CompareTo(DefC.GetOrder(DefCat.TxPriorities,y.Priority));
			}
			//priorities are the same, so sort by toothrange
			if(x.ToothRange!=y.ToothRange) {
				//empty toothranges come before filled toothrange values
				return x.ToothRange.CompareTo(y.ToothRange);
			}
			//toothranges are the same (usually empty), so compare toothnumbers
			if(x.ToothNum!=y.ToothNum) {
				//this also puts invalid or empty toothnumbers before the others.
				return Tooth.ToInt(x.ToothNum).CompareTo(Tooth.ToInt(y.ToothNum));
			}
			//priority and toothnums are the same, so sort by code.
			/*string adaX=x.Code;
			if(x.ProcNumLab !=0){//if x is a Canadian lab proc
				//then use the Code of the procedure instead of the lab code
				adaX=Procedures.GetOneProc(
			}
			string adaY=y.Code;*/
			return ProcedureCodes.GetStringProcCode(x.CodeNum).CompareTo(ProcedureCodes.GetStringProcCode(y.CodeNum));
			//return x.Code.CompareTo(y.Code);
			//return 0;//priority, tooth number, and code are all the same
		}
	}

}
