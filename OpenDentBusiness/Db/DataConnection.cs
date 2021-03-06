/*=============================================================================================================
Open Dental GPL license Copyright (C) 2003  Jordan Sparks, DMD.  http://www.open-dent.com,  www.docsparks.com
See header in FormOpenDental.cs for complete text.  Redistributions must retain this text.
===============================================================================================================*/
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
#if !LINUX
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
#else
using System.Data.OracleClient;
#endif
using CodeBase;

namespace OpenDentBusiness {
	///<summary></summary>
	public enum DatabaseType {
		MySql,
		Oracle
		//MS_Sql
	}

	///<summary></summary>
	public class DataConnection {
		///<summary>This data adapter is used for all queries to the database.</summary>
		private MySqlDataAdapter da;
		///<summary>Data adapter when 'isOracle' is set to true.</summary>
		private OracleDataAdapter daOr;
		///<summary>This is the connection that is used by the data adapter for all queries.  8/30/2010 js Changed this to be not static so that we can use it with multiple threads.  Has potential to cause bugs.</summary>
		private MySqlConnection con;
		///<summary>Connection that is being used when 'isOracle' is set to true.</summary>
		private OracleConnection conOr;
		///<summary>Used to get very small bits of data from the db when the data adapter would be overkill.  For instance retrieving the response after a command is sent.</summary>
		private MySqlDataReader dr;
		///<summary>The data reader being used when 'isOracle' is set to true.</summary>
		private OracleDataReader drOr;
		///<summary>Stores the string of the command that will be sent to the database.</summary>
		private MySqlCommand cmd;
		///<summary>The command to set when 'isOracle' is set to true?</summary>
		public OracleCommand cmdOr;
		///<summary>After inserting a row, this variable will contain the primary key for the newly inserted row.  This can frequently save an additional query to the database.</summary>
		public long InsertID;
		private static string _database;
		private static string _serverName;
		private static string _mysqlUser;
		private static string _mysqlPass;
		//User with lower privileges:
		private static string _mysqlUserLow;
		private static string _mysqlPassLow;
		///<summary>If this is used, then none of the fields above will be set.</summary>
		private static string _connectionString="";
		///<summary>The value here is now reliable for public use.  FormChooseDatabase.DBtype, which used to be used for the client is now gone.</summary>
		private static DatabaseType _dBtype;
		//ThreadStatic Variables are thread specific and are thread safe thus do not require locking.
		[ThreadStatic]
		private static string _databaseT;
		[ThreadStatic]
		private static string _serverNameT;
		[ThreadStatic]
		private static string _mysqlUserT;
		[ThreadStatic]
		private static string _mysqlPassT;
		[ThreadStatic]
		private static string _mysqlUserLowT;
		[ThreadStatic]
		private static string _mysqlPassLowT;
		///<summary>If this is used, then none of the fields above will be set.</summary>
		[ThreadStatic]
		private static string _connectionStringT="";
		[ThreadStatic]
		private static DatabaseType _dBtypeT;
		///<summary>Set this variable to OpenDental's thread ID.  Until this is set, there is no chance of LostConnection events being fired.
		///This is only set after FormChooseDatabase is closed and has allowed the initial connection.  FormChooseDatabase can also be launched in
		///read-only mode from FormOpenDental, but the user cannot change the connection in that manner.</summary>
		public static int MainThreadId;
#if DEBUG
		///<summary>milliseconds.</summary>
		private static int delayForTesting=0;
		private static bool logDebugQueries=false;
#endif

		#region Properties

		///<summary>Only call from the main thread.  The value here is now reliable for public use.  FormChooseDatabase.DBtype, which used to be used for the client is now gone.</summary>
		public static DatabaseType DBtype {
			get {
				//We need to know if thread variables are being used (SetDbT was called).  Because DBType is an enum we do not have a way to check if _dBtypeT is "null or empty".
				//Check if _databaseT and _connectionStringT are null or empty which will indicate that we need to return _dBtype (set by the main thread) instead of _dBtypeT (thread specific).
				if(String.IsNullOrEmpty(_databaseT) && String.IsNullOrEmpty(_connectionStringT)) {
					//This will often get hit by separate threads that were spawned from the main thread and did not use SetDbT.  E.g. FormOpenDental.ThreadEmailInbox
					//They need to follow old behavior and use the old static, non-thread safe variables that are assumed to "never change" except on startup.
					return _dBtype;
				}
				return _dBtypeT;
			}
			set {
				_dBtype=value;
				_dBtypeT=value;
			}
		}

