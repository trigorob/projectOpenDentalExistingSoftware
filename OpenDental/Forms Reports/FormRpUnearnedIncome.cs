using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using OpenDentBusiness;
using System.Collections.Generic;

namespace OpenDental{
///<summary></summary>
	public class FormRpUnearnedIncome:ODForm {
		private System.ComponentModel.Container components = null;
		private MonthCalendar date2;
		private MonthCalendar date1;
		private OpenDental.UI.Button butCancel;
		private OpenDental.UI.Button butOK;
		private RadioButton radioDateRange;
		private RadioButton radioTotal;
		private CheckBox checkAllClin;
		private ListBox listClin;
		private Label labelClin;
		private FormQuery FormQuery2;
		private List<Clinic> _listClinics;

		///<summary></summary>
		public FormRpUnearnedIncome() {
			InitializeComponent();
			Lan.F(this);
		}

		///<summary></summary>
		protected override void Dispose( bool disposing ){
			if( disposing ){
				if(components != null){
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormRpUnearnedIncome));
			this.date2 = new System.Windows.Forms.MonthCalendar();
			this.date1 = new System.Windows.Forms.MonthCalendar();
			this.radioDateRange = new System.Windows.Forms.RadioButton();
			this.radioTotal = new System.Windows.Forms.RadioButton();
			this.butCancel = new OpenDental.UI.Button();
			this.butOK = new OpenDental.UI.Button();
			this.checkAllClin = new System.Windows.Forms.CheckBox();
			this.listClin = new System.Windows.Forms.ListBox();
			this.labelClin = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// date2
			// 
			this.date2.Location = new System.Drawing.Point(281,27);
			this.date2.MaxSelectionCount = 1;
			this.date2.Name = "date2";
			this.date2.TabIndex = 4;
			// 
			// date1
			// 
			this.date1.Location = new System.Drawing.Point(25,27);
			this.date1.MaxSelectionCount = 1;
			this.date1.Name = "date1";
			this.date1.TabIndex = 3;
			// 
			// radioDateRange
			// 
			this.radioDateRange.Checked = true;
			this.radioDateRange.Location = new System.Drawing.Point(25,240);
			this.radioDateRange.Name = "radioDateRange";
			this.radioDateRange.Size = new System.Drawing.Size(169,18);
			this.radioDateRange.TabIndex = 7;
			this.radioDateRange.TabStop = true;
			this.radioDateRange.Text = "Activity for Date Range";
			this.radioDateRange.UseVisualStyleBackColor = true;
			// 
			// radioTotal
			// 
			this.radioTotal.Location = new System.Drawing.Point(25,265);
			this.radioTotal.Name = "radioTotal";
			this.radioTotal.Size = new System.Drawing.Size(205,18);
			this.radioTotal.TabIndex = 8;
			this.radioTotal.Text = "Total Liability";
			this.radioTotal.UseVisualStyleBackColor = true;
			// 
			// butCancel
			// 
			this.butCancel.AdjustImageLocation = new System.Drawing.Point(0,0);
			this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butCancel.Autosize = true;
			this.butCancel.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butCancel.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butCancel.CornerRadius = 4F;
			this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.butCancel.Location = new System.Drawing.Point(475,391);
			this.butCancel.Name = "butCancel";
			this.butCancel.Size = new System.Drawing.Size(75,26);
			this.butCancel.TabIndex = 6;
			this.butCancel.Text = "&Cancel";
			this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
			// 
			// butOK
			// 
			this.butOK.AdjustImageLocation = new System.Drawing.Point(0,0);
			this.butOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.butOK.Autosize = true;
			this.butOK.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
			this.butOK.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
			this.butOK.CornerRadius = 4F;
			this.butOK.Location = new System.Drawing.Point(475,359);
			this.butOK.Name = "butOK";
			this.butOK.Size = new System.Drawing.Size(75,26);
			this.butOK.TabIndex = 5;
			this.butOK.Text = "&OK";
			this.butOK.Click += new System.EventHandler(this.butOK_Click);
			// 
			// checkAllClin
			// 
			this.checkAllClin.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkAllClin.Location = new System.Drawing.Point(281, 222);
			this.checkAllClin.Name = "checkAllClin";
			this.checkAllClin.Size = new System.Drawing.Size(95, 16);
			this.checkAllClin.TabIndex = 54;
			this.checkAllClin.Text = "All";
			this.checkAllClin.CheckedChanged += new System.EventHandler(this.checkAllClin_Click);
			// 
			// listClin
			// 
			this.listClin.Location = new System.Drawing.Point(281, 241);
			this.listClin.Name = "listClin";
			this.listClin.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.listClin.Size = new System.Drawing.Size(154, 147);
			this.listClin.TabIndex = 53;
			this.listClin.Click += new System.EventHandler(this.listClin_Click);
			// 
			// labelClin
			// 
			this.labelClin.Location = new System.Drawing.Point(278, 204);
			this.labelClin.Name = "labelClin";
			this.labelClin.Size = new System.Drawing.Size(104, 16);
			this.labelClin.TabIndex = 52;
			this.labelClin.Text = "Clinics";
			this.labelClin.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// FormRpUnearnedIncome
			// 
			this.ClientSize = new System.Drawing.Size(583, 442);
			this.Controls.Add(this.checkAllClin);
			this.Controls.Add(this.listClin);
			this.Controls.Add(this.labelClin);
			this.Controls.Add(this.radioTotal);
			this.Controls.Add(this.radioDateRange);
			this.Controls.Add(this.butCancel);
			this.Controls.Add(this.butOK);
			this.Controls.Add(this.date2);
			this.Controls.Add(this.date1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FormRpUnearnedIncome";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Unearned Income Activity";
			this.Load += new System.EventHandler(this.FormUnearnedIncomeActivity_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void FormUnearnedIncomeActivity_Load(object sender, System.EventArgs e) {
			DateTime dateThisFirst=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1);
			date1.SelectionStart=dateThisFirst.AddMonths(-1);
			date2.SelectionStart=dateThisFirst.AddDays(-1);
			if(PrefC.HasClinicsEnabled) {//fill clinic list
				_listClinics=Clinics.GetForUserod(Security.CurUser);
				if(!Security.CurUser.ClinicIsRestricted) {
					listClin.Items.Add(Lan.g(this,"Unassigned"));
					listClin.SetSelected(0,true);
				}
				for(int i = 0;i<_listClinics.Count;i++) {
					int curIndex=listClin.Items.Add(_listClinics[i].Abbr);
					if(Clinics.ClinicNum==0) {
						checkAllClin.Checked=true;
					}
					if(_listClinics[i].ClinicNum==Clinics.ClinicNum) {
						listClin.SetSelected(curIndex,true);
					}
				}
			}
			else {//hide label,list,and check box if clinics are not enabled
				listClin.Visible=false;
				labelClin.Visible=false;
				checkAllClin.Visible=false;
			}
		}

		private void checkAllClin_Click(object sender,EventArgs e) {
			if(checkAllClin.Checked) {
				listClin.SelectedIndices.Clear();
			}
		}

		private void listClin_Click(object sender,EventArgs e) {
			if(listClin.SelectedIndices.Count>0) {
				checkAllClin.Checked=false;
			}
		}

		private void butOK_Click(object sender,EventArgs e) {
			if(date2.SelectionStart<date1.SelectionStart) {
				MsgBox.Show(this,"End date cannot be before start date.");
				return;
			}
			if(PrefC.HasClinicsEnabled) {
				if(!checkAllClin.Checked && listClin.SelectedIndices.Count==0) {
					MsgBox.Show(this,"At least one clinic must be selected.");
					return;
				}
			}
			ReportSimpleGrid report=new ReportSimpleGrid();
			string whereClin="";
			if(PrefC.HasClinicsEnabled && !checkAllClin.Checked) { //generate the WHERE clause if clinics are enabled and clinic checkbox is not check.At least one clinic is selected.
				whereClin+="AND paysplit.ClinicNum IN(";
				for(int i = 0;i<listClin.SelectedIndices.Count;i++) {
					if(i>0) {
						whereClin+=",";
					}
					if(Security.CurUser.ClinicIsRestricted) {
						whereClin+=POut.Long(_listClinics[listClin.SelectedIndices[i]].ClinicNum);//we know that the list is a 1:1 to _listClinics
					}
					else {
						if(listClin.SelectedIndices[i]==0) {
							whereClin+="0";
						}
						else {
							whereClin+=POut.Long(_listClinics[listClin.SelectedIndices[i]-1].ClinicNum);//Minus 1 from the selected index
						}
					}
				}
				whereClin+=") ";
			}
			if(radioDateRange.Checked) {
				report.Query="SELECT DatePay,"+DbHelper.Concat("patient.LName","', '","patient.FName","' '","patient.MiddleI")+",ItemName";
				if(PrefC.HasClinicsEnabled) {
					report.Query+=",clinic.Description";
				}
				report.Query+=",SplitAmt FROM paysplit "
					+"INNER JOIN patient ON patient.PatNum=paysplit.PatNum "
					+"INNER JOIN definition ON definition.DefNum=paysplit.UnearnedType ";
				if(PrefC.HasClinicsEnabled) {
					report.Query+="LEFT JOIN clinic ON clinic.ClinicNum=paysplit.ClinicNum ";
				}
				report.Query+="WHERE paysplit.DatePay >= "+POut.Date(date1.SelectionStart)+" "
					+"AND paysplit.DatePay <= "+POut.Date(date2.SelectionStart)+" "
					+whereClin
					+"AND UnearnedType > 0 GROUP BY paysplit.SplitNum "
					+"ORDER BY DatePay";
				FormQuery2=new FormQuery(report);
				FormQuery2.IsReport=true;
				FormQuery2.SubmitReportQuery();
				report.Title="Unearned Income Activity";
				report.SubTitle.Add(PrefC.GetString(PrefName.PracticeTitle));
				string dateRange=date1.SelectionStart.ToShortDateString()+" - "+date2.SelectionStart.ToShortDateString();
				report.SubTitle.Add(dateRange);
				if(PrefC.HasClinicsEnabled) {//show sub titles if clinics are enabled. 
					if(checkAllClin.Checked) {
						report.SubTitle.Add(Lan.g(this,"All Clinics"));
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
						report.SubTitle.Add(clinNames);
					}
				}
				report.SetColumn(this,0,"Date",100);
				report.SetColumn(this,1,"Patient",140);
				report.SetColumn(this,2,"Type",110);
				if(PrefC.HasClinicsEnabled) {
					report.SetColumn(this,3,"Clinic",80);
					report.SetColumn(this,4,"Amount",80,HorizontalAlignment.Right);
				}
				else {
					report.SetColumn(this,3,"Amount",80,HorizontalAlignment.Right);
				}
			}
			else {
				report.Query="SELECT "+DbHelper.Concat("patient.LName","', '","patient.FName","' '","patient.MiddleI")+",";
				report.Query+=DbHelper.GroupConcat("ItemName",true);
				if(PrefC.HasClinicsEnabled) {
					report.Query+=",clinic.Description";
				}
				report.Query+=",SUM(SplitAmt) Amount ";
				report.Query+="FROM paysplit "
					+"INNER JOIN patient ON patient.PatNum=paysplit.PatNum "
					+"INNER JOIN definition ON definition.DefNum=paysplit.UnearnedType ";
				if(PrefC.HasClinicsEnabled) {
					report.Query+="LEFT JOIN clinic ON clinic.ClinicNum=paysplit.ClinicNum ";
				}
				report.Query+="WHERE UnearnedType > 0 "
					+whereClin
					+"GROUP BY paysplit.PatNum HAVING Amount != 0";//still won't work for oracle
				FormQuery2=new FormQuery(report);
				FormQuery2.IsReport=true;
				FormQuery2.SubmitReportQuery();
				report.Title="Unearned Income Liabilities";
				report.SubTitle.Add(PrefC.GetString(PrefName.PracticeTitle));
				if(PrefC.HasClinicsEnabled) {//show sub titles if clinics are enabled. 
					if(checkAllClin.Checked) {
						report.SubTitle.Add(Lan.g(this,"All Clinics"));
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
						report.SubTitle.Add(clinNames);
					}
				}
				report.SetColumn(this,0,"Patient",140);
				report.SetColumn(this,1,"Type(s)",110);
				if(PrefC.HasClinicsEnabled) {
					report.SetColumn(this,2,"Clinic",80);
					report.SetColumn(this,3,"Amount",80,HorizontalAlignment.Right);
				}
				else {
					report.SetColumn(this,2,"Amount",80,HorizontalAlignment.Right);
				}
			}
			FormQuery2.ShowDialog();
			DialogResult=DialogResult.OK;
		}

		private void butCancel_Click(object sender,EventArgs e) {
			DialogResult=DialogResult.Cancel;
		}

	

	
		

		



	}
}
