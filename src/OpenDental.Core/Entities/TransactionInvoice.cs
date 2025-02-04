using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class TransactionInvoice : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long TransactionInvoiceNum;

    ///<summary>File name including the extension.</summary>
    public string FileName;

    ///<summary>The raw file data converted to base64. Will be blank when using FilePath.</summary>
    public string InvoiceData;

    ///<summary>Full file path. Will be blank when using InvoiceData.</summary>
    public string FilePath;
}