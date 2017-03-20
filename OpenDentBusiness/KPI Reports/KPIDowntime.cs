using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace OpenDentBusiness {
	public class KPIDowntime {

		///<summary> dateStart and dateEnd can be MinVal/MaxVal to indicate "forever".</summary>
		public static DataTable GetDowntime(DateTime dateStart,DateTime dateEnd) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),dateStart,dateEnd);
			}
			DataTable table=new DataTable();
            table.Columns.Add("Provider Name");
            table.Columns.Add("Provider Number");
			table.Columns.Add("Total Down-time");
			
            DataRow row;
            string command = @"
            
            SELECT  provider.MI,provider.LName,provider.FName,provider.ProvNum, sec_to_time((sum((CHAR_LENGTH(appointment.Pattern) - CHAR_LENGTH(REPLACE(appointment.Pattern, 'X', ''))) / CHAR_LENGTH('X')))*avg(apptview.RowsPerIncr)*60)  AS DTime
				FROM appointment 
                LEFT JOIN provider ON (provider.ProvNum = appointment.ProvNum)
                CROSS JOIN apptview
                WHERE EXISTS (	SELECT *
								FROM procedurelog
                                LEFT JOIN procedurecode ON (procedurecode.CodeNum = procedurelog.CodeNum)
                                WHERE procedurelog.AptNum = appointment.AptNum
								AND (procedurecode.ProcCode = 99999 OR procedurecode.ProcCode = 99998)
                                )
                AND appointment.AptStatus = 5
				AND appointment.AptDateTime BETWEEN " + POut.DateT(dateStart) + @" AND " + POut.DateT(dateEnd) + @"
                GROUP BY provider.ProvNum";

            DataTable raw=ReportsComplex.GetTable(command);
			Provider prov;
			for(int i=0;i<raw.Rows.Count;i++) {
				row=table.NewRow();
				prov=new Provider();
				prov.LName=raw.Rows[i]["LName"].ToString();
				prov.FName=raw.Rows[i]["FName"].ToString();
				prov.MI=raw.Rows[i]["MI"].ToString();
				row["Provider Name"]=prov.GetFormalName();
                row["Provider Number"]=raw.Rows[i]["ProvNum"].ToString();
                row["Total Down-time"] = raw.Rows[i]["Dtime"].ToString();
				table.Rows.Add(row);
			}
			return table;
		}


	}
}
