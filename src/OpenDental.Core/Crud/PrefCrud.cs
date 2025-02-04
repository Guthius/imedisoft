using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class PrefCrud
{
    public static List<Pref> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<Pref> TableToList(DataTable table)
    {
        var retVal = new List<Pref>();
        foreach (DataRow row in table.Rows)
        {
            var pref = new Pref
            {
                PrefNum = SIn.Long(row["PrefNum"].ToString()),
                PrefName = SIn.String(row["PrefName"].ToString()),
                ValueString = SIn.String(row["ValueString"].ToString()),
                Comments = SIn.String(row["Comments"].ToString())
            };
            retVal.Add(pref);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<Pref> listPrefs, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "Pref";
        var table = new DataTable(tableName);
        table.Columns.Add("PrefNum");
        table.Columns.Add("PrefName");
        table.Columns.Add("ValueString");
        table.Columns.Add("Comments");
        foreach (var pref in listPrefs)
            table.Rows.Add(SOut.Long(pref.PrefNum), pref.PrefName, pref.ValueString, pref.Comments);
        return table;
    }
}