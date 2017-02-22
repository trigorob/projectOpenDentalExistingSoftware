using System;

namespace OpenDentBusiness {
	///<summary>Any row in this table will show up in the main menu of Open Dental to get the attention of the user.
	///The user will be able to click on the alert and take an action.  The actions available to the user are also determined in this row.</summary>
	[Serializable()]
	[CrudTable(IsSynchable=true)]
	public class AlertItem:TableBase{
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long AlertItemNum;
		///<summary>FK to clinic.ClinicNum. Can be 0.</summary>
		public long ClinicNum;
		///<summary>What is displayed in the menu item.</summary>
		public String Description;
		///<summary>Enum:AlertType Identifies what type of alert this row is.</summary>
		public AlertType Type;
		///<summary>Enum:SeverityType The severity will help determine what color this alert should be in the main menu.</summary>
		public SeverityType Severity;
		///<summary>Enum:ActionType Bitwise flag that represents what actions are available for this alert.</summary>
		public ActionType Actions;

		public AlertItem() {
			
		}

		///<summary></summary>
		public AlertItem Copy() {
			return (AlertItem)this.MemberwiseClone();
		}
	}

	///<summary>Enum representing different alert types.</summary>
	public enum AlertType {
		///<summary>Generic. Informational, has no action associated with it</summary>
		Generic,
		///<summary>Opens the Online Payments Window when clicked</summary>
		OnlinePaymentsPending
	}

	///<summary>Represents the urgency of the alert.  Also determines the color for the menu item in the main menu.</summary>
	public enum SeverityType {
		///<summary>0 - White</summary>
		Normal,
		///<summary>1 - Yellow</summary>
		Low,
		///<summary>2 - Orange</summary>
		Medium,
		///<summary>3 - Red</summary>
		High
	}

	///<summary>The possible actions that can be taken on this alert.  Multiple actions can be available for one alert.</summary>
	[Flags]
	public enum ActionType {
		///<summary></summary>
		None=0,
		///<summary></summary>
		MarkAsRead=1,
		///<summary></summary>
		OpenForm=2
	}

}
