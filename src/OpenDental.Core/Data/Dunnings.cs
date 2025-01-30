using System.Collections.Generic;
using System.Linq;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Dunnings
{
    public static List<Dunning> Refresh(List<long> clinicNums)
    {
        var command = "SELECT * FROM dunning";
        if (clinicNums is {Count: > 0})
        {
            command += " WHERE ClinicNum IN (" + string.Join(",", clinicNums) + ")";
        }
        
        return DunningCrud.SelectMany(command)
            .OrderBy(x => x.ClinicNum)
            .ThenBy(x => x.BillingType)
            .ThenBy(x => x.AgeAccount - x.DaysInAdvance)
            .ThenBy(x => x.InsIsPending)
            .ThenBy(x => x.DunningNum)
            .ToList();
    }

    public static void Insert(Dunning dunning)
    {
        DunningCrud.Insert(dunning);
    }

    public static void Update(Dunning dunning)
    {
        DunningCrud.Update(dunning);
    }

    public static void Delete(Dunning dunning)
    {
        Db.NonQ("DELETE FROM dunning WHERE DunningNum = " + dunning.DunningNum);
    }
}