#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class UserQueryCrud
{
    public static UserQuery SelectOne(long queryNum)
    {
        var command = "SELECT * FROM userquery "
                      + "WHERE QueryNum = " + SOut.Long(queryNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static UserQuery SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<UserQuery> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<UserQuery> TableToList(DataTable table)
    {
        var retVal = new List<UserQuery>();
        UserQuery userQuery;
        foreach (DataRow row in table.Rows)
        {
            userQuery = new UserQuery();
            userQuery.QueryNum = SIn.Long(row["QueryNum"].ToString());
            userQuery.Description = SIn.String(row["Description"].ToString());
            userQuery.FileName = SIn.String(row["FileName"].ToString());
            userQuery.QueryText = SIn.String(row["QueryText"].ToString());
            userQuery.IsReleased = SIn.Bool(row["IsReleased"].ToString());
            userQuery.IsPromptSetup = SIn.Bool(row["IsPromptSetup"].ToString());
            userQuery.DefaultFormatRaw = SIn.Bool(row["DefaultFormatRaw"].ToString());
            retVal.Add(userQuery);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<UserQuery> listUserQuerys, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "UserQuery";
        var table = new DataTable(tableName);
        table.Columns.Add("QueryNum");
        table.Columns.Add("Description");
        table.Columns.Add("FileName");
        table.Columns.Add("QueryText");
        table.Columns.Add("IsReleased");
        table.Columns.Add("IsPromptSetup");
        table.Columns.Add("DefaultFormatRaw");
        foreach (var userQuery in listUserQuerys)
            table.Rows.Add(SOut.Long(userQuery.QueryNum), userQuery.Description, userQuery.FileName, userQuery.QueryText, SOut.Bool(userQuery.IsReleased), SOut.Bool(userQuery.IsPromptSetup), SOut.Bool(userQuery.DefaultFormatRaw));
        return table;
    }

    public static long Insert(UserQuery userQuery)
    {
        return Insert(userQuery, false);
    }

    public static long Insert(UserQuery userQuery, bool useExistingPK)
    {
        var command = "INSERT INTO userquery (";

        command += "Description,FileName,QueryText,IsReleased,IsPromptSetup,DefaultFormatRaw) VALUES(";

        command +=
            "'" + SOut.String(userQuery.Description) + "',"
            + "'" + SOut.String(userQuery.FileName) + "',"
            + DbHelper.ParamChar + "paramQueryText,"
            + SOut.Bool(userQuery.IsReleased) + ","
            + SOut.Bool(userQuery.IsPromptSetup) + ","
            + SOut.Bool(userQuery.DefaultFormatRaw) + ")";
        if (userQuery.QueryText == null) userQuery.QueryText = "";
        var paramQueryText = new OdSqlParameter("paramQueryText", OdDbType.Text, SOut.StringParam(userQuery.QueryText));
        {
            userQuery.QueryNum = Db.NonQ(command, true, "QueryNum", "userQuery", paramQueryText);
        }
        return userQuery.QueryNum;
    }

    public static long InsertNoCache(UserQuery userQuery)
    {
        return InsertNoCache(userQuery, false);
    }

    public static long InsertNoCache(UserQuery userQuery, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO userquery (";
        if (isRandomKeys || useExistingPK) command += "QueryNum,";
        command += "Description,FileName,QueryText,IsReleased,IsPromptSetup,DefaultFormatRaw) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(userQuery.QueryNum) + ",";
        command +=
            "'" + SOut.String(userQuery.Description) + "',"
            + "'" + SOut.String(userQuery.FileName) + "',"
            + DbHelper.ParamChar + "paramQueryText,"
            + SOut.Bool(userQuery.IsReleased) + ","
            + SOut.Bool(userQuery.IsPromptSetup) + ","
            + SOut.Bool(userQuery.DefaultFormatRaw) + ")";
        if (userQuery.QueryText == null) userQuery.QueryText = "";
        var paramQueryText = new OdSqlParameter("paramQueryText", OdDbType.Text, SOut.StringParam(userQuery.QueryText));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramQueryText);
        else
            userQuery.QueryNum = Db.NonQ(command, true, "QueryNum", "userQuery", paramQueryText);
        return userQuery.QueryNum;
    }

    public static void Update(UserQuery userQuery)
    {
        var command = "UPDATE userquery SET "
                      + "Description     = '" + SOut.String(userQuery.Description) + "', "
                      + "FileName        = '" + SOut.String(userQuery.FileName) + "', "
                      + "QueryText       =  " + DbHelper.ParamChar + "paramQueryText, "
                      + "IsReleased      =  " + SOut.Bool(userQuery.IsReleased) + ", "
                      + "IsPromptSetup   =  " + SOut.Bool(userQuery.IsPromptSetup) + ", "
                      + "DefaultFormatRaw=  " + SOut.Bool(userQuery.DefaultFormatRaw) + " "
                      + "WHERE QueryNum = " + SOut.Long(userQuery.QueryNum);
        if (userQuery.QueryText == null) userQuery.QueryText = "";
        var paramQueryText = new OdSqlParameter("paramQueryText", OdDbType.Text, SOut.StringParam(userQuery.QueryText));
        Db.NonQ(command, paramQueryText);
    }

    public static bool Update(UserQuery userQuery, UserQuery oldUserQuery)
    {
        var command = "";
        if (userQuery.Description != oldUserQuery.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(userQuery.Description) + "'";
        }

        if (userQuery.FileName != oldUserQuery.FileName)
        {
            if (command != "") command += ",";
            command += "FileName = '" + SOut.String(userQuery.FileName) + "'";
        }

        if (userQuery.QueryText != oldUserQuery.QueryText)
        {
            if (command != "") command += ",";
            command += "QueryText = " + DbHelper.ParamChar + "paramQueryText";
        }

        if (userQuery.IsReleased != oldUserQuery.IsReleased)
        {
            if (command != "") command += ",";
            command += "IsReleased = " + SOut.Bool(userQuery.IsReleased) + "";
        }

        if (userQuery.IsPromptSetup != oldUserQuery.IsPromptSetup)
        {
            if (command != "") command += ",";
            command += "IsPromptSetup = " + SOut.Bool(userQuery.IsPromptSetup) + "";
        }

        if (userQuery.DefaultFormatRaw != oldUserQuery.DefaultFormatRaw)
        {
            if (command != "") command += ",";
            command += "DefaultFormatRaw = " + SOut.Bool(userQuery.DefaultFormatRaw) + "";
        }

        if (command == "") return false;
        if (userQuery.QueryText == null) userQuery.QueryText = "";
        var paramQueryText = new OdSqlParameter("paramQueryText", OdDbType.Text, SOut.StringParam(userQuery.QueryText));
        command = "UPDATE userquery SET " + command
                                          + " WHERE QueryNum = " + SOut.Long(userQuery.QueryNum);
        Db.NonQ(command, paramQueryText);
        return true;
    }

    public static bool UpdateComparison(UserQuery userQuery, UserQuery oldUserQuery)
    {
        if (userQuery.Description != oldUserQuery.Description) return true;
        if (userQuery.FileName != oldUserQuery.FileName) return true;
        if (userQuery.QueryText != oldUserQuery.QueryText) return true;
        if (userQuery.IsReleased != oldUserQuery.IsReleased) return true;
        if (userQuery.IsPromptSetup != oldUserQuery.IsPromptSetup) return true;
        if (userQuery.DefaultFormatRaw != oldUserQuery.DefaultFormatRaw) return true;
        return false;
    }

    public static void Delete(long queryNum)
    {
        var command = "DELETE FROM userquery "
                      + "WHERE QueryNum = " + SOut.Long(queryNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listQueryNums)
    {
        if (listQueryNums == null || listQueryNums.Count == 0) return;
        var command = "DELETE FROM userquery "
                      + "WHERE QueryNum IN(" + string.Join(",", listQueryNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}