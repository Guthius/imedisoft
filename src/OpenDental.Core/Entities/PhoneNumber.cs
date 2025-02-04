using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class PhoneNumber : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PhoneNumberNum;

    ///<summary>FK to patient.PatNum.</summary>
    public long PatNum;

    ///<summary>The actual phone number for the patient.  Includes any punctuation.  No leading 1 or plus, so almost always 10 digits.</summary>
    public string PhoneNumberVal;

    ///<summary>The phone number for the patient with all non-digit chars and any leading 1's or 0's removed.</summary>
    public string PhoneNumberDigits;

    public PhoneType PhoneType;
}

public enum PhoneType
{
    Other,
    HmPhone,
    WkPhone,
    WirelessPhone
}