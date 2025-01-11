using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CodeBase;

namespace OpenDentBusiness;

public abstract class CacheDictNonPkAbs<TItem, TKey, TValue> : CacheDictAbs<TItem, TKey, TValue> where TItem : TableBase
{
    protected abstract DataTable ToDataTable(List<TItem> items);

    private readonly ReaderWriterLockSlim _lock = new();
    private readonly List<TItem> _items = [];

    public List<TItem> GetDeepCopyList(bool shortList = false)
    {
        FillDictIfNull();
        
        _lock.EnterReadLock();
        
        try
        {
            return GetShallowListHelper(shortList).Select(Copy).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public override void ClearCache()
    {
        if (IsNotFilled())
        {
            return;
        }

        _lock.EnterWriteLock();
        
        try
        {
            _items.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        base.ClearCache();
    }

    private List<TItem> GetShallowListHelper(bool shortList = false)
    {
        if (!_lock.IsReadLockHeld && !_lock.IsWriteLockHeld)
        {
            throw new ODException("GetShallowListHelper() requires a lock to be present before invoking the method.");
        }
        
        return shortList ? _items.FindAll(IsInDictShort) : _items;
    }
    
    public List<TItem> GetWhereFromList(Predicate<TItem> predicate, bool shortList = false)
    {
        FillDictIfNull();
        
        _lock.EnterReadLock();
        
        try
        {
            return GetShallowListHelper(shortList).FindAll(predicate).Select(Copy).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    
    public TItem GetFirstOrDefaultFromList(Func<TItem, bool> predicate, bool shortList = false)
    {
        FillDictIfNull();
        
        _lock.EnterReadLock();
        
        try
        {
            var item = GetShallowListHelper(shortList).FirstOrDefault(predicate);
            
            return item == null ? null : Copy(item);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
    
    protected override Dictionary<TKey, TValue> ToDictionary(List<TItem> items)
    {
        _lock.EnterWriteLock();
        
        try
        {
            _items.Clear();
            _items.AddRange(items);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
        
        return items.GroupBy(GetDictKey).ToDictionary(x => x.Key, x => GetDictValue(x.First()));
    }

    protected override void GotNewCache(List<TItem> items)
    {
        _lock.EnterWriteLock();
        try
        {
            _items.Clear();
            _items.AddRange(items);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }


    protected sealed override DataTable ToDataTable()
    {
        return ToDataTable(GetDeepCopyList());
    }
}