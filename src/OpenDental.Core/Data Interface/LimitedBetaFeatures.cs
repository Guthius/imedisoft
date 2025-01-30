using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class LimitedBetaFeatures
{
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

    public static void SyncFromHq(List<LimitedBetaFeature> listLimitedBetaFeaturesHq)
    {
        var isCacheInvalid = false;
        //Remove all unclassified features.
        listLimitedBetaFeaturesHq.RemoveAll(x => x.GetLimitedBetaFeatureEnum() == EServiceFeatureInfoEnum.None);
        var listLimitedBetaFeaturesDB = Cache.GetDeepCopy();
        var listLimitedBetaFeaturesToInsert = new List<LimitedBetaFeature>();
        for (var i = 0; i < listLimitedBetaFeaturesHq.Count; i++)
        {
            var limitedBetaFeatureOld = listLimitedBetaFeaturesDB
                .FirstOrDefault(x => x.ClinicNum == listLimitedBetaFeaturesHq[i].ClinicNum && x.LimitedBetaFeatureTypeNum == listLimitedBetaFeaturesHq[i].LimitedBetaFeatureTypeNum);
            if (limitedBetaFeatureOld == null)
            {
                //Insert if one does not exist
                listLimitedBetaFeaturesToInsert.Add(listLimitedBetaFeaturesHq[i]);
                isCacheInvalid = true;
            }
            else
            {
                //Update if the DB has an entry for the existing feature / clinic combo
                //Set the local PK for the listLimitedBetaFeaturesHq.
                listLimitedBetaFeaturesHq[i].LimitedBetaFeatureNum = limitedBetaFeatureOld.LimitedBetaFeatureNum;
                isCacheInvalid |= LimitedBetaFeatureCrud.Update(listLimitedBetaFeaturesHq[i], limitedBetaFeatureOld);
            }
        }

        var listLimitedBetaFeatureNumsDb = listLimitedBetaFeaturesDB.Select(x => x.LimitedBetaFeatureNum);
        var listLimitedBetaFeatureNumsHq = listLimitedBetaFeaturesHq.Select(x => x.LimitedBetaFeatureNum);
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

    private static readonly LimitedBetaFeatureCache Cache = new();

    public static LimitedBetaFeature GetFirstOrDefault(Func<LimitedBetaFeature, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
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