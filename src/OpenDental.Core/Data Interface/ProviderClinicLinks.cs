using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ProviderClinicLinks
{
    public static List<ProviderClinicLink> GetForProvider(long provNum)
    {
        return GetWhere(x => x.ProvNum == provNum);
    }

    public static List<ProviderClinicLink> GetForClinic(long clinicNum)
    {
        return GetWhere(x => x.ClinicNum == clinicNum);
    }

    ///<summary>Gets a list of ProviderClinicLinks that correspond with a list of clinic nums.  </summary>
    public static List<ProviderClinicLink> GetAllForClinics(List<long> listClinicNums)
    {
        return GetWhere(x => listClinicNums.Contains(x.ClinicNum));
    }

    ///<summary>Returns the providers that are associated to other clinics but not this one.</summary>
    public static List<long> GetProvsRestrictedToOtherClinics(long clinicNum)
    {
        if (clinicNum == 0) //Consider 0 as 'Headquarters'
            return new List<long>();
        var hashSetProvsThisClinic = new HashSet<long>(GetForClinic(clinicNum).Select(x => x.ProvNum));
        return GetWhere(x => x.ClinicNum != clinicNum && !hashSetProvsThisClinic.Contains(x.ProvNum))
            .Select(x => x.ProvNum).Distinct().ToList();
    }

    ///<summary>Returns the providers that are associated to other clinics, excluding the clinics in this list.  </summary>
    public static List<long> GetProvsRestrictedToOtherClinics(List<long> listClinicNums)
    {
        if (listClinicNums.IsNullOrEmpty()
            || (listClinicNums.Count == 1 && listClinicNums.First() == 0)) //Consider 0 as 'Headquarters'
            return new List<long>();
        var hashSetProvsTheseClinics = new HashSet<long>(GetAllForClinics(listClinicNums).Select(x => x.ProvNum));
        return GetWhere(x => !listClinicNums.Contains(x.ClinicNum) && !hashSetProvsTheseClinics.Contains(x.ProvNum))
            .Select(x => x.ProvNum).Distinct().ToList();
    }

    
    public static bool Sync(List<ProviderClinicLink> listNew, List<ProviderClinicLink> listDB)
    {
        return ProviderClinicLinkCrud.Sync(listNew, listDB);
    }

    #region Cache Pattern

    private class ProviderClinicLinkCache : CacheListAbs<ProviderClinicLink>
    {
        protected override List<ProviderClinicLink> GetCacheFromDb()
        {
            var command = "SELECT * FROM providercliniclink";
            return ProviderClinicLinkCrud.SelectMany(command);
        }

        protected override List<ProviderClinicLink> TableToList(DataTable dataTable)
        {
            return ProviderClinicLinkCrud.TableToList(dataTable);
        }

        protected override ProviderClinicLink Copy(ProviderClinicLink item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ProviderClinicLink> items)
        {
            return ProviderClinicLinkCrud.ListToTable(items, "ProviderClinicLink");
        }

        protected override void FillCacheIfNeeded()
        {
            ProviderClinicLinks.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ProviderClinicLinkCache _providerClinicLinkCache = new();

    public static List<ProviderClinicLink> GetDeepCopy(bool isShort = false)
    {
        return _providerClinicLinkCache.GetDeepCopy(isShort);
    }

    public static int GetCount(bool isShort = false)
    {
        return _providerClinicLinkCache.GetCount(isShort);
    }

    public static bool GetExists(Predicate<ProviderClinicLink> match, bool isShort = false)
    {
        return _providerClinicLinkCache.GetExists(match, isShort);
    }

    public static int GetFindIndex(Predicate<ProviderClinicLink> match, bool isShort = false)
    {
        return _providerClinicLinkCache.GetFindIndex(match, isShort);
    }

    public static ProviderClinicLink GetFirst(bool isShort = false)
    {
        return _providerClinicLinkCache.GetFirst(isShort);
    }

    public static ProviderClinicLink GetFirst(Func<ProviderClinicLink, bool> match, bool isShort = false)
    {
        return _providerClinicLinkCache.GetFirst(match, isShort);
    }

    public static ProviderClinicLink GetFirstOrDefault(Func<ProviderClinicLink, bool> match, bool isShort = false)
    {
        return _providerClinicLinkCache.GetFirstOrDefault(match, isShort);
    }

    public static ProviderClinicLink GetLast(bool isShort = false)
    {
        return _providerClinicLinkCache.GetLast(isShort);
    }

    public static ProviderClinicLink GetLastOrDefault(Func<ProviderClinicLink, bool> match, bool isShort = false)
    {
        return _providerClinicLinkCache.GetLastOrDefault(match, isShort);
    }

    public static List<ProviderClinicLink> GetWhere(Predicate<ProviderClinicLink> match, bool isShort = false)
    {
        return _providerClinicLinkCache.GetWhere(match, isShort);
    }

    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _providerClinicLinkCache.FillCacheFromTable(table);
    }

    /// <summary>Returns the cache in the form of a DataTable. Always refreshes the ClientWeb's cache.</summary>
    /// <param name="doRefreshCache">If true, will refresh the cache if RemotingRole is ClientDirect or ServerWeb.</param>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _providerClinicLinkCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _providerClinicLinkCache.ClearCache();
    }

    #endregion Cache Pattern

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.
    #region Get Methods
    
    public static List<ProviderClinicLink> Refresh(long patNum){

        string command="SELECT * FROM providercliniclink WHERE PatNum = "+POut.Long(patNum);
        return Crud.ProviderClinicLinkCrud.SelectMany(command);
    }

    ///<summary>Gets one ProviderClinicLink from the db.</summary>
    public static ProviderClinicLink GetOne(long providerClinicLinkcNum){

        return Crud.ProviderClinicLinkCrud.SelectOne(providerClinicLinkcNum);
    }
    #endregion Get Methods
    #region Modification Methods
    #region Insert
    
    public static long Insert(ProviderClinicLink providerClinicLink){

        return Crud.ProviderClinicLinkCrud.Insert(providerClinicLink);
    }
    #endregion Insert
    #region Update
    
    public static void Update(ProviderClinicLink providerClinicLink){

        Crud.ProviderClinicLinkCrud.Update(providerClinicLink);
    }
    #endregion Update
    #region Delete
    
    public static void Delete(long providerClinicLinkcNum) {

        Crud.ProviderClinicLinkCrud.Delete(providerClinicLinkcNum);
    }
    #endregion Delete
    #endregion Modification Methods
    #region Misc Methods



    #endregion Misc Methods
    */
}