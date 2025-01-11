using System;
using System.Collections.Generic;
using System.Data;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class Computers
{
    public static void EnsureComputerInDB(string clientComputerName, string hostComputerName)
    {
        var command = "SELECT COUNT(*) FROM computer WHERE CompName ='" + SOut.String(clientComputerName) + "'";
        var count = Db.GetLong(command);
        if (count != 0) return;
        var computer = new Computer();
        computer.CompName = clientComputerName;
        var computerNum = Insert(computer);
        //Never copy the printer rows for Thinfinity or AppStream
        if (ODEnvironment.IsCloudServer) return;
        if (clientComputerName.ToLower() != hostComputerName.ToLower())
            CopyPrinterRowsForComputer(computerNum, hostComputerName); //This computer is an RDP remote client. Copy the host computer's printer settings for the new computer.
        else if (PrefC.GetBool(PrefName.EasyHidePrinters)) Printers.PutForSit(PrintSituation.Default, clientComputerName, "", true);
    }

    /// <summary>
    ///     Called when a new computer is added and OD is running on a remote application server.
    ///     This copies any printer settings associated with the application server's computer and applies them to the new
    ///     computer.
    ///     21.3 introduces per-client-computer printer settings for RDP app servers and this prevents printer settings from
    ///     being reset for client computers when updating to 21.3.
    /// </summary>
    public static void CopyPrinterRowsForComputer(long computerNum, string hostComputerName)
    {
        //computerName is the client computer of a remote connection.
        var command = $"SELECT ComputerNum FROM computer WHERE CompName='{SOut.String(hostComputerName)}'";
        var computerNumHost = Db.GetLong(command);
        if (computerNumHost == 0) return; //Could not find the host computer in the database, no printer settings to copy.
        //Copy the host computer's printer settings for the client computer.
        command = "INSERT INTO printer (ComputerNum,PrintSit,PrinterName,DisplayPrompt) " +
                  $"SELECT {SOut.Long(computerNum)},PrintSit,PrinterName,DisplayPrompt FROM printer WHERE ComputerNum={SOut.Long(computerNumHost)}";
        Db.NonQ(command);
    }

    ///<summary>ONLY use this if compname is not already present</summary>
    public static long Insert(Computer computer)
    {
        return ComputerCrud.Insert(computer);
    }

    /*
    
    public static void Update(){
        string command= "UPDATE computer SET "
            +"compname = '"    +POut.PString(CompName)+"' "
            //+"printername = '" +POut.PString(PrinterName)+"' "
            +"WHERE ComputerNum = '"+POut.PInt(ComputerNum)+"'";
        //MessageBox.Show(string command);
        DataConnection dcon=new DataConnection();
        Db.NonQ(command);
    }*/

    
    public static void Delete(Computer computer)
    {
        //Delete any accociated printer settings from the printer table
        var command = $"DELETE FROM printer WHERE ComputerNum={SOut.Long(computer.ComputerNum)}";
        Db.NonQ(command);
        command = $"DELETE FROM computer WHERE ComputerNum={SOut.Long(computer.ComputerNum)}";
        Db.NonQ(command);
    }

    ///<summary>Only called from Printers.GetForSit</summary>
    public static Computer GetCur()
    {
        return GetFirstOrDefault(x => x.CompName.ToUpper() == ODEnvironment.MachineName.ToUpper());
    }

    ///<summary>Returns all computers with an active heart beat.  A heart beat less than 4 minutes old is considered active.</summary>
    public static List<Computer> GetRunningComputers()
    {
        //heartbeat is every three minutes.  We'll allow four to be generous.
        var command = "SELECT * FROM computer WHERE LastHeartBeat > SUBTIME(NOW(),'00:04:00')";
        return ComputerCrud.SelectMany(command);
    }

    /// <summary>When starting up, in an attempt to be fast, it will not add a new computer to the list.</summary>
    public static void UpdateHeartBeat(string computerName, bool isStartup)
    {
        string command;
        if (isStartup)
        {
            command = "UPDATE computer SET LastHeartBeat=" + DbHelper.Now() + " WHERE CompName = '" + SOut.String(computerName) + "'";
            Db.NonQ(command);
            return;
        }

        if (_computerCache.ListIsNull() || !_computerCache.GetExists(x => x.CompName == computerName))
            //RefreshCache if computer name doesn't exist in cache. Happens in cloud when a new computer connects to the db and is assigned the "UNKNOWN" name that is later updated
            //when the ODCloudClient sets the ODEnvironment.MachineName property.   RefreshCache will insert the new computer row with CompName=ODEnvironment.MachineName.
            RefreshCache(); //adds new computer to list
        command = "SELECT LastHeartBeat<" + DbHelper.DateAddMinute(DbHelper.Now(), "-3") + " FROM computer WHERE CompName='" + SOut.String(computerName) + "'";
        if (!SIn.Bool(DataCore.GetScalar(command))) //no need to update if LastHeartBeat is already within the last 3 mins
            return; //remote app servers with multiple connections would fight over the lock on a single row to update the heartbeat unnecessarily
        command = "UPDATE computer SET LastHeartBeat=" + DbHelper.Now() + " WHERE CompName = '" + SOut.String(computerName) + "'";
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

    /// <summary>
    ///     Returns a list of strings in a specific order.
    ///     The strings are as follows; socket (service name), version_comment (service comment), hostname (server name), MySQL
    ///     version,
    ///     and database name. Oracle is not supported and will throw an exception to have the customer call us to add support.
    /// </summary>
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

    #region CachePattern

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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ComputerCache _computerCache = new();

    public static List<Computer> GetDeepCopy(bool isShort = false)
    {
        return _computerCache.GetDeepCopy(isShort);
    }

    public static Computer GetFirstOrDefault(Func<Computer, bool> match, bool isShort = false)
    {
        return _computerCache.GetFirstOrDefault(match, isShort);
    }

    /// <summary>
    ///     Refreshes the cache and returns it as a DataTable. This will refresh the ClientWeb's cache and the ServerWeb's
    ///     cache.
    /// </summary>
    public static DataTable RefreshCache()
    {
        return GetTableFromCache(true);
    }

    ///<summary>Fills the local cache with the passed in DataTable.</summary>
    public static void FillCacheFromTable(DataTable table)
    {
        _computerCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _computerCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _computerCache.ClearCache();
    }

    #endregion
}