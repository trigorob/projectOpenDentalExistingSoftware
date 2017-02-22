using System;

namespace OpenDentBusiness {
	///<summary>Subscribes a user and optional clinic to specifc alert types.  Users will not get alerts unless they have an entry in this table.</summary>
	[Serializable()]
	[CrudTable(IsSynchable=true)]
	public class AlertSub:TableBase {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long AlertSubNum;
		///<summary>FK to userod.EmployeeNum.</summary>
		public long UserNum;
		///<summary>FK to clinic.ClinicNum. Can be 0.</summary>
		public long ClinicNum;
		///<summary>Enum:AlertType Identifies what type of alert this row is.</summary>
		public AlertType Type;

		public AlertSub() {
		}

		public AlertSub(long userNum,long clinicNum,AlertType type) {
			this.UserNum=userNum;
			this.ClinicNum=clinicNum;
			this.Type=type;
		}

		///<summary></summary>
		public AlertSub Copy() {
			return (AlertSub)this.MemberwiseClone();
		}
	}
}
