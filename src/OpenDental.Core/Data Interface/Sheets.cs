using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Printing;
using System.Linq;
using System.Threading;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using OpenDentBusiness.UI;

namespace OpenDentBusiness;

public class Sheets
{
    public static Sheet GetOne(long sheetNum)
    {
        return SheetCrud.SelectOne(sheetNum);
    }

    public static Sheet GetSheet(long sheetNum)
    {
        var sheet = GetOne(sheetNum);
        if (sheet == null) return null; //Sheet was deleted.
        SheetFields.GetFieldsAndParameters(sheet);
        return sheet;
    }

    public static List<Sheet> GetSheets(List<long> listSheetNums)
    {
        if (listSheetNums.IsNullOrEmpty()) return [];

        var command = "SELECT * FROM sheet WHERE SheetNum IN (" + string.Join(",", listSheetNums.Select(x => x)) + ")";
        return SheetCrud.SelectMany(command);
    }

    public static void SaveNewSheet(Sheet sheet)
    {
        //This remoting role check is technically unnecessary but it significantly speeds up the retrieval process for Middle Tier users due to looping.

        if (!sheet.IsNew) throw new ApplicationException("Only new sheets allowed");
        Insert(sheet);
        //insert 'blank' sheetfields to get sheetfieldnums assigned, then use ordered sheetfieldnums with actual field data to update 'blank' db fields
        var listSheetFieldsBlank = sheet.SheetFields.Select(x => new SheetField {SheetNum = sheet.SheetNum}).ToList();
        SheetFields.InsertMany(listSheetFieldsBlank);
        var listSheetFieldsDb = SheetFields.GetListForSheet(sheet.SheetNum);
        //The count can be off for offices that read and write to separate servers when we get objects that were just inserted so we try twice.
        if (listSheetFieldsDb.Count != sheet.SheetFields.Count)
        {
            Thread.Sleep(100);
            listSheetFieldsDb = SheetFields.GetListForSheet(sheet.SheetNum);
        }

        if (listSheetFieldsDb.Count != sheet.SheetFields.Count)
        {
            Delete(sheet.SheetNum); //any blank inserted sheetfields will be linked to the sheet marked deleted
            throw new ApplicationException("Incorrect sheetfield count.");
        }

        var listSheetFieldNums = listSheetFieldsDb.Select(x => x.SheetFieldNum).OrderBy(x => x).ToList();
        //now that we have an ordered list of sheetfieldnums, update db blank fields with all field data from field in memory
        for (var i = 0; i < sheet.SheetFields.Count; i++)
        {
            var sheetField = sheet.SheetFields[i];
            sheetField.SheetFieldNum = listSheetFieldNums[i];
            sheetField.SheetNum = sheet.SheetNum;
            SheetFields.Update(sheetField);
        }
    }

    public static void SaveNewSheetList(List<Sheet> listSheets)
    {
        for (var i = 0; i < listSheets.Count; i++)
        {
            if (!listSheets[i].IsNew) continue;
            SheetCrud.Insert(listSheets[i]);
            var listSheetFields = listSheets[i].SheetFields;
            for (var j = 0; j < listSheetFields.Count; j++) listSheetFields[j].SheetNum = listSheets[i].SheetNum;
            SheetFieldCrud.InsertMany(listSheetFields);
        }
    }

    public static List<Sheet> GetReferralSlips(long patNum, long referralNum)
    {
        var command = "SELECT * FROM sheet WHERE PatNum=" + patNum
                                                          + " AND sheet.SheetType=" + SOut.Int((int) SheetTypeEnum.ReferralSlip)
                                                          + " AND EXISTS(SELECT * FROM sheetfield "
                                                          + "WHERE sheet.SheetNum=sheetfield.SheetNum "
                                                          + "AND sheetfield.FieldType=" + (int) SheetFieldType.Parameter
                                                          + " AND sheetfield.FieldName='ReferralNum' "
                                                          + "AND sheetfield.FieldValue='" + referralNum + "') "
                                                          + "AND IsDeleted=0 "
                                                          + "ORDER BY DateTimeSheet";
        return SheetCrud.SelectMany(command);
    }

