namespace OpenDental
{
    partial class FormKPINonProductivePracticeTime
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.but_OK = new System.Windows.Forms.Button();
            this.but_Cancel = new System.Windows.Forms.Button();
            this.label_title = new System.Windows.Forms.Label();
            this.label_to = new System.Windows.Forms.Label();
            this.dateStart = new System.Windows.Forms.MonthCalendar();
            this.dateEnd = new System.Windows.Forms.MonthCalendar();
            this.SuspendLayout();
            // 
            // but_OK
            // 
            this.but_OK.Location = new System.Drawing.Point(820, 572);
            this.but_OK.Name = "but_OK";
            this.but_OK.Size = new System.Drawing.Size(157, 74);
            this.but_OK.TabIndex = 0;
            this.but_OK.Text = "OK";
            this.but_OK.UseVisualStyleBackColor = true;
            this.but_OK.Click += new System.EventHandler(this.but_OK_Click);
            // 
            // but_Cancel
            // 
            this.but_Cancel.Location = new System.Drawing.Point(1151, 572);
            this.but_Cancel.Name = "but_Cancel";
            this.but_Cancel.Size = new System.Drawing.Size(160, 81);
            this.but_Cancel.TabIndex = 1;
            this.but_Cancel.Text = "Cancel";
            this.but_Cancel.UseVisualStyleBackColor = true;
            this.but_Cancel.Click += new System.EventHandler(this.but_Cancel_Click);
            // 
            // label_title
            // 
            this.label_title.AutoSize = true;
            this.label_title.Location = new System.Drawing.Point(165, 46);
            this.label_title.Name = "label_title";
            this.label_title.Size = new System.Drawing.Size(938, 32);
            this.label_title.TabIndex = 2;
            this.label_title.Text = "Used to calculate total non-productive practice time within the date range.";
            // 
            // label_to
            // 
            this.label_to.AutoSize = true;
            this.label_to.Location = new System.Drawing.Point(641, 316);
            this.label_to.Name = "label_to";
            this.label_to.Size = new System.Drawing.Size(54, 32);
            this.label_to.TabIndex = 3;
            this.label_to.Text = "TO";
            // 
            // dateStart
            // 
            this.dateStart.Location = new System.Drawing.Point(37, 117);
            this.dateStart.Name = "dateStart";
            this.dateStart.TabIndex = 4;
            // 
            // dateEnd
            // 
            this.dateEnd.Location = new System.Drawing.Point(825, 117);
            this.dateEnd.Name = "dateEnd";
            this.dateEnd.TabIndex = 5;
            // 
            // FormKPINonProductivePracticeTime
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1384, 694);
            this.Controls.Add(this.dateEnd);
            this.Controls.Add(this.dateStart);
            this.Controls.Add(this.label_to);
            this.Controls.Add(this.label_title);
            this.Controls.Add(this.but_Cancel);
            this.Controls.Add(this.but_OK);
            this.Name = "FormKPINonProductivePracticeTime";
            this.Text = "Non-Productive Practice Time";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button but_OK;
        private System.Windows.Forms.Button but_Cancel;
        private System.Windows.Forms.Label label_title;
        private System.Windows.Forms.Label label_to;
        private System.Windows.Forms.MonthCalendar dateStart;
        private System.Windows.Forms.MonthCalendar dateEnd;
    }
}