using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CodeBase;
using OpenDentBusiness;
using System.Drawing.Printing;
using System.Data;
using System.Linq;
using OpenDental.UI;

namespace OpenDental {
	public partial class FormSheetFillEdit:ODForm {
		///<summary>Will be null if deleted.</summary>
		public Sheet SheetCur;
		private bool mouseIsDown;
		///<summary>A list of points for a line currently being drawn.  Once the mouse is raised, this list gets cleared.</summary>
		private List<Point> PointList;
		private PictureBox pictDraw;
		private Image imgDraw;
		private bool drawingsAltered;
		public bool RxIsControlled;
		///<summary>When in terminal, some options are not visible.</summary>
		public bool IsInTerminal;
		///<summary>If set to true, then this form will poll the database every 4 seconds, listening for a signal to close itself.  This is useful if the receptionist accidentally loads up the wrong patient.</summary>
		public bool TerminalListenShut;
		///<summary>Only used here to draw the dashed margin lines.</summary>
		private Margins _printMargin=new Margins(0,0,40,60);
		public bool IsStatement;//Used for statements, do not save a sheet version of the statement.
		public Statement Stmt;
		public MedLab MedLabCur;
		///<summary>Statements use Sheets needs access to the entire Account data set for measuring grids.  See RefreshPanel()</summary>
		private DataSet _dataSet;
		///<summary>If true, the sheet cannot be edited, deleted, changed patient, printed, or PDFed.</summary>
		public bool IsReadOnly;

		///<summary>Use this constructor when displaying a statement.  dataSet should be filled with the data set from AccountModules.GetAccount()</summary>
		public FormSheetFillEdit(Sheet sheet, DataSet dataSet=null){
			InitializeComponent();
			Lan.F(this);
			SheetCur=sheet;
			_dataSet=dataSet;
			if(sheet.IsLandscape){
				Width=sheet.Height+190;
				Height=sheet.Width+65;
			}
			else{
				Width=sheet.Width+190;
				Height=sheet.Height+65;
			}
			if(Width>SystemInformation.WorkingArea.Width){
				Width=SystemInformation.WorkingArea.Width;
			}
			if(Height>SystemInformation.WorkingArea.Height){
				Height=SystemInformation.WorkingArea.Height;
			}
			PointList=new List<Point>();
		}

		private void FormSheetFillEdit_Load(object sender,EventArgs e) {
			Sheets.SetPageMargin(SheetCur,_printMargin);
			if(IsInTerminal) {
				labelDateTime.Visible=false;
				textDateTime.Visible=false;
				labelDescription.Visible=false;
				textDescription.Visible=false;
				labelNote.Visible=false;
				textNote.Visible=false;
				labelShowInTerminal.Visible=false;
				textShowInTerminal.Visible=false;
				butPrint.Visible=false;
				butPDF.Visible=false;
				butDelete.Visible=false;
				butChangePat.Visible=false;
				if(TerminalListenShut) {
					timer1.Enabled=true;
				}
			}
			if(SheetCur.IsLandscape){
				panelMain.Width=SheetCur.Height;//+20 for VScrollBar
				panelMain.Height=SheetCur.Width;
			}
			else{
				panelMain.Width=SheetCur.Width;
				panelMain.Height=SheetCur.Height;
			}
			if(IsStatement) {
				labelDateTime.Visible=false;
				textDateTime.Visible=false;
				labelDescription.Visible=false;
				textDescription.Visible=false;
				labelNote.Visible=false;
				textNote.Visible=false;
				labelShowInTerminal.Visible=false;
				textShowInTerminal.Visible=false;
#if !DEBUG
				butPrint.Visible=false;
				butPDF.Visible=false;
#endif
				butDelete.Visible=false;
				butOK.Visible=false;
				butChangePat.Visible=false;
				checkErase.Visible=false;
				butCancel.Text="Close";
			}
			if(SheetCur.SheetType==SheetTypeEnum.PaymentPlan) {//hide buttons if PP sheet type
				labelShowInTerminal.Visible=false;
				textShowInTerminal.Visible=false;
				textNote.Visible=false;
				labelNote.Visible=false;
				butOK.Visible=false;
				butDelete.Visible=false;
				butChangePat.Visible=false;
				butSaveSignature.Visible=true;
			}
			//Some controls may be on subsequent pages if the SheetFieldDef is multipage.
			panelMain.Height=Math.Max(SheetCur.HeightPage,SheetCur.HeightLastField+20);//+20 for Hscrollbar
			textDateTime.Text=SheetCur.DateTimeSheet.ToShortDateString()+" "+SheetCur.DateTimeSheet.ToShortTimeString();
			textDescription.Text=SheetCur.Description;
			textNote.Text=SheetCur.InternalNote;
			if(SheetCur.ShowInTerminal>0) {
				textShowInTerminal.Text=SheetCur.ShowInTerminal.ToString();
			}
			LayoutFields();
			if(IsReadOnly) {
				butOK.Enabled=false;
				butDelete.Enabled=false;
				butChangePat.Enabled=false;
				butPrint.Enabled=false;
				butPDF.Enabled=false;
			}
			if(SheetCur.IsNew && SheetCur.SheetType!=SheetTypeEnum.PaymentPlan) {//payplan does not get saved to db so sheet is always new
				return;
			}
			//from here on, only applies to existing sheets.
			if(!Security.IsAuthorized(Permissions.SheetEdit,SheetCur.DateTimeSheet)){
				panelMain.Enabled=false;
				butOK.Enabled=false;
				butDelete.Enabled=false;
				butChangePat.Enabled=false;
				return;
			}
			if(SheetCur.IsDeleted && !IsStatement && !IsInTerminal) {
				butDelete.Visible=false;
				butRestore.Visible=true;
			}
			//So user has permission
			bool isSigned=false;
			for(int i=0;i<SheetCur.SheetFields.Count;i++) {
				if(SheetCur.SheetFields[i].FieldType==SheetFieldType.SigBox
					&& SheetCur.SheetFields[i].FieldValue.Length>1) 
				{
					isSigned=true;
					break;
				}
			}
			if(SheetCur.SheetType==SheetTypeEnum.PaymentPlan) {
				PayPlan payPlan=(PayPlan)SheetParameter.GetParamByName(SheetCur.Parameters,"payplan").ParamValue;
				if(payPlan.Signature!="" && (payPlan.Signature!=null || !payPlan.IsNew)) {
					foreach(Control control in panelMain.Controls) {
						if(control.GetType()!=typeof(SignatureBoxWrapper)) {
							continue;
						}
						if(control.Tag==null) {
							continue;
						}
						//SheetField field;
						//field=(SheetField)control.Tag;
						OpenDental.UI.SignatureBoxWrapper sigBox=(OpenDental.UI.SignatureBoxWrapper)control;
						butUnlock.Visible=true;
						butSaveSignature.Visible=false;
						sigBox.Enabled=false;
					}
				}
			}
			if(isSigned) {
				panelMain.Enabled=false;
				butUnlock.Visible=true;
			}
			Plugins.HookAddCode(this, "FormSheetFillEdit_Load_End");
		}

