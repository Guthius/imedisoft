#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class LoginAttemptCrud
{
    public static LoginAttempt SelectOne(long loginAttemptNum)
    {
        var command = "SELECT * FROM loginattempt "
                      + "WHERE LoginAttemptNum = " + SOut.Long(loginAttemptNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static LoginAttempt SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<LoginAttempt> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<LoginAttempt> TableToList(DataTable table)
    {
        var retVal = new List<LoginAttempt>();
        LoginAttempt loginAttempt;
        foreach (DataRow row in table.Rows)
        {
            loginAttempt = new LoginAttempt();
            loginAttempt.LoginAttemptNum = SIn.Long(row["LoginAttemptNum"].ToString());
            loginAttempt.UserName = SIn.String(row["UserName"].ToString());
            loginAttempt.LoginType = (UserWebFKeyType) SIn.Int(row["LoginType"].ToString());
            loginAttempt.DateTFail = SIn.DateTime(row["DateTFail"].ToString());
            retVal.Add(loginAttempt);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<LoginAttempt> listLoginAttempts, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "LoginAttempt";
        var table = new DataTable(tableName);
        table.Columns.Add("LoginAttemptNum");
        table.Columns.Add("UserName");
        table.Columns.Add("LoginType");
        table.Columns.Add("DateTFail");
        foreach (var loginAttempt in listLoginAttempts)
            table.Rows.Add(SOut.Long(loginAttempt.LoginAttemptNum), loginAttempt.UserName, SOut.Int((int) loginAttempt.LoginType), SOut.DateT(loginAttempt.DateTFail, false));
        return table;
    }

    public static long Insert(LoginAttempt loginAttempt)
    {
        return Insert(loginAttempt, false);
    }

    public static long Insert(LoginAttempt loginAttempt, bool useExistingPK)
    {
        var command = "INSERT INTO loginattempt (";

        command += "UserName,LoginType,DateTFail) VALUES(";

        command +=
            "'" + SOut.String(loginAttempt.UserName) + "',"
            + SOut.Int((int) loginAttempt.LoginType) + ","
            + DbHelper.Now() + ")";
        {
            loginAttempt.LoginAttemptNum = Db.NonQ(command, true, "LoginAttemptNum", "loginAttempt");
        }
        return loginAttempt.LoginAttemptNum;
    }

    public static long InsertNoCache(LoginAttempt loginAttempt)
    {
        return InsertNoCache(loginAttempt, false);
    }

    public static long InsertNoCache(LoginAttempt loginAttempt, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO loginattempt (";
        if (isRandomKeys || useExistingPK) command += "LoginAttemptNum,";
        command += "UserName,LoginType,DateTFail) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(loginAttempt.LoginAttemptNum) + ",";
        command +=
            "'" + SOut.String(loginAttempt.UserName) + "',"
            + SOut.Int((int) loginAttempt.LoginType) + ","
            + DbHelper.Now() + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            loginAttempt.LoginAttemptNum = Db.NonQ(command, true, "LoginAttemptNum", "loginAttempt");
        return loginAttempt.LoginAttemptNum;
    }

    public static void Update(LoginAttempt loginAttempt)
    {
        var command = "UPDATE loginattempt SET "
                      + "UserName       = '" + SOut.String(loginAttempt.UserName) + "', "
                      + "LoginType      =  " + SOut.Int((int) loginAttempt.LoginType) + " "
                      //DateTFail not allowed to change
                      + "WHERE LoginAttemptNum = " + SOut.Long(loginAttempt.LoginAttemptNum);
        Db.NonQ(command);
    }

    public static bool Update(LoginAttempt loginAttempt, LoginAttempt oldLoginAttempt)
    {
        var command = "";
        if (loginAttempt.UserName != oldLoginAttempt.UserName)
        {
            if (command != "") command += ",";
            command += "UserName = '" + SOut.String(loginAttempt.UserName) + "'";
        }

        if (loginAttempt.LoginType != oldLoginAttempt.LoginType)
        {
            if (command != "") command += ",";
            command += "LoginType = " + SOut.Int((int) loginAttempt.LoginType) + "";
        }

        //DateTFail not allowed to change
        if (command == "") return false;
        command = "UPDATE loginattempt SET " + command
                                             + " WHERE LoginAttemptNum = " + SOut.Long(loginAttempt.LoginAttemptNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(LoginAttempt loginAttempt, LoginAttempt oldLoginAttempt)
    {
        if (loginAttempt.UserName != oldLoginAttempt.UserName) return true;
        if (loginAttempt.LoginType != oldLoginAttempt.LoginType) return true;
        //DateTFail not allowed to change
        return false;
    }

    public static void Delete(long loginAttemptNum)
    {
        var command = "DELETE FROM loginattempt "
                      + "WHERE LoginAttemptNum = " + SOut.Long(loginAttemptNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listLoginAttemptNums)
    {
        if (listLoginAttemptNums == null || listLoginAttemptNums.Count == 0) return;
        var command = "DELETE FROM loginattempt "
                      + "WHERE LoginAttemptNum IN(" + string.Join(",", listLoginAttemptNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}