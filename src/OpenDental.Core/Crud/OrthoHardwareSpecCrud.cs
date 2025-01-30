using System.Collections.Generic;
using System.Data;
using System.Drawing;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class OrthoHardwareSpecCrud
{
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

    public static void Insert(OrthoHardwareSpec orthoHardwareSpec)
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

    public static void Delete(long orthoHardwareSpecNum)
    {
        var command = "DELETE FROM orthohardwarespec "
                      + "WHERE OrthoHardwareSpecNum = " + SOut.Long(orthoHardwareSpecNum);
        Db.NonQ(command);
    }
}