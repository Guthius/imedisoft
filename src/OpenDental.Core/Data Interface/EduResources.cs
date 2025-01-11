using System.Collections.Generic;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class EduResources
{
    
    public static List<EduResource> GenerateForPatient(long patNum)
    {
        var listDiseases = Diseases.Refresh(patNum);
        var listMedicationPats = MedicationPats.Refresh(patNum, false);
        var listLabResults = LabResults.GetAllForPatient(patNum);
        var listEhrLabResults = EhrLabResults.GetAllForPatient(patNum);
        var listEduResourcesAll = EduResourceCrud.SelectMany("SELECT * FROM eduresource");
        var listEhrMeasureEventsTobacco = EhrMeasureEvents.RefreshByType(patNum, EhrMeasureEventType.TobaccoUseAssessed)
            .FindAll(x => x.CodeSystemResult == "SNOMEDCT");
        var listEduResourcesRet = new List<EduResource>();
        for (var i = 0; i < listEduResourcesAll.Count; i++)
        {
            if (listEduResourcesAll[i].DiseaseDefNum != 0 && listDiseases.Exists(x => x.DiseaseDefNum == listEduResourcesAll[i].DiseaseDefNum))
            {
                listEduResourcesRet.Add(listEduResourcesAll[i]);
                continue;
            }

            if (listEduResourcesAll[i].MedicationNum != 0 && listMedicationPats.Exists(x => x.MedicationNum == listEduResourcesAll[i].MedicationNum
                                                                                            || (x.MedicationNum == 0 && Medications.GetMedication(listEduResourcesAll[i].MedicationNum).RxCui == x.RxCui)))
            {
                listEduResourcesRet.Add(listEduResourcesAll[i]);
                continue;
            }

            if (listEduResourcesAll[i].SmokingSnoMed != "" && listEhrMeasureEventsTobacco.Exists(x => x.CodeValueResult == listEduResourcesAll[i].SmokingSnoMed)) listEduResourcesRet.Add(listEduResourcesAll[i]);
        }

        for (var i = 0; i < listEduResourcesAll.Count; i++)
        {
            if (listEduResourcesAll[i].LabResultID == "") continue;
            if (listEduResourcesRet.Contains(listEduResourcesAll[i])) continue; //already added from loop above.
            for (var j = 0; j < listLabResults.Count; j++)
            {
                if (listLabResults[j].TestID != listEduResourcesAll[i].LabResultID) continue;
                if (listEduResourcesAll[i].LabResultCompare.StartsWith("<"))
                    //PIn.Int not used because blank not allowed.
                    try
                    {
                        if (int.Parse(listLabResults[j].ObsValue) < int.Parse(listEduResourcesAll[i].LabResultCompare.Substring(1))) listEduResourcesRet.Add(listEduResourcesAll[i]);
                    }
                    catch
                    {
                        //This could only happen if the validation in either input didn't work.
                    }
                else if (listEduResourcesAll[i].LabResultCompare.StartsWith(">"))
                    try
                    {
                        if (int.Parse(listLabResults[j].ObsValue) > int.Parse(listEduResourcesAll[i].LabResultCompare.Substring(1))) listEduResourcesRet.Add(listEduResourcesAll[i]);
                    }
                    catch
                    {
                        //This could only happen if the validation in either input didn't work.
                    }
            } //end listLabResults

            for (var j = 0; j < listEhrLabResults.Count; j++)
            {
                //matches loinc only.
                if (listEhrLabResults[j].ObservationIdentifierID != listEduResourcesAll[i].LabResultID) continue;
                listEduResourcesRet.Add(listEduResourcesAll[i]);
            } //end listEhrLabResults
        } //end listEduResourcesAll

        return listEduResourcesRet;
    }

    
    public static List<EduResource> SelectAll()
    {
        var command = "SELECT * FROM eduresource";
        return EduResourceCrud.SelectMany(command);
    }

    
    public static void Delete(long eduResourceNum)
    {
        var command = "DELETE FROM eduresource WHERE EduResourceNum = " + SOut.Long(eduResourceNum);
        Db.NonQ(command);
    }

    
    public static long Insert(EduResource eduResource)
    {
        return EduResourceCrud.Insert(eduResource);
    }

    
    public static void Update(EduResource eduResource)
    {
        EduResourceCrud.Update(eduResource);
    }

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<EduResource> Refresh(long patNum){

        string command="SELECT * FROM eduresource WHERE PatNum = "+POut.Long(patNum);
        return Crud.EduResourceCrud.SelectMany(command);
    }

    ///<summary>Gets one EduResource from the db.</summary>
    public static EduResource GetOne(long eduResourceNum){

        return Crud.EduResourceCrud.SelectOne(eduResourceNum);
    }



    */
}