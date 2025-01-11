#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EmailTemplateCrud
{
    public static EmailTemplate SelectOne(long emailTemplateNum)
    {
        var command = "SELECT * FROM emailtemplate "
                      + "WHERE EmailTemplateNum = " + SOut.Long(emailTemplateNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EmailTemplate SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EmailTemplate> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EmailTemplate> TableToList(DataTable table)
    {
        var retVal = new List<EmailTemplate>();
        EmailTemplate emailTemplate;
        foreach (DataRow row in table.Rows)
        {
            emailTemplate = new EmailTemplate();
            emailTemplate.EmailTemplateNum = SIn.Long(row["EmailTemplateNum"].ToString());
            emailTemplate.Subject = SIn.String(row["Subject"].ToString());
            emailTemplate.BodyText = SIn.String(row["BodyText"].ToString());
            emailTemplate.Description = SIn.String(row["Description"].ToString());
            emailTemplate.TemplateType = (EmailType) SIn.Int(row["TemplateType"].ToString());
            retVal.Add(emailTemplate);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EmailTemplate> listEmailTemplates, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EmailTemplate";
        var table = new DataTable(tableName);
        table.Columns.Add("EmailTemplateNum");
        table.Columns.Add("Subject");
        table.Columns.Add("BodyText");
        table.Columns.Add("Description");
        table.Columns.Add("TemplateType");
        foreach (var emailTemplate in listEmailTemplates)
            table.Rows.Add(SOut.Long(emailTemplate.EmailTemplateNum), emailTemplate.Subject, emailTemplate.BodyText, emailTemplate.Description, SOut.Int((int) emailTemplate.TemplateType));
        return table;
    }

    public static long Insert(EmailTemplate emailTemplate)
    {
        return Insert(emailTemplate, false);
    }

    public static long Insert(EmailTemplate emailTemplate, bool useExistingPK)
    {
        var command = "INSERT INTO emailtemplate (";

        command += "Subject,BodyText,Description,TemplateType) VALUES(";

        command +=
            DbHelper.ParamChar + "paramSubject,"
                               + DbHelper.ParamChar + "paramBodyText,"
                               + DbHelper.ParamChar + "paramDescription,"
                               + SOut.Int((int) emailTemplate.TemplateType) + ")";
        if (emailTemplate.Subject == null) emailTemplate.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", OdDbType.Text, SOut.StringParam(emailTemplate.Subject));
        if (emailTemplate.BodyText == null) emailTemplate.BodyText = "";
        var paramBodyText = new OdSqlParameter("paramBodyText", OdDbType.Text, SOut.StringParam(emailTemplate.BodyText));
        if (emailTemplate.Description == null) emailTemplate.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(emailTemplate.Description));
        {
            emailTemplate.EmailTemplateNum = Db.NonQ(command, true, "EmailTemplateNum", "emailTemplate", paramSubject, paramBodyText, paramDescription);
        }
        return emailTemplate.EmailTemplateNum;
    }

    public static long InsertNoCache(EmailTemplate emailTemplate)
    {
        return InsertNoCache(emailTemplate, false);
    }

    public static long InsertNoCache(EmailTemplate emailTemplate, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO emailtemplate (";
        if (isRandomKeys || useExistingPK) command += "EmailTemplateNum,";
        command += "Subject,BodyText,Description,TemplateType) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(emailTemplate.EmailTemplateNum) + ",";
        command +=
            DbHelper.ParamChar + "paramSubject,"
                               + DbHelper.ParamChar + "paramBodyText,"
                               + DbHelper.ParamChar + "paramDescription,"
                               + SOut.Int((int) emailTemplate.TemplateType) + ")";
        if (emailTemplate.Subject == null) emailTemplate.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", OdDbType.Text, SOut.StringParam(emailTemplate.Subject));
        if (emailTemplate.BodyText == null) emailTemplate.BodyText = "";
        var paramBodyText = new OdSqlParameter("paramBodyText", OdDbType.Text, SOut.StringParam(emailTemplate.BodyText));
        if (emailTemplate.Description == null) emailTemplate.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(emailTemplate.Description));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramSubject, paramBodyText, paramDescription);
        else
            emailTemplate.EmailTemplateNum = Db.NonQ(command, true, "EmailTemplateNum", "emailTemplate", paramSubject, paramBodyText, paramDescription);
        return emailTemplate.EmailTemplateNum;
    }

    public static void Update(EmailTemplate emailTemplate)
    {
        var command = "UPDATE emailtemplate SET "
                      + "Subject         =  " + DbHelper.ParamChar + "paramSubject, "
                      + "BodyText        =  " + DbHelper.ParamChar + "paramBodyText, "
                      + "Description     =  " + DbHelper.ParamChar + "paramDescription, "
                      + "TemplateType    =  " + SOut.Int((int) emailTemplate.TemplateType) + " "
                      + "WHERE EmailTemplateNum = " + SOut.Long(emailTemplate.EmailTemplateNum);
        if (emailTemplate.Subject == null) emailTemplate.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", OdDbType.Text, SOut.StringParam(emailTemplate.Subject));
        if (emailTemplate.BodyText == null) emailTemplate.BodyText = "";
        var paramBodyText = new OdSqlParameter("paramBodyText", OdDbType.Text, SOut.StringParam(emailTemplate.BodyText));
        if (emailTemplate.Description == null) emailTemplate.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(emailTemplate.Description));
        Db.NonQ(command, paramSubject, paramBodyText, paramDescription);
    }

    public static bool Update(EmailTemplate emailTemplate, EmailTemplate oldEmailTemplate)
    {
        var command = "";
        if (emailTemplate.Subject != oldEmailTemplate.Subject)
        {
            if (command != "") command += ",";
            command += "Subject = " + DbHelper.ParamChar + "paramSubject";
        }

        if (emailTemplate.BodyText != oldEmailTemplate.BodyText)
        {
            if (command != "") command += ",";
            command += "BodyText = " + DbHelper.ParamChar + "paramBodyText";
        }

        if (emailTemplate.Description != oldEmailTemplate.Description)
        {
            if (command != "") command += ",";
            command += "Description = " + DbHelper.ParamChar + "paramDescription";
        }

        if (emailTemplate.TemplateType != oldEmailTemplate.TemplateType)
        {
            if (command != "") command += ",";
            command += "TemplateType = " + SOut.Int((int) emailTemplate.TemplateType) + "";
        }

        if (command == "") return false;
        if (emailTemplate.Subject == null) emailTemplate.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", OdDbType.Text, SOut.StringParam(emailTemplate.Subject));
        if (emailTemplate.BodyText == null) emailTemplate.BodyText = "";
        var paramBodyText = new OdSqlParameter("paramBodyText", OdDbType.Text, SOut.StringParam(emailTemplate.BodyText));
        if (emailTemplate.Description == null) emailTemplate.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", OdDbType.Text, SOut.StringParam(emailTemplate.Description));
        command = "UPDATE emailtemplate SET " + command
                                              + " WHERE EmailTemplateNum = " + SOut.Long(emailTemplate.EmailTemplateNum);
        Db.NonQ(command, paramSubject, paramBodyText, paramDescription);
        return true;
    }

    public static bool UpdateComparison(EmailTemplate emailTemplate, EmailTemplate oldEmailTemplate)
    {
        if (emailTemplate.Subject != oldEmailTemplate.Subject) return true;
        if (emailTemplate.BodyText != oldEmailTemplate.BodyText) return true;
        if (emailTemplate.Description != oldEmailTemplate.Description) return true;
        if (emailTemplate.TemplateType != oldEmailTemplate.TemplateType) return true;
        return false;
    }

    public static void Delete(long emailTemplateNum)
    {
        var command = "DELETE FROM emailtemplate "
                      + "WHERE EmailTemplateNum = " + SOut.Long(emailTemplateNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEmailTemplateNums)
    {
        if (listEmailTemplateNums == null || listEmailTemplateNums.Count == 0) return;
        var command = "DELETE FROM emailtemplate "
                      + "WHERE EmailTemplateNum IN(" + string.Join(",", listEmailTemplateNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}