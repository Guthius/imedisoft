using System;
using DataConnectionBase;

namespace OpenDentBusiness;

public class MiscData
{
    public static DateTime GetNowDateTime()
    {
        var table = DataCore.GetTable("SELECT NOW()");
        return SIn.DateTime(table.Rows[0][0].ToString());
    }

    public static DateTime GetNowDateTimeWithMilli()
    {
        var dbtime = DataCore.GetScalar("SELECT NOW()");
        var secondInit = SIn.DateTime(dbtime).Second;
        int secondCur;
        do
        {
            dbtime = DataCore.GetScalar("SELECT NOW()");
            secondCur = SIn.DateTime(dbtime).Second;
        } while (secondInit == secondCur);

        return SIn.DateTime(dbtime);
    }

    public static string GetCurrentDatabase()
    {
        var command = "SELECT database()";
        var table = DataCore.GetTable(command);
        return SIn.String(table.Rows[0][0].ToString());
    }

    public static string GetMySqlVersion(bool getRawVersion = false)
    {
        var command = "SELECT @@version";
        var table = DataCore.GetTable(command);
        var version = SIn.String(table.Rows[0][0].ToString());
        var arrayVersion = version.Split('.');
        try
        {
            if (getRawVersion)
            {
                return version;
            }

            return int.Parse(arrayVersion[0]) + "." + int.Parse(arrayVersion[1]);
        }
        catch
        {
            // ignored
        }

        return "0.0";
    }

    public static int GetMaxAllowedPacket()
    {
        var maxAllowedPacket = 0;
        //The SHOW command is used because it was able to run with a user that had no permissions whatsoever.
        var command = "SHOW GLOBAL VARIABLES WHERE Variable_name='max_allowed_packet'";
        var table = DataCore.GetTable(command);
        if (table.Rows.Count > 0)
        {
            maxAllowedPacket = SIn.Int(table.Rows[0]["Value"].ToString());
        }

        return maxAllowedPacket;
    }
}