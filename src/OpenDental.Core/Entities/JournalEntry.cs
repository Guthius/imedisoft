using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class JournalEntry : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long JournalEntryNum;

    ///<summary>FK to transaction.TransactionNum</summary>
    public long TransactionNum;

    ///<summary>FK to account.AccountNum</summary>
    public long AccountNum;

    ///<summary>Always the same for all journal entries within one transaction.</summary>
    public DateTime DateDisplayed;

    ///<summary>Negative numbers never allowed.</summary>
    public double DebitAmt;

    ///<summary>Negative numbers never allowed.</summary>
    public double CreditAmt;
    
    public string Memo;

    ///<summary>A human-readable description of the splits.  Used only for display purposes.</summary>
    public string Splits;

    ///<summary>Any user-defined string.  Usually a check number, but can also be D for deposit, Adj, etc.</summary>
    public string CheckNumber;

    ///<summary>FK to reconcile.ReconcileNum. 0 if not attached to a reconcile. Not allowed to alter amounts if attached.</summary>
    public long ReconcileNum;

    ///<summary>FK to userod.UserNum. The user who created this journal entry.</summary>
    public long SecUserNumEntry;

    ///<summary>The date and time that this journal entry was created.</summary>
    public DateTime SecDateTEntry;

    ///<summary>FK to userod.UserNum. The user who last edited this journal entry.</summary>
    public long SecUserNumEdit;

    ///<summary>The last time this journal entry was edited.</summary>
    public DateTime SecDateTEdit;

    [CrudColumn(IsNotDbColumn = true)]
    public string DescriptionAccount = "";

    public JournalEntry Copy()
    {
        return (JournalEntry) MemberwiseClone();
    }

    public override string ToString()
    {
        var str = "";
        if (DebitAmt != 0)
        {
            str += "Deb:" + DebitAmt.ToString("f2");
        }

        if (CreditAmt != 0)
        {
            str += "Cred:" + CreditAmt.ToString("f2");
        }

        str += ", " + Memo;
        return str;
    }
}