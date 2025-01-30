using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class DrugManufacturers
{
    public static DrugManufacturer GetOne(long drugManufacturerNum)
    {
        return DrugManufacturerCrud.SelectOne(drugManufacturerNum);
    }

    public static void Insert(DrugManufacturer drugManufacturer)
    {
        DrugManufacturerCrud.Insert(drugManufacturer);
    }

    public static void Update(DrugManufacturer drugManufacturer)
    {
        DrugManufacturerCrud.Update(drugManufacturer);
    }

    public static void Delete(long drugManufacturerNum)
    {
        var command = "SELECT COUNT(*) FROM vaccinedef WHERE DrugManufacturerNum=" + drugManufacturerNum;
        if (Db.GetCount(command) != "0")
        {
            throw new ApplicationException("Cannot delete: DrugManufacturer is in use by VaccineDef.");
        }

        Db.NonQ("DELETE FROM drugmanufacturer WHERE DrugManufacturerNum = " + drugManufacturerNum);
    }

    private class DrugManufacturerCache : CacheListAbs<DrugManufacturer>
    {
        protected override List<DrugManufacturer> GetCacheFromDb()
        {
            return DrugManufacturerCrud.SelectMany("SELECT * FROM drugmanufacturer ORDER BY ManufacturerCode");
        }

        protected override List<DrugManufacturer> TableToList(DataTable dataTable)
        {
            return DrugManufacturerCrud.TableToList(dataTable);
        }

        protected override DrugManufacturer Copy(DrugManufacturer item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<DrugManufacturer> items)
        {
            return DrugManufacturerCrud.ListToTable(items, "DrugManufacturer");
        }

        protected override void FillCacheIfNeeded()
        {
            DrugManufacturers.GetTableFromCache(false);
        }
    }

    private static readonly DrugManufacturerCache Cache = new();

    public static bool GetExists(Predicate<DrugManufacturer> predicate, bool shortList = false)
    {
        return Cache.GetExists(predicate, shortList);
    }

    public static List<DrugManufacturer> GetDeepCopy(bool shortList = false)
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