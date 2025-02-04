using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class DashboardARCrud
{
    public static List<DashboardAR> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DashboardAR> TableToList(DataTable table)
    {
        var retVal = new List<DashboardAR>();
        foreach (DataRow row in table.Rows)
        {
            var dashboardAR = new DashboardAR
            {
                DateCalc = SIn.Date(row["DateCalc"].ToString()),
                BalTotal = SIn.Double(row["BalTotal"].ToString()),
                InsEst = SIn.Double(row["InsEst"].ToString())
            };
            retVal.Add(dashboardAR);
        }

        return retVal;
    }

    public static void Insert(DashboardAR dashboardAR)
    {
        var command = "INSERT INTO dashboardar (";

        command += "DateCalc,BalTotal,InsEst) VALUES(";

        command +=
            SOut.Date(dashboardAR.DateCalc) + ","
                                            + SOut.Double(dashboardAR.BalTotal) + ","
                                            + SOut.Double(dashboardAR.InsEst) + ")";
        {
            Db.NonQ(command, true, "DashboardARNum", "dashboardAR");
        }
    }
}