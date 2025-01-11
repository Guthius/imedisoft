#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EmailAttachCrud
{
    public static EmailAttach SelectOne(long emailAttachNum)
    {
        var command = "SELECT * FROM emailattach "
                      + "WHERE EmailAttachNum = " + SOut.Long(emailAttachNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EmailAttach SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EmailAttach> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EmailAttach> TableToList(DataTable table)
    {
        var retVal = new List<EmailAttach>();
        EmailAttach emailAttach;
        foreach (DataRow row in table.Rows)
        {
            emailAttach = new EmailAttach();
            emailAttach.EmailAttachNum = SIn.Long(row["EmailAttachNum"].ToString());
            emailAttach.EmailMessageNum = SIn.Long(row["EmailMessageNum"].ToString());
            emailAttach.DisplayedFileName = SIn.String(row["DisplayedFileName"].ToString());
            emailAttach.ActualFileName = SIn.String(row["ActualFileName"].ToString());
            emailAttach.EmailTemplateNum = SIn.Long(row["EmailTemplateNum"].ToString());
            retVal.Add(emailAttach);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EmailAttach> listEmailAttachs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EmailAttach";
        var table = new DataTable(tableName);
        table.Columns.Add("EmailAttachNum");
        table.Columns.Add("EmailMessageNum");
        table.Columns.Add("DisplayedFileName");
        table.Columns.Add("ActualFileName");
        table.Columns.Add("EmailTemplateNum");
        foreach (var emailAttach in listEmailAttachs)
            table.Rows.Add(SOut.Long(emailAttach.EmailAttachNum), SOut.Long(emailAttach.EmailMessageNum), emailAttach.DisplayedFileName, emailAttach.ActualFileName, SOut.Long(emailAttach.EmailTemplateNum));
        return table;
    }

    public static long Insert(EmailAttach emailAttach)
    {
        return Insert(emailAttach, false);
    }

    public static long Insert(EmailAttach emailAttach, bool useExistingPK)
    {
        var command = "INSERT INTO emailattach (";

        command += "EmailMessageNum,DisplayedFileName,ActualFileName,EmailTemplateNum) VALUES(";

        command +=
            SOut.Long(emailAttach.EmailMessageNum) + ","
                                                   + "'" + SOut.String(emailAttach.DisplayedFileName) + "',"
                                                   + "'" + SOut.String(emailAttach.ActualFileName) + "',"
                                                   + SOut.Long(emailAttach.EmailTemplateNum) + ")";
        {
            emailAttach.EmailAttachNum = Db.NonQ(command, true, "EmailAttachNum", "emailAttach");
        }
        return emailAttach.EmailAttachNum;
    }

    public static void InsertMany(List<EmailAttach> listEmailAttachs)
    {
        InsertMany(listEmailAttachs, false);
    }

    public static void InsertMany(List<EmailAttach> listEmailAttachs, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listEmailAttachs.Count)
        {
            var emailAttach = listEmailAttachs[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO emailattach (");
                if (useExistingPK) sbCommands.Append("EmailAttachNum,");
                sbCommands.Append("EmailMessageNum,DisplayedFileName,ActualFileName,EmailTemplateNum) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(emailAttach.EmailAttachNum));
                sbRow.Append(",");
            }

            sbRow.Append(SOut.Long(emailAttach.EmailMessageNum));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(emailAttach.DisplayedFileName) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(emailAttach.ActualFileName) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Long(emailAttach.EmailTemplateNum));
            sbRow.Append(")");
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
                if (index == listEmailAttachs.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(EmailAttach emailAttach)
    {
        return InsertNoCache(emailAttach, false);
    }

    public static long InsertNoCache(EmailAttach emailAttach, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO emailattach (";
        if (isRandomKeys || useExistingPK) command += "EmailAttachNum,";
        command += "EmailMessageNum,DisplayedFileName,ActualFileName,EmailTemplateNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(emailAttach.EmailAttachNum) + ",";
        command +=
            SOut.Long(emailAttach.EmailMessageNum) + ","
                                                   + "'" + SOut.String(emailAttach.DisplayedFileName) + "',"
                                                   + "'" + SOut.String(emailAttach.ActualFileName) + "',"
                                                   + SOut.Long(emailAttach.EmailTemplateNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            emailAttach.EmailAttachNum = Db.NonQ(command, true, "EmailAttachNum", "emailAttach");
        return emailAttach.EmailAttachNum;
    }

    public static void Update(EmailAttach emailAttach)
    {
        var command = "UPDATE emailattach SET "
                      + "EmailMessageNum  =  " + SOut.Long(emailAttach.EmailMessageNum) + ", "
                      + "DisplayedFileName= '" + SOut.String(emailAttach.DisplayedFileName) + "', "
                      + "ActualFileName   = '" + SOut.String(emailAttach.ActualFileName) + "', "
                      + "EmailTemplateNum =  " + SOut.Long(emailAttach.EmailTemplateNum) + " "
                      + "WHERE EmailAttachNum = " + SOut.Long(emailAttach.EmailAttachNum);
        Db.NonQ(command);
    }

    public static bool Update(EmailAttach emailAttach, EmailAttach oldEmailAttach)
    {
        var command = "";
        if (emailAttach.EmailMessageNum != oldEmailAttach.EmailMessageNum)
        {
            if (command != "") command += ",";
            command += "EmailMessageNum = " + SOut.Long(emailAttach.EmailMessageNum) + "";
        }

        if (emailAttach.DisplayedFileName != oldEmailAttach.DisplayedFileName)
        {
            if (command != "") command += ",";
            command += "DisplayedFileName = '" + SOut.String(emailAttach.DisplayedFileName) + "'";
        }

        if (emailAttach.ActualFileName != oldEmailAttach.ActualFileName)
        {
            if (command != "") command += ",";
            command += "ActualFileName = '" + SOut.String(emailAttach.ActualFileName) + "'";
        }

        if (emailAttach.EmailTemplateNum != oldEmailAttach.EmailTemplateNum)
        {
            if (command != "") command += ",";
            command += "EmailTemplateNum = " + SOut.Long(emailAttach.EmailTemplateNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE emailattach SET " + command
                                            + " WHERE EmailAttachNum = " + SOut.Long(emailAttach.EmailAttachNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EmailAttach emailAttach, EmailAttach oldEmailAttach)
    {
        if (emailAttach.EmailMessageNum != oldEmailAttach.EmailMessageNum) return true;
        if (emailAttach.DisplayedFileName != oldEmailAttach.DisplayedFileName) return true;
        if (emailAttach.ActualFileName != oldEmailAttach.ActualFileName) return true;
        if (emailAttach.EmailTemplateNum != oldEmailAttach.EmailTemplateNum) return true;
        return false;
    }

    public static void Delete(long emailAttachNum)
    {
        var command = "DELETE FROM emailattach "
                      + "WHERE EmailAttachNum = " + SOut.Long(emailAttachNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEmailAttachNums)
    {
        if (listEmailAttachNums == null || listEmailAttachNums.Count == 0) return;
        var command = "DELETE FROM emailattach "
                      + "WHERE EmailAttachNum IN(" + string.Join(",", listEmailAttachNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }

    public static bool Sync(List<EmailAttach> listNew, List<EmailAttach> listDB)
    {
        //Adding items to lists changes the order of operation. All inserts are completed first, then updates, then deletes.
        var listIns = new List<EmailAttach>();
        var listUpdNew = new List<EmailAttach>();
        var listUpdDB = new List<EmailAttach>();
        var listDel = new List<EmailAttach>();
        listNew.Sort((x, y) => { return x.EmailAttachNum.CompareTo(y.EmailAttachNum); });
        listDB.Sort((x, y) => { return x.EmailAttachNum.CompareTo(y.EmailAttachNum); });
        var idxNew = 0;
        var idxDB = 0;
        var rowsUpdatedCount = 0;
        EmailAttach fieldNew;
        EmailAttach fieldDB;
        //Because both lists have been sorted using the same criteria, we can now walk each list to determine which list contians the next element.  The next element is determined by Primary Key.
        //If the New list contains the next item it will be inserted.  If the DB contains the next item, it will be deleted.  If both lists contain the next item, the item will be updated.
        while (idxNew < listNew.Count || idxDB < listDB.Count)
        {
            fieldNew = null;
            if (idxNew < listNew.Count) fieldNew = listNew[idxNew];
            fieldDB = null;
            if (idxDB < listDB.Count) fieldDB = listDB[idxDB];
            //begin compare
            if (fieldNew != null && fieldDB == null)
            {
                //listNew has more items, listDB does not.
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew == null && fieldDB != null)
            {
                //listDB has more items, listNew does not.
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            if (fieldNew.EmailAttachNum < fieldDB.EmailAttachNum)
            {
                //newPK less than dbPK, newItem is 'next'
                listIns.Add(fieldNew);
                idxNew++;
                continue;
            }

            if (fieldNew.EmailAttachNum > fieldDB.EmailAttachNum)
            {
                //dbPK less than newPK, dbItem is 'next'
                listDel.Add(fieldDB);
                idxDB++;
                continue;
            }

            //Both lists contain the 'next' item, update required
            listUpdNew.Add(fieldNew);
            listUpdDB.Add(fieldDB);
            idxNew++;
            idxDB++;
        }

        //Commit changes to DB
        for (var i = 0; i < listIns.Count; i++) Insert(listIns[i]);
        for (var i = 0; i < listUpdNew.Count; i++)
            if (Update(listUpdNew[i], listUpdDB[i]))
                rowsUpdatedCount++;

        DeleteMany(listDel.Select(x => x.EmailAttachNum).ToList());
        if (rowsUpdatedCount > 0 || listIns.Count > 0 || listDel.Count > 0) return true;
        return false;
    }
}