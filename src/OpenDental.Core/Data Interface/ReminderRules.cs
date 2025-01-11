using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ReminderRules
{
    
    public static long Insert(ReminderRule reminderRule)
    {
        return ReminderRuleCrud.Insert(reminderRule);
    }

    
    public static void Update(ReminderRule reminderRule)
    {
        ReminderRuleCrud.Update(reminderRule);
    }

    
    public static void Delete(long reminderRuleNum)
    {
        var command = "DELETE FROM reminderrule WHERE ReminderRuleNum = " + SOut.Long(reminderRuleNum);
        Db.NonQ(command);
    }

    
    public static List<ReminderRule> SelectAll()
    {
        var command = "SELECT * FROM reminderrule";
        return ReminderRuleCrud.SelectMany(command);
    }

    public static List<ReminderRule> GetRemindersForPatient(Patient PatCur)
    {
        //Problem,Medication,Allergy,Age,Gender,LabResult
        var fullListReminders = ReminderRuleCrud.SelectMany("SELECT * FROM reminderrule");
        var retVal = new List<ReminderRule>();
        var listProblems = Diseases.Refresh(PatCur.PatNum);
        var listMedications = Medications.GetMedicationsByPat(PatCur.PatNum);
        var listAllergies = Allergies.Refresh(PatCur.PatNum);
        var listLabResults = LabResults.GetAllForPatient(PatCur.PatNum);
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
                        if (PatCur.Age < int.Parse(fullListReminders[i].CriterionValue.Substring(1, fullListReminders[i].CriterionValue.Length - 1))) retVal.Add(fullListReminders[i]);
                    }
                    else if (fullListReminders[i].CriterionValue[0] == '>')
                    {
                        if (PatCur.Age > int.Parse(fullListReminders[i].CriterionValue.Substring(1, fullListReminders[i].CriterionValue.Length - 1))) retVal.Add(fullListReminders[i]);
                    }

                    //This section should never be reached
                    break;
                case EhrCriterion.Gender:
                    if (PatCur.Gender.ToString().ToLower() == fullListReminders[i].CriterionValue.ToLower()) retVal.Add(fullListReminders[i]);
                    break;
                case EhrCriterion.LabResult:
                    for (var j = 0; j < listLabResults.Count; j++)
                        if (listLabResults[j].TestName.ToLower().Contains(fullListReminders[i].CriterionValue.ToLower()))
                        {
                            retVal.Add(fullListReminders[i]);
                            break;
                        }

                    break;
                //case EhrCriterion.ICD9:
                //  for(int j=0;j<listProblems.Count;j++) {
                //    if(fullListReminders[i].CriterionFK==listProblems[j].DiseaseDefNum) {
                //      retVal.Add(fullListReminders[i]);
                //      break;
                //    }
                //  }
                //  break;
            }

        return retVal;
    }


    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<ReminderRule> Refresh(long patNum){

        string command="SELECT * FROM reminderrule WHERE PatNum = "+POut.Long(patNum);
        return Crud.ReminderRuleCrud.SelectMany(command);
    }

    ///<summary>Gets one ReminderRule from the db.</summary>
    public static ReminderRule GetOne(long reminderRuleNum){

        return Crud.ReminderRuleCrud.SelectOne(reminderRuleNum);
    }


    */
}