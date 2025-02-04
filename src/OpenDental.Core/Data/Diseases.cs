using System;
using System.Collections.Generic;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class Diseases
{
    public static Disease GetSpecificDiseaseForPatient(long patNum, long diseaseDefNum)
    {
        return DiseaseCrud.SelectOne("SELECT * FROM disease WHERE PatNum = " + patNum + " AND DiseaseDefNum = " + diseaseDefNum);
    }

    public static List<Disease> GetDiseasesForPatient(long patNum, long diseaseDefNum, bool activeOnly)
    {
        var commandText = "SELECT * FROM disease WHERE PatNum = " + patNum + " AND DiseaseDefNum = " + diseaseDefNum;

        if (activeOnly)
        {
            commandText += " AND ProbStatus = " + (int) ProblemStatus.Active;
        }

        return DiseaseCrud.SelectMany(commandText);
    }

    public static List<long> GetPatientsWithDisease(List<long> patNums)
    {
        return patNums.Count == 0
            ? []
            : Db.GetListLong(
                "SELECT DISTINCT PatNum FROM disease WHERE PatNum IN (" + string.Join(",", patNums) + ") AND disease.DiseaseDefNum != " + PrefC.GetLong(PrefName.ProblemsIndicateNone));
    }

    public static Disease GetOne(long diseaseNum)
    {
        return DiseaseCrud.SelectOne(diseaseNum);
    }

    public static List<Disease> Refresh(long patNum, bool activeOnly = false)
    {
        var commandText = "SELECT disease.* FROM disease WHERE PatNum = " + patNum;

        if (activeOnly)
        {
            commandText += " AND ProbStatus = " + (int) ProblemStatus.Active;
        }

        return DiseaseCrud.SelectMany(commandText);
    }

    public static List<Disease> GetPatientDiseases(long patNum, bool includeInactive)
    {
        var commandText = "SELECT disease.* FROM disease WHERE PatNum = " + patNum;

        if (includeInactive)
        {
            return DiseaseCrud.SelectMany(commandText);
        }

        commandText += " AND (ProbStatus = " + (int) ProblemStatus.Active + " OR ProbStatus = " + (int) ProblemStatus.Resolved + ")";

        return DiseaseCrud.SelectMany(commandText);
    }

    public static List<Disease> GetPatientData(long patNum)
    {
        return DiseaseCrud.SelectMany("SELECT * FROM disease WHERE PatNum = " + patNum);
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
        Db.NonQ("DELETE FROM disease WHERE DiseaseNum = " + disease.DiseaseNum);
    }

    public static Disease SetDiseaseFields(Disease disease, DateTime dateStart, DateTime dateStop, ProblemStatus problemStatus, string patNote, SnomedProblemTypes snomedProblemTypes, FunctionalStatus functionalStatus)
    {
        disease.DateStart = dateStart;
        disease.DateStop = dateStop;
        disease.ProbStatus = problemStatus;
        disease.PatNote = patNote;
        disease.FunctionStatus = functionalStatus;
        disease.SnomedProblemType = snomedProblemTypes switch
        {
            SnomedProblemTypes.Finding => "404684003",
            SnomedProblemTypes.Complaint => "409586006",
            SnomedProblemTypes.Diagnosis => "282291009",
            SnomedProblemTypes.Condition => "64572001",
            SnomedProblemTypes.FunctionalLimitation => "248536006",
            SnomedProblemTypes.Symptom => "418799008",
            SnomedProblemTypes.Problem => "55607006",
            _ => disease.SnomedProblemType
        };

        return disease;
    }
}