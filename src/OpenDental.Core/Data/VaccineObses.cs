using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class VaccineObses
{
    public static void Insert(VaccineObs vaccineObs)
    {
        VaccineObsCrud.Insert(vaccineObs);
    }

    public static List<VaccineObs> GetForVaccine(long vaccinePatNum)
    {
        return VaccineObsCrud.SelectMany("SELECT * FROM vaccineobs WHERE VaccinePatNum=" + vaccinePatNum + " ORDER BY VaccineObsNumGroup");
    }
    
    public static void Update(VaccineObs vaccineObs)
    {
        VaccineObsCrud.Update(vaccineObs);
    }

    public static void DeleteForVaccinePat(long vaccinePatNum)
    {
        Db.NonQ("DELETE FROM vaccineobs WHERE VaccinePatNum=" + vaccinePatNum);
    }
}