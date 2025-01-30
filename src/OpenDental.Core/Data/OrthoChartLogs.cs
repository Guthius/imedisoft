using System;
using System.Collections.Generic;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class OrthoChartLogs
{
    public static void Log(string logData, string computerName, long patNum, long userNum)
    {
        if (!PrefC.GetBool(PrefName.OrthoChartLoggingOn)) return;

        OrthoChartLogCrud.Insert(new OrthoChartLog
        {
            LogData = logData,
            ComputerName = computerName,
            PatNum = patNum,
            UserNum = userNum,
            DateTimeLog = DateTime.Now
        });
    }

    public static void Log(string logData, string computerName, OrthoChartRow orthoChartRow, long userNum = 0)
    {
        if (!PrefC.GetBool(PrefName.OrthoChartLoggingOn)) return;

        if (userNum == 0)
        {
            userNum = orthoChartRow.UserNum;
        }
        
        OrthoChartLogCrud.Insert(new OrthoChartLog
        {
            LogData = logData,
            ComputerName = computerName,
            PatNum = orthoChartRow.PatNum,
            DateTimeService = orthoChartRow.DateTimeService,
            UserNum = userNum,
            ProvNum = orthoChartRow.ProvNum,
            OrthoChartRowNum = orthoChartRow.OrthoChartRowNum,
            DateTimeLog = DateTime.Now
        });
    }

    public static void Log(string logData, List<OrthoChart> orthoCharts, long orthoChartRowNum, string computerName, long patNum, long userNum)
    {
        if (!PrefC.GetBool(PrefName.OrthoChartLoggingOn)) return;

        logData += Environment.NewLine + "OrthoChart Values:" + Environment.NewLine;
        foreach (var orthoChart in orthoCharts)
        {
            logData += $"	OrthoChartNum:{orthoChart.OrthoChartNum} - {orthoChart.FieldName}:{orthoChart.FieldValue}{Environment.NewLine}";
        }

        OrthoChartLogCrud.Insert(new OrthoChartLog
        {
            LogData = logData,
            ComputerName = computerName,
            PatNum = patNum,
            UserNum = userNum,
            OrthoChartRowNum = orthoChartRowNum,
            DateTimeLog = DateTime.Now
        });
    }

    public static void LogDb(string logData, string computerName, long orthoChartRowNum, long userNum)
    {
        if (!PrefC.GetBool(PrefName.OrthoChartLoggingOn)) return;

        OrthoChartLogCrud.Insert(new OrthoChartLog
        {
            LogData = logData,
            ComputerName = computerName,
            OrthoChartRowNum = orthoChartRowNum,
            UserNum = userNum,
            DateTimeLog = DateTime.Now
        });
    }

    public static void LogDb(string logData, string computerName, OrthoChartRow orthoChartRow, long userNum)
    {
        if (!PrefC.GetBool(PrefName.OrthoChartLoggingOn)) return;

        OrthoChartLogCrud.Insert(new OrthoChartLog
        {
            LogData = logData,
            ComputerName = computerName,
            PatNum = orthoChartRow.PatNum,
            DateTimeService = orthoChartRow.DateTimeService,
            UserNum = userNum,
            ProvNum = orthoChartRow.ProvNum,
            OrthoChartRowNum = orthoChartRow.OrthoChartRowNum,
            DateTimeLog = DateTime.Now
        });
    }
}