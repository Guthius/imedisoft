using System;
using System.Collections.Generic;
using System.Data;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class ReplicationServers
{
	private static long _serverId = -1;

    public static void Update(ReplicationServer replicationServer)
    {
        ReplicationServerCrud.Update(replicationServer);
    }

    public static long GetServerId()
    {
        return _serverId;
    }

    public static string GetAtoZpath()
    {
        var replicationServer = GetFirstOrDefault(x => x.ServerId == GetServerId());
        if (replicationServer == null) return "";
        return replicationServer.AtoZpath;
    }

    public static ReplicationServer GetForLocalComputer()
    {
        return GetFirstOrDefault(x => x.ServerId == GetServerId());
    }
    
    private class ReplicationServerCache : CacheListAbs<ReplicationServer>
    {
        protected override List<ReplicationServer> GetCacheFromDb()
        {
            var command = "SELECT * FROM replicationserver ORDER BY ServerId";
            return ReplicationServerCrud.SelectMany(command);
        }

        protected override List<ReplicationServer> TableToList(DataTable dataTable)
        {
            return ReplicationServerCrud.TableToList(dataTable);
        }

        protected override ReplicationServer Copy(ReplicationServer item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ReplicationServer> items)
        {
            return ReplicationServerCrud.ListToTable(items, "ReplicationServer");
        }

        protected override void FillCacheIfNeeded()
        {
            ReplicationServers.GetTableFromCache(false);
        }
    }
    
    private static readonly ReplicationServerCache Cache = new();

    public static ReplicationServer GetFirstOrDefault(Func<ReplicationServer, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static void FillCacheFromTable(DataTable table)
    {
        Cache.FillCacheFromTable(table);
    }

    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return Cache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}