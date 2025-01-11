#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EmailMessageUidCrud
{
    public static EmailMessageUid SelectOne(long emailMessageUidNum)
    {
        var command = "SELECT * FROM emailmessageuid "
                      + "WHERE EmailMessageUidNum = " + SOut.Long(emailMessageUidNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EmailMessageUid SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EmailMessageUid> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EmailMessageUid> TableToList(DataTable table)
    {
        var retVal = new List<EmailMessageUid>();
        EmailMessageUid emailMessageUid;
        foreach (DataRow row in table.Rows)
        {
            emailMessageUid = new EmailMessageUid();
            emailMessageUid.EmailMessageUidNum = SIn.Long(row["EmailMessageUidNum"].ToString());
            emailMessageUid.MsgId = SIn.String(row["MsgId"].ToString());
            emailMessageUid.RecipientAddress = SIn.String(row["RecipientAddress"].ToString());
            retVal.Add(emailMessageUid);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EmailMessageUid> listEmailMessageUids, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EmailMessageUid";
        var table = new DataTable(tableName);
        table.Columns.Add("EmailMessageUidNum");
        table.Columns.Add("MsgId");
        table.Columns.Add("RecipientAddress");
        foreach (var emailMessageUid in listEmailMessageUids)
            table.Rows.Add(SOut.Long(emailMessageUid.EmailMessageUidNum), emailMessageUid.MsgId, emailMessageUid.RecipientAddress);
        return table;
    }

    public static long Insert(EmailMessageUid emailMessageUid)
    {
        return Insert(emailMessageUid, false);
    }

    public static long Insert(EmailMessageUid emailMessageUid, bool useExistingPK)
    {
        var command = "INSERT INTO emailmessageuid (";

        command += "MsgId,RecipientAddress) VALUES(";

        command +=
            DbHelper.ParamChar + "paramMsgId,"
                               + "'" + SOut.String(emailMessageUid.RecipientAddress) + "')";
        if (emailMessageUid.MsgId == null) emailMessageUid.MsgId = "";
        var paramMsgId = new OdSqlParameter("paramMsgId", OdDbType.Text, SOut.StringParam(emailMessageUid.MsgId));
        {
            emailMessageUid.EmailMessageUidNum = Db.NonQ(command, true, "EmailMessageUidNum", "emailMessageUid", paramMsgId);
        }
        return emailMessageUid.EmailMessageUidNum;
    }

    public static long InsertNoCache(EmailMessageUid emailMessageUid)
    {
        return InsertNoCache(emailMessageUid, false);
    }

    public static long InsertNoCache(EmailMessageUid emailMessageUid, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO emailmessageuid (";
        if (isRandomKeys || useExistingPK) command += "EmailMessageUidNum,";
        command += "MsgId,RecipientAddress) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(emailMessageUid.EmailMessageUidNum) + ",";
        command +=
            DbHelper.ParamChar + "paramMsgId,"
                               + "'" + SOut.String(emailMessageUid.RecipientAddress) + "')";
        if (emailMessageUid.MsgId == null) emailMessageUid.MsgId = "";
        var paramMsgId = new OdSqlParameter("paramMsgId", OdDbType.Text, SOut.StringParam(emailMessageUid.MsgId));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramMsgId);
        else
            emailMessageUid.EmailMessageUidNum = Db.NonQ(command, true, "EmailMessageUidNum", "emailMessageUid", paramMsgId);
        return emailMessageUid.EmailMessageUidNum;
    }

    public static void Update(EmailMessageUid emailMessageUid)
    {
        var command = "UPDATE emailmessageuid SET "
                      + "MsgId             =  " + DbHelper.ParamChar + "paramMsgId, "
                      + "RecipientAddress  = '" + SOut.String(emailMessageUid.RecipientAddress) + "' "
                      + "WHERE EmailMessageUidNum = " + SOut.Long(emailMessageUid.EmailMessageUidNum);
        if (emailMessageUid.MsgId == null) emailMessageUid.MsgId = "";
        var paramMsgId = new OdSqlParameter("paramMsgId", OdDbType.Text, SOut.StringParam(emailMessageUid.MsgId));
        Db.NonQ(command, paramMsgId);
    }

    public static bool Update(EmailMessageUid emailMessageUid, EmailMessageUid oldEmailMessageUid)
    {
        var command = "";
        if (emailMessageUid.MsgId != oldEmailMessageUid.MsgId)
        {
            if (command != "") command += ",";
            command += "MsgId = " + DbHelper.ParamChar + "paramMsgId";
        }

        if (emailMessageUid.RecipientAddress != oldEmailMessageUid.RecipientAddress)
        {
            if (command != "") command += ",";
            command += "RecipientAddress = '" + SOut.String(emailMessageUid.RecipientAddress) + "'";
        }

        if (command == "") return false;
        if (emailMessageUid.MsgId == null) emailMessageUid.MsgId = "";
        var paramMsgId = new OdSqlParameter("paramMsgId", OdDbType.Text, SOut.StringParam(emailMessageUid.MsgId));
        command = "UPDATE emailmessageuid SET " + command
                                                + " WHERE EmailMessageUidNum = " + SOut.Long(emailMessageUid.EmailMessageUidNum);
        Db.NonQ(command, paramMsgId);
        return true;
    }

    public static bool UpdateComparison(EmailMessageUid emailMessageUid, EmailMessageUid oldEmailMessageUid)
    {
        if (emailMessageUid.MsgId != oldEmailMessageUid.MsgId) return true;
        if (emailMessageUid.RecipientAddress != oldEmailMessageUid.RecipientAddress) return true;
        return false;
    }

    public static void Delete(long emailMessageUidNum)
    {
        var command = "DELETE FROM emailmessageuid "
                      + "WHERE EmailMessageUidNum = " + SOut.Long(emailMessageUidNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEmailMessageUidNums)
    {
        if (listEmailMessageUidNums == null || listEmailMessageUidNums.Count == 0) return;
        var command = "DELETE FROM emailmessageuid "
                      + "WHERE EmailMessageUidNum IN(" + string.Join(",", listEmailMessageUidNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}