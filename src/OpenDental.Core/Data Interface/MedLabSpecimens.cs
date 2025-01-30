using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class MedLabSpecimens
{
    public static void Insert(MedLabSpecimen medLabSpecimen)
    {
        MedLabSpecimenCrud.Insert(medLabSpecimen);
    }

    public static void DeleteAllForLabs(List<long> listMedLabNums)
    {
        Db.NonQ("DELETE FROM medlabspecimen WHERE MedLabNum IN(" + string.Join(",", listMedLabNums) + ")");
    }
}