﻿using System;
using System.Collections;
using System.Drawing;

namespace OpenDentBusiness {
	///<summary></summary>
	[Serializable]
	public class UserOdPref:TableBase {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey = true)]
		public long UserOdPrefNum;
		///<summary>FK to userod.UserNum.</summary>
		public long UserNum;
		///<summary>Foreign key to a table associated with FkeyType.  Can be 0 if the user preference does not need a foreign key.</summary>
		public long Fkey;
		///<summary>Enum:UserOdFkeyType Specifies which flag is overridden for the specified definition, since an individual definition can have multiple flags.</summary>
		public UserOdFkeyType FkeyType;
		///<summary>Used to hold the override, which might be a simple primitive value, a comma separated list, or a complex document in xml.</summary>
		public string ValueString;

		///<summary></summary>
		public UserOdPref Clone() {
			return (UserOdPref)this.MemberwiseClone();
		}
	}

	///<summary>These FKey Types are to be used as an identifier for what table the Fkey column is associated to.
	///This enum is not stored as a string so DO NOT reorder it.</summary>
	public enum UserOdFkeyType {
		///<summary>0</summary>
		Definition,
		///<summary>1</summary>
		ClinicLast,
		///<summary>2 - Wiki home pages use ValueString to store the name of the wiki page instead of Fkey due to how FormWiki loads pages.</summary>
		WikiHomePage,
	}
}