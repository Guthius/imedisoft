using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class WikiListHeaderWidths
{
    ///<summary>Used temporarily.</summary>
    public static string DummyColName = "Xrxzes";

    /// <summary>
    ///     Returns header widths for list sorted in the same order as the columns appear in the DB. Can be more efficient
    ///     than using cache.
    /// </summary>
    public static List<WikiListHeaderWidth> GetForListNoCache(string listName)
    {
        var listWikiListHeaderWidthsRet = new List<WikiListHeaderWidth>();
        var listWikiListHeaderWidthsTemp = new List<WikiListHeaderWidth>();
        var command = "DESCRIBE wikilist_" + SOut.String(listName);
        var tableDescriptions = DataCore.GetTable(command);
        command = "SELECT * FROM wikilistheaderwidth WHERE ListName='" + SOut.String(listName) + "'";
        listWikiListHeaderWidthsTemp = WikiListHeaderWidthCrud.SelectMany(command);
        for (var i = 0; i < tableDescriptions.Rows.Count; i++)
        for (var j = 0; j < listWikiListHeaderWidthsTemp.Count; j++)
            //Add WikiListHeaderWidth from listWikiListHeaderWidthsTemp to listWikiListHeaderWidthsRet if it is the next row in tableDescriptions.
            if (tableDescriptions.Rows[i][0].ToString() == listWikiListHeaderWidthsTemp[j].ColName)
            {
                listWikiListHeaderWidthsRet.Add(listWikiListHeaderWidthsTemp[j]);
                break;
            }

        //next description row.
        return listWikiListHeaderWidthsRet;
    }

    ///<summary>Returns header widths for list sorted in the same order as the columns appear in the DB.  Uses cache.</summary>
    public static List<WikiListHeaderWidth> GetForList(string listName)
    {
        var listWikiListHeaderWidthsRet = new List<WikiListHeaderWidth>();
        var command = "DESCRIBE wikilist_" + SOut.String(listName);
        var tableDescripts = DataCore.GetTable(command); //Includes PK, and it's in the correct order.
        var listWikiListHeaderWidths = GetWhere(x => x.ListName == listName); //also includes PK, but it can be out of order.
        for (var i = 0; i < tableDescripts.Rows.Count; i++)
        {
            //known to be in correct order
            var wikiListHeaderWidth = listWikiListHeaderWidths.Find(x => x.ColName == tableDescripts.Rows[i][0].ToString());
            //if it's null, it will crash. We always allow crashes when there's db corruption.
            listWikiListHeaderWidthsRet.Add(wikiListHeaderWidth);
        }

        return listWikiListHeaderWidthsRet;
    }

    ///<summary>Also alters the db table for the list itself.  Throws exception if number of columns does not match.</summary>
    public static void UpdateNamesAndWidths(string listName, List<WikiListHeaderWidth> listWikiListHeaderWidths)
    {
        var command = "DESCRIBE wikilist_" + SOut.String(listName);
        var tableListDescriptions = DataCore.GetTable(command);
        if (tableListDescriptions.Rows.Count != listWikiListHeaderWidths.Count) throw new ApplicationException("List schema has been altered. Unable to save changes to list.");

        var listChanges = new List<string>();
        //rename Columns with dummy names in case user is renaming a new column with an old name.---------------------------------------------
        for (var i = 1; i < tableListDescriptions.Rows.Count; i++)
        {
            //start with index 1 to skip PK col
            var dataRow = tableListDescriptions.Rows[i];
            listChanges.Add($"CHANGE {SOut.String(dataRow[0].ToString())} {SOut.String(DummyColName + i)} TEXT NOT NULL");
            command = $@"UPDATE wikilistheaderwidth SET ColName='{SOut.String(DummyColName + i)}'
					WHERE ListName='{SOut.String(listName)}'
					AND ColName='{SOut.String(dataRow[0].ToString())}'";
            Db.NonQ(command);
        }

        Db.NonQ($"ALTER TABLE wikilist_{SOut.String(listName)} {string.Join(",", listChanges)}");
        listChanges = new List<string>();
        //rename columns names and widths-------------------------------------------------------------------------------------------------------
        for (var i = 1; i < tableListDescriptions.Rows.Count; i++)
        {
            //start with index 1 to skip PK col
            var wikiListHeaderWidth = listWikiListHeaderWidths[i];
            listChanges.Add($"CHANGE {SOut.String(DummyColName + i)} {SOut.String(wikiListHeaderWidth.ColName)} TEXT NOT NULL");
            command = $@"UPDATE wikilistheaderwidth
					SET ColName='{SOut.String(wikiListHeaderWidth.ColName)}',
						ColWidth='{SOut.Int(wikiListHeaderWidth.ColWidth)}',
						PickList='{SOut.String(wikiListHeaderWidth.PickList)}',
						IsHidden={SOut.Bool(wikiListHeaderWidth.IsHidden)}
					WHERE ListName='{SOut.String(listName)}'
					AND ColName='{SOut.String(DummyColName + i)}'";
            Db.NonQ(command);
        }

        Db.NonQ($"ALTER TABLE wikilist_{SOut.String(listName)} {string.Join(",", listChanges)}");
        //handle width of PK seperately because we do not rename the PK column, ever.
        command = $@"UPDATE wikilistheaderwidth
				SET ColWidth='{SOut.Int(listWikiListHeaderWidths[0].ColWidth)}',
					PickList='{SOut.String(listWikiListHeaderWidths[0].PickList)}',
					IsHidden={SOut.Bool(listWikiListHeaderWidths[0].IsHidden)}
				WHERE ListName='{SOut.String(listName)}'
				AND ColName='{SOut.String(listWikiListHeaderWidths[0].ColName)}'";
        Db.NonQ(command);
        RefreshCache();
    }

    ///<summary>No error checking. Only called from WikiLists.</summary>
    public static void InsertNew(WikiListHeaderWidth wikiListHeaderWidth)
    {
        WikiListHeaderWidthCrud.Insert(wikiListHeaderWidth);
        RefreshCache();
    }

    public static void InsertMany(List<WikiListHeaderWidth> listWikiListHeaderWidth)
    {
        WikiListHeaderWidthCrud.InsertMany(listWikiListHeaderWidth);
        RefreshCache();
    }

    /// <summary>
    ///     No error checking. Only called from WikiLists after the corresponding column has been dropped from its
    ///     respective table.
    /// </summary>
    public static void Delete(string listName, string colName)
    {
        var command = "DELETE FROM wikilistheaderwidth WHERE ListName='" + SOut.String(listName) + "' AND ColName='" + SOut.String(colName) + "'";
        Db.NonQ(command);
        RefreshCache();
    }

    public static void DeleteForList(string listName)
    {
        var command = "DELETE FROM wikilistheaderwidth WHERE ListName='" + SOut.String(listName) + "'";
        Db.NonQ(command);
        RefreshCache();
    }

    public static List<WikiListHeaderWidth> GetFromListHist(WikiListHist wikiListHist)
    {
        var listWikiListHeaderWidths = new List<WikiListHeaderWidth>();
        var arrayHeaders = wikiListHist.ListHeaders.Split(';');
        for (var i = 0; i < arrayHeaders.Length; i++)
        {
            var wikiListHeaderWidth = new WikiListHeaderWidth();
            wikiListHeaderWidth.ListName = wikiListHist.ListName;
            wikiListHeaderWidth.ColName = arrayHeaders[i].Split(',')[0];
            wikiListHeaderWidth.ColWidth = SIn.Int(arrayHeaders[i].Split(',')[1]);
            listWikiListHeaderWidths.Add(wikiListHeaderWidth);
        }

        return listWikiListHeaderWidths;
    }

    #region CachePattern

    private class WikiListHeaderWidthCache : CacheListAbs<WikiListHeaderWidth>
    {
        protected override List<WikiListHeaderWidth> GetCacheFromDb()
        {
            var command = "SELECT * FROM wikilistheaderwidth";
            return WikiListHeaderWidthCrud.SelectMany(command);
        }

        protected override List<WikiListHeaderWidth> TableToList(DataTable dataTable)
        {
            return WikiListHeaderWidthCrud.TableToList(dataTable);
        }

        protected override WikiListHeaderWidth Copy(WikiListHeaderWidth item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<WikiListHeaderWidth> items)
        {
            return WikiListHeaderWidthCrud.ListToTable(items, "WikiListHeaderWidth");
        }

        protected override void FillCacheIfNeeded()
        {
            WikiListHeaderWidths.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly WikiListHeaderWidthCache _wikiListHeaderWidthCache = new();

    public static List<WikiListHeaderWidth> GetWhere(Predicate<WikiListHeaderWidth> match, bool isShort = false)
    {
        return _wikiListHeaderWidthCache.GetWhere(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _wikiListHeaderWidthCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _wikiListHeaderWidthCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _wikiListHeaderWidthCache.ClearCache();
    }

    #endregion
}