		///<summary>Runs as the final step of loading the form, and also immediately after fields are moved down due to growth.</summary>
		private void LayoutFields() {
			List<SheetField> fieldsSorted=new List<SheetField>();
			fieldsSorted.AddRange(SheetCur.SheetFields);//Creates a sortable list that will not change sort order of the original list.
			fieldsSorted.Sort(SheetFields.SortDrawingOrderLayers);
			panelMain.Controls.Clear();
			RichTextBox textbox;//has to be richtextbox due to MS bug that doesn't show cursor.
			SheetCheckBox checkbox;
			SheetComboBox comboBox;
			ScreenToothChart toothChart;
			//first, draw images---------------------------------------------------------------------------------------
			//might change this to only happen once when first loading form:
			if(pictDraw!=null) {
				if(panelMain.Controls.Contains(pictDraw)) {
					Controls.Remove(pictDraw);
				}
				pictDraw=null;
			}
			imgDraw=null;
			pictDraw=new PictureBox();
			if(SheetCur.IsLandscape) {
				//imgDraw=new Bitmap(SheetCur.Height,SheetCur.Width);
				pictDraw.Width=SheetCur.Height;
				pictDraw.Height=SheetCur.Width;
			}
			else {
				//imgDraw=new Bitmap(SheetCur.Width,SheetCur.Height);
				pictDraw.Width=SheetCur.Width;
				pictDraw.Height=SheetCur.Height;
			}
			if(Sheets.CalculatePageCount(SheetCur,_printMargin)==1){
				pictDraw.Height=SheetCur.HeightPage;//+10 for HScrollBar
			}
			else {
				int pageCount=0;
				pictDraw.Height=SheetPrinting.bottomCurPage(SheetCur.HeightLastField,SheetCur,out pageCount);
			}
			//imgDraw.Dispose();//dispose of old image before setting it to a new image.
			imgDraw=new Bitmap(pictDraw.Width,pictDraw.Height);
			pictDraw.Location=new Point(0,0);
			pictDraw.Image=(Image)imgDraw.Clone();
			pictDraw.SizeMode=PictureBoxSizeMode.StretchImage;
			panelMain.Controls.Add(pictDraw);
			panelMain.SendToBack();
			Graphics pictGraphics=Graphics.FromImage(imgDraw);
			SheetPrinting.DrawImages(SheetCur,pictGraphics,true);
			pictGraphics.Dispose();
			//Set mouse events for the pictDraw
			pictDraw.MouseDown+=new MouseEventHandler(pictDraw_MouseDown);
			pictDraw.MouseMove+=new MouseEventHandler(pictDraw_MouseMove);
			pictDraw.MouseUp+=new MouseEventHandler(pictDraw_MouseUp);
			//draw drawings, rectangles, and lines, special, and grids.-----------------------------------------------------------------------
			RefreshPanel();
			//draw textboxes----------------------------------------------------------------------------------------------
			foreach(SheetField field in SheetCur.SheetFields) {
				if(field.FieldType!=SheetFieldType.InputField
					&& field.FieldType!=SheetFieldType.OutputText
					&& field.FieldType!=SheetFieldType.StaticText) {
					continue;
				}
				//When filling the textbox here, we cannot clip the text in the textbox or else existing signatures will break.
				//The side affect of not clipping is that the edit window will look different if the text is larger than the textbox.
				//However, if the text is bigger than the textbox, the user can see the issue easily by editing the sheet def.
				textbox=GraphicsHelper.CreateTextBoxForSheetDisplay(field,false);
				textbox.TextChanged+=new EventHandler(text_TextChanged);
				panelMain.Controls.Add(textbox);
				textbox.BringToFront();
			}
			//draw checkboxes----------------------------------------------------------------------------------------------
			foreach(SheetField field in SheetCur.SheetFields) {
				if(field.FieldType!=SheetFieldType.CheckBox) {
					continue;
				}
				checkbox=new SheetCheckBox();
				if(field.FieldValue=="X") {
					checkbox.IsChecked=true;
				}
				checkbox.Location=new Point(field.XPos,field.YPos);
				checkbox.Width=field.Width;
				checkbox.Height=field.Height;
				checkbox.Tag=field;
				checkbox.MouseUp+=new MouseEventHandler(checkbox_MouseUp);
				checkbox.KeyUp+=new KeyEventHandler(checkbox_KeyUp);
				checkbox.TabStop=(field.TabOrder>0);
				checkbox.TabIndex=field.TabOrder;
				panelMain.Controls.Add(checkbox);
				checkbox.BringToFront();
			}
			//draw comboboxes---------------------------------------------------------------------------------------------------
			foreach(SheetField field in SheetCur.SheetFields) {
				if(field.FieldType!=SheetFieldType.ComboBox) {
					continue;
				}
				comboBox=new SheetComboBox(field.FieldValue);
				comboBox.Location=new Point(field.XPos,field.YPos);
				comboBox.BackColor=Color.FromArgb(245,245,200);
				comboBox.Width=field.Width;
				comboBox.Height=field.Height;
				comboBox.Tag=field;
				comboBox.MouseUp+=new MouseEventHandler(comboBox_MouseUp);
				panelMain.Controls.Add(comboBox);
				comboBox.BringToFront();
			}
			//draw toothcharts--------------------------------------------------------------------------------------------------
			foreach(SheetField field in SheetCur.SheetFields) {
				if(field.FieldType!=SheetFieldType.ScreenChart) {
					continue;
				}
				toothChart=new ScreenToothChart(field.FieldValue,field.FieldValue[0]=='1');//Need to pass in value here to set tooth chart items.
				toothChart.Location=new Point(field.XPos,field.YPos);
				toothChart.Width=field.Width;
				toothChart.Height=field.Height;
				toothChart.Tag=field;
				toothChart.Invalidate();
				panelMain.Controls.Add(toothChart);
				panelMain.Controls.SetChildIndex(toothChart,panelMain.Controls.Count-2);//Ensures it's in the right order but in front of the picture frame.
			}
			//draw signature boxes----------------------------------------------------------------------------------------------
			foreach(SheetField field in SheetCur.SheetFields) {
				if(field.FieldType!=SheetFieldType.SigBox) {
					continue;
				}
				if(SheetCur.SheetType==SheetTypeEnum.TreatmentPlan) {
					//==TG 01/12/2016: Removed helper function here after conversation with Ryan.  We never fill any signature boxes for treatment plans in this
					//form.  It is done in FormTPsign, or printed in SheetPrinting.
					MsgBox.Show(this,"Treatment Plan Signatures not currently supported in FormSheetFillEdit.  Contact Support.");
					break;
				}
				OpenDental.UI.SignatureBoxWrapper sigBox=new OpenDental.UI.SignatureBoxWrapper();
				sigBox.Location=new Point(field.XPos,field.YPos);
				sigBox.Width=field.Width;
				sigBox.Height=field.Height; 
				if(field.FieldValue.Length>0) {//a signature is present
					bool sigIsTopaz=(field.FieldValue[0]=='1');
					string signature="";
					if(field.FieldValue.Length>1) {
						signature=field.FieldValue.Substring(1);
					}
					string keyData=Sheets.GetSignatureKey(SheetCur);
					sigBox.FillSignature(sigIsTopaz,keyData,signature);
				}
				if(SheetCur.SheetType==SheetTypeEnum.PaymentPlan) {
					PayPlan payPlan=(PayPlan)SheetParameter.GetParamByName(SheetCur.Parameters,"payplan").ParamValue;
					if(payPlan.Signature!="") {//a PP sig is present
					string keyData=(string)SheetParameter.GetParamByName(SheetCur.Parameters,"keyData").ParamValue;
						sigBox.FillSignature(payPlan.SigIsTopaz,keyData,payPlan.Signature);
					}
				}
				sigBox.Tag=field;
				sigBox.TabStop=(field.TabOrder>0);
				sigBox.TabIndex=field.TabOrder;
				panelMain.Controls.Add(sigBox);
				sigBox.BringToFront();
				if(sigBox.IsValid && field.FieldValue.Length>0) {
					//According to this form's load function the only pre-requisite for being a "signed" sheet and locking it is that it loads with an existing signature.
					//Based on that if we get this far and there's actually a signature then it's "signed", but this only works with the first form load.
					textbox=new RichTextBox();
					textbox.BorderStyle=BorderStyle.None;
					textbox.TabStop=false;
					textbox.Location=new Point(field.XPos+1,field.YPos+field.Height-15);
					textbox.Width=field.Width-2;
					textbox.ScrollBars=RichTextBoxScrollBars.None;
					textbox.SelectionAlignment=HorizontalAlignment.Left;
					textbox.Text=Lan.g(this,"Signed")+": "+field.DateTimeSig.ToShortDateString()+" "+field.DateTimeSig.ToShortTimeString();
					textbox.Multiline=false;
					textbox.Height=14;
					textbox.ReadOnly=true;
					panelMain.Controls.Add(textbox);
					textbox.BringToFront();
				}
			}
		}