		//=====================================================================================================================================================
		// The following properties MUST first check if the thread static variables are null or empty which will cause the non thread safe static variables
		// to be returned.  This is because the main thread of Open Dental is written in a way that SetDb is only called once (on startup) and then
		// the static connection settings are used afterwards for subsequent connections / database calls.
		// Individual threads that need to access different databases (CEMT) need to call SetDbT before making db calls.
		//=====================================================================================================================================================
		
		private static string Database {
			get {
				if(String.IsNullOrEmpty(_databaseT)) {
					return _database;
				}
				else {
					return _databaseT;
				}
			}
			set {
				_database=value;
				_databaseT=value;
			}
		}

		private static string ServerName {
			get {
				if(String.IsNullOrEmpty(_serverNameT)) {
					return _serverName;
				}
				else {
					return _serverNameT;
				}
			}
			set {
				_serverName=value;
				_serverNameT=value;
			}
		}

		private static string MysqlUser {
			get {
				if(String.IsNullOrEmpty(_mysqlUserT)) {
					return _mysqlUser;
				}
				else {
					return _mysqlUserT;
				}
			}
			set {
				_mysqlUser=value;
				_mysqlUserT=value;
			}
		}

		private static string MysqlPass {
			get {
				if(String.IsNullOrEmpty(_mysqlPassT)) {
					return _mysqlPass;
				}
				else {
					return _mysqlPassT;
				}
			}
			set {
				_mysqlPass=value;
				_mysqlPassT=value;
			}
		}

		private static string MysqlUserLow {
			get {
				if(String.IsNullOrEmpty(_mysqlUserLowT)) {
					return _mysqlUserLow;
				}
				else {
					return _mysqlUserLowT;
				}
			}
			set {
				_mysqlUserLow=value;
				_mysqlUserLowT=value;
			}
		}

		private static string MysqlPassLow {
			get {
				if(String.IsNullOrEmpty(_mysqlPassLowT)) {
					return _mysqlPassLow;
				}
				else {
					return _mysqlPassLowT;
				}
			}
			set {
				_mysqlPassLow=value;
				_mysqlPassLowT=value;
			}
		}

		private static string ConnectionString {
			get {
				if(String.IsNullOrEmpty(_connectionStringT)) {
					return _connectionString;
				}
				else {
					return _connectionStringT;
				}
			}
			set {
				_connectionString=value;
				_connectionStringT=value;
			}
		}

		#endregion

		//For queries that do not use this flag, all queries are split into single commands. For those SQL commands which
		//are a single command but contain multiple semi-colons, then this string should be set to false before the 
		//command is executed, then set back to true immediately thereafter.
		//public static bool splitStrings=true;

		public static int DefaultPortNum() {
			switch(DBtype) {
				case DatabaseType.Oracle:
					return 1521;
				case DatabaseType.MySql:
					return 3306;
				default:
					return 3306;//Guess (same as MySQL, to target larger customer crowd).
			}
		}

		///<summary>From local variable.  Does not query the database.</summary>
		public static string GetConnectionString() {
			return ConnectionString;
		}

		///<summary>From local variable.  Does not query the database.</summary>
		public static string GetDatabaseName() {
			return Database;
		}

		///<summary>From local variable.  Does not query the database.</summary>
		public static string GetServerName() {
			return ServerName;
		}

		///<summary>From local variable.  Does not query the database.</summary>
		public static string GetMysqlUser() {
			return MysqlUser;
		}

		///<summary>From local variable.  Does not query the database.</summary>
		public static string GetMysqlPass() {
			return MysqlPass;
		}

		///<summary>From local variable.  Does not query the database.</summary>
		public static string GetMysqlUserLow() {
			return MysqlUserLow;
		}

		///<summary>From local variable.  Does not query the database.</summary>
		public static string GetMysqlPassLow() {
			return MysqlPassLow;
		}

		#region Constructors
		public DataConnection(bool isLow) {
			string connectStr=ConnectionString;
			if(connectStr.Length<1 && ServerName!=null) {
				connectStr=BuildSimpleConnectionString(ServerName,Database,MysqlUserLow,MysqlPassLow);
			}
			if(DBtype==DatabaseType.Oracle) {
				conOr=new OracleConnection(connectStr);
				//drOr = null;
				cmdOr=new OracleCommand();
				cmdOr.Connection=conOr;
			}
			else if(DBtype==DatabaseType.MySql) {
				con=new MySqlConnection(connectStr);
				//dr = null;
				cmd = new MySqlCommand();
				cmd.Connection=con;
			}
		}

		///<summary></summary>
		public DataConnection() {
			string connectStr=ConnectionString;
			if(connectStr.Length<1 && ServerName!=null) {
				connectStr=BuildSimpleConnectionString(ServerName,Database,MysqlUser,MysqlPass);
			}
			if(DBtype==DatabaseType.Oracle) {
				conOr=new OracleConnection(connectStr);
				//drOr = null;
				cmdOr=new OracleCommand();
				cmdOr.Connection=conOr;
				//table=new DataTable();
			}
			else if(DBtype==DatabaseType.MySql) {
				con=new MySqlConnection(connectStr);
				//dr = null;
				cmd = new MySqlCommand();
				cmd.Connection=con;
				//table=new DataTable();
			}
		}

