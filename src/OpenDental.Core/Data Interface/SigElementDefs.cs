using System;
using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class SigElementDefs
{
    
    public static void Update(SigElementDef sigElementDef)
    {
        SigElementDefCrud.Update(sigElementDef);
    }

    
    public static long Insert(SigElementDef sigElementDef)
    {
        return SigElementDefCrud.Insert(sigElementDef);
    }

    
    public static void Delete(SigElementDef sigElementDef)
    {
        var command = "DELETE FROM sigelementdef WHERE SigElementDefNum =" + SOut.Long(sigElementDef.SigElementDefNum);
        Db.NonQ(command);
    }

    
    public static SigElementDef[] GetSubList(SignalElementType signalElementType)
    {
        return GetWhere(x => x.SigElementType == signalElementType).ToArray();
    }

    ///<summary>Moves the selected item up in the supplied sub list.</summary>
    public static void MoveUp(int selected, List<SigElementDef> listSigElementDefsSub)
    {
        if (selected < 0) throw new ApplicationException(Lans.g("SigElementDefs", "Please select an item first."));
        if (selected == 0) //already at top
            return;
        if (selected > listSigElementDefsSub.Count - 1) throw new ApplicationException(Lans.g("SigElementDefs", "Invalid selection."));
        SetOrder(selected - 1, listSigElementDefsSub[selected].ItemOrder, listSigElementDefsSub);
        SetOrder(selected, listSigElementDefsSub[selected].ItemOrder - 1, listSigElementDefsSub);
        //Selected-=1;
    }

    
    public static void MoveDown(int selected, List<SigElementDef> listSigElementDefsSub)
    {
        if (selected < 0) throw new ApplicationException(Lans.g("SigElementDefs", "Please select an item first."));
        if (selected == listSigElementDefsSub.Count - 1) //already at bottom
            return;
        if (selected > listSigElementDefsSub.Count - 1) throw new ApplicationException(Lans.g("SigElementDefs", "Invalid selection."));
        SetOrder(selected + 1, listSigElementDefsSub[selected].ItemOrder, listSigElementDefsSub);
        SetOrder(selected, listSigElementDefsSub[selected].ItemOrder + 1, listSigElementDefsSub);
        //selected+=1;
    }

    ///<summary>Used by MoveUp and MoveDown.</summary>
    private static void SetOrder(int mySelNum, int myItemOrder, List<SigElementDef> listSigElementDefsSub)
    {
        var sigElementDef = listSigElementDefsSub[mySelNum];
        sigElementDef.ItemOrder = myItemOrder;
        Update(sigElementDef);
    }

    ///<summary>Returns the SigElementDef with the specified num from the cache.</summary>
    public static SigElementDef GetElementDef(long SigElementDefNum)
    {
        return GetFirstOrDefault(x => x.SigElementDefNum == SigElementDefNum);
    }

    ///<summary>Gets all sigelementdefs for the sigbutdef passed in.  Includes user, extra, and message element defs.</summary>
    public static List<SigElementDef> GetElementsForButDef(SigButDef sigButDef)
    {
        var listSigElementDefs = new List<SigElementDef>();
        listSigElementDefs.AddRange(GetWhere(x => x.SigElementDefNum == sigButDef.SigElementDefNumUser));
        listSigElementDefs.AddRange(GetWhere(x => x.SigElementDefNum == sigButDef.SigElementDefNumExtra));
        listSigElementDefs.AddRange(GetWhere(x => x.SigElementDefNum == sigButDef.SigElementDefNumMsg));
        return listSigElementDefs;
    }

    ///<summary>Gets all sigelementdefs for the sigmessage passed in.  Includes user, extra, and message element defs.</summary>
    public static List<SigElementDef> GetDefsForSigMessage(SigMessage sigMessage)
    {
        var listSigElementDefs = new List<SigElementDef>();
        listSigElementDefs.AddRange(GetWhere(x => x.SigElementDefNum == sigMessage.SigElementDefNumUser));
        listSigElementDefs.AddRange(GetWhere(x => x.SigElementDefNum == sigMessage.SigElementDefNumExtra));
        listSigElementDefs.AddRange(GetWhere(x => x.SigElementDefNum == sigMessage.SigElementDefNumMsg));
        return listSigElementDefs;
    }

    #region CachePattern

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

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly SigElementDefCache _sigElementDefCache = new();

    public static List<SigElementDef> GetWhere(Predicate<SigElementDef> match, bool isShort = false)
    {
        return _sigElementDefCache.GetWhere(match, isShort);
    }

    public static SigElementDef GetFirstOrDefault(Func<SigElementDef, bool> match, bool isShort = false)
    {
        return _sigElementDefCache.GetFirstOrDefault(match, isShort);
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
        _sigElementDefCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _sigElementDefCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _sigElementDefCache.ClearCache();
    }

    #endregion
}