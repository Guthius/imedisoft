using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class AlertItems
{
    public static void CreateGenericAlert(string description, string itemValue)
    {
        var alertItem = new AlertItem();
        alertItem.Type = AlertType.Generic;
        alertItem.Actions = ActionType.MarkAsRead | ActionType.Delete | ActionType.ShowItemValue;
        alertItem.Description = description;
        alertItem.Severity = SeverityType.Low;
        alertItem.ItemValue = itemValue;
        Insert(alertItem);
    }
    
    public static List<AlertItem> RefreshForClinicAndTypes(long clinicNum, List<AlertType> listAlertTypes = null)
    {
        if (listAlertTypes == null || listAlertTypes.Count == 0) return new List<AlertItem>();
        long provNum = 0;
        if (Security.CurUser != null && Userods.IsUserCpoe(Security.CurUser)) provNum = Security.CurUser.ProvNum;
        long userNum = 0;
        if (Security.CurUser != null) userNum = Security.CurUser.UserNum;
        var command = "SELECT * FROM alertitem "
                      + "WHERE Type IN (" + string.Join(",", listAlertTypes.Cast<int>().ToList()) + ") "
                      + "AND (UserNum=0 OR UserNum=" + SOut.Long(userNum) + ") "
                      //For AlertType.RadiologyProcedures we only care if the alert is associated to the current logged in provider.
                      //When provNum is 0 the initial WHEN check below will not bring any rows by definition of the FKey column.
                      + "AND (CASE TYPE WHEN " + SOut.Int((int) AlertType.RadiologyProcedures) + " THEN FKey=" + SOut.Long(provNum) + " "
                      + "ELSE ClinicNum = " + SOut.Long(clinicNum) + " OR ClinicNum=-1 END)";
        return AlertItemCrud.SelectMany(command);
    }

    public static List<AlertItem> RefreshForType(AlertType alertType)
    {
        var command = "SELECT * FROM alertitem WHERE Type=" + SOut.Int((int) alertType) + ";";
        return AlertItemCrud.SelectMany(command);
    }

    public static List<AlertItem> GetAllForUserNum(long userNum)
    {
        var command = "SELECT * FROM alertitem WHERE UserNum=" + SOut.Long(userNum);
        return AlertItemCrud.SelectMany(command);
    }
    
    public static void Insert(AlertItem alertItem)
    {
        AlertItemCrud.Insert(alertItem);
    }

    public static void DeleteFor(AlertType alertType, List<long> listFKeys = null)
    {
        var listAlerts = RefreshForType(alertType);
        if (listFKeys != null) //Narrow down to just the FKeys provided.
            listAlerts = listAlerts.FindAll(x => listFKeys.Contains(x.FKey));
        foreach (var alert in listAlerts) Delete(alert.AlertItemNum);
    }

    public static void Delete(long alertItemNum)
    {
        Delete(new List<long> {alertItemNum});
    }

    public static void Delete(List<long> listAlertItemNums)
    {
        if (listAlertItemNums.IsNullOrEmpty()) return;
        AlertReads.DeleteForAlertItems(listAlertItemNums);
        var command = "DELETE FROM alertitem WHERE AlertItemNum IN(" + string.Join(",", listAlertItemNums.Select(SOut.Long)) + ")";
        Db.NonQ(command);
    }

    public static void CheckODServiceHeartbeat()
    {
        if (!IsODServiceRunning())
        {
            //If the heartbeat is over 6 minutes old, send the alert if it does not already exist
            //Check if there are any previous alert items
            //Get previous alerts of this type
            var listAlertItemsOld = RefreshForType(AlertType.OpenDentalServiceDown);
            if (listAlertItemsOld.Count == 0)
            {
                //an alert does not already exist
                var alertItem = new AlertItem();
                alertItem.Actions = ActionType.MarkAsRead;
                alertItem.ClinicNum = -1; //all clinics
                alertItem.Description = Lans.g("Alerts", "No instance of Open Dental Service is running.");
                alertItem.Type = AlertType.OpenDentalServiceDown;
                alertItem.Severity = SeverityType.Medium;
                Insert(alertItem);
            }
        }
    }

    public static bool IsODServiceRunning()
    {
        var command = "SELECT ValueString,NOW() FROM preference WHERE PrefName='OpenDentalServiceHeartbeat'";
        var table = DataCore.GetTable(command);
        var dateTimeLastHeartbeat = SIn.DateTime(table.Rows[0][0].ToString());
        var dateTimeNow = SIn.DateTime(table.Rows[0][1].ToString());
        if (dateTimeLastHeartbeat.AddMinutes(6) < dateTimeNow) return false;
        return true;
    }

    public static List<AlertItem> GetAlertsItemsForUser(long userNum, long clinicNum)
    {
        var listAlertSubsForUser = AlertSubs.GetAllForUser(userNum);
        var isAllClinics = listAlertSubsForUser.Any(x => x.ClinicNum == -1);
        var listAlertCategoryNums = new List<long>();
        if (isAllClinics)
        {
            //User subscribed to all clinics.
            listAlertCategoryNums = listAlertSubsForUser.Select(x => x.AlertCategoryNum).Distinct().ToList();
        }
        else
        {
            //List of AlertSubs for current clinic and user combo.
            var listAlertSubsForClinicUser = listAlertSubsForUser.FindAll(x => x.ClinicNum == clinicNum);
            listAlertCategoryNums = listAlertSubsForClinicUser.Select(y => y.AlertCategoryNum).ToList();
        }

        //AlertTypes current user is subscribed to.
        var listAlertTypesForUser = AlertCategoryLinks.GetWhere(x => listAlertCategoryNums.Contains(x.AlertCategoryNum))
            .Select(x => x.AlertType).ToList();
        var listAlertCategoryNumsAll = listAlertSubsForUser.Select(y => y.AlertCategoryNum).ToList();
        //AlertTypes current user is subscribed to for AlertItems which are not clinic specific.
        var listAlertTypesAll = AlertCategoryLinks.GetWhere(x => listAlertCategoryNumsAll.Contains(x.AlertCategoryNum))
            .Select(x => x.AlertType).ToList();
        var listAlertItemsForUser = RefreshForClinicAndTypes(clinicNum, listAlertTypesForUser) //Get alert items for the current clinic
            .Union(RefreshForClinicAndTypes(-1, listAlertTypesAll)) //Get alert items that are for all clinics
            .Union(GetAllForUserNum(userNum)) //Get alert items for the current user
            .DistinctBy(x => x.AlertItemNum).ToList();
        return listAlertItemsForUser;
    }

    public static bool AreDuplicates(AlertItem alertItem1, AlertItem alertItem2)
    {
        if (alertItem1 == null || alertItem2 == null) return false;
        return alertItem1.Actions == alertItem2.Actions
               && alertItem1.ClinicNum == alertItem2.ClinicNum
               && alertItem1.Description == alertItem2.Description
               && alertItem1.FKey == alertItem2.FKey
               && alertItem1.FormToOpen == alertItem2.FormToOpen
               && alertItem1.ItemValue == alertItem2.ItemValue
               && alertItem1.Severity == alertItem2.Severity
               && alertItem1.Type == alertItem2.Type
               && alertItem1.UserNum == alertItem2.UserNum;
    }
}