using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class HL7ProcAttaches
{
    public static void Insert(HL7ProcAttach hL7ProcAttach)
    {
        HL7ProcAttachCrud.Insert(hL7ProcAttach);
    }
}