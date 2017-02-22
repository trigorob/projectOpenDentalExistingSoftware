using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenDental.UI;
using OpenDentBusiness;
using System.Linq;

namespace OpenDental {
	public partial class FormPhoneEmpDefaultEscalationEdit:ODForm {

		///<summary>Master list of employees. Only get this once.</summary>
		List<PhoneEmpDefault> _listPED;
		///<summary>Original dictinary of subgroup types and their list of employees when form opened.</summary>
		private Dictionary<PhoneEmpSubGroupType,List<PhoneEmpSubGroup>> _dictSubGroupsOld;
		///<summary>On load this becomes a copy of _dictSubGroupsOld. Then as we make changes this is updated and used to sync with _dictSubGroupsOld.</summary>
		private Dictionary<PhoneEmpSubGroupType,List<PhoneEmpSubGroup>> _dictSubGroupsNew;

		public FormPhoneEmpDefaultEscalationEdit() {
			InitializeComponent();
			Lan.F(this);
		}

		private void FormEscalationTeamHQ_Load(object sender,System.EventArgs e) {
			FillTabs();
			List<PhoneEmpSubGroup> listGroupsAll = PhoneEmpSubGroups.GetAll();
			_dictSubGroupsOld=Enum.GetValues(typeof(PhoneEmpSubGroupType)).Cast<PhoneEmpSubGroupType>().ToDictionary(x=>x,x=>listGroupsAll.FindAll(y=>y.SubGroupType==x));
			_dictSubGroupsNew=_dictSubGroupsOld.ToDictionary(x=>x.Key,x=>x.Value.Select(y=>y.Copy()).ToList());
			//Get all employees.
			_listPED=PhoneEmpDefaults.Refresh();
			//Sort by name.
			_listPED.Sort(new PhoneEmpDefaults.PhoneEmpDefaultComparer(PhoneEmpDefaults.PhoneEmpDefaultComparer.SortBy.name));
			FillGrids();
		}
		
		///<summary>Clears tabs and populates with values from enum PhoneEmpSubGroupType.</summary>
		private void FillTabs() {
			tabMain.TabPages.Clear();
			foreach(PhoneEmpSubGroupType e in Enum.GetValues(typeof(PhoneEmpSubGroupType))) {
				string title=e.ToString();
				tabMain.TabPages.Add(title,title);//Both key and value are the same.
			}
		}

		///<summary>Fills both grids for currently selected tab.</summary>
		private void FillGrids() {
			PhoneEmpSubGroupType typeCur=(PhoneEmpSubGroupType)tabMain.SelectedIndex;
			List<PhoneEmpSubGroup> listEsc=_dictSubGroupsNew[typeCur];
			listEsc=listEsc.OrderBy(x => x.EscalationOrder).ToList();
			//Fill escalation grid.
			gridEscalation.BeginUpdate();
			gridEscalation.Columns.Clear();
			gridEscalation.Columns.Add(new ODGridColumn("Employee",gridEscalation.Width));
			gridEscalation.Rows.Clear();
			ODGridRow row;
			for(int i=0;i<listEsc.Count;i++) {
				row=new ODGridRow(Employees.GetEmp(listEsc[i].EmployeeNum).FName);
				row.Tag=_listPED.FirstOrDefault(x => x.EmployeeNum==listEsc[i].EmployeeNum);//can be null
				gridEscalation.Rows.Add(row);
				//Set escalation order for this employee.
				//Must happen after the add in order to keep the Escalation order 1-based.
				listEsc[i].EscalationOrder=gridEscalation.Rows.Count;
			}
			gridEscalation.EndUpdate();
			//Fill employee grid.
			gridEmployees.BeginUpdate();
			gridEmployees.Columns.Clear();
			gridEmployees.Columns.Add(new ODGridColumn("Employee",gridEmployees.Width));
			gridEmployees.Rows.Clear();
			for(int i=0;i<_listPED.Count;i++) {
				row=new ODGridRow();
				//Omit employee who are already included in escalation grid.
				if(listEsc.Any(x => x.EmployeeNum==_listPED[i].EmployeeNum)) {
					continue;
				}
				row.Cells.Add(_listPED[i].EmpName.ToString());
				row.Tag=_listPED[i];
				gridEmployees.Rows.Add(row);
			}
			gridEmployees.EndUpdate();	
		}
		
		private void tabMain_SelectedIndexChanged(object sender,EventArgs e) {
			if(!tabMain.TabPages.ContainsKey(PhoneEmpSubGroupType.Escal.ToString())) {//Control has not been initialized.
				return;
			}
			gridEmployees.ScrollValue=0;//scroll to top
			gridEscalation.ScrollValue=0;//scroll to top
			FillGrids();
		}

