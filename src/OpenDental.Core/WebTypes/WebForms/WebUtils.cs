using System;
using OpenDentBusiness.Remoting;

namespace OpenDentBusiness.WebTypes.WebForms;

public class WebUtils
{
    public static long GetDentalOfficeID(string regKey = null)
    {
        if (string.IsNullOrEmpty(regKey))
        {
            regKey = PrefC.GetString(PrefName.RegistrationKey);
        }

        try
        {
            var payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, new PayloadItem(regKey, "RegKey"));
            
            return WebSerializer.DeserializeTag<long>(SheetsSynchProxy.GetWebServiceInstance().GetDentalOfficeID(payload), "Success");
        }
        catch
        {
            // ignored
        }

        return 0;
    }

    public static long GetRegKeyID(string regKey = null)
    {
        if (string.IsNullOrEmpty(regKey))
        {
            regKey = PrefC.GetString(PrefName.RegistrationKey);
        }

        try
        {
            var payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, new PayloadItem(regKey, "RegKey"));
            
            return WebSerializer.DeserializeTag<long>(SheetsSynchProxy.GetWebServiceInstance().GetRegistrationKeyID(payload), "Success");
        }
        catch
        {
            // ignored
        }

        return 0;
    }
    
    public static string GetSheetDefAddress(string regKey = null)
    {
        if (string.IsNullOrEmpty(regKey))
        {
            regKey = PrefC.GetString(PrefName.RegistrationKey);
        }

        try
        {
            var payload = PayloadHelper.CreatePayloadWebHostSynch(regKey, new PayloadItem(regKey, "RegKey"));
            
            return WebSerializer.DeserializeTag<string>(SheetsSynchProxy.GetWebServiceInstance().GetSheetDefAddress(payload), "Success");
        }
        catch
        {
            // ignored
        }

        return "";
    }
}