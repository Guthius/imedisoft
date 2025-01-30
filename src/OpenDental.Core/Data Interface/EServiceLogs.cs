using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class EServiceLogs
{
    public static void Insert(EServiceLog eServiceLog)
    {
        EServiceLogCrud.Insert(eServiceLog);
    }

    public static List<EServiceLog> GetEServiceLog(long clinicNum, DateTime dateFrom, DateTime dateTo)
    {
        var command = $"SELECT * FROM eservicelog WHERE LogDateTime BETWEEN {SOut.DateTime(dateFrom)} AND {SOut.DateTime(dateTo)}";
        if (true && clinicNum != -2) //-2 is the 'All' identifier
            command += $" AND ClinicNum={SOut.Long(clinicNum)}";
        return EServiceLogCrud.SelectMany(command);
    }

    public static EServiceLog GetArrivalLogForMessageToPay(string logGuid)
    {
        if (string.IsNullOrWhiteSpace(logGuid)) return null;

        var command = $"SELECT * FROM eservicelog WHERE LogGuid='{SOut.String(logGuid)}' AND EServiceAction={SOut.Enum(eServiceAction.PayPortalArrivedWithPayGuid)} ORDER BY EServiceLogNum DESC LIMIT 1";
        return EServiceLogCrud.SelectOne(command);
    }

    public static long UseMessageToPayPrefPayType(string logGuid)
    {
        var defNumPayType = PrefC.GetLong(PrefName.PayTypeMessageToPay);
        if (string.IsNullOrWhiteSpace(logGuid) //No Guid
            || defNumPayType == 0 //Use default
            || GetArrivalLogForMessageToPay(logGuid) == null) //No log entry for M2P
            return 0;
        return defNumPayType;
    }

    public static EServiceLog MakeLogEntryWebForms(eServiceAction eServiceAction, long patNum = 0, long clinicNum = 0, long FKey = 0, string logGuid = "", string note = "")
    {
        if (Security.CurUser.EServiceType == EServiceTypes.None)
            note = "User; " + note;
        else
            note = Security.CurUser.EServiceType + "; " + note;
        return MakeLogEntry(eServiceAction, eServiceType.WebForms, FKeyType.WebFormSheetID, patNum, clinicNum, FKey, logGuid, note);
    }

    public static EServiceLog MakeLogEntry(eServiceAction eServiceAction, eServiceType eServiceType, FKeyType fKeyType, long patNum = 0, long clinicNum = 0, long FKey = 0, string logGuid = "", string note = "")
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
}