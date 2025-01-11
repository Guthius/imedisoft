#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class EhrCodeCrud
{
    public static EhrCode SelectOne(long ehrCodeNum)
    {
        var command = "SELECT * FROM ehrcode "
                      + "WHERE EhrCodeNum = " + SOut.Long(ehrCodeNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static EhrCode SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<EhrCode> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<EhrCode> TableToList(DataTable table)
    {
        var retVal = new List<EhrCode>();
        EhrCode ehrCode;
        foreach (DataRow row in table.Rows)
        {
            ehrCode = new EhrCode();
            ehrCode.EhrCodeNum = SIn.Long(row["EhrCodeNum"].ToString());
            ehrCode.MeasureIds = SIn.String(row["MeasureIds"].ToString());
            ehrCode.ValueSetName = SIn.String(row["ValueSetName"].ToString());
            ehrCode.ValueSetOID = SIn.String(row["ValueSetOID"].ToString());
            ehrCode.QDMCategory = SIn.String(row["QDMCategory"].ToString());
            ehrCode.CodeValue = SIn.String(row["CodeValue"].ToString());
            ehrCode.Description = SIn.String(row["Description"].ToString());
            ehrCode.CodeSystem = SIn.String(row["CodeSystem"].ToString());
            ehrCode.CodeSystemOID = SIn.String(row["CodeSystemOID"].ToString());
            ehrCode.IsInDb = SIn.Bool(row["IsInDb"].ToString());
            retVal.Add(ehrCode);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<EhrCode> listEhrCodes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "EhrCode";
        var table = new DataTable(tableName);
        table.Columns.Add("EhrCodeNum");
        table.Columns.Add("MeasureIds");
        table.Columns.Add("ValueSetName");
        table.Columns.Add("ValueSetOID");
        table.Columns.Add("QDMCategory");
        table.Columns.Add("CodeValue");
        table.Columns.Add("Description");
        table.Columns.Add("CodeSystem");
        table.Columns.Add("CodeSystemOID");
        table.Columns.Add("IsInDb");
        foreach (var ehrCode in listEhrCodes)
            table.Rows.Add(SOut.Long(ehrCode.EhrCodeNum), ehrCode.MeasureIds, ehrCode.ValueSetName, ehrCode.ValueSetOID, ehrCode.QDMCategory, ehrCode.CodeValue, ehrCode.Description, ehrCode.CodeSystem, ehrCode.CodeSystemOID, SOut.Bool(ehrCode.IsInDb));
        return table;
    }

    public static long Insert(EhrCode ehrCode)
    {
        return Insert(ehrCode, false);
    }

    public static long Insert(EhrCode ehrCode, bool useExistingPK)
    {
        var command = "INSERT INTO ehrcode (";

        command += "MeasureIds,ValueSetName,ValueSetOID,QDMCategory,CodeValue,Description,CodeSystem,CodeSystemOID,IsInDb) VALUES(";

        command +=
            "'" + SOut.String(ehrCode.MeasureIds) + "',"
            + "'" + SOut.String(ehrCode.ValueSetName) + "',"
            + "'" + SOut.String(ehrCode.ValueSetOID) + "',"
            + "'" + SOut.String(ehrCode.QDMCategory) + "',"
            + "'" + SOut.String(ehrCode.CodeValue) + "',"
            + "'" + SOut.String(ehrCode.Description) + "',"
            + "'" + SOut.String(ehrCode.CodeSystem) + "',"
            + "'" + SOut.String(ehrCode.CodeSystemOID) + "',"
            + SOut.Bool(ehrCode.IsInDb) + ")";
        {
            ehrCode.EhrCodeNum = Db.NonQ(command, true, "EhrCodeNum", "ehrCode");
        }
        return ehrCode.EhrCodeNum;
    }

    public static long InsertNoCache(EhrCode ehrCode)
    {
        return InsertNoCache(ehrCode, false);
    }

    public static long InsertNoCache(EhrCode ehrCode, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ehrcode (";
        if (isRandomKeys || useExistingPK) command += "EhrCodeNum,";
        command += "MeasureIds,ValueSetName,ValueSetOID,QDMCategory,CodeValue,Description,CodeSystem,CodeSystemOID,IsInDb) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ehrCode.EhrCodeNum) + ",";
        command +=
            "'" + SOut.String(ehrCode.MeasureIds) + "',"
            + "'" + SOut.String(ehrCode.ValueSetName) + "',"
            + "'" + SOut.String(ehrCode.ValueSetOID) + "',"
            + "'" + SOut.String(ehrCode.QDMCategory) + "',"
            + "'" + SOut.String(ehrCode.CodeValue) + "',"
            + "'" + SOut.String(ehrCode.Description) + "',"
            + "'" + SOut.String(ehrCode.CodeSystem) + "',"
            + "'" + SOut.String(ehrCode.CodeSystemOID) + "',"
            + SOut.Bool(ehrCode.IsInDb) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            ehrCode.EhrCodeNum = Db.NonQ(command, true, "EhrCodeNum", "ehrCode");
        return ehrCode.EhrCodeNum;
    }

    public static void Update(EhrCode ehrCode)
    {
        var command = "UPDATE ehrcode SET "
                      + "MeasureIds   = '" + SOut.String(ehrCode.MeasureIds) + "', "
                      + "ValueSetName = '" + SOut.String(ehrCode.ValueSetName) + "', "
                      + "ValueSetOID  = '" + SOut.String(ehrCode.ValueSetOID) + "', "
                      + "QDMCategory  = '" + SOut.String(ehrCode.QDMCategory) + "', "
                      + "CodeValue    = '" + SOut.String(ehrCode.CodeValue) + "', "
                      + "Description  = '" + SOut.String(ehrCode.Description) + "', "
                      + "CodeSystem   = '" + SOut.String(ehrCode.CodeSystem) + "', "
                      + "CodeSystemOID= '" + SOut.String(ehrCode.CodeSystemOID) + "', "
                      + "IsInDb       =  " + SOut.Bool(ehrCode.IsInDb) + " "
                      + "WHERE EhrCodeNum = " + SOut.Long(ehrCode.EhrCodeNum);
        Db.NonQ(command);
    }

    public static bool Update(EhrCode ehrCode, EhrCode oldEhrCode)
    {
        var command = "";
        if (ehrCode.MeasureIds != oldEhrCode.MeasureIds)
        {
            if (command != "") command += ",";
            command += "MeasureIds = '" + SOut.String(ehrCode.MeasureIds) + "'";
        }

        if (ehrCode.ValueSetName != oldEhrCode.ValueSetName)
        {
            if (command != "") command += ",";
            command += "ValueSetName = '" + SOut.String(ehrCode.ValueSetName) + "'";
        }

        if (ehrCode.ValueSetOID != oldEhrCode.ValueSetOID)
        {
            if (command != "") command += ",";
            command += "ValueSetOID = '" + SOut.String(ehrCode.ValueSetOID) + "'";
        }

        if (ehrCode.QDMCategory != oldEhrCode.QDMCategory)
        {
            if (command != "") command += ",";
            command += "QDMCategory = '" + SOut.String(ehrCode.QDMCategory) + "'";
        }

        if (ehrCode.CodeValue != oldEhrCode.CodeValue)
        {
            if (command != "") command += ",";
            command += "CodeValue = '" + SOut.String(ehrCode.CodeValue) + "'";
        }

        if (ehrCode.Description != oldEhrCode.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(ehrCode.Description) + "'";
        }

        if (ehrCode.CodeSystem != oldEhrCode.CodeSystem)
        {
            if (command != "") command += ",";
            command += "CodeSystem = '" + SOut.String(ehrCode.CodeSystem) + "'";
        }

        if (ehrCode.CodeSystemOID != oldEhrCode.CodeSystemOID)
        {
            if (command != "") command += ",";
            command += "CodeSystemOID = '" + SOut.String(ehrCode.CodeSystemOID) + "'";
        }

        if (ehrCode.IsInDb != oldEhrCode.IsInDb)
        {
            if (command != "") command += ",";
            command += "IsInDb = " + SOut.Bool(ehrCode.IsInDb) + "";
        }

        if (command == "") return false;
        command = "UPDATE ehrcode SET " + command
                                        + " WHERE EhrCodeNum = " + SOut.Long(ehrCode.EhrCodeNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(EhrCode ehrCode, EhrCode oldEhrCode)
    {
        if (ehrCode.MeasureIds != oldEhrCode.MeasureIds) return true;
        if (ehrCode.ValueSetName != oldEhrCode.ValueSetName) return true;
        if (ehrCode.ValueSetOID != oldEhrCode.ValueSetOID) return true;
        if (ehrCode.QDMCategory != oldEhrCode.QDMCategory) return true;
        if (ehrCode.CodeValue != oldEhrCode.CodeValue) return true;
        if (ehrCode.Description != oldEhrCode.Description) return true;
        if (ehrCode.CodeSystem != oldEhrCode.CodeSystem) return true;
        if (ehrCode.CodeSystemOID != oldEhrCode.CodeSystemOID) return true;
        if (ehrCode.IsInDb != oldEhrCode.IsInDb) return true;
        return false;
    }

    public static void Delete(long ehrCodeNum)
    {
        var command = "DELETE FROM ehrcode "
                      + "WHERE EhrCodeNum = " + SOut.Long(ehrCodeNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listEhrCodeNums)
    {
        if (listEhrCodeNums == null || listEhrCodeNums.Count == 0) return;
        var command = "DELETE FROM ehrcode "
                      + "WHERE EhrCodeNum IN(" + string.Join(",", listEhrCodeNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}