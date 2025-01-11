#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using EhrLaboratories;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrLabResultCrud
{
    public static EhrLabResult SelectOne(long ehrLabResultNum)
    {
        var command = "SELECT * FROM ehrlabresult "
                      + "WHERE EhrLabResultNum = " + SOut.Long(ehrLabResultNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrLabResult SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrLabResult> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrLabResult> TableToList(DataTable table)
    {
        var retVal = new List<EhrLabResult>();
        EhrLabResult ehrLabResult;
        foreach (DataRow row in table.Rows)
        {
            ehrLabResult = new EhrLabResult();
            ehrLabResult.EhrLabResultNum = SIn.Long(row["EhrLabResultNum"].ToString());
            ehrLabResult.EhrLabNum = SIn.Long(row["EhrLabNum"].ToString());
            ehrLabResult.SetIdOBX = SIn.Long(row["SetIdOBX"].ToString());
            var valueType = row["ValueType"].ToString();
            if (valueType == "")
                ehrLabResult.ValueType = 0;
            else
                try
                {
                    ehrLabResult.ValueType = (HL70125) Enum.Parse(typeof(HL70125), valueType);
                }
                catch
                {
                    ehrLabResult.ValueType = 0;
                }

            ehrLabResult.ObservationIdentifierID = SIn.String(row["ObservationIdentifierID"].ToString());
            ehrLabResult.ObservationIdentifierText = SIn.String(row["ObservationIdentifierText"].ToString());
            ehrLabResult.ObservationIdentifierCodeSystemName = SIn.String(row["ObservationIdentifierCodeSystemName"].ToString());
            ehrLabResult.ObservationIdentifierIDAlt = SIn.String(row["ObservationIdentifierIDAlt"].ToString());
            ehrLabResult.ObservationIdentifierTextAlt = SIn.String(row["ObservationIdentifierTextAlt"].ToString());
            ehrLabResult.ObservationIdentifierCodeSystemNameAlt = SIn.String(row["ObservationIdentifierCodeSystemNameAlt"].ToString());
            ehrLabResult.ObservationIdentifierTextOriginal = SIn.String(row["ObservationIdentifierTextOriginal"].ToString());
            ehrLabResult.ObservationIdentifierSub = SIn.String(row["ObservationIdentifierSub"].ToString());
            ehrLabResult.ObservationValueCodedElementID = SIn.String(row["ObservationValueCodedElementID"].ToString());
            ehrLabResult.ObservationValueCodedElementText = SIn.String(row["ObservationValueCodedElementText"].ToString());
            ehrLabResult.ObservationValueCodedElementCodeSystemName = SIn.String(row["ObservationValueCodedElementCodeSystemName"].ToString());
            ehrLabResult.ObservationValueCodedElementIDAlt = SIn.String(row["ObservationValueCodedElementIDAlt"].ToString());
            ehrLabResult.ObservationValueCodedElementTextAlt = SIn.String(row["ObservationValueCodedElementTextAlt"].ToString());
            ehrLabResult.ObservationValueCodedElementCodeSystemNameAlt = SIn.String(row["ObservationValueCodedElementCodeSystemNameAlt"].ToString());
            ehrLabResult.ObservationValueCodedElementTextOriginal = SIn.String(row["ObservationValueCodedElementTextOriginal"].ToString());
            ehrLabResult.ObservationValueDateTime = SIn.String(row["ObservationValueDateTime"].ToString());
            ehrLabResult.ObservationValueTime = SIn.TimeSpan(row["ObservationValueTime"].ToString());
            ehrLabResult.ObservationValueComparator = SIn.String(row["ObservationValueComparator"].ToString());
            ehrLabResult.ObservationValueNumber1 = SIn.Double(row["ObservationValueNumber1"].ToString());
            ehrLabResult.ObservationValueSeparatorOrSuffix = SIn.String(row["ObservationValueSeparatorOrSuffix"].ToString());
            ehrLabResult.ObservationValueNumber2 = SIn.Double(row["ObservationValueNumber2"].ToString());
            ehrLabResult.ObservationValueNumeric = SIn.Double(row["ObservationValueNumeric"].ToString());
            ehrLabResult.ObservationValueText = SIn.String(row["ObservationValueText"].ToString());
            ehrLabResult.UnitsID = SIn.String(row["UnitsID"].ToString());
            ehrLabResult.UnitsText = SIn.String(row["UnitsText"].ToString());
            ehrLabResult.UnitsCodeSystemName = SIn.String(row["UnitsCodeSystemName"].ToString());
            ehrLabResult.UnitsIDAlt = SIn.String(row["UnitsIDAlt"].ToString());
            ehrLabResult.UnitsTextAlt = SIn.String(row["UnitsTextAlt"].ToString());
            ehrLabResult.UnitsCodeSystemNameAlt = SIn.String(row["UnitsCodeSystemNameAlt"].ToString());
            ehrLabResult.UnitsTextOriginal = SIn.String(row["UnitsTextOriginal"].ToString());
            ehrLabResult.referenceRange = SIn.String(row["referenceRange"].ToString());
            ehrLabResult.AbnormalFlags = SIn.String(row["AbnormalFlags"].ToString());
            var observationResultStatus = row["ObservationResultStatus"].ToString();
            if (observationResultStatus == "")
                ehrLabResult.ObservationResultStatus = 0;
            else
                try
                {
                    ehrLabResult.ObservationResultStatus = (HL70085) Enum.Parse(typeof(HL70085), observationResultStatus);
                }
                catch
                {
                    ehrLabResult.ObservationResultStatus = 0;
                }

            ehrLabResult.ObservationDateTime = SIn.String(row["ObservationDateTime"].ToString());
            ehrLabResult.AnalysisDateTime = SIn.String(row["AnalysisDateTime"].ToString());
            ehrLabResult.PerformingOrganizationName = SIn.String(row["PerformingOrganizationName"].ToString());
            ehrLabResult.PerformingOrganizationNameAssigningAuthorityNamespaceId = SIn.String(row["PerformingOrganizationNameAssigningAuthorityNamespaceId"].ToString());
            ehrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalId = SIn.String(row["PerformingOrganizationNameAssigningAuthorityUniversalId"].ToString());
            ehrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalIdType = SIn.String(row["PerformingOrganizationNameAssigningAuthorityUniversalIdType"].ToString());
            var performingOrganizationIdentifierTypeCode = row["PerformingOrganizationIdentifierTypeCode"].ToString();
            if (performingOrganizationIdentifierTypeCode == "")
                ehrLabResult.PerformingOrganizationIdentifierTypeCode = 0;
            else
                try
                {
                    ehrLabResult.PerformingOrganizationIdentifierTypeCode = (HL70203) Enum.Parse(typeof(HL70203), performingOrganizationIdentifierTypeCode);
                }
                catch
                {
                    ehrLabResult.PerformingOrganizationIdentifierTypeCode = 0;
                }

            ehrLabResult.PerformingOrganizationIdentifier = SIn.String(row["PerformingOrganizationIdentifier"].ToString());
            ehrLabResult.PerformingOrganizationAddressStreet = SIn.String(row["PerformingOrganizationAddressStreet"].ToString());
            ehrLabResult.PerformingOrganizationAddressOtherDesignation = SIn.String(row["PerformingOrganizationAddressOtherDesignation"].ToString());
            ehrLabResult.PerformingOrganizationAddressCity = SIn.String(row["PerformingOrganizationAddressCity"].ToString());
            var performingOrganizationAddressStateOrProvince = row["PerformingOrganizationAddressStateOrProvince"].ToString();
            if (performingOrganizationAddressStateOrProvince == "")
                ehrLabResult.PerformingOrganizationAddressStateOrProvince = 0;
            else
                try
                {
                    ehrLabResult.PerformingOrganizationAddressStateOrProvince = (USPSAlphaStateCode) Enum.Parse(typeof(USPSAlphaStateCode), performingOrganizationAddressStateOrProvince);
                }
                catch
                {
                    ehrLabResult.PerformingOrganizationAddressStateOrProvince = 0;
                }

            ehrLabResult.PerformingOrganizationAddressZipOrPostalCode = SIn.String(row["PerformingOrganizationAddressZipOrPostalCode"].ToString());
            ehrLabResult.PerformingOrganizationAddressCountryCode = SIn.String(row["PerformingOrganizationAddressCountryCode"].ToString());
            var performingOrganizationAddressAddressType = row["PerformingOrganizationAddressAddressType"].ToString();
            if (performingOrganizationAddressAddressType == "")
                ehrLabResult.PerformingOrganizationAddressAddressType = 0;
            else
                try
                {
                    ehrLabResult.PerformingOrganizationAddressAddressType = (HL70190) Enum.Parse(typeof(HL70190), performingOrganizationAddressAddressType);
                }
                catch
                {
                    ehrLabResult.PerformingOrganizationAddressAddressType = 0;
                }

            ehrLabResult.PerformingOrganizationAddressCountyOrParishCode = SIn.String(row["PerformingOrganizationAddressCountyOrParishCode"].ToString());
            ehrLabResult.MedicalDirectorID = SIn.String(row["MedicalDirectorID"].ToString());
            ehrLabResult.MedicalDirectorLName = SIn.String(row["MedicalDirectorLName"].ToString());
            ehrLabResult.MedicalDirectorFName = SIn.String(row["MedicalDirectorFName"].ToString());
            ehrLabResult.MedicalDirectorMiddleNames = SIn.String(row["MedicalDirectorMiddleNames"].ToString());
            ehrLabResult.MedicalDirectorSuffix = SIn.String(row["MedicalDirectorSuffix"].ToString());
            ehrLabResult.MedicalDirectorPrefix = SIn.String(row["MedicalDirectorPrefix"].ToString());
            ehrLabResult.MedicalDirectorAssigningAuthorityNamespaceID = SIn.String(row["MedicalDirectorAssigningAuthorityNamespaceID"].ToString());
            ehrLabResult.MedicalDirectorAssigningAuthorityUniversalID = SIn.String(row["MedicalDirectorAssigningAuthorityUniversalID"].ToString());
            ehrLabResult.MedicalDirectorAssigningAuthorityIDType = SIn.String(row["MedicalDirectorAssigningAuthorityIDType"].ToString());
            var medicalDirectorNameTypeCode = row["MedicalDirectorNameTypeCode"].ToString();
            if (medicalDirectorNameTypeCode == "")
                ehrLabResult.MedicalDirectorNameTypeCode = 0;
            else
                try
                {
                    ehrLabResult.MedicalDirectorNameTypeCode = (HL70200) Enum.Parse(typeof(HL70200), medicalDirectorNameTypeCode);
                }
                catch
                {
                    ehrLabResult.MedicalDirectorNameTypeCode = 0;
                }

            var medicalDirectorIdentifierTypeCode = row["MedicalDirectorIdentifierTypeCode"].ToString();
            if (medicalDirectorIdentifierTypeCode == "")
                ehrLabResult.MedicalDirectorIdentifierTypeCode = 0;
            else
                try
                {
                    ehrLabResult.MedicalDirectorIdentifierTypeCode = (HL70203) Enum.Parse(typeof(HL70203), medicalDirectorIdentifierTypeCode);
                }
                catch
                {
                    ehrLabResult.MedicalDirectorIdentifierTypeCode = 0;
                }

            retVal.Add(ehrLabResult);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrLabResult> listEhrLabResults, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrLabResult";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrLabResultNum");
        table.Columns.Add("EhrLabNum");
        table.Columns.Add("SetIdOBX");
        table.Columns.Add("ValueType");
        table.Columns.Add("ObservationIdentifierID");
        table.Columns.Add("ObservationIdentifierText");
        table.Columns.Add("ObservationIdentifierCodeSystemName");
        table.Columns.Add("ObservationIdentifierIDAlt");
        table.Columns.Add("ObservationIdentifierTextAlt");
        table.Columns.Add("ObservationIdentifierCodeSystemNameAlt");
        table.Columns.Add("ObservationIdentifierTextOriginal");
        table.Columns.Add("ObservationIdentifierSub");
        table.Columns.Add("ObservationValueCodedElementID");
        table.Columns.Add("ObservationValueCodedElementText");
        table.Columns.Add("ObservationValueCodedElementCodeSystemName");
        table.Columns.Add("ObservationValueCodedElementIDAlt");
        table.Columns.Add("ObservationValueCodedElementTextAlt");
        table.Columns.Add("ObservationValueCodedElementCodeSystemNameAlt");
        table.Columns.Add("ObservationValueCodedElementTextOriginal");
        table.Columns.Add("ObservationValueDateTime");
        table.Columns.Add("ObservationValueTime");
        table.Columns.Add("ObservationValueComparator");
        table.Columns.Add("ObservationValueNumber1");
        table.Columns.Add("ObservationValueSeparatorOrSuffix");
        table.Columns.Add("ObservationValueNumber2");
        table.Columns.Add("ObservationValueNumeric");
        table.Columns.Add("ObservationValueText");
        table.Columns.Add("UnitsID");
        table.Columns.Add("UnitsText");
        table.Columns.Add("UnitsCodeSystemName");
        table.Columns.Add("UnitsIDAlt");
        table.Columns.Add("UnitsTextAlt");
        table.Columns.Add("UnitsCodeSystemNameAlt");
        table.Columns.Add("UnitsTextOriginal");
        table.Columns.Add("referenceRange");
        table.Columns.Add("AbnormalFlags");
        table.Columns.Add("ObservationResultStatus");
        table.Columns.Add("ObservationDateTime");
        table.Columns.Add("AnalysisDateTime");
        table.Columns.Add("PerformingOrganizationName");
        table.Columns.Add("PerformingOrganizationNameAssigningAuthorityNamespaceId");
        table.Columns.Add("PerformingOrganizationNameAssigningAuthorityUniversalId");
        table.Columns.Add("PerformingOrganizationNameAssigningAuthorityUniversalIdType");
        table.Columns.Add("PerformingOrganizationIdentifierTypeCode");
        table.Columns.Add("PerformingOrganizationIdentifier");
        table.Columns.Add("PerformingOrganizationAddressStreet");
        table.Columns.Add("PerformingOrganizationAddressOtherDesignation");
        table.Columns.Add("PerformingOrganizationAddressCity");
        table.Columns.Add("PerformingOrganizationAddressStateOrProvince");
        table.Columns.Add("PerformingOrganizationAddressZipOrPostalCode");
        table.Columns.Add("PerformingOrganizationAddressCountryCode");
        table.Columns.Add("PerformingOrganizationAddressAddressType");
        table.Columns.Add("PerformingOrganizationAddressCountyOrParishCode");
        table.Columns.Add("MedicalDirectorID");
        table.Columns.Add("MedicalDirectorLName");
        table.Columns.Add("MedicalDirectorFName");
        table.Columns.Add("MedicalDirectorMiddleNames");
        table.Columns.Add("MedicalDirectorSuffix");
        table.Columns.Add("MedicalDirectorPrefix");
        table.Columns.Add("MedicalDirectorAssigningAuthorityNamespaceID");
        table.Columns.Add("MedicalDirectorAssigningAuthorityUniversalID");
        table.Columns.Add("MedicalDirectorAssigningAuthorityIDType");
        table.Columns.Add("MedicalDirectorNameTypeCode");
        table.Columns.Add("MedicalDirectorIdentifierTypeCode");
        foreach (var ehrLabResult in listEhrLabResults)
            table.Rows.Add(SOut.Long(ehrLabResult.EhrLabResultNum), SOut.Long(ehrLabResult.EhrLabNum), SOut.Long(ehrLabResult.SetIdOBX), SOut.Int((int) ehrLabResult.ValueType), ehrLabResult.ObservationIdentifierID, ehrLabResult.ObservationIdentifierText, ehrLabResult.ObservationIdentifierCodeSystemName, ehrLabResult.ObservationIdentifierIDAlt, ehrLabResult.ObservationIdentifierTextAlt, ehrLabResult.ObservationIdentifierCodeSystemNameAlt, ehrLabResult.ObservationIdentifierTextOriginal, ehrLabResult.ObservationIdentifierSub, ehrLabResult.ObservationValueCodedElementID, ehrLabResult.ObservationValueCodedElementText, ehrLabResult.ObservationValueCodedElementCodeSystemName, ehrLabResult.ObservationValueCodedElementIDAlt, ehrLabResult.ObservationValueCodedElementTextAlt, ehrLabResult.ObservationValueCodedElementCodeSystemNameAlt, ehrLabResult.ObservationValueCodedElementTextOriginal, ehrLabResult.ObservationValueDateTime, SOut.Time(ehrLabResult.ObservationValueTime, false), ehrLabResult.ObservationValueComparator, SOut.Double(ehrLabResult.ObservationValueNumber1), ehrLabResult.ObservationValueSeparatorOrSuffix, SOut.Double(ehrLabResult.ObservationValueNumber2), SOut.Double(ehrLabResult.ObservationValueNumeric), ehrLabResult.ObservationValueText, ehrLabResult.UnitsID, ehrLabResult.UnitsText, ehrLabResult.UnitsCodeSystemName, ehrLabResult.UnitsIDAlt, ehrLabResult.UnitsTextAlt, ehrLabResult.UnitsCodeSystemNameAlt, ehrLabResult.UnitsTextOriginal, ehrLabResult.referenceRange, ehrLabResult.AbnormalFlags, SOut.Int((int) ehrLabResult.ObservationResultStatus), ehrLabResult.ObservationDateTime, ehrLabResult.AnalysisDateTime, ehrLabResult.PerformingOrganizationName, ehrLabResult.PerformingOrganizationNameAssigningAuthorityNamespaceId, ehrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalId, ehrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalIdType, SOut.Int((int) ehrLabResult.PerformingOrganizationIdentifierTypeCode), ehrLabResult.PerformingOrganizationIdentifier, ehrLabResult.PerformingOrganizationAddressStreet, ehrLabResult.PerformingOrganizationAddressOtherDesignation, ehrLabResult.PerformingOrganizationAddressCity, SOut.Int((int) ehrLabResult.PerformingOrganizationAddressStateOrProvince), ehrLabResult.PerformingOrganizationAddressZipOrPostalCode, ehrLabResult.PerformingOrganizationAddressCountryCode, SOut.Int((int) ehrLabResult.PerformingOrganizationAddressAddressType), ehrLabResult.PerformingOrganizationAddressCountyOrParishCode, ehrLabResult.MedicalDirectorID, ehrLabResult.MedicalDirectorLName, ehrLabResult.MedicalDirectorFName, ehrLabResult.MedicalDirectorMiddleNames, ehrLabResult.MedicalDirectorSuffix, ehrLabResult.MedicalDirectorPrefix, ehrLabResult.MedicalDirectorAssigningAuthorityNamespaceID, ehrLabResult.MedicalDirectorAssigningAuthorityUniversalID, ehrLabResult.MedicalDirectorAssigningAuthorityIDType, SOut.Int((int) ehrLabResult.MedicalDirectorNameTypeCode), SOut.Int((int) ehrLabResult.MedicalDirectorIdentifierTypeCode));
        return table;
    }

    public static long Insert(EhrLabResult ehrLabResult)
    {
        return Insert(ehrLabResult, false);
    }

    public static long Insert(EhrLabResult ehrLabResult, bool useExistingPK)
    {
        var command = "INSERT INTO ehrlabresult (";

        command += "EhrLabNum,SetIdOBX,ValueType,ObservationIdentifierID,ObservationIdentifierText,ObservationIdentifierCodeSystemName,ObservationIdentifierIDAlt,ObservationIdentifierTextAlt,ObservationIdentifierCodeSystemNameAlt,ObservationIdentifierTextOriginal,ObservationIdentifierSub,ObservationValueCodedElementID,ObservationValueCodedElementText,ObservationValueCodedElementCodeSystemName,ObservationValueCodedElementIDAlt,ObservationValueCodedElementTextAlt,ObservationValueCodedElementCodeSystemNameAlt,ObservationValueCodedElementTextOriginal,ObservationValueDateTime,ObservationValueTime,ObservationValueComparator,ObservationValueNumber1,ObservationValueSeparatorOrSuffix,ObservationValueNumber2,ObservationValueNumeric,ObservationValueText,UnitsID,UnitsText,UnitsCodeSystemName,UnitsIDAlt,UnitsTextAlt,UnitsCodeSystemNameAlt,UnitsTextOriginal,referenceRange,AbnormalFlags,ObservationResultStatus,ObservationDateTime,AnalysisDateTime,PerformingOrganizationName,PerformingOrganizationNameAssigningAuthorityNamespaceId,PerformingOrganizationNameAssigningAuthorityUniversalId,PerformingOrganizationNameAssigningAuthorityUniversalIdType,PerformingOrganizationIdentifierTypeCode,PerformingOrganizationIdentifier,PerformingOrganizationAddressStreet,PerformingOrganizationAddressOtherDesignation,PerformingOrganizationAddressCity,PerformingOrganizationAddressStateOrProvince,PerformingOrganizationAddressZipOrPostalCode,PerformingOrganizationAddressCountryCode,PerformingOrganizationAddressAddressType,PerformingOrganizationAddressCountyOrParishCode,MedicalDirectorID,MedicalDirectorLName,MedicalDirectorFName,MedicalDirectorMiddleNames,MedicalDirectorSuffix,MedicalDirectorPrefix,MedicalDirectorAssigningAuthorityNamespaceID,MedicalDirectorAssigningAuthorityUniversalID,MedicalDirectorAssigningAuthorityIDType,MedicalDirectorNameTypeCode,MedicalDirectorIdentifierTypeCode) VALUES(";

        command +=
            SOut.Long(ehrLabResult.EhrLabNum) + ","
                                              + SOut.Long(ehrLabResult.SetIdOBX) + ","
                                              + "'" + SOut.String(ehrLabResult.ValueType.ToString()) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationIdentifierID) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationIdentifierText) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationIdentifierCodeSystemName) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationIdentifierIDAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationIdentifierTextAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationIdentifierCodeSystemNameAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationIdentifierTextOriginal) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationIdentifierSub) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationValueCodedElementID) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationValueCodedElementText) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationValueCodedElementCodeSystemName) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationValueCodedElementIDAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationValueCodedElementTextAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationValueCodedElementCodeSystemNameAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationValueCodedElementTextOriginal) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationValueDateTime) + "',"
                                              + SOut.Time(ehrLabResult.ObservationValueTime) + ","
                                              + "'" + SOut.String(ehrLabResult.ObservationValueComparator) + "',"
                                              + SOut.Double(ehrLabResult.ObservationValueNumber1) + ","
                                              + "'" + SOut.String(ehrLabResult.ObservationValueSeparatorOrSuffix) + "',"
                                              + SOut.Double(ehrLabResult.ObservationValueNumber2) + ","
                                              + SOut.Double(ehrLabResult.ObservationValueNumeric) + ","
                                              + "'" + SOut.String(ehrLabResult.ObservationValueText) + "',"
                                              + "'" + SOut.String(ehrLabResult.UnitsID) + "',"
                                              + "'" + SOut.String(ehrLabResult.UnitsText) + "',"
                                              + "'" + SOut.String(ehrLabResult.UnitsCodeSystemName) + "',"
                                              + "'" + SOut.String(ehrLabResult.UnitsIDAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.UnitsTextAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.UnitsCodeSystemNameAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.UnitsTextOriginal) + "',"
                                              + "'" + SOut.String(ehrLabResult.referenceRange) + "',"
                                              + "'" + SOut.String(ehrLabResult.AbnormalFlags) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationResultStatus.ToString()) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationDateTime) + "',"
                                              + "'" + SOut.String(ehrLabResult.AnalysisDateTime) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationName) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationNameAssigningAuthorityNamespaceId) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalId) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalIdType) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationIdentifierTypeCode.ToString()) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationIdentifier) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationAddressStreet) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationAddressOtherDesignation) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationAddressCity) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationAddressStateOrProvince.ToString()) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationAddressZipOrPostalCode) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationAddressCountryCode) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationAddressAddressType.ToString()) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationAddressCountyOrParishCode) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorID) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorLName) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorFName) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorMiddleNames) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorSuffix) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorPrefix) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorAssigningAuthorityNamespaceID) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorAssigningAuthorityUniversalID) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorAssigningAuthorityIDType) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorNameTypeCode.ToString()) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorIdentifierTypeCode.ToString()) + "')";
        {
            ehrLabResult.EhrLabResultNum = Db.NonQ(command, true, "EhrLabResultNum", "ehrLabResult");
        }
        return ehrLabResult.EhrLabResultNum;
    }

    public static long InsertNoCache(EhrLabResult ehrLabResult)
    {
        return InsertNoCache(ehrLabResult, false);
    }

    public static long InsertNoCache(EhrLabResult ehrLabResult, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrlabresult (";
        if (isRandomKeys || useExistingPK) command += "EhrLabResultNum,";
        command += "EhrLabNum,SetIdOBX,ValueType,ObservationIdentifierID,ObservationIdentifierText,ObservationIdentifierCodeSystemName,ObservationIdentifierIDAlt,ObservationIdentifierTextAlt,ObservationIdentifierCodeSystemNameAlt,ObservationIdentifierTextOriginal,ObservationIdentifierSub,ObservationValueCodedElementID,ObservationValueCodedElementText,ObservationValueCodedElementCodeSystemName,ObservationValueCodedElementIDAlt,ObservationValueCodedElementTextAlt,ObservationValueCodedElementCodeSystemNameAlt,ObservationValueCodedElementTextOriginal,ObservationValueDateTime,ObservationValueTime,ObservationValueComparator,ObservationValueNumber1,ObservationValueSeparatorOrSuffix,ObservationValueNumber2,ObservationValueNumeric,ObservationValueText,UnitsID,UnitsText,UnitsCodeSystemName,UnitsIDAlt,UnitsTextAlt,UnitsCodeSystemNameAlt,UnitsTextOriginal,referenceRange,AbnormalFlags,ObservationResultStatus,ObservationDateTime,AnalysisDateTime,PerformingOrganizationName,PerformingOrganizationNameAssigningAuthorityNamespaceId,PerformingOrganizationNameAssigningAuthorityUniversalId,PerformingOrganizationNameAssigningAuthorityUniversalIdType,PerformingOrganizationIdentifierTypeCode,PerformingOrganizationIdentifier,PerformingOrganizationAddressStreet,PerformingOrganizationAddressOtherDesignation,PerformingOrganizationAddressCity,PerformingOrganizationAddressStateOrProvince,PerformingOrganizationAddressZipOrPostalCode,PerformingOrganizationAddressCountryCode,PerformingOrganizationAddressAddressType,PerformingOrganizationAddressCountyOrParishCode,MedicalDirectorID,MedicalDirectorLName,MedicalDirectorFName,MedicalDirectorMiddleNames,MedicalDirectorSuffix,MedicalDirectorPrefix,MedicalDirectorAssigningAuthorityNamespaceID,MedicalDirectorAssigningAuthorityUniversalID,MedicalDirectorAssigningAuthorityIDType,MedicalDirectorNameTypeCode,MedicalDirectorIdentifierTypeCode) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrLabResult.EhrLabResultNum) + ",";
        command +=
            SOut.Long(ehrLabResult.EhrLabNum) + ","
                                              + SOut.Long(ehrLabResult.SetIdOBX) + ","
                                              + "'" + SOut.String(ehrLabResult.ValueType.ToString()) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationIdentifierID) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationIdentifierText) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationIdentifierCodeSystemName) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationIdentifierIDAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationIdentifierTextAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationIdentifierCodeSystemNameAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationIdentifierTextOriginal) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationIdentifierSub) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationValueCodedElementID) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationValueCodedElementText) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationValueCodedElementCodeSystemName) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationValueCodedElementIDAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationValueCodedElementTextAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationValueCodedElementCodeSystemNameAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationValueCodedElementTextOriginal) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationValueDateTime) + "',"
                                              + SOut.Time(ehrLabResult.ObservationValueTime) + ","
                                              + "'" + SOut.String(ehrLabResult.ObservationValueComparator) + "',"
                                              + SOut.Double(ehrLabResult.ObservationValueNumber1) + ","
                                              + "'" + SOut.String(ehrLabResult.ObservationValueSeparatorOrSuffix) + "',"
                                              + SOut.Double(ehrLabResult.ObservationValueNumber2) + ","
                                              + SOut.Double(ehrLabResult.ObservationValueNumeric) + ","
                                              + "'" + SOut.String(ehrLabResult.ObservationValueText) + "',"
                                              + "'" + SOut.String(ehrLabResult.UnitsID) + "',"
                                              + "'" + SOut.String(ehrLabResult.UnitsText) + "',"
                                              + "'" + SOut.String(ehrLabResult.UnitsCodeSystemName) + "',"
                                              + "'" + SOut.String(ehrLabResult.UnitsIDAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.UnitsTextAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.UnitsCodeSystemNameAlt) + "',"
                                              + "'" + SOut.String(ehrLabResult.UnitsTextOriginal) + "',"
                                              + "'" + SOut.String(ehrLabResult.referenceRange) + "',"
                                              + "'" + SOut.String(ehrLabResult.AbnormalFlags) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationResultStatus.ToString()) + "',"
                                              + "'" + SOut.String(ehrLabResult.ObservationDateTime) + "',"
                                              + "'" + SOut.String(ehrLabResult.AnalysisDateTime) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationName) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationNameAssigningAuthorityNamespaceId) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalId) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalIdType) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationIdentifierTypeCode.ToString()) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationIdentifier) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationAddressStreet) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationAddressOtherDesignation) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationAddressCity) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationAddressStateOrProvince.ToString()) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationAddressZipOrPostalCode) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationAddressCountryCode) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationAddressAddressType.ToString()) + "',"
                                              + "'" + SOut.String(ehrLabResult.PerformingOrganizationAddressCountyOrParishCode) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorID) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorLName) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorFName) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorMiddleNames) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorSuffix) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorPrefix) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorAssigningAuthorityNamespaceID) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorAssigningAuthorityUniversalID) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorAssigningAuthorityIDType) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorNameTypeCode.ToString()) + "',"
                                              + "'" + SOut.String(ehrLabResult.MedicalDirectorIdentifierTypeCode.ToString()) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            ehrLabResult.EhrLabResultNum = Db.NonQ(command, true, "EhrLabResultNum", "ehrLabResult");
        return ehrLabResult.EhrLabResultNum;
    }

    public static void Update(EhrLabResult ehrLabResult)
    {
        var command = "UPDATE ehrlabresult SET "
                      + "EhrLabNum                                                  =  " + SOut.Long(ehrLabResult.EhrLabNum) + ", "
                      + "SetIdOBX                                                   =  " + SOut.Long(ehrLabResult.SetIdOBX) + ", "
                      + "ValueType                                                  = '" + SOut.String(ehrLabResult.ValueType.ToString()) + "', "
                      + "ObservationIdentifierID                                    = '" + SOut.String(ehrLabResult.ObservationIdentifierID) + "', "
                      + "ObservationIdentifierText                                  = '" + SOut.String(ehrLabResult.ObservationIdentifierText) + "', "
                      + "ObservationIdentifierCodeSystemName                        = '" + SOut.String(ehrLabResult.ObservationIdentifierCodeSystemName) + "', "
                      + "ObservationIdentifierIDAlt                                 = '" + SOut.String(ehrLabResult.ObservationIdentifierIDAlt) + "', "
                      + "ObservationIdentifierTextAlt                               = '" + SOut.String(ehrLabResult.ObservationIdentifierTextAlt) + "', "
                      + "ObservationIdentifierCodeSystemNameAlt                     = '" + SOut.String(ehrLabResult.ObservationIdentifierCodeSystemNameAlt) + "', "
                      + "ObservationIdentifierTextOriginal                          = '" + SOut.String(ehrLabResult.ObservationIdentifierTextOriginal) + "', "
                      + "ObservationIdentifierSub                                   = '" + SOut.String(ehrLabResult.ObservationIdentifierSub) + "', "
                      + "ObservationValueCodedElementID                             = '" + SOut.String(ehrLabResult.ObservationValueCodedElementID) + "', "
                      + "ObservationValueCodedElementText                           = '" + SOut.String(ehrLabResult.ObservationValueCodedElementText) + "', "
                      + "ObservationValueCodedElementCodeSystemName                 = '" + SOut.String(ehrLabResult.ObservationValueCodedElementCodeSystemName) + "', "
                      + "ObservationValueCodedElementIDAlt                          = '" + SOut.String(ehrLabResult.ObservationValueCodedElementIDAlt) + "', "
                      + "ObservationValueCodedElementTextAlt                        = '" + SOut.String(ehrLabResult.ObservationValueCodedElementTextAlt) + "', "
                      + "ObservationValueCodedElementCodeSystemNameAlt              = '" + SOut.String(ehrLabResult.ObservationValueCodedElementCodeSystemNameAlt) + "', "
                      + "ObservationValueCodedElementTextOriginal                   = '" + SOut.String(ehrLabResult.ObservationValueCodedElementTextOriginal) + "', "
                      + "ObservationValueDateTime                                   = '" + SOut.String(ehrLabResult.ObservationValueDateTime) + "', "
                      + "ObservationValueTime                                       =  " + SOut.Time(ehrLabResult.ObservationValueTime) + ", "
                      + "ObservationValueComparator                                 = '" + SOut.String(ehrLabResult.ObservationValueComparator) + "', "
                      + "ObservationValueNumber1                                    =  " + SOut.Double(ehrLabResult.ObservationValueNumber1) + ", "
                      + "ObservationValueSeparatorOrSuffix                          = '" + SOut.String(ehrLabResult.ObservationValueSeparatorOrSuffix) + "', "
                      + "ObservationValueNumber2                                    =  " + SOut.Double(ehrLabResult.ObservationValueNumber2) + ", "
                      + "ObservationValueNumeric                                    =  " + SOut.Double(ehrLabResult.ObservationValueNumeric) + ", "
                      + "ObservationValueText                                       = '" + SOut.String(ehrLabResult.ObservationValueText) + "', "
                      + "UnitsID                                                    = '" + SOut.String(ehrLabResult.UnitsID) + "', "
                      + "UnitsText                                                  = '" + SOut.String(ehrLabResult.UnitsText) + "', "
                      + "UnitsCodeSystemName                                        = '" + SOut.String(ehrLabResult.UnitsCodeSystemName) + "', "
                      + "UnitsIDAlt                                                 = '" + SOut.String(ehrLabResult.UnitsIDAlt) + "', "
                      + "UnitsTextAlt                                               = '" + SOut.String(ehrLabResult.UnitsTextAlt) + "', "
                      + "UnitsCodeSystemNameAlt                                     = '" + SOut.String(ehrLabResult.UnitsCodeSystemNameAlt) + "', "
                      + "UnitsTextOriginal                                          = '" + SOut.String(ehrLabResult.UnitsTextOriginal) + "', "
                      + "referenceRange                                             = '" + SOut.String(ehrLabResult.referenceRange) + "', "
                      + "AbnormalFlags                                              = '" + SOut.String(ehrLabResult.AbnormalFlags) + "', "
                      + "ObservationResultStatus                                    = '" + SOut.String(ehrLabResult.ObservationResultStatus.ToString()) + "', "
                      + "ObservationDateTime                                        = '" + SOut.String(ehrLabResult.ObservationDateTime) + "', "
                      + "AnalysisDateTime                                           = '" + SOut.String(ehrLabResult.AnalysisDateTime) + "', "
                      + "PerformingOrganizationName                                 = '" + SOut.String(ehrLabResult.PerformingOrganizationName) + "', "
                      + "PerformingOrganizationNameAssigningAuthorityNamespaceId    = '" + SOut.String(ehrLabResult.PerformingOrganizationNameAssigningAuthorityNamespaceId) + "', "
                      + "PerformingOrganizationNameAssigningAuthorityUniversalId    = '" + SOut.String(ehrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalId) + "', "
                      + "PerformingOrganizationNameAssigningAuthorityUniversalIdType= '" + SOut.String(ehrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalIdType) + "', "
                      + "PerformingOrganizationIdentifierTypeCode                   = '" + SOut.String(ehrLabResult.PerformingOrganizationIdentifierTypeCode.ToString()) + "', "
                      + "PerformingOrganizationIdentifier                           = '" + SOut.String(ehrLabResult.PerformingOrganizationIdentifier) + "', "
                      + "PerformingOrganizationAddressStreet                        = '" + SOut.String(ehrLabResult.PerformingOrganizationAddressStreet) + "', "
                      + "PerformingOrganizationAddressOtherDesignation              = '" + SOut.String(ehrLabResult.PerformingOrganizationAddressOtherDesignation) + "', "
                      + "PerformingOrganizationAddressCity                          = '" + SOut.String(ehrLabResult.PerformingOrganizationAddressCity) + "', "
                      + "PerformingOrganizationAddressStateOrProvince               = '" + SOut.String(ehrLabResult.PerformingOrganizationAddressStateOrProvince.ToString()) + "', "
                      + "PerformingOrganizationAddressZipOrPostalCode               = '" + SOut.String(ehrLabResult.PerformingOrganizationAddressZipOrPostalCode) + "', "
                      + "PerformingOrganizationAddressCountryCode                   = '" + SOut.String(ehrLabResult.PerformingOrganizationAddressCountryCode) + "', "
                      + "PerformingOrganizationAddressAddressType                   = '" + SOut.String(ehrLabResult.PerformingOrganizationAddressAddressType.ToString()) + "', "
                      + "PerformingOrganizationAddressCountyOrParishCode            = '" + SOut.String(ehrLabResult.PerformingOrganizationAddressCountyOrParishCode) + "', "
                      + "MedicalDirectorID                                          = '" + SOut.String(ehrLabResult.MedicalDirectorID) + "', "
                      + "MedicalDirectorLName                                       = '" + SOut.String(ehrLabResult.MedicalDirectorLName) + "', "
                      + "MedicalDirectorFName                                       = '" + SOut.String(ehrLabResult.MedicalDirectorFName) + "', "
                      + "MedicalDirectorMiddleNames                                 = '" + SOut.String(ehrLabResult.MedicalDirectorMiddleNames) + "', "
                      + "MedicalDirectorSuffix                                      = '" + SOut.String(ehrLabResult.MedicalDirectorSuffix) + "', "
                      + "MedicalDirectorPrefix                                      = '" + SOut.String(ehrLabResult.MedicalDirectorPrefix) + "', "
                      + "MedicalDirectorAssigningAuthorityNamespaceID               = '" + SOut.String(ehrLabResult.MedicalDirectorAssigningAuthorityNamespaceID) + "', "
                      + "MedicalDirectorAssigningAuthorityUniversalID               = '" + SOut.String(ehrLabResult.MedicalDirectorAssigningAuthorityUniversalID) + "', "
                      + "MedicalDirectorAssigningAuthorityIDType                    = '" + SOut.String(ehrLabResult.MedicalDirectorAssigningAuthorityIDType) + "', "
                      + "MedicalDirectorNameTypeCode                                = '" + SOut.String(ehrLabResult.MedicalDirectorNameTypeCode.ToString()) + "', "
                      + "MedicalDirectorIdentifierTypeCode                          = '" + SOut.String(ehrLabResult.MedicalDirectorIdentifierTypeCode.ToString()) + "' "
                      + "WHERE EhrLabResultNum = " + SOut.Long(ehrLabResult.EhrLabResultNum);
        Db.NonQ(command);
    }

    public static bool Update(EhrLabResult ehrLabResult, EhrLabResult oldEhrLabResult)
    {
        var command = "";
        if (ehrLabResult.EhrLabNum != oldEhrLabResult.EhrLabNum)
        {
            if (command != "") command += ",";
            command += "EhrLabNum = " + SOut.Long(ehrLabResult.EhrLabNum) + "";
        }

        if (ehrLabResult.SetIdOBX != oldEhrLabResult.SetIdOBX)
        {
            if (command != "") command += ",";
            command += "SetIdOBX = " + SOut.Long(ehrLabResult.SetIdOBX) + "";
        }

        if (ehrLabResult.ValueType != oldEhrLabResult.ValueType)
        {
            if (command != "") command += ",";
            command += "ValueType = '" + SOut.String(ehrLabResult.ValueType.ToString()) + "'";
        }

        if (ehrLabResult.ObservationIdentifierID != oldEhrLabResult.ObservationIdentifierID)
        {
            if (command != "") command += ",";
            command += "ObservationIdentifierID = '" + SOut.String(ehrLabResult.ObservationIdentifierID) + "'";
        }

        if (ehrLabResult.ObservationIdentifierText != oldEhrLabResult.ObservationIdentifierText)
        {
            if (command != "") command += ",";
            command += "ObservationIdentifierText = '" + SOut.String(ehrLabResult.ObservationIdentifierText) + "'";
        }

        if (ehrLabResult.ObservationIdentifierCodeSystemName != oldEhrLabResult.ObservationIdentifierCodeSystemName)
        {
            if (command != "") command += ",";
            command += "ObservationIdentifierCodeSystemName = '" + SOut.String(ehrLabResult.ObservationIdentifierCodeSystemName) + "'";
        }

        if (ehrLabResult.ObservationIdentifierIDAlt != oldEhrLabResult.ObservationIdentifierIDAlt)
        {
            if (command != "") command += ",";
            command += "ObservationIdentifierIDAlt = '" + SOut.String(ehrLabResult.ObservationIdentifierIDAlt) + "'";
        }

        if (ehrLabResult.ObservationIdentifierTextAlt != oldEhrLabResult.ObservationIdentifierTextAlt)
        {
            if (command != "") command += ",";
            command += "ObservationIdentifierTextAlt = '" + SOut.String(ehrLabResult.ObservationIdentifierTextAlt) + "'";
        }

        if (ehrLabResult.ObservationIdentifierCodeSystemNameAlt != oldEhrLabResult.ObservationIdentifierCodeSystemNameAlt)
        {
            if (command != "") command += ",";
            command += "ObservationIdentifierCodeSystemNameAlt = '" + SOut.String(ehrLabResult.ObservationIdentifierCodeSystemNameAlt) + "'";
        }

        if (ehrLabResult.ObservationIdentifierTextOriginal != oldEhrLabResult.ObservationIdentifierTextOriginal)
        {
            if (command != "") command += ",";
            command += "ObservationIdentifierTextOriginal = '" + SOut.String(ehrLabResult.ObservationIdentifierTextOriginal) + "'";
        }

        if (ehrLabResult.ObservationIdentifierSub != oldEhrLabResult.ObservationIdentifierSub)
        {
            if (command != "") command += ",";
            command += "ObservationIdentifierSub = '" + SOut.String(ehrLabResult.ObservationIdentifierSub) + "'";
        }

        if (ehrLabResult.ObservationValueCodedElementID != oldEhrLabResult.ObservationValueCodedElementID)
        {
            if (command != "") command += ",";
            command += "ObservationValueCodedElementID = '" + SOut.String(ehrLabResult.ObservationValueCodedElementID) + "'";
        }

        if (ehrLabResult.ObservationValueCodedElementText != oldEhrLabResult.ObservationValueCodedElementText)
        {
            if (command != "") command += ",";
            command += "ObservationValueCodedElementText = '" + SOut.String(ehrLabResult.ObservationValueCodedElementText) + "'";
        }

        if (ehrLabResult.ObservationValueCodedElementCodeSystemName != oldEhrLabResult.ObservationValueCodedElementCodeSystemName)
        {
            if (command != "") command += ",";
            command += "ObservationValueCodedElementCodeSystemName = '" + SOut.String(ehrLabResult.ObservationValueCodedElementCodeSystemName) + "'";
        }

        if (ehrLabResult.ObservationValueCodedElementIDAlt != oldEhrLabResult.ObservationValueCodedElementIDAlt)
        {
            if (command != "") command += ",";
            command += "ObservationValueCodedElementIDAlt = '" + SOut.String(ehrLabResult.ObservationValueCodedElementIDAlt) + "'";
        }

        if (ehrLabResult.ObservationValueCodedElementTextAlt != oldEhrLabResult.ObservationValueCodedElementTextAlt)
        {
            if (command != "") command += ",";
            command += "ObservationValueCodedElementTextAlt = '" + SOut.String(ehrLabResult.ObservationValueCodedElementTextAlt) + "'";
        }

        if (ehrLabResult.ObservationValueCodedElementCodeSystemNameAlt != oldEhrLabResult.ObservationValueCodedElementCodeSystemNameAlt)
        {
            if (command != "") command += ",";
            command += "ObservationValueCodedElementCodeSystemNameAlt = '" + SOut.String(ehrLabResult.ObservationValueCodedElementCodeSystemNameAlt) + "'";
        }

        if (ehrLabResult.ObservationValueCodedElementTextOriginal != oldEhrLabResult.ObservationValueCodedElementTextOriginal)
        {
            if (command != "") command += ",";
            command += "ObservationValueCodedElementTextOriginal = '" + SOut.String(ehrLabResult.ObservationValueCodedElementTextOriginal) + "'";
        }

        if (ehrLabResult.ObservationValueDateTime != oldEhrLabResult.ObservationValueDateTime)
        {
            if (command != "") command += ",";
            command += "ObservationValueDateTime = '" + SOut.String(ehrLabResult.ObservationValueDateTime) + "'";
        }

        if (ehrLabResult.ObservationValueTime != oldEhrLabResult.ObservationValueTime)
        {
            if (command != "") command += ",";
            command += "ObservationValueTime = " + SOut.Time(ehrLabResult.ObservationValueTime) + "";
        }

        if (ehrLabResult.ObservationValueComparator != oldEhrLabResult.ObservationValueComparator)
        {
            if (command != "") command += ",";
            command += "ObservationValueComparator = '" + SOut.String(ehrLabResult.ObservationValueComparator) + "'";
        }

        if (ehrLabResult.ObservationValueNumber1 != oldEhrLabResult.ObservationValueNumber1)
        {
            if (command != "") command += ",";
            command += "ObservationValueNumber1 = " + SOut.Double(ehrLabResult.ObservationValueNumber1) + "";
        }

        if (ehrLabResult.ObservationValueSeparatorOrSuffix != oldEhrLabResult.ObservationValueSeparatorOrSuffix)
        {
            if (command != "") command += ",";
            command += "ObservationValueSeparatorOrSuffix = '" + SOut.String(ehrLabResult.ObservationValueSeparatorOrSuffix) + "'";
        }

        if (ehrLabResult.ObservationValueNumber2 != oldEhrLabResult.ObservationValueNumber2)
        {
            if (command != "") command += ",";
            command += "ObservationValueNumber2 = " + SOut.Double(ehrLabResult.ObservationValueNumber2) + "";
        }

        if (ehrLabResult.ObservationValueNumeric != oldEhrLabResult.ObservationValueNumeric)
        {
            if (command != "") command += ",";
            command += "ObservationValueNumeric = " + SOut.Double(ehrLabResult.ObservationValueNumeric) + "";
        }

        if (ehrLabResult.ObservationValueText != oldEhrLabResult.ObservationValueText)
        {
            if (command != "") command += ",";
            command += "ObservationValueText = '" + SOut.String(ehrLabResult.ObservationValueText) + "'";
        }

        if (ehrLabResult.UnitsID != oldEhrLabResult.UnitsID)
        {
            if (command != "") command += ",";
            command += "UnitsID = '" + SOut.String(ehrLabResult.UnitsID) + "'";
        }

        if (ehrLabResult.UnitsText != oldEhrLabResult.UnitsText)
        {
            if (command != "") command += ",";
            command += "UnitsText = '" + SOut.String(ehrLabResult.UnitsText) + "'";
        }

        if (ehrLabResult.UnitsCodeSystemName != oldEhrLabResult.UnitsCodeSystemName)
        {
            if (command != "") command += ",";
            command += "UnitsCodeSystemName = '" + SOut.String(ehrLabResult.UnitsCodeSystemName) + "'";
        }

        if (ehrLabResult.UnitsIDAlt != oldEhrLabResult.UnitsIDAlt)
        {
            if (command != "") command += ",";
            command += "UnitsIDAlt = '" + SOut.String(ehrLabResult.UnitsIDAlt) + "'";
        }

        if (ehrLabResult.UnitsTextAlt != oldEhrLabResult.UnitsTextAlt)
        {
            if (command != "") command += ",";
            command += "UnitsTextAlt = '" + SOut.String(ehrLabResult.UnitsTextAlt) + "'";
        }

        if (ehrLabResult.UnitsCodeSystemNameAlt != oldEhrLabResult.UnitsCodeSystemNameAlt)
        {
            if (command != "") command += ",";
            command += "UnitsCodeSystemNameAlt = '" + SOut.String(ehrLabResult.UnitsCodeSystemNameAlt) + "'";
        }

        if (ehrLabResult.UnitsTextOriginal != oldEhrLabResult.UnitsTextOriginal)
        {
            if (command != "") command += ",";
            command += "UnitsTextOriginal = '" + SOut.String(ehrLabResult.UnitsTextOriginal) + "'";
        }

        if (ehrLabResult.referenceRange != oldEhrLabResult.referenceRange)
        {
            if (command != "") command += ",";
            command += "referenceRange = '" + SOut.String(ehrLabResult.referenceRange) + "'";
        }

        if (ehrLabResult.AbnormalFlags != oldEhrLabResult.AbnormalFlags)
        {
            if (command != "") command += ",";
            command += "AbnormalFlags = '" + SOut.String(ehrLabResult.AbnormalFlags) + "'";
        }

        if (ehrLabResult.ObservationResultStatus != oldEhrLabResult.ObservationResultStatus)
        {
            if (command != "") command += ",";
            command += "ObservationResultStatus = '" + SOut.String(ehrLabResult.ObservationResultStatus.ToString()) + "'";
        }

        if (ehrLabResult.ObservationDateTime != oldEhrLabResult.ObservationDateTime)
        {
            if (command != "") command += ",";
            command += "ObservationDateTime = '" + SOut.String(ehrLabResult.ObservationDateTime) + "'";
        }

        if (ehrLabResult.AnalysisDateTime != oldEhrLabResult.AnalysisDateTime)
        {
            if (command != "") command += ",";
            command += "AnalysisDateTime = '" + SOut.String(ehrLabResult.AnalysisDateTime) + "'";
        }

        if (ehrLabResult.PerformingOrganizationName != oldEhrLabResult.PerformingOrganizationName)
        {
            if (command != "") command += ",";
            command += "PerformingOrganizationName = '" + SOut.String(ehrLabResult.PerformingOrganizationName) + "'";
        }

        if (ehrLabResult.PerformingOrganizationNameAssigningAuthorityNamespaceId != oldEhrLabResult.PerformingOrganizationNameAssigningAuthorityNamespaceId)
        {
            if (command != "") command += ",";
            command += "PerformingOrganizationNameAssigningAuthorityNamespaceId = '" + SOut.String(ehrLabResult.PerformingOrganizationNameAssigningAuthorityNamespaceId) + "'";
        }

        if (ehrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalId != oldEhrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalId)
        {
            if (command != "") command += ",";
            command += "PerformingOrganizationNameAssigningAuthorityUniversalId = '" + SOut.String(ehrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalId) + "'";
        }

        if (ehrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalIdType != oldEhrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalIdType)
        {
            if (command != "") command += ",";
            command += "PerformingOrganizationNameAssigningAuthorityUniversalIdType = '" + SOut.String(ehrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalIdType) + "'";
        }

        if (ehrLabResult.PerformingOrganizationIdentifierTypeCode != oldEhrLabResult.PerformingOrganizationIdentifierTypeCode)
        {
            if (command != "") command += ",";
            command += "PerformingOrganizationIdentifierTypeCode = '" + SOut.String(ehrLabResult.PerformingOrganizationIdentifierTypeCode.ToString()) + "'";
        }

        if (ehrLabResult.PerformingOrganizationIdentifier != oldEhrLabResult.PerformingOrganizationIdentifier)
        {
            if (command != "") command += ",";
            command += "PerformingOrganizationIdentifier = '" + SOut.String(ehrLabResult.PerformingOrganizationIdentifier) + "'";
        }

        if (ehrLabResult.PerformingOrganizationAddressStreet != oldEhrLabResult.PerformingOrganizationAddressStreet)
        {
            if (command != "") command += ",";
            command += "PerformingOrganizationAddressStreet = '" + SOut.String(ehrLabResult.PerformingOrganizationAddressStreet) + "'";
        }

        if (ehrLabResult.PerformingOrganizationAddressOtherDesignation != oldEhrLabResult.PerformingOrganizationAddressOtherDesignation)
        {
            if (command != "") command += ",";
            command += "PerformingOrganizationAddressOtherDesignation = '" + SOut.String(ehrLabResult.PerformingOrganizationAddressOtherDesignation) + "'";
        }

        if (ehrLabResult.PerformingOrganizationAddressCity != oldEhrLabResult.PerformingOrganizationAddressCity)
        {
            if (command != "") command += ",";
            command += "PerformingOrganizationAddressCity = '" + SOut.String(ehrLabResult.PerformingOrganizationAddressCity) + "'";
        }

        if (ehrLabResult.PerformingOrganizationAddressStateOrProvince != oldEhrLabResult.PerformingOrganizationAddressStateOrProvince)
        {
            if (command != "") command += ",";
            command += "PerformingOrganizationAddressStateOrProvince = '" + SOut.String(ehrLabResult.PerformingOrganizationAddressStateOrProvince.ToString()) + "'";
        }

        if (ehrLabResult.PerformingOrganizationAddressZipOrPostalCode != oldEhrLabResult.PerformingOrganizationAddressZipOrPostalCode)
        {
            if (command != "") command += ",";
            command += "PerformingOrganizationAddressZipOrPostalCode = '" + SOut.String(ehrLabResult.PerformingOrganizationAddressZipOrPostalCode) + "'";
        }

        if (ehrLabResult.PerformingOrganizationAddressCountryCode != oldEhrLabResult.PerformingOrganizationAddressCountryCode)
        {
            if (command != "") command += ",";
            command += "PerformingOrganizationAddressCountryCode = '" + SOut.String(ehrLabResult.PerformingOrganizationAddressCountryCode) + "'";
        }

        if (ehrLabResult.PerformingOrganizationAddressAddressType != oldEhrLabResult.PerformingOrganizationAddressAddressType)
        {
            if (command != "") command += ",";
            command += "PerformingOrganizationAddressAddressType = '" + SOut.String(ehrLabResult.PerformingOrganizationAddressAddressType.ToString()) + "'";
        }

        if (ehrLabResult.PerformingOrganizationAddressCountyOrParishCode != oldEhrLabResult.PerformingOrganizationAddressCountyOrParishCode)
        {
            if (command != "") command += ",";
            command += "PerformingOrganizationAddressCountyOrParishCode = '" + SOut.String(ehrLabResult.PerformingOrganizationAddressCountyOrParishCode) + "'";
        }

        if (ehrLabResult.MedicalDirectorID != oldEhrLabResult.MedicalDirectorID)
        {
            if (command != "") command += ",";
            command += "MedicalDirectorID = '" + SOut.String(ehrLabResult.MedicalDirectorID) + "'";
        }

        if (ehrLabResult.MedicalDirectorLName != oldEhrLabResult.MedicalDirectorLName)
        {
            if (command != "") command += ",";
            command += "MedicalDirectorLName = '" + SOut.String(ehrLabResult.MedicalDirectorLName) + "'";
        }

        if (ehrLabResult.MedicalDirectorFName != oldEhrLabResult.MedicalDirectorFName)
        {
            if (command != "") command += ",";
            command += "MedicalDirectorFName = '" + SOut.String(ehrLabResult.MedicalDirectorFName) + "'";
        }

        if (ehrLabResult.MedicalDirectorMiddleNames != oldEhrLabResult.MedicalDirectorMiddleNames)
        {
            if (command != "") command += ",";
            command += "MedicalDirectorMiddleNames = '" + SOut.String(ehrLabResult.MedicalDirectorMiddleNames) + "'";
        }

        if (ehrLabResult.MedicalDirectorSuffix != oldEhrLabResult.MedicalDirectorSuffix)
        {
            if (command != "") command += ",";
            command += "MedicalDirectorSuffix = '" + SOut.String(ehrLabResult.MedicalDirectorSuffix) + "'";
        }

        if (ehrLabResult.MedicalDirectorPrefix != oldEhrLabResult.MedicalDirectorPrefix)
        {
            if (command != "") command += ",";
            command += "MedicalDirectorPrefix = '" + SOut.String(ehrLabResult.MedicalDirectorPrefix) + "'";
        }

        if (ehrLabResult.MedicalDirectorAssigningAuthorityNamespaceID != oldEhrLabResult.MedicalDirectorAssigningAuthorityNamespaceID)
        {
            if (command != "") command += ",";
            command += "MedicalDirectorAssigningAuthorityNamespaceID = '" + SOut.String(ehrLabResult.MedicalDirectorAssigningAuthorityNamespaceID) + "'";
        }

        if (ehrLabResult.MedicalDirectorAssigningAuthorityUniversalID != oldEhrLabResult.MedicalDirectorAssigningAuthorityUniversalID)
        {
            if (command != "") command += ",";
            command += "MedicalDirectorAssigningAuthorityUniversalID = '" + SOut.String(ehrLabResult.MedicalDirectorAssigningAuthorityUniversalID) + "'";
        }

        if (ehrLabResult.MedicalDirectorAssigningAuthorityIDType != oldEhrLabResult.MedicalDirectorAssigningAuthorityIDType)
        {
            if (command != "") command += ",";
            command += "MedicalDirectorAssigningAuthorityIDType = '" + SOut.String(ehrLabResult.MedicalDirectorAssigningAuthorityIDType) + "'";
        }

        if (ehrLabResult.MedicalDirectorNameTypeCode != oldEhrLabResult.MedicalDirectorNameTypeCode)
        {
            if (command != "") command += ",";
            command += "MedicalDirectorNameTypeCode = '" + SOut.String(ehrLabResult.MedicalDirectorNameTypeCode.ToString()) + "'";
        }

        if (ehrLabResult.MedicalDirectorIdentifierTypeCode != oldEhrLabResult.MedicalDirectorIdentifierTypeCode)
        {
            if (command != "") command += ",";
            command += "MedicalDirectorIdentifierTypeCode = '" + SOut.String(ehrLabResult.MedicalDirectorIdentifierTypeCode.ToString()) + "'";
        }

        if (command == "") return false;
        command = "UPDATE ehrlabresult SET " + command
                                             + " WHERE EhrLabResultNum = " + SOut.Long(ehrLabResult.EhrLabResultNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EhrLabResult ehrLabResult, EhrLabResult oldEhrLabResult)
    {
        if (ehrLabResult.EhrLabNum != oldEhrLabResult.EhrLabNum) return true;
        if (ehrLabResult.SetIdOBX != oldEhrLabResult.SetIdOBX) return true;
        if (ehrLabResult.ValueType != oldEhrLabResult.ValueType) return true;
        if (ehrLabResult.ObservationIdentifierID != oldEhrLabResult.ObservationIdentifierID) return true;
        if (ehrLabResult.ObservationIdentifierText != oldEhrLabResult.ObservationIdentifierText) return true;
        if (ehrLabResult.ObservationIdentifierCodeSystemName != oldEhrLabResult.ObservationIdentifierCodeSystemName) return true;
        if (ehrLabResult.ObservationIdentifierIDAlt != oldEhrLabResult.ObservationIdentifierIDAlt) return true;
        if (ehrLabResult.ObservationIdentifierTextAlt != oldEhrLabResult.ObservationIdentifierTextAlt) return true;
        if (ehrLabResult.ObservationIdentifierCodeSystemNameAlt != oldEhrLabResult.ObservationIdentifierCodeSystemNameAlt) return true;
        if (ehrLabResult.ObservationIdentifierTextOriginal != oldEhrLabResult.ObservationIdentifierTextOriginal) return true;
        if (ehrLabResult.ObservationIdentifierSub != oldEhrLabResult.ObservationIdentifierSub) return true;
        if (ehrLabResult.ObservationValueCodedElementID != oldEhrLabResult.ObservationValueCodedElementID) return true;
        if (ehrLabResult.ObservationValueCodedElementText != oldEhrLabResult.ObservationValueCodedElementText) return true;
        if (ehrLabResult.ObservationValueCodedElementCodeSystemName != oldEhrLabResult.ObservationValueCodedElementCodeSystemName) return true;
        if (ehrLabResult.ObservationValueCodedElementIDAlt != oldEhrLabResult.ObservationValueCodedElementIDAlt) return true;
        if (ehrLabResult.ObservationValueCodedElementTextAlt != oldEhrLabResult.ObservationValueCodedElementTextAlt) return true;
        if (ehrLabResult.ObservationValueCodedElementCodeSystemNameAlt != oldEhrLabResult.ObservationValueCodedElementCodeSystemNameAlt) return true;
        if (ehrLabResult.ObservationValueCodedElementTextOriginal != oldEhrLabResult.ObservationValueCodedElementTextOriginal) return true;
        if (ehrLabResult.ObservationValueDateTime != oldEhrLabResult.ObservationValueDateTime) return true;
        if (ehrLabResult.ObservationValueTime != oldEhrLabResult.ObservationValueTime) return true;
        if (ehrLabResult.ObservationValueComparator != oldEhrLabResult.ObservationValueComparator) return true;
        if (ehrLabResult.ObservationValueNumber1 != oldEhrLabResult.ObservationValueNumber1) return true;
        if (ehrLabResult.ObservationValueSeparatorOrSuffix != oldEhrLabResult.ObservationValueSeparatorOrSuffix) return true;
        if (ehrLabResult.ObservationValueNumber2 != oldEhrLabResult.ObservationValueNumber2) return true;
        if (ehrLabResult.ObservationValueNumeric != oldEhrLabResult.ObservationValueNumeric) return true;
        if (ehrLabResult.ObservationValueText != oldEhrLabResult.ObservationValueText) return true;
        if (ehrLabResult.UnitsID != oldEhrLabResult.UnitsID) return true;
        if (ehrLabResult.UnitsText != oldEhrLabResult.UnitsText) return true;
        if (ehrLabResult.UnitsCodeSystemName != oldEhrLabResult.UnitsCodeSystemName) return true;
        if (ehrLabResult.UnitsIDAlt != oldEhrLabResult.UnitsIDAlt) return true;
        if (ehrLabResult.UnitsTextAlt != oldEhrLabResult.UnitsTextAlt) return true;
        if (ehrLabResult.UnitsCodeSystemNameAlt != oldEhrLabResult.UnitsCodeSystemNameAlt) return true;
        if (ehrLabResult.UnitsTextOriginal != oldEhrLabResult.UnitsTextOriginal) return true;
        if (ehrLabResult.referenceRange != oldEhrLabResult.referenceRange) return true;
        if (ehrLabResult.AbnormalFlags != oldEhrLabResult.AbnormalFlags) return true;
        if (ehrLabResult.ObservationResultStatus != oldEhrLabResult.ObservationResultStatus) return true;
        if (ehrLabResult.ObservationDateTime != oldEhrLabResult.ObservationDateTime) return true;
        if (ehrLabResult.AnalysisDateTime != oldEhrLabResult.AnalysisDateTime) return true;
        if (ehrLabResult.PerformingOrganizationName != oldEhrLabResult.PerformingOrganizationName) return true;
        if (ehrLabResult.PerformingOrganizationNameAssigningAuthorityNamespaceId != oldEhrLabResult.PerformingOrganizationNameAssigningAuthorityNamespaceId) return true;
        if (ehrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalId != oldEhrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalId) return true;
        if (ehrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalIdType != oldEhrLabResult.PerformingOrganizationNameAssigningAuthorityUniversalIdType) return true;
        if (ehrLabResult.PerformingOrganizationIdentifierTypeCode != oldEhrLabResult.PerformingOrganizationIdentifierTypeCode) return true;
        if (ehrLabResult.PerformingOrganizationIdentifier != oldEhrLabResult.PerformingOrganizationIdentifier) return true;
        if (ehrLabResult.PerformingOrganizationAddressStreet != oldEhrLabResult.PerformingOrganizationAddressStreet) return true;
        if (ehrLabResult.PerformingOrganizationAddressOtherDesignation != oldEhrLabResult.PerformingOrganizationAddressOtherDesignation) return true;
        if (ehrLabResult.PerformingOrganizationAddressCity != oldEhrLabResult.PerformingOrganizationAddressCity) return true;
        if (ehrLabResult.PerformingOrganizationAddressStateOrProvince != oldEhrLabResult.PerformingOrganizationAddressStateOrProvince) return true;
        if (ehrLabResult.PerformingOrganizationAddressZipOrPostalCode != oldEhrLabResult.PerformingOrganizationAddressZipOrPostalCode) return true;
        if (ehrLabResult.PerformingOrganizationAddressCountryCode != oldEhrLabResult.PerformingOrganizationAddressCountryCode) return true;
        if (ehrLabResult.PerformingOrganizationAddressAddressType != oldEhrLabResult.PerformingOrganizationAddressAddressType) return true;
        if (ehrLabResult.PerformingOrganizationAddressCountyOrParishCode != oldEhrLabResult.PerformingOrganizationAddressCountyOrParishCode) return true;
        if (ehrLabResult.MedicalDirectorID != oldEhrLabResult.MedicalDirectorID) return true;
        if (ehrLabResult.MedicalDirectorLName != oldEhrLabResult.MedicalDirectorLName) return true;
        if (ehrLabResult.MedicalDirectorFName != oldEhrLabResult.MedicalDirectorFName) return true;
        if (ehrLabResult.MedicalDirectorMiddleNames != oldEhrLabResult.MedicalDirectorMiddleNames) return true;
        if (ehrLabResult.MedicalDirectorSuffix != oldEhrLabResult.MedicalDirectorSuffix) return true;
        if (ehrLabResult.MedicalDirectorPrefix != oldEhrLabResult.MedicalDirectorPrefix) return true;
        if (ehrLabResult.MedicalDirectorAssigningAuthorityNamespaceID != oldEhrLabResult.MedicalDirectorAssigningAuthorityNamespaceID) return true;
        if (ehrLabResult.MedicalDirectorAssigningAuthorityUniversalID != oldEhrLabResult.MedicalDirectorAssigningAuthorityUniversalID) return true;
        if (ehrLabResult.MedicalDirectorAssigningAuthorityIDType != oldEhrLabResult.MedicalDirectorAssigningAuthorityIDType) return true;
        if (ehrLabResult.MedicalDirectorNameTypeCode != oldEhrLabResult.MedicalDirectorNameTypeCode) return true;
        if (ehrLabResult.MedicalDirectorIdentifierTypeCode != oldEhrLabResult.MedicalDirectorIdentifierTypeCode) return true;
        return false;
    }

    public static void Delete(long ehrLabResultNum)
    {
        var command = "DELETE FROM ehrlabresult "
                      + "WHERE EhrLabResultNum = " + SOut.Long(ehrLabResultNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrLabResultNums)
    {
        if (listEhrLabResultNums == null || listEhrLabResultNums.Count == 0) return;
        var command = "DELETE FROM ehrlabresult "
                      + "WHERE EhrLabResultNum IN(" + string.Join(",", listEhrLabResultNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}