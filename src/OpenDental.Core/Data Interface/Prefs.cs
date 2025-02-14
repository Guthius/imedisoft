using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Prefs
{
    public static bool GetBoolNoCache(PrefName prefName)
    {
        return SIn.Bool(DataCore.GetScalar("SELECT ValueString FROM preference WHERE PrefName = '" + SOut.String(prefName.ToString()) + "'"));
    }

    public static bool GetYnNoCache(PrefName prefName)
    {
        var command = "SELECT ValueString FROM preference WHERE PrefName = '" + SOut.String(prefName.ToString()) + "'";

        var yn = (YN) SIn.Int(DataCore.GetScalar(command));
        switch (yn)
        {
            case YN.Yes:
                return true;

            case YN.No:
                return false;
        }

        var prefValueType = prefName.GetValueType();

        return prefValueType switch
        {
            PrefValueType.YN_DEFAULT_FALSE => false,
            PrefValueType.YN_DEFAULT_TRUE => true,
            _ => throw new ArgumentException("Invalid type")
        };
    }

    public static void Update(Pref pref)
    {
        Db.NonQ("UPDATE preference SET ValueString = '" + SOut.String(pref.ValueString) + "' " + " WHERE PrefName = '" + SOut.String(pref.PrefName) + "'");
    }

    public static bool UpdateInt(PrefName prefName, int newValue)
    {
        return UpdateLong(prefName, newValue);
    }

    public static bool UpdateYn(PrefName prefName, YN newValue)
    {
        return UpdateLong(prefName, (int) newValue);
    }

    public static bool UpdateYn(PrefName prefName, CheckState checkState)
    {
        return UpdateYn(prefName, checkState switch
        {
            CheckState.Checked => YN.Yes,
            CheckState.Unchecked => YN.No,
            _ => YN.Unknown
        });
    }

    public static bool UpdateByte(PrefName prefName, byte newValue)
    {
        return UpdateLong(prefName, newValue);
    }

    public static bool UpdateLong(PrefName prefName, long newValue)
    {
        var value = PrefC.GetLong(prefName);
        if (value == newValue)
        {
            return false;
        }

        Db.NonQ("UPDATE preference SET ValueString = '" + newValue + "' WHERE PrefName = '" + SOut.String(prefName.ToString()) + "'");

        var pref = new Pref
        {
            PrefName = prefName.ToString(),
            ValueString = newValue.ToString()
        };

        UpdateValueForKey(pref);

        return true;
    }

    public static bool UpdateDouble(PrefName prefName, double newValue, bool doRounding = true, bool useEnUsFormat = false)
    {
        var value = PrefC.GetDouble(prefName, useEnUsFormat);
        if (value == newValue)
        {
            return false;
        }

        Db.NonQ("UPDATE preference SET ValueString = '" + SOut.Double(newValue, doRounding, useEnUsFormat) + "' WHERE PrefName = '" + SOut.String(prefName.ToString()) + "'");

        UpdateValueForKey(new Pref
        {
            PrefName = prefName.ToString(),
            ValueString = newValue.ToString(CultureInfo.InvariantCulture)
        });

        return true;
    }

    public static bool UpdateBool(PrefName prefName, bool newValue, bool isForced = false)
    {
        var value = PrefC.GetBool(prefName);
        if (!isForced && value == newValue)
        {
            return false;
        }

        Db.NonQ("UPDATE preference SET ValueString = '" + SOut.Bool(newValue) + "' WHERE PrefName = '" + SOut.String(prefName.ToString()) + "'");

        UpdateValueForKey(new Pref
        {
            PrefName = prefName.ToString(),
            ValueString = SOut.Bool(newValue)
        });

        return true;
    }

    public static bool UpdateString(PrefName prefName, string newValue)
    {
        var value = PrefC.GetString(prefName);
        if (value == newValue)
        {
            return false;
        }

        Db.NonQ("UPDATE preference SET ValueString = '" + SOut.String(newValue) + "' WHERE PrefName = '" + SOut.String(prefName.ToString()) + "'");

        UpdateValueForKey(new Pref
        {
            PrefName = prefName.ToString(),
            ValueString = newValue
        });

        return true;
    }

    public static void UpdateStringNoCache(PrefName prefName, string newValue)
    {
        Db.NonQ("UPDATE preference SET ValueString = '" + SOut.String(newValue) + "' WHERE PrefName = '" + SOut.String(prefName.ToString()) + "'");
    }

    public static bool UpdateDateT(PrefName prefName, DateTime newValue)
    {
        var value = PrefC.GetDateT(prefName);
        if (value == newValue)
        {
            return false;
        }

        Db.NonQ("UPDATE preference SET ValueString = '" + SOut.DateTime(newValue, false) + "' WHERE PrefName = '" + SOut.String(prefName.ToString()) + "'");

        UpdateValueForKey(new Pref
        {
            PrefName = prefName.ToString(),
            ValueString = SOut.DateTime(newValue, false)
        });

        return true;
    }

    public static void UpdateDefNumsForPref(PrefName prefName, string defNumFrom, string defNumTo)
    {
        var defNums = GetOne(prefName)
            .ValueString
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        defNums = Defs.RemoveOrReplaceDefNum(defNums, defNumFrom, defNumTo);
        if (defNums == null)
        {
            return;
        }

        var strDefNums = string.Join(",", defNums.Select(SOut.String));

        UpdateString(prefName, strDefNums);
    }

    public static Pref GetPref(string prefName)
    {
        return GetOne(prefName);
    }

    public static PrefName GetSheetDefPref(SheetTypeEnum sheetType)
    {
        if (!Enum.TryParse("SheetsDefault" + sheetType.GetDescription(), out PrefName retVal))
        {
            throw new Exception("Unsupported SheetTypeEnum\r\n" + sheetType);
        }

        return retVal;
    }

    public static List<Pref> GetInsHistPrefs()
    {
        return GetPrefs(
        [
            nameof(PrefName.InsHistBWCodes),
            nameof(PrefName.InsHistDebridementCodes),
            nameof(PrefName.InsHistExamCodes),
            nameof(PrefName.InsHistPanoCodes),
            nameof(PrefName.InsHistPerioLLCodes),
            nameof(PrefName.InsHistPerioLRCodes),
            nameof(PrefName.InsHistPerioMaintCodes),
            nameof(PrefName.InsHistPerioULCodes),
            nameof(PrefName.InsHistPerioURCodes),
            nameof(PrefName.InsHistProphyCodes)
        ]);
    }

    public static List<PrefName> GetInsHistPrefNames()
    {
        return
        [
            PrefName.InsHistBWCodes,
            PrefName.InsHistPanoCodes,
            PrefName.InsHistExamCodes,
            PrefName.InsHistProphyCodes,
            PrefName.InsHistPerioURCodes,
            PrefName.InsHistPerioULCodes,
            PrefName.InsHistPerioLRCodes,
            PrefName.InsHistPerioLLCodes,
            PrefName.InsHistPerioMaintCodes,
            PrefName.InsHistDebridementCodes
        ];
    }

    private class PrefCache : CacheDictNonPkAbs<Pref, string, Pref>
    {
        protected override List<Pref> GetCacheFromDb()
        {
            return PrefCrud.SelectMany("SELECT * FROM preference");
        }

        protected override List<Pref> TableToList(DataTable dataTable)
        {
            var prefs = new List<Pref>();

            var containsPrefNum = dataTable.Columns.Contains("PrefNum");

            foreach (DataRow row in dataTable.Rows)
            {
                var pref = new Pref();

                if (containsPrefNum)
                {
                    pref.PrefNum = SIn.Long(row["PrefNum"].ToString());
                }

                pref.PrefName = SIn.String(row["PrefName"].ToString());
                pref.ValueString = SIn.String(row["ValueString"].ToString());

                prefs.Add(pref);
            }

            return prefs;
        }

        protected override Pref Copy(Pref item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(Dictionary<string, Pref> dict)
        {
            return PrefCrud.ListToTable(dict.Values.ToList(), "Pref");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }

        protected override string GetDictKey(Pref item)
        {
            return item.PrefName;
        }

        protected override Pref GetDictValue(Pref item)
        {
            return item;
        }

        protected override Pref CopyValue(Pref pref)
        {
            return pref.Copy();
        }

        protected override Dictionary<string, Pref> ToDictionary(List<Pref> items)
        {
            var prefs = new Dictionary<string, Pref>();
            var duplicatePrefs = new List<string>();

            foreach (var pref in items)
            {
                if (prefs.ContainsKey(pref.PrefName))
                {
                    duplicatePrefs.Add(pref.PrefName);
                }
                else
                {
                    prefs.Add(pref.PrefName, pref);
                }
            }

            return duplicatePrefs.Count switch
            {
                > 0 when prefs.ContainsKey(nameof(PrefName.CorruptedDatabase)) && prefs[nameof(PrefName.CorruptedDatabase)].ValueString != "0" => throw new ApplicationException("Your database is corrupted because an update failed.  Please contact us.  This database is unusable and you will need to restore from a backup."),
                > 0 => throw new ApplicationException("Duplicate preferences found in database: " + string.Join(",", duplicatePrefs)),
                _ => prefs
            };
        }

        protected override DataTable ToDataTable(List<Pref> items)
        {
            return PrefCrud.ListToTable(items);
        }
    }

    private static PrefCache PrefCaches { get; } = new();

    public static bool GetContainsKey(string prefName)
    {
        return PrefCaches.GetContainsKey(prefName);
    }

    public static bool DictIsNull()
    {
        return PrefCaches.DictIsNull();
    }

    public static Pref GetOne(PrefName prefName)
    {
        return GetOne(prefName.ToString());
    }

    public static Pref GetOne(string prefName)
    {
        if (!PrefCaches.GetContainsKey(prefName))
        {
            throw new Exception(prefName + " is an invalid pref name.");
        }

        return PrefCaches.GetOne(prefName);
    }

    public static List<Pref> GetPrefs(List<string> prefNames)
    {
        if (prefNames == null || prefNames.Count == 0)
        {
            return [];
        }

        return PrefCaches.GetWhere(x => prefNames.Contains(x.PrefName));
    }

    public static void RefreshCache()
    {
        PrefCaches.GetTableFromCache(true);
    }

    public static void GetTableFromCache(bool refreshCache)
    {
        PrefCaches.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        PrefCaches.ClearCache();
    }

    public static void UpdateValueForKey(Pref pref)
    {
        PrefCaches.SetValueForKey(pref.PrefName, pref);
    }
}