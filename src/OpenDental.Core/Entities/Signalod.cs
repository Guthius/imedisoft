using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Signalod : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long SignalNum;

    ///<summary>If IType=Date, then this is the affected date in the Appointments module.</summary>
    public DateTime DateViewing;

    ///<summary>The exact server time when this signal was entered into db.
    ///This does not need to be set by sender since it's handled automatically.</summary>
    public DateTime SigDateTime;

    ///<summary>Usually identifies the object that was edited to cause the signal to be created.
    ///Can be used for special scenarios based on the FKeyType.  E.g. for SmsMsgUnreadCount, this represents a count, not an FK.</summary>
    public long FKey;

    ///<summary>Enum:KeyType Describes the type of object referenced by the FKey.</summary>
    public KeyType FKeyType;

    ///<summary>Enum:InvalidType Indicates what cache or entity has been changed.</summary>
    public InvalidType IType;

    ///<summary>Enum:MiddleTierRole The MiddleTierRole of the instance that created this signal.</summary>
    public int RemoteRole;

    ///<summary>Message value of the signal.</summary>
    public string MsgValue;

    public override bool Equals(object obj)
    {
        if (obj is not Signalod signalOther)
        {
            return false;
        }

        if (SignalNum != signalOther.SignalNum)
        {
            return false;
        }

        if (DateViewing != signalOther.DateViewing)
        {
            return false;
        }

        if (SigDateTime != signalOther.SigDateTime)
        {
            return false;
        }

        if (FKey != signalOther.FKey)
        {
            return false;
        }

        if (FKeyType != signalOther.FKeyType)
        {
            return false;
        }

        if (IType != signalOther.IType)
        {
            return false;
        }

        if (RemoteRole != signalOther.RemoteRole)
        {
            return false;
        }

        if (MsgValue != signalOther.MsgValue)
        {
            return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        return SignalNum.GetHashCode();
    }
}

///<summary>Do not combine with SignalType, they must be seperate. Stored as string, safe to reorder enum values.</summary>
public enum KeyType
{
    ///<summary>Probably represented by empty string, but possibly by "Undefined".</summary>
    Undefined,

    FeeSched,

    Job,

    Operatory,

    ///<summary>HQ only.  FKey will be the extension of the corresponding phone that is invalid.
    ///Specifically used to talk to the PhoneTrackingServer in order to let it know that an extension has changed (e.g. queue change).</summary>
    PhoneExtension,

    Provider,

    SigMessage,

    ///<summary>Special KeyType that does not use a FK but instead will set FKey to a count of unread messages.
    ///Used along side the SmsTextMsgReceivedUnreadCount InvalidType.</summary>
    SmsMsgUnreadCount,

    Task,

    ///<summary>Used to identify which signals a form can ignore.  If the FKey==Process.GetCurrentProcess().Id then this process sent it so ignore
    ///it.  Used in FormTerminal, FormTerminalManager, and FormSheetFillEdit (for forms being filled at a kiosk).</summary>
    ProcessId,

    ///<summary>Used to notify the phone tracking server to kick all users out of a conference room.</summary>
    ConfKick,

    ///<summary>Used in AccModule, TPModule, and PerioExams.</summary>
    PatNum,

    ///<summary>Indicates Signalod pertains to a specific UserOd.</summary>
    UserOd,

    ///<summary>Used to indicate that this specific email address is what this signal is for</summary>
    EmailAddress,

    ///<summary>This is HQ specific and will be used to listen in on live calls</summary>
    ChanSpy,

    ///<summary>Used to speficy that the passed-in computerNum is what the signal is for.</summary>
    Computer
}