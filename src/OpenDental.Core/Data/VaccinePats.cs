using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class VaccinePats
{
    public static List<VaccinePat> Refresh(long patNum)
    {
        return VaccinePatCrud.SelectMany("SELECT * FROM vaccinepat WHERE PatNum = " + patNum + " ORDER BY DateTimeStart");
    }

    public static void Insert(VaccinePat vaccinePat)
    {
        VaccinePatCrud.Insert(vaccinePat);
    }

    public static void Update(VaccinePat vaccinePat)
    {
        VaccinePatCrud.Update(vaccinePat);
    }

    public static void Delete(long vaccinePatNum)
    {
        Db.NonQ("DELETE FROM vaccinepat WHERE VaccinePatNum = " + vaccinePatNum);

        VaccineObses.DeleteForVaccinePat(vaccinePatNum);
    }
}