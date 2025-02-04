using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class FamAgings
{
    public static void InsertMany(List<FamAging> famAgings)
    {
        FamAgingCrud.InsertMany(famAgings, true);
    }
}