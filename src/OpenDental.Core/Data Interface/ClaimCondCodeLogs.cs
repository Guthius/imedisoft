using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class ClaimCondCodeLogs
{
    /// <summary>Will be null if this claim has no condition codes.</summary>
    public static ClaimCondCodeLog GetByClaimNum(long claimNum)
    {
        var command = "SELECT * FROM claimcondcodelog WHERE ClaimNum=" + SOut.Long(claimNum);
        return ClaimCondCodeLogCrud.SelectOne(command);
    }

    public static void Update(ClaimCondCodeLog claimCondCodeLog)
    {
        ClaimCondCodeLogCrud.Update(claimCondCodeLog);
    }

    public static long Insert(ClaimCondCodeLog claimCondCodeLog)
    {
        return ClaimCondCodeLogCrud.Insert(claimCondCodeLog);
    }
}