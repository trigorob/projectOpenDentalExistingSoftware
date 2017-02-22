using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenDental.ReportingComplex;
using OpenDentBusiness;
using System.Collections.Generic;

namespace OpenDental{
	public class FormRpProcNote : ODForm {
		private OpenDental.UI.Button butCancel;
		private OpenDental.UI.Button butOK;
		private MonthCalendar dateStart;
		private MonthCalendar dateEnd;
		private CheckBox checkNoNotes;
		private CheckBox checkUnsignedNote;
		private CheckBox checkAllClin;
		private CheckBox checkAllProv;
		private ListBox listClin;
		private Label labelClin;
		private ListBox listProv;
		private Label label1;
		private List<Clinic> _listClinics;
		private List<Provider> _listProviders;

		/// <summary></summary>
		private System.ComponentModel.Container components = null;

		///<summary></summary>
		public FormRpProcNote()
		{
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRpProcNote));
			this.butCancel = new OpenDental.UI.Button();
			this.butOK = new OpenDental.UI.Button();
			this.dateStart = new System.Windows.Forms.MonthCalendar();
			this.dateEnd = new System.Windows.Forms.MonthCalendar();
			this.checkNoNotes = new System.Windows.Forms.CheckBox();
			this.checkUnsignedNote = new System.Windows.Forms.CheckBox();
			this.checkAllClin = new System.Windows.Forms.CheckBox();
			this.checkAllProv = new System.Windows.Forms.CheckBox();
			this.listClin = new System.Windows.Forms.ListBox();
			this.labelClin = new System.Windows.Forms.Label();
			this.listProv = new System.Windows.Forms.ListBox();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// butCancel
			// 
			this.butCancel.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butCancel.Autosize = true;
			this.butCancel.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butCancel.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butCancel.CornerRadius = 4F;
			this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.butCancel.Location = new System.Drawing.Point(498, 430);
			this.butCancel.Name = "butCancel";
			this.butCancel.Size = new System.Drawing.Size(75, 26);
			this.butCancel.TabIndex = 0;
			this.butCancel.Text = "&Cancel";
			this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
			// 
			// butOK
			// 
			this.butOK.AdjustImageLocation = new System.Drawing.Point(0, 0);
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Autosize = true;
			this.butOK.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butOK.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butOK.CornerRadius = 4F;
			this.butOK.Location = new System.Drawing.Point(498, 389);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75, 26);
			this.butOK.TabIndex = 1;
			this.butOK.Text = "&OK";
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// dateStart
			// 
			this.dateStart.Location = new System.Drawing.Point(36, 8);
			this.dateStart.Name = "dateStart";
			this.dateStart.TabIndex = 2;
			// 
			// dateEnd
			// 
			this.dateEnd.Location = new System.Drawing.Point(319, 8);
			this.dateEnd.Name = "dateEnd";
			this.dateEnd.TabIndex = 3;
			// 
			// checkNoNotes
			// 
			this.checkNoNotes.Location = new System.Drawing.Point(36, 399);
			this.checkNoNotes.Name = "checkNoNotes";
			this.checkNoNotes.Size = new System.Drawing.Size(456, 18);
			this.checkNoNotes.TabIndex = 4;
			this.checkNoNotes.Text = "Include procedures for patients with no notes on any procedure for the same day.";
			this.checkNoNotes.UseVisualStyleBackColor = true;
			// 
			// checkUnsignedNote
			// 
			this.checkUnsignedNote.Location = new System.Drawing.Point(36, 423);
			this.checkUnsignedNote.Name = "checkUnsignedNote";
			this.checkUnsignedNote.Size = new System.Drawing.Size(380, 18);
			this.checkUnsignedNote.TabIndex = 5;
			this.checkUnsignedNote.Text = "Include procedures with a note that is unsigned";
			this.checkUnsignedNote.UseVisualStyleBackColor = true;
			// 
			// checkAllClin
			// 
			this.checkAllClin.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkAllClin.Location = new System.Drawing.Point(213, 188);
			this.checkAllClin.Name = "checkAllClin";
			this.checkAllClin.Size = new System.Drawing.Size(95, 16);
			this.checkAllClin.TabIndex = 54;
			this.checkAllClin.Text = "All";
			this.checkAllClin.Click += new System.EventHandler(this.checkAllClin_Click);
			// 
			// checkAllProv
			// 
			this.checkAllProv.Checked = true;
			this.checkAllProv.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkAllProv.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkAllProv.Location = new System.Drawing.Point(36, 188);
			this.checkAllProv.Name = "checkAllProv";
			this.checkAllProv.Size = new System.Drawing.Size(52, 16);
			this.checkAllProv.TabIndex = 53;
			this.checkAllProv.Text = "All";
			this.checkAllProv.Click += new System.EventHandler(this.checkAllProv_Click);
			// 
			// listClin
			// 
			this.listClin.Location = new System.Drawing.Point(213, 207);
			this.listClin.Name = "listClin";
			this.listClin.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.listClin.Size = new System.Drawing.Size(154, 186);
			this.listClin.TabIndex = 52;
			this.listClin.Click += new System.EventHandler(this.listClin_Click);
			// 
			// labelClin
			// 
			this.labelClin.Location = new System.Drawing.Point(210, 170);
			this.labelClin.Name = "labelClin";
			this.labelClin.Size = new System.Drawing.Size(104, 16);
			this.labelClin.TabIndex = 51;
			this.labelClin.Text = "Clinics";
			this.labelClin.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// listProv
			// 
			this.listProv.Location = new System.Drawing.Point(35, 207);
			this.listProv.Name = "listProv";
			this.listProv.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.listProv.Size = new System.Drawing.Size(154, 186);
			this.listProv.TabIndex = 50;
			this.listProv.Click += new System.EventHandler(this.listProv_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(33, 170);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(104, 16);
			this.label1.TabIndex = 49;
			this.label1.Text = "Providers";
			this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// FormRpProcNote
			// 
			this.CancelButton = this.butCancel;
			this.ClientSize = new System.Drawing.Size(593, 478);
			this.Controls.Add(this.checkAllClin);
			this.Controls.Add(this.checkAllProv);
			this.Controls.Add(this.listClin);
			this.Controls.Add(this.labelClin);
			this.Controls.Add(this.listProv);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.checkUnsignedNote);
			this.Controls.Add(this.checkNoNotes);
			this.Controls.Add(this.dateEnd);
			this.Controls.Add(this.dateStart);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.butCancel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormRpProcNote";
			this.ShowInTaskbar = false;
			this.Text = "Incomplete Procedure Notes Report";
			this.Load += new System.EventHandler(this.FormRpProcNote_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void FormRpProcNote_Load(object sender, System.EventArgs e) {
			dateStart.SelectionStart=DateTime.Today;
			dateEnd.SelectionStart=DateTime.Today;			
			checkNoNotes.Checked=PrefC.GetBool(PrefName.ReportsIncompleteProcsNoNotes);
			checkUnsignedNote.Checked=PrefC.GetBool(PrefName.ReportsIncompleteProcsUnsigned);
			_listProviders=ProviderC.GetListReports();
			for(int i=0;i<_listProviders.Count;i++){
				listProv.Items.Add(_listProviders[i].GetLongDesc());
			}
			if(PrefC.HasClinicsEnabled){
				_listClinics=Clinics.GetForUserod(Security.CurUser);
				if(!Security.CurUser.ClinicIsRestricted) {
					listClin.Items.Add(Lan.g(this,"Unassigned"));
					listClin.SetSelected(0,true);
				}
				for(int i=0;i<_listClinics.Count;i++) {
					int curIndex=listClin.Items.Add(_listClinics[i].Abbr);
					if(Clinics.ClinicNum==0) {
						listClin.SetSelected(curIndex,true);
						checkAllClin.Checked=true;
					}
					if(_listClinics[i].ClinicNum==Clinics.ClinicNum) {
						listClin.SelectedIndices.Clear();
						listClin.SetSelected(curIndex,true);
					}
				}
			}
			else {
				listClin.Visible=false;
				labelClin.Visible=false;
				checkAllClin.Visible=false;
			}
		}

		private void listProv_Click(object sender,EventArgs e) {
			if(listProv.SelectedIndices.Count>0) {
				checkAllProv.Checked=false;
			}
		}

		private void checkAllProv_Click(object sender,EventArgs e) {
			if(checkAllProv.Checked) {
				listProv.SelectedIndices.Clear();
			}
		}

		private void listClin_Click(object sender,EventArgs e) {
			if(listClin.SelectedIndices.Count>0) {
				checkAllClin.Checked=false;
			}
		}

		private void checkAllClin_Click(object sender,EventArgs e) {
			if(checkAllClin.Checked) {
				for(int i=0;i<listClin.Items.Count;i++) {
					listClin.SetSelected(i,true);
				}
			}
			else {
				listClin.SelectedIndices.Clear();
			}
		}

		private void butOK_Click(object sender, System.EventArgs e) {
			if(dateEnd.SelectionStart<dateStart.SelectionStart) {
				MsgBox.Show(this,"End date cannot be before start date.");
				return;
			}
			ExecuteReport(dateStart.SelectionStart,dateEnd.SelectionStart);
			DialogResult=DialogResult.OK;
		}

		private void ExecuteReport(DateTime dateStart,DateTime dateEnd){
			if(checkAllProv.Checked) {
				for(int i=0;i<listProv.Items.Count;i++) {
					listProv.SetSelected(i,true);
				}
			}
			if(checkAllClin.Checked) {
				for(int i=0;i<listClin.Items.Count;i++) {
					listClin.SetSelected(i,true);
				}
			}
			List<Provider> listProvs=new List<Provider>();
			for(int i=0;i<listProv.SelectedIndices.Count;i++) {
				listProvs.Add(_listProviders[listProv.SelectedIndices[i]]);
			}
			List<Clinic> listClinics=new List<Clinic>();
			if(PrefC.HasClinicsEnabled) {
				for(int i=0;i<listClin.SelectedIndices.Count;i++) {
					if(Security.CurUser.ClinicIsRestricted) {
						listClinics.Add(_listClinics[listClin.SelectedIndices[i]]);//we know that the list is a 1:1 to _listClinics
					}
					else {
						if(listClin.SelectedIndices[i]==0) {
							Clinic unassigned=new Clinic();
							unassigned.ClinicNum=0;
							unassigned.Abbr=Lan.g(this,"Unassigned");
							listClinics.Add(unassigned);
						}
						else {
							listClinics.Add(_listClinics[listClin.SelectedIndices[i]-1]);//Minus 1 from the selected index
						}
					}
				}
			}
			List<long> listProvNums=new List<long>();
			for(int i=0;i<listProvs.Count;i++) {
				listProvNums.Add(listProvs[i].ProvNum);
			}
			if(!checkAllProv.Checked) {
				listProvNums.Add(0);//ProvNum=0 is unearned
			}
			List<long> listClinicNums=new List<long>();
			for(int i=0;i<listClinics.Count;i++) {
				listClinicNums.Add(listClinics[i].ClinicNum);
			}
			bool includeNoNotes=false;
			bool includeUnsignedNotes=false;
			string whereNoNote="";
			string whereUnsignedNote="";
			string whereNotesClause="";
			if(checkNoNotes.Checked) {
				whereNoNote=@"
					LEFT JOIN (
						SELECT procedurelog.PatNum,procedurelog.ProcDate
						FROM procedurelog
						INNER JOIN procnote ON procnote.ProcNum=procedurelog.ProcNum
						WHERE procedurelog.ProcDate BETWEEN "+POut.Date(dateStart)+" AND "+POut.Date(dateEnd)+@"
						AND procedurelog.ProcStatus="+POut.Int((int)ProcStat.C)+@" 
						GROUP BY procedurelog.PatNum,procedurelog.ProcDate
					) hasNotes ON hasNotes.PatNum=procedurelog.PatNum AND hasNotes.ProcDate=procedurelog.ProcDate ";
				includeNoNotes=true;
				whereNotesClause="AND (n1.ProcNum IS NOT NULL OR hasNotes.PatNum IS NULL) ";
			}
			if(checkUnsignedNote.Checked) {
				includeUnsignedNotes=true;
				if(includeNoNotes) {
					whereNotesClause="AND (n1.ProcNum IS NOT NULL OR hasNotes.PatNum IS NULL OR unsignedNotes.ProcNum IS NOT NULL)";
				}
				else {
					whereNotesClause="AND (n1.ProcNum IS NOT NULL OR unsignedNotes.ProcNum IS NOT NULL)";
				}
				whereUnsignedNote=@"
					LEFT JOIN procnote unsignedNotes ON unsignedNotes.ProcNum=procedurelog.ProcNum
						AND unsignedNotes.Signature=''
						AND unsignedNotes.EntryDateTime= (SELECT MAX(n2.EntryDateTime) 
								FROM procnote n2 
								WHERE unsignedNotes.ProcNum = n2.ProcNum) 
";
			}
			Cursor=Cursors.WaitCursor;
			Font font=new Font("Tahoma",9);
			Font fontTitle=new Font("Tahoma",17,FontStyle.Bold);
			Font fontSubTitle=new Font("Tahoma",10,FontStyle.Bold);
			ReportComplex report=new ReportComplex(true,false);
			report.AddTitle("Title",Lan.g(this,"Incomplete Procedure Notes"),fontTitle);
			report.AddSubTitle("PracTitle",PrefC.GetString(PrefName.PracticeTitle),fontSubTitle);
			if(checkAllProv.Checked) {
				report.AddSubTitle("Providers",Lan.g(this,"All Providers"));
			}
			else {
				string str="";
				for(int i=0;i<listProv.SelectedIndices.Count;i++) {
					if(i>0) {
						str+=", ";
					}
					str+=_listProviders[listProv.SelectedIndices[i]].Abbr;
				}
				report.AddSubTitle("Providers",str);
			}
			if(PrefC.HasClinicsEnabled) {
				if(checkAllClin.Checked) {
					report.AddSubTitle("Clinics",Lan.g(this,"All Clinics"));
				}
				else {
					string clinNames="";
					for(int i=0;i<listClin.SelectedIndices.Count;i++) {
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
			report.AddSubTitle("Dates of Report",dateStart.ToString("d")+" - "+dateEnd.ToString("d"),fontSubTitle);
			//==Tg: We can't add date restriction to procnote searching because we are not guaranteed a new note was inserted on or after procdate.
			//The newest note could be before the date range causing procedures to show on this report that should not.
			//We also cannot add a end date for procnotes because any changes users made would not ever be reflected in the report.
			string command=@"SELECT procedurelog.ProcDate,CONCAT(CONCAT(patient.LName, ', '),patient.FName),
				procedurecode.ProcCode,procedurecode.Descript,procedurelog.ToothNum,procedurelog.Surf "
				+(includeNoNotes || includeUnsignedNotes?",(CASE WHEN n1.ProcNum IS NOT NULL THEN 'X' ELSE '' END) AS Incomplete ":"")
				+(includeNoNotes?",(CASE WHEN hasNotes.PatNum IS NULL THEN 'X' ELSE '' END) AS HasNoNote ":"")
				+(includeUnsignedNotes?",(CASE WHEN unsignedNotes.ProcNum IS NOT NULL THEN 'X' ELSE '' END) AS HasUnsignedNote ":"")+@" 
				FROM procedurelog
				INNER JOIN patient ON procedurelog.PatNum = patient.PatNum 
				INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum 
				"+(includeNoNotes || includeUnsignedNotes?"LEFT":"INNER")+@" JOIN procnote n1 ON procedurelog.ProcNum = n1.ProcNum 
					AND n1.Note LIKE '%""""%' "//looks for ""
				+@" AND n1.EntryDateTime= (SELECT MAX(n2.EntryDateTime) 
				FROM procnote n2 
				WHERE n1.ProcNum = n2.ProcNum) "
				+whereNoNote+" "
				+whereUnsignedNote+@"
				WHERE procedurelog.ProcDate BETWEEN "+POut.Date(dateStart)+" AND "+POut.Date(dateEnd)+@"
				AND (procedurelog.ProcStatus="+POut.Int((int)ProcStat.C)
				+" OR (procedurelog.ProcStatus="+POut.Int((int)ProcStat.EC)+" "
				+@" AND procedurecode.ProcCode='~GRP~')) "
				+whereNotesClause+@"
				AND procedurelog.ProvNum IN ("+string.Join(",",listProvNums)+@")
				"+(PrefC.HasClinicsEnabled?"AND procedurelog.ClinicNum IN ("+string.Join(",",listClinicNums)+") ":"")+@" 
				ORDER BY ProcDate";
			QueryObject query=report.AddQuery(command,"","",SplitByKind.None,1,true);
			query.AddColumn("Date",80,FieldValueType.Date,font);
			query.AddColumn("Patient",120,FieldValueType.String,font);
			if(Clinics.IsMedicalPracticeOrClinic(Clinics.ClinicNum)) {
				query.AddColumn("Code",150,FieldValueType.String,font);
				query.AddColumn("Description",220,FieldValueType.String,font);
			}
			else {
				query.AddColumn("Code",80,FieldValueType.String,font);
				query.AddColumn("Description",220,FieldValueType.String,font);
				query.AddColumn("Tth",30,FieldValueType.String,font);
				query.AddColumn("Surf",40,FieldValueType.String,font);
			}
			if(includeUnsignedNotes || includeNoNotes) {
				query.AddColumn("Incomplete",70,FieldValueType.String,font);
			}
			if(includeNoNotes) {
				query.AddColumn("No Note",60,FieldValueType.String,font);
			}
			if(includeUnsignedNotes) {
				query.AddColumn("Unsigned",70,FieldValueType.String,font);
			}
			if(!report.SubmitQueries()) {
				DialogResult=DialogResult.Cancel;
				return;
			}
			Cursor=Cursors.Default;
			FormReportComplex FormR=new FormReportComplex(report);
			FormR.ShowDialog();
		}

		private void butCancel_Click(object sender, System.EventArgs e) {
			DialogResult=DialogResult.Cancel;
		}
	}
}




















