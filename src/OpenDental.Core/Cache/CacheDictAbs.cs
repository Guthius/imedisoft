using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CodeBase;

namespace OpenDentBusiness;

public abstract class CacheDictAbs<TItem, TKey, TValue> : CacheAbs<TItem> where TItem : TableBase
{
    private readonly ReaderWriterLockSlim _lock = new();
    private Dictionary<TKey, TValue> _items;
    private List<TKey> _shortKeys;

    protected abstract DataTable ToDataTable(Dictionary<TKey, TValue> dict);
    protected abstract TKey GetDictKey(TItem item);
    protected abstract TValue GetDictValue(TItem item);
    protected abstract TValue CopyValue(TValue value);

    protected virtual bool IsInDictShort(TItem item)
    {
        return true;
    }

    protected virtual TValue GetShortValue(TValue value)
    {
        return value;
    }

    protected virtual void GotNewCache(List<TItem> items)
    {
    }

    public bool DictIsNull()
    {
        return IsNotFilled();
    }

    protected void FillDictIfNull()
    {
        if (IsNotFilled())
        {
            FillCacheIfNeeded();
        }
    }

    protected virtual Dictionary<TKey, TValue> ToDictionary(List<TItem> items)
    {
        return items.ToDictionary(GetDictKey, GetDictValue);
    }

    protected virtual List<TKey> GetShortKeys(List<TItem> items)
    {
        return items.Where(IsInDictShort).Select(GetDictKey).ToList();
    }

    private Dictionary<TKey, TValue> GetShallowHelper(bool shortList = false)
    {
        if (!_lock.IsReadLockHeld && !_lock.IsWriteLockHeld)
        {
            throw new ODException("GetShallowHelper() requires a lock to be present before invoking the method.");
        }

        var dict = new Dictionary<TKey, TValue>();

        if (shortList)
        {
            foreach (var key in _shortKeys)
            {
                dict[key] = GetShortValue(_items[key]);
            }
        }
        else
        {
            dict = _items;
        }

        return dict;
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
            _shortKeys = null;
            _items = null;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool GetContainsKey(TKey key, bool shortList = false)
    {
        FillDictIfNull();

        _lock.EnterReadLock();
        try
        {
            return shortList ? _shortKeys.Contains(key) : _items.ContainsKey(key);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public int GetCount(bool shortList = false)
    {
        FillDictIfNull();

        _lock.EnterReadLock();
        try
        {
            return shortList ? _shortKeys.Count : _items.Keys.Count;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public Dictionary<TKey, TValue> GetDeepCopy(bool shortList = false)
    {
        FillDictIfNull();

        _lock.EnterReadLock();
        
        try
        {
            var dict = GetShallowHelper(shortList);
            var dictCopy = new Dictionary<TKey, TValue>();

            foreach (var kvp in dict)
            {
                dictCopy[kvp.Key] = CopyValue(kvp.Value);
            }

            return dictCopy;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public TValue GetOne(TKey key, bool shortList = false)
    {
        FillDictIfNull();

        _lock.EnterReadLock();

        try
        {
            if (!shortList)
            {
                return CopyValue(_items[key]);
            }

            if (!_shortKeys.Contains(key))
            {
                throw new KeyNotFoundException();
            }

            return CopyValue(GetShortValue(_items[key]));
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public TValue GetFirstOrDefault(Func<TValue, bool> predicate, bool shortList = false)
    {
        FillDictIfNull();

        _lock.EnterReadLock();

        try
        {
            var value = GetShallowHelper(shortList).Values.FirstOrDefault(predicate);

            return value == null ? default : CopyValue(value);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public List<TValue> GetWhere(Func<TValue, bool> predicate, bool shortList = false)
    {
        FillDictIfNull();

        _lock.EnterReadLock();

        try
        {
            return GetShallowHelper(shortList).Values.Where(predicate).Select(CopyValue).ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public List<TValue> GetWhereForKey(Func<TKey, bool> predicate, bool shortList = false)
    {
        FillDictIfNull();

        _lock.EnterReadLock();

        try
        {
            var dict = GetShallowHelper(shortList);
            var keys = dict.Keys.Where(predicate).ToList();

            var values = new List<TValue>();
            foreach (var key in keys)
            {
                values.Add(CopyValue(dict[key]));
            }

            return values;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool RemoveKey(TKey key)
    {
        FillDictIfNull();

        _lock.EnterWriteLock();
        try
        {
            _shortKeys.Remove(key);

            return _items.Remove(key);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void SetValueForKey(TKey key, TValue value, bool isShort = false)
    {
        FillDictIfNull();

        _lock.EnterWriteLock();

        try
        {
            if (isShort && !_shortKeys.Contains(key))
            {
                _shortKeys.Add(key);
            }

            _items[key] = CopyValue(value);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    protected sealed override void OnNewCacheReceived(List<TItem> items)
    {
        if (typeof(TKey) != typeof(string) && typeof(TKey) != typeof(long) && !typeof(TKey).IsEnum)
        {
            throw new ODException("CacheDictAbs requires KEY_TYPE to be of type string or long.");
        }

        var dict = ToDictionary(items);
        var shortKeys = GetShortKeys(items);

        _lock.EnterWriteLock();

        try
        {
            _shortKeys = shortKeys;
            _items = dict;
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        GotNewCache(items);
    }

    protected sealed override bool IsNotFilled()
    {
        _lock.EnterReadLock();

        try
        {
            return _items == null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    protected override DataTable ToDataTable()
    {
        FillDictIfNull();

        var result = new Dictionary<TKey, TValue>();

        _lock.EnterReadLock();

        try
        {
            foreach (var key in _items.Keys)
            {
                result.Add(key, CopyValue(_items[key]));
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }

        return ToDataTable(result);
    }
}