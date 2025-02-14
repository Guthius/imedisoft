using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class AutoCodeConds
{
    public static void Insert(AutoCodeCond autoCodeCond)
    {
        AutoCodeCondCrud.Insert(autoCodeCond);
    }
    
    public static void DeleteForItemNum(long autoCodeItemNum)
    {
        Db.NonQ("DELETE from autocodecond WHERE autocodeitemnum = " + autoCodeItemNum);
    }
    
    public static List<AutoCodeCond> GetListForItem(long autoCodeItemNum)
    {
        return GetWhere(x => x.AutoCodeItemNum == autoCodeItemNum);
    }
    
    public static bool ConditionIsMet(AutoCondition autoCondition, string toothNum, string surf, bool isAdditional, bool willBeMissing, int age)
    {
        return autoCondition switch
        {
            AutoCondition.Anterior => Tooth.IsAnterior(toothNum),
            AutoCondition.Posterior => Tooth.IsPosterior(toothNum),
            AutoCondition.Premolar => Tooth.IsPreMolar(toothNum),
            AutoCondition.Molar => Tooth.IsMolar(toothNum),
            AutoCondition.One_Surf => surf.Length == 1,
            AutoCondition.Two_Surf => surf.Length == 2,
            AutoCondition.Three_Surf => surf.Length == 3,
            AutoCondition.Four_Surf => surf.Length == 4,
            AutoCondition.Five_Surf => surf.Length == 5,
            AutoCondition.First => !isAdditional,
            AutoCondition.EachAdditional => isAdditional,
            AutoCondition.Maxillary => Tooth.IsMaxillary(toothNum),
            AutoCondition.Mandibular => !Tooth.IsMaxillary(toothNum),
            AutoCondition.Primary => Tooth.IsPrimary(toothNum),
            AutoCondition.Permanent => !Tooth.IsPrimary(toothNum),
            AutoCondition.Pontic => willBeMissing,
            AutoCondition.Retainer => !willBeMissing,
            AutoCondition.AgeOver18 => age > 18,
            _ => false
        };
    }
    
    private class AutoCodeCondCache : CacheListAbs<AutoCodeCond>
    {
        protected override List<AutoCodeCond> GetCacheFromDb()
        {
            return AutoCodeCondCrud.SelectMany("SELECT * from autocodecond ORDER BY Cond");
        }

        protected override List<AutoCodeCond> TableToList(DataTable dataTable)
        {
            return AutoCodeCondCrud.TableToList(dataTable);
        }

        protected override AutoCodeCond Copy(AutoCodeCond item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<AutoCodeCond> items)
        {
            return AutoCodeCondCrud.ListToTable(items, "AutoCodeCond");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }
    
    private static readonly AutoCodeCondCache Cache = new();

    public static List<AutoCodeCond> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static List<AutoCodeCond> GetWhere(Predicate<AutoCodeCond> predicate, bool shortList = false)
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