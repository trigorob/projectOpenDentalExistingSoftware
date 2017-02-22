using System;

namespace OpenDentBusiness {
	///<summary>This class will likely never be used.  The famaging table is used to store intermediate calculations for aging and once the patient
	///table is updated the data is never accessed again.  A new aging calculation begins with truncating this table.  All edit commands, i.e. truncate,
	///insert, etc., take place in the queries in Ledgers.ComputeAging.</summary>
	public class FamAgings{
		//If this table type will exist as cached data, uncomment the CachePattern region below and edit.
		/*
		#region CachePattern
		//This region can be eliminated if this is not a table type with cached data.
		//If leaving this region in place, be sure to add RefreshCache and FillCache 
		//to the Cache.cs file with all the other Cache types.
		//Also, make sure to consider making an invalid type for this class in Cache.GetAllCachedInvalidTypes() if needed.

		///<summary>A list of all FamAgings.</summary>
		private static List<FamAging> _list;

		///<summary>A list of all FamAgings.</summary>
		public static List<FamAging> List {
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
			string command="SELECT * FROM famaging ORDER BY ItemOrder";//stub query probably needs to be changed
			DataTable table=Cache.GetTableRemotelyIfNeeded(MethodBase.GetCurrentMethod(),command);
			table.TableName="FamAging";
			FillCache(table);
			return table;
		}

		///<summary></summary>
		public static void FillCache(DataTable table){
			//No need to check RemotingRole; no call to db.
			_list=Crud.FamAgingCrud.TableToList(table);
		}
		#endregion
		*/
		/*
		Only pull out the methods below as you need them.  Otherwise, leave them commented out.

		///<summary></summary>
		public static List<FamAging> Refresh(long patNum){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<FamAging>>(MethodBase.GetCurrentMethod(),patNum);
			}
			string command="SELECT * FROM famaging WHERE PatNum = "+POut.Long(patNum);
			return Crud.FamAgingCrud.SelectMany(command);
		}

		///<summary>Gets one FamAging from the db.</summary>
		public static FamAging GetOne(long patNum){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				return Meth.GetObject<FamAging>(MethodBase.GetCurrentMethod(),patNum);
			}
			return Crud.FamAgingCrud.SelectOne(patNum);
		}

		///<summary></summary>
		public static long Insert(FamAging famAging){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				famAging.PatNum=Meth.GetLong(MethodBase.GetCurrentMethod(),famAging);
				return famAging.PatNum;
			}
			return Crud.FamAgingCrud.Insert(famAging);
		}

		///<summary></summary>
		public static void Update(FamAging famAging){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				Meth.GetVoid(MethodBase.GetCurrentMethod(),famAging);
				return;
			}
			Crud.FamAgingCrud.Update(famAging);
		}

		///<summary></summary>
		public static void Delete(long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),patNum);
				return;
			}
			Crud.FamAgingCrud.Delete(patNum);
		}

		

		
		*/



	}
}