		///<summary>Only if TerminalListenShut.  Occurs every 4 seconds. Checks database for status changes.  Shouldn't happen very often, because it means user will lose all data that they may have entered.</summary>
		private void timer1_Tick(object sender,EventArgs e) {
			TerminalActive terminal=TerminalActives.GetTerminal(Environment.MachineName);
			if(terminal==null) {//no terminal is supposed to be running here.
				DialogResult=DialogResult.Cancel;
				return;
			}
			if(terminal.PatNum==SheetCur.PatNum) {
				return;
			}
			//So terminal.PatNum must either be 0 or be an entirely different patient.
			DialogResult=DialogResult.Cancel;
		}

		private void checkbox_MouseUp(object sender,MouseEventArgs e) {
			FieldValueChanged(sender);
		}

		private void checkbox_KeyUp(object sender,KeyEventArgs e) {
			FieldValueChanged(sender);
		}

		private void comboBox_MouseUp(object sender,MouseEventArgs e) {
			FieldValueChanged(sender);
		}

		private void text_TextChanged(object sender,EventArgs e) {
			FieldValueChanged(sender);
		}

		///<summary>Triggered when any field value changes.  Also causes fields to grow as needed and deselects other radiobuttons in a group.
		///Will clear all signature boxes if this is not a "new" sheet.</summary>
		private void FieldValueChanged(object sender) {
			foreach(Control control in panelMain.Controls){
				if(SheetCur.IsNew) {
					break;//The user is changing fields and may have already signed the sheet.  Don't make them re-sign it.
				}
				if(control.GetType()!=typeof(OpenDental.UI.SignatureBoxWrapper)){
					continue;
				}
				if(control.Tag==null){
					continue;
				}
				SheetField field=(SheetField)control.Tag;
				OpenDental.UI.SignatureBoxWrapper sigBox=(OpenDental.UI.SignatureBoxWrapper)control;
				//When field values change, the signature should not show as "invalid" but instead should just be cleared so that the user can re-sign.
				//sigBox.SetInvalid();
				sigBox.ClearSignature();//The user is purposefully "invalidating" the old signature by changing the contents of the sheet. 
			}
			if(sender.GetType()==typeof(SheetCheckBox)) {
				SheetCheckBox checkbox=(SheetCheckBox)sender;
				if(checkbox.Tag==null) {
					return;
				}
				if(!checkbox.IsChecked) {//if user unchecked a radiobutton, nothing else happens
					return;
				}
				SheetField fieldThis=(SheetField)checkbox.Tag;
				if(fieldThis.RadioButtonGroup=="" && fieldThis.RadioButtonValue==""){//if it's a checkbox instead of a radiobutton
					return;
				}
				foreach(Control control in panelMain.Controls) {//set some other radiobuttons to be not checked
					if(control.GetType()!=typeof(SheetCheckBox)) {
						continue;
					}
					if(control.Tag==null) {
						continue;
					}
					if(control==sender) {
						continue;
					}
					SheetField fieldOther=(SheetField)control.Tag;
					if(fieldThis.FieldName!=fieldOther.FieldName) {//different radio group
						continue;
					}
					//If both checkbox field names are set to "misc" then we instead use the RadioButtonGroup as the actual radio button group name.
					if(fieldThis.FieldName=="misc" && fieldThis.RadioButtonGroup!=fieldOther.RadioButtonGroup){
						continue;
					}
					((SheetCheckBox)control).IsChecked=false;
				}
				return;
			}
			if(sender.GetType() != typeof(RichTextBox)){
				//since CheckBoxes also trigger this event for sig invalid.
				return;
			}
			//everything below here is for growth calc.
			RichTextBox textBox=(RichTextBox)sender;
			//remember where we were
			int cursorPos=textBox.SelectionStart;
			//int boxX=textBox.Location.X;
			//int boxY=textBox.Location.Y;
			//string originalFieldValue=((SheetField)((RichTextBox)control).Tag).FieldValue;
			SheetField fld=(SheetField)textBox.Tag;
			if(fld.GrowthBehavior==GrowthBehaviorEnum.None){
				return;
			}
			fld.FieldValue=textBox.Text;
			FontStyle fontstyle=FontStyle.Regular;
			if(fld.FontIsBold){
				fontstyle=FontStyle.Bold;
			}
			Font font=new Font(fld.FontName,fld.FontSize,fontstyle);
			int calcH=GraphicsHelper.MeasureStringH(fld.FieldValue,font,fld.Width,fld.TextAlign);
				//(int)(g.MeasureString(fld.FieldValue,font,fld.Width).Height * 1.133f);//Seems to need 2 pixels per line of text to prevent hidden text due to scroll.
			calcH+=font.Height+2;//add one line just in case.
			if(calcH<=fld.Height){//no growth needed
				return;
			}
			//the field height needs to change, so:
			int amountOfGrowth=calcH-fld.Height;
			fld.Height=calcH;
			//Growth of entire form.
			pictDraw.Height+=amountOfGrowth;
			panelMain.Height+=amountOfGrowth;
			int h=imgDraw.Height+amountOfGrowth;
			int w=imgDraw.Width;
			imgDraw.Dispose();
			imgDraw=new Bitmap(w,h);
			FillFieldsFromControls();//We already changed the value of this field manually, 
				//but if the other fields don't get changed, they will erroneously 'reset'.
			if(fld.GrowthBehavior==GrowthBehaviorEnum.DownGlobal) {
				SheetUtil.MoveAllDownBelowThis(SheetCur,fld,amountOfGrowth);
			}
			else if(fld.GrowthBehavior==GrowthBehaviorEnum.DownLocal) {
				SheetUtil.MoveAllDownWhichIntersect(SheetCur,fld,amountOfGrowth);
			}
			LayoutFields();
			//find the original textbox, and put the cursor back where it belongs
			foreach(Control control in panelMain.Controls){
				if(control.GetType() == typeof(RichTextBox)){
					if((SheetField)(control.Tag)==fld){
						((RichTextBox)control).Select(cursorPos,0);
						((RichTextBox)control).Focus();
						//((RichTextBox)control).SelectionStart=cursorPos;
					}
				}
			}
		}

