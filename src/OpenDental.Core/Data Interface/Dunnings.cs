using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Dunnings
{
	/// <summary>
	///     Gets a list of all dunnings.  Ordered by BillingType, then by AgeAccount-DaysInAdvance, then by InsIsPending,
	///     then by DunningNum.
	/// </summary>
	public static List<Dunning> Refresh(List<long> listClinicNums)
    {
        var command = "SELECT * FROM dunning";
        if (listClinicNums != null && listClinicNums.Count > 0) command += " WHERE ClinicNum IN (" + string.Join(",", listClinicNums) + ")";
        return DunningCrud.SelectMany(command)
            .OrderBy(x => x.ClinicNum) //ensures that the highest precedence is Specific Clinics > Unassigned > All Clinics
            .ThenBy(x => x.BillingType)
            .ThenBy(x => x.AgeAccount - x.DaysInAdvance)
            .ThenBy(x => x.InsIsPending)
            .ThenBy(x => x.DunningNum).ToList(); //PK allows the retval to be predictable.  Works for random PKs.
    }

    
    public static long Insert(Dunning dunning)
    {
        return DunningCrud.Insert(dunning);
    }

    
    public static void Update(Dunning dunning)
    {
        DunningCrud.Update(dunning);
    }

    
    public static void Delete(Dunning dunning)
    {
        var command = "DELETE FROM dunning"
                      + " WHERE DunningNum = " + SOut.Long(dunning.DunningNum);
        Db.NonQ(command);
    }
}