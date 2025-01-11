using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class ClaimAttaches
{
    public static long Insert(ClaimAttach claimAttach)
    {
        return ClaimAttachCrud.Insert(claimAttach);
    }
}