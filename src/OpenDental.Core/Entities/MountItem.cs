using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class MountItem : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long MountItemNum;

    /// <summary>FK to mount.MountNum.</summary>
    public long MountNum;

    public int Xpos;
    public int Ypos;
    public int ItemOrder;
    public int Width;
    public int Height;

    ///<summary>0,90,180,or 270.</summary>
    public int RotateOnAcquire;

    ///<summary>An optional list of tooth numbers. In Db, rigorously formatted as American numbers, and separated by commas.  For display, uses hyphens for sequences.  Very likely supports international tooth numbers, but not tested for that.  These tooth numbers are initially copied here from the MountItemDef. They are then copied to the document (image) that gets put in this mount item.  So mountitem.ToothNumbers is not actually used to indicate the final tooth numbers.  use document.ToothNumbers instead.</summary>
    public string ToothNumbers;

    ///<summary>Instead of an image, a mount item can show text. In this case, ItemOrder=0. Text color and background will be the mount default.</summary>
    public string TextShowing;

    ///<summary>This could vary significantly based on the size of the mount.  It's always relative to mount pixels.</summary>
    public float FontSize;

    public MountItem Copy()
    {
        return (MountItem) MemberwiseClone();
    }

    public override string ToString()
    {
        return "ItemOrder:" + ItemOrder;
    }
}