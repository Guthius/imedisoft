using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EhrLabSpecimenConditions
{
    
    public static List<EhrLabSpecimenCondition> GetForEhrLabSpecimen(long ehrLabSpecimenNum)
    {
        var command = "SELECT * FROM ehrlabspecimencondition WHERE EhrLabSpecimenNum=" + SOut.Long(ehrLabSpecimenNum);
        return EhrLabSpecimenConditionCrud.SelectMany(command);
    }

    
    public static void DeleteForLab(long ehrLabNum)
    {
        var command = "DELETE FROM ehrlabspecimencondition WHERE EhrLabSpecimenNum IN (SELECT EhrLabSpecimenNum FROM ehrlabspecimen WHERE EhrLabNum=" + SOut.Long(ehrLabNum) + ")";
        Db.NonQ(command);
    }

    
    public static void DeleteForLabSpecimen(long ehrLabSpecimenNum)
    {
        var command = "DELETE FROM ehrlabspecimencondition WHERE EhrLabSpecimenNum=" + SOut.Long(ehrLabSpecimenNum);
        Db.NonQ(command);
    }

    
    public static long Insert(EhrLabSpecimenCondition ehrLabSpecimenCondition)
    {
        return EhrLabSpecimenConditionCrud.Insert(ehrLabSpecimenCondition);
    }

    //If this table type will exist as cached data, uncomment the CachePattern region below and edit.
    /*
    #region CachePattern

    private class EhrLabSpecimenConditionCache : CacheListAbs<EhrLabSpecimenCondition> {
        protected override List<EhrLabSpecimenCondition> GetCacheFromDb() {
            string command="SELECT * FROM EhrLabSpecimenCondition ORDER BY ItemOrder";
            return Crud.EhrLabSpecimenConditionCrud.SelectMany(command);
        }
        protected override List<EhrLabSpecimenCondition> TableToList(DataTable table) {
            return Crud.EhrLabSpecimenConditionCrud.TableToList(table);
        }
        protected override EhrLabSpecimenCondition Copy(EhrLabSpecimenCondition EhrLabSpecimenCondition) {
            return EhrLabSpecimenCondition.Clone();
        }
        protected override DataTable ListToTable(List<EhrLabSpecimenCondition> listEhrLabSpecimenConditions) {
            return Crud.EhrLabSpecimenConditionCrud.ListToTable(listEhrLabSpecimenConditions,"EhrLabSpecimenCondition");
        }
        protected override void FillCacheIfNeeded() {
            EhrLabSpecimenConditions.GetTableFromCache(false);
        }
        protected override bool IsInListShort(EhrLabSpecimenCondition EhrLabSpecimenCondition) {
            return !EhrLabSpecimenCondition.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static EhrLabSpecimenConditionCache _EhrLabSpecimenConditionCache=new EhrLabSpecimenConditionCache();

    ///<summary>A list of all EhrLabSpecimenConditions. Returns a deep copy.</summary>
    public static List<EhrLabSpecimenCondition> ListDeep {
        get {
            return _EhrLabSpecimenConditionCache.ListDeep;
        }
    }

    ///<summary>A list of all visible EhrLabSpecimenConditions. Returns a deep copy.</summary>
    public static List<EhrLabSpecimenCondition> ListShortDeep {
        get {
            return _EhrLabSpecimenConditionCache.ListShortDeep;
        }
    }

    ///<summary>A list of all EhrLabSpecimenConditions. Returns a shallow copy.</summary>
    public static List<EhrLabSpecimenCondition> ListShallow {
        get {
            return _EhrLabSpecimenConditionCache.ListShallow;
        }
    }

    ///<summary>A list of all visible EhrLabSpecimenConditions. Returns a shallow copy.</summary>
    public static List<EhrLabSpecimenCondition> ListShort {
        get {
            return _EhrLabSpecimenConditionCache.ListShallowShort;
        }
    }

    ///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _EhrLabSpecimenConditionCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _EhrLabSpecimenConditionCache.GetTableFromCache(doRefreshCache);
    }

    #endregion
    */
    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<EhrLabSpecimenCondition> Refresh(long patNum){

        string command="SELECT * FROM ehrlabspecimencondition WHERE PatNum = "+POut.Long(patNum);
        return Crud.EhrLabSpecimenConditionCrud.SelectMany(command);
    }

    ///<summary>Gets one EhrLabSpecimenCondition from the db.</summary>
    public static EhrLabSpecimenCondition GetOne(long ehrLabSpecimenConditionNum){

        return Crud.EhrLabSpecimenConditionCrud.SelectOne(ehrLabSpecimenConditionNum);
    }

    
    public static long Insert(EhrLabSpecimenCondition ehrLabSpecimenCondition){

        return Crud.EhrLabSpecimenConditionCrud.Insert(ehrLabSpecimenCondition);
    }

    
    public static void Update(EhrLabSpecimenCondition ehrLabSpecimenCondition){

        Crud.EhrLabSpecimenConditionCrud.Update(ehrLabSpecimenCondition);
    }

    
    public static void Delete(long ehrLabSpecimenConditionNum) {

        string command= "DELETE FROM ehrlabspecimencondition WHERE EhrLabSpecimenConditionNum = "+POut.Long(ehrLabSpecimenConditionNum);
        Db.NonQ(command);
    }
    */
}