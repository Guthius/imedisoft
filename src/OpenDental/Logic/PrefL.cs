using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Windows.Forms;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace OpenDental;

public class PrefL
{
    public static bool ShouldDownloadUpdate(string remoteUri, string updateCode, out string updateInfoMajor, out string updateInfoMinor)
    {
        updateInfoMajor = "";
        updateInfoMinor = "";
        var shouldDownload = false;
        var fileName = "Manifest.txt";
        var webClient = new WebClient();
        var myStringWebResource = remoteUri + updateCode + "/" + fileName;
        Version versionNewBuild;
        string strNewVersion;
        var isAlphaBuild = false;
        var isBetaBuild = false;
        var isAlphaVersion = false;
        var isBetaVersion = false;
        try
        {
            using var streamReader = new StreamReader(webClient.OpenRead(myStringWebResource));
            var newBuild = streamReader.ReadLine();
            strNewVersion = streamReader.ReadLine(); //returns null if no second line
            if (newBuild.EndsWith("a"))
            {
                isAlphaBuild = true;
                newBuild = newBuild.Replace("a", "");
            }

            if (newBuild.EndsWith("b"))
            {
                isBetaBuild = true;
                newBuild = newBuild.Replace("b", "");
            }

            versionNewBuild = new Version(newBuild);
            if (versionNewBuild.Revision == -1)
            {
                versionNewBuild = new Version(versionNewBuild.Major, versionNewBuild.Minor, versionNewBuild.Build, 0);
            }

            if (strNewVersion != null && strNewVersion.EndsWith("a"))
            {
                isAlphaVersion = true;
                strNewVersion = strNewVersion.Replace("a", "");
            }

            if (strNewVersion != null && strNewVersion.EndsWith("b"))
            {
                isBetaVersion = true;
                strNewVersion = strNewVersion.Replace("b", "");
            }
        }
        catch
        {
            updateInfoMajor += Lan.g("FormUpdate", "Registration number not valid, or internet connection failed.  ");
            return false;
        }

        if (versionNewBuild == new Version(Application.ProductVersion))
        {
            updateInfoMajor += Lan.g("FormUpdate", "You are using the most current build of this version.  ");
        }
        else
        {
            //this also allows users to install previous versions.
            updateInfoMajor += Lan.g("FormUpdate", "A new build of this version is available for download:  ")
                               + versionNewBuild;
            if (isAlphaBuild)
            {
                updateInfoMajor += Lan.g("FormUpdate", "(alpha)  ");
            }

            if (isBetaBuild)
            {
                updateInfoMajor += Lan.g("FormUpdate", "(beta)  ");
            }

            shouldDownload = true;
        }

        //Whether or not build is current, we want to inform user about the next minor version
        if (strNewVersion != null)
        {
            //we don't really care what it is.
            updateInfoMinor += Lan.g("FormUpdate", "A newer version is also available.  ");
            if (isAlphaVersion)
            {
                updateInfoMinor += Lan.g("FormUpdate", "It is alpha (experimental), so it has bugs and " +
                                                       "you will need to update it frequently.  ");
            }

            if (isBetaVersion)
            {
                updateInfoMinor += Lan.g("FormUpdate", "It is beta (test), so it has some bugs and " +
                                                       "you will need to update it frequently.  ");
            }

            updateInfoMinor += Lan.g("FormUpdate", "Contact us for a new Registration number if you wish to use it.  ");
        }

        return shouldDownload;
    }

    public static void DownloadInstallPatchFromURI(string downloadUri, string destinationPath, bool runSetupAfterDownload, bool showShutdownWindow, string destinationPath2)
    {
        // TODO: Implement me
    }

    public static bool UpgradeOrInstallEConnector(bool isSilent, string updateServerName = null, bool doOverrideBlankUpdateServerName = false, bool isInvalidUpdateServerNameAllowed = false)
    {
        if (updateServerName == null)
        {
            updateServerName = PrefC.GetString(PrefName.WebServiceServerName);
        }

        if ( /* ODEnvironment.IsCloudServer */ false)
        {
            //We do not want to install in case this is a pre-test cloud database.
            if (!isSilent)
            {
                ODMessageBox.Show(Lans.g("ServicesHelper", "Not allowed to install the OpenDentalEConnector service in cloud mode."));
            }

            return false;
        }

        if (string.IsNullOrWhiteSpace(updateServerName))
        {
            //The calling method wants to install the eConnector which is going to be attempted farther down.
            //This will only be permitted if there haven't been any heartbeats within the last 24hrs.
            if (EServiceSignals.HasEverHadHeartbeat())
            {
                //If there is any Econnector activity don't install the Econnector.
                return false; //This is not an error and there is simply another eConnector installed somewhere.
            }

            //Check to see if the calling method wants this computer to take over the WebServiceServerName preference.
            if (doOverrideBlankUpdateServerName)
            {
                try
                {
                    Prefs.UpdateString(PrefName.WebServiceServerName, Dns.GetHostName());
                }
                catch (Exception ex)
                {
                    if (!isSilent)
                    {
                        ODMessageBox.Show(Lans.g("ServicesHelper", "Failed to get host name:") + " " + ex.Message);
                    }

                    return false;
                }
            }
        }
        else if (!ODEnvironment.IdIsThisComputer(updateServerName) && !isInvalidUpdateServerNameAllowed)
        {
            return false; //This is not an error and is simply not the correct computer that should have the eConnector installed on it.
        }

        var hadCustListener = UninstallCustListenerServices();

        //Installing and starting a new eConnector service was successful at this point so we should always return true past this point.
        //Tell HQ that this registration key is now running the eConnector service.
        try
        {
            var listenerServiceType = WebServiceMainHQProxy.SetEConnectorOn();
            var logText = Lan.g("PrefL", "eConnector status automatically set to") + " " + listenerServiceType + ".";
            SecurityLogs.MakeLogEntry(EnumPermType.EServicesSetup, 0, logText);
        }
        catch (Exception)
        {
            //Only notify the customer if they upgraded from the CustListener service and was unable to communicate with HQ.
            //Otherwise, the most likely scenario is that there was a network hiccup and the office already had the service installed and was already on the correct listener type.
            if (hadCustListener && !isSilent)
            {
                //Notify the user that HQ was not updated regarding the status of the eConnector (important).
                //Do not invoke the display error function since we do not want to return false at this point.
                MsgBox.Show("PrefL", "Could not update the eConnector communication status.  Please contact us to enable eServices.");
            }
        }

        return true;
    }

    private static bool UninstallCustListenerServices()
    {
        var hadCustListener = false;
        //Check to see if CustListener service is installed and uninstall any that are detected.
        var listServiceControllersCustListener = new List<ServiceController>();
        ODException.SwallowAnyException(() => listServiceControllersCustListener = ServicesHelper.GetServicesByExe("OpenDentalCustListener.exe"));
        for (var i = 0; i < listServiceControllersCustListener.Count; i++)
        {
            //Attempts to uninstall the service and does not throw UEs if the uninstall failed.
            if (ServicesHelper.Uninstall(listServiceControllersCustListener[i]))
            {
                hadCustListener = true;
            }
        }

        return hadCustListener;
    }
}