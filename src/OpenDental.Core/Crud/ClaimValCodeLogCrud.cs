using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class ClaimValCodeLogCrud
{
    public static List<ClaimValCodeLog> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ClaimValCodeLog> TableToList(DataTable table)
    {
        var retVal = new List<ClaimValCodeLog>();
        ClaimValCodeLog claimValCodeLog;
        foreach (DataRow row in table.Rows)
        {
            claimValCodeLog = new ClaimValCodeLog();
            claimValCodeLog.ClaimValCodeLogNum = SIn.Long(row["ClaimValCodeLogNum"].ToString());
            claimValCodeLog.ClaimNum = SIn.Long(row["ClaimNum"].ToString());
            claimValCodeLog.ClaimField = SIn.String(row["ClaimField"].ToString());
            claimValCodeLog.ValCode = SIn.String(row["ValCode"].ToString());
            claimValCodeLog.ValAmount = SIn.Double(row["ValAmount"].ToString());
            claimValCodeLog.Ordinal = SIn.Int(row["Ordinal"].ToString());
            retVal.Add(claimValCodeLog);
        }

        return retVal;
    }

    public static void Insert(ClaimValCodeLog claimValCodeLog)
    {
        var command = "INSERT INTO claimvalcodelog (";

        command += "ClaimNum,ClaimField,ValCode,ValAmount,Ordinal) VALUES(";

        command +=
            SOut.Long(claimValCodeLog.ClaimNum) + ","
                                                + "'" + SOut.String(claimValCodeLog.ClaimField) + "',"
                                                + "'" + SOut.String(claimValCodeLog.ValCode) + "',"
                                                + SOut.Double(claimValCodeLog.ValAmount) + ","
                                                + SOut.Int(claimValCodeLog.Ordinal) + ")";
        {
            claimValCodeLog.ClaimValCodeLogNum = Db.NonQ(command, true, "ClaimValCodeLogNum", "claimValCodeLog");
        }
    }

    public static void Update(ClaimValCodeLog claimValCodeLog)
    {
        var command = "UPDATE claimvalcodelog SET "
                      + "ClaimNum          =  " + SOut.Long(claimValCodeLog.ClaimNum) + ", "
                      + "ClaimField        = '" + SOut.String(claimValCodeLog.ClaimField) + "', "
                      + "ValCode           = '" + SOut.String(claimValCodeLog.ValCode) + "', "
                      + "ValAmount         =  " + SOut.Double(claimValCodeLog.ValAmount) + ", "
                      + "Ordinal           =  " + SOut.Int(claimValCodeLog.Ordinal) + " "
                      + "WHERE ClaimValCodeLogNum = " + SOut.Long(claimValCodeLog.ClaimValCodeLogNum);
        Db.NonQ(command);
    }
}