    public static Sheet GetLabSlip(long patNum, long labCaseNum)
    {
        var command = "SELECT sheet.* FROM sheet,sheetfield "
                      + "WHERE sheet.SheetNum=sheetfield.SheetNum"
                      + " AND sheet.PatNum=" + patNum
                      + " AND sheet.SheetType=" + (int) SheetTypeEnum.LabSlip
                      + " AND sheetfield.FieldType=" + (int) SheetFieldType.Parameter
                      + " AND sheetfield.FieldName='LabCaseNum' "
                      + "AND sheetfield.FieldValue='" + labCaseNum + "' "
                      + "AND IsDeleted=0";
        return SheetCrud.SelectOne(command);
    }

    public static List<Sheet> GetForTerminal(long patNum)
    {
        var command = "SELECT * FROM sheet WHERE PatNum=" + patNum
                                                          + " AND ShowInTerminal > 0 AND IsDeleted=0"
                                                          + " ORDER BY ShowInTerminal,DateTimeSheet";
        return SheetCrud.SelectMany(command);
    }

    public static int GetMaxTerminalNum(long patNum)
    {
        var command = "SELECT MAX(ShowInTerminal) FROM sheet WHERE PatNum=" + patNum
                                                                            + " AND IsDeleted=0";
        return (int) Db.GetLong(command);
    }

    public static List<Sheet> GetForPatientForToday(long patNum)
    {
        var dateSQL = "CURDATE()";
        var command = "SELECT * FROM sheet WHERE PatNum=" + patNum + " "
                      + "AND DATE(DateTimeSheet) = " + dateSQL + " "
                      + "AND IsDeleted=0";
        return SheetCrud.SelectMany(command);
    }

    public static List<Sheet> GetForPatient(long patNum)
    {
        var command = "SELECT * FROM sheet WHERE IsDeleted=0 AND PatNum=" + patNum;
        return SheetCrud.SelectMany(command);
    }

    public static List<Sheet> GetForDocument(long docNum)
    {
        var command = "";
        command = "SELECT sheet.* FROM sheetfield "
                  + "LEFT JOIN sheet ON sheet.SheetNum = sheetfield.SheetNum "
                  + "WHERE IsDeleted=0 "
                  + "AND FieldType = 10 " //PatImage
                  + "AND FieldValue = '" + docNum + "' " //FieldName == DocCategory, which we do not care about here.
                  + "GROUP BY sheet.SheetNum "
                  + "UNION "
                  + "SELECT sheet.* "
                  + "FROM sheet "
                  + "WHERE sheet.SheetType=" + SOut.Int((int) SheetTypeEnum.ReferralLetter) + " "
                  + "AND sheet.IsDeleted=0 "
                  + "AND sheet.DocNum=" + docNum;
        return SheetCrud.SelectMany(command);
    }

    public static Sheet GetMostRecentExamSheet(long patNum, string examDescript)
    {
        var command = "SELECT * FROM sheet WHERE DateTimeSheet="
                      + "(SELECT MAX(DateTimeSheet) FROM sheet WHERE PatNum=" + patNum + " "
                      + "AND Description='" + SOut.String(examDescript) + "' AND IsDeleted=0) "
                      + "AND PatNum=" + patNum + " "
                      + "AND Description='" + SOut.String(examDescript) + "' "
                      + "AND IsDeleted=0 "
                      + "LIMIT 1";
        return SheetCrud.SelectOne(command);
    }

