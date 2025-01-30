using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using OpenDentBusiness;
using OpenDentBusiness.HL7;
using OpenDentBusiness.Properties;
using OpenDentBusiness.WebTypes.WebForms;

namespace OpenDental;

public class WebFormL
{
    public static string SynchUrlStaging = "https://10.10.1.196/WebHostSynch/SheetsSynch.asmx";
    public static string SynchUrlDev = "http://localhost:2923/SheetsSynch.asmx";

    public static void IgnoreCertificateErrors()
    {
        ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
    }

    public static void LoadImagesToSheetDef(SheetDef sheetDef)
    {
        foreach (var sheetFieldDef in sheetDef.SheetFieldDefs)
        {
            if (sheetFieldDef.FieldType != SheetFieldType.Image)
            {
                sheetFieldDef.ImageData = "";
                continue;
            }

            string filePath;
            try
            {
                filePath = SheetUtil.GetImagePath();
            }
            catch (Exception ex)
            {
                sheetFieldDef.ImageData = "";
                ODMessageBox.Show(ex.Message);
                return;
            }

            var fileName = sheetFieldDef.FieldName;
            var filePathAndName = ODFileUtils.CombinePaths(filePath, fileName);

            Image image = null;
            ImageFormat imageFormat = null;

            if (sheetFieldDef.ImageField != null)
            {
                image = new Bitmap(sheetFieldDef.ImageField);
                imageFormat = ImageFormat.Bmp;
            }
            else if (sheetFieldDef.FieldName == "Patient Info.gif")
            {
                image = Resources.Patient_Info;
                imageFormat = image.RawFormat;
            }
            else if (File.Exists(filePathAndName))
            {
                try
                {
                    image = Image.FromFile(filePathAndName);
                }
                catch (Exception ex)
                {
                    sheetFieldDef.ImageData = "";
                    ODMessageBox.Show(ex.Message);
                    return;
                }

                imageFormat = image.RawFormat;
            }

            if (image == null)
            {
                sheetFieldDef.ImageData = "";
                ODMessageBox.Show($"The file {fileName} could not be found in {filePath}");
                return;
            }

            using var memoryStreamFileSize = new MemoryStream();

            image.Save(memoryStreamFileSize, imageFormat);

            var lengthFileBytes = memoryStreamFileSize.Length;

            sheetFieldDef.ImageData = SOut.Bitmap((Bitmap) image, lengthFileBytes > 2000000 ? ImageFormat.Jpeg : imageFormat);

            image.Dispose();
        }
    }

    public static bool VerifyRequiredFieldsPresent(SheetDef sheetDef)
    {
        var hasFName = false;
        var hasLName = false;
        var hasBirthdate = false;

        foreach (var sheetFieldDef in sheetDef.SheetFieldDefs)
        {
            if (sheetFieldDef.FieldType != SheetFieldType.InputField)
            {
                continue;
            }

            if (sheetFieldDef.FieldName.ToLower().In("fname", "firstname"))
            {
                hasFName = true;
            }
            else if (sheetFieldDef.FieldName.ToLower().In("lname", "lastname"))
            {
                hasLName = true;
            }
            else if (sheetFieldDef.FieldName.ToLower().In("bdate", "birthdate"))
            {
                hasBirthdate = true;
            }
        }

        if (hasFName && hasLName && hasBirthdate)
        {
            return true;
        }

        ODMessageBox.Show("The sheet called \"" + sheetDef.Description + "\" does not contain all three required fields: LName, FName, and Birthdate.");
        return false;
    }

