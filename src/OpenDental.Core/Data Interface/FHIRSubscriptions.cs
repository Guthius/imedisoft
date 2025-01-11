using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class FHIRSubscriptions
{
    ///<summary>Gets one FHIRSubscription from the db.</summary>
    public static FHIRSubscription GetOne(long fHIRSubscriptionNum)
    {
        return FHIRSubscriptionCrud.SelectOne(fHIRSubscriptionNum);
    }

    
    public static long Insert(FHIRSubscription fHIRSubscription)
    {
        var fHIRSubscriptionNum = FHIRSubscriptionCrud.Insert(fHIRSubscription);
        for (var i = 0; i < fHIRSubscription.ListContactPoints.Count; i++)
        {
            fHIRSubscription.ListContactPoints[i].FHIRSubscriptionNum = fHIRSubscriptionNum;
            FHIRContactPoints.Insert(fHIRSubscription.ListContactPoints[i]);
        }

        return fHIRSubscriptionNum;
    }

    
    public static void Update(FHIRSubscription fHIRSubscription)
    {
        var command = "SELECT * FROM fhircontactpoint WHERE FHIRSubscriptionNum=" + SOut.Long(fHIRSubscription.FHIRSubscriptionNum);
        var listFHIRContactPoints = FHIRContactPointCrud.SelectMany(command);
        var listFHIRContactPointsFromFHIRSub = fHIRSubscription.ListContactPoints;
        for (var i = 0; i < listFHIRContactPointsFromFHIRSub.Count; i++)
        {
            listFHIRContactPointsFromFHIRSub[i].FHIRSubscriptionNum = fHIRSubscription.FHIRSubscriptionNum;
            if (listFHIRContactPoints.Any(x => x.FHIRContactPointNum == listFHIRContactPointsFromFHIRSub[i].FHIRContactPointNum))
                //Update any FHIRContactPoint that already exists in the db
                FHIRContactPoints.Update(listFHIRContactPointsFromFHIRSub[i]);
            else
                //Insert any FHIRContactPoint that does not exist in the db
                FHIRContactPoints.Insert(listFHIRContactPointsFromFHIRSub[i]);
        }

        //Delete any FHIRContactPoint that exists in the db but not in the new list
        listFHIRContactPoints = listFHIRContactPoints.FindAll(x => !fHIRSubscription.ListContactPoints.Any(y => y.FHIRContactPointNum == x.FHIRContactPointNum));
        for (var i = 0; i < listFHIRContactPoints.Count; i++) FHIRContactPoints.Delete(listFHIRContactPoints[i].FHIRContactPointNum);
        FHIRSubscriptionCrud.Update(fHIRSubscription);
    }

    
    public static void Update(FHIRSubscription fHIRSubscription, FHIRSubscription fHIRSubscriptionOld)
    {
        FHIRSubscriptionCrud.Update(fHIRSubscription, fHIRSubscriptionOld);
    }

    
    public static void Delete(long fHIRSubscriptionNum)
    {
        FHIRSubscriptionCrud.Delete(fHIRSubscriptionNum);
        var command = "DELETE FROM fhircontactpoint WHERE FHIRSubscriptionNum=" + SOut.Long(fHIRSubscriptionNum);
        Db.NonQ(command);
    }
}