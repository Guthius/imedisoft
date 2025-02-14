using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ProcNotes
{
    public static List<ProcNote> GetProcNotesForProc(long procNum)
    {
        return ProcNoteCrud.SelectMany("SELECT * FROM procnote WHERE ProcNum = " + procNum);
    }

    public static void Insert(ProcNote procNote)
    {
        ProcNoteCrud.Insert(procNote);
    }

    public static List<long> GetIsProcNoteSigned(List<long> listProcNums)
    {
        if (listProcNums.Count == 0)
        {
            return [];
        }
        
        var command = "SELECT * FROM procnote WHERE ProcNum IN (" + string.Join(", ", listProcNums.Select(x => x)) + ")";
        var listProcNotes = ProcNoteCrud.SelectMany(command);
        if (listProcNotes.Count == 0)
        {
            return [];
        }
        
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