    public static bool TryAddOrUpdateSheetDef(Form currentForm, SheetDef sheetDef, bool isNew, List<WebForms_SheetDef> webFormSheetDefs = null)
    {
        try
        {
            SheetsSynchProxy.TimeoutOverride = 300000;

            if (isNew)
            {
                WebForms_SheetDefs.TryUploadSheetDef(sheetDef);
            }
            else
            {
                if (webFormSheetDefs == null)
                {
                    return false;
                }

                foreach (var webFormsSheetDef in webFormSheetDefs)
                {
                    WebForms_SheetDefs.UpdateSheetDef(webFormsSheetDef.WebSheetDefID, sheetDef, doCatchExceptions: false);
                }
            }

            return true;
        }
        catch (WebException webEx)
        {
            if (((HttpWebResponse) webEx.Response).StatusCode != HttpStatusCode.RequestEntityTooLarge)
            {
                FriendlyException.Show(webEx.Message, webEx);
                return false;
            }

            var chunkSize = 1024 * 1024;
            var isTooLargeRequest = true;

            while (isTooLargeRequest && chunkSize >= 1024)
            {
                try
                {
                    if (isNew)
                    {
                        WebForms_SheetDefs.TryUploadSheetDefChunked(sheetDef, chunkSize);
                    }
                    else
                    {
                        foreach (var webFormsSheetDef in webFormSheetDefs)
                        {
                            WebForms_SheetDefs.UpdateSheetDefChunked(webFormsSheetDef.WebSheetDefID, sheetDef, chunkSize);
                        }
                    }
                }
                catch (WebException webEx2)
                {
                    if (((HttpWebResponse) webEx2.Response).StatusCode == HttpStatusCode.RequestEntityTooLarge)
                    {
                        chunkSize /= 2; //Chunksize is divided by two each iteration after the first
                        continue; //if still too large, continue to while loop
                    }

                    FriendlyException.Show(webEx.Message, webEx);
                    return false;
                }
                catch (Exception ex)
                {
                    currentForm.Cursor = Cursors.Default;
                    FriendlyException.Show(ex.Message, ex);
                    return false;
                }

                isTooLargeRequest = false;
            }

            if (!isTooLargeRequest)
            {
                return true;
            }

            currentForm.Cursor = Cursors.Default;
            MsgBox.Show("The sheet you are trying to upload failed after attempts were made to resize the request.");
            return false;
        }
        catch (Exception ex)
        {
            currentForm.Cursor = Cursors.Default;
            FriendlyException.Show(ex.Message, ex);
            return false;
        }
    }

    public static Result TryRetrievePatientTransfersCemt()
    {
        var result = new Result();

        var cemtSheets = Sheets.GetTransferSheets();
        var cemtSheetsToDelete = new List<long>();

        foreach (var sheet in cemtSheets)
        {
            if (cemtSheetsToDelete.Contains(sheet.SheetNum))
            {
                continue;
            }

            try
            {
                if (DidImportSheet(sheet, cemtSheets, ref cemtSheetsToDelete))
                {
                    continue;
                }
                
                result.ListMsgs.Add("User manually cancelled CEMT Patient Transfer importing.");
                result.IsSuccess = false;
                return result;
            }
            catch (Exception e)
            {
                result.ListMsgs.Add(e.Message);
            }
        }

        foreach (var sheetNum in cemtSheetsToDelete)
        {
            Sheets.Delete(sheetNum);
        }

        result.IsSuccess = true;

        return result;
    }

