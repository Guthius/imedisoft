#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ToolButItemCrud
{
    public static ToolButItem SelectOne(long toolButItemNum)
    {
        var command = "SELECT * FROM toolbutitem "
                      + "WHERE ToolButItemNum = " + SOut.Long(toolButItemNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ToolButItem SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ToolButItem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ToolButItem> TableToList(DataTable table)
    {
        var retVal = new List<ToolButItem>();
        ToolButItem toolButItem;
        foreach (DataRow row in table.Rows)
        {
            toolButItem = new ToolButItem();
            toolButItem.ToolButItemNum = SIn.Long(row["ToolButItemNum"].ToString());
            toolButItem.ProgramNum = SIn.Long(row["ProgramNum"].ToString());
            toolButItem.ToolBar = (EnumToolBar) SIn.Int(row["ToolBar"].ToString());
            toolButItem.ButtonText = SIn.String(row["ButtonText"].ToString());
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

    public static long Insert(ToolButItem toolButItem)
    {
        return Insert(toolButItem, false);
    }

    public static long Insert(ToolButItem toolButItem, bool useExistingPK)
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
        return toolButItem.ToolButItemNum;
    }

    public static long InsertNoCache(ToolButItem toolButItem)
    {
        return InsertNoCache(toolButItem, false);
    }

    public static long InsertNoCache(ToolButItem toolButItem, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO toolbutitem (";
        if (isRandomKeys || useExistingPK) command += "ToolButItemNum,";
        command += "ProgramNum,ToolBar,ButtonText) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(toolButItem.ToolButItemNum) + ",";
        command +=
            SOut.Long(toolButItem.ProgramNum) + ","
                                              + SOut.Int((int) toolButItem.ToolBar) + ","
                                              + "'" + SOut.String(toolButItem.ButtonText) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            toolButItem.ToolButItemNum = Db.NonQ(command, true, "ToolButItemNum", "toolButItem");
        return toolButItem.ToolButItemNum;
    }

    public static void Update(ToolButItem toolButItem)
    {
        var command = "UPDATE toolbutitem SET "
                      + "ProgramNum    =  " + SOut.Long(toolButItem.ProgramNum) + ", "
                      + "ToolBar       =  " + SOut.Int((int) toolButItem.ToolBar) + ", "
                      + "ButtonText    = '" + SOut.String(toolButItem.ButtonText) + "' "
                      + "WHERE ToolButItemNum = " + SOut.Long(toolButItem.ToolButItemNum);
        Db.NonQ(command);
    }

    public static bool Update(ToolButItem toolButItem, ToolButItem oldToolButItem)
    {
        var command = "";
        if (toolButItem.ProgramNum != oldToolButItem.ProgramNum)
        {
            if (command != "") command += ",";
            command += "ProgramNum = " + SOut.Long(toolButItem.ProgramNum) + "";
        }

        if (toolButItem.ToolBar != oldToolButItem.ToolBar)
        {
            if (command != "") command += ",";
            command += "ToolBar = " + SOut.Int((int) toolButItem.ToolBar) + "";
        }

        if (toolButItem.ButtonText != oldToolButItem.ButtonText)
        {
            if (command != "") command += ",";
            command += "ButtonText = '" + SOut.String(toolButItem.ButtonText) + "'";
        }

        if (command == "") return false;
        command = "UPDATE toolbutitem SET " + command
                                            + " WHERE ToolButItemNum = " + SOut.Long(toolButItem.ToolButItemNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ToolButItem toolButItem, ToolButItem oldToolButItem)
    {
        if (toolButItem.ProgramNum != oldToolButItem.ProgramNum) return true;
        if (toolButItem.ToolBar != oldToolButItem.ToolBar) return true;
        if (toolButItem.ButtonText != oldToolButItem.ButtonText) return true;
        return false;
    }

    public static void Delete(long toolButItemNum)
    {
        var command = "DELETE FROM toolbutitem "
                      + "WHERE ToolButItemNum = " + SOut.Long(toolButItemNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listToolButItemNums)
    {
        if (listToolButItemNums == null || listToolButItemNums.Count == 0) return;
        var command = "DELETE FROM toolbutitem "
                      + "WHERE ToolButItemNum IN(" + string.Join(",", listToolButItemNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}