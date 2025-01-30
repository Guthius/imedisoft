using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Data;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using OpenDentBusiness.FileIO;
using OpenDentBusiness.HL7;

namespace OpenDentBusiness;

public class MedLabs
{
    public static MedLab GetOne(long medLabNum)
    {
        return MedLabCrud.SelectOne(medLabNum);
    }

    public static int GetCountForPatient(long patNum)
    {
        var command = "SELECT COUNT(*) FROM medlab WHERE PatNum=" + SOut.Long(patNum);
        return SIn.Int(Db.GetCount(command));
    }

    public static List<MedLab> GetOrdersForPatient(Patient patient, bool includeNoPat, bool onlyNoPat, DateTime dateReportedStart, DateTime dateReportedEnd, List<ClinicDto> listClinicsSelected)
    {
        //include all patients unless a patient is specified.
        var patNumClause = "medlab.PatNum>0";
        if (patient != null) patNumClause = "medlab.PatNum=" + SOut.Long(patient.PatNum);
        //do not include patnum=0 unless specified.
        if (includeNoPat) patNumClause += " OR medlab.PatNum=0";
        if (onlyNoPat) patNumClause = "medlab.PatNum=0";
        var listWhereClauseStrs = new List<string>();
        if (true)
        {
            var listAllClinicAcctNums = Clinics.GetWhere(x => !string.IsNullOrWhiteSpace(x.MedLabAccountNumber)).Select(x => x.MedLabAccountNumber).ToList();
            if (listClinicsSelected.Any(x => x.Id == 0) && listAllClinicAcctNums.Count > 0) //include "Unassigned" medlabs
                listWhereClauseStrs.Add("medlab.PatAccountNum NOT IN (" + string.Join(",", listAllClinicAcctNums) + ")");
            listClinicsSelected.RemoveAll(x => x.Id <= 0 || string.IsNullOrWhiteSpace(x.MedLabAccountNumber));
            if (listClinicsSelected.Count > 0) listWhereClauseStrs.Add("medlab.PatAccountNum IN (" + string.Join(",", listClinicsSelected.Select(x => x.MedLabAccountNumber)) + ")");
        }

        var command = "SELECT MAX(CASE WHEN medlab.DateTimeReported=maxDate.DateTimeReported THEN MedLabNum ELSE 0 END) AS MedLabNum,"
                      + "SendingApp,SendingFacility,medlab.PatNum,medlab.ProvNum,PatIDLab,PatIDAlt,PatAge,PatAccountNum,PatFasting,medlab.SpecimenID,"
                      + "SpecimenIDFiller,ObsTestID,ObsTestLoinc,ObsTestLoincText,DateTimeCollected,TotalVolume,ActionCode,ClinicalInfo,"
                      + "MIN(DateTimeEntered) AS DateTimeEntered,OrderingProvNPI,OrderingProvLocalID,OrderingProvLName,OrderingProvFName,SpecimenIDAlt,"
                      + "maxDate.DateTimeReported,MIN(CASE WHEN medlab.DateTimeReported=maxDate.DateTimeReported THEN ResultStatus ELSE NULL END) AS ResultStatus,"
                      + "ParentObsID,ParentObsTestID,NotePat,NoteLab,FileName,"
                      + "MIN(CASE WHEN medlab.DateTimeReported=maxDate.DateTimeReported THEN OriginalPIDSegment ELSE NULL END) AS OriginalPIDSegment,"
                      + DbHelper.GroupConcat("ObsTestDescript", true, separator: "\r\n") + " AS ObsTestDescript "
                      + "FROM medlab "
                      + "INNER JOIN ("
                      + "SELECT PatNum,ProvNum,SpecimenID,MAX(DateTimeReported) AS DateTimeReported "
                      + "FROM medlab "
                      + "WHERE (" + patNumClause + ") " //Ex: WHERE (medlab.PatNum>0 OR medlab.Patnum=0)
                      + "GROUP BY PatNum,ProvNum,SpecimenID "
                      + "HAVING DATE(MAX(DateTimeReported)) BETWEEN " + SOut.Date(dateReportedStart) + " AND " + SOut.Date(dateReportedEnd)
                      + ") maxDate ON maxDate.PatNum=medlab.PatNum AND maxDate.ProvNum=medlab.ProvNum AND maxDate.SpecimenID=medlab.SpecimenID ";
        if (listWhereClauseStrs.Count > 0) command += "WHERE (" + string.Join(" OR ", listWhereClauseStrs) + ") ";
        command += "GROUP BY medlab.PatNum,medlab.ProvNum,medlab.SpecimenID "
                   + "ORDER BY maxDate.DateTimeReported DESC,medlab.SpecimenID,MedLabNum"; //most recently received lab on top, with all for a specific specimen together
        return MedLabCrud.SelectMany(command);
    }

    public static List<MedLab> GetForPatAndSpecimen(long patNum, string specimenId, string specimenIdFiller)
    {
        var command = "SELECT * FROM medlab WHERE PatNum=" + SOut.Long(patNum) + " "
                      + "AND SpecimenID='" + SOut.String(specimenId) + "' "
                      + "AND SpecimenIDFiller='" + SOut.String(specimenIdFiller) + "' "
                      + "ORDER BY DateTimeReported DESC,MedLabNum DESC";
        return MedLabCrud.SelectMany(command);
    }

