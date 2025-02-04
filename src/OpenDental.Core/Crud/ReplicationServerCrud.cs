using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Crud;

public class ReplicationServerCrud
{
    public static List<ReplicationServer> SelectMany(string command)
    {
        var list = TableToList(DataCore.GetTable(command));
        return list;
    }

    public static List<ReplicationServer> TableToList(DataTable table)
    {
        var retVal = new List<ReplicationServer>();
        foreach (DataRow row in table.Rows)
        {
            var replicationServer = new ReplicationServer
            {
                ReplicationServerNum = SIn.Long(row["ReplicationServerNum"].ToString()),
                Descript = SIn.String(row["Descript"].ToString()),
                ServerId = SIn.Int(row["ServerId"].ToString()),
                RangeStart = SIn.Long(row["RangeStart"].ToString()),
                RangeEnd = SIn.Long(row["RangeEnd"].ToString()),
                AtoZpath = SIn.String(row["AtoZpath"].ToString()),
                UpdateBlocked = SIn.Bool(row["UpdateBlocked"].ToString()),
                SlaveMonitor = SIn.String(row["SlaveMonitor"].ToString())
            };
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
        var paramDescript = new OdSqlParameter("paramDescript", SOut.StringParam(replicationServer.Descript));
        Db.NonQ(command, paramDescript);
    }
}