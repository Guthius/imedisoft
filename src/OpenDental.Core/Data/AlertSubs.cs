using System.Collections.Generic;
using System.Linq;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class AlertSubs
{
    public static List<AlertSub> GetAllForUser(long userNum, long clinicNum = -1)
    {
        var commandText = "SELECT * FROM alertsub WHERE UserNum = " + userNum;

        if (clinicNum != -1)
        {
            commandText += " AND ClinicNum = " + clinicNum;
        }

        return AlertSubCrud.SelectMany(commandText);
    }

    public static void Sync(List<AlertSub> listAlertSubsNew, List<AlertSub> listAlertSubsOld)
    {
        AlertSubCrud.Sync(listAlertSubsNew, listAlertSubsOld);
    }

    public static List<AlertType> GetAllAlertTypesForUser(long userNum)
    {
        var alertCategoryNums = GetAllForUser(userNum).Select(x => x.AlertCategoryNum).ToList();

        return AlertCategoryLinks
            .GetWhere(alertCategoryLink => alertCategoryNums.Contains(alertCategoryLink.AlertCategoryNum))
            .Select(alertCategoryLink => alertCategoryLink.AlertType)
            .Distinct()
            .ToList();
    }
}