using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CodeBase;

namespace OpenDentBusiness;

public abstract class CacheListAbs<TItem> : CacheAbs<TItem> where TItem : TableBase
{
    protected abstract DataTable ToDataTable(List<TItem> items);

    private readonly ReaderWriterLockSlim _lock = new();
    private List<TItem> _items;
    private bool _isCacheAllowed = true;

    public bool IsCacheAllowed
    {
        get => _isCacheAllowed;
        set
        {
            _isCacheAllowed = value;

            if (_isCacheAllowed)
            {
                return;
            }

            _lock.EnterWriteLock();
            try
            {
                _items = null;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
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
            _items = null;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    protected virtual bool IsInListShort(TItem item)
    {
        return true;
    }

    public bool ListIsNull()
    {
        return IsNotFilled();
    }

    private void FillListIfNull()
    {
        if (ListIsNull())
        {
            FillCacheIfNeeded();
        }
    }
    
    private List<TItem> GetShallowHelper(bool shortList = false)
    {
        if (!_lock.IsReadLockHeld && !_lock.IsWriteLockHeld)
        {
            throw new ODException("GetShallowHelper() requires a lock to be present before invoking the method.");
        }

        if (_items is null)
        {
            return null;
        }

        return shortList ? _items.FindAll(IsInListShort) : _items;
    }

    public List<TItem> GetDeepCopy(bool shortList = false)
    {
        FillListIfNull();

        _lock.EnterReadLock();

        try
        {
            return GetShallowHelper(shortList).Select(Copy).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public int GetCount(bool shortList = false)
    {
        FillListIfNull();

        _lock.EnterReadLock();

        try
        {
            return GetShallowHelper(shortList).Count;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool GetExists(Predicate<TItem> predicate, bool shortList = false)
    {
        FillListIfNull();

        _lock.EnterReadLock();

        try
        {
            return GetShallowHelper(shortList).Exists(predicate);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public int GetFindIndex(Predicate<TItem> predicate, bool shortList = false)
    {
        FillListIfNull();

        _lock.EnterReadLock();

        try
        {
            return GetShallowHelper(shortList).FindIndex(predicate);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public TItem GetFirst(bool shortList = false)
    {
        FillListIfNull();

        _lock.EnterReadLock();

        try
        {
            return Copy(GetShallowHelper(shortList).First());
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public TItem GetFirst(Func<TItem, bool> predicate, bool shortList = false)
    {
        FillListIfNull();

        _lock.EnterReadLock();

        try
        {
            return Copy(GetShallowHelper(shortList).First(predicate));
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public TItem GetFirstOrDefault(Func<TItem, bool> predicate, bool shortList = false)
    {
        FillListIfNull();

        _lock.EnterReadLock();

        try
        {
            var item = GetShallowHelper(shortList).FirstOrDefault(predicate);

            return item is null ? null : Copy(item);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public TItem GetLast(bool shortList = false)
    {
        FillListIfNull();

        _lock.EnterReadLock();

        try
        {
            return Copy(GetShallowHelper(shortList).Last());
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public TItem GetLastOrDefault(Func<TItem, bool> predicate, bool shortList = false)
    {
        FillListIfNull();

        _lock.EnterReadLock();

        try
        {
            var item = GetShallowHelper(shortList).LastOrDefault(predicate);

            return item is null ? null : Copy(item);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public List<TItem> GetWhere(Predicate<TItem> predicate, bool shortList = false)
    {
        FillListIfNull();

        _lock.EnterReadLock();

        try
        {
            return GetShallowHelper(shortList).FindAll(predicate).Select(Copy).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    protected sealed override void OnNewCacheReceived(List<TItem> items)
    {
        if (!IsCacheAllowed)
        {
            throw new ApplicationException("Caching has been temporarily turned off.  Please call support.");
        }

        _lock.EnterWriteLock();

        try
        {
            _items = items;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    protected sealed override bool IsNotFilled()
    {
        _lock.EnterReadLock();

        try
        {
            return GetShallowHelper() == null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    protected sealed override DataTable ToDataTable()
    {
        return ToDataTable(GetDeepCopy());
    }
}