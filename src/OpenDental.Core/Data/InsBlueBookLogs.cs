using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class InsBlueBookLogs
{
    public static void Insert(InsBlueBookLog insBlueBookLog)
    {
        InsBlueBookLogCrud.Insert(insBlueBookLog);
    }

    public static List<InsBlueBookLog> GetAllByClaimProcNum(long claimProcNum)
    {
        return InsBlueBookLogCrud.SelectMany("SELECT * FROM insbluebooklog WHERE ClaimProcNum = " + claimProcNum);
    }

    public static InsBlueBookLog GetMostRecentForClaimProc(long claimProcNum)
    {
        return InsBlueBookLogCrud.SelectOne(
            $"""
             SELECT * FROM insbluebooklog
             WHERE ClaimProcNum={claimProcNum}
             ORDER BY insbluebooklog.DateTEntry DESC
             LIMIT 1
             """);
    }
}