    public static Sheet PreFillSheetFromPreviousAndDatabase(SheetDef sheetDefOriginal, Sheet sheet)
    {
        var sheetNew = SheetUtil.CreateSheet(sheetDefOriginal, sheet.PatNum);
        sheetNew.DateTimeSheet = DateTime.Now;
        sheetNew.PatNum = sheet.PatNum;
        //Only setting the PatNum sheet parameter was what the Add button was doing from the "Patient Forms and Medical Histories" window.
        SheetParameter.SetParameter(sheetNew, "PatNum", sheet.PatNum);
        //Fill the fields with the most recent values from the non-sheet related tables in database.
        SheetFiller.FillFields(sheetNew);
        if (sheet.SheetFields.IsNullOrEmpty()) SheetFields.GetFieldsAndParameters(sheet);
        //If there are current medications in the DB, display them on the prefilled sheet. If there are not, use the previous sheet.
        var doUseMedsFromPrevSheet = !MedicationPats.GetPatientData(sheet.PatNum).Any(x => MedicationPats.IsMedActive(x));
        var doUseProblemFromPrevSheet = !Diseases.Refresh(sheet.PatNum, false).Any();
        var doUseAllergyFromPrevSheet = !Allergies.GetAll(sheet.PatNum, true).Any();
        //Get the fields that we want to fill from previous sheet.
        //Always skip insurance fields, skip medications if they have active medications in the DB.
        //Always exclude static text. Allow combo or check boxes if the fieldName is misc
        var listSheetFieldsNewEmpty = sheetNew.SheetFields.FindAll(x => x.FieldType != SheetFieldType.StaticText
                                                                        && (!x.FieldType.In(SheetFieldType.CheckBox) || x.FieldName == "misc"
                                                                                                                     || (x.FieldName.StartsWith("problem") && doUseProblemFromPrevSheet)
                                                                                                                     || (x.FieldName.StartsWith("allergy") && doUseAllergyFromPrevSheet))
                                                                        && (x.FieldValue.IsNullOrEmpty() || x.FieldType.In(SheetFieldType.ComboBox))
                                                                        && !x.FieldName.StartsWith("ins1")
                                                                        && !x.FieldName.StartsWith("ins2")
                                                                        && (!x.FieldName.StartsWith("inputMed") || doUseMedsFromPrevSheet)
        );
        //Find the fields that were passed in that can be used with pre-fill logic.
        var listSheetFieldsOrig = sheet.SheetFields.FindAll(x => x.SheetFieldDefNum > 0 && !x.FieldValue.IsNullOrEmpty());
        //Loop through fields on the new sheet, find their matching fields on the passed in sheet by sheetFieldDefNum.
        for (var i = 0; i < listSheetFieldsNewEmpty.Count; i++)
        for (var j = 0; j < listSheetFieldsOrig.Count; j++)
        {
            if (listSheetFieldsNewEmpty[i].SheetFieldDefNum != listSheetFieldsOrig[j].SheetFieldDefNum) continue;
            listSheetFieldsNewEmpty[i].FieldValue = listSheetFieldsOrig[j].FieldValue;
        }

        //Clear signiture boxes.
        for (var i = 0; i < sheetNew.SheetFields.Count; i++)
            if (sheetNew.SheetFields[i].FieldType == SheetFieldType.SigBox || sheetNew.SheetFields[i].FieldType == SheetFieldType.SigBoxPractice)
                sheetNew.SheetFields[i].FieldValue = "";

        return sheetNew;
    }

