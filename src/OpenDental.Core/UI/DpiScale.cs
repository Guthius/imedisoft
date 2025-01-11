using System;

namespace OpenDentBusiness.UI;

public class DpiScale(float scale)
{
    public int ToInt(float value)
    {
        return (int) Math.Round(value * scale);
    }
}