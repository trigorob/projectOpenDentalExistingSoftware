using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace OpenDentBusiness {
    public class KPIPendingTreatments
    {

        ///<summary>If not using clinics then supply an empty list of clinicNums. dateStart and dateEnd can be MinVal/MaxVal to indicate "forever".</summary>
        public static DataTable GetPendingTreatments(DateTime dateStart, DateTime dateEnd)
        {
            if (RemotingClient.RemotingRole == RemotingRole.ClientWeb)
            {
                return Meth.GetTable(MethodBase.GetCurrentMethod(), dateStart, dateEnd);
            }
            DataTable table = new DataTable();
            table.Columns.Add("Name");
            table.Columns.Add("Sex");
            table.Columns.Add("Age");
            table.Columns.Add("Postal Code");
            table.Columns.Add("Date of Service");
            table.Columns.Add("Primary Provider");
            table.Columns.Add("Appointment Description");
            DataRow row;
            /*
            13.Pending treatment(patients waiting for appointment)
            List of all patients whose dental treatment has not been scheduled.This would not include
            procedure code 01202.Extract the information in Chart module/ Planned Appointment tab
            at the top right side of the screen – Report should extract for all patients with entries in this
            section of the chart - patient’s name and ID#, contact information, and procedures planned
            (will include procedure code and description).Note: If the treatment has been scheduled,
            there should not be an entry in the “Planned Appointment” section of the chart.

            This KPI takes information from the Appointment table, 
            finding appointments with the AptStatus of "UnschedList" (3), 
            and pulling patient information from the patient table using the PatNum.
            */

            string command = @"
				SELECT p.LName, p.FName, p.MiddleI, p.Gender, p.Zip, p.PriProv, p.Preferred, r.AptDateTime, p.Birthdate, r.ProcDescript 
FROM patient p 
JOIN appointment r ON r.PatNum = p.PatNum 
WHERE r.AptDateTime = (
	SELECT MAX(r2.AptDateTime) 
	FROM appointment r2
	WHERE r.PatNum = r2.PatNum 
		AND r2.AptStatus = '3'
		## AND r2.CodeNum = 01202 
)";


            DataTable raw = ReportsComplex.GetTable(command);
            Patient pat;
            for (int i = 0; i < raw.Rows.Count; i++)
            {
                row = table.NewRow();
                pat = new Patient();
                pat.LName = raw.Rows[i]["LName"].ToString();
                pat.FName = raw.Rows[i]["FName"].ToString();
                pat.MiddleI = raw.Rows[i]["MiddleI"].ToString();
                pat.Preferred = raw.Rows[i]["Preferred"].ToString();
                row["Name"] = pat.GetNameLF();
                row["Primary Provider"] = Providers.GetAbbr(PIn.Long(raw.Rows[i]["PriProv"].ToString()));
                row["Sex"] = raw.Rows[i]["Gender"].ToString();
                row["Postal Code"] = raw.Rows[i]["Zip"].ToString();
                row["Date of Service"] = raw.Rows[i]["AptDateTime"].ToString().Substring(0, 10);
                row["Appointment Description"] = raw.Rows[i]["ProcDescript"].ToString();
                row["Age"] = birthdate_to_age(raw.Rows[i]["Birthdate"].ToString()); 
                table.Rows.Add(row);
            }
            return table;
        }

        private static string genderFormat(string gNum)
        {
            if (gNum == "0")
            {
                return "M";
            }
            else if (gNum == "1")
            {
                return "F";
            }
            else
            {
                return "Unknown";
            }
        }

        private static int birthdate_to_age(string bd)
        {
            DateTime birthdate = Convert.ToDateTime(bd);
            var today = DateTime.UtcNow;
            var age = today.Year - birthdate.Year;
            if (birthdate > today.AddYears(-age)) age--;
            return age;
        }

    }
}
