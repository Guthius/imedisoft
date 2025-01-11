#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SnomedCrud
{
    public static Snomed SelectOne(long snomedNum)
    {
        var command = "SELECT * FROM snomed "
                      + "WHERE SnomedNum = " + SOut.Long(snomedNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

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
        Snomed snomed;
        foreach (DataRow row in table.Rows)
        {
            snomed = new Snomed();
            snomed.SnomedNum = SIn.Long(row["SnomedNum"].ToString());
            snomed.SnomedCode = SIn.String(row["SnomedCode"].ToString());
            snomed.Description = SIn.String(row["Description"].ToString());
            retVal.Add(snomed);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Snomed> listSnomeds, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Snomed";
        var table = new DataTable(tableName);
        table.Columns.Add("SnomedNum");
        table.Columns.Add("SnomedCode");
        table.Columns.Add("Description");
        foreach (var snomed in listSnomeds)
            table.Rows.Add(SOut.Long(snomed.SnomedNum), snomed.SnomedCode, snomed.Description);
        return table;
    }

    public static long Insert(Snomed snomed)
    {
        return Insert(snomed, false);
    }

    public static long Insert(Snomed snomed, bool useExistingPK)
    {
        var command = "INSERT INTO snomed (";

        command += "SnomedCode,Description) VALUES(";

        command +=
            "'" + SOut.String(snomed.SnomedCode) + "',"
            + "'" + SOut.String(snomed.Description) + "')";
        {
            snomed.SnomedNum = Db.NonQ(command, true, "SnomedNum", "snomed");
        }
        return snomed.SnomedNum;
    }

    public static long InsertNoCache(Snomed snomed)
    {
        return InsertNoCache(snomed, false);
    }

    public static long InsertNoCache(Snomed snomed, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO snomed (";
        if (isRandomKeys || useExistingPK) command += "SnomedNum,";
        command += "SnomedCode,Description) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(snomed.SnomedNum) + ",";
        command +=
            "'" + SOut.String(snomed.SnomedCode) + "',"
            + "'" + SOut.String(snomed.Description) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            snomed.SnomedNum = Db.NonQ(command, true, "SnomedNum", "snomed");
        return snomed.SnomedNum;
    }

    public static void Update(Snomed snomed)
    {
        var command = "UPDATE snomed SET "
                      + "SnomedCode = '" + SOut.String(snomed.SnomedCode) + "', "
                      + "Description= '" + SOut.String(snomed.Description) + "' "
                      + "WHERE SnomedNum = " + SOut.Long(snomed.SnomedNum);
        Db.NonQ(command);
    }

    public static bool Update(Snomed snomed, Snomed oldSnomed)
    {
        var command = "";
        if (snomed.SnomedCode != oldSnomed.SnomedCode)
        {
            if (command != "") command += ",";
            command += "SnomedCode = '" + SOut.String(snomed.SnomedCode) + "'";
        }

        if (snomed.Description != oldSnomed.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(snomed.Description) + "'";
        }

        if (command == "") return false;
        command = "UPDATE snomed SET " + command
                                       + " WHERE SnomedNum = " + SOut.Long(snomed.SnomedNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Snomed snomed, Snomed oldSnomed)
    {
        if (snomed.SnomedCode != oldSnomed.SnomedCode) return true;
        if (snomed.Description != oldSnomed.Description) return true;
        return false;
    }

    public static void Delete(long snomedNum)
    {
        var command = "DELETE FROM snomed "
                      + "WHERE SnomedNum = " + SOut.Long(snomedNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSnomedNums)
    {
        if (listSnomedNums == null || listSnomedNums.Count == 0) return;
        var command = "DELETE FROM snomed "
                      + "WHERE SnomedNum IN(" + string.Join(",", listSnomedNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}