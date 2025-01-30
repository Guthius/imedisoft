using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class OrthoRxCrud
{
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

    public static void Insert(OrthoRx orthoRx)
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

    public static void Delete(long orthoRxNum)
    {
        var command = "DELETE FROM orthorx "
                      + "WHERE OrthoRxNum = " + SOut.Long(orthoRxNum);
        Db.NonQ(command);
    }
}