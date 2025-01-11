using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Threading;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;
using OpenDentBusiness.UI;

namespace OpenDentBusiness;


public class Sheets
{
    ///<Summary>Gets one Sheet from the database.</Summary>
    public static Sheet GetOne(long sheetNum)
    {
        return SheetCrud.SelectOne(sheetNum);
    }

    /// <summary>
    ///     Gets a single sheet from the database.  Then, gets all the fields and parameters for it.  So it returns a
    ///     fully functional sheet. Returns null if the sheet isn't found in the database.
    /// </summary>
    public static Sheet GetSheet(long sheetNum)
    {
        var sheet = GetOne(sheetNum);
        if (sheet == null) return null; //Sheet was deleted.
        SheetFields.GetFieldsAndParameters(sheet);
        return sheet;
    }

    ///<summary>Gets a list of Sheets from the database. The sheets returned will not have SheetFields.</summary>
    public static List<Sheet> GetSheets(List<long> listSheetNums)
    {
        if (listSheetNums.IsNullOrEmpty()) return new List<Sheet>();

        var command = "SELECT * FROM sheet WHERE SheetNum IN (" + string.Join(",", listSheetNums.Select(x => SOut.Long(x))) + ")";
        return SheetCrud.SelectMany(command);
    }

    /// <summary>
    ///     Returns true if a sheet with the WebFormSheetID passed in already exists in the database. Otherwise; false.
    ///     Multiple entities can be retrieving the same web forms from HQ at the same time so it is important to check the
    ///     sheet table to see if the SheetID has been retrieved before.
    ///     Typically called before invoking SaveNewSheet() to avoid making duplicate sheets.
    /// </summary>
    public static bool HasWebFormSheetID(long webFormSheetID)
    {
        var command = "SELECT COUNT(*) FROM sheet WHERE WebFormSheetID = " + SOut.Long(webFormSheetID);
        if (Db.GetCount(command) == "0") return false;
        var note = "Duplicate SheetID detected. This web form will not be inserted into the sheet table since a row with this SheetID is already present.";
        EServiceLogs.MakeLogEntryWebForms(eServiceAction.WFError, FKey: webFormSheetID, note: note);
        return true;
    }

    /// <Summary>
    ///     This is normally done in FormSheetFillEdit, but if we bypass that window for some reason, we can also save a new
    ///     sheet here. Signature
    ///     fields are inserted as they are, so they must be keyed to the field values already. Saves the sheet and sheetfields
    ///     exactly as they are. Used by
    ///     webforms, for example, when a sheet is retrieved from the web server and the sheet signatures have already been
    ///     keyed to the field values and
    ///     need to be inserted as-is into the user's db. Return the SheetNum in case we need to use it locally when using
    ///     middle tier.
    /// </Summary>
    public static long SaveNewSheet(Sheet sheet)
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

