using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class PatRestriction : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PatRestrictionNum;

    ///<summary>FK to patient.PatNum.</summary>
    public long PatNum;

    ///<summary>Enum:PatRestrict </summary>
    public PatRestrict PatRestrictType;
}

public enum PatRestrict
{
    None,
    ApptSchedule
}