using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ChildRoomLogs
{
    ///<summary>Get all the logs for a specified ChildRoom and filtered by the given date.</summary>
    public static List<ChildRoomLog> GetChildRoomLogs(long childRoomNum, DateTime date)
    {
        var command = "SELECT * FROM childroomlog WHERE ChildRoomNum=" + SOut.Long(childRoomNum)
                                                                       + " AND DATE(DateTDisplayed)=" + SOut.Date(date)
                                                                       + " ORDER BY DateTDisplayed";
        return ChildRoomLogCrud.SelectMany(command);
    }

    /// <summary>Get all the logs for a specified date.</summary>
    public static List<ChildRoomLog> GetChildRoomLogsForDate(DateTime date)
    {
        var command = "SELECT * FROM childroomlog WHERE DATE(DateTDisplayed)=" + SOut.Date(date)
                                                                               + " ORDER BY DateTDisplayed";
        return ChildRoomLogCrud.SelectMany(command);
    }

    ///<summary>Returns all child logs for a given date.</summary>
    public static List<ChildRoomLog> GetAllChildrenForDate(DateTime date)
    {
        var command = "SELECT * FROM childroomlog WHERE DATE(DateTDisplayed)=" + SOut.Date(date) //Specify date
                                                                               + " AND ChildNum!=0"; //Only child logs
        return ChildRoomLogCrud.SelectMany(command);
    }

    ///<summary>Returns all the employee logs for a given date.</summary>
    public static List<ChildRoomLog> GetAllEmployeesForDate(DateTime date)
    {
        var command = "SELECT * FROM childroomlog WHERE DATE(DateTDisplayed)=" + SOut.Date(date)
                                                                               + " AND EmployeeNum!=0"; //Only employee logs
        return ChildRoomLogCrud.SelectMany(command);
    }

    ///<summary>Returns all logs for a specific child for a given date.</summary>
    public static List<ChildRoomLog> GetAllLogsForChild(long childNum, DateTime date)
    {
        var command = "SELECT * FROM childroomlog WHERE ChildNum=" + SOut.Long(childNum)
                                                                   + " AND DATE(DateTDisplayed)=" + SOut.Date(date);
        return ChildRoomLogCrud.SelectMany(command);
    }

    ///<summary>Returns all logs for a specific employee for a given date.</summary>
    public static List<ChildRoomLog> GetAllLogsForEmployee(long employeeNum, DateTime date)
    {
        var command = "SELECT * FROM childroomlog WHERE EmployeeNum=" + SOut.Long(employeeNum)
                                                                      + " AND DATE(DateTDisplayed)=" + SOut.Date(date);
        return ChildRoomLogCrud.SelectMany(command);
    }

    /// <summary>
    ///     Get the most recent previous ratio change for a specific child room and date. Returns 0 if no previous ratio
    ///     change was found.
    /// </summary>
    public static double GetPreviousRatio(long childRoomNum, DateTime date)
    {
        var command = "SELECT * FROM childroomlog WHERE ChildRoomNum=" + SOut.Long(childRoomNum) //Specify room
                                                                       + " AND RatioChange!=0" //Specify RatioChange entry
                                                                       + " AND DateTDisplayed <" + SOut.Date(date) //Find previous entries
                                                                       + " ORDER BY DateTDisplayed DESC" //Order so that the first entry is the most recent one
                                                                       + " LIMIT 1";
        var childRoomLog = ChildRoomLogCrud.SelectOne(command);
        if (childRoomLog == null) return 0;
        return childRoomLog.RatioChange;
    }

    
    public static long Insert(ChildRoomLog childRoomLog)
    {
        return ChildRoomLogCrud.Insert(childRoomLog);
    }

    
    public static void Update(ChildRoomLog childRoomLog)
    {
        ChildRoomLogCrud.Update(childRoomLog);
    }

    
    public static void Delete(long childRoomLogNum)
    {
        ChildRoomLogCrud.Delete(childRoomLogNum);
    }

    /// <summary>
    ///     For mixed age groups. Oregon law has specific numbers of teachers required for a classroom that has children
    ///     over and under two years old. These requirements are outlined here
    ///     https://www.oregon.gov/delc/providers/CCLD_Library/CCLD-0084-Rules-for-Certified-Child-Care-Centers-EN.pdf in the
    ///     table on page 46. This method takes two paremeters, the total number of children and the number of children under
    ///     the age of two. Returns the number of teachers required based on the table.
    /// </summary>
    public static int GetNumberTeachersMixed(int totalChildren, int childrenUnderTwo)
    {
        if (totalChildren < 1) return 0; //No teachers required if there are no children present
        if (totalChildren > 16)
            //If for some reason the max of 16 is exceeded, return -1 to indicate something is wrong
            return -1;
        var teachersRequired = 0;
        if (childrenUnderTwo == 0)
        {
            if (totalChildren < 11)
                teachersRequired = 1;
            else
                teachersRequired = 2;
        }
        else if (childrenUnderTwo == 1)
        {
            if (totalChildren < 9)
                teachersRequired = 1;
            else
                teachersRequired = 2;
        }
        else if (childrenUnderTwo == 2)
        {
            if (totalChildren < 8)
                teachersRequired = 1;
            else
                teachersRequired = 2;
        }
        else if (childrenUnderTwo == 3)
        {
            if (totalChildren < 7)
                teachersRequired = 1;
            else
                teachersRequired = 2;
        }
        else if (childrenUnderTwo == 4)
        {
            if (totalChildren < 15)
                teachersRequired = 2;
            else
                teachersRequired = 3;
        }
        else if (childrenUnderTwo == 5)
        {
            if (totalChildren < 13)
                teachersRequired = 2;
            else
                teachersRequired = 3;
        }
        else if (childrenUnderTwo == 6)
        {
            if (totalChildren < 12)
                teachersRequired = 2;
            else
                teachersRequired = 3;
        }
        else if (childrenUnderTwo == 7)
        {
            if (totalChildren < 11)
                teachersRequired = 2;
            else
                teachersRequired = 3;
        }
        else if (childrenUnderTwo == 8)
        {
            if (totalChildren < 9)
                teachersRequired = 2;
            else
                teachersRequired = 3;
        }
        else if (childrenUnderTwo == 9)
        {
            teachersRequired = 3;
        }
        else if (childrenUnderTwo == 10)
        {
            if (totalChildren < 16)
                teachersRequired = 3;
            else
                teachersRequired = 4;
        }
        else if (childrenUnderTwo == 11)
        {
            if (totalChildren < 15)
                teachersRequired = 3;
            else
                teachersRequired = 4;
        }
        else if (childrenUnderTwo == 12)
        {
            if (totalChildren < 13)
                teachersRequired = 3;
            else
                teachersRequired = 4;
        }
        else
        {
            teachersRequired = 4; //13 or higher childrenUnderTwo requires 4 teachers
        }

        return teachersRequired;
    }
}