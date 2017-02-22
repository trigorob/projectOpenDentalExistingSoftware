using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using CodeBase;
using Newtonsoft.Json;

namespace OpenDentBusiness{
	///<summary></summary>
	public class ConfirmationRequests{

		///<summary>Get all rows where RSVPStatus==AwaitingTransmit.</summary>
		public static List<ConfirmationRequest> GetAllOutstandingForSend() {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<ConfirmationRequest>>(MethodBase.GetCurrentMethod());
			}
			string command="SELECT * FROM confirmationrequest WHERE RSVPStatus = "+POut.Int((int)RSVPStatusCodes.AwaitingTransmit);
			return Crud.ConfirmationRequestCrud.SelectMany(command);
		}

		///<summary>HQ only knows about the ShortGUID field. It treats both ShortGUIDEmail and ShortGuid as ShortGuid. 
		///Returns any client side ConfirmationRequest(s) where either ShortGUIDEmail or ShortGuid matches the server side ShortGuid field.</summary>
		public static List<ConfirmationRequest> GetConfirmationsByShortGuid(string shortGuid) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<ConfirmationRequest>>(MethodBase.GetCurrentMethod(),shortGuid);
			}
			string command="SELECT * FROM confirmationrequest WHERE ShortGUID = '"+POut.String(shortGuid)+"' OR ShortGuidEmail = '"+POut.String(shortGuid)+"'";
			return Crud.ConfirmationRequestCrud.SelectMany(command);
		}

		public static ConfirmationRequest GetConfirmationByGuidMessageToMobile(string guidMessageToMobile) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<ConfirmationRequest>(MethodBase.GetCurrentMethod(),guidMessageToMobile);
			}
			string command="SELECT * FROM confirmationrequest WHERE GuidMessageToMobile = '"+POut.String(guidMessageToMobile)+"'";
			return Crud.ConfirmationRequestCrud.SelectOne(command);
		}
		
		public static long Insert(ConfirmationRequest confirmationRequest) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				confirmationRequest.ConfirmationRequestNum=Meth.GetLong(MethodBase.GetCurrentMethod(),confirmationRequest);
				return confirmationRequest.ConfirmationRequestNum;
			}
			return Crud.ConfirmationRequestCrud.Insert(confirmationRequest);
		}

		public static void Update(ConfirmationRequest confirmationRequest) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),confirmationRequest);
				return;
			}
			Crud.ConfirmationRequestCrud.Update(confirmationRequest);
		}

		//If this table type will exist as cached data, uncomment the CachePattern region below and edit.
		/*
		#region CachePattern
		//This region can be eliminated if this is not a table type with cached data.
		//If leaving this region in place, be sure to add RefreshCache and FillCache 
		//to the Cache.cs file with all the other Cache types.
		//Also, make sure to consider making an invalid type for this class in Cache.GetAllCachedInvalidTypes() if needed.

		///<summary>A list of all ConfirmationRequests.</summary>
		private static List<ConfirmationRequest> _list;

		///<summary>A list of all ConfirmationRequests.</summary>
		public static List<ConfirmationRequest> List {
			get {
				if(_list==null) {
					RefreshCache();
				}
				return _list;
			}
			set {
				_list=value;
			}
		}

		///<summary></summary>
		public static DataTable RefreshCache(){
			//No need to check RemotingRole; Calls GetTableRemotelyIfNeeded().
			string command="SELECT * FROM confirmationrequest ORDER BY ItemOrder";//stub query probably needs to be changed
			DataTable table=Cache.GetTableRemotelyIfNeeded(MethodBase.GetCurrentMethod(),command);
			table.TableName="ConfirmationRequest";
			FillCache(table);
			return table;
		}

		///<summary></summary>
		public static void FillCache(DataTable table){
			//No need to check RemotingRole; no call to db.
			_list=Crud.ConfirmationRequestCrud.TableToList(table);
		}
		#endregion
		*/
		/*
		Only pull out the methods below as you need them.  Otherwise, leave them commented out.

		///<summary></summary>
		public static List<ConfirmationRequest> Refresh(long patNum){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<ConfirmationRequest>>(MethodBase.GetCurrentMethod(),patNum);
			}
			string command="SELECT * FROM confirmationrequest WHERE PatNum = "+POut.Long(patNum);
			return Crud.ConfirmationRequestCrud.SelectMany(command);
		}

		///<summary>Gets one ConfirmationRequest from the db.</summary>
		public static ConfirmationRequest GetOne(long confirmationRequestNum){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				return Meth.GetObject<ConfirmationRequest>(MethodBase.GetCurrentMethod(),confirmationRequestNum);
			}
			return Crud.ConfirmationRequestCrud.SelectOne(confirmationRequestNum);
		}

		///<summary></summary>
		public static void Delete(long confirmationRequestNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),confirmationRequestNum);
				return;
			}
			Crud.ConfirmationRequestCrud.Delete(confirmationRequestNum);
		}

		

		
		*/



	}
}