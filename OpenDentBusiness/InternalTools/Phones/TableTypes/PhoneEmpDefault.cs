using System;
using System.Collections;
using System.Drawing;

namespace OpenDentBusiness{
	///<summary>This table is not part of the general release.  User would have to add it manually.</summary>
	[Serializable]
	[CrudTable(IsMissingInGeneral=true)]
	public class PhoneEmpDefault:TableBase {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long EmployeeNum;
		///<summary></summary>
		public bool IsGraphed;
		///<summary></summary>
		public bool HasColor;
		///<summary>Enum:AsteriskRingGroups 0=all, 1=none, 2=backup</summary>
		public AsteriskQueues RingGroups;
		///<summary>Just makes management easier.  Not used by the program.</summary>
		public string EmpName;
		///<summary>The phone extension for the employee.  e.g. 101,102,etc.  Used to be in the employee table.  This can be changed daily by staff who float from workstation to workstation.  Can be 0 in order to keep two rows from sharing the same extension.</summary>
		public int PhoneExt;
		///<summary>Enum:PhoneEmpStatusOverride </summary>
		public PhoneEmpStatusOverride StatusOverride;
		///<summary>Used to be stored as phoneoverride.Explanation.</summary>
		public string Notes;
		///<summary>This is used by the webcam and phone system.</summary>
		public string ComputerName;
		///<summary>Deprecated.  Always set to true because we no longer capture screen shots.</summary>
		public bool IsPrivateScreen;
		///<summary>Used to launch a task window instead of a commlog window when user clicks on name/phone number on the bottom left.</summary>
		public bool IsTriageOperator;
		///<summary>DEPRECATED. Order of escalation importantance. Employees are ranked 1-n in order of importance. 1 is most important, 'n' is least important. -1 means employee is not included in escalation.</summary>
		public int EscalationOrder;

		///<summary></summary>
		public PhoneEmpDefault Clone() {
			return (PhoneEmpDefault)this.MemberwiseClone();
		}
	}

	///<summary></summary>
	public enum AsteriskQueues {
		///<summary>0 - Adds ext into ODQueueReception and then removes ext from ODQueueTriage and ODQueueBackup.</summary>
		Tech,
		///<summary>1 - Removes ext from ODQueueReception, ODQueueTriage, and ODQueueBackup.</summary>
		None,
		///<summary>2 - Adds ext into ODQueueBackup and then removes ext from ODQueueReception and ODQueueTriage.</summary>
		Backup,
		///<summary>3 - Adds ext into ODQueueTriage and ODQueueBackup then removes ext from ODQueueReception.</summary>
		Triage,
	}

	public enum PhoneEmpStatusOverride {
		///<summary>0 - None.</summary>
		None,
		///<summary>1 </summary>
		Unavailable,
		///<summary>2</summary>
		OfflineAssist
	}

	/*CREATE TABLE `phoneempdefault` (
	
}



