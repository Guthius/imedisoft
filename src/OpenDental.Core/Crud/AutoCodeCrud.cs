using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class AutoCodeCrud
{
    public static List<AutoCode> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<AutoCode> TableToList(DataTable table)
    {
        var retVal = new List<AutoCode>();
        foreach (DataRow row in table.Rows)
        {
            var autoCode = new AutoCode
            {
                AutoCodeNum = SIn.Long(row["AutoCodeNum"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                IsHidden = SIn.Bool(row["IsHidden"].ToString()),
                LessIntrusive = SIn.Bool(row["LessIntrusive"].ToString())
            };
            retVal.Add(autoCode);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<AutoCode> listAutoCodes, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "AutoCode";
        var table = new DataTable(tableName);
        table.Columns.Add("AutoCodeNum");
        table.Columns.Add("Description");
        table.Columns.Add("IsHidden");
        table.Columns.Add("LessIntrusive");
        foreach (var autoCode in listAutoCodes)
            table.Rows.Add(SOut.Long(autoCode.AutoCodeNum), autoCode.Description, SOut.Bool(autoCode.IsHidden), SOut.Bool(autoCode.LessIntrusive));
        return table;
    }

    public static void Insert(AutoCode autoCode)
    {
        var command = "INSERT INTO autocode (";

        command += "Description,IsHidden,LessIntrusive) VALUES(";

        command +=
            "'" + SOut.String(autoCode.Description) + "',"
            + SOut.Bool(autoCode.IsHidden) + ","
            + SOut.Bool(autoCode.LessIntrusive) + ")";
        {
            autoCode.AutoCodeNum = Db.NonQ(command, true, "AutoCodeNum", "autoCode");
        }
    }

    public static void Update(AutoCode autoCode)
    {
        var command = "UPDATE autocode SET "
                      + "Description  = '" + SOut.String(autoCode.Description) + "', "
                      + "IsHidden     =  " + SOut.Bool(autoCode.IsHidden) + ", "
                      + "LessIntrusive=  " + SOut.Bool(autoCode.LessIntrusive) + " "
                      + "WHERE AutoCodeNum = " + SOut.Long(autoCode.AutoCodeNum);
        Db.NonQ(command);
    }

    public static void Delete(long autoCodeNum)
    {
        var command = "DELETE FROM autocode "
                      + "WHERE AutoCodeNum = " + SOut.Long(autoCodeNum);
        Db.NonQ(command);
    }
}