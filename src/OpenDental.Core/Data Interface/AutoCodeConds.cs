using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class AutoCodeConds
{
    
    public static long Insert(AutoCodeCond autoCodeCond)
    {
        return AutoCodeCondCrud.Insert(autoCodeCond);
    }

    
    public static void DeleteForItemNum(long autoCodeItemNum)
    {
        var command = "DELETE from autocodecond WHERE autocodeitemnum = '"
                      + SOut.Long(autoCodeItemNum) + "'"; //AutoCodeItems.Cur.AutoCodeItemNum)
        Db.NonQ(command);
    }

    
    public static List<AutoCodeCond> GetListForItem(long autoCodeItemNum)
    {
        return GetWhere(x => x.AutoCodeItemNum == autoCodeItemNum);
    }

    
    public static bool IsSurf(AutoCondition autoCondition)
    {
        switch (autoCondition)
        {
            case AutoCondition.One_Surf:
            case AutoCondition.Two_Surf:
            case AutoCondition.Three_Surf:
            case AutoCondition.Four_Surf:
            case AutoCondition.Five_Surf:
                return true;
            default:
                return false;
        }
    }

    
    public static bool ConditionIsMet(AutoCondition autoCondition, string toothNum, string surf, bool isAdditional, bool willBeMissing, int age)
    {
        switch (autoCondition)
        {
            case AutoCondition.Anterior:
                return Tooth.IsAnterior(toothNum);
            case AutoCondition.Posterior:
                return Tooth.IsPosterior(toothNum);
            case AutoCondition.Premolar:
                return Tooth.IsPreMolar(toothNum);
            case AutoCondition.Molar:
                return Tooth.IsMolar(toothNum);
            case AutoCondition.One_Surf:
                return surf.Length == 1;
            case AutoCondition.Two_Surf:
                return surf.Length == 2;
            case AutoCondition.Three_Surf:
                return surf.Length == 3;
            case AutoCondition.Four_Surf:
                return surf.Length == 4;
            case AutoCondition.Five_Surf:
                return surf.Length == 5;
            case AutoCondition.First:
                return !isAdditional;
            case AutoCondition.EachAdditional:
                return isAdditional;
            case AutoCondition.Maxillary:
                return Tooth.IsMaxillary(toothNum);
            case AutoCondition.Mandibular:
                return !Tooth.IsMaxillary(toothNum);
            case AutoCondition.Primary:
                return Tooth.IsPrimary(toothNum);
            case AutoCondition.Permanent:
                return !Tooth.IsPrimary(toothNum);
            case AutoCondition.Pontic:
                return willBeMissing;
            case AutoCondition.Retainer:
                return !willBeMissing;
            case AutoCondition.AgeOver18:
                return age > 18;
            default:
                return false;
        }
    }

    #region Cache Pattern

    private class AutoCodeCondCache : CacheListAbs<AutoCodeCond>
    {
        protected override List<AutoCodeCond> GetCacheFromDb()
        {
            var command = "SELECT * from autocodecond ORDER BY Cond";
            return AutoCodeCondCrud.SelectMany(command);
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
            AutoCodeConds.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly AutoCodeCondCache _autoCodeCondCache = new();

    public static List<AutoCodeCond> GetDeepCopy(bool isShort = false)
    {
        return _autoCodeCondCache.GetDeepCopy(isShort);
    }

    public static List<AutoCodeCond> GetWhere(Predicate<AutoCodeCond> match, bool isShort = false)
    {
        return _autoCodeCondCache.GetWhere(match, isShort);
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
        _autoCodeCondCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _autoCodeCondCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _autoCodeCondCache.ClearCache();
    }

    #endregion Cache Pattern
}