using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class PharmClinics
{
    public static void Insert(PharmClinic pharmClinic)
    {
        PharmClinicCrud.Insert(pharmClinic);
    }

    public static List<PharmClinic> GetPharmClinicsForPharmacy(long pharmacyNum)
    {
        var command = "SELECT * FROM pharmclinic WHERE PharmacyNum = " + SOut.Long(pharmacyNum);
        return PharmClinicCrud.SelectMany(command);
    }

    public static PharmClinic GetOneForPharmacyAndClinic(long pharmacyNum, long clinicNum)
    {
        var command = "SELECT * FROM pharmclinic WHERE PharmacyNum = " + SOut.Long(pharmacyNum) + " AND ClinicNum = " + SOut.Long(clinicNum);
        return PharmClinicCrud.SelectOne(command);
    }

    public static List<PharmClinic> GetPharmClinicsForPharmacies(List<long> listPharmacyNums)
    {
        if (listPharmacyNums.Count == 0) return new List<PharmClinic>();

        var command = "SELECT * FROM pharmclinic WHERE PharmacyNum IN (" + string.Join(",", listPharmacyNums) + ")";
        return PharmClinicCrud.SelectMany(command);
    }

    public static void Sync(List<PharmClinic> listPharmClinicsNew, List<PharmClinic> listPharmClinicsOld)
    {
        if (listPharmClinicsOld.Count == 0 && listPharmClinicsNew.Count == 0) //No need to send to middle tier.
            return;

        PharmClinicCrud.Sync(listPharmClinicsNew, listPharmClinicsOld);
    }
}