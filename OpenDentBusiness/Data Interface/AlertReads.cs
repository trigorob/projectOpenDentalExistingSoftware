using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace OpenDentBusiness{
	///<summary></summary>
	public class AlertReads{
		//If this table type will exist as cached data, uncomment the CachePattern region below and edit.
		/*
		#region CachePattern
		//This region can be eliminated if this is not a table type with cached data.
		//If leaving this region in place, be sure to add RefreshCache and FillCache 
		//to the Cache.cs file with all the other Cache types.
		//Also, make sure to consider making an invalid type for this class in Cache.GetAllCachedInvalidTypes() if needed.

		///<summary>A list of all AlertReads.</summary>
		private static List<AlertRead> _list;

		///<summary>A list of all AlertReads.</summary>
		public static List<AlertRead> List {
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
			string command="SELECT * FROM alertread ORDER BY ItemOrder";//stub query probably needs to be changed
			DataTable table=Cache.GetTableRemotelyIfNeeded(MethodBase.GetCurrentMethod(),command);
			table.TableName="AlertRead";
			FillCache(table);
			return table;
		}

		///<summary></summary>
		public static void FillCache(DataTable table){
			//No need to check RemotingRole; no call to db.
			_list=Crud.AlertReadCrud.TableToList(table);
		}
		#endregion
		*/

		///<summary></summary>
		public static List<AlertRead> Refresh(long patNum){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<AlertRead>>(MethodBase.GetCurrentMethod(),patNum);
			}
			string command="SELECT * FROM alertread WHERE UserNum = "+POut.Long(patNum);
			return Crud.AlertReadCrud.SelectMany(command);
		}

		///<summary></summary>
		public static List<AlertRead> RefreshForAlertNums(long patNum,List<long> listAlertItemNums){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<AlertRead>>(MethodBase.GetCurrentMethod(),patNum,listAlertItemNums);
			}
			if(listAlertItemNums==null || listAlertItemNums.Count==0) {
				return new List<AlertRead>();
			}
			string command="SELECT * FROM alertread WHERE UserNum = "+POut.Long(patNum)+ " ";
			command+="AND  AlertItemNum IN ("+String.Join(",",listAlertItemNums)+")";
			return Crud.AlertReadCrud.SelectMany(command);
		}

		///<summary>Gets one AlertRead from the db.</summary>
		public static AlertRead GetOne(long alertReadNum){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				return Meth.GetObject<AlertRead>(MethodBase.GetCurrentMethod(),alertReadNum);
			}
			return Crud.AlertReadCrud.SelectOne(alertReadNum);
		}

		///<summary></summary>
		public static long Insert(AlertRead alertRead){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				alertRead.AlertReadNum=Meth.GetLong(MethodBase.GetCurrentMethod(),alertRead);
				return alertRead.AlertReadNum;
			}
			return Crud.AlertReadCrud.Insert(alertRead);
		}

		///<summary></summary>
		public static void Update(AlertRead alertRead){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				Meth.GetVoid(MethodBase.GetCurrentMethod(),alertRead);
				return;
			}
			Crud.AlertReadCrud.Update(alertRead);
		}

		///<summary></summary>
		public static void Delete(long alertReadNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),alertReadNum);
				return;
			}
			Crud.AlertReadCrud.Delete(alertReadNum);
		}

		///<summary></summary>
		public static void DeleteForAlertItem(long alertItemNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),alertItemNum);
				return;
			}
			string command="DELETE FROM alertread "
				+"WHERE AlertItemNum = "+POut.Long(alertItemNum);
			Db.NonQ(command);
		}

		///<summary>Inserts, updates, or deletes db rows to match listNew.  No need to pass in userNum, it's set before remoting role check and passed to
		///the server if necessary.  Doesn't create ApptComm items, but will delete them.  If you use Sync, you must create new Apptcomm items.</summary>
		public static bool Sync(List<AlertRead> listNew,List<AlertRead> listOld) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod(),listNew,listOld);
			}
			return Crud.AlertReadCrud.Sync(listNew,listOld);
		}

		



	}
}