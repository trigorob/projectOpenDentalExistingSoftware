//This file is automatically generated.
//Do not attempt to make changes to this file because the changes will be erased and overwritten.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace OpenDentBusiness.Crud{
	public class StmtAdjAttachCrud {
		///<summary>Gets one StmtAdjAttach object from the database using the primary key.  Returns null if not found.</summary>
		public static StmtAdjAttach SelectOne(long stmtAdjAttachNum){
			string command="SELECT * FROM stmtadjattach "
				+"WHERE StmtAdjAttachNum = "+POut.Long(stmtAdjAttachNum);
			List<StmtAdjAttach> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets one StmtAdjAttach object from the database using a query.</summary>
		public static StmtAdjAttach SelectOne(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<StmtAdjAttach> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets a list of StmtAdjAttach objects from the database using a query.</summary>
		public static List<StmtAdjAttach> SelectMany(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<StmtAdjAttach> list=TableToList(Db.GetTable(command));
			return list;
		}

		///<summary>Converts a DataTable to a list of objects.</summary>
		public static List<StmtAdjAttach> TableToList(DataTable table){
			List<StmtAdjAttach> retVal=new List<StmtAdjAttach>();
			StmtAdjAttach stmtAdjAttach;
			foreach(DataRow row in table.Rows) {
				stmtAdjAttach=new StmtAdjAttach();
				stmtAdjAttach.StmtAdjAttachNum= PIn.Long  (row["StmtAdjAttachNum"].ToString());
				stmtAdjAttach.StatementNum    = PIn.Long  (row["StatementNum"].ToString());
				stmtAdjAttach.AdjNum          = PIn.Long  (row["AdjNum"].ToString());
				retVal.Add(stmtAdjAttach);
			}
			return retVal;
		}

		///<summary>Converts a list of StmtAdjAttach into a DataTable.</summary>
		public static DataTable ListToTable(List<StmtAdjAttach> listStmtAdjAttachs,string tableName="") {
			if(string.IsNullOrEmpty(tableName)) {
				tableName="StmtAdjAttach";
			}
			DataTable table=new DataTable(tableName);
			table.Columns.Add("StmtAdjAttachNum");
			table.Columns.Add("StatementNum");
			table.Columns.Add("AdjNum");
			foreach(StmtAdjAttach stmtAdjAttach in listStmtAdjAttachs) {
				table.Rows.Add(new object[] {
					POut.Long  (stmtAdjAttach.StmtAdjAttachNum),
					POut.Long  (stmtAdjAttach.StatementNum),
					POut.Long  (stmtAdjAttach.AdjNum),
				});
			}
			return table;
		}

		///<summary>Inserts one StmtAdjAttach into the database.  Returns the new priKey.</summary>
		public static long Insert(StmtAdjAttach stmtAdjAttach){
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				stmtAdjAttach.StmtAdjAttachNum=DbHelper.GetNextOracleKey("stmtadjattach","StmtAdjAttachNum");
				int loopcount=0;
				while(loopcount<100){
					try {
						return Insert(stmtAdjAttach,true);
					}
					catch(Oracle.DataAccess.Client.OracleException ex){
						if(ex.Number==1 && ex.Message.ToLower().Contains("unique constraint") && ex.Message.ToLower().Contains("violated")){
							stmtAdjAttach.StmtAdjAttachNum++;
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
				return Insert(stmtAdjAttach,false);
			}
		}

		///<summary>Inserts one StmtAdjAttach into the database.  Provides option to use the existing priKey.</summary>
		public static long Insert(StmtAdjAttach stmtAdjAttach,bool useExistingPK){
			if(!useExistingPK && PrefC.RandomKeys) {
				stmtAdjAttach.StmtAdjAttachNum=ReplicationServers.GetKey("stmtadjattach","StmtAdjAttachNum");
			}
			string command="INSERT INTO stmtadjattach (";
			if(useExistingPK || PrefC.RandomKeys) {
				command+="StmtAdjAttachNum,";
			}
			command+="StatementNum,AdjNum) VALUES(";
			if(useExistingPK || PrefC.RandomKeys) {
				command+=POut.Long(stmtAdjAttach.StmtAdjAttachNum)+",";
			}
			command+=
				     POut.Long  (stmtAdjAttach.StatementNum)+","
				+    POut.Long  (stmtAdjAttach.AdjNum)+")";
			if(useExistingPK || PrefC.RandomKeys) {
				Db.NonQ(command);
			}
			else {
				stmtAdjAttach.StmtAdjAttachNum=Db.NonQ(command,true);
			}
			return stmtAdjAttach.StmtAdjAttachNum;
		}

		///<summary>Inserts one StmtAdjAttach into the database.  Returns the new priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(StmtAdjAttach stmtAdjAttach){
			if(DataConnection.DBtype==DatabaseType.MySql) {
				return InsertNoCache(stmtAdjAttach,false);
			}
			else {
				if(DataConnection.DBtype==DatabaseType.Oracle) {
					stmtAdjAttach.StmtAdjAttachNum=DbHelper.GetNextOracleKey("stmtadjattach","StmtAdjAttachNum"); //Cacheless method
				}
				return InsertNoCache(stmtAdjAttach,true);
			}
		}

		///<summary>Inserts one StmtAdjAttach into the database.  Provides option to use the existing priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(StmtAdjAttach stmtAdjAttach,bool useExistingPK){
			bool isRandomKeys=Prefs.GetBoolNoCache(PrefName.RandomPrimaryKeys);
			string command="INSERT INTO stmtadjattach (";
			if(!useExistingPK && isRandomKeys) {
				stmtAdjAttach.StmtAdjAttachNum=ReplicationServers.GetKeyNoCache("stmtadjattach","StmtAdjAttachNum");
			}
			if(isRandomKeys || useExistingPK) {
				command+="StmtAdjAttachNum,";
			}
			command+="StatementNum,AdjNum) VALUES(";
			if(isRandomKeys || useExistingPK) {
				command+=POut.Long(stmtAdjAttach.StmtAdjAttachNum)+",";
			}
			command+=
				     POut.Long  (stmtAdjAttach.StatementNum)+","
				+    POut.Long  (stmtAdjAttach.AdjNum)+")";
			if(useExistingPK || isRandomKeys) {
				Db.NonQ(command);
			}
			else {
				stmtAdjAttach.StmtAdjAttachNum=Db.NonQ(command,true);
			}
			return stmtAdjAttach.StmtAdjAttachNum;
		}

		///<summary>Updates one StmtAdjAttach in the database.</summary>
		public static void Update(StmtAdjAttach stmtAdjAttach){
			string command="UPDATE stmtadjattach SET "
				+"StatementNum    =  "+POut.Long  (stmtAdjAttach.StatementNum)+", "
				+"AdjNum          =  "+POut.Long  (stmtAdjAttach.AdjNum)+" "
				+"WHERE StmtAdjAttachNum = "+POut.Long(stmtAdjAttach.StmtAdjAttachNum);
			Db.NonQ(command);
		}

		///<summary>Updates one StmtAdjAttach in the database.  Uses an old object to compare to, and only alters changed fields.  This prevents collisions and concurrency problems in heavily used tables.  Returns true if an update occurred.</summary>
		public static bool Update(StmtAdjAttach stmtAdjAttach,StmtAdjAttach oldStmtAdjAttach){
			string command="";
			if(stmtAdjAttach.StatementNum != oldStmtAdjAttach.StatementNum) {
				if(command!=""){ command+=",";}
				command+="StatementNum = "+POut.Long(stmtAdjAttach.StatementNum)+"";
			}
			if(stmtAdjAttach.AdjNum != oldStmtAdjAttach.AdjNum) {
				if(command!=""){ command+=",";}
				command+="AdjNum = "+POut.Long(stmtAdjAttach.AdjNum)+"";
			}
			if(command==""){
				return false;
			}
			command="UPDATE stmtadjattach SET "+command
				+" WHERE StmtAdjAttachNum = "+POut.Long(stmtAdjAttach.StmtAdjAttachNum);
			Db.NonQ(command);
			return true;
		}

		///<summary>Returns true if Update(StmtAdjAttach,StmtAdjAttach) would make changes to the database.
		///Does not make any changes to the database and can be called before remoting role is checked.</summary>
		public static bool UpdateComparison(StmtAdjAttach stmtAdjAttach,StmtAdjAttach oldStmtAdjAttach) {
			if(stmtAdjAttach.StatementNum != oldStmtAdjAttach.StatementNum) {
				return true;
			}
			if(stmtAdjAttach.AdjNum != oldStmtAdjAttach.AdjNum) {
				return true;
			}
			return false;
		}

		///<summary>Deletes one StmtAdjAttach from the database.</summary>
		public static void Delete(long stmtAdjAttachNum){
			string command="DELETE FROM stmtadjattach "
				+"WHERE StmtAdjAttachNum = "+POut.Long(stmtAdjAttachNum);
			Db.NonQ(command);
		}

	}
}