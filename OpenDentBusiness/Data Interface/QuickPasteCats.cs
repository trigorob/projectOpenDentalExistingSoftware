using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Linq;

namespace OpenDentBusiness{
	///<summary></summary>
	public class QuickPasteCats {
		///<summary></summary>
		private static List<QuickPasteCat> list;

		public static List<QuickPasteCat> List {
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
			string command=
				"SELECT * from quickpastecat "
				+"ORDER BY ItemOrder";
			DataTable table=Cache.GetTableRemotelyIfNeeded(MethodBase.GetCurrentMethod(),command);
			table.TableName="QuickPasteCat";
			FillCache(table);
			return table;
		}

		///<summary></summary>
		public static void FillCache(DataTable table) {
			//No need to check RemotingRole; no call to db.
			list=Crud.QuickPasteCatCrud.TableToList(table);
		}

		///<summary></summary>
		public static long Insert(QuickPasteCat cat) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				cat.QuickPasteCatNum=Meth.GetLong(MethodBase.GetCurrentMethod(),cat);
				return cat.QuickPasteCatNum;
			}
			return Crud.QuickPasteCatCrud.Insert(cat);
		}

		///<summary></summary>
		public static void Update(QuickPasteCat cat){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),cat);
				return;
			}
			Crud.QuickPasteCatCrud.Update(cat);
		}

		///<summary></summary>
		public static void Delete(QuickPasteCat cat){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),cat);
				return;
			}
			string command="DELETE from quickpastecat WHERE QuickPasteCatNum = '"
				+POut.Long(cat.QuickPasteCatNum)+"'";
 			Db.NonQ(command);
		}

		///<summary>Gets all categories.</summary>
		public static List<QuickPasteCat> GetAll() {
			return List;
		}

		///<summary>Called from FormQuickPaste and from QuickPasteNotes.Substitute(). Returns the index of the default category for the specified type. If user has entered more than one, only one is returned.</summary>
		public static int GetDefaultType(QuickPasteType type){
			//No need to check RemotingRole; no call to db.
			if(List.Count==0){
				return -1;
			}
			if(type==QuickPasteType.None){
				return 0;//default to first line
			}
			string[] types;
			for(int i=0;i<List.Count;i++){
				if(List[i].DefaultForTypes==""){
					types=new string[0];
				}
				else{
					types=List[i].DefaultForTypes.Split(',');
				}
				for(int j=0;j<types.Length;j++){
					if(((int)type).ToString()==types[j]){
						return i;
					}
				}
			}
			return 0;
		}
		
		///<summary>This should not be passing in two lists. Consider rewriting to only pass in one list and an identifier to get list from DB.</summary>
		public static bool Sync(List<QuickPasteCat> listNew,List<QuickPasteCat> listOld) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod(),listNew,listOld);
			}
			return Crud.QuickPasteCatCrud.Sync(listNew.Select(x=>x.Copy()).ToList(),listOld.Select(x=>x.Copy()).ToList());
		}

		

		


		


	}

	


}









