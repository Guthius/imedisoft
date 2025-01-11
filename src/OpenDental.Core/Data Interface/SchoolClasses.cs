using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class SchoolClasses
{
    
    public static void Update(SchoolClass schoolClass)
    {
        SchoolClassCrud.Update(schoolClass);
    }

    
    public static long Insert(SchoolClass schoolClass)
    {
        return SchoolClassCrud.Insert(schoolClass);
    }

    
    public static void InsertOrUpdate(SchoolClass schoolClass, bool isNew)
    {
        //if(IsRepeating && DateTask.Year>1880){
        //	throw new Exception(Lans.g(this,"Task cannot be tagged repeating and also have a date."));
        //}
        if (isNew)
            Insert(schoolClass);
        else
            Update(schoolClass);
    }

    ///<summary>Surround by a try/catch in case there are dependencies.</summary>
    public static void Delete(long classNum)
    {
        //check for attached providers
        var command = "SELECT COUNT(*) FROM provider WHERE SchoolClassNum = '"
                      + SOut.Long(classNum) + "'";
        var table = DataCore.GetTable(command);
        if (SIn.String(table.Rows[0][0].ToString()) != "0") throw new Exception(Lans.g("SchoolClasses", "Class already in use by providers."));
        //check for attached reqneededs.
        command = "SELECT COUNT(*) FROM reqneeded WHERE SchoolClassNum = '"
                  + SOut.Long(classNum) + "'";
        table = DataCore.GetTable(command);
        if (SIn.String(table.Rows[0][0].ToString()) != "0") throw new Exception(Lans.g("SchoolClasses", "Class already in use by 'requirements needed' table."));
        command = "DELETE from schoolclass WHERE SchoolClassNum = '"
                  + SOut.Long(classNum) + "'";
        Db.NonQ(command);
    }

    public static string GetDescript(long SchoolClassNum)
    {
        var schoolClass = GetFirstOrDefault(x => x.SchoolClassNum == SchoolClassNum);
        if (schoolClass == null) return "";
        return GetDescript(schoolClass);
    }

    public static string GetDescript(SchoolClass schoolClass)
    {
        return schoolClass.GradYear + "-" + schoolClass.Descript;
    }

    #region CachePattern

    private class SchoolClassCache : CacheListAbs<SchoolClass>
    {
        protected override List<SchoolClass> GetCacheFromDb()
        {
            var command =
                "SELECT * FROM schoolclass "
                + "ORDER BY GradYear,Descript";
            return SchoolClassCrud.SelectMany(command);
        }

        protected override List<SchoolClass> TableToList(DataTable dataTable)
        {
            return SchoolClassCrud.TableToList(dataTable);
        }

        protected override SchoolClass Copy(SchoolClass item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<SchoolClass> items)
        {
            return SchoolClassCrud.ListToTable(items, "SchoolClass");
        }

        protected override void FillCacheIfNeeded()
        {
            SchoolClasses.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly SchoolClassCache _schoolClassCache = new();

    public static List<SchoolClass> GetDeepCopy(bool isShort = false)
    {
        return _schoolClassCache.GetDeepCopy(isShort);
    }

    public static SchoolClass GetFirstOrDefault(Func<SchoolClass, bool> funcMatch, bool isShort = false)
    {
        return _schoolClassCache.GetFirstOrDefault(funcMatch, isShort);
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
        _schoolClassCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _schoolClassCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _schoolClassCache.ClearCache();
    }

    #endregion
}