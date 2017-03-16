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
            DataRow row;
            /*
            13.Pending treatment(patients waiting for appointment)
            List of all patients whose dental treatment has not been scheduled.This would not include
            procedure code 01202.Extract the information in Chart module/ Planned Appointment tab
            at the top right side of the screen – Report should extract for all patients with entries in this
            section of the chart - patient’s name and ID#, contact information, and procedures planned
            (will include procedure code and description).Note: If the treatment has been scheduled,
            there should not be an entry in the “Planned Appointment” section of the chart.
            */

            string command = @"
				SELECT p.LName, p.FName, p.MiddleI, p.Gender, p.Zip, p.PriProv, p.Preferred, r.DateDue, p.Birthdate  
				FROM patient p 
				JOIN recall r ON r.PatNum = p.PatNum
				WHERE r.DateDue BETWEEN " + POut.DateT(dateStart) + @" AND " + POut.DateT(dateEnd) + @"";
            /*
            SELECT *
            FROM appointment
            WHERE AptStatus = "UnschedList"
            
            */


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
                row["Date of Service"] = raw.Rows[i]["DateDue"].ToString();
                row["Age"] = raw.Rows[i]["Birthdate"].ToString(); //Change to age properly using a fn TODOKPI
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
