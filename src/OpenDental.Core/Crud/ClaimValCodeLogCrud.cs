using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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
        foreach (DataRow row in table.Rows)
        {
            var claimValCodeLog = new ClaimValCodeLog
            {
                ClaimValCodeLogNum = SIn.Long(row["ClaimValCodeLogNum"].ToString()),
                ClaimNum = SIn.Long(row["ClaimNum"].ToString()),
                ClaimField = SIn.String(row["ClaimField"].ToString()),
                ValCode = SIn.String(row["ValCode"].ToString()),
                ValAmount = SIn.Double(row["ValAmount"].ToString()),
                Ordinal = SIn.Int(row["Ordinal"].ToString())
            };
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