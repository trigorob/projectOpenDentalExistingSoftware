using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDentBusiness{

	///<summary>This does not correspond to any table in the database.  It works with a variety of tables to calculate aging.</summary>
	public class Ledgers {

		///<summary>Returns a rough guess on how long RunAging() will take in milliseconds based on the amount of data within certain tables that are used to compute aging.</summary>
		public static double GetAgingComputationTime() {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetDouble(MethodBase.GetCurrentMethod());
			}
			//Factor of 0.0042680625638876 was discovered by timing aging on a large database.  It proved to be very accurate when tested on other databases.
			//A large database with 6091757 rows in the following tables took on average 26 seconds (26000 ms) to run aging.  26000(ms) / 6091757(rows) = 0.0042680625638876
			string command=@"SELECT ((SELECT COUNT(*) FROM patient)
				+ (SELECT COUNT(*) FROM procedurelog)
				+ (SELECT COUNT(*) FROM paysplit)
				+ (SELECT COUNT(*) FROM adjustment)
				+ (SELECT COUNT(*) FROM claimproc)
				+ (SELECT COUNT(*) FROM payplan)
				+ (SELECT COUNT(*) FROM payplancharge)) * 0.0042680625638876 AgingInMilliseconds";
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				command+=" FROM dual";//Oracle requires a FROM clause be present.
			}
			return PIn.Double(Db.GetScalar(command));
		}

		///<summary>This runs aging for all patients.  If using monthly aging, it always just runs the aging as of the last date again.  If using daily aging, it runs it as of today.  This logic used to be in FormAging, but is now centralized.</summary>
		public static void RunAging() {
			//No need to check RemotingRole; no call to db.
			if(PrefC.GetBool(PrefName.AgingCalculatedMonthlyInsteadOfDaily)) {
				ComputeAging(0,PrefC.GetDate(PrefName.DateLastAging));
			}
			else {
				ComputeAging(0,DateTime.Today);
				if(PrefC.GetDate(PrefName.DateLastAging) != DateTime.Today) {
					Prefs.UpdateString(PrefName.DateLastAging,POut.Date(DateTime.Today,false));
					//Since this is always called from UI, the above line works fine to keep the prefs cache current.
				}
			}
		}

		///<summary>Computes aging for the family specified. Specify guarantor=0 in order to calculate aging for all families. Gets all info from db.
		///<para>The aging calculation will use the following rules within each family:</para>
		///<para>1) The aging "buckets" (0 to 30, 31 to 60, 61 to 90 and Over 90) ONLY include account activity on or before AsOfDate.</para>
		///<para>2) BalTotal includes all account activity, even future entries. If historical, BalTotal excludes entries after AsOfDate.</para>
		///<para>3) InsEst includes all insurance estimates, even future estimates. If historical, InsEst excludes ins est after AsOfDate.</para>
		///<para>4) PayPlanDue includes all payplan charges minus credits. If historical, PayPlanDue excludes charges and credits after AsOfDate.</para></summary>
		public static void ComputeAging(long guarantor,DateTime asOfDate) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),guarantor,asOfDate);
				return;
			}
			bool isMySqlDb=(DataConnection.DBtype==DatabaseType.MySql);
			string command="";
			if(PrefC.GetBool(PrefName.AgingIsEnterprise)) {//for all pats use famaging table, for single fam get amounts first then update pat table
				if(guarantor==0) {
					command="TRUNCATE TABLE famaging;"
						+"INSERT INTO famaging (PatNum,BalOver90,Bal_61_90,Bal_31_60,Bal_0_30,BalTotal,InsEst,PayPlanDue) "
						+"("
							+GetAgingQueryString(0,asOfDate,false)
							+(isMySqlDb?" HAVING BalOver90!=0 OR Bal_61_90!=0 OR Bal_31_60!=0 OR Bal_0_30!=0 OR BalTotal!=0 OR PayPlanDue!=0 OR InsEst!=0":"")
						+");"
					+"UPDATE patient p "
					+"LEFT JOIN famaging f ON p.PatNum=f.PatNum "//left join so non-guarantors and those with no transactions will be coalesced to 0
					+"SET "
					+"p.BalOver90  = COALESCE(f.BalOver90,0),"
					+"p.Bal_61_90  = COALESCE(f.Bal_61_90,0),"
					+"p.Bal_31_60  = COALESCE(f.Bal_31_60,0),"
					+"p.Bal_0_30   = COALESCE(f.Bal_0_30,0),"
					+"p.BalTotal   = COALESCE(f.BalTotal,0),"
					+"p.InsEst     = COALESCE(f.InsEst,0),"
					+"p.PayPlanDue = COALESCE(f.PayPlanDue,0);"
					+"TRUNCATE TABLE famaging;";
				}
				else {
					FamAging famAgingCur=Crud.FamAgingCrud.SelectOne(GetAgingQueryString(guarantor,asOfDate,false));
					command="UPDATE patient p SET "
						+"p.BalOver90 =CASE WHEN p.Guarantor!=p.PatNum THEN 0 ELSE "+famAgingCur.BalOver90 +" END,"
						+"p.Bal_61_90 =CASE WHEN p.Guarantor!=p.PatNum THEN 0 ELSE "+famAgingCur.Bal_61_90 +" END,"
						+"p.Bal_31_60 =CASE WHEN p.Guarantor!=p.PatNum THEN 0 ELSE "+famAgingCur.Bal_31_60 +" END,"
						+"p.Bal_0_30  =CASE WHEN p.Guarantor!=p.PatNum THEN 0 ELSE "+famAgingCur.Bal_0_30  +" END,"
						+"p.BalTotal  =CASE WHEN p.Guarantor!=p.PatNum THEN 0 ELSE "+famAgingCur.BalTotal  +" END,"
						+"p.InsEst    =CASE WHEN p.Guarantor!=p.PatNum THEN 0 ELSE "+famAgingCur.InsEst    +" END,"
						+"p.PayPlanDue=CASE WHEN p.Guarantor!=p.PatNum THEN 0 ELSE "+famAgingCur.PayPlanDue+" END "
						+"WHERE p.Guarantor="+guarantor;
				}
			}
			else {//Not using the famaging table.
				//If is for all patients, not single family, zero out all aged bals in order to catch former guarantors.  Zeroing out for a single family is
				//handled in the query below. (see the region "Get All Family PatNums")  Unioning is too slow for all patients, so run this statement first.
				//Added to the same query string to force Galera Cluster to process both queries on the same node to prevent a deadlock error.
				if(guarantor==0) {
					command="UPDATE patient SET "
					+"Bal_0_30   = 0,"
					+"Bal_31_60  = 0,"
					+"Bal_61_90  = 0,"
					+"BalOver90  = 0,"
					+"InsEst     = 0,"
					+"BalTotal   = 0,"
					+"PayPlanDue = 0;";
				}
				command+=(isMySqlDb?"UPDATE patient p,":"MERGE INTO patient p USING ")
					+"("+GetAgingGuarTransQuery(guarantor,asOfDate,false)+") famSums "
					+(isMySqlDb?"":"ON (p.Guarantor=famSums.Guarantor) WHEN MATCHED THEN UPDATE ")
					//Update the patient table based on the family amounts summed from 'famSums', and distribute the payments into the oldest balances first.
					+"SET p.BalOver90=(CASE WHEN p.Guarantor != p.PatNum THEN 0 "//zero out non-guarantors
						+"ELSE ROUND(CASE WHEN famSums.TotalCredits >= famSums.ChargesOver90 THEN 0 "//over 90 day bal paid in full
						+"ELSE famSums.ChargesOver90-famSums.TotalCredits END,3) END),"//over 90 day bal partially paid or unpaid.
					+"p.Bal_61_90=(CASE WHEN p.Guarantor != p.PatNum THEN 0 "//zero out non-guarantors
						+"ELSE ROUND(CASE WHEN famSums.TotalCredits <= famSums.ChargesOver90 THEN famSums.Charges_61_90 "//61-90 day bal unpaid
						+"WHEN famSums.ChargesOver90+famSums.Charges_61_90 <= famSums.TotalCredits THEN 0 "//61-90 day bal paid in full
						+"ELSE famSums.ChargesOver90+famSums.Charges_61_90-famSums.TotalCredits END,3) END),"//61-90 day bal partially paid
					+"p.Bal_31_60=(CASE WHEN p.Guarantor != p.PatNum THEN 0 "//zero out non-guarantors
						+"ELSE ROUND(CASE WHEN famSums.TotalCredits < famSums.ChargesOver90+famSums.Charges_61_90 "
						+"THEN famSums.Charges_31_60 "//31-60 day bal unpaid
						+"WHEN famSums.ChargesOver90+famSums.Charges_61_90+famSums.Charges_31_60 <= famSums.TotalCredits THEN 0 "//31-60 day bal paid in full
						+"ELSE famSums.ChargesOver90+famSums.Charges_61_90+famSums.Charges_31_60-famSums.TotalCredits END,3) END),"//31-60 day bal partially paid
					+"p.Bal_0_30=(CASE WHEN p.Guarantor != p.PatNum THEN 0 "//zero out non-guarantors
						+"ELSE ROUND(CASE WHEN famSums.TotalCredits < famSums.ChargesOver90+famSums.Charges_61_90+famSums.Charges_31_60 "
						+"THEN famSums.Charges_0_30 "//0-30 day bal unpaid
						+"WHEN famSums.ChargesOver90+famSums.Charges_61_90+famSums.Charges_31_60+famSums.Charges_0_30 <= famSums.TotalCredits "
						+"THEN 0 "//0-30 day bal paid in full
						+"ELSE famSums.ChargesOver90+famSums.Charges_61_90+famSums.Charges_31_60+famSums.Charges_0_30-famSums.TotalCredits "
						+"END,3) END),"//0-30 day bal partially paid
					+"p.BalTotal=(CASE WHEN p.Guarantor != p.PatNum THEN 0 "//zero out non-guarantors
						+"ELSE ROUND(famSums.BalTotal,3) END),"
					+"p.InsEst=(CASE WHEN p.Guarantor != p.PatNum THEN 0 "//zero out non-guarantors
						+"ELSE ROUND(famSums.InsEst,3) END),"
					+"p.PayPlanDue=(CASE WHEN p.Guarantor != p.PatNum THEN 0 "//zero out non-guarantors
						+"ELSE ROUND(famSums.PayPlanDue,3) END)"
					+(isMySqlDb?" WHERE p.Guarantor=famSums.Guarantor":"");//Aging calculations only apply to guarantors, zero out non-guarantor bals
			}
			Db.NonQ(command);
		}

		///<summary>Returns a query string for selecting the guarantor and aged bals with InsEst and PayPlanDue that exactly matches what the patient
		///table amounts would be updated to if aging was run with the same settings.  This allows us to run aging for historic dates without the need to
		///reset the bals for the current date.  Used for reports.  Also for enterprise aging, when isAllPats used to populate the famaging table,
		///when !isAllPats returns 1 row used to update family.</summary>
		public static string GetAgingQueryString(long guarantor,DateTime asOfDate,bool isHistoric=true) {
			//No need to check RemotingRole; no call to db.
			//Returns family amounts summed from 'famSums', with payments distributed into the oldest balances first.
			string command="SELECT famSums.Guarantor PatNum,"
				+"ROUND(CASE WHEN famSums.TotalCredits >= famSums.ChargesOver90 THEN 0 "//over 90 day paid in full
					+"ELSE famSums.ChargesOver90 - famSums.TotalCredits END,3) BalOver90,"//over 90 day partially paid or unpaid.
				+"ROUND(CASE WHEN famSums.TotalCredits <= famSums.ChargesOver90 THEN famSums.Charges_61_90 "//61-90 day unpaid
					+"WHEN famSums.ChargesOver90 + famSums.Charges_61_90 <= famSums.TotalCredits THEN 0 "//61-90 day paid in full
					+"ELSE famSums.ChargesOver90 + famSums.Charges_61_90 - famSums.TotalCredits END,3) Bal_61_90,"//61-90 day partially paid
				+"ROUND(CASE WHEN famSums.TotalCredits < famSums.ChargesOver90 + famSums.Charges_61_90 THEN famSums.Charges_31_60 "//31-60 day unpaid
					+"WHEN famSums.ChargesOver90 + famSums.Charges_61_90 + famSums.Charges_31_60 <= famSums.TotalCredits THEN 0 "//31-60 day paid in full
					+"ELSE famSums.ChargesOver90 + famSums.Charges_61_90 + famSums.Charges_31_60 - famSums.TotalCredits END,3) Bal_31_60,"//31-60 day partially paid
				+"ROUND(CASE WHEN famSums.TotalCredits < famSums.ChargesOver90 + famSums.Charges_61_90 + famSums.Charges_31_60 THEN famSums.Charges_0_30 "//0-30 day unpaid
					+"WHEN famSums.ChargesOver90 + famSums.Charges_61_90 + famSums.Charges_31_60 + famSums.Charges_0_30 <= famSums.TotalCredits THEN 0 "//0-30 day paid in full
					+"ELSE famSums.ChargesOver90 + famSums.Charges_61_90 + famSums.Charges_31_60 + famSums.Charges_0_30 - famSums.TotalCredits END,3) Bal_0_30,"//0-30 day partially paid
				+"ROUND(famSums.BalTotal,3) BalTotal,"
				+"ROUND(famSums.InsEst,3) InsEst,"
				+"ROUND(famSums.PayPlanDue,3) PayPlanDue "//PayPlanDue included for enterprise aging use
				+"FROM ("+GetAgingGuarTransQuery(guarantor,asOfDate,isHistoric)+") famSums";
			return command;
		}

		///<summary>Returns a query string.</summary>
		private static string GetAgingGuarTransQuery(long guarantor,DateTime asOfDate,bool historic) {
			//No need to check RemotingRole; no call to db.
			if(asOfDate.Year<1880) {
				asOfDate=DateTime.Today;
			}
			string asOfDateStr=POut.Date(asOfDate);
			string billInAdvanceDate;
			if(historic) {
				//This if statement never really does anything.  The only places that call this function with historic=true don't look at the
				//patient.payplandue amount, and patient aging gets reset after the reports are generated.  In the future if we start looking at payment plan
				//due amounts when historic=true we may need to revaluate this if statement.
				billInAdvanceDate=POut.Date(DateTime.Today.AddDays(PrefC.GetLong(PrefName.PayPlansBillInAdvanceDays)));
			}
			else {
				billInAdvanceDate=POut.Date(asOfDate.AddDays(PrefC.GetLong(PrefName.PayPlansBillInAdvanceDays)));
			}
			string thirtyDaysAgo=POut.Date(asOfDate.AddDays(-30));
			string sixtyDaysAgo=POut.Date(asOfDate.AddDays(-60));
			string ninetyDaysAgo=POut.Date(asOfDate.AddDays(-90));
			string familyPatNums="";
			string command="";
			if(guarantor>0) {
				command="SELECT p.PatNum FROM patient p WHERE p.Guarantor="+guarantor;
				familyPatNums=string.Join(",",Db.GetListLong(command));//will contain at least one patnum (the guarantor)
			}
			bool isAllPats=string.IsNullOrWhiteSpace(familyPatNums);//true if guarantor==0 or invalid, meaning for all patients not just one family
			int payPlanVersionCur=PrefC.GetInt(PrefName.PayPlansVersion);
			command="SELECT p.Guarantor,"
				+"SUM(CASE WHEN trans.TranAmount > 0 AND trans.TranDate < "+ninetyDaysAgo+" THEN trans.TranAmount ELSE 0 END) ChargesOver90,"
				+"SUM(CASE WHEN trans.TranAmount > 0 AND trans.TranDate < "+sixtyDaysAgo+" "
					+"AND trans.TranDate >= "+ninetyDaysAgo+" THEN trans.TranAmount ELSE 0 END) Charges_61_90,"
				+"SUM(CASE WHEN trans.TranAmount > 0 AND trans.TranDate < "+thirtyDaysAgo+" "
					+"AND trans.TranDate >= "+sixtyDaysAgo+" THEN trans.TranAmount ELSE 0 END) Charges_31_60,"
				+"SUM(CASE WHEN trans.TranAmount > 0 AND trans.TranDate <= "+asOfDateStr+" "
					+"AND trans.TranDate >= "+thirtyDaysAgo+" THEN trans.TranAmount ELSE 0 END) Charges_0_30,"
				+"-SUM(CASE WHEN trans.TranAmount < 0 AND trans.TranDate <= "+asOfDateStr+" THEN trans.TranAmount ELSE 0 END) TotalCredits,"
				+"SUM(CASE WHEN trans.TranAmount != 0 "+(historic?("AND trans.TranDate <= "+asOfDateStr+" "):"")+"THEN trans.TranAmount ELSE 0 END) BalTotal,"
				+"SUM(trans.InsPayEstWo) InsEst,"
				+"SUM(trans.PayPlanAmount) PayPlanDue "
				+"FROM ("
				#region Derived Trans Table Aliased 'trans'
					#region Completed Procs
					+"SELECT pl.PatNum,pl.ProcDate TranDate,pl.ProcFee*(pl.UnitQty+pl.BaseUnits) TranAmount,0 PayPlanAmount,0 InsPayEstWo "
					+"FROM procedurelog pl "
					+"WHERE pl.ProcStatus=2 "
					+"AND pl.ProcFee != 0 "
					+(isAllPats?"":("AND pl.PatNum IN ("+familyPatNums+") "))
					#endregion Completed Procs
					+"UNION ALL "
					#region Insurance Payments and WriteOffs, PayPlan Ins Payments, and InsPayEst
					+"SELECT cp.PatNum,cp.DateCp TranDate,"
					+"(CASE WHEN cp.Status != 0 "//only Received,Supplemental,CapClaim,CapComplete
						+"THEN -cp.WriteOff-(CASE WHEN cp.PayPlanNum=0 THEN cp.InsPayAmt ELSE 0 END) ELSE 0 END) TranAmount,"//Claim payments and capitation WOs
					+"(CASE WHEN cp.PayPlanNum != 0 AND cp.Status IN(1,4,5) "//Received,Supplemental,CapClaim attached to payplan
						+(historic?("AND cp.DateCP <= "+asOfDateStr+" "):"")+"THEN -cp.InsPayAmt ELSE 0 END) PayPlanAmount,"//Claim payments tracked by payment plan
					+"(CASE WHEN "+(historic?("cp.ProcDate <= "+asOfDateStr+" "//historic=NotRcvd OR Rcvd and DateCp>asOfDate
						+"AND (cp.Status=0 OR (cp.Status=1 AND cp.DateCP > "+asOfDateStr+"))"):"cp.Status=0")+" "//not historic=NotReceived
						+"THEN cp.InsPayEst+cp.WriteOff ELSE 0 END) InsPayEstWo "//inspayest+writeoff
					+"FROM claimproc cp "
					+"WHERE cp.status IN (0,1,4,5,7) "//NotReceived,Received,Supplemental,CapClaim,CapComplete
					+(isAllPats?"":("AND cp.PatNum IN ("+familyPatNums+") "))
					+(DataConnection.DBtype==DatabaseType.MySql?"HAVING PayPlanAmount != 0 OR TranAmount != 0 OR InsPayEstWo != 0 ":"")//efficiency improvement for MySQL only.
					#endregion Insurance Payments and WriteOffs, PayPlan Ins Payments, and InsPayEst
					+"UNION ALL "
					#region Adjustments
					+"SELECT a.PatNum,a.AdjDate TranDate,a.AdjAmt TranAmount,0 PayPlanAmount,0 InsPayEstWo "
					+"FROM adjustment a "
					+"WHERE a.AdjAmt != 0 "
					+(isAllPats?"":("AND a.PatNum IN ("+familyPatNums+") "))
					#endregion Adjustments
					+"UNION ALL "
					#region Paysplits and PayPlan Paysplits
					+"SELECT ps.PatNum,ps.DatePay TranDate,"
					//v1: splits not attached to payment plans, v2: all splits for pat/fam
					+(payPlanVersionCur==1?"(CASE WHEN ps.PayPlanNum=0 THEN -ps.SplitAmt ELSE 0 END)":"-ps.SplitAmt")+" TranAmount,"
					//We cannot exclude payments made outside the specified family, since payment plan guarantors can be in another family.
					+"(CASE WHEN ps.PayPlanNum != 0 THEN -ps.SplitAmt ELSE 0 END) PayPlanAmount,0 InsPayEstWo "//Paysplits attached to payment plans
					+"FROM paysplit ps "
					+"WHERE ps.SplitAmt != 0 "
					+(historic?("AND ps.DatePay <= "+asOfDateStr+" "):"")
					+(isAllPats?"":("AND ps.PatNum IN ("+familyPatNums+") "))
					#endregion Paysplits and PayPlan Paysplits
					+"UNION ALL "
					#region PayPlan Charges
					//Calculate the payment plan charges for each payment plan guarantor on or before date considering the PayPlansBillInAdvanceDays setting.
					//Ignore pay plan charges for a different family, since payment plan guarantors might be in another family.
					+"SELECT ppc.Guarantor PatNum,ppc.ChargeDate TranDate,0 TranAmount,COALESCE(ppc.Principal+ppc.Interest,0) PayPlanAmount,0 InsPayEstWo "
					+"FROM payplancharge ppc "
					+"WHERE ppc.ChargeDate <= "+billInAdvanceDate+" "//accounts for historic vs current because of how it's set above
					+"AND ppc.ChargeType="+POut.Int((int)PayPlanChargeType.Debit)+" "
					+(isAllPats?"":("AND ppc.Guarantor IN ("+familyPatNums+") "))
					+"AND COALESCE(ppc.Principal+ppc.Interest,0) != 0 "
					#endregion PayPlan Charges
					+"UNION ALL ";
					#region PayPlan Principal and Interest/CompletedAmt
					#region PayPlan Version 1
					if(payPlanVersionCur==1) {//v1: aging the entire payment plan, not the payPlanCharges						
						command+="SELECT pp.PatNum,pp.PayPlanDate TranDate,-pp.CompletedAmt TranAmount,0 PayPlanAmount,0 InsPayEstWo "
							+"FROM payplan pp "
							+"WHERE pp.CompletedAmt != 0 "
							+(isAllPats?"":("AND pp.PatNum IN ("+familyPatNums+") "));
					}
					#endregion PayPlan Version 1
					#region PayPlan Version 2
					else if(payPlanVersionCur==2) {//v2, we should be looking for payplancharges and aging those as patient debits/credits accordingly	
						//Use the guarantor on the ppcharge because the payment plan guarantor is responsible for the payments.
						command+="SELECT ppc.Guarantor,ppc.ChargeDate TranDate,"
							+"(CASE WHEN ppc.ChargeType != "+POut.Int((int)PayPlanChargeType.Debit)+" THEN -ppc.Principal "
								+"WHEN pp.PlanNum=0 THEN ppc.Principal+ppc.Interest ELSE 0 END) TranAmount,0 PayPlanAmount,0 InsPayEstWo "
							+"FROM payplancharge ppc "
							+"LEFT JOIN payplan pp ON pp.PayPlanNum=ppc.PayPlanNum "
							+"WHERE ppc.ChargeDate <= "+asOfDateStr+" "
							+(isAllPats?"":("AND ppc.Guarantor IN ("+familyPatNums+") "));
					}
					#endregion PayPlan Version 2
					#endregion PayPlan Principal and Interest/CompletedAmt
					#region Get All Family PatNums
					if(!isAllPats) {
						//get all family PatNums in case there are no transactions for the family in order to clear out the family balance
						command+="UNION ALL "
							+"SELECT PatNum,NULL TranDate,0 TranAmount,0 PayPlanAmount,0 InsPayEstWo "
							+"FROM patient "
							+"WHERE PatNum IN ("+familyPatNums+")";
					}
					#endregion Get All Family PatNums
				#endregion Derived Trans Table Aliased 'trans'
				command+=") trans "
				+"INNER JOIN patient p ON p.PatNum=trans.PatNum "
				+"GROUP BY p.Guarantor";
			if(!isAllPats || !PrefC.GetBool(PrefName.AgingIsEnterprise)) {//only if for one fam or if not using famaging table
				command+=" ORDER BY NULL";
			}
			return command;
		}

		///<summary>Gets the earliest date of any portion of the current balance for the family.
		///Returns a data table with two columns: PatNum and DateAccountAge.</summary>
		public static DataTable GetDateBalanceBegan(List<PatAging> listGuarantors,DateTime dateAsOf,bool isSuperBills) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),listGuarantors,dateAsOf,isSuperBills);
			}
			DataTable retval=new DataTable();
			retval.Columns.Add("PatNum");
			retval.Columns.Add("DateAccountAge");
			//key=SuperFamily (PatNum); value=list of PatNums for the guars of each fam on a superbill, or if not a superbill, a list containing the GuarNum
			Dictionary<long,List<long>> dictSuperFamGNums=new Dictionary<long,List<long>>();
			foreach(PatAging patAgeCur in listGuarantors) {
				//if making superBills and this guarantor has super billing and is also the superhead for the super family,
				//fill dict with all guarnums and add all family members for all guars included in the superbill to the all patnums list
				if(isSuperBills && patAgeCur.HasSuperBilling && patAgeCur.SuperFamily==patAgeCur.PatNum) {
					dictSuperFamGNums[patAgeCur.SuperFamily]=Patients.GetSuperFamilyGuarantors(patAgeCur.SuperFamily)
						.FindAll(x => x.HasSuperBilling)
						.Select(x => x.PatNum).ToList();
				}
				else {//not a superBill, just add all family members for this guarantor
					dictSuperFamGNums[patAgeCur.PatNum]=new List<long>() { patAgeCur.PatNum };
				}
			}
			//list of all family member PatNums for each guarantor/superhead for all statements being generated
			List<long> listPatNumsAll=Patients.GetAllFamilyPatNums(dictSuperFamGNums.SelectMany(x => x.Value).ToList());
			if(listPatNumsAll.Count<1) {//should never happen
				return retval;
			}
			string patNumStr=string.Join(",",listPatNumsAll);
			string command="SELECT patient.Guarantor AS PatNum,TranDate,SUM(CASE WHEN TranAmount>0 THEN TranAmount ELSE 0 END) AS ChargeForDate,"
				+"SUM(CASE WHEN TranAmount<0 THEN TranAmount ELSE 0 END) AS PayForDate "
				+"FROM ("
					//Get the completed procedure dates and charges for the fam
					+"SELECT PatNum,ProcDate AS TranDate,ProcFee*(UnitQty+BaseUnits) AS TranAmount "
					+"FROM procedurelog "
					+"WHERE PatNum IN ("+patNumStr+") "
					+"AND ProcStatus="+POut.Int((int)ProcStat.C)+" "
					+"AND "+DbHelper.DtimeToDate("ProcDate")+"<="+POut.Date(dateAsOf)+" "
					+"UNION ALL "
					//Paysplits for the fam
					+"SELECT PatNum,DatePay AS TranDate,-SplitAmt AS TranAmount "
					+"FROM paysplit "
					+"WHERE PatNum IN ("+patNumStr+") "
					+"AND PayPlanNum=0 "//Only splits not attached to payment plans
					+"AND "+DbHelper.DtimeToDate("DatePay")+"<="+POut.Date(dateAsOf)+" "
					+"UNION ALL "
					//Get the adjustment dates and amounts for the fam
					+"SELECT PatNum,AdjDate AS TranDate,AdjAmt AS TranAmount "
					+"FROM adjustment "
					+"WHERE PatNum IN ("+patNumStr+") "
					+"AND "+DbHelper.DtimeToDate("AdjDate")+"<="+POut.Date(dateAsOf)+" "
					+"UNION ALL "
					//Claim payments for the fam
					+"SELECT PatNum,DateCp AS TranDate,-InsPayAmt-Writeoff AS TranAmount "
					+"FROM claimproc "
					+"WHERE PatNum IN ("+patNumStr+") "
					+"AND STATUS IN("+POut.Int((int)ClaimProcStatus.Received)
						+","+POut.Int((int)ClaimProcStatus.Supplemental)
						+","+POut.Int((int)ClaimProcStatus.CapClaim)
						+","+POut.Int((int)ClaimProcStatus.CapComplete)+") "
					+"AND PayPlanNum=0 "//Only ins payments not attached to payment plans
					+"AND "+DbHelper.DtimeToDate("DateCp")+"<="+POut.Date(dateAsOf)+" "
					+"UNION ALL "
					//Payment plan principal for the fam
					+"SELECT PatNum,PayPlanDate AS TranDate,-CompletedAmt AS TranAmount "
					+"FROM payplan "
					+"WHERE PatNum IN ("+patNumStr+") "
					+"AND "+DbHelper.DtimeToDate("PayPlanDate")+"<="+POut.Date(dateAsOf)
				+") RawPatTrans "
				+"INNER JOIN patient ON patient.PatNum=RawPatTrans.PatNum "
				+"GROUP BY patient.Guarantor,TranDate "
				+"ORDER BY patient.Guarantor,TranDate";
			DataTable table=Db.GetTable(command);
			if(table.Rows.Count<1) {
				return retval;
			}
			Dictionary<long,double> dictGuarCreditTotals=table.Rows.OfType<DataRow>()
				.GroupBy(x => PIn.Long(x["PatNum"].ToString()),x => PIn.Double(x["PayForDate"].ToString()))
				.ToDictionary(x => x.Key,y => y.Sum());
			double runTot=0;
			long guarNumCur;
			//Create a dictionary that tells a story about the transactions and their dates for each family.
			Dictionary<long,Dictionary<DateTime,double>> dictGuarDateBal=new Dictionary<long,Dictionary<DateTime,double>>();
			foreach(DataRow rowCur in table.Rows) {
				guarNumCur=PIn.Long(rowCur["PatNum"].ToString());
				if(!dictGuarDateBal.ContainsKey(guarNumCur)) {
					runTot=dictGuarCreditTotals[guarNumCur];
					dictGuarDateBal[guarNumCur]=new Dictionary<DateTime,double>();
				}
				runTot+=PIn.Double(rowCur["ChargeForDate"].ToString());
				dictGuarDateBal[guarNumCur][PIn.Date(rowCur["TranDate"].ToString())]=runTot;
			}
			DataRow row;
			List<DateTime> listDateBals;
			List<long> listGuarNums;
			//find the earliest trans that uses up the account credits and is therefore the trans date for which the account balance is "first" positive
			foreach(PatAging patAgeCur in listGuarantors) {
				if(isSuperBills && patAgeCur.HasSuperBilling && patAgeCur.PatNum!=patAgeCur.SuperFamily) {
					continue;
				}
				if(!isSuperBills || !patAgeCur.HasSuperBilling) {
					listGuarNums=new List<long>() { patAgeCur.PatNum };
				}
				else {//must be superbill and this is the superhead
					if(!dictSuperFamGNums.ContainsKey(patAgeCur.PatNum)) {
						continue;//should never happen
					}
					listGuarNums=dictSuperFamGNums[patAgeCur.PatNum];
				}
				//dateLastZero=DateTime.MinValue;
				listDateBals=new List<DateTime>();
				foreach(long guarNum in listGuarNums) {
					if(!dictGuarDateBal.ContainsKey(guarNum)) {//should never happen
						continue;
					}
					//list of guars, or if not a super statement a list of one guar, and the date of the trans that used up the last of the acct credits
					listDateBals.Add(dictGuarDateBal[guarNum].Where(x => x.Value>0.005).Select(x => x.Key).DefaultIfEmpty(DateTime.MinValue).Min());
				}
				row=retval.NewRow();
				row["PatNum"]=POut.Long(patAgeCur.PatNum);
				//set to the oldest balance date for all guarantors on this superbill, or if not a super bill, the oldest balance date for this guarantor
				//could be DateTime.MinValue if their credits pay for all of their charges
				row["DateAccountAge"]=listDateBals.Where(x => x>DateTime.MinValue).DefaultIfEmpty(DateTime.MinValue).Min();
				retval.Rows.Add(row);
			}
			return retval;
		}
	}

}