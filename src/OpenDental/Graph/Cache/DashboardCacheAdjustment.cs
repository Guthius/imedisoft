using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDental.Graph.Base;

namespace OpenDental.Graph.Cache
{
    public class DashboardCacheAdjustment : DashboardCacheWithQuery<Adjustment>
    {
        protected override string GetCommand(DashboardFilter filter)
        {
            var where = "";
            var listWhereClauses = new List<string>();
            if (filter.UseDateFilter)
            {
                listWhereClauses.Add("AdjDate BETWEEN " + SOut.Date(filter.DateFrom) + " AND " + SOut.Date(filter.DateTo) + " ");
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
                "SELECT AdjDate,ProvNum,SUM(AdjAmt) AdjTotal, ClinicNum "
                + "FROM adjustment "
                + where
                + "GROUP BY AdjDate,ProvNum,ClinicNum "
                + "HAVING AdjTotal<>0 "
                + "ORDER BY AdjDate,ProvNum ";
        }

        protected override Adjustment GetInstanceFromDataRow(DataRow x)
        {
            return new Adjustment
            {
                ProvNum = SIn.Long(x["ProvNum"].ToString()),
                DateStamp = SIn.DateTime(x["AdjDate"].ToString()),
                Val = SIn.Double(x["AdjTotal"].ToString()),
                Count = 0,
                ClinicNum = SIn.Long(x["ClinicNum"].ToString()),
            };
        }
    }

    public class Adjustment : GraphQuantityOverTime.GraphDataPointClinic
    {
    }
}