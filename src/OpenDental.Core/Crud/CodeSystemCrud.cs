using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class CodeSystemCrud
{
    public static List<CodeSystem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<CodeSystem> TableToList(DataTable table)
    {
        var retVal = new List<CodeSystem>();
        foreach (DataRow row in table.Rows)
        {
            var codeSystem = new CodeSystem
            {
                CodeSystemNum = SIn.Long(row["CodeSystemNum"].ToString()),
                CodeSystemName = SIn.String(row["CodeSystemName"].ToString()),
                VersionCur = SIn.String(row["VersionCur"].ToString()),
                VersionAvail = SIn.String(row["VersionAvail"].ToString()),
                HL7OID = SIn.String(row["HL7OID"].ToString()),
                Note = SIn.String(row["Note"].ToString())
            };
            retVal.Add(codeSystem);
        }

        return retVal;
    }

    public static void Update(CodeSystem codeSystem)
    {
        var command = "UPDATE codesystem SET "
                      + "CodeSystemName= '" + SOut.String(codeSystem.CodeSystemName) + "', "
                      + "VersionCur    = '" + SOut.String(codeSystem.VersionCur) + "', "
                      + "VersionAvail  = '" + SOut.String(codeSystem.VersionAvail) + "', "
                      + "HL7OID        = '" + SOut.String(codeSystem.HL7OID) + "', "
                      + "Note          = '" + SOut.String(codeSystem.Note) + "' "
                      + "WHERE CodeSystemNum = " + SOut.Long(codeSystem.CodeSystemNum);
        Db.NonQ(command);
    }
}