using CodeBase;
using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Web;
using System.Windows.Forms;
using System.Linq;

namespace OpenDentBusiness{
	///<summary></summary>
	public class Security{
		///<summary>The current user.  Might be null when first starting the program.  Otherwise, must contain valid user.</summary>
		private static Userod curUser;
		///<summary>Remember the password that the user typed in.  Do not store it in the database.  We will need it when connecting to the web service.  Probably blank if not connected to the web service.  If eCW, then this is already encrypted.</summary>
		public static string PasswordTyped;
		///<summary>Tracks whether or not the user is logged in.  Security.CurUser==null usually is used for this purpose, 
		///but in Middle Tier we do not null out CurUser so that queries can continue to be run on the web service.</summary>
		public static bool IsUserLoggedIn;

		public static Userod CurUser {
			get {
				if(RemotingClient.RemotingRole==RemotingRole.ServerWeb) {
					throw new ApplicationException("Security.Userod not accessible from RemotingRole.ServerWeb.");
				}
				return curUser;
			}
			set {
				if(RemotingClient.RemotingRole==RemotingRole.ServerWeb) {
					throw new ApplicationException("Security.Userod not accessible from RemotingRole.ServerWeb.");
				}
				curUser=value;
			}
		}

		///<summary></summary>
		public Security(){
			//No need to check RemotingRole; no call to db.
		}

		///<summary>Checks to see if current user is authorized.  It also checks any date restrictions.  If not authorized, it gives a Message box saying so and returns false.</summary>
		public static bool IsAuthorized(Permissions perm){
			//No need to check RemotingRole; no call to db.
			return IsAuthorized(perm,DateTime.MinValue,false);
		}

		///<summary>Checks to see if current user is authorized.  It also checks any date restrictions.  If not authorized, it gives a Message box saying so and returns false.</summary>
		public static bool IsAuthorized(Permissions perm,DateTime date){
			//No need to check RemotingRole; no call to db.
			return IsAuthorized(perm,date,false);
		}

		///<summary>Checks to see if current user is authorized.  It also checks any date restrictions.  If not authorized, it gives a Message box saying so and returns false.</summary>
		public static bool IsAuthorized(Permissions perm,bool suppressMessage){
			//No need to check RemotingRole; no call to db.
			return IsAuthorized(perm,DateTime.MinValue,suppressMessage);
		}

		///<summary>Checks to see if current user is authorized.  It also checks any date restrictions.  If not authorized, it gives a Message box saying so and returns false.</summary>
		public static bool IsAuthorized(Permissions perm,DateTime date,bool suppressMessage){
			//No need to check RemotingRole; no call to db.
			return IsAuthorized(perm,date,suppressMessage,false);
		}

		///<summary>Checks to see if current user is authorized.  It also checks any date restrictions.  If not authorized, it gives a Message box saying so and returns false.</summary>
		public static bool IsAuthorized(Permissions perm,DateTime date,bool suppressMessage,bool suppressLockDateMessage) {
			//No need to check RemotingRole; no call to db.
			if(Security.CurUser==null) {
				if(!suppressMessage) {
					MessageBox.Show(Lans.g("Security","Not authorized for")+"\r\n"+GroupPermissions.GetDesc(perm));
				}
				return false;
			}
			try {
				return IsAuthorized(perm,date,suppressMessage,suppressLockDateMessage,curUser.UserGroupNum);
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
				return false;
			}
		}

		///<summary>Will throw an error if not authorized and message not suppressed.</summary>
		public static bool IsAuthorized(Permissions perm,DateTime date,bool suppressMessage,bool suppressLockDateMessage,long userGroupNum) {
			//No need to check RemotingRole; no call to db.
			date=date.Date; //Remove the time portion of date so we can compare strictly as a date later.
			//Check eConnector permission first.
			if(IsValidEServicePermission(perm)) {
				return true;
			}
			if(!GroupPermissions.HasPermission(userGroupNum,perm)){
				if(!suppressMessage){
					throw new Exception(Lans.g("Security","Not authorized.")+"\r\n"
						+Lans.g("Security","A user with the SecurityAdmin permission must grant you access for")+":\r\n"+GroupPermissions.GetDesc(perm));
				}
				return false;
			}
			if(perm==Permissions.AccountingCreate || perm==Permissions.AccountingEdit){
				if(date <= PrefC.GetDate(PrefName.AccountingLockDate)){
					if(!suppressMessage && !suppressLockDateMessage) {
						throw new Exception(Lans.g("Security","Locked by Administrator."));
					}
					return false;	
				}
			}
			//Check the global security lock------------------------------------------------------------------------------------
			if(IsGlobalDateLock(perm,date,suppressMessage||suppressLockDateMessage)) {
				return false;
			}
			//Check date/days limits on individual permission----------------------------------------------------------------
			if(!GroupPermissions.PermTakesDates(perm)){
				return true;
			}
			DateTime dateLimit=GetDateLimit(perm,userGroupNum);
			if(date>dateLimit){//authorized
				return true;
			}
			//Prevents certain bugs when 1/1/1 dates are passed in and compared----------------------------------------------
			//Handling of min dates.  There might be others, but we have to handle them individually to avoid introduction of bugs.
			if(perm==Permissions.ClaimDelete//older versions did not have SecDateEntry
				|| perm==Permissions.ClaimSentEdit//no date sent was entered before setting claim received
				|| perm==Permissions.ProcComplEdit//a completed procedure with a min date.
				|| perm==Permissions.ProcComplEditLimited//because ProcComplEdit was in this list
				|| perm==Permissions.InsPayEdit//a claim payment with no date.
				|| perm==Permissions.InsWriteOffEdit//older versions did not have SecDateEntry or DateEntryC
				|| perm==Permissions.TreatPlanEdit
				|| perm==Permissions.AdjustmentEdit
				|| perm==Permissions.CommlogEdit//usually from a conversion
				|| perm==Permissions.ProcDelete)//because older versions did not set the DateEntryC.
			{
				if(date.Year<1880	&& dateLimit.Year<1880) {
					return true;
				}
			}
			if(!suppressMessage){
				throw new Exception(Lans.g("Security","Not authorized for")+"\r\n"
					+GroupPermissions.GetDesc(perm)+"\r\n"+Lans.g("Security","Date limitation"));
			}
			return false;		
		}

