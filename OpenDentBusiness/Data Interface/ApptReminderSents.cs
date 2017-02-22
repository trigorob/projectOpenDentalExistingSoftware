using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace OpenDentBusiness{
	///<summary></summary>
	public class ApptReminderSents{
		//If this table type will exist as cached data, uncomment the CachePattern region below and edit.
		/*
		#region CachePattern
		//This region can be eliminated if this is not a table type with cached data.
		//If leaving this region in place, be sure to add RefreshCache and FillCache 
		//to the Cache.cs file with all the other Cache types.
		//Also, make sure to consider making an invalid type for this class in Cache.GetAllCachedInvalidTypes() if needed.

		///<summary>A list of all ApptReminderSents.</summary>
		private static List<ApptReminderSent> _list;

		///<summary>A list of all ApptReminderSents.</summary>
		public static List<ApptReminderSent> List {
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
			string command="SELECT * FROM apptremindersent ORDER BY ItemOrder";//stub query probably needs to be changed
			DataTable table=Cache.GetTableRemotelyIfNeeded(MethodBase.GetCurrentMethod(),command);
			table.TableName="ApptReminderSent";
			FillCache(table);
			return table;
		}

		///<summary></summary>
		public static void FillCache(DataTable table){
			//No need to check RemotingRole; no call to db.
			_list=Crud.ApptReminderSentCrud.TableToList(table);
		}
		#endregion
		*/
		/*
		Only pull out the methods below as you need them.  Otherwise, leave them commented out.

		///<summary></summary>
		public static List<ApptReminderSent> Refresh(long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<ApptReminderSent>>(MethodBase.GetCurrentMethod(),patNum);
			}
			string command = "SELECT * FROM apptremindersent WHERE PatNum = "+POut.Long(patNum);
			return Crud.ApptReminderSentCrud.SelectMany(command);
		}

		///<summary>Gets one ApptReminderSent from the db.</summary>
		public static ApptReminderSent GetOne(long apptReminderSentNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<ApptReminderSent>(MethodBase.GetCurrentMethod(),apptReminderSentNum);
			}
			return Crud.ApptReminderSentCrud.SelectOne(apptReminderSentNum);
		}

		

		
		*/

		///<summary></summary>
		public static long Insert(ApptReminderSent apptReminderSent) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				apptReminderSent.ApptReminderSentNum=Meth.GetLong(MethodBase.GetCurrentMethod(),apptReminderSent);
				return apptReminderSent.ApptReminderSentNum;
			}
			return Crud.ApptReminderSentCrud.Insert(apptReminderSent);
		}

		///<summary></summary>
		public static void Update(ApptReminderSent apptReminderSent) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),apptReminderSent);
				return;
			}
			Crud.ApptReminderSentCrud.Update(apptReminderSent);
		}

		///<summary></summary>
		public static void Delete(long apptReminderSentNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),apptReminderSentNum);
				return;
			}
			Crud.ApptReminderSentCrud.Delete(apptReminderSentNum);
		}


	}
}