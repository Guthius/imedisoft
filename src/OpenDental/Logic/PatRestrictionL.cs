using CodeBase;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDental.Logic;

public class PatRestrictionL
{
    public static bool IsRestricted(long patNum, PatRestrict patRestrictType, bool suppressMessage = false)
    {
        if (!PatRestrictions.IsRestricted(patNum, patRestrictType))
        {
            return false;
        }

        if (!suppressMessage)
        {
            ODMessageBox.Show("Not allowed due to patient restriction\r\n" + PatRestrictions.GetPatRestrictDesc(patRestrictType));
        }

        return true;
    }
}