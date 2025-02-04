using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ReconcileCrud
{
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
        foreach (DataRow row in table.Rows)
        {
            var reconcile = new Reconcile
            {
                ReconcileNum = SIn.Long(row["ReconcileNum"].ToString()),
                AccountNum = SIn.Long(row["AccountNum"].ToString()),
                StartingBal = SIn.Double(row["StartingBal"].ToString()),
                EndingBal = SIn.Double(row["EndingBal"].ToString()),
                DateReconcile = SIn.Date(row["DateReconcile"].ToString()),
                IsLocked = SIn.Bool(row["IsLocked"].ToString())
            };
            retVal.Add(reconcile);
        }

        return retVal;
    }

    public static void Insert(Reconcile reconcile)
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
}