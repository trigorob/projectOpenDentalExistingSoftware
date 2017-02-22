﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenDentBusiness;

namespace UnitTests {
	public class OperatoryT {

		///<summary></summary>
		public static Operatory CreateOperatory(string abbrev,string opName,long provDentist=0,long provHygienist=0,long clinicNum=0,bool isHygiene=false
			,bool isWebSched=false,int itemOrder=0) 
		{
			Operatory op=new Operatory();
			op.Abbrev=abbrev;
			op.ClinicNum=clinicNum;
			op.IsHygiene=isHygiene;
			op.IsWebSched=isWebSched;
			op.ItemOrder=itemOrder;
			op.OpName=opName;
			op.ProvDentist=provDentist;
			op.ProvHygienist=provHygienist;
			Operatories.Insert(op);
			Operatories.RefreshCache();
			return op;
		}

		///<summary>Deletes everything from the operatory table.  Does not truncate the table so that PKs are not reused on accident.</summary>
		public static void ClearOperatoryTable() {
			string command="DELETE FROM operatory WHERE OperatoryNum > 0";
			DataCore.NonQ(command);
		}

	}
}
