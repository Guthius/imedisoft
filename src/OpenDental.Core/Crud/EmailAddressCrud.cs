using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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
        foreach (DataRow row in table.Rows)
        {
            var emailAddress = new EmailAddress
            {
                EmailAddressNum = SIn.Long(row["EmailAddressNum"].ToString()),
                SMTPserver = SIn.String(row["SMTPserver"].ToString()),
                EmailUsername = SIn.String(row["EmailUsername"].ToString()),
                EmailPassword = SIn.String(row["EmailPassword"].ToString()),
                ServerPort = SIn.Int(row["ServerPort"].ToString()),
                UseSSL = SIn.Bool(row["UseSSL"].ToString()),
                SenderAddress = SIn.String(row["SenderAddress"].ToString()),
                Pop3ServerIncoming = SIn.String(row["Pop3ServerIncoming"].ToString()),
                ServerPortIncoming = SIn.Int(row["ServerPortIncoming"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                AccessToken = SIn.String(row["AccessToken"].ToString()),
                RefreshToken = SIn.String(row["RefreshToken"].ToString()),
                DownloadInbox = SIn.Bool(row["DownloadInbox"].ToString()),
                QueryString = SIn.String(row["QueryString"].ToString()),
                AuthenticationType = (OAuthType) SIn.Int(row["AuthenticationType"].ToString())
            };
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

    public static void Insert(EmailAddress emailAddress)
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
        var paramRefreshToken = new OdSqlParameter("paramRefreshToken", SOut.StringParam(emailAddress.RefreshToken));
        {
            emailAddress.EmailAddressNum = Db.NonQ(command, true, "EmailAddressNum", "emailAddress", paramRefreshToken);
        }
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
        var paramRefreshToken = new OdSqlParameter("paramRefreshToken", SOut.StringParam(emailAddress.RefreshToken));
        Db.NonQ(command, paramRefreshToken);
    }
}