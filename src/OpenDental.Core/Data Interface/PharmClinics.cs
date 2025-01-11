using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class PharmClinics
{
    #region Insert

    /// <summary></summary>
    /// <param name="pharmClinic">The PharmClinic to insert.</param>
    public static long Insert(PharmClinic pharmClinic)
    {
        return PharmClinicCrud.Insert(pharmClinic);
    }

    #endregion

    #region Delete

    
    public static void Delete(long pharmClinicNum)
    {
        PharmClinicCrud.Delete(pharmClinicNum);
    }

    #endregion

    #region Get Methods

    /// <summary>Gets one PharmClinic from the db.</summary>
    /// <param name="pharmClinicNum">The primary key for the object that will be retrieved.</param>
    public static PharmClinic GetOne(long pharmClinicNum)
    {
        return PharmClinicCrud.SelectOne(pharmClinicNum);
    }

    /// <summary>Gets a list of PharmClinics for a given pharmacy</summary>
    /// <param name="pharmacyNum">The primary key of the pharmacy.</param>
    public static List<PharmClinic> GetPharmClinicsForPharmacy(long pharmacyNum)
    {
        var command = "SELECT * FROM pharmclinic WHERE PharmacyNum = " + SOut.Long(pharmacyNum);
        return PharmClinicCrud.SelectMany(command);
    }

    /// <summary>Gets a pharmclinic for a specific clinic and pharmacy pair. Can return null.</summary>
    /// <param name="pharmacyNum">The primary key of the pharmacy.</param>
    /// <param name="clinicNum">The primary key of the clinic.</param>
    public static PharmClinic GetOneForPharmacyAndClinic(long pharmacyNum, long clinicNum)
    {
        var command = "SELECT * FROM pharmclinic WHERE PharmacyNum = " + SOut.Long(pharmacyNum) + " AND ClinicNum = " + SOut.Long(clinicNum);
        return PharmClinicCrud.SelectOne(command);
    }

    ///<summary>Gets a list of PharmClinics for given list of pharmacyNums</summary>
    public static List<PharmClinic> GetPharmClinicsForPharmacies(List<long> listPharmacyNums)
    {
        if (listPharmacyNums.Count == 0) return new List<PharmClinic>();

        var command = "SELECT * FROM pharmclinic WHERE PharmacyNum IN (" + string.Join(",", listPharmacyNums) + ")";
        return PharmClinicCrud.SelectMany(command);
    }

    #endregion

    #region Update

    /// <summary>Updates a single pharmclinic object.</summary>
    /// <param name="pharmClinic">The PharmClinic to update.</param>
    public static void Update(PharmClinic pharmClinic)
    {
        PharmClinicCrud.Update(pharmClinic);
    }

    /// <summary>Takes two lists of makes the appropriate database changes.</summary>
    /// <param name="listPharmClinicsNew">The new list of PharmClinic objects.</param>
    /// <param name="listPharmClinicsOld">The old list of PharmClinic objects.</param>
    public static void Sync(List<PharmClinic> listPharmClinicsNew, List<PharmClinic> listPharmClinicsOld)
    {
        if (listPharmClinicsOld.Count == 0 && listPharmClinicsNew.Count == 0) //No need to send to middle tier.
            return;

        PharmClinicCrud.Sync(listPharmClinicsNew, listPharmClinicsOld);
    }

    #endregion
}