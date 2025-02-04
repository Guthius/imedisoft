using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class InsSub : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long InsSubNum;

    ///<summary>FK to insplan.PlanNum.</summary>
    public long PlanNum;

    ///<summary>FK to patient.PatNum.</summary>
    public long Subscriber;

    ///<summary>Date plan became effective. Is 0001-01-01 if not set.</summary>
    public DateTime DateEffective;

    ///<summary>Date plan was terminated. Is 0001-01-01 if not set.</summary>
    public DateTime DateTerm;

    ///<summary>Release of information signature is on file.</summary>
    public bool ReleaseInfo;

    ///<summary>Assignment of benefits signature is on file.  For Canada, this handles Payee Code, F01.  Option to pay other third party is not included.</summary>
    public bool AssignBen;

    ///<summary>Number assigned by insurance company. No dashes. Not allowed to be blank.</summary>
    public string SubscriberID;

    ///<summary>User doesn't usually put these in.  Only used when automatically requesting benefits, such as with Trojan.  All the benefits get stored here in text form for later reference.  Not at plan level because might be specific to subscriber.  If blank, we try to display a benefitNote for another subscriber to the plan.</summary>
    public string BenefitNotes;

    ///<summary>Use to store any other info that affects coverage.</summary>
    public string SubscNote;

    ///<summary>FK to userod.UserNum.  Set to the user logged in when the row was inserted at SecDateEntry date and time.</summary>
    public long SecUserNumEntry;

    ///<summary>Timestamp automatically generated and user not allowed to change.  The actual date of entry.</summary>
    public DateTime SecDateEntry;

    ///<summary>Automatically updated by MySQL every time a row is added or changed. Could be changed due to user editing, custom queries or program
    ///updates.  Not user editable with the UI.</summary>
    public DateTime SecDateTEdit;

    ///<summary>Returns a copy of this InsSub.</summary>
    public InsSub Copy()
    {
        return (InsSub) MemberwiseClone();
    }
}