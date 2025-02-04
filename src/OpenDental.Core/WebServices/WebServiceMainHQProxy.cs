using System;

namespace OpenDentBusiness;

public static class WebServiceMainHQProxy
{
    public static IWebServiceMainHQ GetWebServiceMainHQInstance()
    {
        throw new NotImplementedException();
    }

    public class ShortGuidResult
    {
        public string ShortGuid;
        public string ShortURL;
        public string MediumURL;
        public bool IsForSms;
    }
}