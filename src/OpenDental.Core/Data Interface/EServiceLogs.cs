using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EServiceLogs
{
    #region Modification Methods

    
    public static long Insert(EServiceLog eServiceLog)
    {
        return EServiceLogCrud.Insert(eServiceLog);
    }

    #endregion Modification Methods

    #region Get Methods

    ///<summary>Gets one EServiceLog from the db.</summary>
    public static EServiceLog GetOne(long eServiceLogNum)
    {
        return EServiceLogCrud.SelectOne(eServiceLogNum);
    }

    /// <summary>
    ///     Gets Table for specified clinic within date range.
    ///     If clinics are disabled or a -2 is passed in, the clinic filter will be ommitted.
    /// </summary>
    public static List<EServiceLog> GetEServiceLog(long clinicNum, DateTime dateFrom, DateTime dateTo)
    {
        var command = $"SELECT * FROM eservicelog WHERE LogDateTime BETWEEN {SOut.DateT(dateFrom)} AND {SOut.DateT(dateTo)}";
        if (true && clinicNum != -2) //-2 is the 'All' identifier
            command += $" AND ClinicNum={SOut.Long(clinicNum)}";
        return EServiceLogCrud.SelectMany(command);
    }

    ///<summary>Gets the oldest non uploaded rows in ASC order.</summary>
    public static List<EServiceLog> GetEServiceLogsForUpload(int limit)
    {
        if (limit < 1) throw new ArgumentException("Value must be greater than zero", "limit");

        var command = $"SELECT * FROM eservicelog WHERE DateTimeUploaded={SOut.DateT(DateTime.MinValue)} ORDER BY LogDateTime ASC LIMIT {SOut.Int(limit)}";
        return EServiceLogCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets the newest EServiceLog using LogGuid and the action PayPortalArrivedWithPayGuid. Log row will exist if
    ///     the payment portal user arrived via a Message-to-Pay guid.
    /// </summary>
    public static EServiceLog GetArrivalLogForMessageToPay(string logGuid)
    {
        if (string.IsNullOrWhiteSpace(logGuid)) return null;

        var command = $"SELECT * FROM eservicelog WHERE LogGuid='{SOut.String(logGuid)}' AND EServiceAction={SOut.Enum(eServiceAction.PayPortalArrivedWithPayGuid)} ORDER BY EServiceLogNum DESC LIMIT 1";
        return EServiceLogCrud.SelectOne(command);
    }

    /// <summary>
    ///     Returns 0 (use default) if Guid is null or whitespace, PayTypeMessageToPay pref is set to 0 (default), or no
    ///     EServiceLog indicating arrival via M2P exists. Otherwise returns the prefVal.
    /// </summary>
    public static long UseMessageToPayPrefPayType(string logGuid)
    {
        var defNumPayType = PrefC.GetLong(PrefName.PayTypeMessageToPay);
        if (string.IsNullOrWhiteSpace(logGuid) //No Guid
            || defNumPayType == 0 //Use default
            || GetArrivalLogForMessageToPay(logGuid) == null) //No log entry for M2P
            return 0;
        return defNumPayType;
    }

    ///<summary>Sets the DateTimeUploaded of any matching key rows to db time NOW().</summary>
    public static void SetUploadTime(List<long> listEServiceLogNums)
    {
        if (listEServiceLogNums.IsNullOrEmpty()) return;

        var command = $"UPDATE eservicelog SET DateTimeUploaded=NOW() WHERE EServiceLogNum IN ({string.Join(",", listEServiceLogNums.Select(x => SOut.Long(x)))})";
        Db.NonQ(command);
    }

    ///<summary>Deletes all EServiceLogs taht were uploaded over a year ago.</summary>
    public static long DeleteOldLogs()
    {
        var command = $"DELETE FROM eservicelog WHERE DateTimeUploaded<DATE_SUB(NOW(), INTERVAL 1 YEAR) AND DateTimeUploaded!={SOut.DateT(DateTime.MinValue)}";
        return Db.NonQ(command);
    }

    #endregion Get Methods

    #region Misc Methods

    /// <summary>
    ///     Makes a new EServices log entry specifically for WebForms logging. Prepends Security.CurUser.EServiceType to
    ///     note.
    /// </summary>
    public static EServiceLog MakeLogEntryWebForms(eServiceAction eServiceAction, long patNum = 0, long clinicNum = 0, long FKey = 0, string logGuid = "", string note = "")
    {
        if (Security.CurUser.EServiceType == EServiceTypes.None)
            note = "User; " + note;
        else
            note = Security.CurUser.EServiceType + "; " + note;
        return MakeLogEntry(eServiceAction, eServiceType.WebForms, FKeyType.WebFormSheetID, patNum, clinicNum, FKey, logGuid, note);
    }

    ///<summary>Makes a new EServices log entry. PatNum can be 0.</summary>
    public static EServiceLog MakeLogEntry(eServiceAction eServiceAction, eServiceType eServiceType, FKeyType fKeyType,
        long patNum = 0, long clinicNum = 0, long FKey = 0, string logGuid = "", string note = "")
    {
        if (logGuid == "") logGuid = Guid.NewGuid().ToString();

        var eServiceLog = new EServiceLog();
        eServiceLog.LogGuid = logGuid;
        eServiceLog.PatNum = patNum;
        eServiceLog.ClinicNum = clinicNum;
        eServiceLog.KeyType = fKeyType;
        eServiceLog.FKey = FKey;
        eServiceLog.EServiceAction = eServiceAction;
        eServiceLog.EServiceType = eServiceType;
        eServiceLog.Note = note;
        Insert(eServiceLog);
        return eServiceLog;
    }

    /// <summary>
    ///     Pass in an eServiceType to retrieve a list of actions that are associated with that specific type. This list
    ///     is not being alphabetized. The calling function will need to do that if it is required.
    /// </summary>
    public static List<eServiceAction> GetEServiceActions(eServiceType eServiceType)
    {
        var listeServiceActions = Enum.GetValues(typeof(eServiceAction)).Cast<eServiceAction>().ToList();
        if (eServiceType == eServiceType.Unknown) return listeServiceActions;
        var listeServiceActionsResult = new List<eServiceAction>();
        for (var i = 0; i < listeServiceActions.Count; i++)
        {
            var eServiceLogType = EnumTools.GetAttributeOrDefault<EServiceLogType>(listeServiceActions[i]);
            if (eServiceLogType.eServiceTypes.Contains(eServiceType)) listeServiceActionsResult.Add(listeServiceActions[i]);
        }

        return listeServiceActionsResult;
    }

    #endregion Misc Methods
}