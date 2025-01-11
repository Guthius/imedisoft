using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using ODCrypt;
using OpenDentBusiness.Properties;

namespace OpenDentBusiness;

public class Help
{
    public static string GetManualPage(string formName, bool isKeyValid)
    {
        if (CanSaveFormName())
        {
            ODClipboard.SetClipboard(formName);
        }

        if (!isKeyValid)
        {
            return "https://www.opendental.com/site/helpfeature.html";
        }
        
        var strXml = Resources.LinkWinForms;
        var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream);
        streamWriter.Write(strXml);
        streamWriter.Flush();
        memoryStream.Position = 0;
        using var streamReader = new StreamReader(memoryStream, Encoding.UTF8, true);
        var listHelpLinkWins = new List<HelpLinkWin>();
        var xmlSerializer = new XmlSerializer(listHelpLinkWins.GetType());
        listHelpLinkWins = (List<HelpLinkWin>) xmlSerializer.Deserialize(streamReader);
        var helpLinkWin = listHelpLinkWins.Find(x => x.FormName == formName);
        if (helpLinkWin is null)
        {
            return "https://www.opendental.com/site/helpfeature.html";
        }

        var topicName = helpLinkWin.TopicName;
        var listSplits = PrefC.GetString(PrefName.ProgramVersion).Split('.').ToList();
        var version3dig = listSplits[0] + listSplits[1];

        var url = "https://opendental.com/autoLogin.aspx?token=d83JWerd&redirect=help" + version3dig + "/" + topicName + ".html";
        return url;
    }
        
    private static bool CanSaveFormName()
    {
        try
        {
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsEncryptedKeyValid()
    {
        var keyPlainText = DecryptKey(PrefC.GetString(PrefName.HelpKey));
        try
        {
            return IsKeyValid(keyPlainText);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsKeyValid(string helpKeyDecrypted)
    {
        if (helpKeyDecrypted == "")
        {
            helpKeyDecrypted = UpdateHelpKey();
        }

        var arrayHelpKeyValues = helpKeyDecrypted.Split(',');
        var onSupport = SIn.Bool(arrayHelpKeyValues[1]);
        
        if (!DateTime.TryParse(arrayHelpKeyValues[0], new CultureInfo("en-US"), DateTimeStyles.None, out var dateTimeKey))
        {
            throw new ODException("Could not parse helpKey. Please try again or call support.");
        }

        if (onSupport)
        {
            if (DateTime.UtcNow < dateTimeKey.AddDays(7))
            {
                return true;
            }
        }

        var newHelpKey = UpdateHelpKey(); 
        arrayHelpKeyValues = newHelpKey.Split(',');
        return SIn.Bool(arrayHelpKeyValues[1]);
    }

    public static string UpdateHelpKey()
    {
        var officeData = PayloadHelper.CreatePayload("", eServiceCode.ODHelp);
        var helpKeyEncrypted = WebServiceMainHQProxy.GetWebServiceMainHQInstance().CreateNewHelpKey(officeData);
        if (!Prefs.UpdateString(PrefName.HelpKey, helpKeyEncrypted))
        {
            throw new ODException("Could not update HelpKey, try again. If the problem persists please call support.");
        }

        return DecryptKey(helpKeyEncrypted);
    }

    public static string DecryptKey(string helpKey)
    {
        var failText = "";
        
        if (!Encryption.DecryptString(helpKey, true, out var helpKeyPlainText, ref failText))
        {
            throw new ApplicationException(failText);
        }

        return helpKeyPlainText;
    }
}
    
[Serializable]
public class HelpLinkWin
{
    public string FormName;

    public string TopicName;
}