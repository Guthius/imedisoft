#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class WikiPageHistCrud
{
    public static WikiPageHist SelectOne(long wikiPageNum)
    {
        var command = "SELECT * FROM wikipagehist "
                      + "WHERE WikiPageNum = " + SOut.Long(wikiPageNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static WikiPageHist SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<WikiPageHist> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<WikiPageHist> TableToList(DataTable table)
    {
        var retVal = new List<WikiPageHist>();
        WikiPageHist wikiPageHist;
        foreach (DataRow row in table.Rows)
        {
            wikiPageHist = new WikiPageHist();
            wikiPageHist.WikiPageNum = SIn.Long(row["WikiPageNum"].ToString());
            wikiPageHist.UserNum = SIn.Long(row["UserNum"].ToString());
            wikiPageHist.PageTitle = SIn.String(row["PageTitle"].ToString());
            wikiPageHist.PageContent = SIn.String(row["PageContent"].ToString());
            wikiPageHist.DateTimeSaved = SIn.DateTime(row["DateTimeSaved"].ToString());
            wikiPageHist.IsDeleted = SIn.Bool(row["IsDeleted"].ToString());
            retVal.Add(wikiPageHist);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<WikiPageHist> listWikiPageHists, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "WikiPageHist";
        var table = new DataTable(tableName);
        table.Columns.Add("WikiPageNum");
        table.Columns.Add("UserNum");
        table.Columns.Add("PageTitle");
        table.Columns.Add("PageContent");
        table.Columns.Add("DateTimeSaved");
        table.Columns.Add("IsDeleted");
        foreach (var wikiPageHist in listWikiPageHists)
            table.Rows.Add(SOut.Long(wikiPageHist.WikiPageNum), SOut.Long(wikiPageHist.UserNum), wikiPageHist.PageTitle, wikiPageHist.PageContent, SOut.DateT(wikiPageHist.DateTimeSaved, false), SOut.Bool(wikiPageHist.IsDeleted));
        return table;
    }

    public static long Insert(WikiPageHist wikiPageHist)
    {
        return Insert(wikiPageHist, false);
    }

    public static long Insert(WikiPageHist wikiPageHist, bool useExistingPK)
    {
        var command = "INSERT INTO wikipagehist (";

        command += "UserNum,PageTitle,PageContent,DateTimeSaved,IsDeleted) VALUES(";

        command +=
            SOut.Long(wikiPageHist.UserNum) + ","
                                            + "'" + SOut.String(wikiPageHist.PageTitle) + "',"
                                            + DbHelper.ParamChar + "paramPageContent,"
                                            + SOut.DateT(wikiPageHist.DateTimeSaved) + ","
                                            + SOut.Bool(wikiPageHist.IsDeleted) + ")";
        if (wikiPageHist.PageContent == null) wikiPageHist.PageContent = "";
        var paramPageContent = new OdSqlParameter("paramPageContent", OdDbType.Text, SOut.StringParam(wikiPageHist.PageContent));
        {
            wikiPageHist.WikiPageNum = Db.NonQ(command, true, "WikiPageNum", "wikiPageHist", paramPageContent);
        }
        return wikiPageHist.WikiPageNum;
    }

    public static long InsertNoCache(WikiPageHist wikiPageHist)
    {
        return InsertNoCache(wikiPageHist, false);
    }

    public static long InsertNoCache(WikiPageHist wikiPageHist, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO wikipagehist (";
        if (isRandomKeys || useExistingPK) command += "WikiPageNum,";
        command += "UserNum,PageTitle,PageContent,DateTimeSaved,IsDeleted) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(wikiPageHist.WikiPageNum) + ",";
        command +=
            SOut.Long(wikiPageHist.UserNum) + ","
                                            + "'" + SOut.String(wikiPageHist.PageTitle) + "',"
                                            + DbHelper.ParamChar + "paramPageContent,"
                                            + SOut.DateT(wikiPageHist.DateTimeSaved) + ","
                                            + SOut.Bool(wikiPageHist.IsDeleted) + ")";
        if (wikiPageHist.PageContent == null) wikiPageHist.PageContent = "";
        var paramPageContent = new OdSqlParameter("paramPageContent", OdDbType.Text, SOut.StringParam(wikiPageHist.PageContent));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramPageContent);
        else
            wikiPageHist.WikiPageNum = Db.NonQ(command, true, "WikiPageNum", "wikiPageHist", paramPageContent);
        return wikiPageHist.WikiPageNum;
    }

    public static void Update(WikiPageHist wikiPageHist)
    {
        var command = "UPDATE wikipagehist SET "
                      + "UserNum      =  " + SOut.Long(wikiPageHist.UserNum) + ", "
                      + "PageTitle    = '" + SOut.String(wikiPageHist.PageTitle) + "', "
                      + "PageContent  =  " + DbHelper.ParamChar + "paramPageContent, "
                      + "DateTimeSaved=  " + SOut.DateT(wikiPageHist.DateTimeSaved) + ", "
                      + "IsDeleted    =  " + SOut.Bool(wikiPageHist.IsDeleted) + " "
                      + "WHERE WikiPageNum = " + SOut.Long(wikiPageHist.WikiPageNum);
        if (wikiPageHist.PageContent == null) wikiPageHist.PageContent = "";
        var paramPageContent = new OdSqlParameter("paramPageContent", OdDbType.Text, SOut.StringParam(wikiPageHist.PageContent));
        Db.NonQ(command, paramPageContent);
    }

    public static bool Update(WikiPageHist wikiPageHist, WikiPageHist oldWikiPageHist)
    {
        var command = "";
        if (wikiPageHist.UserNum != oldWikiPageHist.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(wikiPageHist.UserNum) + "";
        }

        if (wikiPageHist.PageTitle != oldWikiPageHist.PageTitle)
        {
            if (command != "") command += ",";
            command += "PageTitle = '" + SOut.String(wikiPageHist.PageTitle) + "'";
        }

        if (wikiPageHist.PageContent != oldWikiPageHist.PageContent)
        {
            if (command != "") command += ",";
            command += "PageContent = " + DbHelper.ParamChar + "paramPageContent";
        }

        if (wikiPageHist.DateTimeSaved != oldWikiPageHist.DateTimeSaved)
        {
            if (command != "") command += ",";
            command += "DateTimeSaved = " + SOut.DateT(wikiPageHist.DateTimeSaved) + "";
        }

        if (wikiPageHist.IsDeleted != oldWikiPageHist.IsDeleted)
        {
            if (command != "") command += ",";
            command += "IsDeleted = " + SOut.Bool(wikiPageHist.IsDeleted) + "";
        }

        if (command == "") return false;
        if (wikiPageHist.PageContent == null) wikiPageHist.PageContent = "";
        var paramPageContent = new OdSqlParameter("paramPageContent", OdDbType.Text, SOut.StringParam(wikiPageHist.PageContent));
        command = "UPDATE wikipagehist SET " + command
                                             + " WHERE WikiPageNum = " + SOut.Long(wikiPageHist.WikiPageNum);
        Db.NonQ(command, paramPageContent);
        return true;
    }

    public static bool UpdateComparison(WikiPageHist wikiPageHist, WikiPageHist oldWikiPageHist)
    {
        if (wikiPageHist.UserNum != oldWikiPageHist.UserNum) return true;
        if (wikiPageHist.PageTitle != oldWikiPageHist.PageTitle) return true;
        if (wikiPageHist.PageContent != oldWikiPageHist.PageContent) return true;
        if (wikiPageHist.DateTimeSaved != oldWikiPageHist.DateTimeSaved) return true;
        if (wikiPageHist.IsDeleted != oldWikiPageHist.IsDeleted) return true;
        return false;
    }

    public static void Delete(long wikiPageNum)
    {
        var command = "DELETE FROM wikipagehist "
                      + "WHERE WikiPageNum = " + SOut.Long(wikiPageNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listWikiPageNums)
    {
        if (listWikiPageNums == null || listWikiPageNums.Count == 0) return;
        var command = "DELETE FROM wikipagehist "
                      + "WHERE WikiPageNum IN(" + string.Join(",", listWikiPageNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}