		/*private void panelDraw_Paint(object sender,PaintEventArgs e) {
			//e.Graphics.DrawLine(Pens.Black,0,0,300,300);
			for(int i=1;i<PointList.Count;i++){
				e.Graphics.DrawLine(Pens.Black,PointList[i-1].X,PointList[i-1].Y,PointList[i].X,PointList[i].Y);
			}
		}*/

		private void pictDraw_MouseDown(object sender,MouseEventArgs e) {
			mouseIsDown=true;
			if(checkErase.Checked){
				return;
			}
			PointList.Add(new Point(e.X,e.Y));
			drawingsAltered=true;
		}

		private void pictDraw_MouseEnter(object sender,EventArgs e) {

		}

		private void pictDraw_MouseLeave(object sender,EventArgs e) {

		}

		private void pictDraw_MouseMove(object sender,MouseEventArgs e) {
			if(!mouseIsDown){
				return;
			}
			if(checkErase.Checked){
				//look for any lines that intersect the "eraser".
				//since the line segments are so short, it's sufficient to check end points.
				//Point point;
				string[] xy;
				string[] pointStr;
				float x;
				float y;
				float dist;//the distance between the point being tested and the center of the eraser circle.
				float radius=8f;//by trial and error to achieve best feel.
				PointF eraserPt=new PointF(e.X+pictDraw.Left+8.49f,e.Y+pictDraw.Top+8.49f);
				foreach(SheetField field in SheetCur.SheetFields){
					if(field.FieldType!=SheetFieldType.Drawing){
						continue;
					}
					pointStr=field.FieldValue.Split(';');
					for(int p=0;p<pointStr.Length;p++){
						xy=pointStr[p].Split(',');
						if(xy.Length==2){
							x=PIn.Float(xy[0]);
							y=PIn.Float(xy[1]);
							dist=(float)Math.Sqrt(Math.Pow(Math.Abs(x-eraserPt.X),2)+Math.Pow(Math.Abs(y-eraserPt.Y),2));
							if(dist<=radius){//testing circle intersection here
								SheetCur.SheetFields.Remove(field);
								drawingsAltered=true;
								RefreshPanel();
								return;;
							}
						}
					}
				}	
				return;
			}
			PointList.Add(new Point(e.X,e.Y));
			//RefreshPanel();
			//just add the last line segment instead of redrawing the whole thing.
			Graphics g=Graphics.FromImage(pictDraw.Image);
			g.SmoothingMode=SmoothingMode.HighQuality;
			Pen pen=new Pen(Brushes.Black,2f);
			int i=PointList.Count-1;
			g.DrawLine(pen,PointList[i-1].X,PointList[i-1].Y,PointList[i].X,PointList[i].Y);
			pictDraw.Invalidate();
			g.Dispose();
		}

		private void pictDraw_MouseUp(object sender,MouseEventArgs e) {
			mouseIsDown=false;
			if(checkErase.Checked){
				return;
			}
			SheetField field=new SheetField();
			field.FieldType=SheetFieldType.Drawing;
			field.FieldName="";
			field.FieldValue="";
			for(int i=0;i<PointList.Count;i++){
				if(i>0){
					field.FieldValue+=";";
				}
				field.FieldValue+=(PointList[i].X+pictDraw.Left)+","+(PointList[i].Y+pictDraw.Top);
			}
			field.FontName="";
			field.RadioButtonValue="";
			SheetCur.SheetFields.Add(field);
			FieldValueChanged(sender);
			PointList.Clear();
			RefreshPanel();
		}

		private void RefreshPanel(){
			Image img=(Image)imgDraw.Clone();
			Graphics g=Graphics.FromImage(img);
			g.SmoothingMode=SmoothingMode.HighQuality;
			//g.CompositingQuality=CompositingQuality.Default;
			Pen pen=new Pen(Brushes.Black,2f);
			Pen pen2=new Pen(Brushes.Black,1f);
			string[] pointStr;
			List<Point> points;
			Point point;
			string[] xy;
			for(int f=0;f<SheetCur.SheetFields.Count;f++){
				if(SheetCur.SheetFields[f].FieldType==SheetFieldType.Drawing){
					pointStr=SheetCur.SheetFields[f].FieldValue.Split(';');
					points=new List<Point>();
					for(int p=0;p<pointStr.Length;p++){
						xy=pointStr[p].Split(',');
						if(xy.Length==2){
							point=new Point(PIn.Int(xy[0]),PIn.Int(xy[1]));
							points.Add(point);
						}
					}
					for(int i=1;i<points.Count;i++){
						g.DrawLine(pen,points[i-1].X-pictDraw.Left,
							points[i-1].Y-pictDraw.Top,
							points[i].X-pictDraw.Left,
							points[i].Y-pictDraw.Top);
					}
				}
				if(SheetCur.SheetFields[f].FieldType==SheetFieldType.Line){
					g.DrawLine((SheetCur.SheetFields[f].ItemColor==Color.FromArgb(0)?pen2:new Pen(SheetCur.SheetFields[f].ItemColor,1))
						,SheetCur.SheetFields[f].XPos-pictDraw.Left,
						SheetCur.SheetFields[f].YPos-pictDraw.Top,
						SheetCur.SheetFields[f].XPos+SheetCur.SheetFields[f].Width-pictDraw.Left,
						SheetCur.SheetFields[f].YPos+SheetCur.SheetFields[f].Height-pictDraw.Top);
				}
				if(SheetCur.SheetFields[f].FieldType==SheetFieldType.Rectangle){
					g.DrawRectangle((SheetCur.SheetFields[f].ItemColor==Color.FromArgb(0)?pen2:new Pen(SheetCur.SheetFields[f].ItemColor,1)),
						SheetCur.SheetFields[f].XPos-pictDraw.Left,
						SheetCur.SheetFields[f].YPos-pictDraw.Top,
						SheetCur.SheetFields[f].Width,
						SheetCur.SheetFields[f].Height);
				}
			}
			foreach(SheetField field in SheetCur.SheetFields.Where(x => x.FieldType==SheetFieldType.OutputText)) {//rectangles around specific output fields
				switch(SheetCur.SheetType.ToString()+"."+field.FieldName) {
					case "TreatmentPlan.Note":
						g.DrawRectangle(Pens.DarkGray,
							field.XPos-pictDraw.Left-1,
							field.YPos-pictDraw.Top-1,
							field.Width+2,
							field.Height+2);
						break;
				}
			}
			foreach(SheetField field in SheetCur.SheetFields.Where(x => x.FieldType==SheetFieldType.SigBox)) {//rectangles around specific output fields
				switch(SheetCur.SheetType) {
					case SheetTypeEnum.TreatmentPlan:
						g.DrawRectangle(Pens.Black,field.XPos-pictDraw.Left-1,field.YPos-pictDraw.Top-1,field.Width+2,field.Height+2);
						break;
				}
			}
			foreach(SheetField field in SheetCur.SheetFields.Where(x => x.FieldType==SheetFieldType.Special)) {
				SheetPrinting.drawFieldSpecial(SheetCur,field,g,null);
			}
			foreach(SheetField field in SheetCur.SheetFields.Where(x => x.FieldType==SheetFieldType.Grid)) {
				SheetPrinting.drawFieldGrid(field,SheetCur,g,null,_dataSet,Stmt,MedLabCur);
			}
			//Draw pagebreak
			Pen pDashPage=new Pen(Color.Green);
			pDashPage.DashPattern=new float[] { 4.0F,3.0F,2.0F,3.0F };
			Pen pDashMargin=new Pen(Color.Green);
			pDashMargin.DashPattern=new float[] { 1.0F,5.0F };
			int pageCount=Sheets.CalculatePageCount(SheetCur,_printMargin);
			int margins=(_printMargin.Top+_printMargin.Bottom);
			for(int i=1;i<pageCount;i++) {
				//g.DrawLine(pDashMargin,0,i*SheetCur.HeightPage-_printMargin.Bottom,SheetCur.WidthPage,i*SheetCur.HeightPage-_printMargin.Bottom);
				g.DrawLine(pDashPage,0,i*(SheetCur.HeightPage-margins)+_printMargin.Top,SheetCur.WidthPage,i*(SheetCur.HeightPage-margins)+_printMargin.Top);
				//g.DrawLine(pDashMargin,0,i*SheetCur.HeightPage+_printMargin.Top,SheetCur.WidthPage,i*SheetCur.HeightPage+_printMargin.Top);
			}//End Draw Page Break
			pictDraw.Image.Dispose();
			pictDraw.Image=img;
			g.Dispose();
		}