    public static Result TryRetrieveWebForms(string cultureName, List<long> listClinicNums, int countPerBatch)
    {
        var result = new Result();
        
        if (countPerBatch <= 0)
        {
            result.ListMsgs.Add("Invalid batch size.");
            result.IsSuccess = false;
            return result;
        }

        if (WebUtils.GetDentalOfficeID() == 0)
        {
            result.ListMsgs.Add("Either the registration key provided by the dental office is incorrect or the Host Server Address cannot be found.");
            result.IsSuccess = false;
            return result;
        }

        SheetsSynchProxy.TimeoutOverride = 300000;

        List<long> sheetIDs;
        try
        {
            sheetIDs = WebForms_Sheets.GetSheetIDs(listClinicNums: listClinicNums);
        }
        catch (Exception ex)
        {
            var log = "There was a problem downloading the register of pending web forms: " + ex.Message;

            EServiceLogs.MakeLogEntryWebForms(eServiceAction.WFError, note: log);

            result.ListMsgs.Add(log);
            result.IsSuccess = false;

            return result;
        }

        if (sheetIDs.IsNullOrEmpty())
        {
            result.ListMsgs.Add("No pending Web Forms.");
            result.IsSuccess = true;

            return result;
        }
        
        var countTotalBatches = (sheetIDs.Count + countPerBatch - 1) / countPerBatch;
        var patientImportChoice = new PatientImportChoice();
        
        for (var i = 0; i < countTotalBatches; i++)
        {
            var sheetIDsForBatch = sheetIDs.Skip(i * countPerBatch).Take(countPerBatch).ToList();
            
            List<WebForms_Sheet> webFormSheets;
            try
            {
                webFormSheets = WebForms_Sheets.GetSheets(listSheetIDs: sheetIDsForBatch, listClinicNums: listClinicNums);
            }
            catch (Exception ex)
            {
                var log = "There was a problem downloading pending web forms for batch" + $" {i + 1} / {countTotalBatches}";
                log += "\r\n  ^SheetIDs in batch: " + string.Join(", ", sheetIDsForBatch);
                log += "\r\n  ^Error message: " + ex.Message;
                
                EServiceLogs.MakeLogEntryWebForms(eServiceAction.WFError, note: log);
                
                result.ListMsgs.Add(log);
                
                continue;
            }

            var sheetIdsForDeletion = new List<long>();
            foreach (var webFormsSheet in webFormSheets)
            {
                if (sheetIdsForDeletion.Contains(webFormsSheet.SheetID))
                {
                    continue;
                }

                try
                {
                    if (DidImportSheet(webFormsSheet, null, webFormSheets, null, cultureName, ref sheetIdsForDeletion, ref patientImportChoice))
                    {
                        continue;
                    }
                    
                    const string log = "User manually cancelled Web Form importing.";
                    
                    EServiceLogs.MakeLogEntryWebForms(eServiceAction.WFCancelled, note: log);
                    
                    result.ListMsgs.Add(log);
                    result.IsSuccess = false;
                    
                    return result;
                }
                catch (Exception e)
                {
                    result.ListMsgs.Add(e.Message);
                    EServiceLogs.MakeLogEntryWebForms(eServiceAction.WFError, FKey: webFormsSheet.SheetID, note: e.Message);
                }
            }

            if (sheetIdsForDeletion.Count == 0)
            {
                continue;
            }
            
            var webFormsSheetsDelete = webFormSheets.FindAll(x => sheetIdsForDeletion.Contains(x.SheetID));
            
            WebForms_Sheets.DeleteSheetData(webFormsSheetsDelete);
        }

        result.IsSuccess = true;
        
        return result;
    }

    public static Result TryRetrieveUnmatchedWebForms(List<long> clinicNums)
    {
        var result = new Result();
        if (!PrefC.GetBool(PrefName.WebFormsDownloadAutomcatically))
        {
            result.IsSuccess = true; //There shouldn't be any automatically downloaded web forms.
            return result;
        }

        var sheetsDownloaded = Sheets.GetUnmatchedWebFormSheets(clinicNums);
        
        var patientImportChoice = new PatientImportChoice();
        var sheetIDsToDelete = new List<long>();
        
        foreach (var sheet in sheetsDownloaded)
        {
            if (sheetIDsToDelete.Contains(sheet.SheetNum))
            {
                continue;
            }

            try
            {
                if (DidImportSheet(null, sheet, null, sheetsDownloaded, CultureInfo.CurrentCulture.Name, ref sheetIDsToDelete, ref patientImportChoice))
                {
                    continue;
                }
                
                result.ListMsgs.Add("User manually cancelled unmatched patient web forms importing.");
                result.IsSuccess = false;
                    
                return result;
            }
            catch (Exception e)
            {
                result.ListMsgs.Add(e.Message);
            }
        }

        foreach (var sheetNum in sheetIDsToDelete)
        {
            Sheets.Delete(sheetNum);
        }

        result.IsSuccess = true;
        
        return result;
    }

