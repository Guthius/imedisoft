using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using Imedisoft.Core.Features.Clinics;
using Imedisoft.Core.Features.Clinics.Dtos;
using OpenDentBusiness.Crud;
using OpenDentBusiness.FileIO;
using OpenDentBusiness.HL7;

namespace OpenDentBusiness;


public class MedLabs
{
    ///<summary>Gets one MedLab from the db.</summary>
    public static MedLab GetOne(long medLabNum)
    {
        return MedLabCrud.SelectOne(medLabNum);
    }

    public static int GetCountForPatient(long patNum)
    {
        var command = "SELECT COUNT(*) FROM medlab WHERE PatNum=" + SOut.Long(patNum);
        return SIn.Int(Db.GetCount(command));
    }

    /// <summary>
    ///     Get unique MedLab orders, grouped by PatNum, ProvNum, and SpecimenID.  Also returns the most recent DateTime the
    ///     results
    ///     were released from the lab and a list of test descriptions ordered.  If includeNoPat==true, the lab orders not
    ///     attached to a patient will be
    ///     included.  Filtered by MedLabs for the list of clinics supplied based on the
    ///     medlab.PatAccountNum=clinic.MedLabAccountNum.  ClinicNum 0 will
    ///     be for those medlabs with PatAccountNum that does not match any of the MedLabAccountNums set for a clinic.
    ///     listSelectedClinics is already
    ///     filtered to only those clinics for which the current user has permission to access based on ClinicIsRestricted.  If
    ///     clinics are not enabled,
    ///     listSelectedClinics will contain 0 and all medlabs will be returned.
    /// </summary>
    public static List<MedLab> GetOrdersForPatient(Patient patient, bool includeNoPat, bool onlyNoPat, DateTime dateReportedStart, DateTime dateReportedEnd,
        List<ClinicDto> listClinicsSelected)
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
                      + "HAVING " + DbHelper.DtimeToDate("MAX(DateTimeReported)") + " BETWEEN " + SOut.Date(dateReportedStart) + " AND " + SOut.Date(dateReportedEnd)
                      + ") maxDate ON maxDate.PatNum=medlab.PatNum AND maxDate.ProvNum=medlab.ProvNum AND maxDate.SpecimenID=medlab.SpecimenID ";
        if (true && listWhereClauseStrs.Count > 0) command += "WHERE (" + string.Join(" OR ", listWhereClauseStrs) + ") ";
        command += "GROUP BY medlab.PatNum,medlab.ProvNum,medlab.SpecimenID "
                   + "ORDER BY maxDate.DateTimeReported DESC,medlab.SpecimenID,MedLabNum"; //most recently received lab on top, with all for a specific specimen together
        return MedLabCrud.SelectMany(command);
    }

    /// <summary>
    ///     Get MedLabs for a specific patient and a specific SpecimenID, SpecimenIDFiller combination.
    ///     Ordered by DateTimeReported descending, MedLabNum descending so the most recently reported/processed message is
    ///     first in the list.
    ///     If using random primary keys, this information may be incorectly ordered, but that is only an annoyance and this
    ///     function should still work.
    /// </summary>
    public static List<MedLab> GetForPatAndSpecimen(long patNum, string specimenID, string specimenIDFiller)
    {
        var command = "SELECT * FROM medlab WHERE PatNum=" + SOut.Long(patNum) + " "
                      + "AND SpecimenID='" + SOut.String(specimenID) + "' "
                      + "AND SpecimenIDFiller='" + SOut.String(specimenIDFiller) + "' "
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

    /// <summary>
    ///     Sets the PatNum column on MedLabs with MedLabNum in list.  Used when manually assigning/moving MedLabs to a
    ///     patient.
    /// </summary>
    public static void UpdateAllPatNums(List<long> listMedLabNums, long patNum)
    {
        if (listMedLabNums.Count < 1) return;
        var command = "UPDATE medlab SET PatNum=" + SOut.Long(patNum) + " WHERE MedLabNum IN(" + string.Join(",", listMedLabNums) + ")";
        Db.NonQ(command);
    }

    /// <summary>
    ///     Reprocess the original HL7 msgs for any MedLabs with PatNum 0, creates the embedded PDF files from the base64 text
    ///     in the ZEF segments
    ///     <para>
    ///         The old method used when parsing MedLab HL7 msgs was to wait to extract these files until the msg was manually
    ///         associated with a patient.
    ///         Associating the MedLabs to a patient and reprocessing the HL7 messages using middle tier was very slow.
    ///     </para>
    ///     <para>
    ///         The new method is to create the PDF files and save them in the image folder in a subdirectory called
    ///         "MedLabEmbeddedFiles" if a patient
    ///         isn't located from the details in the PID segment of the message.  Associating the MedLabs to a pat is now just
    ///         a matter of moving the files to
    ///         the pat's image folder and updating the PatNum columns.  All files are now extracted and stored, either in a
    ///         pat's folder or in the
    ///         "MedLabEmbeddedFiles" folder, by the HL7 service.
    ///     </para>
    ///     <para>
    ///         This will reprocess all HL7 messages for MedLabs with PatNum=0 and replace the MedLab, MedLabResult,
    ///         MedLabSpecimen, and MedLabFacAttach
    ///         rows as well as create any embedded files and insert document table rows.  The document table rows will have
    ///         PatNum=0, just like the MedLabs,
    ///         if a pat is still not located with the details in the PID segment.  Once the user manually attaches the MedLab
    ///         to a patient, all rows will be
    ///         updated with the correct PatNum and the embedded PDFs will be moved to the pat's image folder.  The
    ///         document.FileName column will contain the
    ///         name of the file, regardless of where it is located.  The file name will be updated to a relevant name for the
    ///         folder in which it is located.
    ///         i.e. in the MedLabEmbeddedFiles directory it may be named 3YG8Z420150909100527.pdf, but once moved to a pat's
    ///         folder it will be renamed to
    ///         something like PatientAustin375.pdf and the document.FileName column will be the current name.
    ///     </para>
    ///     <para>
    ///         If storing images in the db, the document table rows will contain the base64 text version of the PDFs with
    ///         PatNum=0 and will be updated
    ///         with the correct PatNum once associated.  The FileName will be just the extension ".pdf" until it is associated
    ///         with a patient at which time it
    ///         will be updated to something like PatientAustin375.pdf.
    ///     </para>
    /// </summary>
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
                if (true) fileText = FileAtoZ.ReadAllText(FileAtoZ.CombinePaths(ImageStore.GetPreferredAtoZpath(), relativePath));
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

    /// <summary>
    ///     Cascading delete that deletes all MedLab, MedLabResult, MedLabSpecimen, and MedLabFacAttach.
    ///     Also deletes any embedded PDFs that are linked to by the MedLabResults.
    ///     The MedLabs and all associated results, specimens, and FacAttaches referenced by the MedLabNums in
    ///     listExcludeMedLabNums will not be deleted.
    ///     Used for deleting old entries and keeping new ones.  The list may be empty and then all will be deleted.
    /// </summary>
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
                ImageStore.DeleteDocuments(new List<Document> {document}, ImageStore.GetPatientFolder(patient, ImageStore.GetPreferredAtoZpath()));
            }
            catch (Exception ex)
            {
                failedCount++;
            }
        }

        return failedCount;
    }

    ///<summary>Translates enum values into human readable strings.</summary>
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

    ///<summary>Delete all of the MedLab objects by MedLabNum.</summary>
    public static void DeleteAll(List<long> listMedLabNums)
    {
        var command = "DELETE FROM medlab WHERE MedLabNum IN(" + string.Join(",", listMedLabNums) + ")";
        Db.NonQ(command);
    }
}