﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenDentBusiness;

namespace OpenDental{
	public class SheetFiller {

		///<summary>Gets some data from the database and fills the fields. Input should only be new sheets.
		///dataSet should be prefilled with AccountModules.GetAccount() prior to calling this method when filling fields for statements.</summary>
		public static void FillFields(Sheet sheet,DataSet dataSet=null,Statement stmt=null,MedLab medLab=null){
			foreach(SheetParameter param in sheet.Parameters){
				if(param.IsRequired && param.ParamValue==null){
					throw new ApplicationException(Lan.g("Sheet","Parameter not specified for sheet")+": "+param.ParamName);
				}
			}
			Patient pat=null;
			Provider provider=null;
			Referral refer=null;
			Deposit deposit=null;
			switch(sheet.SheetType) {
				case SheetTypeEnum.LabelPatient:
					pat=Patients.GetPat((long)GetParamByName(sheet,"PatNum").ParamValue);
					FillFieldsForLabelPatient(sheet,pat);
					break;
				case SheetTypeEnum.LabelCarrier:
					Carrier carrier=Carriers.GetCarrier((long)GetParamByName(sheet,"CarrierNum").ParamValue);
					FillFieldsForLabelCarrier(sheet,carrier);
					break;
				case SheetTypeEnum.LabelReferral:
					refer=Referrals.GetReferral((long)GetParamByName(sheet,"ReferralNum").ParamValue);
					FillFieldsForLabelReferral(sheet,refer);
					break;
				case SheetTypeEnum.ReferralSlip:
					pat=Patients.GetPat((long)GetParamByName(sheet,"PatNum").ParamValue);
					refer=Referrals.GetReferral((long)GetParamByName(sheet,"ReferralNum").ParamValue);
					FillFieldsForReferralSlip(sheet,pat,refer);
					break;
				case SheetTypeEnum.LabelAppointment:
					Appointment appt=Appointments.GetOneApt((long)GetParamByName(sheet,"AptNum").ParamValue);
					pat=Patients.GetPat(appt.PatNum);
					FillFieldsForLabelAppointment(sheet,appt,pat);
					break;
				case SheetTypeEnum.Rx:
					RxPat rx=RxPats.GetRx((long)GetParamByName(sheet,"RxNum").ParamValue);
					pat=Patients.GetPat(rx.PatNum);
					Provider prov=Providers.GetProv(rx.ProvNum);
					FillFieldsForRx(sheet,rx,pat,prov);
					break;
				case SheetTypeEnum.Consent:
					pat=Patients.GetPat((long)GetParamByName(sheet,"PatNum").ParamValue);
					FillFieldsForConsent(sheet,pat);
					break;
				case SheetTypeEnum.PatientLetter:
					pat=Patients.GetPat((long)GetParamByName(sheet,"PatNum").ParamValue);
					FillFieldsForPatientLetter(sheet,pat);
					break;
				case SheetTypeEnum.ReferralLetter:
					pat=Patients.GetPat((long)GetParamByName(sheet,"PatNum").ParamValue);
					refer=Referrals.GetReferral((long)GetParamByName(sheet,"ReferralNum").ParamValue);
					FillFieldsForReferralLetter(sheet,pat,refer);
					break;
				case SheetTypeEnum.PatientForm:
					pat=Patients.GetPat((long)GetParamByName(sheet,"PatNum").ParamValue);
					FillFieldsForPatientForm(sheet,pat);
					break;
				case SheetTypeEnum.RoutingSlip:
					Appointment apt=Appointments.GetOneApt((long)GetParamByName(sheet,"AptNum").ParamValue);
					pat=Patients.GetPat(apt.PatNum);
					FillFieldsForRoutingSlip(sheet,pat,apt);
					break;
				case SheetTypeEnum.MedicalHistory:
					pat=Patients.GetPat((long)GetParamByName(sheet,"PatNum").ParamValue);
					FillFieldsForMedicalHistory(sheet,pat);
					break;
				case SheetTypeEnum.LabSlip:
					pat=Patients.GetPat((long)GetParamByName(sheet,"PatNum").ParamValue);
					LabCase lab=LabCases.GetOne((long)GetParamByName(sheet,"LabCaseNum").ParamValue);
					FillFieldsForLabCase(sheet,pat,lab);
					break;
				case SheetTypeEnum.ExamSheet:
					pat=Patients.GetPat((long)GetParamByName(sheet,"PatNum").ParamValue);
					FillFieldsForExamSheet(sheet,pat);
					break;
				case SheetTypeEnum.DepositSlip:
					deposit=Deposits.GetOne((long)GetParamByName(sheet,"DepositNum").ParamValue);
					FillFieldsForDepositSlip(sheet,deposit);
					break;
				case SheetTypeEnum.Statement:
					pat=Patients.GetPat(sheet.PatNum);
					FillFieldsForStatement(sheet,stmt,dataSet);
					break;
				case SheetTypeEnum.MedLabResults:
					pat=Patients.GetPat(sheet.PatNum);
					FillFieldsForMedLabResults(sheet,medLab,pat);
					break;
				case SheetTypeEnum.TreatmentPlan:
					pat=Patients.GetPat(sheet.PatNum);
					FillFieldsForTreatPlan(sheet,pat);
					break;
				case SheetTypeEnum.Screening:
					ScreenGroup screenGroup=ScreenGroups.GetScreenGroup((long)GetParamByName(sheet,"ScreenGroupNum").ParamValue);
					//GetForScreen((long)GetParamByName(sheet,"ScreenNum").ParamValue);
					//Look for the optional PatNum param:
					SheetParameter paraPatNum=GetParamByName(sheet,"PatNum");
					if(paraPatNum!=null && paraPatNum.ParamValue!=null) {
						pat=Patients.GetPat((long)paraPatNum.ParamValue);
					}
					//Look for the optional ProvNum param:
					SheetParameter paraProvNum=GetParamByName(sheet,"ProvNum");
					if(paraProvNum!=null && paraProvNum.ParamValue!=null) {
						provider=Providers.GetProv((long)paraProvNum.ParamValue);
					}
					FillFieldsForScreening(sheet,screenGroup,pat,provider);
					break;
				case SheetTypeEnum.PaymentPlan:
					pat=Patients.GetPat(sheet.PatNum);
					FillFieldsForPaymentPlan(sheet,pat);
					break;
				case SheetTypeEnum.RxMulti:
					pat=Patients.GetPat(sheet.PatNum);
					FillFieldsForRxMulti(sheet,pat);
					break;
			}
			FillFieldsInStaticText(sheet,pat);
			FillPatientImages(sheet,pat);
		}

		private static SheetParameter GetParamByName(Sheet sheet,string paramName){
			foreach(SheetParameter param in sheet.Parameters){
				if(param.ParamName==paramName){
					return param;
				}
			}
			return null;
		}

