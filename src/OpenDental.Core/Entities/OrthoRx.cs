using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class OrthoRx : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long OrthoRxNum;

    ///<summary>FK to orthohardwarespec.OrthoHardwareSpecNum. Description comes from here.</summary>
    public long OrthoHardwareSpecNum;

    public string Description;

    ///<summary>Tooth numbers stored here are always stored in Universal (1-32) notation. They are displayed to the user as Palmer notation. For brackets and elastics, always use tooth numbers separated by commas, like 2,3,4,5,6. For wires, must use a range like 2-15.</summary>
    public string ToothRange;

    public int ItemOrder;

    public OrthoRx Copy()
    {
        return (OrthoRx) MemberwiseClone();
    }
}