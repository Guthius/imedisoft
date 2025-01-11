#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EmailAddressCrud
{
    public static EmailAddress SelectOne(long emailAddressNum)
    {
        var command = "SELECT * FROM emailaddress "
                      + "WHERE EmailAddressNum = " + SOut.Long(emailAddressNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EmailAddress SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EmailAddress> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EmailAddress> TableToList(DataTable table)
    {
        var retVal = new List<EmailAddress>();
        EmailAddress emailAddress;
        foreach (DataRow row in table.Rows)
        {
            emailAddress = new EmailAddress();
            emailAddress.EmailAddressNum = SIn.Long(row["EmailAddressNum"].ToString());
            emailAddress.SMTPserver = SIn.String(row["SMTPserver"].ToString());
            emailAddress.EmailUsername = SIn.String(row["EmailUsername"].ToString());
            emailAddress.EmailPassword = SIn.String(row["EmailPassword"].ToString());
            emailAddress.ServerPort = SIn.Int(row["ServerPort"].ToString());
            emailAddress.UseSSL = SIn.Bool(row["UseSSL"].ToString());
            emailAddress.SenderAddress = SIn.String(row["SenderAddress"].ToString());
            emailAddress.Pop3ServerIncoming = SIn.String(row["Pop3ServerIncoming"].ToString());
            emailAddress.ServerPortIncoming = SIn.Int(row["ServerPortIncoming"].ToString());
            emailAddress.UserNum = SIn.Long(row["UserNum"].ToString());
            emailAddress.AccessToken = SIn.String(row["AccessToken"].ToString());
            emailAddress.RefreshToken = SIn.String(row["RefreshToken"].ToString());
            emailAddress.DownloadInbox = SIn.Bool(row["DownloadInbox"].ToString());
            emailAddress.QueryString = SIn.String(row["QueryString"].ToString());
            emailAddress.AuthenticationType = (OAuthType) SIn.Int(row["AuthenticationType"].ToString());
            retVal.Add(emailAddress);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EmailAddress> listEmailAddresss, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EmailAddress";
        var table = new DataTable(tableName);
        table.Columns.Add("EmailAddressNum");
        table.Columns.Add("SMTPserver");
        table.Columns.Add("EmailUsername");
        table.Columns.Add("EmailPassword");
        table.Columns.Add("ServerPort");
        table.Columns.Add("UseSSL");
        table.Columns.Add("SenderAddress");
        table.Columns.Add("Pop3ServerIncoming");
        table.Columns.Add("ServerPortIncoming");
        table.Columns.Add("UserNum");
        table.Columns.Add("AccessToken");
        table.Columns.Add("RefreshToken");
        table.Columns.Add("DownloadInbox");
        table.Columns.Add("QueryString");
        table.Columns.Add("AuthenticationType");
        foreach (var emailAddress in listEmailAddresss)
            table.Rows.Add(SOut.Long(emailAddress.EmailAddressNum), emailAddress.SMTPserver, emailAddress.EmailUsername, emailAddress.EmailPassword, SOut.Int(emailAddress.ServerPort), SOut.Bool(emailAddress.UseSSL), emailAddress.SenderAddress, emailAddress.Pop3ServerIncoming, SOut.Int(emailAddress.ServerPortIncoming), SOut.Long(emailAddress.UserNum), emailAddress.AccessToken, emailAddress.RefreshToken, SOut.Bool(emailAddress.DownloadInbox), emailAddress.QueryString, SOut.Int((int) emailAddress.AuthenticationType));
        return table;
    }

    public static long Insert(EmailAddress emailAddress)
    {
        return Insert(emailAddress, false);
    }

    public static long Insert(EmailAddress emailAddress, bool useExistingPK)
    {
        var command = "INSERT INTO emailaddress (";

        command += "SMTPserver,EmailUsername,EmailPassword,ServerPort,UseSSL,SenderAddress,Pop3ServerIncoming,ServerPortIncoming,UserNum,AccessToken,RefreshToken,DownloadInbox,QueryString,AuthenticationType) VALUES(";

        command +=
            "'" + SOut.String(emailAddress.SMTPserver) + "',"
            + "'" + SOut.String(emailAddress.EmailUsername) + "',"
            + "'" + SOut.String(emailAddress.EmailPassword) + "',"
            + SOut.Int(emailAddress.ServerPort) + ","
            + SOut.Bool(emailAddress.UseSSL) + ","
            + "'" + SOut.String(emailAddress.SenderAddress) + "',"
            + "'" + SOut.String(emailAddress.Pop3ServerIncoming) + "',"
            + SOut.Int(emailAddress.ServerPortIncoming) + ","
            + SOut.Long(emailAddress.UserNum) + ","
            + "'" + SOut.String(emailAddress.AccessToken) + "',"
            + DbHelper.ParamChar + "paramRefreshToken,"
            + SOut.Bool(emailAddress.DownloadInbox) + ","
            + "'" + SOut.String(emailAddress.QueryString) + "',"
            + SOut.Int((int) emailAddress.AuthenticationType) + ")";
        if (emailAddress.RefreshToken == null) emailAddress.RefreshToken = "";
        var paramRefreshToken = new OdSqlParameter("paramRefreshToken", OdDbType.Text, SOut.StringParam(emailAddress.RefreshToken));
        {
            emailAddress.EmailAddressNum = Db.NonQ(command, true, "EmailAddressNum", "emailAddress", paramRefreshToken);
        }
        return emailAddress.EmailAddressNum;
    }

    public static long InsertNoCache(EmailAddress emailAddress)
    {
        return InsertNoCache(emailAddress, false);
    }

    public static long InsertNoCache(EmailAddress emailAddress, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO emailaddress (";
        if (isRandomKeys || useExistingPK) command += "EmailAddressNum,";
        command += "SMTPserver,EmailUsername,EmailPassword,ServerPort,UseSSL,SenderAddress,Pop3ServerIncoming,ServerPortIncoming,UserNum,AccessToken,RefreshToken,DownloadInbox,QueryString,AuthenticationType) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(emailAddress.EmailAddressNum) + ",";
        command +=
            "'" + SOut.String(emailAddress.SMTPserver) + "',"
            + "'" + SOut.String(emailAddress.EmailUsername) + "',"
            + "'" + SOut.String(emailAddress.EmailPassword) + "',"
            + SOut.Int(emailAddress.ServerPort) + ","
            + SOut.Bool(emailAddress.UseSSL) + ","
            + "'" + SOut.String(emailAddress.SenderAddress) + "',"
            + "'" + SOut.String(emailAddress.Pop3ServerIncoming) + "',"
            + SOut.Int(emailAddress.ServerPortIncoming) + ","
            + SOut.Long(emailAddress.UserNum) + ","
            + "'" + SOut.String(emailAddress.AccessToken) + "',"
            + DbHelper.ParamChar + "paramRefreshToken,"
            + SOut.Bool(emailAddress.DownloadInbox) + ","
            + "'" + SOut.String(emailAddress.QueryString) + "',"
            + SOut.Int((int) emailAddress.AuthenticationType) + ")";
        if (emailAddress.RefreshToken == null) emailAddress.RefreshToken = "";
        var paramRefreshToken = new OdSqlParameter("paramRefreshToken", OdDbType.Text, SOut.StringParam(emailAddress.RefreshToken));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramRefreshToken);
        else
            emailAddress.EmailAddressNum = Db.NonQ(command, true, "EmailAddressNum", "emailAddress", paramRefreshToken);
        return emailAddress.EmailAddressNum;
    }

    public static void Update(EmailAddress emailAddress)
    {
        var command = "UPDATE emailaddress SET "
                      + "SMTPserver        = '" + SOut.String(emailAddress.SMTPserver) + "', "
                      + "EmailUsername     = '" + SOut.String(emailAddress.EmailUsername) + "', "
                      + "EmailPassword     = '" + SOut.String(emailAddress.EmailPassword) + "', "
                      + "ServerPort        =  " + SOut.Int(emailAddress.ServerPort) + ", "
                      + "UseSSL            =  " + SOut.Bool(emailAddress.UseSSL) + ", "
                      + "SenderAddress     = '" + SOut.String(emailAddress.SenderAddress) + "', "
                      + "Pop3ServerIncoming= '" + SOut.String(emailAddress.Pop3ServerIncoming) + "', "
                      + "ServerPortIncoming=  " + SOut.Int(emailAddress.ServerPortIncoming) + ", "
                      + "UserNum           =  " + SOut.Long(emailAddress.UserNum) + ", "
                      + "AccessToken       = '" + SOut.String(emailAddress.AccessToken) + "', "
                      + "RefreshToken      =  " + DbHelper.ParamChar + "paramRefreshToken, "
                      + "DownloadInbox     =  " + SOut.Bool(emailAddress.DownloadInbox) + ", "
                      + "QueryString       = '" + SOut.String(emailAddress.QueryString) + "', "
                      + "AuthenticationType=  " + SOut.Int((int) emailAddress.AuthenticationType) + " "
                      + "WHERE EmailAddressNum = " + SOut.Long(emailAddress.EmailAddressNum);
        if (emailAddress.RefreshToken == null) emailAddress.RefreshToken = "";
        var paramRefreshToken = new OdSqlParameter("paramRefreshToken", OdDbType.Text, SOut.StringParam(emailAddress.RefreshToken));
        Db.NonQ(command, paramRefreshToken);
    }

    public static bool Update(EmailAddress emailAddress, EmailAddress oldEmailAddress)
    {
        var command = "";
        if (emailAddress.SMTPserver != oldEmailAddress.SMTPserver)
        {
            if (command != "") command += ",";
            command += "SMTPserver = '" + SOut.String(emailAddress.SMTPserver) + "'";
        }

        if (emailAddress.EmailUsername != oldEmailAddress.EmailUsername)
        {
            if (command != "") command += ",";
            command += "EmailUsername = '" + SOut.String(emailAddress.EmailUsername) + "'";
        }

        if (emailAddress.EmailPassword != oldEmailAddress.EmailPassword)
        {
            if (command != "") command += ",";
            command += "EmailPassword = '" + SOut.String(emailAddress.EmailPassword) + "'";
        }

        if (emailAddress.ServerPort != oldEmailAddress.ServerPort)
        {
            if (command != "") command += ",";
            command += "ServerPort = " + SOut.Int(emailAddress.ServerPort) + "";
        }

        if (emailAddress.UseSSL != oldEmailAddress.UseSSL)
        {
            if (command != "") command += ",";
            command += "UseSSL = " + SOut.Bool(emailAddress.UseSSL) + "";
        }

        if (emailAddress.SenderAddress != oldEmailAddress.SenderAddress)
        {
            if (command != "") command += ",";
            command += "SenderAddress = '" + SOut.String(emailAddress.SenderAddress) + "'";
        }

        if (emailAddress.Pop3ServerIncoming != oldEmailAddress.Pop3ServerIncoming)
        {
            if (command != "") command += ",";
            command += "Pop3ServerIncoming = '" + SOut.String(emailAddress.Pop3ServerIncoming) + "'";
        }

        if (emailAddress.ServerPortIncoming != oldEmailAddress.ServerPortIncoming)
        {
            if (command != "") command += ",";
            command += "ServerPortIncoming = " + SOut.Int(emailAddress.ServerPortIncoming) + "";
        }

        if (emailAddress.UserNum != oldEmailAddress.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(emailAddress.UserNum) + "";
        }

        if (emailAddress.AccessToken != oldEmailAddress.AccessToken)
        {
            if (command != "") command += ",";
            command += "AccessToken = '" + SOut.String(emailAddress.AccessToken) + "'";
        }

        if (emailAddress.RefreshToken != oldEmailAddress.RefreshToken)
        {
            if (command != "") command += ",";
            command += "RefreshToken = " + DbHelper.ParamChar + "paramRefreshToken";
        }

        if (emailAddress.DownloadInbox != oldEmailAddress.DownloadInbox)
        {
            if (command != "") command += ",";
            command += "DownloadInbox = " + SOut.Bool(emailAddress.DownloadInbox) + "";
        }

        if (emailAddress.QueryString != oldEmailAddress.QueryString)
        {
            if (command != "") command += ",";
            command += "QueryString = '" + SOut.String(emailAddress.QueryString) + "'";
        }

        if (emailAddress.AuthenticationType != oldEmailAddress.AuthenticationType)
        {
            if (command != "") command += ",";
            command += "AuthenticationType = " + SOut.Int((int) emailAddress.AuthenticationType) + "";
        }

        if (command == "") return false;
        if (emailAddress.RefreshToken == null) emailAddress.RefreshToken = "";
        var paramRefreshToken = new OdSqlParameter("paramRefreshToken", OdDbType.Text, SOut.StringParam(emailAddress.RefreshToken));
        command = "UPDATE emailaddress SET " + command
                                             + " WHERE EmailAddressNum = " + SOut.Long(emailAddress.EmailAddressNum);
        Db.NonQ(command, paramRefreshToken);
        return true;
    }

    public static bool UpdateComparison(EmailAddress emailAddress, EmailAddress oldEmailAddress)
    {
        if (emailAddress.SMTPserver != oldEmailAddress.SMTPserver) return true;
        if (emailAddress.EmailUsername != oldEmailAddress.EmailUsername) return true;
        if (emailAddress.EmailPassword != oldEmailAddress.EmailPassword) return true;
        if (emailAddress.ServerPort != oldEmailAddress.ServerPort) return true;
        if (emailAddress.UseSSL != oldEmailAddress.UseSSL) return true;
        if (emailAddress.SenderAddress != oldEmailAddress.SenderAddress) return true;
        if (emailAddress.Pop3ServerIncoming != oldEmailAddress.Pop3ServerIncoming) return true;
        if (emailAddress.ServerPortIncoming != oldEmailAddress.ServerPortIncoming) return true;
        if (emailAddress.UserNum != oldEmailAddress.UserNum) return true;
        if (emailAddress.AccessToken != oldEmailAddress.AccessToken) return true;
        if (emailAddress.RefreshToken != oldEmailAddress.RefreshToken) return true;
        if (emailAddress.DownloadInbox != oldEmailAddress.DownloadInbox) return true;
        if (emailAddress.QueryString != oldEmailAddress.QueryString) return true;
        if (emailAddress.AuthenticationType != oldEmailAddress.AuthenticationType) return true;
        return false;
    }

    public static void Delete(long emailAddressNum)
    {
        var command = "DELETE FROM emailaddress "
                      + "WHERE EmailAddressNum = " + SOut.Long(emailAddressNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEmailAddressNums)
    {
        if (listEmailAddressNums == null || listEmailAddressNums.Count == 0) return;
        var command = "DELETE FROM emailaddress "
                      + "WHERE EmailAddressNum IN(" + string.Join(",", listEmailAddressNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}