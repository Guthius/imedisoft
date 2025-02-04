using System.Collections.Generic;
using System.Linq;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class SubstitutionLinks
{
    public static List<SubstitutionLink> GetAllForPlans(List<InsPlan> listInsPlans)
    {
        return GetAllForPlans(listInsPlans.Select(x => x.PlanNum).ToArray());
    }

    public static List<SubstitutionLink> GetAllForPlans(params long[] planNumArray)
    {
        if (planNumArray.Length == 0) return [];
        var listPlanNums = new List<long>(planNumArray);
        var command = "SELECT * FROM substitutionlink WHERE PlanNum IN(" + string.Join(",", listPlanNums.Select(x => (x))) + ")";
        return SubstitutionLinkCrud.SelectMany(command);
    }

    public static void Sync(List<SubstitutionLink> listSubstitutionLinksNew, List<SubstitutionLink> listSubstitutionLinksOld)
    {
        SubstitutionLinkCrud.Sync(listSubstitutionLinksNew, listSubstitutionLinksOld);
    }

    public static List<SubstitutionLink> FilterSubLinksByCodeNum(long codeNum, List<SubstitutionLink> listSubstitutionLinks)
    {
        if (listSubstitutionLinks is null) return [];
        return listSubstitutionLinks.Where(x => x.CodeNum == codeNum).ToList();
    }

    public static SubstitutionLink GetSubLinkByHierarchy(ProcedureCode procedureCode, string strToothNum, List<SubstitutionLink> listSubstitutionLinks)
    {
        SubstitutionLink substitutionLink = null;
        //Make a copy of the list in case we need to filter it.
        var listSubstitutionLinksThisCode = FilterSubLinksByCodeNum(procedureCode.CodeNum, listSubstitutionLinks);
        //If any of the conditions are 'Never' then we return null here
        if (listSubstitutionLinksThisCode.Any(x => x.SubstOnlyIf == SubstitutionCondition.Never))
            substitutionLink = listSubstitutionLinksThisCode.FirstOrDefault(x => x.SubstOnlyIf == SubstitutionCondition.Never);
        //if a tooth is a second molar, it is also considered a molar and so we need to check for this scenario first
        else if (listSubstitutionLinksThisCode.Any(x => x.SubstOnlyIf == SubstitutionCondition.SecondMolar) && Tooth.IsSecondMolar(strToothNum))
            substitutionLink = listSubstitutionLinksThisCode.FirstOrDefault(x => x.SubstOnlyIf == SubstitutionCondition.SecondMolar);
        else if (listSubstitutionLinksThisCode.Any(x => x.SubstOnlyIf == SubstitutionCondition.Molar) && Tooth.IsMolar(strToothNum))
            substitutionLink = listSubstitutionLinksThisCode.FirstOrDefault(x => x.SubstOnlyIf == SubstitutionCondition.Molar);
        else if (listSubstitutionLinksThisCode.Any(x => x.SubstOnlyIf == SubstitutionCondition.Posterior) && Tooth.IsPosterior(strToothNum))
            substitutionLink = listSubstitutionLinksThisCode.FirstOrDefault(x => x.SubstOnlyIf == SubstitutionCondition.Posterior);
        else
            substitutionLink = listSubstitutionLinksThisCode.FirstOrDefault(x => x.SubstOnlyIf == SubstitutionCondition.Always);
        return substitutionLink;
    }

    public static bool HasSubstCodeForPlan(InsPlan insPlan, long codeNum, List<SubstitutionLink> listSubstitutionLinks)
    {
        if (insPlan.CodeSubstNone) return false;
        return !listSubstitutionLinks.Exists(x => x.PlanNum == insPlan.PlanNum && x.CodeNum == codeNum && x.SubstOnlyIf == SubstitutionCondition.Never);
    }

    public static bool HasSubstCodeForProcCode(ProcedureCode procedureCode, string strToothNum, List<SubstitutionLink> listSubstitutionLinks, List<InsPlan> listInsPlansPat)
    {
        for (var i = 0; i < listInsPlansPat.Count; i++)
        {
            //Check to see if any allow substitutions.
            if (!HasSubstCodeForPlan(listInsPlansPat[i], procedureCode.CodeNum, listSubstitutionLinks)) continue;
            var subCodeNum = ProcedureCodes.GetSubstituteCodeNum(procedureCode.ProcCode, strToothNum, listInsPlansPat[i].PlanNum, listSubstitutionLinks); //for posterior composites
            if (procedureCode.CodeNum != subCodeNum && subCodeNum > 0) return true;
        }

        return false;
    }

    public static void CopyLinksToNewPlan(long planNumNew, long planNumOld)
    {
        //Get a list of the sub links of the old insplan. After the foreach loop below, this list will no longer contain the sub links for the old insplan.
        var listSubstitutionLinksOfOldPlan = GetAllForPlans(planNumOld);
        for (var i = 0; i < listSubstitutionLinksOfOldPlan.Count; i++)
            //Only change the old planNum with the new planNum. Insert will "create" a new SubstitutionLink with a new primary key. 
            listSubstitutionLinksOfOldPlan[i].PlanNum = planNumNew;
        InsertMany(listSubstitutionLinksOfOldPlan);
    }

    public static void InsertMany(List<SubstitutionLink> listSubstitutionLinks)
    {
        SubstitutionLinkCrud.InsertMany(listSubstitutionLinks);
    }
}