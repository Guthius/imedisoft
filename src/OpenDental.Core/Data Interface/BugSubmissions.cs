using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Imedisoft.Core.Features.Clinics;

namespace OpenDentBusiness;

public class BugSubmissions
{
    public static string GetDiagnostics(long patNum = -1)
    {
        var submissionInfo = new BugSubmission(new Exception(), patNum: patNum).Info;
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("-------------");
        stringBuilder.AppendLine("Connection settings");
        //Servername, database name, msq user and password
        var listFieldInfos = submissionInfo.GetType().GetFields().ToList();
        for (var i = 0; i < listFieldInfos.Count; i++)
        {
            var objectValue = listFieldInfos[i].GetValue(submissionInfo);
            if (objectValue.In(null, "")) continue;

            if (objectValue is Dictionary<PrefName, string>)
            {
                //DictPrefValues
                var dictionaryPrefValues = objectValue as Dictionary<PrefName, string>;
                if (dictionaryPrefValues.Keys.Count > 0)
                {
                    stringBuilder.AppendLine(listFieldInfos[i].Name + ":");
                    var listKeyValuePairsPrefVals = dictionaryPrefValues.ToList();
                    for (var j = 0; j < listKeyValuePairsPrefVals.Count; j++) stringBuilder.AppendLine(" " + listKeyValuePairsPrefVals[j].Key + " " + listKeyValuePairsPrefVals[j].Value);

                    stringBuilder.AppendLine("-------------");
                }
            }
            else if (objectValue is List<string>)
            {
                //EnabledPlugins
                var listEnabledPlugins = objectValue as List<string>;
                if (listEnabledPlugins.Count > 0)
                {
                    stringBuilder.AppendLine(listFieldInfos[i].Name + ":");
                    for (var j = 0; j < listEnabledPlugins.Count; j++) stringBuilder.AppendLine(" " + listEnabledPlugins[j]);

                    stringBuilder.AppendLine("-------------");
                }
            }
            else if (objectValue is bool)
            {
                stringBuilder.AppendLine(listFieldInfos[i].Name + ": " + ((bool) objectValue ? "true" : "false"));
            }
            else if (listFieldInfos[i].Name == "CountClinics")
            {
                var countTotalClinics = (int) objectValue;
                var countHiddenClinics = countTotalClinics - Clinics.GetCount(true);
                stringBuilder.AppendLine($"{listFieldInfos[i].Name}: {countTotalClinics} ({countHiddenClinics} hidden)");
            }
            else
            {
                stringBuilder.AppendLine(listFieldInfos[i].Name + ": " + objectValue);
            }
        }

        return stringBuilder.ToString();
    }

    public static void SubmitException(Exception ex, string threadName = "", long patNumCur = -1, string moduleName = "")
    {
        SubmitException(ex, out _, threadName, patNumCur, moduleName);
    }

    public static void SubmitException(Exception ex, out string displayMsg, string threadName = "", long patNumCur = -1, string moduleName = "")
    {
        displayMsg = null;
        
        if (!PrefC.GetBoolSilent(PrefName.SendUnhandledExceptionsToHQ, true)) return;

        var bugSubmission = new BugSubmission(ex, threadName, patNumCur, moduleName);
        string registrationKey = null;
        string practiceTitle = null;
        string practicePhone = null;
        string programVersion = null;
        var webServiceHqURL = "";

        ParseBugSubmissionResult(
            WebServiceMainHQProxy.GetWebServiceMainHQInstance(webServiceHqURL).SubmitUnhandledException(
                PayloadHelper.CreatePayload(
                    PayloadHelper.CreatePayloadContent(bugSubmission, "bugSubmission"), eServiceCode.BugSubmission, registrationKey, practiceTitle, practicePhone, programVersion
                )
            ), out displayMsg
        );
    }

    public static void ParseBugSubmissionResult(string result, out string displayMsg)
    {
        displayMsg = null;
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(result);
        if (xmlDocument.SelectSingleNode("//Error") != null) return;

        //A BugSubmission.Response object will get returned.
        var xmlNodeResponse = xmlDocument.SelectSingleNode("//SubmissionResult");
        if (xmlNodeResponse != null)
        {
            if (Enum.TryParse(xmlNodeResponse.InnerText, out BugSubmissionResult _))
            {
                displayMsg = xmlDocument.SelectSingleNode("//DisplayString")?.InnerText;
            }
        }
    }

    public static bool TryMatchPertinentFixedVersion(Version currentVersion, List<Version> listVersionsFixed, out Version versionPertinentFixed)
    {
        versionPertinentFixed = null;
        //Make sure that the current bug submission is at a version that is lower than all fixed versions.  Rare but it has happened.
        if (listVersionsFixed.All(x => x < currentVersion))
        {
            //Not a single fixed version is higher than the bug submission that just came in.  Guaranteed to be a new bug.
        }
        //Only versions of the same Major and Minor matter.
        else if (listVersionsFixed.Any(x => x.Major == currentVersion.Major && x.Minor == currentVersion.Minor && x.Build <= currentVersion.Build))
        {
            //There are fixed versions, they may or may not be pertinent to the bug submission version.
            //Only care about fixed versions that share the same Major version.  
            //E.g. a fixed version with a higher Major version doesn't necessarily mean this bug is fixed.  
            //subProgVersion = 17.4.32.0  and  listFixedVersions = { 16.8.23.0, 17.4.44.0, 18.1.1.0 }
            //As you can see, the presence of a "fix" being in 18.1.1.0 does not matter to the current bug submission.
            //The inverse can be applied to versions that are less than the current bug submission version.
            //Meaning, just because 16.8.23.0 has been flagged as "fixed", doesn't mean that all major versions above it are fixed as well.
        }
        else
        {
            //Fix is on more recent major version than what user is on.
            versionPertinentFixed = listVersionsFixed.Find(x => x > currentVersion);
        }

        return versionPertinentFixed != null;
    }

    public static string SimplifyStackTrace(string stackTrace)
    {
        return Regex.Replace(stackTrace.ToLower(), @" in [a-z0-9.\\: ]+\n", "\n"); //Case insensitive.
    }
}

public enum BugSubmissionResult;