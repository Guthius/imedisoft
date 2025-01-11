using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class CptCrud
{
    public static Cpt SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Cpt> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Cpt> TableToList(DataTable table)
    {
        var retVal = new List<Cpt>();
        Cpt cpt;
        foreach (DataRow row in table.Rows)
        {
            cpt = new Cpt();
            cpt.CptNum = SIn.Long(row["CptNum"].ToString());
            cpt.CptCode = SIn.String(row["CptCode"].ToString());
            cpt.Description = SIn.String(row["Description"].ToString());
            cpt.VersionIDs = SIn.String(row["VersionIDs"].ToString());
            retVal.Add(cpt);
        }

        return retVal;
    }

    public static long Insert(Cpt cpt)
    {
        var command = "INSERT INTO cpt (";

        command += "CptCode,Description,VersionIDs) VALUES(";

        command +=
            "'" + SOut.String(cpt.CptCode) + "',"
            + "'" + SOut.String(cpt.Description) + "',"
            + "'" + SOut.String(cpt.VersionIDs) + "')";
        {
            cpt.CptNum = Db.NonQ(command, true, "CptNum", "cpt");
        }
        return cpt.CptNum;
    }

    public static void Update(Cpt cpt)
    {
        var command = "UPDATE cpt SET "
                      + "CptCode    = '" + SOut.String(cpt.CptCode) + "', "
                      + "Description= '" + SOut.String(cpt.Description) + "', "
                      + "VersionIDs = '" + SOut.String(cpt.VersionIDs) + "' "
                      + "WHERE CptNum = " + SOut.Long(cpt.CptNum);
        Db.NonQ(command);
    }
}