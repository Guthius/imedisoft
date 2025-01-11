using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class ImagingDevices
{
    
    public static void Update(ImagingDevice imagingDevice)
    {
        ImagingDeviceCrud.Update(imagingDevice);
    }

    
    public static long Insert(ImagingDevice imagingDevice)
    {
        return ImagingDeviceCrud.Insert(imagingDevice);
    }

    ///<summary>No need to surround with try/catch, because all deletions are allowed.</summary>
    public static void Delete(long imagingDeviceNum)
    {
        var command = "DELETE FROM imagingdevice WHERE ImagingDeviceNum=" + SOut.Long(imagingDeviceNum);
        Db.NonQ(command);
    }

    #region CachePattern

    private class ImagingDeviceCache : CacheListAbs<ImagingDevice>
    {
        protected override List<ImagingDevice> GetCacheFromDb()
        {
            var command = "SELECT * FROM imagingdevice ORDER BY ItemOrder";
            return ImagingDeviceCrud.SelectMany(command);
        }

        protected override List<ImagingDevice> TableToList(DataTable dataTable)
        {
            return ImagingDeviceCrud.TableToList(dataTable);
        }

        protected override ImagingDevice Copy(ImagingDevice item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ImagingDevice> items)
        {
            return ImagingDeviceCrud.ListToTable(items, "ImagingDevice");
        }

        protected override void FillCacheIfNeeded()
        {
            ImagingDevices.GetTableFromCache(false);
        }

        protected override bool IsInListShort(ImagingDevice item)
        {
            return true;
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ImagingDeviceCache _imagingDeviceCache = new();

    public static List<ImagingDevice> GetDeepCopy(bool isShort = false)
    {
        return _imagingDeviceCache.GetDeepCopy(isShort);
    }

    public static bool GetExists(Predicate<ImagingDevice> match, bool isShort = false)
    {
        return _imagingDeviceCache.GetExists(match, isShort);
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
        _imagingDeviceCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _imagingDeviceCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _imagingDeviceCache.ClearCache();
    }

    #endregion
}