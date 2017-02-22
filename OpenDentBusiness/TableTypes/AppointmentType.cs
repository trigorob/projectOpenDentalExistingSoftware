using System;
using System.Collections;
using System.Drawing;
using System.Xml.Serialization;

namespace OpenDentBusiness{
	
	///<summary>Appointment type is used to override appointment color.  Might control other properties on appointments in the future.</summary>
	[Serializable()]
	[CrudTable(IsSynchable=true)]
	public class AppointmentType:TableBase{
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long AppointmentTypeNum;
		///<summary></summary>
		public string AppointmentTypeName;
		///<summary></summary>
		[XmlIgnore]
		public Color AppointmentTypeColor;
		///<summary>0 based</summary>
		public int ItemOrder;
		///<summary></summary>
		public bool IsHidden;
		///<summary></summary>
		public string Pattern;
		///<summary></summary>
		public string CodeStr;


		[XmlElement("AppointmentTypeColor")]
		public int AppointmentTypeColorAsArgb {
			get { return AppointmentTypeColor.ToArgb(); }
			set { AppointmentTypeColor = Color.FromArgb(value); }
		}

		///<summary>Returns a copy of the appointment.</summary>
		public AppointmentType Clone() {
			return (AppointmentType)this.MemberwiseClone();
		}

		
	}
	
	


}