		///<summary>Only used from FormChooseDatabase to attempt connection with database.</summary>
		public DataConnection(DatabaseType dbtype) {
			//SetDb will be run immediately, so no need to do anything here.
		}

		///<summary></summary>
		public DataConnection(string database) {
			string connectStr=ConnectionString;//this doesn't really set it to the new db as intended. Deal with later.
			if(connectStr.Length<1) {
				connectStr=BuildSimpleConnectionString(ServerName,database,MysqlUser,MysqlPass);
			}
			if(DBtype==DatabaseType.Oracle) {
				conOr=new OracleConnection(connectStr);
				//drOr=null;
				cmdOr=new OracleCommand();
				cmdOr.Connection=conOr;
				//table=new DataTable();
			}
			else if(DBtype==DatabaseType.MySql) {
				con=new MySqlConnection(connectStr);
				//dr = null;
				cmd = new MySqlCommand();
				cmd.Connection=con;
				//table=new DataTable();
			}
		}

		///<summary>Only used to fill the list of databases in the ChooseDatabase window and from Employees.GetAsteriskMissedCalls.</summary>
		public DataConnection(string serverName,string database,string mysqlUser,string mysqlPass,DatabaseType dtype) {
			string connectStr=ConnectionString;
			if(connectStr.Length<1) {
				connectStr=BuildSimpleConnectionString(dtype,serverName,database,mysqlUser,mysqlPass);
			}
			if(dtype==DatabaseType.Oracle) {
				conOr=new OracleConnection(connectStr);
				//drOr=null;
				cmdOr=new OracleCommand();
				cmdOr.Connection=conOr;
			}
			else if(DBtype==DatabaseType.MySql) {
				con=new MySqlConnection(connectStr);
				//dr = null;
				cmd = new MySqlCommand();
				cmd.Connection=con;
			}
		}

		///<summary>Used by the mobile server because it does not need to worry about 3-tier scenarios.  Only supports MySQL.</summary>
		public DataConnection(string connectStr,bool isMobile) {//isMobile is ignored.  Just needed to make it different than the other constructor.
			con=new MySqlConnection(connectStr);
			cmd = new MySqlCommand();
			cmd.Connection=con;
		}
		#endregion Constructors

		/*
		///<Summary>This is only meaningful if local connection instead of through server.  This might be added to be able to access from ODR when users start customizing their RDL reports.  But for now, we should build the connection string programmatically</Summary>
		public static string GetCurrentConnectionString(){
			//ONLY TEMPORARY
			return BuildSimpleConnectionString(DatabaseType.MySql,ServerName,Database,DefaultPortNum().ToString(),MysqlUser,MysqlPass);
		}*/

		public static string BuildSimpleConnectionString(DatabaseType pDbType,string pServer,string pDatabase,string pUserID,string pPassword) {
			string serverName=pServer;
			string port=DefaultPortNum().ToString();
			if(pServer.Contains(":")) {
				string[] serverNamePort=pServer.Split(new char[] { ':' },StringSplitOptions.RemoveEmptyEntries);
				serverName=serverNamePort[0];
				port=serverNamePort[1];
			}
			string connectStr="";
			/*
			if(DBtype==DatabaseType.Oracle){
				connectStr=
					"Data Source=(DESCRIPTION=(ADDRESS_LIST="
				+ "(ADDRESS=(PROTOCOL=TCP)(HOST="+serverName+")(PORT="+port+")))"
				+ "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME="+pDatabase+")));"
				+ "User Id="+pUserID+";Password="+pPassword+";";
			}
			else if(DBtype==DatabaseType.MySql){*/
			connectStr=
				"Server="+serverName
				+";Port="+port//although this does seem to cause a bug in Mono.  We will revisit this bug if needed to exclude the port option only for Mono.
				+";Database="+pDatabase
				//+";Connect Timeout=20"
				+";User ID="+pUserID
				+";Password="+pPassword
				+";CharSet=utf8"
				+";Treat Tiny As Boolean=false"
				+";Allow User Variables=true"
				+";Default Command Timeout=3600";//one hour timeout on commands.  Prevents crash during conversions, etc.
			//+";Pooling=false";
			//}
			return connectStr;
		}

		private string BuildSimpleConnectionString(string pServer,string pDatabase,string pUserID,string pPassword) {
			return BuildSimpleConnectionString(DBtype,pServer,pDatabase,pUserID,pPassword);
		}

