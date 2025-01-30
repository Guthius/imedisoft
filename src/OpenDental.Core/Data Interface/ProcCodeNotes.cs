using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ProcCodeNotes
{
    public static List<ProcCodeNote> GetList(long codeNum)
    {
        var command = "SELECT * FROM proccodenote WHERE CodeNum=" + SOut.Long(codeNum);
        return ProcCodeNoteCrud.SelectMany(command);
    }

    public static void Insert(ProcCodeNote note)
    {
        ProcCodeNoteCrud.Insert(note);
    }

    public static void Update(ProcCodeNote note)
    {
        ProcCodeNoteCrud.Update(note);
    }

    public static void Delete(long procCodeNoteNum)
    {
        var command = "DELETE FROM proccodenote WHERE ProcCodeNoteNum = " + SOut.Long(procCodeNoteNum);
        Db.NonQ(command);
    }

    public static string GetNote(long provNum, long codeNum, ProcStat procStatus, bool isGroupNote = false)
    {
        var listProcCodeNotes = GetDeepCopy();
        for (var i = 0; i < listProcCodeNotes.Count; i++)
        {
            if (listProcCodeNotes[i].ProvNum != provNum) continue;
            if (listProcCodeNotes[i].CodeNum != codeNum) continue;
            //Skip provider specific notes if this is a group note and the procedure is not complete
            // OR if this is NOT a group note and the procedure does not have the desired status.
            if ((isGroupNote && listProcCodeNotes[i].ProcStatus != ProcStat.C)
                || (!isGroupNote && listProcCodeNotes[i].ProcStatus != procStatus))
                continue;
            return listProcCodeNotes[i].Note;
        }

        //A provider specific procedure code note could not be found, use the default for the procedure code.
        if (procStatus == ProcStat.TP) return ProcedureCodes.GetProcCode(codeNum).DefaultTPNote;
        return ProcedureCodes.GetProcCode(codeNum).DefaultNote;
    }

    public static string GetTimePattern(long provNum, long codeNum)
    {
        var procCodeNote = GetFirstOrDefault(x => x.ProvNum == provNum && x.CodeNum == codeNum);
        return procCodeNote == null ? ProcedureCodes.GetProcCode(codeNum).ProcTime : procCodeNote.ProcTime;
    }
    
    private class ProcCodeNoteCache : CacheListAbs<ProcCodeNote>
    {
        protected override List<ProcCodeNote> GetCacheFromDb()
        {
            var command = "SELECT * FROM proccodenote";
            return ProcCodeNoteCrud.SelectMany(command);
        }

        protected override List<ProcCodeNote> TableToList(DataTable dataTable)
        {
            return ProcCodeNoteCrud.TableToList(dataTable);
        }

        protected override ProcCodeNote Copy(ProcCodeNote item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ProcCodeNote> items)
        {
            return ProcCodeNoteCrud.ListToTable(items, "ProcCodeNote");
        }

        protected override void FillCacheIfNeeded()
        {
            ProcCodeNotes.GetTableFromCache(false);
        }
    }
    
    private static readonly ProcCodeNoteCache Cache = new();

    public static List<ProcCodeNote> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static ProcCodeNote GetFirstOrDefault(Func<ProcCodeNote, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}