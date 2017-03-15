using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDentalWpf;
using OpenDental.Bridges;
using System.Linq;

namespace OpenDental {
	/// <summary>
	/// Summary description for FormKPIMore.
	/// </summary>
	public class FormKPIMore:System.Windows.Forms.Form {
		private OpenDental.UI.Button butClose;
		private Label label1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private OpenDental.UI.ListBoxClickable listKPI;

		///<summary></summary>
		public FormKPIMore() {
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			Lan.F(this);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing) {
			if(disposing) {
				if(components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
            this.label1 = new System.Windows.Forms.Label();
            this.listKPI = new OpenDental.UI.ListBoxClickable();
            this.butClose = new OpenDental.UI.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(104, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(306, 43);
            this.label1.TabIndex = 2;
            this.label1.Text = "KPI List";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // listKPI
            // 
            this.listKPI.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listKPI.FormattingEnabled = true;
            this.listKPI.ItemHeight = 15;
            this.listKPI.Location = new System.Drawing.Point(110, 134);
            this.listKPI.Name = "listKPI";
            this.listKPI.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.listKPI.Size = new System.Drawing.Size(972, 544);
            this.listKPI.TabIndex = 1;
            this.listKPI.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listKPI_MouseDown);
            // 
            // butClose
            // 
            this.butClose.AdjustImageLocation = new System.Drawing.Point(0, 0);
            this.butClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butClose.Autosize = true;
            this.butClose.BtnShape = OpenDental.UI.enumType.BtnShape.Rectangle;
            this.butClose.BtnStyle = OpenDental.UI.enumType.XPStyle.Silver;
            this.butClose.CornerRadius = 4F;
            this.butClose.Location = new System.Drawing.Point(858, 815);
            this.butClose.Name = "butClose";
            this.butClose.Size = new System.Drawing.Size(224, 62);
            this.butClose.TabIndex = 0;
            this.butClose.Text = "Close";
            this.butClose.Click += new System.EventHandler(this.butClose_Click);
            // 
            // FormKPIMore
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(13, 31);
            this.ClientSize = new System.Drawing.Size(1144, 957);
            this.Controls.Add(this.listKPI);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.butClose);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormKPIMore";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Key Performance Indicators";
            this.Load += new System.EventHandler(this.FormKPIMore_Load);
            this.ResumeLayout(false);

		}
		#endregion

		private void FormKPIMore_Load(object sender,EventArgs e) {
			listKPI.Items.AddRange(new string[] { //Delete this comment on deliver: STEP 1 Add report to list
				Lan.g(this,"Patients on Active Recall"),
                Lan.g(this,"Total Non-Productive Practice Time"),
                Lan.g(this,"Provider Downtimes"),

            });
		}
		private void listKPI_MouseDown(object sender,MouseEventArgs e) {
			int selected=listKPI.IndexFromPoint(e.Location);
			if(selected==-1) {
				return;
			}
			switch(selected) { //Delete this comment on deliver: STEP 2 Add case to switch statement
                case 0://Patients on Active Recall
                    FormKPIActiveRecall FormAR = new FormKPIActiveRecall(); //Delete this comment on deliver: STEP 3 Create form cs and designer file (add to project as well) for criteria selection
					FormAR.ShowDialog();
					//SecurityLogs.MakeLogEntry(Permissions.Reports,0,"Active Patients"); TODOKPI ID100
					break;
                case 1://Patients on Active Recall
                    FormKPINonProductivePracticeTime FormNPPT = new FormKPINonProductivePracticeTime(); //Delete this comment on deliver: STEP 3 Create form cs and designer file (add to project as well) for criteria selection
                    FormNPPT.ShowDialog();
                    //SecurityLogs.MakeLogEntry(Permissions.Reports,0,"Active Patients"); TODOKPI ID100
                    break;
                case 2://Patients on Active Recall
                    FormKPIDowntime FormDT = new FormKPIDowntime(); //Delete this comment on deliver: STEP 3 Create form cs and designer file (add to project as well) for criteria selection
                    FormDT.ShowDialog();
                    //SecurityLogs.MakeLogEntry(Permissions.Reports,0,"Active Patients"); TODOKPI ID100
                    break;
            }
		}

		private void butClose_Click(object sender,System.EventArgs e) {
			Close();
		}
	}
}