using System.Collections.Generic;
using CodeBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class InsBlueBookRules
{
    #region Get Methods

    ///<summary>Gets all insbluebookrules from db as list.</summary>
    public static List<InsBlueBookRule> GetAll()
    {
        var command = "SELECT insbluebookrule.* FROM insbluebookrule";
        return InsBlueBookRuleCrud.SelectMany(command);
    }

    #endregion Get Methods

    #region Modification Methods

    ///<summary>Updates an insbluebookrule to the db if it has changed.</summary>
    public static void Update(InsBlueBookRule insBlueBookRule, InsBlueBookRule insBlueBookRuleOld)
    {
        InsBlueBookRuleCrud.Update(insBlueBookRule, insBlueBookRuleOld);
    }

    #endregion Modification Methods

    #region Misc Methods

    /// <summary>
    ///     Returns true if a date limitation applies to the rule's InsBlueBookRuleType. Types InsuranceCarrierGroup,
    ///     InsuranceCarrier, GroupNumber, and InsurancePlan.
    /// </summary>
    public static bool IsDateLimitedType(InsBlueBookRule insBlueBookRule)
    {
        return insBlueBookRule.RuleType.In(
            InsBlueBookRuleType.InsuranceCarrierGroup,
            InsBlueBookRuleType.InsuranceCarrier,
            InsBlueBookRuleType.GroupNumber,
            InsBlueBookRuleType.InsurancePlan);
    }

    #endregion Misc Methods

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.
    #region Get Methods
    
    public static List<InsBlueBookRule> Refresh(long patNum){

        string command="SELECT * FROM insbluebookrule WHERE PatNum = "+POut.Long(patNum);
        return Crud.InsBlueBookRuleCrud.SelectMany(command);
    }

    ///<summary>Gets one InsBlueBookRule from the db.</summary>
    public static InsBlueBookRule GetOne(long insBlueBookRuleNum){

        return Crud.InsBlueBookRuleCrud.SelectOne(insBlueBookRuleNum);
    }
    #endregion Get Methods
    #region Modification Methods
    
    public static long Insert(InsBlueBookRule insBlueBookRule){

        return Crud.InsBlueBookRuleCrud.Insert(insBlueBookRule);
    }
    
    public static void Update(InsBlueBookRule insBlueBookRule){

        Crud.InsBlueBookRuleCrud.Update(insBlueBookRule);
    }
    
    public static void Delete(long insBlueBookRuleNum) {

        Crud.InsBlueBookRuleCrud.Delete(insBlueBookRuleNum);
    }
    #endregion Modification Methods
    #region Misc Methods



    #endregion Misc Methods
    */
}