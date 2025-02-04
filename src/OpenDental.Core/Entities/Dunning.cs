using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Dunning : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long DunningNum;

    ///<summary>The actual dunning message that will go on the patient bill.</summary>
    public string DunMessage;

    ///<summary>FK to definition.DefNum.</summary>
    public long BillingType;

    ///<summary>Program forces only 0,30,60,or 90.</summary>
    public byte AgeAccount;

    ///<summary>Enum:YN Set Y to only show if insurance is pending.</summary>
    public YN InsIsPending;

    ///<summary>A message that will be copied to the NoteBold field of the Statement.</summary>
    public string MessageBold;

    ///<summary>An override for the default email subject.</summary>
    public string EmailSubject;

    ///<summary>An override for the default email body. Limit in db: 16M char.</summary>
    public string EmailBody;

    ///<summary>The number of days before an account reaches AgeAccount to include this dunning message on statements.
    ///Example: If DaysInAdvance=3 and AgeAccount=90, an account that is 87 days old when bills are generated will include this message.</summary>
    public int DaysInAdvance;

    ///<summary>FK to clinic.ClinicNum.</summary>
    public long ClinicNum;

    ///<summary>Boolean. Is true when the message is specifically created for super families.</summary>
    public bool IsSuperFamily;

    public Dunning Copy()
    {
        return (Dunning) MemberwiseClone();
    }
}