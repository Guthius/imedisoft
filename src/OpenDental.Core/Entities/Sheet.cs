using System;
using System.Collections.Generic;
using System.Linq;
using OpenDentBusiness;

namespace Imedisoft.Core.Entities;

public class Sheet : TableBase
{
    [CrudColumn(IsPriKey = true)]
    public long SheetNum;
    
    public SheetTypeEnum SheetType;

    ///<summary>FK to patient.PatNum.  A saved sheet is always attached to a patient (except deposit slip).  There are a few sheets that are so minor that they don't get saved, such as a Carrier label.</summary>
    public long PatNum;

    ///<summary>The date and time of the sheet as it will be displayed in the commlog.</summary>
    public DateTime DateTimeSheet;

    ///<summary>The default fontSize for the sheet.  The actual font must still be saved with each sheetField.</summary>
    public float FontSize;

    ///<summary>The default fontName for the sheet.  The actual font must still be saved with each sheetField.</summary>
    public string FontName;

    public int Width;
    public int Height;

    ///<summary>.</summary>
    public bool IsLandscape;

    ///<summary>An internal note for the use of the office staff regarding the sheet.  Not to be printed on the sheet in any way.</summary>
    public string InternalNote;

    public string Description;

    ///<summary>Examples: 1, 2, etc. The order that this sheet will show in the Kiosk queue, or zero if not set. Also determines if it will show in eClipboard in addition to any eClipboardSheetDef. For eClipboard, this is just treated like a boolean and that actual order is ignored.</summary>
    public byte ShowInTerminal;

    ///<summary>True if this sheet was downloaded from the webforms service. EForms uses Status field instead.</summary>
    public bool IsWebForm;

    ///<summary>Forces old single page behavior, ignoring page breaks.</summary>
    public bool IsMultiPage;

    public bool IsDeleted;

    ///<summary>FK to sheetdef.SheetDefNum. The SheetDef that was used to create this sheet. Will be 0 if an internal sheet or if the sheet was created before 17.2. Can be 0 for sheets that were created from web forms that were associated to web form sheet defs missing this value at HQ. The original purpose of this column was to use it in connection with RefID of the Sheet and SheetDef to automate the updating of forms such as office policies when they change significantly. It is now also used when making a copy of a sheet. Also used alongside EClipboardSheetDef to determine whether patient has already filled out a form. </summary>
    public long SheetDefNum;

    /// <summary>FK to document.DocNum.  Referral letters are stored as PDF in the A to Z folder.</summary>
    public long DocNum;

    /// <summary>FK to clinic.ClinicNum. Used by webforms to limit the sheets displayed based on the currently selected clinic.</summary>
    public long ClinicNum;

    ///<summary>The date and time the sheet was inserted or last time someone opened the sheet and clicked OK on FormSheetFillEdit.
    ///Gets updated even if no changes were made to the sheet or sheetfields, because we don't want to do the lengthy work of comparing all fields.
    ///Used when editing a sheet to warn user if the sheet has been edited by someone else.</summary>
    public DateTime DateTSheetEdited;

    ///<summary>If true then this Sheet has been designed for mobile and will be displayed as a mobile-friendly WebForm.</summary>
    public bool HasMobileLayout;

    ///<summary>Revision ID. Used to determine in conjunction with PrefillMode for eClipboard to determine whether to show a patient a new form or have them update their last filled out form. Must match up with SheetDef RevID to show a previously filled out form.</summary>
    public int RevID;

    ///<summary>Only set when this sheet was created from a Web Form. FK to webforms_sheet.SheetID within the Web Forms server. Used to determine if this particular Web Form has been retrieved before in order to avoid creating duplicate sheet entries for a single Web Form.</summary>
    public long WebFormSheetID;

    public Sheet Copy()
    {
        var retVal = (Sheet) MemberwiseClone();
        retVal.Parameters = Parameters.Select(x => x.Copy()).ToList();
        retVal.SheetFields = SheetFields.Select(x => x.Copy()).ToList();
        return retVal;
    }
    
    [CrudColumn(IsNotDbColumn = true)]
    public List<SheetParameter> Parameters;
    
    [CrudColumn(IsNotDbColumn = true)]
    public List<SheetField> SheetFields;

    public int HeightPage => IsLandscape ? Width : Height;

    public int WidthPage => IsLandscape ? Height : Width;
    
    public SheetField GetSheetFieldByName(string fieldName)
    {
        return SheetFields?.FirstOrDefault(x => x.FieldName == fieldName);
    }
}