		private void checkErase_Click(object sender,EventArgs e) {
			if(checkErase.Checked){
				pictDraw.Cursor=new Cursor(GetType(),"EraseCircle.cur");
			}
			else{
				pictDraw.Cursor=Cursors.Default;
			}
		}

		private void panelColor_DoubleClick(object sender,EventArgs e) {

		}

		private void butPrint_Click(object sender,EventArgs e) {
			bool hasSheetFromDb=!IsStatement;//Statements are the only sheets that should not refresh from the db before printing.
			if(SheetCur.SheetType==SheetTypeEnum.PaymentPlan) {
				SaveSignaturePayPlan();
			}
			else {
				if(!IsStatement && !TryToSaveData()) {//short circuit with !IsStatement
					return;
				}
				if(hasSheetFromDb) {
					//We need to refresh SheetCur with the sheet from the database due to signature printing.
					//Without this line, a user could create a new sheet, sign it, click print and the signature would not show correctly.
					SheetCur=Sheets.GetSheet(SheetCur.SheetNum);
				}
			}
			//whether this is a new sheet, or one pulled from the database,
			//it will have the extra parameter we are looking for.
			//A new sheet will also have a PatNum parameter which we will ignore.
			FormSheetOutputFormat FormS=new FormSheetOutputFormat();
			if(SheetCur.SheetType==SheetTypeEnum.ReferralSlip
				|| SheetCur.SheetType==SheetTypeEnum.ReferralLetter)
			{
				FormS.PaperCopies=2;
			}
			else{
				FormS.PaperCopies=1;
			}
			if(SheetCur.PatNum!=0
				&& SheetCur.SheetType!=SheetTypeEnum.DepositSlip) 
			{
				Patient pat=Patients.GetPat(SheetCur.PatNum);
				if(SheetCur.SheetType==SheetTypeEnum.LabSlip) {
					FormS.IsForLab=true;//Changes label to "E-mail to Lab:"
					SheetParameter _paramLabCaseNum=SheetParameter.GetParamByName(SheetCur.Parameters,"LabCaseNum");//auto populate lab email.
					LabCase labCaseCur=LabCases.GetOne(PIn.Long(_paramLabCaseNum.ParamValue.ToString()));
					FormS.EmailPatOrLabAddress=Laboratories.GetOne(labCaseCur.LaboratoryNum).Email;
				}
				else if(pat.Email!="") {
					FormS.EmailPatOrLabAddress=pat.Email;
					//No need to email to a patient for sheet types: LabelPatient (0), LabelCarrier (1), LabelReferral (2), ReferralSlip (3), LabelAppointment (4), Rx (5), Consent (6), ReferralLetter (8), ExamSheet (13), DepositSlip (14)
					//The data is too private to email unencrypted for sheet types: PatientForm (9), RoutingSlip (10), MedicalHistory (11), LabSlip (12)
					//A patient might want email for the following sheet types and the data is not very private: PatientLetter (7)
					if(SheetCur.SheetType==SheetTypeEnum.PatientLetter) {
						//This just defines the default selection. The user can manually change selections in FormSheetOutputFormat.
						FormS.EmailPatOrLab=true;
						FormS.PaperCopies--;
					}
				}
			}
			Referral referral=null;
			if(SheetCur.SheetType==SheetTypeEnum.ReferralSlip
				|| SheetCur.SheetType==SheetTypeEnum.ReferralLetter)
			{
				FormS.Email2Visible=true;
				SheetParameter parameter=SheetParameter.GetParamByName(SheetCur.Parameters,"ReferralNum");
				if(parameter==null){//it can be null sometimes because of old bug in db.
					FormS.Email2Visible=false;//prevents trying to attach email to nonexistent referral.
				}
				else{
					long referralNum=PIn.Long(parameter.ParamValue.ToString());
					referral=Referrals.GetReferral(referralNum);
					if(referral.EMail!=""){
						FormS.Email2Address=referral.EMail;
						FormS.Email2=true;
						FormS.PaperCopies--;
					}
				}
			}
			else{
				FormS.Email2Visible=false;
			}
			FormS.ShowDialog();
			if(FormS.DialogResult!=DialogResult.OK) {
				//The user canceled out of printing.  Due to signature printing logic we had to update SheetCur to a new object directly from the database.
				//Therefore, all of the Tag objects on our controls are still linked up to the memory of the old SheetCur object.
				//Simply refresh the sheet which will link the controls back up to the current SheetCur object.
				if(hasSheetFromDb) {//Only re-layout the fields if we actually changed SheetCur to a new object from the db.
					LayoutFields();
				}
				return;
			}
			if(FormS.PaperCopies>0){
				SheetPrinting.Print(SheetCur,FormS.PaperCopies,RxIsControlled,Stmt,MedLabCur);
			}
			EmailMessage message;
			Random rnd=new Random();
			string attachPath=EmailAttaches.GetAttachPath();
			string fileName;
			string filePathAndName;
			EmailAddress emailAddress;
			Patient patCur=Patients.GetPat(SheetCur.PatNum);
			if(patCur==null) {
				emailAddress=EmailAddresses.GetByClinic(0);
			}
			else {
				emailAddress=EmailAddresses.GetByClinic(patCur.ClinicNum);
			}
			//Graphics g=this.CreateGraphics();
			if(FormS.EmailPatOrLab){
				if(!Security.IsAuthorized(Permissions.EmailSend,false)) {//Still need to return after printing, but not send emails.
					DialogResult=DialogResult.OK;
					return;
				}
				fileName=DateTime.Now.ToString("yyyyMMdd")+"_"+DateTime.Now.TimeOfDay.Ticks.ToString()+rnd.Next(1000).ToString()+".pdf";
				filePathAndName=ODFileUtils.CombinePaths(attachPath,fileName);
				SheetPrinting.CreatePdf(SheetCur,filePathAndName,Stmt,MedLabCur);
				//Process.Start(filePathAndName);
				message=new EmailMessage();
				message.PatNum=SheetCur.PatNum;
				message.ToAddress=FormS.EmailPatOrLabAddress;
				message.FromAddress=emailAddress.SenderAddress;//Can be blank just as it could with the old pref.
				message.Subject=SheetCur.Description.ToString();//this could be improved
				EmailAttach attach=new EmailAttach();
				string shortFileName=Regex.Replace(SheetCur.Description.ToString(), @"[^\w'@-_()&]", ""); 
				attach.DisplayedFileName=shortFileName+".pdf";
				attach.ActualFileName=fileName;
				message.Attachments.Add(attach);
				FormEmailMessageEdit FormE=new FormEmailMessageEdit(message);
				FormE.IsNew=true;
				FormE.ShowDialog();
			}
			if((SheetCur.SheetType==SheetTypeEnum.ReferralSlip
				|| SheetCur.SheetType==SheetTypeEnum.ReferralLetter)
				&& FormS.Email2)  
			{
				if(!Security.IsAuthorized(Permissions.EmailSend,false)) {//Still need to return after printing, but not send emails.
					DialogResult=DialogResult.OK;
					return;
				}
				//email referral
				fileName=DateTime.Now.ToString("yyyyMMdd")+"_"+DateTime.Now.TimeOfDay.Ticks.ToString()+rnd.Next(1000).ToString()+".pdf";
				filePathAndName=ODFileUtils.CombinePaths(attachPath,fileName);
				SheetPrinting.CreatePdf(SheetCur,filePathAndName,Stmt,MedLabCur);
				//Process.Start(filePathAndName);
				message=new EmailMessage();
				message.PatNum=SheetCur.PatNum;
				message.ToAddress=FormS.Email2Address;
				message.FromAddress=emailAddress.SenderAddress;//Can be blank just as it could with the old pref.
				message.Subject=Lan.g(this,"RE: ")+Patients.GetLim(SheetCur.PatNum).GetNameLF();//works even if patnum invalid
         //SheetCur.Description.ToString()+" to "+Referrals.GetNameFL(referral.ReferralNum);//this could be improved
				EmailAttach attach=new EmailAttach();
				string shortFileName=Regex.Replace(SheetCur.Description.ToString(), @"[^\w,'@-_()&]", "");
				attach.DisplayedFileName=shortFileName+".pdf";
				attach.ActualFileName=fileName;
				message.Attachments.Add(attach);
				FormEmailMessageEdit FormE=new FormEmailMessageEdit(message);
				FormE.IsNew=true;
				FormE.ShowDialog();
			}
			//g.Dispose();
			DialogResult=DialogResult.OK;
		}

