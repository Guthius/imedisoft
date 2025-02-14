using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class StmtLinks
{
    public static List<long> GetForStatementAndType(long statementNum, StmtLinkTypes stmtLinkTypes)
    {
        return Db.GetListLong(
            $"""
             SELECT FKey FROM stmtlink 
             WHERE StatementNum = {statementNum} 
             AND StmtLinkType = {(int) stmtLinkTypes}
             """);
    }

    public static void Insert(StmtLink stmtLink)
    {
        StmtLinkCrud.Insert(stmtLink);
    }

    public static void DetachAllFromStatements(List<long> statementNums)
    {
        if (statementNums is null || statementNums.Count == 0)
        {
            return;
        }

        Db.NonQ($"DELETE FROM stmtlink WHERE StatementNum IN ({string.Join(", ", statementNums)})");
    }
}