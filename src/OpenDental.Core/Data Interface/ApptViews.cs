using System;
using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public static class ApptViews
{
    public const long ApptViewNumNone = 0;

    public static long Insert(ApptView apptView)
    {
        return ApptViewCrud.Insert(apptView);
    }

    public static void Update(ApptView apptView)
    {
        ApptViewCrud.Update(apptView);
    }

    public static void Delete(ApptView apptView)
    {
        Db.NonQ("DELETE FROM apptview WHERE ApptViewNum = " + apptView.ApptViewNum);
    }

    public static bool IsNoneView(ApptView apptView)
    {
        return apptView == null || apptView.ApptViewNum == ApptViewNumNone;
    }

    public static List<ApptView> GetForClinic(long clinicNum = 0, bool isShort = true)
    {
        return clinicNum > 0 ? GetWhere(x => x.ClinicNum == clinicNum, isShort) : GetDeepCopy(isShort);
    }

    public static ApptView GetApptView(long apptViewNum)
    {
        return GetFirstOrDefault(x => x.ApptViewNum == apptViewNum);
    }

    private class ApptViewCache : CacheListAbs<ApptView>
    {
        protected override List<ApptView> GetCacheFromDb()
        {
            return ApptViewCrud.SelectMany("SELECT * FROM apptview ORDER BY ClinicNum,ItemOrder");
        }

        protected override List<ApptView> TableToList(DataTable dataTable)
        {
            return ApptViewCrud.TableToList(dataTable);
        }

        protected override ApptView Copy(ApptView item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ApptView> items)
        {
            return ApptViewCrud.ListToTable(items, "ApptView");
        }

        protected override void FillCacheIfNeeded()
        {
            ApptViews.GetTableFromCache(false);
        }
    }

    private static readonly ApptViewCache Cache = new();

    public static List<ApptView> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    private static ApptView GetFirstOrDefault(Func<ApptView, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static List<ApptView> GetWhere(Predicate<ApptView> predicate, bool shortList = false)
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