		public static string GetCurrentConnectionString() {
			DataConnection dcon=new DataConnection();
			return dcon.con.ConnectionString;
		}

		//private void PrepOracleConnection(){
		//if(parameters.Count>0) {//Getting parameters for statement.
		//	for(int p=0;p<parameters.Count;p++) {
		//		cmdOr.Parameters.Add(":param"+(p+1),parameters[p]);
		//	}
		//	cmdOr.Prepare();
		//	parameters.Clear();
		//}
		//This affects performance.  We need a better alternative than this:
		//cmdOr.CommandText="ALTER SESSION SET NLS_DATE_FORMAT = 'YYYY-MM-DD HH24:MI:SS'";
		//try{
		//	cmdOr.ExecuteNonQuery();	//Change the date-time format for this oracle connection to match our
		//MySQL date-time format.
		//}
		//catch(Exception e) {
		//	Logger.openlog.LogMB("Oracle SQL Error: "+cmdOr.CommandText+"\r\n"+"Exception: "+e.ToString(),Logger.Severity.ERROR);
		//	throw;//continue to pass the exception one level up.
		//}
		//}

		///<summary>This needs to be run every time we switch databases, especially on startup.  Will throw an exception if fails.  Calling class should catch exception.</summary>
		public void SetDb(string server,string db,string user,string password,string userLow,string passLow,DatabaseType dbtype) {
			DBtype=dbtype;
			string connectStr=BuildSimpleConnectionString(server,db,user,password);
			string connectStrLow="";
			if(userLow!="") {
				connectStrLow=BuildSimpleConnectionString(server,db,userLow,passLow);
			}
			TestConnection(connectStr,connectStrLow,dbtype,false);
			//connection strings must be valid, so OK to set permanently
			Database=db;
			ServerName=server;//yes, it includes the port
			MysqlUser=user;
			MysqlPass=password;
			MysqlUserLow=userLow;
			MysqlPassLow=passLow;
		}

		///<summary>This needs to be run every time we switch databases, especially on startup.  Will throw an exception if fails.  Calling class should catch exception.</summary>
		public void SetDb(string connectStr,string connectStrLow,DatabaseType dbtype,bool skipValidation) {
			TestConnection(connectStr,connectStrLow,dbtype,skipValidation);
			//connection string must be valid, so OK to set permanently
			ConnectionString=connectStr;
		}

		///<summary></summary>
		public void SetDb(string connectStr,string connectStrLow,DatabaseType dbtype) {
			SetDb(connectStr,connectStrLow,dbtype,false);
		}

		///<summary>This method sets all the thread specific variables for the DataConnection.  It will leave all normal static connection variables.  
		///Should be called before connecting to a database from a thread outside of the main thread. Will validate both high and low permission connection by running an arbitrary query against each.</summary>
		public void SetDbT(string server,string db,string user,string password,string userLow,string passLow,DatabaseType dbtype) {
			SetDbT(server,db,user,password,userLow,passLow,dbtype,false);
		}
		
		///<summary>This method sets all the thread specific variables for the DataConnection.  It will leave all normal static connection variables.  
		///Should be called before connecting to a database from a thread outside of the main thread. Can optionally validate both high and low permission connections by running an arbitrary query against each.</summary>
		public void SetDbT(string server,string db,string user,string password,string userLow,string passLow,DatabaseType dbtype,bool skipValidation) {
			_dBtypeT=dbtype;
			string connectStr=BuildSimpleConnectionString(server,db,user,password);
			string connectStrLow="";
			if(userLow!="") {
				connectStrLow=BuildSimpleConnectionString(server,db,userLow,passLow);
			}
			TestConnection(connectStr,connectStrLow,dbtype,skipValidation);
			//connection strings must be valid, so OK to set permanently
			_databaseT=db;
			_serverNameT=server;//yes, it includes the port
			_mysqlUserT=user;
			_mysqlPassT=password;
			_mysqlUserLowT=userLow;
			_mysqlPassLowT=passLow;
		}

		///<summary>This method sets all the thread specific variables for the DataConnection.  It will leave all normal static connection variables.  Should be called before connecting to a database from a thread outside of the main thread.</summary>
		public void SetDbT(string connectStr,string connectStrLow,DatabaseType dbtype,bool skipValidation) {
			TestConnection(connectStr,connectStrLow,dbtype,skipValidation);
			//connection string must be valid, so OK to set permanently
			_connectionStringT=connectStr;
		}

		///<summary>This method sets all the thread specific variables for the DataConnection.  It will leave all normal static connection variables.  Should be called before connecting to a database from a thread outside of the main thread.</summary>
		public void SetDbT(string connectStr,string connectStrLow,DatabaseType dbtype) {
			SetDbT(connectStr,connectStrLow,dbtype,false);
		}

