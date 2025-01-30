using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class StmtLinks
{
    public static List<long> GetForStatementAndType(long statementNum, StmtLinkTypes stmtLinkTypes)
    {
        var command = "SELECT FKey FROM stmtlink "
                      + "WHERE StatementNum=" + SOut.Long(statementNum) + " "
                      + "AND StmtLinkType=" + SOut.Int((int) stmtLinkTypes);
        return Db.GetListLong(command);
    }

    public static void Insert(StmtLink stmtLink)
    {
        StmtLinkCrud.Insert(stmtLink);
    }

    public static void DetachAllFromStatements(List<long> listStatementNums)
    {
        if (listStatementNums == null || listStatementNums.Count == 0) return;
        var command = DbHelper.WhereIn("DELETE FROM stmtlink WHERE StatementNum IN ({0})", false, listStatementNums.Select(x => SOut.Long(x)).ToList());
        Db.NonQ(command);
    }
}