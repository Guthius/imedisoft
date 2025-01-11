#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DepositCrud
{
    public static Deposit SelectOne(long depositNum)
    {
        var command = "SELECT * FROM deposit "
                      + "WHERE DepositNum = " + SOut.Long(depositNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Deposit SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Deposit> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Deposit> TableToList(DataTable table)
    {
        var retVal = new List<Deposit>();
        Deposit deposit;
        foreach (DataRow row in table.Rows)
        {
            deposit = new Deposit();
            deposit.DepositNum = SIn.Long(row["DepositNum"].ToString());
            deposit.DateDeposit = SIn.Date(row["DateDeposit"].ToString());
            deposit.BankAccountInfo = SIn.String(row["BankAccountInfo"].ToString());
            deposit.Amount = SIn.Double(row["Amount"].ToString());
            deposit.Memo = SIn.String(row["Memo"].ToString());
            deposit.Batch = SIn.String(row["Batch"].ToString());
            deposit.DepositAccountNum = SIn.Long(row["DepositAccountNum"].ToString());
            deposit.IsSentToQuickBooksOnline = SIn.Bool(row["IsSentToQuickBooksOnline"].ToString());
            retVal.Add(deposit);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Deposit> listDeposits, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Deposit";
        var table = new DataTable(tableName);
        table.Columns.Add("DepositNum");
        table.Columns.Add("DateDeposit");
        table.Columns.Add("BankAccountInfo");
        table.Columns.Add("Amount");
        table.Columns.Add("Memo");
        table.Columns.Add("Batch");
        table.Columns.Add("DepositAccountNum");
        table.Columns.Add("IsSentToQuickBooksOnline");
        foreach (var deposit in listDeposits)
            table.Rows.Add(SOut.Long(deposit.DepositNum), SOut.DateT(deposit.DateDeposit, false), deposit.BankAccountInfo, SOut.Double(deposit.Amount), deposit.Memo, deposit.Batch, SOut.Long(deposit.DepositAccountNum), SOut.Bool(deposit.IsSentToQuickBooksOnline));
        return table;
    }

    public static long Insert(Deposit deposit)
    {
        return Insert(deposit, false);
    }

    public static long Insert(Deposit deposit, bool useExistingPK)
    {
        var command = "INSERT INTO deposit (";

        command += "DateDeposit,BankAccountInfo,Amount,Memo,Batch,DepositAccountNum,IsSentToQuickBooksOnline) VALUES(";

        command +=
            SOut.Date(deposit.DateDeposit) + ","
                                           + DbHelper.ParamChar + "paramBankAccountInfo,"
                                           + SOut.Double(deposit.Amount) + ","
                                           + "'" + SOut.String(deposit.Memo) + "',"
                                           + "'" + SOut.String(deposit.Batch) + "',"
                                           + SOut.Long(deposit.DepositAccountNum) + ","
                                           + SOut.Bool(deposit.IsSentToQuickBooksOnline) + ")";
        if (deposit.BankAccountInfo == null) deposit.BankAccountInfo = "";
        var paramBankAccountInfo = new OdSqlParameter("paramBankAccountInfo", OdDbType.Text, SOut.StringParam(deposit.BankAccountInfo));
        {
            deposit.DepositNum = Db.NonQ(command, true, "DepositNum", "deposit", paramBankAccountInfo);
        }
        return deposit.DepositNum;
    }

    public static long InsertNoCache(Deposit deposit)
    {
        return InsertNoCache(deposit, false);
    }

    public static long InsertNoCache(Deposit deposit, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO deposit (";
        if (isRandomKeys || useExistingPK) command += "DepositNum,";
        command += "DateDeposit,BankAccountInfo,Amount,Memo,Batch,DepositAccountNum,IsSentToQuickBooksOnline) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(deposit.DepositNum) + ",";
        command +=
            SOut.Date(deposit.DateDeposit) + ","
                                           + DbHelper.ParamChar + "paramBankAccountInfo,"
                                           + SOut.Double(deposit.Amount) + ","
                                           + "'" + SOut.String(deposit.Memo) + "',"
                                           + "'" + SOut.String(deposit.Batch) + "',"
                                           + SOut.Long(deposit.DepositAccountNum) + ","
                                           + SOut.Bool(deposit.IsSentToQuickBooksOnline) + ")";
        if (deposit.BankAccountInfo == null) deposit.BankAccountInfo = "";
        var paramBankAccountInfo = new OdSqlParameter("paramBankAccountInfo", OdDbType.Text, SOut.StringParam(deposit.BankAccountInfo));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramBankAccountInfo);
        else
            deposit.DepositNum = Db.NonQ(command, true, "DepositNum", "deposit", paramBankAccountInfo);
        return deposit.DepositNum;
    }

    public static void Update(Deposit deposit)
    {
        var command = "UPDATE deposit SET "
                      + "DateDeposit             =  " + SOut.Date(deposit.DateDeposit) + ", "
                      + "BankAccountInfo         =  " + DbHelper.ParamChar + "paramBankAccountInfo, "
                      + "Amount                  =  " + SOut.Double(deposit.Amount) + ", "
                      + "Memo                    = '" + SOut.String(deposit.Memo) + "', "
                      + "Batch                   = '" + SOut.String(deposit.Batch) + "', "
                      + "DepositAccountNum       =  " + SOut.Long(deposit.DepositAccountNum) + ", "
                      + "IsSentToQuickBooksOnline=  " + SOut.Bool(deposit.IsSentToQuickBooksOnline) + " "
                      + "WHERE DepositNum = " + SOut.Long(deposit.DepositNum);
        if (deposit.BankAccountInfo == null) deposit.BankAccountInfo = "";
        var paramBankAccountInfo = new OdSqlParameter("paramBankAccountInfo", OdDbType.Text, SOut.StringParam(deposit.BankAccountInfo));
        Db.NonQ(command, paramBankAccountInfo);
    }

    public static bool Update(Deposit deposit, Deposit oldDeposit)
    {
        var command = "";
        if (deposit.DateDeposit.Date != oldDeposit.DateDeposit.Date)
        {
            if (command != "") command += ",";
            command += "DateDeposit = " + SOut.Date(deposit.DateDeposit) + "";
        }

        if (deposit.BankAccountInfo != oldDeposit.BankAccountInfo)
        {
            if (command != "") command += ",";
            command += "BankAccountInfo = " + DbHelper.ParamChar + "paramBankAccountInfo";
        }

        if (deposit.Amount != oldDeposit.Amount)
        {
            if (command != "") command += ",";
            command += "Amount = " + SOut.Double(deposit.Amount) + "";
        }

        if (deposit.Memo != oldDeposit.Memo)
        {
            if (command != "") command += ",";
            command += "Memo = '" + SOut.String(deposit.Memo) + "'";
        }

        if (deposit.Batch != oldDeposit.Batch)
        {
            if (command != "") command += ",";
            command += "Batch = '" + SOut.String(deposit.Batch) + "'";
        }

        if (deposit.DepositAccountNum != oldDeposit.DepositAccountNum)
        {
            if (command != "") command += ",";
            command += "DepositAccountNum = " + SOut.Long(deposit.DepositAccountNum) + "";
        }

        if (deposit.IsSentToQuickBooksOnline != oldDeposit.IsSentToQuickBooksOnline)
        {
            if (command != "") command += ",";
            command += "IsSentToQuickBooksOnline = " + SOut.Bool(deposit.IsSentToQuickBooksOnline) + "";
        }

        if (command == "") return false;
        if (deposit.BankAccountInfo == null) deposit.BankAccountInfo = "";
        var paramBankAccountInfo = new OdSqlParameter("paramBankAccountInfo", OdDbType.Text, SOut.StringParam(deposit.BankAccountInfo));
        command = "UPDATE deposit SET " + command
                                        + " WHERE DepositNum = " + SOut.Long(deposit.DepositNum);
        Db.NonQ(command, paramBankAccountInfo);
        return true;
    }

    public static bool UpdateComparison(Deposit deposit, Deposit oldDeposit)
    {
        if (deposit.DateDeposit.Date != oldDeposit.DateDeposit.Date) return true;
        if (deposit.BankAccountInfo != oldDeposit.BankAccountInfo) return true;
        if (deposit.Amount != oldDeposit.Amount) return true;
        if (deposit.Memo != oldDeposit.Memo) return true;
        if (deposit.Batch != oldDeposit.Batch) return true;
        if (deposit.DepositAccountNum != oldDeposit.DepositAccountNum) return true;
        if (deposit.IsSentToQuickBooksOnline != oldDeposit.IsSentToQuickBooksOnline) return true;
        return false;
    }

    public static void Delete(long depositNum)
    {
        var command = "DELETE FROM deposit "
                      + "WHERE DepositNum = " + SOut.Long(depositNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDepositNums)
    {
        if (listDepositNums == null || listDepositNums.Count == 0) return;
        var command = "DELETE FROM deposit "
                      + "WHERE DepositNum IN(" + string.Join(",", listDepositNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}