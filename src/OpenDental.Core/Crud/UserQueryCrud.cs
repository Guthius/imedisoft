using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class UserQueryCrud
{
    public static List<UserQuery> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<UserQuery> TableToList(DataTable table)
    {
        var retVal = new List<UserQuery>();
        foreach (DataRow row in table.Rows)
        {
            var userQuery = new UserQuery
            {
                QueryNum = SIn.Long(row["QueryNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                FileName = SIn.String(row["FileName"].ToString()),
                QueryText = SIn.String(row["QueryText"].ToString()),
                IsReleased = SIn.Bool(row["IsReleased"].ToString()),
                IsPromptSetup = SIn.Bool(row["IsPromptSetup"].ToString()),
                DefaultFormatRaw = SIn.Bool(row["DefaultFormatRaw"].ToString())
            };
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

    public static void Insert(UserQuery userQuery)
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
        var paramQueryText = new OdSqlParameter("paramQueryText", SOut.StringParam(userQuery.QueryText));
        {
            userQuery.QueryNum = Db.NonQ(command, true, "QueryNum", "userQuery", paramQueryText);
        }
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
        var paramQueryText = new OdSqlParameter("paramQueryText", SOut.StringParam(userQuery.QueryText));
        Db.NonQ(command, paramQueryText);
    }
}