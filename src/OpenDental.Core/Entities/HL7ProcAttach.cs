using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class HL7ProcAttach : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long HL7ProcAttachNum;

    ///<summary>FK to hl7msg.HL7MsgNum.</summary>
    public long HL7MsgNum;

    ///<summary>FK to procedurelog.ProcNum.</summary>
    public long ProcNum;
}