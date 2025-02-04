using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class FamAging : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PatNum;

    ///<summary>Aged balance from 0 to 30 days old. Aging numbers are for entire family.  Only stored with guarantor.</summary>
    public double Bal_0_30;

    ///<summary>Aged balance from 31 to 60 days old. Aging numbers are for entire family.  Only stored with guarantor.</summary>
    public double Bal_31_60;

    ///<summary>Aged balance from 61 to 90 days old. Aging numbers are for entire family.  Only stored with guarantor.</summary>
    public double Bal_61_90;

    ///<summary>Aged balance over 90 days old. Aging numbers are for entire family.  Only stored with guarantor.</summary>
    public double BalOver90;

    ///<summary>Insurance Estimate for entire family.  Only stored with guarantor.</summary>
    public double InsEst;

    ///<summary>Total balance for entire family before insurance estimate.  Not the same as the sum of the 4 aging balances because this can be 
    ///negative.  Only stored with guarantor.</summary>
    public double BalTotal;

    ///<summary>Amount "due now" for all payment plans such that someone in this family is the payment plan guarantor.  
    ///This is the total of all payment plan charges past due (taking into account the PayPlansBillInAdvanceDays setting) subtract the amount 
    ///already paid for the payment plans.  Only stored with family guarantor.</summary>
    public double PayPlanDue;
}