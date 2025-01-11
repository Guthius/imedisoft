#region

using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataConnectionBase;

#endregion

namespace OpenDentBusiness.Crud;

public class ReplicationServerCrud
{
    public static ReplicationServer SelectOne(long replicationServerNum)
    {
        var command = "SELECT * FROM replicationserver "
                      + "WHERE ReplicationServerNum = " + SOut.Long(replicationServerNum);
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static ReplicationServer SelectOne(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        if (list.Count == 0) return null;
        return list[0];
    }

    public static List<ReplicationServer> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ReplicationServer> TableToList(DataTable table)
    {
        var retVal = new List<ReplicationServer>();
        ReplicationServer replicationServer;
        foreach (DataRow row in table.Rows)
        {
            replicationServer = new ReplicationServer();
            replicationServer.ReplicationServerNum = SIn.Long(row["ReplicationServerNum"].ToString());
            replicationServer.Descript = SIn.String(row["Descript"].ToString());
            replicationServer.ServerId = SIn.Int(row["ServerId"].ToString());
            replicationServer.RangeStart = SIn.Long(row["RangeStart"].ToString());
            replicationServer.RangeEnd = SIn.Long(row["RangeEnd"].ToString());
            replicationServer.AtoZpath = SIn.String(row["AtoZpath"].ToString());
            replicationServer.UpdateBlocked = SIn.Bool(row["UpdateBlocked"].ToString());
            replicationServer.SlaveMonitor = SIn.String(row["SlaveMonitor"].ToString());
            retVal.Add(replicationServer);
        }

        return retVal;
    }

    public static DataTable ListToTable(List<ReplicationServer> listReplicationServers, string tableName = "")
    {
        if (string.IsNullOrEmpty(tableName)) tableName = "ReplicationServer";
        var table = new DataTable(tableName);
        table.Columns.Add("ReplicationServerNum");
        table.Columns.Add("Descript");
        table.Columns.Add("ServerId");
        table.Columns.Add("RangeStart");
        table.Columns.Add("RangeEnd");
        table.Columns.Add("AtoZpath");
        table.Columns.Add("UpdateBlocked");
        table.Columns.Add("SlaveMonitor");
        foreach (var replicationServer in listReplicationServers)
            table.Rows.Add(SOut.Long(replicationServer.ReplicationServerNum), replicationServer.Descript, SOut.Int(replicationServer.ServerId), SOut.Long(replicationServer.RangeStart), SOut.Long(replicationServer.RangeEnd), replicationServer.AtoZpath, SOut.Bool(replicationServer.UpdateBlocked), replicationServer.SlaveMonitor);
        return table;
    }

    public static long Insert(ReplicationServer replicationServer)
    {
        return Insert(replicationServer, false);
    }

    public static long Insert(ReplicationServer replicationServer, bool useExistingPK)
    {
        var command = "INSERT INTO replicationserver (";

        command += "Descript,ServerId,RangeStart,RangeEnd,AtoZpath,UpdateBlocked,SlaveMonitor) VALUES(";

        command +=
            DbHelper.ParamChar + "paramDescript,"
                               + SOut.Int(replicationServer.ServerId) + ","
                               + SOut.Long(replicationServer.RangeStart) + ","
                               + SOut.Long(replicationServer.RangeEnd) + ","
                               + "'" + SOut.String(replicationServer.AtoZpath) + "',"
                               + SOut.Bool(replicationServer.UpdateBlocked) + ","
                               + "'" + SOut.String(replicationServer.SlaveMonitor) + "')";
        if (replicationServer.Descript == null) replicationServer.Descript = "";
        var paramDescript = new OdSqlParameter("paramDescript", OdDbType.Text, SOut.StringParam(replicationServer.Descript));
        {
            replicationServer.ReplicationServerNum = Db.NonQ(command, true, "ReplicationServerNum", "replicationServer", paramDescript);
        }
        return replicationServer.ReplicationServerNum;
    }

    public static long InsertNoCache(ReplicationServer replicationServer)
    {
        return InsertNoCache(replicationServer, false);
    }

    public static long InsertNoCache(ReplicationServer replicationServer, bool useExistingPK)
    {
        const bool isRandomKeys = false;
        var command = "INSERT INTO replicationserver (";
        if (isRandomKeys || useExistingPK) command += "ReplicationServerNum,";
        command += "Descript,ServerId,RangeStart,RangeEnd,AtoZpath,UpdateBlocked,SlaveMonitor) VALUES(";
        if (isRandomKeys || useExistingPK) command += SOut.Long(replicationServer.ReplicationServerNum) + ",";
        command +=
            DbHelper.ParamChar + "paramDescript,"
                               + SOut.Int(replicationServer.ServerId) + ","
                               + SOut.Long(replicationServer.RangeStart) + ","
                               + SOut.Long(replicationServer.RangeEnd) + ","
                               + "'" + SOut.String(replicationServer.AtoZpath) + "',"
                               + SOut.Bool(replicationServer.UpdateBlocked) + ","
                               + "'" + SOut.String(replicationServer.SlaveMonitor) + "')";
        if (replicationServer.Descript == null) replicationServer.Descript = "";
        var paramDescript = new OdSqlParameter("paramDescript", OdDbType.Text, SOut.StringParam(replicationServer.Descript));
        if (useExistingPK || isRandomKeys)
            Db.NonQ(command, paramDescript);
        else
            replicationServer.ReplicationServerNum = Db.NonQ(command, true, "ReplicationServerNum", "replicationServer", paramDescript);
        return replicationServer.ReplicationServerNum;
    }

    public static void Update(ReplicationServer replicationServer)
    {
        var command = "UPDATE replicationserver SET "
                      + "Descript            =  " + DbHelper.ParamChar + "paramDescript, "
                      + "ServerId            =  " + SOut.Int(replicationServer.ServerId) + ", "
                      + "RangeStart          =  " + SOut.Long(replicationServer.RangeStart) + ", "
                      + "RangeEnd            =  " + SOut.Long(replicationServer.RangeEnd) + ", "
                      + "AtoZpath            = '" + SOut.String(replicationServer.AtoZpath) + "', "
                      + "UpdateBlocked       =  " + SOut.Bool(replicationServer.UpdateBlocked) + ", "
                      + "SlaveMonitor        = '" + SOut.String(replicationServer.SlaveMonitor) + "' "
                      + "WHERE ReplicationServerNum = " + SOut.Long(replicationServer.ReplicationServerNum);
        if (replicationServer.Descript == null) replicationServer.Descript = "";
        var paramDescript = new OdSqlParameter("paramDescript", OdDbType.Text, SOut.StringParam(replicationServer.Descript));
        Db.NonQ(command, paramDescript);
    }

    public static bool Update(ReplicationServer replicationServer, ReplicationServer oldReplicationServer)
    {
        var command = "";
        if (replicationServer.Descript != oldReplicationServer.Descript)
        {
            if (command != "") command += ",";
            command += "Descript = " + DbHelper.ParamChar + "paramDescript";
        }

        if (replicationServer.ServerId != oldReplicationServer.ServerId)
        {
            if (command != "") command += ",";
            command += "ServerId = " + SOut.Int(replicationServer.ServerId) + "";
        }

        if (replicationServer.RangeStart != oldReplicationServer.RangeStart)
        {
            if (command != "") command += ",";
            command += "RangeStart = " + SOut.Long(replicationServer.RangeStart) + "";
        }

        if (replicationServer.RangeEnd != oldReplicationServer.RangeEnd)
        {
            if (command != "") command += ",";
            command += "RangeEnd = " + SOut.Long(replicationServer.RangeEnd) + "";
        }

        if (replicationServer.AtoZpath != oldReplicationServer.AtoZpath)
        {
            if (command != "") command += ",";
            command += "AtoZpath = '" + SOut.String(replicationServer.AtoZpath) + "'";
        }

        if (replicationServer.UpdateBlocked != oldReplicationServer.UpdateBlocked)
        {
            if (command != "") command += ",";
            command += "UpdateBlocked = " + SOut.Bool(replicationServer.UpdateBlocked) + "";
        }

        if (replicationServer.SlaveMonitor != oldReplicationServer.SlaveMonitor)
        {
            if (command != "") command += ",";
            command += "SlaveMonitor = '" + SOut.String(replicationServer.SlaveMonitor) + "'";
        }

        if (command == "") return false;
        if (replicationServer.Descript == null) replicationServer.Descript = "";
        var paramDescript = new OdSqlParameter("paramDescript", OdDbType.Text, SOut.StringParam(replicationServer.Descript));
        command = "UPDATE replicationserver SET " + command
                                                  + " WHERE ReplicationServerNum = " + SOut.Long(replicationServer.ReplicationServerNum);
        Db.NonQ(command, paramDescript);
        return true;
    }

    public static bool UpdateComparison(ReplicationServer replicationServer, ReplicationServer oldReplicationServer)
    {
        if (replicationServer.Descript != oldReplicationServer.Descript) return true;
        if (replicationServer.ServerId != oldReplicationServer.ServerId) return true;
        if (replicationServer.RangeStart != oldReplicationServer.RangeStart) return true;
        if (replicationServer.RangeEnd != oldReplicationServer.RangeEnd) return true;
        if (replicationServer.AtoZpath != oldReplicationServer.AtoZpath) return true;
        if (replicationServer.UpdateBlocked != oldReplicationServer.UpdateBlocked) return true;
        if (replicationServer.SlaveMonitor != oldReplicationServer.SlaveMonitor) return true;
        return false;
    }

    public static void Delete(long replicationServerNum)
    {
        var command = "DELETE FROM replicationserver "
                      + "WHERE ReplicationServerNum = " + SOut.Long(replicationServerNum);
        Db.NonQ(command);
    }

    public static void DeleteMany(List<long> listReplicationServerNums)
    {
        if (listReplicationServerNums == null || listReplicationServerNums.Count == 0) return;
        var command = "DELETE FROM replicationserver "
                      + "WHERE ReplicationServerNum IN(" + string.Join(",", listReplicationServerNums.Select(x => SOut.Long(x))) + ")";
        Db.NonQ(command);
    }
}