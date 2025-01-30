using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class MedLabFacilities
{
    public static long InsertIfNotInDb(MedLabFacility medLabFacility)
    {
        var facility = MedLabFacilityCrud.SelectOne(
            "SELECT * FROM medlabfacility " +
            "WHERE FacilityName='" + SOut.String(medLabFacility.FacilityName) + "' " +
            "AND Address='" + SOut.String(medLabFacility.Address) + "' " +
            "AND City='" + SOut.String(medLabFacility.City) + "' " +
            "AND State='" + SOut.String(medLabFacility.State) + "' " +
            "AND Zip='" + SOut.String(medLabFacility.Zip) + "' " +
            "AND Phone='" + SOut.String(medLabFacility.Phone) + "' " +
            "AND DirectorTitle='" + SOut.String(medLabFacility.DirectorTitle) + "' " +
            "AND DirectorLName='" + SOut.String(medLabFacility.DirectorLName) + "' " +
            "AND DirectorFName='" + SOut.String(medLabFacility.DirectorFName) + "'");

        return facility?.MedLabFacilityNum ?? MedLabFacilityCrud.Insert(medLabFacility);
    }

    public static MedLabFacility GetOne(long medLabFacilityNum)
    {
        return MedLabFacilityCrud.SelectOne(medLabFacilityNum);
    }

    public static List<MedLabFacility> GetFacilityList(List<MedLab> medLabs, out List<MedLabResult> medLabResults)
    {
        medLabResults = medLabs
            .SelectMany(x => x.ListMedLabResults)
            .OrderBy(x => x.MedLabNum)
            .ThenBy(x => x.MedLabResultNum)
            .ToList();

        for (var i = medLabResults.Count - 1; i > -1; i--)
        {
            if (i == 0)
            {
                break;
            }

            if (medLabResults[i].ObsID == medLabResults[i - 1].ObsID && medLabResults[i].ObsIDSub == medLabResults[i - 1].ObsIDSub)
            {
                medLabResults.RemoveAt(i);
            }
        }

        var medLabFacilityNumsByResultNum = MedLabFacAttaches
            .GetAllForResults(medLabResults
                .Select(x => x.MedLabResultNum)
                .Distinct()
                .ToList())
            .ToDictionary(
                x => x.MedLabResultNum,
                x => x.MedLabFacilityNum);

        var medLabFacilities = new List<MedLabFacility>();
        foreach (var medLabResult in medLabResults)
        {
            if (!medLabFacilityNumsByResultNum.TryGetValue(medLabResult.MedLabResultNum, out var facilityNum))
            {
                continue;
            }

            if (!medLabFacilities.Exists(x => x.MedLabFacilityNum == facilityNum))
            {
                medLabFacilities.Add(GetOne(facilityNum));
            }

            medLabResult.FacilityID = (medLabFacilities.Select(x => x.MedLabFacilityNum).ToList().IndexOf(facilityNum) + 1).ToString().PadLeft(2, '0');
        }

        return medLabFacilities;
    }
}