    private static bool DidImportSheet(Sheet sheet, List<Sheet> listSheets, ref List<long> listSheetIdsForDeletion)
    {
        var patientImportChoice = new PatientImportChoice();
        
        return DidImportSheet(null, sheet, null, listSheets, CultureInfo.CurrentCulture.Name, ref listSheetIdsForDeletion, ref patientImportChoice);
    }

    private static bool DidImportSheet(WebForms_Sheet webFormsSheet, Sheet sheet, List<WebForms_Sheet> listWebSheets, List<Sheet> listSheets, string cultureName, ref List<long> listSheetIdsForDeletion, ref PatientImportChoice patientImportChoice)
    {
        var isWebForms = webFormsSheet != null && listWebSheets != null;
        long patNum;
        string lName;
        string fName;
        List<string> listPhoneNumbers;
        string email;
        DateTime bDate;
        
        if (isWebForms)
        {
            WebForms_Sheets.ParseWebFormSheet(webFormsSheet, cultureName, out lName, out fName, out bDate, out listPhoneNumbers, out email);
        }
        else
        {
            Sheets.ParseTransferSheet(sheet, out lName, out fName, out bDate, out listPhoneNumbers, out email);
        }

        PatientImportChoice.ChosenAction lastActionChosen = null;
        if (patientImportChoice != null)
        {
            lastActionChosen = patientImportChoice.GetChoiceForPatient(lName, fName, bDate);
        }

        var listMatchingPats = Patients.GetPatNumsByNameBirthdayEmailAndPhone(lName, fName, bDate, email, listPhoneNumbers);
        
        Patient patient = null;
        
        if (listMatchingPats.IsNullOrEmpty() || listMatchingPats.Count > 1)
        {
            List<long> listWebSheetNumsForPat;
            if (isWebForms)
            {
                var listSheetsToDelete = listSheetIdsForDeletion;
                listWebSheetNumsForPat = WebForms_Sheets.FindSheetsForPat(webFormsSheet, listWebSheets, cultureName).FindAll(x => !listSheetsToDelete.Contains(x));
            }
            else
            {
                //Cemt Import
                listWebSheetNumsForPat = Sheets.FindSheetsForPat(sheet, listSheets);
            }

            //Get the sheets for this patient so we know which ones to pass the logger
            var webFormsSheetsLogged = new List<WebForms_Sheet>();
            if (isWebForms)
            {
                webFormsSheetsLogged = listWebSheets.FindAll(x => listWebSheetNumsForPat.Contains(x.SheetID));
            }

            // Only prompt the user if this is a new name/birthday combo
            if (lastActionChosen == null)
            {
                using var formPatientPickWebForm = new FormPatientPickWebForm();
                
                formPatientPickWebForm.WebFormsSheetCur = webFormsSheet;
                formPatientPickWebForm.CountMatchingSheets = listWebSheetNumsForPat.Count;
                formPatientPickWebForm.SheetCemt = sheet;
                formPatientPickWebForm.LnameEntered = lName;
                formPatientPickWebForm.FnameEntered = fName;
                formPatientPickWebForm.DateBirthEntered = bDate;
                formPatientPickWebForm.HasMoreThanOneMatch = listMatchingPats.Count > 1;

                if (formPatientPickWebForm.ShowDialog() == DialogResult.Cancel)
                {
                    if (!isWebForms)
                    {
                        return false;
                    }
                    
                    var listWebFormsSheetsDelete = new List<WebForms_Sheet>();
                        
                    foreach (var formsSheet in listWebSheets)
                    {
                        if (listSheetIdsForDeletion.Contains(formsSheet.SheetID))
                        {
                            listWebFormsSheetsDelete.Add(formsSheet);
                        }
                    }

                    WebForms_Sheets.DeleteSheetData(listWebFormsSheetsDelete);

                    return false;
                }

                if (formPatientPickWebForm.DialogResult == DialogResult.Ignore)
                {
                    if (formPatientPickWebForm.IsDiscardAll())
                    {
                        listSheetIdsForDeletion.AddRange(listWebSheetNumsForPat);

                        foreach (var formsSheet in webFormsSheetsLogged)
                        {
                            EServiceLogs.MakeLogEntryWebForms(eServiceAction.WFDiscardedForm, 0, formsSheet.ClinicNum, formsSheet.SheetID, formsSheet.EServiceLogGuid);
                        }
                    }
                    else
                    {
                        patientImportChoice?.SetSkipActionForPatient(lName, fName, bDate);

                        foreach (var formsSheet in webFormsSheetsLogged)
                        {
                            EServiceLogs.MakeLogEntryWebForms(eServiceAction.WFSkippedForm, 0, formsSheet.ClinicNum, formsSheet.SheetID, formsSheet.EServiceLogGuid);
                        }
                    }

                    return true;
                }

                patNum = formPatientPickWebForm.PatNumSelected;
                
                if (patientImportChoice != null && patNum != 0)
                {
                    patientImportChoice.SetPatNumForPatient(lName, fName, bDate, patNum);
                }
            }
            else if (lastActionChosen.IsSkip)
            {
                foreach (var formsSheet in webFormsSheetsLogged)
                {
                    EServiceLogs.MakeLogEntryWebForms(eServiceAction.WFSkippedForm, patNum: 0, formsSheet.ClinicNum, formsSheet.SheetID, formsSheet.EServiceLogGuid);
                }

                return true;
            }
            else
            {
                patNum = lastActionChosen.PatNum;
            }
        }
        else
        {
            patNum = listMatchingPats[0];
            patient = Patients.GetPat(patNum);

            var logText = isWebForms ? "Web form import from:" : "CEMT patient transfer import from:";

            logText += " " + lName + ", " + fName + " " + bDate.ToShortDateString() + "\r\nAuto imported into: " + patient.LName + ", " + patient.FName + " " + patient.Birthdate.ToShortDateString();

            SecurityLogs.MakeLogEntry(EnumPermType.SheetEdit, patNum, logText);
        }

        if (patNum == 0)
        {
            patient = CreatePatient(lName, fName, bDate, webFormsSheet, sheet, cultureName);
            patNum = patient.PatNum;
            
            var logText = isWebForms ? "Web form import from:" : "CEMT patient transfer import from:";
            
            logText += " " + lName + ", " + fName + " " + bDate.ToShortDateString() + "\r\nUser created new pat: " + patient.LName + ", " + patient.FName + " " + patient.Birthdate.ToShortDateString();
            
            SecurityLogs.MakeLogEntry(EnumPermType.SheetEdit, patNum, logText);
        }
        else if (patient == null)
        {
            patient = Patients.GetPat(patNum);
        }

        if (isWebForms)
        {
            if (Sheets.HasWebFormSheetID(webFormsSheet.SheetID))
            {
                listSheetIdsForDeletion.Add(webFormsSheet.SheetID);
                
                return true;
            }

            var newSheet = SheetUtil.CreateSheetFromWebSheet(patNum, webFormsSheet);
            
            Sheets.SaveNewSheet(newSheet);
            
            if (DataExistsInDb(newSheet))
            {
                listSheetIdsForDeletion.Add(webFormsSheet.SheetID);
            }

            EServiceLogs.MakeLogEntry(eServiceAction.WFCompletedForm, eServiceType.WebForms, FKeyType.SheetNum, patNum, newSheet.ClinicNum, newSheet.SheetNum);
        }
        else
        {
            sheet.PatNum = patNum;
            sheet.DateTimeSheet = MiscData.GetNowDateTime();
            sheet.ClinicNum = patient.ClinicNum;
            sheet.IsWebForm = true;
            
            Sheets.Update(sheet);
        }

        return true;
    }

