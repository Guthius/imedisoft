using System;
using System.Drawing;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Mount : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long MountNum;

    ///<summary>FK to patient.PatNum</summary>
    public long PatNum;

    ///<summary>FK to definition.DefNum. Categories for documents.</summary>
    public long DocCategory;

    /// <summary>The date/time at which the mount itself was created. Usually, all the images on the mount are the same date, but not always.</summary>
    public DateTime DateCreated;

    public string Description;
    public string Note;
    public int Width;
    public int Height;

    ///<summary>Color of the mount background.  Typically white for photos and black for radiographs. Transparency not allowed.</summary>
    public Color ColorBack;

    ///<summary>FK to provider.ProvNum. Optional. Used for radiographs.</summary>
    public long ProvNum;

    ///<summary>Color of drawings and text.  Typically black for photos and white for radiographs.</summary>
    public Color ColorFore;

    ///<summary>Color of drawing text background.  Typically white for photos and black for radiographs. Transparent is allowed.</summary>
    public Color ColorTextBack;

    ///<summary>If true, each image will be flipped as it's acquired. Because ScanX images are backwards.</summary>
    public bool FlipOnAcquire;

    ///<summary>If true, then it will switch to Adj mode instead of the usual Pan mode.</summary>
    public bool AdjModeAfterSeries;
    
    public Mount Copy()
    {
        return (Mount) MemberwiseClone();
    }
}