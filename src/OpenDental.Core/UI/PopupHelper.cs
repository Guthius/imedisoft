using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CodeBase;
using DataConnectionBase;

namespace OpenDentBusiness.UI;

public class PopupHelper
{
    public static List<MenuItem> GetContextMenuItemLinks(string contextMenuItemText, bool rightClickLinks)
    {
        var menuItems = new List<MenuItem>();

        var matches = GetURLsFromText(contextMenuItemText);
        foreach (var match in matches)
        {
            var title = match;
            if (title.Length > 24)
            {
                title = title.Substring(0, 24) + "...";
            }

            menuItems.Add(new MenuItem("Web - " + title, (_, _) => OpenWebPage(match)));
        }

        matches = ODFileUtils.GetFilePathsFromText(contextMenuItemText);
        foreach (var match in matches)
        {
            menuItems.Add(new MenuItem("File Explorer - " + match, (_, _) => OpenUncPath(match)));
        }

        if (!rightClickLinks)
        {
            return menuItems
                .OrderByDescending(x => x.Text == "-")
                .ThenBy(x => x.Text)
                .ToList();
        }
        
        var patNums = GetPatNumsFromText(contextMenuItemText);
        foreach (var patNum in patNums)
        {
            menuItems.Add(new MenuItem("PatNum - " + patNum, (_, _) => OpenPatNum(patNum)));
        }

        var taskNums = GetTaskNumsFromText(contextMenuItemText);
        foreach (var taskNum in taskNums)
        {
            menuItems.Add(new MenuItem("TaskNum - " + taskNum, (_, _) => OpenTaskNum(taskNum)));
        }

        return menuItems
            .OrderByDescending(x => x.Text == "-")
            .ThenBy(x => x.Text)
            .ToList();
    }

    public static List<string> GetURLsFromText(string text)
    {
        //Regular expresion used to help identify URLs. This is not all encompassing.
        //There will be URLs that do not match this but this should work for 99%.
        //The url regex is generous enough to match urls fine and excludes emails well, but matches some files too.
        //These files get cleaned out though.
        var urlPattern = @"(?<!@)\b(?:https?:\/\/)?(?:www\.)?(?:[a-zA-Z0-9-]+\.)+[a-zA-Z]{2,4}(?:(?:\/|:)[^\s]*)?\b(?!(?:\\))";
        var listStringMatches = Regex.Matches(text, urlPattern)
            .OfType<Match>()
            .Select(m => m.Groups[0].Value)
            .Distinct()
            .ToList();
        for (var i = listStringMatches.Count - 1; i >= 0; i--)
        {
            if (listStringMatches[i].StartsWith("(") && listStringMatches[i].EndsWith(")"))
            {
                listStringMatches[i] = listStringMatches[i].Substring(1, listStringMatches[i].Length - 2);
            }

            listStringMatches[i] = listStringMatches[i].TrimEnd('.');
            var rgx = new Regex(@"[\\]{1}");
            if (rgx.IsMatch(listStringMatches[i]))
            {
                listStringMatches.RemoveAt(i);
                continue;
            }
        }

        return listStringMatches;
    }

    public static List<long> GetPatNumsFromText(string text)
    {
        //If this Regex pattern is ever changed, we may need to change the Select statement below.
        var strPatNum = "patnum:";
        var listNumMatches = Regex.Matches(text, $@"{strPatNum}\d+", RegexOptions.IgnoreCase)
            .OfType<Match>()
            .Select(x => SIn.Long(x.Groups[0].Value.Substring(strPatNum.Length), false)) //Get pat num out of text.
            .Distinct()
            .ToList();
        return listNumMatches;
    }

    public static List<long> GetTaskNumsFromText(string text)
    {
        //If this Regex pattern is ever changed, we may need to change the Select statement below.
        var strTaskNum = "tasknum:";
        var listNumMatches = Regex.Matches(text, $@"{strTaskNum}\d+", RegexOptions.IgnoreCase)
            .OfType<Match>()
            .Select(x => SIn.Long(x.Groups[0].Value.Substring(strTaskNum.Length), false)) //Get task num out of text.
            .Distinct()
            .ToList();
        return listNumMatches;
    }
    
    private static void OpenPatNum(long patNum)
    {
        var patient = Patients.GetPat(patNum);

        if (patient is null)
        {
            MessageBox.Show("Patient does not exist.");
            return;
        }

        GlobalFormOpenDental.PatientSelected(patient, true);
    }

    private static void OpenTaskNum(long taskNum)
    {
        Tasks.NavTaskDelegate?.Invoke(taskNum);
    }

    private static void OpenWebPage(string url)
    {
        try
        {
            if (!url.ToLower().StartsWith("http"))
            {
                url = @"https://" + url;
            }

            Process.Start(url);
        }
        catch
        {
            MessageBox.Show("Failed to open web browser. Please make sure you have a default browser set and are connected to the internet then try again.", "Attention");
        }
    }

    private static void OpenUncPath(string folderPath)
    {
        var isValidPath = Directory.Exists(folderPath);
        if (isValidPath)
        {
            try
            {
                Process.Start(folderPath);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
        else
        {
            MessageBox.Show("Failed to open file location. Please make sure file path is valid.");
        }
    }
}