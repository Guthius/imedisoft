using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ProcCodeNotes
{
    public static List<ProcCodeNote> GetList(long codeNum)
    {
        return ProcCodeNoteCrud.SelectMany("SELECT * FROM proccodenote WHERE CodeNum = " + codeNum);
    }

    public static void Insert(ProcCodeNote procCodeNote)
    {
        ProcCodeNoteCrud.Insert(procCodeNote);
    }

    public static void Update(ProcCodeNote procCodeNote)
    {
        ProcCodeNoteCrud.Update(procCodeNote);
    }

    public static void Delete(long procCodeNoteNum)
    {
        Db.NonQ("DELETE FROM proccodenote WHERE ProcCodeNoteNum = " + procCodeNoteNum);
    }

    public static string GetNote(long provNum, long codeNum, ProcStat procStatus, bool isGroupNote = false)
    {
        var procCodeNotes = GetDeepCopy();

        foreach (var procCodeNote in procCodeNotes)
        {
            if (procCodeNote.ProvNum != provNum ||
                procCodeNote.CodeNum != codeNum)
            {
                continue;
            }

            if ((isGroupNote && procCodeNote.ProcStatus != ProcStat.C) || (!isGroupNote && procCodeNote.ProcStatus != procStatus))
            {
                continue;
            }

            return procCodeNote.Note;
        }

        return procStatus == ProcStat.TP
            ? ProcedureCodes.GetProcCode(codeNum).DefaultTPNote
            : ProcedureCodes.GetProcCode(codeNum).DefaultNote;
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
            return ProcCodeNoteCrud.SelectMany("SELECT * FROM proccodenote");
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
            GetTableFromCache(false);
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