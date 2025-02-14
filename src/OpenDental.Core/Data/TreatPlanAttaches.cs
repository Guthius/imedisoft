using System.Collections.Generic;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class TreatPlanAttaches
{
    public static void SetPriorityForTreatPlanProcs(long priority, long treatPlanNum, List<long> procNums)
    {
        if (procNums is not {Count: > 0})
        {
            return;
        }

        Db.NonQ(
            $"""
             UPDATE treatplanattach 
             SET Priority = {priority} 
             WHERE TreatPlanNum = {treatPlanNum} 
             AND ProcNum IN ({string.Join(",", procNums)})
             """);
    }

    public static void Insert(TreatPlanAttach treatPlanAttach)
    {
        TreatPlanAttachCrud.Insert(treatPlanAttach);
    }

    public static List<TreatPlanAttach> GetAllForPatNum(long patNum)
    {
        var treatPlans = TreatPlans.GetAllForPat(patNum);

        return treatPlans.Count == 0 ? [] : GetAllForTPs(treatPlans.Select(x => x.TreatPlanNum).Distinct().ToList());
    }

    public static List<TreatPlanAttach> GetAllForTPs(List<long> treatPlanNums)
    {
        return treatPlanNums.Count == 0 ? [] : TreatPlanAttachCrud.SelectMany("SELECT * FROM treatplanattach WHERE TreatPlanNum IN (" + string.Join(", ", treatPlanNums) + ")");
    }

    public static List<TreatPlanAttach> GetAllForTreatPlan(long treatPlanNum)
    {
        return TreatPlanAttachCrud.SelectMany("SELECT * FROM treatplanattach WHERE TreatPlanNum = " + treatPlanNum);
    }

    public static void Sync(List<TreatPlanAttach> treatPlanAttachesNew, long treatPlanNum)
    {
        var treatPlanAttachesOld = GetAllForTreatPlan(treatPlanNum);

        TreatPlanAttachCrud.Sync(treatPlanAttachesNew, treatPlanAttachesOld);
    }
}