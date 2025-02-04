using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class MountItemDef : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long MountItemDefNum;

    /// <summary>FK to mountdef.MountDefNum.</summary>
    public long MountDefNum;

    public int Xpos;
    public int Ypos;
    public int Width;
    public int Height;
    public int ItemOrder;

    ///<summary>0,90,180,or 270.</summary>
    public int RotateOnAcquire;

    /// <summary>An optional list of tooth numbers. In Db, rigorously formatted as American numbers, and separated by commas.  For display, uses hyphens for sequences.  Very likely supports international tooth numbers, but not tested for that.</summary>
    public string ToothNumbers;

    ///<summary>Instead of an image, a mount item can show text. In this case, ItemOrder=0. Text color and background will be the mount default.</summary>
    public string TextShowing;

    ///<summary>This could vary significantly based on the size of the mount.  It's always relative to mount pixels.</summary>
    public float FontSize;

    public override string ToString()
    {
        if (TextShowing != "")
        {
            return TextShowing;
        }

        return ItemOrder.ToString();
    }
}