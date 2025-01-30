using System;
using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class DashboardARs
{
    public static List<DashboardAR> Refresh(DateTime dateFrom)
    {
        return DashboardARCrud.SelectMany("SELECT * FROM dashboardar WHERE DateCalc >= " + SOut.Date(dateFrom));
    }

    public static void Insert(DashboardAR dashboardAr)
    {
        DashboardARCrud.Insert(dashboardAr);
    }

    public static void Truncate()
    {
        Db.NonQ("TRUNCATE dashboardar");
    }
}