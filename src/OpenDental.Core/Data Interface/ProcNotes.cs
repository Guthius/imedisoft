using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ProcNotes
{
    public static List<ProcNote> GetProcNotesForProc(long procNum)
    {
        var command = "SELECT * FROM procnote WHERE ProcNum=" + SOut.Long(procNum);
        return ProcNoteCrud.SelectMany(command);
    }

    public static void Insert(ProcNote procNote)
    {
        ProcNoteCrud.Insert(procNote);
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
}