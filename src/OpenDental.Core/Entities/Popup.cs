using System;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Popup : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long PopupNum;

    /// <summary>FK to patient.PatNum.</summary>
    public long PatNum;

    /// <summary>The text of the popup.</summary>
    public string Description;

    ///<summary>Deprecated. Use DateTimeDisabled instead.</summary>
    public bool IsDisabled;

    ///<summary>Enum:EnumPopupLevel 0=Patient, 1=Family, 2=Superfamily. If Family, then this Popup will apply to the entire family.  If Superfamily, then this popup will apply to the entire superfamily.</summary>
    public EnumPopupLevel PopupLevel;

    ///<summary>FK to userod.UserNum.</summary>
    public long UserNum;

    ///<summary>The server time that this note was entered.  Cannot be changed by user.  Does not get changed automatically when level or isDisabled gets changed.  If note itself changes, then a new popup is created along with a new DateTimeEntry. Current popup's edit date gets set to the previous entry's DateTimeEntry</summary>
    public DateTime DateTimeEntry;

    ///<summary>Indicates that this is not the most current popup and that it is an archive.  True for any archived or "deleted" popups.</summary>
    public bool IsArchived;

    ///<summary>This will be zero for current popups that show when a patient is selected.  Archived popups will have a value which is the FK to its parent Popup.  The parent popup could be the most recent popup or another archived popup.  Will be zero for current and "deleted" popups.</summary>
    public long PopupNumArchive;

    ///<summary>The DateTime at which this popup will be disabled. If this is DateTime.MinValue, then it will never be disabled.</summary>
    public DateTime DateTimeDisabled;

    public Popup Copy()
    {
        return (Popup) MemberwiseClone();
    }
}

public enum EnumPopupLevel
{
    Patient,
    Family,
    SuperFamily,
    Automation
}