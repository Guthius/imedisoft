using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class RecallTriggerCrud
{
    public static List<RecallTrigger> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RecallTrigger> TableToList(DataTable table)
    {
        var retVal = new List<RecallTrigger>();
        foreach (DataRow row in table.Rows)
        {
            var recallTrigger = new RecallTrigger
            {
                RecallTriggerNum = SIn.Long(row["RecallTriggerNum"].ToString()),
                RecallTypeNum = SIn.Long(row["RecallTypeNum"].ToString()),
                CodeNum = SIn.Long(row["CodeNum"].ToString())
            };
            retVal.Add(recallTrigger);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<RecallTrigger> listRecallTriggers, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "RecallTrigger";
        var table = new DataTable(tableName);
        table.Columns.Add("RecallTriggerNum");
        table.Columns.Add("RecallTypeNum");
        table.Columns.Add("CodeNum");
        foreach (var recallTrigger in listRecallTriggers)
            table.Rows.Add(SOut.Long(recallTrigger.RecallTriggerNum), SOut.Long(recallTrigger.RecallTypeNum), SOut.Long(recallTrigger.CodeNum));
        return table;
    }

    public static void Insert(RecallTrigger recallTrigger)
    {
        var command = "INSERT INTO recalltrigger (";

        command += "RecallTypeNum,CodeNum) VALUES(";

        command +=
            SOut.Long(recallTrigger.RecallTypeNum) + ","
                                                   + SOut.Long(recallTrigger.CodeNum) + ")";
        {
            recallTrigger.RecallTriggerNum = Db.NonQ(command, true, "RecallTriggerNum", "recallTrigger");
        }
    }
}