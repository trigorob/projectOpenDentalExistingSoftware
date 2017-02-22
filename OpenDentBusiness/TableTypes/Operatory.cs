using System;
using System.Collections;

namespace OpenDentBusiness{
	///<summary>Each row is a single operatory or column in the appts module.</summary>
	[Serializable]
	[CrudTable(IsSynchable=true)]
	public class Operatory:TableBase{
		///<summary>Primary key</summary>
		[CrudColumn(IsPriKey=true)]
		public long OperatoryNum;
		///<summary>The full name to show in the column.</summary>
		public string OpName;
		///<summary>5 char or less. Not used much.</summary>
		public string Abbrev;
		///<summary>The order that this op column will show.  Changing views only hides some ops; it does not change their order.  Zero based.</summary>
		public int ItemOrder;
		///<summary>Used instead of deleting to hide an op that is no longer used.</summary>
		public bool IsHidden;
		///<summary>FK to provider.ProvNum.  The dentist assigned to this op.  If more than one dentist might be assigned to an op, then create a second op and use one for each dentist. If 0, then no dentist is assigned.</summary>
		public long ProvDentist;
		///<summary>FK to provider.ProvNum.  The hygienist assigned to this op.  If 0, then no hygienist is assigned.</summary>
		public long ProvHygienist;
		///<summary>Set true if this is a hygiene operatory.  The hygienist will then be considered the main provider for this op.</summary>
		public bool IsHygiene;
		///<summary>FK to clinic.ClinicNum.  0 if no clinic.</summary>
		public long ClinicNum;
		///<summary>If true patients put into this operatory will have status set to prospective.</summary>
		public bool SetProspective;
		//DateTStamp
		///<summary>Operatories with IsWebSched set to true will be the ONLY operatories considered when searching for available time slots.</summary>
		public bool IsWebSched;
		///<summary>Operatories with IsNewPatAppt set to true will be the ONLY operatories considered when searching for available time slots.
		///This is in regards to the New Patient Appointment portion of the Web Sched web application.</summary>
		public bool IsNewPatAppt;

		///<summary>Returns a copy of this Operatory.</summary>
		public Operatory Copy(){
			return (Operatory)this.MemberwiseClone();
		}

	
	}
	


}













