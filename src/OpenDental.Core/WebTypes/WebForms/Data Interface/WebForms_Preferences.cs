using System.Collections.Generic;
using Imedisoft.Core.Caching;
using OpenDentBusiness.Remoting;

namespace OpenDentBusiness.WebTypes.WebForms;

public class WebForms_Preferences
{
    public static bool SetPreferences(WebForms_Preference pref, string regKey = null, string urlOverride = null)
    {
        var retVal = false;
        if (string.IsNullOrEmpty(regKey))
        {
            regKey = PrefC.GetString(PrefName.RegistrationKey);
        }

        try
        {
            var listPayloadItems = new List<PayloadItem>
            {
                new(regKey, "RegKey"),
                new(pref, nameof(WebForms_Preference))
            };

            var payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, listPayloadItems.ToArray());

            SheetsSynchProxy.UrlOverride = urlOverride;

            retVal = WebSerializer.DeserializeTag<bool>(SheetsSynchProxy.GetWebServiceInstance().SetPreferences(payload), "Success");
        }
        catch
        {
            // ignored
        }

        return retVal;
    }

    public static bool TryGetPreference(out WebForms_Preference _webForms_Preference, string regKey = null)
    {
        _webForms_Preference = new WebForms_Preference();
        if (string.IsNullOrEmpty(regKey))
        {
            regKey = PrefC.GetString(PrefName.RegistrationKey);
        }

        try
        {
            var payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, new PayloadItem(regKey, "RegKey"));

            _webForms_Preference = WebSerializer.DeserializeTag<WebForms_Preference>(SheetsSynchProxy.GetWebServiceInstance().GetPreferences(payload), "Success");

            return true;
        }
        catch
        {
            return false;
        }
    }
}