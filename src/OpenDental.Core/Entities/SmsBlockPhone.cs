using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class SmsBlockPhone : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long SmsBlockPhoneNum;

    ///<summary>The phone number to be blocked.</summary>
    public string BlockWirelessNumber;
}