        return sheet.SheetNum;
    }

    /// <summary>
    ///     Gets sheets with PatNum=0 and IsDeleted=0. Sheets with no PatNums were most likely transferred from CEMT tool.
    ///     Also sets the sheet's SheetFields.
    /// </summary>
    public static List<Sheet> GetTransferSheets()
    {
        //Sheets with patnum=0 and the sheet has a sheetfield. 
        var command = "SELECT * FROM sheet "
                      + "INNER JOIN sheetfield ON sheetfield.SheetNum=sheet.SheetNum "
                      + "WHERE PatNum=0 AND IsDeleted=0 "
                      + "AND sheetfield.FieldName='isTransfer' "
                      + $"AND SheetType={SOut.Int((int) SheetTypeEnum.PatientForm)}";
        var listSheets = SheetCrud.SelectMany(command);
        //Get the Sheetfields and parameters for each of the CEMT sheets
        for (var i = 0; i < listSheets.Count; i++) SheetFields.GetFieldsAndParameters(listSheets[i]);
        return listSheets;
    }

    ///<Summary>Saves a list of sheets to the Database. Only saves new sheets, ignores sheets that are not new.</Summary>
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

    ///<summary>Used in FormRefAttachEdit to show all referral slips for the patient/referral combo.  Usually 0 or 1 results.</summary>
    public static List<Sheet> GetReferralSlips(long patNum, long referralNum)
    {
        var command = "SELECT * FROM sheet WHERE PatNum=" + SOut.Long(patNum)
                                                          + " AND sheet.SheetType=" + SOut.Int((int) SheetTypeEnum.ReferralSlip)
                                                          + " AND EXISTS(SELECT * FROM sheetfield "
                                                          + "WHERE sheet.SheetNum=sheetfield.SheetNum "
                                                          + "AND sheetfield.FieldType=" + SOut.Long((int) SheetFieldType.Parameter)
                                                          + " AND sheetfield.FieldName='ReferralNum' "
                                                          + "AND sheetfield.FieldValue='" + SOut.Long(referralNum) + "') "
                                                          + "AND IsDeleted=0 "
                                                          + "ORDER BY DateTimeSheet";
        return SheetCrud.SelectMany(command);
    }

    ///<summary>Used in FormLabCaseEdit to view an existing lab slip.  Will return null if none exist.</summary>
    public static Sheet GetLabSlip(long patNum, long labCaseNum)
    {
        var command = "SELECT sheet.* FROM sheet,sheetfield "
                      + "WHERE sheet.SheetNum=sheetfield.SheetNum"
                      + " AND sheet.PatNum=" + SOut.Long(patNum)
                      + " AND sheet.SheetType=" + SOut.Long((int) SheetTypeEnum.LabSlip)
                      + " AND sheetfield.FieldType=" + SOut.Long((int) SheetFieldType.Parameter)
                      + " AND sheetfield.FieldName='LabCaseNum' "
                      + "AND sheetfield.FieldValue='" + SOut.Long(labCaseNum) + "' "
                      + "AND IsDeleted=0";
        return SheetCrud.SelectOne(command);
    }

    ///<summary>Used in FormRxEdit to view an existing rx.  Will return null if none exist.</summary>
    public static Sheet GetRx(long patNum, long rxNum)
    {
        var command = "SELECT sheet.* FROM sheet,sheetfield "
                      + "WHERE sheet.PatNum=" + SOut.Long(patNum)
                      + " AND sheet.SheetType=" + SOut.Long((int) SheetTypeEnum.Rx)
                      + " AND sheetfield.FieldType=" + SOut.Long((int) SheetFieldType.Parameter)
                      + " AND sheetfield.FieldName='RxNum' "
                      + "AND sheetfield.FieldValue='" + SOut.Long(rxNum) + "' "
                      + "AND IsDeleted=0";
        return SheetCrud.SelectOne(command);
    }

    ///<summary>Gets all sheets for a patient that have the terminal flag set.  Shallow list, no fields or parameters.</summary>
    public static List<Sheet> GetForTerminal(long patNum)
    {
        var command = "SELECT * FROM sheet WHERE PatNum=" + SOut.Long(patNum)
                                                          + " AND ShowInTerminal > 0 AND IsDeleted=0"
                                                          + " ORDER BY ShowInTerminal,DateTimeSheet";
        return SheetCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets the maximum Terminal Num for the selected patient.  Returns 0 if there's no sheets marked to show in
    ///     terminal.
    /// </summary>
    public static int GetMaxTerminalNum(long patNum)
    {
        var command = "SELECT MAX(ShowInTerminal) FROM sheet WHERE PatNum=" + SOut.Long(patNum)
                                                                            + " AND IsDeleted=0";
        return (int) Db.GetLong(command);
    }

    /// <summary>
    ///     Trys to set the out params with sheet fields valus for LName,FName,DOB,PhoneNumbers, and email. Used when
    ///     importing CEMT patient transfers.
    /// </summary>
    public static void ParseTransferSheet(Sheet sheet, out string lName, out string fName, out DateTime dateBirth, out List<string> listPhoneNumbers,
        out string email)
    {
        lName = "";
        fName = "";
        dateBirth = new DateTime();
        listPhoneNumbers = new List<string>();
        email = "";
        var listSheetFields = sheet.SheetFields;
        for (var i = 0; i < listSheetFields.Count; i++)
            switch (listSheetFields[i].FieldName.ToLower())
            {
                case "lname":
                case "lastname":
                    lName = listSheetFields[i].FieldValue;
                    break;
                case "fname":
                case "firstname":
                    fName = listSheetFields[i].FieldValue;
                    break;
                case "bdate":
                case "birthdate":
                    dateBirth = SIn.Date(listSheetFields[i].FieldValue);
                    break;
                case "hmphone":
                case "wkphone":
                case "wirelessphone":
                    if (listSheetFields[i].FieldValue != "") listPhoneNumbers.Add(listSheetFields[i].FieldValue);
                    break;
                case "email":
                    email = listSheetFields[i].FieldValue;
                    break;
            }
    }

    ///<summary>Returns a list of SheetNums of matching sheets.</summary>
    public static List<long> FindSheetsForPat(Sheet sheetToMatch, List<Sheet> listSheets)
    {
        string lName;
        string fName;
        DateTime dateBirth;
        List<string> listPhoneNumbers;
        string email;
        ParseTransferSheet(sheetToMatch, out lName, out fName, out dateBirth, out listPhoneNumbers, out email);
        var listSheetNumsIdMatch = new List<long>();
        for (var i = 0; i < listSheets.Count; i++)
        {
            var lNameSheet = "";
            var fNameSheet = "";
            var dateBirthSheet = new DateTime();
            var listPhoneNumbersSheet = new List<string>();
            var emailSheet = "";
            ParseTransferSheet(listSheets[i], out lNameSheet, out fNameSheet, out dateBirthSheet, out listPhoneNumbersSheet, out emailSheet);
            if (lName == lNameSheet && fName == fNameSheet && dateBirth == dateBirthSheet && email == emailSheet
                //All phone numbers must match in both.
                && listPhoneNumbers.Except(listPhoneNumbersSheet).Count() == 0 && listPhoneNumbersSheet.Except(listPhoneNumbers).Count() == 0)
                listSheetNumsIdMatch.Add(listSheets[i].SheetNum);
        }

        return listSheetNumsIdMatch;
    }

    ///<summary>Get all sheets for a patient for today.</summary>
    public static List<Sheet> GetForPatientForToday(long patNum)
    {
        var dateSQL = "CURDATE()";
        var command = "SELECT * FROM sheet WHERE PatNum=" + SOut.Long(patNum) + " "
                      + "AND " + DbHelper.DtimeToDate("DateTimeSheet") + " = " + dateSQL + " "
                      + "AND IsDeleted=0";
        return SheetCrud.SelectMany(command);
    }

    ///<summary>Get all sheets for a patient.</summary>
    public static List<Sheet> GetForPatient(long patNum)
    {
        var command = "SELECT * FROM sheet WHERE IsDeleted=0 AND PatNum=" + SOut.Long(patNum);
        return SheetCrud.SelectMany(command);
    }

    /// <summary>Get all sheets that reference a given document. Primarily used to prevent deleting an in use document.</summary>
    /// <returns>
    ///     List of sheets that have fields that reference the given DocNum. Returns empty list if document is not
    ///     referenced.
    /// </returns>
    public static List<Sheet> GetForDocument(long docNum)
    {
        var command = "";
        command = "SELECT sheet.* FROM sheetfield "
                  + "LEFT JOIN sheet ON sheet.SheetNum = sheetfield.SheetNum "
                  + "WHERE IsDeleted=0 "
                  + "AND FieldType = 10 " //PatImage
                  + "AND FieldValue = '" + SOut.Long(docNum) + "' " //FieldName == DocCategory, which we do not care about here.
                  + "GROUP BY sheet.SheetNum "
                  + "UNION "
                  + "SELECT sheet.* "
                  + "FROM sheet "
                  + "WHERE sheet.SheetType=" + SOut.Int((int) SheetTypeEnum.ReferralLetter) + " "
                  + "AND sheet.IsDeleted=0 "
                  + "AND sheet.DocNum=" + SOut.Long(docNum);
        return SheetCrud.SelectMany(command);
    }

    ///<summary>Gets the most recent Exam Sheet based on description to fill a patient letter.</summary>
    public static Sheet GetMostRecentExamSheet(long patNum, string examDescript)
    {
        var command = "SELECT * FROM sheet WHERE DateTimeSheet="
                      + "(SELECT MAX(DateTimeSheet) FROM sheet WHERE PatNum=" + SOut.Long(patNum) + " "
                      + "AND Description='" + SOut.String(examDescript) + "' AND IsDeleted=0) "
                      + "AND PatNum=" + SOut.Long(patNum) + " "
                      + "AND Description='" + SOut.String(examDescript) + "' "
                      + "AND IsDeleted=0 "
                      + "LIMIT 1";
        return SheetCrud.SelectOne(command);
    }

    /// <summary>
    ///     Called by eClipboard check-in once an appointment has been moved to the waiting room and the patient is ready to
    ///     fill out forms.
    ///     Returns number of new sheets created and inserted into Sheet table.
    /// </summary>
    public static int CreateSheetsForCheckIn(Appointment appointment)
    {
        if (!MobileAppDevices.IsClinicSignedUpForEClipboard(true ? appointment.ClinicNum : 0)) //this clinic isn't signed up for this feature
            return 0;
        if (!ClinicPrefs.GetBool(PrefName.EClipboardCreateMissingFormsOnCheckIn, appointment.ClinicNum)) //This feature is turned off
            return 0;
        var useDefault = ClinicPrefs.GetBool(PrefName.EClipboardUseDefaults, appointment.ClinicNum);
        var listEClipboardSheetDefsToCreate = EClipboardSheetDefs.GetForClinic(useDefault ? 0 : appointment.ClinicNum);
        //This list can hold sheets and eForms. Lets remove all forms that are not sheets since this method is only for creating sheets.
        listEClipboardSheetDefsToCreate.RemoveAll(x => x.SheetDefNum == 0);
        if (listEClipboardSheetDefsToCreate.Count == 0) //There aren't any sheets to create here
            return 0;
        var listSheetsAlreadyCompleted = GetForPatient(appointment.PatNum);
        var listSheetsAlreadyInTerminal = GetForTerminal(appointment.PatNum);
        //if we already have sheets queued for the patient don't add duplicates
        if (listSheetsAlreadyInTerminal.Count > 0)
        {
            listSheetsAlreadyCompleted.RemoveAll(x => listSheetsAlreadyInTerminal.Select(y => y.SheetNum).Contains(x.SheetNum));
            listEClipboardSheetDefsToCreate.RemoveAll(x => listSheetsAlreadyInTerminal.Select(y => y.SheetDefNum).Contains(x.SheetDefNum));
        }

        var patient = Patients.GetPat(appointment.PatNum);
        //Remove any sheets that the patient shouldn't see based on age. A value of -1 means ignore.
        listEClipboardSheetDefsToCreate.RemoveAll(x => x.MinAge != -1 && patient.Age < x.MinAge);
        listEClipboardSheetDefsToCreate.RemoveAll(x => x.MaxAge != -1 && patient.Age > x.MaxAge);
        listEClipboardSheetDefsToCreate = EClipboardSheetDefs.FilterPrefillStatuses(listEClipboardSheetDefsToCreate, listSheetsAlreadyCompleted, appointment.ClinicNum, listSheetsAlreadyInTerminal)
            .OrderBy(x => x.ItemOrder).ToList();
        var showInTerminal = GetBiggestShowInTerminal(appointment.PatNum);
        var listSheetsNew = new List<Sheet>();
        for (var i = 0; i < listEClipboardSheetDefsToCreate.Count; i++)
        {
            //First check if we've already completed this form against our resubmission interval rules
            var sheetLastCompleted = listSheetsAlreadyCompleted
                .Where(x => x.SheetDefNum == listEClipboardSheetDefsToCreate[i].SheetDefNum)
                .OrderBy(x => x.DateTimeSheet)
                .LastOrDefault() ?? new Sheet();
            if (sheetLastCompleted.DateTimeSheet > DateTime.MinValue)
            {
                //If the patient has submitted this sheetDef before.
                if (listEClipboardSheetDefsToCreate[i].Frequency == EnumEClipFreq.Once && sheetLastCompleted.RevID >= listEClipboardSheetDefsToCreate[i].PrefillStatusOverride) continue; //If this frequency is set to once and they've already completed this form once, we never want to create it automatically again.

                if (listEClipboardSheetDefsToCreate[i].Frequency == EnumEClipFreq.TimeSpan)
                {
                    var daysElapsed = (DateTime.Today - sheetLastCompleted.DateTimeSheet.Date).Days;
                    if (daysElapsed < listEClipboardSheetDefsToCreate[i].ResubmitInterval.Days) continue; //The interval hasn't elapsed yet so we don't want to create this sheet
                }
                //else if(listEClipboardSheetDefsToCreate[i].Frequency==EnumEClipFreq.EachTime), then we do not care about time elapsed and will populate the form each time.
            }

            var sheetDef = SheetDefs.GetSheetDef(listEClipboardSheetDefsToCreate[i].SheetDefNum);
            var sheetNew = new Sheet();
            //Look up the most recent sheet filledout by this patients wth this def num
            var sheet = GetForPatient(appointment.PatNum).Where(x => x.SheetDefNum == sheetDef.SheetDefNum).OrderByDescending(x => x.DateTimeSheet).FirstOrDefault();
            if (listEClipboardSheetDefsToCreate[i].PrefillStatus == PrefillStatuses.PreFill && sheet != null && sheet.RevID == sheetDef.RevID)
            {
                //do the pre-fill thing from  the other method here.
                sheetNew = PreFillSheetFromPreviousAndDatabase(sheetDef, sheet);
                sheetNew.IsNew = true; //Setting this to true because we want to insert this new sheet not update an old one
                sheetNew.DateTimeSheet = DateTime.Now;
            }
            else
            {
                sheetNew = CreateSheetFromSheetDef(sheetDef, appointment.PatNum);
                SheetParameter.SetParameter(sheetNew, "PatNum", appointment.PatNum); //must come before sheet filler
                SheetFiller.FillFields(sheetNew);
            }

            //Counting starts at 1 in this case and we don't want to ovewrite the previous number so increment first
            sheetNew.ShowInTerminal = ++showInTerminal;
            listSheetsNew.Add(sheetNew);
            SecurityLogs.MakeLogEntry(EnumPermType.FormAdded, sheetNew.PatNum, $"{sheetNew.Description} Created in EClipboard");
            EServiceLogs.MakeLogEntry(eServiceAction.ECAddedForm, eServiceType.EClipboard, FKeyType.SheetNum, sheetNew.PatNum, FKey: sheetNew.SheetNum, clinicNum: appointment.ClinicNum);
        }

        SaveNewSheetList(listSheetsNew);
        return listSheetsNew.Count;
    }

    /// <summary>
    ///     Creates a new sheet instance based on sheetDefOriginal, and fills it with values from the db, and then fills
    ///     remaining with values from sheet. Returns the new, pre-filled sheet.
    /// </summary>
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

    
    public static long Insert(Sheet sheet)
    {
        return SheetCrud.Insert(sheet);
    }

    
    public static void Update(Sheet sheet)
    {
        SheetCrud.Update(sheet);
    }

    /// <summary>
    ///     Sets the IsDeleted flag to true (1) for the specified sheetNum.  The sheet and associated sheetfields are not
    ///     deleted.
    /// </summary>
    public static void Delete(long sheetNum, long patNum = 0, byte showInTerminal = 0)
    {
        var command = "UPDATE sheet SET IsDeleted=1,ShowInTerminal=0 WHERE SheetNum=" + SOut.Long(sheetNum);
        Db.NonQ(command);
        if (patNum > 0 && showInTerminal > 0)
        {
            //showInTerminal must be at least 1, so decrementing those that are at least 2
            command = "UPDATE sheet SET ShowInTerminal=ShowInTerminal-1 "
                      + "WHERE PatNum=" + SOut.Long(patNum) + " "
                      + "AND IsDeleted=0 "
                      + "AND ShowInTerminal>" + SOut.Byte(showInTerminal); //decrement ShowInTerminal for all sheets with a bigger ShowInTerminal than the one deleted
            Db.NonQ(command);
            //Create mobile notification for deleted sheet to eClipboard.
            MobileNotifications.CI_RemoveSheet(patNum, sheetNum);
        }
    }

    /// <summary>
    ///     Converts parameters into sheetfield objects, and then saves those objects in the database.
    ///     The parameters will never again enjoy full parameter status, but will just be read-only fields from here on out.
    ///     It ignores PatNum parameters, since those are already part of the sheet itself.
    /// </summary>
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

    /// <summary>
    ///     Loops through all the fields in the sheet and appends together all the FieldValues.  It obviously excludes all
    ///     SigBox fieldtypes.  It does include Drawing fieldtypes, so any change at all to any drawing will invalidate the
    ///     signature.  It does include Image fieldtypes, although that's just a filename and does not really have any
    ///     meaningful data about the image itself.  The order is absolutely critical.
    /// </summary>
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
                      + "AND PatNum =" + SOut.Long(patNum) + " "
                      + "AND (SheetType=" + SOut.Long((int) SheetTypeEnum.PatientForm) + " OR SheetType=" + SOut.Long((int) SheetTypeEnum.MedicalHistory);
        if (PrefC.GetBool(PrefName.PatientFormsShowConsent)) command += " OR SheetType=" + SOut.Long((int) SheetTypeEnum.Consent); //Show consent forms if pref is true.
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
                  + " AND PatNum =" + SOut.Long(patNum)
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
                  + "WHERE PatNum =" + SOut.Long(patNum);
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

    /// <summary>
    ///     Returns all sheets for the given patient in the given date range which have a description matching the
    ///     examDescript in a case insensitive manner. If examDescript is blank, then sheets with any description are returned.
    /// </summary>
    public static List<Sheet> GetExamSheetsTable(long patNum, DateTime dateStart, DateTime dateEnd, long sheetDefNum = -1)
    {
        var command = "SELECT * "
                      + "FROM sheet WHERE IsDeleted=0 "
                      + "AND PatNum=" + SOut.Long(patNum) + " "
                      + "AND SheetType=" + SOut.Int((int) SheetTypeEnum.ExamSheet) + " ";
        if (sheetDefNum != -1) command += "AND SheetDefNum = " + SOut.Long(sheetDefNum) + " ";
        command += "AND " + DbHelper.DtimeToDate("DateTimeSheet") + ">=" + SOut.Date(dateStart) + " AND " + DbHelper.DtimeToDate("DateTimeSheet") + "<=" + SOut.Date(dateEnd) + " "
                   + "ORDER BY DateTimeSheet";
        return SheetCrud.SelectMany(command);
    }

    /// <summary>
    ///     Used to get sheets that were automatically downloaded by the Open Dental Service. These are the sheets that
    ///     have many or no matching patients that still need to be manually attached to an existing or new pat.
    /// </summary>
    public static List<Sheet> GetUnmatchedWebFormSheets(List<long> listClinicNums)
    {
        var command = "SELECT * "
                      + "FROM sheet WHERE IsDeleted=0 "
                      + "AND PatNum=0 "
                      + "AND IsWebForm = " + SOut.Bool(true) + " "
                      + "AND (SheetType=" + SOut.Long((int) SheetTypeEnum.PatientForm) + " OR SheetType=" + SOut.Long((int) SheetTypeEnum.MedicalHistory) + ") "
                      + (true ? "AND ClinicNum IN (" + string.Join(",", listClinicNums) + ") " : "");
        var listSheets = SheetCrud.SelectMany(command);
        //Get the Sheetfields and parameters for each of the auto downloaded sheets
        for (var i = 0; i < listSheets.Count; i++) SheetFields.GetFieldsAndParameters(listSheets[i]);
        return listSheets;
    }

    /// <summary>
    ///     Used to get the count of sheets that are going to be downloaded by the Open Dental Service. Used for creating
    ///     AlertItems.
    /// </summary>
    public static int GetUnmatchedWebFormSheetsCount(List<long> listClinicNums)
    {
        var command = "SELECT COUNT(SheetNum) "
                      + "FROM sheet WHERE IsDeleted=0 "
                      + "AND PatNum=0 "
                      + "AND IsWebForm = " + SOut.Bool(true) + " "
                      + "AND (SheetType=" + SOut.Long((int) SheetTypeEnum.PatientForm) + " OR SheetType=" + SOut.Long((int) SheetTypeEnum.MedicalHistory) + ") "
                      + (true ? "AND ClinicNum IN (" + string.Join(",", listClinicNums) + ") " : "");
        return (int) Db.GetLong(command);
    }

    /// <summary>
    ///     Used to get sheets filled via the web.  Passing in a null or empty list of clinic nums will only return sheets
    ///     that are not assigned to a clinic.
    /// </summary>
    public static DataTable GetWebFormSheetsTable(DateTime dateFrom, DateTime dateTo, List<long> listClinicNums)
    {
        if (listClinicNums == null || listClinicNums.Count == 0) listClinicNums = new List<long> {0}; //To ensure we filter on at least one clinic (HQ).
        var table = new DataTable("");
        DataRow dataRow;
        //columns that start with lowercase are altered for display rather than being raw data.
        table.Columns.Add("date");
        table.Columns.Add("dateOnly", typeof(DateTime)); //to help with sorting
        table.Columns.Add("dateTime", typeof(DateTime));
        table.Columns.Add("description");
        table.Columns.Add("time");
        table.Columns.Add("timeOnly", typeof(TimeSpan)); //to help with sorting
        table.Columns.Add("PatNum");
        table.Columns.Add("SheetNum");
        table.Columns.Add("IsDeleted");
        table.Columns.Add("ClinicNum");
        var listDataRows = new List<DataRow>();
        var command = "SELECT DateTimeSheet,Description,PatNum,SheetNum,IsDeleted,ClinicNum "
                      + "FROM sheet WHERE "
                      + "DateTimeSheet >= " + SOut.Date(dateFrom) + " AND DateTimeSheet <= " + SOut.Date(dateTo.AddDays(1)) + " "
                      + "AND IsWebForm = " + SOut.Bool(true) + " "
                      + "AND (SheetType=" + SOut.Long((int) SheetTypeEnum.PatientForm) + " OR SheetType=" + SOut.Long((int) SheetTypeEnum.MedicalHistory) + ") "
                      + (true ? "AND ClinicNum IN (" + string.Join(",", listClinicNums) + ") " : "");
        var tableRawSheet = DataCore.GetTable(command);
        DateTime dateT;
        for (var i = 0; i < tableRawSheet.Rows.Count; i++)
        {
            dataRow = table.NewRow();
            dateT = SIn.DateTime(tableRawSheet.Rows[i]["DateTimeSheet"].ToString());
            dataRow["date"] = dateT.ToShortDateString();
            dataRow["dateOnly"] = dateT.Date;
            dataRow["dateTime"] = dateT;
            dataRow["description"] = tableRawSheet.Rows[i]["Description"].ToString();
            dataRow["PatNum"] = tableRawSheet.Rows[i]["PatNum"].ToString();
            dataRow["SheetNum"] = tableRawSheet.Rows[i]["SheetNum"].ToString();
            if (dateT.TimeOfDay != TimeSpan.Zero) dataRow["time"] = dateT.ToString("h:mm") + dateT.ToString("%t").ToLower();
            dataRow["timeOnly"] = dateT.TimeOfDay;
            dataRow["IsDeleted"] = tableRawSheet.Rows[i]["IsDeleted"].ToString();
            dataRow["ClinicNum"] = SIn.Long(tableRawSheet.Rows[i]["ClinicNum"].ToString());
            listDataRows.Add(dataRow);
        }

        for (var i = 0; i < listDataRows.Count; i++) table.Rows.Add(listDataRows[i]);
        var dataView = table.DefaultView;
        dataView.Sort = "dateOnly,timeOnly";
        table = dataView.ToTable();
        return table;
    }

    public static bool ContainsStaticField(Sheet sheet, string fieldName)
    {
        var listSheetFields = sheet.SheetFields;
        for (var i = 0; i < listSheetFields.Count; i++)
        {
            if (listSheetFields[i].FieldType != SheetFieldType.StaticText) continue;
            if (listSheetFields[i].FieldValue.Contains("[" + fieldName + "]")) return true;
        }

        return false;
    }

    
    public static byte GetBiggestShowInTerminal(long patNum)
    {
        var command = "SELECT MAX(ShowInTerminal) FROM sheet WHERE IsDeleted=0 AND PatNum=" + SOut.Long(patNum);
        return SIn.Byte(DataCore.GetScalar(command));
    }

    
    public static void ClearFromTerminal(long patNum)
    {
        var command = "UPDATE sheet SET ShowInTerminal=0 WHERE PatNum=" + SOut.Long(patNum);
        Db.NonQ(command);
    }

    /// <summary>
    ///     This gives the number of pages required to print all fields. This must be calculated ahead of time when
    ///     creating multi page pdfs.
    /// </summary>
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
            //case SheetTypeEnum.ReferralSlip:
            case SheetTypeEnum.LabelAppointment:
            case SheetTypeEnum.Rx:
            //case SheetTypeEnum.Consent:
            //case SheetTypeEnum.PatientLetter:
            //case SheetTypeEnum.ReferralLetter:
            //case SheetTypeEnum.PatientForm:
            //case SheetTypeEnum.RoutingSlip:
            //case SheetTypeEnum.MedicalHistory:
            //case SheetTypeEnum.LabSlip:
            //case SheetTypeEnum.ExamSheet:
            case SheetTypeEnum.DepositSlip:
            //case SheetTypeEnum.Statement:
            case SheetTypeEnum.PatientDashboardWidget:
                return true;
        }

        return false;
    }

    #region Xamarin Methods

    ///<summary>This is supposed to be used explicity with Sheets and not the old Open Dental way of creating sheets.</summary>
    public static string CreatePdfForXamarin(long sheetNum)
    {
        var sheet = GetSheet(sheetNum);
        SheetFields.GetFieldsAndParameters(sheet);
        var sheetDrawingJob = new SheetDrawingJob();
        var tempFile = PrefC.GetRandomTempFile(".pdf");
        var rawBase64 = "";
        //Create a PDF with the given sheet and file. The other parameters can remain null, because they aren't used for TreatPlan sheets.
        var pdfDocument = sheetDrawingJob.CreatePdf(sheet);
        SheetDrawingJob.SavePdfToFile(pdfDocument, tempFile);
        //Convert the pdf into its raw bytes
        rawBase64 = Convert.ToBase64String(File.ReadAllBytes(tempFile));
        return Convert.ToBase64String(File.ReadAllBytes(tempFile));
    }

    public static Sheet CreateExamSheet(long patNum, long sheetDefNum)
    {
        SheetDef sheetDef = null;
        if (sheetDefNum == 0)
            sheetDef = SheetDefs.GetSheetsDefault(SheetTypeEnum.ExamSheet);
        else
            sheetDef = SheetDefs.GetSheetDef(sheetDefNum);
        var sheet = SheetUtil.CreateSheet(sheetDef, patNum);
        SheetParameter.SetParameter(sheet, "PatNum", patNum);
        SheetFiller.FillFields(sheet);
        SheetUtil.CalculateHeights(sheet);
        return sheet;
    }

    #endregion Xamarin Methods
}