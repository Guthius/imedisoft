using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class VaccineObses
{
    
    public static long Insert(VaccineObs vaccineObs)
    {
        return VaccineObsCrud.Insert(vaccineObs);
    }

    ///<summary>Gets one VaccineObs from the db.</summary>
    public static List<VaccineObs> GetForVaccine(long vaccinePatNum)
    {
        var command = "SELECT * FROM vaccineobs WHERE VaccinePatNum=" + SOut.Long(vaccinePatNum) + " ORDER BY VaccineObsNumGroup";
        return VaccineObsCrud.SelectMany(command);
    }

    
    public static void Update(VaccineObs vaccineObs)
    {
        VaccineObsCrud.Update(vaccineObs);
    }

    public static void DeleteForVaccinePat(long vaccinePatNum)
    {
        var command = "DELETE FROM vaccineobs WHERE VaccinePatNum=" + SOut.Long(vaccinePatNum);
        Db.NonQ(command);
    }
}