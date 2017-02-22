//This file is automatically generated.
//Do not attempt to make changes to this file because the changes will be erased and overwritten.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace OpenDentBusiness.Crud{
	public class FeeSchedCrud {
		///<summary>Gets one FeeSched object from the database using the primary key.  Returns null if not found.</summary>
		public static FeeSched SelectOne(long feeSchedNum){
			string command="SELECT * FROM feesched "
				+"WHERE FeeSchedNum = "+POut.Long(feeSchedNum);
			List<FeeSched> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets one FeeSched object from the database using a query.</summary>
		public static FeeSched SelectOne(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<FeeSched> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets a list of FeeSched objects from the database using a query.</summary>
		public static List<FeeSched> SelectMany(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<FeeSched> list=TableToList(Db.GetTable(command));
			return list;
		}

		///<summary>Converts a DataTable to a list of objects.</summary>
		public static List<FeeSched> TableToList(DataTable table){
			List<FeeSched> retVal=new List<FeeSched>();
			FeeSched feeSched;
			foreach(DataRow row in table.Rows) {
				feeSched=new FeeSched();
				feeSched.FeeSchedNum    = PIn.Long  (row["FeeSchedNum"].ToString());
				feeSched.Description    = PIn.String(row["Description"].ToString());
				feeSched.FeeSchedType   = (OpenDentBusiness.FeeScheduleType)PIn.Int(row["FeeSchedType"].ToString());
				feeSched.ItemOrder      = PIn.Int   (row["ItemOrder"].ToString());
				feeSched.IsHidden       = PIn.Bool  (row["IsHidden"].ToString());
				feeSched.IsGlobal       = PIn.Bool  (row["IsGlobal"].ToString());
				feeSched.SecUserNumEntry= PIn.Long  (row["SecUserNumEntry"].ToString());
				feeSched.SecDateEntry   = PIn.Date  (row["SecDateEntry"].ToString());
				feeSched.SecDateTEdit   = PIn.DateT (row["SecDateTEdit"].ToString());
				retVal.Add(feeSched);
			}
			return retVal;
		}

		///<summary>Converts a list of FeeSched into a DataTable.</summary>
		public static DataTable ListToTable(List<FeeSched> listFeeScheds,string tableName="") {
			if(string.IsNullOrEmpty(tableName)) {
				tableName="FeeSched";
			}
			DataTable table=new DataTable(tableName);
			table.Columns.Add("FeeSchedNum");
			table.Columns.Add("Description");
			table.Columns.Add("FeeSchedType");
			table.Columns.Add("ItemOrder");
			table.Columns.Add("IsHidden");
			table.Columns.Add("IsGlobal");
			table.Columns.Add("SecUserNumEntry");
			table.Columns.Add("SecDateEntry");
			table.Columns.Add("SecDateTEdit");
			foreach(FeeSched feeSched in listFeeScheds) {
				table.Rows.Add(new object[] {
					POut.Long  (feeSched.FeeSchedNum),
					            feeSched.Description,
					POut.Int   ((int)feeSched.FeeSchedType),
					POut.Int   (feeSched.ItemOrder),
					POut.Bool  (feeSched.IsHidden),
					POut.Bool  (feeSched.IsGlobal),
					POut.Long  (feeSched.SecUserNumEntry),
					POut.DateT (feeSched.SecDateEntry,false),
					POut.DateT (feeSched.SecDateTEdit,false),
				});
			}
			return table;
		}

		///<summary>Inserts one FeeSched into the database.  Returns the new priKey.</summary>
		public static long Insert(FeeSched feeSched){
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				feeSched.FeeSchedNum=DbHelper.GetNextOracleKey("feesched","FeeSchedNum");
				int loopcount=0;
				while(loopcount<100){
					try {
						return Insert(feeSched,true);
					}
					catch(Oracle.DataAccess.Client.OracleException ex){
						if(ex.Number==1 && ex.Message.ToLower().Contains("unique constraint") && ex.Message.ToLower().Contains("violated")){
							feeSched.FeeSchedNum++;
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
				return Insert(feeSched,false);
			}
		}

		///<summary>Inserts one FeeSched into the database.  Provides option to use the existing priKey.</summary>
		public static long Insert(FeeSched feeSched,bool useExistingPK){
			if(!useExistingPK && PrefC.RandomKeys) {
				feeSched.FeeSchedNum=ReplicationServers.GetKey("feesched","FeeSchedNum");
			}
			string command="INSERT INTO feesched (";
			if(useExistingPK || PrefC.RandomKeys) {
				command+="FeeSchedNum,";
			}
			command+="Description,FeeSchedType,ItemOrder,IsHidden,IsGlobal,SecUserNumEntry,SecDateEntry) VALUES(";
			if(useExistingPK || PrefC.RandomKeys) {
				command+=POut.Long(feeSched.FeeSchedNum)+",";
			}
			command+=
				 "'"+POut.String(feeSched.Description)+"',"
				+    POut.Int   ((int)feeSched.FeeSchedType)+","
				+    POut.Int   (feeSched.ItemOrder)+","
				+    POut.Bool  (feeSched.IsHidden)+","
				+    POut.Bool  (feeSched.IsGlobal)+","
				+    POut.Long  (feeSched.SecUserNumEntry)+","
				+    DbHelper.Now()+")";
				//SecDateTEdit can only be set by MySQL
			if(useExistingPK || PrefC.RandomKeys) {
				Db.NonQ(command);
			}
			else {
				feeSched.FeeSchedNum=Db.NonQ(command,true);
			}
			return feeSched.FeeSchedNum;
		}

		///<summary>Inserts one FeeSched into the database.  Returns the new priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(FeeSched feeSched){
			if(DataConnection.DBtype==DatabaseType.MySql) {
				return InsertNoCache(feeSched,false);
			}
			else {
				if(DataConnection.DBtype==DatabaseType.Oracle) {
					feeSched.FeeSchedNum=DbHelper.GetNextOracleKey("feesched","FeeSchedNum"); //Cacheless method
				}
				return InsertNoCache(feeSched,true);
			}
		}

		///<summary>Inserts one FeeSched into the database.  Provides option to use the existing priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(FeeSched feeSched,bool useExistingPK){
			bool isRandomKeys=Prefs.GetBoolNoCache(PrefName.RandomPrimaryKeys);
			string command="INSERT INTO feesched (";
			if(!useExistingPK && isRandomKeys) {
				feeSched.FeeSchedNum=ReplicationServers.GetKeyNoCache("feesched","FeeSchedNum");
			}
			if(isRandomKeys || useExistingPK) {
				command+="FeeSchedNum,";
			}
			command+="Description,FeeSchedType,ItemOrder,IsHidden,IsGlobal,SecUserNumEntry,SecDateEntry) VALUES(";
			if(isRandomKeys || useExistingPK) {
				command+=POut.Long(feeSched.FeeSchedNum)+",";
			}
			command+=
				 "'"+POut.String(feeSched.Description)+"',"
				+    POut.Int   ((int)feeSched.FeeSchedType)+","
				+    POut.Int   (feeSched.ItemOrder)+","
				+    POut.Bool  (feeSched.IsHidden)+","
				+    POut.Bool  (feeSched.IsGlobal)+","
				+    POut.Long  (feeSched.SecUserNumEntry)+","
				+    DbHelper.Now()+")";
				//SecDateTEdit can only be set by MySQL
			if(useExistingPK || isRandomKeys) {
				Db.NonQ(command);
			}
			else {
				feeSched.FeeSchedNum=Db.NonQ(command,true);
			}
			return feeSched.FeeSchedNum;
		}

		///<summary>Updates one FeeSched in the database.</summary>
		public static void Update(FeeSched feeSched){
			string command="UPDATE feesched SET "
				+"Description    = '"+POut.String(feeSched.Description)+"', "
				+"FeeSchedType   =  "+POut.Int   ((int)feeSched.FeeSchedType)+", "
				+"ItemOrder      =  "+POut.Int   (feeSched.ItemOrder)+", "
				+"IsHidden       =  "+POut.Bool  (feeSched.IsHidden)+", "
				+"IsGlobal       =  "+POut.Bool  (feeSched.IsGlobal)+" "
				//SecUserNumEntry excluded from update
				//SecDateEntry not allowed to change
				//SecDateTEdit can only be set by MySQL
				+"WHERE FeeSchedNum = "+POut.Long(feeSched.FeeSchedNum);
			Db.NonQ(command);
		}

		///<summary>Updates one FeeSched in the database.  Uses an old object to compare to, and only alters changed fields.  This prevents collisions and concurrency problems in heavily used tables.  Returns true if an update occurred.</summary>
		public static bool Update(FeeSched feeSched,FeeSched oldFeeSched){
			string command="";
			if(feeSched.Description != oldFeeSched.Description) {
				if(command!=""){ command+=",";}
				command+="Description = '"+POut.String(feeSched.Description)+"'";
			}
			if(feeSched.FeeSchedType != oldFeeSched.FeeSchedType) {
				if(command!=""){ command+=",";}
				command+="FeeSchedType = "+POut.Int   ((int)feeSched.FeeSchedType)+"";
			}
			if(feeSched.ItemOrder != oldFeeSched.ItemOrder) {
				if(command!=""){ command+=",";}
				command+="ItemOrder = "+POut.Int(feeSched.ItemOrder)+"";
			}
			if(feeSched.IsHidden != oldFeeSched.IsHidden) {
				if(command!=""){ command+=",";}
				command+="IsHidden = "+POut.Bool(feeSched.IsHidden)+"";
			}
			if(feeSched.IsGlobal != oldFeeSched.IsGlobal) {
				if(command!=""){ command+=",";}
				command+="IsGlobal = "+POut.Bool(feeSched.IsGlobal)+"";
			}
			//SecUserNumEntry excluded from update
			//SecDateEntry not allowed to change
			//SecDateTEdit can only be set by MySQL
			if(command==""){
				return false;
			}
			command="UPDATE feesched SET "+command
				+" WHERE FeeSchedNum = "+POut.Long(feeSched.FeeSchedNum);
			Db.NonQ(command);
			return true;
		}

		///<summary>Returns true if Update(FeeSched,FeeSched) would make changes to the database.
		///Does not make any changes to the database and can be called before remoting role is checked.</summary>
		public static bool UpdateComparison(FeeSched feeSched,FeeSched oldFeeSched) {
			if(feeSched.Description != oldFeeSched.Description) {
				return true;
			}
			if(feeSched.FeeSchedType != oldFeeSched.FeeSchedType) {
				return true;
			}
			if(feeSched.ItemOrder != oldFeeSched.ItemOrder) {
				return true;
			}
			if(feeSched.IsHidden != oldFeeSched.IsHidden) {
				return true;
			}
			if(feeSched.IsGlobal != oldFeeSched.IsGlobal) {
				return true;
			}
			//SecUserNumEntry excluded from update
			//SecDateEntry not allowed to change
			//SecDateTEdit can only be set by MySQL
			return false;
		}

		///<summary>Deletes one FeeSched from the database.</summary>
		public static void Delete(long feeSchedNum){
			string command="DELETE FROM feesched "
				+"WHERE FeeSchedNum = "+POut.Long(feeSchedNum);
			Db.NonQ(command);
		}

		///<summary>Inserts, updates, or deletes database rows to match supplied list.  Returns true if db changes were made.
		///Supply Security.CurUser.UserNum, used to set the SecUserNumEntry field for Inserts.</summary>
		public static bool Sync(List<FeeSched> listNew,List<FeeSched> listDB,long userNum) {
			//Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
			List<FeeSched> listIns    =new List<FeeSched>();
			List<FeeSched> listUpdNew =new List<FeeSched>();
			List<FeeSched> listUpdDB  =new List<FeeSched>();
			List<FeeSched> listDel    =new List<FeeSched>();
			listNew.Sort((FeeSched x,FeeSched y) => { return x.FeeSchedNum.CompareTo(y.FeeSchedNum); });//Anonymous function, sorts by compairing PK.  Lambda expressions are not allowed, this is the one and only exception.  JS approved.
			listDB.Sort((FeeSched x,FeeSched y) => { return x.FeeSchedNum.CompareTo(y.FeeSchedNum); });//Anonymous function, sorts by compairing PK.  Lambda expressions are not allowed, this is the one and only exception.  JS approved.
			int idxNew=0;
			int idxDB=0;
			int rowsUpdatedCount=0;
			FeeSched fieldNew;
			FeeSched fieldDB;
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
				else if(fieldNew.FeeSchedNum<fieldDB.FeeSchedNum) {//newPK less than dbPK, newItem is 'next'
					listIns.Add(fieldNew);
					idxNew++;
					continue;
				}
				else if(fieldNew.FeeSchedNum>fieldDB.FeeSchedNum) {//dbPK less than newPK, dbItem is 'next'
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
				listIns[i].SecUserNumEntry=userNum;
				Insert(listIns[i]);
			}
			for(int i=0;i<listUpdNew.Count;i++) {
				if(Update(listUpdNew[i],listUpdDB[i])){
					rowsUpdatedCount++;
				}
			}
			for(int i=0;i<listDel.Count;i++) {
				Delete(listDel[i].FeeSchedNum);
			}
			if(rowsUpdatedCount>0 || listIns.Count>0 || listDel.Count>0) {
				return true;
			}
			return false;
		}

	}
}