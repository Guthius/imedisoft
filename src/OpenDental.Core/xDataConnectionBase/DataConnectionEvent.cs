using System;
using System.ComponentModel;
using CodeBase;

namespace DataConnectionBase;

public enum DataConnectionEventType
{
    [Description(
        "Connection to the MySQL server has been lost.  " +
        "Connectivity will be retried periodically.  " +
        "Click Retry to attempt to connect manually or Exit Program to close the program.")]
    ConnectionLost,

    [Description(
        "Too many connections have been made to the database.  " +
        "Consider increasing the max_connections variable in your my.ini file.  " +
        "Connectivity will be retried periodically.  Click Retry to attempt to connect manually or Exit Program to close the program.")]
    TooManyConnections,

    [Description("Connection Restored.")]
    ConnectionRestored,

    [Description(
        "Reading from MySQL has failed.  " +
        "Connectivity will be retried periodically.  " +
        "Click Retry to attempt to retry manually or Exit Program to close the program.")]
    DataReaderNull,
}
