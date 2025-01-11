using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDentBusiness;

public abstract class ListCache<TItem>
{
    private readonly object _lock = new();
    private readonly List<TItem> _items = [];
    private bool _filled;

    protected abstract List<TItem> GetCacheFromDb();

    protected virtual bool InShortList(TItem item)
    {
        return true;
    }

    private void FillCacheIfNeeded()
    {
        if (_filled)
        {
            return;
        }

        _items.AddRange(GetCacheFromDb());
        _filled = true;
    }

    private IEnumerable<TItem> AsEnumerable(bool shortList = false)
    {
        return shortList ? _items.Where(InShortList) : _items;
    }

    public void ClearCache()
    {
        lock (_lock)
        {
            _items.Clear();
            _filled = false;
        }
    }

    public void Refresh()
    {
        ClearCache();
        FillCacheIfNeeded();
    }
    
    public List<TItem> GetDeepCopy(bool shortList = false)
    {
        lock (_lock)
        {
            FillCacheIfNeeded();

            return AsEnumerable(shortList).ToList();
        }
    }

    public int GetCount(bool shortList = false)
    {
        lock (_lock)
        {
            FillCacheIfNeeded();

            return AsEnumerable(shortList).Count();
        }
    }

    public TItem GetFirst(bool shortList = false)
    {
        lock (_lock)
        {
            FillCacheIfNeeded();

            return AsEnumerable(shortList).First();
        }
    }

    public TItem GetFirstOrDefault(Func<TItem, bool> predicate, bool shortList = false)
    {
        lock (_lock)
        {
            FillCacheIfNeeded();

            return AsEnumerable(shortList).FirstOrDefault(predicate);
        }
    }
    
    public List<TItem> GetWhere(Predicate<TItem> predicate, bool shortList = false)
    {
        lock (_lock)
        {
            FillCacheIfNeeded();

            return AsEnumerable(shortList).Where(item => predicate(item)).ToList();
        }
    }
}