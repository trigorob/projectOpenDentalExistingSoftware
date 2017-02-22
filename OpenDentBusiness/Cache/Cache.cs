using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenDentBusiness {
	public class Cache {
		/// <summary>This is only used in the RefreshCache methods.  Used instead of Meth.  The command is only used if not ClientWeb.</summary>
		public static DataTable GetTableRemotelyIfNeeded(MethodBase methodBase,string command) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(methodBase);
			}
			else {
				return Db.GetTable(command);
			}
		}
		 
		///<summary>itypesStr= comma-delimited list of int.  Called directly from UI in one spot.  Called from above repeatedly.  The end result is that both server and client have been properly refreshed.</summary>
		public static void Refresh(params InvalidType[] arrayITypes) {
			DataSet ds=GetCacheDs(arrayITypes);
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				//Because otherwise it was handled just fine as part of the GetCacheDs
				FillCache(ds,arrayITypes);
			}
		}

		///<summary>This is an incomplete stub and should not be used very much yet.  This will get used when switching databases.  Switching databases is allowed from ClientWeb in the sense that the user can connect to a different server from the ChooseDatabase window.</summary>
		public static void ClearAllCache() {
			//AccountingAutoPays
			AccountingAutoPays.ClearCache();
			//AutoCodes
			AutoCodes.ClearCache();
			AutoCodeItems.ClearCache();
			AutoCodeConds.ClearCache();
			//etc...



			Prefs.ClearCache();
			//etc...


		}

		///<summary>If ClientWeb, then this method is instead run on the server, and the result passed back to the client.  And since it's ClientWeb, FillCache will be run on the client.</summary>
		public static DataSet GetCacheDs(params InvalidType[] arrayITypes) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetDS(MethodBase.GetCurrentMethod(),arrayITypes);
			}
			List<InvalidType> listITypes=arrayITypes.ToList();
			//so this part below only happens if direct or server------------------------------------------------
			bool isAll=false;
			if(listITypes.Contains(InvalidType.AllLocal)) {
				isAll=true;
			}
			DataSet ds=new DataSet();
			//All Internal OD Tables that are cached go here
			if(PrefC.IsODHQ) {
				if(listITypes.Contains(InvalidType.JobPermission) || isAll) {
					ds.Tables.Add(JobPermissions.RefreshCache());
				}
			}
			//All cached public tables go here
			if(listITypes.Contains(InvalidType.AccountingAutoPays) || isAll) {
				ds.Tables.Add(AccountingAutoPays.RefreshCache());
			}
			//if(listITypes.Contains(InvalidType.AlertItems) || isAll) {//THIS IS NOT CACHED. But is used to make server run the alert logic in OpenDentalService.
			//	ds.Tables.Add(AlertItems.RefreshCache());
			//}
			if(listITypes.Contains(InvalidType.AlertSubs) || isAll) {
				ds.Tables.Add(AlertSubs.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.AppointmentTypes) || isAll) {
				ds.Tables.Add(AppointmentTypes.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.AutoCodes) || isAll) {
				ds.Tables.Add(AutoCodes.RefreshCache());
				ds.Tables.Add(AutoCodeItems.RefreshCache());
				ds.Tables.Add(AutoCodeConds.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Automation) || isAll) {
				ds.Tables.Add(Automations.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.AutoNotes) || isAll) {
				ds.Tables.Add(AutoNotes.RefreshCache());
				ds.Tables.Add(AutoNoteControls.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Carriers) || isAll) {
				ds.Tables.Add(Carriers.RefreshCache());//run on startup, after telephone reformat, after list edit.
			}
			if(listITypes.Contains(InvalidType.ClaimForms) || isAll) {
				ds.Tables.Add(ClaimFormItems.RefreshCache());
				ds.Tables.Add(ClaimForms.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.ClearHouses) || isAll) {
				ds.Tables.Add(Clearinghouses.RefreshCacheHq());//kh wants to add an EasyHideClearHouses to disable this
			}
			//InvalidType.Clinics see InvalidType.Providers
			if(listITypes.Contains(InvalidType.Computers) || isAll) {
				ds.Tables.Add(Computers.RefreshCache());
				ds.Tables.Add(Printers.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Defs) || isAll) {
				ds.Tables.Add(Defs.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.DentalSchools) || isAll) {
				ds.Tables.Add(SchoolClasses.RefreshCache());
				ds.Tables.Add(SchoolCourses.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.DictCustoms) || isAll) {
				ds.Tables.Add(DictCustoms.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Diseases) || isAll) {
				ds.Tables.Add(DiseaseDefs.RefreshCache());
				ds.Tables.Add(ICD9s.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.DisplayFields) || isAll) {
				ds.Tables.Add(DisplayFields.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Ebills) || isAll) {
				ds.Tables.Add(Ebills.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.EhrCodes)) {
				EhrCodes.UpdateList();//Unusual pattern for an unusual "table".  Not really a table, but a mishmash of hard coded partial code systems that are needed for CQMs.
			}
			if(listITypes.Contains(InvalidType.ElectIDs) || isAll) {
				ds.Tables.Add(ElectIDs.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Email) || isAll) {
				ds.Tables.Add(EmailAddresses.RefreshCache());
				ds.Tables.Add(EmailTemplates.RefreshCache());
				ds.Tables.Add(EmailAutographs.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Employees) || isAll) {
				ds.Tables.Add(Employees.RefreshCache());
				ds.Tables.Add(PayPeriods.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Employers) || isAll) {
				ds.Tables.Add(Employers.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Fees) || isAll) {
				ds.Tables.Add(Fees.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.FeeScheds) || isAll) {
				ds.Tables.Add(FeeScheds.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.HL7Defs) || isAll) {
				ds.Tables.Add(HL7Defs.RefreshCache());
				ds.Tables.Add(HL7DefMessages.RefreshCache());
				ds.Tables.Add(HL7DefSegments.RefreshCache());
				ds.Tables.Add(HL7DefFields.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.InsCats) || isAll) {
				ds.Tables.Add(CovCats.RefreshCache());
				ds.Tables.Add(CovSpans.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.InsFilingCodes) || isAll) {
				ds.Tables.Add(InsFilingCodes.RefreshCache());
				ds.Tables.Add(InsFilingCodeSubtypes.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Languages) || isAll) {
				if(CultureInfo.CurrentCulture.Name!="en-US") {
					ds.Tables.Add(Lans.RefreshCache());
				}
			}
			if(listITypes.Contains(InvalidType.Letters) || isAll) {
				ds.Tables.Add(Letters.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.LetterMerge) || isAll) {
				ds.Tables.Add(LetterMergeFields.RefreshCache());
				ds.Tables.Add(LetterMerges.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Medications) || isAll) {
				ds.Tables.Add(Medications.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Operatories) || isAll) {
				ds.Tables.Add(Operatories.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.OrthoChartTabs) || isAll) {
				ds.Tables.Add(OrthoChartTabs.RefreshCache());
				ds.Tables.Add(OrthoChartTabLinks.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.PatFields) || isAll) {
				ds.Tables.Add(PatFieldDefs.RefreshCache());
				ds.Tables.Add(ApptFieldDefs.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Pharmacies) || isAll) {
				ds.Tables.Add(Pharmacies.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Prefs) || isAll) {
				ds.Tables.Add(Prefs.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.ProcButtons) || isAll) {
				ds.Tables.Add(ProcButtons.RefreshCache());
				ds.Tables.Add(ProcButtonItems.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.ProcCodes) || isAll) {
				ds.Tables.Add(ProcedureCodes.RefreshCache());
				ds.Tables.Add(ProcCodeNotes.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Programs) || isAll) {
				ds.Tables.Add(Programs.RefreshCache());
				ds.Tables.Add(ProgramProperties.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.ProviderErxs) || isAll) {
				ds.Tables.Add(ProviderErxs.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.ProviderIdents) || isAll) {
				ds.Tables.Add(ProviderIdents.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Providers) || isAll) {
				ds.Tables.Add(Providers.RefreshCache());
				//Refresh the clinics as well because InvalidType.Providers has a comment that says "also includes clinics".  Also, there currently isn't an itype for Clinics.
				ds.Tables.Add(Clinics.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.QuickPaste) || isAll) {
				ds.Tables.Add(QuickPasteNotes.RefreshCache());
				ds.Tables.Add(QuickPasteCats.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.RecallTypes) || isAll) {
				ds.Tables.Add(RecallTypes.RefreshCache());
				ds.Tables.Add(RecallTriggers.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.ReplicationServers) || isAll) {
				ds.Tables.Add(ReplicationServers.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.RequiredFields) || isAll) {
				ds.Tables.Add(RequiredFields.RefreshCache());
				ds.Tables.Add(RequiredFieldConditions.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Security) || isAll) {
				ds.Tables.Add(Userods.RefreshCache());
				ds.Tables.Add(UserGroups.RefreshCache());
				ds.Tables.Add(GroupPermissions.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Sheets) || isAll) {
				ds.Tables.Add(SheetDefs.RefreshCache());
				ds.Tables.Add(SheetFieldDefs.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.SigMessages) || isAll) {
				ds.Tables.Add(SigElementDefs.RefreshCache());
				ds.Tables.Add(SigButDefs.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Sites) || isAll) {
				ds.Tables.Add(Sites.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Sops) || isAll) {  //InvalidType.Sops is currently never used 11/14/2014
				ds.Tables.Add(Sops.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.StateAbbrs) || isAll) {
				ds.Tables.Add(StateAbbrs.RefreshCache());
			}
			//InvalidTypes.Tasks not handled here.
			if(listITypes.Contains(InvalidType.TimeCardRules) || isAll) {
				ds.Tables.Add(TimeCardRules.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.ToolBut) || isAll) {
				ds.Tables.Add(ToolButItems.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.UserClinics) || isAll) {
				ds.Tables.Add(UserClinics.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Vaccines) || isAll) {
				ds.Tables.Add(VaccineDefs.RefreshCache());
				ds.Tables.Add(DrugManufacturers.RefreshCache());
				ds.Tables.Add(DrugUnits.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Views) || isAll) {
				ds.Tables.Add(ApptViews.RefreshCache());
				ds.Tables.Add(ApptViewItems.RefreshCache());
				ds.Tables.Add(AppointmentRules.RefreshCache());
				ds.Tables.Add(ProcApptColors.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.Wiki) || isAll) {
				ds.Tables.Add(WikiListHeaderWidths.RefreshCache());
				ds.Tables.Add(WikiPages.RefreshCache());
			}
			if(listITypes.Contains(InvalidType.ZipCodes) || isAll) {
				ds.Tables.Add(ZipCodes.RefreshCache());
			}
			return ds;
		}

		///<summary>only if ClientWeb</summary>
		public static void FillCache(DataSet ds,params InvalidType[] arrayITypes) {
			List<InvalidType> listITypes=arrayITypes.ToList();
			bool isAll=false;
			if(listITypes.Contains(InvalidType.AllLocal)) {
				isAll=true;
			}
			//All Internal OD Tables that are cached go here
			if(PrefC.IsODHQ) {
				if(listITypes.Contains(InvalidType.JobPermission) || isAll) {
					ds.Tables.Add(JobPermissions.RefreshCache());
				}
			}
			if(listITypes.Contains(InvalidType.AccountingAutoPays) || isAll) {
				AccountingAutoPays.FillCache(ds.Tables["AccountingAutoPay"]);
			}
			//if(listITypes.Contains(InvalidType.AlertItems) || isAll) {//THIS IS NOT CACHED. But is used to make server run the alert logic in OpenDentalService.
			//	AlertSubs.FillCache(ds.Tables["AlertItem"]);
			//}
			if(listITypes.Contains(InvalidType.AlertSubs) || isAll) {
				AlertSubs.FillCache(ds.Tables["AlertSub"]);
			}
			if(listITypes.Contains(InvalidType.AppointmentTypes) || isAll) {
				AppointmentTypes.FillCache(ds.Tables["AppointmentType"]);
			}
			if(listITypes.Contains(InvalidType.AutoCodes) || isAll) {
				AutoCodes.FillCache(ds.Tables["AutoCode"]);
				AutoCodeItems.FillCache(ds.Tables["AutoCodeItem"]);
				AutoCodeConds.FillCache(ds.Tables["AutoCodeCond"]);
			}
			if(listITypes.Contains(InvalidType.Automation) || isAll) {
				Automations.FillCache(ds.Tables["Automation"]);
			}
			if(listITypes.Contains(InvalidType.AutoNotes) || isAll) {
				AutoNotes.FillCache(ds.Tables["AutoNote"]);
				AutoNoteControls.FillCache(ds.Tables["AutoNoteControl"]);
			}
			if(listITypes.Contains(InvalidType.Carriers) || isAll) {
				Carriers.FillCache(ds.Tables["Carrier"]);//run on startup, after telephone reformat, after list edit.
			}
			if(listITypes.Contains(InvalidType.ClaimForms) || isAll) {
				ClaimFormItems.FillCache(ds.Tables["ClaimFormItem"]);
				ClaimForms.FillCache(ds.Tables["ClaimForm"]);
			}
			if(listITypes.Contains(InvalidType.ClearHouses) || isAll) {
				Clearinghouses.FillCacheHq(ds.Tables["Clearinghouse"]);//kh wants to add an EasyHideClearHouses to disable this
			}
			if(listITypes.Contains(InvalidType.Computers) || isAll) {
				Computers.FillCache(ds.Tables["Computer"]);
				Printers.FillCache(ds.Tables["Printer"]);
			}
			if(listITypes.Contains(InvalidType.Defs) || isAll) {
				Defs.FillCache(ds.Tables["Def"]);
			}
			if(listITypes.Contains(InvalidType.DentalSchools) || isAll) {
				SchoolClasses.FillCache(ds.Tables["SchoolClass"]);
				SchoolCourses.FillCache(ds.Tables["SchoolCourse"]);
			}
			if(listITypes.Contains(InvalidType.DictCustoms) || isAll) {
				DictCustoms.FillCache(ds.Tables["DictCustom"]);
			}
			if(listITypes.Contains(InvalidType.Diseases) || isAll) {
				DiseaseDefs.FillCache(ds.Tables["DiseaseDef"]);
				ICD9s.FillCache(ds.Tables["ICD9"]);
			}
			if(listITypes.Contains(InvalidType.DisplayFields) || isAll) {
				DisplayFields.FillCache(ds.Tables["DisplayField"]);
			}
			if(listITypes.Contains(InvalidType.Ebills) || isAll) {
				Ebills.FillCache(ds.Tables["Ebill"]);
			}
			if(listITypes.Contains(InvalidType.ElectIDs) || isAll) {
				ElectIDs.FillCache(ds.Tables["ElectID"]);
			}
			if(listITypes.Contains(InvalidType.Email) || isAll) {
				EmailAddresses.FillCache(ds.Tables["EmailAddress"]);
				EmailTemplates.FillCache(ds.Tables["EmailTemplate"]);
			}
			if(listITypes.Contains(InvalidType.Employees) || isAll) {
				Employees.FillCache(ds.Tables["Employee"]);
				PayPeriods.FillCache(ds.Tables["PayPeriod"]);
			}
			if(listITypes.Contains(InvalidType.Employers) || isAll) {
				Employers.FillCache(ds.Tables["Employer"]);
			}
			if(listITypes.Contains(InvalidType.Fees) || isAll) {
				Fees.FillCache(ds.Tables["Fee"]);
			}
			if(listITypes.Contains(InvalidType.FeeScheds) || isAll) {
				FeeScheds.FillCache(ds.Tables["FeeSched"]);
			}
			if(listITypes.Contains(InvalidType.HL7Defs) || isAll) {
				HL7Defs.FillCache(ds.Tables["HL7Def"]);
				HL7DefMessages.FillCache(ds.Tables["HL7DefMessage"]);
				HL7DefSegments.FillCache(ds.Tables["HL7DefSegment"]);
				HL7DefFields.FillCache(ds.Tables["HL7DefField"]);
			}
			if(listITypes.Contains(InvalidType.InsCats) || isAll) {
				CovCats.FillCache(ds.Tables["CovCat"]);
				CovSpans.FillCache(ds.Tables["CovSpan"]);
			}
			if(listITypes.Contains(InvalidType.InsFilingCodes) || isAll) {
				InsFilingCodes.FillCache(ds.Tables["InsFilingCode"]);
				InsFilingCodeSubtypes.FillCache(ds.Tables["InsFilingCodeSubtype"]);
			}
			if(listITypes.Contains(InvalidType.Languages) || isAll) {
				Lans.FillCache(ds.Tables["Language"]);
			}
			if(listITypes.Contains(InvalidType.Letters) || isAll) {
				Letters.FillCache(ds.Tables["Letter"]);
			}
			if(listITypes.Contains(InvalidType.LetterMerge) || isAll) {
				LetterMergeFields.FillCache(ds.Tables["LetterMergeField"]);
				LetterMerges.FillCache(ds.Tables["LetterMerge"]);
			}
			if(listITypes.Contains(InvalidType.Medications) || isAll) {
				Medications.FillCache(ds.Tables["Medications"]);
			}
			if(listITypes.Contains(InvalidType.Operatories) || isAll) {
				Operatories.FillCache(ds.Tables["Operatory"]);
			}
			if(listITypes.Contains(InvalidType.OrthoChartTabs) || isAll) {
				OrthoChartTabs.FillCache(ds.Tables["OrthoChartTab"]);
				OrthoChartTabLinks.FillCache(ds.Tables["OrthoChartTabLink"]);
			}
			if(listITypes.Contains(InvalidType.PatFields) || isAll) {
				PatFieldDefs.FillCache(ds.Tables["PatFieldDef"]);
				ApptFieldDefs.FillCache(ds.Tables["ApptFieldDef"]);
			}
			if(listITypes.Contains(InvalidType.Pharmacies) || isAll) {
				Pharmacies.FillCache(ds.Tables["Pharmacy"]);
			}
			if(listITypes.Contains(InvalidType.Prefs) || isAll) {
				Prefs.FillCache(ds.Tables["Pref"]);
			}
			if(listITypes.Contains(InvalidType.ProcButtons) || isAll) {
				ProcButtons.FillCache(ds.Tables["ProcButton"]);
				ProcButtonItems.FillCache(ds.Tables["ProcButtonItem"]);
			}
			if(listITypes.Contains(InvalidType.ProcCodes) || isAll) {
				ProcedureCodes.FillCache(ds.Tables["ProcedureCode"]);
				ProcCodeNotes.FillCache(ds.Tables["ProcCodeNote"]);
			}
			if(listITypes.Contains(InvalidType.Programs) || isAll) {
				Programs.FillCache(ds.Tables["Program"]);
				ProgramProperties.FillCache(ds.Tables["ProgramProperty"]);
			}
			if(listITypes.Contains(InvalidType.ProviderErxs) || isAll) {
				ProviderErxs.FillCache(ds.Tables["ProviderErx"]);
			}
			if(listITypes.Contains(InvalidType.ProviderIdents) || isAll) {
				ProviderIdents.FillCache(ds.Tables["ProviderIdent"]);
			}
			if(listITypes.Contains(InvalidType.Providers) || isAll) {
				Providers.FillCache(ds.Tables["Provider"]);
				//Refresh the clinics as well because InvalidType.Providers has a comment that says "also includes clinics".  Also, there currently isn't an itype for Clinics.
				Clinics.FillCache(ds.Tables["clinic"]);//Case must match the table name in Clinics.RefrechCache().
			}
			if(listITypes.Contains(InvalidType.QuickPaste) || isAll) {
				QuickPasteNotes.FillCache(ds.Tables["QuickPasteNote"]);
				QuickPasteCats.FillCache(ds.Tables["QuickPasteCat"]);
			}
			if(listITypes.Contains(InvalidType.RecallTypes) || isAll) {
				RecallTypes.FillCache(ds.Tables["RecallType"]);
				RecallTriggers.FillCache(ds.Tables["RecallTrigger"]);
			}
			if(listITypes.Contains(InvalidType.ReplicationServers) || isAll) {
				ReplicationServers.FillCache(ds.Tables["ReplicationServer"]);
			}
			//if(itypes.Contains(InvalidType.RequiredFields) || isAll) {
			//	RequiredFields.FillCache(ds.Tables["RequiredField"]);
			//}
			if(listITypes.Contains(InvalidType.Security) || isAll) {
				Userods.FillCache(ds.Tables["Userod"]);
				UserGroups.FillCache(ds.Tables["UserGroup"]);
			}
			if(listITypes.Contains(InvalidType.Sheets) || isAll) {
				SheetDefs.FillCache(ds.Tables["SheetDef"]);
				SheetFieldDefs.FillCache(ds.Tables["SheetFieldDef"]);
			}
			if(listITypes.Contains(InvalidType.SigMessages) || isAll) {
				SigElementDefs.FillCache(ds.Tables["SigElementDef"]);
				SigButDefs.FillCache(ds.Tables["SigButDef"]);
			}
			if(listITypes.Contains(InvalidType.Sites) || isAll) {
				Sites.FillCache(ds.Tables["Site"]);
			}
			if(listITypes.Contains(InvalidType.Sops) || isAll) {
				Sops.FillCache(ds.Tables["Sop"]);
			}
			if(listITypes.Contains(InvalidType.StateAbbrs) || isAll) {
				StateAbbrs.FillCache(ds.Tables["StateAbbr"]);
			}
			if(listITypes.Contains(InvalidType.TimeCardRules) || isAll) {
				TimeCardRules.FillCache(ds.Tables["TimeCardRule"]);
			}
			//InvalidTypes.Tasks not handled here.
			if(listITypes.Contains(InvalidType.ToolBut) || isAll) {
				ToolButItems.FillCache(ds.Tables["ToolButItem"]);
			}
			if(listITypes.Contains(InvalidType.UserClinics) || isAll) {
				UserClinics.FillCache(ds.Tables["UserClinic"]);
			}
			if(listITypes.Contains(InvalidType.Vaccines) || isAll) {
				VaccineDefs.FillCache(ds.Tables["VaccineDef"]);
				DrugManufacturers.FillCache(ds.Tables["DrugManufacturer"]);
				DrugUnits.FillCache(ds.Tables["DrugUnit"]);
			}
			if(listITypes.Contains(InvalidType.Views) || isAll) {
				ApptViews.FillCache(ds.Tables["ApptView"]);
				ApptViewItems.FillCache(ds.Tables["ApptViewItem"]);
				AppointmentRules.FillCache(ds.Tables["AppointmentRule"]);
				ProcApptColors.FillCache(ds.Tables["ProcApptColor"]);
			}
			if(listITypes.Contains(InvalidType.Wiki) || isAll) {
				WikiListHeaderWidths.FillCache(ds.Tables["WikiListHeaderWidth"]);
				WikiPages.FillCache(ds.Tables["WikiPage"]);
			}
			if(listITypes.Contains(InvalidType.ZipCodes) || isAll) {
				ZipCodes.FillCache(ds.Tables["ZipCode"]);
			}
		}

		///<summary>Returns a list of all invalid types that are used for the cache.  Currently only called from DBM.</summary>
		public static List<InvalidType> GetAllCachedInvalidTypes() {
			List<InvalidType> listInvalidTypes=new List<InvalidType>();
			//Below is a list of all invalid types in the same order the appear in the InvalidType enum.  
			//Comment out any rows that are not used for cache table refreshes.  See Cache.GetCacheDs() for more info.
			//listInvalidTypes.Add(InvalidType.None);  //No need to send a signal
			//listInvalidTypes.Add(InvalidType.Date);  //Not used with any other flags, not cached
			//listInvalidTypes.Add(InvalidType.AllLocal);  //Deprecated
			//listInvalidTypes.Add(InvalidType.Task);  //Not used with any other flags, not cached
			listInvalidTypes.Add(InvalidType.ProcCodes);
			listInvalidTypes.Add(InvalidType.Prefs);
			listInvalidTypes.Add(InvalidType.Views);
			listInvalidTypes.Add(InvalidType.AutoCodes);
			listInvalidTypes.Add(InvalidType.Carriers);
			listInvalidTypes.Add(InvalidType.ClearHouses);
			listInvalidTypes.Add(InvalidType.Computers);
			listInvalidTypes.Add(InvalidType.InsCats);
			listInvalidTypes.Add(InvalidType.Employees);
			//listInvalidTypes.Add(InvalidType.StartupOld);  //Deprecated
			listInvalidTypes.Add(InvalidType.Defs);
			listInvalidTypes.Add(InvalidType.Email);
			listInvalidTypes.Add(InvalidType.Fees);
			listInvalidTypes.Add(InvalidType.Letters);
			listInvalidTypes.Add(InvalidType.QuickPaste);
			listInvalidTypes.Add(InvalidType.Security);
			listInvalidTypes.Add(InvalidType.Programs);
			listInvalidTypes.Add(InvalidType.ToolBut);
			listInvalidTypes.Add(InvalidType.Providers);
			listInvalidTypes.Add(InvalidType.ClaimForms);
			listInvalidTypes.Add(InvalidType.ZipCodes);
			listInvalidTypes.Add(InvalidType.LetterMerge);
			listInvalidTypes.Add(InvalidType.DentalSchools);
			listInvalidTypes.Add(InvalidType.Operatories);
			//listInvalidTypes.Add(InvalidType.TaskPopup);  //Not needed, not cached
			listInvalidTypes.Add(InvalidType.Sites);
			listInvalidTypes.Add(InvalidType.Pharmacies);
			listInvalidTypes.Add(InvalidType.Sheets);
			listInvalidTypes.Add(InvalidType.RecallTypes);
			listInvalidTypes.Add(InvalidType.FeeScheds);
			//listInvalidTypes.Add(InvalidType.PhoneNumbers);  //Internal only, not cached
			//listInvalidTypes.Add(InvalidType.Signals);  //Deprecated
			listInvalidTypes.Add(InvalidType.DisplayFields);
			listInvalidTypes.Add(InvalidType.PatFields);
			listInvalidTypes.Add(InvalidType.AccountingAutoPays);
			listInvalidTypes.Add(InvalidType.ProcButtons);
			listInvalidTypes.Add(InvalidType.Diseases);
			listInvalidTypes.Add(InvalidType.Languages);
			listInvalidTypes.Add(InvalidType.AutoNotes);
			listInvalidTypes.Add(InvalidType.ElectIDs);
			listInvalidTypes.Add(InvalidType.Employers);
			listInvalidTypes.Add(InvalidType.ProviderIdents);
			//listInvalidTypes.Add(InvalidType.ShutDownNow);  //Do not want to send shutdown signal
			listInvalidTypes.Add(InvalidType.InsFilingCodes);
			listInvalidTypes.Add(InvalidType.ReplicationServers);
			listInvalidTypes.Add(InvalidType.Automation);
			//listInvalidTypes.Add(InvalidType.PhoneAsteriskReload);  //Internal only, not cached
			listInvalidTypes.Add(InvalidType.TimeCardRules);
			listInvalidTypes.Add(InvalidType.Vaccines);
			listInvalidTypes.Add(InvalidType.HL7Defs);
			listInvalidTypes.Add(InvalidType.DictCustoms);
			listInvalidTypes.Add(InvalidType.Wiki);
			listInvalidTypes.Add(InvalidType.Sops);
			listInvalidTypes.Add(InvalidType.EhrCodes);
			listInvalidTypes.Add(InvalidType.AppointmentTypes);
			listInvalidTypes.Add(InvalidType.Medications);
			//listInvalidTypes.Add(InvalidType.SmsTextMsgReceivedUnreadCount);  //Special InvalidType that would break things if we sent, not cached
			listInvalidTypes.Add(InvalidType.ProviderErxs);
			//listInvalidTypes.Add(InvalidType.Jobs);  //Internal only, not needed
			//listInvalidTypes.Add(InvalidType.JobRoles);  //Internal only, not needed
			listInvalidTypes.Add(InvalidType.StateAbbrs);
			listInvalidTypes.Add(InvalidType.RequiredFields);
			listInvalidTypes.Add(InvalidType.Ebills);
			listInvalidTypes.Add(InvalidType.UserClinics);
			listInvalidTypes.Add(InvalidType.OrthoChartTabs);
			listInvalidTypes.Add(InvalidType.SigMessages);
			listInvalidTypes.Add(InvalidType.AlertSubs);
			//listInvalidTypes.Add(InvalidType.AlertItems);//THIS IS NOT CACHED. But is used to make server run the alert logic in OpenDentalService.
			return listInvalidTypes;
		}

	}
}
