using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class EmailMessageUid : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long EmailMessageUidNum;

    public string MsgId;
    public string RecipientAddress;
}