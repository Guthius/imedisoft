using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ProviderClinics
{
    public static void Sync(List<ProviderClinic> listNew, List<ProviderClinic> listOld)
    {
        ProviderClinicCrud.Sync(listNew, listOld);
    }

    public static List<ProviderClinic> GetByProvNums(List<long> listProvNums)
    {
        if (listProvNums == null || listProvNums.Count == 0) return [];
        var command = "SELECT * FROM providerclinic WHERE ProvNum IN(" + string.Join(", ", listProvNums.Select(x => (x))) + ")";
        return ProviderClinicCrud.SelectMany(command);
    }

    public static ProviderClinic GetOneOrDefault(long provNum, long clinicNum)
    {
        var listProvClinics = GetListForProvider(provNum);
        return GetFromList(provNum, clinicNum, listProvClinics, true);
    }

    public static ProviderClinic GetOne(long provNum, long clinicNum)
    {
        var command = "SELECT * FROM providerclinic WHERE ProvNum = " + (provNum) + " AND ClinicNum = " + (clinicNum);
        return ProviderClinicCrud.SelectOne(command);
    }

    public static string GetDEANum(long provNum, long clinicNum = 0)
    {
        var command = "SELECT DEANum FROM providerclinic WHERE ProvNum = " + (provNum) + " AND ClinicNum = " + (clinicNum);
        var retVal = DataCore.GetScalar(command);
        if (clinicNum != 0 && string.IsNullOrWhiteSpace(retVal)) retVal = GetDEANum(provNum);
        return retVal;
    }

    public static string GetStateWhereLicensed(long provNum, long clinicNum = 0)
    {
        var command = "SELECT StateWhereLicensed FROM providerclinic WHERE ProvNum = " + (provNum) + " AND ClinicNum = " + (clinicNum);
        var retVal = DataCore.GetScalar(command);
        if (clinicNum != 0 && string.IsNullOrWhiteSpace(retVal)) retVal = GetStateWhereLicensed(provNum);
        return retVal;
    }

    public static List<ProviderClinic> GetListForProvider(long provNum, List<long> listClinicNums = null)
    {
        var command = "SELECT * FROM providerclinic WHERE ProvNum = " + (provNum);
        if (listClinicNums != null && listClinicNums.Count > 0) command += " AND ClinicNum IN(" + string.Join(", ", listClinicNums) + ") ";
        return ProviderClinicCrud.SelectMany(command);
    }

    public static ProviderClinic GetFromList(long provNum, long clinicNum, List<ProviderClinic> listProvClinics, bool canUseDefault = false)
    {
        var retVal = listProvClinics.FirstOrDefault(x => x.ProvNum == provNum && x.ClinicNum == clinicNum);
        if (canUseDefault && retVal == null) retVal = listProvClinics.FirstOrDefault(x => x.ProvNum == provNum && x.ClinicNum == 0);
        return retVal;
    }

    public static string GetStateLicenseForProv(long provNum, string stateLicensed, long clinicNum = 0, bool useRxId = false)
    {
        var licenseType = useRxId ? "StateRxID" : "StateLicense";
        var command = $"SELECT {SOut.String(licenseType)} FROM providerclinic WHERE ProvNum={(provNum)} AND StateWhereLicensed='{SOut.String(stateLicensed)}' " +
                      $"AND ClinicNum={(clinicNum)}";
        var retVal = SIn.String(DataCore.GetScalar(command));
        if (clinicNum != 0 && string.IsNullOrWhiteSpace(retVal)) retVal = GetStateLicenseForProv(provNum, stateLicensed, 0, useRxId);
        return retVal;
    }
}