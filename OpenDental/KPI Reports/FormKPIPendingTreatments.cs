﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.ReportingComplex;

namespace OpenDental.KPI_Reports
{
    public partial class FormKPIPendingTreatments : Form
    {

        public FormKPIPendingTreatments()
        {
            InitializeComponent();
            Lan.F(this);
        }

        private void FormKPIPendingTreatments_Load(object sender, EventArgs e)
        {
            dateStart.SelectionStart = DateTime.Today;
            dateEnd.SelectionStart = DateTime.Today.AddYears(1);
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            DataTable tablePats = KPIPendingTreatments.GetPendingTreatments(dateStart.SelectionStart, dateEnd.SelectionStart);

            ReportComplex report = new ReportComplex(true, false);
            report.ReportName = Lan.g(this, "Pending Treatments");
            report.AddTitle("Title", Lan.g(this, "Pending Treatments"));
            // report.AddSubTitle("Date", dateStart.SelectionStart.ToShortDateString() + " - " + dateEnd.SelectionStart.ToShortDateString());
            QueryObject query;
            query = report.AddQuery(tablePats, "", "", SplitByKind.None, 0);
            // query.AddColumn("PatNum", 90, FieldValueType.Number);
            query.AddColumn("Name", 150, FieldValueType.String);
            query.AddColumn("Home Phone", 50, FieldValueType.String);
            query.AddColumn("Work Phone", 50, FieldValueType.String);
            query.AddColumn("Wireless Phone", 70, FieldValueType.String);
            query.AddColumn("Email", 100, FieldValueType.String);

            query.AddColumn("Procedure Code", 40, FieldValueType.String);
            query.AddColumn("Description", 250, FieldValueType.String);

            // query.AddColumn("Birthdate", 80, FieldValueType.String);
            // query.AddColumn("Sex", 150, FieldValueType.String);
            // query.AddColumn("Postal Code", 90, FieldValueType.String);
            // query.AddColumn("Date of Service", 100, FieldValueType.String);
            // query.AddColumn("Primary Provider", 40, FieldValueType.String);
            // query.AddGroupSummaryField("Patient Count:", "Name", "Provider", SummaryOperation.Count);
            report.AddPageNum();
            if (!report.SubmitQueries())
            {
                return;
            }
            FormReportComplex FormR = new FormReportComplex(report);
            FormR.ShowDialog();
            //DialogResult=DialogResult.OK;		
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

    }
}