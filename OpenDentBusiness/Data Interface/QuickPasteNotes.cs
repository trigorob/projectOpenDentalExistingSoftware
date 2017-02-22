using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq;

namespace OpenDentBusiness{
	///<summary></summary>
	public class QuickPasteNotes {
		///<summary>list of all notes for all categories. Not very useful.</summary>
		private static List<QuickPasteNote> List;

		public static DataTable RefreshCache() {
			//No need to check RemotingRole; Calls GetTableRemotelyIfNeeded().
			string command=
				"SELECT * from quickpastenote "
				+"ORDER BY ItemOrder";
			DataTable table=Cache.GetTableRemotelyIfNeeded(MethodBase.GetCurrentMethod(),command);
			table.TableName="QuickPasteNote";
			FillCache(table);
			return table;
		}

		///<summary></summary>
		public static void FillCache(DataTable table) {
			//No need to check RemotingRole; no call to db.
			List=Crud.QuickPasteNoteCrud.TableToList(table);
		}

		///<summary></summary>
		public static long Insert(QuickPasteNote note) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				note.QuickPasteNoteNum=Meth.GetLong(MethodBase.GetCurrentMethod(),note);
				return note.QuickPasteNoteNum;
			}
			return Crud.QuickPasteNoteCrud.Insert(note);
		}

		///<summary></summary>
		public static void Update(QuickPasteNote note){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),note);
				return;
			}
			Crud.QuickPasteNoteCrud.Update(note);
		}
		
		///<summary></summary>
		public static void Delete(QuickPasteNote note){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),note);
				return;
			}
			string command="DELETE from quickpastenote WHERE QuickPasteNoteNum = '"
				+POut.Long(note.QuickPasteNoteNum)+"'";
 			Db.NonQ(command);
		}

		///<summary>When saving an abbrev, this makes sure that the abbreviation is not already in use.</summary>
		public static bool AbbrAlreadyInUse(QuickPasteNote note){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod(),note);
			}
			string command="SELECT * FROM quickpastenote WHERE "
				+"Abbreviation='"+POut.String(note.Abbreviation)+"' "
				+"AND QuickPasteNoteNum != '"+POut.Long (note.QuickPasteNoteNum)+"'";
 			DataTable table=Db.GetTable(command);
			if(table.Rows.Count==0){
				return false;
			}
			return true;
		}

		///<summary>Only used from FormQuickPaste to get all notes for the selected cat.</summary>
		public static QuickPasteNote[] GetForCat(long cat) {
			//No need to check RemotingRole; no call to db.
			if(List==null) {
				RefreshCache();
			}
			ArrayList ALnotes=new ArrayList();
			for(int i=0;i<List.Count;i++){
				if(List[i].QuickPasteCatNum==cat){
					ALnotes.Add(List[i]);
				}
			}
			QuickPasteNote[] retArray=new QuickPasteNote[ALnotes.Count];
			for(int i=0;i<ALnotes.Count;i++){
				retArray[i]=(QuickPasteNote)ALnotes[i];
			}
			return retArray;
		}

		///<summary>Only used from FormQuickPaste to get all notes.</summary>
		public static List<QuickPasteNote> GetAll() {
			if(List==null) {
				RefreshCache();
			}
			return List;
		}

		///<summary>Called on KeyUp from various textBoxes in the program to look for a ?abbrev and attempt to substitute.  Substitutes the text if found.</summary>
		public static string Substitute(string text,QuickPasteType type){
			//No need to check RemotingRole; no call to db.
			if(List==null) {
				RefreshCache();
			}
			int typeIndex=QuickPasteCats.GetDefaultType(type);
			for(int i=0;i<List.Count;i++){
				if(List[i].Abbreviation==""){
					continue;
				}
				if(List[i].QuickPasteCatNum!=QuickPasteCats.List[typeIndex].QuickPasteCatNum){
					continue;
				}
				//We have to replace all $ chars with $$ because Regex.Replace allows "Substitutions" in the replacement parameter.
				//The replacement parameter specifies the string that is to replace each match in input. replacement can consist of any combination of literal
				//text and substitutions. For example, the replacement pattern a*${test}b inserts the string "a*" followed by the substring that is matched by
				//the test capturing group, if any, followed by the string "b". 
				//The * character is not recognized as a metacharacter within a replacement pattern.
				//See https://msdn.microsoft.com/en-us/library/taz3ak2f(v=vs.110).aspx for more information.
				string quicknote=List[i].Note.Replace("$","$$");
				text=Regex.Replace(text,@"\?"+List[i].Abbreviation,quicknote);
			}
			return text;
		}

		///<summary>This should not be passing in two lists. Consider rewriting to only pass in one list and an identifier to get list from DB.</summary>
		public static bool Sync(List<QuickPasteNote> listNew,List<QuickPasteNote> listOld) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod(),listNew,listOld);
			}
			//Eventually we may want to change this to not be passing in listOld.
			return Crud.QuickPasteNoteCrud.Sync(listNew.Select(x=>x.Copy()).ToList(),listOld.Select(x=>x.Copy()).ToList());
		}


		


	}

	


}









