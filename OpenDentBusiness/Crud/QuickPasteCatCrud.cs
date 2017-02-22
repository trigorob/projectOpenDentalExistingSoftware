//This file is automatically generated.
//Do not attempt to make changes to this file because the changes will be erased and overwritten.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace OpenDentBusiness.Crud{
	public class QuickPasteCatCrud {
		///<summary>Gets one QuickPasteCat object from the database using the primary key.  Returns null if not found.</summary>
		public static QuickPasteCat SelectOne(long quickPasteCatNum){
			string command="SELECT * FROM quickpastecat "
				+"WHERE QuickPasteCatNum = "+POut.Long(quickPasteCatNum);
			List<QuickPasteCat> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets one QuickPasteCat object from the database using a query.</summary>
		public static QuickPasteCat SelectOne(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<QuickPasteCat> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets a list of QuickPasteCat objects from the database using a query.</summary>
		public static List<QuickPasteCat> SelectMany(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<QuickPasteCat> list=TableToList(Db.GetTable(command));
			return list;
		}

		///<summary>Converts a DataTable to a list of objects.</summary>
		public static List<QuickPasteCat> TableToList(DataTable table){
			List<QuickPasteCat> retVal=new List<QuickPasteCat>();
			QuickPasteCat quickPasteCat;
			foreach(DataRow row in table.Rows) {
				quickPasteCat=new QuickPasteCat();
				quickPasteCat.QuickPasteCatNum= PIn.Long  (row["QuickPasteCatNum"].ToString());
				quickPasteCat.Description     = PIn.String(row["Description"].ToString());
				quickPasteCat.ItemOrder       = PIn.Int   (row["ItemOrder"].ToString());
				quickPasteCat.DefaultForTypes = PIn.String(row["DefaultForTypes"].ToString());
				retVal.Add(quickPasteCat);
			}
			return retVal;
		}

		///<summary>Converts a list of QuickPasteCat into a DataTable.</summary>
		public static DataTable ListToTable(List<QuickPasteCat> listQuickPasteCats,string tableName="") {
			if(string.IsNullOrEmpty(tableName)) {
				tableName="QuickPasteCat";
			}
			DataTable table=new DataTable(tableName);
			table.Columns.Add("QuickPasteCatNum");
			table.Columns.Add("Description");
			table.Columns.Add("ItemOrder");
			table.Columns.Add("DefaultForTypes");
			foreach(QuickPasteCat quickPasteCat in listQuickPasteCats) {
				table.Rows.Add(new object[] {
					POut.Long  (quickPasteCat.QuickPasteCatNum),
					            quickPasteCat.Description,
					POut.Int   (quickPasteCat.ItemOrder),
					            quickPasteCat.DefaultForTypes,
				});
			}
			return table;
		}

		///<summary>Inserts one QuickPasteCat into the database.  Returns the new priKey.</summary>
		public static long Insert(QuickPasteCat quickPasteCat){
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				quickPasteCat.QuickPasteCatNum=DbHelper.GetNextOracleKey("quickpastecat","QuickPasteCatNum");
				int loopcount=0;
				while(loopcount<100){
					try {
						return Insert(quickPasteCat,true);
					}
					catch(Oracle.DataAccess.Client.OracleException ex){
						if(ex.Number==1 && ex.Message.ToLower().Contains("unique constraint") && ex.Message.ToLower().Contains("violated")){
							quickPasteCat.QuickPasteCatNum++;
							loopcount++;
						}
						else{
							throw ex;
						}
					}
				}
				throw new ApplicationException("Insert failed.  Could not generate primary key.");
			}
			else {
				return Insert(quickPasteCat,false);
			}
		}

		///<summary>Inserts one QuickPasteCat into the database.  Provides option to use the existing priKey.</summary>
		public static long Insert(QuickPasteCat quickPasteCat,bool useExistingPK){
			if(!useExistingPK && PrefC.RandomKeys) {
				quickPasteCat.QuickPasteCatNum=ReplicationServers.GetKey("quickpastecat","QuickPasteCatNum");
			}
			string command="INSERT INTO quickpastecat (";
			if(useExistingPK || PrefC.RandomKeys) {
				command+="QuickPasteCatNum,";
			}
			command+="Description,ItemOrder,DefaultForTypes) VALUES(";
			if(useExistingPK || PrefC.RandomKeys) {
				command+=POut.Long(quickPasteCat.QuickPasteCatNum)+",";
			}
			command+=
				 "'"+POut.String(quickPasteCat.Description)+"',"
				+    POut.Int   (quickPasteCat.ItemOrder)+","
				+"'"+POut.String(quickPasteCat.DefaultForTypes)+"')";
			if(useExistingPK || PrefC.RandomKeys) {
				Db.NonQ(command);
			}
			else {
				quickPasteCat.QuickPasteCatNum=Db.NonQ(command,true);
			}
			return quickPasteCat.QuickPasteCatNum;
		}

		///<summary>Inserts one QuickPasteCat into the database.  Returns the new priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(QuickPasteCat quickPasteCat){
			if(DataConnection.DBtype==DatabaseType.MySql) {
				return InsertNoCache(quickPasteCat,false);
			}
			else {
				if(DataConnection.DBtype==DatabaseType.Oracle) {
					quickPasteCat.QuickPasteCatNum=DbHelper.GetNextOracleKey("quickpastecat","QuickPasteCatNum"); //Cacheless method
				}
				return InsertNoCache(quickPasteCat,true);
			}
		}

		///<summary>Inserts one QuickPasteCat into the database.  Provides option to use the existing priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(QuickPasteCat quickPasteCat,bool useExistingPK){
			bool isRandomKeys=Prefs.GetBoolNoCache(PrefName.RandomPrimaryKeys);
			string command="INSERT INTO quickpastecat (";
			if(!useExistingPK && isRandomKeys) {
				quickPasteCat.QuickPasteCatNum=ReplicationServers.GetKeyNoCache("quickpastecat","QuickPasteCatNum");
			}
			if(isRandomKeys || useExistingPK) {
				command+="QuickPasteCatNum,";
			}
			command+="Description,ItemOrder,DefaultForTypes) VALUES(";
			if(isRandomKeys || useExistingPK) {
				command+=POut.Long(quickPasteCat.QuickPasteCatNum)+",";
			}
			command+=
				 "'"+POut.String(quickPasteCat.Description)+"',"
				+    POut.Int   (quickPasteCat.ItemOrder)+","
				+"'"+POut.String(quickPasteCat.DefaultForTypes)+"')";
			if(useExistingPK || isRandomKeys) {
				Db.NonQ(command);
			}
			else {
				quickPasteCat.QuickPasteCatNum=Db.NonQ(command,true);
			}
			return quickPasteCat.QuickPasteCatNum;
		}

		///<summary>Updates one QuickPasteCat in the database.</summary>
		public static void Update(QuickPasteCat quickPasteCat){
			string command="UPDATE quickpastecat SET "
				+"Description     = '"+POut.String(quickPasteCat.Description)+"', "
				+"ItemOrder       =  "+POut.Int   (quickPasteCat.ItemOrder)+", "
				+"DefaultForTypes = '"+POut.String(quickPasteCat.DefaultForTypes)+"' "
				+"WHERE QuickPasteCatNum = "+POut.Long(quickPasteCat.QuickPasteCatNum);
			Db.NonQ(command);
		}

		///<summary>Updates one QuickPasteCat in the database.  Uses an old object to compare to, and only alters changed fields.  This prevents collisions and concurrency problems in heavily used tables.  Returns true if an update occurred.</summary>
		public static bool Update(QuickPasteCat quickPasteCat,QuickPasteCat oldQuickPasteCat){
			string command="";
			if(quickPasteCat.Description != oldQuickPasteCat.Description) {
				if(command!=""){ command+=",";}
				command+="Description = '"+POut.String(quickPasteCat.Description)+"'";
			}
			if(quickPasteCat.ItemOrder != oldQuickPasteCat.ItemOrder) {
				if(command!=""){ command+=",";}
				command+="ItemOrder = "+POut.Int(quickPasteCat.ItemOrder)+"";
			}
			if(quickPasteCat.DefaultForTypes != oldQuickPasteCat.DefaultForTypes) {
				if(command!=""){ command+=",";}
				command+="DefaultForTypes = '"+POut.String(quickPasteCat.DefaultForTypes)+"'";
			}
			if(command==""){
				return false;
			}
			command="UPDATE quickpastecat SET "+command
				+" WHERE QuickPasteCatNum = "+POut.Long(quickPasteCat.QuickPasteCatNum);
			Db.NonQ(command);
			return true;
		}

		///<summary>Returns true if Update(QuickPasteCat,QuickPasteCat) would make changes to the database.
		///Does not make any changes to the database and can be called before remoting role is checked.</summary>
		public static bool UpdateComparison(QuickPasteCat quickPasteCat,QuickPasteCat oldQuickPasteCat) {
			if(quickPasteCat.Description != oldQuickPasteCat.Description) {
				return true;
			}
			if(quickPasteCat.ItemOrder != oldQuickPasteCat.ItemOrder) {
				return true;
			}
			if(quickPasteCat.DefaultForTypes != oldQuickPasteCat.DefaultForTypes) {
				return true;
			}
			return false;
		}

		///<summary>Deletes one QuickPasteCat from the database.</summary>
		public static void Delete(long quickPasteCatNum){
			string command="DELETE FROM quickpastecat "
				+"WHERE QuickPasteCatNum = "+POut.Long(quickPasteCatNum);
			Db.NonQ(command);
		}

		///<summary>Inserts, updates, or deletes database rows to match supplied list.  Returns true if db changes were made.</summary>
		public static bool Sync(List<QuickPasteCat> listNew,List<QuickPasteCat> listDB) {
			//Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
			List<QuickPasteCat> listIns    =new List<QuickPasteCat>();
			List<QuickPasteCat> listUpdNew =new List<QuickPasteCat>();
			List<QuickPasteCat> listUpdDB  =new List<QuickPasteCat>();
			List<QuickPasteCat> listDel    =new List<QuickPasteCat>();
			listNew.Sort((QuickPasteCat x,QuickPasteCat y) => { return x.QuickPasteCatNum.CompareTo(y.QuickPasteCatNum); });//Anonymous function, sorts by compairing PK.  Lambda expressions are not allowed, this is the one and only exception.  JS approved.
			listDB.Sort((QuickPasteCat x,QuickPasteCat y) => { return x.QuickPasteCatNum.CompareTo(y.QuickPasteCatNum); });//Anonymous function, sorts by compairing PK.  Lambda expressions are not allowed, this is the one and only exception.  JS approved.
			int idxNew=0;
			int idxDB=0;
			int rowsUpdatedCount=0;
			QuickPasteCat fieldNew;
			QuickPasteCat fieldDB;
			//Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
			//If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
			while(idxNew<listNew.Count || idxDB<listDB.Count) {
				fieldNew=null;
				if(idxNew<listNew.Count) {
					fieldNew=listNew[idxNew];
				}
				fieldDB=null;
				if(idxDB<listDB.Count) {
					fieldDB=listDB[idxDB];
				}
				//begin compare
				if(fieldNew!=null && fieldDB==null) {//listNew has more items, listDB does not.
					listIns.Add(fieldNew);
					idxNew++;
					continue;
				}
				else if(fieldNew==null && fieldDB!=null) {//listDB has more items, listNew does not.
					listDel.Add(fieldDB);
					idxDB++;
					continue;
				}
				else if(fieldNew.QuickPasteCatNum<fieldDB.QuickPasteCatNum) {//newPK less than dbPK, newItem is 'next'
					listIns.Add(fieldNew);
					idxNew++;
					continue;
				}
				else if(fieldNew.QuickPasteCatNum>fieldDB.QuickPasteCatNum) {//dbPK less than newPK, dbItem is 'next'
					listDel.Add(fieldDB);
					idxDB++;
					continue;
				}
				//Both lists contain the 'next' item, update required
				listUpdNew.Add(fieldNew);
				listUpdDB.Add(fieldDB);
				idxNew++;
				idxDB++;
			}
			//Commit changes to DB
			for(int i=0;i<listIns.Count;i++) {
				Insert(listIns[i]);
			}
			for(int i=0;i<listUpdNew.Count;i++) {
				if(Update(listUpdNew[i],listUpdDB[i])){
					rowsUpdatedCount++;
				}
			}
			for(int i=0;i<listDel.Count;i++) {
				Delete(listDel[i].QuickPasteCatNum);
			}
			if(rowsUpdatedCount>0 || listIns.Count>0 || listDel.Count>0) {
				return true;
			}
			return false;
		}

	}
}