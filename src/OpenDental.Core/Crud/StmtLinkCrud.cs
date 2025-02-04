using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class StmtLinkCrud
{
    public static void Insert(StmtLink stmtLink)
    {
        var command = "INSERT INTO stmtlink (";

        command += "StatementNum,StmtLinkType,FKey) VALUES(";
        command += SOut.Long(stmtLink.StatementNum) + "," + SOut.Int((int) stmtLink.StmtLinkType) + "," + SOut.Long(stmtLink.FKey) + ")";

        stmtLink.StmtLinkNum = Db.NonQ(command, true, "StmtLinkNum", "stmtLink");
    }
}