using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class CountyCrud
{
    public static List<County> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<County> TableToList(DataTable table)
    {
        var retVal = new List<County>();
        County county;
        foreach (DataRow row in table.Rows)
        {
            county = new County();
            county.CountyNum = SIn.Long(row["CountyNum"].ToString());
            county.CountyName = SIn.String(row["CountyName"].ToString());
            county.CountyCode = SIn.String(row["CountyCode"].ToString());
            retVal.Add(county);
        }

        return retVal;
    }

    public static long Insert(County county)
    {
        var command = "INSERT INTO county (";

        command += "CountyName,CountyCode) VALUES(";

        command +=
            "'" + SOut.String(county.CountyName) + "',"
            + "'" + SOut.String(county.CountyCode) + "')";
        {
            county.CountyNum = Db.NonQ(command, true, "CountyNum", "county");
        }
        return county.CountyNum;
    }
}