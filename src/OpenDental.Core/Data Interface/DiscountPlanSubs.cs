using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class DiscountPlanSubs
{
    public static DiscountPlanSub GetSubForPat(long patNum)
    {
        var command = "SELECT * FROM discountplansub WHERE PatNum = " + SOut.Long(patNum);
        return DiscountPlanSubCrud.SelectOne(command);
    }

    public static List<DiscountPlanSub> GetSubsForPats(List<long> listPatNums)
    {
        if (listPatNums.Count < 1) return new List<DiscountPlanSub>();

        var command = "SELECT * FROM discountplansub WHERE PatNum IN (" + string.Join(",", listPatNums) + ")";
        return DiscountPlanSubCrud.SelectMany(command);
    }

    public static DateTime GetAnnualMaxDateEffective(DateTime dateEffective)
    {
        var dateStart = dateEffective;
        if (dateStart.Year < 1880) //example 0001
            dateStart = dateStart.AddYears(DateTime.Now.Year - dateStart.Year); //=0001.AddYears(2024-0001)
        return dateStart;
    }

    public static DateTime GetAnnualMaxDateTerm(DateTime dateTerm)
    {
        var dateEnd = dateTerm;
        if (dateEnd.Year < 1880) dateEnd = DateTime.MaxValue;
        return dateEnd;
    }

    public static DateTime GetDateEffectiveForAnnualDateRangeSegment(DateTime dateRefPoint, DateTime dateEffective, DateTime dateTerm)
    {
        if (dateRefPoint < dateEffective || dateRefPoint > dateTerm) //Outside of date range
            return dateEffective;
        if (dateEffective.AddYears(1) <= dateRefPoint)
        {
            var numYearsLimit = dateRefPoint.Year - dateEffective.Year;
            for (var numYears = 0; numYears < numYearsLimit; numYears++)
            {
                if (dateEffective > dateRefPoint)
                {
                    dateEffective = dateEffective.AddYears(-1);
                    break;
                }

                dateEffective = dateEffective.AddYears(1);
            }
        }

        return dateEffective;
    }

    public static DateTime GetDateTermForAnnualDateRangeSegment(DateTime dateRefPoint, DateTime dateEffective, DateTime dateTerm)
    {
        if (dateRefPoint < dateEffective || dateRefPoint > dateTerm) //Outside of date range
            return dateTerm;
        if (dateEffective.AddYears(1) <= dateRefPoint)
        {
            var numYearsLimit = dateRefPoint.Year - dateEffective.Year;
            for (var numYears = 0; numYears < numYearsLimit; numYears++)
            {
                if (dateEffective > dateRefPoint)
                {
                    dateEffective = dateEffective.AddYears(-1);
                    break;
                }

                dateEffective = dateEffective.AddYears(1);
            }
        }

        if (dateTerm > dateEffective.AddYears(1)) dateTerm = dateEffective.AddYears(1).AddDays(-1);
        return dateTerm;
    }

    public static void Update(DiscountPlanSub discountPlanSub)
    {
        DiscountPlanSubCrud.Update(discountPlanSub);
    }

    public static void UpdateAssociatedDiscountPlanAmts(List<DiscountPlanSub> listDiscountPlanSubs, bool isDiscountPlanSubBeingDeleted = false)
    {
        if (listDiscountPlanSubs.IsNullOrEmpty()) return;
        var listPatNums = listDiscountPlanSubs.Select(x => x.PatNum).Distinct().ToList();
        //Get all TP'd procs for all patient DiscountPlanSubs
        var listProcedures = Procedures.GetAllForPatsAndStatuses(listPatNums, ProcStat.TP);
        //Get all adjustments for all patient DiscountPlanSubs
        var listAdjustments = Adjustments.GetAdjustForPats(listPatNums);
        //Get all DiscountPlans for all patient DiscountPlanSubs
        var listDiscountPlans = DiscountPlans.GetForPats(listPatNums);
        for (var i = 0; i < listDiscountPlanSubs.Count; i++)
        {
            var discountPlanSub = listDiscountPlanSubs[i];
            var discountPlan = listDiscountPlans.FirstOrDefault(x => x.DiscountPlanNum == discountPlanSub.DiscountPlanNum);
            if (discountPlan == null) continue;
            //List of TP procedures for the discountPlanSub, that fall within the discount plan sub date range.
            var listProceduresForPat = listProcedures.FindAll(x => x.PatNum == discountPlanSub.PatNum);
            //List of Existing discountPlan adjustments, should only be populated if patient had a prior DPlan of the same type.
            var listAdjustmentsForPat = listAdjustments.FindAll(x => x.PatNum == discountPlanSub.PatNum && x.AdjType == discountPlan.DefNum);
            if (listProceduresForPat.IsNullOrEmpty()) continue;
            if (isDiscountPlanSubBeingDeleted)
            {
                //If the discount plan sub is being dropped, set all associated procs DiscountPlanAmt to 0;
                listProceduresForPat.ForEach(x => x.DiscountPlanAmt = 0);
            }
            else
            {
                //Otherwise compute the DiscountPlanAmt for all procedures within the date range.
                listProceduresForPat = Procedures.SortListByTreatPlanPriority(listProceduresForPat);
                //Iterates over all of the listProceduresForPat and sets the procedure.DiscountPlanAmt in memory
                var listDiscountPlanProcs = DiscountPlans.GetDiscountPlanProc(listProceduresForPat, discountPlanSub, discountPlan, listAdjustmentsForPat);
                for (var j = 0; j < listProceduresForPat.Count; j++)
                {
                    var procNum = listProceduresForPat[j].ProcNum;
                    listProceduresForPat[j].DiscountPlanAmt = listDiscountPlanProcs.FirstOrDefault(x => x.ProcNum == procNum).DiscountPlanAmt;
                }
            }

            Procedures.UpdateDiscountPlanAmts(listProceduresForPat);
        }
    }

    public static void Insert(DiscountPlanSub discountPlanSub)
    {
        DiscountPlanSubCrud.Insert(discountPlanSub);
    }

    public static void Delete(long discountSubNum)
    {
        DiscountPlanSubCrud.Delete(discountSubNum);
    }

    public static void DeleteForPatient(long patNum)
    {
        var command = "DELETE FROM discountplansub WHERE PatNum = " + SOut.Long(patNum);
        Db.NonQ(command);
    }

    public static long GetDiscountPlanNumForPat(long patNum, DateTime date = default)
    {
        var command = "SELECT DiscountPlanNum FROM discountplansub WHERE PatNum = " + SOut.Long(patNum) + " ";
        if (date.Year > 1880)
            command += "AND (" + SOut.Date(date) + ">=DateEffective) "
                       + "AND (DateTerm='0001-01-01' OR " + SOut.Date(date) + "<=DateTerm)";
        return Db.GetLong(command);
    }

    public static bool HasDiscountPlan(long patNum)
    {
        var command = "SELECT COUNT(*) FROM discountplansub WHERE PatNum=" + SOut.Long(patNum);
        return Db.GetLong(command) > 0;
    }
}