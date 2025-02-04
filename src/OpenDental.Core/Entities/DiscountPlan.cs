using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class DiscountPlan : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long DiscountPlanNum;
    
    public string Description;

    ///<summary>FK to feesched.FeeSchedNum</summary>
    public long FeeSchedNum;

    ///<summary>FK to definition.DefNum.  Represents the adjustment type of the feesched plan.</summary>
    public long DefNum;

    public bool IsHidden;

    ///<summary>Note for this plan.</summary>
    public string PlanNote;

    ///<summary>Number of Procedures allowed for a discount plans Exam category.</summary>
    public int ExamFreqLimit = -1;

    ///<summary>Number of Procedures allowed for a discount plans X-Ray category.</summary>
    public int XrayFreqLimit = -1;

    ///<summary>Number of Procedures allowed for a discount plans Prophylaxis category.</summary>
    public int ProphyFreqLimit = -1;

    ///<summary>Number of Procedures allowed for a discount plans Fluoride category.</summary>
    public int FluorideFreqLimit = -1;

    ///<summary>Number of Procedures allowed for a discount plans Periodontal category.</summary>
    public int PerioFreqLimit = -1;

    ///<summary>Number of Procedures allowed for a discount plans Limited Exam category.</summary>
    public int LimitedExamFreqLimit = -1;

    ///<summary>Number of Procedures allowed for a discount plans Periapical X-Ray category.</summary>
    public int PAFreqLimit = -1;

    ///<summary>Annual discount maximum for frequency limitations. -1 indicates blank or no annual max limitation.</summary>
    public double AnnualMax = -1;
    
    public DiscountPlan Copy()
    {
        return (DiscountPlan) MemberwiseClone();
    }
}