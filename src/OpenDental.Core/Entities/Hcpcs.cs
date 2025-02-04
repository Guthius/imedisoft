using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Hcpcs : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long HcpcsNum;

    ///<summary>Examples: AQ, J1040</summary>
    public string HcpcsCode;

    ///<summary>Short description.  This is the HCPCS supplied abbreviated description.</summary>
    public string DescriptionShort;
}