		private void butRight_Click(object sender,EventArgs e) {
			if(gridEmployees.SelectedIndices.Length<=0) {
				return;
			}
			PhoneEmpSubGroupType typeCur=(PhoneEmpSubGroupType)tabMain.SelectedIndex;
			foreach(int i in gridEmployees.SelectedIndices) {
				PhoneEmpDefault pedKeep=(PhoneEmpDefault)gridEmployees.Rows[i].Tag;
				_dictSubGroupsNew[typeCur].Add(new PhoneEmpSubGroup(pedKeep.EmployeeNum,typeCur,0));
			}
			FillGrids();
		}

		private void butLeft_Click(object sender,EventArgs e) {
			if(gridEscalation.SelectedIndices.Length<=0) {
				return;
			}
			PhoneEmpSubGroupType typeCur=(PhoneEmpSubGroupType)tabMain.SelectedIndex;
			foreach(int i in gridEscalation.SelectedIndices) {
				PhoneEmpDefault pedCur=(PhoneEmpDefault)gridEscalation.Rows[i].Tag;
				_dictSubGroupsNew[typeCur].RemoveAll(x => x.EmployeeNum==pedCur.EmployeeNum);
			}
			FillGrids();
		}

		private void butUp_Click(object sender,EventArgs e) {
			if(gridEscalation.SelectedIndices.Length!=1) {
				MsgBox.Show(this,"Select 1 item from escalation list");
				return;
			}
			if(gridEscalation.SelectedIndices[0]==0) {
				return;
			}
			//Retain current selection.
			int curSelectedIndex=Math.Max(gridEscalation.SelectedIndices[0]-1,0);
			PhoneEmpSubGroupType typeCur=(PhoneEmpSubGroupType)tabMain.SelectedIndex;
			List<int> selectedIndices=new List<int>(gridEscalation.SelectedIndices);
			for(int i=0;i<gridEscalation.Rows.Count;i++) {
				PhoneEmpDefault ped=(PhoneEmpDefault)gridEscalation.Rows[i].Tag;
				if(selectedIndices[0]==i+1) {
					//First should be safe to use here because it must exist in gridescalation to get here.
					_dictSubGroupsNew[typeCur].First(x => x.EmployeeNum==ped.EmployeeNum).EscalationOrder++;
				}
				else if(selectedIndices[0]==i) {
					_dictSubGroupsNew[typeCur].First(x => x.EmployeeNum==ped.EmployeeNum).EscalationOrder--;
				}
			}
			FillGrids();
			//Reset selection so moving up the list rapidly is easier.
			gridEscalation.SetSelected(curSelectedIndex,true);
		}

		private void butDown_Click(object sender,EventArgs e) {
			if(gridEscalation.SelectedIndices.Length!=1) {
				MsgBox.Show(this,"Select 1 item from escalation list");
				return;
			}
			if(gridEscalation.SelectedIndices[0]>=(gridEscalation.Rows.Count-1)) {
				return;
			}
			//Retain current selection.
			int curSelectedIndex=Math.Min(gridEscalation.SelectedIndices[0]+1,(gridEscalation.Rows.Count-1));
			PhoneEmpSubGroupType typeCur=(PhoneEmpSubGroupType)tabMain.SelectedIndex;
			List<int> selectedIndices=new List<int>(gridEscalation.SelectedIndices);
			for(int i=0;i<gridEscalation.Rows.Count;i++) {
				PhoneEmpDefault ped=(PhoneEmpDefault)gridEscalation.Rows[i].Tag;
				if(selectedIndices[0]==i) {
					_dictSubGroupsNew[typeCur].First(x => x.EmployeeNum==ped.EmployeeNum).EscalationOrder++;
				}
				else if(selectedIndices[0]==i-1) {
					_dictSubGroupsNew[typeCur].First(x => x.EmployeeNum==ped.EmployeeNum).EscalationOrder--;
				}
			}
			FillGrids();
			//Reset selection so moving down the list rapidly is easier.
			gridEscalation.SetSelected(curSelectedIndex,true);
		}

		private void butOK_Click(object sender,System.EventArgs e) {
			PhoneEmpSubGroups.Sync(_dictSubGroupsNew.SelectMany(x=>x.Value).ToList(),_dictSubGroupsOld.SelectMany(x=>x.Value).ToList());
				DialogResult=DialogResult.OK;
		}

		private void butCancel_Click(object sender,System.EventArgs e) {
			this.DialogResult=DialogResult.Cancel;
		}

		private void gridEmployees_CellDoubleClick(object sender,ODGridClickEventArgs e) {

		}
	}
}
