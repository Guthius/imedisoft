using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

///<summary>LimitedBetaFeatures show which beta eServices clinics are allowed to use. </summary>
public class LimitedBetaFeatures
{
    ///<summary>Gets one LimitedBetaFeature from the db.</summary>
    public static LimitedBetaFeature GetOne(long limitedBetaFeatureNum)
    {
        return LimitedBetaFeatureCrud.SelectOne(limitedBetaFeatureNum);
    }

    /// <summary>
    ///     Returns true if the clinic is signed up for limited beta feature, or the feature is marked finished at hq.
    ///     This does not guarantee that the clinic should have access to the feature. Only that the limited beta restriction
    ///     is met for this clinic.
    ///     If a given feature typically requires further validation (usually via prefs or HQ validation) then you must perform
    ///     that validation after this check.
    ///     clinicNum of -1 indicates a clinic independent feature.
    ///     The 'feature' parameter is the long value of the EServiceFeatureInfoEnum.
    public static bool IsAllowed(EServiceFeatureInfoEnum eServiceFeatureInfoEnum, long clinicNum = -1)
    {
        #region Completed Override

        //If this eServiceFeatureInfoEnum has a completed attribute, return true. This acts as an override once features are marked complete.
        var eServiceFeatureStatusAttribute = EnumTools.GetAttributeOrDefault<EServiceFeatureStatusAttribute>(eServiceFeatureInfoEnum);
        if (eServiceFeatureStatusAttribute.IsFinished) return true; //Overrides any feature status checking.

        #endregion

        var limitedBetaFeature = GetFirstOrDefault(x => x.LimitedBetaFeatureTypeNum == (long) eServiceFeatureInfoEnum && (x.ClinicNum == clinicNum || x.ClinicNum == -1));
        //Implicit that if a limitedBetaFeature is no longer in the list, it is finished and can be displayed / used. Its available to the general user base.
        //If an entry does exist, we need to check if the office is signed up for the feature
        return limitedBetaFeature?.IsSignedUp ?? false;
    }

    /// <summary>
    ///     Syncs the loacal LimitedBetaFeature table with the list passed in. Ignores rows with undefined
    ///     EServiceFeatureInfoEnums.
    /// </summary>
    public static void SyncFromHQ(List<LimitedBetaFeature> listLimitedBetaFeaturesHQ)
    {
        var isCacheInvalid = false;
        //Remove all unclassified features.
        listLimitedBetaFeaturesHQ.RemoveAll(x => x.GetLimitedBetaFeatureEnum() == EServiceFeatureInfoEnum.None);
        var listLimitedBetaFeaturesDB = _limitedBetaFeatureCache.GetDeepCopy();
        var listLimitedBetaFeaturesToInsert = new List<LimitedBetaFeature>();
        for (var i = 0; i < listLimitedBetaFeaturesHQ.Count; i++)
        {
            var limitedBetaFeatureOld = listLimitedBetaFeaturesDB
                .FirstOrDefault(x => x.ClinicNum == listLimitedBetaFeaturesHQ[i].ClinicNum && x.LimitedBetaFeatureTypeNum == listLimitedBetaFeaturesHQ[i].LimitedBetaFeatureTypeNum);
            if (limitedBetaFeatureOld == null)
            {
                //Insert if one does not exist
                listLimitedBetaFeaturesToInsert.Add(listLimitedBetaFeaturesHQ[i]);
                isCacheInvalid = true;
            }
            else
            {
                //Update if the DB has an entry for the existing feature / clinic combo
                //Set the local PK for the listLimitedBetaFeaturesHq.
                listLimitedBetaFeaturesHQ[i].LimitedBetaFeatureNum = limitedBetaFeatureOld.LimitedBetaFeatureNum;
                isCacheInvalid |= LimitedBetaFeatureCrud.Update(listLimitedBetaFeaturesHQ[i], limitedBetaFeatureOld);
            }
        }

        var listLimitedBetaFeatureNumsDb = listLimitedBetaFeaturesDB.Select(x => x.LimitedBetaFeatureNum);
        var listLimitedBetaFeatureNumsHq = listLimitedBetaFeaturesHQ.Select(x => x.LimitedBetaFeatureNum);
        var listLimitedBetaFeatureNumsToDelete = listLimitedBetaFeatureNumsDb.Except(listLimitedBetaFeatureNumsHq).ToList();
        isCacheInvalid |= listLimitedBetaFeatureNumsToDelete.Count > 0;
        //Perform the bulk inserts and deletes.
        LimitedBetaFeatureCrud.DeleteMany(listLimitedBetaFeatureNumsToDelete);
        LimitedBetaFeatureCrud.InsertMany(listLimitedBetaFeaturesToInsert);
        if (isCacheInvalid)
        {
            Signalods.SetInvalid(InvalidType.LimitedBetaFeature);
            RefreshCache();
        }
    }

