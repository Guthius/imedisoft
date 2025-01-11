using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class ProcApptColors
{
    
    public static long Insert(ProcApptColor procApptColor)
    {
        return ProcApptColorCrud.Insert(procApptColor);
    }

    
    public static void Update(ProcApptColor procApptColor)
    {
        ProcApptColorCrud.Update(procApptColor);
    }

    
    public static void Delete(long procApptColorNum)
    {
        var command = "DELETE FROM procapptcolor WHERE ProcApptColorNum = " + SOut.Long(procApptColorNum);
        Db.NonQ(command);
    }

    /*
    ///<summary>Gets one ProcApptColor from the db.</summary>
    public static ProcApptColor GetOne(long procApptColorNum){

        return Crud.ProcApptColorCrud.SelectOne(procApptColorNum);
    }*/

    ///<summary>Supply code such as D####.  Returns null if no match</summary>
    public static ProcApptColor GetMatch(string procCode)
    {
        var code1 = "";
        var code2 = "";
        var listProcApptColors = GetDeepCopy();
        for (var i = 0; i < listProcApptColors.Count; i++)
        {
            //using public property to trigger refresh if needed.
            if (listProcApptColors[i].CodeRange.Contains("-"))
            {
                var codeSplit = listProcApptColors[i].CodeRange.Split('-');
                code1 = codeSplit[0].Trim();
                code2 = codeSplit[1].Trim();
            }
            else
            {
                code1 = listProcApptColors[i].CodeRange.Trim();
                code2 = listProcApptColors[i].CodeRange.Trim();
            }

            if (procCode.CompareTo(code1) < 0 || procCode.CompareTo(code2) > 0) continue;
            return listProcApptColors[i];
        }

        return null;
    }

    #region CachePattern

    private class ProcApptColorCache : CacheListAbs<ProcApptColor>
    {
        protected override List<ProcApptColor> GetCacheFromDb()
        {
            var command = "SELECT * FROM procapptcolor ORDER BY CodeRange";
            return ProcApptColorCrud.SelectMany(command);
        }

        protected override List<ProcApptColor> TableToList(DataTable dataTable)
        {
            return ProcApptColorCrud.TableToList(dataTable);
        }

        protected override ProcApptColor Copy(ProcApptColor item)
        {
            return item.Copy();
        }

        protected override DataTable ToDataTable(List<ProcApptColor> items)
        {
            return ProcApptColorCrud.ListToTable(items, "ProcApptColor");
        }

        protected override void FillCacheIfNeeded()
        {
            ProcApptColors.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly ProcApptColorCache _procApptColorCache = new();

    public static List<ProcApptColor> GetDeepCopy(bool isShort = false)
    {
        return _procApptColorCache.GetDeepCopy(isShort);
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
        _procApptColorCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _procApptColorCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _procApptColorCache.ClearCache();
    }

    #endregion
}