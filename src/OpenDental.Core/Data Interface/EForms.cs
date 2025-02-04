using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using Newtonsoft.Json;

namespace OpenDentBusiness;

public class EForms
{
    public static EForm GetEForm(long eFormNum)
    {
        var eForm = EFormCrud.SelectOne(eFormNum);
        if (eForm is null)
        {
            return null;
        }
        
        eForm.ListEFormFields = EFormFields.GetForForm(eFormNum);
        return eForm;
    }

    public static void Insert(EForm eForm)
    {
        EFormCrud.Insert(eForm);
    }

    public static void Update(EForm eForm)
    {
        EFormCrud.Update(eForm);
    }
    
    public static void Delete(long eFormNum)
    {
        EFormCrud.Delete(eFormNum);
    }

    public static EForm CreateEFormFromEFormDef(EFormDef eFormDef, long patNum, EnumEFormStatus status)
    {
        return new EForm
        {
            IsNew = true,
            FormType = eFormDef.FormType,
            PatNum = patNum,
            DateTimeShown = DateTime_.Now,
            DateTEdited = DateTime_.Now,
            Description = eFormDef.Description,
            MaxWidth = eFormDef.MaxWidth,
            RevID = eFormDef.RevID,
            ShowLabelsBold = eFormDef.ShowLabelsBold,
            SpaceBelowEachField = eFormDef.SpaceBelowEachField,
            SpaceToRightEachField = eFormDef.SpaceToRightEachField,
            SaveImageCategory = eFormDef.SaveImageCategory,
            ListEFormFields = EFormFields.FromListDefs(eFormDef.ListEFormFieldDefs),
            EFormDefNum = eFormDef.EFormDefNum,
            Status = status
        };
    }

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
        }

        return eFormValidation; //If we get to here, this object should still have default values and everything passed validation.
    }

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
}

public class EFormValidation
{
    ///<summary>If this is empty, then no error.</summary>
    public string ErrorMsg = "";

    ///<summary>So that the UI can then jump to the page where the error was found.</summary>
    public int PageNum;
}