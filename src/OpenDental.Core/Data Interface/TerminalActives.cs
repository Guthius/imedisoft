using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class TerminalActives
{
	/// <summary>
	///     Gets a list of all TerminalActives.  Used by FormTerminalManager.  Data is retrieved when FormTerminalManager
	///     initially loads or when
	///     signalods of type Kiosk are processed by the form to refill the grid if necessary.
	/// </summary>
	public static List<TerminalActive> Refresh()
    {
        var command = "SELECT * FROM terminalactive ORDER BY ComputerName,SessionName";
        return TerminalActiveCrud.SelectMany(command);
    }

	/// <summary>
	///     DEPRECATED.  Use GetForCompAndSession.  Only kept for FormTerminalOld references, which is never displayed.
	///     Consider deleting.
	/// </summary>
	public static TerminalActive GetTerminal(string computerName)
    {
        var command = "SELECT * FROM terminalactive WHERE ComputerName ='" + SOut.String(computerName) + "'";
        return TerminalActiveCrud.SelectOne(command);
    }

	/// <summary>
	///     Get one TerminalActive from the db with ComputerName, SessionId, and ProcessId.  ComputerName is case-insensitive.
	///     Will return null if not found.
	/// </summary>
	public static TerminalActive GetForCmptrSessionAndId(string computerName, int sessionId, int processId = 0)
    {
        var command = "SELECT * FROM terminalactive "
                      + "WHERE ComputerName='" + SOut.String(computerName) + "' "
                      + "AND SessionId=" + SOut.Int(sessionId);
        if (processId > 0) command += " AND ProcessId=" + SOut.Int(processId);
        return TerminalActiveCrud.SelectOne(command);
    }

    
    public static void Update(TerminalActive terminalActive)
    {
        TerminalActiveCrud.Update(terminalActive);
    }

    /// <summary>
    ///     Used to set the patient for a kiosk, to either load a patient or pass in patNum==0 to clear a patient.  Does
    ///     nothing if termNum is not
    ///     a valid TerminalActiveNum.
    /// </summary>
    public static void SetPatNum(long terminalActiveNum, long patNum)
    {
        if (terminalActiveNum < 1) return; //invalid TerminalActiveNum, just return
        var command = "UPDATE terminalactive SET PatNum=" + SOut.Long(patNum) + " WHERE TerminalActiveNum=" + SOut.Long(terminalActiveNum);
        Db.NonQ(command);
        Signalods.SetInvalid(InvalidType.EClipboard);
    }

    
    public static long Insert(TerminalActive terminalActive)
    {
        return TerminalActiveCrud.Insert(terminalActive);
    }

    ///<summary>DEPRECATED.  Use DeleteForCompAndSession.  Only kept for FormTerminalOld references, which is never displayed.</summary>
    public static void DeleteAllForComputer(string computerName)
    {
        var command = "DELETE FROM terminalactive WHERE ComputerName ='" + SOut.String(computerName) + "'";
        Db.NonQ(command);
    }

    /// <summary>
    ///     This can be used to delete a specific terminalactive by computer name, session ID and process ID, e.g. when the
    ///     terminal window closes
    ///     or when the delete button is pressed in the terminal manager window.  Also used to clear any left over
    ///     terminalactives when starting a terminal
    ///     for a specific computer and session, in which case you can supply a process ID to exclude so the current terminal
    ///     won't be deleted but all
    ///     others for the computer and session will be.  ComputerName is case-insensitive.
    /// </summary>
    public static void DeleteForCmptrSessionAndId(string computerName, int sessionId, int processId = 0, int excludeId = 0)
    {
        var command = "DELETE FROM terminalactive WHERE ComputerName='" + SOut.String(computerName) + "' AND SessionId=" + SOut.Int(sessionId);
        if (processId > 0) command += " AND ProcessId=" + SOut.Int(processId);
        if (excludeId > 0) command += " AND ProcessId!=" + SOut.Int(excludeId);
        Db.NonQ(command);
        Signalods.SetInvalid(InvalidType.EClipboard);
    }

    /// <summary>
    ///     Called whenever user wants to edit patient info.  Not allowed to if patient edit window is open at a terminal.
    ///     Once patient is done
    ///     at terminal, then staff allowed back into patient edit window.
    /// </summary>
    public static bool PatIsInUse(long patNum)
    {
        var command = "SELECT COUNT(*) FROM terminalactive WHERE PatNum=" + SOut.Long(patNum)
                                                                          + " AND (TerminalStatus=" + SOut.Long((int) TerminalStatusEnum.PatientInfo)
                                                                          + " OR TerminalStatus=" + SOut.Long((int) TerminalStatusEnum.UpdateOnly) + ")";
        return Db.GetCount(command) != "0";
    }

    /// <summary>
    ///     Returns true if a terminal is already in the database with ComputerName=compName and ClientName=clientName or if
    ///     either names are null
    ///     or whitespace.  Otherwise false.  Case-insensitive name comparison.  Allow the same ClientName for different
    ///     computers.
    /// </summary>
    public static bool IsCompClientNameInUse(string computerName, string clientName)
    {
        if (string.IsNullOrWhiteSpace(computerName) || string.IsNullOrWhiteSpace(clientName)) return true; //this will prevent them from using blank or null for the client name
        var command = "SELECT COUNT(*) FROM terminalactive "
                      + "WHERE ComputerName='" + SOut.String(computerName) + "' "
                      + "AND SessionName='" + SOut.String(clientName) + "'";
        return Db.GetCount(command) != "0";
    }
}