using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Reflection;

namespace OpenDentBusiness{
	///<summary></summary>
	public class ProcButtonItems {
		///<summary>All procbuttonitems for all buttons.</summary>
		private static ProcButtonItem[] list;

		public static ProcButtonItem[] List {
			//No need to check RemotingRole; no call to db.
			get {
				if(list==null) {
					RefreshCache();
				}
				return list;
			}
			set {
				list=value;
			}
		}

		public static DataTable RefreshCache() {
			//No need to check RemotingRole; Calls GetTableRemotelyIfNeeded().
			string command="SELECT * FROM procbuttonitem ORDER BY ItemOrder";
			DataTable table=Cache.GetTableRemotelyIfNeeded(MethodBase.GetCurrentMethod(),command);
			table.TableName="ProcButtonItem";
			FillCache(table);
			return table;
		}

		///<summary></summary>
		public static void FillCache(DataTable table) {
			//No need to check RemotingRole; no call to db.
			list=Crud.ProcButtonItemCrud.TableToList(table).ToArray();
		}

		///<summary>Must have already checked procCode for nonduplicate.</summary>
		public static long Insert(ProcButtonItem item) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				item.ProcButtonItemNum=Meth.GetLong(MethodBase.GetCurrentMethod(),item);
				return item.ProcButtonItemNum;
			}
			return Crud.ProcButtonItemCrud.Insert(item);
		}

		///<summary></summary>
		public static void Update(ProcButtonItem item) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),item);
				return;
			}
			Crud.ProcButtonItemCrud.Update(item);
		}

		///<summary></summary>
		public static void Delete(ProcButtonItem item) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),item);
				return;
			}
			string command="DELETE FROM procbuttonitem WHERE ProcButtonItemNum = '"+POut.Long(item.ProcButtonItemNum)+"'";
			Db.NonQ(command);
		}

		///<summary>Sorted by Item Order.</summary>
		public static long[] GetCodeNumListForButton(long procButtonNum) {
			return List.ToList().FindAll(x => x.ProcButtonNum==procButtonNum && x.CodeNum>0).OrderBy(x => x.ItemOrder).Select(x => x.CodeNum).ToArray();
		}

		///<summary>Sorted by Item Order.</summary>
		public static long[] GetAutoListForButton(long procButtonNum) {
			return List.ToList().FindAll(x => x.ProcButtonNum==procButtonNum && x.AutoCodeNum>0)
				.OrderBy(x => x.ItemOrder)
				.Select(x => x.AutoCodeNum).ToArray();
		}

		///<summary></summary>
		public static void DeleteAllForButton(long procButtonNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),procButtonNum);
				return;
			}
			string command= "DELETE from procbuttonitem WHERE procbuttonnum = '"+POut.Long(procButtonNum)+"'";
			Db.NonQ(command);
		}

	}

	




}










