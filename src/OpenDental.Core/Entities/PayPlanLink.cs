using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class PayPlanLink : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PayPlanLinkNum;

    ///<summary>FK to payplan.PayPlanNum</summary>
    public long PayPlanNum;

    ///<summary>Enum:PayPlanLinkType  The object type being linked to be credited. </summary>
    public PayPlanLinkType LinkType;

    ///<summary>Stores the FKey of object being linked, known from link type. </summary>
    public long FKey;

    ///<summary>Optional override if full amount of object is not desired. </summary>
    public double AmountOverride;

    ///<summary>DateTime. Date the link was created. If pref.PayPlanItemDateShowProc is false, then this is also the date that this entry shows in the main account module.</summary>
    public DateTime SecDateTEntry;

    public PayPlanLink Copy()
    {
        return (PayPlanLink) MemberwiseClone();
    }
}

public enum PayPlanLinkType
{
    None,
    Adjustment,
    Procedure,
    OrthoCase
}