using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class InsVerifyHist : TableBase
{
    public long InsVerifyHistNum;

    ///<summary>FK to userod.UserNum.  User that was logged on when row was inserted.</summary>
    public long VerifyUserNum;

    #region Copies of InsVerify Fields

    ///<summary>Copied from InsVerify.</summary>
    public long InsVerifyNum;

    ///<summary>Copied from InsVerify.</summary>
    public DateTime DateLastVerified;

    ///<summary>Copied from InsVerify.</summary>
    public long UserNum;

    ///<summary>Copied from InsVerify.</summary>
    public VerifyTypes VerifyType;

    ///<summary>Copied from InsVerify.</summary>
    public long FKey;

    ///<summary>Copied from InsVerify.</summary>
    public long DefNum;

    ///<summary>Copied from InsVerify.</summary>
    public string Note;

    ///<summary>Copied from InsVerify.</summary>
    public DateTime DateLastAssigned;

    ///<summary>Copied from InsVerify.</summary>
    public DateTime DateTimeEntry;

    ///<summary>Copied from InsVerify.</summary>
    public double HoursAvailableForVerification;

    ///<summary>Not copied from Task. Automatically updated by MySQL every time a row is added or changed.</summary>
    public DateTime SecDateTEdit;

    #endregion Copies of InsVerify Fields

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

    public InsVerifyHist()
    {
    }

    public InsVerifyHist(InsVerify insVerify)
    {
        VerifyUserNum = Security.CurUser.UserNum;
        InsVerifyNum = insVerify.InsVerifyNum;
        DateLastVerified = insVerify.DateLastVerified;
        UserNum = insVerify.UserNum;
        VerifyType = insVerify.VerifyType;
        FKey = insVerify.FKey;
        DefNum = insVerify.DefNum;
        Note = insVerify.Note;
        DateLastAssigned = insVerify.DateLastAssigned;
        DateTimeEntry = insVerify.DateTimeEntry;
        HoursAvailableForVerification = insVerify.HoursAvailableForVerification;
        SecDateTEdit = insVerify.SecDateTEdit;
    }
}