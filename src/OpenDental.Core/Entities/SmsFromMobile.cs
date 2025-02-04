using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class SmsFromMobile : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long SmsFromMobileNum;

    ///<summary>FK to patient.PatNum. Not sent from HQ.</summary>
    public long PatNum;

    ///<summary>FK to clinic.ClinicNum. </summary>
    public long ClinicNum;

    ///<summary>FK to commlog.CommlogNum. Not sent from HQ.</summary>
    public long CommlogNum;

    ///<summary>Contents of the message.</summary>
    public string MsgText;

    ///<summary>Date and time message was inserted into the DB. Not sent from HQ.</summary>
    public DateTime DateTimeReceived;

    ///<summary>This is the Phone Number of the office that the mobile device sent a message to.</summary>
    public string SmsPhoneNumber;

    ///<summary>This is the PhoneNumber that this message was sent from.</summary>
    public string MobilePhoneNumber;

    ///<summary>Message part sequence number. For single part messages this should always be 1. 
    ///For messages that exist as multiple parts, due to staggered delivery of the parts, this will be a number between 1 and MsgTotal.</summary>
    public int MsgPart;

    ///<summary>Total count of message parts for this single message identified by MsgRefID.
    ///For single part messages this should always be 1.</summary>
    public int MsgTotal;

    ///<summary>Each part of a multipart message will have the same MsgRefID.</summary>
    public string MsgRefID;

    ///<summary>Enum:SmsFromStatus .</summary>
    public SmsFromStatus SmsStatus;

    ///<summary>Words surrounded by spaces, flags should be all lower case. This allows simple querrying. Example: " junk  recall " allows you to 
    ///write "WHERE Flags like "% junk %" without having to worry about commas. Also, adding and removing tags is easier. Example: Flags=Flags.Replace(" junk ","");</summary>
    public string Flags;

    public bool IsHidden;

    public int MatchCount;

    ///<summary>FK to confirmationrequest.GuidMessageFromMobile. Generated at HQ when the confirmation pending is terminated with confirmation text message.</summary>
    public string GuidMessage;

    ///<summary>Automatically updated by MySQL every time a row is added or changed. Could be changed due to user editing, custom queries or program
    ///updates.  Not user editable with the UI.</summary>
    public DateTime SecDateTEdit;

    ///<summary>Indicates that this inbound message was in response to a pending confirmation request. 
    ///Non-db column as this is only needed in memory for a short time by eConnector. 
    ///Determining the flag later can be done with a query and a join on ConfirmationRequest.GuidMessageFromMobile.</summary>
    [CrudColumn(IsNotDbColumn = true)]
    public bool IsConfirmationResponse;

    public SmsFromMobile Copy()
    {
        return (SmsFromMobile) MemberwiseClone();
    }
}

public enum SmsFromStatus
{
    ReceivedUnread,
    ReceivedRead
}