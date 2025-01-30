using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness.Dentalxchange2016;
using OpenDentBusiness.Eclaims;

namespace OpenDentBusiness;

public class ElectIDs
{
    public static void Insert(ElectID electId)
    {
        ElectIDCrud.Insert(electId);
    }

    public static void Update(ElectID electId)
    {
        ElectIDCrud.Update(electId);
    }

    public static bool Update(ElectID electIdNew, ElectID electIdOld)
    {
        return ElectIDCrud.Update(electIdNew, electIdOld);
    }

    public static void UpsertFromDentalXChange(List<supportedTransPayer> supportedTransPayers)
    {
        var hasChanged = false;

        foreach (var supportedTransPayer in supportedTransPayers)
        {
            var payer = supportedTransPayer;

            var electId = GetFirstOrDefault(x => x.PayorID == payer.PayerIDCode && x.CarrierName == payer.Name && x.CommBridge == EclaimsCommBridge.ClaimConnect);
            if (electId is null)
            {
                electId = new ElectID
                {
                    CarrierName = supportedTransPayer.Name,
                    PayorID = supportedTransPayer.PayerIDCode,
                    CommBridge = EclaimsCommBridge.ClaimConnect,
                    Attributes = string.Join(",", ClaimConnect.GetAttributes(supportedTransPayer).Select(x => (int) x))
                };

                Insert(electId);

                hasChanged = true;

                continue;
            }

            var electIdOld = electId.Copy();

            electId.Attributes = string.Join(",", ClaimConnect.GetAttributes(supportedTransPayer).Select(x => (int) x));

            hasChanged |= Update(electId, electIdOld);
        }

        if (hasChanged) Signalods.SetInvalid(InvalidType.ElectIDs);
    }

    public static void UpsertFromEds(List<IdNameAttributes> listIdNameAttributess)
    {
        var hasChanged = false;

        foreach (var idNameAttributes in listIdNameAttributess)
        {
            var payorId = idNameAttributes.ID;
            var name = idNameAttributes.Name;
            var attributes = idNameAttributes.Attributes;

            if (payorId == "NULL")
            {
                continue;
            }

            var electId = GetFirstOrDefault(x => x.PayorID == payorId && x.CarrierName == name && x.CommBridge == EclaimsCommBridge.EDS);
            if (electId is null)
            {
                electId = new ElectID
                {
                    PayorID = payorId,
                    CarrierName = name,
                    CommBridge = EclaimsCommBridge.EDS,
                    Attributes = attributes
                };

                Insert(electId);

                hasChanged = true;

                continue;
            }

            var electIdOld = electId.Copy();

            electId.CarrierName = name;
            electId.PayorID = payorId;
            electId.Attributes = attributes;

            hasChanged |= Update(electId, electIdOld);
        }

        if (hasChanged) Signalods.SetInvalid(InvalidType.ElectIDs);
    }

    public static ElectID GetId(string payorId)
    {
        return GetFirstOrDefault(x => x.PayorID == payorId);
    }

    public static List<ElectID> GetIDs(string payorId)
    {
        return GetWhere(x => x.PayorID == payorId);
    }

    public static List<string> GetDescripts(string payorId)
    {
        return payorId == "" ? [] : GetIDs(payorId).Select(x => x.CarrierName).ToList();
    }

    private class ElectIdCache : CacheListAbs<ElectID>
    {
        protected override List<ElectID> GetCacheFromDb()
        {
            return ElectIDCrud.SelectMany("SELECT * from electid ORDER BY CarrierName");
        }

        protected override List<ElectID> TableToList(DataTable dataTable)
        {
            return ElectIDCrud.TableToList(dataTable);
        }

        protected override ElectID Copy(ElectID item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ElectID> items)
        {
            return ElectIDCrud.ListToTable(items, "ElectID");
        }

        protected override void FillCacheIfNeeded()
        {
            ElectIDs.GetTableFromCache(false);
        }
    }

    private static readonly ElectIdCache Cache = new();

    public static List<ElectID> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    private static ElectID GetFirstOrDefault(Func<ElectID, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static List<ElectID> GetWhere(Predicate<ElectID> match, bool isShort = false)
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

[Serializable]
public class IdNameAttributes
{
    public string Attributes;
    public string ID;
    public string Name;
}