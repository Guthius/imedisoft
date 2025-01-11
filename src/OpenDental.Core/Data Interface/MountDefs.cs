using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class MountDefs
{
    
    public static void Update(MountDef mountDef)
    {
        MountDefCrud.Update(mountDef);
    }

    
    public static long Insert(MountDef mountDef)
    {
        return MountDefCrud.Insert(mountDef);
    }

    ///<summary>No need to surround with try/catch, because all deletions are allowed.</summary>
    public static void Delete(long mountDefNum)
    {
        var command = "DELETE FROM mountdef WHERE MountDefNum=" + SOut.Long(mountDefNum);
        Db.NonQ(command);
        command = "DELETE FROM mountitemdef WHERE MountDefNum =" + SOut.Long(mountDefNum);
        Db.NonQ(command);
    }

    public static string SetScale(float scale, int decimals, string units)
    {
        var retVal = scale + " " + decimals;
        if (units != null && units != "") retVal += " " + units;
        return retVal;
    }

    public static float GetScale(string scaleValue)
    {
        if (scaleValue is null) return 0;
        var listStrings = scaleValue.Split(' ').ToList();
        if (listStrings.Count > 0) return SIn.Float(listStrings[0]);
        return 0;
    }

    public static int GetDecimals(string scaleValue)
    {
        if (scaleValue is null) return 0;
        var listStrings = scaleValue.Split(' ').ToList();
        if (listStrings.Count > 1) return SIn.Int(listStrings[1]);
        return 0;
    }

    public static string GetScaleUnits(string scaleValue)
    {
        if (scaleValue is null) return "";
        var listStrings = scaleValue.Split(' ').ToList();
        if (listStrings.Count == 3) return listStrings[2];
        return "";
    }

    #region CachePattern

    private class MountDefCache : CacheListAbs<MountDef>
    {
        protected override List<MountDef> GetCacheFromDb()
        {
            var command = "SELECT * FROM mountdef ORDER BY ItemOrder";
            return MountDefCrud.SelectMany(command);
        }

        protected override List<MountDef> TableToList(DataTable dataTable)
        {
            return MountDefCrud.TableToList(dataTable);
        }

        protected override MountDef Copy(MountDef item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<MountDef> items)
        {
            return MountDefCrud.ListToTable(items, "MountDef");
        }

        protected override void FillCacheIfNeeded()
        {
            MountDefs.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly MountDefCache _mountDefCache = new();

    public static List<MountDef> GetDeepCopy(bool isShort = false)
    {
        return _mountDefCache.GetDeepCopy(isShort);
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
        _mountDefCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _mountDefCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _mountDefCache.ClearCache();
    }

    #endregion
}