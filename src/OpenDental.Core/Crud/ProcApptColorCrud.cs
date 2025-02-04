using System.Collections.Generic;
using System.Data;
using System.Drawing;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ProcApptColorCrud
{
    public static List<ProcApptColor> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcApptColor> TableToList(DataTable table)
    {
        var retVal = new List<ProcApptColor>();
        foreach (DataRow row in table.Rows)
        {
            var procApptColor = new ProcApptColor
            {
                ProcApptColorNum = SIn.Long(row["ProcApptColorNum"].ToString()),
                CodeRange = SIn.String(row["CodeRange"].ToString()),
                ShowPreviousDate = SIn.Bool(row["ShowPreviousDate"].ToString()),
                ColorText = Color.FromArgb(SIn.Int(row["ColorText"].ToString()))
            };
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

    public static void Insert(ProcApptColor procApptColor)
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
}