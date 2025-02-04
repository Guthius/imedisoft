using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class TerminalActives
{
    public static List<TerminalActive> Refresh()
    {
        return TerminalActiveCrud.SelectMany("SELECT * FROM terminalactive ORDER BY ComputerName, SessionName");
    }

    public static TerminalActive GetForCmptrSessionAndId(string computerName, int sessionId, int processId = 0)
    {
        var commandText =
            "SELECT * FROM terminalactive " +
            "WHERE ComputerName = '" + SOut.String(computerName) + "' " +
            "AND SessionId = " + sessionId;

        if (processId > 0)
        {
            commandText += " AND ProcessId = " + processId;
        }

        return TerminalActiveCrud.SelectOne(commandText);
    }

    public static void SetPatNum(long terminalActiveNum, long patNum)
    {
        if (terminalActiveNum < 1)
        {
            return;
        }

        Db.NonQ("UPDATE terminalactive SET PatNum = " + patNum + " WHERE TerminalActiveNum = " + terminalActiveNum);
    }

    public static void Insert(TerminalActive terminalActive)
    {
        TerminalActiveCrud.Insert(terminalActive);
    }

    public static void DeleteForCmptrSessionAndId(string computerName, int sessionId, int processId = 0, int excludeId = 0)
    {
        var commandText = "DELETE FROM terminalactive WHERE ComputerName = '" + SOut.String(computerName) + "' AND SessionId = " + sessionId;

        if (processId > 0)
        {
            commandText += " AND ProcessId = " + processId;
        }

        if (excludeId > 0)
        {
            commandText += " AND ProcessId != " + excludeId;
        }

        Db.NonQ(commandText);
    }

    public static bool PatIsInUse(long patNum)
    {
        var commandText =
            "SELECT COUNT(*) FROM terminalactive " +
            "WHERE PatNum = " + patNum + " AND " +
            "(TerminalStatus = " + (int) TerminalStatusEnum.PatientInfo + " " +
            "OR TerminalStatus = " + (int) TerminalStatusEnum.UpdateOnly + ")";

        return Db.GetCount(commandText) != "0";
    }

    public static bool IsCompClientNameInUse(string computerName, string clientName)
    {
        if (string.IsNullOrWhiteSpace(computerName) || string.IsNullOrWhiteSpace(clientName))
        {
            return true;
        }

        var commandText =
            "SELECT COUNT(*) FROM terminalactive " +
            "WHERE ComputerName = '" + SOut.String(computerName) + "' " +
            "AND SessionName = '" + SOut.String(clientName) + "'";

        return Db.GetCount(commandText) != "0";
    }
}