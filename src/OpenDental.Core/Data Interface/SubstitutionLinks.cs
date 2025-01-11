using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class SubstitutionLinks
{
    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static void Update(SubstitutionLink substitutionLink){

        Crud.SubstitutionLinkCrud.Update(substitutionLink);
    }

    
    public static void Delete(long substitutionLinkNum) {

        Crud.SubstitutionLinkCrud.Delete(substitutionLinkNum);
    }
    */

    ///<summary>Gets one SubstitutionLink from the db.</summary>
    public static SubstitutionLink GetOne(long substitutionLinkNum)
    {
        return SubstitutionLinkCrud.SelectOne(substitutionLinkNum);
    }

    
    public static List<SubstitutionLink> GetAllForPlans(List<InsPlan> listInsPlans)
    {
        return GetAllForPlans(listInsPlans.Select(x => x.PlanNum).ToArray());
    }

    
    public static List<SubstitutionLink> GetAllForPlans(params long[] planNumArray)
    {
        if (planNumArray.Length == 0) return new List<SubstitutionLink>();
        var listPlanNums = new List<long>(planNumArray);
        var command = "SELECT * FROM substitutionlink WHERE PlanNum IN(" + string.Join(",", listPlanNums.Select(x => SOut.Long(x))) + ")";
        return SubstitutionLinkCrud.SelectMany(command);
    }

    /// <summary>
    ///     Inserts, updates, or deletes the passed in list against the stale list listOld.  Returns true if db changes
    ///     were made.
    /// </summary>
    public static bool Sync(List<SubstitutionLink> listSubstitutionLinksNew, List<SubstitutionLink> listSubstitutionLinksOld)
    {
        return SubstitutionLinkCrud.Sync(listSubstitutionLinksNew, listSubstitutionLinksOld);
    }

    public static List<SubstitutionLink> FilterSubLinksByCodeNum(long codeNum, List<SubstitutionLink> listSubstitutionLinks)
    {
        if (listSubstitutionLinks is null) return new List<SubstitutionLink>();
        return listSubstitutionLinks.Where(x => x.CodeNum == codeNum).ToList();
    }

    /// <summary>
    ///     Follows documented hierarchy to return a sub link based on substitution condition. Function checks that the
    ///     list of sublinks is already filtered. Can return null.
    /// </summary>
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

    ///<summary>Returns true if the procedure has a substitution code for the give tooth and InsPlans.</summary>
    public static bool HasSubstCodeForProcCode(ProcedureCode procedureCode, string strToothNum, List<SubstitutionLink> listSubstitutionLinks,
        List<InsPlan> listInsPlansPat)
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

    /// <summary>
    ///     Inserts a copy of all of the planNumOld SubstitutionLinks with the planNumNew. This should be done every time a new
    ///     insplan gets created
    ///     and you want to maintain the SubstitutionLink of the old insplan.
    /// </summary>
    public static void CopyLinksToNewPlan(long planNumNew, long planNumOld)
    {
        //Get a list of the sub links of the old insplan. After the foreach loop below, this list will no longer contain the sub links for the old insplan.
        var listSubstitutionLinksOfOldPlan = GetAllForPlans(planNumOld);
        for (var i = 0; i < listSubstitutionLinksOfOldPlan.Count; i++)
            //Only change the old planNum with the new planNum. Insert will "create" a new SubstitutionLink with a new primary key. 
            listSubstitutionLinksOfOldPlan[i].PlanNum = planNumNew;
        InsertMany(listSubstitutionLinksOfOldPlan);
    }

    
    public static long Insert(SubstitutionLink substitutionLink)
    {
        return SubstitutionLinkCrud.Insert(substitutionLink);
    }

    public static void InsertMany(List<SubstitutionLink> listSubstitutionLinks)
    {
        SubstitutionLinkCrud.InsertMany(listSubstitutionLinks);
    }
}