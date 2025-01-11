using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DataConnectionBase;
using ODCrypt;
using OpenDentBusiness.Crud;
using OpenDentBusiness.UI;

namespace OpenDentBusiness;

public class ProcNotes
{
    #region Get Methods

    ///<summary>Returns all procedure notes for the procedure passed in.  Returned list is not sorted.</summary>
    public static List<ProcNote> GetProcNotesForProc(long procNum)
    {
        var command = "SELECT * FROM procnote WHERE ProcNum=" + SOut.Long(procNum);
        return ProcNoteCrud.SelectMany(command);
    }

    #endregion

    public static long Insert(ProcNote procNote)
    {
        return ProcNoteCrud.Insert(procNote);
    }

    public static ProcNote GetProcNotesForPat(long patNum, DateTime dateStart, DateTime dateEnd)
    {
        var query = "SELECT procnote.* FROM procnote "
                    + "INNER JOIN procedurelog ON procedurelog.ProcNum=procnote.ProcNum "
                    + "WHERE procnote.PatNum=" + SOut.Long(patNum) + " "
                    + "AND procnote.EntryDateTime BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateEnd) + " "
                    + "AND procedurelog.ProcStatus!=" + SOut.Int((int) ProcStat.D) + " "
                    + "ORDER BY procnote.EntryDateTime DESC";
        var command = DbHelper.LimitOrderBy(query, 1);
        return ProcNoteCrud.SelectOne(command);
    }

    ///<summary>Returns a list of ProcNums from listProcNums where the most recent ProcNote for the proc is signed.</summary>
    public static List<long> GetIsProcNoteSigned(List<long> listProcNums)
    {
        if (listProcNums.Count == 0) return new List<long>();
        var command = "SELECT * FROM procnote WHERE ProcNum IN (" + string.Join(",", listProcNums.Select(x => SOut.Long(x))) + ")";
        var listProcNotes = ProcNoteCrud.SelectMany(command); //get all ProcNotes with ProcNum in the supplied list
        if (listProcNotes.Count == 0) return new List<long>();
        return listProcNotes
            .GroupBy(x => x.ProcNum, (x, y) => y.Aggregate((y1, y2) => y1.EntryDateTime > y2.EntryDateTime ? y1 : y2)) //group by ProcNum, get most recent ProcNote
            .Where(x => !string.IsNullOrWhiteSpace(x.Signature)) //where the most recent ProcNote is signed
            .Select(x => x.ProcNum).ToList(); //return list of ProcNums
    }

    ///<summary>Gets a list of procnotes from the datbase. Used for API.</summary>
    public static List<ProcNote> GetProcNotesForApi(int limit, int offset, long patNum, long procNum)
    {
        var command = "SELECT * FROM procnote "
                      + "WHERE EntryDateTime>=" + SOut.DateT(DateTime.MinValue) + " "; //Needed to use WHERE clause so the rest can be AND.
        if (patNum > 0) command += "AND procnote.PatNum=" + SOut.Long(patNum) + " ";
        if (procNum > 0) command += "AND procnote.ProcNum=" + SOut.Long(procNum) + " ";
        command += "ORDER BY procnotenum DESC " //Ensure order for limit and offset. DESC so the most recent is first, like the UI.
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return ProcNoteCrud.SelectMany(command);
    }

    /// <summary>
    ///     Modifies currentNote and returns the new note string. Also checks PrefName.ProcPromptForAutoNote and remots
    ///     auto notes if needed.
    /// </summary>
    public static string SetProcCompleteNoteHelper(bool isQuickAdd, Procedure procedure, Procedure procedureOld, long provNum, string currentNote = "")
    {
        var procNoteDefault = "";
        if (isQuickAdd)
        {
            //Quick Procs should insert both TP Default Note and C Default Note.
            procNoteDefault = ProcCodeNotes.GetNote(provNum, procedure.CodeNum, ProcStat.TP);
            if (!string.IsNullOrEmpty(procNoteDefault)) procNoteDefault += "\r\n";
        }

        if (procedureOld.ProcStatus != ProcStat.C && procedure.ProcStatus == ProcStat.C)
        {
            //Only append the default note if the procedure changed status to Completed
            procNoteDefault += ProcCodeNotes.GetNote(provNum, procedure.CodeNum, ProcStat.C);
            if (currentNote != "" && procNoteDefault != "") //check to see if a default note is defined.
                currentNote += "\r\n"; //add a new line if there was already a ProcNote on the procedure.
            if (!string.IsNullOrEmpty(procNoteDefault)) currentNote += procNoteDefault;
        }

        if (procedure.ProcStatus == ProcStat.TP && procedureOld.ProcStatus == ProcStat.D)
        {
            //Append the TP note if the user had to enter a tooth number or quadrant
            procNoteDefault += ProcCodeNotes.GetNote(provNum, procedure.CodeNum, ProcStat.TP);
            if (currentNote != "" && procNoteDefault != "") //check to see if a default note is defined.
                currentNote += "\r\n"; //add a new line if there was already a ProcNote on the procedure.
            if (!string.IsNullOrEmpty(procNoteDefault)) currentNote += procNoteDefault;
        }

        if (!PrefC.GetBool(PrefName.ProcPromptForAutoNote))
            //Users do not want to be prompted for auto notes, so remove them all from the procedure note.
            currentNote = Regex.Replace(currentNote, @"\[\[.+?\]\]", "");
        return currentNote;
    }

    ///<summary>Get a single ProcNote from DB, returns null if not found. </summary>
    public static ProcNote GetOneProcNote(long procNoteNum)
    {
        if (procNoteNum == 0) return null;

        var command = "SELECT * FROM procnote "
                      + "WHERE ProcNoteNum = " + SOut.Long(procNoteNum);
        return ProcNoteCrud.SelectOne(command);
    }

    ///<summary>Helper method for xODApi.ProcNotes POST to use digital signature stamp logic.</summary>
    public static ProcNote ProcNoteSignatureForApi(ProcNote odbProcNote, string apiSignatureString)
    {
        //OpenDentBusiness.Procedures.GetSignatureKeyData()
        var keyData = odbProcNote.Note + odbProcNote.UserNum;
        keyData = keyData.Replace("\r\n", "\n"); //We need all newlines to be the same, a mix of \r\n and \n can invalidate the procedure signature.
        //OpenDentBusiness.UI.SignatureBox.SetKeyString()
        var utf8Encoding = new UTF8Encoding();
        //OpenDental.UI.SignatureBoxWrapper.GetSignature()
        var hashNew = utf8Encoding.GetBytes("0000000000000000"); //Set it to "0000000000000000" (16 zeros) to indicate no key string to be used for encryption.
        //OpenDental.UI.SignatureBox.SetAutoKeyData()
        hashNew = MD5.Hash(Encoding.UTF8.GetBytes(keyData));
        //OpenDental.UI.SignatureBox.GetSigString()
        odbProcNote.Signature = SigBox.EncryptSigString(hashNew, apiSignatureString);
        return odbProcNote;
    }

    /*
    
    internal static bool PreviousNoteExists(int procNum){
        string command="SELECT COUNT(*) FROM procnote WHERE ProcNum="+POut.PInt(procNum);
        DataConnection dcon=new DataConnection();
        if(dcon.GetCount(command)=="0"){
            return false;
        }
        return true;
    }*/
}