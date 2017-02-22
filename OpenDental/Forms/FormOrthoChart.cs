using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental {
	public partial class FormOrthoChart:ODForm {
		private PatField[] _arrayPatientFields;
		private List<DisplayField> _listOrthDisplayFields;
		///<summary>A stale copy of all ortho charts associated to the selected patient.  Filled on load.
		///This list will often differ from the database the longer that the window is left open and changes are made (sigs invalidating, etc).</summary>
		private List<OrthoChart> _listOrthoCharts;
		private Patient _patCur;
		private List<string> _listDisplayFieldNames;
		///<summary>Set to true if any data changed within the grid.</summary>
		private bool _hasChanged;
		///<summary>Each row in this table has a date as the first cell.  This table will contain every column from every tab always.
		///Only select display fields will be displayed to the user based on whatever tab is currently selected.
		///There will be additional rows that are not yet in the db.  Each blank cell will be an empty string.  
		///It will also store changes made by the user prior to closing the form.
		///When the form is closed, this table will be compared with the original listOrthoCharts and a synch process will take place to save to db.
		///An empty string in a cell will result in no db row or a deletion of existing db row.</summary>
		private DataTable _tableOrtho;
		///<summary>True if there are any ortho display fields with the internal name of "Signature"</summary>
		private bool _showSigBox;
		///<summary>Keeps track of the column index of the Signature field if one is present.
		///This column index represents the the index of the grid that is displayed to the user, not the index within _tableOrtho.</summary>
		private int _sigColIdx=-1;
		///<summary>Keeps track of the column index within _tableOrtho of the Signature field if one is present.</summary>
		private int _sigTableOrthoColIdx=-1;
		///<summary>The index of the previously selected ortho chart row.  This is used to help save data.</summary>
		private int _prevRow;
		private bool _topazNeedsSaved;
		private List<DateTime> _listDates;

		public FormOrthoChart(Patient patCur) {
			_patCur=patCur;
			_prevRow=-1;
			InitializeComponent();
			Lan.F(this);
		}

		private void FormOrthoChart_Load(object sender,EventArgs e) {
			_listOrthoCharts=OrthoCharts.GetAllForPatient(_patCur.PatNum);
			FillTabs();
			FillDataTable();
			FillGrid();
			FillGridPat();
		}

		///<summary>Sets the title of the form to the TabName of the first tab in the cache and then refreshes the current tabs.</summary>
		private void FillTabs() {
			//Set the title of this form to the first tab in the list.  The button to launch the Ortho Chart from the Chart module will be the same.
			Text=OrthoChartTabs.Listt[0].TabName;//It is considered database corruption if this fails.
			OrthoChartTab orthoChartTabSelected=null;
			if(tabControl.SelectedIndex >= 0) {
				orthoChartTabSelected=(OrthoChartTab)tabControl.TabPages[tabControl.SelectedIndex].Tag;
			}
			tabControl.TabPages.Clear();
			for(int i=0;i<OrthoChartTabs.Listt.Count;i++) {
				TabPage tabPage=new TabPage(OrthoChartTabs.Listt[i].TabName);
				tabPage.Tag=OrthoChartTabs.Listt[i];
				tabControl.TabPages.Add(tabPage);
				if(orthoChartTabSelected!=null && OrthoChartTabs.Listt[i].OrthoChartTabNum==orthoChartTabSelected.OrthoChartTabNum) {
					tabControl.SelectedIndex=i;
				}
			}
			if(tabControl.SelectedIndex==-1) {
				tabControl.SelectedIndex=0;
			}
		}

		private void FillDataTable() {
			//define the table----------------------------------------------------------------------------------------------------------
			_tableOrtho=new DataTable("OrthoChartForPatient");
			//define columns----------------------------------------------------------------------------------------------------------
			_listOrthDisplayFields=DisplayFields.GetForCategory(DisplayFieldCategory.OrthoChart);
			_tableOrtho.Columns.Add(Lan.g(this,"Date"),typeof(DateTime));
			_listDisplayFieldNames=new List<string>();
			foreach(DisplayField field in _listOrthDisplayFields) {
				if(field.InternalName=="Signature") {
					_sigTableOrthoColIdx=_tableOrtho.Columns.Count;
				}
				_tableOrtho.Columns.Add(field.Description);//Set the column header to the description of the display field.  Used in FillGrid().
				_listDisplayFieldNames.Add(field.Description);
			}
			//define rows------------------------------------------------------------------------------------------------------------
			_listDates=new List<DateTime>();
			//start adding dates starting with today's date
			_listDates.Add(DateTime.Today);
			foreach(OrthoChart orthoChart in _listOrthoCharts) {
				if(!_listDisplayFieldNames.Contains(orthoChart.FieldName)) {//skip rows not in display fields
					continue;
				}
				if(!_listDates.Contains(orthoChart.DateService)) {//add dates not already in date list
					_listDates.Add(orthoChart.DateService);
				}
			}
			_listDates.Sort();
			//We now have a complete list of dates necessary to show this patients ortho chart.
			//Add all blank cells to each row except for the date.
			DataRow row;
			//create and add row for each date in date showing
			for(int i=0;i<_listDates.Count;i++) {
				row=_tableOrtho.NewRow();
				row[0]=_listDates[i];
				for(int j=0;j<_listOrthDisplayFields.Count;j++) {
					row[j+1]="";//j+1 because first row is date field.
				}
				_tableOrtho.Rows.Add(row);
			}
		}

		///<summary>Clears the current grid and fills from datatable.  Do not call unless you have saved changes to database first.</summary>
		private void FillGrid() {
			if(tabControl.SelectedIndex==-1) {
				return;//This happens when the tab pages are cleared (updating the selected index).
			}
			Cursor=Cursors.WaitCursor;
			//Fill with data as necessary.
			foreach(OrthoChart orthoChart in _listOrthoCharts) {
				if(!_listDates.Contains(orthoChart.DateService)) {
					continue;
				}
				if(!_listDisplayFieldNames.Contains(orthoChart.FieldName)) {
					continue;
				}
				for(int j=0;j<_tableOrtho.Rows.Count;j++) {
					if(orthoChart.DateService==(DateTime)_tableOrtho.Rows[j][0]) {
						_tableOrtho.Rows[j][_listDisplayFieldNames.IndexOf(orthoChart.FieldName)+1]=orthoChart.FieldValue;
					}
				}
			}
			//Get all the corresponding fields from the OrthoChartTabLink table that are associated to the currently selected ortho tab.
			OrthoChartTab orthoChartTab=OrthoChartTabs.Listt[tabControl.SelectedIndex];
			List<DisplayField> listSelectedTabDisplayFields=OrthoChartTabLinks.List
				.FindAll(x => x.OrthoChartTabNum==orthoChartTab.OrthoChartTabNum)//Determines the number of items that will be returned
				.OrderBy(x => x.ItemOrder)//Each tab is ordered based on the ortho tab link entry
				.Select(x => _listOrthDisplayFields.FirstOrDefault(y => y.DisplayFieldNum==x.DisplayFieldNum))//Project all corresponding display fields in order
				.Where(x => x!=null)//Can happen when there is an OrthoChartTabLink in the database pointing to an invalid display field.
				.ToList();//Casts the projection to a list of display fields
			_sigColIdx=-1;//Clear out the signature column index cause it will most likely change or disappear (switching tabs)
			int gridMainScrollValue=gridMain.ScrollValue;
			gridMain.BeginUpdate();
			//Set the title of the grid to the title of the currently selected ortho chart tab.  This is so that medical users don't see Ortho Chart.
			gridMain.Title=orthoChartTab.TabName;
			gridMain.Columns.Clear();
			ODGridColumn col;
			//First column will always be the date.  gridMain_CellLeave() depends on this fact.
			col=new ODGridColumn(Lan.g(this,"Date"),70);
			gridMain.Columns.Add(col);
			foreach(DisplayField field in listSelectedTabDisplayFields) {
				if(!string.IsNullOrEmpty(field.PickList)) {
					List<string> listComboOptions=field.PickList.Split(new string[] { "\r\n" },StringSplitOptions.None).ToList();
					col=new ODGridColumn(field.Description,field.ColumnWidth,listComboOptions);
				}
				else {
					col=new ODGridColumn(field.Description,field.ColumnWidth,true);
					if(field.InternalName=="Signature") {
						_sigColIdx=gridMain.Columns.Count;
						col.TextAlign=HorizontalAlignment.Center;
						col.IsEditable=false;
					}
				}
				gridMain.Columns.Add(col);
			}
			gridMain.Rows.Clear();
			ODGridRow row;
			foreach(DataRow orthoRow in _tableOrtho.Rows) {
				row=new ODGridRow();
				//First column will always be the date.  gridMain_CellLeave() depends on this fact.
				DateTime tempDate=(DateTime)orthoRow[0];
				row.Cells.Add(tempDate.ToShortDateString());
				row.Tag=tempDate;
				foreach(DisplayField field in listSelectedTabDisplayFields) {
					string cellValue="";
					if(field.InternalName!="Signature") {
						//Find the index of the corresponding column in our table via the column title.
						cellValue=orthoRow[_tableOrtho.Columns.IndexOf(field.Description)].ToString();
					}
					if(!string.IsNullOrEmpty(field.PickList)) {
						List<string> listComboOptions=field.PickList.Split(new string[] { "\r\n" },StringSplitOptions.None).ToList();
						int selectedIndex=listComboOptions.FindIndex(x => x==cellValue);
						row.Cells.Add(cellValue,selectedIndex);
					}
					else {
						row.Cells.Add(cellValue);
					}
				}
				gridMain.Rows.Add(row);
			}
			gridMain.EndUpdate();
			if(gridMainScrollValue==0) {
				gridMain.ScrollToEnd();
			}
			else {
				gridMain.ScrollValue=gridMainScrollValue;
				gridMainScrollValue=0;
			}
			//Show the signature control if a signature display field is present on any tab.
			_showSigBox=_listOrthDisplayFields.Any(x => x.InternalName=="Signature");
			signatureBoxWrapper.Visible=_showSigBox;//Hide the signature box if this tab does not have the signature column present.
			if(_showSigBox) {
				for(int i=0;i<gridMain.Rows.Count;i++) {
					SetSignature(i,false);
				}
				signatureBoxWrapper.ClearSignature();
				signatureBoxWrapper.Enabled=false;//We don't want it to be enabled unless the user has clicked on a row.
				_prevRow=-1;
			}
			Cursor=Cursors.Default;
		}

		private void FillGridPat() {
			gridPat.BeginUpdate();
			gridPat.Columns.Clear();
			ODGridColumn col;
			col=new ODGridColumn("Field",150);
			gridPat.Columns.Add(col);
			col=new ODGridColumn("Value",200);
			gridPat.Columns.Add(col);
			gridPat.Rows.Clear();
			_arrayPatientFields=PatFields.Refresh(_patCur.PatNum);
			PatFieldDefs.RefreshCache();
			ODGridRow row;
			//define and fill rows in grid at the same time.
			for(int i=0;i<PatFieldDefs.ListShort.Count;i++) {
				row=new ODGridRow();
				row.Cells.Add(PatFieldDefs.ListShort[i].FieldName);
				for(int j=0;j<=_arrayPatientFields.Length;j++) {
					if(j==_arrayPatientFields.Length) {//no matches in the list
						row.Cells.Add("");
						break;
					}
					if(_arrayPatientFields[j].FieldName==PatFieldDefs.ListShort[i].FieldName) {
						if(PatFieldDefs.ListShort[i].FieldType==PatFieldType.Checkbox) {
							row.Cells.Add("X");
						}
						else if(PatFieldDefs.ListShort[i].FieldType==PatFieldType.Currency) {
							row.Cells.Add(PIn.Double(_arrayPatientFields[j].FieldValue).ToString("c"));
						}
						else {
							row.Cells.Add(_arrayPatientFields[j].FieldValue);
						}
						break;
					}
				}
				gridPat.Rows.Add(row);
			}
			gridPat.EndUpdate();
		}

		private void menuItemSetup_Click(object sender,EventArgs e) {
			FormDisplayFieldsOrthoChart form=new FormDisplayFieldsOrthoChart();
			if(form.ShowDialog()==DialogResult.OK) {
				FillTabs();
				FillDataTable();
				FillGrid();
			}
		}

		private void tabControl_TabIndexChanged(object sender,EventArgs e) {
			FillGrid();
		}

		private void gridMain_CellTextChanged(object sender,EventArgs e) {
			DateTime orthoDate=PIn.Date(_tableOrtho.Rows[gridMain.SelectedCell.Y][0].ToString());
			if(!Security.IsAuthorized(Permissions.OrthoChartEdit,orthoDate,true)) {
				return;
			}
			_hasChanged=true;
			if(_showSigBox) {
				signatureBoxWrapper.ClearSignature();
			}
		}

		private void signatureBoxWrapper_ClearSignatureClicked(object sender,EventArgs e) {
			if(gridMain.SelectedCell.Y==-1) {
				return;
			}			
			if(OrthoSignature.GetSigString(_tableOrtho.Rows[gridMain.SelectedCell.Y][_sigTableOrthoColIdx].ToString())!="") {
				_hasChanged=true;
			}
			_tableOrtho.Rows[gridMain.SelectedCell.Y][_sigTableOrthoColIdx]="";
			DateTime orthoDate=PIn.Date(_tableOrtho.Rows[gridMain.SelectedCell.Y][0].ToString());//First column will always be the date.
			string sigColumnName=_listOrthDisplayFields.FirstOrDefault(x => x.InternalName=="Signature").Description;
			if(_listOrthoCharts.Any(x => x.DateService==orthoDate && x.FieldName==sigColumnName)) {
				_listOrthoCharts.First(x => x.DateService==orthoDate && x.FieldName==sigColumnName).FieldValue="";
			}
			SetSignature(gridMain.SelectedCell.Y);
			_prevRow=gridMain.SelectedCell.Y;
		}

		private void signatureBoxWrapper_SignTopazClicked(object sender,EventArgs e) {
			if(gridMain.SelectedCell.Y==-1) {
				return;
			}
			if(OrthoSignature.GetSigString(_tableOrtho.Rows[gridMain.SelectedCell.Y][_sigTableOrthoColIdx].ToString())!="") {
				_hasChanged=true;
			}
			_tableOrtho.Rows[gridMain.SelectedCell.Y][_sigTableOrthoColIdx]="";
			DateTime orthoDate=PIn.Date(_tableOrtho.Rows[gridMain.SelectedCell.Y][0].ToString());//First column will always be the date.
			string sigColumnName=_listOrthDisplayFields.FirstOrDefault(x => x.InternalName=="Signature").Description;
			if(_listOrthoCharts.Any(x => x.DateService==orthoDate && x.FieldName==sigColumnName)) {
				_listOrthoCharts.First(x => x.DateService==orthoDate && x.FieldName==sigColumnName).FieldValue="";
			}
			SetSignature(gridMain.SelectedCell.Y);
			_prevRow=gridMain.SelectedCell.Y;
			_topazNeedsSaved=true;
		}

		///<summary>Displays the signature for this row when clicking on the Date column or the Signature column. The gridMain_CellEnter event
		///does not fire when the column is not editable.</summary>
		private void gridMain_CellClick(object sender,ODGridClickEventArgs e) {
			if(e.Col!=0 && e.Col!=_sigColIdx) {//If not the date column or the signature column, return.
				return;
			}
			SaveAndSetSignatures(e.Row);
		}

		///<summary>This is necessary in addition to CellClick for when the user tabs or uses the arrow keys to enter a cell.</summary>
		private void gridMain_CellEnter(object sender,ODGridClickEventArgs e) {
			SaveAndSetSignatures(e.Row);
		}

		///<summary>Saves the signature to the data table if it hasn't been and displays the signature for this row.</summary>
		private void SaveAndSetSignatures(int currentRow) {
			if(!_showSigBox || currentRow<0) {
				return;
			}
			SetSignatureButtonVisibility(currentRow);
			//Try and save the previous row's signature if needed.
			SaveSignatureToTable(_prevRow);
			SetSignature(_prevRow);
			if(_topazNeedsSaved) {
				_topazNeedsSaved=false;
			}
			//Now that any previous sigs have been saved, display the current row's signature.
			SetSignature(currentRow);
			signatureBoxWrapper.Enabled=true;
			_prevRow=currentRow;
		}

		private void gridMain_CellLeave(object sender,ODGridClickEventArgs e) {
			//Get the date for the ortho chart that was just edited.
			DateTime orthoDate=PIn.Date(gridMain.Rows[e.Row].Cells[0].Text);//First column will always be the date.
			string oldText="";//If the selected cell is not in listOrthoCharts then it started out blank.  This will put it back to an empty string.
			for(int i=0;i<_listOrthoCharts.Count;i++) {
				if(_listOrthoCharts[i].DateService!=orthoDate) {
					continue;
				}
				if(_listOrthoCharts[i].FieldName!=gridMain.Columns[e.Col].Heading) {
					continue;
				}
				//This is the cell that the user was editing and it had an entry in the database.  Put it back to the way it was.
				oldText=_listOrthoCharts[i].FieldValue;
				break;
			}
			string newText=gridMain.Rows[e.Row].Cells[e.Col].Text;
			//Suppress the security message because it's crazy annoying if the user is simply clicking around in cells.  They might be copying a cell and not changing it.
			if(Security.IsAuthorized(Permissions.OrthoChartEdit,orthoDate,true)) {
				//If this is a new ortho chart field, add it to the list of ortho charts so that we can use it when hashing the signature.
				if(!_listOrthoCharts.Any(x=>x.DateService==orthoDate && x.FieldName==gridMain.Columns[e.Col].Heading)
					&& !string.IsNullOrEmpty(newText)) 
				{
					OrthoChart orthoChart=new OrthoChart();
					orthoChart.PatNum=_patCur.PatNum;
					orthoChart.DateService=orthoDate;
					orthoChart.FieldName=gridMain.Columns[e.Col].Heading;
					orthoChart.FieldValue=gridMain.Rows[e.Row].Cells[e.Col].Text;
					_listOrthoCharts.Add(orthoChart);
				}
				if(newText != oldText) {
					if(newText=="") {
						_listOrthoCharts.RemoveAll(x=>x.DateService==orthoDate && x.FieldName==gridMain.Columns[e.Col].Heading);
					}
					else { 
						_listOrthoCharts.FindAll(x=>x.DateService==orthoDate && x.FieldName==gridMain.Columns[e.Col].Heading)
							.ForEach(x=>x.FieldValue=newText);
					}
					_hasChanged=true;//They had permission and they made a change.
				}
				if(_showSigBox) {
					SaveSignatureToTable(e.Row);
					SetSignature(e.Row);
				}
				return;//The user has permission.  No need to waste time doing logic below.
			}
			//User is not authorized to edit this cell.  Check if they changed the old value and if they did, put it back to the way it was and warn them about security.
			if(newText!=oldText) {
				//The user actually changed the cell's value and we need to change it back and warn them that they don't have permission.
				gridMain.Rows[e.Row].Cells[e.Col].Text=oldText;
				gridMain.Invalidate();
				Security.IsAuthorized(Permissions.OrthoChartEdit,orthoDate);//This will pop up the message.
			}
		}

		///<summary>Displays the signature that is saved in the data table in the signature box. Colors the grid row green if the signature is valid, 
		///red if invalid, or white if blank. Puts "Valid" or "Invalid" in the grid's signature column.</summary>
		private void SetSignature(int gridRow,bool hasRefresh = true) {
			if(!_showSigBox || gridRow<0) {
				return;
			}
			DateTime orthoDate=PIn.Date(_tableOrtho.Rows[gridRow][0].ToString());
			OrthoSignature sig=new OrthoSignature(_tableOrtho.Rows[gridRow][_sigTableOrthoColIdx].ToString());
			if(sig.SigString=="") {
				signatureBoxWrapper.ClearSignature();
				gridMain.Rows[gridRow].ColorBackG=SystemColors.Window;
				//Empty out the signature column displaying to the user.
				if(_sigColIdx > 0) {//User might be vieweing a tab that does not have the signature column.  Greater than 0 because index 0 is a Date column.
					gridMain.Rows[gridRow].Cells[_sigColIdx].Text="";
				}
				if(hasRefresh) {
					gridMain.Refresh();
				}
				return;
			}
			//Get the "translated" name for the signature column.
			string sigColumnName=_listOrthDisplayFields.FirstOrDefault(x => x.InternalName=="Signature").Description;
			string keyData=OrthoCharts.GetKeyDataForSignatureHash(_patCur,_listOrthoCharts
				.FindAll(x => x.DateService==orthoDate && x.FieldName!=sigColumnName),orthoDate);
			signatureBoxWrapper.FillSignature(sig.IsTopaz,keyData,sig.SigString);
			if(signatureBoxWrapper.IsValid) {
				gridMain.Rows[gridRow].ColorBackG=Color.FromArgb(0,245,165);//A lighter version of Color.MediumSpringGreen
				if(_sigColIdx > 0) {//User might be vieweing a tab that does not have the signature column.  Greater than 0 because index 0 is a Date column.
					gridMain.Rows[gridRow].Cells[_sigColIdx].Text=Lan.g(this,"Valid");
				}
			}
			else {
				gridMain.Rows[gridRow].ColorBackG=Color.FromArgb(255,140,143);//A darker version of Color.LightPink
				if(_sigColIdx > 0) {//User might be vieweing a tab that does not have the signature column.  Greater than 0 because index 0 is a Date column.
					gridMain.Rows[gridRow].Cells[_sigColIdx].Text=Lan.g(this,"Invalid");
				}
			}
			if(hasRefresh) {
				gridMain.Refresh();
			}
		}

		///<summary>Removes the Sign Topaz button and the Clear Signature button from the signature box if the user does not have OrthoChartEdit
		///permissions for that date.</summary>
		private void SetSignatureButtonVisibility(int gridRow) {
			DateTime orthoDate=PIn.Date(gridMain.Rows[gridRow].Cells[0].Text);
			if(Security.IsAuthorized(Permissions.OrthoChartEdit,orthoDate,true)) {
				signatureBoxWrapper.SetButtonVisibility(true);
			}
			else {
				signatureBoxWrapper.SetButtonVisibility(false);
			}
		}

		///<summary>Saves the signature to the data table. The signature is hashed using the patient name, the date of service, and all ortho chart fields
		///(even the ones not showing).</summary>
		private void SaveSignatureToTable(int gridRow) {
			if(!_showSigBox || gridRow<0) {
				return;
			}
			DateTime orthoDate=PIn.Date(_tableOrtho.Rows[gridRow][0].ToString());//First column will always be the date.
			if(!Security.IsAuthorized(Permissions.OrthoChartEdit,orthoDate,true)) {
				return;
			}
			if(!signatureBoxWrapper.GetSigChanged() || !signatureBoxWrapper.IsValid) {
				return;
			}
			string keyData;
			//Get the "translated" name for the signature column.
			string sigColumnName=_listOrthDisplayFields.FirstOrDefault(x => x.InternalName=="Signature").Description;
			keyData=OrthoCharts.GetKeyDataForSignatureSaving(_patCur,_listOrthoCharts
				.FindAll(x => x.DateService==orthoDate && x.FieldName!=sigColumnName),orthoDate);
			OrthoSignature sig=new OrthoSignature();
			sig.IsTopaz=signatureBoxWrapper.GetSigIsTopaz();
			sig.SigString=signatureBoxWrapper.GetSignature(keyData);
			if(sig.IsTopaz && !_topazNeedsSaved) {
				return;
			}
			if(OrthoSignature.GetSigString(_tableOrtho.Rows[gridRow][_sigTableOrthoColIdx].ToString())!=sig.SigString) {
				_hasChanged=true;
				_tableOrtho.Rows[gridRow][_sigTableOrthoColIdx]=sig.ToString();
				//Update the in-memory list that we use to help fill the grid just in case the user invalidated the ortho row on a tab without the signature.
				if(_listOrthoCharts.Any(x => x.DateService==orthoDate && x.FieldName==sigColumnName)) {
					_listOrthoCharts.First(x => x.DateService==orthoDate && x.FieldName==sigColumnName).FieldValue=sig.ToString();
				}
			}
		}

		private void gridPat_CellDoubleClick(object sender,ODGridClickEventArgs e) {
			PatField field=PatFields.GetByName(PatFieldDefs.ListShort[e.Row].FieldName,_arrayPatientFields);
			if(field==null) {
				field=new PatField();
				field.PatNum=_patCur.PatNum;
				field.FieldName=PatFieldDefs.ListShort[e.Row].FieldName;
				if(PatFieldDefs.ListShort[e.Row].FieldType==PatFieldType.Text) {
					FormPatFieldEdit FormPF=new FormPatFieldEdit(field);
					FormPF.IsLaunchedFromOrtho=true;
					FormPF.IsNew=true;
					FormPF.ShowDialog();
				}
				if(PatFieldDefs.ListShort[e.Row].FieldType==PatFieldType.PickList) {
					FormPatFieldPickEdit FormPF=new FormPatFieldPickEdit(field);
					FormPF.IsNew=true;
					FormPF.ShowDialog();
				}
				if(PatFieldDefs.ListShort[e.Row].FieldType==PatFieldType.Date) {
					FormPatFieldDateEdit FormPF=new FormPatFieldDateEdit(field);
					FormPF.IsNew=true;
					FormPF.ShowDialog();
				}
				if(PatFieldDefs.ListShort[e.Row].FieldType==PatFieldType.Checkbox) {
					FormPatFieldCheckEdit FormPF=new FormPatFieldCheckEdit(field);
					FormPF.IsNew=true;
					FormPF.ShowDialog();
				}
				if(PatFieldDefs.ListShort[e.Row].FieldType==PatFieldType.Currency) {
					FormPatFieldCurrencyEdit FormPF=new FormPatFieldCurrencyEdit(field);
					FormPF.IsNew=true;
					FormPF.ShowDialog();
				}
			}
			else {
				if(PatFieldDefs.ListShort[e.Row].FieldType==PatFieldType.Text) {
					FormPatFieldEdit FormPF=new FormPatFieldEdit(field);
					FormPF.ShowDialog();
				}
				if(PatFieldDefs.ListShort[e.Row].FieldType==PatFieldType.PickList) {
					FormPatFieldPickEdit FormPF=new FormPatFieldPickEdit(field);
					FormPF.ShowDialog();
				}
				if(PatFieldDefs.ListShort[e.Row].FieldType==PatFieldType.Date) {
					FormPatFieldDateEdit FormPF=new FormPatFieldDateEdit(field);
					FormPF.ShowDialog();
				}
				if(PatFieldDefs.ListShort[e.Row].FieldType==PatFieldType.Checkbox) {
					FormPatFieldCheckEdit FormPF=new FormPatFieldCheckEdit(field);
					FormPF.ShowDialog();
				}
				if(PatFieldDefs.ListShort[e.Row].FieldType==PatFieldType.Currency) {
					FormPatFieldCurrencyEdit FormPF=new FormPatFieldCurrencyEdit(field);
					FormPF.ShowDialog();
				}
			}
			FillGridPat();
		}

		private void butAdd_Click(object sender,EventArgs e) {
			FormOrthoChartAddDate FormOCAD=new FormOrthoChartAddDate();
			FormOCAD.ShowDialog();
			if(FormOCAD.DialogResult!=DialogResult.OK) {
				return;
			}
			for(int i=0;i<_tableOrtho.Rows.Count;i++) {
				if(FormOCAD.SelectedDate==(DateTime)_tableOrtho.Rows[i][0]) {
					MsgBox.Show(this,"That date already exists.");
					return;
				}
			}
			SaveAndSetSignatures(gridMain.SelectedCell.Y);
			//listDatesAdditional.Add(FormOCAD.SelectedDate);
			//Move data from grid to table, add new date row to datatable, then fill grid from table.
			for(int i=0;i<gridMain.Rows.Count;i++) {
				_tableOrtho.Rows[i][0]=gridMain.Rows[i].Tag;//store date
				for(int j=1;j<gridMain.Columns.Count;j++) {
					if(j==_sigColIdx) {
						continue;//Already saved to table
					}
					_tableOrtho.Rows[i][_tableOrtho.Columns.IndexOf(gridMain.Columns[j].Heading)]=gridMain.Rows[i].Cells[j].Text;
				}
			}
			DataRow row;
			row=_tableOrtho.NewRow();
			row[0]=FormOCAD.SelectedDate;
			for(int i=0;i<_listOrthDisplayFields.Count;i++) {
				row[i+1]="";//j+1 because first row is date field.
			}
			//insert new row in proper ascending datetime order to dataTable
			for(int i=0;i<=_tableOrtho.Rows.Count;i++){
				if(i==_tableOrtho.Rows.Count){
					_tableOrtho.Rows.InsertAt(row,i);
					break;
				}
				if((DateTime)row[0]>(DateTime)_tableOrtho.Rows[i][0]){
					continue;
				}
				_tableOrtho.Rows.InsertAt(row,i);
				break;
			}
			_hasChanged=true;
			FillGrid();
		}

		private void butUseAutoNote_Click(object sender,EventArgs e) {
			if(gridMain.SelectedCell.X==-1 || gridMain.SelectedCell.X==0 || gridMain.SelectedCell.X==_sigColIdx) {
				MsgBox.Show(this,"Please select an editable Ortho Chart cell first.");
				return;
			}
			FormAutoNoteCompose FormA=new FormAutoNoteCompose();
			FormA.ShowDialog();
			if(FormA.DialogResult==DialogResult.OK) {
				Point selectedCell=new Point(gridMain.SelectedCell.X,gridMain.SelectedCell.Y);
				//Add text to current focused cell				
				gridMain.Rows[gridMain.SelectedCell.Y].Cells[gridMain.SelectedCell.X].Text+=FormA.CompletedNote;
				//Since the redrawing of the row height is dependent on the edit text box built into the ODGrid, we have to manually tell the grid to redraw.
				//This will essentially "refresh" the grid.  We do not want to call FillGrid() because that will lose data in other cells that have not been saved to datatable.
				if(_showSigBox && FormA.CompletedNote != "") {
					signatureBoxWrapper.ClearSignature();
					SaveSignatureToTable(gridMain.SelectedCell.Y);
					SetSignature(gridMain.SelectedCell.Y);
				}
				gridMain.BeginUpdate();
				gridMain.EndUpdate();
				gridMain.SetSelected(selectedCell);
				_hasChanged=true;
			}
		}

		private void butAudit_Click(object sender,EventArgs e) {
			FormAuditOrtho FormAO=new FormAuditOrtho();
			SecurityLog[] orthoChartLogs=SecurityLogs.Refresh(_patCur.PatNum,new List<Permissions> { Permissions.OrthoChartEdit },null);
			SecurityLog[] patientFieldLogs=SecurityLogs.Refresh(new DateTime(1,1,1),DateTime.Today,Permissions.PatientFieldEdit,_patCur.PatNum,0);
			SortedDictionary<DateTime,List<SecurityLog>> dictDatesOfServiceLogEntries=new SortedDictionary<DateTime,List<SecurityLog>>();
			//Add all dates from grid first, some may not have audit trail entries, but should be selectable from FormAO
			for(int i=0;i<gridMain.Rows.Count;i++) {
				DateTime dtCur=((DateTime)_tableOrtho.Rows[i][0]).Date;
				if(dictDatesOfServiceLogEntries.ContainsKey(dtCur)) {
					continue;
				}
				dictDatesOfServiceLogEntries.Add(dtCur,new List<SecurityLog>());
			}
			//Add Ortho Audit Trail Entries
			for(int i=0;i<orthoChartLogs.Length;i++) {
				DateTime dtCur=OrthoCharts.GetOrthoDateFromLog(orthoChartLogs[i]);
				if(!dictDatesOfServiceLogEntries.ContainsKey(dtCur)) {
					dictDatesOfServiceLogEntries.Add(dtCur,new List<SecurityLog>());
				}
				dictDatesOfServiceLogEntries[dtCur].Add(orthoChartLogs[i]);//add entry to existing list.
			}
			FormAO.DictDateOrthoLogs=dictDatesOfServiceLogEntries;
			FormAO.PatientFieldLogs.AddRange(patientFieldLogs);
			FormAO.ShowDialog();
		}

		private void butOK_Click(object sender,EventArgs e) {
			DialogResult=DialogResult.OK;
		}

		private void butCancel_Click(object sender,EventArgs e) {
			DialogResult=DialogResult.Cancel;
		}

		private void FormOrthoChart_FormClosing(object sender,FormClosingEventArgs e) {
			if(!_hasChanged) {
				return;
			}
			else if(DialogResult!=DialogResult.OK 
				&& _hasChanged && !MsgBox.Show(this,MsgBoxButtons.YesNo,"Unsaved changes will be lost. Would you like to save changes instead?")) 
			{
				return;
			}
			if(_showSigBox && gridMain.SelectedCell.Y != -1) {
				SaveSignatureToTable(gridMain.SelectedCell.Y);
			}
			//Save data from grid to table
			for(int i=0;i<gridMain.Rows.Count;i++) {
				_tableOrtho.Rows[i][0]=gridMain.Rows[i].Tag;//store date
				for(int j=0;j<gridMain.Columns.Count;j++) {
					if(j==_sigColIdx) {
						continue;//Skip signature field.
					}
					_tableOrtho.Rows[i][_tableOrtho.Columns.IndexOf(gridMain.Columns[j].Heading)]=gridMain.Rows[i].Cells[j].Text;
				}
			}
			if(_showSigBox) {
				//Don't save the signature if the user tried to sign an empty ortho chart.
				for(int i=0;i<_tableOrtho.Rows.Count;i++) {
					bool allEmpty=true;
					for(int j=1;j<_tableOrtho.Columns.Count;j++) {//skip col 0
						if(_tableOrtho.Rows[i][j].ToString() != "" && j != _sigTableOrthoColIdx) {
							allEmpty=false;
							break;
						}
					}
					if(allEmpty) {
						_tableOrtho.Rows.RemoveAt(i);
					}
				}
			}
			//Modified Sync pattern.  We cannot use the standard Sync pattern here because we have to perform logging when updating or deleting.
			#region Modified Sync Pattern
			List<OrthoChart> listDB=OrthoCharts.GetAllForPatient(_patCur.PatNum);
			List<OrthoChart> listNew=new List<OrthoChart>();
			for(int r=0;r<_tableOrtho.Rows.Count;r++) {
				for(int c=1;c<_tableOrtho.Columns.Count;c++) {//skip col 0
					OrthoChart tempChart=new OrthoChart();
					tempChart.DateService=(DateTime)_tableOrtho.Rows[r][0];
					tempChart.FieldName=_listOrthDisplayFields[c-1].Description;
					tempChart.FieldValue=_tableOrtho.Rows[r][c].ToString();
					tempChart.PatNum=_patCur.PatNum;
					listNew.Add(tempChart);
				}
			}
			//Inserts, updates, or deletes database rows to match supplied list.
			//Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
			List<OrthoChart> listIns    =new List<OrthoChart>();
			List<OrthoChart> listUpdNew =new List<OrthoChart>();
			List<OrthoChart> listUpdDB  =new List<OrthoChart>();
			List<OrthoChart> listDel    =new List<OrthoChart>();
			List<string> listColNames=new List<string>();
			//Remove fields from the DB list that are not currently set to display.
			for(int i=0;i<_listOrthDisplayFields.Count;i++){
				listColNames.Add(_listOrthDisplayFields[i].Description);
			}
			for(int i=listDB.Count-1;i>=0;i--){
				if(!listColNames.Contains(listDB[i].FieldName)){
					listDB.RemoveAt(i);
				}
			}
			listNew.Sort(OrthoCharts.SortDateField);
			listDB.Sort(OrthoCharts.SortDateField);
			int idxNew=0;
			int idxDB=0;
			OrthoChart fieldNew;
			OrthoChart fieldDB;
			//Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
			//If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
			while(idxNew<listNew.Count || idxDB<listDB.Count) {
				fieldNew=null;
				if(idxNew<listNew.Count) {
					fieldNew=listNew[idxNew];
				}
				fieldDB=null;
				if(idxDB<listDB.Count) {
					fieldDB=listDB[idxDB];
				}
				//begin compare
				if(fieldNew!=null && fieldDB==null) {//listNew has more items, listDB does not.
					listIns.Add(fieldNew);
					idxNew++;
					continue;
				}
				else if(fieldNew==null && fieldDB!=null) {//listDB has more items, listNew does not.
					listDel.Add(fieldDB);
					idxDB++;
					continue;
				}
				else if(fieldNew.DateService<fieldDB.DateService) {//newPK less than dbPK, newItem is 'next'
					listIns.Add(fieldNew);
					idxNew++;
					continue;
				}
				else if(fieldNew.DateService>fieldDB.DateService) {//dbPK less than newPK, dbItem is 'next'
					listDel.Add(fieldDB);
					idxDB++;
					continue;
				}
				else if(fieldNew.FieldName.CompareTo(fieldDB.FieldName)<0) {//New Fieldname Comes First
					listIns.Add(fieldNew);
					idxNew++;
					continue;
				}
				else if(fieldNew.FieldName.CompareTo(fieldDB.FieldName)>0) {//DB Fieldname Comes First
					listDel.Add(fieldDB);
					idxDB++;
					continue;
				}
				//Both lists contain the 'next' item, update required
				listUpdNew.Add(fieldNew);
				listUpdDB.Add(fieldDB);
				idxNew++;
				idxDB++;
			}
			//Commit changes to DB
			for(int i=0;i<listIns.Count;i++) {
				if(listIns[i].FieldValue=="") {//do not insert new blank values. This happens when fields from today are not used.
					continue;
				}
				OrthoCharts.Insert(listIns[i]);
			}
			for(int i=0;i<listUpdNew.Count;i++) {
				if(listUpdDB[i].FieldValue==listUpdNew[i].FieldValue) {
					continue;//values equal. do not update/create log entry.
				}
				if(listUpdNew[i].FieldValue!="") {//Actually update rows that have a new value.
					OrthoCharts.Update(listUpdNew[i],listUpdDB[i]);
				}
				else {//instead of updating to a blank value, we delete the row from the DB.
					listDel.Add(listUpdDB[i]);
				}
				#region security log entry
				string logText=Lan.g(this,"Ortho chart field edited.  Field date")+": "+listUpdNew[i].DateService.ToShortDateString()+"  "
					+Lan.g(this,"Field name")+": "+listUpdNew[i].FieldName+"\r\n";
				//Do not log the Base64 information into the audit trail if this is a signature column, log some short descriptive text instead.
				if(_tableOrtho.Columns.IndexOf(listUpdNew[i].FieldName)==_sigTableOrthoColIdx) {
					if(listUpdDB[i].FieldValue != "" && listUpdNew[i].FieldValue != "") {
						logText+=Lan.g(this,"Signature modified.")+" ";
					}
					else if(listUpdDB[i].FieldValue != "" && listUpdNew[i].FieldValue=="") {
						logText+=Lan.g(this,"Signature deleted.")+" ";
					}
				}
				else {//Not a signature
					logText+=Lan.g(this,"Old value")+": \""+listUpdDB[i].FieldValue+"\"  "
						+Lan.g(this,"New value")+": \""+listUpdNew[i].FieldValue+"\" ";
				}
				logText+=listUpdDB[i].DateService.ToString("yyyyMMdd");//This date stamp must be the last 8 characters for new OrthoEdit audit trail entries.
				SecurityLogs.MakeLogEntry(Permissions.OrthoChartEdit,_patCur.PatNum,logText);
				#endregion
			}
			for(int i=0;i<listDel.Count;i++) {//All logging should have been performed above in the "Update block"
				OrthoCharts.Delete(listDel[i].OrthoChartNum);
			}
			#endregion
		}

		///<summary>Stores the signature string and whether the signature is Topaz. Use ToString() to store it in the database.
		///This class saves us from adding a IsSigTopaz column to the orthochart table (somehow).</summary>
		private class OrthoSignature {
			public bool IsTopaz;
			public string SigString;

			///<summary>Parses a string like "0:ritwq/wV8vlrgUYahhK+RH5UeBFA6W4jCkZdo0cDWd63aZb1S/W3Z4eW5LmchqfgniG23" into a Signature object. 
			///The 1st character is whether or not the signature is Topaz; the 2nd character is a separator; the rest of the string is the signature data.
			///</summary>
			public OrthoSignature(string dbString) {
				if(dbString.Length < 3) {
					IsTopaz=false;
					SigString="";
					return;
				}
				IsTopaz=dbString[0]=='1' ? true : false;
				SigString=dbString.Substring(2);
			}

			public OrthoSignature() {
				IsTopaz=false;
				SigString="";
			}

			///<summary>Gets the signature string from a string like "1:52222559445999975122111500485555". Passing in an empty string will return an empty
			///string.</summary>
			public static string GetSigString(string dbString) {
				if(dbString.Length < 3) {
					return "";
				}
				return dbString.Substring(2);
			}

			///<summary>Converts the object to a string like "0:ritwq/wV8vlrgUYahhK+RH5UeBFA6W4jCkZdo0cDWd63aZb1S/W3Z4eW5LmchqfgniG23". The 1st character
			///is whether or not the signature is Topaz; the 2nd character is a separator; the rest of the string is the signature data.</summary>
			public override string ToString() {
				return (IsTopaz ? "1" : "0")+":"+SigString;
			}
		}

	}
}