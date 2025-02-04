using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class MountDefs
{
    public static void Update(MountDef mountDef)
    {
        MountDefCrud.Update(mountDef);
    }

    public static void Insert(MountDef mountDef)
    {
        MountDefCrud.Insert(mountDef);
    }

    public static void Delete(long mountDefNum)
    {
        Db.NonQ("DELETE FROM mountdef WHERE MountDefNum = " + mountDefNum);
        Db.NonQ("DELETE FROM mountitemdef WHERE MountDefNum = " + mountDefNum);
    }

    public static string SetScale(float scale, int decimals, string units)
    {
        var result = scale + " " + decimals;
        
        if (!string.IsNullOrEmpty(units))
        {
            result += " " + units;
        }
        
        return result;
    }

    public static float GetScale(string scaleValue)
    {
        if (scaleValue is null)
        {
            return 0;
        }
        
        var tokens = scaleValue.Split(' ').ToList();
        if (tokens.Count > 0)
        {
            return SIn.Float(tokens[0]);
        }
        
        return 0;
    }

    public static int GetDecimals(string scaleValue)
    {
        if (scaleValue is null)
        {
            return 0;
        }
        
        var tokens = scaleValue.Split(' ').ToList();
        
        return tokens.Count > 1 ? SIn.Int(tokens[1]) : 0;
    }

    public static string GetScaleUnits(string scaleValue)
    {
        if (scaleValue is null)
        {
            return string.Empty;
        }
        
        var tokens = scaleValue.Split(' ').ToList();
        
        return tokens.Count == 3 ? tokens[2] : "";
    }

    private class MountDefCache : CacheListAbs<MountDef>
    {
        protected override List<MountDef> GetCacheFromDb()
        {
            return MountDefCrud.SelectMany("SELECT * FROM mountdef ORDER BY ItemOrder");
        }

        protected override List<MountDef> TableToList(DataTable dataTable)
        {
            return MountDefCrud.TableToList(dataTable);
        }

        protected override MountDef Copy(MountDef item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<MountDef> items)
        {
            return MountDefCrud.ListToTable(items, "MountDef");
        }

        protected override void FillCacheIfNeeded()
        {
            MountDefs.GetTableFromCache(false);
        }
    }

    private static readonly MountDefCache Cache = new();

    public static List<MountDef> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}