using System;
using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class ERoutings
{
    public static List<ERouting> GetAllForClinicInDateRange(long clinicNum, DateTime dateFrom, DateTime dateTo, bool includeAll)
    {
        var command = $"SELECT * FROM erouting WHERE SecDateTEntry BETWEEN {SOut.DateTime(dateFrom)} AND {SOut.DateTime(dateTo)} + INTERVAL 1 DAY ";
        if (includeAll)
        {
            command += "AND ClinicNum=" + clinicNum;
        }

        return ERoutingCrud.SelectMany(command);
    }

    public static ERouting GetOne(long eRoutingNum)
    {
        return ERoutingCrud.SelectOne(eRoutingNum);
    }
}