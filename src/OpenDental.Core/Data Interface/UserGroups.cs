using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class UserGroups
{
    ///<summary>A list of all user groups, ordered by description.  Does not include CEMT user groups.</summary>
    public static List<UserGroup> GetList()
    {
        return GetList(false);
    }

    /// <summary>
    ///     A list of all user groups, ordered by description.  Set includeCEMT to true if you want CEMT user groups
    ///     included.
    /// </summary>
    public static List<UserGroup> GetList(bool includeCEMT)
    {
        return GetWhere(x => includeCEMT || x.UserGroupNumCEMT == 0);
    }

    
    public static void Update(UserGroup userGroup)
    {
        UserGroupCrud.Update(userGroup);
    }

    
    public static long Insert(UserGroup userGroup)
    {
        return UserGroupCrud.Insert(userGroup);
    }

    ///<summary>Checks for dependencies first</summary>
    public static void Delete(UserGroup userGroup)
    {
        var command = "SELECT COUNT(*) FROM usergroupattach WHERE UserGroupNum='"
                      + SOut.Long(userGroup.UserGroupNum) + "'";
        var table = DataCore.GetTable(command);
        if (table.Rows[0][0].ToString() != "0") throw new Exception(Lans.g("UserGroups", "Must move users to another group first."));

        if (PrefC.GetLong(PrefName.SecurityGroupForStudents) == userGroup.UserGroupNum) throw new Exception(Lans.g("UserGroups", "Group is the default group for students and cannot be deleted.  Change the default student group before deleting."));

        if (PrefC.GetLong(PrefName.SecurityGroupForInstructors) == userGroup.UserGroupNum) throw new Exception(Lans.g("UserGroups", "Group is the default group for instructors and cannot be deleted.  Change the default instructors group before deleting."));

        command = "DELETE FROM usergroup WHERE UserGroupNum='"
                  + SOut.Long(userGroup.UserGroupNum) + "'";
        Db.NonQ(command);
        command = "DELETE FROM grouppermission WHERE UserGroupNum='"
                  + SOut.Long(userGroup.UserGroupNum) + "'";
        Db.NonQ(command);
    }

    
    public static UserGroup GetGroup(long userGroupNum)
    {
        return GetFirstOrDefault(x => x.UserGroupNum == userGroupNum);
    }

    ///<summary>Returns a list of usergroups given a list of usergroupnums.</summary>
    public static List<UserGroup> GetList(List<long> listUserGroupNums, bool includeCEMT)
    {
        var listUserGroupsRet = new List<UserGroup>();
        List<UserGroup> listUserGroups;
        if (includeCEMT)
            listUserGroups = GetDeepCopy();
        else
            listUserGroups = GetList();

        for (var i = 0; i < listUserGroupNums.Count; i++)
        {
            var userGroup = listUserGroups.FirstOrDefault(x => x.UserGroupNum == listUserGroupNums[i]);
            if (userGroup != null)
                //should never be null.
                listUserGroupsRet.Add(listUserGroups.FirstOrDefault(x => x.UserGroupNum == listUserGroupNums[i]));
        }

        return listUserGroupsRet;
    }

    ///<summary>Returns a list of UserGroups that are associated to the permission passed in.</summary>
    public static List<UserGroup> GetForPermission(EnumPermType permissions)
    {
        var listUserGroupNums = GroupPermissions.GetWhere(x => x.PermType == permissions)
            .Select(x => x.UserGroupNum)
            .Distinct()
            .ToList();
        return GetWhere(x => listUserGroupNums.Contains(x.UserGroupNum));
    }

    /// <summary>
    ///     Returns a list of usergroups for a given user.
    ///     Returns an empty list if the user is not associated to any user groups. (should never happen)
    /// </summary>
    public static List<UserGroup> GetForUser(long userNum, bool includeCEMT)
    {
        //get the user group attaches.
        return GetList(UserGroupAttaches.GetForUser(userNum).Select(x => x.UserGroupNum).ToList(), includeCEMT);
    }

    ///<summary>Returns true if at least one of the usergroups passed in has the SecurityAdmin permission.</summary>
    public static bool IsAdminGroup(List<long> listUserGroupNums)
    {
        var listGroupPermissionsAdmin = GroupPermissions.GetWhere(x => x.PermType == EnumPermType.SecurityAdmin);
        if (listUserGroupNums.Any(x => listGroupPermissionsAdmin.Select(y => y.UserGroupNum).Contains(x))) return true;

        return false;
    }

    #region CachePattern

    private class UserGroupCache : CacheListAbs<UserGroup>
    {
        protected override List<UserGroup> GetCacheFromDb()
        {
            var command = "SELECT * from usergroup ORDER BY Description";
            return UserGroupCrud.SelectMany(command);
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
            UserGroups.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly UserGroupCache _userGroupCache = new();

    public static List<UserGroup> GetDeepCopy(bool isShort = false)
    {
        return _userGroupCache.GetDeepCopy(isShort);
    }

    public static UserGroup GetFirstOrDefault(Func<UserGroup, bool> match, bool isShort = false)
    {
        return _userGroupCache.GetFirstOrDefault(match, isShort);
    }

    public static List<UserGroup> GetWhere(Predicate<UserGroup> match, bool isShort = false)
    {
        return _userGroupCache.GetWhere(match, isShort);
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
        _userGroupCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return _userGroupCache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        _userGroupCache.ClearCache();
    }

    #endregion
}