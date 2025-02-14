using System;
using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Reconciles
{
    public static List<Reconcile> GetList(long accountNum)
    {
        return ReconcileCrud.SelectMany("SELECT * FROM reconcile WHERE AccountNum = " + accountNum + " ORDER BY DateReconcile");
    }

    public static Reconcile GetOne(long reconcileNum)
    {
        return ReconcileCrud.SelectOne("SELECT * FROM reconcile WHERE ReconcileNum = " + reconcileNum);
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
        var commandText = "SELECT COUNT(*) FROM journalentry WHERE ReconcileNum = " + reconcile.ReconcileNum;
        
        if (Db.GetCount(commandText) != "0")
        {
            throw new ApplicationException("Not allowed to delete a Reconcile with existing journal entries.");
        }
        
        Db.NonQ("DELETE FROM reconcile WHERE ReconcileNum = " + reconcile.ReconcileNum);
    }
}