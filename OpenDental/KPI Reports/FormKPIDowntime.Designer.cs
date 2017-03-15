namespace OpenDental
{
    partial class FormKPIDowntime
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
            this.dateStart = new System.Windows.Forms.MonthCalendar();
            this.dateEnd = new System.Windows.Forms.MonthCalendar();
            this.SuspendLayout();
            // 
            // but_OK
            // 
            this.but_OK.Location = new System.Drawing.Point(18, 427);
            this.but_OK.Name = "but_OK";
            this.but_OK.Size = new System.Drawing.Size(228, 71);
            this.but_OK.TabIndex = 0;
            this.but_OK.Text = "OK";
            this.but_OK.UseVisualStyleBackColor = true;
            this.but_OK.Click += new System.EventHandler(this.but_OK_Click);
            // 
            // but_Cancel
            // 
            this.but_Cancel.Location = new System.Drawing.Point(538, 427);
            this.but_Cancel.Name = "but_Cancel";
            this.but_Cancel.Size = new System.Drawing.Size(212, 71);
            this.but_Cancel.TabIndex = 1;
            this.but_Cancel.Text = "Cancel";
            this.but_Cancel.UseVisualStyleBackColor = true;
            this.but_Cancel.Click += new System.EventHandler(this.but_Cancel_Click);
            // 
            // dateStart
            // 
            this.dateStart.Location = new System.Drawing.Point(18, 9);
            this.dateStart.Name = "dateStart";
            this.dateStart.TabIndex = 2;
            // 
            // dateEnd
            // 
            this.dateEnd.Location = new System.Drawing.Point(538, 9);
            this.dateEnd.Name = "dateEnd";
            this.dateEnd.TabIndex = 3;
            // 
            // FormKPIDowntime
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1221, 510);
            this.Controls.Add(this.dateEnd);
            this.Controls.Add(this.dateStart);
            this.Controls.Add(this.but_Cancel);
            this.Controls.Add(this.but_OK);
            this.Name = "FormKPIDowntime";
            this.Text = "FormKPIDowntime";
            this.Load += new System.EventHandler(this.FormKPIDowntime_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button but_OK;
        private System.Windows.Forms.Button but_Cancel;
        private System.Windows.Forms.MonthCalendar dateStart;
        private System.Windows.Forms.MonthCalendar dateEnd;
    }
}