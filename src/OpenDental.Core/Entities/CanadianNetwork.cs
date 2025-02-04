using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class CanadianNetwork : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long CanadianNetworkNum;

    public string Abbrev;
    public string Descript;

    ///<summary>A01.  Up to 12 char.</summary>
    public string CanadianTransactionPrefix;

    ///<summary>Set to true if this network is in charge of handling all Request for Payment Reconciliation (RPR) transactions for all carriers within this network, as opposed to the individual carriers wihtin the network processing the RPR transactions themselves.</summary>
    public bool CanadianIsRprHandler;

    public CanadianNetwork Copy()
    {
        return (CanadianNetwork) MemberwiseClone();
    }
}