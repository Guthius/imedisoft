using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ClaimPayment : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ClaimPaymentNum;

    ///<summary>Date the check was entered into this system, not the date on the check.</summary>
    public DateTime CheckDate;

    ///<summary>The amount of the check.</summary>
    public double CheckAmt;

    ///<summary>The check number.</summary>
    public string CheckNum;

    ///<summary>Bank and branch.</summary>
    public string BankBranch;

    public string Note;

    ///<summary>FK to clinic.ClinicNum.  0 if no clinic (unassigned).</summary>
    public long ClinicNum;

    ///<summary>FK to deposit.DepositNum.  0 if not attached to any deposits.</summary>
    public long DepositNum;

    ///<summary>Descriptive name of the carrier just for reporting purposes.  We use this because the CarrierNums could conceivably be different for the different claimprocs attached.</summary>
    public string CarrierName;

    ///<summary>Date that the carrier issued the check. Date on the check.</summary>
    public DateTime DateIssued;

    public bool IsPartial;

    ///<summary>FK to definition.DefNum.  0 if not attached to any definitions</summary>
    public long PayType;

    ///<summary>FK to userod.UserNum.  Set to the user logged in when the row was inserted at SecDateEntry date and time.</summary>
    public long SecUserNumEntry;

    ///<summary>Timestamp automatically generated and user not allowed to change.  The actual date of entry.</summary>
    public DateTime SecDateEntry;

    ///<summary>Automatically updated by MySQL every time a row is added or changed. Could be changed due to user editing, custom queries or program
    ///updates.  Not user editable with the UI.</summary>
    public DateTime SecDateTEdit;

    ///<summary>FK to definition.DefNum.  The payment group for this claim payment.</summary>
    public long PayGroup;

    public ClaimPayment Copy()
    {
        return (ClaimPayment) MemberwiseClone();
    }
}