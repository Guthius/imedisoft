using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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

    public static List<Deposit> TableToList(DataTable table)
    {
        var retVal = new List<Deposit>();
        foreach (DataRow row in table.Rows)
        {
            var deposit = new Deposit
            {
                DepositNum = SIn.Long(row["DepositNum"].ToString()),
                DateDeposit = SIn.Date(row["DateDeposit"].ToString()),
                BankAccountInfo = SIn.String(row["BankAccountInfo"].ToString()),
                Amount = SIn.Double(row["Amount"].ToString()),
                Memo = SIn.String(row["Memo"].ToString()),
                Batch = SIn.String(row["Batch"].ToString()),
                DepositAccountNum = SIn.Long(row["DepositAccountNum"].ToString()),
                IsSentToQuickBooksOnline = SIn.Bool(row["IsSentToQuickBooksOnline"].ToString())
            };
            retVal.Add(deposit);
        }

        return retVal;
    }

    public static long Insert(Deposit deposit)
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
        var paramBankAccountInfo = new OdSqlParameter("paramBankAccountInfo", SOut.StringParam(deposit.BankAccountInfo));
        {
            deposit.DepositNum = Db.NonQ(command, true, "DepositNum", "deposit", paramBankAccountInfo);
        }
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
        var paramBankAccountInfo = new OdSqlParameter("paramBankAccountInfo", SOut.StringParam(deposit.BankAccountInfo));
        Db.NonQ(command, paramBankAccountInfo);
    }

    public static void Update(Deposit deposit, Deposit oldDeposit)
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

        if (command == "") return;
        if (deposit.BankAccountInfo == null) deposit.BankAccountInfo = "";
        var paramBankAccountInfo = new OdSqlParameter("paramBankAccountInfo", SOut.StringParam(deposit.BankAccountInfo));
        command = "UPDATE deposit SET " + command
                                        + " WHERE DepositNum = " + SOut.Long(deposit.DepositNum);
        Db.NonQ(command, paramBankAccountInfo);
    }

    public static void Delete(long depositNum)
    {
        var command = "DELETE FROM deposit WHERE DepositNum = " + SOut.Long(depositNum);
        Db.NonQ(command);
    }
}