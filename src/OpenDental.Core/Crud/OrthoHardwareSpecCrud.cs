#region

using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class OrthoHardwareSpecCrud
{
    public static OrthoHardwareSpec SelectOne(long orthoHardwareSpecNum)
    {
        var command = "SELECT * FROM orthohardwarespec "
                      + "WHERE OrthoHardwareSpecNum = " + SOut.Long(orthoHardwareSpecNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static OrthoHardwareSpec SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<OrthoHardwareSpec> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<OrthoHardwareSpec> TableToList(DataTable table)
    {
        var retVal = new List<OrthoHardwareSpec>();
        OrthoHardwareSpec orthoHardwareSpec;
        foreach (DataRow row in table.Rows)
        {
            orthoHardwareSpec = new OrthoHardwareSpec();
            orthoHardwareSpec.OrthoHardwareSpecNum = SIn.Long(row["OrthoHardwareSpecNum"].ToString());
            orthoHardwareSpec.OrthoHardwareType = (EnumOrthoHardwareType) SIn.Int(row["OrthoHardwareType"].ToString());
            orthoHardwareSpec.Description = SIn.String(row["Description"].ToString());
            orthoHardwareSpec.ItemColor = Color.FromArgb(SIn.Int(row["ItemColor"].ToString()));
            orthoHardwareSpec.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            orthoHardwareSpec.ItemOrder = SIn.Int(row["ItemOrder"].ToString());
            retVal.Add(orthoHardwareSpec);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<OrthoHardwareSpec> listOrthoHardwareSpecs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "OrthoHardwareSpec";
        var table = new DataTable(tableName);
        table.Columns.Add("OrthoHardwareSpecNum");
        table.Columns.Add("OrthoHardwareType");
        table.Columns.Add("Description");
        table.Columns.Add("ItemColor");
        table.Columns.Add("IsHidden");
        table.Columns.Add("ItemOrder");
        foreach (var orthoHardwareSpec in listOrthoHardwareSpecs)
            table.Rows.Add(SOut.Long(orthoHardwareSpec.OrthoHardwareSpecNum), SOut.Int((int) orthoHardwareSpec.OrthoHardwareType), orthoHardwareSpec.Description, SOut.Int(orthoHardwareSpec.ItemColor.ToArgb()), SOut.Bool(orthoHardwareSpec.IsHidden), SOut.Int(orthoHardwareSpec.ItemOrder));
        return table;
    }

    public static long Insert(OrthoHardwareSpec orthoHardwareSpec)
    {
        return Insert(orthoHardwareSpec, false);
    }

    public static long Insert(OrthoHardwareSpec orthoHardwareSpec, bool useExistingPK)
    {
        var command = "INSERT INTO orthohardwarespec (";

        command += "OrthoHardwareType,Description,ItemColor,IsHidden,ItemOrder) VALUES(";

        command +=
            SOut.Int((int) orthoHardwareSpec.OrthoHardwareType) + ","
                                                                + "'" + SOut.String(orthoHardwareSpec.Description) + "',"
                                                                + SOut.Int(orthoHardwareSpec.ItemColor.ToArgb()) + ","
                                                                + SOut.Bool(orthoHardwareSpec.IsHidden) + ","
                                                                + SOut.Int(orthoHardwareSpec.ItemOrder) + ")";
        {
            orthoHardwareSpec.OrthoHardwareSpecNum = Db.NonQ(command, true, "OrthoHardwareSpecNum", "orthoHardwareSpec");
        }
        return orthoHardwareSpec.OrthoHardwareSpecNum;
    }

    public static long InsertNoCache(OrthoHardwareSpec orthoHardwareSpec)
    {
        return InsertNoCache(orthoHardwareSpec, false);
    }

    public static long InsertNoCache(OrthoHardwareSpec orthoHardwareSpec, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO orthohardwarespec (";
        if (isRandomKeys || useExistingPK) command += "OrthoHardwareSpecNum,";
        command += "OrthoHardwareType,Description,ItemColor,IsHidden,ItemOrder) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(orthoHardwareSpec.OrthoHardwareSpecNum) + ",";
        command +=
            SOut.Int((int) orthoHardwareSpec.OrthoHardwareType) + ","
                                                                + "'" + SOut.String(orthoHardwareSpec.Description) + "',"
                                                                + SOut.Int(orthoHardwareSpec.ItemColor.ToArgb()) + ","
                                                                + SOut.Bool(orthoHardwareSpec.IsHidden) + ","
                                                                + SOut.Int(orthoHardwareSpec.ItemOrder) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            orthoHardwareSpec.OrthoHardwareSpecNum = Db.NonQ(command, true, "OrthoHardwareSpecNum", "orthoHardwareSpec");
        return orthoHardwareSpec.OrthoHardwareSpecNum;
    }

    public static void Update(OrthoHardwareSpec orthoHardwareSpec)
    {
        var command = "UPDATE orthohardwarespec SET "
                      + "OrthoHardwareType   =  " + SOut.Int((int) orthoHardwareSpec.OrthoHardwareType) + ", "
                      + "Description         = '" + SOut.String(orthoHardwareSpec.Description) + "', "
                      + "ItemColor           =  " + SOut.Int(orthoHardwareSpec.ItemColor.ToArgb()) + ", "
                      + "IsHidden            =  " + SOut.Bool(orthoHardwareSpec.IsHidden) + ", "
                      + "ItemOrder           =  " + SOut.Int(orthoHardwareSpec.ItemOrder) + " "
                      + "WHERE OrthoHardwareSpecNum = " + SOut.Long(orthoHardwareSpec.OrthoHardwareSpecNum);
        Db.NonQ(command);
    }

    public static bool Update(OrthoHardwareSpec orthoHardwareSpec, OrthoHardwareSpec oldOrthoHardwareSpec)
    {
        var command = "";
        if (orthoHardwareSpec.OrthoHardwareType != oldOrthoHardwareSpec.OrthoHardwareType)
        {
            if (command != "") command += ",";
            command += "OrthoHardwareType = " + SOut.Int((int) orthoHardwareSpec.OrthoHardwareType) + "";
        }

        if (orthoHardwareSpec.Description != oldOrthoHardwareSpec.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(orthoHardwareSpec.Description) + "'";
        }

        if (orthoHardwareSpec.ItemColor != oldOrthoHardwareSpec.ItemColor)
        {
            if (command != "") command += ",";
            command += "ItemColor = " + SOut.Int(orthoHardwareSpec.ItemColor.ToArgb()) + "";
        }

        if (orthoHardwareSpec.IsHidden != oldOrthoHardwareSpec.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(orthoHardwareSpec.IsHidden) + "";
        }

        if (orthoHardwareSpec.ItemOrder != oldOrthoHardwareSpec.ItemOrder)
        {
            if (command != "") command += ",";
            command += "ItemOrder = " + SOut.Int(orthoHardwareSpec.ItemOrder) + "";
        }

        if (command == "") return false;
        command = "UPDATE orthohardwarespec SET " + command
                                                  + " WHERE OrthoHardwareSpecNum = " + SOut.Long(orthoHardwareSpec.OrthoHardwareSpecNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(OrthoHardwareSpec orthoHardwareSpec, OrthoHardwareSpec oldOrthoHardwareSpec)
    {
        if (orthoHardwareSpec.OrthoHardwareType != oldOrthoHardwareSpec.OrthoHardwareType) return true;
        if (orthoHardwareSpec.Description != oldOrthoHardwareSpec.Description) return true;
        if (orthoHardwareSpec.ItemColor != oldOrthoHardwareSpec.ItemColor) return true;
        if (orthoHardwareSpec.IsHidden != oldOrthoHardwareSpec.IsHidden) return true;
        if (orthoHardwareSpec.ItemOrder != oldOrthoHardwareSpec.ItemOrder) return true;
        return false;
    }

    public static void Delete(long orthoHardwareSpecNum)
    {
        var command = "DELETE FROM orthohardwarespec "
                      + "WHERE OrthoHardwareSpecNum = " + SOut.Long(orthoHardwareSpecNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listOrthoHardwareSpecNums)
    {
        if (listOrthoHardwareSpecNums == null || listOrthoHardwareSpecNums.Count == 0) return;
        var command = "DELETE FROM orthohardwarespec "
                      + "WHERE OrthoHardwareSpecNum IN(" + string.Join(",", listOrthoHardwareSpecNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}