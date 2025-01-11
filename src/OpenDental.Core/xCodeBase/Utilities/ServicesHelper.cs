using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeBase;

public class ServicesHelper
{
    private static readonly string InstallUtilPath = Path.Combine(Directory.GetCurrentDirectory(), "InstallUtil", "installutil.exe");

    private static void ExecuteProcess(string fileName, string arguments, string workingDirectory = "")
    {
        var process = new Process();
        
        if (!string.IsNullOrEmpty(workingDirectory))
        {
            process.StartInfo.WorkingDirectory = workingDirectory;
        }

        process.StartInfo.FileName = fileName;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        process.StandardOutput.ReadToEnd();
        process.WaitForExit(10000);
    }

    public static bool IsMySqlService(string pathToExe)
    {
        return pathToExe.ToLower().Contains("mysqld.exe");
    }

    public static bool Uninstall(ServiceController service)
    {
        try
        {
            Uninstall(service.ServiceName);
            
            return GetServiceByServiceName(service.ServiceName) != null;
        }
        catch
        {
            return false;
        }
    }

    private static void UninstallMySqlService(string imagePath, string serviceName)
    {
        Stop(serviceName);
        
        var pathToExe = GetMySqlPathToExe(imagePath);
        var pathBin = Path.GetDirectoryName(pathToExe);
        
        ExecuteProcess("CMD.exe", "/C mysqld.exe --remove " + serviceName, workingDirectory: pathBin);
    }

    public static void Uninstall(string serviceName)
    {
        var hklm = Registry.LocalMachine;
        hklm = hklm.OpenSubKey(@"System\CurrentControlSet\Services\" + serviceName);
        var imagePath = hklm.GetValue("ImagePath").ToString().Replace("\"", "");
        if (IsMySqlService(imagePath))
        {
            UninstallMySqlService(imagePath, serviceName);
        }
        else
        {
            var serviceFile = new FileInfo(imagePath);
            
            ExecuteProcess(InstallUtilPath, "/u /ServiceName=" + serviceName + " \"" + serviceFile.FullName + "\"", serviceFile.DirectoryName);
        }
    }

    public static bool Start(ServiceController service, bool hasExceptions = false, int timeoutSeconds = 7)
    {
        try
        {
            if (service.Status != ServiceControllerStatus.Stopped && service.Status != ServiceControllerStatus.StopPending)
            {
                return true;
            }

            service.MachineName = Environment.MachineName;
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(timeoutSeconds));
        }
        catch
        {
            if (hasExceptions)
            {
                throw;
            }

            return false;
        }

        return true;
    }

    public static string StartServices(List<ServiceController> listServices, bool hasExceptions = false)
    {
        var stringBuilderErrors = new StringBuilder();
        
        foreach (var service in listServices)
        {
            try
            {
                if (!Start(service, hasExceptions))
                {
                    stringBuilderErrors.AppendLine(service.DisplayName);
                }
            }
            catch (Exception ex)
            {
                if (hasExceptions)
                {
                    throw ex;
                }

                stringBuilderErrors.AppendLine(service.DisplayName);
            }
        }

        return stringBuilderErrors.ToString();
    }

    public static void Stop(string serviceName, bool hasExceptions = false)
    {
        Stop(GetServiceByServiceName(serviceName), hasExceptions);
    }

    public static void Stop(ServiceController service, bool hasExceptions = false, int timeoutSeconds = 7)
    {
        try
        {
            if (service.Status == ServiceControllerStatus.Stopped || service.Status == ServiceControllerStatus.StopPending)
            {
                return;
            }

            service.MachineName = Environment.MachineName;
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(timeoutSeconds));
        }
        catch
        {
            if (hasExceptions)
            {
                throw;
            }
        }
    }

    public static List<ServiceController> GetServices()
    {
        return ServiceController.GetServices().ToList();
    }

    public static List<ServiceController> GetServicesByRegistryImagePath(string serviceExeName)
    {
        var listServices = GetServices();
        var listMatchingServices = new List<ServiceController>();
        foreach (var service in listServices)
        {
            var hklm = Registry.LocalMachine;
            hklm = hklm.OpenSubKey(@"System\CurrentControlSet\Services\" + service.ServiceName);
            var arrayExePath = hklm.GetValue("ImagePath").ToString().Replace("\"", "").Split('\\');
            //This will not work if in the future we allow command line args for the listener service that include paths.
            if (arrayExePath[arrayExePath.Length - 1].StartsWith(serviceExeName))
            {
                listMatchingServices.Add(service);
            }
        }

        return listMatchingServices;
    }

    public static ServiceController GetServiceByServiceName(string serviceName, bool isCaseSensitive = true, bool canPartialMatch = false)
    {
        if (isCaseSensitive && canPartialMatch)
        {
            return GetServices().FirstOrDefault(x => x.ServiceName.Contains(serviceName));
        }
        else if (isCaseSensitive && !canPartialMatch)
        {
            return GetServices().FirstOrDefault(x => x.ServiceName == serviceName);
        }
        else if (!isCaseSensitive && canPartialMatch)
        {
            return GetServices().FirstOrDefault(x => x.ServiceName.ToLower().Contains(serviceName.ToLower()));
        }
        else if (!isCaseSensitive && !canPartialMatch)
        {
            return GetServices().FirstOrDefault(x => x.ServiceName.ToLower() == serviceName.ToLower());
        }

        return null;
    }

    public static List<ServiceController> GetAllOpenDentServices()
    {
        return GetServices().FindAll(x => x.ServiceName.StartsWith("OpenDent"));
    }

    public static string GetMySqlPathToExe(string imagePath)
    {
        //"C:\Program Files\MariaDB 10.5\bin\mysqld.exe" --defaults-file="C:\Program Files\MariaDB 10.5\my.ini" MariaDB
        imagePath = imagePath.Replace("\"", "");
        var match = Regex.Match(imagePath, @"^(.*?mysqld.exe)");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return imagePath;
    }

    public static List<ServiceController> GetServicesByExe(string exeName)
    {
        var retVal = new List<ServiceController>();
        var services = GetServices();
        foreach (var service in services)
        {
            var hklm = Registry.LocalMachine;
            hklm = hklm.OpenSubKey(Path.Combine(@"System\CurrentControlSet\Services\", service.ServiceName));
            if (hklm.GetValue("ImagePath") == null)
            {
                continue;
            }

            var installedServicePath = hklm.GetValue("ImagePath").ToString().Replace("\"", "");
            if (installedServicePath.Contains(exeName))
            {
                retVal.Add(service);
            }
        }

        return retVal;
    }
}