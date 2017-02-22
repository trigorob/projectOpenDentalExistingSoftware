using System;
using System.Collections.Generic;
using OpenDentBusiness;

namespace OpenDentalGraph.Cache {
	public class DashboardCacheAR:DashboardCacheBase<DashboardAR> {
		protected override List<DashboardAR> GetCache(DashboardFilter filter) {
			filter.DateTo=new DateTime(filter.DateTo.Year,filter.DateTo.Month,1);
			return DashboardQueries.GetAR(filter.DateFrom,filter.DateTo,DashboardARs.Refresh(filter.DateFrom));
		}

		protected override bool AllowQueryDateFilter() {
			return false;
		}
	}
}