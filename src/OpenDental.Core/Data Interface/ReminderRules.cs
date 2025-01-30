using System.Collections.Generic;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class ReminderRules
{
    public static void Insert(ReminderRule reminderRule)
    {
        ReminderRuleCrud.Insert(reminderRule);
    }

    public static void Update(ReminderRule reminderRule)
    {
        ReminderRuleCrud.Update(reminderRule);
    }

    public static void Delete(long reminderRuleNum)
    {
        Db.NonQ("DELETE FROM reminderrule WHERE ReminderRuleNum = " + reminderRuleNum);
    }

    public static List<ReminderRule> SelectAll()
    {
        return ReminderRuleCrud.SelectMany("SELECT * FROM reminderrule");
    }

    public static List<ReminderRule> GetRemindersForPatient(Patient patient)
    {
        var fullListReminders = ReminderRuleCrud.SelectMany("SELECT * FROM reminderrule");
        var retVal = new List<ReminderRule>();
        var listProblems = Diseases.Refresh(patient.PatNum);
        var listMedications = Medications.GetMedicationsByPat(patient.PatNum);
        var listAllergies = Allergies.Refresh(patient.PatNum);
        for (var i = 0; i < fullListReminders.Count; i++)
            switch (fullListReminders[i].ReminderCriterion)
            {
                case EhrCriterion.Problem:
                    for (var j = 0; j < listProblems.Count; j++)
                        if (fullListReminders[i].CriterionFK == listProblems[j].DiseaseDefNum)
                        {
                            retVal.Add(fullListReminders[i]);
                            break;
                        }

                    break;
                case EhrCriterion.Medication:
                    for (var j = 0; j < listMedications.Count; j++)
                        if (fullListReminders[i].CriterionFK == listMedications[j].MedicationNum)
                        {
                            retVal.Add(fullListReminders[i]);
                            break;
                        }

                    break;
                case EhrCriterion.Allergy:
                    for (var j = 0; j < listAllergies.Count; j++)
                        if (fullListReminders[i].CriterionFK == listAllergies[j].AllergyDefNum)
                        {
                            retVal.Add(fullListReminders[i]);
                            break;
                        }

                    break;
                case EhrCriterion.Age:
                    if (fullListReminders[i].CriterionValue[0] == '<')
                    {
                        if (patient.Age < int.Parse(fullListReminders[i].CriterionValue.Substring(1, fullListReminders[i].CriterionValue.Length - 1))) retVal.Add(fullListReminders[i]);
                    }
                    else if (fullListReminders[i].CriterionValue[0] == '>')
                    {
                        if (patient.Age > int.Parse(fullListReminders[i].CriterionValue.Substring(1, fullListReminders[i].CriterionValue.Length - 1))) retVal.Add(fullListReminders[i]);
                    }

                    //This section should never be reached
                    break;
                case EhrCriterion.Gender:
                    if (patient.Gender.ToString().ToLower() == fullListReminders[i].CriterionValue.ToLower()) retVal.Add(fullListReminders[i]);
                    break;
            }

        return retVal;
    }
}