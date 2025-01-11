using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class WikiPages
{
    ///<summary>Defines delegate signature to be used for WikiPages.NavPageDelegate.</summary>
    public delegate void NavToPageDelegate(string pageTitle);

    /// <summary>
    ///     Sent in from FormOpendental. Allows static method for business layer to cause wikipage navigation in
    ///     FormOpendental.
    /// </summary>
    public static NavToPageDelegate NavPageDelegate;

    public static WikiPage GetWikiPage(long wikiPageNum)
    {
        var wikiPage = GetWikiPages(new List<long> {wikiPageNum}).FirstOrDefault();
        return wikiPage;
    }

    ///<summary>Returns a list of non draft wiki pages.</summary>
    public static List<WikiPage> GetWikiPages(List<long> listWikiPageNums)
    {
        if (listWikiPageNums == null || listWikiPageNums.Count == 0) return new List<WikiPage>();

        var command = "SELECT * FROM wikipage  "
                      + "WHERE IsDraft=0 "
                      + "AND WikiPageNum IN (" + string.Join(",", listWikiPageNums.Select(x => SOut.Long(x))) + ")";
        return WikiPageCrud.SelectMany(command);
    }

    ///<summary>Returns null if page does not exist. Does not return drafts.</summary>
    public static WikiPage GetByTitle(string pageTitle, bool isDeleted = false)
    {
        var command = "SELECT * FROM wikipage "
                      + "WHERE PageTitle='" + SOut.String(pageTitle) + "' "
                      + "AND IsDraft=0 "
                      + "AND IsDeleted=" + SOut.Bool(isDeleted);
        return WikiPageCrud.SelectOne(command);
    }

    ///<summary>Returns empty list if page does not exist.</summary>
    public static List<WikiPage> GetDraftsByTitle(string pageTitle)
    {
        var command = "SELECT * FROM wikipage WHERE PageTitle='" + SOut.String(pageTitle) + "' AND IsDraft=1";
        return WikiPageCrud.SelectMany(command);
    }

    public static void WikiPageRestore(WikiPage wikiPageRestored, long userNum)
    {
        //Update the wikipage with new user and flip the IsDelete flag.
        wikiPageRestored.IsDeleted = false;
        wikiPageRestored.UserNum = userNum;
        wikiPageRestored.DateTimeSaved = MiscData.GetNowDateTime();
        WikiPageCrud.Update(wikiPageRestored);
    }

    /// <summary>
    ///     Archives first by moving to WikiPageHist if it already exists.  Then, in either case, it inserts the new page.
    ///     Does not delete drafts.
    /// </summary>
    public static long InsertAndArchive(WikiPage wikiPage)
    {
        wikiPage.PageContentPlainText = MarkupEdit.ConvertToPlainText(wikiPage.PageContent);
        wikiPage.IsDraft = false;
        var wikiPageExisting = GetByTitle(wikiPage.PageTitle);
        if (wikiPageExisting != null)
        {
            var wikiPageHist = PageToHist(wikiPageExisting);
            WikiPageHists.Insert(wikiPageHist);
            wikiPage.DateTimeSaved = MiscData.GetNowDateTime();
            //Old behavior was to delete the wiki page and then always insert.
            //It was changed to Update here for storing wiki page references by WikiPageNum instead of PageTitle
            //See JobNum 4429 for additional information.
            WikiPageCrud.Update(wikiPage);
            return wikiPage.WikiPageNum;
        }

        //Deleted(archived) wp with the same page title should be updated with new page content. No need to create a new wp if the wikipage exist already.
        var wikiPageDeleted = GetByTitle(wikiPage.PageTitle, true);
        if (wikiPageDeleted != null)
        {
            //No need to add history since we already added the history when we archived it.
            wikiPage.WikiPageNum = wikiPageDeleted.WikiPageNum;
            wikiPage.DateTimeSaved = wikiPageDeleted.DateTimeSaved;
            WikiPageCrud.Update(wikiPage);
            return wikiPage.WikiPageNum;
        }

        //At this point the wp does not exist. Insert new a new wikipage.
        return WikiPageCrud.Insert(wikiPage);
    }

    ///<summary>Should only be used for inserting drafts.  Inserting a non-draft wikipage should call InsertAndArchive.</summary>
    public static void InsertAsDraft(WikiPage wikiPage)
    {
        wikiPage.IsDraft = true;
        WikiPageCrud.Insert(wikiPage);
    }

    /// <summary>
    ///     Throws Exceptions, surround with try catch. Should only be used for updating drafts.  Updating a non-draft
    ///     wikipage should never happen.
    /// </summary>
    public static void UpdateDraft(WikiPage wikiPage)
    {
        if (!wikiPage.IsDraft) throw new Exception("Can only use for updating drafts.");

        WikiPageCrud.Update(wikiPage);
    }

    ///<summary>Searches keywords, title, content.  Does not return pagetitles for drafts.</summary>
    public static List<string> GetForSearch(string searchText, bool ignoreContent, bool isDeleted = false, bool isExactSearch = false, bool showMainPages = false, bool searchForLinks = true)
    {
        var listPageTitles = new List<string>();
        var tableResults = new DataTable();
        string[] stringArraySearchTokens;
        var listTitlesWithKeyMain = new List<string>();
        if (isExactSearch)
            stringArraySearchTokens = new[] {SOut.String(searchText)};
        else
            stringArraySearchTokens = SOut.String(searchText).Split(' ');

        var command = "";
        //Match keywords first-----------------------------------------------------------------------------------
        //When a page has a wikipage link, we save it as [[WikiPageNum]] in the page content. Get a list of WikiPageNums that match the search term.
        //We will use the WikiPageNum to search for pages with links to the search term
        var listWikiPageNumsPageTitle = new List<long>();
        command =
            "SELECT WikiPageNum,PageTitle,KeyWords FROM wikipage "
            // \_ represents a literal _ because _ has a special meaning in LIKE clauses.
            //The second \ is just to escape the first \.  The other option would be to pass the \ through POut.String.
            + "WHERE PageTitle NOT LIKE '\\_%' AND IsDraft=0 "
            + "AND IsDeleted=" + SOut.Bool(isDeleted) + " ";
        for (var i = 0; i < stringArraySearchTokens.Length; i++) command += $"AND KeyWords LIKE '%{SOut.String(stringArraySearchTokens[i])}%' ";

        command +=
            "GROUP BY PageTitle "
            + "ORDER BY PageTitle";
        tableResults = DataCore.GetTable(command);
        for (var i = 0; i < tableResults.Rows.Count; i++)
        {
            var pageTitle = tableResults.Rows[i]["PageTitle"].ToString();
            if (listPageTitles.Contains(pageTitle)) continue;

            listPageTitles.Add(pageTitle);
            if (showMainPages && HasMainKeyword(tableResults.Rows[i]["KeyWords"].ToString())) listTitlesWithKeyMain.Add(pageTitle);
        }

        //Match PageTitle Second-----------------------------------------------------------------------------------
        command =
            "SELECT WikiPageNum,PageTitle,KeyWords FROM wikipage "
            + "WHERE PageTitle NOT LIKE '\\_%' AND IsDraft=0 "
            + "AND IsDeleted=" + SOut.Bool(isDeleted) + " ";
        for (var i = 0; i < stringArraySearchTokens.Length; i++) command += "AND PageTitle LIKE '%" + SOut.String(stringArraySearchTokens[i]) + "%' ";

        command +=
            "GROUP BY PageTitle "
            + "ORDER BY PageTitle";
        tableResults = DataCore.GetTable(command);
        for (var i = 0; i < tableResults.Rows.Count; i++)
        {
            var pageTitle = tableResults.Rows[i]["PageTitle"].ToString();
            if (listPageTitles.Contains(pageTitle)) continue;

            listPageTitles.Add(pageTitle);
            listWikiPageNumsPageTitle.Add(SIn.Long(tableResults.Rows[i]["WikiPageNum"].ToString()));
            if (showMainPages && HasMainKeyword(tableResults.Rows[i]["KeyWords"].ToString())) listTitlesWithKeyMain.Add(pageTitle);
        }

        //Match Content third-----------------------------------------------------------------------------------
        if (!ignoreContent)
        {
            command =
                "SELECT PageTitle,KeyWords FROM wikipage "
                + "WHERE PageTitle NOT LIKE '\\_%' AND IsDraft=0 "
                + "AND IsDeleted=" + SOut.Bool(isDeleted) + " ";
            for (var i = 0; i < stringArraySearchTokens.Length; i++)
            {
                command += "AND ";
                if (i == 0) command += "((";

                command += "PageContentPlainText LIKE '%" + SOut.String(stringArraySearchTokens[i]) + "%' ";
            }

            command += ") ";
            if (!searchText.IsNullOrEmpty())
                for (var i = 0; i < listWikiPageNumsPageTitle.Count; i++)
                {
                    if (i != 0) command += " ";

                    command += "OR PageContent LIKE '%[[" + listWikiPageNumsPageTitle[i] + "]]%'";
                }

            command += ") ";
            command +=
                "GROUP BY PageTitle "
                + "ORDER BY PageTitle";
            tableResults = DataCore.GetTable(command);
            for (var i = 0; i < tableResults.Rows.Count; i++)
            {
                var pageTitle = tableResults.Rows[i]["PageTitle"].ToString();
                if (listPageTitles.Contains(pageTitle)) continue;

                listPageTitles.Add(pageTitle);
                if (showMainPages && HasMainKeyword(tableResults.Rows[i]["KeyWords"].ToString())) listTitlesWithKeyMain.Add(pageTitle);
            }
        }

        //Show main pages at the top if checked and found some. listTitlesWithKeyMain will always be empty if showMainPages is false.
        if (listTitlesWithKeyMain.Count > 0)
        {
            for (var i = 0; i < listPageTitles.Count; i++)
                if (!listTitlesWithKeyMain.Contains(listPageTitles[i]))
                    listTitlesWithKeyMain.Add(listPageTitles[i]);

            return listTitlesWithKeyMain;
        }

        return listPageTitles;
    }

    /// <summary>
    ///     Returns true if the passed in comma delimited keyword string contains 'Main' by itself. Otherwise false. Not
    ///     case sensitive.
    /// </summary>
    public static bool HasMainKeyword(string keywords)
    {
        var listKeywords = keywords.ToLower().Split(',').ToList(); //Get each keyword
        for (var j = 0; j < listKeywords.Count; j++)
        {
            listKeywords[j] = listKeywords[j].Trim(' ');
            if (listKeywords[j].Equals("main")) return true;
        }

        return false;
    }

    ///<summary>Returns a list of all pages that reference "PageTitle".  No historical pages or drafts.</summary>
    public static List<WikiPage> GetIncomingLinks(string pageTitle)
    {
        var listWikiPages = new List<WikiPage>();
        var wikiPage = GetByTitle(pageTitle);
        if (wikiPage != null)
        {
            var command = "SELECT * FROM wikipage WHERE PageContent LIKE '%[[" + SOut.Long(wikiPage.WikiPageNum) + "]]%' AND IsDraft=0 AND IsDeleted=0 ORDER BY PageTitle";
            listWikiPages = WikiPageCrud.SelectMany(command);
        }

        return listWikiPages;
    }

    /// <summary>
    ///     Validation was already done in FormWikiRename to make sure that the page does not already exist in WikiPage table.
    ///     But what if the page already exists in WikiPageHistory?  In that case, previous history for the other page would
    ///     start showing as history for
    ///     the newly renamed page, which is fine.  Also renamed drafts, so that we can still match them to their parent wiki
    ///     page.
    /// </summary>
    public static void Rename(WikiPage wikiPage, string pageTitleNew)
    {
        //Security.CurUser.UserNum gets set on MT by the DtoProcessor so it matches the user from the client WS.
        wikiPage.UserNum = Security.CurUser.UserNum;
        //a later improvement would be to validate again here in the business layer.
        InsertAndArchive(wikiPage);
        //Rename all pages in both tables: wikiPage and wikiPageHist.
        var command = "UPDATE wikipage SET PageTitle='" + SOut.String(pageTitleNew) + "'WHERE PageTitle='" + SOut.String(wikiPage.PageTitle) + "'";
        Db.NonQ(command);
        command = "UPDATE wikipagehist SET PageTitle='" + SOut.String(pageTitleNew) + "'WHERE PageTitle='" + SOut.String(wikiPage.PageTitle) + "'";
        Db.NonQ(command);
        //Update all home pages for users.
        command = "UPDATE userodpref SET ValueString='" + SOut.String(pageTitleNew) + "' "
                  + "WHERE FkeyType=" + SOut.Int((int) UserOdFkeyType.WikiHomePage) + " "
                  + "AND ValueString='" + SOut.String(wikiPage.PageTitle) + "'";
        Db.NonQ(command);
        Signalods.SetInvalid(InvalidType.UserOdPrefs);
        UserOdPrefs.RefreshCache();
    }

    ///<summary>Used in TranslateToXhtml to know whether to mark a page as not exists.</summary>
    public static List<bool> CheckPageNamesExist(List<string> pageTitles)
    {
        var command = "SELECT PageTitle FROM wikipage WHERE ";
        for (var i = 0; i < pageTitles.Count; i++)
        {
            if (i > 0) command += "OR ";

            command += "PageTitle='" + SOut.String(pageTitles[i]) + "' ";
        }

        var table = DataCore.GetTable(command);
        var listExists = new List<bool>();
        for (var p = 0; p < pageTitles.Count; p++)
        {
            var existsThisPage = false;
            for (var i = 0; i < table.Rows.Count; i++)
                if (table.Rows[i]["PageTitle"].ToString().ToLower() == pageTitles[p].ToLower())
                {
                    existsThisPage = true;
                    break;
                }

            listExists.Add(existsThisPage);
        }

        return listExists;
    }
    
    ///<summary>Surround with try/catch.  Typically returns something similar to \\SERVER\OpenDentImages\Wiki</summary>
    public static string GetWikiPath()
    {
        var wikiPath = Path.Combine(ImageStore.GetPreferredAtoZpath(), "Wiki");
        if (!Directory.Exists(wikiPath)) Directory.CreateDirectory(wikiPath);

        return wikiPath;
    }

    /// <summary>
    ///     When this is called, all WikiPage links in pageContent should be [[PageTitle]] and NOT [[WikiPageNum]],
    ///     otherwise this will invalidate every wiki page link.
    /// </summary>
    public static string ConvertTitlesToPageNums(string pageContent)
    {
        var matchCollection = Regex.Matches(pageContent, @"\[\[.+?\]\]"); //Find [[ and ]] pairs in the pageContent string.
        var listPageTitleLinks = new List<string>();
        for (var i = 0; i < matchCollection.Count; i++)
        {
            var pageTitle = matchCollection[i].Value;
            if (!IsWikiLink(pageTitle)) continue;

            pageTitle = pageTitle.TrimStart('[').TrimEnd(']');
            listPageTitleLinks.Add(pageTitle);
        }

        //Getting the MIN(WikiPageNum) is safe because there will always only be 1 non-draft reference to the PageTitle.
        //This is because we reuse WikiPageNums instead of deleting and reinserting like before 17.4.
        var command = @"SELECT PageTitle,MIN(WikiPageNum) WikiPageNum FROM wikipage 
				WHERE IsDraft=0 AND PageTitle IN('" + string.Join("','", listPageTitleLinks.Select(x => SOut.String(x))) + @"')
				GROUP BY PageTitle";
        var table = DataCore.GetTable(command);
        var listWikiPages = new List<WikiPage>();
        var stringBuilderContent = new StringBuilder(pageContent);
        for (var i = 0; i < table.Rows.Count; i++)
        {
            var wikiPage = new WikiPage();
            wikiPage.PageTitle = SIn.String(table.Rows[i]["PageTitle"].ToString());
            wikiPage.WikiPageNum = SIn.Long(table.Rows[i]["WikiPageNum"].ToString());
            listWikiPages.Add(wikiPage);
        }

        for (var i = 0; i < listPageTitleLinks.Count; i++)
        {
            string replace;
            var wikiPage = listWikiPages.Find(x => x.PageTitle == listPageTitleLinks[i]);
            if (wikiPage is null)
                replace = "[[0]]"; //wiki page does not exist. replace with wikipagenum=0
            else
                replace = "[[" + wikiPage.WikiPageNum + "]]";

            stringBuilderContent.Replace("[[" + listPageTitleLinks[i] + "]]", replace);
        }

        return stringBuilderContent.ToString();
    }

    public static List<string> GetMissingPageTitles(string pageContent)
    {
        var listInvalidLinks = new List<string>();
        var matchCollection = Regex.Matches(pageContent, @"\[\[.+?\]\]"); //Find [[ and ]] pairs in the pageContent string.
        var listWikiLinks = new List<string>();
        for (var i = 0; i < matchCollection.Count; i++)
        {
            var val = matchCollection[i].Value;
            if (!IsWikiLink(val) || val.Contains("INVALID WIKIPAGE LINK")) continue;

            val = val.TrimStart('[').TrimEnd(']');
            listWikiLinks.Add(val);
        }

        if (listWikiLinks.Count == 0) return listInvalidLinks;

        var command = @"SELECT PageTitle FROM wikipage 
				WHERE IsDraft=0 AND PageTitle IN('" + string.Join("','", listWikiLinks.Select(x => SOut.String(x))) + "')";
        var hashSetValidLinks = new HashSet<string>(Db.GetListString(command));
        listInvalidLinks = listWikiLinks.FindAll(x => !hashSetValidLinks.Contains(x));
        return listInvalidLinks;
    }

    public static bool IsWikiLink(string strVal)
    {
        if (strVal.Contains("[[img:")
            || strVal.Contains("[[keywords:")
            || strVal.Contains("[[file:")
            || strVal.Contains("[[folder:")
            || strVal.Contains("[[list:")
            || strVal.Contains("[[color:")
            || strVal.Contains("[[filecloud:")
            || strVal.Contains("[[foldercloud:")
            || strVal.Contains("[[font:"))
            return false;

        return true;
    }

    public static List<long> GetWikiPageNumsFromPageContent(string pageContent)
    {
        var listWikiPageNums = new List<long>();
        var matchCollection = Regex.Matches(pageContent, @"\[\[.+?\]\]");
        for (var i = 0; i < matchCollection.Count; i++)
        {
            var wikiPageNum = matchCollection[i].Value;
            if (!IsWikiLink(wikiPageNum)) continue;

            //The current match is similar to our wiki page links.  The contents between the brackets should be a foreign key to another wiki page.
            wikiPageNum = wikiPageNum.TrimStart('[').TrimEnd(']');
            listWikiPageNums.Add(SIn.Long(wikiPageNum));
        }

        return listWikiPageNums;
    }

    public static string GetWikiPageContentWithWikiPageTitles(string pageContent)
    {
        var listWikiPageNums = GetWikiPageNumsFromPageContent(pageContent);
        var listWikiPages = GetWikiPages(listWikiPageNums);
        var numInvalid = 1;
        var matchCollection = Regex.Matches(pageContent, @"\[\[.+?\]\]");
        for (var i = 0; i < matchCollection.Count; i++)
        {
            var val = matchCollection[i].Value;
            if (!IsWikiLink(val)) continue;

            //The current match is similar to our wiki page links.  The contents between the brackets should be a foreign key to another wiki page.
            val = val.TrimStart('[').TrimEnd(']');
            var wikiPage = listWikiPages.FirstOrDefault(x => x.WikiPageNum == SIn.Long(val, false));
            string replace;
            if (wikiPage != null)
                replace = "[[" + wikiPage.PageTitle + "]]";
            else
                replace = "[[INVALID WIKIPAGE LINK " + numInvalid++ + "]]"; //Wiki page does not exist.

            var regex = new Regex(Regex.Escape(matchCollection[i].Value));
            //Replace the first instance of the match with the wiki page name (or unknown if not found).
            pageContent = regex.Replace(pageContent, replace, 1);
        }

        return pageContent;
    }

    /// <summary>
    ///     Throws exceptions, surround with Try catch.Only delete wiki drafts with this function.  Normal wiki pages
    ///     cannot be deleted, only archived.
    /// </summary>
    public static void DeleteDraft(WikiPage wikiPage)
    {
        if (!wikiPage.IsDraft) throw new Exception("Can only use for deleting drafts.");

        WikiPageCrud.Delete(wikiPage.WikiPageNum);
    }

    /// <summary>
    ///     Creates historical entry of deletion into wikiPageHist, and deletes current non-draft page from WikiPage.
    ///     For middle tier purposes we need to have the currently logged in user passed into this method.
    /// </summary>
    public static void Archive(string pageTitle, long userNum)
    {
        var wikiPage = GetByTitle(pageTitle);
        if (wikiPage == null) return; //The wiki page could not be found by the page title, nothing to do.

        var wikiPageHist = PageToHist(wikiPage);
        //preserve the existing page with user credentials
        WikiPageHists.Insert(wikiPageHist);
        //make entry to show who deleted the page
        wikiPageHist.IsDeleted = true;
        wikiPageHist.UserNum = userNum;
        wikiPageHist.DateTimeSaved = MiscData.GetNowDateTime();
        WikiPageHists.Insert(wikiPageHist);
        //Now mark the wikipage as IsDeleted 
        wikiPage.IsDeleted = true;
        wikiPage.DateTimeSaved = MiscData.GetNowDateTime();
        WikiPageCrud.Update(wikiPage);
        //Remove all associated home pages for all users.
        UserOdPrefs.DeleteForValueString(0, UserOdFkeyType.WikiHomePage, pageTitle);
        Signalods.SetInvalid(InvalidType.UserOdPrefs);
        UserOdPrefs.RefreshCache();
    }

    public static WikiPageHist PageToHist(WikiPage wikiPage)
    {
        var wikiPageHist = new WikiPageHist();
        wikiPageHist.UserNum = wikiPage.UserNum;
        wikiPageHist.PageTitle = wikiPage.PageTitle;
        wikiPageHist.PageContent = wikiPage.PageContent;
        wikiPageHist.DateTimeSaved = wikiPage.DateTimeSaved; //This gets set to NOW if this page is then inserted
        wikiPageHist.IsDeleted = false;
        return wikiPageHist;
    }

    public static bool IsWikiPageTitleValid(string pageTitle, out string errorMsg)
    {
        errorMsg = "";
        if (pageTitle.Contains("#"))
        {
            errorMsg = Lans.g("WikiPages", "Page title cannot contain the pound character.");
            return false;
        }

        if (pageTitle.StartsWith("_"))
        {
            errorMsg = Lans.g("WikiPages", "Page title cannot start with the underscore character.");
            return false;
        }

        if (pageTitle.Contains("\""))
        {
            errorMsg = Lans.g("WikiPages", "Page title cannot contain double quotes.");
            return false;
        }

        if (pageTitle.Contains("\r") || pageTitle.Contains("\n"))
        {
            errorMsg = Lans.g("WikiPages", "Page title cannot contain carriage return."); //there is also no way to enter one.
            return false;
        }

        return true;
    }

    ///<summary>Gets one WikiPage from the db.</summary>
    public static WikiPage GetOne(long wikiPageNum)
    {
        return WikiPageCrud.SelectOne(wikiPageNum);
    }

    ///<summary>Only used in FormDatabaseMaintenance to fix wikipages whose PageContentPlainText got eaten by greedy regex.</summary>
    public static List<WikiPage> GetAll()
    {
        var command = "SELECT * FROM wikipage";
        return WikiPageCrud.SelectMany(command);
    }

    /// <summary>Updates the links to wpOld to wpNew in the wikipage page wikiPageCur.</summary>
    /// <param name="wikiPageNum">This is the wikipage that will get updated.</param>
    /// <param name="wikiPageNumOld">This is the wikipage that will be used to find references in wikiPageCur.</param>
    /// <param name="wikiPageNumNew">This is the wikipage that will be used to replace the references to.</param>
    public static void UpdateWikiPageReferences(long wikiPageNum, long wikiPageNumOld, long wikiPageNumNew)
    {
        var wikiPage = GetOne(wikiPageNum); //Getting from the database to ensure the WikiPageNums have not been replaced with PageTitles.
        if (wikiPage == null) return;

        var wikiPageOld = wikiPage.Copy();
        var sbPageContent = new StringBuilder(wikiPage.PageContent);
        StringTools.RegReplace(sbPageContent, $@"\[\[{wikiPageNumOld}\]\]", "[[" + wikiPageNumNew + "]]");
        wikiPage.PageContent = sbPageContent.ToString();
        Update(wikiPage, wikiPageOld);
    }

    #region Update

    public static void Update(WikiPage wikiPage)
    {
        WikiPageCrud.Update(wikiPage);
    }

    public static void Update(WikiPage wikiPage, WikiPage oldWikiPage)
    {
        WikiPageCrud.Update(wikiPage, oldWikiPage);
    }

    #endregion

    #region CachePattern

    ///<summary>The only wiki page that gets cached is the master page.</summary>
    private static WikiPage _wikiPageMaster;

    
    public static WikiPage WikiPageMaster
    {
        get
        {
            if (_wikiPageMaster == null) RefreshCache();

            return _wikiPageMaster;
        }
        set => _wikiPageMaster = value;
    }

    
    public static DataTable RefreshCache()
    {
        var command = "SELECT * FROM wikipage WHERE PageTitle='_Master' AND IsDraft=0"; //There is currently no way for a master page to be a draft.
        var table = DataCore.GetTable(command);
        table.TableName = "WikiPage";
        FillCache(table);
        return table;
    }

    public static void ClearCache()
    {
        _wikiPageMaster = null;
    }

    
    public static void FillCache(DataTable table)
    {
        _wikiPageMaster = WikiPageCrud.TableToList(table)[0];
    }

    #endregion CachePattern

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<WikiPage> Refresh(long patNum){

        string command="SELECT * FROM wikipage WHERE PatNum = "+POut.Long(patNum);
        return Crud.WikiPageCrud.SelectMany(command);
    }
    */
}