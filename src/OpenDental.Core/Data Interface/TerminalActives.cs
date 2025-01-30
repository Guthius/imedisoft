using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class TerminalActives
{
    public static List<TerminalActive> Refresh()
    {
        var command = "SELECT * FROM terminalactive ORDER BY ComputerName,SessionName";
        return TerminalActiveCrud.SelectMany(command);
    }

    public static TerminalActive GetForCmptrSessionAndId(string computerName, int sessionId, int processId = 0)
    {
        var command = "SELECT * FROM terminalactive "
                      + "WHERE ComputerName='" + SOut.String(computerName) + "' "
                      + "AND SessionId=" + SOut.Int(sessionId);
        if (processId > 0) command += " AND ProcessId=" + SOut.Int(processId);
        return TerminalActiveCrud.SelectOne(command);
    }

    public static void SetPatNum(long terminalActiveNum, long patNum)
    {
        if (terminalActiveNum < 1) return; //invalid TerminalActiveNum, just return
        var command = "UPDATE terminalactive SET PatNum=" + SOut.Long(patNum) + " WHERE TerminalActiveNum=" + SOut.Long(terminalActiveNum);
        Db.NonQ(command);
        Signalods.SetInvalid(InvalidType.EClipboard);
    }
    
    public static void Insert(TerminalActive terminalActive)
    {
        TerminalActiveCrud.Insert(terminalActive);
    }

    public static void DeleteForCmptrSessionAndId(string computerName, int sessionId, int processId = 0, int excludeId = 0)
    {
        var command = "DELETE FROM terminalactive WHERE ComputerName='" + SOut.String(computerName) + "' AND SessionId=" + SOut.Int(sessionId);
        if (processId > 0) command += " AND ProcessId=" + SOut.Int(processId);
        if (excludeId > 0) command += " AND ProcessId!=" + SOut.Int(excludeId);
        Db.NonQ(command);
        Signalods.SetInvalid(InvalidType.EClipboard);
    }

    public static bool PatIsInUse(long patNum)
    {
        var command = "SELECT COUNT(*) FROM terminalactive WHERE PatNum=" + SOut.Long(patNum)
                                                                          + " AND (TerminalStatus=" + SOut.Long((int) TerminalStatusEnum.PatientInfo)
                                                                          + " OR TerminalStatus=" + SOut.Long((int) TerminalStatusEnum.UpdateOnly) + ")";
        return Db.GetCount(command) != "0";
    }

    public static bool IsCompClientNameInUse(string computerName, string clientName)
    {
        if (string.IsNullOrWhiteSpace(computerName) || string.IsNullOrWhiteSpace(clientName)) return true; //this will prevent them from using blank or null for the client name
        var command = "SELECT COUNT(*) FROM terminalactive "
                      + "WHERE ComputerName='" + SOut.String(computerName) + "' "
                      + "AND SessionName='" + SOut.String(clientName) + "'";
        return Db.GetCount(command) != "0";
    }
}