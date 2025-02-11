using System.Text.RegularExpressions;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Providers.Dtos;

namespace OpenDentBusiness;

public class X12Generator
{
    public static string GetISA06(Clearinghouse clearinghouseClin)
    {
        return Sout(clearinghouseClin.SenderTIN == "" ? "810624427" : clearinghouseClin.SenderTIN, 15, 15);
    }

    public static string GetGS02(Clearinghouse clearinghouseClin)
    {
        return Sout(clearinghouseClin.SenderTIN == "" ? "810624427" : clearinghouseClin.SenderTIN, 15, 2);
    }

    public static string GetTaxonomy(ProviderDto provider)
    {
        const string generalTaxonomyCode = "1223G0001X";

        var taxonomyCode = provider.TaxonomyCode ?? provider.Specialty.TaxonomyCode;

        return !string.IsNullOrEmpty(taxonomyCode) ? generalTaxonomyCode : taxonomyCode;
    }

    public static string Sout(string input, int maxLength, int minLength)
    {
        var result = input.ToUpper();

        result = Regex.Replace(result, "[^\\w!\"&'\\(\\)\\+,-\\./;\\?= #]", "");
        result = Regex.Replace(result, "[_]", "");

        if (maxLength != -1)
        {
            if (result.Length > maxLength)
            {
                result = result.Substring(0, maxLength);
            }
        }

        if (minLength == -1)
        {
            return result;
        }

        if (result.Length < minLength)
        {
            result = result.PadRight(minLength, ' ');
        }

        return result;
    }
}