using System.Collections.Generic;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class InsPlanSubstitution(ProcedureCode procCode, SubstitutionLink subLink = null)
{
    public readonly ProcedureCode ProcCode = procCode;
    public readonly SubstitutionLink SubLink = subLink;
    public SubstitutionCondition SubCondition = subLink?.SubstOnlyIf ?? procCode.SubstOnlyIf;

    public static bool AreEqual(InsPlanSubstitution insPlanSub1, InsPlanSubstitution insPlanSub2)
    {
        return insPlanSub1.ProcCode?.CodeNum == insPlanSub2.ProcCode?.CodeNum
               && insPlanSub1.SubLink?.SubstitutionCode == insPlanSub2.SubLink?.SubstitutionCode
               && insPlanSub1.SubCondition == insPlanSub2.SubCondition;
    }

    public static bool HasDuplicates(List<InsPlanSubstitution> listInsPlanSubs)
    {
        for (var i = 0; i < listInsPlanSubs.Count; i++)
        {
            for (var j = i + 1; j < listInsPlanSubs.Count; j++)
            {
                if (AreEqual(listInsPlanSubs[i], listInsPlanSubs[j]))
                {
                    return true;
                }
            }
        }

        return false;
    }
}