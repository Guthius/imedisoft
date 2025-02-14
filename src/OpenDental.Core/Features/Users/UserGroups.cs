using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class UserGroups
{
    public static List<UserGroup> GetList()
    {
        return GetDeepCopy();
    }

    public static void Update(UserGroup userGroup)
    {
        UserGroupCrud.Update(userGroup);
    }

    public static void Insert(UserGroup userGroup)
    {
        UserGroupCrud.Insert(userGroup);
    }

    public static void Delete(UserGroup userGroup)
    {
        var command = "SELECT COUNT(*) FROM usergroupattach WHERE UserGroupNum=" + userGroup.UserGroupNum;

        var dataTable = DataCore.GetTable(command);
        if (dataTable.Rows[0][0].ToString() != "0")
        {
            throw new Exception("Must move users to another group first.");
        }

        Db.NonQ("DELETE FROM usergroup WHERE UserGroupNum=" + userGroup.UserGroupNum);
        Db.NonQ("DELETE FROM grouppermission WHERE UserGroupNum=" + userGroup.UserGroupNum);
    }

    public static UserGroup GetGroup(long userGroupNum)
    {
        return GetFirstOrDefault(x => x.UserGroupNum == userGroupNum);
    }

    public static List<UserGroup> GetList(List<long> userGroupNums)
    {
        var result = new List<UserGroup>();

        var userGroups = GetList();

        foreach (var userGroupNum in userGroupNums)
        {
            var userGroup = userGroups.FirstOrDefault(x => x.UserGroupNum == userGroupNum);
            if (userGroup != null)
            {
                result.Add(userGroups.FirstOrDefault(x => x.UserGroupNum == userGroupNum));
            }
        }

        return result;
    }

    public static List<UserGroup> GetForPermission(EnumPermType permissions)
    {
        var userGroupNums = GroupPermissions
            .GetWhere(x => x.PermType == permissions)
            .Select(x => x.UserGroupNum)
            .Distinct()
            .ToList();

        return GetWhere(x => userGroupNums.Contains(x.UserGroupNum));
    }

    public static List<UserGroup> GetForUser(long userNum)
    {
        return GetList(UserGroupAttaches.GetForUser(userNum).Select(x => x.UserGroupNum).ToList());
    }

    public static bool IsAdminGroup(List<long> userGroupNums)
    {
        var groupPermissions = GroupPermissions.GetWhere(x => x.PermType == EnumPermType.SecurityAdmin);

        return userGroupNums.Any(x => groupPermissions.Select(y => y.UserGroupNum).Contains(x));
    }

    private class UserGroupCache : CacheListAbs<UserGroup>
    {
        protected override List<UserGroup> GetCacheFromDb()
        {
            return UserGroupCrud.SelectMany("SELECT * from usergroup ORDER BY Description");
        }

        protected override List<UserGroup> TableToList(DataTable dataTable)
        {
            return UserGroupCrud.TableToList(dataTable);
        }

        protected override UserGroup Copy(UserGroup item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<UserGroup> items)
        {
            return UserGroupCrud.ListToTable(items, "UserGroup");
        }

        protected override void FillCacheIfNeeded()
        {
            GetTableFromCache(false);
        }
    }

    private static readonly UserGroupCache Cache = new();

    public static List<UserGroup> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static UserGroup GetFirstOrDefault(Func<UserGroup, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static List<UserGroup> GetWhere(Predicate<UserGroup> predicate, bool shortList = false)
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