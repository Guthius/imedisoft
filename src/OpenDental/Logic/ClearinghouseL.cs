using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental.Logic;

public class ClearinghouseL
{
    public static Clearinghouse GetClearinghouseHq(long clearinghouseNumHq, bool suppressError = false)
    {
        var clearinghouse = Clearinghouses.GetClearinghouse(clearinghouseNumHq);

        if (clearinghouse == null && !suppressError)
        {
            MsgBox.Show("Clearinghouses", "Error. Could not locate Clearinghouse.");
        }

        return clearinghouse;
    }

    public static string GetDescript(long clearinghouseNum)
    {
        return clearinghouseNum == 0 ? "" : GetClearinghouseHq(clearinghouseNum).Description;
    }
}