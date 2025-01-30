using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class DashboardQueries
{
    public static List<DashboardAR> GetAR(DateTime dateFrom, DateTime dateTo, List<DashboardAR> dashboardArs)
    {
        var results = new List<DashboardAR>();

        var months = 0;
        while (dateTo >= dateFrom.AddMonths(months))
        {
            months++;
        }

        for (var i = 0; i < months; i++)
        {
            var dateLastOfMonth = dateFrom.AddMonths(i + 1).AddDays(-1);
            
            DashboardAR dash = null;
            
            foreach (var dashboardAr in dashboardArs)
            {
                if (dashboardAr.DateCalc != dateLastOfMonth)
                {
                    continue;
                }

                dash = dashboardAr;
            }

            if (dash != null)
            {
                results.Add(dash);
                
                continue;
            }
            
            var commandText = "SELECT SUM(Bal_0_30+Bal_31_60+Bal_61_90+BalOver90),SUM(InsEst) FROM (" + Ledgers.GetAgingQueryString(dateLastOfMonth, isHistoric: true) + ") guarBals";
            var dataTable = DataCore.GetTable(commandText);

            dash = new DashboardAR
            {
                DateCalc = dateLastOfMonth,
                BalTotal = SIn.Double(dataTable.Rows[0][0].ToString()),
                InsEst = SIn.Double(dataTable.Rows[0][1].ToString())
            };
            
            DashboardARs.Insert(dash);
            
            results.Add(dash);
        }
            
        return results;
    }
        
    public static DataTable GetTable(string commandText)
    {
        return DataCore.GetTable(commandText);
    }
}