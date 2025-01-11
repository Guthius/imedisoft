#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EmailSecureAttachCrud
{
    public static EmailSecureAttach SelectOne(long emailSecureAttachNum)
    {
        var command = "SELECT * FROM emailsecureattach "
                      + "WHERE EmailSecureAttachNum = " + SOut.Long(emailSecureAttachNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EmailSecureAttach SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EmailSecureAttach> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EmailSecureAttach> TableToList(DataTable table)
    {
        var retVal = new List<EmailSecureAttach>();
        EmailSecureAttach emailSecureAttach;
        foreach (DataRow row in table.Rows)
        {
            emailSecureAttach = new EmailSecureAttach();
            emailSecureAttach.EmailSecureAttachNum = SIn.Long(row["EmailSecureAttachNum"].ToString());
            emailSecureAttach.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            emailSecureAttach.EmailAttachNum = SIn.Long(row["EmailAttachNum"].ToString());
            emailSecureAttach.EmailSecureNum = SIn.Long(row["EmailSecureNum"].ToString());
            emailSecureAttach.AttachmentGuid = SIn.String(row["AttachmentGuid"].ToString());
            emailSecureAttach.DisplayedFileName = SIn.String(row["DisplayedFileName"].ToString());
            emailSecureAttach.Extension = SIn.String(row["Extension"].ToString());
            emailSecureAttach.DateTEntry = SIn.DateTime(row["DateTEntry"].ToString());
            emailSecureAttach.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            retVal.Add(emailSecureAttach);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EmailSecureAttach> listEmailSecureAttachs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EmailSecureAttach";
        var table = new DataTable(tableName);
        table.Columns.Add("EmailSecureAttachNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("EmailAttachNum");
        table.Columns.Add("EmailSecureNum");
        table.Columns.Add("AttachmentGuid");
        table.Columns.Add("DisplayedFileName");
        table.Columns.Add("Extension");
        table.Columns.Add("DateTEntry");
        table.Columns.Add("SecDateTEdit");
        foreach (var emailSecureAttach in listEmailSecureAttachs)
            table.Rows.Add(SOut.Long(emailSecureAttach.EmailSecureAttachNum), SOut.Long(emailSecureAttach.ClinicNum), SOut.Long(emailSecureAttach.EmailAttachNum), SOut.Long(emailSecureAttach.EmailSecureNum), emailSecureAttach.AttachmentGuid, emailSecureAttach.DisplayedFileName, emailSecureAttach.Extension, SOut.DateT(emailSecureAttach.DateTEntry, false), SOut.DateT(emailSecureAttach.SecDateTEdit, false));
        return table;
    }

    public static long Insert(EmailSecureAttach emailSecureAttach)
    {
        return Insert(emailSecureAttach, false);
    }

    public static long Insert(EmailSecureAttach emailSecureAttach, bool useExistingPK)
    {
        var command = "INSERT INTO emailsecureattach (";

        command += "ClinicNum,EmailAttachNum,EmailSecureNum,AttachmentGuid,DisplayedFileName,Extension,DateTEntry) VALUES(";

        command +=
            SOut.Long(emailSecureAttach.ClinicNum) + ","
                                                   + SOut.Long(emailSecureAttach.EmailAttachNum) + ","
                                                   + SOut.Long(emailSecureAttach.EmailSecureNum) + ","
                                                   + "'" + SOut.String(emailSecureAttach.AttachmentGuid) + "',"
                                                   + "'" + SOut.String(emailSecureAttach.DisplayedFileName) + "',"
                                                   + "'" + SOut.String(emailSecureAttach.Extension) + "',"
                                                   + DbHelper.Now() + ")";
        //SecDateTEdit can only be set by MySQL

        emailSecureAttach.EmailSecureAttachNum = Db.NonQ(command, true, "EmailSecureAttachNum", "emailSecureAttach");
        return emailSecureAttach.EmailSecureAttachNum;
    }

    public static void InsertMany(List<EmailSecureAttach> listEmailSecureAttachs)
    {
        InsertMany(listEmailSecureAttachs, false);
    }

    public static void InsertMany(List<EmailSecureAttach> listEmailSecureAttachs, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listEmailSecureAttachs.Count)
        {
            var emailSecureAttach = listEmailSecureAttachs[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO emailsecureattach (");
                if (useExistingPK) sbCommands.Append("EmailSecureAttachNum,");
                sbCommands.Append("ClinicNum,EmailAttachNum,EmailSecureNum,AttachmentGuid,DisplayedFileName,Extension,DateTEntry) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(emailSecureAttach.EmailSecureAttachNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(emailSecureAttach.ClinicNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(emailSecureAttach.EmailAttachNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(emailSecureAttach.EmailSecureNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(emailSecureAttach.AttachmentGuid) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(emailSecureAttach.DisplayedFileName) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(emailSecureAttach.Extension) + "'");
            sbRow.Append(",");
            sbRow.Append(DbHelper.Now());
            sbRow.Append(")");
            //SecDateTEdit can only be set by MySQL
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
                if (index == listEmailSecureAttachs.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(EmailSecureAttach emailSecureAttach)
    {
        return InsertNoCache(emailSecureAttach, false);
    }

    public static long InsertNoCache(EmailSecureAttach emailSecureAttach, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO emailsecureattach (";
        if (isRandomKeys || useExistingPK) command += "EmailSecureAttachNum,";
        command += "ClinicNum,EmailAttachNum,EmailSecureNum,AttachmentGuid,DisplayedFileName,Extension,DateTEntry) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(emailSecureAttach.EmailSecureAttachNum) + ",";
        command +=
            SOut.Long(emailSecureAttach.ClinicNum) + ","
                                                   + SOut.Long(emailSecureAttach.EmailAttachNum) + ","
                                                   + SOut.Long(emailSecureAttach.EmailSecureNum) + ","
                                                   + "'" + SOut.String(emailSecureAttach.AttachmentGuid) + "',"
                                                   + "'" + SOut.String(emailSecureAttach.DisplayedFileName) + "',"
                                                   + "'" + SOut.String(emailSecureAttach.Extension) + "',"
                                                   + DbHelper.Now() + ")";
        //SecDateTEdit can only be set by MySQL
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            emailSecureAttach.EmailSecureAttachNum = Db.NonQ(command, true, "EmailSecureAttachNum", "emailSecureAttach");
        return emailSecureAttach.EmailSecureAttachNum;
    }

    public static void Update(EmailSecureAttach emailSecureAttach)
    {
        var command = "UPDATE emailsecureattach SET "
                      + "ClinicNum           =  " + SOut.Long(emailSecureAttach.ClinicNum) + ", "
                      + "EmailAttachNum      =  " + SOut.Long(emailSecureAttach.EmailAttachNum) + ", "
                      + "EmailSecureNum      =  " + SOut.Long(emailSecureAttach.EmailSecureNum) + ", "
                      + "AttachmentGuid      = '" + SOut.String(emailSecureAttach.AttachmentGuid) + "', "
                      + "DisplayedFileName   = '" + SOut.String(emailSecureAttach.DisplayedFileName) + "', "
                      + "Extension           = '" + SOut.String(emailSecureAttach.Extension) + "' "
                      //DateTEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "WHERE EmailSecureAttachNum = " + SOut.Long(emailSecureAttach.EmailSecureAttachNum);
        Db.NonQ(command);
    }

    public static bool Update(EmailSecureAttach emailSecureAttach, EmailSecureAttach oldEmailSecureAttach)
    {
        var command = "";
        if (emailSecureAttach.ClinicNum != oldEmailSecureAttach.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(emailSecureAttach.ClinicNum) + "";
        }

        if (emailSecureAttach.EmailAttachNum != oldEmailSecureAttach.EmailAttachNum)
        {
            if (command != "") command += ",";
            command += "EmailAttachNum = " + SOut.Long(emailSecureAttach.EmailAttachNum) + "";
        }

        if (emailSecureAttach.EmailSecureNum != oldEmailSecureAttach.EmailSecureNum)
        {
            if (command != "") command += ",";
            command += "EmailSecureNum = " + SOut.Long(emailSecureAttach.EmailSecureNum) + "";
        }

        if (emailSecureAttach.AttachmentGuid != oldEmailSecureAttach.AttachmentGuid)
        {
            if (command != "") command += ",";
            command += "AttachmentGuid = '" + SOut.String(emailSecureAttach.AttachmentGuid) + "'";
        }

        if (emailSecureAttach.DisplayedFileName != oldEmailSecureAttach.DisplayedFileName)
        {
            if (command != "") command += ",";
            command += "DisplayedFileName = '" + SOut.String(emailSecureAttach.DisplayedFileName) + "'";
        }

        if (emailSecureAttach.Extension != oldEmailSecureAttach.Extension)
        {
            if (command != "") command += ",";
            command += "Extension = '" + SOut.String(emailSecureAttach.Extension) + "'";
        }

        //DateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (command == "") return false;
        command = "UPDATE emailsecureattach SET " + command
                                                  + " WHERE EmailSecureAttachNum = " + SOut.Long(emailSecureAttach.EmailSecureAttachNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EmailSecureAttach emailSecureAttach, EmailSecureAttach oldEmailSecureAttach)
    {
        if (emailSecureAttach.ClinicNum != oldEmailSecureAttach.ClinicNum) return true;
        if (emailSecureAttach.EmailAttachNum != oldEmailSecureAttach.EmailAttachNum) return true;
        if (emailSecureAttach.EmailSecureNum != oldEmailSecureAttach.EmailSecureNum) return true;
        if (emailSecureAttach.AttachmentGuid != oldEmailSecureAttach.AttachmentGuid) return true;
        if (emailSecureAttach.DisplayedFileName != oldEmailSecureAttach.DisplayedFileName) return true;
        if (emailSecureAttach.Extension != oldEmailSecureAttach.Extension) return true;
        //DateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        return false;
    }

    public static void Delete(long emailSecureAttachNum)
    {
        var command = "DELETE FROM emailsecureattach "
                      + "WHERE EmailSecureAttachNum = " + SOut.Long(emailSecureAttachNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEmailSecureAttachNums)
    {
        if (listEmailSecureAttachNums == null || listEmailSecureAttachNums.Count == 0) return;
        var command = "DELETE FROM emailsecureattach "
                      + "WHERE EmailSecureAttachNum IN(" + string.Join(",", listEmailSecureAttachNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}