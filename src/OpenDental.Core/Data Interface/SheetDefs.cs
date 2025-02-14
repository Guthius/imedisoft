using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness;

public class SheetDefs
{
    public static SheetDef GetSheetDef(long sheetDefNum, bool hasExceptions = true)
    {
        var sheetDef = GetFirstOrDefault(x => x.SheetDefNum == sheetDefNum);
        if (hasExceptions || sheetDef != null) GetFieldsAndParameters(sheetDef);
        return sheetDef;
    }

    public static void InsertOrUpdate(SheetDef sheetDef, bool isOldSheetDuplicate = false)
    {
        if (sheetDef.IsNew)
        {
            if (!isOldSheetDuplicate) sheetDef.DateTCreated = MiscData.GetNowDateTime();
            sheetDef.SheetDefNum = SheetDefCrud.Insert(sheetDef);
        }
        else
        {
            SheetDefCrud.Update(sheetDef);
        }

        for (var i = 0; i < sheetDef.SheetFieldDefs.Count; i++) sheetDef.SheetFieldDefs[i].SheetDefNum = sheetDef.SheetDefNum;
        SheetFieldDefs.Sync(sheetDef.SheetFieldDefs, sheetDef.SheetDefNum);
    }

    public static void DeleteObject(long sheetDefNum)
    {
        //validate that not already in use by a refferral.
        var command = "SELECT LName,FName FROM referral WHERE Slip=" + sheetDefNum;
        var table = DataCore.GetTable(command);
        //int count=PIn.PInt(Db.GetCount(command));
        var referralNames = "";
        for (var i = 0; i < table.Rows.Count; i++)
        {
            if (i > 0) referralNames += ", ";
            referralNames += table.Rows[i]["FName"] + " " + table.Rows[i]["LName"];
        }

        if (table.Rows.Count > 0) throw new ApplicationException(Lans.g("sheetDefs", "SheetDef is already in use by referrals. Not allowed to delete.") + " " + referralNames);
        //validate that not already in use by automation.
        command = "SELECT AutomationNum FROM automation WHERE SheetDefNum=" + sheetDefNum;
        table = DataCore.GetTable(command);
        if (table.Rows.Count > 0) throw new ApplicationException(Lans.g("sheetDefs", "SheetDef is in use by automation. Not allowed to delete."));
        //validate that not already in use by a laboratory
        command = "SELECT Description FROM laboratory WHERE Slip=" + sheetDefNum;
        table = DataCore.GetTable(command);
        if (table.Rows.Count > 0)
            throw new ApplicationException(Lans.g("sheetDefs", "SheetDef is in use by laboratories. Not allowed to delete.")
                                           + "\r\n" + string.Join(", ", table.Select().Select(x => x["Description"].ToString())));
        //validate that not already in use as a default sheet
        var listPrefNamesDefault = new List<PrefName>();
        listPrefNamesDefault.Add(PrefName.SheetsDefaultChartModule);
        listPrefNamesDefault.Add(PrefName.SheetsDefaultLimited);
        listPrefNamesDefault.Add(PrefName.SheetsDefaultStatement);
        listPrefNamesDefault.Add(PrefName.SheetsDefaultInvoice);
        listPrefNamesDefault.Add(PrefName.SheetsDefaultReceipt);
        listPrefNamesDefault.Add(PrefName.SheetsDefaultTreatmentPlan);
        if (listPrefNamesDefault.Any(x => PrefC.GetLong(x) == sheetDefNum)) throw new ApplicationException(Lans.g("sheetDefs", "SheetDef is in use as a default sheet. Not allowed to delete."));
        //validate that not already in use by clinicPref.
        var listPrefNamesClinicDefault = new List<PrefName>();
        listPrefNamesClinicDefault.Add(PrefName.SheetsDefaultChartModule);
        listPrefNamesClinicDefault.Add(PrefName.SheetsDefaultTreatmentPlan);
        command = "SELECT ClinicNum "
                  + "FROM clinicpref "
                  + "WHERE ValueString='" + sheetDefNum + "' "
                  + "AND PrefName IN(" + string.Join(",", listPrefNamesClinicDefault.Select(x => "'" + x + "'")) + ") ";
        table = DataCore.GetTable(command);
        if (table.Rows.Count > 0)
            throw new ApplicationException(Lans.g("sheetDefs", "SheetDef is in use by clinics. Not allowed to delete.")
                                           + "\r\n" + string.Join(", ", table.Select().Select(x => Clinics.GetAbbr(SIn.Long(x["ClinicNum"].ToString())))));
        //validate that not already in use by eClipboard
        command = "SELECT EClipboardSheetDefNum,ClinicNum FROM eclipboardsheetdef WHERE SheetDefNum=" + sheetDefNum;
        table = DataCore.GetTable(command);
        if (table.Rows.Count > 0)
        {
            if (true)
                throw new ApplicationException(Lans.g("sheetDefs", "SheetDef is in use by eClipboard. Not allowed to delete.")
                                               + "\r\n" + string.Join(", ", table.Select()
                                                   .Select(x => Clinics.GetAbbr(SIn.Long(x["ClinicNum"].ToString())))
                                                   .Select(x => string.IsNullOrEmpty(x) ? "Default" : x)));

            throw new ApplicationException(Lans.g("sheetDefs", "SheetDef is in use by eClipboard. Not allowed to delete."));
        }

        //Set payplan.SheetDefNum to 0. Setting it to 0 will use the default or internal payplan sheet type.
        command = "UPDATE payplan SET payplan.SheetDefNum = 0 WHERE payplan.SheetDefNum = " + sheetDefNum;
        Db.NonQ(command);
        //Set payplantemplate.sheetDefNum to 0.
        //We don't have to worry about clinics because sheet defs are not clinic specific.
        command = "UPDATE payplantemplate SET payplantemplate.SheetDefNum = 0 WHERE payplantemplate.SheetDefNum = " + sheetDefNum;
        Db.NonQ(command);
        command = "DELETE FROM grouppermission"
                  + " WHERE FKey=" + sheetDefNum
                  + " AND PermType=" + SOut.Enum(EnumPermType.DashboardWidget);
        Db.NonQ(command);
        command = "DELETE FROM sheetfielddef WHERE SheetDefNum=" + sheetDefNum;
        Db.NonQ(command);
        SheetDefCrud.Delete(sheetDefNum);
    }

