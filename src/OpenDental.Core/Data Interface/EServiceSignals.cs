using System;
using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class EServiceSignals
{
    public const int ECONNECTOR_HEARTBEAT_MINUTES = 5;

    public static List<EServiceSignal> GetServiceHistory(eServiceCode eServiceCode_, DateTime dateStart, DateTime dateStop, int limit = 0)
    {
        var command = "SELECT * FROM eservicesignal "
                      + "WHERE ServiceCode=" + SOut.Int((int) eServiceCode_) + " "
                      + "AND SigDateTime BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateStop.Date.AddDays(1)) + " "
                      + "ORDER BY SigDateTime DESC, Severity DESC";
        if (limit > 0) command = DbHelper.LimitOrderBy(command, limit);
        return EServiceSignalCrud.SelectMany(command);
    }

    public static bool HasEverHadHeartbeat()
    {
        var command = "SELECT COUNT(*) FROM eservicesignal WHERE Severity=" + SOut.Int((int) eServiceSignalSeverity.Working);
        return Db.GetLong(command) > 0;
    }

    public static eServiceSignalSeverity GetListenerServiceStatus()
    {
        //Additionally, this query will run a subselect to get the count of all unprocessed errors.
        //Running that query as a subselect here simply saves an extra call to the database.
        //This subselect should be fine to run here since the query is limited to one result and the count of unprocessed errors should be small.
        var command = "SELECT eservicesignal.*," //eservicesignal.* is required because we will manually call TableToList() later.
                      + "(SELECT COUNT(*) FROM eservicesignal WHERE Severity=" + SOut.Int((int) eServiceSignalSeverity.Error) + " AND IsProcessed=0) PendingErrors, "
                      + "NOW()" + " ServerTime "
                      + "FROM eservicesignal WHERE ServiceCode=" + SOut.Int((int) eServiceCode.ListenerService) + " "
                      + "AND Severity IN(" + SOut.Int((int) eServiceSignalSeverity.NotEnabled) + ","
                      + SOut.Int((int) eServiceSignalSeverity.Working) + ","
                      + SOut.Int((int) eServiceSignalSeverity.Error) + ","
                      + SOut.Int((int) eServiceSignalSeverity.Critical) + ") "
                      + "ORDER BY SigDateTime DESC, Severity DESC ";
        command = DbHelper.LimitOrderBy(command, 1);
        var table = DataCore.GetTable(command);
        var listEServiceSignals = EServiceSignalCrud.TableToList(table);
        if (listEServiceSignals.Count == 0) //No signals means the eConnector has never run. Nothing to report.				
            return eServiceSignalSeverity.None;
        if (listEServiceSignals[0].Severity == eServiceSignalSeverity.NotEnabled) //NotEnabled means they don't care what the status is. Nothing to report.
            return eServiceSignalSeverity.NotEnabled;
        var dateTimeNow = SIn.DateTime(table.Rows[0]["ServerTime"].ToString());
        if (
            //eConnector exited gracefully and inserted its own critical signal.
            listEServiceSignals[0].Severity == eServiceSignalSeverity.Critical
            //eConnector did not exit gracefully but has not inserted a heartbeat in at least 6 minutes. It is considered critical.
            //Listener is dropping a heartbeat every 5 minutes, so give 1 minute grace period to squelch race condition.
            || listEServiceSignals[0].SigDateTime < dateTimeNow.AddMinutes(-(ECONNECTOR_HEARTBEAT_MINUTES + 1)))
            return eServiceSignalSeverity.Critical;
        //We need to flag the service monitor as Error if there are ANY pending errors.
        if (table.Rows[0]["PendingErrors"].ToString() != "0") return eServiceSignalSeverity.Error;
        return listEServiceSignals[0].Severity;
    }

    public static long Insert(EServiceSignal eServiceSignal)
    {
        //If this is an error and the EConnectorError alert is not already present, create it now.
        if (eServiceSignal.Severity == eServiceSignalSeverity.Error && AlertItems.RefreshForType(AlertType.EConnectorError).Count == 0)
        {
            //Create an alert.
            var alertItem = new AlertItem();
            //Do not allow delete. The only way for this alert to be deleted is to open the eConnector form and ACK the error(s).
            alertItem.Actions = ActionType.MarkAsRead | ActionType.OpenForm;
            alertItem.Description = Lans.g("EConnector", "eConnector has posted an error.");
            alertItem.Severity = SeverityType.Low;
            alertItem.Type = AlertType.EConnectorError;
            alertItem.FormToOpen = FormType.FormEServicesEConnector;
            AlertItems.Insert(alertItem);
        }

        return EServiceSignalCrud.Insert(eServiceSignal);
    }

    public static void ProcessSignalsForSeverity(eServiceSignalSeverity eServiceSignalSeverity_)
    {
        var command = "UPDATE eservicesignal SET IsProcessed=1 WHERE Severity=" + SOut.Int((int) eServiceSignalSeverity_);
        Db.NonQ(command);
        if (eServiceSignalSeverity_ == eServiceSignalSeverity.Error) //Delete corresponding alert.
            AlertItems.DeleteFor(AlertType.EConnectorError);
    }

    public static void ProcessErrorSignalsAroundTime(DateTime dateTime)
    {
        if (dateTime.Year < 1880) return; //Nothing to do.
        var command = "UPDATE eservicesignal SET IsProcessed=1 "
                      + "WHERE Severity=" + SOut.Int((int) eServiceSignalSeverity.Error) + " "
                      + "AND SigDateTime BETWEEN " + SOut.DateTime(dateTime.AddMinutes(-15)) + " AND " + SOut.DateTime(dateTime.AddMinutes(15));
        Db.NonQ(command);
    }
}