using System;
using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Diseases
{
    /// <summary>
    ///     This returns a single disease, but a patient may have multiple instances of the same disease.  For example,
    ///     they may have multiple pregnancy instances with the same DiseaseDefNum.  This will return a single instance of the
    ///     disease, chosen at random by MySQL.  Would be better to use GetDiseasesForPatient below which returns a list of
    ///     diseases with this DiseaseDefNum for the patient.
    /// </summary>
    public static Disease GetSpecificDiseaseForPatient(long patNum, long diseaseDefNum)
    {
        var command = "SELECT * FROM disease WHERE PatNum=" + SOut.Long(patNum)
                                                            + " AND DiseaseDefNum=" + SOut.Long(diseaseDefNum);
        return DiseaseCrud.SelectOne(command);
    }

    /// <summary>
    ///     Gets a list of every disease for the patient that has the specified DiseaseDefNum.  Set showActiveOnly true to
    ///     only show active Diseases based on status (i.e. it could have a stop date but still be active, or marked inactive
    ///     with no stop date).
    /// </summary>
    public static List<Disease> GetDiseasesForPatient(long patNum, long diseaseDefNum, bool showActiveOnly)
    {
        var command = "SELECT * FROM disease WHERE PatNum=" + SOut.Long(patNum)
                                                            + " AND DiseaseDefNum=" + SOut.Long(diseaseDefNum);
        if (showActiveOnly) command += " AND ProbStatus=" + SOut.Int((int) ProblemStatus.Active);

        return DiseaseCrud.SelectMany(command);
    }

    ///<summary>Returns a list of PatNums that have a disease from the PatNums that are passed in.</summary>
    public static List<long> GetPatientsWithDisease(List<long> listPatNums)
    {
        if (listPatNums.Count == 0) return new List<long>();

        var command = "SELECT DISTINCT PatNum FROM disease WHERE PatNum IN (" + string.Join(",", listPatNums) + ") "
                      + "AND disease.DiseaseDefNum != " + SOut.Long(PrefC.GetLong(PrefName.ProblemsIndicateNone));
        return Db.GetListLong(command);
    }

    ///<summary>Gets one disease by DiseaseNum from the db.</summary>
    public static Disease GetOne(long diseaseNum)
    {
        return DiseaseCrud.SelectOne(diseaseNum);
    }

    ///<summary>Gets a list of all Diseases for a given patient.  Includes hidden. Sorted by diseasedef.ItemOrder.</summary>
    public static List<Disease> Refresh(long patNum)
    {
        return Refresh(patNum, false);
    }

    ///<summary>Gets a list of all Diseases for a given patient. Set showActive true to only show active Diseases.</summary>
    public static List<Disease> Refresh(long patNum, bool showActiveOnly)
    {
        var command = "SELECT disease.* FROM disease "
                      + "WHERE PatNum=" + SOut.Long(patNum);
        if (showActiveOnly) command += " AND ProbStatus=" + SOut.Int((int) ProblemStatus.Active);

        return DiseaseCrud.SelectMany(command);
    }

    /// <summary>
    ///     Gets a list of all Diseases for a given patient. Setting includeInactive to true returns all, otherwise only
    ///     resolved and active problems.
    /// </summary>
    public static List<Disease> GetPatientDiseases(long patNum, bool includeInactive)
    {
        var command = "SELECT disease.* FROM disease "
                      + "WHERE PatNum=" + SOut.Long(patNum);
        if (includeInactive) return DiseaseCrud.SelectMany(command);

        command += " AND (ProbStatus=" + SOut.Int((int) ProblemStatus.Active) + " OR ProbStatus=" + SOut.Int((int) ProblemStatus.Resolved) + ")";
        return DiseaseCrud.SelectMany(command);
    }

    
    public static List<Disease> GetPatientData(long patNum)
    {
        var command = "SELECT * FROM disease "
                      + "WHERE PatNum=" + SOut.Long(patNum);
        return DiseaseCrud.SelectMany(command);
    }

    
    public static void Update(Disease disease)
    {
        DiseaseCrud.Update(disease);
    }

    
    public static void Update(Disease disease, Disease diseaseOld)
    {
        DiseaseCrud.Update(disease, diseaseOld);
    }

    
    public static long Insert(Disease disease)
    {
        return DiseaseCrud.Insert(disease);
    }

    
    public static void Delete(Disease disease)
    {
        var command = "DELETE FROM disease WHERE DiseaseNum =" + SOut.Long(disease.DiseaseNum);
        Db.NonQ(command);
    }

    /// <summary>
    ///     Changes the value of the DateTStamp column to the current time stamp for all diseases of a patient that are
    ///     the status specified.
    /// </summary>
    public static void ResetTimeStamps(long patNum, ProblemStatus problemStatus)
    {
        var command = "UPDATE disease SET DateTStamp = CURRENT_TIMESTAMP WHERE PatNum =" + SOut.Long(patNum);
        command += " AND ProbStatus = " + SOut.Int((int) problemStatus);
        Db.NonQ(command);
    }

    /// <summary>
    ///     Checks if disease(problem) can be deleted. If can't delete, throws exception with error containing the number of
    ///     vital signs the disease is attached to and the
    ///     dates of the vital sign exams. If disease can be deleted, does not throw exception and simply returns.
    /// </summary>
    public static void VerifyCanDelete(long diseaseNum)
    {
        var listVitalsigns = Vitalsigns.GetListFromPregDiseaseNum(diseaseNum);
        if (listVitalsigns.Count <= 0) return;

        //if attached to vital sign exam, block delete
        var strDates = "";
        for (var i = 0; i < listVitalsigns.Count; i++)
        {
            if (i > 5) break;

            strDates += "\r\n" + listVitalsigns[i].DateTaken.ToShortDateString();
        }

        throw new Exception("Not allowed to delete this problem. It is attached to " + listVitalsigns.Count + " vital sign exams with dates including: " + strDates + ".");
    }

    /// <summary>
    ///     Checks if disease(problem) can be updated. If can't be updated, throws exception with message containing the dates
    ///     of the preganancy vital sign exams whose dates no longer fall
    ///     within the active dates of the disease. If disease can be updated, does not throw exception and simply returns.
    /// </summary>
    public static void VerifyCanUpdate(Disease disease)
    {
        //See if this problem is the pregnancy linked to a vitalsign exam
        var listVitalsigns = Vitalsigns.GetListFromPregDiseaseNum(disease.DiseaseNum);
        if (listVitalsigns.Count <= 0) return;

        //See if the vitalsign exam date is now outside of the active dates of the disease (pregnancy)
        var strDates = "";
        for (var i = 0; i < listVitalsigns.Count; i++)
            if (listVitalsigns[i].DateTaken < disease.DateStart
                || (disease.DateStop.Year > 1880 && listVitalsigns[i].DateTaken > disease.DateStop))
                strDates += "\r\n" + listVitalsigns[i].DateTaken.ToShortDateString();

        //If vitalsign exam is now outside the dates of the problem, tell the user they must fix the dates of the pregnancy dx
        if (strDates.Length > 0)
            throw new Exception("This problem is attached to 1 or more vital sign exams as a pregnancy diagnosis with dates:" + strDates + "\r\nNot allowed to change the active dates of " +
                                "the diagnosis to be outside the dates of the exam(s).  You must first remove the diagnosis from the vital sign exam(s).");
    }

    /// <summary>
    ///     Takes in vitalsign object and sets its fields to the other passed in arguments. Does not set all of the
    ///     vitalsign object's fields.
    /// </summary>
    public static Disease SetDiseaseFields(Disease disease, DateTime dateStart, DateTime dateStop, ProblemStatus problemStatus, string patNote, SnomedProblemTypes snomedProblemTypes,
        FunctionalStatus functionalStatus)
    {
        disease.DateStart = dateStart;
        disease.DateStop = dateStop;
        disease.ProbStatus = problemStatus;
        disease.PatNote = patNote;
        disease.FunctionStatus = functionalStatus;
        switch (snomedProblemTypes)
        {
            case SnomedProblemTypes.Finding:
                disease.SnomedProblemType = "404684003";
                break;
            case SnomedProblemTypes.Complaint:
                disease.SnomedProblemType = "409586006";
                break;
            case SnomedProblemTypes.Diagnosis:
                disease.SnomedProblemType = "282291009";
                break;
            case SnomedProblemTypes.Condition:
                disease.SnomedProblemType = "64572001";
                break;
            case SnomedProblemTypes.FunctionalLimitation:
                disease.SnomedProblemType = "248536006";
                break;
            case SnomedProblemTypes.Symptom:
                disease.SnomedProblemType = "418799008";
                break;
            case SnomedProblemTypes.Problem:
                disease.SnomedProblemType = "55607006";
                break;
        }

        return disease;
    }
}