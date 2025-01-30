using System;
using System.Collections.Generic;
using System.Data;
using CodeBase;
using DataConnectionBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class Computers
{
    public static void Delete(Computer computer)
    {
        //Delete any accociated printer settings from the printer table
        var command = $"DELETE FROM printer WHERE ComputerNum={SOut.Long(computer.ComputerNum)}";
        Db.NonQ(command);
        command = $"DELETE FROM computer WHERE ComputerNum={SOut.Long(computer.ComputerNum)}";
        Db.NonQ(command);
    }

    public static Computer GetCur()
    {
        return GetFirstOrDefault(x => x.CompName.ToUpper() == ODEnvironment.MachineName.ToUpper());
    }

    public static void UpdateHeartBeat(string computerName, bool isStartup)
    {
        string command;
        if (isStartup)
        {
            command = "UPDATE computer SET LastHeartBeat=" + "NOW()" + " WHERE CompName = '" + SOut.String(computerName) + "'";
            Db.NonQ(command);
            return;
        }

        if (Cache.ListIsNull() || !Cache.GetExists(x => x.CompName == computerName))
            //RefreshCache if computer name doesn't exist in cache. Happens in cloud when a new computer connects to the db and is assigned the "UNKNOWN" name that is later updated
            //when the ODCloudClient sets the ODEnvironment.MachineName property.   RefreshCache will insert the new computer row with CompName=ODEnvironment.MachineName.
            RefreshCache(); //adds new computer to list
        command = "SELECT LastHeartBeat<ADDDATE(NOW(), INTERVAL -3 MINUTE) FROM computer WHERE CompName='" + SOut.String(computerName) + "'";
        if (!SIn.Bool(DataCore.GetScalar(command))) //no need to update if LastHeartBeat is already within the last 3 mins
            return; //remote app servers with multiple connections would fight over the lock on a single row to update the heartbeat unnecessarily
        command = "UPDATE computer SET LastHeartBeat=NOW() WHERE CompName = '" + SOut.String(computerName) + "'";
        Db.NonQ(command);
    }

    public static void ClearHeartBeat(string computerName)
    {
        var command = "UPDATE computer SET LastHeartBeat=" + SOut.Date(new DateTime(0001, 1, 1), true) + " WHERE CompName = '" + SOut.String(computerName) + "'";
        Db.NonQ(command);
    }

    public static void ClearAllHeartBeats(string machineNameException)
    {
        var command = "UPDATE computer SET LastHeartBeat=" + SOut.Date(new DateTime(0001, 1, 1), true) + " "
                      + "WHERE CompName != '" + SOut.String(machineNameException) + "'";
        Db.NonQ(command);
    }

    public static List<string> GetServiceInfo()
    {
        var listStringsServiceInfo = new List<string>();
        var table = DataCore.GetTable("SHOW VARIABLES WHERE Variable_name='socket'"); //service name
        if (table.Rows.Count > 0)
            listStringsServiceInfo.Add(table.Rows[0]["VALUE"].ToString());
        else
            listStringsServiceInfo.Add("Not Found");
        table = DataCore.GetTable("SHOW VARIABLES WHERE Variable_name='version_comment'"); //service comment
        if (table.Rows.Count > 0)
            listStringsServiceInfo.Add(table.Rows[0]["VALUE"].ToString());
        else
            listStringsServiceInfo.Add("Not Found");
        table = null;
        try
        {
            table = DataCore.GetTable("SELECT @@hostname"); //server name
        }
        catch
        {
            listStringsServiceInfo.Add("Not Found"); //hostname variable doesn't exist
        }

        if (table != null)
        {
            if (table.Rows.Count > 0)
                listStringsServiceInfo.Add(table.Rows[0][0].ToString());
            else
                listStringsServiceInfo.Add("Not Found");
        }

        listStringsServiceInfo.Add(MiscData.GetMySqlVersion());
        var dbName = "";
        try
        {
            dbName = MiscData.GetCurrentDatabase(); //database name
        }
        catch
        {
            listStringsServiceInfo.Add("Not Found."); //database variable doesn't exist
            return listStringsServiceInfo;
        }

        if (string.IsNullOrEmpty(dbName))
            listStringsServiceInfo.Add("Not Found");
        else
            listStringsServiceInfo.Add(dbName);
        return listStringsServiceInfo;
    }
    
    private class ComputerCache : CacheListAbs<Computer>
    {
        protected override List<Computer> GetCacheFromDb()
        {
            var command = "SELECT * FROM computer ORDER BY CompName";
            return ComputerCrud.SelectMany(command);
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

    public static List<Computer> GetDeepCopy(bool isShort = false)
    {
        return Cache.GetDeepCopy(isShort);
    }

    public static Computer GetFirstOrDefault(Func<Computer, bool> match, bool isShort = false)
    {
        return Cache.GetFirstOrDefault(match, isShort);
    }

    public static void RefreshCache()
    {
        GetTableFromCache(true);
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