using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Newtonsoft.Json;

namespace OpenDentBusiness
{
    public class EFormFiller
    {
        public static void FillFields(EForm eForm)
        {
            var family = Patients.GetFamily(eForm.PatNum);
            
            var patPlans = PatPlans.Refresh(eForm.PatNum);
            if (!PatPlans.IsPatPlanListValid(patPlans))
            {
                patPlans = PatPlans.Refresh(eForm.PatNum);
            }

            var patient = family.GetPatient(eForm.PatNum);
            
            var insSubs = InsSubs.RefreshForFam(family);
            var insPlans = InsPlans.RefreshForSubList(insSubs);
            
            InsPlan insPlan1 = null;
            InsSub insSub1 = null;
            Carrier carrier1 = null;
            
            if (patPlans.Count > 0)
            {
                insSub1 = InsSubs.GetSub(patPlans[0].InsSubNum, insSubs);
                insPlan1 = InsPlans.GetPlan(insSub1.PlanNum, insPlans);
                carrier1 = Carriers.GetCarrier(insPlan1.CarrierNum);
            }

            InsPlan insplan2 = null;
            InsSub sub2 = null;
            Carrier carrier2 = null;
            
            if (patPlans.Count > 1)
            {
                sub2 = InsSubs.GetSub(patPlans[1].InsSubNum, insSubs);
                insplan2 = InsPlans.GetPlan(sub2.PlanNum, insPlans);
                carrier2 = Carriers.GetCarrier(insplan2.CarrierNum);
            }
            
            var allergyDefs = AllergyDefs.GetAll(includeHidden: true);
            
            var patientNote = PatientNotes.Refresh(patient.PatNum, patient.Guarantor);
            var patientAllergies = Allergies.GetAll(eForm.PatNum, showInactive: false);
            var patientMedications = MedicationPats.Refresh(patient.PatNum, includeDiscontinued: false);
            var patientDiseases = Diseases.Refresh(patient.PatNum, activeOnly: true);

            foreach (var formField in eForm.ListEFormFields)
            {
                switch (formField.DbLink)
                {
                    case "Address":
                        formField.ValueString = patient.Address;
                        break;

                    case "Address2":
                        formField.ValueString = patient.Address2;
                        break;

                    case "allergiesNone":
                    {
                        if (patientAllergies.Count == 0)
                        {
                            formField.ValueString = "X";
                        }

                        break;
                    }

                    case "allergiesOther":
                    {
                        var listStrAllergiesChecks = eForm.ListEFormFields.FindAll(x => x.DbLink.StartsWith("allergy:")).Select(x => x.DbLink.Substring(8)).ToList();
                        var listStrAllergiesToAdd = new List<string>();
                        
                        foreach (var allergy in patientAllergies)
                        {
                            var allergyDef = allergyDefs.Find(x => x.AllergyDefNum == allergy.AllergyDefNum);
                            var strAllerg = allergyDef.Description;
                            if (listStrAllergiesChecks.Contains(strAllerg))
                            {
                                continue;
                            }

                            if (strAllerg.Contains(","))
                            {
                                continue;
                            }

                            if (strAllerg == "Other")
                            {
                                if (allergy.Reaction.Contains(","))
                                {
                                    continue;
                                }

                                listStrAllergiesToAdd.Add(allergy.Reaction);
                                continue;
                            }

                            listStrAllergiesToAdd.Add(strAllerg);
                        }

                        formField.ValueString = string.Join(", ", listStrAllergiesToAdd);
                        break;
                    }
                }

                if (formField.DbLink.StartsWith("allergy:"))
                {
                    var strAllergyName = formField.DbLink.Substring(8);
                    var hasAllergy = false;
                    for (var a = 0; a < patientAllergies.Count; a++)
                    {
                        var allergyDef = allergyDefs.Find(x => x.AllergyDefNum == patientAllergies[a].AllergyDefNum);
                        if (allergyDef is null)
                        {
                            continue;
                        }

                        var strAllergDes = allergyDef.Description;
                        if (strAllergDes != strAllergyName)
                        {
                            continue;
                        }
                        
                        hasAllergy = true;
                        break;
                    }

                    switch (formField.FieldType)
                    {
                        case EnumEFormFieldType.CheckBox:
                        {
                            if (hasAllergy)
                            {
                                formField.ValueString = "X";
                            }

                            break;
                        }
                        
                        case EnumEFormFieldType.RadioButtons when hasAllergy:
                            formField.ValueString = "Y";
                            break;
                        
                        case EnumEFormFieldType.RadioButtons:
                            formField.ValueString = "N";
                            break;
                    }
                }

                switch (formField.DbLink)
                {
                    case "Birthdate":
                        formField.ValueString = patient.Birthdate.ToShortDateString();
                        break;
                    
                    case "City":
                        formField.ValueString = patient.City;
                        break;
                    
                    case "Email":
                        formField.ValueString = patient.Email;
                        break;
                    
                    case "FName":
                        formField.ValueString = patient.FName;
                        break;
                    
                    case "Gender":
                        formField.ValueString = patient.Gender.ToString();
                        break;
                    
                    case "HmPhone":
                        formField.ValueString = patient.HmPhone;
                        break;
                    
                    case "ICEName":
                        formField.ValueString = patientNote.ICEName;
                        break;
                    
                    case "ICEPhone":
                        formField.ValueString = patientNote.ICEPhone;
                        break;
                    
                    case "ins1CarrierName":
                    {
                        if (carrier1 != null)
                        {
                            formField.ValueString = carrier1.CarrierName;
                        }

                        break;
                    }
                    case "ins1CarrierPhone":
                    {
                        if (carrier1 != null)
                        {
                            formField.ValueString = carrier1.Phone;
                        }

                        break;
                    }
                    
                    case "ins1EmployerName":
                    {
                        if (insPlan1 != null)
                        {
                            formField.ValueString = Employers.GetName(insPlan1.EmployerNum);
                        }

                        break;
                    }
                    
                    case "ins1GroupName":
                    {
                        if (insPlan1 != null)
                        {
                            formField.ValueString = insPlan1.GroupName;
                        }

                        break;
                    }
                    case "ins1GroupNum":
                    {
                        if (insPlan1 != null)
                        {
                            formField.ValueString = insPlan1.GroupNum;
                        }

                        break;
                    }
                    
                    case "ins1Relat" when patPlans.Count == 0:
                        formField.ValueString = "";
                        break;
                    
                    case "ins1Relat":
                        formField.ValueString = patPlans[0].Relationship.ToString();
                        break;
                    
                    case "ins1SubscriberID":
                    {
                        if (insPlan1 != null)
                        {
                            formField.ValueString = insSub1.SubscriberID;
                        }

                        break;
                    }
                    
                    case "ins1SubscriberNameF":
                    {
                        if (insPlan1 != null)
                        {
                            formField.ValueString = family.GetNameInFamFirst(insSub1.Subscriber);
                        }

                        break;
                    }
                    
                    case "ins2CarrierName":
                    {
                        if (carrier2 != null)
                        {
                            formField.ValueString = carrier2.CarrierName;
                        }

                        break;
                    }
                    
                    case "ins2CarrierPhone":
                    {
                        if (carrier2 != null)
                        {
                            formField.ValueString = carrier2.Phone;
                        }

                        break;
                    }
                    
                    case "ins2EmployerName":
                    {
                        if (insplan2 != null)
                        {
                            formField.ValueString = Employers.GetName(insplan2.EmployerNum);
                        }

                        break;
                    }
                    
                    case "ins2GroupName":
                    {
                        if (insplan2 != null)
                        {
                            formField.ValueString = insplan2.GroupName;
                        }

                        break;
                    }
                    
                    case "ins2GroupNum":
                    {
                        if (insplan2 != null)
                        {
                            formField.ValueString = insplan2.GroupNum;
                        }

                        break;
                    }
                    
                    case "ins2Relat" when patPlans.Count < 2:
                        formField.ValueString = "";
                        break;
                    
                    case "ins2Relat":
                        formField.ValueString = patPlans[1].Relationship.ToString();
                        break;
                    
                    case "ins2SubscriberID":
                    {
                        if (insplan2 != null)
                        {
                            formField.ValueString = sub2.SubscriberID;
                        }

                        break;
                    }
                    case "ins2SubscriberNameF":
                    {
                        if (insplan2 != null)
                        {
                            formField.ValueString = family.GetNameInFamFirst(sub2.Subscriber);
                        }

                        break;
                    }
                    case "LName":
                        formField.ValueString = patient.LName;
                        break;
                }
                
                if (formField.FieldType == EnumEFormFieldType.MedicationList)
                {
                    var eFormMedListLayout = JsonConvert.DeserializeObject<EFormMedListLayout>(formField.ValueLabel);
                    if (eFormMedListLayout.PrefillCol1)
                    {
                        var listEFormMeds = new List<EFormMed>();
                        foreach (var medicationPat in patientMedications)
                        {
                            var medication = Medications.GetMedication(medicationPat.MedicationNum);
                            var strMed = medicationPat.MedDescript;
                            if (medication != null)
                            {
                                strMed = medication.MedName;
                            }

                            var eFormMed = new EFormMed
                            {
                                MedName = strMed
                            };
                            
                            if (eFormMedListLayout.PrefillCol2)
                            {
                                eFormMed.StrengthFreq = medicationPat.PatNote;
                            }

                            listEFormMeds.Add(eFormMed);
                        }

                        formField.ValueString = JsonConvert.SerializeObject(listEFormMeds);
                    }
                }

                switch (formField.DbLink)
                {
                    case "MiddleI":
                        formField.ValueString = patient.MiddleI;
                        break;
                    
                    case "Position":
                        formField.ValueString = patient.Position.ToString();
                        break;
                    
                    case "PreferConfirmMethod":
                        formField.ValueString = patient.PreferConfirmMethod.ToString();
                        break;
                    
                    case "PreferContactMethod":
                        formField.ValueString = patient.PreferContactMethod.ToString();
                        break;
                    
                    case "PreferRecallMethod":
                        formField.ValueString = patient.PreferRecallMethod.ToString();
                        break;
                    
                    case "Preferred":
                        formField.ValueString = patient.Preferred;
                        break;
                    
                    case "problemsNone":
                    {
                        if (patientDiseases.Count == 0)
                        {
                            formField.ValueString = "X";
                        }

                        break;
                    }
                    
                    case "problemsOther":
                    {
                        var listStrDiseasesChecks = eForm.ListEFormFields.FindAll(x => x.DbLink.StartsWith("problem:")).Select(x => x.DbLink.Substring(8)).ToList();
                        var listStrDiseasesToAdd = new List<string>();
                        
                        foreach (var disease in patientDiseases)
                        {
                            var diseaseDef = DiseaseDefs.GetItem(disease.DiseaseDefNum);
                            var strDisease = diseaseDef.DiseaseName;
                            if (listStrDiseasesChecks.Contains(strDisease))
                            {
                                continue;
                            }

                            if (strDisease.Contains(","))
                            {
                                continue;
                            }

                            if (strDisease == "Other")
                            {
                                if (disease.PatNote.Contains(","))
                                {
                                    continue;
                                }

                                listStrDiseasesToAdd.Add(disease.PatNote);
                                continue;
                            }

                            listStrDiseasesToAdd.Add(strDisease);
                        }

                        formField.ValueString = string.Join(", ", listStrDiseasesToAdd);
                        break;
                    }
                }

                if (formField.DbLink.StartsWith("problem:"))
                {
                    var problemName = formField.DbLink.Substring(8);
                    var hasProblem = false;
                    
                    foreach (var disease in patientDiseases)
                    {
                        var diseaseDef = DiseaseDefs.GetItem(disease.DiseaseDefNum);
                        if (diseaseDef == null)
                        {
                            continue;
                        }

                        var strDiseaseName = diseaseDef.DiseaseName;
                        if (strDiseaseName != problemName) continue;
                        hasProblem = true;
                        break;
                    }

                    if (formField.FieldType == EnumEFormFieldType.CheckBox)
                    {
                        if (hasProblem)
                        {
                            formField.ValueString = "X";
                        }
                    }

                    if (formField.FieldType == EnumEFormFieldType.RadioButtons)
                    {
                        formField.ValueString = hasProblem ? "Y" : "N";
                    }
                }

                if (formField.DbLink == "referredFrom")
                {
                    var referral = Referrals.GetReferralForPat(patient.PatNum);
                    if (referral != null)
                    {
                        formField.ValueString = Referrals.GetNameFL(referral.ReferralNum);
                    }
                }

                if (formField.DbLink == "SSN")
                {
                    if (CultureInfo.CurrentCulture.Name == "en-US" && patient.SSN.Length == 9)
                    {
                        formField.ValueString = patient.SSN.Substring(0, 3) + "-" + patient.SSN.Substring(3, 2) + "-" + patient.SSN.Substring(5, 4);
                    }
                    else
                    {
                        formField.ValueString = patient.SSN;
                    }
                }

                if (formField.DbLink == "State")
                {
                    formField.ValueString = patient.State;
                }

                if (formField.DbLink == "StateNoValidation")
                {
                    formField.ValueString = patient.State;
                }

                if (formField.DbLink == "StudentStatus")
                {
                    formField.ValueString = patient.StudentStatus;
                }

                if (formField.DbLink == "WirelessPhone")
                {
                    formField.ValueString = patient.WirelessPhone;
                }

                if (formField.DbLink == "wirelessCarrier")
                {
                    formField.ValueString = ""; //not implemented
                }

                if (formField.DbLink == "WkPhone")
                {
                    formField.ValueString = patient.WkPhone;
                }

                if (formField.DbLink == "Zip")
                {
                    formField.ValueString = patient.Zip;
                }
            }

            var listEnumStaticTextFields = GetAllStaticTextFieldsForEForm(eForm);
            var staticTextFieldDependency = StaticTextData.GetStaticTextDependencies(listEnumStaticTextFields);
            var listStaticTextReplacements = SheetFiller.GetStaticTextReplacements(listEnumStaticTextFields, patient, family, staticTextData: null, staticTextFieldDependency, aptNum: 0);
            
            ReplaceStaticTextFields(listStaticTextReplacements, eForm, patient, family);
        }

