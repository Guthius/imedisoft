using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EhrLabSpecimenRejectReasons
{
    
    public static List<EhrLabSpecimenRejectReason> GetForEhrLabSpecimen(long ehrLabSpecimenNum)
    {
        var command = "SELECT * FROM ehrlabspecimenrejectreason WHERE EhrLabSpecimenNum = " + SOut.Long(ehrLabSpecimenNum);
        return EhrLabSpecimenRejectReasonCrud.SelectMany(command);
    }

    
    public static void DeleteForLab(long ehrLabNum)
    {
        var command = "DELETE FROM ehrlabspecimenrejectreason WHERE EhrLabSpecimenNum IN (SELECT EhrLabSpecimenNum FROM ehrlabspecimen WHERE EhrLabNum=" + SOut.Long(ehrLabNum) + ")";
        Db.NonQ(command);
    }

    
    public static void DeleteForLabSpecimen(long ehrLabSpecimenNum)
    {
        var command = "DELETE FROM ehrlabspecimenrejectreason WHERE EhrLabSpecimenNum=" + SOut.Long(ehrLabSpecimenNum);
        Db.NonQ(command);
    }

    
    public static long Insert(EhrLabSpecimenRejectReason ehrLabSpecimenRejectReason)
    {
        return EhrLabSpecimenRejectReasonCrud.Insert(ehrLabSpecimenRejectReason);
    }

    //If this table type will exist as cached data, uncomment the CachePattern region below and edit.
    /*
    #region CachePattern

    private class EhrLabSpecimenRejectReasonCache : CacheListAbs<EhrLabSpecimenRejectReason> {
        protected override List<EhrLabSpecimenRejectReason> GetCacheFromDb() {
            string command="SELECT * FROM EhrLabSpecimenRejectReason ORDER BY ItemOrder";
            return Crud.EhrLabSpecimenRejectReasonCrud.SelectMany(command);
        }
        protected override List<EhrLabSpecimenRejectReason> TableToList(DataTable table) {
            return Crud.EhrLabSpecimenRejectReasonCrud.TableToList(table);
        }
        protected override EhrLabSpecimenRejectReason Copy(EhrLabSpecimenRejectReason EhrLabSpecimenRejectReason) {
            return EhrLabSpecimenRejectReason.Clone();
        }
        protected override DataTable ListToTable(List<EhrLabSpecimenRejectReason> listEhrLabSpecimenRejectReasons) {
            return Crud.EhrLabSpecimenRejectReasonCrud.ListToTable(listEhrLabSpecimenRejectReasons,"EhrLabSpecimenRejectReason");
        }
        protected override void FillCacheIfNeeded() {
            EhrLabSpecimenRejectReasons.GetTableFromCache(false);
        }
        protected override bool IsInListShort(EhrLabSpecimenRejectReason EhrLabSpecimenRejectReason) {
            return !EhrLabSpecimenRejectReason.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static EhrLabSpecimenRejectReasonCache _EhrLabSpecimenRejectReasonCache=new EhrLabSpecimenRejectReasonCache();

    ///<summary>A list of all EhrLabSpecimenRejectReasons. Returns a deep copy.</summary>
    public static List<EhrLabSpecimenRejectReason> ListDeep {
        get {
            return _EhrLabSpecimenRejectReasonCache.ListDeep;
        }
    }

    ///<summary>A list of all visible EhrLabSpecimenRejectReasons. Returns a deep copy.</summary>
    public static List<EhrLabSpecimenRejectReason> ListShortDeep {
        get {
            return _EhrLabSpecimenRejectReasonCache.ListShortDeep;
        }
    }

    ///<summary>A list of all EhrLabSpecimenRejectReasons. Returns a shallow copy.</summary>
    public static List<EhrLabSpecimenRejectReason> ListShallow {
        get {
            return _EhrLabSpecimenRejectReasonCache.ListShallow;
        }
    }

    ///<summary>A list of all visible EhrLabSpecimenRejectReasons. Returns a shallow copy.</summary>
    public static List<EhrLabSpecimenRejectReason> ListShort {
        get {
            return _EhrLabSpecimenRejectReasonCache.ListShallowShort;
        }
    }

    ///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _EhrLabSpecimenRejectReasonCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _EhrLabSpecimenRejectReasonCache.GetTableFromCache(doRefreshCache);
    }

    #endregion
    */
    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<EhrLabSpecimenRejectReason> Refresh(long patNum){

        string command="SELECT * FROM ehrlabspecimenrejectreason WHERE PatNum = "+POut.Long(patNum);
        return Crud.EhrLabSpecimenRejectReasonCrud.SelectMany(command);
    }

    ///<summary>Gets one EhrLabSpecimenRejectReason from the db.</summary>
    public static EhrLabSpecimenRejectReason GetOne(long ehrLabSpecimenRejectReasonNum){

        return Crud.EhrLabSpecimenRejectReasonCrud.SelectOne(ehrLabSpecimenRejectReasonNum);
    }

    
    public static long Insert(EhrLabSpecimenRejectReason ehrLabSpecimenRejectReason){

        return Crud.EhrLabSpecimenRejectReasonCrud.Insert(ehrLabSpecimenRejectReason);
    }

    
    public static void Update(EhrLabSpecimenRejectReason ehrLabSpecimenRejectReason){

        Crud.EhrLabSpecimenRejectReasonCrud.Update(ehrLabSpecimenRejectReason);
    }

    
    public static void Delete(long ehrLabSpecimenRejectReasonNum) {

        string command= "DELETE FROM ehrlabspecimenrejectreason WHERE EhrLabSpecimenRejectReasonNum = "+POut.Long(ehrLabSpecimenRejectReasonNum);
        Db.NonQ(command);
    }
    */
}