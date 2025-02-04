using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Snomed : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long SnomedNum;

    ///<summary>Used as FK by other tables.  Also called the Concept ID.  Not allowed to edit this column once saved in the database.</summary>
    public string SnomedCode;

    public string Description;
}