using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace OpenDentBusiness
{
    public class NonProductivePracticeTime
    {

        ///<summary> dateStart and dateEnd can be MinVal/MaxVal to indicate "forever".</summary>
        public static DataTable GetNonProductivePracticeTime(DateTime dateStart, DateTime dateEnd)
        {
            if (RemotingClient.RemotingRole == RemotingRole.ClientWeb)
            {
                return Meth.GetTable(MethodBase.GetCurrentMethod(), dateStart, dateEnd);
            }
            DataTable table = new DataTable();
            table.Columns.Add("Total Non-Productive Practice Time");
            
            DataRow row;
            string command = @"
            SELECT  sec_to_time(sum(length(appointment.Pattern))*avg(apptview.RowsPerIncr)*60) AS NonProdTime
				FROM appointment 
                LEFT JOIN patient ON (patient.PatNum = appointment.PatNum)
                CROSS JOIN apptview
                WHERE EXISTS (	SELECT *
								FROM procedurelog 
                                WHERE procedurelog.AptNum = appointment.AptNum
								AND procedurelog.CodeNum = 99998 or 99999)
                AND appointment.AptStatus = 5 
				AND appointment.AptDateTime BETWEEN " + POut.DateT(dateStart) + @" AND " + POut.DateT(dateEnd) + @"";

            DataTable raw = ReportsComplex.GetTable(command);
            for (int i = 0; i < raw.Rows.Count; i++)
            {
                row = table.NewRow();
                row["Total Non-Productive Practice Time"] = raw.Rows[i]["NonProdTime"].ToString();
                table.Rows.Add(row);
            }
            return table;
        }


    }
}

