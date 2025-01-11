using System.Data;
using DataConnectionBase;
using OpenDental.Graph.Base;

namespace OpenDental.Graph.Cache
{
    public class DashboardCachePaySplit : DashboardCacheWithQuery<PaySplit>
    {
        protected override string GetCommand(DashboardFilter filter)
        {
            var where = "";
            if (filter.UseDateFilter)
            {
                where = "DatePay BETWEEN " + SOut.Date(filter.DateFrom) + " AND " + SOut.Date(filter.DateTo) + " AND ";
            }

            if (filter.UseProvFilter)
            {
                where += "ProvNum=" + SOut.Long(filter.ProvNum) + " AND ";
            }

            return
                "SELECT ProvNum,DatePay,SUM(SplitAmt) AS GrossSplit,ClinicNum " +
                "FROM paysplit " +
                "WHERE " + where + "IsDiscount=0 " +
                "GROUP BY ProvNum,DatePay,ClinicNum ";
        }

        protected override PaySplit GetInstanceFromDataRow(DataRow x)
        {
            return new PaySplit
            {
                ProvNum = SIn.Long(x["ProvNum"].ToString()),
                DateStamp = SIn.DateTime(x["DatePay"].ToString()),
                Val = SIn.Double(x["GrossSplit"].ToString()),
                Count = 0,
                ClinicNum = SIn.Long(x["ClinicNum"].ToString()),
            };
        }
    }

    public class PaySplit : GraphQuantityOverTime.GraphDataPointClinic
    {
    }
}