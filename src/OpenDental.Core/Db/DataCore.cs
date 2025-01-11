using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

namespace OpenDentBusiness;

public class DataCore
{
    public static DataTable GetTable(string commandText)
    {
        using var dataConnection = new DataConnection();

        return dataConnection.GetTable(commandText);
    }

    public static List<T> GetList<T>(string commandText, Func<IDataRecord, T> parser)
    {
        using var dataConnection = new DataConnection();

        return dataConnection.GetList(commandText, parser);
    }

    public static long NonQ(string command, bool getInsertId, params OdSqlParameter[] parameters)
    {
        using var dataConnection = new DataConnection();

        var result = dataConnection.NonQ(command, parameters.Select(x => x.GetMySqlParameter()).ToArray());

        if (getInsertId)
        {
            result = dataConnection.InsertId;
        }

        return result;
    }

    public static string GetScalar(string command)
    {
        using var dataConnection = new DataConnection();

        return dataConnection.GetScalar(command);
    }
}