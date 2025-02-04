using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Referral : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ReferralNum;

    public string LName;
    public string FName;
    public string MName;

    ///<summary>SSN or TIN, no punctuation.  For Canada, this holds the referring provider CDA num for claims.</summary>
    public string SSN;

    public bool UsingTIN;

    ///<summary>FK to definition.DefNum.</summary>
    public long Specialty;

    public string ST;
    public string Telephone;
    public string Address;
    public string Address2;
    public string City;
    public string Zip;
    public string Note;
    public string Phone2;

    public bool IsHidden;

    ///<summary>Set to true for referralls such as Yellow Pages.</summary>
    public bool NotPerson;

    public string Title;
    public string EMail;

    ///<summary>FK to patient.PatNum for referrals that are patients.</summary>
    public long PatNum;

    ///<summary>NPI for the referral</summary>
    public string NationalProvID;

    ///<summary>FK to sheetdef.SheetDefNum.  Referral slips can be set for individual referral sources.  If zero, then the default internal referral slip will be used instead of a custom referral slip.</summary>
    public long Slip;

    ///<summary>True if another dentist or physician.  Cannot be a patient.</summary>
    public bool IsDoctor;

    ///<summary>True if checkbox E-mail Trust for Direct is checked.</summary>
    public bool IsTrustedDirect;

    ///<summary>The datetime this referral was last edited.</summary>
    public DateTime DateTStamp;

    ///<summary>True if the referral is a preferred referral. The only purpose is to allow filtering in the list of referrals so that the list can be much shorter.</summary>
    public bool IsPreferred;

    public string BusinessName;

    ///<summary>This is a global field used for Scheduling Notes that will show in the family module patient info grid.</summary>
    public string DisplayNote;

    public Referral Copy()
    {
        return (Referral) MemberwiseClone();
    }

    public string GetNameFL()
    {
        var retVal = "";
        if (FName != "")
        {
            retVal += FName + " ";
        }

        if (MName != "")
        {
            retVal += MName + " ";
        }

        retVal += LName;
        if (Title != "")
        {
            retVal += ", " + Title;
        }

        return retVal;
    }
}