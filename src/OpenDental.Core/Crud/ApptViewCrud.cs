using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ApptViewCrud
{
    public static List<ApptView> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ApptView> TableToList(DataTable table)
    {
        var retVal = new List<ApptView>();
        ApptView apptView;
        foreach (DataRow row in table.Rows)
        {
            apptView = new ApptView();
            apptView.ApptViewNum = SIn.Long(row["ApptViewNum"].ToString());
            apptView.Description = SIn.String(row["Description"].ToString());
            apptView.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            apptView.RowsPerIncr = SIn.Byte(row["RowsPerIncr"].ToString());
            apptView.OnlyScheduledProvs = SIn.Bool(row["OnlyScheduledProvs"].ToString());
            apptView.OnlySchedBeforeTime = SIn.TimeSpan(row["OnlySchedBeforeTime"].ToString());
            apptView.OnlySchedAfterTime = SIn.TimeSpan(row["OnlySchedAfterTime"].ToString());
            apptView.StackBehavUR = (ApptViewStackBehavior) SIn.Int(row["StackBehavUR"].ToString());
            apptView.StackBehavLR = (ApptViewStackBehavior) SIn.Int(row["StackBehavLR"].ToString());
            apptView.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            apptView.ApptTimeScrollStart = SIn.TimeSpan(row["ApptTimeScrollStart"].ToString());
            apptView.IsScrollStartDynamic = SIn.Bool(row["IsScrollStartDynamic"].ToString());
            apptView.IsApptBubblesDisabled = SIn.Bool(row["IsApptBubblesDisabled"].ToString());
            apptView.WidthOpMinimum = SIn.Int(row["WidthOpMinimum"].ToString());
            apptView.WaitingRmName = (EnumWaitingRmName) SIn.Int(row["WaitingRmName"].ToString());
            apptView.OnlyScheduledProvDays = SIn.Bool(row["OnlyScheduledProvDays"].ToString());
            retVal.Add(apptView);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ApptView> listApptViews, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ApptView";
        var table = new DataTable(tableName);
        table.Columns.Add("ApptViewNum");
        table.Columns.Add("Description");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("RowsPerIncr");
        table.Columns.Add("OnlyScheduledProvs");
        table.Columns.Add("OnlySchedBeforeTime");
        table.Columns.Add("OnlySchedAfterTime");
        table.Columns.Add("StackBehavUR");
        table.Columns.Add("StackBehavLR");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("ApptTimeScrollStart");
        table.Columns.Add("IsScrollStartDynamic");
        table.Columns.Add("IsApptBubblesDisabled");
        table.Columns.Add("WidthOpMinimum");
        table.Columns.Add("WaitingRmName");
        table.Columns.Add("OnlyScheduledProvDays");
        foreach (var apptView in listApptViews)
            table.Rows.Add(SOut.Long(apptView.ApptViewNum), apptView.Description, SOut.Int(apptView.ItemOrder), SOut.Byte(apptView.RowsPerIncr), SOut.Bool(apptView.OnlyScheduledProvs), SOut.Time(apptView.OnlySchedBeforeTime, false), SOut.Time(apptView.OnlySchedAfterTime, false), SOut.Int((int) apptView.StackBehavUR), SOut.Int((int) apptView.StackBehavLR), SOut.Long(apptView.ClinicNum), SOut.Time(apptView.ApptTimeScrollStart, false), SOut.Bool(apptView.IsScrollStartDynamic), SOut.Bool(apptView.IsApptBubblesDisabled), SOut.Int(apptView.WidthOpMinimum), SOut.Int((int) apptView.WaitingRmName), SOut.Bool(apptView.OnlyScheduledProvDays));
        return table;
    }

    public static long Insert(ApptView apptView)
    {
        var command = "INSERT INTO apptview (";

        command += "Description,ItemOrder,RowsPerIncr,OnlyScheduledProvs,OnlySchedBeforeTime,OnlySchedAfterTime,StackBehavUR,StackBehavLR,ClinicNum,ApptTimeScrollStart,IsScrollStartDynamic,IsApptBubblesDisabled,WidthOpMinimum,WaitingRmName,OnlyScheduledProvDays) VALUES(";

        command +=
            "'" + SOut.String(apptView.Description) + "',"
            + SOut.Int(apptView.ItemOrder) + ","
            + SOut.Byte(apptView.RowsPerIncr) + ","
            + SOut.Bool(apptView.OnlyScheduledProvs) + ","
            + SOut.Time(apptView.OnlySchedBeforeTime) + ","
            + SOut.Time(apptView.OnlySchedAfterTime) + ","
            + SOut.Int((int) apptView.StackBehavUR) + ","
            + SOut.Int((int) apptView.StackBehavLR) + ","
            + SOut.Long(apptView.ClinicNum) + ","
            + SOut.Time(apptView.ApptTimeScrollStart) + ","
            + SOut.Bool(apptView.IsScrollStartDynamic) + ","
            + SOut.Bool(apptView.IsApptBubblesDisabled) + ","
            + SOut.Int(apptView.WidthOpMinimum) + ","
            + SOut.Int((int) apptView.WaitingRmName) + ","
            + SOut.Bool(apptView.OnlyScheduledProvDays) + ")";
        {
            apptView.ApptViewNum = Db.NonQ(command, true, "ApptViewNum", "apptView");
        }
        return apptView.ApptViewNum;
    }

    public static void Update(ApptView apptView)
    {
        var command = "UPDATE apptview SET "
                      + "Description          = '" + SOut.String(apptView.Description) + "', "
                      + "ItemOrder            =  " + SOut.Int(apptView.ItemOrder) + ", "
                      + "RowsPerIncr          =  " + SOut.Byte(apptView.RowsPerIncr) + ", "
                      + "OnlyScheduledProvs   =  " + SOut.Bool(apptView.OnlyScheduledProvs) + ", "
                      + "OnlySchedBeforeTime  =  " + SOut.Time(apptView.OnlySchedBeforeTime) + ", "
                      + "OnlySchedAfterTime   =  " + SOut.Time(apptView.OnlySchedAfterTime) + ", "
                      + "StackBehavUR         =  " + SOut.Int((int) apptView.StackBehavUR) + ", "
                      + "StackBehavLR         =  " + SOut.Int((int) apptView.StackBehavLR) + ", "
                      + "ClinicNum            =  " + SOut.Long(apptView.ClinicNum) + ", "
                      + "ApptTimeScrollStart  =  " + SOut.Time(apptView.ApptTimeScrollStart) + ", "
                      + "IsScrollStartDynamic =  " + SOut.Bool(apptView.IsScrollStartDynamic) + ", "
                      + "IsApptBubblesDisabled=  " + SOut.Bool(apptView.IsApptBubblesDisabled) + ", "
                      + "WidthOpMinimum       =  " + SOut.Int(apptView.WidthOpMinimum) + ", "
                      + "WaitingRmName        =  " + SOut.Int((int) apptView.WaitingRmName) + ", "
                      + "OnlyScheduledProvDays=  " + SOut.Bool(apptView.OnlyScheduledProvDays) + " "
                      + "WHERE ApptViewNum = " + SOut.Long(apptView.ApptViewNum);
        Db.NonQ(command);
    }
}