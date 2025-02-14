using System.Collections.Generic;
using System.Linq;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ProviderClinics
{
    public static List<ProviderClinic> GetByProvNums(List<long> listProvNums)
    {
        if (listProvNums == null || listProvNums.Count == 0) return [];
        var command = "SELECT * FROM providerclinic WHERE ProvNum IN(" + string.Join(", ", listProvNums.Select(x => x)) + ")";
        return ProviderClinicCrud.SelectMany(command);
    }

    public static ProviderClinic GetOneOrDefault(long provNum, long clinicNum)
    {
        var listProvClinics = GetListForProvider(provNum);
        return GetFromList(provNum, clinicNum, listProvClinics, true);
    }

    public static List<ProviderClinic> GetListForProvider(long provNum, List<long> listClinicNums = null)
    {
        var command = "SELECT * FROM providerclinic WHERE ProvNum = " + provNum;
        if (listClinicNums != null && listClinicNums.Count > 0) command += " AND ClinicNum IN(" + string.Join(", ", listClinicNums) + ") ";
        return ProviderClinicCrud.SelectMany(command);
    }

    public static ProviderClinic GetFromList(long provNum, long clinicNum, List<ProviderClinic> listProvClinics, bool canUseDefault = false)
    {
        var retVal = listProvClinics.FirstOrDefault(x => x.ProvNum == provNum && x.ClinicNum == clinicNum);
        if (canUseDefault && retVal == null) retVal = listProvClinics.FirstOrDefault(x => x.ProvNum == provNum && x.ClinicNum == 0);
        return retVal;
    }
}