		///<summary>Pat can be null sometimes.  For example, in deposit slip.</summary>
		private static void FillFieldsInStaticText(Sheet sheet,Patient pat) {
			#region Instantiate Strings
			string fldval="";
			string activeProblems="";
			string address="";
			string activeAllergies="";
			string apptModNote="";//This is the Appointment Module Note in the Appointments for window.  http://www.opendental.com/manual/apptsched.html
			string apptsAllFuture="";
			string birthdate="";
			string carrierAddress="";
			string carrierCityStZip="";
			string carrierName="";
			string carrier2Name="";
			string clinicDescription="";
			string clinicAddress="";
			string clinicCityStZip="";
			string clinicPhone="";
			string dateTodayLong="";
			string dateFirstVisit="";
			string dateOfLastSavedTP="";
			string dateRecallDue="";
			string dateTimeLastAppt="";
			string dateLastAppt="";
			string dateLastBW="";
			string dateLastExam="";
			string dateLastPerio="";
			string dateLastPanoFMX="";
			string dateLastProphy="";
			string dueForBWYN="";
			string dueForPanoYN="";
			string famPopups="";
			string famRecallDue="";
			string genderHeShe="";
			string genderheshe="";
			string genderHimHer="";
			string genderhimher="";
			string genderHimselfHerself="";
			string genderhimselfherself="";
			string genderHisHer="";
			string genderhisher="";
			string genderHisHers="";
			string genderhishers="";
			string guarantorHmPhone="";
			string guarantorNameF="";
			string guarantorNameFL="";
			string guarantorNameL="";
			string guarantorNamePref="";
			string guarantorNameLF="";
			string guarantorWirelessPhone="";
			string guarantorWkPhone="";
			string insAnnualMax="";
			string insDeductible="";
			string insDeductibleUsed="";
			string insEmployer="";
			string insFeeSchedule="";
			string insPending="";
			string insPercentages="";
			string insPlanGroupNumber="";
			string insPlanGroupName="";
			string insPlanNote="";
			string insSubNote="";
			string insSubBirthDate="";
			string insRemaining="";
			string insUsed="";
			string insFreqBW="";
			string insFreqExams="";
			string insFreqPanoFMX="";
			string insType=""; //(ppo, etc)
			string ins2AnnualMax="";
			string ins2Deductible="";
			string ins2DeductibleUsed="";
			string ins2Employer="";
			string ins2FreqBW="";
			string ins2FreqExams="";
			string ins2FreqPanoFMX="";
			string ins2PlanGroupNumber="";
			string ins2PlanGroupName="";
			string ins2Pending="";
			string ins2Percentages="";
			string ins2Remaining="";
			string ins2Used="";
			string medicalSummary="";
			string currentMedications="";
			string nextSchedApptDateT="";
			string nextSchedApptDate="";
			string nextSchedApptsFam="";
			string phone="";
			string plannedAppointmentInfo="";
			string premedicateYN="";
			string recallInterval="";
			string recallScheduledYN="";
			string referredFrom=""; //(just one)
			string referredTo=""; //(typically Drs. could be multiline. Include date)
			string serviceNote="";
			string subscriberId="";
			string subscriberNameFL="";
			string subscriber2NameFL="";
			string tpResponsPartyAddress="";
			string tpResponsPartyCityStZip="";
			string tpResponsPartyNameFL="";
			string treatmentNote="";
			string treatmentPlanProcs="";
			string treatmentPlanProcsPriority="";
			#endregion
			Family fam=null;
			Provider priProv=null;
			PatientNote patNote=null;
			#region Patient Fields
			if(pat!=null) {
				fam=Patients.GetFamily(pat.PatNum);
				premedicateYN=Lan.g("All","No");
				recallScheduledYN=Lan.g("All","No");
				dueForBWYN=Lan.g("All","No");
				dueForPanoYN=Lan.g("All","No");
				if(pat.Premed) {
					premedicateYN=Lan.g("All","Yes");
				}
				patNote=PatientNotes.Refresh(pat.PatNum,pat.Guarantor);
				medicalSummary=patNote.Medical;
				treatmentNote=patNote.Treatment;
				apptModNote=pat.ApptModNote;
				dateTodayLong=DateTime.Today.ToLongDateString();
				#region Gender
				switch(pat.Gender) {
					case PatientGender.Male:
						genderHeShe=Lan.g("PatientInfo","He");
						genderheshe=Lan.g("PatientInfo","he");
						genderHimHer=Lan.g("PatientInfo","Him");
						genderhimher=Lan.g("PatientInfo","him");
						genderHimselfHerself=Lan.g("PatientInfo","Himself");
						genderhimselfherself=Lan.g("PatientInfo","Herself");
						genderHisHer=Lan.g("PatientInfo","His");
						genderhisher=Lan.g("PatientInfo","his");
						genderHisHers=Lan.g("PatientInfo","His");
						genderhishers=Lan.g("PatientInfo","his");
						break;
					case PatientGender.Female:
						genderHeShe=Lan.g("PatientInfo","She");
						genderheshe=Lan.g("PatientInfo","she");
						genderHimHer=Lan.g("PatientInfo","Her");
						genderhimher=Lan.g("PatientInfo","her");
						genderHimselfHerself=Lan.g("PatientInfo","Herself");
						genderhimselfherself=Lan.g("PatientInfo","herself");
						genderHisHer=Lan.g("PatientInfo","Her");
						genderhisher=Lan.g("PatientInfo","her");
						genderHisHers=Lan.g("PatientInfo","Hers");
						genderhishers=Lan.g("PatientInfo","hers");
						break;
					case PatientGender.Unknown:
						genderHeShe=Lan.g("PatientInfo","The patient");
						genderheshe=Lan.g("PatientInfo","the patient");
						genderHimHer=Lan.g("PatientInfo","The patient");
						genderhimher=Lan.g("PatientInfo","the patient");
						genderHimselfHerself=Lan.g("PatientInfo","The patient");
						genderhimselfherself=Lan.g("PatientInfo","the patient");
						genderHisHer=Lan.g("PatientInfo","The patient's");
						genderhisher=Lan.g("PatientInfo","the patient's");
						genderHisHers=Lan.g("PatientInfo","The patient's");
						genderhishers=Lan.g("PatientInfo","the patient's");
						break;
				}
				#endregion
				#region Guarantor
				Patient guar=Patients.GetPat(pat.Guarantor);
				if(guar!=null) {
					guarantorNameF=guar.FName;
					guarantorNameFL=guar.GetNameFL();
					guarantorNameL=guar.LName;
					guarantorNameLF=guar.GetNameLF();
					guarantorNamePref=guar.Preferred;
					guarantorHmPhone=guar.HmPhone;
					guarantorWirelessPhone=guar.WirelessPhone;
					guarantorWkPhone=guar.WkPhone;
				}
				#endregion
				#region Address
				address=pat.Address;
				if(pat.Address2!="") {
					address+=", "+pat.Address2;
				}
				#endregion
				#region Birthdate
				birthdate=pat.Birthdate.ToShortDateString();
				if(pat.Birthdate.Year<1880) {
					birthdate="";
				}
				#endregion
				#region Date First Visit
				dateFirstVisit=pat.DateFirstVisit.ToShortDateString();
				if(pat.DateFirstVisit.Year<1880) {
					dateFirstVisit="";
				}
				#endregion
				#region Treatment Plan Procs
				//todo some day: move this section down to TP section
				List<Procedure> procsList=null;//there is another variable that does the same thing. Carefully combine them.
				if(Sheets.ContainsStaticField(sheet,"treatmentPlanProcs") 
					|| Sheets.ContainsStaticField(sheet,"plannedAppointmentInfo") 
					|| Sheets.ContainsStaticField(sheet,"treatmentPlanProcsPriority")) 
				{
					procsList=Procedures.Refresh(pat.PatNum);
					if(Sheets.ContainsStaticField(sheet,"treatmentPlanProcs") || Sheets.ContainsStaticField(sheet,"treatmentPlanProcsPriority")) {
						Procedure[] procListTP=Procedures.GetListTPandTPi(procsList);//sorted by priority, then toothnum
						for(int i=0;i<procListTP.Length;i++) {
							if(procListTP[i].ProcStatus!=ProcStat.TP) {
								continue;
							}
							if(treatmentPlanProcs!="") {
								treatmentPlanProcs+="\r\n";
								treatmentPlanProcsPriority+="\r\n";
							}
							//Figure out what the procedure description will be like.
							string procDescript=ProcedureCodes.GetStringProcCode(procListTP[i].CodeNum)+", "
								+Procedures.GetDescription(procListTP[i])+", "
								+procListTP[i].ProcFee.ToString("c");
							//Get the procedure's priority.
							string priority=DefC.GetName(DefCat.TxPriorities,procListTP[i].Priority);
							if(priority=="") {
								priority=Lan.g("TreatmentPlans","No priority");
							}
							//Set the corresponding static field text.
							treatmentPlanProcsPriority+=priority+", "+procDescript;
							treatmentPlanProcs+=procDescript;
						}
					}
				}
				#endregion
				serviceNote=PatientNotes.Refresh(pat.PatNum,pat.Guarantor).Service;
				#region Referrals
				List<RefAttach> RefAttachList=RefAttaches.Refresh(pat.PatNum);
				Referral tempReferralFrom = Referrals.GetReferralForPat(pat.PatNum);
				if(Referrals.GetReferralForPat(pat.PatNum)!=null) {
					if(tempReferralFrom.IsDoctor) {
						referredFrom+=tempReferralFrom.FName+" "+tempReferralFrom.LName+" "+tempReferralFrom.Title+" : "
							+DefC.GetName(DefCat.ProviderSpecialties,tempReferralFrom.Specialty);
					}
					else {
						referredFrom+=tempReferralFrom.FName+" "+tempReferralFrom.LName;
					}
				}
				for(int i=0;i<RefAttachList.Count;i++) {
					if(RefAttachList[i].IsFrom) {
						continue;
					}
					Referral tempRef = Referrals.GetReferral(RefAttachList[i].ReferralNum);
					if(tempRef.IsDoctor) {
						referredTo+=tempRef.FName+" "+tempRef.LName+" "+tempRef.Title+" : "+DefC.GetName(DefCat.ProviderSpecialties,tempRef.Specialty)+" "
							+RefAttachList[i].RefDate.ToShortDateString()+"\r\n";
					}
					else {
						referredTo+=tempRef.FName+" "+tempRef.LName+" "+RefAttachList[i].RefDate.ToShortDateString()+"\r\n";
					}
				}
				#endregion
				#region Insurance
				//Insurance-------------------------------------------------------------------------------------------------------------------
				List<InsSub> subList=InsSubs.RefreshForFam(fam);
				List<InsPlan> planList=InsPlans.RefreshForSubList(subList);
				List<PatPlan> patPlanList=PatPlans.Refresh(pat.PatNum);
				int ordinal=PatPlans.GetOrdinal(PriSecMed.Primary,patPlanList,planList,subList);
				if(ordinal==0) { //No primary dental plan. See if they have a medical plan instead.
					ordinal=PatPlans.GetOrdinal(PriSecMed.Medical,patPlanList,planList,subList);
				}
				long subNum=PatPlans.GetInsSubNum(patPlanList,ordinal);
				long patPlanNum=PatPlans.GetPatPlanNum(subNum,patPlanList);
				InsSub sub=InsSubs.GetSub(subNum,subList);
				Patient subscriber=Patients.GetPat(sub.Subscriber);
				if(subscriber!=null) {
					insSubBirthDate=subscriber.Birthdate.ToShortDateString();
				}
				InsPlan plan=null;
				if(sub!=null) {
					plan=InsPlans.GetPlan(sub.PlanNum,planList);
					insSubNote=sub.SubscNote;
				}
				Carrier carrier=null;
				List<Benefit> benefitList=Benefits.Refresh(patPlanList,subList);
				List<ClaimProcHist> histList=ClaimProcs.GetHistList(pat.PatNum,benefitList,patPlanList,planList,DateTimeOD.Today,subList);
				double doubAnnualMax;
				double doubDeductible;
				double doubDeductibleUsed;
				double doubPending;
				double doubRemain;
				double doubUsed;
				if(plan!=null) {
					insFeeSchedule=FeeScheds.GetDescription(plan.FeeSched);
					insPlanGroupName=plan.GroupName;
					insPlanGroupNumber=plan.GroupNum;
					insPlanNote=plan.PlanNote;
					carrier=Carriers.GetCarrier(plan.CarrierNum);
					carrierName=carrier.CarrierName;
					carrierAddress=carrier.Address;
					if(carrier.Address2!="") {
						carrierAddress+=", "+carrier.Address2;
					}
					carrierCityStZip=carrier.City+", "+carrier.State+"  "+carrier.Zip;
					subscriberId=sub.SubscriberID;
					if(subscriber!=null) {
						subscriberNameFL=subscriber.GetNameFL();
					}
					doubAnnualMax=Benefits.GetAnnualMaxDisplay(benefitList,plan.PlanNum,patPlanNum,false);
					doubRemain=-1;
					if(doubAnnualMax!=-1) {
						insAnnualMax=doubAnnualMax.ToString("c");
						doubRemain=doubAnnualMax;
					}
					doubDeductible=Benefits.GetDeductGeneralDisplay(benefitList,plan.PlanNum,patPlanNum,BenefitCoverageLevel.Individual);
					if(doubDeductible!=-1) {
						insDeductible=doubDeductible.ToString("c");
					}
					doubDeductibleUsed=InsPlans.GetDedUsedDisplay(histList,DateTime.Today,plan.PlanNum,patPlanNum,-1,planList,BenefitCoverageLevel.Individual,pat.PatNum);
					if(doubDeductibleUsed!=-1) {
						insDeductibleUsed=doubDeductibleUsed.ToString("c");
					}
					doubPending=InsPlans.GetPendingDisplay(histList,DateTime.Today,plan,patPlanNum,-1,pat.PatNum,subNum,benefitList);
					if(doubPending!=-1) {
						insPending=doubPending.ToString("c");
						if(doubRemain!=-1) {
							doubRemain-=doubPending;
						}
					}
					doubUsed=InsPlans.GetInsUsedDisplay(histList,DateTime.Today,plan.PlanNum,patPlanNum,-1,planList,benefitList,pat.PatNum,subNum);
					if(doubUsed!=-1) {
						insUsed=doubUsed.ToString("c");
						if(doubRemain!=-1) {
							doubRemain-=doubUsed;
						}
					}
					if(doubRemain!=-1) {
						insRemaining=doubRemain.ToString("c");
					}
					for(int j=0;j<benefitList.Count;j++) {
						if(benefitList[j].PlanNum != plan.PlanNum) {
							continue;
						}
						if(benefitList[j].BenefitType != InsBenefitType.CoInsurance) {
							continue;
						}
						if(insPercentages!="") {
							insPercentages+=",  ";
						}
						insPercentages+=CovCats.GetDesc(benefitList[j].CovCatNum)+" "+benefitList[j].Percent.ToString()+"%";
					}
					insFreqBW=Benefits.GetFrequencyDisplay(FrequencyType.BW,benefitList,plan.PlanNum);
					insFreqExams=Benefits.GetFrequencyDisplay(FrequencyType.Exam,benefitList,plan.PlanNum);
					insFreqPanoFMX=Benefits.GetFrequencyDisplay(FrequencyType.PanoFMX,benefitList,plan.PlanNum);
					switch(plan.PlanType) {//(ppo, etc)
						case "p":
							insType=Lan.g("InsurancePlans","PPO Percentage");
							break;
						case "f":
							insType=Lan.g("InsurancePlans","Medicaid or Flat Copay");
							break;
						case "c":
							insType=Lan.g("InsurancePlans","Capitation");
							break;
						case "":
							insType=Lan.g("InsurancePlans","Category Percentage");
							break;
					}
					insEmployer=Employers.GetEmployer(plan.EmployerNum).EmpName; //blank if no Employer listed
				}
				subNum=PatPlans.GetInsSubNum(patPlanList,PatPlans.GetOrdinal(PriSecMed.Secondary,patPlanList,planList,subList));
				patPlanNum=PatPlans.GetPatPlanNum(subNum,patPlanList);
				sub=InsSubs.GetSub(subNum,subList);
				if(sub!=null) {
					plan=InsPlans.GetPlan(sub.PlanNum,planList);
				}
				if(plan!=null) { //secondary insurance
					ins2PlanGroupName=plan.GroupName;
					ins2PlanGroupNumber=plan.GroupNum;
					carrier=Carriers.GetCarrier(plan.CarrierNum);
					carrier2Name=carrier.CarrierName;
					//carrierAddress=carrier.Address;
					//if(carrier.Address2!="") {
					//	carrierAddress+=", "+carrier.Address2;
					//}
					//carrierCityStZip=carrier.City+", "+carrier.State+"  "+carrier.Zip;
					//subscriberId=plan.SubscriberID;
					subscriber2NameFL=Patients.GetLim(sub.Subscriber).GetNameFL();
					doubAnnualMax=Benefits.GetAnnualMaxDisplay(benefitList,plan.PlanNum,patPlanNum,false);
					doubRemain=-1;
					if(doubAnnualMax!=-1) {
						ins2AnnualMax=doubAnnualMax.ToString("c");
						doubRemain=doubAnnualMax;
					}
					doubDeductible=Benefits.GetDeductGeneralDisplay(benefitList,plan.PlanNum,patPlanNum,BenefitCoverageLevel.Individual);
					if(doubDeductible!=-1) {
						ins2Deductible=doubDeductible.ToString("c");
					}
					doubDeductibleUsed=InsPlans.GetDedUsedDisplay(histList,DateTime.Today,plan.PlanNum,patPlanNum,-1,planList,BenefitCoverageLevel.Individual,pat.PatNum);
					if(doubDeductibleUsed!=-1) {
						ins2DeductibleUsed=doubDeductibleUsed.ToString("c");
					}
					doubPending=InsPlans.GetPendingDisplay(histList,DateTime.Today,plan,patPlanNum,-1,pat.PatNum,subNum,benefitList);
					if(doubPending!=-1) {
						ins2Pending=doubPending.ToString("c");
						if(doubRemain!=-1) {
							doubRemain-=doubPending;
						}
					}
					doubUsed=InsPlans.GetInsUsedDisplay(histList,DateTime.Today,plan.PlanNum,patPlanNum,-1,planList,benefitList,pat.PatNum,subNum);
					if(doubUsed!=-1) {
						ins2Used=doubUsed.ToString("c");
						if(doubRemain!=-1) {
							doubRemain-=doubUsed;
						}
					}
					if(doubRemain!=-1) {
						ins2Remaining=doubRemain.ToString("c");
					}
					for(int j=0;j<benefitList.Count;j++) {
						if(benefitList[j].PlanNum != plan.PlanNum) {
							continue;
						}
						if(benefitList[j].BenefitType != InsBenefitType.CoInsurance) {
							continue;
						}
						if(ins2Percentages!="") {
							ins2Percentages+=",  ";
						}
						ins2Percentages+=CovCats.GetDesc(benefitList[j].CovCatNum)+" "+benefitList[j].Percent.ToString()+"%";
						ins2FreqBW=Benefits.GetFrequencyDisplay(FrequencyType.BW,benefitList,plan.PlanNum);
						ins2FreqExams=Benefits.GetFrequencyDisplay(FrequencyType.Exam,benefitList,plan.PlanNum);
						ins2FreqPanoFMX=Benefits.GetFrequencyDisplay(FrequencyType.PanoFMX,benefitList,plan.PlanNum);
					}
					ins2Employer=Employers.GetEmployer(plan.EmployerNum).EmpName;//blank if no Employer listed
				}
				#endregion
				#region Treatment Plan
				//Treatment plan-----------------------------------------------------------------------------------------------------------
				List<TreatPlan> treatPlanList=TreatPlans.Refresh(pat.PatNum);
				TreatPlan treatPlan=null;
				if(treatPlanList.Count>0) {
					treatPlan=treatPlanList[treatPlanList.Count-1].Copy();
					dateOfLastSavedTP=treatPlan.DateTP.ToShortDateString();
					Patient patRespParty=Patients.GetPat(treatPlan.ResponsParty);
					if(patRespParty!=null) {
						tpResponsPartyAddress=patRespParty.Address;
						if(patRespParty.Address2!="") {
							tpResponsPartyAddress+=", "+patRespParty.Address2;
						}
						tpResponsPartyCityStZip=patRespParty.City+", "+patRespParty.State+"  "+patRespParty.Zip;
						tpResponsPartyNameFL=patRespParty.GetNameFL();
					}
				}
				#endregion
				#region Procedure Log
				//Procedure Log-------------------------------------------------------------------------------------------------------------
				List<long> listProcCodeNums=new List<long>();
				listProcCodeNums.Add(ProcedureCodes.GetCodeNum("D0210"));//intraoral - complete series (including bitewings) 
				listProcCodeNums.Add(ProcedureCodes.GetCodeNum("D0270"));//bitewing - single film
				listProcCodeNums.Add(ProcedureCodes.GetCodeNum("D0272"));//bitewings - two films
				listProcCodeNums.Add(ProcedureCodes.GetCodeNum("D0274"));//bitewings - four films
				listProcCodeNums.Add(ProcedureCodes.GetCodeNum("D0277"));//vertical bitewings - 7 to 8 films
				listProcCodeNums.Add(ProcedureCodes.GetCodeNum("D0273"));//bitewings - three films
				listProcCodeNums.Add(ProcedureCodes.GetCodeNum("D0120"));//periodic oral evaluation - established patient
				listProcCodeNums.Add(ProcedureCodes.GetCodeNum("D0140"));//limited oral evaluation - problem focused
				listProcCodeNums.Add(ProcedureCodes.GetCodeNum("D0150"));//comprehensive oral evaluation - new or established patient
				listProcCodeNums.Add(ProcedureCodes.GetCodeNum("D0160"));//detailed and extensive oral evaluation - problem focused, by report
				listProcCodeNums.Add(ProcedureCodes.GetCodeNum("D4910"));//Periodontal Maintenance 
				listProcCodeNums.Add(ProcedureCodes.GetCodeNum("D0210"));//intraoral - complete series (including bitewings)
				listProcCodeNums.Add(ProcedureCodes.GetCodeNum("D0330"));//panoramic film
				listProcCodeNums.Add(ProcedureCodes.GetCodeNum("D1110"));//prophylaxis - adult
				listProcCodeNums.Add(ProcedureCodes.GetCodeNum("D1120"));//prophylaxis - child
				listProcCodeNums.Add(ProcedureCodes.GetCodeNum("D1201"));//Topical Fluoride Including Prophy-Child
				listProcCodeNums.Add(ProcedureCodes.GetCodeNum("D1205"));//Topical Fluoride Including Prophy-Adult
				List<Procedure> proceduresList=Procedures.RefreshForProcCodeNums(pat.PatNum,listProcCodeNums);
				DateTime dBW=DateTime.MinValue;
				DateTime dExam=DateTime.MinValue;
				DateTime dPerio=DateTime.MinValue;
				DateTime dPanoFMX=DateTime.MinValue;
				DateTime dProphy=DateTime.MinValue;
				for(int i=0;i<proceduresList.Count;i++) {
					Procedure proc = proceduresList[i];//cache Proc to speed up process
					if(proc.ProcStatus!=ProcStat.C
						&& proc.ProcStatus!=ProcStat.EC
						&& proc.ProcStatus!=ProcStat.EO) {
						continue;//only look at completed or existing procedures
					}
					if((proc.CodeNum==ProcedureCodes.GetCodeNum("D0210")//intraoral - complete series (including bitewings) 
						||proc.CodeNum==ProcedureCodes.GetCodeNum("D0270")//bitewing - single film
						||proc.CodeNum==ProcedureCodes.GetCodeNum("D0272")//bitewings - two films
						||proc.CodeNum==ProcedureCodes.GetCodeNum("D0274")//bitewings - four films
						||proc.CodeNum==ProcedureCodes.GetCodeNum("D0277")//vertical bitewings - 7 to 8 films
						||proc.CodeNum==ProcedureCodes.GetCodeNum("D0273"))//bitewings - three films
						&& proc.ProcDate>dBW) //newest
					{
						dBW=proc.ProcDate;
						dateLastBW=proc.ProcDate.ToShortDateString();
					}
					if((proc.CodeNum==ProcedureCodes.GetCodeNum("D0120")//periodic oral evaluation - established patient
						||proc.CodeNum==ProcedureCodes.GetCodeNum("D0140")//limited oral evaluation - problem focused
						||proc.CodeNum==ProcedureCodes.GetCodeNum("D0150")//comprehensive oral evaluation - new or established patient
						||proc.CodeNum==ProcedureCodes.GetCodeNum("D0160"))//detailed and extensive oral evaluation - problem focused, by report
						&& proc.ProcDate>dExam) //newest
					{
						dExam=proc.ProcDate;
						dateLastExam=proc.ProcDate.ToShortDateString();
					}
					if(proc.CodeNum==ProcedureCodes.GetCodeNum("D4910")//Periodontal Maintenance 
						&& proc.ProcDate>dPerio)//newest 
					{
						dPerio=proc.ProcDate;
						dateLastPerio=proc.ProcDate.ToShortDateString();
					}
					if((proc.CodeNum==ProcedureCodes.GetCodeNum("D0210")//intraoral - complete series (including bitewings)
						||proc.CodeNum==ProcedureCodes.GetCodeNum("D0330"))//panoramic film
						&& proc.ProcDate>dPanoFMX) //newest
					{
						dPanoFMX=proc.ProcDate;
						dateLastPanoFMX=proc.ProcDate.ToShortDateString();
					}
					if((proc.CodeNum==ProcedureCodes.GetCodeNum("D1110")//prophylaxis - adult
						||proc.CodeNum==ProcedureCodes.GetCodeNum("D1120")//prophylaxis - child
						||proc.CodeNum==ProcedureCodes.GetCodeNum("D1201")//Topical Fluoride Including Prophy-Child
						||proc.CodeNum==ProcedureCodes.GetCodeNum("D1205"))//Topical Fluoride Including Prophy-Adult
						&& proc.ProcDate>dProphy) //newest
					{
						dProphy=proc.ProcDate;
						dateLastProphy=proc.ProcDate.ToShortDateString(); ;
					}
				}
				#endregion
				#region Recall
				//Recall--------------------------------------------------------------------------------------------------------------------
				List<Recall> listRecalls=Recalls.GetList(pat.PatNum);
				for(int i=0;i<listRecalls.Count;i++) {
					if(listRecalls[i].DateDue>DateTime.Today) { //don't care about recalls in the future.
						continue;
					}
					List<string> listProcCodes=RecallTypes.GetProcs(listRecalls[i].RecallTypeNum);
					for(int j=0;j<listProcCodes.Count;j++) {
						if(listProcCodes[j]=="D0210"//intraoral - complete series (including bitewings) 				   
							||listProcCodes[j]=="D0270"//bitewing - single film													   
							||listProcCodes[j]=="D0272"//bitewings - two films														   
							||listProcCodes[j]=="D0274"//bitewings - four films													   
							||listProcCodes[j]=="D0277"//vertical bitewings - 7 to 8 films											   
							||listProcCodes[j]=="D0273")//bitewings - three films
						{
							dueForBWYN=Lan.g("All","Yes");
						}
						if(listProcCodes[j]=="D0210"//intraoral - complete series (including bitewings)				   
							||listProcCodes[j]=="D0330")//panoramic film
						{
							dueForPanoYN=Lan.g("All","Yes");
						}
					}
				}
				Recall recall=Recalls.GetRecallProphyOrPerio(pat.PatNum);
				if(recall!=null && !recall.IsDisabled) {
					if(recall.DateDue.Year>1880) {
						dateRecallDue=recall.DateDue.ToShortDateString();
					}
					recallInterval=recall.RecallInterval.ToString();
					if(recall.DateScheduled>=DateTime.Today) {
						recallScheduledYN=Lan.g("All","Yes");
					}
				}
				for(int i=0;i<fam.ListPats.Length;i++) {
					recall=Recalls.GetRecallProphyOrPerio(fam.ListPats[i].PatNum);
					if(recall==null || recall.IsDisabled || recall.DateDue==DateTime.MinValue || recall.DateDue>=DateTime.Today) {
						continue;
					}
					if(famRecallDue!="") {
						famRecallDue+="\r\n";
					}
					famRecallDue+=fam.ListPats[i].FName+", "+recall.DateDue.ToShortDateString()+" "+RecallTypes.GetDescription(recall.RecallTypeNum);
				}
				#endregion
				#region Appointments
				//Appointments--------------------------------------------------------------------------------------------------------------
				List<Appointment> apptList=Appointments.GetListForPat(pat.PatNum);
				List<Appointment> apptFutureList=Appointments.GetFutureSchedApts(pat.PatNum);
				for(int i=0;i<apptList.Count;i++) {
					if(apptList[i].AptStatus != ApptStatus.Scheduled
					&& apptList[i].AptStatus != ApptStatus.Complete
					&& apptList[i].AptStatus != ApptStatus.None
					&& apptList[i].AptStatus != ApptStatus.ASAP) {
						continue;
					}
					if(apptList[i].AptDateTime < DateTime.Now) {
						//this will happen repeatedly up until the most recent.
						dateTimeLastAppt=apptList[i].AptDateTime.ToShortDateString()+"  "+apptList[i].AptDateTime.ToShortTimeString();
						dateLastAppt=apptList[i].AptDateTime.ToShortDateString();
					}
					else {//after now
						if(nextSchedApptDateT=="") {//only the first one found
							nextSchedApptDateT=apptList[i].AptDateTime.ToShortDateString()+"  "+apptList[i].AptDateTime.ToShortTimeString();
							nextSchedApptDate=apptList[i].AptDateTime.ToShortDateString();
							break;//we're done with the list now.
						}
					}
				}
				for(int i=0;i<apptFutureList.Count;i++) {//cannot be combined in loop above because of the break in the loop.
					apptsAllFuture+=apptFutureList[i].AptDateTime.ToShortDateString()+" "+apptFutureList[i].AptDateTime.ToShortTimeString()+" : "+apptFutureList[i].ProcDescript+"\r\n";
				}
				for(int i=0;i<fam.ListPats.Length;i++) {
					List<Appointment> futAptsList=Appointments.GetFutureSchedApts(fam.ListPats[i].PatNum);
					if(futAptsList.Count>0) {//just gets one future appt for each person
						nextSchedApptsFam+=fam.ListPats[i].FName+": "+futAptsList[0].AptDateTime.ToShortDateString()+" "+futAptsList[0].AptDateTime.ToShortTimeString()+" : "+futAptsList[0].ProcDescript+"\r\n";
					}
				}
				if(Sheets.ContainsStaticField(sheet,"plannedAppointmentInfo")) {
					PlannedAppt plannedAppt=PlannedAppts.GetOneOrderedByItemOrder(pat.PatNum);
					for(int i=0;i<apptList.Count;i++) {
						if(plannedAppt!=null && apptList[i].AptNum==plannedAppt.AptNum) {
							plannedAppointmentInfo=Lan.g("Appointments","Procedures:")+" ";
							plannedAppointmentInfo+=apptList[i].ProcDescript+"\r\n";
							int minutesTotal=apptList[i].Pattern.Length*5;
							int hours=minutesTotal/60;//automatically rounds down
							int minutes=minutesTotal-hours*60;
							plannedAppointmentInfo+=Lan.g("Appointments","Appt Length:")+" ";
							if(hours>0) {
								plannedAppointmentInfo+=hours.ToString()+" "+Lan.g("Appointments","hours")+", ";
							}
							plannedAppointmentInfo+=minutes.ToString()+" "+Lan.g("Appointments","min")+"\r\n";
							if(Programs.UsingOrion) {
								DateTime newDateSched=new DateTime();
								for(int p=0;p<procsList.Count;p++) {
									if(procsList[p].PlannedAptNum==apptList[i].AptNum) {
										OrionProc op=OrionProcs.GetOneByProcNum(procsList[p].ProcNum);
										if(op!=null && op.DateScheduleBy.Year>1880) {
											if(newDateSched.Year<1880) {
												newDateSched=op.DateScheduleBy;
											}
											else {
												if(op.DateScheduleBy<newDateSched) {
													newDateSched=op.DateScheduleBy;
												}
											}
										}
									}
								}
								if(newDateSched.Year>1880) {
									plannedAppointmentInfo+=Lan.g("Appointments","Schedule by")+": "+newDateSched.ToShortDateString();
								}
								else {
									plannedAppointmentInfo+=Lan.g("Appointments","No schedule by date.");
								}
							}
						}
					}
				}
				#endregion
				#region Clinic
				priProv=Providers.GetProv(Patients.GetProvNum(pat));//guaranteed to work
				//Clinic-------------------------------------------------------------------------------------------------------------
				Clinic clinic=Clinics.GetClinic(pat.ClinicNum);
				if(clinic==null) {
					clinicDescription=PrefC.GetString(PrefName.PracticeTitle);
					clinicAddress=PrefC.GetString(PrefName.PracticeAddress);
					if(PrefC.GetString(PrefName.PracticeAddress2)!="") {
						clinicAddress+=", "+PrefC.GetString(PrefName.PracticeAddress2);
					}
					clinicCityStZip=PrefC.GetString(PrefName.PracticeCity)+", "+PrefC.GetString(PrefName.PracticeST)+"  "+PrefC.GetString(PrefName.PracticeZip);
					phone=PrefC.GetString(PrefName.PracticePhone);
				}
				else {
					clinicDescription=clinic.Description;
					clinicAddress=clinic.Address;
					if(clinic.Address2!="") {
						clinicAddress+=", "+clinic.Address2;
					}
					clinicCityStZip=clinic.City+", "+clinic.State+"  "+clinic.Zip;
					phone=clinic.Phone;
				}
				if(phone.Length==10 && System.Globalization.CultureInfo.CurrentCulture.Name=="en-US") {
					clinicPhone="("+phone.Substring(0,3)+")"+phone.Substring(3,3)+"-"+phone.Substring(6);
				}
				else {
					clinicPhone=phone;
				}
				#endregion
				#region Diseases/Allergies
				List<Disease> listDiseases=Diseases.Refresh(pat.PatNum,true);
				for(int i=0;i<listDiseases.Count;i++) {
					if(activeProblems!="") {
						activeProblems+=", ";
					}
					activeProblems+=DiseaseDefs.GetName(listDiseases[i].DiseaseDefNum);
				}
				List<Allergy> listAllergies=Allergies.GetAll(pat.PatNum, false);
				for(int i=0;i<listAllergies.Count;i++) {
					if(activeAllergies!="") {
						activeAllergies+=", ";
					}
					activeAllergies+=AllergyDefs.GetDescription(listAllergies[i].AllergyDefNum);
				}
				#endregion
				#region Medication
				List<MedicationPat> listMedicationPats=MedicationPats.Refresh(pat.PatNum,false);
				for(int i=0;i<listMedicationPats.Count;i++) {
					Medication medCur=Medications.GetMedication(listMedicationPats[i].MedicationNum);
					if(medCur==null) {
						continue;
					}
					if(currentMedications!="") {
						currentMedications+=", ";
					}
					if(medCur.MedicationNum==0) {//NewCrop medication order.
						currentMedications+=listMedicationPats[i].MedDescript;
						continue;
					}
					currentMedications+=medCur.MedName;
					if(medCur.MedicationNum != medCur.GenericNum) {
						currentMedications+=" ("+Medications.GetMedication(medCur.GenericNum).MedName+")";
					}
				}
				#endregion
				#region Popups
				//patient, family & superfam popups
				List<Popup> listFamPopups=Popups.GetForFamily(pat);
				for(int i=0;i<listFamPopups.Count;i++) {
					if(listFamPopups[i].IsDisabled) {
						continue;
					}
					if(famPopups!="") {
						famPopups+=", ";
					}
					famPopups+=listFamPopups[i].Description;
				}
				#endregion
			}//End of if(pat!=null)
			#endregion
			#region Fill Fields
			//Fill fields---------------------------------------------------------------------------------------------------------
			foreach(SheetField field in sheet.SheetFields) {
				if(field.FieldType!=SheetFieldType.StaticText) {
					continue;
				}
				fldval=field.FieldValue;
				if(pat!=null) {
					fldval=fldval.Replace("[activeAllergies]",activeAllergies);
					fldval=fldval.Replace("[activeProblems]",activeProblems);
					fldval=fldval.Replace("[address]",address);
					fldval=fldval.Replace("[apptsAllFuture]",apptsAllFuture.TrimEnd());
					fldval=fldval.Replace("[apptModNote]",apptModNote);
					fldval=fldval.Replace("[age]",Patients.AgeToString(pat.Age));
					fldval=fldval.Replace("[balTotal]",fam.ListPats[0].BalTotal.ToString("c"));
					fldval=fldval.Replace("[bal_0_30]",fam.ListPats[0].Bal_0_30.ToString("c"));
					fldval=fldval.Replace("[bal_31_60]",fam.ListPats[0].Bal_31_60.ToString("c"));
					fldval=fldval.Replace("[bal_61_90]",fam.ListPats[0].Bal_61_90.ToString("c"));
					fldval=fldval.Replace("[balOver90]",fam.ListPats[0].BalOver90.ToString("c"));
					fldval=fldval.Replace("[balInsEst]",fam.ListPats[0].InsEst.ToString("c"));
					fldval=fldval.Replace("[balTotalMinusInsEst]",(fam.ListPats[0].BalTotal-fam.ListPats[0].InsEst).ToString("c"));
					fldval=fldval.Replace("[BillingType]",DefC.GetName(DefCat.BillingTypes,pat.BillingType));
					fldval=fldval.Replace("[Birthdate]",birthdate);
					fldval=fldval.Replace("[carrierName]",carrierName);
					fldval=fldval.Replace("[carrier2Name]",carrier2Name);
					fldval=fldval.Replace("[ChartNumber]",pat.ChartNumber);
					fldval=fldval.Replace("[carrierAddress]",carrierAddress);
					fldval=fldval.Replace("[carrierCityStZip]",carrierCityStZip);
					fldval=fldval.Replace("[cityStateZip]",pat.City+", "+pat.State+"  "+pat.Zip);
					fldval=fldval.Replace("[clinicDescription]",clinicDescription);
					fldval=fldval.Replace("[clinicAddress]",clinicAddress);
					fldval=fldval.Replace("[clinicCityStZip]",clinicCityStZip);
					fldval=fldval.Replace("[clinicPhone]",clinicPhone);
					fldval=fldval.Replace("[currentMedications]",currentMedications);
					fldval=fldval.Replace("[DateFirstVisit]",dateFirstVisit);
					fldval=fldval.Replace("[dateLastAppt]",dateLastAppt);
					fldval=fldval.Replace("[dateLastBW]",dateLastBW);
					fldval=fldval.Replace("[dateLastExam]",dateLastExam);
					fldval=fldval.Replace("[dateLastPerio]",dateLastPerio);
					fldval=fldval.Replace("[dateLastPanoFMX]",dateLastPanoFMX);
					fldval=fldval.Replace("[dateLastProphy]",dateLastProphy);
					fldval=fldval.Replace("[dateOfLastSavedTP]",dateOfLastSavedTP);
					fldval=fldval.Replace("[dateRecallDue]",dateRecallDue);
					fldval=fldval.Replace("[dateTimeLastAppt]",dateTimeLastAppt);
					fldval=fldval.Replace("[dateTodayLong]",dateTodayLong);
					fldval=fldval.Replace("[dueForBWYN]",dueForBWYN);
					fldval=fldval.Replace("[dueForPanoYN]",dueForPanoYN);
					fldval=fldval.Replace("[Email]",pat.Email);
					fldval=fldval.Replace("[famFinNote]",patNote.FamFinancial);
					fldval=fldval.Replace("[famFinUrgNote]",fam.ListPats[0].FamFinUrgNote);
					fldval=fldval.Replace("[famRecallDue]",famRecallDue);
					fldval=fldval.Replace("[guarantorHmPhone]",guarantorHmPhone);
					fldval=fldval.Replace("[guarantorNameF]",guarantorNameF);
					fldval=fldval.Replace("[guarantorNameFL]",guarantorNameFL);
					fldval=fldval.Replace("[guarantorNameL]",guarantorNameL);
					fldval=fldval.Replace("[guarantorNamePref]",guarantorNamePref);
					fldval=fldval.Replace("[guarantorNameLF]",guarantorNameLF);
					fldval=fldval.Replace("[guarantorWirelessPhone]",guarantorWirelessPhone);
					fldval=fldval.Replace("[guarantorWkPhone]",guarantorWkPhone);
					fldval=fldval.Replace("[gender]",Lan.g("enumPatientGender",pat.Gender.ToString()));
					fldval=fldval.Replace("[genderHeShe]",genderHeShe);
					fldval=fldval.Replace("[genderheshe]",genderheshe);
					fldval=fldval.Replace("[genderHimHer]",genderHimHer);
					fldval=fldval.Replace("[genderhimher]",genderhimher);
					fldval=fldval.Replace("[genderHimselfHerself]",genderHimselfHerself);
					fldval=fldval.Replace("[genderhimselfherself]",genderhimselfherself);
					fldval=fldval.Replace("[genderHisHer]",genderHisHer);
					fldval=fldval.Replace("[genderhisher]",genderhisher);
					fldval=fldval.Replace("[genderHisHers]",genderHisHers);
					fldval=fldval.Replace("[genderhishers]",genderhishers);
					fldval=fldval.Replace("[HmPhone]",pat.HmPhone);
					fldval=fldval.Replace("[insAnnualMax]",insAnnualMax);
					fldval=fldval.Replace("[insDeductible]",insDeductible);
					fldval=fldval.Replace("[insDeductibleUsed]",insDeductibleUsed);
					fldval=fldval.Replace("[insEmployer]",insEmployer);
					fldval=fldval.Replace("[insFeeSchedule]",insFeeSchedule);
					fldval=fldval.Replace("[insFreqBW]",insFreqBW.TrimEnd());
					fldval=fldval.Replace("[insFreqExams]",insFreqExams.TrimEnd());
					fldval=fldval.Replace("[insFreqPanoFMX]",insFreqPanoFMX.TrimEnd());
					fldval=fldval.Replace("[insPending]",insPending);
					fldval=fldval.Replace("[insPercentages]",insPercentages);
					fldval=fldval.Replace("[insPlanGroupNumber]",insPlanGroupNumber);
					fldval=fldval.Replace("[insPlanGroupName]",insPlanGroupName);
					fldval=fldval.Replace("[insPlanNote]",insPlanNote);
					fldval=fldval.Replace("[insType]",insType);
					fldval=fldval.Replace("[insSubBirthDate]",insSubBirthDate);
					fldval=fldval.Replace("[insSubNote]",insSubNote);
					fldval=fldval.Replace("[insRemaining]",insRemaining);
					fldval=fldval.Replace("[insUsed]",insUsed);
					fldval=fldval.Replace("[ins2AnnualMax]",ins2AnnualMax);
					fldval=fldval.Replace("[ins2Deductible]",ins2Deductible);
					fldval=fldval.Replace("[ins2DeductibleUsed]",ins2DeductibleUsed);
					fldval=fldval.Replace("[ins2Employer]",ins2Employer);
					fldval=fldval.Replace("[ins2FreqBW]",ins2FreqBW.TrimEnd());
					fldval=fldval.Replace("[ins2FreqExams]",ins2FreqExams.TrimEnd());
					fldval=fldval.Replace("[ins2FreqPanoFMX]",ins2FreqPanoFMX.TrimEnd());
					fldval=fldval.Replace("[ins2PlanGroupNumber]",ins2PlanGroupNumber);
					fldval=fldval.Replace("[ins2PlanGroupName]",ins2PlanGroupName);
					fldval=fldval.Replace("[ins2Pending]",ins2Pending);
					fldval=fldval.Replace("[ins2Percentages]",ins2Percentages);
					fldval=fldval.Replace("[ins2Remaining]",ins2Remaining);
					fldval=fldval.Replace("[ins2Used]",ins2Used);
					fldval=fldval.Replace("[medicalSummary]",medicalSummary);
					fldval=fldval.Replace("[MedUrgNote]",pat.MedUrgNote);
					fldval=fldval.Replace("[nameF]",pat.FName);
					fldval=fldval.Replace("[nameFL]",pat.GetNameFL());
					fldval=fldval.Replace("[nameFLFormal]",pat.GetNameFLFormal());
					fldval=fldval.Replace("[nameL]",pat.LName);
					fldval=fldval.Replace("[nameLF]",pat.GetNameLF());
					fldval=fldval.Replace("[nameMI]",pat.MiddleI);
					fldval=fldval.Replace("[namePref]",pat.Preferred);
					fldval=fldval.Replace("[nextSchedApptDate]",nextSchedApptDate);
					fldval=fldval.Replace("[nextSchedApptDateT]",nextSchedApptDateT);
					fldval=fldval.Replace("[nextSchedApptsFam]",nextSchedApptsFam.TrimEnd());
					fldval=fldval.Replace("[PatNum]",pat.PatNum.ToString());
					fldval=fldval.Replace("[famPopups]",famPopups);
					fldval=fldval.Replace("[plannedAppointmentInfo]",plannedAppointmentInfo);
					fldval=fldval.Replace("[premedicateYN]",premedicateYN);
					fldval=fldval.Replace("[priProvNameFormal]",priProv.GetFormalName());
					fldval=fldval.Replace("[recallInterval]",recallInterval);
					fldval=fldval.Replace("[recallScheduledYN]",recallScheduledYN);
					fldval=fldval.Replace("[referredFrom]",referredFrom);
					fldval=fldval.Replace("[referredTo]",referredTo.TrimEnd());
					fldval=fldval.Replace("[salutation]",pat.GetSalutation());
					fldval=fldval.Replace("[serviceNote]",serviceNote);
					fldval=fldval.Replace("[siteDescription]",Sites.GetDescription(pat.SiteNum));
					fldval=fldval.Replace("[SSN]",pat.SSN);
					fldval=fldval.Replace("[subscriberID]",subscriberId);
					fldval=fldval.Replace("[subscriberNameFL]",subscriberNameFL);
					fldval=fldval.Replace("[subscriber2NameFL]",subscriber2NameFL);
					fldval=fldval.Replace("[timeNow]",DateTime.Now.ToShortTimeString());
					fldval=fldval.Replace("[tpResponsPartyAddress]",tpResponsPartyAddress);
					fldval=fldval.Replace("[tpResponsPartyCityStZip]",tpResponsPartyCityStZip);
					fldval=fldval.Replace("[tpResponsPartyNameFL]",tpResponsPartyNameFL);
					fldval=fldval.Replace("[treatmentNote]",treatmentNote);
					fldval=fldval.Replace("[treatmentPlanProcs]",treatmentPlanProcs);
					fldval=fldval.Replace("[treatmentPlanProcsPriority]",treatmentPlanProcsPriority);
					fldval=fldval.Replace("[WirelessPhone]",pat.WirelessPhone);
					fldval=fldval.Replace("[WkPhone]",pat.WkPhone);
				}
				fldval=fldval.Replace("[dateToday]",DateTime.Today.ToShortDateString());
				fldval=fldval.Replace("[practiceTitle]",PrefC.GetString(PrefName.PracticeTitle));
				field.FieldValue=fldval;
			}
			#endregion
			#region Fill Exam Sheet Fields
			//Fill Exam Sheet Fields----------------------------------------------------------------------------------------------
			//Example: ExamSheet:MyExamSheet;MyField
			if(sheet.SheetType==SheetTypeEnum.PatientLetter && pat!=null) {
				foreach(SheetField field in sheet.SheetFields) {
					if(field.FieldType!=SheetFieldType.StaticText) {
						continue;
					}
					fldval=field.FieldValue;
					string rgx=@"\[ExamSheet\:([^;]+);([^\]]+)\]";
					Match match=Regex.Match(fldval,rgx);
					while(match.Success) {
						string examSheetDescript=match.Result("$1");
						string fieldName=match.Result("$2");
						List<SheetField> examFields=SheetFields.GetFieldFromExamSheet(pat.PatNum,examSheetDescript,fieldName);//Either a list of fields (if radio button) or single field
						if(examFields==null || examFields.Count==0) {
							match=match.NextMatch();
							continue;
						}
						if(examFields[0].RadioButtonGroup!="") {//a user defined 'misc' radio button check box, find the selected item and replace with reportable name
							for(int i=0;i<examFields.Count;i++) {
								if(examFields[i].FieldValue=="X") {
									fldval=fldval.Replace(match.Value,examFields[i].ReportableName);//each radio button in the group has a different reportable name.
									break;
								}
							}
						}
						else if(examFields[0].ReportableName!="") {//not a radio button, so either user defined single misc check boxes or misc input field with reportable name
							fldval=fldval.Replace(match.Value,examFields[0].FieldValue);//checkboxes from exam sheets will show as X or blank on letter.
						}
						else if(examFields[0].FieldName!="" && examFields[0].FieldName!="misc") {//internally defined
							if(examFields[0].RadioButtonValue=="") {//internally defined field, not part of a radio button group
								fldval=fldval.Replace(match.Value,examFields[0].FieldValue);//checkbox or input
							}
							else {//internally defined radio button, look for one selected
								for(int i=0;i<examFields.Count;i++) {
									if(examFields[i].FieldValue=="X") {
										fldval=fldval.Replace(match.Value,examFields[i].RadioButtonValue);
										break;
									}
								}
							}
						}
						match=match.NextMatch();
					}//while
					field.FieldValue=fldval;
				}
			}
			#endregion
		}

