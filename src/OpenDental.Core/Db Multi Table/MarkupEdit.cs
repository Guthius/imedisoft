using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class MarkupEdit
{
    private const string LanThis = "MarkupEdit";
    private const string HtmlTag = "(?<!&)<.+?(?<!&)>";

    public static void ValidateNodes(XmlNodeList nodes)
    {
        foreach (XmlNode node in nodes)
        {
            if (node.NodeType == XmlNodeType.Comment)
            {
                throw new ApplicationException("The comment tag <!-- --> " + node.Name + " is not allowed.");
            }

            if (node.NodeType == XmlNodeType.ProcessingInstruction)
            {
                throw new ApplicationException("The XML processing instruction <?xml ?> " + node.Name + " is not allowed.");
            }

            if (node.NodeType == XmlNodeType.XmlDeclaration)
            {
                throw new ApplicationException("XML declarations like <?xml ?> " + node.Name + "> are not allowed.");
            }

            if (node.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            //check child nodes for nested duplicate
            switch (node.Name)
            {
                case "i":
                case "b":
                case "h1":
                case "h2":
                case "h3":
                    //These are all valid nodes that are allowed.
                    break;
                case "div":
                    //The only thing div is used for right now is to designate bookmarks within the page.
                    //Therefore we require that there be one and only one attribute and that attribute can only be "id".
                    if (node.Attributes.Count != 1 || node.Attributes[0].Name != "id")
                    {
                        throw new ApplicationException(Lans.g(LanThis, "All <div> tags MUST be identified by the 'id' attribute."));
                    }

                    break;
                case "a":
                    //a is an allowed node but can only have one attribute; href
                    for (int i = 0; i < node.Attributes.Count; i++)
                    {
                        if (node.Attributes[i].Name != "href")
                        {
                            throw new ApplicationException(node.Attributes[i].Name + " attribute is not allowed on <a> tag.");
                        }

                        //We know the only attribute is "href", make sure the user didn't manually type out a "wiki" link using <a>.  They need to use [[ ]].
                        if (node.Attributes[i].InnerText.StartsWith("wiki:"))
                        {
                            throw new ApplicationException("wiki: is not allowed in an <a> tag.  Use [[ ]] instead of <a>.");
                        }
                    }

                    break;
                case "img":
                    throw new ApplicationException("Image tags are not allowed. Instead use [[img: ... ]]");
                default:
                    throw new ApplicationException("<" + node.Name + "> is not one of the allowed tags. To display as plain text, escape the brackets with ampersands. I.e. \"&<" + node.Name + "&>\"");
            }

            ValidateNodes(node.ChildNodes);
            ValidateDuplicateNesting(node.Name, node.ChildNodes);
        }
    }

    public static void ValidateDuplicateNesting(string nodeName, XmlNodeList nodes)
    {
        foreach (XmlNode node in nodes)
        {
            if (node.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            if (nodeName == node.Name)
            {
                throw new ApplicationException("There are multiple <" + node.Name + "> tags nested within each other.  Remove the unneeded tags.");
            }

            ValidateDuplicateNesting(nodeName, node.ChildNodes);
        }
    }

    public static bool ContainsOdHtmlTags(string text)
    {
        Regex tagRegex = new Regex(@"<\s*([^ >]+)[^>]*>.*?<\s*/\s*\1\s*>");
        return tagRegex.IsMatch(text ?? "");
    }

    public static string TranslateToXhtml(string markupText, bool isEmail = false, bool canAggregate = true, float scale = 1)
    {
        #region Basic Xml Validation

        string s = markupText;
        MatchCollection matches;
        //"<",">", and "&"-----------------------------------------------------------------------------------------------------------
        s = s.Replace("&", "&amp;");
        s = s.Replace("&amp;<", "&lt;"); //because "&" was changed to "&amp;" in the line above.
        s = s.Replace("&amp;>", "&gt;"); //because "&" was changed to "&amp;" in the line above.
        s = "<body>" + s + "</body>";
        XmlDocument doc = new XmlDocument();
        using (StringReader reader = new StringReader(s))
        {
            doc.Load(reader);
        }

        #endregion

        #region Validate brackets

        //Looks for replacement strings within markupText in order to find missing closing brackets. Ex: "[[color:... [[img:...]]", "[[file:...", "...[[".
        //This handles allowing nested bracketing implementation in the possible future. Ex: [[font-family:...[[color:...]] ...]]".
        int index = 0;
        //Keep track of the start index for all replacement strings that are not 'closed' correctly. Ex: "abc[[def" will have '3' as the only value in the stack.
        Stack<int> stackUnclosedIndices = new Stack<int>();
        while (index + 1 < markupText.Length)
        {
            string str = markupText[index].ToString() + markupText[index + 1].ToString();
            if (str == "[[")
            {
                //If we find double-open brackets, we store the start location in case it is missing closing brackets.
                stackUnclosedIndices.Push(index);
                index += 2; //We can skip over the double-open brackets since they are not in concern anymore.
                continue;
            }
            else if (str == "]]")
            {
                //If we find double-close brackets, we remove the most recent location stored since they are a pair.
                if (stackUnclosedIndices.Count > 0)
                {
                    stackUnclosedIndices.Pop();
                }

                index += 2;
                continue;
            }

            index += 1;
        }

        int end = markupText.Length;
        List<string> listInvalidMarkupStrings = new List<string>();
        while (stackUnclosedIndices.Count > 0)
        {
            int start = stackUnclosedIndices.Pop();
            int length = end - start;
            string invalidMarkupString = markupText.Substring(start, length);
            if (invalidMarkupString.Length > 50)
            {
                invalidMarkupString = invalidMarkupString.Substring(0, 50) + "...";
            }

            listInvalidMarkupStrings.Add(invalidMarkupString);
            end = start;
        }

        index = 0;
        string errorMessage = "";
        for (int i = listInvalidMarkupStrings.Count - 1; i >= 0; i--)
        {
            //Display invalid replacement strings found chronologically.
            index += 1;
            errorMessage += "\r\n" + index + ": \"" + listInvalidMarkupStrings[i] + "\"";
        }

        if (!String.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ApplicationException(Lans.g("WikiPages", "Invalid markup syntax detected. The following have unclosed brackets:") + errorMessage);
        }

        #endregion

        #region regex replacements

        //Unordered List----------------------------------------------------------------------------------------------------------------
        //Instead of using a regex, this will hunt through the rows in sequence.
        //later nesting by running ***, then **, then *
        s = ProcessList(s, "*");
        //numbered list---------------------------------------------------------------------------------------------------------------------
        s = ProcessList(s, "#");

        #endregion regex replacements

        #region paragraph grouping

        StringBuilder strbSnew = new StringBuilder();
        //a paragraph is defined as all text between sibling tags, even if just a \n.
        int iScanInParagraph = 0; //scan starting at the beginning of s.  S gets chopped from the start each time we grab a paragraph or a sibiling element.
        //The scanning position represents the verified paragraph content, and does not advance beyond that.
        //move <body> tag over.
        strbSnew.Append("<body>");
        s = s.Substring(6);
        bool startsWithCR = false; //todo: handle one leading CR if there is no text preceding it.
        if (s.StartsWith("\n"))
        {
            startsWithCR = true;
        }

        string tagName;
        Match tagCurMatch;
        while (true)
        {
            //loop to either construct a paragraph, or to immediately add the next tag to strbSnew.
            iScanInParagraph = s.IndexOf("<", iScanInParagraph); //Advance the scanner to the start of the next tag
            if (iScanInParagraph == -1)
            {
                //there aren't any more tags, so current paragraph goes to end of string.  This won't happen
                throw new ApplicationException(Lans.g("WikiPages", "No tags found."));
                //strbSnew.Append(ProcessParagraph(s));
            }

            if (s.Substring(iScanInParagraph).StartsWith("</body>"))
            {
                strbSnew.Append(ProcessParagraph(s.Substring(0, iScanInParagraph), startsWithCR));
                //startsWithCR=false;
                //strbSnew.Append("</body>");
                s = "";
                iScanInParagraph = 0;
                break;
            }

            tagName = "";
            tagCurMatch = Regex.Match(s.Substring(iScanInParagraph), "^<.*?>"); //regMatch);//.*? means any char, zero or more, as few as possible
            if (tagCurMatch == null)
            {
                //shouldn't happen unless closing bracket is missing
                throw new ApplicationException(Lans.g("WikiPages", "Unexpected tag:") + " " + s.Substring(iScanInParagraph));
            }

            if (tagCurMatch.Value.Trim('<', '>').EndsWith("/"))
            {
                //self terminating tags NOT are allowed
                //this should catch all non-allowed self-terminating tags i.e. <br />, <inherits />, etc...
                throw new ApplicationException(Lans.g("WikiPages", "All elements must have a beginning and ending tag. Unexpected tag:") + " " + s.Substring(iScanInParagraph));
            }

            //Nesting of identical tags causes problems: 
            //<h1><h1>some text</h1></h1>
            //The first <h1> will match with the first </h1>.
            //We don't have time to support this outlier, so we will catch it in the validator when they save.
            //One possible strategy here might be:
            //idxNestedDuplicate=s.IndexOf("<"+tagName+">");
            //if(idxNestedDuplicate<s.IndexOf("</"+tagName+">"){
            //
            //}
            //Another possible strategy might be to use regular expressions.
            tagName = tagCurMatch.Value.Split(new string[] {"<", " ", ">"}, StringSplitOptions.RemoveEmptyEntries)[0]; //works with tags like <i>, <span ...>, and <img .../>
            if (s.IndexOf("</" + tagName + ">") == -1)
            {
                //this will happen if no ending tag.
                throw new ApplicationException(Lans.g("WikiPages", "No ending tag:") + " " + s.Substring(iScanInParagraph));
            }

            switch (tagName)
            {
                case "a":
                case "b":
                case "div":
                case "i":
                case "span":
                    iScanInParagraph = s.IndexOf("</" + tagName + ">", iScanInParagraph) + 3 + tagName.Length;
                    continue; //continues scanning this paragraph.
                case "h1":
                case "h2":
                case "h3":
                case "ol":
                case "ul":
                case "table":
                case "img": //can NOT be self-terminating
                    if (iScanInParagraph == 0)
                    {
                        //s starts with a non-paragraph tag, so there is no partially assembled paragraph to process.
                        //do nothing
                    }
                    else
                    {
                        //we are already part way into assembling a paragraph.  
                        strbSnew.Append(ProcessParagraph(s.Substring(0, iScanInParagraph), startsWithCR));
                        startsWithCR = false; //subsequent paragraphs will not need this
                        s = s.Substring(iScanInParagraph); //chop off start of s
                        iScanInParagraph = 0;
                    }

                    //scan to the end of this element
                    int iScanSibling = s.IndexOf("</" + tagName + ">") + 3 + tagName.Length;
                    //tags without a closing tag were caught above.
                    //move the non-paragraph content over to s new.
                    if (tagName == "img")
                    {
                        //wrap in <p> tags so IE prints properly and rotated images do not overlap other content
                        strbSnew.Append(ProcessParagraph(s.Substring(0, iScanSibling), startsWithCR));
                    }
                    else
                    {
                        strbSnew.Append(s.Substring(0, iScanSibling));
                    }

                    s = s.Substring(iScanSibling);
                    //scanning will start a totally new paragraph
                    break;
                default:
                    if (isEmail)
                    {
                        iScanInParagraph = s.IndexOf("</" + tagName + ">", iScanInParagraph) + 3 + tagName.Length;
                        continue; //continues scanning this paragraph
                    }

                    throw new ApplicationException(Lans.g("WikiPages", "Unexpected tag:") + " " + s.Substring(iScanInParagraph));
            }
        }

        strbSnew.Append("</body>");

        #endregion

        #region aggregation

        doc = new XmlDocument();
        using (StringReader reader = new StringReader(strbSnew.ToString()))
        {
            doc.Load(reader);
        }

        StringBuilder strbOut = new StringBuilder();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Indent = true;
        settings.IndentChars = "\t";
        settings.OmitXmlDeclaration = true;
        settings.NewLineChars = "\n";
        using (XmlWriter writer = XmlWriter.Create(strbOut, settings))
        {
            doc.WriteTo(writer);
        }

        //spaces can't be handled prior to this point because &nbsp; crashes the xml parser.
        strbOut.Replace("  ", "&nbsp;&nbsp;"); //handle extra spaces. 
        strbOut.Replace("<td></td>", "<td>&nbsp;</td>"); //force blank table cells to show not collapsed
        strbOut.Replace("<th></th>", "<th>&nbsp;</th>"); //and blank table headers
        strbOut.Replace("{{nbsp}}", "&nbsp;"); //couldn't add the &nbsp; earlier because 
        strbOut.Replace("<p></p>", "<p>&nbsp;</p>"); //probably redundant but harmless
        //aggregate with master
        if (isEmail)
        {
            if (canAggregate)
            {
                s = PrefC.GetString(PrefName.EmailMasterTemplate).Replace("@@@body@@@", strbOut.ToString());
                return s;
            }

            return strbOut.ToString();
        }

        #endregion aggregation

        #region scaling

        if (scale == 1)
        {
            //Do nothing
        }
        else
        {
            //Adjust the font size and table column widths of the wiki to account for any "Zoom" changes.
            string fontTextRegexPattern = @"font-size:\s*\d+\.?\d?pt"; //To find each font size text
            string fontNumRegexPattern = @"\d+\.?\d?"; //To find only the font size itself
            MatchCollection matchCollection = Regex.Matches(s, fontTextRegexPattern);
            for (int i = matchCollection.Count - 1; i >= 0; i--)
            {
                //Walk through pageContent backwards to correctly rebuild the string 
                Match matchFontNumOnly = Regex.Match(matchCollection[i].Value, fontNumRegexPattern); //Find the font value itself
                string[] arrayFontText = Regex.Split(matchCollection[i].Value, fontNumRegexPattern); //Separate the other text around the font value
                string fontNumUpdate = Convert.ToString(Math.Round(scale * SIn.Float(matchFontNumOnly.Value), 1)); //Adjust the font value to the nearest tenth
                string fontTextUpdate = arrayFontText[0] + fontNumUpdate + arrayFontText[1]; //Rebuild the font text with the updated font value
                s = s.Substring(0, matchCollection[i].Index) + fontTextUpdate + s.Substring(matchCollection[i].Index + matchCollection[i].Length); //Rebuild pageContent css
            }

            string colTextRegexPattern = @"t(h|d)\sWidth=""\d+"""; //To find each col size text
            string colNumRegexPattern = @"\d+"; //To find only the col width itself
            matchCollection = Regex.Matches(s, colTextRegexPattern);
            for (int i = matchCollection.Count - 1; i >= 0; i--)
            {
                //Walk through pageContent backwards to correctly rebuild the string 
                Match matchColNumOnly = Regex.Match(matchCollection[i].Value, colNumRegexPattern); //Find the col value itself
                string[] arrayColText = Regex.Split(matchCollection[i].Value, colNumRegexPattern); //Separate the other text around the col value
                string colNumUpdate = Convert.ToString(Math.Round(scale * SIn.Float(matchColNumOnly.Value), 1)); //Adjust the col value to the nearest tenth
                string colTextUpdate = arrayColText[0] + colNumUpdate + arrayColText[1]; //Rebuild the col text with the updated col value
                s = s.Substring(0, matchCollection[i].Index) + colTextUpdate + s.Substring(matchCollection[i].Index + matchCollection[i].Length); //Rebuild pageContent body
            }
        }

        #endregion scaling

        return s;
    }

    public static string ConvertMarkupToPlainText(string rawText)
    {
        StringBuilder strb = new StringBuilder(rawText);
        //strip image
        StringTools.RegReplace(strb, @"\[\[img:(?=[^\[\]]*?\]\])|(?<=\[\[img:[^\[\]]*?)\]\]", "");
        //strip font
        StringTools.RegReplace(strb, @"\[\[font:[^\[\]]*?\|(?=[^\[\]]*?\]\])|(?<=\[\[font:[^\[\]]*?\|[^\]]*?)\]\]", "");
        //strip color
        StringTools.RegReplace(strb, @"\[\[color:[^\[\]]*?\|(?=[^\[\]]*?\]\])|(?<=\[\[color:[^\[\]]*?\|[^\]]*?)\]\]", "");
        //strip table
        //Remove headers
        StringTools.RegReplace(strb, @"(?<={\|.*?)!.*?\|(?=.*?\|})", " ", RegexOptions.Singleline);
        //Remove row start
        StringTools.RegReplace(strb, @"(?<={\|.*?)\n\|-(?=.*?\|})|(?<={\|.*?\n)\|(?=.*?\|})", " ", RegexOptions.Singleline);
        //Remove row innards
        StringTools.RegReplace(strb, @"(?<={\|.*?)\|\|(?=.*?\|})", " ", RegexOptions.Singleline);
        //Remove table beginning / end
        StringTools.RegReplace(strb, @"{\|(?=.*?\|})|(?<={\|.*?)\|}", "", RegexOptions.Singleline);
        //The regex pattern below will match anything enclosed within "<" and ">". However for our wiki pages, we use "&" as an escape character
        //for "<" and ">", so we do not want to match "&<" or "&>". We know that this will not perfectly parse all HTML tags, but it is good enough
        //to use for searching.
        StringTools.RegReplace(strb, HtmlTag, "");
        return strb.ToString();
    }

    public static string ConvertToPlainText(string rawWikipageText)
    {
        StringBuilder strb = new StringBuilder(rawWikipageText);
        //The regex pattern below will match anything enclosed within "<" and ">". However for our wiki pages, we use "&" as an escape character
        //for "<" and ">", so we do not want to match "&<" or "&>". We know that this will not perfectly parse all HTML tags, but it is good enough
        //to use for searching.
        StringTools.RegReplace(strb, HtmlTag, "");
        //We also want to remove links to other wiki pages. We can assume that no internal links with pipes exist, since that is forbidden.
        StringTools.RegReplace(strb, @"\[\[[^|]+?\]\]", "");
        //Matches a pair of opening brackets with the first following pair of closing brackets, and everything between.
        //This will capture everything between the first pipe and the first pair of closing brackets.
        //Replaces the whole match with the captured text, aka the tag's content.
        StringTools.RegReplace(strb, @"\[\[.*?\|(.*?)\]\]", "$1", RegexOptions.Singleline);
        return strb.ToString();
    }

    public static string ProcessList(string s, string prefixChars)
    {
        string listTag = "";
        string otherPrefixChar = "";
        if (prefixChars == "#")
        {
            listTag = "ol";
            otherPrefixChar = "*";
        }
        else if (prefixChars == "*")
        {
            listTag = "ul";
            otherPrefixChar = "#";
        }

        string[] lines = s.Split("\n", StringSplitOptions.None); //includes empty elements
        bool isWithinListTag = false; //Keep track of when we enter a list tag and have yet to close it.
        for (int i = 0; i < lines.Length; i++)
        {
            if (!lines[i].Contains(prefixChars))
            {
                continue;
            }

            lines[i] = lines[i].Replace("\r", "");
            //Exactly matches the format of a table row that has been processed most of the way at this point. Each set of parenthesis is a different match group that will be used below for processing prefixchars into ul/ol and li tags. Example: <td Width="100"><p>*1<br/>*2</p></td>
            string patternListsInTable = @"<td Width=""\d+"">(<p>(.+)</p>)</td>";
            StringBuilder stringBuilder = new StringBuilder();
            Match match = Regex.Match(lines[i], patternListsInTable);
            if (match.Success)
            {
                //There are list(s) present in table(s)
                //Groups[2] represents the outermost set of parenthesis in the regex above.
                //Example: In a table row like: <td Width="100"><p>*1<br/>*2</p></td>
                //Groups[2] refers to the contents ofthe opening and closing <td> tags, namely <p>*1<br/>*2</p>
                string strCellContent = match.Groups[2].Value.Replace("<br/>", "\n"); //Newlines are needed for the recursive calls below.
                //Recursively process the content of this table cell.
                strCellContent = ProcessList(strCellContent, prefixChars);
                if (strCellContent.Contains(otherPrefixChar))
                {
                    strCellContent = ProcessList(strCellContent, otherPrefixChar);
                }

                strCellContent = strCellContent.Replace("\n", "<br/>"); //But back the <br/>s we removed for the recursion above.
                //We will now have too many <br/>s since there is an implicit <br/> between <li> tags.
                //Reduce those groupings by only 1 so that any intentional formatting is preserved.
                strCellContent = ReduceTagGroupingsByOne(strCellContent, "<br/>");
                lines[i] = lines[i].Replace(match.Groups[2].Value, strCellContent);
            }
            else
            {
                //List(s) are present outside of tables
                string line = lines[i];
                //At this point in the markup processing there will be some other tags present in the text we're parsing.
                //The only tags that will cause errors are <body> tags. Trim them off and add them back after we have wrapped the content in li tags.
                bool addEndBodyTag = false;
                bool addStartBodyTag = false;
                if (line.StartsWith("<body>"))
                {
                    line = line.Substring("<body>".Length);
                    addStartBodyTag = true;
                }

                if (line.EndsWith("</body>"))
                {
                    line = line.Substring(0, line.Length - "</body>".Length);
                    addEndBodyTag = true;
                }

                if (!line.StartsWith(prefixChars))
                {
                    continue; //This is not a list and simply contains a # or * E.g. Math is #fun.
                }

                line = line.Substring(prefixChars.Length); //Trim off the prefix chars
                //There is CSS code in our master template that does formatting things on the ListItemContent class specifically.
                line = "<li><span class=\"ListItemContent\">" + line + "</span></li>";
                //Add the approriate ol/ul tag if this is the beginning of a list (the previous line is not a list item).
                if (i == 0 || (i > 0 && !isWithinListTag))
                {
                    line = $"<{listTag}>{line}";
                    isWithinListTag = true;
                }

                //Add the approriate closing ol/ul tag if this is the end of a list (the next line is not a list item).
                if (i == lines.Length - 1 || !lines[i + 1].StartsWith(prefixChars))
                {
                    line = $"{line}</{listTag}>";
                    isWithinListTag = false;
                }

                //Add in the body tags if we removed them
                if (addStartBodyTag)
                {
                    line = "<body>" + line;
                }

                if (addEndBodyTag)
                {
                    line = line + "</body>";
                }

                lines[i] = lines[i].Replace(lines[i], line);
            }
        }

        return string.Join("\n", lines);
    }

    public static string ReduceTagGroupingsByOne(string content, string tag)
    {
        if (string.IsNullOrEmpty(content)
            || !content.Contains(tag)
            || (!content.Contains("<ol>") && !content.Contains("<ul>") && !content.Contains("</ol>") && !content.Contains("</ul>")))
        {
            return content;
        }

        //It is possible for the passed in content to contain text that is not bounded by list tags in some way.
        //This text needs to be excluded from any tag replacing so that whatever format the text has is preserved.
        //Reducing tag groupings directly applies to tags within HTML lists (between ol, ul, or li elements).
        int startIndex = content.IndexOf("<ol>");
        if (content.IndexOf("<ul>") > -1 &&
            (startIndex == -1 || content.IndexOf("<ul>") < startIndex))
        {
            startIndex = content.IndexOf("<ul>");
        }

        int endIndex = content.LastIndexOf("</ol>");
        if (content.LastIndexOf("</ul>") > -1 &&
            content.LastIndexOf("</ul>") > endIndex)
        {
            endIndex = content.LastIndexOf("</ul>");
        }

        string contentInList = content.Substring(startIndex, endIndex + 5 - startIndex); //+5 to account for the length of a closing </ol> or </ul> tag
        //Split the list content by tag. We will be looking for empty entries between tags, and replacing the count of tag with the count of empty entries.
        //This effectively reduces tag count by 1 for all tag groupings.
        //Example: <br><br> split by <br> => "" which is 1 less than the number of <br> tags present.
        string[] strArrayContents = contentInList.Split(tag, StringSplitOptions.None);
        for (int i = 0; i < strArrayContents.Length; i++)
        {
            if (strArrayContents[i].IsNullOrEmpty())
            {
                strArrayContents[i] = tag; //Replace the tags that were removed by the split above, so that the count of tags will be reduced.
            }
        }

        content = content.Replace(contentInList, string.Join("", strArrayContents));
        return content;
    }

    private static string ProcessParagraph(string paragraph, bool startsWithCr, bool removeTrailingCr = true)
    {
        if (paragraph.StartsWith("\n") && !startsWithCr)
        {
            paragraph = paragraph.Substring(1);
        }

        if (paragraph == "")
        {
            //this must come after the first CR is stripped off, but before the ending CR is stripped off.
            return "";
        }

        if (paragraph.EndsWith("\n") && removeTrailingCr)
        {
            //trailing CR remove
            paragraph = paragraph.Substring(0, paragraph.Length - 1);
        }

        string strP = "";
        //images rotated 90 and 270 degrees need their paragraph to be sized correctly
        if (paragraph.StartsWith("<img") && (paragraph.Contains("transform:rotate(90") || paragraph.Contains("transform:rotate(270")))
        {
            strP += "<p";
            strP += " style=\"";
            List<string> listHeightAndWidth = paragraph.Split(' ').Where(x => x.StartsWith("height") || x.StartsWith("width")).ToList();
            for (int i = 0; i < listHeightAndWidth.Count; i++)
            {
                listHeightAndWidth[i] = listHeightAndWidth[i].Replace('=', ':').Replace("\"", "");
                //width and height will swap so paragraph is the right dimensions to hold the rotated image
                if (listHeightAndWidth[i].StartsWith("height"))
                {
                    listHeightAndWidth[i] = listHeightAndWidth[i].Replace("height", "width");
                }
                else
                {
                    listHeightAndWidth[i] = listHeightAndWidth[i].Replace("width", "height");
                }

                strP += listHeightAndWidth[i] + ";";
            }

            strP += "\">";
        }
        else
        {
            strP += "<p>";
        }

        //if the paragraph starts with any number of spaces followed by a tag such as <b> or <span>, then we need to force those spaces to show.
        if (paragraph.StartsWith(" ") && paragraph.TrimStart(' ').StartsWith("<"))
        {
            paragraph = "{{nbsp}}" + paragraph.Substring(1); //this will later be converted to &nbsp;
        }

        paragraph = paragraph.Replace("\n", "<br/>"); //We tried </p><p>, but that didn't allow bold, italic, or color to span lines.
        paragraph = strP + paragraph + "</p>"; //surround paragraph with tags
        paragraph = paragraph.Replace("<p> ", "<p>{{nbsp}}"); //spaces at the beginnings of paragraphs
        paragraph = paragraph.Replace("<br/> ", "<br/>{{nbsp}}"); //spaces at beginnings of lines
        paragraph = paragraph.Replace("<br/></p>", "<br/>{{nbsp}}</p>"); //have a cr show if it's at the end of a paragraph
        return paragraph;
    }
}