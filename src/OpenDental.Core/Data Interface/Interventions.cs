using System.Collections.Generic;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Interventions
{
    public static void Insert(Intervention intervention)
    {
        InterventionCrud.Insert(intervention);
    }

    public static void Update(Intervention intervention)
    {
        InterventionCrud.Update(intervention);
    }

    public static void Delete(long interventionNum)
    {
        var command = "DELETE FROM intervention WHERE InterventionNum = " + SOut.Long(interventionNum);
        Db.NonQ(command);
    }

    public static List<Intervention> Refresh(long patNum)
    {
        var command = "SELECT * FROM intervention WHERE PatNum = " + SOut.Long(patNum);
        return InterventionCrud.SelectMany(command);
    }

    public static List<Intervention> Refresh(long patNum, InterventionCodeSet interventionCodeSet)
    {
        var command = "SELECT * FROM intervention WHERE PatNum = " + SOut.Long(patNum) + " AND CodeSet = " + SOut.Int((int) interventionCodeSet);
        return InterventionCrud.SelectMany(command);
    }

    public static List<string> GetAllForCodeSet(InterventionCodeSet interventionCodeSet)
    {
        var command = "SELECT CodeValue FROM intervention WHERE CodeSet=" + SOut.Int((int) interventionCodeSet) + " "
                      + "AND DATE(DateEntry)>=" + SOut.Date(MiscData.GetNowDateTime().AddYears(-1)) + " "
                      + "GROUP BY CodeValue,CodeSystem";
        return Db.GetListString(command);
    }
}