		private static string StripPhoneBeyondSpace(string phone) {
			if(!phone.Contains(" ")) {
				return phone;
			}
			int idx=phone.IndexOf(" ");
			return phone.Substring(0,idx);
		}

		///<summary>For new sheets only. Sets sheetField.FieldValue=DocumentNum(FK) based on documentCategoryNum (stored in SheetField.FieldName) and pat.PatNum.
		///<para>Example: if DocCategory=132 (PatImages) then FieldValue would be set equal to the DocNum of the most recent PatImage in the patient's image folder.  
		///If there are no images in the patient's folder, FieldValue will be blank.</para></summary>
		private static void FillPatientImages(Sheet sheet,Patient pat){
			Document[] docList=new Document[0];
			if(pat!=null) {
				docList=Documents.GetAllWithPat(pat.PatNum);
			}
			foreach(SheetField field in sheet.SheetFields){
				if(field.FieldType!=SheetFieldType.PatImage){
					continue;//only examine PatImage fields
				}
				field.FieldValue="";
				long categoryNum=PIn.Long(field.FieldName);
				//itterate backwards to find most recent
				for(int i=docList.Length-1;i>=0;i--) {
					if(docList[i].DocCategory!=categoryNum) {
						continue;
					}
					//At this point we should have found the most recent document in the document category.
					field.FieldValue=docList[i].DocNum.ToString();
					break;
				}//end docList
			}//end foreach field
		}

		private static void FillFieldsForLabelPatient(Sheet sheet,Patient pat){
			foreach(SheetField field in sheet.SheetFields){
				switch(field.FieldName){
					case "nameFL":
						field.FieldValue=pat.GetNameFLFormal();
						break;
					case "nameLF":
						field.FieldValue=pat.GetNameLF();
						break;
					case "address":
						field.FieldValue=pat.Address;
						if(pat.Address2!=""){
							field.FieldValue+="\r\n"+pat.Address2;
						}
						break;
					case "cityStateZip":
						field.FieldValue=pat.City+", "+pat.State+" "+pat.Zip;
						break;
					case "ChartNumber":
						field.FieldValue=pat.ChartNumber;
						break;
					case "PatNum":
						field.FieldValue=pat.PatNum.ToString();
						break;
					case "dateTime.Today":
						field.FieldValue=DateTime.Today.ToShortDateString();
						break;
					case "birthdate":
						//only a temporary workaround:
						field.FieldValue="BD: "+pat.Birthdate.ToShortDateString();
						break;
					case "priProvName":
						field.FieldValue=Providers.GetLongDesc(pat.PriProv);
						break;
					case "text":
						//If the user sets a Label Text as their patient label, then the "text" param will be null.
						//We will handle this case by manually setting the field value to name and address here.
						SheetParameter paramCur=GetParamByName(sheet,"text");
						if(paramCur==null) {
							field.FieldValue=pat.FName+" "+pat.LName+"\r\n"+pat.Address+"\r\n"+pat.City+", "+pat.State+" "+pat.Zip+"\r\n";
						}
						else {
							field.FieldValue=paramCur.ParamValue.ToString();
						}
						break;
				}
			}

			
		}

