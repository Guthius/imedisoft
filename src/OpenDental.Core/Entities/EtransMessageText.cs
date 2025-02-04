using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class EtransMessageText : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long EtransMessageTextNum;
    
    public string MessageText;
}