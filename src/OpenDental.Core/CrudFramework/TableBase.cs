using System;
using System.Xml.Serialization;

namespace OpenDentBusiness;

public abstract class TableBase
{
    public bool IsNew { get; set; }

    [XmlIgnore]
    public object TagOD { get; set; }

    private static int _maxAllowedPacket;

    public static int MaxAllowedPacketCount
    {
        get
        {
            if (_maxAllowedPacket > 0)
            {
                return _maxAllowedPacket;
            }

            const int kilobyte = 1024; //1KB
            const int megabyte = kilobyte * kilobyte; //1MB
            
            var retVal = MiscData.GetMaxAllowedPacket() - 8 * kilobyte;
            _maxAllowedPacket = Math.Min(Math.Max(retVal, 8 * kilobyte), megabyte);
            return _maxAllowedPacket;
        }
    }
}