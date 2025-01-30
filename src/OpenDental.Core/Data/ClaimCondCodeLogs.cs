using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class ClaimCondCodeLogs
{
    public static ClaimCondCodeLog GetByClaimNum(long claimNum)
    {
        return ClaimCondCodeLogCrud.SelectOne("SELECT * FROM claimcondcodelog WHERE ClaimNum=" + claimNum);
    }

    public static void Update(ClaimCondCodeLog claimCondCodeLog)
    {
        ClaimCondCodeLogCrud.Update(claimCondCodeLog);
    }

    public static void Insert(ClaimCondCodeLog claimCondCodeLog)
    {
        ClaimCondCodeLogCrud.Insert(claimCondCodeLog);
    }
}