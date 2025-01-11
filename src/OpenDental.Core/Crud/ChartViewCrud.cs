using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ChartViewCrud
{
    public static List<ChartView> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ChartView> TableToList(DataTable table)
    {
        var retVal = new List<ChartView>();
        ChartView chartView;
        foreach (DataRow row in table.Rows)
        {
            chartView = new ChartView();
            chartView.ChartViewNum = SIn.Long(row["ChartViewNum"].ToString());
            chartView.Description = SIn.String(row["Description"].ToString());
            chartView.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            chartView.ProcStatuses = (ChartViewProcStat) SIn.Int(row["ProcStatuses"].ToString());
            chartView.ObjectTypes = (ChartViewObjs) SIn.Int(row["ObjectTypes"].ToString());
            chartView.ShowProcNotes = SIn.Bool(row["ShowProcNotes"].ToString());
            chartView.IsAudit = SIn.Bool(row["IsAudit"].ToString());
            chartView.SelectedTeethOnly = SIn.Bool(row["SelectedTeethOnly"].ToString());
            chartView.OrionStatusFlags = (OrionStatus) SIn.Int(row["OrionStatusFlags"].ToString());
            chartView.DatesShowing = (ChartViewDates) SIn.Int(row["DatesShowing"].ToString());
            chartView.IsTpCharting = SIn.Bool(row["IsTpCharting"].ToString());
            retVal.Add(chartView);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ChartView> listChartViews, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ChartView";
        var table = new DataTable(tableName);
        table.Columns.Add("ChartViewNum");
        table.Columns.Add("Description");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("ProcStatuses");
        table.Columns.Add("ObjectTypes");
        table.Columns.Add("ShowProcNotes");
        table.Columns.Add("IsAudit");
        table.Columns.Add("SelectedTeethOnly");
        table.Columns.Add("OrionStatusFlags");
        table.Columns.Add("DatesShowing");
        table.Columns.Add("IsTpCharting");
        foreach (var chartView in listChartViews)
            table.Rows.Add(SOut.Long(chartView.ChartViewNum), chartView.Description, SOut.Int(chartView.ItemOrder), SOut.Int((int) chartView.ProcStatuses), SOut.Int((int) chartView.ObjectTypes), SOut.Bool(chartView.ShowProcNotes), SOut.Bool(chartView.IsAudit), SOut.Bool(chartView.SelectedTeethOnly), SOut.Int((int) chartView.OrionStatusFlags), SOut.Int((int) chartView.DatesShowing), SOut.Bool(chartView.IsTpCharting));
        return table;
    }

    public static long Insert(ChartView chartView)
    {
        var command = "INSERT INTO chartview (";

        command += "Description,ItemOrder,ProcStatuses,ObjectTypes,ShowProcNotes,IsAudit,SelectedTeethOnly,OrionStatusFlags,DatesShowing,IsTpCharting) VALUES(";

        command +=
            "'" + SOut.String(chartView.Description) + "',"
            + SOut.Int(chartView.ItemOrder) + ","
            + SOut.Int((int) chartView.ProcStatuses) + ","
            + SOut.Int((int) chartView.ObjectTypes) + ","
            + SOut.Bool(chartView.ShowProcNotes) + ","
            + SOut.Bool(chartView.IsAudit) + ","
            + SOut.Bool(chartView.SelectedTeethOnly) + ","
            + SOut.Int((int) chartView.OrionStatusFlags) + ","
            + SOut.Int((int) chartView.DatesShowing) + ","
            + SOut.Bool(chartView.IsTpCharting) + ")";
        {
            chartView.ChartViewNum = Db.NonQ(command, true, "ChartViewNum", "chartView");
        }
        return chartView.ChartViewNum;
    }

    public static void Update(ChartView chartView)
    {
        var command = "UPDATE chartview SET "
                      + "Description      = '" + SOut.String(chartView.Description) + "', "
                      + "ItemOrder        =  " + SOut.Int(chartView.ItemOrder) + ", "
                      + "ProcStatuses     =  " + SOut.Int((int) chartView.ProcStatuses) + ", "
                      + "ObjectTypes      =  " + SOut.Int((int) chartView.ObjectTypes) + ", "
                      + "ShowProcNotes    =  " + SOut.Bool(chartView.ShowProcNotes) + ", "
                      + "IsAudit          =  " + SOut.Bool(chartView.IsAudit) + ", "
                      + "SelectedTeethOnly=  " + SOut.Bool(chartView.SelectedTeethOnly) + ", "
                      + "OrionStatusFlags =  " + SOut.Int((int) chartView.OrionStatusFlags) + ", "
                      + "DatesShowing     =  " + SOut.Int((int) chartView.DatesShowing) + ", "
                      + "IsTpCharting     =  " + SOut.Bool(chartView.IsTpCharting) + " "
                      + "WHERE ChartViewNum = " + SOut.Long(chartView.ChartViewNum);
        Db.NonQ(command);
    }

    public static bool Update(ChartView chartView, ChartView oldChartView)
    {
        var command = "";
        if (chartView.Description != oldChartView.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(chartView.Description) + "'";
        }

        if (chartView.ItemOrder != oldChartView.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(chartView.ItemOrder) + "";
        }

        if (chartView.ProcStatuses != oldChartView.ProcStatuses)
        {
            if (command != "") command += ",";
            command += "ProcStatuses = " + SOut.Int((int) chartView.ProcStatuses) + "";
        }

        if (chartView.ObjectTypes != oldChartView.ObjectTypes)
        {
            if (command != "") command += ",";
            command += "ObjectTypes = " + SOut.Int((int) chartView.ObjectTypes) + "";
        }

        if (chartView.ShowProcNotes != oldChartView.ShowProcNotes)
        {
            if (command != "") command += ",";
            command += "ShowProcNotes = " + SOut.Bool(chartView.ShowProcNotes) + "";
        }

        if (chartView.IsAudit != oldChartView.IsAudit)
        {
            if (command != "") command += ",";
            command += "IsAudit = " + SOut.Bool(chartView.IsAudit) + "";
        }

        if (chartView.SelectedTeethOnly != oldChartView.SelectedTeethOnly)
        {
            if (command != "") command += ",";
            command += "SelectedTeethOnly = " + SOut.Bool(chartView.SelectedTeethOnly) + "";
        }

        if (chartView.OrionStatusFlags != oldChartView.OrionStatusFlags)
        {
            if (command != "") command += ",";
            command += "OrionStatusFlags = " + SOut.Int((int) chartView.OrionStatusFlags) + "";
        }

        if (chartView.DatesShowing != oldChartView.DatesShowing)
        {
            if (command != "") command += ",";
            command += "DatesShowing = " + SOut.Int((int) chartView.DatesShowing) + "";
        }

        if (chartView.IsTpCharting != oldChartView.IsTpCharting)
        {
            if (command != "") command += ",";
            command += "IsTpCharting = " + SOut.Bool(chartView.IsTpCharting) + "";
        }

        if (command == "") return false;
        command = "UPDATE chartview SET " + command
                                          + " WHERE ChartViewNum = " + SOut.Long(chartView.ChartViewNum);
        Db.NonQ(command);
        return true;
    }
}