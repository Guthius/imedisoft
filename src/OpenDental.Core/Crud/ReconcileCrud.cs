#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ReconcileCrud
{
    public static Reconcile SelectOne(long reconcileNum)
    {
        var command = "SELECT * FROM reconcile "
                      + "WHERE ReconcileNum = " + SOut.Long(reconcileNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Reconcile SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Reconcile> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Reconcile> TableToList(DataTable table)
    {
        var retVal = new List<Reconcile>();
        Reconcile reconcile;
        foreach (DataRow row in table.Rows)
        {
            reconcile = new Reconcile();
            reconcile.ReconcileNum = SIn.Long(row["ReconcileNum"].ToString());
            reconcile.AccountNum = SIn.Long(row["AccountNum"].ToString());
            reconcile.StartingBal = SIn.Double(row["StartingBal"].ToString());
            reconcile.EndingBal = SIn.Double(row["EndingBal"].ToString());
            reconcile.DateReconcile = SIn.Date(row["DateReconcile"].ToString());
            reconcile.IsLocked = SIn.Bool(row["IsLocked"].ToString());
            retVal.Add(reconcile);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Reconcile> listReconciles, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Reconcile";
        var table = new DataTable(tableName);
        table.Columns.Add("ReconcileNum");
        table.Columns.Add("AccountNum");
        table.Columns.Add("StartingBal");
        table.Columns.Add("EndingBal");
        table.Columns.Add("DateReconcile");
        table.Columns.Add("IsLocked");
        foreach (var reconcile in listReconciles)
            table.Rows.Add(SOut.Long(reconcile.ReconcileNum), SOut.Long(reconcile.AccountNum), SOut.Double(reconcile.StartingBal), SOut.Double(reconcile.EndingBal), SOut.DateT(reconcile.DateReconcile, false), SOut.Bool(reconcile.IsLocked));
        return table;
    }

    public static long Insert(Reconcile reconcile)
    {
        return Insert(reconcile, false);
    }

    public static long Insert(Reconcile reconcile, bool useExistingPK)
    {
        var command = "INSERT INTO reconcile (";

        command += "AccountNum,StartingBal,EndingBal,DateReconcile,IsLocked) VALUES(";

        command +=
            SOut.Long(reconcile.AccountNum) + ","
                                            + SOut.Double(reconcile.StartingBal) + ","
                                            + SOut.Double(reconcile.EndingBal) + ","
                                            + SOut.Date(reconcile.DateReconcile) + ","
                                            + SOut.Bool(reconcile.IsLocked) + ")";
        {
            reconcile.ReconcileNum = Db.NonQ(command, true, "ReconcileNum", "reconcile");
        }
        return reconcile.ReconcileNum;
    }

    public static long InsertNoCache(Reconcile reconcile)
    {
        return InsertNoCache(reconcile, false);
    }

    public static long InsertNoCache(Reconcile reconcile, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO reconcile (";
        if (isRandomKeys || useExistingPK) command += "ReconcileNum,";
        command += "AccountNum,StartingBal,EndingBal,DateReconcile,IsLocked) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(reconcile.ReconcileNum) + ",";
        command +=
            SOut.Long(reconcile.AccountNum) + ","
                                            + SOut.Double(reconcile.StartingBal) + ","
                                            + SOut.Double(reconcile.EndingBal) + ","
                                            + SOut.Date(reconcile.DateReconcile) + ","
                                            + SOut.Bool(reconcile.IsLocked) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            reconcile.ReconcileNum = Db.NonQ(command, true, "ReconcileNum", "reconcile");
        return reconcile.ReconcileNum;
    }

    public static void Update(Reconcile reconcile)
    {
        var command = "UPDATE reconcile SET "
                      + "AccountNum   =  " + SOut.Long(reconcile.AccountNum) + ", "
                      + "StartingBal  =  " + SOut.Double(reconcile.StartingBal) + ", "
                      + "EndingBal    =  " + SOut.Double(reconcile.EndingBal) + ", "
                      + "DateReconcile=  " + SOut.Date(reconcile.DateReconcile) + ", "
                      + "IsLocked     =  " + SOut.Bool(reconcile.IsLocked) + " "
                      + "WHERE ReconcileNum = " + SOut.Long(reconcile.ReconcileNum);
        Db.NonQ(command);
    }

    public static bool Update(Reconcile reconcile, Reconcile oldReconcile)
    {
        var command = "";
        if (reconcile.AccountNum != oldReconcile.AccountNum)
        {
            if (command != "") command += ",";
            command += "AccountNum = " + SOut.Long(reconcile.AccountNum) + "";
        }

        if (reconcile.StartingBal != oldReconcile.StartingBal)
        {
            if (command != "") command += ",";
            command += "StartingBal = " + SOut.Double(reconcile.StartingBal) + "";
        }

        if (reconcile.EndingBal != oldReconcile.EndingBal)
        {
            if (command != "") command += ",";
            command += "EndingBal = " + SOut.Double(reconcile.EndingBal) + "";
        }

        if (reconcile.DateReconcile.Date != oldReconcile.DateReconcile.Date)
        {
            if (command != "") command += ",";
            command += "DateReconcile = " + SOut.Date(reconcile.DateReconcile) + "";
        }

        if (reconcile.IsLocked != oldReconcile.IsLocked)
        {
            if (command != "") command += ",";
            command += "IsLocked = " + SOut.Bool(reconcile.IsLocked) + "";
        }

        if (command == "") return false;
        command = "UPDATE reconcile SET " + command
                                          + " WHERE ReconcileNum = " + SOut.Long(reconcile.ReconcileNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Reconcile reconcile, Reconcile oldReconcile)
    {
        if (reconcile.AccountNum != oldReconcile.AccountNum) return true;
        if (reconcile.StartingBal != oldReconcile.StartingBal) return true;
        if (reconcile.EndingBal != oldReconcile.EndingBal) return true;
        if (reconcile.DateReconcile.Date != oldReconcile.DateReconcile.Date) return true;
        if (reconcile.IsLocked != oldReconcile.IsLocked) return true;
        return false;
    }

    public static void Delete(long reconcileNum)
    {
        var command = "DELETE FROM reconcile "
                      + "WHERE ReconcileNum = " + SOut.Long(reconcileNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listReconcileNums)
    {
        if (listReconcileNums == null || listReconcileNums.Count == 0) return;
        var command = "DELETE FROM reconcile "
                      + "WHERE ReconcileNum IN(" + string.Join(",", listReconcileNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}