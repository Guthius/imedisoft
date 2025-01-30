using System;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class ErxLogs
{
    public static void Insert(ErxLog erxLog)
    {
        ErxLogCrud.Insert(erxLog);
    }

    public static ErxLog GetLatestForPat(long patNum, DateTime dateTimeMax)
    {
        var erxLogs = ErxLogCrud.SelectMany(
            "SELECT * FROM erxlog " +
            "WHERE PatNum=" + patNum + " " +
            "AND DateTStamp<" + SOut.DateTime(dateTimeMax) + " " +
            "ORDER BY DateTStamp DESC LIMIT 1");

        return erxLogs.Count == 0 ? null : erxLogs[0];
    }
}