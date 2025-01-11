#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using EhrLaboratories;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrLabCrud
{
    public static EhrLab SelectOne(long ehrLabNum)
    {
        var command = "SELECT * FROM ehrlab "
                      + "WHERE EhrLabNum = " + SOut.Long(ehrLabNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrLab SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrLab> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrLab> TableToList(DataTable table)
    {
        var retVal = new List<EhrLab>();
        EhrLab ehrLab;
        foreach (DataRow row in table.Rows)
        {
            ehrLab = new EhrLab();
            ehrLab.EhrLabNum = SIn.Long(row["EhrLabNum"].ToString());
            ehrLab.PatNum = SIn.Long(row["PatNum"].ToString());
            var orderControlCode = row["OrderControlCode"].ToString();
            if (orderControlCode == "")
                ehrLab.OrderControlCode = 0;
            else
                try
                {
                    ehrLab.OrderControlCode = (HL70119) Enum.Parse(typeof(HL70119), orderControlCode);
                }
                catch
                {
                    ehrLab.OrderControlCode = 0;
                }

            ehrLab.PlacerOrderNum = SIn.String(row["PlacerOrderNum"].ToString());
            ehrLab.PlacerOrderNamespace = SIn.String(row["PlacerOrderNamespace"].ToString());
            ehrLab.PlacerOrderUniversalID = SIn.String(row["PlacerOrderUniversalID"].ToString());
            ehrLab.PlacerOrderUniversalIDType = SIn.String(row["PlacerOrderUniversalIDType"].ToString());
            ehrLab.FillerOrderNum = SIn.String(row["FillerOrderNum"].ToString());
            ehrLab.FillerOrderNamespace = SIn.String(row["FillerOrderNamespace"].ToString());
            ehrLab.FillerOrderUniversalID = SIn.String(row["FillerOrderUniversalID"].ToString());
            ehrLab.FillerOrderUniversalIDType = SIn.String(row["FillerOrderUniversalIDType"].ToString());
            ehrLab.PlacerGroupNum = SIn.String(row["PlacerGroupNum"].ToString());
            ehrLab.PlacerGroupNamespace = SIn.String(row["PlacerGroupNamespace"].ToString());
            ehrLab.PlacerGroupUniversalID = SIn.String(row["PlacerGroupUniversalID"].ToString());
            ehrLab.PlacerGroupUniversalIDType = SIn.String(row["PlacerGroupUniversalIDType"].ToString());
            ehrLab.OrderingProviderID = SIn.String(row["OrderingProviderID"].ToString());
            ehrLab.OrderingProviderLName = SIn.String(row["OrderingProviderLName"].ToString());
            ehrLab.OrderingProviderFName = SIn.String(row["OrderingProviderFName"].ToString());
            ehrLab.OrderingProviderMiddleNames = SIn.String(row["OrderingProviderMiddleNames"].ToString());
            ehrLab.OrderingProviderSuffix = SIn.String(row["OrderingProviderSuffix"].ToString());
            ehrLab.OrderingProviderPrefix = SIn.String(row["OrderingProviderPrefix"].ToString());
            ehrLab.OrderingProviderAssigningAuthorityNamespaceID = SIn.String(row["OrderingProviderAssigningAuthorityNamespaceID"].ToString());
            ehrLab.OrderingProviderAssigningAuthorityUniversalID = SIn.String(row["OrderingProviderAssigningAuthorityUniversalID"].ToString());
            ehrLab.OrderingProviderAssigningAuthorityIDType = SIn.String(row["OrderingProviderAssigningAuthorityIDType"].ToString());
            var orderingProviderNameTypeCode = row["OrderingProviderNameTypeCode"].ToString();
            if (orderingProviderNameTypeCode == "")
                ehrLab.OrderingProviderNameTypeCode = 0;
            else
                try
                {
                    ehrLab.OrderingProviderNameTypeCode = (HL70200) Enum.Parse(typeof(HL70200), orderingProviderNameTypeCode);
                }
                catch
                {
                    ehrLab.OrderingProviderNameTypeCode = 0;
                }

            var orderingProviderIdentifierTypeCode = row["OrderingProviderIdentifierTypeCode"].ToString();
            if (orderingProviderIdentifierTypeCode == "")
                ehrLab.OrderingProviderIdentifierTypeCode = 0;
            else
                try
                {
                    ehrLab.OrderingProviderIdentifierTypeCode = (HL70203) Enum.Parse(typeof(HL70203), orderingProviderIdentifierTypeCode);
                }
                catch
                {
                    ehrLab.OrderingProviderIdentifierTypeCode = 0;
                }

            ehrLab.SetIdOBR = SIn.Long(row["SetIdOBR"].ToString());
            ehrLab.UsiID = SIn.String(row["UsiID"].ToString());
            ehrLab.UsiText = SIn.String(row["UsiText"].ToString());
            ehrLab.UsiCodeSystemName = SIn.String(row["UsiCodeSystemName"].ToString());
            ehrLab.UsiIDAlt = SIn.String(row["UsiIDAlt"].ToString());
            ehrLab.UsiTextAlt = SIn.String(row["UsiTextAlt"].ToString());
            ehrLab.UsiCodeSystemNameAlt = SIn.String(row["UsiCodeSystemNameAlt"].ToString());
            ehrLab.UsiTextOriginal = SIn.String(row["UsiTextOriginal"].ToString());
            ehrLab.ObservationDateTimeStart = SIn.String(row["ObservationDateTimeStart"].ToString());
            ehrLab.ObservationDateTimeEnd = SIn.String(row["ObservationDateTimeEnd"].ToString());
            var specimenActionCode = row["SpecimenActionCode"].ToString();
            if (specimenActionCode == "")
                ehrLab.SpecimenActionCode = 0;
            else
                try
                {
                    ehrLab.SpecimenActionCode = (HL70065) Enum.Parse(typeof(HL70065), specimenActionCode);
                }
                catch
                {
                    ehrLab.SpecimenActionCode = 0;
                }

            ehrLab.ResultDateTime = SIn.String(row["ResultDateTime"].ToString());
            var resultStatus = row["ResultStatus"].ToString();
            if (resultStatus == "")
                ehrLab.ResultStatus = 0;
            else
                try
                {
                    ehrLab.ResultStatus = (HL70123) Enum.Parse(typeof(HL70123), resultStatus);
                }
                catch
                {
                    ehrLab.ResultStatus = 0;
                }

            ehrLab.ParentObservationID = SIn.String(row["ParentObservationID"].ToString());
            ehrLab.ParentObservationText = SIn.String(row["ParentObservationText"].ToString());
            ehrLab.ParentObservationCodeSystemName = SIn.String(row["ParentObservationCodeSystemName"].ToString());
            ehrLab.ParentObservationIDAlt = SIn.String(row["ParentObservationIDAlt"].ToString());
            ehrLab.ParentObservationTextAlt = SIn.String(row["ParentObservationTextAlt"].ToString());
            ehrLab.ParentObservationCodeSystemNameAlt = SIn.String(row["ParentObservationCodeSystemNameAlt"].ToString());
            ehrLab.ParentObservationTextOriginal = SIn.String(row["ParentObservationTextOriginal"].ToString());
            ehrLab.ParentObservationSubID = SIn.String(row["ParentObservationSubID"].ToString());
            ehrLab.ParentPlacerOrderNum = SIn.String(row["ParentPlacerOrderNum"].ToString());
            ehrLab.ParentPlacerOrderNamespace = SIn.String(row["ParentPlacerOrderNamespace"].ToString());
            ehrLab.ParentPlacerOrderUniversalID = SIn.String(row["ParentPlacerOrderUniversalID"].ToString());
            ehrLab.ParentPlacerOrderUniversalIDType = SIn.String(row["ParentPlacerOrderUniversalIDType"].ToString());
            ehrLab.ParentFillerOrderNum = SIn.String(row["ParentFillerOrderNum"].ToString());
            ehrLab.ParentFillerOrderNamespace = SIn.String(row["ParentFillerOrderNamespace"].ToString());
            ehrLab.ParentFillerOrderUniversalID = SIn.String(row["ParentFillerOrderUniversalID"].ToString());
            ehrLab.ParentFillerOrderUniversalIDType = SIn.String(row["ParentFillerOrderUniversalIDType"].ToString());
            ehrLab.ListEhrLabResultsHandlingF = SIn.Bool(row["ListEhrLabResultsHandlingF"].ToString());
            ehrLab.ListEhrLabResultsHandlingN = SIn.Bool(row["ListEhrLabResultsHandlingN"].ToString());
            ehrLab.TQ1SetId = SIn.Long(row["TQ1SetId"].ToString());
            ehrLab.TQ1DateTimeStart = SIn.String(row["TQ1DateTimeStart"].ToString());
            ehrLab.TQ1DateTimeEnd = SIn.String(row["TQ1DateTimeEnd"].ToString());
            ehrLab.IsCpoe = SIn.Bool(row["IsCpoe"].ToString());
            ehrLab.OriginalPIDSegment = SIn.String(row["OriginalPIDSegment"].ToString());
            retVal.Add(ehrLab);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrLab> listEhrLabs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrLab";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrLabNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("OrderControlCode");
        table.Columns.Add("PlacerOrderNum");
        table.Columns.Add("PlacerOrderNamespace");
        table.Columns.Add("PlacerOrderUniversalID");
        table.Columns.Add("PlacerOrderUniversalIDType");
        table.Columns.Add("FillerOrderNum");
        table.Columns.Add("FillerOrderNamespace");
        table.Columns.Add("FillerOrderUniversalID");
        table.Columns.Add("FillerOrderUniversalIDType");
        table.Columns.Add("PlacerGroupNum");
        table.Columns.Add("PlacerGroupNamespace");
        table.Columns.Add("PlacerGroupUniversalID");
        table.Columns.Add("PlacerGroupUniversalIDType");
        table.Columns.Add("OrderingProviderID");
        table.Columns.Add("OrderingProviderLName");
        table.Columns.Add("OrderingProviderFName");
        table.Columns.Add("OrderingProviderMiddleNames");
        table.Columns.Add("OrderingProviderSuffix");
        table.Columns.Add("OrderingProviderPrefix");
        table.Columns.Add("OrderingProviderAssigningAuthorityNamespaceID");
        table.Columns.Add("OrderingProviderAssigningAuthorityUniversalID");
        table.Columns.Add("OrderingProviderAssigningAuthorityIDType");
        table.Columns.Add("OrderingProviderNameTypeCode");
        table.Columns.Add("OrderingProviderIdentifierTypeCode");
        table.Columns.Add("SetIdOBR");
        table.Columns.Add("UsiID");
        table.Columns.Add("UsiText");
        table.Columns.Add("UsiCodeSystemName");
        table.Columns.Add("UsiIDAlt");
        table.Columns.Add("UsiTextAlt");
        table.Columns.Add("UsiCodeSystemNameAlt");
        table.Columns.Add("UsiTextOriginal");
        table.Columns.Add("ObservationDateTimeStart");
        table.Columns.Add("ObservationDateTimeEnd");
        table.Columns.Add("SpecimenActionCode");
        table.Columns.Add("ResultDateTime");
        table.Columns.Add("ResultStatus");
        table.Columns.Add("ParentObservationID");
        table.Columns.Add("ParentObservationText");
        table.Columns.Add("ParentObservationCodeSystemName");
        table.Columns.Add("ParentObservationIDAlt");
        table.Columns.Add("ParentObservationTextAlt");
        table.Columns.Add("ParentObservationCodeSystemNameAlt");
        table.Columns.Add("ParentObservationTextOriginal");
        table.Columns.Add("ParentObservationSubID");
        table.Columns.Add("ParentPlacerOrderNum");
        table.Columns.Add("ParentPlacerOrderNamespace");
        table.Columns.Add("ParentPlacerOrderUniversalID");
        table.Columns.Add("ParentPlacerOrderUniversalIDType");
        table.Columns.Add("ParentFillerOrderNum");
        table.Columns.Add("ParentFillerOrderNamespace");
        table.Columns.Add("ParentFillerOrderUniversalID");
        table.Columns.Add("ParentFillerOrderUniversalIDType");
        table.Columns.Add("ListEhrLabResultsHandlingF");
        table.Columns.Add("ListEhrLabResultsHandlingN");
        table.Columns.Add("TQ1SetId");
        table.Columns.Add("TQ1DateTimeStart");
        table.Columns.Add("TQ1DateTimeEnd");
        table.Columns.Add("IsCpoe");
        table.Columns.Add("OriginalPIDSegment");
        foreach (var ehrLab in listEhrLabs)
            table.Rows.Add(SOut.Long(ehrLab.EhrLabNum), SOut.Long(ehrLab.PatNum), SOut.Int((int) ehrLab.OrderControlCode), ehrLab.PlacerOrderNum, ehrLab.PlacerOrderNamespace, ehrLab.PlacerOrderUniversalID, ehrLab.PlacerOrderUniversalIDType, ehrLab.FillerOrderNum, ehrLab.FillerOrderNamespace, ehrLab.FillerOrderUniversalID, ehrLab.FillerOrderUniversalIDType, ehrLab.PlacerGroupNum, ehrLab.PlacerGroupNamespace, ehrLab.PlacerGroupUniversalID, ehrLab.PlacerGroupUniversalIDType, ehrLab.OrderingProviderID, ehrLab.OrderingProviderLName, ehrLab.OrderingProviderFName, ehrLab.OrderingProviderMiddleNames, ehrLab.OrderingProviderSuffix, ehrLab.OrderingProviderPrefix, ehrLab.OrderingProviderAssigningAuthorityNamespaceID, ehrLab.OrderingProviderAssigningAuthorityUniversalID, ehrLab.OrderingProviderAssigningAuthorityIDType, SOut.Int((int) ehrLab.OrderingProviderNameTypeCode), SOut.Int((int) ehrLab.OrderingProviderIdentifierTypeCode), SOut.Long(ehrLab.SetIdOBR), ehrLab.UsiID, ehrLab.UsiText, ehrLab.UsiCodeSystemName, ehrLab.UsiIDAlt, ehrLab.UsiTextAlt, ehrLab.UsiCodeSystemNameAlt, ehrLab.UsiTextOriginal, ehrLab.ObservationDateTimeStart, ehrLab.ObservationDateTimeEnd, SOut.Int((int) ehrLab.SpecimenActionCode), ehrLab.ResultDateTime, SOut.Int((int) ehrLab.ResultStatus), ehrLab.ParentObservationID, ehrLab.ParentObservationText, ehrLab.ParentObservationCodeSystemName, ehrLab.ParentObservationIDAlt, ehrLab.ParentObservationTextAlt, ehrLab.ParentObservationCodeSystemNameAlt, ehrLab.ParentObservationTextOriginal, ehrLab.ParentObservationSubID, ehrLab.ParentPlacerOrderNum, ehrLab.ParentPlacerOrderNamespace, ehrLab.ParentPlacerOrderUniversalID, ehrLab.ParentPlacerOrderUniversalIDType, ehrLab.ParentFillerOrderNum, ehrLab.ParentFillerOrderNamespace, ehrLab.ParentFillerOrderUniversalID, ehrLab.ParentFillerOrderUniversalIDType, SOut.Bool(ehrLab.ListEhrLabResultsHandlingF), SOut.Bool(ehrLab.ListEhrLabResultsHandlingN), SOut.Long(ehrLab.TQ1SetId), ehrLab.TQ1DateTimeStart, ehrLab.TQ1DateTimeEnd, SOut.Bool(ehrLab.IsCpoe), ehrLab.OriginalPIDSegment);
        return table;
    }

    public static long Insert(EhrLab ehrLab)
    {
        return Insert(ehrLab, false);
    }


    public static long Insert(EhrLab ehrLab, bool useExistingPK)
    {
        var command = "INSERT INTO ehrlab (";

        command += "PatNum,OrderControlCode,PlacerOrderNum,PlacerOrderNamespace,PlacerOrderUniversalID,PlacerOrderUniversalIDType,FillerOrderNum,FillerOrderNamespace,FillerOrderUniversalID,FillerOrderUniversalIDType,PlacerGroupNum,PlacerGroupNamespace,PlacerGroupUniversalID,PlacerGroupUniversalIDType,OrderingProviderID,OrderingProviderLName,OrderingProviderFName,OrderingProviderMiddleNames,OrderingProviderSuffix,OrderingProviderPrefix,OrderingProviderAssigningAuthorityNamespaceID,OrderingProviderAssigningAuthorityUniversalID,OrderingProviderAssigningAuthorityIDType,OrderingProviderNameTypeCode,OrderingProviderIdentifierTypeCode,SetIdOBR,UsiID,UsiText,UsiCodeSystemName,UsiIDAlt,UsiTextAlt,UsiCodeSystemNameAlt,UsiTextOriginal,ObservationDateTimeStart,ObservationDateTimeEnd,SpecimenActionCode,ResultDateTime,ResultStatus,ParentObservationID,ParentObservationText,ParentObservationCodeSystemName,ParentObservationIDAlt,ParentObservationTextAlt,ParentObservationCodeSystemNameAlt,ParentObservationTextOriginal,ParentObservationSubID,ParentPlacerOrderNum,ParentPlacerOrderNamespace,ParentPlacerOrderUniversalID,ParentPlacerOrderUniversalIDType,ParentFillerOrderNum,ParentFillerOrderNamespace,ParentFillerOrderUniversalID,ParentFillerOrderUniversalIDType,ListEhrLabResultsHandlingF,ListEhrLabResultsHandlingN,TQ1SetId,TQ1DateTimeStart,TQ1DateTimeEnd,IsCpoe,OriginalPIDSegment) VALUES(";

        command +=
            SOut.Long(ehrLab.PatNum) + ","
                                     + "'" + SOut.String(ehrLab.OrderControlCode.ToString()) + "',"
                                     + "'" + SOut.String(ehrLab.PlacerOrderNum) + "',"
                                     + "'" + SOut.String(ehrLab.PlacerOrderNamespace) + "',"
                                     + "'" + SOut.String(ehrLab.PlacerOrderUniversalID) + "',"
                                     + "'" + SOut.String(ehrLab.PlacerOrderUniversalIDType) + "',"
                                     + "'" + SOut.String(ehrLab.FillerOrderNum) + "',"
                                     + "'" + SOut.String(ehrLab.FillerOrderNamespace) + "',"
                                     + "'" + SOut.String(ehrLab.FillerOrderUniversalID) + "',"
                                     + "'" + SOut.String(ehrLab.FillerOrderUniversalIDType) + "',"
                                     + "'" + SOut.String(ehrLab.PlacerGroupNum) + "',"
                                     + "'" + SOut.String(ehrLab.PlacerGroupNamespace) + "',"
                                     + "'" + SOut.String(ehrLab.PlacerGroupUniversalID) + "',"
                                     + "'" + SOut.String(ehrLab.PlacerGroupUniversalIDType) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderID) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderLName) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderFName) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderMiddleNames) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderSuffix) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderPrefix) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderAssigningAuthorityNamespaceID) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderAssigningAuthorityUniversalID) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderAssigningAuthorityIDType) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderNameTypeCode.ToString()) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderIdentifierTypeCode.ToString()) + "',"
                                     + SOut.Long(ehrLab.SetIdOBR) + ","
                                     + "'" + SOut.String(ehrLab.UsiID) + "',"
                                     + "'" + SOut.String(ehrLab.UsiText) + "',"
                                     + "'" + SOut.String(ehrLab.UsiCodeSystemName) + "',"
                                     + "'" + SOut.String(ehrLab.UsiIDAlt) + "',"
                                     + "'" + SOut.String(ehrLab.UsiTextAlt) + "',"
                                     + "'" + SOut.String(ehrLab.UsiCodeSystemNameAlt) + "',"
                                     + "'" + SOut.String(ehrLab.UsiTextOriginal) + "',"
                                     + "'" + SOut.String(ehrLab.ObservationDateTimeStart) + "',"
                                     + "'" + SOut.String(ehrLab.ObservationDateTimeEnd) + "',"
                                     + "'" + SOut.String(ehrLab.SpecimenActionCode.ToString()) + "',"
                                     + "'" + SOut.String(ehrLab.ResultDateTime) + "',"
                                     + "'" + SOut.String(ehrLab.ResultStatus.ToString()) + "',"
                                     + "'" + SOut.String(ehrLab.ParentObservationID) + "',"
                                     + "'" + SOut.String(ehrLab.ParentObservationText) + "',"
                                     + "'" + SOut.String(ehrLab.ParentObservationCodeSystemName) + "',"
                                     + "'" + SOut.String(ehrLab.ParentObservationIDAlt) + "',"
                                     + "'" + SOut.String(ehrLab.ParentObservationTextAlt) + "',"
                                     + "'" + SOut.String(ehrLab.ParentObservationCodeSystemNameAlt) + "',"
                                     + "'" + SOut.String(ehrLab.ParentObservationTextOriginal) + "',"
                                     + "'" + SOut.String(ehrLab.ParentObservationSubID) + "',"
                                     + "'" + SOut.String(ehrLab.ParentPlacerOrderNum) + "',"
                                     + "'" + SOut.String(ehrLab.ParentPlacerOrderNamespace) + "',"
                                     + "'" + SOut.String(ehrLab.ParentPlacerOrderUniversalID) + "',"
                                     + "'" + SOut.String(ehrLab.ParentPlacerOrderUniversalIDType) + "',"
                                     + "'" + SOut.String(ehrLab.ParentFillerOrderNum) + "',"
                                     + "'" + SOut.String(ehrLab.ParentFillerOrderNamespace) + "',"
                                     + "'" + SOut.String(ehrLab.ParentFillerOrderUniversalID) + "',"
                                     + "'" + SOut.String(ehrLab.ParentFillerOrderUniversalIDType) + "',"
                                     + SOut.Bool(ehrLab.ListEhrLabResultsHandlingF) + ","
                                     + SOut.Bool(ehrLab.ListEhrLabResultsHandlingN) + ","
                                     + SOut.Long(ehrLab.TQ1SetId) + ","
                                     + "'" + SOut.String(ehrLab.TQ1DateTimeStart) + "',"
                                     + "'" + SOut.String(ehrLab.TQ1DateTimeEnd) + "',"
                                     + SOut.Bool(ehrLab.IsCpoe) + ","
                                     + DbHelper.ParamChar + "paramOriginalPIDSegment)";
        if (ehrLab.OriginalPIDSegment == null) ehrLab.OriginalPIDSegment = "";
        var paramOriginalPIDSegment = new OdSqlParameter("paramOriginalPIDSegment", OdDbType.Text, SOut.StringParam(ehrLab.OriginalPIDSegment));
        {
            ehrLab.EhrLabNum = Db.NonQ(command, true, "EhrLabNum", "ehrLab", paramOriginalPIDSegment);
        }
        return ehrLab.EhrLabNum;
    }


    public static long InsertNoCache(EhrLab ehrLab)
    {
        return InsertNoCache(ehrLab, false);
    }


    public static long InsertNoCache(EhrLab ehrLab, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrlab (";
        if (isRandomKeys || useExistingPK) command += "EhrLabNum,";
        command += "PatNum,OrderControlCode,PlacerOrderNum,PlacerOrderNamespace,PlacerOrderUniversalID,PlacerOrderUniversalIDType,FillerOrderNum,FillerOrderNamespace,FillerOrderUniversalID,FillerOrderUniversalIDType,PlacerGroupNum,PlacerGroupNamespace,PlacerGroupUniversalID,PlacerGroupUniversalIDType,OrderingProviderID,OrderingProviderLName,OrderingProviderFName,OrderingProviderMiddleNames,OrderingProviderSuffix,OrderingProviderPrefix,OrderingProviderAssigningAuthorityNamespaceID,OrderingProviderAssigningAuthorityUniversalID,OrderingProviderAssigningAuthorityIDType,OrderingProviderNameTypeCode,OrderingProviderIdentifierTypeCode,SetIdOBR,UsiID,UsiText,UsiCodeSystemName,UsiIDAlt,UsiTextAlt,UsiCodeSystemNameAlt,UsiTextOriginal,ObservationDateTimeStart,ObservationDateTimeEnd,SpecimenActionCode,ResultDateTime,ResultStatus,ParentObservationID,ParentObservationText,ParentObservationCodeSystemName,ParentObservationIDAlt,ParentObservationTextAlt,ParentObservationCodeSystemNameAlt,ParentObservationTextOriginal,ParentObservationSubID,ParentPlacerOrderNum,ParentPlacerOrderNamespace,ParentPlacerOrderUniversalID,ParentPlacerOrderUniversalIDType,ParentFillerOrderNum,ParentFillerOrderNamespace,ParentFillerOrderUniversalID,ParentFillerOrderUniversalIDType,ListEhrLabResultsHandlingF,ListEhrLabResultsHandlingN,TQ1SetId,TQ1DateTimeStart,TQ1DateTimeEnd,IsCpoe,OriginalPIDSegment) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrLab.EhrLabNum) + ",";
        command +=
            SOut.Long(ehrLab.PatNum) + ","
                                     + "'" + SOut.String(ehrLab.OrderControlCode.ToString()) + "',"
                                     + "'" + SOut.String(ehrLab.PlacerOrderNum) + "',"
                                     + "'" + SOut.String(ehrLab.PlacerOrderNamespace) + "',"
                                     + "'" + SOut.String(ehrLab.PlacerOrderUniversalID) + "',"
                                     + "'" + SOut.String(ehrLab.PlacerOrderUniversalIDType) + "',"
                                     + "'" + SOut.String(ehrLab.FillerOrderNum) + "',"
                                     + "'" + SOut.String(ehrLab.FillerOrderNamespace) + "',"
                                     + "'" + SOut.String(ehrLab.FillerOrderUniversalID) + "',"
                                     + "'" + SOut.String(ehrLab.FillerOrderUniversalIDType) + "',"
                                     + "'" + SOut.String(ehrLab.PlacerGroupNum) + "',"
                                     + "'" + SOut.String(ehrLab.PlacerGroupNamespace) + "',"
                                     + "'" + SOut.String(ehrLab.PlacerGroupUniversalID) + "',"
                                     + "'" + SOut.String(ehrLab.PlacerGroupUniversalIDType) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderID) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderLName) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderFName) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderMiddleNames) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderSuffix) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderPrefix) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderAssigningAuthorityNamespaceID) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderAssigningAuthorityUniversalID) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderAssigningAuthorityIDType) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderNameTypeCode.ToString()) + "',"
                                     + "'" + SOut.String(ehrLab.OrderingProviderIdentifierTypeCode.ToString()) + "',"
                                     + SOut.Long(ehrLab.SetIdOBR) + ","
                                     + "'" + SOut.String(ehrLab.UsiID) + "',"
                                     + "'" + SOut.String(ehrLab.UsiText) + "',"
                                     + "'" + SOut.String(ehrLab.UsiCodeSystemName) + "',"
                                     + "'" + SOut.String(ehrLab.UsiIDAlt) + "',"
                                     + "'" + SOut.String(ehrLab.UsiTextAlt) + "',"
                                     + "'" + SOut.String(ehrLab.UsiCodeSystemNameAlt) + "',"
                                     + "'" + SOut.String(ehrLab.UsiTextOriginal) + "',"
                                     + "'" + SOut.String(ehrLab.ObservationDateTimeStart) + "',"
                                     + "'" + SOut.String(ehrLab.ObservationDateTimeEnd) + "',"
                                     + "'" + SOut.String(ehrLab.SpecimenActionCode.ToString()) + "',"
                                     + "'" + SOut.String(ehrLab.ResultDateTime) + "',"
                                     + "'" + SOut.String(ehrLab.ResultStatus.ToString()) + "',"
                                     + "'" + SOut.String(ehrLab.ParentObservationID) + "',"
                                     + "'" + SOut.String(ehrLab.ParentObservationText) + "',"
                                     + "'" + SOut.String(ehrLab.ParentObservationCodeSystemName) + "',"
                                     + "'" + SOut.String(ehrLab.ParentObservationIDAlt) + "',"
                                     + "'" + SOut.String(ehrLab.ParentObservationTextAlt) + "',"
                                     + "'" + SOut.String(ehrLab.ParentObservationCodeSystemNameAlt) + "',"
                                     + "'" + SOut.String(ehrLab.ParentObservationTextOriginal) + "',"
                                     + "'" + SOut.String(ehrLab.ParentObservationSubID) + "',"
                                     + "'" + SOut.String(ehrLab.ParentPlacerOrderNum) + "',"
                                     + "'" + SOut.String(ehrLab.ParentPlacerOrderNamespace) + "',"
                                     + "'" + SOut.String(ehrLab.ParentPlacerOrderUniversalID) + "',"
                                     + "'" + SOut.String(ehrLab.ParentPlacerOrderUniversalIDType) + "',"
                                     + "'" + SOut.String(ehrLab.ParentFillerOrderNum) + "',"
                                     + "'" + SOut.String(ehrLab.ParentFillerOrderNamespace) + "',"
                                     + "'" + SOut.String(ehrLab.ParentFillerOrderUniversalID) + "',"
                                     + "'" + SOut.String(ehrLab.ParentFillerOrderUniversalIDType) + "',"
                                     + SOut.Bool(ehrLab.ListEhrLabResultsHandlingF) + ","
                                     + SOut.Bool(ehrLab.ListEhrLabResultsHandlingN) + ","
                                     + SOut.Long(ehrLab.TQ1SetId) + ","
                                     + "'" + SOut.String(ehrLab.TQ1DateTimeStart) + "',"
                                     + "'" + SOut.String(ehrLab.TQ1DateTimeEnd) + "',"
                                     + SOut.Bool(ehrLab.IsCpoe) + ","
                                     + DbHelper.ParamChar + "paramOriginalPIDSegment)";
        if (ehrLab.OriginalPIDSegment == null) ehrLab.OriginalPIDSegment = "";
        var paramOriginalPIDSegment = new OdSqlParameter("paramOriginalPIDSegment", OdDbType.Text, SOut.StringParam(ehrLab.OriginalPIDSegment));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramOriginalPIDSegment);
        else
            ehrLab.EhrLabNum = Db.NonQ(command, true, "EhrLabNum", "ehrLab", paramOriginalPIDSegment);
        return ehrLab.EhrLabNum;
    }


    public static void Update(EhrLab ehrLab)
    {
        var command = "UPDATE ehrlab SET "
                      + "PatNum                                       =  " + SOut.Long(ehrLab.PatNum) + ", "
                      + "OrderControlCode                             = '" + SOut.String(ehrLab.OrderControlCode.ToString()) + "', "
                      + "PlacerOrderNum                               = '" + SOut.String(ehrLab.PlacerOrderNum) + "', "
                      + "PlacerOrderNamespace                         = '" + SOut.String(ehrLab.PlacerOrderNamespace) + "', "
                      + "PlacerOrderUniversalID                       = '" + SOut.String(ehrLab.PlacerOrderUniversalID) + "', "
                      + "PlacerOrderUniversalIDType                   = '" + SOut.String(ehrLab.PlacerOrderUniversalIDType) + "', "
                      + "FillerOrderNum                               = '" + SOut.String(ehrLab.FillerOrderNum) + "', "
                      + "FillerOrderNamespace                         = '" + SOut.String(ehrLab.FillerOrderNamespace) + "', "
                      + "FillerOrderUniversalID                       = '" + SOut.String(ehrLab.FillerOrderUniversalID) + "', "
                      + "FillerOrderUniversalIDType                   = '" + SOut.String(ehrLab.FillerOrderUniversalIDType) + "', "
                      + "PlacerGroupNum                               = '" + SOut.String(ehrLab.PlacerGroupNum) + "', "
                      + "PlacerGroupNamespace                         = '" + SOut.String(ehrLab.PlacerGroupNamespace) + "', "
                      + "PlacerGroupUniversalID                       = '" + SOut.String(ehrLab.PlacerGroupUniversalID) + "', "
                      + "PlacerGroupUniversalIDType                   = '" + SOut.String(ehrLab.PlacerGroupUniversalIDType) + "', "
                      + "OrderingProviderID                           = '" + SOut.String(ehrLab.OrderingProviderID) + "', "
                      + "OrderingProviderLName                        = '" + SOut.String(ehrLab.OrderingProviderLName) + "', "
                      + "OrderingProviderFName                        = '" + SOut.String(ehrLab.OrderingProviderFName) + "', "
                      + "OrderingProviderMiddleNames                  = '" + SOut.String(ehrLab.OrderingProviderMiddleNames) + "', "
                      + "OrderingProviderSuffix                       = '" + SOut.String(ehrLab.OrderingProviderSuffix) + "', "
                      + "OrderingProviderPrefix                       = '" + SOut.String(ehrLab.OrderingProviderPrefix) + "', "
                      + "OrderingProviderAssigningAuthorityNamespaceID= '" + SOut.String(ehrLab.OrderingProviderAssigningAuthorityNamespaceID) + "', "
                      + "OrderingProviderAssigningAuthorityUniversalID= '" + SOut.String(ehrLab.OrderingProviderAssigningAuthorityUniversalID) + "', "
                      + "OrderingProviderAssigningAuthorityIDType     = '" + SOut.String(ehrLab.OrderingProviderAssigningAuthorityIDType) + "', "
                      + "OrderingProviderNameTypeCode                 = '" + SOut.String(ehrLab.OrderingProviderNameTypeCode.ToString()) + "', "
                      + "OrderingProviderIdentifierTypeCode           = '" + SOut.String(ehrLab.OrderingProviderIdentifierTypeCode.ToString()) + "', "
                      + "SetIdOBR                                     =  " + SOut.Long(ehrLab.SetIdOBR) + ", "
                      + "UsiID                                        = '" + SOut.String(ehrLab.UsiID) + "', "
                      + "UsiText                                      = '" + SOut.String(ehrLab.UsiText) + "', "
                      + "UsiCodeSystemName                            = '" + SOut.String(ehrLab.UsiCodeSystemName) + "', "
                      + "UsiIDAlt                                     = '" + SOut.String(ehrLab.UsiIDAlt) + "', "
                      + "UsiTextAlt                                   = '" + SOut.String(ehrLab.UsiTextAlt) + "', "
                      + "UsiCodeSystemNameAlt                         = '" + SOut.String(ehrLab.UsiCodeSystemNameAlt) + "', "
                      + "UsiTextOriginal                              = '" + SOut.String(ehrLab.UsiTextOriginal) + "', "
                      + "ObservationDateTimeStart                     = '" + SOut.String(ehrLab.ObservationDateTimeStart) + "', "
                      + "ObservationDateTimeEnd                       = '" + SOut.String(ehrLab.ObservationDateTimeEnd) + "', "
                      + "SpecimenActionCode                           = '" + SOut.String(ehrLab.SpecimenActionCode.ToString()) + "', "
                      + "ResultDateTime                               = '" + SOut.String(ehrLab.ResultDateTime) + "', "
                      + "ResultStatus                                 = '" + SOut.String(ehrLab.ResultStatus.ToString()) + "', "
                      + "ParentObservationID                          = '" + SOut.String(ehrLab.ParentObservationID) + "', "
                      + "ParentObservationText                        = '" + SOut.String(ehrLab.ParentObservationText) + "', "
                      + "ParentObservationCodeSystemName              = '" + SOut.String(ehrLab.ParentObservationCodeSystemName) + "', "
                      + "ParentObservationIDAlt                       = '" + SOut.String(ehrLab.ParentObservationIDAlt) + "', "
                      + "ParentObservationTextAlt                     = '" + SOut.String(ehrLab.ParentObservationTextAlt) + "', "
                      + "ParentObservationCodeSystemNameAlt           = '" + SOut.String(ehrLab.ParentObservationCodeSystemNameAlt) + "', "
                      + "ParentObservationTextOriginal                = '" + SOut.String(ehrLab.ParentObservationTextOriginal) + "', "
                      + "ParentObservationSubID                       = '" + SOut.String(ehrLab.ParentObservationSubID) + "', "
                      + "ParentPlacerOrderNum                         = '" + SOut.String(ehrLab.ParentPlacerOrderNum) + "', "
                      + "ParentPlacerOrderNamespace                   = '" + SOut.String(ehrLab.ParentPlacerOrderNamespace) + "', "
                      + "ParentPlacerOrderUniversalID                 = '" + SOut.String(ehrLab.ParentPlacerOrderUniversalID) + "', "
                      + "ParentPlacerOrderUniversalIDType             = '" + SOut.String(ehrLab.ParentPlacerOrderUniversalIDType) + "', "
                      + "ParentFillerOrderNum                         = '" + SOut.String(ehrLab.ParentFillerOrderNum) + "', "
                      + "ParentFillerOrderNamespace                   = '" + SOut.String(ehrLab.ParentFillerOrderNamespace) + "', "
                      + "ParentFillerOrderUniversalID                 = '" + SOut.String(ehrLab.ParentFillerOrderUniversalID) + "', "
                      + "ParentFillerOrderUniversalIDType             = '" + SOut.String(ehrLab.ParentFillerOrderUniversalIDType) + "', "
                      + "ListEhrLabResultsHandlingF                   =  " + SOut.Bool(ehrLab.ListEhrLabResultsHandlingF) + ", "
                      + "ListEhrLabResultsHandlingN                   =  " + SOut.Bool(ehrLab.ListEhrLabResultsHandlingN) + ", "
                      + "TQ1SetId                                     =  " + SOut.Long(ehrLab.TQ1SetId) + ", "
                      + "TQ1DateTimeStart                             = '" + SOut.String(ehrLab.TQ1DateTimeStart) + "', "
                      + "TQ1DateTimeEnd                               = '" + SOut.String(ehrLab.TQ1DateTimeEnd) + "', "
                      + "IsCpoe                                       =  " + SOut.Bool(ehrLab.IsCpoe) + ", "
                      + "OriginalPIDSegment                           =  " + DbHelper.ParamChar + "paramOriginalPIDSegment "
                      + "WHERE EhrLabNum = " + SOut.Long(ehrLab.EhrLabNum);
        if (ehrLab.OriginalPIDSegment == null) ehrLab.OriginalPIDSegment = "";
        var paramOriginalPIDSegment = new OdSqlParameter("paramOriginalPIDSegment", OdDbType.Text, SOut.StringParam(ehrLab.OriginalPIDSegment));
        Db.NonQ(command, paramOriginalPIDSegment);
    }


    public static bool Update(EhrLab ehrLab, EhrLab oldEhrLab)
    {
        var command = "";
        if (ehrLab.PatNum != oldEhrLab.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(ehrLab.PatNum) + "";
        }

        if (ehrLab.OrderControlCode != oldEhrLab.OrderControlCode)
        {
            if (command != "") command += ",";
            command += "OrderControlCode = '" + SOut.String(ehrLab.OrderControlCode.ToString()) + "'";
        }

        if (ehrLab.PlacerOrderNum != oldEhrLab.PlacerOrderNum)
        {
            if (command != "") command += ",";
            command += "PlacerOrderNum = '" + SOut.String(ehrLab.PlacerOrderNum) + "'";
        }

        if (ehrLab.PlacerOrderNamespace != oldEhrLab.PlacerOrderNamespace)
        {
            if (command != "") command += ",";
            command += "PlacerOrderNamespace = '" + SOut.String(ehrLab.PlacerOrderNamespace) + "'";
        }

        if (ehrLab.PlacerOrderUniversalID != oldEhrLab.PlacerOrderUniversalID)
        {
            if (command != "") command += ",";
            command += "PlacerOrderUniversalID = '" + SOut.String(ehrLab.PlacerOrderUniversalID) + "'";
        }

        if (ehrLab.PlacerOrderUniversalIDType != oldEhrLab.PlacerOrderUniversalIDType)
        {
            if (command != "") command += ",";
            command += "PlacerOrderUniversalIDType = '" + SOut.String(ehrLab.PlacerOrderUniversalIDType) + "'";
        }

        if (ehrLab.FillerOrderNum != oldEhrLab.FillerOrderNum)
        {
            if (command != "") command += ",";
            command += "FillerOrderNum = '" + SOut.String(ehrLab.FillerOrderNum) + "'";
        }

        if (ehrLab.FillerOrderNamespace != oldEhrLab.FillerOrderNamespace)
        {
            if (command != "") command += ",";
            command += "FillerOrderNamespace = '" + SOut.String(ehrLab.FillerOrderNamespace) + "'";
        }

        if (ehrLab.FillerOrderUniversalID != oldEhrLab.FillerOrderUniversalID)
        {
            if (command != "") command += ",";
            command += "FillerOrderUniversalID = '" + SOut.String(ehrLab.FillerOrderUniversalID) + "'";
        }

        if (ehrLab.FillerOrderUniversalIDType != oldEhrLab.FillerOrderUniversalIDType)
        {
            if (command != "") command += ",";
            command += "FillerOrderUniversalIDType = '" + SOut.String(ehrLab.FillerOrderUniversalIDType) + "'";
        }

        if (ehrLab.PlacerGroupNum != oldEhrLab.PlacerGroupNum)
        {
            if (command != "") command += ",";
            command += "PlacerGroupNum = '" + SOut.String(ehrLab.PlacerGroupNum) + "'";
        }

        if (ehrLab.PlacerGroupNamespace != oldEhrLab.PlacerGroupNamespace)
        {
            if (command != "") command += ",";
            command += "PlacerGroupNamespace = '" + SOut.String(ehrLab.PlacerGroupNamespace) + "'";
        }

        if (ehrLab.PlacerGroupUniversalID != oldEhrLab.PlacerGroupUniversalID)
        {
            if (command != "") command += ",";
            command += "PlacerGroupUniversalID = '" + SOut.String(ehrLab.PlacerGroupUniversalID) + "'";
        }

        if (ehrLab.PlacerGroupUniversalIDType != oldEhrLab.PlacerGroupUniversalIDType)
        {
            if (command != "") command += ",";
            command += "PlacerGroupUniversalIDType = '" + SOut.String(ehrLab.PlacerGroupUniversalIDType) + "'";
        }

        if (ehrLab.OrderingProviderID != oldEhrLab.OrderingProviderID)
        {
            if (command != "") command += ",";
            command += "OrderingProviderID = '" + SOut.String(ehrLab.OrderingProviderID) + "'";
        }

        if (ehrLab.OrderingProviderLName != oldEhrLab.OrderingProviderLName)
        {
            if (command != "") command += ",";
            command += "OrderingProviderLName = '" + SOut.String(ehrLab.OrderingProviderLName) + "'";
        }

        if (ehrLab.OrderingProviderFName != oldEhrLab.OrderingProviderFName)
        {
            if (command != "") command += ",";
            command += "OrderingProviderFName = '" + SOut.String(ehrLab.OrderingProviderFName) + "'";
        }

        if (ehrLab.OrderingProviderMiddleNames != oldEhrLab.OrderingProviderMiddleNames)
        {
            if (command != "") command += ",";
            command += "OrderingProviderMiddleNames = '" + SOut.String(ehrLab.OrderingProviderMiddleNames) + "'";
        }

        if (ehrLab.OrderingProviderSuffix != oldEhrLab.OrderingProviderSuffix)
        {
            if (command != "") command += ",";
            command += "OrderingProviderSuffix = '" + SOut.String(ehrLab.OrderingProviderSuffix) + "'";
        }

        if (ehrLab.OrderingProviderPrefix != oldEhrLab.OrderingProviderPrefix)
        {
            if (command != "") command += ",";
            command += "OrderingProviderPrefix = '" + SOut.String(ehrLab.OrderingProviderPrefix) + "'";
        }

        if (ehrLab.OrderingProviderAssigningAuthorityNamespaceID != oldEhrLab.OrderingProviderAssigningAuthorityNamespaceID)
        {
            if (command != "") command += ",";
            command += "OrderingProviderAssigningAuthorityNamespaceID = '" + SOut.String(ehrLab.OrderingProviderAssigningAuthorityNamespaceID) + "'";
        }

        if (ehrLab.OrderingProviderAssigningAuthorityUniversalID != oldEhrLab.OrderingProviderAssigningAuthorityUniversalID)
        {
            if (command != "") command += ",";
            command += "OrderingProviderAssigningAuthorityUniversalID = '" + SOut.String(ehrLab.OrderingProviderAssigningAuthorityUniversalID) + "'";
        }

        if (ehrLab.OrderingProviderAssigningAuthorityIDType != oldEhrLab.OrderingProviderAssigningAuthorityIDType)
        {
            if (command != "") command += ",";
            command += "OrderingProviderAssigningAuthorityIDType = '" + SOut.String(ehrLab.OrderingProviderAssigningAuthorityIDType) + "'";
        }

        if (ehrLab.OrderingProviderNameTypeCode != oldEhrLab.OrderingProviderNameTypeCode)
        {
            if (command != "") command += ",";
            command += "OrderingProviderNameTypeCode = '" + SOut.String(ehrLab.OrderingProviderNameTypeCode.ToString()) + "'";
        }

        if (ehrLab.OrderingProviderIdentifierTypeCode != oldEhrLab.OrderingProviderIdentifierTypeCode)
        {
            if (command != "") command += ",";
            command += "OrderingProviderIdentifierTypeCode = '" + SOut.String(ehrLab.OrderingProviderIdentifierTypeCode.ToString()) + "'";
        }

        if (ehrLab.SetIdOBR != oldEhrLab.SetIdOBR)
        {
            if (command != "") command += ",";
            command += "SetIdOBR = " + SOut.Long(ehrLab.SetIdOBR) + "";
        }

        if (ehrLab.UsiID != oldEhrLab.UsiID)
        {
            if (command != "") command += ",";
            command += "UsiID = '" + SOut.String(ehrLab.UsiID) + "'";
        }

        if (ehrLab.UsiText != oldEhrLab.UsiText)
        {
            if (command != "") command += ",";
            command += "UsiText = '" + SOut.String(ehrLab.UsiText) + "'";
        }

        if (ehrLab.UsiCodeSystemName != oldEhrLab.UsiCodeSystemName)
        {
            if (command != "") command += ",";
            command += "UsiCodeSystemName = '" + SOut.String(ehrLab.UsiCodeSystemName) + "'";
        }

        if (ehrLab.UsiIDAlt != oldEhrLab.UsiIDAlt)
        {
            if (command != "") command += ",";
            command += "UsiIDAlt = '" + SOut.String(ehrLab.UsiIDAlt) + "'";
        }

        if (ehrLab.UsiTextAlt != oldEhrLab.UsiTextAlt)
        {
            if (command != "") command += ",";
            command += "UsiTextAlt = '" + SOut.String(ehrLab.UsiTextAlt) + "'";
        }

        if (ehrLab.UsiCodeSystemNameAlt != oldEhrLab.UsiCodeSystemNameAlt)
        {
            if (command != "") command += ",";
            command += "UsiCodeSystemNameAlt = '" + SOut.String(ehrLab.UsiCodeSystemNameAlt) + "'";
        }

        if (ehrLab.UsiTextOriginal != oldEhrLab.UsiTextOriginal)
        {
            if (command != "") command += ",";
            command += "UsiTextOriginal = '" + SOut.String(ehrLab.UsiTextOriginal) + "'";
        }

        if (ehrLab.ObservationDateTimeStart != oldEhrLab.ObservationDateTimeStart)
        {
            if (command != "") command += ",";
            command += "ObservationDateTimeStart = '" + SOut.String(ehrLab.ObservationDateTimeStart) + "'";
        }

        if (ehrLab.ObservationDateTimeEnd != oldEhrLab.ObservationDateTimeEnd)
        {
            if (command != "") command += ",";
            command += "ObservationDateTimeEnd = '" + SOut.String(ehrLab.ObservationDateTimeEnd) + "'";
        }

        if (ehrLab.SpecimenActionCode != oldEhrLab.SpecimenActionCode)
        {
            if (command != "") command += ",";
            command += "SpecimenActionCode = '" + SOut.String(ehrLab.SpecimenActionCode.ToString()) + "'";
        }

        if (ehrLab.ResultDateTime != oldEhrLab.ResultDateTime)
        {
            if (command != "") command += ",";
            command += "ResultDateTime = '" + SOut.String(ehrLab.ResultDateTime) + "'";
        }

        if (ehrLab.ResultStatus != oldEhrLab.ResultStatus)
        {
            if (command != "") command += ",";
            command += "ResultStatus = '" + SOut.String(ehrLab.ResultStatus.ToString()) + "'";
        }

        if (ehrLab.ParentObservationID != oldEhrLab.ParentObservationID)
        {
            if (command != "") command += ",";
            command += "ParentObservationID = '" + SOut.String(ehrLab.ParentObservationID) + "'";
        }

        if (ehrLab.ParentObservationText != oldEhrLab.ParentObservationText)
        {
            if (command != "") command += ",";
            command += "ParentObservationText = '" + SOut.String(ehrLab.ParentObservationText) + "'";
        }

        if (ehrLab.ParentObservationCodeSystemName != oldEhrLab.ParentObservationCodeSystemName)
        {
            if (command != "") command += ",";
            command += "ParentObservationCodeSystemName = '" + SOut.String(ehrLab.ParentObservationCodeSystemName) + "'";
        }

        if (ehrLab.ParentObservationIDAlt != oldEhrLab.ParentObservationIDAlt)
        {
            if (command != "") command += ",";
            command += "ParentObservationIDAlt = '" + SOut.String(ehrLab.ParentObservationIDAlt) + "'";
        }

        if (ehrLab.ParentObservationTextAlt != oldEhrLab.ParentObservationTextAlt)
        {
            if (command != "") command += ",";
            command += "ParentObservationTextAlt = '" + SOut.String(ehrLab.ParentObservationTextAlt) + "'";
        }

        if (ehrLab.ParentObservationCodeSystemNameAlt != oldEhrLab.ParentObservationCodeSystemNameAlt)
        {
            if (command != "") command += ",";
            command += "ParentObservationCodeSystemNameAlt = '" + SOut.String(ehrLab.ParentObservationCodeSystemNameAlt) + "'";
        }

        if (ehrLab.ParentObservationTextOriginal != oldEhrLab.ParentObservationTextOriginal)
        {
            if (command != "") command += ",";
            command += "ParentObservationTextOriginal = '" + SOut.String(ehrLab.ParentObservationTextOriginal) + "'";
        }

        if (ehrLab.ParentObservationSubID != oldEhrLab.ParentObservationSubID)
        {
            if (command != "") command += ",";
            command += "ParentObservationSubID = '" + SOut.String(ehrLab.ParentObservationSubID) + "'";
        }

        if (ehrLab.ParentPlacerOrderNum != oldEhrLab.ParentPlacerOrderNum)
        {
            if (command != "") command += ",";
            command += "ParentPlacerOrderNum = '" + SOut.String(ehrLab.ParentPlacerOrderNum) + "'";
        }

        if (ehrLab.ParentPlacerOrderNamespace != oldEhrLab.ParentPlacerOrderNamespace)
        {
            if (command != "") command += ",";
            command += "ParentPlacerOrderNamespace = '" + SOut.String(ehrLab.ParentPlacerOrderNamespace) + "'";
        }

        if (ehrLab.ParentPlacerOrderUniversalID != oldEhrLab.ParentPlacerOrderUniversalID)
        {
            if (command != "") command += ",";
            command += "ParentPlacerOrderUniversalID = '" + SOut.String(ehrLab.ParentPlacerOrderUniversalID) + "'";
        }

        if (ehrLab.ParentPlacerOrderUniversalIDType != oldEhrLab.ParentPlacerOrderUniversalIDType)
        {
            if (command != "") command += ",";
            command += "ParentPlacerOrderUniversalIDType = '" + SOut.String(ehrLab.ParentPlacerOrderUniversalIDType) + "'";
        }

        if (ehrLab.ParentFillerOrderNum != oldEhrLab.ParentFillerOrderNum)
        {
            if (command != "") command += ",";
            command += "ParentFillerOrderNum = '" + SOut.String(ehrLab.ParentFillerOrderNum) + "'";
        }

        if (ehrLab.ParentFillerOrderNamespace != oldEhrLab.ParentFillerOrderNamespace)
        {
            if (command != "") command += ",";
            command += "ParentFillerOrderNamespace = '" + SOut.String(ehrLab.ParentFillerOrderNamespace) + "'";
        }

        if (ehrLab.ParentFillerOrderUniversalID != oldEhrLab.ParentFillerOrderUniversalID)
        {
            if (command != "") command += ",";
            command += "ParentFillerOrderUniversalID = '" + SOut.String(ehrLab.ParentFillerOrderUniversalID) + "'";
        }

        if (ehrLab.ParentFillerOrderUniversalIDType != oldEhrLab.ParentFillerOrderUniversalIDType)
        {
            if (command != "") command += ",";
            command += "ParentFillerOrderUniversalIDType = '" + SOut.String(ehrLab.ParentFillerOrderUniversalIDType) + "'";
        }

        if (ehrLab.ListEhrLabResultsHandlingF != oldEhrLab.ListEhrLabResultsHandlingF)
        {
            if (command != "") command += ",";
            command += "ListEhrLabResultsHandlingF = " + SOut.Bool(ehrLab.ListEhrLabResultsHandlingF) + "";
        }

        if (ehrLab.ListEhrLabResultsHandlingN != oldEhrLab.ListEhrLabResultsHandlingN)
        {
            if (command != "") command += ",";
            command += "ListEhrLabResultsHandlingN = " + SOut.Bool(ehrLab.ListEhrLabResultsHandlingN) + "";
        }

        if (ehrLab.TQ1SetId != oldEhrLab.TQ1SetId)
        {
            if (command != "") command += ",";
            command += "TQ1SetId = " + SOut.Long(ehrLab.TQ1SetId) + "";
        }

        if (ehrLab.TQ1DateTimeStart != oldEhrLab.TQ1DateTimeStart)
        {
            if (command != "") command += ",";
            command += "TQ1DateTimeStart = '" + SOut.String(ehrLab.TQ1DateTimeStart) + "'";
        }

        if (ehrLab.TQ1DateTimeEnd != oldEhrLab.TQ1DateTimeEnd)
        {
            if (command != "") command += ",";
            command += "TQ1DateTimeEnd = '" + SOut.String(ehrLab.TQ1DateTimeEnd) + "'";
        }

        if (ehrLab.IsCpoe != oldEhrLab.IsCpoe)
        {
            if (command != "") command += ",";
            command += "IsCpoe = " + SOut.Bool(ehrLab.IsCpoe) + "";
        }

        if (ehrLab.OriginalPIDSegment != oldEhrLab.OriginalPIDSegment)
        {
            if (command != "") command += ",";
            command += "OriginalPIDSegment = " + DbHelper.ParamChar + "paramOriginalPIDSegment";
        }

        if (command == "") return false;
        if (ehrLab.OriginalPIDSegment == null) ehrLab.OriginalPIDSegment = "";
        var paramOriginalPIDSegment = new OdSqlParameter("paramOriginalPIDSegment", OdDbType.Text, SOut.StringParam(ehrLab.OriginalPIDSegment));
        command = "UPDATE ehrlab SET " + command
                                       + " WHERE EhrLabNum = " + SOut.Long(ehrLab.EhrLabNum);
        Db.NonQ(command, paramOriginalPIDSegment);
        return true;
    }


    public static bool UpdateComparison(EhrLab ehrLab, EhrLab oldEhrLab)
    {
        if (ehrLab.PatNum != oldEhrLab.PatNum) return true;
        if (ehrLab.OrderControlCode != oldEhrLab.OrderControlCode) return true;
        if (ehrLab.PlacerOrderNum != oldEhrLab.PlacerOrderNum) return true;
        if (ehrLab.PlacerOrderNamespace != oldEhrLab.PlacerOrderNamespace) return true;
        if (ehrLab.PlacerOrderUniversalID != oldEhrLab.PlacerOrderUniversalID) return true;
        if (ehrLab.PlacerOrderUniversalIDType != oldEhrLab.PlacerOrderUniversalIDType) return true;
        if (ehrLab.FillerOrderNum != oldEhrLab.FillerOrderNum) return true;
        if (ehrLab.FillerOrderNamespace != oldEhrLab.FillerOrderNamespace) return true;
        if (ehrLab.FillerOrderUniversalID != oldEhrLab.FillerOrderUniversalID) return true;
        if (ehrLab.FillerOrderUniversalIDType != oldEhrLab.FillerOrderUniversalIDType) return true;
        if (ehrLab.PlacerGroupNum != oldEhrLab.PlacerGroupNum) return true;
        if (ehrLab.PlacerGroupNamespace != oldEhrLab.PlacerGroupNamespace) return true;
        if (ehrLab.PlacerGroupUniversalID != oldEhrLab.PlacerGroupUniversalID) return true;
        if (ehrLab.PlacerGroupUniversalIDType != oldEhrLab.PlacerGroupUniversalIDType) return true;
        if (ehrLab.OrderingProviderID != oldEhrLab.OrderingProviderID) return true;
        if (ehrLab.OrderingProviderLName != oldEhrLab.OrderingProviderLName) return true;
        if (ehrLab.OrderingProviderFName != oldEhrLab.OrderingProviderFName) return true;
        if (ehrLab.OrderingProviderMiddleNames != oldEhrLab.OrderingProviderMiddleNames) return true;
        if (ehrLab.OrderingProviderSuffix != oldEhrLab.OrderingProviderSuffix) return true;
        if (ehrLab.OrderingProviderPrefix != oldEhrLab.OrderingProviderPrefix) return true;
        if (ehrLab.OrderingProviderAssigningAuthorityNamespaceID != oldEhrLab.OrderingProviderAssigningAuthorityNamespaceID) return true;
        if (ehrLab.OrderingProviderAssigningAuthorityUniversalID != oldEhrLab.OrderingProviderAssigningAuthorityUniversalID) return true;
        if (ehrLab.OrderingProviderAssigningAuthorityIDType != oldEhrLab.OrderingProviderAssigningAuthorityIDType) return true;
        if (ehrLab.OrderingProviderNameTypeCode != oldEhrLab.OrderingProviderNameTypeCode) return true;
        if (ehrLab.OrderingProviderIdentifierTypeCode != oldEhrLab.OrderingProviderIdentifierTypeCode) return true;
        if (ehrLab.SetIdOBR != oldEhrLab.SetIdOBR) return true;
        if (ehrLab.UsiID != oldEhrLab.UsiID) return true;
        if (ehrLab.UsiText != oldEhrLab.UsiText) return true;
        if (ehrLab.UsiCodeSystemName != oldEhrLab.UsiCodeSystemName) return true;
        if (ehrLab.UsiIDAlt != oldEhrLab.UsiIDAlt) return true;
        if (ehrLab.UsiTextAlt != oldEhrLab.UsiTextAlt) return true;
        if (ehrLab.UsiCodeSystemNameAlt != oldEhrLab.UsiCodeSystemNameAlt) return true;
        if (ehrLab.UsiTextOriginal != oldEhrLab.UsiTextOriginal) return true;
        if (ehrLab.ObservationDateTimeStart != oldEhrLab.ObservationDateTimeStart) return true;
        if (ehrLab.ObservationDateTimeEnd != oldEhrLab.ObservationDateTimeEnd) return true;
        if (ehrLab.SpecimenActionCode != oldEhrLab.SpecimenActionCode) return true;
        if (ehrLab.ResultDateTime != oldEhrLab.ResultDateTime) return true;
        if (ehrLab.ResultStatus != oldEhrLab.ResultStatus) return true;
        if (ehrLab.ParentObservationID != oldEhrLab.ParentObservationID) return true;
        if (ehrLab.ParentObservationText != oldEhrLab.ParentObservationText) return true;
        if (ehrLab.ParentObservationCodeSystemName != oldEhrLab.ParentObservationCodeSystemName) return true;
        if (ehrLab.ParentObservationIDAlt != oldEhrLab.ParentObservationIDAlt) return true;
        if (ehrLab.ParentObservationTextAlt != oldEhrLab.ParentObservationTextAlt) return true;
        if (ehrLab.ParentObservationCodeSystemNameAlt != oldEhrLab.ParentObservationCodeSystemNameAlt) return true;
        if (ehrLab.ParentObservationTextOriginal != oldEhrLab.ParentObservationTextOriginal) return true;
        if (ehrLab.ParentObservationSubID != oldEhrLab.ParentObservationSubID) return true;
        if (ehrLab.ParentPlacerOrderNum != oldEhrLab.ParentPlacerOrderNum) return true;
        if (ehrLab.ParentPlacerOrderNamespace != oldEhrLab.ParentPlacerOrderNamespace) return true;
        if (ehrLab.ParentPlacerOrderUniversalID != oldEhrLab.ParentPlacerOrderUniversalID) return true;
        if (ehrLab.ParentPlacerOrderUniversalIDType != oldEhrLab.ParentPlacerOrderUniversalIDType) return true;
        if (ehrLab.ParentFillerOrderNum != oldEhrLab.ParentFillerOrderNum) return true;
        if (ehrLab.ParentFillerOrderNamespace != oldEhrLab.ParentFillerOrderNamespace) return true;
        if (ehrLab.ParentFillerOrderUniversalID != oldEhrLab.ParentFillerOrderUniversalID) return true;
        if (ehrLab.ParentFillerOrderUniversalIDType != oldEhrLab.ParentFillerOrderUniversalIDType) return true;
        if (ehrLab.ListEhrLabResultsHandlingF != oldEhrLab.ListEhrLabResultsHandlingF) return true;
        if (ehrLab.ListEhrLabResultsHandlingN != oldEhrLab.ListEhrLabResultsHandlingN) return true;
        if (ehrLab.TQ1SetId != oldEhrLab.TQ1SetId) return true;
        if (ehrLab.TQ1DateTimeStart != oldEhrLab.TQ1DateTimeStart) return true;
        if (ehrLab.TQ1DateTimeEnd != oldEhrLab.TQ1DateTimeEnd) return true;
        if (ehrLab.IsCpoe != oldEhrLab.IsCpoe) return true;
        if (ehrLab.OriginalPIDSegment != oldEhrLab.OriginalPIDSegment) return true;
        return false;
    }


    public static void Delete(long ehrLabNum)
    {
        var command = "DELETE FROM ehrlab "
                      + "WHERE EhrLabNum = " + SOut.Long(ehrLabNum);
        Db.NonQ(command);
    }


    public static void DeleteMany(List<long> listEhrLabNums)
    {
        if (listEhrLabNums == null || listEhrLabNums.Count == 0) return;
        var command = "DELETE FROM ehrlab "
                      + "WHERE EhrLabNum IN(" + string.Join(",", listEhrLabNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}