    public static Sheet CreateSheetFromSheetDef(SheetDef sheetDef, long patNum = 0, bool hidePaymentOptions = false)
    {
        bool FieldIsPaymentOptionHelper(SheetFieldDef sheetFieldDef)
        {
            if (sheetFieldDef.IsPaymentOption) return true;
            switch (sheetFieldDef.FieldName)
            {
                case "StatementEnclosed":
                case "StatementAging":
                    return true;
            }

            return false;
        }

        List<SheetField> CreateFieldList(List<SheetFieldDef> listSheetFieldDefs, string language)
        {
            var listSheetFields = new List<SheetField>();
            //SheetDefs that are not setup with the desired language translation SheetFieldDefs should default to the non-translated SheetFieldDefs.
            var hasTranslationForLanguage = listSheetFieldDefs.Any(x => x.Language == language);
            for (var i = 0; i < listSheetFieldDefs.Count; i++)
            {
                //Only use the SheetFieldDefs for the specified language if available.
                if (hasTranslationForLanguage)
                {
                    if (listSheetFieldDefs[i].Language != language) continue;
                }
                //Otherwise, only use the SheetFieldDefs for the default language.
                else if (!string.IsNullOrWhiteSpace(listSheetFieldDefs[i].Language))
                {
                    continue;
                }

                if (hidePaymentOptions && FieldIsPaymentOptionHelper(listSheetFieldDefs[i])) continue;
                var sheetField = new SheetField();
                sheetField.IsNew = true;
                sheetField.FieldName = listSheetFieldDefs[i].FieldName;
                sheetField.FieldType = listSheetFieldDefs[i].FieldType;
                sheetField.FieldValue = listSheetFieldDefs[i].FieldValue;
                sheetField.FontIsBold = listSheetFieldDefs[i].FontIsBold;
                sheetField.FontName = listSheetFieldDefs[i].FontName;
                sheetField.FontSize = listSheetFieldDefs[i].FontSize;
                sheetField.GrowthBehavior = listSheetFieldDefs[i].GrowthBehavior;
                sheetField.Height = listSheetFieldDefs[i].Height;
                sheetField.RadioButtonValue = listSheetFieldDefs[i].RadioButtonValue;
                //field.SheetNum=sheetFieldDef.SheetNum;//set later
                sheetField.Width = listSheetFieldDefs[i].Width;
                sheetField.XPos = listSheetFieldDefs[i].XPos;
                sheetField.YPos = listSheetFieldDefs[i].YPos;
                sheetField.RadioButtonGroup = listSheetFieldDefs[i].RadioButtonGroup;
                sheetField.IsRequired = listSheetFieldDefs[i].IsRequired;
                sheetField.TabOrder = listSheetFieldDefs[i].TabOrder;
                sheetField.ReportableName = listSheetFieldDefs[i].ReportableName;
                sheetField.SheetFieldDefNum = listSheetFieldDefs[i].SheetFieldDefNum;
                sheetField.TextAlign = listSheetFieldDefs[i].TextAlign;
                sheetField.ItemColor = listSheetFieldDefs[i].ItemColor;
                sheetField.IsLocked = listSheetFieldDefs[i].IsLocked;
                sheetField.TabOrderMobile = listSheetFieldDefs[i].TabOrderMobile;
                sheetField.UiLabelMobile = listSheetFieldDefs[i].UiLabelMobile;
                sheetField.UiLabelMobileRadioButton = listSheetFieldDefs[i].UiLabelMobileRadioButton;
                sheetField.CanElectronicallySign = listSheetFieldDefs[i].CanElectronicallySign;
                sheetField.IsSigProvRestricted = listSheetFieldDefs[i].IsSigProvRestricted;
                listSheetFields.Add(sheetField);
            }

            return listSheetFields;
        }

        var language = patNum == 0 ? "" : Patients.GetPat(patNum).Language; //Blank string will use 'Default' translation.
        var sheet = new Sheet();
        sheet.IsNew = true;
        sheet.DateTimeSheet = DateTime.Now;
        sheet.FontName = sheetDef.FontName;
        sheet.FontSize = sheetDef.FontSize;
        sheet.Height = sheetDef.Height;
        sheet.SheetType = sheetDef.SheetType;
        sheet.Width = sheetDef.Width;
        sheet.PatNum = patNum;
        sheet.Description = sheetDef.Description;
        sheet.IsLandscape = sheetDef.IsLandscape;
        sheet.IsMultiPage = sheetDef.IsMultiPage;
        sheet.SheetFields = CreateFieldList(sheetDef.SheetFieldDefs, language); //Blank fields with no values. Values filled later from SheetFiller.FillFields()
        sheet.Parameters = sheetDef.Parameters;
        sheet.SheetDefNum = sheetDef.SheetDefNum;
        sheet.HasMobileLayout = sheetDef.HasMobileLayout;
        sheet.RevID = sheetDef.RevID;
        return sheet;
    }

