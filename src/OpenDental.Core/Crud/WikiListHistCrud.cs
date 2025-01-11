#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class WikiListHistCrud
{
    public static WikiListHist SelectOne(long wikiListHistNum)
    {
        var command = "SELECT * FROM wikilisthist "
                      + "WHERE WikiListHistNum = " + SOut.Long(wikiListHistNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static WikiListHist SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<WikiListHist> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<WikiListHist> TableToList(DataTable table)
    {
        var retVal = new List<WikiListHist>();
        WikiListHist wikiListHist;
        foreach (DataRow row in table.Rows)
        {
            wikiListHist = new WikiListHist();
            wikiListHist.WikiListHistNum = SIn.Long(row["WikiListHistNum"].ToString());
            wikiListHist.UserNum = SIn.Long(row["UserNum"].ToString());
            wikiListHist.ListName = SIn.String(row["ListName"].ToString());
            wikiListHist.ListHeaders = SIn.String(row["ListHeaders"].ToString());
            wikiListHist.ListContent = SIn.String(row["ListContent"].ToString());
            wikiListHist.DateTimeSaved = SIn.DateTime(row["DateTimeSaved"].ToString());
            retVal.Add(wikiListHist);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<WikiListHist> listWikiListHists, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "WikiListHist";
        var table = new DataTable(tableName);
        table.Columns.Add("WikiListHistNum");
        table.Columns.Add("UserNum");
        table.Columns.Add("ListName");
        table.Columns.Add("ListHeaders");
        table.Columns.Add("ListContent");
        table.Columns.Add("DateTimeSaved");
        foreach (var wikiListHist in listWikiListHists)
            table.Rows.Add(SOut.Long(wikiListHist.WikiListHistNum), SOut.Long(wikiListHist.UserNum), wikiListHist.ListName, wikiListHist.ListHeaders, wikiListHist.ListContent, SOut.DateT(wikiListHist.DateTimeSaved, false));
        return table;
    }

    public static long Insert(WikiListHist wikiListHist)
    {
        return Insert(wikiListHist, false);
    }

    public static long Insert(WikiListHist wikiListHist, bool useExistingPK)
    {
        var command = "INSERT INTO wikilisthist (";

        command += "UserNum,ListName,ListHeaders,ListContent,DateTimeSaved) VALUES(";

        command +=
            SOut.Long(wikiListHist.UserNum) + ","
                                            + "'" + SOut.String(wikiListHist.ListName) + "',"
                                            + DbHelper.ParamChar + "paramListHeaders,"
                                            + DbHelper.ParamChar + "paramListContent,"
                                            + SOut.DateT(wikiListHist.DateTimeSaved) + ")";
        if (wikiListHist.ListHeaders == null) wikiListHist.ListHeaders = "";
        var paramListHeaders = new OdSqlParameter("paramListHeaders", OdDbType.Text, SOut.StringParam(wikiListHist.ListHeaders));
        if (wikiListHist.ListContent == null) wikiListHist.ListContent = "";
        var paramListContent = new OdSqlParameter("paramListContent", OdDbType.Text, SOut.StringParam(wikiListHist.ListContent));
        {
            wikiListHist.WikiListHistNum = Db.NonQ(command, true, "WikiListHistNum", "wikiListHist", paramListHeaders, paramListContent);
        }
        return wikiListHist.WikiListHistNum;
    }

    public static long InsertNoCache(WikiListHist wikiListHist)
    {
        return InsertNoCache(wikiListHist, false);
    }

    public static long InsertNoCache(WikiListHist wikiListHist, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO wikilisthist (";
        if (isRandomKeys || useExistingPK) command += "WikiListHistNum,";
        command += "UserNum,ListName,ListHeaders,ListContent,DateTimeSaved) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(wikiListHist.WikiListHistNum) + ",";
        command +=
            SOut.Long(wikiListHist.UserNum) + ","
                                            + "'" + SOut.String(wikiListHist.ListName) + "',"
                                            + DbHelper.ParamChar + "paramListHeaders,"
                                            + DbHelper.ParamChar + "paramListContent,"
                                            + SOut.DateT(wikiListHist.DateTimeSaved) + ")";
        if (wikiListHist.ListHeaders == null) wikiListHist.ListHeaders = "";
        var paramListHeaders = new OdSqlParameter("paramListHeaders", OdDbType.Text, SOut.StringParam(wikiListHist.ListHeaders));
        if (wikiListHist.ListContent == null) wikiListHist.ListContent = "";
        var paramListContent = new OdSqlParameter("paramListContent", OdDbType.Text, SOut.StringParam(wikiListHist.ListContent));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramListHeaders, paramListContent);
        else
            wikiListHist.WikiListHistNum = Db.NonQ(command, true, "WikiListHistNum", "wikiListHist", paramListHeaders, paramListContent);
        return wikiListHist.WikiListHistNum;
    }

    public static void Update(WikiListHist wikiListHist)
    {
        var command = "UPDATE wikilisthist SET "
                      + "UserNum        =  " + SOut.Long(wikiListHist.UserNum) + ", "
                      + "ListName       = '" + SOut.String(wikiListHist.ListName) + "', "
                      + "ListHeaders    =  " + DbHelper.ParamChar + "paramListHeaders, "
                      + "ListContent    =  " + DbHelper.ParamChar + "paramListContent, "
                      + "DateTimeSaved  =  " + SOut.DateT(wikiListHist.DateTimeSaved) + " "
                      + "WHERE WikiListHistNum = " + SOut.Long(wikiListHist.WikiListHistNum);
        if (wikiListHist.ListHeaders == null) wikiListHist.ListHeaders = "";
        var paramListHeaders = new OdSqlParameter("paramListHeaders", OdDbType.Text, SOut.StringParam(wikiListHist.ListHeaders));
        if (wikiListHist.ListContent == null) wikiListHist.ListContent = "";
        var paramListContent = new OdSqlParameter("paramListContent", OdDbType.Text, SOut.StringParam(wikiListHist.ListContent));
        Db.NonQ(command, paramListHeaders, paramListContent);
    }

    public static bool Update(WikiListHist wikiListHist, WikiListHist oldWikiListHist)
    {
        var command = "";
        if (wikiListHist.UserNum != oldWikiListHist.UserNum)
        {
            if (command != "") command += ",";
            command += "UserNum = " + SOut.Long(wikiListHist.UserNum) + "";
        }

        if (wikiListHist.ListName != oldWikiListHist.ListName)
        {
            if (command != "") command += ",";
            command += "ListName = '" + SOut.String(wikiListHist.ListName) + "'";
        }

        if (wikiListHist.ListHeaders != oldWikiListHist.ListHeaders)
        {
            if (command != "") command += ",";
            command += "ListHeaders = " + DbHelper.ParamChar + "paramListHeaders";
        }

        if (wikiListHist.ListContent != oldWikiListHist.ListContent)
        {
            if (command != "") command += ",";
            command += "ListContent = " + DbHelper.ParamChar + "paramListContent";
        }

        if (wikiListHist.DateTimeSaved != oldWikiListHist.DateTimeSaved)
        {
            if (command != "") command += ",";
            command += "DateTimeSaved = " + SOut.DateT(wikiListHist.DateTimeSaved) + "";
        }

        if (command == "") return false;
        if (wikiListHist.ListHeaders == null) wikiListHist.ListHeaders = "";
        var paramListHeaders = new OdSqlParameter("paramListHeaders", OdDbType.Text, SOut.StringParam(wikiListHist.ListHeaders));
        if (wikiListHist.ListContent == null) wikiListHist.ListContent = "";
        var paramListContent = new OdSqlParameter("paramListContent", OdDbType.Text, SOut.StringParam(wikiListHist.ListContent));
        command = "UPDATE wikilisthist SET " + command
                                             + " WHERE WikiListHistNum = " + SOut.Long(wikiListHist.WikiListHistNum);
        Db.NonQ(command, paramListHeaders, paramListContent);
        return true;
    }

    public static bool UpdateComparison(WikiListHist wikiListHist, WikiListHist oldWikiListHist)
    {
        if (wikiListHist.UserNum != oldWikiListHist.UserNum) return true;
        if (wikiListHist.ListName != oldWikiListHist.ListName) return true;
        if (wikiListHist.ListHeaders != oldWikiListHist.ListHeaders) return true;
        if (wikiListHist.ListContent != oldWikiListHist.ListContent) return true;
        if (wikiListHist.DateTimeSaved != oldWikiListHist.DateTimeSaved) return true;
        return false;
    }

    public static void Delete(long wikiListHistNum)
    {
        var command = "DELETE FROM wikilisthist "
                      + "WHERE WikiListHistNum = " + SOut.Long(wikiListHistNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listWikiListHistNums)
    {
        if (listWikiListHistNums == null || listWikiListHistNums.Count == 0) return;
        var command = "DELETE FROM wikilisthist "
                      + "WHERE WikiListHistNum IN(" + string.Join(",", listWikiListHistNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}