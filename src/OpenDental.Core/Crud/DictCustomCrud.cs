#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class DictCustomCrud
{
    public static DictCustom SelectOne(long dictCustomNum)
    {
        var command = "SELECT * FROM dictcustom "
                      + "WHERE DictCustomNum = " + SOut.Long(dictCustomNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static DictCustom SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<DictCustom> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DictCustom> TableToList(DataTable table)
    {
        var retVal = new List<DictCustom>();
        DictCustom dictCustom;
        foreach (DataRow row in table.Rows)
        {
            dictCustom = new DictCustom();
            dictCustom.DictCustomNum = SIn.Long(row["DictCustomNum"].ToString());
            dictCustom.WordText = SIn.String(row["WordText"].ToString());
            retVal.Add(dictCustom);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<DictCustom> listDictCustoms, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "DictCustom";
        var table = new DataTable(tableName);
        table.Columns.Add("DictCustomNum");
        table.Columns.Add("WordText");
        foreach (var dictCustom in listDictCustoms)
            table.Rows.Add(SOut.Long(dictCustom.DictCustomNum), dictCustom.WordText);
        return table;
    }

    public static long Insert(DictCustom dictCustom)
    {
        return Insert(dictCustom, false);
    }

    public static long Insert(DictCustom dictCustom, bool useExistingPK)
    {
        var command = "INSERT INTO dictcustom (";

        command += "WordText) VALUES(";

        command +=
            "'" + SOut.String(dictCustom.WordText) + "')";
        {
            dictCustom.DictCustomNum = Db.NonQ(command, true, "DictCustomNum", "dictCustom");
        }
        return dictCustom.DictCustomNum;
    }

    public static long InsertNoCache(DictCustom dictCustom)
    {
        return InsertNoCache(dictCustom, false);
    }

    public static long InsertNoCache(DictCustom dictCustom, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO dictcustom (";
        if (isRandomKeys || useExistingPK) command += "DictCustomNum,";
        command += "WordText) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(dictCustom.DictCustomNum) + ",";
        command +=
            "'" + SOut.String(dictCustom.WordText) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            dictCustom.DictCustomNum = Db.NonQ(command, true, "DictCustomNum", "dictCustom");
        return dictCustom.DictCustomNum;
    }

    public static void Update(DictCustom dictCustom)
    {
        var command = "UPDATE dictcustom SET "
                      + "WordText     = '" + SOut.String(dictCustom.WordText) + "' "
                      + "WHERE DictCustomNum = " + SOut.Long(dictCustom.DictCustomNum);
        Db.NonQ(command);
    }

    public static bool Update(DictCustom dictCustom, DictCustom oldDictCustom)
    {
        var command = "";
        if (dictCustom.WordText != oldDictCustom.WordText)
        {
            if (command != "") command += ",";
            command += "WordText = '" + SOut.String(dictCustom.WordText) + "'";
        }

        if (command == "") return false;
        command = "UPDATE dictcustom SET " + command
                                           + " WHERE DictCustomNum = " + SOut.Long(dictCustom.DictCustomNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(DictCustom dictCustom, DictCustom oldDictCustom)
    {
        if (dictCustom.WordText != oldDictCustom.WordText) return true;
        return false;
    }

    public static void Delete(long dictCustomNum)
    {
        var command = "DELETE FROM dictcustom "
                      + "WHERE DictCustomNum = " + SOut.Long(dictCustomNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listDictCustomNums)
    {
        if (listDictCustomNums == null || listDictCustomNums.Count == 0) return;
        var command = "DELETE FROM dictcustom "
                      + "WHERE DictCustomNum IN(" + string.Join(",", listDictCustomNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}