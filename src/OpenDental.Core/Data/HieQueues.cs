using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class HieQueues
{
    public static void Insert(HieQueue hieQueue)
    {
        HieQueueCrud.Insert(hieQueue);
    }
}