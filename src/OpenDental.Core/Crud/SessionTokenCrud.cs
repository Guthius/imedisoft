#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SessionTokenCrud
{
    public static SessionToken SelectOne(long sessionTokenNum)
    {
        var command = "SELECT * FROM sessiontoken "
                      + "WHERE SessionTokenNum = " + SOut.Long(sessionTokenNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static SessionToken SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<SessionToken> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<SessionToken> TableToList(DataTable table)
    {
        var retVal = new List<SessionToken>();
        SessionToken sessionToken;
        foreach (DataRow row in table.Rows)
        {
            sessionToken = new SessionToken();
            sessionToken.SessionTokenNum = SIn.Long(row["SessionTokenNum"].ToString());
            sessionToken.SessionTokenHash = SIn.String(row["SessionTokenHash"].ToString());
            sessionToken.Expiration = SIn.DateTime(row["Expiration"].ToString());
            sessionToken.TokenType = (SessionTokenType) SIn.Int(row["TokenType"].ToString());
            sessionToken.FKey = SIn.Long(row["FKey"].ToString());
            retVal.Add(sessionToken);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<SessionToken> listSessionTokens, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "SessionToken";
        var table = new DataTable(tableName);
        table.Columns.Add("SessionTokenNum");
        table.Columns.Add("SessionTokenHash");
        table.Columns.Add("Expiration");
        table.Columns.Add("TokenType");
        table.Columns.Add("FKey");
        foreach (var sessionToken in listSessionTokens)
            table.Rows.Add(SOut.Long(sessionToken.SessionTokenNum), sessionToken.SessionTokenHash, SOut.DateT(sessionToken.Expiration, false), SOut.Int((int) sessionToken.TokenType), SOut.Long(sessionToken.FKey));
        return table;
    }

    public static long Insert(SessionToken sessionToken)
    {
        return Insert(sessionToken, false);
    }

    public static long Insert(SessionToken sessionToken, bool useExistingPK)
    {
        var command = "INSERT INTO sessiontoken (";

        command += "SessionTokenHash,Expiration,TokenType,FKey) VALUES(";

        command +=
            "'" + SOut.String(sessionToken.SessionTokenHash) + "',"
            + SOut.DateT(sessionToken.Expiration) + ","
            + SOut.Int((int) sessionToken.TokenType) + ","
            + SOut.Long(sessionToken.FKey) + ")";
        {
            sessionToken.SessionTokenNum = Db.NonQ(command, true, "SessionTokenNum", "sessionToken");
        }
        return sessionToken.SessionTokenNum;
    }

    public static long InsertNoCache(SessionToken sessionToken)
    {
        return InsertNoCache(sessionToken, false);
    }

    public static long InsertNoCache(SessionToken sessionToken, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO sessiontoken (";
        if (isRandomKeys || useExistingPK) command += "SessionTokenNum,";
        command += "SessionTokenHash,Expiration,TokenType,FKey) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(sessionToken.SessionTokenNum) + ",";
        command +=
            "'" + SOut.String(sessionToken.SessionTokenHash) + "',"
            + SOut.DateT(sessionToken.Expiration) + ","
            + SOut.Int((int) sessionToken.TokenType) + ","
            + SOut.Long(sessionToken.FKey) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            sessionToken.SessionTokenNum = Db.NonQ(command, true, "SessionTokenNum", "sessionToken");
        return sessionToken.SessionTokenNum;
    }

    public static void Update(SessionToken sessionToken)
    {
        var command = "UPDATE sessiontoken SET "
                      + "SessionTokenHash= '" + SOut.String(sessionToken.SessionTokenHash) + "', "
                      + "Expiration      =  " + SOut.DateT(sessionToken.Expiration) + ", "
                      + "TokenType       =  " + SOut.Int((int) sessionToken.TokenType) + ", "
                      + "FKey            =  " + SOut.Long(sessionToken.FKey) + " "
                      + "WHERE SessionTokenNum = " + SOut.Long(sessionToken.SessionTokenNum);
        Db.NonQ(command);
    }

    public static bool Update(SessionToken sessionToken, SessionToken oldSessionToken)
    {
        var command = "";
        if (sessionToken.SessionTokenHash != oldSessionToken.SessionTokenHash)
        {
            if (command != "") command += ",";
            command += "SessionTokenHash = '" + SOut.String(sessionToken.SessionTokenHash) + "'";
        }

        if (sessionToken.Expiration != oldSessionToken.Expiration)
        {
            if (command != "") command += ",";
            command += "Expiration = " + SOut.DateT(sessionToken.Expiration) + "";
        }

        if (sessionToken.TokenType != oldSessionToken.TokenType)
        {
            if (command != "") command += ",";
            command += "TokenType = " + SOut.Int((int) sessionToken.TokenType) + "";
        }

        if (sessionToken.FKey != oldSessionToken.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(sessionToken.FKey) + "";
        }

        if (command == "") return false;
        command = "UPDATE sessiontoken SET " + command
                                             + " WHERE SessionTokenNum = " + SOut.Long(sessionToken.SessionTokenNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(SessionToken sessionToken, SessionToken oldSessionToken)
    {
        if (sessionToken.SessionTokenHash != oldSessionToken.SessionTokenHash) return true;
        if (sessionToken.Expiration != oldSessionToken.Expiration) return true;
        if (sessionToken.TokenType != oldSessionToken.TokenType) return true;
        if (sessionToken.FKey != oldSessionToken.FKey) return true;
        return false;
    }

    public static void Delete(long sessionTokenNum)
    {
        var command = "DELETE FROM sessiontoken "
                      + "WHERE SessionTokenNum = " + SOut.Long(sessionTokenNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSessionTokenNums)
    {
        if (listSessionTokenNums == null || listSessionTokenNums.Count == 0) return;
        var command = "DELETE FROM sessiontoken "
                      + "WHERE SessionTokenNum IN(" + string.Join(",", listSessionTokenNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}