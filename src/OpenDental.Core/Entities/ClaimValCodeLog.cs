using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ClaimValCodeLog : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ClaimValCodeLogNum;

    ///<summary>FK to claim.ClaimNum.</summary>
    public long ClaimNum;

    ///<summary>Descriptive abbreviation to help place field on form (Ex: "FL55" for field 55).</summary>
    public string ClaimField;

    ///<summary>Value Code. 2 char.</summary>
    public string ValCode;

    ///<summary>Value Code Amount.</summary>
    public double ValAmount;

    ///<summary>Order of Value Code</summary>
    public int Ordinal;
}