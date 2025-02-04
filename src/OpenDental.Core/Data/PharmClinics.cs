using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class PharmClinics
{
    public static List<PharmClinic> GetPharmClinicsForPharmacy(long pharmacyNum)
    {
        return PharmClinicCrud.SelectMany("SELECT * FROM pharmclinic WHERE PharmacyNum = " + pharmacyNum);
    }

    public static List<PharmClinic> GetPharmClinicsForPharmacies(List<long> pharmacyNums)
    {
        return pharmacyNums.Count == 0 ? [] : PharmClinicCrud.SelectMany("SELECT * FROM pharmclinic WHERE PharmacyNum IN (" + string.Join(",", pharmacyNums) + ")");
    }

    public static void Sync(List<PharmClinic> pharmClinicsNew, List<PharmClinic> pharmClinicsOld)
    {
        if (pharmClinicsOld.Count == 0 && pharmClinicsNew.Count == 0)
        {
            return;
        }

        PharmClinicCrud.Sync(pharmClinicsNew, pharmClinicsOld);
    }
}