using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness.WebTypes.WebForms;

namespace OpenDentBusiness;

public class SheetFields
{
    public static void InsertMany(List<SheetField> listSheetFields)
    {
        SheetFieldCrud.InsertMany(listSheetFields);
    }

    public static List<SheetField> GetListForSheet(long sheetNum)
    {
        var command = "SELECT * FROM sheetfield WHERE SheetNum=" + SOut.Long(sheetNum)
                                                                 + " ORDER BY SheetFieldNum"; //the ordering is CRITICAL because the signature key is based on order.
        return SheetFieldCrud.SelectMany(command);
    }

    public static List<SheetField> GetListForSheets(List<long> listSheetNums)
    {
        if (listSheetNums.IsNullOrEmpty()) return new List<SheetField>();

        var command = $"SELECT * FROM sheetfield WHERE SheetNum IN({string.Join(",", listSheetNums.Select(x => SOut.Long(x)))})";
        return SheetFieldCrud.SelectMany(command);
    }

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

    public static List<SheetField> GetFieldFromExamSheet(long patNum, string examDescript, string fieldName)
    {
        var sheet = Sheets.GetMostRecentExamSheet(patNum, examDescript);
        if (sheet == null) return null;
        var command = "SELECT * FROM sheetfield WHERE SheetNum="
                      + SOut.Long(sheet.SheetNum) + " "
                      + "AND (RadioButtonGroup='" + SOut.String(fieldName) + "' OR ReportableName='" + SOut.String(fieldName) + "' OR FieldName='" + SOut.String(fieldName) + "')";
        return SheetFieldCrud.SelectMany(command);
    }

    public static void Update(SheetField sheetField)
    {
        SheetFieldCrud.Update(sheetField);
    }

    public static void DeleteObject(long sheetFieldNum)
    {
        SheetFieldCrud.Delete(sheetFieldNum);
    }

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

    public static int SortPrimaryKey(SheetField sheetField1, SheetField sheetField2)
    {
        return sheetField1.SheetFieldNum.CompareTo(sheetField2.SheetFieldNum);
    }

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