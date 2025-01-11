using System.Data;
using DataConnectionBase;
using OpenDental.Graph.Base;
using OpenDentBusiness;

namespace OpenDental.Graph.Cache
{
    public class DashboardCacheNewPatient : DashboardCacheWithQuery<NewPatient>
    {
        protected override string GetCommand(DashboardFilter filter)
        {
            var where = "ProcStatus=" + SOut.Int((int) ProcStat.C);
            if (filter.UseProvFilter)
            {
                where += " AND ProvNum=" + SOut.Long(filter.ProvNum);
            }

            var cmd =
                "SELECT PatNum, MIN(ProcDate) FirstProc, ClinicNum, ProvNum "
                + "FROM procedurelog USE INDEX(indexPNPSCN) "
                + "INNER JOIN procedurecode ON procedurecode.CodeNum = procedurelog.CodeNum "
                + "AND procedurecode.ProcCode NOT IN ('D9986','D9987')"
                + "WHERE " + where + " "
                + "GROUP BY PatNum";
            return cmd;
        }

        protected override NewPatient GetInstanceFromDataRow(DataRow x)
        {
            return new NewPatient
            {
                DateStamp = SIn.DateTime(x["FirstProc"].ToString()),
                Count = 1, //Each row counts as 1.
                Val = 0, //there are no fees
                SeriesName = "All", //Only 1 series.
                ProvNum = SIn.Long(x["ProvNum"].ToString()),
                ClinicNum = SIn.Long(x["ClinicNum"].ToString()),
            };
        }

        protected override bool AllowQueryDateFilter()
        {
            return false;
        }
    }

    public class NewPatient : GraphQuantityOverTime.GraphDataPointClinic
    {
    }
}