		private void butPDF_Click(object sender,EventArgs e) {
			if(SheetCur.SheetType==SheetTypeEnum.PaymentPlan) {
				SaveSignaturePayPlan();
			}
			else {
				if(!IsStatement && !TryToSaveData()) {
					return;
				}
				if(!IsStatement && SheetCur.SheetType!=SheetTypeEnum.TreatmentPlan) {
					SheetCur=Sheets.GetSheet(SheetCur.SheetNum);
				}
			}
			string filePathAndName=PrefC.GetRandomTempFile(".pdf");
			//Graphics g=this.CreateGraphics();
			SheetPrinting.CreatePdf(SheetCur,filePathAndName,Stmt,MedLabCur);
			//g.Dispose();
			Process.Start(filePathAndName);
			SecurityLogs.MakeLogEntry(Permissions.SheetEdit,SheetCur.PatNum,SheetCur.Description+" from "+SheetCur.DateTimeSheet.ToShortDateString()+" pdf was created");
			DialogResult=DialogResult.OK;
		}

		private void butChangePat_Click(object sender,EventArgs e) {
			FormPatientSelect FormPS=new FormPatientSelect();
			FormPS.SelectionModeOnly=true;
			if(FormPS.ShowDialog()==DialogResult.OK) {
				SecurityLogs.MakeLogEntry(Permissions.SheetEdit,SheetCur.PatNum,Lan.g(this,"Sheet with ID")+" "+SheetCur.SheetNum+" "+Lan.g(this,"moved to PatNum")+" "+FormPS.SelectedPatNum);
				SecurityLogs.MakeLogEntry(Permissions.SheetEdit,FormPS.SelectedPatNum,Lan.g(this,"Sheet with ID")+" "+SheetCur.SheetNum+" "+Lan.g(this,"moved from PatNum")+" "+SheetCur.PatNum);
				SheetCur.PatNum=FormPS.SelectedPatNum;
			}
		}

		private void butUnlock_Click(object sender,EventArgs e) {
			//we already know the user has permission, because otherwise, button is not visible.
			if(SheetCur.SheetType==SheetTypeEnum.PaymentPlan) {
				PayPlan payPlan=(PayPlan)SheetParameter.GetParamByName(SheetCur.Parameters,"payplan").ParamValue;
				foreach(Control control in panelMain.Controls) {
					if(control.GetType()!=typeof(SignatureBoxWrapper)) {
						continue;
					}
					if(control.Tag==null) {
						continue;
					}
					OpenDental.UI.SignatureBoxWrapper sigBox=(OpenDental.UI.SignatureBoxWrapper)control;
					sigBox.Enabled=true;
				}
				butSaveSignature.Visible=true;
			}
			panelMain.Enabled=true;
			butUnlock.Visible=false;
		}