    public static void UpdateFileNames(List<long> listMedLabNums, string fileNameNew)
    {
        var command = "UPDATE medlab SET FileName='" + SOut.String(fileNameNew) + "' WHERE MedLabNum IN(" + string.Join(",", listMedLabNums) + ")";
        Db.NonQ(command);
    }

    public static long Insert(MedLab medLab)
    {
        return MedLabCrud.Insert(medLab);
    }

    public static void Update(MedLab medLab)
    {
        MedLabCrud.Update(medLab);
    }

    public static void UpdateAllPatNums(List<long> listMedLabNums, long patNum)
    {
        if (listMedLabNums.Count < 1) return;
        var command = "UPDATE medlab SET PatNum=" + SOut.Long(patNum) + " WHERE MedLabNum IN(" + string.Join(",", listMedLabNums) + ")";
        Db.NonQ(command);
    }

    public static int Reconcile()
    {
        var command = "SELECT * FROM medlab WHERE PatNum=0";
        var listMedLabs = MedLabCrud.SelectMany(command);
        if (listMedLabs.Count < 1) return 0;
        var listMedLabNumsNew = new List<long>(); //used to delete old MedLab objects after creating these new ones from the HL7 message text
        var failedCount = 0;
        foreach (var relativePath in listMedLabs.Select(x => x.FileName).Distinct().ToList())
        {
            var fileText = "";
            try
            {
                if (true) fileText = File.ReadAllText(Path.Combine(ImageStore.GetDataFolder(), relativePath));
            }
            catch (Exception ex)
            {
                failedCount++;
                continue;
            }

            var messageHL7 = new MessageHL7(fileText);
            var listMedLabNums = MessageParserMedLab.Process(messageHL7, relativePath, false); //re-creates the documents from the ZEF segments
            if (listMedLabNums == null || listMedLabNums.Count < 1)
            {
                failedCount++;
                continue; //not sure what to do, just move on?
            }

            listMedLabNumsNew.AddRange(listMedLabNums);
            UpdateFileNames(listMedLabNums, relativePath);
        }

        //Delete all MedLabs, MedLabResults, MedLabSpecimens, and MedLabFacAttaches except the ones just created
        //Don't delete until we successfully process the messages and have valid new MedLab objects
        foreach (var medLab in listMedLabs) failedCount += DeleteLabsAndResults(medLab, listMedLabNumsNew);
        return failedCount;
    }

    public static int DeleteLabsAndResults(MedLab medLab, List<long> listExcludeMedLabNums = null)
    {
        var listMedLabsOld = GetForPatAndSpecimen(medLab.PatNum, medLab.SpecimenID, medLab.SpecimenIDFiller); //patNum could be 0
        if (listExcludeMedLabNums != null) listMedLabsOld = listMedLabsOld.FindAll(x => !listExcludeMedLabNums.Contains(x.MedLabNum));
        if (listMedLabsOld.Count < 1) return 0;
        var failedCount = 0;
        var listLabNumsOld = listMedLabsOld.Select(x => x.MedLabNum).ToList();
        var listMedLabResultsOld = listMedLabsOld.SelectMany(x => x.ListMedLabResults).ToList(); //sends one query to the db per MedLab
        MedLabFacAttaches.DeleteAllForLabsOrResults(listLabNumsOld, listMedLabResultsOld.Select(x => x.MedLabResultNum).ToList());
        MedLabSpecimens.DeleteAllForLabs(listLabNumsOld); //MedLabSpecimens have a FK to MedLabNum
        MedLabResults.DeleteAllForMedLabs(listLabNumsOld); //MedLabResults have a FK to MedLabNum
        DeleteAll(listLabNumsOld);
        foreach (var document in Documents.GetByNums(listMedLabResultsOld.Select(x => x.DocNum).ToList()))
        {
            var patient = Patients.GetPat(document.PatNum);
            if (patient == null)
            {
                Documents.Delete(document);
                continue;
            }

            try
            {
                ImageStore.DeleteDocuments(new List<Document> {document}, ImageStore.GetPatientFolder(patient, ImageStore.GetDataFolder()));
            }
            catch (Exception ex)
            {
                failedCount++;
            }
        }

        return failedCount;
    }

    public static string GetStatusDescript(ResultStatus resultStatus)
    {
        switch (resultStatus)
        {
            case ResultStatus.C:
                return "Corrected";
            case ResultStatus.F:
                return "Final";
            case ResultStatus.I:
                return "Incomplete";
            case ResultStatus.P:
                return "Preliminary";
            case ResultStatus.X:
                return "Canceled";
            default:
                return "";
        }
    }

    public static void DeleteAll(List<long> listMedLabNums)
    {
        var command = "DELETE FROM medlab WHERE MedLabNum IN(" + string.Join(",", listMedLabNums) + ")";
        Db.NonQ(command);
    }
}