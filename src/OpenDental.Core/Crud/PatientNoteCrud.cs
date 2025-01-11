#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class PatientNoteCrud
{
    public static PatientNote SelectOne(long patNum)
    {
        var command = "SELECT * FROM patientnote "
                      + "WHERE PatNum = " + SOut.Long(patNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static PatientNote SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<PatientNote> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<PatientNote> TableToList(DataTable table)
    {
        var retVal = new List<PatientNote>();
        PatientNote patientNote;
        foreach (DataRow row in table.Rows)
        {
            patientNote = new PatientNote();
            patientNote.PatNum = SIn.Long(row["PatNum"].ToString());
            patientNote.FamFinancial = SIn.String(row["FamFinancial"].ToString());
            patientNote.ApptPhone = SIn.String(row["ApptPhone"].ToString());
            patientNote.Medical = SIn.String(row["Medical"].ToString());
            patientNote.Service = SIn.String(row["Service"].ToString());
            patientNote.MedicalComp = SIn.String(row["MedicalComp"].ToString());
            patientNote.Treatment = SIn.String(row["Treatment"].ToString());
            patientNote.ICEName = SIn.String(row["ICEName"].ToString());
            patientNote.ICEPhone = SIn.String(row["ICEPhone"].ToString());
            patientNote.OrthoMonthsTreatOverride = SIn.Int(row["OrthoMonthsTreatOverride"].ToString());
            patientNote.DateOrthoPlacementOverride = SIn.Date(row["DateOrthoPlacementOverride"].ToString());
            patientNote.SecDateTEntry = SIn.DateTime(row["SecDateTEntry"].ToString());
            patientNote.SecDateTEdit = SIn.DateTime(row["SecDateTEdit"].ToString());
            patientNote.Consent = (PatConsentFlags) SIn.Int(row["Consent"].ToString());
            patientNote.UserNumOrthoLocked = SIn.Long(row["UserNumOrthoLocked"].ToString());
            patientNote.Pronoun = (PronounPreferred) SIn.Int(row["Pronoun"].ToString());
            retVal.Add(patientNote);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<PatientNote> listPatientNotes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "PatientNote";
        var table = new DataTable(tableName);
        table.Columns.Add("PatNum");
        table.Columns.Add("FamFinancial");
        table.Columns.Add("ApptPhone");
        table.Columns.Add("Medical");
        table.Columns.Add("Service");
        table.Columns.Add("MedicalComp");
        table.Columns.Add("Treatment");
        table.Columns.Add("ICEName");
        table.Columns.Add("ICEPhone");
        table.Columns.Add("OrthoMonthsTreatOverride");
        table.Columns.Add("DateOrthoPlacementOverride");
        table.Columns.Add("SecDateTEntry");
        table.Columns.Add("SecDateTEdit");
        table.Columns.Add("Consent");
        table.Columns.Add("UserNumOrthoLocked");
        table.Columns.Add("Pronoun");
        foreach (var patientNote in listPatientNotes)
            table.Rows.Add(SOut.Long(patientNote.PatNum), patientNote.FamFinancial, patientNote.ApptPhone, patientNote.Medical, patientNote.Service, patientNote.MedicalComp, patientNote.Treatment, patientNote.ICEName, patientNote.ICEPhone, SOut.Int(patientNote.OrthoMonthsTreatOverride), SOut.DateT(patientNote.DateOrthoPlacementOverride, false), SOut.DateT(patientNote.SecDateTEntry, false), SOut.DateT(patientNote.SecDateTEdit, false), SOut.Int((int) patientNote.Consent), SOut.Long(patientNote.UserNumOrthoLocked), SOut.Int((int) patientNote.Pronoun));
        return table;
    }

    public static long Insert(PatientNote patientNote)
    {
        return Insert(patientNote, false);
    }

    public static long Insert(PatientNote patientNote, bool useExistingPK)
    {
        var command = "INSERT INTO patientnote (";

        command += "FamFinancial,ApptPhone,Medical,Service,MedicalComp,Treatment,ICEName,ICEPhone,OrthoMonthsTreatOverride,DateOrthoPlacementOverride,SecDateTEntry,Consent,UserNumOrthoLocked,Pronoun) VALUES(";

        command +=
            DbHelper.ParamChar + "paramFamFinancial,"
                               + DbHelper.ParamChar + "paramApptPhone,"
                               + DbHelper.ParamChar + "paramMedical,"
                               + DbHelper.ParamChar + "paramService,"
                               + DbHelper.ParamChar + "paramMedicalComp,"
                               + DbHelper.ParamChar + "paramTreatment,"
                               + "'" + SOut.String(patientNote.ICEName) + "',"
                               + "'" + SOut.String(patientNote.ICEPhone) + "',"
                               + SOut.Int(patientNote.OrthoMonthsTreatOverride) + ","
                               + SOut.Date(patientNote.DateOrthoPlacementOverride) + ","
                               + DbHelper.Now() + ","
                               //SecDateTEdit can only be set by MySQL
                               + SOut.Int((int) patientNote.Consent) + ","
                               + SOut.Long(patientNote.UserNumOrthoLocked) + ","
                               + SOut.Int((int) patientNote.Pronoun) + ")";
        if (patientNote.FamFinancial == null) patientNote.FamFinancial = "";
        var paramFamFinancial = new OdSqlParameter("paramFamFinancial", OdDbType.Text, SOut.StringNote(patientNote.FamFinancial));
        if (patientNote.ApptPhone == null) patientNote.ApptPhone = "";
        var paramApptPhone = new OdSqlParameter("paramApptPhone", OdDbType.Text, SOut.StringParam(patientNote.ApptPhone));
        if (patientNote.Medical == null) patientNote.Medical = "";
        var paramMedical = new OdSqlParameter("paramMedical", OdDbType.Text, SOut.StringNote(patientNote.Medical));
        if (patientNote.Service == null) patientNote.Service = "";
        var paramService = new OdSqlParameter("paramService", OdDbType.Text, SOut.StringNote(patientNote.Service));
        if (patientNote.MedicalComp == null) patientNote.MedicalComp = "";
        var paramMedicalComp = new OdSqlParameter("paramMedicalComp", OdDbType.Text, SOut.StringNote(patientNote.MedicalComp));
        if (patientNote.Treatment == null) patientNote.Treatment = "";
        var paramTreatment = new OdSqlParameter("paramTreatment", OdDbType.Text, SOut.StringNote(patientNote.Treatment));
        {
            patientNote.PatNum = Db.NonQ(command, true, "PatNum", "patientNote", paramFamFinancial, paramApptPhone, paramMedical, paramService, paramMedicalComp, paramTreatment);
        }
        return patientNote.PatNum;
    }

    public static long InsertNoCache(PatientNote patientNote)
    {
        return InsertNoCache(patientNote, false);
    }

    public static long InsertNoCache(PatientNote patientNote, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO patientnote (";
        if (isRandomKeys || useExistingPK) command += "PatNum,";
        command += "FamFinancial,ApptPhone,Medical,Service,MedicalComp,Treatment,ICEName,ICEPhone,OrthoMonthsTreatOverride,DateOrthoPlacementOverride,SecDateTEntry,Consent,UserNumOrthoLocked,Pronoun) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(patientNote.PatNum) + ",";
        command +=
            DbHelper.ParamChar + "paramFamFinancial,"
                               + DbHelper.ParamChar + "paramApptPhone,"
                               + DbHelper.ParamChar + "paramMedical,"
                               + DbHelper.ParamChar + "paramService,"
                               + DbHelper.ParamChar + "paramMedicalComp,"
                               + DbHelper.ParamChar + "paramTreatment,"
                               + "'" + SOut.String(patientNote.ICEName) + "',"
                               + "'" + SOut.String(patientNote.ICEPhone) + "',"
                               + SOut.Int(patientNote.OrthoMonthsTreatOverride) + ","
                               + SOut.Date(patientNote.DateOrthoPlacementOverride) + ","
                               + DbHelper.Now() + ","
                               //SecDateTEdit can only be set by MySQL
                               + SOut.Int((int) patientNote.Consent) + ","
                               + SOut.Long(patientNote.UserNumOrthoLocked) + ","
                               + SOut.Int((int) patientNote.Pronoun) + ")";
        if (patientNote.FamFinancial == null) patientNote.FamFinancial = "";
        var paramFamFinancial = new OdSqlParameter("paramFamFinancial", OdDbType.Text, SOut.StringNote(patientNote.FamFinancial));
        if (patientNote.ApptPhone == null) patientNote.ApptPhone = "";
        var paramApptPhone = new OdSqlParameter("paramApptPhone", OdDbType.Text, SOut.StringParam(patientNote.ApptPhone));
        if (patientNote.Medical == null) patientNote.Medical = "";
        var paramMedical = new OdSqlParameter("paramMedical", OdDbType.Text, SOut.StringNote(patientNote.Medical));
        if (patientNote.Service == null) patientNote.Service = "";
        var paramService = new OdSqlParameter("paramService", OdDbType.Text, SOut.StringNote(patientNote.Service));
        if (patientNote.MedicalComp == null) patientNote.MedicalComp = "";
        var paramMedicalComp = new OdSqlParameter("paramMedicalComp", OdDbType.Text, SOut.StringNote(patientNote.MedicalComp));
        if (patientNote.Treatment == null) patientNote.Treatment = "";
        var paramTreatment = new OdSqlParameter("paramTreatment", OdDbType.Text, SOut.StringNote(patientNote.Treatment));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramFamFinancial, paramApptPhone, paramMedical, paramService, paramMedicalComp, paramTreatment);
        else
            patientNote.PatNum = Db.NonQ(command, true, "PatNum", "patientNote", paramFamFinancial, paramApptPhone, paramMedical, paramService, paramMedicalComp, paramTreatment);
        return patientNote.PatNum;
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
        var paramFamFinancial = new OdSqlParameter("paramFamFinancial", OdDbType.Text, SOut.StringNote(patientNote.FamFinancial));
        if (patientNote.ApptPhone == null) patientNote.ApptPhone = "";
        var paramApptPhone = new OdSqlParameter("paramApptPhone", OdDbType.Text, SOut.StringParam(patientNote.ApptPhone));
        if (patientNote.Medical == null) patientNote.Medical = "";
        var paramMedical = new OdSqlParameter("paramMedical", OdDbType.Text, SOut.StringNote(patientNote.Medical));
        if (patientNote.Service == null) patientNote.Service = "";
        var paramService = new OdSqlParameter("paramService", OdDbType.Text, SOut.StringNote(patientNote.Service));
        if (patientNote.MedicalComp == null) patientNote.MedicalComp = "";
        var paramMedicalComp = new OdSqlParameter("paramMedicalComp", OdDbType.Text, SOut.StringNote(patientNote.MedicalComp));
        if (patientNote.Treatment == null) patientNote.Treatment = "";
        var paramTreatment = new OdSqlParameter("paramTreatment", OdDbType.Text, SOut.StringNote(patientNote.Treatment));
        Db.NonQ(command, paramFamFinancial, paramApptPhone, paramMedical, paramService, paramMedicalComp, paramTreatment);
    }

    public static bool Update(PatientNote patientNote, PatientNote oldPatientNote)
    {
        var command = "";
        //FamFinancial excluded from update
        if (patientNote.ApptPhone != oldPatientNote.ApptPhone)
        {
            if (command != "") command += ",";
            command += "ApptPhone = " + DbHelper.ParamChar + "paramApptPhone";
        }

        if (patientNote.Medical != oldPatientNote.Medical)
        {
            if (command != "") command += ",";
            command += "Medical = " + DbHelper.ParamChar + "paramMedical";
        }

        if (patientNote.Service != oldPatientNote.Service)
        {
            if (command != "") command += ",";
            command += "Service = " + DbHelper.ParamChar + "paramService";
        }

        if (patientNote.MedicalComp != oldPatientNote.MedicalComp)
        {
            if (command != "") command += ",";
            command += "MedicalComp = " + DbHelper.ParamChar + "paramMedicalComp";
        }

        if (patientNote.Treatment != oldPatientNote.Treatment)
        {
            if (command != "") command += ",";
            command += "Treatment = " + DbHelper.ParamChar + "paramTreatment";
        }

        if (patientNote.ICEName != oldPatientNote.ICEName)
        {
            if (command != "") command += ",";
            command += "ICEName = '" + SOut.String(patientNote.ICEName) + "'";
        }

        if (patientNote.ICEPhone != oldPatientNote.ICEPhone)
        {
            if (command != "") command += ",";
            command += "ICEPhone = '" + SOut.String(patientNote.ICEPhone) + "'";
        }

        if (patientNote.OrthoMonthsTreatOverride != oldPatientNote.OrthoMonthsTreatOverride)
        {
            if (command != "") command += ",";
            command += "OrthoMonthsTreatOverride = " + SOut.Int(patientNote.OrthoMonthsTreatOverride) + "";
        }

        if (patientNote.DateOrthoPlacementOverride.Date != oldPatientNote.DateOrthoPlacementOverride.Date)
        {
            if (command != "") command += ",";
            command += "DateOrthoPlacementOverride = " + SOut.Date(patientNote.DateOrthoPlacementOverride) + "";
        }

        //SecDateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (patientNote.Consent != oldPatientNote.Consent)
        {
            if (command != "") command += ",";
            command += "Consent = " + SOut.Int((int) patientNote.Consent) + "";
        }

        if (patientNote.UserNumOrthoLocked != oldPatientNote.UserNumOrthoLocked)
        {
            if (command != "") command += ",";
            command += "UserNumOrthoLocked = " + SOut.Long(patientNote.UserNumOrthoLocked) + "";
        }

        if (patientNote.Pronoun != oldPatientNote.Pronoun)
        {
            if (command != "") command += ",";
            command += "Pronoun = " + SOut.Int((int) patientNote.Pronoun) + "";
        }

        if (command == "") return false;
        if (patientNote.FamFinancial == null) patientNote.FamFinancial = "";
        var paramFamFinancial = new OdSqlParameter("paramFamFinancial", OdDbType.Text, SOut.StringNote(patientNote.FamFinancial));
        if (patientNote.ApptPhone == null) patientNote.ApptPhone = "";
        var paramApptPhone = new OdSqlParameter("paramApptPhone", OdDbType.Text, SOut.StringParam(patientNote.ApptPhone));
        if (patientNote.Medical == null) patientNote.Medical = "";
        var paramMedical = new OdSqlParameter("paramMedical", OdDbType.Text, SOut.StringNote(patientNote.Medical));
        if (patientNote.Service == null) patientNote.Service = "";
        var paramService = new OdSqlParameter("paramService", OdDbType.Text, SOut.StringNote(patientNote.Service));
        if (patientNote.MedicalComp == null) patientNote.MedicalComp = "";
        var paramMedicalComp = new OdSqlParameter("paramMedicalComp", OdDbType.Text, SOut.StringNote(patientNote.MedicalComp));
        if (patientNote.Treatment == null) patientNote.Treatment = "";
        var paramTreatment = new OdSqlParameter("paramTreatment", OdDbType.Text, SOut.StringNote(patientNote.Treatment));
        command = "UPDATE patientnote SET " + command
                                            + " WHERE PatNum = " + SOut.Long(patientNote.PatNum);
        Db.NonQ(command, paramFamFinancial, paramApptPhone, paramMedical, paramService, paramMedicalComp, paramTreatment);
        return true;
    }

    public static bool UpdateComparison(PatientNote patientNote, PatientNote oldPatientNote)
    {
        //FamFinancial excluded from update
        if (patientNote.ApptPhone != oldPatientNote.ApptPhone) return true;
        if (patientNote.Medical != oldPatientNote.Medical) return true;
        if (patientNote.Service != oldPatientNote.Service) return true;
        if (patientNote.MedicalComp != oldPatientNote.MedicalComp) return true;
        if (patientNote.Treatment != oldPatientNote.Treatment) return true;
        if (patientNote.ICEName != oldPatientNote.ICEName) return true;
        if (patientNote.ICEPhone != oldPatientNote.ICEPhone) return true;
        if (patientNote.OrthoMonthsTreatOverride != oldPatientNote.OrthoMonthsTreatOverride) return true;
        if (patientNote.DateOrthoPlacementOverride.Date != oldPatientNote.DateOrthoPlacementOverride.Date) return true;
        //SecDateTEntry not allowed to change
        //SecDateTEdit can only be set by MySQL
        if (patientNote.Consent != oldPatientNote.Consent) return true;
        if (patientNote.UserNumOrthoLocked != oldPatientNote.UserNumOrthoLocked) return true;
        if (patientNote.Pronoun != oldPatientNote.Pronoun) return true;
        return false;
    }

    public static void Delete(long patNum)
    {
        var command = "DELETE FROM patientnote "
                      + "WHERE PatNum = " + SOut.Long(patNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listPatNums)
    {
        if (listPatNums == null || listPatNums.Count == 0) return;
        var command = "DELETE FROM patientnote "
                      + "WHERE PatNum IN(" + string.Join(",", listPatNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}