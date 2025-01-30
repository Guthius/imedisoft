using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class WebSchedCarrierRules
{
    public static List<WebSchedCarrierRule> GetWebSchedCarrierRulesForClinic(long clinicNum)
    {
        return WebSchedCarrierRuleCrud.SelectMany("SELECT * FROM webschedcarrierrule WHERE ClinicNum = " + clinicNum + " ORDER BY CarrierName");
    }

    public static List<WebSchedCarrierRule> GetWebSchedCarrierRulesForClinics(List<long> listClinicNums)
    {
        return WebSchedCarrierRuleCrud.SelectMany("SELECT * FROM webschedcarrierrule WHERE ClinicNum IN(" + string.Join(",", listClinicNums) + ")");
    }
    
    public static void InsertMany(List<WebSchedCarrierRule> listWebSchedCarrierRules)
    {
        WebSchedCarrierRuleCrud.InsertMany(listWebSchedCarrierRules);
    }
    
    public static void Update(WebSchedCarrierRule webSchedCarrierRule)
    {
        WebSchedCarrierRuleCrud.Update(webSchedCarrierRule);
    }

    public static void DeleteMany(List<long> listWebSchedCarrierRuleNums)
    {
        WebSchedCarrierRuleCrud.DeleteMany(listWebSchedCarrierRuleNums);
    }
}