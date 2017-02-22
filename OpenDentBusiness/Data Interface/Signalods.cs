using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CodeBase;
using OpenDentBusiness.UI;

namespace OpenDentBusiness {
	///<summary></summary>
	public class Signalods {
		///<summary>This is not the actual date/time last refreshed.  It is really the server based date/time of the last item in the database retrieved on previous refreshes.  That way, the local workstation time is irrelevant.</summary>
		public static DateTime SignalLastRefreshed;
		///<summary>Set from FormOpenDental to the method that starts the signal processor timer.</summary>
		public static Action StartTimer;
		///<summary>Set from FormOpenDental to the method that stops the signal processor timer.</summary>
		public static Action StopTimer;
		///<summary>Set from FormOpenDental to the method that is used to perform the shutdown workstation function.</summary>
		public static Action OnShutdown;
		private static List<ISignalProcessor> _listISignalProcessors = new List<ISignalProcessor>();

		///<summary>Called in Form_Load() to subscribe a given form the signals.</summary>
		public static bool Subscribe(Form form) {
			//No need to check RemotingRole; no call to db.
			ISignalProcessor sigProcessor = form as ISignalProcessor;
			if(sigProcessor==null) {
				return false;
			}
			_listISignalProcessors.Add(sigProcessor);
			form.FormClosed+=delegate {
				//todo: ISignalProcess may need an identifier so we know "which" one to remove. 
				//EIther that or _listISignalProcessors should be a list of Forms, not a list of ISignalProcessor. Form has a built-in identifier so this should probably work.
				_listISignalProcessors.Remove(sigProcessor);
			};
			return true;
		}

		///<summary>Retreives new signals from the DB, updates Caches, and broadcasts signals to all subscribed forms.
		///Returns the list of signals processed if there weren't any errors.  Otherwise, returns an empty list.</summary>
		public static List<Signalod> SignalsTick(List<Signalod> listLocalSignals=null) {
			//No need to check RemotingRole; no call to db.
			try {
				if(StopTimer!=null) {
					StopTimer();
				}
				//Get new signals from DB.
				List<Signalod> listSignals=RefreshTimed(SignalLastRefreshed);
				//Only update the time stamp with signals retreived from the DB. Do NOT use listLocalSignals to set timestamp.
				if(listSignals.Count>0) {
					SignalLastRefreshed=listSignals.Max(x => x.SigDateTime);
				}
				if(listLocalSignals!=null) {
					listSignals.AddRange(listLocalSignals);
				}
				if(listSignals.Count==0) {
					return new List<Signalod>();
				}
				if(listSignals.Exists(x => x.IType==InvalidType.ShutDownNow)) {
					OnShutdown();//should never ever be null.
					return new List<Signalod>();
				}
				List<Signalod> listFeeSignals=listSignals.FindAll(x => x.IType==InvalidType.Fees && x.FKeyType==KeyType.FeeSched && x.FKey>0);
				if(listFeeSignals.Count>0) {
					Fees.InvalidateFeeSchedules(listFeeSignals.Select(x => x.FKey).ToList());
				}
				InvalidType[] cacheRefreshArray = listSignals.FindAll(x => x.FKey==0 && x.FKeyType==KeyType.Undefined).Select(x => x.IType).Distinct().ToArray();
				Cache.Refresh(cacheRefreshArray);//refresh invalid data
				BroadcastSignals(listSignals);
				return listSignals;
			}
			catch {
				DateTime dateTimeRefreshed;
				try {
					//Signal processing should always use the server's time.
					dateTimeRefreshed=MiscData.GetNowDateTime();
				}
				catch {
					//If the server cannot be reached, we still need to move the signal processing forward so use local time as a fail-safe.
					dateTimeRefreshed=DateTime.Now;
				}
				SignalLastRefreshed=dateTimeRefreshed;
			}
			finally {
				if(StartTimer!=null) {
					StartTimer();
				}
			}
			return new List<Signalod>();
		}

		public static void BroadcastSignals(List<Signalod> listSignals) {
			//No need to check RemotingRole; no call to db.
			_listISignalProcessors.ForEach(x => { try { x.ProcessSignals(listSignals); } catch {/*Do Nothing, or show msgbox?*/} });
		}

