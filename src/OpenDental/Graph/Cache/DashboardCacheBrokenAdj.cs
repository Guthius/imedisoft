using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDental.Graph.Base;

namespace OpenDental.Graph.Cache
{
    public class DashboardCacheBrokenAdj : DashboardCacheWithQuery<BrokenAdj>
    {
        protected override string GetCommand(DashboardFilter filter)
        {
            var where = "";
            var listWhereClauses = new List<string>();
            if (filter.UseDateFilter)
            {
                listWhereClauses.Add("DATE(AdjDate) BETWEEN " + SOut.Date(filter.DateFrom) + " AND " + SOut.Date(filter.DateTo) + " ");
            }

            if (filter.UseProvFilter)
            {
                listWhereClauses.Add("ProvNum=" + SOut.Long(filter.ProvNum) + " ");
            }

            if (listWhereClauses.Count > 0)
            {
                where = "WHERE " + string.Join("AND ", listWhereClauses);
            }

            return
                "SELECT AdjDate,ProvNum,COUNT(AdjNum) AdjCount,ClinicNum,AdjType, SUM(AdjAmt) AdjAmt "
                + "FROM adjustment "
                + "INNER JOIN definition ON definition.DefNum=adjustment.AdjType "
                + "AND definition.ItemValue = '+' "
                + where
                + "GROUP BY AdjDate,ProvNum,ClinicNum,AdjType "
                + "ORDER BY AdjDate,ProvNum,ClinicNum ";
        }

        protected override BrokenAdj GetInstanceFromDataRow(DataRow x)
        {
            return new BrokenAdj
            {
                ProvNum = SIn.Long(x["ProvNum"].ToString()),
                ClinicNum = SIn.Long(x["ClinicNum"].ToString()),
                DateStamp = SIn.Date(x["AdjDate"].ToString()),
                Val = SIn.Double(x["AdjAmt"].ToString()),
                AdjType = SIn.Long(x["AdjType"].ToString()),
                Count = SIn.Long(x["AdjCount"].ToString()),
            };
        }
    }

    public class BrokenAdj : GraphQuantityOverTime.GraphDataPointClinic
    {
        public long AdjType;
    }
}