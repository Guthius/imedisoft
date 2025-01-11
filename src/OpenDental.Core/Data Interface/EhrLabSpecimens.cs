using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EhrLabSpecimens
{
    
    public static List<EhrLabSpecimen> GetForLab(long ehrLabNum)
    {
        var command = "SELECT * FROM ehrlabspecimen WHERE EhrLabNum = " + SOut.Long(ehrLabNum);
        return EhrLabSpecimenCrud.SelectMany(command);
    }

    
    public static void DeleteForLab(long ehrLabNum)
    {
        EhrLabSpecimenConditions.DeleteForLab(ehrLabNum);
        EhrLabSpecimenRejectReasons.DeleteForLab(ehrLabNum);
        var command = "DELETE FROM ehrlabspecimen WHERE EhrLabNum = " + SOut.Long(ehrLabNum);
        Db.NonQ(command);
    }

    
    public static EhrLabSpecimen InsertItem(EhrLabSpecimen ehrLabSpecimen)
    {
        ehrLabSpecimen.EhrLabNum = EhrLabSpecimenCrud.Insert(ehrLabSpecimen);
        for (var i = 0; i < ehrLabSpecimen.ListEhrLabSpecimenCondition.Count; i++)
        {
            ehrLabSpecimen.ListEhrLabSpecimenCondition[i].EhrLabSpecimenNum = ehrLabSpecimen.EhrLabSpecimenNum;
            EhrLabSpecimenConditions.Insert(ehrLabSpecimen.ListEhrLabSpecimenCondition[i]);
        }

        for (var i = 0; i < ehrLabSpecimen.ListEhrLabSpecimenRejectReason.Count; i++)
        {
            ehrLabSpecimen.ListEhrLabSpecimenRejectReason[i].EhrLabSpecimenNum = ehrLabSpecimen.EhrLabSpecimenNum;
            EhrLabSpecimenRejectReasons.Insert(ehrLabSpecimen.ListEhrLabSpecimenRejectReason[i]);
        }

        return ehrLabSpecimen;
    }

    //If this table type will exist as cached data, uncomment the CachePattern region below and edit.
    /*
    #region CachePattern

    private class EhrLabSpecimenCache : CacheListAbs<EhrLabSpecimen> {
        protected override List<EhrLabSpecimen> GetCacheFromDb() {
            string command="SELECT * FROM EhrLabSpecimen ORDER BY ItemOrder";
            return Crud.EhrLabSpecimenCrud.SelectMany(command);
        }
        protected override List<EhrLabSpecimen> TableToList(DataTable table) {
            return Crud.EhrLabSpecimenCrud.TableToList(table);
        }
        protected override EhrLabSpecimen Copy(EhrLabSpecimen EhrLabSpecimen) {
            return EhrLabSpecimen.Clone();
        }
        protected override DataTable ListToTable(List<EhrLabSpecimen> listEhrLabSpecimens) {
            return Crud.EhrLabSpecimenCrud.ListToTable(listEhrLabSpecimens,"EhrLabSpecimen");
        }
        protected override void FillCacheIfNeeded() {
            EhrLabSpecimens.GetTableFromCache(false);
        }
        protected override bool IsInListShort(EhrLabSpecimen EhrLabSpecimen) {
            return !EhrLabSpecimen.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static EhrLabSpecimenCache _EhrLabSpecimenCache=new EhrLabSpecimenCache();

    ///<summary>A list of all EhrLabSpecimens. Returns a deep copy.</summary>
    public static List<EhrLabSpecimen> ListDeep {
        get {
            return _EhrLabSpecimenCache.ListDeep;
        }
    }

    ///<summary>A list of all visible EhrLabSpecimens. Returns a deep copy.</summary>
    public static List<EhrLabSpecimen> ListShortDeep {
        get {
            return _EhrLabSpecimenCache.ListShortDeep;
        }
    }

    ///<summary>A list of all EhrLabSpecimens. Returns a shallow copy.</summary>
    public static List<EhrLabSpecimen> ListShallow {
        get {
            return _EhrLabSpecimenCache.ListShallow;
        }
    }

    ///<summary>A list of all visible EhrLabSpecimens. Returns a shallow copy.</summary>
    public static List<EhrLabSpecimen> ListShort {
        get {
            return _EhrLabSpecimenCache.ListShallowShort;
        }
    }

    ///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _EhrLabSpecimenCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _EhrLabSpecimenCache.GetTableFromCache(doRefreshCache);
    }

    #endregion
    */
    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<EhrLabSpecimen> Refresh(long patNum){

        string command="SELECT * FROM ehrlabspecimen WHERE PatNum = "+POut.Long(patNum);
        return Crud.EhrLabSpecimenCrud.SelectMany(command);
    }

    
    public static long Insert(EhrLabSpecimen ehrLabSpecimen) {

        return Crud.EhrLabSpecimenCrud.Insert(ehrLabSpecimen);
    }

    ///<summary>Gets one EhrLabSpecimen from the db.</summary>
    public static EhrLabSpecimen GetOne(long ehrLabSpecimenNum){

        return Crud.EhrLabSpecimenCrud.SelectOne(ehrLabSpecimenNum);
    }

    
    public static long Insert(EhrLabSpecimen ehrLabSpecimen){

        return Crud.EhrLabSpecimenCrud.Insert(ehrLabSpecimen);
    }

    
    public static void Update(EhrLabSpecimen ehrLabSpecimen){

        Crud.EhrLabSpecimenCrud.Update(ehrLabSpecimen);
    }

    
    public static void Delete(long ehrLabSpecimenNum) {

        string command= "DELETE FROM ehrlabspecimen WHERE EhrLabSpecimenNum = "+POut.Long(ehrLabSpecimenNum);
        Db.NonQ(command);
    }
    */
}