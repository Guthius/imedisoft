using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;
using OpenDentBusiness.WebTypes.WebForms;

namespace OpenDentBusiness;


public class SheetFields
{
    #region Insert

    public static void InsertMany(List<SheetField> listSheetFields)
    {
        SheetFieldCrud.InsertMany(listSheetFields);
    }

    #endregion

    ///<Summary>Gets one SheetField from the database.</Summary>
    public static SheetField CreateObject(long sheetFieldNum)
    {
        return SheetFieldCrud.SelectOne(sheetFieldNum);
    }

    public static List<SheetField> GetListForSheet(long sheetNum)
    {
        var command = "SELECT * FROM sheetfield WHERE SheetNum=" + SOut.Long(sheetNum)
                                                                 + " ORDER BY SheetFieldNum"; //the ordering is CRITICAL because the signature key is based on order.
        return SheetFieldCrud.SelectMany(command);
    }

    ///<summary>Returns a list of SheetFields for the list of SheetNums passed in.</summary>
    public static List<SheetField> GetListForSheets(List<long> listSheetNums)
    {
        if (listSheetNums.IsNullOrEmpty()) return new List<SheetField>();

        var command = $"SELECT * FROM sheetfield WHERE SheetNum IN({string.Join(",", listSheetNums.Select(x => SOut.Long(x)))})";
        return SheetFieldCrud.SelectMany(command);
    }

    /// <summary>
    ///     When we need to use a sheet, we must run this method to pull all the associated fields and parameters from the
    ///     database.
    ///     Then it will be ready for printing, copying, etc.
    /// </summary>
    public static void GetFieldsAndParameters(Sheet sheet, List<SheetField> listSheetFields = null)
    {
        if (listSheetFields == null)
            sheet.SheetFields = GetListForSheet(sheet.SheetNum);
        else
            sheet.SheetFields = listSheetFields;
        //so parameters will also be in the field list, but they will just be ignored from here on out.
        //because we will have an explicit parameter list instead.
        sheet.Parameters = new List<SheetParameter>();
        SheetParameter sheetParameter;
        //int paramVal;
        for (var i = 0; i < sheet.SheetFields.Count; i++)
            if (sheet.SheetFields[i].FieldType == SheetFieldType.Parameter)
            {
                sheetParameter = new SheetParameter(true, sheet.SheetFields[i].FieldName, sheet.SheetFields[i].FieldValue);
                sheet.Parameters.Add(sheetParameter);
            }
    }

    /// <summary>
    ///     Used in SheetFiller to fill patient letter with exam sheet information.  Will return null if no exam sheet
    ///     matching the description exists for the patient.  Usually just returns one field, but will return a list of fields
    ///     if it's for a RadioButtonGroup.
    /// </summary>
    public static List<SheetField> GetFieldFromExamSheet(long patNum, string examDescript, string fieldName)
    {
        var sheet = Sheets.GetMostRecentExamSheet(patNum, examDescript);
        if (sheet == null) return null;
        var command = "SELECT * FROM sheetfield WHERE SheetNum="
                      + SOut.Long(sheet.SheetNum) + " "
                      + "AND (RadioButtonGroup='" + SOut.String(fieldName) + "' OR ReportableName='" + SOut.String(fieldName) + "' OR FieldName='" + SOut.String(fieldName) + "')";
        return SheetFieldCrud.SelectMany(command);
    }

    
    public static long Insert(SheetField sheetField)
    {
        return SheetFieldCrud.Insert(sheetField);
    }

    
    public static void Update(SheetField sheetField)
    {
        SheetFieldCrud.Update(sheetField);
    }

    
    public static void DeleteObject(long sheetFieldNum)
    {
        SheetFieldCrud.Delete(sheetFieldNum);
    }

    ///<summary>Deletes all existing drawing fields for a sheet from the database and then adds back the list supplied.</summary>
    public static void SetDrawings(List<SheetField> listSheetFields, long sheetNum)
    {
        var command = "DELETE FROM sheetfield WHERE SheetNum=" + SOut.Long(sheetNum)
                                                               + " AND FieldType=" + SOut.Long((int) SheetFieldType.Drawing);
        Db.NonQ(command);
        for (var i = 0; i < listSheetFields.Count; i++) Insert(listSheetFields[i]);
    }

