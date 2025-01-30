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
        var command = "SELECT * FROM hl7def WHERE IsInternal=1 "
                      + "AND InternalType='" + SOut.String(hL7InternalType.ToString()) + "'";
        return HL7DefCrud.SelectOne(command);
    }

    public static List<HL7Def> GetListInternalFromDb()
    {
        var command = "SELECT * FROM hl7def WHERE IsInternal=1";
        return HL7DefCrud.SelectMany(command);
    }

    public static HL7Def GetOneDeepEnabled()
    {
        return GetOneDeepEnabled(false);
    }

    public static HL7Def GetOneDeepEnabled(bool isMedLabHL7)
    {
        var hL7DefRet = GetFirstOrDefault(x => x.IsEnabled && isMedLabHL7 == (x.InternalType == HL7InternalType.MedLabv2_3));
        if (hL7DefRet == null) return null;
        if (hL7DefRet.IsInternal) //if internal, messages, segments, and fields will not be in the database
            GetDeepForInternal(hL7DefRet);
        else
            hL7DefRet.hl7DefMessages = HL7DefMessages.GetDeepFromCache(hL7DefRet.HL7DefNum);
        return hL7DefRet;
    }

    public static List<HL7Def> GetDeepInternalList()
    {
        var listHL7DefsInternalDb = GetListInternalFromDb();
        var listHL7DefsRet = new List<HL7Def>();
        HL7Def hL7Def;
        //Whether or not the def was in the db, internal def messages, segments, and fields will not be in the db.  GetDeep from C# code
        var listHL7InternalTypes = Enum.GetValues(typeof(HL7InternalType)).Cast<HL7InternalType>().ToList();
        for (var i = 0; i < listHL7InternalTypes.Count; i++)
        {
            hL7Def = listHL7DefsInternalDb.Find(x => x.InternalType == listHL7InternalTypes[i]); //might be null
            switch (listHL7InternalTypes[i])
            {
                case HL7InternalType.eCWFull:
                    listHL7DefsRet.Add(InternalEcwFull.GetDeepInternal(hL7Def));
                    continue;
                case HL7InternalType.eCWStandalone:
                    listHL7DefsRet.Add(InternalEcwStandalone.GetDeepInternal(hL7Def));
                    continue;
                case HL7InternalType.eCWTight:
                    listHL7DefsRet.Add(InternalEcwTight.GetDeepInternal(hL7Def));
                    continue;
                case HL7InternalType.Centricity:
                    listHL7DefsRet.Add(InternalCentricity.GetDeepInternal(hL7Def));
                    continue;
                case HL7InternalType.HL7v2_6:
                    listHL7DefsRet.Add(InternalHL7v2_6.GetDeepInternal(hL7Def));
                    continue;
                case HL7InternalType.MedLabv2_3:
                    listHL7DefsRet.Add(MedLabv2_3.GetDeepInternal(hL7Def));
                    continue;
                default:
                    continue;
            }
        }

        return listHL7DefsRet;
    }

    private static void GetDeepForInternal(HL7Def hL7Def)
    {
        if (hL7Def.InternalType == HL7InternalType.eCWFull)
            hL7Def = InternalEcwFull.GetDeepInternal(hL7Def); //def that we're passing in is guaranteed to not be null
        else if (hL7Def.InternalType == HL7InternalType.eCWStandalone)
            hL7Def = InternalEcwStandalone.GetDeepInternal(hL7Def);
        else if (hL7Def.InternalType == HL7InternalType.eCWTight)
            hL7Def = InternalEcwTight.GetDeepInternal(hL7Def);
        else if (hL7Def.InternalType == HL7InternalType.Centricity)
            hL7Def = InternalCentricity.GetDeepInternal(hL7Def);
        else if (hL7Def.InternalType == HL7InternalType.HL7v2_6)
            hL7Def = InternalHL7v2_6.GetDeepInternal(hL7Def);
        else if (hL7Def.InternalType == HL7InternalType.MedLabv2_3) hL7Def = MedLabv2_3.GetDeepInternal(hL7Def);
        //no need to return a def because the original reference won't have been lost.
    }

    public static bool IsExistingHL7Enabled(long hL7DefNumExclude, bool isMedLabHL7)
    {
        var command = "SELECT COUNT(*) FROM hl7def WHERE IsEnabled=1 AND HL7DefNum != " + SOut.Long(hL7DefNumExclude);
        if (isMedLabHL7)
            command += " AND InternalType='" + SOut.String(HL7InternalType.MedLabv2_3.ToString()) + "'";
        else
            command += " AND InternalType!='" + SOut.String(HL7InternalType.MedLabv2_3.ToString()) + "'";
        if (Db.GetCount(command) == "0") return false;
        return true;
    }

    public static bool IsExistingHL7Enabled()
    {
        return Cache.GetWhere(x => x.IsEnabled && x.InternalType != HL7InternalType.MedLabv2_3).Count > 0;
    }

    public static List<HL7Def> GetDeepCustomList()
    {
        var listHL7Defs = GetShallowFromDb();
        for (var d = 0; d < listHL7Defs.Count; d++) listHL7Defs[d].hl7DefMessages = HL7DefMessages.GetDeepFromDb(listHL7Defs[d].HL7DefNum);
        return listHL7Defs;
    }

    public static List<HL7Def> GetShallowFromDb()
    {
        var command = "SELECT * FROM hl7def WHERE IsInternal=0";
        return HL7DefCrud.SelectMany(command);
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
        var command = "DELETE FROM hl7def WHERE HL7DefNum = " + SOut.Long(hL7DefNum);
        Db.NonQ(command);
    }
    
    private class HL7DefCache : CacheListAbs<HL7Def>
    {
        protected override List<HL7Def> GetCacheFromDb()
        {
            var command = "SELECT * FROM hl7def ORDER BY Description";
            return HL7DefCrud.SelectMany(command);
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
            HL7Defs.GetTableFromCache(false);
        }
    }
    
    private static readonly HL7DefCache Cache = new();

    public static HL7Def GetFirstOrDefault(Func<HL7Def, bool> match, bool isShort = false)
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