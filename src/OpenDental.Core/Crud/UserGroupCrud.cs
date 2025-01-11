#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class UserGroupCrud
{
    public static UserGroup SelectOne(long userGroupNum)
    {
        var command = "SELECT * FROM usergroup "
                      + "WHERE UserGroupNum = " + SOut.Long(userGroupNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static UserGroup SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<UserGroup> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<UserGroup> TableToList(DataTable table)
    {
        var retVal = new List<UserGroup>();
        UserGroup userGroup;
        foreach (DataRow row in table.Rows)
        {
            userGroup = new UserGroup();
            userGroup.UserGroupNum = SIn.Long(row["UserGroupNum"].ToString());
            userGroup.Description = SIn.String(row["Description"].ToString());
            userGroup.UserGroupNumCEMT = SIn.Long(row["UserGroupNumCEMT"].ToString());
            retVal.Add(userGroup);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<UserGroup> listUserGroups, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "UserGroup";
        var table = new DataTable(tableName);
        table.Columns.Add("UserGroupNum");
        table.Columns.Add("Description");
        table.Columns.Add("UserGroupNumCEMT");
        foreach (var userGroup in listUserGroups)
            table.Rows.Add(SOut.Long(userGroup.UserGroupNum), userGroup.Description, SOut.Long(userGroup.UserGroupNumCEMT));
        return table;
    }

    public static long Insert(UserGroup userGroup)
    {
        return Insert(userGroup, false);
    }

    public static long Insert(UserGroup userGroup, bool useExistingPK)
    {
        var command = "INSERT INTO usergroup (";

        command += "Description,UserGroupNumCEMT) VALUES(";

        command +=
            "'" + SOut.String(userGroup.Description) + "',"
            + SOut.Long(userGroup.UserGroupNumCEMT) + ")";
        {
            userGroup.UserGroupNum = Db.NonQ(command, true, "UserGroupNum", "userGroup");
        }
        return userGroup.UserGroupNum;
    }

    public static long InsertNoCache(UserGroup userGroup)
    {
        return InsertNoCache(userGroup, false);
    }

    public static long InsertNoCache(UserGroup userGroup, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO usergroup (";
        if (isRandomKeys || useExistingPK) command += "UserGroupNum,";
        command += "Description,UserGroupNumCEMT) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(userGroup.UserGroupNum) + ",";
        command +=
            "'" + SOut.String(userGroup.Description) + "',"
            + SOut.Long(userGroup.UserGroupNumCEMT) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            userGroup.UserGroupNum = Db.NonQ(command, true, "UserGroupNum", "userGroup");
        return userGroup.UserGroupNum;
    }

    public static void Update(UserGroup userGroup)
    {
        var command = "UPDATE usergroup SET "
                      + "Description     = '" + SOut.String(userGroup.Description) + "', "
                      + "UserGroupNumCEMT=  " + SOut.Long(userGroup.UserGroupNumCEMT) + " "
                      + "WHERE UserGroupNum = " + SOut.Long(userGroup.UserGroupNum);
        Db.NonQ(command);
    }

    public static bool Update(UserGroup userGroup, UserGroup oldUserGroup)
    {
        var command = "";
        if (userGroup.Description != oldUserGroup.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(userGroup.Description) + "'";
        }

        if (userGroup.UserGroupNumCEMT != oldUserGroup.UserGroupNumCEMT)
        {
            if (command != "") command += ",";
            command += "UserGroupNumCEMT = " + SOut.Long(userGroup.UserGroupNumCEMT) + "";
        }

        if (command == "") return false;
        command = "UPDATE usergroup SET " + command
                                          + " WHERE UserGroupNum = " + SOut.Long(userGroup.UserGroupNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(UserGroup userGroup, UserGroup oldUserGroup)
    {
        if (userGroup.Description != oldUserGroup.Description) return true;
        if (userGroup.UserGroupNumCEMT != oldUserGroup.UserGroupNumCEMT) return true;
        return false;
    }

    public static void Delete(long userGroupNum)
    {
        var command = "DELETE FROM usergroup "
                      + "WHERE UserGroupNum = " + SOut.Long(userGroupNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listUserGroupNums)
    {
        if (listUserGroupNums == null || listUserGroupNums.Count == 0) return;
        var command = "DELETE FROM usergroup "
                      + "WHERE UserGroupNum IN(" + string.Join(",", listUserGroupNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}