    private static Patient CreatePatient(string lastName, string firstName, DateTime birthDate, WebForms_Sheet webFormSheet, Sheet sheet, string cultureName)
    {
        var isWebForm = webFormSheet != null;

        var patientNew = new Patient
        {
            LName = lastName,
            FName = firstName,
            Birthdate = birthDate,
            ClinicNum = isWebForm ? webFormSheet.ClinicNum : sheet.ClinicNum
        };

        patientNew.BillingType = SIn.Long(ClinicPrefs.GetPrefValue(PrefName.PracticeDefaultBillType, patientNew.ClinicNum));
        patientNew.PriProv = Providers.GetDefaultProvider(Clinics.ClinicNum).ProvNum;

        var type = patientNew.GetType();
        var fieldInfos = type.GetFields();

        foreach (var fieldInfo in fieldInfos)
        {
            if (isWebForm)
            {
                var webFormsSheetFields = webFormSheet.SheetFields.FindAll(x => x.FieldName.ToLower() == fieldInfo.Name.ToLower() || (x.FieldName.ToLower().StartsWith("state") && fieldInfo.Name == nameof(Patient.State)));

                if (!webFormsSheetFields.Any())
                {
                    continue;
                }

                foreach (var webFormsSheetField in webFormsSheetFields)
                {
                    FillPatientFields(patientNew, fieldInfo, webFormsSheetField.FieldValue, webFormsSheetField.RadioButtonValue, cultureName, true, false);
                }
            }
            else
            {
                var listSheetFields = sheet.SheetFields.FindAll(x => x.FieldName.ToLower() == fieldInfo.Name.ToLower() || (x.FieldName.ToLower().StartsWith("state") && fieldInfo.Name == nameof(Patient.State)));

                if (!listSheetFields.Any())
                {
                    continue;
                }

                foreach (var sheetField in listSheetFields)
                {
                    FillPatientFields(patientNew, fieldInfo, sheetField.FieldValue, sheetField.RadioButtonValue, "", false, sheet.IsCemtTransfer);
                }
            }
        }

        Patients.Insert(patientNew);

        var logText = "Created from CEMT transfer.";
        if (isWebForm)
        {
            logText = "Created from Web Forms.";
        }

        SecurityLogs.MakeLogEntry(EnumPermType.PatientCreate, patientNew.PatNum, logText);

        var patientOld = patientNew.Copy();
        patientNew.Guarantor = patientNew.PatNum;
        Patients.Update(patientNew, patientOld);

        if (HL7Defs.IsExistingHL7Enabled())
        {
            var messageHl7 = MessageConstructor.GenerateADT(patientNew, patientNew, EventTypeHL7.A04);

            if (messageHl7 != null)
            {
                HL7Msgs.Insert(new HL7Msg
                {
                    AptNum = 0,
                    HL7Status = HL7MessageStatus.OutPending,
                    MsgText = messageHl7.ToString(),
                    PatNum = patientNew.PatNum
                });
            }
        }

        if (HieClinics.IsEnabled())
        {
            HieQueues.Insert(new HieQueue(patientNew.PatNum));
        }

        return patientNew;
    }

