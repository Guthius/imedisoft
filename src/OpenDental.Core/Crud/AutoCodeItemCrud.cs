using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class AutoCodeItemCrud
{
    public static List<AutoCodeItem> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<AutoCodeItem> TableToList(DataTable table)
    {
        var retVal = new List<AutoCodeItem>();
        AutoCodeItem autoCodeItem;
        foreach (DataRow row in table.Rows)
        {
            autoCodeItem = new AutoCodeItem();
            autoCodeItem.AutoCodeItemNum = SIn.Long(row["AutoCodeItemNum"].ToString());
            autoCodeItem.AutoCodeNum = SIn.Long(row["AutoCodeNum"].ToString());
            autoCodeItem.OldCode = SIn.String(row["OldCode"].ToString());
            autoCodeItem.CodeNum = SIn.Long(row["CodeNum"].ToString());
            retVal.Add(autoCodeItem);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<AutoCodeItem> listAutoCodeItems, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "AutoCodeItem";
        var table = new DataTable(tableName);
        table.Columns.Add("AutoCodeItemNum");
        table.Columns.Add("AutoCodeNum");
        table.Columns.Add("OldCode");
        table.Columns.Add("CodeNum");
        foreach (var autoCodeItem in listAutoCodeItems)
            table.Rows.Add(SOut.Long(autoCodeItem.AutoCodeItemNum), SOut.Long(autoCodeItem.AutoCodeNum), autoCodeItem.OldCode, SOut.Long(autoCodeItem.CodeNum));
        return table;
    }

    public static void Insert(AutoCodeItem autoCodeItem)
    {
        var command = "INSERT INTO autocodeitem (";

        command += "AutoCodeNum,OldCode,CodeNum) VALUES(";

        command +=
            SOut.Long(autoCodeItem.AutoCodeNum) + ","
                                                + "'" + SOut.String(autoCodeItem.OldCode) + "',"
                                                + SOut.Long(autoCodeItem.CodeNum) + ")";
        {
            autoCodeItem.AutoCodeItemNum = Db.NonQ(command, true, "AutoCodeItemNum", "autoCodeItem");
        }
    }

    public static void Update(AutoCodeItem autoCodeItem)
    {
        var command = "UPDATE autocodeitem SET "
                      + "AutoCodeNum    =  " + SOut.Long(autoCodeItem.AutoCodeNum) + ", "
                      + "OldCode        = '" + SOut.String(autoCodeItem.OldCode) + "', "
                      + "CodeNum        =  " + SOut.Long(autoCodeItem.CodeNum) + " "
                      + "WHERE AutoCodeItemNum = " + SOut.Long(autoCodeItem.AutoCodeItemNum);
        Db.NonQ(command);
    }
}