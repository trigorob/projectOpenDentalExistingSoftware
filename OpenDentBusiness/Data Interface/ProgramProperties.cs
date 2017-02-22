using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CodeBase;
using System.Text.RegularExpressions;

namespace OpenDentBusiness {

	///<summary></summary>
	public class ProgramProperties{
		///<summary></summary>
		public static DataTable RefreshCache() {
			//No need to check RemotingRole; Calls GetTableRemotelyIfNeeded().
			string command="SELECT * FROM programproperty";
			DataTable table=Cache.GetTableRemotelyIfNeeded(MethodBase.GetCurrentMethod(),command);
			table.TableName="ProgramProperty";
			FillCache(table);
			return table;
		}
	
		///<summary></summary>
		public static void FillCache(DataTable table) {
			//No need to check RemotingRole; no call to db.
			List<ProgramProperty> listProgramProperties=Crud.ProgramPropertyCrud.TableToList(table);
			//This is where code should go if there is ever a short list implemented for program properties.
			ProgramPropertyC.Listt=listProgramProperties;
		}

		///<summary></summary>
		public static void Update(ProgramProperty programProp){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),programProp);
				return;
			}
			Crud.ProgramPropertyCrud.Update(programProp);
		}

		///<summary>Returns true if the program property was updated.  False if no change needed.  Callers need to invalidate cache as needed.</summary>
		public static bool UpdateProgramPropertyWithValue(ProgramProperty programProp,string newValue) {
			//No need to check RemotingRole; no call to db.
			if(programProp.PropertyValue==newValue) {
				return false;
			}
			programProp.PropertyValue=newValue;
			ProgramProperties.Update(programProp);
			return true;
		}

		///<summary>This is called from FormClinicEdit and from InsertOrUpdateLocalOverridePath.  PayConnect can have clinic specific login credentials,
		///so the ProgramProperties for PayConnect are duplicated for each clinic.  The properties duplicated are Username, Password, and PaymentType.
		///There's also a 'Headquarters' or no clinic set of these props with ClinicNum 0, which is the set of props inserted with each new clinic.</summary>
		public static long Insert(ProgramProperty programProp){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				programProp.ProgramPropertyNum=Meth.GetLong(MethodBase.GetCurrentMethod(),programProp);
				return programProp.ProgramPropertyNum;
			}
			return Crud.ProgramPropertyCrud.Insert(programProp);
		}
		
		///<summary>Copies rows for a given programNum for each clinic in listClinicNums.  Returns true if changes were made to the db.</summary>
		public static bool InsertForClinic(long programNum,List<long> listClinicNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod(),programNum,listClinicNums);
			}
			if(listClinicNums==null || listClinicNums.Count==0) {
				return false;
			}
			bool hasInsert=false;
			string command="";
			if(DataConnection.DBtype==DatabaseType.MySql) {
				command="INSERT INTO programproperty (ProgramNum,PropertyDesc,PropertyValue,ComputerName,ClinicNum) ";
				for(int i=0;i<listClinicNums.Count;i++) {
					if(i>0) {
						command+=" UNION ";
					}
					command+="SELECT ProgramNum,PropertyDesc,PropertyValue,ComputerName,"+POut.Long(listClinicNums[i])+" "
						+"FROM programproperty "
						+"WHERE ProgramNum="+POut.Long(programNum)+" "
						+"AND ClinicNum=0";
				}
				hasInsert=(Db.NonQ(command) > 0);
			}
			else {//Oracle
				command="SELECT ProgramNum,PropertyDesc,PropertyValue,ComputerName "
					+"FROM programproperty "
					+"WHERE ProgramNum="+POut.Long(programNum)+" "
					+"AND ClinicNum=0";
				DataTable tableProgProps=Db.GetTable(command);
				//Loop through all program properties for this program.
				foreach(DataRow row in tableProgProps.Rows) {
					//Insert this program property for every single clinic passed in.
					foreach(long clinicNum in listClinicNums) {
						command="INSERT INTO programproperty (ProgramPropertyNum,ProgramNum,PropertyDesc,PropertyValue,ComputerName,ClinicNum) "
							+"VALUES("
							+"(SELECT COALESCE(MAX(ProgramPropertyNum),0)+1 FROM programproperty),"
							+row["ProgramNum"].ToString()+","
							+"'"+row["PropertyDesc"].ToString()+"',"
							+"'"+row["PropertyValue"].ToString()+"',"
							+"'"+row["ComputerName"].ToString()+"',"
							+POut.Long(clinicNum)+")";
						Db.NonQ(command);
						hasInsert=true;
					}
				}
			}
			return hasInsert;
		}

		///<summary>Safe to call on any program. Only returns true if the program is not enabled AND the program has a property of "Disable Advertising" = 1.</summary>
		public static bool IsAdvertisingDisabled(ProgramName progName) {
			Program program = Programs.GetCur(progName);
			if(program==null || program.Enabled) {
				return false;//do not block advertising
			}
			return GetListForProgram(program.ProgramNum).Any(x => x.PropertyDesc=="Disable Advertising" && x.PropertyValue=="1");
		}

		///<summary>Returns a List of ProgramProperties attached to the specified programNum.  Does not include path overrides.</summary>
		public static List<ProgramProperty> GetListForProgram(long programNum) {
			//No need to check RemotingRole; no call to db.
			List<ProgramProperty> listProgPropsResult=new List<ProgramProperty>();
			List<ProgramProperty> listProgPropsCache=ProgramPropertyC.GetListt();
			for(int i=0;i<listProgPropsCache.Count;i++) {
				if(listProgPropsCache[i].ProgramNum==programNum && listProgPropsCache[i].PropertyDesc!="") {
					listProgPropsResult.Add(listProgPropsCache[i]);
				}
			}
			return listProgPropsResult;
		}

		///<summary>Returns a list of ProgramProperties with the specified programNum and the specified clinicNum from the cache.
		///To get properties when clinics are not enabled or properties for 'Headquarters' use clinicNum 0.
		///Does not include path overrides.</summary>
		public static List<ProgramProperty> GetListForProgramAndClinic(long programNum,long clinicNum) {
			//No need to check RemotingRole; no call to db.
			return ProgramPropertyC.GetListt().FindAll(x => x.ProgramNum==programNum)
				.FindAll(x => x.ClinicNum==clinicNum)
				.FindAll(x => x.PropertyDesc!="");
		}
		
		///<summary>Returns a List of ProgramProperties attached to the specified programNum with the given clinicnum.  
		///Includes the default program properties as well (ClinicNum==0).</summary>
		public static List<ProgramProperty> GetListForProgramAndClinicWithDefault(long programNum,long clinicNum) {
			//No need to check RemotingRole; no call to db.
			List<ProgramProperty> listProperties=ProgramPropertyC.GetListt();
			List<ProgramProperty> listClinicProperties=listProperties.FindAll(x => x.ProgramNum==programNum && x.ClinicNum==clinicNum);
			if(clinicNum==0) {
				return listClinicProperties;//return the defaults cause ClinicNum of 0 is default.
			}
			//Get all the defaults and return a list of defaults mixed with overrides.
			List<ProgramProperty> listClinicAndDefaultProperties=listProperties.FindAll(x => x.ProgramNum==programNum 
				&& !listClinicProperties.Any(y => y.PropertyDesc==x.PropertyDesc));
			listClinicAndDefaultProperties.AddRange(listClinicProperties);
			return listClinicAndDefaultProperties;//Clinic users need to have all properties, defaults with the clinic overrides.
		}

		///<summary>Returns the property value of the clinic override or default program property if no clinic override is found.</summary>
		public static string GetPropValForClinicOrDefault(long programNum,string desc,long clinicNum) {
			//No need to check RemotingRole; no call to db.
			return GetListForProgramAndClinicWithDefault(programNum,clinicNum).FirstOrDefault(x => x.PropertyDesc==desc).PropertyValue;
		}

		///<summary>Returns an ArrayList of ProgramProperties attached to the specified programNum.  Does not include path overrides.
		///Uses thread-safe caching pattern.  Each call to this method creates an copy of the entire ProgramProperty cache.</summary>
		public static List<ProgramProperty> GetForProgram(long programNum) {
			//No need to check RemotingRole; no call to db.
			return ProgramPropertyC.GetListt().FindAll(x => x.ProgramNum==programNum && x.PropertyDesc!="");
		}

		public static void SetProperty(long programNum,string desc,string propval) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),programNum,desc,propval);
				return;
			}
			string command="UPDATE programproperty SET PropertyValue='"+POut.String(propval)+"' "
				+"WHERE ProgramNum="+POut.Long(programNum)+" "
				+"AND PropertyDesc='"+POut.String(desc)+"'";
			Db.NonQ(command);
		}

		///<summary>After GetForProgram has been run, this gets one of those properties.  DO NOT MODIFY the returned property.  Read only.</summary>
		public static ProgramProperty GetCur(List<ProgramProperty> arrayForProgram,string desc) {
			//No need to check RemotingRole; no call to db.
			return arrayForProgram.FirstOrDefault(x => x.PropertyDesc==desc);
		}

		public static string GetPropVal(long programNum,string desc) {
			//No need to check RemotingRole; no call to db.
			List<ProgramProperty> listProgramProperties=ProgramPropertyC.GetListt();
			for(int i=0;i<listProgramProperties.Count;i++) {
				if(listProgramProperties[i].ProgramNum!=programNum) {
					continue;
				}
				if(listProgramProperties[i].PropertyDesc!=desc) {
					continue;
				}
				return listProgramProperties[i].PropertyValue;
			}
			throw new ApplicationException("Property not found: "+desc);
		}

		public static string GetPropVal(ProgramName programName,string desc) {
			//No need to check RemotingRole; no call to db.
			long programNum=Programs.GetProgramNum(programName);
			return GetPropVal(programNum,desc);
		}

		///<summary>Returns the PropertyVal for programNum and clinicNum specified with the description specified.  If the property doesn't exist,
		///returns an empty string.  For the PropertyVal for 'Headquarters' or clincs not enabled, use clinicNum 0.</summary>
		public static string GetPropVal(long programNum,string desc,long clinicNum) {
			return GetPropValFromList(ProgramPropertyC.GetListt().FindAll(x => x.ProgramNum==programNum),desc,clinicNum);
		}

		///<summary>Returns the PropertyVal from the list by PropertyDesc and ClinicNum.
		///For the 'Headquarters' or for clinics not enabled, omit clinicNum or send clinicNum 0.  If not found returns an empty string.
		///Primarily used when a local list has been copied from the cache and may differ from what's in the database.  Also possibly useful if dealing with a filtered list </summary>
		public static string GetPropValFromList(List<ProgramProperty> listProps,string propertyDesc,long clinicNum=0) {
			string retval="";
			ProgramProperty prop=listProps.Where(x => x.ClinicNum==clinicNum).Where(x => x.PropertyDesc==propertyDesc).FirstOrDefault();
			if(prop!=null) {
				retval=prop.PropertyValue;
			}
			return retval;
		}

		///<summary>Returns the property with the matching description from the provided list.  Null if the property cannot be found by the description.</summary>
		public static ProgramProperty GetPropByDesc(string propertyDesc,List<ProgramProperty> listProperties) {
			//No need to check RemotingRole; no call to db.
			ProgramProperty property=null;
			for(int i=0;i<listProperties.Count;i++) {
				if(listProperties[i].PropertyDesc==propertyDesc) {
					property=listProperties[i];
					break;
				}
			}
			return property;
		}

		///<summary>Used in FormUAppoint to get frequent and current data.</summary>
		public static string GetValFromDb(long programNum,string desc) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetString(MethodBase.GetCurrentMethod(),programNum,desc);
			}
			string command="SELECT PropertyValue FROM programproperty WHERE ProgramNum="+POut.Long(programNum)
				+" AND PropertyDesc='"+POut.String(desc)+"'";
			DataTable table=Db.GetTable(command);
			if(table.Rows.Count==0){
				return "";
			}
			return table.Rows[0][0].ToString();
		}

		///<summary>Returns the path override for the current computer and the specified programNum.  Returns empty string if no override found.</summary>
		public static string GetLocalPathOverrideForProgram(long programNum) {
			//No need to check RemotingRole; no call to db.
			List<ProgramProperty> listProgramProperties=ProgramPropertyC.GetListt();
			for(int i=0;i<listProgramProperties.Count;i++) {
				if(listProgramProperties[i].ProgramNum==programNum
					&& listProgramProperties[i].PropertyDesc==""
					&& listProgramProperties[i].ComputerName.ToUpper()==Environment.MachineName.ToUpper()) 
				{
					return listProgramProperties[i].PropertyValue;
				}
			}
			return "";
		}

		///<summary>This will insert or update a local path override property for the specified programNum.</summary>
		public static void InsertOrUpdateLocalOverridePath(long programNum,string newPath) {
			//No need to check RemotingRole; no call to db.
			List<ProgramProperty> listProgramProperties=ProgramPropertyC.GetListt();
			for(int i=0;i<listProgramProperties.Count;i++) {
				if(listProgramProperties[i].ProgramNum==programNum
					&& listProgramProperties[i].PropertyDesc==""
					&& listProgramProperties[i].ComputerName.ToUpper()==Environment.MachineName.ToUpper()) 
				{
					listProgramProperties[i].PropertyValue=newPath;
					ProgramProperties.Update(listProgramProperties[i]);
					return;//Will only be one override per computer per program.
				}
			}
			//Path override does not exist for the current computer so create a new one.
			ProgramProperty pp=new ProgramProperty();
			pp.ProgramNum=programNum;
			pp.PropertyValue=newPath;
			pp.ComputerName=Environment.MachineName.ToUpper();
			ProgramProperties.Insert(pp);
		}

		///<summary>Syncs list against cache copy of program properties.  listProgPropsNew should never include local path overrides (PropertyDesc=="").
		///This sync uses the cache copy of program properties rather than a stale list because we want to make sure we never have duplicate properties
		///and concurrency isn't really an issue.</summary>
		public static void Sync(List<ProgramProperty> listProgPropsNew,long programNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),listProgPropsNew,programNum);
				return;
			}
			//prevents delete of program properties for clinics added while editing program properties.
			List<long> listClinicNums = listProgPropsNew.Select(x => x.ClinicNum).Distinct().ToList();
			List<ProgramProperty> listProgPropsDb=ProgramPropertyC.GetListt().FindAll(x => x.ProgramNum==programNum && x.PropertyDesc!="" && listClinicNums.Contains(x.ClinicNum));
			Crud.ProgramPropertyCrud.Sync(listProgPropsNew,listProgPropsDb);
		}

		///<summary>Exception means failed. Return means success. paymentsAllowed should be check after return. If false then assume payments cannot be made for this clinic.</summary>
		public static void GetXWebCreds(long clinicNum,out bool paymentsAllowed,out string xWebID,out string authKey,out string terminalID,out long paymentTypeDefNum) {
			//No need to check RemotingRole;no call to db.
			//Secure arguments are held in the db.
			OpenDentBusiness.Program programXcharge=OpenDentBusiness.Programs.GetCur(OpenDentBusiness.ProgramName.Xcharge);
			if(programXcharge==null) { //XCharge not setup.
				throw new ODException("X-Charge program link not found.",ODException.ErrorCodes.XWebProgramProperties);
			}
			if(!programXcharge.Enabled) { //XCharge not turned on.
				throw new ODException("X-Charge program link is disabled.",ODException.ErrorCodes.XWebProgramProperties);
			}
			//Validate ALL XWebID, AuthKey, and TerminalID.  Each is required for X-Web to work.
			List<OpenDentBusiness.ProgramProperty> listXchargeProperties=OpenDentBusiness.ProgramProperties.GetListForProgramAndClinic(programXcharge.ProgramNum,clinicNum);
			xWebID=OpenDentBusiness.ProgramProperties.GetPropValFromList(listXchargeProperties,"XWebID",clinicNum);
			authKey=OpenDentBusiness.ProgramProperties.GetPropValFromList(listXchargeProperties,"AuthKey",clinicNum);
			terminalID=OpenDentBusiness.ProgramProperties.GetPropValFromList(listXchargeProperties,"TerminalID",clinicNum);
			string paymentTypeDefString=OpenDentBusiness.ProgramProperties.GetPropValFromList(listXchargeProperties,"PaymentType",clinicNum);
			if(string.IsNullOrEmpty(xWebID)||string.IsNullOrEmpty(authKey)||string.IsNullOrEmpty(terminalID)||!long.TryParse(paymentTypeDefString,out paymentTypeDefNum)) {
				throw new ODException("X-Web program properties not found.",ODException.ErrorCodes.XWebProgramProperties);
			}
			//XWeb ID must be 12 digits, Auth Key 32 alphanumeric characters, and Terminal ID 8 digits.
			if(!Regex.IsMatch(xWebID,"^[0-9]{12}$")
				||!Regex.IsMatch(authKey,"^[A-Za-z0-9]{32}$")
				||!Regex.IsMatch(terminalID,"^[0-9]{8}$")) {
				throw new ODException("X-Web program properties not valid.",ODException.ErrorCodes.XWebProgramProperties);
			}
			string asString=OpenDentBusiness.ProgramProperties.GetPropValFromList(listXchargeProperties,"IsOnlinePaymentsEnabled",clinicNum);
			paymentsAllowed=OpenDentBusiness.PIn.Bool(asString);
		}
	}
}










