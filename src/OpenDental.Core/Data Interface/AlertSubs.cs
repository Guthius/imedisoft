using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class AlertSubs
{
    public static List<AlertSub> GetAllForUser(long userNum, long clinicNum = -1)
    {
        var command = "SELECT * FROM alertsub WHERE UserNum=" + SOut.Long(userNum);
        if (clinicNum != -1) command += " AND ClinicNum=" + SOut.Long(clinicNum);
        return AlertSubCrud.SelectMany(command);
    }
    
    public static void Sync(List<AlertSub> listAlertSubsNew, List<AlertSub> listAlertSubsOld)
    {
        AlertSubCrud.Sync(listAlertSubsNew, listAlertSubsOld);
    }

    public static List<AlertType> GetAllAlertTypesForUser(long userNum)
    {
        //Get all AlertCategoryNums for the user's AlertSubs.
        var listAlertCategoryNums = GetAllForUser(userNum).Select(x => x.AlertCategoryNum).ToList();
        //Get all links between the AlertCategories and AlertTypes.
        var listAlertCategoryLinks = AlertCategoryLinks.GetWhere(x => listAlertCategoryNums.Contains(x.AlertCategoryNum));
        //Return all distinct AlertTypes associated to links.
        return listAlertCategoryLinks.Select(x => x.AlertType).Distinct().ToList();
    }
}