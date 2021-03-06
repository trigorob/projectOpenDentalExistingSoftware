using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace OpenDentBusiness{
	
	///<summary></summary>
	public class Statements{
		///<Summary>Gets one statement from the database.</Summary>
		public static Statement GetStatement(long statementNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<Statement>(MethodBase.GetCurrentMethod(),statementNum);
			}
			return Crud.StatementCrud.SelectOne(statementNum);
		}

		public static List<Statement> GetStatements(List<long> listStatementNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Statement>>(MethodBase.GetCurrentMethod(),listStatementNums);
			}
			if(listStatementNums==null || listStatementNums.Count < 1) {
				return new List<Statement>();
			}
			string command="SELECT * FROM statement WHERE StatementNum IN ("+string.Join(",",listStatementNums)+")";
			return Crud.StatementCrud.SelectMany(command);
		}

		///<summary></summary>
		public static long Insert(Statement statement) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				statement.StatementNum=Meth.GetLong(MethodBase.GetCurrentMethod(),statement);
				return statement.StatementNum;
			}
			return Crud.StatementCrud.Insert(statement);
		}

		///<summary></summary>
		public static void Update(Statement statement) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),statement);
				return;
			}
			Crud.StatementCrud.Update(statement);
		}

		///<summary></summary>
		public static void Delete(Statement statement) {
			//No need to check RemotingRole; no call to db.
			Delete(statement.StatementNum);
		}

		///<summary>For deleting a statement when user clicks Cancel.  No need to make entry in DeletedObject table.</summary>
		public static void Delete(long statementNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),statementNum);
				return;
			}
			//Removed dependencies from procedures, adjustments, StmtProcAttaches, StmtAdjAttaches, and StmtPaySplitAttaches.
			Procedures.DetachFromInvoice(statementNum);
			StmtProcAttaches.DetachFromStatement(statementNum);
			Adjustments.DetachFromInvoice(statementNum);
			StmtAdjAttaches.DetachFromStatement(statementNum);
			StmtPaySplitAttaches.DetachFromStatement(statementNum);
			Crud.StatementCrud.Delete(statementNum);
		}

		public static void DeleteAll(List<long> listStatementNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),listStatementNums);
				return;
			}
			if(listStatementNums==null || listStatementNums.Count==0) {
				return;
			}
			//Removed dependencies from procedures, adjustments, StmtProcAttaches, StmtAdjAttaches, and StmtPaySplitAttaches.
			Procedures.DetachAllFromInvoices(listStatementNums);
			StmtProcAttaches.DetachAllFromStatements(listStatementNums);
			Adjustments.DetachAllFromInvoices(listStatementNums);
			StmtAdjAttaches.DetachAllFromStatements(listStatementNums);
			StmtPaySplitAttaches.DetachAllFromStatements(listStatementNums);
			string command="DELETE FROM statement WHERE StatementNum IN ("+string.Join(",",listStatementNums.Select(x => POut.Long(x)))+")";
			Db.NonQ(command);
		}

		///<summary>Queries the database to determine if there are any unsent statements.</summary>
		public static bool UnsentStatementsExist(){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod());
			}
			string command="SELECT COUNT(*) FROM statement WHERE IsSent=0";
			if(Db.GetCount(command)=="0"){
				return false;
			}
			return true;
		}

		///<summary>Queries the database to determine if there are any unsent statements for a particular clinic.</summary>
		public static bool UnsentClinicStatementsExist(long clinicNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod(),clinicNum);
			}
			if(clinicNum==0) {//All clinics.
				return UnsentStatementsExist();
			}
			string command=@"SELECT COUNT(*) FROM statement 
				LEFT JOIN patient ON statement.PatNum=patient.PatNum
				WHERE statement.IsSent=0
				AND patient.ClinicNum="+clinicNum;
			if(Db.GetCount(command)=="0") {
				return false;
			}
			return true;
		}

		public static void MarkSent(long statementNum,DateTime dateSent) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),statementNum,dateSent);
				return;
			}
			string command="UPDATE statement SET DateSent="+POut.Date(dateSent)+", "
				+"IsSent=1 WHERE StatementNum="+POut.Long(statementNum);
			Db.NonQ(command);
		}

		public static void AttachDoc(long statementNum,long docNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),statementNum,docNum);
				return;
			}
			string command="UPDATE statement SET DocNum="+POut.Long(docNum)
				+" WHERE StatementNum="+POut.Long(statementNum);
			Db.NonQ(command);
		}

		///<summary>For orderBy, use 0 for BillingType and 1 for PatientName.</summary>
		public static DataTable GetBilling(bool isSent,int orderBy,DateTime dateFrom,DateTime dateTo,List<long> clinicNums){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),isSent,orderBy,dateFrom,dateTo,clinicNums);
			}
			DataTable table=new DataTable();
			DataRow row;
			//columns that start with lowercase are altered for display rather than being raw data.
			table.Columns.Add("amountDue");
			table.Columns.Add("balTotal");
			table.Columns.Add("billingType");
			table.Columns.Add("insEst");
			table.Columns.Add("IsSent");
			table.Columns.Add("lastStatement");
			table.Columns.Add("mode");
			table.Columns.Add("name");
			table.Columns.Add("PatNum");
			table.Columns.Add("payPlanDue");
			table.Columns.Add("StatementNum");
			table.Columns.Add("SuperFamily");
			string command="SELECT patient.BalTotal,BillingType,FName,patient.InsEst,statement.IsSent,"
				+"IFNULL(MAX(s2.DateSent),"+POut.Date(DateTime.MinValue)+") LastStatement,"
				+"LName,MiddleI,statement.Mode_,PayPlanDue,Preferred,"
				+"statement.PatNum,statement.StatementNum,statement.SuperFamily "
				+"FROM statement "
				+"LEFT JOIN patient ON statement.PatNum=patient.PatNum "
				+"LEFT JOIN statement s2 ON s2.PatNum=patient.PatNum "
				+"AND s2.IsSent=1 ";
			if(PrefC.GetBool(PrefName.BillingIgnoreInPerson)) {
				command+="AND s2.Mode_ !=1 ";
			}
			if(orderBy==0){//BillingType
				command+="LEFT JOIN definition ON patient.BillingType=definition.DefNum ";
			}
			command+="WHERE statement.IsSent="+POut.Bool(isSent)+" ";
			//if(dateFrom.Year>1800){
			command+="AND statement.DateSent>="+POut.Date(dateFrom)+" ";//greater than midnight this morning
			//}
			//if(dateFrom.Year>1800){
			command+="AND statement.DateSent<"+POut.Date(dateTo.AddDays(1))+" ";//less than midnight tonight
			//}
			if(clinicNums.Count>0) {
				command+="AND patient.ClinicNum IN ("+string.Join(",",clinicNums)+") ";
			}
			command+="GROUP BY patient.BalTotal,BillingType,FName,patient.InsEst,statement.IsSent,"
				+"LName,MiddleI,statement.Mode_,PayPlanDue,Preferred,"
				+"statement.PatNum,statement.StatementNum,statement.SuperFamily "; 
			if(orderBy==0){//BillingType
				command+="ORDER BY definition.ItemOrder,LName,FName,MiddleI,PayPlanDue";
			}
			else{
				command+="ORDER BY LName,FName";
			}
			DataTable rawTable=Db.GetTable(command);
			double balTotal;
			double insEst;
			double payPlanDue;
			DateTime lastStatement;
			List<Patient> listFamilyGuarantors;
			foreach(DataRow rawRow in rawTable.Rows) {
				row=table.NewRow();
				if(rawRow["SuperFamily"].ToString()=="0") {//not a super statement, just get bal info from guarantor
					balTotal=PIn.Double(rawRow["BalTotal"].ToString());
					insEst=PIn.Double(rawRow["InsEst"].ToString());
					payPlanDue=PIn.Double(rawRow["PayPlanDue"].ToString());
				}
				else {//super statement, add all guar positive balances to get bal total for super family
					listFamilyGuarantors=Patients.GetSuperFamilyGuarantors(PIn.Long(rawRow["SuperFamily"].ToString())).FindAll(x => x.HasSuperBilling);
					//exclude fams with neg balances in the total for super family stmts (per Nathan 5/25/2016)
					if(PrefC.GetBool(PrefName.BalancesDontSubtractIns)) {
						listFamilyGuarantors=listFamilyGuarantors.FindAll(x => x.BalTotal>0);
						insEst=0;
					}
					else {
						listFamilyGuarantors=listFamilyGuarantors.FindAll(x => (x.BalTotal-x.InsEst)>0);
						insEst=listFamilyGuarantors.Sum(x => x.InsEst);
					}
					balTotal=listFamilyGuarantors.Sum(x => x.BalTotal);
					payPlanDue=listFamilyGuarantors.Sum(x => x.PayPlanDue);
				}
				row["amountDue"]=(balTotal-insEst).ToString("F");
				row["balTotal"]=balTotal.ToString("F");;
				row["billingType"]=DefC.GetName(DefCat.BillingTypes,PIn.Long(rawRow["BillingType"].ToString()));
				if(insEst==0){
					row["insEst"]="";
				}
				else{
					row["insEst"]=insEst.ToString("F");
				}
				row["IsSent"]=rawRow["IsSent"].ToString();
				lastStatement=PIn.Date(rawRow["LastStatement"].ToString());
				if(lastStatement.Year<1880){
					row["lastStatement"]="";
				}
				else{
					row["lastStatement"]=lastStatement.ToShortDateString();
				}
				row["mode"]=Lans.g("enumStatementMode",((StatementMode)PIn.Int(rawRow["Mode_"].ToString())).ToString());
				row["name"]=Patients.GetNameLF(rawRow["LName"].ToString(),rawRow["FName"].ToString(),rawRow["Preferred"].ToString(),rawRow["MiddleI"].ToString());
				row["PatNum"]=rawRow["PatNum"].ToString();
				if(payPlanDue==0){
					row["payPlanDue"]="";
				}
				else{
					row["payPlanDue"]=payPlanDue.ToString("F");
				}
				row["StatementNum"]=rawRow["StatementNum"].ToString();
				row["SuperFamily"]=rawRow["SuperFamily"].ToString();
				table.Rows.Add(row);
			}
			return table;
		}

		///<summary>This query is flawed.</summary>
		public static DataTable GetStatementNotesPracticeWeb(long PatientID) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),PatientID);
			}
			string command=@"SELECT Note FROM Statement Where Patnum="+PatientID;
			return Db.GetTable(command);
		}

		///<summary>This query is flawed.</summary>
		public static Statement GetStatementInfoPracticeWeb(long PatientID) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<Statement>(MethodBase.GetCurrentMethod(),PatientID);
			}
			string command=@"Select SinglePatient,DateRangeFrom,DateRangeTo,Intermingled
                        FROM statement WHERE PatNum = "+PatientID;
			return Crud.StatementCrud.SelectOne(command);
		}

		///<summary>Fetches StatementNums restricted by the DateTStamp, PatNums and a limit of records per patient. If limitPerPatient is zero all StatementNums of a patient are fetched</summary>
		public static List<long> GetChangedSinceStatementNums(DateTime changedSince,List<long> eligibleForUploadPatNumList,int limitPerPatient) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<long>>(MethodBase.GetCurrentMethod(),changedSince,eligibleForUploadPatNumList);
			}
			List<long> statementnums = new List<long>();
			string limitStr="";
			if(limitPerPatient>0) {
				limitStr="LIMIT "+ limitPerPatient;
			}
			DataTable table;
			// there are possibly more efficient ways to implement this using a single sql statement but readability of the sql can be compromised
			if(eligibleForUploadPatNumList.Count>0) {
				for(int i=0;i<eligibleForUploadPatNumList.Count;i++) {
					string command="SELECT StatementNum FROM statement WHERE DateTStamp > "+POut.DateT(changedSince)+" AND PatNum='" 
						+eligibleForUploadPatNumList[i].ToString()+"' ORDER BY DateSent DESC, StatementNum DESC "+limitStr;
					table=Db.GetTable(command);
					for(int j=0;j<table.Rows.Count;j++) {
						statementnums.Add(PIn.Long(table.Rows[j]["StatementNum"].ToString()));
					}
				}
			}
			return statementnums;
		}

		///<summary>Used along with GetChangedSinceStatementNums</summary>
		public static List<Statement> GetMultStatements(List<long> statementNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Statement>>(MethodBase.GetCurrentMethod(),statementNums);
			}
			string strStatementNums="";
			DataTable table;
			if(statementNums.Count>0) {
				for(int i=0;i<statementNums.Count;i++) {
					if(i>0) {
						strStatementNums+="OR ";
					}
					strStatementNums+="StatementNum='"+statementNums[i].ToString()+"' ";
				}
				string command="SELECT * FROM statement WHERE "+strStatementNums;
				table=Db.GetTable(command);
			}
			else {
				table=new DataTable();
			}
			Statement[] multStatements=Crud.StatementCrud.TableToList(table).ToArray();
			List<Statement> statementList=new List<Statement>(multStatements);
			return statementList;
		}

		///<summary>Changes the value of the DateTStamp column to the current time stamp for all statements of a patient</summary>
		public static void ResetTimeStamps(long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),patNum);
				return;
			}
			string command="UPDATE statement SET DateTStamp = CURRENT_TIMESTAMP WHERE PatNum ="+POut.Long(patNum);
			Db.NonQ(command);
		}

		///<summary>Returns an email message for the patient based on the statement passed in.</summary>
		public static EmailMessage GetEmailMessageForStatement(Statement stmt,Patient pat) {
			if(stmt.PatNum!=pat.PatNum) {// && stmt.PatNum!=pat.Guarantor && stmt.PatNum!=pat.SuperFamily) {
				string logMsg = "Mismatched PatNums detected between current patient and current statement:\r\n\r\n"
					+"Statement PatNum: "+stmt.PatNum+" (assumed correct)\r\n"
					+"Patient PatNum: "+pat.PatNum+" (possibly incorrect)";
				SecurityLogs.MakeLogEntry(Permissions.StatementPatNumMismatch,stmt.PatNum,logMsg,LogSources.Diagnostic);
			}
			//No need to check RemotingRole; no call to db.
			EmailMessage message=new EmailMessage();
			message.PatNum=pat.PatNum;
			message.ToAddress=pat.Email;
			message.FromAddress=EmailAddresses.GetByClinic(pat.ClinicNum).SenderAddress;
			string str;
			if(stmt.EmailSubject!=null && stmt.EmailSubject!="") {
				str=stmt.EmailSubject;//Set str to the email subject if one was already set.
			}
			else {//Subject was not set.  Set str to the default billing email subject.
				str=PrefC.GetString(PrefName.BillingEmailSubject);
			}
			message.Subject=Statements.ReplaceVarsForEmail(str,pat);
			if(stmt.EmailBody!=null && stmt.EmailBody!="") {
				str=stmt.EmailBody;//Set str to the email body if one was already set.
			}
			else {//Body was not set.  Set str to the default billing email body text.
				str=PrefC.GetString(PrefName.BillingEmailBodyText);
			}
			message.BodyText=Statements.ReplaceVarsForEmail(str,pat);
			return message;
		}

		///<summary>Email statements allow variables to be present in the message body and subject, this method replaces those variables with the information from the patient passed in.  Simply pass in the string for the subject or body and the corresponding patient.</summary>
		private static string ReplaceVarsForEmail(string str,Patient pat) {
			//No need to check RemotingRole; no call to db.
			str=str.Replace("[monthlyCardsOnFile]",CreditCards.GetMonthlyCardsOnFile(pat.PatNum));
			str=str.Replace("[nameF]",pat.GetNameFirst());
			str=str.Replace("[nameFL]",pat.GetNameFL());
			str=str.Replace("[nameFLnoPref]",pat.GetNameFLnoPref());
			str=str.Replace("[namePref]",pat.Preferred);
			str=str.Replace("[PatNum]",pat.PatNum.ToString());
			str=str.Replace("[currentMonth]",DateTime.Now.ToString("MMMM"));
			return str;
		}




	}


}










