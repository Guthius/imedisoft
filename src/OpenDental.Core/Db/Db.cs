using System.Collections.Generic;
using DataConnectionBase;

namespace OpenDentBusiness;

public class Db
{
    public static bool HasDatabaseConnection()
    {
        return !string.IsNullOrEmpty(DataConnection.GetConnectionString());
    }

    internal static List<long> GetListLong(string command, bool hasExceptions = true)
    {
        var retVal = new List<long>();
        var table = DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++)
        {
            retVal.Add(SIn.Long(table.Rows[i][0].ToString(), hasExceptions));
        }

        return retVal;
    }

    internal static List<string> GetListString(string command)
    {
        var retVal = new List<string>();
        var table =  DataCore.GetTable(command);
        for (var i = 0; i < table.Rows.Count; i++)
        {
            retVal.Add(SIn.String(table.Rows[i][0].ToString()));
        }

        return retVal;
    }

    internal static long NonQ(string command, bool getInsertId, string columnNamePk, string tableName, params OdSqlParameter[] parameters)
    {
        return DataCore.NonQ(command, getInsertId, parameters);
    }

    internal static long NonQ(string command, bool getInsertId, params OdSqlParameter[] parameters)
    {
        return NonQ(command, getInsertId, "", "", parameters);
    }

    internal static long NonQ(string command, params OdSqlParameter[] parameters)
    {
        return NonQ(command, false, parameters);
    }

    internal static long GetLong(string command)
    {
        var table = DataCore.GetTable(command);
        return table.Rows.Count == 0 ? 0 : SIn.Long(table.Rows[0][0].ToString());
    }

    internal static string GetCount(string command)
    {
        return GetLong(command).ToString();
    }
}