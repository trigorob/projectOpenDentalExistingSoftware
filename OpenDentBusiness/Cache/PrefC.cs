using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using OpenDentBusiness;
using CodeBase;
using System.IO;

namespace OpenDentBusiness {
	public class PrefC {
		private static Dictionary<string,Pref> _dict;
		private static object _lockObj=new object();
		private static bool _isTreatPlanSortByTooth;

		///<summary>Key is prefName.  Can't use the enum, because prefs are allowed to be added by outside programmers, and this framework will support those prefs, too.</summary>
		internal static Dictionary<string,Pref> Dict {
			get {
				return GetDict();
			}
			set {
				lock(_lockObj) {
					_dict=value;
				}
			}
		}

		///<summary>Key is prefName.  Can't use the enum, because prefs are allowed to be added by outside programmers, and this framework will support those prefs, too.</summary>
		public static Dictionary<string,Pref> GetDict() {
			bool isDictNull=false;
			lock(_lockObj) {
				if(_dict==null) {
					isDictNull=true;
				}
			}
			if(isDictNull) {
				Prefs.RefreshCache();
			}
			Dictionary<string,Pref> dictPrefs=new Dictionary<string,Pref>();
			lock(_lockObj) {
				//Jordan approved foreach loop for speed purposes.  Looping through a dictionary of 38,000 items using a for loop took ~22,840ms whereas a foreach loop takes ~10ms.
				foreach(KeyValuePair<string,Pref> kv in _dict) {
					dictPrefs.Add(kv.Key,((Pref)kv.Value).Copy());
				}
			}
			return dictPrefs;
		}

		///<summary>This property is just a shortcut to this pref to make typing faster.  This pref is used a lot.</summary>
		public static bool RandomKeys {
			get {
				return GetBool(PrefName.RandomPrimaryKeys);
			}
		}

		///<summary>This property is just a shortcut to this pref to make typing faster.  This pref is used a lot.</summary>
		public static DataStorageType AtoZfolderUsed {
			get {
				return PIn.Enum<DataStorageType>(GetInt(PrefName.AtoZfolderUsed));
			}
		}

		///<summary>This property is just a shortcut to the negative of the EasyNoClinics pref to make logic easier to follow.</summary>
		public static bool HasClinicsEnabled {
			get {
				return !GetBool(PrefName.EasyNoClinics);
			}
		}

		///<summary>This property is just a shortcut.  Returns true if both StatementsUseSheets and ShowFeatureSuperFamilies are true.</summary>
		public static bool HasSuperStatementsEnabled {
			get {
				return (GetBool(PrefName.StatementsUseSheets) && GetBool(PrefName.ShowFeatureSuperfamilies));
			}
		}

		///<summary>Returns true if DockPhonePanelShow is enabled. Convenience function that should be used if for ODHQ only, and not resellers.</summary>
		/// <returns></returns>
		public static bool IsODHQ {
			get {
				return GetBool(PrefName.DockPhonePanelShow);
			}
		}

		///<summary>Gets a pref of type long.  Pass in a dictionary of preferences to avoid getting a deep copy of the current cache.</summary>
		public static long GetLong(PrefName prefName,Dictionary<string,Pref> dictPrefs=null) {
			if(dictPrefs==null) {
				dictPrefs=GetDict();
			}
			if(!dictPrefs.ContainsKey(prefName.ToString())) {
				throw new Exception(prefName+" is an invalid pref name.");
			}
			return PIn.Long(dictPrefs[prefName.ToString()].ValueString);
		}

		///<summary>Gets a pref of type int32.  Used when the pref is an enumeration, itemorder, etc.  Also used for historical queries in ConvertDatabase.</summary>
		public static int GetInt(PrefName prefName) {
			Dictionary<string,Pref> dictPrefs=GetDict();
			if(!dictPrefs.ContainsKey(prefName.ToString())) {
				throw new Exception(prefName+" is an invalid pref name.");
			}
			return PIn.Int(dictPrefs[prefName.ToString()].ValueString);
		}

		///<summary>Gets a pref of type byte.  Used when the pref is a very small integer (0-255).</summary>
		public static byte GetByte(PrefName prefName) {
			Dictionary<string,Pref> dictPrefs=GetDict();
			if(!dictPrefs.ContainsKey(prefName.ToString())) {
				throw new Exception(prefName+" is an invalid pref name.");
			}
			return PIn.Byte(dictPrefs[prefName.ToString()].ValueString);
		}

