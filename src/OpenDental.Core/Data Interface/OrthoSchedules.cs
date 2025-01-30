using System;
using System.Collections.Generic;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class OrthoSchedules
{
    public static long Insert(OrthoSchedule orthoSchedule)
    {
        return OrthoScheduleCrud.Insert(orthoSchedule);
    }

    public static void Update(OrthoSchedule orthoScheduleNew, OrthoSchedule orthoScheduleOld)
    {
        OrthoScheduleCrud.Update(orthoScheduleNew, orthoScheduleOld);
    }

    public static OrthoSchedule GetOne(long orthoScheduleNum)
    {
        return OrthoScheduleCrud.SelectOne(orthoScheduleNum);
    }

    public static List<OrthoSchedule> GetMany(List<long> listOrthoScheduleNums)
    {
        if (listOrthoScheduleNums.Count == 0) return new List<OrthoSchedule>();

        var command = $"SELECT * FROM orthoschedule WHERE orthoschedule.OrthoScheduleNum IN({string.Join(",", listOrthoScheduleNums)})";
        return OrthoScheduleCrud.SelectMany(command);
    }

    public static int CalculatePlannedVisitsCount(double bandingAmount, double debondAmount, double visitAmount, double totalFee)
    {
        if (CompareDouble.IsZero(visitAmount)) return 0;
        var allVisitsAmount = Math.Round((totalFee - (bandingAmount + debondAmount)) * 100) / 100;
        var plannedVisitsCount = (int) Math.Round(allVisitsAmount / visitAmount);
        if (CompareDouble.IsLessThan(plannedVisitsCount * visitAmount, allVisitsAmount)) plannedVisitsCount++;
        return plannedVisitsCount;
    }

    public static int CalculateAutoOrthoTimeInMonths(DateTime dateFirstOrthoProc, DateSpan dateSpan, int txMonthsTotal)
    {
        var txTimeInMonths = dateSpan.YearsDiff * 12 + dateSpan.MonthsDiff + (dateSpan.DaysDiff < 15 ? 0 : 1);
        if (txTimeInMonths > txMonthsTotal && PrefC.GetBool(PrefName.OrthoDebondProcCompletedSetsMonthsTreat))
        {
            //Capping if preference is set
            dateSpan = new DateSpan(dateFirstOrthoProc, dateFirstOrthoProc.AddMonths(txMonthsTotal));
            txTimeInMonths = dateSpan.YearsDiff * 12 + dateSpan.MonthsDiff + (dateSpan.DaysDiff < 15 ? 0 : 1);
        }

        return txTimeInMonths;
    }
}