using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using OpenDentBusiness.Crud;
using OpenDentBusiness.Dentalxchange2016;
using OpenDentBusiness.Eclaims;

namespace OpenDentBusiness;

public class ElectIDs
{
    public static long Insert(ElectID electID)
    {
        return ElectIDCrud.Insert(electID);
    }

    public static void Update(ElectID electID)
    {
        ElectIDCrud.Update(electID);
    }

    public static bool Update(ElectID electIDNew, ElectID electIDOld)
    {
        return ElectIDCrud.Update(electIDNew, electIDOld);
    }

    ///<summary>Takes a list of PayorIDs from DxC's getPayerListService API method. Inserts/updates new or existing electids.</summary>
    public static void UpsertFromDentalXChange(List<supportedTransPayer> listSupportedTransPayers)
    {
        var hasChanged = false;
        for (var i = 0; i < listSupportedTransPayers.Count; i++)
        {
            var supportedTransPayer = listSupportedTransPayers[i];
            var electID = GetFirstOrDefault(x => x.PayorID == supportedTransPayer.PayerIDCode && x.CarrierName == supportedTransPayer.Name && x.CommBridge == EclaimsCommBridge.ClaimConnect);
            if (electID is null)
            {
                electID = new ElectID();
                electID.CarrierName = supportedTransPayer.Name;
                electID.PayorID = supportedTransPayer.PayerIDCode;
                electID.CommBridge = EclaimsCommBridge.ClaimConnect;
                electID.Attributes = string.Join(",", ClaimConnect.GetAttributes(supportedTransPayer).Select(x => (int) x));
                Insert(electID);
                hasChanged = true;
                continue;
            }

            var electIDOld = electID.Copy();
            electID.Attributes = string.Join(",", ClaimConnect.GetAttributes(supportedTransPayer).Select(x => (int) x));
            hasChanged |= Update(electID, electIDOld);
        }

        if (hasChanged) Signalods.SetInvalid(InvalidType.ElectIDs);
    }

    ///<summary>Takes a list of PayorIDs from EDS's List_Payers API method. Inserts/updates new or existing electids.</summary>
    public static void UpsertFromEDS(List<IdNameAttributes> listIdNameAttributess)
    {
        var hasChanged = false;
        for (var i = 0; i < listIdNameAttributess.Count; i++)
        {
            var idNameAttributes = listIdNameAttributess[i];
            var payorID = idNameAttributes.ID;
            var name = idNameAttributes.Name;
            var attributes = idNameAttributes.Attributes;
            if (payorID == "NULL") //EDS may send over an empty electronic id with "NULL" as the payer id.
                continue;
            var electID = GetFirstOrDefault(x => x.PayorID == payorID && x.CarrierName == name && x.CommBridge == EclaimsCommBridge.EDS);
            if (electID is null)
            {
                electID = new ElectID();
                electID.PayorID = payorID;
                electID.CarrierName = name;
                electID.CommBridge = EclaimsCommBridge.EDS;
                electID.Attributes = attributes;
                Insert(electID);
                hasChanged = true;
                continue;
            }

            var electIDOld = electID.Copy();
            electID.CarrierName = name;
            electID.PayorID = payorID;
            electID.Attributes = attributes;
            hasChanged |= Update(electID, electIDOld);
        }

        if (hasChanged) Signalods.SetInvalid(InvalidType.ElectIDs);
    }

    
    public static List<ProviderSupplementalID> GetRequiredIdents(string payorID)
    {
        var electID = GetID(payorID);
        if (electID == null) return new List<ProviderSupplementalID>();
        if (electID.ProviderTypes == "") return new List<ProviderSupplementalID>();
        var listProvTypes = electID.ProviderTypes.Split(',').ToList();
        if (listProvTypes.Count == 0) return new List<ProviderSupplementalID>();
        var listProviderSupplementalIDsRet = new List<ProviderSupplementalID>();
        for (var i = 0; i < listProvTypes.Count; i++) listProviderSupplementalIDsRet[i] = (ProviderSupplementalID) Convert.ToInt32(listProvTypes[i]);
        /*
        if(electID=="SB601"){//BCBS of GA
            retVal=new ProviderSupplementalID[2];
            retVal[0]=ProviderSupplementalID.BlueShield;
            retVal[1]=ProviderSupplementalID.SiteNumber;
        }*/
        return listProviderSupplementalIDsRet;
    }

    /// <summary>
    ///     Gets ONE ElectID that uses the supplied payorID. Even if there are multiple payors using that ID.  So use this
    ///     carefully.
    /// </summary>
    public static ElectID GetID(string payorID)
    {
        return GetFirstOrDefault(x => x.PayorID == payorID);
    }

    /// <summary>
    ///     Gets an arrayList of ElectID objects based on a supplied payorID. If no matches found, then returns array of 0
    ///     length. Used to display payors in FormInsPlan and also to get required idents.  This means that all payors with the
    ///     same ID should have the same required idents and notes.
    /// </summary>
    public static List<ElectID> GetIDs(string payorID)
    {
        return GetWhere(x => x.PayorID == payorID);
    }

    /// <summary>
    ///     Gets the names of the payors to display based on the payorID.  Since carriers sometimes share payorIDs, there
    ///     will often be multiple payor names returned.
    /// </summary>
    public static List<string> GetDescripts(string payorID)
    {
        if (payorID == "") return new List<string>();
        return GetIDs(payorID).Select(x => x.CarrierName).ToList();
    }

    public static bool IsMedicaid(string payorID)
    {
        var electID = GetID(payorID);
        if (electID == null) return false;
        return electID.IsMedicaid;
    }

    #region CachePattern

    private class ElectIDCache : CacheListAbs<ElectID>
    {
        protected override List<ElectID> GetCacheFromDb()
        {
            var command = "SELECT * from electid ORDER BY CarrierName";
            return ElectIDCrud.SelectMany(command);
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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ElectIDCache _electIDCache = new();

    public static List<ElectID> GetDeepCopy(bool isShort = false)
    {
        return _electIDCache.GetDeepCopy(isShort);
    }

    private static ElectID GetFirstOrDefault(Func<ElectID, bool> match, bool isShort = false)
    {
        return _electIDCache.GetFirstOrDefault(match, isShort);
    }

    public static List<ElectID> GetWhere(Predicate<ElectID> match, bool isShort = false)
    {
        return _electIDCache.GetWhere(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _electIDCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _electIDCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _electIDCache.ClearCache();
    }

    #endregion
}

[Serializable]
public class IdNameAttributes
{
    public string Attributes;
    public string ID;
    public string Name;
}