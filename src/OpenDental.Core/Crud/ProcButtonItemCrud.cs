#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ProcButtonItemCrud
{
    public static ProcButtonItem SelectOne(long procButtonItemNum)
    {
        var command = "SELECT * FROM procbuttonitem "
                      + "WHERE ProcButtonItemNum = " + SOut.Long(procButtonItemNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ProcButtonItem SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ProcButtonItem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcButtonItem> TableToList(DataTable table)
    {
        var retVal = new List<ProcButtonItem>();
        ProcButtonItem procButtonItem;
        foreach (DataRow row in table.Rows)
        {
            procButtonItem = new ProcButtonItem();
            procButtonItem.ProcButtonItemNum = SIn.Long(row["ProcButtonItemNum"].ToString());
            procButtonItem.ProcButtonNum = SIn.Long(row["ProcButtonNum"].ToString());
            procButtonItem.OldCode = SIn.String(row["OldCode"].ToString());
            procButtonItem.AutoCodeNum = SIn.Long(row["AutoCodeNum"].ToString());
            procButtonItem.CodeNum = SIn.Long(row["CodeNum"].ToString());
            procButtonItem.ItemOrder = SIn.Long(row["ItemOrder"].ToString());
            retVal.Add(procButtonItem);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ProcButtonItem> listProcButtonItems, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ProcButtonItem";
        var table = new DataTable(tableName);
        table.Columns.Add("ProcButtonItemNum");
        table.Columns.Add("ProcButtonNum");
        table.Columns.Add("OldCode");
        table.Columns.Add("AutoCodeNum");
        table.Columns.Add("CodeNum");
        table.Columns.Add("ItemOrder");
        foreach (var procButtonItem in listProcButtonItems)
            table.Rows.Add(SOut.Long(procButtonItem.ProcButtonItemNum), SOut.Long(procButtonItem.ProcButtonNum), procButtonItem.OldCode, SOut.Long(procButtonItem.AutoCodeNum), SOut.Long(procButtonItem.CodeNum), SOut.Long(procButtonItem.ItemOrder));
        return table;
    }

    public static long Insert(ProcButtonItem procButtonItem)
    {
        return Insert(procButtonItem, false);
    }

    public static long Insert(ProcButtonItem procButtonItem, bool useExistingPK)
    {
        var command = "INSERT INTO procbuttonitem (";

        command += "ProcButtonNum,OldCode,AutoCodeNum,CodeNum,ItemOrder) VALUES(";

        command +=
            SOut.Long(procButtonItem.ProcButtonNum) + ","
                                                    + "'" + SOut.String(procButtonItem.OldCode) + "',"
                                                    + SOut.Long(procButtonItem.AutoCodeNum) + ","
                                                    + SOut.Long(procButtonItem.CodeNum) + ","
                                                    + SOut.Long(procButtonItem.ItemOrder) + ")";
        {
            procButtonItem.ProcButtonItemNum = Db.NonQ(command, true, "ProcButtonItemNum", "procButtonItem");
        }
        return procButtonItem.ProcButtonItemNum;
    }

    public static long InsertNoCache(ProcButtonItem procButtonItem)
    {
        return InsertNoCache(procButtonItem, false);
    }

    public static long InsertNoCache(ProcButtonItem procButtonItem, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO procbuttonitem (";
        if (isRandomKeys || useExistingPK) command += "ProcButtonItemNum,";
        command += "ProcButtonNum,OldCode,AutoCodeNum,CodeNum,ItemOrder) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(procButtonItem.ProcButtonItemNum) + ",";
        command +=
            SOut.Long(procButtonItem.ProcButtonNum) + ","
                                                    + "'" + SOut.String(procButtonItem.OldCode) + "',"
                                                    + SOut.Long(procButtonItem.AutoCodeNum) + ","
                                                    + SOut.Long(procButtonItem.CodeNum) + ","
                                                    + SOut.Long(procButtonItem.ItemOrder) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            procButtonItem.ProcButtonItemNum = Db.NonQ(command, true, "ProcButtonItemNum", "procButtonItem");
        return procButtonItem.ProcButtonItemNum;
    }

    public static void Update(ProcButtonItem procButtonItem)
    {
        var command = "UPDATE procbuttonitem SET "
                      + "ProcButtonNum    =  " + SOut.Long(procButtonItem.ProcButtonNum) + ", "
                      + "OldCode          = '" + SOut.String(procButtonItem.OldCode) + "', "
                      + "AutoCodeNum      =  " + SOut.Long(procButtonItem.AutoCodeNum) + ", "
                      + "CodeNum          =  " + SOut.Long(procButtonItem.CodeNum) + ", "
                      + "ItemOrder        =  " + SOut.Long(procButtonItem.ItemOrder) + " "
                      + "WHERE ProcButtonItemNum = " + SOut.Long(procButtonItem.ProcButtonItemNum);
        Db.NonQ(command);
    }

    public static bool Update(ProcButtonItem procButtonItem, ProcButtonItem oldProcButtonItem)
    {
        var command = "";
        if (procButtonItem.ProcButtonNum != oldProcButtonItem.ProcButtonNum)
        {
            if (command != "") command += ",";
            command += "ProcButtonNum = " + SOut.Long(procButtonItem.ProcButtonNum) + "";
        }

        if (procButtonItem.OldCode != oldProcButtonItem.OldCode)
        {
            if (command != "") command += ",";
            command += "OldCode = '" + SOut.String(procButtonItem.OldCode) + "'";
        }

        if (procButtonItem.AutoCodeNum != oldProcButtonItem.AutoCodeNum)
        {
            if (command != "") command += ",";
            command += "AutoCodeNum = " + SOut.Long(procButtonItem.AutoCodeNum) + "";
        }

        if (procButtonItem.CodeNum != oldProcButtonItem.CodeNum)
        {
            if (command != "") command += ",";
            command += "CodeNum = " + SOut.Long(procButtonItem.CodeNum) + "";
        }

        if (procButtonItem.ItemOrder != oldProcButtonItem.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Long(procButtonItem.ItemOrder) + "";
        }

        if (command == "") return false;
        command = "UPDATE procbuttonitem SET " + command
                                               + " WHERE ProcButtonItemNum = " + SOut.Long(procButtonItem.ProcButtonItemNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ProcButtonItem procButtonItem, ProcButtonItem oldProcButtonItem)
    {
        if (procButtonItem.ProcButtonNum != oldProcButtonItem.ProcButtonNum) return true;
        if (procButtonItem.OldCode != oldProcButtonItem.OldCode) return true;
        if (procButtonItem.AutoCodeNum != oldProcButtonItem.AutoCodeNum) return true;
        if (procButtonItem.CodeNum != oldProcButtonItem.CodeNum) return true;
        if (procButtonItem.ItemOrder != oldProcButtonItem.ItemOrder) return true;
        return false;
    }

    public static void Delete(long procButtonItemNum)
    {
        var command = "DELETE FROM procbuttonitem "
                      + "WHERE ProcButtonItemNum = " + SOut.Long(procButtonItemNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listProcButtonItemNums)
    {
        if (listProcButtonItemNums == null || listProcButtonItemNums.Count == 0) return;
        var command = "DELETE FROM procbuttonitem "
                      + "WHERE ProcButtonItemNum IN(" + string.Join(",", listProcButtonItemNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}