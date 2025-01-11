using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class WebSchedCarrierRules
{
    #region Methods - Get

    ///<summary>Returns a list of WebSchedCarrierRules for the passed in clinic, orders the list by CarrierName.</summary>
    public static List<WebSchedCarrierRule> GetWebSchedCarrierRulesForClinic(long clinicNum)
    {
        var command = "SELECT * FROM webschedcarrierrule WHERE ClinicNum = " + SOut.Long(clinicNum) + " ORDER BY CarrierName";
        return WebSchedCarrierRuleCrud.SelectMany(command);
    }

    ///<summary>Returns a list of WebSchedCarrierRules for the passed in list of clinics.</summary>
    public static List<WebSchedCarrierRule> GetWebSchedCarrierRulesForClinics(List<long> listClinicNums)
    {
        var command = "SELECT * FROM webschedcarrierrule WHERE ClinicNum IN(" + string.Join(",", listClinicNums.Select(x => SOut.Long(x))) + ")";
        return WebSchedCarrierRuleCrud.SelectMany(command);
    }

    #endregion Methods - Get

    #region Methods - Modify

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

    #endregion Methods - Modify
}