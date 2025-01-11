#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;
using EhrLaboratories;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrLabResultsCopyToCrud
{
    public static EhrLabResultsCopyTo SelectOne(long ehrLabResultsCopyToNum)
    {
        var command = "SELECT * FROM ehrlabresultscopyto "
                      + "WHERE EhrLabResultsCopyToNum = " + SOut.Long(ehrLabResultsCopyToNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrLabResultsCopyTo SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrLabResultsCopyTo> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrLabResultsCopyTo> TableToList(DataTable table)
    {
        var retVal = new List<EhrLabResultsCopyTo>();
        EhrLabResultsCopyTo ehrLabResultsCopyTo;
        foreach (DataRow row in table.Rows)
        {
            ehrLabResultsCopyTo = new EhrLabResultsCopyTo();
            ehrLabResultsCopyTo.EhrLabResultsCopyToNum = SIn.Long(row["EhrLabResultsCopyToNum"].ToString());
            ehrLabResultsCopyTo.EhrLabNum = SIn.Long(row["EhrLabNum"].ToString());
            ehrLabResultsCopyTo.CopyToID = SIn.String(row["CopyToID"].ToString());
            ehrLabResultsCopyTo.CopyToLName = SIn.String(row["CopyToLName"].ToString());
            ehrLabResultsCopyTo.CopyToFName = SIn.String(row["CopyToFName"].ToString());
            ehrLabResultsCopyTo.CopyToMiddleNames = SIn.String(row["CopyToMiddleNames"].ToString());
            ehrLabResultsCopyTo.CopyToSuffix = SIn.String(row["CopyToSuffix"].ToString());
            ehrLabResultsCopyTo.CopyToPrefix = SIn.String(row["CopyToPrefix"].ToString());
            ehrLabResultsCopyTo.CopyToAssigningAuthorityNamespaceID = SIn.String(row["CopyToAssigningAuthorityNamespaceID"].ToString());
            ehrLabResultsCopyTo.CopyToAssigningAuthorityUniversalID = SIn.String(row["CopyToAssigningAuthorityUniversalID"].ToString());
            ehrLabResultsCopyTo.CopyToAssigningAuthorityIDType = SIn.String(row["CopyToAssigningAuthorityIDType"].ToString());
            var copyToNameTypeCode = row["CopyToNameTypeCode"].ToString();
            if (copyToNameTypeCode == "")
                ehrLabResultsCopyTo.CopyToNameTypeCode = 0;
            else
                try
                {
                    ehrLabResultsCopyTo.CopyToNameTypeCode = (HL70200) Enum.Parse(typeof(HL70200), copyToNameTypeCode);
                }
                catch
                {
                    ehrLabResultsCopyTo.CopyToNameTypeCode = 0;
                }

            var copyToIdentifierTypeCode = row["CopyToIdentifierTypeCode"].ToString();
            if (copyToIdentifierTypeCode == "")
                ehrLabResultsCopyTo.CopyToIdentifierTypeCode = 0;
            else
                try
                {
                    ehrLabResultsCopyTo.CopyToIdentifierTypeCode = (HL70203) Enum.Parse(typeof(HL70203), copyToIdentifierTypeCode);
                }
                catch
                {
                    ehrLabResultsCopyTo.CopyToIdentifierTypeCode = 0;
                }

            retVal.Add(ehrLabResultsCopyTo);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrLabResultsCopyTo> listEhrLabResultsCopyTos, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrLabResultsCopyTo";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrLabResultsCopyToNum");
        table.Columns.Add("EhrLabNum");
        table.Columns.Add("CopyToID");
        table.Columns.Add("CopyToLName");
        table.Columns.Add("CopyToFName");
        table.Columns.Add("CopyToMiddleNames");
        table.Columns.Add("CopyToSuffix");
        table.Columns.Add("CopyToPrefix");
        table.Columns.Add("CopyToAssigningAuthorityNamespaceID");
        table.Columns.Add("CopyToAssigningAuthorityUniversalID");
        table.Columns.Add("CopyToAssigningAuthorityIDType");
        table.Columns.Add("CopyToNameTypeCode");
        table.Columns.Add("CopyToIdentifierTypeCode");
        foreach (var ehrLabResultsCopyTo in listEhrLabResultsCopyTos)
            table.Rows.Add(SOut.Long(ehrLabResultsCopyTo.EhrLabResultsCopyToNum), SOut.Long(ehrLabResultsCopyTo.EhrLabNum), ehrLabResultsCopyTo.CopyToID, ehrLabResultsCopyTo.CopyToLName, ehrLabResultsCopyTo.CopyToFName, ehrLabResultsCopyTo.CopyToMiddleNames, ehrLabResultsCopyTo.CopyToSuffix, ehrLabResultsCopyTo.CopyToPrefix, ehrLabResultsCopyTo.CopyToAssigningAuthorityNamespaceID, ehrLabResultsCopyTo.CopyToAssigningAuthorityUniversalID, ehrLabResultsCopyTo.CopyToAssigningAuthorityIDType, SOut.Int((int) ehrLabResultsCopyTo.CopyToNameTypeCode), SOut.Int((int) ehrLabResultsCopyTo.CopyToIdentifierTypeCode));
        return table;
    }

    public static long Insert(EhrLabResultsCopyTo ehrLabResultsCopyTo)
    {
        return Insert(ehrLabResultsCopyTo, false);
    }

    public static long Insert(EhrLabResultsCopyTo ehrLabResultsCopyTo, bool useExistingPK)
    {
        var command = "INSERT INTO ehrlabresultscopyto (";

        command += "EhrLabNum,CopyToID,CopyToLName,CopyToFName,CopyToMiddleNames,CopyToSuffix,CopyToPrefix,CopyToAssigningAuthorityNamespaceID,CopyToAssigningAuthorityUniversalID,CopyToAssigningAuthorityIDType,CopyToNameTypeCode,CopyToIdentifierTypeCode) VALUES(";

        command +=
            SOut.Long(ehrLabResultsCopyTo.EhrLabNum) + ","
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToID) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToLName) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToFName) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToMiddleNames) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToSuffix) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToPrefix) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToAssigningAuthorityNamespaceID) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToAssigningAuthorityUniversalID) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToAssigningAuthorityIDType) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToNameTypeCode.ToString()) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToIdentifierTypeCode.ToString()) + "')";
        {
            ehrLabResultsCopyTo.EhrLabResultsCopyToNum = Db.NonQ(command, true, "EhrLabResultsCopyToNum", "ehrLabResultsCopyTo");
        }
        return ehrLabResultsCopyTo.EhrLabResultsCopyToNum;
    }

    public static long InsertNoCache(EhrLabResultsCopyTo ehrLabResultsCopyTo)
    {
        return InsertNoCache(ehrLabResultsCopyTo, false);
    }

    public static long InsertNoCache(EhrLabResultsCopyTo ehrLabResultsCopyTo, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrlabresultscopyto (";
        if (isRandomKeys || useExistingPK) command += "EhrLabResultsCopyToNum,";
        command += "EhrLabNum,CopyToID,CopyToLName,CopyToFName,CopyToMiddleNames,CopyToSuffix,CopyToPrefix,CopyToAssigningAuthorityNamespaceID,CopyToAssigningAuthorityUniversalID,CopyToAssigningAuthorityIDType,CopyToNameTypeCode,CopyToIdentifierTypeCode) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrLabResultsCopyTo.EhrLabResultsCopyToNum) + ",";
        command +=
            SOut.Long(ehrLabResultsCopyTo.EhrLabNum) + ","
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToID) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToLName) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToFName) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToMiddleNames) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToSuffix) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToPrefix) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToAssigningAuthorityNamespaceID) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToAssigningAuthorityUniversalID) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToAssigningAuthorityIDType) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToNameTypeCode.ToString()) + "',"
                                                     + "'" + SOut.String(ehrLabResultsCopyTo.CopyToIdentifierTypeCode.ToString()) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            ehrLabResultsCopyTo.EhrLabResultsCopyToNum = Db.NonQ(command, true, "EhrLabResultsCopyToNum", "ehrLabResultsCopyTo");
        return ehrLabResultsCopyTo.EhrLabResultsCopyToNum;
    }

    public static void Update(EhrLabResultsCopyTo ehrLabResultsCopyTo)
    {
        var command = "UPDATE ehrlabresultscopyto SET "
                      + "EhrLabNum                          =  " + SOut.Long(ehrLabResultsCopyTo.EhrLabNum) + ", "
                      + "CopyToID                           = '" + SOut.String(ehrLabResultsCopyTo.CopyToID) + "', "
                      + "CopyToLName                        = '" + SOut.String(ehrLabResultsCopyTo.CopyToLName) + "', "
                      + "CopyToFName                        = '" + SOut.String(ehrLabResultsCopyTo.CopyToFName) + "', "
                      + "CopyToMiddleNames                  = '" + SOut.String(ehrLabResultsCopyTo.CopyToMiddleNames) + "', "
                      + "CopyToSuffix                       = '" + SOut.String(ehrLabResultsCopyTo.CopyToSuffix) + "', "
                      + "CopyToPrefix                       = '" + SOut.String(ehrLabResultsCopyTo.CopyToPrefix) + "', "
                      + "CopyToAssigningAuthorityNamespaceID= '" + SOut.String(ehrLabResultsCopyTo.CopyToAssigningAuthorityNamespaceID) + "', "
                      + "CopyToAssigningAuthorityUniversalID= '" + SOut.String(ehrLabResultsCopyTo.CopyToAssigningAuthorityUniversalID) + "', "
                      + "CopyToAssigningAuthorityIDType     = '" + SOut.String(ehrLabResultsCopyTo.CopyToAssigningAuthorityIDType) + "', "
                      + "CopyToNameTypeCode                 = '" + SOut.String(ehrLabResultsCopyTo.CopyToNameTypeCode.ToString()) + "', "
                      + "CopyToIdentifierTypeCode           = '" + SOut.String(ehrLabResultsCopyTo.CopyToIdentifierTypeCode.ToString()) + "' "
                      + "WHERE EhrLabResultsCopyToNum = " + SOut.Long(ehrLabResultsCopyTo.EhrLabResultsCopyToNum);
        Db.NonQ(command);
    }

    public static bool Update(EhrLabResultsCopyTo ehrLabResultsCopyTo, EhrLabResultsCopyTo oldEhrLabResultsCopyTo)
    {
        var command = "";
        if (ehrLabResultsCopyTo.EhrLabNum != oldEhrLabResultsCopyTo.EhrLabNum)
        {
            if (command != "") command += ",";
            command += "EhrLabNum = " + SOut.Long(ehrLabResultsCopyTo.EhrLabNum) + "";
        }

        if (ehrLabResultsCopyTo.CopyToID != oldEhrLabResultsCopyTo.CopyToID)
        {
            if (command != "") command += ",";
            command += "CopyToID = '" + SOut.String(ehrLabResultsCopyTo.CopyToID) + "'";
        }

        if (ehrLabResultsCopyTo.CopyToLName != oldEhrLabResultsCopyTo.CopyToLName)
        {
            if (command != "") command += ",";
            command += "CopyToLName = '" + SOut.String(ehrLabResultsCopyTo.CopyToLName) + "'";
        }

        if (ehrLabResultsCopyTo.CopyToFName != oldEhrLabResultsCopyTo.CopyToFName)
        {
            if (command != "") command += ",";
            command += "CopyToFName = '" + SOut.String(ehrLabResultsCopyTo.CopyToFName) + "'";
        }

        if (ehrLabResultsCopyTo.CopyToMiddleNames != oldEhrLabResultsCopyTo.CopyToMiddleNames)
        {
            if (command != "") command += ",";
            command += "CopyToMiddleNames = '" + SOut.String(ehrLabResultsCopyTo.CopyToMiddleNames) + "'";
        }

        if (ehrLabResultsCopyTo.CopyToSuffix != oldEhrLabResultsCopyTo.CopyToSuffix)
        {
            if (command != "") command += ",";
            command += "CopyToSuffix = '" + SOut.String(ehrLabResultsCopyTo.CopyToSuffix) + "'";
        }

        if (ehrLabResultsCopyTo.CopyToPrefix != oldEhrLabResultsCopyTo.CopyToPrefix)
        {
            if (command != "") command += ",";
            command += "CopyToPrefix = '" + SOut.String(ehrLabResultsCopyTo.CopyToPrefix) + "'";
        }

        if (ehrLabResultsCopyTo.CopyToAssigningAuthorityNamespaceID != oldEhrLabResultsCopyTo.CopyToAssigningAuthorityNamespaceID)
        {
            if (command != "") command += ",";
            command += "CopyToAssigningAuthorityNamespaceID = '" + SOut.String(ehrLabResultsCopyTo.CopyToAssigningAuthorityNamespaceID) + "'";
        }

        if (ehrLabResultsCopyTo.CopyToAssigningAuthorityUniversalID != oldEhrLabResultsCopyTo.CopyToAssigningAuthorityUniversalID)
        {
            if (command != "") command += ",";
            command += "CopyToAssigningAuthorityUniversalID = '" + SOut.String(ehrLabResultsCopyTo.CopyToAssigningAuthorityUniversalID) + "'";
        }

        if (ehrLabResultsCopyTo.CopyToAssigningAuthorityIDType != oldEhrLabResultsCopyTo.CopyToAssigningAuthorityIDType)
        {
            if (command != "") command += ",";
            command += "CopyToAssigningAuthorityIDType = '" + SOut.String(ehrLabResultsCopyTo.CopyToAssigningAuthorityIDType) + "'";
        }

        if (ehrLabResultsCopyTo.CopyToNameTypeCode != oldEhrLabResultsCopyTo.CopyToNameTypeCode)
        {
            if (command != "") command += ",";
            command += "CopyToNameTypeCode = '" + SOut.String(ehrLabResultsCopyTo.CopyToNameTypeCode.ToString()) + "'";
        }

        if (ehrLabResultsCopyTo.CopyToIdentifierTypeCode != oldEhrLabResultsCopyTo.CopyToIdentifierTypeCode)
        {
            if (command != "") command += ",";
            command += "CopyToIdentifierTypeCode = '" + SOut.String(ehrLabResultsCopyTo.CopyToIdentifierTypeCode.ToString()) + "'";
        }

        if (command == "") return false;
        command = "UPDATE ehrlabresultscopyto SET " + command
                                                    + " WHERE EhrLabResultsCopyToNum = " + SOut.Long(ehrLabResultsCopyTo.EhrLabResultsCopyToNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EhrLabResultsCopyTo ehrLabResultsCopyTo, EhrLabResultsCopyTo oldEhrLabResultsCopyTo)
    {
        if (ehrLabResultsCopyTo.EhrLabNum != oldEhrLabResultsCopyTo.EhrLabNum) return true;
        if (ehrLabResultsCopyTo.CopyToID != oldEhrLabResultsCopyTo.CopyToID) return true;
        if (ehrLabResultsCopyTo.CopyToLName != oldEhrLabResultsCopyTo.CopyToLName) return true;
        if (ehrLabResultsCopyTo.CopyToFName != oldEhrLabResultsCopyTo.CopyToFName) return true;
        if (ehrLabResultsCopyTo.CopyToMiddleNames != oldEhrLabResultsCopyTo.CopyToMiddleNames) return true;
        if (ehrLabResultsCopyTo.CopyToSuffix != oldEhrLabResultsCopyTo.CopyToSuffix) return true;
        if (ehrLabResultsCopyTo.CopyToPrefix != oldEhrLabResultsCopyTo.CopyToPrefix) return true;
        if (ehrLabResultsCopyTo.CopyToAssigningAuthorityNamespaceID != oldEhrLabResultsCopyTo.CopyToAssigningAuthorityNamespaceID) return true;
        if (ehrLabResultsCopyTo.CopyToAssigningAuthorityUniversalID != oldEhrLabResultsCopyTo.CopyToAssigningAuthorityUniversalID) return true;
        if (ehrLabResultsCopyTo.CopyToAssigningAuthorityIDType != oldEhrLabResultsCopyTo.CopyToAssigningAuthorityIDType) return true;
        if (ehrLabResultsCopyTo.CopyToNameTypeCode != oldEhrLabResultsCopyTo.CopyToNameTypeCode) return true;
        if (ehrLabResultsCopyTo.CopyToIdentifierTypeCode != oldEhrLabResultsCopyTo.CopyToIdentifierTypeCode) return true;
        return false;
    }

    public static void Delete(long ehrLabResultsCopyToNum)
    {
        var command = "DELETE FROM ehrlabresultscopyto "
                      + "WHERE EhrLabResultsCopyToNum = " + SOut.Long(ehrLabResultsCopyToNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrLabResultsCopyToNums)
    {
        if (listEhrLabResultsCopyToNums == null || listEhrLabResultsCopyToNums.Count == 0) return;
        var command = "DELETE FROM ehrlabresultscopyto "
                      + "WHERE EhrLabResultsCopyToNum IN(" + string.Join(",", listEhrLabResultsCopyToNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}