using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using DataConnectionBase;
using Newtonsoft.Json;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class AutoNotes
{
    public static void Insert(AutoNote autoNote)
    {
        AutoNoteCrud.Insert(autoNote);
    }

    public static void InsertBatch(List<SerializableAutoNote> listSerializableAutoNotes)
    {
        if (listSerializableAutoNotes == null || listSerializableAutoNotes.Count == 0) return;

        var listAutoNotes = new List<AutoNote>();
        for (var i = 0; i < listSerializableAutoNotes.Count; i++)
        {
            var newNote = new AutoNote();
            newNote.AutoNoteName = listSerializableAutoNotes[i].AutoNoteName;
            newNote.Category = 0; //It would be too error-prone trying to select the category, so we'll just leave it as 0.
            newNote.MainText = listSerializableAutoNotes[i].MainText;
            listAutoNotes.Add(newNote);
        }

        AutoNoteCrud.InsertMany(listAutoNotes);
    }

    public static void Update(AutoNote autoNote)
    {
        AutoNoteCrud.Update(autoNote);
    }

    public static void Delete(long autoNoteNum)
    {
        var command = "DELETE FROM autonote " + "WHERE AutoNoteNum = " + SOut.Long(autoNoteNum);
        Db.NonQ(command);
    }

    public static string GetByTitle(string autoNoteTitle)
    {
        var autoNote = GetFirstOrDefault(x => x.AutoNoteName == autoNoteTitle);
        if (autoNote == null) return "";

        return autoNote.MainText;
    }

    public static List<SerializableAutoNote> GetSerializableAutoNotes(List<AutoNote> listAutoNotes)
    {
        return listAutoNotes.Select(x => new SerializableAutoNote(x)).ToList();
    }

    public static string GetAutoNoteName(string promptResponse)
    {
        var retVal = "";
        var stackBrackets = new Stack<int>();
        //The AutoNoteResponseText should be in format "Auto Note Response Text : {AutoNoteName}". Go through each of the charactors in promptResponse
        //and find each of the possible AutoNote names. We need to do this in case the AutoNote name has brackets("{,}") in the name. 
        for (var posCur = 0; posCur < promptResponse.Length; posCur++)
        {
            if (promptResponse[posCur] == '{')
            {
                stackBrackets.Push(posCur); //add the position of the '{' to the stack.
                continue;
            }

            if (promptResponse[posCur] != '}' || stackBrackets.Count() == 0)
                //continue if the the stack does not have an open '{', or this is not a '}'
                continue;

            //The posOpenBracket will include the '{'. We will have to remove it.
            var posOpenBracket = stackBrackets.Peek();
            //Get the length of the possible autonote. The length will include the closing '}'. We will also need to remove it.
            var length = posCur - posOpenBracket;
            if (length < 1)
            {
                stackBrackets.Pop();
                continue;
            }

            //Get string of possible AutoNoteName. Remove the bracket from the beginning and end. 
            var autoNoteName = promptResponse.Substring(posOpenBracket + 1, length - 1);
            if (!string.IsNullOrEmpty(autoNoteName) && IsValidAutoNote(autoNoteName))
            {
                retVal = autoNoteName;
                break;
            }

            //no match found. Remove position from stack and continue.
            stackBrackets.Pop();
        }

        return retVal; //could be empty string if no valid autonote name is found
    }

    public static bool IsValidAutoNote(string autoNoteName)
    {
        var autoNote = GetFirstOrDefault(x => x.AutoNoteName == autoNoteName);
        return autoNote != null;
    }

    public static void RemoveFromCategory(long autoNoteCatDefNum)
    {
        var command = "UPDATE autonote SET Category=0 WHERE Category=" + SOut.Long(autoNoteCatDefNum);
        Db.NonQ(command);
    }

    public static void WriteAutoNotesToJson(List<SerializableAutoNote> listSerializableAutoNotes, List<SerializableAutoNoteControl> listSerializableAutoNoteControls, string path)
    {
        var transferableAutoNotes = new TransferableAutoNotes(listSerializableAutoNotes, listSerializableAutoNoteControls);
        var json = JsonConvert.SerializeObject(transferableAutoNotes);
        File.WriteAllText(path, json);
    }

    private class AutoNoteCache : CacheListAbs<AutoNote>
    {
        protected override List<AutoNote> GetCacheFromDb()
        {
            var command = "SELECT * FROM autonote ORDER BY AutoNoteName";
            return AutoNoteCrud.SelectMany(command);
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