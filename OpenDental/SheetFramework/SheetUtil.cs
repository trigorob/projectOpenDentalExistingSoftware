﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using OpenDental.UI;
using OpenDentBusiness;

namespace OpenDental{
	public class SheetUtil {
		private static List<MedLabResult> _listResults;
		///<summary>Supply a template sheet as well as a list of primary keys.  This method creates a new collection of sheets which each have a parameter of int.  It also fills the sheets with data from the database, so no need to run that separately.</summary>
		public static List<Sheet> CreateBatch(SheetDef sheetDef,List<long> priKeys) {
			//we'll assume for now that a batch sheet has only one parameter, so no need to check for values.
			//foreach(SheetParameter param in sheet.Parameters){
			//	if(param.IsRequired && param.ParamValue==null){
			//		throw new ApplicationException(Lan.g("Sheet","Parameter not specified for sheet: ")+param.ParamName);
			//	}
			//}
			List<Sheet> retVal=new List<Sheet>();
			//List<int> paramVals=(List<int>)sheet.Parameters[0].ParamValue;
			Sheet newSheet;
			SheetParameter paramNew;
			for(int i=0;i<priKeys.Count;i++){
				newSheet=CreateSheet(sheetDef);
				newSheet.Parameters=new List<SheetParameter>();
				paramNew=new SheetParameter(sheetDef.Parameters[0].IsRequired,sheetDef.Parameters[0].ParamName);
				paramNew.ParamValue=priKeys[i];
				newSheet.Parameters.Add(paramNew);
				SheetFiller.FillFields(newSheet);
				retVal.Add(newSheet);
			}
			return retVal;
		}

		///<summary>Just before printing or displaying the final sheet output, the heights and y positions of various fields are adjusted according to 
		///their growth behavior.  This also now gets run every time a user changes the value of a textbox while filling out a sheet.
		///dataSet should be prefilled by calling AccountModules.GetAccount() before calling this method in order to calculate for statements.</summary>
		public static void CalculateHeights(Sheet sheet,DataSet dataSet=null,Statement stmt=null,bool isPrinting=false,
			int topMargin=40,int bottomMargin=60,MedLab medLab=null)
		{
			int calcH;
			Font font;
			FontStyle fontstyle;
			sheet.SheetFields=sheet.SheetFields.OrderBy(x => x.YPos).ThenBy(x => x.XPos).ToList();
			foreach(SheetField field in sheet.SheetFields) {
				if(field.FieldType==SheetFieldType.Image || field.FieldType==SheetFieldType.PatImage) {
					#region Get the path for the image
					string filePathAndName;
					switch(field.FieldType) {
						case SheetFieldType.Image:
							filePathAndName=ODFileUtils.CombinePaths(GetImagePath(),field.FieldName);
							break;
						case SheetFieldType.PatImage:
							if(field.FieldValue=="") {
								//There is no document object to use for display, but there may be a baked in image and that situation is dealt with below.
								filePathAndName="";
								break;
							}
							Document patDoc=Documents.GetByNum(PIn.Long(field.FieldValue));
							List<string> paths=Documents.GetPaths(new List<long> { patDoc.DocNum },ImageStore.GetPreferredAtoZpath());
							if(paths.Count < 1) {//No path was found so we cannot draw the image.
								continue;
							}
							filePathAndName=paths[0];
							break;
						default:
							//not an image field
							continue;
					}
					#endregion
					if(field.FieldName=="Patient Info.gif" || File.Exists(filePathAndName)) {
						continue;
					}
					else {//img doesn't exist or we do not have access to it.
						field.Height=0;//Set height to zero so that it will not cause extra pages to print.
					}
				}
				if(field.GrowthBehavior==GrowthBehaviorEnum.None){//Images don't have growth behavior, so images are excluded below this point.
					continue;
				}
				fontstyle=FontStyle.Regular;
				if(field.FontIsBold){
					fontstyle=FontStyle.Bold;
				}
				font=new Font(field.FontName,field.FontSize,fontstyle);
				//calcH=(int)g.MeasureString(field.FieldValue,font).Height;//this was too short
				switch(field.FieldType) {
					case SheetFieldType.Grid:
						calcH=CalculateGridHeightHelper(field,sheet,stmt,topMargin,bottomMargin,medLab,dataSet);
						break;
					case SheetFieldType.Special:
						calcH=field.Height;
						break;
					default:
						calcH=GraphicsHelper.MeasureStringH(field.FieldValue,font,field.Width,field.TextAlign);
						break;
				}
				if(calcH<=field.Height //calc height is smaller
					&& field.FieldName!="StatementPayPlan" //allow this grid to shrink and disapear.
					&& field.FieldName!="TreatPlanBenefitsFamily" //allow this grid to shrink and disapear.
					&& field.FieldName!="TreatPlanBenefitsIndividual") //allow this grid to shrink and disapear.
				{
					continue;
				}
				int amountOfGrowth=calcH-field.Height;
				field.Height=calcH;
				if(field.GrowthBehavior==GrowthBehaviorEnum.DownLocal){
					MoveAllDownWhichIntersect(sheet,field,amountOfGrowth);
				}
				else if(field.GrowthBehavior==GrowthBehaviorEnum.DownGlobal){
					//All sheet grids should have DownGlobal growth.
					MoveAllDownBelowThis(sheet,field,amountOfGrowth);
				}
			}
			if(isPrinting && !Sheets.SheetTypeIsSinglePage(sheet.SheetType)) {
				//now break all text fields in between lines, not in the middle of actual text
				sheet.SheetFields.Sort(SheetFields.SortDrawingOrderLayers);
				int originalSheetFieldCount=sheet.SheetFields.Count;
				for(int i=0;i<originalSheetFieldCount;i++) {
					SheetField fieldCur=sheet.SheetFields[i];
					if(fieldCur.FieldType==SheetFieldType.StaticText
						|| fieldCur.FieldType==SheetFieldType.OutputText
						|| fieldCur.FieldType==SheetFieldType.InputField)
					{
						//recursive function to split text boxes for page breaks in between lines of text, not in the middle of text
						CalculateHeightsPageBreakForText(fieldCur,sheet);
					}
				}
			}
			//sort the fields again since we may have broken up some of the text fields into multiple fields and added them to sheetfields.
			sheet.SheetFields.Sort(SheetFields.SortDrawingOrderLayers);
			//return sheetCopy;
		}

		///<summary>Recursive.  Only for text sheet fields.</summary>
		private static void CalculateHeightsPageBreakForText(SheetField field,Sheet sheet) {
			FontStyle fontstyle=FontStyle.Regular;
			if(field.FontIsBold) {
				fontstyle=FontStyle.Bold;
			}
			Font font=new Font(field.FontName,field.FontSize,fontstyle);
			string text=field.FieldValue.Replace("\r\n","\n");//The RichTextBox control converts \r\n to \n.  We need to mimic so we can substring() below.
			//adjust the height of the text box to accomodate PDFs if the field has a growth behavior other than None
			int calcH=GraphicsHelper.MeasureStringH(text,font,field.Width,field.TextAlign);
			if(field.GrowthBehavior!=GrowthBehaviorEnum.None && field.Height < calcH) {
				int amtGrowth=calcH-field.Height;
				field.Height+=amtGrowth;
				if(field.GrowthBehavior==GrowthBehaviorEnum.DownLocal) {
					MoveAllDownWhichIntersect(sheet,field,amtGrowth);
				}
				else if(field.GrowthBehavior==GrowthBehaviorEnum.DownGlobal) {
					MoveAllDownBelowThis(sheet,field,amtGrowth);
				}
			}
			int topMargin=40;
			if(sheet.SheetType==SheetTypeEnum.MedLabResults) {
				topMargin=120;
			}
			int pageCount;
			int bottomCurPage=SheetPrinting.bottomCurPage(field.YPos,sheet,out pageCount);
			//recursion base case, the field now fits on the current page, break out of recursion
			if(field.YPos+field.Height<=bottomCurPage) {
				return;
			}
			//field extends beyond the bottom of the current page, so we will split the text box in between lines, not through the middle of text
			//if the height of one line is greater than the printable height of the page, don't try to split between lines (only for huge fonts)
			if(font.Height+2 > (sheet.HeightPage-60-topMargin) || text.Length==0) {
				return;
			}
			int textBoxHeight=bottomCurPage-field.YPos;//the max height that the new text box can be in order to fit on the current page.
			//figure out how many lines of text will fit on the current page
			RichTextBox textboxClip=GraphicsHelper.CreateTextBoxForSheetDisplay(text,font,field.Width,textBoxHeight,field.TextAlign);
			List <RichTextLineInfo> listClipTextLines=GraphicsHelper.GetTextSheetDisplayLines(textboxClip);
			//if no lines of text will fit on current page or textboxClip's height is smaller than one line, move the entire text box to the next page
			if(listClipTextLines.Count==0 || textBoxHeight < (font.Height+2)) {
				int moveAmount=bottomCurPage+1-field.YPos;
				field.Height+=moveAmount;
				MoveAllDownWhichIntersect(sheet,field,moveAmount);
				field.Height-=moveAmount;
				field.YPos+=moveAmount;
				//recursive call
				CalculateHeightsPageBreakForText(field,sheet);
				return;
			}
			//prepare to split the text box into two text boxes, one with the lines that will fit on the current page, the other with all other lines
			int fieldH=GraphicsHelper.MeasureStringH(textboxClip.Text,textboxClip.Font,textboxClip.Width,textboxClip.SelectionAlignment);
			//get ready to copy text from the current field to a copy of the field that will be moved down.
			//find the character in the text box that makes the text box taller than the calculated max line height and split the text box at that line
			SheetField fieldNew;
			fieldNew=field.Copy();
			field.Height=fieldH;
			fieldNew.Height-=fieldH;//reduce the size of the new text box by the height of the text removed
			fieldNew.YPos+=fieldH;//move the new field down the amount of the removed text to maintain the distance between all fields below
			fieldNew.FieldValue=text.Substring(textboxClip.Text.Length);
			field.FieldValue=textboxClip.Text;
			int moveAmountNew=bottomCurPage+1-fieldNew.YPos;
			fieldNew.Height+=moveAmountNew;
			MoveAllDownWhichIntersect(sheet,fieldNew,moveAmountNew);
			fieldNew.Height-=moveAmountNew;
			fieldNew.YPos+=moveAmountNew;
			sheet.SheetFields.Add(fieldNew);
			//recursive call
			CalculateHeightsPageBreakForText(fieldNew,sheet);
		}

