using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ProcButtonItemCrud
{
    public static List<ProcButtonItem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ProcButtonItem> TableToList(DataTable table)
    {
        var retVal = new List<ProcButtonItem>();
        foreach (DataRow row in table.Rows)
        {
            var procButtonItem = new ProcButtonItem
            {
                ProcButtonItemNum = SIn.Long(row["ProcButtonItemNum"].ToString()),
                ProcButtonNum = SIn.Long(row["ProcButtonNum"].ToString()),
                OldCode = SIn.String(row["OldCode"].ToString()),
                AutoCodeNum = SIn.Long(row["AutoCodeNum"].ToString()),
                CodeNum = SIn.Long(row["CodeNum"].ToString()),
                ItemOrder = SIn.Long(row["ItemOrder"].ToString())
            };
            retVal.Add(procButtonItem);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ProcButtonItem> listProcButtonItems, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ProcButtonItem";
        var table = new DataTable(tableName);
        table.Columns.Add("ProcButtonItemNum");
        table.Columns.Add("ProcButtonNum");
        table.Columns.Add("OldCode");
        table.Columns.Add("AutoCodeNum");
        table.Columns.Add("CodeNum");
        table.Columns.Add("ItemOrder");
        foreach (var procButtonItem in listProcButtonItems)
            table.Rows.Add(SOut.Long(procButtonItem.ProcButtonItemNum), SOut.Long(procButtonItem.ProcButtonNum), procButtonItem.OldCode, SOut.Long(procButtonItem.AutoCodeNum), SOut.Long(procButtonItem.CodeNum), SOut.Long(procButtonItem.ItemOrder));
        return table;
    }

    public static void Insert(ProcButtonItem procButtonItem)
    {
        var command = "INSERT INTO procbuttonitem (";

        command += "ProcButtonNum,OldCode,AutoCodeNum,CodeNum,ItemOrder) VALUES(";

        command +=
            SOut.Long(procButtonItem.ProcButtonNum) + ","
                                                    + "'" + SOut.String(procButtonItem.OldCode) + "',"
                                                    + SOut.Long(procButtonItem.AutoCodeNum) + ","
                                                    + SOut.Long(procButtonItem.CodeNum) + ","
                                                    + SOut.Long(procButtonItem.ItemOrder) + ")";
        {
            procButtonItem.ProcButtonItemNum = Db.NonQ(command, true, "ProcButtonItemNum", "procButtonItem");
        }
    }
}