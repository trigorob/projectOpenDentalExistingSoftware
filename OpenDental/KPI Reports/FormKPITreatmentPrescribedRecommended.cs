using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenDentBusiness;
using OpenDental.ReportingComplex;

namespace OpenDental
{
    public partial class FormKPIRecTreatment : Form
    {

        public FormKPIRecTreatment()
        {
            InitializeComponent();
            Lan.F(this);
        }

        ComboBox cmbPatient = new ComboBox();
        ComboBox cmbProcCode = new ComboBox();

        private void FormKPIRecTreatment_Load(object sender, EventArgs e)
        {
            dateStart.SelectionStart = DateTime.Today.AddYears(-1);
            dateEnd.SelectionStart = DateTime.Today;
            cmbPatient.Items.Add("ALL");
            cmbProcCode.Items.Add("ALL");
            cmbPatient.SelectedIndex = 0;
            cmbProcCode.SelectedIndex = 0;
            using (SqlConnection conn = new SqlConnection(@"Data Source=opendental;Initial Catalog=Booking;Persist Security Info=True;User ID=sa;Password=123456"))
            {
                try
                {
                    string query = "SELECT LName, FName, PatNum FROM patient ORDER BY LName";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    conn.Open();
                    DataSet ds = new DataSet();
                    da.Fill(ds, "patient");
                    
                    cmbPatient.DisplayMember = "LName" + "FName";
                    cmbPatient.ValueMember = "PatNum";
                    cmbPatient.DataSource = ds.Tables["patient"];
                }
                catch (Exception ex)
                {
                    // write exception info to log or anything else
                    MessageBox.Show("Error occured!");
                }
            }

            using (SqlConnection conn = new SqlConnection(@"Data Source=opendental;Initial Catalog=Booking;Persist Security Info=True;User ID=sa;Password=123456"))
            {
                try
                {
                    string query = "SELECT ProcCode FROM procedurecode ORDER BY ProcCode";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    conn.Open();
                    DataSet ds = new DataSet();
                    da.Fill(ds, "procedurecode");

                    cmbPatient.DisplayMember = "ProcCode";
                    cmbPatient.ValueMember = "ProcCode";
                    cmbPatient.DataSource = ds.Tables["procedurecode"];
                }
                catch (Exception ex)
                {
                    // write exception info to log or anything else
                    MessageBox.Show("Error occured!");
                }
            }

            Controls.Add(cmbPatient);
            Controls.Add(cmbProcCode);

        }

        private void butOK_Click(object sender, EventArgs e)
        {
            DataTable tablePats = new DataTable(); 
            if (cmbPatient.SelectedItem.ToString() == "ALL" && cmbProcCode.SelectedItem.ToString() == "ALL")
            {
                tablePats = KPIRecTreatment.GetRecTreatmentALL(dateStart.SelectionStart, dateEnd.SelectionStart);

            }

          /* else if (cmbPatient.SelectedItem.ToString() == "ALL" && cmbProcCode.SelectedItem.ToString() != "ALL")
           {
                tablePats = KPIRecTreatment.GetRecTreatmentALLPat(dateStart.SelectionStart, dateEnd.SelectionStart, pc.SelectionStart, pnum.SelectionStart);
            }
            
            else if (cmbProcCode.SelectedItem.ToString() == "ALL" && cmbPatient.SelectedItem.ToString() != "ALL")
            {
                tablePats = KPIRecTreatment.GetRecTreatmentALLProc(dateStart.SelectionStart, dateEnd.SelectionStart, pc.SelectionStart, pnum.SelectionStart);

            }*/

            ReportComplex report = new ReportComplex(true, false);
            report.ReportName = Lan.g(this, "Types of Treatment Prescribed/Recommended");
            report.AddTitle("Title", Lan.g(this, "Types of Treatment Prescribed/Recommended"));
            report.AddSubTitle("Date", dateStart.SelectionStart.ToShortDateString() + " - " + dateEnd.SelectionStart.ToShortDateString());
            QueryObject query;
            query = report.AddQuery(tablePats, "", "", SplitByKind.None, 0);
            query.AddColumn("Date of Service", 100, FieldValueType.String);
            query.AddColumn("Name", 150, FieldValueType.String);
            query.AddColumn("Procedure", 40, FieldValueType.String);
            query.AddColumn("Priority", 90, FieldValueType.String);
            query.AddColumn("Status of Pre-Authorization", 80, FieldValueType.String);
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
