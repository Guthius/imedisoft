using System.ComponentModel;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Etrans835 : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long Etrans835Num;

    ///<summary>FK to etrans.EtransNum .</summary>
    public long EtransNum;

    ///<summary>Up to 60 characters.  Corresponds to X835.PayerName, a read-only field.</summary>
    public string PayerName;

    ///<summary>Up to 50 characters.  Corresponds to X835.TransRefNum, a read-only field.</summary>
    public string TransRefNum;

    ///<summary>Corresponds to X835.InsPaid, a read-only field.</summary>
    public double InsPaid;

    ///<summary>Up to 9 characters.  Corresponds to X835.ControlId, a read-only field.</summary>
    public string ControlId;

    ///<summary>Up to 3 characters.  Corresponds to X835._paymentMethodCode, a read-only field.</summary>
    public string PaymentMethodCode;

    ///<summary>Up to 100 characters (not based on actual patient name field sizes).
    ///Corresponds to Hx835_Claim.PatientName.ToString() if one patient, or says "(#)" if multiple patients to show count.</summary>
    public string PatientName;

    ///<summary>Enum:X835Status .  Calculated status.  Only changes when ERA changes.</summary>
    public X835Status Status;

    ///<summary>Enum:X835AutoProcessed .  The initial disposition of ERA's that have passed through our auto/semi-auto processing system.</summary>
    public X835AutoProcessed AutoProcessed;

    ///<summary>True if a user has acknowledged the auto processed ERA.</summary>
    public bool IsApproved;

    public Etrans835 Copy()
    {
        return (Etrans835) MemberwiseClone();
    }
}

public enum X835AutoProcessed
{
    None,

    [Description("Semi-automatic Incomplete")]
    SemiAutoIncomplete,

    [Description("Semi-automatic Complete")]
    SemiAutoComplete,

    [Description("Fully-automatic Incomplete")]
    FullAutoIncomplete,

    [Description("Fully-automatic Complete")]
    FullAutoComplete,
}