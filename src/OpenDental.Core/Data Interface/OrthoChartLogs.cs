using System;
using System.Collections.Generic;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class OrthoChartLogs
{
    public static void Log(string logData, string computerName, long patNum, long userNum)
    {
        if (!PrefC.GetBool(PrefName.OrthoChartLoggingOn)) return;
        var orthoChartLog = new OrthoChartLog();
        orthoChartLog.LogData = logData;
        orthoChartLog.ComputerName = computerName;
        orthoChartLog.PatNum = patNum;
        orthoChartLog.UserNum = userNum;
        orthoChartLog.DateTimeLog = DateTime.Now;
        OrthoChartLogCrud.Insert(orthoChartLog);
    }

    public static void Log(string logData, string computerName, OrthoChartRow orthoChartRow, long userNum = 0)
    {
        if (!PrefC.GetBool(PrefName.OrthoChartLoggingOn)) return;
        var orthoChartLog = new OrthoChartLog();
        orthoChartLog.LogData = logData;
        orthoChartLog.ComputerName = computerName;
        orthoChartLog.PatNum = orthoChartRow.PatNum;
        orthoChartLog.DateTimeService = orthoChartRow.DateTimeService;
        orthoChartLog.UserNum = userNum;
        if (userNum == 0) orthoChartLog.UserNum = orthoChartRow.UserNum;
        orthoChartLog.ProvNum = orthoChartRow.ProvNum;
        orthoChartLog.OrthoChartRowNum = orthoChartRow.OrthoChartRowNum;
        orthoChartLog.DateTimeLog = DateTime.Now;
        OrthoChartLogCrud.Insert(orthoChartLog);
    }

    public static void Log(string logData, List<OrthoChart> listOrthoCharts, long orthoChartRowNum, string computerName, long patNum, long userNum)
    {
        if (!PrefC.GetBool(PrefName.OrthoChartLoggingOn)) return;

        logData += Environment.NewLine + "OrthoChart Values:" + Environment.NewLine;
        for (var i = 0; i < listOrthoCharts.Count; i++) logData += $"	OrthoChartNum:{listOrthoCharts[i].OrthoChartNum} - {listOrthoCharts[i].FieldName}:{listOrthoCharts[i].FieldValue}{Environment.NewLine}";
        var orthoChartLog = new OrthoChartLog();
        orthoChartLog.LogData = logData;
        orthoChartLog.ComputerName = computerName;
        orthoChartLog.PatNum = patNum;
        orthoChartLog.UserNum = userNum;
        orthoChartLog.OrthoChartRowNum = orthoChartRowNum;
        orthoChartLog.DateTimeLog = DateTime.Now;
        OrthoChartLogCrud.Insert(orthoChartLog);
    }

    public static void LogDb(string logData, string computerName, long orthoChartRowNum, long userNum)
    {
        if (!PrefC.GetBool(PrefName.OrthoChartLoggingOn)) return;
        //unfortunately, we don't have the OrthoChartRow object here to be able to save
        var orthoChartLog = new OrthoChartLog();
        orthoChartLog.LogData = logData;
        orthoChartLog.ComputerName = computerName;
        orthoChartLog.OrthoChartRowNum = orthoChartRowNum;
        orthoChartLog.UserNum = userNum;
        orthoChartLog.DateTimeLog = DateTime.Now;
        OrthoChartLogCrud.Insert(orthoChartLog);
    }

    public static void LogDb(string logData, string computerName, OrthoChartRow orthoChartRow, long userNum)
    {
        if (!PrefC.GetBool(PrefName.OrthoChartLoggingOn)) return;
        var orthoChartLog = new OrthoChartLog();
        orthoChartLog.LogData = logData;
        orthoChartLog.ComputerName = computerName;
        orthoChartLog.PatNum = orthoChartRow.PatNum;
        orthoChartLog.DateTimeService = orthoChartRow.DateTimeService;
        orthoChartLog.UserNum = userNum;
        orthoChartLog.ProvNum = orthoChartRow.ProvNum;
        orthoChartLog.OrthoChartRowNum = orthoChartRow.OrthoChartRowNum;
        orthoChartLog.DateTimeLog = DateTime.Now;
        OrthoChartLogCrud.Insert(orthoChartLog);
    }
}