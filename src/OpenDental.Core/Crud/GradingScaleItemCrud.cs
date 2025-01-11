#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class GradingScaleItemCrud
{
    public static GradingScaleItem SelectOne(long gradingScaleItemNum)
    {
        var command = "SELECT * FROM gradingscaleitem "
                      + "WHERE GradingScaleItemNum = " + SOut.Long(gradingScaleItemNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static GradingScaleItem SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<GradingScaleItem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<GradingScaleItem> TableToList(DataTable table)
    {
        var retVal = new List<GradingScaleItem>();
        GradingScaleItem gradingScaleItem;
        foreach (DataRow row in table.Rows)
        {
            gradingScaleItem = new GradingScaleItem();
            gradingScaleItem.GradingScaleItemNum = SIn.Long(row["GradingScaleItemNum"].ToString());
            gradingScaleItem.GradingScaleNum = SIn.Long(row["GradingScaleNum"].ToString());
            gradingScaleItem.GradeShowing = SIn.String(row["GradeShowing"].ToString());
            gradingScaleItem.GradeNumber = SIn.Float(row["GradeNumber"].ToString());
            gradingScaleItem.Description = SIn.String(row["Description"].ToString());
            retVal.Add(gradingScaleItem);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<GradingScaleItem> listGradingScaleItems, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "GradingScaleItem";
        var table = new DataTable(tableName);
        table.Columns.Add("GradingScaleItemNum");
        table.Columns.Add("GradingScaleNum");
        table.Columns.Add("GradeShowing");
        table.Columns.Add("GradeNumber");
        table.Columns.Add("Description");
        foreach (var gradingScaleItem in listGradingScaleItems)
            table.Rows.Add(SOut.Long(gradingScaleItem.GradingScaleItemNum), SOut.Long(gradingScaleItem.GradingScaleNum), gradingScaleItem.GradeShowing, SOut.Float(gradingScaleItem.GradeNumber), gradingScaleItem.Description);
        return table;
    }

    public static long Insert(GradingScaleItem gradingScaleItem)
    {
        return Insert(gradingScaleItem, false);
    }

    public static long Insert(GradingScaleItem gradingScaleItem, bool useExistingPK)
    {
        var command = "INSERT INTO gradingscaleitem (";

        command += "GradingScaleNum,GradeShowing,GradeNumber,Description) VALUES(";

        command +=
            SOut.Long(gradingScaleItem.GradingScaleNum) + ","
                                                        + "'" + SOut.String(gradingScaleItem.GradeShowing) + "',"
                                                        + SOut.Float(gradingScaleItem.GradeNumber) + ","
                                                        + "'" + SOut.String(gradingScaleItem.Description) + "')";
        {
            gradingScaleItem.GradingScaleItemNum = Db.NonQ(command, true, "GradingScaleItemNum", "gradingScaleItem");
        }
        return gradingScaleItem.GradingScaleItemNum;
    }

    public static long InsertNoCache(GradingScaleItem gradingScaleItem)
    {
        return InsertNoCache(gradingScaleItem, false);
    }

    public static long InsertNoCache(GradingScaleItem gradingScaleItem, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO gradingscaleitem (";
        if (isRandomKeys || useExistingPK) command += "GradingScaleItemNum,";
        command += "GradingScaleNum,GradeShowing,GradeNumber,Description) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(gradingScaleItem.GradingScaleItemNum) + ",";
        command +=
            SOut.Long(gradingScaleItem.GradingScaleNum) + ","
                                                        + "'" + SOut.String(gradingScaleItem.GradeShowing) + "',"
                                                        + SOut.Float(gradingScaleItem.GradeNumber) + ","
                                                        + "'" + SOut.String(gradingScaleItem.Description) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            gradingScaleItem.GradingScaleItemNum = Db.NonQ(command, true, "GradingScaleItemNum", "gradingScaleItem");
        return gradingScaleItem.GradingScaleItemNum;
    }

    public static void Update(GradingScaleItem gradingScaleItem)
    {
        var command = "UPDATE gradingscaleitem SET "
                      + "GradingScaleNum    =  " + SOut.Long(gradingScaleItem.GradingScaleNum) + ", "
                      + "GradeShowing       = '" + SOut.String(gradingScaleItem.GradeShowing) + "', "
                      + "GradeNumber        =  " + SOut.Float(gradingScaleItem.GradeNumber) + ", "
                      + "Description        = '" + SOut.String(gradingScaleItem.Description) + "' "
                      + "WHERE GradingScaleItemNum = " + SOut.Long(gradingScaleItem.GradingScaleItemNum);
        Db.NonQ(command);
    }

    public static bool Update(GradingScaleItem gradingScaleItem, GradingScaleItem oldGradingScaleItem)
    {
        var command = "";
        if (gradingScaleItem.GradingScaleNum != oldGradingScaleItem.GradingScaleNum)
        {
            if (command != "") command += ",";
            command += "GradingScaleNum = " + SOut.Long(gradingScaleItem.GradingScaleNum) + "";
        }

        if (gradingScaleItem.GradeShowing != oldGradingScaleItem.GradeShowing)
        {
            if (command != "") command += ",";
            command += "GradeShowing = '" + SOut.String(gradingScaleItem.GradeShowing) + "'";
        }

        if (gradingScaleItem.GradeNumber != oldGradingScaleItem.GradeNumber)
        {
            if (command != "") command += ",";
            command += "GradeNumber = " + SOut.Float(gradingScaleItem.GradeNumber) + "";
        }

        if (gradingScaleItem.Description != oldGradingScaleItem.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(gradingScaleItem.Description) + "'";
        }

        if (command == "") return false;
        command = "UPDATE gradingscaleitem SET " + command
                                                 + " WHERE GradingScaleItemNum = " + SOut.Long(gradingScaleItem.GradingScaleItemNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(GradingScaleItem gradingScaleItem, GradingScaleItem oldGradingScaleItem)
    {
        if (gradingScaleItem.GradingScaleNum != oldGradingScaleItem.GradingScaleNum) return true;
        if (gradingScaleItem.GradeShowing != oldGradingScaleItem.GradeShowing) return true;
        if (gradingScaleItem.GradeNumber != oldGradingScaleItem.GradeNumber) return true;
        if (gradingScaleItem.Description != oldGradingScaleItem.Description) return true;
        return false;
    }

    public static void Delete(long gradingScaleItemNum)
    {
        var command = "DELETE FROM gradingscaleitem "
                      + "WHERE GradingScaleItemNum = " + SOut.Long(gradingScaleItemNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listGradingScaleItemNums)
    {
        if (listGradingScaleItemNums == null || listGradingScaleItemNums.Count == 0) return;
        var command = "DELETE FROM gradingscaleitem "
                      + "WHERE GradingScaleItemNum IN(" + string.Join(",", listGradingScaleItemNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}