    public static void Insert(Sheet sheet)
    {
        SheetCrud.Insert(sheet);
    }

    public static void Update(Sheet sheet)
    {
        SheetCrud.Update(sheet);
    }

    public static void Delete(long sheetNum, long patNum = 0, byte showInTerminal = 0)
    {
        var command = "UPDATE sheet SET IsDeleted=1,ShowInTerminal=0 WHERE SheetNum=" + sheetNum;
        Db.NonQ(command);
        if (patNum > 0 && showInTerminal > 0)
        {
            //showInTerminal must be at least 1, so decrementing those that are at least 2
            command = "UPDATE sheet SET ShowInTerminal=ShowInTerminal-1 "
                      + "WHERE PatNum=" + patNum + " "
                      + "AND IsDeleted=0 "
                      + "AND ShowInTerminal>" + SOut.Byte(showInTerminal); //decrement ShowInTerminal for all sheets with a bigger ShowInTerminal than the one deleted
            Db.NonQ(command);
        }
    }

    public static void SaveParameters(Sheet sheet)
    {
        var listSheetFields = new List<SheetField>();
        for (var i = 0; i < sheet.Parameters.Count; i++)
        {
            if (sheet.Parameters[i].ParamName.In("PatNum",
                    //These types are not primitives so they cannot be saved to the database.
                    "CompletedProcs", "toothChartImg"))
                continue;
            if (!sheet.Parameters[i].IsRequired && sheet.Parameters[i].ParamValue == null) continue;
            var sheetField = new SheetField();
            sheetField.IsNew = true;
            sheetField.SheetNum = sheet.SheetNum;
            sheetField.FieldType = SheetFieldType.Parameter;
            sheetField.FieldName = sheet.Parameters[i].ParamName;
            if (sheet.Parameters[i].ParamName == "ListProcNums")
            {
                //Save this parameter as a comma delimited list
                var listProcNums = (List<long>) SheetParameter.GetParamByName(sheet.Parameters, "ListProcNums").ParamValue;
                sheetField.FieldValue = string.Join(",", listProcNums);
            }
            else
            {
                sheetField.FieldValue = sheet.Parameters[i].ParamValue.ToString(); //the object will be an int. Stored as a string.
            }

            sheetField.FontSize = 0;
            sheetField.FontName = "";
            sheetField.FontIsBold = false;
            sheetField.XPos = 0;
            sheetField.YPos = 0;
            sheetField.Width = 0;
            sheetField.Height = 0;
            sheetField.GrowthBehavior = GrowthBehaviorEnum.None;
            sheetField.RadioButtonValue = "";
            listSheetFields.Add(sheetField);
        }

        SheetFields.InsertMany(listSheetFields);
    }

    public static string GetSignatureKey(Sheet sheet)
    {
        //The order of sheet fields is absolutely critical when it comes to the signature key.
        //Therefore, we will make a local copy of the sheet fields and sort them how we want them here just in case their order has changed for any other reason.
        var listSheetFieldsCopy = new List<SheetField>();
        for (var i = 0; i < sheet.SheetFields.Count; i++) listSheetFieldsCopy.Add(sheet.SheetFields[i]);
        if (listSheetFieldsCopy.All(x => x.SheetFieldNum > 0)) //the sheet has not been loaded into the db, so it has no primary keys to sort on
            listSheetFieldsCopy.Sort(SheetFields.SortPrimaryKey);
        return SigBox.GetSignatureKeySheets(listSheetFieldsCopy);
    }

