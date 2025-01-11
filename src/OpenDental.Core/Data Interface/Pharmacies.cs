using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Pharmacies
{
    ///<Summary>Gets one Pharmacy from the database.</Summary>
    public static Pharmacy GetOne(long pharmacyNum)
    {
        return PharmacyCrud.SelectOne(pharmacyNum);
    }

    ///<summary>Gets all pharmacies ordered by StoreName from the database.</summary>
    public static List<Pharmacy> GetAllNoCache()
    {
        return PharmacyCrud.SelectMany("SELECT * FROM pharmacy ORDER BY StoreName");
    }

    
    public static long Insert(Pharmacy pharmacy)
    {
        return PharmacyCrud.Insert(pharmacy);
    }

    
    public static void Update(Pharmacy pharmacy)
    {
        PharmacyCrud.Update(pharmacy);
    }

    
    public static void DeleteObject(long pharmacyNum)
    {
        PharmacyCrud.Delete(pharmacyNum);
    }

    public static string GetDescription(long PharmacyNum)
    {
        var pharmacy = GetFirstOrDefault(x => x.PharmacyNum == PharmacyNum);
        return pharmacy == null ? "" : pharmacy.StoreName;
    }

    public static List<long> GetChangedSincePharmacyNums(DateTime changedSince)
    {
        var command = "SELECT PharmacyNum FROM pharmacy WHERE DateTStamp > " + SOut.DateT(changedSince);
        var dt = DataCore.GetTable(command);
        var provnums = new List<long>(dt.Rows.Count);
        for (var i = 0; i < dt.Rows.Count; i++) provnums.Add(SIn.Long(dt.Rows[i]["PharmacyNum"].ToString()));
        return provnums;
    }

    ///<summary>Used along with GetChangedSincePharmacyNums</summary>
    public static List<Pharmacy> GetMultPharmacies(List<long> pharmacyNums)
    {
        var strPharmacyNums = "";
        DataTable table;
        if (pharmacyNums.Count > 0)
        {
            for (var i = 0; i < pharmacyNums.Count; i++)
            {
                if (i > 0) strPharmacyNums += "OR ";
                strPharmacyNums += "PharmacyNum='" + pharmacyNums[i] + "' ";
            }

            var command = "SELECT * FROM pharmacy WHERE " + strPharmacyNums;
            table = DataCore.GetTable(command);
        }
        else
        {
            table = new DataTable();
        }

        var multPharmacys = PharmacyCrud.TableToList(table).ToArray();
        var pharmacyList = new List<Pharmacy>(multPharmacys);
        return pharmacyList;
    }

    /// <summary>Gets a list of Pharmacies for a given clinic based on PharmClinic links.</summary>
    /// <param name="clinicNum">The primary key of the clinic.</param>
    public static List<Pharmacy> GetPharmaciesForClinic(long clinicNum)
    {
        var command = "SELECT * "
                      + "FROM pharmacy "
                      + "WHERE PharmacyNum IN ("
                      + "SELECT PharmacyNum "
                      + "FROM pharmclinic "
                      + "WHERE clinicNum = " + SOut.Long(clinicNum)
                      + ") ORDER BY StoreName";
        return PharmacyCrud.SelectMany(command);
    }

    ///<summary>Gets all Pharmacies from database. Returns empty list if not found.</summary>
    public static List<Pharmacy> GetPharmaciesForApi(int limit, int offset)
    {
        var command = "SELECT * FROM pharmacy ";
        command += "ORDER BY PharmacyNum " //same fixed order each time
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        return PharmacyCrud.SelectMany(command);
    }

    #region CachePattern

    private class PharmacyCache : CacheListAbs<Pharmacy>
    {
        protected override List<Pharmacy> GetCacheFromDb()
        {
            var command = "SELECT * FROM pharmacy ORDER BY StoreName";
            return PharmacyCrud.SelectMany(command);
        }

        protected override List<Pharmacy> TableToList(DataTable dataTable)
        {
            return PharmacyCrud.TableToList(dataTable);
        }

        protected override Pharmacy Copy(Pharmacy item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Pharmacy> items)
        {
            return PharmacyCrud.ListToTable(items, "Pharmacy");
        }

        protected override void FillCacheIfNeeded()
        {
            Pharmacies.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly PharmacyCache _pharmacyCache = new();

    public static List<Pharmacy> GetDeepCopy(bool isShort = false)
    {
        return _pharmacyCache.GetDeepCopy(isShort);
    }

    public static Pharmacy GetFirstOrDefault(Func<Pharmacy, bool> match, bool isShort = false)
    {
        return _pharmacyCache.GetFirstOrDefault(match, isShort);
    }

    public static List<Pharmacy> GetWhere(Predicate<Pharmacy> match, bool isShort = false)
    {
        return _pharmacyCache.GetWhere(match, isShort);
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
        _pharmacyCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _pharmacyCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _pharmacyCache.ClearCache();
    }

    #endregion
}