using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace OpenDentBusiness {
	///<summary></summary>
	public class PatRestrictions{
		//If this table type will exist as cached data, uncomment the CachePattern region below and edit.
		/*
		#region CachePattern
		//This region can be eliminated if this is not a table type with cached data.
		//If leaving this region in place, be sure to add RefreshCache and FillCache 
		//to the Cache.cs file with all the other Cache types.
		//Also, make sure to consider making an invalid type for this class in Cache.GetAllCachedInvalidTypes() if needed.

		///<summary>A list of all PatRestrictions.</summary>
		private static List<PatRestriction> _list;

		///<summary>A list of all PatRestrictions.</summary>
		public static List<PatRestriction> List {
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
			string command="SELECT * FROM patrestriction ORDER BY ItemOrder";//stub query probably needs to be changed
			DataTable table=Cache.GetTableRemotelyIfNeeded(MethodBase.GetCurrentMethod(),command);
			table.TableName="PatRestriction";
			FillCache(table);
			return table;
		}

		///<summary></summary>
		public static void FillCache(DataTable table){
			//No need to check RemotingRole; no call to db.
			_list=Crud.PatRestrictionCrud.TableToList(table);
		}
		#endregion
		*/

		///<summary>Gets all patrestrictions for the specified patient.</summary>
		public static List<PatRestriction> GetAllForPat(long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<PatRestriction>>(MethodBase.GetCurrentMethod(),patNum);
			}
			string command="SELECT * FROM patrestriction WHERE PatNum="+POut.Long(patNum);
			return Crud.PatRestrictionCrud.SelectMany(command);
		}

		///<summary>This will only insert a new PatRestriction if there is not already an existing PatRestriction in the db for this patient and type.
		///If exists, returns the PatRestrictionNum of the first one found.  Otherwise returns the PatRestrictionNum of the newly inserted one.</summary>
		public static long Insert(long patNum,PatRestrict patRestrictType) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				return Meth.GetLong(MethodBase.GetCurrentMethod(),patNum,patRestrictType);
			}
			List<PatRestriction> listPatRestricts=GetAllForPat(patNum).FindAll(x => x.PatRestrictType==patRestrictType);
			if(listPatRestricts.Count>0) {
				return listPatRestricts[0].PatRestrictionNum;
			}
			return Crud.PatRestrictionCrud.Insert(new PatRestriction() { PatNum=patNum,PatRestrictType=patRestrictType });
		}

		///<summary>Checks for an existing patrestriction for the specified patient and PatRestrictType.
		///If one exists, returns true (IsRestricted).  If none exist, returns false (!IsRestricted).
		///If suppressMessage is omitted or is set to false, a message box will display the patrestriction blocking the action.</summary>
		public static bool IsRestricted(long patNum,PatRestrict patRestrictType,bool suppressMessage=false) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				return Meth.GetBool(MethodBase.GetCurrentMethod(),patNum,patRestrictType,suppressMessage);
			}
			string command="SELECT COUNT(*) FROM patrestriction WHERE PatNum="+POut.Long(patNum)+" AND PatRestrictType="+POut.Int((int)patRestrictType);
			if(PIn.Int(Db.GetCount(command))>0) {
				if(!suppressMessage) {
					MessageBox.Show(Lans.g("PatRestrictions","Not allowed due to patient restriction")+"\r\n"+GetPatRestrictDesc(patRestrictType));
				}
				return true;
			}
			else {
				return false;
			}
		}

		///<summary>Gets the human readable description of the patrestriction, passed through Lans.g.</summary>
		public static string GetPatRestrictDesc(PatRestrict patRestrictType) {
			switch(patRestrictType) {
				case PatRestrict.ApptSchedule:
					return Lans.g("patRestrictEnum","Appointment Scheduling");
				case PatRestrict.None:
				default:
					return "";
			}
		}

		///<summary>Deletes any patrestrictions for the specified patient and type.</summary>
		public static void RemovePatRestriction(long patNum,PatRestrict patRestrictType) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				Meth.GetVoid(MethodBase.GetCurrentMethod(),patNum,patRestrictType);
				return;
			}
			string command="DELETE FROM patrestriction WHERE PatNum="+POut.Long(patNum)+" AND PatRestrictType="+POut.Int((int)patRestrictType);
			Db.NonQ(command);
			return;
		}

		//Only pull out the methods below as you need them.  Otherwise, leave them commented out.
		/*
		///<summary>Gets one PatRestriction from the db.</summary>
		public static PatRestriction GetOne(long patRestrictionNum){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				return Meth.GetObject<PatRestriction>(MethodBase.GetCurrentMethod(),patRestrictionNum);
			}
			return Crud.PatRestrictionCrud.SelectOne(patRestrictionNum);
		}

		///<summary></summary>
		public static void Update(PatRestrict patRestrict){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				Meth.GetVoid(MethodBase.GetCurrentMethod(),patRestrict);
				return;
			}
			Crud.PatRestrictionCrud.Update(patRestrict);
		}

		///<summary></summary>
		public static void Delete(long patRestrictionNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),patRestrictionNum);
				return;
			}
			Crud.PatRestrictionCrud.Delete(patRestrictionNum);
		}		
		*/



	}
}