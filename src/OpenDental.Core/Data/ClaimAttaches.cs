using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class ClaimAttaches
{
    public static void Insert(ClaimAttach claimAttach)
    {
        ClaimAttachCrud.Insert(claimAttach);
    }
}