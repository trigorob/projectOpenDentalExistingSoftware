﻿using System;
using System.Collections.Generic;
using System.Text;
using OpenDentBusiness;

namespace UnitTests {
	public class InsPlanT {
		///<summary>Creats an insurance plan with the default fee schedule of 53.</summary>
		public static InsPlan CreateInsPlan(long carrierNum) {
			return CreateInsPlan(carrierNum,EnumCobRule.Basic);
		}

		///<summary>Creates an insurance plan with the default fee schedule of 53.</summary>
		public static InsPlan CreateInsPlan(long carrierNum,EnumCobRule cobRule) {
			InsPlan plan=new InsPlan();
			plan.CarrierNum=carrierNum;
			plan.PlanType="";
			plan.FeeSched=53;
			plan.CobRule=cobRule;
			InsPlans.Insert(plan);
			return plan;
		}

		///<summary>Creats an insurance plan with the default fee schedule of 53.</summary>
		public static InsPlan CreateInsPlanPPO(long carrierNum,long feeSchedNum){
			return CreateInsPlanPPO(carrierNum,feeSchedNum,EnumCobRule.Basic);
		}

		///<summary>Creates an insurance plan with the default fee schedule of 53.</summary>
		public static InsPlan CreateInsPlanPPO(long carrierNum,long feeSchedNum,EnumCobRule cobRule) {
			InsPlan plan=new InsPlan();
			plan.CarrierNum=carrierNum;
			plan.PlanType="p";
			plan.FeeSched=feeSchedNum;
			plan.CobRule=cobRule;
			InsPlans.Insert(plan);
			return plan;
		}

		///<summary>Creats a Medicaid/Flat Copay insurance plan.</summary>
		public static InsPlan CreateInsPlanMediFlatCopay(long carrierNum,long feeSchedNum,long copayFeeSchedNum = 0) {
			InsPlan plan=new InsPlan();
			plan.CarrierNum=carrierNum;
			plan.PlanType="f";
			plan.FeeSched=feeSchedNum;
			plan.CobRule=EnumCobRule.Standard;
			plan.CopayFeeSched=copayFeeSchedNum;
			InsPlans.Insert(plan);
			return plan;
		}

	}
}
