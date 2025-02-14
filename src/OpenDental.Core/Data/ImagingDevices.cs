using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class ImagingDevices
{
    public static void Update(ImagingDevice imagingDevice)
    {
        ImagingDeviceCrud.Update(imagingDevice);
    }

    public static void Insert(ImagingDevice imagingDevice)
    {
        ImagingDeviceCrud.Insert(imagingDevice);
    }

    public static void Delete(long imagingDeviceNum)
    {
        Db.NonQ("DELETE FROM imagingdevice WHERE ImagingDeviceNum=" + imagingDeviceNum);
    }

    private class ImagingDeviceCache : CacheListAbs<ImagingDevice>
    {
        protected override List<ImagingDevice> GetCacheFromDb()
        {
            return ImagingDeviceCrud.SelectMany("SELECT * FROM imagingdevice ORDER BY ItemOrder");
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
            GetTableFromCache(false);
        }

        protected override bool IsInListShort(ImagingDevice item)
        {
            return true;
        }
    }

    private static readonly ImagingDeviceCache Cache = new();

    public static List<ImagingDevice> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static void RefreshCache()
    {
        Cache.GetTableFromCache(true);
    }

    public static void GetTableFromCache(bool refreshCache)
    {
        Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}