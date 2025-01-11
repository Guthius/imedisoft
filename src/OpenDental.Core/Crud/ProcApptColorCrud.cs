#region

using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ProcApptColorCrud
{
    public static ProcApptColor SelectOne(long procApptColorNum)
    {
        var command = "SELECT * FROM procapptcolor "
                      + "WHERE ProcApptColorNum = " + SOut.Long(procApptColorNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ProcApptColor SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ProcApptColor> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcApptColor> TableToList(DataTable table)
    {
        var retVal = new List<ProcApptColor>();
        ProcApptColor procApptColor;
        foreach (DataRow row in table.Rows)
        {
            procApptColor = new ProcApptColor();
            procApptColor.ProcApptColorNum = SIn.Long(row["ProcApptColorNum"].ToString());
            procApptColor.CodeRange = SIn.String(row["CodeRange"].ToString());
            procApptColor.ShowPreviousDate = SIn.Bool(row["ShowPreviousDate"].ToString());
            procApptColor.ColorText = Color.FromArgb(SIn.Int(row["ColorText"].ToString()));
            retVal.Add(procApptColor);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ProcApptColor> listProcApptColors, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ProcApptColor";
        var table = new DataTable(tableName);
        table.Columns.Add("ProcApptColorNum");
        table.Columns.Add("CodeRange");
        table.Columns.Add("ShowPreviousDate");
        table.Columns.Add("ColorText");
        foreach (var procApptColor in listProcApptColors)
            table.Rows.Add(SOut.Long(procApptColor.ProcApptColorNum), procApptColor.CodeRange, SOut.Bool(procApptColor.ShowPreviousDate), SOut.Int(procApptColor.ColorText.ToArgb()));
        return table;
    }

    public static long Insert(ProcApptColor procApptColor)
    {
        return Insert(procApptColor, false);
    }

    public static long Insert(ProcApptColor procApptColor, bool useExistingPK)
    {
        var command = "INSERT INTO procapptcolor (";

        command += "CodeRange,ShowPreviousDate,ColorText) VALUES(";

        command +=
            "'" + SOut.String(procApptColor.CodeRange) + "',"
            + SOut.Bool(procApptColor.ShowPreviousDate) + ","
            + SOut.Int(procApptColor.ColorText.ToArgb()) + ")";
        {
            procApptColor.ProcApptColorNum = Db.NonQ(command, true, "ProcApptColorNum", "procApptColor");
        }
        return procApptColor.ProcApptColorNum;
    }

    public static long InsertNoCache(ProcApptColor procApptColor)
    {
        return InsertNoCache(procApptColor, false);
    }

    public static long InsertNoCache(ProcApptColor procApptColor, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO procapptcolor (";
        if (isRandomKeys || useExistingPK) command += "ProcApptColorNum,";
        command += "CodeRange,ShowPreviousDate,ColorText) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(procApptColor.ProcApptColorNum) + ",";
        command +=
            "'" + SOut.String(procApptColor.CodeRange) + "',"
            + SOut.Bool(procApptColor.ShowPreviousDate) + ","
            + SOut.Int(procApptColor.ColorText.ToArgb()) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            procApptColor.ProcApptColorNum = Db.NonQ(command, true, "ProcApptColorNum", "procApptColor");
        return procApptColor.ProcApptColorNum;
    }

    public static void Update(ProcApptColor procApptColor)
    {
        var command = "UPDATE procapptcolor SET "
                      + "CodeRange       = '" + SOut.String(procApptColor.CodeRange) + "', "
                      + "ShowPreviousDate=  " + SOut.Bool(procApptColor.ShowPreviousDate) + ", "
                      + "ColorText       =  " + SOut.Int(procApptColor.ColorText.ToArgb()) + " "
                      + "WHERE ProcApptColorNum = " + SOut.Long(procApptColor.ProcApptColorNum);
        Db.NonQ(command);
    }

    public static bool Update(ProcApptColor procApptColor, ProcApptColor oldProcApptColor)
    {
        var command = "";
        if (procApptColor.CodeRange != oldProcApptColor.CodeRange)
        {
            if (command != "") command += ",";
            command += "CodeRange = '" + SOut.String(procApptColor.CodeRange) + "'";
        }

        if (procApptColor.ShowPreviousDate != oldProcApptColor.ShowPreviousDate)
        {
            if (command != "") command += ",";
            command += "ShowPreviousDate = " + SOut.Bool(procApptColor.ShowPreviousDate) + "";
        }

        if (procApptColor.ColorText != oldProcApptColor.ColorText)
        {
            if (command != "") command += ",";
            command += "ColorText = " + SOut.Int(procApptColor.ColorText.ToArgb()) + "";
        }

        if (command == "") return false;
        command = "UPDATE procapptcolor SET " + command
                                              + " WHERE ProcApptColorNum = " + SOut.Long(procApptColor.ProcApptColorNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ProcApptColor procApptColor, ProcApptColor oldProcApptColor)
    {
        if (procApptColor.CodeRange != oldProcApptColor.CodeRange) return true;
        if (procApptColor.ShowPreviousDate != oldProcApptColor.ShowPreviousDate) return true;
        if (procApptColor.ColorText != oldProcApptColor.ColorText) return true;
        return false;
    }

    public static void Delete(long procApptColorNum)
    {
        var command = "DELETE FROM procapptcolor "
                      + "WHERE ProcApptColorNum = " + SOut.Long(procApptColorNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listProcApptColorNums)
    {
        if (listProcApptColorNums == null || listProcApptColorNums.Count == 0) return;
        var command = "DELETE FROM procapptcolor "
                      + "WHERE ProcApptColorNum IN(" + string.Join(",", listProcApptColorNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}