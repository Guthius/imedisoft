using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Providers;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class CanadianNetworks
{
    public static CanadianNetwork GetNetwork(long networkNum, Clearinghouse clearinghouseClin, Claim claim = null)
    {
        var canadianNetwork = GetFirstOrDefault(x => x.CanadianNetworkNum == networkNum);

        if (clearinghouseClin.CommBridge != EclaimsCommBridge.Claimstream || canadianNetwork.Abbrev != "CSI" || claim == null)
        {
            return canadianNetwork;
        }

        var providerTreat = Providers.GetFirstOrDefault(x => x.Id == claim.ProvTreat);

        if (providerTreat.NationalProviderId.StartsWith("202") || providerTreat.NationalProviderId.StartsWith("8"))
        {
            canadianNetwork = GetFirstOrDefault(x => x.Abbrev == "TELUS B");
        }

        return canadianNetwork;
    }

    private class CanadianNetworkCache : CacheListAbs<CanadianNetwork>
    {
        protected override List<CanadianNetwork> GetCacheFromDb()
        {
            return CanadianNetworkCrud.SelectMany("SELECT * FROM canadiannetwork ORDER BY Descript");
        }

        protected override List<CanadianNetwork> TableToList(DataTable dataTable)
        {
            return CanadianNetworkCrud.TableToList(dataTable);
        }

        protected override CanadianNetwork Copy(CanadianNetwork item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<CanadianNetwork> items)
        {
            return CanadianNetworkCrud.ListToTable(items, "CanadianNetwork");
        }

        protected override void FillCacheIfNeeded()
        {
            CanadianNetworks.GetTableFromCache(false);
        }
    }

    private static readonly CanadianNetworkCache Cache = new();

    public static List<CanadianNetwork> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static CanadianNetwork GetFirstOrDefault(Func<CanadianNetwork, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static void GetTableFromCache(bool refreshCache)
    {
        Cache.GetTableFromCache(refreshCache);
    }
}