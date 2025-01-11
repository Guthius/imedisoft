#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class QueryFilterCrud
{
    public static QueryFilter SelectOne(long queryFilterNum)
    {
        var command = "SELECT * FROM queryfilter "
                      + "WHERE QueryFilterNum = " + SOut.Long(queryFilterNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static QueryFilter SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<QueryFilter> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<QueryFilter> TableToList(DataTable table)
    {
        var retVal = new List<QueryFilter>();
        QueryFilter queryFilter;
        foreach (DataRow row in table.Rows)
        {
            queryFilter = new QueryFilter();
            queryFilter.QueryFilterNum = SIn.Long(row["QueryFilterNum"].ToString());
            queryFilter.GroupName = SIn.String(row["GroupName"].ToString());
            queryFilter.FilterText = SIn.String(row["FilterText"].ToString());
            retVal.Add(queryFilter);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<QueryFilter> listQueryFilters, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "QueryFilter";
        var table = new DataTable(tableName);
        table.Columns.Add("QueryFilterNum");
        table.Columns.Add("GroupName");
        table.Columns.Add("FilterText");
        foreach (var queryFilter in listQueryFilters)
            table.Rows.Add(SOut.Long(queryFilter.QueryFilterNum), queryFilter.GroupName, queryFilter.FilterText);
        return table;
    }

    public static long Insert(QueryFilter queryFilter)
    {
        return Insert(queryFilter, false);
    }

    public static long Insert(QueryFilter queryFilter, bool useExistingPK)
    {
        var command = "INSERT INTO queryfilter (";

        command += "GroupName,FilterText) VALUES(";

        command +=
            "'" + SOut.String(queryFilter.GroupName) + "',"
            + "'" + SOut.String(queryFilter.FilterText) + "')";
        {
            queryFilter.QueryFilterNum = Db.NonQ(command, true, "QueryFilterNum", "queryFilter");
        }
        return queryFilter.QueryFilterNum;
    }

    public static long InsertNoCache(QueryFilter queryFilter)
    {
        return InsertNoCache(queryFilter, false);
    }

    public static long InsertNoCache(QueryFilter queryFilter, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO queryfilter (";
        if (isRandomKeys || useExistingPK) command += "QueryFilterNum,";
        command += "GroupName,FilterText) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(queryFilter.QueryFilterNum) + ",";
        command +=
            "'" + SOut.String(queryFilter.GroupName) + "',"
            + "'" + SOut.String(queryFilter.FilterText) + "')";
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command);
        else
            queryFilter.QueryFilterNum = Db.NonQ(command, true, "QueryFilterNum", "queryFilter");
        return queryFilter.QueryFilterNum;
    }

    public static void Update(QueryFilter queryFilter)
    {
        var command = "UPDATE queryfilter SET "
                      + "GroupName     = '" + SOut.String(queryFilter.GroupName) + "', "
                      + "FilterText    = '" + SOut.String(queryFilter.FilterText) + "' "
                      + "WHERE QueryFilterNum = " + SOut.Long(queryFilter.QueryFilterNum);
        Db.NonQ(command);
    }

    public static bool Update(QueryFilter queryFilter, QueryFilter oldQueryFilter)
    {
        var command = "";
        if (queryFilter.GroupName != oldQueryFilter.GroupName)
        {
            if (command != "") command += ",";
            command += "GroupName = '" + SOut.String(queryFilter.GroupName) + "'";
        }

        if (queryFilter.FilterText != oldQueryFilter.FilterText)
        {
            if (command != "") command += ",";
            command += "FilterText = '" + SOut.String(queryFilter.FilterText) + "'";
        }

        if (command == "") return false;
        command = "UPDATE queryfilter SET " + command
                                            + " WHERE QueryFilterNum = " + SOut.Long(queryFilter.QueryFilterNum);
        Db.NonQ(command);
        return true;
    }

    public static bool UpdateComparison(QueryFilter queryFilter, QueryFilter oldQueryFilter)
    {
        if (queryFilter.GroupName != oldQueryFilter.GroupName) return true;
        if (queryFilter.FilterText != oldQueryFilter.FilterText) return true;
        return false;
    }

    public static void Delete(long queryFilterNum)
    {
        var command = "DELETE FROM queryfilter "
                      + "WHERE QueryFilterNum = " + SOut.Long(queryFilterNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listQueryFilterNums)
    {
        if (listQueryFilterNums == null || listQueryFilterNums.Count == 0) return;
        var command = "DELETE FROM queryfilter "
                      + "WHERE QueryFilterNum IN(" + string.Join(",", listQueryFilterNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}