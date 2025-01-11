using System.Collections.Generic;
using System.Data;
using DataConnectionBase;
using OpenDentBusiness.Crud;

namespace OpenDentBusiness;


public class HL7DefSegments
{
    /// <summary>Gets it straight from the database instead of from cache. No child objects included.</summary>
    public static List<HL7DefSegment> GetShallowFromDb(long hL7DefMessageNum)
    {
        var command = "SELECT * FROM hl7defsegment WHERE HL7DefMessageNum='" + SOut.Long(hL7DefMessageNum) + "' ORDER BY ItemOrder";
        return HL7DefSegmentCrud.SelectMany(command);
    }

    ///<summary>Gets deep list from cache.</summary>
    public static List<HL7DefSegment> GetDeepFromCache(long hL7DefMessageNum)
    {
        var listHL7DefSegmentsRet = new List<HL7DefSegment>();
        var listHL7DefSegments = GetDeepCopy();
        for (var i = 0; i < listHL7DefSegments.Count; i++)
            if (listHL7DefSegments[i].HL7DefMessageNum == hL7DefMessageNum)
            {
                listHL7DefSegmentsRet.Add(listHL7DefSegments[i]);
                listHL7DefSegmentsRet[listHL7DefSegmentsRet.Count - 1].hl7DefFields = HL7DefFields.GetFromCache(listHL7DefSegments[i].HL7DefSegmentNum);
            }

        return listHL7DefSegmentsRet;
    }

    ///<summary>Gets a full deep list of all Segments for this message from the database.</summary>
    public static List<HL7DefSegment> GetDeepFromDb(long hL7DefMessageNum)
    {
        var listHL7DefSegments = new List<HL7DefSegment>();
        listHL7DefSegments = GetShallowFromDb(hL7DefMessageNum);
        for (var i = 0; i < listHL7DefSegments.Count; i++) listHL7DefSegments[i].hl7DefFields = HL7DefFields.GetFromDb(listHL7DefSegments[i].HL7DefSegmentNum);
        return listHL7DefSegments;
    }

    
    public static long Insert(HL7DefSegment hL7DefSegment)
    {
        return HL7DefSegmentCrud.Insert(hL7DefSegment);
    }

    
    public static void Update(HL7DefSegment hL7DefSegment)
    {
        HL7DefSegmentCrud.Update(hL7DefSegment);
    }

    
    public static void Delete(long hL7DefSegmentNum)
    {
        var command = "DELETE FROM hl7defsegment WHERE HL7DefSegmentNum = " + SOut.Long(hL7DefSegmentNum);
        Db.NonQ(command);
    }

    #region CachePattern

    private class HL7DefSegmentCache : CacheListAbs<HL7DefSegment>
    {
        protected override List<HL7DefSegment> GetCacheFromDb()
        {
            var command = "SELECT * FROM hl7defsegment ORDER BY ItemOrder";
            return HL7DefSegmentCrud.SelectMany(command);
        }

        protected override List<HL7DefSegment> TableToList(DataTable dataTable)
        {
            return HL7DefSegmentCrud.TableToList(dataTable);
        }

        protected override HL7DefSegment Copy(HL7DefSegment item)
        {
            return item.Clone();
        }

        protected override DataTable ToDataTable(List<HL7DefSegment> items)
        {
            return HL7DefSegmentCrud.ListToTable(items, "HL7DefSegment");
        }

        protected override void FillCacheIfNeeded()
        {
            HL7DefSegments.GetTableFromCache(false);
        }
    }

    ///<summary>The object that accesses the cache in a thread-safe manner.</summary>
    private static readonly HL7DefSegmentCache _HL7DefSegmentCache = new();

    public static List<HL7DefSegment> GetDeepCopy(bool isShort = false)
    {
        return _HL7DefSegmentCache.GetDeepCopy(isShort);
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
        _HL7DefSegmentCache.FillCacheFromTable(table);
    }

    ///<summary>Always refreshes the ClientWeb's cache.</summary>
    public static DataTable GetTableFromCache(bool doRefreshCache)
    {
        return _HL7DefSegmentCache.GetTableFromCache(doRefreshCache);
    }

    public static void ClearCache()
    {
        _HL7DefSegmentCache.ClearCache();
    }

    #endregion

    /*
    Only pull out the methods below as you need them.  Otherwise, leave them commented out.

    
    public static List<HL7DefSegment> Refresh(long patNum){

        string command="SELECT * FROM hl7defsegment WHERE PatNum = "+POut.Long(patNum);
        return Crud.HL7DefSegmentCrud.SelectMany(command);
    }

    ///<summary>Gets one HL7DefSegment from the db.</summary>
    public static HL7DefSegment GetOne(long hL7DefSegmentNum){

        return Crud.HL7DefSegmentCrud.SelectOne(hL7DefSegmentNum);
    }

    */
}