    /// <summary>
    ///     Sorts fields in the order that they shoudl be drawn on top of eachother. First Images, then Drawings, Lines,
    ///     Rectangles, Text, Check Boxes, and SigBoxes. In that order.
    /// </summary>
    public static int SortDrawingOrderLayers(SheetField sheetField1, SheetField sheetField2)
    {
        if (FieldTypeSortOrder(sheetField1.FieldType) != FieldTypeSortOrder(sheetField2.FieldType)) return FieldTypeSortOrder(sheetField1.FieldType).CompareTo(FieldTypeSortOrder(sheetField2.FieldType));
        return sheetField1.YPos.CompareTo(sheetField2.YPos);
        //return f1.SheetFieldNum.CompareTo(f2.SheetFieldNum);
    }

    
    public static DateTime GetBirthDate(string strDate, bool isWebForm, bool isCemtTransfer, string cultureName = "")
    {
        DateTime dateTime;
        //Parse the birthdate field using our websheet_preference for this practice if this sheet was a WebForm, otherwise, use the current 
        //computer's region/language settings.
        if (isWebForm && !isCemtTransfer)
            dateTime = WebForms_Sheets.ParseDateWebForms(strDate, cultureName);
        else
            dateTime = SIn.Date(strDate);
        return dateTime;
    }

    /// <summary>
    ///     Re-orders the SheetFieldType enum to a drawing order. Images should be drawn first, then drawings, then lines,
    ///     then rectangles, etc...
    /// </summary>
    internal static int FieldTypeSortOrder(SheetFieldType sheetFieldType)
    {
        switch (sheetFieldType)
        {
            case SheetFieldType.Image:
            case SheetFieldType.PatImage:
                return 0;
            case SheetFieldType.Drawing:
                return 1;
            case SheetFieldType.Line:
            case SheetFieldType.Rectangle:
                return 2;
            case SheetFieldType.Grid:
                return 3;
            case SheetFieldType.OutputText:
            case SheetFieldType.InputField:
            case SheetFieldType.StaticText:
                return 4;
            case SheetFieldType.CheckBox:
                return 5;
            case SheetFieldType.SigBox:
            case SheetFieldType.SigBoxPractice:
                return 6;
            case SheetFieldType.Special:
            case SheetFieldType.Parameter:
            default:
                return int.MaxValue;
        }
    }

    /// <summary>
    ///     Sorts the sheet fields by SheetFieldNum.  This is used when creating a signature key and is absolutely
    ///     critical that it not change.
    /// </summary>
    public static int SortPrimaryKey(SheetField sheetField1, SheetField sheetField2)
    {
        return sheetField1.SheetFieldNum.CompareTo(sheetField2.SheetFieldNum);
    }

    /// <summary>
    ///     SigBoxes must be synced after all other fields have been synced for the keyData to be in the right order.
    ///     So sync must be called first without SigBoxes, then the keyData for the signature(s) can be retrieved, then the
    ///     SigBoxes can be synced.
    ///     This function uses a DB comparison rather than a stale list because we are not worried about concurrency of a
    ///     single sheet and enhancing the
    ///     functions that call this would take a lot of restructuring.
    /// </summary>
    public static void Sync(List<SheetField> listSheetFieldsNew, long sheetNum, bool isSigBoxOnly)
    {
        var listSheetFieldsDB = GetListForSheet(sheetNum);
        if (!isSigBoxOnly)
        {
            var listSheetFieldsNoSigNew = listSheetFieldsNew.FindAll(x => x.FieldType != SheetFieldType.Parameter
                                                                          && !x.FieldType.In(SheetFieldType.SigBox, SheetFieldType.SigBoxPractice));
            var listSheetFieldsNoSigDB = listSheetFieldsDB.FindAll(x => x.FieldType != SheetFieldType.Parameter
                                                                        && !x.FieldType.In(SheetFieldType.SigBox, SheetFieldType.SigBoxPractice));
            SheetFieldCrud.Sync(listSheetFieldsNoSigNew, listSheetFieldsNoSigDB);
            return;
        }

        //SigBoxes must come after ALL other types in order for the keyData to be in the right order.
        var listSheetFieldsSigOnlyNew = listSheetFieldsNew.FindAll(x => x.FieldType.In(SheetFieldType.SigBox, SheetFieldType.SigBoxPractice));
        var listSheetFieldsSigOnlyDB = listSheetFieldsDB.FindAll(x => x.FieldType.In(SheetFieldType.SigBox, SheetFieldType.SigBoxPractice));
        SheetFieldCrud.Sync(listSheetFieldsSigOnlyNew, listSheetFieldsSigOnlyDB);
    }

