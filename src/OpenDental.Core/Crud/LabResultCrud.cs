#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class LabResultCrud
{
    public static LabResult SelectOne(long labResultNum)
    {
        var command = "SELECT * FROM labresult "
                      + "WHERE LabResultNum = " + SOut.Long(labResultNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static LabResult SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<LabResult> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<LabResult> TableToList(DataTable table)
    {
        var retVal = new List<LabResult>();
        LabResult labResult;
        foreach (DataRow row in table.Rows)
        {
            labResult = new LabResult();
            labResult.LabResultNum = SIn.Long(row["LabResultNum"].ToString());
            labResult.LabPanelNum = SIn.Long(row["LabPanelNum"].ToString());
            labResult.DateTimeTest = SIn.DateTime(row["DateTimeTest"].ToString());
            labResult.TestName = SIn.String(row["TestName"].ToString());
            labResult.DateTStamp = SIn.DateTime(row["DateTStamp"].ToString());
            labResult.TestID = SIn.String(row["TestID"].ToString());
            labResult.ObsValue = SIn.String(row["ObsValue"].ToString());
            labResult.ObsUnits = SIn.String(row["ObsUnits"].ToString());
            labResult.ObsRange = SIn.String(row["ObsRange"].ToString());
            labResult.AbnormalFlag = (LabAbnormalFlag) SIn.Int(row["AbnormalFlag"].ToString());
            retVal.Add(labResult);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<LabResult> listLabResults, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "LabResult";
        var table = new DataTable(tableName);
        table.Columns.Add("LabResultNum");
        table.Columns.Add("LabPanelNum");
        table.Columns.Add("DateTimeTest");
        table.Columns.Add("TestName");
        table.Columns.Add("DateTStamp");
        table.Columns.Add("TestID");
        table.Columns.Add("ObsValue");
        table.Columns.Add("ObsUnits");
        table.Columns.Add("ObsRange");
        table.Columns.Add("AbnormalFlag");
        foreach (var labResult in listLabResults)
            table.Rows.Add(SOut.Long(labResult.LabResultNum), SOut.Long(labResult.LabPanelNum), SOut.DateT(labResult.DateTimeTest, false), labResult.TestName, SOut.DateT(labResult.DateTStamp, false), labResult.TestID, labResult.ObsValue, labResult.ObsUnits, labResult.ObsRange, SOut.Int((int) labResult.AbnormalFlag));
        return table;
    }

    public static long Insert(LabResult labResult)
    {
        return Insert(labResult, false);
    }

    public static long Insert(LabResult labResult, bool useExistingPK)
    {
        var command = "INSERT INTO labresult (";

        command += "LabPanelNum,DateTimeTest,TestName,TestID,ObsValue,ObsUnits,ObsRange,AbnormalFlag) VALUES(";

        command +=
            SOut.Long(labResult.LabPanelNum) + ","
                                             + SOut.DateT(labResult.DateTimeTest) + ","
                                             + "'" + SOut.String(labResult.TestName) + "',"
                                             //DateTStamp can only be set by MySQL
                                             + "'" + SOut.String(labResult.TestID) + "',"
                                             + "'" + SOut.String(labResult.ObsValue) + "',"
                                             + "'" + SOut.String(labResult.ObsUnits) + "',"
                                             + "'" + SOut.String(labResult.ObsRange) + "',"
                                             + SOut.Int((int) labResult.AbnormalFlag) + ")";
        {
            labResult.LabResultNum = Db.NonQ(command, true, "LabResultNum", "labResult");
        }
        return labResult.LabResultNum;
    }

    public static long InsertNoCache(LabResult labResult)
    {
        return InsertNoCache(labResult, false);
    }

    public static long InsertNoCache(LabResult labResult, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO labresult (";
        if (isRandomKeys || useExistingPK) command += "LabResultNum,";
        command += "LabPanelNum,DateTimeTest,TestName,TestID,ObsValue,ObsUnits,ObsRange,AbnormalFlag) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(labResult.LabResultNum) + ",";
        command +=
            SOut.Long(labResult.LabPanelNum) + ","
                                             + SOut.DateT(labResult.DateTimeTest) + ","
                                             + "'" + SOut.String(labResult.TestName) + "',"
                                             //DateTStamp can only be set by MySQL
                                             + "'" + SOut.String(labResult.TestID) + "',"
                                             + "'" + SOut.String(labResult.ObsValue) + "',"
                                             + "'" + SOut.String(labResult.ObsUnits) + "',"
                                             + "'" + SOut.String(labResult.ObsRange) + "',"
                                             + SOut.Int((int) labResult.AbnormalFlag) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            labResult.LabResultNum = Db.NonQ(command, true, "LabResultNum", "labResult");
        return labResult.LabResultNum;
    }

    public static void Update(LabResult labResult)
    {
        var command = "UPDATE labresult SET "
                      + "LabPanelNum =  " + SOut.Long(labResult.LabPanelNum) + ", "
                      + "DateTimeTest=  " + SOut.DateT(labResult.DateTimeTest) + ", "
                      + "TestName    = '" + SOut.String(labResult.TestName) + "', "
                      //DateTStamp can only be set by MySQL
                      + "TestID      = '" + SOut.String(labResult.TestID) + "', "
                      + "ObsValue    = '" + SOut.String(labResult.ObsValue) + "', "
                      + "ObsUnits    = '" + SOut.String(labResult.ObsUnits) + "', "
                      + "ObsRange    = '" + SOut.String(labResult.ObsRange) + "', "
                      + "AbnormalFlag=  " + SOut.Int((int) labResult.AbnormalFlag) + " "
                      + "WHERE LabResultNum = " + SOut.Long(labResult.LabResultNum);
        Db.NonQ(command);
    }

    public static bool Update(LabResult labResult, LabResult oldLabResult)
    {
        var command = "";
        if (labResult.LabPanelNum != oldLabResult.LabPanelNum)
        {
            if (command != "") command += ",";
            command += "LabPanelNum = " + SOut.Long(labResult.LabPanelNum) + "";
        }

        if (labResult.DateTimeTest != oldLabResult.DateTimeTest)
        {
            if (command != "") command += ",";
            command += "DateTimeTest = " + SOut.DateT(labResult.DateTimeTest) + "";
        }

        if (labResult.TestName != oldLabResult.TestName)
        {
            if (command != "") command += ",";
            command += "TestName = '" + SOut.String(labResult.TestName) + "'";
        }

        //DateTStamp can only be set by MySQL
        if (labResult.TestID != oldLabResult.TestID)
        {
            if (command != "") command += ",";
            command += "TestID = '" + SOut.String(labResult.TestID) + "'";
        }

        if (labResult.ObsValue != oldLabResult.ObsValue)
        {
            if (command != "") command += ",";
            command += "ObsValue = '" + SOut.String(labResult.ObsValue) + "'";
        }

        if (labResult.ObsUnits != oldLabResult.ObsUnits)
        {
            if (command != "") command += ",";
            command += "ObsUnits = '" + SOut.String(labResult.ObsUnits) + "'";
        }

        if (labResult.ObsRange != oldLabResult.ObsRange)
        {
            if (command != "") command += ",";
            command += "ObsRange = '" + SOut.String(labResult.ObsRange) + "'";
        }

        if (labResult.AbnormalFlag != oldLabResult.AbnormalFlag)
        {
            if (command != "") command += ",";
            command += "AbnormalFlag = " + SOut.Int((int) labResult.AbnormalFlag) + "";
        }

        if (command == "") return false;
        command = "UPDATE labresult SET " + command
                                          + " WHERE LabResultNum = " + SOut.Long(labResult.LabResultNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(LabResult labResult, LabResult oldLabResult)
    {
        if (labResult.LabPanelNum != oldLabResult.LabPanelNum) return true;
        if (labResult.DateTimeTest != oldLabResult.DateTimeTest) return true;
        if (labResult.TestName != oldLabResult.TestName) return true;
        //DateTStamp can only be set by MySQL
        if (labResult.TestID != oldLabResult.TestID) return true;
        if (labResult.ObsValue != oldLabResult.ObsValue) return true;
        if (labResult.ObsUnits != oldLabResult.ObsUnits) return true;
        if (labResult.ObsRange != oldLabResult.ObsRange) return true;
        if (labResult.AbnormalFlag != oldLabResult.AbnormalFlag) return true;
        return false;
    }

    public static void Delete(long labResultNum)
    {
        var command = "DELETE FROM labresult "
                      + "WHERE LabResultNum = " + SOut.Long(labResultNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listLabResultNums)
    {
        if (listLabResultNums == null || listLabResultNums.Count == 0) return;
        var command = "DELETE FROM labresult "
                      + "WHERE LabResultNum IN(" + string.Join(",", listLabResultNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}