    public static void GetFieldsAndParameters(SheetDef sheetdef)
    {
        //images first
        sheetdef.SheetFieldDefs = SheetFieldDefs.GetWhere(x => x.SheetDefNum == sheetdef.SheetDefNum && x.FieldType == SheetFieldType.Image);
        //then all other fields
        sheetdef.SheetFieldDefs.AddRange(SheetFieldDefs.GetWhere(x => x.SheetDefNum == sheetdef.SheetDefNum
                                                                      && x.FieldType != SheetFieldType.Image
                                                                      && x.FieldType != SheetFieldType.Parameter)); //Defs never store parameters. Fields store filled parameters, but that's different.
        sheetdef.Parameters = SheetParameter.GetForType(sheetdef.SheetType);
    }

    public static List<SheetDef> GetCustomForType(SheetTypeEnum sheetType)
    {
        return GetWhere(x => x.SheetType == sheetType);
    }

    public static string GetDescription(long sheetDefNum)
    {
        var sheetDef = GetFirstOrDefault(x => x.SheetDefNum == sheetDefNum);
        if (sheetDef == null) return "";
        return sheetDef.Description;
    }

    public static SheetDef GetInternalOrCustom(SheetInternalType sheetInternalType)
    {
        var sheetDefRetVal = SheetsInternal.GetSheetDef(sheetInternalType);
        var sheetDefCustom = GetCustomForType(sheetDefRetVal.SheetType).OrderBy(x => x.Description).ThenBy(x => x.SheetDefNum).FirstOrDefault();
        if (sheetDefCustom != null) sheetDefRetVal = GetSheetDef(sheetDefCustom.SheetDefNum);
        return sheetDefRetVal;
    }

    public static SheetDef GetSheetsDefault(SheetTypeEnum sheetType, long clinicNum = 0)
    {
        var clinicPref = ClinicPrefs.GetPref(Prefs.GetSheetDefPref(sheetType), clinicNum);
        SheetDef sheetDefDefault;
        if (clinicPref == null)
        {
            //If there wasn't a row for the specific clinic, use the base default sheetdef
            sheetDefDefault = GetSheetDef(PrefC.GetDefaultSheetDefNum(sheetType), false);
            if (sheetDefDefault == null) sheetDefDefault = SheetsInternal.GetSheetDef(sheetType);
            return sheetDefDefault; //Return the base default sheetdef
        }

        //Clinic specific sheet def found
        if (SIn.Long(clinicPref.ValueString) == 0) //If ValueString is 0 then we want to keep it as the internal sheet def.
            sheetDefDefault = SheetsInternal.GetSheetDef(sheetType);
        else
            sheetDefDefault = GetSheetDef(SIn.Long(clinicPref.ValueString), false);
        return sheetDefDefault;
    }

