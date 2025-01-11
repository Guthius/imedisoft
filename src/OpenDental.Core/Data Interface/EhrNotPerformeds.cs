using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EhrNotPerformeds
{
    //If this table type will exist as cached data, uncomment the CachePattern region below.
    /*
    #region CachePattern

    private class EhrLabNotPerformedCache : CacheListAbs<EhrLabNotPerformed> {
        protected override List<EhrLabNotPerformed> GetCacheFromDb() {
            string command="SELECT * FROM EhrLabNotPerformed ORDER BY ItemOrder";
            return Crud.EhrLabNotPerformedCrud.SelectMany(command);
        }
        protected override List<EhrLabNotPerformed> TableToList(DataTable table) {
            return Crud.EhrLabNotPerformedCrud.TableToList(table);
        }
        protected override EhrLabNotPerformed Copy(EhrLabNotPerformed EhrLabNotPerformed) {
            return EhrLabNotPerformed.Clone();
        }
        protected override DataTable ListToTable(List<EhrLabNotPerformed> listEhrLabNotPerformeds) {
            return Crud.EhrLabNotPerformedCrud.ListToTable(listEhrLabNotPerformeds,"EhrLabNotPerformed");
        }
        protected override void FillCacheIfNeeded() {
            EhrLabNotPerformeds.GetTableFromCache(false);
        }
        protected override bool IsInListShort(EhrLabNotPerformed EhrLabNotPerformed) {
            return !EhrLabNotPerformed.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static EhrLabNotPerformedCache _EhrLabNotPerformedCache=new EhrLabNotPerformedCache();

    ///<summary>A list of all EhrLabNotPerformeds. Returns a deep copy.</summary>
    public static List<EhrLabNotPerformed> ListDeep {
        get {
            return _EhrLabNotPerformedCache.ListDeep;
        }
    }

    ///<summary>A list of all visible EhrLabNotPerformeds. Returns a deep copy.</summary>
    public static List<EhrLabNotPerformed> ListShortDeep {
        get {
            return _EhrLabNotPerformedCache.ListShortDeep;
        }
    }

    ///<summary>A list of all EhrLabNotPerformeds. Returns a shallow copy.</summary>
    public static List<EhrLabNotPerformed> ListShallow {
        get {
            return _EhrLabNotPerformedCache.ListShallow;
        }
    }

    ///<summary>A list of all visible EhrLabNotPerformeds. Returns a shallow copy.</summary>
    public static List<EhrLabNotPerformed> ListShort {
        get {
            return _EhrLabNotPerformedCache.ListShallowShort;
        }
    }

    ///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _EhrLabNotPerformedCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _EhrLabNotPerformedCache.GetTableFromCache(doRefreshCache);
    }

    #endregion
    */

    
    public static List<EhrNotPerformed> Refresh(long patNum)
    {
        var command = "SELECT * FROM ehrnotperformed WHERE PatNum = " + SOut.Long(patNum) + " ORDER BY DateEntry";
        return EhrNotPerformedCrud.SelectMany(command);
    }

    
    public static long Insert(EhrNotPerformed ehrNotPerformed)
    {
        return EhrNotPerformedCrud.Insert(ehrNotPerformed);
    }

    
    public static void Update(EhrNotPerformed ehrNotPerformed)
    {
        EhrNotPerformedCrud.Update(ehrNotPerformed);
    }

    
    public static void Delete(long ehrNotPerformedNum)
    {
        var command = "DELETE FROM ehrnotperformed WHERE EhrNotPerformedNum = " + SOut.Long(ehrNotPerformedNum);
        Db.NonQ(command);
    }

    ///<summary>Gets one EhrNotPerformed from the db.</summary>
    public static EhrNotPerformed GetOne(long ehrNotPerformedNum)
    {
        return EhrNotPerformedCrud.SelectOne(ehrNotPerformedNum);
    }
}