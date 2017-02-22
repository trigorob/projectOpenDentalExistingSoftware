//This file is automatically generated.
//Do not attempt to make changes to this file because the changes will be erased and overwritten.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace OpenDentBusiness.Crud{
	public class MedLabResultCrud {
		///<summary>Gets one MedLabResult object from the database using the primary key.  Returns null if not found.</summary>
		public static MedLabResult SelectOne(long medLabResultNum){
			string command="SELECT * FROM medlabresult "
				+"WHERE MedLabResultNum = "+POut.Long(medLabResultNum);
			List<MedLabResult> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets one MedLabResult object from the database using a query.</summary>
		public static MedLabResult SelectOne(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<MedLabResult> list=TableToList(Db.GetTable(command));
			if(list.Count==0) {
				return null;
			}
			return list[0];
		}

		///<summary>Gets a list of MedLabResult objects from the database using a query.</summary>
		public static List<MedLabResult> SelectMany(string command){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				throw new ApplicationException("Not allowed to send sql directly.  Rewrite the calling class to not use this query:\r\n"+command);
			}
			List<MedLabResult> list=TableToList(Db.GetTable(command));
			return list;
		}

		///<summary>Converts a DataTable to a list of objects.</summary>
		public static List<MedLabResult> TableToList(DataTable table){
			List<MedLabResult> retVal=new List<MedLabResult>();
			MedLabResult medLabResult;
			foreach(DataRow row in table.Rows) {
				medLabResult=new MedLabResult();
				medLabResult.MedLabResultNum= PIn.Long  (row["MedLabResultNum"].ToString());
				medLabResult.MedLabNum      = PIn.Long  (row["MedLabNum"].ToString());
				medLabResult.ObsID          = PIn.String(row["ObsID"].ToString());
				medLabResult.ObsText        = PIn.String(row["ObsText"].ToString());
				medLabResult.ObsLoinc       = PIn.String(row["ObsLoinc"].ToString());
				medLabResult.ObsLoincText   = PIn.String(row["ObsLoincText"].ToString());
				medLabResult.ObsIDSub       = PIn.String(row["ObsIDSub"].ToString());
				medLabResult.ObsValue       = PIn.String(row["ObsValue"].ToString());
				string obsSubType=row["ObsSubType"].ToString();
				if(obsSubType==""){
					medLabResult.ObsSubType   =(DataSubtype)0;
				}
				else try{
					medLabResult.ObsSubType   =(DataSubtype)Enum.Parse(typeof(DataSubtype),obsSubType);
				}
				catch{
					medLabResult.ObsSubType   =(DataSubtype)0;
				}
				medLabResult.ObsUnits       = PIn.String(row["ObsUnits"].ToString());
				medLabResult.ReferenceRange = PIn.String(row["ReferenceRange"].ToString());
				string abnormalFlag=row["AbnormalFlag"].ToString();
				if(abnormalFlag==""){
					medLabResult.AbnormalFlag =(AbnormalFlag)0;
				}
				else try{
					medLabResult.AbnormalFlag =(AbnormalFlag)Enum.Parse(typeof(AbnormalFlag),abnormalFlag);
				}
				catch{
					medLabResult.AbnormalFlag =(AbnormalFlag)0;
				}
				string resultStatus=row["ResultStatus"].ToString();
				if(resultStatus==""){
					medLabResult.ResultStatus =(ResultStatus)0;
				}
				else try{
					medLabResult.ResultStatus =(ResultStatus)Enum.Parse(typeof(ResultStatus),resultStatus);
				}
				catch{
					medLabResult.ResultStatus =(ResultStatus)0;
				}
				medLabResult.DateTimeObs    = PIn.DateT (row["DateTimeObs"].ToString());
				medLabResult.FacilityID     = PIn.String(row["FacilityID"].ToString());
				medLabResult.DocNum         = PIn.Long  (row["DocNum"].ToString());
				medLabResult.Note           = PIn.String(row["Note"].ToString());
				retVal.Add(medLabResult);
			}
			return retVal;
		}

		///<summary>Converts a list of MedLabResult into a DataTable.</summary>
		public static DataTable ListToTable(List<MedLabResult> listMedLabResults,string tableName="") {
			if(string.IsNullOrEmpty(tableName)) {
				tableName="MedLabResult";
			}
			DataTable table=new DataTable(tableName);
			table.Columns.Add("MedLabResultNum");
			table.Columns.Add("MedLabNum");
			table.Columns.Add("ObsID");
			table.Columns.Add("ObsText");
			table.Columns.Add("ObsLoinc");
			table.Columns.Add("ObsLoincText");
			table.Columns.Add("ObsIDSub");
			table.Columns.Add("ObsValue");
			table.Columns.Add("ObsSubType");
			table.Columns.Add("ObsUnits");
			table.Columns.Add("ReferenceRange");
			table.Columns.Add("AbnormalFlag");
			table.Columns.Add("ResultStatus");
			table.Columns.Add("DateTimeObs");
			table.Columns.Add("FacilityID");
			table.Columns.Add("DocNum");
			table.Columns.Add("Note");
			foreach(MedLabResult medLabResult in listMedLabResults) {
				table.Rows.Add(new object[] {
					POut.Long  (medLabResult.MedLabResultNum),
					POut.Long  (medLabResult.MedLabNum),
					            medLabResult.ObsID,
					            medLabResult.ObsText,
					            medLabResult.ObsLoinc,
					            medLabResult.ObsLoincText,
					            medLabResult.ObsIDSub,
					            medLabResult.ObsValue,
					POut.Int   ((int)medLabResult.ObsSubType),
					            medLabResult.ObsUnits,
					            medLabResult.ReferenceRange,
					POut.Int   ((int)medLabResult.AbnormalFlag),
					POut.Int   ((int)medLabResult.ResultStatus),
					POut.DateT (medLabResult.DateTimeObs,false),
					            medLabResult.FacilityID,
					POut.Long  (medLabResult.DocNum),
					            medLabResult.Note,
				});
			}
			return table;
		}

		///<summary>Inserts one MedLabResult into the database.  Returns the new priKey.</summary>
		public static long Insert(MedLabResult medLabResult){
			if(DataConnection.DBtype==DatabaseType.Oracle) {
				medLabResult.MedLabResultNum=DbHelper.GetNextOracleKey("medlabresult","MedLabResultNum");
				int loopcount=0;
				while(loopcount<100){
					try {
						return Insert(medLabResult,true);
					}
					catch(Oracle.DataAccess.Client.OracleException ex){
						if(ex.Number==1 && ex.Message.ToLower().Contains("unique constraint") && ex.Message.ToLower().Contains("violated")){
							medLabResult.MedLabResultNum++;
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
				return Insert(medLabResult,false);
			}
		}

		///<summary>Inserts one MedLabResult into the database.  Provides option to use the existing priKey.</summary>
		public static long Insert(MedLabResult medLabResult,bool useExistingPK){
			if(!useExistingPK && PrefC.RandomKeys) {
				medLabResult.MedLabResultNum=ReplicationServers.GetKey("medlabresult","MedLabResultNum");
			}
			string command="INSERT INTO medlabresult (";
			if(useExistingPK || PrefC.RandomKeys) {
				command+="MedLabResultNum,";
			}
			command+="MedLabNum,ObsID,ObsText,ObsLoinc,ObsLoincText,ObsIDSub,ObsValue,ObsSubType,ObsUnits,ReferenceRange,AbnormalFlag,ResultStatus,DateTimeObs,FacilityID,DocNum,Note) VALUES(";
			if(useExistingPK || PrefC.RandomKeys) {
				command+=POut.Long(medLabResult.MedLabResultNum)+",";
			}
			command+=
				     POut.Long  (medLabResult.MedLabNum)+","
				+"'"+POut.String(medLabResult.ObsID)+"',"
				+"'"+POut.String(medLabResult.ObsText)+"',"
				+"'"+POut.String(medLabResult.ObsLoinc)+"',"
				+"'"+POut.String(medLabResult.ObsLoincText)+"',"
				+"'"+POut.String(medLabResult.ObsIDSub)+"',"
				+    DbHelper.ParamChar+"paramObsValue,"
				+"'"+POut.String(medLabResult.ObsSubType.ToString())+"',"
				+"'"+POut.String(medLabResult.ObsUnits)+"',"
				+"'"+POut.String(medLabResult.ReferenceRange)+"',"
				+"'"+POut.String(medLabResult.AbnormalFlag.ToString())+"',"
				+"'"+POut.String(medLabResult.ResultStatus.ToString())+"',"
				+    POut.DateT (medLabResult.DateTimeObs)+","
				+"'"+POut.String(medLabResult.FacilityID)+"',"
				+    POut.Long  (medLabResult.DocNum)+","
				+    DbHelper.ParamChar+"paramNote)";
			if(medLabResult.ObsValue==null) {
				medLabResult.ObsValue="";
			}
			OdSqlParameter paramObsValue=new OdSqlParameter("paramObsValue",OdDbType.Text,medLabResult.ObsValue);
			if(medLabResult.Note==null) {
				medLabResult.Note="";
			}
			OdSqlParameter paramNote=new OdSqlParameter("paramNote",OdDbType.Text,medLabResult.Note);
			if(useExistingPK || PrefC.RandomKeys) {
				Db.NonQ(command,paramObsValue,paramNote);
			}
			else {
				medLabResult.MedLabResultNum=Db.NonQ(command,true,paramObsValue,paramNote);
			}
			return medLabResult.MedLabResultNum;
		}

		///<summary>Inserts one MedLabResult into the database.  Returns the new priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(MedLabResult medLabResult){
			if(DataConnection.DBtype==DatabaseType.MySql) {
				return InsertNoCache(medLabResult,false);
			}
			else {
				if(DataConnection.DBtype==DatabaseType.Oracle) {
					medLabResult.MedLabResultNum=DbHelper.GetNextOracleKey("medlabresult","MedLabResultNum"); //Cacheless method
				}
				return InsertNoCache(medLabResult,true);
			}
		}

		///<summary>Inserts one MedLabResult into the database.  Provides option to use the existing priKey.  Doesn't use the cache.</summary>
		public static long InsertNoCache(MedLabResult medLabResult,bool useExistingPK){
			bool isRandomKeys=Prefs.GetBoolNoCache(PrefName.RandomPrimaryKeys);
			string command="INSERT INTO medlabresult (";
			if(!useExistingPK && isRandomKeys) {
				medLabResult.MedLabResultNum=ReplicationServers.GetKeyNoCache("medlabresult","MedLabResultNum");
			}
			if(isRandomKeys || useExistingPK) {
				command+="MedLabResultNum,";
			}
			command+="MedLabNum,ObsID,ObsText,ObsLoinc,ObsLoincText,ObsIDSub,ObsValue,ObsSubType,ObsUnits,ReferenceRange,AbnormalFlag,ResultStatus,DateTimeObs,FacilityID,DocNum,Note) VALUES(";
			if(isRandomKeys || useExistingPK) {
				command+=POut.Long(medLabResult.MedLabResultNum)+",";
			}
			command+=
				     POut.Long  (medLabResult.MedLabNum)+","
				+"'"+POut.String(medLabResult.ObsID)+"',"
				+"'"+POut.String(medLabResult.ObsText)+"',"
				+"'"+POut.String(medLabResult.ObsLoinc)+"',"
				+"'"+POut.String(medLabResult.ObsLoincText)+"',"
				+"'"+POut.String(medLabResult.ObsIDSub)+"',"
				+    DbHelper.ParamChar+"paramObsValue,"
				+"'"+POut.String(medLabResult.ObsSubType.ToString())+"',"
				+"'"+POut.String(medLabResult.ObsUnits)+"',"
				+"'"+POut.String(medLabResult.ReferenceRange)+"',"
				+"'"+POut.String(medLabResult.AbnormalFlag.ToString())+"',"
				+"'"+POut.String(medLabResult.ResultStatus.ToString())+"',"
				+    POut.DateT (medLabResult.DateTimeObs)+","
				+"'"+POut.String(medLabResult.FacilityID)+"',"
				+    POut.Long  (medLabResult.DocNum)+","
				+    DbHelper.ParamChar+"paramNote)";
			if(medLabResult.ObsValue==null) {
				medLabResult.ObsValue="";
			}
			OdSqlParameter paramObsValue=new OdSqlParameter("paramObsValue",OdDbType.Text,medLabResult.ObsValue);
			if(medLabResult.Note==null) {
				medLabResult.Note="";
			}
			OdSqlParameter paramNote=new OdSqlParameter("paramNote",OdDbType.Text,medLabResult.Note);
			if(useExistingPK || isRandomKeys) {
				Db.NonQ(command,paramObsValue,paramNote);
			}
			else {
				medLabResult.MedLabResultNum=Db.NonQ(command,true,paramObsValue,paramNote);
			}
			return medLabResult.MedLabResultNum;
		}

		///<summary>Updates one MedLabResult in the database.</summary>
		public static void Update(MedLabResult medLabResult){
			string command="UPDATE medlabresult SET "
				+"MedLabNum      =  "+POut.Long  (medLabResult.MedLabNum)+", "
				+"ObsID          = '"+POut.String(medLabResult.ObsID)+"', "
				+"ObsText        = '"+POut.String(medLabResult.ObsText)+"', "
				+"ObsLoinc       = '"+POut.String(medLabResult.ObsLoinc)+"', "
				+"ObsLoincText   = '"+POut.String(medLabResult.ObsLoincText)+"', "
				+"ObsIDSub       = '"+POut.String(medLabResult.ObsIDSub)+"', "
				+"ObsValue       =  "+DbHelper.ParamChar+"paramObsValue, "
				+"ObsSubType     = '"+POut.String(medLabResult.ObsSubType.ToString())+"', "
				+"ObsUnits       = '"+POut.String(medLabResult.ObsUnits)+"', "
				+"ReferenceRange = '"+POut.String(medLabResult.ReferenceRange)+"', "
				+"AbnormalFlag   = '"+POut.String(medLabResult.AbnormalFlag.ToString())+"', "
				+"ResultStatus   = '"+POut.String(medLabResult.ResultStatus.ToString())+"', "
				+"DateTimeObs    =  "+POut.DateT (medLabResult.DateTimeObs)+", "
				+"FacilityID     = '"+POut.String(medLabResult.FacilityID)+"', "
				+"DocNum         =  "+POut.Long  (medLabResult.DocNum)+", "
				+"Note           =  "+DbHelper.ParamChar+"paramNote "
				+"WHERE MedLabResultNum = "+POut.Long(medLabResult.MedLabResultNum);
			if(medLabResult.ObsValue==null) {
				medLabResult.ObsValue="";
			}
			OdSqlParameter paramObsValue=new OdSqlParameter("paramObsValue",OdDbType.Text,medLabResult.ObsValue);
			if(medLabResult.Note==null) {
				medLabResult.Note="";
			}
			OdSqlParameter paramNote=new OdSqlParameter("paramNote",OdDbType.Text,medLabResult.Note);
			Db.NonQ(command,paramObsValue,paramNote);
		}

		///<summary>Updates one MedLabResult in the database.  Uses an old object to compare to, and only alters changed fields.  This prevents collisions and concurrency problems in heavily used tables.  Returns true if an update occurred.</summary>
		public static bool Update(MedLabResult medLabResult,MedLabResult oldMedLabResult){
			string command="";
			if(medLabResult.MedLabNum != oldMedLabResult.MedLabNum) {
				if(command!=""){ command+=",";}
				command+="MedLabNum = "+POut.Long(medLabResult.MedLabNum)+"";
			}
			if(medLabResult.ObsID != oldMedLabResult.ObsID) {
				if(command!=""){ command+=",";}
				command+="ObsID = '"+POut.String(medLabResult.ObsID)+"'";
			}
			if(medLabResult.ObsText != oldMedLabResult.ObsText) {
				if(command!=""){ command+=",";}
				command+="ObsText = '"+POut.String(medLabResult.ObsText)+"'";
			}
			if(medLabResult.ObsLoinc != oldMedLabResult.ObsLoinc) {
				if(command!=""){ command+=",";}
				command+="ObsLoinc = '"+POut.String(medLabResult.ObsLoinc)+"'";
			}
			if(medLabResult.ObsLoincText != oldMedLabResult.ObsLoincText) {
				if(command!=""){ command+=",";}
				command+="ObsLoincText = '"+POut.String(medLabResult.ObsLoincText)+"'";
			}
			if(medLabResult.ObsIDSub != oldMedLabResult.ObsIDSub) {
				if(command!=""){ command+=",";}
				command+="ObsIDSub = '"+POut.String(medLabResult.ObsIDSub)+"'";
			}
			if(medLabResult.ObsValue != oldMedLabResult.ObsValue) {
				if(command!=""){ command+=",";}
				command+="ObsValue = "+DbHelper.ParamChar+"paramObsValue";
			}
			if(medLabResult.ObsSubType != oldMedLabResult.ObsSubType) {
				if(command!=""){ command+=",";}
				command+="ObsSubType = '"+POut.String(medLabResult.ObsSubType.ToString())+"'";
			}
			if(medLabResult.ObsUnits != oldMedLabResult.ObsUnits) {
				if(command!=""){ command+=",";}
				command+="ObsUnits = '"+POut.String(medLabResult.ObsUnits)+"'";
			}
			if(medLabResult.ReferenceRange != oldMedLabResult.ReferenceRange) {
				if(command!=""){ command+=",";}
				command+="ReferenceRange = '"+POut.String(medLabResult.ReferenceRange)+"'";
			}
			if(medLabResult.AbnormalFlag != oldMedLabResult.AbnormalFlag) {
				if(command!=""){ command+=",";}
				command+="AbnormalFlag = '"+POut.String(medLabResult.AbnormalFlag.ToString())+"'";
			}
			if(medLabResult.ResultStatus != oldMedLabResult.ResultStatus) {
				if(command!=""){ command+=",";}
				command+="ResultStatus = '"+POut.String(medLabResult.ResultStatus.ToString())+"'";
			}
			if(medLabResult.DateTimeObs != oldMedLabResult.DateTimeObs) {
				if(command!=""){ command+=",";}
				command+="DateTimeObs = "+POut.DateT(medLabResult.DateTimeObs)+"";
			}
			if(medLabResult.FacilityID != oldMedLabResult.FacilityID) {
				if(command!=""){ command+=",";}
				command+="FacilityID = '"+POut.String(medLabResult.FacilityID)+"'";
			}
			if(medLabResult.DocNum != oldMedLabResult.DocNum) {
				if(command!=""){ command+=",";}
				command+="DocNum = "+POut.Long(medLabResult.DocNum)+"";
			}
			if(medLabResult.Note != oldMedLabResult.Note) {
				if(command!=""){ command+=",";}
				command+="Note = "+DbHelper.ParamChar+"paramNote";
			}
			if(command==""){
				return false;
			}
			if(medLabResult.ObsValue==null) {
				medLabResult.ObsValue="";
			}
			OdSqlParameter paramObsValue=new OdSqlParameter("paramObsValue",OdDbType.Text,medLabResult.ObsValue);
			if(medLabResult.Note==null) {
				medLabResult.Note="";
			}
			OdSqlParameter paramNote=new OdSqlParameter("paramNote",OdDbType.Text,medLabResult.Note);
			command="UPDATE medlabresult SET "+command
				+" WHERE MedLabResultNum = "+POut.Long(medLabResult.MedLabResultNum);
			Db.NonQ(command,paramObsValue,paramNote);
			return true;
		}

		///<summary>Returns true if Update(MedLabResult,MedLabResult) would make changes to the database.
		///Does not make any changes to the database and can be called before remoting role is checked.</summary>
		public static bool UpdateComparison(MedLabResult medLabResult,MedLabResult oldMedLabResult) {
			if(medLabResult.MedLabNum != oldMedLabResult.MedLabNum) {
				return true;
			}
			if(medLabResult.ObsID != oldMedLabResult.ObsID) {
				return true;
			}
			if(medLabResult.ObsText != oldMedLabResult.ObsText) {
				return true;
			}
			if(medLabResult.ObsLoinc != oldMedLabResult.ObsLoinc) {
				return true;
			}
			if(medLabResult.ObsLoincText != oldMedLabResult.ObsLoincText) {
				return true;
			}
			if(medLabResult.ObsIDSub != oldMedLabResult.ObsIDSub) {
				return true;
			}
			if(medLabResult.ObsValue != oldMedLabResult.ObsValue) {
				return true;
			}
			if(medLabResult.ObsSubType != oldMedLabResult.ObsSubType) {
				return true;
			}
			if(medLabResult.ObsUnits != oldMedLabResult.ObsUnits) {
				return true;
			}
			if(medLabResult.ReferenceRange != oldMedLabResult.ReferenceRange) {
				return true;
			}
			if(medLabResult.AbnormalFlag != oldMedLabResult.AbnormalFlag) {
				return true;
			}
			if(medLabResult.ResultStatus != oldMedLabResult.ResultStatus) {
				return true;
			}
			if(medLabResult.DateTimeObs != oldMedLabResult.DateTimeObs) {
				return true;
			}
			if(medLabResult.FacilityID != oldMedLabResult.FacilityID) {
				return true;
			}
			if(medLabResult.DocNum != oldMedLabResult.DocNum) {
				return true;
			}
			if(medLabResult.Note != oldMedLabResult.Note) {
				return true;
			}
			return false;
		}

		///<summary>Deletes one MedLabResult from the database.</summary>
		public static void Delete(long medLabResultNum){
			string command="DELETE FROM medlabresult "
				+"WHERE MedLabResultNum = "+POut.Long(medLabResultNum);
			Db.NonQ(command);
		}

	}
}