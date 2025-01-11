#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TimeAdjustCrud
{
    public static TimeAdjust SelectOne(long timeAdjustNum)
    {
        var command = "SELECT * FROM timeadjust "
                      + "WHERE TimeAdjustNum = " + SOut.Long(timeAdjustNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static TimeAdjust SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TimeAdjust> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TimeAdjust> TableToList(DataTable table)
    {
        var retVal = new List<TimeAdjust>();
        TimeAdjust timeAdjust;
        foreach (DataRow row in table.Rows)
        {
            timeAdjust = new TimeAdjust();
            timeAdjust.TimeAdjustNum = SIn.Long(row["TimeAdjustNum"].ToString());
            timeAdjust.EmployeeNum = SIn.Long(row["EmployeeNum"].ToString());
            timeAdjust.TimeEntry = SIn.DateTime(row["TimeEntry"].ToString());
            timeAdjust.RegHours = SIn.TimeSpan(row["RegHours"].ToString());
            timeAdjust.OTimeHours = SIn.TimeSpan(row["OTimeHours"].ToString());
            timeAdjust.Note = SIn.String(row["Note"].ToString());
            timeAdjust.IsAuto = SIn.Bool(row["IsAuto"].ToString());
            timeAdjust.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            timeAdjust.PtoDefNum = SIn.Long(row["PtoDefNum"].ToString());
            timeAdjust.PtoHours = SIn.TimeSpan(row["PtoHours"].ToString());
            timeAdjust.IsUnpaidProtectedLeave = SIn.Bool(row["IsUnpaidProtectedLeave"].ToString());
            timeAdjust.SecuUserNumEntry = SIn.Long(row["SecuUserNumEntry"].ToString());
            retVal.Add(timeAdjust);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<TimeAdjust> listTimeAdjusts, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "TimeAdjust";
        var table = new DataTable(tableName);
        table.Columns.Add("TimeAdjustNum");
        table.Columns.Add("EmployeeNum");
        table.Columns.Add("TimeEntry");
        table.Columns.Add("RegHours");
        table.Columns.Add("OTimeHours");
        table.Columns.Add("Note");
        table.Columns.Add("IsAuto");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("PtoDefNum");
        table.Columns.Add("PtoHours");
        table.Columns.Add("IsUnpaidProtectedLeave");
        table.Columns.Add("SecuUserNumEntry");
        foreach (var timeAdjust in listTimeAdjusts)
            table.Rows.Add(SOut.Long(timeAdjust.TimeAdjustNum), SOut.Long(timeAdjust.EmployeeNum), SOut.DateT(timeAdjust.TimeEntry, false), SOut.Time(timeAdjust.RegHours, false), SOut.Time(timeAdjust.OTimeHours, false), timeAdjust.Note, SOut.Bool(timeAdjust.IsAuto), SOut.Long(timeAdjust.ClinicNum), SOut.Long(timeAdjust.PtoDefNum), SOut.Time(timeAdjust.PtoHours, false), SOut.Bool(timeAdjust.IsUnpaidProtectedLeave), SOut.Long(timeAdjust.SecuUserNumEntry));
        return table;
    }

    public static long Insert(TimeAdjust timeAdjust)
    {
        return Insert(timeAdjust, false);
    }

    public static long Insert(TimeAdjust timeAdjust, bool useExistingPK)
    {
        var command = "INSERT INTO timeadjust (";

        command += "EmployeeNum,TimeEntry,RegHours,OTimeHours,Note,IsAuto,ClinicNum,PtoDefNum,PtoHours,IsUnpaidProtectedLeave,SecuUserNumEntry) VALUES(";

        command +=
            SOut.Long(timeAdjust.EmployeeNum) + ","
                                              + SOut.DateT(timeAdjust.TimeEntry) + ","
                                              + "'" + SOut.TSpan(timeAdjust.RegHours) + "',"
                                              + "'" + SOut.TSpan(timeAdjust.OTimeHours) + "',"
                                              + DbHelper.ParamChar + "paramNote,"
                                              + SOut.Bool(timeAdjust.IsAuto) + ","
                                              + SOut.Long(timeAdjust.ClinicNum) + ","
                                              + SOut.Long(timeAdjust.PtoDefNum) + ","
                                              + "'" + SOut.TSpan(timeAdjust.PtoHours) + "',"
                                              + SOut.Bool(timeAdjust.IsUnpaidProtectedLeave) + ","
                                              + SOut.Long(timeAdjust.SecuUserNumEntry) + ")";
        if (timeAdjust.Note == null) timeAdjust.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(timeAdjust.Note));
        {
            timeAdjust.TimeAdjustNum = Db.NonQ(command, true, "TimeAdjustNum", "timeAdjust", paramNote);
        }
        return timeAdjust.TimeAdjustNum;
    }

    public static long InsertNoCache(TimeAdjust timeAdjust)
    {
        return InsertNoCache(timeAdjust, false);
    }

    public static long InsertNoCache(TimeAdjust timeAdjust, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO timeadjust (";
        if (isRandomKeys || useExistingPK) command += "TimeAdjustNum,";
        command += "EmployeeNum,TimeEntry,RegHours,OTimeHours,Note,IsAuto,ClinicNum,PtoDefNum,PtoHours,IsUnpaidProtectedLeave,SecuUserNumEntry) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(timeAdjust.TimeAdjustNum) + ",";
        command +=
            SOut.Long(timeAdjust.EmployeeNum) + ","
                                              + SOut.DateT(timeAdjust.TimeEntry) + ","
                                              + "'" + SOut.TSpan(timeAdjust.RegHours) + "',"
                                              + "'" + SOut.TSpan(timeAdjust.OTimeHours) + "',"
                                              + DbHelper.ParamChar + "paramNote,"
                                              + SOut.Bool(timeAdjust.IsAuto) + ","
                                              + SOut.Long(timeAdjust.ClinicNum) + ","
                                              + SOut.Long(timeAdjust.PtoDefNum) + ","
                                              + "'" + SOut.TSpan(timeAdjust.PtoHours) + "',"
                                              + SOut.Bool(timeAdjust.IsUnpaidProtectedLeave) + ","
                                              + SOut.Long(timeAdjust.SecuUserNumEntry) + ")";
        if (timeAdjust.Note == null) timeAdjust.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(timeAdjust.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNote);
        else
            timeAdjust.TimeAdjustNum = Db.NonQ(command, true, "TimeAdjustNum", "timeAdjust", paramNote);
        return timeAdjust.TimeAdjustNum;
    }

    public static void Update(TimeAdjust timeAdjust)
    {
        var command = "UPDATE timeadjust SET "
                      + "EmployeeNum           =  " + SOut.Long(timeAdjust.EmployeeNum) + ", "
                      + "TimeEntry             =  " + SOut.DateT(timeAdjust.TimeEntry) + ", "
                      + "RegHours              = '" + SOut.TSpan(timeAdjust.RegHours) + "', "
                      + "OTimeHours            = '" + SOut.TSpan(timeAdjust.OTimeHours) + "', "
                      + "Note                  =  " + DbHelper.ParamChar + "paramNote, "
                      + "IsAuto                =  " + SOut.Bool(timeAdjust.IsAuto) + ", "
                      + "ClinicNum             =  " + SOut.Long(timeAdjust.ClinicNum) + ", "
                      + "PtoDefNum             =  " + SOut.Long(timeAdjust.PtoDefNum) + ", "
                      + "PtoHours              = '" + SOut.TSpan(timeAdjust.PtoHours) + "', "
                      + "IsUnpaidProtectedLeave=  " + SOut.Bool(timeAdjust.IsUnpaidProtectedLeave) + ", "
                      + "SecuUserNumEntry      =  " + SOut.Long(timeAdjust.SecuUserNumEntry) + " "
                      + "WHERE TimeAdjustNum = " + SOut.Long(timeAdjust.TimeAdjustNum);
        if (timeAdjust.Note == null) timeAdjust.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(timeAdjust.Note));
        Db.NonQ(command, paramNote);
    }

    public static bool Update(TimeAdjust timeAdjust, TimeAdjust oldTimeAdjust)
    {
        var command = "";
        if (timeAdjust.EmployeeNum != oldTimeAdjust.EmployeeNum)
        {
            if (command != "") command += ",";
            command += "EmployeeNum = " + SOut.Long(timeAdjust.EmployeeNum) + "";
        }

        if (timeAdjust.TimeEntry != oldTimeAdjust.TimeEntry)
        {
            if (command != "") command += ",";
            command += "TimeEntry = " + SOut.DateT(timeAdjust.TimeEntry) + "";
        }

        if (timeAdjust.RegHours != oldTimeAdjust.RegHours)
        {
            if (command != "") command += ",";
            command += "RegHours = '" + SOut.TSpan(timeAdjust.RegHours) + "'";
        }

        if (timeAdjust.OTimeHours != oldTimeAdjust.OTimeHours)
        {
            if (command != "") command += ",";
            command += "OTimeHours = '" + SOut.TSpan(timeAdjust.OTimeHours) + "'";
        }

        if (timeAdjust.Note != oldTimeAdjust.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (timeAdjust.IsAuto != oldTimeAdjust.IsAuto)
        {
            if (command != "") command += ",";
            command += "IsAuto = " + SOut.Bool(timeAdjust.IsAuto) + "";
        }

        if (timeAdjust.ClinicNum != oldTimeAdjust.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(timeAdjust.ClinicNum) + "";
        }

        if (timeAdjust.PtoDefNum != oldTimeAdjust.PtoDefNum)
        {
            if (command != "") command += ",";
            command += "PtoDefNum = " + SOut.Long(timeAdjust.PtoDefNum) + "";
        }

        if (timeAdjust.PtoHours != oldTimeAdjust.PtoHours)
        {
            if (command != "") command += ",";
            command += "PtoHours = '" + SOut.TSpan(timeAdjust.PtoHours) + "'";
        }

        if (timeAdjust.IsUnpaidProtectedLeave != oldTimeAdjust.IsUnpaidProtectedLeave)
        {
            if (command != "") command += ",";
            command += "IsUnpaidProtectedLeave = " + SOut.Bool(timeAdjust.IsUnpaidProtectedLeave) + "";
        }

        if (timeAdjust.SecuUserNumEntry != oldTimeAdjust.SecuUserNumEntry)
        {
            if (command != "") command += ",";
            command += "SecuUserNumEntry = " + SOut.Long(timeAdjust.SecuUserNumEntry) + "";
        }

        if (command == "") return false;
        if (timeAdjust.Note == null) timeAdjust.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(timeAdjust.Note));
        command = "UPDATE timeadjust SET " + command
                                           + " WHERE TimeAdjustNum = " + SOut.Long(timeAdjust.TimeAdjustNum);
        Db.NonQ(command, paramNote);
        return true;
    }

    public static bool UpdateComparison(TimeAdjust timeAdjust, TimeAdjust oldTimeAdjust)
    {
        if (timeAdjust.EmployeeNum != oldTimeAdjust.EmployeeNum) return true;
        if (timeAdjust.TimeEntry != oldTimeAdjust.TimeEntry) return true;
        if (timeAdjust.RegHours != oldTimeAdjust.RegHours) return true;
        if (timeAdjust.OTimeHours != oldTimeAdjust.OTimeHours) return true;
        if (timeAdjust.Note != oldTimeAdjust.Note) return true;
        if (timeAdjust.IsAuto != oldTimeAdjust.IsAuto) return true;
        if (timeAdjust.ClinicNum != oldTimeAdjust.ClinicNum) return true;
        if (timeAdjust.PtoDefNum != oldTimeAdjust.PtoDefNum) return true;
        if (timeAdjust.PtoHours != oldTimeAdjust.PtoHours) return true;
        if (timeAdjust.IsUnpaidProtectedLeave != oldTimeAdjust.IsUnpaidProtectedLeave) return true;
        if (timeAdjust.SecuUserNumEntry != oldTimeAdjust.SecuUserNumEntry) return true;
        return false;
    }

    public static void Delete(long timeAdjustNum)
    {
        var command = "DELETE FROM timeadjust "
                      + "WHERE TimeAdjustNum = " + SOut.Long(timeAdjustNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTimeAdjustNums)
    {
        if (listTimeAdjustNums == null || listTimeAdjustNums.Count == 0) return;
        var command = "DELETE FROM timeadjust "
                      + "WHERE TimeAdjustNum IN(" + string.Join(",", listTimeAdjustNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}