using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class AutoNoteControls
{
    public static void Insert(AutoNoteControl autoNoteControl)
    {
        AutoNoteControlCrud.Insert(autoNoteControl);
    }

    public static void InsertBatch(List<SerializableAutoNoteControl> listSerializableAutoNoteControls)
    {
        if (listSerializableAutoNoteControls == null || listSerializableAutoNoteControls.Count == 0) return;
        var listAutoNoteControls = new List<AutoNoteControl>();
        for (var i = 0; i < listSerializableAutoNoteControls.Count; i++)
        {
            var autoNoteControl = new AutoNoteControl();
            autoNoteControl.ControlLabel = listSerializableAutoNoteControls[i].ControlLabel;
            autoNoteControl.ControlOptions = listSerializableAutoNoteControls[i].ControlOptions;
            autoNoteControl.ControlType = listSerializableAutoNoteControls[i].ControlType;
            autoNoteControl.Descript = listSerializableAutoNoteControls[i].Descript;
            listAutoNoteControls.Add(autoNoteControl);
        }

        AutoNoteControlCrud.InsertMany(listAutoNoteControls);
    }

    public static void Update(AutoNoteControl autoNoteControl)
    {
        AutoNoteControlCrud.Update(autoNoteControl);
    }

    public static void Delete(long autoNoteControlNum)
    {
        //no validation for now.
        var command = "DELETE FROM autonotecontrol WHERE AutoNoteControlNum=" + SOut.Long(autoNoteControlNum);
        Db.NonQ(command);
    }

    public static AutoNoteControl GetByDescript(string descript)
    {
        return GetFirstOrDefault(x => x.Descript == descript);
    }
    
    public static List<SerializableAutoNoteControl> GetSerializableAutoNoteControls(List<AutoNoteControl> listAutoNoteControls)
    {
        return listAutoNoteControls.Select(x => new SerializableAutoNoteControl(x)).ToList();
    }

    public static List<AutoNoteControl> GetListByParsingAutoNoteText(List<SerializableAutoNote> listSerializableAutoNotes)
    {
        var listAutoNoteControls = new List<AutoNoteControl>();
        var listMatches = new List<Match>();
        //Find all prompts in all the provided AutoNotes
        for (var i = 0; i < listSerializableAutoNotes.Count; i++) listMatches.AddRange(GetPrompts(listSerializableAutoNotes[i].MainText));
        //Clean up the text for every discovered prompt to retrieve the appropriate AutoNoteControl if it exists
        for (var i = 0; i < listMatches.Count; i++)
        {
            var descript = listMatches[i].ToString();
            //Trimming down the match to just the Descript of the prompt text
            descript = descript.Replace("[Prompt:\"", "");
            descript = descript.Replace("\"]", "");
            var autoNoteControl = GetByDescript(descript);
            if (autoNoteControl != null) listAutoNoteControls.Add(autoNoteControl);
        }

        return listAutoNoteControls.DistinctBy(x => x.Descript).ToList();
    }

    public static List<Match> GetPrompts(string noteText)
    {
        //Prompts are stored in the form [Prompt: "PromptName"]
        return Regex.Matches(noteText, @"\[Prompt:""[a-zA-Z_0-9 ]+""\]").OfType<Match>().ToList();
    }

    public static void RemoveDuplicatesFromList(List<SerializableAutoNoteControl> listSerializableAutoNoteControls, List<SerializableAutoNote> listSerializableAutoNotes)
    {
        var listTrueDuplicates = new List<string>();
        for (var i = 0; i < listSerializableAutoNoteControls.Count; i++)
        {
            var wasNameChanged = false;
            var autoNoteControl = GetByDescript(listSerializableAutoNoteControls[i].Descript);
            var dupNum = 0;
            var name = listSerializableAutoNoteControls[i].Descript; //Used to hold the current name so we can safely change the name while we check for duplicates
            while (true)
            {
                //While the control name is not unique
                if (autoNoteControl == null) break;
                if (autoNoteControl.ControlOptions == listSerializableAutoNoteControls[i].ControlOptions
                    && autoNoteControl.ControlType == listSerializableAutoNoteControls[i].ControlType) //ControlLabel is omitted because it serves no functional purpose in a duplication check
                {
                    listTrueDuplicates.Add(listSerializableAutoNoteControls[i].Descript); //Add this to a list of true duplicates so it doesn't get reimported
                    break;
                }

                dupNum++;
                listSerializableAutoNoteControls[i].Descript = string.Join("_", name, dupNum.ToString());
                wasNameChanged = true;
                autoNoteControl = GetByDescript(listSerializableAutoNoteControls[i].Descript);
            }

            if (wasNameChanged) //If the name changed, replace all instances of it across the new AutoNotes
                for (var j = 0; j < listSerializableAutoNotes.Count; j++)
                    listSerializableAutoNotes[j].MainText = listSerializableAutoNotes[j].MainText.Replace("[Prompt:\"" + name + "\"]", "[Prompt:\"" + listSerializableAutoNoteControls[i].Descript + "\"]");
        }

        listSerializableAutoNoteControls.RemoveAll(x => listTrueDuplicates.Contains(x.Descript));
    }
    
    private class AutoNoteControlCache : CacheListAbs<AutoNoteControl>
    {
        protected override List<AutoNoteControl> GetCacheFromDb()
        {
            var command = "SELECT * FROM autonotecontrol ORDER BY Descript";
            return AutoNoteControlCrud.SelectMany(command);
        }

        protected override List<AutoNoteControl> TableToList(DataTable dataTable)
        {
            return AutoNoteControlCrud.TableToList(dataTable);
        }

        protected override AutoNoteControl Copy(AutoNoteControl item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<AutoNoteControl> items)
        {
            return AutoNoteControlCrud.ListToTable(items, "AutoNoteControl");
        }

        protected override void FillCacheIfNeeded()
        {
            AutoNoteControls.GetTableFromCache(false);
        }
    }

    private static readonly AutoNoteControlCache Cache = new();

    public static List<AutoNoteControl> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    private static AutoNoteControl GetFirstOrDefault(Func<AutoNoteControl, bool> match, bool isShort = false)
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

public class SerializableAutoNoteControl(AutoNoteControl autoNoteControl)
{
    public readonly string ControlLabel = autoNoteControl.ControlLabel;
    public readonly string ControlOptions = autoNoteControl.ControlOptions;
    public readonly string ControlType = autoNoteControl.ControlType;
    public string Descript = autoNoteControl.Descript;
}