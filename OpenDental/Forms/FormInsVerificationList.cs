using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.UI;
using System.Linq;

namespace OpenDental {
	public partial class FormInsVerificationList:ODForm {
		///<summary>-1 represents "All", and 0 represents "none".</summary>
		private long _verifyUserNum=-1;
		///<summary>0 represents "Unassign".</summary>
		private long _assignUserNum;
		///<summary>-1 represents "All", and 0 represents "Unassigned".</summary>
		private long _clinicNumVerifyClinicsFilter=-1;
		///<summary>-1 and 0 represent "All".</summary>
		private long _defNumVerifyRegionsFilter=-1;
		private long _defNumVerifyStatusFilter;
		private long _defNumVerifyStatusAssign;
		///<summary>This will only have a selection if selecting from gridMain.</summary>
		private InsVerifyGridObject _gridRowSelected;
		private List<Clinic> _listClinicsDb;
		private List<Clinic> _listClinicsFiltered;
		private List<Userod> _listUsersInRegionWithAssignedIns=new List<Userod>();
		private List<Userod> _listUsersInRegion=new List<Userod>();
		private List<Def> _listVerifyStatuses=new List<Def>();
		private long _userNumVerifyGrid=0;
		private int _selectedRowVerifyGrid;
		private int _selectedRowAssignGrid;
		private bool _hasLoaded=false;
		private Dictionary<long,Def> _dictStatusDefs=new Dictionary<long,Def>();

		public FormInsVerificationList() {
			InitializeComponent();
			Lan.F(this);
		}

		private void FormInsVerificationList_Load(object sender,EventArgs e) {
			if(PrefC.GetBool(PrefName.InsVerifyDefaultToCurrentUser)) {
				_verifyUserNum=Security.CurUser.UserNum;
			}
			if(!PrefC.HasClinicsEnabled) {
				labelClinic.Visible=false;
				comboVerifyClinics.Visible=false;
				labelRegion.Visible=false;
				comboVerifyRegions.Visible=false;
			}
			List<Def> listVerifyStatuses=DefC.Short[(int)DefCat.InsuranceVerificationStatus].ToList();//I may want to update this more often than only on load.  "Refresh" button perhaps?
			foreach(Def defCur in listVerifyStatuses) {
				if(!_dictStatusDefs.ContainsKey(defCur.DefNum)) {
					_dictStatusDefs.Add(defCur.DefNum,defCur);
				}
			}
			textAppointmentScheduledDays.Text=POut.Int(PrefC.GetInt(PrefName.InsVerifyAppointmentScheduledDays));
			textInsBenefitEligibilityDays.Text=POut.Int(PrefC.GetInt(PrefName.InsVerifyBenefitEligibilityDays));
			textPatientEnrollmentDays.Text=POut.Int(PrefC.GetInt(PrefName.InsVerifyPatientEnrollmentDays));
			_listClinicsDb=Clinics.GetForUserod(Security.CurUser);
			InsVerifies.CleanupInsVerifyRows(DateTime.Today,DateTime.Today.AddDays(PIn.Int(textAppointmentScheduledDays.Text)));
			FillGrids();
			_hasLoaded=true;
		}

		private void butRefresh_Click(object sender,EventArgs e) {
			FillGrids();
		}

		public void FillGrids() {
			FillComboBoxes();
			FillUsers();
			if(tabControl1.SelectedTab==tabVerify) {
				FillGridMain();
			}
			else if(tabControl1.SelectedTab==tabAssign) {
				FillGridAssign();
			}
		}

		private void FillDisplayInfo(InsVerifyGridObject gridRowObject) {
			//Always blank out the old selected information before filling with the new information.
			textSubscriberName.Text="";
			textSubscriberBirthdate.Text="";
			textSubscriberSSN.Text="";
			textSubscriberID.Text="";
			textInsPlanGroupName.Text="";
			textInsPlanGroupNumber.Text="";
			textInsPlanNote.Text="";
			textCarrierName.Text="";
			textCarrierPhoneNumber.Text="";
			textInsPlanEmployer.Text="";
			textInsVerifyReadOnlyNote.Text="";
			if(gridRowObject==null) {
				return;
			}
			PatPlan patPlanVerify=PatPlans.GetByPatPlanNum(gridRowObject.GetPatPlanNum());
			if(patPlanVerify==null) {//Should never happen, but if it does, return because it is just for display purposes.
				return;
			}
			InsSub insSubVerify=InsSubs.GetOne(patPlanVerify.InsSubNum);
			textSubscriberID.Text=insSubVerify.SubscriberID;
			Patient patSubscriberVerify=Patients.GetPat(insSubVerify.Subscriber);
			if(patSubscriberVerify!=null) {
				textSubscriberName.Text=patSubscriberVerify.GetNameFL();
				textSubscriberBirthdate.Text=patSubscriberVerify.Birthdate.ToShortDateString();
				textSubscriberSSN.Text=patSubscriberVerify.SSN;
			}
			FillInsuranceDisplay(InsPlans.GetPlan(insSubVerify.PlanNum,null));
			textPatBirthdate.Text=Patients.GetPat(gridRowObject.GetPatNum()).Birthdate.ToShortDateString();
			if(gridRowObject.IsOnlyInsRow()) {
				textInsVerifyReadOnlyNote.Text=_gridRowSelected.PlanInsVerify.Note;
			}
			else {//Both or Pat row
				textInsVerifyReadOnlyNote.Text=_gridRowSelected.PatInsVerify.Note;
			}
		}

		private void FillInsuranceDisplay(InsPlan insPlanVerify) {
			if(insPlanVerify==null) {//Should never happen, but if it does, return because it is just for display purposes.
				return;
			}
			textInsPlanGroupName.Text=insPlanVerify.GroupName;
			textInsPlanGroupNumber.Text=insPlanVerify.GroupNum;
			textInsPlanNote.Text=insPlanVerify.PlanNote;
			Employer employer=Employers.GetEmployer(insPlanVerify.EmployerNum);
			if(employer!=null) {
				textInsPlanEmployer.Text=employer.EmpName;
			}
			Carrier carrierVerify=Carriers.GetCarrier(insPlanVerify.CarrierNum);
			if(carrierVerify!=null) {
				textCarrierName.Text=carrierVerify.CarrierName;
				textCarrierPhoneNumber.Text=carrierVerify.Phone;
			}
		}

