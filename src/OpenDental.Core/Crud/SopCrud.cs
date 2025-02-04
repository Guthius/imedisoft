using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class SopCrud
{
    public static List<Sop> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Sop> TableToList(DataTable table)
    {
        var retVal = new List<Sop>();
        foreach (DataRow row in table.Rows)
        {
            var sop = new Sop
            {
                SopNum = SIn.Long(row["SopNum"].ToString()),
                SopCode = SIn.String(row["SopCode"].ToString()),
                Description = SIn.String(row["Description"].ToString())
            };
            retVal.Add(sop);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Sop> listSops, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Sop";
        var table = new DataTable(tableName);
        table.Columns.Add("SopNum");
        table.Columns.Add("SopCode");
        table.Columns.Add("Description");
        foreach (var sop in listSops)
            table.Rows.Add(SOut.Long(sop.SopNum), sop.SopCode, sop.Description);
        return table;
    }

    public static void Insert(Sop sop)
    {
        var command = "INSERT INTO sop (";

        command += "SopCode,Description) VALUES(";

        command +=
            "'" + SOut.String(sop.SopCode) + "',"
            + "'" + SOut.String(sop.Description) + "')";
        {
            sop.SopNum = Db.NonQ(command, true, "SopNum", "sop");
        }
    }

    public static void Update(Sop sop)
    {
        var command = "UPDATE sop SET "
                      + "SopCode    = '" + SOut.String(sop.SopCode) + "', "
                      + "Description= '" + SOut.String(sop.Description) + "' "
                      + "WHERE SopNum = " + SOut.Long(sop.SopNum);
        Db.NonQ(command);
    }
}