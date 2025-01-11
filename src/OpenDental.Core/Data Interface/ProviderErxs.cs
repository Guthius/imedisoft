using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ProviderErxs
{
    
    public static void Delete(long providerErxNum)
    {
        ProviderErxCrud.Delete(providerErxNum);
    }

    ///<summary>Gets from db.  Used from FormErxAccess at HQ only.</summary>
    public static List<ProviderErx> Refresh(long patNum)
    {
        var command = "SELECT * FROM providererx WHERE PatNum = " + SOut.Long(patNum) + " ORDER BY NationalProviderID";
        return ProviderErxCrud.SelectMany(command);
    }

    
    public static ProviderErx GetOne(long provErxNum)
    {
        var command = "SELECT * FROM providererx WHERE ProviderErxNum = " + SOut.Long(provErxNum);
        return ProviderErxCrud.SelectOne(command);
    }

    ///<summary>Gets one ProviderErx from the cache.</summary>
    public static ProviderErx GetOneForNpiAndOption(string npi, ErxOption erxOption)
    {
        return GetFirstOrDefault(x => x.NationalProviderID == npi && x.ErxType == erxOption);
    }

    ///<summary>Gets all ProviderErx which have not yet been sent to HQ.</summary>
    public static List<ProviderErx> GetAllUnsent()
    {
        return GetWhere(x => !x.IsSentToHq);
    }

    ///<summary>This should only be used for ODHQ.  Gets all account ids associated to the patient account.</summary>
    public static List<string> GetAccountIdsForPatNum(long patNum)
    {
        return GetDeepCopy().FindAll(x => x.PatNum == patNum && !string.IsNullOrWhiteSpace(x.AccountId))
            .Select(x => x.AccountId)
            .ToList();
    }

    ///<summary>Returns all ProviderErx rows that have an exact match of the passed in National Provider ID.</summary>
    public static List<ProviderErx> GetAllForNPI(string npi)
    {
        return GetDeepCopy().FindAll(x => x.NationalProviderID == npi);
    }

    
    public static long Insert(ProviderErx providerErx)
    {
        return ProviderErxCrud.Insert(providerErx);
    }

    
    public static void Update(ProviderErx providerErx)
    {
        ProviderErxCrud.Update(providerErx);
    }

    
    public static bool Update(ProviderErx providerErx, ProviderErx oldProviderErx)
    {
        return ProviderErxCrud.Update(providerErx, oldProviderErx);
    }

    ///<summary>Inserts, updates, or deletes the passed in list verses the old list.  Returns true if db changes were made.</summary>
    public static bool Sync(List<ProviderErx> listNew, List<ProviderErx> listOld)
    {
        return ProviderErxCrud.Sync(listNew, listOld);
    }

    #region CachePattern

    private class ProviderErxCache : CacheListAbs<ProviderErx>
    {
        protected override List<ProviderErx> GetCacheFromDb()
        {
            var command = "SELECT * FROM providererx ORDER BY NationalProviderID";
            return ProviderErxCrud.SelectMany(command);
        }

        protected override List<ProviderErx> TableToList(DataTable dataTable)
        {
            return ProviderErxCrud.TableToList(dataTable);
        }

        protected override ProviderErx Copy(ProviderErx item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<ProviderErx> items)
        {
            return ProviderErxCrud.ListToTable(items, "ProviderErx");
        }

        protected override void FillCacheIfNeeded()
        {
            ProviderErxs.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ProviderErxCache _providerErxCache = new();

    public static List<ProviderErx> GetDeepCopy(bool isShort = false)
    {
        return _providerErxCache.GetDeepCopy(isShort);
    }

    public static int GetCount(bool isShort = false)
    {
        return _providerErxCache.GetCount(isShort);
    }

    public static int GetFindIndex(Predicate<ProviderErx> match, bool isShort = false)
    {
        return _providerErxCache.GetFindIndex(match, isShort);
    }

    public static ProviderErx GetFirstOrDefault(Func<ProviderErx, bool> match, bool isShort = false)
    {
        return _providerErxCache.GetFirstOrDefault(match, isShort);
    }

    public static List<ProviderErx> GetWhere(Predicate<ProviderErx> match, bool isShort = false)
    {
        return _providerErxCache.GetWhere(match, isShort);
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
        _providerErxCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _providerErxCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _providerErxCache.ClearCache();
    }

    #endregion

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static void Update(ProviderErx providerErx) {

        Crud.ProviderErxCrud.Update(providerErx);
    }

    ///<summary>Gets one ProviderErx from the db.</summary>
    public static ProviderErx GetOne(long providerErxNum){

        return Crud.ProviderErxCrud.SelectOne(providerErxNum);
    }
    */
}