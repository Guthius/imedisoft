using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class UserGroupAttaches
{
    //If this table type will exist as cached data, uncomment the Cache Pattern region below and edit.

    #region Cache Pattern

    //This region can be eliminated if this is not a table type with cached data.
    //If leaving this region in place, be sure to add GetTableFromCache and FillCacheFromTable to the Cache.cs file with all the other Cache types.
    //Also, consider making an invalid type for this class in Cache.GetAllCachedInvalidTypes() if needed.

    private class UserGroupAttachCache : CacheListAbs<UserGroupAttach>
    {
        protected override List<UserGroupAttach> GetCacheFromDb()
        {
            var command = "SELECT * FROM usergroupattach";
            return UserGroupAttachCrud.SelectMany(command);
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
            UserGroupAttaches.GetTableFromCache(false);
        }
        //protected override bool IsInListShort(UserGroupAttach userGroupAttach) {
        //	return true;//Either change this method or delete it.
        //}
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly UserGroupAttachCache _userGroupAttachCache = new();

    public static List<UserGroupAttach> GetWhere(Predicate<UserGroupAttach> match, bool isShort = false)
    {
        return _userGroupAttachCache.GetWhere(match, isShort);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _userGroupAttachCache.FillCacheFromTable(table);
    }

    /// <summary>Returns the cache in the form of a DataTable. Always refreshes the ClientWeb's cache.</summary>
    /// <param name="refreshCache">If true, will refresh the cache if RemotingRole is ClientDirect or ServerWeb.</param>
    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return _userGroupAttachCache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        _userGroupAttachCache.ClearCache();
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    #endregion Cache Pattern

    #region Get Methods

    ///<summary>Returns all usergroupattaches for a single user from the cache.</summary>
    public static List<UserGroupAttach> GetForUser(long userNum)
    {
        return GetWhere(x => x.UserNum == userNum);
    }

    public static List<UserGroupAttach> GetForUserGroup(long userGroupNum)
    {
        return GetWhere(x => x.UserGroupNum == userGroupNum);
    }

    ///<summary>Pass in a list of UserGroups and return a distinct list of longs for the UserNums</summary>
    public static List<long> GetUserNumsForUserGroups(List<UserGroup> listUserGroups)
    {
        return GetUserNumsForUserGroups(listUserGroups.Select(x => x.UserGroupNum).ToList());
    }

    ///<summary>Pass in a list of UserGroupNums and return a distinct list of longs for the UserNums</summary>
    public static List<long> GetUserNumsForUserGroups(List<long> listUserGroupNums)
    {
        return GetWhere(x => listUserGroupNums.Contains(x.UserGroupNum)).Select(x => x.UserNum).Distinct().ToList();
    }

    #endregion

    #region Delete

    public static void Delete(UserGroupAttach userGroupAttach)
    {
        UserGroupAttachCrud.Delete(userGroupAttach.UserGroupAttachNum);
    }

    ///<summary>Does not add a new usergroupattach if the passed-in userCur is already attached to userGroup.</summary>
    public static void AddForUser(Userod userod, long userGroupNum)
    {
        if (!userod.IsInUserGroup(userGroupNum))
        {
            var userGroupAttach = new UserGroupAttach();
            userGroupAttach.UserGroupNum = userGroupNum;
            userGroupAttach.UserNum = userod.UserNum;
            UserGroupAttachCrud.Insert(userGroupAttach);
        }
    }

    /// <summary>
    ///     Pass in the user and all of the userGroups that the user should be attached to.
    ///     Detaches the userCur from any usergroups that are not in the given list.
    ///     Returns a count of how many user group attaches were affected.
    /// </summary>
    public static long SyncForUser(Userod userod, List<long> listUserGroupNums)
    {
        long rowsChanged = 0;
        for (var i = 0; i < listUserGroupNums.Count; i++)
            if (!userod.IsInUserGroup(listUserGroupNums[i]))
            {
                var userGroupAttach = new UserGroupAttach();
                userGroupAttach.UserGroupNum = listUserGroupNums[i];
                userGroupAttach.UserNum = userod.UserNum;
                UserGroupAttachCrud.Insert(userGroupAttach);
                rowsChanged++;
            }

        var listUserGroupAttaches = GetForUser(userod.UserNum);
        for (var i = 0; i < listUserGroupAttaches.Count; i++)
            if (!listUserGroupNums.Contains(listUserGroupAttaches[i].UserGroupNum))
            {
                UserGroupAttachCrud.Delete(listUserGroupAttaches[i].UserGroupAttachNum);
                rowsChanged++;
            }

        return rowsChanged;
    }

    #endregion
}