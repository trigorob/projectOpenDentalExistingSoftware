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
using CodeBase;

namespace OpenDental {
	///<summary>In-memory form. Changes are not saved to the DB from this form.</summary>
	public partial class FormApptReminderRuleEdit:ODForm {
		public ApptReminderRule ApptReminderRuleCur;
		private List<CommType> _sendOrder;

		public FormApptReminderRuleEdit(ApptReminderRule apptReminderCur) {
			InitializeComponent();
			Lan.F(this);
			ApptReminderRuleCur=apptReminderCur;
		}

		private void FormApptReminderRuleEdit_Load(object sender,EventArgs e) {
			this.Text=ApptReminderRuleCur.TypeCur==ApptReminderType.ConfirmationFutureDay ? Lan.g(this,"Edit eConfirmation Rule") : Lan.g(this,"Edit eReminder Rule");
			labelRuleType.Text=ApptReminderRuleCur.TypeCur.GetDescription();
			labelTags.Text=Lan.g(this,"Use the following replacement tags to customize messages : ")+string.Join(", ",ApptReminderRules.GetAvailableTags(ApptReminderRuleCur.TypeCur));
			//replacementTags.ForEach(x => listBoxTags.Items.Add(x));
			textTemplateSms.Text=ApptReminderRuleCur.TemplateSMS;
			textTemplateEmail.Text=ApptReminderRuleCur.TemplateEmail;
			_sendOrder=ApptReminderRuleCur.SendOrder.Split(',').Select(x => (CommType)PIn.Int(x)).ToList();
			FillGridPriority();
			if(ApptReminderRuleCur.TypeCur==ApptReminderType.ReminderSameDay) {
				labelLeadTime.Text=Lan.g(this,"Hours");
			}
			else {
				labelLeadTime.Text=Lan.g(this,"Days");
			}
			FillTimeSpan();
			textTemplateSubject.Text=ApptReminderRuleCur.TemplateEmailSubject;
			checkSendAll.Checked=ApptReminderRuleCur.IsSendAll;
			textTime.errorProvider1.SetIconAlignment(textTime,ErrorIconAlignment.MiddleLeft);
		}

		private void FillTimeSpan() {
			if(ApptReminderRuleCur.TypeCur==ApptReminderType.ReminderSameDay) {
				textTime.Text=ApptReminderRuleCur.TSPrior.Hours.ToString();//Hours, not total hours.
			}
			else {
				textTime.Text=ApptReminderRuleCur.TSPrior.Days.ToString();//Days, not total Days.
			}
		}

		private void FillGridPriority() {
			gridPriorities.BeginUpdate();
			gridPriorities.Columns.Clear();
			gridPriorities.Columns.Add(new ODGridColumn("",0));
			gridPriorities.Rows.Clear();
			for(int i = 0;i<_sendOrder.Count;i++) {
				CommType typeCur = _sendOrder[i];
				if(typeCur==CommType.Preferred && checkSendAll.Checked) {
					//"Preferred" is irrelevant when SendAll is checked.
					continue;
				}
				if(typeCur==CommType.Text && !SmsPhones.IsIntegratedTextingEnabled()) {
					gridPriorities.AddRow(Lan.g(this,typeCur.ToString())+" ("+Lan.g(this,"Not Configured")+")");
					gridPriorities.Rows[gridPriorities.Rows.Count-1].ColorBackG=Color.LightGray;
				}
				else {
					gridPriorities.AddRow(Lan.g(this,typeCur.ToString()));
				}
			}
			gridPriorities.EndUpdate();
		}

		private void butUp_Click(object sender,EventArgs e) {
			int idx = gridPriorities.GetSelectedIndex();
			if(idx<1) {
				//-1 if nothing selected. 0 if top item selected.
				return;
			}
			_sendOrder.Reverse(idx-1,2);
			FillGridPriority();
			gridPriorities.SetSelected(idx-1,true);
		}

		private void butDown_Click(object sender,EventArgs e) {
			int idx = gridPriorities.GetSelectedIndex();
			if(idx==-1 || idx==_sendOrder.Count-1) {
				//-1 nothing selected. Count-1 if last item selected.
				return;
			}
			_sendOrder.Reverse(idx,2);
			FillGridPriority();
			gridPriorities.SetSelected(idx+1,true);
		}

