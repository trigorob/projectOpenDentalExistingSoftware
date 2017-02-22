using CodeBase;
using OpenDentBusiness.WebTypes.WebSched.TimeSlot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;

namespace OpenDentBusiness {
	
	///<summary></summary>
	public class Recalls {
		private const string WEB_SCHED_SIGN_UP_URL="http://www.patientviewer.com/WebSchedSignUp.html";

		///<summary>http://www.patientviewer.com/WebSchedSignUp.html</summary>
		public static string GetWebSchedPromoURL() {
			//No need to check RemotingRole; no call to db.
			return WEB_SCHED_SIGN_UP_URL;
		}

		///<summary>Gets all recalls for the supplied patients, usually a family or single pat.  Result might have a length of zero.  
		///Each recall will also have the DateScheduled filled by pulling that info from other tables.</summary>
		public static List<Recall> GetList(List<long> patNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Recall>>(MethodBase.GetCurrentMethod(),patNums);
			} 
			string wherePats="";
			for(int i=0;i<patNums.Count;i++){
				if(i!=0){
					wherePats+=" OR ";
				}
				wherePats+="PatNum="+patNums[i].ToString();
			}
			string command="SELECT * FROM recall WHERE "+wherePats;
			return Crud.RecallCrud.SelectMany(command);
		}

		public static List<Recall> GetList(long patNum) {
			//No need to check RemotingRole; no call to db.
			List<long> patNums=new List<long>();
			patNums.Add(patNum);
			return GetList(patNums);
		}

		/// <summary></summary>
		public static List<Recall> GetList(List<Patient> patients){
			//No need to check RemotingRole; no call to db.
			List<long> patNums=new List<long>();
			for(int i=0;i<patients.Count;i++){
				patNums.Add(patients[i].PatNum);
			}
			return GetList(patNums);
		}

