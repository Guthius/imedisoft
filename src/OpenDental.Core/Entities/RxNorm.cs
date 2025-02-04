using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class RxNorm : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long RxNormNum;

    ///<summary>RxNorm Concept universal ID.  Throughout the program, this is actually used as the Primary Key of this table rather than the RxNormNum.</summary>
    public string RxCui;

    ///<summary>Multum code.  Only used for crosscoding during import/export with electronic Rx program.  User cannot see multum codes.  Most of the rows in this table do not have an MmslCode and user searches ignore rows with an MmslCode.</summary>
    public string MmslCode;
    
    public string Description;
}