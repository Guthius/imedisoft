using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Prefs
{
    public delegate void OnCacheRefreshDelegate();

    public static bool GetBoolNoCache(PrefName prefName)
    {
        var command = "SELECT ValueString FROM preference WHERE PrefName = '" + SOut.String(prefName.ToString()) + "'";
        return SIn.Bool(DataCore.GetScalar(command));
    }

    public static bool GetYNNoCache(PrefName prefName)
    {
        var command = "SELECT ValueString FROM preference WHERE PrefName = '" + SOut.String(prefName.ToString()) + "'";
        var yn = (YN) SIn.Int(DataCore.GetScalar(command));
        if (yn == YN.Yes) return true;
        if (yn == YN.No) return false;
        //unknown, so use the default
        var prefValueType = prefName.GetValueType();
        if (prefValueType == PrefValueType.YN_DEFAULT_FALSE) return false;
        if (prefValueType == PrefValueType.YN_DEFAULT_TRUE) return true;
        throw new ArgumentException("Invalid type");
    }
    
    public static void Update(Pref pref)
    {
        //Don't use CRUD here because we want to update based on PrefName instead of PrefNum.  Otherwise, it might fail the first time someone runs 7.6.
        var command = "UPDATE preference SET "
                      + "ValueString = '" + SOut.String(pref.ValueString) + "' "
                      + " WHERE PrefName = '" + SOut.String(pref.PrefName) + "'";
        Db.NonQ(command);
    }

    public static bool UpdateInt(PrefName prefName, int newValue)
    {
        return UpdateLong(prefName, newValue);
    }

    public static bool UpdateYN(PrefName prefName, YN newValue)
    {
        return UpdateLong(prefName, (int) newValue);
    }

    public static bool UpdateYN(PrefName prefName, CheckState checkState)
    {
        var yn = YN.Unknown;
        if (checkState == CheckState.Checked) yn = YN.Yes;
        if (checkState == CheckState.Unchecked) yn = YN.No;
        return UpdateYN(prefName, yn);
    }

    public static bool UpdateByte(PrefName prefName, byte newValue)
    {
        return UpdateLong(prefName, newValue);
    }

    public static void UpdateIntNoCache(PrefName prefName, int newValue)
    {
        var command = "UPDATE preference SET ValueString='" + SOut.Long(newValue) + "' WHERE PrefName='" + SOut.String(prefName.ToString()) + "'";
        Db.NonQ(command);
    }

    public static bool UpdateLong(PrefName prefName, long newValue)
    {
        //Very unusual.  Involves cache, so Meth is used further down instead of here at the top.
        var curValue = PrefC.GetLong(prefName);
        if (curValue == newValue) return false; //no change needed
        var command = "UPDATE preference SET "
                      + "ValueString = '" + SOut.Long(newValue) + "' "
                      + "WHERE PrefName = '" + SOut.String(prefName.ToString()) + "'";
        var retVal = true;
        Db.NonQ(command);
        var pref = new Pref();
        pref.PrefName = prefName.ToString();
        pref.ValueString = newValue.ToString();
        UpdateValueForKey(pref);
        return retVal;
    }

    public static bool UpdateDouble(PrefName prefName, double newValue, bool doRounding = true, bool doUseEnUSFormat = false)
    {
        //Very unusual.  Involves cache, so Meth is used further down instead of here at the top.
        var curValue = PrefC.GetDouble(prefName, doUseEnUSFormat);
        if (curValue == newValue) return false; //no change needed
        var command = "UPDATE preference SET "
                      + "ValueString = '" + SOut.Double(newValue, doRounding, doUseEnUSFormat) + "' "
                      + "WHERE PrefName = '" + SOut.String(prefName.ToString()) + "'";
        var retVal = true;
        Db.NonQ(command);
        var pref = new Pref();
        pref.PrefName = prefName.ToString();
        pref.ValueString = newValue.ToString();
        UpdateValueForKey(pref);
        return retVal;
    }

    public static bool UpdateBool(PrefName prefName, bool newValue)
    {
        return UpdateBool(prefName, newValue, false);
    }

    public static bool UpdateBool(PrefName prefName, bool newValue, bool isForced)
    {
        //Very unusual.  Involves cache, so Meth is used further down instead of here at the top.
        var curValue = PrefC.GetBool(prefName);
        if (!isForced && curValue == newValue) return false; //no change needed
        var command = "UPDATE preference SET "
                      + "ValueString = '" + SOut.Bool(newValue) + "' "
                      + "WHERE PrefName = '" + SOut.String(prefName.ToString()) + "'";
        var retVal = true;
        Db.NonQ(command);
        var pref = new Pref();
        pref.PrefName = prefName.ToString();
        pref.ValueString = SOut.Bool(newValue);
        UpdateValueForKey(pref);
        return retVal;
    }

    public static bool UpdateString(PrefName prefName, string newValue)
    {
        //Very unusual.  Involves cache, so Meth is used further down instead of here at the top.
        var curValue = PrefC.GetString(prefName);
        if (curValue == newValue) return false; //no change needed
        var command = "UPDATE preference SET "
                      + "ValueString = '" + SOut.String(newValue) + "' "
                      + "WHERE PrefName = '" + SOut.String(prefName.ToString()) + "'";
        var retVal = true;
        Db.NonQ(command);
        var pref = new Pref();
        pref.PrefName = prefName.ToString();
        pref.ValueString = newValue;
        UpdateValueForKey(pref);
        return retVal;
    }

    public static void UpdateStringNoCache(PrefName prefName, string newValue)
    {
        var command = "UPDATE preference SET ValueString='" + SOut.String(newValue) + "' WHERE PrefName='" + SOut.String(prefName.ToString()) + "'";
        Db.NonQ(command);
    }

    public static bool UpdateDateT(PrefName prefName, DateTime newValue)
    {
        //Very unusual.  Involves cache, so Meth is used further down instead of here at the top.
        var curValue = PrefC.GetDateT(prefName);
        if (curValue == newValue) return false; //no change needed
        var command = "UPDATE preference SET "
                      + "ValueString = '" + SOut.DateT(newValue, false) + "' "
                      + "WHERE PrefName = '" + SOut.String(prefName.ToString()) + "'";
        var retVal = true;
        Db.NonQ(command);
        var pref = new Pref();
        pref.PrefName = prefName.ToString();
        pref.ValueString = SOut.DateT(newValue, false);
        UpdateValueForKey(pref);
        return retVal;
    }

    public static void UpdateDefNumsForPref(PrefName prefName, string defNumFrom, string defNumTo)
    {
        var listStrDefNums = GetOne(prefName)
            .ValueString
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .ToList();
        listStrDefNums = Defs.RemoveOrReplaceDefNum(listStrDefNums, defNumFrom, defNumTo);
        if (listStrDefNums == null) return; //Nothing to update.
        var strDefNums = string.Join(",", listStrDefNums.Select(x => SOut.String(x)));
        UpdateString(prefName, strDefNums);
    }

    public static Pref GetPref(string PrefName)
    {
        return GetOne(PrefName);
    }
    
    public static PrefName GetSheetDefPref(SheetTypeEnum sheetType)
    {
        var retVal = PrefName.SheetsDefaultConsent;
        //The following SheetTypeEnums will always fail this Enum.TryParse(...)
        //ERA, ERAGridHeader, PatientDashboard, PatientDashboardWidget
        //These SheetTypeEnums do not save to the DB when created and do not have a corresponding 'practice wide default'.
        //The mentioned SheetTypeEnums really shouldn't call this function.
        if (!Enum.TryParse("SheetsDefault" + sheetType.GetDescription(), out retVal)) throw new Exception(Lans.g("SheetDefs", "Unsupported SheetTypeEnum") + "\r\n" + sheetType);
        return retVal;
    }

    public static List<Pref> GetInsHistPrefs()
    {
        return GetPrefs(new List<string>
        {
            PrefName.InsHistBWCodes.ToString(), PrefName.InsHistDebridementCodes.ToString(),
            PrefName.InsHistExamCodes.ToString(), PrefName.InsHistPanoCodes.ToString(), PrefName.InsHistPerioLLCodes.ToString(),
            PrefName.InsHistPerioLRCodes.ToString(), PrefName.InsHistPerioMaintCodes.ToString(), PrefName.InsHistPerioULCodes.ToString(),
            PrefName.InsHistPerioURCodes.ToString(), PrefName.InsHistProphyCodes.ToString()
        });
    }

    public static List<PrefName> GetInsHistPrefNames()
    {
        return new List<PrefName>
        {
            PrefName.InsHistBWCodes, PrefName.InsHistPanoCodes, PrefName.InsHistExamCodes, PrefName.InsHistProphyCodes,
            PrefName.InsHistPerioURCodes, PrefName.InsHistPerioULCodes, PrefName.InsHistPerioLRCodes, PrefName.InsHistPerioLLCodes,
            PrefName.InsHistPerioMaintCodes, PrefName.InsHistDebridementCodes
        };
    }
    
    private class PrefCache : CacheDictNonPkAbs<Pref, string, Pref>
    {
        public OnCacheRefreshDelegate OnCacheRefresh;

        protected override List<Pref> GetCacheFromDb()
        {
            var command = "SELECT * FROM preference";
            return PrefCrud.SelectMany(command);
        }

        protected override List<Pref> TableToList(DataTable dataTable)
        {
            //Can't use Crud.PrefCrud.TableToList(table) because it will fail the first time someone runs 7.6 before conversion.
            var listPrefs = new List<Pref>();
            var containsPrefNum = dataTable.Columns.Contains("PrefNum");
            foreach (DataRow row in dataTable.Rows)
            {
                var pref = new Pref();
                if (containsPrefNum) pref.PrefNum = SIn.Long(row["PrefNum"].ToString());
                pref.PrefName = SIn.String(row["PrefName"].ToString());
                pref.ValueString = SIn.String(row["ValueString"].ToString());
                //no need to load up the comments.  Especially since this will fail when user first runs version 5.8.
                listPrefs.Add(pref);
            }

            return listPrefs;
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
            Prefs.GetTableFromCache(false);
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
            var dictPrefs = new Dictionary<string, Pref>();
            var listDuplicatePrefs = new List<string>();
            foreach (var pref in items)
                if (dictPrefs.ContainsKey(pref.PrefName))
                    listDuplicatePrefs.Add(pref.PrefName); //The current preference is a duplicate preference.
                else
                    dictPrefs.Add(pref.PrefName, pref);

            if (listDuplicatePrefs.Count > 0 && //Duplicate preferences found, and
                dictPrefs.ContainsKey(PrefName.CorruptedDatabase.ToString()) && //CorruptedDatabase preference exists (only v3.4+), and
                dictPrefs[PrefName.CorruptedDatabase.ToString()].ValueString != "0") //The CorruptedDatabase flag is set.
                throw new ApplicationException(Lans.g("Prefs", "Your database is corrupted because an update failed.  Please contact us.  This database is unusable and you will need to restore from a backup."));

            if (listDuplicatePrefs.Count > 0) //Duplicate preferences, but the CorruptedDatabase flag is not set.
                throw new ApplicationException(Lans.g("Prefs", "Duplicate preferences found in database") + ": " + string.Join(",", listDuplicatePrefs));
            return dictPrefs;
        }

        protected override DataTable ToDataTable(List<Pref> items)
        {
            return PrefCrud.ListToTable(items);
        }

        protected override void GotNewCache(List<Pref> items)
        {
            base.GotNewCache(items);
            ODException.SwallowAnyException(() => OnCacheRefresh?.Invoke());
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
        if (!PrefCaches.GetContainsKey(prefName)) throw new Exception(prefName + " is an invalid pref name.");
        return PrefCaches.GetOne(prefName);
    }

    public static List<Pref> GetPrefs(List<string> listPrefNames)
    {
        if (listPrefNames == null || listPrefNames.Count == 0) return new List<Pref>();
        return PrefCaches.GetWhere(x => listPrefNames.Contains(x.PrefName));
    }

    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return PrefCaches.GetTableFromCache(doRefreshCache);
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