    public static string GetComboSelectedOption(SheetField sheetField)
    {
        var listOptions = sheetField.FieldValue.Split(';').ToList();
        if (listOptions.Count > 1)
        {
            var str = listOptions[0]; //empty string when nothing selected
            return str;
        }

        //Incorrect format.
        return "";
    }

    /// <summary>
    ///     Parses the menu item options out of the sheet field passed in and returns values ready for display.
    ///     Since these values can be manipulated for display purposes, any changes in this method need to be reflected in
    ///     SetComboFieldValue().
    /// </summary>
    public static List<string> GetComboMenuItems(SheetField sheetField)
    {
        var listStringsReturn = new List<string>();
        var listOptions = sheetField.FieldValue.Split(';').ToList();
        if (listOptions.Count > 1)
            listStringsReturn = listOptions[1].Split('|').ToList();
        else //Incorrect format.
            //Default to empty string when 'values' is in format 'A|B|C', indicating only combobox options, 
            //rather than 'C;A|B|C' which indicates selection as well as options.
            //Upon Ok click this will correct the fieldvalue format.
            listStringsReturn = listOptions[0].Split('|').ToList(); //Will be an empty string if no '|' is present.
        for (var i = 0; i < listStringsReturn.Count; i++)
        {
            //'&' is a special character in System.Windows.Forms.ContextMenu. We need to escape all ampersands so that they are displayed correctly in the fill sheet window.
            listStringsReturn[i] = listStringsReturn[i].Replace("&", "&&");
            //'-' by itself is a special character in System.Windows.Forms.ContextMenu. We need to escapte it so that it is displayed correctly in the fill sheet window.
            if (listStringsReturn[i] == "-") listStringsReturn[i] = "&-";
        }

        return listStringsReturn;
    }

    /// <summary>
    ///     Parses the selected option passed in and sets the sheet field's FieldValue accordingly.
    ///     Since the selected option may have been manipulated for display purposes, any changes in this method need to be
    ///     reflected in GetComboMenuItems().
    /// </summary>
    public static void SetComboFieldValue(SheetField sheetField, string selectedOption)
    {
        var stringAll = "";
        var listOptions = sheetField.FieldValue.Split(';').ToList();
        if (listOptions.Count > 1)
            stringAll = listOptions[1];
        else //Incorrect format.
            stringAll = listOptions[0];
        //If there are any double && signs, we need to set them back to single & symbols. If the option is a single hyphen, we would have added a & symbol to the front of it in order to get the context menu to add it as an option. We need to remove either of these additional symbols after selection so that they display correctly. See method GetComboMenuItems() just above.
        var fieldVal = selectedOption.Replace("&&", "&") + ";" + stringAll;
        if (selectedOption == "&-") fieldVal = selectedOption.Replace("&-", "-") + ";" + stringAll;
        sheetField.FieldValue = fieldVal;
    }

    public static bool IsStaticTextFieldObsolete(EnumStaticTextField staticTextField)
    {
        if (staticTextField.In(EnumStaticTextField.clinicDescription, EnumStaticTextField.clinicAddress, EnumStaticTextField.clinicCityStZip, EnumStaticTextField.clinicPhone)) return true;
        return false;
    }
}