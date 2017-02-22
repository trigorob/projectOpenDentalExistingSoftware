using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Linq;

namespace OpenDentBusiness{
	///<summary></summary>
	public class InsVerifies {

		///<summary>Gets one InsVerify from the db.</summary>
		public static InsVerify GetOne(long insVerifyNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<InsVerify>(MethodBase.GetCurrentMethod(),insVerifyNum);
			}
			return Crud.InsVerifyCrud.SelectOne(insVerifyNum);
		}
		
		///<summary>Gets one InsVerify from the db that has the given fkey and verify type.</summary>
		public static InsVerify GetOneByFKey(long fkey,VerifyTypes verifyType) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<InsVerify>(MethodBase.GetCurrentMethod(),fkey,verifyType);
			}
			string command="SELECT * FROM insverify WHERE FKey="+POut.Long(fkey)+" AND VerifyType="+POut.Int((int)verifyType)+"";
			return Crud.InsVerifyCrud.SelectOne(command);
		}
		
		///<summary>Gets one InsVerifyNum from the db that has the given fkey and verify type.</summary>
		public static long GetInsVerifyNumByFKey(long fkey,VerifyTypes verifyType) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetLong(MethodBase.GetCurrentMethod(),fkey,verifyType);
			}
			string command="SELECT * FROM insverify WHERE FKey="+POut.Long(fkey)+" AND VerifyType="+POut.Int((int)verifyType)+"";
			InsVerify insVerify=Crud.InsVerifyCrud.SelectOne(command);
			if(insVerify==null) {
				return 0;
			}
			return insVerify.InsVerifyNum;
		}

		///<summary></summary>
		public static long Insert(InsVerify insVerify) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				insVerify.InsVerifyNum=Meth.GetLong(MethodBase.GetCurrentMethod(),insVerify);
				return insVerify.InsVerifyNum;
			}
			return Crud.InsVerifyCrud.Insert(insVerify);
		}

		///<summary></summary>
		public static void Update(InsVerify insVerify) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),insVerify);
				return;
			}
			Crud.InsVerifyCrud.Update(insVerify);
		}

		///<summary></summary>
		public static void Delete(long insVerifyNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),insVerifyNum);
				return;
			}
			Crud.InsVerifyCrud.Delete(insVerifyNum);
		}
		
		///<summary>Inserts a default InsVerify into the database based on the passed in patplan.  Used when inserting a new patplan.
		///Returns the primary key of the new InsVerify.</summary>
		public static long InsertForPatPlanNum(long patPlanNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetLong(MethodBase.GetCurrentMethod(),patPlanNum);
			}
			InsVerify insVerify=new InsVerify();
			insVerify.VerifyType=VerifyTypes.PatientEnrollment;
			insVerify.FKey=patPlanNum;
			return Crud.InsVerifyCrud.Insert(insVerify);
		}
		
		///<summary>Inserts a default InsVerify into the database based on the passed in insplan.  Used when inserting a new insplan.
		///Returns the primary key of the new InsVerify.</summary>
		public static long InsertForPlanNum(long planNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetLong(MethodBase.GetCurrentMethod(),planNum);
			}
			InsVerify insVerify=new InsVerify();
			insVerify.VerifyType=VerifyTypes.InsuranceBenefit;
			insVerify.FKey=planNum;
			return Crud.InsVerifyCrud.Insert(insVerify);
		}
		
		///<summary>Deletes an InsVerify with the passed in FKey and VerifyType.</summary>
		public static void DeleteByFKey(long fkey,VerifyTypes verifyType) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetLong(MethodBase.GetCurrentMethod(),fkey,verifyType);
				return;
			}
			long insVerifyNum=GetInsVerifyNumByFKey(fkey,verifyType);
			Crud.InsVerifyCrud.Delete(insVerifyNum);//Will do nothing if insVerifyNum was 0.
		}

		public static List<InsVerify> GetAll() {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<InsVerify>>(MethodBase.GetCurrentMethod());
			}
			string command="SELECT * FROM insverify";
			return Crud.InsVerifyCrud.SelectMany(command);
    }

		public static List<long> GetAllInsVerifyUserNums() {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<long>>(MethodBase.GetCurrentMethod());
			}
			string command="SELECT DISTINCT UserNum FROM insverify";
			return Db.GetListLong(command);
		}
		
		///<summary>UserNum=-1 is "All", UserNum=0 is "Unassigned". statusDefNum=-1 or statusDefNum=0 is "All".  ClinicNum=-1 is "All". ClinicNum=0 is "Unassigned". regionDefNum=0 or regionDefNum=-1 is "All".  </summary>
		public static List<InsVerifyGridObject> GetVerifyGridList(DateTime startDate, DateTime endDate,DateTime datePatEligibilityLastVerified,DateTime datePlanBenefitsLastVerified,long clinicNum,long regionDefNum,long statusDefNum,long userNum,string carrierName,bool excludePatVerifyWhenNoIns,bool excludePatClones) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<InsVerifyGridObject>>(MethodBase.GetCurrentMethod(),startDate,endDate,datePatEligibilityLastVerified,datePlanBenefitsLastVerified,clinicNum,regionDefNum,statusDefNum,userNum,carrierName,excludePatVerifyWhenNoIns,excludePatClones);
			}
			//clinicJoin should only be used if the passed in clinicNum is a value other than 0 (Unassigned).
			string whereClinic="";
			if(clinicNum==-1) {//All clinics
				whereClinic="AND (clinic.IsInsVerifyExcluded=0 OR clinic.ClinicNum IS NULL) ";
				if(regionDefNum>0) {//Specific region
					whereClinic+=" AND clinic.Region="+POut.Long(regionDefNum)+" ";
				}
			}
			else if(clinicNum==0) {//Unassigned clinics
				whereClinic="AND clinic.ClinicNum IS NULL ";
			}
			else {//Specific Clinic
				whereClinic="AND clinic.IsInsVerifyExcluded=0 AND clinic.ClinicNum="+POut.Long(clinicNum)+" ";
				if(regionDefNum>0) {//Specific region
					whereClinic+=" AND clinic.Region="+POut.Long(regionDefNum)+" ";
				}
			}
			string command=@"
				SELECT insverify.*,
				patient.LName,patient.FName,patient.Preferred,appointment.PatNum,appointment.AptDateTime,patplan.PatPlanNum,insplan.PlanNum,carrier.CarrierName,
				COALESCE(clinic.Abbr,'None') AS ClinicName
				FROM appointment 
				LEFT JOIN clinic ON clinic.ClinicNum=appointment.ClinicNum 
				INNER JOIN patient ON patient.PatNum=appointment.PatNum 
					"+(excludePatClones ? "AND UPPER(patient.LName)!=patient.LName AND UPPER(patient.FName)!=patient.FName" : "")+@" 
				INNER JOIN patplan ON patplan.PatNum=appointment.PatNum 
				INNER JOIN inssub ON inssub.InsSubNum=patplan.InsSubNum 
				INNER JOIN insplan ON insplan.PlanNum=inssub.PlanNum 
					"+(excludePatVerifyWhenNoIns ? "AND insplan.HideFromVerifyList=0" : "")+@"
				INNER JOIN carrier ON carrier.CarrierNum=insplan.CarrierNum 
					"+(string.IsNullOrEmpty(carrierName) ? "" : "AND carrier.CarrierName LIKE '%"+POut.String(carrierName)+"%'")+@" 
				INNER JOIN insverify ON 
					(insverify.VerifyType="+POut.Int((int)VerifyTypes.InsuranceBenefit)+@" 
					AND insverify.FKey=insplan.PlanNum 
					AND insverify.DateLastVerified<"+POut.Date(datePlanBenefitsLastVerified)+@"
					"+(excludePatVerifyWhenNoIns ? "" : "AND insplan.HideFromVerifyList=0")+@")
					OR 
					(insverify.VerifyType="+POut.Int((int)VerifyTypes.PatientEnrollment)+@"
					AND insverify.FKey=patplan.PatPlanNum
					AND insverify.DateLastVerified<"+POut.Date(datePatEligibilityLastVerified)+@")
				WHERE appointment.AptDateTime BETWEEN "+DbHelper.DtimeToDate(POut.Date(startDate))+" AND "+DbHelper.DtimeToDate(POut.Date(endDate.AddDays(1)))+@" 
				AND appointment.AptStatus IN ("+POut.Int((int)ApptStatus.Scheduled)+","+POut.Int((int)ApptStatus.ASAP)+","+POut.Int((int)ApptStatus.Complete)+@")
				"+(userNum==-1 ? "" : "AND insverify.UserNum="+POut.Long(userNum))+@"
				"+(statusDefNum<1 ? "" : "AND insverify.DefNum="+POut.Long(statusDefNum))+@"
				"+whereClinic;
			DataTable table=Db.GetTable(command);
			List<InsVerify> listInsVerifies=Crud.InsVerifyCrud.TableToList(table);
			List<InsVerifyGridObject> retVal=new List<InsVerifyGridObject>();
			for(int i=0;i<table.Rows.Count;i++) {
				DataRow row=table.Rows[i];
				InsVerify insVerifyCur=listInsVerifies[i].Clone();
				insVerifyCur.PatNum=PIn.Long(row["PatNum"].ToString());
				insVerifyCur.PlanNum=PIn.Long(row["PlanNum"].ToString());
				insVerifyCur.PatPlanNum=PIn.Long(row["PatPlanNum"].ToString());
				insVerifyCur.ClinicName=PIn.String(row["ClinicName"].ToString());
				string patName=PIn.String(row["LName"].ToString())
					+", ";
				if(PIn.String(row["Preferred"].ToString())!="") {
					patName+="'"+PIn.String(row["Preferred"].ToString())+"' ";
				}
				patName+=PIn.String(row["FName"].ToString());
				insVerifyCur.PatientName=patName;
				insVerifyCur.CarrierName=PIn.String(row["CarrierName"].ToString());
				insVerifyCur.AppointmentDateTime=PIn.DateT(row["AptDateTime"].ToString());
				if(insVerifyCur.VerifyType==VerifyTypes.InsuranceBenefit) {
					InsVerifyGridObject gridObjPlanExists=retVal.FirstOrDefault(x => x.PlanInsVerify!=null && x.PlanInsVerify.PlanNum==insVerifyCur.PlanNum);
					if(gridObjPlanExists==null) {
						InsVerifyGridObject gridObjExists=retVal.FirstOrDefault(x => x.PatInsVerify!=null 
							&& x.PatInsVerify.PatPlanNum==insVerifyCur.PatPlanNum 
							&& x.PatInsVerify.PlanNum==insVerifyCur.PlanNum 
							&& x.PatInsVerify.Note==insVerifyCur.Note 
							&& x.PatInsVerify.DefNum==insVerifyCur.DefNum 
							&& x.PlanInsVerify==null);
						if(gridObjExists!=null) {
							gridObjExists.PlanInsVerify=insVerifyCur;
						}
						else {
							retVal.Add(new InsVerifyGridObject(plan:insVerifyCur));
						}
					}
				}
				else if(insVerifyCur.VerifyType==VerifyTypes.PatientEnrollment) {
					InsVerifyGridObject gridObjPatExists=retVal.FirstOrDefault(x => x.PatInsVerify!=null && x.PatInsVerify.PatPlanNum==insVerifyCur.PatPlanNum);
					if(gridObjPatExists==null) {
						InsVerifyGridObject gridObjExists=retVal.FirstOrDefault(x => x.PlanInsVerify!=null 
						&& x.PlanInsVerify.PlanNum==insVerifyCur.PlanNum 
						&& x.PlanInsVerify.Note==insVerifyCur.Note 
						&& x.PlanInsVerify.DefNum==insVerifyCur.DefNum 
						&& x.PatInsVerify==null);
						if(gridObjExists!=null) {
							gridObjExists.PatInsVerify=insVerifyCur;
						}
						else {
							retVal.Add(new InsVerifyGridObject(pat:insVerifyCur));
						}
					}
				}
			}
			return retVal;
		}

		public static void CleanupInsVerifyRows(DateTime startDate, DateTime endDate) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),startDate,endDate);
				return;
			}
			//Nathan OK'd the necessity for a complex update query like this to avoid looping through update statements.  This will be changed to a crud update method sometime in the future.
			string command="";
			List<long> listInsVerifyNums=Db.GetListLong(GetInsVerifyCleanupQuery(startDate,endDate));
			if(listInsVerifyNums.Count==0) {
				return;
			}
			command="UPDATE insverify "
				+"SET insverify.DateLastAssigned='0001-01-01', "
				+"insverify.DefNum=0, "
				+"insverify.Note='', "
				+"insverify.UserNum=0 "
				+"WHERE insverify.InsVerifyNum IN ("+string.Join(",",listInsVerifyNums)+")";
			Db.NonQ(command);
		}

		private static string GetInsVerifyCleanupQuery(DateTime startDate, DateTime endDate) {
			return @"SELECT InsVerifyNum
				FROM (
					SELECT InsVerifyNum,patplan.PatNum
					FROM patplan
					INNER JOIN inssub ON inssub.InsSubNum=patplan.InsSubNum
					INNER JOIN insplan ON insplan.PlanNum=inssub.PlanNum
						AND insplan.HideFromVerifyList=0
					INNER JOIN insverify ON VerifyType="+POut.Int((int)VerifyTypes.InsuranceBenefit)+@"
						AND insverify.FKey=insplan.PlanNum
					WHERE insverify.DateLastAssigned>'0001-01-01'
					AND insverify.DateLastAssigned<"+POut.Date(DateTime.Today.AddDays(-30))+@"
				
					UNION
					
					SELECT InsVerifyNum,patplan.PatNum
					FROM patplan
					INNER JOIN insverify ON VerifyType="+POut.Int((int)VerifyTypes.PatientEnrollment)+@"
						AND insverify.FKey=patplan.PatPlanNum
					WHERE insverify.DateLastAssigned>'0001-01-01'
					AND insverify.DateLastAssigned<"+POut.Date(DateTime.Today.AddDays(-30))+@"
				) insverifies
				LEFT JOIN appointment ON appointment.PatNum=insverifies.PatNum
					AND appointment.AptStatus IN ("+POut.Int((int)ApptStatus.Scheduled)+","+POut.Int((int)ApptStatus.Complete)+","+POut.Int((int)ApptStatus.ASAP)+@")
					AND "+DbHelper.DtimeToDate("appointment.AptDateTime")+" BETWEEN "+POut.Date(startDate)+" AND "+POut.Date(endDate)+@"
				GROUP BY insverifies.InsVerifyNum
				HAVING MAX(appointment.AptNum) IS NULL";
		}

		//If this table type will exist as cached data, uncomment the CachePattern region below and edit.
		/*
		#region CachePattern
		//This region can be eliminated if this is not a table type with cached data.
		//If leaving this region in place, be sure to add RefreshCache and FillCache 
		//to the Cache.cs file with all the other Cache types.

		///<summary>A list of all InsVerifies.</summary>
		private static List<InsVerify> listt;

		///<summary>A list of all InsVerifies.</summary>
		public static List<InsVerify> Listt{
			get {
				if(listt==null) {
					RefreshCache();
				}
				return listt;
			}
			set {
				listt=value;
			}
		}

		///<summary></summary>
		public static DataTable RefreshCache(){
			//No need to check RemotingRole; Calls GetTableRemotelyIfNeeded().
			string command="SELECT * FROM insverify ORDER BY ItemOrder";//stub query probably needs to be changed
			DataTable table=Cache.GetTableRemotelyIfNeeded(MethodBase.GetCurrentMethod(),command);
			table.TableName="InsVerify";
			FillCache(table);
			return table;
		}

		///<summary></summary>
		public static void FillCache(DataTable table){
			//No need to check RemotingRole; no call to db.
			listt=Crud.InsVerifyCrud.TableToList(table);
		}
		#endregion
		*/
		/*
		Only pull out the methods below as you need them.  Otherwise, leave them commented out.

		///<summary></summary>
		public static List<InsVerify> Refresh(long patNum){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<InsVerify>>(MethodBase.GetCurrentMethod(),patNum);
			}
			string command="SELECT * FROM insverify WHERE PatNum = "+POut.Long(patNum);
			return Crud.InsVerifyCrud.SelectMany(command);
		}
		*/
	}
}