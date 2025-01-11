using System;
using System.Collections.Generic;
using System.Data;
using MySqlConnector;

namespace DataConnectionBase;

public class DataConnection : IDisposable
{
    private static string ConnectionString { get; set; } = "";

    private static int _commandTimeout;
    
    public long InsertId;

    public static int CommandTimeout
    {
        get
        {
            if (_commandTimeout == 0)
            {
                _commandTimeout = 3600;
            }

            return _commandTimeout;
        }
        set => _commandTimeout = value;
    }

    public static int ConnectionRetryTimeoutSeconds { get; set; }
    
    public static string Database => new MySqlConnectionStringBuilder(ConnectionString).Database;

    public static string GetConnectionString()
    {
        return ConnectionString;
    }

    private readonly string _connectionString;

    public DataConnection()
    {
        _connectionString = ConnectionString;
    }

    public DataConnection(string server, string database, string userId, string password, string sslCa = "")
    {
        _connectionString = MakeConnectionString(server, database, userId, password, sslCa);
    }

    public static string MakeConnectionString(string server, string database, string userId, string password, string sslCa = "")
    {
        var port = 3306u;

        if (server.Contains(":"))
        {
            var parts = server.Split([':'], StringSplitOptions.RemoveEmptyEntries);

            server = parts[0];

            if (ushort.TryParse(parts[1], out var result))
            {
                port = result;
            }
        }

        var connectionStringBuilder = new MySqlConnectionStringBuilder
        {
            Server = server,
            Port = port,
            Database = database,
            UserID = userId,
            Password = password,
            CharacterSet = "utf8",
            TreatTinyAsBoolean = false,
            AllowUserVariables = true,
            ConvertZeroDateTime = true,
            DefaultCommandTimeout = (uint) CommandTimeout
        };

        if (string.IsNullOrEmpty(sslCa))
        {
            connectionStringBuilder.SslMode = MySqlSslMode.None;
        }
        else
        {
            connectionStringBuilder.SslMode = MySqlSslMode.VerifyCA;
            connectionStringBuilder.SslCa = sslCa;
        }

        return connectionStringBuilder.ToString();
    }

    public static void SetDb(string server, string database, string userId, string password, bool skipValidation = false, string sslCa = "")
    {
        SetDb(MakeConnectionString(server, database, userId, password, sslCa), skipValidation);
    }

    public static void SetDb(string connectionString, bool skipValidation = false)
    {
        TestConnection(connectionString, skipValidation);

        ConnectionString = connectionString;
    }

    private static void TestConnection(string connectionString, bool skipValidation)
    {
        using var connection = new MySqlConnection(connectionString);
        using var command = connection.CreateCommand();

        connection.Open();

        if (!skipValidation)
        {
            command.CommandText = "UPDATE preference SET Comments = '' WHERE PrefName = 'DataBaseVersion'";

            QueryMonitor.Instance.RunMonitoredQuery(() => command.ExecuteNonQuery(), command);
        }

        connection.Close();
    }

    public DataTable GetTable(string commandText)
    {
        var dataTable = new DataTable();

        using var connection = new MySqlConnection(_connectionString);

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = commandText;

        using var dataAdapter = new MySqlDataAdapter(command);

        QueryMonitor.Instance.RunMonitoredQuery(() => dataAdapter.Fill(dataTable), command);

        return dataTable;
    }

    public List<T> GetList<T>(string commandText, Func<IDataRecord, T> reader)
    {
        var list = new List<T>();

        using var connection = new MySqlConnection(_connectionString);

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = commandText;

        QueryMonitor.Instance.RunMonitoredQuery(() =>
        {
            using var dataReader = command.ExecuteReader();

            while (dataReader.Read())
            {
                list.Add(reader(dataReader));
            }
        }, command);

        return list;
    }

    public long NonQ(string commandText, params MySqlParameter[] parameters)
    {
        long rowsChanged = 0;

        using var connection = new MySqlConnection(_connectionString);

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = commandText;
        foreach (var p in parameters)
        {
            command.Parameters.Add(p);
        }

        QueryMonitor.Instance.RunMonitoredQuery(() => rowsChanged = command.ExecuteNonQuery(), command);

        InsertId = command.LastInsertedId;

        return rowsChanged;
    }

    public string GetScalar(string commandText)
    {
        object scalar = null;
        
        using var connection = new MySqlConnection(_connectionString);

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = commandText;

        QueryMonitor.Instance.RunMonitoredQuery(() => scalar = command.ExecuteScalar(), command);

        return scalar?.ToString() ?? string.Empty;
    }

    public void Dispose()
    {
    }
}

public enum DatabaseType
{
    MySql
}