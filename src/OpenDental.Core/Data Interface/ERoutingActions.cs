using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ERoutingActions
{
    #region commented cache pattern

    //If this table type will exist as cached data, uncomment the Cache Pattern region below and edit.
    /*
    #region Cache Pattern
    //This region can be eliminated if this is not a table type with cached data.
    //If leaving this region in place, be sure to add GetTableFromCache and FillCacheFromTable to the Cache.cs file with all the other Cache types.
    //Also, consider making an invalid type for this class in Cache.GetAllCachedInvalidTypes() if needed.

    private class ERoutingActionCache : CacheListAbs<ERoutingAction> {
        protected override List<ERoutingAction> GetCacheFromDb() {
            string command="SELECT * FROM eRoutingaction";
            return Crud.ERoutingActionCrud.SelectMany(command);
        }
        protected override List<ERoutingAction> TableToList(DataTable table) {
            return Crud.ERoutingActionCrud.TableToList(table);
        }
        protected override ERoutingAction Copy(ERoutingAction ERoutingAction) {
            return ERoutingAction.Copy();
        }
        protected override DataTable ListToTable(List<ERoutingAction> listERoutingActions) {
            return Crud.ERoutingActionCrud.ListToTable(listERoutingActions,"ERoutingAction");
        }
        protected override void FillCacheIfNeeded() {
            ERoutingActions.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static ERoutingActionCache _eRoutingActionCache=new ERoutingActionCache();

    public static List<ERoutingAction> GetDeepCopy(bool isShort=false) {
        return _eRoutingActionCache.GetDeepCopy(isShort);
    }

    public static int GetCount(bool isShort=false) {
        return _eRoutingActionCache.GetCount(isShort);
    }

    public static bool GetExists(Predicate<ERoutingAction> match,bool isShort=false) {
        return _eRoutingActionCache.GetExists(match,isShort);
    }

    public static int GetFindIndex(Predicate<ERoutingAction> match,bool isShort=false) {
        return _eRoutingActionCache.GetFindIndex(match,isShort);
    }

    public static ERoutingAction GetFirst(bool isShort=false) {
        return _eRoutingActionCache.GetFirst(isShort);
    }

    public static ERoutingAction GetFirst(Func<ERoutingAction,bool> match,bool isShort=false) {
        return _eRoutingActionCache.GetFirst(match,isShort);
    }

    public static ERoutingAction GetFirstOrDefault(Func<ERoutingAction,bool> match,bool isShort=false) {
        return _eRoutingActionCache.GetFirstOrDefault(match,isShort);
    }

    public static ERoutingAction GetLast(bool isShort=false) {
        return _eRoutingActionCache.GetLast(isShort);
    }

    public static ERoutingAction GetLastOrDefault(Func<ERoutingAction,bool> match,bool isShort=false) {
        return _eRoutingActionCache.GetLastOrDefault(match,isShort);
    }

    public static List<ERoutingAction> GetWhere(Predicate<ERoutingAction> match,bool isShort=false) {
        return _eRoutingActionCache.GetWhere(match,isShort);
    }

    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _eRoutingActionCache.FillCacheFromTable(table);
    }

    ///<summary>Returns the cache in the form of a DataTable. Always refreshes the ClientMT's cache.</summary>
    ///<param name="doRefreshCache">If true, will refresh the cache if MiddleTierRole is ClientDirect or ServerWeb.</param>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _eRoutingActionCache.GetTableFromCache(doRefreshCache);
    }
    #endregion Cache Pattern
    */

    #endregion

    #region Methods - Get

    
    public static List<ERoutingAction> Refresh(long patNum)
    {
        var command = "SELECT * FROM eroutingaction WHERE PatNum = " + SOut.Long(patNum);
        return ERoutingActionCrud.SelectMany(command);
    }

    ///<summary>Gets one ERoutingAction from the db.</summary>
    public static ERoutingAction GetOne(long eRoutingActionNum)
    {
        return ERoutingActionCrud.SelectOne(eRoutingActionNum);
    }

    ///<summary>Gets one ERoutingAction from the db.</summary>
    public static List<ERoutingAction> GetListForERouting(long eRoutingNum)
    {
        var command = $"SELECT * FROM eroutingaction WHERE ERoutingNum = {eRoutingNum}";
        return ERoutingActionCrud.SelectMany(command);
    }

    #endregion Methods - Get

    #region Methods - Modify

    
    public static long Insert(ERoutingAction eRoutingAction)
    {
        return ERoutingActionCrud.Insert(eRoutingAction);
    }

    
    public static void Update(ERoutingAction eRoutingAction)
    {
        ERoutingActionCrud.Update(eRoutingAction);
    }

    
    public static void Delete(long eRoutingActionNum)
    {
        ERoutingActionCrud.Delete(eRoutingActionNum);
    }

    ///<summary>Deletes all ERoutingActions for the specified eRoutingNum.</summary>
    public static void DeleteAllForERouting(long eRoutingNum)
    {
        var command = "DELETE FROM eroutingaction "
                      + "WHERE ERoutingNum=" + SOut.Long(eRoutingNum);
        Db.NonQ(command);
    }

    #endregion Methods - Modify
}