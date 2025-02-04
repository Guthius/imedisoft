using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class OrthoPlanLink : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long OrthoPlanLinkNum;

    ///<summary>FK to orthocase.OrthoCaseNum.</summary>
    public long OrthoCaseNum;

    ///<summary>Enum:OrthoPlanLinkType  Holds the type of object that is being linked. </summary>
    public OrthoPlanLinkType LinkType;

    ///<summary>Holds the FKey of the object from the LinkType. </summary>
    public long FKey;

    public bool IsActive;

    ///<summary>DateTime. Date plan link was added. Not editable by user. </summary>
    public DateTime SecDateTEntry;

    ///<summary>FK to userod.UseNum. User that added the plan link.</summary>
    public long SecUserNumEntry;

    public OrthoPlanLink Copy()
    {
        return (OrthoPlanLink) MemberwiseClone();
    }
}

public enum OrthoPlanLinkType
{
    OrthoSchedule,

    /// <summary>
    /// Insurance Payment Plan
    /// </summary>
    InsPayPlan,

    /// <summary>
    /// Patient Payment Plan
    /// </summary>
    PatPayPlan
}