    public static void SetPatImageFieldNames(SheetDef sheetDef)
    {
        //We need to figure out which Image Category should be used for any PatImage SheetFieldDefs.
        var listDefsImage = Defs.GetDefsForCategory(DefCat.ImageCats, true);
        long defNum = 0;
        //A user can define a specific image category as being the Patient Picture definition, see FormDefEditImages.butOK_Click().
        //SheetFieldDef.FieldName corresponds to Def.DefNum for a PatImage type SheetFieldDef.
        var def = listDefsImage.FirstOrDefault(x => x.ItemValue.Contains("P"));
        if (def == null) def = listDefsImage.FirstOrDefault(); //Default to the first image category definition if one isn't defined as the Patient Image definition.
        if (def == null) //No Image Category definitions setup.
            defNum = 0;
        else
            defNum = def.DefNum;
        for (var i = 0; i < sheetDef.SheetFieldDefs.Count; i++)
        {
            if (sheetDef.SheetFieldDefs[i].FieldType != SheetFieldType.PatImage) continue;
            sheetDef.SheetFieldDefs[i].FieldName = defNum.ToString();
        }
    }

    public static List<SheetFieldType> GetVisibleButtons(SheetTypeEnum sheetType)
    {
        var listSheetFieldTypes = new List<SheetFieldType>();
        if (sheetType == SheetTypeEnum.ChartModule)
        {
            listSheetFieldTypes.Add(SheetFieldType.Grid);
            listSheetFieldTypes.Add(SheetFieldType.Special);
            return listSheetFieldTypes;
        }

        if (sheetType == SheetTypeEnum.PatientDashboardWidget)
        {
            listSheetFieldTypes.Add(SheetFieldType.StaticText);
            listSheetFieldTypes.Add(SheetFieldType.PatImage);
            listSheetFieldTypes.Add(SheetFieldType.Grid);
            listSheetFieldTypes.Add(SheetFieldType.Special);
            listSheetFieldTypes.Add(SheetFieldType.Line);
            listSheetFieldTypes.Add(SheetFieldType.Rectangle);
            return listSheetFieldTypes;
        }

        listSheetFieldTypes.Add(SheetFieldType.OutputText);
        listSheetFieldTypes.Add(SheetFieldType.InputField);
        listSheetFieldTypes.Add(SheetFieldType.StaticText);
        listSheetFieldTypes.Add(SheetFieldType.Image);
        listSheetFieldTypes.Add(SheetFieldType.Line);
        listSheetFieldTypes.Add(SheetFieldType.Rectangle);
        if (!sheetType.In(SheetTypeEnum.ERA, SheetTypeEnum.ERAGridHeader)) listSheetFieldTypes.Add(SheetFieldType.CheckBox);
        if (!sheetType.In(SheetTypeEnum.DepositSlip, SheetTypeEnum.ERA, SheetTypeEnum.ERAGridHeader)) listSheetFieldTypes.Add(SheetFieldType.PatImage);
        if (!sheetType.In(SheetTypeEnum.DepositSlip,
                SheetTypeEnum.ERA,
                SheetTypeEnum.ERAGridHeader,
                SheetTypeEnum.RoutingSlip,
                SheetTypeEnum.LabelCarrier,
                SheetTypeEnum.LabelPatient,
                SheetTypeEnum.LabelReferral,
                SheetTypeEnum.LabelAppointment,
                SheetTypeEnum.Statement,
                SheetTypeEnum.TreatmentPlan))
            listSheetFieldTypes.Add(SheetFieldType.ComboBox);
        if (!sheetType.In(SheetTypeEnum.DepositSlip,
                SheetTypeEnum.ERA,
                SheetTypeEnum.ERAGridHeader,
                SheetTypeEnum.RoutingSlip,
                SheetTypeEnum.LabelCarrier,
                SheetTypeEnum.LabelPatient,
                SheetTypeEnum.LabelReferral,
                SheetTypeEnum.LabelAppointment,
                SheetTypeEnum.Statement))
            listSheetFieldTypes.Add(SheetFieldType.SigBox);
        if (sheetType == SheetTypeEnum.TreatmentPlan) listSheetFieldTypes.Add(SheetFieldType.SigBoxPractice);
        if (sheetType.In(SheetTypeEnum.TreatmentPlan, SheetTypeEnum.ReferralLetter)) listSheetFieldTypes.Add(SheetFieldType.Special);
        if (sheetType.In(SheetTypeEnum.Statement, SheetTypeEnum.MedLabResults, SheetTypeEnum.TreatmentPlan, SheetTypeEnum.PaymentPlan,
                SheetTypeEnum.ReferralLetter, SheetTypeEnum.ERA, SheetTypeEnum.Consent, SheetTypeEnum.PatientForm, SheetTypeEnum.PatientLetter))
            listSheetFieldTypes.Add(SheetFieldType.Grid);
        if (IsMobileAllowed(sheetType)) listSheetFieldTypes.Add(SheetFieldType.MobileHeader);
        return listSheetFieldTypes;
    }

