using System;
using System.Collections.Generic;
using System.Data;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Crud;
using Imedisoft.Core.Entities;

namespace OpenDentBusiness;

public class SigElementDefs
{
    public static void Update(SigElementDef sigElementDef)
    {
        SigElementDefCrud.Update(sigElementDef);
    }

    public static void Insert(SigElementDef sigElementDef)
    {
        SigElementDefCrud.Insert(sigElementDef);
    }

    public static void Delete(SigElementDef sigElementDef)
    {
        Db.NonQ("DELETE FROM sigelementdef WHERE SigElementDefNum =" + (sigElementDef.SigElementDefNum));
    }

    public static SigElementDef[] GetSubList(SignalElementType signalElementType)
    {
        return GetWhere(x => x.SigElementType == signalElementType).ToArray();
    }

    public static void MoveUp(int selected, List<SigElementDef> sigElementDefsSub)
    {
        switch (selected)
        {
            case < 0:
                throw new ApplicationException("Please select an item first.");

            case 0:
                return;
        }

        if (selected > sigElementDefsSub.Count - 1)
        {
            throw new ApplicationException("Invalid selection.");
        }

        SetOrder(selected - 1, sigElementDefsSub[selected].ItemOrder, sigElementDefsSub);
        SetOrder(selected, sigElementDefsSub[selected].ItemOrder - 1, sigElementDefsSub);
    }

    public static void MoveDown(int selected, List<SigElementDef> listSigElementDefsSub)
    {
        if (selected < 0) throw new ApplicationException(Lans.g("SigElementDefs", "Please select an item first."));
        if (selected == listSigElementDefsSub.Count - 1) //already at bottom
            return;
        if (selected > listSigElementDefsSub.Count - 1) throw new ApplicationException(Lans.g("SigElementDefs", "Invalid selection."));
        SetOrder(selected + 1, listSigElementDefsSub[selected].ItemOrder, listSigElementDefsSub);
        SetOrder(selected, listSigElementDefsSub[selected].ItemOrder + 1, listSigElementDefsSub);
    }

    private static void SetOrder(int mySelNum, int myItemOrder, List<SigElementDef> listSigElementDefsSub)
    {
        var sigElementDef = listSigElementDefsSub[mySelNum];
        sigElementDef.ItemOrder = myItemOrder;
        Update(sigElementDef);
    }

    public static SigElementDef GetElementDef(long sigElementDefNum)
    {
        return GetFirstOrDefault(x => x.SigElementDefNum == sigElementDefNum);
    }

    public static List<SigElementDef> GetElementsForButDef(SigButDef sigButDef)
    {
        var listSigElementDefs = new List<SigElementDef>();
        listSigElementDefs.AddRange(GetWhere(x => x.SigElementDefNum == sigButDef.SigElementDefNumUser));
        listSigElementDefs.AddRange(GetWhere(x => x.SigElementDefNum == sigButDef.SigElementDefNumExtra));
        listSigElementDefs.AddRange(GetWhere(x => x.SigElementDefNum == sigButDef.SigElementDefNumMsg));
        return listSigElementDefs;
    }

    public static List<SigElementDef> GetDefsForSigMessage(SigMessage sigMessage)
    {
        var listSigElementDefs = new List<SigElementDef>();
        listSigElementDefs.AddRange(GetWhere(x => x.SigElementDefNum == sigMessage.SigElementDefNumUser));
        listSigElementDefs.AddRange(GetWhere(x => x.SigElementDefNum == sigMessage.SigElementDefNumExtra));
        listSigElementDefs.AddRange(GetWhere(x => x.SigElementDefNum == sigMessage.SigElementDefNumMsg));
        return listSigElementDefs;
    }

    private class SigElementDefCache : CacheListAbs<SigElementDef>
    {
        protected override List<SigElementDef> GetCacheFromDb()
        {
            var command = "SELECT * FROM sigelementdef ORDER BY ItemOrder";
            return SigElementDefCrud.SelectMany(command);
        }

        protected override List<SigElementDef> TableToList(DataTable dataTable)
        {
            return SigElementDefCrud.TableToList(dataTable);
        }

        protected override SigElementDef Copy(SigElementDef item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<SigElementDef> items)
        {
            return SigElementDefCrud.ListToTable(items, "SigElementDef");
        }

        protected override void FillCacheIfNeeded()
        {
            SigElementDefs.GetTableFromCache(false);
        }
    }

    private static readonly SigElementDefCache Cache = new();

    public static List<SigElementDef> GetWhere(Predicate<SigElementDef> match, bool isShort = false)
    {
        return Cache.GetWhere(match, isShort);
    }

    public static SigElementDef GetFirstOrDefault(Func<SigElementDef, bool> match, bool isShort = false)
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