		///<summary>Fires an event to launch the LostConnection window to freeze the calling thread.</summary>
		private void ConnectionLost() {
			//Event to launch connection lost window from OpenDental.  Only do it once for each connection loss.
			DataConnectionEvent.Fire(new ODEventArgs("DataConnection","Connection to the MySQL server has been lost.  "
				+"Connectivity will be retried periodically.  Click Retry to attempt to connect manually or Exit Program to close the program."));
		}

		private void TestConnection(string connectStr,string connectStrLow,DatabaseType dbtype,bool skipValidation) {
			DBtype=dbtype;
			if(DBtype==DatabaseType.Oracle) {
				conOr=new OracleConnection(connectStr);
				cmdOr=new OracleCommand();
				conOr.Open();
				cmdOr.Connection=conOr;
				//UPDATE preference SET ValueString = '0' WHERE ValueString = '0'
				cmdOr.CommandText="UPDATE preference SET PrefName = '0' WHERE PrefName = '0'";
				cmdOr.ExecuteNonQuery();
				conOr.Close();
				if(connectStrLow!="") {
					conOr=new OracleConnection(connectStrLow);
					cmdOr=new OracleCommand();
					conOr.Open();
					cmdOr.Connection=conOr;
					cmdOr.CommandText="SELECT * FROM preference WHERE ValueString = 'DataBaseVersion'";
					DataTable table=new DataTable();
					daOr=new OracleDataAdapter(cmdOr);
					daOr.Fill(table);
					conOr.Close();
				}
			}
			else {
				con=new MySqlConnection(connectStr);
				cmd = new MySqlCommand();
				//cmd.CommandTimeout=30;
				cmd.Connection=con;
				con.Open();
				if(!skipValidation) {
					cmd.CommandText="UPDATE preference SET ValueString = '0' WHERE ValueString = '0'";
					cmd.ExecuteNonQuery();
				}
				con.Close();
				if(connectStrLow!="") {
					con=new MySqlConnection(connectStrLow);
					cmd = new MySqlCommand();
					cmd.Connection=con;
					con.Open();
					cmd.CommandText="SELECT * FROM preference WHERE ValueString = 'DataBaseVersion'";
					DataTable table=new DataTable();
					da=new MySqlDataAdapter(cmd);
					da.Fill(table);
					con.Close();
				}
			}
		}

		///<summary>Fills table with data from the database.</summary>
		public DataTable GetTable(string command,bool hasConnLost=true) {
#if DEBUG
			if(logDebugQueries) {
				Debug.WriteLine(command);
			}
			Thread.Sleep(delayForTesting);
#endif
			DataTable table=new DataTable();
			if(DBtype==DatabaseType.Oracle) {
				conOr.Open();
				//PrepOracleConnection();
				cmdOr.CommandText=command;
				daOr=new OracleDataAdapter(cmdOr);
#if DEBUG
				daOr.Fill(table);//When in debug, don't log the error.  This also throws the error from here so that programmers can fix the issue as they occur instead of rebuilding after each fix.
#else
				try {
					daOr.Fill(table);
				}
				catch(System.Exception e) {
					Logger.openlog.LogMB("Oracle SQL Error: "+cmdOr.CommandText+"\r\n"+"Exception: "+e.ToString(),Logger.Severity.ERROR);
					throw;//continue to pass the exception one level up.
				}
#endif
				conOr.Close();	
			}
			else if(DBtype==DatabaseType.MySql) {
				cmd.CommandText=command;
				try {
					con.Open();	
				}
				catch(MySqlException ex) {
					//MySqlExceptions are the only ones we want to catch.
					//Don't catch error 1153 (max_allowed_packet error), it will change program behavior if we do.
					if(hasConnLost
						&& Thread.CurrentThread.ManagedThreadId==MainThreadId 
						&& ((ex.Message.ToLower().Contains("stream") && ex.Message.ToLower().Contains("failed"))//Reading from the stream has failed 
						|| ex.Number==1042//Unable to connect to any of the specified MySQL hosts
						|| ex.Number==1045))//Access denied
						{
						//Only block the main thread with a ConnectionLost window.  All other threads need to handle their own exceptions.
						//This also addresses Middle Tier as the server's main thread shouldn't be going through this code (does not listen for the fail event).
						ConnectionLost();//The application pauses here in the main thread to wait for user input.
						con.Close();
						return GetTable(command);
					}
					else {
						throw ex;//Throw the exception for any child threads to preserve functionality.
					}
				}
				da=new MySqlDataAdapter(cmd);
				da.Fill(table);
				con.Close();
			}
			return table;
		}

