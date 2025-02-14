using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness.HL7;

namespace OpenDentBusiness;

public class HL7Defs
{
    public static HL7Def GetInternalFromDb(HL7InternalType hL7InternalType)
    {
        return HL7DefCrud.SelectOne("SELECT * FROM hl7def WHERE IsInternal = 1 AND InternalType = '" + SOut.String(hL7InternalType.ToString()) + "'");
    }

    public static List<HL7Def> GetListInternalFromDb()
    {
        return HL7DefCrud.SelectMany("SELECT * FROM hl7def WHERE IsInternal = 1");
    }

    public static HL7Def GetOneDeepEnabled(bool isMedLabHL7 = false)
    {
        var result = GetFirstOrDefault(x => x.IsEnabled && isMedLabHL7 == (x.InternalType == HL7InternalType.MedLabv2_3));
        if (result is null)
        {
            return null;
        }

        if (result.IsInternal)
        {
            GetDeepForInternal(result);
        }
        else
        {
            result.hl7DefMessages = HL7DefMessages.GetDeepFromCache(result.HL7DefNum);
        }

        return result;
    }

    public static List<HL7Def> GetDeepInternalList()
    {
        var listHL7DefsInternalDb = GetListInternalFromDb();
        var results = new List<HL7Def>();

        var hl7InternalTypes = Enum.GetValues(typeof(HL7InternalType)).Cast<HL7InternalType>().ToList();
        
        foreach (var hl7InternalType in hl7InternalTypes)
        {
            var hL7Def = listHL7DefsInternalDb.Find(x => x.InternalType == hl7InternalType);
            
            switch (hl7InternalType)
            {
                case HL7InternalType.eCWFull:
                    results.Add(InternalEcwFull.GetDeepInternal(hL7Def));
                    continue;
                
                case HL7InternalType.eCWStandalone:
                    results.Add(InternalEcwStandalone.GetDeepInternal(hL7Def));
                    continue;
                
                case HL7InternalType.eCWTight:
                    results.Add(InternalEcwTight.GetDeepInternal(hL7Def));
                    continue;
                
                case HL7InternalType.Centricity:
                    results.Add(InternalCentricity.GetDeepInternal(hL7Def));
                    continue;
                
                case HL7InternalType.HL7v2_6:
                    results.Add(InternalHL7v2_6.GetDeepInternal(hL7Def));
                    continue;
                
                case HL7InternalType.MedLabv2_3:
                    results.Add(MedLabv2_3.GetDeepInternal(hL7Def));
                    continue;
                
                default:
                    continue;
            }
        }

        return results;
    }

    private static void GetDeepForInternal(HL7Def hL7Def)
    {
        switch (hL7Def.InternalType)
        {
            case HL7InternalType.eCWFull:
                InternalEcwFull.GetDeepInternal(hL7Def);
                break;
            
            case HL7InternalType.eCWStandalone:
                InternalEcwStandalone.GetDeepInternal(hL7Def);
                break;
            
            case HL7InternalType.eCWTight:
                InternalEcwTight.GetDeepInternal(hL7Def);
                break;
            
            case HL7InternalType.Centricity:
                InternalCentricity.GetDeepInternal(hL7Def);
                break;
            
            case HL7InternalType.HL7v2_6:
                InternalHL7v2_6.GetDeepInternal(hL7Def);
                break;
            
            case HL7InternalType.MedLabv2_3:
                MedLabv2_3.GetDeepInternal(hL7Def);
                break;
        }
    }

    public static bool IsExistingHL7Enabled(long hL7DefNumExclude, bool isMedLabHL7)
    {
        var commandText = "SELECT COUNT(*) FROM hl7def WHERE IsEnabled = 1 AND HL7DefNum != " + hL7DefNumExclude;

        if (isMedLabHL7)
        {
            commandText += " AND InternalType = '" + SOut.String(nameof(HL7InternalType.MedLabv2_3)) + "'";
        }
        else
        {
            commandText += " AND InternalType != '" + SOut.String(nameof(HL7InternalType.MedLabv2_3)) + "'";
        }

        return Db.GetCount(commandText) != "0";
    }

    public static bool IsExistingHL7Enabled()
    {
        return Cache.GetWhere(x => x.IsEnabled && x.InternalType != HL7InternalType.MedLabv2_3).Count > 0;
    }

    public static List<HL7Def> GetDeepCustomList()
    {
        var hl7Defs = GetShallowFromDb();

        foreach (var hl7Def in hl7Defs)
        {
            hl7Def.hl7DefMessages = HL7DefMessages.GetDeepFromDb(hl7Def.HL7DefNum);
        }

        return hl7Defs;
    }

    public static List<HL7Def> GetShallowFromDb()
    {
        return HL7DefCrud.SelectMany("SELECT * FROM hl7def WHERE IsInternal = 0");
    }

    public static long Insert(HL7Def hL7Def)
    {
        return HL7DefCrud.Insert(hL7Def);
    }

    public static void Update(HL7Def hL7Def)
    {
        HL7DefCrud.Update(hL7Def);
    }

    public static void Delete(long hL7DefNum)
    {
        Db.NonQ("DELETE FROM hl7def WHERE HL7DefNum = " + hL7DefNum);
    }

    private class HL7DefCache : CacheListAbs<HL7Def>
    {
        protected override List<HL7Def> GetCacheFromDb()
        {
            return HL7DefCrud.SelectMany("SELECT * FROM hl7def ORDER BY Description");
        }

        protected override List<HL7Def> TableToList(DataTable dataTable)
        {
            return HL7DefCrud.TableToList(dataTable);
        }

        protected override HL7Def Copy(HL7Def item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<HL7Def> items)
        {
            return HL7DefCrud.ListToTable(items, "HL7Def");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly HL7DefCache Cache = new();

    public static HL7Def GetFirstOrDefault(Func<HL7Def, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static void GetTableFromCache(bool refreshCache)
    {
        Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}