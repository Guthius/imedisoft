#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class UserGroupAttachCrud
{
    public static UserGroupAttach SelectOne(long userGroupAttachNum)
    {
        var command = "SELECT * FROM usergroupattach "
                      + "WHERE UserGroupAttachNum = " + SOut.Long(userGroupAttachNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static UserGroupAttach SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<UserGroupAttach> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<UserGroupAttach> TableToList(DataTable table)
    {
        var retVal = new List<UserGroupAttach>();
        UserGroupAttach userGroupAttach;
        foreach (DataRow row in table.Rows)
        {
            userGroupAttach = new UserGroupAttach();
            userGroupAttach.UserGroupAttachNum = SIn.Long(row["UserGroupAttachNum"].ToString());
            userGroupAttach.UserNum = SIn.Long(row["UserNum"].ToString());
            userGroupAttach.UserGroupNum = SIn.Long(row["UserGroupNum"].ToString());
            retVal.Add(userGroupAttach);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<UserGroupAttach> listUserGroupAttachs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "UserGroupAttach";
        var table = new DataTable(tableName);
        table.Columns.Add("UserGroupAttachNum");
        table.Columns.Add("UserNum");
        table.Columns.Add("UserGroupNum");
        foreach (var userGroupAttach in listUserGroupAttachs)
            table.Rows.Add(SOut.Long(userGroupAttach.UserGroupAttachNum), SOut.Long(userGroupAttach.UserNum), SOut.Long(userGroupAttach.UserGroupNum));
        return table;
    }

    public static long Insert(UserGroupAttach userGroupAttach)
    {
        return Insert(userGroupAttach, false);
    }

    public static long Insert(UserGroupAttach userGroupAttach, bool useExistingPK)
    {
        var command = "INSERT INTO usergroupattach (";

        command += "UserNum,UserGroupNum) VALUES(";

        command +=
            SOut.Long(userGroupAttach.UserNum) + ","
                                               + SOut.Long(userGroupAttach.UserGroupNum) + ")";
        {
            userGroupAttach.UserGroupAttachNum = Db.NonQ(command, true, "UserGroupAttachNum", "userGroupAttach");
        }
        return userGroupAttach.UserGroupAttachNum;
    }

    public static long InsertNoCache(UserGroupAttach userGroupAttach)
    {
        return InsertNoCache(userGroupAttach, false);
    }

    public static long InsertNoCache(UserGroupAttach userGroupAttach, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO usergroupattach (";
        if (isRandomKeys || useExistingPK) command += "UserGroupAttachNum,";
        command += "UserNum,UserGroupNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(userGroupAttach.UserGroupAttachNum) + ",";
        command +=
            SOut.Long(userGroupAttach.UserNum) + ","
                                               + SOut.Long(userGroupAttach.UserGroupNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            userGroupAttach.UserGroupAttachNum = Db.NonQ(command, true, "UserGroupAttachNum", "userGroupAttach");
        return userGroupAttach.UserGroupAttachNum;
    }

    public static void Update(UserGroupAttach userGroupAttach)
    {
        var command = "UPDATE usergroupattach SET "
                      + "UserNum           =  " + SOut.Long(userGroupAttach.UserNum) + ", "
                      + "UserGroupNum      =  " + SOut.Long(userGroupAttach.UserGroupNum) + " "
                      + "WHERE UserGroupAttachNum = " + SOut.Long(userGroupAttach.UserGroupAttachNum);
        Db.NonQ(command);
    }

    public static bool Update(UserGroupAttach userGroupAttach, UserGroupAttach oldUserGroupAttach)
    {
        var command = "";
        if (userGroupAttach.UserNum != oldUserGroupAttach.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(userGroupAttach.UserNum) + "";
        }

        if (userGroupAttach.UserGroupNum != oldUserGroupAttach.UserGroupNum)
        {
            if (command != "") command += ",";
            command += "UserGroupNum = " + SOut.Long(userGroupAttach.UserGroupNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE usergroupattach SET " + command
                                                + " WHERE UserGroupAttachNum = " + SOut.Long(userGroupAttach.UserGroupAttachNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(UserGroupAttach userGroupAttach, UserGroupAttach oldUserGroupAttach)
    {
        if (userGroupAttach.UserNum != oldUserGroupAttach.UserNum) return true;
        if (userGroupAttach.UserGroupNum != oldUserGroupAttach.UserGroupNum) return true;
        return false;
    }

    public static void Delete(long userGroupAttachNum)
    {
        var command = "DELETE FROM usergroupattach "
                      + "WHERE UserGroupAttachNum = " + SOut.Long(userGroupAttachNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listUserGroupAttachNums)
    {
        if (listUserGroupAttachNums == null || listUserGroupAttachNums.Count == 0) return;
        var command = "DELETE FROM usergroupattach "
                      + "WHERE UserGroupAttachNum IN(" + string.Join(",", listUserGroupAttachNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}