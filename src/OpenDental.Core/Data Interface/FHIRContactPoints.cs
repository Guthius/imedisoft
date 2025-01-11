using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class FHIRContactPoints
{
    public static List<FHIRContactPoint> GetContactPoints(long fHIRSubscriptionNum)
    {
        var command = "SELECT * FROM fhircontactpoint WHERE FHIRSubscriptionNum=" + SOut.Long(fHIRSubscriptionNum);
        return FHIRContactPointCrud.SelectMany(command);
    }

    
    public static long Insert(FHIRContactPoint fHIRContactPoint)
    {
        return FHIRContactPointCrud.Insert(fHIRContactPoint);
    }

    
    public static void Update(FHIRContactPoint fHIRContactPoint)
    {
        FHIRContactPointCrud.Update(fHIRContactPoint);
    }

    
    public static void Delete(long fHIRContactPointNum)
    {
        FHIRContactPointCrud.Delete(fHIRContactPointNum);
    }
}