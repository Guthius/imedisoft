using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Transaction : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long TransactionNum;

    ///<summary>Not user editable.  Server time.</summary>
    public DateTime DateTimeEntry;

    ///<summary>FK to userod.UserNum. The user that entered this transaction.</summary>
    public long UserNum;

    ///<summary>FK to deposit.DepositNum.  Will eventually be replaced by a source document table, and deposits will just be one of many types.</summary>
    public long DepositNum;

    ///<summary>FK to payment.PayNum.  Like DepositNum, it will eventually be replaced by a source document table, and payments will just be one of many types.</summary>
    public long PayNum;

    ///<summary>FK to userod.UserNum. The user who last edited this transaction.</summary>
    public long SecUserNumEdit;

    ///<summary>The last time this transaction was edited.</summary>
    public DateTime SecDateTEdit;

    ///<summary>FK to transactioninvoice.TransactionInvoiceNum.  A document that can be attached to the transaction.</summary>
    public long TransactionInvoiceNum;
}