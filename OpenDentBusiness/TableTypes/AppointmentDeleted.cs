using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OpenDentBusiness {
	///<summary>Inherits from Appointment. All the values from the appointment table will be stored in this table when deleted.</summary>
	[Serializable]
	public class AppointmentDeleted:Appointment {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long AppointmentDeletedNum;

		///<summary>Empty constructor necessary for serialization.</summary>
		public AppointmentDeleted() {	}

		///<summary>Assigns all fields from the Appointment object to the inherited fields on the AppointmentDeleted.</summary>
		public AppointmentDeleted(Appointment apt) {
			FieldInfo[] arrayFieldInfos=typeof(Appointment).GetFields();
			foreach(FieldInfo aptField in arrayFieldInfos) {
				FieldInfo aptDelField=typeof(AppointmentDeleted).GetField(aptField.Name);
				aptDelField.SetValue(this,aptField.GetValue(apt));
			}
		}

		public new AppointmentDeleted Copy() {
			return (AppointmentDeleted)this.MemberwiseClone();
		}
	}
}
