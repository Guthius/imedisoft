using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class QuickPasteNote : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long QuickPasteNoteNum;

    ///<summary>FK to quickpastecat.QuickPasteCatNum.  Keeps track of which category this note is in.</summary>
    public long QuickPasteCatNum;

    public int ItemOrder;

    ///<summary>The actual note. Can be multiple lines and possibly very long.</summary>
    public string Note;

    public string Abbreviation;

    public QuickPasteNote Copy()
    {
        return (QuickPasteNote) MemberwiseClone();
    }
}