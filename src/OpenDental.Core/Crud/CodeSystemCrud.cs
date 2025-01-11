using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

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
        CodeSystem codeSystem;
        foreach (DataRow row in table.Rows)
        {
            codeSystem = new CodeSystem();
            codeSystem.CodeSystemNum = SIn.Long(row["CodeSystemNum"].ToString());
            codeSystem.CodeSystemName = SIn.String(row["CodeSystemName"].ToString());
            codeSystem.VersionCur = SIn.String(row["VersionCur"].ToString());
            codeSystem.VersionAvail = SIn.String(row["VersionAvail"].ToString());
            codeSystem.HL7OID = SIn.String(row["HL7OID"].ToString());
            codeSystem.Note = SIn.String(row["Note"].ToString());
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