    private static bool DataExistsInDb(Sheet sheet)
    {
        var dataExistsInDb = true;
        if (sheet is null)
        {
            return true;
        }
        
        var sheetNum = sheet.SheetNum;
        var sheetFromDb = Sheets.GetSheet(sheetNum);
        if (sheetFromDb != null)
        {
            dataExistsInDb = CompareSheets(sheetFromDb, sheet);
        }

        return dataExistsInDb;
    }

    private static bool CompareSheets(Sheet sheetFromDb, Sheet sheetNew)
    {
        var sheetFromDbSorted = new Sheet();
        var sheetNewSorted = new Sheet();
        
        sheetFromDbSorted.SheetFields = sheetFromDb.SheetFields.OrderBy(sf => sf.SheetFieldNum).ToList();
        sheetNewSorted.SheetFields = sheetNew.SheetFields.OrderBy(sf => sf.SheetFieldNum).ToList();
        
        for (var i = 0; i < sheetFromDbSorted.SheetFields.Count; i++)
        {
            if (sheetFromDbSorted.SheetFields[i].SheetNum != sheetNewSorted.SheetFields[i].SheetNum
                || sheetFromDbSorted.SheetFields[i].FieldType != sheetNewSorted.SheetFields[i].FieldType
                || sheetFromDbSorted.SheetFields[i].FieldName != sheetNewSorted.SheetFields[i].FieldName
                || sheetFromDbSorted.SheetFields[i].FieldValue != sheetNewSorted.SheetFields[i].FieldValue
                || sheetFromDbSorted.SheetFields[i].FontSize != sheetNewSorted.SheetFields[i].FontSize
                || sheetFromDbSorted.SheetFields[i].FontName != sheetNewSorted.SheetFields[i].FontName
                || sheetFromDbSorted.SheetFields[i].FontIsBold != sheetNewSorted.SheetFields[i].FontIsBold
                || sheetFromDbSorted.SheetFields[i].XPos != sheetNewSorted.SheetFields[i].XPos
                || sheetFromDbSorted.SheetFields[i].YPos != sheetNewSorted.SheetFields[i].YPos
                || sheetFromDbSorted.SheetFields[i].Width != sheetNewSorted.SheetFields[i].Width
                || sheetFromDbSorted.SheetFields[i].Height != sheetNewSorted.SheetFields[i].Height
                || sheetFromDbSorted.SheetFields[i].GrowthBehavior != sheetNewSorted.SheetFields[i].GrowthBehavior
                || sheetFromDbSorted.SheetFields[i].RadioButtonValue != sheetNewSorted.SheetFields[i].RadioButtonValue
                || sheetFromDbSorted.SheetFields[i].RadioButtonGroup != sheetNewSorted.SheetFields[i].RadioButtonGroup
                || sheetFromDbSorted.SheetFields[i].IsRequired != sheetNewSorted.SheetFields[i].IsRequired
                || sheetFromDbSorted.SheetFields[i].TabOrder != sheetNewSorted.SheetFields[i].TabOrder
                || sheetFromDbSorted.SheetFields[i].ReportableName != sheetNewSorted.SheetFields[i].ReportableName
                || sheetFromDbSorted.SheetFields[i].TextAlign != sheetNewSorted.SheetFields[i].TextAlign
                || sheetFromDbSorted.SheetFields[i].ItemColor != sheetNewSorted.SheetFields[i].ItemColor
                || sheetFromDbSorted.SheetFields[i].UiLabelMobile != sheetNewSorted.SheetFields[i].UiLabelMobile
                || sheetFromDbSorted.SheetFields[i].UiLabelMobileRadioButton != sheetNewSorted.SheetFields[i].UiLabelMobileRadioButton
               )
            {
                return false;
            }
        }

        return true;
    }

