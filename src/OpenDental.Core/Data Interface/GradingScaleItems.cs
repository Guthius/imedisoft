using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class GradingScaleItems
{
    //If this table type will exist as cached data, uncomment the CachePattern region below and edit.
    /*
    #region CachePattern

    private class GradingScaleItemCache : CacheListAbs<GradingScaleItem> {
        protected override List<GradingScaleItem> GetCacheFromDb() {
            string command="SELECT * FROM GradingScaleItem ORDER BY ItemOrder";
            return Crud.GradingScaleItemCrud.SelectMany(command);
        }
        protected override List<GradingScaleItem> TableToList(DataTable table) {
            return Crud.GradingScaleItemCrud.TableToList(table);
        }
        protected override GradingScaleItem Copy(GradingScaleItem GradingScaleItem) {
            return GradingScaleItem.Clone();
        }
        protected override DataTable ListToTable(List<GradingScaleItem> listGradingScaleItems) {
            return Crud.GradingScaleItemCrud.ListToTable(listGradingScaleItems,"GradingScaleItem");
        }
        protected override void FillCacheIfNeeded() {
            GradingScaleItems.GetTableFromCache(false);
        }
        protected override bool IsInListShort(GradingScaleItem GradingScaleItem) {
            return !GradingScaleItem.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static GradingScaleItemCache _GradingScaleItemCache=new GradingScaleItemCache();

    ///<summary>A list of all GradingScaleItems. Returns a deep copy.</summary>
    public static List<GradingScaleItem> ListDeep {
        get {
            return _GradingScaleItemCache.ListDeep;
        }
    }

    ///<summary>A list of all visible GradingScaleItems. Returns a deep copy.</summary>
    public static List<GradingScaleItem> ListShortDeep {
        get {
            return _GradingScaleItemCache.ListShortDeep;
        }
    }

    ///<summary>A list of all GradingScaleItems. Returns a shallow copy.</summary>
    public static List<GradingScaleItem> ListShallow {
        get {
            return _GradingScaleItemCache.ListShallow;
        }
    }

    ///<summary>A list of all visible GradingScaleItems. Returns a shallow copy.</summary>
    public static List<GradingScaleItem> ListShort {
        get {
            return _GradingScaleItemCache.ListShallowShort;
        }
    }

    ///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _GradingScaleItemCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _GradingScaleItemCache.GetTableFromCache(doRefreshCache);
    }

    #endregion
    */

    ///<summary>Gets all grading scale items ordered by GradeNumber descending.</summary>
    public static List<GradingScaleItem> Refresh(long gradingScaleNum)
    {
        var command = "SELECT * FROM gradingscaleitem WHERE GradingScaleNum = " + SOut.Long(gradingScaleNum)
                                                                                + " ORDER BY GradeNumber DESC";
        return GradingScaleItemCrud.SelectMany(command);
    }

    ///<summary>Gets one GradingScaleItem from the db.</summary>
    public static GradingScaleItem GetOne(long gradingScaleItemNum)
    {
        return GradingScaleItemCrud.SelectOne(gradingScaleItemNum);
    }

    
    public static long Insert(GradingScaleItem gradingScaleItem)
    {
        return GradingScaleItemCrud.Insert(gradingScaleItem);
    }

    
    public static void Update(GradingScaleItem gradingScaleItem)
    {
        GradingScaleItemCrud.Update(gradingScaleItem);
    }

    
    public static void DeleteAllByGradingScale(long gradingScaleNum)
    {
        var command = "DELETE FROM gradingscaleitem WHERE GradingScaleNum = " + SOut.Long(gradingScaleNum);
        Db.NonQ(command);
    }

    
    public static void Delete(long gradingScaleItemNum)
    {
        var command = "DELETE FROM gradingscaleitem WHERE GradingScaleItemNum = " + SOut.Long(gradingScaleItemNum);
        Db.NonQ(command);
    }
}