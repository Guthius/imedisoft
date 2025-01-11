using System;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class PatientNotes
{
    
    public static PatientNote GetPatientData(long patNum, long guarantor)
    {
        return Refresh(patNum, guarantor);
    }

    /// <summary>
    ///     Gets the PatientNote for the patient passed in.
    ///     Inserts a row into the database for the patient AND for the guarantor passed in if one does not exist for either.
    ///     The PatientNote returned always has the guarantor's FamFinancial value which should always override all family
    ///     member's value.
    /// </summary>
    public static PatientNote Refresh(long patNum, long guarantor)
    {
        //RemotingRole check is needed here even though this method does not run methods, it does however call multiple private methods.

        var patientNote = GetOne(patNum);
        if (patientNote == null)
        {
            InsertRow(patNum);
            //Do NOT go back to the database to get the newly inserted row because there could be replication delay for larger customers.
            //Instead, just fill in the patientNote object with default values.
            patientNote = new PatientNote();
            patientNote.PatNum = patNum;
        }

        PatientNote patientNoteGuarantor;
        //Check to see if the patient passed in IS the guarantor.
        if (patNum == guarantor)
        {
            //Do NOT try and insert yet another row for the guarantor if the guarantor IS the patient that we just inserted a new row for.
            //Make a deep copy of the current patientNote instead.
            patientNoteGuarantor = patientNote.Copy();
        }
        else
        {
            //Guarantor is a different patient than the patNum passed in.
            patientNoteGuarantor = GetOne(guarantor);
            if (patientNoteGuarantor == null)
            {
                InsertRow(guarantor);
                //Do NOT go back to the database to get the newly inserted row because there could be replication delay for larger customers.
                //Instead, just fill in the patientNote object with default values.
                patientNoteGuarantor = new PatientNote();
                patientNoteGuarantor.PatNum = guarantor;
            }
        }

        //Always override the family memeber's FamFinancial value with that of the guarantors (old behavior).
        patientNote.FamFinancial = patientNoteGuarantor.FamFinancial;
        return patientNote;
    }

    
    public static void Update(PatientNote Cur, long guarantor)
    {
        PatientNoteCrud.Update(Cur); //FamFinancial gets skipped
        var command = "UPDATE patientnote SET "
                      + "FamFinancial = '" + SOut.String(Cur.FamFinancial) + "'"
                      + " WHERE patnum = '" + SOut.Long(guarantor) + "'";
        Db.NonQ(command);
    }

    
    public static void Update(PatientNote patientNote, PatientNote patientNoteOld, long guarantor)
    {
        PatientNoteCrud.Update(patientNote, patientNoteOld); //FamFinancial gets skipped
        var command = "UPDATE patientnote SET "
                      + "FamFinancial = '" + SOut.String(patientNote.FamFinancial) + "'"
                      + " WHERE patnum = '" + SOut.Long(guarantor) + "'";
        Db.NonQ(command);
    }

    /// <summary>
    ///     Gets the PatientNote for the patient passed in.  The FamFinancial note could be incorrect.
    ///     Users should call Refresh() to get the correct PatientNote for the patient and guarantor combo.
    /// </summary>
    private static PatientNote GetOne(long patNum)
    {
        var command = "SELECT * FROM patientnote WHERE PatNum = " + SOut.Long(patNum);
        return PatientNoteCrud.SelectOne(command);
    }

    
    private static void InsertRow(long patNum)
    {
        //Random keys not necessary to check because of 1:1 patNum.
        //However, this is a lazy insert, so multiple locations might attempt it.
        //Just in case, we will have it fail silently.
        try
        {
            var command = "INSERT INTO patientnote (PatNum,SecDateTEntry) VALUES('" + patNum + "'," + DbHelper.Now() + ")";
            //We may need to do this in Oracle in the future as well.
            //If using Replication, then we need to watch for duplicate errors, because the insert is lazy.
            //Replication servers can insert a patient note with a primary key belonging to another replication server's key range.
            command += " ON DUPLICATE KEY UPDATE PatNum='" + patNum + "'";
            Db.NonQ(command);
        }
        catch
        {
            //Fail Silently.
        }
    }

    /// <summary>
    ///     Merge the PatientNote for patFrom into the PatientNote for patTo.  Appends to FamFinancial, Medical, Service,
    ///     MedicalComp, Treatment.
    ///     Overwrites ICEName, ICEPhone, OrthoMonthsTreatOverride, DateOrthoPlacementOverride, but only if those fields are
    ///     not already set for patTo.
    /// </summary>
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

    /// <summary>
    ///     Returns 0 if not locked. This gets hit every 4 seconds by every open ortho chart, but that's a pretty low
    ///     load.
    /// </summary>
    public static long GetUserNumOrthoLocked(long patNum)
    {
        var command = "SELECT UserNumOrthoLocked FROM patientnote "
                      + " WHERE PatNum = '" + SOut.Long(patNum) + "'";
        var raw = DataCore.GetScalar(command);
        if (raw is null)
        {
            InsertRow(patNum);
            return 0;
        }

        return SIn.Long(raw);
    }

    
    public static void SetUserNumOrthoLocked(long patNum, long userNum)
    {
        var command = "UPDATE patientnote "
                      + " SET UserNumOrthoLocked= '" + SOut.Long(userNum) + "'"
                      + " WHERE PatNum = '" + SOut.Long(patNum) + "'";
        Db.NonQ(command);
    }
}