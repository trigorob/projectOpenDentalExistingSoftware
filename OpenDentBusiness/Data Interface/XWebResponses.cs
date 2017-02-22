using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenDentBusiness{
	///<summary></summary>
	public class XWebResponses{
		///<summary>Gets one XWebResponse from the db.</summary>
		public static XWebResponse GetOne(long xWebResponseNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<XWebResponse>(MethodBase.GetCurrentMethod(),xWebResponseNum);
			}
			return Crud.XWebResponseCrud.SelectOne(xWebResponseNum);
		}

		///<summary>Gets the XWeb transactions for approved transactions. To get for all clinics, pass in a list of empty clinicNums.</summary>
		public static DataTable GetApprovedTransactions(List<long> listClinicNums,DateTime dateFrom,DateTime dateTo) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),listClinicNums,dateFrom,dateTo);
			}
			string command="SELECT "+DbHelper.Concat("patient.LName","', '","patient.FName")+" Patient,xwebresponse.DateTUpdate,xwebresponse.TransactionID,"
				+"xwebresponse.MaskedAcctNum,xwebresponse.ExpDate,xwebresponse.Amount,xwebresponse.PaymentNum,xwebresponse.TransactionStatus,"
				+"(CASE WHEN payment.PayNum IS NULL THEN 0 ELSE 1 END) doesPaymentExist,COALESCE(clinic.Abbr,'Unassigned') Clinic,xwebresponse.PatNum, "
				+"xwebresponse.XWebResponseNum,xwebresponse.Alias "
				+"FROM xwebresponse "
				+"INNER JOIN patient ON patient.PatNum=xwebresponse.PatNum "
				+"LEFT JOIN payment ON payment.PayNum=xwebresponse.PaymentNum "
				+"LEFT JOIN clinic ON clinic.ClinicNum=xwebresponse.ClinicNum "
				+"WHERE xwebresponse.TransactionStatus IN("
				+POut.Int((int)XWebTransactionStatus.DtgPaymentApproved)+","
				+POut.Int((int)XWebTransactionStatus.HpfCompletePaymentApproved)+","
				+POut.Int((int)XWebTransactionStatus.HpfCompletePaymentApprovedPartial)+","
				+POut.Int((int)XWebTransactionStatus.DtgPaymentReturned)+","
				+POut.Int((int)XWebTransactionStatus.DtgPaymentVoided)+") "
				+"AND xwebresponse.ResponseCode IN("
				+POut.Int((int)XWebResponseCodes.Approval)+","
				+POut.Int((int)XWebResponseCodes.PartialApproval)+") "
				+"AND xwebresponse.DateTUpdate BETWEEN "+POut.DateT(dateFrom)+" AND "+POut.DateT(dateTo.AddDays(1))+" ";
			if(listClinicNums.Count>0) {
				command+="AND xwebresponse.ClinicNum IN ("+string.Join(",",listClinicNums.Select(x => POut.Long(x)))+") ";
			}
			command+="ORDER BY xwebresponse.DateTUpdate,patient.LName,patient.FName ";
			return Db.GetTable(command);
		}

		///<summary>Gets the XWebResponse that is associated with this payNum. Returns null if the XWebResponse does not exist.</summary>
		public static XWebResponse GetOneByPaymentNum(long payNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<XWebResponse>(MethodBase.GetCurrentMethod(),payNum);
			}
			string command="SELECT * FROM xwebresponse WHERE PaymentNum="+POut.Long(payNum);
			return Crud.XWebResponseCrud.SelectOne(command);
		}

		///<summary>Gets all XWebResponses where TransactionStatus==XWebTransactionStatus.HpfPending from the db.</summary>
		public static List<XWebResponse> GetPendingHPFs() {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<XWebResponse>>(MethodBase.GetCurrentMethod());
			}
			return Crud.XWebResponseCrud.SelectMany("SELECT * FROM xwebresponse "
				+"WHERE TransactionStatus = "+POut.Int((int)XWebTransactionStatus.HpfPending)+" "
				+"AND (TransactionType = '"+POut.String(XWebTransactionType.AliasCreateTransaction.ToString())+"' OR TransactionType = '"+POut.String(XWebTransactionType.CreditSaleTransaction.ToString())+"')");
		}
		
		///<summary></summary>
		public static long Insert(XWebResponse xWebResponse) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				xWebResponse.XWebResponseNum=Meth.GetLong(MethodBase.GetCurrentMethod(),xWebResponse);
				return xWebResponse.XWebResponseNum;
			}
			return Crud.XWebResponseCrud.Insert(xWebResponse);
		}

		///<summary></summary>
		public static void Update(XWebResponse xWebResponse) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),xWebResponse);
				return;
			}
			Crud.XWebResponseCrud.Update(xWebResponse);
		}

		//If this table type will exist as cached data, uncomment the CachePattern region below and edit.
		/*
		#region CachePattern
		//This region can be eliminated if this is not a table type with cached data.
		//If leaving this region in place, be sure to add RefreshCache and FillCache 
		//to the Cache.cs file with all the other Cache types.

		///<summary>A list of all XWebResponses.</summary>
		private static List<XWebResponse> listt;

		///<summary>A list of all XWebResponses.</summary>
		public static List<XWebResponse> Listt{
			get {
				if(listt==null) {
					RefreshCache();
				}
				return listt;
			}
			set {
				listt=value;
			}
		}

		///<summary></summary>
		public static DataTable RefreshCache(){
			//No need to check RemotingRole; Calls GetTableRemotelyIfNeeded().
			string command="SELECT * FROM xwebresponse ORDER BY ItemOrder";//stub query probably needs to be changed
			DataTable table=Cache.GetTableRemotelyIfNeeded(MethodBase.GetCurrentMethod(),command);
			table.TableName="XWebResponse";
			FillCache(table);
			return table;
		}

		///<summary></summary>
		public static void FillCache(DataTable table){
			//No need to check RemotingRole; no call to db.
			listt=Crud.XWebResponseCrud.TableToList(table);
		}
		#endregion
		*/
		/*
		Only pull out the methods below as you need them.  Otherwise, leave them commented out.

		///<summary></summary>
		public static List<XWebResponse> Refresh(long patNum){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<XWebResponse>>(MethodBase.GetCurrentMethod(),patNum);
			}
			string command="SELECT * FROM xwebresponse WHERE PatNum = "+POut.Long(patNum);
			return Crud.XWebResponseCrud.SelectMany(command);
		}

		///<summary></summary>
		public static void Delete(long xWebResponseNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),xWebResponseNum);
				return;
			}
			Crud.XWebResponseCrud.Delete(xWebResponseNum);
		}

		

		
		*/



	}
}