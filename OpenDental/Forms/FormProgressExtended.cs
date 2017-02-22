using CodeBase;
using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDental {
	///<summary>Do not instatiate this class.  It is not meant for public use.  Use ODProgress.ShowProgressExtended() instead.
	///Launch this window in a separate thread so that the progress bar can smoothly spin without waiting on the main thread.
	///Send the phrase "DEFCON 1" in order to have the window gracefully close (as to not rely on thread abort).
	///FormProgressExtended is extremely tailored to monitoring the progress of FormBilling.  Much will need to change about this to make it generic.</summary>
	public partial class FormProgressExtended:Form {
		private string _odEventName;
		private Type _eventType;

		///<summary>Do not instatiate this class.  It is not meant for public use.  Use ODProgress.ShowProgressExtended() instead.</summary>
		public FormProgressExtended(string odEventName) : this(odEventName,typeof(ODEvent),false) {

		}

		///<summary>Do not instatiate this class.  It is not meant for public use.  Use ODProgress.ShowProgressExtended() instead.
		///Launches a progress window that will constantly spin and display status updates for global ODEvents with corresponding name.
		///eventType must be a Type that contains an event called Fired.</summary>
		public FormProgressExtended(string odEventName,Type eventType,bool hasHistory=false) {
			InitializeComponent();
			Lan.F(this);
			_odEventName=odEventName;
			_eventType=eventType;
			//Registers this form for any progress status updates that happen throughout the entire program.
			EventInfo fireEvent;
			try {
				fireEvent=eventType.GetEvent("Fired");
			}
			catch(Exception) {
				fireEvent=typeof(ODEvent).GetEvent("Fired");
			}
			Type fireEventType = fireEvent.EventHandlerType;
			Delegate fireDelegate = Delegate.CreateDelegate(fireEventType, this, "ODEvent_Fired");
			MethodInfo addHandler = fireEvent.GetAddMethod();
			Object[] addHandlerArgs = { fireDelegate };
			addHandler.Invoke(this, addHandlerArgs);
		}

		private void FormProgressExtended_Load(object sender,EventArgs e) {
		}

		public void ODEvent_Fired(ODEventArgs e) {
			//We don't know what thread will cause a progress status change, so invoke this method as a delegate if necessary.
			if(this.InvokeRequired) {
				this.Invoke((Action)delegate() { ODEvent_Fired(e); });
				return;
			}
			//Make sure that this ODEvent is for FormProgressExtended and that the Tag is not null and is a string.
			if(e.Name!=_odEventName || e.Tag==null) {
				return;
			}
			ProgressBarHelper progHelper=new ProgressBarHelper("");
			bool hasProgHelper=false;
			string status="";
			if(e.Tag.GetType()==typeof(string)) {
				status=((string)e.Tag);
			}
			else if(e.Tag.GetType()==typeof(ProgressBarHelper)) {
				progHelper=(ProgressBarHelper)e.Tag;
				status=progHelper.LabelValue;
				hasProgHelper=true;
			}
			else {//Unsupported type passed in.
				return;
			}
			//When the developer wants to close the window, they will send an ODEvent with "DEFCON 1" to signal this form to shut everything down.
			if(status.ToUpper()=="DEFCON 1") {
				DialogResult=DialogResult.OK;
				Close();
				return;
			}
			if(hasProgHelper) {
				switch(progHelper.TagString.ToLower()) {
					case "bringtofront":
						this.TopMost=true;
						this.TopMost=false;
						break;
					case "header":
						this.Text=status;
						break;
					case "1":
						label1.Text=status;
						if(progHelper.PercentValue!=null) {
							label1Percent.Text=progHelper.PercentValue;
						}
						if(progHelper.BlockMax!=0) {
							progressBar1.Maximum=progHelper.BlockMax;
						}
						if(progHelper.BlockValue!=0) {
							if(progHelper.BlockValue>progressBar1.Maximum || progHelper.BlockValue<progressBar1.Minimum) {
								progressBar1.Value=progressBar1.Maximum;
							}
							else {
								progressBar1.Value=progHelper.BlockValue;
							}
						}
						if(progHelper.ProgressStyle==ProgBarStyle.Marquee) {
							progressBar1.Style=ProgressBarStyle.Marquee;
						}
						else if(progHelper.ProgressStyle==ProgBarStyle.Blocks) {
							progressBar1.Style=ProgressBarStyle.Blocks;
						}
						else if(progHelper.ProgressStyle==ProgBarStyle.Continuous) {
							progressBar1.Style=ProgressBarStyle.Continuous;
						}
						if(progHelper.MarqueeSpeed!=0) {
							progressBar1.MarqueeAnimationSpeed=progHelper.MarqueeSpeed;
						}
						break;
					case "2":
						label2.Text=status;
						if(progHelper.PercentValue!=null) {
							label2Percent.Text=progHelper.PercentValue;
						}
						if(progHelper.BlockMax!=0) {
							progressBar2.Maximum=progHelper.BlockMax;
						}
						if(progHelper.BlockValue!=0) {
							if(progHelper.BlockValue>progressBar2.Maximum || progHelper.BlockValue<progressBar2.Minimum) {
								progressBar2.Value=progressBar1.Maximum;
							}
							else {
								progressBar2.Value=progHelper.BlockValue;
							}
						}
						if(progHelper.ProgressStyle==ProgBarStyle.Marquee) {
							progressBar2.Style=ProgressBarStyle.Marquee;
						}
						else if(progHelper.ProgressStyle==ProgBarStyle.Blocks) {
							progressBar2.Style=ProgressBarStyle.Blocks;
						}
						else if(progHelper.ProgressStyle==ProgBarStyle.Continuous) {
							progressBar2.Style=ProgressBarStyle.Continuous;
						}
						if(progHelper.MarqueeSpeed!=0) {
							progressBar2.MarqueeAnimationSpeed=progHelper.MarqueeSpeed;
						}
						break;
					case "3":
						label3.Text=status;
						if(progHelper.PercentValue!=null) {
							label3Percent.Text=progHelper.PercentValue;
						}
						if(progHelper.BlockMax!=0) {
							progressBar3.Maximum=progHelper.BlockMax;
						}
						if(progHelper.BlockValue!=0) {
							if(progHelper.BlockValue>progressBar3.Maximum || progHelper.BlockValue<progressBar3.Minimum) {
								progressBar3.Value=progressBar1.Maximum;
							}
							else {
								progressBar3.Value=progHelper.BlockValue;
							}
						}
						if(progHelper.ProgressStyle==ProgBarStyle.Marquee) {
							progressBar3.Style=ProgressBarStyle.Marquee;
						}
						else if(progHelper.ProgressStyle==ProgBarStyle.Blocks) {
							progressBar3.Style=ProgressBarStyle.Blocks;
						}
						else if(progHelper.ProgressStyle==ProgBarStyle.Continuous) {
							progressBar3.Style=ProgressBarStyle.Continuous;
						}
						if(progHelper.MarqueeSpeed!=0) {
							progressBar3.MarqueeAnimationSpeed=progHelper.MarqueeSpeed;
						}
						break;
					case "4":
						label4.Text=status;
						break;
					case "textbox":
						textMsg.AppendText("\r\n"+status.PadRight(60));
						break;
					case "warningoff":
						labelWarning.Visible=false;
						butPause.Enabled=true;
						butPause.Text=Lan.g(this,"Resume");
						break;
				}
			}
			Application.DoEvents();//So that the label updates with the new status.
		}

		private void butPause_Click(object sender,EventArgs e) {
			if(FormBilling.HasProgressPaused) {
				butPause.Text=Lan.g(this,"Pause");
				FormBilling.HasProgressPaused=false;
			}
			else {
				butPause.Enabled=false;
				labelWarning.Visible=true;
				FormBilling.HasProgressPaused=true;
			}
		}

		private void butClose_Click(object sender,EventArgs e) {
			FormBilling.HasProgressPaused=false;
			FormBilling.HasProgressCanceled=true;
			DialogResult=DialogResult.OK;
		}

		private void FormProgressExtended_FormClosing(object sender,FormClosingEventArgs e) {
			EventInfo fireEvent;
			try {
				fireEvent=_eventType.GetEvent("Fired");
			}
			catch(Exception) {
				fireEvent=typeof(ODEvent).GetEvent("Fired");
			}
			Type fireEventType = fireEvent.EventHandlerType;
			Delegate fireDelegate = Delegate.CreateDelegate(fireEventType, this, "ODEvent_Fired");
			MethodInfo removeHandler = fireEvent.GetRemoveMethod();
			Object[] removeHandlerArgs = { fireDelegate };
			removeHandler.Invoke(this, removeHandlerArgs);
		}

		private void FormProgressExtended_Shown(object sender,EventArgs e) {
			FormBilling.HasProgressOpened=true;
		}
	}
}