    #region Cache Pattern

    //This region can be eliminated if this is not a table type with cached data.
    //If leaving this region in place, be sure to add GetTableFromCache and FillCacheFromTable to the Cache.cs file with all the other Cache types.
    //Also, consider making an invalid type for this class in Cache.GetAllCachedInvalidTypes() if needed.

    private class LimitedBetaFeatureCache : CacheListAbs<LimitedBetaFeature>
    {
        protected override List<LimitedBetaFeature> GetCacheFromDb()
        {
            var command = "SELECT * FROM limitedbetafeature";
            return LimitedBetaFeatureCrud.SelectMany(command);
        }

        protected override List<LimitedBetaFeature> TableToList(DataTable dataTable)
        {
            return LimitedBetaFeatureCrud.TableToList(dataTable);
        }

        protected override LimitedBetaFeature Copy(LimitedBetaFeature item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<LimitedBetaFeature> items)
        {
            return LimitedBetaFeatureCrud.ListToTable(items, "LimitedBetaFeature");
        }

        protected override void FillCacheIfNeeded()
        {
            LimitedBetaFeatures.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly LimitedBetaFeatureCache _limitedBetaFeatureCache = new();

    public static List<LimitedBetaFeature> GetDeepCopy(bool isShort = false)
    {
        return _limitedBetaFeatureCache.GetDeepCopy(isShort);
    }

    public static int GetCount(bool isShort = false)
    {
        return _limitedBetaFeatureCache.GetCount(isShort);
    }

    public static bool GetExists(Predicate<LimitedBetaFeature> match, bool isShort = false)
    {
        return _limitedBetaFeatureCache.GetExists(match, isShort);
    }

    public static int GetFindIndex(Predicate<LimitedBetaFeature> match, bool isShort = false)
    {
        return _limitedBetaFeatureCache.GetFindIndex(match, isShort);
    }

    public static LimitedBetaFeature GetFirst(bool isShort = false)
    {
        return _limitedBetaFeatureCache.GetFirst(isShort);
    }

    public static LimitedBetaFeature GetFirst(Func<LimitedBetaFeature, bool> match, bool isShort = false)
    {
        return _limitedBetaFeatureCache.GetFirst(match, isShort);
    }

    public static LimitedBetaFeature GetFirstOrDefault(Func<LimitedBetaFeature, bool> match, bool isShort = false)
    {
        return _limitedBetaFeatureCache.GetFirstOrDefault(match, isShort);
    }

    public static LimitedBetaFeature GetLast(bool isShort = false)
    {
        return _limitedBetaFeatureCache.GetLast(isShort);
    }

    public static LimitedBetaFeature GetLastOrDefault(Func<LimitedBetaFeature, bool> match, bool isShort = false)
    {
        return _limitedBetaFeatureCache.GetLastOrDefault(match, isShort);
    }

    public static List<LimitedBetaFeature> GetWhere(Predicate<LimitedBetaFeature> match, bool isShort = false)
    {
        return _limitedBetaFeatureCache.GetWhere(match, isShort);
    }

    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _limitedBetaFeatureCache.FillCacheFromTable(table);
    }

    /// <summary>Returns the cache in the form of a DataTable. Always refreshes the ClientWeb's cache.</summary>
    /// <param name="doRefreshCache">If true, will refresh the cache if RemotingRole is ClientDirect or ServerWeb.</param>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _limitedBetaFeatureCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _limitedBetaFeatureCache.ClearCache();
    }

    #endregion Cache Pattern
}