		///<summary>Does not fill the Clinic Combo Box</summary>
		private void FillComboBoxes() {
			comboSetVerifyStatus.Items.Clear();
			comboFilterVerifyStatus.Items.Clear();
			comboFilterVerifyStatus.Items.Add("All");
			comboSetVerifyStatus.Items.Add("none");
			_listVerifyStatuses=DefC.Short[(int)DefCat.InsuranceVerificationStatus].ToList();
			for(int i=0;i<_listVerifyStatuses.Count;i++) {
				comboFilterVerifyStatus.Items.Add(_listVerifyStatuses[i].ItemName);
				comboSetVerifyStatus.Items.Add(_listVerifyStatuses[i].ItemName);
				if(_listVerifyStatuses[i].DefNum==_defNumVerifyStatusFilter) {
					comboFilterVerifyStatus.SelectedIndex=i+1;
				}
				if(_listVerifyStatuses[i].DefNum==_defNumVerifyStatusAssign) {
					comboSetVerifyStatus.SelectedIndex=i+1;
				}
			}
			if(comboFilterVerifyStatus.SelectedIndex==-1) {
				comboFilterVerifyStatus.SelectedIndex=0;
			}
			if(comboSetVerifyStatus.SelectedIndex==-1) {
				comboSetVerifyStatus.SelectedIndex=0;
			}
			comboVerifyRegions.Items.Clear();
			if(PrefC.HasClinicsEnabled) {
				List<Def> listRegionDefs=DefC.Short[(int)DefCat.Regions].ToList();
				if(listRegionDefs.Count!=0) {
					listRegionDefs.RemoveAll(x => !_listClinicsDb.Any(y => y.Region==x.DefNum));
					comboVerifyRegions.Items.Add(Lan.g(this,"All"));
					for(int i=0;i<listRegionDefs.Count;i++) {
						comboVerifyRegions.Items.Add(listRegionDefs[i].ItemName);
						if(listRegionDefs[i].DefNum==_defNumVerifyRegionsFilter) {
							comboVerifyRegions.SelectedIndex=i+1;
						}
					}
					if(comboVerifyRegions.SelectedIndex==-1) {//Will select either "All" or the restricted clinic's region.
							comboVerifyRegions.SelectedIndex=0;
					}
				}
				else {
					comboVerifyRegions.Visible=false;
					labelRegion.Visible=false;
					_defNumVerifyRegionsFilter=-1;
				}
				FillClinicComboBox();
			}
		}

		private void FillClinicComboBox() {
			_listClinicsDb=Clinics.GetForUserod(Security.CurUser);
			_listClinicsFiltered=_listClinicsDb.Where(x=>x.Region==_defNumVerifyRegionsFilter).ToList();
			comboVerifyClinics.Items.Clear();
			comboVerifyClinics.SelectedIndex=-1;
			int indexOffset=0;
			if(!Security.CurUser.ClinicIsRestricted) {
				comboVerifyClinics.Items.Add(Lan.g(this,"All"));
				indexOffset=1;
			}
			if(_defNumVerifyRegionsFilter<1) {
				for(int i=0;i<_listClinicsDb.Count;i++) {
					comboVerifyClinics.Items.Add(_listClinicsDb[i].Abbr);
					if(_clinicNumVerifyClinicsFilter==_listClinicsDb[i].ClinicNum) {
						comboVerifyClinics.SelectedIndex=i+indexOffset;
					}
				}
				if(!Security.CurUser.ClinicIsRestricted) {
					comboVerifyClinics.Items.Add(Lan.g(this,"Unassigned"));//Add this at the end so it is on the bottom
					if(_clinicNumVerifyClinicsFilter==0) {
						comboVerifyClinics.SelectedIndex=comboVerifyClinics.Items.Count-1;
					}
				}
				if(comboVerifyClinics.SelectedIndex==-1 && comboVerifyClinics.Items.Count>0) {
					comboVerifyClinics.SelectedIndex=0;
				}
			}
			else {//User selected a region to filter
				for(int i=0;i<_listClinicsFiltered.Count;i++) {
					comboVerifyClinics.Items.Add(_listClinicsFiltered[i].Abbr);
					if(_clinicNumVerifyClinicsFilter==_listClinicsFiltered[i].ClinicNum) {
						comboVerifyClinics.SelectedIndex=i+indexOffset;
					}
				}
				if(comboVerifyClinics.SelectedIndex==-1 && comboVerifyClinics.Items.Count>0) {
					comboVerifyClinics.SelectedIndex=0;
				}
			}
		}

		private void FillUsers() {
			comboVerifyUser.Items.Clear();
			comboVerifyUser.SelectedIndex=-1;
			comboVerifyUser.Items.Add(Lan.g(this,"All Users"));
			comboVerifyUser.Items.Add(Lan.g(this,"Unassigned"));
			List<long> listClinicNums=new List<long>();
			if(_clinicNumVerifyClinicsFilter!=-1) {
				listClinicNums=new List<long>() { _clinicNumVerifyClinicsFilter };
			}
			else if(_defNumVerifyRegionsFilter>0) {
				listClinicNums=Clinics.GetListByRegion(_defNumVerifyRegionsFilter);
			}
			_listUsersInRegion=Userods.GetUsersForVerifyList(listClinicNums,true);
			_listUsersInRegionWithAssignedIns=Userods.GetUsersForVerifyList(listClinicNums,false);
			for(int i=0;i<_listUsersInRegionWithAssignedIns.Count;i++) {
				comboVerifyUser.Items.Add(_listUsersInRegionWithAssignedIns[i].UserName);
				if(_verifyUserNum==_listUsersInRegionWithAssignedIns[i].UserNum) {
					comboVerifyUser.SelectedIndex=i+2;//Add 2 because of the "All Users" and "Unassigned" combo items.
				}
			}
			if(_verifyUserNum==-1) {
				comboVerifyUser.SelectedIndex=0;//"All Users"
			}
			if(_verifyUserNum==0) {
				comboVerifyUser.SelectedIndex=1;//"Unassigned"
			}
			for(int i=0;i<_listUsersInRegion.Count;i++) {
				if(_assignUserNum==_listUsersInRegion[i].UserNum) {
					textAssignUser.Text=_listUsersInRegion[i].UserName;
				}
			}
			if(_assignUserNum==0) {
				textAssignUser.Text="Unassign";
			}
		}

