using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Sop : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long SopNum;

    public string SopCode;
    public string Description;

    public Sop Copy()
    {
        return (Sop) MemberwiseClone();
    }
}