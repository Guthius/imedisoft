using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class UpdateHistoryCrud
{
    public static UpdateHistory SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<UpdateHistory> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<UpdateHistory> TableToList(DataTable table)
    {
        var retVal = new List<UpdateHistory>();
        foreach (DataRow row in table.Rows)
        {
            var updateHistory = new UpdateHistory
            {
                UpdateHistoryNum = SIn.Long(row["UpdateHistoryNum"].ToString()),
                DateTimeUpdated = SIn.DateTime(row["DateTimeUpdated"].ToString()),
                ProgramVersion = SIn.String(row["ProgramVersion"].ToString()),
                Signature = SIn.String(row["Signature"].ToString())
            };
            retVal.Add(updateHistory);
        }

        return retVal;
    }
}