using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenDentBusiness {
	public class AccountModules {

		///<summary>If intermingled=true, the patnum of any family member will get entire family intermingled.</summary>
		public static DataSet GetAll(long patNum,bool viewingInRecall,DateTime fromDate,DateTime toDate,bool intermingled,bool showProcBreakdown,bool showPayNotes,bool showAdjNotes) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetDS(MethodBase.GetCurrentMethod(),patNum,viewingInRecall,fromDate,toDate,intermingled,showProcBreakdown,showPayNotes,showAdjNotes);
			} 
			Family fam=Patients.GetFamily(patNum);
			Patient pat=fam.GetPatient(patNum);
			if(intermingled){
				patNum=fam.ListPats[0].PatNum;//guarantor
			}
			DataSet retVal=new DataSet();
			if(viewingInRecall) {
				retVal.Tables.Add(ChartModules.GetProgNotes(patNum,false,new ChartModuleComponentsToLoad()));
			}
			else {
				retVal.Tables.Add(GetCommLog(patNum));
			}
			bool singlePatient=!intermingled;//so one or the other will be true
			decimal payPlanDue=0;
			decimal balanceForward=0;
			//Gets 3 tables: account(or account###,account###,etc), patient, payplan.
			DataSet dataSetAccount=GetAccount(patNum,fromDate,toDate,intermingled,singlePatient,0,showProcBreakdown,showPayNotes,false,showAdjNotes,false
				,pat,fam,out payPlanDue,out balanceForward);
			for(int i=0;i<dataSetAccount.Tables.Count;i++) {
				DataTable table=new DataTable();
				table=dataSetAccount.Tables[i].Clone();
				for(int j=0;j<dataSetAccount.Tables[i].Rows.Count;j++) {
					table.ImportRow(dataSetAccount.Tables[i].Rows[j]);
				}
				retVal.Tables.Add(table);
			}
			retVal.Tables.Add(GetMisc(fam,patNum,payPlanDue,balanceForward,StmtType.NotSet,null));//table=misc.  Just holds some info that we can't find anywhere else.
			return retVal;
		}

		///<summary>If intermingled=true the patnum of any family member will get entire family intermingled.  toDate should not be Max, or PayPlan amort will include too many charges.  The 10 days will not be added to toDate until creating the actual amortization schedule.</summary>
		public static DataSet GetStatementDataSet(Statement stmt,bool isComputeAging=true){
			//long patNum,bool singlePatient,DateTime fromDate,DateTime toDate,bool intermingled) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetDS(MethodBase.GetCurrentMethod(),stmt,isComputeAging);
			}
			long patNum;
			if(stmt.SuperFamily!=0) {//Superfamily statement
				patNum=stmt.SuperFamily;
			}
			else {
				patNum=stmt.PatNum;
			}
			Family fam=Patients.GetFamily(patNum);
			if(stmt.Intermingled) {
				patNum=fam.ListPats[0].PatNum;//guarantor
			}
			Patient pat=fam.GetPatient(patNum);
			//Gets 3 tables: account(or account###,account###,etc), patient, payplan.
			DataSet retVal;
			if(stmt.SuperFamily!=0) {//Superfamily statement, Intermingled and SinglePatient should always be false
				retVal=GetSuperFamAccount(stmt,isComputeAging);//GetSuperFamAccount will use stmt.SuperFamily as the superhead
			}
			else {
				retVal=GetAccount(patNum,stmt,isComputeAging);
			}
			return retVal;
		}

		///<summary>Gets a table of charges mixed with payments to show in the payplan edit window.  Parameters: 0:payPlanNum</summary>
		public static DataSet GetPayPlanAmort(long payPlanNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetDS(MethodBase.GetCurrentMethod(),payPlanNum);
			} 
			DataSet retVal=new DataSet();
			DataTable table=GetPayPlanAmortTable(payPlanNum);
			retVal.Tables.Add(table);
			return retVal;
		}

		private static DataTable GetPayPlanAmortTable(long payPlanNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),payPlanNum);
			}
			DataConnection dcon=new DataConnection();
			DataTable table=new DataTable("payplanamort");
			DataRow row;
			SetTableColumns(table);
			List<DataRow> rows=new List<DataRow>();
			string command="SELECT ChargeDate,Interest,Note,PayPlanChargeNum,Principal,ProvNum,PatNum FROM payplancharge "
				+"WHERE PayPlanNum="+POut.Long(payPlanNum)+" AND ChargeType="+POut.Int((int)PayPlanChargeType.Debit);//for v1, debits are the only ChargeType
			DataTable rawCharge=dcon.GetTable(command);
			DateTime dateT;
			decimal principal;
			decimal interest;
			decimal total;
			for(int i=0;i<rawCharge.Rows.Count;i++){
				interest=PIn.Decimal(rawCharge.Rows[i]["Interest"].ToString());
				principal=PIn.Decimal(rawCharge.Rows[i]["Principal"].ToString());
				total=principal+interest;
				row=table.NewRow();
				row["AdjNum"]="0";
				row["balance"]="";//fill this later
				row["balanceDouble"]=0;//fill this later
				row["chargesDouble"]=total;
				row["charges"]=((decimal)row["chargesDouble"]).ToString("n");
				row["ClaimNum"]="0";
				row["ClaimPaymentNum"]="0";
				row["colorText"]=Color.Black.ToArgb().ToString();
				row["creditsDouble"]=0;
				row["credits"]="";//((double)row["creditsDouble"]).ToString("n");
				dateT=PIn.DateT(rawCharge.Rows[i]["ChargeDate"].ToString());
				row["DateTime"]=dateT;
				row["date"]=dateT.ToShortDateString();
				row["description"]="";//"Princ: "+principal.ToString("n")+
				if(interest!=0){
					row["description"]+="Interest: "+interest.ToString("n");//+"Princ: "+principal.ToString("n")+;
				}
				if(rawCharge.Rows[i]["Note"].ToString()!=""){
					if(row["description"].ToString()!=""){
						row["description"]+="  ";	
					}
					row["description"]+=rawCharge.Rows[i]["Note"].ToString();
				}
				//row["extraDetail"]="";
				row["patient"]="";
				row["PatNum"]=rawCharge.Rows[i]["PatNum"].ToString();
				row["PayNum"]="0";
				row["PayPlanNum"]="0";
				row["PayPlanChargeNum"]=rawCharge.Rows[i]["PayPlanChargeNum"].ToString();
				row["ProcCode"]=Lans.g("AccountModule","PPcharge");
				row["ProcNum"]="0";
				row["procsOnObj"]="";
				row["prov"]=Providers.GetAbbr(PIn.Long(rawCharge.Rows[i]["ProvNum"].ToString()));
				row["signed"]="";
				row["StatementNum"]="0";
				row["tth"]="";
				rows.Add(row);
			}
			long payPlanPlanNum=0;
			PayPlan payPlanCur=PayPlans.GetOne(payPlanNum);
			if(payPlanCur!=null) {
				payPlanPlanNum=payPlanCur.PlanNum;
			}
			if(payPlanPlanNum==0) {//not a insurance payment plan
				//Paysplits
				command="SELECT CheckNum,DatePay,paysplit.PatNum,PayAmt,paysplit.PayNum,PayPlanNum,"
					+"PayType,ProcDate,ProvNum,SplitAmt "
					+"FROM paysplit "
					+"LEFT JOIN payment ON paysplit.PayNum=payment.PayNum "
					+"WHERE ("
					+"paysplit.PayPlanNum="+POut.Long(payPlanNum);
				/*for(int i=0;i<fam.List.Length;i++){
					if(i!=0){
						command+="OR ";
					}
					command+="paysplit.PatNum ="+POut.PInt(fam.List[i].PatNum)+" ";
				}*/
				command+=") ORDER BY ProcDate";
			}
			else {//insurance payment plan
				//Ins Payments
				command="SELECT ClaimNum,MAX(CheckNum) CheckNum,DateCP,MAX(PatNum) PatNum,MAX(CheckAmt) CheckAmt,claimproc.ClaimPaymentNum,"
					+"MAX(PayPlanNum) PayPlanNum,MAX(PayType) PayType,MAX(ProcDate) ProcDate,SUM(InsPayAmt) InsPayAmt,"
					+"(SELECT ProvTreat FROM claim WHERE claimproc.ClaimNum=claim.ClaimNum) ProvNum "//MAX functions added to preserve behavior in Oracle.
					+"FROM claimproc "
					+"LEFT JOIN claimpayment ON claimproc.ClaimPaymentNum=claimpayment.ClaimPaymentNum "
					+"WHERE PayPlanNum="+POut.Long(payPlanNum)+" "
					+"AND (Status=1 OR Status=4 OR Status=5) "//received or supplemental or capclaim
					+"GROUP BY ClaimNum,DateCP,claimproc.ClaimPaymentNum "
					+"ORDER BY ProcDate";
			}
			DataTable rawPay=dcon.GetTable(command);
			decimal payamt;
			decimal amt;
			Def[][] arrayDefs=DefC.GetArrayLong();
			for(int i=0;i<rawPay.Rows.Count;i++){
				row=table.NewRow();
				row["AdjNum"]="0";
				row["balance"]="";//fill this later
				row["balanceDouble"]=0;//fill this later
				row["chargesDouble"]=0;
				row["charges"]="";
				row["ClaimNum"]="0";
				row["ClaimPaymentNum"]="0";
				row["colorText"]=arrayDefs[(int)DefCat.AccountColors][3].ItemColor.ToArgb().ToString();
				if(payPlanPlanNum!=0) {//ins payments
					row["ClaimNum"]=rawPay.Rows[i]["ClaimNum"].ToString();
					row["ClaimPaymentNum"]=rawPay.Rows[i]["ClaimPaymentNum"].ToString();
					row["colorText"]=arrayDefs[(int)DefCat.AccountColors][7].ItemColor.ToArgb().ToString();
				}
				if(payPlanPlanNum==0) {
					amt=PIn.Decimal(rawPay.Rows[i]["SplitAmt"].ToString());
				}
				else {
					amt=PIn.Decimal(rawPay.Rows[i]["InsPayAmt"].ToString());
				}
				row["creditsDouble"]=amt;
				row["credits"]=((decimal)row["creditsDouble"]).ToString("n");
				if(payPlanPlanNum==0) {
					dateT=PIn.DateT(rawPay.Rows[i]["DatePay"].ToString());
				}
				else {
					dateT=PIn.DateT(rawPay.Rows[i]["DateCP"].ToString());//this may be changed to ProcDate in the future
				}
				row["DateTime"]=dateT;
				row["date"]=dateT.ToShortDateString();
				if(payPlanPlanNum==0) {
					row["description"]=DefC.GetName(DefCat.PaymentTypes,PIn.Long(rawPay.Rows[i]["PayType"].ToString()));
					payamt=PIn.Decimal(rawPay.Rows[i]["PayAmt"].ToString());
				}
				else {
					row["description"]=DefC.GetName(DefCat.InsurancePaymentType,PIn.Long(rawPay.Rows[i]["PayType"].ToString()));
					payamt=PIn.Decimal(rawPay.Rows[i]["CheckAmt"].ToString());
				}
				if(rawPay.Rows[i]["CheckNum"].ToString()!=""){
					row["description"]+=" #"+rawPay.Rows[i]["CheckNum"].ToString();
				}
				if(payPlanPlanNum!=0 && rawPay.Rows[i]["ClaimPaymentNum"].ToString()=="0") {//attached to claim but no check (claimpayment) created
					row["description"]=Lans.g("AccountModule","No Insurance Check Created");
				}
				else {
					row["description"]+=" "+payamt.ToString("c");
					if(payamt!=amt){
						row["description"]+=" "+Lans.g("ContrAccount","(split)");
					}
				}
				//we might use DatePay/DateCP here to add to description
				//row["extraDetail"]="";
				row["patient"]="";
				row["PatNum"]=rawPay.Rows[i]["PatNum"].ToString();
				if(payPlanPlanNum==0) {
					row["PayNum"]=rawPay.Rows[i]["PayNum"].ToString();
				}
				else {
					row["PayNum"]="0";
				}
				row["PayPlanNum"]="0";
				row["PayPlanChargeNum"]="0";
				if(payPlanPlanNum==0) {
					row["ProcCode"]=Lans.g("AccountModule","Pay");
				}
				else {
					row["ProcCode"]=Lans.g("AccountModule","InsPay");
				}
				row["ProcNum"]="0";
				row["procsOnObj"]="";
				row["prov"]=Providers.GetAbbr(PIn.Long(rawPay.Rows[i]["ProvNum"].ToString()));
				row["signed"]="";
				row["StatementNum"]="0";
				row["tth"]="";
				rows.Add(row);
			}
			//Sorting-----------------------------------------------------------------------------------------
			rows.Sort(new AccountLineComparer());
			//Add # indicators to charges
			int num=1;
			for(int i=0;i<rows.Count;i++) {
				if(rows[i]["PayPlanChargeNum"].ToString()=="0"){//if not a payplancharge
					continue;
				}
				rows[i]["description"]="#"+num.ToString()+" "+rows[i]["description"].ToString();
				num++;
			}
			//Compute balances-------------------------------------------------------------------------------------
			decimal bal=0;
			for(int i=0;i<rows.Count;i++) {
				bal+=(decimal)rows[i]["chargesDouble"];
				bal-=(decimal)rows[i]["creditsDouble"];
				rows[i]["balanceDouble"]=bal;
				//if(rows[i]["ClaimPaymentNum"].ToString()=="0" && rows[i]["ClaimNum"].ToString()!="0"){//claims
				//	rows[i]["balance"]="";
				//}
				//else{
					rows[i]["balance"]=bal.ToString("n");
				//}
			}
			for(int i=0;i<rows.Count;i++) {
				table.Rows.Add(rows[i]);
			}
			return table;
		}

		/*private static void GetPayPlanCharges(){
		  string datesql="CURDATE()";
		  if(DataConnection.Dbtype==DatabaseType.Oracle){
				datesql="(SELECT CURRENT_DATE FROM dual)";
		  }
			string command="SELECT "
				+"(SELECT SUM(Principal) FROM payplancharge WHERE payplancharge.PayPlanNum=payplan.PayPlanNum) principal_,"
				+"(SELECT SUM(Interest) FROM payplancharge WHERE payplancharge.PayPlanNum=payplan.PayPlanNum) interest_,"
				+"(SELECT SUM(Principal) FROM payplancharge WHERE payplancharge.PayPlanNum=payplan.PayPlanNum "
					+"AND ChargeDate <= "+datesql+@") principalDue_,"
				+"(SELECT SUM(Interest) FROM payplancharge WHERE payplancharge.PayPlanNum=payplan.PayPlanNum "
					+"AND ChargeDate <= "+datesql+@") interestDue_,"
				+"CarrierName,payplan.Guarantor,"
				+"payplan.PatNum,PayPlanDate,payplan.PayPlanNum,"
				+"payplan.PlanNum "
				+"FROM payplan "
				+"LEFT JOIN insplan ON insplan.PlanNum=payplan.PlanNum "
				+"LEFT JOIN carrier ON carrier.CarrierNum=insplan.CarrierNum "
				+"WHERE  (";
			for(int i=0;i<fam.List.Length;i++){
				if(i!=0){
					command+="OR ";
				}
				command+="payplan.Guarantor ="+POut.PInt(fam.List[i].PatNum)+" "
					+"OR payplan.PatNum ="+POut.PInt(fam.List[i].PatNum)+" ";
			}
			command+=") GROUP BY payplan.PayPlanNum ORDER BY PayPlanDate";
			DataTable rawPayPlan=dcon.GetTable(command);
		}*/

		public static DataTable GetCommLog(long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),patNum);
			}
			DataConnection dcon=new DataConnection();
			DataTable table=new DataTable("Commlog");
			DataRow row;
			//columns that start with lowercase are altered for display rather than being raw data.
			table.Columns.Add("CommDateTime",typeof(DateTime));
			table.Columns.Add("commDate");
			table.Columns.Add("commTime");
			table.Columns.Add("CommlogNum");
			table.Columns.Add("commType");
			table.Columns.Add("EmailMessageNum");
			table.Columns.Add("FormPatNum");
			table.Columns.Add("mode");
			table.Columns.Add("Note");
			table.Columns.Add("patName");
			table.Columns.Add("PatNum");
			table.Columns.Add("SheetNum");
			//table.Columns.Add("sentOrReceived");			
			//table.Columns.Add("");
			//but we won't actually fill this table with rows until the very end.  It's more useful to use a List<> for now.
			List<DataRow> rows=new List<DataRow>();
			//Commlog------------------------------------------------------------------------------------------
			string command="SELECT CommDateTime,CommType,Mode_,SentOrReceived,Note,CommlogNum,p1.FName,commlog.PatNum "
				+"FROM commlog,patient p1,patient p2 "
				+"WHERE commlog.PatNum=p1.PatNum "
				+"AND p1.Guarantor=p2.Guarantor "
				+"AND p2.PatNum ="+POut.Long(patNum)+" ORDER BY CommDateTime";
			DataTable rawComm=dcon.GetTable(command);
			DateTime dateT;
			for(int i=0;i<rawComm.Rows.Count;i++){
				//if(rawComm.Rows[i]["IsStatementSent"].ToString()=="1"){
				//  continue;
				//}
				row=table.NewRow();
				dateT=PIn.DateT(rawComm.Rows[i]["CommDateTime"].ToString());
				row["CommDateTime"]=dateT;
				row["commDate"]=dateT.ToString(Lans.GetShortDateTimeFormat());
				row["commTime"]="";
				if(dateT.TimeOfDay!=TimeSpan.Zero) {
					row["commTime"]=dateT.ToString("h:mm")+dateT.ToString("%t").ToLower();
				}
				row["CommlogNum"]=rawComm.Rows[i]["CommlogNum"].ToString();
				row["commType"]=DefC.GetName(DefCat.CommLogTypes,PIn.Long(rawComm.Rows[i]["CommType"].ToString()));
				row["EmailMessageNum"]="0";
				row["FormPatNum"]="0";
				row["mode"]="";
				if(rawComm.Rows[i]["Mode_"].ToString()!="0"){//anything except none
					row["mode"]=Lans.g("enumCommItemMode",((CommItemMode)PIn.Long(rawComm.Rows[i]["Mode_"].ToString())).ToString());
				}
				row["Note"]=rawComm.Rows[i]["Note"].ToString();
				//row["patName"]="";
				//if(rawComm.Rows[i]["PatNum"].ToString()!=patNum.ToString()){
				row["patName"]=rawComm.Rows[i]["FName"].ToString();
				//}
				row["PatNum"]=rawComm.Rows[i]["PatNum"].ToString();
				row["SheetNum"]="0";
				//row["sentOrReceived"]=Lans.g("enumCommSentOrReceived",
				//	((CommSentOrReceived)PIn.PInt(rawComm.Rows[i]["SentOrReceived"].ToString())).ToString());
				rows.Add(row);
			}
			//emailmessage---------------------------------------------------------------------------------------
			//Get all emails for the entire family.
			//If a user creates an email that is attached to a patient, it will show up here for everyone.
			command="SELECT p2.FName,emailmessage.MsgDateTime,emailmessage.SentOrReceived,emailmessage.Subject,emailmessage.EmailMessageNum, "
				+"emailmessage.PatNum,emailmessage.RecipientAddress "
				+"FROM emailmessage "
				+"INNER JOIN patient p1 ON p1.PatNum="+POut.Long(patNum)+" "
				+"INNER JOIN patient p2 ON p1.Guarantor=p2.Guarantor "
					+"AND emailmessage.PatNum=p2.PatNum "
				+"WHERE emailmessage.SentOrReceived NOT IN ("+POut.Int((int)EmailSentOrReceived.AckDirectProcessed)+","
					+POut.Int((int)EmailSentOrReceived.AckDirectNotSent)+") "//Do not show Direct message acknowledgements.
				+"ORDER BY MsgDateTime";
			DataTable rawEmail=dcon.GetTable(command);
			string txt;
			for(int i=0;i<rawEmail.Rows.Count;i++) {
				row=table.NewRow();
				dateT=PIn.DateT(rawEmail.Rows[i]["MsgDateTime"].ToString());
				row["CommDateTime"]=dateT;
				row["commDate"]=dateT.ToShortDateString();
				if(dateT.TimeOfDay!=TimeSpan.Zero){
					row["commTime"]=dateT.ToString("h:mm")+dateT.ToString("%t").ToLower();
				}
				row["CommlogNum"]="0";
				//type
				row["EmailMessageNum"]=rawEmail.Rows[i]["EmailMessageNum"].ToString();
				row["FormPatNum"]="0";
				row["mode"]=Lans.g("enumCommItemMode",CommItemMode.Email.ToString());
				txt="";
				if(rawEmail.Rows[i]["SentOrReceived"].ToString()=="0") {
					txt="("+Lans.g("AccountModule","Unsent")+") ";
				}
				row["Note"]=txt+rawEmail.Rows[i]["Subject"].ToString();
				row["patName"]=rawEmail.Rows[i]["FName"].ToString();
				row["PatNum"]=rawEmail.Rows[i]["PatNum"].ToString();
				row["SheetNum"]="0";
				//if(rawEmail.Rows[i]["SentOrReceived"].ToString()=="0") {
				//	row["sentOrReceived"]=Lans.g("AccountModule","Unsent");
				//}
				//else {
				//	row["sentOrReceived"]=Lans.g("enumCommSentOrReceived",
				//		((CommSentOrReceived)PIn.PInt(rawEmail.Rows[i]["SentOrReceived"].ToString())).ToString());
				//}
				rows.Add(row);
			}
			//formpat---------------------------------------------------------------------------------------
			command="SELECT FormDateTime,FormPatNum "
				+"FROM formpat WHERE PatNum ='"+POut.Long(patNum)+"' ORDER BY FormDateTime";
			DataTable rawForm=dcon.GetTable(command);
			for(int i=0;i<rawForm.Rows.Count;i++) {
				row=table.NewRow();
				dateT=PIn.DateT(rawForm.Rows[i]["FormDateTime"].ToString());
				row["CommDateTime"]=dateT;
				row["commDate"]=dateT.ToShortDateString();
				if(dateT.TimeOfDay!=TimeSpan.Zero) {
					row["commTime"]=dateT.ToString("h:mm")+dateT.ToString("%t").ToLower();
				}
				row["CommlogNum"]="0";
				row["commType"]=Lans.g("AccountModule","Questionnaire");
				row["EmailMessageNum"]="0";
				row["FormPatNum"]=rawForm.Rows[i]["FormPatNum"].ToString();
				row["mode"]="";
				row["Note"]="";
				row["patName"]="";
				row["PatNum"]="0";//PatNum is not selected in the query and this is expected because the patName column will be blank which causes it to show in the comm log grid.
				row["SheetNum"]="0";
				//row["sentOrReceived"]="";
				rows.Add(row);
			}
			//sheet---------------------------------------------------------------------------------------
			command="SELECT p1.FName,DateTimeSheet,SheetNum,SheetType,Description,sheet.PatNum "
				+"FROM sheet,patient p1,patient p2 "
				+"WHERE sheet.PatNum =p1.PatNum "
				+"AND IsDeleted=0 "//Don't show deleted sheets in the Account module Communications Log section.
				+"AND p1.Guarantor=p2.Guarantor "
				+"AND p2.PatNum="+POut.Long(patNum)
				+" AND SheetType!="+POut.Long((int)SheetTypeEnum.Rx)//rx are only accesssible from within Rx edit window.
				+" ORDER BY DateTimeSheet";
			DataTable rawSheet=dcon.GetTable(command);
			//SheetTypeEnum sheetType;
			for(int i=0;i<rawSheet.Rows.Count;i++) {
				row=table.NewRow();
				dateT=PIn.DateT(rawSheet.Rows[i]["DateTimeSheet"].ToString());
				row["CommDateTime"]=dateT;
				row["commDate"]=dateT.ToShortDateString();
				if(dateT.TimeOfDay!=TimeSpan.Zero) {
					row["commTime"]=dateT.ToString("h:mm")+dateT.ToString("%t").ToLower();
				}
				row["CommlogNum"]="0";
				row["commType"]=Lans.g("AccountModule","Sheet");//
				row["EmailMessageNum"]="0";
				row["FormPatNum"]="0";
				row["mode"]="";
				//sheetType=(SheetTypeEnum)PIn.Long(rawSheet.Rows[i]["SheetType"].ToString());
				row["Note"]=rawSheet.Rows[i]["Description"].ToString();
				row["patName"]=rawSheet.Rows[i]["FName"].ToString();
				row["PatNum"]=rawSheet.Rows[i]["PatNum"].ToString();
				row["SheetNum"]=rawSheet.Rows[i]["SheetNum"].ToString();
				//row["sentOrReceived"]="";
				rows.Add(row);
			}
			//Sorting
			//rows.Sort(CompareCommRows);
			for(int i=0;i<rows.Count;i++) {
				table.Rows.Add(rows[i]);
			}
			DataView view = table.DefaultView;
			view.Sort = "CommDateTime";
			table = view.ToTable();
			return table;
		}

		private static void SetTableColumns(DataTable table){
			//No need to check RemotingRole; no call to db.
			//columns that start with lowercase are altered for display rather than being raw data.
			table.Columns.Add("AdjNum");
			table.Columns.Add("AbbrDesc");
			table.Columns.Add("balance");
			table.Columns.Add("balanceDouble",typeof(decimal));
			table.Columns.Add("charges");
			table.Columns.Add("chargesDouble",typeof(decimal));
			table.Columns.Add("ClaimNum");
			table.Columns.Add("ClaimPaymentNum");//if this is set, also set ClaimNum
			table.Columns.Add("clinic");
			table.Columns.Add("colorText");
			table.Columns.Add("credits");
			table.Columns.Add("creditsDouble",typeof(decimal));
			table.Columns.Add("date");
			table.Columns.Add("DateTime",typeof(DateTime));
			table.Columns.Add("description");
			//table.Columns.Add("extraDetail");
			table.Columns.Add("patient");
			table.Columns.Add("PatNum");
			table.Columns.Add("PayNum");//even though we only show split objects
			table.Columns.Add("PayPlanNum");
			table.Columns.Add("PayPlanChargeNum");
			table.Columns.Add("ProcCode");
			table.Columns.Add("ProcNum");
			table.Columns.Add("ProcNumLab");
			table.Columns.Add("procsOnObj");//for a claim or payment, the ProcNums, comma delimited.
			table.Columns.Add("prov");
			table.Columns.Add("signed");
			table.Columns.Add("StatementNum");
			table.Columns.Add("ToothNum");
			table.Columns.Add("ToothRange");
			table.Columns.Add("tth");
			table.Columns.Add("SuperFamily");
		}
		
		///<summary>Returns a data set that is designed for a super family statement.
		///This means that GetAccount will be run for every guarantor that HasSuperBilling within the super family.</summary>
		public static DataSet GetSuperFamAccount(Statement stmtCur,bool isComputeAging=true) {
			//This method does not call the database directly but still requires a remoting role check because it calls a method that uses out variables.
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetDS(MethodBase.GetCurrentMethod(),stmtCur,isComputeAging);
			}
			DataSet retVal=new DataSet();
			List<Patient> listSuperFamilyGuars=new List<Patient>();
			if(stmtCur.IsInvoice) {
				//Just add the super family head to the list of patients to include for the super statement.
				listSuperFamilyGuars.Add(Patients.GetPat(stmtCur.SuperFamily));
			}
			else {//Regular super family statement.
				listSuperFamilyGuars=Patients.GetSuperFamilyGuarantors(stmtCur.SuperFamily);
			}
			bool showProcBreakdown=false;
			if(!stmtCur.IsInvoice) {
				showProcBreakdown=PrefC.GetBool(PrefName.StatementShowProcBreakdown);
			}
			foreach(Patient guarantor in listSuperFamilyGuars) {
				if(!guarantor.HasSuperBilling) {
					continue;
				}
				//Add each family account to the data set that is included in superfamily billing.
				Family fam=Patients.GetFamily(guarantor.PatNum);
				decimal payPlanDue=0;
				decimal balanceForward=0;
				DataSet account=GetAccount(guarantor.PatNum,stmtCur.DateRangeFrom,stmtCur.DateRangeTo,true,false,stmtCur.StatementNum,showProcBreakdown,
					PrefC.GetBool(PrefName.StatementShowNotes),stmtCur.IsInvoice,PrefC.GetBool(PrefName.StatementShowAdjNotes),true,guarantor,fam,
					out payPlanDue,out balanceForward,stmtCur,isComputeAging);
				//Setting the PatNum for all rows to the guarantor so that each family will be interminged in one grid. 
				account.Tables["account"].Rows.Cast<DataRow>().ToList().ForEach(x => x["PatNum"]=guarantor.PatNum);
				account.Tables.Add(GetApptTable(fam,false,guarantor.PatNum));
				account.Tables.Add(GetMisc(fam,guarantor.PatNum,payPlanDue,balanceForward,stmtCur.StatementType,account));
				retVal.Merge(account);//This works for the purposes we need it for.  Sheets framework auto-splits entries by patnum.
			}
			//Sort rows in table by cloning table, sorting rows, then re-adding table to DataSet.
			List<DataRow> listRows=retVal.Tables["account"].Rows.OfType<DataRow>().ToList();
			//Sort the data rows first by PatNum then by date.
			//This will potentially change the order of rows to where the balance column does not make sense.
			//Recalculate balances after the sort runs.
			listRows.Sort(SortRowsForStatmentPrinting);
			decimal bal=0;
			string patNumPrev="0";
			foreach(DataRow row in listRows) {
				if(row["PatNum"].ToString()!=patNumPrev) {
					bal=0;
					patNumPrev=row["PatNum"].ToString();
					if(row["description"].ToString()==Lans.g("AccountModule","Balance Forward")) {
						bal=(decimal)row["balanceDouble"];
					}
				}
				bal+=(decimal)row["chargesDouble"];
				bal-=(decimal)row["creditsDouble"];
				row["balanceDouble"]=bal;
				if(row["ClaimPaymentNum"].ToString()=="0" && row["ClaimNum"].ToString()!="0"){//claims
					row["balance"]="";
				}
				else if(row["StatementNum"].ToString()=="0"){
					row["balance"]=bal.ToString("n");
				}
			}
			List<DataRow> listPayPlanRows=retVal.Tables["payplan"].Rows.OfType<DataRow>()
				.OrderBy(x => x["PatNum"].ToString()).ThenBy(x => PIn.DateT(x["DateTime"].ToString())).ToList();
			decimal payplanBal=0;
			foreach(DataRow row in listPayPlanRows) {
				payplanBal+=(decimal)row["chargesDouble"];
				payplanBal-=(decimal)row["creditsDouble"];
				if(row["PayNum"].ToString()=="0" && row["PayPlanChargeNum"].ToString()=="0") {//this is the payplan description row
					row["balanceDouble"]=0;
					row["balance"]="";
				}
				else {
					row["balanceDouble"]=payplanBal;
					row["balance"]=payplanBal.ToString("n");
				}
			}
			DataTable accountSorted=retVal.Tables["account"].Clone();//Easy way to copy the columns.
			listRows.ForEach(x=>accountSorted.Rows.Add(x.ItemArray));
			retVal.Tables.Remove(retVal.Tables["account"]);
			retVal.Tables.Add(accountSorted);
			DataTable payplanSorted=retVal.Tables["payplan"].Clone();//Easy way to copy the columns.
			listPayPlanRows.ForEach(x => payplanSorted.Rows.Add(x.ItemArray));
			retVal.Tables.Remove(retVal.Tables["payplan"]);
			retVal.Tables.Add(payplanSorted);
			return retVal;
		}

		///<summary>Also gets the patient table, which has one row for each family member. Also currently runs aging.  Also gets payplan table.  
		///If stmt.StatementNum is not zero, then it's for a statement, and the resulting payplan table looks totally different.  
		///If stmt.IsInvoice or stmt.StatementType==StmtType.LimitedStatement, this does some extra filtering.</summary>
		public static DataSet GetAccount(long patNum,Statement stmt,bool isComputeAging=true) {
			//This method does not call the database directly but still requires a remoting role check because it calls a method that uses out variables.
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetDS(MethodBase.GetCurrentMethod(),patNum,stmt,isComputeAging);
			}
			bool showProcBreakdown=false;
			if(!stmt.IsInvoice) {
				showProcBreakdown=PrefC.GetBool(PrefName.StatementShowProcBreakdown);
			}
			Family fam=Patients.GetFamily(patNum);
			Patient pat=fam.GetPatient(patNum);
			decimal payPlanDue=0;
			decimal balanceForward=0;
			DataSet retVal=GetAccount(patNum,stmt.DateRangeFrom,stmt.DateRangeTo,stmt.Intermingled,stmt.SinglePatient,stmt.StatementNum,showProcBreakdown,
				PrefC.GetBool(PrefName.StatementShowNotes),stmt.IsInvoice,PrefC.GetBool(PrefName.StatementShowAdjNotes),true,pat,fam,out payPlanDue,
				out balanceForward,stmt,isComputeAging);
			retVal.Tables.Add(GetApptTable(fam,stmt.SinglePatient,patNum));
			retVal.Tables.Add(GetMisc(fam,patNum,payPlanDue,balanceForward,stmt.StatementType,retVal));//table=misc; Just holds some info we can't find anywhere else.
			return retVal;
		}

		///<summary>Also gets the patient table, which has one row for each family member. Also currently runs aging.  Also gets payplan table.  
		///If StatementNum is not zero, then it's for a statement, and the resulting payplan table looks totally different.  
		///If IsInvoice or statementType==StmtType.LimitedStatement, this does some extra filtering.
		///This method cannot be called from the Middle Tier as long as it uses out parameters.
		///If !isComputeAging (ONLY if AgingIsEnterprise and printing/sending statements), we assume that aging has been run for all pats for the current
		///date and doesn't need to run again for this fam.  Used so aging doesn't run for each statement after we just ran it for all patients when the
		///statement list was generated.  We use DateLastAging to determine if computing aging is necessary.</summary>
		private static DataSet GetAccount(long patNum,DateTime fromDate,DateTime toDate,bool intermingled,bool singlePatient,long statementNum
			,bool showProcBreakdown,bool showPayNotes,bool isInvoice,bool showAdjNotes,bool isForStatementPrinting
			,Patient pat,Family fam,out decimal payPlanDue,out decimal balanceForward,Statement stmt=null,bool isComputeAging=true) 
		{
			//No need to check RemotingRole; this method contains out parameters.
			if(stmt==null) {
				//for when we are loading account data for the actual account module.
				stmt=new Statement() { StatementType=StmtType.NotSet };
			}
			DataSet retVal=new DataSet();
			payPlanDue=0;
			balanceForward=0;
			bool isReseller=false;//Used to display data in the account module differently when patient is a reseller.
			//HQ only, find out if this patient is a reseller.
			if(PrefC.GetBool(PrefName.DockPhonePanelShow) && Resellers.IsResellerFamily(fam.ListPats[0].PatNum)) {
				isReseller=true;
			}
			DataConnection dcon=new DataConnection();
			DataTable table=new DataTable("account");
			if(isComputeAging) {
				//run aging.-------------------------------------------------------
				if(PrefC.GetBool(PrefName.AgingCalculatedMonthlyInsteadOfDaily)) {
					Ledgers.ComputeAging(pat.Guarantor,PrefC.GetDate(PrefName.DateLastAging));
				}
				else {
					Ledgers.ComputeAging(pat.Guarantor,DateTime.Today);
				}
			}
			//Now, back to getting the tables------------------------------------------------------------------
			DataRow row;
			SetTableColumns(table);
			//but we won't actually fill this table with rows until the very end.  It's more useful to use a List<> for now.
			List<DataRow> rows=new List<DataRow>();
			DateTime dateT;
			decimal qty;
			decimal amt;
			string command;
			string familyPatNums="";
			if(fam!=null && fam.ListPats!=null && fam.ListPats.Length>0) {
				familyPatNums=string.Join(",",fam.ListPats.Select(x => POut.Long(x.PatNum)));
			}
			string adjNumsForLimited="";
			string paySplitNumsForLimited="";
			string procNumsForLimited="";
			if(stmt.StatementType==StmtType.LimitedStatement && statementNum>0) {
				adjNumsForLimited=string.Join(",",stmt.ListAdjNums.Select(x => POut.Long(x)));
				paySplitNumsForLimited=string.Join(",",stmt.ListPaySplitNums.Select(x => POut.Long(x)));
				procNumsForLimited=string.Join(",",stmt.ListProcNums.Select(x => POut.Long(x)));
			}
			#region Claimprocs
			//claimprocs (ins payments)----------------------------------------------------------------------------
			command="SELECT ClaimNum,MAX(ClaimPaymentNum) ClaimPaymentNum,MAX(ClinicNum) ClinicNum,DateCP,"
				+"SUM(CASE WHEN PayPlanNum=0 THEN InsPayAmt ELSE 0 END) InsPayAmt_,"//ins payments attached to payment plans tracked there
				+"SUM(CASE WHEN PayPlanNum!=0 THEN InsPayAmt ELSE 0 END) InsPayAmtPayPlan,"
				+"MAX(PatNum) PatNum,MAX(ProcDate) ProcDate,"//MAX functions added to preserve behavior in Oracle.
				//+"MAX(ProvNum) ProvNum,
				+"SUM(WriteOff) WriteOff_, "
				//js 1/28/13  The following line has been the source of many complaints in the past.  
				//When it was claim.ProvBill, it didn't match daily payment report or the account Claim row entry.
				//When it was MAX(claimproc.ProvNum), the user had no control over it because it was one prov at random.
 				//By switching to claim.ProvTreat, we are more closely matching the P&I report and the account Claim row.  ProvBill is not very meaningful outside of the claim itself.
				+"(SELECT ProvTreat FROM claim WHERE claimproc.ClaimNum=claim.ClaimNum) provNum_,MAX(PayPlanNum) PayPlanNum "//MAX PayPlanNum will return 0 or the num of the payplan tracking the payments.  Every claim will only be allowed to have payments tracked by one payplan.
				+"FROM claimproc "
				+"WHERE (Status=1 OR Status=4 OR Status=5) "//received or supplemental or capclaim
				+"AND (WriteOff>0 OR InsPayAmt!=0) ";
			if(familyPatNums!="") {
				command+="AND PatNum IN ("+familyPatNums+") ";
			}
			if(stmt.StatementType==StmtType.LimitedStatement && procNumsForLimited!="") {
				command+="AND ProcNum IN ("+procNumsForLimited+") ";
			}
			command+="GROUP BY ClaimNum,DateCP "
				+"ORDER BY DateCP";
			DataTable rawClaimPay=new DataTable();
			if(!isInvoice && (stmt.StatementType!=StmtType.LimitedStatement || procNumsForLimited!="")) {//don't run if IsInvoice or if LimitedStatement with no procs
				rawClaimPay=dcon.GetTable(command);
			}
			DateTime procdate;
			decimal writeoff;
			Def[][] arrayDefs=DefC.GetArrayLong();
			foreach(DataRow rawClaimPayRow in rawClaimPay.Rows) {//0 rows if isInvoice or is LimitedStatement with no procs
				row=table.NewRow();
				row["AbbrDesc"]="";//fill this later
				row["AdjNum"]="0";
				row["balance"]="";//fill this later
				row["balanceDouble"]=0;//fill this later
				row["chargesDouble"]=0;
				row["charges"]="";
				row["ClaimNum"]=rawClaimPayRow["ClaimNum"].ToString();
				//jsalmon - I do not agree with the next line but am leaving it here so as to not break unknown parts of the program.  Something like this should never be done.
				//          We either need to create a separate column using a naming convention that leads programmers to think it is a boolean or
				//          we need to make the column lowercase "claimPaymentNum".  Making the first character lowercase will at least lead OD developers to this line 
				//          so that they can then learn that this variable is not to be trusted and that it is in fact a boolean...
				row["ClaimPaymentNum"]="1";//this is now just a boolean flag indicating that it is a payment.
				//this is because it will frequently not be attached to an actual claim payment.
				row["clinic"]=Clinics.GetDesc(PIn.Long(rawClaimPayRow["ClinicNum"].ToString()));
				row["colorText"]=arrayDefs[(int)DefCat.AccountColors][7].ItemColor.ToArgb().ToString();
				amt=PIn.Decimal(rawClaimPayRow["InsPayAmt_"].ToString());//payments tracked in payment plans will show in the payment plan grid
				writeoff=PIn.Decimal(rawClaimPayRow["WriteOff_"].ToString());
				if(rawClaimPayRow["PayPlanNum"].ToString()!="0" && amt+writeoff==0) {//payplan payments are tracked in the payplan, so nothing to display.
					continue;//Does not add a row, so don't worry about setting the remaining columns.
				}
				row["creditsDouble"]=amt+writeoff;
				row["credits"]=((decimal)row["creditsDouble"]).ToString("n");
				dateT=PIn.DateT(rawClaimPayRow["DateCP"].ToString());
				row["DateTime"]=dateT;
				row["date"]=dateT.ToString(Lans.GetShortDateTimeFormat());
				procdate=PIn.DateT(rawClaimPayRow["ProcDate"].ToString());
				row["description"]=Lans.g("AccountModule","Insurance Payment for Claim")+" "+procdate.ToShortDateString();
				if(rawClaimPayRow["PayPlanNum"].ToString()!="0") {
					row["description"]+="\r\n("+Lans.g("AccountModule","Payments Tracked in Payment Plan")+")";
				}
				if(rawClaimPayRow["PayPlanNum"].ToString()!="0" || writeoff!=0) {
					row["description"]+="\r\n"+Lans.g("AccountModule","Payment")+": "+amt.ToString("c");
				}
				if(writeoff!=0) {
					string writeoffDescript=PrefC.GetString(PrefName.InsWriteoffDescript);
					if(writeoffDescript=="") {
						writeoffDescript=Lans.g("AccountModule","Writeoff");
					}
					row["description"]+="\r\n"+writeoffDescript+": "+writeoff.ToString("c");
				}
				if(!isForStatementPrinting && amt!=0 && rawClaimPayRow["ClaimPaymentNum"].ToString()=="0") {
					//Not all claim payments have been finalized and are not yet attached to claim payments (checks).
					//Indicate to the user that they need to finalize this payment before reports will be accurate.
					row["description"]+="\r\n"+Lans.g("AccountModule","PAYMENT NEEDS TO BE FINALIZED");
				}
				row["patient"]=fam.GetNameInFamFirst(PIn.Long(rawClaimPayRow["PatNum"].ToString()));
				row["PatNum"]=rawClaimPayRow["PatNum"].ToString();
				row["PayNum"]="0";
				row["PayPlanNum"]="0";
				row["PayPlanChargeNum"]="0";
				row["ProcCode"]=Lans.g("AccountModule","InsPay");
				row["ProcNum"]="0";
				row["ProcNumLab"]="";
				row["procsOnObj"]="";
				row["prov"]=Providers.GetAbbr(PIn.Long(rawClaimPayRow["provNum_"].ToString()));
				row["signed"]="";
				row["StatementNum"]="0";
				row["ToothNum"]="";
				row["ToothRange"]="";
				row["tth"]="";
				rows.Add(row);
			}
			#endregion Claimprocs
			#region Procedures
			//Procedures------------------------------------------------------------------------------------------
			command=@"SELECT 
				(SELECT SUM(AdjAmt) FROM adjustment WHERE procedurelog.ProcNum=adjustment.ProcNum 
				AND adjustment.ProcNum!=0) adj_,"//Prevents long load time in a patient with thousands of entries.
				+@"procedurelog.BaseUnits,procedurelog.BillingNote,procedurelog.ClinicNum,procedurecode.CodeNum,procedurecode.AbbrDesc,Descript,
				SUM(claimproc.InsPayAmt) insPayAmt_,
				SUM(CASE WHEN claimproc.Status=0 THEN claimproc.InsPayEst END) insPayEst_,"//only include estimates for pending claims
				+@"LaymanTerm,procedurelog.MedicalCode,MAX(claimproc.NoBillIns) noBillIns_,procedurelog.PatNum,
				(SELECT SUM(paysplit.SplitAmt) FROM paysplit WHERE procedurelog.ProcNum=paysplit.ProcNum 
				AND paysplit.ProcNum!=0) patPay_,"//Prevents long load time in a patient with thousands of entries.
				+@"ProcCode,procedurelog.ProcDate procDate_,ProcFee,procedurelog.ProcNum,procedurelog.ProcNumLab,procedurelog.ProvNum,procedurelog.Surf,
				ToothNum,ToothRange,UnitQty,SUM(claimproc.WriteOff) writeOff_, 
				MIN(CASE WHEN claimproc.Status!="+POut.Int((int)ClaimProcStatus.CapComplete)+@" 
					AND insplan.IsMedical=(CASE WHEN procedurelog.MedicalCode!='' THEN 1 ELSE 0 END) 
					THEN claimproc.ClaimNum END) unsent_,
				SUM(CASE WHEN claimproc.Status="+POut.Int((int)ClaimProcStatus.CapComplete)+@" THEN claimproc.WriteOff END) writeOffCap_ "
				+@"FROM procedurelog 
				INNER JOIN procedurecode ON procedurelog.CodeNum=procedurecode.CodeNum "
				//indexAcctCov will always exists because the convert script fails if it can't be added.
				+"LEFT JOIN claimproc "+DbHelper.UseIndex("indexAcctCov","claimproc")+@" ON procedurelog.ProcNum=claimproc.ProcNum 
				LEFT JOIN insplan ON insplan.PlanNum=claimproc.PlanNum
				WHERE ProcStatus="+POut.Int((int)ProcStat.C)+" ";
			if(familyPatNums!="") {
				command+="AND procedurelog.PatNum IN ("+familyPatNums+") ";
			}
			if(stmt.StatementType==StmtType.LimitedStatement && procNumsForLimited!="") {
				command+="AND procedurelog.ProcNum IN ("+procNumsForLimited+") ";
			}
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				command+="GROUP BY procedurelog.ClinicNum,procedurelog.BaseUnits,procedurelog.BillingNote,procedurecode.CodeNum,procedurecode.AbbrDesc"
					+",Descript,LaymanTerm,procedurelog.MedicalCode,procedurelog.PatNum,ProcCode,procedurelog.ProcDate,ProcFee"
					+",procedurelog.ProcNum,procedurelog.ProcNumLab,procedurelog.ProvNum,procedurelog.Surf,ToothNum,ToothRange,UnitQty ";
			}
			else{//mysql. Including Descript in the GROUP BY causes mysql to lock up sometimes.  Unsure why.
				command+="GROUP BY procedurelog.ProcNum ";
			}
			command+="ORDER BY procedurelog.ProcDate";
			if(isInvoice) {
				//different query here.  Include all column names.
				command="SELECT '' AS adj_,procedurelog.BaseUnits,procedurelog.BillingNote,procedurelog.ClinicNum,procedurecode.CodeNum,procedurecode.AbbrDesc,procedurecode.Descript,"
					+"'' AS insPayAmt_,'' AS insPayEst_,procedurecode.LaymanTerm,procedurelog.MedicalCode,'' AS noBillIns_,procedurelog.PatNum,"
					+"'' AS patPay_,procedurecode.ProcCode,"+DbHelper.DtimeToDate("procedurelog.ProcDate")+" procDate_,procedurelog.ProcFee,procedurelog.ProcNum,procedurelog.ProcNumLab,"
					+"procedurelog.ProvNum,procedurelog.Surf,procedurelog.ToothNum,procedurelog.ToothRange,procedurelog.UnitQty,"
					+"'' AS writeOff_,'' AS unsent_,'' AS writeOffCap_ "
					+"FROM procedurelog "
					+"LEFT JOIN procedurecode ON procedurelog.CodeNum=procedurecode.CodeNum "
					+"WHERE StatementNum="+POut.Long(statementNum)+" "
					+"ORDER BY procDate_";
			}
			DataTable rawProc=new DataTable();
			if(stmt.StatementType!=StmtType.LimitedStatement || procNumsForLimited!="") {//Don't run if this is a limited statement with no procs
				rawProc=dcon.GetTable(command);
			}
			List<long> listSignedProcNums=new List<long>();//filled with subset of procnums from the rawProc table where most recent ProcNote is signed
			if(!isForStatementPrinting //not for a statement
				&& familyPatNums!="" //and we have a family
				&& DisplayFields.GetForCategory(DisplayFieldCategory.AccountModule).Any(x => x.InternalName=="Signed")) //"Signed" is displayed in acct grid
			{
				listSignedProcNums=ProcNotes.GetIsProcNoteSigned(rawProc.Select().Select(x => PIn.Long(x["ProcNum"].ToString())).ToList());
			}
			decimal insPayAmt;
			decimal insPayEst;
			decimal writeOff;
			decimal writeOffCap;
			decimal patPort;
			decimal patPay;
			bool isNoBill;
			decimal adjAmt;
			string extraDetail;
			List<DataRow> labRows=new List<DataRow>();//Canadian lab procs, which must be added in a loop at the very end.
			for(int i=0;i<rawProc.Rows.Count;i++){
				DataRow rawProcRow=rawProc.Rows[i];
				row=table.NewRow();
				row["AbbrDesc"]=rawProcRow["AbbrDesc"].ToString();
				row["AdjNum"]="0";
				row["balance"]="";//fill this later
				row["balanceDouble"]=0;//fill this later
				qty=PIn.Long(rawProcRow["UnitQty"].ToString()) + PIn.Long(rawProcRow["BaseUnits"].ToString());
				if(qty==0){
					qty=1;
				}
				amt=PIn.Decimal(rawProcRow["ProcFee"].ToString())*qty;
				writeOffCap=PIn.Decimal(rawProcRow["writeOffCap_"].ToString());
				amt-=writeOffCap;
				row["chargesDouble"]=amt;//*qty;
				row["charges"]=((decimal)row["chargesDouble"]).ToString("n");
				row["ClaimNum"]="0";
				row["ClaimPaymentNum"]="0";
				row["clinic"]=Clinics.GetDesc(PIn.Long(rawProcRow["ClinicNum"].ToString()));
				string procCode=rawProcRow["ProcCode"].ToString();
				if(procCode=="D9986") {//Broken appointment procedure
					row["colorText"]=DefC.GetDefByExactName(DefCat.AccountColors,"Broken Appointment Procedure").ItemColor.ToArgb().ToString();
				}
				else if(procCode=="D9987") {//Canceled appointment procedure
					row["colorText"]=DefC.GetDefByExactName(DefCat.AccountColors,"Canceled Appointment Procedure").ItemColor.ToArgb().ToString();
				}
				else {//Not a broken appointment procedure.
					row["colorText"]=DefC.GetDefByExactName(DefCat.AccountColors,"Default").ItemColor.ToArgb().ToString();
				}
				row["creditsDouble"]=0;
				row["credits"]="";
				dateT=PIn.DateT(rawProcRow["procDate_"].ToString());
				row["DateTime"]=dateT;
				row["date"]=dateT.ToString(Lans.GetShortDateTimeFormat());
				//row["description"]="";
				long codeNum=PIn.Long(rawProcRow["CodeNum"].ToString());
				string surf=rawProcRow["Surf"].ToString();
				string toothNum=rawProcRow["ToothNum"].ToString();
				row["description"]=Procedures.ConvertProcToString(codeNum,surf,toothNum,true)+" ";
				if(rawProcRow["MedicalCode"].ToString()!=""){
					row["description"]+=Lans.g("ContrAccount","(medical)")+" ";
				}
				//row["description"]+=rawProcRow["Descript"].ToString();
				//if(rawProcRow["LaymanTerm"].ToString()!=""){
				//	row["description"]+=rawProcRow["LaymanTerm"].ToString();
				//}
				string toothRange=rawProcRow["ToothRange"].ToString();
				if(toothRange!=""){
					row["description"]+=" #"+Tooth.FormatRangeForDisplay(toothRange);
				}
				isNoBill=(rawProcRow["noBillIns_"].ToString()!="" && rawProcRow["noBillIns_"].ToString()!="0");
				if(isNoBill){
					row["description"]+=" "+Lans.g("ContrAccount","(NoBillIns)");
				}
				bool isShowUnsent=(!isNoBill && rawProcRow["unsent_"].ToString()=="0");//no claim attached and marked to bill insurance
				string strProcNumLab=rawProcRow["ProcNumLab"].ToString();
				if(!isNoBill && CultureInfo.CurrentCulture.Name.EndsWith("CA") && strProcNumLab!="0") {//Canadian. en-CA or fr-CA, lab fee.
					//true if the parent proc does not have a claim attached and this lab is not marked "no bill ins".  Lab is unsent if parent proc is unsent
					isShowUnsent=rawProc.Select().Any(x => x["ProcNum"].ToString()==strProcNumLab && x["unsent_"].ToString()=="0");
				}
				if(isShowUnsent) {
					row["description"]+=" "+Lans.g("ContrAccount","(unsent)");
				}
				insPayAmt=PIn.Decimal(rawProcRow["insPayAmt_"].ToString());
				insPayEst=PIn.Decimal(rawProcRow["insPayEst_"].ToString());
				writeOff=0;
				if(rawProcRow["unsent_"].ToString()!="0") {
					//If there's an attached claim, use the writeoff in the claimproc.
					//Otherwise it's just an estimate and we don't want to use anything for the writeoff.
					writeOff=PIn.Decimal(rawProcRow["writeOff_"].ToString());
				}
				long procNum=PIn.Long(rawProcRow["ProcNum"].ToString());
				if(CultureInfo.CurrentCulture.Name.EndsWith("CA")) {
					List<Procedure> listLabs=Procedures.GetCanadianLabFees(procNum);
					if(listLabs.Count>0) {//Lab Fee
						amt+=(decimal)listLabs.Select(x=>x.ProcFee).Sum();
					}
				}
				patPort=amt-insPayAmt-insPayEst-writeOff;
				patPay=PIn.Decimal(rawProcRow["patPay_"].ToString());
				adjAmt=PIn.Decimal(rawProcRow["adj_"].ToString());
				extraDetail="";
				if(patPay>0){
					extraDetail+=Lans.g("AccountModule","Pat Paid: ")+patPay.ToString("c");
				}
				if(adjAmt!=0){
					if(extraDetail!=""){
						extraDetail+=", ";
					}
					extraDetail+=Lans.g("AccountModule","Adj: ")+adjAmt.ToString("c");
				}
				if(insPayAmt>0 || writeOff>0){
					if(extraDetail!=""){
						extraDetail+=", ";
					}
					extraDetail+=Lans.g("AccountModule","Ins Paid: ")+insPayAmt.ToString("c");
					if(writeOff>0) {
						string writeoffDescript=PrefC.GetString(PrefName.InsWriteoffDescript);
						if(writeoffDescript=="") {
							writeoffDescript=Lans.g("AccountModule","Writeoff");
						}
						extraDetail+=", "+writeoffDescript+": "+writeOff.ToString("c");
					}
				}
				if(insPayEst>0) {
					if(extraDetail!="") {
						extraDetail+=", ";
					}
					extraDetail+=Lans.g("AccountModule","Ins Est: ")+insPayEst.ToString("c");
				}
				if(patPort>0 && writeOffCap==0){//if there is a cap writeoff, showing a patient portion would calculate wrong.
					if(extraDetail!="") {
						extraDetail+=", ";
					}
					extraDetail+=Lans.g("AccountModule","Pat Port: ")+patPort.ToString("c");
				}
				if(showProcBreakdown && strProcNumLab=="0") {//Not a Canadian Lab Fee.
					if(extraDetail!="") {
						row["description"]+="\r\n"+extraDetail;
					}
				}
				string billingNote=PIn.String(rawProcRow["BillingNote"].ToString());
				if(billingNote!="") {
					row["description"]+="\r\n"+billingNote;
				}
				long patNumCur=PIn.Long(rawProcRow["PatNum"].ToString());
				string patname=fam.GetNameInFamFirst(patNumCur);
				if(isReseller) {
					patname=fam.GetNameInFamLF(patNumCur);
				}
				row["patient"]=patname;
				row["PatNum"]=patNumCur;
				row["PayNum"]="0";
				row["PayPlanNum"]="0";
				row["PayPlanChargeNum"]="0";
				row["ProcCode"]=procCode;
				row["ProcNum"]=rawProcRow["ProcNum"].ToString();
				row["ProcNumLab"]=strProcNumLab;
				row["procsOnObj"]="";
				row["prov"]=Providers.GetAbbr(PIn.Long(rawProcRow["ProvNum"].ToString()));
				row["signed"]=listSignedProcNums.Contains(PIn.Long(rawProcRow["ProcNum"].ToString()))?"Signed":"";
				row["StatementNum"]="0";
				row["ToothNum"]=toothNum;
				row["ToothRange"]=toothRange;
				row["tth"]=Tooth.GetToothLabel(toothNum);
				if(strProcNumLab=="0") {//normal proc
					rows.Add(row);
				}
				else {
					row["description"]="^ ^ "+row["description"].ToString();
					labRows.Add(row);//these will be added in the loop at the end
				}
			}
			#endregion Procedures
			#region Adjustments
			//Adjustments---------------------------------------------------------------------------------------
			command="SELECT AdjAmt,AdjDate,AdjNum,AdjType,ClinicNum,PatNum,ProvNum,AdjNote "
				+"FROM adjustment ";
			if(isInvoice) {
				command+="WHERE StatementNum="+POut.Long(statementNum)+" ";
			}
			else if(stmt.StatementType==StmtType.LimitedStatement) {
				List<string> listAdjWhereOR=new List<string>();
				if(adjNumsForLimited!="") {
					listAdjWhereOR.Add("adjustment.AdjNum IN ("+adjNumsForLimited+")");//adjustments highlighted by user
				}
				if(procNumsForLimited!="") {
					listAdjWhereOR.Add("adjustment.ProcNum IN ("+procNumsForLimited+")");//add adjustments for selected procs whether user highlighted or not
				}
				if(listAdjWhereOR.Count>0) {
					command+="WHERE ("+string.Join(" OR ",listAdjWhereOR)+") ";
				}
			}
			else if(familyPatNums!="") {
				command+="WHERE PatNum IN ("+familyPatNums+") ";
			}
			command+="ORDER BY AdjDate";
			DataTable rawAdj=new DataTable();
			//don't run query if LimitedStatement and both lists are empty
			if(stmt.StatementType!=StmtType.LimitedStatement || adjNumsForLimited!="" || procNumsForLimited!="") {
				rawAdj=dcon.GetTable(command);
			}
			for(int i=0;i<rawAdj.Rows.Count;i++){
				row=table.NewRow();
				row["AbbrDesc"]="";
				row["AdjNum"]=rawAdj.Rows[i]["AdjNum"].ToString();
				row["balance"]="";//fill this later
				row["balanceDouble"]=0;//fill this later
				amt=PIn.Decimal(rawAdj.Rows[i]["AdjAmt"].ToString());
				if(amt<0){
					row["chargesDouble"]=0;
					row["charges"]="";
					row["creditsDouble"]=-amt;
					row["credits"]=(-amt).ToString("n");
				}
				else{
					row["chargesDouble"]=amt;
					row["charges"]=amt.ToString("n");
					row["creditsDouble"]=0;
					row["credits"]="";
				}
				row["ClaimNum"]="0";
				row["ClaimPaymentNum"]="0";
				row["clinic"]=Clinics.GetDesc(PIn.Long(rawAdj.Rows[i]["ClinicNum"].ToString()));
				row["colorText"]=arrayDefs[(int)DefCat.AccountColors][1].ItemColor.ToArgb().ToString();
				dateT=PIn.DateT(rawAdj.Rows[i]["AdjDate"].ToString());
				row["DateTime"]=dateT;
				row["date"]=dateT.ToString(Lans.GetShortDateTimeFormat());
				row["description"]=DefC.GetName(DefCat.AdjTypes,PIn.Long(rawAdj.Rows[i]["AdjType"].ToString()));
				if(rawAdj.Rows[i]["AdjNote"].ToString() !="" && showAdjNotes) {
					//row["extraDetail"] = rawAdj.Rows[i]["AdjNote"].ToString();
					row["description"]+="\r\n"+rawAdj.Rows[i]["AdjNote"].ToString();
				}
				string patname=fam.GetNameInFamFirst(PIn.Long(rawAdj.Rows[i]["PatNum"].ToString()));
				if(isReseller) {
					patname=fam.GetNameInFamLF(PIn.Long(rawAdj.Rows[i]["PatNum"].ToString()));
				}
				row["patient"]=patname;
				row["PatNum"]=rawAdj.Rows[i]["PatNum"].ToString();
				row["PayNum"]="0";
				row["PayPlanNum"]="0";
				row["PayPlanChargeNum"]="0";
				row["ProcCode"]=Lans.g("AccountModule","Adjust");
				row["ProcNum"]="0";
				row["ProcNumLab"]="";
				row["procsOnObj"]="";
				row["prov"]=Providers.GetAbbr(PIn.Long(rawAdj.Rows[i]["ProvNum"].ToString()));
				row["signed"]="";
				row["StatementNum"]="0";
				row["ToothNum"]="";
				row["ToothRange"]="";
				row["tth"]="";
				rows.Add(row);
			}
			#endregion Adjustments
			#region Paysplits
			//paysplits-----------------------------------------------------------------------------------------
			List<string> listWhereClauses=new List<string>();
			string familyPayPlanNums=String.Join(",",PayPlans.GetForPats(fam.ListPats.Select(x=>x.PatNum).ToList(),pat.PatNum)
				.Select(y => y.PayPlanNum).ToList());
			if(familyPatNums!="") {
				//grab any paysplits for the family or that are attached to payment plans for the family.
				//This is so we can associate payments to payment plans for this family, even if someone from a different family is making the payments.
				string whereFamilyPatPayPlan="(paysplit.PatNum IN ("+familyPatNums+")";
				if(familyPayPlanNums!="") {
					whereFamilyPatPayPlan+="OR paysplit.PayPlanNum IN ("+familyPayPlanNums+")";
				}
				whereFamilyPatPayPlan+=")";
				listWhereClauses.Add(whereFamilyPatPayPlan);
			}
			if(stmt.StatementType==StmtType.LimitedStatement) {
				List<string> listLimitedWhere=new List<string>();
				if(paySplitNumsForLimited!="") {
					listLimitedWhere.Add("paysplit.SplitNum IN ("+paySplitNumsForLimited+")");//add paysplits highlighted by the user
				}
				if(procNumsForLimited!="") {
					listLimitedWhere.Add("paysplit.ProcNum IN ("+procNumsForLimited+")");//add paysplits for selected procs whether user highlighted or not
				}
				if(listLimitedWhere.Count>0) {
					listWhereClauses.Add("("+string.Join(" OR ",listLimitedWhere)+")");
				}
			}
			string whereClause="";//create whereClause variable so both queries get the same paysplits
			if(listWhereClauses.Count>0) {
				whereClause="WHERE "+string.Join(" AND ",listWhereClauses);
			}
			//Column names with MAX left the same as they should not be considered aggregate (even though they are).
			//MAX function used to preserve behavior in Oracle.
			command="SELECT MAX(CheckNum) CheckNum,paysplit.ClinicNum,MAX(DatePay) DatePay,paysplit.PatNum,MAX(payment.PatNum) patNumPayment_,"
				+"MAX(PayAmt) PayAmt,paysplit.PayNum,paysplit.PayPlanNum,MAX(PayType) PayType,MAX(ProcDate) ProcDate,'' AS ProcNums_,MAX(ProvNum) ProvNum,"
				+"SUM(SplitAmt) splitAmt_,MAX(payment.PayNote) PayNote,MAX(paysplit.UnearnedType) UnearnedType "
				+"FROM paysplit "
				+"LEFT JOIN payment ON paysplit.PayNum=payment.PayNum "
				+whereClause+" "
				//if this GROUP BY changes, the foreach loop below must be changed to match
				+"GROUP BY paysplit.DatePay,paysplit.PayPlanNum,paysplit.PayNum,paysplit.PatNum,paysplit.ClinicNum ORDER BY DatePay";//ProcDate ORDER BY ProcDate";
			DataTable rawPay=new DataTable();
			//don't run query if isInvoice or if it's a LimitedStatement and no paysplits or procs were selected
			if(!isInvoice && (stmt.StatementType!=StmtType.LimitedStatement || paySplitNumsForLimited!="" || procNumsForLimited!="")) {
				rawPay=dcon.GetTable(command);
			}
			command="SELECT * FROM paysplit "+whereClause;
			List<PaySplit> listPaysplits=new List<PaySplit>();
			//don't run query if isInvoice or if it's a LimitedStatement and no paysplits or procs were selected
			if(!isInvoice && (stmt.StatementType!=StmtType.LimitedStatement || paySplitNumsForLimited!="" || procNumsForLimited!="")) {
				listPaysplits=Crud.PaySplitCrud.SelectMany(command).FindAll(x => x.ProcNum>0);
			}
			foreach(DataRow rowRp in rawPay.Rows) {
				if(listPaysplits.Count==0) {
					break;
				}
				//these are the GROUP BY columns from the rawPay query above, used to select the ProcNums from all of the paysplits using the same grouping
				DateTime rpDatePay=PIn.Date(rowRp["DatePay"].ToString());
				long rpPayPlanNum=PIn.Long(rowRp["PayPlanNum"].ToString());
				long rpPayNum=PIn.Long(rowRp["PayNum"].ToString());
				long rpPatNum=PIn.Long(rowRp["PatNum"].ToString());
				long rpClinicNum=PIn.Long(rowRp["ClinicNum"].ToString());
				//if the GROUP BY for the query used to fill table rawPay changes, this Linq needs to be changed to match exactly
				List<PaySplit> listPaySplitMatches=listPaysplits.FindAll(x => x.DatePay==rpDatePay && x.PayPlanNum==rpPayPlanNum && x.PayNum==rpPayNum
					&& x.PatNum==rpPatNum && x.ClinicNum==rpClinicNum);
				if(listPaySplitMatches.Count>0) {
					rowRp["ProcNums_"]=string.Join(",",listPaySplitMatches.Select(x => x.ProcNum));
				}
			}
			int payPlanVersionCur=PrefC.GetInt(PrefName.PayPlansVersion);
			decimal payamt;
			//if isInvoice or if it's a LimitedStatement and no paysplits or procs were selected there will be 0 rows and this loop will be skipped
			for(int i=0;i<rawPay.Rows.Count;i++){
				//Skip payments where the patnum is not in the current family.
				if(!fam.ListPats.Select(x => x.PatNum).Contains(PIn.Long(rawPay.Rows[i]["PatNum"].ToString()))) {
					continue;
				}
				string strDescript="";
				if(rawPay.Rows[i]["PayPlanNum"].ToString()!="0"){
					if(payPlanVersionCur==1) {
						continue;//in v1, do not add rows that are attached to payment plans
					}
					else {
						strDescript=" "+Lans.g("ContrAccount","(Attached to payment plan)"); //in v2, add the rows and show that they are attached to a payplan.
					}
				}
				row=table.NewRow();
				row["AbbrDesc"]="";
				row["AdjNum"]="0";
				row["balance"]="";//fill this later
				row["balanceDouble"]=0;//fill this later
				row["chargesDouble"]=0;
				row["charges"]="";
				row["ClaimNum"]="0";
				row["ClaimPaymentNum"]="0";
				row["clinic"]=Clinics.GetDesc(PIn.Long(rawPay.Rows[i]["ClinicNum"].ToString()));
				row["colorText"]=arrayDefs[(int)DefCat.AccountColors][3].ItemColor.ToArgb().ToString();
				amt=PIn.Decimal(rawPay.Rows[i]["splitAmt_"].ToString());
				row["creditsDouble"]=amt;
				row["credits"]=((decimal)row["creditsDouble"]).ToString("n");
				dateT=PIn.DateT(rawPay.Rows[i]["DatePay"].ToString());//was ProcDate in earlier versions
				row["DateTime"]=dateT;
				row["date"]=dateT.ToString(Lans.GetShortDateTimeFormat());
				row["description"]=DefC.GetName(DefCat.PaymentTypes,PIn.Long(rawPay.Rows[i]["PayType"].ToString()));
				if(rawPay.Rows[i]["CheckNum"].ToString()!=""){
					row["description"]+=" #"+rawPay.Rows[i]["CheckNum"].ToString();
				}
				payamt=PIn.Decimal(rawPay.Rows[i]["PayAmt"].ToString());
				row["description"]+=" "+payamt.ToString("c");
				if(rawPay.Rows[i]["PatNum"].ToString() != rawPay.Rows[i]["patNumPayment_"].ToString()){
					row["description"]+=" ("+Lans.g("ContrAccount","Paid by ")
						+fam.GetNameInFamFirstOrPreferredOrLast(PIn.Long(rawPay.Rows[i]["patNumPayment_"].ToString()))+")";
				}
				if(payamt!=amt){
					row["description"]+=" "+Lans.g("ContrAccount","(split)");
				}
				if(rawPay.Rows[i]["UnearnedType"].ToString()!="0") {
					row["description"]+=" - "+DefC.GetName(DefCat.PaySplitUnearnedType,PIn.Long(rawPay.Rows[i]["UnearnedType"].ToString()));
				}
				if(rawPay.Rows[i]["PayType"].ToString()=="0") {//if a txfr, clear the description
					row["description"]="";
				}
				//we might use DatePay here to add to description
				if(rawPay.Rows[i]["PayNote"].ToString() !="" && showPayNotes) {
					if(rawPay.Rows[i]["PayType"].ToString()!="0") {//if not a txfr
						row["description"]+="\r\n";
					}
					row["description"]+=rawPay.Rows[i]["PayNote"].ToString();
				}
				if(PrefC.GetBool(PrefName.AccountShowPaymentNums)) {
					row["description"]+="\r\n"+Lans.g("AccountModule","Payment Number: ")+rawPay.Rows[i]["PayNum"].ToString();
				}
				row["description"]+=strDescript;
				string patname=fam.GetNameInFamFirst(PIn.Long(rawPay.Rows[i]["PatNum"].ToString()));
				if(isReseller) {
					patname=fam.GetNameInFamLF(PIn.Long(rawPay.Rows[i]["PatNum"].ToString()));
				}
				row["patient"]=patname;
				row["patient"]=fam.GetNameInFamFirst(PIn.Long(rawPay.Rows[i]["PatNum"].ToString()));
				row["PatNum"]=rawPay.Rows[i]["PatNum"].ToString();
				row["PayNum"]=rawPay.Rows[i]["PayNum"].ToString();
				row["PayPlanNum"]="0";
				row["PayPlanChargeNum"]="0";
				if(rawPay.Rows[i]["PayType"].ToString()=="0") {//if a txfr
					row["ProcCode"]=Lans.g("AccountModule","Txfr");
				}
				else {
					row["ProcCode"]=Lans.g("AccountModule","Pay");
				}
				row["ProcNum"]="0";
				row["ProcNumLab"]="";
				row["procsOnObj"]=rawPay.Rows[i]["ProcNums_"];
				//Odd that this shows only one provider on the payment when there could be multiple, but there is no easy way to fix this currently.
				row["prov"]=Providers.GetAbbr(PIn.Long(rawPay.Rows[i]["ProvNum"].ToString()));
				row["signed"]="";
				row["StatementNum"]="0";
				row["ToothNum"]="";
				row["ToothRange"]="";
				row["tth"]="";
				rows.Add(row);
			}
			#endregion Paysplits
			#region Claims
			//claims (do not affect balance)-------------------------------------------------------------------------
			string whereAndClause="";//create whereAndClause variable so both queries get the same claims
			if(familyPatNums!="") {
				whereAndClause="AND claim.PatNum IN ("+familyPatNums+") ";
			}
			command="SELECT CarrierName,ClaimFee,claim.ClaimNum,ClaimStatus,ClaimType,claim.ClinicNum,DateReceived,DateService,"
				+"claim.DedApplied,claim.InsPayEst,claim.InsPayAmt,claim.PatNum,"
				+DbHelper.GroupConcat("procedurelog.ProcNum",distinct:true)+" ProcNums_,ProvTreat,claim.ReasonUnderPaid,claim.WriteOff,"
				+"SUM(ProcFee*(BaseUnits+(CASE WHEN UnitQty<1 THEN 1 ELSE UnitQty END))) procAmt_ "
				+"FROM claim "
				+"LEFT JOIN insplan ON claim.PlanNum=insplan.PlanNum "
				+"LEFT JOIN carrier ON carrier.CarrierNum=insplan.CarrierNum "
				+"INNER JOIN claimproc ON claimproc.ClaimNum=claim.ClaimNum "
				+"LEFT JOIN procedurelog ON claimproc.ProcNum=procedurelog.ProcNum "
				+"WHERE ClaimType != 'PreAuth' "
				+whereAndClause
				+"GROUP BY carrier.CarrierName,claim.ClaimNum,claim.ClaimFee,claim.ClaimStatus,claim.ClaimType,"
				+"claim.ClinicNum,claim.DateReceived,claim.DateService,claim.DedApplied,claim.InsPayEst,claim.InsPayAmt,"
				+"claim.PatNum,claim.ProvTreat,claim.ReasonUnderPaid,claim.WriteOff "
				+"ORDER BY DateService";
			DataTable rawClaim=new DataTable();
			if(!isInvoice && stmt.StatementType!=StmtType.LimitedStatement) {
				rawClaim=dcon.GetTable(command);
			}
			//Select the claimprocs attached to claims for this patient using the same list of pats from family where the claim is not a preauth
			//and there is a ProcNum on the claimproc.  Used to highlight the procs attached to a claim when the claim is selected.
			command="SELECT claim.ClaimNum,claimproc.ProcNum "
				+"FROM claim "
				+"INNER JOIN claimproc ON claimproc.ClaimNum=claim.ClaimNum AND claimproc.ProcNum>0 "
				+"WHERE claim.ClaimType!='PreAuth' "
				+whereAndClause;
			DataTable rawProcNumsClaim=new DataTable();
			if(!isInvoice && stmt.StatementType!=StmtType.LimitedStatement) {
				rawProcNumsClaim=dcon.GetTable(command);
			}
			foreach(DataRow rcRow in rawClaim.Rows) {//rawClaim will have 0 rows if isInvoice or StatementType is LimitedStatement
				if(rawProcNumsClaim.Rows.Count==0) {
					break;
				}
				List<DataRow> listRowMatches=rawProcNumsClaim.Rows.OfType<DataRow>().ToList().FindAll(x => x["ClaimNum"].ToString()==rcRow["ClaimNum"].ToString());
				if(listRowMatches.Count>0) {
					rcRow["ProcNums_"]=string.Join(",",listRowMatches.Select(x => PIn.Long(x["ProcNum"].ToString())));
				}
			}
			DateTime daterec;
			decimal amtpaid;//can be different than amt if claims show UCR.
			decimal procAmt;//If Canadian and lab fees exists for procedures on claim, lab fees will be added to this.
			decimal insest;
			decimal deductible;
			decimal patport;
			string claimStatus;
			for(int i=0;i<rawClaim.Rows.Count;i++){//rawClaim will have 0 rows if isInvoice or StatementType is LimitedStatement
				row=table.NewRow();
				row["AbbrDesc"]="";
				row["AdjNum"]="0";
				row["balance"]="";//fill this later
				row["balanceDouble"]=0;//fill this later
				row["chargesDouble"]=0;
				row["charges"]="";
				row["ClaimNum"]=rawClaim.Rows[i]["ClaimNum"].ToString();
				row["ClaimPaymentNum"]="0";
				row["clinic"]=Clinics.GetDesc(PIn.Long(rawClaim.Rows[i]["ClinicNum"].ToString()));
				row["colorText"]=arrayDefs[(int)DefCat.AccountColors][4].ItemColor.ToArgb().ToString();
					//might be changed lower down based on claim status
				row["creditsDouble"]=0;
				row["credits"]="";
				dateT=PIn.DateT(rawClaim.Rows[i]["DateService"].ToString());
				row["DateTime"]=dateT;
				row["date"]=dateT.ToString(Lans.GetShortDateTimeFormat());
				if(rawClaim.Rows[i]["ClaimType"].ToString()=="P"){
					row["description"]=Lans.g("ContrAccount","Pri")+" ";
				}
				else if(rawClaim.Rows[i]["ClaimType"].ToString()=="S"){
					row["description"]=Lans.g("ContrAccount","Sec")+" ";
				}
				//else if(rawClaim.Rows[i]["ClaimType"].ToString()=="PreAuth"){
				//	row["description"]=Lans.g("ContrAccount","PreAuth")+" ";
				//}
				else if(rawClaim.Rows[i]["ClaimType"].ToString()=="Other"){
					row["description"]="";
				}
				else if(rawClaim.Rows[i]["ClaimType"].ToString()=="Cap"){
					row["description"]=Lans.g("ContrAccount","Cap")+" ";
				}
				amt=PIn.Decimal(rawClaim.Rows[i]["ClaimFee"].ToString());
				row["description"]+=Lans.g("ContrAccount","Claim")+" "+amt.ToString("c")+" "
					+rawClaim.Rows[i]["CarrierName"].ToString();
				daterec=PIn.DateT(rawClaim.Rows[i]["DateReceived"].ToString());
				claimStatus=rawClaim.Rows[i]["ClaimStatus"].ToString();
				if(claimStatus=="R"){
					row["description"]+="\r\n"+Lans.g("ContrAccount","Received")+" ";
					if(daterec.Year<1880){
						row["description"]+=Lans.g("ContrAccount","(no date)");//although I don't think UI allows this
					}
					else{
						row["description"]+=daterec.ToShortDateString();
					}
					row["colorText"] = arrayDefs[(int)DefCat.AccountColors][8].ItemColor.ToArgb().ToString();
				} 
				else if(claimStatus=="U"){
					row["description"]+="\r\n"+Lans.g("ContrAccount","Unsent");
				} 
				else if(claimStatus=="H"){
					row["description"]+="\r\n"+Lans.g("ContrAccount","Hold until Pri received");
				} 
				else if(claimStatus=="W"){
					row["description"]+="\r\n"+Lans.g("ContrAccount","Waiting to Send");
				} 
				else if(claimStatus=="S"){
					row["description"]+="\r\n"+Lans.g("ContrAccount","Sent");
				}
				decimal claimLabFeeTotalAmt=0;
				//For Canada, add lab fee amounts into total claim amount.
				if(CultureInfo.CurrentCulture.Name.EndsWith("CA")) {
					string[] arrayProcNumsForClaim=PIn.ByteArray(rawClaim.Rows[i]["ProcNums_"]).Split(',');
					for(int j=0;j<arrayProcNumsForClaim.Length;j++) {
						long procNum=PIn.Long(arrayProcNumsForClaim[j]);
						if(procNum==0) {//ProcNum will be 0 for Total Payments on claims.
							continue;
						}
						for(int k=0;k<rawProc.Rows.Count;k++) {//For each procedure attached to the claim, add the lab fees into the total amount. The lab fees show in the account because they are complete.
							long procNumLab=PIn.Long(rawProc.Rows[k]["ProcNumLab"].ToString());
							if(procNumLab==procNum) {
								claimLabFeeTotalAmt+=PIn.Decimal(rawProc.Rows[k]["ProcFee"].ToString());
							}
						}
					}
				}
				if(claimLabFeeTotalAmt>0) {
					row["description"]+="\r\n"+Lans.g("ContrAccount","Lab Fees")+" "+claimLabFeeTotalAmt.ToString("c");
				}
				procAmt=PIn.Decimal(rawClaim.Rows[i]["procAmt_"].ToString());
				insest=PIn.Decimal(rawClaim.Rows[i]["InsPayEst"].ToString());
				amtpaid=PIn.Decimal(rawClaim.Rows[i]["InsPayAmt"].ToString());
				writeoff=PIn.Decimal(rawClaim.Rows[i]["WriteOff"].ToString());
				deductible=PIn.Decimal(rawClaim.Rows[i]["DedApplied"].ToString());
				if(!PrefC.GetBool(PrefName.BalancesDontSubtractIns) 
					&& (claimStatus=="W" || claimStatus=="S")
					&& rawClaim.Rows[i]["ClaimType"].ToString()!="Cap")
				{
					if (amtpaid != 0 && ((insest - amtpaid) >= 0)) {//show additional info on resubmits
						row["description"] += "\r\n" + Lans.g("ContrAccount", "Remaining Est. Payment Pending:") + " " + (insest - amtpaid).ToString("c");
					}
					else {
						row["description"] += "\r\n" + Lans.g("ContrAccount", "Estimated Payment Pending:") + " " + insest.ToString("c");
					}
				}
				if(rawClaim.Rows[i]["ClaimType"].ToString()!="Cap"){
					if(amtpaid != 0){
						row["description"]+="\r\n"+Lans.g("ContrAccount","Payment:")+" "+amtpaid.ToString("c");
					} 
					else if(amtpaid==0 && (claimStatus=="R")){
						row["description"]+="\r\n"+Lans.g("ContrAccount", "NO PAYMENT");
					}
				}
				if(writeoff!=0) {
					string writeoffDesctipt=PrefC.GetString(PrefName.InsWriteoffDescript);
					if(writeoffDesctipt=="") {
						writeoffDesctipt=Lans.g("ContrAccount","Writeoff");
					}
					row["description"]+="\r\n"+writeoffDesctipt+": "+writeoff.ToString("c");
				}
				if(deductible!=0){
					row["description"]+="\r\n"+Lans.g("ContrAccount","Deductible Applied:")+" "+deductible.ToString("c");
				}
				if(!PrefC.GetBool(PrefName.BalancesDontSubtractIns) 
					&&	(claimStatus=="W" || claimStatus=="S")
					&& rawClaim.Rows[i]["ClaimType"].ToString()!="Cap")
				{				
					List<long> procNums=PIn.String(rawClaim.Rows[i]["ProcNums_"].ToString()).Split(',').Select(x => PIn.Long(x)).ToList();
					if(CultureInfo.CurrentCulture.Name.EndsWith("CA")) {
						List<Procedure> listLabs=Procedures.GetCanadianLabFees(procNums);
						if(listLabs.Count>0) {//Lab Fee
							procAmt+=(decimal)listLabs.Select(x => x.ProcFee).Sum();
						}
					}
					patport=procAmt-insest-writeoff;
					if(patport<0){
						patport=0;
					}
					row["description"]+="\r\n"+Lans.g("ContrAccount","Est. Patient Portion:")+" "+patport.ToString("c");
				}
				if(rawClaim.Rows[i]["ReasonUnderPaid"].ToString()!=""){
					row["description"]+="\r\n"+rawClaim.Rows[i]["ReasonUnderPaid"].ToString();
				}
				//row["extraDetail"]="";
				row["patient"]=fam.GetNameInFamFirst(PIn.Long(rawClaim.Rows[i]["PatNum"].ToString()));
				row["PatNum"]=rawClaim.Rows[i]["PatNum"].ToString();
				row["PayNum"]="0";
				row["PayPlanNum"]="0";
				row["PayPlanChargeNum"]="0";
				row["ProcCode"]=Lans.g("AccountModule","Claim");
				row["ProcNum"]="0";
				row["ProcNumLab"]="";
				row["procsOnObj"]=rawClaim.Rows[i]["ProcNums_"];
				row["prov"]=Providers.GetAbbr(PIn.Long(rawClaim.Rows[i]["ProvTreat"].ToString()));
				row["signed"]="";
				row["StatementNum"]="0";
				row["ToothNum"]="";
				row["ToothRange"]="";
				row["tth"]="";
				rows.Add(row);
			}
			#endregion Claims
			#region Statements
			//Statement----------------------------------------------------------------------------------------
			List<long> listPatNums=fam.ListPats.ToList().Select(x => x.PatNum).Distinct().ToList();
			command="SELECT DateSent,IsSent,Mode_,StatementNum,PatNum,Note,NoteBold,IsInvoice,SuperFamily "
				+"FROM statement "
				+"WHERE (PatNum IN ("+string.Join(",",listPatNums)+") ";
			//Always include all statements from the super family if a super family is set.  They will be filtered out later.
			if(fam.ListPats[0].SuperFamily > 0) {
				command+="OR SuperFamily ="+POut.Long(fam.ListPats[0].SuperFamily)+" ";//Get all statements for the superfamily as well.
			}
			command+=") ";
			if(statementNum>0) {
				command+="AND StatementNum != "+POut.Long(statementNum)+" ";
			}
			command+="ORDER BY DateSent";
			DataTable rawState=new DataTable();
			if(!isInvoice && stmt.StatementType!=StmtType.LimitedStatement) {
				rawState=dcon.GetTable(command);
			}
			StatementMode _mode;
			//if we are getting a DataSet for a super statement and this guar in the super family is not the super head, skip super statement rows
			bool isSuperStmtSkipped=(isForStatementPrinting && stmt.SuperFamily>0 && stmt.SuperFamily!=pat.PatNum);
			foreach(DataRow rowCur in rawState.Rows) {//rawState will have 0 rows if isInvoice or StatementType is LimitedStatement
				long patNumCur=PIn.Long(rowCur["PatNum"].ToString());
				long superFamNum=PIn.Long(rowCur["SuperFamily"].ToString());
				if(isSuperStmtSkipped && superFamNum>0 && superFamNum!=pat.PatNum) {//skip super stmt rows for all members of super fam except for the super head
					continue;
				}
				row=table.NewRow();
				row["AbbrDesc"]="";
				row["AdjNum"]="0";
				row["balance"]="";//fill this later
				row["balanceDouble"]=0;//fill this later
				row["chargesDouble"]=0;
				row["charges"]="";
				row["ClaimNum"]="0";
				row["ClaimPaymentNum"]="0";
				row["clinic"]="";
				row["colorText"]=arrayDefs[(int)DefCat.AccountColors][5].ItemColor.ToArgb().ToString();
				row["creditsDouble"]=0;
				row["credits"]="";
				dateT=PIn.DateT(rowCur["DateSent"].ToString());
				row["DateTime"]=dateT;
				row["date"]=dateT.ToString(Lans.GetShortDateTimeFormat());
				if(rowCur["IsInvoice"].ToString()=="0") {//not an invoice
					row["description"]+=Lans.g("ContrAccount","Statement");
				}
				else {//Must be invoice
					row["description"]+=Lans.g("ContrAccount","Invoice")+" #"+rowCur["StatementNum"].ToString();
				}
				_mode=(StatementMode)PIn.Long(rowCur["Mode_"].ToString());
				row["description"]+="-"+Lans.g("enumStatementMode",_mode.ToString());
				if(rowCur["IsSent"].ToString()=="0"){
					row["description"]+=" "+Lans.g("ContrAccount","(unsent)");
				}
				string patname=fam.GetNameInFamFirst(patNumCur);
				if(isReseller) {
					patname=fam.GetNameInFamLF(patNumCur);
				}
				row["patient"]=patname;
				row["PatNum"]=patNumCur.ToString();
				row["PayNum"]="0";
				row["PayPlanNum"]="0";
				row["PayPlanChargeNum"]="0";
				row["ProcCode"]=Lans.g("AccountModule","Stmt");
				row["ProcNum"]="0";
				row["ProcNumLab"]="";
				row["procsOnObj"]="";
				row["prov"]="";
				row["signed"]="";
				row["StatementNum"]=rowCur["StatementNum"].ToString();
				row["ToothNum"]="";
				row["ToothRange"]="";
				row["tth"]="";
				row["SuperFamily"]=superFamNum.ToString();
				rows.Add(row);
			}
			#endregion Statements
			#region Payment Plans
			//Payment plans----------------------------------------------------------------------------------
			string datesql="CURDATE()";
			if(DataConnection.DBtype==DatabaseType.Oracle){
				datesql="(SELECT CURRENT_DATE FROM dual)";
			}
			//get all payment plans for members of this family. For V1, this means that the payplan row in the ledger will correctly appear as a credit.
			//this also helps populate the payment plans grid in the account module by getting passed into GetPayPlans().
			string debitType=POut.Int((int)PayPlanChargeType.Debit);
			command="SELECT "
				+"(SELECT SUM(Principal) FROM payplancharge WHERE payplancharge.PayPlanNum=payplan.PayPlanNum AND ChargeType="+debitType+") principal_,"
				+"(SELECT SUM(Interest) FROM payplancharge WHERE payplancharge.PayPlanNum=payplan.PayPlanNum AND ChargeType="+debitType+") interest_,"
				+"(SELECT SUM(Principal) FROM payplancharge WHERE payplancharge.PayPlanNum=payplan.PayPlanNum AND ChargeType="+debitType+" "
					+"AND ChargeDate <= "+datesql+") principalDue_,"
				+"(SELECT SUM(Interest) FROM payplancharge WHERE payplancharge.PayPlanNum=payplan.PayPlanNum "
					+"AND ChargeDate <= "+datesql+" AND ChargeType="+debitType+") interestDue_,"
				+"MAX(CarrierName) CarrierName,CompletedAmt,payplan.Guarantor,payplan.PatNum,PayPlanDate,payplan.PayPlanNum,payplan.PlanNum,payplan.IsClosed "
				+"FROM payplan "
				+"LEFT JOIN insplan ON insplan.PlanNum=payplan.PlanNum "
				+"LEFT JOIN carrier ON carrier.CarrierNum=insplan.CarrierNum "
				+"WHERE (payplan.PatNum IN ("+familyPatNums+") OR payplan.Guarantor IN ("+familyPatNums+")) "
				+"GROUP BY CompletedAmt,payplan.Guarantor,payplan.PatNum,PayPlanDate,payplan.PayPlanNum,payplan.PlanNum";
			if(DataConnection.DBtype==DatabaseType.Oracle){
				command+=",CarrierName,payplan.Guarantor,payplan.PatNum,PayPlanDate,payplan.PlanNum,payplan.IsClosed";
			}
			command+=" ORDER BY PayPlanDate";
			DataTable rawPayPlan=new DataTable();
			if(!isInvoice && stmt.StatementType!=StmtType.LimitedStatement) {
				rawPayPlan=dcon.GetTable(command);
			}
			if(payPlanVersionCur==1) {
				//0 rows if isInvoice or statement type is LimitedStatement.  In spite of this, the payment plans breakdown will still show at the top of invoices.
				for(int i=0;i<rawPayPlan.Rows.Count;i++) {
					//Version 1. If the payment plan's patnum isn't in the current family, then skip. We only want it to show as a credit for the patient of the payment plan.
					if(!fam.ListPats.Select(x => x.PatNum).Contains(PIn.Long(rawPayPlan.Rows[i]["PatNum"].ToString()))){
						continue;
					}
					row=table.NewRow();
					row["AbbrDesc"]="";
					row["AdjNum"]="0";
					row["balance"]="";//fill this later
					row["balanceDouble"]=0;//fill this later
					row["chargesDouble"]=0;
					row["charges"]="";
					row["ClaimNum"]="0";
					row["ClaimPaymentNum"]="0";
					row["clinic"]="";
					row["colorText"]=arrayDefs[(int)DefCat.AccountColors][6].ItemColor.ToArgb().ToString();
					//amt=PIn.PDouble(rawPayPlan.Rows[i]["principal_"].ToString());
					amt=PIn.Decimal(rawPayPlan.Rows[i]["CompletedAmt"].ToString());
					row["creditsDouble"]=amt;
					row["credits"]=((decimal)row["creditsDouble"]).ToString("n");
					dateT=PIn.DateT(rawPayPlan.Rows[i]["PayPlanDate"].ToString());
					row["DateTime"]=dateT;
					row["date"]=dateT.ToString(Lans.GetShortDateTimeFormat());
					if(rawPayPlan.Rows[i]["PlanNum"].ToString()=="0"){
						row["description"]=Lans.g("ContrAccount","Payment Plan");
					}
					else{
						row["description"]=Lans.g("ContrAccount","Expected payments from ")
							+rawPayPlan.Rows[i]["CarrierName"].ToString();
					}
					//row["extraDetail"]="";
					string patname=fam.GetNameInFamFirst(PIn.Long(rawPayPlan.Rows[i]["PatNum"].ToString()));
					if(isReseller) {
						patname=fam.GetNameInFamLF(PIn.Long(rawPayPlan.Rows[i]["PatNum"].ToString()));
					}
					row["patient"]=patname;
					row["PatNum"]=rawPayPlan.Rows[i]["PatNum"].ToString();
					row["PayNum"]="0";
					row["PayPlanNum"]=rawPayPlan.Rows[i]["PayPlanNum"].ToString();
					row["PayPlanChargeNum"]="0";
					row["ProcCode"]=Lans.g("AccountModule","PayPln");
					row["ProcNum"]="0";
					row["ProcNumLab"]="";
					row["procsOnObj"]="";
					row["prov"]="";
					row["signed"]="";
					row["StatementNum"]="0";
					row["ToothNum"]="";
					row["ToothRange"]="";
					row["tth"]="";
					rows.Add(row);
				}
			}
			#endregion Payment Plans
			#region Payment Plans Version 2
			if(payPlanVersionCur==2) { //this information is only required for v2
				command="SELECT ChargeDate,payplancharge.PatNum,payplancharge.Guarantor,ProvNum,ClinicNum,payplancharge.Note,Principal,ChargeType,"
					+"Interest,payplancharge.PayPlanNum,PayPlanChargeNum,IsClosed,ProcNum,payplan.PlanNum,carrier.CarrierName "
					+"FROM payplancharge "
					+"INNER JOIN payplan ON payplan.PayPlanNum = payplancharge.PayPlanNum "
					+"LEFT JOIN insplan ON insplan.PlanNum = payplan.PlanNum "
					+"LEFT JOIN carrier ON carrier.CarrierNum = insplan.CarrierNum "
					+"WHERE (payplancharge.Guarantor IN ("+familyPatNums+") OR payplancharge.PatNum IN ("+familyPatNums+")) "
					+"AND ChargeDate<="+DbHelper.Curdate()+" "
					+"ORDER BY ChargeDate";
				DataTable rawPayPlan2=new DataTable();
				if(!isInvoice && stmt.StatementType!=StmtType.LimitedStatement) {
					rawPayPlan2=dcon.GetTable(command);
				}
				//0 rows if isInvoice or statement type is LimitedStatement.  In spite of this, the payment plans breakdown will still show at the top of invoices.
				for(int i=0;i<rawPayPlan2.Rows.Count;i++) {
					if(rawPayPlan2.Rows[i]["PlanNum"].ToString()!="0" && PIn.Int(rawPayPlan2.Rows[i]["ChargeType"].ToString())==(int)PayPlanChargeType.Debit) {
						//debits attached to insurance payplans do not get shown in the account module.
						continue;
					}
					if(PIn.Int(rawPayPlan2.Rows[i]["ChargeType"].ToString()) == (int)PayPlanChargeType.Debit 
						&& !fam.ListPats.Select(x => x.PatNum).Contains(PIn.Long(rawPayPlan2.Rows[i]["Guarantor"].ToString()))) 
					{
						//Any debit of which no one in the current family is the guarantor should be skipped. 
						//Credits get to pass, as they are posted to the patient of the payplan.
						continue;
					}
					row=table.NewRow();
					row["AbbrDesc"]="";
					row["AdjNum"]="0";
					row["balance"]="";//fill this later
					row["balanceDouble"]=0;//fill this later
					row["chargesDouble"]=0;
					int chargetype=PIn.Int(rawPayPlan2.Rows[i]["ChargeType"].ToString());
					amt=PIn.Decimal(rawPayPlan2.Rows[i]["Principal"].ToString());
					if(chargetype==(int)PayPlanChargeType.Debit) {//show principle amount as a charge if it's a debit chargeType.
						amt+=PIn.Decimal(rawPayPlan2.Rows[i]["Interest"].ToString());
						row["chargesDouble"]=amt;
						row["charges"]=amt.ToString("n");
						row["creditsDouble"]=0;
						row["credits"]="";
					}
					else if(chargetype==(int)PayPlanChargeType.Credit) {//show principle amount as a credit if it's a credit chargeType.
						row["chargesDouble"]=0;
						row["charges"]="";
						row["creditsDouble"]=amt;
						row["credits"]=amt.ToString("n");
					}
					row["ClaimNum"]="0";
					row["ClaimPaymentNum"]="0";
					row["clinic"]="";
					row["colorText"]=arrayDefs[(int)DefCat.AccountColors][6].ItemColor.ToArgb().ToString();
					dateT=PIn.DateT(rawPayPlan2.Rows[i]["ChargeDate"].ToString());
					row["DateTime"]=dateT;
					row["date"]=dateT.ToString(Lans.GetShortDateTimeFormat());
					if(rawPayPlan2.Rows[i]["PlanNum"].ToString()=="0") { //not an insurance payplan
						row["description"]=rawPayPlan2.Rows[i]["Note"].ToString();
					}
					else {//an insurance payplan
						row["description"]=Lans.g("ContrAccount","Expected payments from ")
							+rawPayPlan2.Rows[i]["CarrierName"].ToString();
					}
					//row["extraDetail"]="";
					string patname=fam.GetNameInFamFirst(PIn.Long(rawPayPlan2.Rows[i]["PatNum"].ToString()));
					if(isReseller) {
						patname=fam.GetNameInFamLF(PIn.Long(rawPayPlan2.Rows[i]["PatNum"].ToString()));
					}
					row["patient"]=patname;
					if(rawPayPlan2.Rows[i]["PlanNum"].ToString()=="0") { //not an insurance payplan
						//The guarantor on the payplancharge is always set to the account it should appear in for patient payment plans.
						row["PatNum"]=rawPayPlan2.Rows[i]["Guarantor"].ToString();
					}
					else {//an insurance payplan
						//For insurance payment plans, the charges should appear on the patient's account.
						row["PatNum"]=rawPayPlan2.Rows[i]["PatNum"].ToString();
					}
					row["PayNum"]="0";
					row["PayPlanNum"]=rawPayPlan2.Rows[i]["PayPlanNum"].ToString();
					row["PayPlanChargeNum"]=rawPayPlan2.Rows[i]["PayPlanChargeNum"].ToString();
					row["ProcCode"]=Lans.g("AccountModule","PayPln:")+" "+(PayPlanChargeType)(PIn.Int(rawPayPlan2.Rows[i]["ChargeType"].ToString()));
					row["ProcNum"]="0";
					row["ProcNumLab"]="";
					row["procsOnObj"]=rawPayPlan2.Rows[i]["ProcNum"].ToString();
					row["prov"]=Providers.GetAbbr(PIn.Int(rawPayPlan2.Rows[i]["ProvNum"].ToString()));
					row["signed"]="";
					row["StatementNum"]="0";
					row["ToothNum"]="";
					row["ToothRange"]="";
					row["tth"]="";
					rows.Add(row);
				}
			}
			#endregion Payment Plans Version 2
			#region Installment Plans
			//Installment plans----------------------------------------------------------------------------------
			if(statementNum==0) {
				command="SELECT * FROM installmentplan WHERE PatNum IN ("+familyPatNums+")";
				DataTable rawInstall=Db.GetTable(command);
				retVal.Tables.Add(GetPayPlans(rawPayPlan,rawPay,rawInstall,rawClaimPay,fam,pat));
			}
			else {
				//Always includes the payment plan breakdown for statements, receipts, and invoices.  LimitedStatements will return an empty payplan table.
				retVal.Tables.Add(GetPayPlansForStatement(rawPayPlan,rawPay,fromDate,toDate,singlePatient,rawClaimPay,fam,pat,out payPlanDue,stmt.StatementType));
			}
			#endregion Installment Plans
			//Sorting-----------------------------------------------------------------------------------------
			if(isForStatementPrinting && !intermingled) {
				rows.Sort(SortRowsForStatmentPrinting);
			}
			else {
				rows.Sort(new AccountLineComparer());
			}
			//Canadian lab procedures need to come immediately after their corresponding proc---------------------------------
			foreach(DataRow labRow in labRows) {
				int labRowIndex=rows.FindIndex(x => x["ProcNum"].ToString()==labRow["ProcNumLab"].ToString())+1;//+1 insert just after proc
				if(labRowIndex>0) {//must come after proc, so if index==0 no proc was found, don't insert
					rows.Insert(labRowIndex,labRow);
				}
			}
			//Pass off all the rows for the whole family in order to compute the patient balances----------------
			retVal.Tables.Add(GetPatientTable(fam,rows,isInvoice,stmt.StatementType));
			//Regroup rows by patient---------------------------------------------------------------------------
			DataTable[] rowsByPat=null;//will only used if multiple patients not intermingled
			if(singlePatient) {//This is usually used for Account module grid.  Always gets used for superstatements.
				Patient patGuarantor=Patients.GetFamily(patNum).ListPats[0];
				rows.RemoveAll(x => PIn.Long(x["SuperFamily"].ToString())!=0 && !patGuarantor.HasSuperBilling);
				rows.RemoveAll(x => PIn.Long(x["SuperFamily"].ToString())==0 && x["PatNum"].ToString()!=patNum.ToString());
			}
			else if(!intermingled) {//multiple patients not intermingled.  This is most common for an ordinary statement.  Never gets used with superstatements.
				rows.ForEach(x => table.Rows.Add(x));
				rowsByPat=new DataTable[fam.ListPats.Length];
				DataTable tableCur;
				for(int i=0;i<rowsByPat.Length;i++) {
					tableCur=new DataTable();
					SetTableColumns(tableCur);
					rows.FindAll(x => x["PatNum"].ToString()==fam.ListPats[i].PatNum.ToString()).ForEach(x => tableCur.ImportRow(x));
					rowsByPat[i]=tableCur;
				}
			}
			//Compute balances-------------------------------------------------------------------------------------
			decimal bal;
			if(rowsByPat==null){//just one table
				bal=0;
				foreach(DataRow rowCur in rows) {
					bal+=(decimal)rowCur["chargesDouble"];
					bal-=(decimal)rowCur["creditsDouble"];
					rowCur["balanceDouble"]=bal;
					if(rowCur["ClaimPaymentNum"].ToString()=="0" && rowCur["ClaimNum"].ToString()!="0"){//claims
						rowCur["balance"]="";
					}
					else if(rowCur["StatementNum"].ToString()=="0") {
						rowCur["balance"]=bal.ToString("n");
					}
				}
			}
			else {//family rows
				foreach(DataTable patTable in rowsByPat) {
					bal=0;
					foreach(DataRow rowCur in patTable.Rows) {
						bal+=(decimal)rowCur["chargesDouble"];
						bal-=(decimal)rowCur["creditsDouble"];
						rowCur["balanceDouble"]=bal;
						if(rowCur["ClaimPaymentNum"].ToString()=="0" && rowCur["ClaimNum"].ToString()!="0") {//claims
							rowCur["balance"]="";
						}
						else if(rowCur["StatementNum"].ToString()=="0") {
							rowCur["balance"]=bal.ToString("n");
						}
					}
				}
			}
			//Remove rows outside of daterange-------------------------------------------------------------------
			bool foundBalForward;
			long pnum=pat.Guarantor;//the patnum that should be put on the Balance foreward row.
			if(rowsByPat==null && stmt.StatementType!=StmtType.LimitedStatement) {//LimitedStatements don't have a balance forward row
				foundBalForward=false;
				for(int i=rows.Count-1;i>=0;i--) {//go backwards and remove from end
					if(((DateTime)rows[i]["DateTime"])>toDate){
						rows.RemoveAt(i);
					}
					else if(((DateTime)rows[i]["DateTime"])<fromDate){
						if(!foundBalForward){
							foundBalForward=true;
							balanceForward=(decimal)rows[i]["balanceDouble"];
						}
						rows.RemoveAt(i);
					}
				}
				//Add balance forward row
				if(foundBalForward && (rows.Count>0 || balanceForward>0)) {//don't add bal forward row if it will be the only row and there is no balance
					row=table.NewRow();
					SetBalForwardRow(row,balanceForward,pnum);
					rows.Insert(0,row);
				}
			}
			else if(stmt.StatementType!=StmtType.LimitedStatement) {//LimitedStatements don't have a balance forward row
				for(int p=0;p<rowsByPat.Length;p++){
					foundBalForward=false;
					for(int i=rowsByPat[p].Rows.Count-1;i>=0;i--) {//go backwards and remove from end
						if(((DateTime)rowsByPat[p].Rows[i]["DateTime"])>toDate){
							rowsByPat[p].Rows.RemoveAt(i);
						}
						else if(((DateTime)rowsByPat[p].Rows[i]["DateTime"])<fromDate){
							if(!foundBalForward){
								foundBalForward=true;
								balanceForward=(decimal)rowsByPat[p].Rows[i]["balanceDouble"];
							}
							long.TryParse(rowsByPat[p].Rows[i]["PatNum"].ToString(),out pnum);//using TryParse so no try-catch and pnum will be 0 if invalid
							rowsByPat[p].Rows.RemoveAt(i);
						}
					}
					//Add balance forward row
					if(foundBalForward && (rowsByPat[p].Rows.Count>0 || balanceForward>0)) {//don't add bal forward row if it will be the only row and there is no balance
						row=rowsByPat[p].NewRow();
						SetBalForwardRow(row,balanceForward,pnum);
						rowsByPat[p].Rows.InsertAt(row,0);
					}
				}
			}
			//Finally, add rows to new table(s)-----------------------------------------------------------------------
			if(rowsByPat==null){
				table.Rows.Clear();
				rows.ForEach(x => table.Rows.Add(x));
				retVal.Tables.Add(table);
			}
			else{
				DataTable tablep;
				for(int p=0;p<rowsByPat.Length;p++){
					Patient patRowCur=fam.ListPats[p];
					if(p>0 && statementNum>0 && patRowCur.PatStatus!=PatientStatus.Patient && patRowCur.EstBalance==0 ){
						continue;
					}
					tablep=new DataTable("account"+patRowCur.PatNum.ToString());
					SetTableColumns(tablep);
					rowsByPat[p].Rows.OfType<DataRow>().ToList().ForEach(x => tablep.ImportRow(x));
					retVal.Tables.Add(tablep);
				}
			}
			return retVal;
		}

		private static void SetBalForwardRow(DataRow row,decimal amt,long patNum){
			//No need to check RemotingRole; no call to db.
			row["AdjNum"]="0";
			row["balance"]=amt.ToString("n");
			row["balanceDouble"]=amt;
			row["chargesDouble"]=0;
			row["charges"]="";
			row["ClaimNum"]="0";
			row["ClaimPaymentNum"]="0";
			row["colorText"]=Color.Black.ToArgb().ToString();
			row["creditsDouble"]="0";
			row["credits"]="";
			row["DateTime"]=DateTime.MinValue;
			row["date"]="";
			row["description"]=Lans.g("AccountModule","Balance Forward");
			row["patient"]="";
			row["PatNum"]=patNum;
			row["PayNum"]="0";
			row["PayPlanNum"]="0";
			row["PayPlanChargeNum"]="0";
			row["ProcCode"]="";
			row["ProcNum"]="0";
			row["procsOnObj"]="";
			row["prov"]="";
			row["signed"]="";
			row["StatementNum"]="0";
			row["tth"]="";
		}

		///<summary>Gets payment plans for the family.  This defines what will show in the PayPlans grid in the account module.
		///RawPay will include any paysplits for anyone in the family plus any paysplits for payment plans being paid by someone outside the family. </summary>
		private static DataTable GetPayPlans(DataTable rawPayPlan,DataTable rawPay,DataTable rawInstall,DataTable rawClaimPay, Family fam, Patient pat) {
			//No need to check RemotingRole; no call to db.
			DataConnection dcon=new DataConnection();
			DataTable table=new DataTable("payplan");
			DataRow row;
			table.Columns.Add("accumDue");
			table.Columns.Add("balance");
			table.Columns.Add("date");
			table.Columns.Add("DateTime",typeof(DateTime));
			table.Columns.Add("due");
			table.Columns.Add("guarantor");
			table.Columns.Add("InstallmentPlanNum");
			table.Columns.Add("IsClosed");
			table.Columns.Add("paid");
			table.Columns.Add("patient");
			table.Columns.Add("PatNum");
			table.Columns.Add("PayPlanNum");
			table.Columns.Add("principal");
			table.Columns.Add("princPaid");
			table.Columns.Add("totalCost");
			table.Columns.Add("type");
			List<DataRow> rows=new List<DataRow>();
			DateTime dateT;
			decimal paid;
			decimal princ;
			decimal princDue;
			decimal interestDue;
			decimal accumDue;
			decimal princPaid;
			decimal totCost;
			decimal due;
			decimal balance;
			for(int i=0;i<rawPayPlan.Rows.Count;i++){
				//first, calculate the numbers-------------------------------------------------------------
				paid=0;
				for(int p=0;p<rawPay.Rows.Count;p++){
					if(rawPay.Rows[p]["PayPlanNum"].ToString()==rawPayPlan.Rows[i]["PayPlanNum"].ToString()){
						paid+=PIn.Decimal(rawPay.Rows[p]["splitAmt_"].ToString());
					}
				}
				for(int c=0;c<rawClaimPay.Rows.Count;c++) {
					if(rawClaimPay.Rows[c]["PayPlanNum"].ToString()==rawPayPlan.Rows[i]["PayPlanNum"].ToString()) {
						paid+=PIn.Decimal(rawClaimPay.Rows[c]["InsPayAmtPayPlan"].ToString());
					}
				}
				princ=PIn.Decimal(rawPayPlan.Rows[i]["principal_"].ToString());
				princDue=PIn.Decimal(rawPayPlan.Rows[i]["principalDue_"].ToString());
				interestDue=PIn.Decimal(rawPayPlan.Rows[i]["interestDue_"].ToString());
				accumDue=princDue+interestDue;
				princPaid=paid-interestDue;
				if(princPaid<0){
					princPaid=0;
				}
				totCost=princ+PIn.Decimal(rawPayPlan.Rows[i]["interest_"].ToString());
				due=accumDue-paid;
				balance=princ-princPaid;
				//then fill the row----------------------------------------------------------------------
				row=table.NewRow();
				row["accumDue"]=accumDue.ToString("n");
				row["balance"]=balance.ToString("n");
				dateT=PIn.DateT(rawPayPlan.Rows[i]["PayPlanDate"].ToString());
				row["DateTime"]=dateT;
				row["date"]=dateT.ToShortDateString();
				if(rawPayPlan.Rows[i]["PlanNum"].ToString()=="0") {
					row["due"]=due.ToString("n");
				}
				else {
					row["due"]=0.ToString("n");
				}
				row["guarantor"]=fam.GetNameInFamLF(PIn.Long(rawPayPlan.Rows[i]["Guarantor"].ToString()));
				row["InstallmentPlanNum"]="0";
				row["IsClosed"]=rawPayPlan.Rows[i]["IsClosed"].ToString();
				row["paid"]=paid.ToString("n");
				row["patient"]=fam.GetNameInFamLF(PIn.Long(rawPayPlan.Rows[i]["PatNum"].ToString()));
				row["PatNum"]=rawPayPlan.Rows[i]["PatNum"].ToString();
				row["PayPlanNum"]=rawPayPlan.Rows[i]["PayPlanNum"].ToString();
				row["principal"]=princ.ToString("n");
				row["princPaid"]=princPaid.ToString("n");
				row["totalCost"]=totCost.ToString("n");
				if(rawPayPlan.Rows[i]["PlanNum"].ToString()=="0"){
					row["type"]="PP";
				}
				else{
					row["type"]="Ins";
				}
				rows.Add(row);
			}
			//Installment plans-------------------------------------------------------------------------
			for(int i=0;i<rawInstall.Rows.Count;i++){
				row=table.NewRow();
				row["accumDue"]="";
				row["balance"]="";
				dateT=PIn.DateT(rawInstall.Rows[i]["DateAgreement"].ToString());
				row["DateTime"]=dateT;
				row["date"]=dateT.ToShortDateString();
				row["due"]=PIn.Decimal(rawInstall.Rows[i]["MonthlyPayment"].ToString()).ToString("f");
				row["guarantor"]="";
				row["InstallmentPlanNum"]=PIn.Long(rawInstall.Rows[i]["InstallmentPlanNum"].ToString());
				row["IsClosed"]="0"; //installment plans are never closed.
				row["paid"]="";
				row["patient"]=fam.GetNameInFamLF(PIn.Long(rawInstall.Rows[i]["PatNum"].ToString()));
				row["PatNum"]=rawInstall.Rows[i]["PatNum"].ToString();
				row["PayPlanNum"]="0";
				row["principal"]="";
				row["princPaid"]="";
				row["totalCost"]="";
				row["type"]="IP";
				rows.Add(row);
			}
			for(int i=0;i<rows.Count;i++) {
				table.Rows.Add(rows[i]);
			}
			return table;
		}

		///<summary>Gets payment plans for the family.  RawPay will include any paysplits for anyone in the family plus splits for payment plans being 
		///paid by someone outside the family.  fromDate and toDate are only used if isForStatement.  From date lets us restrict how many
		///amortization items to show.  toDate is typically 10 days in the future.  This method cannot be called by the Middle Tier due to its use of an
		///out parameter.  LimitedStatements will return an empty DataTable.</summary>
		private static DataTable GetPayPlansForStatement(DataTable rawPayPlan,DataTable rawPay,DateTime fromDate,DateTime toDate,bool singlePatient,
			DataTable rawClaimPay,Family fam,Patient pat,out decimal payPlanDue,StmtType statementType)
		{
			//No need to check RemotingRole; no call to db.
			//We may need to add installment plans to this grid some day.  No time right now.
			DataTable table=new DataTable("payplan");
			DataRow row;
			SetTableColumns(table);//this will allow it to later be fully integrated into a single grid.
			payPlanDue=0;
			if(statementType==StmtType.LimitedStatement) {//don't include payment plans on LimitedStatements
				return table;
			}
			List<DataRow> rows=new List<DataRow>();
			decimal princ;
			decimal bal;
			DataTable rawAmort;
			long payPlanNum;
			for(int i=0;i<rawPayPlan.Rows.Count;i++){//loop through the payment plans (usually zero or one)
				princ=PIn.Decimal(rawPayPlan.Rows[i]["principal_"].ToString());
				bal=princ;
				for(int p=0;p<rawPay.Rows.Count;p++){
					if(rawPay.Rows[p]["PayPlanNum"].ToString()==rawPayPlan.Rows[i]["PayPlanNum"].ToString()){
						bal-=PIn.Decimal(rawPay.Rows[p]["splitAmt_"].ToString());
					}
				}
				for(int c=0;c<rawClaimPay.Rows.Count;c++) {
					if(rawClaimPay.Rows[c]["PayPlanNum"].ToString()==rawPayPlan.Rows[i]["PayPlanNum"].ToString()) {
						bal-=PIn.Decimal(rawClaimPay.Rows[c]["InsPayAmtPayPlan"].ToString());
					}
				}       
				//If on version 1, don't show closed plans with nothing due. If on version 2, don't show closed payplans at all.
				if(rawPayPlan.Rows[i]["IsClosed"].ToString()=="1" && (PrefC.GetInt(PrefName.PayPlansVersion) == 2 || bal == 0)) {
					continue;
				}
				//summary row----------------------------------------------------------------------
				row=table.NewRow();
				row["AdjNum"]="0";
				row["balance"]="";
				row["balanceDouble"]=0;
				row["chargesDouble"]=0;
				row["charges"]="";
				row["ClaimNum"]="0";
				row["ClaimPaymentNum"]="0";
				row["colorText"]=Color.Black.ToArgb().ToString();
				row["creditsDouble"]=0;
				row["credits"]="";
				row["DateTime"]=DateTime.MinValue;
				row["date"]="";
				row["description"]=Lans.g("AccountModule","Payment Plan.")+"\r\n"
					+Lans.g("AccountModule","Total loan amount: ")+princ.ToString("c")+"\r\n"
					+Lans.g("AccountModule","Principal Remaining: ")+bal.ToString("c");
				if(rawPayPlan.Rows[i]["PlanNum"].ToString()!="0"){
					//row["description"]+="\r\n"+Lans.g("AccountModule","This 'payment plan' is only used ");
					continue;//don't show insurance payment plans on statements.
					//Although if they are properly deleting insurance pp charges, no such pp's will be here anyway.
				}
				//so all payment plans will have a patient.
				//If Single Patient is checked, then we only want to show payment plans where the selected patient is either the PayPlan's Patient or Guarantor.
				if(singlePatient
					&& rawPayPlan.Rows[i]["Guarantor"].ToString()!=pat.PatNum.ToString()
					&& rawPayPlan.Rows[i]["PatNum"].ToString()!=pat.PatNum.ToString()) 
				{
					continue;
				}
				row["description"]+="\r\nPatient: "+fam.GetNameInFamLF(PIn.Long(rawPayPlan.Rows[i]["PatNum"].ToString()));
				row["patient"]="";
				row["PatNum"]=rawPayPlan.Rows[i]["PatNum"].ToString();
				row["PayNum"]="0";
				row["PayPlanNum"]="0";
				row["PayPlanChargeNum"]="0";
				row["ProcCode"]="";
				row["ProcNum"]="0";
				row["procsOnObj"]="";
				row["prov"]="";
				row["signed"]="";
				row["StatementNum"]="0";
				row["tth"]="";
				rows.Add(row);
				//detail rows-------------------------------------------------------------------------------
				payPlanNum=PIn.Long(rawPayPlan.Rows[i]["PayPlanNum"].ToString());
				rawAmort=GetPayPlanAmortTable(payPlanNum);
				//remove future entries, going backwards
				for(int d=rawAmort.Rows.Count-1;d>=0;d--) {
					if((DateTime)rawAmort.Rows[d]["DateTime"]>toDate.AddDays(PrefC.GetLong(PrefName.PayPlansBillInAdvanceDays))) {
						rawAmort.Rows.RemoveAt(d);
					}
				}
				//grab the payPlanDue amount from the last row
				if(rawAmort.Rows.Count>0) {
					payPlanDue+=(decimal)rawAmort.Rows[rawAmort.Rows.Count-1]["balanceDouble"];
				}
				//remove old entries, going backwards
				for(int d=rawAmort.Rows.Count-1;d>=0;d--){
					if((DateTime)rawAmort.Rows[d]["DateTime"]<fromDate){
						rawAmort.Rows.RemoveAt(d);
					}
				}
				for(int d=0;d<rawAmort.Rows.Count;d++){
					row=table.NewRow();
					row["AdjNum"]="0";
					row["balance"]=rawAmort.Rows[d]["balance"];
					row["balanceDouble"]=rawAmort.Rows[d]["balanceDouble"];
					row["chargesDouble"]=rawAmort.Rows[d]["chargesDouble"];
					row["charges"]=rawAmort.Rows[d]["charges"];
					row["ClaimNum"]="0";
					row["ClaimPaymentNum"]="0";
					row["colorText"]=Color.Black.ToArgb().ToString();
					row["creditsDouble"]=rawAmort.Rows[d]["creditsDouble"];
					row["credits"]=rawAmort.Rows[d]["credits"];
					row["DateTime"]=rawAmort.Rows[d]["DateTime"];
					row["date"]=rawAmort.Rows[d]["date"];
					row["description"]=rawAmort.Rows[d]["description"];
					row["patient"]=rawAmort.Rows[d]["patient"];
					row["PatNum"]=rawAmort.Rows[d]["PatNum"];
					row["PayNum"]=rawAmort.Rows[d]["PayNum"];
					row["PayPlanNum"]="0";
					row["PayPlanChargeNum"]=rawAmort.Rows[d]["PayPlanChargeNum"];
					row["ProcCode"]="";
					row["ProcNum"]="0";
					row["procsOnObj"]="";
					row["prov"]=rawAmort.Rows[d]["prov"];
					row["signed"]="";
					row["StatementNum"]="0";
					row["tth"]="";
					rows.Add(row);
				}
			}
			for(int i=0;i<rows.Count;i++) {
				table.Rows.Add(rows[i]);
			}
			return table;
		}

		///<summary>All rows for the entire family are getting passed in here.  (Except Invoices)  The rows have already been sorted.  Balances have not been computed, and we will do that here, separately for each patient (except invoices).</summary>
		private static DataTable GetPatientTable(Family fam,List<DataRow> rows,bool isInvoice,StmtType statementType){
			//No need to check RemotingRole; no call to db.
			//DataConnection dcon=new DataConnection();
			DataTable table=new DataTable("patient");
			DataRow row;
			table.Columns.Add("balance");
			table.Columns.Add("balanceDouble",typeof(decimal));
			table.Columns.Add("name");
			table.Columns.Add("PatNum");
			List<DataRow> rowspat=new List<DataRow>();
			decimal bal;
			decimal balfam=0;
			for(int p=0;p<fam.ListPats.Length;p++){
				row=table.NewRow();
				bal=0;
				for(int i=0;i<rows.Count;i++) {
					if(fam.ListPats[p].PatNum.ToString()==rows[i]["PatNum"].ToString()){
						bal+=(decimal)rows[i]["chargesDouble"];
						bal-=(decimal)rows[i]["creditsDouble"];
					}
				}
				balfam+=bal;
				row["balanceDouble"]=bal;
				row["balance"]=bal.ToString("n");
				row["name"]=fam.ListPats[p].GetNameLF();
				row["PatNum"]=fam.ListPats[p].PatNum.ToString();
				rowspat.Add(row);
				if(isInvoice || statementType==StmtType.LimitedStatement) {
					//we don't have all the rows, so we don't want to try to compute balance
				}
				else {
					if((double)bal!=fam.ListPats[p].EstBalance) {
						Patient patnew=fam.ListPats[p].Copy();
						patnew.EstBalance=(double)bal;
						Patients.Update(patnew,fam.ListPats[p]);
					}
				}
			}
			//Row for entire family
			row=table.NewRow();
			row["balanceDouble"]=balfam;
			row["balance"]=balfam.ToString("f");
			row["name"]=Lans.g("AccountModule","Entire Family");
			row["PatNum"]=fam.ListPats[0].PatNum.ToString();
			rowspat.Add(row);
			for(int i=0;i<rowspat.Count;i++) {
				table.Rows.Add(rowspat[i]);
			}
			return table;
		}

		///<summary>Future appointments.</summary>
		private static DataTable GetApptTable(Family fam,bool singlePatient,long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),fam,singlePatient,patNum);
			}
			DataConnection dcon=new DataConnection();
			DataTable table=new DataTable("appts");
			DataRow row;
			table.Columns.Add("descript");
			table.Columns.Add("PatNum");
			List<DataRow> rows=new List<DataRow>();
			string command="SELECT AptDateTime,PatNum,ProcDescript "
				+"FROM appointment "
				+"WHERE AptDateTime > "+POut.DateT(DateTime.Now)+" "//Today.AddDays(1) midnight tonight
				+"AND AptStatus !="+POut.Long((int)ApptStatus.PtNote)+" "
				+"AND AptStatus !="+POut.Long((int)ApptStatus.PtNoteCompleted)+" "
				+"AND AptStatus !="+POut.Long((int)ApptStatus.UnschedList)+" "
				+"AND (";
			if(singlePatient){
				command+="PatNum ="+POut.Long(patNum);
			}
			else{
				for(int i=0;i<fam.ListPats.Length;i++){
					if(i!=0){
						command+="OR ";
					}
					command+="PatNum ="+POut.Long(fam.ListPats[i].PatNum)+" ";
				}
			}
			command+=") ORDER BY PatNum,AptDateTime";
			DataTable raw=dcon.GetTable(command);
			DateTime dateT;
			long patNumm;
			for(int i=0;i<raw.Rows.Count;i++){
				row=table.NewRow();
				patNumm=PIn.Long(raw.Rows[i]["PatNum"].ToString());
				dateT=PIn.DateT(raw.Rows[i]["AptDateTime"].ToString());
				row["descript"]=fam.GetNameInFamFL(patNumm)+":  "
					+dateT.ToString("dddd")+",  "
					+dateT.ToShortDateString()
					+",  "+dateT.ToShortTimeString()+",  "+raw.Rows[i]["ProcDescript"].ToString();
				row["PatNum"]=patNumm.ToString();
				rows.Add(row);
			}
			for(int i=0;i<rows.Count;i++) {
				table.Rows.Add(rows[i]);
			}
			return table;
		}

		public static DataTable GetMisc(Family fam,long patNum,decimal payPlanDue,decimal balanceForward,StmtType statementType,DataSet ds) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),fam,patNum,payPlanDue,balanceForward,statementType,ds);
			}
			DataTable table=new DataTable("misc");
			DataRow row;
			table.Columns.Add("descript");
			table.Columns.Add("value");
			List<DataRow> rows=new List<DataRow>();
			//FamFinancial note--------------------
			string command = 
				"SELECT FamFinancial "
				+"FROM patientnote WHERE patnum ="+POut.Long(fam.ListPats[0].PatNum);
			DataTable raw=Db.GetTable(command);
			row=table.NewRow();
			row["descript"]="FamFinancial";
			row["value"]="";
			if(raw.Rows.Count==1){
				row["value"]=PIn.String(raw.Rows[0][0].ToString());
			}
			rows.Add(row);
			//payPlanDue---------------------------
			row=table.NewRow();
			row["descript"]="payPlanDue";
			row["value"]=POut.Decimal(payPlanDue);
			rows.Add(row);
			//balanceForward-----------------------
			row=table.NewRow();
			row["descript"]="balanceForward";
			row["value"]=POut.Decimal(balanceForward);
			rows.Add(row);
			//patInsEst----------------------------
			string procNumsForInsEst="";
			command="SELECT COALESCE(SUM(inspayest+writeoff),0) FROM claimproc "
				+"WHERE status = 0 ";//not received
			if(statementType!=StmtType.LimitedStatement) {
				command+="AND PatNum="+POut.Long(patNum);
			}
			else {
				procNumsForInsEst=string.Join(",",ds.Tables.OfType<DataTable>()//only reference to ds, should never be null if it's a LimitedStatement
					.Where(x => x.TableName.StartsWith("account"))
					.SelectMany(x => x.Rows.OfType<DataRow>()
						.Select(y => POut.String(y["ProcNum"].ToString()))
						.Where(y => y!="0")));
				command+="AND ProcNum IN ("+procNumsForInsEst+")";
			}
			row=table.NewRow();
			if(statementType!=StmtType.LimitedStatement || procNumsForInsEst!="") {//don't run if LimitedStatement and no procs selected
				raw=Db.GetTable(command);
				row["descript"]="patInsEst";
				row["value"]=raw.Rows[0][0].ToString();
			}
			rows.Add(row);
			//Unearned income----------------------
			command="SELECT SUM(SplitAmt) FROM paysplit WHERE "
				+"UnearnedType>0 AND (";
			for(int i=0;i<fam.ListPats.Length;i++) {
				if(i>0) {
					command+=" OR ";
				}
				command+="PatNum= "+POut.Long(fam.ListPats[i].PatNum);
			}
			command+=")";
			//Unearned Amount from this datatable is deprecated.  Account module uses S-class methods to calculate it now.
			double unearnedAmt=PIn.Double(Db.GetScalar(command));
			row=table.NewRow();
			row["descript"]="unearnedIncome";
			row["value"]=unearnedAmt;
			rows.Add(row);
			//final prep:
			for(int i=0;i<rows.Count;i++) {
				table.Rows.Add(rows[i]);
			}
			return table;
		}

		///<summary>Used to resort data rows used for printing main account grid on statements.</summary>
		public static int SortRowsForStatmentPrinting(DataRow x,DataRow y) {
			if(x["PatNum"].ToString()!=y["PatNum"].ToString()
			&&(x["PatNum"].ToString()!="0" && y["PatNum"].ToString()!="0")) {
				return x["PatNum"].ToString().CompareTo(y["PatNum"].ToString());
			}
			//if dates are different, then sort by date
			if(((DateTime)x["DateTime"]).Date!=((DateTime)y["DateTime"]).Date) {
				return (((DateTime)x["DateTime"]).Date).CompareTo(((DateTime)y["DateTime"]).Date);
			}
			//Sort by Type (right now just sorts procedures first and then adjustments...)
			if(x["ProcNum"].ToString()!="0" && y["ProcNum"].ToString()=="0") {
				return -1;
			}
			if(x["ProcNum"].ToString()=="0" && y["ProcNum"].ToString()!="0") {
				return 1;
			}
			//Put adjustments next
			if(x["AdjNum"].ToString()!="0" && y["AdjNum"].ToString()=="0") {
				return -1;
			}
			if(x["AdjNum"].ToString()=="0" && y["AdjNum"].ToString()!="0") {
				return 1;
			}
			//Payment plans and claim payments might need to be sorted in order for receipts to calculate the balance consistently.
			//Sort procedures by status, priority, tooth region/num, proc code
			if(x["ProcNum"].ToString()!="0" && y["ProcNum"].ToString()!="0") {//if both are procedures
				return ProcedureLogic.CompareProcedures(x,y);
			}
			return 0;
		}




	}

	///<summary>The supplied DataRows must include the following columns: ProcNum,DateTime,(Priority not needed),ToothRange,ToothNum,ProcCode. This sorts all objects in Account module based on their types, dates, toothrange, toothnum, and proccode.  Times are always ignored if present.</summary>
	class AccountLineComparer:IComparer<DataRow> {
		///<summary></summary>
		public int Compare(DataRow x,DataRow y) {
			//if dates are different, then sort by date
			if(((DateTime)x["DateTime"]).Date!=((DateTime)y["DateTime"]).Date) {
				return (((DateTime)x["DateTime"]).Date).CompareTo(((DateTime)y["DateTime"]).Date);
			}
			//Sort by Type (right now just sorts procedures first and then adjustments...)
			if(x["ProcNum"].ToString()!="0" && y["ProcNum"].ToString()=="0") {
				return -1;
			}
			if(x["ProcNum"].ToString()=="0" && y["ProcNum"].ToString()!="0") {
				return 1;
			}
			//Put adjustments next
			if(x["AdjNum"].ToString()!="0" && y["AdjNum"].ToString()=="0") {
				return -1;
			}
			if(x["AdjNum"].ToString()=="0" && y["AdjNum"].ToString()!="0") {
				return 1;
			}
			//Payment plans and claim payments might need to be sorted in order for receipts to calculate the balance consistently.
			//Sort procedures by status, priority, tooth region/num, proc code
			if(x["ProcNum"].ToString()!="0" && y["ProcNum"].ToString()!="0") {//if both are procedures
				return ProcedureLogic.CompareProcedures(x,y);
			}
			return 0;
		}
	}

	/*
	///<summary>A generic comparison that sorts the rows of the payplanamort table by date and type.</summary>
	class PayPlanLineComparer : IComparer<DataRow>	{
		///<summary>A generic comparison that sorts the rows of the payplanamort table by date and type.</summary>
		public int Compare (DataRow rowA,DataRow rowB){
			//if dates are different, then sort by date
			if((DateTime)rowA["DateTime"]!=(DateTime)rowB["DateTime"]){
				return ((DateTime)rowA["DateTime"]).CompareTo((DateTime)rowB["DateTime"]);
			}
			//Charges come before paysplits, but rare to be on same date anyway.
			if(rowA["PayPlanChargeNum"].ToString()!="0" && rowB["PaySplitNum"].ToString()=="0"){
				return -1;
			}
			if(rowA["PaySplitNum"].ToString()=="0" && rowB["PayPlanChargeNum"].ToString()!="0"){
				return 1;
			}
			return 0;
		}
	}*/


}