		///<summary>Calculates height of grid taking into account page breaks, word wrapping, cell width, font size, and actual data to be used to fill this grid.
		///DataSet should be prefilled with AccountModules.GetAccount() before calling this method if calculating for a statement.</summary>
		private static int CalculateGridHeightHelper(SheetField field,Sheet sheet,Statement stmt,int topMargin,int bottomMargin,MedLab medLab
			,DataSet dataSet) 
		{
			ODGrid odGrid=new ODGrid();
			odGrid.FontForSheets=new Font(field.FontName,field.FontSize,field.FontIsBold?FontStyle.Bold:FontStyle.Regular);
			odGrid.Width=field.Width;
			odGrid.HideScrollBars=true;
			odGrid.YPosField=field.YPos;
			odGrid.TopMargin=topMargin;
			odGrid.BottomMargin=bottomMargin;
			odGrid.PageHeight=sheet.HeightPage;
			odGrid.Title=field.FieldName;
			if(stmt!=null) {
				odGrid.Title+=(stmt.Intermingled?".Intermingled":".NotIntermingled");//Important for calculating heights.
			}
			DataTable table=GetDataTableForGridType(sheet,dataSet,field.FieldName,stmt,medLab);
			List<DisplayField> columns=GetGridColumnsAvailable(field.FieldName);
			if(sheet.SheetType==SheetTypeEnum.Statement && stmt!=null &&  stmt.SuperFamily==0) {
				columns.RemoveAll(x => x.InternalName=="AcctTotal" || x.InternalName=="Account");
			}
			#region  Fill Grid
			odGrid.BeginUpdate();
			odGrid.Columns.Clear();
			ODGridColumn col;
			for(int i=0;i<columns.Count;i++) {
				col=new ODGridColumn(columns[i].InternalName,columns[i].ColumnWidth);
				odGrid.Columns.Add(col);
			}
			ODGridRow row;
			for(int i=0;i<table.Rows.Count;i++) {
				row=new ODGridRow();
				for(int c=0;c<columns.Count;c++) {//Selectively fill columns from the dataTable into the odGrid.
					row.Cells.Add(table.Rows[i][columns[c].InternalName].ToString());
				}
				if(table.Columns.Contains("PatNum")) {//Used for statments to determine account splitting.
					row.Tag=table.Rows[i]["PatNum"].ToString();
				}
				odGrid.Rows.Add(row);
			}
			odGrid.EndUpdate(true);//Calls ComputeRows and ComputeColumns, meaning the RowHeights int[] has been filled.
			#endregion
			return odGrid.PrintHeight;
		}

		public static void MoveAllDownBelowThis(Sheet sheet,SheetField field,int amountOfGrowth){
			foreach(SheetField field2 in sheet.SheetFields) {
				if(field2.YPos>field.YPos) {//for all fields that are below this one
					field2.YPos+=amountOfGrowth;//bump down by amount that this one grew
				}
			}
		}

		///<Summary>Supply the field that we are testing.  All other fields which intersect with it will be moved down.  Each time one (or maybe some) is moved down, this method is called recursively.  The end result should be no intersections among fields near the original field that grew.</Summary>
		public static void MoveAllDownWhichIntersect(Sheet sheet,SheetField field,int amountOfGrowth) {
			//Phase 1 is to move everything that intersects with the field down. Phase 2 is to call this method on everything that was moved.
			//Phase 1: Move 
			List<SheetField> affectedFields=new List<SheetField>();
			foreach(SheetField field2 in sheet.SheetFields) {
				if(field2==field){
					continue;
				}
				if(field2.YPos<field.YPos){//only fields which are below this one
					continue;
				}
				if(field2.FieldType==SheetFieldType.Drawing){
					continue;
					//drawings do not get moved down.
				}
				if(field.Bounds.IntersectsWith(field2.Bounds)) {
					field2.YPos+=amountOfGrowth;
					affectedFields.Add(field2);
				}
			}
			//Phase 2: Recursion
			foreach(SheetField field2 in affectedFields) {
			  //reuse the same amountOfGrowth again.
			  MoveAllDownWhichIntersect(sheet,field2,amountOfGrowth);
			}
		}

		///<summary>Returns the lowest Y position from all the sheet field defs passed in.</summary>
		public static int GetLowestYPos(List<SheetFieldDef> listSheetFieldDefs) {
			int lowestYPos=0;
			foreach(SheetFieldDef sheetFieldDef in listSheetFieldDefs) {
				int bottom=sheetFieldDef.YPos + sheetFieldDef.Height;
				if(bottom > lowestYPos) {
					lowestYPos=bottom;
				}
			}
			return lowestYPos;
		}

		///<summary>Creates a Sheet object from a sheetDef, complete with fields and parameters.  Sets date to today. If patNum=0, do not save to DB, such as for labels.</summary>
		public static Sheet CreateSheet(SheetDef sheetDef,long patNum=0,bool hidePaymentOptions=false) {
			Sheet sheet=new Sheet();
			sheet.IsNew=true;
			sheet.DateTimeSheet=DateTime.Now;
			sheet.FontName=sheetDef.FontName;
			sheet.FontSize=sheetDef.FontSize;
			sheet.Height=sheetDef.Height;
			sheet.SheetType=sheetDef.SheetType;
			sheet.Width=sheetDef.Width;
			sheet.PatNum=patNum;
			sheet.Description=sheetDef.Description;
			sheet.IsLandscape=sheetDef.IsLandscape;
			sheet.IsMultiPage=sheetDef.IsMultiPage;
			sheet.SheetFields=CreateFieldList(sheetDef.SheetFieldDefs,hidePaymentOptions);//Blank fields with no values. Values filled later from SheetFiller.FillFields()
			sheet.Parameters=sheetDef.Parameters;
			return sheet;
		}

