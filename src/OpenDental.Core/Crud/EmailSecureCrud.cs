#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EmailSecureCrud
{
    public static EmailSecure SelectOne(long emailSecureNum)
    {
        var command = "SELECT * FROM emailsecure "
                      + "WHERE EmailSecureNum = " + SOut.Long(emailSecureNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EmailSecure SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EmailSecure> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EmailSecure> TableToList(DataTable table)
    {
        var retVal = new List<EmailSecure>();
        EmailSecure emailSecure;
        foreach (DataRow row in table.Rows)
        {
            emailSecure = new EmailSecure();
            emailSecure.EmailSecureNum = SIn.Long(row["EmailSecureNum"].ToString());
            emailSecure.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            emailSecure.PatNum = SIn.Long(row["PatNum"].ToString());
            emailSecure.EmailMessageNum = SIn.Long(row["EmailMessageNum"].ToString());
            emailSecure.EmailChainFK = SIn.Long(row["EmailChainFK"].ToString());
            emailSecure.EmailFK = SIn.Long(row["EmailFK"].ToString());
            emailSecure.DateTEntry = SIn.DateTime(row["DateTEntry"].ToString());
            emailSecure.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            retVal.Add(emailSecure);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EmailSecure> listEmailSecures, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EmailSecure";
        var table = new DataTable(tableName);
        table.Columns.Add("EmailSecureNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("EmailMessageNum");
        table.Columns.Add("EmailChainFK");
        table.Columns.Add("EmailFK");
        table.Columns.Add("DateTEntry");
        table.Columns.Add("SecDateTEdit");
        foreach (var emailSecure in listEmailSecures)
            table.Rows.Add(SOut.Long(emailSecure.EmailSecureNum), SOut.Long(emailSecure.ClinicNum), SOut.Long(emailSecure.PatNum), SOut.Long(emailSecure.EmailMessageNum), SOut.Long(emailSecure.EmailChainFK), SOut.Long(emailSecure.EmailFK), SOut.DateT(emailSecure.DateTEntry, false), SOut.DateT(emailSecure.SecDateTEdit, false));
        return table;
    }

    public static long Insert(EmailSecure emailSecure)
    {
        return Insert(emailSecure, false);
    }

    public static long Insert(EmailSecure emailSecure, bool useExistingPK)
    {
        var command = "INSERT INTO emailsecure (";

        command += "ClinicNum,PatNum,EmailMessageNum,EmailChainFK,EmailFK,DateTEntry) VALUES(";

        command +=
            SOut.Long(emailSecure.ClinicNum) + ","
                                             + SOut.Long(emailSecure.PatNum) + ","
                                             + SOut.Long(emailSecure.EmailMessageNum) + ","
                                             + SOut.Long(emailSecure.EmailChainFK) + ","
                                             + SOut.Long(emailSecure.EmailFK) + ","
                                             + DbHelper.Now() + ")";
        //SecDateTEdit can only be set by MySQL

        emailSecure.EmailSecureNum = Db.NonQ(command, true, "EmailSecureNum", "emailSecure");
        return emailSecure.EmailSecureNum;
    }

    public static void InsertMany(List<EmailSecure> listEmailSecures)
    {
        InsertMany(listEmailSecures, false);
    }

    public static void InsertMany(List<EmailSecure> listEmailSecures, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listEmailSecures.Count)
        {
            var emailSecure = listEmailSecures[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO emailsecure (");
                if (useExistingPK) sbCommands.Append("EmailSecureNum,");
                sbCommands.Append("ClinicNum,PatNum,EmailMessageNum,EmailChainFK,EmailFK,DateTEntry) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(emailSecure.EmailSecureNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(emailSecure.ClinicNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(emailSecure.PatNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(emailSecure.EmailMessageNum));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(emailSecure.EmailChainFK));
            sbRow.Append(",");
            sbRow.Append(SOut.Long(emailSecure.EmailFK));
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
                if (index == listEmailSecures.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(EmailSecure emailSecure)
    {
        return InsertNoCache(emailSecure, false);
    }

    public static long InsertNoCache(EmailSecure emailSecure, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO emailsecure (";
        if (isRandomKeys || useExistingPK) command += "EmailSecureNum,";
        command += "ClinicNum,PatNum,EmailMessageNum,EmailChainFK,EmailFK,DateTEntry) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(emailSecure.EmailSecureNum) + ",";
        command +=
            SOut.Long(emailSecure.ClinicNum) + ","
                                             + SOut.Long(emailSecure.PatNum) + ","
                                             + SOut.Long(emailSecure.EmailMessageNum) + ","
                                             + SOut.Long(emailSecure.EmailChainFK) + ","
                                             + SOut.Long(emailSecure.EmailFK) + ","
                                             + DbHelper.Now() + ")";
        //SecDateTEdit can only be set by MySQL
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            emailSecure.EmailSecureNum = Db.NonQ(command, true, "EmailSecureNum", "emailSecure");
        return emailSecure.EmailSecureNum;
    }

    public static void Update(EmailSecure emailSecure)
    {
        var command = "UPDATE emailsecure SET "
                      + "ClinicNum      =  " + SOut.Long(emailSecure.ClinicNum) + ", "
                      + "PatNum         =  " + SOut.Long(emailSecure.PatNum) + ", "
                      + "EmailMessageNum=  " + SOut.Long(emailSecure.EmailMessageNum) + ", "
                      + "EmailChainFK   =  " + SOut.Long(emailSecure.EmailChainFK) + ", "
                      + "EmailFK        =  " + SOut.Long(emailSecure.EmailFK) + " "
                      //DateTEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "WHERE EmailSecureNum = " + SOut.Long(emailSecure.EmailSecureNum);
        Db.NonQ(command);
    }

    public static bool Update(EmailSecure emailSecure, EmailSecure oldEmailSecure)
    {
        var command = "";
        if (emailSecure.ClinicNum != oldEmailSecure.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(emailSecure.ClinicNum) + "";
        }

        if (emailSecure.PatNum != oldEmailSecure.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(emailSecure.PatNum) + "";
        }

        if (emailSecure.EmailMessageNum != oldEmailSecure.EmailMessageNum)
        {
            if (command != "") command += ",";
            command += "EmailMessageNum = " + SOut.Long(emailSecure.EmailMessageNum) + "";
        }

        if (emailSecure.EmailChainFK != oldEmailSecure.EmailChainFK)
        {
            if (command != "") command += ",";
            command += "EmailChainFK = " + SOut.Long(emailSecure.EmailChainFK) + "";
        }

        if (emailSecure.EmailFK != oldEmailSecure.EmailFK)
        {
            if (command != "") command += ",";
            command += "EmailFK = " + SOut.Long(emailSecure.EmailFK) + "";
        }

        //DateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (command == "") return false;
        command = "UPDATE emailsecure SET " + command
                                            + " WHERE EmailSecureNum = " + SOut.Long(emailSecure.EmailSecureNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EmailSecure emailSecure, EmailSecure oldEmailSecure)
    {
        if (emailSecure.ClinicNum != oldEmailSecure.ClinicNum) return true;
        if (emailSecure.PatNum != oldEmailSecure.PatNum) return true;
        if (emailSecure.EmailMessageNum != oldEmailSecure.EmailMessageNum) return true;
        if (emailSecure.EmailChainFK != oldEmailSecure.EmailChainFK) return true;
        if (emailSecure.EmailFK != oldEmailSecure.EmailFK) return true;
        //DateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        return false;
    }

    public static void Delete(long emailSecureNum)
    {
        var command = "DELETE FROM emailsecure "
                      + "WHERE EmailSecureNum = " + SOut.Long(emailSecureNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEmailSecureNums)
    {
        if (listEmailSecureNums == null || listEmailSecureNums.Count == 0) return;
        var command = "DELETE FROM emailsecure "
                      + "WHERE EmailSecureNum IN(" + string.Join(",", listEmailSecureNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}