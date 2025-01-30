using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class DashboardCellCrud
{
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

    public static void Insert(DashboardCell dashboardCell)
    {
        var command = "INSERT INTO dashboardcell (";

        command += "DashboardLayoutNum,CellRow,CellColumn,CellType,CellSettings,LastQueryTime,LastQueryData,RefreshRateSeconds) VALUES(";

        command +=
            SOut.Long(dashboardCell.DashboardLayoutNum) + ","
                                                        + SOut.Int(dashboardCell.CellRow) + ","
                                                        + SOut.Int(dashboardCell.CellColumn) + ","
                                                        + "'" + SOut.String(dashboardCell.CellType.ToString()) + "',"
                                                        + DbHelper.ParamChar + "paramCellSettings,"
                                                        + SOut.DateTime(dashboardCell.LastQueryTime) + ","
                                                        + DbHelper.ParamChar + "paramLastQueryData,"
                                                        + SOut.Int(dashboardCell.RefreshRateSeconds) + ")";
        if (dashboardCell.CellSettings == null) dashboardCell.CellSettings = "";
        var paramCellSettings = new OdSqlParameter("paramCellSettings", SOut.StringParam(dashboardCell.CellSettings));
        if (dashboardCell.LastQueryData == null) dashboardCell.LastQueryData = "";
        var paramLastQueryData = new OdSqlParameter("paramLastQueryData", SOut.StringParam(dashboardCell.LastQueryData));
        {
            dashboardCell.DashboardCellNum = Db.NonQ(command, true, "DashboardCellNum", "dashboardCell", paramCellSettings, paramLastQueryData);
        }
    }

    public static void Delete(long dashboardCellNum)
    {
        var command = "DELETE FROM dashboardcell "
                      + "WHERE DashboardCellNum = " + SOut.Long(dashboardCellNum);
        Db.NonQ(command);
    }
}