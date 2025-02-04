using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class AlertReadCrud
{
    public static List<AlertRead> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<AlertRead> TableToList(DataTable table)
    {
        var retVal = new List<AlertRead>();
        foreach (DataRow row in table.Rows)
        {
            var alertRead = new AlertRead
            {
                AlertItemNum = SIn.Long(row["AlertItemNum"].ToString()),
                UserNum = SIn.Long(row["UserNum"].ToString())
            };
            retVal.Add(alertRead);
        }

        return retVal;
    }

    public static void Insert(AlertRead alertRead)
    {
        var command = "INSERT INTO alertread (";

        command += "AlertItemNum,UserNum) VALUES(";

        command +=
            SOut.Long(alertRead.AlertItemNum) + ","
                                              + SOut.Long(alertRead.UserNum) + ")";
        {
            Db.NonQ(command, true, "AlertReadNum", "alertRead");
        }
    }
}