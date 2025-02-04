using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class UserGroupCrud
{
    public static List<UserGroup> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<UserGroup> TableToList(DataTable table)
    {
        var retVal = new List<UserGroup>();
        foreach (DataRow row in table.Rows)
        {
            var userGroup = new UserGroup
            {
                UserGroupNum = SIn.Long(row["UserGroupNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                UserGroupNumCEMT = SIn.Long(row["UserGroupNumCEMT"].ToString())
            };
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

    public static void Insert(UserGroup userGroup)
    {
        var command = "INSERT INTO usergroup (";

        command += "Description,UserGroupNumCEMT) VALUES(";

        command +=
            "'" + SOut.String(userGroup.Description) + "',"
            + SOut.Long(userGroup.UserGroupNumCEMT) + ")";
        {
            userGroup.UserGroupNum = Db.NonQ(command, true, "UserGroupNum", "userGroup");
        }
    }

    public static void Update(UserGroup userGroup)
    {
        var command = "UPDATE usergroup SET "
                      + "Description     = '" + SOut.String(userGroup.Description) + "', "
                      + "UserGroupNumCEMT=  " + SOut.Long(userGroup.UserGroupNumCEMT) + " "
                      + "WHERE UserGroupNum = " + SOut.Long(userGroup.UserGroupNum);
        Db.NonQ(command);
    }
}