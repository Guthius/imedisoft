using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class WikiListHists
{
    /// <summary>
    ///     Ordered by dateTimeSaved.  Case insensitive.  Objects will not have the ListHeaders and ListContent fields
    ///     populated.  Use SelectOne
    ///     to get the headers and content for a specific revision.
    /// </summary>
    public static List<WikiListHist> GetByNameNoContent(string listName)
    {
        if (string.IsNullOrEmpty(listName)) return new List<WikiListHist>();

        var command = $@"SELECT WikiListHistNum,UserNum,ListName,'' ListHeaders,'' ListContent,DateTimeSaved
				FROM wikilisthist WHERE ListName='{SOut.String(listName)}' ORDER BY DateTimeSaved";
        return WikiListHistCrud.SelectMany(command);
    }

    public static WikiListHist SelectOne(long wikiListHistNum)
    {
        if (wikiListHistNum <= 0) return null;

        return WikiListHistCrud.SelectOne(wikiListHistNum);
    }

    
    public static long Insert(WikiListHist wikiListHist)
    {
        return WikiListHistCrud.Insert(wikiListHist);
    }

    ///<summary>Deletes all WikiListHists before the given cutoff date. Returns the number of entries deleted.</summary>
    public static long DeleteBeforeDate(DateTime dateCutoff)
    {
        var command = $"Delete FROM wikilisthist WHERE DateTimeSaved <= {SOut.DateT(dateCutoff)} ";
        return Db.NonQ(command);
    }

    /// <summary>
    ///     Does not save to DB. Return null if listName does not exist.
    ///     Pass in the userod.UserNum of the user that is making the change.  Typically Security.CurUser.UserNum.
    ///     Security.CurUser cannot be used within this method due to the server side of middle tier.
    /// </summary>
    public static WikiListHist GenerateFromName(string listName, long userNum)
    {
        if (!WikiLists.CheckExists(listName)) return null;

        var wikiListHist = new WikiListHist();
        wikiListHist.UserNum = userNum;
        wikiListHist.ListName = listName;
        wikiListHist.DateTimeSaved = DateTime.Now;
        var table = WikiLists.GetByName(listName);
        table.TableName = listName; //required for xmlwriter
        using (var writer = new StringWriter())
        {
            table.WriteXml(writer, XmlWriteMode.WriteSchema);
            wikiListHist.ListContent = writer.ToString();
        }

        var listWikiListHeaderWidths = WikiListHeaderWidths.GetForList(listName);
        if (listWikiListHeaderWidths.Count > 0) wikiListHist.ListHeaders = string.Join(";", listWikiListHeaderWidths.Select(x => x.ColName + "," + x.ColWidth));

        return wikiListHist;
    }

    /// <summary>
    ///     Drops table in DB.  Recreates Table, then fills with Data.
    ///     Pass in the userod.UserNum of the user that is making the change.  Typically Security.CurUser.UserNum.
    /// </summary>
    public static void RevertFrom(WikiListHist wikiListHist, long userNum)
    {
        if (!WikiLists.CheckExists(wikiListHist.ListName)) return; //should never happen.

        Insert(GenerateFromName(wikiListHist.ListName, userNum)); //Save current wiki list content to the history
        var listWikiListHeaderWidths = WikiListHeaderWidths.GetFromListHist(wikiListHist); //Load header data
        WikiLists.CreateNewWikiList(wikiListHist.ListName, listWikiListHeaderWidths, true); //dropTableIfExists=true, so the existing table and HeaderWidth rows will be dropped
        var tableRevertedContent = new DataTable();
        using (var sr = new StringReader(wikiListHist.ListContent))
        using (var xmlReader = XmlReader.Create(sr))
        {
            tableRevertedContent.ReadXml(xmlReader);
        }

        var commandStart = $@"INSERT INTO wikilist_{SOut.String(wikiListHist.ListName)} ({string.Join(",", listWikiListHeaderWidths.Select(x => SOut.String(x.ColName)))})
				VALUES ";
        var stringBuilder = new StringBuilder(commandStart);
        var strComma = "";
        for (var i = 0; i < tableRevertedContent.Rows.Count; i++)
        {
            var strRow = "(" + string.Join(",", tableRevertedContent.Rows[i].ItemArray.Select(x => "'" + SOut.String(x.ToString()) + "'")) + ")";
            if (stringBuilder.Length + strRow.Length + 1 > TableBase.MaxAllowedPacketCount)
            {
                Db.NonQ(stringBuilder.ToString());
                stringBuilder = new StringBuilder(commandStart);
                strComma = "";
            }

            stringBuilder.Append(strComma + strRow);
            strComma = ",";
            if (i == tableRevertedContent.Rows.Count - 1) Db.NonQ(stringBuilder.ToString());
        }
    }

    /// <summary>
    ///     Checks remoting roles. Does not check permissions. Does not check for existing listname. If listname already
    ///     exists it will "merge" the history.
    /// </summary>
    public static void Rename(string wikiListCurName, string wikiListNewName)
    {
        var command = "UPDATE wikilisthist SET ListName = '" + SOut.String(wikiListNewName) + "' WHERE ListName='" + SOut.String(wikiListCurName) + "'";
        Db.NonQ(command);
    }
}