		///<summary>Creates a Sheet from a WebSheet. Does not insert it into the database.</summary>
		public static Sheet CreateSheetFromWebSheet(long PatNum,WebSheets.SheetAndSheetField sAnds) {
			Sheet newSheet=null;
			SheetDef sheetDef=new SheetDef((SheetTypeEnum)sAnds.web_sheet.SheetType);
			newSheet=CreateSheet(sheetDef,PatNum);
			SheetParameter.SetParameter(newSheet,"PatNum",PatNum);
			newSheet.DateTimeSheet=sAnds.web_sheet.DateTimeSheet;
			newSheet.Description=sAnds.web_sheet.Description;
			newSheet.Height=sAnds.web_sheet.Height;
			newSheet.Width=sAnds.web_sheet.Width;
			newSheet.FontName=sAnds.web_sheet.FontName;
			newSheet.FontSize=sAnds.web_sheet.FontSize;
			newSheet.SheetType=(SheetTypeEnum)sAnds.web_sheet.SheetType;
			newSheet.IsLandscape=sAnds.web_sheet.IsLandscape==1 ? true : false;
			newSheet.InternalNote="";
			newSheet.IsWebForm=true;
			//loop through each variable in a single sheetfield
			for(int i=0;i<sAnds.web_sheetfieldlist.Count();i++) {
				SheetField sheetfield=new SheetField();
				sheetfield.FieldName=sAnds.web_sheetfieldlist[i].FieldName;
				sheetfield.FieldType=(SheetFieldType)sAnds.web_sheetfieldlist[i].FieldType;
				if(sAnds.web_sheetfieldlist[i].FontIsBold==1) {
					sheetfield.FontIsBold=true;
				}
				else {
					sheetfield.FontIsBold=false;
				}
				sheetfield.FontIsBold=sAnds.web_sheetfieldlist[i].FontIsBold==1 ? true : false;
				sheetfield.FontName=sAnds.web_sheetfieldlist[i].FontName;
				sheetfield.FontSize=sAnds.web_sheetfieldlist[i].FontSize;
				sheetfield.Height=sAnds.web_sheetfieldlist[i].Height;
				sheetfield.Width=sAnds.web_sheetfieldlist[i].Width;
				sheetfield.XPos=sAnds.web_sheetfieldlist[i].XPos;
				sheetfield.YPos=sAnds.web_sheetfieldlist[i].YPos;
				if(sAnds.web_sheetfieldlist[i].IsRequired==1) {
					sheetfield.IsRequired=true;
				}
				else {
					sheetfield.IsRequired=false;
				}
				sheetfield.TabOrder=sAnds.web_sheetfieldlist[i].TabOrder;
				sheetfield.ReportableName=sAnds.web_sheetfieldlist[i].ReportableName;
				sheetfield.RadioButtonGroup=sAnds.web_sheetfieldlist[i].RadioButtonGroup;
				sheetfield.RadioButtonValue=sAnds.web_sheetfieldlist[i].RadioButtonValue;
				sheetfield.GrowthBehavior=(GrowthBehaviorEnum)sAnds.web_sheetfieldlist[i].GrowthBehavior;
				sheetfield.FieldValue=sAnds.web_sheetfieldlist[i].FieldValue;
				sheetfield.TextAlign=(HorizontalAlignment)sAnds.web_sheetfieldlist[i].TextAlign;
				sheetfield.ItemColor=Color.FromArgb(sAnds.web_sheetfieldlist[i].ItemColor);
				newSheet.SheetFields.Add(sheetfield);
			}
			return newSheet;
		}

		///<summary>Returns either a user defined statements sheet, the internal sheet if StatementsUseSheets is true. Returns null if StatementsUseSheets is false.</summary>
		public static SheetDef GetStatementSheetDef() {
			if(!PrefC.GetBool(PrefName.StatementsUseSheets)) {
				return null;
			}
			List<SheetDef> listDefs=SheetDefs.GetCustomForType(SheetTypeEnum.Statement);
			if(listDefs.Count>0) {
				return SheetDefs.GetSheetDef(listDefs[0].SheetDefNum);//Return first custom statement. Should be ordred by Description ascending.
			}
			return SheetsInternal.GetSheetDef(SheetInternalType.Statement);
		}

		///<summary>Returns either a user defined MedLabResults sheet or the internal sheet.</summary>
		public static SheetDef GetMedLabResultsSheetDef() {
			List<SheetDef> listDefs=SheetDefs.GetCustomForType(SheetTypeEnum.MedLabResults);
			if(listDefs.Count>0) {
				return SheetDefs.GetSheetDef(listDefs[0].SheetDefNum);//Return first custom statement. Should be ordred by Description ascending.
			}
			return SheetsInternal.GetSheetDef(SheetInternalType.MedLabResults);
		}

		/*
		///<summary>After pulling a list of SheetFieldData objects from the database, we use this to convert it to a list of SheetFields as we create the Sheet.</summary>
		public static List<SheetField> CreateSheetFields(List<SheetFieldData> sheetFieldDataList){
			List<SheetField> retVal=new List<SheetField>();
			SheetField field;
			FontStyle style;
			for(int i=0;i<sheetFieldDataList.Count;i++){
				style=FontStyle.Regular;
				if(sheetFieldDataList[i].FontIsBold){
					style=FontStyle.Bold;
				}
				field=new SheetField(sheetFieldDataList[i].FieldType,sheetFieldDataList[i].FieldName,sheetFieldDataList[i].FieldValue,
					sheetFieldDataList[i].XPos,sheetFieldDataList[i].YPos,sheetFieldDataList[i].Width,sheetFieldDataList[i].Height,
					new Font(sheetFieldDataList[i].FontName,sheetFieldDataList[i].FontSize,style),sheetFieldDataList[i].GrowthBehavior);
				retVal.Add(field);
			}
			return retVal;
		}*/

		///<summary>Creates the initial fields from the sheetDef.FieldDefs.</summary>
		private static List<SheetField> CreateFieldList(List<SheetFieldDef> sheetFieldDefList,bool hidePaymentOptions=false){
			List<SheetField> retVal=new List<SheetField>();
			SheetField field;
			for(int i=0;i<sheetFieldDefList.Count;i++){
				if(hidePaymentOptions && FieldIsPaymentOptionHelper(sheetFieldDefList[i])){
					continue;
				}
				field=new SheetField();
				field.IsNew=true;
				field.FieldName=sheetFieldDefList[i].FieldName;
				field.FieldType=sheetFieldDefList[i].FieldType;
				field.FieldValue=sheetFieldDefList[i].FieldValue;
				field.FontIsBold=sheetFieldDefList[i].FontIsBold;
				field.FontName=sheetFieldDefList[i].FontName;
				field.FontSize=sheetFieldDefList[i].FontSize;
				field.GrowthBehavior=sheetFieldDefList[i].GrowthBehavior;
				field.Height=sheetFieldDefList[i].Height;
				field.RadioButtonValue=sheetFieldDefList[i].RadioButtonValue;
				//field.SheetNum=sheetFieldList[i];//set later
				field.Width=sheetFieldDefList[i].Width;
				field.XPos=sheetFieldDefList[i].XPos;
				field.YPos=sheetFieldDefList[i].YPos;
				field.RadioButtonGroup=sheetFieldDefList[i].RadioButtonGroup;
				field.IsRequired=sheetFieldDefList[i].IsRequired;
				field.TabOrder=sheetFieldDefList[i].TabOrder;
				field.ReportableName=sheetFieldDefList[i].ReportableName;
				field.TextAlign=sheetFieldDefList[i].TextAlign;
				field.ItemColor=sheetFieldDefList[i].ItemColor;
				retVal.Add(field);
			}
			return retVal;
		}

		private static bool FieldIsPaymentOptionHelper(SheetFieldDef sheetFieldDef) {
			if(sheetFieldDef.IsPaymentOption) {
				return true;
			}
			switch(sheetFieldDef.FieldName) {
				case "StatementEnclosed":
				case "StatementAging":
					return true;
			}
			return false;
		}

		///<summary>Typically returns something similar to \\SERVER\OpenDentImages\SheetImages</summary>
		public static string GetImagePath(){
			string imagePath;
			if(PrefC.AtoZfolderUsed==DataStorageType.InDatabase) {
				throw new ApplicationException("Must be using AtoZ folders.");
			}
			imagePath=ODFileUtils.CombinePaths(ImageStore.GetPreferredAtoZpath(),"SheetImages");
			if(PrefC.AtoZfolderUsed==DataStorageType.DropboxAtoZ) {
				imagePath=imagePath.Replace("\\","/");
			}
			if(PrefC.AtoZfolderUsed==DataStorageType.LocalAtoZ && !Directory.Exists(imagePath)) {
				Directory.CreateDirectory(imagePath);
			}
			return imagePath;
		}

