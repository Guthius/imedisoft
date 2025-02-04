using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class SmsPhone : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long SmsPhoneNum;

    ///<summary>FK to clinic.ClinicNum. </summary>
    public long ClinicNum;
    
    public string PhoneNumber;

    ///<summary>Date and time this phone number became active.</summary>
    public DateTime DateTimeActive;

    ///<summary>Date and time this phone number became inactive. Once inactive, the phone is dead and cannot be reactivated. A new number will have to be purchased.</summary>
    public DateTime DateTimeInactive;

    ///<summary>Used to indicate why this phone number was made inactive.</summary>
    public string InactiveCode;

    public string CountryCode;
    
    public SmsPhone Copy()
    {
        return (SmsPhone) MemberwiseClone();
    }
}