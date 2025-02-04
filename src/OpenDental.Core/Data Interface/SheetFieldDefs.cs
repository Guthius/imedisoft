using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class SheetFieldDefs
{
    public static List<SheetFieldDef> GetForExamSheet(long sheetDefNum)
    {
        var command = "SELECT * FROM sheetfielddef WHERE SheetDefNum=" + (sheetDefNum) + " "
                      + "AND ((FieldName!='misc' AND FieldName!='') OR (ReportableName!='')) "
                      + "GROUP BY FieldName,ReportableName";
        return SheetFieldDefCrud.SelectMany(command);
    }

    public static List<SheetFieldDef> GetForSheetDef(long sheetDefNum)
    {
        var command = "SELECT * FROM sheetfielddef WHERE SheetDefNum=" + (sheetDefNum);
        return SheetFieldDefCrud.SelectMany(command);
    }

    public static bool IsMobileFieldType(SheetFieldType sheetFieldType, int tabOrderMobile, string fieldName)
    {
        //Always include these FieldTypes
        if (sheetFieldType.In(
                SheetFieldType.ComboBox,
                SheetFieldType.InputField,
                SheetFieldType.MobileHeader,
                SheetFieldType.SigBox,
                SheetFieldType.OutputText))
            return true;
        //Only include StaticText if it has a valid TabOrderMobile.  Treat OutputText fields as a StaticText
        if (tabOrderMobile >= 1)
            if (sheetFieldType == SheetFieldType.StaticText || sheetFieldType == SheetFieldType.OutputText)
                return true;

        //All other checkboxes are included. CheckMeds are acceptable in eClipboard, but function a little different than checkboxes. CheckMeds are explicitly looked for when importing.
        if (sheetFieldType == SheetFieldType.CheckBox) return true;
        return false;
    }

    public static List<SheetFieldDef> GetRadioGroupForSheetFieldDef(SheetFieldDef sheetFieldDef, List<SheetFieldDef> listSheetFieldDefs)
    {
        var listSheetFieldDefsRetVal = new List<SheetFieldDef>();
        //Each SheetFieldDef item goes into the panel of available fields.
        //Only mobile-friendly fields.
        var listSheetFieldDefsMobile = listSheetFieldDefs.FindAll(x => IsMobileFieldType(x.FieldType, x.TabOrderMobile, x.FieldName));
        //2 different ways that fields can be grouped for radio groups so make a super-set of both styles.
        var funcCriteria1 = new Func<SheetFieldDef, bool>(x =>
        {
            //Misc and it has a RadioButtonGroup.						
            return x.FieldName == "misc" && !string.IsNullOrEmpty(x.RadioButtonGroup);
        });
        var funcCriteria2 = new Func<SheetFieldDef, bool>(x =>
        {
            //Not misc but it has a RadioButtonValue.						
            return x.FieldName != "misc" && !x.FieldName.Contains("checkMed") && !string.IsNullOrEmpty(x.RadioButtonValue);
        });
        var listSheetFieldDefsRadioSuper = listSheetFieldDefsMobile
            .FindAll(x => x.FieldType == SheetFieldType.CheckBox && (funcCriteria1(x) || funcCriteria2(x)));
        //The first way.
        var listSheetFieldDefsRadio1 = listSheetFieldDefsRadioSuper
            .FindAll(x => funcCriteria1(x));
        //The second way.
        var listSheetFieldDefsRadio2 = listSheetFieldDefsRadioSuper
            //Don't include any fields that have already been handled above.
            .FindAll(x => funcCriteria2(x));
        var listSheetFieldDefsCheckboxGroups = listSheetFieldDefsMobile
            .FindAll(x => x.FieldName == "misc"
                          && x.FieldType == SheetFieldType.CheckBox
                          && !string.IsNullOrEmpty(x.UiLabelMobile)
                          && string.IsNullOrEmpty(x.RadioButtonGroup))
            .Except(listSheetFieldDefsRadio1.Union(listSheetFieldDefsRadio2))
            .ToList();
        if (listSheetFieldDefsRadio1.Any(x => CompareSheetFieldDefsByValueForMobileLayout(x, sheetFieldDef)))
            listSheetFieldDefsRetVal = listSheetFieldDefsRadio1.GroupBy(x => x.RadioButtonGroup).Where(x => x.Key == sheetFieldDef.RadioButtonGroup).SelectMany(x => x).ToList();
        else if (listSheetFieldDefsRadio2.Any(x => CompareSheetFieldDefsByValueForMobileLayout(x, sheetFieldDef)))
            listSheetFieldDefsRetVal = listSheetFieldDefsRadio2.GroupBy(x => x.FieldName).Where(x => x.Key == sheetFieldDef.FieldName).SelectMany(x => x).ToList();
        else if (listSheetFieldDefsCheckboxGroups.Any(x => CompareSheetFieldDefsByValueForMobileLayout(x, sheetFieldDef)))
            listSheetFieldDefsRetVal = listSheetFieldDefsCheckboxGroups.GroupBy(x => x.UiLabelMobile).Where(x => x.Key == sheetFieldDef.UiLabelMobile).SelectMany(x => x).ToList();
        else
            listSheetFieldDefsRetVal.AddRange(listSheetFieldDefs.FindAll(x => CompareSheetFieldDefsByValueForMobileLayout(x, sheetFieldDef)));
        return listSheetFieldDefsRetVal;
    }

    public static string GetUiLabelMobileRadioButton(SheetFieldDef sheetFieldDef)
    {
        if (sheetFieldDef == null || sheetFieldDef.FieldType != SheetFieldType.CheckBox) return "";
        if (string.IsNullOrEmpty(sheetFieldDef.UiLabelMobileRadioButton)) return sheetFieldDef.RadioButtonValue;
        return sheetFieldDef.UiLabelMobileRadioButton;
    }

    public static bool CompareSheetFieldDefsByValueForMobileLayout(SheetFieldDef sheetFieldDefA, SheetFieldDef sheetFieldDefB, bool ignoreLanguage = false)
    {
        if (!ignoreLanguage)
        {
            if (sheetFieldDefA.SheetDefNum != sheetFieldDefB.SheetDefNum) return false;
            if (sheetFieldDefA.Language != sheetFieldDefB.Language) return false;
            if (sheetFieldDefA.SheetFieldDefNum != sheetFieldDefB.SheetFieldDefNum)
                //For new sheet fields this will be 0, hence needing to compare all other fields.
                return false;
        }

        if (sheetFieldDefA.FieldName != sheetFieldDefB.FieldName) return false;
        if (sheetFieldDefA.FieldType != sheetFieldDefB.FieldType) return false;
        if (sheetFieldDefA.FieldValue != sheetFieldDefB.FieldValue) return false;
        if (sheetFieldDefA.RadioButtonGroup != sheetFieldDefB.RadioButtonGroup) return false;
        if (sheetFieldDefA.RadioButtonValue != sheetFieldDefB.RadioButtonValue) return false;
        if (sheetFieldDefA.UiLabelMobile != sheetFieldDefB.UiLabelMobile) return false;
        if (GetUiLabelMobileRadioButton(sheetFieldDefA) != GetUiLabelMobileRadioButton(sheetFieldDefB))
            if (sheetFieldDefA.SheetFieldDefNum != sheetFieldDefB.SheetFieldDefNum)
                return false;

        if (sheetFieldDefA.XPos != sheetFieldDefB.XPos || sheetFieldDefA.YPos != sheetFieldDefB.YPos)
            if (sheetFieldDefA.SheetFieldDefNum != sheetFieldDefB.SheetFieldDefNum)
                return false;

        return true;
    }

    public static void Delete(long sheetFieldDefNum)
    {
        SheetFieldDefCrud.Delete(sheetFieldDefNum);
    }

    public static void Sync(List<SheetFieldDef> listSheetFieldDefs, long sheetDefNum)
    {
        var listSheetFieldDefsDB = GetForSheetDef(sheetDefNum);
        SheetFieldDefCrud.Sync(listSheetFieldDefs, listSheetFieldDefsDB);
    }

    public static int CompareTabOrder(SheetFieldDef sheetFieldDef1, SheetFieldDef sheetFieldDef2)
    {
        if (sheetFieldDef1.FieldType == sheetFieldDef2.FieldType)
        {
            //do nothing
        }
        else if (sheetFieldDef1.FieldType == SheetFieldType.Image)
        {
            //Always move images to the top of the list. This is because of the way the sheet is drawn.
            return -1;
        }
        else if (sheetFieldDef2.FieldType == SheetFieldType.Image)
        {
            //Always move images to the top of the list. This is because of the way the sheet is drawn.
            return 1;
        }
        else if (sheetFieldDef1.FieldType == SheetFieldType.PatImage)
        {
            //Move PatImage to the top of the list under images.
            return -1;
        }
        else if (sheetFieldDef2.FieldType == SheetFieldType.PatImage)
        {
            //Move PatImage to the top of the list under images.
            return 1;
        }
        else if (sheetFieldDef1.FieldType == SheetFieldType.Special)
        {
            //Move Special to the top of the list under PatImages.
            return -1;
        }
        else if (sheetFieldDef2.FieldType == SheetFieldType.Special)
        {
            //Move Special to the top of the list under PatImages.
            return 1;
        }
        else if (sheetFieldDef1.FieldType == SheetFieldType.OutputText)
        {
            //Move Output text to the top of the list under Special.
            return -1;
        }
        else if (sheetFieldDef2.FieldType == SheetFieldType.OutputText)
        {
            //Move Output text to the top of the list under Special.
            return 1;
        }
        else if (sheetFieldDef1.FieldType == SheetFieldType.MobileHeader)
        {
            //Move MobileHeader to bottom
            return 1;
        }
        else if (sheetFieldDef2.FieldType == SheetFieldType.MobileHeader)
        {
            //Move MobileHeader to bottom
            return -1;
        }

        if (sheetFieldDef1.TabOrder != sheetFieldDef2.TabOrder) return sheetFieldDef1.TabOrder - sheetFieldDef2.TabOrder;
        var intComp = (sheetFieldDef1.FieldName + sheetFieldDef1.RadioButtonValue).CompareTo(sheetFieldDef2.FieldName + sheetFieldDef2.RadioButtonValue); //RadioButtionValuecan be filled or ""
        if (intComp != 0) return intComp;
        intComp = sheetFieldDef1.YPos - sheetFieldDef2.YPos; //arbitrarily order by YPos if both controls have the same tab orer and name. This will only happen if both fields are either identical or if they are both misc fields.
        if (intComp != 0) return intComp;
        intComp = sheetFieldDef1.XPos - sheetFieldDef2.XPos; //If tabOrder, Name, and YPos are equal then compare based on X coordinate. 
        if (intComp != 0) return intComp;
        return sheetFieldDef1.TabOrderMobile - sheetFieldDef2.TabOrderMobile;
    }

    private class SheetFieldDefCache : CacheListAbs<SheetFieldDef>
    {
        protected override List<SheetFieldDef> GetCacheFromDb()
        {
            var command = "SELECT * FROM sheetfielddef ORDER BY SheetDefNum";
            return SheetFieldDefCrud.SelectMany(command);
        }

        protected override List<SheetFieldDef> TableToList(DataTable dataTable)
        {
            return SheetFieldDefCrud.TableToList(dataTable);
        }

        protected override SheetFieldDef Copy(SheetFieldDef item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<SheetFieldDef> items)
        {
            return SheetFieldDefCrud.ListToTable(items, "SheetFieldDef");
        }

        protected override void FillCacheIfNeeded()
        {
            SheetFieldDefs.GetTableFromCache(false);
        }
    }

    private static readonly SheetFieldDefCache Cache = new();

    public static List<SheetFieldDef> GetWhere(Predicate<SheetFieldDef> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}