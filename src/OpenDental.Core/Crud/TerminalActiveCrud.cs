#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TerminalActiveCrud
{
    public static TerminalActive SelectOne(long terminalActiveNum)
    {
        var command = "SELECT * FROM terminalactive "
                      + "WHERE TerminalActiveNum = " + SOut.Long(terminalActiveNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static TerminalActive SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<TerminalActive> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<TerminalActive> TableToList(DataTable table)
    {
        var retVal = new List<TerminalActive>();
        TerminalActive terminalActive;
        foreach (DataRow row in table.Rows)
        {
            terminalActive = new TerminalActive();
            terminalActive.TerminalActiveNum = SIn.Long(row["TerminalActiveNum"].ToString());
            terminalActive.ComputerName = SIn.String(row["ComputerName"].ToString());
            terminalActive.TerminalStatus = (TerminalStatusEnum) SIn.Int(row["TerminalStatus"].ToString());
            terminalActive.PatNum = SIn.Long(row["PatNum"].ToString());
            terminalActive.SessionId = SIn.Int(row["SessionId"].ToString());
            terminalActive.ProcessId = SIn.Int(row["ProcessId"].ToString());
            terminalActive.SessionName = SIn.String(row["SessionName"].ToString());
            retVal.Add(terminalActive);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<TerminalActive> listTerminalActives, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "TerminalActive";
        var table = new DataTable(tableName);
        table.Columns.Add("TerminalActiveNum");
        table.Columns.Add("ComputerName");
        table.Columns.Add("TerminalStatus");
        table.Columns.Add("PatNum");
        table.Columns.Add("SessionId");
        table.Columns.Add("ProcessId");
        table.Columns.Add("SessionName");
        foreach (var terminalActive in listTerminalActives)
            table.Rows.Add(SOut.Long(terminalActive.TerminalActiveNum), terminalActive.ComputerName, SOut.Int((int) terminalActive.TerminalStatus), SOut.Long(terminalActive.PatNum), SOut.Int(terminalActive.SessionId), SOut.Int(terminalActive.ProcessId), terminalActive.SessionName);
        return table;
    }

    public static long Insert(TerminalActive terminalActive)
    {
        return Insert(terminalActive, false);
    }

    public static long Insert(TerminalActive terminalActive, bool useExistingPK)
    {
        var command = "INSERT INTO terminalactive (";

        command += "ComputerName,TerminalStatus,PatNum,SessionId,ProcessId,SessionName) VALUES(";

        command +=
            "'" + SOut.String(terminalActive.ComputerName) + "',"
            + SOut.Int((int) terminalActive.TerminalStatus) + ","
            + SOut.Long(terminalActive.PatNum) + ","
            + SOut.Int(terminalActive.SessionId) + ","
            + SOut.Int(terminalActive.ProcessId) + ","
            + "'" + SOut.String(terminalActive.SessionName) + "')";
        {
            terminalActive.TerminalActiveNum = Db.NonQ(command, true, "TerminalActiveNum", "terminalActive");
        }
        return terminalActive.TerminalActiveNum;
    }

    public static long InsertNoCache(TerminalActive terminalActive)
    {
        return InsertNoCache(terminalActive, false);
    }

    public static long InsertNoCache(TerminalActive terminalActive, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO terminalactive (";
        if (isRandomKeys || useExistingPK) command += "TerminalActiveNum,";
        command += "ComputerName,TerminalStatus,PatNum,SessionId,ProcessId,SessionName) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(terminalActive.TerminalActiveNum) + ",";
        command +=
            "'" + SOut.String(terminalActive.ComputerName) + "',"
            + SOut.Int((int) terminalActive.TerminalStatus) + ","
            + SOut.Long(terminalActive.PatNum) + ","
            + SOut.Int(terminalActive.SessionId) + ","
            + SOut.Int(terminalActive.ProcessId) + ","
            + "'" + SOut.String(terminalActive.SessionName) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            terminalActive.TerminalActiveNum = Db.NonQ(command, true, "TerminalActiveNum", "terminalActive");
        return terminalActive.TerminalActiveNum;
    }

    public static void Update(TerminalActive terminalActive)
    {
        var command = "UPDATE terminalactive SET "
                      + "ComputerName     = '" + SOut.String(terminalActive.ComputerName) + "', "
                      + "TerminalStatus   =  " + SOut.Int((int) terminalActive.TerminalStatus) + ", "
                      + "PatNum           =  " + SOut.Long(terminalActive.PatNum) + ", "
                      + "SessionId        =  " + SOut.Int(terminalActive.SessionId) + ", "
                      + "ProcessId        =  " + SOut.Int(terminalActive.ProcessId) + ", "
                      + "SessionName      = '" + SOut.String(terminalActive.SessionName) + "' "
                      + "WHERE TerminalActiveNum = " + SOut.Long(terminalActive.TerminalActiveNum);
        Db.NonQ(command);
    }

    public static bool Update(TerminalActive terminalActive, TerminalActive oldTerminalActive)
    {
        var command = "";
        if (terminalActive.ComputerName != oldTerminalActive.ComputerName)
        {
            if (command != "") command += ",";
            command += "ComputerName = '" + SOut.String(terminalActive.ComputerName) + "'";
        }

        if (terminalActive.TerminalStatus != oldTerminalActive.TerminalStatus)
        {
            if (command != "") command += ",";
            command += "TerminalStatus = " + SOut.Int((int) terminalActive.TerminalStatus) + "";
        }

        if (terminalActive.PatNum != oldTerminalActive.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(terminalActive.PatNum) + "";
        }

        if (terminalActive.SessionId != oldTerminalActive.SessionId)
        {
            if (command != "") command += ",";
            command += "SessionId = " + SOut.Int(terminalActive.SessionId) + "";
        }

        if (terminalActive.ProcessId != oldTerminalActive.ProcessId)
        {
            if (command != "") command += ",";
            command += "ProcessId = " + SOut.Int(terminalActive.ProcessId) + "";
        }

        if (terminalActive.SessionName != oldTerminalActive.SessionName)
        {
            if (command != "") command += ",";
            command += "SessionName = '" + SOut.String(terminalActive.SessionName) + "'";
        }

        if (command == "") return false;
        command = "UPDATE terminalactive SET " + command
                                               + " WHERE TerminalActiveNum = " + SOut.Long(terminalActive.TerminalActiveNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(TerminalActive terminalActive, TerminalActive oldTerminalActive)
    {
        if (terminalActive.ComputerName != oldTerminalActive.ComputerName) return true;
        if (terminalActive.TerminalStatus != oldTerminalActive.TerminalStatus) return true;
        if (terminalActive.PatNum != oldTerminalActive.PatNum) return true;
        if (terminalActive.SessionId != oldTerminalActive.SessionId) return true;
        if (terminalActive.ProcessId != oldTerminalActive.ProcessId) return true;
        if (terminalActive.SessionName != oldTerminalActive.SessionName) return true;
        return false;
    }

    public static void Delete(long terminalActiveNum)
    {
        var command = "DELETE FROM terminalactive "
                      + "WHERE TerminalActiveNum = " + SOut.Long(terminalActiveNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTerminalActiveNums)
    {
        if (listTerminalActiveNums == null || listTerminalActiveNums.Count == 0) return;
        var command = "DELETE FROM terminalactive "
                      + "WHERE TerminalActiveNum IN(" + string.Join(",", listTerminalActiveNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}