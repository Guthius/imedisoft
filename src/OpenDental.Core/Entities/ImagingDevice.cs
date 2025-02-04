using System.ComponentModel;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ImagingDevice : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ImagingDeviceNum;

    public string Description;

    /// <summary>Name of the computer where this device is available.  Optional.  If blank, then this device will be available to all computers.</summary>
    public string ComputerName;

    ///<summary>Enum:EnumImgDeviceType </summary>
    public EnumImgDeviceType DeviceType;

    ///<summary>The name of the twain device as in Windows.</summary>
    public string TwainName;

    public int ItemOrder;
    public bool ShowTwainUI;

    public ImagingDevice Copy()
    {
        return (ImagingDevice) MemberwiseClone();
    }
}

public enum EnumImgDeviceType
{
    TwainRadiograph,

    [Description("XDR (not functional)")]
    XDR,

    TwainMulti
}