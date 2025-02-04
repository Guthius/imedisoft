using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

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

    public static List<Transaction> TableToList(DataTable table)
    {
        var retVal = new List<Transaction>();
        foreach (DataRow row in table.Rows)
        {
            var transaction = new Transaction
            {
                TransactionNum = SIn.Long(row["TransactionNum"].ToString()),
                DateTimeEntry = SIn.DateTime(row["DateTimeEntry"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString()),
                DepositNum = SIn.Long(row["DepositNum"].ToString()),
                PayNum = SIn.Long(row["PayNum"].ToString()),
                SecUserNumEdit = SIn.Long(row["SecUserNumEdit"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString()),
                TransactionInvoiceNum = SIn.Long(row["TransactionInvoiceNum"].ToString())
            };
            retVal.Add(transaction);
        }

        return retVal;
    }

    public static void Insert(Transaction transaction)
    {
        var command = "INSERT INTO transaction (";

        command += "DateTimeEntry,UserNum,DepositNum,PayNum,SecUserNumEdit,TransactionInvoiceNum) VALUES(";

        command +=
            "NOW()" + ","
                    + SOut.Long(transaction.UserNum) + ","
                    + SOut.Long(transaction.DepositNum) + ","
                    + SOut.Long(transaction.PayNum) + ","
                    + SOut.Long(transaction.SecUserNumEdit) + ","
                    //SecDateTEdit can only be set by MySQL
                    + SOut.Long(transaction.TransactionInvoiceNum) + ")";
        {
            transaction.TransactionNum = Db.NonQ(command, true, "TransactionNum", "transaction");
        }
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
}