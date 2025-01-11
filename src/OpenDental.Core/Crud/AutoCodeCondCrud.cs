using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

public class AutoCodeCondCrud
{
    public static List<AutoCodeCond> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<AutoCodeCond> TableToList(DataTable table)
    {
        var retVal = new List<AutoCodeCond>();
        AutoCodeCond autoCodeCond;
        foreach (DataRow row in table.Rows)
        {
            autoCodeCond = new AutoCodeCond();
            autoCodeCond.AutoCodeCondNum = SIn.Long(row["AutoCodeCondNum"].ToString());
            autoCodeCond.AutoCodeItemNum = SIn.Long(row["AutoCodeItemNum"].ToString());
            autoCodeCond.Cond = (AutoCondition) SIn.Int(row["Cond"].ToString());
            retVal.Add(autoCodeCond);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<AutoCodeCond> listAutoCodeConds, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "AutoCodeCond";
        var table = new DataTable(tableName);
        table.Columns.Add("AutoCodeCondNum");
        table.Columns.Add("AutoCodeItemNum");
        table.Columns.Add("Cond");
        foreach (var autoCodeCond in listAutoCodeConds)
            table.Rows.Add(SOut.Long(autoCodeCond.AutoCodeCondNum), SOut.Long(autoCodeCond.AutoCodeItemNum), SOut.Int((int) autoCodeCond.Cond));
        return table;
    }

    public static long Insert(AutoCodeCond autoCodeCond)
    {
        var command = "INSERT INTO autocodecond (";

        command += "AutoCodeItemNum,Cond) VALUES(";

        command +=
            SOut.Long(autoCodeCond.AutoCodeItemNum) + ","
                                                    + SOut.Int((int) autoCodeCond.Cond) + ")";
        {
            autoCodeCond.AutoCodeCondNum = Db.NonQ(command, true, "AutoCodeCondNum", "autoCodeCond");
        }
        return autoCodeCond.AutoCodeCondNum;
    }
}