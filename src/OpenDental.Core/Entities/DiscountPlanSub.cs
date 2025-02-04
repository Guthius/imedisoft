using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class DiscountPlanSub : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long DiscountSubNum;

    ///<summary>FK to discountplan.DiscountPlanNum, represents which plan the patient is subscribed to.</summary>
    public long DiscountPlanNum;

    ///<summary>FK to patient.PatNum which represents the subscriber</summary>
    public long PatNum;

    ///<summary>When the discount plan should start to impact procedure fees.</summary>
    public DateTime DateEffective;

    ///<summary>When the discount plan should no longer impact procedure fees.</summary>
    public DateTime DateTerm;
    
    public string SubNote;

    public bool IsValidForDate(DateTime date)
    {
        return date >= DateEffective && (DateTerm.Year < 1880 || date <= DateTerm);
    }
}