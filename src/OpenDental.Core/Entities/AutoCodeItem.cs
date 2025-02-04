using System.Collections.Generic;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class AutoCodeItem : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long AutoCodeItemNum;

    ///<summary>FK to autocode.AutoCodeNum</summary>
    public long AutoCodeNum;

    ///<summary>Do not use</summary>
    public string OldCode;

    ///<summary>FK to procedurecode.CodeNum</summary>
    public long CodeNum;

    ///<summary>Only used in the validation section when closing FormAutoCodeEdit.  Will normally be empty.</summary>
    [CrudColumn(IsNotDbColumn = true)]
    public List<AutoCodeCond> ListConditions;

    public AutoCodeItem Copy()
    {
        return (AutoCodeItem) MemberwiseClone();
    }
}