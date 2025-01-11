using System;
using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class CanadianNetworks
{
    public static CanadianNetwork GetNetwork(long networkNum, Clearinghouse clearinghouseClin, Claim claim = null)
    {
        var canadianNetwork = GetFirstOrDefault(x => x.CanadianNetworkNum == networkNum);
        //CSI is the previous name for the network now known as INSTREAM.
        //According to Telus 05/18/2023: "TELUS has signed a contract with Instream Canada so that TELUS can send Denturists and Hygienists claims to Instream Canada
        //for carriers defined as Instream, and also, Instream Canada can send Denturists and Hygienists claims to TELUS for carriers defined as TELUS"
        //Dentist claims must not be redirected.
        if (clearinghouseClin.CommBridge == EclaimsCommBridge.Claimstream && canadianNetwork.Abbrev == "CSI" && claim != null)
        {
            var providerTreat = Providers.GetFirstOrDefault(x => x.ProvNum == claim.ProvTreat);
            if (providerTreat.NationalProvID.StartsWith("202") || providerTreat.NationalProvID.StartsWith("8")) //Hygienist or Denturist.
                //Network redirect only allowed for Hygienists or Denturists.
                canadianNetwork = GetFirstOrDefault(x => x.Abbrev == "TELUS B");
        }

        return canadianNetwork;
    }

    private class CanadianNetworkCache : CacheListAbs<CanadianNetwork>
    {
        protected override List<CanadianNetwork> GetCacheFromDb()
        {
            var command = "SELECT * FROM canadiannetwork ORDER BY Descript";
            return CanadianNetworkCrud.SelectMany(command);
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

    public static List<CanadianNetwork> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static CanadianNetwork GetFirstOrDefault(Func<CanadianNetwork, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static void GetTableFromCache(bool doRefreshCache)
    {
        Cache.GetTableFromCache(doRefreshCache);
    }
}