#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class HieQueueCrud
{
    public static HieQueue SelectOne(long hieQueueNum)
    {
        var command = "SELECT * FROM hiequeue "
                      + "WHERE HieQueueNum = " + SOut.Long(hieQueueNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static HieQueue SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<HieQueue> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<HieQueue> TableToList(DataTable table)
    {
        var retVal = new List<HieQueue>();
        HieQueue hieQueue;
        foreach (DataRow row in table.Rows)
        {
            hieQueue = new HieQueue();
            hieQueue.HieQueueNum = SIn.Long(row["HieQueueNum"].ToString());
            hieQueue.PatNum = SIn.Long(row["PatNum"].ToString());
            retVal.Add(hieQueue);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<HieQueue> listHieQueues, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "HieQueue";
        var table = new DataTable(tableName);
        table.Columns.Add("HieQueueNum");
        table.Columns.Add("PatNum");
        foreach (var hieQueue in listHieQueues)
            table.Rows.Add(SOut.Long(hieQueue.HieQueueNum), SOut.Long(hieQueue.PatNum));
        return table;
    }

    public static long Insert(HieQueue hieQueue)
    {
        return Insert(hieQueue, false);
    }

    public static long Insert(HieQueue hieQueue, bool useExistingPK)
    {
        var command = "INSERT INTO hiequeue (";

        command += "PatNum) VALUES(";

        command +=
            SOut.Long(hieQueue.PatNum) + ")";
        {
            hieQueue.HieQueueNum = Db.NonQ(command, true, "HieQueueNum", "hieQueue");
        }
        return hieQueue.HieQueueNum;
    }

    public static long InsertNoCache(HieQueue hieQueue)
    {
        return InsertNoCache(hieQueue, false);
    }

    public static long InsertNoCache(HieQueue hieQueue, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO hiequeue (";
        if (isRandomKeys || useExistingPK) command += "HieQueueNum,";
        command += "PatNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(hieQueue.HieQueueNum) + ",";
        command +=
            SOut.Long(hieQueue.PatNum) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            hieQueue.HieQueueNum = Db.NonQ(command, true, "HieQueueNum", "hieQueue");
        return hieQueue.HieQueueNum;
    }

    public static void Update(HieQueue hieQueue)
    {
        var command = "UPDATE hiequeue SET "
                      + "PatNum     =  " + SOut.Long(hieQueue.PatNum) + " "
                      + "WHERE HieQueueNum = " + SOut.Long(hieQueue.HieQueueNum);
        Db.NonQ(command);
    }

    public static bool Update(HieQueue hieQueue, HieQueue oldHieQueue)
    {
        var command = "";
        if (hieQueue.PatNum != oldHieQueue.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(hieQueue.PatNum) + "";
        }

        if (command == "") return false;
        command = "UPDATE hiequeue SET " + command
                                         + " WHERE HieQueueNum = " + SOut.Long(hieQueue.HieQueueNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(HieQueue hieQueue, HieQueue oldHieQueue)
    {
        if (hieQueue.PatNum != oldHieQueue.PatNum) return true;
        return false;
    }

    public static void Delete(long hieQueueNum)
    {
        var command = "DELETE FROM hiequeue "
                      + "WHERE HieQueueNum = " + SOut.Long(hieQueueNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listHieQueueNums)
    {
        if (listHieQueueNums == null || listHieQueueNums.Count == 0) return;
        var command = "DELETE FROM hiequeue "
                      + "WHERE HieQueueNum IN(" + string.Join(",", listHieQueueNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}