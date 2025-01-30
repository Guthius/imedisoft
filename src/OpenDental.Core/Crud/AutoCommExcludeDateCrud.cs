using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class AutoCommExcludeDateCrud
{
    public static List<AutoCommExcludeDate> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<AutoCommExcludeDate> TableToList(DataTable table)
    {
        var retVal = new List<AutoCommExcludeDate>();
        AutoCommExcludeDate autoCommExcludeDate;
        foreach (DataRow row in table.Rows)
        {
            autoCommExcludeDate = new AutoCommExcludeDate();
            autoCommExcludeDate.AutoCommExcludeDateNum = SIn.Long(row["AutoCommExcludeDateNum"].ToString());
            autoCommExcludeDate.ClinicNum = SIn.Long(row["ClinicNum"].ToString());
            autoCommExcludeDate.DateExclude = SIn.DateTime(row["DateExclude"].ToString());
            retVal.Add(autoCommExcludeDate);
        }

        return retVal;
    }

    public static long Insert(AutoCommExcludeDate autoCommExcludeDate)
    {
        var command = "INSERT INTO autocommexcludedate (";

        command += "ClinicNum,DateExclude) VALUES(";

        command +=
            SOut.Long(autoCommExcludeDate.ClinicNum) + ","
                                                     + SOut.DateTime(autoCommExcludeDate.DateExclude) + ")";
        {
            autoCommExcludeDate.AutoCommExcludeDateNum = Db.NonQ(command, true, "AutoCommExcludeDateNum", "autoCommExcludeDate");
        }
        return autoCommExcludeDate.AutoCommExcludeDateNum;
    }

    public static void Delete(long autoCommExcludeDateNum)
    {
        var command = "DELETE FROM autocommexcludedate "
                      + "WHERE AutoCommExcludeDateNum = " + SOut.Long(autoCommExcludeDateNum);
        Db.NonQ(command);
    }
}