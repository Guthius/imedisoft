using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CodeBase;
using DataConnectionBase;
using Newtonsoft.Json;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EForms
{
	/// <summary>
	///     Gets a single eForm from the database.  Then, gets all the fields for it.  So it returns a fully functional
	///     eForm. Returns null if the eform isn't found in the database.
	/// </summary>
	public static EForm GetEForm(long eFormNum)
    {
        var eForm = EFormCrud.SelectOne(eFormNum);
        if (eForm == null) return null; //eForm was deleted.
        eForm.ListEFormFields = EFormFields.GetForForm(eFormNum);
        return eForm;
    }

	/// <summary>
	///     Gets a list of eForms for a single patient from the database.
	///     Optionally provide a list of EnumEFormStatus values to filter on.
	/// </summary>
	public static List<EForm> GetForPatient(long patNum, params EnumEFormStatus[] listEFormStatusFilter)
    {
        var command = "SELECT * FROM eform WHERE PatNum=" + SOut.Long(patNum) + " ";
        if (!listEFormStatusFilter.IsNullOrEmpty()) command += " AND eform.Status IN(" + string.Join(",", listEFormStatusFilter.Select(x => SOut.Int((int) x))) + ") ";
        var listEForms = EFormCrud.SelectMany(command);
        for (var i = 0; i < listEForms.Count; i++) listEForms[i].ListEFormFields = EFormFields.GetForForm(listEForms[i].EFormNum);
        return listEForms;
    }

    
    public static long Insert(EForm eForm)
    {
        return EFormCrud.Insert(eForm);
    }

    /// <Summary>
    ///     Saves a list of eForms, including their fields, to the Database. Only saves new eForms, ignores eForms that
    ///     are not new.
    /// </Summary>
    public static void SaveNewEForms(List<EForm> listEForms)
    {
        for (var i = 0; i < listEForms.Count; i++)
        {
            if (!listEForms[i].IsNew) continue;
            EFormCrud.Insert(listEForms[i]);
            var listEFormFields = listEForms[i].ListEFormFields;
            for (var j = 0; j < listEFormFields.Count; j++)
            {
                listEFormFields[j].EFormNum = listEForms[i].EFormNum;
                EFormFieldCrud.Insert(listEFormFields[j]);
            }
        }
    }

    
    public static void Update(EForm eForm)
    {
        EFormCrud.Update(eForm);
    }

    
    public static void Delete(long eFormNum, long patNum)
    {
//todo: mark deleted instead, just like sheets
        EFormCrud.Delete(eFormNum);
        //This triggers removal from eClipboard.
        MobileNotifications.CI_RemoveEForm(patNum, eFormNum);
    }

    /// <summary>
    ///     The eFormDef passed in must have ListEFormFieldDefs already filled. The resulting EForm will also have its
    ///     fields already attached. Neither the form nor the fields get inserted into the db here.
    /// </summary>
    public static EForm CreateEFormFromEFormDef(EFormDef eFormDef, long patNum, EnumEFormStatus status)
    {
        var eForm = new EForm();
        eForm.IsNew = true;
        eForm.FormType = eFormDef.FormType;
        eForm.PatNum = patNum;
        eForm.DateTimeShown = DateTime_.Now;
        eForm.DateTEdited = DateTime_.Now;
        eForm.Description = eFormDef.Description;
        eForm.MaxWidth = eFormDef.MaxWidth;
        eForm.RevID = eFormDef.RevID;
        eForm.ShowLabelsBold = eFormDef.ShowLabelsBold;
        eForm.SpaceBelowEachField = eFormDef.SpaceBelowEachField;
        eForm.SpaceToRightEachField = eFormDef.SpaceToRightEachField;
        eForm.SaveImageCategory = eFormDef.SaveImageCategory;
        eForm.ListEFormFields = EFormFields.FromListDefs(eFormDef.ListEFormFieldDefs);
        eForm.EFormDefNum = eFormDef.EFormDefNum;
        eForm.Status = status;
        return eForm;
    }

    /// <summary>
    ///     Validates a few fields like phone numbers and state format. The eForm must contain a list of eFormFields. The
    ///     maskedSSNOld is used to keep track of what masked SSN was shown when the form was loaded, and stop us from storing
    ///     masked SSNs on accident.  Returns an object that stores the error message and the page number that the problem
    ///     field is located on. Use the page number to go directly to the problem field. If the return object has an empty
    ///     error message, everything passed validation.
    /// </summary>
    public static EFormValidation Validate(EForm eForm, string maskedSSNOld)
    {
        var eFormValidation = new EFormValidation();
        var listEFormFields = eForm.ListEFormFields;
        var pageNum = 1;
        for (var i = 0; i < listEFormFields.Count; i++)
        {
            if (listEFormFields[i].FieldType == EnumEFormFieldType.PageBreak)
            {
                pageNum++;
                continue;
            }

            //Validating Allergies
            if (listEFormFields[i].DbLink == "allergiesNone")
            {
                var listEFormFieldsAllergyChecks = listEFormFields.FindAll(x => x.DbLink.StartsWith("allergy:"));
                var listEFormFieldsAllergiesOther = listEFormFields.FindAll(x => x.DbLink == "allergiesOther");
                if (listEFormFields[i].ValueString == "X")
                {
                    if (listEFormFieldsAllergyChecks.Any(x => x.ValueString == "X") || listEFormFieldsAllergiesOther.Any(x => x.ValueString != ""))
                    {
                        eFormValidation.ErrorMsg = "You cannot have '" + listEFormFields[i].ValueLabel + "' checked and an allergy checked or written in.";
                        eFormValidation.PageNum = pageNum;
                        return eFormValidation;
                    }
                }
                else if (listEFormFields[i].IsRequired)
                {
                    if (listEFormFieldsAllergyChecks.All(x => x.ValueString == "") && listEFormFieldsAllergiesOther.All(x => x.ValueString == ""))
                    {
                        eFormValidation.ErrorMsg = "If you do not have any allergies, you must check '" + listEFormFields[i].ValueLabel + "'";
                        eFormValidation.PageNum = pageNum;
                        return eFormValidation;
                    }
                }
            }

            //Validating Problems
            if (listEFormFields[i].DbLink == "problemsNone")
            {
                var listEFormFieldsProblemChecks = listEFormFields.FindAll(x => x.DbLink.StartsWith("problem:"));
                var listEFormFieldsProblemsOther = listEFormFields.FindAll(x => x.DbLink == "problemsOther");
                if (listEFormFields[i].ValueString == "X")
                {
                    if (listEFormFieldsProblemChecks.Any(x => x.ValueString == "X") || listEFormFieldsProblemsOther.Any(x => x.ValueString != ""))
                    {
                        eFormValidation.ErrorMsg = "You cannot have '" + listEFormFields[i].ValueLabel + "' checked and a problem checked or written in.";
                        eFormValidation.PageNum = pageNum;
                        return eFormValidation;
                    }
                }
                else if (listEFormFields[i].IsRequired)
                {
                    if (listEFormFieldsProblemChecks.All(x => x.ValueString == "") && listEFormFieldsProblemsOther.All(x => x.ValueString != ""))
                    {
                        eFormValidation.ErrorMsg = "If you do not have any problems, you must check '" + listEFormFields[i].ValueLabel + "'";
                        eFormValidation.PageNum = pageNum;
                        return eFormValidation;
                    }
                }
            }

            //Validating Medications
            if (listEFormFields[i].FieldType == EnumEFormFieldType.MedicationList)
            {
            }

            //Validating State Field
            if (!ValidateStateField(eForm))
            {
                eFormValidation.ErrorMsg = "The State field must be exactly two characters in length.";
                eFormValidation.PageNum = pageNum;
                return eFormValidation;
            }

            //Validating Phone Numbers
            if (listEFormFields[i].DbLink.Contains("Phone"))
            {
                if (TelephoneNumbers.IsNumberValidTenDigit(listEFormFields[i].ValueString))
                {
                    listEFormFields[i].ValueString = TelephoneNumbers.AutoFormat(listEFormFields[i].ValueString);
                }
                else
                {
                    eFormValidation.ErrorMsg = "Please fix the format on the entered phone number";
                    eFormValidation.PageNum = pageNum;
                    return eFormValidation;
                }
            }

            //Validating SSN
            if (!ValidateSSN(listEFormFields[i], maskedSSNOld))
            {
                eFormValidation.ErrorMsg = "Patient's Social Security Number is invalid.";
                eFormValidation.PageNum = pageNum;
                return eFormValidation;
            }
//todo
//Validate email field always
        }

        return eFormValidation; //If we get to here, this object should still have default values and everything passed validation.
    }

    /// <summary>
    ///     This is not called from OD proper. Required fields are only enforced in eClipboard. Returns an object that
    ///     stores the error message and the page number that the problem field is located on. Use the page number to go
    ///     directly to the problem field. If the return object has an empty error message, everything passed validation.
    /// </summary>
    public static EFormValidation ValidateRequired(EForm eForm, bool isMedNoneChecked)
    {
        var eFormValidation = new EFormValidation();
        var listEFormFields = eForm.ListEFormFields;
        var pageNum = 1;
        for (var i = 0; i < listEFormFields.Count; i++)
        {
            if (listEFormFields[i].FieldType == EnumEFormFieldType.PageBreak)
            {
                pageNum++;
                continue;
            }

            if (listEFormFields[i].FieldType == EnumEFormFieldType.MedicationList
                && listEFormFields[i].IsRequired)
            {
                var listEFormMeds = JsonConvert.DeserializeObject<List<EFormMed>>(listEFormFields[i].ValueString);
                if (listEFormMeds.Count == 0 && !isMedNoneChecked)
                {
                    eFormValidation.ErrorMsg = "Medication List is a required field";
                    eFormValidation.PageNum = pageNum;
                    return eFormValidation;
                }
            }

            if (listEFormFields[i].DbLink != "allergiesNone" && listEFormFields[i].DbLink != "problemsNone")
                //This is for all the other fields. This does work with radiobuttons, for example
                if (listEFormFields[i].IsRequired
                    && listEFormFields[i].ValueString.IsNullOrEmpty()
                    && !listEFormFields[i].IsHiddenCondit)
                {
                    eFormValidation.ErrorMsg = listEFormFields[i].ValueLabel + " is a required field";
                    eFormValidation.PageNum = pageNum;
                    return eFormValidation;
                }
        }

        return eFormValidation; //If we get to here, this object should still have default values and everything passed validation.
    }

    ///<summary>Verify that the InputField of "State" is exactly 2 characters in length. </summary>
    public static bool ValidateStateField(EForm eForm)
    {
        if (eForm.FormType != EnumEFormType.PatientForm) return true;
        for (var i = 0; i < eForm.ListEFormFields.Count; i++)
        {
            if (eForm.ListEFormFields[i].DbLink != "State") continue;
            if (eForm.ListEFormFields[i].ValueString.Trim().Length != 2 && eForm.ListEFormFields[i].ValueString.Trim().Length > 0) return false;
        }

        return true;
    }

    public static bool ValidateSSN(EFormField eFormField, string maskedSSNOld)
    {
        if (eFormField.DbLink != "SSN") return true;
        var textSSN = eFormField.ValueString;
        if (CultureInfo.CurrentCulture.Name != "en-US") return true;
        //only reformats if in USA and exactly 9 digits.
        if (string.IsNullOrEmpty(textSSN)) return true;
        if (PrefC.GetBool(PrefName.PatientSSNMasked) || !Security.IsAuthorized(EnumPermType.PatientSSNView, true))
            if (textSSN == maskedSSNOld)
                //If SSN hasn't changed, don't validate.  It is masked.
                return true;

        if (!Regex.IsMatch(textSSN, @"^\d{9}$") && !Regex.IsMatch(textSSN, @"^\d{3}-\d{2}-\d{4}$")) return false;
        if (textSSN.Length == 9)
        {
            //if just numbers, try to reformat.
            for (var j = 0; j < textSSN.Length; j++)
                if (!char.IsNumber(textSSN, j))
                    return false;

            eFormField.ValueString = textSSN.Substring(0, 3) + "-"
                                                             + textSSN.Substring(3, 2) + "-" + textSSN.Substring(5, 4);
        }

        return true;
    }

    /// <summary>
    ///     Page numbers will be incremented for each PageBreak field type found. If no breaks are found, PageNum will be
    ///     1.
    /// </summary>
    public static void SetPageNumber(EForm eForm)
    {
        var pageNum = 1;
        for (var i = 0; i < eForm.ListEFormFields.Count; i++)
        {
            if (eForm.ListEFormFields[i].FieldType == EnumEFormFieldType.PageBreak)
            {
                pageNum++;
                continue;
            }

            eForm.ListEFormFields[i].Page = pageNum;
        }
    }

    /// <summary>
    ///     Creates eForms for the patient to fill out. Starts by getting all EClipboardSheetDefs from the
    ///     eClipboardSheetDef table. This includes sheets and eForms. We then filter out any eForms that are already created
    ///     and need filling out. Next we will remove any that don't pass the MinAge, MaxAge settings and any that have already
    ///     been completed and have a prefillStatus that is set to Once.
    /// </summary>
    public static int CreateEFormForCheckIn(Appointment appointment)
    {
        if (!MobileAppDevices.IsClinicSignedUpForEClipboard(true ? appointment.ClinicNum : 0)) //this clinic isn't signed up for this feature
            return 0;
        if (!ClinicPrefs.GetBool(PrefName.EClipboardCreateMissingFormsOnCheckIn, appointment.ClinicNum)) //This feature is turned off
            return 0;
        var useDefault = ClinicPrefs.GetBool(PrefName.EClipboardUseDefaults, appointment.ClinicNum);
        var listEClipboardSheetDefEFormDefsToCreate = EClipboardSheetDefs.GetForClinic(useDefault ? 0 : appointment.ClinicNum);
        //This list can hold sheets and eForms. Lets remove all forms that are not eForms since this method is only for creating eForms.
        listEClipboardSheetDefEFormDefsToCreate.RemoveAll(x => x.EFormDefNum == 0);
        if (listEClipboardSheetDefEFormDefsToCreate.Count == 0) //There aren't any eForms to create here
            return 0;
        //Get all eForms that the patient has already filled out.
        var listEFormsAlreadyCompleted = GetForPatient(appointment.PatNum);
        var patient = Patients.GetPat(appointment.PatNum);
        //Filter out any that don't pass the MinAge, MaxAge settings and any that have already been completed and have a prefillStatus that is set to Once. 
        listEClipboardSheetDefEFormDefsToCreate.RemoveAll(x => x.MinAge != -1 && patient.Age < x.MinAge);
        listEClipboardSheetDefEFormDefsToCreate.RemoveAll(x => x.MaxAge != -1 && patient.Age > x.MaxAge);
        //Remove any eClipboard defs that are set to EnumEClipFreq.Once and have been filled out. 
        listEClipboardSheetDefEFormDefsToCreate.RemoveAll(x => x.Frequency == EnumEClipFreq.Once && listEFormsAlreadyCompleted.Any(y => y.EFormDefNum == x.EFormDefNum));
        listEClipboardSheetDefEFormDefsToCreate = listEClipboardSheetDefEFormDefsToCreate.OrderBy(x => x.ItemOrder).ToList();
        var listEFormsNew = new List<EForm>();
        for (var i = 0; i < listEClipboardSheetDefEFormDefsToCreate.Count; i++)
        {
            //First check if we've already completed this form against our resubmission interval rules
            var eFormLastCompleted = listEFormsAlreadyCompleted
                .Where(x => x.EFormDefNum == listEClipboardSheetDefEFormDefsToCreate[i].EFormDefNum)
                .OrderBy(x => x.DateTimeShown)
                .LastOrDefault() ?? new EForm();
            //This is an edge case where the eForm was inserted in the database but not submitted yet. We don't want to create the eForm again if it's still on the checkin list.
            if (eFormLastCompleted.Status == EnumEFormStatus.ShowInEClipboard) continue;
            //If the eForm's frequency is set to EachTime, we need to skip this because it should create the eForm every time they checkin.
            if (eFormLastCompleted.DateTimeShown > DateTime.MinValue && listEClipboardSheetDefEFormDefsToCreate[i].Frequency != EnumEClipFreq.EachTime)
            {
                if (listEClipboardSheetDefEFormDefsToCreate[i].ResubmitInterval.Days == 0) continue; //If this interval is set to 0 and they've already completed this form once, we never want to creat it automatically again.
                var elapsed = (DateTime_.Today - eFormLastCompleted.DateTimeShown.Date).Days;
                if (elapsed < listEClipboardSheetDefEFormDefsToCreate[i].ResubmitInterval.Days) continue; //The interval hasn't elapsed yet so we don't want to create this eForm.
            }

            var eFormDef = EFormDefs.GetFirstOrDefault(x => x.EFormDefNum == listEClipboardSheetDefEFormDefsToCreate[i].EFormDefNum);
            if (eFormDef == null) continue; //Didn't find a matching eFormDef in the cache.
            eFormDef.ListEFormFieldDefs = EFormFieldDefs.GetWhere(x => x.EFormDefNum == eFormDef.EFormDefNum);
            var eForm = CreateEFormFromEFormDef(eFormDef, appointment.PatNum, EnumEFormStatus.ShowInEClipboard);
            if (listEClipboardSheetDefEFormDefsToCreate[i].PrefillStatus == PrefillStatuses.PreFill)
            {
                EFormFiller.FillFields(eForm);
            }
            else
            {
                //If we are not prefilling the eForm, we still need to replace static text fields. 
                var family = Patients.GetFamily(eForm.PatNum);
                var listEnumStaticTextFields = EFormFiller.GetAllStaticTextFieldsForEForm(eForm);
                var staticTextFieldDependency = StaticTextData.GetStaticTextDependencies(listEnumStaticTextFields);
                var listStaticTextReplacements = SheetFiller.GetStaticTextReplacements(listEnumStaticTextFields, patient, family, null, staticTextFieldDependency, 0);
                EFormFiller.ReplaceStaticTextFields(listStaticTextReplacements, eForm, patient, family);
            }

            TranslateFields(eForm, patient.Language);
            listEFormsNew.Add(eForm);
        }

        SaveNewEForms(listEFormsNew);
        return listEFormsNew.Count;
    }

    /// <summary>
    ///     Loops through all the fields and appends together all the ValueStrings. All the ValueStrings must have been
    ///     filled first, and it excludes all SigBox types. The order is critical.
    /// </summary>
    public static string GetSignatureKeyData(List<EFormField> listEFormFields)
    {
        //The fields will already be sorted by ItemOrder
        var stringBuilder = new StringBuilder();
        for (var i = 0; i < listEFormFields.Count; i++)
        {
            if (listEFormFields[i].FieldType.In(EnumEFormFieldType.SigBox)) continue;
            stringBuilder.Append(listEFormFields[i].ValueString);
        }

        return stringBuilder.ToString();
    }

    ///<summary>Language will be empty string if the patient does not have a language set.</summary>
    public static void TranslateFields(EForm eForm, string langIso3)
    {
        for (var i = 0; i < eForm.ListEFormFields.Count; i++)
        {
            if (eForm.ListEFormFields[i].FieldType == EnumEFormFieldType.RadioButtons)
            {
                //Language translations are stored as pipe delimited string like this: "label|button1|button2"
                //Our setup window, FrmEFormDefEdit, rigorously ensures that the number of items in the translation exactly matches label+buttons.
                //List<string> listEnglishs=
                var numRadioBtns = eForm.ListEFormFields[i].PickListVis.Split('|').ToList().Count();
                var strLabels = eForm.ListEFormFields[i].ValueLabel + "|" + eForm.ListEFormFields[i].PickListVis;
                var strTranslations = LanguagePats.TranslateEFormField(eForm.ListEFormFields[i].EFormFieldDefNum, "", strLabels, langIso3);
                var listTranslations = strTranslations.Split('|').ToList(); //Ex: [label,button1,button2]
                //int numTranslations=strTranslations.Split('|').ToList().Count()-1;
                if (listTranslations.Count - 1 != numRadioBtns) //subtract 1 because of the label at idx 0.
                    continue; //should never happen
                eForm.ListEFormFields[i].ValueLabel = listTranslations[0];
                listTranslations.RemoveAt(0);
                eForm.ListEFormFields[i].PickListVis = string.Join("|", listTranslations);
                continue;
            }

            if (eForm.ListEFormFields[i].FieldType == EnumEFormFieldType.MedicationList)
            {
                //Language translations are stored as pipe delimited string.
                var eFormMedListLayout = JsonConvert.DeserializeObject<EFormMedListLayout>(eForm.ListEFormFields[i].ValueLabel);
                var strEnglish = eFormMedListLayout.Title + "|" + eFormMedListLayout.HeaderCol1 + "|" + eFormMedListLayout.HeaderCol2 + "|Delete|Add|None";
                var strTranslations = LanguagePats.TranslateEFormField(eForm.ListEFormFields[i].EFormFieldDefNum, "", strEnglish, langIso3);
                var listTranslations = strTranslations.Split('|').ToList();
                eFormMedListLayout.Title = listTranslations[0];
                eFormMedListLayout.HeaderCol1 = listTranslations[1];
                eFormMedListLayout.HeaderCol2 = listTranslations[2];
                eFormMedListLayout.StrDelete = listTranslations[3];
                eFormMedListLayout.StrAdd = listTranslations[4];
                eFormMedListLayout.StrNone = listTranslations[5];
                eForm.ListEFormFields[i].ValueLabel = JsonConvert.SerializeObject(eFormMedListLayout);
                continue;
            }

            //checkbox,date,label,sigbox,textbox:
            eForm.ListEFormFields[i].ValueLabel = LanguagePats.TranslateEFormField(eForm.ListEFormFields[i].EFormFieldDefNum, "", eForm.ListEFormFields[i].ValueLabel, langIso3);
        }
    }

    /// <summary>
    ///     Used by both FrmEFormDefs and FormEServicesEClipboard. If there are no EFormDefs or EForms in the db when this
    ///     method is called, then this immediately copies all our internal forms into the db. This could have been done in the
    ///     ConvertDb script, but it's easier here and we don't every run complex methods when updating versions. Returns true
    ///     if internal forms were inserted, and returns false if eFormDefs already exist in the db.
    /// </summary>
    public static bool InsertInternalToDb()
    {
        var listEFormDefsCustom = EFormDefs.GetDeepCopy();
        if (listEFormDefsCustom.Count > 0) return false; //eFormDefs already exist in the db, no need to copy internal into db.
        var listEFormDefsInternal = EFormInternal.GetAllInternal();
        for (var i = 0; i < listEFormDefsInternal.Count; i++)
        {
            listEFormDefsInternal[i].DateTCreated = DateTime.Now;
            EFormDefs.Insert(listEFormDefsInternal[i]);
            for (var f = 0; f < listEFormDefsInternal[i].ListEFormFieldDefs.Count; f++)
            {
                listEFormDefsInternal[i].ListEFormFieldDefs[f].EFormDefNum = listEFormDefsInternal[i].EFormDefNum;
                EFormFieldDefs.Insert(listEFormDefsInternal[i].ListEFormFieldDefs[f]);
            }
        }

        return true;
    }
}

public class EFormValidation
{
    ///<summary>If this is empty, then no error.</summary>
    public string ErrorMsg = "";

    ///<summary>So that the UI can then jump to the page where the error was found.</summary>
    public int PageNum;
}