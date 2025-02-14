using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public static class ElectIDs
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

    public static void UpsertFromEds(List<IdNameAttributes> listIdNameAttributess)
    {
        var changed = false;

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

                changed = true;

                continue;
            }

            var electIdOld = electId.Copy();

            electId.CarrierName = name;
            electId.PayorID = payorId;
            electId.Attributes = attributes;

            changed |= Update(electId, electIdOld);
        }

        if (changed)
        {
            Signalods.SetInvalid(InvalidType.ElectIDs);
        }
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
            return ElectIDCrud.SelectMany("SELECT * FROM electid ORDER BY CarrierName");
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
            GetTableFromCache(false);
        }
    }

    private static readonly ElectIdCache Cache = new();

    public static List<ElectID> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    private static ElectID GetFirstOrDefault(Func<ElectID, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static List<ElectID> GetWhere(Predicate<ElectID> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
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

[Serializable]
public class IdNameAttributes
{
    public string Attributes;
    public string ID;
    public string Name;
}