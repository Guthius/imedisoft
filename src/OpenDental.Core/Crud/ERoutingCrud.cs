#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

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

    public static ERouting SelectOne(string command)
    {
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

    public static DataTable ListToTable(List<ERouting> listERoutings, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ERouting";
        var table = new DataTable(tableName);
        table.Columns.Add("ERoutingNum");
        table.Columns.Add("Description");
        table.Columns.Add("PatNum");
        table.Columns.Add("ClinicNum");
        table.Columns.Add("SecDateTEntry");
        table.Columns.Add("IsComplete");
        foreach (var eRouting in listERoutings)
            table.Rows.Add(SOut.Long(eRouting.ERoutingNum), eRouting.Description, SOut.Long(eRouting.PatNum), SOut.Long(eRouting.ClinicNum), SOut.DateT(eRouting.SecDateTEntry, false), SOut.Bool(eRouting.IsComplete));
        return table;
    }

    public static long Insert(ERouting eRouting)
    {
        return Insert(eRouting, false);
    }

    public static long Insert(ERouting eRouting, bool useExistingPK)
    {
        var command = "INSERT INTO erouting (";

        command += "Description,PatNum,ClinicNum,SecDateTEntry,IsComplete) VALUES(";

        command +=
            "'" + SOut.String(eRouting.Description) + "',"
            + SOut.Long(eRouting.PatNum) + ","
            + SOut.Long(eRouting.ClinicNum) + ","
            + DbHelper.Now() + ","
            + SOut.Bool(eRouting.IsComplete) + ")";
        {
            eRouting.ERoutingNum = Db.NonQ(command, true, "ERoutingNum", "eRouting");
        }
        return eRouting.ERoutingNum;
    }

    public static long InsertNoCache(ERouting eRouting)
    {
        return InsertNoCache(eRouting, false);
    }

    public static long InsertNoCache(ERouting eRouting, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO erouting (";
        if (isRandomKeys || useExistingPK) command += "ERoutingNum,";
        command += "Description,PatNum,ClinicNum,SecDateTEntry,IsComplete) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(eRouting.ERoutingNum) + ",";
        command +=
            "'" + SOut.String(eRouting.Description) + "',"
            + SOut.Long(eRouting.PatNum) + ","
            + SOut.Long(eRouting.ClinicNum) + ","
            + DbHelper.Now() + ","
            + SOut.Bool(eRouting.IsComplete) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            eRouting.ERoutingNum = Db.NonQ(command, true, "ERoutingNum", "eRouting");
        return eRouting.ERoutingNum;
    }

    public static void Update(ERouting eRouting)
    {
        var command = "UPDATE erouting SET "
                      + "Description  = '" + SOut.String(eRouting.Description) + "', "
                      + "PatNum       =  " + SOut.Long(eRouting.PatNum) + ", "
                      + "ClinicNum    =  " + SOut.Long(eRouting.ClinicNum) + ", "
                      //SecDateTEntry not allowed to change
                      + "IsComplete   =  " + SOut.Bool(eRouting.IsComplete) + " "
                      + "WHERE ERoutingNum = " + SOut.Long(eRouting.ERoutingNum);
        Db.NonQ(command);
    }

    public static bool Update(ERouting eRouting, ERouting oldERouting)
    {
        var command = "";
        if (eRouting.Description != oldERouting.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(eRouting.Description) + "'";
        }

        if (eRouting.PatNum != oldERouting.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(eRouting.PatNum) + "";
        }

        if (eRouting.ClinicNum != oldERouting.ClinicNum)
        {
            if (command != "") command += ",";
            command += "ClinicNum = " + SOut.Long(eRouting.ClinicNum) + "";
        }

        //SecDateTEntry not allowed to change
        if (eRouting.IsComplete != oldERouting.IsComplete)
        {
            if (command != "") command += ",";
            command += "IsComplete = " + SOut.Bool(eRouting.IsComplete) + "";
        }

        if (command == "") return false;
        command = "UPDATE erouting SET " + command
                                         + " WHERE ERoutingNum = " + SOut.Long(eRouting.ERoutingNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ERouting eRouting, ERouting oldERouting)
    {
        if (eRouting.Description != oldERouting.Description) return true;
        if (eRouting.PatNum != oldERouting.PatNum) return true;
        if (eRouting.ClinicNum != oldERouting.ClinicNum) return true;
        //SecDateTEntry not allowed to change
        if (eRouting.IsComplete != oldERouting.IsComplete) return true;
        return false;
    }

    public static void Delete(long eRoutingNum)
    {
        var command = "DELETE FROM erouting "
                      + "WHERE ERoutingNum = " + SOut.Long(eRoutingNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listERoutingNums)
    {
        if (listERoutingNums == null || listERoutingNums.Count == 0) return;
        var command = "DELETE FROM erouting "
                      + "WHERE ERoutingNum IN(" + string.Join(",", listERoutingNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}