using System;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Transactions
{
    public static Transaction GetTrans(long transactionNum)
    {
        return TransactionCrud.SelectOne(transactionNum);
    }

    public static Transaction GetAttachedToDeposit(long depositNum)
    {
        return TransactionCrud.SelectOne("SELECT * FROM transaction WHERE DepositNum = " + depositNum);
    }

    public static Transaction GetAttachedToPayment(long payNum)
    {
        return TransactionCrud.SelectOne("SELECT * FROM transaction WHERE PayNum = " + payNum);
    }

    public static void Insert(Transaction transaction)
    {
        transaction.SecUserNumEdit = Security.CurUser.UserNum;

        TransactionCrud.Insert(transaction);
    }

    public static void Update(Transaction transaction)
    {
        transaction.SecUserNumEdit = Security.CurUser.UserNum;

        TransactionCrud.Update(transaction);
    }

    public static void UpdateInvoiceNum(long transactionNum, long transactionInvoiceNum)
    {
        Db.NonQ("UPDATE transaction SET TransactionInvoiceNum=" + transactionInvoiceNum + " WHERE TransactionNum=" + transactionNum);
    }

    public static void Delete(Transaction transaction)
    {
        if (IsTransactionLocked(transaction.TransactionNum))
        {
            throw new ApplicationException("Not allowed to delete transactions because it is attached to a reconcile that is locked.");
        }

        Db.NonQ("DELETE FROM journalentry WHERE TransactionNum = " + transaction.TransactionNum);

        if (transaction.TransactionInvoiceNum != 0)
        {
            Db.NonQ("DELETE FROM transactioninvoice WHERE TransactionInvoiceNum = " + transaction.TransactionInvoiceNum);
        }

        Db.NonQ("DELETE FROM transaction WHERE TransactionNum = " + transaction.TransactionNum);
    }

    private static bool IsTransactionLocked(long transactionNum)
    {
        var dataTable = DataCore.GetTable("SELECT IsLocked FROM journalentry j, reconcile r WHERE j.TransactionNum = " + transactionNum + " AND j.ReconcileNum = r.ReconcileNum");

        return dataTable.Rows.Count > 0 && SIn.Int(dataTable.Rows[0][0].ToString()) == 1;
    }

    public static bool IsAttachedToLockedReconcile(Transaction transaction)
    {
        return IsTransactionLocked(transaction.TransactionNum);
    }

    public static bool IsReconciled(Transaction transaction)
    {
        return Db.GetCount("SELECT COUNT(*) FROM journalentry WHERE ReconcileNum !=0  AND TransactionNum = " + transaction.TransactionNum) != "0";
    }
}