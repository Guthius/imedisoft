#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class TransactionCrud
{
    public static Transaction SelectOne(long transactionNum)
    {
        var command = "SELECT * FROM transaction "
                      + "WHERE TransactionNum = " + SOut.Long(transactionNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Transaction SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Transaction> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Transaction> TableToList(DataTable table)
    {
        var retVal = new List<Transaction>();
        Transaction transaction;
        foreach (DataRow row in table.Rows)
        {
            transaction = new Transaction();
            transaction.TransactionNum = SIn.Long(row["TransactionNum"].ToString());
            transaction.DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString());
            transaction.UserNum = SIn.Long(row["UserNum"].ToString());
            transaction.DepositNum = SIn.Long(row["DepositNum"].ToString());
            transaction.PayNum = SIn.Long(row["PayNum"].ToString());
            transaction.SecUserNumEdit = SIn.Long(row["SecUserNumEdit"].ToString());
            transaction.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            transaction.TransactionInvoiceNum = SIn.Long(row["TransactionInvoiceNum"].ToString());
            retVal.Add(transaction);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Transaction> listTransactions, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Transaction";
        var table = new DataTable(tableName);
        table.Columns.Add("TransactionNum");
        table.Columns.Add("DateTimeEntry");
        table.Columns.Add("UserNum");
        table.Columns.Add("DepositNum");
        table.Columns.Add("PayNum");
        table.Columns.Add("SecUserNumEdit");
        table.Columns.Add("SecDateTEdit");
        table.Columns.Add("TransactionInvoiceNum");
        foreach (var transaction in listTransactions)
            table.Rows.Add(SOut.Long(transaction.TransactionNum), SOut.DateT(transaction.DateTimeEntry, false), SOut.Long(transaction.UserNum), SOut.Long(transaction.DepositNum), SOut.Long(transaction.PayNum), SOut.Long(transaction.SecUserNumEdit), SOut.DateT(transaction.SecDateTEdit, false), SOut.Long(transaction.TransactionInvoiceNum));
        return table;
    }

    public static long Insert(Transaction transaction)
    {
        return Insert(transaction, false);
    }

    public static long Insert(Transaction transaction, bool useExistingPK)
    {
        var command = "INSERT INTO transaction (";

        command += "DateTimeEntry,UserNum,DepositNum,PayNum,SecUserNumEdit,TransactionInvoiceNum) VALUES(";

        command +=
            DbHelper.Now() + ","
                           + SOut.Long(transaction.UserNum) + ","
                           + SOut.Long(transaction.DepositNum) + ","
                           + SOut.Long(transaction.PayNum) + ","
                           + SOut.Long(transaction.SecUserNumEdit) + ","
                           //SecDateTEdit can only be set by MySQL
                           + SOut.Long(transaction.TransactionInvoiceNum) + ")";
        {
            transaction.TransactionNum = Db.NonQ(command, true, "TransactionNum", "transaction");
        }
        return transaction.TransactionNum;
    }

    public static long InsertNoCache(Transaction transaction)
    {
        return InsertNoCache(transaction, false);
    }

    public static long InsertNoCache(Transaction transaction, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO transaction (";
        if (isRandomKeys || useExistingPK) command += "TransactionNum,";
        command += "DateTimeEntry,UserNum,DepositNum,PayNum,SecUserNumEdit,TransactionInvoiceNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(transaction.TransactionNum) + ",";
        command +=
            DbHelper.Now() + ","
                           + SOut.Long(transaction.UserNum) + ","
                           + SOut.Long(transaction.DepositNum) + ","
                           + SOut.Long(transaction.PayNum) + ","
                           + SOut.Long(transaction.SecUserNumEdit) + ","
                           //SecDateTEdit can only be set by MySQL
                           + SOut.Long(transaction.TransactionInvoiceNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            transaction.TransactionNum = Db.NonQ(command, true, "TransactionNum", "transaction");
        return transaction.TransactionNum;
    }

    public static void Update(Transaction transaction)
    {
        var command = "UPDATE transaction SET "
                      //DateTimeEntry not allowed to change
                      //UserNum excluded from update
                      + "DepositNum           =  " + SOut.Long(transaction.DepositNum) + ", "
                      + "PayNum               =  " + SOut.Long(transaction.PayNum) + ", "
                      + "SecUserNumEdit       =  " + SOut.Long(transaction.SecUserNumEdit) + ", "
                      //SecDateTEdit can only be set by MySQL
                      + "TransactionInvoiceNum=  " + SOut.Long(transaction.TransactionInvoiceNum) + " "
                      + "WHERE TransactionNum = " + SOut.Long(transaction.TransactionNum);
        Db.NonQ(command);
    }

    public static bool Update(Transaction transaction, Transaction oldTransaction)
    {
        var command = "";
        //DateTimeEntry not allowed to change
        //UserNum excluded from update
        if (transaction.DepositNum != oldTransaction.DepositNum)
        {
            if (command != "") command += ",";
            command += "DepositNum = " + SOut.Long(transaction.DepositNum) + "";
        }

        if (transaction.PayNum != oldTransaction.PayNum)
        {
            if (command != "") command += ",";
            command += "PayNum = " + SOut.Long(transaction.PayNum) + "";
        }

        if (transaction.SecUserNumEdit != oldTransaction.SecUserNumEdit)
        {
            if (command != "") command += ",";
            command += "SecUserNumEdit = " + SOut.Long(transaction.SecUserNumEdit) + "";
        }

        //SecDateTEdit can only be set by MySQL
        if (transaction.TransactionInvoiceNum != oldTransaction.TransactionInvoiceNum)
        {
            if (command != "") command += ",";
            command += "TransactionInvoiceNum = " + SOut.Long(transaction.TransactionInvoiceNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE transaction SET " + command
                                            + " WHERE TransactionNum = " + SOut.Long(transaction.TransactionNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Transaction transaction, Transaction oldTransaction)
    {
        //DateTimeEntry not allowed to change
        //UserNum excluded from update
        if (transaction.DepositNum != oldTransaction.DepositNum) return true;
        if (transaction.PayNum != oldTransaction.PayNum) return true;
        if (transaction.SecUserNumEdit != oldTransaction.SecUserNumEdit) return true;
        //SecDateTEdit can only be set by MySQL
        if (transaction.TransactionInvoiceNum != oldTransaction.TransactionInvoiceNum) return true;
        return false;
    }

    public static void Delete(long transactionNum)
    {
        var command = "DELETE FROM transaction "
                      + "WHERE TransactionNum = " + SOut.Long(transactionNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listTransactionNums)
    {
        if (listTransactionNums == null || listTransactionNums.Count == 0) return;
        var command = "DELETE FROM transaction "
                      + "WHERE TransactionNum IN(" + string.Join(",", listTransactionNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}