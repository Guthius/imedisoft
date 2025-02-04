using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class AppointmentRule : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long AppointmentRuleNum;

    ///<summary>The description of the rule which will be displayed to the user.</summary>
    public string RuleDesc;

    ///<summary>The procedure code of the start of the range.</summary>
    public string CodeStart;

    ///<summary>The procedure code of the end of the range.</summary>
    public string CodeEnd;

    public bool IsEnabled;

    public AppointmentRule Clone()
    {
        return (AppointmentRule) MemberwiseClone();
    }
}