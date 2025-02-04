using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ProcButtonItem : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ProcButtonItemNum;

    ///<summary>FK to procbutton.ProcButtonNum.</summary>
    public long ProcButtonNum;

    ///<summary>Do not use.</summary>
    public string OldCode;

    ///<summary>FK to autocode.AutoCodeNum.  0 if this is a procedure code.</summary>
    public long AutoCodeNum;

    ///<summary>FK to procedurecode.CodeNum.  0 if this is an autocode.</summary>
    public long CodeNum;
    
    public long ItemOrder;
    
    public ProcButtonItem Copy()
    {
        return new ProcButtonItem
        {
            ProcButtonItemNum = ProcButtonItemNum,
            ProcButtonNum = ProcButtonNum,
            AutoCodeNum = AutoCodeNum,
            CodeNum = CodeNum
        };
    }
}