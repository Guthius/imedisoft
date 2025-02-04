using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Computers
{
    public static void Delete(Computer computer)
    {
        Db.NonQ($"DELETE FROM printer WHERE ComputerNum={computer.ComputerNum}");
        Db.NonQ($"DELETE FROM computer WHERE ComputerNum={computer.ComputerNum}");
    }

    public static Computer GetCur()
    {
        return GetFirstOrDefault(x => string.Equals(x.CompName, Environment.MachineName, StringComparison.CurrentCultureIgnoreCase));
    }

    public static void UpdateHeartBeat(string computerName, bool startup)
    {
        if (startup)
        {
            Db.NonQ("UPDATE computer SET LastHeartBeat = NOW() WHERE CompName = '" + SOut.String(computerName) + "'");
            return;
        }

        if (Cache.ListIsNull() || !Cache.GetExists(x => x.CompName == computerName))
        {
            RefreshCache();
        }
        
        var commandText = "SELECT LastHeartBeat<ADDDATE(NOW(), INTERVAL -3 MINUTE) FROM computer WHERE CompName='" + SOut.String(computerName) + "'";
        if (!SIn.Bool(DataCore.GetScalar(commandText)))
        {
            return;
        }
        
        Db.NonQ("UPDATE computer SET LastHeartBeat=NOW() WHERE CompName = '" + SOut.String(computerName) + "'");
    }

    public static void ClearHeartBeat(string computerName)
    {
        Db.NonQ("UPDATE computer SET LastHeartBeat=" + SOut.Date(new DateTime(0001, 1, 1)) + " WHERE CompName = '" + SOut.String(computerName) + "'");
    }

    public static void ClearAllHeartBeats(string machineNameException)
    {
        Db.NonQ("UPDATE computer SET LastHeartBeat=" + SOut.Date(new DateTime(0001, 1, 1)) + " WHERE CompName != '" + SOut.String(machineNameException) + "'");
    }

    public static List<string> GetServiceInfo()
    {
        var serviceInfo = new List<string>();
        
        var dataTable = DataCore.GetTable("SHOW VARIABLES WHERE Variable_name='socket'");
        
        serviceInfo.Add(dataTable.Rows.Count > 0 ? dataTable.Rows[0]["VALUE"].ToString() : "Not Found");
        
        dataTable = DataCore.GetTable("SHOW VARIABLES WHERE Variable_name='version_comment'");
        
        serviceInfo.Add(dataTable.Rows.Count > 0 ? dataTable.Rows[0]["VALUE"].ToString() : "Not Found");

        dataTable = null;
        try
        {
            dataTable = DataCore.GetTable("SELECT @@hostname");
        }
        catch
        {
            serviceInfo.Add("Not Found");
        }

        if (dataTable is not null)
        {
            serviceInfo.Add(dataTable.Rows.Count > 0 ? dataTable.Rows[0][0].ToString() : "Not Found");
        }

        serviceInfo.Add(MiscData.GetMySqlVersion());
        string databaseName;
        try
        {
            databaseName = MiscData.GetCurrentDatabase();
        }
        catch
        {
            serviceInfo.Add("Not Found.");
            
            return serviceInfo;
        }

        serviceInfo.Add(string.IsNullOrEmpty(databaseName) ? "Not Found" : databaseName);
        
        return serviceInfo;
    }
    
    private class ComputerCache : CacheListAbs<Computer>
    {
        protected override List<Computer> GetCacheFromDb()
        {
            return ComputerCrud.SelectMany("SELECT * FROM computer ORDER BY CompName");
        }

        protected override List<Computer> TableToList(DataTable dataTable)
        {
            return ComputerCrud.TableToList(dataTable);
        }

        protected override Computer Copy(Computer item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<Computer> items)
        {
            return ComputerCrud.ListToTable(items, "Computer");
        }

        protected override void FillCacheIfNeeded()
        {
            Computers.GetTableFromCache(false);
        }
    }
    
    private static readonly ComputerCache Cache = new();

    public static List<Computer> GetDeepCopy(bool shortList = false)
    {
        return Cache.GetDeepCopy(shortList);
    }

    public static Computer GetFirstOrDefault(Func<Computer, bool> predicate, bool shortList = false)
    {
        return Cache.GetFirstOrDefault(predicate, shortList);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
    }

    public static DataTable GetTableFromCache(bool refreshCache)
    {
        return Cache.GetTableFromCache(refreshCache);
    }

    public static void ClearCache()
    {
        Cache.ClearCache();
    }
}