using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class RefAttach : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long RefAttachNum;

    ///<summary>FK to referral.ReferralNum.</summary>
    public long ReferralNum;

    ///<summary>FK to patient.PatNum.</summary>
    public long PatNum;

    public int ItemOrder;

    ///<summary>Date of referral.</summary>
    public DateTime RefDate;

    public ReferralType RefType;
    public ReferralToStatus RefToStatus;

    public string Note;

    ///<summary>Used to track ehr events.  All outgoing referrals default to true.  The incoming ones get a popup asking if it's a transition of care.</summary>
    public bool IsTransitionOfCare;

    ///<summary>FK to procedurelog.ProcNum</summary>
    public long ProcNum;
    
    public DateTime DateProcComplete;

    ///<summary>FK to provider.ProvNum.  Used when referring out a patient to track the referring provider for EHR meaningful use.  Will be -1 when RefType is not set to RefTo.</summary>
    public long ProvNum;

    public RefAttach Copy()
    {
        return (RefAttach) MemberwiseClone();
    }
}

public enum ReferralType
{
    RefTo,
    RefFrom,
    RefCustom
}