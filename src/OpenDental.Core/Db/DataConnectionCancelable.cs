using System;
using System.Collections.Concurrent;
using System.Data;
using DataConnectionBase;
using MySqlConnector;

namespace OpenDentBusiness;

public class DataConnectionCancelable
{
    private static readonly ConcurrentDictionary<int, MySqlConnection> Connections = new();

    public static int GetServerThread()
    {
        var connectionString = DataConnection.GetConnectionString();
        var connection = new MySqlConnection(connectionString + ";pooling=false");

        connection.Open();

        var serverThread = connection.ServerThread;

        if (Connections.TryAdd(serverThread, connection))
        {
            return serverThread;
        }

        connection.Close();

        throw new ApplicationException("Critical error in GetServerThread: A duplicate connection was found via the server thread ID.");
    }

    public static DataTable GetTableConAlreadyOpen(int serverThread, string commandText, bool wasSqlValidated, bool isSqlAllowedReportServer = false, bool hasStackTrace = false, bool suppressMessage = false)
    {
        if (!Connections.TryGetValue(serverThread, out var connection))
        {
            throw new ApplicationException("Critical error in GetTableConAlreadyOpen: A connection could not be found via the given server thread ID.");
        }

        if (!wasSqlValidated)
        {
            throw new ApplicationException("Error: Command is either not safe or user does not have permission.");
        }

        var dataTable = new DataTable();
        var dataAdapter = new MySqlDataAdapter(new MySqlCommand(commandText, connection));

        try
        {
            QueryMonitor.Instance.RunMonitoredQuery(() => dataAdapter.Fill(dataTable), dataAdapter.SelectCommand, hasStackTrace);
        }
        finally
        {
            connection.Close();

            Connections.TryRemove(serverThread, out _);
        }

        return dataTable;
    }

    public static void CancelQuery(int serverThread)
    {
        if (!Connections.ContainsKey(serverThread))
        {
            return;
        }

        try
        {
            Db.NonQ("KILL QUERY " + serverThread);
        }
        finally
        {
            Connections.TryRemove(serverThread, out _);
        }
    }
}