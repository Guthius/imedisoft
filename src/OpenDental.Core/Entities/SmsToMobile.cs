using System;
using System.ComponentModel;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class SmsToMobile : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long SmsToMobileNum;

    ///<summary>FK to patient.PatNum</summary>
    public long PatNum;

    ///<summary>GUID. Uniquely identifies this message and is used for tracking message status.</summary>
    public string GuidMessage;

    ///<summary>GUID. When sending batch messages, all messages will have the same batch GUID that should be the GUID of the first message within the batch.</summary>
    public string GuidBatch;

    ///<summary>This is the sending phone number in international format. Each office may have several different numbers that they use.</summary>
    public string SmsPhoneNumber;

    ///<summary>The phone number that this message was sent to. Must be kept in addition to the PatNum.</summary>
    public string MobilePhoneNumber;

    ///<summary>Set to true if this message should "jump the queue" and be sent asap.</summary>
    public bool IsTimeSensitive;

    ///<summary>Enum:SmsMessageSource  This is used to identify where in the program this message originated from.</summary>
    public SmsMessageSource MsgType;

    ///<summary>The contents of the message.</summary>
    public string MsgText;

    ///<summary>Enum:SmsDeliveryStatus  Set by the Listener, tracks status of SMS.</summary>
    public SmsDeliveryStatus SmsStatus;

    ///<summary>The count of parts that this message will be broken into when sent.
    ///A single long message will be broken into several smaller 153 utf8 or 70 unicode character messages.</summary>
    public int MsgParts;

    ///<summary>The amount charged to the customer. Total cost for this message always stored in US Dollars.</summary>
    public float MsgChargeUSD;

    ///<summary>FK to clinic.ClinicNum.  0 when not using clinics.</summary>
    public long ClinicNum;

    ///<summary>Only used when SmsDeliveryStatus==Failed.</summary>
    public string CustErrorText;

    ///<summary>Time message was accepted at ODHQ.</summary>
    public DateTime DateTimeSent;

    ///<summary>Date time that the message was either successfully delivered or failed.</summary>
    public DateTime DateTimeTerminated;

    public bool IsHidden;

    ///<summary>Any discount applied to this message. 
    ///If a particular messages has a MsgDiscountUSD > 0  then the MsgChargeUSD will reflect the charge to the customer after the discount has already been applied. 
    ///Multi-part messages will still be charged the wholesale rate for all parts after the first part.
    ///To calculate the typical charge that this customer would pay without the discount use MsgChargeUSD + MsgDiscountUSD.
    ///To calculate the percentage discounted off standard charges use (MsgDiscountUSD / (MsgChargeUSD + MsgDiscountUSD)).</summary>
    public float MsgDiscountUSD;

    public SmsToMobile Copy()
    {
        return (SmsToMobile) MemberwiseClone();
    }
}

public enum SmsMessageSource
{
    ///<summary>1. This should be used for one-off messages that might be sent as direct communication with patient.
    ///Short Code Supported: NO
    ///</summary>
    [Description("Manual")]
    DirectSms = 1,

    ///<summary>5. Used when sending confirmations.
    ///Short Code Supported: YES
    ///</summary>
    Confirmation = 5,

    ///<summary>8. Used when sending single or batch SMS from the clicking the Text button on the ASAP window.
    ///Short Code Supported: YES
    ///</summary>
    [Description("ASAP Manual")]
    AsapManual = 8,

    ///<summary>11. Sending an SMS to let the patient know that a statement is available.
    ///Short Code Supported: YES
    ///</summary>
    [Description("Statements")]
    Statements = 11,

    ///<summary>22. Used to texting patients about appointment arrival instructions.</summary>
    Arrival = 22,

    ///<summary>27. Used for Payment Portal Msg-To-Pay messages.
    ///Short Code Supported: NO</summary>
    MsgToPay = 27,

    ///<summary>28. Used when the office checks NO for the patient to receive texts in the patient edit form.
    ///Short Code Supported: NO
    ///</summary>
    OptOutReply = 28
}

///<summary>None should never be used, the code should be re-written to not use it.</summary>
public enum SmsDeliveryStatus
{
    ///<summary>0. Should not be used.</summary>
    None,

    ///<summary>1. After a message has been accepted at ODHQ. Before any feedback.</summary>
    Pending,

    ///<summary>2. Delivered to customer, carrier replied with confirmation.</summary>
    DeliveryConf,

    ///<summary>3. Delivered to customer, no confirmation of failure or delivery sent back from carrier.</summary>
    DeliveryUnconf,

    ///<summary>4. Attempted delivery, failure message return after arriving at handset.</summary>
    FailWithCharge,

    ///<summary>5. Attempted delivery, immediate failure confirmation received from carrier.</summary>
    FailNoCharge
}