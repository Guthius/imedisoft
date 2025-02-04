using System;
using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Reconciles
{
    public static List<Reconcile> GetList(long accountNum)
    {
        return ReconcileCrud.SelectMany("SELECT * FROM reconcile WHERE AccountNum=" + accountNum + " ORDER BY DateReconcile");
    }

    public static Reconcile GetOne(long reconcileNum)
    {
        return ReconcileCrud.SelectOne("SELECT * FROM reconcile WHERE ReconcileNum=" + reconcileNum);
    }

    public static void Insert(Reconcile reconcile)
    {
        ReconcileCrud.Insert(reconcile);
    }

    public static void Update(Reconcile reconcile)
    {
        ReconcileCrud.Update(reconcile);
    }

    public static void Delete(Reconcile reconcile)
    {
        //check to see if any journal entries are attached to this Reconcile
        var command = "SELECT COUNT(*) FROM journalentry WHERE ReconcileNum=" + (reconcile.ReconcileNum);
        if (Db.GetCount(command) != "0")
            throw new ApplicationException(Lans.g("FormReconcileEdit",
                "Not allowed to delete a Reconcile with existing journal entries."));
        command = "DELETE FROM reconcile WHERE ReconcileNum = " + (reconcile.ReconcileNum);
        Db.NonQ(command);
    }
}