		///<summary>Surrond with Try/Catch. Error messages will be thrown to caller.</summary>
		public static bool IsGlobalDateLock(Permissions perm,DateTime date,bool isSilent=false) {
			if(!(new[] {Permissions.AdjustmentCreate
				,Permissions.AdjustmentEdit
				,Permissions.PaymentCreate
				,Permissions.PaymentEdit
				,Permissions.ProcComplCreate
				,Permissions.ProcComplEdit
			//,Permissions.ProcComplEditLimited
			//,Permissions.ImageDelete
				,Permissions.InsPayCreate
				,Permissions.InsPayEdit
			//,Permissions.InsWriteOffEdit//per Nathan 7/5/2016 this should not be affected by the global date lock
				,Permissions.SheetEdit
				,Permissions.CommlogEdit
				,Permissions.ClaimDelete }).Contains(perm)) 
			{
				return false;//permission being checked is not affected by global lock date.
			}
			if(date.Year==1) {
				return false;//Invalid or MinDate passed in.
			}
			if(!PrefC.GetBool(PrefName.SecurityLockIncludesAdmin) && GroupPermissions.HasPermission(Security.CurUser.UserGroupNum,Permissions.SecurityAdmin)) {
				return false;//admins are never affected by global date limitation when preference enabled.
			}
			//If global lock is Date based.
			if(date <= PrefC.GetDate(PrefName.SecurityLockDate)) {
				if(!isSilent) {
					MessageBox.Show(Lans.g("Security","Locked by Administrator before ")+PrefC.GetDate(PrefName.SecurityLockDate).ToShortDateString());
				}
				return true;
			}
			//If global lock is days based.
			int lockDays = PrefC.GetInt(PrefName.SecurityLockDays);
			if(lockDays>0 && date<=DateTime.Today.AddDays(-lockDays)) {
				if(!isSilent) {
					MessageBox.Show(Lans.g("Security","Locked by Administrator before ")+lockDays.ToString()+" days.");
				}
				return true;
			}
			return false;
		}

		private static DateTime GetDateLimit(Permissions permType,long userGroupNum){
			//No need to check RemotingRole; no call to db.
			DateTime nowDate=MiscData.GetNowDateTime().Date;
			DateTime retVal=DateTime.MinValue;
			for(int i=0;i<GroupPermissionC.List.Length;i++){
				if(GroupPermissionC.List[i].UserGroupNum!=userGroupNum || GroupPermissionC.List[i].PermType!=permType){
					continue;
				}
				//this should only happen once.  One match.
				if(GroupPermissionC.List[i].NewerDate.Year>1880){
					retVal=GroupPermissionC.List[i].NewerDate;
				}
				if(GroupPermissionC.List[i].NewerDays==0){//do not restrict by days
					//do not change retVal
				}
				else if(nowDate.AddDays(-GroupPermissionC.List[i].NewerDays)>retVal){
					retVal=nowDate.AddDays(-GroupPermissionC.List[i].NewerDays);
				}
			}
			return retVal;
		}

		///<summary>Gets a module that the user has permission to use.  Tries the suggestedI first.  If a -1 is supplied, it tries to find any authorized module.  If no authorization for any module, it returns a -1, causing no module to be selected.</summary>
		public static int GetModule(int suggestI){
			//No need to check RemotingRole; no call to db.
			if(suggestI!=-1 && IsAuthorized(PermofModule(suggestI),DateTime.MinValue,true)){
				return suggestI;
			}
			for(int i=0;i<7;i++){
				if(IsAuthorized(PermofModule(i),DateTime.MinValue,true)){
					return i;
				}
			}
			return -1;
		}

