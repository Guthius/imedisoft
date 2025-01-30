using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class FamilyHealths
{
    public static void Delete(long familyHealthNum)
    {
        Db.NonQ("DELETE FROM familyhealth WHERE FamilyHealthNum = " + familyHealthNum);
    }

    public static void Insert(FamilyHealth familyHealth)
    {
        FamilyHealthCrud.Insert(familyHealth);
    }

    public static void Update(FamilyHealth familyHealth)
    {
        FamilyHealthCrud.Update(familyHealth);
    }

    public static List<FamilyHealth> GetFamilyHealthForPat(long patNum)
    {
        return FamilyHealthCrud.SelectMany("SELECT * FROM familyhealth WHERE PatNum = " + patNum);
    }
}