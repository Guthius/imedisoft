using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataConnectionBase;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;
using OpenDentBusiness;

namespace Imedisoft.Core.Data;

public static class ActiveInstances
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

    public static void CloseActiveInstances(List<ActiveInstance> activeInstances)
    {
        if (activeInstances is not {Count: > 0})
        {
            return;
        }

        foreach (var activeInstance in activeInstances)
        {
            Signalods.SetInvalid(InvalidType.ActiveInstance, KeyType.Undefined, activeInstance.ActiveInstanceNum);
        }
    }

    public static List<ActiveInstance> GetAllOldInstances()
    {
        var dateTimeToCheck = DateTime.Now.AddMinutes(-4);

        return ActiveInstanceCrud.SelectMany("SELECT * FROM activeinstance WHERE DateTRecorded < " + SOut.DateTime(dateTimeToCheck));
    }

    public static List<ActiveInstance> GetAllResponsiveActiveInstances()
    {
        var dateTimeToCheck = DateTime.Now.AddMinutes(-4);

        return ActiveInstanceCrud.SelectMany("SELECT * FROM activeinstance WHERE DateTRecorded > " + SOut.DateTime(dateTimeToCheck));
    }

    public static ActiveInstance GetOne(long userNum, long computerNum, long processId)
    {
        return ActiveInstanceCrud.SelectOne("SELECT * FROM activeinstance WHERE UserNum=" + userNum + " AND ComputerNum=" + computerNum + " AND ProcessId=" + processId);
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

            Update(activeInstance);
        }
    }

    public static void DeleteMany(List<ActiveInstance> activeInstances)
    {
        activeInstances?.RemoveAll(x => x == null);

        if (activeInstances is not {Count: > 0})
        {
            return;
        }

        Db.NonQ("DELETE FROM activeinstance WHERE ActiveInstanceNum IN (" + string.Join(",", activeInstances.Select(x => x.ActiveInstanceNum)) + ")");
    }
}