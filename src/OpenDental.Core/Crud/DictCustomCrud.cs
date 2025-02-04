using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class DictCustomCrud
{
    public static List<DictCustom> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<DictCustom> TableToList(DataTable table)
    {
        var retVal = new List<DictCustom>();
        foreach (DataRow row in table.Rows)
        {
            var dictCustom = new DictCustom
            {
                DictCustomNum = SIn.Long(row["DictCustomNum"].ToString()),
                WordText = SIn.String(row["WordText"].ToString())
            };
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

    public static void Insert(DictCustom dictCustom)
    {
        var command = "INSERT INTO dictcustom (";

        command += "WordText) VALUES(";

        command +=
            "'" + SOut.String(dictCustom.WordText) + "')";
        {
            dictCustom.DictCustomNum = Db.NonQ(command, true, "DictCustomNum", "dictCustom");
        }
    }

    public static void Update(DictCustom dictCustom)
    {
        var command = "UPDATE dictcustom SET "
                      + "WordText     = '" + SOut.String(dictCustom.WordText) + "' "
                      + "WHERE DictCustomNum = " + SOut.Long(dictCustom.DictCustomNum);
        Db.NonQ(command);
    }

    public static void Delete(long dictCustomNum)
    {
        var command = "DELETE FROM dictcustom "
                      + "WHERE DictCustomNum = " + SOut.Long(dictCustomNum);
        Db.NonQ(command);
    }
}