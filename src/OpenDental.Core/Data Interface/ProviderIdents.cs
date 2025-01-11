using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

///<summary>Refreshed with local data.</summary>
public class ProviderIdents
{
    
    public static void Update(ProviderIdent pi)
    {
        ProviderIdentCrud.Update(pi);
    }

    
    public static long Insert(ProviderIdent pi)
    {
        return ProviderIdentCrud.Insert(pi);
    }

    
    public static void Delete(ProviderIdent pi)
    {
        var command = "DELETE FROM providerident "
                      + "WHERE ProviderIdentNum = " + SOut.Long(pi.ProviderIdentNum);
        Db.NonQ(command);
    }

    ///<summary>Gets all supplemental identifiers that have been attached to this provider. Used in the provider edit window.</summary>
    public static ProviderIdent[] GetForProv(long provNum)
    {
        return GetWhere(x => x.ProvNum == provNum).ToArray();
    }

    /// <summary>
    ///     Gets all supplemental identifiers that have been attached to this provider and for this particular payorID.
    ///     Called from X12 when creating a claim file.  Also used now on printed claims.
    /// </summary>
    public static ProviderIdent[] GetForPayor(long provNum, string payorID)
    {
        return GetWhere(x => x.ProvNum == provNum && x.PayorID == payorID).ToArray();
    }

    ///<summary>Called from FormProvEdit if cancel on a new provider.</summary>
    public static void DeleteAllForProv(long provNum)
    {
        var command = "DELETE from providerident WHERE provnum = '" + SOut.Long(provNum) + "'";
        Db.NonQ(command);
    }

    /// <summary></summary>
    public static bool IdentExists(ProviderSupplementalID type, long provNum, string payorID)
    {
        var providerIdent = GetFirstOrDefault(x => x.ProvNum == provNum && x.SuppIDType == type && x.PayorID == payorID);
        return providerIdent != null;
    }

    #region CachePattern

    private class ProviderIdentCache : CacheListAbs<ProviderIdent>
    {
        protected override List<ProviderIdent> GetCacheFromDb()
        {
            var command = "SELECT * FROM providerident";
            return ProviderIdentCrud.SelectMany(command);
        }

        protected override List<ProviderIdent> TableToList(DataTable dataTable)
        {
            return ProviderIdentCrud.TableToList(dataTable);
        }

        protected override ProviderIdent Copy(ProviderIdent item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ProviderIdent> items)
        {
            return ProviderIdentCrud.ListToTable(items, "ProviderIdent");
        }

        protected override void FillCacheIfNeeded()
        {
            ProviderIdents.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ProviderIdentCache _ProviderIdentCache = new();

    public static ProviderIdent GetFirstOrDefault(Func<ProviderIdent, bool> match, bool isShort = false)
    {
        return _ProviderIdentCache.GetFirstOrDefault(match, isShort);
    }

    public static List<ProviderIdent> GetWhere(Predicate<ProviderIdent> match, bool isShort = false)
    {
        return _ProviderIdentCache.GetWhere(match, isShort);
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
        _ProviderIdentCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _ProviderIdentCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _ProviderIdentCache.ClearCache();
    }

    #endregion
}