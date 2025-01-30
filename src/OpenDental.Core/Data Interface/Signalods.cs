using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Signalods
{
    public static List<Signalod> RefreshTimed(DateTime dateTSince, List<InvalidType> listInvalidTypes = null, List<InvalidType> listInvalidTypesExclude = null)
    {
        //This command was written to take into account the fact that MySQL truncates seconds to the the whole second on DateTime columns. (newer versions support fractional seconds)
        //By selecting signals less than Now() we avoid missing signals the next time this function is called. Without the addition of Now() it was possible
        //to miss up to ((N-1)/N)% of the signals generated in the worst case scenario.
        var command = "SELECT * FROM signalod "
                      + "WHERE (SigDateTime>" + SOut.DateTime(dateTSince) + " AND SigDateTime< " + "NOW()" + ") ";
        if (!listInvalidTypes.IsNullOrEmpty()) command += "AND IType IN(" + string.Join(",", listInvalidTypes.Select(x => (int) x)) + ") ";
        if (!listInvalidTypesExclude.IsNullOrEmpty()) command += "AND IType NOT IN(" + string.Join(",", listInvalidTypesExclude.Select(x => (int) x)) + ") ";
        command += "ORDER BY SigDateTime";
        //note: this might return an occasional row that has both times newer.
        var listSignalods = new List<Signalod>();
        try
        {
            listSignalods = SignalodCrud.SelectMany(command);
        }
        catch
        {
            //we don't want an error message to show, because that can cause a cascade of a large number of error messages.
        }

        return listSignalods;
    }

    public static long Insert(params Signalod[] signalodArray)
    {
        if (signalodArray == null || signalodArray.Length < 1) return 0;
        if (signalodArray.Length == 1) return SignalodCrud.Insert(signalodArray[0]);
        SignalodCrud.InsertMany(signalodArray.ToList());
        return 0;
    }

    public static long SetInvalid(InvalidType invalidType, KeyType fKeyType, long fKey)
    {
        //Remoting role check performed in the Insert.
        var signalod = new Signalod();
        signalod.IType = invalidType;
        signalod.DateViewing = DateTime.MinValue;
        signalod.FKey = fKey;
        signalod.FKeyType = fKeyType;
        return Insert(signalod);
    }

    public static void SetInvalidAppt(Appointment appointmentNew, Appointment appointmentOld = null)
    {
        if (appointmentNew == null)
        {
            if (appointmentOld == null) return; //should never happen. Both apptNew and apptOld are null in this scenario
            //If apptOld is not null then use it as the apptNew so we can send signals
            //Most likely occurred due to appointment delete.
            appointmentNew = appointmentOld;
            appointmentOld = null;
        }

        var addSigForNewApt = IsApptInRefreshRange(appointmentNew);
        var addSignForOldAppt = IsApptInRefreshRange(appointmentOld);
        //The eight possible signals are:
        //  1.New Provider
        //  2.New Hyg
        //  3.New Op
        //  4.Old Provider
        //  5.Old Hyg
        //  6.Old Op
        //  7.New Appt
        //  8.Old Appt
        //If there is no change between new and old, or if there is not an old appt provided, then fewer than 8 signals may be generated.
        var listSignalods = new List<Signalod>();
        if (addSigForNewApt)
        {
            //  1.New Provider
            var signalodProv = new Signalod();
            signalodProv.DateViewing = appointmentNew.AptDateTime;
            signalodProv.IType = InvalidType.Appointment;
            signalodProv.FKey = appointmentNew.ProvNum;
            signalodProv.FKeyType = KeyType.Provider;
            listSignalods.Add(signalodProv);
            //  2.New Hyg
            if (appointmentNew.ProvHyg > 0)
            {
                var signalodHyg = new Signalod();
                signalodHyg.DateViewing = appointmentNew.AptDateTime;
                signalodHyg.IType = InvalidType.Appointment;
                signalodHyg.FKey = appointmentNew.ProvHyg;
                signalodHyg.FKeyType = KeyType.Provider;
                listSignalods.Add(signalodHyg);
            }

            //  3.New Op
            if (appointmentNew.Op > 0)
            {
                var signalodOp = new Signalod();
                signalodOp.DateViewing = appointmentNew.AptDateTime;
                signalodOp.IType = InvalidType.Appointment;
                signalodOp.FKey = appointmentNew.Op;
                signalodOp.FKeyType = KeyType.Operatory;
                listSignalods.Add(signalodOp);
            }

            //  7.New Appt
            if (appointmentNew != null)
            {
                var signalodAppt = new Signalod();
                signalodAppt.DateViewing = appointmentNew.AptDateTime;
                signalodAppt.IType = InvalidType.Appointment;
                signalodAppt.FKey = appointmentNew.PatNum;
                signalodAppt.FKeyType = KeyType.PatNum;
                listSignalods.Add(signalodAppt);
            }
        }

        if (addSignForOldAppt)
        {
            //  4.Old Provider
            if (appointmentOld != null && appointmentOld.ProvNum > 0 && (appointmentOld.AptDateTime.Date != appointmentNew.AptDateTime.Date || appointmentOld.ProvNum != appointmentNew.ProvNum))
            {
                var signalodProvOld = new Signalod();
                signalodProvOld.DateViewing = appointmentOld.AptDateTime;
                signalodProvOld.IType = InvalidType.Appointment;
                signalodProvOld.FKey = appointmentOld.ProvNum;
                signalodProvOld.FKeyType = KeyType.Provider;
                listSignalods.Add(signalodProvOld);
            }

            //  5.Old Hyg
            if (appointmentOld != null && appointmentOld.ProvHyg > 0 && (appointmentOld.AptDateTime.Date != appointmentNew.AptDateTime.Date || appointmentOld.ProvHyg != appointmentNew.ProvHyg))
            {
                var signalodHygOld = new Signalod();
                signalodHygOld.DateViewing = appointmentOld.AptDateTime;
                signalodHygOld.IType = InvalidType.Appointment;
                signalodHygOld.FKey = appointmentOld.ProvHyg;
                signalodHygOld.FKeyType = KeyType.Provider;
                listSignalods.Add(signalodHygOld);
            }

            //  6.Old Op
            if (appointmentOld != null && appointmentOld.Op > 0 && (appointmentOld.AptDateTime.Date != appointmentNew.AptDateTime.Date || appointmentOld.Op != appointmentNew.Op))
            {
                var signalodOpOld = new Signalod();
                signalodOpOld.DateViewing = appointmentOld.AptDateTime;
                signalodOpOld.IType = InvalidType.Appointment;
                signalodOpOld.FKey = appointmentOld.Op;
                signalodOpOld.FKeyType = KeyType.Operatory;
                listSignalods.Add(signalodOpOld);
            }

            //  8.Old Appt
            if (appointmentOld != null && appointmentOld.AptDateTime.Date != appointmentNew.AptDateTime.Date)
            {
                var signalodApptOld = new Signalod();
                signalodApptOld.DateViewing = appointmentOld.AptDateTime;
                signalodApptOld.IType = InvalidType.Appointment;
                signalodApptOld.FKey = appointmentOld.PatNum;
                signalodApptOld.FKeyType = KeyType.PatNum;
                listSignalods.Add(signalodApptOld);
            }
        }

        for (var i = 0; i < listSignalods.Count; i++) Insert(listSignalods[i]);
        //There was a delay when using this method to refresh the appointment module due to the time it takes to loop through the signals that iSignalProcessors need to loop through.
        //BroadcastSignals(listSignals);//for immediate update. Signals will be processed again at next tick interval.
    }

    public static bool IsApptInRefreshRange(Appointment appointment)
    {
        if (appointment == null) return false;
        var days = PrefC.GetInt(PrefName.ApptAutoRefreshRange);
        if (days == -1)
            //ApptAutoRefreshRange preference is -1, so all appointments are in range
            return true;
        //Returns true if the appointment is between today and today + the auto refresh day range preference.
        return appointment.AptDateTime.Between(DateTime.Today, DateTime.Today.AddDays(days));
    }

    public static void SetInvalidSchedForOps(List<Schedule> listSchedules)
    {
        var listSignalods = new List<Signalod>();
        for (var i = 0; i < listSchedules.Count; i++)
            //All three places that call this just use a single op in their op list.
            //But this is a little more future proof.
        for (var j = 0; j < listSchedules[i].Ops.Count; j++)
        {
            var signalodForOp = new Signalod();
            signalodForOp.IType = InvalidType.Schedules;
            signalodForOp.DateViewing = listSchedules[i].SchedDate;
            signalodForOp.FKey = listSchedules[i].Ops[j];
            signalodForOp.FKeyType = KeyType.Operatory;
            listSignalods.Add(signalodForOp);
        }

        Insert(listSignalods.ToArray());
    }

    public static void SetInvalidSched(params Schedule[] scheduleArray)
    {
        //Per Nathan, we are only going to insert signals for today's schedules. Most workstations will not be looking at other days for extended
        //lengths of time.
        //Make a list of signals for every operatory involved.
        var dateTimeServer = MiscData.GetNowDateTime();
        var listSchedules = scheduleArray.ToList();
        var listSchedulesToday = listSchedules.Where(x => x.SchedDate.Date == DateTime.Today || x.SchedDate.Date == dateTimeServer.Date).ToList();
        var listSignalods = new List<Signalod>();
        for (var i = 0; i < listSchedulesToday.Count; i++)
        {
            var listOpNums = listSchedulesToday[i].Ops;
            for (var j = 0; j < listOpNums.Count; j++)
            {
                var signalodOp = new Signalod();
                signalodOp.IType = InvalidType.Schedules;
                signalodOp.DateViewing = listSchedulesToday[i].SchedDate;
                signalodOp.FKey = listOpNums[j];
                signalodOp.FKeyType = KeyType.Operatory;
                listSignalods.Add(signalodOp);
            }
        }

        //Make a list of signals for every provider involved.
        var listSchedulesProvider = scheduleArray
            .Where(x => x.ProvNum > 0 && (x.SchedDate.Date == DateTime.Today || x.SchedDate.Date == dateTimeServer.Date)).ToList();
        var listSignalodsProvider = new List<Signalod>();
        for (var i = 0; i < listSchedulesProvider.Count; i++)
        {
            var signalodProvider = new Signalod();
            signalodProvider.IType = InvalidType.Schedules;
            signalodProvider.DateViewing = listSchedulesProvider[i].SchedDate;
            signalodProvider.FKey = listSchedulesProvider[i].ProvNum;
            signalodProvider.FKeyType = KeyType.Provider;
            listSignalodsProvider.Add(signalodProvider);
        }

        var listSignalodsUnique = listSignalods.Union(listSignalodsProvider).ToList();
        if (listSignalodsUnique.Count <= 1000)
        {
            Insert(listSignalodsUnique.ToArray());
            return;
        }

        //We've had offices insert tens of thousands of signals at once which severely slowed down their database.
        var signalod = new Signalod();
        signalod.IType = InvalidType.Schedules;
        signalod.DateViewing = DateTime.MinValue; //This will cause every workstation to refresh regardless of what they're viewing.
        Insert(signalod);
    }

    public static void SetInvalidSched(DateTime dateViewing)
    {
        if (dateViewing == DateTime.MinValue) return; //A dateViewing of 01-01-0001 will be ignored because it would otherwise cause a full refresh for all connected client workstations.
        var signalod = new Signalod();
        signalod.IType = InvalidType.Schedules;
        signalod.DateViewing = dateViewing;
        Insert(signalod);
    }

    public static List<SmsFromMobiles.SmsNotification> UpsertSmsNotification()
    {
        var command = "SELECT ClinicNum,COUNT(*) AS CountUnread FROM smsfrommobile WHERE SmsStatus=0 AND IsHidden=0 GROUP BY ClinicNum "
                      + "ORDER BY ClinicNum";
        var tableSmsFromMobile = DataCore.GetTable(command);
        var listSmsNotifications = new List<SmsFromMobiles.SmsNotification>();
        for (var i = 0; i < tableSmsFromMobile.Rows.Count; i++)
        {
            var smsNotification = new SmsFromMobiles.SmsNotification();
            smsNotification.ClinicNum = SIn.Long(tableSmsFromMobile.Rows[i]["ClinicNum"].ToString());
            smsNotification.Count = SIn.Int(tableSmsFromMobile.Rows[i]["CountUnread"].ToString());
            listSmsNotifications.Add(smsNotification);
        }

        //Insert as structured data signal so all workstations won't have to query the db to get the counts. They will get it directly from Signalod.MsgValue.
        var json = SmsFromMobiles.SmsNotification.GetJsonFromList(listSmsNotifications);
        //FKeyType SmsMsgUnreadCount is written to db as a string. 
        command = "SELECT * FROM signalod WHERE IType=" + SOut.Int((int) InvalidType.SmsTextMsgReceivedUnreadCount)
                                                        + " AND FKeyType='" + SOut.String(KeyType.SmsMsgUnreadCount.ToString()) + "' ORDER BY SigDateTime DESC LIMIT 1";
        var table = DataCore.GetTable(command);
        var signalod = SignalodCrud.TableToList(table).FirstOrDefault();
        if (signalod != null && signalod.MsgValue == json) //No changes, not need to insert a new signal.
            return listSmsNotifications; //Return the list of notifications, but do not update the existing signal.
        var signalodNew = new Signalod();
        signalodNew.IType = InvalidType.SmsTextMsgReceivedUnreadCount;
        signalodNew.FKeyType = KeyType.SmsMsgUnreadCount;
        signalodNew.MsgValue = json;
        Insert(signalodNew);
        return listSmsNotifications;
    }

    public static bool IsApptRefreshNeeded(DateTime dateTimeShowing, List<Signalod> listSignalods, List<long> listOpNumsVisible, List<long> listProvNumsVisible)
    {
        return IsApptRefreshNeeded(dateTimeShowing, dateTimeShowing, listSignalods, listOpNumsVisible, listProvNumsVisible);
    }

    public static bool IsApptRefreshNeeded(DateTime dateStart, DateTime dateEnd, List<Signalod> listSignalods, List<long> listOpNumsVisible, List<long> listProvNumsVisible)
    {
        //A date range was refreshed.  Easier to refresh all without checking.
        if (listSignalods.Exists(x => (x.DateViewing.Date == DateTime.MinValue.Date || x.FKeyType == KeyType.PatNum) && x.IType == InvalidType.Appointment)) return true;
        var listSignalodsAppt = listSignalods.FindAll(x => x.IType == InvalidType.Appointment &&
                                                           x.DateViewing.Date >= dateStart.Date && x.DateViewing.Date <= dateEnd.Date);
        if (listSignalodsAppt.Count == 0) return false;
        //List<long> visibleOps = ApptDrawing.VisOps.Select(x => x.OperatoryNum).ToList();
        //List<long> visibleProvs = ApptDrawing.VisProvs.Select(x => x.ProvNum).ToList();
        if (listSignalodsAppt.Any(x => x.FKeyType == KeyType.Operatory && listOpNumsVisible.Contains(x.FKey))
            || listSignalodsAppt.Any(x => x.FKeyType == KeyType.Provider && listProvNumsVisible.Contains(x.FKey)))
            return true;
        return false;
    }

    public static bool IsSchedRefreshNeeded(DateTime dateTimeShowing, List<Signalod> listSignalods, List<long> listOpNumsVisible, List<long> listProvNumsVisible)
    {
        return IsSchedRefreshNeeded(dateTimeShowing, dateTimeShowing, listSignalods, listOpNumsVisible, listProvNumsVisible);
    }

    public static bool IsSchedRefreshNeeded(DateTime dateStart, DateTime dateEnd, List<Signalod> listSignalods, List<long> listOpNumsVisible, List<long> listProvNumsVisible)
    {
        //A date range was refreshed.  Easier to refresh all without checking.
        if (listSignalods.Exists(x => x.DateViewing.Date == DateTime.MinValue.Date && x.IType == InvalidType.Schedules)) return true;
        var listSignalodsSched = listSignalods.FindAll(x => x.IType == InvalidType.Schedules &&
                                                            x.DateViewing.Date >= dateStart.Date && x.DateViewing.Date <= dateEnd.Date);
        if (listSignalodsSched.Count == 0) return false;
        if (listSignalodsSched.Any(x => x.FKeyType == KeyType.Operatory && listOpNumsVisible.Contains(x.FKey))
            || listSignalodsSched.Any(x => x.FKeyType == KeyType.Provider && listProvNumsVisible.Contains(x.FKey))
            || listSignalodsSched.Any(x => x.FKeyType == KeyType.Undefined)) //For blockouts cleared on a single day.
            return true;
        return false;
    }

    public static bool IsContrApptButtonRefreshNeeded(List<Signalod> listSignalods)
    {
        if (listSignalods.Exists(x => x.IType == InvalidType.Defs)) return true;
        return false;
    }

    public static InvalidType[] GetInvalidTypes(List<Signalod> listSignalods)
    {
        var invalidTypeArray = listSignalods.FindAll(x => x.IType != InvalidType.Task
                                                          && x.IType != InvalidType.TaskPopup
                                                          && x.IType != InvalidType.SmsTextMsgReceivedUnreadCount
                                                          && x.FKey == 0
                                                          && x.FKeyType == KeyType.Undefined)
            .Select(x => x.IType).ToArray();
        return invalidTypeArray;
    }

    public static void SetInvalid(params InvalidType[] invalidTypeArray)
    {
        for (var i = 0; i < invalidTypeArray.Length; i++)
        {
            var signalod = new Signalod();
            signalod.IType = invalidTypeArray[i];
            signalod.DateViewing = DateTime.MinValue;
            switch (invalidTypeArray[i])
            {
                case InvalidType.UserOdPrefs:
                    signalod.FKey = Security.CurUser?.UserNum ?? 0;
                    signalod.FKeyType = KeyType.UserOd;
                    break;
            }

            Insert(signalod);
        }
    }

    public static void ClearOldSignals()
    {
        var dateTimeServer = MiscData.GetNowDateTime();
        if (Prefs.GetContainsKey(PrefName.SignalLastClearedDate.ToString())
            && PrefC.GetDateT(PrefName.SignalLastClearedDate) > dateTimeServer.AddDays(-7) //Has already been run in the past week. This is all server based time.
            && PrefC.GetDateT(PrefName.SignalLastClearedDate) < dateTimeServer) //SignalLastClearedDate isn't in the future job 46490
            return; //Do not run this process again.
        Prefs.UpdateDateT(PrefName.SignalLastClearedDate, dateTimeServer); //Set Last cleared to now.
        var command = "";
        //easier to read that using the DbHelper Functions and it also matches the ConvertDB3 script
        command = "DELETE FROM signalod WHERE SigDateTime < DATE_ADD(NOW(),INTERVAL -2 DAY)"; //Itypes only older than 2 days
        Db.NonQ(command);

        SigMessages.ClearOldSigMessages(); //Clear messaging buttons which use to be stored in the signal table.
        //SigElements.DeleteOrphaned();
    }

    public static DateTime DateTRegularPrioritySignalLastRefreshed;
    public static DateTime DateTHighPrioritySignalLastRefreshed;
    public static DateTime DateTApptSignalLastRefreshed;
}