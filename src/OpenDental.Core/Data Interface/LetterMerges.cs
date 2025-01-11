using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class LetterMerges
{
    ///<summary>Inserts this lettermerge into database.</summary>
    public static long Insert(LetterMerge letterMerge)
    {
        return LetterMergeCrud.Insert(letterMerge);
    }

    
    public static void Update(LetterMerge letterMerge)
    {
        LetterMergeCrud.Update(letterMerge);
    }

    
    public static void Delete(LetterMerge letterMerge)
    {
        var command = "DELETE FROM lettermerge "
                      + "WHERE LetterMergeNum = " + SOut.Long(letterMerge.LetterMergeNum);
        Db.NonQ(command);
    }

    ///<summary>Supply the index of the cat within Defs.Short.</summary>
    public static List<LetterMerge> GetListForCat(int catIndex)
    {
        var defNum = Defs.GetDefsForCategory(DefCat.LetterMergeCats, true)[catIndex].DefNum;
        return GetWhere(x => x.Category == defNum);
    }

    #region CachePattern

    private class LetterMergeCache : CacheListAbs<LetterMerge>
    {
        protected override List<LetterMerge> GetCacheFromDb()
        {
            var command = "SELECT * FROM lettermerge ORDER BY Description";
            var listLetterMerges = LetterMergeCrud.SelectMany(command);
            return listLetterMerges;
        }

        protected override List<LetterMerge> TableToList(DataTable dataTable)
        {
            return LetterMergeCrud.TableToList(dataTable);
        }

        protected override LetterMerge Copy(LetterMerge item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<LetterMerge> items)
        {
            return LetterMergeCrud.ListToTable(items, "LetterMerge");
        }

        protected override void FillCacheIfNeeded()
        {
            LetterMerges.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly LetterMergeCache _letterMergeCache = new();

    public static List<LetterMerge> GetWhere(Predicate<LetterMerge> match, bool isShort = false)
    {
        return _letterMergeCache.GetWhere(match, isShort);
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
        _letterMergeCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _letterMergeCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _letterMergeCache.ClearCache();
    }

    #endregion
}