using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class CommlogHist : TableBase
{
    #region CommlogHist rows

    [CrudColumn(IsPriKey = true)]
    public long CommlogHistNum;

    ///<summary>Raw Text of customer phone number that the tech was talking to.</summary>
    public string CustomerNumberRaw;

    ///<summary>Enum:CommlogHistSource Indicates what event triggered this CommlogHist row to be created.</summary>
    public CommlogHistSource HistSource;

    ///<summary>Automatically updated by MySQL every time a row is added or changed.</summary>
    public DateTime DateTStamp;

    ///<summary>Track Date Created for commloghists. Value for existing commlogs show as blank in the UI. Not editable by user.</summary>
    public DateTime DateTEntry;

    #endregion CommlogHist rows

    #region Copy/Paste Inheritance from Commlog

    public long CommlogNum;
    public long PatNum;

    public DateTime CommDateTime;
    public long CommType;
    public string Note;

    public CommItemMode Mode_;
    public CommSentOrReceived SentOrReceived;
    public long UserNum;
    public string Signature;
    public bool SigIsTopaz;
    public DateTime DateTimeEnd;
    public CommItemSource CommSource;
    public long ProgramNum;
    public long ReferralNum;
    public EnumCommReferralBehavior CommReferralBehavior;

    #endregion Copy/Paste Inheritance from Commlog

    public static Commlog ConvertToCommlog(CommlogHist commlogHist)
    {
        var commlog = new Commlog();
        commlog.CommlogNum = commlogHist.CommlogNum;
        commlog.PatNum = commlogHist.PatNum;
        commlog.CommDateTime = commlogHist.CommDateTime;
        commlog.CommType = commlogHist.CommType;
        commlog.Note = commlogHist.Note;
        commlog.Mode_ = commlogHist.Mode_;
        commlog.SentOrReceived = commlogHist.SentOrReceived;
        commlog.UserNum = commlogHist.UserNum;
        commlog.Signature = commlogHist.Signature;
        commlog.SigIsTopaz = commlogHist.SigIsTopaz;
        commlog.DateTStamp = commlogHist.DateTStamp;
        commlog.DateTimeEnd = commlogHist.DateTimeEnd;
        commlog.CommSource = commlogHist.CommSource;
        commlog.ProgramNum = commlogHist.ProgramNum;
        commlog.DateTEntry = commlogHist.DateTEntry;
        commlog.ReferralNum = commlogHist.ReferralNum;
        commlog.CommReferralBehavior = commlogHist.CommReferralBehavior;
        return commlog;
    }
}

public enum CommlogHistSource
{
    WrapUp
}