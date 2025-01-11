using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ChildParents
{
    //If this table type will exist as cached data, uncomment the Cache Pattern region below and edit.
    /*
    #region Cache Pattern
    //This region can be eliminated if this is not a table type with cached data.
    //If leaving this region in place, be sure to add GetTableFromCache and FillCacheFromTable to the Cache.cs file with all the other Cache types.
    //Also, consider making an invalid type for this class in Cache.GetAllCachedInvalidTypes() if needed.

    private class ChildParentCache : CacheListAbs<ChildParent> {
        protected override List<ChildParent> GetCacheFromDb() {
            string command="SELECT * FROM childparent";
            return Crud.ChildParentCrud.SelectMany(command);
        }
        protected override List<ChildParent> TableToList(DataTable table) {
            return Crud.ChildParentCrud.TableToList(table);
        }
        protected override ChildParent Copy(ChildParent childParent) {
            return childParent.Copy();
        }
        protected override DataTable ListToTable(List<ChildParent> listChildParents) {
            return Crud.ChildParentCrud.ListToTable(listChildParents,"ChildParent");
        }
        protected override void FillCacheIfNeeded() {
            ChildParents.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static ChildParentCache _childParentCache=new ChildParentCache();

    public static void ClearCache() {
        _childParentCache.ClearCache();
    }

    public static List<ChildParent> GetDeepCopy(bool isShort=false) {
        return _childParentCache.GetDeepCopy(isShort);
    }

    public static int GetCount(bool isShort=false) {
        return _childParentCache.GetCount(isShort);
    }

    public static bool GetExists(Predicate<ChildParent> match,bool isShort=false) {
        return _childParentCache.GetExists(match,isShort);
    }

    public static int GetFindIndex(Predicate<ChildParent> match,bool isShort=false) {
        return _childParentCache.GetFindIndex(match,isShort);
    }

    public static ChildParent GetFirst(bool isShort=false) {
        return _childParentCache.GetFirst(isShort);
    }

    public static ChildParent GetFirst(Func<ChildParent,bool> match,bool isShort=false) {
        return _childParentCache.GetFirst(match,isShort);
    }

    public static ChildParent GetFirstOrDefault(Func<ChildParent,bool> match,bool isShort=false) {
        return _childParentCache.GetFirstOrDefault(match,isShort);
    }

    public static ChildParent GetLast(bool isShort=false) {
        return _childParentCache.GetLast(isShort);
    }

    public static ChildParent GetLastOrDefault(Func<ChildParent,bool> match,bool isShort=false) {
        return _childParentCache.GetLastOrDefault(match,isShort);
    }

    public static List<ChildParent> GetWhere(Predicate<ChildParent> match,bool isShort=false) {
        return _childParentCache.GetWhere(match,isShort);
    }

    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _childParentCache.FillCacheFromTable(table);
    }

    ///<summary>Returns the cache in the form of a DataTable. Always refreshes the ClientMT's cache.</summary>
    ///<param name="doRefreshCache">If true, will refresh the cache if MiddleTierRole is ClientDirect or ServerWeb.</param>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _childParentCache.GetTableFromCache(doRefreshCache);
    }
    #endregion Cache Pattern
    */
    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.
    #region Methods - Get
    
    public static List<ChildParent> Refresh(long patNum){

        string command="SELECT * FROM childparent WHERE PatNum = "+POut.Long(patNum);
        return Crud.ChildParentCrud.SelectMany(command);
    }

    #endregion Methods - Get
    #region Methods - Modify
    
    public static void Delete(long childParentNum) {

        Crud.ChildParentCrud.Delete(childParentNum);
    }
    #endregion Methods - Modify
    */

    ///<summary>Gets one ChildParent from the db.</summary>
    public static ChildParent GetOne(long childParentNum)
    {
        return ChildParentCrud.SelectOne(childParentNum);
    }

    public static string GetName(long childParentNum)
    {
        var childParent = ChildParentCrud.SelectOne(childParentNum);
        if (childParent == null) return "";
        return childParent.FName + " " + childParent.LName;
    }

    public static List<ChildParent> GetAll()
    {
        var command = "SELECT * FROM childparent ORDER BY LName";
        return ChildParentCrud.TableToList(DataCore.GetTable(command));
    }

    /// <summary>
    ///     Gets the first parent with the matching badgeId passed in. Expecting int with 4 digits or less.  Returns null
    ///     if not found.
    /// </summary>
    public static ChildParent GetByBadgeId(string badgeId)
    {
        var command = "SELECT * FROM childparent WHERE BadgeId <> '' AND BadgeId = RIGHT('" + SOut.String(badgeId) + "', LENGTH(BadgeId))";
        //Example BadgeId in db="123". Select compares "123" with RIGHT('00000123',3)
        var listChildParents = ChildParentCrud.TableToList(DataCore.GetTable(command));
        return listChildParents.FirstOrDefault();
    }

    
    public static long Insert(ChildParent childParent)
    {
        return ChildParentCrud.Insert(childParent);
    }

    
    public static void Update(ChildParent childParent)
    {
        ChildParentCrud.Update(childParent);
    }
}