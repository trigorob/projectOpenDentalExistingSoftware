using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.UI;

namespace OpenDental {
	public partial class FormRxManage:ODForm {
		private UI.ODGrid gridMain;
		private OpenDental.UI.Button butPrintSelected;
		private OpenDental.UI.Button butClose;
		private UI.Button butNewRx;
		private Label labelECWerror;
		private Patient _patCur;
		private List<RxPat> _listRx;

		public FormRxManage(Patient patCur) {
			InitializeComponent();
			_patCur=patCur;
			Lan.F(this);
		}

		private void FormRxManage_Load(object sender,System.EventArgs e) {
			FillGrid();
		}

		private void FillGrid() {
			gridMain.BeginUpdate();
			gridMain.Columns.Clear();
			ODGridColumn col=new ODGridColumn(Lan.g("TableRxManage","Date"),70);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lan.g("TableRxManage","Drug"),140);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lan.g("TableRxManage","Sig"),250);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lan.g("TableRxManage","Disp"),70);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lan.g("TableRxManage","Refills"),70);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lan.g("TableRxManage","Provider"),70);
			gridMain.Columns.Add(col);
			col=new ODGridColumn(Lan.g("TableRxManage","Notes"),300);
			gridMain.Columns.Add(col);
			gridMain.Rows.Clear();
			_listRx=RxPats.GetAllForPat(_patCur.PatNum);
			_listRx.Sort(SortByRxDate);
			ODGridRow row;
			for(int i = 0;i<_listRx.Count;i++) {
				row=new ODGridRow();
				row.Cells.Add(_listRx[i].RxDate.ToShortDateString());
				row.Cells.Add(_listRx[i].Drug);
				row.Cells.Add(_listRx[i].Sig);
				row.Cells.Add(_listRx[i].Disp);
				row.Cells.Add(_listRx[i].Refills);
				row.Cells.Add(Providers.GetAbbr(_listRx[i].ProvNum));
				row.Cells.Add(_listRx[i].Notes);
				row.Tag=_listRx[i].Copy();
				gridMain.Rows.Add(row);
			}
			gridMain.EndUpdate();
		}

		///<summary>Sorts the passed in RxPats by RxDate and then RxNum.</summary>
		private int SortByRxDate(RxPat rx1,RxPat rx2) {
			if(rx1.RxDate!=rx2.RxDate) {
				return rx2.RxDate.CompareTo(rx1.RxDate);
			}
			return rx2.RxNum.CompareTo(rx1.RxNum);
		}

		private void gridMain_CellDoubleClick(object sender,ODGridClickEventArgs e) {
			if(gridMain.GetSelectedIndex()==-1) {
				//this should never happen
				return;
			}
			RxPat rx=_listRx[gridMain.GetSelectedIndex()];
			FormRxEdit FormRxE=new FormRxEdit(_patCur,rx);
			FormRxE.ShowDialog();
			if(FormRxE.DialogResult!=DialogResult.OK) {
				return;
			}
			FillGrid();
		}

		///<summary>Prints the selected rx's. If one rx is selected, uses single rx sheet. If more than one is selected, uses multirx sheet</summary>
		private void butPrintSelect_Click(object sender,EventArgs e) {
			List<RxPat> listSelectRx=new List<RxPat>();
			SheetDef sheetDef;
			Sheet sheet;
			for(int i=0;i<gridMain.SelectedIndices.Length;i++) {
				listSelectRx.Add(_listRx[gridMain.SelectedIndices[i]]);
			}
			if(listSelectRx.Count==0) {
				MsgBox.Show(this,"At least one prescription must be selected");
				return;
			}
			if(listSelectRx.Count==1) {//old way of printing one rx
				//This logic is an exact copy of FormRxEdit.butPrint_Click()'s logic.  If this is updated, that method needs to be updated as well.
				List<SheetDef> customSheetDefs=SheetDefs.GetCustomForType(SheetTypeEnum.Rx);
				if(customSheetDefs.Count==0) {
					sheetDef=SheetsInternal.GetSheetDef(SheetInternalType.Rx);
				}
				else {
					sheetDef=customSheetDefs[0];
					SheetDefs.GetFieldsAndParameters(sheetDef);
				}
				sheet=SheetUtil.CreateSheet(sheetDef,_patCur.PatNum);
				SheetParameter.SetParameter(sheet,"RxNum",listSelectRx[0].RxNum);
				SheetFiller.FillFields(sheet);
				SheetUtil.CalculateHeights(sheet);
				SheetPrinting.PrintRx(sheet,listSelectRx[0].IsControlled);
			}
			else { //multiple rx selected
				sheetDef=SheetDefs.GetInternalOrCustom(SheetInternalType.RxMulti);
				List<int> rxSheetCountList=GetSheetRxCount(sheetDef);//gets the number of rx available in the sheet
				if(sheetDef.Parameters.Count==0) {//adds parameters if internal sheet
					sheetDef.Parameters.Add(new SheetParameter(true,"ListRxNums"));
					sheetDef.Parameters.Add(new SheetParameter(true,"ListRxSheet"));
				}
				List<Sheet> batchRxList=new List<Sheet>();//list of sheets to be batch printed
				if(rxSheetCountList.Count==0) {
					MessageBox.Show(Lan.g(this,"At least one drug output text field must be added to the MultiRx Sheet"));
					return;
				}
				//Sort RxPats into batches. rxSheetCount is most rx's we can print on one sheet.
				int batchSize=rxSheetCountList.Count;
				int batchIdx=0;
				List<List<RxPat>> batches=new List<List<RxPat>>();
				for(int i = 0;i<listSelectRx.Count;i++) {
					if(i>0 && i%batchSize==0) {
						batchIdx++;
					}
					if(i%batchSize==0) {
						batches.Add(new List<RxPat>());
					}
					batches[batchIdx].Add(listSelectRx[i]);
				}
				//Fill and add sheets to batchRxList to be printed
				foreach(List<RxPat> listBatch in batches) {
					sheet=SheetUtil.CreateSheet(sheetDef,_patCur.PatNum);
					SheetParameter.SetParameter(sheet,"ListRxNums",listBatch);
					SheetParameter.SetParameter(sheet,"ListRxSheet",rxSheetCountList);
					SheetFiller.FillFields(sheet);
					batchRxList.Add(sheet);
				}
				//Print batch list of rx
				SheetPrinting.PrintMultiRx(batchRxList);
			}
		}

		///<summary>Returns a list of the integer values corresponding to the Rx multi defs (1 through 6) found on the MultiRx sheet.
		///An rx is considered valid if it has ProvNameFL,PatNameFL,PatBirthdate, and Drug</summary>
		private List<int> GetSheetRxCount(SheetDef sheetDef) {
			List<int> rxFieldList=new List<int>();
			bool hasProvNameFL=false;
			bool hasProvNameFL2=false;
			bool hasProvNameFL3=false;
			bool hasProvNameFL4=false;
			bool hasProvNameFL5=false;
			bool hasProvNameFL6=false;
			bool hasPatNameFL=false;
			bool hasPatNameFL2=false;
			bool hasPatNameFL3=false;
			bool hasPatNameFL4=false;
			bool hasPatNameFL5=false;
			bool hasPatNameFL6=false;
			bool hasPatBirthdate=false;
			bool hasPatBirthdate2=false;
			bool hasPatBirthdate3=false;
			bool hasPatBirthdate4=false;
			bool hasPatBirthdate5=false;
			bool hasPatBirthdate6=false;
			bool hasDrug=false;
			bool hasDrug2=false;
			bool hasDrug3=false;
			bool hasDrug4=false;
			bool hasDrug5=false;
			bool hasDrug6=false;
			foreach(SheetFieldDef field in sheetDef.SheetFieldDefs) {
				if(field.FieldName=="prov.nameFL") {
					hasProvNameFL=true;
				}
				if(field.FieldName=="prov.nameFL2") {
					hasProvNameFL2=true;
				}
				if(field.FieldName=="prov.nameFL3") {
					hasProvNameFL3=true;
				}
				if(field.FieldName=="prov.nameFL4") {
					hasProvNameFL4=true;
				}
				if(field.FieldName=="prov.nameFL5") {
					hasProvNameFL5=true;
				}
				if(field.FieldName=="prov.nameFL6") {
					hasProvNameFL6=true;
				}
				if(field.FieldName=="pat.nameFL") {
					hasPatNameFL=true;
				}
				if(field.FieldName=="pat.nameFL2") {
					hasPatNameFL2=true;
				}
				if(field.FieldName=="pat.nameFL3") {
					hasPatNameFL3=true;
				}
				if(field.FieldName=="pat.nameFL4") {
					hasPatNameFL4=true;
				}
				if(field.FieldName=="pat.nameFL5") {
					hasPatNameFL5=true;
				}
				if(field.FieldName=="pat.nameFL6") {
					hasPatNameFL6=true;
				}
				if(field.FieldName=="pat.Birthdate") {
					hasPatBirthdate=true;
				}
				if(field.FieldName=="pat.Birthdate2") {
					hasPatBirthdate2=true;
				}
				if(field.FieldName=="pat.Birthdate3") {
					hasPatBirthdate3=true;
				}
				if(field.FieldName=="pat.Birthdate4") {
					hasPatBirthdate4=true;
				}
				if(field.FieldName=="pat.Birthdate5") {
					hasPatBirthdate5=true;
				}
				if(field.FieldName=="pat.Birthdate6") {
					hasPatBirthdate6=true;
				}
				if(field.FieldName=="Drug") {
					hasDrug=true;
				}
				if(field.FieldName=="Drug2") {
					hasDrug2=true;
				}
				if(field.FieldName=="Drug3") {
					hasDrug3=true;
				}
				if(field.FieldName=="Drug4") {
					hasDrug4=true;
				}
				if(field.FieldName=="Drug5") {
					hasDrug5=true;
				}
				if(field.FieldName=="Drug6") {
					hasDrug6=true;
				}
			}	
			if(hasProvNameFL && hasPatNameFL && hasPatBirthdate && hasDrug) {
				rxFieldList.Add(1);
			}
			if(hasProvNameFL2 && hasPatNameFL2 && hasPatBirthdate2 && hasDrug2) {
				rxFieldList.Add(2);
			}
			if(hasProvNameFL3 && hasPatNameFL3 && hasPatBirthdate3 && hasDrug3) {
				rxFieldList.Add(3);
			}
			if(hasProvNameFL4 && hasPatNameFL4 && hasPatBirthdate4 && hasDrug4) {
				rxFieldList.Add(4);
			}
			if(hasProvNameFL5 && hasPatNameFL5 && hasPatBirthdate5 && hasDrug5) {
				rxFieldList.Add(5);
			}
			if(hasProvNameFL6 && hasPatNameFL6 && hasPatBirthdate6 && hasDrug6) {
				rxFieldList.Add(6);
			}
			return rxFieldList;
		}

		private void butNewRx_Click(object sender,EventArgs e) {
			//This code is a copy of ContrChart.Tool_Rx_Click().  Any changes to this code need to be changed there too.
			if(!Security.IsAuthorized(Permissions.RxCreate)) {
				return;
			}
			if(Programs.UsingEcwTightOrFullMode() && Bridges.ECW.UserId!=0) {
				VBbridges.Ecw.LoadRxForm((int)Bridges.ECW.UserId,Bridges.ECW.EcwConfigPath,(int)Bridges.ECW.AptNum);
				//refresh the right panel:
				try {
					string strAppServer=VBbridges.Ecw.GetAppServer((int)Bridges.ECW.UserId,Bridges.ECW.EcwConfigPath);
					labelECWerror.Visible=false;
				}
				catch(Exception ex) {
					labelECWerror.Text="Error: "+ex.Message;
					labelECWerror.Visible=true;
				}
			}
			else {
				FormRxSelect FormRS=new FormRxSelect(_patCur);
				FormRS.ShowDialog();
				if(FormRS.DialogResult!=DialogResult.OK) {
					return;
				}
				SecurityLogs.MakeLogEntry(Permissions.RxCreate,_patCur.PatNum,"Created prescription.");
			}
			FillGrid();
		}

		private void butClose_Click(object sender,EventArgs e) {
			Close();
		}

	}
}