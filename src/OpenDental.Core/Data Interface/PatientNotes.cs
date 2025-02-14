using System;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class PatientNotes
{
    public static PatientNote GetPatientData(long patNum, long guarantor)
    {
        return Refresh(patNum, guarantor);
    }

    public static PatientNote Refresh(long patNum, long guarantor)
    {
        var patientNote = GetOne(patNum);
        if (patientNote is null)
        {
            InsertRow(patNum);

            patientNote = new PatientNote
            {
                PatNum = patNum
            };
        }

        PatientNote patientNoteGuarantor;
        if (patNum == guarantor)
        {
            patientNoteGuarantor = patientNote.Copy();
        }
        else
        {
            patientNoteGuarantor = GetOne(guarantor);
            if (patientNoteGuarantor is null)
            {
                InsertRow(guarantor);

                patientNoteGuarantor = new PatientNote
                {
                    PatNum = guarantor
                };
            }
        }

        patientNote.FamFinancial = patientNoteGuarantor.FamFinancial;

        return patientNote;
    }

    public static void Update(PatientNote patientNote, long guarantor)
    {
        PatientNoteCrud.Update(patientNote);

        Db.NonQ("UPDATE patientnote SET FamFinancial = '" + SOut.String(patientNote.FamFinancial) + "'" + " WHERE patnum = " + guarantor);
    }

    private static PatientNote GetOne(long patNum)
    {
        return PatientNoteCrud.SelectOne("SELECT * FROM patientnote WHERE PatNum = " + patNum);
    }

    private static void InsertRow(long patNum)
    {
        try
        {
            Db.NonQ(
                "INSERT INTO patientnote (PatNum, SecDateTEntry) " +
                "VALUES('" + patNum + "', NOW()) " +
                "ON DUPLICATE KEY UPDATE PatNum = " + patNum);
        }
        catch
        {
            //Fail Silently.
        }
    }

    public static void Merge(Patient patFrom, Patient patTo)
    {
        var patNoteFrom = Refresh(patFrom.PatNum, patFrom.Guarantor); //Never returns null.
        var patNoteTo = Refresh(patTo.PatNum, patTo.Guarantor); //Never returns null.
        var strMergeDiv = "\r\n";
        if (!string.IsNullOrEmpty(patNoteTo.FamFinancial)) //FamFinancial
            patNoteTo.FamFinancial += strMergeDiv;
        patNoteTo.FamFinancial += patNoteFrom.FamFinancial;
        //Skip ApptPhone, no longer used as of 4/2007.
        if (!string.IsNullOrEmpty(patNoteTo.Medical)) //Medical
            patNoteTo.Medical += strMergeDiv;
        patNoteTo.Medical += patNoteFrom.Medical;
        if (!string.IsNullOrEmpty(patNoteTo.Service)) //Service
            patNoteTo.Service += strMergeDiv;
        patNoteTo.Service += patNoteFrom.Service;
        if (!string.IsNullOrEmpty(patNoteTo.MedicalComp)) //MedicalComp
            patNoteTo.MedicalComp += strMergeDiv;
        patNoteTo.MedicalComp += patNoteFrom.MedicalComp;
        if (!string.IsNullOrEmpty(patNoteTo.Treatment)) //Treatment
            patNoteTo.Treatment += strMergeDiv;
        patNoteTo.Treatment += patNoteFrom.Treatment;
        if (string.IsNullOrEmpty(patNoteTo.ICEName)) //ICEName, only change if patNotTo was not set.
            patNoteTo.ICEName += patNoteFrom.ICEName;
        if (string.IsNullOrEmpty(patNoteTo.ICEPhone)) //ICEPhone, only change if patNotTo was not set.
            patNoteTo.ICEPhone += patNoteFrom.ICEPhone;
        if (patNoteTo.OrthoMonthsTreatOverride == -1) //OrthoMonthsTreatOverride, only change if patNoteTo was not set.
            patNoteTo.OrthoMonthsTreatOverride = patNoteFrom.OrthoMonthsTreatOverride;
        if (patNoteTo.DateOrthoPlacementOverride != DateTime.MinValue) //DateOrthoPlacementOverride, only change if patNotTo was not set.
            patNoteTo.DateOrthoPlacementOverride = patNoteFrom.DateOrthoPlacementOverride;
        Update(patNoteTo, patTo.Guarantor); //Will cause the guarantor's FamFinancial field to be updated.
    }

    public static long GetUserNumOrthoLocked(long patNum)
    {
        var raw = DataCore.GetScalar("SELECT UserNumOrthoLocked FROM patientnote WHERE PatNum = " + patNum);
        if (raw is not null)
        {
            return SIn.Long(raw);
        }

        InsertRow(patNum);

        return 0;
    }

    public static void SetUserNumOrthoLocked(long patNum, long userNum)
    {
        Db.NonQ("UPDATE patientnote SET UserNumOrthoLocked= " + userNum + " WHERE PatNum = " + patNum);
    }
}