        public static List<EnumStaticTextField> GetAllStaticTextFieldsForEForm(EForm eForm)
        {
            var staticTextFields = new List<EnumStaticTextField>();

            foreach (var formField in eForm.ListEFormFields)
            {
                if (formField.FieldType != EnumEFormFieldType.Label)
                {
                    continue;
                }

                var pattern = @"\[" //beginning square bracket
                              + @"(" //beginning of group
                              + @"\w+" //one or more word characters (letters, digits, or underscores)
                              + @")" //end of group
                              + @"\]"; //ending square bracket

                var regex = new Regex(pattern);
                var matchCollection = regex.Matches(formField.ValueLabel);

                for (var m = 0; m < matchCollection.Count; m++)
                {
                    EnumStaticTextField enumStaticTextField;
                    try
                    {
                        enumStaticTextField = (EnumStaticTextField) Enum.Parse(typeof(EnumStaticTextField), matchCollection[m].Groups[1].Value);
                    }
                    catch
                    {
                        continue;
                    }

                    staticTextFields.Add(enumStaticTextField);
                }
            }

            return staticTextFields;
        }

        public static void ReplaceStaticTextFields(List<StaticTextReplacement> staticTextReplacements, EForm eForm, Patient patient, Family family)
        {
            foreach (var formField in eForm.ListEFormFields)
            {
                if (formField.FieldType != EnumEFormFieldType.Label)
                {
                    continue;
                }

                foreach (var replacement in staticTextReplacements)
                {
                    formField.ValueLabel = formField.ValueLabel.Replace("[" + replacement.StaticTextField + "]", replacement.NewValue);
                }
            }
        }
    }
}