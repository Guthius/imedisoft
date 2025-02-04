using System.ComponentModel;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class InsBlueBookRule : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long InsBlueBookRuleNum;

    public int ItemOrder;

    ///<summary>Enum:InsBlueBookRuleType Types 0 to 3 are for rules that determine estimates by looking at the payment history of an insurance plan, insurance plan group, carrier, or carrier group. Type 4 utilizes fee schedules that are attached to out of network plans that are manually maintained by the user. Type 5 bases estimates off of the UCR fee.</summary>
    public InsBlueBookRuleType RuleType;

    ///<summary>The number of years, months, weeks, or days of insurance payment history that will be considered when generating a Blue Book estimate. Will be 0 if the RuleType is 4-ManualBlueBookSchedule or 5-UcrFee as limits do not apply to these rule types.</summary>
    public int LimitValue;

    ///<summary>Enum:InsBlueBookRuleLimitType Determines the unit of time that InsBlueBookRule.LimitValue represents. Will be 0-None if the RuleType is 4-ManualBlueBookSchedule or 5-UcrFee as limits do not apply to these rule types.</summary>
    public InsBlueBookRuleLimitType LimitType;

    public InsBlueBookRule Copy()
    {
        return (InsBlueBookRule) MemberwiseClone();
    }
}

public enum InsBlueBookRuleType
{
    [Description("Insurance Plan")]
    InsurancePlan,

    [Description("Group Number")]
    GroupNumber,

    [Description("Insurance Carrier")]
    InsuranceCarrier,

    [Description("Insurance Carrier Group")]
    InsuranceCarrierGroup,

    [Description("Manual Blue Book Fee Schedule")]
    ManualBlueBookSchedule,

    [Description("UCR Fee")]
    UcrFee
}

public enum InsBlueBookRuleLimitType
{
    [Description("None")]
    None,

    [Description("Years")]
    Years,

    [Description("Months")]
    Months,

    [Description("Weeks")]
    Weeks,

    [Description("Days")]
    Days
}