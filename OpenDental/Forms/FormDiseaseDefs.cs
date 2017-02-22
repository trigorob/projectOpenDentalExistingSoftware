using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using OpenDental.UI;
using OpenDentBusiness;
using System.Linq;

namespace OpenDental{
	/// <summary>
	/// </summary>
	public class FormDiseaseDefs:ODForm {
		private OpenDental.UI.Button butClose;
		private OpenDental.UI.Button butAdd;
		private System.ComponentModel.IContainer components;
		private Label label1;
		private OpenDental.UI.Button butDown;
		private OpenDental.UI.Button butUp;
		private System.Windows.Forms.ToolTip toolTip1;
		private OpenDental.UI.Button butOK;
		///<summary>Set to true when user is using this to select a disease def. Currently used when adding Alerts to Rx.</summary>
		public bool IsSelectionMode;
		///<summary>Set to true when user is using FormMedical to allow multiple problems to be selected at once.</summary>
		public bool IsMultiSelect;
		///<summary>If IsSelectionMode, then after closing with OK, this will contain number.</summary>
		public long SelectedDiseaseDefNum;
		///<summary>If IsMultiSelect, then this will contain a list of numbers when closing with OK.</summary>
		public List<long> SelectedDiseaseDefNums;
		private ODGrid gridMain;
		private bool IsChanged;
		private CheckBox checkAlpha;
		///<summary>A complete list of disease defs including hidden.  Only used when not in selection mode (item orders can change).  It's main purpose is to keep track of the item order for the life of the window so that we do not have to make unnecessary update calls to the database every time the up and down buttons are clicked.</summary>
		private List<DiseaseDef> _listDiseaseDefs;
		///<summary>Stale deep copy of _listDiseaseDefs to use with sync.</summary>
		private List<DiseaseDef> _listDiseaseDefsOld;
		///<summary>List of all the DiseaseDefNums that cannot be deleted because they could be in use by other tables.</summary>
		private List<long> _listDiseaseDefsNumsNotDeletable;
		///<summary>List of messages returned by FormDiseaseDefEdit for creating log messages after syncing.  All messages in this list use ProblemEdit
		///permission.</summary>
		private List<string> _listSecurityLogMsgs;
		///<summary>List of selected indexes within _listDiseaseDefs.  Used to set SelectedDiseaseDefNum(s) since newly added DiseaseDefs will not have a
		///DiseaseDefNum until after syncing in FormClosing.</summary>
		private List<int> _listSelectedIndexes;