		private static void FillFieldsForLabelCarrier(Sheet sheet,Carrier carrier) {
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "CarrierName":
						field.FieldValue=carrier.CarrierName;
						break;
					case "address":
						field.FieldValue=carrier.Address;
						if(carrier.Address2!="") {
							field.FieldValue+="\r\n"+carrier.Address2;
						}
						break;
					case "cityStateZip":
						field.FieldValue=carrier.City+", "+carrier.State+" "+carrier.Zip;
						break;
				}
			}
		}

		private static void FillFieldsForLabelReferral(Sheet sheet,Referral refer) {
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "nameFL":
						field.FieldValue=Referrals.GetNameFL(refer.ReferralNum);
						break;
					case "address":
						field.FieldValue=refer.Address;
						if(refer.Address2!="") {
							field.FieldValue+="\r\n"+refer.Address2;
						}
						break;
					case "cityStateZip":
						field.FieldValue=refer.City+", "+refer.ST+" "+refer.Zip;
						break;
				}
			}
		}

		private static void FillFieldsForReferralSlip(Sheet sheet,Patient pat,Referral refer) {
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "referral.nameFL":
						field.FieldValue=Referrals.GetNameFL(refer.ReferralNum);
						break;
					case "referral.address":
						field.FieldValue=refer.Address;
						if(refer.Address2!="") {
							field.FieldValue+="\r\n"+refer.Address2;
						}
						break;
					case "referral.cityStateZip":
						field.FieldValue=refer.City+", "+refer.ST+" "+refer.Zip;
						break;
					case "referral.phone":
						field.FieldValue="";
						if(refer.Telephone.Length==10){
							field.FieldValue="("+refer.Telephone.Substring(0,3)+")"
								+refer.Telephone.Substring(3,3)+"-"
								+refer.Telephone.Substring(6);
						}
						break;
					case "referral.phone2":
						field.FieldValue=refer.Phone2;
						break;
					case "patient.nameFL":
						field.FieldValue=pat.GetNameFL();
						break;
					case "dateTime.Today":
						field.FieldValue=DateTime.Today.ToShortDateString();
						break;
					case "patient.WkPhone":
						field.FieldValue=pat.WkPhone;
						break;
					case "patient.HmPhone":
						field.FieldValue=pat.HmPhone;
						break;
					case "patient.WirelessPhone":
						field.FieldValue=pat.WirelessPhone;
						break;
					case "patient.address":
						field.FieldValue=pat.Address;
						if(pat.Address2!="") {
							field.FieldValue+="\r\n"+pat.Address2;
						}
						break;
					case "patient.cityStateZip":
						field.FieldValue=pat.City+", "+pat.State+" "+pat.Zip;
						break;
					case "patient.provider":
						field.FieldValue=Providers.GetProv(Patients.GetProvNum(pat)).GetFormalName();
						break;
					//case "notes"://an input field
				}
			}
		}

		private static void FillFieldsForLabelAppointment(Sheet sheet,Appointment appt,Patient pat) {
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "nameFL":
						field.FieldValue=pat.GetNameFirstOrPrefL();
						break;
					case "nameLF":
						field.FieldValue=pat.GetNameLF();
						break;
					case "weekdayDateTime":
						field.FieldValue=appt.AptDateTime.ToString("ddd")+"   "
							+appt.AptDateTime.ToShortDateString()+"  "
							+appt.AptDateTime.ToShortTimeString();//  h:mm tt");
						break;
					case "length":
						int minutesTotal=appt.Pattern.Length*5;
						int hours=minutesTotal/60;//automatically rounds down
						int minutes=minutesTotal-hours*60;
						field.FieldValue="";
						if(hours>0){
							field.FieldValue=hours.ToString()+" hours, ";
						}
						field.FieldValue+=minutes.ToString()+" min";
						break;
				}
			}
		}

		private static void FillFieldsForRx(Sheet sheet,RxPat rx,Patient pat,Provider prov) {
			Clinic clinic=null;
			if(pat.ClinicNum != 0) {
				clinic=Clinics.GetClinic(pat.ClinicNum);
			}
			string text;
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "prov.nameFL":
						field.FieldValue=prov.GetFormalName();
						break;
					case "prov.address":
						if(clinic==null) {
							field.FieldValue=PrefC.GetString(PrefName.PracticeAddress);
							if(PrefC.GetString(PrefName.PracticeAddress2)!="") {
								field.FieldValue+="\r\n"+PrefC.GetString(PrefName.PracticeAddress2);
							}
						}
						else{
							field.FieldValue=clinic.Address;
							if(clinic.Address2!="") {
								field.FieldValue+="\r\n"+clinic.Address2;
							}
						}
						break;
					case "prov.cityStateZip":
						if(clinic==null) {
							field.FieldValue=PrefC.GetString(PrefName.PracticeCity)+", "+PrefC.GetString(PrefName.PracticeST)+" "+PrefC.GetString(PrefName.PracticeZip);
						}
						else {
							field.FieldValue=clinic.City+", "+clinic.State+" "+clinic.Zip;
						}
						break;
					case "prov.phone":
						if(clinic==null) {
							text=PrefC.GetString(PrefName.PracticePhone);
						}
						else {
							text=clinic.Phone;
						}
						field.FieldValue=text;
						if(text.Length==10) {
							field.FieldValue="("+text.Substring(0,3)+")"+text.Substring(3,3)+"-"+text.Substring(6);
						}
						break;
					case "RxDate":
						field.FieldValue=rx.RxDate.ToShortDateString();
						break;
					case "RxDateMonthSpelled":
						field.FieldValue=rx.RxDate.ToString("MMM dd,yyyy");
						break;
					case "prov.dEANum":
						if(rx.IsControlled){
							field.FieldValue=prov.DEANum;
						}
						else{
							field.FieldValue="";
						}
						break;
					case "pat.nameFL":
						//Can't include preferred, so:
						field.FieldValue=pat.FName+" "+pat.MiddleI+"  "+pat.LName;
						break;
					case "pat.Birthdate":
						if(pat.Birthdate.Year<1880){
							field.FieldValue="";
						}
						else{
							field.FieldValue=pat.Birthdate.ToShortDateString();
						}
						break;
					case "pat.HmPhone":
						field.FieldValue=pat.HmPhone;
						break;
					case "pat.address":
						field.FieldValue=pat.Address;
						if(pat.Address2!=""){
							field.FieldValue+="\r\n"+pat.Address2;
						}
						break;
					case "pat.cityStateZip":
						field.FieldValue=pat.City+", "+pat.State+" "+pat.Zip;
						break;
					case "Drug":
						field.FieldValue=rx.Drug;
						break;
					case "Disp":
						field.FieldValue=rx.Disp;
						break;
					case "Sig":
						field.FieldValue=rx.Sig;
						break;
					case "Refills":
						field.FieldValue=rx.Refills;
						break;	
					case "prov.stateRxID":
						field.FieldValue=prov.StateRxID;
						break;
					case "prov.StateLicense":
						field.FieldValue=prov.StateLicense;
						break;
					case "prov.NationalProvID":
						field.FieldValue=prov.NationalProvID;
						break;
				}
			}
		}

		private static void FillFieldsForConsent(Sheet sheet,Patient pat) {
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "patient.nameFL":
						field.FieldValue=pat.GetNameFL();
						break;
					case "dateTime.Today":
						field.FieldValue=DateTime.Today.ToShortDateString();
						break;
				}
			}
		}

		private static void FillFieldsForPatientLetter(Sheet sheet,Patient pat) {
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "PracticeTitle":
						field.FieldValue=PrefC.GetString(PrefName.PracticeTitle);
						break;
					case "PracticeAddress":
						field.FieldValue=PrefC.GetString(PrefName.PracticeAddress);
						if(PrefC.GetString(PrefName.PracticeAddress2) != ""){
							field.FieldValue+="\r\n"+PrefC.GetString(PrefName.PracticeAddress2);
						}
						break;
					case "practiceCityStateZip":
						field.FieldValue=PrefC.GetString(PrefName.PracticeCity)+", "
							+PrefC.GetString(PrefName.PracticeST)+"  "
							+PrefC.GetString(PrefName.PracticeZip);
						break;
					case "patient.nameFL":
						field.FieldValue=pat.GetNameFLFormal();
						break;
					case "patient.address":
						field.FieldValue=pat.Address;
						if(pat.Address2!="") {
							field.FieldValue+="\r\n"+pat.Address2;
						}
						break;
					case "patient.cityStateZip":
						field.FieldValue=pat.City+", "+pat.State+" "+pat.Zip;
						break;
					case "today.DayDate":
						field.FieldValue=DateTime.Today.ToString("dddd")+", "+DateTime.Today.ToShortDateString();
						break;
					case "patient.salutation":
						field.FieldValue="Dear "+pat.GetSalutation()+":";
						break;
					case "patient.priProvNameFL":
						field.FieldValue=Providers.GetFormalName(pat.PriProv);
						break;
				}
			}
		}

		private static void FillFieldsForReferralLetter(Sheet sheet,Patient pat,Referral refer) {
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "PracticeTitle":
						field.FieldValue=PrefC.GetString(PrefName.PracticeTitle);
						break;
					case "PracticeAddress":
						field.FieldValue=PrefC.GetString(PrefName.PracticeAddress);
						if(PrefC.GetString(PrefName.PracticeAddress2) != ""){
							field.FieldValue+="\r\n"+PrefC.GetString(PrefName.PracticeAddress2);
						}
						break;
					case "practiceCityStateZip":
						field.FieldValue=PrefC.GetString(PrefName.PracticeCity)+", "
							+PrefC.GetString(PrefName.PracticeST)+"  "
							+PrefC.GetString(PrefName.PracticeZip);
						break;
					case "referral.phone":
						field.FieldValue="";
						if(refer.Telephone.Length==10) {
							field.FieldValue="("+refer.Telephone.Substring(0,3)+")"
								+refer.Telephone.Substring(3,3)+"-"
								+refer.Telephone.Substring(6);
						}
						break;
					case "referral.phone2":
						field.FieldValue=refer.Phone2;
						break;
					case "referral.nameFL":
						field.FieldValue=Referrals.GetNameFL(refer.ReferralNum);
						break;
					case "referral.address":
						field.FieldValue=refer.Address;
						if(refer.Address2!="") {
							field.FieldValue+="\r\n"+refer.Address2;
						}
						break;
					case "referral.cityStateZip":
						field.FieldValue=refer.City+", "+refer.ST+" "+refer.Zip;
						break;
					case "today.DayDate":
						field.FieldValue=DateTime.Today.ToString("dddd")+", "+DateTime.Today.ToShortDateString();
						break;
					case "patient.nameFL":
						field.FieldValue=pat.GetNameFL();
						break;
					case "referral.salutation":
						field.FieldValue="Dear "+refer.FName+":";
						break;
					case "patient.priProvNameFL":
						field.FieldValue=Providers.GetFormalName(pat.PriProv);
						break;
				}
			}
		}

		private static void FillFieldsForPatientForm(Sheet sheet,Patient pat) {
			Family fam=Patients.GetFamily(pat.PatNum);
			List<PatPlan> patPlanList=PatPlans.Refresh(pat.PatNum);
			List<InsSub> subList=InsSubs.RefreshForFam(fam);
			List<InsPlan> planList=InsPlans.RefreshForSubList(subList);
			InsPlan insplan1=null;
			InsSub sub1=null;
			Carrier carrier1=null;
			if(patPlanList.Count>0){
				sub1=InsSubs.GetSub(patPlanList[0].InsSubNum,subList);
				insplan1=InsPlans.GetPlan(sub1.PlanNum,planList);
				carrier1=Carriers.GetCarrier(insplan1.CarrierNum);
			}
			InsPlan insplan2=null;
			InsSub sub2=null;
			Carrier carrier2=null;
			if(patPlanList.Count>1) {
				sub2=InsSubs.GetSub(patPlanList[1].InsSubNum,subList);
				insplan2=InsPlans.GetPlan(sub2.PlanNum,planList);
				carrier2=Carriers.GetCarrier(insplan2.CarrierNum);
			}
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "Address":
						field.FieldValue=pat.Address;
						break;
					case "Address2":
						field.FieldValue=pat.Address2;
						break;
					case "addressAndHmPhoneIsSameEntireFamily":
						bool isSame=true;
						for(int i=0;i<fam.ListPats.Length;i++){
							if(pat.HmPhone!=fam.ListPats[i].HmPhone
								|| pat.Address!=fam.ListPats[i].Address
								|| pat.Address2!=fam.ListPats[i].Address2
								|| pat.City!=fam.ListPats[i].City
								|| pat.State!=fam.ListPats[i].State
								|| pat.Zip!=fam.ListPats[i].Zip)
							{
								isSame=false;
								break;
							}
						}
						if(isSame) {
							field.FieldValue="X";
						}
						break;
					case "Birthdate":
						field.FieldValue=pat.Birthdate.ToShortDateString();
						break;
					case "City":
						field.FieldValue=pat.City;
						break;
					case "Email":
						field.FieldValue=pat.Email;
						break;
					case "FName":
						field.FieldValue=pat.FName;
						break;
					case "Gender":
						if(field.RadioButtonValue==pat.Gender.ToString()) {
							field.FieldValue="X";
						}
						break;
					case "HmPhone":
						field.FieldValue=pat.HmPhone;
						break;
					case "ins1CarrierName":
						if(carrier1!=null){
							field.FieldValue=carrier1.CarrierName;
						}
						break;
					case "ins1CarrierPhone":
						if(carrier1!=null) {
							field.FieldValue=carrier1.Phone;
						}
						break;
					case "ins1EmployerName":
						if(insplan1!=null) {
							field.FieldValue=Employers.GetName(insplan1.EmployerNum);
						}
						break;
					case "ins1GroupName":
						if(insplan1!=null) {
							field.FieldValue=insplan1.GroupName;
						}
						break;
					case "ins1GroupNum":
						if(insplan1!=null) {
							field.FieldValue=insplan1.GroupNum;
						}
						break;
					case "ins1Relat":
						if(patPlanList.Count>0 && patPlanList[0].Relationship.ToString()==field.RadioButtonValue) {
							field.FieldValue="X";
						}
						break;
					case "ins1SubscriberID":
						if(insplan1!=null) {
							field.FieldValue=sub1.SubscriberID;
						}
						break;
					case "ins1SubscriberNameF":
						if(insplan1!=null) {
							field.FieldValue=fam.GetNameInFamFirst(sub1.Subscriber);
						}
						break;
					case "ins2CarrierName":
						if(carrier2!=null) {
							field.FieldValue=carrier2.CarrierName;
						}
						break;
					case "ins2CarrierPhone":
						if(carrier2!=null) {
							field.FieldValue=carrier2.Phone;
						}
						break;
					case "ins2EmployerName":
						if(insplan2!=null) {
							field.FieldValue=Employers.GetName(insplan2.EmployerNum);
						}
						break;
					case "ins2GroupName":
						if(insplan2!=null) {
							field.FieldValue=insplan2.GroupName;
						}
						break;
					case "ins2GroupNum":
						if(insplan2!=null) {
							field.FieldValue=insplan2.GroupNum;
						}
						break;
					case "ins2Relat":
						if(patPlanList.Count>1 && patPlanList[1].Relationship.ToString()==field.RadioButtonValue) {
							field.FieldValue="X";
						}
						break;
					case "ins2SubscriberID":
						if(insplan2!=null) {
							field.FieldValue=sub2.SubscriberID;
						}
						break;
					case "ins2SubscriberNameF":
						if(insplan2!=null) {
							field.FieldValue=fam.GetNameInFamFirst(sub2.Subscriber);
						}
						break;
					case "LName":
						field.FieldValue=pat.LName;
						break;
					case "MiddleI":
						field.FieldValue=pat.MiddleI;
						break;
					case "Position":
						if(pat.Position.ToString()==field.RadioButtonValue) {
							field.FieldValue="X";
						}
						break;
					case "PreferConfirmMethod":
						if(pat.PreferConfirmMethod.ToString()==field.RadioButtonValue) {
							field.FieldValue="X";
						}
						break;
					case "PreferContactMethod":
						if(pat.PreferContactMethod.ToString()==field.RadioButtonValue) {
							field.FieldValue="X";
						}
						break;
					case "PreferRecallMethod":
						if(pat.PreferRecallMethod.ToString()==field.RadioButtonValue) {
							field.FieldValue="X";
						}
						break;
					case "Preferred":
						field.FieldValue=pat.Preferred;
						break;
					case "referredFrom":
						Referral referral=Referrals.GetReferralForPat(pat.PatNum);
						if(referral!=null){
							field.FieldValue=Referrals.GetNameFL(referral.ReferralNum);
						}
						break;
					case "SSN":
						if(CultureInfo.CurrentCulture.Name=="en-US" && pat.SSN.Length==9){//and length exactly 9 (no data gets lost in formatting)
							field.FieldValue=pat.SSN.Substring(0,3)+"-"+pat.SSN.Substring(3,2)+"-"+pat.SSN.Substring(5,4);
						}
						else {
							field.FieldValue=pat.SSN;
						}
						break;
					case "State":
						field.FieldValue=pat.State;
						break;
					case "StudentStatus":
						if(pat.StudentStatus=="F" && field.RadioButtonValue=="Fulltime") {
							field.FieldValue="X";
						}
						if(pat.StudentStatus=="N" && field.RadioButtonValue=="Nonstudent") {
							field.FieldValue="X";
						}
						if(pat.StudentStatus=="P" && field.RadioButtonValue=="Parttime") {
							field.FieldValue="X";
						}

						break;
					case "WirelessPhone":
						field.FieldValue=pat.WirelessPhone;
						break;
					case "wirelessCarrier":
						field.FieldValue="";//not implemented
						break;
					case "WkPhone":
						field.FieldValue=pat.WkPhone;
						break;
					case "Zip":
						field.FieldValue=pat.Zip;
						break;
				}
			}
		}

		private static void FillFieldsForRoutingSlip(Sheet sheet,Patient pat,Appointment apt) {
			Family fam=Patients.GetFamily(apt.PatNum);
			LabCase labForApt=LabCases.GetForApt(apt.AptNum);
			string str;
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "appt.timeDate":
						field.FieldValue=apt.AptDateTime.ToShortTimeString()+"  "+apt.AptDateTime.ToShortDateString();
						break;
					case "appt.length":
						field.FieldValue=(apt.Pattern.Length*5).ToString()+" "+Lan.g("SheetRoutingSlip","minutes");
						break;
					case "appt.providers":
						str=Providers.GetLongDesc(apt.ProvNum);
						if(apt.ProvHyg!=0){
							str+="\r\n"+Providers.GetLongDesc(apt.ProvHyg);
						}
						field.FieldValue=str;
						break;
					case "appt.procedures":
						str="";
						List<Procedure> procs=Procedures.GetProcsForSingle(apt.AptNum,false);
						bool isOnlyTP=true;
						for(int i=0;i<procs.Count;i++) {
							if(procs[i].ProcStatus!=ProcStat.TP) {
								isOnlyTP=false;
								break;
							}
						}
						if(isOnlyTP) {
							Procedure[] procListTP=Procedures.GetListTPandTPi(procs);//this sorts.  Doesn't work unless all are TP.
							for(int i=0;i<procListTP.Length;i++) {
								if(i>0) {
									str+="\r\n";
								}
								str+=Procedures.GetDescription(procListTP[i]);
							}
						}
						else {
							for(int i=0;i<procs.Count;i++) {
								if(i>0) {
									str+="\r\n";
								}
								str+=Procedures.GetDescription(procs[i]);
							}
						}
						field.FieldValue=str;
						break;
					case "appt.Note":
						field.FieldValue=apt.Note;
						break;
					case "otherFamilyMembers":
						str="";
						for(int i=0;i<fam.ListPats.Length;i++) {
							if(fam.ListPats[i].PatNum==pat.PatNum) {
								continue;
							}
							if(fam.ListPats[i].PatStatus==PatientStatus.Archived
								|| fam.ListPats[i].PatStatus==PatientStatus.Deceased) {
								//Prospective patients will show.
								continue;
							}
							if(str!="") {
								str+="\r\n";
							}
							str+=fam.ListPats[i].GetNameFL();
							if(fam.ListPats[i].Age>0){
								str+=",   "+fam.ListPats[i].Age.ToString();
							}
						}
						field.FieldValue=str;
						break;
					case "labName":
						if(labForApt!=null) {
							field.FieldValue=Laboratories.GetOne(labForApt.LaboratoryNum).Description;
						}
						break;
					case "dateLabSent":
						if(labForApt!=null) {
							if(labForApt.DateTimeSent==DateTime.MinValue) {
								field.FieldValue="Not Sent";
							}
							else {
								field.FieldValue=labForApt.DateTimeSent.ToShortDateString();
							}
						}
						break;
					case "dateLabReceived":
						if(labForApt!=null) {
							if(labForApt.DateTimeRecd==DateTime.MinValue) {
								field.FieldValue="Not Received";
							}
							else {
								field.FieldValue=labForApt.DateTimeRecd.ToShortDateString();
							}
						}
						break;
				}
			}
		}

		private static void FillFieldsForMedicalHistory(Sheet sheet,Patient pat) {
			List<SheetField> inputMedList=new List<SheetField>();
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "Birthdate":
						field.FieldValue=pat.Birthdate.ToShortDateString();
						continue;
					case "FName":
						field.FieldValue=pat.FName;
						continue;
					case "LName":
						field.FieldValue=pat.LName;
						continue;
				}
				if(field.FieldType==SheetFieldType.CheckBox) {
					if(field.FieldName.StartsWith("allergy:")) {//"allergy:Pen"
						List<Allergy> allergies=Allergies.GetAll(pat.PatNum,true);
						for(int i=0;i<allergies.Count;i++) {
							if(AllergyDefs.GetDescription(allergies[i].AllergyDefNum)==field.FieldName.Remove(0,8)) {
								if(allergies[i].StatusIsActive && field.RadioButtonValue=="Y") {
									field.FieldValue="X";
								}
								else if(!allergies[i].StatusIsActive && field.RadioButtonValue=="N") {
									field.FieldValue="X";
								}
								break;
							}
						}
					}
					else if(field.FieldName.StartsWith("problem:")) {//"problem:Hepatitis B"
						List<Disease> diseases=Diseases.Refresh(pat.PatNum,false);
						for(int i=0;i<diseases.Count;i++) {
							if(DiseaseDefs.GetName(diseases[i].DiseaseDefNum)==field.FieldName.Remove(0,8)) {
								if(diseases[i].ProbStatus==ProblemStatus.Active && field.RadioButtonValue=="Y") {
									field.FieldValue="X";
								}
								else if(diseases[i].ProbStatus!=ProblemStatus.Active && field.RadioButtonValue=="N") {
									field.FieldValue="X";
								}
								break;
							}
						}
					}
				}
				else if(field.FieldType==SheetFieldType.InputField && field.FieldName.StartsWith("inputMed")) {
					inputMedList.Add(field);
				}
			}
			//Special logic for checkMed and inputMed.
			if(inputMedList.Count>0) {
				inputMedList.Sort(CompareSheetFieldNames);
				//Loop through the patients medications and fill in the input fields.
				List<MedicationPat> medPatList=MedicationPats.Refresh(pat.PatNum,false);
				for(int i=0;i<medPatList.Count;i++) {
					if(i==inputMedList.Count) {
						break;//Pat has more medications than inputMed fields on sheet.
					}
					if(medPatList[i].MedicationNum==0) {
						inputMedList[i].FieldValue=medPatList[i].MedDescript;
					}
					else {
						inputMedList[i].FieldValue=Medications.GetDescription(medPatList[i].MedicationNum);
					}
					inputMedList[i].FieldType=SheetFieldType.OutputText;//Don't try to import as a new medication.
				}
			}
		}

		private static void FillFieldsForLabCase(Sheet sheet,Patient pat,LabCase labcase) {
			Laboratory lab=Laboratories.GetOne(labcase.LaboratoryNum);//might possibly be null
			Provider prov=Providers.GetProv(labcase.ProvNum);
			Appointment appt=Appointments.GetOneApt(labcase.AptNum);//might be null
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "lab.Description":
						if(lab!=null){
							field.FieldValue=lab.Description;
						}
						break;
					case "lab.Phone":
						if(lab!=null){
							field.FieldValue=lab.Phone;
						}
						break;
					case "lab.Notes":
						if(lab!=null){
							field.FieldValue=lab.Notes;
						}
						break;
					case "lab.WirelessPhone":
						if(lab!=null){
							field.FieldValue=lab.WirelessPhone;
						}
						break;
					case "lab.Address":
						if(lab!=null){
							field.FieldValue=lab.Address;
						}
						break;
					case "lab.CityStZip":
						if(lab!=null){
							field.FieldValue=lab.City+", "+lab.State+" "+lab.Zip;
						}
						break;
					case "lab.Email":
						if(lab!=null){
							field.FieldValue=lab.Email;
						}
						break;
					case "appt.DateTime":
						if(appt!=null) {
							field.FieldValue=appt.AptDateTime.ToShortDateString()+"  "+appt.AptDateTime.ToShortTimeString();
						}
						break;
					case "labcase.DateTimeDue":
						field.FieldValue=labcase.DateTimeDue.ToShortDateString()+"  "+labcase.DateTimeDue.ToShortTimeString();
						break;
					case "labcase.DateTimeCreated":
						field.FieldValue=labcase.DateTimeCreated.ToShortDateString()+"  "+labcase.DateTimeCreated.ToShortTimeString();
						break;
					case "labcase.Instructions":
						field.FieldValue=labcase.Instructions;
						break;
					case "prov.nameFormal":
						field.FieldValue=prov.GetFormalName();
						break;
					case "prov.stateLicence":
						field.FieldValue=prov.StateLicense;
						break;
				}
			}
		}

		private static void FillFieldsForExamSheet(Sheet sheet,Patient pat) {
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "Birthdate":
						field.FieldValue=pat.Birthdate.ToShortDateString();
						break;
					case "FName":
						field.FieldValue=pat.FName;
						break;
					case "Gender":
						if(field.RadioButtonValue==pat.Gender.ToString()) {
							field.FieldValue="X";
						}
						break;
					case "LName":
						field.FieldValue=pat.LName;
						break;
					case "MiddleI":
						field.FieldValue=pat.MiddleI;
						break;
					case "patient.priProvNameFL":
						field.FieldValue=Providers.GetFormalName(pat.PriProv);
						break;
					case "Preferred":
						field.FieldValue=pat.Preferred;
						break;
					case "Race":
						if(field.RadioButtonValue==PatientRaces.GetPatientRaceOldFromPatientRaces(pat.PatNum).ToString()) { //==pat.Race.ToString()) {
							field.FieldValue="X";
						}
						break;
					case "sheet.DateTimeSheet":
						field.FieldValue=sheet.DateTimeSheet.ToString();
						break;
				}
			}
		}

		private static void FillFieldsForDepositSlip(Sheet sheet,Deposit deposit) {
			List <Payment> PatPayList=Payments.GetForDeposit(deposit.DepositNum);
			ClaimPayment[] ClaimPayList=ClaimPayments.GetForDeposit(deposit.DepositNum);
			List<string[]> depositList=new List<string[]> ();
			int[] colSize=new int[] {11,33,15,14,0};
			for(int i=0;i<PatPayList.Count;i++){
				string amount=PatPayList[i].PayAmt.ToString("F");
				colSize[4]=Math.Max(colSize[4],amount.Length);
			}
			for(int i=0;i<ClaimPayList.Length;i++){
				string amount=ClaimPayList[i].CheckAmt.ToString("F");
				colSize[4]=Math.Max(colSize[4],amount.Length);
			}
			for(int i=0;i<PatPayList.Count;i++){
				string[] depositItem=new string[5];
				string date=PatPayList[i].PayDate.ToShortDateString();
				if(date.Length>colSize[0]){
					date=date.Substring(0,colSize[0]);
				}
				depositItem[0]=date.PadRight(colSize[0],' ')+" ";
				Patient pat=Patients.GetPat(PatPayList[i].PatNum);
				string name=pat.GetNameLF();
				if(name.Length>colSize[1]){
					name=name.Substring(0,colSize[1]);
				}
				depositItem[1]=name.PadRight(colSize[1],' ')+" ";
				string checkNum=PatPayList[i].CheckNum;
				if(checkNum.Length>colSize[2]){
					checkNum=checkNum.Substring(0,colSize[2]);
				}
				depositItem[2]=checkNum.PadRight(colSize[2],' ')+" ";
				string bankBranch=PatPayList[i].BankBranch;
				if(bankBranch.Length>colSize[3]){
					bankBranch=bankBranch.Substring(0,colSize[3]);
				}
				depositItem[3]=bankBranch.PadRight(colSize[3],' ')+" ";
				depositItem[4]=PatPayList[i].PayAmt.ToString("F").PadLeft(colSize[4],' ');
				depositList.Add(depositItem);
			}
			for(int i=0;i<ClaimPayList.Length;i++){
				string[] depositItem=new string[5];
				string date=ClaimPayList[i].CheckDate.ToShortDateString();
				if(date.Length>colSize[0]){
					date=date.Substring(0,colSize[0]);
				}
				depositItem[0]=date.PadRight(colSize[0],' ')+" ";
				string name=ClaimPayList[i].CarrierName;
				if(name.Length>colSize[1]){
					name=name.Substring(0,colSize[1]);
				}
				depositItem[1]=name.PadRight(colSize[1],' ')+" ";
				string checkNum=ClaimPayList[i].CheckNum;
				if(checkNum.Length>colSize[2]){
					checkNum=checkNum.Substring(0,colSize[2]);
				}
				depositItem[2]=checkNum.PadRight(colSize[2],' ')+" ";
				string bankBranch=ClaimPayList[i].BankBranch;
				if(bankBranch.Length>colSize[3]){
					bankBranch=bankBranch.Substring(0,colSize[3]);
				}
				depositItem[3]=bankBranch.PadRight(colSize[3],' ')+" ";
				depositItem[4]=ClaimPayList[i].CheckAmt.ToString("F").PadLeft(colSize[4],' ');
				depositList.Add(depositItem);
			}
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "deposit.BankAccountInfo":
						field.FieldValue=deposit.BankAccountInfo;
						break;
					case "deposit.DateDeposit":
						field.FieldValue=deposit.DateDeposit.ToShortDateString();
						break;
					case "depositList":
						StringBuilder depositListB=new StringBuilder(Lan.g("Deposits","Date").PadRight(12)+Lan.g("Deposits","Name").PadRight(34)
							+Lan.g("Deposits","Check Number").PadRight(16)+Lan.g("Deposits","Bank-Branch").PadRight(15)+Lan.g("Deposits","Amount")+Environment.NewLine);
						for(int i=0;i<depositList.Count;i++){
							if(i>0){
								depositListB.Append(Environment.NewLine);
							}
							for(int j=0;j<5;j++){
								depositListB.Append(depositList[i][j]);
							}
						}
						field.FieldValue=depositListB.ToString();
						break;
					case "depositTotal":
						decimal total=0;
						for(int i=0;i<PatPayList.Count;i++){
							total+=(decimal)PatPayList[i].PayAmt;
						}
						for(int i=0;i<ClaimPayList.Length;i++){
							total+=(decimal)ClaimPayList[i].CheckAmt;
						}
						field.FieldValue=total.ToString("n").PadLeft(12,' ');
						break;
					case "depositItemCount":
						field.FieldValue=(PatPayList.Count+ClaimPayList.Length).ToString().PadLeft(2,'0');
						break;
					case "depositItem01":
						if(depositList.Count>=1){
							field.FieldValue=depositList[0][4].PadLeft(12,' ');
						}
						break;
					case "depositItem02":
						if(depositList.Count>=2){
							field.FieldValue=depositList[1][4].PadLeft(12,' ');
						}
						break;
					case "depositItem03":
						if(depositList.Count>=3){
							field.FieldValue=depositList[2][4].PadLeft(12,' ');
						}
						break;
					case "depositItem04":
						if(depositList.Count>=4){
							field.FieldValue=depositList[3][4].PadLeft(12,' ');
						}
						break;
					case "depositItem05":
						if(depositList.Count>=5){
							field.FieldValue=depositList[4][4].PadLeft(12,' ');
						}
						break;
					case "depositItem06":
						if(depositList.Count>=6){
							field.FieldValue=depositList[5][4].PadLeft(12,' ');
						}
						break;
					case "depositItem07":
						if(depositList.Count>=7){
							field.FieldValue=depositList[6][4].PadLeft(12,' ');
						}
						break;
					case "depositItem08":
						if(depositList.Count>=8){
							field.FieldValue=depositList[7][4].PadLeft(12,' ');
						}
						break;
					case "depositItem09":
						if(depositList.Count>=9){
							field.FieldValue=depositList[8][4].PadLeft(12,' ');
						}
						break;
					case "depositItem10":
						if(depositList.Count>=10){
							field.FieldValue=depositList[9][4].PadLeft(12,' ');
						}
						break;
					case "depositItem11":
						if(depositList.Count>=11){
							field.FieldValue=depositList[10][4].PadLeft(12,' ');
						}
						break;
					case "depositItem12":
						if(depositList.Count>=12){
							field.FieldValue=depositList[11][4].PadLeft(12,' ');
						}
						break;
					case "depositItem13":
						if(depositList.Count>=13){
							field.FieldValue=depositList[12][4].PadLeft(12,' ');
						}
						break;
					case "depositItem14":
						if(depositList.Count>=14){
							field.FieldValue=depositList[13][4].PadLeft(12,' ');
						}
						break;
					case "depositItem15":
						if(depositList.Count>=15){
							field.FieldValue=depositList[14][4].PadLeft(12,' ');
						}
						break;
					case "depositItem16":
						if(depositList.Count>=16){
							field.FieldValue=depositList[15][4].PadLeft(12,' ');
						}
						break;
					case "depositItem17":
						if(depositList.Count>=17){
							field.FieldValue=depositList[16][4].PadLeft(12,' ');
						}
						break;
					case "depositItem18":
						if(depositList.Count>=18){
							field.FieldValue=depositList[17][4].PadLeft(12,' ');
						}
						break;
				}
			}
		}

		private static void FillFieldsForStatement(Sheet sheet,Statement Stmt,DataSet dataSet) {
			Patient pat=null;
			if(Stmt.SuperFamily!=0) {//Superfamily statement
				pat=Patients.GetPat(Stmt.SuperFamily);
			}
			else {
				pat=Patients.GetPat(Stmt.PatNum);
			}
			Patient PatGuar=Patients.GetPat(pat.Guarantor);
			DataTable tableAppt=dataSet.Tables["appts"];
			string[] totInsBalVals=totInsBalValsHelper(sheet,Stmt,dataSet);
			string[] totInsBalLabs=totInsBalLabsHelper(sheet,Stmt);
			if(tableAppt==null){
				tableAppt=new DataTable();	
			}
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "accountNumber":
						#region Account Number
						field.FieldValue=Lan.g("Statements","Account Number")+" ";
						if(PrefC.GetBool(PrefName.StatementAccountsUseChartNumber)) {
							field.FieldValue+=PatGuar.ChartNumber;
						}
						else {
							field.FieldValue+=PatGuar.PatNum;
						}
						#endregion
						break;
					case "futureAppointments":
						#region Future Appointments
						if(!Stmt.IsReceipt && !Stmt.IsInvoice) {
							if(tableAppt.Rows.Count>0) {
								field.FieldValue=Lan.g("Statements","Scheduled Appointments:");
							}
							for(int i=0;i<tableAppt.Rows.Count;i++) {
								field.FieldValue+="\r\n"+tableAppt.Rows[i]["descript"].ToString();
							}
						}
						#endregion
						break;
					case "statement.NoteBold":
						field.FieldValue=Stmt.NoteBold;
						if(field.FieldValue==null) {
							field.FieldValue="";
						}
						break;
					case "statement.Note":
						field.FieldValue=Stmt.Note;
						if(field.FieldValue==null) {
							field.FieldValue="";
						}
						break;
					case "totalLabel":
						field.FieldValue=totInsBalLabs[0];
						break;
					case "insEstLabel":
						field.FieldValue=totInsBalLabs[1];
						break;
					case "balanceLabel":
						field.FieldValue=totInsBalLabs[2];
						break;
					case "totalValue":
						field.FieldValue=totInsBalVals[0];
						break;
					case "insEstValue":
						field.FieldValue=totInsBalVals[1];
						break;
					case "balanceValue":
						field.FieldValue=totInsBalVals[2];
						break;
					case "amountDueValue":
						try {
							field.FieldValue=PIn.Double(SheetUtil.GetDataTableForGridType(sheet,dataSet,"StatementEnclosed",Stmt,null).Rows[0][0].ToString()).ToString("C");
						}
						catch {
							field.FieldValue=0.ToString("C");
						}
						break;
					case "payPlanAmtDueValue":
						DataTable tableMisc=dataSet.Tables["misc"];
						if(tableMisc==null){
							tableMisc=new DataTable();	
						}
						double payPlanDue=tableMisc.Select().Where(x => x["descript"].ToString()=="payPlanDue").Sum(x => PIn.Double(x["value"].ToString()));
						field.FieldValue=payPlanDue.ToString("c");
						break;
					case "statementReceiptInvoice":
						#region Sta/Rec/Inv
						if(Stmt.IsInvoice) {
							if(CultureInfo.CurrentCulture.Name=="en-NZ" || CultureInfo.CurrentCulture.Name=="en-AU") {//New Zealand and Australia
								field.FieldValue=Lan.g("Statements","TAX INVOICE");
							}
							else {
								field.FieldValue=Lan.g("Statements","INVOICE")+" #"+Stmt.StatementNum.ToString();
							}
						}
						else if(Stmt.IsReceipt) {
							field.FieldValue=Lan.g("Statements","RECEIPT");
							if(CultureInfo.CurrentCulture.Name.EndsWith("SG")) {//SG=Singapore
								field.FieldValue+=" #"+Stmt.StatementNum.ToString();
							}
						}
						else {
							field.FieldValue=Lan.g("Statements","STATEMENT");
							if(Stmt.StatementType==StmtType.LimitedStatement) {
								field.FieldValue+=" ("+Lan.g("Statements","Limited")+")";
							}
						}
						#endregion
						break;
					case "returnAddress":
						#region ReturnAddress
						if(!PrefC.GetBool(PrefName.StatementShowReturnAddress)) {
							field.FieldValue="";
							break;
						}
						#region Practice Address
						if(!PrefC.GetBool(PrefName.EasyNoClinics) && Clinics.List.Length>0 //if using clinics
						&& Clinics.GetClinic(PatGuar.ClinicNum)!=null)//and this patient assigned to a clinic
							{
							Clinic clinic=Clinics.GetClinic(PatGuar.ClinicNum);
							field.FieldValue=clinic.Description+"\r\n";
							if(CultureInfo.CurrentCulture.Name=="en-AU") {//Australia
								Provider defaultProv=Providers.GetProv(PrefC.GetLong(PrefName.PracticeDefaultProv));
								field.FieldValue+="ABN: "+defaultProv.NationalProvID+"\r\n";
							}
							if(CultureInfo.CurrentCulture.Name=="en-NZ") {//New Zealand
								Provider defaultProv=Providers.GetProv(PrefC.GetLong(PrefName.PracticeDefaultProv));
								field.FieldValue+="GST: "+defaultProv.SSN+"\r\n";
							}
							field.FieldValue+=clinic.Address+"\r\n";
							if(clinic.Address2!="") {
								field.FieldValue+=clinic.Address2+"\r\n";
							}
							if(CultureInfo.CurrentCulture.Name.EndsWith("CH")) {//CH is for switzerland. eg de-CH
								field.FieldValue+=clinic.Zip+" "+clinic.City+"\r\n";
							}
							else if(CultureInfo.CurrentCulture.Name.EndsWith("SG")) {//SG=Singapore
								field.FieldValue+=clinic.City+" "+clinic.Zip+"\r\n";
							}
							else {
								field.FieldValue+=clinic.City+", "+clinic.State+" "+clinic.Zip+"\r\n";
							}
							if(clinic.Phone.Length==10) {
								field.FieldValue+="("+clinic.Phone.Substring(0,3)+")"+clinic.Phone.Substring(3,3)+"-"+clinic.Phone.Substring(6)+"\r\n";
							}
							else {
								field.FieldValue+=clinic.Phone+"\r\n";
							}
						}
						else {//no clinics
							field.FieldValue=PrefC.GetString(PrefName.PracticeTitle)+"\r\n";
							if(CultureInfo.CurrentCulture.Name=="en-AU") {//Australia
								Provider defaultProv=Providers.GetProv(PrefC.GetLong(PrefName.PracticeDefaultProv));
								field.FieldValue+="ABN: "+defaultProv.NationalProvID+"\r\n";
							}
							if(CultureInfo.CurrentCulture.Name=="en-NZ") {//New Zealand
								Provider defaultProv=Providers.GetProv(PrefC.GetLong(PrefName.PracticeDefaultProv));
								field.FieldValue+="GST: "+defaultProv.SSN+"\r\n";
							}
							field.FieldValue+=PrefC.GetString(PrefName.PracticeAddress)+"\r\n";
							if(PrefC.GetString(PrefName.PracticeAddress2)!="") {
								field.FieldValue+=PrefC.GetString(PrefName.PracticeAddress2)+"\r\n";
							}
							if(CultureInfo.CurrentCulture.Name.EndsWith("CH")) {//CH is for switzerland. eg de-CH
								field.FieldValue+=PrefC.GetString(PrefName.PracticeZip)+" "+PrefC.GetString(PrefName.PracticeCity)+"\r\n";
							}
							else if(CultureInfo.CurrentCulture.Name.EndsWith("SG")) {//SG=Singapore
								field.FieldValue+=PrefC.GetString(PrefName.PracticeCity)+" "+PrefC.GetString(PrefName.PracticeZip)+"\r\n";
							}
							else {
								field.FieldValue+=PrefC.GetString(PrefName.PracticeCity)+", "+PrefC.GetString(PrefName.PracticeST)+" "+PrefC.GetString(PrefName.PracticeZip)+"\r\n";
							}
							if(PrefC.GetString(PrefName.PracticePhone).Length==10) {
								field.FieldValue+="("+PrefC.GetString(PrefName.PracticePhone).Substring(0,3)+")"+PrefC.GetString(PrefName.PracticePhone).Substring(3,3)+"-"+PrefC.GetString(PrefName.PracticePhone).Substring(6)+"\r\n";
							}
							else {
								field.FieldValue+=PrefC.GetString(PrefName.PracticePhone)+"\r\n";
							}
						}
						#endregion
						#endregion
						break;
					case "billingAddress":
						#region BillingAddress
						if(Stmt.SinglePatient){
							field.FieldValue=pat.GetNameFLnoPref()+"\r\n";
						}
						else{
							field.FieldValue=PatGuar.GetNameFLFormal()+"\r\n";
						}
						field.FieldValue+=PatGuar.Address+"\r\n";
						if(PatGuar.Address2!="") {
							field.FieldValue+=PatGuar.Address2+"\r\n";
						}
						if(CultureInfo.CurrentCulture.Name.EndsWith("CH")) {//CH is for switzerland. eg de-CH
							field.FieldValue+=(PatGuar.Zip+" "+PatGuar.City).Trim();//no line break
						}
						else if(CultureInfo.CurrentCulture.Name.EndsWith("SG")) {//SG=Singapore
							field.FieldValue+=(PatGuar.City+" "+PatGuar.Zip).Trim();//no line break
						}
						else {
							field.FieldValue+=((PatGuar.City+", "+PatGuar.State).Trim(new[] { ',',' ' })+" "+PatGuar.Zip).Trim();//no line break
						}
						if(!string.IsNullOrWhiteSpace(PatGuar.Country)) {
							if(CultureInfo.CurrentCulture.Name.EndsWith("CH")||CultureInfo.CurrentCulture.Name.EndsWith("SG")) {//if Singapore or Switzerland
								if(!string.IsNullOrWhiteSpace(PatGuar.City+PatGuar.Zip)) {//and either city or zip are not blank, add line break
									field.FieldValue+="\r\n";
								}
							}
							else {//all other cultures
								if(!string.IsNullOrWhiteSpace(PatGuar.City+PatGuar.State+PatGuar.Zip)) {//any field, city, state or zip contain data, add line break
									field.FieldValue+="\r\n";
								}
							}
							field.FieldValue+=PatGuar.Country;
						}
						#endregion
						break;
					case "practiceTitle":
						field.FieldValue=PrefC.GetString(PrefName.PracticeTitle);
						break;
					case "statementIsCopy":
						field.FieldValue=(Stmt.IsInvoiceCopy?Lan.g("Statements","COPY"):"");
						break;
					case "statementIsTaxReceipt":
						//if(!CultureInfo.CurrentCulture.Name.EndsWith("CA")) { field.FieldValue=""; break; }
						field.FieldValue=(Stmt.IsReceipt?Lan.g("Statements","KEEP THIS RECEIPT FOR INCOME TAX PURPOSES"):"");
						break;
					case "practiceAddress":
						field.FieldValue=PrefC.GetString(PrefName.PracticeAddress);
						if(PrefC.GetString(PrefName.PracticeAddress2) != "") {
							field.FieldValue+="\r\n"+PrefC.GetString(PrefName.PracticeAddress2);
						}
						break;
					case "practiceCityStateZip":
						field.FieldValue=PrefC.GetString(PrefName.PracticeCity)+", "
							+PrefC.GetString(PrefName.PracticeST)+"  "
							+PrefC.GetString(PrefName.PracticeZip);
						break;
					case "statement.DateSent":
						field.FieldValue=Stmt.DateSent.ToShortDateString();
						break;
					case "patient.salutation":
						field.FieldValue="Dear "+pat.GetSalutation()+":";
						break;
					case "patient.priProvNameFL":
						field.FieldValue=Providers.GetFormalName(pat.PriProv);
						break;
					case "ProviderLegendAUS":
						#region ProviderLegendAUS
						if(CultureInfo.CurrentCulture.Name!="en-AU") {//English (Australia)
							field.FieldValue="";
							break;
						}
						Providers.RefreshCache();
						field.FieldValue="PROVIDERS:"+"\r\n";
						for(int i=0;i<ProviderC.ListShort.Count;i++) {//All non-hidden providers are added to the legend.
							Provider prov=ProviderC.ListShort[i];
							string suffix="";
							if(prov.Suffix.Trim()!=""){
								suffix=", "+prov.Suffix.Trim();
							}
							field.FieldValue+=prov.Abbr+" - "+prov.FName+" "+prov.LName+suffix+" - "+prov.MedicaidID+"\r\n";
						}
						#endregion
						break;
				}
			}
		}

		private static void FillFieldsForMedLabResults(Sheet sheet,MedLab medLab,Patient pat) {
			//pat might be null and sheet.PatNum might be invalid, that is ok.
			List<MedLab> listMedLabs=MedLabs.GetForPatAndSpecimen(pat.PatNum,medLab.SpecimenID,medLab.SpecimenIDFiller);//should always be at least one MedLab
			List<MedLabResult> listResults;
			List<MedLabFacility> listFacilities=MedLabFacilities.GetFacilityList(listMedLabs,out listResults);
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "medlab.ClinicalInfo":
						if(listMedLabs.Count==0) {
							break;
						}
						field.FieldValue=listMedLabs[0].ClinicalInfo;
						for(int i=0;i<listMedLabs.Count;i++) {
							if(Regex.IsMatch(field.FieldValue,Regex.Escape(listMedLabs[i].ClinicalInfo),RegexOptions.IgnoreCase)) {
								continue;
							}
							if(field.FieldValue!="") {
								field.FieldValue+="\r\n";
							}
							field.FieldValue+=listMedLabs[i].ClinicalInfo;
						}
						break;
					case "medlab.dateEntered":
						if(listMedLabs.Count==0) {
							break;
						}
						DateTime minDateEntered=DateTime.MaxValue;
						for(int i=0;i<listMedLabs.Count;i++) {
							if(listMedLabs[i].DateTimeEntered>DateTime.MinValue && listMedLabs[i].DateTimeEntered<minDateEntered) {
								minDateEntered=listMedLabs[i].DateTimeEntered;
							}
						}
						if(minDateEntered==DateTime.MaxValue) {
							field.FieldValue="";
							break;
						}
						field.FieldValue=minDateEntered.ToShortDateString();
						break;
					case "medlab.DateTimeCollected":
						if(listMedLabs.Count==0) {
							break;
						}
						DateTime minDateCollected=DateTime.MaxValue;
						for(int i=0;i<listMedLabs.Count;i++) {
							if(listMedLabs[i].DateTimeCollected>DateTime.MinValue && listMedLabs[i].DateTimeCollected<minDateCollected) {
								minDateCollected=listMedLabs[i].DateTimeCollected;
							}
						}
						if(minDateCollected==DateTime.MaxValue) {
							break;
						}
						field.FieldValue=minDateCollected.ToString("MM/dd/yyyy hh:mm tt");
						break;
					case "medlab.DateTimeReported":
						if(listMedLabs.Count==0) {
							break;
						}
						DateTime maxDateReported=DateTime.MinValue;
						for(int i=0;i<listMedLabs.Count;i++) {
							if(listMedLabs[i].DateTimeReported>maxDateReported) {
								maxDateReported=listMedLabs[i].DateTimeReported;
							}
						}
						if(maxDateReported==DateTime.MinValue) {
							break;
						}
						field.FieldValue=maxDateReported.ToString("MM/dd/yyyy hh:mm tt");
						break;
					case "medlab.NoteLab":
						if(listMedLabs.Count==0) {
							break;
						}
						string patNote="";
						string labNote="";
						for(int i=0;i<listMedLabs.Count;i++) {
							if(!Regex.IsMatch(patNote,Regex.Escape(listMedLabs[i].NotePat),RegexOptions.IgnoreCase)) {
								if(patNote!="") {
									patNote+="\r\n";
								}
								patNote+=listMedLabs[i].NotePat;
							}
							if(!Regex.IsMatch(labNote,Regex.Escape(listMedLabs[i].NoteLab),RegexOptions.IgnoreCase)) {
								if(labNote!="") {
									labNote+="\r\n";
								}
								labNote+=listMedLabs[i].NoteLab;
							}
						}
						field.FieldValue=patNote;
						if(field.FieldValue!="" && labNote!="") {
							field.FieldValue+="\r\n";
						}
						field.FieldValue+=labNote;
						break;
					case "medlab.obsTests":
						List<string> listTestIds=new List<string>();
						for(int i=0;i<listMedLabs.Count;i++) {
							if(listTestIds.Contains(listMedLabs[i].ObsTestID)) {
								continue;
							}
							if(listMedLabs[i].ActionCode==ResultAction.G) {//"G" indicates a reflex test, not a test originally ordered, so skip these
								continue;
							}
							listTestIds.Add(listMedLabs[i].ObsTestID);
							if(field.FieldValue!="") {
								field.FieldValue+="\r\n";
							}
							field.FieldValue+=listMedLabs[i].ObsTestID+" - "+listMedLabs[i].ObsTestDescript+"  (Reported: "
								+listMedLabs[i].DateTimeReported.ToString("MM/dd/yyyy hh:mm tt")+")";
						}
						break;
					case "medlab.ProvID":
						for(int i=0;i<listMedLabs.Count;i++) {
							if(listMedLabs[i].OrderingProvLocalID=="") {
								continue;
							}
							field.FieldValue=listMedLabs[i].OrderingProvLocalID;
							break;
						}
						break;
					case "medlab.provNameLF":
						if(listMedLabs.Count==0) {
							break;
						}
						string provLName="";
						string provFName="";
						for(int i=0;i<listMedLabs.Count;i++) {
							if(listMedLabs[i].OrderingProvFName=="" && listMedLabs[i].OrderingProvLName=="") {//both names missing, continue
								continue;
							}
							provFName=listMedLabs[i].OrderingProvFName;
							provLName=listMedLabs[i].OrderingProvLName;
							if(provFName!="" && provLName!="") {//if both first and last name are included, use the values from this medlab
								break;
							}
						}
						if(provLName!="") {
							field.FieldValue=provLName;
						}
						if(provFName!="") {
							if(field.FieldValue!="") {
								field.FieldValue+=", ";
							}
							field.FieldValue+=provFName;
						}
						break;
					case "medlab.ProvNPI":
						for(int i=0;i<listMedLabs.Count;i++) {
							if(listMedLabs[i].OrderingProvNPI=="") {
								continue;
							}
							field.FieldValue=listMedLabs[i].OrderingProvNPI;
							break;
						}
						break;
					case "medlab.PatAccountNum":
						for(int i=0;i<listMedLabs.Count;i++) {
							if(listMedLabs[i].PatAccountNum=="") {
								continue;
							}
							field.FieldValue=listMedLabs[i].PatAccountNum;
							break;
						}
						break;
					case "medlab.PatAge":
						for(int i=0;i<listMedLabs.Count;i++) {
							if(listMedLabs[i].PatAge=="") {
								continue;
							}
							field.FieldValue=listMedLabs[i].PatAge;
							break;
						}
						break;
					case "medlab.PatFasting":
						for(int i=0;i<listMedLabs.Count;i++) {
							if(listMedLabs[i].PatFasting==YN.Unknown) {
								continue;
							}
							field.FieldValue=listMedLabs[i].PatFasting.ToString();
							break;
						}
						break;
					case "medlab.PatIDAlt":
						for(int i=0;i<listMedLabs.Count;i++) {
							if(listMedLabs[i].PatIDAlt=="") {
								continue;
							}
							field.FieldValue=listMedLabs[i].PatIDAlt;
							break;
						}
						break;
					case "medlab.PatIDLab":
						for(int i=0;i<listMedLabs.Count;i++) {
							if(listMedLabs[i].PatIDLab=="") {
								continue;
							}
							field.FieldValue=listMedLabs[i].PatIDLab;
							break;
						}
						break;
					case "medlab.SpecimenID":
						for(int i=0;i<listMedLabs.Count;i++) {
							if(listMedLabs[i].SpecimenID=="") {
								continue;
							}
							field.FieldValue=listMedLabs[i].SpecimenID;
							break;
						}
						break;
					case "medlab.SpecimenIDAlt":
						for(int i=0;i<listMedLabs.Count;i++) {
							if(listMedLabs[i].SpecimenIDAlt=="") {
								continue;
							}
							field.FieldValue=listMedLabs[i].SpecimenIDAlt;
							break;
						}
						break;
					case "medlab.TotalVolume":
						for(int i=0;i<listMedLabs.Count;i++) {
							if(listMedLabs[i].TotalVolume=="") {
								continue;
							}
							field.FieldValue=listMedLabs[i].TotalVolume;
							break;
						}
						break;
					case "medLabFacilityAddr":
						if(listMedLabs.Count==0) {
							break;
						}
						string fieldValAddr="";
						MedLabFacility facilityCur;
						for(int i=0;i<listFacilities.Count;i++) {
							if(fieldValAddr!="") {
								fieldValAddr+="\r\n\r\n";
							}
							fieldValAddr+=(i+1).ToString().PadLeft(2,'0').PadRight(14,' ');
							string spaces=new string(' ',16);
							facilityCur=listFacilities[i];
							fieldValAddr+=facilityCur.FacilityName+"\r\n"+spaces+facilityCur.Address+"\r\n"+spaces+facilityCur.City+", "+facilityCur.State+"  ";
							if(facilityCur.Zip.Length==9) {
								fieldValAddr+=facilityCur.Zip.Substring(0,5)+"-"+facilityCur.Zip.Substring(5,4);
							}
							else {
								fieldValAddr+=facilityCur.Zip;
							}
						}
						field.FieldValue=fieldValAddr;
						break;
					case "medLabFacilityDir":
						if(listMedLabs.Count==0) {
							break;
						}
						string fieldValDir="";
						MedLabFacility facCur;
						for(int i=0;i<listFacilities.Count;i++) {
							if(fieldValDir!="") {
								fieldValDir+="\r\n\r\n";
							}
							facCur=listFacilities[i];
							fieldValDir+="Dir:  "+facCur.DirectorFName+" "+facCur.DirectorLName+", "+facCur.DirectorTitle+"\r\nPh:  ";
							if(facCur.Phone.Length==10) {
								fieldValDir+=facCur.Phone.Substring(0,3)+"-"+facCur.Phone.Substring(3,3)+"-"+facCur.Phone.Substring(6);
							}
							else {
								fieldValDir+=facCur.Phone;
							}
							fieldValDir+="\r\n";//blank line to keep it in line with the address
						}
						field.FieldValue=fieldValDir;
						break;
					case "patient.addrCityStZip":
						if(pat==null) {
							continue;
						}
						field.FieldValue="";
						if(pat.Address!="") {
							field.FieldValue+=pat.Address+"\r\n";
						}
						if(pat.Address2!="") {
							field.FieldValue+=pat.Address2+"\r\n";
						}
						if(pat.City!="") {
							field.FieldValue+=pat.City;
							if(pat.State!="" || pat.Zip!="") {
								field.FieldValue+=", ";
							}
						}
						if(pat.State!="") {
							field.FieldValue+=pat.State;
							if(pat.Zip!="") {
								field.FieldValue+=" ";
							}
						}
						field.FieldValue+=pat.Zip;
						break;
					case "patient.Birthdate":
						if(pat==null) {
							continue;
						}
						if(pat.Birthdate.Year<1880) {
							field.FieldValue="";
							break;
						}
						field.FieldValue=pat.Birthdate.ToShortDateString();
						break;
					case "patient.FName":
						if(pat==null) {
							continue;
						}
						field.FieldValue=pat.FName;
						break;
					case "patient.Gender":
						if(pat==null) {
							continue;
						}
						field.FieldValue=Lan.g("enumPatientGender",pat.Gender.ToString());
						break;
					case "patient.HmPhone":
						if(pat==null) {
							continue;
						}
						if(pat.HmPhone.Length==10) {
							field.FieldValue=pat.HmPhone.Substring(0,3)+"-"+pat.HmPhone.Substring(3,3)+"-"+pat.HmPhone.Substring(6);
							break;
						}
						field.FieldValue=pat.HmPhone;
						break;
					case "patient.MiddleI":
						if(pat==null) {
							continue;
						}
						field.FieldValue=pat.MiddleI;
						break;
					case "patient.LName":
						if(pat==null) {
							continue;
						}
						field.FieldValue=pat.LName;
						break;
					case "patient.PatNum":
						if(pat==null) {
							continue;
						}
						field.FieldValue=pat.PatNum.ToString();
						break;
					case "patient.SSN":
						if(pat==null) {
							continue;
						}
						field.FieldValue="***-**-";
						if(pat.SSN.Length>3) {
							field.FieldValue+=pat.SSN.Substring(pat.SSN.Length-4,4);
						}
						break;
					case "practiceAddrCityStZip":
						field.FieldValue="";
						if(PrefC.GetString(PrefName.PracticeAddress)!="") {
							field.FieldValue+=PrefC.GetString(PrefName.PracticeAddress)+"\r\n";
						}
						if(PrefC.GetString(PrefName.PracticeAddress2)!="") {
							field.FieldValue+=PrefC.GetString(PrefName.PracticeAddress2)+"\r\n";
						}
						if(PrefC.GetString(PrefName.PracticeCity)!="") {
							field.FieldValue+=PrefC.GetString(PrefName.PracticeCity);
							if(PrefC.GetString(PrefName.PracticeST)!="" || PrefC.GetString(PrefName.PracticeZip)!="") {
								field.FieldValue+=", ";
							}
						}
						if(PrefC.GetString(PrefName.PracticeST)!="") {
							field.FieldValue+=PrefC.GetString(PrefName.PracticeST);
							if(PrefC.GetString(PrefName.PracticeZip)!="") {
								field.FieldValue+=" ";
							}
						}
						field.FieldValue+=PrefC.GetString(PrefName.PracticeZip);
						break;
					case "PracticePh":
						if(PrefC.GetString(PrefName.PracticePhone).Length==10) {
							field.FieldValue=PrefC.GetString(PrefName.PracticePhone).Substring(0,3)+"-"+PrefC.GetString(PrefName.PracticePhone).Substring(3,3)+
								"-"+PrefC.GetString(PrefName.PracticePhone).Substring(6);
							break;
						}
						field.FieldValue=PrefC.GetString(PrefName.PracticePhone);
						break;
					case "PracticeTitle":
						field.FieldValue=PrefC.GetString(PrefName.PracticeTitle);
						break;
				}
			}
		}

		private static void FillFieldsForTreatPlan(Sheet sheet,Patient pat) {
			TreatPlan treatPlan=(TreatPlan)SheetParameter.GetParamByName(sheet.Parameters,"TreatPlan").ParamValue;
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "Heading":
						field.FieldValue=treatPlan.Heading;
						break;
					case "defaultHeading":
						string value="";
						Clinic clinic;
						if(pat.ClinicNum==0) {
							clinic=Clinics.GetPracticeAsClinicZero();
						}
						else {
							clinic=Clinics.GetClinic(pat.ClinicNum);
						}
						value=clinic.Description;
						if(clinic.Phone.Length==10 && CultureInfo.CurrentCulture.Name=="en-US") {
							value+="\r\n"+"("+clinic.Phone.Substring(0,3)+")"+clinic.Phone.Substring(3,3)+"-"+clinic.Phone.Substring(6);
						}
						else {
							value+="\r\n"+clinic.Phone;
						}
						value+="\r\n"+pat.GetNameFLFormal()+", DOB "+pat.Birthdate.ToShortDateString();
						if(treatPlan.ResponsParty!=0) {
							value+="\r\n"+Lan.g("ContrTreat","Responsible Party")+": "+Patients.GetLim(treatPlan.ResponsParty).GetNameFL();
						}
						if(treatPlan.TPStatus==TreatPlanStatus.Saved) {
							value+="\r\n"+treatPlan.DateTP.ToShortDateString();
						}
						else {//active or inactive TP
							value+="\r\n"+DateTime.Today.ToShortDateString();
						}
						field.FieldValue=value;
						break;
					case "Note":
						field.FieldValue=treatPlan.Note;
						break;
				}
			}
		}

		private static void FillFieldsForScreening(Sheet sheet,ScreenGroup screenGroup,Patient pat,Provider prov) {
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "Description":
						field.FieldValue=screenGroup.Description;
						break;
					case "DateScreenGroup":
						field.FieldValue=screenGroup.SGDate.ToShortDateString();
						break;
					case "ProvName":
						if(prov!=null) {
							field.FieldValue=prov.LName+", "+prov.FName;
						}
						else { 
							field.FieldValue=screenGroup.ProvName;
						}
						break;
					case "PlaceOfService":
						field.FieldValue=screenGroup.PlaceService.ToString()+field.FieldValue;
						break;
					case "County":
						field.FieldValue=screenGroup.County;
						break;
					case "GradeSchool":
						field.FieldValue=screenGroup.GradeSchool;
						break;
				}
				//Patient specific fields--------------------------------------------------
				if(pat!=null) {
					switch(field.FieldName) {
						case "Birthdate":
							field.FieldValue=pat.Birthdate.ToShortDateString();
							break;
						case "Age":
							field.FieldValue=pat.Age.ToString();
							break;
						case "FName":
							field.FieldValue=pat.FName;
							break;
						case "LName":
							field.FieldValue=pat.LName;
							break;
						case "MiddleI":
							field.FieldValue=pat.MiddleI;
							break;
						case "Preferred":
							field.FieldValue=pat.Preferred;
							break;
						case "Gender":
							field.FieldValue=pat.Gender.ToString()+field.FieldValue;
							break;
						case "GradeLevel":
							field.FieldValue=pat.GradeLevel.ToString()+field.FieldValue;
							break;
						case "Race/Ethnicity":
							//The patient object no longer has the same type of race so default to Unknown.
							field.FieldValue=PatientRaceOld.Unknown.ToString()+field.FieldValue;
							break;
						case "Urgency":
							field.FieldValue=pat.Urgency.ToString()+field.FieldValue;
							break;
					}
				}
			}
		}

		///<summary>Returns three label strings: Total, Insurance, Balance. These labels change based on various settings.</summary>
		private static string[] totInsBalLabsHelper(Sheet sheet,Statement Stmt) {
			string sLine1="";//Total
			string sLine2="";//InsExt
			string sLine3="";//Balance
			if(Stmt.IsInvoice) {//invoices can't be superstatements
				sLine1=Lan.g("Statements","Procedures:");
				sLine2=Lan.g("Statements","Adjustments:");
				sLine3=Lan.g("Statements","Total:");
			}
			else if(PrefC.GetBool(PrefName.BalancesDontSubtractIns)) {
				if(Stmt.SuperFamily!=0) {
					sLine1=Lan.g("Statements","Sum of Balances:");
				}
				else {
					sLine1=Lan.g("Statements","Balance:");
				}
				//sLine2=Lan.g("Statements","Ins Pending:");
				//sLine3=Lan.g("Statements","After Ins:");
			}
			else {//this is more common
				if(PrefC.GetBool(PrefName.FuchsOptionsOn)) {
					sLine1=Lan.g("Statements","Balance:");
					sLine2=Lan.g("Statements","-Ins Estimate:");
					sLine3=Lan.g("Statements","=Owed Now:");
				}
				else {
					if(Stmt.SuperFamily!=0) {
						sLine1=Lan.g("Statements","Sum of Totals:");
						sLine2=Lan.g("Statements","-Sum of Ins Estimates:");
						sLine3=Lan.g("Statements","=Sum of Balances:");
					}
					else {
						sLine1=Lan.g("Statements","Total:");
						sLine2=Lan.g("Statements","-Ins Estimate:");
						sLine3=Lan.g("Statements","=Balance:");
					}
				}
			}
			return new string[] { sLine1,sLine2,sLine3 };
		}

		///<summary>Returns three value strings: Total, Insurance, Balance. These values change based on various settings.</summary>
		private static string[] totInsBalValsHelper(Sheet sheet,Statement Stmt,DataSet stmtData) {
			string sLine1="";//Total
			string sLine2="";//InsExt
			string sLine3="";//Balance
			DataTable tableAcct;
			DataSet dataSet=stmtData;
			DataTable tableMisc=dataSet.Tables["misc"];
			if(tableMisc==null) {
				tableMisc=new DataTable();
			}
			Patient pat=null;
			if(Stmt.SuperFamily!=0) {//Superfamily statement
				pat=Patients.GetPat(Stmt.SuperFamily);
			}
			else {
				pat=Patients.GetPat(Stmt.PatNum);
			}
			Patient PatGuar=Patients.GetPat(pat.Guarantor);
			if(Stmt.IsInvoice) {
				double adjAmt=0;
				double procAmt=0;
				string tableName;
				for(int i=0;i<dataSet.Tables.Count;i++) {
					tableAcct=dataSet.Tables[i];
					tableName=tableAcct.TableName;
					if(!tableName.StartsWith("account")) {
						continue;
					}
					for(int p=0;p<tableAcct.Rows.Count;p++) {
						if(tableAcct.Rows[p]["AdjNum"].ToString()!="0") {
							adjAmt-=PIn.Double(tableAcct.Rows[p]["creditsDouble"].ToString());
							adjAmt+=PIn.Double(tableAcct.Rows[p]["chargesDouble"].ToString());
						}
						else {//must be a procedure
							procAmt+=PIn.Double(tableAcct.Rows[p]["chargesDouble"].ToString());
						}
					}
				}
				sLine1+=procAmt.ToString("c");
				sLine2+=adjAmt.ToString("c");
				sLine3+=(procAmt+adjAmt).ToString("c");
			}
			else if(Stmt.StatementType==StmtType.LimitedStatement) {
				double statementTotal=dataSet.Tables.OfType<DataTable>().Where(x => x.TableName.StartsWith("account"))
					.SelectMany(x => x.Rows.OfType<DataRow>())
					.Where(x => x["AdjNum"].ToString()!="0"//adjustments, may be charges or credits
						|| x["ProcNum"].ToString()!="0"//procs, will be charges with credits==0
						|| x["PayNum"].ToString()!="0"//patient payments, will be credits with charges==0
						|| x["ClaimPaymentNum"].ToString()!="0").ToList()//claimproc payments+writeoffs, will be credits with charges==0
					.Sum(x => PIn.Double(x["chargesDouble"].ToString())-PIn.Double(x["creditsDouble"].ToString()));//add charges-credits
				if(PrefC.GetBool(PrefName.BalancesDontSubtractIns)) {
					sLine1+=statementTotal.ToString("c");
				}
				else {
					double patInsEst=PIn.Double(tableMisc.Rows.OfType<DataRow>()
						.Where(x => x["descript"].ToString()=="patInsEst")
						.Select(x => x["value"].ToString()).FirstOrDefault());//safe, if string is blank or null PIn.Double will return 0
					sLine1+=statementTotal.ToString("c");
					sLine2+=patInsEst.ToString("c");
					sLine3+=(statementTotal-patInsEst).ToString("c");
				}
			}
			else if(PrefC.GetBool(PrefName.BalancesDontSubtractIns)) {
				if(Stmt.SinglePatient) {
					sLine1+=pat.EstBalance.ToString("c");
				}
				else {
					if(Stmt.SuperFamily!=0) {//Superfam statement
						//If the family is included in superfamily billing, sum their totals into the running total.
						//don't include families with negative balances in the total balance for super family (per Nathan 5/25/2016)
						double balTot=Patients.GetSuperFamilyGuarantors(Stmt.SuperFamily).FindAll(x => x.HasSuperBilling && x.BalTotal>0).Sum(x => x.BalTotal);
						sLine1+=balTot.ToString("c");
					}
					else {
						//Show the current family's balance without subtracting insurance estimates.
						sLine1+=PatGuar.BalTotal.ToString("c");
					}
				}
			}
			else {//more common
				if(Stmt.SinglePatient) {
					double patInsEst=0;
					for(int m=0;m<tableMisc.Rows.Count;m++) {
						if(tableMisc.Rows[m]["descript"].ToString()=="patInsEst") {
							patInsEst=PIn.Double(tableMisc.Rows[m]["value"].ToString());
						}
					}
					double patBal=pat.EstBalance-patInsEst;
					sLine1+=pat.EstBalance.ToString("c");
					sLine2+=patInsEst.ToString("c");
					sLine3+=patBal.ToString("c");
				}
				else {
					if(Stmt.SuperFamily!=0) {//Superfam statement
						List<Patient> listFamilyGuarantors=Patients.GetSuperFamilyGuarantors(Stmt.SuperFamily).FindAll(x => x.HasSuperBilling && (x.BalTotal-x.InsEst)>0);
						double balTot=listFamilyGuarantors.Sum(x => x.BalTotal);
						double insEst=listFamilyGuarantors.Sum(x => x.InsEst);
						sLine1+=balTot.ToString("c");
						sLine2+=insEst.ToString("c");
						sLine3+=(balTot-insEst).ToString("c");
					}
					else {
						sLine1+=PatGuar.BalTotal.ToString("c");
						sLine2+=PatGuar.InsEst.ToString("c");
						sLine3+=(PatGuar.BalTotal - PatGuar.InsEst).ToString("c");
					}
				}
			}
			return new string[] { sLine1,sLine2,sLine3 };
		}

		private static int CompareSheetFieldNames(SheetField input1,SheetField input2) {
			if(Convert.ToInt32(input1.FieldName.Remove(0,8)) < Convert.ToInt32(input2.FieldName.Remove(0,8))) {
				return -1;
			}
			if(Convert.ToInt32(input1.FieldName.Remove(0,8)) > Convert.ToInt32(input2.FieldName.Remove(0,8))) {
				return 1;
			}
			return 0;
		}

		private static void FillFieldsForPaymentPlan(Sheet sheet,Patient pat) {
			PayPlan payPlan=(PayPlan)SheetParameter.GetParamByName(sheet.Parameters,"payplan").ParamValue;
			SheetParameter principal=GetParamByName(sheet,"Principal");
			SheetParameter totFinCharge=GetParamByName(sheet,"totalFinanceCharge");
			SheetParameter totCostLoan=GetParamByName(sheet,"totalCostOfLoan");
			Patient PatGuar=Patients.GetPat(pat.Guarantor);
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "PracticeTitle":
						field.FieldValue=PrefC.GetString(PrefName.PracticeTitle);
						break;
					case "dateToday":
						field.FieldValue=DateTime.Today.ToShortDateString();
						break;
					case "nameLF":
						field.FieldValue=pat.GetNameLF();
						break;
					case "guarantor":
						field.FieldValue=PatGuar.GetNameLFnoPref();
						break;
					case "Principal":
						field.FieldValue=principal.ParamValue.ToString();
						break;
					case "DateOfAgreement":
						field.FieldValue=payPlan.PayPlanDate.ToShortDateString();
						break;
					case "APR":
						field.FieldValue=payPlan.APR.ToString("f1");
						break;
					case "totalFinanceCharge":
						field.FieldValue=((double)totFinCharge.ParamValue).ToString("n");
						break;
					case "totalCostOfLoan":
						field.FieldValue=totCostLoan.ParamValue.ToString();
						break;
					case "Note":
						field.FieldValue=payPlan.Note;
						break;
				}
			}

		}

		private static void FillFieldsForRxMulti(Sheet sheet,Patient pat) {
			List<RxPat> listMultiRx=(List<RxPat>)SheetParameter.GetParamByName(sheet.Parameters,"ListRxNums").ParamValue;
			//listMultiRxSheet contains a list of ints that correspond to the rxmulti "defs" (1-6) that are on this sheet (i.e Disp5/Drug5/RxDate5 would have a 5 in the list). 
			List<int> listMultiRxSheet=(List<int>)SheetParameter.GetParamByName(sheet.Parameters,"ListRxSheet").ParamValue;
			Dictionary<int,RxPat> dictRxs=new Dictionary<int, RxPat>();
			Dictionary<int,Provider> dictProvs=new Dictionary<int, Provider>();
			for(int i=0;i<listMultiRx.Count;i++) {
				dictRxs.Add(listMultiRxSheet[i],listMultiRx[i].Copy());
				dictProvs.Add(listMultiRxSheet[i],Providers.GetProv(listMultiRx[i].ProvNum));
			}
			Clinic clinic=null;
			Provider prov=null;
			if(pat.ClinicNum!=0) {
				clinic=Clinics.GetClinic(pat.ClinicNum);
			}
			string text;
			foreach(SheetField field in sheet.SheetFields) {
				switch(field.FieldName) {
					case "prov.nameFL":
						if(dictRxs.ContainsKey(1)) {
							field.FieldValue=dictProvs[1].GetFormalName();
						}
						break;
					case "prov.nameFL2":
						if(dictRxs.ContainsKey(2)) {
							field.FieldValue=dictProvs[2].GetFormalName();
						}
						break;
					case "prov.nameFL3":
						if(dictRxs.ContainsKey(3)) {
							field.FieldValue=dictProvs[3].GetFormalName();
						}
						break;
					case "prov.nameFL4":
						if(dictRxs.ContainsKey(4)) {
							field.FieldValue=dictProvs[4].GetFormalName();
						}							
						break;
					case "prov.nameFL5":
						if(dictRxs.ContainsKey(5)) {
							field.FieldValue=dictProvs[5].GetFormalName();
						}
						break;
					case "prov.nameFL6":
						if(dictRxs.ContainsKey(6)) {
							field.FieldValue=dictProvs[6].GetFormalName();
						}
						break;
					case "prov.address":
						if(dictRxs.ContainsKey(1)) {
							if(clinic==null) {
								field.FieldValue=PrefC.GetString(PrefName.PracticeAddress);
								if(PrefC.GetString(PrefName.PracticeAddress2)!="") {
									field.FieldValue+="\r\n"+PrefC.GetString(PrefName.PracticeAddress2);
								}
							}
							else {
								field.FieldValue=clinic.Address;
								if(clinic.Address2!="") {
									field.FieldValue+="\r\n"+clinic.Address2;
								}
							}
						}
						break;
					case "prov.address2":
						if(dictRxs.ContainsKey(2)) {
							if(clinic==null) {
								field.FieldValue=PrefC.GetString(PrefName.PracticeAddress);
								if(PrefC.GetString(PrefName.PracticeAddress2)!="") {
									field.FieldValue+="\r\n"+PrefC.GetString(PrefName.PracticeAddress2);
								}
							}
							else {
								field.FieldValue=clinic.Address;
								if(clinic.Address2!="") {
									field.FieldValue+="\r\n"+clinic.Address2;
								}
							}
						}
						break;
					case "prov.address3":
						if(dictRxs.ContainsKey(3)) {
							if(clinic==null) {
								field.FieldValue=PrefC.GetString(PrefName.PracticeAddress);
								if(PrefC.GetString(PrefName.PracticeAddress2)!="") {
									field.FieldValue+="\r\n"+PrefC.GetString(PrefName.PracticeAddress2);
								}
							}
							else {
								field.FieldValue=clinic.Address;
								if(clinic.Address2!="") {
									field.FieldValue+="\r\n"+clinic.Address2;
								}
							}
						}
						break;
					case "prov.address4":
						if(dictRxs.ContainsKey(4)) {
							if(clinic==null) {
								field.FieldValue=PrefC.GetString(PrefName.PracticeAddress);
								if(PrefC.GetString(PrefName.PracticeAddress2)!="") {
									field.FieldValue+="\r\n"+PrefC.GetString(PrefName.PracticeAddress2);
								}
							}
							else {
								field.FieldValue=clinic.Address;
								if(clinic.Address2!="") {
									field.FieldValue+="\r\n"+clinic.Address2;
								}
							}
						}
						break;
					case "prov.address5":
						if(dictRxs.ContainsKey(5)) {
							if(clinic==null) {
								field.FieldValue=PrefC.GetString(PrefName.PracticeAddress);
								if(PrefC.GetString(PrefName.PracticeAddress2)!="") {
									field.FieldValue+="\r\n"+PrefC.GetString(PrefName.PracticeAddress2);
								}
							}
							else {
								field.FieldValue=clinic.Address;
								if(clinic.Address2!="") {
									field.FieldValue+="\r\n"+clinic.Address2;
								}
							}
						}
						break;
					case "prov.address6":
						if(dictRxs.ContainsKey(6)) {
							if(clinic==null) {
								field.FieldValue=PrefC.GetString(PrefName.PracticeAddress);
								if(PrefC.GetString(PrefName.PracticeAddress2)!="") {
									field.FieldValue+="\r\n"+PrefC.GetString(PrefName.PracticeAddress2);
								}
							}
							else {
								field.FieldValue=clinic.Address;
								if(clinic.Address2!="") {
									field.FieldValue+="\r\n"+clinic.Address2;
								}
							}
						}
						break;
					case "prov.cityStateZip":
						if(dictRxs.ContainsKey(1)) {
							if(clinic==null) {
								field.FieldValue=PrefC.GetString(PrefName.PracticeCity)+", "+PrefC.GetString(PrefName.PracticeST)+" "+PrefC.GetString(PrefName.PracticeZip);
							}
							else {
								field.FieldValue=clinic.City+", "+clinic.State+" "+clinic.Zip;
							}
						}
						break;
					case "prov.cityStateZip2":
						if(dictRxs.ContainsKey(2)) {
							if(clinic==null) {
								field.FieldValue=PrefC.GetString(PrefName.PracticeCity)+", "+PrefC.GetString(PrefName.PracticeST)+" "+PrefC.GetString(PrefName.PracticeZip);
							}
							else {
								field.FieldValue=clinic.City+", "+clinic.State+" "+clinic.Zip;
							}
						}
						break;
					case "prov.cityStateZip3":
						if(dictRxs.ContainsKey(3)) {
							if(clinic==null) {
								field.FieldValue=PrefC.GetString(PrefName.PracticeCity)+", "+PrefC.GetString(PrefName.PracticeST)+" "+PrefC.GetString(PrefName.PracticeZip);
							}
							else {
								field.FieldValue=clinic.City+", "+clinic.State+" "+clinic.Zip;
							}
						}
						break;
					case "prov.cityStateZip4":
						if(dictRxs.ContainsKey(4)) {
							if(clinic==null) {
								field.FieldValue=PrefC.GetString(PrefName.PracticeCity)+", "+PrefC.GetString(PrefName.PracticeST)+" "+PrefC.GetString(PrefName.PracticeZip);
							}
							else {
								field.FieldValue=clinic.City+", "+clinic.State+" "+clinic.Zip;
							}
						}
						break;
					case "prov.cityStateZip5":
						if(dictRxs.ContainsKey(5)) {
							if(clinic==null) {
								field.FieldValue=PrefC.GetString(PrefName.PracticeCity)+", "+PrefC.GetString(PrefName.PracticeST)+" "+PrefC.GetString(PrefName.PracticeZip);
							}
							else {
								field.FieldValue=clinic.City+", "+clinic.State+" "+clinic.Zip;
							}
						}
						break;
					case "prov.cityStateZip6":
						if(dictRxs.ContainsKey(6)) {
							if(clinic==null) {
								field.FieldValue=PrefC.GetString(PrefName.PracticeCity)+", "+PrefC.GetString(PrefName.PracticeST)+" "+PrefC.GetString(PrefName.PracticeZip);
							}
							else {
								field.FieldValue=clinic.City+", "+clinic.State+" "+clinic.Zip;
							}
						}
						break;
					case "prov.phone":
						if(dictRxs.ContainsKey(1)) {
							if(clinic==null) {
								text=PrefC.GetString(PrefName.PracticePhone);
								text=new string(text.Where(x => char.IsDigit(x)).ToArray());
							}
							else {
								text=clinic.Phone;
							}
							field.FieldValue=text;
							if(text.Length==10) {
								field.FieldValue="("+text.Substring(0,3)+")"+text.Substring(3,3)+"-"+text.Substring(6);
							}
						}
						break;
					case "prov.phone2":
						if(dictRxs.ContainsKey(2)) {
							if(clinic==null) {
								text=PrefC.GetString(PrefName.PracticePhone);
								text=new string(text.Where(x => char.IsDigit(x)).ToArray());
							}
							else {
								text=clinic.Phone;
							}
							field.FieldValue=text;
							if(text.Length==10) {
								field.FieldValue="("+text.Substring(0,3)+")"+text.Substring(3,3)+"-"+text.Substring(6);
							}
						}
						break;
					case "prov.phone3":
						if(dictRxs.ContainsKey(3)) {
							if(clinic==null) {
								text=PrefC.GetString(PrefName.PracticePhone);
								text=new string(text.Where(x => char.IsDigit(x)).ToArray());
							}
							else {
								text=clinic.Phone;
							}
							field.FieldValue=text;
							if(text.Length==10) {
								field.FieldValue="("+text.Substring(0,3)+")"+text.Substring(3,3)+"-"+text.Substring(6);
							}
						}
						break;
					case "prov.phone4":
						if(dictRxs.ContainsKey(4)) {
							if(clinic==null) {
								text=PrefC.GetString(PrefName.PracticePhone);
								text=new string(text.Where(x => char.IsDigit(x)).ToArray());
							}
							else {
								text=clinic.Phone;
							}
							field.FieldValue=text;
							if(text.Length==10) {
								field.FieldValue="("+text.Substring(0,3)+")"+text.Substring(3,3)+"-"+text.Substring(6);
							}
						}
						break;
					case "prov.phone5":
						if(dictRxs.ContainsKey(5)) {
							if(clinic==null) {
								text=PrefC.GetString(PrefName.PracticePhone);
								text=new string(text.Where(x => char.IsDigit(x)).ToArray());
							}
							else {
								text=clinic.Phone;
							}
							field.FieldValue=text;
							if(text.Length==10) {
								field.FieldValue="("+text.Substring(0,3)+")"+text.Substring(3,3)+"-"+text.Substring(6);
							}
						}
						break;
					case "prov.phone6":
						if(dictRxs.ContainsKey(6)) {
							if(clinic==null) {
								text=PrefC.GetString(PrefName.PracticePhone);
								text=new string(text.Where(x => char.IsDigit(x)).ToArray());
							}
							else {
								text=clinic.Phone;
							}
							field.FieldValue=text;
							if(text.Length==10) {
								field.FieldValue="("+text.Substring(0,3)+")"+text.Substring(3,3)+"-"+text.Substring(6);
							}
						}
						break;
					case "RxDate":
						if(dictRxs.ContainsKey(1)) {
							field.FieldValue=dictRxs[1].RxDate.ToShortDateString();
						}
						break;
					case "RxDate2":
						if(dictRxs.ContainsKey(2)) {
							field.FieldValue=dictRxs[2].RxDate.ToShortDateString();
						}
						break;
					case "RxDate3":
						if(dictRxs.ContainsKey(3)) {
							field.FieldValue=dictRxs[3].RxDate.ToShortDateString();
						}
						break;
					case "RxDate4":
						if(dictRxs.ContainsKey(4)) {
							field.FieldValue=dictRxs[4].RxDate.ToShortDateString();
						}
						break;
					case "RxDate5":
						if(dictRxs.ContainsKey(5)) {
							field.FieldValue=dictRxs[5].RxDate.ToShortDateString();
						}
						break;
					case "RxDate6":
						if(dictRxs.ContainsKey(6)) {
							field.FieldValue=dictRxs[6].RxDate.ToShortDateString();
						}
						break;
					case "RxDateMonthSpelled":
						if(dictRxs.ContainsKey(1)) {
							field.FieldValue=dictRxs[1].RxDate.ToString("MMM dd,yyyy");
						}
						break;
					case "RxDateMonthSpelled2":
						if(dictRxs.ContainsKey(2)) {
							field.FieldValue=dictRxs[2].RxDate.ToString("MMM dd,yyyy");
						}
						break;
					case "RxDateMonthSpelled3":
						if(dictRxs.ContainsKey(3)) {
							field.FieldValue=dictRxs[3].RxDate.ToString("MMM dd,yyyy");
						}
						break;
					case "RxDateMonthSpelled4":
						if(dictRxs.ContainsKey(4)) {
							field.FieldValue=dictRxs[4].RxDate.ToString("MMM dd,yyyy");
						}
						break;
					case "RxDateMonthSpelled5":
						if(dictRxs.ContainsKey(5)) {
							field.FieldValue=dictRxs[5].RxDate.ToString("MMM dd,yyyy");
						}
						break;
					case "RxDateMonthSpelled6":
						if(dictRxs.ContainsKey(6)) {
							field.FieldValue=dictRxs[6].RxDate.ToString("MMM dd,yyyy");
						}
						break;
					case "prov.dEANum":
						if(dictRxs.ContainsKey(1)) {
							if(dictRxs[1].IsControlled) {
								field.FieldValue=dictProvs[1].DEANum;
							}
							else {
								field.FieldValue="";
							}
						}
						break;
					case "prov.dEANum2":
						if(dictRxs.ContainsKey(2)) {
							if(dictRxs[2].IsControlled) {
								field.FieldValue=dictProvs[2].DEANum;
							}
							else {
								field.FieldValue="";
							}
						}
						break;
					case "prov.dEANum3":
						if(dictRxs.ContainsKey(3)) {
							if(dictRxs[3].IsControlled) {
								field.FieldValue=dictProvs[3].DEANum;
							}
							else {
								field.FieldValue="";
							}
						}
						break;
					case "prov.dEANum4":
						if(dictRxs.ContainsKey(4)) {
							if(dictRxs[4].IsControlled) {
								field.FieldValue=dictProvs[4].DEANum;
							}
							else {
								field.FieldValue="";
							}
						}
						break;
					case "prov.dEANum5":
						if(dictRxs.ContainsKey(5)) {
							if(dictRxs[5].IsControlled) {
								field.FieldValue=dictProvs[5].DEANum;
							}
							else {
								field.FieldValue="";
							}
						}
						break;
					case "prov.dEANum6":
						if(dictRxs.ContainsKey(6)) {
							if(dictRxs[6].IsControlled) {
								field.FieldValue=dictProvs[6].DEANum;
							}
							else {
								field.FieldValue="";
							}
						}
						break;
					case "pat.nameFL":
						if(dictRxs.ContainsKey(1)) {
							//Can't include preferred, so:
							field.FieldValue=pat.FName+" "+pat.MiddleI+"  "+pat.LName;
						}
						break;
					case "pat.nameFL2":
						if(dictRxs.ContainsKey(2)) {
							field.FieldValue=pat.FName+" "+pat.MiddleI+"  "+pat.LName;
						}
						break;
					case "pat.nameFL3":
						if(dictRxs.ContainsKey(3)) {
							field.FieldValue=pat.FName+" "+pat.MiddleI+"  "+pat.LName;
						}
						break;
					case "pat.nameFL4":
						if(dictRxs.ContainsKey(4)) {
							field.FieldValue=pat.FName+" "+pat.MiddleI+"  "+pat.LName;
						}
						break;
					case "pat.nameFL5":
						if(dictRxs.ContainsKey(5)) {
							field.FieldValue=pat.FName+" "+pat.MiddleI+"  "+pat.LName;
						}
						break;
					case "pat.nameFL6":
						if(dictRxs.ContainsKey(6)) {
							field.FieldValue=pat.FName+" "+pat.MiddleI+"  "+pat.LName;
						}
						break;
					case "pat.Birthdate":
						if(dictRxs.ContainsKey(1)) {
							if(pat.Birthdate.Year<1880) {
								field.FieldValue="";
							}
							else {
								field.FieldValue=pat.Birthdate.ToShortDateString();
							}
						}
						break;
					case "pat.Birthdate2":
						if(dictRxs.ContainsKey(2)) {
							if(pat.Birthdate.Year<1880) {
								field.FieldValue="";
							}
							else {
								field.FieldValue=pat.Birthdate.ToShortDateString();
							}
						}
						break;
					case "pat.Birthdate3":
						if(dictRxs.ContainsKey(3)) {
							if(pat.Birthdate.Year<1880) {
								field.FieldValue="";
							}
							else {
								field.FieldValue=pat.Birthdate.ToShortDateString();
							}
						}
						break;
					case "pat.Birthdate4":
						if(dictRxs.ContainsKey(4)) {
							if(pat.Birthdate.Year<1880) {
								field.FieldValue="";
							}
							else {
								field.FieldValue=pat.Birthdate.ToShortDateString();
							}
						}
						break;
					case "pat.Birthdate5":
						if(dictRxs.ContainsKey(5)) {
							if(pat.Birthdate.Year<1880) {
								field.FieldValue="";
							}
							else {
								field.FieldValue=pat.Birthdate.ToShortDateString();
							}
						}
						break;
					case "pat.Birthdate6":
						if(dictRxs.ContainsKey(6)) {
							if(pat.Birthdate.Year<1880) {
								field.FieldValue="";
							}
							else {
								field.FieldValue=pat.Birthdate.ToShortDateString();
							}
						}
						break;
					case "pat.HmPhone":
						if(dictRxs.ContainsKey(1)) {
							field.FieldValue=pat.HmPhone;
						}
							break;
					case "pat.HmPhone2":
						if(dictRxs.ContainsKey(2)) {
							field.FieldValue=pat.HmPhone;
						}
						break;
					case "pat.HmPhone3":
						if(dictRxs.ContainsKey(3)) {
							field.FieldValue=pat.HmPhone;
						}
						break;
					case "pat.HmPhone4":
						if(dictRxs.ContainsKey(4)) {
							field.FieldValue=pat.HmPhone;
						}
						break;
					case "pat.HmPhone5":
						if(dictRxs.ContainsKey(5)) {
							field.FieldValue=pat.HmPhone;
						}
						break;
					case "pat.HmPhone6":
						if(dictRxs.ContainsKey(6)) {
							field.FieldValue=pat.HmPhone;
						}
						break;
					case "pat.address":
						if(dictRxs.ContainsKey(1)) {
							field.FieldValue=pat.Address;
							if(pat.Address2!="") {
								field.FieldValue+="\r\n"+pat.Address2;
							}
						}
						break;
					case "pat.address2":
						if(dictRxs.ContainsKey(2)) {
							field.FieldValue=pat.Address;
							if(pat.Address2!="") {
								field.FieldValue+="\r\n"+pat.Address2;
							}
						}
						break;
					case "pat.address3":
						if(dictRxs.ContainsKey(3)) {
							field.FieldValue=pat.Address;
							if(pat.Address2!="") {
								field.FieldValue+="\r\n"+pat.Address2;
							}
						}
						break;
					case "pat.address4":
						if(dictRxs.ContainsKey(4)) {
							field.FieldValue=pat.Address;
							if(pat.Address2!="") {
								field.FieldValue+="\r\n"+pat.Address2;
							}
						}
						break;
					case "pat.address5":
						if(dictRxs.ContainsKey(5)) {
							field.FieldValue=pat.Address;
							if(pat.Address2!="") {
								field.FieldValue+="\r\n"+pat.Address2;
							}
						}
						break;
					case "pat.address6":
						if(dictRxs.ContainsKey(6)) {
							field.FieldValue=pat.Address;
							if(pat.Address2!="") {
								field.FieldValue+="\r\n"+pat.Address2;
							}
						}
						break;
					case "pat.cityStateZip":
						if(dictRxs.ContainsKey(1)) {
							field.FieldValue=pat.City+", "+pat.State+" "+pat.Zip;
						}
						break;
					case "pat.cityStateZip2":
						if(dictRxs.ContainsKey(2)) {
							field.FieldValue=pat.City+", "+pat.State+" "+pat.Zip;
						}
						break;
					case "pat.cityStateZip3":
						if(dictRxs.ContainsKey(3)) {
							field.FieldValue=pat.City+", "+pat.State+" "+pat.Zip;
						}
						break;
					case "pat.cityStateZip4":
						if(dictRxs.ContainsKey(4)) {
							field.FieldValue=pat.City+", "+pat.State+" "+pat.Zip;
						}
						break;
					case "pat.cityStateZip5":
						if(dictRxs.ContainsKey(5)) {
							field.FieldValue=pat.City+", "+pat.State+" "+pat.Zip;
						}
						break;
					case "pat.cityStateZip6":
						if(dictRxs.ContainsKey(6)) {
							field.FieldValue=pat.City+", "+pat.State+" "+pat.Zip;
						}
						break;
					case "Drug":
						if(dictRxs.ContainsKey(1)) {
							field.FieldValue=dictRxs[1].Drug;
						}
						break;
					case "Drug2":
						if(dictRxs.ContainsKey(2)) {
							field.FieldValue=dictRxs[2].Drug;
						}
						break;
					case "Drug3":
						if(dictRxs.ContainsKey(3)) {
							field.FieldValue=dictRxs[3].Drug;
						}
						break;
					case "Drug4":
						if(dictRxs.ContainsKey(4)) {
							field.FieldValue=dictRxs[4].Drug;
						}
						break;
					case "Drug5":
						if(dictRxs.ContainsKey(5)) {
							field.FieldValue=dictRxs[5].Drug;
						}
						break;
					case "Drug6":
						if(dictRxs.ContainsKey(6)) {
							field.FieldValue=dictRxs[6].Drug;
						}
						break;
					case "Disp":
						if(dictRxs.ContainsKey(1)) {
							field.FieldValue=dictRxs[1].Disp;
						}
						break;
					case "Disp2":
						if(dictRxs.ContainsKey(2)) {
							field.FieldValue=dictRxs[2].Disp;
						}
						break;
					case "Disp3":
						if(dictRxs.ContainsKey(3)) {
							field.FieldValue=dictRxs[3].Disp;
						}
						break;
					case "Disp4":
						if(dictRxs.ContainsKey(4)) {
							field.FieldValue=dictRxs[4].Disp;
						}
						break;
					case "Disp5":
						if(dictRxs.ContainsKey(5)) {
							field.FieldValue=dictRxs[5].Disp;
						}
						break;
					case "Disp6":
						if(dictRxs.ContainsKey(6)) {
							field.FieldValue=dictRxs[6].Disp;
						}
						break;
					case "Sig":
						if(dictRxs.ContainsKey(1)) {
							field.FieldValue=dictRxs[1].Sig;
						}
						break;
					case "Sig2":
						if(dictRxs.ContainsKey(2)) {
							field.FieldValue=dictRxs[2].Sig;
						}
						break;
					case "Sig3":
						if(dictRxs.ContainsKey(3)) {
							field.FieldValue=dictRxs[3].Sig;
						}
						break;
					case "Sig4":
						if(dictRxs.ContainsKey(4)) {
							field.FieldValue=dictRxs[4].Sig;
						}
						break;
					case "Sig5":
						if(dictRxs.ContainsKey(5)) {
							field.FieldValue=dictRxs[5].Sig;
						}
						break;
					case "Sig6":
						if(dictRxs.ContainsKey(6)) {
							field.FieldValue=dictRxs[6].Sig;
						}
						break;
					case "Refills":
						if(dictRxs.ContainsKey(1)) {
							field.FieldValue=dictRxs[1].Refills;
						}
						break;
					case "Refills2":
						if(dictRxs.ContainsKey(2)) {
							field.FieldValue=dictRxs[2].Refills;
						}
						break;
					case "Refills3":
						if(dictRxs.ContainsKey(3)) {
							field.FieldValue=dictRxs[3].Refills;
						}
						break;
					case "Refills4":
						if(dictRxs.ContainsKey(4)) {
							field.FieldValue=dictRxs[4].Refills;
						}
						break;
					case "Refills5":
						if(dictRxs.ContainsKey(5)) {
							field.FieldValue=dictRxs[5].Refills;
						}
						break;
					case "Refills6":
						if(dictRxs.ContainsKey(6)) {
							field.FieldValue=dictRxs[6].Refills;
						}
						break;
					case "prov.stateRxID":
						if(dictRxs.ContainsKey(1)) {
							field.FieldValue=dictProvs[1].StateRxID;
						}
						break;
					case "prov.stateRxID2":
						if(dictRxs.ContainsKey(2)) {
							field.FieldValue=dictProvs[2].StateRxID;
						}
						break;
					case "prov.stateRxID3":
						if(dictRxs.ContainsKey(3)) {
							field.FieldValue=dictProvs[3].StateRxID;
						}
						break;
					case "prov.stateRxID4":
						if(dictRxs.ContainsKey(4)) {
							field.FieldValue=dictProvs[4].StateRxID;
						}
						break;
					case "prov.stateRxID5":
						if(dictRxs.ContainsKey(5)) {
							field.FieldValue=dictProvs[5].StateRxID;
						}
						break;
					case "prov.stateRxID6":
						if(dictRxs.ContainsKey(6)) {
							field.FieldValue=dictProvs[6].StateRxID;
						}
						break;
					case "prov.StateLicense":
						if(dictRxs.ContainsKey(1)) {
							field.FieldValue=dictProvs[1].StateLicense;
						}
						break;
					case "prov.StateLicense2":
						if(dictRxs.ContainsKey(2)) {
							field.FieldValue=dictProvs[2].StateLicense;
						}
						break;
					case "prov.StateLicense3":
						if(dictRxs.ContainsKey(3)) {
							field.FieldValue=dictProvs[3].StateLicense;
						}
						break;
					case "prov.StateLicense4":
						if(dictRxs.ContainsKey(4)) {
							field.FieldValue=dictProvs[4].StateLicense;
						}
						break;
					case "prov.StateLicense5":
						if(dictRxs.ContainsKey(5)) {
							field.FieldValue=dictProvs[5].StateLicense;
						}
						break;
					case "prov.StateLicense6":
						if(dictRxs.ContainsKey(6)) {
							field.FieldValue=dictProvs[6].StateLicense;
						}
						break;
					case "prov.NationalProvID":
						if(dictRxs.ContainsKey(1)) {
							field.FieldValue=dictProvs[1].NationalProvID;
						}
						break;
					case "prov.NationalProvID2":
						if(dictRxs.ContainsKey(2)) {
							field.FieldValue=dictProvs[2].NationalProvID;
						}
						break;
					case "prov.NationalProvID3":
						if(dictRxs.ContainsKey(3)) {
							field.FieldValue=dictProvs[3].NationalProvID;
						}
						break;
					case "prov.NationalProvID4":
						if(dictRxs.ContainsKey(4)) {
							field.FieldValue=dictProvs[4].NationalProvID;
						}
						break;
					case "prov.NationalProvID5":
						if(dictRxs.ContainsKey(5)) {
							field.FieldValue=dictProvs[5].NationalProvID;
						}
						break;
					case "prov.NationalProvID6":
						if(dictRxs.ContainsKey(6)) {
							field.FieldValue=dictProvs[6].NationalProvID;
						}
						break;
				}
			}
		}

	}
}
