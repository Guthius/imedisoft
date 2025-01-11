using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EServiceSignals
{
    public const int ECONNECTOR_HEARTBEAT_MINUTES = 5;

    ///<summary>returns all eServiceSignals for a given service within the date range, inclusive.</summary>
    public static List<EServiceSignal> GetServiceHistory(eServiceCode eServiceCode_, DateTime dateStart, DateTime dateStop, int limit = 0)
    {
        var command = "SELECT * FROM eservicesignal "
                      + "WHERE ServiceCode=" + SOut.Int((int) eServiceCode_) + " "
                      + "AND SigDateTime BETWEEN " + SOut.Date(dateStart) + " AND " + SOut.Date(dateStop.Date.AddDays(1)) + " "
                      + "ORDER BY SigDateTime DESC, Severity DESC";
        if (limit > 0) command = DbHelper.LimitOrderBy(command, limit);
        return EServiceSignalCrud.SelectMany(command);
    }

    ///<summary>returns true if any eServiceSignals with the eServiceSignalSeverity.Working have ever been detected.</summary>
    public static bool HasEverHadHeartbeat()
    {
        var command = "SELECT COUNT(*) FROM eservicesignal WHERE Severity=" + SOut.Int((int) eServiceSignalSeverity.Working);
        return Db.GetLong(command) > 0;
    }

    ///<summary>Returns the last known status for the given eService.</summary>
    public static eServiceSignalSeverity GetServiceStatus(eServiceCode eServiceCode_)
    {
        //The only statuses within the eServiceSignalSeverity enum are NotEnabled, Working, and Critical.
        //All other statuses are used for logging purposes and should not be considered within this method.
        var command = "SELECT * FROM eservicesignal WHERE ServiceCode=" + SOut.Int((int) eServiceCode_) + " "
                      + "ORDER BY SigDateTime DESC, Severity DESC ";
        command = DbHelper.LimitOrderBy(command, 1);
        var listEServiceSignals = EServiceSignalCrud.SelectMany(command);
        if (listEServiceSignals.Count == 0)
            //NoSignals exist for this service.
            return eServiceSignalSeverity.None;
        return listEServiceSignals[0].Severity;
    }

    /// <summary>
    ///     Returns the last known status for the Listener Service.
    ///     Returns Critical if a signal has not been entered in the last 5 minutes.
    ///     Returns Error if there are ANY error signals that have not been processed.
    /// </summary>
    public static eServiceSignalSeverity GetListenerServiceStatus()
    {
        //Additionally, this query will run a subselect to get the count of all unprocessed errors.
        //Running that query as a subselect here simply saves an extra call to the database.
        //This subselect should be fine to run here since the query is limited to one result and the count of unprocessed errors should be small.
        var command = "SELECT eservicesignal.*," //eservicesignal.* is required because we will manually call TableToList() later.
                      + "(SELECT COUNT(*) FROM eservicesignal WHERE Severity=" + SOut.Int((int) eServiceSignalSeverity.Error) + " AND IsProcessed=0) PendingErrors, "
                      + DbHelper.Now() + " ServerTime "
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

    ///<summary>Also inserts an EConnectorError Alert where applicable.</summary>
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

    ///<summary>Inserts a healthy heartbeat.</summary>
    public static void InsertHeartbeatForService(eServiceCode eServiceCode_)
    {
        AlertItems.DeleteFor(AlertType.EConnectorDown);
        var command = "SELECT * FROM eservicesignal WHERE ServiceCode=" + SOut.Int((int) eServiceCode_)
                                                                        + " AND Severity IN ("
                                                                        + SOut.Int((int) eServiceSignalSeverity.NotEnabled) + ","
                                                                        + SOut.Int((int) eServiceSignalSeverity.Working) + ","
                                                                        + SOut.Int((int) eServiceSignalSeverity.Critical)
                                                                        + ") ORDER BY SigDateTime DESC " + DbHelper.LimitWhere(1); //only select not enabled, working, and critical statuses.
        var eServiceSignalLast = EServiceSignalCrud.SelectOne(command);
        var dateTNow = MiscData.GetNowDateTime();
        //If initializing or changing state to working from not working, insert two signals; An anchor and a rolling timestamp.
        if (eServiceSignalLast == null || eServiceSignalLast.Severity != eServiceSignalSeverity.Working)
        {
            //First ever heartbeat or critical which was not previously critical.
            if (eServiceSignalLast != null && eServiceSignalLast.Severity == eServiceSignalSeverity.Critical)
            {
                var dateTimeLastUpdate = UpdateHistories.GetLastUpdateHistory()?.DateTimeUpdated ?? DateTime.MinValue;
                //Do not create a signal if the eConnector was stopped because of an update
                if (eServiceSignalLast.SigDateTime > dateTimeLastUpdate || dateTimeLastUpdate.AddMinutes(10) < dateTNow)
                    //Changing from critical to working so alert user that this change took place and tell them how long we were in critical state.
                    //Insert() will also insert Alert.
                    Insert(new EServiceSignal {ServiceCode = (int) eServiceCode_, Severity = eServiceSignalSeverity.Error, SigDateTime = dateTNow, IsProcessed = false, Description = "Listener was critical for " + DateTime.Now.Subtract(eServiceSignalLast.SigDateTime).ToStringHmm()});
            }

            Insert(new EServiceSignal {ServiceCode = (int) eServiceCode_, Severity = eServiceSignalSeverity.Working, SigDateTime = dateTNow, IsProcessed = true, Description = "Heartbeat Anchor"}); //anchor heartbeat
            Insert(new EServiceSignal {ServiceCode = (int) eServiceCode_, Severity = eServiceSignalSeverity.Working, SigDateTime = dateTNow.AddSeconds(1), IsProcessed = true, Description = "Heartbeat"}); //rolling heartbeat
            return;
        }

        eServiceSignalLast.SigDateTime = dateTNow;
        Update(eServiceSignalLast);
    }

    
    public static void Update(EServiceSignal eServiceSignal)
    {
        EServiceSignalCrud.Update(eServiceSignal);
    }

    ///<summary>Sets IsProcessed to true on all eService signals of the passed in severity.</summary>
    public static void ProcessSignalsForSeverity(eServiceSignalSeverity eServiceSignalSeverity_)
    {
        var command = "UPDATE eservicesignal SET IsProcessed=1 WHERE Severity=" + SOut.Int((int) eServiceSignalSeverity_);
        Db.NonQ(command);
        if (eServiceSignalSeverity_ == eServiceSignalSeverity.Error) //Delete corresponding alert.
            AlertItems.DeleteFor(AlertType.EConnectorError);
    }

    /// <summary>
    ///     Sets IsProcessed to true on eService signals of Error severity that are within 15 minutes of the passed in
    ///     DateTime.
    /// </summary>
    public static void ProcessErrorSignalsAroundTime(DateTime dateTime)
    {
        if (dateTime.Year < 1880) return; //Nothing to do.
        var command = "UPDATE eservicesignal SET IsProcessed=1 "
                      + "WHERE Severity=" + SOut.Int((int) eServiceSignalSeverity.Error) + " "
                      + "AND SigDateTime BETWEEN " + SOut.DateT(dateTime.AddMinutes(-15)) + " AND " + SOut.DateT(dateTime.AddMinutes(15));
        Db.NonQ(command);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<EServiceSignal> Refresh(long patNum){

        string command="SELECT * FROM eservicesignal WHERE PatNum = "+POut.Long(patNum);
        return Crud.EServiceSignalCrud.SelectMany(command);
    }

    ///<summary>Gets one EServiceSignal from the db.</summary>
    public static EServiceSignal GetOne(long eServiceSignalNum){

        return Crud.EServiceSignalCrud.SelectOne(eServiceSignalNum);
    }

    
    public static long Insert(EServiceSignal eServiceSignal){

        return Crud.EServiceSignalCrud.Insert(eServiceSignal);
    }

    
    public static void Update(EServiceSignal eServiceSignal){

        Crud.EServiceSignalCrud.Update(eServiceSignal);
    }

    
    public static void Delete(long eServiceSignalNum) {

        string command= "DELETE FROM eservicesignal WHERE EServiceSignalNum = "+POut.Long(eServiceSignalNum);
        Db.NonQ(command);
    }
    */
}