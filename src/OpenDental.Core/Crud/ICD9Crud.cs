using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ICD9Crud
{
    public static List<ICD9> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ICD9> TableToList(DataTable table)
    {
        var retVal = new List<ICD9>();
        foreach (DataRow row in table.Rows)
        {
            var iCD9 = new ICD9
            {
                ICD9Num = SIn.Long(row["ICD9Num"].ToString()),
                ICD9Code = SIn.String(row["ICD9Code"].ToString()),
                Description = SIn.String(row["Description"].ToString()),
                DateTStamp = SIn.DateTime(row["DateTStamp"].ToString())
            };
            retVal.Add(iCD9);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ICD9> listICD9s, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ICD9";
        var table = new DataTable(tableName);
        table.Columns.Add("ICD9Num");
        table.Columns.Add("ICD9Code");
        table.Columns.Add("Description");
        table.Columns.Add("DateTStamp");
        foreach (var iCD9 in listICD9s)
            table.Rows.Add(SOut.Long(iCD9.ICD9Num), iCD9.ICD9Code, iCD9.Description, SOut.DateTime(iCD9.DateTStamp, false));
        return table;
    }

    public static void Insert(ICD9 iCD9)
    {
        var command = "INSERT INTO icd9 (";

        command += "ICD9Code,Description) VALUES(";

        command +=
            "'" + SOut.String(iCD9.ICD9Code) + "',"
            + "'" + SOut.String(iCD9.Description) + "')";
        //DateTStamp can only be set by MySQL

        iCD9.ICD9Num = Db.NonQ(command, true, "ICD9Num", "iCD9");
    }

    public static void Update(ICD9 iCD9)
    {
        var command = "UPDATE icd9 SET "
                      + "ICD9Code   = '" + SOut.String(iCD9.ICD9Code) + "', "
                      + "Description= '" + SOut.String(iCD9.Description) + "' "
                      //DateTStamp can only be set by MySQL
                      + "WHERE ICD9Num = " + SOut.Long(iCD9.ICD9Num);
        Db.NonQ(command);
    }
}