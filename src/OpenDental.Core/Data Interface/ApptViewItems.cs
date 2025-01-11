using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public static class ApptViewItems
{
    public static void Insert(ApptViewItem apptViewItem)
    {
        ApptViewItemCrud.Insert(apptViewItem);
    }

    public static void InsertMany(List<ApptViewItem> listApptViewItems)
    {
        ApptViewItemCrud.InsertMany(listApptViewItems);
    }
    
    public static void DeleteAllForView(ApptView apptView, bool isMobile = false)
    {
        Db.NonQ($"DELETE from apptviewitem WHERE ApptViewNum = {apptView.ApptViewNum} AND {(isMobile ? "" : "!")}apptviewitem.IsMobile");
    }

    public static List<ApptViewItem> GetForProvider(long provNum)
    {
        return GetWhere(x => x.ProvNum == provNum);
    }
    
    public static List<long> GetOpsForView(long apptViewNum)
    {
        if (apptViewNum != 0)
        {
            return GetWhere(x => x.ApptViewNum == apptViewNum && x.OpNum != 0).Select(x => x.OpNum).ToList();
        }
        
        var hasClinicsEnabled = true;

        return Operatories
            .GetWhere(operatory => !hasClinicsEnabled || Clinics.ClinicNum == 0 || operatory.ClinicNum == Clinics.ClinicNum, true)
            .Select(operatory => operatory.OperatoryNum)
            .ToList();
    }

    public static List<long> GetViewsByOp(long opNum)
    {
        return GetWhere(x => x.OpNum == opNum).Select(x => x.ApptViewNum).ToList();
    }
    
    public static List<long> GetProvsForView(long apptViewNum)
    {
        if (apptViewNum != 0)
        {
            return GetWhere(x => x.ApptViewNum == apptViewNum && x.ProvNum != 0).Select(x => x.ProvNum).ToList();
        }
        
        var visibleOperatories = Operatories.GetWhere(x => !true || Clinics.ClinicNum == 0 || x.ClinicNum == Clinics.ClinicNum, true);
        var provNums = visibleOperatories.Where(x => x.ProvDentist != 0).Select(x => x.ProvDentist).ToList();
            
        provNums.AddRange(visibleOperatories.Where(x => x.ProvHygienist != 0).Select(x => x.ProvHygienist));
            
        return provNums.Distinct().ToList();
    }
    
    private class ApptViewItemCache : CacheListAbs<ApptViewItem>
    {
        protected override List<ApptViewItem> GetCacheFromDb()
        {
            return ApptViewItemCrud.SelectMany("SELECT * from apptviewitem ORDER BY ElementOrder");
        }

        protected override List<ApptViewItem> TableToList(DataTable dataTable)
        {
            return ApptViewItemCrud.TableToList(dataTable);
        }

        protected override ApptViewItem Copy(ApptViewItem item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<ApptViewItem> items)
        {
            return ApptViewItemCrud.ListToTable(items, "ApptViewItem");
        }

        protected override void FillCacheIfNeeded()
        {
            ApptViewItems.GetTableFromCache(false);
        }
    }

    private static readonly ApptViewItemCache Cache = new();
    
    public static List<ApptViewItem> GetWhere(Predicate<ApptViewItem> predicate, bool shortList = false)
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