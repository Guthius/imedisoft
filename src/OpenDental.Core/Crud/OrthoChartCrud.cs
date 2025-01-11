#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class OrthoChartCrud
{
    public static OrthoChart SelectOne(long orthoChartNum)
    {
        var command = "SELECT * FROM orthochart "
                      + "WHERE OrthoChartNum = " + SOut.Long(orthoChartNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static OrthoChart SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<OrthoChart> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OrthoChart> TableToList(DataTable table)
    {
        var retVal = new List<OrthoChart>();
        OrthoChart orthoChart;
        foreach (DataRow row in table.Rows)
        {
            orthoChart = new OrthoChart();
            orthoChart.OrthoChartNum = SIn.Long(row["OrthoChartNum"].ToString());
            orthoChart.PatNum = SIn.Long(row["PatNum"].ToString());
            orthoChart.DateService = SIn.Date(row["DateService"].ToString());
            orthoChart.FieldName = SIn.String(row["FieldName"].ToString());
            orthoChart.FieldValue = SIn.String(row["FieldValue"].ToString());
            orthoChart.UserNum = SIn.Long(row["UserNum"].ToString());
            orthoChart.ProvNum = SIn.Long(row["ProvNum"].ToString());
            orthoChart.OrthoChartRowNum = SIn.Long(row["OrthoChartRowNum"].ToString());
            retVal.Add(orthoChart);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<OrthoChart> listOrthoCharts, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "OrthoChart";
        var table = new DataTable(tableName);
        table.Columns.Add("OrthoChartNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("DateService");
        table.Columns.Add("FieldName");
        table.Columns.Add("FieldValue");
        table.Columns.Add("UserNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("OrthoChartRowNum");
        foreach (var orthoChart in listOrthoCharts)
            table.Rows.Add(SOut.Long(orthoChart.OrthoChartNum), SOut.Long(orthoChart.PatNum), SOut.DateT(orthoChart.DateService, false), orthoChart.FieldName, orthoChart.FieldValue, SOut.Long(orthoChart.UserNum), SOut.Long(orthoChart.ProvNum), SOut.Long(orthoChart.OrthoChartRowNum));
        return table;
    }

    public static long Insert(OrthoChart orthoChart)
    {
        return Insert(orthoChart, false);
    }

    public static long Insert(OrthoChart orthoChart, bool useExistingPK)
    {
        var command = "INSERT INTO orthochart (";

        command += "PatNum,DateService,FieldName,FieldValue,UserNum,ProvNum,OrthoChartRowNum) VALUES(";

        command +=
            SOut.Long(orthoChart.PatNum) + ","
                                         + SOut.Date(orthoChart.DateService) + ","
                                         + "'" + SOut.String(orthoChart.FieldName) + "',"
                                         + DbHelper.ParamChar + "paramFieldValue,"
                                         + SOut.Long(orthoChart.UserNum) + ","
                                         + SOut.Long(orthoChart.ProvNum) + ","
                                         + SOut.Long(orthoChart.OrthoChartRowNum) + ")";
        if (orthoChart.FieldValue == null) orthoChart.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", OdDbType.Text, SOut.StringParam(orthoChart.FieldValue));
        {
            orthoChart.OrthoChartNum = Db.NonQ(command, true, "OrthoChartNum", "orthoChart", paramFieldValue);
        }
        return orthoChart.OrthoChartNum;
    }

    public static long InsertNoCache(OrthoChart orthoChart)
    {
        return InsertNoCache(orthoChart, false);
    }

    public static long InsertNoCache(OrthoChart orthoChart, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO orthochart (";
        if (isRandomKeys || useExistingPK) command += "OrthoChartNum,";
        command += "PatNum,DateService,FieldName,FieldValue,UserNum,ProvNum,OrthoChartRowNum) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(orthoChart.OrthoChartNum) + ",";
        command +=
            SOut.Long(orthoChart.PatNum) + ","
                                         + SOut.Date(orthoChart.DateService) + ","
                                         + "'" + SOut.String(orthoChart.FieldName) + "',"
                                         + DbHelper.ParamChar + "paramFieldValue,"
                                         + SOut.Long(orthoChart.UserNum) + ","
                                         + SOut.Long(orthoChart.ProvNum) + ","
                                         + SOut.Long(orthoChart.OrthoChartRowNum) + ")";
        if (orthoChart.FieldValue == null) orthoChart.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", OdDbType.Text, SOut.StringParam(orthoChart.FieldValue));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramFieldValue);
        else
            orthoChart.OrthoChartNum = Db.NonQ(command, true, "OrthoChartNum", "orthoChart", paramFieldValue);
        return orthoChart.OrthoChartNum;
    }

    public static void Update(OrthoChart orthoChart)
    {
        var command = "UPDATE orthochart SET "
                      + "PatNum          =  " + SOut.Long(orthoChart.PatNum) + ", "
                      + "DateService     =  " + SOut.Date(orthoChart.DateService) + ", "
                      + "FieldName       = '" + SOut.String(orthoChart.FieldName) + "', "
                      + "FieldValue      =  " + DbHelper.ParamChar + "paramFieldValue, "
                      + "UserNum         =  " + SOut.Long(orthoChart.UserNum) + ", "
                      + "ProvNum         =  " + SOut.Long(orthoChart.ProvNum) + ", "
                      + "OrthoChartRowNum=  " + SOut.Long(orthoChart.OrthoChartRowNum) + " "
                      + "WHERE OrthoChartNum = " + SOut.Long(orthoChart.OrthoChartNum);
        if (orthoChart.FieldValue == null) orthoChart.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", OdDbType.Text, SOut.StringParam(orthoChart.FieldValue));
        Db.NonQ(command, paramFieldValue);
    }

    public static bool Update(OrthoChart orthoChart, OrthoChart oldOrthoChart)
    {
        var command = "";
        if (orthoChart.PatNum != oldOrthoChart.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(orthoChart.PatNum) + "";
        }

        if (orthoChart.DateService.Date != oldOrthoChart.DateService.Date)
        {
            if (command != "") command += ",";
            command += "DateService = " + SOut.Date(orthoChart.DateService) + "";
        }

        if (orthoChart.FieldName != oldOrthoChart.FieldName)
        {
            if (command != "") command += ",";
            command += "FieldName = '" + SOut.String(orthoChart.FieldName) + "'";
        }

        if (orthoChart.FieldValue != oldOrthoChart.FieldValue)
        {
            if (command != "") command += ",";
            command += "FieldValue = " + DbHelper.ParamChar + "paramFieldValue";
        }

        if (orthoChart.UserNum != oldOrthoChart.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(orthoChart.UserNum) + "";
        }

        if (orthoChart.ProvNum != oldOrthoChart.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(orthoChart.ProvNum) + "";
        }

        if (orthoChart.OrthoChartRowNum != oldOrthoChart.OrthoChartRowNum)
        {
            if (command != "") command += ",";
            command += "OrthoChartRowNum = " + SOut.Long(orthoChart.OrthoChartRowNum) + "";
        }

        if (command == "") return false;
        if (orthoChart.FieldValue == null) orthoChart.FieldValue = "";
        var paramFieldValue = new OdSqlParameter("paramFieldValue", OdDbType.Text, SOut.StringParam(orthoChart.FieldValue));
        command = "UPDATE orthochart SET " + command
                                           + " WHERE OrthoChartNum = " + SOut.Long(orthoChart.OrthoChartNum);
        Db.NonQ(command, paramFieldValue);
        return true;
    }

    public static bool UpdateComparison(OrthoChart orthoChart, OrthoChart oldOrthoChart)
    {
        if (orthoChart.PatNum != oldOrthoChart.PatNum) return true;
        if (orthoChart.DateService.Date != oldOrthoChart.DateService.Date) return true;
        if (orthoChart.FieldName != oldOrthoChart.FieldName) return true;
        if (orthoChart.FieldValue != oldOrthoChart.FieldValue) return true;
        if (orthoChart.UserNum != oldOrthoChart.UserNum) return true;
        if (orthoChart.ProvNum != oldOrthoChart.ProvNum) return true;
        if (orthoChart.OrthoChartRowNum != oldOrthoChart.OrthoChartRowNum) return true;
        return false;
    }

    public static void Delete(long orthoChartNum)
    {
        var command = "DELETE FROM orthochart "
                      + "WHERE OrthoChartNum = " + SOut.Long(orthoChartNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listOrthoChartNums)
    {
        if (listOrthoChartNums == null || listOrthoChartNums.Count == 0) return;
        var command = "DELETE FROM orthochart "
                      + "WHERE OrthoChartNum IN(" + string.Join(",", listOrthoChartNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}