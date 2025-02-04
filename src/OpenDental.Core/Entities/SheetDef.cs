using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class SheetDef : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long SheetDefNum;

    public string Description;
    public SheetTypeEnum SheetType;

    ///<summary>The default fontSize for the sheet.  The actual font must still be saved with each sheetField.</summary>
    public float FontSize;

    ///<summary>The default fontName for the sheet.  The actual font must still be saved with each sheetField.</summary>
    public string FontName;

    public int Width;
    public int Height;

    ///<summary>Set to true to print landscape.</summary>
    public bool IsLandscape;

    ///<summary>Amount of editable space. Actual size when filling sheet may be different.</summary>
    public int PageCount;

    ///<summary>If false, forces old single page behavior which ignores page breaks.</summary>
    public bool IsMultiPage;

    ///<summary>Enum:BypassLockStatus Specifies whether a sheet can be created before the global lock date.</summary>
    public BypassLockStatus BypassGlobalLock;

    ///<summary>If true then this Sheet has been designed for mobile and will be displayed as a mobile-friendly WebForm.</summary>
    public bool HasMobileLayout;

    ///<summary>The Date and time that SheetDef was created. Defaults to 0001-01-01 00:00:00 for existing sheets. When duplicating a custom sheet,
    ///if the original custom sheet's DateTCreated is 0001-01-01 00:00:00, the duplicate's DateTCreated will also be 0001-01-01 00:00:00. This is
    ///because this column is used for altering text fields' positions in PDFs.</summary>
    public DateTime DateTCreated;

    ///<summary>Revision ID. Gets updated any time a sheet field is added or deleted from a sheetdef (this includes any time a new language is added) or a static text field is changed. Used to determine in conjunction with PrefillMode for eClipboard to determine whether to show a patient a new form or have them update their last filled out form. Must match up with Sheet RevID to show a filled out form.</summary>
    public int RevID;

    ///<summary>Indicates whether sheets created with this sheet def load with the "Save to Images" box checked</summary>
    public bool AutoCheckSaveImage;

    ///<summary>FK to definition.DefNum. Used to override the category that is selected when auto saving the sheet to the imaging module.  This allows users to choose the category that a sheet is saved to on a per sheet basis.</summary>
    public long AutoCheckSaveImageDocCategory;

    ///<Summary>A collection of all parameters for this sheetdef.  There's usually only one parameter.  The first parameter will be a List long if it's a batch.  If a sheet has already been filled, saved to the database, and printed, then there is no longer any need for the parameters in order to fill the data.  So a retrieved sheet will have no parameters, signalling a skip in the fill phase.  There will still be parameters tucked away in the Field data in the database, but they won't become part of the sheet.</Summary>
    [CrudColumn(IsNotDbColumn = true)]
    public List<SheetParameter> Parameters;

    [CrudColumn(IsNotDbColumn = true)]
    public List<SheetFieldDef> SheetFieldDefs;

    public int HeightPage => IsLandscape ? Width : Height;

    public int WidthPage => IsLandscape ? Height : Width;

    public int HeightTotal => IsLandscape ? Math.Max(Width, Width * PageCount) : Math.Max(Height, Height * PageCount);

    public Font GetFont()
    {
        return new Font(FontName, FontSize);
    }

    public SheetDef()
    {
        PageCount = 1;
    }

    public SheetDef(SheetTypeEnum sheetType)
    {
        SheetType = sheetType;
        PageCount = 1;
        Parameters = SheetParameter.GetForType(sheetType);
        SheetFieldDefs = [];
    }

    public SheetDef Copy()
    {
        var sheetdef = (SheetDef) MemberwiseClone();
        if (Parameters != null)
        {
            sheetdef.Parameters = Parameters.Select(x => x.Copy()).ToList();
        }

        if (SheetFieldDefs != null)
        {
            sheetdef.SheetFieldDefs = SheetFieldDefs.Select(x => x.Copy()).ToList();
        }

        return sheetdef;
    }
}