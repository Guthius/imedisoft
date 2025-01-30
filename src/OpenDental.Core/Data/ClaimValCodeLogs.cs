using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class ClaimValCodeLogs
{
    public static double GetValAmountTotal(long claimNum, string valCode)
    {
        return SIn.Double(DataCore.GetScalar("SELECT SUM(ValAmount) FROM claimvalcodelog WHERE ClaimNum=" + claimNum + " AND ValCode='" + SOut.String(valCode) + "'"));
    }

    public static List<ClaimValCodeLog> GetForClaim(long claimNum)
    {
        return ClaimValCodeLogCrud.SelectMany("SELECT * FROM claimvalcodelog WHERE ClaimNum=" + claimNum);
    }

    public static void UpdateList(List<ClaimValCodeLog> claimValCodeLogs)
    {
        foreach (var claimValCodeLog in claimValCodeLogs)
        {
            if (claimValCodeLog.ClaimValCodeLogNum == 0)
            {
                ClaimValCodeLogCrud.Insert(claimValCodeLog);
            }
            else
            {
                ClaimValCodeLogCrud.Update(claimValCodeLog);
            }
        }
    }
}