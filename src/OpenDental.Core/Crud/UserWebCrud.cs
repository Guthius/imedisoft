#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class UserWebCrud
{
    public static UserWeb SelectOne(long userWebNum)
    {
        var command = "SELECT * FROM userweb "
                      + "WHERE UserWebNum = " + SOut.Long(userWebNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static UserWeb SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<UserWeb> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<UserWeb> TableToList(DataTable table)
    {
        var retVal = new List<UserWeb>();
        UserWeb userWeb;
        foreach (DataRow row in table.Rows)
        {
            userWeb = new UserWeb();
            userWeb.UserWebNum = SIn.Long(row["UserWebNum"].ToString());
            userWeb.FKey = SIn.Long(row["FKey"].ToString());
            userWeb.FKeyType = (UserWebFKeyType) SIn.Int(row["FKeyType"].ToString());
            userWeb.UserName = SIn.String(row["UserName"].ToString());
            userWeb.Password = SIn.String(row["Password"].ToString());
            userWeb.PasswordResetCode = SIn.String(row["PasswordResetCode"].ToString());
            userWeb.RequireUserNameChange = SIn.Bool(row["RequireUserNameChange"].ToString());
            userWeb.DateTimeLastLogin = SIn.DateTime(row["DateTimeLastLogin"].ToString());
            userWeb.RequirePasswordChange = SIn.Bool(row["RequirePasswordChange"].ToString());
            retVal.Add(userWeb);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<UserWeb> listUserWebs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "UserWeb";
        var table = new DataTable(tableName);
        table.Columns.Add("UserWebNum");
        table.Columns.Add("FKey");
        table.Columns.Add("FKeyType");
        table.Columns.Add("UserName");
        table.Columns.Add("Password");
        table.Columns.Add("PasswordResetCode");
        table.Columns.Add("RequireUserNameChange");
        table.Columns.Add("DateTimeLastLogin");
        table.Columns.Add("RequirePasswordChange");
        foreach (var userWeb in listUserWebs)
            table.Rows.Add(SOut.Long(userWeb.UserWebNum), SOut.Long(userWeb.FKey), SOut.Int((int) userWeb.FKeyType), userWeb.UserName, userWeb.Password, userWeb.PasswordResetCode, SOut.Bool(userWeb.RequireUserNameChange), SOut.DateT(userWeb.DateTimeLastLogin, false), SOut.Bool(userWeb.RequirePasswordChange));
        return table;
    }

    public static long Insert(UserWeb userWeb)
    {
        return Insert(userWeb, false);
    }

    public static long Insert(UserWeb userWeb, bool useExistingPK)
    {
        var command = "INSERT INTO userweb (";

        command += "FKey,FKeyType,UserName,Password,PasswordResetCode,RequireUserNameChange,DateTimeLastLogin,RequirePasswordChange) VALUES(";

        command +=
            SOut.Long(userWeb.FKey) + ","
                                    + SOut.Int((int) userWeb.FKeyType) + ","
                                    + "'" + SOut.String(userWeb.UserName) + "',"
                                    + "'" + SOut.String(userWeb.Password) + "',"
                                    + "'" + SOut.String(userWeb.PasswordResetCode) + "',"
                                    + SOut.Bool(userWeb.RequireUserNameChange) + ","
                                    + SOut.DateT(userWeb.DateTimeLastLogin) + ","
                                    + SOut.Bool(userWeb.RequirePasswordChange) + ")";
        {
            userWeb.UserWebNum = Db.NonQ(command, true, "UserWebNum", "userWeb");
        }
        return userWeb.UserWebNum;
    }

    public static void InsertMany(List<UserWeb> listUserWebs)
    {
        InsertMany(listUserWebs, false);
    }

    public static void InsertMany(List<UserWeb> listUserWebs, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listUserWebs.Count)
        {
            var userWeb = listUserWebs[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO userweb (");
                if (useExistingPK) sbCommands.Append("UserWebNum,");
                sbCommands.Append("FKey,FKeyType,UserName,Password,PasswordResetCode,RequireUserNameChange,DateTimeLastLogin,RequirePasswordChange) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(userWeb.UserWebNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(userWeb.FKey));
            sbRow.Append(",");
            sbRow.Append(SOut.Int((int) userWeb.FKeyType));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(userWeb.UserName) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(userWeb.Password) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(userWeb.PasswordResetCode) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(userWeb.RequireUserNameChange));
            sbRow.Append(",");
            sbRow.Append(SOut.DateT(userWeb.DateTimeLastLogin));
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(userWeb.RequirePasswordChange));
            sbRow.Append(")");
            if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
            {
                Db.NonQ(sbCommands.ToString());
                sbCommands = null;
            }
            else
            {
                if (hasComma) sbCommands.Append(",");
                sbCommands.Append(sbRow);
                countRows++;
                if (index == listUserWebs.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(UserWeb userWeb)
    {
        return InsertNoCache(userWeb, false);
    }

    public static long InsertNoCache(UserWeb userWeb, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO userweb (";
        if (isRandomKeys || useExistingPK) command += "UserWebNum,";
        command += "FKey,FKeyType,UserName,Password,PasswordResetCode,RequireUserNameChange,DateTimeLastLogin,RequirePasswordChange) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(userWeb.UserWebNum) + ",";
        command +=
            SOut.Long(userWeb.FKey) + ","
                                    + SOut.Int((int) userWeb.FKeyType) + ","
                                    + "'" + SOut.String(userWeb.UserName) + "',"
                                    + "'" + SOut.String(userWeb.Password) + "',"
                                    + "'" + SOut.String(userWeb.PasswordResetCode) + "',"
                                    + SOut.Bool(userWeb.RequireUserNameChange) + ","
                                    + SOut.DateT(userWeb.DateTimeLastLogin) + ","
                                    + SOut.Bool(userWeb.RequirePasswordChange) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            userWeb.UserWebNum = Db.NonQ(command, true, "UserWebNum", "userWeb");
        return userWeb.UserWebNum;
    }

    public static void Update(UserWeb userWeb)
    {
        var command = "UPDATE userweb SET "
                      + "FKey                 =  " + SOut.Long(userWeb.FKey) + ", "
                      + "FKeyType             =  " + SOut.Int((int) userWeb.FKeyType) + ", "
                      + "UserName             = '" + SOut.String(userWeb.UserName) + "', "
                      + "Password             = '" + SOut.String(userWeb.Password) + "', "
                      + "PasswordResetCode    = '" + SOut.String(userWeb.PasswordResetCode) + "', "
                      + "RequireUserNameChange=  " + SOut.Bool(userWeb.RequireUserNameChange) + ", "
                      + "DateTimeLastLogin    =  " + SOut.DateT(userWeb.DateTimeLastLogin) + ", "
                      + "RequirePasswordChange=  " + SOut.Bool(userWeb.RequirePasswordChange) + " "
                      + "WHERE UserWebNum = " + SOut.Long(userWeb.UserWebNum);
        Db.NonQ(command);
    }

    public static bool Update(UserWeb userWeb, UserWeb oldUserWeb)
    {
        var command = "";
        if (userWeb.FKey != oldUserWeb.FKey)
        {
            if (command != "") command += ",";
            command += "FKey = " + SOut.Long(userWeb.FKey) + "";
        }

        if (userWeb.FKeyType != oldUserWeb.FKeyType)
        {
            if (command != "") command += ",";
            command += "FKeyType = " + SOut.Int((int) userWeb.FKeyType) + "";
        }

        if (userWeb.UserName != oldUserWeb.UserName)
        {
            if (command != "") command += ",";
            command += "UserName = '" + SOut.String(userWeb.UserName) + "'";
        }

        if (userWeb.Password != oldUserWeb.Password)
        {
            if (command != "") command += ",";
            command += "Password = '" + SOut.String(userWeb.Password) + "'";
        }

        if (userWeb.PasswordResetCode != oldUserWeb.PasswordResetCode)
        {
            if (command != "") command += ",";
            command += "PasswordResetCode = '" + SOut.String(userWeb.PasswordResetCode) + "'";
        }

        if (userWeb.RequireUserNameChange != oldUserWeb.RequireUserNameChange)
        {
            if (command != "") command += ",";
            command += "RequireUserNameChange = " + SOut.Bool(userWeb.RequireUserNameChange) + "";
        }

        if (userWeb.DateTimeLastLogin != oldUserWeb.DateTimeLastLogin)
        {
            if (command != "") command += ",";
            command += "DateTimeLastLogin = " + SOut.DateT(userWeb.DateTimeLastLogin) + "";
        }

        if (userWeb.RequirePasswordChange != oldUserWeb.RequirePasswordChange)
        {
            if (command != "") command += ",";
            command += "RequirePasswordChange = " + SOut.Bool(userWeb.RequirePasswordChange) + "";
        }

        if (command == "") return false;
        command = "UPDATE userweb SET " + command
                                        + " WHERE UserWebNum = " + SOut.Long(userWeb.UserWebNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(UserWeb userWeb, UserWeb oldUserWeb)
    {
        if (userWeb.FKey != oldUserWeb.FKey) return true;
        if (userWeb.FKeyType != oldUserWeb.FKeyType) return true;
        if (userWeb.UserName != oldUserWeb.UserName) return true;
        if (userWeb.Password != oldUserWeb.Password) return true;
        if (userWeb.PasswordResetCode != oldUserWeb.PasswordResetCode) return true;
        if (userWeb.RequireUserNameChange != oldUserWeb.RequireUserNameChange) return true;
        if (userWeb.DateTimeLastLogin != oldUserWeb.DateTimeLastLogin) return true;
        if (userWeb.RequirePasswordChange != oldUserWeb.RequirePasswordChange) return true;
        return false;
    }

    public static void Delete(long userWebNum)
    {
        var command = "DELETE FROM userweb "
                      + "WHERE UserWebNum = " + SOut.Long(userWebNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listUserWebNums)
    {
        if (listUserWebNums == null || listUserWebNums.Count == 0) return;
        var command = "DELETE FROM userweb "
                      + "WHERE UserWebNum IN(" + string.Join(",", listUserWebNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}