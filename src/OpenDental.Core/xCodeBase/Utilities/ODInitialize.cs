using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace CodeBase;

public class ODInitialize
{
    ///<summary>Indicates the list of packages that have already been custom loaded.</summary>
    private static List<string> _listPackages = new List<string>();

    ///<summary>This method is called from all Open Dental programs or projects. This method can throw. There is a good
    ///chance you should not let the user continue if the method throws as it can cause the program to behave in unexpecting ways.</summary>
    public static void Initialize()
    {
        //The default SecurityProtocol is "Ssl3|Tls".  We must add Tls12 in order to support Tls1.2 web reference handshakes, 
        //without breaking any web references using Ssl3 or Tls. This is necessary for XWeb payments.
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
    }

    ///<summary>This will only load the assembly once, even if called multiple times.
    ///Some packages are widly popular, 3rd-party libraries reference different versions than what the current project expects.
    ///This can cause runtime errors like "Could not load file or assembly 'package...'". The best way to fix these errors is to update the
    ///3rd-party library to use a newer version of the package. If that option is not available, you can call this method to force any failed
    ///loads of the package to use the specified dll.</summary>
    public static void FixPackageAssembly(string packageName, string pathToDll)
    {
        //Kick out early if we have already loaded the assembly.
        if (_listPackages.Contains(packageName))
        {
            return;
        }

        _listPackages.Add(packageName);
        AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveArgs) =>
        {
            string assemblyInfo = resolveArgs.Name; // e.g "Lib1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            string[] arrParts = assemblyInfo.Split(',');
            string name = arrParts[0];
            string fullName;
            if (name == packageName)
            {
                fullName = pathToDll;
            }
            else
            {
                return null;
            }

            return Assembly.LoadFile(fullName);
        };
    }
}