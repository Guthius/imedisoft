using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class DashboardARs
{
    public static List<DashboardAR> Refresh(DateTime dateFrom)
    {
        var command = "SELECT * FROM dashboardar WHERE DateCalc >= " + SOut.Date(dateFrom);
        return ReportsComplex.RunFuncOnReportServer(() => DashboardARCrud.SelectMany(command));
    }

    public static void Insert(DashboardAR dashboardAR)
    {
        DashboardARCrud.Insert(dashboardAR);
    }

    public static void Truncate()
    {
        Db.NonQ("TRUNCATE dashboardar");
    }
}