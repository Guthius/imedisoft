#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EmailHostingTemplateCrud
{
    public static EmailHostingTemplate SelectOne(long emailHostingTemplateNum)
    {
        var command = "SELECT * FROM emailhostingtemplate "
                      + "WHERE EmailHostingTemplateNum = " + SOut.Long(emailHostingTemplateNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EmailHostingTemplate SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EmailHostingTemplate> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EmailHostingTemplate> TableToList(DataTable table)
    {
        var retVal = new List<EmailHostingTemplate>();
        EmailHostingTemplate emailHostingTemplate;
        foreach (DataRow row in table.Rows)
        {
            emailHostingTemplate = new EmailHostingTemplate();
            emailHostingTemplate.EmailHostingTemplateNum = SIn.Long(row["EmailHostingTemplateNum"].ToString());
            emailHostingTemplate.TemplateName = SIn.String(row["TemplateName"].ToString());
            emailHostingTemplate.Subject = SIn.String(row["Subject"].ToString());
            emailHostingTemplate.BodyPlainText = SIn.String(row["BodyPlainText"].ToString());
            emailHostingTemplate.BodyHTML = SIn.String(row["BodyHTML"].ToString());
            emailHostingTemplate.TemplateId = SIn.Long(row["TemplateId"].ToString());
            emailHostingTemplate.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            var emailTemplateType = row["EmailTemplateType"].ToString();
            if (emailTemplateType == "")
                emailHostingTemplate.EmailTemplateType = 0;
            else
                try
                {
                    emailHostingTemplate.EmailTemplateType = (EmailType) Enum.Parse(typeof(EmailType), emailTemplateType);
                }
                catch
                {
                    emailHostingTemplate.EmailTemplateType = 0;
                }

            var templateType = row["TemplateType"].ToString();
            if (templateType == "")
                emailHostingTemplate.TemplateType = 0;
            else
                try
                {
                    emailHostingTemplate.TemplateType = (PromotionType) Enum.Parse(typeof(PromotionType), templateType);
                }
                catch
                {
                    emailHostingTemplate.TemplateType = 0;
                }

            retVal.Add(emailHostingTemplate);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EmailHostingTemplate> listEmailHostingTemplates, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EmailHostingTemplate";
        var table = new DataTable(tableName);
        table.Columns.Add("EmailHostingTemplateNum");
        table.Columns.Add("TemplateName");
        table.Columns.Add("Subject");
        table.Columns.Add("BodyPlainText");
        table.Columns.Add("BodyHTML");
        table.Columns.Add("TemplateId");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("EmailTemplateType");
        table.Columns.Add("TemplateType");
        foreach (var emailHostingTemplate in listEmailHostingTemplates)
            table.Rows.Add(SOut.Long(emailHostingTemplate.EmailHostingTemplateNum), emailHostingTemplate.TemplateName, emailHostingTemplate.Subject, emailHostingTemplate.BodyPlainText, emailHostingTemplate.BodyHTML, SOut.Long(emailHostingTemplate.TemplateId), SOut.Long(emailHostingTemplate.ClinicNum), SOut.Int((int) emailHostingTemplate.EmailTemplateType), SOut.Int((int) emailHostingTemplate.TemplateType));
        return table;
    }

    public static long Insert(EmailHostingTemplate emailHostingTemplate)
    {
        return Insert(emailHostingTemplate, false);
    }

    public static long Insert(EmailHostingTemplate emailHostingTemplate, bool useExistingPK)
    {
        var command = "INSERT INTO emailhostingtemplate (";

        command += "TemplateName,Subject,BodyPlainText,BodyHTML,TemplateId,ClinicNum,EmailTemplateType,TemplateType) VALUES(";

        command +=
            "'" + SOut.String(emailHostingTemplate.TemplateName) + "',"
            + DbHelper.ParamChar + "paramSubject,"
            + DbHelper.ParamChar + "paramBodyPlainText,"
            + DbHelper.ParamChar + "paramBodyHTML,"
            + SOut.Long(emailHostingTemplate.TemplateId) + ","
            + SOut.Long(emailHostingTemplate.ClinicNum) + ","
            + "'" + SOut.String(emailHostingTemplate.EmailTemplateType.ToString()) + "',"
            + "'" + SOut.String(emailHostingTemplate.TemplateType.ToString()) + "')";
        if (emailHostingTemplate.Subject == null) emailHostingTemplate.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", OdDbType.Text, SOut.StringParam(emailHostingTemplate.Subject));
        if (emailHostingTemplate.BodyPlainText == null) emailHostingTemplate.BodyPlainText = "";
        var paramBodyPlainText = new OdSqlParameter("paramBodyPlainText", OdDbType.Text, SOut.StringParam(emailHostingTemplate.BodyPlainText));
        if (emailHostingTemplate.BodyHTML == null) emailHostingTemplate.BodyHTML = "";
        var paramBodyHTML = new OdSqlParameter("paramBodyHTML", OdDbType.Text, SOut.StringParam(emailHostingTemplate.BodyHTML));
        {
            emailHostingTemplate.EmailHostingTemplateNum = Db.NonQ(command, true, "EmailHostingTemplateNum", "emailHostingTemplate", paramSubject, paramBodyPlainText, paramBodyHTML);
        }
        return emailHostingTemplate.EmailHostingTemplateNum;
    }

    public static long InsertNoCache(EmailHostingTemplate emailHostingTemplate)
    {
        return InsertNoCache(emailHostingTemplate, false);
    }

    public static long InsertNoCache(EmailHostingTemplate emailHostingTemplate, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO emailhostingtemplate (";
        if (isRandomKeys || useExistingPK) command += "EmailHostingTemplateNum,";
        command += "TemplateName,Subject,BodyPlainText,BodyHTML,TemplateId,ClinicNum,EmailTemplateType,TemplateType) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(emailHostingTemplate.EmailHostingTemplateNum) + ",";
        command +=
            "'" + SOut.String(emailHostingTemplate.TemplateName) + "',"
            + DbHelper.ParamChar + "paramSubject,"
            + DbHelper.ParamChar + "paramBodyPlainText,"
            + DbHelper.ParamChar + "paramBodyHTML,"
            + SOut.Long(emailHostingTemplate.TemplateId) + ","
            + SOut.Long(emailHostingTemplate.ClinicNum) + ","
            + "'" + SOut.String(emailHostingTemplate.EmailTemplateType.ToString()) + "',"
            + "'" + SOut.String(emailHostingTemplate.TemplateType.ToString()) + "')";
        if (emailHostingTemplate.Subject == null) emailHostingTemplate.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", OdDbType.Text, SOut.StringParam(emailHostingTemplate.Subject));
        if (emailHostingTemplate.BodyPlainText == null) emailHostingTemplate.BodyPlainText = "";
        var paramBodyPlainText = new OdSqlParameter("paramBodyPlainText", OdDbType.Text, SOut.StringParam(emailHostingTemplate.BodyPlainText));
        if (emailHostingTemplate.BodyHTML == null) emailHostingTemplate.BodyHTML = "";
        var paramBodyHTML = new OdSqlParameter("paramBodyHTML", OdDbType.Text, SOut.StringParam(emailHostingTemplate.BodyHTML));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramSubject, paramBodyPlainText, paramBodyHTML);
        else
            emailHostingTemplate.EmailHostingTemplateNum = Db.NonQ(command, true, "EmailHostingTemplateNum", "emailHostingTemplate", paramSubject, paramBodyPlainText, paramBodyHTML);
        return emailHostingTemplate.EmailHostingTemplateNum;
    }

    public static void Update(EmailHostingTemplate emailHostingTemplate)
    {
        var command = "UPDATE emailhostingtemplate SET "
                      + "TemplateName           = '" + SOut.String(emailHostingTemplate.TemplateName) + "', "
                      + "Subject                =  " + DbHelper.ParamChar + "paramSubject, "
                      + "BodyPlainText          =  " + DbHelper.ParamChar + "paramBodyPlainText, "
                      + "BodyHTML               =  " + DbHelper.ParamChar + "paramBodyHTML, "
                      + "TemplateId             =  " + SOut.Long(emailHostingTemplate.TemplateId) + ", "
                      + "ClinicNum              =  " + SOut.Long(emailHostingTemplate.ClinicNum) + ", "
                      + "EmailTemplateType      = '" + SOut.String(emailHostingTemplate.EmailTemplateType.ToString()) + "', "
                      + "TemplateType           = '" + SOut.String(emailHostingTemplate.TemplateType.ToString()) + "' "
                      + "WHERE EmailHostingTemplateNum = " + SOut.Long(emailHostingTemplate.EmailHostingTemplateNum);
        if (emailHostingTemplate.Subject == null) emailHostingTemplate.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", OdDbType.Text, SOut.StringParam(emailHostingTemplate.Subject));
        if (emailHostingTemplate.BodyPlainText == null) emailHostingTemplate.BodyPlainText = "";
        var paramBodyPlainText = new OdSqlParameter("paramBodyPlainText", OdDbType.Text, SOut.StringParam(emailHostingTemplate.BodyPlainText));
        if (emailHostingTemplate.BodyHTML == null) emailHostingTemplate.BodyHTML = "";
        var paramBodyHTML = new OdSqlParameter("paramBodyHTML", OdDbType.Text, SOut.StringParam(emailHostingTemplate.BodyHTML));
        Db.NonQ(command, paramSubject, paramBodyPlainText, paramBodyHTML);
    }

    public static bool Update(EmailHostingTemplate emailHostingTemplate, EmailHostingTemplate oldEmailHostingTemplate)
    {
        var command = "";
        if (emailHostingTemplate.TemplateName != oldEmailHostingTemplate.TemplateName)
        {
            if (command != "") command += ",";
            command += "TemplateName = '" + SOut.String(emailHostingTemplate.TemplateName) + "'";
        }

        if (emailHostingTemplate.Subject != oldEmailHostingTemplate.Subject)
        {
            if (command != "") command += ",";
            command += "Subject = " + DbHelper.ParamChar + "paramSubject";
        }

        if (emailHostingTemplate.BodyPlainText != oldEmailHostingTemplate.BodyPlainText)
        {
            if (command != "") command += ",";
            command += "BodyPlainText = " + DbHelper.ParamChar + "paramBodyPlainText";
        }

        if (emailHostingTemplate.BodyHTML != oldEmailHostingTemplate.BodyHTML)
        {
            if (command != "") command += ",";
            command += "BodyHTML = " + DbHelper.ParamChar + "paramBodyHTML";
        }

        if (emailHostingTemplate.TemplateId != oldEmailHostingTemplate.TemplateId)
        {
            if (command != "") command += ",";
            command += "TemplateId = " + SOut.Long(emailHostingTemplate.TemplateId) + "";
        }

        if (emailHostingTemplate.ClinicNum != oldEmailHostingTemplate.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(emailHostingTemplate.ClinicNum) + "";
        }

        if (emailHostingTemplate.EmailTemplateType != oldEmailHostingTemplate.EmailTemplateType)
        {
            if (command != "") command += ",";
            command += "EmailTemplateType = '" + SOut.String(emailHostingTemplate.EmailTemplateType.ToString()) + "'";
        }

        if (emailHostingTemplate.TemplateType != oldEmailHostingTemplate.TemplateType)
        {
            if (command != "") command += ",";
            command += "TemplateType = '" + SOut.String(emailHostingTemplate.TemplateType.ToString()) + "'";
        }

        if (command == "") return false;
        if (emailHostingTemplate.Subject == null) emailHostingTemplate.Subject = "";
        var paramSubject = new OdSqlParameter("paramSubject", OdDbType.Text, SOut.StringParam(emailHostingTemplate.Subject));
        if (emailHostingTemplate.BodyPlainText == null) emailHostingTemplate.BodyPlainText = "";
        var paramBodyPlainText = new OdSqlParameter("paramBodyPlainText", OdDbType.Text, SOut.StringParam(emailHostingTemplate.BodyPlainText));
        if (emailHostingTemplate.BodyHTML == null) emailHostingTemplate.BodyHTML = "";
        var paramBodyHTML = new OdSqlParameter("paramBodyHTML", OdDbType.Text, SOut.StringParam(emailHostingTemplate.BodyHTML));
        command = "UPDATE emailhostingtemplate SET " + command
                                                     + " WHERE EmailHostingTemplateNum = " + SOut.Long(emailHostingTemplate.EmailHostingTemplateNum);
        Db.NonQ(command, paramSubject, paramBodyPlainText, paramBodyHTML);
        return true;
    }

    public static bool UpdateComparison(EmailHostingTemplate emailHostingTemplate, EmailHostingTemplate oldEmailHostingTemplate)
    {
        if (emailHostingTemplate.TemplateName != oldEmailHostingTemplate.TemplateName) return true;
        if (emailHostingTemplate.Subject != oldEmailHostingTemplate.Subject) return true;
        if (emailHostingTemplate.BodyPlainText != oldEmailHostingTemplate.BodyPlainText) return true;
        if (emailHostingTemplate.BodyHTML != oldEmailHostingTemplate.BodyHTML) return true;
        if (emailHostingTemplate.TemplateId != oldEmailHostingTemplate.TemplateId) return true;
        if (emailHostingTemplate.ClinicNum != oldEmailHostingTemplate.ClinicNum) return true;
        if (emailHostingTemplate.EmailTemplateType != oldEmailHostingTemplate.EmailTemplateType) return true;
        if (emailHostingTemplate.TemplateType != oldEmailHostingTemplate.TemplateType) return true;
        return false;
    }

    public static void Delete(long emailHostingTemplateNum)
    {
        var command = "DELETE FROM emailhostingtemplate "
                      + "WHERE EmailHostingTemplateNum = " + SOut.Long(emailHostingTemplateNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEmailHostingTemplateNums)
    {
        if (listEmailHostingTemplateNums == null || listEmailHostingTemplateNums.Count == 0) return;
        var command = "DELETE FROM emailhostingtemplate "
                      + "WHERE EmailHostingTemplateNum IN(" + string.Join(",", listEmailHostingTemplateNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}