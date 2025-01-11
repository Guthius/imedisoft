using System;
using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Ebills
{
    ///<summary>To get the defaults, use clinicNum=0.</summary>
    public static Ebill GetForClinic(long clinicNum)
    {
        return GetFirstOrDefault(x => x.ClinicNum == clinicNum);
    }

    public static bool Sync(List<Ebill> listEbillsNew, List<Ebill> listEbillsOld)
    {
        return EbillCrud.Sync(listEbillsNew, listEbillsOld);
    }

    #region CachePattern

    private class EbillCache : CacheListAbs<Ebill>
    {
        protected override List<Ebill> GetCacheFromDb()
        {
            var command = "SELECT * FROM ebill";
            return EbillCrud.SelectMany(command);
        }

        protected override List<Ebill> TableToList(DataTable dataTable)
        {
            return EbillCrud.TableToList(dataTable);
        }

        protected override Ebill Copy(Ebill item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Ebill> items)
        {
            return EbillCrud.ListToTable(items, "Ebill");
        }

        protected override void FillCacheIfNeeded()
        {
            Ebills.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly EbillCache _EbillCache = new();

    public static List<Ebill> GetDeepCopy(bool isShort = false)
    {
        return _EbillCache.GetDeepCopy(isShort);
    }

    public static Ebill GetFirstOrDefault(Func<Ebill, bool> match, bool isShort = false)
    {
        return _EbillCache.GetFirstOrDefault(match, isShort);
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
        _EbillCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _EbillCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _EbillCache.ClearCache();
    }

    #endregion
}