		///<summary>Gets a pref of type double.</summary>
		public static double GetDouble(PrefName prefName) {
			Dictionary<string,Pref> dictPrefs=GetDict();
			if(!dictPrefs.ContainsKey(prefName.ToString())) {
				throw new Exception(prefName+" is an invalid pref name.");
			}
			return PIn.Double(dictPrefs[prefName.ToString()].ValueString);
		}

		///<summary>Gets a pref of type bool.</summary>
		public static bool GetBool(PrefName prefName) {
			Dictionary<string,Pref> dictPrefs=GetDict();
			if(!dictPrefs.ContainsKey(prefName.ToString())) {
				throw new Exception(prefName+" is an invalid pref name.");
			}
			return PIn.Bool(dictPrefs[prefName.ToString()].ValueString);
		}

		///<Summary>Gets a pref of type bool, but will not throw an exception if null or not found.  Indicate whether the silent default is true or false.</Summary>
		public static bool GetBoolSilent(PrefName prefName,bool silentDefault) {
			if(HListIsNull()) {
				return silentDefault;
			}
			Dictionary<string,Pref> dictPrefs=GetDict();
			if(!dictPrefs.ContainsKey(prefName.ToString())) {
				return silentDefault;
			}
			return PIn.Bool(dictPrefs[prefName.ToString()].ValueString);
		}

		///<summary>Gets a pref of type string.  Pass in a dictionary of preferences to avoid getting a deep copy of the current cache.</summary>
		public static string GetString(PrefName prefName,Dictionary<string,Pref> dictPrefs=null) {
			if(dictPrefs==null) {
				dictPrefs=GetDict();
			}
			if(!dictPrefs.ContainsKey(prefName.ToString())) {
				throw new Exception(prefName+" is an invalid pref name.");
			}
			return dictPrefs[prefName.ToString()].ValueString;
		}

		///<summary>Gets a pref of type string without using the cache.</summary>
		public static string GetStringNoCache(PrefName prefName) {
			if(RemotingClient.RemotingRole==RemotingRole.ClientWeb) {
				return Meth.GetString(MethodBase.GetCurrentMethod(),prefName);
			}
			string command="SELECT ValueString FROM preference WHERE PrefName='"+POut.String(prefName.ToString())+"'";
			return Db.GetScalar(command);
		}

		///<summary>Gets a pref of type string.  Will not throw an exception if null or not found.</summary>
		public static string GetStringSilent(PrefName prefName) {
			if(HListIsNull()) {
				return "";
			}
			Dictionary<string,Pref> dictPrefs=GetDict();
			if(!dictPrefs.ContainsKey(prefName.ToString())) {
				return "";
			}
			return dictPrefs[prefName.ToString()].ValueString;
		}

		///<summary>Gets a pref of type date.</summary>
		public static DateTime GetDate(PrefName prefName) {
			Dictionary<string,Pref> dictPrefs=GetDict();
			if(!dictPrefs.ContainsKey(prefName.ToString())) {
				throw new Exception(prefName+" is an invalid pref name.");
			}
			return PIn.Date(dictPrefs[prefName.ToString()].ValueString);
		}

		///<summary>Gets a pref of type datetime.</summary>
		public static DateTime GetDateT(PrefName prefName) {
			Dictionary<string,Pref> dictPrefs=GetDict();
			if(!dictPrefs.ContainsKey(prefName.ToString())) {
				throw new Exception(prefName+" is an invalid pref name.");
			}
			return PIn.DateT(dictPrefs[prefName.ToString()].ValueString);
		}

		///<summary>Gets a color from an int32 pref.</summary>
		public static Color GetColor(PrefName prefName) {
			Dictionary<string,Pref> dictPrefs=GetDict();
			if(!dictPrefs.ContainsKey(prefName.ToString())) {
				throw new Exception(prefName+" is an invalid pref name.");
			}
			return Color.FromArgb(PIn.Int(dictPrefs[prefName.ToString()].ValueString));
		}

		///<summary>Used sometimes for prefs that are not part of the enum, especially for outside developers.</summary>
		public static string GetRaw(string prefName) {
			Dictionary<string,Pref> dictPrefs=GetDict();
			if(!dictPrefs.ContainsKey(prefName)) {
				throw new Exception(prefName+" is an invalid pref name.");
			}
			return dictPrefs[prefName].ValueString;
		}

