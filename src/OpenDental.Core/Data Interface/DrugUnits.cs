using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class DrugUnits
{
    ///<summary>Gets one DrugUnit from the db.</summary>
    public static DrugUnit GetOne(long drugUnitNum)
    {
        return DrugUnitCrud.SelectOne(drugUnitNum);
    }

    
    public static long Insert(DrugUnit drugUnit)
    {
        return DrugUnitCrud.Insert(drugUnit);
    }

    
    public static void Update(DrugUnit drugUnit)
    {
        DrugUnitCrud.Update(drugUnit);
    }

    ///<summary>Surround with a try/catch.  Will fail if drug unit is in use.</summary>
    public static void Delete(long drugUnitNum)
    {
        //validation
        string command;

        command = "SELECT COUNT(*) FROM vaccinepat WHERE DrugUnitNum=" + SOut.Long(drugUnitNum);
        if (Db.GetCount(command) != "0") throw new ApplicationException(Lans.g("FormDrugUnitEdit", "Cannot delete: DrugUnit is in use by VaccinePat."));

        command = "DELETE FROM drugunit WHERE DrugUnitNum = " + SOut.Long(drugUnitNum);
        Db.NonQ(command);
    }

    #region CachePattern

    private class DrugUnitCache : CacheListAbs<DrugUnit>
    {
        protected override List<DrugUnit> GetCacheFromDb()
        {
            var command = "SELECT * FROM drugunit ORDER BY UnitIdentifier";
            return DrugUnitCrud.SelectMany(command);
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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly DrugUnitCache _drugUnitCache = new();

    public static bool GetExists(Predicate<DrugUnit> match, bool isShort = false)
    {
        return _drugUnitCache.GetExists(match, isShort);
    }

    public static List<DrugUnit> GetDeepCopy(bool isShort = false)
    {
        return _drugUnitCache.GetDeepCopy(isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _drugUnitCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _drugUnitCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _drugUnitCache.ClearCache();
    }

    #endregion
}