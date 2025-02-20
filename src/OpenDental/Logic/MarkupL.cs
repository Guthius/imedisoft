using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using CodeBase;
using OpenDentBusiness;

namespace OpenDental;

///<summary>Used for wiki and HTML email editing.</summary>
public class MarkupL
{
    public static void AddTag(string tagStart, string tagClose, ODcodeBox codeBox)
    {
        var startSelection = codeBox.SelectionStart;
        var lengthSelection = codeBox.SelectionLength;
        var str = tagStart + codeBox.SelectedText + tagClose;
        codeBox.SelectedText = str;

        if (lengthSelection == 0)
        {
            codeBox.SelectionStart = startSelection + tagStart.Length + lengthSelection;
            return;
        }

        codeBox.SelectionStart = startSelection + str.Length;
        codeBox.SelectionLength = 0;
    }

    public static bool ValidateMarkup(ODcodeBox codeBox, bool isForSaving, bool showMsgBox = true, bool isEmail = false)
    {
        MatchCollection matchCollection;

        var str = codeBox.Text;

        str = str.Replace("&", "&amp;");
        str = str.Replace("&amp;<", "&lt;");
        str = str.Replace("&amp;>", "&gt;");
        str = "<body>" + str + "</body>";

        var xmlDocument = new XmlDocument();
        var stringReader = new StringReader(str);

        try
        {
            xmlDocument.Load(stringReader);
        }
        catch (Exception ex)
        {
            if (showMsgBox)
            {
                ODMessageBox.Show(ex.Message);
            }

            return false;
        }

        if (!isEmail)
        {
            try
            {
                MarkupEdit.ValidateNodes(xmlDocument.DocumentElement.ChildNodes);
            }
            catch (Exception ex)
            {
                if (showMsgBox)
                {
                    ODMessageBox.Show(ex.Message);
                }

                return false;
            }
        }

        var matchCollectionTags = Regex.Matches(codeBox.Text, "(?<!&)<.*?>", RegexOptions.Singleline);
        for (var i = 0; i < matchCollectionTags.Count; i++)
        {
            if (!matchCollectionTags[i].ToString().Contains("\n"))
            {
                continue;
            }

            if (showMsgBox)
            {
                ODMessageBox.Show(
                    "Error at line: " + codeBox.GetLineFromCharIndex(matchCollectionTags[i].Index) + " - " +
                    "Tag definitions cannot contain a return line: " + matchCollectionTags[i].Value.Replace("\n", ""));
            }

            return false;
        }

        if (isEmail)
        {
            var emailImagePath = "";
            try
            {
                emailImagePath = ImageStore.GetEmailImagePath();
            }
            catch
            {
                // ignored
            }

            matchCollection = Regex.Matches(codeBox.Text, @"\[\[(img:).*?\]\]");
            if (isForSaving)
            {
                for (var i = 0; i < matchCollection.Count; i++)
                {
                    var imageName = matchCollection[i].Value.Substring(6).Trim(']');
                    if (MiscUtils.IsValidHttpUri(imageName))
                    {
                        continue;
                    }

                    var imgPath = Path.Combine(emailImagePath, imageName);
                    if (File.Exists(imgPath))
                    {
                        continue;
                    }

                    if (showMsgBox)
                    {
                        ODMessageBox.Show("Error at line: " + codeBox.GetLineFromCharIndex(matchCollection[i].Index) + " - Not allowed to save because image does not exist:  " + imgPath);
                    }

                    return false;
                }
            }
        }

        var lines = codeBox.Text.Split(["\n"], StringSplitOptions.None);
        for (var i = 0; i < lines.Length; i++)
        {
            if (lines[i].Trim().StartsWith("*"))
            {
                if (!lines[i].StartsWith("*"))
                {
                    if (showMsgBox)
                    {
                        ODMessageBox.Show("Error at line: " + (i + 1) + " - Stars used for lists may not have a space before them.");
                    }

                    return false;
                }

                if (lines[i].Trim().StartsWith("* "))
                {
                    if (showMsgBox)
                    {
                        ODMessageBox.Show("Error at line: " + (i + 1) + " - Stars used for lists may not have a space after them.");
                    }

                    return false;
                }
            }

            if (!lines[i].Trim().StartsWith("#"))
            {
                continue;
            }

            if (!lines[i].StartsWith("#"))
            {
                if (showMsgBox)
                {
                    ODMessageBox.Show("Error at line: " + (i + 1) + " - Hashes used for lists may not have a space before them.");
                }

                return false;
            }

            if (!lines[i].Trim().StartsWith("# "))
            {
                continue;
            }

            if (showMsgBox)
            {
                ODMessageBox.Show("Error at line: " + (i + 1) + " - Hashes used for lists may not have a space after them.");
            }

            return false;
        }

        matchCollection = Regex.Matches(codeBox.Text, @"\[\[.*?\]\]");
        for (var m = 0; m < matchCollection.Count; m++)
        {
            if (matchCollection[m].Value.Contains("\"") && !matchCollection[m].Value.StartsWith("[[color:") && !matchCollection[m].Value.StartsWith("[[font:"))
            {
                if (showMsgBox)
                {
                    ODMessageBox.Show("Error at line: " + codeBox.GetLineFromCharIndex(matchCollection[m].Index) + " - Link cannot contain double quotes:" + " " + matchCollection[m].Value);
                }

                return false;
            }

            if (matchCollection[m].Value.StartsWith("[[img:") ||
                matchCollection[m].Value.StartsWith("[[keywords:") ||
                matchCollection[m].Value.StartsWith("[[file:") ||
                matchCollection[m].Value.StartsWith("[[folder:") ||
                matchCollection[m].Value.StartsWith("[[list:") ||
                matchCollection[m].Value.StartsWith("[[color:") ||
                matchCollection[m].Value.StartsWith("[[font:"))
            {
                continue;
            }

            if (!matchCollection[m].Value.Contains("|"))
            {
                continue;
            }

            if (showMsgBox)
            {
                ODMessageBox.Show("Error at line: " + codeBox.GetLineFromCharIndex(matchCollection[m].Index) + " - Internal link cannot contain a pipe character: " + matchCollection[m].Value);
            }

            return false;
        }

        matchCollection = Regex.Matches(str, @"\{\|\n.+?\n\|\}", RegexOptions.Singleline);
        for (var m = 0; m < matchCollection.Count; m++)
        {
            lines = matchCollection[m].Value.Split(["{|\n", "\n|-\n", "\n|}"], StringSplitOptions.RemoveEmptyEntries);
            if (!lines[0].StartsWith("!"))
            {
                if (showMsgBox)
                {
                    ODMessageBox.Show("Error at line: " + codeBox.GetLineFromCharIndex(matchCollection[m].Index) + " - The second line of a table markup section must start with ! to indicate column headers.");
                }

                return false;
            }

            if (lines[0].StartsWith("! "))
            {
                if (showMsgBox)
                {
                    ODMessageBox.Show("Error at line: " + codeBox.GetLineFromCharIndex(matchCollection[m].Index) + " - In the table, at line 2, there cannot be a space after the first !");
                }

                return false;
            }

            var cells = lines[0].Substring(1).Split(["!!"], StringSplitOptions.None);
            if (cells.Any(cell => !Regex.IsMatch(cell, """^(Width=")\d+"\|""")))
            {
                if (showMsgBox)
                {
                    ODMessageBox.Show("Error at line: " + codeBox.GetLineFromCharIndex(matchCollection[m].Index) + " - In the table markup, each header must be formatted like this: Width=\"#\"|...");
                }

                return false;
            }

            for (var i = 1; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("|"))
                {
                    continue;
                }

                if (showMsgBox)
                {
                    ODMessageBox.Show("Table rows must start with |.  At line " + (i + 1) + ", this was found instead:" + lines[i]);
                }

                return false;
            }
        }

        return true;
    }
}