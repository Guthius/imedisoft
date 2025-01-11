using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

///<summary>Letters are refreshed as local data.</summary>
public class Letters
{
    
    public static void Update(Letter letter)
    {
        LetterCrud.Update(letter);
    }

    
    public static long Insert(Letter letter)
    {
        return LetterCrud.Insert(letter);
    }

    
    public static void Delete(Letter letter)
    {
        var command = "DELETE from letter WHERE LetterNum = '" + letter.LetterNum + "'";
        Db.NonQ(command);
    }

    #region CachePattern

    private class LetterCache : CacheListAbs<Letter>
    {
        protected override List<Letter> GetCacheFromDb()
        {
            var command = "SELECT * from letter ORDER BY Description";
            return LetterCrud.SelectMany(command);
        }

        protected override List<Letter> TableToList(DataTable dataTable)
        {
            return LetterCrud.TableToList(dataTable);
        }

        protected override Letter Copy(Letter item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Letter> items)
        {
            return LetterCrud.ListToTable(items, "Letter");
        }

        protected override void FillCacheIfNeeded()
        {
            Letters.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly LetterCache _LetterCache = new();

    public static List<Letter> GetDeepCopy(bool isShort = false)
    {
        return _LetterCache.GetDeepCopy(isShort);
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
        _LetterCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _LetterCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _LetterCache.ClearCache();
    }

    #endregion
}