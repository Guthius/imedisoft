using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class AutoNoteControls
{
    public static void Insert(AutoNoteControl autoNoteControl)
    {
        AutoNoteControlCrud.Insert(autoNoteControl);
    }

    public static void InsertBatch(List<SerializableAutoNoteControl> serializableAutoNoteControls)
    {
        if (serializableAutoNoteControls == null || serializableAutoNoteControls.Count == 0)
        {
            return;
        }
        
        var autoNoteControls = new List<AutoNoteControl>();
        
        foreach (var serializableAutoNoteControl in serializableAutoNoteControls)
        {
            autoNoteControls.Add(new AutoNoteControl
            {
                ControlLabel = serializableAutoNoteControl.ControlLabel,
                ControlOptions = serializableAutoNoteControl.ControlOptions,
                ControlType = serializableAutoNoteControl.ControlType,
                Descript = serializableAutoNoteControl.Descript
            });
        }

        AutoNoteControlCrud.InsertMany(autoNoteControls);
    }

    public static void Update(AutoNoteControl autoNoteControl)
    {
        AutoNoteControlCrud.Update(autoNoteControl);
    }

    public static void Delete(long autoNoteControlNum)
    {
        Db.NonQ("DELETE FROM autonotecontrol WHERE AutoNoteControlNum=" + autoNoteControlNum);
    }

    public static AutoNoteControl GetByDescript(string descript)
    {
        return GetFirstOrDefault(x => x.Descript == descript);
    }
    
    public static List<SerializableAutoNoteControl> GetSerializableAutoNoteControls(List<AutoNoteControl> autoNoteControls)
    {
        return autoNoteControls.Select(x => new SerializableAutoNoteControl(x)).ToList();
    }

    public static List<AutoNoteControl> GetListByParsingAutoNoteText(List<SerializableAutoNote> serializableAutoNotes)
    {
        var autoNoteControls = new List<AutoNoteControl>();
        var matches = new List<Match>();
        
        foreach (var serializableAutoNote in serializableAutoNotes)
        {
            matches.AddRange(GetPrompts(serializableAutoNote.MainText));
        }
        
        foreach (var match in matches)
        {
            var description = match.ToString();
            
            description = description.Replace("[Prompt:\"", "");
            description = description.Replace("\"]", "");
            
            var autoNoteControl = GetByDescript(description);
            if (autoNoteControl != null)
            {
                autoNoteControls.Add(autoNoteControl);
            }
        }

        return autoNoteControls.DistinctBy(x => x.Descript).ToList();
    }

    public static List<Match> GetPrompts(string noteText)
    {
        return Regex.Matches(noteText, """\[Prompt:"[a-zA-Z_0-9 ]+"\]""").OfType<Match>().ToList();
    }

    public static void RemoveDuplicatesFromList(List<SerializableAutoNoteControl> serializableAutoNoteControls, List<SerializableAutoNote> serializableAutoNotes)
    {
        var duplicates = new List<string>();
        
        foreach (var serializableAutoNoteControl in serializableAutoNoteControls)
        {
            var nameChanged = false;
            var autoNoteControl = GetByDescript(serializableAutoNoteControl.Descript);
            var count = 0;
            
            var name = serializableAutoNoteControl.Descript; 
            while (true)
            {
                if (autoNoteControl is null)
                {
                    break;
                }
                
                if (autoNoteControl.ControlOptions == serializableAutoNoteControl.ControlOptions &&
                    autoNoteControl.ControlType == serializableAutoNoteControl.ControlType) 
                {
                    duplicates.Add(serializableAutoNoteControl.Descript);
                    break;
                }

                count++;
                
                serializableAutoNoteControl.Descript = string.Join("_", name, count.ToString());
                
                nameChanged = true;
                
                autoNoteControl = GetByDescript(serializableAutoNoteControl.Descript);
            }

            if (!nameChanged)
            {
                continue;
            }
            
            foreach (var serializableAutoNote in serializableAutoNotes)
            {
                serializableAutoNote.MainText = serializableAutoNote.MainText.Replace("[Prompt:\"" + name + "\"]", "[Prompt:\"" + serializableAutoNoteControl.Descript + "\"]");
            }
        }

        serializableAutoNoteControls.RemoveAll(x => duplicates.Contains(x.Descript));
    }
    
    private class AutoNoteControlCache : CacheListAbs<AutoNoteControl>
    {
        protected override List<AutoNoteControl> GetCacheFromDb()
        {
            return AutoNoteControlCrud.SelectMany("SELECT * FROM autonotecontrol ORDER BY Descript");
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