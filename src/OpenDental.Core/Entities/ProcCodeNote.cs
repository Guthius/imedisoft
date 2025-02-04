using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ProcCodeNote : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long ProcCodeNoteNum;

    ///<summary>FK to procedurecode.CodeNum.</summary>
    public long CodeNum;

    ///<summary>FK to provider.ProvNum.</summary>
    public long ProvNum;
    
    public string Note;

    ///<summary>X's and /'s describe Dr's time and assistant's time in the same increments as the user has set.</summary>
    public string ProcTime;

    ///<summary>Enum:ProcStat Indicates which status the procedure has to be set to in order for this note to take affect.
    ///Should only ever be 1 (TP) or 2 (C).  See procedurelog.ProcStatus for more info.</summary>
    public ProcStat ProcStatus;
    
    public ProcCodeNote Copy()
    {
        return (ProcCodeNote) MemberwiseClone();
    }
}