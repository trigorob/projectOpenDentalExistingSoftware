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
    public partial class FormKPINonProductivePracticeTime : Form
    {
        public FormKPINonProductivePracticeTime()
        {
            InitializeComponent();
            Lan.F(this);
        }
        private void FormKPINonProductivePracticeTime_Load(object sender, EventArgs e)
        {
            dateStart.SelectionStart = DateTime.Today.AddYears(-1);
            dateEnd.SelectionStart = DateTime.Today;
        }

        private void but_OK_Click(object sender, EventArgs e)
        {
            DataTable tableProvs = NonProductivePracticeTime.GetNonProductivePracticeTime(dateStart.SelectionStart, dateEnd.SelectionStart);

            ReportComplex report = new ReportComplex(true, false);
            report.ReportName = Lan.g(this, "Total Non-Productive Practice Time");
            report.AddTitle("Title", Lan.g(this, "Total Non-Productive Practice Time"));
            report.AddSubTitle("Date", dateStart.SelectionStart.ToShortDateString() + " - " + dateEnd.SelectionStart.ToShortDateString());
            QueryObject query;
            query = report.AddQuery(tableProvs, "", "", SplitByKind.None, 0);
            query.AddColumn("Time (Hours:Min:Seconds)", 200, FieldValueType.String);
            
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
