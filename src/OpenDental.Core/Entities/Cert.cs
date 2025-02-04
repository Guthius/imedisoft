using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Cert : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long CertNum;

    public string Description;

    ///<summary>The exact name of a wiki page.</summary>
    public string WikiPageLink;

    public int ItemOrder;
    public bool IsHidden;

    ///<summary>FK to definition.DefNum.</summary>
    public long CertCategoryNum;
}