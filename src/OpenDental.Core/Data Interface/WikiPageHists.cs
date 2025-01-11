using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class WikiPageHists
{
    /// <summary>
    ///     Ordered by dateTimeSaved.  Objects will not have the PageContent field populated.  Use GetPageContent to get the
    ///     content for a
    ///     specific revision.
    /// </summary>
    public static List<WikiPageHist> GetByTitleNoPageContent(string pageTitle)
    {
        var command = "SELECT WikiPageNum,UserNum,PageTitle,'' PageContent,DateTimeSaved,IsDeleted "
                      + "FROM wikipagehist WHERE PageTitle='" + SOut.String(pageTitle) + "' ORDER BY DateTimeSaved";
        return WikiPageHistCrud.SelectMany(command);
    }

    public static string GetPageContent(long wikiPageNum)
    {
        if (wikiPageNum < 1) return "";

        var command = "SELECT PageContent FROM wikipagehist WHERE WikiPageNum=" + SOut.Long(wikiPageNum);
        return DataCore.GetScalar(command);
    }

    
    public static long Insert(WikiPageHist wikiPageHist)
    {
        return WikiPageHistCrud.Insert(wikiPageHist);
    }

    ///<summary>Deletes all WikiPageHists before the given cutoff date. Returns the number of entries deleted.</summary>
    public static long DeleteBeforeDate(DateTime dateCutoff)
    {
        var command = $"DELETE FROM wikipagehist WHERE DateTimeSaved <= {SOut.DateT(dateCutoff)} ";
        return Db.NonQ(command);
    }

    public static WikiPage RevertFrom(WikiPageHist wikiPageHist)
    {
        //Get the existing WikiPage to ensure the WikiPageNum is preserved for links.
        //See JobNum 4429 for the job that made this necessary.
        var wikiPage = WikiPages.GetByTitle(wikiPageHist.PageTitle);
        if (wikiPage == null) wikiPage = new WikiPage();

        //retVal.WikiPageNum
        //retVal.UserNum
        wikiPage.PageTitle = wikiPageHist.PageTitle;
        wikiPage.PageContent = wikiPageHist.PageContent;
        wikiPage.KeyWords = "";
        var match = Regex.Match(wikiPageHist.PageContent, @"\[\[(keywords:).*?\]\]");
        if (match.Length > 0) wikiPage.KeyWords = match.Value.Substring(11).TrimEnd(']');

        //retVal.DateTimeSaved=DateTime.Now;//gets set when inserted.
        return wikiPage;
    }


    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    ///<summary>Gets one WikiPageHist from the db.</summary>
    public static WikiPageHist GetOne(long wikiPageNum){

        return Crud.WikiPageHistCrud.SelectOne(wikiPageNum);
    }

    
    public static void Update(WikiPageHist wikiPageHist){

        Crud.WikiPageHistCrud.Update(wikiPageHist);
    }

    
    public static void Delete(long wikiPageNum) {

        string command= "DELETE FROM wikipagehist WHERE WikiPageNum = "+POut.Long(wikiPageNum);
        Db.NonQ(command);
    }
    */
}