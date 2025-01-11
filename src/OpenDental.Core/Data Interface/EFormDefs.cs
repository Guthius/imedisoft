using System;
using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EFormDefs
{
    
    public static long Insert(EFormDef eFormDef)
    {
        return EFormDefCrud.Insert(eFormDef);
    }

    
    public static void Update(EFormDef eFormDef)
    {
        EFormDefCrud.Update(eFormDef);
    }

    
    public static void Delete(long eFormDefNum)
    {
        EFormDefCrud.Delete(eFormDefNum);
    }

    #region Cache Pattern

    //Uses InvalidType.Sheets
    private class EFormDefCache : CacheListAbs<EFormDef>
    {
        protected override List<EFormDef> GetCacheFromDb()
        {
            var command = "SELECT * FROM eformdef";
            return EFormDefCrud.SelectMany(command);
        }

        protected override List<EFormDef> TableToList(DataTable dataTable)
        {
            return EFormDefCrud.TableToList(dataTable);
        }

        protected override EFormDef Copy(EFormDef item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<EFormDef> items)
        {
            return EFormDefCrud.ListToTable(items, "EFormDef");
        }

        protected override void FillCacheIfNeeded()
        {
            EFormDefs.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly EFormDefCache _eFormDefCache = new();

    public static void ClearCache()
    {
        _eFormDefCache.ClearCache();
    }

    public static List<EFormDef> GetDeepCopy(bool isShort = false)
    {
        return _eFormDefCache.GetDeepCopy(isShort);
    }

    public static int GetCount(bool isShort = false)
    {
        return _eFormDefCache.GetCount(isShort);
    }

    public static bool GetExists(Predicate<EFormDef> match, bool isShort = false)
    {
        return _eFormDefCache.GetExists(match, isShort);
    }

    public static int GetFindIndex(Predicate<EFormDef> match, bool isShort = false)
    {
        return _eFormDefCache.GetFindIndex(match, isShort);
    }

    public static EFormDef GetFirst(bool isShort = false)
    {
        return _eFormDefCache.GetFirst(isShort);
    }

    public static EFormDef GetFirst(Func<EFormDef, bool> match, bool isShort = false)
    {
        return _eFormDefCache.GetFirst(match, isShort);
    }

    public static EFormDef GetFirstOrDefault(Func<EFormDef, bool> match, bool isShort = false)
    {
        return _eFormDefCache.GetFirstOrDefault(match, isShort);
    }

    public static EFormDef GetLast(bool isShort = false)
    {
        return _eFormDefCache.GetLast(isShort);
    }

    public static EFormDef GetLastOrDefault(Func<EFormDef, bool> match, bool isShort = false)
    {
        return _eFormDefCache.GetLastOrDefault(match, isShort);
    }

    public static List<EFormDef> GetWhere(Predicate<EFormDef> match, bool isShort = false)
    {
        return _eFormDefCache.GetWhere(match, isShort);
    }

    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _eFormDefCache.FillCacheFromTable(table);
    }

    /// <summary>Returns the cache in the form of a DataTable. Always refreshes the ClientMT's cache.</summary>
    /// <param name="doRefreshCache">If true, will refresh the cache if MiddleTierRole is ClientDirect or ServerWeb.</param>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _eFormDefCache.GetTableFromCache(doRefreshCache);
    }

    #endregion Cache Pattern

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<EFormDef> Refresh(long patNum){

        string command="SELECT * FROM eformdef WHERE PatNum = "+POut.Long(patNum);
        return Crud.EFormDefCrud.SelectMany(command);
    }

    ///<summary>Gets one EFormDef from the db.</summary>
    public static EFormDef GetOne(long eFormDefNum){

        return Crud.EFormDefCrud.SelectOne(eFormDefNum);
    }



    */
}