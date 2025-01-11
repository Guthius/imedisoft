using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Signalods
{
    #region Fields - Private

    ///<summary>SignalNums that this ServerMT instance has processed.</summary>
    private static ConcurrentHashSet<long> _concurrentHashSetSignalNums = new();

    #endregion

    ///<summary> Get the latest signal's SigDateTime from the DB </summary>
    public static DateTime GetLatestSignalTime()
    {
        var command = "SELECT * FROM signalod ";
        command += "ORDER BY SigDateTime DESC ";
        command += "LIMIT 1";
        try
        {
            return SignalodCrud.SelectOne(command).SigDateTime;
        }
        catch
        {
            return MiscData.GetNowDateTime();
        }
    }

    /// <summary>
    ///     Gets all Signals since a given DateTime.  If it can't connect to the database, then it returns a list of length 0.
    ///     Remeber that the supplied dateTime is server time.  This has to be accounted for.
    ///     ListITypes is an optional parameter for querying specific signal types.
    ///     ServerMT instances will always be given a chance to process signals being returned from this method.
    /// </summary>
    public static List<Signalod> RefreshTimed(DateTime dateTSince, List<InvalidType> listInvalidTypes = null, List<InvalidType> listInvalidTypesExclude = null)
    {
        //This command was written to take into account the fact that MySQL truncates seconds to the the whole second on DateTime columns. (newer versions support fractional seconds)
        //By selecting signals less than Now() we avoid missing signals the next time this function is called. Without the addition of Now() it was possible
        //to miss up to ((N-1)/N)% of the signals generated in the worst case scenario.
        var command = "SELECT * FROM signalod "
                      + "WHERE (SigDateTime>" + SOut.DateT(dateTSince) + " AND SigDateTime< " + DbHelper.Now() + ") ";
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

    
    public static List<SignalodForApi> GetSignalOdsForApi(int limit, int offset, DateTime dateTSince, List<InvalidType> listInvalidTypes = null)
    {
        //This command was written to take into account the fact that MySQL truncates seconds to the the whole second on DateTime columns. (newer versions support fractional seconds)
        //By selecting signals less than Now() we avoid missing signals the next time this function is called. Without the addition of Now() it was possible
        //to miss up to ((N-1)/N)% of the signals generated in the worst case scenario.
        var command = "SELECT * FROM signalod "
                      + "WHERE (SigDateTime>" + SOut.DateT(dateTSince) + " AND SigDateTime< " + DbHelper.Now() + ") ";
        if (!listInvalidTypes.IsNullOrEmpty()) command += "AND IType IN(" + string.Join(",", listInvalidTypes.Select(x => (int) x)) + ") ";
        command += "ORDER BY SignalNum "
                   + "LIMIT " + SOut.Int(offset) + ", " + SOut.Int(limit);
        //note: this might return an occasional row that has both times newer.
        var listSignalods = new List<Signalod>();
        var commandDatetime = "SELECT " + DbHelper.Now();
        var dateTimeServer = SIn.DateTime(DataCore.GetScalar(commandDatetime)); //run before signals for rigorous inclusion of signals
        try
        {
            listSignalods = SignalodCrud.SelectMany(command);
        }
        catch
        {
            //we don't want an error message to show, because that can cause a cascade of a large number of error messages.
        }

        var listSignalodForApis = new List<SignalodForApi>();
        for (var i = 0; i < listSignalods.Count; i++)
        {
            //list can be empty
            var signalodForApi = new SignalodForApi();
            signalodForApi.Signalod = listSignalods[i];
            signalodForApi.DateTimeServer = dateTimeServer;
            listSignalodForApis.Add(signalodForApi);
        }

        return listSignalodForApis;
    }

    ///<summary>Queries the database and returns true if we found a shutdown signal</summary>
    public static bool DoesNeedToShutDown(DateTime dateTimeSinceLastChecked)
    {
        var numShutDownSignals = GetCountForTypes(dateTimeSinceLastChecked, InvalidType.ShutDownNow);
        return numShutDownSignals > 0;
    }

    ///<summary>Queries the database and returns true if we found a Sites signal</summary>
    public static bool DoesNeedToRefreshSitesCache(DateTime dateTSinceLastChecked)
    {
        var numSitesSignals = GetCountForTypes(dateTSinceLastChecked, InvalidType.Sites);
        return numSitesSignals > 0;
    }

    public static int GetCountForTypes(DateTime dateTimeSinceLastChecked, params InvalidType[] invalidTypeArray)
    {
        if (invalidTypeArray.IsNullOrEmpty()) return 0;

        //string[] array=;
        var command = $"SELECT COUNT(*) FROM signalod "
                      + $"WHERE SigDateTime>{SOut.DateT(dateTimeSinceLastChecked)} "
                      + $"AND SigDateTime<{DbHelper.Now()} "
                      + $"AND IType IN({string.Join(",", invalidTypeArray.Select(x => SOut.Int((int) x)))})";
        var numSitesSignals = SIn.Int(Db.GetCount(command));
        return numSitesSignals;
    }

    ///<summary>Returns the PK of the signal inserted if only one signal was passed in; Otherwise, returns 0.</summary>
    public static long Insert(params Signalod[] signalodArray)
    {
        if (signalodArray == null || signalodArray.Length < 1) return 0;
        if (signalodArray.Length == 1) return SignalodCrud.Insert(signalodArray[0]);
        SignalodCrud.InsertMany(signalodArray.ToList());
        return 0;
    }

    /// <summary>
    ///     Simplest way to use the new fKey and FKeyType. Set isBroadcast=true to process signals immediately on
    ///     workstation.
    /// </summary>
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

    /// <summary>
    ///     Creates up to 3 signals for each supplied appt.  The signals are needed for many different kinds of changes to
    ///     the appointment, but the signals only specify Provs and Ops because that's what's needed to tell workstations which
    ///     views to refresh.  Always call a refresh of the appointment module before calling this method.  apptNew cannot be
    ///     null.  apptOld is only used when making changes to an existing appt and Provs or Ops have changed. Generally should
    ///     not be called outside of Appointments.cs
    /// </summary>
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

    /// <summary>
    ///     Returns true if the Apppointment.AptDateTime is between DateTime.Today and the number of ApptAutoRefreshRange
    ///     preference days.
    /// </summary>
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

    ///<summary>The given dateStart must be less than or equal to dateEnd. Both dates must be valid dates (not min date, etc).</summary>
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

    /// <summary>
    ///     Inserts a signal for each operatory in the schedule that has been changed, and for the provider the schedule is
    ///     for. This only
    ///     inserts a signal for today's schedules. Generally should not be called outside of Schedules.cs
    /// </summary>
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

    /// <summary>
    ///     Schedules, when we don't have a specific FKey and want to set an invalid for the entire type.
    ///     Includes the dateViewing parameter for Refresh.
    ///     A dateViewing of 01-01-0001 will be ignored because it would otherwise cause a full refresh for all connected
    ///     client workstations.
    /// </summary>
    public static void SetInvalidSched(DateTime dateViewing)
    {
        if (dateViewing == DateTime.MinValue) return; //A dateViewing of 01-01-0001 will be ignored because it would otherwise cause a full refresh for all connected client workstations.
        var signalod = new Signalod();
        signalod.IType = InvalidType.Schedules;
        signalod.DateViewing = dateViewing;
        Insert(signalod);
    }

    /// <summary>
    ///     Upserts the InvalidType.SmsTextMsgReceivedUnreadCount signal which tells all client machines to update the received
    ///     unread SMS
    ///     message count.  There should only be max one of this signal IType in the database.
    /// </summary>
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

    /// <summary>Check for appointment signals for a single date.</summary>
    public static bool IsApptRefreshNeeded(DateTime dateTimeShowing, List<Signalod> listSignalods, List<long> listOpNumsVisible,
        List<long> listProvNumsVisible)
    {
        return IsApptRefreshNeeded(dateTimeShowing, dateTimeShowing, listSignalods, listOpNumsVisible, listProvNumsVisible);
    }

    /// <summary>
    ///     After a refresh, this is used to determine whether the Appt Module needs to be refreshed. Returns true if there are
    ///     any signals
    ///     with InvalidType=Appointment where the DateViewing time of the signal falls within the provided daterange, and the
    ///     signal matches either
    ///     the list of visible operatories or visible providers in the current Appt Module View. Always returns true if any
    ///     signals have
    ///     DateViewing=DateTime.MinVal.
    /// </summary>
    public static bool IsApptRefreshNeeded(DateTime dateStart, DateTime dateEnd, List<Signalod> listSignalods, List<long> listOpNumsVisible,
        List<long> listProvNumsVisible)
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

    /// <summary>Check for schedule signals for a single date.</summary>
    public static bool IsSchedRefreshNeeded(DateTime dateTimeShowing, List<Signalod> listSignalods, List<long> listOpNumsVisible,
        List<long> listProvNumsVisible)
    {
        return IsSchedRefreshNeeded(dateTimeShowing, dateTimeShowing, listSignalods, listOpNumsVisible, listProvNumsVisible);
    }

    /// <summary>
    ///     After a refresh, this is used to determine whether the Appt Module needs to be refreshed.  Returns true if there
    ///     are any signals
    ///     with InvalidType=Appointment where the DateViewing time of the signal falls within the provided daterange, and the
    ///     signal matches either
    ///     the list of visible operatories or visible providers in the current Appt Module View.  Always returns true if any
    ///     signals have
    ///     DateViewing=DateTime.MinVal.
    /// </summary>
    public static bool IsSchedRefreshNeeded(DateTime dateStart, DateTime dateEnd, List<Signalod> listSignalods, List<long> listOpNumsVisible,
        List<long> listProvNumsVisible)
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

    /// <summary>
    ///     After a refresh, this is used to determine whether the buttons and listboxes need to be refreshed on the
    ///     ContrApptPanel.
    ///     Will return true with InvalidType==Defs.
    /// </summary>
    public static bool IsContrApptButtonRefreshNeeded(List<Signalod> listSignalods)
    {
        if (listSignalods.Exists(x => x.IType == InvalidType.Defs)) return true;
        return false;
    }

    /// <summary>
    ///     After a refresh, this is used to get a list containing all flags of types that need to be refreshed. The FKey must
    ///     be 0 and the
    ///     FKeyType must Undefined. Types of Task and SmsTextMsgReceivedUnreadCount are not included.
    /// </summary>
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


    /// <summary>
    ///     Our eServices have not been refactored yet to handle granular refreshes yet. This method does include signals that
    ///     have a FKey.
    ///     Ideally this method will be deprecated once eServices uses FKeys in cache refreshes.
    /// </summary>
    public static InvalidType[] GetInvalidTypesForWeb(List<Signalod> listSignalods)
    {
        var invalidTypeArray = listSignalods.FindAll(x => x.IType != InvalidType.Task
                                                          && x.IType != InvalidType.TaskPopup
                                                          && x.IType != InvalidType.SmsTextMsgReceivedUnreadCount)
            //TODO: Future enhancement is to rejoin this method with GetInvalidTypes. To do that we will need to have our eServices refresh parts of 
            //caches based on FKey.
            .Select(x => x.IType).ToArray();
        return invalidTypeArray;
    }

    /// <summary>
    ///     2024-05-08-Jordan. I just noticed this. I think it's wrong, but it's heavily used. We're always supposed to
    ///     use DataValid.SetInvalid, not this. Looks like I added it in 2009, but I don't remember. We need to at least test
    ///     and compare this with DataValid.SetInvalid. We need to make sure it's doing the same thing, refreshing local
    ///     machine, other machines, and MT. Seems to be used in UI layer 77 times vs 456 times for DataValid.SetInvalid.
    ///     That's good. And another 68 times in ODB, which might be unavoidable, and is probably why it was created.
    /// </summary>
    //Won't work with InvalidType.Date, InvalidType.Task, or InvalidType.TaskPopup  yet.
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

    /// <summary>
    ///     Insertion logic that doesn't use the cache. Has special cases for generating random PK's and handling Oracle
    ///     insertions.
    /// </summary>
    public static void SetInvalidNoCache(params InvalidType[] invalidTypeArray)
    {
        for (var i = 0; i < invalidTypeArray.Length; i++)
        {
            var signalod = new Signalod();
            signalod.IType = invalidTypeArray[i];
            signalod.DateViewing = DateTime.MinValue;
            SignalodCrud.InsertNoCache(signalod);
        }
    }

    /// <summary>
    ///     Must be called after Preference cache has been filled.
    ///     Deletes all signals older than 2 days if this has not been run within the last week.  Will fail silently if
    ///     anything goes wrong.
    /// </summary>
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

    ///<summary>A helper class that locks access to a HashSet for thread safety.</summary>
    private class ConcurrentHashSet<T>
    {
        private readonly HashSet<T> _hashSet = new();
        private readonly ReaderWriterLockSlim _readerWriterLockSlim = new();

        /// <summary>
        ///     Adds the specified element to a set. Returns true if the element is added or false if the element is already
        ///     present.
        /// </summary>
        public bool Add(T tItem)
        {
            _readerWriterLockSlim.EnterWriteLock();
            try
            {
                return _hashSet.Add(tItem);
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }

        ///<summary>Returns true if the specified element is already present in the set; otherwise, false.</summary>
        public bool Contains(T tItem)
        {
            _readerWriterLockSlim.EnterReadLock();
            try
            {
                return _hashSet.Contains(tItem);
            }
            finally
            {
                _readerWriterLockSlim.ExitReadLock();
            }
        }

        ///<summary>Clears the set if there are at least 5 million elements.</summary>
        public void ClearIfNeeded()
        {
            var isClearNeeded = false;
            _readerWriterLockSlim.EnterReadLock();
            try
            {
                isClearNeeded = _hashSet.Count > 5_000_000;
            }
            finally
            {
                _readerWriterLockSlim.ExitReadLock();
            }

            if (!isClearNeeded) return;
            _readerWriterLockSlim.EnterWriteLock();
            try
            {
                _hashSet.Clear();
            }
            finally
            {
                _readerWriterLockSlim.ExitWriteLock();
            }
        }
    }

    #region Fields - Public

    /// <summary>
    ///     This is not the actual date/time last refreshed.  It is really the server based date/time of the last item in the
    ///     database retrieved on previous refreshes.  That way, the local workstation time is irrelevant.
    ///     Middle tier also uses this field, however middle tier only processes cache refresh signals.
    /// </summary>
    public static DateTime DateTRegularPrioritySignalLastRefreshed;

    /// <summary>
    ///     This is the server based date/time of the last item in the database retrieved on a previous refresh. There is an
    ///     attampt to update this value on every tick, with no conditions.
    ///     Middle tier does not uses this field, however middle tier only processes cache refresh signals, and does not need
    ///     to handle high priority signals (like shutdown, printing, etc).
    /// </summary>
    public static DateTime DateTHighPrioritySignalLastRefreshed;

    /// <summary>
    ///     Mimics the behavior of DateSignalLastRefreshed, but is used exclusively in ContrAppt.TickRefresh(). The root issue
    ///     was that when a client came back from being inactive
    ///     ContrAppt.TickRefresh() was using DateSignalLastRefreshed, which is only set after we process signals. Therefore,
    ///     when a client went inactive, we could potentially query the
    ///     SignalOD table for a much larger dataset than intended. E.g.- Client goes inactive for 3 hours, comes back,
    ///     ContrAppt.TickRefresh() is called and calls RefreshTimed() with a 3 hour old datetime.
    /// </summary>
    public static DateTime DateTApptSignalLastRefreshed;

    /// <summary>
    ///     Track the last time that the web service refreshed it's cache.
    ///     The cache is shared by all consumers of this web service for this app pool.
    ///     Yes this goes against best practice and yes this could lead to occasional collisions.
    ///     But the risk of these things happening is very low given the low frequency of traffic and the low frequency of
    ///     cache-eligible changes being made.
    /// </summary>
    public static DateTime DateTSignalLastRefreshedWeb = DateTime.MinValue;

    #endregion
}

public class SignalodForApi
{
    public DateTime DateTimeServer;
    public Signalod Signalod;
}