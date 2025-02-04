using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ToolButItemCrud
{
    public static List<ToolButItem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ToolButItem> TableToList(DataTable table)
    {
        var retVal = new List<ToolButItem>();
        foreach (DataRow row in table.Rows)
        {
            var toolButItem = new ToolButItem
            {
                ToolButItemNum = SIn.Long(row["ToolButItemNum"].ToString()),
                ProgramNum = SIn.Long(row["ProgramNum"].ToString()),
                ToolBar = (EnumToolBar) SIn.Int(row["ToolBar"].ToString()),
                ButtonText = SIn.String(row["ButtonText"].ToString())
            };
            retVal.Add(toolButItem);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ToolButItem> listToolButItems, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ToolButItem";
        var table = new DataTable(tableName);
        table.Columns.Add("ToolButItemNum");
        table.Columns.Add("ProgramNum");
        table.Columns.Add("ToolBar");
        table.Columns.Add("ButtonText");
        foreach (var toolButItem in listToolButItems)
            table.Rows.Add(SOut.Long(toolButItem.ToolButItemNum), SOut.Long(toolButItem.ProgramNum), SOut.Int((int) toolButItem.ToolBar), toolButItem.ButtonText);
        return table;
    }

    public static void Insert(ToolButItem toolButItem)
    {
        var command = "INSERT INTO toolbutitem (";

        command += "ProgramNum,ToolBar,ButtonText) VALUES(";

        command +=
            SOut.Long(toolButItem.ProgramNum) + ","
                                              + SOut.Int((int) toolButItem.ToolBar) + ","
                                              + "'" + SOut.String(toolButItem.ButtonText) + "')";
        {
            toolButItem.ToolButItemNum = Db.NonQ(command, true, "ToolButItemNum", "toolButItem");
        }
    }
}