		/*
		///<summary>Fills dataset with data from the database.</summary>
		public DataSet GetDs(string commands) {
			#if DEBUG
				if(logDebugQueries){
					Debug.WriteLine(commands);
				}
				Thread.Sleep(delayForTesting);
			#endif
			DataSet ds=new DataSet();
			if(DBtype==DatabaseType.Oracle){
				conOr.Open();
				//PrepOracleConnection();
				//string[] commandArray=new string[] { commands };
				//if(splitStrings) {
				//	commandArray=commands.Split(new char[] { ';' },StringSplitOptions.RemoveEmptyEntries);
				//}
				//Can't do batch queries in Oracle, so we have to split them up and run them individually.
				//foreach(string com in commandArray){
					cmdOr.CommandText=commands;
					daOr=new OracleDataAdapter(cmdOr);
					//DataTable dsTab=new DataTable();
					//try{
						daOr.Fill(ds);
					//}
					//catch(System.Exception e) {
					//	Logger.openlog.LogMB("Oracle SQL Error: "+cmdOr.CommandText+"\r\n"+"Exception: "+e.ToString(),Logger.Severity.ERROR);
					//	throw e;//continue to pass the exception one level up.
					//}
					//ds.Tables.Add(dsTab);
				//}
				conOr.Close();
			}
			else if(DBtype==DatabaseType.MySql){
				cmd.CommandText=commands;
				da=new MySqlDataAdapter(cmd);
				da.Fill(ds);
				con.Close();
			}
			return ds;
		}*/

		///<summary>Sends a non query command to the database and returns the number of rows affected. If true, then InsertID will be set to the value of the primary key of the newly inserted row.   WILL NOT RETURN CORRECT PRIMARY KEY if the query specifies the primary key.</summary>
		public long NonQ(string commands,bool getInsertID,params OdSqlParameter[] parameters) {
#if DEBUG
			if(logDebugQueries) {
				Debug.WriteLine(commands);
			}
			Thread.Sleep(delayForTesting);
#endif
			long rowsChanged=0;
			if(DBtype==DatabaseType.Oracle) {
				conOr.Open();
				//PrepOracleConnection();
				//string[] commandArray=new string[] {commands};
				//if(splitStrings){
				//  commandArray=commands.Split(new char[] {';'},StringSplitOptions.RemoveEmptyEntries);
				//}
				//Can't do batch queries in Oracle, so we have to split them up and run them individually.
				//try{
				//if(getInsertID){
				//	cmdOr.CommandText="LOCK TABLE preference IN EXCLUSIVE MODE";
				//	cmdOr.ExecuteNonQuery();//Lock the preference table, because we need exclusive access to the OracleInsertId.
				//}
				string[] commandList=SplitCommands(commands);
				for(int i=0;i<commandList.Length;i++) {
					cmdOr.CommandText=commandList[i];
					for(int p=0;p<parameters.Length;p++) { //Parameters are not used with batch commands so we don't need to worry about adding the parameters mulitple times.
						cmdOr.Parameters.Add(DbHelper.ParamChar+parameters[p].ParameterName,parameters[p].GetOracleDbType()).Value=parameters[p].Value;
						//cmdOr.Parameters.Add(parameters[p].GetOracleParameter());//doesn't work
					}
					rowsChanged=cmdOr.ExecuteNonQuery();
				}
				//}
				//}
				//catch(System.Exception e){
				//	Logger.openlog.LogMB("Oracle SQL Error: "+cmdOr.CommandText+"\r\n"+"Exception: "+e.ToString(),Logger.Severity.ERROR);
				//	throw;//continue to pass the exception one level up.
				//}
				//finally{
				/*
					if(getInsertID){
						try{
							cmdOr.CommandText="SELECT ValueString FROM preference WHERE PrefName='OracleInsertId'";
							daOr=new OracleDataAdapter(cmdOr);
							DataTable table=new DataTable();
							daOr.Fill(table);//Will always return a result, unless a critical error, in which case we will catch.
							this.InsertID=Convert.ToInt32((table.Rows[0][0]).ToString());
							cmdOr.CommandText="commit";
							cmdOr.ExecuteNonQuery();//Release the exlusive lock we attaned above.
						}
						catch(Exception e){
							Logger.openlog.LogMB("Oracle SQL Error: "+cmdOr.CommandText+"\r\n"+"Exception: "+e.ToString(),
								Logger.Severity.ERROR);
							throw e;//continue to pass the exception one level up.
						}
					}*/
				//}
				conOr.Close();
			}
			else if(DBtype==DatabaseType.MySql) {
#if DEBUG
				//Wait up to 5 minutes for queries to finish before timing out in debug mode.
				//This is because we have several projects that run huge DB creation scripts and cannot have the MySQL command timeout.
				cmd.CommandTimeout=300;
#endif
				cmd.CommandText=commands;
				for(int p=0;p<parameters.Length;p++) {
					cmd.Parameters.Add(DbHelper.ParamChar+parameters[p].ParameterName,parameters[p].GetMySqlDbType()).Value=parameters[p].Value;
				}
				try {
					con.Open();
				}
				catch(MySqlException ex) {
					//MySqlExceptions are the only ones we want to catch.
					//Don't catch error 1153 (max_allowed_packet error), it will change program behavior if we do.
					if(Thread.CurrentThread.ManagedThreadId==MainThreadId && 
						((ex.Message.ToLower().Contains("stream") && ex.Message.ToLower().Contains("failed"))//Reading from the stream has failed 
						|| ex.Number==1042//Unable to connect to any of the specified MySQL hosts
						|| ex.Number==1045))//Access denied
						{
						//Only block the main thread with a ConnectionLost window.  All other threads need to handle their own exceptions.
						//This also addresses Middle Tier as the server's main thread shouldn't be going through this code (does not listen for the fail event).
						ConnectionLost();//The application pauses here in the main thread to wait for user input.
						con.Close();
						return NonQ(commands,getInsertID,parameters);
					}
					else {
						throw ex;//Throw the exception for any child threads to preserve functionality.
					}
				}
				try {
					rowsChanged=cmd.ExecuteNonQuery();
				}
				catch(MySqlException ex) {
					if(ex.Number==1153) {
						throw new ApplicationException("Please add the following to your my.ini file: max_allowed_packet=40000000");
					}
					throw ex;
				}
				if(getInsertID) {
					//Will not return the correct primary key if the query specifies the primary key.
					cmd.CommandText="SELECT LAST_INSERT_ID()";
					dr=(MySqlDataReader)cmd.ExecuteReader();
					if(dr.Read()) {
						InsertID=Convert.ToInt64(dr[0].ToString());
					}
				}
				con.Close();
			}
			return rowsChanged;
		}

