using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class MedLabFacAttaches
{
    public static void Insert(MedLabFacAttach medLabFacAttach)
    {
        MedLabFacAttachCrud.Insert(medLabFacAttach);
    }

    public static List<MedLabFacAttach> GetAllForResults(List<long> medLabResultNums)
    {
        return MedLabFacAttachCrud.SelectMany("SELECT * FROM medlabfacattach WHERE MedLabResultNum IN(" + string.Join(",", medLabResultNums) + ")");
    }

    public static void DeleteAllForLabsOrResults(List<long> medLabNums, List<long> medLabResultNums)
    {
        Db.NonQ(
            "DELETE FROM medlabfacattach " +
            "WHERE MedLabNum IN(" + string.Join(",", medLabNums) + ") " +
            "OR MedLabResultNum IN(" + string.Join(",", medLabResultNums) + ")");
    }
}