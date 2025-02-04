using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PatientNoteCrud
{
    public static PatientNote SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PatientNote> TableToList(DataTable table)
    {
        var retVal = new List<PatientNote>();
        foreach (DataRow row in table.Rows)
        {
            var patientNote = new PatientNote
            {
                PatNum = SIn.Long(row["PatNum"].ToString()),
                FamFinancial = SIn.String(row["FamFinancial"].ToString()),
                ApptPhone = SIn.String(row["ApptPhone"].ToString()),
                Medical = SIn.String(row["Medical"].ToString()),
                Service = SIn.String(row["Service"].ToString()),
                MedicalComp = SIn.String(row["MedicalComp"].ToString()),
                Treatment = SIn.String(row["Treatment"].ToString()),
                ICEName = SIn.String(row["ICEName"].ToString()),
                ICEPhone = SIn.String(row["ICEPhone"].ToString()),
                OrthoMonthsTreatOverride = SIn.Int(row["OrthoMonthsTreatOverride"].ToString()),
                DateOrthoPlacementOverride = SIn.Date(row["DateOrthoPlacementOverride"].ToString()),
                SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString()),
                SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString()),
                Consent = (PatConsentFlags) SIn.Int(row["Consent"].ToString()),
                UserNumOrthoLocked = SIn.Long(row["UserNumOrthoLocked"].ToString()),
                Pronoun = (PronounPreferred) SIn.Int(row["Pronoun"].ToString())
            };
            retVal.Add(patientNote);
        }

        return retVal;
    }

    public static void Update(PatientNote patientNote)
    {
        var command = "UPDATE patientnote SET "
                      //FamFinancial excluded from update
                      + "ApptPhone                 =  " + DbHelper.ParamChar + "paramApptPhone, "
                      + "Medical                   =  " + DbHelper.ParamChar + "paramMedical, "
                      + "Service                   =  " + DbHelper.ParamChar + "paramService, "
                      + "MedicalComp               =  " + DbHelper.ParamChar + "paramMedicalComp, "
                      + "Treatment                 =  " + DbHelper.ParamChar + "paramTreatment, "
                      + "ICEName                   = '" + SOut.String(patientNote.ICEName) + "', "
                      + "ICEPhone                  = '" + SOut.String(patientNote.ICEPhone) + "', "
                      + "OrthoMonthsTreatOverride  =  " + SOut.Int(patientNote.OrthoMonthsTreatOverride) + ", "
                      + "DateOrthoPlacementOverride=  " + SOut.Date(patientNote.DateOrthoPlacementOverride) + ", "
                      //SecDateTEntry not allowed to change
                      //SecDateTEdit can only be set by MySQL
                      + "Consent                   =  " + SOut.Int((int) patientNote.Consent) + ", "
                      + "UserNumOrthoLocked        =  " + SOut.Long(patientNote.UserNumOrthoLocked) + ", "
                      + "Pronoun                   =  " + SOut.Int((int) patientNote.Pronoun) + " "
                      + "WHERE PatNum = " + SOut.Long(patientNote.PatNum);
        if (patientNote.FamFinancial == null) patientNote.FamFinancial = "";
        var paramFamFinancial = new OdSqlParameter("paramFamFinancial", SOut.StringNote(patientNote.FamFinancial));
        if (patientNote.ApptPhone == null) patientNote.ApptPhone = "";
        var paramApptPhone = new OdSqlParameter("paramApptPhone", SOut.StringParam(patientNote.ApptPhone));
        if (patientNote.Medical == null) patientNote.Medical = "";
        var paramMedical = new OdSqlParameter("paramMedical", SOut.StringNote(patientNote.Medical));
        if (patientNote.Service == null) patientNote.Service = "";
        var paramService = new OdSqlParameter("paramService", SOut.StringNote(patientNote.Service));
        if (patientNote.MedicalComp == null) patientNote.MedicalComp = "";
        var paramMedicalComp = new OdSqlParameter("paramMedicalComp", SOut.StringNote(patientNote.MedicalComp));
        if (patientNote.Treatment == null) patientNote.Treatment = "";
        var paramTreatment = new OdSqlParameter("paramTreatment", SOut.StringNote(patientNote.Treatment));
        Db.NonQ(command, paramFamFinancial, paramApptPhone, paramMedical, paramService, paramMedicalComp, paramTreatment);
    }
}