#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class MedLabCrud
{
    public static MedLab SelectOne(long medLabNum)
    {
        var command = "SELECT * FROM medlab "
                      + "WHERE MedLabNum = " + SOut.Long(medLabNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static MedLab SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<MedLab> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<MedLab> TableToList(DataTable table)
    {
        var retVal = new List<MedLab>();
        MedLab medLab;
        foreach (DataRow row in table.Rows)
        {
            medLab = new MedLab();
            medLab.MedLabNum = SIn.Long(row["MedLabNum"].ToString());
            medLab.ProvNum = SIn.Long(row["ProvNum"].ToString());
            medLab.SendingApp = SIn.String(row["SendingApp"].ToString());
            medLab.SendingFacility = SIn.String(row["SendingFacility"].ToString());
            medLab.PatNum = SIn.Long(row["PatNum"].ToString());
            medLab.PatIDLab = SIn.String(row["PatIDLab"].ToString());
            medLab.PatIDAlt = SIn.String(row["PatIDAlt"].ToString());
            medLab.PatAge = SIn.String(row["PatAge"].ToString());
            medLab.PatAccountNum = SIn.String(row["PatAccountNum"].ToString());
            medLab.PatFasting = (YN) SIn.Int(row["PatFasting"].ToString());
            medLab.SpecimenID = SIn.String(row["SpecimenID"].ToString());
            medLab.SpecimenIDFiller = SIn.String(row["SpecimenIDFiller"].ToString());
            medLab.ObsTestID = SIn.String(row["ObsTestID"].ToString());
            medLab.ObsTestDescript = SIn.String(row["ObsTestDescript"].ToString());
            medLab.ObsTestLoinc = SIn.String(row["ObsTestLoinc"].ToString());
            medLab.ObsTestLoincText = SIn.String(row["ObsTestLoincText"].ToString());
            medLab.DateTimeCollected = SIn.DateTime(row["DateTimeCollected"].ToString());
            medLab.TotalVolume = SIn.String(row["TotalVolume"].ToString());
            var actionCode = row["ActionCode"].ToString();
            if (actionCode == "")
                medLab.ActionCode = 0;
            else
                try
                {
                    medLab.ActionCode = (ResultAction) Enum.Parse(typeof(ResultAction), actionCode);
                }
                catch
                {
                    medLab.ActionCode = 0;
                }

            medLab.ClinicalInfo = SIn.String(row["ClinicalInfo"].ToString());
            medLab.DateTimeEntered = SIn.DateTime(row["DateTimeEntered"].ToString());
            medLab.OrderingProvNPI = SIn.String(row["OrderingProvNPI"].ToString());
            medLab.OrderingProvLocalID = SIn.String(row["OrderingProvLocalID"].ToString());
            medLab.OrderingProvLName = SIn.String(row["OrderingProvLName"].ToString());
            medLab.OrderingProvFName = SIn.String(row["OrderingProvFName"].ToString());
            medLab.SpecimenIDAlt = SIn.String(row["SpecimenIDAlt"].ToString());
            medLab.DateTimeReported = SIn.DateTime(row["DateTimeReported"].ToString());
            var resultStatus = row["ResultStatus"].ToString();
            if (resultStatus == "")
                medLab.ResultStatus = 0;
            else
                try
                {
                    medLab.ResultStatus = (ResultStatus) Enum.Parse(typeof(ResultStatus), resultStatus);
                }
                catch
                {
                    medLab.ResultStatus = 0;
                }

            medLab.ParentObsID = SIn.String(row["ParentObsID"].ToString());
            medLab.ParentObsTestID = SIn.String(row["ParentObsTestID"].ToString());
            medLab.NotePat = SIn.String(row["NotePat"].ToString());
            medLab.NoteLab = SIn.String(row["NoteLab"].ToString());
            medLab.FileName = SIn.String(row["FileName"].ToString());
            medLab.OriginalPIDSegment = SIn.String(row["OriginalPIDSegment"].ToString());
            retVal.Add(medLab);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<MedLab> listMedLabs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "MedLab";
        var table = new DataTable(tableName);
        table.Columns.Add("MedLabNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("SendingApp");
        table.Columns.Add("SendingFacility");
        table.Columns.Add("PatNum");
        table.Columns.Add("PatIDLab");
        table.Columns.Add("PatIDAlt");
        table.Columns.Add("PatAge");
        table.Columns.Add("PatAccountNum");
        table.Columns.Add("PatFasting");
        table.Columns.Add("SpecimenID");
        table.Columns.Add("SpecimenIDFiller");
        table.Columns.Add("ObsTestID");
        table.Columns.Add("ObsTestDescript");
        table.Columns.Add("ObsTestLoinc");
        table.Columns.Add("ObsTestLoincText");
        table.Columns.Add("DateTimeCollected");
        table.Columns.Add("TotalVolume");
        table.Columns.Add("ActionCode");
        table.Columns.Add("ClinicalInfo");
        table.Columns.Add("DateTimeEntered");
        table.Columns.Add("OrderingProvNPI");
        table.Columns.Add("OrderingProvLocalID");
        table.Columns.Add("OrderingProvLName");
        table.Columns.Add("OrderingProvFName");
        table.Columns.Add("SpecimenIDAlt");
        table.Columns.Add("DateTimeReported");
        table.Columns.Add("ResultStatus");
        table.Columns.Add("ParentObsID");
        table.Columns.Add("ParentObsTestID");
        table.Columns.Add("NotePat");
        table.Columns.Add("NoteLab");
        table.Columns.Add("FileName");
        table.Columns.Add("OriginalPIDSegment");
        foreach (var medLab in listMedLabs)
            table.Rows.Add(SOut.Long(medLab.MedLabNum), SOut.Long(medLab.ProvNum), medLab.SendingApp, medLab.SendingFacility, SOut.Long(medLab.PatNum), medLab.PatIDLab, medLab.PatIDAlt, medLab.PatAge, medLab.PatAccountNum, SOut.Int((int) medLab.PatFasting), medLab.SpecimenID, medLab.SpecimenIDFiller, medLab.ObsTestID, medLab.ObsTestDescript, medLab.ObsTestLoinc, medLab.ObsTestLoincText, SOut.DateT(medLab.DateTimeCollected, false), medLab.TotalVolume, SOut.Int((int) medLab.ActionCode), medLab.ClinicalInfo, SOut.DateT(medLab.DateTimeEntered, false), medLab.OrderingProvNPI, medLab.OrderingProvLocalID, medLab.OrderingProvLName, medLab.OrderingProvFName, medLab.SpecimenIDAlt, SOut.DateT(medLab.DateTimeReported, false), SOut.Int((int) medLab.ResultStatus), medLab.ParentObsID, medLab.ParentObsTestID, medLab.NotePat, medLab.NoteLab, medLab.FileName, medLab.OriginalPIDSegment);
        return table;
    }

    public static long Insert(MedLab medLab)
    {
        return Insert(medLab, false);
    }

    public static long Insert(MedLab medLab, bool useExistingPK)
    {
        var command = "INSERT INTO medlab (";

        command += "ProvNum,SendingApp,SendingFacility,PatNum,PatIDLab,PatIDAlt,PatAge,PatAccountNum,PatFasting,SpecimenID,SpecimenIDFiller,ObsTestID,ObsTestDescript,ObsTestLoinc,ObsTestLoincText,DateTimeCollected,TotalVolume,ActionCode,ClinicalInfo,DateTimeEntered,OrderingProvNPI,OrderingProvLocalID,OrderingProvLName,OrderingProvFName,SpecimenIDAlt,DateTimeReported,ResultStatus,ParentObsID,ParentObsTestID,NotePat,NoteLab,FileName,OriginalPIDSegment) VALUES(";

        command +=
            SOut.Long(medLab.ProvNum) + ","
                                      + "'" + SOut.String(medLab.SendingApp) + "',"
                                      + "'" + SOut.String(medLab.SendingFacility) + "',"
                                      + SOut.Long(medLab.PatNum) + ","
                                      + "'" + SOut.String(medLab.PatIDLab) + "',"
                                      + "'" + SOut.String(medLab.PatIDAlt) + "',"
                                      + "'" + SOut.String(medLab.PatAge) + "',"
                                      + "'" + SOut.String(medLab.PatAccountNum) + "',"
                                      + SOut.Int((int) medLab.PatFasting) + ","
                                      + "'" + SOut.String(medLab.SpecimenID) + "',"
                                      + "'" + SOut.String(medLab.SpecimenIDFiller) + "',"
                                      + "'" + SOut.String(medLab.ObsTestID) + "',"
                                      + "'" + SOut.String(medLab.ObsTestDescript) + "',"
                                      + "'" + SOut.String(medLab.ObsTestLoinc) + "',"
                                      + "'" + SOut.String(medLab.ObsTestLoincText) + "',"
                                      + SOut.DateT(medLab.DateTimeCollected) + ","
                                      + "'" + SOut.String(medLab.TotalVolume) + "',"
                                      + "'" + SOut.String(medLab.ActionCode.ToString()) + "',"
                                      + "'" + SOut.String(medLab.ClinicalInfo) + "',"
                                      + SOut.DateT(medLab.DateTimeEntered) + ","
                                      + "'" + SOut.String(medLab.OrderingProvNPI) + "',"
                                      + "'" + SOut.String(medLab.OrderingProvLocalID) + "',"
                                      + "'" + SOut.String(medLab.OrderingProvLName) + "',"
                                      + "'" + SOut.String(medLab.OrderingProvFName) + "',"
                                      + "'" + SOut.String(medLab.SpecimenIDAlt) + "',"
                                      + SOut.DateT(medLab.DateTimeReported) + ","
                                      + "'" + SOut.String(medLab.ResultStatus.ToString()) + "',"
                                      + "'" + SOut.String(medLab.ParentObsID) + "',"
                                      + "'" + SOut.String(medLab.ParentObsTestID) + "',"
                                      + DbHelper.ParamChar + "paramNotePat,"
                                      + DbHelper.ParamChar + "paramNoteLab,"
                                      + "'" + SOut.String(medLab.FileName) + "',"
                                      + DbHelper.ParamChar + "paramOriginalPIDSegment)";
        if (medLab.NotePat == null) medLab.NotePat = "";
        var paramNotePat = new OdSqlParameter("paramNotePat", OdDbType.Text, SOut.StringParam(medLab.NotePat));
        if (medLab.NoteLab == null) medLab.NoteLab = "";
        var paramNoteLab = new OdSqlParameter("paramNoteLab", OdDbType.Text, SOut.StringParam(medLab.NoteLab));
        if (medLab.OriginalPIDSegment == null) medLab.OriginalPIDSegment = "";
        var paramOriginalPIDSegment = new OdSqlParameter("paramOriginalPIDSegment", OdDbType.Text, SOut.StringParam(medLab.OriginalPIDSegment));
        {
            medLab.MedLabNum = Db.NonQ(command, true, "MedLabNum", "medLab", paramNotePat, paramNoteLab, paramOriginalPIDSegment);
        }
        return medLab.MedLabNum;
    }

    public static long InsertNoCache(MedLab medLab)
    {
        return InsertNoCache(medLab, false);
    }

    public static long InsertNoCache(MedLab medLab, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO medlab (";
        if (isRandomKeys || useExistingPK) command += "MedLabNum,";
        command += "ProvNum,SendingApp,SendingFacility,PatNum,PatIDLab,PatIDAlt,PatAge,PatAccountNum,PatFasting,SpecimenID,SpecimenIDFiller,ObsTestID,ObsTestDescript,ObsTestLoinc,ObsTestLoincText,DateTimeCollected,TotalVolume,ActionCode,ClinicalInfo,DateTimeEntered,OrderingProvNPI,OrderingProvLocalID,OrderingProvLName,OrderingProvFName,SpecimenIDAlt,DateTimeReported,ResultStatus,ParentObsID,ParentObsTestID,NotePat,NoteLab,FileName,OriginalPIDSegment) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(medLab.MedLabNum) + ",";
        command +=
            SOut.Long(medLab.ProvNum) + ","
                                      + "'" + SOut.String(medLab.SendingApp) + "',"
                                      + "'" + SOut.String(medLab.SendingFacility) + "',"
                                      + SOut.Long(medLab.PatNum) + ","
                                      + "'" + SOut.String(medLab.PatIDLab) + "',"
                                      + "'" + SOut.String(medLab.PatIDAlt) + "',"
                                      + "'" + SOut.String(medLab.PatAge) + "',"
                                      + "'" + SOut.String(medLab.PatAccountNum) + "',"
                                      + SOut.Int((int) medLab.PatFasting) + ","
                                      + "'" + SOut.String(medLab.SpecimenID) + "',"
                                      + "'" + SOut.String(medLab.SpecimenIDFiller) + "',"
                                      + "'" + SOut.String(medLab.ObsTestID) + "',"
                                      + "'" + SOut.String(medLab.ObsTestDescript) + "',"
                                      + "'" + SOut.String(medLab.ObsTestLoinc) + "',"
                                      + "'" + SOut.String(medLab.ObsTestLoincText) + "',"
                                      + SOut.DateT(medLab.DateTimeCollected) + ","
                                      + "'" + SOut.String(medLab.TotalVolume) + "',"
                                      + "'" + SOut.String(medLab.ActionCode.ToString()) + "',"
                                      + "'" + SOut.String(medLab.ClinicalInfo) + "',"
                                      + SOut.DateT(medLab.DateTimeEntered) + ","
                                      + "'" + SOut.String(medLab.OrderingProvNPI) + "',"
                                      + "'" + SOut.String(medLab.OrderingProvLocalID) + "',"
                                      + "'" + SOut.String(medLab.OrderingProvLName) + "',"
                                      + "'" + SOut.String(medLab.OrderingProvFName) + "',"
                                      + "'" + SOut.String(medLab.SpecimenIDAlt) + "',"
                                      + SOut.DateT(medLab.DateTimeReported) + ","
                                      + "'" + SOut.String(medLab.ResultStatus.ToString()) + "',"
                                      + "'" + SOut.String(medLab.ParentObsID) + "',"
                                      + "'" + SOut.String(medLab.ParentObsTestID) + "',"
                                      + DbHelper.ParamChar + "paramNotePat,"
                                      + DbHelper.ParamChar + "paramNoteLab,"
                                      + "'" + SOut.String(medLab.FileName) + "',"
                                      + DbHelper.ParamChar + "paramOriginalPIDSegment)";
        if (medLab.NotePat == null) medLab.NotePat = "";
        var paramNotePat = new OdSqlParameter("paramNotePat", OdDbType.Text, SOut.StringParam(medLab.NotePat));
        if (medLab.NoteLab == null) medLab.NoteLab = "";
        var paramNoteLab = new OdSqlParameter("paramNoteLab", OdDbType.Text, SOut.StringParam(medLab.NoteLab));
        if (medLab.OriginalPIDSegment == null) medLab.OriginalPIDSegment = "";
        var paramOriginalPIDSegment = new OdSqlParameter("paramOriginalPIDSegment", OdDbType.Text, SOut.StringParam(medLab.OriginalPIDSegment));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramNotePat, paramNoteLab, paramOriginalPIDSegment);
        else
            medLab.MedLabNum = Db.NonQ(command, true, "MedLabNum", "medLab", paramNotePat, paramNoteLab, paramOriginalPIDSegment);
        return medLab.MedLabNum;
    }

    public static void Update(MedLab medLab)
    {
        var command = "UPDATE medlab SET "
                      + "ProvNum            =  " + SOut.Long(medLab.ProvNum) + ", "
                      + "SendingApp         = '" + SOut.String(medLab.SendingApp) + "', "
                      + "SendingFacility    = '" + SOut.String(medLab.SendingFacility) + "', "
                      + "PatNum             =  " + SOut.Long(medLab.PatNum) + ", "
                      + "PatIDLab           = '" + SOut.String(medLab.PatIDLab) + "', "
                      + "PatIDAlt           = '" + SOut.String(medLab.PatIDAlt) + "', "
                      + "PatAge             = '" + SOut.String(medLab.PatAge) + "', "
                      + "PatAccountNum      = '" + SOut.String(medLab.PatAccountNum) + "', "
                      + "PatFasting         =  " + SOut.Int((int) medLab.PatFasting) + ", "
                      + "SpecimenID         = '" + SOut.String(medLab.SpecimenID) + "', "
                      + "SpecimenIDFiller   = '" + SOut.String(medLab.SpecimenIDFiller) + "', "
                      + "ObsTestID          = '" + SOut.String(medLab.ObsTestID) + "', "
                      + "ObsTestDescript    = '" + SOut.String(medLab.ObsTestDescript) + "', "
                      + "ObsTestLoinc       = '" + SOut.String(medLab.ObsTestLoinc) + "', "
                      + "ObsTestLoincText   = '" + SOut.String(medLab.ObsTestLoincText) + "', "
                      + "DateTimeCollected  =  " + SOut.DateT(medLab.DateTimeCollected) + ", "
                      + "TotalVolume        = '" + SOut.String(medLab.TotalVolume) + "', "
                      + "ActionCode         = '" + SOut.String(medLab.ActionCode.ToString()) + "', "
                      + "ClinicalInfo       = '" + SOut.String(medLab.ClinicalInfo) + "', "
                      + "DateTimeEntered    =  " + SOut.DateT(medLab.DateTimeEntered) + ", "
                      + "OrderingProvNPI    = '" + SOut.String(medLab.OrderingProvNPI) + "', "
                      + "OrderingProvLocalID= '" + SOut.String(medLab.OrderingProvLocalID) + "', "
                      + "OrderingProvLName  = '" + SOut.String(medLab.OrderingProvLName) + "', "
                      + "OrderingProvFName  = '" + SOut.String(medLab.OrderingProvFName) + "', "
                      + "SpecimenIDAlt      = '" + SOut.String(medLab.SpecimenIDAlt) + "', "
                      + "DateTimeReported   =  " + SOut.DateT(medLab.DateTimeReported) + ", "
                      + "ResultStatus       = '" + SOut.String(medLab.ResultStatus.ToString()) + "', "
                      + "ParentObsID        = '" + SOut.String(medLab.ParentObsID) + "', "
                      + "ParentObsTestID    = '" + SOut.String(medLab.ParentObsTestID) + "', "
                      + "NotePat            =  " + DbHelper.ParamChar + "paramNotePat, "
                      + "NoteLab            =  " + DbHelper.ParamChar + "paramNoteLab, "
                      + "FileName           = '" + SOut.String(medLab.FileName) + "', "
                      + "OriginalPIDSegment =  " + DbHelper.ParamChar + "paramOriginalPIDSegment "
                      + "WHERE MedLabNum = " + SOut.Long(medLab.MedLabNum);
        if (medLab.NotePat == null) medLab.NotePat = "";
        var paramNotePat = new OdSqlParameter("paramNotePat", OdDbType.Text, SOut.StringParam(medLab.NotePat));
        if (medLab.NoteLab == null) medLab.NoteLab = "";
        var paramNoteLab = new OdSqlParameter("paramNoteLab", OdDbType.Text, SOut.StringParam(medLab.NoteLab));
        if (medLab.OriginalPIDSegment == null) medLab.OriginalPIDSegment = "";
        var paramOriginalPIDSegment = new OdSqlParameter("paramOriginalPIDSegment", OdDbType.Text, SOut.StringParam(medLab.OriginalPIDSegment));
        Db.NonQ(command, paramNotePat, paramNoteLab, paramOriginalPIDSegment);
    }

    public static bool Update(MedLab medLab, MedLab oldMedLab)
    {
        var command = "";
        if (medLab.ProvNum != oldMedLab.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(medLab.ProvNum) + "";
        }

        if (medLab.SendingApp != oldMedLab.SendingApp)
        {
            if (command != "") command += ",";
            command += "SendingApp = '" + SOut.String(medLab.SendingApp) + "'";
        }

        if (medLab.SendingFacility != oldMedLab.SendingFacility)
        {
            if (command != "") command += ",";
            command += "SendingFacility = '" + SOut.String(medLab.SendingFacility) + "'";
        }

        if (medLab.PatNum != oldMedLab.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(medLab.PatNum) + "";
        }

        if (medLab.PatIDLab != oldMedLab.PatIDLab)
        {
            if (command != "") command += ",";
            command += "PatIDLab = '" + SOut.String(medLab.PatIDLab) + "'";
        }

        if (medLab.PatIDAlt != oldMedLab.PatIDAlt)
        {
            if (command != "") command += ",";
            command += "PatIDAlt = '" + SOut.String(medLab.PatIDAlt) + "'";
        }

        if (medLab.PatAge != oldMedLab.PatAge)
        {
            if (command != "") command += ",";
            command += "PatAge = '" + SOut.String(medLab.PatAge) + "'";
        }

        if (medLab.PatAccountNum != oldMedLab.PatAccountNum)
        {
            if (command != "") command += ",";
            command += "PatAccountNum = '" + SOut.String(medLab.PatAccountNum) + "'";
        }

        if (medLab.PatFasting != oldMedLab.PatFasting)
        {
            if (command != "") command += ",";
            command += "PatFasting = " + SOut.Int((int) medLab.PatFasting) + "";
        }

        if (medLab.SpecimenID != oldMedLab.SpecimenID)
        {
            if (command != "") command += ",";
            command += "SpecimenID = '" + SOut.String(medLab.SpecimenID) + "'";
        }

        if (medLab.SpecimenIDFiller != oldMedLab.SpecimenIDFiller)
        {
            if (command != "") command += ",";
            command += "SpecimenIDFiller = '" + SOut.String(medLab.SpecimenIDFiller) + "'";
        }

        if (medLab.ObsTestID != oldMedLab.ObsTestID)
        {
            if (command != "") command += ",";
            command += "ObsTestID = '" + SOut.String(medLab.ObsTestID) + "'";
        }

        if (medLab.ObsTestDescript != oldMedLab.ObsTestDescript)
        {
            if (command != "") command += ",";
            command += "ObsTestDescript = '" + SOut.String(medLab.ObsTestDescript) + "'";
        }

        if (medLab.ObsTestLoinc != oldMedLab.ObsTestLoinc)
        {
            if (command != "") command += ",";
            command += "ObsTestLoinc = '" + SOut.String(medLab.ObsTestLoinc) + "'";
        }

        if (medLab.ObsTestLoincText != oldMedLab.ObsTestLoincText)
        {
            if (command != "") command += ",";
            command += "ObsTestLoincText = '" + SOut.String(medLab.ObsTestLoincText) + "'";
        }

        if (medLab.DateTimeCollected != oldMedLab.DateTimeCollected)
        {
            if (command != "") command += ",";
            command += "DateTimeCollected = " + SOut.DateT(medLab.DateTimeCollected) + "";
        }

        if (medLab.TotalVolume != oldMedLab.TotalVolume)
        {
            if (command != "") command += ",";
            command += "TotalVolume = '" + SOut.String(medLab.TotalVolume) + "'";
        }

        if (medLab.ActionCode != oldMedLab.ActionCode)
        {
            if (command != "") command += ",";
            command += "ActionCode = '" + SOut.String(medLab.ActionCode.ToString()) + "'";
        }

        if (medLab.ClinicalInfo != oldMedLab.ClinicalInfo)
        {
            if (command != "") command += ",";
            command += "ClinicalInfo = '" + SOut.String(medLab.ClinicalInfo) + "'";
        }

        if (medLab.DateTimeEntered != oldMedLab.DateTimeEntered)
        {
            if (command != "") command += ",";
            command += "DateTimeEntered = " + SOut.DateT(medLab.DateTimeEntered) + "";
        }

        if (medLab.OrderingProvNPI != oldMedLab.OrderingProvNPI)
        {
            if (command != "") command += ",";
            command += "OrderingProvNPI = '" + SOut.String(medLab.OrderingProvNPI) + "'";
        }

        if (medLab.OrderingProvLocalID != oldMedLab.OrderingProvLocalID)
        {
            if (command != "") command += ",";
            command += "OrderingProvLocalID = '" + SOut.String(medLab.OrderingProvLocalID) + "'";
        }

        if (medLab.OrderingProvLName != oldMedLab.OrderingProvLName)
        {
            if (command != "") command += ",";
            command += "OrderingProvLName = '" + SOut.String(medLab.OrderingProvLName) + "'";
        }

        if (medLab.OrderingProvFName != oldMedLab.OrderingProvFName)
        {
            if (command != "") command += ",";
            command += "OrderingProvFName = '" + SOut.String(medLab.OrderingProvFName) + "'";
        }

        if (medLab.SpecimenIDAlt != oldMedLab.SpecimenIDAlt)
        {
            if (command != "") command += ",";
            command += "SpecimenIDAlt = '" + SOut.String(medLab.SpecimenIDAlt) + "'";
        }

        if (medLab.DateTimeReported != oldMedLab.DateTimeReported)
        {
            if (command != "") command += ",";
            command += "DateTimeReported = " + SOut.DateT(medLab.DateTimeReported) + "";
        }

        if (medLab.ResultStatus != oldMedLab.ResultStatus)
        {
            if (command != "") command += ",";
            command += "ResultStatus = '" + SOut.String(medLab.ResultStatus.ToString()) + "'";
        }

        if (medLab.ParentObsID != oldMedLab.ParentObsID)
        {
            if (command != "") command += ",";
            command += "ParentObsID = '" + SOut.String(medLab.ParentObsID) + "'";
        }

        if (medLab.ParentObsTestID != oldMedLab.ParentObsTestID)
        {
            if (command != "") command += ",";
            command += "ParentObsTestID = '" + SOut.String(medLab.ParentObsTestID) + "'";
        }

        if (medLab.NotePat != oldMedLab.NotePat)
        {
            if (command != "") command += ",";
            command += "NotePat = " + DbHelper.ParamChar + "paramNotePat";
        }

        if (medLab.NoteLab != oldMedLab.NoteLab)
        {
            if (command != "") command += ",";
            command += "NoteLab = " + DbHelper.ParamChar + "paramNoteLab";
        }

        if (medLab.FileName != oldMedLab.FileName)
        {
            if (command != "") command += ",";
            command += "FileName = '" + SOut.String(medLab.FileName) + "'";
        }

        if (medLab.OriginalPIDSegment != oldMedLab.OriginalPIDSegment)
        {
            if (command != "") command += ",";
            command += "OriginalPIDSegment = " + DbHelper.ParamChar + "paramOriginalPIDSegment";
        }

        if (command == "") return false;
        if (medLab.NotePat == null) medLab.NotePat = "";
        var paramNotePat = new OdSqlParameter("paramNotePat", OdDbType.Text, SOut.StringParam(medLab.NotePat));
        if (medLab.NoteLab == null) medLab.NoteLab = "";
        var paramNoteLab = new OdSqlParameter("paramNoteLab", OdDbType.Text, SOut.StringParam(medLab.NoteLab));
        if (medLab.OriginalPIDSegment == null) medLab.OriginalPIDSegment = "";
        var paramOriginalPIDSegment = new OdSqlParameter("paramOriginalPIDSegment", OdDbType.Text, SOut.StringParam(medLab.OriginalPIDSegment));
        command = "UPDATE medlab SET " + command
                                       + " WHERE MedLabNum = " + SOut.Long(medLab.MedLabNum);
        Db.NonQ(command, paramNotePat, paramNoteLab, paramOriginalPIDSegment);
        return true;
    }

    public static bool UpdateComparison(MedLab medLab, MedLab oldMedLab)
    {
        if (medLab.ProvNum != oldMedLab.ProvNum) return true;
        if (medLab.SendingApp != oldMedLab.SendingApp) return true;
        if (medLab.SendingFacility != oldMedLab.SendingFacility) return true;
        if (medLab.PatNum != oldMedLab.PatNum) return true;
        if (medLab.PatIDLab != oldMedLab.PatIDLab) return true;
        if (medLab.PatIDAlt != oldMedLab.PatIDAlt) return true;
        if (medLab.PatAge != oldMedLab.PatAge) return true;
        if (medLab.PatAccountNum != oldMedLab.PatAccountNum) return true;
        if (medLab.PatFasting != oldMedLab.PatFasting) return true;
        if (medLab.SpecimenID != oldMedLab.SpecimenID) return true;
        if (medLab.SpecimenIDFiller != oldMedLab.SpecimenIDFiller) return true;
        if (medLab.ObsTestID != oldMedLab.ObsTestID) return true;
        if (medLab.ObsTestDescript != oldMedLab.ObsTestDescript) return true;
        if (medLab.ObsTestLoinc != oldMedLab.ObsTestLoinc) return true;
        if (medLab.ObsTestLoincText != oldMedLab.ObsTestLoincText) return true;
        if (medLab.DateTimeCollected != oldMedLab.DateTimeCollected) return true;
        if (medLab.TotalVolume != oldMedLab.TotalVolume) return true;
        if (medLab.ActionCode != oldMedLab.ActionCode) return true;
        if (medLab.ClinicalInfo != oldMedLab.ClinicalInfo) return true;
        if (medLab.DateTimeEntered != oldMedLab.DateTimeEntered) return true;
        if (medLab.OrderingProvNPI != oldMedLab.OrderingProvNPI) return true;
        if (medLab.OrderingProvLocalID != oldMedLab.OrderingProvLocalID) return true;
        if (medLab.OrderingProvLName != oldMedLab.OrderingProvLName) return true;
        if (medLab.OrderingProvFName != oldMedLab.OrderingProvFName) return true;
        if (medLab.SpecimenIDAlt != oldMedLab.SpecimenIDAlt) return true;
        if (medLab.DateTimeReported != oldMedLab.DateTimeReported) return true;
        if (medLab.ResultStatus != oldMedLab.ResultStatus) return true;
        if (medLab.ParentObsID != oldMedLab.ParentObsID) return true;
        if (medLab.ParentObsTestID != oldMedLab.ParentObsTestID) return true;
        if (medLab.NotePat != oldMedLab.NotePat) return true;
        if (medLab.NoteLab != oldMedLab.NoteLab) return true;
        if (medLab.FileName != oldMedLab.FileName) return true;
        if (medLab.OriginalPIDSegment != oldMedLab.OriginalPIDSegment) return true;
        return false;
    }

    public static void Delete(long medLabNum)
    {
        var command = "DELETE FROM medlab "
                      + "WHERE MedLabNum = " + SOut.Long(medLabNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listMedLabNums)
    {
        if (listMedLabNums == null || listMedLabNums.Count == 0) return;
        var command = "DELETE FROM medlab "
                      + "WHERE MedLabNum IN(" + string.Join(",", listMedLabNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}