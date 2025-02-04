using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class InsFilingCodeSubtype : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long InsFilingCodeSubtypeNum;

    ///<summary>FK to insfilingcode.insfilingcodenum</summary>
    public long InsFilingCodeNum;

    public string Descript;

    public InsFilingCodeSubtype Clone()
    {
        return (InsFilingCodeSubtype) MemberwiseClone();
    }
}