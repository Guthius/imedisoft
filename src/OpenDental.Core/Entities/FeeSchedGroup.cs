using System.Collections.Generic;
using System.Linq;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class FeeSchedGroup : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long FeeSchedGroupNum;

    public string Description;

    ///<summary>FK to FeeSched.FeeSchedNum.</summary>
    public long FeeSchedNum;

    ///<summary>Comma delimited list of Clinic.ClinicNums.</summary>
    public string ClinicNums;

    public List<long> ListClinicNumsAll
    {
        get
        {
            if (ClinicNums == "")
            {
                return [];
            }

            return [..ClinicNums.Split(',').Select(long.Parse).Distinct().ToList()];
        }
        set => ClinicNums = string.Join(",", value);
    }
}