		private void PickUser(bool isAssigning) {
			FormUserPick FormUP=new FormUserPick();
			FormUP.IsSelectionmode=true;
			FormUP.ListUserodsFiltered=_listUsersInRegion;
			if(!isAssigning) {
				FormUP.IsPickAllAllowed=true;
			}
			FormUP.IsPickNoneAllowed=true;
			FormUP.ShowDialog();
			if(FormUP.DialogResult==DialogResult.OK) {
				if(isAssigning) {//Setting the user
					_assignUserNum=FormUP.SelectedUserNum;
				}
				else {//Filter by user
					_verifyUserNum=FormUP.SelectedUserNum;
				}
				FillGrids();
			}
		}

		private List<InsVerifyGridRow> GetRowsForGrid(bool isAssignGrid) {
			List<InsVerifyGridRow> listGridRows=new List<InsVerifyGridRow>();
			bool excludePatVerifyWhenNoIns=PrefC.GetBool(PrefName.InsVerifyExcludePatVerify);
			bool excludePatClones=(PrefC.GetBool(PrefName.ShowFeaturePatientClone)==true) && PrefC.GetBool(PrefName.InsVerifyExcludePatientClones);
			DateTime dateTimeStart=DateTime.Today;
			DateTime dateTimeEnd=DateTime.Today.AddDays(PIn.Int(textAppointmentScheduledDays.Text));//Don't need to add 1 because we will be getting only the date portion of this datetime.
			DateTime dateTimeLastPatEligibility=DateTime.Today.AddDays(-PIn.Int(textPatientEnrollmentDays.Text));
			DateTime dateTimeLastPlanBenefits=DateTime.Today.AddDays(-PIn.Int(textInsBenefitEligibilityDays.Text));
			List<InsVerifyGridObject> listGridObjs=InsVerifies.GetVerifyGridList(dateTimeStart,dateTimeEnd,dateTimeLastPatEligibility,dateTimeLastPlanBenefits,_clinicNumVerifyClinicsFilter,_defNumVerifyRegionsFilter,_defNumVerifyStatusFilter,_verifyUserNum,textVerifyCarrier.Text,excludePatVerifyWhenNoIns,excludePatClones);
			foreach(InsVerifyGridObject gridObjCur in listGridObjs) {
				listGridRows.Add(new InsVerifyGridRow(gridObjCur,_dictStatusDefs,_listUsersInRegion,isAssignGrid));
			}
			return listGridRows;
		}

		private int CompareGridRows(InsVerifyGridRow x,InsVerifyGridRow y) {
			if(x.Type==y.Type && x.Clinic==y.Clinic && x.NextApptDate==y.NextApptDate) {
				return x.CarrierName.CompareTo(y.CarrierName);
			}
			if(x.Type==y.Type && x.Clinic==y.Clinic) {
				return x.NextApptDate.CompareTo(y.NextApptDate);
			}
			if(x.Type==y.Type) {
				return x.Clinic.CompareTo(y.Clinic);
			}
			return y.Type.CompareTo(x.Type);//x and y are flipped to order by Type descending (Z-A)
		}

		private ODGridRow VerifyRowToODGridRow(InsVerifyGridRow vrow,bool isAssignGrid) {
			ODGridRow row=new ODGridRow();
			row.Cells.Add(vrow.Type);
			if(PrefC.HasClinicsEnabled) {
				row.Cells.Add(vrow.Clinic);
			}
			row.Cells.Add(vrow.PatientName);
			row.Cells.Add(vrow.NextApptDate.ToString());
			row.Cells.Add(vrow.CarrierName);
			row.Cells.Add(vrow.DateLastVerified.ToShortDateString());
			row.Cells.Add(vrow.VerifyStatus);
			if(!isAssignGrid) {
				row.Cells.Add(vrow.DateLastAssigned.ToShortDateString());
			}
			row.Cells.Add(vrow.AssignedTo);
			row.Tag=vrow.Tag;
			return row;
		}

