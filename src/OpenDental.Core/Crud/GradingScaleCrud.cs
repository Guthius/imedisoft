#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class GradingScaleCrud
{
    public static GradingScale SelectOne(long gradingScaleNum)
    {
        var command = "SELECT * FROM gradingscale "
                      + "WHERE GradingScaleNum = " + SOut.Long(gradingScaleNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static GradingScale SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<GradingScale> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<GradingScale> TableToList(DataTable table)
    {
        var retVal = new List<GradingScale>();
        GradingScale gradingScale;
        foreach (DataRow row in table.Rows)
        {
            gradingScale = new GradingScale();
            gradingScale.GradingScaleNum = SIn.Long(row["GradingScaleNum"].ToString());
            gradingScale.ScaleType = (EnumScaleType) SIn.Int(row["ScaleType"].ToString());
            gradingScale.Description = SIn.String(row["Description"].ToString());
            retVal.Add(gradingScale);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<GradingScale> listGradingScales, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "GradingScale";
        var table = new DataTable(tableName);
        table.Columns.Add("GradingScaleNum");
        table.Columns.Add("ScaleType");
        table.Columns.Add("Description");
        foreach (var gradingScale in listGradingScales)
            table.Rows.Add(SOut.Long(gradingScale.GradingScaleNum), SOut.Int((int) gradingScale.ScaleType), gradingScale.Description);
        return table;
    }

    public static long Insert(GradingScale gradingScale)
    {
        return Insert(gradingScale, false);
    }

    public static long Insert(GradingScale gradingScale, bool useExistingPK)
    {
        var command = "INSERT INTO gradingscale (";

        command += "ScaleType,Description) VALUES(";

        command +=
            SOut.Int((int) gradingScale.ScaleType) + ","
                                                   + "'" + SOut.String(gradingScale.Description) + "')";
        {
            gradingScale.GradingScaleNum = Db.NonQ(command, true, "GradingScaleNum", "gradingScale");
        }
        return gradingScale.GradingScaleNum;
    }

    public static long InsertNoCache(GradingScale gradingScale)
    {
        return InsertNoCache(gradingScale, false);
    }

    public static long InsertNoCache(GradingScale gradingScale, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO gradingscale (";
        if (isRandomKeys || useExistingPK) command += "GradingScaleNum,";
        command += "ScaleType,Description) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(gradingScale.GradingScaleNum) + ",";
        command +=
            SOut.Int((int) gradingScale.ScaleType) + ","
                                                   + "'" + SOut.String(gradingScale.Description) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            gradingScale.GradingScaleNum = Db.NonQ(command, true, "GradingScaleNum", "gradingScale");
        return gradingScale.GradingScaleNum;
    }

    public static void Update(GradingScale gradingScale)
    {
        var command = "UPDATE gradingscale SET "
                      + "ScaleType      =  " + SOut.Int((int) gradingScale.ScaleType) + ", "
                      + "Description    = '" + SOut.String(gradingScale.Description) + "' "
                      + "WHERE GradingScaleNum = " + SOut.Long(gradingScale.GradingScaleNum);
        Db.NonQ(command);
    }

    public static bool Update(GradingScale gradingScale, GradingScale oldGradingScale)
    {
        var command = "";
        if (gradingScale.ScaleType != oldGradingScale.ScaleType)
        {
            if (command != "") command += ",";
            command += "ScaleType = " + SOut.Int((int) gradingScale.ScaleType) + "";
        }

        if (gradingScale.Description != oldGradingScale.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(gradingScale.Description) + "'";
        }

        if (command == "") return false;
        command = "UPDATE gradingscale SET " + command
                                             + " WHERE GradingScaleNum = " + SOut.Long(gradingScale.GradingScaleNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(GradingScale gradingScale, GradingScale oldGradingScale)
    {
        if (gradingScale.ScaleType != oldGradingScale.ScaleType) return true;
        if (gradingScale.Description != oldGradingScale.Description) return true;
        return false;
    }

    public static void Delete(long gradingScaleNum)
    {
        var command = "DELETE FROM gradingscale "
                      + "WHERE GradingScaleNum = " + SOut.Long(gradingScaleNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listGradingScaleNums)
    {
        if (listGradingScaleNums == null || listGradingScaleNums.Count == 0) return;
        var command = "DELETE FROM gradingscale "
                      + "WHERE GradingScaleNum IN(" + string.Join(",", listGradingScaleNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}