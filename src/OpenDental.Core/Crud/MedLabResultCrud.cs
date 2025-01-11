#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class MedLabResultCrud
{
    public static MedLabResult SelectOne(long medLabResultNum)
    {
        var command = "SELECT * FROM medlabresult "
                      + "WHERE MedLabResultNum = " + SOut.Long(medLabResultNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static MedLabResult SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<MedLabResult> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MedLabResult> TableToList(DataTable table)
    {
        var retVal = new List<MedLabResult>();
        MedLabResult medLabResult;
        foreach (DataRow row in table.Rows)
        {
            medLabResult = new MedLabResult();
            medLabResult.MedLabResultNum = SIn.Long(row["MedLabResultNum"].ToString());
            medLabResult.MedLabNum = SIn.Long(row["MedLabNum"].ToString());
            medLabResult.ObsID = SIn.String(row["ObsID"].ToString());
            medLabResult.ObsText = SIn.String(row["ObsText"].ToString());
            medLabResult.ObsLoinc = SIn.String(row["ObsLoinc"].ToString());
            medLabResult.ObsLoincText = SIn.String(row["ObsLoincText"].ToString());
            medLabResult.ObsIDSub = SIn.String(row["ObsIDSub"].ToString());
            medLabResult.ObsValue = SIn.String(row["ObsValue"].ToString());
            var obsSubType = row["ObsSubType"].ToString();
            if (obsSubType == "")
                medLabResult.ObsSubType = 0;
            else
                try
                {
                    medLabResult.ObsSubType = (DataSubtype) Enum.Parse(typeof(DataSubtype), obsSubType);
                }
                catch
                {
                    medLabResult.ObsSubType = 0;
                }

            medLabResult.ObsUnits = SIn.String(row["ObsUnits"].ToString());
            medLabResult.ReferenceRange = SIn.String(row["ReferenceRange"].ToString());
            var abnormalFlag = row["AbnormalFlag"].ToString();
            if (abnormalFlag == "")
                medLabResult.AbnormalFlag = 0;
            else
                try
                {
                    medLabResult.AbnormalFlag = (AbnormalFlag) Enum.Parse(typeof(AbnormalFlag), abnormalFlag);
                }
                catch
                {
                    medLabResult.AbnormalFlag = 0;
                }

            var resultStatus = row["ResultStatus"].ToString();
            if (resultStatus == "")
                medLabResult.ResultStatus = 0;
            else
                try
                {
                    medLabResult.ResultStatus = (ResultStatus) Enum.Parse(typeof(ResultStatus), resultStatus);
                }
                catch
                {
                    medLabResult.ResultStatus = 0;
                }

            medLabResult.DateTimeObs = SIn.DateTime(row["DateTimeObs"].ToString());
            medLabResult.FacilityID = SIn.String(row["FacilityID"].ToString());
            medLabResult.DocNum = SIn.Long(row["DocNum"].ToString());
            medLabResult.Note = SIn.String(row["Note"].ToString());
            retVal.Add(medLabResult);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<MedLabResult> listMedLabResults, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "MedLabResult";
        var table = new DataTable(tableName);
        table.Columns.Add("MedLabResultNum");
        table.Columns.Add("MedLabNum");
        table.Columns.Add("ObsID");
        table.Columns.Add("ObsText");
        table.Columns.Add("ObsLoinc");
        table.Columns.Add("ObsLoincText");
        table.Columns.Add("ObsIDSub");
        table.Columns.Add("ObsValue");
        table.Columns.Add("ObsSubType");
        table.Columns.Add("ObsUnits");
        table.Columns.Add("ReferenceRange");
        table.Columns.Add("AbnormalFlag");
        table.Columns.Add("ResultStatus");
        table.Columns.Add("DateTimeObs");
        table.Columns.Add("FacilityID");
        table.Columns.Add("DocNum");
        table.Columns.Add("Note");
        foreach (var medLabResult in listMedLabResults)
            table.Rows.Add(SOut.Long(medLabResult.MedLabResultNum), SOut.Long(medLabResult.MedLabNum), medLabResult.ObsID, medLabResult.ObsText, medLabResult.ObsLoinc, medLabResult.ObsLoincText, medLabResult.ObsIDSub, medLabResult.ObsValue, SOut.Int((int) medLabResult.ObsSubType), medLabResult.ObsUnits, medLabResult.ReferenceRange, SOut.Int((int) medLabResult.AbnormalFlag), SOut.Int((int) medLabResult.ResultStatus), SOut.DateT(medLabResult.DateTimeObs, false), medLabResult.FacilityID, SOut.Long(medLabResult.DocNum), medLabResult.Note);
        return table;
    }

    public static long Insert(MedLabResult medLabResult)
    {
        return Insert(medLabResult, false);
    }

    public static long Insert(MedLabResult medLabResult, bool useExistingPK)
    {
        var command = "INSERT INTO medlabresult (";

        command += "MedLabNum,ObsID,ObsText,ObsLoinc,ObsLoincText,ObsIDSub,ObsValue,ObsSubType,ObsUnits,ReferenceRange,AbnormalFlag,ResultStatus,DateTimeObs,FacilityID,DocNum,Note) VALUES(";

        command +=
            SOut.Long(medLabResult.MedLabNum) + ","
                                              + "'" + SOut.String(medLabResult.ObsID) + "',"
                                              + "'" + SOut.String(medLabResult.ObsText) + "',"
                                              + "'" + SOut.String(medLabResult.ObsLoinc) + "',"
                                              + "'" + SOut.String(medLabResult.ObsLoincText) + "',"
                                              + "'" + SOut.String(medLabResult.ObsIDSub) + "',"
                                              + DbHelper.ParamChar + "paramObsValue,"
                                              + "'" + SOut.String(medLabResult.ObsSubType.ToString()) + "',"
                                              + "'" + SOut.String(medLabResult.ObsUnits) + "',"
                                              + "'" + SOut.String(medLabResult.ReferenceRange) + "',"
                                              + "'" + SOut.String(medLabResult.AbnormalFlag.ToString()) + "',"
                                              + "'" + SOut.String(medLabResult.ResultStatus.ToString()) + "',"
                                              + SOut.DateT(medLabResult.DateTimeObs) + ","
                                              + "'" + SOut.String(medLabResult.FacilityID) + "',"
                                              + SOut.Long(medLabResult.DocNum) + ","
                                              + DbHelper.ParamChar + "paramNote)";
        if (medLabResult.ObsValue == null) medLabResult.ObsValue = "";
        var paramObsValue = new OdSqlParameter("paramObsValue", OdDbType.Text, SOut.StringParam(medLabResult.ObsValue));
        if (medLabResult.Note == null) medLabResult.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(medLabResult.Note));
        {
            medLabResult.MedLabResultNum = Db.NonQ(command, true, "MedLabResultNum", "medLabResult", paramObsValue, paramNote);
        }
        return medLabResult.MedLabResultNum;
    }

    public static long InsertNoCache(MedLabResult medLabResult)
    {
        return InsertNoCache(medLabResult, false);
    }

    public static long InsertNoCache(MedLabResult medLabResult, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO medlabresult (";
        if (isRandomKeys || useExistingPK) command += "MedLabResultNum,";
        command += "MedLabNum,ObsID,ObsText,ObsLoinc,ObsLoincText,ObsIDSub,ObsValue,ObsSubType,ObsUnits,ReferenceRange,AbnormalFlag,ResultStatus,DateTimeObs,FacilityID,DocNum,Note) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(medLabResult.MedLabResultNum) + ",";
        command +=
            SOut.Long(medLabResult.MedLabNum) + ","
                                              + "'" + SOut.String(medLabResult.ObsID) + "',"
                                              + "'" + SOut.String(medLabResult.ObsText) + "',"
                                              + "'" + SOut.String(medLabResult.ObsLoinc) + "',"
                                              + "'" + SOut.String(medLabResult.ObsLoincText) + "',"
                                              + "'" + SOut.String(medLabResult.ObsIDSub) + "',"
                                              + DbHelper.ParamChar + "paramObsValue,"
                                              + "'" + SOut.String(medLabResult.ObsSubType.ToString()) + "',"
                                              + "'" + SOut.String(medLabResult.ObsUnits) + "',"
                                              + "'" + SOut.String(medLabResult.ReferenceRange) + "',"
                                              + "'" + SOut.String(medLabResult.AbnormalFlag.ToString()) + "',"
                                              + "'" + SOut.String(medLabResult.ResultStatus.ToString()) + "',"
                                              + SOut.DateT(medLabResult.DateTimeObs) + ","
                                              + "'" + SOut.String(medLabResult.FacilityID) + "',"
                                              + SOut.Long(medLabResult.DocNum) + ","
                                              + DbHelper.ParamChar + "paramNote)";
        if (medLabResult.ObsValue == null) medLabResult.ObsValue = "";
        var paramObsValue = new OdSqlParameter("paramObsValue", OdDbType.Text, SOut.StringParam(medLabResult.ObsValue));
        if (medLabResult.Note == null) medLabResult.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(medLabResult.Note));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramObsValue, paramNote);
        else
            medLabResult.MedLabResultNum = Db.NonQ(command, true, "MedLabResultNum", "medLabResult", paramObsValue, paramNote);
        return medLabResult.MedLabResultNum;
    }

    public static void Update(MedLabResult medLabResult)
    {
        var command = "UPDATE medlabresult SET "
                      + "MedLabNum      =  " + SOut.Long(medLabResult.MedLabNum) + ", "
                      + "ObsID          = '" + SOut.String(medLabResult.ObsID) + "', "
                      + "ObsText        = '" + SOut.String(medLabResult.ObsText) + "', "
                      + "ObsLoinc       = '" + SOut.String(medLabResult.ObsLoinc) + "', "
                      + "ObsLoincText   = '" + SOut.String(medLabResult.ObsLoincText) + "', "
                      + "ObsIDSub       = '" + SOut.String(medLabResult.ObsIDSub) + "', "
                      + "ObsValue       =  " + DbHelper.ParamChar + "paramObsValue, "
                      + "ObsSubType     = '" + SOut.String(medLabResult.ObsSubType.ToString()) + "', "
                      + "ObsUnits       = '" + SOut.String(medLabResult.ObsUnits) + "', "
                      + "ReferenceRange = '" + SOut.String(medLabResult.ReferenceRange) + "', "
                      + "AbnormalFlag   = '" + SOut.String(medLabResult.AbnormalFlag.ToString()) + "', "
                      + "ResultStatus   = '" + SOut.String(medLabResult.ResultStatus.ToString()) + "', "
                      + "DateTimeObs    =  " + SOut.DateT(medLabResult.DateTimeObs) + ", "
                      + "FacilityID     = '" + SOut.String(medLabResult.FacilityID) + "', "
                      + "DocNum         =  " + SOut.Long(medLabResult.DocNum) + ", "
                      + "Note           =  " + DbHelper.ParamChar + "paramNote "
                      + "WHERE MedLabResultNum = " + SOut.Long(medLabResult.MedLabResultNum);
        if (medLabResult.ObsValue == null) medLabResult.ObsValue = "";
        var paramObsValue = new OdSqlParameter("paramObsValue", OdDbType.Text, SOut.StringParam(medLabResult.ObsValue));
        if (medLabResult.Note == null) medLabResult.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(medLabResult.Note));
        Db.NonQ(command, paramObsValue, paramNote);
    }

    public static bool Update(MedLabResult medLabResult, MedLabResult oldMedLabResult)
    {
        var command = "";
        if (medLabResult.MedLabNum != oldMedLabResult.MedLabNum)
        {
            if (command != "") command += ",";
            command += "MedLabNum = " + SOut.Long(medLabResult.MedLabNum) + "";
        }

        if (medLabResult.ObsID != oldMedLabResult.ObsID)
        {
            if (command != "") command += ",";
            command += "ObsID = '" + SOut.String(medLabResult.ObsID) + "'";
        }

        if (medLabResult.ObsText != oldMedLabResult.ObsText)
        {
            if (command != "") command += ",";
            command += "ObsText = '" + SOut.String(medLabResult.ObsText) + "'";
        }

        if (medLabResult.ObsLoinc != oldMedLabResult.ObsLoinc)
        {
            if (command != "") command += ",";
            command += "ObsLoinc = '" + SOut.String(medLabResult.ObsLoinc) + "'";
        }

        if (medLabResult.ObsLoincText != oldMedLabResult.ObsLoincText)
        {
            if (command != "") command += ",";
            command += "ObsLoincText = '" + SOut.String(medLabResult.ObsLoincText) + "'";
        }

        if (medLabResult.ObsIDSub != oldMedLabResult.ObsIDSub)
        {
            if (command != "") command += ",";
            command += "ObsIDSub = '" + SOut.String(medLabResult.ObsIDSub) + "'";
        }

        if (medLabResult.ObsValue != oldMedLabResult.ObsValue)
        {
            if (command != "") command += ",";
            command += "ObsValue = " + DbHelper.ParamChar + "paramObsValue";
        }

        if (medLabResult.ObsSubType != oldMedLabResult.ObsSubType)
        {
            if (command != "") command += ",";
            command += "ObsSubType = '" + SOut.String(medLabResult.ObsSubType.ToString()) + "'";
        }

        if (medLabResult.ObsUnits != oldMedLabResult.ObsUnits)
        {
            if (command != "") command += ",";
            command += "ObsUnits = '" + SOut.String(medLabResult.ObsUnits) + "'";
        }

        if (medLabResult.ReferenceRange != oldMedLabResult.ReferenceRange)
        {
            if (command != "") command += ",";
            command += "ReferenceRange = '" + SOut.String(medLabResult.ReferenceRange) + "'";
        }

        if (medLabResult.AbnormalFlag != oldMedLabResult.AbnormalFlag)
        {
            if (command != "") command += ",";
            command += "AbnormalFlag = '" + SOut.String(medLabResult.AbnormalFlag.ToString()) + "'";
        }

        if (medLabResult.ResultStatus != oldMedLabResult.ResultStatus)
        {
            if (command != "") command += ",";
            command += "ResultStatus = '" + SOut.String(medLabResult.ResultStatus.ToString()) + "'";
        }

        if (medLabResult.DateTimeObs != oldMedLabResult.DateTimeObs)
        {
            if (command != "") command += ",";
            command += "DateTimeObs = " + SOut.DateT(medLabResult.DateTimeObs) + "";
        }

        if (medLabResult.FacilityID != oldMedLabResult.FacilityID)
        {
            if (command != "") command += ",";
            command += "FacilityID = '" + SOut.String(medLabResult.FacilityID) + "'";
        }

        if (medLabResult.DocNum != oldMedLabResult.DocNum)
        {
            if (command != "") command += ",";
            command += "DocNum = " + SOut.Long(medLabResult.DocNum) + "";
        }

        if (medLabResult.Note != oldMedLabResult.Note)
        {
            if (command != "") command += ",";
            command += "Note = " + DbHelper.ParamChar + "paramNote";
        }

        if (command == "") return false;
        if (medLabResult.ObsValue == null) medLabResult.ObsValue = "";
        var paramObsValue = new OdSqlParameter("paramObsValue", OdDbType.Text, SOut.StringParam(medLabResult.ObsValue));
        if (medLabResult.Note == null) medLabResult.Note = "";
        var paramNote = new OdSqlParameter("paramNote", OdDbType.Text, SOut.StringParam(medLabResult.Note));
        command = "UPDATE medlabresult SET " + command
                                             + " WHERE MedLabResultNum = " + SOut.Long(medLabResult.MedLabResultNum);
        Db.NonQ(command, paramObsValue, paramNote);
        return true;
    }

    public static bool UpdateComparison(MedLabResult medLabResult, MedLabResult oldMedLabResult)
    {
        if (medLabResult.MedLabNum != oldMedLabResult.MedLabNum) return true;
        if (medLabResult.ObsID != oldMedLabResult.ObsID) return true;
        if (medLabResult.ObsText != oldMedLabResult.ObsText) return true;
        if (medLabResult.ObsLoinc != oldMedLabResult.ObsLoinc) return true;
        if (medLabResult.ObsLoincText != oldMedLabResult.ObsLoincText) return true;
        if (medLabResult.ObsIDSub != oldMedLabResult.ObsIDSub) return true;
        if (medLabResult.ObsValue != oldMedLabResult.ObsValue) return true;
        if (medLabResult.ObsSubType != oldMedLabResult.ObsSubType) return true;
        if (medLabResult.ObsUnits != oldMedLabResult.ObsUnits) return true;
        if (medLabResult.ReferenceRange != oldMedLabResult.ReferenceRange) return true;
        if (medLabResult.AbnormalFlag != oldMedLabResult.AbnormalFlag) return true;
        if (medLabResult.ResultStatus != oldMedLabResult.ResultStatus) return true;
        if (medLabResult.DateTimeObs != oldMedLabResult.DateTimeObs) return true;
        if (medLabResult.FacilityID != oldMedLabResult.FacilityID) return true;
        if (medLabResult.DocNum != oldMedLabResult.DocNum) return true;
        if (medLabResult.Note != oldMedLabResult.Note) return true;
        return false;
    }

    public static void Delete(long medLabResultNum)
    {
        var command = "DELETE FROM medlabresult "
                      + "WHERE MedLabResultNum = " + SOut.Long(medLabResultNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listMedLabResultNums)
    {
        if (listMedLabResultNums == null || listMedLabResultNums.Count == 0) return;
        var command = "DELETE FROM medlabresult "
                      + "WHERE MedLabResultNum IN(" + string.Join(",", listMedLabResultNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}