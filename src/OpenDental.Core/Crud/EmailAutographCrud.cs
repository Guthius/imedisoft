using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class EmailAutographCrud
{
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

    public static void Insert(EmailAutograph emailAutograph)
    {
        var command = "INSERT INTO emailautograph (";

        command += "Description,EmailAddress,AutographText) VALUES(";

        command +=
            DbHelper.ParamChar + "paramDescription,"
                               + "'" + SOut.String(emailAutograph.EmailAddress) + "',"
                               + DbHelper.ParamChar + "paramAutographText)";
        if (emailAutograph.Description == null) emailAutograph.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", SOut.StringParam(emailAutograph.Description));
        if (emailAutograph.AutographText == null) emailAutograph.AutographText = "";
        var paramAutographText = new OdSqlParameter("paramAutographText", SOut.StringParam(emailAutograph.AutographText));
        {
            emailAutograph.EmailAutographNum = Db.NonQ(command, true, "EmailAutographNum", "emailAutograph", paramDescription, paramAutographText);
        }
    }

    public static void Update(EmailAutograph emailAutograph)
    {
        var command = "UPDATE emailautograph SET "
                      + "Description      =  " + DbHelper.ParamChar + "paramDescription, "
                      + "EmailAddress     = '" + SOut.String(emailAutograph.EmailAddress) + "', "
                      + "AutographText    =  " + DbHelper.ParamChar + "paramAutographText "
                      + "WHERE EmailAutographNum = " + SOut.Long(emailAutograph.EmailAutographNum);
        if (emailAutograph.Description == null) emailAutograph.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", SOut.StringParam(emailAutograph.Description));
        if (emailAutograph.AutographText == null) emailAutograph.AutographText = "";
        var paramAutographText = new OdSqlParameter("paramAutographText", SOut.StringParam(emailAutograph.AutographText));
        Db.NonQ(command, paramDescription, paramAutographText);
    }

    public static void Delete(long emailAutographNum)
    {
        var command = "DELETE FROM emailautograph "
                      + "WHERE EmailAutographNum = " + SOut.Long(emailAutographNum);
        Db.NonQ(command);
    }
}