using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class HieQueue : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long HieQueueNum;

    public long PatNum;

    public HieQueue()
    {
    }

    public HieQueue(long patNum)
    {
        PatNum = patNum;
    }
}