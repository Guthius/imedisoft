using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class EmailTemplateCrud
{
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

    public static void Insert(EmailTemplate emailTemplate)
    {
        var command = "INSERT INTO emailtemplate (";

        command += "Subject,BodyText,Description,TemplateType) VALUES(";

        command +=
            DbHelper.ParamChar + "paramSubject,"
                               + DbHelper.ParamChar + "paramBodyText,"
                               + DbHelper.ParamChar + "paramDescription,"
                               + SOut.Int((int) emailTemplate.TemplateType) + ")";
        if (emailTemplate.Subject == null) emailTemplate.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", SOut.StringParam(emailTemplate.Subject));
        if (emailTemplate.BodyText == null) emailTemplate.BodyText = "";
        var paramBodyText = new OdSqlParameter("paramBodyText", SOut.StringParam(emailTemplate.BodyText));
        if (emailTemplate.Description == null) emailTemplate.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", SOut.StringParam(emailTemplate.Description));
        {
            emailTemplate.EmailTemplateNum = Db.NonQ(command, true, "EmailTemplateNum", "emailTemplate", paramSubject, paramBodyText, paramDescription);
        }
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
        var paramSubject = new OdSqlParameter("paramSubject", SOut.StringParam(emailTemplate.Subject));
        if (emailTemplate.BodyText == null) emailTemplate.BodyText = "";
        var paramBodyText = new OdSqlParameter("paramBodyText", SOut.StringParam(emailTemplate.BodyText));
        if (emailTemplate.Description == null) emailTemplate.Description = "";
        var paramDescription = new OdSqlParameter("paramDescription", SOut.StringParam(emailTemplate.Description));
        Db.NonQ(command, paramSubject, paramBodyText, paramDescription);
    }
}