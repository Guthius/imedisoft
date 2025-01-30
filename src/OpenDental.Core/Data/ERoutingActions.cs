using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class ERoutingActions
{
    public static List<ERoutingAction> GetListForERouting(long eRoutingNum)
    {
        return ERoutingActionCrud.SelectMany($"SELECT * FROM eroutingaction WHERE ERoutingNum = {eRoutingNum}");
    }
}