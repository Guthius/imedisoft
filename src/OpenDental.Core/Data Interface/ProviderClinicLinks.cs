using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ProviderClinicLinks
{
    public static List<ProviderClinicLink> GetForProvider(long provNum)
    {
        return GetWhere(x => x.ProvNum == provNum);
    }

    public static List<ProviderClinicLink> GetAllForClinics(List<long> listClinicNums)
    {
        return GetWhere(x => listClinicNums.Contains(x.ClinicNum));
    }

    public static List<long> GetProvsRestrictedToOtherClinics(List<long> listClinicNums)
    {
        if (listClinicNums.IsNullOrEmpty() || (listClinicNums.Count == 1 && listClinicNums.First() == 0))
            return [];
        var hashSetProvsTheseClinics = new HashSet<long>(GetAllForClinics(listClinicNums).Select(x => x.ProvNum));
        return GetWhere(x => !listClinicNums.Contains(x.ClinicNum) && !hashSetProvsTheseClinics.Contains(x.ProvNum))
            .Select(x => x.ProvNum).Distinct().ToList();
    }

    public static bool Sync(List<ProviderClinicLink> listNew, List<ProviderClinicLink> listDB)
    {
        return ProviderClinicLinkCrud.Sync(listNew, listDB);
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
            ProviderClinicLinks.GetTableFromCache(false);
        }
    }

    private static readonly ProviderClinicLinkCache Cache = new();

    public static List<ProviderClinicLink> GetWhere(Predicate<ProviderClinicLink> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}