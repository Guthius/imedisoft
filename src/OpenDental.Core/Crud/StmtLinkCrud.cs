#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class StmtLinkCrud
{
    public static StmtLink SelectOne(long stmtLinkNum)
    {
        var command = "SELECT * FROM stmtlink "
                      + "WHERE StmtLinkNum = " + SOut.Long(stmtLinkNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static StmtLink SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<StmtLink> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<StmtLink> TableToList(DataTable table)
    {
        var retVal = new List<StmtLink>();
        StmtLink stmtLink;
        foreach (DataRow row in table.Rows)
        {
            stmtLink = new StmtLink();
            stmtLink.StmtLinkNum = SIn.Long(row["StmtLinkNum"].ToString());
            stmtLink.StatementNum = SIn.Long(row["StatementNum"].ToString());
            stmtLink.StmtLinkType = (StmtLinkTypes) SIn.Int(row["StmtLinkType"].ToString());
            stmtLink.FKey = SIn.Long(row["FKey"].ToString());
            retVal.Add(stmtLink);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<StmtLink> listStmtLinks, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "StmtLink";
        var table = new DataTable(tableName);
        table.Columns.Add("StmtLinkNum");
        table.Columns.Add("StatementNum");
        table.Columns.Add("StmtLinkType");
        table.Columns.Add("FKey");
        foreach (var stmtLink in listStmtLinks)
            table.Rows.Add(SOut.Long(stmtLink.StmtLinkNum), SOut.Long(stmtLink.StatementNum), SOut.Int((int) stmtLink.StmtLinkType), SOut.Long(stmtLink.FKey));
        return table;
    }

    public static long Insert(StmtLink stmtLink)
    {
        return Insert(stmtLink, false);
    }

    public static long Insert(StmtLink stmtLink, bool useExistingPK)
    {
        var command = "INSERT INTO stmtlink (";

        command += "StatementNum,StmtLinkType,FKey) VALUES(";

        command +=
            SOut.Long(stmtLink.StatementNum) + ","
                                             + SOut.Int((int) stmtLink.StmtLinkType) + ","
                                             + SOut.Long(stmtLink.FKey) + ")";
        {
            stmtLink.StmtLinkNum = Db.NonQ(command, true, "StmtLinkNum", "stmtLink");
        }
        return stmtLink.StmtLinkNum;
    }

    public static long InsertNoCache(StmtLink stmtLink)
    {
        return InsertNoCache(stmtLink, false);
    }

    public static long InsertNoCache(StmtLink stmtLink, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO stmtlink (";
        if (isRandomKeys || useExistingPK) command += "StmtLinkNum,";
        command += "StatementNum,StmtLinkType,FKey) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(stmtLink.StmtLinkNum) + ",";
        command +=
            SOut.Long(stmtLink.StatementNum) + ","
                                             + SOut.Int((int) stmtLink.StmtLinkType) + ","
                                             + SOut.Long(stmtLink.FKey) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            stmtLink.StmtLinkNum = Db.NonQ(command, true, "StmtLinkNum", "stmtLink");
        return stmtLink.StmtLinkNum;
    }

    public static void Update(StmtLink stmtLink)
    {
        var command = "UPDATE stmtlink SET "
                      + "StatementNum=  " + SOut.Long(stmtLink.StatementNum) + ", "
                      + "StmtLinkType=  " + SOut.Int((int) stmtLink.StmtLinkType) + ", "
                      + "FKey        =  " + SOut.Long(stmtLink.FKey) + " "
                      + "WHERE StmtLinkNum = " + SOut.Long(stmtLink.StmtLinkNum);
        Db.NonQ(command);
    }

    public static bool Update(StmtLink stmtLink, StmtLink oldStmtLink)
    {
        var command = "";
        if (stmtLink.StatementNum != oldStmtLink.StatementNum)
        {
            if (command != "") command += ",";
            command += "StatementNum = " + SOut.Long(stmtLink.StatementNum) + "";
        }

        if (stmtLink.StmtLinkType != oldStmtLink.StmtLinkType)
        {
            if (command != "") command += ",";
            command += "StmtLinkType = " + SOut.Int((int) stmtLink.StmtLinkType) + "";
        }

        if (stmtLink.FKey != oldStmtLink.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(stmtLink.FKey) + "";
        }

        if (command == "") return false;
        command = "UPDATE stmtlink SET " + command
                                         + " WHERE StmtLinkNum = " + SOut.Long(stmtLink.StmtLinkNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(StmtLink stmtLink, StmtLink oldStmtLink)
    {
        if (stmtLink.StatementNum != oldStmtLink.StatementNum) return true;
        if (stmtLink.StmtLinkType != oldStmtLink.StmtLinkType) return true;
        if (stmtLink.FKey != oldStmtLink.FKey) return true;
        return false;
    }

    public static void Delete(long stmtLinkNum)
    {
        var command = "DELETE FROM stmtlink "
                      + "WHERE StmtLinkNum = " + SOut.Long(stmtLinkNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listStmtLinkNums)
    {
        if (listStmtLinkNums == null || listStmtLinkNums.Count == 0) return;
        var command = "DELETE FROM stmtlink "
                      + "WHERE StmtLinkNum IN(" + string.Join(",", listStmtLinkNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}