		private bool TryToSaveData() {
			if(!butOK.Enabled) {//if the OK button is not enabled, user does not have permission.
				return true;
			}
			if(textShowInTerminal.errorProvider1.GetError(textShowInTerminal)!="") {
				MsgBox.Show(this,"Please fix data entry errors first.");
				return false;
			}
			DateTime dateTimeSheet=DateTime.MinValue;
			try {
				dateTimeSheet=DateTime.Parse(textDateTime.Text);
			}
			catch(Exception ex) {
				MsgBox.Show(this,"Please fix data entry errors first.");
				return false;
			}
			SheetCur.DateTimeSheet=dateTimeSheet;
			SheetCur.Description=textDescription.Text;
			SheetCur.InternalNote=textNote.Text;
			SheetCur.ShowInTerminal=PIn.Byte(textShowInTerminal.Text);
			FillFieldsFromControls(true);//But SheetNums will still be 0 for a new sheet.
			if(SheetCur.IsNew) {
				Sheets.Insert(SheetCur);
				Sheets.SaveParameters(SheetCur);
			}
			else {
				Sheets.Update(SheetCur);
			}
			SheetCur.SheetFields.ForEach(x => x.SheetNum=SheetCur.SheetNum);//Set all sheetfield.SheetNums
			SheetFields.Sync(SheetCur.SheetFields.FindAll(x => x.FieldType!=SheetFieldType.SigBox),SheetCur.SheetNum,false);//Sync fields before sigBoxes
			List<SheetField> listSheetFields=new List<SheetField>();
			//SigBoxes must come after ALL other types in order for the keyData to be in the right order.
			SheetField field;
			foreach(Control control in panelMain.Controls) {
				if(control.GetType()!=typeof(OpenDental.UI.SignatureBoxWrapper)) {
					continue;
				}
				if(control.Tag==null) {
					continue;
				}
				field=(SheetField)control.Tag;
				OpenDental.UI.SignatureBoxWrapper sigBox=(OpenDental.UI.SignatureBoxWrapper)control;
				if(!sigBox.GetSigChanged()) {
					//signature hasn't changed, add to the list of fields
					listSheetFields.Add(field);
					continue;
				}
				//signature changed so clear out the current field data from the control.Tag, get the new keyData, and get the signature string for the field
				field.FieldValue="";
				//refresh the fields so they are in the correct order
				SheetFields.GetFieldsAndParameters(SheetCur);
				string keyData=Sheets.GetSignatureKey(SheetCur);
				string signature=sigBox.GetSignature(keyData);
				field.DateTimeSig=DateTime.MinValue;
				if(signature!="") {
					//This line of code is more readable, but uses ternary operator
					//field.FieldValue=(sigBox.GetSigIsTopaz()?1:0)+signature;
					field.FieldValue="0";
					if(sigBox.GetSigIsTopaz()) {
						field.FieldValue="1";
					}
					field.FieldValue+=signature;
					if(sigBox.IsValid) {
						//Save date of modified signature in the sheetfield here.
						field.DateTimeSig=DateTime.Now;
					}
				}
				listSheetFields.Add(field);
			}
			//now sync SigBoxes
			SheetFields.Sync(listSheetFields,SheetCur.SheetNum,true);
			return true;
		}

		///<summary>This is always done before the save process.  But it's also done before bumping down fields due to growth behavior.</summary>
		private void FillFieldsFromControls(bool isSave=false){			
			//SheetField field;
			//Images------------------------------------------------------
				//Images can't be changed in this UI
			//RichTextBoxes-----------------------------------------------
			foreach(Control control in panelMain.Controls){
				if(control.GetType()!=typeof(RichTextBox)){
					continue;
				}
				if(control.Tag==null){
					continue;
				}
				//field=(SheetField)control.Tag;
				((SheetField)control.Tag).FieldValue=control.Text;
			}
			//CheckBoxes-----------------------------------------------
			foreach(Control control in panelMain.Controls){
				if(control.GetType()!=typeof(SheetCheckBox)){
					continue;
				}
				if(control.Tag==null){
					continue;
				}
				//field=(SheetField)control.Tag;
				if(((SheetCheckBox)control).IsChecked){
					((SheetField)control.Tag).FieldValue="X";
				}
				else{
					((SheetField)control.Tag).FieldValue="";
				}
			}
			//ComboBoxes-----------------------------------------------
			foreach(Control control in panelMain.Controls) {
				if(control.GetType()!=typeof(SheetComboBox)) {
					continue;
				}
				if(control.Tag==null) {
					continue;
				}
				SheetComboBox comboBox=(SheetComboBox)control;
				//FieldValue will contain the selected option, followed by a semicolon, followed by a | delimited list of all options.
				((SheetField)control.Tag).FieldValue=comboBox.SelectedOption+";"+String.Join("|",comboBox.ComboOptions);
			}
			//ToothChart------------------------------------------------
			foreach(Control control in panelMain.Controls) {
				if(control.GetType()!=typeof(ScreenToothChart)) {
					continue;
				}
				if(control.Tag==null) {
					continue;
				}
				ScreenToothChart toothChart=(ScreenToothChart)control;
				List<UserControlScreenTooth> listTeeth=null;
				if(toothChart.IsPrimary) {
					listTeeth=toothChart.GetPrimaryTeeth;
				}
				else {
					listTeeth=toothChart.GetTeeth;
				}
				string value="";
				if(toothChart.IsPrimary) {
					value+="1;";
				}
				else {
					value+="0;";
				}
				for(int i=0;i<listTeeth.Count;i++) {
					if(i > 0) {
						value+=";";//Don't add ';' at very end or it will mess with .Split() logic.
					}
					value+=String.Join(",",listTeeth[i].GetSelected());
				}
				((SheetField)control.Tag).FieldValue=value;
			}
			//Rectangles and lines-----------------------------------------
				//Rectangles and lines can't be changed in this UI
			//Drawings----------------------------------------------------
				//Drawings data is already saved to fields
			//SigBoxes---------------------------------------------------
				//SigBoxes won't be strictly checked for validity
				//or data saved to the field until it's time to actually save to the database.
		}

		///<summary>Returns true when all of the sheet fields with IsRequired set to true have a value set. Otherwise, a message box shows and false is returned.</summary>
		private bool VerifyRequiredFields(){
			FillFieldsFromControls();
			foreach(Control control in panelMain.Controls){
				if(control.Tag==null){
					continue;
				}
				if(control.GetType()==typeof(RichTextBox)){
					SheetField field=(SheetField)control.Tag;
					if(field.FieldType!=SheetFieldType.InputField){
						continue;
					}
					RichTextBox inputBox=(RichTextBox)control;
					if(field.IsRequired && inputBox.Text.Trim()==""){
						MessageBox.Show(Lan.g(this,"You must enter a value for")+" "+field.FieldName+" "+Lan.g(this,"before continuing."));
						return false;			
					}	
				}
				else if(control.GetType()==typeof(OpenDental.UI.SignatureBoxWrapper)){
					SheetField field=(SheetField)control.Tag;
					if(field.FieldType!=SheetFieldType.SigBox){
						continue;
					}
					OpenDental.UI.SignatureBoxWrapper sigBox=(OpenDental.UI.SignatureBoxWrapper)control;
					if(field.IsRequired && (!sigBox.IsValid || sigBox.SigIsBlank)){
						MsgBox.Show(this,"Signature required");
						return false;
					}
				}
				else if(control.GetType()==typeof(SheetCheckBox)){//Radio button groups or misc checkboxes
					SheetField field=(SheetField)control.Tag;
					if(field.IsRequired && field.FieldValue!="X"){//required but this one not checked
						//first, checkboxes that are not radiobuttons.  For example, a checkbox at bottom of web form used in place of signature.
						if(field.RadioButtonValue=="" //doesn't belong to a built-in group
							&& field.RadioButtonGroup=="") //doesn't belong to a custom group
						{
							//field.FieldName is always "misc"
							//int widthActual=(SheetCur.IsLandscape?SheetCur.Height:SheetCur.Width);
							//int heightActual=(SheetCur.IsLandscape?SheetCur.Width:SheetCur.Height);
							//int topMidBottom=(heightActual/3)
							MessageBox.Show(Lan.g(this,"You must check the required checkbox."));
							return false;
						}
						else{//then radiobuttons (of both kinds)
							//All radio buttons within a group should either all be marked required or all be marked not required. 
							//Not the most efficient check, but there won't usually be more than a few hundred items so the user will not ever notice. We can speed up later if needed.
							bool valueSet=false;//we will be checking to see if at least one in the group has a value
							int numGroupButtons=0;//a count of the buttons in the group
							foreach(Control control2 in panelMain.Controls){//loop through all controls in the sheet
								if(control2.GetType()!=typeof(SheetCheckBox)){
									continue;//skip everything that's not a checkbox
								}
								SheetField field2=(SheetField)control2.Tag;
								//whether built-in or custom, this makes sure it's a match.
								//the other comparison will also match because they are empty strings
								if(field2.RadioButtonGroup.ToLower()==field.RadioButtonGroup.ToLower()//if they are in the same group ("" for built-in, some string for custom group)
									&& field2.FieldName==field.FieldName)//"misc" for custom group, some string for built in groups.
								{
									numGroupButtons++;
									if(field2.FieldValue=="X"){
										valueSet=true;
										break;
									}
								}
							}
							if(numGroupButtons>0 && !valueSet){//there is not at least one radiobutton in the group that's checked.
								if(field.RadioButtonGroup!="") {//if they are in a custom group
									MessageBox.Show(Lan.g(this,"You must select a value for radio button group")+" '"+field.RadioButtonGroup+"'. ");
								}
								else {
									MessageBox.Show(Lan.g(this,"You must select a value for radio button group")+" '"+field.FieldName+"'. ");
								}
								return false;
							}
						}
					}
				}
			}
			return true;
		}