    public static DataTable GetPatientFormsTable(long patNum)
    {
        //DataConnection dcon=new DataConnection();
        var table = new DataTable("");
        DataRow dataRow;
        //columns that start with lowercase are altered for display rather than being raw data.
        table.Columns.Add("date");
        table.Columns.Add("dateOnly", typeof(DateTime)); //to help with sorting
        table.Columns.Add("dateTime", typeof(DateTime));
        table.Columns.Add("DateTSheetEdited", typeof(DateTime));
        table.Columns.Add("description");
        table.Columns.Add("DocNum");
        table.Columns.Add("EFormNum");
        table.Columns.Add("imageCat");
        table.Columns.Add("SheetNum");
        table.Columns.Add("showInTerminal");
        table.Columns.Add("time");
        table.Columns.Add("timeOnly", typeof(TimeSpan)); //to help with sorting
        //but we won't actually fill this table with rows until the very end.  It's more useful to use a List<> for now.
        var listDataRows = new List<DataRow>();
        //sheet---------------------------------------------------------------------------------------
        var command = "SELECT DateTimeSheet,SheetNum,Description,ShowInTerminal,DateTSheetEdited "
                      + "FROM sheet WHERE IsDeleted=0 "
                      + "AND PatNum =" + patNum + " "
                      + "AND (SheetType=" + (int) SheetTypeEnum.PatientForm + " OR SheetType=" + (int) SheetTypeEnum.MedicalHistory;
        if (PrefC.GetBool(PrefName.PatientFormsShowConsent)) command += " OR SheetType=" + (int) SheetTypeEnum.Consent; //Show consent forms if pref is true.
        command += ")";
        //+"ORDER BY ShowInTerminal";//DATE(DateTimeSheet),ShowInTerminal,TIME(DateTimeSheet)";
        var tableRawSheet = DataCore.GetTable(command);
        DateTime dateT;
        for (var i = 0; i < tableRawSheet.Rows.Count; i++)
        {
            dataRow = table.NewRow();
            dateT = SIn.DateTime(tableRawSheet.Rows[i]["DateTimeSheet"].ToString());
            dataRow["date"] = dateT.ToShortDateString();
            dataRow["dateOnly"] = dateT.Date;
            dataRow["dateTime"] = dateT;
            dataRow["DateTSheetEdited"] = SIn.DateTime(tableRawSheet.Rows[i]["DateTSheetEdited"].ToString());
            dataRow["description"] = tableRawSheet.Rows[i]["Description"].ToString();
            dataRow["DocNum"] = "0";
            dataRow["EFormNum"] = "0";
            dataRow["imageCat"] = "";
            dataRow["SheetNum"] = tableRawSheet.Rows[i]["SheetNum"].ToString();
            if (tableRawSheet.Rows[i]["ShowInTerminal"].ToString() == "0")
                dataRow["showInTerminal"] = "";
            else
                dataRow["showInTerminal"] = tableRawSheet.Rows[i]["ShowInTerminal"].ToString();
            if (dateT.TimeOfDay != TimeSpan.Zero) dataRow["time"] = dateT.ToString("h:mm") + dateT.ToString("%t").ToLower();
            dataRow["timeOnly"] = dateT.TimeOfDay;
            listDataRows.Add(dataRow);
        }

        //document---------------------------------------------------------------------------------------
        command = "SELECT DateCreated,DocCategory,DocNum,Description,document.DateTStamp "
                  + "FROM document,definition "
                  + "WHERE document.DocCategory=definition.DefNum"
                  + " AND PatNum =" + patNum
                  + " AND definition.ItemValue LIKE '%F%'";
        //+" ORDER BY DateCreated";
        var tableRawDoc = DataCore.GetTable(command);
        long docCat;
        for (var i = 0; i < tableRawDoc.Rows.Count; i++)
        {
            dataRow = table.NewRow();
            dateT = SIn.DateTime(tableRawDoc.Rows[i]["DateCreated"].ToString());
            dataRow["date"] = dateT.ToShortDateString();
            dataRow["dateOnly"] = dateT.Date;
            dataRow["dateTime"] = dateT;
            dataRow["DateTSheetEdited"] = SIn.DateTime(tableRawDoc.Rows[i]["DateTStamp"].ToString());
            dataRow["description"] = tableRawDoc.Rows[i]["Description"].ToString();
            dataRow["DocNum"] = tableRawDoc.Rows[i]["DocNum"].ToString();
            dataRow["EFormNum"] = "0";
            docCat = SIn.Long(tableRawDoc.Rows[i]["DocCategory"].ToString());
            dataRow["imageCat"] = Defs.GetName(DefCat.ImageCats, docCat);
            dataRow["SheetNum"] = "0";
            dataRow["showInTerminal"] = "";
            if (dateT.TimeOfDay != TimeSpan.Zero) dataRow["time"] = dateT.ToString("h:mm") + dateT.ToString("%t").ToLower();
            dataRow["timeOnly"] = dateT.TimeOfDay;
            listDataRows.Add(dataRow);
        }

        //eForms---------------------------------------------------------------------------------------
        command = "SELECT EFormNum,DateTimeShown,Description,DateTEdited "
                  + "FROM eform "
                  + "WHERE PatNum =" + patNum;
        var tableRawEForm = DataCore.GetTable(command);
        for (var i = 0; i < tableRawEForm.Rows.Count; i++)
        {
            dataRow = table.NewRow();
            dateT = SIn.DateTime(tableRawEForm.Rows[i]["DateTimeShown"].ToString());
            dataRow["date"] = dateT.ToShortDateString();
            dataRow["dateOnly"] = dateT.Date;
            dataRow["dateTime"] = dateT;
            dataRow["DateTSheetEdited"] = SIn.DateTime(tableRawEForm.Rows[i]["DateTEdited"].ToString());
            dataRow["description"] = tableRawEForm.Rows[i]["Description"].ToString();
            dataRow["DocNum"] = "0";
            dataRow["EFormNum"] = tableRawEForm.Rows[i]["EFormNum"].ToString();
            dataRow["imageCat"] = "";
            dataRow["SheetNum"] = "0";
            dataRow["showInTerminal"] = "";
            if (dateT.TimeOfDay != TimeSpan.Zero) dataRow["time"] = dateT.ToString("h:mm") + dateT.ToString("%t").ToLower();
            dataRow["timeOnly"] = dateT.TimeOfDay;
            listDataRows.Add(dataRow);
        }

        //Sorting
        for (var i = 0; i < listDataRows.Count; i++) table.Rows.Add(listDataRows[i]);
        var dataView = table.DefaultView;
        dataView.Sort = "dateOnly,showInTerminal,timeOnly";
        table = dataView.ToTable();
        return table;
    }

