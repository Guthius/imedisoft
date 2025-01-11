using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class LabTurnarounds
{
    
    public static List<LabTurnaround> GetForLab(long laboratoryNum)
    {
        var command = "SELECT * FROM labturnaround WHERE LaboratoryNum=" + SOut.Long(laboratoryNum);
        var table = DataCore.GetTable(command);
        var listLabTurnarounds = new List<LabTurnaround>();
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var labTurnaround = new LabTurnaround();
            labTurnaround.LabTurnaroundNum = SIn.Long(table.Rows[i][0].ToString());
            labTurnaround.LaboratoryNum = SIn.Long(table.Rows[i][1].ToString());
            labTurnaround.Description = SIn.String(table.Rows[i][2].ToString());
            labTurnaround.DaysPublished = SIn.Int(table.Rows[i][3].ToString());
            labTurnaround.DaysActual = SIn.Int(table.Rows[i][4].ToString());
            listLabTurnarounds.Add(labTurnaround);
        }

        return listLabTurnarounds;
    }

    ///<summary>Gets one labturnaround for the API. Returns null if not found.</summary>
    public static LabTurnaround GetOneLabTurnaroundForApi(long labTurnaroundNum)
    {
        var command = "SELECT * FROM labturnaround WHERE LabTurnaroundNum=" + SOut.Long(labTurnaroundNum);
        return LabTurnaroundCrud.SelectOne(command);
    }

    ///<summary>Gets a list of labturnarounds optionally filtered for the API. Returns an empty list if not found.</summary>
    public static List<LabTurnaround> GetLabTurnaroundsForApi(int limit, int offset, long laboratoryNum)
    {
        var command = "SELECT * FROM labturnaround ";
        if (laboratoryNum > 0) command += "WHERE LaboratoryNum=" + SOut.Long(laboratoryNum) + " ";
        command += "ORDER BY LabTurnaroundNum " //Ensure order for limit and offset.
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return LabTurnaroundCrud.SelectMany(command);
    }

    /// <summary>
    ///     This is used when saving a laboratory.  All labturnarounds for the lab are deleted and recreated.  So the list
    ///     that's passed in will not have the correct keys set.  The key columns will be ignored.
    /// </summary>
    public static void SetForLab(long laboratoryNum, List<LabTurnaround> listLabTurnarounds)
    {
        var command = "DELETE FROM labturnaround WHERE LaboratoryNum=" + SOut.Long(laboratoryNum);
        Db.NonQ(command);
        for (var i = 0; i < listLabTurnarounds.Count; i++)
        {
            listLabTurnarounds[i].LaboratoryNum = laboratoryNum;
            Insert(listLabTurnarounds[i]);
        }
    }

    
    public static long Insert(LabTurnaround labTurnaround)
    {
        return LabTurnaroundCrud.Insert(labTurnaround);
    }

    /// <summary>
    ///     Calculates the due date by adding the number of business days listed.  Adds an additional day for each office
    ///     holiday.
    /// </summary>
    public static DateTime ComputeDueDate(DateTime dateStart, int days)
    {
        var date = dateStart;
        var counter = 0;
        while (true)
        {
            if (counter >= days) break;
            date = date.AddDays(1);
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) continue;
            if (Schedules.DateIsHoliday(date)) continue;
            counter++;
        }

        return date + TimeSpan.FromHours(17); //always due at 5pm on day specified.
    }
}