using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class AutoNoteControl : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long AutoNoteControlNum;

    ///<summary>The description of the prompt as it will be referred to from other windows.</summary>
    public string Descript;

    ///<summary>'Text', 'OneResponse', or 'MultiResponse'.  More types to be added later.</summary>
    public string ControlType;

    ///<summary>The prompt text.</summary>
    public string ControlLabel;

    ///<summary>For TextBox, this is the default text.  For a ComboBox, this is the list of possible responses, one per line.</summary>
    public string ControlOptions;
    
    public AutoNoteControl Copy()
    {
        return (AutoNoteControl) MemberwiseClone();
    }
}