		///<summary>Sends a non query command to the database and returns the number of rows affected. If true, then InsertID will be set to the value of the primary key of the newly inserted row.</summary>
		public long NonQ(string command) {
			return NonQ(command,false);
		}

		///<summary>Use this for count(*) queries.  They are always guaranteed to return one and only one value.  Uses datareader instead of datatable, so faster.  Can also be used when retrieving prefs manually, since they will also return exactly one value</summary>
		public string GetCount(string command) {
#if DEBUG
			if(logDebugQueries) {
				Debug.WriteLine(command);
			}
			Thread.Sleep(delayForTesting);
#endif
			string retVal="";
			if(DBtype==DatabaseType.Oracle) {
				conOr.Open();
				//PrepOracleConnection();
				cmdOr.CommandText=command;
				try {
					drOr=(OracleDataReader)cmdOr.ExecuteReader();
				}
				catch(System.Exception e) {
					Logger.openlog.LogMB("Oracle SQL Error: "+cmdOr.CommandText+"\r\n"+"Exception: "+e.ToString(),Logger.Severity.ERROR);
					throw;//continue to pass the exception one level up.
				}
				drOr.Read();
				retVal=drOr[0].ToString();
				conOr.Close();
			}
			else if(DBtype==DatabaseType.MySql) {
				cmd.CommandText=command;
				try {
					con.Open();
				}
				catch(MySqlException ex) {
					//MySqlExceptions are the only ones we want to catch.
					//Don't catch error 1153 (max_allowed_packet error), it will change program behavior if we do.
					if(Thread.CurrentThread.ManagedThreadId==MainThreadId && 
						((ex.Message.ToLower().Contains("stream") && ex.Message.ToLower().Contains("failed"))//Reading from the stream has failed 
						|| ex.Number==1042//Unable to connect to any of the specified MySQL hosts
						|| ex.Number==1045))//Access denied
						{
						//Only block the main thread with a ConnectionLost window.  All other threads need to handle their own exceptions.
						//This also addresses Middle Tier as the server's main thread shouldn't be going through this code (does not listen for the fail event).
						ConnectionLost();//The application pauses here in the main thread to wait for user input.
						con.Close();
						return GetCount(command);
					}
					else {
						throw ex;//Throw the exception for any child threads to preserve functionality.
					}
				}
				dr=(MySqlDataReader)cmd.ExecuteReader();
				dr.Read();
				retVal=dr[0].ToString();
				con.Close();
			}
			return retVal;
		}

