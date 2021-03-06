using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace OpenDentBusiness{
	///<summary></summary>
	public class Computers {
		///<summary>A list of all computers that have logged into the database in the past.  Might be some extra computer names in the list unless user has cleaned it up.</summary>
		private static Computer[] list;

		public static Computer[] List{
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

		public static void EnsureComputerInDB(string computerName){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),computerName);
				return;
			}
			string command=
				"SELECT * from computer "
				+"WHERE compname = '"+computerName+"'";
			DataTable table=Db.GetTable(command);
			if(table.Rows.Count==0) {
				Computer Cur=new Computer();
				Cur.CompName=computerName;
				Computers.Insert(Cur);
			}
		}

		public static DataTable RefreshCache() {
			//No need to check RemotingRole; Calls GetTableRemotelyIfNeeded().
			EnsureComputerInDB(Environment.MachineName);
			string command="SELECT * FROM computer ORDER BY CompName";
			DataTable table=Cache.GetTableRemotelyIfNeeded(MethodBase.GetCurrentMethod(),command);
			table.TableName="Computer";
			FillCache(table);
			return table;
		}

		///<summary></summary>
		public static void FillCache(DataTable table) {
			//No need to check RemotingRole; no call to db.
			List=Crud.ComputerCrud.TableToList(table).ToArray();
		}

		///<summary>ONLY use this if compname is not already present</summary>
		public static long Insert(Computer comp) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				comp.ComputerNum=Meth.GetLong(MethodBase.GetCurrentMethod(),comp);
				return comp.ComputerNum;
			}
			return Crud.ComputerCrud.Insert(comp);
		}

		/*
		///<summary></summary>
		public static void Update(){
			string command= "UPDATE computer SET "
				+"compname = '"    +POut.PString(CompName)+"' "
				//+"printername = '" +POut.PString(PrinterName)+"' "
				+"WHERE ComputerNum = '"+POut.PInt(ComputerNum)+"'";
			//MessageBox.Show(string command);
			DataConnection dcon=new DataConnection();
 			Db.NonQ(command);
		}*/

		///<summary></summary>
		public static void Delete(Computer comp){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),comp);
				return;
			}
			string command= "DELETE FROM computer WHERE computernum = '"+comp.ComputerNum.ToString()+"'";
 			Db.NonQ(command);
		}

		///<summary>Only called from Printers.GetForSit</summary>
		public static Computer GetCur(){
			//No need to check RemotingRole; no call to db.
			for(int i=0;i<List.Length;i++){
				if(Environment.MachineName.ToUpper()==List[i].CompName.ToUpper()) {
					return List[i];
				}
			}
			return null;//this will never happen
		}

		public static List<string> GetRunningComputers() {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<string>>(MethodBase.GetCurrentMethod());
			}
			//heartbeat is every three minutes.  We'll allow four to be generous.
			string command="SELECT CompName FROM computer WHERE LastHeartBeat > SUBTIME(NOW(),'00:04:00')";
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				command="SELECT CompName FROM computer WHERE LastHeartBeat > SYSDATE - (4/1440)";
			}
			DataTable table=Db.GetTable(command);
			List<string> retVal=new List<string>();
			for(int i=0;i<table.Rows.Count;i++) {
				retVal.Add(table.Rows[i][0].ToString());
			}
			return retVal;
		}

		/// <summary>When starting up, in an attempt to be fast, it will not add a new computer to the list.</summary>
		public static void UpdateHeartBeat(string computerName,bool isStartup) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),computerName,isStartup);
				return;
			}
			if(!isStartup && list==null) {
				RefreshCache();//adds new computer to list
			}
			string command= "UPDATE computer SET LastHeartBeat="+DbHelper.Now()+" WHERE CompName = '"+POut.String(computerName)+"'";
			Db.NonQ(command);
		}

		public static void ClearHeartBeat(string computerName) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),computerName);
				return;
			}
			string command= "UPDATE computer SET LastHeartBeat="+POut.Date(new DateTime(0001,1,1),true)+" WHERE CompName = '"+POut.String(computerName)+"'";
			Db.NonQ(command);
		}

		public static void ClearAllHeartBeats(string machineNameException) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),machineNameException);
				return;
			}
			string command= "UPDATE computer SET LastHeartBeat="+POut.Date(new DateTime(0001,1,1),true)+" "
				+"WHERE CompName != '"+POut.String(machineNameException)+"'";
			Db.NonQ(command);
		}

		///<summary>Returns a list of strings in a specific order.  
		///The strings are as follows; socket (service name), version_comment (service comment), hostname (server name), and MySQL version
		///Oracle is not supported and will throw an exception to have the customer call us to add support.</summary>
		public static List<string> GetServiceInfo() {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<string>>(MethodBase.GetCurrentMethod());
			}
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				throw new Exception(Lans.g("Computer","Currently not Oracle compatible.  Please call support."));
			}
			List<string> retVal=new List<string>();
			DataTable table=Db.GetTable("SHOW VARIABLES WHERE Variable_name='socket'");//service name
			if(table.Rows.Count>0) {
				retVal.Add(table.Rows[0]["VALUE"].ToString());
			}
			else {
				retVal.Add("Not Found");
			}
			table=Db.GetTable("SHOW VARIABLES WHERE Variable_name='version_comment'");//service comment
			if(table.Rows.Count>0) {
				retVal.Add(table.Rows[0]["VALUE"].ToString());
			}
			else {
				retVal.Add("Not Found");
			}
			try { 
				table=Db.GetTable("SELECT @@hostname");//server name
				if(table.Rows.Count>0) {
					retVal.Add(table.Rows[0][0].ToString());
				}
				else {
					retVal.Add("Not Found");
				}
			}
			catch {
				retVal.Add("Not Found");//hostname variable doesn't exist
			}
			retVal.Add(MiscData.GetMySqlVersion());
			return retVal;
		}
	}
}