    private static void FillPatientFields(Patient patient, FieldInfo fieldInfo, string sheetWebFieldValue, string radioButtonValue, string cultureName, bool isWebForm, bool isCemtTransfer)
    {
        try
        {
            switch (fieldInfo.Name)
            {
                case "Birthdate":
                    fieldInfo.SetValue(patient, SheetFields.GetBirthDate(sheetWebFieldValue, isWebForm, isCemtTransfer, cultureName: cultureName));
                    break;

                case "Gender":
                    switch (radioButtonValue)
                    {
                        case "Male":
                        {
                            if (sheetWebFieldValue == "X")
                            {
                                fieldInfo.SetValue(patient, PatientGender.Male);
                            }

                            break;
                        }
                        case "Female":
                        {
                            if (sheetWebFieldValue == "X")
                            {
                                fieldInfo.SetValue(patient, PatientGender.Female);
                            }

                            break;
                        }
                    }

                    break;
                case "Position":
                    switch (radioButtonValue)
                    {
                        case "Married":
                        {
                            if (sheetWebFieldValue == "X")
                            {
                                fieldInfo.SetValue(patient, PatientPosition.Married);
                            }

                            break;
                        }

                        case "Single":
                        {
                            if (sheetWebFieldValue == "X")
                            {
                                fieldInfo.SetValue(patient, PatientPosition.Single);
                            }

                            break;
                        }
                    }

                    break;

                case "PreferContactMethod":
                case "PreferConfirmMethod":
                case "PreferRecallMethod":
                    if (Enum.TryParse(sheetWebFieldValue, out ContactMethod method))
                    {
                        fieldInfo.SetValue(patient, method);
                    }

                    switch (radioButtonValue)
                    {
                        case "HmPhone" when sheetWebFieldValue == "X":
                            fieldInfo.SetValue(patient, ContactMethod.HmPhone);
                            break;
                        case "WkPhone" when sheetWebFieldValue == "X":
                            fieldInfo.SetValue(patient, ContactMethod.WkPhone);
                            break;
                        case "WirelessPh" when sheetWebFieldValue == "X":
                            fieldInfo.SetValue(patient, ContactMethod.WirelessPh);
                            break;
                        case "Email" when sheetWebFieldValue == "X":
                            fieldInfo.SetValue(patient, ContactMethod.Email);
                            break;
                    }

                    break;

                case "StudentStatus":
                    switch (radioButtonValue)
                    {
                        case "Nonstudent":
                        {
                            if (sheetWebFieldValue == "X")
                            {
                                fieldInfo.SetValue(patient, "");
                            }

                            break;
                        }
                        case "Fulltime":
                        {
                            if (sheetWebFieldValue == "X")
                            {
                                fieldInfo.SetValue(patient, "F");
                            }

                            break;
                        }
                        case "Parttime":
                        {
                            if (sheetWebFieldValue == "X")
                            {
                                fieldInfo.SetValue(patient, "P");
                            }

                            break;
                        }
                    }

                    break;

                case "ins1Relat":
                case "ins2Relat":
                    if (radioButtonValue == "Self")
                    {
                        if (sheetWebFieldValue == "X")
                        {
                            fieldInfo.SetValue(patient, Relat.Self);
                        }
                    }

                    if (radioButtonValue == "Spouse")
                    {
                        if (sheetWebFieldValue == "X")
                        {
                            fieldInfo.SetValue(patient, Relat.Spouse);
                        }
                    }

                    if (radioButtonValue == "Child")
                    {
                        if (sheetWebFieldValue == "X")
                        {
                            fieldInfo.SetValue(patient, Relat.Child);
                        }
                    }

                    break;
                default:
                    fieldInfo.SetValue(patient, sheetWebFieldValue);
                    break;
            }
        }
        catch (Exception e)
        {
            ODMessageBox.Show(fieldInfo.Name + e.Message);
        }
    }

    private class PatientImportChoice
    {
        private readonly Dictionary<string, ChosenAction> _patientChoices = new();

        private static string GetKeyForPatient(string lName, string fName, DateTime bDate)
        {
            return lName + fName + bDate.ToString("MMddyyyy");
        }

        public void SetSkipActionForPatient(string lName, string fName, DateTime bDate)
        {
            var patientData = GetKeyForPatient(lName, fName, bDate);

            _patientChoices[patientData] = new ChosenAction(isSkip: true);
        }

        public void SetPatNumForPatient(string lName, string fName, DateTime bDate, long patNum)
        {
            var patientData = GetKeyForPatient(lName, fName, bDate);

            _patientChoices[patientData] = new ChosenAction(isSkip: false, patNum: patNum);
        }

        public ChosenAction GetChoiceForPatient(string lName, string fName, DateTime bDate)
        {
            var patientData = GetKeyForPatient(lName, fName, bDate);

            return _patientChoices.TryGetValue(patientData, out var patient) ? patient : null;
        }

        public class ChosenAction(bool isSkip, long patNum = 0)
        {
            public readonly long PatNum = patNum;
            public readonly bool IsSkip = isSkip;
        }
    }
}