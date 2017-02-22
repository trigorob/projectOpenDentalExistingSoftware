using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace OpenDentBusiness{
	///<summary></summary>
	public class UserWebs {

		///<summary>Gets one UserWeb from the db.</summary>
		public static UserWeb GetOne(long userWebNum){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				return Meth.GetObject<UserWeb>(MethodBase.GetCurrentMethod(),userWebNum);
			}
			return Crud.UserWebCrud.SelectOne(userWebNum);
		}

		///<summary></summary>
		public static long Insert(UserWeb userWeb){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				userWeb.UserWebNum=Meth.GetLong(MethodBase.GetCurrentMethod(),userWeb);
				return userWeb.UserWebNum;
			}
			return Crud.UserWebCrud.Insert(userWeb);
		}

		///<summary></summary>
		public static void Update(UserWeb userWeb){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				Meth.GetVoid(MethodBase.GetCurrentMethod(),userWeb);
				return;
			}
			Crud.UserWebCrud.Update(userWeb);
		}

		///<summary></summary>
		public static void Update(UserWeb userWeb,UserWeb userWebOld){
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				Meth.GetVoid(MethodBase.GetCurrentMethod(),userWeb);
				return;
			}
			Crud.UserWebCrud.Update(userWeb,userWebOld);
		}

		///<summary></summary>
		public static void Delete(long userWebNum) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),userWebNum);
				return;
			}
			Crud.UserWebCrud.Delete(userWebNum);
		}

		///<summary>Gets the UserWeb associated to the passed in username and hashed password.  Must provide the FKeyType.  Returns null if not found.</summary>
		public static UserWeb GetByUserNameAndPassword(string userName,string passwordHashed,UserWebFKeyType fkeyType) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				return Meth.GetObject<UserWeb>(MethodBase.GetCurrentMethod(),userName,passwordHashed,fkeyType);
			}
			string command="SELECT * "
				+"FROM userweb "
				+"WHERE Password='"+passwordHashed+"' "
				+"AND UserName='"+OpenDentBusiness.POut.String(userName)+"' "
				+"AND FKeyType="+OpenDentBusiness.POut.Int((int)fkeyType)+"";
			return Crud.UserWebCrud.SelectOne(command);
		}

		///<summary>Gets the UserWeb associated to the passed in username.  Must provide the FKeyType.  Returns null if not found.</summary>
		public static UserWeb GetByUserName(string userName,UserWebFKeyType fkeyType) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				return Meth.GetObject<UserWeb>(MethodBase.GetCurrentMethod(),userName,fkeyType);
			}
			string command="SELECT * "
				+"FROM userweb "
				+"WHERE UserName='"+OpenDentBusiness.POut.String(userName)+"' "
				+"AND FKeyType="+OpenDentBusiness.POut.Int((int)fkeyType)+"";
			return Crud.UserWebCrud.SelectOne(command);
		}

		///<summary>Gets the UserWeb associated to the passed in username and reset code.  Must provide the FKeyType.  Returns null if not found.</summary>
		public static UserWeb GetByUserNameAndResetCode(string userName,string resetCode,UserWebFKeyType fkeyType) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				return Meth.GetObject<UserWeb>(MethodBase.GetCurrentMethod(),userName,resetCode,fkeyType);
			}
			string command="SELECT * "
				+"FROM userweb "
				+"WHERE userweb.FKeyType="+POut.Int((int)UserWebFKeyType.PatientPortal)+" "
				+"AND userweb.UserName='"+POut.String(userName)+"' "
				+"AND userweb.PasswordResetCode='"+POut.String(resetCode)+"' ";
			return Crud.UserWebCrud.SelectOne(command);
		}

		public static UserWeb GetByFKeyAndType(long fkey,UserWebFKeyType fkeyType) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				return Meth.GetObject<UserWeb>(MethodBase.GetCurrentMethod(),fkey,fkeyType);
			}
			string command="SELECT * FROM userweb WHERE FKey="+POut.Long(fkey)+" AND FKeyType="+POut.Int((int)fkeyType)+" ";
			return Crud.UserWebCrud.SelectOne(command);
		}

		public static bool UserNameExists(string userName,UserWebFKeyType fkeyType) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				return Meth.GetBool(MethodBase.GetCurrentMethod(),userName,fkeyType);
			}
			string command="SELECT COUNT(*) FROM userweb WHERE UserName='"+POut.String(userName)+"' AND FKeyType="+POut.Int((int)fkeyType)+" ";
			string count=Db.GetCount(command);
			if(count!="0") {
				return true;
			}
			return false;
		}

	}
}