    public static List<Sheet> GetExamSheetsTable(long patNum, DateTime dateStart, DateTime dateEnd, long sheetDefNum = -1)
    {
        var command = "SELECT * "
                      + "FROM sheet WHERE IsDeleted=0 "
                      + "AND PatNum=" + patNum + " "
                      + "AND SheetType=" + SOut.Int((int) SheetTypeEnum.ExamSheet) + " ";
        if (sheetDefNum != -1) command += "AND SheetDefNum = " + sheetDefNum + " ";
        command += "AND DATE(DateTimeSheet)>=" + SOut.Date(dateStart) + " AND DATE(DateTimeSheet)<=" + SOut.Date(dateEnd) + " "
                   + "ORDER BY DateTimeSheet";
        return SheetCrud.SelectMany(command);
    }

    public static byte GetBiggestShowInTerminal(long patNum)
    {
        var command = "SELECT MAX(ShowInTerminal) FROM sheet WHERE IsDeleted=0 AND PatNum=" + patNum;
        return SIn.Byte(DataCore.GetScalar(command));
    }

    public static void ClearFromTerminal(long patNum)
    {
        var command = "UPDATE sheet SET ShowInTerminal=0 WHERE PatNum=" + patNum;
        Db.NonQ(command);
    }

    public static int CalculatePageCount(Sheet sheet, Margins margins)
    {
        //HeightPage is the value of Width/Length depending on Landscape/Portrait.
        var bottomLastField = 0;
        if (sheet.SheetFields.Count > 0) bottomLastField = sheet.SheetFields.Max(x => x.Bounds.Bottom);
        if (bottomLastField <= sheet.HeightPage && sheet.SheetType != SheetTypeEnum.MedLabResults) //MedLabResults always implements footer, needs true multi-page count
            return 1; //if all of the fields are less than one page, even if some of the fields fall within the margin of the first page.
        if (SheetTypeIsSinglePage(sheet.SheetType)) return 1; //labels and RX forms are always single pages
        SetPageMargin(sheet, margins);
        var printableHeightPerPage = sheet.HeightPage - (margins.Top + margins.Bottom);
        if (printableHeightPerPage < 1) return 1; //otherwise we get negative, infinite, or thousands of pages.
        var maxY = 0;
        for (var i = 0; i < sheet.SheetFields.Count; i++) maxY = Math.Max(maxY, sheet.SheetFields[i].Bounds.Bottom);
        var pageCount = 1;
        maxY -= margins.Top; //adjust for ignoring the top margin of the first page.
        pageCount = Convert.ToInt32(Math.Ceiling((double) maxY / printableHeightPerPage));
        pageCount = Math.Max(pageCount, 1); //minimum of at least one page.
        return pageCount;
    }