		///<summary>Gets culture info from DB if possible, if not returns current culture.</summary
		public static CultureInfo GetLanguageAndRegion() {
			Dictionary<string,Pref> dictPrefs=new Dictionary<string,Pref>();
			try {
				dictPrefs=GetDict();
			}
			catch(Exception ex) {
				//if no database selected on startup.
			}
			if(dictPrefs.ContainsKey("LanguageAndRegion") && dictPrefs["LanguageAndRegion"].ValueString!="") {
				try {
					return CultureInfo.GetCultureInfo(dictPrefs["LanguageAndRegion"].ValueString);
				}
				catch(Exception ex) { 
					//if saved pref is invalid
				}
			}
			return CultureInfo.CurrentCulture;
		}

		///<summary>Returns true if the XCharge program is enabled and at least one clinic has online payments enabled.</summary>
		public static bool HasOnlinePaymentEnabled() {
			Program prog=Programs.GetCur(ProgramName.Xcharge);
			if(!prog.Enabled) {
				return false;
			}
			List<ProgramProperty> listXChargeProps=ProgramProperties.GetListForProgram(prog.ProgramNum);
			return listXChargeProps.Any(x => x.PropertyDesc=="IsOnlinePaymentsEnabled" && x.PropertyValue=="1");
		}

		///<summary>Used by an outside developer.</summary>
		public static bool ContainsKey(string prefName) {
			Dictionary<string,Pref> dictPrefs=GetDict();
			return dictPrefs.ContainsKey(prefName);
		}

		///<summary>Used by an outside developer.</summary>
		public static bool HListIsNull() {
			bool isDictNull=false;
			lock(_lockObj) {
				if(_dict==null) {
					isDictNull=true;
				}
			}
			return isDictNull;
		}

		///<summary>Only used in the unit tests.  This quick hack has not been tested.</summary>
		public static Dictionary<string,Pref> DictRef{
			get {
				return Dict;
			}
			set {
				Dict=value;
			}
		}
		
		///<summary>Static preference used to always reflect FormOpenDental.IsTreatPlanSortByTooth.  
		///This setter should only be called in FormOpenDental.IsTreatPlanSortByTooth.</summary>
		public static bool IsTreatPlanSortByTooth {
			get {
				return _isTreatPlanSortByTooth;
			}
			set {
				_isTreatPlanSortByTooth=value;
			}
		}

		///<summary>Returns the path to the temporary opendental directory, temp/opendental.  Also performs one-time cleanup, if necessary.  In FormOpenDental_FormClosing, the contents of temp/opendental get cleaned up.</summary>
		public static string GetTempFolderPath() {
			//Will clean up entire temp folder for a month after the enhancement of temp file cleanups as long as the temp\opendental folder doesn't already exist.
			string tempPathOD=ODFileUtils.CombinePaths(Path.GetTempPath(),"opendental");
			if(Directory.Exists(tempPathOD)) {
				//Cleanup has already run for the old temp folder.  Do nothing.
				return tempPathOD;
			}
			Directory.CreateDirectory(tempPathOD);
			if(DateTime.Today>PrefC.GetDate(PrefName.TempFolderDateFirstCleaned).AddMonths(1)) {
				return tempPathOD;
			}
			//This might be used if this is the first time running this version on the computer that did the db update.
			//This might also be used if this is a computer that was turned off for a few weeks around the time of update conversion.
			//We need some sort of time limit just in case it's annoying and keeps happening.
			//So this will have a small risk of missing a computer, but the benefit of limiting outweighs the risk.
			//Empty entire temp folder.  Blank folders will be left behind because they do not matter.
			string[] arrayFileNames=Directory.GetFiles(Path.GetTempPath());
			for(int i=0;i<arrayFileNames.Length;i++) {
				try {
					if(arrayFileNames[i].Substring(arrayFileNames[i].LastIndexOf('.'))==".exe" || arrayFileNames[i].Substring(arrayFileNames[i].LastIndexOf('.'))==".cs") {
						//Do nothing.  We don't care about .exe or .cs files and don't want to interrupt other programs' files.
					}
					else {
						File.Delete(arrayFileNames[i]);
					}
				}
				catch {
					//Do nothing because the file could have been in use or there were not sufficient permissions.
					//This file will most likely get deleted next time a temp file is created.
				}
			}
			return tempPathOD;
		}

		///<summary>Creates a new randomly named file in the given directory path with the given extension and returns the full path to the new file.</summary>
		public static string GetRandomTempFile(string ext) {
			return ODFileUtils.CreateRandomFile(GetTempFolderPath(),ext);
		}

	}
}
