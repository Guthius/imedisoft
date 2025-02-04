using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class UserGroupAttachCrud
{
    public static List<UserGroupAttach> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<UserGroupAttach> TableToList(DataTable table)
    {
        var retVal = new List<UserGroupAttach>();
        foreach (DataRow row in table.Rows)
        {
            var userGroupAttach = new UserGroupAttach
            {
                UserGroupAttachNum = SIn.Long(row["UserGroupAttachNum"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                UserGroupNum = SIn.Long(row["UserGroupNum"].ToString())
            };
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

    public static void Insert(UserGroupAttach userGroupAttach)
    {
        var command = "INSERT INTO usergroupattach (";

        command += "UserNum,UserGroupNum) VALUES(";

        command +=
            SOut.Long(userGroupAttach.UserNum) + ","
                                               + SOut.Long(userGroupAttach.UserGroupNum) + ")";
        {
            userGroupAttach.UserGroupAttachNum = Db.NonQ(command, true, "UserGroupAttachNum", "userGroupAttach");
        }
    }

    public static void Delete(long userGroupAttachNum)
    {
        var command = "DELETE FROM usergroupattach "
                      + "WHERE UserGroupAttachNum = " + SOut.Long(userGroupAttachNum);
        Db.NonQ(command);
    }
}