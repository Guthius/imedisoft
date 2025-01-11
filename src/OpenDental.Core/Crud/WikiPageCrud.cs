#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class WikiPageCrud
{
    public static WikiPage SelectOne(long wikiPageNum)
    {
        var command = "SELECT * FROM wikipage "
                      + "WHERE WikiPageNum = " + SOut.Long(wikiPageNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static WikiPage SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<WikiPage> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<WikiPage> TableToList(DataTable table)
    {
        var retVal = new List<WikiPage>();
        WikiPage wikiPage;
        foreach (DataRow row in table.Rows)
        {
            wikiPage = new WikiPage();
            wikiPage.WikiPageNum = SIn.Long(row["WikiPageNum"].ToString());
            wikiPage.UserNum = SIn.Long(row["UserNum"].ToString());
            wikiPage.PageTitle = SIn.String(row["PageTitle"].ToString());
            wikiPage.KeyWords = SIn.String(row["KeyWords"].ToString());
            wikiPage.PageContent = SIn.String(row["PageContent"].ToString());
            wikiPage.DateTimeSaved = SIn.DateTime(row["DateTimeSaved"].ToString());
            wikiPage.IsDeleted = SIn.Bool(row["IsDeleted"].ToString());
            wikiPage.IsDraft = SIn.Bool(row["IsDraft"].ToString());
            wikiPage.IsLocked = SIn.Bool(row["IsLocked"].ToString());
            wikiPage.PageContentPlainText = SIn.String(row["PageContentPlainText"].ToString());
            retVal.Add(wikiPage);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<WikiPage> listWikiPages, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "WikiPage";
        var table = new DataTable(tableName);
        table.Columns.Add("WikiPageNum");
        table.Columns.Add("UserNum");
        table.Columns.Add("PageTitle");
        table.Columns.Add("KeyWords");
        table.Columns.Add("PageContent");
        table.Columns.Add("DateTimeSaved");
        table.Columns.Add("IsDeleted");
        table.Columns.Add("IsDraft");
        table.Columns.Add("IsLocked");
        table.Columns.Add("PageContentPlainText");
        foreach (var wikiPage in listWikiPages)
            table.Rows.Add(SOut.Long(wikiPage.WikiPageNum), SOut.Long(wikiPage.UserNum), wikiPage.PageTitle, wikiPage.KeyWords, wikiPage.PageContent, SOut.DateT(wikiPage.DateTimeSaved, false), SOut.Bool(wikiPage.IsDeleted), SOut.Bool(wikiPage.IsDraft), SOut.Bool(wikiPage.IsLocked), wikiPage.PageContentPlainText);
        return table;
    }

    public static long Insert(WikiPage wikiPage)
    {
        return Insert(wikiPage, false);
    }

    public static long Insert(WikiPage wikiPage, bool useExistingPK)
    {
        var command = "INSERT INTO wikipage (";

        command += "UserNum,PageTitle,KeyWords,PageContent,DateTimeSaved,IsDeleted,IsDraft,IsLocked,PageContentPlainText) VALUES(";

        command +=
            SOut.Long(wikiPage.UserNum) + ","
                                        + "'" + SOut.String(wikiPage.PageTitle) + "',"
                                        + "'" + SOut.String(wikiPage.KeyWords) + "',"
                                        + DbHelper.ParamChar + "paramPageContent,"
                                        + DbHelper.Now() + ","
                                        + SOut.Bool(wikiPage.IsDeleted) + ","
                                        + SOut.Bool(wikiPage.IsDraft) + ","
                                        + SOut.Bool(wikiPage.IsLocked) + ","
                                        + DbHelper.ParamChar + "paramPageContentPlainText)";
        if (wikiPage.PageContent == null) wikiPage.PageContent = "";
        var paramPageContent = new OdSqlParameter("paramPageContent", OdDbType.Text, SOut.StringParam(wikiPage.PageContent));
        if (wikiPage.PageContentPlainText == null) wikiPage.PageContentPlainText = "";
        var paramPageContentPlainText = new OdSqlParameter("paramPageContentPlainText", OdDbType.Text, SOut.StringParam(wikiPage.PageContentPlainText));
        {
            wikiPage.WikiPageNum = Db.NonQ(command, true, "WikiPageNum", "wikiPage", paramPageContent, paramPageContentPlainText);
        }
        return wikiPage.WikiPageNum;
    }

    public static long InsertNoCache(WikiPage wikiPage)
    {
        return InsertNoCache(wikiPage, false);
    }

    public static long InsertNoCache(WikiPage wikiPage, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO wikipage (";
        if (isRandomKeys || useExistingPK) command += "WikiPageNum,";
        command += "UserNum,PageTitle,KeyWords,PageContent,DateTimeSaved,IsDeleted,IsDraft,IsLocked,PageContentPlainText) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(wikiPage.WikiPageNum) + ",";
        command +=
            SOut.Long(wikiPage.UserNum) + ","
                                        + "'" + SOut.String(wikiPage.PageTitle) + "',"
                                        + "'" + SOut.String(wikiPage.KeyWords) + "',"
                                        + DbHelper.ParamChar + "paramPageContent,"
                                        + DbHelper.Now() + ","
                                        + SOut.Bool(wikiPage.IsDeleted) + ","
                                        + SOut.Bool(wikiPage.IsDraft) + ","
                                        + SOut.Bool(wikiPage.IsLocked) + ","
                                        + DbHelper.ParamChar + "paramPageContentPlainText)";
        if (wikiPage.PageContent == null) wikiPage.PageContent = "";
        var paramPageContent = new OdSqlParameter("paramPageContent", OdDbType.Text, SOut.StringParam(wikiPage.PageContent));
        if (wikiPage.PageContentPlainText == null) wikiPage.PageContentPlainText = "";
        var paramPageContentPlainText = new OdSqlParameter("paramPageContentPlainText", OdDbType.Text, SOut.StringParam(wikiPage.PageContentPlainText));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramPageContent, paramPageContentPlainText);
        else
            wikiPage.WikiPageNum = Db.NonQ(command, true, "WikiPageNum", "wikiPage", paramPageContent, paramPageContentPlainText);
        return wikiPage.WikiPageNum;
    }

    public static void Update(WikiPage wikiPage)
    {
        var command = "UPDATE wikipage SET "
                      + "UserNum             =  " + SOut.Long(wikiPage.UserNum) + ", "
                      + "PageTitle           = '" + SOut.String(wikiPage.PageTitle) + "', "
                      + "KeyWords            = '" + SOut.String(wikiPage.KeyWords) + "', "
                      + "PageContent         =  " + DbHelper.ParamChar + "paramPageContent, "
                      + "DateTimeSaved       =  " + SOut.DateT(wikiPage.DateTimeSaved) + ", "
                      + "IsDeleted           =  " + SOut.Bool(wikiPage.IsDeleted) + ", "
                      + "IsDraft             =  " + SOut.Bool(wikiPage.IsDraft) + ", "
                      + "IsLocked            =  " + SOut.Bool(wikiPage.IsLocked) + ", "
                      + "PageContentPlainText=  " + DbHelper.ParamChar + "paramPageContentPlainText "
                      + "WHERE WikiPageNum = " + SOut.Long(wikiPage.WikiPageNum);
        if (wikiPage.PageContent == null) wikiPage.PageContent = "";
        var paramPageContent = new OdSqlParameter("paramPageContent", OdDbType.Text, SOut.StringParam(wikiPage.PageContent));
        if (wikiPage.PageContentPlainText == null) wikiPage.PageContentPlainText = "";
        var paramPageContentPlainText = new OdSqlParameter("paramPageContentPlainText", OdDbType.Text, SOut.StringParam(wikiPage.PageContentPlainText));
        Db.NonQ(command, paramPageContent, paramPageContentPlainText);
    }

    public static bool Update(WikiPage wikiPage, WikiPage oldWikiPage)
    {
        var command = "";
        if (wikiPage.UserNum != oldWikiPage.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(wikiPage.UserNum) + "";
        }

        if (wikiPage.PageTitle != oldWikiPage.PageTitle)
        {
            if (command != "") command += ",";
            command += "PageTitle = '" + SOut.String(wikiPage.PageTitle) + "'";
        }

        if (wikiPage.KeyWords != oldWikiPage.KeyWords)
        {
            if (command != "") command += ",";
            command += "KeyWords = '" + SOut.String(wikiPage.KeyWords) + "'";
        }

        if (wikiPage.PageContent != oldWikiPage.PageContent)
        {
            if (command != "") command += ",";
            command += "PageContent = " + DbHelper.ParamChar + "paramPageContent";
        }

        if (wikiPage.DateTimeSaved != oldWikiPage.DateTimeSaved)
        {
            if (command != "") command += ",";
            command += "DateTimeSaved = " + SOut.DateT(wikiPage.DateTimeSaved) + "";
        }

        if (wikiPage.IsDeleted != oldWikiPage.IsDeleted)
        {
            if (command != "") command += ",";
            command += "IsDeleted = " + SOut.Bool(wikiPage.IsDeleted) + "";
        }

        if (wikiPage.IsDraft != oldWikiPage.IsDraft)
        {
            if (command != "") command += ",";
            command += "IsDraft = " + SOut.Bool(wikiPage.IsDraft) + "";
        }

        if (wikiPage.IsLocked != oldWikiPage.IsLocked)
        {
            if (command != "") command += ",";
            command += "IsLocked = " + SOut.Bool(wikiPage.IsLocked) + "";
        }

        if (wikiPage.PageContentPlainText != oldWikiPage.PageContentPlainText)
        {
            if (command != "") command += ",";
            command += "PageContentPlainText = " + DbHelper.ParamChar + "paramPageContentPlainText";
        }

        if (command == "") return false;
        if (wikiPage.PageContent == null) wikiPage.PageContent = "";
        var paramPageContent = new OdSqlParameter("paramPageContent", OdDbType.Text, SOut.StringParam(wikiPage.PageContent));
        if (wikiPage.PageContentPlainText == null) wikiPage.PageContentPlainText = "";
        var paramPageContentPlainText = new OdSqlParameter("paramPageContentPlainText", OdDbType.Text, SOut.StringParam(wikiPage.PageContentPlainText));
        command = "UPDATE wikipage SET " + command
                                         + " WHERE WikiPageNum = " + SOut.Long(wikiPage.WikiPageNum);
        Db.NonQ(command, paramPageContent, paramPageContentPlainText);
        return true;
    }

    public static bool UpdateComparison(WikiPage wikiPage, WikiPage oldWikiPage)
    {
        if (wikiPage.UserNum != oldWikiPage.UserNum) return true;
        if (wikiPage.PageTitle != oldWikiPage.PageTitle) return true;
        if (wikiPage.KeyWords != oldWikiPage.KeyWords) return true;
        if (wikiPage.PageContent != oldWikiPage.PageContent) return true;
        if (wikiPage.DateTimeSaved != oldWikiPage.DateTimeSaved) return true;
        if (wikiPage.IsDeleted != oldWikiPage.IsDeleted) return true;
        if (wikiPage.IsDraft != oldWikiPage.IsDraft) return true;
        if (wikiPage.IsLocked != oldWikiPage.IsLocked) return true;
        if (wikiPage.PageContentPlainText != oldWikiPage.PageContentPlainText) return true;
        return false;
    }

    public static void Delete(long wikiPageNum)
    {
        var command = "DELETE FROM wikipage "
                      + "WHERE WikiPageNum = " + SOut.Long(wikiPageNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listWikiPageNums)
    {
        if (listWikiPageNums == null || listWikiPageNums.Count == 0) return;
        var command = "DELETE FROM wikipage "
                      + "WHERE WikiPageNum IN(" + string.Join(",", listWikiPageNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}