		///<summary></summary>
		public FormDiseaseDefs()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			Lan.F(this);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDiseaseDefs));
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.gridMain = new OpenDental.UI.ODGrid();
			this.butOK = new OpenDental.UI.Button();
			this.butDown = new OpenDental.UI.Button();
			this.butUp = new OpenDental.UI.Button();
			this.butClose = new OpenDental.UI.Button();
			this.butAdd = new OpenDental.UI.Button();
			this.checkAlpha = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(15, 11);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(335, 20);
			this.label1.TabIndex = 8;
			this.label1.Text = "This is a list of medical problems that patients might have. ";
			this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// gridMain
			// 
			this.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridMain.HScrollVisible = false;
			this.gridMain.Location = new System.Drawing.Point(18, 35);
			this.gridMain.Name = "gridMain";
			this.gridMain.ScrollValue = 0;
			this.gridMain.Size = new System.Drawing.Size(548, 628);
			this.gridMain.TabIndex = 16;
			this.gridMain.Title = null;
			this.gridMain.TranslationName = null;
			this.gridMain.CellDoubleClick += new OpenDental.UI.ODGridClickEventHandler(this.gridMain_CellDoubleClick);
			// 
			// butOK
			// 
			this.butOK.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Autosize = true;
			this.butOK.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butOK.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butOK.CornerRadius = 4F;
			this.butOK.Location = new System.Drawing.Point(584, 605);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(79, 26);
			this.butOK.TabIndex = 15;
			this.butOK.Text = "OK";
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// butDown
			// 
			this.butDown.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butDown.Autosize = true;
			this.butDown.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butDown.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butDown.CornerRadius = 4F;
			this.butDown.Image = global::OpenDental.Properties.Resources.down;
			this.butDown.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butDown.Location = new System.Drawing.Point(584, 464);
			this.butDown.Name = "butDown";
			this.butDown.Size = new System.Drawing.Size(79, 26);
			this.butDown.TabIndex = 14;
			this.butDown.Text = "&Down";
			this.butDown.Click += new System.EventHandler(this.butDown_Click);
			// 
			// butUp
			// 
			this.butUp.AdjustImageLocation = new System.Drawing.Point(0, 1);
			this.butUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butUp.Autosize = true;
			this.butUp.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butUp.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butUp.CornerRadius = 4F;
			this.butUp.Image = global::OpenDental.Properties.Resources.up;
			this.butUp.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butUp.Location = new System.Drawing.Point(584, 432);
			this.butUp.Name = "butUp";
			this.butUp.Size = new System.Drawing.Size(79, 26);
			this.butUp.TabIndex = 13;
			this.butUp.Text = "&Up";
			this.butUp.Click += new System.EventHandler(this.butUp_Click);
			// 
			// butClose
			// 
			this.butClose.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butClose.Autosize = true;
			this.butClose.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butClose.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butClose.CornerRadius = 4F;
			this.butClose.Location = new System.Drawing.Point(584, 637);
			this.butClose.Name = "butClose";
			this.butClose.Size = new System.Drawing.Size(79, 26);
			this.butClose.TabIndex = 1;
			this.butClose.Text = "Close";
			this.butClose.Click += new System.EventHandler(this.butClose_Click);
			// 
			// butAdd
			// 
			this.butAdd.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butAdd.Autosize = true;
			this.butAdd.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butAdd.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butAdd.CornerRadius = 4F;
			this.butAdd.Image = global::OpenDental.Properties.Resources.Add;
			this.butAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.butAdd.Location = new System.Drawing.Point(584, 265);
			this.butAdd.Name = "butAdd";
			this.butAdd.Size = new System.Drawing.Size(79, 26);
			this.butAdd.TabIndex = 7;
			this.butAdd.Text = "&Add";
			this.butAdd.Click += new System.EventHandler(this.butAdd_Click);
			// 
			// checkIns
			// 
			this.checkAlpha.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkAlpha.Location = new System.Drawing.Point(441,11);
			this.checkAlpha.Name = "checkIns";
			this.checkAlpha.Size = new System.Drawing.Size(222,18);
			this.checkAlpha.TabIndex = 18;
			this.checkAlpha.Text = "Keep problem list alphabetized";
			this.checkAlpha.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.checkAlpha.CheckedChanged += new System.EventHandler(this.checkAlpha_CheckedChanged);
			// 
			// FormDiseaseDefs
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(682, 675);
			this.Controls.Add(this.checkAlpha);
			this.Controls.Add(this.gridMain);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.butDown);
			this.Controls.Add(this.butUp);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.butClose);
			this.Controls.Add(this.butAdd);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormDiseaseDefs";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Problems";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormDiseaseDefs_FormClosing);
			this.Load += new System.EventHandler(this.FormDiseaseDefs_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void FormDiseaseDefs_Load(object sender, System.EventArgs e) {
			if(DiseaseDefs.FixItemOrders()) {
				DataValid.SetInvalid(InvalidType.Diseases);
			}
			_listSecurityLogMsgs=new List<string>();
			_listSelectedIndexes=new List<int>();
			if(IsSelectionMode){
				butClose.Text=Lan.g(this,"Cancel");
				butDown.Visible=false;
				butUp.Visible=false;
				checkAlpha.Visible=false;
				if(IsMultiSelect) {
					gridMain.SelectionMode=GridSelectionMode.MultiExtended;
				}
			}
			else{
				butOK.Visible=false;
			}
			checkAlpha.Checked=PrefC.GetBool(PrefName.ProblemListIsAlpabetical);
			if(PrefC.GetBool(PrefName.ProblemListIsAlpabetical)) {
				butUp.Visible=false;
				butDown.Visible=false;
			}
			if(IsSelectionMode) {//Do not show hidden.
				_listDiseaseDefs=DiseaseDefs.List.ToList();
			}
			else {
				_listDiseaseDefs=DiseaseDefs.ListLong.ToList();
			}
			_listDiseaseDefsOld=_listDiseaseDefs.Select(x => x.Copy()).ToList();
			_listDiseaseDefsNumsNotDeletable=DiseaseDefs.ValidateDeleteList(_listDiseaseDefs.Select(x => x.DiseaseDefNum).ToList());
			FillGrid();
		}

		private void FillGrid() {
			gridMain.BeginUpdate();
			gridMain.Columns.Clear();
			ODGridColumn col;
			col=new ODGridColumn(Lan.g(this,"ICD-9"),50);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lan.g(this,"ICD-10"),50);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lan.g(this,"SNOMED CT"),100);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lan.g(this,"Description"),250);
			gridMain.Columns.Add(col);
			if(!IsSelectionMode) {
				col=new ODGridColumn(Lan.g(this,"Hidden"),50,HorizontalAlignment.Center);
				gridMain.Columns.Add(col);
			}
			gridMain.Rows.Clear();
			ODGridRow row;
			foreach(DiseaseDef defCur in _listDiseaseDefs) {
				row=new ODGridRow();
				row.Cells.Add(defCur.ICD9Code);
				row.Cells.Add(defCur.Icd10Code);
				row.Cells.Add(defCur.SnomedCode);
				row.Cells.Add(defCur.DiseaseName);
				if(!IsSelectionMode) {
					row.Cells.Add(defCur.IsHidden?"X":"");
				}
				gridMain.Rows.Add(row);
			}
			gridMain.EndUpdate();
		}

		private void gridMain_CellDoubleClick(object sender,ODGridClickEventArgs e) {
			if(!IsSelectionMode && !Security.IsAuthorized(Permissions.ProblemEdit)) {//trying to double click to edit, but no permission.
				return;
			}
			if(gridMain.SelectedIndices.Length==0) {
				return;
			}
			if(IsSelectionMode) {
				if(IsMultiSelect) {
					_listSelectedIndexes=new List<int>() { gridMain.GetSelectedIndex() };
				}
				else {
					if(Snomeds.GetByCode(_listDiseaseDefs[gridMain.GetSelectedIndex()].SnomedCode)!=null) {
						_listSelectedIndexes=new List<int>() { gridMain.GetSelectedIndex() };
					}
					else {
						MsgBox.Show(this,"You have selected a problem with an unofficial SNOMED CT code.  Please correct the problem definition by going to "
							+"Lists | Problems and choosing an official code from the SNOMED CT list.");
						return;
					}
				}
				DialogResult=DialogResult.OK;
				return;
			}
			bool hasDelete=true;
			if(_listDiseaseDefsNumsNotDeletable.Contains(_listDiseaseDefs[gridMain.GetSelectedIndex()].DiseaseDefNum)) {
				hasDelete=false;
			}
			//everything below this point is _not_ selection mode.  User guaranteed to have permission for ProblemEdit.
			FormDiseaseDefEdit FormD=new FormDiseaseDefEdit(_listDiseaseDefs[gridMain.GetSelectedIndex()],hasDelete);
			FormD.ShowDialog();
			//Security log entry made inside that form.
			if(FormD.DialogResult!=DialogResult.OK) {
				return;
			}
			_listSecurityLogMsgs.Add(FormD.SecurityLogMsgText);
			if(FormD.DiseaseDefCur==null) {//User deleted the DiseaseDef.
				_listDiseaseDefs.RemoveAt(gridMain.GetSelectedIndex());
			}
			IsChanged=true;
			FillGrid();
		}

		private void butAdd_Click(object sender,System.EventArgs e) {
			if(!Security.IsAuthorized(Permissions.ProblemEdit)) {
				return;
			}
			DiseaseDef def=new DiseaseDef() { ItemOrder=DiseaseDefs.ListLong.Length };
			FormDiseaseDefEdit FormD=new FormDiseaseDefEdit(def,true);//also sets ItemOrder correctly if using alphabetical during the insert diseaseDef call.
			FormD.IsNew=true;
			FormD.ShowDialog();
			if(FormD.DialogResult!=DialogResult.OK) {
				return;
			}
			_listSecurityLogMsgs.Add(FormD.SecurityLogMsgText);
			//Need to invalidate cache for selection mode so that the new problem shows up.
			if(IsSelectionMode) {
				DataValid.SetInvalid(InvalidType.Diseases);
			}
			//Items are already in the right order in the DB, re-order in memory list to match
			_listDiseaseDefs.FindAll(x => x.ItemOrder>=def.ItemOrder).ForEach(x => x.ItemOrder++);
			_listDiseaseDefs.Add(def);
			_listDiseaseDefs.Sort(DiseaseDefs.SortItemOrder);
			IsChanged=true;
			FillGrid();
		}

		private void checkAlpha_CheckedChanged(object sender,EventArgs e) {
			if(PrefC.GetBool(PrefName.ProblemListIsAlpabetical)==checkAlpha.Checked) {
				return;//when loading form.
			}
			if(!checkAlpha.Checked) {
				butUp.Visible=true;
				butDown.Visible=true;
				Prefs.UpdateBool(PrefName.ProblemListIsAlpabetical,checkAlpha.Checked);
				return;//turned off alphabetizing
			}
			if(!MsgBox.Show(this,MsgBoxButtons.OKCancel,"Problems will be reordered, this cannot be undone.")) {
				checkAlpha.Checked=false;
				return;
			}
			Prefs.UpdateBool(PrefName.ProblemListIsAlpabetical,checkAlpha.Checked);
			butUp.Visible=false;
			butDown.Visible=false;
			_listDiseaseDefs.Sort(DiseaseDefs.SortAlphabetically);
			for(int i=0;i<_listDiseaseDefs.Count;i++) {
				_listDiseaseDefs[i].ItemOrder=i;
			}
			IsChanged=true;
			FillGrid();
		}

		private void butUp_Click(object sender,EventArgs e) {
			if(gridMain.SelectedIndices.Length==0) {
				MsgBox.Show(this,"Please select an item in the grid first.");
				return;
			}
			List<int> listSelectedIndexes=gridMain.SelectedIndices.ToList();
			if(listSelectedIndexes.First()==0) {
				return;
			}
			listSelectedIndexes.ForEach(x => _listDiseaseDefs.Reverse(x-1,2));
			for(int i=0;i<_listDiseaseDefs.Count;i++) {
				_listDiseaseDefs[i].ItemOrder=i;//change itemOrder to reflect order changes.
			}
			FillGrid();
			listSelectedIndexes.ForEach(x => gridMain.SetSelected(x-1,true));
			IsChanged=true;
		}

		private void butDown_Click(object sender,EventArgs e) {
			if(gridMain.SelectedIndices.Length==0) {
				MsgBox.Show(this,"Please select an item in the grid first.");
				return;
			}
			List<int> listSelectedIndexes=gridMain.SelectedIndices.ToList();
			if(listSelectedIndexes.Last()==_listDiseaseDefs.Count-1) {
				return;
			}
			listSelectedIndexes.Reverse<int>().ToList().ForEach(x => _listDiseaseDefs.Reverse(x,2));
			for(int i=0;i<_listDiseaseDefs.Count;i++) {
				_listDiseaseDefs[i].ItemOrder=i;//change itemOrder to reflect order changes.
			}
			FillGrid();
			listSelectedIndexes.ForEach(x => gridMain.SetSelected(x+1,true));
			IsChanged=true;
		}

		private void butOK_Click(object sender,EventArgs e) {
			//not even visible unless IsSelectionMode
			if(gridMain.SelectedIndices.Length==0){
				MsgBox.Show(this,"Please select an item first.");
				return;
			}
			if(IsMultiSelect) {
				_listSelectedIndexes=gridMain.SelectedIndices.ToList();
			}
			else if(IsSelectionMode) {
				if(Snomeds.GetByCode(_listDiseaseDefs[gridMain.GetSelectedIndex()].SnomedCode)!=null) {
					_listSelectedIndexes=new List<int>() { gridMain.GetSelectedIndex() };
				}
				else {
					MsgBox.Show(this,"You have selected a problem containing an invalid SNOMED CT.");
					return;
				}
			}
			else {
				_listSelectedIndexes=new List<int>() { gridMain.GetSelectedIndex() };
			}
			DialogResult=DialogResult.OK;
		}

		private void butClose_Click(object sender, System.EventArgs e) {
			DialogResult=DialogResult.Cancel;
		}

		private void FormDiseaseDefs_FormClosing(object sender,FormClosingEventArgs e) {
			if(IsChanged) {
				DiseaseDefs.Sync(_listDiseaseDefs,_listDiseaseDefsOld);//Update if anything has changed, even in selection mode.
				//old securitylog pattern pasted from FormDiseaseDefEdit
				_listSecurityLogMsgs.FindAll(x => !string.IsNullOrEmpty(x)).ForEach(x => SecurityLogs.MakeLogEntry(Permissions.ProblemEdit,0,x));
				DataValid.SetInvalid(InvalidType.Diseases);//refreshes cache
				if(IsSelectionMode) {//Do not show hidden.
					_listDiseaseDefs=DiseaseDefs.List.ToList();//uses newly refreshed cache if changes were made
				}
				else {
					_listDiseaseDefs=DiseaseDefs.ListLong.ToList();//uses newly refreshed cache if changes were made
				}
			}
			if(IsMultiSelect) {//set entire selected list and close form
				SelectedDiseaseDefNums=_listSelectedIndexes.Select(x => _listDiseaseDefs[x].DiseaseDefNum).ToList();
			}
			else if(_listSelectedIndexes.Count>0) {//set selected num and close form
				SelectedDiseaseDefNum=_listDiseaseDefs[_listSelectedIndexes.First()].DiseaseDefNum;
			}
		}

		

		

		

		

		

		

		

		


	}
}



























