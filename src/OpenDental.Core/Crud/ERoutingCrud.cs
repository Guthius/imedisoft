using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ERoutingCrud
{
    public static ERouting SelectOne(long eRoutingNum)
    {
        var command = "SELECT * FROM erouting "
                      + "WHERE ERoutingNum = " + SOut.Long(eRoutingNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ERouting> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ERouting> TableToList(DataTable table)
    {
        var retVal = new List<ERouting>();
        ERouting eRouting;
        foreach (DataRow row in table.Rows)
        {
            eRouting = new ERouting();
            eRouting.ERoutingNum = SIn.Long(row["ERoutingNum"].ToString());
            eRouting.Description = SIn.String(row["Description"].ToString());
            eRouting.PatNum = SIn.Long(row["PatNum"].ToString());
            eRouting.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            eRouting.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            eRouting.IsComplete = SIn.Bool(row["IsComplete"].ToString());
            retVal.Add(eRouting);
        }

        return retVal;
    }
}