using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class DashboardLayoutCrud
{
    public static List<DashboardLayout> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DashboardLayout> TableToList(DataTable table)
    {
        var retVal = new List<DashboardLayout>();
        foreach (DataRow row in table.Rows)
        {
            var dashboardLayout = new DashboardLayout
            {
                DashboardLayoutNum = SIn.Long(row["DashboardLayoutNum"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                UserGroupNum = SIn.Long(row["UserGroupNum"].ToString()),
                DashboardTabName = SIn.String(row["DashboardTabName"].ToString()),
                DashboardTabOrder = SIn.Int(row["DashboardTabOrder"].ToString()),
                DashboardRows = SIn.Int(row["DashboardRows"].ToString()),
                DashboardColumns = SIn.Int(row["DashboardColumns"].ToString()),
                DashboardGroupName = SIn.String(row["DashboardGroupName"].ToString())
            };
            retVal.Add(dashboardLayout);
        }

        return retVal;
    }

    public static long Insert(DashboardLayout dashboardLayout)
    {
        var command = "INSERT INTO dashboardlayout (";

        command += "UserNum,UserGroupNum,DashboardTabName,DashboardTabOrder,DashboardRows,DashboardColumns,DashboardGroupName) VALUES(";

        command +=
            SOut.Long(dashboardLayout.UserNum) + ","
                                               + SOut.Long(dashboardLayout.UserGroupNum) + ","
                                               + "'" + SOut.String(dashboardLayout.DashboardTabName) + "',"
                                               + SOut.Int(dashboardLayout.DashboardTabOrder) + ","
                                               + SOut.Int(dashboardLayout.DashboardRows) + ","
                                               + SOut.Int(dashboardLayout.DashboardColumns) + ","
                                               + "'" + SOut.String(dashboardLayout.DashboardGroupName) + "')";
        {
            dashboardLayout.DashboardLayoutNum = Db.NonQ(command, true, "DashboardLayoutNum", "dashboardLayout");
        }
        return dashboardLayout.DashboardLayoutNum;
    }

    public static void Delete(long dashboardLayoutNum)
    {
        var command = "DELETE FROM dashboardlayout "
                      + "WHERE DashboardLayoutNum = " + SOut.Long(dashboardLayoutNum);
        Db.NonQ(command);
    }
}