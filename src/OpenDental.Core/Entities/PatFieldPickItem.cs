using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class PatFieldPickItem : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PatFieldPickItemNum;

    ///<summary>FK to patfielddef.PatFieldDefNum</summary>
    public long PatFieldDefNum;

    public string Name;

    /// <summary>Abbr to show when PickList item is displayed in cramped spaces like columns. Only implemented in Superfamily grid so far.</summary>
    public string Abbreviation;

    public bool IsHidden;
    public int ItemOrder;

    public PatFieldPickItem Copy()
    {
        return (PatFieldPickItem) MemberwiseClone();
    }
}