using System;
using System.Collections.Generic;
using CodeBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class OrthoSchedules
{
    #region Insert

    ///<summary>Insert a OrthoSchedule into the database.</summary>
    public static long Insert(OrthoSchedule orthoSchedule)
    {
        return OrthoScheduleCrud.Insert(orthoSchedule);
    }

    #endregion Insert

    #region Update

    ///<summary>Update only data that is different in newOrthoSchedule</summary>
    public static void Update(OrthoSchedule orthoScheduleNew, OrthoSchedule orthoScheduleOld)
    {
        OrthoScheduleCrud.Update(orthoScheduleNew, orthoScheduleOld);
    }

    #endregion Update

    #region Get Methods

    ///<summary>Gets one OrthoSchedule from the database.</summary>
    public static OrthoSchedule GetOne(long orthoScheduleNum)
    {
        return OrthoScheduleCrud.SelectOne(orthoScheduleNum);
    }

    ///<summary>Gets all ortho schedules for a list of orthoschedulenums.</summary>
    public static List<OrthoSchedule> GetMany(List<long> listOrthoScheduleNums)
    {
        if (listOrthoScheduleNums.Count == 0) return new List<OrthoSchedule>();

        var command = $"SELECT * FROM orthoschedule WHERE orthoschedule.OrthoScheduleNum IN({string.Join(",", listOrthoScheduleNums)})";
        return OrthoScheduleCrud.SelectMany(command);
    }

    #endregion Get Methods

    #region Delete

    /////<summary>Delete a OrthoSchedule from the database.</summary>
    //public static void Delete(long orthoScheduleNum) {
    //	
    //	Crud.OrthoScheduleCrud.Delete(orthoScheduleNum);
    //}

    #endregion Delete

    #region Misc Methods

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

    #endregion Misc Methods

    //If this table type will exist as cached data, uncomment the Cache Pattern region below and edit.
    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.
    
    public static List<OrthoSchedule> Refresh(long patNum){

        string command="SELECT * FROM orthoschedule WHERE PatNum = "+POut.Long(patNum);
        return Crud.OrthoScheduleCrud.SelectMany(command);
    }
    
    public static long Insert(OrthoSchedule orthoSchedule){

        return Crud.OrthoScheduleCrud.Insert(orthoSchedule);
    }
    
    public static void Update(OrthoSchedule orthoSchedule){

        Crud.OrthoScheduleCrud.Update(orthoSchedule);
    }
    */
}