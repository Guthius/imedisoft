﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class SheetFieldDefs
{
    ///<summary>Gets all internal SheetFieldDefs from the database for a specific sheet, used in FormSheetFieldExam.</summary>
    public static List<SheetFieldDef> GetForExamSheet(long sheetDefNum)
    {
        var command = "SELECT * FROM sheetfielddef WHERE SheetDefNum=" + SOut.Long(sheetDefNum) + " "
                      + "AND ((FieldName!='misc' AND FieldName!='') OR (ReportableName!='')) "
                      + "GROUP BY FieldName,ReportableName";
        return SheetFieldDefCrud.SelectMany(command);
    }

    ///<summary>Gets all SheetFieldDefs from the database for a specific sheet, used in FormSheetFieldExam.</summary>
    public static List<SheetFieldDef> GetForSheetDef(long sheetDefNum)
    {
        var command = "SELECT * FROM sheetfielddef WHERE SheetDefNum=" + SOut.Long(sheetDefNum);
        return SheetFieldDefCrud.SelectMany(command);
    }

    /// <summary>
    ///     Evaluates input fields an determines if this is a SheetField (or SheetFieldDef) that would be shown in mobile
    ///     layout.
    ///     Returns true if criteria is met to show this field in mobile layout. Otherwise returns false.
    /// </summary>
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

    /// <summary>
    ///     Returns a group of checkboxes from listSheetFieldDefs which are grouped with the passed in checkbox.
    ///     Returns a single checkbox if no group members found, or an empty list if no checkboxes are found from group.
    /// </summary>
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

    /// <summary>
    ///     Returns the UI Label text for a radiobutton. Misc will use UiLabelMobileRadioButton text, pre-defined will use
    ///     RadioButtonValue
    /// </summary>
    public static string GetUiLabelMobileRadioButton(SheetFieldDef sheetFieldDef)
    {
        if (sheetFieldDef == null || sheetFieldDef.FieldType != SheetFieldType.CheckBox) return "";
        if (string.IsNullOrEmpty(sheetFieldDef.UiLabelMobileRadioButton)) return sheetFieldDef.RadioButtonValue;
        return sheetFieldDef.UiLabelMobileRadioButton;
    }

    /// <summary>
    ///     Compares sheet fields by value. Returns true if properties are the same, false otherwise. Omits compairing tab
    ///     ordering, and location.
    /// </summary>
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

    ///<Summary>Gets one SheetFieldDef from the database.</Summary>
    public static SheetFieldDef CreateObject(long sheetFieldDefNum)
    {
        return SheetFieldDefCrud.SelectOne(sheetFieldDefNum);
    }

    
    public static long Insert(SheetFieldDef sheetFieldDef)
    {
        return SheetFieldDefCrud.Insert(sheetFieldDef);
    }

    
    public static void Update(SheetFieldDef sheetFieldDef)
    {
        SheetFieldDefCrud.Update(sheetFieldDef);
    }

    
    public static void Delete(long sheetFieldDefNum)
    {
        SheetFieldDefCrud.Delete(sheetFieldDefNum);
    }

    /// <summary>
    ///     Inserts, updates, or deletes database rows to match supplied list. Must always pass in sheetDefNum.
    ///     This function uses a DB comparison rather than a stale list because we are not worried about concurrency of a
    ///     single sheet and enhancing the
    ///     functions that call this would take a lot of restructuring.
    /// </summary>
    public static void Sync(List<SheetFieldDef> listSheetFieldDefs, long sheetDefNum)
    {
        var listSheetFieldDefsDB = GetForSheetDef(sheetDefNum);
        SheetFieldDefCrud.Sync(listSheetFieldDefs, listSheetFieldDefsDB);
    }


    /// <summary>
    ///     Sorts fields in the order that they shoudl be drawn on top of eachother. First Images, then Drawings, Lines,
    ///     Rectangles, Text, Check Boxes, and SigBoxes. In that order.
    /// </summary>
    public static int SortDrawingOrderLayers(SheetFieldDef sheetFieldDef1, SheetFieldDef sheetFieldDef2)
    {
        if (sheetFieldDef1.FieldType != sheetFieldDef2.FieldType) return SheetFields.FieldTypeSortOrder(sheetFieldDef1.FieldType).CompareTo(SheetFields.FieldTypeSortOrder(sheetFieldDef2.FieldType));
        return sheetFieldDef1.YPos.CompareTo(sheetFieldDef2.YPos);
        //return f1.SheetFieldNum.CompareTo(f2.SheetFieldNum);
    }

    /// <summary>
    ///     This is a comparator function used by List&lt;T&gt;.Sort()
    ///     When compairing SheetFieldDef.TabOrder it returns a negative number if def1&lt;def2, 0 if def1==def2, and a
    ///     positive number if def1&gt;def2.
    ///     Does not handle null values, but there should never be any instances of null being passed in.
    ///     Must always return 0 when compairing item to itself.
    ///     This function should probably be moved to SheetFieldDefs.
    /// </summary>
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

    #region CachePattern

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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly SheetFieldDefCache _sheetFieldDefCache = new();

    public static List<SheetFieldDef> GetWhere(Predicate<SheetFieldDef> match, bool isShort = false)
    {
        return _sheetFieldDefCache.GetWhere(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _sheetFieldDefCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _sheetFieldDefCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _sheetFieldDefCache.ClearCache();
    }

    #endregion
}