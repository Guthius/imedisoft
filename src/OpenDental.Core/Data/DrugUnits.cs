using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class DrugUnits
{
    public static DrugUnit GetOne(long drugUnitNum)
    {
        return DrugUnitCrud.SelectOne(drugUnitNum);
    }
    
    public static void Insert(DrugUnit drugUnit)
    {
        DrugUnitCrud.Insert(drugUnit);
    }
    
    public static void Update(DrugUnit drugUnit)
    {
        DrugUnitCrud.Update(drugUnit);
    }

    public static void Delete(long drugUnitNum)
    {
        var command = "SELECT COUNT(*) FROM vaccinepat WHERE DrugUnitNum=" + drugUnitNum;
        if (Db.GetCount(command) != "0")
        {
            throw new ApplicationException("Cannot delete: DrugUnit is in use by VaccinePat.");
        }

        Db.NonQ("DELETE FROM drugunit WHERE DrugUnitNum = " + drugUnitNum);
    }
    
    private class DrugUnitCache : CacheListAbs<DrugUnit>
    {
        protected override List<DrugUnit> GetCacheFromDb()
        {
            return DrugUnitCrud.SelectMany("SELECT * FROM drugunit ORDER BY UnitIdentifier");
        }

        protected override List<DrugUnit> TableToList(DataTable dataTable)
        {
            return DrugUnitCrud.TableToList(dataTable);
        }

        protected override DrugUnit Copy(DrugUnit item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<DrugUnit> items)
        {
            return DrugUnitCrud.ListToTable(items, "DrugUnit");
        }

        protected override void FillCacheIfNeeded()
        {
            DrugUnits.GetTableFromCache(false);
        }
    }

    private static readonly DrugUnitCache Cache = new();

    public static bool GetExists(Predicate<DrugUnit> match, bool isShort = false)
    {
        return Cache.GetExists(match, isShort);
    }

    public static List<DrugUnit> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
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