using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class QueryFilter : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long QueryFilterNum;

    ///<summary>This is a simple string instead of a FK to another small table.</summary>
    public string GroupName;

    ///<summary>The text that we look for in the query monitor. Any query that contains this text will be filtered out.</summary>
    public string FilterText;
}