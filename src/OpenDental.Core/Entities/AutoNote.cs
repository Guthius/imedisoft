using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class AutoNote : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long AutoNoteNum;

    ///<summary>Name of AutoNote</summary>
    public string AutoNoteName;

    ///<summary>Was 'ControlsToInc' in previous versions.</summary>
    public string MainText;
    
    ///<summary>FK to definition.DefNum.  This is the AutoNoteCat definition category (DefCat=41), for categorizing autonotes.
    ///Uncategorized autonotes will be set to 0.</summary>
    public long Category;
    
    public AutoNote Copy()
    {
        return (AutoNote) MemberwiseClone();
    }
}