using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class TimeAdjusts
{
    public static List<TimeAdjust> Refresh(long employeeNum, DateTime dateFrom, DateTime dateTo)
    {
        var command =
            "SELECT * FROM timeadjust WHERE "
            + "EmployeeNum = " + employeeNum + " "
            + "AND DATE(TimeEntry) >= " + SOut.Date(dateFrom) + " "
            + "AND DATE(TimeEntry) <= " + SOut.Date(dateTo) + " "
            + "ORDER BY TimeEntry";
        return TimeAdjustCrud.SelectMany(command);
    }

    public static List<TimeAdjust> GetValidList(long employeeNum, DateTime dateFrom, DateTime dateTo)
    {
        var listTimeAdjusts = new List<TimeAdjust>();
        var command =
            "SELECT * FROM timeadjust WHERE "
            + "EmployeeNum = " + employeeNum + " "
            + "AND DATE(TimeEntry) >= " + SOut.Date(dateFrom) + " "
            + "AND DATE(TimeEntry) <= " + SOut.Date(dateTo) + " "
            + "ORDER BY TimeEntry";
        listTimeAdjusts = TimeAdjustCrud.SelectMany(command);
        //Validate---------------------------------------------------------------------------------------------------------------
        //none necessary at this time.
        return listTimeAdjusts;
    }

    public static List<TimeAdjust> GetListForTimeCardManage(List<long> listEmployeeNums, long clinicNum, DateTime dateFrom, DateTime dateTo, bool isAll)
    {
        if (listEmployeeNums.IsNullOrEmpty()) return [];

        var command = "SELECT * FROM timeadjust WHERE "
                      + "EmployeeNum IN (" + string.Join(",", listEmployeeNums.Select(x => x)) + ") "
                      + "AND DATE(TimeEntry) >= " + SOut.Date(dateFrom) + " "
                      + "AND DATE(TimeEntry) <= " + SOut.Date(dateTo) + " ";
        if (!isAll) command += "AND ClinicNum = " + clinicNum + " ";
        command += "ORDER BY TimeEntry";
        return TimeAdjustCrud.SelectMany(command);
    }

    public static List<TimeAdjust> GetAllForPeriod(DateTime dateFrom, DateTime dateTo)
    {
        var command = "SELECT * FROM timeadjust "
                      + "WHERE TimeEntry >= " + SOut.Date(dateFrom) + " "
                      + "AND TimeEntry < " + SOut.Date(dateTo.AddDays(1)) + " ";
        return TimeAdjustCrud.SelectMany(command);
    }

    public static void Insert(TimeAdjust timeAdjust)
    {
        TimeAdjustCrud.Insert(timeAdjust);
    }

    public static void Update(TimeAdjust timeAdjust)
    {
        TimeAdjustCrud.Update(timeAdjust);
    }

    public static void Update(TimeAdjust timeAdjust, TimeAdjust timeAdjustOld)
    {
        TimeAdjustCrud.Update(timeAdjust, timeAdjustOld);
    }

    public static void Delete(TimeAdjust timeAdjust)
    {
        var command = "DELETE FROM timeadjust WHERE TimeAdjustNum = " + timeAdjust.TimeAdjustNum;
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTimeAdjustNums)
    {
        if (listTimeAdjustNums.IsNullOrEmpty()) return;

        var command = "DELETE FROM timeadjust WHERE TimeAdjustNum IN(" + string.Join(",", listTimeAdjustNums.Select(x => x)) + ")";
        Db.NonQ(command);
    }

    public static List<TimeAdjust> GetSimpleListAuto(long employeeNum, DateTime dateStart, DateTime dateStop)
    {
        var listTimeAdjusts = new List<TimeAdjust>();
        //List<TimeAdjust> listTimeAdjusts=new List<TimeAdjust>();
        var command =
            "SELECT * FROM timeadjust WHERE "
            + "EmployeeNum = " + employeeNum + " "
            + "AND " + "DATE(TimeEntry)" + " >= " + SOut.Date(dateStart) + " "
            + "AND " + "DATE(TimeEntry)" + " < " + SOut.Date(dateStop.AddDays(1)) + " " //add one day to go the end of the specified date.
            + "AND IsAuto=1";
        //listTimeAdjusts=Crud.TimeAdjustCrud.SelectMany(command);
        return TimeAdjustCrud.SelectMany(command);
    }

    public static TimeAdjust GetPayPeriodNote(long employeeNum, DateTime dateStart)
    {
        var command = "SELECT * FROM timeadjust WHERE EmployeeNum=" + employeeNum + " AND TimeEntry=" + SOut.DateTime(dateStart) + " AND IsAuto=0 ";
        command += "AND RegHours='00:00:00' AND OTimeHours='00:00:00' AND PtoHours='00:00:00' ";
        return TimeAdjustCrud.SelectOne(command);
    }

    public static List<TimeAdjust> GetNotesForPayPeriod(DateTime dateStart)
    {
        var command = "SELECT * FROM timeadjust WHERE TimeEntry=" + SOut.DateTime(dateStart) + " AND isAuto=0 ";
        command += "AND RegHours='00:00:00' AND OTimeHours='00:00:00' AND PtoHours='00:00:00' ";
        return TimeAdjustCrud.SelectMany(command);
    }
}