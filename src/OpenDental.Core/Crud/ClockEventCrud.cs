using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ClockEventCrud
{
    public static ClockEvent SelectOne(long clockEventNum)
    {
        var command = "SELECT * FROM clockevent "
                      + "WHERE ClockEventNum = " + SOut.Long(clockEventNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ClockEvent SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ClockEvent> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ClockEvent> TableToList(DataTable table)
    {
        var retVal = new List<ClockEvent>();
        ClockEvent clockEvent;
        foreach (DataRow row in table.Rows)
        {
            clockEvent = new ClockEvent();
            clockEvent.ClockEventNum = SIn.Long(row["ClockEventNum"].ToString());
            clockEvent.EmployeeNum = SIn.Long(row["EmployeeNum"].ToString());
            clockEvent.TimeEntered1 = SIn.DateTime(row["TimeEntered1"].ToString());
            clockEvent.TimeDisplayed1 = SIn.DateTime(row["TimeDisplayed1"].ToString());
            clockEvent.ClockStatus = (TimeClockStatus) SIn.Int(row["ClockStatus"].ToString());
            clockEvent.Note = SIn.String(row["Note"].ToString());
            clockEvent.TimeEntered2 = SIn.DateTime(row["TimeEntered2"].ToString());
            clockEvent.TimeDisplayed2 = SIn.DateTime(row["TimeDisplayed2"].ToString());
            clockEvent.OTimeHours = SIn.TimeSpan(row["OTimeHours"].ToString());
            clockEvent.OTimeAuto = SIn.TimeSpan(row["OTimeAuto"].ToString());
            clockEvent.Adjust = SIn.TimeSpan(row["Adjust"].ToString());
            clockEvent.AdjustAuto = SIn.TimeSpan(row["AdjustAuto"].ToString());
            clockEvent.AdjustIsOverridden = SIn.Bool(row["AdjustIsOverridden"].ToString());
            clockEvent.Rate2Hours = SIn.TimeSpan(row["Rate2Hours"].ToString());
            clockEvent.Rate2Auto = SIn.TimeSpan(row["Rate2Auto"].ToString());
            clockEvent.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            clockEvent.Rate3Hours = SIn.TimeSpan(row["Rate3Hours"].ToString());
            clockEvent.Rate3Auto = SIn.TimeSpan(row["Rate3Auto"].ToString());
            clockEvent.IsWorkingHome = SIn.Bool(row["IsWorkingHome"].ToString());
            retVal.Add(clockEvent);
        }

        return retVal;
    }

    public static long Insert(ClockEvent clockEvent)
    {
        var command = "INSERT INTO clockevent (";

        command += "EmployeeNum,TimeEntered1,TimeDisplayed1,ClockStatus,Note,TimeEntered2,TimeDisplayed2,OTimeHours,OTimeAuto,Adjust,AdjustAuto,AdjustIsOverridden,Rate2Hours,Rate2Auto,ClinicNum,Rate3Hours,Rate3Auto,IsWorkingHome) VALUES(";

        command +=
            SOut.Long(clockEvent.EmployeeNum) + ","
                                              + DbHelper.Now() + ","
                                              + DbHelper.Now() + ","
                                              + SOut.Int((int) clockEvent.ClockStatus) + ","
                                              + DbHelper.ParamChar + "paramNote,"
                                              + SOut.DateT(clockEvent.TimeEntered2) + ","
                                              + SOut.DateT(clockEvent.TimeDisplayed2) + ","
                                              + "'" + SOut.TSpan(clockEvent.OTimeHours) + "',"
                                              + "'" + SOut.TSpan(clockEvent.OTimeAuto) + "',"
                                              + "'" + SOut.TSpan(clockEvent.Adjust) + "',"
                                              + "'" + SOut.TSpan(clockEvent.AdjustAuto) + "',"
                                              + SOut.Bool(clockEvent.AdjustIsOverridden) + ","
                                              + "'" + SOut.TSpan(clockEvent.Rate2Hours) + "',"
                                              + "'" + SOut.TSpan(clockEvent.Rate2Auto) + "',"
                                              + SOut.Long(clockEvent.ClinicNum) + ","
                                              + "'" + SOut.TSpan(clockEvent.Rate3Hours) + "',"
                                              + "'" + SOut.TSpan(clockEvent.Rate3Auto) + "',"
                                              + SOut.Bool(clockEvent.IsWorkingHome) + ")";
        if (clockEvent.Note == null) clockEvent.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(clockEvent.Note));
        {
            clockEvent.ClockEventNum = Db.NonQ(command, true, "ClockEventNum", "clockEvent", paramNote);
        }
        return clockEvent.ClockEventNum;
    }

    public static void Update(ClockEvent clockEvent)
    {
        var command = "UPDATE clockevent SET "
                      + "EmployeeNum       =  " + SOut.Long(clockEvent.EmployeeNum) + ", "
                      //TimeEntered1 not allowed to change
                      + "TimeDisplayed1    =  " + SOut.DateT(clockEvent.TimeDisplayed1) + ", "
                      + "ClockStatus       =  " + SOut.Int((int) clockEvent.ClockStatus) + ", "
                      + "Note              =  " + DbHelper.ParamChar + "paramNote, "
                      + "TimeEntered2      =  " + SOut.DateT(clockEvent.TimeEntered2) + ", "
                      + "TimeDisplayed2    =  " + SOut.DateT(clockEvent.TimeDisplayed2) + ", "
                      + "OTimeHours        = '" + SOut.TSpan(clockEvent.OTimeHours) + "', "
                      + "OTimeAuto         = '" + SOut.TSpan(clockEvent.OTimeAuto) + "', "
                      + "Adjust            = '" + SOut.TSpan(clockEvent.Adjust) + "', "
                      + "AdjustAuto        = '" + SOut.TSpan(clockEvent.AdjustAuto) + "', "
                      + "AdjustIsOverridden=  " + SOut.Bool(clockEvent.AdjustIsOverridden) + ", "
                      + "Rate2Hours        = '" + SOut.TSpan(clockEvent.Rate2Hours) + "', "
                      + "Rate2Auto         = '" + SOut.TSpan(clockEvent.Rate2Auto) + "', "
                      + "ClinicNum         =  " + SOut.Long(clockEvent.ClinicNum) + ", "
                      + "Rate3Hours        = '" + SOut.TSpan(clockEvent.Rate3Hours) + "', "
                      + "Rate3Auto         = '" + SOut.TSpan(clockEvent.Rate3Auto) + "', "
                      + "IsWorkingHome     =  " + SOut.Bool(clockEvent.IsWorkingHome) + " "
                      + "WHERE ClockEventNum = " + SOut.Long(clockEvent.ClockEventNum);
        if (clockEvent.Note == null) clockEvent.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(clockEvent.Note));
        Db.NonQ(command, paramNote);
    }
}