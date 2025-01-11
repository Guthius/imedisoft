using System;
using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class CodeGroups
{
    #region Cache Pattern

    private class CodeGroupCache : CacheListAbs<CodeGroup>
    {
        protected override List<CodeGroup> GetCacheFromDb()
        {
            var command = "SELECT * FROM codegroup ORDER BY ItemOrder";
            return CodeGroupCrud.SelectMany(command);
        }

        protected override List<CodeGroup> TableToList(DataTable dataTable)
        {
            return CodeGroupCrud.TableToList(dataTable);
        }

        protected override CodeGroup Copy(CodeGroup item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<CodeGroup> items)
        {
            return CodeGroupCrud.ListToTable(items, "CodeGroup");
        }

        protected override void FillCacheIfNeeded()
        {
            CodeGroups.GetTableFromCache(false);
        }

        protected override bool IsInListShort(CodeGroup item)
        {
            return item.IsVisible();
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly CodeGroupCache _codeGroupCache = new();

    public static void ClearCache()
    {
        _codeGroupCache.ClearCache();
    }

    public static List<CodeGroup> GetDeepCopy(bool isShort = false)
    {
        return _codeGroupCache.GetDeepCopy(isShort);
    }

    public static int GetCount(bool isShort = false)
    {
        return _codeGroupCache.GetCount(isShort);
    }

    public static bool GetExists(Predicate<CodeGroup> match, bool isShort = false)
    {
        return _codeGroupCache.GetExists(match, isShort);
    }

    public static int GetFindIndex(Predicate<CodeGroup> match, bool isShort = false)
    {
        return _codeGroupCache.GetFindIndex(match, isShort);
    }

    public static CodeGroup GetFirst(bool isShort = false)
    {
        return _codeGroupCache.GetFirst(isShort);
    }

    public static CodeGroup GetFirst(Func<CodeGroup, bool> match, bool isShort = false)
    {
        return _codeGroupCache.GetFirst(match, isShort);
    }

    public static CodeGroup GetFirstOrDefault(Func<CodeGroup, bool> match, bool isShort = false)
    {
        return _codeGroupCache.GetFirstOrDefault(match, isShort);
    }

    public static CodeGroup GetLast(bool isShort = false)
    {
        return _codeGroupCache.GetLast(isShort);
    }

    public static CodeGroup GetLastOrDefault(Func<CodeGroup, bool> match, bool isShort = false)
    {
        return _codeGroupCache.GetLastOrDefault(match, isShort);
    }

    public static List<CodeGroup> GetWhere(Predicate<CodeGroup> match, bool isShort = false)
    {
        return _codeGroupCache.GetWhere(match, isShort);
    }

    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _codeGroupCache.FillCacheFromTable(table);
    }

    /// <summary>Returns the cache in the form of a DataTable. Always refreshes the ClientMT's cache.</summary>
    /// <param name="doRefreshCache">If true, will refresh the cache if MiddleTierRole is ClientDirect or ServerWeb.</param>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _codeGroupCache.GetTableFromCache(doRefreshCache);
    }

    #endregion Cache Pattern

    #region Methods - Get

    ///<summary>Determines whether or not a given Procedure Code is contained within a codegroup.</summary>
    public static bool IsProcInCodeGroup(string procCodeString, long codeGroupNum)
    {
        var codeGroup = GetOne(codeGroupNum);
        //May be null when called from ComputeForOrdinal.
        if (codeGroup == null) return false;

        return ProcedureCodes.IsCodeInList(procCodeString, codeGroup.ProcCodes);
    }

    /// <summary>
    ///     If true, this codegroup is hidden from the frequency limitations grid. Control of showing in age limitations
    ///     grid is done separately using ShowInAgeLimit. Returns true if the codeGroupNum passed in is invalid. Returns false
    ///     if 0 is passed in.
    /// </summary>
    public static bool IsHidden(long codeGroupNum)
    {
        if (codeGroupNum == 0) return false;

        var codeGroup = GetOne(codeGroupNum);
        if (codeGroup == null) return true; //Invalid CodeGroupNum passed in or database corruption.

        return codeGroup.IsHidden;
    }

    /// <summary>
    ///     If true, this codegroup shows in Age Limitations grid. Control of showing in Freq Lim is done separately using
    ///     IsHidden. Returns true if the codeGroupNum passed in is invalid. Returns false if 0 is passed in.
    /// </summary>
    public static bool IsShownInAgeLimit(long codeGroupNum)
    {
        if (codeGroupNum == 0) return false;

        var codeGroup = GetOne(codeGroupNum);
        if (codeGroup == null) return true; //Invalid CodeGroupNum passed in or database corruption.

        return codeGroup.ShowInAgeLimit;
    }

    public static List<long> GetCodeNums(long codeGroupNum)
    {
        var listCodeNums = new List<long>();
        var codeGroup = GetOne(codeGroupNum);
        if (codeGroup != null) listCodeNums = ProcedureCodes.GetCodeNumsForProcCodes(codeGroup.ProcCodes);

        return listCodeNums;
    }

    ///<summary>Returns the GroupName, including '(hidden)' if isHidden is true, for the CodeGroup passed in.</summary>
    public static string GetGroupName(long codeGroupNum, bool isHidden = false)
    {
        var codeGroup = GetOne(codeGroupNum);
        return GetGroupName(codeGroup, isHidden);
    }

    ///<summary>Returns the GroupName, including '(hidden)' if isHidden is true, for the CodeGroup passed in.</summary>
    public static string GetGroupName(CodeGroup codeGroup, bool isHidden = false)
    {
        var groupName = "";
        if (codeGroup == null) return groupName;

        groupName = codeGroup.GroupName;
        if (isHidden) groupName += " " + Lans.g("CodeGroups", "(hidden)");

        return groupName;
    }

    ///<summary>Grab one code group from the full list of codegroups in the cache.</summary>
    public static CodeGroup GetOne(long codeGroupNum)
    {
        return GetFirstOrDefault(x => x.CodeGroupNum == codeGroupNum);
    }

    public static CodeGroup GetOneForCodeGroupFixed(EnumCodeGroupFixed codeGroupFixed, bool isShort = true)
    {
        return GetFirstOrDefault(x => x.CodeGroupFixed == codeGroupFixed, isShort);
    }

    ///<summary>Should only be called when comparing the item order of non-zero codegroup nums.</summary>
    public static int GetOrder(long codeGroupNum)
    {
        return GetFirst(x => x.CodeGroupNum == codeGroupNum).ItemOrder;
    }

    #endregion Methods - Get

    #region Methods - Modify

    
    public static long Insert(CodeGroup codeGroup)
    {
        return CodeGroupCrud.Insert(codeGroup);
    }

    ///<summary>Inserts, updates, or deletes database rows to match supplied list.  Returns true if db changes were made.</summary>
    public static bool Sync(List<CodeGroup> listCodeGroups, List<CodeGroup> listCodeGroupsOld)
    {
        return CodeGroupCrud.Sync(listCodeGroups, listCodeGroupsOld);
    }

    
    public static void Update(CodeGroup codeGroup)
    {
        CodeGroupCrud.Update(codeGroup);
    }

    #endregion Methods - Modify
}