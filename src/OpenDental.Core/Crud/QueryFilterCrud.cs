using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class QueryFilterCrud
{
    public static List<QueryFilter> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<QueryFilter> TableToList(DataTable table)
    {
        var retVal = new List<QueryFilter>();
        foreach (DataRow row in table.Rows)
        {
            var queryFilter = new QueryFilter
            {
                QueryFilterNum = SIn.Long(row["QueryFilterNum"].ToString()),
                GroupName = SIn.String(row["GroupName"].ToString()),
                FilterText = SIn.String(row["FilterText"].ToString())
            };
            retVal.Add(queryFilter);
        }

        return retVal;
    }

    public static void Insert(QueryFilter queryFilter)
    {
        var command = "INSERT INTO queryfilter (";

        command += "GroupName,FilterText) VALUES(";

        command +=
            "'" + SOut.String(queryFilter.GroupName) + "',"
            + "'" + SOut.String(queryFilter.FilterText) + "')";
        {
            queryFilter.QueryFilterNum = Db.NonQ(command, true, "QueryFilterNum", "queryFilter");
        }
    }

    public static void Update(QueryFilter queryFilter)
    {
        var command = "UPDATE queryfilter SET "
                      + "GroupName     = '" + SOut.String(queryFilter.GroupName) + "', "
                      + "FilterText    = '" + SOut.String(queryFilter.FilterText) + "' "
                      + "WHERE QueryFilterNum = " + SOut.Long(queryFilter.QueryFilterNum);
        Db.NonQ(command);
    }

    public static void Delete(long queryFilterNum)
    {
        var command = "DELETE FROM queryfilter "
                      + "WHERE QueryFilterNum = " + SOut.Long(queryFilterNum);
        Db.NonQ(command);
    }
}