		private void SaveSignaturePayPlan() {
			PayPlan payPlan=(PayPlan)SheetParameter.GetParamByName(SheetCur.Parameters,"payplan").ParamValue;
			string keyData=(string)SheetParameter.GetParamByName(SheetCur.Parameters,"keyData").ParamValue;
			bool isValidSig=true;
			bool sigChanged=false;
			foreach(Control control in panelMain.Controls) {
				if(control.GetType()!=typeof(SignatureBoxWrapper)) {
					continue;
				}
				if(control.Tag==null) {
					continue;
				}
				OpenDental.UI.SignatureBoxWrapper sigBox=(OpenDental.UI.SignatureBoxWrapper)control;
				if(!sigBox.IsValid) {//invalid sig. Do not save signature.
					isValidSig=sigBox.IsValid; 
					continue;
				}
				payPlan.Signature=sigBox.GetSignature(keyData);
				if(payPlan.Signature!="") {
					payPlan.SigIsTopaz=false;
					if(sigBox.GetSigIsTopaz()) {
						payPlan.SigIsTopaz=true; ;
					}
				}
				sigChanged=sigBox.GetSigChanged();
				PayPlans.Update(payPlan);
			}
			//save .pdf file if payPlan is new or signature has changed and signature is valid
			if((payPlan.IsNew || sigChanged) && isValidSig) {
				long category=0;
				//Determine the first category that this PP should be saved to.
				//"A"==payplan; see FormDefEditImages.cs
				//look at ContrTreat.cs to change it to handle more than one
				for(int i = 0;i<DefC.Short[(int)DefCat.ImageCats].Length;i++) {
					if(Regex.IsMatch(DefC.Short[(int)DefCat.ImageCats][i].ItemValue,@"A")) {
						category=DefC.Short[(int)DefCat.ImageCats][i].DefNum;
						break;
					}
				}
				if(category==0) {
					if(DefC.Short[(int)DefCat.ImageCats].Length!=0) {
						category=DefC.Short[(int)DefCat.ImageCats][0].DefNum;//put it in the first category.
					}
					else if(DefC.Long[(int)DefCat.ImageCats].Length!=0) {//All categories are hidden
						category=DefC.Long[(int)DefCat.ImageCats][0].DefNum;//put it in the first category.
					}
					else {
						MsgBox.Show(this,"Error saving document. Unable to find image category.");
						return;
					}
				}
				string filePathAndName=PrefC.GetRandomTempFile(".pdf");
				SheetPrinting.CreatePdf(SheetCur,filePathAndName,null);
				//create doc--------------------------------------------------------------------------------------
				OpenDentBusiness.Document docc=null;
				try {
					docc=ImageStore.Import(filePathAndName,category,Patients.GetPat(payPlan.PatNum));
				}
				catch {
					MsgBox.Show(this,"Error saving document.");
					return;
				}
				docc.Description="PPArchive"+docc.DocNum+"_"+DateTime.Now.ToShortDateString();
				docc.ImgType=ImageType.Document;
				docc.DateCreated=DateTime.Now;
				Documents.Update(docc);
				//remove temp file---------------------------------------------------------------------------------
				try {
					System.IO.File.Delete(filePathAndName);
				}
				catch { }
			}			
		}

		private void FormSheetFillEdit_FormClosing(object sender,FormClosingEventArgs e) {
			if(imgDraw!=null) {
				imgDraw.Dispose();
				imgDraw=null;
			}
			if(pictDraw!=null) {
				pictDraw.Dispose();
				pictDraw=null;
			}
			GC.Collect();
		}

		private void butDelete_Click(object sender,EventArgs e) {
			if(SheetCur.IsNew){
				DialogResult=DialogResult.Cancel;
				return;
			}
			if(!MsgBox.Show(this,true,"Delete?")){
				return;
			}
			if(SheetCur.SheetType==SheetTypeEnum.Screening) {
				Screens.DeleteForSheet(SheetCur.SheetNum);
			}
			Sheets.Delete(SheetCur.SheetNum);
			SecurityLogs.MakeLogEntry(Permissions.SheetEdit,SheetCur.PatNum,SheetCur.Description
				+" "+Lan.g(this,"deleted from")+" "+SheetCur.DateTimeSheet.ToShortDateString());
			SheetCur=null;
			DialogResult=DialogResult.OK;
		}

		private void butRestore_Click(object sender,EventArgs e) {
			SheetCur.IsDeleted=false;
			if(!VerifyRequiredFields()){
				return;
			}
			if(!TryToSaveData()){
				return;
			}
			SecurityLogs.MakeLogEntry(Permissions.SheetEdit,SheetCur.PatNum,SheetCur.Description+" from "+SheetCur.DateTimeSheet.ToShortDateString());
			DialogResult=DialogResult.OK;
		}

		private void butOK_Click(object sender,EventArgs e) {
			if(!VerifyRequiredFields()){
				return;
			}
			if(!TryToSaveData()){
				return;
			}
			SecurityLogs.MakeLogEntry(Permissions.SheetEdit,SheetCur.PatNum,SheetCur.Description+" from "+SheetCur.DateTimeSheet.ToShortDateString());
			DialogResult=DialogResult.OK;
		}

		private void butCancel_Click(object sender,EventArgs e) {
			DialogResult=DialogResult.Cancel;
		}

		private void butSaveSig(object sender,EventArgs e) {
			if(!VerifyRequiredFields()) {
				return;
			}
			SaveSignaturePayPlan();
			SecurityLogs.MakeLogEntry(Permissions.SheetEdit,SheetCur.PatNum,SheetCur.Description+" from "+SheetCur.DateTimeSheet.ToShortDateString());
			DialogResult=DialogResult.OK;
		}
	}
}