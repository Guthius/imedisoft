using System;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Transactions
{
    ///<summary>Since transactions are always viewed individually, this function returns one transaction</summary>
    public static Transaction GetTrans(long transactionNum)
    {
        return TransactionCrud.SelectOne(transactionNum);
    }

    /// <summary>
    ///     Gets one transaction directly from the database which has this deposit attached to it.  If none exist, then
    ///     returns null.
    /// </summary>
    public static Transaction GetAttachedToDeposit(long depositNum)
    {
        var command =
            "SELECT * FROM transaction "
            + "WHERE DepositNum=" + SOut.Long(depositNum);
        return TransactionCrud.SelectOne(command);
    }

    /// <summary>
    ///     Gets one transaction directly from the database which has this payment attached to it.  If none exist, then
    ///     returns null.  There should never be more than one, so that's why it doesn't return more than one.
    /// </summary>
    public static Transaction GetAttachedToPayment(long payNum)
    {
        var command =
            "SELECT * FROM transaction "
            + "WHERE PayNum=" + SOut.Long(payNum);
        return TransactionCrud.SelectOne(command);
    }

    
    public static long Insert(Transaction transaction)
    {
        transaction.SecUserNumEdit = Security.CurUser.UserNum; //Before middle tier check to catch user at workstation

        return TransactionCrud.Insert(transaction);
    }

    
    public static void Update(Transaction transaction)
    {
        transaction.SecUserNumEdit = Security.CurUser.UserNum; //Before middle tier check to catch user at workstation

        TransactionCrud.Update(transaction);
    }

    
    public static void UpdateInvoiceNum(long transactionNum, long transactionInvoiceNum)
    {
        var command = "UPDATE transaction SET TransactionInvoiceNum=" + SOut.Long(transactionInvoiceNum)
                                                                      + " WHERE TransactionNum=" + SOut.Long(transactionNum);
        Db.NonQ(command);
    }

    /// <summary>
    ///     Also deletes all journal entries for the transaction.  Will later throw an error if journal entries attached
    ///     to any reconciles.  Be sure to surround with try-catch.
    /// </summary>
    public static void Delete(Transaction transaction)
    {
        if (IsTransactionLocked(transaction.TransactionNum)) throw new ApplicationException(Lans.g("Transactions", "Not allowed to delete transactions because it is attached to a reconcile that is locked."));

        var command = "DELETE FROM journalentry WHERE TransactionNum=" + SOut.Long(transaction.TransactionNum);
        Db.NonQ(command);
        if (transaction.TransactionInvoiceNum != 0)
        {
            command = "DELETE FROM transactioninvoice WHERE TransactionInvoiceNum=" + SOut.Long(transaction.TransactionInvoiceNum);
            Db.NonQ(command);
        }

        command = "DELETE FROM transaction WHERE TransactionNum=" + SOut.Long(transaction.TransactionNum);
        Db.NonQ(command);
    }

    private static bool IsTransactionLocked(long transactionNum)
    {
        var command = "SELECT IsLocked FROM journalentry j, reconcile r WHERE j.TransactionNum=" + SOut.Long(transactionNum)
                                                                                                 + " AND j.ReconcileNum = r.ReconcileNum";
        var table = DataCore.GetTable(command);
        if (table.Rows.Count > 0 && SIn.Int(table.Rows[0][0].ToString()) == 1) return true;

        return false;
    }

    public static bool IsAttachedToLockedReconcile(Transaction transaction)
    {
        return IsTransactionLocked(transaction.TransactionNum);
    }

    
    public static bool IsReconciled(Transaction transaction)
    {
        var command = "SELECT COUNT(*) FROM journalentry WHERE ReconcileNum !=0"
                      + " AND TransactionNum=" + SOut.Long(transaction.TransactionNum);
        if (Db.GetCount(command) == "0") return false;

        return true;
    }
}