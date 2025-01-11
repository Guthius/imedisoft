using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Guardians
{
    ///<summary>Get all guardians for a one dependant/child.</summary>
    public static List<Guardian> Refresh(long patNumChild)
    {
        var command = "SELECT * FROM guardian WHERE PatNumChild = " + SOut.Long(patNumChild) + " ORDER BY Relationship";
        return GuardianCrud.SelectMany(command);
    }

    
    public static long Insert(Guardian guardian)
    {
        return GuardianCrud.Insert(guardian);
    }

    
    public static void Update(Guardian guardian)
    {
        GuardianCrud.Update(guardian);
    }

    
    public static void Delete(long guardianNum)
    {
        GuardianCrud.Delete(guardianNum);
    }

    
    public static void DeleteForFamily(long patNumGuar)
    {
        var command = "DELETE FROM guardian "
                      + "WHERE PatNumChild IN (SELECT p.PatNum FROM patient p WHERE p.Guarantor=" + SOut.Long(patNumGuar) + ")";
        Db.NonQ(command);
    }

    
    public static bool ExistForFamily(long patNumGuar)
    {
        var command = "SELECT COUNT(*) FROM guardian "
                      + "WHERE PatNumChild IN (SELECT p.PatNum FROM patient p WHERE p.Guarantor=" + SOut.Long(patNumGuar) + ")";
        if (Db.GetCount(command) == "0") return false;
        return true;
    }

    /// <summary>Short abbreviation of relationship within parentheses.</summary>
    public static string GetGuardianRelationshipStr(GuardianRelationship guardianRelationship)
    {
        switch (guardianRelationship)
        {
            case GuardianRelationship.Brother: return "(br)";
            case GuardianRelationship.CareGiver: return "(cg)";
            case GuardianRelationship.Child: return "(c)";
            case GuardianRelationship.Father: return "(d)";
            case GuardianRelationship.FosterChild: return "(fc)";
            case GuardianRelationship.Friend: return "(f)";
            case GuardianRelationship.Grandchild: return "(gc)";
            case GuardianRelationship.Grandfather: return "(gf)";
            case GuardianRelationship.Grandmother: return "(gm)";
            case GuardianRelationship.Grandparent: return "(gp)";
            case GuardianRelationship.Guardian: return "(g)";
            case GuardianRelationship.LifePartner: return "(lp)";
            case GuardianRelationship.Mother: return "(m)";
            case GuardianRelationship.Other: return "(o)";
            case GuardianRelationship.Parent: return "(p)";
            case GuardianRelationship.Self: return "(se)";
            case GuardianRelationship.Sibling: return "(sb)";
            case GuardianRelationship.Sister: return "(ss)";
            case GuardianRelationship.Sitter: return "(s)";
            case GuardianRelationship.Spouse: return "(sp)";
            case GuardianRelationship.Stepchild: return "(sc)";
            case GuardianRelationship.Stepfather: return "(sf)";
            case GuardianRelationship.Stepmother: return "(sm)";
        }

        return "";
    }

    /// <summary>
    ///     Inserts, updates, or deletes database rows from the provided list of family PatNums back to the state of
    ///     listGuardiansNew.
    ///     Must always pass in the list of family PatNums.
    /// </summary>
    public static void RevertChanges(List<Guardian> listGuardiansNew, List<long> listPatNumsFam)
    {
        var listGuardiansDb = listPatNumsFam.SelectMany(x => Refresh(x)).ToList();
        //Usually we don't like using a DB list for sync because of potential deletions of newer entries.  However I am leaving this function alone 
        //because it would be a lot of work to rewrite FormPatientEdit to only undo the changes that this instance of the window specifically made.
        GuardianCrud.Sync(listGuardiansNew, listGuardiansDb);
    }

    ///<summary>Inserts, updates, or deletes database rows to match supplied list.</summary>
    public static void Sync(List<Guardian> listGuardiansNew, List<Guardian> listGuardiansOld)
    {
        GuardianCrud.Sync(listGuardiansNew, listGuardiansOld);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    ///<summary>Gets one Guardian from the db.</summary>
    public static Guardian GetOne(long guardianNum){

        return Crud.GuardianCrud.SelectOne(guardianNum);
    }




    */
}