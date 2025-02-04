using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class AccountingAutoPay : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long AccountingAutoPayNum;

    ///<summary>FK to definition.DefNum.</summary>
    public long PayType;

    ///<summary>FK to account.AccountNum.  AccountNums separated by commas.  No spaces.</summary>
    public string PickList;

    public AccountingAutoPay Clone()
    {
        return (AccountingAutoPay) MemberwiseClone();
    }
}