using System.Collections.Generic;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class MedLabSpecimens
{
    //If this table type will exist as cached data, uncomment the CachePattern region below and edit.
    /*
    #region CachePattern

    private class MedLabSpecimenCache : CacheListAbs<MedLabSpecimen> {
        protected override List<MedLabSpecimen> GetCacheFromDb() {
            string command="SELECT * FROM MedLabSpecimen ORDER BY ItemOrder";
            return Crud.MedLabSpecimenCrud.SelectMany(command);
        }
        protected override List<MedLabSpecimen> TableToList(DataTable table) {
            return Crud.MedLabSpecimenCrud.TableToList(table);
        }
        protected override MedLabSpecimen Copy(MedLabSpecimen MedLabSpecimen) {
            return MedLabSpecimen.Clone();
        }
        protected override DataTable ListToTable(List<MedLabSpecimen> listMedLabSpecimens) {
            return Crud.MedLabSpecimenCrud.ListToTable(listMedLabSpecimens,"MedLabSpecimen");
        }
        protected override void FillCacheIfNeeded() {
            MedLabSpecimens.GetTableFromCache(false);
        }
        protected override bool IsInListShort(MedLabSpecimen MedLabSpecimen) {
            return !MedLabSpecimen.IsHidden;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static MedLabSpecimenCache _MedLabSpecimenCache=new MedLabSpecimenCache();

    ///<summary>A list of all MedLabSpecimens. Returns a deep copy.</summary>
    public static List<MedLabSpecimen> ListDeep {
        get {
            return _MedLabSpecimenCache.ListDeep;
        }
    }

    ///<summary>A list of all visible MedLabSpecimens. Returns a deep copy.</summary>
    public static List<MedLabSpecimen> ListShortDeep {
        get {
            return _MedLabSpecimenCache.ListShortDeep;
        }
    }

    ///<summary>A list of all MedLabSpecimens. Returns a shallow copy.</summary>
    public static List<MedLabSpecimen> ListShallow {
        get {
            return _MedLabSpecimenCache.ListShallow;
        }
    }

    ///<summary>A list of all visible MedLabSpecimens. Returns a shallow copy.</summary>
    public static List<MedLabSpecimen> ListShort {
        get {
            return _MedLabSpecimenCache.ListShallowShort;
        }
    }

    ///<summary>Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's cache.</summary>
    public static DataTable RefreshCache() {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table) {
        _MedLabSpecimenCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache) {

        return _MedLabSpecimenCache.GetTableFromCache(doRefreshCache);
    }

    #endregion
    */

    
    public static long Insert(MedLabSpecimen medLabSpecimen)
    {
        return MedLabSpecimenCrud.Insert(medLabSpecimen);
    }

    ///<summary>Deletes all MedLabSpecimen objects from the db for a list of MedLabNums.</summary>
    public static void DeleteAllForLabs(List<long> listMedLabNums)
    {
        var command = "DELETE FROM medlabspecimen WHERE MedLabNum IN(" + string.Join(",", listMedLabNums) + ")";
        Db.NonQ(command);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<MedLabSpecimen> Refresh(long patNum){

        string command="SELECT * FROM medlabspecimen WHERE PatNum = "+POut.Long(patNum);
        return Crud.MedLabSpecimenCrud.SelectMany(command);
    }

    ///<summary>Gets one MedLabSpecimen from the db.</summary>
    public static MedLabSpecimen GetOne(long medLabSpecimenNum){

        return Crud.MedLabSpecimenCrud.SelectOne(medLabSpecimenNum);
    }

    
    public static void Update(MedLabSpecimen medLabSpecimen){

        Crud.MedLabSpecimenCrud.Update(medLabSpecimen);
    }

    
    public static void Delete(long medLabSpecimenNum) {

        string command= "DELETE FROM medlabspecimen WHERE MedLabSpecimenNum = "+POut.Long(medLabSpecimenNum);
        Db.NonQ(command);
    }
    */
}