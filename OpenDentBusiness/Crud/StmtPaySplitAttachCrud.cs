//This file is automatically generated.
//Do not attempt to make changes to this file because the changes will be erased and overwritten.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace OpenDentBusiness.Crud{
	public class StmtPaySplitAttachCrud {
		///<summary>Gets one StmtPaySplitAttach object from the database using the primary key.  Returns null if not found.</summary>
		public static StmtPaySplitAttach SelectOne(long stmtPaySplitAttachNum){
			string command="SELECT * FROM stmtpaysplitattach "
				+"WHERE StmtPaySplitAttachNum = "+POut.Long(stmtPaySplitAttachNum);
			List<StmtPaySplitAttach> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets one StmtPaySplitAttach object from the database using a query.</summary>
		public static StmtPaySplitAttach SelectOne(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<StmtPaySplitAttach> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets a list of StmtPaySplitAttach objects from the database using a query.</summary>
		public static List<StmtPaySplitAttach> SelectMany(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<StmtPaySplitAttach> list=TableToList(Db.GetTable(command));
			return list;
		}

		///<summary>Converts a DataTable to a list of objects.</summary>
		public static List<StmtPaySplitAttach> TableToList(DataTable table){
			List<StmtPaySplitAttach> retVal=new List<StmtPaySplitAttach>();
			StmtPaySplitAttach stmtPaySplitAttach;
			foreach(DataRow row in table.Rows) {
				stmtPaySplitAttach=new StmtPaySplitAttach();
				stmtPaySplitAttach.StmtPaySplitAttachNum= PIn.Long  (row["StmtPaySplitAttachNum"].ToString());
				stmtPaySplitAttach.StatementNum         = PIn.Long  (row["StatementNum"].ToString());
				stmtPaySplitAttach.PaySplitNum          = PIn.Long  (row["PaySplitNum"].ToString());
				retVal.Add(stmtPaySplitAttach);
			}
			return retVal;
		}

		///<summary>Converts a list of StmtPaySplitAttach into a DataTable.</summary>
		public static DataTable ListToTable(List<StmtPaySplitAttach> listStmtPaySplitAttachs,string tableName="") {
			if(string.IsNullOrEmpty(tableName)) {
				tableName="StmtPaySplitAttach";
			}
			DataTable table=new DataTable(tableName);
			table.Columns.Add("StmtPaySplitAttachNum");
			table.Columns.Add("StatementNum");
			table.Columns.Add("PaySplitNum");
			foreach(StmtPaySplitAttach stmtPaySplitAttach in listStmtPaySplitAttachs) {
				table.Rows.Add(new object[] {
					POut.Long  (stmtPaySplitAttach.StmtPaySplitAttachNum),
					POut.Long  (stmtPaySplitAttach.StatementNum),
					POut.Long  (stmtPaySplitAttach.PaySplitNum),
				});
			}
			return table;
		}

		///<summary>Inserts one StmtPaySplitAttach into the database.  Returns the new priKey.</summary>
		public static long Insert(StmtPaySplitAttach stmtPaySplitAttach){
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				stmtPaySplitAttach.StmtPaySplitAttachNum=DbHelper.GetNextOracleKey("stmtpaysplitattach","StmtPaySplitAttachNum");
				int loopcount=0;
				while(loopcount<100){
					try {
						return Insert(stmtPaySplitAttach,true);
					}
					catch(Oracle.DataAccess.Client.OracleException ex){
						if(ex.Number==1 && ex.Message.ToLower().Contains("unique constraint") && ex.Message.ToLower().Contains("violated")){
							stmtPaySplitAttach.StmtPaySplitAttachNum++;
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
				return Insert(stmtPaySplitAttach,false);
			}
		}

		///<summary>Inserts one StmtPaySplitAttach into the database.  Provides option to use the existing priKey.</summary>
		public static long Insert(StmtPaySplitAttach stmtPaySplitAttach,bool useExistingPK){
			if(!useExistingPK && PrefC.RandomKeys) {
				stmtPaySplitAttach.StmtPaySplitAttachNum=ReplicationServers.GetKey("stmtpaysplitattach","StmtPaySplitAttachNum");
			}
			string command="INSERT INTO stmtpaysplitattach (";
			if(useExistingPK || PrefC.RandomKeys) {
				command+="StmtPaySplitAttachNum,";
			}
			command+="StatementNum,PaySplitNum) VALUES(";
			if(useExistingPK || PrefC.RandomKeys) {
				command+=POut.Long(stmtPaySplitAttach.StmtPaySplitAttachNum)+",";
			}
			command+=
				     POut.Long  (stmtPaySplitAttach.StatementNum)+","
				+    POut.Long  (stmtPaySplitAttach.PaySplitNum)+")";
			if(useExistingPK || PrefC.RandomKeys) {
				Db.NonQ(command);
			}
			else {
				stmtPaySplitAttach.StmtPaySplitAttachNum=Db.NonQ(command,true);
			}
			return stmtPaySplitAttach.StmtPaySplitAttachNum;
		}

		///<summary>Inserts one StmtPaySplitAttach into the database.  Returns the new priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(StmtPaySplitAttach stmtPaySplitAttach){
			if(DataConnection.DBtype==DatabaseType.MySql) {
				return InsertNoCache(stmtPaySplitAttach,false);
			}
			else {
				if(DataConnection.DBtype==DatabaseType.Oracle) {
					stmtPaySplitAttach.StmtPaySplitAttachNum=DbHelper.GetNextOracleKey("stmtpaysplitattach","StmtPaySplitAttachNum"); //Cacheless method
				}
				return InsertNoCache(stmtPaySplitAttach,true);
			}
		}

		///<summary>Inserts one StmtPaySplitAttach into the database.  Provides option to use the existing priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(StmtPaySplitAttach stmtPaySplitAttach,bool useExistingPK){
			bool isRandomKeys=Prefs.GetBoolNoCache(PrefName.RandomPrimaryKeys);
			string command="INSERT INTO stmtpaysplitattach (";
			if(!useExistingPK && isRandomKeys) {
				stmtPaySplitAttach.StmtPaySplitAttachNum=ReplicationServers.GetKeyNoCache("stmtpaysplitattach","StmtPaySplitAttachNum");
			}
			if(isRandomKeys || useExistingPK) {
				command+="StmtPaySplitAttachNum,";
			}
			command+="StatementNum,PaySplitNum) VALUES(";
			if(isRandomKeys || useExistingPK) {
				command+=POut.Long(stmtPaySplitAttach.StmtPaySplitAttachNum)+",";
			}
			command+=
				     POut.Long  (stmtPaySplitAttach.StatementNum)+","
				+    POut.Long  (stmtPaySplitAttach.PaySplitNum)+")";
			if(useExistingPK || isRandomKeys) {
				Db.NonQ(command);
			}
			else {
				stmtPaySplitAttach.StmtPaySplitAttachNum=Db.NonQ(command,true);
			}
			return stmtPaySplitAttach.StmtPaySplitAttachNum;
		}

		///<summary>Updates one StmtPaySplitAttach in the database.</summary>
		public static void Update(StmtPaySplitAttach stmtPaySplitAttach){
			string command="UPDATE stmtpaysplitattach SET "
				+"StatementNum         =  "+POut.Long  (stmtPaySplitAttach.StatementNum)+", "
				+"PaySplitNum          =  "+POut.Long  (stmtPaySplitAttach.PaySplitNum)+" "
				+"WHERE StmtPaySplitAttachNum = "+POut.Long(stmtPaySplitAttach.StmtPaySplitAttachNum);
			Db.NonQ(command);
		}

		///<summary>Updates one StmtPaySplitAttach in the database.  Uses an old object to compare to, and only alters changed fields.  This prevents collisions and concurrency problems in heavily used tables.  Returns true if an update occurred.</summary>
		public static bool Update(StmtPaySplitAttach stmtPaySplitAttach,StmtPaySplitAttach oldStmtPaySplitAttach){
			string command="";
			if(stmtPaySplitAttach.StatementNum != oldStmtPaySplitAttach.StatementNum) {
				if(command!=""){ command+=",";}
				command+="StatementNum = "+POut.Long(stmtPaySplitAttach.StatementNum)+"";
			}
			if(stmtPaySplitAttach.PaySplitNum != oldStmtPaySplitAttach.PaySplitNum) {
				if(command!=""){ command+=",";}
				command+="PaySplitNum = "+POut.Long(stmtPaySplitAttach.PaySplitNum)+"";
			}
			if(command==""){
				return false;
			}
			command="UPDATE stmtpaysplitattach SET "+command
				+" WHERE StmtPaySplitAttachNum = "+POut.Long(stmtPaySplitAttach.StmtPaySplitAttachNum);
			Db.NonQ(command);
			return true;
		}

		///<summary>Returns true if Update(StmtPaySplitAttach,StmtPaySplitAttach) would make changes to the database.
		///Does not make any changes to the database and can be called before remoting role is checked.</summary>
		public static bool UpdateComparison(StmtPaySplitAttach stmtPaySplitAttach,StmtPaySplitAttach oldStmtPaySplitAttach) {
			if(stmtPaySplitAttach.StatementNum != oldStmtPaySplitAttach.StatementNum) {
				return true;
			}
			if(stmtPaySplitAttach.PaySplitNum != oldStmtPaySplitAttach.PaySplitNum) {
				return true;
			}
			return false;
		}

		///<summary>Deletes one StmtPaySplitAttach from the database.</summary>
		public static void Delete(long stmtPaySplitAttachNum){
			string command="DELETE FROM stmtpaysplitattach "
				+"WHERE StmtPaySplitAttachNum = "+POut.Long(stmtPaySplitAttachNum);
			Db.NonQ(command);
		}

	}
}