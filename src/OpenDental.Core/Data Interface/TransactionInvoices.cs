using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class TransactionInvoices
{
    public static TransactionInvoice GetOne(long transactionInvoiceNum)
    {
        var command = "SELECT * FROM transactioninvoice WHERE TransactionInvoiceNum = " + SOut.Long(transactionInvoiceNum);
        return TransactionInvoiceCrud.SelectOne(command);
    }

    public static string GetName(long transactionInvoiceNum)
    {
        var command = "SELECT FileName FROM transactioninvoice WHERE TransactionInvoiceNum = " + SOut.Long(transactionInvoiceNum);
        return DataCore.GetScalar(command);
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