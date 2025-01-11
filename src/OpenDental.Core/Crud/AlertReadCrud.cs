using System.Collections.Generic;
using System.Data;
using DataConnectionBase;

namespace OpenDentBusiness.Crud;

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
        AlertRead alertRead;
        foreach (DataRow row in table.Rows)
        {
            alertRead = new AlertRead();
            alertRead.AlertReadNum = SIn.Long(row["AlertReadNum"].ToString());
            alertRead.AlertItemNum = SIn.Long(row["AlertItemNum"].ToString());
            alertRead.UserNum = SIn.Long(row["UserNum"].ToString());
            retVal.Add(alertRead);
        }

        return retVal;
    }

    public static long Insert(AlertRead alertRead)
    {
        var command = "INSERT INTO alertread (";

        command += "AlertItemNum,UserNum) VALUES(";

        command +=
            SOut.Long(alertRead.AlertItemNum) + ","
                                              + SOut.Long(alertRead.UserNum) + ")";
        {
            alertRead.AlertReadNum = Db.NonQ(command, true, "AlertReadNum", "alertRead");
        }
        return alertRead.AlertReadNum;
    }
}