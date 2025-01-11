using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Sops
{
    
    public static long Insert(Sop sop)
    {
        return SopCrud.Insert(sop);
    }

    
    public static void Update(Sop sop)
    {
        SopCrud.Update(sop);
    }

    ///<summary>Returns the count of all SOP codes.  SOP codes cannot be hidden.</summary>
    public static long GetCodeCount()
    {
        var command = "SELECT COUNT(*) FROM sop";
        return SIn.Long(Db.GetCount(command));
    }

    ///<summary>Returns the description for the specified SopCode.  Returns an empty string if no code is found.</summary>
    public static string GetDescriptionFromCode(string sopCode)
    {
        var sop = GetFirstOrDefault(x => x.SopCode == sopCode);
        if (sop == null) return "";
        return sop.Description;
    }

    #region CachePattern

    private class SopCache : CacheListAbs<Sop>
    {
        protected override List<Sop> GetCacheFromDb()
        {
            var command = "SELECT * FROM sop";
            return SopCrud.SelectMany(command);
        }

        protected override List<Sop> TableToList(DataTable dataTable)
        {
            return SopCrud.TableToList(dataTable);
        }

        protected override Sop Copy(Sop item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Sop> items)
        {
            return SopCrud.ListToTable(items, "Sop");
        }

        protected override void FillCacheIfNeeded()
        {
            Sops.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly SopCache _sopCache = new();

    public static List<Sop> GetDeepCopy(bool isShort = false)
    {
        return _sopCache.GetDeepCopy(isShort);
    }

    public static Sop GetFirstOrDefault(Func<Sop, bool> match, bool isShort = false)
    {
        return _sopCache.GetFirstOrDefault(match, isShort);
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
        _sopCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _sopCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _sopCache.ClearCache();
    }

    #endregion

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<Sop> Refresh(long patNum){

        string command="SELECT * FROM sop WHERE PatNum = "+POut.Long(patNum);
        return Crud.SopCrud.SelectMany(command);
    }

    ///<summary>Gets one Sop from the db.</summary>
    public static Sop GetOne(long sopNum){

        return Crud.SopCrud.SelectOne(sopNum);
    }

    
    public static void Delete(long sopNum) {

        string command= "DELETE FROM sop WHERE SopNum = "+POut.Long(sopNum);
        Db.NonQ(command);
    }
    */
}