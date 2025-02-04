namespace Imedisoft.Core.Entities;

///<summary>Used by both Statement and WebSchedRecall (and probably other places).</summary>
public enum AutoCommStatus
{
    ///<summary>0 - Should not be in the database but can be used in the program.</summary>
    Undefined,

    ///<summary>1 - Do not send a reminder.</summary>
    DoNotSend,

    ///<summary>2 - Has not been attempted to send yet.</summary>
    SendNotAttempted,

    ///<summary>3 - Has been sent successfully.</summary>
    SendSuccessful,

    ///<summary>4 - Attempted to send but not successful.</summary>
    SendFailed,

    ///<summary>5 - Has been sent successfully, awaiting receipt.</summary>
    SentAwaitingReceipt
}