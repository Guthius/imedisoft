using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public static class ApiSubscriptions
{
    private class ApiSubscriptionCache : CacheListAbs<ApiSubscription>
    {
        protected override List<ApiSubscription> GetCacheFromDb()
        {
            return ApiSubscriptionCrud.SelectMany("SELECT * FROM apisubscription");
        }

        protected override List<ApiSubscription> TableToList(DataTable dataTable)
        {
            return ApiSubscriptionCrud.TableToList(dataTable);
        }

        protected override ApiSubscription Copy(ApiSubscription item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ApiSubscription> items)
        {
            return ApiSubscriptionCrud.ListToTable(items, "ApiSubscription");
        }

        protected override void FillCacheIfNeeded()
        {
            ApiSubscriptions.GetTableFromCache(false);
        }
    }

    private static readonly ApiSubscriptionCache Cache = new();

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}