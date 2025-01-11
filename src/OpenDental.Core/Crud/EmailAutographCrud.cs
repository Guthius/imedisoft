#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EmailAutographCrud
{
    public static EmailAutograph SelectOne(long emailAutographNum)
    {
        var command = "SELECT * FROM emailautograph "
                      + "WHERE EmailAutographNum = " + SOut.Long(emailAutographNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EmailAutograph SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EmailAutograph> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EmailAutograph> TableToList(DataTable table)
    {
        var retVal = new List<EmailAutograph>();
        EmailAutograph emailAutograph;
        foreach (DataRow row in table.Rows)
        {
            emailAutograph = new EmailAutograph();
            emailAutograph.EmailAutographNum = SIn.Long(row["EmailAutographNum"].ToString());
            emailAutograph.Description = SIn.String(row["Description"].ToString());
            emailAutograph.EmailAddress = SIn.String(row["EmailAddress"].ToString());
            emailAutograph.AutographText = SIn.String(row["AutographText"].ToString());
            retVal.Add(emailAutograph);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EmailAutograph> listEmailAutographs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EmailAutograph";
        var table = new DataTable(tableName);
        table.Columns.Add("EmailAutographNum");
        table.Columns.Add("Description");
        table.Columns.Add("EmailAddress");
        table.Columns.Add("AutographText");
        foreach (var emailAutograph in listEmailAutographs)
            table.Rows.Add(SOut.Long(emailAutograph.EmailAutographNum), emailAutograph.Description, emailAutograph.EmailAddress, emailAutograph.AutographText);
        return table;
    }

    public static long Insert(EmailAutograph emailAutograph)
    {
        return Insert(emailAutograph, false);
    }

    public static long Insert(EmailAutograph emailAutograph, bool useExistingPK)
    {
        var command = "INSERT INTO emailautograph (";

        command += "Description,EmailAddress,AutographText) VALUES(";

        command +=
            DbHelper.ParamChar + "paramDescription,"
                               + "'" + SOut.String(emailAutograph.EmailAddress) + "',"
                               + DbHelper.ParamChar + "paramAutographText)";
        if (emailAutograph.Description == null) emailAutograph.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(emailAutograph.Description));
        if (emailAutograph.AutographText == null) emailAutograph.AutographText = "";
        var paramAutographText = new OdSqlParameter("paramAutographText", OdDbType.Text, SOut.StringParam(emailAutograph.AutographText));
        {
            emailAutograph.EmailAutographNum = Db.NonQ(command, true, "EmailAutographNum", "emailAutograph", paramDescription, paramAutographText);
        }
        return emailAutograph.EmailAutographNum;
    }

    public static long InsertNoCache(EmailAutograph emailAutograph)
    {
        return InsertNoCache(emailAutograph, false);
    }

    public static long InsertNoCache(EmailAutograph emailAutograph, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO emailautograph (";
        if (isRandomKeys || useExistingPK) command += "EmailAutographNum,";
        command += "Description,EmailAddress,AutographText) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(emailAutograph.EmailAutographNum) + ",";
        command +=
            DbHelper.ParamChar + "paramDescription,"
                               + "'" + SOut.String(emailAutograph.EmailAddress) + "',"
                               + DbHelper.ParamChar + "paramAutographText)";
        if (emailAutograph.Description == null) emailAutograph.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(emailAutograph.Description));
        if (emailAutograph.AutographText == null) emailAutograph.AutographText = "";
        var paramAutographText = new OdSqlParameter("paramAutographText", OdDbType.Text, SOut.StringParam(emailAutograph.AutographText));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDescription, paramAutographText);
        else
            emailAutograph.EmailAutographNum = Db.NonQ(command, true, "EmailAutographNum", "emailAutograph", paramDescription, paramAutographText);
        return emailAutograph.EmailAutographNum;
    }

    public static void Update(EmailAutograph emailAutograph)
    {
        var command = "UPDATE emailautograph SET "
                      + "Description      =  " + DbHelper.ParamChar + "paramDescription, "
                      + "EmailAddress     = '" + SOut.String(emailAutograph.EmailAddress) + "', "
                      + "AutographText    =  " + DbHelper.ParamChar + "paramAutographText "
                      + "WHERE EmailAutographNum = " + SOut.Long(emailAutograph.EmailAutographNum);
        if (emailAutograph.Description == null) emailAutograph.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(emailAutograph.Description));
        if (emailAutograph.AutographText == null) emailAutograph.AutographText = "";
        var paramAutographText = new OdSqlParameter("paramAutographText", OdDbType.Text, SOut.StringParam(emailAutograph.AutographText));
        Db.NonQ(command, paramDescription, paramAutographText);
    }

    public static bool Update(EmailAutograph emailAutograph, EmailAutograph oldEmailAutograph)
    {
        var command = "";
        if (emailAutograph.Description != oldEmailAutograph.Description)
        {
            if (command != "") command += ",";
            command += "Description = " + DbHelper.ParamChar + "paramDescription";
        }

        if (emailAutograph.EmailAddress != oldEmailAutograph.EmailAddress)
        {
            if (command != "") command += ",";
            command += "EmailAddress = '" + SOut.String(emailAutograph.EmailAddress) + "'";
        }

        if (emailAutograph.AutographText != oldEmailAutograph.AutographText)
        {
            if (command != "") command += ",";
            command += "AutographText = " + DbHelper.ParamChar + "paramAutographText";
        }

        if (command == "") return false;
        if (emailAutograph.Description == null) emailAutograph.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(emailAutograph.Description));
        if (emailAutograph.AutographText == null) emailAutograph.AutographText = "";
        var paramAutographText = new OdSqlParameter("paramAutographText", OdDbType.Text, SOut.StringParam(emailAutograph.AutographText));
        command = "UPDATE emailautograph SET " + command
                                               + " WHERE EmailAutographNum = " + SOut.Long(emailAutograph.EmailAutographNum);
        Db.NonQ(command, paramDescription, paramAutographText);
        return true;
    }

    public static bool UpdateComparison(EmailAutograph emailAutograph, EmailAutograph oldEmailAutograph)
    {
        if (emailAutograph.Description != oldEmailAutograph.Description) return true;
        if (emailAutograph.EmailAddress != oldEmailAutograph.EmailAddress) return true;
        if (emailAutograph.AutographText != oldEmailAutograph.AutographText) return true;
        return false;
    }

    public static void Delete(long emailAutographNum)
    {
        var command = "DELETE FROM emailautograph "
                      + "WHERE EmailAutographNum = " + SOut.Long(emailAutographNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEmailAutographNums)
    {
        if (listEmailAutographNums == null || listEmailAutographNums.Count == 0) return;
        var command = "DELETE FROM emailautograph "
                      + "WHERE EmailAutographNum IN(" + string.Join(",", listEmailAutographNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}