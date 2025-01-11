using DataConnectionBase;

namespace OpenDentBusiness;

public class Currency
{
    public static string GetCurrencyFormat()
    {
        return "F2";
    }

    public static double Round(double amt)
    {
        return SIn.Double(amt.ToString(GetCurrencyFormat()));
    }
}