#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class WikiListHeaderWidthCrud
{
    public static WikiListHeaderWidth SelectOne(long wikiListHeaderWidthNum)
    {
        var command = "SELECT * FROM wikilistheaderwidth "
                      + "WHERE WikiListHeaderWidthNum = " + SOut.Long(wikiListHeaderWidthNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static WikiListHeaderWidth SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<WikiListHeaderWidth> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<WikiListHeaderWidth> TableToList(DataTable table)
    {
        var retVal = new List<WikiListHeaderWidth>();
        WikiListHeaderWidth wikiListHeaderWidth;
        foreach (DataRow row in table.Rows)
        {
            wikiListHeaderWidth = new WikiListHeaderWidth();
            wikiListHeaderWidth.WikiListHeaderWidthNum = SIn.Long(row["WikiListHeaderWidthNum"].ToString());
            wikiListHeaderWidth.ListName = SIn.String(row["ListName"].ToString());
            wikiListHeaderWidth.ColName = SIn.String(row["ColName"].ToString());
            wikiListHeaderWidth.ColWidth = SIn.Int(row["ColWidth"].ToString());
            wikiListHeaderWidth.PickList = SIn.String(row["PickList"].ToString());
            wikiListHeaderWidth.IsHidden = SIn.Bool(row["IsHidden"].ToString());
            retVal.Add(wikiListHeaderWidth);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<WikiListHeaderWidth> listWikiListHeaderWidths, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "WikiListHeaderWidth";
        var table = new DataTable(tableName);
        table.Columns.Add("WikiListHeaderWidthNum");
        table.Columns.Add("ListName");
        table.Columns.Add("ColName");
        table.Columns.Add("ColWidth");
        table.Columns.Add("PickList");
        table.Columns.Add("IsHidden");
        foreach (var wikiListHeaderWidth in listWikiListHeaderWidths)
            table.Rows.Add(SOut.Long(wikiListHeaderWidth.WikiListHeaderWidthNum), wikiListHeaderWidth.ListName, wikiListHeaderWidth.ColName, SOut.Int(wikiListHeaderWidth.ColWidth), wikiListHeaderWidth.PickList, SOut.Bool(wikiListHeaderWidth.IsHidden));
        return table;
    }

    public static long Insert(WikiListHeaderWidth wikiListHeaderWidth)
    {
        return Insert(wikiListHeaderWidth, false);
    }

    public static long Insert(WikiListHeaderWidth wikiListHeaderWidth, bool useExistingPK)
    {
        var command = "INSERT INTO wikilistheaderwidth (";

        command += "ListName,ColName,ColWidth,PickList,IsHidden) VALUES(";

        command +=
            "'" + SOut.String(wikiListHeaderWidth.ListName) + "',"
            + "'" + SOut.String(wikiListHeaderWidth.ColName) + "',"
            + SOut.Int(wikiListHeaderWidth.ColWidth) + ","
            + DbHelper.ParamChar + "paramPickList,"
            + SOut.Bool(wikiListHeaderWidth.IsHidden) + ")";
        if (wikiListHeaderWidth.PickList == null) wikiListHeaderWidth.PickList = "";
        var paramPickList = new OdSqlParameter("paramPickList", OdDbType.Text, SOut.StringParam(wikiListHeaderWidth.PickList));
        {
            wikiListHeaderWidth.WikiListHeaderWidthNum = Db.NonQ(command, true, "WikiListHeaderWidthNum", "wikiListHeaderWidth", paramPickList);
        }
        return wikiListHeaderWidth.WikiListHeaderWidthNum;
    }

    public static void InsertMany(List<WikiListHeaderWidth> listWikiListHeaderWidths)
    {
        InsertMany(listWikiListHeaderWidths, false);
    }

    public static void InsertMany(List<WikiListHeaderWidth> listWikiListHeaderWidths, bool useExistingPK)
    {
        StringBuilder sbCommands = null;
        var index = 0;
        var countRows = 0;
        while (index < listWikiListHeaderWidths.Count)
        {
            var wikiListHeaderWidth = listWikiListHeaderWidths[index];
            var sbRow = new StringBuilder("(");
            var hasComma = false;
            if (sbCommands == null)
            {
                sbCommands = new StringBuilder();
                sbCommands.Append("INSERT INTO wikilistheaderwidth (");
                if (useExistingPK) sbCommands.Append("WikiListHeaderWidthNum,");
                sbCommands.Append("ListName,ColName,ColWidth,PickList,IsHidden) VALUES ");
                countRows = 0;
            }
            else
            {
                hasComma = true;
            }

            if (useExistingPK)
            {
                sbRow.Append(SOut.Long(wikiListHeaderWidth.WikiListHeaderWidthNum));
                sbRow.Append(",");
            }

            sbRow.Append("'" + SOut.String(wikiListHeaderWidth.ListName) + "'");
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(wikiListHeaderWidth.ColName) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Int(wikiListHeaderWidth.ColWidth));
            sbRow.Append(",");
            sbRow.Append("'" + SOut.String(wikiListHeaderWidth.PickList) + "'");
            sbRow.Append(",");
            sbRow.Append(SOut.Bool(wikiListHeaderWidth.IsHidden));
            sbRow.Append(")");
            if (sbCommands.Length + sbRow.Length + 1 > TableBase.MaxAllowedPacketCount && countRows > 0)
            {
                Db.NonQ(sbCommands.ToString());
                sbCommands = null;
            }
            else
            {
                if (hasComma) sbCommands.Append(",");
                sbCommands.Append(sbRow);
                countRows++;
                if (index == listWikiListHeaderWidths.Count - 1) Db.NonQ(sbCommands.ToString());
                index++;
            }
        }
    }

    public static long InsertNoCache(WikiListHeaderWidth wikiListHeaderWidth)
    {
        return InsertNoCache(wikiListHeaderWidth, false);
    }

    public static long InsertNoCache(WikiListHeaderWidth wikiListHeaderWidth, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO wikilistheaderwidth (";
        if (isRandomKeys || useExistingPK) command += "WikiListHeaderWidthNum,";
        command += "ListName,ColName,ColWidth,PickList,IsHidden) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(wikiListHeaderWidth.WikiListHeaderWidthNum) + ",";
        command +=
            "'" + SOut.String(wikiListHeaderWidth.ListName) + "',"
            + "'" + SOut.String(wikiListHeaderWidth.ColName) + "',"
            + SOut.Int(wikiListHeaderWidth.ColWidth) + ","
            + DbHelper.ParamChar + "paramPickList,"
            + SOut.Bool(wikiListHeaderWidth.IsHidden) + ")";
        if (wikiListHeaderWidth.PickList == null) wikiListHeaderWidth.PickList = "";
        var paramPickList = new OdSqlParameter("paramPickList", OdDbType.Text, SOut.StringParam(wikiListHeaderWidth.PickList));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramPickList);
        else
            wikiListHeaderWidth.WikiListHeaderWidthNum = Db.NonQ(command, true, "WikiListHeaderWidthNum", "wikiListHeaderWidth", paramPickList);
        return wikiListHeaderWidth.WikiListHeaderWidthNum;
    }

    public static void Update(WikiListHeaderWidth wikiListHeaderWidth)
    {
        var command = "UPDATE wikilistheaderwidth SET "
                      + "ListName              = '" + SOut.String(wikiListHeaderWidth.ListName) + "', "
                      + "ColName               = '" + SOut.String(wikiListHeaderWidth.ColName) + "', "
                      + "ColWidth              =  " + SOut.Int(wikiListHeaderWidth.ColWidth) + ", "
                      + "PickList              =  " + DbHelper.ParamChar + "paramPickList, "
                      + "IsHidden              =  " + SOut.Bool(wikiListHeaderWidth.IsHidden) + " "
                      + "WHERE WikiListHeaderWidthNum = " + SOut.Long(wikiListHeaderWidth.WikiListHeaderWidthNum);
        if (wikiListHeaderWidth.PickList == null) wikiListHeaderWidth.PickList = "";
        var paramPickList = new OdSqlParameter("paramPickList", OdDbType.Text, SOut.StringParam(wikiListHeaderWidth.PickList));
        Db.NonQ(command, paramPickList);
    }

    public static bool Update(WikiListHeaderWidth wikiListHeaderWidth, WikiListHeaderWidth oldWikiListHeaderWidth)
    {
        var command = "";
        if (wikiListHeaderWidth.ListName != oldWikiListHeaderWidth.ListName)
        {
            if (command != "") command += ",";
            command += "ListName = '" + SOut.String(wikiListHeaderWidth.ListName) + "'";
        }

        if (wikiListHeaderWidth.ColName != oldWikiListHeaderWidth.ColName)
        {
            if (command != "") command += ",";
            command += "ColName = '" + SOut.String(wikiListHeaderWidth.ColName) + "'";
        }

        if (wikiListHeaderWidth.ColWidth != oldWikiListHeaderWidth.ColWidth)
        {
            if (command != "") command += ",";
            command += "ColWidth = " + SOut.Int(wikiListHeaderWidth.ColWidth) + "";
        }

        if (wikiListHeaderWidth.PickList != oldWikiListHeaderWidth.PickList)
        {
            if (command != "") command += ",";
            command += "PickList = " + DbHelper.ParamChar + "paramPickList";
        }

        if (wikiListHeaderWidth.IsHidden != oldWikiListHeaderWidth.IsHidden)
        {
            if (command != "") command += ",";
            command += "IsHidden = " + SOut.Bool(wikiListHeaderWidth.IsHidden) + "";
        }

        if (command == "") return false;
        if (wikiListHeaderWidth.PickList == null) wikiListHeaderWidth.PickList = "";
        var paramPickList = new OdSqlParameter("paramPickList", OdDbType.Text, SOut.StringParam(wikiListHeaderWidth.PickList));
        command = "UPDATE wikilistheaderwidth SET " + command
                                                    + " WHERE WikiListHeaderWidthNum = " + SOut.Long(wikiListHeaderWidth.WikiListHeaderWidthNum);
        Db.NonQ(command, paramPickList);
        return true;
    }

    public static bool UpdateComparison(WikiListHeaderWidth wikiListHeaderWidth, WikiListHeaderWidth oldWikiListHeaderWidth)
    {
        if (wikiListHeaderWidth.ListName != oldWikiListHeaderWidth.ListName) return true;
        if (wikiListHeaderWidth.ColName != oldWikiListHeaderWidth.ColName) return true;
        if (wikiListHeaderWidth.ColWidth != oldWikiListHeaderWidth.ColWidth) return true;
        if (wikiListHeaderWidth.PickList != oldWikiListHeaderWidth.PickList) return true;
        if (wikiListHeaderWidth.IsHidden != oldWikiListHeaderWidth.IsHidden) return true;
        return false;
    }

    public static void Delete(long wikiListHeaderWidthNum)
    {
        var command = "DELETE FROM wikilistheaderwidth "
                      + "WHERE WikiListHeaderWidthNum = " + SOut.Long(wikiListHeaderWidthNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listWikiListHeaderWidthNums)
    {
        if (listWikiListHeaderWidthNums == null || listWikiListHeaderWidthNums.Count == 0) return;
        var command = "DELETE FROM wikilistheaderwidth "
                      + "WHERE WikiListHeaderWidthNum IN(" + string.Join(",", listWikiListHeaderWidthNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}