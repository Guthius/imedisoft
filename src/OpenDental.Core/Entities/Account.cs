using System.Drawing;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Account : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long AccountNum;

    public string Description;
    public AccountType AcctType;

    ///<summary>For asset accounts, this would be the bank account number for deposit slips.</summary>
    public string BankNumber;

    public bool Inactive;
    public Color AccountColor;

    ///<summary>This will be set true for exactly one account, and it can't be changed.  On the Balance Sheet report, this special account will also contain the sum of all expenses and income for all previous years.</summary>
    public bool IsRetainedEarnings;

    public Account Clone()
    {
        return (Account) MemberwiseClone();
    }
}