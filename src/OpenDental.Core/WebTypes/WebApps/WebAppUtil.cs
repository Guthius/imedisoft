using System;

namespace OpenDentBusiness.WebTypes.WebApps;

public class WebAppUtil
{
    public static string GetWebAppUrl(eServiceCode eserviceCode, long clinicNum = 0)
    {
        if (eserviceCode == eServiceCode.PaymentPortalUI)
        {
            return BuildPaymentPortalUrl(clinicNum);
        }
        
        throw new Exception("Cannot build URL for the specified eService code.");
    }

    public static string BuildPaymentPortalUrl(long clinicNum = 0)
    {
        var url = "https://" + PrefC.GetString(PrefName.PaymentPortalSubDomain) + "." + PrefC.GetString(PrefName.WebAppDomain);
        
        if (clinicNum == 0)
        {
            url += "?cid=" + PrefC.GetString(PrefName.WebAppClinicGuid);
        }
        else
        {
            url += "?cid=" + ClinicPrefs.GetPrefValue(PrefName.WebAppClinicGuid, clinicNum);
        }

        return url;
    }
}