		///<summary>Gets all Signals since a given DateTime.  If it can't connect to the database, then it returns a list of length 0.
		///Remeber that the supplied dateTime is server time.  This has to be accounted for.</summary>
		public static List<Signalod> RefreshTimed(DateTime sinceDateT) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetObject<List<Signalod>>(MethodBase.GetCurrentMethod(),sinceDateT);
			}
			//This command was written to take into account the fact that MySQL truncates seconds to the the whole second on DateTime columns. (newer versions support fractional seconds)
			//By selecting signals less than Now() we avoid missing signals the next time this function is called. Without the addition of Now() it was possible
			//to miss up to ((N-1)/N)% of the signals generated in the worst case scenario.
			string command="SELECT * FROM signalod "
				+"WHERE (SigDateTime>"+POut.DateT(sinceDateT)+" AND SigDateTime< "+DbHelper.Now()+") "
				+"ORDER BY SigDateTime";
			//note: this might return an occasional row that has both times newer.
			List<Signalod> listSignals=new List<Signalod>();
			try {
				listSignals=Crud.SignalodCrud.SelectMany(command);
			} 
			catch {
				//we don't want an error message to show, because that can cause a cascade of a large number of error messages.
			}
			return listSignals;
		}

		///<summary>Process all Signals and Acks Since a given DateTime.  Only to be used by OpenDentalWebService.
		///Returns latest valid signal Date/Time and the list of InvalidTypes that were refreshed.
		///Can throw exception.</summary>
		public static List<InvalidType> RefreshForWeb(ref DateTime sinceDateT) {
			InvalidType[] arrayInvalidTypes=new InvalidType[0];
			//No need to check RemotingRole; no call to db.
			try {
				if(sinceDateT.Year<1880) {
					sinceDateT=MiscData.GetNowDateTime();
				}
				arrayInvalidTypes=Signalods.GetInvalidTypesForWeb(Signalods.RefreshTimed(sinceDateT));
				//Get all invalid types since given time and refresh the cache for those given invalid types.
				Cache.Refresh(arrayInvalidTypes);
			}
			catch(Exception e) {
				//Most likely cause for an exception here would be a thread collision between 2 consumers trying to refresh the cache at the exact same instant.
				//There is a chance that performing as subsequent refresh here would cause yet another collision but it's the best we can do without redesigning the entire cache pattern.
				Cache.Refresh(InvalidType.AllLocal);
				throw new Exception("Server cache may be invalid. Please try again. Error: "+e.Message);
			}
			finally {
				DateTime dateTimeNow=DateTime.Now;
				try {
					dateTimeNow=OpenDentBusiness.MiscData.GetNowDateTime();
				}
				catch(Exception) { }
				sinceDateT=dateTimeNow;
			}
			return arrayInvalidTypes.ToList();
		}

		///<summary></summary>
		public static long Insert(Signalod sig) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				sig.SignalNum=Meth.GetLong(MethodBase.GetCurrentMethod(),sig);
				return sig.SignalNum;
			}
			//we need to explicitly get the server time in advance rather than using NOW(),
			//because we need to update the signal object soon after creation.
			sig.SigDateTime=MiscData.GetNowDateTime();
			return Crud.SignalodCrud.Insert(sig);
		}

		///<summary>Simplest way to use the new fKey and FKeyType. Set isBroadcast=true to process signals immediately on workstation.</summary>
		public static void SetInvalid(InvalidType iType,KeyType fKeyType,long fKey) {
			//Remoting role check performed in the Insert.
			Signalod sig=new Signalod();
			sig.IType=iType;
			sig.DateViewing=DateTime.MinValue;
			sig.FKey=fKey;
			sig.FKeyType=fKeyType;
			Insert(sig);
		}

		///<summary>Pass one or two appointments into this function to send 2 to 6 invalid signals depending on the changes made to the appointment.
		/// Always call a refresh of the appointment module before calling this method.
		/// Cannot pass null for both parameters.</summary>
		/// <param name="apptNew">Required. If changes are made to an appointment or a new appointment is made, it should be passed in here.</param>
		/// <param name="apptOld">Optional. Only used if changes are being made to an existing appointment.</param>
		public static void SetInvalidAppt(Appointment apptNew,Appointment apptOld = null) {
			if(apptNew==null) {
				//If apptOld is not null then use it as the apptNew so we can send signals
				//Most likely occurred due to appointment delete.
				if(apptOld!=null) {
					apptNew=apptOld;
					apptOld=null;
				}
				else {
					return;//should never happen. Both apptNew and apptOld are null in this scenario
				}
			}
			//The six possible signals are:
			//  1.New Provider
			//  2.New Hyg
			//  3.New Op
			//  4.Old Provider
			//  5.Old Hyg
			//  6.Old Op
			//If there is no change between new and old, or if there is not an old appt provided, then fewer than 6 signals may be generated.
			List<Signalod> listSignals = new List<Signalod>();
			//  1.New Provider
			listSignals.Add(
				new Signalod() {
					DateViewing=apptNew.AptDateTime,
					IType=InvalidType.Appointment,
					FKey=apptNew.ProvNum,
					FKeyType=KeyType.Provider,
				});
			//  2.New Hyg
			if(apptNew.ProvHyg>0) {
				listSignals.Add(
					new Signalod() {
						DateViewing=apptNew.AptDateTime,
						IType=InvalidType.Appointment,
						FKey=apptNew.ProvHyg,
						FKeyType=KeyType.Provider,
					});
			}
			//  3.New Op
			if(apptNew.Op>0) {
				listSignals.Add(
					new Signalod() {
						DateViewing=apptNew.AptDateTime,
						IType=InvalidType.Appointment,
						FKey=apptNew.Op,
						FKeyType=KeyType.Operatory,
					});
			}
			//  4.Old Provider
			if(apptOld!=null && apptOld.ProvNum>0 && (apptOld.AptDateTime.Date!=apptNew.AptDateTime.Date || apptOld.ProvNum!=apptNew.ProvNum)) {
				listSignals.Add(
					new Signalod() {
						DateViewing=apptOld.AptDateTime,
						IType=InvalidType.Appointment,
						FKey=apptOld.ProvNum,
						FKeyType=KeyType.Provider,
					});
			}
			//  5.Old Hyg
			if(apptOld!=null && apptOld.ProvHyg>0 && (apptOld.AptDateTime.Date!=apptNew.AptDateTime.Date || apptOld.ProvHyg!=apptNew.ProvHyg)) {
				listSignals.Add(
					new Signalod() {
						DateViewing=apptOld.AptDateTime,
						IType=InvalidType.Appointment,
						FKey=apptOld.ProvHyg,
						FKeyType=KeyType.Provider,
					});
			}
			//  6.Old Op
			if(apptOld!=null && apptOld.Op>0 && (apptOld.AptDateTime.Date!=apptNew.AptDateTime.Date || apptOld.Op!=apptNew.Op)) {
				listSignals.Add(
					new Signalod() {
						DateViewing=apptOld.AptDateTime,
						IType=InvalidType.Appointment,
						FKey=apptOld.Op,
						FKeyType=KeyType.Operatory,
					});
			}
			listSignals.ForEach(x=>Insert(x));
			//There was a delay when using this method to refresh the appointment module due to the time it takes to loop through the signals that iSignalProcessors need to loop through.
			//BroadcastSignals(listSignals);//for immediate update. Signals will be processed again at next tick interval.
		}

		///<summary>Inserts a signal which tells all client machines to update the received unread SMS message count inside the Text button of the main toolbar.  To get the current count from the database, use SmsFromMobiles.GetSmsNotification().</summary>
		public static long InsertSmsNotification(long smsReceivedUnreadCount) {
			Signalod sig=new Signalod();
			sig.IType=InvalidType.SmsTextMsgReceivedUnreadCount;
			sig.FKeyType=KeyType.SmsMsgUnreadCount;
			sig.FKey=smsReceivedUnreadCount;
			return Signalods.Insert(sig);
		}

		///<summary>After a refresh, this is used to determine whether the Appt Module needs to be refreshed.  Must supply the current date showing as well as the recently retrieved signal list.</summary>
		public static bool ApptNeedsRefresh(List<Signalod> signalList,DateTime dateTimeShowing) {
			//No need to check RemotingRole; no call to db.
			List<Signalod> listApptSignals=signalList.FindAll(x => x.IType==InvalidType.Appointment && x.DateViewing.Date==dateTimeShowing.Date);
			if(listApptSignals.Count==0) {
				return false;
			}
			List<long> visibleOps = ApptDrawing.VisOps.Select(x => x.OperatoryNum).ToList();
			List<long> visibleProvs = ApptDrawing.VisProvs.Select(x => x.ProvNum).ToList();
			if(listApptSignals.Any(x=> x.FKeyType==KeyType.Operatory && visibleOps.Contains(x.FKey))
				|| listApptSignals.Any(x=> x.FKeyType==KeyType.Provider && visibleProvs.Contains(x.FKey))) 
			{
				return true;
			}
			return false;
		}


		///<summary>After a refresh, this is used to get a list containing all flags of types that need to be refreshed. The FKey must be 0 and the
		///FKeyType must Undefined. Types of Task and SmsTextMsgReceivedUnreadCount are not included.</summary>
		public static InvalidType[] GetInvalidTypes(List<Signalod> signalodList) {
			//No need to check RemotingRole; no call to db.
			return signalodList.FindAll(x => x.IType!=InvalidType.Task
					&& x.IType!=InvalidType.TaskPopup
					&& x.IType!=InvalidType.SmsTextMsgReceivedUnreadCount
					&& x.FKey==0
					&& x.FKeyType==KeyType.Undefined)
				.Select(x => x.IType).ToArray();
		}


		///<summary>Our eServices have not been refactored yet to handle granular refreshes yet. This method does include signals that have a FKey. 
		///Ideally this method will be deprecated once eServices uses FKeys in cache refreshes.</summary>
		public static InvalidType[] GetInvalidTypesForWeb(List<Signalod> signalodList) {
			//No need to check RemotingRole; no call to db.
			return signalodList.FindAll(x => x.IType!=InvalidType.Task
					&& x.IType!=InvalidType.TaskPopup
					&& x.IType!=InvalidType.SmsTextMsgReceivedUnreadCount)
					//TODO: Future enhancement is to rejoin this method with GetInvalidTypes. To do that we will need to have our eServices refresh parts of 
					//caches based on FKey.
				.Select(x => x.IType).ToArray();
		}

		/// <summary>Won't work with InvalidType.Date, InvalidType.Task, or InvalidType.TaskPopup  yet.</summary>
		public static void SetInvalid(params InvalidType[] arrayITypes) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),arrayITypes);
				return;
			}
			foreach(InvalidType iType in arrayITypes) {
				Signalod sig=new Signalod();
				sig.IType=iType;
				sig.DateViewing=DateTime.MinValue;
				Insert(sig);
			}
		}

		///<summary>Insertion logic that doesn't use the cache. Has special cases for generating random PK's and handling Oracle insertions.</summary>
		public static void SetInvalidNoCache(params InvalidType[] itypes) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb){
				Meth.GetVoid(MethodBase.GetCurrentMethod(),itypes);
				return;
			}
			foreach(InvalidType iType in itypes) {
				Signalod sig=new Signalod();
				sig.IType=iType;
				sig.DateViewing=DateTime.MinValue;
				sig.SigDateTime=MiscData.GetNowDateTime();
				Crud.SignalodCrud.InsertNoCache(sig);
			}
		}

		///<summary>Must be called after Preference cache has been filled.
		///Deletes all signals older than 2 days if this has not been run within the last week.  Will fail silently if anything goes wrong.</summary>
		public static void ClearOldSignals() {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				Meth.GetVoid(MethodBase.GetCurrentMethod());
				return;
			}
			try {
				if(PrefC.GetDict().ContainsKey(PrefName.SignalLastClearedDate.ToString())
					&& PrefC.GetDateT(PrefName.SignalLastClearedDate)>MiscData.GetNowDateTime().AddDays(-7)) //Has already been run in the past week. This is all server based time.
				{
					return;//Do not run this process again.
				}
				string command="";
				if(DataConnection.DBtype==DatabaseType.MySql) {//easier to read that using the DbHelper Functions and it also matches the ConvertDB3 script
					command="DELETE FROM signalod WHERE SigDateTime < DATE_ADD(NOW(),INTERVAL -2 DAY)";//Itypes only older than 2 days
					Db.NonQ(command);
				}
				else {//oracle
					command="DELETE FROM signalod WHERE SigDateTime < CURRENT_TIMESTAMP -2";//Itypes only older than 2 days
					Db.NonQ(command);
				}
				SigMessages.ClearOldSigMessages();//Clear messaging buttons which use to be stored in the signal table.
				//SigElements.DeleteOrphaned();
				Prefs.UpdateDateT(PrefName.SignalLastClearedDate,MiscData.GetNowDateTime());//Set Last cleared to now.
			}
			catch(Exception) {
				//fail silently
			}
		}
	}

	

	


}




















