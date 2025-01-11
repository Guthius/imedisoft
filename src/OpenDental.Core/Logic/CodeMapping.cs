using System.Globalization;
using System.Linq;

namespace OpenDentBusiness;

public class CodeMapping
{
    public static string GetArchSurfaceFromProcCode(ProcedureCode procedureCode)
    {
        if (CultureInfo.CurrentCulture.Name.EndsWith("CA"))
        {
            return "";
        }

        return ProcedureCodes.GetMandibularCodes().Any(x => x.ProcCode == procedureCode.ProcCode) ? "L" : "U";
    }
}