using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Collections;

namespace OpenDentBusiness {
	public class KPINewToRecall {

        //actually just true if new then recall, false if recall then new (aka patient became new again)
        private static bool withinAYear(String startdate, String enddate)
        {
 
            DateTime start = Convert.ToDateTime(startdate);
            DateTime end = Convert.ToDateTime(enddate);

            //if new patient procedure happens after recall, it means patient took break longer than year and is new again
            // if recall happens after new, can be assumed to be within timeframe (else why would it be billed recall?)
            if (start.Year > end.Year ||
                (start.Year == end.Year && start.Month > end.Month)) 
            {
                return false;
            }

            return true;
        }

        ///<summary>If not using clinics then supply an empty list of clinicNums. dateStart and dateEnd can be MinVal/MaxVal to indicate "forever".</summary>
        public static DataTable GetNewToRecall(DateTime dateStart,DateTime dateEnd) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),dateStart,dateEnd);
			}
            DataTable table = new DataTable();
            Dictionary<long, String> patnums = new Dictionary<long, String>();

            table.Columns.Add("Name");
            table.Columns.Add("Gender");
            table.Columns.Add("Age");
            table.Columns.Add("Type of Recall");
            DataRow row;
            string command1 = @"
                SELECT p.PatNum, r.ProcDate
                FROM patient p 
                JOIN procedurelog r ON r.PatNum = p.PatNum 
                WHERE r.ProcDate = (SELECT MAX(r2.ProcDate)
                    FROM procedurelog r2
                    WHERE r.PatNum = r2.PatNum AND
                    (r2.CodeNum = 01101 OR r2.CodeNum = 01102 OR r2.CodeNum = 01103) AND
                    r2.ProcDate BETWEEN " + POut.DateT(dateStart) + @" AND " + POut.DateT(dateEnd) + @")";

            DataTable rawnew = ReportsComplex.GetTable(command1);
            for (int i = 0; i < rawnew.Rows.Count; i++)
            {
                patnums.Add(Convert.ToInt64(rawnew.Rows[i]["PatNum"].ToString()), rawnew.Rows[i]["ProcDate"].ToString());
            }

            string command2 = @"
                SELECT p.PatNum, p.LName, p.FName, p.MiddleI, p.Gender, p.Preferred, r.ProcDate, p.Birthdate  
                FROM patient p 
                JOIN procedurelog r ON r.PatNum = p.PatNum 
                WHERE r.ProcDate = (SELECT MAX(r2.ProcDate)
                    FROM procedurelog r2
                    WHERE r.PatNum = r2.PatNum AND
                    r2.CodeNum = 01202 AND
                    r2.ProcDate BETWEEN " + POut.DateT(dateStart) + @" AND " + POut.DateT(dateEnd) + @")";

            DataTable rawrecall = ReportsComplex.GetTable(command2);
            Patient pat;
            String newdate;
            String recalldate;
            String recalltype;

            for (int j = 0; j < rawrecall.Rows.Count; j++)
            {
                long patnum = Convert.ToInt64(rawrecall.Rows[j]["PatNum"].ToString());

                if (patnums.ContainsKey(patnum))
                {
                    newdate = patnums[patnum];
                    recalldate = rawrecall.Rows[j]["ProcDate"].ToString();
                    recalltype = ReportsComplex.GetTable(@"SELECT t1.Description 
                                                            FROM recalltype t1
	                                                            INNER JOIN recalltrigger t2
		                                                            ON t1.RecallTypeNum = t2.RecallTypeNum
	                                                            INNER JOIN procedurecode t3
		                                                            ON t2.CodeNum = t3.CodeNum
	                                                            INNER JOIN procedurelog t4
		                                                            ON t3.CodeNum = t4.CodeNum
	                                                            INNER JOIN patient t5
		                                                            ON t4.PatNum = t5.PatNum
                                                                    WHERE t5.PatNum = " + patnum + @";").Rows[1]["Description"].ToString();

                    if (!(recalltype == "Prophy" || recalltype == "Child Prophy" || recalltype == "Perio"))
                    {
                        recalltype = "default";
                    }

                    if (withinAYear(newdate, recalldate))
                    {
                        row = table.NewRow();
                        pat = new Patient();
                        pat.LName = rawrecall.Rows[j]["LName"].ToString();
                        pat.FName = rawrecall.Rows[j]["FName"].ToString();
                        pat.MiddleI = rawrecall.Rows[j]["MiddleI"].ToString();
                        pat.Preferred = rawrecall.Rows[j]["Preferred"].ToString();
                        row["Name"] = pat.GetNameLF();
                        row["Type of Recall"] = recalltype; 
                        row["Gender"] = genderFormat(rawnew.Rows[j]["Gender"].ToString());
                        row["Age"] = birthdate_to_age(rawnew.Rows[j]["Birthdate"].ToString()); 
                        table.Rows.Add(row);
                    }

                }
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
