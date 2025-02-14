using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace Imedisoft.Core.Data;

public static class ReplicationServers
{
    private const long ServerId = -1;

    public static void Update(ReplicationServer replicationServer)
    {
        ReplicationServerCrud.Update(replicationServer);
    }

    public static long GetServerId()
    {
        return ServerId;
    }

    public static string GetAtoZpath()
    {
        var replicationServer = GetFirstOrDefault(x => x.ServerId == GetServerId());
        return replicationServer == null ? "" : replicationServer.AtoZpath;
    }

    public static ReplicationServer GetForLocalComputer()
    {
        return GetFirstOrDefault(x => x.ServerId == GetServerId());
    }

    private class ReplicationServerCache : CacheListAbs<ReplicationServer>
    {
        protected override List<ReplicationServer> GetCacheFromDb()
        {
            return ReplicationServerCrud.SelectMany("SELECT * FROM replicationserver ORDER BY ServerId");
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
            GetTableFromCache(false);
        }
    }

    private static readonly ReplicationServerCache Cache = new();

    public static ReplicationServer GetFirstOrDefault(Func<ReplicationServer, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }
}