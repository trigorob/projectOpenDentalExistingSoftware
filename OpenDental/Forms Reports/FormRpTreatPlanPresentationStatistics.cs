using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenDental.ReportingComplex;
using OpenDentBusiness;
using System.Linq;

namespace OpenDental {
	public partial class FormRpTreatPlanPresentationStatistics:ODForm {
		private List<Userod> _listUsers;
		private List<Clinic> _listClinics;
		public FormRpTreatPlanPresentationStatistics() {
			InitializeComponent();
			Lan.F(this);
		}

		private void FormRpTreatPlanPresenter_Load(object sender,EventArgs e) {
			date1.SelectionStart=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1).AddMonths(-1);
			date2.SelectionStart=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1).AddDays(-1);
			_listUsers=UserodC.ShortList;
			listUser.Items.AddRange(_listUsers.Select(x => x.UserName).ToArray());
			checkAllUsers.Checked=true;
			if(PrefC.HasClinicsEnabled) {
				if(!Security.CurUser.ClinicIsRestricted) {
					listClin.Items.Add(Lan.g(this,"Unassigned"));
				}
				_listClinics=Clinics.GetForUserod(Security.CurUser);
				listClin.Items.AddRange(_listClinics.Select(x => x.Abbr).ToArray());
				checkAllClinics.Checked=true;
			}
			else {
				listClin.Visible=false;
				checkAllClinics.Visible=false;
				labelClin.Visible=false;
				groupGrossNet.Location=new Point(185,225);
				groupOrder.Location=new Point(185,295);
				groupUser.Location=new Point(185,365);
				listUser.Width+=30;
			}
		}

		private void RunReport(List<long> listUserNums,List<long> listClinicsNums) {
			ReportComplex report=new ReportComplex(true,false);
			report.AddTitle("Title",Lan.g(this,"Presented Proc Totals"));
			report.AddSubTitle("PracTitle",PrefC.GetString(PrefName.PracticeTitle));
			report.AddSubTitle("Date",date1.SelectionStart.ToShortDateString()+" - "+date2.SelectionStart.ToShortDateString());
			if(checkAllUsers.Checked) {
				report.AddSubTitle("Users",Lan.g(this,"All Users"));
			}
			else {
				string strUsers="";
				for(int i = 0;i < listUser.SelectedIndices.Count;i++) {
					if(i == 0) {
						strUsers=_listUsers[listUser.SelectedIndices[i]].UserName;
					}
					else {
						strUsers+=", "+_listUsers[listUser.SelectedIndices[i]].UserName;
					}
				}
				report.AddSubTitle("Users",strUsers);
			}
			if(PrefC.HasClinicsEnabled) {
				if(checkAllClinics.Checked) {
					report.AddSubTitle("Clinics",Lan.g(this,"All Clinics"));
				}
				else {
					string clinNames="";
					for(int i = 0;i<listClin.SelectedIndices.Count;i++) {
						if(i>0) {
							clinNames+=", ";
						}
						if(Security.CurUser.ClinicIsRestricted) {
							clinNames+=_listClinics[listClin.SelectedIndices[i]].Abbr;
						}
						else {
							if(listClin.SelectedIndices[i]==0) {
								clinNames+=Lan.g(this,"Unassigned");
							}
							else {
								clinNames+=_listClinics[listClin.SelectedIndices[i]-1].Abbr;//Minus 1 from the selected index
							}
						}
					}
					report.AddSubTitle("Clinics",clinNames);
				}
			}
			string queryText=@" SELECT userod.UserName AS Presenter, 
				COUNT(DISTINCT proc.TreatPlanNum) AS '#Plans',
				COUNT(proc.ProcNum) AS '#Procs', 
				SUM(IF(proc.ProcStatus="+POut.Int((int)ProcStat.TP)+@" AND proc.AptNum IS NOT NULL,1,0)) AS '#ProcSched', 
				SUM(IF(proc.ProcStatus="+POut.Int((int)ProcStat.C)+@",1,0)) AS '#ProcComp', ";
			if(radioGross.Checked) {
				queryText+=@" COALESCE(SUM(proc.ProcFee),0) - COALESCE(SUM(WO.CapCompWO),0) AS '$TP',
				SUM(IF(proc.ProcStatus="+POut.Int((int)ProcStat.TP)+@" AND proc.AptNum IS NOT NULL,COALESCE(proc.ProcFee,0) - COALESCE(WO.CapCompWO,0),0)) AS '$ProcSched',
				SUM(IF(proc.ProcStatus="+POut.Int((int)ProcStat.C)+@",COALESCE(proc.ProcFee,0) - COALESCE(WO.CapCompWO,0),0)) AS '$ProcComp' ";
			}
			else {
				queryText+=@" SUM(CASE WHEN 
				proc.ProcStatus = "+POut.Int((int)ProcStat.C)+@" THEN 
					COALESCE(proc.ProcFee,0) + COALESCE(adjs.AdjAmt,0) - (COALESCE(WO.CapCompWO,0) + COALESCE(WO.WriteOff,0))
				ELSE
					COALESCE(proc.ProcFee,0) + COALESCE(adjs.AdjAmt,0) - (COALESCE(WO.CapCompWO,0) + COALESCE(WO.WriteOffEst,0) + proc.Discount)
				END) AS '$NetTP',
				SUM(CASE WHEN 
				proc.ProcStatus = "+POut.Int((int)ProcStat.TP)+@" AND proc.AptNum IS NOT NULL THEN 
					COALESCE(proc.ProcFee,0) + COALESCE(adjs.AdjAmt,0) - (COALESCE(WO.CapCompWO,0) + COALESCE(WO.WriteOffEst,0) + proc.Discount)
				ELSE
					0
				END) AS '$NetSched',
				SUM(CASE WHEN 
				proc.ProcStatus = "+POut.Int((int)ProcStat.C)+@" THEN 
					COALESCE(proc.ProcFee,0) + COALESCE(adjs.AdjAmt,0) - (COALESCE(WO.CapCompWO,0) + COALESCE(WO.WriteOff,0))
				ELSE
					0
				END) AS '$NetComp' ";
			}
			queryText+=@" FROM ( #get all procedures to consider
					SELECT procedurelog.ProcNum,
					procedurelog.ProcFee*(procedurelog.UnitQty+procedurelog.BaseUnits) ProcFee, 
					treatpl.UserNumPresenter, treatpl.SecUserNumEntry, procedurelog.ProcStatus, treatpl.TreatPlanNum,
					appointment.AptNum, procedurelog.Discount
					FROM procedurelog
					INNER JOIN (
						SELECT treatplan.UserNumPresenter, treatplan.SecUserNumEntry,trtplan.ProcNumOrig, treatplan.TreatPlanNum
						FROM (
							SELECT MIN(treatplan.TreatPlanNum) AS TreatPlanNum,proctp.ProcNumOrig
							FROM (
								SELECT ";
			if(radioFirstPresented.Checked) {
				queryText+=" MIN(treatplan.DateTP) DatePresented, ";
			}
			else {
				queryText+=" MAX(treatplan.DateTP) DatePresented, ";
			}
			queryText+=@" proctp.ProcNumOrig
								FROM treatplan
								INNER JOIN proctp ON proctp.TreatPlanNum = treatplan.TreatPlanNum
								GROUP BY proctp.ProcNumOrig
							) tpmaxdate
							INNER JOIN proctp ON proctp.ProcNumOrig = tpmaxdate.ProcNumOrig
							INNER JOIN treatplan ON treatplan.TreatPlanNum = proctp.TreatPlanNum 
								AND treatplan.DateTP = tpmaxdate.DatePresented
							GROUP BY proctp.ProcNumOrig
						) trtplan
						INNER JOIN treatplan ON treatplan.TreatPlanNum = trtplan.TreatPlanNum 
							AND treatplan.DateTP BETWEEN "+POut.Date(date1.SelectionStart)+" AND "+POut.Date(date2.SelectionStart)+@"
					)treatpl ON treatpl.ProcNumOrig = procedurelog.ProcNum	
					LEFT JOIN appointment ON appointment.AptNum = procedurelog.AptNum
						AND appointment.AptStatus IN ("+POut.Int((int)ApptStatus.Scheduled)+","+POut.Int((int)ApptStatus.ASAP)+@")
					WHERE procedurelog.ProcStatus IN ("+POut.Int((int)ProcStat.TP)+","+POut.Int((int)ProcStat.C)+") ";
			if(PrefC.HasClinicsEnabled) {
				queryText+=" AND procedurelog.ClinicNum IN ("+String.Join(",",listClinicsNums)+") ";
			}
			queryText+=@" GROUP BY procedurelog.ProcNum
				)proc	
				LEFT JOIN ( -- writeoffs
					SELECT SUM(IF(claimproc.Status IN ("
					+POut.Int((int)ClaimProcStatus.NotReceived)
					+","+POut.Int((int)ClaimProcStatus.Received)
					+","+POut.Int((int)ClaimProcStatus.Supplemental)
				+@"),claimproc.WriteOff,0)) WriteOff, 
					SUM(IF(claimproc.Status = "+POut.Int((int)ClaimProcStatus.Estimate)+@",IF(claimproc.WriteOffEstOverride!=-1,claimproc.WriteOffEstOverride,claimproc.WriteOffEst),0)) WriteOffEst, 
					SUM(IF(claimproc.Status = "+POut.Int((int)ClaimProcStatus.CapComplete)+@",claimproc.WriteOff,0)) CapCompWO, 
					claimproc.ProcNum
					FROM claimproc
					WHERE claimproc.Status IN ("
					+POut.Int((int)ClaimProcStatus.NotReceived)
					+","+POut.Int((int)ClaimProcStatus.Received)
					+","+POut.Int((int)ClaimProcStatus.Supplemental)
					+","+POut.Int((int)ClaimProcStatus.Estimate)
					+","+POut.Int((int)ClaimProcStatus.CapComplete)
				+@") -- NotReceived, Received, Supp, estimate
					GROUP BY claimproc.ProcNum
				)WO ON WO.ProcNum = proc.ProcNum
				LEFT JOIN ( -- adjustments
					SELECT SUM(adjustment.AdjAmt) AdjAmt, adjustment.ProcNum
					FROM adjustment
					GROUP BY adjustment.ProcNum
				)adjs ON adjs.ProcNum = proc.ProcNum ";
			if(radioPresenter.Checked) {
				queryText+=@" INNER JOIN userod ON userod.UserNum = proc.UserNumPresenter
					AND userod.UserNum IN ("+String.Join(",",listUserNums)+@")
				GROUP BY proc.UserNumPresenter";
			}
			else {
				queryText+=@" INNER JOIN userod ON userod.UserNum = proc.SecUserNumEntry
					AND userod.UserNum IN ("+String.Join(",",listUserNums)+@")
				GROUP BY proc.SecUserNumEntry";
			}
			QueryObject query=report.AddQuery(queryText,"","",SplitByKind.None,1,true);
			query.AddColumn(Lan.g(this,"Presenter"),100,FieldValueType.String);
			query.AddColumn(Lan.g(this,"# of Plans"),85,FieldValueType.Integer);
			query.AddColumn(Lan.g(this,"# of Procs"),85,FieldValueType.Integer);
			query.AddColumn(Lan.g(this,"# of ProcsSched"),100,FieldValueType.Integer);
			query.AddColumn(Lan.g(this,"# of ProcsComp"),100,FieldValueType.Integer);
			if(radioGross.Checked) {
				query.AddColumn(Lan.g(this,"GrossTPAmt"),95,FieldValueType.Number);
				query.AddColumn(Lan.g(this,"GrossSchedAmt"),95,FieldValueType.Number);
				query.AddColumn(Lan.g(this,"GrossCompAmt"),95,FieldValueType.Number);
			}
			else {
				query.AddColumn(Lan.g(this,"NetTPAmt"),95,FieldValueType.Number);
				query.AddColumn(Lan.g(this,"NetSchedAmt"),95,FieldValueType.Number);
				query.AddColumn(Lan.g(this,"NetCompAmt"),95,FieldValueType.Number);
			}
			if(!report.SubmitQueries()) {
				DialogResult=DialogResult.Cancel;
				return;
			}
			FormReportComplex FormR=new FormReportComplex(report);
			FormR.ShowDialog();
			DialogResult=DialogResult.OK;
		}

		private void checkAllUsers_Click(object sender,EventArgs e) {
			if(checkAllUsers.Checked) {
				listUser.SelectedIndices.Clear();
			}
		}

		private void listUser_Click(object sender,EventArgs e) {
			if(listUser.SelectedIndices.Count>0) {
				checkAllUsers.Checked=false;
			}
		}

		private void checkAllClinics_Click(object sender,EventArgs e) {
			if(checkAllClinics.Checked) {
				listClin.SelectedIndices.Clear();
			}
		}

		private void listClin_Click(object sender,EventArgs e) {
			if(listClin.SelectedIndices.Count>0) {
				checkAllClinics.Checked=false;
			}
		}

		private void butOK_Click(object sender,EventArgs e) {
			if(date2.SelectionStart<date1.SelectionStart) {
				MsgBox.Show(this,"End date cannot be before start date.");
				return;
			}
			if(!checkAllUsers.Checked && listUser.SelectedIndices.Count==0) {
				MsgBox.Show(this,"Please select at least one user.");
				return;
			}
			if(PrefC.HasClinicsEnabled && !checkAllClinics.Checked && listClin.SelectedIndices.Count==0) {
				MsgBox.Show(this,"Please select at least one clinic.");
				return;
			}
			List<long> listUserNums=new List<long>();
			List<long> listClinicNums=new List<long>();
			if(checkAllUsers.Checked) {
				listUserNums=_listUsers.Select(x => x.UserNum).ToList();
			}
			else {
				listUserNums=listUser.SelectedIndices.OfType<int>().ToList().Select(x => _listUsers[x].UserNum).ToList();
			}
			if(PrefC.HasClinicsEnabled) {
				if(checkAllClinics.Checked) {
					listClinicNums=_listClinics.Select(x => x.ClinicNum).ToList();
				}
				else {
					for(int i = 0;i<listClin.SelectedIndices.Count;i++) {
						if(Security.CurUser.ClinicIsRestricted) {
							listClinicNums.Add(_listClinics[listClin.SelectedIndices[i]].ClinicNum);
						}
						else if(listClin.SelectedIndices[i]!=0) {
							listClinicNums.Add(_listClinics[listClin.SelectedIndices[i]-1].ClinicNum);
						}
					}
				}
				if(!Security.CurUser.ClinicIsRestricted && (listClin.GetSelected(0) || checkAllClinics.Checked)) {
					listClinicNums.Add(0);
				}
			}
			RunReport(listUserNums,listClinicNums);
		}

		private void butCancel_Click(object sender,EventArgs e) {
			DialogResult=DialogResult.Cancel;
		}
	}
}