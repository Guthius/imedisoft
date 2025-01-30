using System.Collections.Generic;
using System.Linq;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Guardians
{
    public static List<Guardian> Refresh(long patNumChild)
    {
        return GuardianCrud.SelectMany("SELECT * FROM guardian WHERE PatNumChild = " + patNumChild + " ORDER BY Relationship");
    }
    
    public static void Insert(Guardian guardian)
    {
        GuardianCrud.Insert(guardian);
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
        Db.NonQ("DELETE FROM guardian WHERE PatNumChild IN (SELECT p.PatNum FROM patient p WHERE p.Guarantor=" + patNumGuar + ")");
    }
    
    public static bool ExistForFamily(long patNumGuar)
    {
        return Db.GetCount("SELECT COUNT(*) FROM guardian WHERE PatNumChild IN (SELECT p.PatNum FROM patient p WHERE p.Guarantor=" + patNumGuar + ")") != "0";
    }

    public static string GetGuardianRelationshipStr(GuardianRelationship guardianRelationship)
    {
        return guardianRelationship switch
        {
            GuardianRelationship.Brother => "(br)",
            GuardianRelationship.CareGiver => "(cg)",
            GuardianRelationship.Child => "(c)",
            GuardianRelationship.Father => "(d)",
            GuardianRelationship.FosterChild => "(fc)",
            GuardianRelationship.Friend => "(f)",
            GuardianRelationship.Grandchild => "(gc)",
            GuardianRelationship.Grandfather => "(gf)",
            GuardianRelationship.Grandmother => "(gm)",
            GuardianRelationship.Grandparent => "(gp)",
            GuardianRelationship.Guardian => "(g)",
            GuardianRelationship.LifePartner => "(lp)",
            GuardianRelationship.Mother => "(m)",
            GuardianRelationship.Other => "(o)",
            GuardianRelationship.Parent => "(p)",
            GuardianRelationship.Self => "(se)",
            GuardianRelationship.Sibling => "(sb)",
            GuardianRelationship.Sister => "(ss)",
            GuardianRelationship.Sitter => "(s)",
            GuardianRelationship.Spouse => "(sp)",
            GuardianRelationship.Stepchild => "(sc)",
            GuardianRelationship.Stepfather => "(sf)",
            GuardianRelationship.Stepmother => "(sm)",
            _ => ""
        };
    }
    
    public static void RevertChanges(List<Guardian> listGuardiansNew, List<long> listPatNumsFam)
    {
        var listGuardiansDb = listPatNumsFam.SelectMany(Refresh).ToList();
        
        GuardianCrud.Sync(listGuardiansNew, listGuardiansDb);
    }
}