		///<summary>Typically returns something similar to \\SERVER\OpenDentImages\SheetImages</summary>
		public static string GetPatImagePath() {
			string imagePath;
			if(PrefC.AtoZfolderUsed==DataStorageType.InDatabase) {
				throw new ApplicationException("Must be using AtoZ folders.");
			}
			imagePath=ODFileUtils.CombinePaths(ImageStore.GetPreferredAtoZpath(),"SheetPatImages");
			if(PrefC.AtoZfolderUsed==DataStorageType.LocalAtoZ && !Directory.Exists(imagePath)) {
				Directory.CreateDirectory(imagePath);
			}
			if(PrefC.AtoZfolderUsed==DataStorageType.DropboxAtoZ) {
				imagePath=imagePath.Replace("\\","/");
			}
			return imagePath;
		}

		///<summary>Returns the current list of all columns available for the grid in the data table.</summary>
		public static List<DisplayField> GetGridColumnsAvailable(string gridType) {
			int i=0;
			List<DisplayField> retVal=new List<DisplayField>();
			switch(gridType) {
				case "StatementMain":
					retVal=DisplayFields.GetForCategory(DisplayFieldCategory.StatementMainGrid);
					break;
				case "StatementEnclosed":
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="AmountDue",Description="Amount Due",ColumnWidth=107,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="DateDue",Description="Date Due",ColumnWidth=107,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="AmountEnclosed",Description="Amount Enclosed",ColumnWidth=107,ItemOrder=++i });
					break;
				case "StatementAging":
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="Account",Description="Account",ColumnWidth=200,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="Age00to30",Description="0-30",ColumnWidth=75,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="Age31to60",Description="31-60",ColumnWidth=75,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="Age61to90",Description="61-90",ColumnWidth=75,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="Age90plus",Description="over 90",ColumnWidth=75,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="AcctTotal",Description="Total",ColumnWidth=75,ItemOrder=++i });
					break;
				case "StatementPayPlan":
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="date",Description="Date",ColumnWidth=80,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="description",Description="Description",ColumnWidth=250,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="charges",Description="Charges",ColumnWidth=60,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="credits",Description="Credits",ColumnWidth=60,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="balance",Description="Balance",ColumnWidth=60,ItemOrder=++i });
					break;
				case "MedLabResults":
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="obsIDValue",Description="Test / Result",ColumnWidth=500,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="obsAbnormalFlag",Description="Flag",ColumnWidth=75,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="obsUnits",Description="Units",ColumnWidth=70,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="obsRefRange",Description="Ref Interval",ColumnWidth=97,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="facilityID",Description="Lab",ColumnWidth=28,ItemOrder=++i });
					break;
				case "PayPlanMain":
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="ChargeDate",Description="Date",ColumnWidth=75,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="Provider",Description="Provider",ColumnWidth=75,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="Description",Description="Description",ColumnWidth=150,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="Principal",Description="Principal",ColumnWidth=75,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="Interest",Description="Interest",ColumnWidth=75,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="due",Description="Due",ColumnWidth=75,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="payment",Description="Payment",ColumnWidth=75,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="balance",Description="Balance",ColumnWidth=75,ItemOrder=++i });
					break;
				case "TreatPlanMain":
					retVal=DisplayFields.GetForCategory(DisplayFieldCategory.TreatmentPlanModule);
					break;
				case "TreatPlanBenefitsFamily":
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="BenefitName",Description="",ColumnWidth=150,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="Primary",Description="Primary",ColumnWidth=75,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="Secondary",Description="Secondary",ColumnWidth=75,ItemOrder=++i });
					break;
				case "TreatPlanBenefitsIndividual":
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="BenefitName",Description="",ColumnWidth=150,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="Primary",Description="Primary",ColumnWidth=75,ItemOrder=++i });
					retVal.Add(new DisplayField { Category=DisplayFieldCategory.None,InternalName="Secondary",Description="Secondary",ColumnWidth=75,ItemOrder=++i });
					break;
			}
			return retVal;
		}

		///<summary></summary>
		public static List<string> GetGridsAvailable(SheetTypeEnum sheetType) {
			List<string> retVal=new List<string>();
			switch(sheetType) {
				case SheetTypeEnum.Statement:
					retVal.Add("StatementAging");
					retVal.Add("StatementEnclosed");
					retVal.Add("StatementMain");
					retVal.Add("StatementPayPlan");
					break;
				case SheetTypeEnum.MedLabResults:
					retVal.Add("MedLabResults");
					break;
				case SheetTypeEnum.PaymentPlan:
					retVal.Add("PayPlanMain");
					break;
				case SheetTypeEnum.TreatmentPlan:
					retVal.Add("TreatPlanMain");
					retVal.Add("TreatPlanBenefitsFamily");
					retVal.Add("TreatPlanBenefitsIndividual");
					break;
			}
			return retVal;
		}

		///<Summary>DataSet should be prefilled with AccountModules.GetAccount() before calling this method if getting a table for a statement.</Summary>
		public static DataTable GetDataTableForGridType(Sheet sheet,DataSet dataSet,string gridType,Statement stmt,MedLab medLab) {
			DataTable retVal=new DataTable();
			switch(gridType) {
				case "StatementMain":
					retVal=getTable_StatementMain(dataSet,stmt);
					break;
				case "StatementAging":
					retVal=getTable_StatementAging(stmt);
					break;
				case "StatementPayPlan":
					retVal=getTable_StatementPayPlan(dataSet);
					break;
				case "StatementEnclosed":
					retVal=getTable_StatementEnclosed(dataSet,stmt);
					break;
				case "MedLabResults":
					retVal=getTable_MedLabResults(medLab);
					break;
				case "PayPlanMain":
					retVal=getTable_PayPlanMain(sheet);
					break;
				case "TreatPlanMain":
					retVal=getTable_TreatPlanMain(sheet);
					break;
				case "TreatPlanBenefitsFamily":
					retVal=getTable_TreatPlanBenefitsFamily(sheet);
					break;
				case "TreatPlanBenefitsIndividual":
					retVal=getTable_TreatPlanBenefitsIndividual(sheet);
					break;
				default:
					break;
			}
			return retVal;
		}

		private static DataTable getTable_PayPlanMain(Sheet sheet) {
			PayPlan payPlan=(PayPlan)SheetParameter.GetParamByName(sheet.Parameters,"payplan").ParamValue;
			//Construct empty Data table ===============================================================================
			DataTable retVal=new DataTable();
			retVal.Columns.AddRange(new[] {
				new DataColumn("ChargeDate",typeof(string)),
				new DataColumn("Provider",typeof(string)),
				new DataColumn("Description",typeof(string)),
				new DataColumn("Principal",typeof(string)),
				new DataColumn("Interest",typeof(string)),
				new DataColumn("Due",typeof(string)),
				new DataColumn("Payment",typeof(string)),
				new DataColumn("Balance",typeof(string)),
				new DataColumn("Type",typeof(string)),
				new DataColumn("paramIsBold",typeof(bool)),
			});
			Patient patCur=Patients.GetPat(payPlan.PatNum);
			if(payPlan.PatNum==0 || patCur==null) {//Pay plan should never exist without a patnum or be null.
				return retVal;//return an empty data table that has the correct format.
			}
			//Fill data table if neccessary ===============================================================================
			List<PayPlanCharge> payPlanChargeList=PayPlanCharges.GetForPayPlan(payPlan.PayPlanNum);
			DataTable bundledPayments=PaySplits.GetForPayPlan(payPlan.PayPlanNum);
			List<PaySplit> payPlanSplitList=PaySplits.GetFromBundled(bundledPayments);
			DataTable bundledClaimProcs=ClaimProcs.GetBundlesForPayPlan(payPlan.PayPlanNum);
			int count=0;//used to set Note decription
			//===payplan charges===
			for(int i = 0;i<payPlanChargeList.Count;i++) {
				if(payPlanChargeList[i].Note.Trim().ToLower().Contains("recalculated based on") 
					|| payPlanChargeList[i].Note.Trim().ToLower().Contains("expected")) //from clasic pay plan print outs.
				{
					count++;
					continue;//Attempt to show only the real payment plan charge 
				}
				if(payPlanChargeList[i].ChargeType==PayPlanChargeType.Credit) {
					count++;
					continue;//hide credits from the amortization grid.
				}
				retVal.Rows.Add(FormPayPlan.CreateRowForPayPlanChargeDT(retVal,payPlanChargeList[i],i-count));
			}
			//===payplan payments===
			if(payPlan.PlanNum==0) {//===normal payplan===
				for(int i = 0;i<payPlanSplitList.Count;i++) {
					retVal.Rows.Add(FormPayPlan.CreateRowForPayPlanSplitDT(retVal,payPlanSplitList[i],bundledPayments.Rows[i],i));
				}
			}
			else {//===insurance payplan===
				for(int i = 0;i<bundledClaimProcs.Rows.Count;i++) {
					retVal.Rows.Add(FormPayPlan.CreateRowForClaimProcsDT(retVal,bundledClaimProcs.Rows[i]));
				}
			}
			count=0;//reset to zero for the next loop.
			//Sort rows based on date ===============================================================================
			List<DataRow> payPlanList =new List<DataRow>(retVal.Select());
			payPlanList.Sort(FormPayPlan.ComparePayPlanRowsDT);
			//Fill sorted data rows to sortRetVal DataTable ===============================================================================
			DataTable sortRetVal=retVal.Clone();
			for(int i = 0;i<payPlanList.Count;i++) {
				if(payPlanList[i][2].ToString().Trim().ToLower().Contains("downPayment")) {//description
					count++; 
				}
				if(PIn.Double(payPlanList[i][6].ToString()) > 0){//payment
					count++;
				}
				sortRetVal.Rows.Add(FormPayPlan.CreateRowForPayPlanListDT(sortRetVal,payPlanList[i],i-count));
			}
			//Calculate running totals and add to sortRetVal DataTable ===============================================================================
			double totPrincipal=0;
			double totInterest=0;
			double totDue=0;
			double totPayment=0;
			double runningBalance=0;
			for(int i = 0;i<sortRetVal.Rows.Count;i++) {
				DataRow rowTemp=sortRetVal.Rows[i];
				double rowPrincipal=PIn.Double(rowTemp["Principal"].ToString());
				double rowInterest=PIn.Double(rowTemp["Interest"].ToString());
				double rowDue=PIn.Double(rowTemp["Due"].ToString());
				double rowPayment=PIn.Double(rowTemp["Payment"].ToString());
				totPrincipal+=rowPrincipal;
				totInterest+=rowInterest;
				totDue+=rowDue;
				totPayment+=rowPayment;
				if(rowDue>0) {
					runningBalance+=rowDue;
				}
				if(rowPayment==0) {
					rowTemp["Payment"]=0.ToString("n");
				}
				else {
					runningBalance-=rowPayment;
				}
				rowTemp["Balance"]=runningBalance.ToString("n");
				rowTemp["paramIsBold"]=false;
			}
			DataRow totalRow=sortRetVal.NewRow();
			//Fill payment and balance columns/cells to sortRetVal ===============================================================================
			totalRow["Principal"]=totPrincipal.ToString("n");
			totalRow["Interest"]=totInterest.ToString("n");
			totalRow["Due"]=totDue.ToString("n");
			totalRow["Payment"]=totPayment.ToString("n");
			totalRow["paramIsBold"]=true;
			sortRetVal.Rows.Add(totalRow);
			return sortRetVal;
		}

		private static DataTable getTable_TreatPlanMain(Sheet sheet) {
			TreatPlan treatPlan=(TreatPlan)SheetParameter.GetParamByName(sheet.Parameters,"TreatPlan").ParamValue;
			bool checkShowSubtotals=(bool)SheetParameter.GetParamByName(sheet.Parameters,"checkShowSubTotals").ParamValue;
			bool checkShowTotals=(bool)SheetParameter.GetParamByName(sheet.Parameters,"checkShowTotals").ParamValue;
			//Note: this logic was ported from ContrTreat.cs
			//Construct empty Data table ===============================================================================
			DataTable retVal=new DataTable();
			retVal.Columns.AddRange(new[] {
				new DataColumn("Done",typeof(string)),
				new DataColumn("Priority",typeof(string)),
				new DataColumn("Tth",typeof(string)),
				new DataColumn("Surf",typeof(string)),
				new DataColumn("Code",typeof(string)),
				new DataColumn("Sub",typeof(string)),
				new DataColumn("Description",typeof(string)),
				new DataColumn("Fee",typeof(string)),
				new DataColumn("Pri Ins",typeof(string)),
				new DataColumn("Sec Ins",typeof(string)),
				new DataColumn("Discount",typeof(string)),
				new DataColumn("Pat",typeof(string)),
				new DataColumn("Prognosis",typeof(string)),
				new DataColumn("Dx",typeof(string)),
				new DataColumn("Abbr",typeof(string)),
				new DataColumn("paramTextColor",typeof(int)),//Name. EG "Black" or "ff0000d7"
				new DataColumn("paramIsBold",typeof(bool)),
				new DataColumn("paramIsBorderBoldBottom",typeof(bool))
			});
			Patient patCur=Patients.GetPat(treatPlan.PatNum);
			if(treatPlan.PatNum==0 || patCur==null) {
				return retVal;//return an empty data table that has the correct format.
			}
			//Fill data table if neccessary ===============================================================================
			Family famCur=Patients.GetFamily(patCur.PatNum);
			List<InsSub> subList=InsSubs.RefreshForFam(famCur);
			List<InsPlan> insPlanList=InsPlans.RefreshForSubList(subList);
			List<PatPlan> patPlanList=PatPlans.Refresh(patCur.PatNum);
			List<Benefit> benefitList=Benefits.Refresh(patPlanList,subList);
			List<Procedure> procList=Procedures.Refresh(patCur.PatNum);
			decimal subfee=0;
			decimal subpriIns=0;
			decimal subsecIns=0;
			decimal subdiscount=0;
			decimal subpat=0;
			decimal totFee=0;
			decimal totPriIns=0;
			decimal totSecIns=0;
			decimal totDiscount=0;
			decimal totPat=0;
			List<TpRow> rowsMain=new List<TpRow>();
			TpRow row;
			#region AnyTP
			//else {//any except current tp selected
				//ProcTP[] ProcTPSelectList=ProcTPs.GetListForTP(treatPlan.TreatPlanNum,procTPList);
				bool isDone;
				for(int i=0;i<treatPlan.ListProcTPs.Count;i++) {
					row=new TpRow();
					isDone=false;
					for(int j=0;j<procList.Count;j++) {
						if(procList[j].ProcNum==treatPlan.ListProcTPs[i].ProcNumOrig) {
							if(procList[j].ProcStatus==ProcStat.C) {
								isDone=true;
							}
						}
					}
					if(isDone) {
						row.Done="X";
					}
					row.Priority=DefC.GetName(DefCat.TxPriorities,treatPlan.ListProcTPs[i].Priority);
					row.Tth=treatPlan.ListProcTPs[i].ToothNumTP;
					row.Surf=treatPlan.ListProcTPs[i].Surf;
					row.Code=treatPlan.ListProcTPs[i].ProcCode;
					row.Description=treatPlan.ListProcTPs[i].Descript;
					row.Fee=(decimal)treatPlan.ListProcTPs[i].FeeAmt;//Fee
					subfee+=(decimal)treatPlan.ListProcTPs[i].FeeAmt;
					totFee+=(decimal)treatPlan.ListProcTPs[i].FeeAmt;
					row.PriIns=(decimal)treatPlan.ListProcTPs[i].PriInsAmt;//PriIns
					subpriIns+=(decimal)treatPlan.ListProcTPs[i].PriInsAmt;
					totPriIns+=(decimal)treatPlan.ListProcTPs[i].PriInsAmt;
					row.SecIns=(decimal)treatPlan.ListProcTPs[i].SecInsAmt;//SecIns
					subsecIns+=(decimal)treatPlan.ListProcTPs[i].SecInsAmt;
					totSecIns+=(decimal)treatPlan.ListProcTPs[i].SecInsAmt;
					row.Discount=(decimal)treatPlan.ListProcTPs[i].Discount;//Discount
					subdiscount+=(decimal)treatPlan.ListProcTPs[i].Discount;
					totDiscount+=(decimal)treatPlan.ListProcTPs[i].Discount;
					row.Pat=(decimal)treatPlan.ListProcTPs[i].PatAmt;//Pat
					subpat+=(decimal)treatPlan.ListProcTPs[i].PatAmt;
					totPat+=(decimal)treatPlan.ListProcTPs[i].PatAmt;
					row.Prognosis=treatPlan.ListProcTPs[i].Prognosis;//Prognosis
					row.ProcAbbr=treatPlan.ListProcTPs[i].ProcAbbr;//Abbr
					row.Dx=treatPlan.ListProcTPs[i].Dx;
					row.ColorText=DefC.GetColor(DefCat.TxPriorities,treatPlan.ListProcTPs[i].Priority);
					if(row.ColorText==System.Drawing.Color.White) {
						row.ColorText=System.Drawing.Color.Black;
					}
					row.Tag=treatPlan.ListProcTPs[i].Copy();
					rowsMain.Add(row);
					#region subtotal
					if(checkShowSubtotals &&
						(i==treatPlan.ListProcTPs.Count-1 || treatPlan.ListProcTPs[i+1].Priority != treatPlan.ListProcTPs[i].Priority)) {
						row=new TpRow();
						row.Description=Lan.g("TableTP","Subtotal");
						row.Fee=subfee;
						row.PriIns=subpriIns;
						row.SecIns=subsecIns;
						row.Discount=subdiscount;
						row.Pat=subpat;
						row.ColorText=DefC.GetColor(DefCat.TxPriorities,treatPlan.ListProcTPs[i].Priority);
						if(row.ColorText==System.Drawing.Color.White) {
							row.ColorText=System.Drawing.Color.Black;
						}
						row.Bold=true;
						row.ColorLborder=System.Drawing.Color.Black;
						rowsMain.Add(row);
						subfee=0;
						subpriIns=0;
						subsecIns=0;
						subdiscount=0;
						subpat=0;
					}
					#endregion
				}
			#endregion AnyTP except current
			#region Totals
			if(checkShowTotals) {
				row=new TpRow();
				row.Description=Lan.g("TableTP","Total");
				row.Fee=totFee;
				row.PriIns=totPriIns;
				row.SecIns=totSecIns;
				row.Discount=totDiscount;
				row.Pat=totPat;
				row.Bold=true;
				row.ColorText=System.Drawing.Color.Black;
				rowsMain.Add(row);
			}
			#endregion Totals
			foreach(TpRow tpRow in rowsMain){
				DataRow dRow=retVal.NewRow();
				dRow["Done"]                   =tpRow.Done;
				dRow["Priority"]               =tpRow.Priority;
				dRow["Tth"]                    =tpRow.Tth;
				dRow["Surf"]                   =tpRow.Surf;
				dRow["Code"]                   =tpRow.Code;
				//If any patient insplan allows subst codes (if !plan.CodeSubstNone) and the code has a valid substitution code, then indicate the substitution.
				//If it is not a valid substitution code or if none of the plans allow substitutions, leave the it blank.
				string subCode=ProcedureCodes.GetProcCode(tpRow.Code).SubstitutionCode;
				if(!ProcedureCodes.IsValidCode(subCode)) {
					dRow["Sub"]="";
				}
				else { 
					dRow["Sub"]=insPlanList.Any(x=>!x.CodeSubstNone)?"X":"";//confusing double degative here; If any plan allows substitution, show X
				}
				dRow["Description"]            =tpRow.Description;
				if(PrefC.GetBool(PrefName.TreatPlanItemized) 
					|| tpRow.Description==Lan.g("TableTP","Subtotal") || tpRow.Description==Lan.g("TableTP","Total")) 
				{
					dRow["Fee"]                  =tpRow.Fee.ToString("F");
					dRow["Pri Ins"]              =tpRow.PriIns.ToString("F");
					dRow["Sec Ins"]              =tpRow.SecIns.ToString("F");
					dRow["Discount"]             =tpRow.Discount.ToString("F");
					dRow["Pat"]                  =tpRow.Pat.ToString("F");
				}
				dRow["Prognosis"]              =tpRow.Prognosis;
				dRow["Dx"]                     =tpRow.Dx;
				dRow["Abbr"]                   =tpRow.ProcAbbr;
				dRow["paramTextColor"]         =tpRow.ColorText.ToArgb();
				dRow["paramIsBold"]            =tpRow.Bold;
				dRow["paramIsBorderBoldBottom"]=tpRow.Bold;
				retVal.Rows.Add(dRow);
			}
			return retVal;
		}

		private static DataTable getTable_TreatPlanBenefitsFamily(Sheet sheet) {
			TreatPlan treatPlan=(TreatPlan)SheetParameter.GetParamByName(sheet.Parameters,"TreatPlan").ParamValue;
			bool checkShowIns=(bool)SheetParameter.GetParamByName(sheet.Parameters,"checkShowIns").ParamValue;
			//Note this logic was ported from ContrTreat.cs and is intended to emulate the way ContrTreat.CreateDocument created the insurance benefit table
			//Construct empty Data table ===============================================================================
			DataTable retVal=new DataTable();
			retVal.Columns.AddRange(new[] {
				new DataColumn("BenefitName",typeof(string)),
				new DataColumn("Primary",typeof(string)),
				new DataColumn("Secondary",typeof(string))
			});
			if(!checkShowIns) {
				return retVal;
			}
			retVal.Rows.Add("Family Maximum","","");
			retVal.Rows.Add("Family Deductible","","");
			Patient patCur=Patients.GetPat(treatPlan.PatNum);
			if(treatPlan.PatNum==0 || patCur==null) {
				return retVal;//return an empty data table that has the correct format.
			}
			//Fill data table if neccessary ===============================================================================
			Family famCur=Patients.GetFamily(patCur.PatNum);
			List<InsSub> subList=InsSubs.RefreshForFam(famCur);
			List<InsPlan> insPlanList=InsPlans.RefreshForSubList(subList);
			List<PatPlan> patPlanList=PatPlans.Refresh(patCur.PatNum);
			List<Benefit> benefitList=Benefits.Refresh(patPlanList,subList);
			for(int i=0;i<patPlanList.Count && i<2;i++) {//limit to first 2 insplans
				InsSub subCur=InsSubs.GetSub(patPlanList[i].InsSubNum,subList);
				InsPlan planCur=InsPlans.GetPlan(subCur.PlanNum,insPlanList);
				double familyMax=Benefits.GetAnnualMaxDisplay(benefitList,planCur.PlanNum,patPlanList[i].PatPlanNum,true);
				if(!familyMax.IsEqual(-1)) {
					retVal.Rows[0][i+1]=familyMax.ToString("F");
				}
				double familyDed=Benefits.GetDeductGeneralDisplay(benefitList,planCur.PlanNum,patPlanList[i].PatPlanNum,BenefitCoverageLevel.Family);
				if(!familyDed.IsEqual(-1)) {
					retVal.Rows[1][i+1]=familyDed.ToString("F");
				}
			}
			return retVal;
		}

		private static DataTable getTable_TreatPlanBenefitsIndividual(Sheet sheet) {
			TreatPlan treatPlan=(TreatPlan)SheetParameter.GetParamByName(sheet.Parameters,"TreatPlan").ParamValue;
			bool checkShowIns=(bool)SheetParameter.GetParamByName(sheet.Parameters,"checkShowIns").ParamValue;
			//Note this logic was ported from ContrTreat.cs and is intended to emulate the way ContrTreat.CreateDocument created the insurance benefit table
			//Construct empty Data table ===============================================================================
			DataTable retVal=new DataTable();
			retVal.Columns.AddRange(new[] {
				new DataColumn("BenefitName",typeof(string)),
				new DataColumn("Primary",typeof(string)),
				new DataColumn("Secondary",typeof(string))
			});
			if(!checkShowIns) {
				return retVal;
			}
			Patient patCur=Patients.GetPat(treatPlan.PatNum);
			retVal.Rows.Add("Annual Maximum","","");
			retVal.Rows.Add("Deductible","","");
			retVal.Rows.Add("Deductible Remaining","","");
			retVal.Rows.Add("Insurance Used","","");
			retVal.Rows.Add("Pending","","");
			retVal.Rows.Add("Remaining","","");
			if(treatPlan.PatNum==0 || patCur==null) {
				return retVal;//return an empty data table that has the correct format.
			}
			//Fill data table if neccessary ===============================================================================
			Family famCur=Patients.GetFamily(patCur.PatNum);
			List<InsSub> subList=InsSubs.RefreshForFam(famCur);
			List<InsPlan> insPlanList=InsPlans.RefreshForSubList(subList);
			List<PatPlan> patPlanList=PatPlans.Refresh(patCur.PatNum);
			List<Benefit> benefitList=Benefits.Refresh(patPlanList,subList);
			List<ClaimProcHist> histList=ClaimProcs.GetHistList(patCur.PatNum,benefitList,patPlanList,insPlanList,DateTimeOD.Today,subList);
			for(int i=0;i<patPlanList.Count && i<2;i++){
				InsSub subCur=InsSubs.GetSub(patPlanList[i].InsSubNum,subList);
				InsPlan planCur=InsPlans.GetPlan(subCur.PlanNum,insPlanList);
				double pend=InsPlans.GetPendingDisplay(histList,DateTime.Today,planCur,patPlanList[i].PatPlanNum,-1,patCur.PatNum,patPlanList[i].InsSubNum,benefitList);
				double used=InsPlans.GetInsUsedDisplay(histList,DateTime.Today,planCur.PlanNum,patPlanList[i].PatPlanNum,-1,insPlanList,benefitList,patCur.PatNum,patPlanList[i].InsSubNum);
				retVal.Rows[3][i+1]=used.ToString("F");
				retVal.Rows[4][i+1]=pend.ToString("F");
				double maxInd=Benefits.GetAnnualMaxDisplay(benefitList,planCur.PlanNum,patPlanList[i].PatPlanNum,false);
				if(!maxInd.IsEqual(-1)) {
					double remain=maxInd-used-pend;
					if(remain<0) {
						remain=0;
					}
					retVal.Rows[0][i+1]=maxInd.ToString("F");
					retVal.Rows[5][i+1]=remain.ToString("F");
				}
				//deductible:
				double ded=Benefits.GetDeductGeneralDisplay(benefitList,planCur.PlanNum,patPlanList[i].PatPlanNum,BenefitCoverageLevel.Individual);
				double dedFam=Benefits.GetDeductGeneralDisplay(benefitList,planCur.PlanNum,patPlanList[i].PatPlanNum,BenefitCoverageLevel.Family);
				if(!ded.IsEqual(-1)) {
					double dedRem=InsPlans.GetDedRemainDisplay(histList,DateTime.Today,planCur.PlanNum,patPlanList[i].PatPlanNum,-1,insPlanList,patCur.PatNum,ded,dedFam);
					retVal.Rows[1][i+1]=ded.ToString("F");
					retVal.Rows[2][i+1]=dedRem.ToString("F");
				}
			}
			return retVal;
		}

		///<summary>Gets account tables by calling AccountModules.GetAccount and then appends dataRows together into a single table.
		///DataSet should be prefilled with AccountModules.GetAccount() before calling this method.</summary>
		private static DataTable getTable_StatementMain(DataSet dataSet,Statement stmt) {
			DataTable retVal=null;
			foreach(DataTable t in dataSet.Tables) {
				if(!t.TableName.StartsWith("account")) {
					continue;
				}
				if(retVal==null) {//first pass
					retVal=t.Clone();
				}
				foreach(DataRow r in t.Rows) {
					if(CultureInfo.CurrentCulture.Name.EndsWith("CA") && stmt.IsReceipt) {//Canadian. en-CA or fr-CA
						if(r["StatementNum"].ToString()!="0") {//Hide statement rows for Canadian receipts.
							continue;
						}
						if(r["ClaimNum"].ToString()!="0") {//Hide claim rows and claim payment rows for Canadian receipts.
							continue;
						}
						if(PIn.Long(r["ProcNum"].ToString())!=0){
							r["description"]="";//Description: blank in Canada normally because this information is used on taxes and is considered a security concern.
						}
						r["ProcCode"]="";//Code: blank in Canada normally because this information is used on taxes and is considered a security concern.
						r["tth"]="";//Tooth: blank in Canada normally because this information is used on taxes and is considered a security concern.
					}
					if(CultureInfo.CurrentCulture.Name=="en-US"	&& stmt.IsReceipt && r["PayNum"].ToString()=="0") {//Hide everything except patient payments
						continue;
						//js Some additional features would be nice for receipts, such as hiding the bal column, the aging, and the amount due sections.
					}
					//The old way of printing "Single patient only" receipts would simply show all rows from the "account" table in one grid for foreign users.
					//In order to keep this functionality for "Statements use Sheets" we need to force all rows to be associated to the stmt.PatNum.
					if(CultureInfo.CurrentCulture.Name!="en-US"
						&& !CultureInfo.CurrentCulture.Name.EndsWith("CA")
						&& stmt.IsReceipt
						&& stmt.SinglePatient) 
					{
						long patNumCur=PIn.Long(r["PatNum"].ToString());
						//If the PatNum column is valid and is for a different patient then force it to be for this patient so that it shows up in the same grid.
						if(patNumCur > 0 && patNumCur!=stmt.PatNum) {
							r["PatNum"]=POut.Long(stmt.PatNum);
						}
					}
					if(CultureInfo.CurrentCulture.Name=="en-AU" && r["prov"].ToString().Trim()!="") {//English (Australia)
						r["description"]=r["prov"]+" - "+r["description"];
					}
					retVal.ImportRow(r);
				}
				if(t.Rows.Count==0) {
					Patient p=Patients.GetPat(PIn.Long(t.TableName.Replace("account","")))??Patients.GetPat(stmt.PatNum);
					retVal.Rows.Add(
						"",//"AdjNum"          
						"",//"AbbrDesc"
						"",//"balance"         
						0,//"balanceDouble"   
						"",//"charges"         
						0,//"chargesDouble"   
						"",//"ClaimNum"        
						"",//"ClaimPaymentNum" 
						"",//"clinic"          
						"",//"colorText"       
						"",//"credits"         
						0,//"creditsDouble"   
						DateTime.Today.ToShortDateString(),//"date"            
						DateTime.Today,//"DateTime"        
						Lans.g("Statements","No Account Activity"),//"description"     
						p.FName,//"patient"         
						p.PatNum,//"PatNum"          
						0,//"PayNum"          
						0,//"PayPlanNum"      
						0,//"PayPlanChargeNum"
						"",//"ProcCode"        
						0,//"ProcNum"         
						0,//"ProcNumLab"      
						0,//"procsOnObj"      
						0,//"prov"            
						0,//"StatementNum"    
						"",//"ToothNum"        
						"",//"ToothRange"      
						""//"tth"       
						);
				}
			}
			return retVal;
		}

		private static DataTable getTable_StatementAging(Statement stmt) {
			DataTable retVal=new DataTable();
			if(stmt.SuperFamily!=0) {
				retVal.Columns.Add(new DataColumn("Account"));
			}
			retVal.Columns.Add(new DataColumn("Age00to30"));
			retVal.Columns.Add(new DataColumn("Age31to60"));
			retVal.Columns.Add(new DataColumn("Age61to90"));
			retVal.Columns.Add(new DataColumn("Age90plus"));
			if(stmt.SuperFamily!=0) {
				retVal.Columns.Add(new DataColumn("AcctTotal"));
			}
			DataRow row;
			if(stmt.SuperFamily!=0) {//Superfamily statement
				List<Patient> listSuperfamGuars=Patients.GetSuperFamilyGuarantors(stmt.SuperFamily)
					.FindAll(x => x.HasSuperBilling && new[] { x.Bal_0_30,x.Bal_31_60,x.Bal_61_90,x.BalOver90,x.BalTotal }.Any(y => y>0));
				foreach(Patient guarantor in listSuperfamGuars) {//seperate rows instead of summed into a single row for all families.
					row=retVal.NewRow();
					row[0]=guarantor.GetNameFL();
					row[1]=guarantor.Bal_0_30.ToString("F");
					row[2]=guarantor.Bal_31_60.ToString("F");
					row[3]=guarantor.Bal_61_90.ToString("F");
					row[4]=guarantor.BalOver90.ToString("F");
					row[5]=guarantor.BalTotal.ToString("F");
					retVal.Rows.Add(row);
				}
			}
			else {
				Patient guar=Patients.GetPat(Patients.GetPat(stmt.PatNum).Guarantor);
				row=retVal.NewRow();
				row[0]=guar.Bal_0_30.ToString("F");
				row[1]=guar.Bal_31_60.ToString("F");
				row[2]=guar.Bal_61_90.ToString("F");
				row[3]=guar.BalOver90.ToString("F");
				retVal.Rows.Add(row);
			}
			return retVal;
		}

		///<Summary>DataSet should be prefilled with AccountModules.GetAccount() before calling this method.</Summary>
		private static DataTable getTable_StatementPayPlan(DataSet dataSet) {
			DataTable retVal=new DataTable();
			foreach(DataTable t in dataSet.Tables) {
				if(!t.TableName.StartsWith("payplan")) {
					continue;
				}
				retVal=t.Clone();
				foreach(DataRow r in t.Rows) {
					retVal.ImportRow(r);
				}
			}
			return retVal;
		}

		///<Summary>DataSet should be prefilled with AccountModules.GetAccount() before calling this method.</Summary>
		private static DataTable getTable_StatementEnclosed(DataSet dataSet,Statement stmt) {
			int payPlanVersionCur=PrefC.GetInt(PrefName.PayPlansVersion);
			DataTable tableMisc=dataSet.Tables["misc"];
			string text="";
			DataTable table=new DataTable();
			table.Columns.Add(new DataColumn("AmountDue"));
			table.Columns.Add(new DataColumn("DateDue"));
			table.Columns.Add(new DataColumn("AmountEnclosed"));
			DataRow row=table.NewRow();
			#region Statement Type NotSet
			if(stmt.StatementType!=StmtType.LimitedStatement) {
				Patient patGuar=null;
				List<Patient> listSuperFamGuars=new List<Patient>();
				if(stmt.SuperFamily!=0) {//Superfamily statement
					patGuar=Patients.GetPat(Patients.GetPat(stmt.SuperFamily).Guarantor);
					listSuperFamGuars=Patients.GetSuperFamilyGuarantors(patGuar.SuperFamily).FindAll(x => x.HasSuperBilling);
				}
				else {
					patGuar=Patients.GetPat(Patients.GetPat(stmt.PatNum).Guarantor);
				}
				double balTotal=0;
				if(stmt.SuperFamily!=0) {//Superfamily statement
					double balCur;
					foreach(Patient guarantor in listSuperFamGuars) {
						balCur=guarantor.BalTotal;
						if(!PrefC.GetBool(PrefName.BalancesDontSubtractIns)) {
							balCur-=guarantor.InsEst;
						}
						if(balCur<=0) {//if this guarantor has a negative balance, don't subtract from the super statement amount due (Ryan says so)
							continue;
						}
						balTotal+=balCur;
					}
				}
				else {
					balTotal=patGuar.BalTotal;
					if(!PrefC.GetBool(PrefName.BalancesDontSubtractIns)) {
						balTotal-=patGuar.InsEst;
					}
				}
				for(int m=0;m<tableMisc.Rows.Count;m++) {
					//only add payplandue value to total balance in version 1 (version 2+ already account for it when calculating aging)
					if(tableMisc.Rows[m]["descript"].ToString()=="payPlanDue" && payPlanVersionCur==1) {
						balTotal+=PIn.Double(tableMisc.Rows[m]["value"].ToString());
						//payPlanDue;//PatGuar.PayPlanDue;
					}
				}
				if(stmt.SuperFamily!=0) {//Superfamily statement
					List<InstallmentPlan> listInstallPlans=InstallmentPlans.GetForSuperFam(patGuar.SuperFamily);
					if(listInstallPlans.Count>0) {
						double installPlanTotal=0;
						foreach(InstallmentPlan plan in listInstallPlans) {
							installPlanTotal+=plan.MonthlyPayment;
						}
						if(installPlanTotal < balTotal) {
							text=installPlanTotal.ToString("F");
						}
						else {
							text=balTotal.ToString("F");
						}
					}
					else {//No installment plans
						text=balTotal.ToString("F");
					}
				}
				else {
					InstallmentPlan installPlan=InstallmentPlans.GetOneForFam(patGuar.PatNum);
					if(installPlan!=null) {
						//show lesser of normal total balance or the monthly payment amount.
						if(installPlan.MonthlyPayment < balTotal) {
							text=installPlan.MonthlyPayment.ToString("F");
						}
						else {
							text=balTotal.ToString("F");
						}
					}
					else {//no installmentplan
						text=balTotal.ToString("F");
					}
				}
			}
			#endregion Statement Type NotSet
			#region Statement Type LimitedStatement
			else {
				double statementTotal=dataSet.Tables.OfType<DataTable>().Where(x => x.TableName.StartsWith("account"))
					.SelectMany(x => x.Rows.OfType<DataRow>())
					.Where(x => x["AdjNum"].ToString()!="0"//adjustments, may be charges or credits
						|| x["ProcNum"].ToString()!="0"//procs, will be charges with credits==0
						|| x["PayNum"].ToString()!="0"//patient payments, will be credits with charges==0
						|| x["ClaimPaymentNum"].ToString()!="0").ToList()//claimproc payments+writeoffs, will be credits with charges==0
					.Sum(x => PIn.Double(x["chargesDouble"].ToString())-PIn.Double(x["creditsDouble"].ToString()));//add charges-credits
				if(PrefC.GetBool(PrefName.BalancesDontSubtractIns)) {
					text=statementTotal.ToString("c");
				}
				else {
					double patInsEst=PIn.Double(tableMisc.Rows.OfType<DataRow>()
						.Where(x => x["descript"].ToString()=="patInsEst")
						.Select(x => x["value"].ToString()).FirstOrDefault());//safe, if string is blank or null PIn.Double will return 0
					text=(statementTotal-patInsEst).ToString("c");
				}
			}
			#endregion Statement Type LimitedStatement
			row[0]=text;
			if(PrefC.GetLong(PrefName.StatementsCalcDueDate)==-1) {
				text=Lans.g("Statements","Upon Receipt");
			}
			else {
				text=DateTime.Today.AddDays(PrefC.GetLong(PrefName.StatementsCalcDueDate)).ToShortDateString();
			}
			row[1]=text;
			row[2]="";
			table.Rows.Add(row);
			return table;
		}

		private static DataTable getTable_MedLabResults(MedLab medLab) {
			DataTable retval=new DataTable();
			retval.Columns.Add(new DataColumn("obsIDValue"));
			retval.Columns.Add(new DataColumn("obsAbnormalFlag"));
			retval.Columns.Add(new DataColumn("obsUnits"));
			retval.Columns.Add(new DataColumn("obsRefRange"));
			retval.Columns.Add(new DataColumn("facilityID"));
			List<MedLab> listMedLabs=MedLabs.GetForPatAndSpecimen(medLab.PatNum,medLab.SpecimenID,medLab.SpecimenIDFiller);//should always be at least one MedLab
			MedLabFacilities.GetFacilityList(listMedLabs,out _listResults);//refreshes and sorts the classwide _listResults variable
			string obsDescriptPrev="";
			for(int i=0;i<_listResults.Count;i++) {
				//LabCorp requested that these non-performance results not be displayed on the report
				if((_listResults[i].ResultStatus==ResultStatus.F || _listResults[i].ResultStatus==ResultStatus.X)
					&& _listResults[i].ObsValue==""
					&& _listResults[i].Note=="")
				{
					continue;
				}
				string obsDescript="";
				MedLab medLabCur=listMedLabs.FirstOrDefault(x => x.MedLabNum==_listResults[i].MedLabNum);
				if(i==0 || _listResults[i].MedLabNum!=_listResults[i-1].MedLabNum) {
					if(medLabCur!=null && medLabCur.ActionCode!=ResultAction.G) {
						if(obsDescriptPrev==medLabCur.ObsTestDescript) {
							obsDescript=".";
						}
						else {
							obsDescript=medLabCur.ObsTestDescript;
							obsDescriptPrev=obsDescript;
						}
					}
				}
				DataRow row=retval.NewRow();
				string spaces="  ";
				string spaces2="    ";
				string obsVal="";
				int padR=38;
				string newLine="";
				if(obsDescript!="") {
					if(obsDescript==_listResults[i].ObsText) {
						spaces="";
						spaces2="  ";
						padR=40;
					}
					else {
						obsVal+=obsDescript+"\r\n";
						newLine+="\r\n";
					}
				}
				if(_listResults[i].ObsValue=="Test Not Performed") {
					obsVal+=spaces+_listResults[i].ObsText;
				}
				else if(_listResults[i].ObsText=="."
					|| _listResults[i].ObsValue.Contains(":")
					|| _listResults[i].ObsValue.Length>20
					|| (medLabCur!=null && medLabCur.ActionCode==ResultAction.G))
				{
					obsVal+=spaces+_listResults[i].ObsText+"\r\n"+spaces2+_listResults[i].ObsValue.Replace("\r\n","\r\n"+spaces2);
					newLine+="\r\n";
				}
				else {
					obsVal+=spaces+_listResults[i].ObsText.PadRight(padR,' ')+_listResults[i].ObsValue;
				}
				if(_listResults[i].Note!="") {
					obsVal+="\r\n"+spaces2+_listResults[i].Note.Replace("\r\n","\r\n"+spaces2);
				}
				row["obsIDValue"]=obsVal;
				row["obsAbnormalFlag"]=newLine+MedLabResults.GetAbnormalFlagDescript(_listResults[i].AbnormalFlag);
				row["obsUnits"]=newLine+_listResults[i].ObsUnits;
				row["obsRefRange"]=newLine+_listResults[i].ReferenceRange;
				row["facilityID"]=newLine+_listResults[i].FacilityID;
				retval.Rows.Add(row);
			}
			return retval;
		}
	}
}
