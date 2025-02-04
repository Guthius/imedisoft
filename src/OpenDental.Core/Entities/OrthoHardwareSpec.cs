using System.Drawing;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class OrthoHardwareSpec : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long OrthoHardwareSpecNum;

    public EnumOrthoHardwareType OrthoHardwareType;
    public string Description;
    public Color ItemColor;
    public bool IsHidden;
    public int ItemOrder;

    public OrthoHardwareSpec Copy()
    {
        return (OrthoHardwareSpec) MemberwiseClone();
    }
}