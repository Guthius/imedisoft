using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class VaccineDefs
{
    ///<summary>Gets one VaccineDef from the db.</summary>
    public static VaccineDef GetOne(long vaccineDefNum)
    {
        return VaccineDefCrud.SelectOne(vaccineDefNum);
    }

    
    public static long Insert(VaccineDef vaccineDef)
    {
        return VaccineDefCrud.Insert(vaccineDef);
    }

    
    public static void Update(VaccineDef vaccineDef)
    {
        VaccineDefCrud.Update(vaccineDef);
    }

    
    public static void Delete(long vaccineDefNum)
    {
        //validation
        string command;
        command = "SELECT COUNT(*) FROM VaccinePat WHERE VaccineDefNum=" + SOut.Long(vaccineDefNum);
        if (Db.GetCount(command) != "0") throw new ApplicationException(Lans.g("FormDrugUnitEdit", "Cannot delete: VaccineDef is in use by VaccinePat."));

        command = "DELETE FROM vaccinedef WHERE VaccineDefNum = " + SOut.Long(vaccineDefNum);
        Db.NonQ(command);
    }

    #region CachePattern

    private class VaccineDefCache : CacheListAbs<VaccineDef>
    {
        protected override List<VaccineDef> GetCacheFromDb()
        {
            var command = "SELECT * FROM vaccinedef ORDER BY CVXCode";
            return VaccineDefCrud.SelectMany(command);
        }

        protected override List<VaccineDef> TableToList(DataTable dataTable)
        {
            return VaccineDefCrud.TableToList(dataTable);
        }

        protected override VaccineDef Copy(VaccineDef item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<VaccineDef> items)
        {
            return VaccineDefCrud.ListToTable(items, "VaccineDef");
        }

        protected override void FillCacheIfNeeded()
        {
            VaccineDefs.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly VaccineDefCache _vaccineDefCache = new();

    public static bool GetExists(Predicate<VaccineDef> match, bool isShort = false)
    {
        return _vaccineDefCache.GetExists(match, isShort);
    }

    public static VaccineDef GetFirstOrDefault(Func<VaccineDef, bool> match, bool isShort = false)
    {
        return _vaccineDefCache.GetFirstOrDefault(match, isShort);
    }

    public static List<VaccineDef> GetDeepCopy(bool isShort = false)
    {
        return _vaccineDefCache.GetDeepCopy(isShort);
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
        _vaccineDefCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return _vaccineDefCache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        _vaccineDefCache.ClearCache();
    }

    #endregion

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<VaccineDef> Refresh(long patNum){

        string command="SELECT * FROM vaccinedef WHERE PatNum = "+POut.Long(patNum);
        return Crud.VaccineDefCrud.SelectMany(command);
    }
    */
}