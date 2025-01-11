using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class SmsBlockPhones
{
    #region Insert

    
    public static long Insert(SmsBlockPhone smsBlockPhone)
    {
        return SmsBlockPhoneCrud.Insert(smsBlockPhone);
    }

    #endregion

    #region Cache Pattern

    private class SmsBlockPhoneCache : CacheListAbs<SmsBlockPhone>
    {
        protected override List<SmsBlockPhone> GetCacheFromDb()
        {
            var command = "SELECT * FROM smsblockphone";
            return SmsBlockPhoneCrud.SelectMany(command);
        }

        protected override List<SmsBlockPhone> TableToList(DataTable dataTable)
        {
            return SmsBlockPhoneCrud.TableToList(dataTable);
        }

        protected override SmsBlockPhone Copy(SmsBlockPhone item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<SmsBlockPhone> items)
        {
            return SmsBlockPhoneCrud.ListToTable(items, "SmsBlockPhone");
        }

        protected override void FillCacheIfNeeded()
        {
            SmsBlockPhones.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly SmsBlockPhoneCache _smsBlockPhoneCache = new();

    public static List<SmsBlockPhone> GetDeepCopy(bool isShort = false)
    {
        return _smsBlockPhoneCache.GetDeepCopy(isShort);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _smsBlockPhoneCache.FillCacheFromTable(table);
    }

    /// <summary>Returns the cache in the form of a DataTable. Always refreshes the ClientWeb's cache.</summary>
    /// <param name="doRefreshCache">If true, will refresh the cache if RemotingRole is ClientDirect or ServerWeb.</param>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _smsBlockPhoneCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _smsBlockPhoneCache.ClearCache();
    }

    #endregion Cache Pattern

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.
    #region Get Methods
    
    public static List<SmsBlockPhone> Refresh(long patNum){

        string command="SELECT * FROM smsblockphone WHERE PatNum = "+POut.Long(patNum);
        return Crud.SmsBlockPhoneCrud.SelectMany(command);
    }

    ///<summary>Gets one SmsBlockPhone from the db.</summary>
    public static SmsBlockPhone GetOne(long smsBlockPhoneNum){

        return Crud.SmsBlockPhoneCrud.SelectOne(smsBlockPhoneNum);
    }
    #endregion
    #region Modification Methods
        #region Update
    
    public static void Update(SmsBlockPhone smsBlockPhone){

        Crud.SmsBlockPhoneCrud.Update(smsBlockPhone);
    }
        #endregion
        #region Delete
    
    public static void Delete(long smsBlockPhoneNum) {

        Crud.SmsBlockPhoneCrud.Delete(smsBlockPhoneNum);
    }
        #endregion
    #endregion
    #region Misc Methods



    #endregion
    */
}