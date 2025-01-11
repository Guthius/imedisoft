using System;
using System.Collections.Generic;
using OpenDentBusiness.Remoting;

namespace OpenDentBusiness.WebTypes.WebForms;

public class WebForms_Preferences
{
    public static bool SetPreferences(WebForms_Preference pref, string regKey = null, string urlOverride = null)
    {
        bool retVal = false;
        if (string.IsNullOrEmpty(regKey))
        {
            regKey = PrefC.GetString(PrefName.RegistrationKey);
        }

        try
        {
            List<PayloadItem> listPayloadItems = new List<PayloadItem>
            {
                new PayloadItem(regKey, "RegKey"),
                new PayloadItem(pref, nameof(WebForms_Preference))
            };
            string payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, listPayloadItems.ToArray());
            SheetsSynchProxy.UrlOverride = urlOverride; //SheetsSynchProxy.GetWebServiceInstance() gracefully handles null.
            retVal = WebSerializer.DeserializeTag<bool>(SheetsSynchProxy.GetWebServiceInstance().SetPreferences(payload), "Success");
        }
        catch (Exception ex)
        {
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
            string payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, new PayloadItem(regKey, "RegKey"));

            _webForms_Preference = WebSerializer.DeserializeTag<WebForms_Preference>(SheetsSynchProxy.GetWebServiceInstance().GetPreferences(payload), "Success");
        }
        catch (Exception ex)
        {
            return false;
        }

        return true;
    }
}