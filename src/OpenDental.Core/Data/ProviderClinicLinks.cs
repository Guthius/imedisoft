using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class ProviderClinicLinks
{
    public static List<ProviderClinicLink> GetForProvider(long provNum)
    {
        return GetWhere(x => x.ProvNum == provNum);
    }

    public static List<ProviderClinicLink> GetAllForClinics(List<long> clinicNums)
    {
        return GetWhere(x => clinicNums.Contains(x.ClinicNum));
    }

    public static List<long> GetProvsRestrictedToOtherClinics(List<long> clinicNums)
    {
        if (clinicNums is not {Count: > 0} || clinicNums[0] == 0)
        {
            return [];
        }

        var provsTheseClinics = new HashSet<long>(GetAllForClinics(clinicNums).Select(x => x.ProvNum));

        return GetWhere(x =>
                !clinicNums.Contains(x.ClinicNum) &&
                !provsTheseClinics.Contains(x.ProvNum))
            .Select(x => x.ProvNum)
            .Distinct()
            .ToList();
    }

    public static bool Sync(List<ProviderClinicLink> providerClinicLinks, List<ProviderClinicLink> providerClinicLinksDb)
    {
        return ProviderClinicLinkCrud.Sync(providerClinicLinks, providerClinicLinksDb);
    }

    private class ProviderClinicLinkCache : CacheListAbs<ProviderClinicLink>
    {
        protected override List<ProviderClinicLink> GetCacheFromDb()
        {
            return ProviderClinicLinkCrud.SelectMany("SELECT * FROM providercliniclink");
        }

        protected override List<ProviderClinicLink> TableToList(DataTable dataTable)
        {
            return ProviderClinicLinkCrud.TableToList(dataTable);
        }

        protected override ProviderClinicLink Copy(ProviderClinicLink item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ProviderClinicLink> items)
        {
            return ProviderClinicLinkCrud.ListToTable(items, "ProviderClinicLink");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly ProviderClinicLinkCache Cache = new();

    public static List<ProviderClinicLink> GetWhere(Predicate<ProviderClinicLink> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}