		private static Permissions PermofModule(int i){
			//No need to check RemotingRole; no call to db.
			switch(i){
				case 0:
					return Permissions.AppointmentsModule;
				case 1:
					return Permissions.FamilyModule;
				case 2:
					return Permissions.AccountModule;
				case 3:
					return Permissions.TPModule;
				case 4:
					return Permissions.ChartModule;
				case 5:
					return Permissions.ImagesModule;
				case 6:
					return Permissions.ManageModule;
			}
			return Permissions.None;
		}

		///<summary>RemotingRole has not yet been set to ClientWeb, but it will if this succeeds.  Will throw an exception if server cannot validate username and password.  configPath will be empty from a workstation and filled from the server.  If Ecw, odpass will actually be the hash.</summary>
		public static Userod LogInWeb(string oduser,string odpass,string configPath,string clientVersionStr,bool usingEcw) {
			//Very unusual method.  Remoting role can't be checked, but is implied by the presence of a value in configPath.
			if(RemotingClient.RemotingRole==RemotingRole.ServerWeb) {
				Userod user=Userods.CheckUserAndPassword(oduser,odpass,usingEcw);
				if(user==null) {
					throw new Exception("Invalid username or password.");
				}
				string command="SELECT ValueString FROM preference WHERE PrefName='ProgramVersion'";
				string dbVersionStr=Db.GetScalar(command);
				string serverVersionStr=Assembly.GetAssembly(typeof(Db)).GetName().Version.ToString(4);
				#if DEBUG
					if(Assembly.GetAssembly(typeof(Db)).GetName().Version.Build==0) {
						command="SELECT ValueString FROM preference WHERE PrefName='DataBaseVersion'";//Using this during debug in the head makes it open fast with less fiddling.
						dbVersionStr=Db.GetScalar(command);
					}
				#endif
				if(dbVersionStr!=serverVersionStr) {
					throw new Exception("Version mismatch.  Server:"+serverVersionStr+"  Database:"+dbVersionStr);
				}
				Version clientVersion=new Version(clientVersionStr);
				Version serverVersion=new Version(serverVersionStr);
				if(clientVersion > serverVersion){
					throw new Exception("Version mismatch.  Client:"+clientVersionStr+"  Server:"+serverVersionStr);
				}
				//if clientVersion == serverVersion, than we need do nothing.
				//if clientVersion < serverVersion, than an update will later be triggered.
				//Security.CurUser=user;//we're on the server, so this is meaningless
				return user;
				//return 0;//meaningless
			}
			else {
				//Because RemotingRole has not been set, and because CurUser has not been set,
				//this particular method is more verbose than most and does not use Meth.
				//It's not a good example of the standard way of doing things.
				DtoGetObject dto=new DtoGetObject();
				dto.Credentials=new Credentials();
				dto.Credentials.Username=oduser;
				dto.Credentials.Password=odpass;//Userods.EncryptPassword(password);
				dto.MethodName="OpenDentBusiness.Security.LogInWeb";
				dto.ObjectType=typeof(Userod).FullName;
				object[] parameters=new object[] { oduser,odpass,configPath,clientVersionStr,usingEcw };
				Type[] objTypes=new Type[] { typeof(string),typeof(string),typeof(string),typeof(string),typeof(bool) };
				dto.Params=DtoObject.ConstructArray(parameters,objTypes);
				return RemotingClient.ProcessGetObject<Userod>(dto);//can throw exception
			}
		}

		#region eServices

		///<summary>Returns false if the currently logged in user is not designated for the eConnector or if the user does not have permission.</summary>
		private static bool IsValidEServicePermission(Permissions perm) {
			//No need to check RemotingRole; no call to db.
			if(curUser==null) {
				return false;
			}
			//Run specific checks against certain types of eServices.
			switch(curUser.EServiceType) {
				case Userod.EServiceTypes.Broadcaster:
				case Userod.EServiceTypes.BroadcastMonitor:
				case Userod.EServiceTypes.ServiceMainHQ:
					return true;//These eServices are at HQ and we trust ourselves to have full permissions for any S class method.
				case Userod.EServiceTypes.EConnector:
					return IsPermAllowedEConnector(perm);
				case Userod.EServiceTypes.None:
				default:
					return false;//Not an eService, let IsAuthorized handle the permission checking.
			}
		}

		///<summary>Returns true if the eConnector should be allowed to run methods with the passed in permission.</summary>
		private static bool IsPermAllowedEConnector(Permissions perm) {
			//We are typically on the customers eConnector and need to be careful when giving access to certain permission types.
			//Engineers must EXCPLICITLY add permissions to this switch statement as they need them.
			//Be very cautious when adding permissions because the flood gates for that permission will be opened once added.
			//E.g. we should never add a permission like Setup or SecurityAdmin.  If there is a need for such a thing, we need to rethink this paradigm.
			switch(perm) {
				//Add additional permissions to this case as needed to grant access.
				case Permissions.EmailSend:
					return true;
				default:
					return false;
			}
		}

		#endregion


	}
}

























