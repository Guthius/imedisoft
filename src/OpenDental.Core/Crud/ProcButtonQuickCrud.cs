#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ProcButtonQuickCrud
{
    public static ProcButtonQuick SelectOne(long procButtonQuickNum)
    {
        var command = "SELECT * FROM procbuttonquick "
                      + "WHERE ProcButtonQuickNum = " + SOut.Long(procButtonQuickNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ProcButtonQuick SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ProcButtonQuick> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcButtonQuick> TableToList(DataTable table)
    {
        var retVal = new List<ProcButtonQuick>();
        ProcButtonQuick procButtonQuick;
        foreach (DataRow row in table.Rows)
        {
            procButtonQuick = new ProcButtonQuick();
            procButtonQuick.ProcButtonQuickNum = SIn.Long(row["ProcButtonQuickNum"].ToString());
            procButtonQuick.Description = SIn.String(row["Description"].ToString());
            procButtonQuick.CodeValue = SIn.String(row["CodeValue"].ToString());
            procButtonQuick.Surf = SIn.String(row["Surf"].ToString());
            procButtonQuick.YPos = SIn.Int(row["YPos"].ToString());
            procButtonQuick.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            procButtonQuick.IsLabel = SIn.Bool(row["IsLabel"].ToString());
            retVal.Add(procButtonQuick);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ProcButtonQuick> listProcButtonQuicks, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ProcButtonQuick";
        var table = new DataTable(tableName);
        table.Columns.Add("ProcButtonQuickNum");
        table.Columns.Add("Description");
        table.Columns.Add("CodeValue");
        table.Columns.Add("Surf");
        table.Columns.Add("YPos");
        table.Columns.Add("ItemOrder");
        table.Columns.Add("IsLabel");
        foreach (var procButtonQuick in listProcButtonQuicks)
            table.Rows.Add(SOut.Long(procButtonQuick.ProcButtonQuickNum), procButtonQuick.Description, procButtonQuick.CodeValue, procButtonQuick.Surf, SOut.Int(procButtonQuick.YPos), SOut.Int(procButtonQuick.ItemOrder), SOut.Bool(procButtonQuick.IsLabel));
        return table;
    }

    public static long Insert(ProcButtonQuick procButtonQuick)
    {
        return Insert(procButtonQuick, false);
    }

    public static long Insert(ProcButtonQuick procButtonQuick, bool useExistingPK)
    {
        var command = "INSERT INTO procbuttonquick (";

        command += "Description,CodeValue,Surf,YPos,ItemOrder,IsLabel) VALUES(";

        command +=
            "'" + SOut.String(procButtonQuick.Description) + "',"
            + "'" + SOut.String(procButtonQuick.CodeValue) + "',"
            + "'" + SOut.String(procButtonQuick.Surf) + "',"
            + SOut.Int(procButtonQuick.YPos) + ","
            + SOut.Int(procButtonQuick.ItemOrder) + ","
            + SOut.Bool(procButtonQuick.IsLabel) + ")";
        {
            procButtonQuick.ProcButtonQuickNum = Db.NonQ(command, true, "ProcButtonQuickNum", "procButtonQuick");
        }
        return procButtonQuick.ProcButtonQuickNum;
    }

    public static long InsertNoCache(ProcButtonQuick procButtonQuick)
    {
        return InsertNoCache(procButtonQuick, false);
    }

    public static long InsertNoCache(ProcButtonQuick procButtonQuick, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO procbuttonquick (";
        if (isRandomKeys || useExistingPK) command += "ProcButtonQuickNum,";
        command += "Description,CodeValue,Surf,YPos,ItemOrder,IsLabel) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(procButtonQuick.ProcButtonQuickNum) + ",";
        command +=
            "'" + SOut.String(procButtonQuick.Description) + "',"
            + "'" + SOut.String(procButtonQuick.CodeValue) + "',"
            + "'" + SOut.String(procButtonQuick.Surf) + "',"
            + SOut.Int(procButtonQuick.YPos) + ","
            + SOut.Int(procButtonQuick.ItemOrder) + ","
            + SOut.Bool(procButtonQuick.IsLabel) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            procButtonQuick.ProcButtonQuickNum = Db.NonQ(command, true, "ProcButtonQuickNum", "procButtonQuick");
        return procButtonQuick.ProcButtonQuickNum;
    }

    public static void Update(ProcButtonQuick procButtonQuick)
    {
        var command = "UPDATE procbuttonquick SET "
                      + "Description       = '" + SOut.String(procButtonQuick.Description) + "', "
                      + "CodeValue         = '" + SOut.String(procButtonQuick.CodeValue) + "', "
                      + "Surf              = '" + SOut.String(procButtonQuick.Surf) + "', "
                      + "YPos              =  " + SOut.Int(procButtonQuick.YPos) + ", "
                      + "ItemOrder         =  " + SOut.Int(procButtonQuick.ItemOrder) + ", "
                      + "IsLabel           =  " + SOut.Bool(procButtonQuick.IsLabel) + " "
                      + "WHERE ProcButtonQuickNum = " + SOut.Long(procButtonQuick.ProcButtonQuickNum);
        Db.NonQ(command);
    }

    public static bool Update(ProcButtonQuick procButtonQuick, ProcButtonQuick oldProcButtonQuick)
    {
        var command = "";
        if (procButtonQuick.Description != oldProcButtonQuick.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(procButtonQuick.Description) + "'";
        }

        if (procButtonQuick.CodeValue != oldProcButtonQuick.CodeValue)
        {
            if (command != "") command += ",";
            command += "CodeValue = '" + SOut.String(procButtonQuick.CodeValue) + "'";
        }

        if (procButtonQuick.Surf != oldProcButtonQuick.Surf)
        {
            if (command != "") command += ",";
            command += "Surf = '" + SOut.String(procButtonQuick.Surf) + "'";
        }

        if (procButtonQuick.YPos != oldProcButtonQuick.YPos)
        {
            if (command != "") command += ",";
            command += "YPos = " + SOut.Int(procButtonQuick.YPos) + "";
        }

        if (procButtonQuick.ItemOrder != oldProcButtonQuick.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(procButtonQuick.ItemOrder) + "";
        }

        if (procButtonQuick.IsLabel != oldProcButtonQuick.IsLabel)
        {
            if (command != "") command += ",";
            command += "IsLabel = " + SOut.Bool(procButtonQuick.IsLabel) + "";
        }

        if (command == "") return false;
        command = "UPDATE procbuttonquick SET " + command
                                                + " WHERE ProcButtonQuickNum = " + SOut.Long(procButtonQuick.ProcButtonQuickNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(ProcButtonQuick procButtonQuick, ProcButtonQuick oldProcButtonQuick)
    {
        if (procButtonQuick.Description != oldProcButtonQuick.Description) return true;
        if (procButtonQuick.CodeValue != oldProcButtonQuick.CodeValue) return true;
        if (procButtonQuick.Surf != oldProcButtonQuick.Surf) return true;
        if (procButtonQuick.YPos != oldProcButtonQuick.YPos) return true;
        if (procButtonQuick.ItemOrder != oldProcButtonQuick.ItemOrder) return true;
        if (procButtonQuick.IsLabel != oldProcButtonQuick.IsLabel) return true;
        return false;
    }

    public static void Delete(long procButtonQuickNum)
    {
        var command = "DELETE FROM procbuttonquick "
                      + "WHERE ProcButtonQuickNum = " + SOut.Long(procButtonQuickNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listProcButtonQuickNums)
    {
        if (listProcButtonQuickNums == null || listProcButtonQuickNums.Count == 0) return;
        var command = "DELETE FROM procbuttonquick "
                      + "WHERE ProcButtonQuickNum IN(" + string.Join(",", listProcButtonQuickNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}