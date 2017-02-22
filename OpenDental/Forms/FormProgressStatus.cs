using CodeBase;
using System;
using System.Reflection;
using System.Windows.Forms;
using OpenDentBusiness;

namespace OpenDental {
	///<summary>Launch this window in a separate thread so that the progress bar can smoothly spin without waiting on the main thread.
	///Send the phrase "DEFCON 1" in order to have the window gracefully close (as to not rely on thread abort).</summary>
	public partial class FormProgressStatus:Form {
		private string _odEventName;
		private Type _eventType;
		///<summary>Indicates that this progress form is in "history" mode which will show a text box with all messages it takes action on 
		///and then will stay open (even after DEFCON 1 has been recieved) forcing the user to click Close.</summary>
		private bool _hasHistory;
		///<summary>The date and time of the most recent event that this form processed.</summary>
		private DateTime _dateTimeLastEvent;
		///<summary>The date and time that this form was initialized.  Used to help calculate the total time.</summary>
		private DateTime _dateTimeInit;

		///<summary>Do not instatiate this class.  It is not meant for public use.  Use ODProgress.ShowProgressStatus() instead.
		///Launches a progress window that will constantly spin and display status updates for global ODEvents with corresponding name.</summary>
		public FormProgressStatus(string odEventName) : this(odEventName,typeof(ODEvent),false) {

		}

		///<summary>Do not instatiate this class.  It is not meant for public use.  Use ODProgress.ShowProgressStatus() instead.
		///Launches a progress window that will constantly spin and display status updates for global ODEvents with corresponding name.
		///eventType must be a Type that contains an event called Fired.</summary>
		public FormProgressStatus(string odEventName,Type eventType,bool hasHistory=false) {
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
			Type fireEventType=fireEvent.EventHandlerType;
			Delegate fireDelegate=Delegate.CreateDelegate(fireEventType, this, "ODEvent_Fired");
			MethodInfo addHandler=fireEvent.GetAddMethod();
			Object[] addHandlerArgs={ fireDelegate };
			addHandler.Invoke(this, addHandlerArgs);
			_hasHistory=hasHistory;
			if(hasHistory) {
				this.Height+=120;
				this.Width+=60;
				_dateTimeLastEvent=DateTime.MinValue;
				labelMsg.Visible=false;
				textHistoryMsg.Visible=true;
			}
			_dateTimeInit=DateTime.Now;
		}

		public void ODEvent_Fired(ODEventArgs e) {
			//We don't know what thread will cause a progress status change, so invoke this method as a delegate if necessary.
			if(this.InvokeRequired) {
				this.Invoke((Action)delegate() { ODEvent_Fired(e); });
				return;
			}
			//Make sure that this ODEvent is for FormProgressStatus and that the Tag is not null and is a string.
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
			if(this.Visible && _hasHistory && !progressBar.Visible) {
				//Once the progress bar is hidden when history is showing, we never want to process another event.
				return;
			}
			//When the developer wants to close the window, they will send an ODEvent with "DEFCON 1" to signal this form to shut everything down.
			if(status.ToUpper()=="DEFCON 1") {
				if(_hasHistory) {
					butCopyToClipboard.Visible=true;
					butClose.Visible=true;
					progressBar.Visible=false;
				}
				else {
					DialogResult=DialogResult.OK;
					Close();
					return;
				}
			}
			labelMsg.Text=status;
			if(hasProgHelper) {
				if(progHelper.BlockMax!=0) {
					progressBar.Maximum=progHelper.BlockMax;
				}
				if(progHelper.BlockValue!=0) {
					progressBar.Value=progHelper.BlockValue;
				}
				if(progHelper.ProgressStyle==ProgBarStyle.Marquee) {
					progressBar.Style=ProgressBarStyle.Marquee;
				}
				else if(progHelper.ProgressStyle==ProgBarStyle.Blocks) {
					progressBar.Style=ProgressBarStyle.Blocks;
				}
				else if(progHelper.ProgressStyle==ProgBarStyle.Continuous) {
					progressBar.Style=ProgressBarStyle.Continuous;
				}
			}
			if(_hasHistory) {
				if(status.ToUpper()=="DEFCON 1") {
					status=Lan.g(this,"Complete.  Total")+" "+GetElapsedTime(_dateTimeInit,DateTime.Now);
				}
				if(_dateTimeLastEvent==DateTime.MinValue) {
					textHistoryMsg.AppendText(status.PadRight(60));
					_dateTimeLastEvent=DateTime.Now;
				}
				else {
					textHistoryMsg.AppendText(GetElapsedTime(_dateTimeLastEvent,DateTime.Now)+"\r\n"+status.PadRight(60));
					_dateTimeLastEvent=DateTime.Now;
				}
			}
			Application.DoEvents();//So that the label updates with the new status.
		}

		///<summary>Gets a user friendly elapsed time message to display in the history text box.</summary>
		private string GetElapsedTime(DateTime start, DateTime end) {
			TimeSpan timeElapsed=new TimeSpan(end.Ticks-start.Ticks);
			if(timeElapsed.TotalMinutes>2) {
				return Lan.g(this,"Elapsed Time:")+" "+timeElapsed.TotalMinutes+" "+Lan.g(this,"minutes");
			}
			else if(timeElapsed.TotalSeconds>2) {
				return Lan.g(this,"Elapsed Time:")+" "+timeElapsed.TotalSeconds+" "+Lan.g(this,"seconds");
			}
			else {
				return Lan.g(this,"Elapsed Time:")+" "+timeElapsed.TotalMilliseconds+" "+Lan.g(this,"milliseconds");
			}
		}

		private void FormProgressStatus_FormClosing(object sender,FormClosingEventArgs e) {
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
			ODEvent.Fired-=ODEvent_Fired;
		}

		private void butCopyToClipboard_Click(object sender,EventArgs e) {
			try {
				Clipboard.SetText(textHistoryMsg.Text,TextDataFormat.Text);
				MsgBox.Show(this,"Copied");
			}
			catch(Exception) {
				MsgBox.Show(this,"Could not copy contents to the clipboard.  Please try again.");
			}
		}

		private void butClose_Click(object sender,EventArgs e) {
			DialogResult=DialogResult.OK;
			Close();
		}
	}
}