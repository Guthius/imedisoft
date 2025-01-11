using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class SheetDefs
{
    ///<Summary>Gets one SheetDef from the cache.  Also includes the fields and parameters for the sheetdef.</Summary>
    public static SheetDef GetSheetDef(long sheetDefNum, bool hasExceptions = true)
    {
        var sheetDef = GetFirstOrDefault(x => x.SheetDefNum == sheetDefNum);
        if (hasExceptions || sheetDef != null) GetFieldsAndParameters(sheetDef);
        return sheetDef;
    }

    ///<summary>Updates the SheetDef only.  Does not included attached fields.</summary>
    public static long Update(SheetDef sheetDef)
    {
        SheetDefCrud.Update(sheetDef);
        return sheetDef.SheetDefNum;
    }

    /// <summary>Includes all attached fields.  Intelligently inserts, updates, or deletes old fields.</summary>
    /// <param name="isOldSheetDuplicate">
    ///     True if the sheetDef being created is a copy of a custom sheet that has a DateTCreated of 0001-01-01.
    ///     DateTCreated determines whether or not text fields will be shifted up 5 pixels when PDF is created from sheet to
    ///     fix bug job B16020.
    /// </param>
    public static long InsertOrUpdate(SheetDef sheetDef, bool isOldSheetDuplicate = false)
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
        return sheetDef.SheetDefNum;
    }

    
    public static void DeleteObject(long sheetDefNum)
    {
        //validate that not already in use by a refferral.
        var command = "SELECT LName,FName FROM referral WHERE Slip=" + SOut.Long(sheetDefNum);
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
        command = "SELECT AutomationNum FROM automation WHERE SheetDefNum=" + SOut.Long(sheetDefNum);
        table = DataCore.GetTable(command);
        if (table.Rows.Count > 0) throw new ApplicationException(Lans.g("sheetDefs", "SheetDef is in use by automation. Not allowed to delete."));
        //validate that not already in use by a laboratory
        command = "SELECT Description FROM laboratory WHERE Slip=" + SOut.Long(sheetDefNum);
        table = DataCore.GetTable(command);
        if (table.Rows.Count > 0)
            throw new ApplicationException(Lans.g("sheetDefs", "SheetDef is in use by laboratories. Not allowed to delete.")
                                           + "\r\n" + string.Join(", ", table.Select().Select(x => x["Description"].ToString())));
        //validate that not already in use as a default sheet
        var listPrefNamesDefault = new List<PrefName>();
        listPrefNamesDefault.Add(PrefName.SheetsDefaultChartModule);
        listPrefNamesDefault.Add(PrefName.SheetsDefaultRx);
        listPrefNamesDefault.Add(PrefName.SheetsDefaultLimited);
        listPrefNamesDefault.Add(PrefName.SheetsDefaultStatement);
        listPrefNamesDefault.Add(PrefName.SheetsDefaultInvoice);
        listPrefNamesDefault.Add(PrefName.SheetsDefaultReceipt);
        listPrefNamesDefault.Add(PrefName.SheetsDefaultTreatmentPlan);
        if (listPrefNamesDefault.Any(x => PrefC.GetLong(x) == sheetDefNum)) throw new ApplicationException(Lans.g("sheetDefs", "SheetDef is in use as a default sheet. Not allowed to delete."));
        //validate that not already in use by clinicPref.
        var listPrefNamesClinicDefault = new List<PrefName>();
        listPrefNamesClinicDefault.Add(PrefName.SheetsDefaultRx);
        listPrefNamesClinicDefault.Add(PrefName.SheetsDefaultChartModule);
        listPrefNamesClinicDefault.Add(PrefName.SheetsDefaultTreatmentPlan);
        command = "SELECT ClinicNum "
                  + "FROM clinicpref "
                  + "WHERE ValueString='" + SOut.Long(sheetDefNum) + "' "
                  + "AND PrefName IN(" + string.Join(",", listPrefNamesClinicDefault.Select(x => "'" + x + "'")) + ") ";
        table = DataCore.GetTable(command);
        if (table.Rows.Count > 0)
            throw new ApplicationException(Lans.g("sheetDefs", "SheetDef is in use by clinics. Not allowed to delete.")
                                           + "\r\n" + string.Join(", ", table.Select().Select(x => Clinics.GetAbbr(SIn.Long(x["ClinicNum"].ToString())))));
        //validate that not already in use by eClipboard
        command = "SELECT EClipboardSheetDefNum,ClinicNum FROM eclipboardsheetdef WHERE SheetDefNum=" + SOut.Long(sheetDefNum);
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
        command = "UPDATE payplan SET payplan.SheetDefNum = 0 WHERE payplan.SheetDefNum = " + SOut.Long(sheetDefNum);
        Db.NonQ(command);
        //Set payplantemplate.sheetDefNum to 0.
        //We don't have to worry about clinics because sheet defs are not clinic specific.
        command = "UPDATE payplantemplate SET payplantemplate.SheetDefNum = 0 WHERE payplantemplate.SheetDefNum = " + SOut.Long(sheetDefNum);
        Db.NonQ(command);
        command = "DELETE FROM grouppermission"
                  + " WHERE FKey=" + SOut.Long(sheetDefNum)
                  + " AND PermType=" + SOut.Enum(EnumPermType.DashboardWidget);
        Db.NonQ(command);
        command = "DELETE FROM sheetfielddef WHERE SheetDefNum=" + SOut.Long(sheetDefNum);
        Db.NonQ(command);
        SheetDefCrud.Delete(sheetDefNum);
    }

    /// <summary>
    ///     Sheetdefs and sheetfielddefs are archived separately.
    ///     So when we need to use a sheetdef, we must run this method to pull all the associated fields from the archive.
    ///     Then it will be ready for printing, copying, etc.
    /// </summary>
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

    ///<summary>Gets all custom sheetdefs(without fields or parameters) for a particular type.</summary>
    public static List<SheetDef> GetCustomForType(SheetTypeEnum sheetType)
    {
        return GetWhere(x => x.SheetType == sheetType);
    }

    ///<summary>Gets the description from the cache.</summary>
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

    /// <summary>
    ///     Passing in a clinicNum of 0 will use the base default sheet def.  Otherwise returns the clinic specific
    ///     default sheetdef.
    /// </summary>
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

    /// <summary>
    ///     Gets a list of sheetdefs from the DB. Used by the API. If modifying this method, please contact someone from
    ///     the API team.
    /// </summary>
    public static List<SheetDef> GetSheetDefsForApi(int intLimit, int intOffset)
    {
        var command = "SELECT * FROM sheetdef ";
        command += "ORDER BY SheetDefNum "
                   + "LIMIT " + SOut.Int(intOffset) + ", " + SOut.Int(intLimit);
        return SheetDefCrud.SelectMany(command);
    }

    /// <summary>
    ///     Sets the FieldName for each SheetFieldDef in sheetDef.SheetFieldDefs to the Def.DefNum defined as the Patient Image
    ///     definition.
    ///     Defaults to the first definition in the Image category if Patient Image is not defined.
    ///     This is necessary because the resource for the internal sheet likely does not contain a valid Def primary key.
    /// </summary>
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
            sheetDef.SheetFieldDefs[i].FieldName = SOut.Long(defNum);
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
        if (sheetType == SheetTypeEnum.Screening) listSheetFieldTypes.Add(SheetFieldType.ScreenChart);
        if (IsMobileAllowed(sheetType)) listSheetFieldTypes.Add(SheetFieldType.MobileHeader);
        return listSheetFieldTypes;
    }

    #region Misc Methods

    ///<summary>Returns true if this sheet def is allowed to bypass the global lock date.</summary>
    public static bool CanBypassLockDate(long sheetDefNum)
    {
        var sheetDef = GetFirstOrDefault(x => x.SheetDefNum == sheetDefNum);
        if (sheetDef == null) return false;
        return sheetDef.BypassGlobalLock == BypassLockStatus.BypassAlways;
    }

    /// <summary>
    ///     Returns true if any StaticText fields on the sheet def contain any of the StaticTextFields passed in.
    ///     Otherwise false.
    /// </summary>
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

    ///<summary>Returns true if any Grids on the sheet def contain any of the specific Grids passed in. Otherwise false.</summary>
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

    ///<summary>SheetType must either by PatientForm of MedicalHistory.</summary>
    public static bool IsWebFormAllowed(SheetTypeEnum sheetType)
    {
        if (sheetType.In(SheetTypeEnum.PatientForm, SheetTypeEnum.MedicalHistory)) return true;
        return false;
    }

    ///<summary>SheetType must either by PatientForm of MedicalHistory.</summary>
    public static bool IsMobileAllowed(SheetTypeEnum sheetType)
    {
        if (IsWebFormAllowed(sheetType)) return true;
        if (sheetType == SheetTypeEnum.Consent) return true;
        if (sheetType == SheetTypeEnum.ExamSheet) return true;
        return false;
    }

    ///<summary>Determines if a sheetDef is of a SheetTypeEnum that describes a Dashboard.</summary>
    public static bool IsDashboardType(SheetDef sheetDef)
    {
        return IsDashboardType(sheetDef.SheetType);
    }

    ///<summary>Determines if a SheetTypeEnum is a Dashboard.</summary>
    public static bool IsDashboardType(SheetTypeEnum sheetType)
    {
        if (sheetType.In(SheetTypeEnum.PatientDashboard, SheetTypeEnum.PatientDashboardWidget)) return true;
        return false;
    }

    #endregion

    #region CachePattern

    private class SheetDefCache : CacheListAbs<SheetDef>
    {
	    /// <summary>
	    ///     Ordered by Description and then SheetDefNum to be a deterministic sorting.  This matches the sorting in
	    ///     GetInternalOrCustom().
	    ///     So the order in the grid matches the order when choosing a sheetdef for use.
	    /// </summary>
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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly SheetDefCache _sheetDefCache = new();

    public static List<SheetDef> GetDeepCopy(bool isShort = false)
    {
        return _sheetDefCache.GetDeepCopy(isShort);
    }

    public static List<SheetDef> GetWhere(Predicate<SheetDef> match, bool isShort = false)
    {
        return _sheetDefCache.GetWhere(match, isShort);
    }

    public static SheetDef GetFirstOrDefault(Func<SheetDef, bool> match, bool isShort = false)
    {
        return _sheetDefCache.GetFirstOrDefault(match, isShort);
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
        _sheetDefCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _sheetDefCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _sheetDefCache.ClearCache();
    }

    #endregion
}