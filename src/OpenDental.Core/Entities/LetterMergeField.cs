using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class LetterMergeField : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long FieldNum;

    ///<summary>FK to lettermerge.LetterMergeNum.</summary>
    public long LetterMergeNum;

    ///<summary>One of the preset available field names.</summary>
    public string FieldName;

    public LetterMergeField Copy()
    {
        return (LetterMergeField) MemberwiseClone();
    }
}