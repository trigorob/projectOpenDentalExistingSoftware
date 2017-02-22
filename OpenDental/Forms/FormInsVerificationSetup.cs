using System;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDental {
	public partial class FormInsVerificationSetup:ODForm {
		private bool _hasChanged;

		public FormInsVerificationSetup() {
			InitializeComponent();
			Lan.F(this);
		}

		private void FormInsVerificationSetup_Load(object sender,EventArgs e) {
			textInsBenefitEligibilityDays.Text=POut.Int(PrefC.GetInt(PrefName.InsVerifyBenefitEligibilityDays));
			textPatientEnrollmentDays.Text=POut.Int(PrefC.GetInt(PrefName.InsVerifyPatientEnrollmentDays));
			textScheduledAppointmentDays.Text=POut.Int(PrefC.GetInt(PrefName.InsVerifyAppointmentScheduledDays));
			checkInsVerifyUseCurrentUser.Checked=PrefC.GetBool(PrefName.InsVerifyDefaultToCurrentUser);
			checkInsVerifyExcludePatVerify.Checked=PrefC.GetBool(PrefName.InsVerifyExcludePatVerify);
			if(!PrefC.GetBool(PrefName.ShowFeaturePatientClone)) {
				checkExcludePatientClones.Visible=false;
			}
			else {
				checkExcludePatientClones.Checked=PrefC.GetBool(PrefName.InsVerifyExcludePatientClones);
			}
		}

		private void butOK_Click(object sender,EventArgs e) {
			if(textInsBenefitEligibilityDays.errorProvider1.GetError(textInsBenefitEligibilityDays)!="") {
				MsgBox.Show(this,"The number entered for insurance benefit eligibility was not a valid number.  Please enter a valid number to continue.");
				return;
			}
			if(textPatientEnrollmentDays.errorProvider1.GetError(textPatientEnrollmentDays)!="") {
				MsgBox.Show(this,"The number entered for patient enrollment was not a valid number.  Please enter a valid number to continue.");
				return;
			}
			if(textScheduledAppointmentDays.errorProvider1.GetError(textScheduledAppointmentDays)!="") {
				MsgBox.Show(this,"The number entered for scheduled appointments was not a valid number.  Please enter a valid number to continue.");
				return;
			}
			int insBenefitEligibilityDays=PIn.Int(textInsBenefitEligibilityDays.Text);
			int patientEnrollmentDays=PIn.Int(textPatientEnrollmentDays.Text);
			int scheduledAppointmentDays=PIn.Int(textScheduledAppointmentDays.Text);
			if(Prefs.UpdateInt(PrefName.InsVerifyBenefitEligibilityDays,insBenefitEligibilityDays)
				| Prefs.UpdateInt(PrefName.InsVerifyPatientEnrollmentDays,patientEnrollmentDays)
				| Prefs.UpdateInt(PrefName.InsVerifyAppointmentScheduledDays,scheduledAppointmentDays)
				| Prefs.UpdateBool(PrefName.InsVerifyExcludePatVerify,checkInsVerifyExcludePatVerify.Checked)
				| Prefs.UpdateBool(PrefName.InsVerifyExcludePatientClones,checkExcludePatientClones.Checked)
				| Prefs.UpdateBool(PrefName.InsVerifyDefaultToCurrentUser,checkInsVerifyUseCurrentUser.Checked)) 
			{
				_hasChanged=true;
			}
			DialogResult=DialogResult.OK;
		}

		private void butCancel_Click(object sender,EventArgs e) {
			DialogResult=DialogResult.Cancel;
		}

		private void FormInsVerificationSetup_FormClosing(object sender,FormClosingEventArgs e) {
			if(_hasChanged) {
				DataValid.SetInvalid(InvalidType.Prefs);
			}
		}
	}
}