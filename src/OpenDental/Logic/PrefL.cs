using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

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
}