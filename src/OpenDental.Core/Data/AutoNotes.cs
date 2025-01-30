using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using Newtonsoft.Json;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class AutoNotes
{
    public static void Insert(AutoNote autoNote)
    {
        AutoNoteCrud.Insert(autoNote);
    }

    public static void InsertBatch(List<SerializableAutoNote> serializableAutoNotes)
    {
        if (serializableAutoNotes == null || serializableAutoNotes.Count == 0)
        {
            return;
        }

        var autoNotes = new List<AutoNote>();
        
        foreach (var serializableAutoNote in serializableAutoNotes)
        {
            var newNote = new AutoNote
            {
                AutoNoteName = serializableAutoNote.AutoNoteName,
                Category = 0,
                MainText = serializableAutoNote.MainText
            };
            autoNotes.Add(newNote);
        }

        AutoNoteCrud.InsertMany(autoNotes);
    }

    public static void Update(AutoNote autoNote)
    {
        AutoNoteCrud.Update(autoNote);
    }

    public static void Delete(long autoNoteNum)
    {
        Db.NonQ("DELETE FROM autonote WHERE AutoNoteNum = " + autoNoteNum);
    }

    public static string GetByTitle(string autoNoteTitle)
    {
        var autoNote = GetFirstOrDefault(x => x.AutoNoteName == autoNoteTitle);
        return autoNote == null ? "" : autoNote.MainText;
    }

    public static List<SerializableAutoNote> GetSerializableAutoNotes(List<AutoNote> autoNotes)
    {
        return autoNotes.Select(x => new SerializableAutoNote(x)).ToList();
    }

    public static string GetAutoNoteName(string promptResponse)
    {
        var result = "";
        var brackets = new Stack<int>();
        
        for (var pos = 0; pos < promptResponse.Length; pos++)
        {
            if (promptResponse[pos] == '{')
            {
                brackets.Push(pos);
                continue;
            }

            if (promptResponse[pos] != '}' || !brackets.Any())
            {
                continue;
            }
            
            var posOpenBracket = brackets.Peek();
            
            var length = pos - posOpenBracket;
            if (length < 1)
            {
                brackets.Pop();
                continue;
            }
            
            var autoNoteName = promptResponse.Substring(posOpenBracket + 1, length - 1);
            if (!string.IsNullOrEmpty(autoNoteName) && IsValidAutoNote(autoNoteName))
            {
                result = autoNoteName;
                break;
            }

            brackets.Pop();
        }

        return result;
    }

    public static bool IsValidAutoNote(string autoNoteName)
    {
        var autoNote = GetFirstOrDefault(x => x.AutoNoteName == autoNoteName);
        return autoNote != null;
    }

    public static void RemoveFromCategory(long autoNoteCatDefNum)
    {
        Db.NonQ("UPDATE autonote SET Category=0 WHERE Category=" + autoNoteCatDefNum);
    }

    public static void WriteAutoNotesToJson(List<SerializableAutoNote> serializableAutoNotes, List<SerializableAutoNoteControl> serializableAutoNoteControls, string path)
    {
        var transferableAutoNotes = new TransferableAutoNotes(serializableAutoNotes, serializableAutoNoteControls);
        var json = JsonConvert.SerializeObject(transferableAutoNotes);
        File.WriteAllText(path, json);
    }

    private class AutoNoteCache : CacheListAbs<AutoNote>
    {
        protected override List<AutoNote> GetCacheFromDb()
        {
            return AutoNoteCrud.SelectMany("SELECT * FROM autonote ORDER BY AutoNoteName");
        }

        protected override List<AutoNote> TableToList(DataTable dataTable)
        {
            return AutoNoteCrud.TableToList(dataTable);
        }

        protected override AutoNote Copy(AutoNote item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<AutoNote> items)
        {
            return AutoNoteCrud.ListToTable(items, "AutoNote");
        }

        protected override void FillCacheIfNeeded()
        {
            AutoNotes.GetTableFromCache(false);
        }
    }

    private static readonly AutoNoteCache Cache = new();

    public static List<AutoNote> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static List<AutoNote> GetWhere(Predicate<AutoNote> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static bool GetExists(Predicate<AutoNote> match, bool isShort = false)
    {
        return Cache.GetExists(match, isShort);
    }

    private static AutoNote GetFirstOrDefault(Func<AutoNote, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
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

public class SerializableAutoNote(AutoNote autoNote)
{
    public readonly string AutoNoteName = autoNote.AutoNoteName;

    public string MainText = autoNote.MainText;
}

public class TransferableAutoNotes
{
    public readonly List<SerializableAutoNoteControl> AutoNoteControls;
    public readonly List<SerializableAutoNote> AutoNotes;

    public TransferableAutoNotes()
    {
    }

    public TransferableAutoNotes(List<SerializableAutoNote> autoNotes, List<SerializableAutoNoteControl> autoNoteControls)
    {
        AutoNotes = autoNotes;
        AutoNoteControls = autoNoteControls;
    }
}