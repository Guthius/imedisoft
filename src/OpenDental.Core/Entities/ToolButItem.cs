using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class ToolButItem : TableBase
{
    public long ToolButItemNum;

    ///<summary>FK to program.ProgramNum.</summary>
    public long ProgramNum;

    ///<summary>Enum:EnumToolBar The toolbar to show the button on.</summary>
    public EnumToolBar ToolBar;

    public string ButtonText;

    public static int Compare(ToolButItem item1, ToolButItem item2)
    {
        return item1.ButtonText.CompareTo(item2.ButtonText);
    }

    public ToolButItem Copy()
    {
        return (ToolButItem) MemberwiseClone();
    }
}

public enum EnumToolBar
{
    AccountModule,
    ApptModule,
    ChartModule,
    ImagingModule,
    FamilyModule,
    TreatmentPlanModule,
    ClaimsSend,

    /// <summary>
    /// Shows in the toolbar at the top that is common to all modules.
    /// </summary>
    MainToolbar,

    /// <summary>
    /// Shows in the main menu Reports submenu.
    /// </summary>
    ReportsMenu,
}