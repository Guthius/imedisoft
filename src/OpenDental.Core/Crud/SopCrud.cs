#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class SopCrud
{
    public static Sop SelectOne(long sopNum)
    {
        var command = "SELECT * FROM sop "
                      + "WHERE SopNum = " + SOut.Long(sopNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Sop SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Sop> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Sop> TableToList(DataTable table)
    {
        var retVal = new List<Sop>();
        Sop sop;
        foreach (DataRow row in table.Rows)
        {
            sop = new Sop();
            sop.SopNum = SIn.Long(row["SopNum"].ToString());
            sop.SopCode = SIn.String(row["SopCode"].ToString());
            sop.Description = SIn.String(row["Description"].ToString());
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

    public static long Insert(Sop sop)
    {
        return Insert(sop, false);
    }

    public static long Insert(Sop sop, bool useExistingPK)
    {
        var command = "INSERT INTO sop (";

        command += "SopCode,Description) VALUES(";

        command +=
            "'" + SOut.String(sop.SopCode) + "',"
            + "'" + SOut.String(sop.Description) + "')";
        {
            sop.SopNum = Db.NonQ(command, true, "SopNum", "sop");
        }
        return sop.SopNum;
    }

    public static long InsertNoCache(Sop sop)
    {
        return InsertNoCache(sop, false);
    }

    public static long InsertNoCache(Sop sop, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO sop (";
        if (isRandomKeys || useExistingPK) command += "SopNum,";
        command += "SopCode,Description) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(sop.SopNum) + ",";
        command +=
            "'" + SOut.String(sop.SopCode) + "',"
            + "'" + SOut.String(sop.Description) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            sop.SopNum = Db.NonQ(command, true, "SopNum", "sop");
        return sop.SopNum;
    }

    public static void Update(Sop sop)
    {
        var command = "UPDATE sop SET "
                      + "SopCode    = '" + SOut.String(sop.SopCode) + "', "
                      + "Description= '" + SOut.String(sop.Description) + "' "
                      + "WHERE SopNum = " + SOut.Long(sop.SopNum);
        Db.NonQ(command);
    }

    public static bool Update(Sop sop, Sop oldSop)
    {
        var command = "";
        if (sop.SopCode != oldSop.SopCode)
        {
            if (command != "") command += ",";
            command += "SopCode = '" + SOut.String(sop.SopCode) + "'";
        }

        if (sop.Description != oldSop.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(sop.Description) + "'";
        }

        if (command == "") return false;
        command = "UPDATE sop SET " + command
                                    + " WHERE SopNum = " + SOut.Long(sop.SopNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Sop sop, Sop oldSop)
    {
        if (sop.SopCode != oldSop.SopCode) return true;
        if (sop.Description != oldSop.Description) return true;
        return false;
    }

    public static void Delete(long sopNum)
    {
        var command = "DELETE FROM sop "
                      + "WHERE SopNum = " + SOut.Long(sopNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listSopNums)
    {
        if (listSopNums == null || listSopNums.Count == 0) return;
        var command = "DELETE FROM sop "
                      + "WHERE SopNum IN(" + string.Join(",", listSopNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}