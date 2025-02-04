using System.Collections.Generic;
using System.Data;
using OpenDentBusiness;

namespace Imedisoft.Core.Caching;

public abstract class CacheAbs<TItem> where TItem : TableBase
{
    protected abstract void OnNewCacheReceived(List<TItem> items);
    protected abstract List<TItem> GetCacheFromDb();
    protected abstract List<TItem> TableToList(DataTable dataTable);
    protected abstract TItem Copy(TItem item);
    protected abstract DataTable ToDataTable();
    protected abstract void FillCacheIfNeeded();
    protected abstract bool IsNotFilled();

    public abstract void ClearCache();

    protected CacheAbs()
    {
        Cache.TrackCacheObject(this);
    }

    private void FillCache(FillCacheSource source, DataTable table)
    {
        var items = source switch
        {
            FillCacheSource.Database => GetCacheFromDb(),
            FillCacheSource.DataTable => TableToList(table),
            _ => []
        };

        OnNewCacheReceived(items);
    }

    public DataTable GetTableFromCache(bool refreshCache)
    {
        if (IsNotFilled())
        {
            refreshCache = true;
        }

        if (refreshCache)
        {
            FillCache(FillCacheSource.Database, null);
        }

        return ToDataTable();
    }

    private enum FillCacheSource
    {
        Database,
        DataTable
    }
}