using System;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace CodeBase;

public class ODEnvironment
{
    public static string MachineName => Environment.MachineName;

    public static bool IdIsThisComputer(string id)
    {
        id = id.ToLower();

        if (Environment.MachineName.ToLower() == id)
        {
            return true;
        }

        IPHostEntry iphostentry;
        try
        {
            iphostentry = Dns.GetHostEntry(Environment.MachineName);
        }
        catch
        {
            return false;
        }

        return iphostentry.AddressList.Any(ipaddress => ipaddress.ToString() == id);
    }

    public static bool IsWindows7(bool hasExceptions = true)
    {
        Version version;
        try
        {
            var pathToKernel32Dll = ODFileUtils.CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.System), "kernel32.dll");
            var versionInfo = FileVersionInfo.GetVersionInfo(pathToKernel32Dll);

            version = new Version(versionInfo.ProductVersion);
        }
        catch (Exception ex)
        {
            if (hasExceptions)
            {
                throw ex;
            }

            return false;
        }

        return version.Major == 6 && version.Minor == 1;
    }
}