		public static Recall GetRecall(long recallNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<Recall>(MethodBase.GetCurrentMethod(),recallNum);
			}
			return Crud.RecallCrud.SelectOne(recallNum);
		}

		///<summary>Will return a recall or null.</summary>
		public static Recall GetRecallProphyOrPerio(long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<Recall>(MethodBase.GetCurrentMethod(),patNum);
			} 
			string command="SELECT * FROM recall WHERE PatNum="+POut.Long(patNum)
				+" AND (RecallTypeNum="+RecallTypes.ProphyType+" OR RecallTypeNum="+RecallTypes.PerioType+")";
			return Crud.RecallCrud.SelectOne(command);
		}

		///<summary>Returns the recall time pattern for the patient and the specific recall passed in.
		///Loops through all recalls passed in and adds any due recall procedures to the time pattern if recallCur is a special recall type.
		///Also, this method will manipulate listProcStrs if any additional procedures are added.</summary>
		public static string GetRecallTimePattern(Recall recallCur,List<Recall> listRecalls,Patient patCur,List<string> listProcStrs) {
			//No need to check RemotingRole; no call to db.  Also, listProcStrs is used like an "out" or "ref" parameter.
			string recallPattern=RecallTypes.GetTimePattern(recallCur.RecallTypeNum);
			//Check the patients birth date in regards to the age they will be when the recall is due.  E.g. if pt's 12th birthday falls after recall date.
			if(RecallTypes.IsSpecialRecallType(recallCur.RecallTypeNum)
				&& patCur.Birthdate.AddYears(PrefC.GetInt(PrefName.RecallAgeAdult)) > ((recallCur.DateDue>DateTime.Today)?recallCur.DateDue:DateTime.Today)) 
			{
				List<RecallType> listRecallTypes=RecallTypeC.GetListt();
				for(int i=0;i<listRecallTypes.Count;i++) {
					if(listRecallTypes[i].RecallTypeNum==RecallTypes.ChildProphyType) {
						List<string> childprocs=RecallTypes.GetProcs(listRecallTypes[i].RecallTypeNum);
						if(childprocs.Count>0) {
							listProcStrs.Clear();
							listProcStrs.AddRange(childprocs);//overrides adult procs.
						}
						string childpattern=RecallTypes.GetTimePattern(listRecallTypes[i].RecallTypeNum);
						if(childpattern!="") {
							recallPattern=childpattern;//overrides adult pattern.
						}
					}
				}
			}
			List<string> listProcPatterns=new List<string>() { recallPattern };
			//Add films------------------------------------------------------------------------------------------------------
			if(RecallTypes.IsSpecialRecallType(recallCur.RecallTypeNum)) {//if this is a prophy or perio
				for(int i=0;i<listRecalls.Count;i++) {
					if(recallCur.RecallNum==listRecalls[i].RecallNum) {
						continue;//already handled.
					}
					if(listRecalls[i].IsDisabled) {
						continue;
					}
					if(listRecalls[i].DateDue.Year<1880) {
						continue;
					}
					if(listRecalls[i].DateDue>recallCur.DateDue//if film due date is after prophy due date
						&& listRecalls[i].DateDue>DateTime.Today)//and not overdue
					{
						continue;
					}
					//Due to automation complexities with Web Sched and the many ways that users can dictate what recall types show in the Recall List,
					//  excluding manual recall types should be done prior to calling this method.
					//Meaning that listRecalls should have its list manipulated to exclude unwanted recalls prior to passing them into this method.
					listProcStrs.AddRange(RecallTypes.GetProcs(listRecalls[i].RecallTypeNum));
					listProcPatterns.Add(RecallTypes.GetTimePattern(listRecalls[i].RecallTypeNum));
				}
			}
			return Appointments.GetApptTimePatternFromProcPatterns(listProcPatterns);
		}

		public static List<Recall> GetChangedSince(DateTime changedSince) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Recall>>(MethodBase.GetCurrentMethod(),changedSince);
			} 
			string command="SELECT * FROM recall WHERE DateTStamp > "+POut.DateT(changedSince);
			return Crud.RecallCrud.SelectMany(command);
		}

		///<summary>Only used in FormRecallList and recall automation to get a list of patients with recall.  
		///Supply a date range, using min and max values if user left blank.  If provNum=0, then it will get all provnums.  
		///It looks for both provider match in either PriProv or SecProv.</summary>
		public static DataTable GetRecallList(DateTime fromDate,DateTime toDate,bool groupByFamilies,long provNum,long clinicNum,
			long siteNum,RecallListSort sortBy,RecallListShowNumberReminders showReminders,List<long> excludePatNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),fromDate,toDate,groupByFamilies,provNum,clinicNum,siteNum,sortBy,showReminders,excludePatNums);
			}
			DataTable table=new DataTable();
			//columns that start with lowercase are altered for display rather than being raw data.
			table.Columns.Add("age");
			table.Columns.Add("billingType");
			table.Columns.Add("contactMethod");//text representation for display
			table.Columns.Add("ClinicNum");
			table.Columns.Add("dateLastReminder");
			table.Columns.Add("DateDue",typeof(DateTime));
			table.Columns.Add("dueDate");//blank if minVal
			table.Columns.Add("Email");
			table.Columns.Add("FName");
			table.Columns.Add("Guarantor");
			table.Columns.Add("guarFName");
			table.Columns.Add("guarLName");
			table.Columns.Add("LName");
			table.Columns.Add("maxDateDue",typeof(DateTime));
			table.Columns.Add("Note");
			table.Columns.Add("numberOfReminders");
			table.Columns.Add("patientName");
			table.Columns.Add("PatNum");
			table.Columns.Add("PreferRecallMethod");
			table.Columns.Add("recallInterval");
			table.Columns.Add("RecallNum");
			table.Columns.Add("recallType");
			table.Columns.Add("status");
			List<DataRow> rows=new List<DataRow>();
			string command;
			#region Run Queries and Create Dictionaries
			#region Recall query
			command="SELECT recall.RecallNum,recall.PatNum,recall.DateDue,recall.DatePrevious,recall.RecallInterval,recall.RecallStatus,recall.Note,"
				+"recall.DisableUntilBalance,recall.DisableUntilDate,recalltype.Description recalltype "
				+"FROM recall "
				+"INNER JOIN recalltype ON recall.RecallTypeNum=recalltype.RecallTypeNum "
				+"WHERE recall.DateDue BETWEEN "+POut.Date(fromDate)+" AND "+POut.Date(toDate)+" "
				+"AND recall.IsDisabled=0 ";
			if(PrefC.GetString(PrefName.RecallTypesShowingInList)!="") {
				command+="AND recall.RecallTypeNum IN("+PrefC.GetString(PrefName.RecallTypesShowingInList)+") ";
			}
			if(PrefC.GetBool(PrefName.RecallExcludeIfAnyFutureAppt)) {
				command+="AND NOT EXISTS(SELECT * FROM appointment "
					+"WHERE appointment.PatNum=recall.PatNum "
					+"AND appointment.AptDateTime>"+DbHelper.Curdate()+" "//early this morning
					+"AND appointment.AptStatus IN(1,4))";//scheduled,ASAP
			}
			else {
				command+="AND recall.DateScheduled='0001-01-01'"; //Only show rows where no future recall appointment.
			}
			DataTable rawRecallTable=Db.GetTable(command);
			#endregion Recall query
			if(rawRecallTable.Rows.Count<1) {
				return table;//No recalls, no need to proceed any further.
			}
			//Sort recalls into dictionary of PatNum to List<DataRow>, one DataRow for each recall. 
			//Excludes recalls that are disabled.
			Dictionary<long,List<DataRow>> dictRecallRows=rawRecallTable.Rows.OfType<DataRow>()
				.GroupBy(x => PIn.Long(x["PatNum"].ToString()))
				.ToDictionary(x => x.Key,x => x.ToList());
			#region Patient query
			command="SELECT patient.PatNum,patient.LName,patient.FName,patient.Preferred,patient.Birthdate,patient.HmPhone,patient.WkPhone,"
				+"patient.WirelessPhone,patient.Email,patient.ClinicNum,patient.PreferRecallMethod,patient.BillingType,"
				+"patient.Guarantor,patguar.LName guarLName,patguar.FName guarFName,patguar.Email guarEmail,patguar.InsEst,patguar.BalTotal "
				+"FROM patient "
				+"INNER JOIN patient patguar ON patient.Guarantor=patguar.PatNum "
				+"WHERE patient.PatNum IN ("+string.Join(",",dictRecallRows.Keys)+") "
				+"AND patient.PatStatus="+POut.Int((int)PatientStatus.Patient)+" ";
			if(provNum>0) {
				command+="AND (patient.PriProv="+POut.Long(provNum)+" OR patient.SecProv="+POut.Long(provNum)+") ";
			}
			if(clinicNum>0) {
				command+="AND patient.ClinicNum="+POut.Long(clinicNum)+" ";
			}
			if(siteNum>0) {
				command+="AND patient.SiteNum="+POut.Long(siteNum);
			}
			DataTable rawPatientTable=Db.GetTable(command);
			#endregion Patient query
			if(rawPatientTable.Rows.Count<1) {
				return table;//No active patients in recall list (after filtering by patstatus, provnum, sitenum, or clinicNum)
			}
			//Create dict of PatNum to DataRow continaing pat/guarantor data, one row per patnum.
			Dictionary<long,DataRow> dictPatientRows=rawPatientTable.Rows.OfType<DataRow>().ToDictionary(x => PIn.Long(x["PatNum"].ToString()));
			//Create dict Guarantor to max DateDue for all family member recalls in the date range
			Dictionary<long,DateTime> dictGuarMaxDateDue=rawPatientTable.Rows.OfType<DataRow>()
				.GroupBy(x => PIn.Long(x["Guarantor"].ToString()),x => PIn.Long(x["PatNum"].ToString())) //get key=guarantor PatNum, value=family member PatNums
				.ToDictionary(x => x.Key,x => x.Where(y => dictRecallRows.ContainsKey(y)) //where there is a recall for the family member
					.SelectMany(y => dictRecallRows[y] //SelectMany because a patient may have more than one recalltype due
						.Select(z => PIn.Date(z["DateDue"].ToString()))).Max());//Select max DateDue for all recalls for all family members
			#region Commlog query
			command="SELECT PatNum,CommDateTime "
				+"FROM commlog "
				+"WHERE CommType="+POut.Long(Commlogs.GetTypeAuto(CommItemTypeAuto.RECALL))+" "
				+"AND PatNum IN ("+string.Join(",",dictPatientRows.Keys)+")";
			DataTable rawCommlogTable=Db.GetTable(command);
			#endregion Commlog query
			//Create dictionary of key=PatNum, value=List<DataRow> where rows are recall commlogs for the patient
			Dictionary<long,List<DataRow>> dictCommlogRows=new Dictionary<long,List<DataRow>>();
			if(rawCommlogTable.Rows.Count>0) {
				dictCommlogRows=rawCommlogTable.Rows.OfType<DataRow>()
				.GroupBy(x => PIn.Long(x["PatNum"].ToString()),x => x)
				.ToDictionary(x => x.Key,x => x.ToList());
			}
			#endregion Run Queries and Create Dictionaries
			List<DataRow> listPatCommlogRows;
			DataRow rowPat;
			DateTime dateDue;
			DateTime datePrevious;
			DateTime dateRemind;
			ContactMethod contmeth;
			int numberOfReminders;
			long patNum;
			long guarNum;
			double disableUntilBalance;
			double familyBalance;
			DataRow row;
			long recallMaxNumberReminders=PrefC.GetLong(PrefName.RecallMaxNumberReminders);
			#region Create List of Rows for Return Table
			//loop through the patients in the recall dictionary
			foreach(KeyValuePair<long,List<DataRow>> kvp in dictRecallRows) {
				patNum=kvp.Key;
				if(!dictPatientRows.ContainsKey(patNum) || excludePatNums.Contains(patNum)) {//patient.PatStatus wasn't 'Patient' or in exclude list, skip
					continue;
				}
				rowPat=dictPatientRows[patNum];
				guarNum=PIn.Long(rowPat["Guarantor"].ToString());
				listPatCommlogRows=new List<DataRow>();
				if(dictCommlogRows.ContainsKey(patNum)) {
					listPatCommlogRows=dictCommlogRows[patNum];
				}
				familyBalance=PIn.Double(rowPat["BalTotal"].ToString());//from the guarantor's patient table
				if(!PrefC.GetBool(PrefName.BalancesDontSubtractIns)) {//typical
					familyBalance-=PIn.Double(rowPat["InsEst"].ToString());
				}
				//loop through the recalls for each patient
				foreach(DataRow rowCur in kvp.Value) {
					dateDue=PIn.Date(rowCur["DateDue"].ToString());
					datePrevious=PIn.Date(rowCur["DatePrevious"].ToString());
					//get list of commlog dates where the commlogs are recall commlogs and the date is after datePrevious for this recall
					List<DateTime> listDTReminders=listPatCommlogRows.Select(x => PIn.DateT(x["CommDateTime"].ToString())).Where(x => x>datePrevious).ToList();
					numberOfReminders=listDTReminders.Count();//number of recall commlogs that happened after datePrevious
					dateRemind=listDTReminders.DefaultIfEmpty(DateTime.MinValue).Max();
					#region Filter by Number of Reminders and Disabled Until Date/Balance
					//filter by number of reminders, if numberOfReminders==0, always show
					if(numberOfReminders==1) {
						if(PrefC.GetInt(PrefName.RecallShowIfDaysFirstReminder)==-1) {
							continue;
						}
						if(dateRemind.AddDays(PrefC.GetInt(PrefName.RecallShowIfDaysFirstReminder)).Date>DateTime.Today) {
							continue;
						}
					}
					else if(numberOfReminders>1) {
						if(PrefC.GetInt(PrefName.RecallShowIfDaysSecondReminder)==-1) {
							continue;
						}
						if(dateRemind.AddDays(PrefC.GetInt(PrefName.RecallShowIfDaysSecondReminder)).Date>DateTime.Today) {
							continue;
						}
					}
					if(recallMaxNumberReminders>-1 && numberOfReminders>recallMaxNumberReminders) {
						continue;
					}
					if(showReminders==RecallListShowNumberReminders.All) {
						//don't skip, add all to list
					}
					else if(showReminders<RecallListShowNumberReminders.SixPlus) {
						if(numberOfReminders!=((int)showReminders-1)) {//if numberOfReminders!=enum value cast to int -1 to account for All being 0
							continue;
						}
					}
					else if(showReminders==RecallListShowNumberReminders.SixPlus) {
						if(numberOfReminders<((int)showReminders-1)) {//numberOfReminders<6 (SixPlus is index 7 since All=0 and Zero=1, so -1)
							continue;
						}
					}
					//filter by disable until date and balance
					if(PIn.Date(rowCur["DisableUntilDate"].ToString())>DateTime.Today) {
						continue;
					}
					disableUntilBalance=PIn.Double(rowCur["DisableUntilBalance"].ToString());
					if(disableUntilBalance>0 && familyBalance>disableUntilBalance) {
						continue;
					}
					#endregion Filter by Number of Reminders and Disabled Until Date/Balance
					#region Create Row
					row=table.NewRow();
					row["age"]=Patients.DateToAge(PIn.Date(rowPat["Birthdate"].ToString())).ToString();
					row["billingType"]=DefC.GetName(DefCat.BillingTypes,PIn.Long(rowPat["BillingType"].ToString()));
					row["ClinicNum"]=PIn.Long(rowPat["ClinicNum"].ToString());
					#region Contact Method
					contmeth=(ContactMethod)PIn.Long(rowPat["PreferRecallMethod"].ToString());
					switch(contmeth) {
						case ContactMethod.None:
							if(PrefC.GetBool(PrefName.RecallUseEmailIfHasEmailAddress)) {//if user wants to use email if there is an email address
								if(groupByFamilies && rowPat["guarEmail"].ToString()!="") {
									row["contactMethod"]=rowPat["guarEmail"].ToString();
									break;
								}
								else if(!groupByFamilies && rowPat["Email"].ToString()!="") {
									row["contactMethod"]=rowPat["Email"].ToString();
									break;
								}
							}
							//no email, or user doesn't want to use email even if there is one, default to using HmPhone
							row["contactMethod"]=Lans.g("FormRecallList","Hm")+":"+rowPat["HmPhone"].ToString();
							break;
						case ContactMethod.HmPhone:
							row["contactMethod"]=Lans.g("FormRecallList","Hm")+":"+rowPat["HmPhone"].ToString();
							break;
						case ContactMethod.WkPhone:
							row["contactMethod"]=Lans.g("FormRecallList","Wk")+":"+rowPat["WkPhone"].ToString();
							break;
						case ContactMethod.WirelessPh:
							row["contactMethod"]=Lans.g("FormRecallList","Cell")+":"+rowPat["WirelessPhone"].ToString();
							break;
						case ContactMethod.TextMessage:
							row["contactMethod"]=Lans.g("FormRecallList","Text")+":"+rowPat["WirelessPhone"].ToString();
							break;
						case ContactMethod.Email:
							if(groupByFamilies) {
								row["contactMethod"]=rowPat["guarEmail"].ToString();//always use guarantor email if grouped by fam
							}
							else {
								row["contactMethod"]=rowPat["Email"].ToString();
							}
							break;
						case ContactMethod.Mail:
							row["contactMethod"]=Lans.g("FormRecallList","Mail");
							break;
						case ContactMethod.DoNotCall:
						case ContactMethod.SeeNotes:
							row["contactMethod"]=Lans.g("enumContactMethod",contmeth.ToString());
							break;
					}
					#endregion Contact Method
					row["dateLastReminder"]="";
					if(dateRemind.Year>1880) {
						row["dateLastReminder"]=dateRemind.ToShortDateString();
					}
					row["DateDue"]=dateDue;
					if(dateDue.Year>1880) {
						row["dueDate"]=dateDue.ToShortDateString();
					}
					if(groupByFamilies) {
						row["Email"]=rowPat["guarEmail"].ToString();
					}
					else {
						row["Email"]=rowPat["Email"].ToString();
					}
					row["PatNum"]=patNum.ToString();
					row["FName"]=rowPat["FName"].ToString();
					row["LName"]=rowPat["LName"].ToString();
					row["patientName"]=Patients.GetNameLF(rowPat["LName"].ToString(),rowPat["FName"].ToString(),rowPat["Preferred"].ToString(),"");
					row["Guarantor"]=guarNum.ToString();
					row["guarFName"]=rowPat["guarFName"].ToString();
					row["guarLName"]=rowPat["guarLName"].ToString();
					row["maxDateDue"]=DateTime.MinValue;
					if(dictGuarMaxDateDue.ContainsKey(guarNum)) {
						row["maxDateDue"]=dictGuarMaxDateDue[guarNum];
					}
					row["Note"]=rowCur["Note"].ToString();
					row["numberOfReminders"]="";
					if(numberOfReminders>0) {
						row["numberOfReminders"]=numberOfReminders.ToString();
					}
					row["PreferRecallMethod"]=rowPat["PreferRecallMethod"].ToString();
					row["recallInterval"]=(new Interval(PIn.Int(rowCur["RecallInterval"].ToString()))).ToString();
					row["RecallNum"]=rowCur["RecallNum"].ToString();
					row["recallType"]=rowCur["recalltype"].ToString();
					row["status"]=DefC.GetName(DefCat.RecallUnschedStatus,PIn.Long(rowCur["RecallStatus"].ToString()));
					#endregion Create Row
					rows.Add(row);
				}
			}
			#endregion Create List of Rows for Return Table
			RecallComparer comparer=new RecallComparer();
			comparer.GroupByFamilies=groupByFamilies;
			comparer.SortBy=sortBy;
			rows.Sort(comparer);
			rows.ForEach(x => table.Rows.Add(x));
			return table;
		}

		///<summary></summary>
		public static long Insert(Recall recall) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				recall.RecallNum=Meth.GetLong(MethodBase.GetCurrentMethod(),recall);
				return recall.RecallNum;
			}
			return Crud.RecallCrud.Insert(recall);
		}

		///<summary></summary>
		public static void Update(Recall recall) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),recall);
				return;
			}
			Crud.RecallCrud.Update(recall);
		}

		///<summary></summary>
		public static void Delete(Recall recall) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),recall);
				return;
			}
			string command= "DELETE from recall WHERE RecallNum = "+POut.Long(recall.RecallNum);
			Db.NonQ(command);
			DeletedObjects.SetDeleted(DeletedObjectType.RecallPatNum,recall.PatNum);
		}

		public static void FullSynch() {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod());
				return;
			}
			//All active recall types for each active patient. 
			//Also includes the last date the proc occurred as well as if the patient is perio or not.
			string command = @"SELECT DISTINCT patient.PatNum,patrecalltrigger.RecallTypeNum,patrecalltrigger.DefaultInterval
									,recentproc.procDate_, CASE WHEN recall.RecallTypeNum IS NULL THEN '0' ELSE '1' END isPerio
								FROM patient 
								-- Make X rows for each patient where X is each active recall type.
								INNER JOIN (
									SELECT recalltype.RecallTypeNum,recalltype.DefaultInterval
									FROM recalltype
									INNER JOIN recalltrigger ON recalltype.RecallTypeNum=recalltrigger.RecallTypeNum) patrecalltrigger ON TRUE
								-- Get the most recent proc date for each recall type for each patient.
								LEFT JOIN (
									SELECT RecallTypeNum,MAX(ProcDate) procDate_,procedurelog.PatNum 
									FROM procedurelog
									INNER JOIN recalltrigger ON procedurelog.CodeNum=recalltrigger.CodeNum
									INNER JOIN patient ON procedurelog.patnum=patient.patnum
									WHERE ProcStatus IN ("+POut.Long((int)ProcStat.C)+","+POut.Long((int)ProcStat.EC)+","+POut.Long((int)ProcStat.EO)+@")
									AND patient.PatStatus="+POut.Long((int)PatientStatus.Patient)+@"
									GROUP BY PatNum,RecallTypeNum) recentproc ON patient.PatNum=recentproc.PatNum AND patrecalltrigger.RecalltypeNum=recentproc.RecallTypeNum
								-- Check to see if the patient is perio or not
								LEFT JOIN recall ON patient.PatNum=recall.PatNum AND recall.RecallTypeNum="+PrefC.GetLong(PrefName.RecallTypeSpecialPerio)+@"
								WHERE patient.PatStatus="+POut.Long((int)PatientStatus.Patient)+@"
								ORDER BY patient.PatNum";
			DataTable table = Db.GetTable(command);
			//Organize the data structure into a dictionary so that we only loop through each patient once and consider all recall types for that specific patient.
			Dictionary<long,List<DataRow>> dictRecallsToUpdate = table.Select()
				.GroupBy(x => PIn.Long(x["PatNum"].ToString()))
				.ToDictionary(y => y.Key,y => y.ToList());
			if(dictRecallsToUpdate.Count==0) {
				return;
			}
			//Get every recall in the database for each active patient with the specified active recall types.
			command=@"SELECT recall.* FROM recall 
								INNER JOIN patient ON patient.PatNum=recall.PatNum
								WHERE patient.PatStatus="+POut.Long((int)PatientStatus.Patient)+@"
								AND recall.RecallTypeNum IN ("+String.Join(",",dictRecallsToUpdate.First().Value.Select(x => x["RecallTypeNum"]).ToList())+")";
			List<Recall> recallList = Crud.RecallCrud.SelectMany(command);
			foreach(KeyValuePair<long,List<DataRow>> recallPair in dictRecallsToUpdate) {
				SynchPatient(recallPair.Key,recallPair.Value,recallList);
			}
		}

		///<summary>Helper method for Updating and Inserting recalls for active patients.  Used primarily in the FullSynch from FormRecallTypes</summary>
		private static void SynchPatient(long patNum,List<DataRow> listPatRecallTypeRows,List<Recall> listAllRecalls) {
			//No need to check RemotingRole; no call to db.
			//Loop through all of the recall type rows for this patient and take actions needed for any recall triggers.
			foreach(DataRow row in listPatRecallTypeRows) {
				long recallTypeNum=PIn.Long(row["RecallTypeNum"].ToString());
				Interval defaultInterval=new Interval(PIn.Int(row["DefaultInterval"].ToString()));
				DateTime dateProc=PIn.Date(row["procDate_"].ToString());
				bool isPerio=PIn.Bool(row["isPerio"].ToString());
				//Skip the recall type row that does not match the "special type" of recall that this patient is (assume there is only one).
				//E.g. This patient is flagged as a perio patient (isPerio = 1) so we need to skip the special prophy type.
				if((isPerio && recallTypeNum==PrefC.GetLong(PrefName.RecallTypeSpecialProphy))
					|| (!isPerio && recallTypeNum==PrefC.GetLong(PrefName.RecallTypeSpecialPerio))) 
				{
					continue;
				}
				//At this point, we know that action may be needed for this particular recall type.
				//We will either update recalls, create new recalls, or do nothing for this patient and the particular recall type.
				DateTime datePrev;
				Recall recallMatch;
				Recall recallNew;
				recallMatch=null;
				for(int r = 0;r<listAllRecalls.Count;r++) {//recalls for patient
					if(listAllRecalls[r].RecallTypeNum==recallTypeNum && listAllRecalls[r].PatNum==patNum) {
						recallMatch=listAllRecalls[r];
						break;
					}
				}
				//Default datePrev to the most recent proc date for this recall type for this patient (the query gives us this).
				datePrev=dateProc;
				//For special recall types only, we need to get the most recent proc date.
				if(PrefC.GetLong(PrefName.RecallTypeSpecialProphy)==recallTypeNum
					|| PrefC.GetLong(PrefName.RecallTypeSpecialPerio)==recallTypeNum)
				{
					DateTime dateMaxSpecialType=GetMaxSpecialTypeDate(listPatRecallTypeRows);
					if(dateMaxSpecialType.Year>1880) {
						datePrev=dateMaxSpecialType;
					}
				}
				if(recallMatch==null) {//if there is no existing recall,
					if(PrefC.GetLong(PrefName.RecallTypeSpecialProphy)==recallTypeNum
						|| PrefC.GetLong(PrefName.RecallTypeSpecialPerio)==recallTypeNum
						|| datePrev.Year>1880)//for other types, if date is not minVal, then add a recall
					{
						//add a recall
						recallNew=new Recall();
						recallNew.RecallTypeNum=recallTypeNum;
						recallNew.PatNum=patNum;
						recallNew.DatePrevious=datePrev;//will be min val for prophy/perio with no previous procs
						recallNew.RecallInterval=defaultInterval;
						if(datePrev.Year<1880) {
							recallNew.DateDueCalc=DateTime.MinValue;
						}
						else {
							recallNew.DateDueCalc=datePrev+recallNew.RecallInterval;
						}
						recallNew.DateDue=recallNew.DateDueCalc;
						Recalls.Insert(recallNew);
					}
				}
				else {//alter the existing recall
					if(!recallMatch.IsDisabled
						&& recallMatch.DisableUntilBalance==0
						&& recallMatch.DisableUntilDate.Year<1880
						&& datePrev.Year>1880 //this protects recalls that were manually added as part of a conversion
						&& datePrev != recallMatch.DatePrevious) //if datePrevious has changed, reset
					{
						recallMatch.RecallStatus=0;
						recallMatch.Note="";
						recallMatch.DateDue=recallMatch.DateDueCalc;//now it is allowed to be changed in the steps below
					}
					if(datePrev.Year<1880) {//if no previous date
						recallMatch.DatePrevious=DateTime.MinValue;
						if(recallMatch.DateDue==recallMatch.DateDueCalc) {//user did not enter a DateDue
							recallMatch.DateDue=DateTime.MinValue;
						}
						recallMatch.DateDueCalc=DateTime.MinValue;
						Recalls.Update(recallMatch);
					}
					else {//if previous date is a valid date
						recallMatch.DatePrevious=datePrev;
						if(recallMatch.IsDisabled) {//if the existing recall is disabled 
							recallMatch.DateDue=DateTime.MinValue;//DateDue is always blank
						}
						else {//but if not disabled
							if(recallMatch.DateDue==recallMatch.DateDueCalc//if user did not enter a DateDue
								|| recallMatch.DateDue.Year<1880)//or DateDue was blank
							{
								recallMatch.DateDue=recallMatch.DatePrevious+recallMatch.RecallInterval;//set same as DateDueCalc
							}
						}
						recallMatch.DateDueCalc=recallMatch.DatePrevious+recallMatch.RecallInterval;
						Recalls.Update(recallMatch);
					}
				}
			}
		}

		///<summary>Helper method to get the most recent proc date for either of the special recall types.
		///Returns DateTime.MinValue if the patient does not have any procedures.</summary>
		private static DateTime GetMaxSpecialTypeDate(List<DataRow> listPatRecallTypeRows) {
			DateTime dateMaxSpecialType=DateTime.MinValue;
			DateTime dateProc=DateTime.MinValue;
			foreach(DataRow row in listPatRecallTypeRows) {
				dateProc=PIn.Date(row["procDate_"].ToString());
				long recallTypeNum=PIn.Long(row["RecallTypeNum"].ToString());
				if((PrefC.GetLong(PrefName.RecallTypeSpecialProphy)==recallTypeNum || PrefC.GetLong(PrefName.RecallTypeSpecialPerio)==recallTypeNum) 
					&& dateProc>dateMaxSpecialType) 
				{
					dateMaxSpecialType=dateProc;
				}
			}
			return dateMaxSpecialType;
		}

		///<summary>Synchronizes all recalls for one patient. 
		///If datePrevious has changed, then it completely deletes the old status and note information and sets a new DatePrevious and dateDueCalc.  
		///Also updates dateDue to match dateDueCalc if not disabled.  Creates any recalls as necessary.  
		///Recalls will never get automatically deleted except when all triggers are removed.  Otherwise, the dateDueCalc just gets cleared.</summary>
		public static void Synch(long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),patNum);
				return;
			}
			List<RecallType> typeListActive=RecallTypes.GetActive();
			List<RecallType> typeList=new List<RecallType>(typeListActive);
			string command="SELECT * FROM recall WHERE PatNum="+POut.Long(patNum);
			List<Recall> recallList=Crud.RecallCrud.SelectMany(command);
			//determine if this patient is a perio patient.
			bool isPerio=false;
			for(int i=0;i<recallList.Count;i++){
				if(PrefC.GetLong(PrefName.RecallTypeSpecialPerio)==recallList[i].RecallTypeNum){
					isPerio=true;
					break;
				}
			}
			//remove types from the list which do not apply to this patient.
			for(int i=0;i<typeList.Count;i++){//it's ok to not go backwards because we immediately break.
				if(isPerio) {
					if(PrefC.GetLong(PrefName.RecallTypeSpecialProphy)==typeList[i].RecallTypeNum) {
						typeList.RemoveAt(i);
						break;
					}
				}
				else {
					if(PrefC.GetLong(PrefName.RecallTypeSpecialPerio)==typeList[i].RecallTypeNum) {
						typeList.RemoveAt(i);
						break;
					}
				}
			}
			//get previous dates for all types at once.
			//Because of the inner join, this will not include recall types with no trigger.
			command="SELECT RecallTypeNum,MAX(ProcDate) procDate_ "
				+"FROM procedurelog,recalltrigger "
				+"WHERE PatNum="+POut.Long(patNum)
				+" AND procedurelog.CodeNum=recalltrigger.CodeNum "
				+"AND (";
			if(typeListActive.Count>0) {//This will include both prophy and perio, regardless of whether this is a prophy or perio patient.
				for(int i=0;i<typeListActive.Count;i++) {
					if(i>0) {
						command+=" OR";
					}
					command+=" RecallTypeNum="+POut.Long(typeListActive[i].RecallTypeNum);
				}
			} 
			else {
				command+=" RecallTypeNum=0";//Effectively forces an empty result set, without changing the returned table structure.
			}
			command+=") AND (ProcStatus = "+POut.Long((int)ProcStat.C)+" "
				+"OR ProcStatus = "+POut.Long((int)ProcStat.EC)+" "
				+"OR ProcStatus = "+POut.Long((int)ProcStat.EO)+") "
				+"GROUP BY RecallTypeNum";
			DataTable tableDates=Db.GetTable(command);
			//Go through the type list and either update recalls, or create new recalls.
			//Recalls that are no longer active because their type has no triggers will be ignored.
			//It is assumed that there are no duplicate recall types for a patient.
			DateTime prevDate;
			Recall matchingRecall;
			Recall recallNew;
			DateTime prevDateProphy=DateTime.MinValue;
			DateTime dateProphyTesting;
			for(int i=0;i<typeListActive.Count;i++) {
				if(PrefC.GetLong(PrefName.RecallTypeSpecialProphy)!=typeListActive[i].RecallTypeNum
					&& PrefC.GetLong(PrefName.RecallTypeSpecialPerio)!=typeListActive[i].RecallTypeNum) 
				{
					//we are only working with prophy and perio in this loop.
					continue;
				}
				for(int d=0;d<tableDates.Rows.Count;d++) {//procs for patient
					if(tableDates.Rows[d]["RecallTypeNum"].ToString()==typeListActive[i].RecallTypeNum.ToString()) {
						dateProphyTesting=PIn.Date(tableDates.Rows[d]["procDate_"].ToString());
						//but patient could have both perio and prophy.
						//So must test to see if the date is newer
						if(dateProphyTesting>prevDateProphy) {
							prevDateProphy=dateProphyTesting;
						}
						break;
					}
				}
			}
			for(int i=0;i<typeList.Count;i++){//active types for this patient.
				if(RecallTriggers.GetForType(typeList[i].RecallTypeNum).Count==0) {
					//if no triggers for this recall type, then skip it.  Don't try to add or alter.
					continue;
				}
				//set prevDate:
				if(PrefC.GetLong(PrefName.RecallTypeSpecialProphy)==typeList[i].RecallTypeNum
					|| PrefC.GetLong(PrefName.RecallTypeSpecialPerio)==typeList[i].RecallTypeNum) 
				{
					prevDate=prevDateProphy;
				}
				else {
					prevDate=DateTime.MinValue;
					for(int d=0;d<tableDates.Rows.Count;d++) {//procs for patient
						if(tableDates.Rows[d]["RecallTypeNum"].ToString()==typeList[i].RecallTypeNum.ToString()) {
							prevDate=PIn.Date(tableDates.Rows[d]["procDate_"].ToString());
							break;
						}
					}
				}
				matchingRecall=null;
				for(int r=0;r<recallList.Count;r++){//recalls for patient
					if(recallList[r].RecallTypeNum==typeList[i].RecallTypeNum){
						matchingRecall=recallList[r];
						break;
					}
				}
				if(matchingRecall==null){//if there is no existing recall,
					if(PrefC.GetLong(PrefName.RecallTypeSpecialProphy)==typeList[i].RecallTypeNum
						|| PrefC.GetLong(PrefName.RecallTypeSpecialPerio)==typeList[i].RecallTypeNum
						|| prevDate.Year>1880)//for other types, if date is not minVal, then add a recall
					{
						//add a recall
						recallNew=new Recall();
						recallNew.RecallTypeNum=typeList[i].RecallTypeNum;
						recallNew.PatNum=patNum;
						recallNew.DatePrevious=prevDate;//will be min val for prophy/perio with no previous procs
						recallNew.RecallInterval=typeList[i].DefaultInterval;
						if(prevDate.Year<1880) {
							recallNew.DateDueCalc=DateTime.MinValue;
						}
						else {
							recallNew.DateDueCalc=prevDate+recallNew.RecallInterval;
						}
						recallNew.DateDue=recallNew.DateDueCalc;
						Recalls.Insert(recallNew);
					}
				}
				else{//alter the existing recall
					if(!matchingRecall.IsDisabled
						&& matchingRecall.DisableUntilBalance==0
						&& matchingRecall.DisableUntilDate.Year<1880
						&& prevDate.Year>1880//this protects recalls that were manually added as part of a conversion
						&& prevDate != matchingRecall.DatePrevious) 
					{//if datePrevious has changed, reset
						matchingRecall.RecallStatus=0;
						matchingRecall.Note="";
						matchingRecall.DateDue=matchingRecall.DateDueCalc;//now it is allowed to be changed in the steps below
					}
					if(prevDate.Year<1880){//if no previous date
						matchingRecall.DatePrevious=DateTime.MinValue;
						if(matchingRecall.DateDue==matchingRecall.DateDueCalc){//user did not enter a DateDue
							matchingRecall.DateDue=DateTime.MinValue;
						}
						matchingRecall.DateDueCalc=DateTime.MinValue;
						Recalls.Update(matchingRecall);
					}
					else{//if previous date is a valid date
						matchingRecall.DatePrevious=prevDate;
						if(matchingRecall.IsDisabled){//if the existing recall is disabled 
							matchingRecall.DateDue=DateTime.MinValue;//DateDue is always blank
						}
						else{//but if not disabled
							if(matchingRecall.DateDue==matchingRecall.DateDueCalc//if user did not enter a DateDue
								|| matchingRecall.DateDue.Year<1880)//or DateDue was blank
							{
								matchingRecall.DateDue=matchingRecall.DatePrevious+matchingRecall.RecallInterval;//set same as DateDueCalc
							}
						}
						matchingRecall.DateDueCalc=matchingRecall.DatePrevious+matchingRecall.RecallInterval;
						Recalls.Update(matchingRecall);
					}
				}
			}
			//now, we need to loop through all the inactive recall types and clear the DateDueCalc
			//We don't do this anymore. User must explicitly delete recalls, either one-by-one, or from the recall type window.
			/*
			List<RecallType> typeListInactive=RecallTypes.GetInactive();
			for(int i=0;i<typeListInactive.Count;i++){
				matchingRecall=null;
				for(int r=0;r<recallList.Count;r++){
					if(recallList[r].RecallTypeNum==typeListInactive[i].RecallTypeNum){
						matchingRecall=recallList[r];
					}
				}
				if(matchingRecall==null){//if there is no existing recall,
					continue;
				}
				Recalls.Delete(matchingRecall);//we'll just delete it
				//There is an existing recall, so alter it if certain conditions are met
				//matchingRecall.DatePrevious=DateTime.MinValue;
				//if(matchingRecall.DateDue==matchingRecall.DateDueCalc){//if user did not enter a DateDue
					//we can safely alter the DateDue
				//	matchingRecall.DateDue=DateTime.MinValue;
				//}
				//matchingRecall.DateDueCalc=DateTime.MinValue;
				//Recalls.Update(matchingRecall);
			}*/
		}

		///<summary>Synchronizes DateScheduled column in recall table for one patient.  
		///This must be used instead of lazy synch in RecallsForPatient, when deleting an appointment, when sending to unscheduled list, setting an appointment complete, etc.  
		///This is fast, but it would be inefficient to call it too much.</summary>
		public static void SynchScheduledApptFull(long patNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),patNum);
				return;
			}
			//Clear out DateScheduled column for this pat before changing
			string command="UPDATE recall "
				+"SET recall.DateScheduled="+POut.Date(DateTime.MinValue)+" "
				+"WHERE recall.PatNum="+POut.Long(patNum);
			Db.NonQ(command);
			//Get table of future appointments dates with recall type for this patient, where a procedure is attached that is a recall trigger procedure
			command="SELECT recalltrigger.RecallTypeNum,MIN("+DbHelper.DtimeToDate("appointment.AptDateTime")+") AS AptDateTime "
				+"FROM procedurelog "
				+"INNER JOIN recalltrigger ON procedurelog.CodeNum=recalltrigger.CodeNum "
				+"INNER JOIN recall ON recalltrigger.RecallTypeNum=recall.RecallTypeNum "
					+"AND recall.PatNum="+POut.Long(patNum)+" "
				+"INNER JOIN appointment ON appointment.AptNum=procedurelog.AptNum "
					+"AND appointment.PatNum="+POut.Long(patNum)+" "
					+"AND (appointment.AptStatus="+POut.Int((int)ApptStatus.Scheduled)+" OR appointment.AptStatus="+POut.Int((int)ApptStatus.ASAP)+") "
					+"AND appointment.AptDateTime > "+DbHelper.Curdate()+" "//early this morning
				+"WHERE procedurelog.PatNum="+POut.Long(patNum)+" "
				+"GROUP BY recalltrigger.RecallTypeNum";
			DataTable table=Db.GetTable(command);
			//Update the recalls for this patient with DATE(AptDateTime) where there is a future appointment with recall proc on it
			for(int i=0;i<table.Rows.Count;i++) {
				if(table.Rows[i]["RecallTypeNum"].ToString()=="") {
					continue;
				}
				command=@"UPDATE recall	SET recall.DateScheduled="+POut.Date(PIn.Date(table.Rows[i]["AptDateTime"].ToString()))+" " 
					+"WHERE recall.RecallTypeNum="+POut.Long(PIn.Long(table.Rows[i]["RecallTypeNum"].ToString()))+" "
					+"AND recall.PatNum="+POut.Long(patNum)+" ";
				Db.NonQ(command);
			}
		}

		///<summary>Updates RecallInterval and DueDate for all patients that have the recallTypeNum and defaultIntervalOld to use the defaultIntervalNew.</summary>
		public static void UpdateDefaultIntervalForPatients(long recallTypeNum,Interval defaultIntervalOld,Interval defaultIntervalNew) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),recallTypeNum,defaultIntervalOld,defaultIntervalNew);
				return;
			}
			string command="SELECT * FROM recall WHERE IsDisabled=0 AND RecallTypeNum="+POut.Long(recallTypeNum)+" AND RecallInterval="+POut.Int(defaultIntervalOld.ToInt());
			List<Recall> recallList=Crud.RecallCrud.SelectMany(command);
			for(int i=0;i<recallList.Count;i++) {
				if(recallList[i].DateDue!=recallList[i].DateDueCalc) {//User entered a DueDate.
					//Don't change the DateDue since user already overrode it
				}
				else{
					recallList[i].DateDue=recallList[i].DatePrevious+defaultIntervalNew;
				}
				recallList[i].DateDueCalc=recallList[i].DatePrevious+defaultIntervalNew;
				recallList[i].RecallInterval=defaultIntervalNew;
				Update(recallList[i]);
			}
		}

		public static void DeleteAllOfType(long recallTypeNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),recallTypeNum);
				return;
			}
			string command="DELETE FROM recall WHERE RecallTypeNum= "+POut.Long(recallTypeNum);
			Db.NonQ(command);
		}

		///<summary></summary>
		public static void SynchAllPatients(){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod());
				return;
			}
			FullSynch();
			//get all active patients with future scheduled appointments that have a procedure attached which is a recall trigger procedure
			string command="SELECT DISTINCT patient.PatNum "
						+"FROM patient "
						+"INNER JOIN appointment ON appointment.PatNum=patient.PatNum AND AptDateTime>"+DbHelper.Curdate()+" "
						+"AND AptStatus IN ("+POut.Int((int)ApptStatus.Scheduled)+","+POut.Int((int)ApptStatus.ASAP)+","+POut.Int((int)ApptStatus.Broken)+") "//Scheduled,ASAP, or Broken
						//Broken is only included to fix a bug that existed between versions 12.4 and 13.2.  It clears out the datesched
						//if broken future appt is the only appt with a recall trigger on it so the patient will be on the recall list again.
						+"INNER JOIN procedurelog ON procedurelog.AptNum=appointment.AptNum "
						+"INNER JOIN recalltrigger ON recalltrigger.CodeNum=procedurelog.CodeNum "
						+"WHERE PatStatus="+POut.Long((int)PatientStatus.Patient);
			DataTable table=Db.GetTable(command);
			for(int i=0;i<table.Rows.Count;i++) {
				SynchScheduledApptFull(PIn.Long(table.Rows[i][0].ToString()));
			}
		}

		///<summary></summary>
		public static DataTable GetAddrTable(List<long> recallNums,bool groupByFamily,RecallListSort sortBy) {
			//No need to check RemotingRole; no call to db.
			DataTable rawTable=GetAddrTableRaw(recallNums);
			List<DataRow> rawRows=new List<DataRow>();
			for(int i=0;i<rawTable.Rows.Count;i++){
				rawRows.Add(rawTable.Rows[i]);
			}
			RecallComparer comparer=new RecallComparer();
			comparer.GroupByFamilies=groupByFamily;
			comparer.SortBy=sortBy;
			rawRows.Sort(comparer);
			DataTable table=new DataTable();
			table.Columns.Add("address");//includes address2. Can be guar.
			table.Columns.Add("City");//Can be guar.
			table.Columns.Add("clinicNum");//will be the guar clinicNum if grouped.
			table.Columns.Add("dateDue");
			table.Columns.Add("email");//Will be guar if grouped by family
			table.Columns.Add("emailPatNum");//Will be guar if grouped by family
			table.Columns.Add("famList");
			table.Columns.Add("guarLName");
			table.Columns.Add("numberOfReminders");//for a family, this will be the max for the family
			table.Columns.Add("patientNameF");//Only used when single email
			table.Columns.Add("patientNameFL");
			table.Columns.Add("patNums");//Comma delimited.  Used in email.
			table.Columns.Add("recallNums");//Comma delimited.  Used during e-mail and eCards
			table.Columns.Add("State");//Can be guar.
			table.Columns.Add("Zip");//Can be guar.
			string familyAptList="";
			string recallNumStr="";
			string patNumStr="";
			DataRow row;
			List<DataRow> rows=new List<DataRow>();
			int maxNumReminders=0;
			int maxRemindersThisPat;
			Patient pat;
			for(int i=0;i<rawRows.Count;i++) {
				if(!groupByFamily) {
					row=table.NewRow();
					row["address"]=rawRows[i]["Address"].ToString();
					if(rawRows[i]["Address2"].ToString()!="") {
						row["address"]+="\r\n"+rawRows[i]["Address2"].ToString();
					}
					row["City"]=rawRows[i]["City"].ToString();
					row["clinicNum"]=rawRows[i]["ClinicNum"].ToString();
					row["dateDue"]=PIn.Date(rawRows[i]["DateDue"].ToString()).ToShortDateString();
					//since not grouping by family, this is always just the patient email
					row["email"]=rawRows[i]["Email"].ToString();
					row["emailPatNum"]=rawRows[i]["PatNum"].ToString();
					row["famList"]="";
					row["guarLName"]=rawRows[i]["guarLName"].ToString();//even though we won't use it.
					row["numberOfReminders"]=PIn.Long(rawRows[i]["numberOfReminders"].ToString()).ToString();
					pat=new Patient();
					pat.LName=rawRows[i]["LName"].ToString();
					pat.FName=rawRows[i]["FName"].ToString();
					pat.Preferred=rawRows[i]["Preferred"].ToString();
					row["patientNameF"]=pat.GetNameFirstOrPreferred();
					row["patientNameFL"]=pat.GetNameFLnoPref();// GetNameFirstOrPrefL();
					row["patNums"]=rawRows[i]["PatNum"].ToString();
					row["recallNums"]=rawRows[i]["RecallNum"].ToString();
					row["State"]=rawRows[i]["State"].ToString();
					row["Zip"]=rawRows[i]["Zip"].ToString();
					rows.Add(row);
					continue;
				}
				//groupByFamily----------------------------------------------------------------------
				if(familyAptList==""){//if this is the first patient in the family
					maxNumReminders=0;
					//loop through the whole family, and determine the maximum number of reminders
					for(int f=i;f<rawRows.Count;f++) {
						maxRemindersThisPat=PIn.Int(rawRows[f]["numberOfReminders"].ToString());
						if(maxRemindersThisPat>maxNumReminders) {
							maxNumReminders=maxRemindersThisPat;
						}
						if(f==rawRows.Count-1//if this is the last row
							|| rawRows[i]["Guarantor"].ToString()!=rawRows[f+1]["Guarantor"].ToString())//or if the guarantor on next line is different
						{
							break;
						}
					}
					//now we know the max number of reminders for the family
					if(i==rawRows.Count-1//if this is the last row
						|| rawRows[i]["Guarantor"].ToString()!=rawRows[i+1]["Guarantor"].ToString())//or if the guarantor on next line is different
					{
						//then this is a single patient, and there are no other family members in the list.
						row=table.NewRow();
						row["address"]=rawRows[i]["Address"].ToString();
						if(rawRows[i]["Address2"].ToString()!="") {
							row["address"]+="\r\n"+rawRows[i]["Address2"].ToString();
						}
						row["City"]=rawRows[i]["City"].ToString();
						row["State"]=rawRows[i]["State"].ToString();
						row["Zip"]=rawRows[i]["Zip"].ToString();
						row["clinicNum"]=rawRows[i]["ClinicNum"].ToString();
						row["dateDue"]=PIn.Date(rawRows[i]["DateDue"].ToString()).ToShortDateString();
						//this will always be the guarantor email
						row["email"]=rawRows[i]["guarEmail"].ToString();
						row["emailPatNum"]=rawRows[i]["Guarantor"].ToString();
						row["famList"]="";
						row["guarLName"]=rawRows[i]["guarLName"].ToString();//even though we won't use it.
						row["numberOfReminders"]=maxNumReminders.ToString();
						//if(rawRows[i]["Preferred"].ToString()=="") {
						row["patientNameF"]=rawRows[i]["FName"].ToString();
						//}
						//else {
						//	row["patientNameF"]=rawRows[i]["Preferred"].ToString();
						//}
						row["patientNameFL"]=rawRows[i]["FName"].ToString()+" "
							+rawRows[i]["MiddleI"].ToString()+" "
							+rawRows[i]["LName"].ToString();
						row["patNums"]=rawRows[i]["PatNum"].ToString();
						row["recallNums"]=rawRows[i]["RecallNum"].ToString();
						rows.Add(row);
						continue;
					}
					else{//this is the first patient of a family with multiple family members
						familyAptList=rawRows[i]["FName"].ToString()+":  "
							+PIn.Date(rawRows[i]["DateDue"].ToString()).ToShortDateString();
						patNumStr=rawRows[i]["PatNum"].ToString();
						recallNumStr=rawRows[i]["RecallNum"].ToString();
						continue;
					}
				}
				else{//not the first patient
					familyAptList+="\r\n"+rawRows[i]["FName"].ToString()+":  "
						+PIn.Date(rawRows[i]["DateDue"].ToString()).ToShortDateString();
					patNumStr+=","+rawRows[i]["PatNum"].ToString();
					recallNumStr+=","+rawRows[i]["RecallNum"].ToString();
				}
				if(i==rawRows.Count-1//if this is the last row
					|| rawRows[i]["Guarantor"].ToString()!=rawRows[i+1]["Guarantor"].ToString())//or if the guarantor on next line is different
				{
					//This part only happens for the last family member of a grouped family
					row=table.NewRow();
					row["address"]=rawRows[i]["guarAddress"].ToString();
					if(rawRows[i]["guarAddress2"].ToString()!="") {
						row["address"]+="\r\n"+rawRows[i]["guarAddress2"].ToString();
					}
					row["City"]=rawRows[i]["guarCity"].ToString();
					row["State"]=rawRows[i]["guarState"].ToString();
					row["Zip"]=rawRows[i]["guarZip"].ToString();
					row["clinicNum"]=rawRows[i]["guarClinicNum"].ToString();
					row["dateDue"]=PIn.Date(rawRows[i]["DateDue"].ToString()).ToShortDateString();
					row["email"]=rawRows[i]["guarEmail"].ToString();
					row["emailPatNum"]=rawRows[i]["Guarantor"].ToString();
					row["famList"]=familyAptList;
					row["guarLName"]=rawRows[i]["guarLName"].ToString();
					row["numberOfReminders"]=maxNumReminders.ToString();
					row["patientNameF"]="";//not used here
					row["patientNameFL"]="";//we won't use this
					row["patNums"]=patNumStr;
					row["recallNums"]=recallNumStr;
					rows.Add(row);
					familyAptList="";
				}	
			}
			for(int i=0;i<rows.Count;i++) {
				table.Rows.Add(rows[i]);
			}
			return table;
		}

		///<summary></summary>
		public static DataTable GetAddrTableForWebSched(List<long> recallNums,bool groupByFamily,RecallListSort sortBy) {
			//No need to check RemotingRole; no call to db.
			DataTable rawTable=GetAddrTableRaw(recallNums);
			List<DataRow> rawRows=new List<DataRow>();
			for(int i=0;i<rawTable.Rows.Count;i++) {
				rawRows.Add(rawTable.Rows[i]);
			}
			RecallComparer comparer=new RecallComparer();
			comparer.GroupByFamilies=groupByFamily;
			comparer.SortBy=sortBy;
			rawRows.Sort(comparer);
			DataTable table=new DataTable();
			table.Columns.Add("clinicNum");//will be the guar clinicNum if grouped.
			table.Columns.Add("dateDue");
			table.Columns.Add("email");//will be guar if grouped by family
			table.Columns.Add("emailPatNum");//will be guar if grouped by family
			table.Columns.Add("numberOfReminders");//for a family, this will be the max for the family
			table.Columns.Add("patientNameF");
			table.Columns.Add("patientNameFL");
			table.Columns.Add("PatNum");
			table.Columns.Add("RecallNum");
			DataRow row;
			List<DataRow> rows=new List<DataRow>();
			Patient pat;
			for(int i=0;i<rawRows.Count;i++) {
				row=table.NewRow();
				if(groupByFamily) {
					//Use guarantors clinic and email for all notifications.
					row["clinicNum"]=rawRows[i]["guarClinicNum"].ToString();
					row["email"]=rawRows[i]["guarEmail"].ToString();
					row["emailPatNum"]=rawRows[i]["Guarantor"].ToString();
				}
				else {
					row["clinicNum"]=rawRows[i]["ClinicNum"].ToString();
					row["email"]=rawRows[i]["Email"].ToString();
					row["emailPatNum"]=rawRows[i]["PatNum"].ToString();
				}
				row["dateDue"]=PIn.Date(rawRows[i]["DateDue"].ToString()).ToShortDateString();
				row["numberOfReminders"]=PIn.Long(rawRows[i]["numberOfReminders"].ToString()).ToString();
				row["PatNum"]=rawRows[i]["PatNum"].ToString();
				pat=new Patient();
				pat.LName=rawRows[i]["LName"].ToString();
				pat.FName=rawRows[i]["FName"].ToString();
				pat.Preferred=rawRows[i]["Preferred"].ToString();
				row["patientNameF"]=pat.GetNameFirstOrPreferred();
				row["patientNameFL"]=pat.GetNameFLnoPref();
				row["RecallNum"]=rawRows[i]["RecallNum"].ToString();
				rows.Add(row);
			}
			for(int i=0;i<rows.Count;i++) {
				table.Rows.Add(rows[i]);
			}
			return table;
		}

		///<summary>Gets a base table used for creating </summary>
		public static DataTable GetAddrTableRaw(List<long> recallNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),recallNums);
			}
			//get maxDateDue for each family.
			string command=@"DROP TABLE IF EXISTS temprecallmaxdate;
				CREATE table temprecallmaxdate(
					Guarantor bigint NOT NULL,
					MaxDateDue date NOT NULL,
					PRIMARY KEY (Guarantor)
				);
				INSERT INTO temprecallmaxdate 
				SELECT patient.Guarantor,MAX(recall.DateDue) maxDateDue
				FROM patient
				LEFT JOIN recall ON patient.PatNum=recall.PatNum
				AND (";
			for(int i=0;i<recallNums.Count;i++) {
				if(i>0) {
					command+=" OR ";
				}
				command+="recall.RecallNum="+POut.Long(recallNums[i]);
			}
			command+=") GROUP BY patient.Guarantor";
			Db.NonQ(command);
			command=@"SELECT patient.Address,patguar.Address guarAddress,CONCAT('',patient.BillingType) billingType,
				patient.Address2,patguar.Address2 guarAddress2,
				patient.City,patguar.City guarCity,patient.ClinicNum,patguar.ClinicNum guarClinicNum,
				recall.DateDue,patient.Email,patguar.Email guarEmail,
				patient.FName,patguar.FName guarFName,patient.Guarantor,
				patient.LName,patguar.LName guarLName,temprecallmaxdate.MaxDateDue maxDateDue,
				patient.MiddleI,
				COUNT(commlog.CommlogNum) numberOfReminders,
				patient.PatNum,patient.Preferred,recall.RecallNum,
				patient.State,patguar.State guarState,patient.Zip,patguar.Zip guarZip
				FROM recall 
				LEFT JOIN patient ON patient.PatNum=recall.PatNum 
				LEFT JOIN patient patguar ON patient.Guarantor=patguar.PatNum
				LEFT JOIN commlog ON commlog.PatNum=recall.PatNum
				AND CommType="+POut.Long(Commlogs.GetTypeAuto(CommItemTypeAuto.RECALL))+" "
				//+"AND SentOrReceived = "+POut.Long((int)CommSentOrReceived.Sent)+" "
				+"AND CommDateTime > recall.DatePrevious "
				+"LEFT JOIN temprecallmaxdate ON temprecallmaxdate.Guarantor=patient.Guarantor "
				+"WHERE ";
			for(int i=0;i<recallNums.Count;i++) {
				if(i>0) {
					command+=" OR ";
				}
				command+="recall.RecallNum="+POut.Long(recallNums[i]);
			}
			command+=@" GROUP BY patient.Address,patguar.Address,
				patient.Address2,patguar.Address2,
				patient.City,patguar.City,patient.ClinicNum,patguar.ClinicNum,
				recall.DateDue,patient.Email,patguar.Email,
				patient.FName,patguar.FName,patient.Guarantor,
				patient.LName,patguar.LName,temprecallmaxdate.MaxDateDue,
				patient.MiddleI,patient.PatNum,patient.Preferred,recall.RecallNum,
				patient.State,patguar.State,patient.Zip,patguar.Zip";
			DataTable rawTable=Db.GetTable(command);
			command="DROP TABLE IF EXISTS temprecallmaxdate";
			Db.NonQ(command);
			for(int i=0;i<rawTable.Rows.Count;i++) {
				rawTable.Rows[i]["billingType"]=DefC.GetName(DefCat.BillingTypes,PIn.Long(rawTable.Rows[i]["billingType"].ToString()));
			}
			return rawTable;
		}

		/// <summary></summary>
		public static void UpdateStatus(long recallNum,long newStatus) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),recallNum,newStatus);
				return;
			}
			string command="UPDATE recall SET RecallStatus="+newStatus.ToString()
				+" WHERE RecallNum="+recallNum.ToString();
			Db.NonQ(command);
		}

		public static int GetCountForType(long recallTypeNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetInt(MethodBase.GetCurrentMethod(),recallTypeNum);
			}
			string command="SELECT COUNT(*) FROM recall "
				+"JOIN recalltype ON recall.RecallTypeNum=recalltype.RecallTypeNum "
				+"WHERE recalltype.recallTypeNum="+POut.Long(recallTypeNum);
			return PIn.Int(Db.GetCount(command));
		}

		///<summary>Return RecallNums that have changed since a paticular time. </summary>
		public static List<long> GetChangedSinceRecallNums(DateTime changedSince) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<long>>(MethodBase.GetCurrentMethod(),changedSince);
			}
			string command="SELECT RecallNum FROM recall WHERE DateTStamp > "+POut.DateT(changedSince);
			DataTable dt=Db.GetTable(command);
			List<long> recallnums = new List<long>(dt.Rows.Count);
			for(int i=0;i<dt.Rows.Count;i++) {
				recallnums.Add(PIn.Long(dt.Rows[i]["RecallNum"].ToString()));
			}
			return recallnums;
		}

		///<summary>Returns recalls with given list of RecallNums. Used along with GetChangedSinceRecallNums.</summary>
		public static List<Recall> GetMultRecalls(List<long> recallNums) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Recall>>(MethodBase.GetCurrentMethod(),recallNums);
			}
			string strRecallNums="";
			DataTable table;
			if(recallNums.Count>0) {
				for(int i=0;i<recallNums.Count;i++) {
					if(i>0) {
						strRecallNums+="OR ";
					}
					strRecallNums+="RecallNum='"+recallNums[i].ToString()+"' ";
				}
				string command="SELECT * FROM recall WHERE "+strRecallNums;
				table=Db.GetTable(command);
			}
			else {
				table=new DataTable();
			}
			Recall[] multRecalls=Crud.RecallCrud.TableToList(table).ToArray();
			List<Recall> recallList=new List<Recall>(multRecalls);
			return recallList;
		}

		#region Web Sched
		///<summary>Makes a web service call to WebServiceCustomerUpdates to make sure this customer is signed up for Web Sched.
		///Throws exceptions if the user is not valid.
		///An ODException will have an error code; 0=no errors. 110=No Web Sched repeating charge. 120=Invalid web service response. 190=All other errors.</summary>
		public static void ValidateWebSched() {
			//Either the Web Sched service was enabled or they just enabled it.
			//Send off a web request to  WebServiceCustomersUpdates to verify that the office is still valid and is currently paying for the eService.  
			StringBuilder strbuild=new StringBuilder();
			#region Web Service Call
#if DEBUG
			OpenDentBusiness.localhost.Service1 updateService=new OpenDentBusiness.localhost.Service1();
#else
			OpenDentBusiness.customerUpdates.Service1 updateService=new OpenDentBusiness.customerUpdates.Service1();
			updateService.Url=PrefC.GetString(PrefName.UpdateServerAddress);
#endif
			if(PrefC.GetString(PrefName.UpdateWebProxyAddress) !="") {
				IWebProxy proxy = new WebProxy(PrefC.GetString(PrefName.UpdateWebProxyAddress));
				ICredentials cred=new NetworkCredential(PrefC.GetString(PrefName.UpdateWebProxyUserName),PrefC.GetString(PrefName.UpdateWebProxyPassword));
				proxy.Credentials=cred;
				updateService.Proxy=proxy;
			}
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = ("    ");
			using(XmlWriter writer=XmlWriter.Create(strbuild,settings)) {
				writer.WriteStartElement("RegistrationKey");
				writer.WriteString(PrefC.GetString(PrefName.RegistrationKey));
				writer.WriteEndElement();
			}
			#endregion
			string result="";
			try {
				result=updateService.ValidateWebSched(strbuild.ToString());
			}
			catch {
				//Do nothing.  Leaving result empty will display correct error messages later on.
			}
			ValidateWebSchedResponse(result);
		}

		///<summary>Validates the results of our ValidateWebSched web service call.
		///Returns true if they are valid otherwise throws an exception if the office is not allowed to use the Web Sched or other errors.
		///An ODException will have an error code; 0=no errors. 110=No Web Sched repeating charge. 120=Invalid web service response. 190=All other errors.</summary>
		///<param name="response">This should be the result string that was received from WebServiceCustomerUpdates.ValidateWebSched()</param>
		///<returns>True if user is an active customer and they have an active WebSched repeating charge.</returns>
		private static void ValidateWebSchedResponse(string response) {
			//No need to check RemotingRole; no call to db.
			XmlDocument doc=new XmlDocument();
			XmlNode node=null;
			try {
				doc.LoadXml(response);
				node=doc.SelectSingleNode("//ValidateWebSchedResponse");
			}
			catch {
				//Invalid web service response passed in.  Node will be null and will return false correctly.
			}
			if(node==null) {
				//There should always be a ValidateWebSchedResponse node.  If there isn't, something went wrong.
				throw new ODException(Lans.g("Recalls","Invalid web service response.  Please try again or give us a call."),120);
			}
			if(node.InnerText=="Valid") {
				return;
			}
			#region Specific Error Handling
			//At this point we know something went wrong.  So we need to give the user a hint as to why they can't enable the Web Sched.
			XmlNode nodeError=doc.SelectSingleNode("//Error");
			XmlNode nodeErrorCode=doc.SelectSingleNode("//ErrorCode");
			if(nodeError==null || nodeErrorCode==null) {
				//Something went wronger than wrong.
				throw new ODException(Lans.g("Recalls","Invalid web service response.  Please try again or give us a call."),120);
			}
			//Typical error messages will say something like: "Registration key period has ended", "Customer not registered for WebSched monthly service", etc.
			if(nodeErrorCode.InnerText=="110") {//Customer not registered for WebSched monthly service
				if(Prefs.UpdateBool(PrefName.WebSchedService,false)) {
					Signalods.SetInvalid(InvalidType.Prefs);
				}
				throw new ODException(Lans.g("Recalls","Please give us a call or visit our web page to see more information about signing up for this service."
					+"\r\n"+Recalls.GetWebSchedPromoURL()),110);
			}
			//For every other error message returned, we'll simply show it to the user.
			//Inner text can be exception text if something goes very wrong.  Do not translate.
			throw new ODException(Lans.g("Recalls","Error")+": "+nodeError.InnerText,190);
			#endregion
		}

		///<summary>Used in the eConnector service.  Honors the preferences for Web Sched automation.
		///Returns a list of errors that the eConnector needs to log.  Returns an empty list if no errors or automatic sending is off.</summary>
		public static List<string> SendAutomaticWebSchedNotifications() {
			//No need to check RemotingRole; no call to db.
			List<string> listErrors=new List<string>();
			WebSchedAutomaticSend webSchedSendSetting=(WebSchedAutomaticSend)PrefC.GetInt(PrefName.WebSchedAutomaticSendSetting);
			if(webSchedSendSetting==WebSchedAutomaticSend.DoNotSend) {
				return listErrors;//Do not flood the logs with unessecary text if they don't even have this enabled.
			}
			try {
				ValidateWebSched();
			}
			catch(Exception ex) {
				listErrors.Add(ex.Message);
				return listErrors;
			}
			DateTime fromDate=DateTime.MinValue;
			DateTime toDate=DateTime.MinValue;
			int daysPast=PrefC.GetInt(PrefName.RecallDaysPast);
			int daysFuture=PrefC.GetInt(PrefName.RecallDaysFuture);
			if(daysPast==-1) {
				fromDate=DateTime.MinValue;
			}
			else {
				fromDate=DateTime.Today.AddDays(-daysPast);
			}
			if(daysFuture==-1) {
				toDate=DateTime.MaxValue;
			}
			else {
				toDate=DateTime.Today.AddDays(daysFuture);
			}
			long provNum=0;
			long clinicNum=0;
			long siteNum=0;
			List<long> listRecallNums=new List<long>();
			DataTable table=Recalls.GetRecallList(fromDate,toDate,PrefC.GetBool(PrefName.RecallGroupByFamily),provNum,clinicNum,siteNum,RecallListSort.Alphabetical,RecallListShowNumberReminders.All,listRecallNums);
			for(int i=0;i<table.Rows.Count;i++) {
				Patient patCur=Patients.GetPat(PIn.Long(table.Rows[i]["PatNum"].ToString()));
				if(patCur==null) {
					continue;//Should never happen.  If it does, we obviously can't send an email to a null patient.
				}
				if(patCur.Email=="") {//Can't send emails to a patient with no email set.
					continue;
				}
				if(webSchedSendSetting==WebSchedAutomaticSend.SendToEmailNoPreferred 
					&& patCur.PreferRecallMethod!=ContactMethod.None 
					&& patCur.PreferRecallMethod!=ContactMethod.Email) 
				{
					continue;//The patient has a preferred recall contact method set and it isn't email.
				}
				if(webSchedSendSetting==WebSchedAutomaticSend.SendToEmailOnlyPreferred && patCur.PreferRecallMethod!=ContactMethod.Email) {
					continue;//The patient's preferred recall contact method isn't set or is set to a contact method other than email.
				}
				long recallNum=PIn.Long(table.Rows[i]["RecallNum"].ToString());
				DateTime dateDue=PIn.Date(table.Rows[i]["DateDue"].ToString());
				if(dateDue.Year < 1880) {
					continue;//We should never send automatic Web Sched notifications for patients that do not have a valid due date.
				}
				//The patient has a valid email address, check to see if they'll have any potential time slots via their Web Sched link.
				if(dateDue.Date<=DateTime.Now.Date) {
					dateDue=DateTime.Now;
				}
				DateTime dateEnd=dateDue.AddMonths(2);
				//This takes a long time to run for lots of recalls.  Might consider making a faster overload in the future (213 recalls ~ 10 seconds).
				bool hasTimeSlots=false;
				try {
					hasTimeSlots=(OpenDentBusiness.WebTypes.WebSched.TimeSlot.TimeSlots.GetAvailableWebSchedTimeSlots(recallNum,dateDue,dateEnd).Count > 0);
				}
				catch(Exception) {
				}
				if(hasTimeSlots) {
					listRecallNums.Add(recallNum);
				}
			}
			if(listRecallNums.Count==0) {
				return listErrors;
			}
			return SendWebSchedNotifications(listRecallNums,PrefC.GetBool(PrefName.RecallGroupByFamily),RecallListSort.Alphabetical);
		}

		///<summary>Makes several web service calls to WebServiceCustomersUpdates in order to get Web Sched URLs.
		///Returns a list of errors to display to the user if anything went wrong otherwise returns empty list if everything was successful.</summary>
		public static List<string> SendWebSchedNotifications(List<long> recallNums,bool isGroupFamily,RecallListSort sortBy,EmailAddress emailAddressOverride=null) {
			//No need to check RemotingRole; no call to db.
			string response="";
			List<string> listErrors=new List<string>();
			Dictionary<long,string> dictWebSchedParameters=new Dictionary<long,string>();
			//Send off a web request to WebServiceMainHQ to get the obfuscated URLs for the selected patients.
			#region Send Web Service Request For URLs
			try {
				response=WebServiceMainHQProxy.GetWebServiceMainHQInstance()
					.BuildWebSchedRecallURLs(PrefC.GetString(PrefName.RegistrationKey),String.Join("|",recallNums));
			}
			catch {
				//Do nothing.  Leaving result empty will display correct error messages later on.
			}
			#endregion
			#region Parse Response
			XmlDocument doc=new XmlDocument();
			XmlNode nodeError=null;
			XmlNode nodeResponse=null;
			XmlNodeList nodeURLs=null;
			try {
				doc.LoadXml(response);
				nodeError=doc.SelectSingleNode("//Error");
				nodeResponse=doc.SelectSingleNode("//GetWebSchedURLsResponse");
			}
			catch {
				//Invalid web service response passed in.  Node will be null and will return false correctly.
			}
			#region Error Handling
			if(nodeError!=null || nodeResponse==null) {
				string error=Lans.g("WebSched","There was an error with the web request.  Please try again or give us a call.");
				//Either something went wrong or someone tried to get cute and use our Web Sched service when they weren't supposed to.
				if(nodeError!=null) {
					error+="\r\n"+Lans.g("WebSched","Error Details")+":\r\n" +nodeError.InnerText;
				}
				listErrors.Add(error);
				return listErrors;
			}
			#endregion
			//At this point we know we got a valid response from our web service.
			dictWebSchedParameters.Clear();
			nodeURLs=doc.GetElementsByTagName("URL");
			if(nodeURLs!=null) {
				//Loop through all the URL nodes that were returned.
				//Each URL node will contain an RN attribute which will be the corresponding recall num.
				for(int i=0;i<nodeURLs.Count;i++) {
					long recallNum=0;
					XmlAttribute attributeRecallNum=nodeURLs[i].Attributes["RN"];
					if(attributeRecallNum!=null) {
						recallNum=PIn.Long(attributeRecallNum.Value);
					}
					dictWebSchedParameters.Add(recallNum,nodeURLs[i].InnerText);
				}
			}
			#endregion
			//Now that the web service response has been validated, parsed, and our dictionary filled, we now can loop through the selected patients and send off the emails.
			DataTable addrTable=Recalls.GetAddrTableForWebSched(recallNums,isGroupFamily,sortBy);
			EmailMessage emailMessage;
			EmailAddress emailAddress;
			for(int i=0;i<addrTable.Rows.Count;i++) {
				#region Send Email Notification
				string emailBody="";
				string emailSubject="";
				emailMessage=new EmailMessage();
				emailMessage.PatNum=PIn.Long(addrTable.Rows[i]["emailPatNum"].ToString());
				emailMessage.ToAddress=PIn.String(addrTable.Rows[i]["email"].ToString());//might be guarantor email
				if(emailAddressOverride!=null) {
					emailAddress=emailAddressOverride;
				}
				else {
					emailAddress=EmailAddresses.GetByClinic(PIn.Long(addrTable.Rows[i]["ClinicNum"].ToString()));
				}
				emailMessage.FromAddress=emailAddress.SenderAddress;
				if(addrTable.Rows[i]["numberOfReminders"].ToString()=="0") {
					emailSubject=PrefC.GetString(PrefName.WebSchedSubject);
					emailBody=PrefC.GetString(PrefName.WebSchedMessage);
				}
				else if(addrTable.Rows[i]["numberOfReminders"].ToString()=="1") {
					emailSubject=PrefC.GetString(PrefName.WebSchedSubject2);
					emailBody=PrefC.GetString(PrefName.WebSchedMessage2);
				}
				else {
					emailSubject=PrefC.GetString(PrefName.WebSchedSubject3);
					emailBody=PrefC.GetString(PrefName.WebSchedMessage3);
				}
				emailSubject=emailSubject.Replace("[NameF]",addrTable.Rows[i]["patientNameF"].ToString());
				//It is common for offices to have paitents with a blank recall date (they've never had a recall performed at the office).
				//Instead of showing 01/01/0001 in the email, we will simply show today's date because that is what the Web Sched time slots will start showing.
				DateTime dateDue=PIn.Date(addrTable.Rows[i]["dateDue"].ToString());
				if(dateDue.Year < 1880) {
					dateDue=DateTime.Today;
				}
				emailBody=emailBody.Replace("[DueDate]",dateDue.ToShortDateString());
				emailBody=emailBody.Replace("[NameF]",addrTable.Rows[i]["patientNameF"].ToString());
				string URL="";
				try {
					dictWebSchedParameters.TryGetValue(PIn.Long(addrTable.Rows[i]["RecallNum"].ToString()),out URL);
				}
				catch(Exception ex) {
					string error=ex.Message+"\r\n"
						+Lans.g("WebSched","Problem getting Web Sched URL for patient")+": "+addrTable.Rows[i]["patientNameFL"].ToString();
					listErrors.Add(error);
					continue;
				}
				emailBody=emailBody.Replace("[URL]",URL);
				string officePhone=PrefC.GetString(PrefName.PracticePhone);
				Clinic clinic=Clinics.GetClinic(PIn.Long(addrTable.Rows[i]["clinicNum"].ToString()));
				if(clinic!=null && !String.IsNullOrEmpty(clinic.Phone)) {
					officePhone=clinic.Phone;
				}
				if(CultureInfo.CurrentCulture.Name=="en-US" && officePhone.Length==10) {
					officePhone="("+officePhone.Substring(0,3)+")"+officePhone.Substring(3,3)+"-"+officePhone.Substring(6);
				}
				emailBody=emailBody.Replace("[OfficePhone]",officePhone);
				emailMessage.Subject=emailSubject;
				emailMessage.BodyText=emailBody;
				try {
					EmailMessages.SendEmailUnsecure(emailMessage,emailAddress);
				}
				catch(Exception ex) {
					string error=ex.Message+"\r\n";
					if(ex.GetType()==typeof(System.ArgumentException)) {
						error+=Lans.g("WebSched","Go to Setup | Appointments | Recall.  The subject for WebSched notifications must not span multiple lines.")+"\r\n";
					}
					error+=Lans.g("WebSched","Patient")+": "+addrTable.Rows[i]["patientNameFL"].ToString();
					listErrors.Add(error);
					continue;
				}
				emailMessage.MsgDateTime=DateTime.Now;
				emailMessage.SentOrReceived=EmailSentOrReceived.Sent;
				EmailMessages.Insert(emailMessage);
				#endregion
				#region Insert Commlog
				long userNum=0;//For Web Sched
				if(Security.CurUser!=null) {
					userNum=Security.CurUser.UserNum;//Middle tier should not be using this method, it does not call the db.
				}
				Commlogs.InsertForRecall(PIn.Long(addrTable.Rows[i]["PatNum"].ToString()),CommItemMode.Email,PIn.Int(addrTable.Rows[i]["numberOfReminders"].ToString()),
					PrefC.GetLong(PrefName.RecallStatusEmailed),CommItemSource.WebSched,userNum);
				Recalls.UpdateStatus(PIn.Long(addrTable.Rows[i]["RecallNum"].ToString()),PrefC.GetLong(PrefName.RecallStatusEmailed));
				#endregion
			}
			return listErrors;
		}

		///<summary>Creates and inserts an appointment for the recall passed in using the dateStart hour as the beginning of the appointment.
		///It will be scheduled in the first available operatory.
		///<para>The first available operatory is determined by the order in which they are stored in the database (operatory.ItemOrder).</para>
		///<para>This means that (visually to the user) we will be filling up their appointment schedule from the left to the right.</para>
		///<para>Surround with a try catch.  Throws exceptions if anything goes wrong.</para>
		///<para>Returns the list of procedures that were scheduled on the appointment created.</para></summary>
		public static List<Procedure> CreateRecallApptForWebSched(long recallNum,DateTime dateStart,DateTime dateEnd
			,List<TimeSlot> listAvailableTimeSlots) 
		{
			//No need to check RemotingRole; no call to db.
			foreach(TimeSlot timeSlot in listAvailableTimeSlots) {
				if(dateStart!=timeSlot.DateTimeStart || dateEnd!=timeSlot.DateTimeStop) {
					continue;//Not the available slot that the patient selected within the app.
				}
				//At this point we know the slot time that the patient selected matches this open time slot.
				Recall recallCur=Recalls.GetRecall(recallNum);
				if(recallCur==null) {
					throw new ODException("This recall appointment is no longer available.\r\nPlease call us to schedule your appointment.");
				}
				Patient patCur=Patients.GetPat(recallCur.PatNum);
				List<Recall> listRecalls=Recalls.GetList(patCur.PatNum);
				for(int j=0;j<listRecalls.Count;j++) {
					if(listRecalls[j].RecallNum==recallNum) {
						recallCur=listRecalls[j].Copy();
						break;
					}
				}
				Appointment aptCur=new Appointment();
				Family fam=Patients.GetFamily(patCur.PatNum);
				List<Procedure> procList=Procedures.Refresh(patCur.PatNum);
				List<InsSub> listSubs=InsSubs.RefreshForFam(fam);
				List<InsPlan> listPlans=InsPlans.RefreshForSubList(listSubs);
				List<string> listProcStrs=RecallTypes.GetProcs(recallCur.RecallTypeNum);
				//Now we need to completely fill the appointment with procedures, claimprocs, etc. for this specific recall.
				List<Procedure> listProcedures=Appointments.FillAppointmentForRecall(aptCur,recallCur,listRecalls,patCur,listProcStrs,listPlans,listSubs);
				Appointment aptOld=aptCur.Copy();
				//Take the recall appointment that was just inserted via FillAppointmentForRecall() and update the time and operatory.
				Operatory opCur=Operatories.GetOperatory(timeSlot.OperatoryNum);
				aptCur.AptStatus=ApptStatus.Scheduled;
				aptCur.AptDateTime=dateStart;
				aptCur.Op=opCur.OperatoryNum;
				//Make sure that operatory specific settings are applied to the appointment.
				List<Schedule> listSchedules=Schedules.RefreshDayEdit(aptCur.AptDateTime);
				long assignedDent=Schedules.GetAssignedProvNumForSpot(listSchedules,opCur,false,aptCur.AptDateTime);
				long assignedHyg=Schedules.GetAssignedProvNumForSpot(listSchedules,opCur,true,aptCur.AptDateTime);
				if(assignedDent > 0) {//if no dentist is assigned to op, then keep the original dentist.  All appts must have prov.
					aptCur.ProvNum=assignedDent;
				}
				if(assignedHyg > 0) {
					aptCur.ProvHyg=assignedHyg;
				}
				aptCur.IsHygiene=opCur.IsHygiene;
				if(opCur.ClinicNum==0) {
					aptCur.ClinicNum=patCur.ClinicNum;
				}
				else {
					aptCur.ClinicNum=opCur.ClinicNum;
				}
				//Note: We do not need to do any prospective operatory checks here because the query currently excludes prospective ops.
				//Also, aptCur already has the correct time pattern set.  No need to set it again here.
				Appointments.Update(aptCur,aptOld);
				//Create a security log so that the office knows where this appointment came from.
				SecurityLogs.MakeLogEntry(Permissions.AppointmentCreate,aptCur.PatNum,
					aptCur.AptDateTime.ToString()+", "+aptCur.ProcDescript+"  -  Created via Web Sched",
					aptCur.AptNum,LogSources.WebSched);
				//There is no need to make security logs for anything other than the appointment.  That is how the recall list system currently does it.
				Recalls.SynchScheduledApptFull(aptCur.PatNum);//Synch the recalls so that the appointment will disappear from the recall list.
				return listProcedures;
			}
			//It is very possible that from the time the patient loaded the Web Sched app and now that the available time slot has been removed or filled.
			throw new ODException("The selected appointment time is no longer available.\r\nPlease choose a different time slot.",100);
		}
		#endregion
	}

	///<summary>The supplied DataRows must include the following columns: 
	///Guarantor, PatNum, guarLName, guarFName, LName, FName, DateDue, maxDateDue, billingType.  
	///maxDateDue is the most recent DateDue for all family members in the list and needs to be the same for all family members.  
	///This date will be used for better grouping.</summary>
	class RecallComparer:IComparer<DataRow> {
		public bool GroupByFamilies;
		///<summary>rather than by the ordinary DueDate.</summary>
		public RecallListSort SortBy;

		///<summary></summary>
		public int Compare(DataRow x,DataRow y) {
			//NOTE: Even if grouping by families, each family is not necessarily going to have a guarantor.
			if(GroupByFamilies) {
				if(SortBy==RecallListSort.Alphabetical) {
					//if guarantors are different, sort by guarantor name
					if(x["Guarantor"].ToString() != y["Guarantor"].ToString()) {
						if(x["guarLName"].ToString() != y["guarLName"].ToString()) {
							return x["guarLName"].ToString().CompareTo(y["guarLName"].ToString());
						}
						return x["guarFName"].ToString().CompareTo(y["guarFName"].ToString());
					}
					return 0;//order within family does not matter
				}
				else if(SortBy==RecallListSort.DueDate) {
					DateTime xD=PIn.Date(x["maxDateDue"].ToString());
					DateTime yD=PIn.Date(y["maxDateDue"].ToString());
					if(xD != yD) {
						return (xD.CompareTo(yD));
					}
					//if dates are same, sort/group by guarantor
					if(x["Guarantor"].ToString() != y["Guarantor"].ToString()) {
						return (x["Guarantor"].ToString().CompareTo(y["Guarantor"].ToString()));
					}
					//within the same family, sort by actual DueDate
					xD=PIn.Date(x["DateDue"].ToString());
					yD=PIn.Date(y["DateDue"].ToString());
					return (xD.CompareTo(yD));
					//return 0;
				}
				else if(SortBy==RecallListSort.BillingType){
					if(x["billingType"].ToString()!=y["billingType"].ToString()){
						return x["billingType"].ToString().CompareTo(y["billingType"].ToString());
					}
					//if billing types are the same, sort by dueDate
					DateTime xD=PIn.Date(x["maxDateDue"].ToString());
					DateTime yD=PIn.Date(y["maxDateDue"].ToString());
					if(xD != yD) {
						return (xD.CompareTo(yD));
					}
					//if dates are same, sort/group by guarantor
					if(x["Guarantor"].ToString() != y["Guarantor"].ToString()) {
						return (x["Guarantor"].ToString().CompareTo(y["Guarantor"].ToString()));
					}
				}
			}
			else {//individual patients
				if(SortBy==RecallListSort.Alphabetical) {
					if(x["LName"].ToString() != y["LName"].ToString()) {
						return x["LName"].ToString().CompareTo(y["LName"].ToString());
					}
					return x["FName"].ToString().CompareTo(y["FName"].ToString());
				}
				else if(SortBy==RecallListSort.DueDate) {
					if((DateTime)x["DateDue"] != (DateTime)y["DateDue"]) {
						return ((DateTime)x["DateDue"]).CompareTo(((DateTime)y["DateDue"]));
					}
					//if duedates are the same, sort by LName
					return x["LName"].ToString().CompareTo(y["LName"].ToString());
				}
				else if(SortBy==RecallListSort.BillingType){
					if(x["billingType"].ToString()!=y["billingType"].ToString()){
						return x["billingType"].ToString().CompareTo(y["billingType"].ToString());
					}
					//if billing types are the same, sort by dueDate
					if((DateTime)x["DateDue"] != (DateTime)y["DateDue"]) {
						return ((DateTime)x["DateDue"]).CompareTo(((DateTime)y["DateDue"]));
					}
					//if duedates are the same, sort by LName
					return x["LName"].ToString().CompareTo(y["LName"].ToString());
				}
			}
			return 0;
		}




	}

	public enum RecallListShowNumberReminders {
		All,
		Zero,
		One,
		Two,
		Three,
		Four,
		Five,
		SixPlus
	}

	public enum RecallListSort{
		DueDate,
		Alphabetical,
		BillingType
	}
	

}









