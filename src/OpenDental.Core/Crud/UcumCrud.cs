#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class UcumCrud
{
    public static Ucum SelectOne(long ucumNum)
    {
        var command = "SELECT * FROM ucum "
                      + "WHERE UcumNum = " + SOut.Long(ucumNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static Ucum SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<Ucum> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Ucum> TableToList(DataTable table)
    {
        var retVal = new List<Ucum>();
        Ucum ucum;
        foreach (DataRow row in table.Rows)
        {
            ucum = new Ucum();
            ucum.UcumNum = SIn.Long(row["UcumNum"].ToString());
            ucum.UcumCode = SIn.String(row["UcumCode"].ToString());
            ucum.Description = SIn.String(row["Description"].ToString());
            ucum.IsInUse = SIn.Bool(row["IsInUse"].ToString());
            retVal.Add(ucum);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Ucum> listUcums, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Ucum";
        var table = new DataTable(tableName);
        table.Columns.Add("UcumNum");
        table.Columns.Add("UcumCode");
        table.Columns.Add("Description");
        table.Columns.Add("IsInUse");
        foreach (var ucum in listUcums)
            table.Rows.Add(SOut.Long(ucum.UcumNum), ucum.UcumCode, ucum.Description, SOut.Bool(ucum.IsInUse));
        return table;
    }

    public static long Insert(Ucum ucum)
    {
        return Insert(ucum, false);
    }

    public static long Insert(Ucum ucum, bool useExistingPK)
    {
        var command = "INSERT INTO ucum (";

        command += "UcumCode,Description,IsInUse) VALUES(";

        command +=
            "'" + SOut.String(ucum.UcumCode) + "',"
            + "'" + SOut.String(ucum.Description) + "',"
            + SOut.Bool(ucum.IsInUse) + ")";
        {
            ucum.UcumNum = Db.NonQ(command, true, "UcumNum", "ucum");
        }
        return ucum.UcumNum;
    }

    public static long InsertNoCache(Ucum ucum)
    {
        return InsertNoCache(ucum, false);
    }

    public static long InsertNoCache(Ucum ucum, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO ucum (";
        if (isRandomKeys || useExistingPK) command += "UcumNum,";
        command += "UcumCode,Description,IsInUse) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(ucum.UcumNum) + ",";
        command +=
            "'" + SOut.String(ucum.UcumCode) + "',"
            + "'" + SOut.String(ucum.Description) + "',"
            + SOut.Bool(ucum.IsInUse) + ")";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            ucum.UcumNum = Db.NonQ(command, true, "UcumNum", "ucum");
        return ucum.UcumNum;
    }

    public static void Update(Ucum ucum)
    {
        var command = "UPDATE ucum SET "
                      + "UcumCode   = '" + SOut.String(ucum.UcumCode) + "', "
                      + "Description= '" + SOut.String(ucum.Description) + "', "
                      + "IsInUse    =  " + SOut.Bool(ucum.IsInUse) + " "
                      + "WHERE UcumNum = " + SOut.Long(ucum.UcumNum);
        Db.NonQ(command);
    }

    public static bool Update(Ucum ucum, Ucum oldUcum)
    {
        var command = "";
        if (ucum.UcumCode != oldUcum.UcumCode)
        {
            if (command != "") command += ",";
            command += "UcumCode = '" + SOut.String(ucum.UcumCode) + "'";
        }

        if (ucum.Description != oldUcum.Description)
        {
            if (command != "") command += ",";
            command += "Description = '" + SOut.String(ucum.Description) + "'";
        }

        if (ucum.IsInUse != oldUcum.IsInUse)
        {
            if (command != "") command += ",";
            command += "IsInUse = " + SOut.Bool(ucum.IsInUse) + "";
        }

        if (command == "") return false;
        command = "UPDATE ucum SET " + command
                                     + " WHERE UcumNum = " + SOut.Long(ucum.UcumNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(Ucum ucum, Ucum oldUcum)
    {
        if (ucum.UcumCode != oldUcum.UcumCode) return true;
        if (ucum.Description != oldUcum.Description) return true;
        if (ucum.IsInUse != oldUcum.IsInUse) return true;
        return false;
    }

    public static void Delete(long ucumNum)
    {
        var command = "DELETE FROM ucum "
                      + "WHERE UcumNum = " + SOut.Long(ucumNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listUcumNums)
    {
        if (listUcumNums == null || listUcumNums.Count == 0) return;
        var command = "DELETE FROM ucum "
                      + "WHERE UcumNum IN(" + string.Join(",", listUcumNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}