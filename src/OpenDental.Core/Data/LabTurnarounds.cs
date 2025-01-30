using System;
using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class LabTurnarounds
{
    public static List<LabTurnaround> GetForLab(long laboratoryNum)
    {
        var dataTable = DataCore.GetTable("SELECT * FROM labturnaround WHERE LaboratoryNum=" + laboratoryNum);
        
        var labTurnarounds = new List<LabTurnaround>();
        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            labTurnarounds.Add(new LabTurnaround
            {
                LabTurnaroundNum = SIn.Long(dataTable.Rows[i][0].ToString()),
                LaboratoryNum = SIn.Long(dataTable.Rows[i][1].ToString()),
                Description = SIn.String(dataTable.Rows[i][2].ToString()),
                DaysPublished = SIn.Int(dataTable.Rows[i][3].ToString()),
                DaysActual = SIn.Int(dataTable.Rows[i][4].ToString())
            });
        }

        return labTurnarounds;
    }

    public static void SetForLab(long laboratoryNum, List<LabTurnaround> labTurnarounds)
    {
        Db.NonQ("DELETE FROM labturnaround WHERE LaboratoryNum=" + laboratoryNum);

        foreach (var labTurnaround in labTurnarounds)
        {
            labTurnaround.LaboratoryNum = laboratoryNum;
            
            Insert(labTurnaround);
        }
    }
    
    public static void Insert(LabTurnaround labTurnaround)
    {
        LabTurnaroundCrud.Insert(labTurnaround);
    }

    public static DateTime ComputeDueDate(DateTime dateStart, int days)
    {
        var date = dateStart;
        var counter = 0;
        
        while (true)
        {
            if (counter >= days)
            {
                break;
            }
            
            date = date.AddDays(1);
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }
            
            if (Schedules.DateIsHoliday(date))
            {
                continue;
            }
            counter++;
        }

        return date + TimeSpan.FromHours(17); // Always due at 5pm on day specified.
    }
}