    public static void SetPageMargin(Sheet sheet, Margins margins)
    {
        margins.Left = 0;
        margins.Right = 0;
        if (SheetTypeIsSinglePage(sheet.SheetType))
        {
            margins.Top = 0;
            margins.Bottom = 0;
            //m=new System.Drawing.Printing.Margins(0,0,0,0); //does not work, creates new reference.
            return;
        }

        margins.Top = 40;
        if (sheet.SheetType == SheetTypeEnum.MedLabResults) margins.Top = 120;
        margins.Bottom = 60;
    }

    public static void SetSheetFieldsForSheets(List<Sheet> listSheets)
    {
        var listSheetNums = listSheets.Select(x => x.SheetNum).ToList();
        var listSheetFields = SheetFields.GetListForSheets(listSheetNums);
        for (var i = 0; i < listSheets.Count; i++)
        {
            var listSheetFieldsForSheet = listSheetFields.FindAll(x => x.SheetNum == listSheets[i].SheetNum);
            listSheetFieldsForSheet.Sort(SheetFields.SortDrawingOrderLayers);
            SheetFields.GetFieldsAndParameters(listSheets[i], listSheetFieldsForSheet);
            var listSheetFieldsSigBox = listSheets[i].SheetFields
                .FindAll(x => x.FieldType.In(SheetFieldType.SigBox, SheetFieldType.SigBoxPractice));
            for (var j = 0; j < listSheetFieldsSigBox.Count; j++) listSheetFieldsSigBox[j].SigKey = GetSignatureKey(listSheets[i]);
        }
    }

    public static bool SheetTypeIsSinglePage(SheetTypeEnum sheetType)
    {
        switch (sheetType)
        {
            case SheetTypeEnum.LabelPatient:
            case SheetTypeEnum.LabelCarrier:
            case SheetTypeEnum.LabelReferral:
            case SheetTypeEnum.LabelAppointment:
            case SheetTypeEnum.DepositSlip:
            case SheetTypeEnum.PatientDashboardWidget:
                return true;
        }

        return false;
    }
}