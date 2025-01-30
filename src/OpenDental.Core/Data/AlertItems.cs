using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class AlertItems
{
    public static void CreateGenericAlert(string description, string itemValue)
    {
        Insert(new AlertItem
        {
            Type = AlertType.Generic,
            Actions = ActionType.MarkAsRead | ActionType.Delete | ActionType.ShowItemValue,
            Description = description,
            Severity = SeverityType.Low,
            ItemValue = itemValue
        });
    }

    public static List<AlertItem> RefreshForClinicAndTypes(long clinicNum, List<AlertType> listAlertTypes = null)
    {
        if (listAlertTypes == null || listAlertTypes.Count == 0)
        {
            return [];
        }

        long provNum = 0;
        if (Security.CurUser != null && Userods.IsUserCpoe(Security.CurUser))
        {
            provNum = Security.CurUser.ProvNum;
        }

        long userNum = 0;
        if (Security.CurUser != null)
        {
            userNum = Security.CurUser.UserNum;
        }
        
        return AlertItemCrud.SelectMany(
            "SELECT * FROM alertitem " +
            "WHERE Type IN (" + string.Join(",", listAlertTypes.Cast<int>()) + ") " +
            "AND (UserNum=0 OR UserNum=" + userNum + ") " +
            "AND (CASE TYPE WHEN " + (int) AlertType.RadiologyProcedures + " THEN FKey=" + provNum + " " +
            "ELSE ClinicNum = " + clinicNum + " OR ClinicNum=-1 END)");
    }

    public static List<AlertItem> RefreshForType(AlertType alertType)
    {
        return AlertItemCrud.SelectMany("SELECT * FROM alertitem WHERE Type=" + (int) alertType);
    }

    public static List<AlertItem> GetAllForUserNum(long userNum)
    {
        return AlertItemCrud.SelectMany("SELECT * FROM alertitem WHERE UserNum=" + userNum);
    }

    public static void Insert(AlertItem alertItem)
    {
        AlertItemCrud.Insert(alertItem);
    }

    public static void DeleteFor(AlertType alertType, List<long> listFKeys = null)
    {
        var alerts = RefreshForType(alertType);

        if (listFKeys != null)
        {
            alerts = alerts.FindAll(x => listFKeys.Contains(x.FKey));
        }

        foreach (var alert in alerts) Delete(alert.AlertItemNum);
    }

    public static void Delete(long alertItemNum)
    {
        Delete([alertItemNum]);
    }

    public static void Delete(List<long> alertItemNums)
    {
        if (alertItemNums.IsNullOrEmpty())
        {
            return;
        }

        AlertReads.DeleteForAlertItems(alertItemNums);

        Db.NonQ("DELETE FROM alertitem WHERE AlertItemNum IN (" + string.Join(",", alertItemNums) + ")");
    }

    public static void CheckOdServiceHeartbeat()
    {
        if (IsOdServiceRunning())
        {
            return;
        }

        var alertItemsOld = RefreshForType(AlertType.OpenDentalServiceDown);
        if (alertItemsOld.Count == 0)
        {
            Insert(new AlertItem
            {
                Actions = ActionType.MarkAsRead,
                ClinicNum = -1,
                Description = "No instance of Open Dental Service is running.",
                Type = AlertType.OpenDentalServiceDown,
                Severity = SeverityType.Medium
            });
        }
    }

    public static bool IsOdServiceRunning()
    {
        var dataTable = DataCore.GetTable("SELECT ValueString,NOW() FROM preference WHERE PrefName='OpenDentalServiceHeartbeat'");
        var dateTimeLastHeartbeat = SIn.DateTime(dataTable.Rows[0][0].ToString());
        var dateTimeNow = SIn.DateTime(dataTable.Rows[0][1].ToString());

        return dateTimeLastHeartbeat.AddMinutes(6) >= dateTimeNow;
    }

    public static List<AlertItem> GetAlertsItemsForUser(long userNum, long clinicNum)
    {
        var alertSubsForUser = AlertSubs.GetAllForUser(userNum);
        var allClinics = alertSubsForUser.Any(x => x.ClinicNum == -1);

        List<long> alertCategoryNums;

        if (allClinics)
        {
            alertCategoryNums = alertSubsForUser
                .Select(x => x.AlertCategoryNum)
                .Distinct()
                .ToList();
        }
        else
        {
            alertCategoryNums = alertSubsForUser
                .Where(x => x.ClinicNum == clinicNum)
                .Select(y => y.AlertCategoryNum)
                .ToList();
        }

        var listAlertTypesForUser = AlertCategoryLinks
            .GetWhere(x => alertCategoryNums.Contains(x.AlertCategoryNum))
            .Select(x => x.AlertType)
            .ToList();

        var alertCategoryNumsAll = alertSubsForUser.Select(y => y.AlertCategoryNum).ToList();

        var alertTypesAll = AlertCategoryLinks
            .GetWhere(x => alertCategoryNumsAll.Contains(x.AlertCategoryNum))
            .Select(x => x.AlertType)
            .ToList();

        return RefreshForClinicAndTypes(clinicNum, listAlertTypesForUser)
            .Union(RefreshForClinicAndTypes(-1, alertTypesAll))
            .Union(GetAllForUserNum(userNum))
            .DistinctBy(x => x.AlertItemNum)
            .ToList();
    }

    public static bool AreDuplicates(AlertItem alertItem1, AlertItem alertItem2)
    {
        if (alertItem1 == null || alertItem2 == null)
        {
            return false;
        }

        return alertItem1.Actions == alertItem2.Actions &&
               alertItem1.ClinicNum == alertItem2.ClinicNum &&
               alertItem1.Description == alertItem2.Description &&
               alertItem1.FKey == alertItem2.FKey &&
               alertItem1.FormToOpen == alertItem2.FormToOpen &&
               alertItem1.ItemValue == alertItem2.ItemValue &&
               alertItem1.Severity == alertItem2.Severity &&
               alertItem1.Type == alertItem2.Type &&
               alertItem1.UserNum == alertItem2.UserNum;
    }
}