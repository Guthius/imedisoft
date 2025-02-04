using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class StateAbbr : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long StateAbbrNum;

    public string Description;
    public string Abbr;

    ///<summary>The length that the Medicaid ID should be for this state. If 0, then the Medicaid length is not enforced for this state</summary>
    public int MedicaidIDLength;

    public StateAbbr Clone()
    {
        return (StateAbbr) MemberwiseClone();
    }

    public StateAbbr()
    {
    }

    public StateAbbr(string description, string abbr)
    {
        Description = description;
        Abbr = abbr;
    }
}