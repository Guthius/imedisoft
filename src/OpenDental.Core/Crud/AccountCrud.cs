#region

using System.Collections.Generic;
using System.Data;
using System.Drawing;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class AccountCrud
{
    public static List<Account> SelectMany(string command)
    {
        return TableToList(DataCore.GetTable(command));
    }

    public static List<Account> TableToList(DataTable table)
    {
        var retVal = new List<Account>();
        foreach (DataRow row in table.Rows)
        {
            var account = new Account();
            account.AccountNum = SIn.Long(row["AccountNum"].ToString());
            account.Description = SIn.String(row["Description"].ToString());
            account.AcctType = (AccountType) SIn.Int(row["AcctType"].ToString());
            account.BankNumber = SIn.String(row["BankNumber"].ToString());
            account.Inactive = SIn.Bool(row["Inactive"].ToString());
            account.AccountColor = Color.FromArgb(SIn.Int(row["AccountColor"].ToString()));
            account.IsRetainedEarnings = SIn.Bool(row["IsRetainedEarnings"].ToString());
            retVal.Add(account);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Account> listAccounts, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Account";

        var table = new DataTable(tableName);
        table.Columns.Add("AccountNum");
        table.Columns.Add("Description");
        table.Columns.Add("AcctType");
        table.Columns.Add("BankNumber");
        table.Columns.Add("Inactive");
        table.Columns.Add("AccountColor");
        table.Columns.Add("IsRetainedEarnings");
        foreach (var account in listAccounts)
            table.Rows.Add(SOut.Long(account.AccountNum), account.Description, SOut.Int((int) account.AcctType), account.BankNumber, SOut.Bool(account.Inactive), SOut.Int(account.AccountColor.ToArgb()), SOut.Bool(account.IsRetainedEarnings));

        return table;
    }

    public static long Insert(Account account)
    {
        var command = "INSERT INTO account (";

        command += "Description,AcctType,BankNumber,Inactive,AccountColor,IsRetainedEarnings) VALUES(";

        command +=
            "'" + SOut.String(account.Description) + "',"
            + SOut.Int((int) account.AcctType) + ","
            + "'" + SOut.String(account.BankNumber) + "',"
            + SOut.Bool(account.Inactive) + ","
            + SOut.Int(account.AccountColor.ToArgb()) + ","
            + SOut.Bool(account.IsRetainedEarnings) + ")";
        {
            account.AccountNum = Db.NonQ(command, true, "AccountNum", "account");
        }
        return account.AccountNum;
    }

    public static void Update(Account account, Account oldAccount)
    {
        var command = "";
        if (account.Description != oldAccount.Description)
        {
            if (command != "") command += ",";

            command += "Description = '" + SOut.String(account.Description) + "'";
        }

        if (account.AcctType != oldAccount.AcctType)
        {
            if (command != "") command += ",";

            command += "AcctType = " + SOut.Int((int) account.AcctType) + "";
        }

        if (account.BankNumber != oldAccount.BankNumber)
        {
            if (command != "") command += ",";

            command += "BankNumber = '" + SOut.String(account.BankNumber) + "'";
        }

        if (account.Inactive != oldAccount.Inactive)
        {
            if (command != "") command += ",";

            command += "Inactive = " + SOut.Bool(account.Inactive) + "";
        }

        if (account.AccountColor != oldAccount.AccountColor)
        {
            if (command != "") command += ",";

            command += "AccountColor = " + SOut.Int(account.AccountColor.ToArgb()) + "";
        }

        if (account.IsRetainedEarnings != oldAccount.IsRetainedEarnings)
        {
            if (command != "") command += ",";

            command += "IsRetainedEarnings = " + SOut.Bool(account.IsRetainedEarnings) + "";
        }

        if (command == "") return;

        command = "UPDATE account SET " + command + " WHERE AccountNum = " + SOut.Long(account.AccountNum);
        Db.NonQ(command);
    }
}