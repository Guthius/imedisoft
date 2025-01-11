#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class RecallTriggerCrud
{
    public static RecallTrigger SelectOne(long recallTriggerNum)
    {
        var command = "SELECT * FROM recalltrigger "
                      + "WHERE RecallTriggerNum = " + SOut.Long(recallTriggerNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static RecallTrigger SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<RecallTrigger> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<RecallTrigger> TableToList(DataTable table)
    {
        var retVal = new List<RecallTrigger>();
        RecallTrigger recallTrigger;
        foreach (DataRow row in table.Rows)
        {
            recallTrigger = new RecallTrigger();
            recallTrigger.RecallTriggerNum = SIn.Long(row["RecallTriggerNum"].ToString());
            recallTrigger.RecallTypeNum = SIn.Long(row["RecallTypeNum"].ToString());
            recallTrigger.CodeNum = SIn.Long(row["CodeNum"].ToString());
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

    public static long Insert(RecallTrigger recallTrigger)
    {
        return Insert(recallTrigger, false);
    }

    public static long Insert(RecallTrigger recallTrigger, bool useExistingPK)
    {
        var command = "INSERT INTO recalltrigger (";

        command += "RecallTypeNum,CodeNum) VALUES(";

        command +=
            SOut.Long(recallTrigger.RecallTypeNum) + ","
                                                   + SOut.Long(recallTrigger.CodeNum) + ")";
        {
            recallTrigger.RecallTriggerNum = Db.NonQ(command, true, "RecallTriggerNum", "recallTrigger");
        }
        return recallTrigger.RecallTriggerNum;
    }

    public static long InsertNoCache(RecallTrigger recallTrigger)
    {
        return InsertNoCache(recallTrigger, false);
    }

    public static long InsertNoCache(RecallTrigger recallTrigger, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO recalltrigger (";
        if (isRandomKeys || useExistingPK) command += "RecallTriggerNum,";
        command += "RecallTypeNum,CodeNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(recallTrigger.RecallTriggerNum) + ",";
        command +=
            SOut.Long(recallTrigger.RecallTypeNum) + ","
                                                   + SOut.Long(recallTrigger.CodeNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            recallTrigger.RecallTriggerNum = Db.NonQ(command, true, "RecallTriggerNum", "recallTrigger");
        return recallTrigger.RecallTriggerNum;
    }

    public static void Update(RecallTrigger recallTrigger)
    {
        var command = "UPDATE recalltrigger SET "
                      + "RecallTypeNum   =  " + SOut.Long(recallTrigger.RecallTypeNum) + ", "
                      + "CodeNum         =  " + SOut.Long(recallTrigger.CodeNum) + " "
                      + "WHERE RecallTriggerNum = " + SOut.Long(recallTrigger.RecallTriggerNum);
        Db.NonQ(command);
    }

    public static bool Update(RecallTrigger recallTrigger, RecallTrigger oldRecallTrigger)
    {
        var command = "";
        if (recallTrigger.RecallTypeNum != oldRecallTrigger.RecallTypeNum)
        {
            if (command != "") command += ",";
            command += "RecallTypeNum = " + SOut.Long(recallTrigger.RecallTypeNum) + "";
        }

        if (recallTrigger.CodeNum != oldRecallTrigger.CodeNum)
        {
            if (command != "") command += ",";
            command += "CodeNum = " + SOut.Long(recallTrigger.CodeNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE recalltrigger SET " + command
                                              + " WHERE RecallTriggerNum = " + SOut.Long(recallTrigger.RecallTriggerNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(RecallTrigger recallTrigger, RecallTrigger oldRecallTrigger)
    {
        if (recallTrigger.RecallTypeNum != oldRecallTrigger.RecallTypeNum) return true;
        if (recallTrigger.CodeNum != oldRecallTrigger.CodeNum) return true;
        return false;
    }

    public static void Delete(long recallTriggerNum)
    {
        var command = "DELETE FROM recalltrigger "
                      + "WHERE RecallTriggerNum = " + SOut.Long(recallTriggerNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listRecallTriggerNums)
    {
        if (listRecallTriggerNums == null || listRecallTriggerNums.Count == 0) return;
        var command = "DELETE FROM recalltrigger "
                      + "WHERE RecallTriggerNum IN(" + string.Join(",", listRecallTriggerNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}