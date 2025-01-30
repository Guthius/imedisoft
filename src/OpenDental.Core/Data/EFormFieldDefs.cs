using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class EFormFieldDefs
{
    public static void Insert(EFormFieldDef eFormFieldDef)
    {
        EFormFieldDefCrud.Insert(eFormFieldDef);
    }

    public static void Update(EFormFieldDef eFormFieldDef)
    {
        EFormFieldDefCrud.Update(eFormFieldDef);
    }

    public static void Delete(long eFormFieldDefNum)
    {
        LanguagePats.DeleteForEFormFieldDef(eFormFieldDefNum);
        EFormFieldDefCrud.Delete(eFormFieldDefNum);
    }

    public static bool IsHorizStackableType(EnumEFormFieldType enumEFormFieldType)
    {
        return enumEFormFieldType switch
        {
            EnumEFormFieldType.CheckBox or
                EnumEFormFieldType.DateField or
                EnumEFormFieldType.Label or
                EnumEFormFieldType.TextField or
                EnumEFormFieldType.RadioButtons
                => true,

            EnumEFormFieldType.SigBox or
                EnumEFormFieldType.PageBreak or
                EnumEFormFieldType.MedicationList
                => false,

            _ => throw new Exception("Missing a type.")
        };
    }
    
    private class EFormFieldDefCache : CacheListAbs<EFormFieldDef>
    {
        protected override List<EFormFieldDef> GetCacheFromDb()
        {
            return EFormFieldDefCrud.SelectMany("SELECT * FROM eformfielddef ORDER BY ItemOrder");
        }

        protected override List<EFormFieldDef> TableToList(DataTable dataTable)
        {
            return EFormFieldDefCrud.TableToList(dataTable);
        }

        protected override EFormFieldDef Copy(EFormFieldDef item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<EFormFieldDef> items)
        {
            return EFormFieldDefCrud.ListToTable(items, "EFormFieldDef");
        }

        protected override void FillCacheIfNeeded()
        {
            EFormFieldDefs.GetTableFromCache(false);
        }
    }

    private static readonly EFormFieldDefCache Cache = new();

    public static void ClearCache()
    {
        Cache.ClearCache();
    }

    public static List<EFormFieldDef> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static List<EFormFieldDef> GetWhere(Predicate<EFormFieldDef> match, bool isShort = false)
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
}