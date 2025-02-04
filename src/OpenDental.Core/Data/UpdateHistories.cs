using System;
using System.Collections.Generic;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class UpdateHistories
{
    public static List<UpdateHistory> GetAll()
    {
        return UpdateHistoryCrud.SelectMany("SELECT * FROM updatehistory ORDER BY DateTimeUpdated");
    }

    public static List<UpdateHistory> GetPreviousUpdateHistories(int count)
    {
        return UpdateHistoryCrud.SelectMany(
            $"""
             SELECT * 
             FROM updatehistory
             ORDER BY DateTimeUpdated DESC
             LIMIT {count}
             """);
    }

    public static UpdateHistory GetForVersion(string version)
    {
        return UpdateHistoryCrud.SelectOne("SELECT * FROM updatehistory WHERE ProgramVersion='" + SOut.String(version) + "'");
    }

    public static DateTime GetDateForVersion(Version version)
    {
        var updateHistories = GetAll();

        foreach (var updateHistory in updateHistories)
        {
            var versionCompare = new Version();

            ODException.SwallowAnyException(() => { versionCompare = new Version(updateHistory.ProgramVersion); });

            if (versionCompare >= version)
            {
                return updateHistory.DateTimeUpdated;
            }
        }

        return new DateTime(1, 1, 1);
    }
}