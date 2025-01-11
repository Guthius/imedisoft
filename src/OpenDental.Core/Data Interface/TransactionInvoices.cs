using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class TransactionInvoices
{
    
    public static TransactionInvoice GetOne(long transactionInvoiceNum)
    {
        var command = "SELECT * FROM transactioninvoice WHERE TransactionInvoiceNum = " + SOut.Long(transactionInvoiceNum);
        return TransactionInvoiceCrud.SelectOne(command);
    }

    /// <summary>
    ///     Used only to get the name of the file, so we're not querying the entire document data (which could be multiple
    ///     megabytes).
    /// </summary>
    public static string GetName(long transactionInvoiceNum)
    {
        var command = "SELECT FileName FROM transactioninvoice WHERE TransactionInvoiceNum = " + SOut.Long(transactionInvoiceNum);
        return DataCore.GetScalar(command);
    }

    
    public static long Insert(TransactionInvoice transactionInvoice)
    {
        return TransactionInvoiceCrud.Insert(transactionInvoice);
    }

    
    public static void Delete(long transactionInvoiceNum)
    {
        TransactionInvoiceCrud.Delete(transactionInvoiceNum);
    }
}