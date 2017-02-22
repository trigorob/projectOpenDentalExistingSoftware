using System;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDental {
	public partial class FormTaskOptions:ODForm {
		public bool IsShowFinishedTasks;
		public DateTime DateTimeStartShowFinished;
		public bool IsSortApptDateTime;

		public FormTaskOptions(bool isShowFinishedTasks, DateTime dateTimeStartShowFinished, bool isAptDateTimeSort) {
			InitializeComponent();
			Lan.F(this);
			checkShowFinished.Checked=isShowFinishedTasks;
			textStartDate.Text=dateTimeStartShowFinished.ToShortDateString();
			checkTaskSortApptDateTime.Checked=isAptDateTimeSort;
			if(!isShowFinishedTasks) {
				labelStartDate.Enabled=false;
				textStartDate.Enabled=false;
			}
		}

		private void checkShowFinished_Click(object sender,EventArgs e) {
			if(checkShowFinished.Checked) {
				labelStartDate.Enabled=true;
				textStartDate.Enabled=true;
			}
			else {
				labelStartDate.Enabled=false;
				textStartDate.Enabled=false;
			}
		}

		private void butOK_Click(object sender,EventArgs e) {
			if(!(textStartDate.errorProvider1.GetError(textStartDate)=="")) {
				if(checkShowFinished.Checked) {
					MsgBox.Show(this,"Invalid finished task start date");
					return;
				}
				else {
					//We are not going to be using the textStartDate so not reason to warn the user, just reset it back to the default value.
					textStartDate.Text=DateTimeOD.Today.AddDays(-7).ToShortDateString();
				}
			}
			IsShowFinishedTasks=checkShowFinished.Checked;
			DateTimeStartShowFinished=PIn.Date(textStartDate.Text);//Note that this may have not been enabled but we'll pass it back anyway, won't be used.
			IsSortApptDateTime=checkTaskSortApptDateTime.Checked;
			DialogResult=DialogResult.OK;
		}

		private void butCancel_Click(object sender,EventArgs e) {
			DialogResult=DialogResult.Cancel;
		}
	}
}