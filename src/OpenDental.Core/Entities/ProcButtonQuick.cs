using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ProcButtonQuick : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ProcButtonQuickNum;

    public string Description;

    ///<summary>FK to procedurecode.ProcCode. </summary>
    public string CodeValue;

    public string Surf;
    public int YPos;
    public int ItemOrder;
    public bool IsLabel;

    public bool Equals(ProcButtonQuick other)
    {
        return ProcButtonQuickNum == other.ProcButtonQuickNum
               && Description == other.Description
               && CodeValue == other.CodeValue
               && Surf == other.Surf
               && ItemOrder == other.ItemOrder
               && YPos == other.YPos
               && IsLabel == other.IsLabel;
    }
}