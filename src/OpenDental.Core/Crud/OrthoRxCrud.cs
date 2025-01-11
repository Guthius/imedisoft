#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class OrthoRxCrud
{
    public static OrthoRx SelectOne(long orthoRxNum)
    {
        var command = "SELECT * FROM orthorx "
                      + "WHERE OrthoRxNum = " + SOut.Long(orthoRxNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static OrthoRx SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<OrthoRx> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OrthoRx> TableToList(DataTable table)
    {
        var retVal = new List<OrthoRx>();
        OrthoRx orthoRx;
        foreach (DataRow row in table.Rows)
        {
            orthoRx = new OrthoRx();
            orthoRx.OrthoRxNum = SIn.Long(row["OrthoRxNum"].ToString());
            orthoRx.OrthoHardwareSpecNum = SIn.Long(row["OrthoHardwareSpecNum"].ToString());
            orthoRx.Description = SIn.String(row["Description"].ToString());
            orthoRx.ToothRange = SIn.String(row["ToothRange"].ToString());
            orthoRx.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            retVal.Add(orthoRx);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<OrthoRx> listOrthoRxs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "OrthoRx";
        var table = new DataTable(tableName);
        table.Columns.Add("OrthoRxNum");
        table.Columns.Add("OrthoHardwareSpecNum");
        table.Columns.Add("Description");
        table.Columns.Add("ToothRange");
        table.Columns.Add("ItemOrder");
        foreach (var orthoRx in listOrthoRxs)
            table.Rows.Add(SOut.Long(orthoRx.OrthoRxNum), SOut.Long(orthoRx.OrthoHardwareSpecNum), orthoRx.Description, orthoRx.ToothRange, SOut.Int(orthoRx.ItemOrder));
        return table;
    }

    public static long Insert(OrthoRx orthoRx)
    {
        return Insert(orthoRx, false);
    }

    public static long Insert(OrthoRx orthoRx, bool useExistingPK)
    {
        var command = "INSERT INTO orthorx (";

        command += "OrthoHardwareSpecNum,Description,ToothRange,ItemOrder) VALUES(";

        command +=
            SOut.Long(orthoRx.OrthoHardwareSpecNum) + ","
                                                    + "'" + SOut.String(orthoRx.Description) + "',"
                                                    + "'" + SOut.String(orthoRx.ToothRange) + "',"
                                                    + SOut.Int(orthoRx.ItemOrder) + ")";
        {
            orthoRx.OrthoRxNum = Db.NonQ(command, true, "OrthoRxNum", "orthoRx");
        }
        return orthoRx.OrthoRxNum;
    }

    public static long InsertNoCache(OrthoRx orthoRx)
    {
        return InsertNoCache(orthoRx, false);
    }

    public static long InsertNoCache(OrthoRx orthoRx, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO orthorx (";
        if (isRandomKeys || useExistingPK) command += "OrthoRxNum,";
        command += "OrthoHardwareSpecNum,Description,ToothRange,ItemOrder) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(orthoRx.OrthoRxNum) + ",";
        command +=
            SOut.Long(orthoRx.OrthoHardwareSpecNum) + ","
                                                    + "'" + SOut.String(orthoRx.Description) + "',"
                                                    + "'" + SOut.String(orthoRx.ToothRange) + "',"
                                                    + SOut.Int(orthoRx.ItemOrder) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            orthoRx.OrthoRxNum = Db.NonQ(command, true, "OrthoRxNum", "orthoRx");
        return orthoRx.OrthoRxNum;
    }

    public static void Update(OrthoRx orthoRx)
    {
        var command = "UPDATE orthorx SET "
                      + "OrthoHardwareSpecNum=  " + SOut.Long(orthoRx.OrthoHardwareSpecNum) + ", "
                      + "Description         = '" + SOut.String(orthoRx.Description) + "', "
                      + "ToothRange          = '" + SOut.String(orthoRx.ToothRange) + "', "
                      + "ItemOrder           =  " + SOut.Int(orthoRx.ItemOrder) + " "
                      + "WHERE OrthoRxNum = " + SOut.Long(orthoRx.OrthoRxNum);
        Db.NonQ(command);
    }

    public static bool Update(OrthoRx orthoRx, OrthoRx oldOrthoRx)
    {
        var command = "";
        if (orthoRx.OrthoHardwareSpecNum != oldOrthoRx.OrthoHardwareSpecNum)
        {
            if (command != "") command += ",";
            command += "OrthoHardwareSpecNum = " + SOut.Long(orthoRx.OrthoHardwareSpecNum) + "";
        }

        if (orthoRx.Description != oldOrthoRx.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(orthoRx.Description) + "'";
        }

        if (orthoRx.ToothRange != oldOrthoRx.ToothRange)
        {
            if (command != "") command += ",";
            command += "ToothRange = '" + SOut.String(orthoRx.ToothRange) + "'";
        }

        if (orthoRx.ItemOrder != oldOrthoRx.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(orthoRx.ItemOrder) + "";
        }

        if (command == "") return false;
        command = "UPDATE orthorx SET " + command
                                        + " WHERE OrthoRxNum = " + SOut.Long(orthoRx.OrthoRxNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(OrthoRx orthoRx, OrthoRx oldOrthoRx)
    {
        if (orthoRx.OrthoHardwareSpecNum != oldOrthoRx.OrthoHardwareSpecNum) return true;
        if (orthoRx.Description != oldOrthoRx.Description) return true;
        if (orthoRx.ToothRange != oldOrthoRx.ToothRange) return true;
        if (orthoRx.ItemOrder != oldOrthoRx.ItemOrder) return true;
        return false;
    }

    public static void Delete(long orthoRxNum)
    {
        var command = "DELETE FROM orthorx "
                      + "WHERE OrthoRxNum = " + SOut.Long(orthoRxNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listOrthoRxNums)
    {
        if (listOrthoRxNums == null || listOrthoRxNums.Count == 0) return;
        var command = "DELETE FROM orthorx "
                      + "WHERE OrthoRxNum IN(" + string.Join(",", listOrthoRxNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}