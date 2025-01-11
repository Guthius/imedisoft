#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DashboardLayoutCrud
{
    public static DashboardLayout SelectOne(long dashboardLayoutNum)
    {
        var command = "SELECT * FROM dashboardlayout "
                      + "WHERE DashboardLayoutNum = " + SOut.Long(dashboardLayoutNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static DashboardLayout SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<DashboardLayout> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DashboardLayout> TableToList(DataTable table)
    {
        var retVal = new List<DashboardLayout>();
        DashboardLayout dashboardLayout;
        foreach (DataRow row in table.Rows)
        {
            dashboardLayout = new DashboardLayout();
            dashboardLayout.DashboardLayoutNum = SIn.Long(row["DashboardLayoutNum"].ToString());
            dashboardLayout.UserNum = SIn.Long(row["UserNum"].ToString());
            dashboardLayout.UserGroupNum = SIn.Long(row["UserGroupNum"].ToString());
            dashboardLayout.DashboardTabName = SIn.String(row["DashboardTabName"].ToString());
            dashboardLayout.DashboardTabOrder = SIn.Int(row["DashboardTabOrder"].ToString());
            dashboardLayout.DashboardRows = SIn.Int(row["DashboardRows"].ToString());
            dashboardLayout.DashboardColumns = SIn.Int(row["DashboardColumns"].ToString());
            dashboardLayout.DashboardGroupName = SIn.String(row["DashboardGroupName"].ToString());
            retVal.Add(dashboardLayout);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<DashboardLayout> listDashboardLayouts, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "DashboardLayout";
        var table = new DataTable(tableName);
        table.Columns.Add("DashboardLayoutNum");
        table.Columns.Add("UserNum");
        table.Columns.Add("UserGroupNum");
        table.Columns.Add("DashboardTabName");
        table.Columns.Add("DashboardTabOrder");
        table.Columns.Add("DashboardRows");
        table.Columns.Add("DashboardColumns");
        table.Columns.Add("DashboardGroupName");
        foreach (var dashboardLayout in listDashboardLayouts)
            table.Rows.Add(SOut.Long(dashboardLayout.DashboardLayoutNum), SOut.Long(dashboardLayout.UserNum), SOut.Long(dashboardLayout.UserGroupNum), dashboardLayout.DashboardTabName, SOut.Int(dashboardLayout.DashboardTabOrder), SOut.Int(dashboardLayout.DashboardRows), SOut.Int(dashboardLayout.DashboardColumns), dashboardLayout.DashboardGroupName);
        return table;
    }

    public static long Insert(DashboardLayout dashboardLayout)
    {
        return Insert(dashboardLayout, false);
    }

    public static long Insert(DashboardLayout dashboardLayout, bool useExistingPK)
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

    public static long InsertNoCache(DashboardLayout dashboardLayout)
    {
        return InsertNoCache(dashboardLayout, false);
    }

    public static long InsertNoCache(DashboardLayout dashboardLayout, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO dashboardlayout (";
        if (isRandomKeys || useExistingPK) command += "DashboardLayoutNum,";
        command += "UserNum,UserGroupNum,DashboardTabName,DashboardTabOrder,DashboardRows,DashboardColumns,DashboardGroupName) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(dashboardLayout.DashboardLayoutNum) + ",";
        command +=
            SOut.Long(dashboardLayout.UserNum) + ","
                                               + SOut.Long(dashboardLayout.UserGroupNum) + ","
                                               + "'" + SOut.String(dashboardLayout.DashboardTabName) + "',"
                                               + SOut.Int(dashboardLayout.DashboardTabOrder) + ","
                                               + SOut.Int(dashboardLayout.DashboardRows) + ","
                                               + SOut.Int(dashboardLayout.DashboardColumns) + ","
                                               + "'" + SOut.String(dashboardLayout.DashboardGroupName) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            dashboardLayout.DashboardLayoutNum = Db.NonQ(command, true, "DashboardLayoutNum", "dashboardLayout");
        return dashboardLayout.DashboardLayoutNum;
    }

    public static void Update(DashboardLayout dashboardLayout)
    {
        var command = "UPDATE dashboardlayout SET "
                      + "UserNum           =  " + SOut.Long(dashboardLayout.UserNum) + ", "
                      + "UserGroupNum      =  " + SOut.Long(dashboardLayout.UserGroupNum) + ", "
                      + "DashboardTabName  = '" + SOut.String(dashboardLayout.DashboardTabName) + "', "
                      + "DashboardTabOrder =  " + SOut.Int(dashboardLayout.DashboardTabOrder) + ", "
                      + "DashboardRows     =  " + SOut.Int(dashboardLayout.DashboardRows) + ", "
                      + "DashboardColumns  =  " + SOut.Int(dashboardLayout.DashboardColumns) + ", "
                      + "DashboardGroupName= '" + SOut.String(dashboardLayout.DashboardGroupName) + "' "
                      + "WHERE DashboardLayoutNum = " + SOut.Long(dashboardLayout.DashboardLayoutNum);
        Db.NonQ(command);
    }

    public static bool Update(DashboardLayout dashboardLayout, DashboardLayout oldDashboardLayout)
    {
        var command = "";
        if (dashboardLayout.UserNum != oldDashboardLayout.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(dashboardLayout.UserNum) + "";
        }

        if (dashboardLayout.UserGroupNum != oldDashboardLayout.UserGroupNum)
        {
            if (command != "") command += ",";
            command += "UserGroupNum = " + SOut.Long(dashboardLayout.UserGroupNum) + "";
        }

        if (dashboardLayout.DashboardTabName != oldDashboardLayout.DashboardTabName)
        {
            if (command != "") command += ",";
            command += "DashboardTabName = '" + SOut.String(dashboardLayout.DashboardTabName) + "'";
        }

        if (dashboardLayout.DashboardTabOrder != oldDashboardLayout.DashboardTabOrder)
        {
            if (command != "") command += ",";
            command += "DashboardTabOrder = " + SOut.Int(dashboardLayout.DashboardTabOrder) + "";
        }

        if (dashboardLayout.DashboardRows != oldDashboardLayout.DashboardRows)
        {
            if (command != "") command += ",";
            command += "DashboardRows = " + SOut.Int(dashboardLayout.DashboardRows) + "";
        }

        if (dashboardLayout.DashboardColumns != oldDashboardLayout.DashboardColumns)
        {
            if (command != "") command += ",";
            command += "DashboardColumns = " + SOut.Int(dashboardLayout.DashboardColumns) + "";
        }

        if (dashboardLayout.DashboardGroupName != oldDashboardLayout.DashboardGroupName)
        {
            if (command != "") command += ",";
            command += "DashboardGroupName = '" + SOut.String(dashboardLayout.DashboardGroupName) + "'";
        }

        if (command == "") return false;
        command = "UPDATE dashboardlayout SET " + command
                                                + " WHERE DashboardLayoutNum = " + SOut.Long(dashboardLayout.DashboardLayoutNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(DashboardLayout dashboardLayout, DashboardLayout oldDashboardLayout)
    {
        if (dashboardLayout.UserNum != oldDashboardLayout.UserNum) return true;
        if (dashboardLayout.UserGroupNum != oldDashboardLayout.UserGroupNum) return true;
        if (dashboardLayout.DashboardTabName != oldDashboardLayout.DashboardTabName) return true;
        if (dashboardLayout.DashboardTabOrder != oldDashboardLayout.DashboardTabOrder) return true;
        if (dashboardLayout.DashboardRows != oldDashboardLayout.DashboardRows) return true;
        if (dashboardLayout.DashboardColumns != oldDashboardLayout.DashboardColumns) return true;
        if (dashboardLayout.DashboardGroupName != oldDashboardLayout.DashboardGroupName) return true;
        return false;
    }

    public static void Delete(long dashboardLayoutNum)
    {
        var command = "DELETE FROM dashboardlayout "
                      + "WHERE DashboardLayoutNum = " + SOut.Long(dashboardLayoutNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDashboardLayoutNums)
    {
        if (listDashboardLayoutNums == null || listDashboardLayoutNums.Count == 0) return;
        var command = "DELETE FROM dashboardlayout "
                      + "WHERE DashboardLayoutNum IN(" + string.Join(",", listDashboardLayoutNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}