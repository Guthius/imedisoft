using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ProcGroupItem : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ProcGroupItemNum;

    ///<summary>FK to procedurelog.ProcNum.</summary>
    public long ProcNum;

    ///<summary>FK to procedurelog.ProcNum.This is the group note that the procedure is in.</summary>
    public long GroupNum;
}