using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class TerminalActiveCrud
{
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
        foreach (DataRow row in table.Rows)
        {
            var terminalActive = new TerminalActive
            {
                TerminalActiveNum = SIn.Long(row["TerminalActiveNum"].ToString()),
                ComputerName = SIn.String(row["ComputerName"].ToString()),
                TerminalStatus = (TerminalStatusEnum) SIn.Int(row["TerminalStatus"].ToString()),
                PatNum = SIn.Long(row["PatNum"].ToString()),
                SessionId = SIn.Int(row["SessionId"].ToString()),
                ProcessId = SIn.Int(row["ProcessId"].ToString()),
                SessionName = SIn.String(row["SessionName"].ToString())
            };
            retVal.Add(terminalActive);
        }

        return retVal;
    }

    public static void Insert(TerminalActive terminalActive)
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
    }
}