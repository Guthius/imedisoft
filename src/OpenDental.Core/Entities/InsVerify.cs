using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class InsVerify : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long InsVerifyNum;

    ///<summary>The date of the last successful verification.</summary>
    public DateTime DateLastVerified;

    ///<summary>FK to userod.UserNum. Typically 0. There is an optional feature that lets an office "assign" users to a verification so that they can split the load of verifying between different users.</summary>
    public long UserNum;

    ///<summary>Enum:VerifyTypes either InsuranceBenefits or PatientEnrollment</summary>
    public VerifyTypes VerifyType;

    ///<summary>Foreign key either insplan.PlanNum or patplan.PatPlanNum.</summary>
    public long FKey;

    ///<summary>FK to definition.DefNum.  Links to the category InsVerifyStatus</summary>
    public long DefNum;

    ///<summary>DateTime of either the last time this verification was assigned or the last time a status/note was set.</summary>
    public DateTime DateLastAssigned;

    public string Note;

    ///<summary>DateTime the row was added.</summary>
    public DateTime DateTimeEntry;

    ///<summary>Number of hours that were available from the time the insurance needed verified to the date of the appointment.
    ///Includes minutes if applicable.</summary>
    public double HoursAvailableForVerification;

    ///<summary>Automatically updated by MySQL every time a row is added or changed.</summary>
    public DateTime SecDateTEdit;

    #region Not Db Columns

    [CrudColumn(IsNotDbColumn = true)]
    public long PatNum;

    [CrudColumn(IsNotDbColumn = true)]
    public long PlanNum;

    [CrudColumn(IsNotDbColumn = true)]
    public long PatPlanNum;

    [CrudColumn(IsNotDbColumn = true)]
    public string ClinicName;

    [CrudColumn(IsNotDbColumn = true)]
    public string PatientName;

    [CrudColumn(IsNotDbColumn = true)]
    public string CarrierName;

    [CrudColumn(IsNotDbColumn = true)]
    public DateTime AppointmentDateTime;

    [CrudColumn(IsNotDbColumn = true)]
    public long AptNum;

    [CrudColumn(IsNotDbColumn = true)]
    public long ClinicNum;

    [CrudColumn(IsNotDbColumn = true)]
    public long InsSubNum;

    [CrudColumn(IsNotDbColumn = true)]
    public long CarrierNum;

    #endregion Not Db Columns

    public InsVerify Clone()
    {
        return (InsVerify) MemberwiseClone();
    }
}

public class InsVerifyGridObject
{
    public InsVerify InsVerifyPat;
    public InsVerify InsVerifyPlan;
    public bool IsForMedicaidPlan;

    public long GetPatPlanNum()
    {
        if (InsVerifyPat != null)
        {
            return InsVerifyPat.PatPlanNum;
        }

        return InsVerifyPlan?.PatPlanNum ?? 0;
    }

    public long GetPatNum()
    {
        if (InsVerifyPat != null)
        {
            return InsVerifyPat.PatNum;
        }

        return InsVerifyPlan?.PatNum ?? 0;
    }

    public bool IsPatAndInsRow()
    {
        return InsVerifyPlan != null && InsVerifyPat != null;
    }

    public bool IsOnlyPatRow()
    {
        return InsVerifyPlan == null && InsVerifyPat != null;
    }

    public bool IsOnlyInsRow()
    {
        return InsVerifyPlan != null && InsVerifyPat == null;
    }
}

public enum VerifyTypes
{
    ///<summary>0.  This means FKey should be 0.</summary>
    None,

    ///<summary>1.  This means FKey will link to insplan.PlanNum</summary>
    InsuranceBenefit,

    ///<summary>2.  This means FKey will link to patplan.PatPlanNum</summary>
    PatientEnrollment
}