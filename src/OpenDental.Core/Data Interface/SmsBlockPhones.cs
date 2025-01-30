using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class SmsBlockPhones
{
    public static void Insert(SmsBlockPhone smsBlockPhone)
    {
        SmsBlockPhoneCrud.Insert(smsBlockPhone);
    }

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
    
    private static readonly SmsBlockPhoneCache Cache = new();
    
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}