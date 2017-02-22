using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenDentBusiness;

namespace UnitTests {
	public class RecallTypeT {

		///<summary></summary>
		public static RecallType CreateRecallType(string description,string procedures,string timePattern,Interval defaultInterval) {
			RecallType recallType=new RecallType();
			recallType.DefaultInterval=defaultInterval;
			recallType.Description=description;
			recallType.Procedures=procedures;
			recallType.TimePattern=timePattern;
			RecallTypes.Insert(recallType);
			RecallTypes.RefreshCache();
			return recallType;
		}

		///<summary>Deletes everything from the recalltype table.  Does not truncate the table so that PKs are not reused on accident.</summary>
		public static void ClearRecallTypeTable() {
			string command="DELETE FROM recalltype WHERE RecallTypeNum > 0";
			DataCore.NonQ(command);
		}
	}
}
