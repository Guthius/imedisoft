using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using CDT;
using CodeBase;
using DataConnectionBase;
using MySqlConnector;
using OpenDental.Cloud.Shared;
using OpenDental.UI;
using OpenDentBusiness;
using OpenDentBusiness.Misc;

namespace OpenDental
{
    public class PrefL
    {
        ///<summary>Directory names of special folders that need to have their files preserved.
        ///These names should be treated as if they are being appended to the end of a directory path.
        ///Any sub folders should be explicitly listed within this list because the copy method used is not recursive.
        ///E.g. "\\Parent", "\\Parent\\Child", and "\\Gramps\\Dad\\Bro" are all valid directory names.</summary>
        private static readonly List<string> _listSpecialDirs = new List<string>()
        {
            "\\Sparks3D",
            "\\OpenDentalReplicationService",
        };

        ///<summary>Returns true if the download at the specified remoteUri with the given registration code should be downloaded and installed as an update, and false is returned otherwise. Also, information about the decision making process is stored in the updateInfoMajor and updateInfoMinor strings, but only holds significance to a human user.</summary>
        public static bool ShouldDownloadUpdate(string remoteUri, string updateCode, out string updateInfoMajor, out string updateInfoMinor)
        {
            updateInfoMajor = "";
            updateInfoMinor = "";
            bool shouldDownload = false;
            string fileName = "Manifest.txt";
            WebClient webClient = new WebClient();
            string myStringWebResource = remoteUri + updateCode + "/" + fileName;
            Version versionNewBuild = null;
            string strNewVersion = "";
            string newBuild = "";
            bool isAlphaBuild = false;
            bool isBetaBuild = false;
            bool isAlphaVersion = false;
            bool isBetaVersion = false;
            try
            {
                using StreamReader streamReader = new StreamReader(webClient.OpenRead(myStringWebResource));
                newBuild = streamReader.ReadLine(); //must be be 3 or 4 components (revision is optional)
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
                                   + versionNewBuild.ToString();
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

        /// <summary>destinationPath includes filename (Setup.exe).  destinationPath2 will create a second copy at the specified path/filename, or it will be skipped if null or empty.</summary>
        public static void DownloadInstallPatchFromURI(string downloadUri, string destinationPath, bool runSetupAfterDownload, bool showShutdownWindow, string destinationPath2)
        {
            // TODO: Implement me
        }
        
        ///<summary>Returns true if the eConnector service is already installed or was successfully installed. 
        ///Returns false if there are EConnectors present on other machines, or on any error.
        ///Uninstalls all OpenDentalCustListener services that are present. Installs the eConnector service after successfully removing all CustListener services.
        ///Set isSilent to false to show meaningful error messages, otherwise fails silently.
        ///Set doOverrideBlankUpdateServerName to true if this computer should be set as the new update server if the WebServiceServerName preference is currently blank.
        ///Set isInvalidUpdateServerNameAllowed to true if it is okay for the eConnector to be installed on a computer that is NOT the WebServiceServerName preference.</summary>
        public static bool UpgradeOrInstallEConnector(bool isSilent, string updateServerName = null, bool doOverrideBlankUpdateServerName = false,
            bool isInvalidUpdateServerNameAllowed = false)
        {
            if (updateServerName == null)
            {
                updateServerName = PrefC.GetString(PrefName.WebServiceServerName);
            }

            if (ODEnvironment.IsCloudServer)
            {
                //We do not want to install in case this is a pre-test cloud database.
                if (!isSilent)
                {
                    MessageBox.Show(Lans.g("ServicesHelper", "Not allowed to install the OpenDentalEConnector service in cloud mode."));
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
                            MessageBox.Show(Lans.g("ServicesHelper", "Failed to get host name:") + " " + ex.Message);
                        }

                        return false;
                    }
                }
            }
            else if (!ODEnvironment.IdIsThisComputer(updateServerName) && !isInvalidUpdateServerNameAllowed)
            {
                return false; //This is not an error and is simply not the correct computer that should have the eConnector installed on it.
            }

            bool hadCustListener = UninstallCustListenerServices();

            //Installing and starting a new eConnector service was successful at this point so we should always return true past this point.
            //Tell HQ that this registration key is now running the eConnector service.
            try
            {
                ListenerServiceType listenerServiceType = WebServiceMainHQProxy.SetEConnectorOn();
                string logText = Lan.g("PrefL", "eConnector status automatically set to") + " " + listenerServiceType.ToString() + ".";
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

        ///<summary>Returns true if a CustListener service was detected and successfully uninstalled. Otherwise; false.</summary>
        private static bool UninstallCustListenerServices()
        {
            bool hadCustListener = false;
            //Check to see if CustListener service is installed and uninstall any that are detected.
            List<ServiceController> listServiceControllersCustListener = new List<ServiceController>();
            ODException.SwallowAnyException(() => listServiceControllersCustListener = ServicesHelper.GetServicesByExe("OpenDentalCustListener.exe"));
            for (int i = 0; i < listServiceControllersCustListener.Count; i++)
            {
                //Attempts to uninstall the service and does not throw UEs if the uninstall failed.
                if (ServicesHelper.Uninstall(listServiceControllersCustListener[i]))
                {
                    hadCustListener = true;
                }
            }

            return hadCustListener;
        }

        /// <summary>Check for a developer only license</summary>
        public static bool IsRegKeyForTesting()
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.IndentChars = ("    ");
            StringBuilder stringBuilder = new StringBuilder();
            using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, xmlWriterSettings))
            {
                xmlWriter.WriteStartElement("RegistrationKey");
                xmlWriter.WriteString(PrefC.GetString(PrefName.RegistrationKey));
                xmlWriter.WriteEndElement();
            }

            try
            {
                string response = CustomerUpdatesProxy.GetWebServiceInstance().RequestIsDevKey(stringBuilder.ToString());
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(response);
                XmlNode xmlNode = xmlDocument.SelectSingleNode("//IsDevKey");
                return PIn.Bool(xmlNode.InnerText);
            }
            catch (Exception ex)
            {
                //They don't have an external internet connection.
                return false;
            }
        }
    }
}