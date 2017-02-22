using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDentBusiness.WebTypes.WebSched.TimeSlot {
	[Serializable]
	///<summary></summary>
	public class TimeSlot : WebBase {
		///<summary></summary>
		public DateTime DateTimeStart;
		///<summary></summary>
		public DateTime DateTimeStop;
		///<summary></summary>
		public long OperatoryNum;

		public TimeSlot() {

		}

		public TimeSlot(DateTime dateTimeStart,DateTime dateTimeStop,long operatoryNum=0) {
			DateTimeStart=dateTimeStart;
			DateTimeStop=dateTimeStop;
			OperatoryNum=operatoryNum;
		}

		public TimeSlot Copy() {
			return (TimeSlot)this.MemberwiseClone();
		}
	}
}
