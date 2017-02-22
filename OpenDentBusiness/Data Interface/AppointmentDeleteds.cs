using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace OpenDentBusiness{
	///<summary></summary>
	public class AppointmentDeleteds{

		///<summary>Gets one AppointmentDeleted from the db.</summary>
		public static AppointmentDeleted GetByAptNum(long aptNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<AppointmentDeleted>(MethodBase.GetCurrentMethod(),aptNum);
			}
			string command="SELECT * FROM appointmentdeleted WHERE AptNum="+POut.Long(aptNum);
			return Crud.AppointmentDeletedCrud.SelectOne(command);
		}

		///<summary></summary>
		public static long Insert(AppointmentDeleted appointmentDeleted) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				appointmentDeleted.AppointmentDeletedNum=Meth.GetLong(MethodBase.GetCurrentMethod(),appointmentDeleted);
				return appointmentDeleted.AppointmentDeletedNum;
			}
			return Crud.AppointmentDeletedCrud.Insert(appointmentDeleted);
		}


		/*
		Only pull out the methods below as you need them.  Otherwise, leave them commented out.

		///<summary></summary>
		public static List<AppointmentDeleted> Refresh(long patNum){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<AppointmentDeleted>>(MethodBase.GetCurrentMethod(),patNum);
			}
			string command="SELECT * FROM appointmentdeleted WHERE PatNum = "+POut.Long(patNum);
			return Crud.AppointmentDeletedCrud.SelectMany(command);
		}

		///<summary></summary>
		public static void Update(AppointmentDeleted appointmentDeleted){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				Meth.GetVoid(MethodBase.GetCurrentMethod(),appointmentDeleted);
				return;
			}
			Crud.AppointmentDeletedCrud.Update(appointmentDeleted);
		}

		///<summary></summary>
		public static void Delete(long appointmentDeletedNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),appointmentDeletedNum);
				return;
			}
			Crud.AppointmentDeletedCrud.Delete(appointmentDeletedNum);
		}

		

		
		*/



	}
}