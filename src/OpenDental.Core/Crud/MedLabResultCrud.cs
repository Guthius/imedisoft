using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class MedLabResultCrud
{
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

    public static long Insert(MedLabResult medLabResult)
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
                                              + SOut.DateTime(medLabResult.DateTimeObs) + ","
                                              + "'" + SOut.String(medLabResult.FacilityID) + "',"
                                              + SOut.Long(medLabResult.DocNum) + ","
                                              + DbHelper.ParamChar + "paramNote)";
        if (medLabResult.ObsValue == null) medLabResult.ObsValue = "";
        var paramObsValue = new OdSqlParameter("paramObsValue", SOut.StringParam(medLabResult.ObsValue));
        if (medLabResult.Note == null) medLabResult.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(medLabResult.Note));
        {
            medLabResult.MedLabResultNum = Db.NonQ(command, true, "MedLabResultNum", "medLabResult", paramObsValue, paramNote);
        }
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
                      + "DateTimeObs    =  " + SOut.DateTime(medLabResult.DateTimeObs) + ", "
                      + "FacilityID     = '" + SOut.String(medLabResult.FacilityID) + "', "
                      + "DocNum         =  " + SOut.Long(medLabResult.DocNum) + ", "
                      + "Note           =  " + DbHelper.ParamChar + "paramNote "
                      + "WHERE MedLabResultNum = " + SOut.Long(medLabResult.MedLabResultNum);
        if (medLabResult.ObsValue == null) medLabResult.ObsValue = "";
        var paramObsValue = new OdSqlParameter("paramObsValue", SOut.StringParam(medLabResult.ObsValue));
        if (medLabResult.Note == null) medLabResult.Note = "";
        var paramNote = new OdSqlParameter("paramNote", SOut.StringParam(medLabResult.Note));
        Db.NonQ(command, paramObsValue, paramNote);
    }
}