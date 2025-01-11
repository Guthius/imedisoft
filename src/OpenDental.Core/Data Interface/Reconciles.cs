using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

///<summary>The two lists get refreshed the first time they are needed rather than at startup.</summary>
public class Reconciles
{
    
    public static List<Reconcile> GetList(long accountNum)
    {
        var command = "SELECT * FROM reconcile WHERE AccountNum=" + SOut.Long(accountNum)
                                                                  + " ORDER BY DateReconcile";
        return ReconcileCrud.SelectMany(command);
    }

    ///<summary>Gets one reconcile directly from the database.  Program will crash if reconcile not found.</summary>
    public static Reconcile GetOne(long reconcileNum)
    {
        var command = "SELECT * FROM reconcile WHERE ReconcileNum=" + SOut.Long(reconcileNum);
        return ReconcileCrud.SelectOne(command);
    }

    
    public static long Insert(Reconcile reconcile)
    {
        return ReconcileCrud.Insert(reconcile);
    }

    
    public static void Update(Reconcile reconcile)
    {
        ReconcileCrud.Update(reconcile);
    }

    ///<summary>Throws exception if Reconcile is in use.</summary>
    public static void Delete(Reconcile reconcile)
    {
        //check to see if any journal entries are attached to this Reconcile
        var command = "SELECT COUNT(*) FROM journalentry WHERE ReconcileNum=" + SOut.Long(reconcile.ReconcileNum);
        if (Db.GetCount(command) != "0")
            throw new ApplicationException(Lans.g("FormReconcileEdit",
                "Not allowed to delete a Reconcile with existing journal entries."));
        command = "DELETE FROM reconcile WHERE ReconcileNum = " + SOut.Long(reconcile.ReconcileNum);
        Db.NonQ(command);
    }
}