using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class TimeAdjusts
{
    
    public static List<TimeAdjust> Refresh(long employeeNum, DateTime dateFrom, DateTime dateTo)
    {
        var command =
            "SELECT * FROM timeadjust WHERE "
            + "EmployeeNum = " + SOut.Long(employeeNum) + " "
            + "AND " + DbHelper.DtimeToDate("TimeEntry") + " >= " + SOut.Date(dateFrom) + " "
            + "AND " + DbHelper.DtimeToDate("TimeEntry") + " <= " + SOut.Date(dateTo) + " "
            + "ORDER BY TimeEntry";
        return TimeAdjustCrud.SelectMany(command);
    }

    /// <summary>
    ///     Validates and throws exceptions. Gets all time adjusts between date range and time adjusts made during the
    ///     current work week.
    /// </summary>
    public static List<TimeAdjust> GetValidList(long employeeNum, DateTime dateFrom, DateTime dateTo)
    {
        var listTimeAdjusts = new List<TimeAdjust>();
        var command =
            "SELECT * FROM timeadjust WHERE "
            + "EmployeeNum = " + SOut.Long(employeeNum) + " "
            + "AND " + DbHelper.DtimeToDate("TimeEntry") + " >= " + SOut.Date(dateFrom) + " "
            + "AND " + DbHelper.DtimeToDate("TimeEntry") + " <= " + SOut.Date(dateTo) + " "
            + "ORDER BY TimeEntry";
        listTimeAdjusts = TimeAdjustCrud.SelectMany(command);
        //Validate---------------------------------------------------------------------------------------------------------------
        //none necessary at this time.
        return listTimeAdjusts;
    }

    
    public static List<TimeAdjust> GetListForTimeCardManage(long employeeNum, long clinicNum, DateTime dateFrom, DateTime dateTo, bool isAll)
    {
        return GetListForTimeCardManage(new List<long> {employeeNum}, clinicNum, dateFrom, dateTo, isAll);
    }

    
    public static List<TimeAdjust> GetListForTimeCardManage(List<long> listEmployeeNums, long clinicNum, DateTime dateFrom, DateTime dateTo, bool isAll)
    {
        if (listEmployeeNums.IsNullOrEmpty()) return new List<TimeAdjust>();

        var command = "SELECT * FROM timeadjust WHERE "
                      + "EmployeeNum IN (" + string.Join(",", listEmployeeNums.Select(x => SOut.Long(x))) + ") "
                      + "AND " + DbHelper.DtimeToDate("TimeEntry") + " >= " + SOut.Date(dateFrom) + " "
                      + "AND " + DbHelper.DtimeToDate("TimeEntry") + " <= " + SOut.Date(dateTo) + " ";
        if (!isAll) command += "AND ClinicNum = " + SOut.Long(clinicNum) + " ";
        command += "ORDER BY TimeEntry";
        return TimeAdjustCrud.SelectMany(command);
    }

    ///<summary>Dates are INCLUSIVE.</summary>
    public static List<TimeAdjust> GetAllForPeriod(DateTime dateFrom, DateTime dateTo)
    {
        var command = "SELECT * FROM timeadjust "
                      + "WHERE TimeEntry >= " + SOut.Date(dateFrom) + " "
                      + "AND TimeEntry < " + SOut.Date(dateTo.AddDays(1)) + " ";
        return TimeAdjustCrud.SelectMany(command);
    }

    
    public static long Insert(TimeAdjust timeAdjust)
    {
        return TimeAdjustCrud.Insert(timeAdjust);
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
        var command = "DELETE FROM timeadjust WHERE TimeAdjustNum = " + SOut.Long(timeAdjust.TimeAdjustNum);
        Db.NonQ(command);
    }

    
    public static void DeleteMany(List<long> listTimeAdjustNums)
    {
        if (listTimeAdjustNums.IsNullOrEmpty()) return;

        var command = "DELETE FROM timeadjust WHERE TimeAdjustNum IN(" + string.Join(",", listTimeAdjustNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    ///<summary>Returns all automatically generated timeAdjusts for a given employee between the date range (inclusive).</summary>
    public static List<TimeAdjust> GetSimpleListAuto(long employeeNum, DateTime dateStart, DateTime dateStop)
    {
        var listTimeAdjusts = new List<TimeAdjust>();
        //List<TimeAdjust> listTimeAdjusts=new List<TimeAdjust>();
        var command =
            "SELECT * FROM timeadjust WHERE "
            + "EmployeeNum = " + SOut.Long(employeeNum) + " "
            + "AND " + DbHelper.DtimeToDate("TimeEntry") + " >= " + SOut.Date(dateStart) + " "
            + "AND " + DbHelper.DtimeToDate("TimeEntry") + " < " + SOut.Date(dateStop.AddDays(1)) + " " //add one day to go the end of the specified date.
            + "AND IsAuto=1";
        //listTimeAdjusts=Crud.TimeAdjustCrud.SelectMany(command);
        return TimeAdjustCrud.SelectMany(command);
    }

    #region Get Methods

    ///<summary>Attempts to get one TimeAdjust based on a time.  Returns null if not found. </summary>
    public static TimeAdjust GetPayPeriodNote(long employeeNum, DateTime dateStart)
    {
        var command = "SELECT * FROM timeadjust WHERE EmployeeNum=" + SOut.Long(employeeNum) + " AND TimeEntry=" + SOut.DateT(dateStart) + " AND IsAuto=0 ";
        command += "AND RegHours='00:00:00' AND OTimeHours='00:00:00' AND PtoHours='00:00:00' ";
        return TimeAdjustCrud.SelectOne(command);
    }

    /// <summary>
    ///     Gets a list of payperiod notes.  Start Date should be the start date of the pay period trying to get notes
    ///     for.
    /// </summary>
    public static List<TimeAdjust> GetNotesForPayPeriod(DateTime dateStart)
    {
        var command = "SELECT * FROM timeadjust WHERE TimeEntry=" + SOut.DateT(dateStart) + " AND isAuto=0 ";
        command += "AND RegHours='00:00:00' AND OTimeHours='00:00:00' AND PtoHours='00:00:00' ";
        return TimeAdjustCrud.SelectMany(command);
    }

    #endregion
}