		///<summary>Get one value.</summary>
		public string GetScalar(string command) {
#if DEBUG
			if(logDebugQueries) {
				Debug.WriteLine(command);
			}
			Thread.Sleep(delayForTesting);
#endif
			object scalar;
			string retVal="";
			if(DBtype==DatabaseType.Oracle) {
				conOr.Open();
				//PrepOracleConnection();
				cmdOr.CommandText=command;
				try {
					scalar=cmdOr.ExecuteScalar();
				}
				catch(System.Exception e) {
					Logger.openlog.LogMB("Oracle SQL Error: "+cmdOr.CommandText+"\r\n"+"Exception: "+e.ToString(),Logger.Severity.ERROR);
					throw;//continue to pass the exception one level up.
				}
				if(scalar==null) {
					retVal="";
				}
				else {
					retVal=scalar.ToString();
				}
				conOr.Close();
			}
			else if(DBtype==DatabaseType.MySql) {
				cmd.CommandText=command;
				try {
					con.Open();
				}
				catch(MySqlException ex) {
					//MySqlExceptions are the only ones we want to catch.
					//Don't catch error 1153 (max_allowed_packet error), it will change program behavior if we do.
					if(Thread.CurrentThread.ManagedThreadId==MainThreadId && 
						((ex.Message.ToLower().Contains("stream") && ex.Message.ToLower().Contains("failed"))//Reading from the stream has failed 
						|| ex.Number==1042//Unable to connect to any of the specified MySQL hosts
						|| ex.Number==1045))//Access denied
						{
						//Only block the main thread with a ConnectionLost window.  All other threads need to handle their own exceptions.
						//This also addresses Middle Tier as the server's main thread shouldn't be going through this code (does not listen for the fail event).
						ConnectionLost();//The application pauses here in the main thread to wait for user input.
						con.Close();
						return GetScalar(command);
					}
					else {
						throw ex;//Throw the exception for any child threads to preserve functionality.
					}
				}
				scalar=cmd.ExecuteScalar();
				if(scalar==null) {
					retVal="";
				}
				else {
					retVal=scalar.ToString();
				}
				con.Close();
			}
			return retVal;
		}

		///<summary>Oracle only. Used to split a command string into a list of individual commands so that each command can be run individually. It has proven difficult to run multiple commands at one time in Oracle without making drastic changes to existing queries.</summary>
		private string[] SplitCommands(string batchCmd) {
			if(DBtype!=DatabaseType.Oracle) {
				return new string[] { batchCmd };
			}
			//Some commands are surrounded by a BEGIN and END block, which does correctly execute in the Oracle connector and is hard for us to parse if there are nested BEGIN and END blocks.
			if(batchCmd.TrimStart().StartsWith("BEGIN",StringComparison.OrdinalIgnoreCase)) {
				return new string[] { batchCmd };
			}
			//Possibilities within a single statement:
			//Reference for Oracle Text Literals: http://download.oracle.com/docs/cd/B19306_01/server.102/b14200/sql_elements003.htm#SQLRF00218
			//Oracle allows one to use alternative quoting mechanisms instead of single-quotes, but we are not going to worry about that here.
			//In Oracle, one must escape ' with '' so we can just treat these like a string ending and a new one beginning immediately afterward.
			//Commands are terminated with semi-colons. We need to watch for the case when a semi-colon is within a single-quote string. As long as we correctly handle strings and ignore all data in a string, this shouldn't matter.
			//If batchCmd contains a command which has maulformed single or double quotes, this algorithm will not work so we will need to throw an exception in this case.
			List<string> result=new List<string>();
			StringBuilder strb=new StringBuilder();
			//bool beginningChar;
			//bool isInString;
			for(int i=0;i<batchCmd.Length;i++) {
				/*
				if(batchCmd[i]=='"') {
					if(isInString && beginningChar=='"') {
						isInString=false;
					}
					else {
						isInString=true;
					}
				}
				if(batchCmd[i]==";" && !isInString) {
					if(strb.Length>0){
						result.Add(strb.ToString();
					}
					strb=new StringBuilder();
					continue;
				}
				strb.Append(batchCmd[i]);*/
				if(batchCmd[i]=='\'') { //Single quotes are escaped with a single quote. So, for the string 'Hi I''m Bob' there are always an even number of single quotes.
					do {
						strb.Append(batchCmd[i++]);//Uses i then increments it.
						if(i>=batchCmd.Length) {
							throw new ApplicationException("Mismatched quotes found while splitting command.");
						}
					} while(batchCmd[i]!='\'');
					strb.Append(batchCmd[i]);
				}
				else if(batchCmd[i]==';') { //top-level ; so this is the end of a command.
					if(strb.Length>0) {
						result.Add(strb.ToString());
					}
					strb=new StringBuilder();
				}
				else { //All other characters
					strb.Append(batchCmd[i]);
				}
			}
			if(strb.Length>0) { //Make sure to add the last command if it did not have a ;
				result.Add(strb.ToString());
			}
			return result.ToArray();
		}


	}
}
