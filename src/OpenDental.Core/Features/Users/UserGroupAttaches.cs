using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class UserGroupAttaches
{
    private class UserGroupAttachCache : CacheListAbs<UserGroupAttach>
    {
        protected override List<UserGroupAttach> GetCacheFromDb()
        {
            return UserGroupAttachCrud.SelectMany("SELECT * FROM usergroupattach");
        }

        protected override List<UserGroupAttach> TableToList(DataTable dataTable)
        {
            return UserGroupAttachCrud.TableToList(dataTable);
        }

        protected override UserGroupAttach Copy(UserGroupAttach item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<UserGroupAttach> items)
        {
            return UserGroupAttachCrud.ListToTable(items, "UserGroupAttach");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly UserGroupAttachCache Cache = new();

    public static List<UserGroupAttach> GetWhere(Predicate<UserGroupAttach> predicate, bool shortList = false)
    {
        return Cache.GetWhere(predicate, shortList);
    }

    public static void GetTableFromCache(bool refreshCache)
    {
        Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }

    public static List<UserGroupAttach> GetForUser(long userNum)
    {
        return GetWhere(x => x.UserNum == userNum);
    }

    public static List<long> GetUserNumsForUserGroups(List<UserGroup> userGroups)
    {
        return GetUserNumsForUserGroups(userGroups.Select(x => x.UserGroupNum).ToList());
    }

    public static List<long> GetUserNumsForUserGroups(List<long> userGroupNums)
    {
        return GetWhere(x => userGroupNums.Contains(x.UserGroupNum))
            .Select(x => x.UserNum)
            .Distinct()
            .ToList();
    }

    public static void SyncForUser(Userod user, List<long> userGroupNums)
    {
        foreach (var userGroupNum in userGroupNums)
        {
            if (user.IsInUserGroup(userGroupNum))
            {
                continue;
            }

            UserGroupAttachCrud.Insert(new UserGroupAttach
            {
                UserGroupNum = userGroupNum,
                UserNum = user.UserNum
            });
        }

        var userGroupAttaches = GetForUser(user.UserNum);

        foreach (var userGroupAttach in userGroupAttaches)
        {
            if (!userGroupNums.Contains(userGroupAttach.UserGroupNum))
            {
                UserGroupAttachCrud.Delete(userGroupAttach.UserGroupAttachNum);
            }
        }
    }
}