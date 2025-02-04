using System;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness.localhost;

namespace OpenDentBusiness;

public class CustomerUpdatesProxy
{
    public static Service1 GetWebServiceInstance()
    {
        var service1 = new Service1();
        
        service1.Url = PrefC.GetString(PrefName.UpdateServerAddress);
        service1.Timeout = (int) TimeSpan.FromMinutes(20).TotalMilliseconds;
        
        return service1;
    }
}