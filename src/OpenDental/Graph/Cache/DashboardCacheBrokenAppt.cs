using System.Data;
using DataConnectionBase;
using OpenDental.Graph.Base;
using OpenDentBusiness;

namespace OpenDental.Graph.Cache
{
    public class DashboardCacheBrokenAppt : DashboardCacheWithQuery<BrokenAppt>
    {
        protected override string GetCommand(DashboardFilter filter)
        {
            var where = "WHERE AptStatus=" + (int) ApptStatus.Broken + " ";
            if (filter.UseDateFilter)
            {
                where += "AND DATE(AptDateTime) BETWEEN " + SOut.Date(filter.DateFrom) + " AND " + SOut.Date(filter.DateTo) + " ";
            }

            if (filter.UseProvFilter)
            {
                where += "AND ProvNum=" + SOut.Long(filter.ProvNum) + " ";
            }

            return
                "SELECT DATE(AptDateTime) ApptDate,ProvNum,ClinicNum,COUNT(AptNum) ApptCount "
                + "FROM appointment "
                + where
                + "GROUP BY ApptDate,ProvNum,ClinicNum ";
        }

        protected override BrokenAppt GetInstanceFromDataRow(DataRow x)
        {
            return new BrokenAppt
            {
                ProvNum = SIn.Long(x["ProvNum"].ToString()),
                ClinicNum = SIn.Long(x["ClinicNum"].ToString()),
                DateStamp = SIn.DateTime(x["ApptDate"].ToString()),
                Count = SIn.Long(x["ApptCount"].ToString()),
                Val = 0
            };
        }
    }

    public class BrokenAppt : GraphQuantityOverTime.GraphDataPointClinic
    {
    }
}