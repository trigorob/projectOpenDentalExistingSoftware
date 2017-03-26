namespace OpenDental{
	partial class FormKPIActiveRecall {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if(disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            this.dateEnd = new System.Windows.Forms.MonthCalendar();
            this.dateStart = new System.Windows.Forms.MonthCalendar();
            this.labelTO = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // dateEnd
            // 
            this.dateEnd.Location = new System.Drawing.Point(697, 32);
            this.dateEnd.Name = "dateEnd";
            this.dateEnd.TabIndex = 57;
            // 
            // dateStart
            // 
            this.dateStart.Location = new System.Drawing.Point(143, 111);
            this.dateStart.Name = "dateStart";
            this.dateStart.TabIndex = 56;
            // 
            // labelTO
            // 
            this.labelTO.Location = new System.Drawing.Point(193, 40);
            this.labelTO.Name = "labelTO";
            this.labelTO.Size = new System.Drawing.Size(72, 23);
            this.labelTO.TabIndex = 58;
            this.labelTO.Text = "TO";
            this.labelTO.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(12, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(492, 16);
            this.label3.TabIndex = 72;
            this.label3.Text = "Used to get a list of all patients on active recall within the date range.";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // FormKPIActiveRecall
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1318, 539);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.dateEnd);
            this.Controls.Add(this.dateStart);
            this.Controls.Add(this.labelTO);
            this.MinimumSize = new System.Drawing.Size(533, 528);
            this.Name = "FormKPIActiveRecall";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Patients on Active Recall";
            this.Load += new System.EventHandler(this.FormKPIActiveRecall_Load);
            this.ResumeLayout(false);

		}

		#endregion

		private OpenDental.UI.Button butOK;
		private OpenDental.UI.Button butCancel;
		private System.Windows.Forms.MonthCalendar dateEnd;
		private System.Windows.Forms.MonthCalendar dateStart;
		private System.Windows.Forms.Label labelTO;
		private System.Windows.Forms.Label label3;
	}
}