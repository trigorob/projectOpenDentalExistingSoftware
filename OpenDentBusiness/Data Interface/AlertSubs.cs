using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace OpenDentBusiness{
	///<summary></summary>
	public class AlertSubs{

		#region CachePattern
		//This region can be eliminated if this is not a table type with cached data.
		//If leaving this region in place, be sure to add RefreshCache and FillCache 
		//to the Cache.cs file with all the other Cache types.
		//Also, make sure to consider making an invalid type for this class in Cache.GetAllCachedInvalidTypes() if needed.

		///<summary>A list of all AlertSubs.</summary>
		private static List<AlertSub> _list;

		///<summary>A list of all AlertSubs.</summary>
		public static List<AlertSub> List {
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
			string command="SELECT * FROM alertsub";//stub query probably needs to be changed
			DataTable table=Cache.GetTableRemotelyIfNeeded(MethodBase.GetCurrentMethod(),command);
			table.TableName="AlertSub";
			FillCache(table);
			return table;
		}

		///<summary></summary>
		public static void FillCache(DataTable table){
			//No need to check RemotingRole; no call to db.
			_list=Crud.AlertSubCrud.TableToList(table);
		}
		#endregion
		
		///<summary></summary>
		public static List<AlertSub> Refresh(long patNum){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<AlertSub>>(MethodBase.GetCurrentMethod(),patNum);
			}
			string command="SELECT * FROM alertsub WHERE PatNum = "+POut.Long(patNum);
			return Crud.AlertSubCrud.SelectMany(command);
		}

		///<summary>Gets one AlertSub from the db.</summary>
		public static AlertSub GetOne(long alertSubNum){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				return Meth.GetObject<AlertSub>(MethodBase.GetCurrentMethod(),alertSubNum);
			}
			return Crud.AlertSubCrud.SelectOne(alertSubNum);
		}

		///<summary></summary>
		public static long Insert(AlertSub alertSub){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				alertSub.AlertSubNum=Meth.GetLong(MethodBase.GetCurrentMethod(),alertSub);
				return alertSub.AlertSubNum;
			}
			return Crud.AlertSubCrud.Insert(alertSub);
		}

		///<summary></summary>
		public static void Update(AlertSub alertSub){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				Meth.GetVoid(MethodBase.GetCurrentMethod(),alertSub);
				return;
			}
			Crud.AlertSubCrud.Update(alertSub);
		}

		///<summary></summary>
		public static void Delete(long alertSubNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),alertSubNum);
				return;
			}
			Crud.AlertSubCrud.Delete(alertSubNum);
		}

		///<summary>Inserts, updates, or deletes db rows to match listNew.  No need to pass in userNum, it's set before remoting role check and passed to
		///the server if necessary.  Doesn't create ApptComm items, but will delete them.  If you use Sync, you must create new Apptcomm items.</summary>
		public static bool Sync(List<AlertSub> listNew,List<AlertSub> listOld) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod(),listNew,listOld);
			}
			return Crud.AlertSubCrud.Sync(listNew,listOld);
		}

	}
}