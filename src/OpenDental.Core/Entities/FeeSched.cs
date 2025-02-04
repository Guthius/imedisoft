using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class FeeSched : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long FeeSchedNum;

    public string Description;
    public FeeScheduleType FeeSchedType;
    public int ItemOrder;

    ///<summary>True if the fee schedule is hidden.  Can't delete fee schedules or change their type once created.</summary>
    public bool IsHidden;

    ///<summary>True if the fee schedule is used globally and linked to the HQ. Localization of the fees is not allowed. ClinicNum and ProvNum must both be zero for all fees.</summary>
    public bool IsGlobal;

    ///<summary>FK to userod.UserNum.  Set to the user logged in when the row was inserted at SecDateEntry date and time.</summary>
    public long SecUserNumEntry;

    ///<summary>Timestamp automatically generated and user not allowed to change.  The actual date of entry.</summary>
    public DateTime SecDateEntry;

    ///<summary>Automatically updated by MySQL every time a row is added or changed. Could be changed due to user editing, custom queries or program
    ///updates.  Not user editable with the UI.</summary>
    public DateTime SecDateTEdit;

    public FeeSched Copy()
    {
        return (FeeSched) MemberwiseClone();
    }
}

public enum FeeScheduleType
{
    Normal,
    CoPay,
    OutNetwork,
    FixedBenefit,
    ManualBlueBook
}