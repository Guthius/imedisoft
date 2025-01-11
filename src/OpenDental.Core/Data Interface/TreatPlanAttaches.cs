using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class TreatPlanAttaches
{
    #region Update

    ///<summary>Sets the priority for the procedures passed in that are associated to the designated treatment plan.</summary>
    public static void SetPriorityForTreatPlanProcs(long priority, long treatPlanNum, List<long> listProcNums)
    {
        if (listProcNums.IsNullOrEmpty()) return;

        Db.NonQ($@"UPDATE treatplanattach SET Priority = {SOut.Long(priority)}
				WHERE TreatPlanNum = {SOut.Long(treatPlanNum)}
				AND ProcNum IN({string.Join(",", listProcNums.Select(x => SOut.Long(x)))})");
    }

    #endregion

    
    public static long Insert(TreatPlanAttach treatPlanAttach)
    {
        return TreatPlanAttachCrud.Insert(treatPlanAttach);
    }

    
    public static void Delete(long treatPlanAttachNum)
    {
        TreatPlanAttachCrud.Delete(treatPlanAttachNum);
    }

    ///<summary>Gets one TreatPlanAttach from the db.</summary>
    public static TreatPlanAttach GetOne(long treatPlanAttachNum)
    {
        return TreatPlanAttachCrud.SelectOne(treatPlanAttachNum);
    }

    
    public static List<TreatPlanAttach> GetAllForPatNum(long patNum)
    {
        var listTreatPlans = TreatPlans.GetAllForPat(patNum);
        if (listTreatPlans.Count == 0) return new List<TreatPlanAttach>();

        return GetAllForTPs(listTreatPlans.Select(x => x.TreatPlanNum).Distinct().ToList());
    }

    ///<summary>Gets all treatplanattaches with TreatPlanNum in listTpNums.</summary>
    public static List<TreatPlanAttach> GetAllForTPs(List<long> listTreatPlanNums)
    {
        if (listTreatPlanNums.Count == 0) return new List<TreatPlanAttach>();

        var command = "SELECT * FROM treatplanattach WHERE TreatPlanNum IN (" + string.Join(",", listTreatPlanNums) + ")";
        return TreatPlanAttachCrud.SelectMany(command);
    }

    
    public static List<TreatPlanAttach> GetAllForTreatPlan(long treatPlanNum)
    {
        var command = "SELECT * FROM treatplanattach WHERE TreatPlanNum=" + SOut.Long(treatPlanNum);
        return TreatPlanAttachCrud.SelectMany(command);
    }

    
    public static void DeleteOrphaned()
    {
        //Orphaned TreatPlanAttaches due to missing treatment plans
        var command = "DELETE FROM treatplanattach WHERE TreatPlanNum NOT IN (SELECT TreatPlanNum FROM treatplan)";
        Db.NonQ(command);
        //Orphaned TreatPlanAttaches due to missing procedures or procedures that are no longer TP or TPi status.
        command = "DELETE FROM treatplanattach WHERE ProcNum NOT IN (SELECT ProcNum FROM procedurelog " +
                  "WHERE ProcStatus IN (" + string.Join(",", new[] {(int) ProcStat.TP, (int) ProcStat.TPi}) + "))";
        Db.NonQ(command);
    }

    
    public static void Sync(List<TreatPlanAttach> listTreatPlanAttachesNew, long treatPlanNum)
    {
        var listTreatPlanAttachesDB = GetAllForTreatPlan(treatPlanNum);
        TreatPlanAttachCrud.Sync(listTreatPlanAttachesNew, listTreatPlanAttachesDB);
    }
}