		private void butOk_Click(object sender,EventArgs e) {
			if(!string.IsNullOrWhiteSpace(textTime.errorProvider1.GetError(textTime))) {
				MsgBox.Show(this,"Fix data entry errors first.");
				return;
			}
			if(!ValidateRule()) {
				return;
			}
			ApptReminderRuleCur.TemplateSMS=textTemplateSms.Text;
			ApptReminderRuleCur.TemplateEmailSubject=textTemplateSubject.Text;
			ApptReminderRuleCur.TemplateEmail=textTemplateEmail.Text;
			ApptReminderRuleCur.SendOrder=string.Join(",",_sendOrder.Select(x => ((int)x).ToString()).ToArray());
			ApptReminderRuleCur.IsSendAll=checkSendAll.Checked;
			if(ApptReminderRuleCur.TypeCur==ApptReminderType.ReminderSameDay) {
				ApptReminderRuleCur.TSPrior=TimeSpan.FromHours(PIn.Int(textTime.Text,false));
			}
			else {
				ApptReminderRuleCur.TSPrior=TimeSpan.FromDays(PIn.Int(textTime.Text,false));
			}
			DialogResult=DialogResult.OK;
		}

		private bool ValidateRule() {
			List<string> errors = new List<string>();
			if(string.IsNullOrWhiteSpace(textTemplateSms.Text)) {
				errors.Add(Lan.g(this,"Text message cannot be blank."));
			}
			if(string.IsNullOrWhiteSpace(textTemplateSubject.Text)) {
				errors.Add(Lan.g(this,"Email subject cannot be blank."));
			}
			if(string.IsNullOrWhiteSpace(textTemplateEmail.Text)) {
				errors.Add(Lan.g(this,"Email message cannot be blank."));
			}
			if(ApptReminderRuleCur.TypeCur==ApptReminderType.ReminderSameDay) {
				if(PIn.Int(textTime.Text,false)>=24) {
					errors.Add(Lan.g(this,"Lead time must be less than 24 hours for same day reminders."));
				}
			}
			if(ApptReminderRuleCur.TypeCur==ApptReminderType.ReminderFutureDay) {
				if(PIn.Int(textTime.Text,false)>366) {
					errors.Add(Lan.g(this,"Lead time must 365 days or less for future reminders."));
				}
			}
			if(ApptReminderRuleCur.TypeCur==ApptReminderType.ConfirmationFutureDay) {
				if(PIn.Int(textTime.Text,false)>366) {
					errors.Add(Lan.g(this,"Lead time must 365 days or less for confirmations."));
				}
				if(!textTemplateSms.Text.ToLower().Contains("[confirmcode]")) {
					errors.Add(Lan.g(this,"Confirmation texts must contain the \"[ConfirmCode]\" tag."));
				}
				if(textTemplateEmail.Text.ToLower().Contains("[confirmcode]")) {
					errors.Add(Lan.g(this,"Confirmation emails should not contain the \"[ConfirmCode]\" tag."));
				}
				if(!textTemplateEmail.Text.ToLower().Contains("[confirmurl]")) {
					errors.Add(Lan.g(this,"Confirmation emails must contain the \"[ConfirmURL]\" tag."));
				}
			}
			if(errors.Count>0) {
				MessageBox.Show(Lan.g(this,"You must fix the following errors before continuing.")+"\r\n\r\n-"+string.Join("\r\n-",errors));
			}
			return errors.Count==0;
		}

		private void checkSendAll_CheckedChanged(object sender,EventArgs e) {
			butUp.Enabled=!checkSendAll.Checked;
			butDown.Enabled=!checkSendAll.Checked;
			gridPriorities.Enabled=!checkSendAll.Checked;
			gridPriorities.SetSelected(false);
			FillGridPriority();
		}

		private void butCancel_Click(object sender,EventArgs e) {
			DialogResult=DialogResult.Cancel;
		}

		private void butDelete_Click(object sender,EventArgs e) {
			ApptReminderRuleCur=null;
			DialogResult=DialogResult.OK;
		}

	}
}
