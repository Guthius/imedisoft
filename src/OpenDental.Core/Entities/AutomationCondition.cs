using System.ComponentModel;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class AutomationCondition : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long AutomationConditionNum;

    ///<summary>FK to automation.AutomationNum.</summary>
    public long AutomationNum;

    public AutoCondField CompareField;
    public AutoCondComparison Comparison;
    public string CompareString;
}

public enum AutoCondField
{
    [Description("Needs Sheet")]
    NeedsSheet = 0,

    Problem = 1,

    Medication = 2,

    Allergy = 3,

    /// <summary>Example, 23</summary>
    Age = 4,

    /// <summary>
    /// Allowed values are M or F, not case sensitive.
    /// Enforce at entry time.
    /// </summary>
    Gender = 5,

    [Description("Insurance Not Effective")]
    InsuranceNotEffective = 6,

    [Description("Billing Type")]
    BillingType = 7,

    [Description("Insurance Plan ID")]
    PlanNum = 11,

    [Description("Claim Contains Procedure Code")]
    ClaimContainsProcCode = 12,
}

public enum AutoCondComparison
{
    Equals,
    GreaterThan,
    LessThan,
    Contains,

    /// <summary>
    /// Should not be displayed to users to choose from.
    /// Used when the condition has one and only one 'comparison' to trigger it.
    /// E.g. ins not effective.
    /// </summary>
    None
}