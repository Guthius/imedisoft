using System;
using System.ComponentModel;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Commlog : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long CommlogNum;

    ///<summary>FK to patient.PatNum. This will be 0 if Referral.</summary>
    public long PatNum;

    ///<summary>Date and time of entry</summary>
    public DateTime CommDateTime;

    ///<summary>FK to definition.DefNum. This will be 0 if Referral.</summary>
    public long CommType;

    public string Note;

    ///<summary>Enum:CommItemMode Phone, email, etc.</summary>
    public CommItemMode Mode_;

    ///<summary>Enum:CommSentOrReceived Neither=0,Sent=1,Received=2.</summary>
    public CommSentOrReceived SentOrReceived;

    ///<summary>FK to userod.UserNum.</summary>
    public long UserNum;

    ///<summary>Signature.  For details, see procnote.Signature.</summary>
    public string Signature;

    ///<summary>True if signed using the Topaz signature pad, false otherwise.</summary>
    public bool SigIsTopaz;

    ///<summary>Automatically updated by MySQL every time a row is added or changed.</summary>
    public DateTime DateTStamp;

    ///<summary>Date and time when commlog ended.  Mainly for internal use.</summary>
    public DateTime DateTimeEnd;

    ///<summary>Enum:CommItemSource Set to the source of the entity that created this commlog.  E.g. WebSched.</summary>
    public CommItemSource CommSource;

    ///<summary>FK to program.ProgramNum.  This will be 0 unless CommSource is set to ProgramLink.</summary>
    public long ProgramNum;

    ///<summary>Track Date Created for commlogs. Value for existing commlogs show as blank in the UI. Not editable by user.</summary>
    public DateTime DateTEntry;

    ///<summary>FK to referral.ReferralNum.</summary>
    public long ReferralNum;

    ///<summary>Enum:EnumCommReferralBehavior Changes how this referral commlog displays within grids.</summary>
    public EnumCommReferralBehavior CommReferralBehavior;

    public Commlog Copy()
    {
        return (Commlog) MemberwiseClone();
    }

    public bool Compare(Commlog commlogCur)
    {
        return PatNum == commlogCur.PatNum &&
               CommDateTime == commlogCur.CommDateTime &&
               CommType == commlogCur.CommType &&
               Note == commlogCur.Note &&
               Mode_ == commlogCur.Mode_ &&
               SentOrReceived == commlogCur.SentOrReceived &&
               UserNum == commlogCur.UserNum;
    }
}

public enum CommItemMode
{
    None,
    Email,
    Mail,
    Phone,

    [Description("In Person")]
    InPerson,

    Text,

    [Description("Email and Text")]
    EmailAndText,

    [Description("Phone and Text")]
    PhoneAndText,

    Fax
}

public enum CommSentOrReceived
{
    Neither,
    Sent,
    Received
}

public enum CommItemSource
{
    User = 0,
    WebSched = 1
}

public enum EnumCommReferralBehavior
{
    None,
    TopAnchored,
    Hidden
}