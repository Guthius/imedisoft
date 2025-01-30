using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class InsBlueBookRules
{
    public static List<InsBlueBookRule> GetAll()
    {
        return InsBlueBookRuleCrud.SelectMany("SELECT insbluebookrule.* FROM insbluebookrule");
    }

    public static void Update(InsBlueBookRule insBlueBookRule, InsBlueBookRule insBlueBookRuleOld)
    {
        InsBlueBookRuleCrud.Update(insBlueBookRule, insBlueBookRuleOld);
    }

    public static bool IsDateLimitedType(InsBlueBookRule insBlueBookRule)
    {
        return insBlueBookRule.RuleType is
            InsBlueBookRuleType.InsuranceCarrierGroup or
            InsBlueBookRuleType.InsuranceCarrier or
            InsBlueBookRuleType.GroupNumber or
            InsBlueBookRuleType.InsurancePlan;
    }
}