		#region Grid Verify
		private void FillGridMain() {
			_selectedRowVerifyGrid=gridMain.GetSelectedIndex();
			gridMain.BeginUpdate();
			gridMain.Columns.Clear();
			ODGridColumn col;
			col=new ODGridColumn(Lans.g(this,"Type"),45);
			gridMain.Columns.Add(col);
			if(PrefC.HasClinicsEnabled) {
				col=new ODGridColumn(Lans.g(this,"Clinic"),90);
				gridMain.Columns.Add(col);
			}
			col=new ODGridColumn(Lans.g(this,"Patient"),120);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lans.g(this,"Appt Date Time"),130,HorizontalAlignment.Center,GridSortingStrategy.DateParse);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lans.g(this,"Carrier"),160);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lans.g(this,"Last Verified"),90,HorizontalAlignment.Center,GridSortingStrategy.DateParse);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lans.g(this,"Status"),110);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lans.g(this,"Status Date"),80,HorizontalAlignment.Center);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lans.g(this,"Assigned to"),0);
			gridMain.Columns.Add(col);
			gridMain.Rows.Clear();
			List<InsVerifyGridRow> listGridRows=GetRowsForGrid(false);
			listGridRows.Sort(CompareGridRows);
			for(int i=0;i<listGridRows.Count;i++) {
				gridMain.Rows.Add(VerifyRowToODGridRow(listGridRows[i],false));
			}
			gridMain.EndUpdate();
			gridMain.SetSelected(_selectedRowVerifyGrid,true);
		}

		private void gridMain_CellClick(object sender,UI.ODGridClickEventArgs e) {
			_gridRowSelected=((InsVerifyGridObject)gridMain.Rows[e.Row].Tag);
			if(_gridRowSelected.IsOnlyInsRow()) {
				butVerifyPlan.Enabled=true;
				butVerifyPat.Enabled=false;
			}
			else if(_gridRowSelected.IsOnlyPatRow()){
				butVerifyPlan.Enabled=false;
				butVerifyPat.Enabled=true;
			}
			else {
				butVerifyPlan.Enabled=true;
				butVerifyPat.Enabled=true;
			}
			FillDisplayInfo(_gridRowSelected);
		}
		#endregion
		
		#region Verify Logic

		private void butVerifyPlan_Click(object sender,EventArgs e) {
			OnVerify(PlanToVerify.InsuranceBenefits);
		}

		private void butVerifyPat_Click(object sender,EventArgs e) {
			OnVerify(PlanToVerify.PatientEligibility);
		}

		private void OnVerify(PlanToVerify planToVerifyEnum) {
			if(gridMain.SelectedIndices.Length<1) {
				MsgBox.Show(this,"Please select an insurance to verify.");
				return;
			}
			InsVerifyGridObject selectedRowObject=((InsVerifyGridObject)gridMain.Rows[gridMain.GetSelectedIndex()].Tag);
			if((planToVerifyEnum==PlanToVerify.Both && !selectedRowObject.IsPatAndInsRow()) 
				|| (planToVerifyEnum==PlanToVerify.PatientEligibility && selectedRowObject.PatInsVerify==null) 
				|| (planToVerifyEnum==PlanToVerify.InsuranceBenefits && selectedRowObject.PlanInsVerify==null)) 
			{
				//This will only happen if somehow the selected grid row differed from the passed in PlanToVerify.
				MsgBox.Show(this,"Something went wrong with your selection.  Click on the refresh button and try again.");
				return;
			}
			string verifyType="";
			if(planToVerifyEnum==PlanToVerify.Both) {
				verifyType="patient eligibility and insurance benefits";
			}
			else if(planToVerifyEnum==PlanToVerify.PatientEligibility) {
				verifyType="patient eligibility";
			}
			else if(planToVerifyEnum==PlanToVerify.InsuranceBenefits) {
				verifyType="insurance benefits";
			}
			if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Are you sure you want to verify the selected "+verifyType+"?")) {
				return;
			}
			if(planToVerifyEnum==PlanToVerify.Both || planToVerifyEnum==PlanToVerify.PatientEligibility) {
				selectedRowObject.PatInsVerify.DateLastVerified=DateTime.Today;
				InsVerifyHists.InsertFromInsVerify(selectedRowObject.PatInsVerify);
			}
			if(planToVerifyEnum==PlanToVerify.Both || planToVerifyEnum==PlanToVerify.InsuranceBenefits) {
				selectedRowObject.PlanInsVerify.DateLastVerified=DateTime.Today;
				InsVerifyHists.InsertFromInsVerify(selectedRowObject.PlanInsVerify);
			}
			FillDisplayInfo(null);
			FillGrids();
		}

		private void gridMain_MouseUp(object sender,MouseEventArgs e) {
			if(gridMain.GetSelectedIndex()==-1) {
				return;
			}
			if(e.Button==MouseButtons.Right && gridMain.SelectedIndices.Length>0) {
				_menuRightClick.Items.Clear();
				_menuRightClick.Items.Add(Lan.g(this,"Go to Patient"),null,new EventHandler(gridMainRight_click));
				string verifyDescription=Lan.g(this,"Go to Insurance Plan");
				if(_gridRowSelected.PatInsVerify!=null) {
					verifyDescription=Lan.g(this,"Go to Patient Plan");
				}
				_menuRightClick.Items.Add(verifyDescription,null,new EventHandler(gridMainRight_click));
				ToolStripMenuItem assignUserToolItem=new ToolStripMenuItem(Lan.g(this,"Assign to User"));
				foreach(Userod user in _listUsersInRegion) {
					ToolStripMenuItem assignUserDropDownCur=new ToolStripMenuItem(user.UserName);
					assignUserDropDownCur.Tag=user;
					assignUserDropDownCur.Click+=new EventHandler(assignUserToolItemDropDown_Click);
					assignUserToolItem.DropDownItems.Add(assignUserDropDownCur);
				}
				_menuRightClick.Items.Add(assignUserToolItem);
				ToolStripMenuItem verifyStatusToolItem=new ToolStripMenuItem(Lan.g(this,"Set Verify Status to"));
				foreach(Def status in _listVerifyStatuses) {
					ToolStripMenuItem verifyStatusDropDownCur=new ToolStripMenuItem(status.ItemName);
					verifyStatusDropDownCur.Tag=status;
					verifyStatusDropDownCur.Click+=new EventHandler(verifyStatusToolItemDropDown_Click);
					verifyStatusToolItem.DropDownItems.Add(verifyStatusDropDownCur);
				}
				_menuRightClick.Items.Add(verifyStatusToolItem);
				if(_gridRowSelected.IsPatAndInsRow()) {
					_menuRightClick.Items.Add(Lan.g(this,"Verify Patient Eligibility"),null,new EventHandler(gridMainRight_click));//Number 3 in gridMainRight_click
					_menuRightClick.Items.Add(Lan.g(this,"Verify Insurance Benefits"),null,new EventHandler(gridMainRight_click));//Number 4 in gridMainRight_click
					_menuRightClick.Items.Add(Lan.g(this,"Verify Both"),null,new EventHandler(gridMainRight_click));//Number 5 in gridMainRight_click
				}
				else if(_gridRowSelected.IsOnlyPatRow()) {
					_menuRightClick.Items.Add(Lan.g(this,"Verify Patient Eligibility"),null,new EventHandler(gridMainRight_click));//Number 3 in gridMainRight_click
				}
				else if(_gridRowSelected.IsOnlyInsRow()) {
					_menuRightClick.Items.Add(Lan.g(this,"Verify Insurance Benefits"),null,new EventHandler(gridMainRight_click));//Number 3 in gridMainRight_click
				}
				_menuRightClick.Show(gridMain,new Point(e.X,e.Y));
			}
		}

		private void gridMain_CellDoubleClick(object sender,ODGridClickEventArgs e) {
			OnOpenInsPlan();
		}

		private void OnOpenInsPlan() {	
			FormInsPlan FormIP;
			if(_gridRowSelected.PatInsVerify!=null) {
				PatPlan pp=PatPlans.GetByPatPlanNum(_gridRowSelected.PatInsVerify.FKey);
				InsSub insSub=InsSubs.GetOne(pp.InsSubNum);
				InsPlan ip=InsPlans.GetPlan(insSub.PlanNum,new List<InsPlan>());
				FormIP=new FormInsPlan(ip,pp,insSub);
				FormIP.ShowDialog();
				if(FormIP.DialogResult==DialogResult.OK) {
					FillGrids();
				}
			}
			else if(_gridRowSelected.PlanInsVerify!=null) {
				FormIP=new FormInsPlan(InsPlans.GetPlan(_gridRowSelected.PlanInsVerify.FKey,new List<InsPlan>()),null,null);
				FormIP.ShowDialog();
				if(FormIP.DialogResult==DialogResult.OK) {
					FillGrids();
				}
			}
		}

		private void gridMainRight_click(object sender,System.EventArgs e) {
			switch(_menuRightClick.Items.IndexOf((ToolStripMenuItem)sender)) {
				case 0:
					GotoModule.GotoFamily(_gridRowSelected.GetPatNum());
					break;
				case 1:
					OnOpenInsPlan();
					break;
				case 2:
					//No need for action on Assign click
					break;
				case 3:
					break;
				case 4:
					if(_gridRowSelected.IsOnlyInsRow()) {
						OnVerify(PlanToVerify.InsuranceBenefits);
					}
					else {
						OnVerify(PlanToVerify.PatientEligibility);//If both or only pat, then 3 will be patient eligibility verification
					}
					break;
				case 5:
					OnVerify(PlanToVerify.InsuranceBenefits);//This will only be visible if selecting a row with both ins and pat
					break;
				case 6:
					OnVerify(PlanToVerify.Both);//This will only be visible if selecting a row with both ins and pat
					break;
			}
		}
		#endregion

		#region Grid Assign
		private void FillGridAssign() {
			_selectedRowAssignGrid=gridAssign.GetSelectedIndex();
			gridAssign.BeginUpdate();
			gridAssign.Columns.Clear();
			ODGridColumn col;
			col=new ODGridColumn(Lans.g(this,"Type"),45);
			gridAssign.Columns.Add(col);
			if(PrefC.HasClinicsEnabled) {
				col=new ODGridColumn(Lans.g(this,"Clinic"),90);
				gridAssign.Columns.Add(col);
			}
			col=new ODGridColumn(Lans.g(this,"Patient"),120);
			gridAssign.Columns.Add(col);
			col=new ODGridColumn(Lans.g(this,"Appt Date Time"),130,GridSortingStrategy.DateParse);
			col.TextAlign=HorizontalAlignment.Center;
			gridAssign.Columns.Add(col);
			col=new ODGridColumn(Lans.g(this,"Carrier"),160);
			gridAssign.Columns.Add(col);
			col=new ODGridColumn(Lans.g(this,"Last Verified"),90,GridSortingStrategy.DateParse);
			col.TextAlign=HorizontalAlignment.Center;
			gridAssign.Columns.Add(col);
			col=new ODGridColumn(Lans.g(this,"Status"),110);
			gridAssign.Columns.Add(col);
			col=new ODGridColumn(Lans.g(this,"Assigned to"),0);
			gridAssign.Columns.Add(col);
			gridAssign.Rows.Clear();
			List<InsVerifyGridRow> listGridRows=GetRowsForGrid(true);
			listGridRows.Sort(CompareGridRows);
			for(int i=0;i<listGridRows.Count;i++) {
				gridAssign.Rows.Add(VerifyRowToODGridRow(listGridRows[i],true));
			}
			gridAssign.EndUpdate();
			gridAssign.SetSelected(_selectedRowAssignGrid,true);
		}

		private List<InsVerifyGridObject> GetSelectedInsVerifyList() {
			List<InsVerifyGridObject> selectedGridObjectRows=new List<InsVerifyGridObject>();
			for(int i=0;i<gridAssign.SelectedIndices.Length;i++) {
				selectedGridObjectRows.Add(((InsVerifyGridObject)gridAssign.Rows[gridAssign.SelectedIndices[i]].Tag));
			}
			return selectedGridObjectRows;
		}
		#endregion

		#region Assigning Logic
		private void butAssignUser_Click(object sender,EventArgs e) {
			if(gridAssign.SelectedIndices.Length==0) {
				MsgBox.Show(this,"Please select an insurance to assign.");
				return;
			}
			if(_assignUserNum==0) {
				if(!MsgBox.Show(this,MsgBoxButtons.YesNo,"Are you sure you want to unassign the selected plan?")) {
					return;
				}
			}
			List<InsVerifyGridObject> listRowsSelected=GetSelectedInsVerifyList();
			foreach(InsVerifyGridObject gridRowObject in listRowsSelected) {
				if(gridRowObject.PatInsVerify!=null) {
					gridRowObject.PatInsVerify.UserNum=_assignUserNum;
					gridRowObject.PatInsVerify.Note=textInsVerifyNote.Text;
					gridRowObject.PatInsVerify.DateLastAssigned=DateTime.Today;
					InsVerifies.Update(gridRowObject.PatInsVerify);
				}
				if(gridRowObject.PlanInsVerify!=null) {
					gridRowObject.PlanInsVerify.UserNum=_assignUserNum;
					gridRowObject.PlanInsVerify.Note=textInsVerifyNote.Text;
					gridRowObject.PlanInsVerify.DateLastAssigned=DateTime.Today;
					InsVerifies.Update(gridRowObject.PlanInsVerify);
				}
			}
			FillGrids();
		}

		private void gridAssign_MouseUp(object sender,MouseEventArgs e) {
			if(gridAssign.GetSelectedIndex()==-1) {
				return;
			}
			if(e.Button==MouseButtons.Right && gridAssign.SelectedIndices.Length>0) {
				_menuRightClick.Items.Clear();
				ToolStripMenuItem assignUserToolItem=new ToolStripMenuItem(Lan.g(this,"Assign to User"));
				foreach(Userod user in _listUsersInRegion) {
					ToolStripMenuItem assignUserDropDownCur=new ToolStripMenuItem(user.UserName);
					assignUserDropDownCur.Tag=user;
					assignUserDropDownCur.Click+=new EventHandler(assignUserToolItemDropDown_Click);
					assignUserToolItem.DropDownItems.Add(assignUserDropDownCur);
				}
				_menuRightClick.Items.Add(assignUserToolItem);
				ToolStripMenuItem verifyStatusToolItem=new ToolStripMenuItem(Lan.g(this,"Set Verify Status to"));
				foreach(Def status in _listVerifyStatuses) {
					ToolStripMenuItem verifyStatusDropDownCur=new ToolStripMenuItem(status.ItemName);
					verifyStatusDropDownCur.Tag=status;
					verifyStatusDropDownCur.Click+=new EventHandler(verifyStatusToolItemDropDown_Click);
					verifyStatusToolItem.DropDownItems.Add(verifyStatusDropDownCur);
				}
				_menuRightClick.Items.Add(verifyStatusToolItem);
				_menuRightClick.Show(gridAssign,new Point(e.X,e.Y));
			}
		}

		private void gridAssignRight_click(object sender,System.EventArgs e) {
			switch(_menuRightClick.Items.IndexOf((ToolStripMenuItem)sender)) {
				case 0:
					if(_assignUserNum==0) {
						if(!MsgBox.Show(this,MsgBoxButtons.YesNo,"Are you sure you want to unassign the selected plan?")) {
							return;
						}
					}
					List<InsVerifyGridObject> listRowsSelected=GetSelectedInsVerifyList();
					foreach(InsVerifyGridObject gridRowObject in listRowsSelected) {
						if(gridRowObject.PatInsVerify!=null) {
							gridRowObject.PatInsVerify.UserNum=_assignUserNum;
							gridRowObject.PatInsVerify.DateLastAssigned=DateTime.Today;
							InsVerifies.Update(gridRowObject.PatInsVerify);
						}
						if(gridRowObject.PlanInsVerify!=null) {
							gridRowObject.PlanInsVerify.UserNum=_assignUserNum;
							gridRowObject.PlanInsVerify.DateLastAssigned=DateTime.Today;
							InsVerifies.Update(gridRowObject.PlanInsVerify);
						}
					}
					break;
				case 1:
					//Not clickable due to being a dropdown menu.
					break;
			}
			FillGrids();
		}

		private void butAssignUserPick_Click(object sender,EventArgs e) {
			PickUser(true);
		}

		private void comboSetVerifyStatus_SelectionChangeCommitted(object sender,EventArgs e) {
			if(comboSetVerifyStatus.SelectedIndex<1) {
				_defNumVerifyStatusAssign=0;
				comboSetVerifyStatus.Text="none";
			}
			else {
				_defNumVerifyStatusAssign=_listVerifyStatuses[comboSetVerifyStatus.SelectedIndex-1].DefNum;
				comboSetVerifyStatus.Text=_listVerifyStatuses[comboSetVerifyStatus.SelectedIndex-1].ItemName;
			}
			if(gridMain.GetSelectedIndex()!=-1) {
				SetStatus(_defNumVerifyStatusAssign,true);
			}
			FillGrids();
		}
		#endregion

		#region Grid Filters
		private void butVerifyUserPick_Click(object sender,EventArgs e) {
			PickUser(false);
			FillGrids();
		}

		private void comboFilterVerifyStatus_SelectionChangeCommitted(object sender,EventArgs e) {
			if(comboFilterVerifyStatus.SelectedIndex<1) {
				_defNumVerifyStatusFilter=0;
			}
			else {
				_defNumVerifyStatusFilter=_listVerifyStatuses[comboFilterVerifyStatus.SelectedIndex-1].DefNum;
			}
			FillGrids();
		}

		private void comboVerifyClinic_SelectionChangeCommitted(object sender,EventArgs e) {
			if(Security.CurUser.ClinicIsRestricted) {
				if(_defNumVerifyRegionsFilter<1) {
					_clinicNumVerifyClinicsFilter=_listClinicsDb[comboVerifyClinics.SelectedIndex].ClinicNum;
				}
				else if(_defNumVerifyRegionsFilter>=1) {
					_clinicNumVerifyClinicsFilter=_listClinicsFiltered[comboVerifyClinics.SelectedIndex].ClinicNum;
				}
			}
			else {
				if(comboVerifyClinics.SelectedIndex<1) {
					_clinicNumVerifyClinicsFilter=-1;
				}
				else if(_defNumVerifyRegionsFilter<1) {
					if(comboVerifyClinics.SelectedIndex==comboVerifyClinics.Items.Count-1) {
						_clinicNumVerifyClinicsFilter=0;
					}
					else {
						_clinicNumVerifyClinicsFilter=_listClinicsDb[comboVerifyClinics.SelectedIndex-1].ClinicNum;
					}
				}
				else if(_defNumVerifyRegionsFilter>=1) {
					_clinicNumVerifyClinicsFilter=_listClinicsFiltered[comboVerifyClinics.SelectedIndex-1].ClinicNum;
				}
			}
			FillGrids();
		}

		private void comboVerifyRegions_SelectionChangeCommitted(object sender,EventArgs e) {
			if(comboVerifyRegions.SelectedIndex<1) {
				_defNumVerifyRegionsFilter=-1;
			}
			else {
				_defNumVerifyRegionsFilter=DefC.Short[(int)DefCat.Regions][comboVerifyRegions.SelectedIndex-1].DefNum;
			}
			FillGrids();
		}

		private void comboVerifyUser_SelectionChangeCommitted(object sender,EventArgs e) {
			if(comboVerifyUser.SelectedIndex==1) {//Selected "Unassigned"
				_verifyUserNum=0;
			}
			else if(comboVerifyUser.SelectedIndex<1) {//Selected "All Users" or selected index is invalid
				_verifyUserNum=-1;
			}
			else {//Selected a real User.
				_verifyUserNum=_listUsersInRegionWithAssignedIns[comboVerifyUser.SelectedIndex-2].UserNum;
			}
			FillGrids();
		}

		private void textVerifyCarrier_TextChanged(object sender,EventArgs e) {
			if(!_hasLoaded) {
				return;
			}
			timerRefresh.Stop();
			timerRefresh.Start();
		}

		private void textAppointmentScheduledDays_TextChanged(object sender,EventArgs e) {
			if(!_hasLoaded) {
				return;
			}
			timerRefresh.Stop();
			timerRefresh.Start();
		}

		private void textInsBenefitEligibilityDays_TextChanged(object sender,EventArgs e) {
			if(!_hasLoaded) {
				return;
			}
			timerRefresh.Stop();
			timerRefresh.Start();
		}

		private void textPatientEnrollmentDays_TextChanged(object sender,EventArgs e) {
			if(!_hasLoaded) {
				return;
			}
			timerRefresh.Stop();
			timerRefresh.Start();
		}

		private void timerRefresh_Tick(object sender,EventArgs e) {
			//This timer was set by textVerifyCarrier_TextChanged in order to prevent refreshing too frequently.
			timerRefresh.Stop();
			FillGrids();
		}
		#endregion

		private void SetStatus(long statusDefNum,bool isVerifyGrid) {
			string statusNote="";
			bool hasChanged=false;
			InputBox ib=new InputBox(Lan.g(this,"Add a status note:"));
			ib.setTitle(Lan.g(this,"Add Status Note"));
			ib.IsMultiline=true;
			if(!isVerifyGrid) {
				ib.textResult.Text=textInsVerifyNote.Text;
			}
			ib.ShowDialog();
			if(ib.DialogResult==DialogResult.OK) {
				statusNote=ib.textResult.Text;
				hasChanged=true;
			}
			if(isVerifyGrid) {
				if(_gridRowSelected.PatInsVerify!=null) {
					_gridRowSelected.PatInsVerify.DefNum=statusDefNum;
					if(hasChanged) {
						_gridRowSelected.PatInsVerify.Note=statusNote;
					}
					_gridRowSelected.PatInsVerify.DateLastAssigned=DateTime.Today;
					InsVerifies.Update(_gridRowSelected.PatInsVerify);
				}
				if(_gridRowSelected.PlanInsVerify!=null) {
					_gridRowSelected.PlanInsVerify.DefNum=statusDefNum;
					if(hasChanged) {
						_gridRowSelected.PlanInsVerify.Note=statusNote;
						textInsVerifyReadOnlyNote.Text=statusNote;
					}
					_gridRowSelected.PlanInsVerify.DateLastAssigned=DateTime.Today;
					InsVerifies.Update(_gridRowSelected.PlanInsVerify);
				}
			}
			else {
				List<InsVerifyGridObject> listRowsSelected=GetSelectedInsVerifyList();
				foreach(InsVerifyGridObject gridRowObject in listRowsSelected) {
					if(gridRowObject.PatInsVerify!=null) {
						gridRowObject.PatInsVerify.DefNum=statusDefNum;
						if(hasChanged) {
							gridRowObject.PatInsVerify.Note=statusNote;
						}
						gridRowObject.PatInsVerify.DateLastAssigned=DateTime.Today;
						InsVerifies.Update(gridRowObject.PatInsVerify);
					}
					if(gridRowObject.PlanInsVerify!=null) {
						gridRowObject.PlanInsVerify.DefNum=statusDefNum;
						if(hasChanged) {
							gridRowObject.PlanInsVerify.Note=statusNote;
						}
						gridRowObject.PlanInsVerify.DateLastAssigned=DateTime.Today;
						InsVerifies.Update(gridRowObject.PlanInsVerify);
					}
				}
			}
		}

		private void verifyStatusToolItemDropDown_Click(object sender, EventArgs e) {
			Def status=(Def)((ToolStripMenuItem)sender).Tag;
			if(tabControl1.SelectedTab==tabVerify) {
				SetStatus(status.DefNum,true);
			}
			if(tabControl1.SelectedTab==tabAssign) {
				SetStatus(status.DefNum,false);
			}
			FillGrids();
		}

		private void assignUserToolItemDropDown_Click(object sender, EventArgs e) {
			Userod user=(Userod)((ToolStripMenuItem)sender).Tag;
			if(tabControl1.SelectedTab==tabVerify) {
				if(_gridRowSelected.PatInsVerify!=null) {
					_gridRowSelected.PatInsVerify.UserNum=user.UserNum;
					_gridRowSelected.PatInsVerify.DateLastAssigned=DateTime.Today;
					InsVerifies.Update(_gridRowSelected.PatInsVerify);
				}
				if(_gridRowSelected.PlanInsVerify!=null) {
					_gridRowSelected.PlanInsVerify.UserNum=user.UserNum;
					_gridRowSelected.PlanInsVerify.DateLastAssigned=DateTime.Today;
					InsVerifies.Update(_gridRowSelected.PlanInsVerify);
				}
			}
			if(tabControl1.SelectedTab==tabAssign) {
				List<InsVerifyGridObject> listRowsSelected=GetSelectedInsVerifyList();
				foreach(InsVerifyGridObject gridRowObject in listRowsSelected) {
					if(gridRowObject.PatInsVerify!=null) {
						gridRowObject.PatInsVerify.UserNum=user.UserNum;
						gridRowObject.PatInsVerify.DateLastAssigned=DateTime.Today;
						InsVerifies.Update(gridRowObject.PatInsVerify);
					}
					if(gridRowObject.PlanInsVerify!=null) {
						gridRowObject.PlanInsVerify.UserNum=user.UserNum;
						gridRowObject.PlanInsVerify.DateLastAssigned=DateTime.Today;
						InsVerifies.Update(gridRowObject.PlanInsVerify);
					}
				}
			}
			FillGrids();
		}

		private void tabControl1_Selected(object sender,TabControlEventArgs e) {
			if(e.TabPage==tabAssign) {
				if(_verifyUserNum!=0) {
					_userNumVerifyGrid=_verifyUserNum;
					_verifyUserNum=0;//Set filter user to Unassigned when switching to Assign tab.=
				}
			}
			else if(e.TabPage==tabVerify) {
				if(_verifyUserNum==0) {
					_verifyUserNum=_userNumVerifyGrid;
				}
			}
			FillGrids();
		}

		private void butClose_Click(object sender,EventArgs e) {
			Close();
		}

		///<summary>This represents a row in either the Verification List Grid, or the Assignment Grid.  The assignment grid won't use the DateLastAssigned variable.</summary>
		private class InsVerifyGridRow {
			public string Type;
			public string Clinic;
			public string PatientName;
			public DateTime NextApptDate;
			public string CarrierName;
			public DateTime DateLastVerified;
			public string VerifyStatus;
			//DateLastAssigned isn't used if IsAssignGrid=true
			public DateTime DateLastAssigned;
			public string AssignedTo;
			public Object Tag;
			
			public bool IsAssignGrid;

			///<summary>An updated dictionary of status defs should be passed in.  
			///This is to avoid grabbing definitions cache from inside this nested class, which will be instanced in a loop.</summary>
			public InsVerifyGridRow(InsVerifyGridObject gridObj,Dictionary<long,Def> dictStatusDefs,List<Userod> listUsers,bool isAssignGrid) {
				if(gridObj==null) {
					return;
				}
				Type="";
				Clinic="";
				PatientName="";
				CarrierName="";
				VerifyStatus="";
				AssignedTo="";
				IsAssignGrid=isAssignGrid;
				if(gridObj.IsPatAndInsRow()) {//If showing a consolidated row, use the PatInsVerify information, since they are same anyways.
					Type="Pat/Ins";
					Clinic=gridObj.PatInsVerify.ClinicName;
					PatientName=gridObj.PatInsVerify.PatientName;
					NextApptDate=gridObj.PatInsVerify.AppointmentDateTime;
					CarrierName=gridObj.PatInsVerify.CarrierName;
					//Get the oldest DateLastVerified
					DateLastVerified=(gridObj.PatInsVerify.DateLastVerified<=gridObj.PlanInsVerify.DateLastVerified ? 
						gridObj.PatInsVerify.DateLastVerified : 
						gridObj.PlanInsVerify.DateLastVerified);
					if(dictStatusDefs.ContainsKey(gridObj.PatInsVerify.DefNum)) {
						VerifyStatus=dictStatusDefs[gridObj.PatInsVerify.DefNum].ItemName;
					}
					bool isPatLastAssignedNewer=gridObj.PatInsVerify.DateLastAssigned>=gridObj.PlanInsVerify.DateLastAssigned;
					//Get the most recent DateLastAssigned
					DateLastAssigned=(isPatLastAssignedNewer ? 
						gridObj.PatInsVerify.DateLastAssigned : 
						gridObj.PlanInsVerify.DateLastAssigned);
					if(isPatLastAssignedNewer) {
						Userod userCur=listUsers.FirstOrDefault(x => x.UserNum==gridObj.PatInsVerify.UserNum);
						if(userCur!=null) {
							AssignedTo=userCur.UserName;
						}
					}
					else {
						Userod userCur=listUsers.FirstOrDefault(x => x.UserNum==gridObj.PlanInsVerify.UserNum);
						if(userCur!=null) {
							AssignedTo=userCur.UserName;
						}
					}
					Tag=gridObj;
				}
				else if(gridObj.IsOnlyPatRow()) {
					Type="Pat";
					Clinic=gridObj.PatInsVerify.ClinicName;
					PatientName=gridObj.PatInsVerify.PatientName;
					NextApptDate=gridObj.PatInsVerify.AppointmentDateTime;
					CarrierName=gridObj.PatInsVerify.CarrierName;
					DateLastVerified=gridObj.PatInsVerify.DateLastVerified;
					if(dictStatusDefs.ContainsKey(gridObj.PatInsVerify.DefNum)) {
						VerifyStatus=dictStatusDefs[gridObj.PatInsVerify.DefNum].ItemName;
					}
					DateLastAssigned=gridObj.PatInsVerify.DateLastAssigned;
					Userod userCur=listUsers.FirstOrDefault(x => x.UserNum==gridObj.PatInsVerify.UserNum);
					if(userCur!=null) {
						AssignedTo=userCur.UserName;
					}
					Tag=gridObj;
				}
				else if(gridObj.IsOnlyInsRow()) {
					Type="Ins";
					Clinic=gridObj.PlanInsVerify.ClinicName;
					PatientName=gridObj.PlanInsVerify.PatientName;
					NextApptDate=gridObj.PlanInsVerify.AppointmentDateTime;
					CarrierName=gridObj.PlanInsVerify.CarrierName;
					DateLastVerified=gridObj.PlanInsVerify.DateLastVerified;
					if(dictStatusDefs.ContainsKey(gridObj.PlanInsVerify.DefNum)) {
						VerifyStatus=dictStatusDefs[gridObj.PlanInsVerify.DefNum].ItemName;
					}
					DateLastAssigned=gridObj.PlanInsVerify.DateLastAssigned;
					Userod userCur=listUsers.FirstOrDefault(x => x.UserNum==gridObj.PlanInsVerify.UserNum);
					if(userCur!=null) {
						AssignedTo=userCur.UserName;
					}
					Tag=gridObj;
				}
			}
		}

			public enum PlanToVerify {
				Both,
				PatientEligibility,
				InsuranceBenefits
			}
	}
}