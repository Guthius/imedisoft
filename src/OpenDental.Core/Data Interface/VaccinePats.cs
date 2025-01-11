using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class VaccinePats
{
    public static List<VaccinePat> Refresh(long patNum)
    {
        var command = "SELECT * FROM vaccinepat WHERE PatNum = " + SOut.Long(patNum) + " ORDER BY DateTimeStart";
        return VaccinePatCrud.SelectMany(command);
    }

    public static long Insert(VaccinePat vaccinePat)
    {
        return VaccinePatCrud.Insert(vaccinePat);
    }

    public static void Update(VaccinePat vaccinePat)
    {
        VaccinePatCrud.Update(vaccinePat);
    }

    public static void Delete(long vaccinePatNum)
    {
        var command = "DELETE FROM vaccinepat WHERE VaccinePatNum = " + SOut.Long(vaccinePatNum);
        Db.NonQ(command);
        //Delete any attached observations.
        VaccineObses.DeleteForVaccinePat(vaccinePatNum);
    }
}