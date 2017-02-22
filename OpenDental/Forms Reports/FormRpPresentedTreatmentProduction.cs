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
	public partial class FormRpPresentedTreatmentProduction:ODForm {
		private List<Userod> _listUsers;
		private List<Clinic> _listClinics;
		public FormRpPresentedTreatmentProduction() {
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
				groupType.Location=new Point(185,225);
				groupOrder.Location=new Point(185,295);
				groupUser.Location=new Point(185,365);
				listUser.Width+=30;
			}
		}

		private void RunTotals(List<long> listUserNums,List<long> listClinicsNums) {
			ReportComplex report=new ReportComplex(true,false);
			report.AddTitle("Title",Lan.g(this,"Treatment Plan Presenter Totals"));
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
			string queryText=@"SELECT userod.UserName, COUNT(proc.ProcNum) CountProcs, 
				COALESCE(SUM(proc.ProcFees),0)-COALESCE(SUM(WO.CapCompWO),0) GrossProd,
				COALESCE(SUM(WO.WriteOff),0) WriteOffs,
				COALESCE(SUM(adjs.AdjAmt),0) AS Adjustments,
				COALESCE(SUM(proc.ProcFees),0) + COALESCE(SUM(adjs.AdjAmt),0) - (COALESCE(SUM(WO.WriteOff),0) + COALESCE(SUM(WO.CapCompWO),0)) NetProd
				FROM ( -- get all procedures to consider.
					SELECT procedurelog.ProcNum,
					procedurelog.ProcFee*(procedurelog.UnitQty+procedurelog.BaseUnits) ProcFees, 
					treatpl.UserNumPresenter, treatpl.SecUserNumEntry
					FROM procedurelog
					INNER JOIN (
						SELECT treatplan.UserNumPresenter, treatplan.SecUserNumEntry,trtplan.ProcNumOrig
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
					)treatpl ON treatpl.ProcNumOrig = procedurelog.ProcNum	
					WHERE procedurelog.ProcDate BETWEEN "+POut.Date(date1.SelectionStart)+" AND "+POut.Date(date2.SelectionStart)+@"
						AND procedurelog.ProcStatus = "+POut.Int((int)ProcStat.C);
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
					SUM(IF(claimproc.Status="+POut.Int((int)ClaimProcStatus.CapComplete)+@",claimproc.WriteOff,0)) CapCompWO,
					claimproc.ProcNum
					FROM claimproc
					WHERE claimproc.Status IN ("
								+POut.Int((int)ClaimProcStatus.NotReceived)
								+","+POut.Int((int)ClaimProcStatus.Received)
								+","+POut.Int((int)ClaimProcStatus.Supplemental)
								+","+POut.Int((int)ClaimProcStatus.CapComplete)
					+@") -- NotReceived, Received, Supp, CapComplete
					GROUP BY claimproc.ProcNum
				)WO ON WO.ProcNum = proc.ProcNum
				LEFT JOIN ( -- adjustments
					SELECT SUM(adjustment.AdjAmt) AdjAmt, adjustment.ProcNum
					FROM adjustment
					GROUP BY adjustment.ProcNum
				)adjs ON adjs.ProcNum = proc.ProcNum ";
			if(radioPresenter.Checked) {
				queryText+=@"INNER JOIN userod ON userod.UserNum = proc.UserNumPresenter
					AND userod.UserNum IN ("+String.Join(",",listUserNums)+@")
				GROUP BY proc.UserNumPresenter";
			}
			else {
				queryText+=@"INNER JOIN userod ON userod.UserNum = proc.SecUserNumEntry
					AND userod.UserNum IN ("+String.Join(",",listUserNums)+@")
				GROUP BY proc.SecUserNumEntry";
			}
			QueryObject query=report.AddQuery(queryText,"","",SplitByKind.None,1,true);
			query.AddColumn(Lan.g(this,"Presenter"),100,FieldValueType.String);
			query.AddColumn(Lan.g(this,"# of Procs"),70,FieldValueType.Integer);
			query.AddColumn(Lan.g(this,"GrossProd"),100,FieldValueType.Number);
			query.AddColumn(Lan.g(this,"WriteOffs"),100,FieldValueType.Number);
			query.AddColumn(Lan.g(this,"Adjustments"),100,FieldValueType.Number);
			query.AddColumn(Lan.g(this,"NetProduction"),100,FieldValueType.Number);
			if(!report.SubmitQueries()) {
				DialogResult=DialogResult.Cancel;
				return;
			}
			FormReportComplex FormR=new FormReportComplex(report);
			FormR.ShowDialog();
			DialogResult=DialogResult.OK;
		}

		private void RunDetailed(List<long> listUserNums,List<long> listClinicsNums) {
			ReportComplex report=new ReportComplex(true,false);
			report.AddTitle("Title",Lan.g(this,"Treatment Plan Presenter Totals"));
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
			string queryText=@"SELECT userod.UserName,DatePresented, proc.Descript ProcDescript,
				COALESCE(proc.ProcFees,0) - COALESCE(WO.CapCompWO,0) GrossProd,
				COALESCE(WO.WriteOff,0) WriteOffs,
				COALESCE(adjs.AdjAmt,0) AS Adjustments,
				COALESCE(proc.ProcFees,0) + COALESCE(adjs.AdjAmt,0) - (COALESCE(WO.WriteOff,0) + COALESCE(WO.CapCompWO,0)) NetProd
				FROM ( -- get all procedures to consider.
					SELECT procedurelog.ProcNum,
					procedurelog.ProcFee*(procedurelog.UnitQty+procedurelog.BaseUnits) ProcFees, 
					treatpl.UserNumPresenter, treatpl.SecUserNumEntry, procedurecode.Descript,DatePresented
					FROM procedurelog
					INNER JOIN (
						SELECT treatplan.UserNumPresenter, treatplan.SecUserNumEntry,trtplan.ProcNumOrig,DatePresented
						FROM (
							SELECT MIN(treatplan.TreatPlanNum) AS TreatPlanNum,proctp.ProcNumOrig,DatePresented
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
					)treatpl ON treatpl.ProcNumOrig = procedurelog.ProcNum	
					INNER JOIN procedurecode ON procedurecode.CodeNum = procedurelog.CodeNum
					WHERE procedurelog.ProcDate BETWEEN "+POut.Date(date1.SelectionStart)+" AND "+POut.Date(date2.SelectionStart)+@"
						AND procedurelog.ProcStatus = "+POut.Int((int)ProcStat.C);
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
					SUM(IF(claimproc.Status="+POut.Int((int)ClaimProcStatus.CapComplete)+@",claimproc.WriteOff,0)) CapCompWO,
					claimproc.ProcNum
					FROM claimproc
					WHERE claimproc.Status IN ("
								+POut.Int((int)ClaimProcStatus.NotReceived)
								+","+POut.Int((int)ClaimProcStatus.Received)
								+","+POut.Int((int)ClaimProcStatus.Supplemental)
								+","+POut.Int((int)ClaimProcStatus.CapComplete)
					+@") -- NotReceived, Received, Supp, CapComplete
					GROUP BY claimproc.ProcNum
				)WO ON WO.ProcNum = proc.ProcNum
				LEFT JOIN ( -- adjustments
					SELECT SUM(adjustment.AdjAmt) AdjAmt, adjustment.ProcNum
					FROM adjustment
					GROUP BY adjustment.ProcNum
				)adjs ON adjs.ProcNum = proc.ProcNum ";
			if(radioPresenter.Checked) {
				queryText+=@"INNER JOIN userod ON userod.UserNum = proc.UserNumPresenter";
			}
			else {
				queryText+=@"INNER JOIN userod ON userod.UserNum = proc.SecUserNumEntry";
			}
			queryText+=@" 
				AND userod.UserNum IN ("+String.Join(",",listUserNums)+@")
				GROUP BY proc.ProcNum
				ORDER BY userod.UserName";
			QueryObject query=report.AddQuery(queryText,"","",SplitByKind.None,1,true);
			query.AddColumn(Lan.g(this,"Presenter"),100,FieldValueType.String);
			query.AddColumn(Lan.g(this,"DatePresented"),100,FieldValueType.Date);
			query.AddColumn(Lan.g(this,"Descript"),200,FieldValueType.String);
			query.AddColumn(Lan.g(this,"GrossProd"),100,FieldValueType.Number);
			query.AddColumn(Lan.g(this,"WriteOffs"),100,FieldValueType.Number);
			query.AddColumn(Lan.g(this,"Adjustments"),100,FieldValueType.Number);
			query.AddColumn(Lan.g(this,"NetProduction"),100,FieldValueType.Number);
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
			if(radioDetailed.Checked) {
				RunDetailed(listUserNums,listClinicNums);
			}
			else {
				RunTotals(listUserNums,listClinicNums);
			}
		}

		private void butCancel_Click(object sender,EventArgs e) {
			DialogResult=DialogResult.Cancel;
		}
	}
}