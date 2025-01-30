using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class CodeGroups
{
    private class CodeGroupCache : CacheListAbs<CodeGroup>
    {
        protected override List<CodeGroup> GetCacheFromDb()
        {
            return CodeGroupCrud.SelectMany("SELECT * FROM codegroup ORDER BY ItemOrder");
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

    private static readonly CodeGroupCache Cache = new();

    public static void ClearCache()
    {
        Cache.ClearCache();
    }

    public static List<CodeGroup> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static CodeGroup GetFirst(Func<CodeGroup, bool> match, bool isShort = false)
    {
        return Cache.GetFirst(match, isShort);
    }

    public static CodeGroup GetFirstOrDefault(Func<CodeGroup, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static bool IsProcInCodeGroup(string procCodeString, long codeGroupNum)
    {
        var codeGroup = GetOne(codeGroupNum);

        return codeGroup != null && ProcedureCodes.IsCodeInList(procCodeString, codeGroup.ProcCodes);
    }

    public static bool IsHidden(long codeGroupNum)
    {
        if (codeGroupNum == 0) return false;

        var codeGroup = GetOne(codeGroupNum);

        return codeGroup == null || codeGroup.IsHidden;
    }

    public static bool IsShownInAgeLimit(long codeGroupNum)
    {
        if (codeGroupNum == 0) return false;

        var codeGroup = GetOne(codeGroupNum);

        return codeGroup == null || codeGroup.ShowInAgeLimit;
    }

    public static string GetGroupName(long codeGroupNum, bool isHidden = false)
    {
        var codeGroup = GetOne(codeGroupNum);

        return GetGroupName(codeGroup, isHidden);
    }

    public static string GetGroupName(CodeGroup codeGroup, bool isHidden = false)
    {
        var groupName = "";
        if (codeGroup == null)
        {
            return groupName;
        }

        groupName = codeGroup.GroupName;
        if (isHidden)
        {
            groupName += " (hidden)";
        }

        return groupName;
    }

    public static CodeGroup GetOne(long codeGroupNum)
    {
        return GetFirstOrDefault(x => x.CodeGroupNum == codeGroupNum);
    }

    public static CodeGroup GetOneForCodeGroupFixed(EnumCodeGroupFixed codeGroupFixed, bool isShort = true)
    {
        return GetFirstOrDefault(x => x.CodeGroupFixed == codeGroupFixed, isShort);
    }

    public static int GetOrder(long codeGroupNum)
    {
        return GetFirst(x => x.CodeGroupNum == codeGroupNum).ItemOrder;
    }

    public static bool Sync(List<CodeGroup> listCodeGroups, List<CodeGroup> listCodeGroupsOld)
    {
        return CodeGroupCrud.Sync(listCodeGroups, listCodeGroupsOld);
    }
}