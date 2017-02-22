using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Linq;

namespace OpenDentBusiness{
	///<summary></summary>
	public class AlertItems{
		//If this table type will exist as cached data, uncomment the CachePattern region below and edit.
		/*
		#region CachePattern
		//This region can be eliminated if this is not a table type with cached data.
		//If leaving this region in place, be sure to add RefreshCache and FillCache 
		//to the Cache.cs file with all the other Cache types.
		//Also, make sure to consider making an invalid type for this class in Cache.GetAllCachedInvalidTypes() if needed.

		///<summary>A list of all AlertItems.</summary>
		private static List<AlertItem> _list;

		///<summary>A list of all AlertItems.</summary>
		public static List<AlertItem> List {
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
			string command="SELECT * FROM alertitem ORDER BY ItemOrder";//stub query probably needs to be changed
			DataTable table=Cache.GetTableRemotelyIfNeeded(MethodBase.GetCurrentMethod(),command);
			table.TableName="AlertItem";
			FillCache(table);
			return table;
		}

		///<summary></summary>
		public static void FillCache(DataTable table){
			//No need to check RemotingRole; no call to db.
			_list=Crud.AlertItemCrud.TableToList(table);
		}
		#endregion
		*/

		///<summary></summary>
		public static List<AlertItem> Refresh(long patNum){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<AlertItem>>(MethodBase.GetCurrentMethod(),patNum);
			}
			string command="SELECT * FROM alertitem WHERE PatNum = "+POut.Long(patNum);
			return Crud.AlertItemCrud.SelectMany(command);
		}

		///<summary></summary>
		public static List<AlertItem> RefreshAll(){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<AlertItem>>(MethodBase.GetCurrentMethod());
			}
			string command="SELECT * FROM alertitem";
			return Crud.AlertItemCrud.SelectMany(command);
		}

		///<summary>Returns a list of AlertItems for the given clinicNum.</summary>
		public static List<AlertItem> RefreshForClinicAndTypes(long clinicNum,List<AlertType> listAlertTypes=null){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<AlertItem>>(MethodBase.GetCurrentMethod(),clinicNum,listAlertTypes);
			}
			if(listAlertTypes==null || listAlertTypes.Count==0) {
				return new List<AlertItem>();
			}
			string command="SELECT * FROM alertitem WHERE ClinicNum = "+POut.Long(clinicNum)+" ";
			command+="AND Type IN ("+String.Join(",",listAlertTypes.Cast<int>().ToList())+")";
			return Crud.AlertItemCrud.SelectMany(command);
		}

		///<summary>Returns a list of AlertItems for the given cinicNum.</summary>
		public static List<AlertItem> RefreshForType(AlertType alertType){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<AlertItem>>(MethodBase.GetCurrentMethod(),alertType);
			}
			string command="SELECT * FROM alertitem WHERE Type = "+POut.Int((int)alertType)+" ";
			return Crud.AlertItemCrud.SelectMany(command);
		}

		///<summary>Gets one AlertItem from the db.</summary>
		public static AlertItem GetOne(long alertItemNum){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				return Meth.GetObject<AlertItem>(MethodBase.GetCurrentMethod(),alertItemNum);
			}
			return Crud.AlertItemCrud.SelectOne(alertItemNum);
		}

		///<summary></summary>
		public static long Insert(AlertItem alertItem){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				alertItem.AlertItemNum=Meth.GetLong(MethodBase.GetCurrentMethod(),alertItem);
				return alertItem.AlertItemNum;
			}
			return Crud.AlertItemCrud.Insert(alertItem);
		}

		///<summary></summary>
		public static void Update(AlertItem alertItem){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				Meth.GetVoid(MethodBase.GetCurrentMethod(),alertItem);
				return;
			}
			Crud.AlertItemCrud.Update(alertItem);
		}

		///<summary></summary>
		public static void Delete(long alertItemNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),alertItemNum);
				return;
			}
			Crud.AlertItemCrud.Delete(alertItemNum);
		}

		///<summary>Inserts, updates, or deletes db rows to match listNew.  No need to pass in userNum, it's set before remoting role check and passed to
		///the server if necessary.  Doesn't create ApptComm items, but will delete them.  If you use Sync, you must create new Apptcomm items.</summary>
		public static void Sync(List<AlertItem> listNew,List<AlertItem> listOld) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),listNew,listOld);
				return;
			}
			Crud.AlertItemCrud.Sync(listNew,listOld);
		}
		
	}
}