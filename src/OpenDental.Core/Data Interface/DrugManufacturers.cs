using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class DrugManufacturers
{
    ///<summary>Gets one DrugManufacturer from the db.</summary>
    public static DrugManufacturer GetOne(long drugManufacturerNum)
    {
        return DrugManufacturerCrud.SelectOne(drugManufacturerNum);
    }

    
    public static long Insert(DrugManufacturer drugManufacturer)
    {
        return DrugManufacturerCrud.Insert(drugManufacturer);
    }

    
    public static void Update(DrugManufacturer drugManufacturer)
    {
        DrugManufacturerCrud.Update(drugManufacturer);
    }

    
    public static void Delete(long drugManufacturerNum)
    {
        //validation
        string command;
        command = "SELECT COUNT(*) FROM vaccinedef WHERE DrugManufacturerNum=" + SOut.Long(drugManufacturerNum);
        if (Db.GetCount(command) != "0") throw new ApplicationException(Lans.g("FormDrugUnitEdit", "Cannot delete: DrugManufacturer is in use by VaccineDef."));
        command = "DELETE FROM drugmanufacturer WHERE DrugManufacturerNum = " + SOut.Long(drugManufacturerNum);
        Db.NonQ(command);
    }

    #region CachePattern

    private class DrugManufacturerCache : CacheListAbs<DrugManufacturer>
    {
        protected override List<DrugManufacturer> GetCacheFromDb()
        {
            var command = "SELECT * FROM drugmanufacturer ORDER BY ManufacturerCode";
            return DrugManufacturerCrud.SelectMany(command);
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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly DrugManufacturerCache _drugManufacturerCache = new();

    public static bool GetExists(Predicate<DrugManufacturer> match, bool isShort = false)
    {
        return _drugManufacturerCache.GetExists(match, isShort);
    }

    public static List<DrugManufacturer> GetDeepCopy(bool isShort = false)
    {
        return _drugManufacturerCache.GetDeepCopy(isShort);
    }

    public static DrugManufacturer GetFirstOrDefault(Func<DrugManufacturer, bool> match, bool isShort = false)
    {
        return _drugManufacturerCache.GetFirstOrDefault(match, isShort);
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
        _drugManufacturerCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _drugManufacturerCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _drugManufacturerCache.ClearCache();
    }

    #endregion

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<DrugManufacturer> Refresh(long patNum){

        string command="SELECT * FROM drugmanufacturer WHERE PatNum = "+POut.Long(patNum);
        return Crud.DrugManufacturerCrud.SelectMany(command);
    }
    */
}