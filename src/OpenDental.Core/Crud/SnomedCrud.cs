using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class SnomedCrud
{
    public static Snomed SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Snomed> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Snomed> TableToList(DataTable table)
    {
        var retVal = new List<Snomed>();
        foreach (DataRow row in table.Rows)
        {
            var snomed = new Snomed
            {
                SnomedNum = SIn.Long(row["SnomedNum"].ToString()),
                SnomedCode = SIn.String(row["SnomedCode"].ToString()),
                Description = SIn.String(row["Description"].ToString())
            };
            retVal.Add(snomed);
        }

        return retVal;
    }

    public static void Insert(Snomed snomed)
    {
        var command = "INSERT INTO snomed (";

        command += "SnomedCode,Description) VALUES(";

        command +=
            "'" + SOut.String(snomed.SnomedCode) + "',"
            + "'" + SOut.String(snomed.Description) + "')";
        {
            snomed.SnomedNum = Db.NonQ(command, true, "SnomedNum", "snomed");
        }
    }

    public static void Update(Snomed snomed)
    {
        var command = "UPDATE snomed SET "
                      + "SnomedCode = '" + SOut.String(snomed.SnomedCode) + "', "
                      + "Description= '" + SOut.String(snomed.Description) + "' "
                      + "WHERE SnomedNum = " + SOut.Long(snomed.SnomedNum);
        Db.NonQ(command);
    }
}