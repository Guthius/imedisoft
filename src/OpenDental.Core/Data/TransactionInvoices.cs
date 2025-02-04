using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class TransactionInvoices
{
    public static TransactionInvoice GetOne(long transactionInvoiceNum)
    {
        return TransactionInvoiceCrud.SelectOne("SELECT * FROM transactioninvoice WHERE TransactionInvoiceNum = " + transactionInvoiceNum);
    }

    public static string GetName(long transactionInvoiceNum)
    {
        return DataCore.GetScalar("SELECT FileName FROM transactioninvoice WHERE TransactionInvoiceNum = " + transactionInvoiceNum);
    }

    public static void Insert(TransactionInvoice transactionInvoice)
    {
        TransactionInvoiceCrud.Insert(transactionInvoice);
    }

    public static void Delete(long transactionInvoiceNum)
    {
        TransactionInvoiceCrud.Delete(transactionInvoiceNum);
    }
}