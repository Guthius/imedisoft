using System.Collections.Generic;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public static class SheetFields
{
    public static void InsertMany(List<SheetField> listSheetFields)
    {
        SheetFieldCrud.InsertMany(listSheetFields);
    }

    public static List<SheetField> GetListForSheet(long sheetNum)
    {
        return SheetFieldCrud.SelectMany("SELECT * FROM sheetfield WHERE SheetNum = " + sheetNum + " ORDER BY SheetFieldNum");
    }

    public static List<SheetField> GetListForSheets(List<long> sheetNums)
    {
        return sheetNums.IsNullOrEmpty() ? [] : SheetFieldCrud.SelectMany($"SELECT * FROM sheetfield WHERE SheetNum IN ({string.Join(",", sheetNums)})");
    }

    public static void GetFieldsAndParameters(Sheet sheet, List<SheetField> sheetFields = null)
    {
        sheet.SheetFields = sheetFields ?? GetListForSheet(sheet.SheetNum);
        sheet.Parameters = [];

        foreach (var sheetField in sheet.SheetFields)
        {
            if (sheetField.FieldType != SheetFieldType.Parameter)
            {
                continue;
            }

            sheet.Parameters.Add(new SheetParameter(true, sheetField.FieldName, sheetField.FieldValue));
        }
    }

    public static List<SheetField> GetFieldFromExamSheet(long patNum, string examDescript, string fieldName)
    {
        var sheet = Sheets.GetMostRecentExamSheet(patNum, examDescript);
        if (sheet is null)
        {
            return null;
        }

        return SheetFieldCrud.SelectMany(
            "SELECT * FROM sheetfield " +
            "WHERE SheetNum = " + sheet.SheetNum + " " +
            "AND (RadioButtonGroup = '" + SOut.String(fieldName) + "' " +
            "OR ReportableName='" + SOut.String(fieldName) + "' " +
            "OR FieldName='" + SOut.String(fieldName) + "')");
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
        return FieldTypeSortOrder(sheetField1.FieldType) != FieldTypeSortOrder(sheetField2.FieldType)
            ? FieldTypeSortOrder(sheetField1.FieldType).CompareTo(FieldTypeSortOrder(sheetField2.FieldType))
            : sheetField1.YPos.CompareTo(sheetField2.YPos);
    }

    internal static int FieldTypeSortOrder(SheetFieldType sheetFieldType)
    {
        return sheetFieldType switch
        {
            SheetFieldType.Image or SheetFieldType.PatImage => 0,
            SheetFieldType.Drawing => 1,
            SheetFieldType.Line or SheetFieldType.Rectangle => 2,
            SheetFieldType.Grid => 3,
            SheetFieldType.OutputText or SheetFieldType.InputField or SheetFieldType.StaticText => 4,
            SheetFieldType.CheckBox => 5,
            SheetFieldType.SigBox or SheetFieldType.SigBoxPractice => 6,
            SheetFieldType.Special or SheetFieldType.Parameter => int.MaxValue,
            _ => int.MaxValue
        };
    }

    public static int SortPrimaryKey(SheetField sheetField1, SheetField sheetField2)
    {
        return sheetField1.SheetFieldNum.CompareTo(sheetField2.SheetFieldNum);
    }

    public static void Sync(List<SheetField> sheetFieldsNew, long sheetNum, bool isSigBoxOnly)
    {
        var sheetFieldsDb = GetListForSheet(sheetNum);
        if (!isSigBoxOnly)
        {
            var sheetFieldsNoSigNew = sheetFieldsNew.FindAll(x => x.FieldType != SheetFieldType.Parameter && x.FieldType is not (SheetFieldType.SigBox or SheetFieldType.SigBoxPractice));
            var sheetFieldsNoSigDb = sheetFieldsDb.FindAll(x => x.FieldType != SheetFieldType.Parameter && x.FieldType is not (SheetFieldType.SigBox or SheetFieldType.SigBoxPractice));

            SheetFieldCrud.Sync(sheetFieldsNoSigNew, sheetFieldsNoSigDb);

            return;
        }

        var sheetFieldsSigOnlyNew = sheetFieldsNew.FindAll(x => x.FieldType is SheetFieldType.SigBox or SheetFieldType.SigBoxPractice);
        var sheetFieldsSigOnlyDb = sheetFieldsDb.FindAll(x => x.FieldType is SheetFieldType.SigBox or SheetFieldType.SigBoxPractice);

        SheetFieldCrud.Sync(sheetFieldsSigOnlyNew, sheetFieldsSigOnlyDb);
    }

    public static string GetComboSelectedOption(SheetField sheetField)
    {
        var options = sheetField.FieldValue.Split(';').ToList();

        return options.Count > 1 ? options[0] : "";
    }

    public static List<string> GetComboMenuItems(SheetField sheetField)
    {
        var options = sheetField.FieldValue.Split(';').ToList();

        var results = options.Count > 1 ? options[1].Split('|').ToList() : options[0].Split('|').ToList();
        for (var i = 0; i < results.Count; i++)
        {
            results[i] = results[i].Replace("&", "&&");

            if (results[i] == "-")
            {
                results[i] = "&-";
            }
        }

        return results;
    }

    public static void SetComboFieldValue(SheetField sheetField, string selectedOption)
    {
        var options = sheetField.FieldValue.Split(';').ToList();
        var stringAll = options.Count > 1 ? options[1] : options[0];

        var fieldValue = selectedOption.Replace("&&", "&") + ";" + stringAll;
        if (selectedOption == "&-")
        {
            fieldValue = selectedOption.Replace("&-", "-") + ";" + stringAll;
        }

        sheetField.FieldValue = fieldValue;
    }

    public static bool IsStaticTextFieldObsolete(EnumStaticTextField staticTextField)
    {
        return staticTextField is
            EnumStaticTextField.clinicDescription or
            EnumStaticTextField.clinicAddress or
            EnumStaticTextField.clinicCityStZip or
            EnumStaticTextField.clinicPhone;
    }
}