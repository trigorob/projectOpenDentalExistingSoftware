using CodeBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace OpenDentBusiness {
	///<summary>HQ only. This is NOT a table type. It is a class that is populated by the Broadcaster at a fairly frequenty interval (30 seconds or so).
	///It is then serialized and saved as a ESerivceSignal via upsert. Each HQ workstation will then select that EServiceSignal very frequently and display the results.</summary>
	[Serializable()]
	public class EServiceMetrics {
		///<summary>Time at which this data was generated. It past a certain threshold in the past then consider the data invalid.</summary>
		public DateTime Timestamp=DateTime.MinValue;
		///<summary>True if all Broadcaster heartbeats are current and not critical; otherwise false.</summary>
		public bool IsBroadcasterHeartbeatOk;
		///<summary>Count of unprocessed warnings issued by Broadcaster.</summary>
		public int Warnings;
		///<summary>Count of unprocessed errors issued by Broadcaster</summary>
		public int Errors;
		///<summary>Count of messages sent out from doctors to patients in the given date range.</summary>
		public int InboundMessageCount;
		///<summary>Count of messages received from patients in the given date range.</summary>
		public int OutboundMessageCount;
		///<summary>Total charges that will be billed to OD customers for the given outbound messages.</summary>
		public float TotalChargedToCustomersUSD;
		///<summary>Retreived from NexmoAPI.GetAccountBalance().</summary>
		public float AccountBalanceEuro;
		///<summary>If true then this data is valid and came from the Broadcaster AccountMaintThread; otherwise this data is not accurate.
		///Will be set after deserialization to indicate that the data was found and deserialized correctly.</summary>
		public bool IsValid;

		///<summary>This is derived property. Do not serialize.</summary>
		[XmlIgnore]
		public eServiceSignalSeverity Severity {
			get {
				if(!IsValid) {
					return eServiceSignalSeverity.Critical;
				}
				if(DateTime.Now.Subtract(Timestamp)>TimeSpan.FromMinutes(5)) {
					return eServiceSignalSeverity.Critical;
				}
				if(!IsBroadcasterHeartbeatOk) {
					return eServiceSignalSeverity.Critical;
				}
				if(Errors>0) {
					return eServiceSignalSeverity.Error;
				}
				if(Warnings>0) {
					return eServiceSignalSeverity.Warning;
				}
				return eServiceSignalSeverity.Working;
			}
		}
		
		public delegate void EServiceMetricsArgs(EServiceMetrics eServiceMetrics);

		///<summary>Gets one EServiceSignalHQ from the serviceshq db located on SERVER184. Returns null in case of failure.</summary>
		public static EServiceMetrics GetEServiceMetricsFromSignalHQ() {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<EServiceMetrics>(MethodBase.GetCurrentMethod());
			}
			//Create an ODThread so that we can safely change the database connection settings without affecting the calling method's connection.
			ODThread odThread=new ODThread(new ODThread.WorkerDelegate((ODThread o) => {
				//Always set the thread static database connection variables to set the serviceshq db conn.
				//Do not commit any changes for DEBUG mode and switching to localhost. Broadcast Monitor always runs in debug, so this needs to connect to server184 even in debug.
				new DataConnection().SetDbT("server184","serviceshq","root","","","",DatabaseType.MySql,true);
				//See EServiceSignalHQs.GetEServiceMetrics() for details.
				string command =@"SELECT 0 EServiceSignalNum, h.* FROM eservicesignalhq h 
					WHERE h.ReasonCode=1024
						AND h.ReasonCategory=1
						AND h.ServiceCode=2
						AND h.RegistrationKeyNum=-1
					ORDER BY h.SigDateTime DESC 
					LIMIT 1";
				EServiceSignal eServiceSignal=Crud.EServiceSignalCrud.SelectOne(command);
				EServiceMetrics eServiceMetric=new EServiceMetrics();
				if(eServiceSignal!=null) {
					using(XmlReader reader=XmlReader.Create(new System.IO.StringReader(eServiceSignal.Tag))) {
						eServiceMetric=(EServiceMetrics)new XmlSerializer(typeof(EServiceMetrics)).Deserialize(reader);
					}
					eServiceMetric.IsValid=true;
				}
				o.Tag=eServiceMetric;
			}));
			odThread.AddExceptionHandler(new ODThread.ExceptionDelegate((Exception e) => { }));//Do nothing
			odThread.Name="eServiceMetricsThread";
			odThread.Start(true);
			if(!odThread.Join(2000)) { //Give this thread up to 2 seconds to complete.
				return null;
			}
			EServiceMetrics retVal=(EServiceMetrics)odThread.Tag;
			return retVal;
		}

		#region Calculate metrics

		///<summary>Get metrics from serviceshq.</summary>
		public static EServiceMetrics CalculateMetricsForToday(float accountBalanceEuro) {
			//No remoting role check, No call to database.
			return CalculateMetricsForDateRange(DateTime.Today,DateTime.Today.AddDays(1),accountBalanceEuro);
		}

		public static void GetErrors(out int errors,out int warnings) {
			//No remoting role check, No call to database.
			errors=0; warnings=0;
			//Count of unprocessed warnings and errors issued by Broadcaster.
			DataTable table=GetBroadcastersErrors();
			foreach(DataRow row in table.Rows) {
				OpenDentBusiness.eServiceSignalSeverity severity=(OpenDentBusiness.eServiceSignalSeverity)PIn.Int(row["Severity"].ToString());
				int count=PIn.Int(row["CountOf"].ToString());
				if(severity==eServiceSignalSeverity.Error) {
					errors+=count;
				}
				else if(severity==eServiceSignalSeverity.Warning) {
					warnings+=count;
				}
			}
		}

		///<summary>Get metrics from serviceshq.</summary>
		/// <param name="dateTimeStart">Used for message counts.</param>
		/// <param name="dateTimeEnd">Used for message counts.</param>
		public static EServiceMetrics CalculateMetricsForDateRange(DateTime dateTimeStart,DateTime dateTimeEnd,float accountBalanceEuro) {
			//No remoting role check, No call to database.
			EServiceMetrics ret=new EServiceMetrics();
			ret.Timestamp=DateTime.Now;
			ret.AccountBalanceEuro=accountBalanceEuro;
			ret.IsBroadcasterHeartbeatOk=GetIsBroadcasterHeartbeatOk();
			ret.Warnings=0;
			ret.Errors=0;
			GetErrors(out ret.Errors,out ret.Warnings);
			DataTable table=GetSmsOutbound(dateTimeStart,dateTimeEnd);
			ret.OutboundMessageCount=PIn.Int(table.Rows[0]["NumMessages"].ToString());
			ret.TotalChargedToCustomersUSD=PIn.Float(table.Rows[0]["MsgChargeUSDTotal"].ToString());
			table=GetSmsInbound(dateTimeStart,dateTimeEnd);
			ret.InboundMessageCount=PIn.Int(table.Rows[0]["NumMessages"].ToString());
			return ret;
		}

		private static bool GetIsBroadcasterHeartbeatOk() {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetBool(MethodBase.GetCurrentMethod());
			}
			//Reason categories are defined by enums: BroadcasterThreadDefs AND ProxyThreadDef.
			string command=@"
				SELECT * FROM (
				  SELECT 
					0 EServiceSignalNum,
					e.*
				  FROM eservicesignalhq e
					WHERE
					  e.RegistrationKeyNum=-1  -- HQ
					  AND (e.ServiceCode=2 OR e.ServiceCode=3) -- IntegratedTexting OR HQProxyService
					  AND 
					  (
						e.ReasonCode=1004 -- Heartbeat
						OR e.ReasonCode=1005 -- ThreadExit
					   ) 
					ORDER BY 
					  e.SigDateTime DESC
				) a
				GROUP BY a.ReasonCategory
				ORDER BY a.SigDateTime DESC;";
			List<EServiceSignal> signals=Crud.EServiceSignalCrud.SelectMany(command);			
			if(signals.Exists(x => x.Severity==eServiceSignalSeverity.Critical || DateTime.Now.Subtract(x.SigDateTime)>TimeSpan.FromMinutes(10))) {
				return false;
			}
			//We got this far so all good.
			return true;
		}

		private static DataTable GetSmsInbound(DateTime dateTimeStart,DateTime dateTimeEnd) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),dateTimeStart,dateTimeEnd);
			}
			//-- Returns Count of outbound messages and total customer charges accrued.
			string command=@"
				SELECT 
				  COUNT(*) NumMessages
				FROM 
				  smsnexmomoterminated t
				WHERE
				  t.DateTimeODRcv>="+POut.DateT(dateTimeStart,true)+@"
				  AND t.DateTimeODRcv <"+POut.DateT(dateTimeEnd,true)+";";
			return Db.GetTable(command);
		}

		private static DataTable GetSmsOutbound(DateTime dateTimeStart,DateTime dateTimeEnd) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod(),dateTimeStart,dateTimeEnd);
			}
			//-- Returns Count of outbound messages and total customer charges accrued.
			string command=@"
				SELECT 
				  COUNT(*) NumMessages,
				  SUM(t.MsgChargeUSD) MsgChargeUSDTotal
				FROM 
				  smsnexmomtterminated t
				WHERE
				  t.MsgStatusCust IN(1,2,3,4)
				  AND t.DateTimeTerminated>="+POut.DateT(dateTimeStart,true)+@"
				  AND t.DateTimeTerminated <"+POut.DateT(dateTimeEnd,true)+";";
			return Db.GetTable(command);
		}

		private static DataTable GetBroadcastersErrors() {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetTable(MethodBase.GetCurrentMethod());
			}
			//-- Returns Count of all unprocessed rows which have severity of Warning or Error.
			string command=@"
				SELECT e.Severity,COUNT(*) CountOf
				  FROM eservicesignalhq e
				WHERE
				  e.RegistrationKeyNum=-1  -- HQ
				  AND e.ServiceCode=2 -- IntegratedTexting
				  AND e.IsProcessed=0 -- NOT processed
				  AND 
				  (
					e.ReasonCode<>1004 -- NOT Heartbeat
					OR e.ReasonCode<>1005 -- NOT ThreadExit
				  ) 
				  AND 
				  (
					e.Severity=3 -- Warning
					OR e.Severity=4 -- Error
				  )
				GROUP BY
				  e.Severity
				;";
			return Db.GetTable(command);
		}

		#endregion
	}	
}
