using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ProcCodeNotes
{
    
    public static List<ProcCodeNote> GetList(long codeNum)
    {
        var command = "SELECT * FROM proccodenote WHERE CodeNum=" + SOut.Long(codeNum);
        return ProcCodeNoteCrud.SelectMany(command);
    }

    
    public static long Insert(ProcCodeNote note)
    {
        return ProcCodeNoteCrud.Insert(note);
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

    /// <summary>
    ///     Gets the note for the given provider, if one exists.  Otherwise, gets the proccode.defaultnote.
    ///     Currently procStatus only supports TP or C statuses.
    /// </summary>
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

    ///<summary>Gets the time pattern for the given provider, if one exists.  Otherwise, gets the proccode.ProcTime.</summary>
    public static string GetTimePattern(long provNum, long codeNum)
    {
        var procCodeNote = GetFirstOrDefault(x => x.ProvNum == provNum && x.CodeNum == codeNum);
        return procCodeNote == null ? ProcedureCodes.GetProcCode(codeNum).ProcTime : procCodeNote.ProcTime;
    }

    #region CachePattern

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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ProcCodeNoteCache _procCodeNoteCache = new();

    public static List<ProcCodeNote> GetDeepCopy(bool isShort = false)
    {
        return _procCodeNoteCache.GetDeepCopy(isShort);
    }

    public static ProcCodeNote GetFirstOrDefault(Func<ProcCodeNote, bool> match, bool isShort = false)
    {
        return _procCodeNoteCache.GetFirstOrDefault(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _procCodeNoteCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _procCodeNoteCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _procCodeNoteCache.ClearCache();
    }

    #endregion
}