using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace OpenDentBusiness {
	public class KPICompletedCases {

		///<summary>If not using clinics then supply an empty list of clinicNums. dateStart and dateEnd can be MinVal/MaxVal to indicate "forever".</summary>
		public static DataTable GetCompletedCases(DateTime dateStart,DateTime dateEnd) {
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
            /*
            Completed cases
            List of patients compiled from the Treatment Plan module whose treatment has been
            completed within the period designated by the operator
            */

            string command = @"
/*1146 List of patients with completed appointments in the seen date range, and has the specified hygienist on the appointment. 
Gives a procedure fee total from the completed appointments. 
Also totals treatment planned RESTORATIVE procedures, treatment planned in the TP date range, breaking them into TP'd procs on and not on a scheduled appointment.*/
/*Query code written/modified: 06/08/2016*/

SET @SeenFromDate='2016-04-20', @SeenToDate='2016-05-31'; #Set date range for when the patient was seen. Should be for the past, up to present.
SET @TPFromDate='2016-01-01', @TPToDate='2016-12-31'; #Set date range for TPd procs. Should be for the future.
SET @Hyg='%%'; #Set Hyg Abbr here. Leave blank for all providers.

/*--------------------  Do not modify under this line  --------------------*/
SELECT A.AptDate AS 'DateSeen', A.Abbr AS AptHygProv,
CONCAT(p.LName,', ',p.FName)  AS 'Patient',
SUM(A.ProcFee) AS '$CompAptsProcs_',
SUM(IFNULL(A.ProcFeeTot, 0)) AS '$TotalTxPlan_',
SUM(IFNULL(A.ProcFeeAppt, 0)) AS '$TxScheduled_', 
SUM(IFNULL(A.ProcFeeNoAppt, 0)) AS '$TxNotScheduled_'
FROM (
	SELECT GROUP_CONCAT(DISTINCT(DATE(apt.AptDateTime))) AS AptDate, 
	apt.PatNum, apt.AptNum, 
	SUM(proc.GrossProd) AS ProcFee, 
	prov.Abbr,0 AS ProcFeeTot,0 AS ProcFeeNoAppt,0 AS ProcFeeAppt,prov.ProvNum
	FROM appointment apt
	INNER JOIN provider prov ON prov.ProvNum=apt.ProvHyg
		AND prov.Abbr LIKE @Hyg
	INNER JOIN (
		SELECT pl.ProcNum,pl.AptNum,pl.CodeNum,pl.ProcFee*(pl.UnitQty+pl.BaseUnits)-COALESCE(SUM(cp.WriteOff),0) AS GrossProd
		FROM procedurelog pl
		LEFT JOIN claimproc cp ON cp.ProcNum=pl.ProcNum
			AND cp.Status=7 /*CapComplete*/
		GROUP BY pl.ProcNum
	) proc ON proc.AptNum=apt.AptNum
	WHERE apt.AptDateTime BETWEEN @SeenFromDate AND @SeenToDate+INTERVAL 1 DAY
	AND apt.IsHygiene=1
	AND apt.AptStatus=2 #Complete
	GROUP BY apt.PatNum, prov.ProvNum
	
	UNION ALL
	
	SELECT Seen.AptDate AS AptDate,pl.PatNum, pl.AptNum,0 AS ProcFee,Seen.Abbr AS ProvNum,
	(CASE WHEN pl.DateTP BETWEEN @TPFromDate AND @TPToDate 
	THEN pl.ProcFee*(pl.UnitQty+pl.BaseUnits) ELSE 0 END) AS ProcFeeTot,
	(CASE WHEN pl.AptNum=0 	AND pl.DateTP BETWEEN @TPFromDate AND @TPToDate 
	THEN pl.ProcFee*(pl.UnitQty+pl.BaseUnits) ELSE 0 END) AS ProcFeeNoAppt,
	(CASE WHEN ap.AptDateTime BETWEEN  @TPFromDate AND @TPToDate+INTERVAL 1 DAY 
	THEN pl.ProcFee*(pl.UnitQty+pl.BaseUnits) ELSE 0 END) AS ProcFeeAppt,Seen.ProvNum
	FROM (
		SELECT appointment.PatNum, provider.Abbr, GROUP_CONCAT(DISTINCT(DATE(appointment.AptDateTime))) AS AptDate,provider.ProvNum
		FROM appointment 
		INNER JOIN provider ON provider.ProvNum=appointment.ProvHyg
			AND provider.Abbr LIKE @Hyg
		INNER JOIN procedurelog ON procedurelog.AptNum=appointment.AptNum
		WHERE appointment.AptDateTime BETWEEN @SeenFromDate AND @SeenToDate+INTERVAL 1 DAY
		AND appointment.IsHygiene=1
		AND appointment.AptStatus=2 #Complete
		GROUP BY appointment.PatNum, appointment.AptNum
	)Seen
	INNER JOIN procedurelog  pl ON pl.PatNum=Seen.PatNum
	INNER JOIN procedurecode ON procedurecode.CodeNum=pl.CodeNum
		AND (procedurecode.ProcCode BETWEEN 'D2000' AND 'D4000'
		OR procedurecode.ProcCode BETWEEN 'D5000' AND 'D7900')
	LEFT JOIN appointment ap ON ap.AptNum=pl.AptNum
	WHERE pl.ProcStatus=1 #TPd
	GROUP BY pl.ProcNum, Seen.Abbr, pl.AptNum
)A
INNER JOIN patient p ON p.PatNum=A.PatNum
GROUP BY A.PatNum, A.ProvNum
ORDER BY AptHygProv, Patient;
                ";

            /*1073 Completed Procedures in date range with counts for individual procedurecodes and percentage of individual procedurecode/total procedures completed.*/
            /*Query code written/modified: 08/31/2015*/
            //SET @StartDate = '2013-01-01', @EndDate = '2013-12-31'; #Edit date range here.
            /*DO NOT MODIFY @TotalCmplt*/
            /*
            SET @TotalCmplt = (SELECT COUNT(procedurelog.ProcNum) FROM procedurelog WHERE procedurelog.ProcDate BETWEEN @StartDate AND @EndDate AND procedurelog.ProcStatus = 2);
            SELECT procedurecode.ProcCode, procedurecode.Descript, COUNT(procedurecode.ProcCode) AS 'ProcsPerCode',
            @TotalCmplt AS 'TotalProcs',
            CONCAT(FORMAT(ROUND((COUNT(procedurecode.ProcCode) / @TotalCmplt) * 100, 2), 2), '%') AS 'PerCode%'
            FROM procedurelog
            INNER JOIN procedurecode ON procedurelog.CodeNum = procedurecode.CodeNum
            WHERE procedurelog.ProcDate BETWEEN @StartDate AND @EndDate
            AND procedurelog.ProcStatus = 2 #complete
            GROUP BY procedurecode.ProcCode
            ORDER BY procedurecode.ProcCode
            */

            DataTable raw =ReportsComplex.GetTable(command);
			Patient pat;
           
			for(int i=0;i<raw.Rows.Count;i++) {
                
                row =table.NewRow();
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
