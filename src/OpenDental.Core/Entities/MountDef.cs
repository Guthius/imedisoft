using System.Drawing;
using System.Xml.Serialization;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class MountDef : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long MountDefNum;

    public string Description;
    public int ItemOrder;
    public int Width;
    public int Height;

    ///<summary>Color of the mount background.  Typically white for photos and black for radiographs.</summary>
    [XmlIgnore]
    public Color ColorBack;

    ///<summary>Color of drawings and text.  Typically black for photos and white for radiographs.</summary>
    public Color ColorFore;

    ///<summary>Color of drawing text background.  Typically white for photos and black for radiographs. Transparent is allowed.</summary>
    public Color ColorTextBack;

    ///<summary>Scale, decimal places, and units, separated by spaces.  Example: "123.4 0 mm". The first two are required; units is optional.  When a mount is created, and if this isn't blank, then this is converted into an ImageDraw of type ScaleValue.</summary>
    public string ScaleValue;

    ///<summary>FK to definition.DefNum. If set, a new mount will go into this category, regardless of which category is currently selected.</summary>
    public long DefaultCat;

    ///<summary>If true, each image will be flipped as it's acquired. Because ScanX images are backwards.</summary>
    public bool FlipOnAcquire;

    ///<summary>If true, then it will switch to Adj mode instead of the usual Pan mode.</summary>
    public bool AdjModeAfterSeries;

    public MountDef Copy()
    {
        return (MountDef) MemberwiseClone();
    }
}