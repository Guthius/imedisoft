using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EhrQuarterlyKeys
{
    ///<summary>Pass in a guarantor of 0 when not using from OD tech station.</summary>
    public static List<EhrQuarterlyKey> Refresh(long guarantor)
    {
        string command;
        if (guarantor == 0) //customer looking at their own quarterly keys
            command = "SELECT * FROM ehrquarterlykey WHERE PatNum=0";
        else //
            command = "SELECT ehrquarterlykey.* FROM ehrquarterlykey,patient "
                      + "WHERE ehrquarterlykey.PatNum=patient.PatNum "
                      + "AND patient.Guarantor=" + SOut.Long(guarantor) + " "
                      + "GROUP BY ehrquarterlykey.EhrQuarterlyKeyNum "
                      + "ORDER BY ehrquarterlykey.YearValue,ehrquarterlykey.QuarterValue";
        return EhrQuarterlyKeyCrud.SelectMany(command);
    }

    public static EhrQuarterlyKey GetKeyThisQuarter()
    {
        string command;
        var quarter = MonthToQuarter(DateTime.Today.Month);
        command = "SELECT * FROM ehrquarterlykey WHERE YearValue=" + (DateTime.Today.Year - 2000) + " "
                  + "AND QuarterValue=" + quarter + " " //we don't care about practice title in the query
                  + "AND PatNum=0";
        return EhrQuarterlyKeyCrud.SelectOne(command);
    }

    
    public static List<EhrQuarterlyKey> GetAllKeys()
    {
        string command;
        command = "SELECT * FROM ehrquarterlykey WHERE PatNum=0 ORDER BY YearValue,QuarterValue";
        var ehrKeys = EhrQuarterlyKeyCrud.SelectMany(command);
        return ehrKeys;
    }

    ///<summary>Returns all keys in the given years.</summary>
    public static List<EhrQuarterlyKey> GetAllKeys(DateTime startDate, DateTime endDate)
    {
        var startYear = startDate.Year - 2000;
        var endYear = endDate.Year - 2000;
        string command;
        command = "SELECT * FROM ehrquarterlykey WHERE (YearValue BETWEEN " + startYear + " " + "AND " + endYear + ") "
                  + "AND PatNum=0 ORDER BY YearValue,QuarterValue";
        var ehrKeys = EhrQuarterlyKeyCrud.SelectMany(command);
        return ehrKeys;
    }

    /// <summary>
    ///     We want to find the first day of the oldest quarter less than or equal to one year old from the latest entered
    ///     valid key.  validKeys must be sorted ascending.
    /// </summary>
    public static DateTime FindStartDate(List<EhrQuarterlyKey> validKeys)
    {
        if (validKeys.Count < 1) return new DateTime(DateTime.Today.Year, 1, 1);
        var ehrKey = validKeys[validKeys.Count - 1];
        var latestReportDate = GetLastDayOfQuarter(ehrKey);
        var earliestStartDate = latestReportDate.AddYears(-1).AddDays(1);
        for (var i = validKeys.Count - 1; i > -1; i--)
        {
            ehrKey = validKeys[i];
            if (i == 0) break;
            var expectedPrevQuarter = validKeys[i].QuarterValue - 1;
            if (validKeys[i].QuarterValue == 1) expectedPrevQuarter = 4;
            var prevQuarter = validKeys[i - 1].QuarterValue;
            var expectedYear = validKeys[i].YearValue;
            if (validKeys[i].QuarterValue == 1) expectedYear = validKeys[i].YearValue - 1;
            var prevQuarter_Year = validKeys[i - 1].YearValue;
            if (expectedPrevQuarter != prevQuarter || expectedYear != prevQuarter_Year) //There is an quarterly key gap, so we ignore any older quarterly keys.
                break;
            var prevQuarterFirstDay = GetFirstDayOfQuarter(validKeys[i - 1]);
            if (prevQuarterFirstDay < earliestStartDate) break;
        }

        return GetFirstDayOfQuarter(ehrKey);
    }

    public static DateTime GetFirstDayOfQuarter(DateTime date)
    {
        return new DateTime(date.Year, 3 * (MonthToQuarter(date.Month) - 1) + 1, 1);
    }

    public static DateTime GetFirstDayOfQuarter(EhrQuarterlyKey ehrKey)
    {
        return GetFirstDayOfQuarter(new DateTime(ehrKey.YearValue + 2000, ehrKey.QuarterValue * 3, 1));
    }

    public static DateTime GetLastDayOfQuarter(DateTime date)
    {
        return new DateTime(date.Year, 1, 1).AddMonths(3 * MonthToQuarter(date.Month)).AddDays(-1);
    }

    public static DateTime GetLastDayOfQuarter(EhrQuarterlyKey ehrKey)
    {
        return GetLastDayOfQuarter(new DateTime(ehrKey.YearValue + 2000, ehrKey.QuarterValue * 3, 1));
    }

    public static string ValidateDateRange(List<EhrQuarterlyKey> validKeys, DateTime startDate, DateTime endDate)
    {
        if (validKeys.Count == 0) return "No Valid Quarterly Keys";
        var startOfFirstQuarter = GetFirstDayOfQuarter(startDate);
        var endOfLastQuarter = GetLastDayOfQuarter(endDate);
        var explanation = "";
        var msgCount = 0;
        var numberOfQuarters = CalculateQuarters(startDate, endDate);
        for (var j = 0; j < numberOfQuarters; j++)
        {
            var isValid = false;
            if (startOfFirstQuarter > endOfLastQuarter) break;
            for (var i = 0; i < validKeys.Count; i++)
                if (MonthToQuarter(startOfFirstQuarter.Month) == validKeys[i].QuarterValue && startOfFirstQuarter.Year - 2000 == validKeys[i].YearValue)
                {
                    isValid = true;
                    break;
                }

            if (!isValid)
            {
                if (explanation == "") explanation = "Selected date range is invalid. You are missing these quarterly keys: \r\n";
                explanation += startOfFirstQuarter.Year + "-Q" + MonthToQuarter(startOfFirstQuarter.Month) + "\r\n";
                msgCount++;
                if (msgCount > 8) return explanation;
            }

            startOfFirstQuarter = startOfFirstQuarter.AddMonths(3);
        }

        return explanation;
    }

    ///<summary>Gets the count of quarters between the dates inclusive.</summary>
    private static int CalculateQuarters(DateTime startDate, DateTime endDate)
    {
        var startQuarter = MonthToQuarter(startDate.Month);
        var endQuarter = MonthToQuarter(endDate.Month);
        return 4 * (endDate.Year - startDate.Year) + (endQuarter - startQuarter) + 1;
    }

    public static int MonthToQuarter(int month)
    {
        var quarter = 1;
        if (month >= 4 && month <= 6) quarter = 2;
        if (month >= 7 && month <= 9) quarter = 3;
        if (month >= 10) quarter = 4;
        return quarter;
    }

    
    public static long Insert(EhrQuarterlyKey ehrQuarterlyKey)
    {
        return EhrQuarterlyKeyCrud.Insert(ehrQuarterlyKey);
    }

    
    public static void Update(EhrQuarterlyKey ehrQuarterlyKey)
    {
        EhrQuarterlyKeyCrud.Update(ehrQuarterlyKey);
    }

    
    public static void Delete(long ehrQuarterlyKeyNum)
    {
        var command = "DELETE FROM ehrquarterlykey WHERE EhrQuarterlyKeyNum = " + SOut.Long(ehrQuarterlyKeyNum);
        Db.NonQ(command);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.



    ///<summary>Gets one EhrQuarterlyKey from the db.</summary>
    public static EhrQuarterlyKey GetOne(long ehrQuarterlyKeyNum){

        return Crud.EhrQuarterlyKeyCrud.SelectOne(ehrQuarterlyKeyNum);
    }


    */
}