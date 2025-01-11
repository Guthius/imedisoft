#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class OrthoChartRowCrud
{
    public static OrthoChartRow SelectOne(long orthoChartRowNum)
    {
        var command = "SELECT * FROM orthochartrow "
                      + "WHERE OrthoChartRowNum = " + SOut.Long(orthoChartRowNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static OrthoChartRow SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<OrthoChartRow> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OrthoChartRow> TableToList(DataTable table)
    {
        var retVal = new List<OrthoChartRow>();
        OrthoChartRow orthoChartRow;
        foreach (DataRow row in table.Rows)
        {
            orthoChartRow = new OrthoChartRow();
            orthoChartRow.OrthoChartRowNum = SIn.Long(row["OrthoChartRowNum"].ToString());
            orthoChartRow.PatNum = SIn.Long(row["PatNum"].ToString());
            orthoChartRow.DateTimeService = SIn.DateTime(row["DateTimeService"].ToString());
            orthoChartRow.UserNum = SIn.Long(row["UserNum"].ToString());
            orthoChartRow.ProvNum = SIn.Long(row["ProvNum"].ToString());
            orthoChartRow.Signature = SIn.String(row["Signature"].ToString());
            retVal.Add(orthoChartRow);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<OrthoChartRow> listOrthoChartRows, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "OrthoChartRow";
        var table = new DataTable(tableName);
        table.Columns.Add("OrthoChartRowNum");
        table.Columns.Add("PatNum");
        table.Columns.Add("DateTimeService");
        table.Columns.Add("UserNum");
        table.Columns.Add("ProvNum");
        table.Columns.Add("Signature");
        foreach (var orthoChartRow in listOrthoChartRows)
            table.Rows.Add(SOut.Long(orthoChartRow.OrthoChartRowNum), SOut.Long(orthoChartRow.PatNum), SOut.DateT(orthoChartRow.DateTimeService, false), SOut.Long(orthoChartRow.UserNum), SOut.Long(orthoChartRow.ProvNum), orthoChartRow.Signature);
        return table;
    }

    public static long Insert(OrthoChartRow orthoChartRow)
    {
        return Insert(orthoChartRow, false);
    }

    public static long Insert(OrthoChartRow orthoChartRow, bool useExistingPK)
    {
        var command = "INSERT INTO orthochartrow (";

        command += "PatNum,DateTimeService,UserNum,ProvNum,Signature) VALUES(";

        command +=
            SOut.Long(orthoChartRow.PatNum) + ","
                                            + SOut.DateT(orthoChartRow.DateTimeService) + ","
                                            + SOut.Long(orthoChartRow.UserNum) + ","
                                            + SOut.Long(orthoChartRow.ProvNum) + ","
                                            + DbHelper.ParamChar + "paramSignature)";
        if (orthoChartRow.Signature == null) orthoChartRow.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(orthoChartRow.Signature));
        {
            orthoChartRow.OrthoChartRowNum = Db.NonQ(command, true, "OrthoChartRowNum", "orthoChartRow", paramSignature);
        }
        return orthoChartRow.OrthoChartRowNum;
    }

    public static long InsertNoCache(OrthoChartRow orthoChartRow)
    {
        return InsertNoCache(orthoChartRow, false);
    }

    public static long InsertNoCache(OrthoChartRow orthoChartRow, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO orthochartrow (";
        if (isRandomKeys || useExistingPK) command += "OrthoChartRowNum,";
        command += "PatNum,DateTimeService,UserNum,ProvNum,Signature) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(orthoChartRow.OrthoChartRowNum) + ",";
        command +=
            SOut.Long(orthoChartRow.PatNum) + ","
                                            + SOut.DateT(orthoChartRow.DateTimeService) + ","
                                            + SOut.Long(orthoChartRow.UserNum) + ","
                                            + SOut.Long(orthoChartRow.ProvNum) + ","
                                            + DbHelper.ParamChar + "paramSignature)";
        if (orthoChartRow.Signature == null) orthoChartRow.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(orthoChartRow.Signature));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramSignature);
        else
            orthoChartRow.OrthoChartRowNum = Db.NonQ(command, true, "OrthoChartRowNum", "orthoChartRow", paramSignature);
        return orthoChartRow.OrthoChartRowNum;
    }

    public static void Update(OrthoChartRow orthoChartRow)
    {
        var command = "UPDATE orthochartrow SET "
                      + "PatNum          =  " + SOut.Long(orthoChartRow.PatNum) + ", "
                      + "DateTimeService =  " + SOut.DateT(orthoChartRow.DateTimeService) + ", "
                      + "UserNum         =  " + SOut.Long(orthoChartRow.UserNum) + ", "
                      + "ProvNum         =  " + SOut.Long(orthoChartRow.ProvNum) + ", "
                      + "Signature       =  " + DbHelper.ParamChar + "paramSignature "
                      + "WHERE OrthoChartRowNum = " + SOut.Long(orthoChartRow.OrthoChartRowNum);
        if (orthoChartRow.Signature == null) orthoChartRow.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(orthoChartRow.Signature));
        Db.NonQ(command, paramSignature);
    }

    public static bool Update(OrthoChartRow orthoChartRow, OrthoChartRow oldOrthoChartRow)
    {
        var command = "";
        if (orthoChartRow.PatNum != oldOrthoChartRow.PatNum)
        {
            if (command != "") command += ",";
            command += "PatNum = " + SOut.Long(orthoChartRow.PatNum) + "";
        }

        if (orthoChartRow.DateTimeService != oldOrthoChartRow.DateTimeService)
        {
            if (command != "") command += ",";
            command += "DateTimeService = " + SOut.DateT(orthoChartRow.DateTimeService) + "";
        }

        if (orthoChartRow.UserNum != oldOrthoChartRow.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(orthoChartRow.UserNum) + "";
        }

        if (orthoChartRow.ProvNum != oldOrthoChartRow.ProvNum)
        {
            if (command != "") command += ",";
            command += "ProvNum = " + SOut.Long(orthoChartRow.ProvNum) + "";
        }

        if (orthoChartRow.Signature != oldOrthoChartRow.Signature)
        {
            if (command != "") command += ",";
            command += "Signature = " + DbHelper.ParamChar + "paramSignature";
        }

        if (command == "") return false;
        if (orthoChartRow.Signature == null) orthoChartRow.Signature = "";
        var paramSignature = new OdSqlParameter("paramSignature", OdDbType.Text, SOut.StringParam(orthoChartRow.Signature));
        command = "UPDATE orthochartrow SET " + command
                                              + " WHERE OrthoChartRowNum = " + SOut.Long(orthoChartRow.OrthoChartRowNum);
        Db.NonQ(command, paramSignature);
        return true;
    }

    public static bool UpdateComparison(OrthoChartRow orthoChartRow, OrthoChartRow oldOrthoChartRow)
    {
        if (orthoChartRow.PatNum != oldOrthoChartRow.PatNum) return true;
        if (orthoChartRow.DateTimeService != oldOrthoChartRow.DateTimeService) return true;
        if (orthoChartRow.UserNum != oldOrthoChartRow.UserNum) return true;
        if (orthoChartRow.ProvNum != oldOrthoChartRow.ProvNum) return true;
        if (orthoChartRow.Signature != oldOrthoChartRow.Signature) return true;
        return false;
    }

    public static void Delete(long orthoChartRowNum)
    {
        var command = "DELETE FROM orthochartrow "
                      + "WHERE OrthoChartRowNum = " + SOut.Long(orthoChartRowNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listOrthoChartRowNums)
    {
        if (listOrthoChartRowNums == null || listOrthoChartRowNums.Count == 0) return;
        var command = "DELETE FROM orthochartrow "
                      + "WHERE OrthoChartRowNum IN(" + string.Join(",", listOrthoChartRowNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}