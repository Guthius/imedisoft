#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DashboardCellCrud
{
    public static DashboardCell SelectOne(long dashboardCellNum)
    {
        var command = "SELECT * FROM dashboardcell "
                      + "WHERE DashboardCellNum = " + SOut.Long(dashboardCellNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static DashboardCell SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<DashboardCell> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DashboardCell> TableToList(DataTable table)
    {
        var retVal = new List<DashboardCell>();
        DashboardCell dashboardCell;
        foreach (DataRow row in table.Rows)
        {
            dashboardCell = new DashboardCell();
            dashboardCell.DashboardCellNum = SIn.Long(row["DashboardCellNum"].ToString());
            dashboardCell.DashboardLayoutNum = SIn.Long(row["DashboardLayoutNum"].ToString());
            dashboardCell.CellRow = SIn.Int(row["CellRow"].ToString());
            dashboardCell.CellColumn = SIn.Int(row["CellColumn"].ToString());
            var cellType = row["CellType"].ToString();
            if (cellType == "")
                dashboardCell.CellType = 0;
            else
                try
                {
                    dashboardCell.CellType = (DashboardCellType) Enum.Parse(typeof(DashboardCellType), cellType);
                }
                catch
                {
                    dashboardCell.CellType = 0;
                }

            dashboardCell.CellSettings = SIn.String(row["CellSettings"].ToString());
            dashboardCell.LastQueryTime = SIn.DateTime(row["LastQueryTime"].ToString());
            dashboardCell.LastQueryData = SIn.String(row["LastQueryData"].ToString());
            dashboardCell.RefreshRateSeconds = SIn.Int(row["RefreshRateSeconds"].ToString());
            retVal.Add(dashboardCell);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<DashboardCell> listDashboardCells, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "DashboardCell";
        var table = new DataTable(tableName);
        table.Columns.Add("DashboardCellNum");
        table.Columns.Add("DashboardLayoutNum");
        table.Columns.Add("CellRow");
        table.Columns.Add("CellColumn");
        table.Columns.Add("CellType");
        table.Columns.Add("CellSettings");
        table.Columns.Add("LastQueryTime");
        table.Columns.Add("LastQueryData");
        table.Columns.Add("RefreshRateSeconds");
        foreach (var dashboardCell in listDashboardCells)
            table.Rows.Add(SOut.Long(dashboardCell.DashboardCellNum), SOut.Long(dashboardCell.DashboardLayoutNum), SOut.Int(dashboardCell.CellRow), SOut.Int(dashboardCell.CellColumn), SOut.Int((int) dashboardCell.CellType), dashboardCell.CellSettings, SOut.DateT(dashboardCell.LastQueryTime, false), dashboardCell.LastQueryData, SOut.Int(dashboardCell.RefreshRateSeconds));
        return table;
    }

    public static long Insert(DashboardCell dashboardCell)
    {
        return Insert(dashboardCell, false);
    }

    public static long Insert(DashboardCell dashboardCell, bool useExistingPK)
    {
        var command = "INSERT INTO dashboardcell (";

        command += "DashboardLayoutNum,CellRow,CellColumn,CellType,CellSettings,LastQueryTime,LastQueryData,RefreshRateSeconds) VALUES(";

        command +=
            SOut.Long(dashboardCell.DashboardLayoutNum) + ","
                                                        + SOut.Int(dashboardCell.CellRow) + ","
                                                        + SOut.Int(dashboardCell.CellColumn) + ","
                                                        + "'" + SOut.String(dashboardCell.CellType.ToString()) + "',"
                                                        + DbHelper.ParamChar + "paramCellSettings,"
                                                        + SOut.DateT(dashboardCell.LastQueryTime) + ","
                                                        + DbHelper.ParamChar + "paramLastQueryData,"
                                                        + SOut.Int(dashboardCell.RefreshRateSeconds) + ")";
        if (dashboardCell.CellSettings == null) dashboardCell.CellSettings = "";
        var paramCellSettings = new OdSqlParameter("paramCellSettings", OdDbType.Text, SOut.StringParam(dashboardCell.CellSettings));
        if (dashboardCell.LastQueryData == null) dashboardCell.LastQueryData = "";
        var paramLastQueryData = new OdSqlParameter("paramLastQueryData", OdDbType.Text, SOut.StringParam(dashboardCell.LastQueryData));
        {
            dashboardCell.DashboardCellNum = Db.NonQ(command, true, "DashboardCellNum", "dashboardCell", paramCellSettings, paramLastQueryData);
        }
        return dashboardCell.DashboardCellNum;
    }

    public static long InsertNoCache(DashboardCell dashboardCell)
    {
        return InsertNoCache(dashboardCell, false);
    }

    public static long InsertNoCache(DashboardCell dashboardCell, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO dashboardcell (";
        if (isRandomKeys || useExistingPK) command += "DashboardCellNum,";
        command += "DashboardLayoutNum,CellRow,CellColumn,CellType,CellSettings,LastQueryTime,LastQueryData,RefreshRateSeconds) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(dashboardCell.DashboardCellNum) + ",";
        command +=
            SOut.Long(dashboardCell.DashboardLayoutNum) + ","
                                                        + SOut.Int(dashboardCell.CellRow) + ","
                                                        + SOut.Int(dashboardCell.CellColumn) + ","
                                                        + "'" + SOut.String(dashboardCell.CellType.ToString()) + "',"
                                                        + DbHelper.ParamChar + "paramCellSettings,"
                                                        + SOut.DateT(dashboardCell.LastQueryTime) + ","
                                                        + DbHelper.ParamChar + "paramLastQueryData,"
                                                        + SOut.Int(dashboardCell.RefreshRateSeconds) + ")";
        if (dashboardCell.CellSettings == null) dashboardCell.CellSettings = "";
        var paramCellSettings = new OdSqlParameter("paramCellSettings", OdDbType.Text, SOut.StringParam(dashboardCell.CellSettings));
        if (dashboardCell.LastQueryData == null) dashboardCell.LastQueryData = "";
        var paramLastQueryData = new OdSqlParameter("paramLastQueryData", OdDbType.Text, SOut.StringParam(dashboardCell.LastQueryData));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramCellSettings, paramLastQueryData);
        else
            dashboardCell.DashboardCellNum = Db.NonQ(command, true, "DashboardCellNum", "dashboardCell", paramCellSettings, paramLastQueryData);
        return dashboardCell.DashboardCellNum;
    }

    public static void Update(DashboardCell dashboardCell)
    {
        var command = "UPDATE dashboardcell SET "
                      + "DashboardLayoutNum=  " + SOut.Long(dashboardCell.DashboardLayoutNum) + ", "
                      + "CellRow           =  " + SOut.Int(dashboardCell.CellRow) + ", "
                      + "CellColumn        =  " + SOut.Int(dashboardCell.CellColumn) + ", "
                      + "CellType          = '" + SOut.String(dashboardCell.CellType.ToString()) + "', "
                      + "CellSettings      =  " + DbHelper.ParamChar + "paramCellSettings, "
                      + "LastQueryTime     =  " + SOut.DateT(dashboardCell.LastQueryTime) + ", "
                      + "LastQueryData     =  " + DbHelper.ParamChar + "paramLastQueryData, "
                      + "RefreshRateSeconds=  " + SOut.Int(dashboardCell.RefreshRateSeconds) + " "
                      + "WHERE DashboardCellNum = " + SOut.Long(dashboardCell.DashboardCellNum);
        if (dashboardCell.CellSettings == null) dashboardCell.CellSettings = "";
        var paramCellSettings = new OdSqlParameter("paramCellSettings", OdDbType.Text, SOut.StringParam(dashboardCell.CellSettings));
        if (dashboardCell.LastQueryData == null) dashboardCell.LastQueryData = "";
        var paramLastQueryData = new OdSqlParameter("paramLastQueryData", OdDbType.Text, SOut.StringParam(dashboardCell.LastQueryData));
        Db.NonQ(command, paramCellSettings, paramLastQueryData);
    }

    public static bool Update(DashboardCell dashboardCell, DashboardCell oldDashboardCell)
    {
        var command = "";
        if (dashboardCell.DashboardLayoutNum != oldDashboardCell.DashboardLayoutNum)
        {
            if (command != "") command += ",";
            command += "DashboardLayoutNum = " + SOut.Long(dashboardCell.DashboardLayoutNum) + "";
        }

        if (dashboardCell.CellRow != oldDashboardCell.CellRow)
        {
            if (command != "") command += ",";
            command += "CellRow = " + SOut.Int(dashboardCell.CellRow) + "";
        }

        if (dashboardCell.CellColumn != oldDashboardCell.CellColumn)
        {
            if (command != "") command += ",";
            command += "CellColumn = " + SOut.Int(dashboardCell.CellColumn) + "";
        }

        if (dashboardCell.CellType != oldDashboardCell.CellType)
        {
            if (command != "") command += ",";
            command += "CellType = '" + SOut.String(dashboardCell.CellType.ToString()) + "'";
        }

        if (dashboardCell.CellSettings != oldDashboardCell.CellSettings)
        {
            if (command != "") command += ",";
            command += "CellSettings = " + DbHelper.ParamChar + "paramCellSettings";
        }

        if (dashboardCell.LastQueryTime != oldDashboardCell.LastQueryTime)
        {
            if (command != "") command += ",";
            command += "LastQueryTime = " + SOut.DateT(dashboardCell.LastQueryTime) + "";
        }

        if (dashboardCell.LastQueryData != oldDashboardCell.LastQueryData)
        {
            if (command != "") command += ",";
            command += "LastQueryData = " + DbHelper.ParamChar + "paramLastQueryData";
        }

        if (dashboardCell.RefreshRateSeconds != oldDashboardCell.RefreshRateSeconds)
        {
            if (command != "") command += ",";
            command += "RefreshRateSeconds = " + SOut.Int(dashboardCell.RefreshRateSeconds) + "";
        }

        if (command == "") return false;
        if (dashboardCell.CellSettings == null) dashboardCell.CellSettings = "";
        var paramCellSettings = new OdSqlParameter("paramCellSettings", OdDbType.Text, SOut.StringParam(dashboardCell.CellSettings));
        if (dashboardCell.LastQueryData == null) dashboardCell.LastQueryData = "";
        var paramLastQueryData = new OdSqlParameter("paramLastQueryData", OdDbType.Text, SOut.StringParam(dashboardCell.LastQueryData));
        command = "UPDATE dashboardcell SET " + command
                                              + " WHERE DashboardCellNum = " + SOut.Long(dashboardCell.DashboardCellNum);
        Db.NonQ(command, paramCellSettings, paramLastQueryData);
        return true;
    }

    public static bool UpdateComparison(DashboardCell dashboardCell, DashboardCell oldDashboardCell)
    {
        if (dashboardCell.DashboardLayoutNum != oldDashboardCell.DashboardLayoutNum) return true;
        if (dashboardCell.CellRow != oldDashboardCell.CellRow) return true;
        if (dashboardCell.CellColumn != oldDashboardCell.CellColumn) return true;
        if (dashboardCell.CellType != oldDashboardCell.CellType) return true;
        if (dashboardCell.CellSettings != oldDashboardCell.CellSettings) return true;
        if (dashboardCell.LastQueryTime != oldDashboardCell.LastQueryTime) return true;
        if (dashboardCell.LastQueryData != oldDashboardCell.LastQueryData) return true;
        if (dashboardCell.RefreshRateSeconds != oldDashboardCell.RefreshRateSeconds) return true;
        return false;
    }

    public static void Delete(long dashboardCellNum)
    {
        var command = "DELETE FROM dashboardcell "
                      + "WHERE DashboardCellNum = " + SOut.Long(dashboardCellNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDashboardCellNums)
    {
        if (listDashboardCellNums == null || listDashboardCellNums.Count == 0) return;
        var command = "DELETE FROM dashboardcell "
                      + "WHERE DashboardCellNum IN(" + string.Join(",", listDashboardCellNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}