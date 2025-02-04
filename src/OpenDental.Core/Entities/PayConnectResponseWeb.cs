using System;
using OpenDentBusiness;
using OpenDentBusiness.PayConnectService;

namespace Imedisoft.Core.Entities;

public class PayConnectResponseWeb : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PayConnectResponseWebNum;

    ///<summary>FK to patient.PatNum.</summary>
    public long PatNum;

    ///<summary>FK to payment.PayNum.</summary>
    public long PayNum;

    public CreditCardSource CCSource;

    ///<summary>The amount of the payment that is attempting to be made.</summary>
    public double Amount;

    ///<summary>The note entered when making a payment.</summary>
    public string PayNote;

    ///<summary>The account token used to poll the processing status.</summary>
    public string AccountToken;

    ///<summary>The payment token used to poll the processing status.</summary>
    public string PayToken;

    ///<summary>Enum:PayConnectWebStatus Used to determine if the payment is pending, needs action, or is completed and attached to a payment.</summary>
    public PayConnectWebStatus ProcessingStatus;

    ///<summary>Timestamp automatically generated and user not allowed to change.  The actual datetime of entry.</summary>
    public DateTime DateTimeEntry;

    ///<summary>DateTime that the payment went to the pending status.</summary>
    public DateTime DateTimePending;

    ///<summary>DateTime that the payment went to the completed status and is attached to a payment.</summary>
    public DateTime DateTimeCompleted;

    ///<summary>DateTime that the payment opportunity time expired.</summary>
    public DateTime DateTimeExpired;

    ///<summary>DateTime of the last time that the payment had an error.</summary>
    public DateTime DateTimeLastError;

    ///<summary>Raw JSON response (or error) from PayConnect.</summary>
    public string LastResponseStr;

    ///<summary>Whether or not the credit card token can be saved for future uses.</summary>
    public bool IsTokenSaved;

    ///<summary>The payment token used for future payments.</summary>
    public string PaymentToken;

    ///<summary>Provides the Expiration Date of the account being accessed. Format is yyMM from XWeb gateway. Will be converted to ExpirationDate.</summary>
    public string ExpDateToken;

    ///<summary>The RefNumber associated to this transaction.  Will only be set for Completed PayConnectWebStatuses.</summary>
    public string RefNumber;

    ///<summary>The Transaction Type associated to this transaction.  Will only be set for Completed PayConnectWebStatuses.</summary>
    public transType TransType;

    ///<summary>Email address used for a requested receipt provided by the user when making a payment via the patient portal.</summary>
    public string EmailResponse;

    ///<summary>The GUID used in EserviceLogs related to this response. May be blank.</summary>
    public string LogGuid;

    public bool IsFromWebPortal => !string.IsNullOrWhiteSpace(AccountToken) && !string.IsNullOrWhiteSpace(LastResponseStr);
}

public enum PayConnectWebStatus
{
    Created,
    CreatedError,
    Pending,
    PendingError,
    Expired,
    Completed,
    Cancelled,
    Declined,
    Unknown,
    UnknownError
}