using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace OpenDentBusiness {
	public class KPIActiveRecall {

		///<summary>If not using clinics then supply an empty list of clinicNums. dateStart and dateEnd can be MinVal/MaxVal to indicate "forever".</summary>
		public static DataTable GetActiveRecall(DateTime dateStart,DateTime dateEnd) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),dateStart,dateEnd);
			}
			DataTable table=new DataTable();
			table.Columns.Add("Name");
			table.Columns.Add("Sex");
			table.Columns.Add("Age");
			table.Columns.Add("Postal Code");
			table.Columns.Add("Date of Service");
            table.Columns.Add("Primary Provider");
            DataRow row;
            string command = @"
				SELECT p.LName, p.FName, p.MiddleI, p.Gender, p.Zip, p.PriProv, p.Preferred, r.DateDue, p.Birthdate  
				FROM patient p 
				JOIN recall r ON r.PatNum = p.PatNum
				WHERE r.DateDue BETWEEN " + POut.DateT(dateStart) + @" AND " + POut.DateT(dateEnd) + @"";

			DataTable raw=ReportsComplex.GetTable(command);
			Patient pat;
			for(int i=0;i<raw.Rows.Count;i++) {
				row=table.NewRow();
				pat=new Patient();
				pat.LName=raw.Rows[i]["LName"].ToString();
				pat.FName=raw.Rows[i]["FName"].ToString();
				pat.MiddleI=raw.Rows[i]["MiddleI"].ToString();
				pat.Preferred=raw.Rows[i]["Preferred"].ToString();
				row["Name"]=pat.GetNameLF();
				row["Primary Provider"]=Providers.GetAbbr(PIn.Long(raw.Rows[i]["PriProv"].ToString()));
                row["Sex"] = raw.Rows[i]["Gender"].ToString();
				row["Postal Code"]=raw.Rows[i]["Zip"].ToString();
                row["Date of Service"] = raw.Rows[i]["DateDue"].ToString();
                row["Age"] = raw.Rows[i]["Birthdate"].ToString(); //Change to age properly using a fn TODOKPI
				table.Rows.Add(row);
			}
			return table;
		}

	}
}