    public static bool CanBypassLockDate(long sheetDefNum)
    {
        var sheetDef = GetFirstOrDefault(x => x.SheetDefNum == sheetDefNum);
        if (sheetDef == null) return false;
        return sheetDef.BypassGlobalLock == BypassLockStatus.BypassAlways;
    }

    public static bool ContainsStaticFields(SheetDef sheetDef, params EnumStaticTextField[] staticTextFieldArray)
    {
        if (sheetDef.SheetFieldDefs.IsNullOrEmpty() || staticTextFieldArray.IsNullOrEmpty()) return false;
        var listStaticTextFields = staticTextFieldArray.Select(x => x.ToReplacementString()).ToList();
        for (var i = 0; i < sheetDef.SheetFieldDefs.Count; i++)
        {
            if (sheetDef.SheetFieldDefs[i].FieldType != SheetFieldType.StaticText) continue;
            if (listStaticTextFields.Any(x => sheetDef.SheetFieldDefs[i].FieldValue.Contains(x))) return true;
        }

        return false;
    }

    public static bool ContainsGrids(SheetDef sheetDef, params string[] gridNameArray)
    {
        if (sheetDef.SheetFieldDefs.IsNullOrEmpty() || gridNameArray.IsNullOrEmpty()) return false;
        var listGrids = gridNameArray.ToList();
        for (var i = 0; i < sheetDef.SheetFieldDefs.Count; i++)
        {
            if (sheetDef.SheetFieldDefs[i].FieldType != SheetFieldType.Grid) continue;
            if (listGrids.Any(x => sheetDef.SheetFieldDefs[i].FieldName.Contains(x))) return true;
        }

        return false;
    }

    public static bool IsWebFormAllowed(SheetTypeEnum sheetType)
    {
        if (sheetType.In(SheetTypeEnum.PatientForm, SheetTypeEnum.MedicalHistory)) return true;
        return false;
    }

    public static bool IsMobileAllowed(SheetTypeEnum sheetType)
    {
        if (IsWebFormAllowed(sheetType)) return true;
        if (sheetType == SheetTypeEnum.Consent) return true;
        if (sheetType == SheetTypeEnum.ExamSheet) return true;
        return false;
    }

    public static bool IsDashboardType(SheetDef sheetDef)
    {
        return IsDashboardType(sheetDef.SheetType);
    }

    public static bool IsDashboardType(SheetTypeEnum sheetType)
    {
        if (sheetType.In(SheetTypeEnum.PatientDashboard, SheetTypeEnum.PatientDashboardWidget)) return true;
        return false;
    }

    private class SheetDefCache : CacheListAbs<SheetDef>
    {
        protected override List<SheetDef> GetCacheFromDb()
        {
            var command = "SELECT * FROM sheetdef ORDER BY Description,SheetDefNum";
            return SheetDefCrud.SelectMany(command);
        }

        protected override List<SheetDef> TableToList(DataTable dataTable)
        {
            return SheetDefCrud.TableToList(dataTable);
        }

        protected override SheetDef Copy(SheetDef item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<SheetDef> items)
        {
            return SheetDefCrud.ListToTable(items, "SheetDef");
        }

        protected override void FillCacheIfNeeded()
        {
            SheetDefs.GetTableFromCache(false);
        }
    }

    private static readonly SheetDefCache Cache = new();

    public static List<SheetDef> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static List<SheetDef> GetWhere(Predicate<SheetDef> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static SheetDef GetFirstOrDefault(Func<SheetDef, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
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