using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CodeBase;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;

public class ActiveInstances
{
    private static readonly ReaderWriterLockSlim Lock = new();
    private static ActiveInstance _activeInstance;
    
    public static ActiveInstance GetActiveInstance()
    {
        Lock.EnterReadLock();
        try
        {
            return _activeInstance;
        }
        finally
        {
            Lock.ExitReadLock();
        }
    }
    
    private static void SetActiveInstance(ActiveInstance activeInstance)
    {
        Lock.EnterWriteLock();
        try
        {
            _activeInstance = activeInstance;
        }
        finally
        {
            Lock.ExitWriteLock();
        }
    }
    
    public static void CloseActiveInstances(List<ActiveInstance> listActiveInstances)
    {
        if (listActiveInstances.IsNullOrEmpty()) return;
        foreach (var t in listActiveInstances)
        {
            Signalods.SetInvalid(InvalidType.ActiveInstance, KeyType.Undefined, t.ActiveInstanceNum);
        }
    }

    public static List<ActiveInstance> GetAllOldInstances()
    {
        var dateTimeToCheck = DateTime.Now.AddMinutes(-4);
        var command = "SELECT * FROM activeinstance WHERE DateTRecorded < " + SOut.DateT(dateTimeToCheck);
        return ActiveInstanceCrud.SelectMany(command);
    }

    public static List<ActiveInstance> GetAllResponsiveActiveInstances()
    {
        var dateTimeToCheck = DateTime.Now.AddMinutes(-4);
        var command = "SELECT * FROM activeinstance WHERE DateTRecorded > " + SOut.DateT(dateTimeToCheck);
        return ActiveInstanceCrud.SelectMany(command);
    }
    
    public static int GetCountCloudActiveInstances(long excludeInstanceNum = 0)
    {
        var dateTimeToCheck = DateTime.Now.AddMinutes(-4);
        var command = "SELECT COUNT(*) FROM activeinstance WHERE DateTRecorded > " + SOut.DateT(dateTimeToCheck)
                                                                                   + " AND ConnectionType=" + SOut.Enum(ConnectionTypes.Thinfinity);
        if (excludeInstanceNum != 0) command += " AND ActiveInstanceNum!=" + SOut.Long(excludeInstanceNum);
        return SIn.Int(Db.GetCount(command));
    }

    public static ActiveInstance GetOne(long userNum, long computerNum, long processId)
    {
        var command = "SELECT * FROM activeinstance"
                      + " WHERE UserNum=" + SOut.Long(userNum)
                      + " AND ComputerNum=" + SOut.Long(computerNum)
                      + " AND ProcessId=" + SOut.Long(processId);
        return ActiveInstanceCrud.SelectOne(command);
    }
    
    public static void Insert(ActiveInstance activeInstance)
    {
        activeInstance.DateTRecorded = DateTime.Now;
        ActiveInstanceCrud.Insert(activeInstance);
    }
    
    public static void Update(ActiveInstance activeInstance)
    {
        activeInstance.DateTRecorded = DateTime.Now;
        ActiveInstanceCrud.Update(activeInstance);
    }
    
    public static void Upsert(long userNum, long computerNum, long processId)
    {
        var activeInstance = GetActiveInstance() ?? GetOne(userNum, computerNum, processId);
        if (activeInstance == null)
        {
            activeInstance = new ActiveInstance
            {
                ConnectionType = ConnectionTypes.Direct,
                ComputerNum = computerNum,
                ProcessId = processId,
                UserNum = userNum,
                DateTimeLastActive = DateTime.Now
            };
            Insert(activeInstance);
            SetActiveInstance(activeInstance);
        }
        else
        {
            activeInstance.DateTimeLastActive = Security.DateTimeLastActivity;
            if (ODEnvironment.IsCloudServer && (activeInstance.UserNum != userNum || activeInstance.ComputerNum != computerNum || activeInstance.ProcessId != processId))
            {
                activeInstance.UserNum = userNum;
                activeInstance.ComputerNum = computerNum;
                activeInstance.ProcessId = processId;
                SetActiveInstance(activeInstance);
            }

            Update(activeInstance);
        }
    }
    
    public static void DeleteMany(List<ActiveInstance> listActiveInstances)
    {
        listActiveInstances?.RemoveAll(x => x == null);
        if (listActiveInstances.IsNullOrEmpty()) return;
        var command = "DELETE FROM activeinstance WHERE ActiveInstanceNum IN(" + string.Join(",", listActiveInstances.Select(x => x.ActiveInstanceNum).ToList()) + ")";
        Db.NonQ(command);
    }
}