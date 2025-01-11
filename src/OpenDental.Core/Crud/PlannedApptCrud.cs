#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PlannedApptCrud
{
    public static PlannedAppt SelectOne(long plannedApptNum)
    {
        var command = "SELECT * FROM plannedappt "
                      + "WHERE PlannedApptNum = " + SOut.Long(plannedApptNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PlannedAppt SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PlannedAppt> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PlannedAppt> TableToList(DataTable table)
    {
        var retVal = new List<PlannedAppt>();
        PlannedAppt plannedAppt;
        foreach (DataRow row in table.Rows)
        {
            plannedAppt = new PlannedAppt();
            plannedAppt.PlannedApptNum = SIn.Long(row["PlannedApptNum"].ToString());
            plannedAppt.PatNum = SIn.Long(row["PatNum"].ToString());
            plannedAppt.AptNum = SIn.Long(row["AptNum"].ToString());
            plannedAppt.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            retVal.Add(plannedAppt);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PlannedAppt> listPlannedAppts, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PlannedAppt";
        var table = new DataTable(tableName);
        table.Columns.Add("PlannedApptNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("AptNum");
        table.Columns.Add("ItemOrder");
        foreach (var plannedAppt in listPlannedAppts)
            table.Rows.Add(SOut.Long(plannedAppt.PlannedApptNum), SOut.Long(plannedAppt.PatNum), SOut.Long(plannedAppt.AptNum), SOut.Int(plannedAppt.ItemOrder));
        return table;
    }

    public static long Insert(PlannedAppt plannedAppt)
    {
        return Insert(plannedAppt, false);
    }

    public static long Insert(PlannedAppt plannedAppt, bool useExistingPK)
    {
        var command = "INSERT INTO plannedappt (";

        command += "PatNum,AptNum,ItemOrder) VALUES(";

        command +=
            SOut.Long(plannedAppt.PatNum) + ","
                                          + SOut.Long(plannedAppt.AptNum) + ","
                                          + SOut.Int(plannedAppt.ItemOrder) + ")";
        {
            plannedAppt.PlannedApptNum = Db.NonQ(command, true, "PlannedApptNum", "plannedAppt");
        }
        return plannedAppt.PlannedApptNum;
    }

    public static long InsertNoCache(PlannedAppt plannedAppt)
    {
        return InsertNoCache(plannedAppt, false);
    }

    public static long InsertNoCache(PlannedAppt plannedAppt, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO plannedappt (";
        if (isRandomKeys || useExistingPK) command += "PlannedApptNum,";
        command += "PatNum,AptNum,ItemOrder) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(plannedAppt.PlannedApptNum) + ",";
        command +=
            SOut.Long(plannedAppt.PatNum) + ","
                                          + SOut.Long(plannedAppt.AptNum) + ","
                                          + SOut.Int(plannedAppt.ItemOrder) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            plannedAppt.PlannedApptNum = Db.NonQ(command, true, "PlannedApptNum", "plannedAppt");
        return plannedAppt.PlannedApptNum;
    }

    public static void Update(PlannedAppt plannedAppt)
    {
        var command = "UPDATE plannedappt SET "
                      + "PatNum        =  " + SOut.Long(plannedAppt.PatNum) + ", "
                      + "AptNum        =  " + SOut.Long(plannedAppt.AptNum) + ", "
                      + "ItemOrder     =  " + SOut.Int(plannedAppt.ItemOrder) + " "
                      + "WHERE PlannedApptNum = " + SOut.Long(plannedAppt.PlannedApptNum);
        Db.NonQ(command);
    }

    public static bool Update(PlannedAppt plannedAppt, PlannedAppt oldPlannedAppt)
    {
        var command = "";
        if (plannedAppt.PatNum != oldPlannedAppt.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(plannedAppt.PatNum) + "";
        }

        if (plannedAppt.AptNum != oldPlannedAppt.AptNum)
        {
            if (command != "") command += ",";
            command += "AptNum = " + SOut.Long(plannedAppt.AptNum) + "";
        }

        if (plannedAppt.ItemOrder != oldPlannedAppt.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(plannedAppt.ItemOrder) + "";
        }

        if (command == "") return false;
        command = "UPDATE plannedappt SET " + command
                                            + " WHERE PlannedApptNum = " + SOut.Long(plannedAppt.PlannedApptNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(PlannedAppt plannedAppt, PlannedAppt oldPlannedAppt)
    {
        if (plannedAppt.PatNum != oldPlannedAppt.PatNum) return true;
        if (plannedAppt.AptNum != oldPlannedAppt.AptNum) return true;
        if (plannedAppt.ItemOrder != oldPlannedAppt.ItemOrder) return true;
        return false;
    }

    public static void Delete(long plannedApptNum)
    {
        var command = "DELETE FROM plannedappt "
                      + "WHERE PlannedApptNum = " + SOut.Long(plannedApptNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPlannedApptNums)
    {
        if (listPlannedApptNums == null || listPlannedApptNums.Count == 0) return;
        var command = "DELETE FROM plannedappt "
                      + "WHERE PlannedApptNum IN(" + string.Join(",", listPlannedApptNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}