using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class TimeAdjustCrud
{
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
        foreach (DataRow row in table.Rows)
        {
            var timeAdjust = new TimeAdjust
            {
                TimeAdjustNum = SIn.Long(row["TimeAdjustNum"].ToString()),
                EmployeeNum = SIn.Long(row["EmployeeNum"].ToString()),
                TimeEntry = SIn.DateTime(row["TimeEntry"].ToString()),
                RegHours = SIn.TimeSpan(row["RegHours"].ToString()),
                OTimeHours = SIn.TimeSpan(row["OTimeHours"].ToString()),
                Note = SIn.String(row["Note"].ToString()),
                IsAuto = SIn.Bool(row["IsAuto"].ToString()),
                ClinicNum = SIn.Long(row["ClinicNum"].ToString()),
                PtoDefNum = SIn.Long(row["PtoDefNum"].ToString()),
                PtoHours = SIn.TimeSpan(row["PtoHours"].ToString()),
                IsUnpaidProtectedLeave = SIn.Bool(row["IsUnpaidProtectedLeave"].ToString()),
                SecuUserNumEntry = SIn.Long(row["SecuUserNumEntry"].ToString())
            };
            retVal.Add(timeAdjust);
        }

        return retVal;
    }

    public static void Insert(TimeAdjust timeAdjust)
    {
        var command = "INSERT INTO timeadjust (";

        command += "EmployeeNum,TimeEntry,RegHours,OTimeHours,Note,IsAuto,ClinicNum,PtoDefNum,PtoHours,IsUnpaidProtectedLeave,SecuUserNumEntry) VALUES(";

        command +=
            SOut.Long(timeAdjust.EmployeeNum) + ","
                                              + SOut.DateTime(timeAdjust.TimeEntry) + ","
                                              + "'" + SOut.TimeSpan(timeAdjust.RegHours) + "',"
                                              + "'" + SOut.TimeSpan(timeAdjust.OTimeHours) + "',"
                                              + DbHelper.ParamChar + "paramNote,"
                                              + SOut.Bool(timeAdjust.IsAuto) + ","
                                              + SOut.Long(timeAdjust.ClinicNum) + ","
                                              + SOut.Long(timeAdjust.PtoDefNum) + ","
                                              + "'" + SOut.TimeSpan(timeAdjust.PtoHours) + "',"
                                              + SOut.Bool(timeAdjust.IsUnpaidProtectedLeave) + ","
                                              + SOut.Long(timeAdjust.SecuUserNumEntry) + ")";
        if (timeAdjust.Note == null) timeAdjust.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(timeAdjust.Note));
        {
            timeAdjust.TimeAdjustNum = Db.NonQ(command, true, "TimeAdjustNum", "timeAdjust", paramNote);
        }
    }

    public static void Update(TimeAdjust timeAdjust)
    {
        var command = "UPDATE timeadjust SET "
                      + "EmployeeNum           =  " + SOut.Long(timeAdjust.EmployeeNum) + ", "
                      + "TimeEntry             =  " + SOut.DateTime(timeAdjust.TimeEntry) + ", "
                      + "RegHours              = '" + SOut.TimeSpan(timeAdjust.RegHours) + "', "
                      + "OTimeHours            = '" + SOut.TimeSpan(timeAdjust.OTimeHours) + "', "
                      + "Note                  =  " + DbHelper.ParamChar + "paramNote, "
                      + "IsAuto                =  " + SOut.Bool(timeAdjust.IsAuto) + ", "
                      + "ClinicNum             =  " + SOut.Long(timeAdjust.ClinicNum) + ", "
                      + "PtoDefNum             =  " + SOut.Long(timeAdjust.PtoDefNum) + ", "
                      + "PtoHours              = '" + SOut.TimeSpan(timeAdjust.PtoHours) + "', "
                      + "IsUnpaidProtectedLeave=  " + SOut.Bool(timeAdjust.IsUnpaidProtectedLeave) + ", "
                      + "SecuUserNumEntry      =  " + SOut.Long(timeAdjust.SecuUserNumEntry) + " "
                      + "WHERE TimeAdjustNum = " + SOut.Long(timeAdjust.TimeAdjustNum);
        if (timeAdjust.Note == null) timeAdjust.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(timeAdjust.Note));
        Db.NonQ(command, paramNote);
    }

    public static void Update(TimeAdjust timeAdjust, TimeAdjust oldTimeAdjust)
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
            command += "TimeEntry = " + SOut.DateTime(timeAdjust.TimeEntry) + "";
        }

        if (timeAdjust.RegHours != oldTimeAdjust.RegHours)
        {
            if (command != "") command += ",";
            command += "RegHours = '" + SOut.TimeSpan(timeAdjust.RegHours) + "'";
        }

        if (timeAdjust.OTimeHours != oldTimeAdjust.OTimeHours)
        {
            if (command != "") command += ",";
            command += "OTimeHours = '" + SOut.TimeSpan(timeAdjust.OTimeHours) + "'";
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
            command += "PtoHours = '" + SOut.TimeSpan(timeAdjust.PtoHours) + "'";
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

        if (command == "") return;
        if (timeAdjust.Note == null) timeAdjust.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(timeAdjust.Note));
        command = "UPDATE timeadjust SET " + command
                                           + " WHERE TimeAdjustNum = " + SOut.Long(timeAdjust.TimeAdjustNum);
        Db.NonQ(command, paramNote);
    }
}