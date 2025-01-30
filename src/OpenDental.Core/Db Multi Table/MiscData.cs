using System;
using System.Linq;
using System.Management;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Microsoft.VisualBasic.Devices;

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

    public static string GetOSVersionInfo()
    {
        var computerInfo = new ComputerInfo();
        var versionInfo = computerInfo.OSFullName + (Environment.Is64BitOperatingSystem ? " 64-bit" : " 32-bit");
        try
        {
            var mangementQuery = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            var systemInfo = mangementQuery.Get().Cast<ManagementObject>().FirstOrDefault();
            versionInfo += " Build " + systemInfo.Properties["Version"].Value;
        }
        catch
        {
            // ignored
        }

        return versionInfo;
    }

    public static string GetAssemblyVersion()
    {
        return typeof(MiscData).Assembly.GetName().Version.ToString();
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

    public static void SetSqlMode()
    {
        try
        {
            if (PrefC.GetBool(PrefName.DatabaseGlobalVariablesDontSet))
            {
                return;
            }
        }
        catch
        {
            // ignored
        }

        //The SHOW command is used because it was able to run with a user that had no permissions whatsoever.
        var command = "SHOW GLOBAL VARIABLES WHERE Variable_name='sql_mode'";
        var table = DataCore.GetTable(command);
        //We want to run the SET GLOBAL command when no rows were returned (above query failed) or if the sql_mode is not blank or NO_AUTO_CREATE_USER
        //(set to something that could cause errors).
        if (table.Rows.Count < 1 || (table.Rows[0]["Value"].ToString() != "" && table.Rows[0]["Value"].ToString().ToUpper() != "NO_AUTO_CREATE_USER"))
        {
            command = "SET GLOBAL sql_mode=''"; //in case user did not use our my.ini file.  http://www.opendental.com/manual/mysqlservervariables.html
            Db.NonQ(command);
        }
    }
}