using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.ReportingComplex;

namespace OpenDental
{
    public partial class FormKPIDowntime : Form
    {
        public FormKPIDowntime()
        {
            InitializeComponent();
            Lan.F(this);

        }

        private void FormKPIDowntime_Load(object sender, EventArgs e)
        {
            dateStart.SelectionStart = DateTime.Today.AddYears(-1);
            dateEnd.SelectionStart = DateTime.Today;
        }

        private void but_OK_Click(object sender, EventArgs e)
        {
            DataTable tableProvs = KPIDowntime.GetDowntime(dateStart.SelectionStart, dateEnd.SelectionStart);

            ReportComplex report = new ReportComplex(true, false);
            report.ReportName = Lan.g(this, "Provider Down-times");
            report.AddTitle("Title", Lan.g(this, "Provider Down-times"));
            report.AddSubTitle("Date", dateStart.SelectionStart.ToShortDateString() + " - " + dateEnd.SelectionStart.ToShortDateString());
            QueryObject query;
            query = report.AddQuery(tableProvs, "", "", SplitByKind.None, 0);
            query.AddColumn("Provider Name", 150, FieldValueType.String);
            query.AddColumn("Provider Number", 90, FieldValueType.String);
            query.AddColumn("Total Down-time", 100, FieldValueType.String);
            report.AddPageNum();
            if (!report.SubmitQueries())
            {
                return;
            }
            FormReportComplex FormR = new FormReportComplex(report);
            FormR.ShowDialog();
        }

        private void but_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
